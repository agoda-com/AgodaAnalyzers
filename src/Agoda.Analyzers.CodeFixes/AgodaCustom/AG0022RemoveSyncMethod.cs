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
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {            
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        CustomRulesResources.AG0022FixTitle,
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                        nameof(AG0022RemoveSyncMethod)),
                    diagnostic);
            }

            return SpecializedTasks.CompletedTask;
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

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods.DIAGNOSTIC_ID);
    }
}