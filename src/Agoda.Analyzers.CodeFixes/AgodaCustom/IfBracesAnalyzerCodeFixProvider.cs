using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Agoda.Analyzers.CodeFixes.AgodaCustom
{
    [ExportCodeFixProvider("IfBracesAnalyzerCodeFixProvider", LanguageNames.CSharp), Shared]
    public class IfBracesAnalyzerCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("IfBracesAnalyzer");

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;


        private async Task<Document> AddBracesAsync(Document document,
            IfStatementSyntax ifStatement, CancellationToken cancellationToken)
        {
            var nonBlockStatement = ifStatement.Statement as ExpressionStatementSyntax;

            var newBlockStatement = SyntaxFactory.Block(nonBlockStatement)
                .WithAdditionalAnnotations(Formatter.Annotation);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(nonBlockStatement, newBlockStatement);

            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }

        public async override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root
                .FindToken(diagnosticSpan.Start).Parent
                .AncestorsAndSelf()
                .OfType<IfStatementSyntax>()
                .First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create("Add braces",
                c => AddBracesAsync(context.Document, declaration, c)),
                diagnostic);
        }
    }
}
