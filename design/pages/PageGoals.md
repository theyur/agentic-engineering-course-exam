# PageGoals

The Goals list: Tabs Active · Paused · Ended, and one GoalListItem per goal.

- Page head: `heading-1` "Goals"; "+ New goal" is in the TopNav.
- Active: sorted by what needs action first (Pending, then due today), then by name.
- Paused tab: the item meta says "Paused until …" and the action is "Resume". Ended tab: outcome Tag (Achieved / Abandoned / No longer relevant) and "Start new attempt".
- No goals: EmptyState "No goals yet" with Browse the catalog / Create custom habit.

## Slice 4 state

The second part of the preview shows the page as slice 4 builds it (HP-003, ADR 0008). Today is Tue 17 Nov 2026 in the preview.

- `/goals` for registered users; an administrator gets the Not found page.
- Page head: `heading-1` "Goals" only; "+ New goal" is in the TopNav. No Tabs: Paused (slice 7) and Ended (slice 8) come later, and all goals are active.
- One `ht-list` of reduced GoalListItems (`components/GoalListItem`), in name order (slice 2 FR-004 comparison), then in the order they were created.
- Each item: the goal's name linking to its goal page (`/goals/{id}`), the Build or Reduce Tag, and the meta line: the schedule, then " · " and the first that applies of
  1. "first week starts Mon 23 Nov": a per-week goal whose first week has not begun, whatever its start date;
  2. "starts Tue 24 Nov": any other goal whose start date is after today (exactly when the list entry gives its start date, FR-014);
  3. "next today" or "next Wed 18 Nov": a commitment-based goal that has started;
  4. nothing (and no " · "): a per-day goal that has started, or a per-week goal in its weeks.
- The schedule: fixed days as "Every day" for all seven, otherwise the days in week order with runs of three or more as first–last, joined by " · " ("Mon · Wed · Fri", "Mon–Fri", "Mon–Wed · Sat"); N times per day "Every day" for 1 and "2 times a day"; N times per week "Flexible · once a week" or "Flexible · 3 times a week".
- No consistency, streak, strip, action or footnote (slices 5 and 6).
- No goals: the EmptyState "No goals yet", "Pick a habit from the catalog or create your own, then set a target and a schedule for it.", primary "Browse the catalog" (→ `/catalog`) and secondary "Create custom habit" (→ the custom habit form).
- Goal names in the preview follow FR-010: "Yoga, 3 days a week" for fixed days Mon, Wed and Fri (not "3 times a week", which is a period-based goal), "Coffee, at most 2 cups a day".
