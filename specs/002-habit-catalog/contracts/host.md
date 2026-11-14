# Contract: Host routes, pages and operator log of the habit catalog

**Feature**: specs/002-habit-catalog · **Plan**: [../plan.md](../plan.md)

What `HT.Ui` adds for browsers and operators in this slice. Verified by `HT.UiTests` (ADR 0006), not by
acceptance tests. Slice 1's routes, session and configuration are unchanged
([../../001-accounts-and-access/contracts/host.md](../../001-accounts-and-access/contracts/host.md)).

## Routes

| Path | Who | Render | Behaviour | Requirement |
|---|---|---|---|---|
| `/catalog` | anyone | interactive (prerendered) | the Public catalog: visitor, registered-user or administrator variant; query `q`, `category`, `intent` | HP-001 to HP-004, US1-AS10, US1-AS11, US4-AS12 |
| `/custom-habits` | registered users (`[Authorize]`) | interactive (prerendered) | "My custom habits": the Catalog page head and Tabs with the user's custom habits; an administrator gets Not found | HP-002, HP-007, US4-AS12, US4-AS15 |
| `/catalog/{id:guid}` | anyone | static | details of a Published habit; anything else is Not found | HP-005, HP-006, US2-AS6 to US2-AS8 |
| `/custom-habits/new` | registered users (`[Authorize]`) | interactive (prerendered) | the custom habit form; an administrator gets Not found | HP-007, US4-AS13, US4-AS15 |
| `/custom-habits/{id:guid}` | registered users (`[Authorize]`) | interactive (prerendered) | edit with "Delete"; another user's, a removed or an unknown habit, and an administrator, get Not found | HP-007, US4-AS13, US4-AS15 |
| `/admin` | administrators (`[Authorize]`) | interactive (prerendered) | the catalog table; query `q`; a registered user gets Not found | HP-009, HP-010, US3-AS20, US5-AS4 |
| `/admin/catalog/new` | administrators (`[Authorize]`) | interactive (prerendered) | the administrator's catalog habit form, create | HP-009, HP-010 |
| `/admin/catalog/{id:guid}` | administrators (`[Authorize]`) | interactive (prerendered) | the same form, edit, any state; unknown identifier is Not found | HP-009, HP-010 |
| `/profile` | signed in (slice 1) | static | registered users: Tabs "Account" and "Custom habits" (`?tab=custom-habits`); administrators unchanged | HP-008, US4-AS14 |
| `/` | anyone (slice 1) | static | visitor Home gains "Browse the catalog" and "Popular in the catalog"; a registered user's Home gains the EmptyState | HP-012, HP-013 |

- `[Authorize]` sends an anonymous visitor to `/sign-in?returnUrl=…` (slice 1).
- "Not found" is `NavigationManager.NotFound()`: the Router's `NotFoundPage` (slice 1's `NotFound`) with
  status 404, the same for every audience and every reason (research R-11).
- Query values: `category` is the lower-case category name, `intent` is `build` or `reduce`; an unknown value
  is ignored. Filter changes replace the address (`replace: true`); the search updates it 300 ms after the
  last keystroke.
- The Catalog Tabs ("Public catalog" → `/catalog`, "My custom habits" → `/custom-habits`) and Profile's Tabs
  are the link variant of Tabs added in the design step (research R-11).
- Interactive pages call every use case in a scope of its own (research R-15).

## Navigation (`SiteNav`)

| Audience | Added links | Requirement |
|---|---|---|
| anonymous visitor | "Catalog" | HP-011, US5-AS1 |
| registered user | "Catalog" | HP-011, US5-AS1 |
| administrator | "Catalog", "Admin" | HP-011, US5-AS1 |

Order and markup follow `design/components/TopNav` and `MobileNav` after the design step. No "Goals",
"Community" or "+ New goal".

## Page behaviour the tests check

- **Home**: the visitor Home's hero has the secondary button "Browse the catalog" (→ `/catalog`); after "How
  it works", "Popular in the catalog" shows `GetHomePreviewAsync`'s `Habits` as HabitCards and "See all N →"
  with N = `PublishedCount`; with no Published habit the section is absent (HP-012). A registered user's
  signed-in Home shows, under "Today", an EmptyState with "Browse the catalog"; an administrator's Home is
  unchanged (HP-013).
- **Catalog**: cards in `BrowseAsync` order; "Suggested starting goal: …" with the marked suggested starting goal's `Text`,
  no line without one; the count with "Sort: A–Z" as a plain label; "See details" (visitor) or "Details"
  (signed in); EmptyState with a clear-filters action when nothing matches (HP-004). Registered users: Tabs
  "Public catalog" and "My custom habits" with counts, "+ Create custom habit", custom-habit cards with the
  "Private" tag and an edit action, EmptyState when there are none. Administrators: no Tabs, no custom-habit
  button, no register Notice (HP-002).
- **Custom habit form**: one refusal at a time under its field; "This custom habit does not exist." as a
  Notice above the form with a link to "My custom habits", the form no longer saved; "Delete" opens a Dialog
  (focus on open, Esc and × close, focus back to "Delete"); after Save or Delete, back to `/custom-habits`
  (HP-007). The BackLink "← My custom habits" leads there too.
- **Administrator's form**: up to 5 suggested starting goals (add, remove, move up and down, mark the card's
  one) with the FR-007 controls (SegmentedControl, Stepper, DayPicker as the New goal form); a refusal with a
  position sits under that suggested starting goal's field (HP-009).
- **Admin table**: Habit, Category, Intent, Status (a Tag with the state's text, never color alone), Edit and
  Publish or Unpublish per row, the new state shown at once; "Showing N of M" (N habits listed, M `Total`).

## Operator log (category `HT.Ui.Operator.StartupTasks`)

Slice 1's events are unchanged. Added, for `Loaded`:

| Event | Level | Message template | Requirement |
|---|---|---|---|
| demo custom habits added | Information | `Demo custom habits {Email}: {Result}`, right after that email's `Demo account {Email}: {Result}` line, only for accounts with a custom-habit result | HP-017 |
| demo catalog | Information | `Demo catalog: {Result}`, once, after the account lines | HP-017 |

No line contains a custom-habit name or description (HP-015). For `NoPassword` and `PasswordRuleNotMet` the
only line is slice 1's `Demo loading: {Result}`.
