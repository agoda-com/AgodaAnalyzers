---
name: dotnet-api-design
description: Use when adding or modifying public API surface in a .NET library — new or changed types, attributes, helpers, extension points, or any visibility change. Covers the conventions that protect downstream consumers from breaking changes and long-term maintenance cost.
---

# .NET API design

A library ships its public surface to every consumer that takes a package reference. Every publicly visible change is evaluated for binary compatibility and long-term maintenance cost. Apply these rules when designing new types/members or altering existing ones.

## Visibility defaults

- **Default new types and members to `internal`.** Make something `public` only when an external consumer needs to call it. "It might be useful one day" isn't a reason.
- **Properties, not fields.** Public (or protected/protected-internal) fields are not acceptable. Use read-only auto-properties (`public X Y { get; }`), a constructor-initialized property, or keep the field `private`/`internal`.
- **Prefer constructor initialization + read-only properties** over settable properties when the value shouldn't change after construction.
- If you need to share state between related classes, consider moving the fields into one of them rather than exposing them — passing `this` into a helper is usually a sign of tight coupling.

## Binary and source compatibility

- **Breaking changes are deferred to the next major version.** Binary-compat is non-negotiable. Source-compat is a strong preference.
- If you want to change a signature, keep the original member and add a new one that forwards to it (e.g. overloads). Don't delete or repurpose the old one outside a major version.
- Return types of existing public methods are load-bearing. Changing a return type from a concrete class to something else — even a derived type — is binary-breaking for downstream code compiled against the old signature.
- Adding `params` to an existing array parameter is not a breaking change: existing call sites with an explicit array keep working. Removing `params`, however, is.

## Attributes and consumer-facing API

- Boolean flags on attributes should be **named properties**, not positional constructor parameters.
  - Good: `[Retry(5, StopOnFailure = true)]`
  - Avoid: `[Retry(5, true)]` — the call site doesn't tell the reader what `true` means.
- When introducing a new flag on an existing attribute, add it as a property on the attribute; don't introduce a new constructor overload with more positional parameters.

## Extensibility over modification

- **Additive extension over interface growth.** When you need a new capability on an interface that already has public implementers, add a *new* interface for just that capability (e.g. an `IAsyncFoo` alongside an existing `IFoo`). Growing the existing interface breaks every third-party implementer who has to recompile and add the new member.
- Prefer keeping evolution surfaces (utility/helper types you might refactor) `internal`, so future refactors aren't blocked by external callers.

## XML documentation on public API

- XML docs must match the signature and behavior exactly: parameter names, parameter types, return conditions, and any non-obvious rules.
- If `<returns>` has additional rules beyond the obvious (for example, "returns true when `T` implements `IComparable` **and** the value is not null"), state them.

## Common patterns to avoid

- Making a helper `public` "because I needed to call it from a test" — make it `internal` and use `InternalsVisibleTo`, or make the caller live where the helper does.
- Exposing a field directly because a property felt like ceremony — the property is required.
- Changing an existing signature rather than adding an overload — back-compat forbids it.
- Positional `bool` in an attribute constructor — switch to a named property.
- Return type narrowed/changed from what the existing public surface promised — revert and add a new method with the new return type instead.
