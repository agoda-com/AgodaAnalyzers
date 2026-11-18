---
name: analyzer-test-coverage-matrix
description: Use when writing or changing tests for a Roslyn diagnostic analyzer. Produces an explicit coverage matrix so negative and edge cases are covered before the PR opens.
---

# Analyzer test coverage matrix

For each positive case, cover the matching negatives and edges along these axes:

- accessibility: `public` / `internal` / `protected` / `private`
- syntactic form: literal, named constant, local variable, parenthesised, binary expression, interpolated string, fully-qualified reference
- generics: generic vs. non-generic
- collection-ness: scalar vs. array vs. `IEnumerable<T>` vs. `IReadOnlyList<T>`
- interface vs. class
- test vs. production context ([[test-context-detection]])

## Write the matrix as a comment at the top of the test file

Forces consideration of each axis and documents coverage. You needn't test the full cartesian product — but each cell is either covered or deliberately skipped (skip recorded in the matrix).

```csharp
// AG0XXX — flags public fields on a class.
// Axis           | Cases                         | Test
// ---------------+-------------------------------+----------------------------
// Accessibility  | public / internal / protected | Public/Internal/Protected_*
//                | private                       | PrivateField_IsAllowed
// Field type     | int / byte[] / string / List  | *Field_*
// Generic        | generic class                 | GenericClass_PublicField
// Context        | test vs production            | InTest_IsAllowed / InProd_IsFlagged
// Regression     | issue #N — fully-qualified    | Issue_N_FullyQualified
```

## One assertion per cell

One focused test per cell, so a failure names the exact axis.

## Negatives must actually fail on regression

A negative test that only asserts "no exception" is decorative. Assert an empty diagnostic list.

```csharp
// Decorative
await VerifyCSharpDiagnosticAsync(code);
// Real
await VerifyCSharpDiagnosticAsync(code, expected: EmptyDiagnosticResults);
```

## Always add a regression test for a cited issue

If the PR cites `Fixes #N`, include a `Issue_N_*` test reproducing the exact symptom. A later bug adds a new test — never edit the original ([[dotnet-testing]]).

## Don't edit existing tests to fill gaps

Add new methods. Editing risks silently moving an existing assertion's behaviour.

## Checklist

- [ ] Coverage matrix at top of file.
- [ ] Each negative asserts empty diagnostics, not just no-throw.
- [ ] Every cited issue has an `Issue_N_*` test.
- [ ] No existing test edited.
