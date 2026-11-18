---
name: dotnet-testing
description: Use when writing or modifying tests in a .NET library's test projects, or when making a behavioral change to production code that needs test coverage. Covers test structure, member references, helper visibility, timing-sensitive tests, platform-gating, and which edge cases reviewers expect.
---

# .NET testing

Every behavioral change to production code ships with a test. Bug fixes include a regression test that would have failed before the fix. New features cover the golden path plus edge cases a reviewer is likely to ask about (null, empty, boundary sizes, double-dispose, collections larger than common loop-unroll thresholds).

## Avoid modifying existing tests

If you modify an existing test it means you have potentially modified behavior. Even though it's not a breaking contract change, it might be a breaking functional change — prefer adding new tests over modifying existing ones.

## Member references — use `nameof`, not strings

Hard-coded strings that refer to members are brittle: they don't move when the member is renamed, they don't surface in find-references, and they silently rot.

```csharp
// Avoid
Assert.That(result.TestCaseCount, Is.EqualTo(2));
Assert.That(context.CurrentTest.Name, Is.EqualTo("OneTimeSetup"));

// Prefer
Assert.That(result.TestCaseCount, Is.EqualTo(fixture.TestCaseCount));
Assert.That(context.CurrentTest.Name, Is.EqualTo(nameof(AsyncDisposableFixture.OneTimeSetup)));
```

## Parameterisation — small fixed sets vs cartesian products

Use the framework's row-style attribute (e.g. `[TestCase]`, `[InlineData]`, `[DataRow]`) when you have a small fixed set of (input, expected) tuples. Use a values-per-parameter facility (e.g. `[Values]`, `MemberData`, `DynamicData`) when you want the cartesian product of several parameter values.

Avoid framework-specific facilities that trigger broad engine-side scanning across discovered data sources unless you actually need their behaviour — they make the test less obvious to a reader and slow discovery down.

## Timing-sensitive tests — explicit, not flaky

Tests that depend on wall-clock timing (`Thread.Sleep`, `Task.Delay`, polling intervals) are unreliable on CI: GitHub runners can be slow and heterogeneous. If a test is timing-sensitive:

- Prefer rewriting to avoid wall-clock dependence (inject a virtual clock, assert on call counts, etc.).
- If you genuinely need wall-clock behaviour, mark the test with whatever opt-in mechanism your test framework provides for explicit/manual runs. It will still run locally and in targeted runs, but won't flake the main CI.

## Platform-specific tests — gate them

For features that only make sense on one OS (long paths, Windows registry, etc.), gate the test using the framework's platform attribute or a runtime check:

```csharp
// Runtime check works in any framework
if (!OperatingSystem.IsWindows())
    Assert.Ignore("Windows-only");
```

Don't rely on `#if NETFRAMEWORK` alone — Mono runs `NETFRAMEWORK` binaries on Linux and macOS.

## Test helpers inside a fixture — keep them non-discoverable

Most .NET test frameworks discover public methods in a test class as tests. A non-test helper you call from a test must be `private` (or `internal`), or decorated with whatever the framework provides to exclude it from discovery. A `public` helper without a `[Test]`/`[Fact]` attribute can still trigger discovery analyzers or get accidentally listed.

```csharp
public class MyFixture
{
    [Test]
    public void UsesHelper()
    {
        Assert.That(BuildInput(), Is.EqualTo("abc"));
    }

    // Private: not discovered as a test
    private string BuildInput() => "abc";
}
```

If the helper needs to be accessible from another test file, use `internal` plus `InternalsVisibleTo`, not `public`.

## Test data on the fixture — keep it private

Data-source members (`[TestCaseSource]`, `[MemberData]`, `[ClassData]`, etc.) used within a single fixture should be `private static` (or `private`). Public source members are a common accidental test-discovery trigger.

```csharp
private static IEnumerable<int> Primes => new[] { 2, 3, 5, 7, 11 };

[TestCaseSource(nameof(Primes))]
public void IsPrime(int n) { ... }
```

## Double-disposal, null, empty — edge cases reviewers ask about

When adding tests for a type that owns a resource or a collection, cover:

- **Double-dispose**: calling `Dispose()` twice shouldn't throw.
- **Null / empty inputs**: what does the method return or throw?
- **Boundary sizes**: tuples > 7 elements, collections > typical stackalloc thresholds, deeply nested inputs.
- **Exception paths**: property getters that throw, converters that fail partway through.

## Regression tests cite the issue

A bug-fix PR's test should reproduce the original symptom. Reference the issue in the test name or comment so the intent is clear:

```csharp
[Test]
public void TearDownOutputIsPreserved_Issue4598()
{
    // Regression: output written during TearDown used to be lost.
    ...
}
```
