# HabitCard

A catalog habit: line icon, name, category and intent tags, the description, the marked suggested starting goal, and actions.

## Actions by audience
- Visitor: ghost "See details" only (the page Notice invites to register).
- Signed in, not tracked: primary "Use this habit" + ghost "Details" (slice 4).
- Signed in, already tracked: a strong tag "2 goals" next to the name and secondary "Add another goal" (slice 4).

## Rules
- The suggested starting goal is an example: write "Suggested starting goal: **15 min, 5 days a week**" with the text of the suggested starting goal marked for the card. On the detail page the list says "Examples to adjust, not prescribed targets."
- The text follows the spec's rules for suggested starting goals (slice 2, FR-017): "15 min, 5 days a week", "20 pages, 3 times a week", "at most 2 h a day", "once a day". A habit without a suggested starting goal has no such line at all.
- Category and intent are Tags with their words ("Mind", "Build"); never colour alone.
- Glyph: the habit's Lucide icon from the "Habit icons" group (`ht-icon--lg`, see Icons) on `accent-soft` in a 44px `radius-sm` tile. No photos, no emoji.

## Custom habit card
- The same card, with a strong "Private" Tag after the category and intent Tags: the word says the habit is private, not a colour.
- No suggested starting goal line (custom habits have none).
- One ghost action, "Edit", which opens the custom habit form. Its accessible name includes the habit's name ("Edit Cold shower").
- "Suggest for catalog" is a later ghost action (slice 10).

## Slice 2 state

The second part of the preview shows the card as slice 2 builds it (ADR 0008):
- the visitor catalog card with "See details";
- the signed-in catalog card with "Details" only: no "Use this habit", no "Add another goal", no "N goals" (slice 4);
- a catalog habit without a suggested starting goal: no "Suggested starting goal" line;
- the custom-habit card with the "Private" Tag and "Edit", without "Suggest for catalog".
