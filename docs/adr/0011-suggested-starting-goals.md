# 0011 — Suggested starting goals: several per habit, structured like a goal

**Status:** Accepted, 2026-11-12. Owner answers in slice 2, `specs/002-habit-catalog` (Clarifications, FR-006,
FR-007). Settles the roadmap rows "One or several suggested starting goals per catalog habit" and "Shape of a
suggested starting goal, compatible with goals". Slice 4 (`goals`) cites it.

## Context

- The idea brief (§7, §13) chose "at most one" suggested starting goal per catalog habit. SPEC §4 says
  "suggested starting goals", SPEC §2 uses the singular, `design/pages/PageHabitDetail` shows three and
  `design/components/HabitCard` one per card.
- Slice 4 offers a suggested starting goal as the starting point of a new goal ("Start with this", "Use this
  habit"). The New goal form (`design/pages.md`) has Build / Reduce, a measurement (yes/no, count, duration),
  a target ("at least" / "at most", an amount and a unit) and a schedule (fixed days, or N times per day or
  week).
- The idea brief (§9, §10) names the risk: the suggestion's shape must stay compatible with goals, and
  presets must not duplicate a goal model that does not exist yet.

## Options

Number per habit:

1. **Several, one marked for the card (chosen).** Matches the design; the card shows the marked one.
2. At most one. Matches the brief; the detail page design changes first.
3. Several, the card shows the first. Like 1, with the order deciding the card.

Shape:

1. **Structured fields that match the New goal form (chosen).** Slice 4 pre-fills the form directly.
2. Display text only. Simplest now; slice 4 cannot pre-fill.
3. Text plus a few key fields. Partial pre-fill.

## Decision

- A catalog habit carries 0 to 5 suggested starting goals in the administrator's order. A non-empty list has
  exactly one marked as the card's suggestion.
- A suggested starting goal stores: Build or Reduce; the measurement (yes/no, count or duration); for count
  and duration a whole-number amount (1–999 for Build, 0–999 for Reduce) and a unit (free text up to 20
  characters for count; minutes or hours for duration); and the schedule, either fixed days of the week or N
  times (1–20) per day or per week. A Build target reads "at least", a Reduce target "at most" (SPEC §5.2).
- The text shown ("15 min, 5 days a week") is produced from the fields by one shared formatter, never stored
  or typed separately.
- Slice 2 stores and shows these fields only. How a goal built from them is evaluated, and whether the goal
  model needs more (for example periods longer than a week), is slice 4's decision; slice 4 extends these
  fields if it adds options.

## Consequences

- Slice 2 validates the fields and has a suggested-starting-goal editor in the administrator's catalog habit
  form, which needs a new design page first (constitution IV).
- The detail page preview's "1 book a month · monthly period" cannot be expressed and is replaced in
  `design/` before build.
- Slice 4's goal settings start from the same vocabulary, so "Start with this" can pre-fill every field. If
  slice 4 changes the vocabulary, it migrates the stored suggestions in the same change.
- Part of the goal model's shape is fixed before slice 4 plans it; this is the accepted cost of pre-filling.
