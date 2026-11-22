# 0025 — Goal change rules: streaks across settings changes, what an ended attempt contains

**Status:** Accepted, 2026-11-22. Owner choices in slice 8, `specs/009-goal-changes`: spec D-1 and D-2
(Clarifications 2026-11-22 (specify)). Decides SPEC §11.7, second part (the meaning of a streak when goal
evaluation settings change). Builds on [ADR 0016](0016-time-zones.md) (a change applies from the goal's next
commitment or evaluation period, and at most one settings version waits after the current date),
[ADR 0018](0018-evaluation-on-read-and-demo-history.md) and [ADR 0020](0020-derived-figures-on-read.md): figures
are derived on read from stored settings versions, attempts and endings. Keeps
[ADR 0024](0024-excusal-and-pause-rules.md) as it is.

## Context

- SPEC §5.4: goal settings change prospectively; completed evaluations keep the rules in effect at the time and
  new rules apply at the next evaluation boundary. Ending a goal stops future commitments without creating a
  failure. Restarting an ended goal creates a new attempt with its own history and a streak from zero.
- SPEC §6: streaks use the goal's relevant unit (commitments, days or weeks) and must show it.
- SPEC §11.7 leaves open what a streak means when goal evaluation settings change. A change of schedule kind
  (fixed days, times per day, times per week) also changes the streak's unit.
- SPEC §5.4 does not say which commitments or periods an ended attempt contains: the one in progress at the
  ending, and the Pending ones whose grace period is still open.
- Constitution, Decision Discipline: both decisions reject real alternatives. The first also spans features: the
  community slice (slice 9) computes bands and catalog aggregates from the same evaluations.

## Options

**Streaks across settings changes (SPEC §11.7, second part)**

1. **A change keeps the schedule kind, and the streak runs across versions (chosen).** Each result is judged by
   the settings version of its date. Consistency counts all counted results of the attempt. The unit never
   changes within an attempt. A different schedule kind needs a new attempt, which may have any settings.
2. **Any change allowed; the current streak restarts when the unit changes.** Needs rules for mixed units in the
   longest streak and in consistency. Rejected.
3. **Any change allowed; every change restarts the current streak.** Simple, but raising a target slightly wipes
   out a long streak, which discourages honest adjustments. Rejected.

**What an ended attempt contains**

1. **The commitment or period in progress leaves the attempt; Pending ones finalize as usual (chosen).** The
   attempt's last date is the day before the ending date (commitment-based and per-day goals) or the Sunday before
   the ending date's week (per-week goals). Ending itself never adds a Failed or Missed result.
2. **The attempt includes the ending date.** A per-week goal ended mid-week gets a week judged against its full
   target, so ending creates a failure, against SPEC §5.4. Rejected.
3. **As option 1, and unrecorded Pending results are left out too.** Days the user had already missed vanish
   from the figures. Rejected.

## Decision

- A settings change stays within the goal's schedule kind and takes effect from the goal's next commitment or
  evaluation period: the next date for commitment-based and per-day goals, the next Monday for per-week goals. It
  merges into the one settings version that may already wait after the current date. No commitment or period
  whose day has begun changes its version.
- The current and longest streak of an attempt run over its counted results in date order across all its
  settings versions, in the unit of its schedule kind.
- Ending a goal stores the ending date, outcome and optional note. The attempt's last date is derived as option 1
  gives it. The commitment or period in progress at the ending, and everything after it, belong to no attempt:
  they are never expected or counted, and their records are kept as stored facts. Pending results up to the last
  date can still be recorded, corrected or excused during their grace period, and then finalize as usual.
- Ending cuts the goal's pauses to the last date and removes excusals and the waiting settings version after it.
- A new attempt starts on a chosen date from today on, with any settings. Dates between attempts belong to none.
  Each attempt has its own figures, and the report range is the latest attempt.
- Later slices, the community slice included, cite these rules rather than decide them again.

## Consequences

- A user who wants to switch between "daily" and "3 times a week" ends the goal and starts a new attempt. The
  streak then starts from zero, which matches SPEC §5.4's rule for new attempts.
- A commitment recorded on the ending date is not counted. This is accepted: the record is kept, and a new
  attempt that starts on that date counts it.
- Streaks and consistency can mix results judged by different targets within one attempt. This is intended:
  each result reflects the commitment the user had made for its date.
- Changing either rule later needs a superseding ADR, and it changes past figures, because figures are derived on
  read (ADR 0020).
