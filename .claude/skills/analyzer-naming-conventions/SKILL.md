---
name: analyzer-naming-conventions
description: Use when creating a new analyzer class, code-fix, helper, or constant. Captures the file, type, member, and constant naming conventions used in this repo.
---

# Analyzer naming conventions

Read two recently-merged siblings in the same category folder before writing, and match them.

## Analyzer class and file

`AG0NNN<VerbPhrase>.cs`, where the verb phrase states the rule's prescription or prohibition and matches the resx Title. File name == class name. Lives under `src/Agoda.Analyzers/AgodaCustom/` (or the conventional sibling folder).

- Good: `AG0006RegisteredComponentShouldHaveExactlyOnePublicConstructor.cs`, `AG0018PermitOnlyCertainPubliclyExposedEnumerables.cs`
- Bad: `AG0006Constructor.cs` (terse), `AG0050TestNames.cs` (no direction)

## Diagnostic ID constant

`public const string DIAGNOSTIC_ID = "AG00XX";` — SCREAMING_SNAKE_CASE. Not `DiagnosticId`/`DiagnosticID`.

## Syntax-node callback

Name it `AnalyzeNode`. For multiple handlers, use distinct verbs (`AnalyzeMethod`, `AnalyzeProperty`), not `AnalyzeNode1/2`.

## Reusable helpers

Own file in `src/Agoda.Analyzers/Helpers/` with a responsibility-describing name (`TestMethodHelpers.cs`, `ForbiddenMethodAnalyzerBase.cs`). Not nested private classes, not `Helpers.cs`/`Utils.cs`/`Common.cs`.

## Resource keys

Add all three in the same PR: `AG00XXTitle`, `AG00XXMessageFormat`, `AG00XXDescription`. Missing keys produce empty IDE strings.

## Checklist

- [ ] `DIAGNOSTIC_ID` constant casing.
- [ ] Callback named `AnalyzeNode`.
- [ ] Helper in own file with a specific name.
- [ ] Class name == file name.
- [ ] Verb phrase communicates rule direction.
- [ ] All three resx keys present.
