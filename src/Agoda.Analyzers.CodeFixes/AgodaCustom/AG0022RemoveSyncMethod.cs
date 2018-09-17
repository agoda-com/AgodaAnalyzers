using System;
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

namespace Agoda.Analyzers.CodeFixes.AgodaCustom
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AG0022RemoveSyncMethod)), Shared]
    public class AG0022RemoveSyncMethod : CodeFixProvider
    {
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
              CodeAction.Create(
                  CustomRulesResources.AG0022FixTitle,
                  cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                  nameof(AG0022RemoveSyncMethod)),
              diagnostic);
        }

        private async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root;
            if (root.FindNode(diagnostic.Location.SourceSpan) is MethodDeclarationSyntax node && !node.Identifier.ValueText.EndsWith("Async"))
            {               
                newRoot = newRoot.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);                                 
            
                var updatedDocument =  document.WithSyntaxRoot(newRoot);                
                                          
                return updatedDocument;
            }
            
            return document;            
        }

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods.DiagnosticId);
    }
}