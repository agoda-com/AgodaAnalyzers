---
name: roslyn-rule-design
description: Use when proposing a new Roslyn analyzer rule, changing a shipped rule's ID, default severity, category or enabled-by-default state, deciding whether to add a code fix for a third-party diagnostic (Sonar S-rules, IDE rules) via FixableDiagnosticIds, adding diagnostic properties such as confidence, or scoping a PR. Covers rule independence (never narrow a rule because another rule catches the case), AnalyzerReleases.Shipped.md / Unshipped.md (RS2008) as the public contract, fixer-only support for someone else's diagnostic ID, and one rule per PR. Trigger on - new DiagnosticDescriptor, DiagnosticSeverity, isEnabledByDefault, AnalyzerReleases.Unshipped.md, FixableDiagnosticIds containing a non-AG ID, a comment like "AG00NN already covers this".
---

# Rule design

Decisions about *what* a rule is, before how it detects. Each one here is cheap to get right up front and expensive to change once the rule ships.

## 1. Rules are independent

Every rule can be turned off on its own (`.editorconfig`, `.globalconfig`, `#pragma`, `[SuppressMessage]`). Never narrow detection or skip a test because another rule "already catches it".

```csharp
// Don't
// Thread.Sleep isn't checked here: AG0023 already bans it.

// Do: AG0052 checks Thread.Sleep with a hardcoded duration too (AA #229),
// because a team can disable AG0023 and still want AG0052.
```

The same goes for robustness: a rule must not crash on input another rule forbids (`dynamic`, AA #80 review: "User can disable any rule at any time, so other checks should work properly"). Overlap between rules is fine. Say so in the rule doc.

## 2. ID, severity and default state are a public contract

Consumers have `.editorconfig` entries, suppressions and `TreatWarningsAsErrors` keyed on the ID and severity. Once shipped:

- **Never reuse or renumber an ID.** A retired rule's ID stays retired.
- Changing default severity, category or `isEnabledByDefault` is a breaking change. It goes in `AnalyzerReleases.Unshipped.md` under `### Changed Rules` and in the release notes.
- A new rule goes in `AnalyzerReleases.Unshipped.md` under `### New Rules` in the same PR. RS2008 fails the build otherwise.

```markdown
### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
AG0051  | Agoda.CSharp.CustomQualityRules | Warning | Agoda.CSharp.CustomQualityRules | Info | Promoted after false-positive fixes (#242)
```

Pick the default severity for the *first* release carefully: `Warning` on a noisy rule breaks every `TreatWarningsAsErrors` build that updates the package. When unsure, ship as `Info` and promote once real-codebase numbers (`roslyn-real-codebase-validation`) show it's quiet.

## 3. Fixer-only for someone else's ID

A `CodeFixProvider` can list a third-party ID in `FixableDiagnosticIds` without adding an analyzer. Use it when the other analyzer already reports the problem and you only want to offer the fix.

```csharp
// Don't: a new AG rule that re-reports what Sonar S6678 already reports → two warnings per site.

// Do: fix S6678 without duplicating the diagnostic (AA #246)
public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("S6678");
```

Good fit when the fix is a deliberate bulk operation (`dotnet format analyzers --diagnostics S6678`) that shouldn't show up as a new build warning. Test it with a test-only stub analyzer that reports the ID, not a package reference to the third-party analyzer.

## 4. Say how sure you are

If a rule has high- and low-confidence cases, put it in the diagnostic's properties so fixers, tooling and suppression scripts can tell them apart:

```csharp
// Add to whatever properties the repo already requires on every diagnostic.
context.ReportDiagnostic(Diagnostic.Create(Rule, location, Properties.Add("confidence", "high")));
```

Evidence: AA #242 (AG0051 reports `confidence=high` after dropping low-risk cases). If most cases are low confidence, that's a sign the rule's scope is too wide (`roslyn-analyzer-scope`).

## 5. Design the rule so it can be cleared

Before writing the analyzer, write the "Do" for every case it reports. If there's a reported shape with no acceptable alternative, the rule will get suppressed wholesale. Narrow it or give it a separate ID. If a fixer is planned, the analyzer only reports what the fixer or a human can change (`roslyn-codefix-fix-all-and-parity`).

## 6. One rule per PR

A PR adds or changes one diagnostic ID. #226 bundled AG0051–AG0054 and was closed in favour of one PR per rule (#228, #229, #230, #231), so each could be reviewed, merged or dropped on its own. AG0053 (#230) was dropped without holding up the other three.

Before opening the PR:

```bash
git fetch origin
git diff --name-only origin/master...HEAD   # (or origin/main) every file relates to the one rule
```

If a rule you didn't touch shows up as a new addition, the branch is behind the default branch: rebase (AA #235 review). Shared helper fixes go in their own PR first.

## Checklist

- [ ] No code, comment or missing test justified by "another rule covers it".
- [ ] New ID claimed in an issue first; never reused.
- [ ] `AnalyzerReleases.Unshipped.md` updated: `New Rules` or `Changed Rules`.
- [ ] Default severity chosen for the first release, with real-codebase numbers if `Warning`/`Error`.
- [ ] Fixer for a third-party ID has no duplicate AG analyzer and is tested with a stub.
- [ ] Every reported shape has a "Do".
- [ ] One diagnostic ID in the diff.

Related: `roslyn-analyzer-scope`, `roslyn-rule-documentation`. ID claiming and file naming for a given repo are in its `AGENTS.md`.
