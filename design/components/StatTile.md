# StatTile

A tile with one big number and a short label, in a row of four at the top of the signed-in Home.

- `ht-card ht-tile` with `ht-tile__value` (`stat-large`) and `ht-tile__label` (small, muted).
- Colour the number only for Pending (`ht-tile--pending`) and Missed (`ht-tile--failed`); the label carries the meaning.
- Tiles are in-app indicators. Clicking a tile filters the Today list; there are no outbound reminders.

## Slice 6 state

The last part of the preview shows the tiles as slice 6 builds them on the signed-in Home (HP-003, HP-006, ADR 0008). One Razor component, `StatTile`, renders both forms. The full preview's first tile reads "Due now" too, replacing "Due today".

| Tile | Value | Classes | Form |
|---|---|---|---|
| Due now | the Due commitments, days and weeks (their rows read "Due today" or "Due this week") | `ht-card ht-tile` | button |
| Pending · still can record | the Pending ones | `ht-card ht-tile ht-tile--pending` | button |
| Missed this week | the ones that became Missed this week (their grace period ended at or after Monday 00:00 in the account's time zone) | `ht-card ht-tile ht-tile--failed` | button |
| Active goals | the user's goals | `ht-card ht-tile` | link to `/goals` |

- A button tile is `<button type="button" class="ht-card ht-tile …" aria-pressed="false">` with two `span`s, `ht-tile__value` then `ht-tile__label` (a button holds no `div`). Choosing it filters the Today list to that state and sets `aria-pressed="true"`. Choosing it again turns the filter off and shows all rows; choosing another tile moves the filter there, so at most one tile is pressed. Under a filter the Today list has no Upcoming rows.
- The pressed tile has a 2px accent frame (`.ht-tile[aria-pressed="true"]`), so its state is seen as a shape, not as a fill colour, and is announced through `aria-pressed`. A hovered tile gets the `line-strong` border.
- "Active goals" is `<a class="ht-card ht-tile" href="/goals">` with the same two `span`s.
- Tiles are reached with Tab and work with Enter and Space (native button and link behaviour); the focus ring is the usual 2px `--accent` outline.
- Upcoming entries are not counted in any tile.
- The labels are exact: the smoke check reads `ht-tile__label`.
