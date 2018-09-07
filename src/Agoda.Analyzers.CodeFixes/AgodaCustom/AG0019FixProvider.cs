using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agoda.Analyzers.CodeFixes.AgodaCustom
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AG0019FixProvider))]
    public class AG0019FixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AG0019PreventUseOfInternalsVisibleToAttribute.DiagnosticId);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        CustomRulesResources.AG0019FixTitle,
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                        nameof(AG0019FixProvider)),
                    diagnostic);
            }

            return SpecializedTasks.CompletedTask;
        }
        
        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);

            return await ConvertToEmptyEnumerableAsync(document, token, cancellationToken).ConfigureAwait(false);
        }

        private async static Task<Document> ConvertToEmptyEnumerableAsync(Document document, SyntaxToken token, CancellationToken cancellationToken)
        {
            return null;
        }
    }
}
