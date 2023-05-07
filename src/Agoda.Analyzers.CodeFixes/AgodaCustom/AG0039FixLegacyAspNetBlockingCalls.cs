using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.Extensions.Options;
using Document = Microsoft.CodeAnalysis.Document;

namespace Agoda.Analyzers.CodeFixes.AgodaCustom
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AG0039FixLegacyAspNetBlockingCalls)), Shared]
    public class AG0039FixLegacyAspNetBlockingCalls : CodeFixProvider
    {
        private const string title = "Convert blocking call to async";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AG0039DetectLegacyAspNetBlockingCalls.MyRule.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Find the node to fix
            foreach (var diagnostic in context.Diagnostics)
            {
                var diagnostics = ImmutableArray.Create(diagnostic ?? context.Diagnostics[0]);

                // Register a code fix for the diagnostic
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedDocument: c => 
                            FixAllWithEditorAsync(context.Document, 
                                editor => FixAllAsync(context.Document, diagnostics, editor, context.CancellationToken)
                                , c),
                        equivalenceKey: title
                ),
                    diagnostics
                );
                //RegisterCodeFix(context, title, nameof(AG0039FixLegacyAspNetBlockingCalls), diagnostic);

            }
        }
        protected Task FixAllAsync(
            Document document,
            ImmutableArray<Diagnostic> diagnostics,
            SyntaxEditor editor,
            CancellationToken cancellationToken)
        {
            foreach (var diagnostic in diagnostics)
            {
                var nullableDirective = diagnostic.Location.FindNode(findInsideTrivia: true, getInnermostNodeForTie: true, cancellationToken);
                editor.RemoveNode(nullableDirective, SyntaxRemoveOptions.KeepNoTrivia);
            }
            return Task.CompletedTask;
        }
        internal static async Task<Document> FixAllWithEditorAsync(
            Document document,
            Func<SyntaxEditor, Task> editAsync,
            CancellationToken cancellationToken)
        {
            try
            {
                var root = await document.GetRequiredSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                var editor = new SyntaxEditor(root, document.Project.Solution.Services);

                await editAsync(editor).ConfigureAwait(false);

                var newRoot = editor.GetChangedRoot();
                return document.WithSyntaxRoot(newRoot);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        // Local function
        static void RemoveStatement(SyntaxEditor editor, SyntaxNode statement)
        {
            if (!statement.IsParentKind(SyntaxKind.Block)
                && !statement.IsParentKind(SyntaxKind.SwitchSection)
                && !statement.IsParentKind(SyntaxKind.GlobalStatement))
            {
                editor.ReplaceNode(statement, SyntaxFactory.Block());
            }
            else
            {
                editor.RemoveNode(statement, SyntaxRemoveOptions.KeepUnbalancedDirectives);
            }
        }
        private static async Task<Document> ReplaceBlockingCallWithAsync(Document document, 
            SyntaxNode root,
            SyntaxNode node, 
            CancellationToken cancellationToken)
        {
            var accessExpressionSyntax = node.FirstAncestorOrSelf<MemberAccessExpressionSyntax>();
            try
            {
                SyntaxNode previousNode = null;
                foreach (var syntaxNode in accessExpressionSyntax.ChildNodes())
                {
                    if (syntaxNode is IdentifierNameSyntax identifierNameSyntax)
                    {
                        if (identifierNameSyntax.Identifier.Text == "Result")
                        {
                            accessExpressionSyntax = accessExpressionSyntax.RemoveMultipleNodes(new[]{ previousNode , syntaxNode});
                        }
                    }
                    previousNode = syntaxNode;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            // Get the original method symbol
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var symbol = semanticModel.GetSymbolInfo(accessExpressionSyntax).Symbol as IMethodSymbol;

            // Create the new async method call
            var asyncMethodCall = SyntaxFactory.ParseExpression($"await {accessExpressionSyntax.GetText()}");

            // Create the new method declaration with async keyword and async return type
            var methodDeclaration = (MethodDeclarationSyntax)node.Parent;
            var asyncReturnType = SyntaxFactory.ParseTypeName($"Task<{methodDeclaration.ReturnType}>");
            var asyncMethodDeclaration = methodDeclaration
                .WithIdentifier(SyntaxFactory.Identifier(methodDeclaration.Identifier.Text))
                .WithAsyncKeyword()
                .WithReturnType(asyncReturnType);

            // Create the new method body with the async method call
            var newBody = SyntaxFactory.Block(
                SyntaxFactory.ReturnStatement(asyncMethodCall)
            );

            // Replace the old method body with the new body
            var newMethod = asyncMethodDeclaration.WithBody(newBody);
            var newRoot = root.ReplaceNode(node.Parent, newMethod);

            // Update document with new syntax
            return document.WithSyntaxRoot(newRoot);
        }
    }

    public static class SyntaxNodeExt
    {

        public static async ValueTask<SyntaxNode> GetRequiredSyntaxRootAsync(this Document document, CancellationToken cancellationToken)
        {
            if (document.TryGetSyntaxRoot(out var root))
                return root;

            root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            return root ?? throw new InvalidOperationException(string.Format("Error {0}", document.Name));
        }
        public static T ReplaceMultipleNodesWithOne<T>(
            this T root, IReadOnlyList<SyntaxNode> oldNodes, SyntaxNode newNode)
            where T : SyntaxNode
        {
            if (oldNodes.Count == 0)
                throw new ArgumentException();

            var newRoot = root.TrackNodes(oldNodes);

            var first = newRoot.GetCurrentNode(oldNodes[0]);

            newRoot = newRoot.ReplaceNode(first, newNode);

            var toRemove = oldNodes.Skip(1).Select(newRoot.GetCurrentNode);

            newRoot = newRoot.RemoveNodes(toRemove, SyntaxRemoveOptions.KeepNoTrivia);
            
            return newRoot;
        }
        public static T RemoveMultipleNodes<T>(
            this T root, IReadOnlyList<SyntaxNode> oldNodes)
            where T : SyntaxNode
        {
            if (oldNodes.Count == 0)
                throw new ArgumentException();

            var newRoot = root.TrackNodes(oldNodes);
            
            var toRemove = oldNodes.Select(newRoot.GetCurrentNode);

            newRoot = newRoot.RemoveNodes(toRemove, SyntaxRemoveOptions.KeepNoTrivia);

            return newRoot;
        }
    }
    internal static class LocationExtensions
    {
        public static SyntaxTree GetSourceTreeOrThrow(this Location location)
        {
            if(location.SourceTree == null) throw new NullReferenceException("SourceTree was null");
            return location.SourceTree;
        }

        public static SyntaxToken FindToken(this Location location, CancellationToken cancellationToken)
            => location.GetSourceTreeOrThrow().GetRoot(cancellationToken).FindToken(location.SourceSpan.Start);

        public static SyntaxNode FindNode(this Location location, CancellationToken cancellationToken)
            => location.GetSourceTreeOrThrow().GetRoot(cancellationToken).FindNode(location.SourceSpan);

        public static SyntaxNode FindNode(this Location location, bool getInnermostNodeForTie, CancellationToken cancellationToken)
            => location.GetSourceTreeOrThrow().GetRoot(cancellationToken).FindNode(location.SourceSpan, getInnermostNodeForTie: getInnermostNodeForTie);

        public static SyntaxNode FindNode(this Location location, bool findInsideTrivia, bool getInnermostNodeForTie, CancellationToken cancellationToken)
            => location.GetSourceTreeOrThrow().GetRoot(cancellationToken).FindNode(location.SourceSpan, findInsideTrivia, getInnermostNodeForTie);
        
        public static bool IntersectsWith(this Location loc1, Location loc2)
        {
            Debug.Assert(loc1.IsInSource && loc2.IsInSource);
            return loc1.SourceTree == loc2.SourceTree && loc1.SourceSpan.IntersectsWith(loc2.SourceSpan);
        }
        public static bool IsParentKind(this SyntaxNode node, SyntaxKind kind)
            => (bool)node?.Parent.IsKind(kind);

        public static bool IsParentKind<TNode>(this SyntaxNode node, SyntaxKind kind, out TNode result)
            where TNode : SyntaxNode
        {
            if (node.IsParentKind(kind))
            {
                result = (TNode)node.Parent;
                return true;
            }

            result = null;
            return false;
        }
    }
}