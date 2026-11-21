# PageHomeSignedIn

The signed-in Home (Today): greeting, four StatTiles, the Today list and active goals.

- Page head: `heading-1` greeting + date. "+ New goal" lives in the TopNav.
- Left (wider) column: Today card with `CommitmentRow`s, Pending first. Right column: `GoalSummary` and Recent notes.
- Under 860px the columns stack: Today first.

## Slice 1 state

Slice 1 has no goals yet, so the signed-in Home shows only the page head with the heading "Today" (HP-013). No greeting and no date: a date would depend on the user's time zone, which slice 1 does not decide (HP-015); slice 4 adds them. The tiles, lists and notes appear with the slices that build them (ADR 0008).

## Slice 2 state

The second part of the preview shows the registered user's Home as slice 2 builds it (HP-013, US5-AS3): the heading "Today", then an EmptyState in a Card: the `layout-grid` icon, "No goals yet", one sentence that points to the catalog and custom habits, and primary "Browse the catalog" (→ `/catalog`). An administrator's Home keeps the slice 1 state: the heading "Today" only.

## Slice 4 state

The last part of the preview shows the signed-in Home as slice 4 builds it (HP-010, FR-029, ADR 0008).

- Page head for registered users and administrators: the greeting as `heading-1` and today's date as the muted line, both at the current time in the account's time zone. It replaces the heading "Today".
  - The greeting is "Good morning" from 05:00 to 11:59, "Good afternoon" from 12:00 to 17:59 and "Good evening" from 18:00 to 04:59, followed by ", " and the account name when there is one: "Good evening, Yuri", or "Good evening" without a name.
  - The date is the weekday, the day and the month in English: "Tuesday, 17 November".
- A registered user with no goals: the EmptyState in a Card, with the `list-checks` icon, the title "No goals yet", the text "Pick a habit from the catalog or create your own, then set a target and a schedule for it.", primary "Browse the catalog" (→ `/catalog`) and secondary "Create custom habit" (→ the custom habit form). It replaces slice 2's "Goals come in a later version." EmptyState, and it is the same as the Goals page's.
- A registered user with goals: the page head only, until slice 6 adds the tiles and lists.
- An administrator: the page head only.

## Slice 6 state

The last part of the preview shows the signed-in Home as slice 6 builds it (HP-003, D-2, ADR 0008). Today is Wed 18 Nov 2026, 19:00 in the preview, in the account's time zone. The full preview's first tile reads "Due now" too, replacing "Due today".

- Page head: slice 4's greeting and date, unchanged.
- A registered user with goals gets, under the page head, one grid `ht-home`: the four StatTiles across both columns, then the Today card (left, wider) and the right column (`ht-stack`). Under 860px they stack: tiles, Today, then the right column.
  - The tiles and the Today card are one interactive island, `ht-home__panel` (`display:contents`, so its two parts still sit in the grid). The right column is static.
- StatTiles (`components/StatTile.md`, "Slice 6 state"): "Due now", "Pending · still can record", "Missed this week" (buttons with `aria-pressed` that filter the Today list) and "Active goals" (a link to `/goals`). "This week" is Monday to Sunday in the account's time zone.
- Today card (`section.ht-card`, title "Today"): one CommitmentRow per entry (`components/CommitmentRow.md`, "Slice 6 state"): the Pending rows, then the Due rows, then the rows Missed this week, then the Upcoming rows. With a tile pressed, only that state's rows (no Upcoming rows). With no row: "Nothing to record right now.".
  - "Record" and "Edit" open the RecordDialog fixed to the row's commitment or period; "Add note" and "Edit note" open the NoteDialog. After a save the island reads the list again, and the focus returns to the row's action.
  - In the preview: three Pending rows of Mon 16 Nov (5 h left) and two of Tue 17 Nov (29 h left); four Due rows (a Reduce count day with its bar, a duration commitment with its bar, a yes/no commitment with its schedule, a week with its progress); two rows Missed this week (Sun 15 Nov and the week 9 – 15 Nov, whose grace periods ended Wed 18 Nov 00:00); and one Upcoming row (a goal whose first week starts Mon 23 Nov).
- Right column:
  1. the "Active goals" card (`components/GoalSummary.md`, "Slice 6 state"): at most 5 goals, each name a link, its consistency as one whole text, its streak line; then "All goals →";
  2. the "Recent notes" card: at most 5 notes across the user's goals, those of the latest commitments or periods first (notes of the same date in goal order), each an `ht-goal` item with the note in quotes and "{goal name} · {date}" under it (`ht-goal__meta`). Absent when the user has no note.
- The second example shows "Missed this week" pressed: the Today card has only the two Missed rows.
- A registered user with no goals keeps slice 4's EmptyState; an administrator keeps the page head only.
- No "Excuse" (slice 7) and no reminder of any kind: the tiles and rows are in-app indicators only.

## Slice 7 state

The last part of the preview shows the signed-in Home as slice 7 builds it (HP-005, FR-019, US1-AS12, US4, ADR 0008). Today is Wed 18 Nov 2026, 19:00 in the preview. The user has six goals; "Meditation, 10 min daily" is paused from Mon 16 until Sun 22 Nov.

- Everything of slice 6 stays: the page head, the four StatTiles, the Today card with its order, filters and dialogs, the "Active goals" card and the "Recent notes" card. Two changes:
  1. Today card: a Pending row of a commitment-based goal whose commitment has no record gets the ghost "Excuse" after its "Record" (`components/CommitmentRow.md`, "Slice 7 state"); it opens the Excuse dialog (`components/ExcuseDialog.md`). After "excused" the row leaves the Today list and the "Pending · still can record" tile drops by 1. In the preview: "Reading, 20 min daily", Mon 16 Nov.
  2. "Active goals": the tile counts only the goals that are not paused (`Counts.Goals`), and the card lists only those (at most 5, as in slice 6). In the preview the tile says 5, and Meditation is not in the card.
- Excused and Paused entries are not rows and are not counted in "Due now", "Pending · still can record" or "Missed this week" (FR-019). In the preview Meditation has no row: its days from Mon 16 Nov are Paused. A paused goal's Pending results from before its pause are still listed (spec US4-AS2).
- The "Recent notes" card is unchanged: a note of an Excused or Paused commitment or period is listed as any other note.
- A registered user with no goals keeps slice 4's EmptyState; an administrator keeps the page head only. Still no reminder of any kind. A user whose goals are all paused has goals: the dashboard, with "Active goals" at 0 and no goal in the card (settled here, T078).
