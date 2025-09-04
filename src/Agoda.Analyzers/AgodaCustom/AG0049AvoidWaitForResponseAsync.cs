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
    public class AG0049AvoidWaitForResponseAsync : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0049";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0049Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0049MessageFormat),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0049Description),
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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocationExpression, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            // Check if this is a method call on a member access expression
            if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess))
                return;

            // Check if the method name is WaitForResponseAsync
            if (memberAccess.Name.Identifier.ValueText != "WaitForResponseAsync")
                return;

            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
            if (!(symbolInfo.Symbol is IMethodSymbol methodSymbol))
                return;

            // Check if this is a Playwright method
            if (!IsPlaywrightWaitForResponseMethod(methodSymbol))
                return;

            // Report the diagnostic
            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor, 
                invocation.GetLocation(), 
                properties: _props.ToImmutableDictionary()));
        }

        private static bool IsPlaywrightWaitForResponseMethod(IMethodSymbol methodSymbol)
        {
            var containingType = methodSymbol.ContainingType.ToString();
            
            // Check if it's called on IPage or Page
            return containingType == "Microsoft.Playwright.IPage" || 
                   containingType == "Microsoft.Playwright.Page";
        }

        private static Dictionary<string, string> _props = new Dictionary<string, string>()
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "15" }
        };
    }
}
