# Icons

The icon set is Lucide (version 0.460.0, ISC license): 24px line icons, drawn with `stroke-width` 1.75, round caps, `currentColor`.

- Markup: `<svg class="ht-icon" viewBox="0 0 24 24" aria-hidden="true">…paths…</svg>`. Sizes: `ht-icon--sm` 16px (inside inputs and small buttons), default 20px (menus, buttons), `ht-icon--lg` 24px (habit glyph tiles, empty states).
- The SVG files are in `design/icons/` (with Lucide's LICENSE). In Blazor either copy the SVG paths into a small `HtIcon` component (`Name` parameter) or use a Lucide Blazor package; keep stroke-width 1.75.
- Each name in this card has one meaning; use the same icon for the same thing everywhere.
- Catalog habits get one icon each (book-open = Reading…). Admins pick it from this list when they create a catalog habit.
- Icons in buttons are optional and always next to a word; an icon-only button (menu, close) needs `aria-label`.
