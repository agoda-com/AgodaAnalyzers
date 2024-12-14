using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Agoda.Analyzers.AgodaCustom
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AG0041CodeFixProvider)), Shared]
    public class AG0041CodeFixProvider : CodeFixProvider
    {
        private const string Title = "Use message template";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AG0041LogTemplateAnalyzer.DIAGNOSTIC_ID);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var invocation = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => ConvertToMessageTemplateAsync(context.Document, invocation, c),
                    equivalenceKey: Title),
                diagnostic);
        }
        private async Task<Document> ConvertToMessageTemplateAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var root = await document.GetSyntaxRootAsync(cancellationToken);

            var newInvocation = ConvertToMessageTemplate(invocation, semanticModel);

            var newRoot = root.ReplaceNode(invocation, newInvocation);
            return document.WithSyntaxRoot(newRoot);
        }

        private InvocationExpressionSyntax ConvertToMessageTemplate(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            var arguments = invocation.ArgumentList.Arguments;
            var firstArgument = arguments.First();

            if (firstArgument.Expression is InterpolatedStringExpressionSyntax interpolatedString)
            {
                var (template, parameters) = ExtractFromInterpolatedString(interpolatedString);
                return CreateNewInvocation(invocation, template, parameters);
            }
            else if (firstArgument.Expression is BinaryExpressionSyntax binaryExpression &&
                     binaryExpression.IsKind(SyntaxKind.AddExpression))
            {
                var (template, parameters) = ExtractFromConcatenation(binaryExpression);
                return CreateNewInvocation(invocation, template, parameters);
            }

            // If we're here, the format is already correct, so we return the original invocation
            return invocation;
        }

        private (string template, SeparatedSyntaxList<ArgumentSyntax> parameters) ExtractFromInterpolatedString(InterpolatedStringExpressionSyntax interpolatedString)
        {
            var template = "";
            var parameters = new SeparatedSyntaxList<ArgumentSyntax>();

            foreach (var content in interpolatedString.Contents)
            {
                if (content is InterpolatedStringTextSyntax text)
                {
                    template += text.TextToken.ValueText;
                }
                else if (content is InterpolationSyntax interpolation)
                {
                    var paramName = interpolation.Expression.ToString();
                    template += $"{{{char.ToUpper(paramName[0]) + paramName.Substring(1)}}}";
                    parameters = parameters.Add(SyntaxFactory.Argument(interpolation.Expression));
                }
            }

            return (template, parameters);
        }

        private (string template, SeparatedSyntaxList<ArgumentSyntax> parameters) ExtractFromConcatenation(BinaryExpressionSyntax expression)
        {
            var template = "";
            var parameters = new SeparatedSyntaxList<ArgumentSyntax>();

            void ExtractPart(ExpressionSyntax expr)
            {
                if (expr is LiteralExpressionSyntax literal)
                {
                    template += literal.Token.ValueText;
                }
                else
                {
                    var paramName = expr.ToString();
                    template += $"{{{char.ToUpper(paramName[0]) + paramName.Substring(1)}}}";
                    parameters = parameters.Add(SyntaxFactory.Argument(expr));
                }
            }

            void TraverseConcatenation(BinaryExpressionSyntax binaryExpr)
            {
                if (binaryExpr.Left is BinaryExpressionSyntax leftBinary && leftBinary.IsKind(SyntaxKind.AddExpression))
                {
                    TraverseConcatenation(leftBinary);
                }
                else
                {
                    ExtractPart(binaryExpr.Left);
                }

                ExtractPart(binaryExpr.Right);
            }

            TraverseConcatenation(expression);

            return (template, parameters);
        }

        private InvocationExpressionSyntax CreateNewInvocation(InvocationExpressionSyntax originalInvocation, string template, SeparatedSyntaxList<ArgumentSyntax> parameters)
        {
            var newArguments = new SeparatedSyntaxList<ArgumentSyntax>()
                .Add(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(template))))
                .AddRange(parameters);

            return originalInvocation.WithArgumentList(SyntaxFactory.ArgumentList(newArguments));
        }
    }
}