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
    public class AG0047RedundantScrollBeforeClickAnalyzer : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0047";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0047Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0047MessageFormat),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0047Description),
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
            $"https://github.com/agoda-com/AgodaAnalyzers/blob/master/doc/{DIAGNOSTIC_ID}.md");

        private static readonly Dictionary<string, string> _props = new Dictionary<string, string>()
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "5" }
        };
        
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ExpressionStatement);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var exprStmt = (ExpressionStatementSyntax)context.Node;
            if (!(exprStmt.Expression is AwaitExpressionSyntax awaitExpr)) return;
            if (!(awaitExpr.Expression is InvocationExpressionSyntax invocation)) return;
            if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess)) return;
            if (memberAccess.Name.Identifier.Text != "ScrollIntoViewIfNeededAsync") return;

            var locatorExpr = memberAccess.Expression.ToString();

            // Find the next statement
            var parentBlock = exprStmt.Parent as BlockSyntax;
            if (parentBlock == null) return;
            var statements = parentBlock.Statements;
            var idx = statements.IndexOf(exprStmt);
            if (idx < 0 || idx + 1 >= statements.Count) return;
            var nextStmt = statements[idx + 1];

            // Check for small delays in between
            int lookahead = 1;
            while (lookahead + idx < statements.Count)
            {
                var candidate = statements[idx + lookahead];
                if (candidate is ExpressionStatementSyntax es &&
                    es.Expression is AwaitExpressionSyntax ae &&
                    ae.Expression is InvocationExpressionSyntax inv &&
                    inv.Expression is MemberAccessExpressionSyntax ma &&
                    ma.Name.Identifier.Text == "ClickAsync" &&
                    ma.Expression.ToString() == locatorExpr)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, exprStmt.GetLocation(), properties: _props.ToImmutableDictionary()));
                    return;
                }
                // Allow Task.Delay or trivial statements
                if (candidate.ToString().Contains("Task.Delay"))
                {
                    lookahead++;
                    continue;
                }
                break;
            }
        }
    }
}
