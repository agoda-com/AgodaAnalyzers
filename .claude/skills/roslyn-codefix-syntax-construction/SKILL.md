---
name: roslyn-codefix-syntax-construction
description: Use when a Roslyn code fix builds or edits syntax, with SyntaxFactory, ReplaceNode, DocumentEditor, WithArgumentList, SeparatedList, WithTriviaFrom, WithoutTrivia, SourceText.WithChanges, or Formatter/Simplifier annotations. Covers operator precedence when appending a method call to an expression, keeping trivia and separators on untouched nodes, removing one child instead of its container, text edits inside string literals, and brace escaping in interpolated strings. Trigger on CodeFixProvider, SyntaxFactory, MemberAccessExpression, ParenthesizedExpression, ArgumentList, AttributeList, GetSeparator, ValueText, or a bug where a fix doesn't compile, flattens a multi-line call, or deletes a comment.
---

# Building replacement syntax

A code fix should change the node the diagnostic points at, and nothing else. Everything next to it
(arguments, attributes, comments, line breaks) comes through exactly as it was. The new syntax also has
to parse the way you meant it to.

## 1. Parenthesise the receiver before you add `.Method()`

`x.Method()` binds to the whole of `x` only when `x` is a primary expression. Anything else binds to its
last operand. Sometimes that's a compile error, sometimes it compiles with a different meaning.

```csharp
// Don't: append to the receiver as-is
await GetAsync()   →   await GetAsync().ShouldBe(1);   // CS1929: ShouldBe binds to the Task
a?.B               →   a?.B.ShouldBe(1);                // compiles; skips the assert when a is null
a ?? b             →   a ?? b.ShouldBe(1);              // binds to b; an error only because ShouldBe is void

// Do: wrap anything that isn't a primary expression
await GetAsync()   →   (await GetAsync()).ShouldBe(1);
a?.B               →   (a?.B).ShouldBe(1);
```

Do this **once**, as a last step on the finished node, not in each conversion branch. A branch added
later will miss it (Shouldly.FromAssert #14 was the report, #22 the fix). The
allow-list of shapes that don't need parentheses is in
[references/precedence.md](references/precedence.md).

## 2. Keep trivia and separators on anything you don't rewrite

Calling `WithoutTrivia()` on nodes you're copying, and rebuilding the list with fresh `, ` separators,
flattens multi-line calls and deletes comments between arguments. On a single fix nobody notices. On a
Fix All over a repo, it rewrites every call site.

```csharp
// Don't
var args = original.Arguments.Select(a => a.WithoutTrivia());
return list.WithArguments(SeparatedList(args));             // `, ` everywhere, comments gone

// Do: reuse the original nodes and their separators; only new arguments get new separators
nodes.Add(original.Arguments[i]);
nodes.Add(original.Arguments.GetSeparator(i));
...
nodes.Add(newTemplate.WithTriviaFrom(original.Arguments[templateIndex]));
return list.WithArguments(SeparatedList<ArgumentSyntax>(nodes));
```

AgodaAnalyzers #246. The full recipe, including where the new separators go, is in
[references/trivia-and-separators.md](references/trivia-and-separators.md).

## 3. Remove the child, not its container

`[A, B, C]` is one `AttributeListSyntax` with three attributes. To remove `B`, remove `B`. Remove the
list only when it ends up empty. The same goes for argument lists, parameter lists, modifiers and
`using` directives.

```csharp
// Don't: drops A and C as well
editor.RemoveNode(attributeList);

// Do
editor.RemoveNode(attributeList.Attributes.Count == 1 ? (SyntaxNode)attributeList : attribute);
```

AgodaAnalyzers #86. For a `SyntaxTokenList`, use `modifiers.Remove(token)`, not a new list.

## 4. Change text inside a literal with a text edit

If the fix only changes characters inside a string literal (renaming a placeholder, fixing a format
specifier), apply a `TextChange` to the `SourceText`. Rebuilding the literal with `SyntaxFactory.Literal`
re-escapes it and turns verbatim (`@"..."`) and raw (`"""..."""`) strings into regular ones.

```csharp
var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
return document.WithText(text.WithChanges(new TextChange(new TextSpan(start, 1), "N")));
```

AgodaAnalyzers #246 (the S6678 fixer).

## 5. Know what `ValueText` has already done

`token.ValueText` removes quote and backslash escapes. In an interpolated string's text part it does
**not** undouble braces, because a single brace can't appear there. Escape braces again and `{{x}}`
becomes `{{{{x}}}}`. A regular string literal's `ValueText` can hold single braces, so those do need
escaping if the output is a template.

| Source | `ValueText` of the text part | Escape braces for a template? |
|---|---|---|
| `$"a {{b}} {c}"` (text part) | `a {{b}} ` | No, already doubled |
| `"a {b}"` (literal in a concatenation) | `a {b}` | Yes |

AgodaAnalyzers #246.

## 6. Let the workspace do the formatting

- Add `Formatter.Annotation` and `Simplifier.Annotation` to new nodes rather than building whitespace by
  hand. Build whitespace yourself only when you copy it from the original, as in rule 2.
- For several edits caused by one diagnostic (add a `using`, rewrite a call, remove a statement), use
  `DocumentEditor`. After the first `ReplaceNode`, the other nodes you found belong to the old tree, so a
  second `ReplaceNode` with them can't find its target.
- Pass the `CancellationToken` into every `Get*Async` call and `ConfigureAwait(false)` on each await.

## Tests this needs

- A receiver of each non-primary kind the fixer can see (`await`, `?.`, `??`, binary, cast, conditional).
- A multi-line call with a comment between two arguments, fixed with Fix All, with the comment and line
  breaks still there.
- The target among siblings (`[A, Target, C]`, `f(a, target, c)`), with the siblings still there.
- Each literal kind the fix can touch: regular, verbatim, raw, interpolated, and one with braces in it.

Related: `roslyn-codefix-semantic-preservation` (what the new code should do),
`roslyn-codefix-fix-all-and-parity` (Fix All and `dotnet format`).
