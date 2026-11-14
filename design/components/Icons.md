# Icons

The icon set is Lucide (version 0.460.0, ISC license): 24px line icons, drawn with `stroke-width` 1.75, round caps, `currentColor`.

- Markup: `<svg class="ht-icon" viewBox="0 0 24 24" aria-hidden="true">…paths…</svg>`. Sizes: `ht-icon--sm` 16px (inside inputs and small buttons), default 20px (menus, buttons), `ht-icon--lg` 24px (habit glyph tiles, empty states).
- The SVG files are in `design/icons/` (with Lucide's LICENSE). In Blazor either copy the SVG paths into a small `HtIcon` component (`Name` parameter) or use a Lucide Blazor package; keep stroke-width 1.75.
- Each name in this card has one meaning; use the same icon for the same thing everywhere.
- Two groups. **Habit icons**: the 24 icons a catalog or custom habit can have, in this order: book-open, person-standing, smartphone, footprints, piggy-bank, candy, coffee, moon, dumbbell, droplet, bike, brain, pen-line, languages, music, salad, utensils, cigarette-off, wine-off, wallet, receipt, sprout, sparkles, tv. The icon picker of the custom habit form and of the administrator's catalog habit form offers exactly this group, in this order. **App icons**: every other icon, for navigation and actions; none of them is a habit icon.
- A habit has one icon from the Habit icons group (book-open for Reading…). Several habits may use the same icon; the meaning under each icon is an example.
- Icons in buttons are optional and always next to a word; an icon-only button (menu, close) needs `aria-label`.
