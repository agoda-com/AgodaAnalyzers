---
name: dotnet-code-style
description: Use when writing or modifying C# method bodies, type declarations, or non-trivial logic in a .NET library. Covers taste-and-judgment conventions — exception construction, simplification, naming intent, `StringComparison`, non-null check form, and `var` usage — that aren't already caught at build time.
---

# .NET code style

Most formatting, naming, and basic analyzer rules are enforced by the build (`.editorconfig`, StyleCop, IDE analyzers, `CSharpIsNullAnalyzer`). This skill covers the style choices the build can't make for you.

## Exceptions include the offending value

Argument-validation exceptions must pass the actual parameter value, not just its name. This turns "ArgumentOutOfRangeException: value" into a useful diagnostic.

```csharp
// Avoid — the log line says "Actual value was X." but X is generic
if (depth < 1)
    throw new ArgumentOutOfRangeException(nameof(depth), "Depth must be at least 1");

// Prefer
if (depth < 1)
    throw new ArgumentOutOfRangeException(nameof(depth), depth, "Depth must be at least 1");
```

Same pattern for `ArgumentException` when the value is informative. Don't hide inputs inside a generic message.

## Simplify — inline, early-return, no redundant state

Reduce statements and variables that exist only to be referenced once.

```csharp
// Avoid
bool allMatched = true;
foreach (var arg in args)
{
    if (!Matches(arg))
    {
        allMatched = false;
        break;
    }
}
return allMatched;

// Prefer
foreach (var arg in args)
{
    if (!Matches(arg))
        return false;
}
return true;
```

Drop `else` after `return`/`throw`. Don't allocate a list you're going to immediately replace. Compute a value next to where you use it, not several lines earlier.

Use early returns in loops. Avoid LINQ if this is allocation-sensitive code.

## `is not null` for non-null checks

Prefer the pattern-matching form for readability.

```csharp
if (other.Extension is not null) { ... }
```

Not `is object`, not `is {}`, and not `!= null` (the analyzer will flag that anyway).

## `StringComparison.Ordinal` for technical strings

File paths, TFM names, assembly names, identifiers, anything technical — pass `StringComparison.Ordinal` explicitly. Implicit culture-aware comparison is treated as a bug here.

```csharp
// Avoid — culture-sensitive by default
if (targetFramework.StartsWith("net4"))

// Prefer
if (targetFramework.StartsWith("net4", StringComparison.Ordinal))
```

Use `StringComparison.OrdinalIgnoreCase` if case-insensitivity is genuinely wanted; never rely on the current culture for technical strings.

## Naming intent — descriptive over terse

Names describe what the code *does*, not what it *is*. Prefer a longer, self-documenting name over a short cryptic one, and prefer the verb that matches the actual behaviour.

- `IgnoreLineEndingFormat` is better than `NormalizeLineEndings` — the method doesn't normalise, it configures the comparison to ignore differences.
- `CopyOutputTo` rather than `CopyOutputFrom` when the method copies outward.
- `HasCompilerGeneratedEquals` rather than `IsCompilerGenerated` when the predicate is specifically about the `Equals` method.

If you find yourself wanting to write a comment to explain what a method does, try renaming the method first.

## `var` only when the type is on the same line

`var` is allowed when the type is obvious from the right-hand side:

```csharp
var list = new List<string>();            // type is literally right there
var items = CreateItems();                // type is not obvious — prefer explicit
```

For the second form, write the explicit type: `List<Item> items = CreateItems();`.

`var` is also permitted for `out var` parameters and pattern matching.

## One statement per line, one thing per statement

Don't chain side-effectful calls across a single line to save vertical space. Split multi-step expressions into named intermediates when it aids the reader. Keep test assertions obvious — one logical check per assertion, or use the test framework's group/multi-assert facility for related checks.

## Code comments — explain *why*, not *what*

Comments should add information the code can't communicate itself. Restating the syntax is noise.

```csharp
// Avoid — the comment is what the code already says
// check if x is a MemberAccessExpressionSyntax
if (x is MemberAccessExpressionSyntax) { ... }

// Avoid — the comment is what the method name already says
// check the method name
if (methodName != "QuerySelectorAsync") { ... }

// Prefer — comment captures the non-obvious *why*
// Skip generated client projects — they're allowed to use raw GraphQL types here
if (filePath.EndsWith(".g.cs", StringComparison.Ordinal)) return;
```

Strip before committing:

- **Restatement comments** that just verbalise the next line.
- **AI-generated boilerplate** — `// As an AI assistant…`, emoji-headed sections, "I have implemented…" narration.
- **Stale TODOs** — point to a tracking issue/ID, or delete.

When referencing an external API, link the official docs in the doc comment or rule documentation so the reader lands on authoritative material.

## Prefer positive predicates

Double negation is hard to read. Prefer inverting the check with a guard clause, or applying De Morgan's law, rather than leaving negations stacked.

```csharp
// Avoid — two negations gate the happy path
if (!string.IsNullOrEmpty(name) && !IsReserved(name))
{
    Process(name);
}

// Prefer — guard clause inverts the check, happy path is unindented
if (string.IsNullOrEmpty(name) || IsReserved(name))
    return;

Process(name);
```

**Do not invent a wrapper method to avoid negation.** Only extract a named predicate (`IsValidName`, `IsReady`) when (a) the method already exists, or (b) the concept is genuinely reusable and you are actually adding the method. Manufacturing a call to a method that doesn't exist in the codebase just to satisfy this rule is worse than the original double-negation.
