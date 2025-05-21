using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Agoda.Analyzers.Helpers;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0048LoggerExceptionParameterAnalyzer : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0048";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0048Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0048MessageFormat),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description =
            DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0048LoggerExceptionParameterAnalyzer));

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
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (methodSymbol == null) return;

            // Check if this is a logger method
            if (!IsLoggerMethod(methodSymbol)) return;

            var arguments = invocation.ArgumentList.Arguments;
            // Only report if an exception is passed NOT as the first parameter
            for (int i = 1; i < arguments.Count; i++)
            {
                var argument = arguments[i];
                var typeInfo = context.SemanticModel.GetTypeInfo(argument.Expression);
                if (typeInfo.Type != null && IsOrInheritsFromException(typeInfo.Type))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, argument.GetLocation(), properties: _props.ToImmutableDictionary()));
                }
            }
        }

        private bool IsLoggerMethod(IMethodSymbol methodSymbol)
        {
            var containingType = methodSymbol.ContainingType;
            var containingNamespace = containingType.ContainingNamespace;
            var namespaceName = containingNamespace.ToDisplayString();
            var methodName = methodSymbol.Name;

            // Microsoft.Extensions.Logging: only extension methods
            if (namespaceName == "Microsoft.Extensions.Logging" && methodSymbol.IsExtensionMethod)
                return true;

            // Serilog: allow instance methods on Serilog.ILogger
            if (namespaceName.StartsWith("Serilog"))
            {
                var serilogMethods = new[] { "Verbose", "Debug", "Information", "Warning", "Error", "Fatal", "Write" };
                // Check if the containing type implements Serilog.ILogger
                if (serilogMethods.Contains(methodName))
                    return true;
            }
            return false;
        }


        private static Dictionary<string, string> _props = new Dictionary<string, string>()
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "10" }
        };

        private static bool IsOrInheritsFromException(ITypeSymbol typeSymbol)
        {
            while (typeSymbol != null)
            {
                if (typeSymbol.ToString() == "System.Exception")
                    return true;
                typeSymbol = typeSymbol.BaseType;
            }
            return false;
        }
    }
} 