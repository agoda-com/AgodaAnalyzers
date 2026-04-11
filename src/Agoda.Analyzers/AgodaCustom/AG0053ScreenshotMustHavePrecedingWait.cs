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
    public class AG0053ScreenshotMustHavePrecedingWait : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0053";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0053Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0053MessageFormat),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0053Description),
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

        private static readonly HashSet<string> ScreenshotMethods = new HashSet<string>
        {
            "ScreenshotAsync",
            "ToHaveScreenshotAsync",
            "ToMatchSnapshotAsync"
        };

        private static readonly HashSet<string> ScreenshotContainingTypes = new HashSet<string>
        {
            "Microsoft.Playwright.IPage",
            "Microsoft.Playwright.ILocator",
            "Microsoft.Playwright.IPageAssertions",
            "Microsoft.Playwright.ILocatorAssertions"
        };

        private static readonly HashSet<string> WaitMethodNames = new HashSet<string>
        {
            "ToBeVisibleAsync",
            "WaitForAsync",
            "WaitForSelectorAsync"
        };

        private static readonly Dictionary<string, string> _props = new Dictionary<string, string>
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "10" }
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess))
                return;

            var methodName = memberAccess.Name.Identifier.ValueText;
            if (!ScreenshotMethods.Contains(methodName))
                return;

            if (!IsPlaywrightScreenshotCall(invocation, context))
                return;

            var containingMethod = invocation.Ancestors()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault();

            if (containingMethod == null)
                return;

            if (HasPrecedingWaitCall(containingMethod, invocation))
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                invocation.GetLocation(),
                properties: _props.ToImmutableDictionary()));
        }

        private static bool IsPlaywrightScreenshotCall(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context)
        {
            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
            if (!(symbolInfo.Symbol is IMethodSymbol methodSymbol))
                return false;

            var containingType = methodSymbol.ContainingType?.ToString();
            return containingType != null && ScreenshotContainingTypes.Contains(containingType);
        }

        private static bool HasPrecedingWaitCall(MethodDeclarationSyntax method, InvocationExpressionSyntax screenshotCall)
        {
            var screenshotPosition = screenshotCall.SpanStart;

            foreach (var block in screenshotCall.Ancestors().OfType<BlockSyntax>())
            {
                foreach (var statement in block.Statements)
                {
                    if (statement.SpanStart >= screenshotPosition)
                        break;

                    foreach (var inv in statement.DescendantNodes().OfType<InvocationExpressionSyntax>())
                    {
                        if (inv.Expression is MemberAccessExpressionSyntax ma &&
                            WaitMethodNames.Contains(ma.Name.Identifier.ValueText))
                        {
                            return true;
                        }
                    }
                }

                if (block.Parent is MethodDeclarationSyntax)
                    break;
            }

            return false;
        }
    }
}
