using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace Agoda.Analyzers.CodeFixes.AgodaCustom
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AG0026FixProvider))]
    [Shared]
    public class AG0026FixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AG0026EnsureXPathNotUsedToFindElements.DIAGNOSTIC_ID);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        CustomRulesResources.AG0026FixTitle,
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                        nameof(AG0026FixProvider)),
                    diagnostic);
            }

            return SpecializedTasks.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);

            return await ConvertToUseCssSelectorAsync(document, token, cancellationToken).ConfigureAwait(false);
        }

        private async static Task<Document> ConvertToUseCssSelectorAsync(Document document, SyntaxToken token, CancellationToken cancellationToken)
        {          
            var spanToRemove = TextSpan.FromBounds(token.Span.Start, token.Span.End);
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var change = new TextChange(spanToRemove, "By.CssSelector");
            return document.WithText(sourceText.WithChanges(change));
        }
    }
}