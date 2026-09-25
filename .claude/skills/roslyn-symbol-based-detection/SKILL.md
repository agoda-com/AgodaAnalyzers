---
name: roslyn-symbol-based-detection
description: Use when writing or changing how a Roslyn analyzer decides that a syntax node is the API it cares about. Covers resolving invocations with the SemanticModel, GetSymbolInfo and CandidateSymbols, Compilation.GetTypeByMetadataName in RegisterCompilationStartAction, SymbolEqualityComparer, ContainingType and OriginalDefinition checks, matching a family of logger/assertion APIs by declaring type, and testing name forms (using static, aliases, global using, fully-qualified, generic methods, look-alike user types). Trigger on - DiagnosticAnalyzer, IMethodSymbol, INamedTypeSymbol, ToDisplayString() comparisons, Identifier.ValueText == "SomeMethod", name prefix checks such as StartsWith("Assert").
---

# Symbol-based detection

An analyzer that matches on text fires on look-alikes and misses renamed or re-imported forms. Match on the **symbol**.

## 1. Resolve the invocation, compare the containing type

Resolve well-known types once per compilation. If the library isn't referenced, register nothing.

```csharp
// Don't: text match. Fires on any user method called AssertSomething, misses `using static`.
if (invocation.Expression.ToString().StartsWith("Assert")) { ... }

// Do
public override void Initialize(AnalysisContext context)
{
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();
    context.RegisterCompilationStartAction(start =>
    {
        var assertType = start.Compilation.GetTypeByMetadataName("NUnit.Framework.Assert");
        if (assertType is null) return;   // not referenced: nothing to analyze

        start.RegisterOperationAction(ctx =>
        {
            var method = ((IInvocationOperation)ctx.Operation).TargetMethod;
            if (!SymbolEqualityComparer.Default.Equals(method.ContainingType, assertType)) return;
            ...
        }, OperationKind.Invocation);
    });
}
```

`GetTypeByMetadataName` returns `null` when the type is missing *or* ambiguous (defined in two referenced assemblies). Treat both as "don't analyze". Evidence: Shouldly.FromAssert #18/#19 (any method named `Assert*` was flagged).

## 2. Compare symbols with `SymbolEqualityComparer`, not strings

```csharp
// Don't
method.ContainingType.ToDisplayString() == "Serilog.ILogger"
// Do
SymbolEqualityComparer.Default.Equals(method.ContainingType.OriginalDefinition, loggerType)
```

- Generic types: compare `OriginalDefinition` (`List<int>` vs `List<T>`).
- Extension methods called as instance methods: use `method.ReducedFrom ?? method` to get the static declaration.
- Generic methods: `method.OriginalDefinition` / `ConstructedFrom`. A syntax-name check misses these entirely: AG0012 only looked at `IdentifierNameSyntax`, so `result.ShouldBeOfType<T>()` (a `GenericNameSyntax`) didn't count as an assertion (AA #216).

Older helpers that compare `ToDisplayString()` results (AgodaAnalyzers' `InvocationRule`, `TestMethodHelpers`) aren't a model for new rules. Moving them over is a separate PR.

## 3. Fall back to `CandidateSymbols`

Code being edited often doesn't bind. `GetSymbolInfo(node).Symbol` is `null` when overload resolution fails, but `CandidateSymbols` still says which method group it was.

```csharp
var info = ctx.SemanticModel.GetSymbolInfo(invocation, ctx.CancellationToken);
var method = info.Symbol as IMethodSymbol
    ?? info.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
```

Decide per rule whether a candidate-only match is enough to report. For a ban ("never call X") it usually is; for a rule that inspects argument types it usually isn't.

## 4. A family of APIs: match the declaring type, not the receiver

Don't require a particular receiver kind. How the call is written doesn't change which API it is.

```csharp
// Don't: AG0041 before #246 only looked at field receivers,
// so static Log.Information(...), locals, properties and parameters were all missed.
if (ctx.SemanticModel.GetSymbolInfo(memberAccess.Expression).Symbol is not IFieldSymbol) return;

// Do: ask what the *method* belongs to (plus types that implement the interface)
static bool IsLoggerMethod(IMethodSymbol m, INamedTypeSymbol iLogger) =>
    SymbolEqualityComparer.Default.Equals(m.ContainingType, iLogger)
    || m.ContainingType.AllInterfaces.Contains(iLogger, SymbolEqualityComparer.Default);
```

Evidence: AA #246.

## 5. Library recognition: namespace + method allow-list

When one type mixes relevant and irrelevant methods, recognise the library by namespace or containing type **and** keep an allow-list of method names.

```csharp
// NSubstitute: Received() is an assertion, Returns() is setup. Same namespace.
private static readonly ImmutableHashSet<string> NSubstituteAssertions =
    ImmutableHashSet.Create("Received", "DidNotReceive", "ReceivedWithAnyArgs", "DidNotReceiveWithAnyArgs");
```

Evidence: AA #233 (AG0012 did not recognise Shouldly, NSubstitute or FluentAssertions assertions).

## 6. Test the name forms text matching gets wrong

Each is a separate test case. If one is out of scope, add a negative test and say so in the rule doc.

| Form | Example |
|---|---|
| `using static` | `using static NUnit.Framework.Assert; ... That(x, Is.True);` |
| alias | `using A = NUnit.Framework.Assert; A.That(...)` |
| `global using` | in a separate syntax tree of the test compilation |
| fully-qualified | `global::NUnit.Framework.Assert.That(...)` |
| generic method | `x.ShouldBeOfType<Foo>()` (AA #216) |
| look-alike user type | a user class `Assert` in the project's own namespace (SFA #19) |
| extension vs static call | `logger.LogInformation(...)` and `LoggerExtensions.LogInformation(logger, ...)` |

## Checklist

- [ ] Target types resolved once in `RegisterCompilationStartAction`; nothing registered when they're `null`.
- [ ] No `ToString()`, `ToDisplayString()` or `StartsWith` on names to decide *which API* a node is.
- [ ] `SymbolEqualityComparer.Default` with `OriginalDefinition` / `ReducedFrom` where generics or extensions are possible.
- [ ] `CandidateSymbols` handled on purpose.
- [ ] Tests cover every row of the name-form table, or document it as out of scope.

Related: `roslyn-analyzer-scope` (where to fire), `roslyn-syntax-shape-coverage` (which argument forms), `roslyn-analyzer-performance` (cheap name check before binding).
