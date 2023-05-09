﻿using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Agoda.Analyzers.CodeFixes.Helpers;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DocumentBasedFixAllProvider = Agoda.Analyzers.CodeFixes.Helpers.DocumentBasedFixAllProvider;

namespace Agoda.Analyzers.CodeFixes.StyleCop
{
    internal sealed class RemoveRegionFixAllProvider : DocumentBasedFixAllProvider
    {
        protected override string CodeActionTitle => "Remove region";

        protected override async Task<SyntaxNode> FixAllInDocumentAsync(FixAllContext fixAllContext, Document document, ImmutableArray<Diagnostic> diagnostics)
        {
            if (diagnostics.IsEmpty)
            {
                return null;
            }

            var root = await document.GetSyntaxRootAsync().ConfigureAwait(false);

            var nodesToRemove = diagnostics.Select(d => root.FindNode(d.Location.SourceSpan, findInsideTrivia: true))
                .Where(node => node != null && !node.IsMissing)
                .OfType<RegionDirectiveTriviaSyntax>()
                .SelectMany(node => node.GetRelatedDirectives())
                .Where(node => !node.IsMissing);

            return root.RemoveNodes(nodesToRemove, SyntaxRemoveOptions.AddElasticMarker);
        }
    }
}