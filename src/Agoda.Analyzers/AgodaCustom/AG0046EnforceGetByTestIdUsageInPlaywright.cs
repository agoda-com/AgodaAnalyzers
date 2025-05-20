using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using Agoda.Analyzers.Helpers;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0046EnforceGetByTestIdUsageInPlaywright : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0046";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0046Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0046MessageFormat),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0046Description),
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

        private static readonly HashSet<string> AllowedMethods = new HashSet<string>
        {
            "GetByTestId"
        };

        private static readonly ImmutableDictionary<string, string> _props = new Dictionary<string, string>()
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "15" }
        }.ToImmutableDictionary();

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocationExpression = (InvocationExpressionSyntax)context.Node;

            if (!(invocationExpression.Expression is MemberAccessExpressionSyntax memberAccess))
                return;

            var semanticModel = context.SemanticModel;
            var methodSymbol = semanticModel.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;
            
            if (methodSymbol == null || !IsPlaywrightLocatorMethod(methodSymbol))
                return;

            var methodName = methodSymbol.Name;
            if (methodName == "Locator" || (methodName.StartsWith("GetBy") && !AllowedMethods.Contains(methodName)))
            {
                // Check if this is a GetByRole call for accessibility testing
                if (methodName == "GetByRole" && IsAccessibilityTesting(invocationExpression))
                    return;

                var diagnostic = Diagnostic.Create(
                    Descriptor,
                    memberAccess.Name.GetLocation(),
                    _props,
                    methodName);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsAccessibilityTesting(InvocationExpressionSyntax invocationExpression)
        {
            // Get the statement containing this invocation
            var statement = invocationExpression.Ancestors().OfType<StatementSyntax>().FirstOrDefault();
            if (statement == null)
                return false;

            // Get all trivia (comments) before this statement
            var trivia = statement.GetLeadingTrivia();
            return trivia.Any(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia) && 
                                 t.ToString().ToLowerInvariant().Contains("accessibility"));
        }

        private static bool IsPlaywrightLocatorMethod(IMethodSymbol methodSymbol)
        {
            var containingType = methodSymbol.ContainingType.ToString();
            return containingType == "Microsoft.Playwright.IPage" || 
                   containingType == "Microsoft.Playwright.Page" ||
                   containingType == "Microsoft.Playwright.ILocator" ||
                   containingType == "Microsoft.Playwright.Locator" ||
                   containingType == "Microsoft.Playwright.IFrameLocator" ||
                   containingType == "Microsoft.Playwright.FrameLocator";
        }
    }
} 