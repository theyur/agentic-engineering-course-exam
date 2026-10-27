---
status: Living
updated_at: "2026-10-27"
---

# Domain Context — Habit Tracker

<!--
CONTEXT.md is the domain glossary — not a PRD and not a scratch pad. NO implementation
detail here (no datastore/broker/framework names, no API contracts) — only domain words
and the boundaries between them. Implementation choices live in the SAD and ADRs;
behaviour lives in PRD.md.

Multi-context repos: each bounded context has its own CONTEXT.md at its root path
(registered in CONTEXT-MAP.md). System-wide terms that span all contexts live in the
repo-root CONTEXT.md. Never duplicate a term across files — pick one owner.

Terms get fixed inline, the moment they surface in an interview / PRD / review — never
batched «I'll consolidate later». Empty H2 → prune before commit; keep only the sections
that carry real content. ## Glossary is mandatory; the other two are optional.
-->

## Glossary

<!-- One line per term: name · one-sentence canonical definition · one-sentence boundary
     (what it is NOT / the concept it gets confused with). Alphabetical once there are a few. -->
- Catalog habit — a public, reusable description of a behavior (e.g. Reading, Yoga), maintained by administrators and visible to anyone once published. NOT a personal goal (one user's measurable commitment based on a habit), and NOT a signal that anyone's participation is public.
- Custom habit — a behavior created privately by one user for their own goals, visible only to its owner. NOT a catalog habit (never public, never in community statistics, not visible to administrators).
- Excused — the user-chosen state of an individual scheduled commitment in a commitment-based goal that exempts it from evaluation, requiring no approval and optionally carrying a reason, and that can be set only while the commitment is upcoming or Pending, never after it has been finalized as Missed or Failed; it is excluded from consistency and preserves but does not extend the streak. NOT Pause (a pause suspends a whole goal over a date interval and applies to period-based goals too; excused covers one commitment of a commitment-based goal), and NOT Pending (pending is temporary and resolves on its own; excused is the user's deliberate, lasting choice).
- Failed — the finalized outcome of a commitment or evaluation period whose recorded activity did not meet the applicable target: below the required amount for a Build goal, or above the permitted amount for a Reduce goal. NOT Missed (a missed outcome has no record at all), and NOT partial progress (the recorded amount is preserved and reported; Failed is only the evaluation verdict on it).
- Missed — the finalized outcome of a commitment or evaluation period for which no activity was recorded by the end of the 48-hour grace period; it counts as unsuccessful in consistency and resets the streak. NOT Failed (a failure has a record that fell short of the target), and NOT Pending (pending is still inside the grace period and not yet final).
- Pending — the not-yet-final state of a commitment or evaluation period that has ended but whose 48-hour grace period is still open, whether or not any activity has been recorded so far; it is excluded from consistency and never resets a streak. NOT Missed (missed is final once the grace period has elapsed with no record), and NOT Excused (excused is a deliberate, lasting exemption by the user, while pending is temporary and resolves on its own).
- Suggested starting goal — an optional, fully customizable example target shown with a catalog habit. NOT a prescribed target or coaching advice, and NOT a personal goal until a user creates one from it.
