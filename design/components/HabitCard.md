# HabitCard

A catalog habit: line icon, name, category and intent tags, the suggested starting goal, and actions.

## Actions by audience
- Visitor: ghost "See details" only (the page Notice invites to register).
- Signed in, not tracked: primary "Use this habit" + ghost "Details".
- Signed in, already tracked: a strong tag "2 goals" next to the name and secondary "Add another goal".

## Rules
- The suggested start is an example: write "Suggested start: **15 min, 5 days a week**" and, on the detail page, "An example to adjust, not a prescribed target."
- Glyph: the habit's Lucide icon (`ht-icon--lg`, see Icons) on `accent-soft` in a 44px `radius-sm` tile. No photos, no emoji.
- Custom habits use the same card with a "Private" tag and "Suggest for catalog" as a ghost action (later feature).
