using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Agoda.Analyzers.Helpers;
using Agoda.Analyzers.StyleCop;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Agoda.Analyzers.CodeFixes.StyleCop
{
    /// <summary>
    /// Implements a code fix for <see cref="SA1123DoNotPlaceRegionsWithinElements"/> and <see cref="SA1124DoNotUseRegions"/>.
    /// </summary>
    /// <remarks>
    /// <para>To fix a violation of this rule, remove the region.</para>
    /// </remarks>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveRegionCodeFixProvider))]
    [Shared]
    public class RemoveRegionCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(SA1123DoNotPlaceRegionsWithinElements.DIAGNOSTIC_ID);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            // The batch fixer does not do a very good job if regions are stacked in each other
            return new RemoveRegionFixAllProvider();
        }

        /// <inheritdoc/>
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        ReadabilityResources.RemoveRegionCodeFix,
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic),
                        nameof(RemoveRegionCodeFixProvider)),
                    diagnostic);
            }

            return SpecializedTasks.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic)
        {
            var syntaxRoot = await document.GetSyntaxRootAsync().ConfigureAwait(false);
            var node = syntaxRoot?.FindNode(diagnostic.Location.SourceSpan, findInsideTrivia: true, getInnermostNodeForTie: true);
            if (node != null && node.IsKind(SyntaxKind.RegionDirectiveTrivia))
            {
                var regionDirective = node as RegionDirectiveTriviaSyntax;

                var newSyntaxRoot = syntaxRoot.RemoveNodes(regionDirective.GetRelatedDirectives(), SyntaxRemoveOptions.AddElasticMarker);

                return document.WithSyntaxRoot(newSyntaxRoot);
            }

            return document.WithSyntaxRoot(syntaxRoot);
        }
    }
}