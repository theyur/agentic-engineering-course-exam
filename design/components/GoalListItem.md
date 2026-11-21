# GoalListItem

One goal in the Goals list: name and schedule, consistency, current streak, the last 15 results and the next action.

- Wrap items in `ht-list` (one card); each item is `ht-goalitem` (5 columns; 2 columns under 860px).
- Name links to the goal page. Meta line: the Build or Reduce Tag (never called "intent", `CONTEXT.md`), schedule and today's state ("12 of 20 min today", "missed Tue"); add a Pending StatusChip when something is waiting.
- Consistency with count, or N/A; streak with its unit; mini ConsistencyStrip (`ht-strip--mini`).
- One action: Record (primary) when something is due, otherwise a secondary button.

## Slice 4 state

The second part of the preview shows the item as slice 4 builds it (HP-003, ADR 0008): `ht-goalitem ht-goalitem--reduced`, one column.

- The name links to the goal page.
- The meta line: the Build or Reduce Tag, then the schedule and at most one of "first week starts Mon 23 Nov", "starts Tue 24 Nov", "next today" / "next Wed 18 Nov" (the rules are in `pages/PageGoals.md`).
- No consistency, streak, strip or action (slices 5 and 6).

## Slice 6 state

The last part of the preview shows the full item as slice 6 builds it (HP-002, ADR 0008), with the parts of this slice. It replaces the reduced item: no slice-6 page shows one. Today is Tue 17 Nov 2026 in the preview.

- `ht-goalitem`, five columns (two under 860px), as in the full preview.
- Column 1:
  - the goal's name linking to its goal page (`ht-goalitem__name`);
  - the meta line (`ht-goalitem__meta`): the Build or Reduce Tag; a Pending StatusChip (`ht-chip ht-status ht-status--pending`, "Pending") when the goal has a Pending commitment or period; then the text.
- The meta line's text is slice 4's (the schedule, then " · " and the first variant that applies: "first week starts …", "starts …", "next today" / "next …"; `pages/PageGoals.md`, "Slice 4 state"), followed by " · " and the progress of the goal's Due entry when it has a record:
  - "Every day · next today · 12 of 20 min today" (a commitment-based duration goal);
  - "Every day · 1 of at most 2 cups today" (a Reduce count goal counted per day);
  - "Flexible · 2 times a week · 1 of 2 this week" (a goal counted per week);
  - with no Due entry, or a Due entry without a record, the text is slice 4's alone ("Mon · Wed · Fri · next Wed 18 Nov", "Tue · Thu · next today").

  The progress is "today" for a commitment or a day and "this week" for a week. No "missed Tue": a Missed result is in the strip, not the meta line.
- Column 2: "Consistency" (`ht-goalitem__label`) under the consistency as one whole text in `ht-goalitem__value`: "90% (18 of 20)", or "N/A" with no finalized result. No `<small>` part (tasks, Conventions).
- Column 3: "Current streak" under the streak with its unit as one whole text: "6 commitments", "1 day", "2 weeks", "0 commitments".
- Column 4: the mini strip (`ht-strip ht-strip--mini`) of the last 15 ended results, oldest first, with no legend; `role="img"` and its counting `aria-label` ("Last 15 commitments: 12 met, 1 missed, 2 pending"; `components/ConsistencyStrip.md`, "Slice 6 state"). A goal with fewer ended results has fewer cells; one with none has an empty strip, "Last 0 commitments".
- Column 5 (`ht-goalitem__actions`), one action "Record" (`ht-btn--sm`, `aria-haspopup="dialog"`, `aria-label="Record for {goal name}"`), which opens the RecordDialog with its "For" select (`components/RecordDialog.md`, "Slice 5 state"):
  - primary (`ht-btn--primary`) when the goal has a Due commitment or period;
  - secondary (`ht-btn--secondary`) when it has only Pending ones;
  - absent when nothing can be recorded now. The empty column stays, so the columns line up.
- No "Add time", "+1", "Resume", "Start new attempt" or outcome Tag (slices 7 and 8).
- Every value comes from `ReportText` through the overview (HP-005); the item computes nothing.
