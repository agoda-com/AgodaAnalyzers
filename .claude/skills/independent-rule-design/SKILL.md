---
name: independent-rule-design
description: Use when an analyzer's design or test rationale appeals to "another rule already catches this." Prevents the fragile coupling where disabling one rule silently breaks the detection of another, or where a shared input shape goes untested because each rule's author assumed the other was handling it.
---

# Independent rule design

Every diagnostic in this repo is independently configurable — a user can disable any one of `AG0001`…`AG0NNN` via `.editorconfig`, `.globalconfig`, or attribute suppressions. That means each analyzer must be **self-sufficient**: it cannot rely on another analyzer to filter, normalise, or pre-validate its input.

The maintainer has corrected this pattern repeatedly:

> "I believe our checks shouldn't interact with each other. Otherwise we have to review all our rules on any change. User can disable any rule at any time, so other checks should work properly."

## What "independent" means in practice

### Don't narrow detection because another rule covers the input

If your analyzer's logic ever reads "we don't have to handle `dynamic` here because AG00NN flags all uses of `dynamic`," stop. The user may disable AG00NN. Your rule must still produce correct behaviour on `dynamic` input — even if "correct behaviour" is "ignore it and don't crash." Explicitly handle the case in your code.

```csharp
// Wrong — assumes AG0030 will have filtered out `dynamic` upstream
if (typeSymbol is INamedTypeSymbol named && named.Name == "Task") ...

// Right — explicitly handle the case your rule cares about, accept that other inputs reach you
if (typeSymbol is INamedTypeSymbol { Name: "Task" } named) ...
// (and add a test that `dynamic` input doesn't crash or false-positive)
```

### Don't skip test cases because another rule covers them

A common shortcut: "I won't test `Thread.Sleep` in my polling-rule because AG0023 forbids `Thread.Sleep` outright." But:

1. Users may disable AG0023 and keep your rule.
2. Your rule's regression test should cover its own input space, including overlapping inputs.

If `Thread.Sleep` is a relevant input to your rule, test it. Annotate the test if the overlap is intentional, but don't omit it.

### Don't share mutable state across analyzers

Each analyzer instance is independent and may run in parallel. Static fields shared across analyzers create order dependencies and concurrency hazards. If two analyzers need shared logic, factor a **pure helper** (no state) into `src/Agoda.Analyzers/Helpers/` and call it from both.

```csharp
// Wrong — shared mutable cache, ordering-dependent
internal static class SharedState { public static HashSet<string> SeenTypes = new(); }

// Right — pure helper, each analyzer manages its own state if needed
internal static class TypeHelpers
{
    public static bool IsAgodaInjected(INamedTypeSymbol type) => /* pure */ ...;
}
```

## When a coupling is actually intentional

There are real cases where one rule's design assumes another rule will be enabled — e.g. a "polish" rule that only makes sense when a baseline rule is also active. Two options:

1. **Document it explicitly** in the PR description: "AG00XX assumes AG00YY is enabled; if AG00YY is disabled this rule may flag false positives in shape Z." Reviewers can then decide whether the coupling is acceptable.
2. **Use the analyzer's own configuration** to expose the coupling: read `analyzerConfigOptions` for a flag that the user (or another analyzer's authoring guidance) sets. The coupling becomes explicit configuration, not implicit assumption.

Don't leave the coupling silent. The next person to disable `AG00YY` shouldn't discover the dependency by chasing a bug report.

## How to spot the failure mode in your own PR

Read your test file. For each test case, ask:

> If I disabled every analyzer in this repo *except* the one this PR adds, would my test still meaningfully exercise the rule?

If the answer is "no, because input X would have been pre-filtered by AG00NN," the rule is coupled. Add explicit handling for X in the analyzer code and a test for X that doesn't depend on other rules being enabled.

## Before-merge checklist

- [ ] No comments or code in the analyzer reference another rule by ID to justify skipping detection.
- [ ] Every shared piece of logic lives in a pure helper, not in static mutable state.
- [ ] Tests cover inputs that other rules might also flag — without depending on those rules.
- [ ] If a coupling is intentional, it's documented in the PR description and (ideally) configurable.
