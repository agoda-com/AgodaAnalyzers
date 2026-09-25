---
name: roslyn-analyzer-testing
description: Use when adding or changing tests for a Roslyn DiagnosticAnalyzer or CodeFixProvider — any *UnitTests.cs / *Tests.cs that uses DiagnosticVerifier, CodeFixVerifier, VerifyDiagnosticsAsync, VerifyCodeFixAsync, CSharpAnalyzerTest, CSharpCodeFixTest, ExpectedDiagnostics, FixedCode, BatchFixedCode or ReferenceAssemblies. Covers negative cases, regression tests that fail without the fix, compiling fixed output against the real target package, round-trip and Fix All checks, tests that pass for the wrong reason, and testing a fixer for a third-party (Sonar/IDE) diagnostic ID.
---

# Roslyn analyzer and code-fix testing

A green analyzer test proves less than it looks. Most shipped analyzer bugs had passing tests: the positive case was covered and the look-alike, the second shape or the compiled output wasn't. Harness APIs for both styles are in [references/harness-cheatsheet.md](references/harness-cheatsheet.md).

## 1. Every positive case gets negatives

For each case that should report, add cases that look similar and must not. Vary them along the axes from `roslyn-analyzer-scope` (context: test vs production, generated, look-alike type names) and `roslyn-syntax-shape-coverage` (literal vs constant vs local, `await`, `?.`, named arguments, overloads). Also: accessibility, generic vs non-generic, `using static` / alias / fully-qualified names.

A negative asserts **zero diagnostics** explicitly (`EmptyDiagnosticResults`, or no `ExpectedDiagnostics` and no markup). It must also reach the check it is meant to prove. If the input doesn't reference the target type, a symbol-based analyzer registers nothing and every negative passes. Write each negative as a near copy of a positive, with one thing changed.

```csharp
// Don't: passes because Playwright isn't referenced, not because the guard works
var code = "class C { void M() { var s = \"#submit\"; } }";
await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
// Do: same file as the positive case, only the call site differs
var code = PlaywrightTest("var s = \"#submit\"; logger.Info(s);");
await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
```

Evidence: AgodaAnalyzers #235 (reviewer asked for `global using` and fully-qualified cases), #229 (method-level `[Fact]`/`[Test]` detection had no test).

## 2. A regression test reproduces the issue and fails without the fix

Write the test from the issue's exact input first. Run it against the unfixed code and watch it fail, then fix. Put the issue number in the test name (`Issue241_ElapsedMonth_IsNotFlagged`, or `.SetName("... (#14)")`).

Say it in the PR: "4 new cases, all 4 fail against the old fixer." To check after the fact, revert only the `src/` change (`git stash push -- src/<Project>/`), run the filtered tests, restore.

Evidence: Shouldly.FromAssert #21, #22, #24 each state which new cases fail on `main`.

## 3. Fixer tests compile the fixed output against the real target package

If the fixer emits calls into a library (Shouldly, Serilog, FluentAssertions), the test must reference **the version users run**, and fail on compiler errors in `FixedCode`. That is how Shouldly 4.2.1's `[Obsolete(error: true)]` `Func<string>` overloads were caught (Shouldly.FromAssert #26).

```csharp
ReferenceAssemblies = ReferenceAssemblies.Default.AddPackages(ImmutableArray.Create(
    new PackageIdentity("Shouldly", "4.2.1"),
    new PackageIdentity("NUnit", "3.14.0")));
```

A harness with a fixed list of `MetadataReference`s (the StyleCop-derived `CodeFixVerifier`) can't do this: the input can't reference the package, so overload and obsolete errors never surface. Use `CSharpCodeFixTest` for any fixer that targets a third-party API.

## 4. Check the round trip and Fix All

- After the fix, the diagnostic is gone. Both harnesses re-run the analyzer on `FixedCode`; don't turn that off (`CodeFixTestBehaviors.SkipFixAllCheck`, a non-zero remaining-diagnostics list) without a comment saying why.
- Fix All output must equal fixing one at a time. If it can't (nested diagnostics, overlapping spans), set `BatchFixedCode` and explain the difference in a comment. Include at least one test with **two or more** diagnostics in one document, so Fix All actually batches.
- Include a multi-line call with a comment between arguments. Trivia loss only shows up on Fix All (AgodaAnalyzers #246).
- When the fixer deliberately skips a shape, test that the analyzer does not report it either (`roslyn-codefix-fix-all-and-parity`), or that it reports and `GetOfferedCSharpFixesAsync` / `CodeActionIndex` shows no action.

## 5. Watch for tests that pass for the wrong reason

- The test code is skipped by the rule for an unrelated reason: an `internal` test method when the rule only looks at `public` ones (AgodaAnalyzers #233), a class outside the namespace the rule checks, a missing `[Test]` attribute.
- The expected output locks in a bug. AG0041 expected `{Ex.Message}`, an illegal Serilog property name (#246). Read every expected fix as if it were production code.
- The markup span is on the wrong node, and the test still passes because the harness only compares line/column starts (`DiagnosticLocation(line, column)`). Prefer full spans (`WithSpan(l1, c1, l2, c2)` or `[|...|]`).

If an existing expectation is wrong, fix it and say so in the PR. Otherwise **add** tests rather than editing old ones, so a behaviour change can't hide in a modified assertion.

## 6. Fixers for someone else's diagnostic ID use a stub analyzer

When a fixer lists a Sonar or IDE ID in `FixableDiagnosticIds`, don't add the third-party analyzer package to the test project. Add a small test-only `DiagnosticAnalyzer` that reports that ID on the shape you fix, and run `CSharpCodeFixTest<StubAnalyzer, YourFixer, NUnitVerifier>`. You are testing your fixer, not their detection. Suppress the analyzer-authoring rules (RS1036, RS2008) on the stub with a comment. See `AG0041LogTemplateAnalyzerTests.StubSonarS2629Analyzer` (#246).

## 7. Test data a reviewer can read

A reviewer should see input and expected output without assembling a template in their head (#184). For table-driven tests: a short fixed wrapper, then `Setup` / `Input` / `Expected` per case, with a `SetName` that states the shape. Put `using`s and fields in the wrapper, not in every case. Use `nameof` and `DiagnosticId` constants, not string IDs.

## 8. Know the baseline before you run the suite

Run the suite on a clean checkout of the target branch first and record what already fails. In the PR, list pre-existing failures and say they fail identically on the base branch (#242, #246: 7 StyleCop CRLF expectations). Never "fix" an unrelated failing test in a rule PR.

## Checklist

- [ ] Each positive case has negatives along the context and shape axes; negatives assert zero diagnostics.
- [ ] Regression tests use the issue's input, carry the issue number, and were seen failing without the fix (stated in the PR).
- [ ] Fixer tests reference the real target package version and fail on compiler errors in the fixed code.
- [ ] At least one test with 2+ diagnostics in a document; Fix All output checked; multi-line/comment trivia case.
- [ ] Shapes the fixer skips are tested for no-report or no-action.
- [ ] Every expected output reviewed as production code; no test relies on an unrelated skip.
- [ ] Third-party IDs tested through a stub analyzer, not a package dependency.
- [ ] Pre-existing failures listed and confirmed on the base branch.
