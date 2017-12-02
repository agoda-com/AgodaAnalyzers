using System.Collections.Immutable;
using System.Threading.Tasks;
using Agoda.Analyzers.Helpers;
using Agoda.Analyzers.StyleCop;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;

namespace Agoda.Analyzers.CodeFixes.StyleCop
{
    public class SA1107FixAllProvider : DocumentBasedFixAllProvider
    {
        protected override string CodeActionTitle => ReadabilityResources.SA1107CodeFix;

        protected override async Task<SyntaxNode> FixAllInDocumentAsync(FixAllContext fixAllContext, Document document, ImmutableArray<Diagnostic> diagnostics)
        {
            if (diagnostics.IsEmpty)
            {
                return null;
            }

            var editor = await DocumentEditor.CreateAsync(document, fixAllContext.CancellationToken).ConfigureAwait(false);

            var root = editor.GetChangedRoot();

            var nodesToChange = ImmutableList.Create<SyntaxNode>();

            // Make sure all nodes we care about are tracked
            foreach (var diagnostic in diagnostics)
            {
                var location = diagnostic.Location;
                var syntaxNode = root.FindNode(location.SourceSpan);
                if (syntaxNode != null)
                {
                    editor.TrackNode(syntaxNode);
                    nodesToChange = nodesToChange.Add(syntaxNode);
                }
            }

            foreach (var node in nodesToChange)
            {
                editor.ReplaceNode(node, node.WithLeadingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed));
            }

            return editor.GetChangedRoot();
        }
    }
}