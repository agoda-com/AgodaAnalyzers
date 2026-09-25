# Validation loop: commands and report

## Wire the local build into the target

Build the analyzer in Release, then point the target at the DLL. Put this in a **throwaway** `Directory.Build.props` (or append to the target's existing one and discard the change afterwards):

```xml
<Project>
  <ItemGroup>
    <!-- local analyzer under test; not committed -->
    <Analyzer Include="/abs/path/to/AgodaAnalyzers/src/Agoda.Analyzers/bin/Release/netstandard2.0/Agoda.Analyzers.dll" />
  </ItemGroup>
</Project>
```

- If the target already references a published version of the same package, remove that `PackageReference` for the run, or both versions load and every diagnostic doubles.
- Code fixes must be in an assembly that is also passed as an `Analyzer` item, or `dotnet format` can't see them. Add the fixer DLL too if it's separate.
- Analyzer dependencies (for example a bundled helper DLL) need their own `Analyzer` items.
- Alternative: `dotnet pack` to a local folder, add it as a source in a throwaway `nuget.config`, and bump the target's `PackageReference` to the local version. Slower, but it tests the packaging too.

Force the rule on at warning level so it shows in the build log, in the target's `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.AG0041.severity = warning
```

## Analyzer run

```bash
cd /path/to/target
dotnet build -nologo -clp:NoSummary > build-new.log 2>&1
python3 /path/to/skill/scripts/count_diagnostics.py build-new.log AG0041
python3 /path/to/skill/scripts/count_diagnostics.py build-new.log AG0041 --sites > sites.txt
shuf -n 30 --random-source=<(yes) sites.txt > sample.txt   # repeatable sample to label
```

For a changed rule, repeat with the previous analyzer build into `build-old.log` on the same target commit and report both counts.

Recall: search for the sites the rule should catch (`git grep -nE '\.(Error|Warning|Information)\(\s*\w+\s*,\s*\$"'`), then check each against `sites.txt`.

## Fixer run

```bash
git switch -c probe/ag0041                                   # throwaway
dotnet test --logger trx --results-directory ./probe-results/before
dotnet format analyzers --diagnostics AG0041 --severity info --verbosity diagnostic
dotnet build -nologo -clp:NoSummary > build-after-fix.log 2>&1   # new errors = doesn't compile
dotnet test --logger trx --results-directory ./probe-results/after
python3 /path/to/skill/scripts/compare_trx.py probe-results/before probe-results/after
git diff --stat && git diff
python3 /path/to/skill/scripts/count_diagnostics.py build-after-fix.log AG0041   # left unconverted
```

- `dotnet test` writes one TRX per test project; `compare_trx.py` reads every `.trx` under each folder. Keep `probe-results/` out of the diff (`git clean` it, or put it outside the repo).
- `dotnet format` needs a restorable solution or project. Point it at one explicitly (`dotnet format analyzers App.sln ...`) when the folder has several.
- If `dotnet format` reports nothing fixed while the build reports the ID, the fixer isn't loaded (see the wiring notes) or `--severity` is filtering it out.
- `--include <paths>` narrows the run to a folder when the whole repo is too slow for a first pass.

## Report block for the PR

```markdown
## Real-codebase validation

Target: <repo name or "an internal test suite of ~N files">, commit <sha>, analyzer <branch/sha>.

| | Count |
|---|---|
| Diagnostics raised (distinct sites) | 312 |
| Sample labelled | 30 (28 true, 2 false positive) |
| False-positive causes | 2 × string interpolation in a custom logger wrapper |
| Known sites found (recall) | 43 / 45 |

Fixer (`dotnet format analyzers --diagnostics AG0041`):

| Bucket | Count |
|---|---|
| Converted, same meaning | 290 |
| Left unconverted (not reported) | 12 |
| Left unconverted (still reported) | 10 → #NNN |
| Doesn't compile | 0 |
| Changes meaning (test result changed / diff review) | 0 |

Tests: 1,204 before / 1,204 after, 0 outcome changes.
```

The numbers above show the layout only; they aren't real results.
