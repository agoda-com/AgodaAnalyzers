---
name: one-pr-one-rule
description: Use before committing or opening a PR. Keeps each PR to a single diagnostic and the branch rebased so unrelated rules don't appear in the diff.
---

# One PR, one rule

Each PR introduces or modifies exactly one diagnostic ID. CONTRIBUTING.md: *"limit the changes contained in your PR to a single issue."*

## Before staging

1. `git diff --name-only origin/master` — every file must relate to the one rule. Revert: unrelated `AG00YY*.cs`, IDE-generated `.csproj` edits ([[dotnet-build-infra]]), whitespace-only `.editorconfig`/`.globalconfig` churn. Split out: build-config or NuGet bumps.
2. Count diagnostic IDs touched. More than one → split, or confirm a multi-rule PR is warranted (e.g. an atomic cross-cutting helper change).
3. `git log master..HEAD --oneline` — every commit should be yours; if `master` commits appear, rebase.

## Rebase, don't merge

```bash
git fetch origin master
git rebase origin/master
git push --force-with-lease
```

Merge commits from `master` make the diff noisy and surface unrelated rules as additions.

## When the PR grows

Found a bug in a shared helper, or a doc/typo in an unrelated file? Land it in a separate PR first, then rebase on top. Don't bundle.

## "AGxx shows up as new addition" symptom

Branch was cut from a base lacking that rule. Fix:

```bash
git fetch origin master
git rebase origin/master
git diff origin/master..HEAD --name-only   # verify it's gone
git push --force-with-lease
```

## Checklist

- [ ] `git diff origin/master..HEAD --name-only` lists only one rule's files.
- [ ] No IDE `.csproj` Version attrs, no whitespace-only config edits.
- [ ] Branch rebased on `origin/master`, not merged.
- [ ] PR description names the single rule/issue (or justifies a multi-rule PR).
