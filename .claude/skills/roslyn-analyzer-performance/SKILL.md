---
name: roslyn-analyzer-performance
description: Use when writing or changing any Roslyn analyzer callback - code that runs on every keystroke in the IDE and on every build. Covers filtering at registration (RegisterSyntaxNodeAction with SyntaxKind, RegisterOperationAction with OperationKind), cheap syntactic checks before SemanticModel calls, resolving types once in RegisterCompilationStartAction, static readonly arrays/sets/regexes, allocating only on the diagnostic path, avoiding LINQ and DescendantNodes() over whole methods, EnableConcurrentExecution with no mutable analyzer state, and measuring with ReportAnalyzer. Trigger on - Initialize(AnalysisContext), SemanticModel.GetSymbolInfo, GetTypeByMetadataName, DescendantNodes, new[] { ... } or new Regex inside a callback, fields on a DiagnosticAnalyzer class.
---

# Analyzer performance

Analyzer callbacks run on every keystroke, in every project that references the package. A small cost per node adds up across a solution. Adapted from NUnit's performance and threading skills; only the parts that apply to analyzers are kept.

## 1. Filter at registration

```csharp
// Don't: called for every node, filtered inside
context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression, SyntaxKind.ObjectCreationExpression, ...);
private static void Analyze(SyntaxNodeAnalysisContext ctx) { if (ctx.Node is not InvocationExpressionSyntax) return; ... }

// Do: only the kinds you handle, one callback per kind
context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
```

`RegisterSymbolAction(..., SymbolKind.NamedType)` for rules about declarations (AG0054) is cheaper than walking syntax.

## 2. Cheap syntax check before the semantic model

`GetSymbolInfo` binds the expression. A name comparison is a string compare. Do the cheap one first.

```csharp
// Do
var invocation = (InvocationExpressionSyntax)ctx.Node;
if (GetMethodName(invocation) is not ("ScreenshotAsync" or "Screenshot")) return;   // cheap
if (ctx.SemanticModel.GetSymbolInfo(invocation, ctx.CancellationToken).Symbol   // bound only now
        is not IMethodSymbol method) return;
```

The name is a filter, not the decision: the symbol check still decides (`roslyn-symbol-based-detection`). With `RegisterOperationAction` the node is already bound, so check `TargetMethod.Name` first and the containing type second.

## 3. Resolve types once per compilation

```csharp
// Don't: a lookup per node
var loggerType = ctx.Compilation.GetTypeByMetadataName("Microsoft.Extensions.Logging.ILogger");

// Do
context.RegisterCompilationStartAction(start =>
{
    var loggerType = start.Compilation.GetTypeByMetadataName("Microsoft.Extensions.Logging.ILogger");
    if (loggerType is null) return;
    start.RegisterOperationAction(ctx => Analyze(ctx, loggerType), OperationKind.Invocation);
});
```

## 4. Allocate once, and only on the diagnostic path

```csharp
// Don't: allocates on every call (AA #228)
var parts = text.Split(new[] { '-', '/' });

// Do
private static readonly char[] DateSeparators = { '-', '/' };
private static readonly Regex XPathPattern = new Regex(@"^//[a-zA-Z]", RegexOptions.Compiled);
```

- `static readonly` for arrays, sets (`ImmutableHashSet`), regexes and `ImmutableDictionary` diagnostic properties.
- Don't build message arguments, `ImmutableDictionary` properties or locations until you're about to report.
- Compare `name.Identifier.ValueText`, not `node.ToString()`: printing a node allocates a new string every time.

## 5. No whole-method walks in the hot path

```csharp
// Don't: every screenshot call scans every invocation in the method. N screenshots → O(N²) (AA #226/#230).
var waits = method.DescendantNodes().OfType<InvocationExpressionSyntax>().Where(IsWait);

// Do: walk back from the node through the statements before it, then up through enclosing blocks.
for (SyntaxNode node = statement; node != null && node != methodBody; node = node.Parent)
{
    if (node.Parent is BlockSyntax block)
    {
        foreach (var previous in block.Statements)
        {
            if (previous == node) break;
            if (ContainsWait(previous)) return true;
        }
    }
}
```

If you need data about the whole method for every node in it, compute it once with `RegisterOperationBlockStartAction` / `RegisterCodeBlockStartAction` and report at block end.

LINQ is fine in setup code and on the diagnostic path. In a callback that runs for every invocation, prefer `foreach` and early return.

## 6. Concurrency: no mutable state on the analyzer

```csharp
context.EnableConcurrentExecution();
context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
```

- The analyzer instance is shared across threads and compilations. No instance or `static` fields that callbacks write to.
- Per-compilation state lives in locals captured by the `RegisterCompilationStartAction` closure. If callbacks add to a collection, use `ConcurrentBag` / `ConcurrentDictionary` and report in `RegisterCompilationEndAction`.
- Pass `ctx.CancellationToken` to every `GetSymbolInfo`, `GetOperation`, `GetTypeInfo` and `GetDeclaredSymbol` call.

## 7. Back claims with numbers

A performance change quotes a before/after:

```bash
dotnet build <real-solution> -p:ReportAnalyzer=true -bl:analyzers.binlog
# open in MSBuild Structured Log Viewer: Analyzer Summary → time per analyzer
```

Run it on a real consuming repo, not the test project. Quote the rule's time and the total analyzer time.

## Checklist

- [ ] Registration filters by `SyntaxKind` / `OperationKind` / `SymbolKind`.
- [ ] Name check before any semantic-model call.
- [ ] Well-known types resolved in `RegisterCompilationStartAction`.
- [ ] No `new[]`, `new Regex`, `new HashSet` inside a callback.
- [ ] No `DescendantNodes()` over a whole method per node.
- [ ] `EnableConcurrentExecution()`, no mutable fields, `CancellationToken` passed through.
- [ ] Perf claims backed by `ReportAnalyzer` numbers.
