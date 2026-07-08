---
name: test-context-detection
description: Use when an analyzer must fire only inside test code. Detects test classes/methods correctly via multiple independent signals, avoiding both missed violations in valid test classes and false positives on production names that contain "Test".
---

# Test context detection

"Is this test code?" has several independent signals; any one is sufficient. Checking only one is the trap.

## Canonical check (OR, not AND)

A class is a test class if **any** holds:

1. The class or a base class has `[TestFixture]` or `[TestClass]`.
2. **Any method** in the class has `[Test]`, `[Fact]`, `[Theory]`, `[TestMethod]`, or `[TestCase]`.
3. A containing namespace **segment** equals `Test` or `Tests` (anchored — not a prefix match).

A method is a test method if it has a test attribute directly, or sits in a test class.

## Why each branch matters

- **Class attribute is optional.** A class with only test-attributed methods is still a test class. Checking class-level attributes alone silently skips real violations.
- **Prefix matching is wrong.** `TestimonialService`, `ContestEntry`, `LatestPrice` contain "Test". Match anchored namespace segments:

```csharp
// Wrong — matches "Testimonial..."
ns.StartsWith("Test", StringComparison.Ordinal);
// Right
ns.Split('.').Any(s => s is "Tests" or "Test");
```

## Centralize it

Put one implementation in `src/Agoda.Analyzers/Helpers/`; don't reimplement per analyzer.

```csharp
internal static class TestContextHelpers
{
    private static readonly string[] MethodAttrs =
        { "Test","TestAttribute","Fact","FactAttribute","Theory","TheoryAttribute",
          "TestMethod","TestMethodAttribute","TestCase","TestCaseAttribute" };
    private static readonly string[] ClassAttrs =
        { "TestFixture","TestFixtureAttribute","TestClass","TestClassAttribute" };

    public static bool IsInTestContext(SyntaxNodeAnalysisContext ctx)
    {
        var type = ctx.Node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
        if (type is null) return false;
        if (HasAttr(type.AttributeLists, ClassAttrs)) return true;
        if (type.Members.OfType<MethodDeclarationSyntax>()
                .Any(m => HasAttr(m.AttributeLists, MethodAttrs))) return true;
        var ns = type.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>()?.Name.ToString();
        return ns?.Split('.').Any(s => s is "Tests" or "Test") ?? false;
    }

    private static bool HasAttr(SyntaxList<AttributeListSyntax> lists, string[] names) =>
        lists.SelectMany(l => l.Attributes)
             .Any(a => names.Contains(a.Name.ToString().Split('.').Last()));
}
```

For higher accuracy, resolve attributes via `ctx.SemanticModel.GetSymbolInfo(attr).Symbol` instead of name matching — but that's expensive in hot paths ([[dotnet-performance]]). Choose by how often the analyzer fires.

## Required

- Document which test frameworks are recognized (list the attribute set in the rule doc).
- Negative tests: class named `TestimonialService` (no fire), namespace `Latest.Pricing` (no fire).
- Positive tests: `[TestFixture]`-only path, method-`[Test]`-only path (no class attribute).

See [[analyzer-test-coverage-matrix]] for the full table.
