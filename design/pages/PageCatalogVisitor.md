# PageCatalogVisitor

The catalog for visitors: search, a register Notice, FilterPanel on the left and a grid of HabitCards.

- Page head: `heading-1` "Habit catalog" + SearchField ("Search habits") on the right.
- The Notice "Ideas to start from. Register to turn any habit into your own goal." with "Register" is the one place that asks to register on this page.
- FilterPanel: Category as radio buttons, "All" first, then each category that has at least one Published habit, in the fixed order Body, Mind, Health, Money, Home, Other; Intent as the two checkboxes "Build" and "Reduce" (none or both checked shows both).
- Above the cards: the count of habits shown ("22 habits") and "Sort: A–Z" as a plain label, not a control. Name order is the only order.
- Cards show category, intent, a short description and "Suggested starting goal: …" with the text of the suggested starting goal marked for the card; a habit without one has no such line. The only action is "See details".
- Filters and the search apply at once (the search after a short pause while typing), with no Apply button, and are kept in the address (`/catalog?q=…&category=money&intent=reduce`).

## Slice 2 state

The page above is what slice 2 builds. The second part of the preview adds the state with no match (HP-004): an EmptyState "No habits match" in place of the cards, with "Clear filters", which empties the search and resets Category to "All" and Intent to none checked.
