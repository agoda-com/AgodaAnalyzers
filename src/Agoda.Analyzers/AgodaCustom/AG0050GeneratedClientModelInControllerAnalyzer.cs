using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0050GeneratedClientModelInControllerAnalyzer : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0050";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0050Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0050MessageFormat),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description
            = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0050GeneratedClientModelInControllerAnalyzer));

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DIAGNOSTIC_ID,
            Title,
            MessageFormat,
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Warning,
            AnalyzerConstants.EnabledByDefault,
            Description,
            $"https://github.com/agoda-com/AgodaAnalyzers/blob/master/doc/{DIAGNOSTIC_ID}.md",
            WellKnownDiagnosticTags.EditAndContinue);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is MethodDeclarationSyntax methodDeclaration)) return;

            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration) as IMethodSymbol;
            if (methodSymbol == null) return;

            // Check if this is a controller action method
            if (!IsControllerActionMethod(methodSymbol)) return;

            var violations = new List<GeneratedModelViolation>();

            // Analyze return type
            var returnType = GetActualReturnType(methodSymbol.ReturnType);
            if (returnType != null)
            {
                var returnViolations = FindGeneratedClientModelViolations(returnType, new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default));
                foreach (var violation in returnViolations)
                {
                    violations.Add(new GeneratedModelViolation
                    {
                        ActionName = methodSymbol.Name,
                        ModelType = violation.ModelType,
                        AssemblyName = violation.AssemblyName,
                        Location = methodDeclaration.ReturnType.GetLocation(),
                        PropertyPath = violation.PropertyPath
                    });
                }
            }

            // Analyze parameters
            foreach (var parameter in methodSymbol.Parameters)
            {
                var paramViolations = FindGeneratedClientModelViolations(parameter.Type, new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default));
                foreach (var violation in paramViolations)
                {
                    var parameterSyntax = methodDeclaration.ParameterList.Parameters
                        .FirstOrDefault(p => context.SemanticModel.GetDeclaredSymbol(p)?.Name == parameter.Name);

                    violations.Add(new GeneratedModelViolation
                    {
                        ActionName = methodSymbol.Name,
                        ModelType = violation.ModelType,
                        AssemblyName = violation.AssemblyName,
                        Location = parameterSyntax?.GetLocation() ?? methodDeclaration.ParameterList.GetLocation(),
                        PropertyPath = violation.PropertyPath
                    });
                }
            }

            // Report violations
            foreach (var violation in violations)
            {
                var diagnostic = Diagnostic.Create(
                    Descriptor,
                    violation.Location,
                    properties: _props.ToImmutableDictionary(),
                    violation.ActionName,
                    violation.ModelType,
                    violation.AssemblyName);

                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsControllerActionMethod(IMethodSymbol methodSymbol)
        {
            // Check if the containing class inherits from Controller or ControllerBase
            var containingType = methodSymbol.ContainingType;
            if (!InheritsFromController(containingType)) return false;

            // Must be public
            if (methodSymbol.DeclaredAccessibility != Accessibility.Public) return false;

            // Check for NonAction attribute
            if (methodSymbol.GetAttributes().Any(attr => attr.AttributeClass?.Name == "NonActionAttribute")) return false;

            // Exclude special methods (constructors, properties, events, operators)
            if (methodSymbol.MethodKind != MethodKind.Ordinary) return false;

            return true;
        }

        private static bool InheritsFromController(INamedTypeSymbol typeSymbol)
        {
            var currentType = typeSymbol;
            while (currentType != null)
            {
                if (currentType.Name == "Controller" || currentType.Name == "ControllerBase")
                {
                    return currentType.ContainingNamespace?.ToDisplayString() == "Microsoft.AspNetCore.Mvc";
                }
                currentType = currentType.BaseType;
            }
            return false;
        }

        private static ITypeSymbol GetActualReturnType(ITypeSymbol returnType)
        {
            if (returnType == null) return null;

            // Handle Task<T> and Task<ActionResult<T>>
            if (returnType is INamedTypeSymbol namedType)
            {
                // Check for Task<T>
                if (namedType.Name == "Task" && namedType.TypeArguments.Length == 1)
                {
                    return GetActualReturnType(namedType.TypeArguments[0]);
                }

                // Check for ActionResult<T>
                if (namedType.Name == "ActionResult" && namedType.TypeArguments.Length == 1)
                {
                    return namedType.TypeArguments[0];
                }
            }

            return returnType;
        }

        private static List<GeneratedModelViolation> FindGeneratedClientModelViolations(
            ITypeSymbol type,
            HashSet<ITypeSymbol> visited,
            string propertyPath = "")
        {
            var violations = new List<GeneratedModelViolation>();

            if (type == null || visited.Contains(type)) return violations;
            visited.Add(type);

            // Check if current type is a generated client model
            if (IsGeneratedClientModel(type))
            {
                violations.Add(new GeneratedModelViolation
                {
                    ModelType = type.Name,
                    AssemblyName = GetAssemblyName(type),
                    PropertyPath = propertyPath
                });
                return violations; // Don't traverse deeper if the type itself is a violation
            }

            // Handle generic types (List<T>, IEnumerable<T>, etc.)
            if (type is INamedTypeSymbol namedType)
            {
                foreach (var typeArg in namedType.TypeArguments)
                {
                    violations.AddRange(FindGeneratedClientModelViolations(typeArg, visited, propertyPath));
                }
            }

            // Handle array types
            if (type is IArrayTypeSymbol arrayType)
            {
                violations.AddRange(FindGeneratedClientModelViolations(arrayType.ElementType, visited, propertyPath));
            }

            // Traverse properties for custom types (avoid system types)
            if (type.TypeKind == TypeKind.Class || type.TypeKind == TypeKind.Struct)
            {
                var namespaceName = type.ContainingNamespace?.ToDisplayString() ?? "";
                if (!namespaceName.StartsWith("System") && !namespaceName.StartsWith("Microsoft"))
                {
                    foreach (var member in type.GetMembers().OfType<IPropertySymbol>())
                    {
                        if (member.DeclaredAccessibility == Accessibility.Public)
                        {
                            var newPath = string.IsNullOrEmpty(propertyPath) 
                                ? member.Name 
                                : $"{propertyPath}.{member.Name}";
                            violations.AddRange(FindGeneratedClientModelViolations(member.Type, visited, newPath));
                        }
                    }
                }
            }

            return violations;
        }

        private static bool IsGeneratedClientModel(ITypeSymbol type)
        {
            if (type?.ContainingAssembly == null) return false;

            var assemblyName = GetAssemblyName(type);
            if (!string.IsNullOrEmpty(assemblyName))
            {
                // Check assembly name patterns (case-insensitive)
                if (assemblyName.ToLowerInvariant().EndsWith(".client") ||
                    assemblyName.ToLowerInvariant().EndsWith(".graphql"))
                {
                    return true;
                }
            }

            // Fallback to namespace-based detection for test scenarios
            var namespaceName = type?.ContainingNamespace?.ToDisplayString();
            if (!string.IsNullOrEmpty(namespaceName))
            {
                var namespaceParts = namespaceName.Split('.');
                if (namespaceParts.Length > 0)
                {
                    var lastPart = namespaceParts[namespaceParts.Length - 1].ToLowerInvariant();
                    return lastPart == "client" || lastPart == "graphql";
                }
            }

            return false;
        }

        private static string GetAssemblyName(ITypeSymbol type)
        {
            try
            {
                var assemblyName = type?.ContainingAssembly?.Name;
                if (!string.IsNullOrEmpty(assemblyName))
                {
                    return assemblyName;
                }
            }
            catch
            {
                // Ignore assembly access errors
            }

            // Fallback to namespace for test scenarios
            return type?.ContainingNamespace?.ToDisplayString() ?? "";
        }

        private static readonly Dictionary<string, string> _props = new Dictionary<string, string>()
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "15" }
        };

        private class GeneratedModelViolation
        {
            public string ActionName { get; set; }
            public string ModelType { get; set; }
            public string AssemblyName { get; set; }
            public Location Location { get; set; }
            public string PropertyPath { get; set; }
        }
    }
} 