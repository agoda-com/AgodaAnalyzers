using Microsoft.CodeAnalysis;
using Agoda.Analyzers.Helpers;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0026EnsureOnlyCssSelectorIsUsedToFindElements : InvocationExpressionAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0026";

        private static readonly LocalizableResourceString _msg = new LocalizableResourceString(
                nameof(CustomRulesResources.AG0026Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
              
        protected override bool IsStatic => true;
        protected override string NamespaceAndType => "OpenQA.Selenium.By";
        protected override ImmutableArray<string> Methods => ImmutableArray.Create("CssSelector");
        protected override InvocationExpressionMode Mode => InvocationExpressionMode.Allowed;
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
    }
    
    public abstract class InvocationExpressionAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);
        protected abstract DiagnosticDescriptor Descriptor { get; }
        protected abstract bool IsStatic { get; }
        protected abstract string NamespaceAndType { get; }
        protected abstract ImmutableArray<string> Methods { get; }
        protected abstract InvocationExpressionMode Mode { get; }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            bool mode(bool x) => Mode == InvocationExpressionMode.Forbidden ? x : !x;

            var invocationExpressionSyntax = (InvocationExpressionSyntax)context.Node;
            var symbol = context.SemanticModel?.GetSymbolInfo(invocationExpressionSyntax).Symbol;
            if (symbol != null
                && (!IsStatic || symbol.IsStatic)
                && symbol.ContainingType.ConstructedFrom.ToString() == NamespaceAndType
                && mode(Methods.Contains(symbol.Name)))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocationExpressionSyntax.GetLocation()));
            }
        }
    }

    public enum InvocationExpressionMode
    {
        Allowed = 1,
        Forbidden = 2
    }
}
