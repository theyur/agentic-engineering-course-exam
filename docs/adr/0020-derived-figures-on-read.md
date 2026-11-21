# 0020 — Derived figures are computed on read, with no caching

**Status:** Accepted, 2026-11-20. Owner choice in slice 6, `specs/007-personal-reports` (spec D-4, Clarifications
2026-11-20). Extends [ADR 0018](0018-evaluation-on-read-and-demo-history.md) from evaluation states to the
figures derived from them; changes nothing else in it. Decides the roadmap row "Caching of derived figures"
(ARCHITECTURE §9).

## Context

- Constitution II: every derived figure (consistency, current and longest streak, band membership, catalog
  aggregates) MUST be reproducible from stored facts, and a correction MUST give the same results as if the
  corrected value had been recorded on time; "any stored derivation is invalidated by the correction".
- ADR 0018 computes every evaluation state on read, from the stored records, the settings version of its date and
  the supplied time; nothing runs in the background. It leaves caching to later slices: they "compute from the
  same function and add caching only with their own decision".
- Slice 6 adds the first derived figures: consistency, current and longest streak, recent results and the trend
  of each goal, the goals overview and the signed-in Home's dashboard (spec FR-001 to FR-013). Slice 7 adds the
  Excused and Paused exclusions to them, and slice 9 builds Consistency Bands and Catalog Habit Performance from
  the same consistency over a rolling 30-day window.
- These figures change not only when a user acts (a record added, changed or deleted, a note, a settings change)
  but also when time passes: a Pending result becomes Met, Failed or Missed at the end of its grace period, and
  the Home's "this week" moves at local midnight between Sunday and Monday.
- Scale: one hosted demo with a handful of demo accounts and reviewers' own accounts, refreshed daily (ADR 0015,
  ADR 0018). ARCHITECTURE §1 and constitution VI rule out infrastructure for hypothetical scale.

## Options

1. **Compute on read (chosen).** Every figure is computed for each read from the stored records, the settings
   versions and the supplied time, by the same evaluation function as the states of ADR 0018. Nothing is stored.
2. **Cache per goal.** Store each goal's figures and invalidate them on every record, note and settings change,
   and at every grace-period end and week boundary. The time-driven invalidations need a scheduled job or a
   "valid until" instant per entry; a missed invalidation shows a stale streak or consistency, and every rule of
   slices 7 and 8 adds invalidation cases to test. Rejected.
3. **Store final results only** (a result never changes once final, so it could be kept). False here: a
   correction changes a final result at any time (slice 5 FR-006), so the store would still need invalidation.
   Rejected.

## Decision

- Consistency, streaks, recent results, the trend, the goals overview and the dashboard are computed on every
  read from stored facts and the supplied time. No figure is stored, cached across reads, or written by a
  background job.
- Within one read, a use case may compute each goal's evaluations once and derive all of that read's figures from
  them. That is not caching: nothing outlives the read.
- A later slice that needs caching, for example community views (slice 9) if they become too slow, decides it in
  its own ADR that supersedes this one for its figures, and shows how each invalidation is tested.

## Consequences

- One source of truth: a correction, a late record, a deleted record or the end of a grace period changes every
  figure on the next read, with nothing to invalidate or re-synchronize (constitution II holds by construction).
- Every read recomputes a goal's whole attempt. At demo scale (goals of weeks to a few months, a year at most in
  tests) that is cheap; slice 6's SC-007 (a goal with 366 daily results read in under 2 seconds) checks it in
  an integration test against SQL Server, since the hosted demo's goals are at most 84 days old.
- The dashboard and the goals overview evaluate every goal of the user on each read; a user with many goals pays
  for all of them. Accepted for v1.
- Slice 9's figures over all consenting users will be the most expensive reads; this ADR does not decide them
  beyond "on read by default".
