---
name: roslyn-rule-documentation
description: Use when writing or editing the user-facing doc for a Roslyn analyzer rule or code fix (a rule's markdown page such as doc/AG0NNN.md, the page a DiagnosticDescriptor helpLinkUri points to, a README conversion table for a CodeFixProvider, or the Title/MessageFormat/Description resource strings). Covers the section order, why-first writing for someone seeing the diagnostic for the first time, Don't/Do direction, the detection-scope section, and the "what gets converted" table for fixers.
---

# Roslyn rule documentation

The reader has just seen the diagnostic ID in their IDE or build log and clicked the help link. They are deciding whether to fix the code, suppress it, or ask someone. Write so that fixing the code is the obvious choice, and so that when the rule is wrong for their case they can tell from the doc rather than from reading the analyzer.

## Before you write

1. Open the two most recently edited sibling docs and copy their heading names, heading levels, code fence tags and section order. Propose layout changes in a separate PR.
2. Open the analyzer's tests. The doc describes what the tests prove, not what the analyzer was meant to do.

## Sections, in order

1. **Title**: `# <ID>: <rule as a phrase>`, matching the resource `Title` string.
2. **Cause**: one or two sentences on what is flagged. No motivation yet.
3. **Why** (`Rule description` in most sibling docs): the concrete failure if it's ignored. For example a test that passes today and fails next month (AG0051), steps that intermittently can't be found (AG0054), log lines that lose their structured values and can't be searched by them (AG0041). "Best practice" or "cleaner code" is not a reason. Write it for a junior engineer.
4. **How to fix**: the smallest change that clears the diagnostic.
5. **Don't / Do** (`Bad example` / `Good example`): the smallest snippet that fires, then the same snippet fixed, so the reader can diff them in their head. No `namespace`/`using`/class noise unless it's what the rule is about. For a ban rule with no direct replacement, drop the Do and say what to do instead and what the risk is.
6. **Detection scope** (`Scope` / `Suppression rules`): which contexts are checked and which are skipped on purpose. Test-only rules list the frameworks and markers they recognise. List the shapes that are deliberately not reported (for example AG0051 skips elapsed months and sentinel years). If the result depends on anything outside the code (the date, the Roslyn version, a package being referenced), say so.
7. **Code fix** (fixers only): see the table below.
8. **How to suppress**: the `[SuppressMessage("<Category>", "<ID>")]` form with the real category string, and the `.editorconfig` line.
9. **Related**: links to the official docs for the API involved (Microsoft Learn, the library's own docs), the issue or standard that motivated the rule, and any sibling rule that overlaps so readers don't disable the wrong one.

## Code fixes: say what is and isn't converted

A fixer that silently skips shapes looks broken. Add a table of before/after pairs, one row per supported form, and a short list of forms that are reported but not fixed, with the reason:

```markdown
| Before | After |
|---|---|
| `Assert.That(x, Is.EqualTo(y))` | `x.ShouldBe(y)` |
| `Assert.That(x, Is.EqualTo(y), "why")` | `x.ShouldBe(y, "why")` |

Not fixed: `Assert.That(x, Is.EqualTo(y).Within(5))` (no Shouldly equivalent with the same tolerance semantics).
```

The Shouldly.FromAssert README "Supported Conversions" table is the model. Add a row in the same PR as every new converted form.

## Check the direction

Don't/Do pairs get swapped more often than you'd expect. After writing:

- The Don't snippet must match a test that expects the diagnostic.
- The Do snippet must match a test that expects no diagnostic (or the fixer's expected output).
- If there's no such test, add one or change the snippet.

## Resource strings

- `Title`: the rule as a short phrase. Tooling such as SonarQube needs it, so it must not be empty.
- `MessageFormat`: what is wrong at this location, with `{0}` placeholders for the symbol names. Don't repeat the title.
- `Description`: one or two sentences. The doc page holds the detail.

## Writing style

- Plain sentences. No marketing, no "simply" or "just".
- Numbers only if they're sourced (AG0051 cites ~13% of flaky-test fixes).
- Code comments in snippets mark the offending line (`// <-- flagged`). They don't restate the code.

## Avoid

- Opening without a why. The reader disables the rule.
- Missing detection scope. Every false-positive report then needs someone to read the analyzer to answer it.
- The canonical explanation living only in an issue or PR thread.
- A helpLinkUri that points at a file that doesn't exist. Check the link resolves on the default branch after merge.
