using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Agoda.Analyzers.AgodaCustom
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AG0047RedundantScrollBeforeClickCodeFixProvider)), Shared]
    public class AG0047RedundantScrollBeforeClickCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Remove redundant ScrollIntoViewIfNeededAsync call";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AG0049RedundantScrollBeforeClickAnalyzer.DIAGNOSTIC_ID);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var node = root.FindNode(diagnosticSpan);
            var exprStmt = node as ExpressionStatementSyntax;
            if (exprStmt == null) return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => RemoveRedundantScrollAsync(context.Document, exprStmt, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private async Task<Document> RemoveRedundantScrollAsync(Document document, ExpressionStatementSyntax exprStmt, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.RemoveNode(exprStmt, SyntaxRemoveOptions.KeepLeadingTrivia);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
