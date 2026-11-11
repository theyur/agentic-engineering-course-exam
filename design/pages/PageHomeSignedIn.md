# PageHomeSignedIn

The signed-in Home (Today): greeting, four StatTiles, the Today list and active goals.

- Page head: `heading-1` greeting + date. "+ New goal" lives in the TopNav.
- Left (wider) column: Today card with `CommitmentRow`s, Pending first. Right column: `GoalSummary` and Recent notes.
- Under 860px the columns stack: Today first.

## Slice 1 state

Slice 1 has no goals yet, so the signed-in Home shows only the page head with the heading "Today" (HP-013). No greeting and no date: a date would depend on the user's time zone, which slice 1 does not decide (HP-015). The tiles, lists and notes appear with the slices that build them (ADR 0008).
