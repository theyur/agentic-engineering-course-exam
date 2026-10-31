# Tabs

Tabs switch between views of the same page: catalog vs. my custom habits, profile sections, admin queues.

- `div.ht-tabs[role=tablist]` with `button.ht-tab[role=tab]`; the selected one has `aria-selected="true"` (accent underline).
- Add a count in `span.ht-count` when it helps decide ("Suggestions 3").
- Use 2–5 tabs. Keep labels to one or two words.
- Each tab should have its own URL (for example `/catalog?view=mine`) so links and the back button work.
