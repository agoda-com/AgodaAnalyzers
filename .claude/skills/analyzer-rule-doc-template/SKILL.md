---
name: analyzer-rule-doc-template
description: Use when creating or editing a rule's user-facing documentation — `doc/AGxxxx.md` or `src/Agoda.Analyzers/RuleContent/*.html`. Enforces the Do/Don't, why-explained-first format that the maintainers ask for on every rule, written for a developer encountering the diagnostic in their IDE for the first time.
---

# Analyzer rule doc template

The persona to write for: a developer who has just seen `AG0XXX` for the first time in their IDE, doesn't know why their code is flagged, and is deciding between (a) fixing the code, (b) asking on Slack, or (c) disabling the rule. Your job is to make (a) the obvious choice.

The maintainer has stated this standard repeatedly across PRs. Drift from it triggers blocking review comments.

## Required sections, in order

Every rule doc file (`doc/AGxxxx.md` and its `.html` sibling under `RuleContent/`) must contain, in this order:

### 1. Summary — one sentence

What is flagged. No motivation yet. A reader who lands here from "view code analysis" should know in one line whether they're in the right place.

> **AG0050** — public fields on Agoda-injected components must be properties.

### 2. Why — failure mode or business risk

The reason this exists. What goes wrong if the rule is ignored. Cite the consequence in concrete terms (binding redirect churn, runtime crash, silent test pass, security boundary violation, etc.) — not abstract appeals to "best practice."

### 3. Don't — non-compliant code block

A short C# snippet that triggers the rule. **No surrounding boilerplate** — show the smallest input that fires the diagnostic, with the offending construct visible at first glance.

```csharp
public class UserService
{
    public string ApiKey;   // ← AG0050 fires here
}
```

### 4. Do — compliant code block

The recommended replacement. Same shape as the Don't, with the minimal change applied. The reader should be able to diff Don't → Do in their head.

```csharp
public class UserService
{
    public string ApiKey { get; }
}
```

### 5. Detection scope

A bulleted list of which syntactic contexts the analyzer inspects, and which it deliberately doesn't. Reviewers ask for this whenever it's missing — pre-empt it. Include:

- Which class kinds the rule applies to (all classes / only public types / only test code / etc.).
- Which test frameworks are recognised if the rule is test-only (see [[test-context-detection]]).
- Any AST forms deliberately skipped (e.g. generated code, partial-name collisions, fully-qualified references).
- Whether the rule fires in expression-bodied members, lambdas, or local functions.

### 6. Framework/version assumptions

If the rule depends on a specific package version, target framework, or runtime feature, state it explicitly. Otherwise users get false positives in projects that don't match the assumption and can't tell why.

## Stylistic uniformity

Before writing a new doc, open the most-recently-edited rule under `doc/` or `RuleContent/` and **mimic its structure exactly**:

- heading levels (`#` vs `##`)
- code-fence language tags
- icon usage (✗/✓, ❌/✅, or whatever the repo's existing convention is — match it)
- ordering of sections
- whether sample code uses single-line or multi-line braces

Reviewers will ask for consistency edits on any drift. Match first, propose improvements separately if the convention itself needs updating.

## The Do/Don't ordering trap

It's surprisingly easy to label the compliant code "Don't" and vice-versa during a hurried doc edit. Re-read the file after writing. Compare the code in the "Don't" block to what the analyzer's test cases assert *should fire* — they must match. The compliant code is what the analyzer's "no diagnostic" tests assert.

## Cross-link to the rule and the issue

End the doc with a `## Related` section linking:

- The issue or RFC that motivated the rule (so the historical context is one click away).
- Any sibling rule that overlaps or is intentionally distinct, so users don't disable the wrong one.

## Common content failures

- **No "why."** A doc that opens with "this rule flags X" but doesn't say why X is bad puts the reader straight to "disable" or "Slack."
- **Boilerplate code blocks.** A 40-line example with `namespace`, `using`, and `class Wrapper` around three meaningful lines hides the diagnostic in noise. Strip everything not load-bearing.
- **Reversed Do/Don't.** Re-read after writing.
- **No detection scope section.** Users discover the gap when they hit a false positive and file an issue.
- **Documentation that lives in the issue tracker instead of `doc/`.** Discussion is fine in issues; the canonical doc must be in the docs folder so it ships with the rule.
