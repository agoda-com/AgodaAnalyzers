---
name: code-fix-preservation
description: Use when authoring or modifying a Roslyn `CodeFixProvider`. Prevents the most severe class of analyzer bug — a code fix that silently removes unrelated user code while "fixing" the diagnostic.
---

# Code-fix preservation

The cost of a wrong code fix is asymmetric: a missed fix is annoying, an incorrect fix that *removes* user code is a corruption. This has happened in this repo when a code fix replaced an entire attribute list to remove one offending attribute, taking the surrounding `[AssemblyDescription("...")]`, `[AssemblyVersion(...)]`, and similar siblings with it.

The rule is simple: **a code fix must alter only the exact syntax node identified by the diagnostic.** Siblings, parents, and trivia must survive verbatim.

## Common patterns where preservation fails

### Attribute lists

`[A, B, C]` is one `AttributeListSyntax` with three `AttributeSyntax` children. Removing attribute `B` by replacing the entire `AttributeListSyntax` removes `A` and `C` too. Operate on the child collection:

```csharp
// Wrong — replaces the whole list, drops siblings
var newAttributeList = oldAttributeList.WithAttributes(SyntaxFactory.SeparatedList<AttributeSyntax>());
editor.ReplaceNode(oldAttributeList, newAttributeList);

// Right — remove just the offending attribute, leave siblings intact
var remaining = oldAttributeList.Attributes.Where(a => a != offendingAttribute);
var newList = oldAttributeList.WithAttributes(SyntaxFactory.SeparatedList(remaining));
editor.ReplaceNode(oldAttributeList, newList);
```

If removing the last attribute would leave an empty `[]`, remove the entire list node instead. Don't ship empty attribute lists.

### Parameter lists, argument lists, member lists

Same pattern as attributes — the container is a single node holding a separated list of children. Edit the children, not the container.

### Modifier lists

`public static readonly int X = 1;` — the modifiers are a `SyntaxTokenList`. Removing `readonly` shouldn't touch `public static`. Use `WithModifiers(originalModifiers.Remove(readonlyToken))`, not a new token list built from scratch.

### Trivia (whitespace and comments)

Leading and trailing trivia (the whitespace, newlines, and comments around a node) belong to the node. When you replace a node, propagate its original trivia:

```csharp
var newNode = MakeReplacement(oldNode)
    .WithLeadingTrivia(oldNode.GetLeadingTrivia())
    .WithTrailingTrivia(oldNode.GetTrailingTrivia());
```

Otherwise the user's blank lines, XML doc comments, or `// ` comments above the line vanish silently.

## Required test: target is one of several siblings

For every code fix, write a test where the targeted syntax is **embedded among siblings**, and assert the siblings survive verbatim:

```csharp
[Test]
public async Task CodeFix_PreservesSiblingAttributes()
{
    const string before = """
        [AssemblyDescription("Description")]
        [DeprecatedAttribute]
        [AssemblyVersion("1.0.0")]
        public class Foo {}
        """;

    const string after = """
        [AssemblyDescription("Description")]
        [AssemblyVersion("1.0.0")]
        public class Foo {}
        """;

    await VerifyCSharpFixAsync(before, after);
}
```

Same shape for parameters, arguments, modifiers, members. A code fix without this test is not finished.

## Use `DocumentEditor` when fixes touch multiple nodes

If a single diagnostic produces multiple syntax edits (e.g. remove an attribute *and* add a `using` directive), use `DocumentEditor` rather than chained `ReplaceNode` calls. Multiple replaces against the same root produce stale spans; `DocumentEditor` handles span tracking correctly.

## Re-test the round-trip

After applying the fix, parse the result and assert the diagnostic no longer fires. A fix that "compiles" but still triggers the analyzer means the fix didn't actually address the diagnostic — the test infrastructure (`VerifyCSharpFixAsync`) does this round-trip automatically; don't skip it by manually constructing expected strings.

## Before-merge checklist

- [ ] Fix operates on the precise offending node, not its parent container.
- [ ] Siblings, modifiers, and trivia are preserved.
- [ ] Empty containers (empty attribute lists, empty parameter lists) are removed entirely, not left as `[]` or `()`.
- [ ] A test case exists where the target sits among other siblings, asserting siblings survive verbatim.
- [ ] The fix has been verified to remove the diagnostic, not just compile.
