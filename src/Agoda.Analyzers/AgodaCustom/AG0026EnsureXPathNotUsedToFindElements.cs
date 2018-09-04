using Microsoft.CodeAnalysis;
using Agoda.Analyzers.Helpers;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0026EnsureXPathNotUsedToFindElements : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0026";
        private readonly DiagnosticDescriptor _descriptor;
        
        public AG0026EnsureXPathNotUsedToFindElements()
        {
            var msg = new LocalizableResourceString(
                nameof(CustomRulesResources.AG0026Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));

            _descriptor = new DiagnosticDescriptor(
                DIAGNOSTIC_ID,
                msg,
                msg,
                AnalyzerCategory.CustomQualityRules,
                DiagnosticSeverity.Warning,
                AnalyzerConstants.EnabledByDefault,
                DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0026EnsureXPathNotUsedToFindElements)),
                "https://github.agodadev.io/pages/standards-c-sharp/code-standards/gui-testing/css-selectors.html",
                WellKnownDiagnosticTags.EditAndContinue
            );
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_descriptor);

        public override void Initialize(AnalysisContext context) => context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocationExpressionSyntax = (InvocationExpressionSyntax)context.Node;
            var model = context.SemanticModel;
            var symbol = model.GetSymbolInfo(invocationExpressionSyntax).Symbol;
            if (symbol != null && symbol.ToDisplayString() == "OpenQA.Selenium.By.XPath(string)")
            {
                context.ReportDiagnostic(Diagnostic.Create(_descriptor, invocationExpressionSyntax.GetLocation()));
            }
        }
    }
}
