# PageGoalDetail

The goal page: everything about one attempt of one goal — stats, trend, history with corrections, settings, pauses, notes and earlier attempts.

- Page head: BackLink "← Goals", `heading-1` goal name, tags (category, Build or Reduce, Commitment-based or Period-based), attempt line. Actions right: ghost Edit settings, secondary Pause, ghost End goal, primary Record.
- Left column: GoalStats (consistency + streaks + strip), TrendChart, History DataTable (date, recorded amount, StatusChip, note, action). Missed and finalized rows can be corrected ("Add record", "Edit").
- Right column: Settings (KeyValue) with "changes apply from the next commitment", Pauses, Recent notes, Earlier attempts (each with its own consistency and outcome).
- Pause and End goal open a Dialog that explains the effect; End goal asks for the outcome (Achieved, Abandoned, No longer relevant) and an optional note.

## Slice 4 state

The second part of the preview shows the page as slice 4 builds it (HP-004, ADR 0008). Today is Tue 17 Nov 2026 in the preview.

- `/goals/{id}` for the goal's owner; another account's goal, an unknown goal and any goal address for an administrator show the Not found page.
- "← Goals", the goal's name as `heading-1`, then the tags: category, Build or Reduce, and "Commitment-based" or "Period-based", and the attempt line: "Attempt 1 · started 14 Jul" once the goal has started, "Attempt 1 · starts 25 Nov" before.
- A goal on a catalog habit that is no longer in the catalog: the neutral Notice "This habit is no longer in the public catalog. Your goal keeps working." under the page head. The goal list shows nothing extra.
- The Settings card (KeyValue), in this order:
  - Habit: the habit's current name, " · " and "catalog" or "custom" ("Yoga · catalog");
  - Build or reduce: "Build" or "Reduce";
  - Measure: "Yes / no", "Count" or "Duration";
  - Target: the goal's target text ("3 days a week", "15 min, 5 days a week", "at most 2 cups a day", "avoided every day");
  - Schedule: the schedule as on the Goals page ("Mon · Wed · Fri", "Every day", "Flexible · 3 times a week");
  - Evaluated: "Each scheduled day", "Each day" or "Each week", as on the New goal summary card;
  - First week, for a per-week goal only: the date its first week starts ("30 Nov 2026");
  - Started: the start date ("14 Jul 2026"); the row is "Starts" while the goal has not started.
- Under the list: "Changes apply from the next commitment. Past results keep their rules."
- No actions, GoalStats, trend, history, pauses, notes or earlier attempts (slices 5 to 8).
- The full preview's texts follow slice 4 too: the "Build or reduce" row (not "Intent"), the Target row, "Each scheduled day" (not "Each commitment") and the streak unit "commitments" for a commitment-based goal (not "sessions").

## Slice 5 state

The last part of the preview shows the page as slice 5 builds it (HP-001, US3-AS8, US4-AS7, US5-AS5, ADR 0008). Today is Tue 17 Nov 2026 in the first two examples and Tue 18 Aug 2026 in the per-week one.

- Page head: slice 4's "← Goals", name, tags and attempt line, then `ht-spacer` and the primary action "Record" (`aria-haspopup="dialog"`), which opens the RecordDialog with its "For" select (`components/RecordDialog.md`, "Slice 5 state"). No other action (Edit settings, Pause and End goal are slices 7 and 8). "Record" is absent when nothing can be recorded now (no Due and no Pending commitment or period).
- Under the head: slice 4's Notice for a habit no longer in the catalog, unchanged.
- Two columns (`ht-grid-2`), as in the full preview:
  - left: the History card;
  - right: slice 4's Settings card, unchanged, then the Recent notes card.
- History card (`ht-card`):
  - the card head (`ht-card__head`): the title "History", `ht-spacer`, and the muted sentence "Corrections are evaluated with the rules of that date." (`ht-card__sub`) beside it;
  - then a DataTable (`components/DataTable`, `aria-label="History"`) with the columns Date, Recorded, Result, Note and an empty-headed actions column (`td.ht-actions`);
  - the rows: the commitments or periods that have begun in the last 366 days up to the goal's current date, newest first. Upcoming ones are not listed;
  - the first 10 rows, then the ghost button "Show all N →" (`aria-expanded="false"`; N counts all the listed rows), which shows the rest in place.
- The cells:
  - Date: "Tue 17 Nov" for a commitment or a day; "10 – 16 Aug" for a week, or "27 Jul – 2 Aug" across a month end;
  - Recorded: "—" without a record. A commitment shows its record as text ("25 min", "Yes", "3 cups"; a count with its unit as stored, "1 sweet", "2 sweet"). A period shows each of its records as its own ghost button with its value, side by side (`ht-inline`); each opens the RecordDialog's "Edit" state for that record;
  - Result: the StatusChip of the row's state (`components/StatusChip.md`, "Slice 5 state");
  - Note: the ghost button "Add note" without a note, or the note itself as a ghost button; both open the NoteDialog (`components/NoteDialog.md`, "Slice 5 state");
  - actions: "Add record" on a commitment without a record, "Edit" on a commitment with one (it opens that one record), and "Add record" on a period with fewer than 50 records. In the slice-5 state the Due row's action is "Add record" too; the full preview keeps its primary "Record".
- The row actions are offered whatever the state, final rows included, so a Missed or Failed commitment or period takes a record as a correction (US3-AS4). All row controls are ghost `ht-btn--sm` buttons with `aria-haspopup="dialog"`.
- Accessible names of the repeated row controls. Each starts with its visible text and names its row ({date} is "Tue 17 Nov", or the week's "10 – 16 Aug"):

  | Control | Accessible name |
  |---|---|
  | a period's record button | "12 min, edit record 1 of Tue 17 Nov" (the value, then the number of the record in the row, counted from 1) |
  | "Add record" | "Add record for Tue 17 Nov" |
  | "Edit" | "Edit record of Tue 17 Nov" |
  | "Add note" | "Add note for Tue 17 Nov" |
  | a note button | the note's text, then ", edit note of Tue 17 Nov" ("Away for the weekend., edit note of Sat 14 Nov") |

- Recent notes card (right column, after Settings): the title "Recent notes", then at most 5 notes, newest first, each an `ht-goal` item: the note in quotes and its StatusChip on the top line (`ht-goal__top`, `ht-spacer` before the chip), and its date under it (`ht-goal__meta`: "Thu 12 Nov", or "27 Jul – 2 Aug" for a week). The card is absent while the goal has no note.
- A goal with no begun entry (not started yet, or a current date with no commitment or period yet, such as a per-week goal's days before its first week) shows no "Record", no History card and no Recent notes card: only slice 4's content.
- No GoalStats, strip, trend, pauses or earlier attempts (slices 6 to 8).
