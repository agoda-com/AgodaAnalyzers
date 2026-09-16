using System;
using System.Collections.Generic;
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
using Microsoft.CodeAnalysis.Text;

namespace Agoda.Analyzers.AgodaCustom
{
    /// <summary>
    /// Code fix for SonarAnalyzer's S6678 ("Use PascalCase for named placeholders").
    ///
    /// This ships as a fixer only - there is deliberately no matching AG rule, so nothing new appears as a
    /// build warning. Renaming a placeholder renames the structured property Serilog emits, which breaks any
    /// dashboard, saved search or alert keyed on the old name, so the fix is meant to be applied deliberately
    /// per repo via `dotnet format analyzers --diagnostics S6678`. See doc/S6678.md.
    ///
    /// NOTE: unlike every other fixer in this repo, the fixable diagnostic ID is owned by another assembly
    /// (SonarAnalyzer.CSharp). Roslyn matches code fix providers to diagnostics by ID regardless of which
    /// assembly reported them, so this fixer activates in any project that also references SonarAnalyzer.CSharp,
    /// and is simply inert everywhere else.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(S6678PascalCasePlaceholderCodeFixProvider)), Shared]
    public class S6678PascalCasePlaceholderCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Use PascalCase for named placeholders";

        /// <summary>
        /// Owned by SonarAnalyzer.CSharp, not by this assembly. See the remarks on the class.
        /// </summary>
        public const string SonarDiagnosticId = "S6678";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(SonarDiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return;
            }

            foreach (var diagnostic in context.Diagnostics)
            {
                var literal = FindStringLiteral(root, diagnostic.Location.SourceSpan);
                if (literal == null)
                {
                    // The diagnostic did not resolve to a single string literal we understand. Register no fix
                    // rather than guess, and rather than throw.
                    continue;
                }

                var token = literal.Token;
                var changes = GetPlaceholderChanges(token.Text, token.SpanStart, diagnostic.Location.SourceSpan);
                if (changes.Count == 0)
                {
                    continue;
                }

                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: Title,
                        createChangedDocument: c => ApplyChangesAsync(context.Document, changes, c),
                        equivalenceKey: Title),
                    diagnostic);
            }
        }

        private static async Task<Document> ApplyChangesAsync(Document document, IEnumerable<TextChange> changes, CancellationToken cancellationToken)
        {
            // A purely textual edit: it only ever flips a single letter to upper case inside the literal, so the
            // rest of the literal, the other arguments of the call, and all trivia survive byte for byte. This
            // also means verbatim (@"...") and raw string literals need no special casing.
            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            return document.WithText(text.WithChanges(changes));
        }

        /// <summary>
        /// Locates the string literal the diagnostic refers to. Sonar may report either on the whole message
        /// template literal or on an individual placeholder inside it, so this handles both and returns null
        /// when it cannot decide.
        /// </summary>
        private static LiteralExpressionSyntax FindStringLiteral(SyntaxNode root, TextSpan span)
        {
            if (span.Start < root.FullSpan.Start || span.End > root.FullSpan.End)
            {
                return null;
            }

            try
            {
                // Covers both "points at the whole literal" and "points somewhere inside the literal".
                var token = root.FindToken(span.Start);
                for (var node = token.Parent; node != null; node = node.Parent)
                {
                    var candidate = node as LiteralExpressionSyntax;
                    if (candidate != null && candidate.IsKind(SyntaxKind.StringLiteralExpression))
                    {
                        return candidate;
                    }

                    // Don't walk out past the statement the diagnostic is in.
                    if (node is StatementSyntax || node is MemberDeclarationSyntax)
                    {
                        break;
                    }
                }

                // The diagnostic may point at a wider node (the argument, or the whole invocation). Accept it
                // only when exactly one string literal sits underneath, otherwise we'd be guessing.
                var enclosing = root.FindNode(span, getInnermostNodeForTie: true);
                if (enclosing == null)
                {
                    return null;
                }

                var literals = enclosing.DescendantNodesAndSelf()
                    .OfType<LiteralExpressionSyntax>()
                    .Where(l => l.IsKind(SyntaxKind.StringLiteralExpression))
                    .Take(2)
                    .ToList();

                return literals.Count == 1 ? literals[0] : null;
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

        /// <summary>
        /// Scans the raw source text of a string literal token for Serilog message template placeholders and
        /// returns the single character replacements needed to PascalCase the named ones.
        /// </summary>
        /// <param name="text">The raw token text, including quotes and any escaping.</param>
        /// <param name="offset">Absolute position of <paramref name="text"/> in the document.</param>
        /// <param name="diagnosticSpan">
        /// The reported span. When it covers the whole token every placeholder is rewritten; when it points
        /// inside the token only the placeholders it touches are, so that several diagnostics on the same
        /// literal produce non-overlapping fixes for Fix All.
        /// </param>
        private static List<TextChange> GetPlaceholderChanges(string text, int offset, TextSpan diagnosticSpan)
        {
            var changes = new List<TextChange>();
            if (string.IsNullOrEmpty(text))
            {
                return changes;
            }

            var rewriteAll = diagnosticSpan.Contains(new TextSpan(offset, text.Length));

            var i = 0;
            while (i < text.Length)
            {
                var c = text[i];

                if (c == '}')
                {
                    // "}}" is an escaped literal brace, not the end of a placeholder.
                    i += (i + 1 < text.Length && text[i + 1] == '}') ? 2 : 1;
                    continue;
                }

                if (c != '{')
                {
                    i++;
                    continue;
                }

                // "{{" is an escaped literal brace - never a placeholder.
                if (i + 1 < text.Length && text[i + 1] == '{')
                {
                    i += 2;
                    continue;
                }

                var close = text.IndexOf('}', i + 1);
                if (close < 0)
                {
                    break;
                }

                var nameStart = i + 1;

                // Destructuring ("@") and stringification ("$") prefixes are part of the placeholder, not the name.
                if (nameStart < close && (text[nameStart] == '@' || text[nameStart] == '$'))
                {
                    nameStart++;
                }

                var nameEnd = nameStart;
                while (nameEnd < close && (char.IsLetterOrDigit(text[nameEnd]) || text[nameEnd] == '_'))
                {
                    nameEnd++;
                }

                // Anything after the name must be an alignment (",-10") or format (":F2") clause, both of which
                // we leave untouched. Anything else means we don't understand this placeholder, so skip it.
                var suffixIsUnderstood = nameEnd == close || text[nameEnd] == ',' || text[nameEnd] == ':';

                if (nameEnd > nameStart && suffixIsUnderstood)
                {
                    var first = text[nameStart];
                    var upper = char.ToUpperInvariant(first);

                    // Leave alone: positional placeholders ("{0}", "{1,5:F2}") which are indexes rather than
                    // names, names starting with "_" or a digit, and names that are already PascalCase.
                    if (char.IsLower(first) && upper != first)
                    {
                        var placeholderSpan = new TextSpan(offset + i, close - i + 1);
                        if (rewriteAll || diagnosticSpan.IntersectsWith(placeholderSpan))
                        {
                            changes.Add(new TextChange(new TextSpan(offset + nameStart, 1), upper.ToString()));
                        }
                    }
                }

                i = close + 1;
            }

            return changes;
        }
    }
}
