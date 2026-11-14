# Tabs

Tabs switch between views of the same page: catalog vs. my custom habits, profile sections, admin queues.

- Add a count in `span.ht-count` when it helps decide ("Suggestions 3").
- Use 2–5 tabs. Keep labels to one or two words.
- Each tab should have its own URL (for example `/custom-habits` for "My custom habits", `/profile?tab=custom-habits`) so links and the back button work.

## Two variants

- **Link tabs**, for tabs that change the address (each view is its own page or query, loaded by a normal link): `nav.ht-tabs` with an `aria-label`, and `a.ht-tab` links; the current one has `aria-current="page"` (accent underline). Use them when each view has its own address, for example the Catalog tabs "Public catalog" (`/catalog`) and "My custom habits" (`/custom-habits`), and Profile's "Account" and "Custom habits" (`/profile?tab=custom-habits`).
- **Button tabs**, for in-page tabs that switch content without a new address: `div.ht-tabs[role=tablist]` with `button.ht-tab[role=tab]`; the selected one has `aria-selected="true"` (accent underline).
