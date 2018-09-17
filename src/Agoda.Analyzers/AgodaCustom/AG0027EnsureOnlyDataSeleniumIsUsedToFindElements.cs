using System;
using Microsoft.CodeAnalysis;
using Agoda.Analyzers.Helpers;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0027EnsureOnlyDataSeleniumIsUsedToFindElements : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AG0027";
        private static readonly Regex MatchDataSelenium = new Regex("^[\\]?[\"]?\\[data-selenium=*");

        private static readonly LocalizableResourceString Msg = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0027Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly Action<SyntaxNodeAnalysisContext> AnalyzeSeleniumGetElement = HandleDependencyResolverUsage;

        protected static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DiagnosticId,
            Msg,
            Msg,
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Warning,
            AnalyzerConstants.EnabledByDefault,
            DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0027EnsureOnlyDataSeleniumIsUsedToFindElements)),
            "https://github.agodadev.io/pages/standards-c-sharp/code-standards/gui-testing/data-selenium.html",
            WellKnownDiagnosticTags.EditAndContinue);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        protected static ImmutableArray<ForbiddenInvocationRule> Rules =>
            ImmutableArray.Create(
                ForbiddenInvocationRule.Create(
                    "OpenQA.Selenium.By",
                    new Regex("^(CssSelector).*$")),
                ForbiddenInvocationRule.Create(
                    "OpenQA.Selenium.Remote.RemoteWebDriver",
                    new Regex("^(FindElement[s]?ByCssSelector)$"))
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeSeleniumGetElement, SyntaxKind.InvocationExpression);
        }

        private static void HandleDependencyResolverUsage(SyntaxNodeAnalysisContext context)
        {
            var invocationExpressionSyntax = (InvocationExpressionSyntax)context.Node;

            if (!(context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol is IMethodSymbol methodSymbol)) return;

            if (Rules
                .Where(rule => methodSymbol.ContainingType.ConstructedFrom.ToString() == rule.NamespaceAndType)
                .Any(rule => !rule.ForbiddenIdentifierNameRegex.IsMatch(methodSymbol.Name))) return;

            foreach (var argument in invocationExpressionSyntax.ArgumentList.Arguments)
            {
                if (MatchDataSelenium.IsMatch(argument.ToString())) return;

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation()));
            }
        }
    }
}
