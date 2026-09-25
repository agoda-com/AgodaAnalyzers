---
name: roslyn-codefix-fix-all-and-parity
description: Use when registering a Roslyn code fix, or when changing what an analyzer reports for a diagnostic that has a fixer. Covers analyzer/fixer parity (don't report what can't be fixed), a shared "is this reportable" predicate, never registering a code action that does nothing, Fix All support and equivalence keys, nested diagnostics, bulk runs with dotnet format analyzers, and what to do when the diagnostic span doesn't resolve. Trigger on RegisterCodeFixesAsync, RegisterCodeFix, CodeAction.Create, equivalenceKey, GetFixAllProvider, WellKnownFixAllProviders.BatchFixer, FixAllProvider, FixableDiagnosticIds, ReportDiagnostic in an analyzer that has a CodeFixProvider, FindNode/FindToken on a diagnostic span, or a report of a warning that can't be cleared.
---

# Fix All and analyzer/fixer parity

An analyzer and its fixer are one feature. People see the warning, press the light bulb or run
`dotnet format`, and expect the warning to go. When they disagree, you get warnings that can never be
cleared, or a light bulb that does nothing.

## 1. Don't report what the fixer can't fix

If the fixer has no conversion for a shape, the analyzer shouldn't report it under the fixer's ID. Either
don't report it at all, or report it under a separate ID with no fixer, so people can turn it off on its
own.

```csharp
// Don't: every Assert.Multiple is reported, but the fixer has no case for it
if (IsNUnitAssert(method)) context.ReportDiagnostic(...);   // 172 warnings nobody can clear

// Do: report only the shapes the fixer can convert
if (IsNUnitAssert(method) && (method.Name != "Multiple" || AssertMultiple.GetConditions(call) != null))
    context.ReportDiagnostic(...);
```

Shouldly.FromAssert #17 (report), #20 (fix). Not every gap needs closing before release, but each one
should be a decision, written in the rule's doc, not an accident.

## 2. Put "is this reportable" in one method both sides call

When the analyzer and the fixer each have their own idea of what's in scope, they drift. Put the
predicate in one `internal static` method on the analyzer, and call it from the fixer too, including for
nested nodes the fixer converts as part of a bigger edit.

```csharp
// Analyzer
internal static bool IsReported(InvocationExpressionSyntax call, SemanticModel model, CancellationToken ct) { ... }

// Fixer: convert an inner call only if the analyzer would have reported it
if (!NUnitToShouldlyAnalyzer.IsReported(inner, model, ct)) return new[] { condition };
```

Shouldly.FromAssert #20.

## 3. Never register a code action that does nothing

If the fixer can't produce a change for this diagnostic, don't call `RegisterCodeFix`. Work out whether a
change is possible **in `RegisterCodeFixesAsync`**, not inside the `createChangedDocument` callback,
where returning the same document looks to the user like a broken light bulb.

```csharp
// Don't: always register; find out later that there's nothing to do
context.RegisterCodeFix(CodeAction.Create(Title, c => FixAsync(doc, span, c), Title), diagnostic);
// ...FixAsync returns `document` unchanged for `using static` calls

// Do: check first
if (FindTemplateArgumentIndex(invocation, span) < 0) return;
context.RegisterCodeFix(CodeAction.Create(Title, c => FixAsync(doc, span, c), Title), diagnostic);
```

Shouldly.FromAssert #19 (`using static` calls got an action that did nothing). If a pre-check would cost
too much, the analyzer should narrow what it reports instead (rule 1).

## 4. If the span doesn't resolve, register nothing and return

When `FindNode` / `FindToken` doesn't give you the node you expect, return without registering. Don't
wrap the lookup in `try/catch`: it hides real bugs and suggests a crash that isn't possible. Guard the
one case that throws (a span outside the tree) with a check.

```csharp
// Don't
try { node = root.FindNode(span); } catch (ArgumentOutOfRangeException) { return; } catch (IndexOutOfRangeException) { return; }

// Do
if (span.End > root.FullSpan.End) return;
if (!(root.FindNode(span) is InvocationExpressionSyntax invocation)) return;
```

AgodaAnalyzers #246. This matters more for fixers that list another analyzer's ID (Sonar, IDE), because
you don't control where that analyzer puts its span.

## 5. Support Fix All, and test the way people really run it

- Return a Fix All provider from `GetFixAllProvider()`. `WellKnownFixAllProviders.BatchFixer` is right
  when each fix edits only its own diagnostic's span. When fixes overlap or depend on each other, write a
  custom `FixAllProvider` or make one fix cover the whole overlapping region (rule 6).
- Give every `CodeAction` an `equivalenceKey`. Fix All uses it to find the same action on every other
  diagnostic, so a missing or per-instance key (one that includes a name or a position) breaks Fix All. If one diagnostic can offer several actions, give each a
  different, stable key.
- **`dotnet format analyzers --diagnostics <ID> --severity info` is the path people actually use** to fix a
  whole repo. It runs the Fix All provider over every document. Try it on a real project before release,
  and check the diff, not just the build. The details are in
  [references/dotnet-format.md](references/dotnet-format.md).

## 6. Nested diagnostics: one edit, same result as one-at-a-time

When diagnostics nest (asserts inside `Assert.Multiple`, calls inside a lambda that's also flagged), the
batch fixer can produce conflicting edits and drop some. Convert the inner nodes as part of the outer
node's fix, in one edit. Then Fix All gives the same code as fixing each diagnostic by hand.

Test it: run the harness's Fix All on a sample with nested diagnostics, and check the output matches the
one-at-a-time result.

Shouldly.FromAssert #20.

## Before you open the PR

- [ ] Every shape the analyzer reports under this ID has a conversion, or the gap is deliberate and
      documented.
- [ ] The analyzer and fixer share one "is this reportable" predicate.
- [ ] `RegisterCodeFixesAsync` returns without registering when there's nothing to do.
- [ ] No `try/catch` around node lookup.
- [ ] `GetFixAllProvider()` is implemented and every action has a stable `equivalenceKey`.
- [ ] Fix All output is tested, including a nested case if diagnostics can nest.
- [ ] `dotnet format analyzers --diagnostics <ID>` has been run on a real project, and the result builds
      and passes the same tests as before.

Related: `roslyn-codefix-semantic-preservation`, `roslyn-codefix-syntax-construction`.
