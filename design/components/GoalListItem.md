# GoalListItem

One goal in the Goals list: name and schedule, consistency, current streak, the last 15 results and the next action.

- Wrap items in `ht-list` (one card); each item is `ht-goalitem` (5 columns; 2 columns under 860px).
- Name links to the goal page. Meta line: intent Tag, schedule and today's state ("12 of 20 min today", "missed Tue"); add a Pending StatusChip when something is waiting.
- Consistency with count, or N/A; streak with its unit; mini ConsistencyStrip (`ht-strip--mini`).
- One action: Record (primary) when something is due, otherwise a secondary button.
