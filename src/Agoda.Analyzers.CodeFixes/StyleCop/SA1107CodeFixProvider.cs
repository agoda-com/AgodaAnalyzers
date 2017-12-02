using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using Agoda.Analyzers.StyleCop;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Agoda.Analyzers.CodeFixes.StyleCop
{
    /// <summary>
    /// Implements a code fix for <see cref="SA1107CodeMustNotContainMultipleStatementsOnOneLine"/>.
    /// </summary>
    /// <remarks>
    /// <para>To fix a violation of this rule, add or remove a space after the keyword, according to the description
    /// above.</para>
    /// </remarks>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SA1107CodeFixProvider))]
    [Shared]
    public class SA1107CodeFixProvider : CodeFixProvider
    {
        private static readonly SA1107FixAllProvider FixAllProvider = new SA1107FixAllProvider();

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(SA1107CodeMustNotContainMultipleStatementsOnOneLine.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            return FixAllProvider;
        }

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                var node = root?.FindNode(diagnostic.Location.SourceSpan, findInsideTrivia: true, getInnermostNodeForTie: true);

                if (node?.Parent as BlockSyntax != null)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            ReadabilityResources.SA1107CodeFix,
                            cancellationToken => GetTransformedDocumentAsync(context.Document, root, node),
                            nameof(SA1107CodeFixProvider)),
                        diagnostic);
                }
            }
        }

        private static Task<Document> GetTransformedDocumentAsync(Document document, SyntaxNode root, SyntaxNode node)
        {
            var newSyntaxRoot = root;
            Debug.Assert(!node.HasLeadingTrivia, "The trivia should be trailing trivia of the previous node");

            var newNode = node.WithLeadingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);
            newSyntaxRoot = newSyntaxRoot.ReplaceNode(node, newNode);

            return Task.FromResult(document.WithSyntaxRoot(newSyntaxRoot));
        }
    }
}