# AgodaAnalyzers

Opinionated Roslyn analyzers (and a few code fixes) for C#, shipped as the `Agoda.Analyzers` NuGet package and a VSIX. `CLAUDE.md` is a symlink to this file.

## Layout

| Path | What |
|---|---|
| `src/Agoda.Analyzers/AgodaCustom/` | Agoda rules (`AG0NNN*.cs`), their code fixes (`*FixProvider.cs` / `*CodeFixProvider.cs`) and `CustomRulesResources.resx` |
| `src/Agoda.Analyzers/StyleCop/` | Ported StyleCop rules (`SA*`). Leave these alone unless the task is about them |
| `src/Agoda.Analyzers/Helpers/` | Shared helpers (test-context detection, Fix All providers, trivia, type symbols) |
| `src/Agoda.Analyzers/AnalyzerReleases.*.md` | Release tracking (RS2008) |
| `src/Agoda.Analyzers.Test/AgodaCustom/` | Analyzer tests, `AG0NNNUnitTests.cs` |
| `src/Agoda.Analyzers.Test/CodeFixes/AgodaCustom/` | Code-fix tests, `AG0NNNFixProviderUnitTests.cs` |
| `src/Agoda.Analyzers.Test/AllAnalyzersUnitTests.cs` | Checks every analyzer: non-empty `Title`, and every `Diagnostic.Create` passes properties |
| `src/Agoda.Analyzers.Vsix/` | VSIX wrapper around the same analyzer project |
| `doc/AG0NNN.md` | User-facing rule docs. Each descriptor's `helpLinkUri` points here |

## Build and test

```bash
cd src
dotnet build AgodaAnalyzers.Build.sln
dotnet test Agoda.Analyzers.Test/Agoda.Analyzers.Test.csproj --filter "FullyQualifiedName~AG0NNN"
```

The test project targets `net8`. On a machine that only has a newer runtime, set `DOTNET_ROLL_FORWARD=Major`. Under a rolled-forward runtime a few StyleCop code-fix tests fail on newline expectations, so run with `--filter "FullyQualifiedName!~StyleCop"` for the rest of the suite and say so in the PR.

CI (`.github/workflows/build.yml`) builds `AgodaAnalyzers.Build.sln` in Release on Windows, runs the tests with coverage, packs with a CI-derived version and pushes to NuGet on master.

## Conventions for a new rule

Claim the ID first: open an issue titled `AG0NNN: <rule>` with the `New rule` label, using the next ID not taken by an open or closed issue. One rule per PR.

A new rule ships with all of these in the same PR:

- **Analyzer**: `src/Agoda.Analyzers/AgodaCustom/AG0NNN<RuleAsPhrase>.cs`. The class name matches the file name, and the phrase says which way the rule points (`AG0006RegisteredComponentShouldHaveExactlyOnePublicConstructor`, not `AG0006Constructor`).
- **ID constant**: `public const string DIAGNOSTIC_ID = "AG0NNN";`, written exactly like that.
- **Descriptor**: category `AnalyzerCategory.CustomQualityRules`, `AnalyzerConstants.EnabledByDefault`, and `helpLinkUri` `$"https://github.com/agoda-com/AgodaAnalyzers/blob/master/doc/{DIAGNOSTIC_ID}.md"`.
- **Diagnostic properties**: pass a properties dictionary with `AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES` to every `Diagnostic.Create`. `AllAnalyzersUnitTests` fails without it.
- **Callbacks**: `AnalyzeNode` for a single syntax-node callback. With several, use a verb per target (`AnalyzeMethod`, `AnalyzeNamedType`), not `AnalyzeNode1`/`AnalyzeNode2`.
- **Resources**: `AG0NNNTitle`, `AG0NNNMessageFormat` and `AG0NNNDescription` in `AgodaCustom/CustomRulesResources.resx`, loaded with `LocalizableResourceString`. A code fix also gets `AG0NNNFixTitle`. Don't use `DescriptionContentLoader` in new rules. It returns an empty string, and the `RuleContent/*.html` files it used to load no longer exist.
- **Shared logic** in its own file in `Helpers/`, named for what it does (`TestMethodHelpers`, `TypeSymbolHelper`). Not a nested private class, and not `Utils.cs` or `Common.cs`. Check `Helpers/` before writing a new test-context or type check.
- **Tests** in `src/Agoda.Analyzers.Test/AgodaCustom/AG0NNNUnitTests.cs` (and `CodeFixes/AgodaCustom/` for a fixer), with test names that start with the rule ID (`AG0054_WhenBindingPartialClassSplitAcrossFiles_ShowError`). Stub third-party types inline in the test source rather than adding a package reference, unless the package is already referenced.
- **Doc**: `doc/AG0NNN.md`. Follow the `roslyn-rule-documentation` skill.
- **Release tracking**: a row in `src/Agoda.Analyzers/AnalyzerReleases.Unshipped.md` with the doc link.

The legacy `CONTRIBUTING.md` still mentions a `Agoda.Analyzers.CodeFixes` project and `RuleContent/*.html` descriptions. Both are gone. Code fixes live next to their analyzer in `AgodaCustom/`, and docs go in `doc/`.

## Skills

Agent skills live in `.claude/skills/` (Claude Code and Cursor both load them). The `roslyn-*` skills are generic Roslyn guidance and are meant to be copied unchanged to other analyzer repos, such as agoda-com/Shouldly.FromAssert. Put anything specific to this repo in this file, not in a `roslyn-*` skill. The full plan is in [#247](https://github.com/agoda-com/AgodaAnalyzers/issues/247).
