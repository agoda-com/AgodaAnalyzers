using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Helpers;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Agoda.Analyzers.CodeFixes.AgodaCustom
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AG0020FixProvider))]
    [Shared]
    public class AG0020FixProvider : CodeFixProvider
    {
        // this isn't very nice, we need to move IDs to a shared const class
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("AG0020");

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        CustomRulesResources.AG0020FixTitle,
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                        nameof(AG0020FixProvider)),
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
            var returnStatement = token.Parent as ReturnStatementSyntax;

            var method = returnStatement.Ancestors().OfType<MethodDeclarationSyntax>().First();
            var returnType = method.ReturnType as GenericNameSyntax;
            var defaultExp = SyntaxFactory.DefaultExpression(returnType);
            var newText = defaultExp.GetText().ToString();

            TextSpan spanToRemove = TextSpan.FromBounds(token.GetNextToken().Span.Start, token.GetNextToken().Span.End);
            
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var change = new TextChange(spanToRemove, newText);
            return document.WithText(sourceText.WithChanges(change));
        }
    }
}
