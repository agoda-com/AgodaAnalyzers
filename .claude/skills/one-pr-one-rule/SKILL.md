---
name: one-pr-one-rule
description: Use before committing or opening a PR in this repo. Enforces the "one diagnostic per PR" convention plus the rebase hygiene that prevents unrelated rules from showing up in the diff and causing avoidable review churn.
---

# One PR, one rule

CONTRIBUTING.md says: *"Please limit the changes contained in your PR to a single issue."* Reviewers enforce this — PRs that touch multiple `AG0NNN*.cs` files get split, and PRs with diffs from stale base branches get a "please rebase" before any substantive review.

The rule is mechanical enough to apply before pushing.

## Before staging any commit

1. **Diff against `master`.** `git diff --name-only origin/master`. Every file in the list must relate to the single rule the PR is about. If you see:
   - Unrelated rule files (`AG00YY*.cs` that you didn't touch deliberately) → revert, your base is stale.
   - IDE-generated `.csproj` changes → see [[dotnet-build-infra]]; remove them.
   - Whitespace-only `.editorconfig` / `.globalconfig` churn → revert.
   - Build-config or NuGet bumps → split into a separate PR.

2. **Count diagnostic IDs touched.** If you modified files for more than one `AG0NNN`, stop. Either split the PR or confirm with the reviewer that a multi-rule PR is acceptable for this specific case (e.g. a cross-cutting helper refactor that has to touch several consumers atomically).

3. **Confirm the base branch is current.** `git fetch origin master` then `git log master..HEAD --oneline` — every commit shown should be one you wrote. If commits from `master` appear in the listing, your branch is behind and a rebase is needed before pushing.

## Rebase, don't merge

Reviewers prefer rebased branches. Merge commits from `master` into a feature branch make the diff harder to read and cause unrelated rules to show as additions on the feature branch.

```bash
git fetch origin master
git rebase origin/master       # not: git merge origin/master
git push --force-with-lease    # not --force; preserves any new server-side commits
```

If a rebase produces conflicts in files you haven't touched, that's a signal something is wrong (probably the base was already stale when you branched). Resolve carefully — when in doubt, take `master`'s version of any file your PR doesn't actually intend to modify.

## When the PR organically grows

You start with one rule. Half-way through, you find a bug in a shared helper that your rule depends on. **Don't fix it in this PR.** Land the helper fix in a separate PR first; rebase your rule's branch on top once it merges. The shared helper fix is independently reviewable and shouldn't be hidden inside a rule PR.

Same pattern for:
- doc improvements to sibling rules you happened to read while writing your own
- analyzer-config / `.globalconfig` cleanups
- typo fixes in unrelated files

Each gets its own PR.

## The "AG0054 shows up as new addition" symptom

When a reviewer comments *"Please rebase, AG00XX shows up as new addition in the diffs"* — your branch was created from a base that didn't yet have AG00XX. Rebasing onto `origin/master` removes the file from your diff.

```bash
git fetch origin master
git rebase origin/master
# Inspect — verify AG00XX is no longer in the diff
git diff origin/master..HEAD --name-only
git push --force-with-lease
```

## Before-merge checklist

- [ ] `git diff origin/master..HEAD --name-only` lists only files related to one diagnostic ID.
- [ ] No IDE-generated `.csproj` Version attributes, no `.editorconfig` whitespace-only edits.
- [ ] Branch is rebased on top of `origin/master`, not merged.
- [ ] PR description names the single issue or rule the PR addresses.
- [ ] If the PR genuinely needs to touch multiple rules, the description explains why and lists each rule explicitly.
