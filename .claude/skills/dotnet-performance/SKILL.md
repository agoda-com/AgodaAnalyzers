---
name: dotnet-performance
description: Use when modifying hot paths in a .NET library — equality/comparison, value formatting, reflection-based dispatch, collection inspection, or anything that runs in tight loops or per-call on a popular API. Covers allocation patterns, reflection caching, and how to back performance claims with evidence.
---

# .NET performance

Library hot paths run on every call from every consumer. A 10 µs regression in a core method is invisible in a single call and a wall in a 100k-call workload. The conventions below exist because .NET libraries have been bitten by each of them.

## Cache reflection results per `Type`

`Type.GetProperties()`, `GetMethods()`, `GetInterfaces()`, `GetCustomAttributes()` are not free — they walk the type, allocate arrays, and on .NET Framework hit a backing dictionary lookup. Inside hot paths, cache the result keyed by `Type`.

```csharp
// Avoid — runs reflection on every call
private static IEnumerable<PropertyInfo> InterestingProperties(Type type)
    => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
           .Where(p => p.GetIndexParameters().Length == 0);

// Prefer — pay once per Type, then dictionary lookup
private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

private static PropertyInfo[] InterestingProperties(Type type)
    => _propertyCache.GetOrAdd(type, static t =>
        t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
         .Where(p => p.GetIndexParameters().Length == 0)
         .ToArray());
```

Use a `static` field so the cache is per-process, not per-instance. Use `ConcurrentDictionary` when the hot path may run on multiple threads.

## Allocate lazily

Don't allocate state up-front for the failure path. Many calls succeed; only the failing path needs the diagnostic state.

```csharp
// Avoid — allocates on every call, used on the rare failure
private List<object> _missingItems = new();

public bool ApplyTo(IEnumerable expected, IEnumerable actual)
{
    _missingItems = new List<object>();   // allocates again
    ...
}

// Prefer — null until needed, materialise on miss
private List<object>? _missingItems;

public bool ApplyTo(IEnumerable expected, IEnumerable actual)
{
    foreach (var item in expected)
    {
        if (!Contains(actual, item))
            (_missingItems ??= new List<object>()).Add(item);
    }
    ...
}
```

Same pattern for diagnostic strings, exception messages, and per-call scratch buffers.

## Prefer direct iteration over LINQ in hot code

LINQ is fine for clarity in setup code, tests, and one-shot helpers. In tight loops, comparison paths, or equality checks it allocates iterators, closures, and lambdas. Use `for`/`foreach` directly.

```csharp
// Avoid in hot code
return delegates.GetInvocationList().FirstOrDefault(d => d.Method == method) is not null;

// Prefer in hot code
foreach (var d in delegates.GetInvocationList())
{
    if (d.Method == method) return true;
}
return false;
```

Note: `Delegate.GetInvocationList()` itself returns an array — iterate it directly, don't wrap it in `.ForEach(...)` extension methods (which are 3× slower than `foreach`).

## `stackalloc` for small buffers, `ArrayPool` for larger ones

For temporary byte/char/int buffers in hot code:

- **≤ ~1 KB** (rule of thumb): `stackalloc` the buffer. No GC pressure, no rental cost.
- **> 1 KB or size unknown at compile time**: rent from `ArrayPool<T>.Shared` and return it in a `try`/`finally`.
- **Tiny and bounded** (a few elements): a plain stack `Span<T>` over a stackalloc is usually fastest.

```csharp
// stackalloc for small fixed sizes
Span<byte> buffer = stackalloc byte[64];

// ArrayPool for larger or variable
var rented = ArrayPool<byte>.Shared.Rent(size);
try
{
    var span = rented.AsSpan(0, size);
    ...
}
finally
{
    ArrayPool<byte>.Shared.Return(rented);
}
```

Don't `new byte[size]` in a hot path unless the buffer escapes.

## Avoid boxing on value types

`object.Equals`, `IComparable.CompareTo` non-generic forms, and `IEnumerable` (non-generic) box every value-type element. In equality / comparison paths, prefer the generic interfaces (`IEquatable<T>`, `IComparable<T>`, `IEnumerable<T>`) and constrain or check for them at the entry point.

```csharp
// Avoid — boxes on every comparison
if (((IComparable)x).CompareTo(y) != 0) ...

// Prefer — when T is known
if (EqualityComparer<T>.Default.Equals(x, y)) ...

// Or — type-test the generic interface once, dispatch
if (x is IEquatable<T> eq) return eq.Equals((T)y);
```

For collection comparison, `EqualityComparer<T>.Default` and `Comparer<T>.Default` cache the per-`T` comparer instance; use them rather than constructing one per call.

## Don't allocate strings you don't use

Diagnostic message construction is a common offender. The check succeeds 99% of the time — don't pay for the message until you know it's needed.

```csharp
// Avoid — formats on every call
var message = $"Expected {expected} but was {actual}";
if (!matches)
    throw new InvalidOperationException(message);

// Prefer — format only on the failure path
if (!matches)
    throw new InvalidOperationException($"Expected {expected} but was {actual}");
```

Same for `string.Format`, interpolated strings stored in a variable, and concatenation.

## Beware of hidden params allocations

If you are designing internal helper methods for diagnostics, avoid `params object[]` arrays if the method is called on the happy path. Provide explicit overloads for 1, 2, or 3 arguments to avoid the array allocation, or ensure the formatting method is only invoked after the check has already failed.

## Analyzer callbacks run on every keystroke

Roslyn analyzer callbacks (the methods registered via `RegisterSyntaxNodeAction`, `RegisterSymbolAction`, etc.) execute every time the user types. The cost compounds across every project the analyzer ships to. Tight rules apply on top of the general guidance above:

- **Hoist all literal allocations.** Any `new[] { ... }`, `Regex`, dictionary literal, or string array used by the callback belongs in a `private static readonly` field, not constructed inline.
- **Cheap-out early.** Do the cheapest possible discriminator first — a string/identifier comparison on the syntax node before touching the semantic model. `SemanticModel.GetSymbolInfo` is *not* free.
- **Filter at registration, not in the callback.** `RegisterSyntaxNodeAction(handler, SyntaxKind.InvocationExpression)` is cheaper than registering for everything and filtering inside `handler`.
- **Avoid `DescendantNodes()` over the whole method.** When you need to look at neighbours of the current node, walk the containing block/single ancestor — `DescendantNodes().OfType<InvocationExpressionSyntax>()` over an entire method body becomes O(N²) when the analyzer fires multiple times per method.
- **Prefer `SymbolEqualityComparer` over `ToDisplayString()` comparisons.** Stringifying a symbol per callback is wasteful; use the comparer or compare `ISymbol` references directly.

```csharp
// Avoid — allocates per call, semantic model first, walks whole method
private static void Analyze(SyntaxNodeAnalysisContext ctx)
{
    var sep = new[] { '-', '/' };
    var symbol = ctx.SemanticModel.GetSymbolInfo(ctx.Node).Symbol;
    var allInvocations = ctx.Node.Ancestors().OfType<MethodDeclarationSyntax>()
        .First().DescendantNodes().OfType<InvocationExpressionSyntax>();
    ...
}

// Prefer — hoisted, cheap check first, scoped walk
private static readonly char[] Separators = { '-', '/' };

private static void Analyze(SyntaxNodeAnalysisContext ctx)
{
    if (ctx.Node is not InvocationExpressionSyntax invocation) return;
    if (invocation.Expression is not MemberAccessExpressionSyntax member) return;
    if (member.Name.Identifier.ValueText != "Screenshot") return;          // cheap

    var symbol = ctx.SemanticModel.GetSymbolInfo(invocation).Symbol;       // expensive, gated
    ...
}
```

## Back performance claims with a benchmark

Performance changes should come with evidence. For non-trivial perf PRs, include a `BenchmarkDotNet` benchmark showing the before/after numbers.

Agent Action: When proposing a performance change, explicitly outline the benchmark setup. Define a repeatable baseline, isolate the target endpoint or method, and structure a re-measure loop that tests against distinct data profiles: an empty input, a single element, a 100-element input, and a high-cardinality 10k-element input. "I think this should be faster" is unacceptable; provide the deterministic metrics.
