# Receiver precedence

Read this when a fixer builds `receiver.Method(...)`, `receiver.Property` or `receiver[index]` from an
expression taken from user code.

## Which receivers are safe as they are

Member access binds tighter than every operator except other primary expressions. These are safe to put
`.` after without parentheses:

| Syntax type | Example |
|---|---|
| `SimpleNameSyntax` (`IdentifierNameSyntax`, `GenericNameSyntax`) | `x`, `List<int>` |
| `MemberAccessExpressionSyntax` | `a.B` |
| `InvocationExpressionSyntax` | `a.B()` |
| `ElementAccessExpressionSyntax` | `a[0]` |
| `ParenthesizedExpressionSyntax` | `(a + b)` |
| `LiteralExpressionSyntax` | `1`, `"s"` |
| `ThisExpressionSyntax`, `BaseExpressionSyntax` | `this` |
| `PredefinedTypeSyntax` | `string` |
| `TypeOfExpressionSyntax` | `typeof(T)` |

Everything else needs wrapping. Use an allow-list like this, not a deny-list: a deny-list misses the
next syntax kind the language adds.

## Shapes that go wrong, and how

| Receiver | Without parentheses | Result |
|---|---|---|
| `await t` | `await t.ShouldBe(1)` | CS1929: binds to the `Task` |
| `a?.B` | `a?.B.ShouldBe(1)` | Compiles. When `a` is null the chain short-circuits and nothing is asserted |
| `a ?? b` | `a ?? b.ShouldBe(1)` | Binds to `b`. A `void` assertion makes it a compile error; a `.Length` or `.Count` compiles and reads `b` only |
| `a + b` | `a + b.Length` | Binds to `b` |
| `c ? a : b` | `c ? a : b.Length` | Binds to `b` |
| `(T)x` | `(T)x.Length` | Casts the result, not `x` |
| `!x`, `-x` | `-x.Length` | Applies the operator to the result |
| `x!` | `x!.ShouldBe(1)` | Fine: postfix `!` binds like a primary expression. Wrapping it is harmless |

When the chain ends in a `void` assertion (`ShouldBe`), most of these become compile errors, which is
the lucky case. The `?.` row is the exception: it compiles, passes, and asserts nothing when the receiver
is null. A fix that adds a member returning a value (`.Length`, `.Count`, an indexer) and stops there
would compile and quietly read the wrong thing.

## Post-processing step

Apply it once to the finished call rather than in each branch:

```csharp
private static ExpressionSyntax ParenthesiseReceiver(ExpressionSyntax converted) =>
    converted is InvocationExpressionSyntax call &&
    call.Expression is MemberAccessExpressionSyntax access
        ? call.WithExpression(access.WithExpression(ParenthesiseIfNeeded(access.Expression)))
        : converted;

private static ExpressionSyntax ParenthesiseIfNeeded(ExpressionSyntax e) =>
    NeedsParentheses(e)
        ? SyntaxFactory.ParenthesizedExpression(e.WithoutTrivia()).WithTriviaFrom(e)
        : e;
```

The same helper applies when the fixer adds `.Count`, `.Length` or an indexer to a receiver:
`(a ?? "").Length.ShouldBeLessThan(4)`.

Another option is to always wrap and add `Simplifier.Annotation`: the simplifier removes the
parentheses that aren't needed. `CodeAction`'s default post-processing runs the simplifier on annotated
nodes, so this works wherever the action is applied (IDE, test harness, `dotnet format`). Check the
expected output in your tests either way.

Source: Shouldly.FromAssert #10, #14, #22.
