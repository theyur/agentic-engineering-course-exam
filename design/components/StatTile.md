# StatTile

A tile with one big number and a short label, in a row of four at the top of the signed-in Home.

- `ht-card ht-tile` with `ht-tile__value` (`stat-large`) and `ht-tile__label` (small, muted).
- Colour the number only for Pending (`ht-tile--pending`) and Missed (`ht-tile--failed`); the label carries the meaning.
- Tiles are in-app indicators. Clicking a tile filters the Today list; there are no outbound reminders.
