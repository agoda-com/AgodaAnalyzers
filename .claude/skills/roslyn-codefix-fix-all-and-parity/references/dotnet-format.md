# Bulk runs with `dotnet format analyzers`

Read this before releasing a fixer, or when a fixer works in the IDE and the test harness but a bulk run
gives a different result.

## Why it matters

Most people don't fix a migration diagnostic one light bulb at a time. They run:

```bash
dotnet format analyzers <solution-or-project> --diagnostics SHU001 --severity info
```

That runs the analyzer over the solution and applies the fixer's **Fix All provider** at solution
scope. Every Fix All bug shows up here, over every file at once: a wrong conversion, flattened trivia,
edits that clash and get dropped. The Shouldly.FromAssert bug wave (issues #10 and #12 to #18) was found
by one such run over a 1,340-assert suite.

## Things that catch people out

- **`--severity` defaults to `warn`.** A rule whose default severity is `info` (or that a project has set
  to `suggestion`) is skipped unless you pass `--severity info`.
- **`--diagnostics` filters by ID**, so it also works for a fixer that lists a third-party ID (a Sonar
  `S****` or `IDE****` ID) as long as the analyzer that reports it is loaded by the project.
- **The analyzer has to be loaded by the project** being formatted: a package reference, or a project
  reference with `OutputItemType="Analyzer"`. Loading your fixer from a local build is the quickest way
  to try an unreleased change.
- **A fixer without a Fix All provider is skipped** in a bulk run. Look at the output for a message that
  a code fix doesn't support Fix All.
- **Overlapping edits.** At solution scope, `BatchFixer` merges all the edits for a document. Edits that
  overlap can be dropped, so the result can differ from fixing one at a time. Nested diagnostics need the
  single-edit approach from the skill (rule 6).
- **`--verify-no-changes`** exits non-zero if anything would change. Useful in CI once a repo has been
  migrated, to stop the old pattern coming back.

## The validation loop

Run this on at least one real repo before releasing a new fixer or a new conversion:

```bash
git switch -c try-fixer
dotnet build && dotnet test > before.txt          # baseline pass/fail per test
dotnet format analyzers --diagnostics <ID> --severity info
dotnet build                                      # fixed-but-doesn't-compile shows up here
dotnet test > after.txt                           # compare with before.txt
git diff --stat && git diff                       # read a sample of the conversions
dotnet build 2>&1 | grep -c '<ID>'                # what's left (warnings only; info doesn't show in build output)
```

Then report these in the PR:

| Count | What it tells you |
|---|---|
| Diagnostics before the run | Size of the problem |
| Converted | What the fixer handles |
| Left unconverted (still reported) | Parity gaps: group them by shape and count each |
| Converted but doesn't compile | Syntax construction bugs |
| Converted, compiles, but a test changed result | Meaning changed. This is the most serious one |

A test that went from pass to fail, or fail to pass, after the fix points to a conversion that changed
what the test checks (Shouldly.FromAssert #12: 8 of 9 `Is.Not.Empty` asserts were inverted). Group the
leftovers by shape, count them, and turn the biggest groups into issues (Shouldly.FromAssert #16 is a good
example).
