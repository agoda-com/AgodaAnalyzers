---
name: roslyn-analyzer-packaging
description: Use when editing the build or packaging of a Roslyn analyzer or code-fix package - the analyzer .csproj (TargetFrameworks, Microsoft.CodeAnalysis.* versions, PackagePath, IncludeBuildOutput, EnforceExtendedAnalyzerRules), AnalyzerReleases.Shipped.md / AnalyzerReleases.Unshipped.md (RS2000-RS2008), a VSIX project or source.extension.vsixmanifest, nuspec, Directory.Build.props, or the CI workflow that builds, packs and pushes the NuGet package.
---

# Roslyn analyzer packaging

An analyzer package is loaded by someone else's compiler, in someone else's IDE, on a Roslyn version you don't control. Most packaging bugs only show up there: the analyzer silently doesn't load, or loads in the CLI build but not in Visual Studio.

## Target framework

- The analyzer assembly targets `netstandard2.0`. The compiler host (VS on .NET Framework, `dotnet build` on .NET) can only load that.
- An extra TFM is only for a consumer that needs it (for example a VSIX built against `net472`). The NuGet payload still comes from the `netstandard2.0` build.
- Don't use APIs that aren't in `netstandard2.0` because they compile in the test project. Test projects target a modern .NET and hide this.

## Microsoft.CodeAnalysis versions

- Pin `Microsoft.CodeAnalysis.CSharp`, `.Common` and `.Workspaces.Common` to the **oldest** Roslyn you support, not the latest. The analyzer won't load in a compiler older than the version it references, and nothing warns the consumer. It just stops reporting.
- Bumping it is a support decision, so it gets its own PR. Say which VS and SDK versions the new floor drops.
- `Microsoft.CodeAnalysis.Analyzers` is a development dependency: `PrivateAssets="all"`.
- Keep the test project's Roslyn version in step with the analyzer's, or the tests run against a compiler consumers don't have.

## Package layout

- The analyzer DLL, its satellite resource DLLs and PDB go in `analyzers/dotnet/cs`. Set `IncludeBuildOutput=false` so it isn't also shipped as a `lib/` reference.
- Use `bin\$(Configuration)\...` in pack paths, never a hard-coded `bin\Release\`. AgodaAnalyzers #215: a Release-only path meant a Debug build failed until someone had built Release once.
- Any runtime dependency of the analyzer has to be loadable by the compiler host. Bundle it into `analyzers/dotnet/cs` or remove it. A normal `PackageReference` flows to consumers as their dependency and still isn't on the analyzer's load path.
- Keep `EnforceExtendedAnalyzerRules=true`. It turns on the RS1xxx checks that catch banned APIs (file IO, `Environment`, etc.) in analyzer code.

## Release tracking (RS2000-RS2008)

- Every new diagnostic ID gets a row in `AnalyzerReleases.Unshipped.md` in the same PR: ID, category, default severity, and notes with the doc link.
- A change to category or default severity is a `### Changed Rules` entry, not an edit of the old row.
- Rows move to `Shipped.md` when a release is cut, not while the rule is in progress.
- Don't suppress RS2008 to get a build green. Add the row.

## VSIX and NuGet

- Build both from the same analyzer project. The VSIX takes a `ProjectReference` to it. It never gets its own copy of the source or a second analyzer project.
- When a project is renamed or removed, update `source.extension.vsixmanifest` too. AgodaAnalyzers #185: the manifest still referenced a deleted `Agoda.Analyzers.CodeFixes` project, and a fresh clone didn't build.

## Versioning

- The package version comes from CI (for example `-p:PackageVersion=<major.minor>.<run number>`), not from the csproj. Don't hand-set `Version`/`PackageVersion` in a project file to "bump" a release.
- Preview builds from branches get a `-preview` suffix, so a PR build can't be mistaken for a release.
- Leave `AssemblyVersion` alone when it's deliberately fixed at `1.0.0.0`. Changing it makes consumers' binding redirects churn.

## CI

- Build, test and pack from one solution, in Release, with `--no-build` on the steps after the build, so the tested bits are the packed bits.
- Keep deploy steps minimal. Use `dotnet nuget push` and nothing else. Shouldly.FromAssert #25: an unused `setup-nuget` step needed Mono, which ubuntu-latest no longer ships, and it failed the deploy job on every PR.
- `--skip-duplicate` on push so a re-run doesn't fail.
- Don't add a TFM, a Roslyn bump, or a runner OS change as part of an unrelated PR.

## Checklist for a packaging change

- [ ] `dotnet pack` output opened (it's a zip): DLL + resources under `analyzers/dotnet/cs`, nothing under `lib/`
- [ ] Clean clone builds in both Debug and Release
- [ ] New rule IDs are in `AnalyzerReleases.Unshipped.md`
- [ ] The Roslyn floor is unchanged, or the PR says what it drops
- [ ] Tried the package in a consumer project and saw a diagnostic fire, if the layout or dependencies changed
