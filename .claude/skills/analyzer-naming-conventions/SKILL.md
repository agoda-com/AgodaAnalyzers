---
name: analyzer-naming-conventions
description: Use when creating a new analyzer class, code-fix, helper, or constant in this repo. Captures the file-name, type-name, member-name, and constant-name conventions reviewers consistently enforce so new contributions don't need a naming round-trip on review.
---

# Analyzer naming conventions

The conventions below come from PR review history. They aren't enforced by an analyzer (yet — that's circular). Match them on the first attempt by reading two existing siblings before writing a new file.

## Analyzer class and file name

Pattern: `AG0NNN<VerbPhrase>.cs`, where the verb phrase reads as either a positive prescription or a clear prohibition, matching the resx Title.

Good:
- `AG0006RegisteredComponentShouldHaveExactlyOnePublicConstructor.cs`
- `AG0018PermitOnlyCertainPubliclyExposedEnumerables.cs`
- `AG0050TestMethodNamesMustFollowConvention.cs`

Bad:
- `AG0006Constructor.cs` — too terse, doesn't say what the rule enforces.
- `AG0050TestNames.cs` — doesn't disambiguate enforcement direction.

The file name and the class name must match exactly. The file lives under `src/Agoda.Analyzers/AgodaCustom/` (or the conventional sibling for the rule category — read neighbours first).

## Diagnostic ID constant

The constant holding the rule ID is `DIAGNOSTIC_ID`, all-caps with underscores. **Not** `DiagnosticId`, not `DiagnosticID`.

```csharp
public class AG00XXSomeRule : DiagnosticAnalyzer
{
    public const string DIAGNOSTIC_ID = "AG00XX";
    ...
}
```

Reviewers will flag any other casing as "the standards for constants are `DIAGNOSTIC_ID`."

## Syntax-node callback method name

The method registered with `RegisterSyntaxNodeAction` is named `AnalyzeNode`. This is the convention every existing analyzer uses; deviating from it breaks the "open three random analyzers and they all have the same shape" experience.

```csharp
public override void Initialize(AnalysisContext context)
{
    context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
}

private static void AnalyzeNode(SyntaxNodeAnalysisContext context) { ... }
```

If you register multiple handlers, use distinct verbs that describe what each one inspects (`AnalyzeMethod`, `AnalyzeProperty`) — not `AnalyzeNode1` / `AnalyzeNode2`.

## Reusable helpers — own file, descriptive name

When you extract logic that two or more analyzers will use, place it in `src/Agoda.Analyzers/Helpers/<DescriptiveName>.cs`. Don't nest it as a private class inside one of the analyzers — the next reader won't find it.

Examples that match the convention:
- `TestMethodHelpers.cs`
- `InvocationRule.cs`
- `ForbiddenMethodAnalyzerBase.cs`

If you write `<Verb>Util.cs` or `<Thing>Stuff.cs`, rename it. The name should describe the helper's responsibility precisely enough that a reader looking for "where do we check if we're in test code" finds `TestMethodHelpers` on the first guess.

## Resource keys (`.resx`)

Resource keys for diagnostic titles, messages, and descriptions follow the rule ID:

- `AG00XXTitle`
- `AG00XXMessageFormat`
- `AG00XXDescription`

When you add a new rule, add all three keys in the same PR — missing entries silently produce empty strings in the IDE.

## Read two siblings before writing

Before creating any new analyzer, helper, code-fix, or test file:

1. Open the two most recently merged analyzers in the same category folder.
2. Note: file location, type/file name pattern, callback method name, constant style, resource key style, test file location, doc file location.
3. Match all of the above in your new file.

This pre-empts the entire class of "this should match the convention of other rules" comments.

## Common naming failures

- **`DiagnosticId` instead of `DIAGNOSTIC_ID`** — flagged on review.
- **`AnalyzeFoo` for the syntax-node callback** when neighbouring files use `AnalyzeNode` — flagged for inconsistency.
- **Generic file names** (`Helpers.cs`, `Utils.cs`, `Common.cs`) — flagged; helpers must say what they help with.
- **Class name doesn't match file name** — fails build in some configurations and confuses tooling.
- **Verb phrase that doesn't communicate the rule's direction** (`AG00XXTestMethods.cs` — enforce what about them?) — reviewers will rename.
