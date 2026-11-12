# Feature Specification: Habit catalog

**Feature Branch**: `claude/speckit-specify-slice-2-habits-461103`

**Created**: 2026-11-12

**Status**: Draft

**Input**: User description: "slice 2 habit catalog"

Roadmap outcome (slice 2, `habit-catalog`): a new user does not start from a blank page. Anyone can browse
curated catalog habits by category, with suggested starting goals; administrators curate them; users keep
private custom habits (SPEC §3, §4 without Suggest for catalog, §8 catalog part). The confirmed
[idea brief](../../docs/features/habit-catalog/idea-brief.md) (Approach C) is the starting point, minus the
account foundation that slice 1 now carries.

## Clarifications

### Session 2026-11-12

- Q: One or several suggested starting goals per catalog habit? The idea brief says at most one; SPEC §4 is
  plural and SPEC §2 singular; `design/pages/PageHabitDetail` shows three and `HabitCard` one per card. → A:
  Several, in the administrator's order, with exactly one marked as the one the catalog card shows. Detail:
  [ADR 0011](../../docs/adr/0011-suggested-starting-goals.md).
- Q: What does a suggested starting goal store, given that slice 4 offers it as a starting point for a goal?
  → A: Structured fields that match the New goal form of `design/pages.md`: Build or Reduce; the measurement
  (yes/no, count or duration); for count and duration the amount and unit; and the schedule, either fixed days
  of the week or N times per day or per week. The shown text ("15 min, 5 days a week") is produced from the
  fields. How a goal is evaluated stays slice 4's decision. Detail:
  [ADR 0011](../../docs/adr/0011-suggested-starting-goals.md).
- Q: How do goals behave when their catalog habit is edited or unpublished? → A: Goals keep working and always
  show the habit's current name and details; unpublishing only takes the habit out of the public catalog.
  Unpublishing a habit that goals use first asks the administrator to confirm, showing how many goals use it
  and nothing about whose goals they are. No goal exists in this slice; slice 4 builds the confirmation and
  the goal side. Detail: [ADR 0012](../../docs/adr/0012-goals-on-changed-catalog-habit.md).
- Q: Does the catalog page have a name search, a Build/Reduce filter and A–Z order, as the design shows, though
  the idea brief says "search can wait"? → A: Yes, all three as the design shows. A–Z is the only order, so
  "Sort: A–Z" is a label, not a control.
- Q: Can an administrator account have its own custom habits? → A: No. Custom habits are for registered users;
  every custom-habit use case refuses an administrator with "Custom habits are for registered users.". Slice 4
  reopens this only if it lets administrators track goals.
- Q: Who writes the seed catalog content? → A: Claude drafts it in this slice (at least 20 Published habits in
  at least 4 categories, plus one Draft and one Unpublished), following `design/brand-book.md` and HP-014; the
  owner reviews it in the PR. (FR-030 later widened the seed to all six categories; see the critic revision
  below.)
- Q: Which four habits does the visitor Home show under "Popular in the catalog", with no usage data yet? → A:
  The first four Published habits in name order (fewer when fewer exist); the heading stays "Popular in the
  catalog" as the design shows.
- Q: Must every habit have a category, given that SPEC §4 says the catalog is "optionally categorized"? → A:
  Yes. Every catalog habit and custom habit has exactly one category from the fixed list Body, Mind, Health,
  Money, Home, Other; a missing category is refused with "Choose a category.". "Other" is the catch-all, and
  the filter has no "Uncategorized" choice.

### Critic revision 2026-11-12

Decisions made while revising after [critic pass 1](critic-spec-pass-1.md) and
[critic pass 2](critic-spec-pass-2.md). They fill gaps the owner's answers left open and stay within the
recorded decisions; the owner confirmed them on 2026-11-12, before `/speckit-ac-translate`.

- The seed catalog is demo data (SPEC §10, constitution VI), so demo loading loads it, as ADR 0007 says later
  slices extend demo loading; there is no separate startup task (FR-030). It is created as a whole or not at
  all.
- Demo loading creates a demo account's custom habits only in the run that creates that account, never for an
  account that already exists, and never reads or changes any other private data (FR-031). Detail:
  [ADR 0013](../../docs/adr/0013-private-demo-data.md).
- The habit icons are a fixed list of 24 Lucide names (FR-003); the design's Icons card gains a "Habit icons"
  group first.
- Every use case checks in one stated order (FR-016); unpublishing an Unpublished habit is a success (FR-011).
- The text and kind of a suggested starting goal follow fixed rules (FR-017), and the card label becomes
  "Suggested starting goal: …" (constitution I).
- Names are compared ordinally, ignoring case (FR-004).
- A count unit is stored trimmed; "as entered" means its spelling and case only (FR-007; owner, after
  [critic ac pass 2](critic-ac-pass-2.md) MINOR-1).
- The administrator's catalog table keeps its search and "Showing N of M" line (FR-012).

## Decisions this slice cites

Recorded in earlier slices and not decided again here:

- Use cases take the acting user as an explicit parameter and return named outcomes that carry the literal
  text this spec gives; expected refusals are outcomes, not errors
  ([ADR 0005](../../docs/adr/0005-use-case-convention.md)). An anonymous visitor who calls a use case for
  signed-in users gets "You are not signed in." (slice 1, FR-011).
- The administrator role grants no access to private data (slice 1, FR-015; SPEC §3, §8).
- Partly built pages show only the sections and links whose slice has shipped; sentences keep the wording of
  the previews ([ADR 0008](../../docs/adr/0008-partly-built-pages.md)).
- At every start the host runs administrator creation and then demo loading; later slices extend demo
  loading with their demo data, re-runnable and visibly synthetic
  ([ADR 0007](../../docs/adr/0007-startup-operator-tasks.md)). Demo loading creates the demo accounts
  `demo1@example.com` to `demo3@example.com` (slice 1, FR-016).
- Integration and UI tests: [ADR 0006](../../docs/adr/0006-integration-tests.md). Acceptance tests:
  [ADR 0003](../../docs/adr/0003-acceptance-tests.md).
- No time-dependent text until slice 4 (`goals`) decides time zones (slice 1, Clarifications).

## Slice 1 behaviour these tests use

The scenarios of this slice set up their acting users and demo data with slice 1's use cases
(`specs/001-accounts-and-access/spec.md`). Restated here, unchanged, so that this spec alone gives the rules
and texts its acceptance tests depend on:

- **Password rule** (slice 1 FR-002): every password the system sets has at least 8 characters and contains at
  least one digit; otherwise the result is "At least 8 characters, with a number." (`walking42` and
  `admin2026` meet it; `walk4` does not).
- **Registration** (slice 1 FR-001, FR-003, FR-004): an anonymous visitor creates an account with an email and
  a password that meets the password rule. The email is kept trimmed and in lower case, and is unique. A new
  account has the role registered user.
- **Sign-in** (slice 1 FR-005 to FR-007): signing in with an account's email (matched ignoring case and
  surrounding spaces) and its password makes that account, with its role, the acting user. Every failed
  sign-in gives "Email or password is not correct."
- **Administrator creation** (slice 1 FR-014): an operator capability that takes an email and a password. For
  an email that has no account and a password that meets the password rule, the result is "administrator
  created" and an account with the role administrator exists. Its other results are "no administrator
  configured", "configured email is not valid", "administrator already exists", "email belongs to a
  registered user" and "configured password does not meet the password rule", in that order of checks.
- **Demo loading** (slice 1 FR-016): an operator capability that takes a demo password. With no demo password
  (absent, empty or only spaces) the result is "no demo password configured"; with one that fails the password
  rule, "demo password does not meet the password rule"; in both cases nothing is created. Otherwise, for each
  of the demo emails `demo1@example.com`, `demo2@example.com` and `demo3@example.com`, the account result is
  "created" (a registered user with the demo password as its password is created) or "already exists" (the
  existing account, of either role, is left unchanged). This slice adds the catalog result and the custom-habit
  results (FR-030, FR-031).

## What acceptance tests can reach

ADR 0003 limits acceptance tests to the Application use cases, the two `HT.Persistence` entry points and the
Domain types they use. As in slice 1, this spec splits its requirements:

- **FR-NNN** are application behaviours, reachable through Application use cases, so acceptance tests can
  verify them. Demo loading (FR-030, FR-031) is an operator capability: no page offers it, the host runs it at
  start (HP-017), and acceptance tests call it directly.
- **HP-NNN** are host, page and integration requirements, verified by unit, integration and selective UI
  tests, not by acceptance tests.
- Scenarios marked *(page)* describe page, browser or host behaviour. They are expected to be listed as "Not
  testable through the API" in `acceptance.md`, with this section as the reason.
- FR-015 has no scenario in this slice: no goal exists yet, so what it promises can only be verified in slice
  4. It is listed so that slice 4 cites it.

Test data: all acceptance scenarios of every feature share one database in one run (ADR 0003), and nothing in
this slice removes a catalog habit. Slice 1's locked demo scenarios reset that database and run demo loading
with a valid demo password, which after this slice also loads the seed catalog and the demo custom habits, at
points of the run this feature does not control. So:

- Most scenarios work in whatever state the run has: they use catalog-habit names, custom-habit names and
  account emails of their own, and check results by inclusion, exclusion and relative order, never by
  catalog totals.
- The scenarios that need a known state start, as slice 1's demo scenarios do, from a reset database (the
  reset entry point of ADR 0003) and arrange everything they need after it: US1-AS9 (no Published habit in
  Home), US6-AS1 to US6-AS4 (an empty or known catalog) and US6-AS5 to US6-AS8 (known demo accounts).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Browse the catalog (Priority: P1)

Anyone, signed in or not, browses the published catalog habits: name, category, Build or Reduce, a short
description and the card's suggested starting goal. They narrow the list by category, by intent and by a
search on the name. Draft and unpublished habits, and every custom habit, never appear.

**Why this priority**: This is the blank-page answer and the first thing a reviewer meets (idea brief §2,
§4). Without it the slice delivers nothing visible.

**Independent Test**: With a few published, draft and unpublished catalog habits in different categories,
browse as an anonymous visitor and confirm that only the published ones appear, in name order, and that each
filter narrows the list.

**Acceptance Scenarios**:

1. **Given** published catalog habits "Ac1 Reading" (Mind, Build) and "Ac1 Yoga" (Body, Build), a draft
   "Ac1 Journaling" and an unpublished "Ac1 Stretching", **When** an anonymous visitor browses the catalog,
   **Then** the result contains "Ac1 Reading" and "Ac1 Yoga", each with its category, intent, icon,
   description and the suggested starting goal marked for its card, and contains neither "Ac1 Journaling" nor
   "Ac1 Stretching".
2. **Given** published catalog habits "Ac2 walking", "Ac2 Breathing" and "Ac2 Cycling", **When** anyone
   browses the catalog, **Then** they appear in the order "Ac2 Breathing", "Ac2 Cycling", "Ac2 walking" (name
   order, ignoring case).
3. **Given** published catalog habits "Ac3 Saving" (Money) and "Ac3 Running" (Body), **When** a visitor browses
   with the category Money, **Then** the result contains "Ac3 Saving" and not "Ac3 Running".
4. **Given** published catalog habits "Ac4 Sugar" (Health, Reduce) and "Ac4 Water" (Health, Build), **When** a
   visitor browses with the intent Reduce, **Then** the result contains "Ac4 Sugar" and not "Ac4 Water".
5. **Given** published catalog habits "Ac5 Morning pages" and "Ac5 Evening walk", **When** a visitor searches
   for " morning ", **Then** the result contains "Ac5 Morning pages" and not "Ac5 Evening walk" (trimmed,
   ignoring case, part of the name).
6. **Given** a registered user who owns the custom habit "Ac6 Cold shower" and a published catalog habit
   "Ac6 Cold water", **When** that user, another registered user, an administrator or an anonymous visitor
   browses or searches the catalog for "Ac6", **Then** the result contains "Ac6 Cold water" and never
   "Ac6 Cold shower".
7. **Given** a search and filters that match no published habit, **When** a visitor browses, **Then** the
   result is empty with a count of 0.
8. **Given** published catalog habits "Ac26 Lifting" (Body) and "Ac26 Budget" (Money), **When** anyone asks for
   the category filter, **Then** it starts with "All" and contains "Body" before "Money" (FR-002 order).
9. **Given** a reset database with only a Published "Ac27 Lifting" in Body and a draft "Ac27 Tidying" in
   Home, **When** anyone asks for the category filter, **Then** it is "All", "Body" and does not contain
   "Home".
10. *(page)* **Given** an anonymous visitor on the Catalog page, **When** it opens, **Then** it follows
    `design/pages/PageCatalogVisitor`: heading "Habit catalog", search field, the register Notice, the filter
    panel, the count with the label "Sort: A–Z", and a card per habit with "See details".
11. *(page)* **Given** a visitor who picks a category and an intent, **When** the filters change, **Then** the
    cards update at once without an Apply button, and the filters are kept in the page address.

---

### User Story 2 - Catalog habit details (Priority: P1)

Anyone opens a published catalog habit and sees its name, tags, description and suggested starting goals,
each presented as an example to adjust, not a prescribed target.

**Why this priority**: The detail page is where the suggested starting goals are shown in full; it completes
the browsing journey and is the entry point that slice 4 later extends with "Use this habit".

**Independent Test**: Open a published habit and confirm its full content; try to open a draft, an
unpublished and a non-existent habit and confirm that all three give the same "not in the catalog" result.

**Acceptance Scenarios**:

1. **Given** a published catalog habit "Ac7 Reading" with a description and three suggested starting goals —
   Build, duration 15 minutes on the fixed days Monday to Friday; Build, count 20 "pages" 3 times per week
   (marked for the card); Build, yes/no once per day — **When** anyone reads its details, **Then** the result
   has its name, category, intent, icon, description and all three suggested starting goals in that order,
   with their fields, the marked one identified, and the texts and kinds "15 min, 5 days a week" / "Duration ·
   fixed days Mon–Fri", "20 pages, 3 times a week" / "Count · flexible" and "once a day" / "Yes/no ·
   flexible" (FR-017).
2. **Given** a published catalog habit "Ac7b Walking" with no suggested starting goal, **When** anyone browses
   the catalog or reads its details, **Then** it is shown with no suggested starting goal.
3. **Given** published catalog habits whose suggested starting goals are: Build, duration 30 minutes on all
   seven days; Reduce, duration 2 hours 1 time per day; Reduce, count 1 "sweet" 1 time per day; Build, yes/no
   1 time per week; Build, duration 1 hour on the fixed days Monday, Wednesday and Friday; Build, count 10
   "pages" on the fixed day Saturday; Reduce, yes/no 1 time per day; Build, yes/no on the fixed days Monday,
   Tuesday, Wednesday and Saturday; Build, yes/no on the fixed days Saturday and Sunday, **When** anyone reads
   them, **Then** the texts are "30 min daily", "at most 2 h a day", "at most 1 sweet a day", "once a week",
   "1 h, 3 days a week", "10 pages, once a week", "at most once a day", "4 days a week" and "2 days a week",
   and the kinds are "Duration · fixed days every day", "Duration · flexible", "Count · flexible", "Yes/no ·
   flexible", "Duration · fixed days Mon, Wed, Fri", "Count · fixed days Sat", "Yes/no · flexible", "Yes/no
   · fixed days Mon–Wed, Sat" and "Yes/no · fixed days Sat, Sun".
4. **Given** a draft "Ac8 Draft habit", an unpublished "Ac8 Old habit" and an identifier that belongs to no
   catalog habit, **When** an anonymous visitor, a registered user or an administrator reads the details of
   each through the public catalog, **Then** each result is "This habit is not in the catalog."
5. **Given** a custom habit "Ac9 Private one" of a registered user, **When** anyone reads catalog details with
   that custom habit's identifier, **Then** the result is "This habit is not in the catalog."
6. *(page)* **Given** a visitor on the detail page, **When** it opens, **Then** it follows
   `design/pages/PageHabitDetail` for its shipped parts: "← Catalog", the glyph, name and tags, the
   description, "Suggested starting goals" with "Examples to adjust, not prescribed targets.", and the head
   button "Register to use this habit". There is no "Your goals" card, no community card and no "Start with
   this" (slices 4 and 9).
7. *(page)* **Given** a signed-in user on the detail page, **When** it opens, **Then** it shows the same content
   without the head button.
8. *(page)* **Given** the address of a draft or unpublished habit, **When** anyone opens it, **Then** the Not
   found page is shown.

---

### User Story 3 - Administer the catalog (Priority: P1)

An administrator creates a catalog habit (it starts as Draft), edits it, publishes it so that everyone sees it,
and unpublishes it so that it leaves the public catalog. Registered users and visitors cannot do any of this.

**Why this priority**: Without curation the catalog is only seed data; idea brief scenario 3 (admin creates,
edits, publishes and unpublishes, and the public view reflects it) is one of the three demo scenarios.

**Independent Test**: As an administrator, create, edit, publish and unpublish one habit, checking the public
catalog after each step; as a registered user and as a visitor, confirm that each of these actions is refused.

**Acceptance Scenarios**:

1. **Given** an administrator, **When** they create the catalog habit "Ac10 Stretching" (Body, Build, icon
   `person-standing`, a description and one suggested starting goal, marked for the card), **Then** it exists
   with the state Draft, the administrator's catalog list shows it as Draft, and the public catalog does not
   contain it.
2. **Given** a Draft "Ac11 Stretching", **When** an administrator publishes it, **Then** its state is
   Published and the public catalog contains it.
3. **Given** a Published "Ac12 Stretching", **When** an administrator changes its name to "Ac12 Morning
   stretch" and its category to Health, **Then** the public catalog contains "Ac12 Morning stretch" in Health
   and no longer "Ac12 Stretching".
4. **Given** a Published "Ac13 Stretching", **When** an administrator unpublishes it, **Then** its state is
   Unpublished, the public catalog no longer contains it, and its details give "This habit is not in the
   catalog."
5. **Given** an Unpublished "Ac14 Stretching", **When** an administrator publishes it again, **Then** it is
   Published and back in the public catalog.
6. **Given** a Draft "Ac15 Stretching", **When** an administrator unpublishes it, **Then** the result is "This
   catalog habit is not published." and it stays Draft.
7. **Given** a Published "Ac16 Stretching", **When** an administrator publishes it, **Then** the result is
   success and it stays Published.
8. **Given** an Unpublished "Ac16b Stretching", **When** an administrator unpublishes it, **Then** the result
   is success and it stays Unpublished.
9. **Given** a catalog habit "Ac17 Reading" in any state, **When** an administrator creates or renames another
   catalog habit to " ac17 reading ", **Then** the result is "A catalog habit with this name already exists."
   and nothing changes.
10. **Given** an administrator, **When** they create a catalog habit with the name "   ", a name of 61
    characters, a description of 501 characters, no category, no intent, no icon or the icon `menu`, **Then**
    the result is "Enter a name.", "The name can have at most 60 characters.", "The description can have at
    most 500 characters.", "Choose a category.", "Choose Build or Reduce.", "Choose an icon." or "Choose an
    icon." respectively, and nothing is created. A name of exactly 60 characters and a description of exactly
    500 characters are accepted.
11. **Given** a registered user, **When** they list, read, create, edit, publish or unpublish catalog habits as
    an administrator would, **Then** each result is "Only administrators can manage the catalog." and nothing
    changes; for an anonymous visitor each result is "You are not signed in."
12. **Given** an administrator and an identifier that belongs to no catalog habit, **When** they read, edit,
    publish or unpublish it, **Then** the result is "This catalog habit does not exist."
13. **Given** a registered user's custom habit "Ac18 Private", **When** an administrator lists all catalog
    habits or reads, edits, publishes or unpublishes with that custom habit's identifier, **Then** the list
    does not contain it and each action gives "This catalog habit does not exist."
14. **Given** a Draft "Ac22 Reading" with no suggested starting goal, **When** an administrator gives it the
    suggested starting goals "Build, count 20 pages, 3 times per week" and "Reduce, duration 2 hours, 1 time
    per day", the second marked for the card, **Then** reading it returns both in that order with the second
    marked, and once published, browsing shows the second with the habit.
15. **Given** an administrator editing "Ac23 Reading", **When** they give it two suggested starting goals with
    none marked, or with both marked, **Then** the result is "Mark one suggested starting goal for the card."
    and nothing changes; **when** they give it six, none marked, **then** the result is "A habit can have at
    most 5 suggested starting goals." (the count is checked first); five, one marked, are accepted.
16. **Given** an administrator editing "Ac24 Reading", **When** the second of its suggested starting goals has
    no Build or Reduce; no measurement; a yes/no with an amount or with a unit; a Build count with no amount,
    or with the amount 0 or 1000; a Reduce count of 1000; a count with the unit "  " or a unit of 21
    characters; a duration with no unit; no schedule; fixed days with no day; or 0 or 21 times, or times with
    no "per day" or "per week", **Then** the result is respectively "Choose Build or Reduce.", "Choose how it
    is measured.", "A yes/no suggested starting goal has no amount or unit." (two cases), "Enter a whole number
    from 1 to 999." (three cases), "Enter a whole number from 0 to 999.", "Enter a unit, for example pages.", "The
    unit can have at most 20 characters.", "Choose minutes or hours.", "Choose fixed days or how many times.",
    "Choose at least one day.", "Enter a number of times from 1 to 20." (two cases) or "Choose per day or per
    week.", the result gives the position 2, and nothing changes. A Build amount of 1 or 999, a unit of 20
    characters and 1 or 20 times are accepted.
17. **Given** a Published "Ac25 Sugar", **When** an administrator gives it the suggested starting goal "Reduce,
    count 0 sweets, 1 time per day", marked for the card, **Then** it is accepted (a Reduce amount may be 0)
    and its text is "at most 0 sweets a day".
18. **Given** a registered user, **When** they create a catalog habit with the name "   ", **Then** the result
    is "Only administrators can manage the catalog."; **given** an administrator, **when** they edit an
    identifier that belongs to no catalog habit with the name "   ", **then** the result is "This catalog habit
    does not exist." (FR-016 order).
19. **Given** catalog habits "Ac28 Rowing" (Draft) and "Ac28 Running" (Published), **When** an administrator
    lists the catalog with the search "ac28 ro", **Then** the list contains "Ac28 Rowing" with its state and
    not "Ac28 Running", and the result gives the total number of catalog habits, which is at least 2.
20. *(page)* **Given** an administrator on the Admin page, **When** it opens, **Then** it follows
    `design/pages/PageAdmin` for its shipped part: heading "Administration", "+ New catalog habit", the privacy
    Notice, the SearchField "Search catalog", the line "Showing N of M", and the catalog table (Habit,
    Category, Intent, Status, with Edit and Publish or Unpublish per row). It has no Suggestions, Feedback or
    Accounts sections (slices 10 and 11) and, with a single section, no Tabs.

---

### User Story 4 - Private custom habits (Priority: P2)

A registered user creates a private custom habit for a behavior the catalog does not have, edits it and
deletes it. Nobody else sees it: not other users, not administrators, not visitors, and not the public
catalog.

**Why this priority**: Idea brief scenario 2. It completes the "no blank page" promise for behaviors the
catalog lacks, and it is the first private data in the product, so it sets the privacy pattern later slices
follow (constitution III).

**Independent Test**: As one registered user, create, list, edit and delete a custom habit; as another
registered user, an administrator and an anonymous visitor, confirm that none of them can list, read, edit or
delete it.

**Acceptance Scenarios**:

1. **Given** a registered user, **When** they create the custom habit "Cold shower" (Body, Build, icon
   `droplet`, description "At least 1 minute of cold water."), **Then** it exists and their list of custom
   habits contains it with these values.
2. **Given** a registered user with the custom habits "b habit" and "A habit", **When** they list their custom
   habits, **Then** the list is "A habit", "b habit" and contains no catalog habit and no other user's custom
   habit.
3. **Given** a registered user's custom habit "Cold shower", **When** they change its name to "Cold shower,
   2 min" and its intent to Reduce, **Then** reading it returns the new values.
4. **Given** a registered user's custom habit "Cold shower", **When** they delete it, **Then** it no longer
   exists and their list no longer contains it.
5. **Given** a registered user with the custom habit "Cold shower", **When** they create or rename another of
   their custom habits to " cold SHOWER ", **Then** the result is "You already have a custom habit with this
   name." and nothing changes.
6. **Given** user A with the custom habit "Cold shower", **When** user B creates the custom habit "Cold
   shower", **Then** it is created; and **when** any user creates a custom habit with the name of a published
   catalog habit, **then** it is created.
7. **Given** user A's custom habit "Ac19 Secret", **When** user B reads, edits or deletes it by its
   identifier, **Then** each result is "This custom habit does not exist." and the habit is unchanged; user B's
   list does not contain it.
8. **Given** user A's custom habit "Ac20 Secret", **When** an administrator lists custom habits, reads, edits
   or deletes it by its identifier, or creates a custom habit, **Then** each result is "Custom habits are for
   registered users.", no result contains it or any detail of it, and nothing changes (FR-025).
9. **Given** user A's custom habit "Ac21 Secret", **When** an anonymous visitor lists, reads, creates, edits or
   deletes custom habits, **Then** each result is "You are not signed in."
10. **Given** a registered user, **When** they create a custom habit with the name "   ", a name of 61
    characters, a description of 501 characters, no category, no intent or no icon, **Then** the results are
    the same messages as for catalog habits (FR-005) and nothing is created.
11. **Given** an anonymous visitor, **When** they create a custom habit with the name "   ", **Then** the
    result is "You are not signed in."; **given** user B, **when** they edit user A's custom habit with the
    name "   ", **then** the result is "This custom habit does not exist." (FR-016 order).
12. *(page)* **Given** a registered user on the Catalog page, **When** it opens, **Then** it shows the Tabs
    "Public catalog" and "My custom habits" with their counts and "+ Create custom habit"; "My custom habits"
    shows their custom habits as cards with the "Private" tag, or an EmptyState when there are none.
13. *(page)* **Given** a registered user on the custom habit page, **When** it opens, **Then** it follows
    `design/pages/PageCustomHabit` for its shipped parts: the form (Name, Description, Category, "Usually you
    want to" Build or Reduce, Icon), "Save habit", Cancel, and the privacy card. There is no "Save and create
    goal" (slice 4) and no "Suggest for catalog" action (slice 10); editing adds "Delete", which asks for
    confirmation first.
14. *(page)* **Given** a registered user on Profile, **When** it opens, **Then** it has the Tabs "Account" and
    "Custom habits", and the Custom habits tab lists their custom habits with a way to create and edit them.
15. *(page)* **Given** the address of the custom habit page or of "My custom habits", **When** an anonymous
    visitor opens it, **Then** they are sent to Sign in; **when** an administrator opens it, or a registered
    user opens the address of a custom habit that is not theirs or no longer exists, **then** the Not found
    page is shown.

---

### User Story 5 - Catalog in the navigation and on Home (Priority: P3)

The catalog becomes reachable from every page: "Catalog" in the navigation for everyone, "Admin" for
administrators, and the visitor Home gets "Browse the catalog" and a catalog preview. The signed-in Home of a
registered user, who has no goals yet, points to the catalog.

**Why this priority**: It makes the slice demonstrable without typing addresses, but every capability above
works without it.

**Independent Test**: As a visitor, a registered user and an administrator, follow the navigation and the
Home links to the catalog and to the Admin page.

**Acceptance Scenarios**:

1. *(page)* **Given** any audience, **When** a page opens, **Then** TopNav and MobileNav show "Catalog"; for
   administrators they also show "Admin". No "Goals", "Community" or "+ New goal" (slices 4 and 9).
2. *(page)* **Given** an anonymous visitor on Home, **When** it opens, **Then** the hero has the secondary
   button "Browse the catalog", and after "How it works" the section "Popular in the catalog" shows the Home
   preview (FR-008) as cards with "See all N →", N being the number of Published habits.
3. *(page)* **Given** a registered user on the signed-in Home, **When** it opens, **Then** under the heading
   "Today" an EmptyState points to the catalog with "Browse the catalog". An administrator's Home keeps its
   slice-1 state (the heading "Today" only).
4. *(page)* **Given** a registered user or a visitor, **When** they open the Admin page by its address,
   **Then** they do not get it: a visitor is sent to Sign in, a registered user sees the Not found page.

---

### User Story 6 - Seed catalog and demo custom habits (Priority: P3)

Demo loading, which runs at every start, now also loads a seed catalog into an empty catalog, and gives each
demo account it creates a few custom habits.

**Why this priority**: Constitution VI and SPEC §10: a reviewer must see a full catalog at once. The seed is
content, not new behaviour, so it comes last.

**Independent Test**: Run demo loading against an empty database and confirm the seed catalog and the demo
custom habits; run it again and confirm nothing changed.

**Acceptance Scenarios** (each starts from a reset database; see "What acceptance tests can reach"):

1. **Given** no catalog habit exists, **When** demo loading runs with a valid demo password, **Then** its
   catalog result is "seed catalog loaded", the public catalog meets FR-030 (at least 20 Published habits in
   all six categories, at least 4 of them Reduce, each seed habit with a different icon), and the
   administrator's list also has exactly one Draft and one Unpublished seed habit.
2. **Given** demo loading has loaded the seed catalog, **When** it runs again, **Then** its catalog result is
   "catalog not empty" and the catalog is unchanged.
3. **Given** an administrator created the catalog habit "Ac29 Rowing", **When** demo loading runs, **Then** its
   catalog result is "catalog not empty" and the catalog still has only "Ac29 Rowing".
4. **Given** no catalog habit and no demo account exists, **When** demo loading runs with no demo password,
   **Then** the result is "no demo password configured", and no account, no catalog habit and no custom habit
   is created.
5. **Given** no demo account exists, **When** demo loading runs with a valid demo password, **Then** each demo
   account result is "created" (slice 1, FR-016) and each custom-habit result is "demo custom habits added";
   signed in with the demo password, `demo1@example.com` lists "Cold shower" and "No phone in bed",
   `demo2@example.com` lists "Evening stretch" and "Plan tomorrow", and `demo3@example.com` lists "Late
   snacks" and "Water the plants", with the fields of FR-031; and none of these appears in the public catalog
   or the administrator's list.
6. **Given** demo loading has created the demo accounts and their custom habits, **When** it runs again,
   **Then** each account result is "already exists", there is no custom-habit result, and each account lists
   the same custom habits.
7. **Given** the user of `demo1@example.com` deleted "Cold shower", and the user of `demo2@example.com` deleted
   both its custom habits, **When** demo loading runs again, **Then** `demo1@example.com` lists only "No phone
   in bed" and `demo2@example.com` lists none: demo custom habits are never created again.
8. **Given** `demo2@example.com` was registered by a person with the password `owner2026` before demo loading
   first runs, **When** demo loading runs with a valid demo password, **Then** that account result is "already
   exists", there is no custom-habit result for it, and, signed in with `owner2026`, it lists no custom habit.

---

### Edge Cases

- A name with surrounding spaces is the same name as its trimmed form, for uniqueness and display; uniqueness
  ignores case ordinally (FR-004).
- A custom habit may have the same name as a catalog habit; the user's list and the catalog stay separate
  (US4-AS6).
- A catalog habit's name stays taken while it is Draft or Unpublished, so publishing never meets a duplicate.
- An administrator edits a Published habit: the change is public on the next read; there is no separate draft
  of a published habit (FR-012).
- A category with no published habit does not appear in the public category filter, even if drafts use it
  (US1-AS9).
- Every identifier lookup gives the same result for "does not exist", "not published" and "a custom habit",
  so nothing reveals that drafts or other users' custom habits exist (US2-AS4, US2-AS5, US4-AS7).
- An administrator browses the public catalog exactly like a registered user; drafts appear only in the
  administrator's own catalog list.
- A Reduce suggested starting goal (for example sugar or screen time) is written as an example with no health
  claim, in the brand-book tone.
- Two administrators who create a habit with the same name at almost the same time end with exactly one of
  them (HP-018).
- A custom habit deleted in another browser: its address shows the Not found page (US4-AS15); saving or
  deleting it from a form that was already open gives "This custom habit does not exist." (FR-022), shown as
  in HP-007.
- Deleting a custom habit is permanent; there is nothing to restore. No goal can use one yet (slice 4).
- A demo custom habit that a reviewer deletes does not come back; demo loading adds demo custom habits only
  when it creates the account (US6-AS7).
- A demo email that a person registered before demo loading ran keeps its owner's data untouched (US6-AS8).

## Requirements *(mandatory)*

### Functional Requirements (application behaviour, on the AC surface)

**Habits and their fields**

- **FR-001**: A catalog habit and a custom habit MUST each have a name, an optional description, exactly one
  category, an intent (Build or Reduce) and one icon. A catalog habit also has its suggested starting goals
  (FR-006, FR-007) and a state (FR-011).
- **FR-002**: The categories MUST be the fixed list, in this order: Body, Mind, Health, Money, Home, Other.
  Administrators cannot add, rename or remove categories in this slice. A habit without a particular category
  uses "Other"; there is no uncategorized habit (Clarifications, 2026-11-12).
- **FR-003**: The icon MUST be one of these 24 habit icons (Lucide names): `book-open`, `person-standing`,
  `smartphone`, `footprints`, `piggy-bank`, `candy`, `coffee`, `moon`, `dumbbell`, `droplet`, `bike`, `brain`,
  `pen-line`, `languages`, `music`, `salad`, `utensils`, `cigarette-off`, `wine-off`, `wallet`, `receipt`,
  `sprout`, `sparkles`, `tv`. Any other name, including the app's other icons such as `menu` or `trash-2`, is
  not a habit icon. Several habits may use the same icon.
- **FR-004**: The system MUST trim names and descriptions. Catalog-habit names MUST be unique among all catalog
  habits in any state, and a user's custom-habit names unique among that user's custom habits. Uniqueness and
  name order compare names ordinally, ignoring case (culture-invariant), never by the current culture. A
  catalog habit and a custom habit may share a name, and two users' custom habits may share a name.
- **FR-005**: The field checks of creating or editing a habit give one result, in this order: an empty name
  gives "Enter a name."; a name longer than 60 characters "The name can have at most 60 characters."; a
  description longer than 500 characters "The description can have at most 500 characters."; no category
  "Choose a category."; no intent "Choose Build or Reduce."; no icon, or a name not in FR-003, "Choose an
  icon."; for a catalog habit, then the suggested starting goals (FR-006, FR-007); then a name taken under
  FR-004 gives "A catalog habit with this name already exists." or "You already have a custom habit with this
  name.". An empty description after trimming means no description.
- **FR-006**: A catalog habit MUST carry an ordered list of 0 to 5 suggested starting goals, in the order the
  administrator gives, each an example to adjust, not a prescribed target. When the list is not empty, exactly
  one of them MUST be marked as the one the catalog card shows. The list is checked first for its length:
  more than 5 gives "A habit can have at most 5 suggested starting goals."; then a non-empty list with no mark
  or several marks gives "Mark one suggested starting goal for the card.". Custom habits have no suggested
  starting goals.
- **FR-007**: A suggested starting goal MUST consist of structured fields that match the New goal form of
  `design/pages.md`, so that slice 4 can pre-fill a goal from it:
  - Build or Reduce. A Build target means "at least" the amount, a Reduce target "at most" (SPEC §5.2).
  - The measurement: yes/no, count or duration. Yes/no carries no amount and no unit.
  - For count and duration, the amount: a whole number from 1 to 999 for Build, from 0 to 999 for Reduce. For
    count, a unit of 1 to 20 characters after trimming, stored trimmed and otherwise as entered, with its
    spelling and case unchanged (for example "pages", "sweet"; `"  pages  "` is stored as "pages"); for
    duration, the unit minutes or hours.
  - The schedule: either fixed days (one or more days of the week), or N times (1 to 20) per day or per week.

  Each suggested starting goal gives one result, checking in this order: no Build or Reduce "Choose Build or
  Reduce."; no measurement "Choose how it is measured."; a yes/no with an amount or a unit "A yes/no suggested
  starting goal has no amount or unit."; for count and duration an amount missing or out of
  range "Enter a whole number from 1 to 999." (Build) or "Enter a whole number from 0 to 999." (Reduce); a
  count with an empty unit "Enter a unit, for example pages."; a unit longer than 20 characters "The unit can
  have at most 20 characters."; a duration with no unit "Choose minutes or hours."; no schedule "Choose fixed
  days or how many times."; fixed days with no day "Choose at least one day."; times out of range "Enter a
  number of times from 1 to 20."; times with no period "Choose per day or per week.". The suggested starting
  goals are checked in list order, after the list checks of FR-006. A refusal carries its message as its text
  and, as a separate value, the position of the suggested starting goal it is about, counted from 1. How a
  goal built from these fields is evaluated is slice 4's decision; this slice only stores and shows them.

**Browsing the catalog (any acting user, including the anonymous visitor)**

- **FR-008**: Browsing MUST return only Published catalog habits, never a Draft, an Unpublished habit or a
  custom habit, in name order (FR-004), with each habit's name, category, intent, icon, description and the
  suggested starting goal marked for its card with its text and kind (FR-017), or none when it has none, and
  the count of habits returned. The same result is given to every audience. The Home preview is the first four
  habits of this result with no filters, fewer when fewer exist.
- **FR-009**: Browsing MUST accept an optional category (one of FR-002, or all), an optional intent (Build,
  Reduce, or both) and an optional search text. The search matches catalog habits whose name contains the
  trimmed text, ignoring case ordinally; an empty search matches all. All given filters apply together. The
  category filter offers "All" plus each category that has at least one Published habit, in FR-002 order.
  Name order (FR-008) is the only order; there is no other sort.
- **FR-010**: Reading one catalog habit's details MUST return its FR-008 content, with all its suggested
  starting goals in order, each with its fields, text and kind, and the marked one identified, when it is
  Published; for a Draft, an Unpublished habit, a custom habit's identifier or an identifier that belongs to
  nothing, the result is "This habit is not in the catalog.", the same for every audience including
  administrators.

**Catalog administration (administrators only)**

- **FR-011**: A catalog habit MUST have exactly one state: Draft, Published or Unpublished. Creating gives
  Draft. Publishing turns a Draft or an Unpublished habit into Published, and leaves a Published one Published
  with a success result. Unpublishing turns a Published habit into Unpublished and leaves an Unpublished one
  Unpublished with a success result; for a Draft the result is "This catalog habit is not published." and it
  stays Draft. Catalog habits cannot be deleted (SPEC §4).
- **FR-012**: An administrator MUST be able to list all catalog habits in every state, in name order, with
  each one's state, filtered by an optional search text under the rule of FR-009, together with the total
  number of catalog habits; to read any one catalog habit in any state; to create a catalog habit; and to edit
  every field of FR-001 except the state in any state. An edit of a Published habit is visible to everyone on
  the next read.
- **FR-013**: An administrator's read, edit, publish or unpublish with an identifier that belongs to no catalog
  habit, including a custom habit's identifier, MUST give "This catalog habit does not exist."
- **FR-014**: Every catalog administration use case MUST refuse a registered user with "Only administrators
  can manage the catalog." and an anonymous visitor with "You are not signed in.", and change nothing.
- **FR-015**: Editing or unpublishing a catalog habit MUST NOT change or end any goal based on it: such goals
  keep working and show the habit's current name and details, and unpublishing only takes the habit out of the
  public catalog (ADR 0012). Unpublishing a habit that goals use asks the administrator to confirm first,
  showing only how many goals use it, never whose. No goal exists in this slice, so every unpublish here acts
  at once; slice 4 adds the goal count and the confirmation, its own goal-side rules, and their tests.

**Check order and suggested starting goal text**

- **FR-016**: Every use case of this slice MUST give one result, checking in this order: (1) an anonymous
  visitor where a signed-in user is needed: "You are not signed in."; (2) the role: a catalog administration
  use case for a registered user "Only administrators can manage the catalog.", a custom-habit use case for an
  administrator "Custom habits are for registered users."; (3) the identifier: FR-013 or FR-022; (4) for
  create and edit, the field checks of FR-005 to FR-007; for publish and unpublish, FR-011; (5) name
  uniqueness (FR-004).
- **FR-017**: Browsing and details MUST give each suggested starting goal a text and a kind, produced from its
  FR-007 fields by these rules:
  - Amount part: count "N unit" with the unit as stored ("20 pages", "1 sweet"); duration "N min" for minutes
    and "N h" for hours; yes/no has none.
  - Schedule part: fixed days on all seven days "daily", on one day "once a week", otherwise "N days a week";
    1 time per day "a day" ("once a day" for yes/no); N times per day "N times a day"; 1 time per week "once a
    week"; N times per week "N times a week".
  - Text: the amount part and the schedule part, joined by a space when the schedule part is "daily" or "a
    day" and by ", " otherwise; yes/no is the schedule part alone. A Reduce text starts with "at most ".
  - Kind: "Yes/no", "Count" or "Duration", then " · ", then "flexible" for N times, or "fixed days " and the
    days: "every day" for all seven; otherwise the chosen days in week order (Monday first, the week does not
    wrap from Sunday to Monday), where each run of three or more consecutive days is written as its first and
    last day joined by "–" and every other day on its own, all comma-separated ("Mon–Fri", "Mon, Wed, Fri",
    "Mon–Wed, Sat", "Sat, Sun"). Day names are Mon, Tue, Wed, Thu, Fri, Sat, Sun.

**Custom habits (owner only)**

- **FR-020**: A registered user MUST be able to create a custom habit, list their own custom habits (in name
  order, FR-004), read one, edit every field of it, and delete it permanently.
- **FR-021**: Every custom-habit use case MUST take the acting user and act only on that user's own custom
  habits (constitution III). No use case returns, changes or deletes a custom habit of another account, and
  none looks one up by identifier alone.
- **FR-022**: Reading, editing or deleting with an identifier that is not one of the acting user's custom
  habits (another user's, a catalog habit's, or none) MUST give "This custom habit does not exist." and change
  nothing, so the result never reveals that someone else's custom habit exists.
- **FR-023**: Every custom-habit use case MUST refuse an anonymous visitor with "You are not signed in."
- **FR-024**: Custom habits MUST NOT appear in any catalog result: browsing, search, the category filter,
  details, the Home preview or the administrator's catalog list.
- **FR-025**: An administrator MUST get no custom habit of any account, by listing, reading, editing or
  deleting. Custom habits are for registered users: every custom-habit use case refuses an administrator with
  "Custom habits are for registered users." (Clarifications, 2026-11-12).

**Demonstration data (demo loading, an operator capability)**

- **FR-030**: Demo loading (slice 1, FR-016) MUST also load the seed catalog. After its account step, and only
  when it has a valid demo password: when no catalog habit exists it creates the seed catalog, with the catalog
  result "seed catalog loaded"; when any catalog habit exists its catalog result is "catalog not empty" and
  nothing is added. The seed catalog is created as a whole or not at all: a fault part-way leaves no seed habit,
  so the next start loads it again. It has at least 20 Published habits that cover all six categories, at
  least 4 of them Reduce; each with a neutral description and an icon that no other seed habit has; among
  their suggested starting goals all three measurements and both schedule kinds; at least one Published habit
  with no suggested starting goal and at least one with three or more; plus exactly one Draft and one
  Unpublished habit. Running it twice leaves the same catalog as running it once.
- **FR-031**: Demo loading MUST give each demo account that it creates in a run (account result "created",
  slice 1 FR-016) its demo custom habits, in the same run and as a whole with the account:
  - `demo1@example.com`: "Cold shower" (Body, Build, `droplet`) and "No phone in bed" (Home, Reduce,
    `smartphone`);
  - `demo2@example.com`: "Evening stretch" (Body, Build, `person-standing`) and "Plan tomorrow" (Mind, Build,
    `pen-line`);
  - `demo3@example.com`: "Late snacks" (Health, Reduce, `candy`) and "Water the plants" (Home, Build,
    `sprout`).

  For each such account the custom-habit result is "demo custom habits added". An account that already exists
  (account result "already exists"), whoever owns it and whatever its role, gets no custom-habit result and
  nothing of its private data is read or changed; demo custom habits are never created again after deletion.
  Demo loading is an operator capability with no acting user (ADR 0005); it writes private data only for the
  accounts it has just created and reads no private data at all
  ([ADR 0013](../../docs/adr/0013-private-demo-data.md)). The results name the demo email but never a
  custom-habit name.

### Host, page and integration requirements (outside the AC surface)

- **HP-001**: The Catalog page MUST follow `design/pages/PageCatalogVisitor` for visitors: heading "Habit
  catalog", SearchField ("Search habits"), the Notice "Ideas to start from. Register to turn any habit into
  your own goal." with Register, the FilterPanel (Category, Intent), the count ("24 habits") with "Sort: A–Z"
  as a plain label (not a control), and one HabitCard per habit with "Suggested starting goal: …" (FR-017
  text; no such line for a habit without one) and "See details".
- **HP-002**: For registered users the Catalog page MUST follow `design/pages/PageCatalogSignedIn` without the
  parts of slice 4: "+ Create custom habit", the Tabs "Public catalog" and "My custom habits" with counts, the
  FilterPanel without "Only habits I track", and cards with "Details" only (no "Use this habit", "Add another
  goal" or "N goals"). Administrators see the Public catalog without the Tabs, the custom-habit button and
  the register Notice.
- **HP-003**: Filters and search MUST apply at once (search debounced while typing), with no Apply button, and
  be kept in the page address so that a shared or reloaded address shows the same list.
- **HP-004**: With no matching habit, the catalog MUST show an EmptyState that says nothing matches and offers
  to clear the filters.
- **HP-005**: The detail page MUST follow US2-AS6 and US2-AS7; each suggested starting goal shows its FR-017
  text and kind, in the administrator's order, under "Examples to adjust, not prescribed targets.".
- **HP-006**: The address of a Draft, an Unpublished or a non-existent habit MUST show the Not found page
  (`design/components/EmptyState`), for every audience.
- **HP-007**: The custom habit page MUST follow US4-AS13; "Save habit" returns to "My custom habits" and shows
  the saved habit; refusals sit under their field, one at a time. "Delete" opens a `design/components/Dialog`
  confirmation and, after it, returns to "My custom habits". Its addresses follow US4-AS15. "This custom habit
  does not exist." from Save or Delete belongs to no field: it is shown as a Notice above the form with a link
  to "My custom habits", and the form is no longer saved.
- **HP-008**: For registered users Profile MUST have the Tabs "Account" and "Custom habits" (US4-AS14);
  Account keeps its slice-1 content. An administrator's Profile keeps its slice-1 state (Account only, no
  Tabs).
- **HP-009**: The Admin page MUST follow US3-AS20; the administrator's catalog habit form (create and edit)
  has the fields of FR-001 and an editor for up to 5 suggested starting goals (add, remove, reorder, mark the
  card's one) with the FR-007 fields, and Publish and Unpublish act from the table row and show the new state
  at once (no confirmation in this slice: FR-015). The table has no "Updated" column until time zones are
  decided (slice 4).
- **HP-010**: The Admin page and the administrator's catalog habit form MUST be reachable only by
  administrators: an anonymous visitor is sent to Sign in, a registered user sees the Not found page. The page
  is never the protection: FR-014 refuses in the use cases.
- **HP-011**: TopNav and MobileNav MUST add "Catalog" for every audience and "Admin" for administrators
  (US5-AS1).
- **HP-012**: The visitor Home MUST add "Browse the catalog" to the hero and the section "Popular in the
  catalog" with the Home preview of FR-008 as HabitCards and "See all N →" (US5-AS2). With no Published habit
  the section is absent.
- **HP-013**: The signed-in Home of a registered user MUST show, under "Today", an EmptyState that points to the
  catalog with "Browse the catalog" (US5-AS3). An administrator's Home keeps its slice-1 state.
- **HP-014**: All text MUST follow `design/brand-book.md`; suggested starting goals and seed descriptions make
  no health, medical or coaching claim, and every suggested starting goal reads as an example.
- **HP-015**: Logs, exception messages and error pages MUST NOT contain custom-habit names or descriptions
  (constitution III).
- **HP-016**: Every page of this slice MUST meet constitution IV: tokens and `ht-` classes only, visible focus
  ring, text contrast of at least 4.5:1, labelled icon-only buttons, the dialog managing focus, and a status
  (Draft, Published, Unpublished, Private) never shown by color alone.
- **HP-017**: The host MUST keep running demo loading at every start right after administrator creation, as in
  slice 1 (HP-021 there), and write its new results to the operator log: the line `Demo catalog: {Result}`
  once, and `Demo custom habits {Email}: {Result}` for each account that got a custom-habit result, naming the
  demo email but no custom-habit name. Slice 1's lines `Demo account {Email}: {Result}` stay as they are. As in
  slice 1, the host starts normally whatever the result, and a fault is logged without secrets and does not
  stop the start.
- **HP-018**: Uniqueness under FR-004 MUST hold under concurrency: two creations or renames with the same name
  at almost the same time result in one success and one refusal, with the same comparison as FR-004.

### Slice 1 requirements and tests this slice changes

Constitution IV and ADR 0008: the slice-1 state of these pages ends when this slice ships. Slice 1's
requirements below then apply only as amended here, and its tests that assert the slice-1 state are updated in
the same change:

- Slice 1 HP-010 and HP-011 (navigation without later links; visitor Home "nothing else from that preview"):
  amended by HP-011 and HP-012. Tests: `tests/HT.UiTests/Components/Layout/SiteNavTests.cs` (the later links
  "Catalog" and "Admin"), `tests/HT.UiTests/Components/Pages/HomeVisitorTests.cs` ("Browse the catalog",
  "Popular in the catalog", cards).
- Slice 1 HP-013 (signed-in Home shows only "Today"): amended by HP-013 for registered users. Test:
  `tests/HT.UiTests/Components/Pages/HomeSignedInTests.cs`.
- Slice 1 HP-014 (Profile with Account only, no Tabs): amended by HP-008 for registered users. Test:
  `tests/HT.UiTests/Host/ProfilePageTests.cs`.
- Slice 1 FR-016 and HP-021 (demo loading and its log): extended by FR-030, FR-031 and HP-017. The account
  results and their log lines are unchanged. Tests: `tests/HT.UiTests/Operator/StartupTasksDemoTests.cs`, whose
  check of the lines starting with "Demo " after the administrator line gains the new result lines;
  `tests/HT.UiTests/Host/StartupLogTests.cs`, which counts the lines starting with "Demo account " and stays
  valid because the new lines start with "Demo catalog" and "Demo custom habits" (it is re-run, not changed).

### Design changes before build

Constitution IV and ADR 0008 require `design/` to change before the app does. Before the affected pages are
built:

- `design/components/Icons` and `design/icons/`: a "Habit icons" group with the 24 names of FR-003; the 14 not
  yet in `design/icons/` (`bike`, `brain`, `pen-line`, `languages`, `music`, `salad`, `utensils`,
  `cigarette-off`, `wine-off`, `wallet`, `receipt`, `sprout`, `sparkles`, `tv`) are added from Lucide 0.460.0.
  `design/brand-book.md` (Iconography) and `design/pages/PageCustomHabit` (the icon picker) follow that group.
- The label "Suggested starting goal: …" (constitution I) everywhere the design still prescribes "Suggested
  start": the content rule of `design/brand-book.md`, the cards of `design/pages/PageHomeVisitor`, and the
  notes of `design/pages/PageCatalogVisitor.md`, `PageCatalogSignedIn.md`, `PageNewGoal.md` and
  `design/README.md`.
- `design/components/HabitCard`: the label "Suggested starting goal: …" instead of "Suggested start: …"
  (constitution I), the texts of FR-017, and the slice-2 states — signed-in catalog card with "Details" only,
  and the custom-habit card with the "Private" tag and an edit action, without "Suggest for catalog".
- `design/pages/PageCatalogVisitor` and `design/pages/PageCatalogSignedIn`: the new card label; for the
  signed-in page the slice-2 state (no "Only habits I track", no goal tags or goal actions), the "My custom
  habits" tab with cards and its EmptyState, the no-results EmptyState, and the administrator's view without
  Tabs.
- `design/pages/PageHabitDetail`: the slice-2 state (no "Your goals", no community card, no "Start with this",
  no head button for signed-in users). Its texts follow FR-017 ("15 min", not "15 minutes"), and the example
  "1 book a month · monthly period", which FR-007 cannot express (per day or per week only), is replaced.
- `design/pages/PageCustomHabit`: the slice-2 state (no "Save and create goal", no "Suggest for catalog"),
  the edit state with "Delete", its confirmation Dialog, and the refusal states of FR-005.
- `design/pages/PageAdmin`: the slice-2 state (one section, no Tabs, no "Updated" column; SearchField "Search
  catalog" and "Showing N of M" stay), and a new page for the administrator's catalog habit form (create and
  edit) with the suggested-starting-goal editor and its refusal states, which no preview shows yet. Its
  controls reuse the New goal form's components (SegmentedControl, Stepper, DayPicker) so slice 4 and this
  form look alike.
- `design/pages/PageProfile`: the Tabs "Account" and "Custom habits" and the Custom habits tab content for
  registered users.
- `design/pages/PageHomeVisitor` and `design/pages/PageHomeSignedIn`: the slice-2 states (catalog preview,
  "Browse the catalog" and "See all N →"; the registered user's EmptyState).
- `design/components/TopNav` and `design/components/MobileNav`: "Catalog" for all and "Admin" for
  administrators.
- `design/pages.md` and `design/README.md`: the slice-2 structure of these pages and the new admin form page.

### Key Entities

- **Catalog habit**: a public, reusable description of a behavior, maintained by administrators: name (unique
  among catalog habits), optional description, category, intent, icon, suggested starting goals and state
  (Draft, Published, Unpublished). Only Published ones are visible outside administration.
- **Custom habit**: a behavior one registered user created for their own goals: name (unique for that user),
  optional description, category, intent and icon, and its owner. Visible only to its owner.
- **Suggested starting goal**: an optional example target shown with a catalog habit, one of up to 5 in the
  administrator's order, one of which is marked for the card: Build or Reduce, the measurement, the amount and
  unit for count and duration, and the schedule (fixed days, or N times per day or per week) (FR-006, FR-007).
- **Category**: one of a fixed list of six groupings of habits, used for filtering.
- **Intent**: Build or Reduce, the direction a habit is usually pursued in. A goal later defines its own Build
  or Reduce (SPEC §5.1).
- **Seed catalog**: the catalog habits that demo loading creates in an empty catalog (FR-030).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An anonymous visitor can go from the visitor Home to the details of a habit in a chosen category
  in under 1 minute.
- **SC-002**: A started app with an empty database and a demo password shows at least 20 Published catalog
  habits in all six categories to an anonymous visitor (idea brief scenario 1).
- **SC-003**: A habit that an administrator publishes or unpublishes appears in or leaves the public catalog
  on the very next read, in 100% of cases (idea brief scenario 3).
- **SC-004**: A registered user can create a custom habit from the Catalog page in under 1 minute.
- **SC-005**: Draft and Unpublished habits and custom habits appear in 0 public results: browsing, search,
  the category filter, details and the Home preview.
- **SC-006**: Every custom-habit use case has negative tests (constitution III): another registered user, an
  administrator and an anonymous visitor are denied, and none of them gets any custom habit of another account
  (US4-AS7 to US4-AS9). Every catalog administration use case has negative tests for a registered user and an
  anonymous visitor (US3-AS11).
- **SC-007**: Running demo loading twice in a row leaves exactly the same catalog and custom habits as running
  it once.
- **SC-008**: Every page of this slice passes the accessibility rules of constitution IV (HP-016).

## Decisions due before acceptance criteria

The roadmap places these decisions in this slice, before `/speckit-ac-translate`. Each is answered by the
owner and recorded as a dated line in `## Clarifications` (plus an ADR where it spans features).

- Decided 2026-11-12 (see `## Clarifications`):
  - several suggested starting goals per catalog habit, one marked for the card (FR-006):
    [ADR 0011](../../docs/adr/0011-suggested-starting-goals.md);
  - the shape of a suggested starting goal, structured like the New goal form (FR-007):
    [ADR 0011](../../docs/adr/0011-suggested-starting-goals.md);
  - goals keep working on an edited or unpublished catalog habit, and unpublishing a used habit asks for
    confirmation with the goal count (FR-015): [ADR 0012](../../docs/adr/0012-goals-on-changed-catalog-habit.md);
  - catalog search, the Intent filter and A–Z order are kept as the design shows (FR-009);
  - administrators have no custom habits; slice 4 reopens it only if administrators get goals (FR-025);
  - Claude drafts the seed catalog content and the owner reviews it in the PR (FR-030);
  - the visitor Home's "Popular in the catalog" shows the first four Published habits in name order (FR-008,
    HP-012);
  - every habit has exactly one category from the fixed list, "Other" as the catch-all (FR-001, FR-002).
- The decisions of the critic revision (see `## Clarifications`), confirmed by the owner on 2026-11-12;
  private demo data: [ADR 0013](../../docs/adr/0013-private-demo-data.md).
- No decision of this slice is open.

## Assumptions

- Decision R7 (ADR 0008) applies to every page and the navigation: parts owned by slices 4, 9, 10 and 11 are
  absent, not disabled. Sentences keep the wording of the previews ("Register to turn any habit into your own
  goal.", "Later you can Suggest for catalog.").
- Suggest for catalog and its administrator review are out of scope (slice 10). Creating a goal from a
  catalog habit or a suggested starting goal is out of scope (slice 4). Catalog Habit Performance is out of
  scope (slice 9).
- Catalog habits have no delete (SPEC §4 lists create, edit, publish and unpublish); a mistaken one is
  unpublished.
- Name and description limits (60 and 500 characters), the limit of 5 suggested starting goals, and the
  ranges of FR-007 (amount 0 or 1 to 999, unit up to 20 characters, 1 to 20 times) are this spec's defaults;
  the design's examples fit them.
- A suggested starting goal's schedule offers only fixed days or N times per day or per week, as the New goal
  form does. Longer periods (for example a month, SPEC §2 "or other defined interval") wait for slice 4's time
  model; if slice 4 adds them, it extends FR-007.
- The seed catalog is loaded only into an empty catalog, so administrator edits to seeded habits are never
  overwritten and an unpublished seed habit never comes back. A deployment without a demo password gets no
  seed catalog; its administrators build the catalog. Seed content added later (for example for the hosted
  demo, slice 3) needs its own decision.
- A local database whose demo accounts were created in slice 1 does not get the demo custom habits, since
  demo loading adds them only when it creates an account; dropping the local database (or its demo accounts)
  gives them at the next start. The hosted demo starts from an empty database (slice 3).
- No account is deactivated yet (slice 11), so this slice has no rule for a deactivated owner's custom habits;
  SPEC §8 keeps their data.
- Hosting is out of scope (slice 3).
