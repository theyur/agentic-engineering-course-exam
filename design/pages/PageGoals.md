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

## Slice 6 state

The last part of the preview shows the page as slice 6 builds it (HP-002, ADR 0008). Today is Tue 17 Nov 2026 in the preview.

- `/goals` for registered users; an administrator gets the Not found page, as in slice 4.
- Page head: `heading-1` "Goals" only. Still no Tabs: Paused (slice 7) and Ended (slice 8) come later.
- One `ht-list` of full GoalListItems (`components/GoalListItem.md`, "Slice 6 state"), in this order:
  1. the goals with a Pending commitment or period;
  2. then the goals with a Due one;
  3. then the others;

  within each group by name (slice 2 FR-004 comparison), then in the order they were created. In the preview: Coffee, Reading, Running and Yoga have Pending results; Strength training is only Due today; Meditation starts next week.
- Each item: the name link, the Build or Reduce Tag, the Pending StatusChip, slice 4's meta line plus the progress of a Due entry with a record, "Consistency" and "Current streak" as whole texts, the mini strip of the last 15 results and "Record" as primary, as secondary or absent.
- Under the list, the footnote (`ht-footnote`): "Each goal is counted on its own. Consistency counts only finalized results; pending ones are left out." Slice 7 adds "excused and paused".
- After a record is saved in the RecordDialog, the list is read again, and the focus returns to that item's "Record" (or its name link when "Record" is gone).
- No goals: slice 4's EmptyState "No goals yet", unchanged, with no footnote.

## Slice 7 state

The last part of the preview shows the page as slice 7 builds it (HP-004, US4-AS5, US4-AS6, ADR 0008). Today is Mon 16 Nov 2026, 10:00 in the preview (spec US4): "Meditation, 10 min daily" is active, and "Drinking water, 8 glasses a day" is paused from Mon 16 until Sun 22 Nov.

- `/goals` for registered users; an administrator gets the Not found page, as in slices 4 and 6.
- Page head: `heading-1` "Goals" only.
- Under it, the link Tabs (`components/Tabs.md`, link variant): `nav.ht-tabs` with `aria-label="Goal lists"`, then "Active" (`/goals`) and "Paused" (`/goals?tab=paused`); the current one has `aria-current="page"`. No "Ended" (slice 8). No counts (settled here, T007: HP-004 names none, and the page shell reads no data). `?tab=paused` shows Paused; any other value, or none, shows Active.
- Active: one `ht-list` of the goals that are not paused, as slice 6 lists them (the same items, the same order). When every goal is paused, the Active tab has no list, only the Tabs and the footnote (settled here, T077).
- Paused: one `ht-list` of the paused goals (`PausedUntil`), in the overview's order (slice 6's), each in the Paused form of the GoalListItem (`components/GoalListItem.md`, "Slice 7 state"): the name link, the Build or Reduce Tag, the Pending StatusChip when it has a Pending result, the meta "Paused until Sun 22 Nov", its figures and mini strip, and the secondary "Resume".
- An empty Paused tab: a Card with the sentence "No paused goals." (`ht-card__sub`, as the Home's "Nothing to record right now."; settled here, T007).
- Under the list, on either tab, the footnote (`ht-footnote`): "Each goal is counted on its own. Consistency counts only finalized results; pending, excused and paused ones are left out." It replaces slice 6's footnote.
- After "Resume", the list is read again and the goal moves to the Active tab; after a record is saved on the Active tab, slice 6's behaviour.
- No goals: slice 4's EmptyState "No goals yet", unchanged, in place of the tabs, with no footnote.
