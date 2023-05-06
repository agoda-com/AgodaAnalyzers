using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

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
            var diagnostic = context.Diagnostics.First();
            var node = root.FindNode(diagnostic.Location.SourceSpan);

            // Register a code fix for the diagnostic
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => ReplaceBlockingCallWithAsync(context.Document, root, node, c),
                    equivalenceKey: title
                ),
                diagnostic
            );
        }
        public async Task<Document> RemoveResultFromClass3(Document document, SyntaxNode root, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var methodDeclarationNodes = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

            var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

            foreach (var methodDeclarationNode in methodDeclarationNodes)
            {
                var nodes = methodDeclarationNode.DescendantNodes().OfType<MemberAccessExpressionSyntax>();
                var results = new List<MemberAccessExpressionSyntax>();

                foreach (var node in nodes)
                {
                    if (node.Name.ToString() == "Result" &&
                        semanticModel.GetTypeInfo(node.Expression, cancellationToken).Type?.Name == "Task")
                    {
                        results.Add(node);
                    }
                }

                if (!results.Any())
                {
                    continue;
                }

                foreach (var result in results)
                {
                    var awaitExpression = SyntaxFactory.AwaitExpression(result.Expression)
                        .WithTriviaFrom(result);

                    editor.ReplaceNode(result, awaitExpression);
                }
            }

            return editor.GetChangedDocument();
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
}