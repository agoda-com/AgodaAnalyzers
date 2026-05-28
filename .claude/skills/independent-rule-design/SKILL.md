---
name: independent-rule-design
description: Use when a diagnostic's design or tests assume another rule is enabled. Keeps each analyzer self-sufficient, since any rule can be disabled independently.
---

# Independent rule design

Every diagnostic is independently configurable (`.editorconfig`, `.globalconfig`, suppressions). Each analyzer must work correctly even if every other rule is disabled.

## Don't narrow detection assuming another rule filters input

If your logic reads "no need to handle `dynamic`, AG00NN bans it," handle it anyway — the user may disable AG00NN. "Correct" can be "ignore and don't crash," but it must be explicit.

## Don't skip tests because another rule covers the shape

Test your rule's own input space, including inputs other rules also flag. Annotate intentional overlap; don't omit it.

## Don't share mutable state

Analyzers run in parallel; shared static state creates ordering and concurrency hazards. Shared logic goes in a **pure** helper in `src/Agoda.Analyzers/Helpers/`.

```csharp
// Wrong
internal static class SharedState { public static HashSet<string> Seen = new(); }
// Right
internal static class TypeHelpers { public static bool IsAgodaInjected(INamedTypeSymbol t) => /* pure */; }
```

## Intentional coupling

If a rule genuinely depends on another being enabled, either:
1. Document it in the PR description (which rule, what breaks if disabled, which input shape), or
2. Expose it via `analyzerConfigOptions` so the coupling is explicit configuration.

Never leave it silent.

## Self-check

For each test: if every rule but this one were disabled, would the test still exercise the rule? If "no, input X would be pre-filtered by AG00NN," add explicit handling for X plus a test that doesn't depend on other rules.

## Checklist

- [ ] No code/comment cites another rule ID to justify skipping detection.
- [ ] Shared logic is a pure helper, no static mutable state.
- [ ] Tests cover overlapping inputs without depending on other rules.
- [ ] Intentional couplings documented or made configurable.
