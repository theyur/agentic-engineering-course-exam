# CommitmentRow

One row per commitment or period in the Today list: name, schedule or progress, a status chip and the next action.

## Parts
- `ht-row__name`: habit / goal name (`body-strong`). Add a `Reduce` tag for Reduce goals.
- `ht-row__meta`: schedule ("Mon · Wed · Fri"), progress ("12 of 20 min", "1 of at most 2 cups") or time left ("22 h left to record").
- `ht-progress` for count and duration goals: `<div class="ht-progress"><span style="width:60%"></span></div>` with `role="progressbar"`.
- One `StatusChip`, then one action button.

## Rules
- Order in the Today card: Pending first (time is running out), then Due today, then finalized Missed.
- Action per state: Due → "Record" (primary for yes/no) or "Add time" / "+1"; Pending → "Record" + ghost "Excuse" (commitment-based goals only); Missed → ghost "Add note".
- Partial progress is shown, never hidden: "12 of 20 min" stays visible after the day ends.

## Slice 6 state

The last part of the preview shows the rows as slice 6 builds them in the Home's Today card (HP-003, D-2, ADR 0008). Today is Wed 18 Nov 2026, 19:00 in the first card. One Razor component, `CommitmentRow`, renders every state.

- `ht-row__name`: the goal's name, then a "Reduce" Tag (`ht-chip`) for a Reduce goal; a Build goal has no Tag.
- `ht-row__meta`, per state ({date} as in the History: "Mon 16 Nov" for a commitment or a day, "16 – 22 Nov" for a week):

  | State | Meta line |
  |---|---|
  | Due, no record | the goal's schedule: "Mon · Wed · Fri" |
  | Due, with a record | its progress: "15 of 20 min today", "1 of at most 2 cups today", "1 of 2 this week" |
  | Pending | "{date} · {N} h left to record": "Mon 16 Nov · 5 h left to record", "16 – 22 Nov · 5 h left to record"; N is the whole hours until its grace period ends, rounded up, as in the RecordDialog's "For" labels |
  | Missed | "{date} · no record": "Sun 15 Nov · no record", "9 – 15 Nov · no record" |
  | Upcoming | its date: "Fri 20 Nov", "23 – 29 Nov" |

- The progress bar (`ht-progress`, `role="progressbar"`, `aria-valuenow`, `aria-valuemin="0"`, `aria-valuemax` the goal's amount, `aria-label="{goal name} progress"`) is shown under the meta line of a Due row of a count or duration goal that has a record: the row whose meta line is its progress. Its width is the progress's share of the amount, at most 100%. Yes/no goals, Pending, Missed and Upcoming rows have no bar.
- Then the StatusChip (`components/StatusChip.md`, "Slice 6 state"): "Due today", "Due this week", "Pending", "Missed" or "Upcoming".
- Then one action, `ht-btn--sm` with `aria-haspopup="dialog"`:

  | Row | Action | Opens |
  |---|---|---|
  | a Due or Pending period (per day or per week) | "Record" (secondary) | the RecordDialog fixed to that period |
  | a Due or Pending commitment without a record | "Record" (secondary) | the RecordDialog fixed to that commitment |
  | a Due or Pending commitment with a record | "Edit" (secondary) | that record in the RecordDialog |
  | a Missed commitment or period without a note | "Add note" (ghost) | the NoteDialog |
  | a Missed commitment or period with a note | "Edit note" (ghost) | the NoteDialog |
  | Upcoming | none | — |

  The accessible name starts with the visible text and names the row: "Record for {goal name}, {date}", "Edit record of {goal name}, {date}", "Add note for {goal name}, {date}", "Edit note of {goal name}, {date}". No "Add time", "+1" or "Excuse" (slice 7). No row action is primary, because the card holds many rows of equal weight.
- Order in the Today card: every Pending row, the one whose grace period ends soonest first; then every Due row, in the goal order of the Goals page; then every row Missed this week, the latest grace end first; rows whose grace periods end at the same instant in goal order; then the Upcoming rows, the earliest start first. An Upcoming row is shown for each goal with no Due row whose next commitment or period starts within 6 days.
- A Today card with no row says "Nothing to record right now." (`ht-card__sub`). A tile's filter that leaves no row says the same.
- Partial progress is shown, never hidden: "15 of 20 min today" stays on the row while the day runs.

## Slice 7 state

The last part of the preview shows the rows as slice 7 builds them in the Home's Today card (HP-005, HP-003, D-2, D-3, US1-AS12, ADR 0008). Everything of slice 6 stays: the parts, the meta lines, the progress bar, the chips, the order and "Nothing to record right now.".

- A Pending row of a commitment-based goal whose commitment has no record gets a second action after "Record": the ghost "Excuse" (`ht-btn--ghost ht-btn--sm`, `aria-haspopup="dialog"`). It opens the Excuse dialog for that row's goal and date (`components/ExcuseDialog.md`).
- The actions, slice 6's table with the new row:

  | Row | Actions | Opens |
  |---|---|---|
  | a Pending commitment without a record | "Record" (secondary), then "Excuse" (ghost) | the RecordDialog fixed to that commitment; the Excuse dialog |
  | a Due commitment without a record | "Record" (secondary) | the RecordDialog fixed to that commitment |
  | a Due or Pending period (per day or per week) | "Record" (secondary) | the RecordDialog fixed to that period |
  | a Due or Pending commitment with a record | "Edit" (secondary) | that record in the RecordDialog |
  | a Missed commitment or period without a note | "Add note" (ghost) | the NoteDialog |
  | a Missed commitment or period with a note | "Edit note" (ghost) | the NoteDialog |
  | Upcoming | none | — |

  A Due commitment is excused from the RecordDialog's "Excuse instead", not from its row (HP-003, HP-005). "Excuse" comes after "Record", as in the full preview, so "Record" stays where it is on every row (settled here, T008).
- The accessible name of "Excuse" starts with its visible text and names the row: "Excuse {goal name}, {date}" ("Excuse Reading, 20 min daily, Mon 16 Nov"; settled here, T008, after slice 6's "Record for {goal name}, {date}").
- After "excused" the island reads the list again: the row leaves the Today list, and the "Pending · still can record" tile drops by 1. Focus goes to the nearest remaining control, as the row and its buttons are gone (HP-007).
- Excused and Paused entries are never rows: they are not Due, Pending or Missed (FR-019). A goal's Upcoming row is its earliest Upcoming commitment or period that is not Paused, within slice 6's 6 days, so a goal paused for the whole window has none.
- Still no "Add time" or "+1".

## Slice 8 state

The last part of the preview shows the rows as slice 8 builds them in the Home's Today card (HP-007, FR-019, US3-AS12, US4-AS2, contracts/host.md "Signed-in Home", ADR 0008). Everything of slice 7 stays: the parts, the meta lines, the progress bar, the chips, the actions, the accessible names, the order and "Nothing to record right now.".

- A Pending row of an ended attempt stays a row until its grace period ends. It carries one Tag (`ht-chip`, `components/Tag.md`) in `ht-row__name`, directly after the name (HP-007):
  - "Attempt 1" when its attempt is not the goal's latest (`IsLatestAttempt` false: the goal was restarted). The name before it is that attempt's name (FR-004, FR-019: as it was at its last date), and the row's Build or Reduce is that of the version of its date;
  - otherwise "Ended" when its goal is ended (`TodayEntry.GoalEnded`).

  A Reduce goal's "Reduce" Tag comes after it ("Coffee, at most 2 cups a day", "Ended", "Reduce"): the Tag belongs to the name, as HP-007 places it (settled here, T009). The texts come from `GoalChangeTexts` and `ReportText` (HP-008).
- Such a row is shown and recorded with its own settings (`Entry.Goal.SettingsOf(Entry)`): the state words, the progress bar test, the RecordDialog's field and unit. An earlier attempt of another kind than the latest attempt keeps its own (AC-080).
- Its actions are slice 7's for a Pending row: "Record" (or "Edit" with a record), and the ghost "Excuse" for a commitment without a record (FR-010 keeps excusals of an ended attempt's dates). The accessible names name the row's own name and date: "Record for Meditation, 10 min daily, Sun 15 Nov".
- An ended attempt has no Due, Missed or Upcoming row (FR-019): once its grace period ends, the row leaves the list and is not counted in "Missed this week".
- A restarted goal's new attempt has its own rows without a Tag, as any goal's (the second card: attempt 2's Due Tue 17 Nov).
