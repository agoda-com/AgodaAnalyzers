using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Agoda.Analyzers.AgodaCustom
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AG0041CodeFixProvider)), Shared]
    public class AG0041CodeFixProvider : CodeFixProvider
    {
        private const string Title = "Use message template";

        /// <summary>
        /// Sonar's "Don't use string interpolation in logging message templates". Roslyn matches
        /// fixers to diagnostics by ID regardless of which assembly reported them, so listing it
        /// here makes this fix available in repositories that only run SonarAnalyzer.CSharp.
        /// </summary>
        internal const string SONAR_INTERPOLATED_TEMPLATE_DIAGNOSTIC_ID = "S2629";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
            AG0041LogTemplateAnalyzer.DIAGNOSTIC_ID,
            SONAR_INTERPOLATED_TEMPLATE_DIAGNOSTIC_ID);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var token = root.FindToken(diagnosticSpan.Start);
            if (token.Parent == null)
                return;

            var invocation = token.Parent.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
            if (invocation == null)
                return;

            if (FindTemplateArgumentIndex(invocation, diagnosticSpan) < 0)
                return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => ConvertToMessageTemplateAsync(context.Document, invocation, diagnosticSpan, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private async Task<Document> ConvertToMessageTemplateAsync(Document document, InvocationExpressionSyntax invocation, TextSpan diagnosticSpan, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var newInvocation = ConvertToMessageTemplate(invocation, diagnosticSpan);
            if (newInvocation == invocation)
                return document;

            var newRoot = root.ReplaceNode(invocation, newInvocation);
            return document.WithSyntaxRoot(newRoot);
        }

        private InvocationExpressionSyntax ConvertToMessageTemplate(InvocationExpressionSyntax invocation, TextSpan diagnosticSpan)
        {
            var templateIndex = FindTemplateArgumentIndex(invocation, diagnosticSpan);
            if (templateIndex < 0)
                return invocation;

            var templateExpression = invocation.ArgumentList.Arguments[templateIndex].Expression;

            var interpolatedString = templateExpression as InterpolatedStringExpressionSyntax;
            if (interpolatedString != null)
            {
                string template;
                List<ArgumentSyntax> parameters;
                ExtractFromInterpolatedString(interpolatedString, out template, out parameters);
                return CreateNewInvocation(invocation, templateIndex, template, parameters);
            }

            var binaryExpression = templateExpression as BinaryExpressionSyntax;
            if (binaryExpression != null && binaryExpression.IsKind(SyntaxKind.AddExpression))
            {
                string template;
                List<ArgumentSyntax> parameters;
                ExtractFromConcatenation(binaryExpression, out template, out parameters);
                return CreateNewInvocation(invocation, templateIndex, template, parameters);
            }

            // If we're here, the format is already correct, so we return the original invocation
            return invocation;
        }

        /// <summary>
        /// Locates the argument that holds the message template. The diagnostic is normally reported
        /// on that argument (or on the interpolated string inside it), but other analyzers reporting
        /// the same ID may point elsewhere in the invocation, so we fall back to the first argument
        /// that can be converted.
        /// </summary>
        private static int FindTemplateArgumentIndex(InvocationExpressionSyntax invocation, TextSpan diagnosticSpan)
        {
            if (invocation.ArgumentList == null)
                return -1;

            var arguments = invocation.ArgumentList.Arguments;

            for (var i = 0; i < arguments.Count; i++)
            {
                if (IsConvertible(arguments[i].Expression) && arguments[i].Span.IntersectsWith(diagnosticSpan))
                    return i;
            }

            for (var i = 0; i < arguments.Count; i++)
            {
                if (IsConvertible(arguments[i].Expression))
                    return i;
            }

            return -1;
        }

        private static bool IsConvertible(ExpressionSyntax expression)
        {
            if (expression is InterpolatedStringExpressionSyntax)
                return true;

            var binary = expression as BinaryExpressionSyntax;
            return binary != null
                   && binary.IsKind(SyntaxKind.AddExpression)
                   && binary.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>()
                       .Any(bes => bes.IsKind(SyntaxKind.AddExpression) &&
                                   (bes.Left.IsKind(SyntaxKind.StringLiteralExpression) ||
                                    bes.Right.IsKind(SyntaxKind.StringLiteralExpression)));
        }

        private void ExtractFromInterpolatedString(InterpolatedStringExpressionSyntax interpolatedString, out string template, out List<ArgumentSyntax> parameters)
        {
            var builder = new StringBuilder();
            var namer = new PlaceholderNamer();
            var arguments = new List<ArgumentSyntax>();

            foreach (var content in interpolatedString.Contents)
            {
                var text = content as InterpolatedStringTextSyntax;
                if (text != null)
                {
                    // ValueText unescapes quotes and backslash escapes but leaves "{{" / "}}"
                    // doubled - inside an interpolated string a brace can only ever appear doubled,
                    // so the text is already escaped exactly the way the template needs it.
                    // Escaping again here would turn "{{x}}" into "{{{{x}}}}".
                    builder.Append(text.TextToken.ValueText);
                    continue;
                }

                var interpolation = content as InterpolationSyntax;
                if (interpolation == null)
                    continue;

                var expression = interpolation.Expression.WithoutTrivia();
                builder.Append(CreatePlaceholder(namer.NameFor(expression), interpolation.AlignmentClause, interpolation.FormatClause));
                arguments.Add(SyntaxFactory.Argument(expression));
            }

            template = builder.ToString();
            parameters = arguments;
        }

        private void ExtractFromConcatenation(BinaryExpressionSyntax expression, out string template, out List<ArgumentSyntax> parameters)
        {
            var builder = new StringBuilder();
            var namer = new PlaceholderNamer();
            var arguments = new List<ArgumentSyntax>();

            void ExtractPart(ExpressionSyntax expr)
            {
                var literal = expr as LiteralExpressionSyntax;
                if (literal != null && literal.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    // A plain string literal may contain single braces, which Serilog would read as
                    // a placeholder, so they have to be doubled on the way into the template.
                    builder.Append(EscapeBraces(literal.Token.ValueText));
                }
                else
                {
                    var operand = expr.WithoutTrivia();
                    builder.Append(CreatePlaceholder(namer.NameFor(operand), null, null));
                    arguments.Add(SyntaxFactory.Argument(operand));
                }
            }

            void TraverseConcatenation(BinaryExpressionSyntax binaryExpr)
            {
                var leftBinary = binaryExpr.Left as BinaryExpressionSyntax;
                if (leftBinary != null && leftBinary.IsKind(SyntaxKind.AddExpression))
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

            template = builder.ToString();
            parameters = arguments;
        }

        private static string EscapeBraces(string text)
        {
            return text.Replace("{", "{{").Replace("}", "}}");
        }

        /// <summary>
        /// Emits <c>{Name,alignment:format}</c>, carrying over the interpolation's alignment and
        /// format clauses; both are supported verbatim by Serilog and Microsoft.Extensions.Logging.
        /// </summary>
        private static string CreatePlaceholder(string name, InterpolationAlignmentClauseSyntax alignment, InterpolationFormatClauseSyntax format)
        {
            var builder = new StringBuilder();
            builder.Append('{').Append(name);

            if (alignment != null)
            {
                builder.Append(',').Append(alignment.Value.ToString().Trim());
            }

            if (format != null)
            {
                builder.Append(':').Append(format.FormatStringToken.ValueText);
            }

            return builder.Append('}').ToString();
        }

        private static InvocationExpressionSyntax CreateNewInvocation(InvocationExpressionSyntax originalInvocation, int templateIndex, string template, List<ArgumentSyntax> parameters)
        {
            var originalArguments = originalInvocation.ArgumentList.Arguments;
            var newArguments = new List<ArgumentSyntax>();

            // Leading arguments (exception / EventId) must survive the rewrite.
            for (var i = 0; i < templateIndex; i++)
            {
                newArguments.Add(originalArguments[i].WithoutTrivia());
            }

            newArguments.Add(SyntaxFactory.Argument(
                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(template))));
            newArguments.AddRange(parameters);

            // Anything that followed the template was never part of it, so it is preserved too.
            for (var i = templateIndex + 1; i < originalArguments.Count; i++)
            {
                newArguments.Add(originalArguments[i].WithoutTrivia());
            }

            var nodesAndTokens = new List<SyntaxNodeOrToken>();
            for (var i = 0; i < newArguments.Count; i++)
            {
                if (i > 0)
                {
                    nodesAndTokens.Add(SyntaxFactory.Token(SyntaxKind.CommaToken).WithTrailingTrivia(SyntaxFactory.Space));
                }

                nodesAndTokens.Add(newArguments[i]);
            }

            var newArgumentList = originalInvocation.ArgumentList
                .WithArguments(SyntaxFactory.SeparatedList<ArgumentSyntax>(nodesAndTokens));

            return originalInvocation
                .WithArgumentList(newArgumentList)
                .WithTriviaFrom(originalInvocation);
        }

        /// <summary>
        /// Derives legal, unique Serilog property names for the expressions pulled out of a template.
        /// </summary>
        private sealed class PlaceholderNamer
        {
            private readonly HashSet<string> _used = new HashSet<string>(StringComparer.Ordinal);
            private int _fallbackCount;

            public string NameFor(ExpressionSyntax expression)
            {
                var name = Sanitise(DeriveName(expression));

                if (string.IsNullOrEmpty(name))
                {
                    name = NextFallbackName();
                }

                return MakeUnique(name);
            }

            private string NextFallbackName()
            {
                _fallbackCount++;
                return "Arg" + _fallbackCount.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            private string MakeUnique(string name)
            {
                if (_used.Add(name))
                    return name;

                for (var suffix = 2; ; suffix++)
                {
                    var candidate = name + suffix.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    if (_used.Add(candidate))
                        return candidate;
                }
            }

            private static string DeriveName(ExpressionSyntax expression)
            {
                var parenthesized = expression as ParenthesizedExpressionSyntax;
                if (parenthesized != null)
                    return DeriveName(parenthesized.Expression);

                var identifier = expression as IdentifierNameSyntax;
                if (identifier != null)
                    return identifier.Identifier.ValueText;

                var memberAccess = expression as MemberAccessExpressionSyntax;
                if (memberAccess != null)
                    return memberAccess.Name.Identifier.ValueText;

                var memberBinding = expression as MemberBindingExpressionSyntax;
                if (memberBinding != null)
                    return memberBinding.Name.Identifier.ValueText;

                var conditionalAccess = expression as ConditionalAccessExpressionSyntax;
                if (conditionalAccess != null)
                    return DeriveName(conditionalAccess.WhenNotNull) ?? DeriveName(conditionalAccess.Expression);

                var invocation = expression as InvocationExpressionSyntax;
                if (invocation != null)
                    return DeriveNameFromInvocation(invocation);

                return null;
            }

            private static string DeriveNameFromInvocation(InvocationExpressionSyntax invocation)
            {
                var callee = invocation.Expression as IdentifierNameSyntax;
                if (callee != null && callee.Identifier.ValueText == "nameof" &&
                    invocation.ArgumentList != null && invocation.ArgumentList.Arguments.Count == 1)
                {
                    // nameof(X) already *is* the name we want.
                    return DeriveName(invocation.ArgumentList.Arguments[0].Expression);
                }

                // fromDate.ToShortDateString() describes fromDate, so the receiver is the better name.
                var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                if (memberAccess != null)
                {
                    return DeriveName(memberAccess.Expression) ?? memberAccess.Name.Identifier.ValueText;
                }

                return callee?.Identifier.ValueText;
            }

            /// <summary>
            /// Reduces a derived name to <c>[A-Za-z0-9_]+</c> in PascalCase; returns null when
            /// nothing usable is left, which pushes the caller onto the ArgN fallback.
            /// </summary>
            private static string Sanitise(string name)
            {
                if (string.IsNullOrEmpty(name))
                    return null;

                var builder = new StringBuilder(name.Length);
                foreach (var c in name)
                {
                    if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_')
                    {
                        builder.Append(c);
                    }
                }

                if (builder.Length == 0)
                    return null;

                var sanitised = builder.ToString();

                if (sanitised[0] >= '0' && sanitised[0] <= '9')
                {
                    sanitised = "_" + sanitised;
                }

                return char.ToUpperInvariant(sanitised[0]) + sanitised.Substring(1);
            }
        }
    }
}
