using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0054DetectSplitBindingStepDefinitions : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0054";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0054Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0054MessageFormat),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0054Description),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

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

        private static readonly ImmutableDictionary<string, string> Properties =
            new Dictionary<string, string>
            {
                { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "15" }
            }.ToImmutableDictionary();

        private static readonly string[] BindingAttributeNames =
        {
            "TechTalk.SpecFlow.BindingAttribute",
            "Reqnroll.BindingAttribute"
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var namedType = (INamedTypeSymbol)context.Symbol;

            if (namedType.TypeKind != TypeKind.Class)
                return;

            if (!HasBindingAttribute(namedType))
                return;

            var syntaxReferences = namedType.DeclaringSyntaxReferences;
            if (syntaxReferences.Length <= 1)
                return;

            var distinctTrees = syntaxReferences
                .Select(r => r.SyntaxTree)
                .Distinct()
                .Count();

            if (distinctTrees <= 1)
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                namedType.Locations[0],
                Properties,
                namedType.Name,
                distinctTrees));
        }

        private static bool HasBindingAttribute(INamedTypeSymbol symbol)
        {
            return symbol.GetAttributes().Any(attr =>
            {
                var fullName = attr.AttributeClass?.ToDisplayString();
                if (fullName == null)
                    return false;

                return BindingAttributeNames.Any(name => fullName == name);
            });
        }
    }
}
