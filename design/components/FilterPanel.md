# FilterPanel

The left column of the catalog: category (single choice) and intent (Build / Reduce).

- Radio buttons for Category, checkboxes for Intent; signed-in users also get "Only habits I track".
- Use native inputs inside `label.ht-check` so the whole line is clickable; `accent-color` paints them `accent`.
- Filters apply at once (no Apply button) and are kept in the URL query.
- Under 860px the panel moves above the cards; later it can become a "Filters" button.
