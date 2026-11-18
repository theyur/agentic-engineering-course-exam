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
