---
name: analyzer-test-coverage-matrix
description: Use when adding or modifying tests for a Roslyn diagnostic analyzer. Replaces the reviewer's hand-rolled "did you cover X, Y, and Z?" list with an explicit coverage matrix in the test file, so the question is answered before the PR is opened.
---

# Analyzer test coverage matrix

For every positive case the analyzer fires on, reviewers enumerate the negative and edge cases you forgot. The recurring list across this repo's review history:

- accessibility modifiers: `public` / `internal` / `protected` / `private`
- syntactic form: literal, named constant, local variable, parenthesised expression, binary expression, interpolated string, fully-qualified type reference
- generics: generic vs. non-generic, open vs. closed
- collection-ness: scalar vs. array vs. `IEnumerable<T>` vs. `IReadOnlyList<T>`
- interface vs. class implementation
- test context: in-test vs. production (see [[test-context-detection]])

A test file that doesn't visibly cover these axes triggers blocking review comments. The fix is to make the coverage explicit.

## Build the matrix from the rule's behaviour

At the top of the test file, write a coverage matrix as a comment. It serves two purposes: forces you to think about each axis before writing tests, and answers the reviewer's question before they ask.

```csharp
// AG0XXX coverage matrix
//
// Trigger:  public field on a class — flag.
//
// Axis                         | Cases covered                                  | Test method
// -----------------------------+------------------------------------------------+---------------------------
// Accessibility                | public                                         | PublicField_IsFlagged
//                              | internal                                       | InternalField_IsAllowed
//                              | protected                                      | ProtectedField_IsAllowed
//                              | private                                        | PrivateField_IsAllowed
// Field type                   | int                                            | IntField_IsFlagged
//                              | byte[]                                         | ByteArrayField_IsFlagged
//                              | string                                         | StringField_IsFlagged
//                              | List<T>                                        | ListField_IsFlagged
// Generic context              | generic class                                  | GenericClass_PublicField_IsFlagged
// Test vs production           | test class                                     | InTestClass_IsAllowed
//                              | production class                               | InProductionClass_IsFlagged
// Regression                   | issue #NNN — fully-qualified field type        | Issue_NNN_FullyQualifiedFieldType
```

You don't have to test the cartesian product. You do have to *consider* it and either cover or deliberately skip each cell, with the skip recorded in the matrix.

## One assertion per cell

Each cell in the matrix gets one focused test method. Don't combine three cells into one test; when it fails, you want the failing assertion to name the exact axis.

```csharp
[Test]
public async Task ProtectedField_IsAllowed()
{
    const string code = @"
        public class Foo { protected int X; }
    ";
    await VerifyCSharpDiagnosticAsync(code, expected: EmptyDiagnosticResults);
}
```

## Always include a regression test for the cited issue

If the PR title or body cites an issue number (`Fixes #N`, `Closes #N`), the test file must contain a test reproducing the exact symptom from that issue:

```csharp
// Regression: input from issue #N — escaped string in attribute argument used to throw.
[Test]
public async Task Issue_N_EscapedStringInAttributeArgument()
{
    const string code = @"[SomeAttribute(""\""quoted\"""")] public class Foo {}";
    await VerifyCSharpDiagnosticAsync(code, expected: ...);
}
```

If a reviewer files a bug after merge, the next PR adds another regression test, never modifies the original. See [[dotnet-testing]] for the broader rule on not editing existing tests.

## Negative tests have to actually run

A frequent failure mode: the matrix lists negative cases, but the test method body matches the positive case (just without the assert). Read each negative test and ask: *if the analyzer changed to fire on this input, would this test catch it?* If the only assertion is "no exception thrown," the test is decorative; assert the diagnostic list is empty.

```csharp
// Decorative — doesn't fail if the analyzer starts flagging this input
[Test]
public async Task InternalField_IsAllowed()
{
    const string code = "public class Foo { internal int X; }";
    await VerifyCSharpDiagnosticAsync(code);
}

// Real — fails if the analyzer regresses
[Test]
public async Task InternalField_IsAllowed()
{
    const string code = "public class Foo { internal int X; }";
    await VerifyCSharpDiagnosticAsync(code, expected: EmptyDiagnosticResults);
}
```

## Don't modify existing tests when adding coverage

If the matrix shows a gap, add a new test method — don't edit an existing one to also cover the new axis. Editing risks moving the existing assertion's behaviour silently. See [[dotnet-testing]] for the reasoning.

## Before-merge checklist

- [ ] Coverage matrix at the top of the test file, listing every axis the rule's behaviour depends on.
- [ ] Each negative test asserts an empty diagnostic list, not just "no exception."
- [ ] Every cited issue number has a corresponding `Issue_NNN_*` regression test.
- [ ] No existing test method was edited; gaps were filled with new methods.
