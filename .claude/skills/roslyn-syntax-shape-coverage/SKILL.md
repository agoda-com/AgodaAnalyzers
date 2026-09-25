---
name: roslyn-syntax-shape-coverage
description: Use when a Roslyn analyzer inspects an argument, expression or literal - hardcoded values, string templates, constant arguments, receivers of a call - and must decide which syntactic forms count. Covers literal vs const vs binary vs parenthesised expressions, locals/fields/parameters, await, ?., !, casts, interpolated vs concatenated vs string.Format vs verbatim vs raw strings, named arguments and prefix arguments (Exception, EventId, LogLevel, LogEventLevel), every overload of the target API, and using IOperation (IInvocationOperation, IArgumentOperation, ConstantValue) to collapse shapes. Trigger on - LiteralExpressionSyntax, NumericLiteralExpression, StringLiteralExpression, InterpolatedStringExpression, BinaryExpressionSyntax, ArgumentList.Arguments[0], IsKind(SyntaxKind...).
---

# Syntax shape coverage

The same value can be written many ways. A rule that checks one `SyntaxKind` misses the rest (false negatives) or catches the wrong one (false positives). Before writing the check, **list the equivalent forms and decide which are in scope**. Put that list in the PR and the rule doc.

## 1. Prefer `IOperation` when it collapses shapes

```csharp
// Don't: AG0052 walks the syntax by hand. Misses `const int Delay = 1000; Task.Delay(Delay)`,
// `Task.Delay(-1)` (unary minus), casts, and any shape the recursion forgot.
static bool IsConstantNumericExpression(ExpressionSyntax e) =>
    e is LiteralExpressionSyntax l && l.IsKind(SyntaxKind.NumericLiteralExpression)
    || e is BinaryExpressionSyntax b && IsConstantNumericExpression(b.Left) && IsConstantNumericExpression(b.Right)
    || e is ParenthesizedExpressionSyntax p && IsConstantNumericExpression(p.Expression);

// Do: one check covers literals, consts, constant expressions, unary minus and constant casts.
var argument = ((IInvocationOperation)ctx.Operation).Arguments[0];
if (argument.Value.ConstantValue.HasValue) Report(...);
```

`IInvocationOperation.Arguments` is in **parameter order**, with named arguments and defaults already matched to parameters. Use `argument.Parameter.Name` or `.Ordinal`, never the syntax position.

Evidence: AA #229 (`Task.Delay(5 * 200)` is a `BinaryExpression`, not a `NumericLiteralExpression`).

## 2. The shape list

Go through each row. For each one, the rule either handles it (test) or leaves it out on purpose (negative test + doc line).

| Axis | Forms |
|---|---|
| Constants | literal `1000`, `const` local/field, `5 * 200`, `(1000)`, `-1`, `(int)1.5`, `TimeSpan.FromSeconds(1)` |
| Data flow | local `var d = 1000;`, field, parameter, property. Usually out of scope; say so |
| Wrappers | `await x`, `x?.Y`, `x!`, `(T)x`, `x as T`, `x ?? y`, `cond ? a : b` |
| Strings | `"a" + b`, `$"{b}"`, `$@"..."`, `@"..."`, `"""raw"""`, `$"""{b}"""`, `string.Format(...)`, `string.Concat(...)`, `nameof(...)` |
| Argument position | positional, named (`message: ...`), out of order (`args: x, message: y`) |
| Prefix arguments | `Exception`, `EventId`, `LogLevel`, `LogEventLevel` before the template (AA #246) |
| Overloads | every overload and extension of the target API, including `params` and generic forms |

## 3. Strings: interpolation and concatenation are different kinds

```csharp
// Don't: catches $"..." only
if (arg.Expression.IsKind(SyntaxKind.InterpolatedStringExpression)) Report(...);

// Do: name every form you mean and test each one
static bool IsBuiltString(IOperation value) => !value.ConstantValue.HasValue && value switch
{
    IInterpolatedStringOperation => true,                                          // $"{x}", $@"...", $"""{x}"""
    IBinaryOperation { OperatorKind: BinaryOperatorKind.Add, Type.SpecialType: SpecialType.System_String } => true,  // "a" + x
    IInvocationOperation { TargetMethod: { Name: "Format", ContainingType.SpecialType: SpecialType.System_String } } => true,
    _ => false,
};
```

`"a" + "b"` and `$"a{Const}"` (interpolating a `const string`) are constants: `ConstantValue.HasValue` is `true`, so the guard above skips them. Decide on purpose whether that's right for your rule.

Evidence: AA #184 (AG0041 interpolated/concatenated forms), #246 (verbatim and `LogLevel`-first forms missed).

## 4. Find the argument by parameter, not by index

```csharp
// Don't: syntax (InvocationExpressionSyntax), and assumes the template is always first.
// Misses logger.LogError(ex, "...") and logger.Log(LogLevel.Warning, "...").
var template = invocationSyntax.ArgumentList.Arguments[0];

// Do: operation (IInvocationOperation), and find the argument by the parameter it binds to
var template = invocationOp.Arguments.FirstOrDefault(a => a.Parameter?.Name is "message" or "messageTemplate");
```

Evidence: AA #246 (AG0041 missed exception-first and `LogLevel`-first calls; the fix matches `Exception`, `EventId`, `LogLevel` and `LogEventLevel` prefixes by type).

## 5. Receivers and wrappers

A receiver can be wrapped. Unwrap on purpose, and only as far as the rule means:

```csharp
static IOperation Unwrap(IOperation op) =>
    op is IConversionOperation { IsImplicit: true } c ? Unwrap(c.Operand) : op;
```

C# parentheses don't produce an operation node, so `IOperation` already sees `(x)` as `x`. In syntax you have to strip `ParenthesizedExpressionSyntax` yourself.

`await`, `?.` and `!` change meaning, so don't blindly unwrap them. `x?.Y.Assert()` doesn't run when `x` is null (SFA #22). If the rule treats them like the bare form, say why in a comment and test it.

## Checklist

- [ ] Shape list written in the PR, each row marked in scope / out of scope.
- [ ] `IOperation` + `ConstantValue` used where it replaces hand-written syntax recursion.
- [ ] Arguments found by `IArgumentOperation.Parameter`, not position.
- [ ] Every overload of the target API has a test (including prefix-argument overloads).
- [ ] Out-of-scope shapes have a negative test and a line in the rule doc's scope section.

Related: `roslyn-symbol-based-detection` (which method), `roslyn-analyzer-scope` (where), `roslyn-analyzer-performance` (cost of binding).
