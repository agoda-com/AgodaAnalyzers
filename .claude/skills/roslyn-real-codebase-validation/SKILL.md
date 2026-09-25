---
name: roslyn-real-codebase-validation
description: Use before releasing a new Roslyn analyzer rule, a rule whose detection was broadened or narrowed, or a new or changed CodeFixProvider — or when a PR claims a rule "reduces false positives", "covers more shapes" or "fixes all X". Runs the local analyzer build against a real repo, labels a sample of diagnostics as true/false positives, runs `dotnet format analyzers --diagnostics <ID>` then build, test and git diff to classify fixer output (converted / unconverted / doesn't compile / changes meaning), ranks leftovers into issues, and puts the numbers in the PR.
---

# Validate a rule or fixer on a real codebase

Unit tests cover the shapes you thought of. A real repo has the ones you didn't. Most fixer bugs in the last two years (Shouldly.FromAssert #10, #12–#18) were found by running the fixer over one 1,340-assert suite, not by review. Do that run **before** release, and put the numbers in the PR.

Commands, wiring and the report template are in [references/validation-loop.md](references/validation-loop.md). Two helpers are in `scripts/`.

## 1. Pick a target that exercises the rule

At least one real repo that uses the API the rule is about, at the size where gaps show (hundreds of call sites, not ten). Find one by searching for the API (Sourcegraph, `git grep`), not by picking the repo you have open (AgodaAnalyzers #233 cites a Sourcegraph survey). For a test-only rule, the target needs a real test suite.

The target is a probe. Work on a throwaway branch, never push it, and don't commit the analyzer wiring.

## 2. Analyzer: count, then label a sample

- **Raised:** distinct diagnostics per ID (`scripts/count_diagnostics.py` on the build log; multi-targeted projects report each site once per target framework).
- **Precision:** label a random sample as true or false positive: all of them if there are 30 or fewer, otherwise at least 30. Report the false-positive rate and group the false positives by cause. A false-positive group is a scope bug for `roslyn-analyzer-scope`, not a note for the doc.
- **Recall**, when the rule targets a known API: search the repo for the sites the rule *should* flag and report how many it did. Before #246, AG0041 missed 40 of 45 Serilog sites in one repo because they were exception-first calls.
- **Changed rule:** run the old and new build on the same commit and report both counts. #242 needs "N before, M after, and the M are all still real".

## 3. Fixer: format, build, test, diff

Do this on a clean checkout of the target:

1. Build and run the tests once. Save the results (`dotnet test --logger trx`) as the baseline.
2. `dotnet format analyzers --diagnostics <ID> --severity info` (the bulk path people actually use; it runs Fix All).
3. Build. Every new compiler error is a fixer bug.
4. Run the tests again. Compare outcomes against the baseline (`scripts/compare_trx.py`). **A test whose result changed points to a fixer that changed meaning.** Shouldly.FromAssert #12 was found this way: 8 of 9 `Is.Not.Empty` asserts inverted, and they compiled.
5. Read `git diff`. Look for dropped arguments, lost comments, flattened multi-line calls and weakened checks that still pass (`ShouldContain` is case-insensitive where the NUnit original was not, SFA #15).
6. Build again with the analyzer on and count what's still reported.

Sort every diagnosed site into one bucket:

| Bucket | How to find it |
|---|---|
| converted, same meaning | changed in diff, builds, test result unchanged, reads right |
| left unconverted | still reported after step 6 |
| doesn't compile | new errors in step 3 |
| changes meaning | test result changed in step 4, or found by reading the diff |

"Doesn't compile" and "changes meaning" block the release. "Left unconverted" is fine if the analyzer doesn't report it (`roslyn-codefix-fix-all-and-parity`); otherwise the warning can never be cleared.

## 4. Turn leftovers into issues

Group what's left unconverted (or reported with no fix) by syntactic form, count each form, and rank. File the top forms as issues with the count and one minimal example each. SFA #16 is the model: 494 leftovers, 137 of them one shape (a message argument), and a table of constraints with counts.

Every "doesn't compile" and "changes meaning" site becomes a regression test first (`roslyn-analyzer-testing`), then a fix.

## 5. Put the numbers in the PR

Use the report block in the reference. At minimum: the target (name or description), the commit, diagnostics raised, false-positive rate and sample size, and for fixers the four buckets. #246 is the model: "40 of the 45 sites measured on `supply-ari-web`".

These repos are public. Quote **counts and shapes**, and write small made-up examples of each shape. Don't paste code, file paths, type names or test names from an internal repo into a PR or issue. Naming the internal repo is fine.

## Checklist

- [ ] Target chosen by searching for the API; big enough to show gaps; work not pushed.
- [ ] Distinct diagnostics per ID; random sample labelled; false-positive rate and causes reported.
- [ ] Recall reported against a search for known sites (when the API is known).
- [ ] Fixer: baseline tests, `dotnet format analyzers`, build, tests, diff, re-count, in that order.
- [ ] Every site in one of the four buckets; zero "doesn't compile" and zero "changes meaning" before release.
- [ ] Leftovers ranked by form and filed as issues with counts.
- [ ] Numbers in the PR; no internal code pasted.
