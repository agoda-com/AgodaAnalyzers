using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Agoda.Analyzers.CodeFixes.AgodaCustom
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AG0019FixProvider))]
    public class AG0019FixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AG0019PreventUseOfInternalsVisibleToAttribute.DIAGNOSTIC_ID);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: CustomRulesResources.AG0019FixTitle,
                        createChangedDocument: cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                        equivalenceKey: nameof(AG0019FixProvider)),
                    diagnostic);
            }

            return SpecializedTasks.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);

            return await RemoveInternalsVisibleToAttribute(document, token, cancellationToken).ConfigureAwait(false);
        }

        private async static Task<Document> RemoveInternalsVisibleToAttribute(Document document, SyntaxToken token, CancellationToken cancellationToken)
        {
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var attributeList = token.Parent.Parent.Parent as AttributeListSyntax;

            var tokenToRemove = token.Parent.Parent.Parent;
            var spanToRemove = TextSpan.FromBounds(tokenToRemove.Span.Start, tokenToRemove.Span.End);

            if (attributeList.Attributes.Count == 1)
            {
                return document.WithText(sourceText.WithChanges(new TextChange(spanToRemove, string.Empty)));
            }

            var newAttributeNodes = new List<AttributeSyntax>();
            foreach (AttributeSyntax attribute in attributeList.Attributes)
            {
                if (attribute.Name is IdentifierNameSyntax name && name.Identifier.Text == "InternalsVisibleTo") continue;

                newAttributeNodes.Add(attribute);
            }
            
            var assemblyKeyword = SyntaxFactory.Token(SyntaxKind.AssemblyKeyword);
            var colon = SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.ColonToken, SyntaxFactory.TriviaList(SyntaxFactory.Whitespace(" ")));
            var target = SyntaxFactory.AttributeTargetSpecifier(assemblyKeyword, colon);
            var newAttributes = SyntaxFactory.SeparatedList(newAttributeNodes);
            var newAttributeList = SyntaxFactory.AttributeList(SyntaxFactory.Token(SyntaxKind.OpenBracketToken), target, newAttributes, SyntaxFactory.Token(SyntaxKind.CloseBracketToken));

            var change = new TextChange(spanToRemove, newAttributeList.ToString());
            return document.WithText(sourceText.WithChanges(change));
        }
    }
}
