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
