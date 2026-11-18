---
name: analyzer-rule-doc-template
description: Use when creating or editing a rule's user-facing docs (`doc/AGxxxx.md` or `src/Agoda.Analyzers/RuleContent/*.html`). Enforces a why-first, Do/Don't format aimed at a developer hitting the diagnostic in their IDE for the first time.
---

# Analyzer rule doc template

Write for a developer who just saw `AG0XXX` for the first time and is deciding: fix the code, ask on Slack, or disable the rule. Make "fix the code" obvious.

## Required sections, in order

1. **Summary** — one sentence: what is flagged. No motivation yet.
2. **Why** — the failure mode or concrete risk if ignored (binding-redirect churn, runtime crash, silent test pass, security boundary). Not "best practice."
3. **Don't** — smallest snippet that triggers the rule, no boilerplate around it.
4. **Do** — the recommended fix, same shape, minimal change (reader can diff Don't→Do mentally).
5. **Detection scope** — which contexts are inspected and which are skipped: class kinds, recognized test frameworks (if test-only, [[test-context-detection]]), skipped AST forms (generated code, fully-qualified refs), and whether expression-bodied members / lambdas / local functions fire.
6. **Framework/version assumptions** — any package/TFM/runtime dependency, else users get unexplained false positives.

## Match sibling style

Before writing, open the most recently edited rule under `doc/` or `RuleContent/` and copy its heading levels, code-fence tags, icon convention (✗/✓ etc.), and section order. Propose convention changes separately.

## Verify Do/Don't direction

Easy to swap. After writing, confirm the "Don't" snippet matches what the analyzer's tests assert *should fire*, and "Do" matches the "no diagnostic" tests.

## End with links

`## Related`: the motivating issue/RFC, and any sibling rule that overlaps (so users don't disable the wrong one).

## Avoid

- Opening with no "why" → reader disables or escalates.
- Boilerplate code blocks hiding the diagnostic in `namespace`/`using`/`class` noise.
- Reversed Do/Don't.
- Missing detection-scope section.
- Canonical docs living in issue threads instead of `doc/`.
