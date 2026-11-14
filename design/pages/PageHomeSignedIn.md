# PageHomeSignedIn

The signed-in Home (Today): greeting, four StatTiles, the Today list and active goals.

- Page head: `heading-1` greeting + date. "+ New goal" lives in the TopNav.
- Left (wider) column: Today card with `CommitmentRow`s, Pending first. Right column: `GoalSummary` and Recent notes.
- Under 860px the columns stack: Today first.

## Slice 1 state

Slice 1 has no goals yet, so the signed-in Home shows only the page head with the heading "Today" (HP-013). No greeting and no date: a date would depend on the user's time zone, which slice 1 does not decide (HP-015). The tiles, lists and notes appear with the slices that build them (ADR 0008).

## Slice 2 state

The second part of the preview shows the registered user's Home as slice 2 builds it (HP-013, US5-AS3): the heading "Today", then an EmptyState in a Card: the `layout-grid` icon, "No goals yet", one sentence that points to the catalog and custom habits, and primary "Browse the catalog" (→ `/catalog`). An administrator's Home keeps the slice 1 state: the heading "Today" only.
