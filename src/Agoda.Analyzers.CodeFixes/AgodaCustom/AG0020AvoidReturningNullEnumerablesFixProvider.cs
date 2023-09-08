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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Agoda.Analyzers.CodeFixes.AgodaCustom
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AG0020AvoidReturningNullEnumerablesFixProvider))]
    [Shared]
    public class AG0020AvoidReturningNullEnumerablesFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AG0020AvoidReturningNullEnumerables.DIAGNOSTIC_ID);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        CustomRulesResources.AG0020FixTitle,
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                        nameof(AG0020AvoidReturningNullEnumerablesFixProvider)),
                    diagnostic);
            }

            return Task.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);

            return await ConvertToEmptyEnumerableAsync(document, token, cancellationToken).ConfigureAwait(false);
        }

        private async static Task<Document> ConvertToEmptyEnumerableAsync(Document document, SyntaxToken token, CancellationToken cancellationToken)
        {
            var statement = token.Parent;

            GenericNameSyntax returnType = null;

            MemberDeclarationSyntax method = statement.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (method != null)
            {
                returnType = (method as MethodDeclarationSyntax).ReturnType as GenericNameSyntax;
            }
            else
            {
                var p = statement.Ancestors().OfType<PropertyDeclarationSyntax>().First();
                returnType = p.ChildNodes().First() as GenericNameSyntax;
            }

            string newText;

            if (returnType == null)
            {
                var arrayReturnType = (method as MethodDeclarationSyntax).ReturnType as ArrayTypeSyntax;

                newText = $"Array.Empty<{arrayReturnType.ElementType}>()";
            }
            else if (returnType.Identifier.Text == "IEnumerable")
            {
                newText = $"Enumerable.Empty{returnType.TypeArgumentList}()";
            }
            else
            {
                var enumerableType = SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(returnType.Identifier.Text)) as TypeSyntax;
                var listSyntax = SyntaxFactory.GenericName(enumerableType.GetFirstToken(), returnType.TypeArgumentList);
                var objConstructor = SyntaxFactory.ObjectCreationExpression(listSyntax, SyntaxFactory.ArgumentList(), null);

                newText = objConstructor.NormalizeWhitespace().ToString();
            }

            var spanToRemove = TextSpan.FromBounds(token.Span.Start, token.Span.End);
            
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var change = new TextChange(spanToRemove, newText);
            return document.WithText(sourceText.WithChanges(change));
        }
    }
}
