using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Agoda.Analyzers.Helpers;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0027EnsureOnlyDataSeleniumIsUsedToFindElements : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0027";
        
        private static readonly Regex MatchDataSelenium = new Regex(@"^""\[data-selenium=.*?[^\\]\]""");

        private static readonly LocalizableResourceString Msg = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0027Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DIAGNOSTIC_ID,
            Msg,
            Msg,
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Warning,
            AnalyzerConstants.EnabledByDefault,
            DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0027EnsureOnlyDataSeleniumIsUsedToFindElements)),
            "https://github.agodadev.io/pages/standards-c-sharp/code-standards/gui-testing/data-selenium.html",
            WellKnownDiagnosticTags.EditAndContinue);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocationExpressionSyntax = (InvocationExpressionSyntax) context.Node;

            if (!(context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol is IMethodSymbol))
            {
                return;
            }

            if (!TestMethodHelpers.PermittedSeleniumAccessors.Any(accessor => accessor.IsMatch(context)))
            {
                return;
            }
            
            var firstArgument = invocationExpressionSyntax.ArgumentList.Arguments.FirstOrDefault();
            if (firstArgument?.Expression is LiteralExpressionSyntax && !MatchDataSelenium.IsMatch(firstArgument.ToString()))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, firstArgument.GetLocation()));
            }



        }
    }
}
