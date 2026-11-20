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
