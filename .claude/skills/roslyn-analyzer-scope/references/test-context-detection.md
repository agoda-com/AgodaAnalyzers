# Test-context detection helper

A symbol-based version of the check in `roslyn-analyzer-scope`. Resolve the attribute types once per compilation and pass them in, so the per-node cost is an attribute scan and no `GetTypeByMetadataName`.

```csharp
internal sealed class TestAttributes
{
    private readonly ImmutableHashSet<INamedTypeSymbol> _classAttributes;
    private readonly ImmutableHashSet<INamedTypeSymbol> _methodAttributes;

    private TestAttributes(ImmutableHashSet<INamedTypeSymbol> classAttributes, ImmutableHashSet<INamedTypeSymbol> methodAttributes)
    {
        _classAttributes = classAttributes;
        _methodAttributes = methodAttributes;
    }

    private static readonly string[] ClassAttributeNames =
    {
        "NUnit.Framework.TestFixtureAttribute",
        "Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute",
    };

    private static readonly string[] MethodAttributeNames =
    {
        "NUnit.Framework.TestAttribute",
        "NUnit.Framework.TestCaseAttribute",
        "NUnit.Framework.TestCaseSourceAttribute",
        "Xunit.FactAttribute",
        "Xunit.TheoryAttribute",
        "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute",
    };

    // Call from RegisterCompilationStartAction. Unreferenced frameworks resolve to null and drop out.
    public static TestAttributes Create(Compilation compilation) => new TestAttributes(
        Resolve(compilation, ClassAttributeNames),
        Resolve(compilation, MethodAttributeNames));

    public bool IsTestType(INamedTypeSymbol type)
    {
        for (var t = type; t != null; t = t.BaseType)
        {
            if (HasAny(t, _classAttributes)) return true;
        }

        foreach (var member in type.GetMembers())
        {
            if (member is IMethodSymbol method && HasAny(method, _methodAttributes)) return true;
        }

        return HasTestNamespaceSegment(type.ContainingNamespace);
    }

    private static bool HasAny(ISymbol symbol, ImmutableHashSet<INamedTypeSymbol> attributes)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            // Subclasses count: xUnit's [SkippableFact] derives from FactAttribute.
            for (var c = attribute.AttributeClass; c != null; c = c.BaseType)
            {
                if (attributes.Contains(c)) return true;
            }
        }
        return false;
    }

    private static bool HasTestNamespaceSegment(INamespaceSymbol ns)
    {
        for (; ns is { IsGlobalNamespace: false }; ns = ns.ContainingNamespace)
        {
            if (ns.Name.EndsWith("Tests", StringComparison.Ordinal)
                || ns.Name.EndsWith("Test", StringComparison.Ordinal)) return true;
        }
        return false;
    }

    private static ImmutableHashSet<INamedTypeSymbol> Resolve(Compilation compilation, string[] names) =>
        names.Select(compilation.GetTypeByMetadataName)
             .Where(t => t != null)
             .ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
}
```

`HasTestNamespaceSegment` is case-sensitive on purpose: `Contest` ends with `test` but not `Test`. It still matches a segment like `LoadTest`. If that's a problem for a rule, drop the namespace branch and rely on attributes.

## Tests to write

| Case | Expect |
|---|---|
| `[TestFixture]` class, no method attributes | fires |
| no class attribute, one `[Test]` method (NUnit) | fires |
| no class attribute, one `[Fact]` method (xUnit) | fires |
| `[TestClass]` + `[TestMethod]` (MSTest) | fires |
| class inheriting a `[TestFixture]` base | fires |
| production class in namespace `MyApp.Tests.Helpers` | fires (namespace branch) |
| class `TestimonialService` in `MyApp.Services` | does not fire |
| namespace `MyApp.Contest` | does not fire |
