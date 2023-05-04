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

        private static async Task<Document> ReplaceBlockingCallWithAsync(Document document, 
            SyntaxNode root,
            SyntaxNode node, 
            CancellationToken cancellationToken)
        {
            var memberAccess = (MemberAccessExpressionSyntax)node.Parent;

            // Get the original method symbol
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var symbol = semanticModel.GetSymbolInfo(memberAccess).Symbol as IMethodSymbol;

            // Create the new async method call
            var asyncMethodCall = SyntaxFactory.ParseExpression($"await {symbol.ToDisplayString()}");

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
}