# DataTable

A table for admin lists (catalog entries, suggestions, feedback, accounts) and for goal history.

- Wrap in `ht-table-wrap` (card look, scrolls sideways on small screens).
- Header: `eyebrow` style. First column is the name in bold. Actions are right-aligned in `td.ht-actions`, small buttons, ghost first.
- States in tables use `Tag` (Published, Draft) — not status chips.
- Admin tables never show private goals, records or notes.
