# 0024 — Excusal and pause rules: partly paused weeks, when a commitment can be excused

**Status:** Accepted, 2026-11-21. Owner choices in slice 7, `specs/008-excusals-and-pauses`: spec D-1, D-2 and
D-3 (Clarifications 2026-11-21 (specify)), and the early-resume and excusal-removal answers (Clarifications
2026-11-21 (clarify)). Decides SPEC §11.2 (partially overlapping pauses) and SPEC §11.3
(excusing after finalization). Builds on [ADR 0018](0018-evaluation-on-read-and-demo-history.md) and
[ADR 0020](0020-derived-figures-on-read.md): Excused and Paused are derived on read from stored excusals and
pauses, like every evaluation state and figure.

## Context

- SPEC §5.3: users excuse individual scheduled commitments of commitment-based goals and pause any goal for a
  date interval; excused and paused expectations leave consistency and neither extend nor break a streak; a pause
  cannot turn a finalized failure into an exemption.
- SPEC §11.2 leaves open how a pause that covers only part of a multi-day evaluation period (a Monday-to-Sunday
  week of a per-week goal) affects that period. SPEC §11.3 leaves open whether a commitment can be excused after
  it is final.
- Constitution, Decision Discipline: these decisions reject real alternatives, and they span features: the
  community slice (slice 9) computes Consistency Bands and Catalog Habit Performance from the same eligible final
  evaluations (SPEC §7.2, §7.3), so it inherits which evaluations are exempt.

## Options

**Partly paused week (SPEC §11.2)**

1. **The whole week is Paused while at least one of its dates is paused (chosen).** Simple to explain and to
   compute; no fractional targets. Cost: one paused day exempts its whole week, also when its records already
   reach the target.
2. **Paused only when all seven days are paused, otherwise the full target.** No relief for a partial week; a user
   ill from Thursday fails a 3-times-a-week target they could not reach. Rejected.
3. **A target scaled to the unpaused days.** Needs rounding rules for Build and Reduce, count and duration, and
   makes the target of a past week depend on later pause edits. Rejected.

**When a commitment can be excused (SPEC §11.3)**

1. **Only while it is Due or Pending (chosen).** The excusal answers "I could not do this one"; planned absences
   use a pause.
2. **Upcoming commitments too.** Overlaps with pauses, and needs a page entry for future commitments. Rejected.
3. **Final commitments too.** Rewrites finalized history, against the spirit of SPEC §5.3's rule for pauses.
   Rejected.

**A commitment that has a record**

1. **Refused: the user deletes the record first (chosen).** An Excused commitment never has a record.
2. **Excused with the record kept.** A record and an exemption on one commitment. Rejected.
3. **Excused with the record deleted silently.** Data loss. Rejected.

**Until when an excusal can be removed**

1. **Only while the commitment's grace period is open (chosen).** From the end instant on, the excusal is as
   lasting as a final result.
2. **At any time.** Removing a late excusal would turn the commitment straight into Missed or Failed, rewriting
   history the user had settled. Rejected.
3. **Never.** A mistaken excusal could not be undone, even while the commitment can still be recorded. Rejected.

**Resuming a goal early**

1. **The current date is expected again (chosen).** The pause ends on the day before the current date; a pause
   that began on the current date is removed. The user records today within its grace period as usual.
2. **The current date stays paused; expectations return the next day.** Simpler for a user who resumes late in
   the day, but "resume now" would not mean now. Rejected.

## Decision

- A commitment or a per-day period is Paused exactly on the dates of a pause of its goal. A per-week period is
  Paused as a whole, whatever its records, while at least one of its seven dates is in a pause of its goal. This
  holds for an upcoming pause too: a pause that begins later in the current week already pauses that week.
- A commitment can be excused only while it is Due or Pending and has no record. An excusal can be removed only
  while the commitment's grace period is open; from its end instant on it is permanent.
- Resuming early ends the current pause on the day before the goal's current date, or removes it when it began on
  the current date; a per-week period that keeps a paused date stays Paused.
- A pause starts on the goal's current date at the earliest, so it never changes a result that was Pending or final
  when it was set. SPEC §5.3's "failures already recorded" are read as final Failed results: a Due commitment or
  period is not yet a failure, also when its records already exceed a Reduce limit, so a pause from the current
  date, or under the partly paused week rule a pause later in its week, makes it Paused; its records are kept, so no
  activity is erased. A pause takes precedence over an excusal of the same date; the excusal is kept.
- Excused and Paused replace the evaluation state; they are counted neither as successful nor in the total of
  consistency, and skipped by streaks. Later slices, the community slice included, cite these rules rather than
  decide them again.

## Consequences

- Slice 5's period logic gains one rule for per-week periods; no target is ever scaled.
- A user can exempt a whole week with a one-day pause, also a week whose records already reach the target or
  already exceed a Reduce limit. This is accepted: pauses are the user's own choice, the records are kept, and the
  figures still show the exempt week as Paused.
- The community slice treats Excused and Paused evaluations as not eligible.
- Changing any of these rules later needs a superseding ADR, and it changes past figures, because figures are
  derived on read (ADR 0020).
