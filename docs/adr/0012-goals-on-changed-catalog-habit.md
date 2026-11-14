# 0012 — Goals keep working when their catalog habit is edited or unpublished

**Status:** Accepted, 2026-11-12. Owner answer in slice 2, `specs/002-habit-catalog` (Clarifications,
FR-015). Settles the roadmap row "How goals behave when their catalog habit is edited or unpublished" (idea
brief §9, §15). Slice 4 (`goals`) builds it.

## Context

- Administrators edit and unpublish catalog habits (SPEC §4); catalog habits cannot be deleted.
- Personal goals are based on a catalog or custom habit (SPEC §2, §5.1) and arrive in slice 4. The idea brief
  lists the risk that an edit or unpublish breaks or silently renames goals.
- The administrator must never see whose goals use a habit (SPEC §3, §8; constitution III).

## Options

1. Goals keep working and show the habit's current name; unpublishing only removes it from the catalog.
2. Goals keep working and keep the habit name from when the goal was created.
3. **Like 1, and unpublishing a habit that goals use first asks the administrator to confirm, showing how
   many goals use it (chosen).**

## Decision

- Editing or unpublishing a catalog habit never changes, ends or fails a goal based on it. Such goals keep
  working and always show the habit's current name and details.
- Unpublishing takes the habit out of the public catalog only. Its goals' owners still see it in their goals.
- Unpublishing a habit that at least one goal uses asks the administrator to confirm first. The confirmation
  shows only the number of goals that use the habit: no owner, display name or any other goal detail.
- Slice 2 has no goals, so every unpublish there acts at once. Slice 4 adds the count and the confirmation.

## Consequences

- An administrator's rename shows in users' goals, so catalog edits should keep a habit's meaning; renaming a
  habit into a different behavior is a misuse, not a supported change.
- The goal count is an aggregate shown to administrators. It reveals no individual's data, but slice 4 must
  test that the confirmation carries nothing beyond the number (constitution III).
- Slice 4 decides how a goal on an unpublished habit looks to its owner (for example a note that the habit
  left the catalog) and whether such a habit can still start new goals.
