---
name: conventional-commits
description: Guides agents through writing commit messages that pass the project's commitlint rules. Use when composing a git commit message or when a commit is rejected by the pre-commit hook.
---

# Conventional Commits

## Overview
This project enforces commit messages via `commitlint` + `Husky`. Messages must follow Conventional Commits format, support Gitmoji types, and require a JIRA ticket for feature/fix/refactor commits.

## Format
```
<type>(optional-scope)!?: <subject>
```
or with a JIRA prefix:
```
ABC-123 <type>(optional-scope): <subject>
```
or with a JIRA suffix:
```
<type>(optional-scope): <subject> #ABC-123
```

## Allowed types

**Conventional:** `feat`, `fix`, `chore`, `docs`, `style`, `refactor`, `test`, `revert`, `build`, `ci`

**Gitmoji:** any emoji from the Gitmoji set (✨, 🐛, 🚀, ♻️, 📝, etc.)

## JIRA ticket rules
The following types **require** a JIRA ticket reference (prefix, suffix, or `WIP:`):
`fix`, `feat`, `refactor`, `test`, `🐛`, `🚑️`, `✨`, `💄`, `🔒️`, `👽️`, `💥`, `🍱`, `♿️`, `🚸`, `🚩`, `🩹`

Exempt types (no JIRA needed): `chore`, `docs`, `style`, `revert`, `build`, `ci`

## Examples

**Invalid:**
```
feat: add login page                                    # ✗ missing JIRA ticket
fix(auth): avoid NRE on startup                        # ✗ missing JIRA ticket
```

**Valid:**
```
BIZ-123 feat: add login page                           # ✓ JIRA prefix
feat: add login page #BIZ-123                          # ✓ JIRA suffix
WIP: feat add login page                               # ✓ WIP exemption
fix(auth): avoid NRE on startup #BIZ-456               # ✓ with scope
chore: update dependencies                             # ✓ no JIRA needed
docs: update README                                    # ✓ no JIRA needed
✨ add user registration #BIZ-789                       # ✓ gitmoji + JIRA suffix
```

## Steps

### 1. Write the message
Determine the type:
- New feature → `feat` (JIRA required)
- Bug fix → `fix` (JIRA required)
- Refactoring without new feature → `refactor` (JIRA required)
- Test changes only → `test` (JIRA required)
- Tooling/config update → `chore` (no JIRA needed)
- Documentation → `docs` (no JIRA needed)

### 2. Add scope if useful (optional)
Scope is the subsystem being changed, e.g. `(auth)`, `(user)`, `(infrastructure)`.

### 3. Verify the hook passes
The Husky hook will run `commitlint` automatically on `git commit`. If the message fails, revise and retry.

### 4. Breaking changes
Append `!` before `:` to signal a breaking change:
```
feat(api)!: remove deprecated endpoint #BIZ-100
```

## Common Rationalizations
| Rationalization | Reality |
|---|---|
| "It's a small fix, no JIRA needed" | The rule applies to all `fix` commits regardless of size. Use `WIP:` if the ticket doesn't exist yet. |
| "I'll skip the hook with `--no-verify`" | `--no-verify` bypasses all hooks including the commitlint check; CI will still enforce the rule. |
| "Gitmoji types don't need JIRA" | Several gitmoji types (`✨`, `🐛`, `🩹`, etc.) are in the JIRA-required set. |

## Red Flags
- Commit message without a `:` separator.
- Missing subject after `:`.
- `fix` or `feat` without a JIRA ticket and without `WIP:`.
- Using a type not in the allowed list.

## Verification
- [ ] Message follows `<type>(scope)?: <subject>` format.
- [ ] Type is in the allowed list.
- [ ] JIRA ticket present if type requires it (or `WIP:` used).
- [ ] `commitlint` hook passes (no error output on commit).

