# Test harness cheat-sheet

Two harness styles are in use. Pick by what the test needs, not by what the neighbouring file uses.

| Need | Harness |
|---|---|
| Analyzer only, BCL types only | either |
| Input or fixed code references a NuGet package (Shouldly, Serilog, NUnit, Playwright) | `Microsoft.CodeAnalysis.Testing` |
| Fixer uses `WellKnownFixAllProviders.BatchFixer` | `Microsoft.CodeAnalysis.Testing` (the legacy `CodeFixVerifier` asserts the provider is *not* `BatchFixer`) |
| Fixer for a third-party ID via a stub analyzer | `Microsoft.CodeAnalysis.Testing` |

## Microsoft.CodeAnalysis.Testing (`CSharpAnalyzerTest` / `CSharpCodeFixTest`)

```csharp
var test = new CSharpCodeFixTest<MyAnalyzer, MyCodeFixProvider, NUnitVerifier>
{
    TestCode = source,            // markup: [|span|] or {|AG0041:span|}
    FixedCode = fixedSource,
    BatchFixedCode = batchFixed,  // only when Fix All legitimately differs
    ReferenceAssemblies = ReferenceAssemblies.Default.AddPackages(ImmutableArray.Create(
        new PackageIdentity("Serilog", "2.10.0"))),  // the version consumers run, not the latest
};
test.ExpectedDiagnostics.Add(new DiagnosticResult(MyAnalyzer.Rule).WithSpan(13, 36, 13, 69).WithArguments("string interpolation"));
await test.RunAsync(CancellationToken.None);
```

Useful knobs:

- `CompilerDiagnostics = CompilerDiagnostics.Warnings` — also fail on new compiler warnings in the fixed code (the default checks errors only).
- `NumberOfFixAllIterations`, `NumberOfIncrementalIterations` — set when one fix exposes another diagnostic; a surprising count is usually a bug.
- `CodeActionIndex` / `CodeActionEquivalenceKey` — pick a specific action when the fixer offers several.
- `TestState.AdditionalFiles`, `TestState.AnalyzerConfigFiles` — `.editorconfig` driven options and severities.
- `DisabledDiagnostics.Add("CS1591")` — only for noise unrelated to the rule, with a comment.

Markup spans: `[|...|]` means "the single expected diagnostic of the analyzer under test is here". Use `{|ID:...|}` when there are several IDs.

## Legacy StyleCop-derived verifier (`DiagnosticVerifier` / `CodeFixVerifier`)

```csharp
[TestFixture]
internal class AG0051UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0051DetectHardcodedDateLiterals();
    protected override string DiagnosticId => AG0051DetectHardcodedDateLiterals.DiagnosticId;

    [Test]
    public async Task ElapsedMonth_IsNotFlagged()
        => await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
}
```

- References come from a fixed list (`MetadataReferences`) plus `CodeDescriptor.References` assemblies loaded from the test project. Anything not referenced by the test project can't be used in input code.
- `DiagnosticLocation(line, column)` checks the start only. Two diagnostics on one line with the wrong end still pass.
- Compiler errors in the input are reported alongside analyzer diagnostics, so broken test code fails the test (good). In `VerifyCodeFixAsync`, `allowNewCompilerDiagnostics: true` hides a fixer that emits broken code — leave it `false`.
- `VerifyCodeFixAsync` runs single-fix, then Fix All at document, project and solution scope. Pass `batchNewSource` when batch output differs.

## Proving a regression test fails without the fix

```bash
git stash push -- src/Agoda.Analyzers/            # revert only the production change
dotnet test src/Agoda.Analyzers.Test --filter "FullyQualifiedName~AG0041" --no-restore
git stash pop
```

On a machine with only a newer runtime, the net8 test host needs `DOTNET_ROLL_FORWARD=Major`.
