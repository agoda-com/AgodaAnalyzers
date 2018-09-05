using Microsoft.CodeAnalysis;
using Agoda.Analyzers.Helpers;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0026EnsureOnlyCssSelectorIsUsedToFindElements : InvocationExpressionAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0026";

        private static readonly LocalizableResourceString _msg = new LocalizableResourceString(
                nameof(CustomRulesResources.AG0026Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
              
        protected override DiagnosticDescriptor Descriptor => new DiagnosticDescriptor(
                DIAGNOSTIC_ID,
                _msg,
                _msg,
                AnalyzerCategory.CustomQualityRules,
                DiagnosticSeverity.Warning,
                AnalyzerConstants.EnabledByDefault,
                DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0026EnsureOnlyCssSelectorIsUsedToFindElements)),
                "https://github.agodadev.io/pages/standards-c-sharp/code-standards/gui-testing/css-selectors.html",
                WellKnownDiagnosticTags.EditAndContinue
            );

        protected override string NamespaceAndType => "OpenQA.Selenium.By";
        protected override Regex Regex => new Regex("^((?!CssSelector).)*$");
    }
    
    public abstract class InvocationExpressionAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);
        protected abstract DiagnosticDescriptor Descriptor { get; }
        protected abstract string NamespaceAndType { get; }
        protected abstract Regex Regex { get; }
        
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocationExpressionSyntax = (InvocationExpressionSyntax)context.Node;
            var methodSymbol = (IMethodSymbol)context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol;
            var t = methodSymbol.ContainingType.ConstructedFrom.ToString();
            if (methodSymbol != null
                && methodSymbol.ContainingType.ConstructedFrom.ToString() == NamespaceAndType
                && Regex.IsMatch(methodSymbol.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocationExpressionSyntax.GetLocation()));
            }
        }
    }
}
