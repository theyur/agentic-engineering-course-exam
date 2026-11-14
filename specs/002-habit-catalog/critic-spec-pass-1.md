# Critic pass 1: spec for 002-habit-catalog

**Verdict**: REVISE
**Date**: 2026-11-12
**Reviewed**: `specs/002-habit-catalog/spec.md`, `specs/002-habit-catalog/checklists/requirements.md`

Checked against: `.specify/memory/constitution.md`, `SPEC.md`, `ARCHITECTURE.md`, `CLAUDE.md`, `CONTEXT.md`,
`docs/roadmap.md`, `docs/features/habit-catalog/idea-brief.md`, ADR 0003, 0005, 0007, 0008, 0011, 0012,
`design/` (pages.md, README.md, brand-book.md, the named page and component files, `design/icons/`),
`specs/001-accounts-and-access/spec.md`, `.workflow/config.json`, and the code and tests under `src/` and
`tests/` except `tests/HT.AcceptanceTests`, which was not opened.

## Notes

### MAJOR-1: Demo loading writes private custom habits with no acting user, and its behaviour is not defined
- **Where**: spec.md FR-031 (lines 494–497), US6-AS4 to US6-AS6 (lines 352–357), FR-021 (lines 474–476),
  FR-025 (lines 483–485), HP-015 (line 545), HP-017 (lines 550–551).
- **Against**: constitution III ("Every use case that reads or changes private data MUST take the acting user
  as input and scope its queries to that owner"); constitution VI (demo data "MUST obey the same consent and
  privacy rules as real data"); ADR 0005 ("Operator capabilities (administrator creation, demo loading) take
  no acting user"; "each expected refusal or operator result is a named outcome that also carries the spec's
  literal text"); slice 1 FR-016; `src/HT.Application/Operator/DemoDataService.cs`.
- **Reasoning**:
  - Custom habits are private data (CONTEXT.md "Registered user", "Custom habit"; spec US4 calls them "the
    first private data in the product"). FR-021 says "Every custom-habit use case MUST take the acting user".
    FR-031 makes demo loading read ("does not have by name") and create custom habits of three accounts, and
    demo loading is an operator use case without an acting user (ADR 0005, ADR 0007). The spec states both
    rules and does not say how they fit together, and it records no exception to constitution III. The
    plan's Constitution Check cannot mark III as met from this text.
  - FR-031 defines no result. Slice 1 gives every operator outcome a literal text ("created", "already
    exists"), and the host writes each result to the operator log (`StartupTasks.LoadDemoDataAsync`). What
    demo loading returns and logs for custom habits is not stated, while HP-015 forbids custom-habit names in
    logs. US6-AS4 to US6-AS6 assert no result at all.
  - FR-016's early results are not connected to FR-031. Verified in `DemoDataService.LoadAsync`: with no demo
    password, or one that fails the rule, it returns before touching any account. Whether demo custom habits
    are still created for demo accounts that already exist is not stated, and US6-AS4 to US6-AS6 do not say
    which demo password the run uses.
  - A demo email can belong to an administrator. `DemoDataService` leaves "an existing account, of either
    role" unchanged and reports "already exists". FR-031 ("each demo account") would then give custom habits
    to an administrator account, which FR-025 excludes. The case is not covered.
  - "At least 2 per account, visibly synthetic and different between accounts" gives no names and no test for
    "visibly synthetic" on a habit; "does not have by name" does not say whether the match ignores case as
    FR-004 does.

### MAJOR-2: The set of valid icon names (FR-003) is not defined, and it exists only in `design/`
- **Where**: spec.md FR-003 (lines 396–397), FR-005 (line 404, "no icon, or a name not in FR-003"), US3-AS9,
  US4-AS10, FR-030 (seed of at least 22 habits), "Design changes before build" (lines 555–579).
- **Against**: `design/components/Icons.html` and `Icons.md`; `design/icons/`; `design/brand-book.md`
  (Iconography); `design/pages/PageCustomHabit.html`; CLAUDE.md ("Every domain rule the acceptance criteria
  depend on belongs in the feature's `spec.md` or in the constitution"); `.workflow/config.json` (`specInputs`
  are `SPEC.md` and ADR 0003 only).
- **Reasoning**:
  - FR-003 names "the habit icons of `design/components/Icons`". The Icons card has no such group: it is one
    flat list of 33 names. Ten carry a habit meaning (book-open, person-standing, smartphone, footprints,
    piggy-bank, candy, coffee, moon, dumbbell, droplet); the other 23 are navigation, action and status icons
    (menu, x, trash-2, circle-alert, and so on).
  - The parenthesis in FR-003 equates the habit icons with "the Lucide names in `design/icons/`", which is all
    33 files. The custom-habit preview's picker shows 8 names. So three different sets (8, 10, 33) can be
    read from the sources, and FR-005's refusal "Choose an icon." depends on which one applies.
  - The rule is validated in a use case (FR-005 is on the AC surface), but acceptance criteria are drafted
    from the spec, the constitution and `specInputs`, none of which contains the list. The spec gives three
    example names only.
  - brand-book.md says "use only those, one meaning each" and "Every catalog habit has one icon ... picked
    from the Icons list". FR-030 needs at least 22 seed habits in at least 4 categories (and the category
    list includes Money, Home and Other). With ten habit-meaning icons, the seed either reuses icons across
    different habits or needs new icons. Neither is stated, and "Design changes before build" does not list
    `design/components/Icons` or `design/icons/`, though constitution IV requires the design change first.

### MINOR-1: Catalog seeding is a new startup task that departs from ADR 0007 and slice 1 without a recorded decision
- **Where**: spec.md HP-017 (lines 550–551), "Decisions this slice cites" (lines 64–66), Assumptions (lines
  652–654), FR-030.
- **Against**: ADR 0007 (Decision: the three numbered start steps; "Each later slice extends demo loading with
  its own demo data"; opt-in "through configuration"; re-run rule "existing data is reported and left
  unchanged"); slice 1 HP-021 ("right after administrator creation, the host MUST run demo loading") and
  US7-AS6; constitution, Decision Discipline ("Changing a recorded decision requires a new or superseding
  ADR"; a decision that spans features or rejects real alternatives "MUST also become an ADR"); SPEC §10.
- **Reasoning**: The spec's own sources treat the seed catalog as demonstration data (US6 "Why this priority"
  cites constitution VI and SPEC §10; idea brief §4). ADR 0007 says later slices extend demo loading. The spec
  instead adds a separate operator task that runs in every deployment regardless of the demo password, acts
  only on an empty catalog, and sits between administrator creation and demo loading, which changes slice 1's
  "right after". The summary in "Decisions this slice cites" ("Operator tasks run at every start, after
  administrator creation") is wider than what ADR 0007 decides. The choice appears only under Assumptions,
  not in `## Clarifications` or an ADR, although the spec itself notes that it affects slice 3. HP-017 also
  omits what slice 1 HP-018 and HP-021 state for their tasks: that the host starts normally whatever the
  result and that a fault is logged and does not stop the start.

### MINOR-2: Unpublishing an Unpublished habit has no defined result
- **Where**: spec.md FR-011 (lines 452–455), US3-AS4 to US3-AS7.
- **Against**: FR-011 itself (three states, two actions); CONTEXT.md "Catalog habit state".
- **Reasoning**: FR-011 defines publish for Draft, Unpublished and Published, and unpublish for Published and
  Draft. Unpublish on an Unpublished habit is not defined: it could be success (as publishing a Published
  habit is) or "This catalog habit is not published." (as for a Draft). No scenario covers it. The state of
  the row's button on the Admin table for an Unpublished habit is implied only by "Publish or Unpublish per
  row".

### MINOR-3: The test-data rule is broken by US1-AS8 and by the demo scenarios
- **Where**: spec.md "What acceptance tests can reach", Test data (lines 84–89); US1-AS8 (lines 129–131);
  US6-AS4 to US6-AS6 (lines 352–357).
- **Against**: the same paragraph ("checks browsing results by inclusion, exclusion and relative order rather
  than by catalog totals"; "Every scenario therefore uses ... account emails of its own"); ADR 0003 (one
  shared database, one schema reset per run); slice 1 spec lines 129–133 and US7-AS1 to US7-AS4.
- **Reasoning**:
  - US1-AS8 needs published habits "in the categories Body and Money only" and asserts that the filter does
    not list "Home". In the shared database other scenarios publish habits in Mind and Health (US1-AS1,
    US1-AS4, US3-AS3), and the seed of US6-AS1 covers at least 4 categories. The paragraph names only US6-AS1
    and US6-AS2 as needing a special run state.
  - US6-AS4 to US6-AS6 cannot use emails of their own: the demo emails are fixed. Slice 1's locked scenarios
    US7-AS1, US7-AS3 and US7-AS4 need a run in which the demo accounts do not exist yet, and US7-AS2 changes
    the password of `demo3@example.com`. Listing a demo account's custom habits needs an acting user for it,
    which tests build from a sign-in (`ActingUser` doc comment). The spec does not mention this coupling with
    slice 1's scenarios. Whether the two features' tests share one run was not checked, because the
    acceptance-test project is off limits.

### MINOR-4: Parts of the Admin and Home previews are dropped or reworded without being listed
- **Where**: spec.md US3-AS17 (lines 241–245), HP-009 (lines 529–532), FR-012, "Design changes before build"
  for `PageAdmin` (lines 570–573); US5-AS2 (lines 321–323), HP-012.
- **Against**: `design/pages/PageAdmin.html` (SearchField "Search catalog" and "Showing 7 of 24" above the
  table); `design/pages.md` ("Tabs: Catalog · Suggestions · Feedback · Accounts; each is a SearchField +
  DataTable with row actions"); `design/pages/PageHomeVisitor.html` ("See all 24 →"); constitution IV ("Pages
  MUST follow the structure and wording of their previews"); ADR 0008 (only sections and links of later
  slices are absent); roadmap "Pages across slices" (`PageAdmin`: slice 2 owns the Catalog tab).
- **Reasoning**: The Catalog tab belongs to this slice, and its preview has a search field and a "Showing N
  of M" line. US3-AS17 and HP-009 list the shipped parts without them, FR-012's list takes no search text,
  and the design-change entry for `PageAdmin` removes only the Tabs and the "Updated" column. The search is
  therefore neither specified nor recorded as removed. The catalog search question in `## Clarifications`
  covers the public Catalog page only. The Home link is "See all 24 →" in the preview and "See all" in the
  spec, with no design change listed for the wording.

### MINOR-5: The order of role, existence and field checks is not stated
- **Where**: spec.md FR-005 (lines 401–407), FR-006 and FR-007 (lines 408–431), FR-013, FR-014, FR-022,
  FR-023, FR-025.
- **Against**: slice 1 convention, which states one result per call in a full order (slice 1 FR-003, FR-013,
  FR-014); ADR 0005 ("one result per call in a stated order").
- **Reasoning**: FR-005 orders the field checks, but no requirement places the role refusals (FR-014, FR-023,
  FR-025) or the "does not exist" results (FR-013, FR-022) before or after them. A registered user who
  creates a catalog habit with an empty name, or an administrator who edits an unknown identifier with an
  invalid name, has two possible results. FR-007 says the list checks of FR-006 come first but not which of
  the two (more than 5, no single mark) wins when both apply.

### MINOR-6: Administrator and visitor variants of the custom-habit pages are not specified
- **Where**: spec.md HP-008 (lines 527–528), US4-AS13, HP-002 (lines 505–509), HP-013 (lines 541–542),
  US5-AS3, HP-007 (lines 524–526), HP-010 (lines 533–535), Edge Cases (lines 380–381).
- **Against**: FR-025 and the Clarification "Can an administrator account have its own custom habits? → No";
  `design/components/Tabs.md` ("Use 2–5 tabs"); slice 1 HP-008 (Profile sends a visitor to Sign in).
- **Reasoning**:
  - HP-008 gives Profile the Tabs "Account" and "Custom habits" for everyone, while US4-AS13 is about a
    registered user and HP-002 removes "My custom habits" for administrators. An administrator's Profile would
    show a tab whose every use case answers "Custom habits are for registered users.", or a single section,
    which Tabs.md does not allow as Tabs.
  - HP-013 puts the catalog EmptyState on "the signed-in Home"; US5-AS3 names a registered user. The
    administrator's Home is not stated.
  - HP-010 covers who may open the Admin pages. No requirement says what an anonymous visitor or an
    administrator gets at the custom habit page address or the "My custom habits" address, nor what the page
    shows for "This custom habit does not exist." (another user's or a deleted habit).

### MINOR-7: The wording of a suggested starting goal is defined by two places that point at each other
- **Where**: spec.md HP-019 (lines 518–521), "Design changes before build" for `PageHabitDetail` (lines
  565–567), HP-005, HP-009.
- **Against**: constitution IV (shared, unit-tested formatter; design first); `design/pages/PageHabitDetail.html`
  ("15 minutes, 5 days a week") and `design/components/HabitCard.html` ("15 min, 5 days a week");
  `design/pages/PageCatalogVisitor.html` ("at most 1 sweet a day", "30 min daily", "once a week").
- **Reasoning**: HP-019 says "the exact wording follows the design change below"; that design change says
  "the kind line and the card text follow HP-019". No rule is given for the cases the previews already show
  differently: "min" on the card and "minutes" on the detail page for the same suggestion; "daily" and "a
  day"; "once"; a free-text unit with the amount 1 ("1 sweet" in the preview, "sweets" in FR-007 and
  US3-AS16); a Reduce yes/no suggestion. HP-019 also names "the administrator's form preview", which HP-009
  and the design-change list do not contain.

### MINOR-8: Scenario gaps and imprecise scenario data for FR-006, FR-007, FR-012 and FR-023
- **Where**: spec.md US3-AS15 (lines 231–238), US2-AS1 (lines 153–157), US3-AS13 (lines 223–226), US3-AS10
  (lines 215–217), US4-AS9 (lines 287–288), US4-AS1 (lines 265–267), FR-007 (lines 422–431).
- **Against**: FR-007's own check list; FR-012 and FR-014; FR-023; ADR 0005 (results carry literal text).
- **Reasoning**:
  - FR-007's first check, a suggested starting goal with no Build or Reduce ("Choose Build or Reduce."), and
    the "amount missing" case have no scenario; US3-AS15 lists ten other cases.
  - US2-AS1 and the first goal of US3-AS13 give no Build or Reduce, which FR-007 refuses when missing.
  - "The result names the position" (FR-007, US3-AS15) does not say whether the position is part of the
    literal text or a separate value, nor whether it counts from 1.
  - US3-AS10 omits the administrator's read of one habit (FR-012); US4-AS9 omits create (FR-023).
  - US4-AS1 "it is marked private" has no matching requirement: FR-001 and FR-020 define no such field.

### MINOR-9: "Suggested start" is used beside "suggested starting goal", and CONTEXT.md does not carry the sharpened meaning
- **Where**: spec.md FR-006 (lines 408–412: "Mark one suggested start for the card." next to "A habit can have
  at most 5 suggested starting goals."), lines 96, 145, 544; CONTEXT.md "Suggested starting goal".
- **Against**: constitution I (glossary terms verbatim, no synonyms; a change that sharpens a term or adds one
  updates CONTEXT.md in the same change); SPEC §2.
- **Reasoning**: Two refusal texts of one requirement use two names for the same thing. The spec changes what
  a suggested starting goal is (up to 5 per habit, structured fields, one marked for the card), and adds the
  operator terms "catalog seeding" and "seed catalog". This change's CONTEXT.md edit adds Catalog habit state,
  Category and Intent only; the "Suggested starting goal" entry is unchanged and still singular, and the
  "Custom habit" entry still says "one user" where the spec now says registered users only.

### MINOR-10: The seed requirements do not ensure the demo shows the slice's states
- **Where**: spec.md FR-030 (lines 489–493), US6-AS1, SC-002.
- **Against**: constitution VI ("extend the representative demo data so a reviewer sees its states"); FR-009
  (Intent filter), FR-002 (six categories), FR-007 (three measurements, two schedules), HP-005 ("a habit
  without suggested starting goals shows no such line").
- **Reasoning**: FR-030 fixes counts (20 Published, 4 categories, one Draft, one Unpublished) and "suggested
  starting goals" per habit. It does not require a Reduce habit, so the Intent filter can be empty in the
  demo, and it does not require any spread of measurements or schedules. "Each with ... suggested starting
  goals" also leaves open whether one is enough.

### MINOR-11: Slice 1 requirements and tests that this slice overrides are not acknowledged
- **Where**: spec.md HP-008, HP-011, HP-012, HP-013, HP-017.
- **Against**: slice 1 HP-010, HP-011 ("and nothing else from that preview"), HP-013 ("only the heading
  'Today'"), HP-014 ("no other tab"), HP-021; existing tests:
  `tests/HT.UiTests/Components/Layout/SiteNavTests.cs` (line 18 `LaterLinks` with "Admin" and "Catalog"; the
  single "Today" link at lines 47–58), `Components/Pages/HomeVisitorTests.cs` (lines 58–70, "Browse the
  catalog" and "Popular in the catalog" must be absent, no `.ht-card`), `Components/Pages/HomeSignedInTests.cs`
  (lines 25–36, nothing but the heading), `Host/ProfilePageTests.cs` (lines 40–42, no `ht-tabs`, no "Custom
  habits"), `Operator/StartupTasksDemoTests.cs` (lines 88–98, the lines starting with "Demo " after the
  administrator line must be exactly the three account lines).
- **Reasoning**: These tests assert the slice-1 state and fail once the slice-2 pages exist; the last one also
  constrains how demo loading may log custom-habit results. The spec says "Account keeps its slice-1 content"
  but does not state that the listed slice-1 requirements stop applying, so two specs give opposite MUSTs for
  the same pages.

### MINOR-12: The checklist is out of date and two of its ticks are not supported
- **Where**: checklists/requirements.md lines 17, 27, 39–44.
- **Against**: spec.md `## Clarifications` (eight answers) and "No decision of this slice is open" (line 634);
  roadmap status "clarified"; notes MAJOR-1, MAJOR-2, MINOR-2, MINOR-5 and MINOR-8 above.
- **Reasoning**: The Notes say five decisions "go to `/speckit-clarify`", and the last entry (Iteration 2)
  records only the three marker answers. Nothing records that the five were answered, although the spec and
  the roadmap say so. "Requirements are testable and unambiguous" and "All functional requirements have clear
  acceptance criteria" are ticked, while FR-015 has no scenario and cannot have one in this slice (no goal
  exists), FR-011 leaves one transition undefined, and FR-003 and FR-031 are open as described above.

### MINOR-13: "Name order, ignoring case" does not say which comparison applies
- **Where**: spec.md FR-004 (lines 398–400), FR-008 (lines 435–439), FR-012, FR-020, US1-AS2, US4-AS2.
- **Against**: constitution II (Domain and Application do not read the current culture); FR-008 (the Home
  preview is "the first four habits" of the ordered result); HP-018 (uniqueness also held by the database).
- **Reasoning**: Uniqueness and order "ignoring case" can be ordinal or linguistic, and the database and the
  application can disagree for names with digits, punctuation or non-ASCII letters (the scenarios use only
  ASCII letters, for example "Cold shower, 2 min" next to "Cold shower"). The rule decides which four habits
  the Home shows and whether two names count as the same.

## Summary

2 MAJOR, 13 MINOR.

The catalog, administration and custom-habit rules are detailed and mostly consistent with SPEC §3, §4 and
§8, the idea brief's three scenarios, ADR 0011 and ADR 0012, and the design previews; the quoted design
wording was verified. The spec is not ready for `/speckit-ac-translate` yet. FR-031 lets an operator use case
read and write private custom habits without an acting user, with no result, no rule for a missing demo
password and no rule for a demo email held by an administrator, which leaves constitution III unanswered for
the first private data in the product. FR-003 refers to a set of icons that the design does not define, and
the list is outside the inputs of the acceptance criteria. The MINOR notes are mostly undefined corners
(unpublish of an Unpublished habit, check order, administrator page variants), unrecorded departures from
ADR 0007 and from the Admin preview, and a checklist that no longer matches the spec.
