# PageCatalogSignedIn

The catalog for signed-in users: Public / My custom habits tabs, extra filter and goal actions on each card.

- "+ Create custom habit" (secondary) next to search; Tabs switch between public catalog and private custom habits.
- Habits already tracked show a strong "N goals" tag and "Add another goal"; others show primary "Use this habit".
- "Use this habit" opens the New goal page with the suggested starting goal filled in.

## Slice 2 state

The second part of the preview shows the page as slice 2 builds it (HP-002, ADR 0008).

- Registered user, Public catalog (`/catalog`): page head "Habit catalog" with SearchField and "+ Create custom habit" (→ the custom habit form); the link Tabs "Public catalog" (`/catalog`) and "My custom habits" (`/custom-habits`), each with a count; FilterPanel with Category and Intent only (no "Only habits I track"); the count and "Sort: A–Z"; cards with "Details" only. No goal tags ("N goals") and no goal actions ("Use this habit", "Add another goal"): those come with goals in slice 4.
- The counts on the Tabs: "Public catalog" counts every Published catalog habit, whatever the search and filters; "My custom habits" counts all of the user's own custom habits. The count above the cards ("22 habits") counts the habits shown after the search and filters.
- Registered user, My custom habits (`/custom-habits`): the same page head (without the SearchField) and Tabs, then the user's custom habits as HabitCards with the "Private" Tag and "Edit", in name order. With none, an EmptyState "No custom habits yet" with "+ Create custom habit".
- No match on the Public catalog: the EmptyState "No habits match" with "Clear filters", as on the visitor page.
- Administrator: the Public catalog without Tabs, without "+ Create custom habit" and without the register Notice; cards with "Details". An administrator has no custom habits, and `/custom-habits` is Not found for them.

## Slice 4 state

The last part of the preview shows the registered user's Public catalog as slice 4 builds it (HP-005, ADR 0008). Visitors and administrators keep their slice 2 state, and so does My custom habits.

- Cards: a habit with at least one of the user's goals has the strong Tag "1 goal" or "N goals" after its name, secondary "Add another goal" and ghost "Details"; every other card has primary "Use this habit" and ghost "Details". Both goal actions open the New goal page with the habit (`/goals/new?habit={id}&kind=catalog`).
- FilterPanel: Category, Intent, and "Show" with the checkbox "Only habits I track", kept in the address (`tracked=true`) with the other filters.
- The count: "22 habits · you track 2", where the second number counts the returned habits the user tracks.
- With "Only habits I track", only the tracked Published habits are listed, with the other filters still applied; with none, the EmptyState "No habits match" as in slice 2.
- Goal counts are the user's own only (FR-032).
