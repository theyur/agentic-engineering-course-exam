# PageAdmin

The admin page: privacy Notice, Tabs for Catalog, Suggestions, Feedback and Accounts, each a DataTable.

- The neutral privacy Notice is always visible above the tabs.
- Catalog tab: "+ New catalog habit" primary; rows with Published / Draft tags and Edit, Publish / Unpublish.
- Accounts tab: Deactivate / Reactivate as `ht-btn--danger` / secondary with confirmation. No access to private data anywhere.

## Slice 2 state

The second part of the preview shows the page as slice 2 builds it (US3-AS20, HP-009, ADR 0008).

- Heading "Administration" with primary "+ New catalog habit" (→ the administrator's catalog habit form, `PageAdminCatalogHabit`), then the neutral privacy Notice.
- One section only, the catalog, so there are no Tabs: Suggestions (slice 10), Feedback and Accounts (slice 11) come later.
- SearchField "Search catalog" and "Showing N of M": N is the number of habits listed after the search, M the number of all catalog habits.
- The table: Habit, Category, Intent, Status, and the row actions. No "Updated" column until time zones are decided (slice 4).
- Status is a Tag with its word: "Draft" and "Unpublished" neutral, "Published" strong. Never colour alone.
- Row actions: ghost "Edit" (→ the form), then primary "Publish" for a Draft or Unpublished habit, or secondary "Unpublish" for a Published one. Each action's accessible name includes the habit's name ("Publish Cold shower"). Publish and Unpublish act at once and the row shows the new state; there is no confirmation in this slice.
- Rows are in name order, every state mixed.
