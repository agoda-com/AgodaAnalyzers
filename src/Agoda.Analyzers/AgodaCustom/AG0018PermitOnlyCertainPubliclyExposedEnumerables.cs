// <copyright file="AG0018EnsureThatPubliclyExposedIEnumerableTypes.cs" company="Agoda Company Co., Ltd.">
// AGODA ® is a registered trademark of AGIP LLC, used under license by Agoda Company Co., Ltd.. Agoda is part of Priceline (NASDAQ:PCLN)
// </copyright>
using System.Collections.Immutable;
using System.Linq;
using System.Collections.Generic;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0018PermitOnlyCertainPubliclyExposedEnumerables : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0018";

        private static readonly HashSet<string> AllowedTypes = new HashSet<string>
        {
            "System.Collections.Generic.ISet<T>",
            "System.Collections.Generic.IList<T>",
            "System.Collections.Generic.IDictionary<TKey, TValue>",
            "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>",
            "System.Collections.Generic.KeyedCollection<TKey, TValue>",
            "System.Collections.Generic.IEnumerable<T>",
            "System.Collections.Generic.IReadOnlyCollection<T>",
            "System.Collections.Generic.IReadOnlyList<T>"
        };

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0018Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0018Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly LocalizableString Description 
            = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0018PermitOnlyCertainPubliclyExposedEnumerables));

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DIAGNOSTIC_ID, 
            Title, 
            MessageFormat, 
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Warning, 
            AnalyzerConstants.EnabledByDefault, 
            Description, 
            "https://agoda-com.github.io/standards-c-sharp/collections/choosing-collection-implementation.html", 
            WellKnownDiagnosticTags.EditAndContinue);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.PropertyDeclaration, SyntaxKind.MethodDeclaration);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            if (context.ContainingSymbol.DeclaredAccessibility != Accessibility.Public || IsPubliclyExposedIEnumerableTypes(context.ContainingSymbol)) { return; }
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation(), properties: _props.ToImmutableDictionary()));
        }

        private static bool IsPubliclyExposedIEnumerableTypes(ISymbol symbol)
        {
            INamedTypeSymbol namedTypeSymbol = null;
            
            if (symbol.Kind == SymbolKind.Method)
            {
                IMethodSymbol method = (IMethodSymbol)symbol;
                if(method.ReturnType is IArrayTypeSymbol)
                {
                    if ((method.ReturnType as IArrayTypeSymbol).ElementType.Name != "Byte")
                    {
                        return false;
                    }
                }
                namedTypeSymbol = method.ReturnType as INamedTypeSymbol;
                
            }
            else if (symbol.Kind == SymbolKind.Property)
            {
                IPropertySymbol property = (IPropertySymbol)symbol;
                if (property.Type is IArrayTypeSymbol)
                {
                    if ((property.Type as IArrayTypeSymbol).ElementType.Name != "Byte")
                    {
                        return false;
                    }
                }
                namedTypeSymbol = property.Type as INamedTypeSymbol;
            }


            if (namedTypeSymbol != null
                     && namedTypeSymbol.ContainingNamespace.ToDisplayString().StartsWith("System.Collections")
                     && namedTypeSymbol.ConstructedFrom.Interfaces.Any(x => x.ToDisplayString() == "System.Collections.IEnumerable"))
            {
                var fullName = namedTypeSymbol.OriginalDefinition.ToDisplayString();
                if (AllowedTypes.Contains(fullName))
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        private static Dictionary<string, string> _props = new Dictionary<string, string>()
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "10" }
        };
    }
}