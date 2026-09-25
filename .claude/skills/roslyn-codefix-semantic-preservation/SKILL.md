---
name: roslyn-codefix-semantic-preservation
description: Use when writing or changing a Roslyn CodeFixProvider that rewrites a call to a different API or a different shape (migration fixers such as NUnit to Shouldly, logging template fixers, API replacement fixers). Makes sure the fixed code means the same thing as the original, not just that it compiles. Trigger on CodeFixProvider, RegisterCodeFixesAsync, CodeAction.Create, a switch over method or constraint names that picks a target method, GetSemanticModelAsync inside a fixer, GetSpeculativeSymbolInfo, a new conversion case, or a fixer bug report where the fixed test passes or fails differently.
---

# Code fixes must keep the meaning

A fix that doesn't compile is caught at build time. A fix that compiles but means something else may
never be caught: the test still runs, it just checks less, or the opposite. Three of the five code-fix
bugs shipped between 2024 and 2026 were like this.

The rule: **after the fix, the code must do exactly what it did before.** If the fixer can't be sure, it
leaves the code alone.

## 1. Compare the defaults of both APIs, parameter by parameter

Two methods with the same name in different libraries often have different defaults. Before you add a
conversion, write down each implicit behaviour of the source call and check the target has the same one.
Where the target's default differs, pass the value explicitly.

Things that commonly differ: case sensitivity, culture, ordering (`ignoreOrder`), null handling,
tolerance, and whether a collection or a string overload is chosen.

```csharp
// Don't: NUnit Does.Contain is ordinal, Shouldly's string ShouldContain defaults to Case.Insensitive
Assert.That(greeting, Does.Contain("World"));   →   greeting.ShouldContain("World");

// Do: pass the difference explicitly (string receivers only; the collection overload has no Case)
Assert.That(greeting, Does.Contain("World"));   →   greeting.ShouldContain("World", Case.Sensitive);
```

Shouldly.FromAssert #21.

## 2. Match the whole input, not its last part

A fluent chain (`Is.Not.Empty`, `Has.Count.EqualTo`) or a qualified name is one thing. Matching only its
last member lets a negated or wrapped form fall into the wrong branch.

```csharp
// Don't: `constraint.Name == "Empty"` also matches Is.Not.Empty, and the negation is lost
Assert.That(orders, Is.Not.Empty);   →   orders.ShouldBeEmpty();

// Do: match the full chain (`Is.Empty` exactly, `Is.Not.Empty` separately) and leave any other
// chain ending in `.Empty` unconverted
Assert.That(orders, Is.Not.Empty);   →   orders.ShouldNotBeEmpty();
```

Shouldly.FromAssert #23.

## 3. Take the operand, not the wrapper around it

When two source forms share a `case` (`Assert.Greater(x, y)` and `Assert.That(x, Is.GreaterThan(y))`),
their arguments are in different places. Pull each value out of the form that actually holds it.

```csharp
// Don't: arguments[1] is the whole constraint, so this is CS0411
Assert.That(id, Is.GreaterThan(0));   →   id.ShouldBeGreaterThan(Is.GreaterThan(0));

// Do: read the constraint's own argument
Assert.That(id, Is.GreaterThan(0));   →   id.ShouldBeGreaterThan(0);
```

Shouldly.FromAssert #24.

## 4. Never drop an argument you aren't rewriting

A fixer usually rewrites one argument. Every other argument (an `Exception`, an `EventId`, a
`LogLevel`, a message, a `CancellationToken`) has to be in the output, in the same position.

```csharp
// Don't: rebuild the argument list from the template alone; the exception is gone
_logger.LogError(ex, $"Failed {id}");   →   _logger.LogError("Failed {Id}", id);

// Do: copy the arguments before and after the template, and only replace the template
_logger.LogError(ex, $"Failed {id}");   →   _logger.LogError(ex, "Failed {Id}", id);
```

AgodaAnalyzers #246. Add a test for each argument position the target API allows.

## 5. If you're not sure, don't convert

When a shape isn't recognised, return `null` so the caller keeps the original node (or register no
fix). Never add a `default:` branch that falls back to a "close enough" conversion. Then decide on
purpose whether the analyzer keeps reporting that shape (see `roslyn-codefix-fix-all-and-parity`).
Shouldly.FromAssert #23 and #24 both replaced a guess with "leave it unconverted".

## 6. Check the target overload exists and isn't obsolete

An overload that exists can still be unusable. Shouldly 4.2.1 keeps its `Func<string>` message overloads,
but they're `[Obsolete(error: true)]`, so the fixed code fails to build.

- Check the target library **version** the fixer supports, not just its API surface.
- When the choice depends on how the call binds (positional or named, which overload), build the candidate
  and bind it with `SemanticModel.GetSpeculativeSymbolInfo`. If the positional form doesn't bind, try the
  named one. If neither binds, don't convert.
- Speculative binding doesn't tell you an overload is obsolete: it binds to it without complaint. Check
  `IMethodSymbol.GetAttributes()` for `ObsoleteAttribute`, and compile the fixed output in tests.

```csharp
var positional = call.AddArgumentListArguments(Argument(message));
if (Binds(positional)) return positional;
var named = call.AddArgumentListArguments(Argument(message).WithNameColon(NameColon("customMessage")));
return Binds(named) ? named : null;
```

Shouldly.FromAssert #26.

## 7. Use the semantic model when the right rewrite depends on a type

If the output differs between a `string` and a collection receiver, `bool` and `bool?`, or an array
(`Length`) and a list (`Count`), call `document.GetSemanticModelAsync(ct)` in the fixer. Nodes you've
just built aren't in the tree, so ask about them with `GetSpeculativeTypeInfo(position, node,
SpeculativeBindingOption.BindAsExpression)`, using the original node's `SpanStart` as the position.

Shouldly.FromAssert #21 (string vs collection `ShouldContain`), #26 (`Has.Count` on an array).

## Before you open the PR

- [ ] For each new conversion, the source and target defaults are listed and any difference is passed
      explicitly.
- [ ] Negated and wrapped forms of every matched chain have a test: either converted correctly or left
      alone.
- [ ] Every argument position the source API allows has a test, and none of them is dropped.
- [ ] Fixed output is compiled against the target package version the fixer supports.
- [ ] Where you can, the fixer has been run over a real test suite and pass/fail compared before and after.
      A test that changes result means a conversion changed its meaning.

Related: `roslyn-codefix-syntax-construction` (building the replacement correctly),
`roslyn-codefix-fix-all-and-parity` (what to report when there's no safe fix).
