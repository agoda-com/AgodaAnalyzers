using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0040TooManyElseIfConditions : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AG0040TooManyElseIfConditions";
        private const string Category = "Syntax";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "Convert if-else if to switch expression",
            "Consider converting this if-else if chain to a switch expression",
            Category,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Consider using a switch expression to simplify the code.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

            // Register the syntax node action for if statements
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;

            // Check if it's part of an if-else if chain
            if (!IsPartOfIfElseChain(ifStatement))
            {
                return;
            }

            // Report a diagnostic
            var diagnostic = Diagnostic.Create(Rule, ifStatement.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }

        private bool IsPartOfIfElseChain(IfStatementSyntax ifStatement)
        {
            // Check if the if statement has an else clause with an if statement as its statement
            if (ifStatement.Else?.Statement is IfStatementSyntax elseIf)
            {
                // Recursively check the else-if part
                return IsPartOfIfElseChain(elseIf);
            }

            return true;
        }
    }
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AG0040TooManyElseIfConditionsFix))]

    public class AG0040TooManyElseIfConditionsFix : CodeFixProvider
    {
        public const string DIAGNOSTIC_ID = "AG0040";
        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0040Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0040Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description
            = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0040TooManyElseIfConditionsFix));

        private const string Category = "Documentation";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DIAGNOSTIC_ID,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning, // THis rule should be opt in and not on by default
            isEnabledByDefault: true,
            helpLinkUri: "");
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AG0040TooManyElseIfConditions.DiagnosticId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the if-else if statements within the diagnostic span
            var ifStatements = root.DescendantNodes(diagnosticSpan)
                .OfType<IfStatementSyntax>()
                .ToList();

            foreach (var ifStatement in ifStatements)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Convert to switch statement",
                        cancellationToken => ConvertToSwitchAsync(context.Document, ifStatement, cancellationToken),
                        nameof(AG0040TooManyElseIfConditionsFix)),
                    diagnostic);
            }
        }

        private async Task<Document> ConvertToSwitchAsync(
         Document document, IfStatementSyntax ifStatement, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            // Get the root of the syntax tree
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            // Extract the conditions and associated statements from the if-else if chain
            var conditionsAndStatements = new List<(ExpressionSyntax Condition, StatementSyntax Statement)>();
            var currentIf = ifStatement;

            while (currentIf != null)
            {
                conditionsAndStatements.Add((currentIf.Condition, currentIf.Statement));
                if (currentIf.Else?.Statement is IfStatementSyntax elseIf)
                {
                    currentIf = elseIf;
                }
                else
                {
                    currentIf = null;
                }
            }

            // Create a switch expression based on the extracted conditions
            var switchExpression = SyntaxFactory.SwitchExpression(
                SyntaxFactory.ParenthesizedExpression(
                    conditionsAndStatements
                        .Select(cs => SyntaxFactory.BinaryExpression(SyntaxKind.LogicalOrExpression, ifStatement.Condition, cs.Condition))
                        .Aggregate((agg, next) => SyntaxFactory.BinaryExpression(SyntaxKind.LogicalOrExpression, agg, next))
                )
            );

            // Create switch arms for each condition
            var switchArms = conditionsAndStatements.Select(cs =>
                SyntaxFactory.SwitchExpressionArm(
                    SyntaxFactory.SeparatedList<PatternSyntax>(
                        new[]
                        {
                        SyntaxFactory.ConstantPattern(cs.Condition),
                        SyntaxFactory.VarPattern(SyntaxFactory.Token(SyntaxKind.UnderscoreToken))
                        }
                    ),
                    SyntaxFactory.Block(cs.Statement)));

            // Create the switch expression
            var switchExpressionSyntax = SyntaxFactory.SwitchExpression(
                switchExpression,
                SyntaxFactory.Token(SyntaxKind.SwitchKeyword),
                SyntaxFactory.Token(SyntaxKind.None),
                SyntaxFactory.SeparatedList(switchArms),
                SyntaxFactory.Token(SyntaxKind.None));
            // Replace the if-else if chain with the switch expression
            root = root.ReplaceNode(ifStatement, switchExpressionSyntax);

            // Apply the changes to the document
            var newDocument = document.WithSyntaxRoot(root);

            return newDocument;
        }

    }
}