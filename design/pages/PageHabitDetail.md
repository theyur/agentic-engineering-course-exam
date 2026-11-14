# PageHabitDetail

The catalog habit page, opened from "See details" / "Details" on a HabitCard.

- Head: BackLink, large glyph, `heading-1` name, tags; signed-in users see "You track this · N goals" and primary "Use this habit".
- Lead paragraph: the description (neutral, no coaching or health claims).
- Left: Suggested starting goals (each "Start with this" opens New goal pre-filled) and Your goals on this habit (signed in only).
- Right: community performance with contributor count, or "Insufficient community data".
- Visitor: no "Your goals" card; the head button is "Register to use this habit".
- Each suggested starting goal shows its text and its kind, written by the spec's rules (slice 2, FR-017): "15 min, 5 days a week" with "Duration · fixed days Mon–Fri"; "20 pages, 3 times a week" with "Count · flexible". A schedule is fixed days, or a number of times per day or per week; there is no monthly period.

## Slice 2 state

The second part of the preview shows the page as slice 2 builds it (US2-AS6, US2-AS7, ADR 0008).

- "← Catalog", the large glyph, the name as `heading-1`, the category and intent Tags, and the description.
- "Suggested starting goals" with the line "Examples to adjust, not prescribed targets.", then each suggested starting goal in the administrator's order, with its text and its kind. No "Start with this" (slice 4).
- Visitors: the head button "Register to use this habit" (→ Register). Registered users and administrators: no head button.
- No "You track this · N goals", no "Your goals on this habit" (slice 4), no community card (slice 9).
- A habit without suggested starting goals has no "Suggested starting goals" card.
- A Draft, an Unpublished or an unknown habit's address shows the Not found page (`components/EmptyState`), for every audience.
