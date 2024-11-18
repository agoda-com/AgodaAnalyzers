using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Agoda.Analyzers.AgodaCustom
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AG0022RemoveSyncMethodFixProvider)), Shared]
    public class AG0022RemoveSyncMethodFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods.DIAGNOSTIC_ID);
        
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {            
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        CustomRulesResources.AG0022FixTitle,
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                        nameof(AG0022RemoveSyncMethodFixProvider)),
                    diagnostic);
            }

            return Task.CompletedTask;
        }

        private async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            
            if (root.FindNode(diagnostic.Location.SourceSpan) is MethodDeclarationSyntax node)
            {
                var newRoot = root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);                                                
                var updatedDocument =  document.WithSyntaxRoot(newRoot);                                                              
                return updatedDocument;
            }
            
            return document;
        }
    }
}