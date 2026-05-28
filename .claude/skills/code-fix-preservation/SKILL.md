---
name: code-fix-preservation
description: Use when writing or changing a Roslyn CodeFixProvider. Ensures a fix alters only the node the diagnostic identifies, never removing sibling syntax or trivia.
---

# Code-fix preservation

A wrong fix that *removes* user code is corruption, not an annoyance. Rule: **alter only the exact offending node; siblings, modifiers, and trivia survive verbatim.**

## Edit children, not the container

`[A, B, C]` is one `AttributeListSyntax` with three children. Replacing the list to drop `B` drops `A` and `C` too.

```csharp
// Wrong — drops siblings
editor.ReplaceNode(oldList, oldList.WithAttributes(SyntaxFactory.SeparatedList<AttributeSyntax>()));
// Right — remove only the offender
var remaining = oldList.Attributes.Where(a => a != offending);
editor.ReplaceNode(oldList, oldList.WithAttributes(SyntaxFactory.SeparatedList(remaining)));
```

Same for parameter lists, argument lists, member lists. If removing the last element leaves an empty `[]`/`()`, remove the whole container node instead.

For modifiers (`SyntaxTokenList`), use `WithModifiers(original.Remove(token))`, not a fresh list.

## Preserve trivia

Whitespace/comments belong to the node. Propagate on replace:

```csharp
var newNode = MakeReplacement(old)
    .WithLeadingTrivia(old.GetLeadingTrivia())
    .WithTrailingTrivia(old.GetTrailingTrivia());
```

## Required test: target among siblings

Every fix needs a test where the target sits among siblings, asserting they survive:

```csharp
[Test]
public async Task CodeFix_PreservesSiblings()
{
    const string before = """
        [AssemblyDescription("D")]
        [Deprecated]
        [AssemblyVersion("1.0.0")]
        public class Foo {}
        """;
    const string after = """
        [AssemblyDescription("D")]
        [AssemblyVersion("1.0.0")]
        public class Foo {}
        """;
    await VerifyCSharpFixAsync(before, after);
}
```

## Multi-node fixes

If one diagnostic produces several edits (remove attribute + add `using`), use `DocumentEditor` — chained `ReplaceNode` calls produce stale spans.

## Checklist

- [ ] Operates on the offending node, not its container.
- [ ] Siblings, modifiers, trivia preserved.
- [ ] Empty containers removed entirely, not left as `[]`/`()`.
- [ ] Test with target among siblings, asserting siblings survive.
- [ ] `VerifyCSharpFixAsync` confirms the diagnostic is gone (round-trip), not just that it compiles.
