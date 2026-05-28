---
name: test-context-detection
description: Use when an analyzer's logic includes "only fire inside test code." Prevents the long-running trap in this repo where test detection only checks class-level fixture attributes and silently skips real violations in valid test classes that don't have a fixture attribute (or fires on production classes whose names happen to start with `Test`).
---

# Test context detection

Multiple analyzers in this repo have shipped, then been corrected, because their "is this test code?" check missed cases. The trap is checking *one* signal (class attribute, or namespace prefix) when several signals can independently mark code as test code, and any one of them is enough.

## Canonical check

A class is a test class if **any** of these holds. Treat them as an OR, not an AND:

1. The class or a base class carries `[TestFixture]` or `[TestClass]`.
2. **Any method in the class** carries `[Test]`, `[Fact]`, `[Theory]`, `[TestMethod]`, or `[TestCase]`.
3. The containing namespace segment matches `^Test$|Tests?$` — i.e. ends in `Test` or `Tests` exactly, **not** prefix-matches against arbitrary words.

A method is a test method if it carries one of the test attributes above directly, or sits inside a test class.

## Why class-level attributes alone are not enough

`[TestFixture]` is optional in current versions of common test frameworks — a class with only test-attributed methods discovers as a test class without any class-level marker. Checking only the class attribute means real violations in valid test classes get silently skipped.

```csharp
// This is a test class even though it has no [TestFixture] attribute.
// A check that only inspects class-level attributes will misclassify it as production.
public class MyServiceTests
{
    [Test]
    public void ItDoesTheThing() { ... }
}
```

## Why namespace prefix matching is dangerous

`TestimonialService`, `ContestEntry`, `LatestPrice` — all of these contain the substring "Test" or start with "Test" but are not test code. Match on namespace **segments**, anchored:

```csharp
// Avoid — matches "TestimonialService.Reviews"
isTestNs = ns.StartsWith("Test", StringComparison.Ordinal);

// Prefer — matches exactly "Tests" or "Test" as a namespace segment
isTestNs = ns.Split('.').Any(seg =>
    seg.Equals("Tests", StringComparison.Ordinal) ||
    seg.Equals("Test", StringComparison.Ordinal));
```

If your repo has a `Tests/` project folder convention reflected in the assembly name, you can also check `Compilation.AssemblyName` for a `.Tests` suffix as a coarse pre-filter.

## Recommended helper

Centralise this in `src/Agoda.Analyzers/Helpers/` so every analyzer that needs it calls one canonical implementation. Don't reimplement it inside each `AnalyzeNode`.

```csharp
internal static class TestContextHelpers
{
    private static readonly string[] TestMethodAttributeNames =
    {
        "Test", "TestAttribute",
        "Fact", "FactAttribute",
        "Theory", "TheoryAttribute",
        "TestMethod", "TestMethodAttribute",
        "TestCase", "TestCaseAttribute",
    };

    private static readonly string[] TestClassAttributeNames =
    {
        "TestFixture", "TestFixtureAttribute",
        "TestClass", "TestClassAttribute",
    };

    public static bool IsInTestContext(SyntaxNodeAnalysisContext ctx)
    {
        var containingType = ctx.Node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
        if (containingType is null) return false;

        if (HasAnyAttribute(containingType.AttributeLists, TestClassAttributeNames))
            return true;

        // Any method inside the class with a test attribute => test class
        foreach (var member in containingType.Members.OfType<MethodDeclarationSyntax>())
        {
            if (HasAnyAttribute(member.AttributeLists, TestMethodAttributeNames))
                return true;
        }

        // Namespace segment match
        var ns = containingType.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>()?.Name.ToString();
        if (ns is not null && ns.Split('.').Any(seg => seg is "Tests" or "Test"))
            return true;

        return false;
    }

    private static bool HasAnyAttribute(SyntaxList<AttributeListSyntax> lists, string[] names)
    {
        foreach (var list in lists)
            foreach (var attr in list.Attributes)
                if (names.Contains(attr.Name.ToString().Split('.').Last()))
                    return true;
        return false;
    }
}
```

For higher accuracy, resolve attribute identities via the semantic model (`ctx.SemanticModel.GetSymbolInfo(attr).Symbol`) instead of name-matching strings — name-matching can be fooled by user-defined attributes that happen to share a name. The trade-off is performance (see [[dotnet-performance]] — semantic-model calls are expensive in hot paths). Choose based on whether the analyzer fires often enough to matter.

## Document what's covered

Every test-only rule's documentation must say which test frameworks are recognised. Reviewers consistently ask for this disclosure when it's missing — pre-empt it by listing the supported attribute set in the rule's `.md`/`.html` doc.

## Negative test cases

For each detection signal, add a negative test:

- A class named `TestimonialService` — must **not** be flagged.
- A namespace `Latest.Pricing` — must **not** be flagged.
- A class with `[TestFixture]` and a violation in production-shaped code inside it — **must** be flagged (proves the fixture-attribute path works).
- A class with no class-level attribute but a method with `[Test]` and a violation — **must** be flagged (proves the method-attribute path works).
- A class in a `Tests` namespace with no test attributes at all (e.g. test utilities) — flag according to your rule's intent; document the decision either way.

See [[analyzer-test-coverage-matrix]] for how to structure the full coverage table.
