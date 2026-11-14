# Implementation Plan: Habit catalog

**Branch**: `claude/speckit-specify-slice-2-habits-461103` | **Date**: 2026-11-14 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/002-habit-catalog/spec.md`; acceptance criteria
[acceptance.md](acceptance.md) and `tests/HT.AcceptanceTests/Features/F002/habit-catalog.feature` (both
locked). Revised after [critic-plan-pass-1.md](critic-plan-pass-1.md) and, for its MINOR notes,
[critic-plan-pass-2.md](critic-plan-pass-2.md).

## Summary

Anyone browses the Published catalog habits by category, intent and name, sees the Home preview, and reads a
habit's details with its suggested starting goals; administrators create, edit, publish and unpublish catalog
habits; registered users keep private custom habits; demo loading adds a seed catalog and each new demo
account's custom habits. The habit fields, their checks in the spec's order, the suggested starting goal and
its text and kind, and the catalog habit state live in `HT.Domain.Habits`. Three Application services
(`CatalogService` without an acting user, `CatalogAdministrationService` and `CustomHabitService` with one)
return named outcomes with the spec's texts (ADR 0005). `HT.Persistence` adds three tables in one migration,
two stores and a demo-data store whose two operations are each one transaction (ADR 0013); custom-habit
writes refuse a taken name without a database error, so no name reaches a log (HP-015). `HT.Ui` adds the
Catalog (two tabs), detail, custom habit, Admin and admin form pages, the Profile tabs and the Home and
navigation changes, after `design/` is updated first (constitution IV, ADR 0008).

## Technical Context

**Language/Version**: C# on .NET 10 (`net10.0`), nullable and implicit usings on, warnings as errors
(`Directory.Build.props`); unchanged from slice 1.

**Primary Dependencies**: ASP.NET Core 10 (Blazor static server rendering, and Interactive Server for five
pages, research R-11), EF Core 10 with the SQL Server provider, ASP.NET Core Identity's EF stores. No new
package ([research.md R-01](research.md#r-01-packages)); 14 icon files from Lucide 0.460.0 (R-16).

**Storage**: SQL Server, the databases of slice 1 (`HT`, `HT-IT`, `HT-UI`, `HT-AC`). New migration
`HabitCatalog` with `CatalogHabits`, `CatalogHabitSuggestedStartingGoals` and `CustomHabits`
([data-model.md](data-model.md#tables-htpersistence-migration-habitcatalog)).

**Testing**: xUnit v2, ArchUnitNET and reflection rules, bUnit and `WebApplicationFactory` host tests,
hand-written fakes (ADR 0006); acceptance tests by the AC workflow (ADR 0003). Detail:
[research.md R-13](research.md#r-13-tests).

**Target Platform**: browser-based web app over HTTPS; Windows developer machines; CI on Ubuntu with the SQL
Server step of slice 1. Hosting is slice 3.

**Project Type**: web application (Blazor host) over the layered .NET solution.

**Performance Goals**: demo scale. SC-001 and SC-004 (under a minute) are user-task times; the catalog has
tens of habits, filtered and ordered in memory (research R-03).

**Constraints**: no clock or culture reads in Domain or Application (constitution II; one invariant name key,
research R-03; culture-sensitive members forbidden by rule, R-12); custom habits only through owner-scoped
calls (constitution III, R-08, R-12); no custom-habit name in logs or exceptions (HP-015, R-14); `design/`
first and tokens only (constitution IV); use cases in circuits each in their own scope (R-15).

**Scale/Scope**: 22 seed habits, 6 demo custom habits; 15 user-facing use cases and one extended operator
capability; 6 new pages on 8 routes (Catalog, My custom habits, detail, custom habit, Admin, admin form) and 3
changed pages (Home, Profile, navigation).

## Constitution Check

*GATE: checked before Phase 0 and again after Phase 1, after the revision for critic plan pass 1, and after
the owner answers of 2026-11-14. Pass, no exceptions.*

| Principle | Status | How this plan meets it |
|---|---|---|
| I. Ubiquitous Domain Language | met | Type names use the glossary: `CatalogHabit`, `CustomHabit`, `SuggestedStartingGoal`, `Category`, `Intent` (a habit's direction only), `CatalogHabitState` (Draft, Published, Unpublished), `SeedCatalog`; the spec's critic revision already added these terms to `CONTEXT.md`. A suggested starting goal's Build or Reduce is `BuildOrReduce` (SPEC §2's "Build / Reduce"), not `Intent`, which `CONTEXT.md` keeps apart from a goal's; the card's one is the "marked suggested starting goal", never a "suggestion", which SPEC uses for slice 10 (research R-05). `HabitDefinition` and `HabitName` are value types, not new domain terms, so `CONTEXT.md` gets no entry for them. The card label is "Suggested starting goal: …", never "Suggested start" (design step). |
| II. Deterministic, Reproducible Evaluation | met | No evaluation in this slice. Uniqueness, order and search share one key, invariant upper-casing compared ordinally, with no current culture (research R-03); the formatter of FR-017 is a pure Domain function (R-06). `ClockReadTests` gains the culture-sensitive base-library members (R-12), and the name tests run under `tr-TR` (R-13). Unit tests cover both sides of every FR-005, FR-006 and FR-007 boundary. |
| III. Privacy Enforced Below the UI | met | Custom habits are reached only through `CustomHabitService`, whose methods take the acting user first and pass its `AccountId` as the owner of every store call; `ICustomHabitStore` has no member without an owner (research R-08). Rules: no `HT.Ui` type depends on any of the three ports, only `HT.Application.Operator` uses `IDemoDataStore` within Application, and the acting-user and owner parameters are checked by reflection (R-12). Negative tests for another user, an administrator and the anonymous visitor at unit level and in the AC items (SC-006). Demo loading writes private data only into accounts it has just created, in one transaction, and reads none (ADR 0013, R-09). Custom-habit writes never raise an error that carries the name, EF Core logs no database error text, and integration tests capture every log entry to prove HP-015 (R-14, R-13). |
| IV. Design-System Fidelity and Accessible UI | met | `design/` changes first (Work order step 1): the spec's "Design changes before build" list plus the link variant of Tabs and the 14 icon files (R-11, R-16). Each design component used (HabitCard, Tag, Tabs, SearchField, FilterPanel, SegmentedControl, Stepper, DayPicker, Dialog, DataTable, TextArea, BackLink) is built once as a Razor component. `HtIcon`'s drawings are checked against `design/icons/` (R-16). Status is a Tag with text; the Dialog manages focus; icon-only buttons are labelled; the design-sync test keeps the CSS copies equal (SC-008, HP-016). |
| V. Verified, Not Asserted | met | Build and the full suite run before every report of done; steps 3 and 4 of the Work order are verified and committed together. The slice adds types to every layer and extends the rules for them (research R-12); HP-015 and the icon drawings get executed checks (R-13, R-16). The pre-commit gate stays on. |
| VI. Demonstrable Vertical Slices | met | Domain rules, use cases, persistence, pages and tests ship together; demo loading adds the seed catalog and the demo custom habits, synthetic and on `example.com` accounts (FR-030, FR-031); no infrastructure beyond the two transactions the spec asks for. |
| Decision Discipline | met | No SPEC §11 question is touched (no time-dependent text, as slice 1 decided). The slice's product decisions are in the spec's `## Clarifications` and ADRs 0011 to 0013. Where the spec was silent, the plan's positions ("Spec discrepancies" 1 to 5) and its one cross-slice technical choice (research R-02, one `HabitId`) were accepted by the owner on 2026-11-14 and are recorded in the spec's `## Clarifications` ("Owner answers 2026-11-14 (plan)") and in [ADR 0014](../../docs/adr/0014-one-habit-identifier.md). The other choices stay inside ADRs 0003 to 0010 (the three ports follow ADR 0010's pattern). |

## Project Structure

### Documentation (this feature)

```text
specs/002-habit-catalog/
├── spec.md              # /speckit-specify, /speckit-clarify, critic revisions
├── acceptance.md        # /speckit-ac-translate (locked)
├── ac/                  # locks and AC run records
├── plan.md              # this file
├── research.md          # Phase 0
├── data-model.md        # Phase 1
├── quickstart.md        # Phase 1
├── contracts/
│   ├── application-api.md
│   └── host.md
└── tasks.md             # /speckit-tasks (not created here)
```

### Source Code (repository root)

```text
src/
├── HT.Domain/Habits/                   # HabitId, Category, Intent, BuildOrReduce, HabitIcon, HabitName,
│                                       #   HabitDefinition, SuggestedStartingGoal(+Fields, Schedule, problems),
│                                       #   SuggestedStartingGoals, SuggestedStartingGoalText,
│                                       #   CatalogHabitState(s), CatalogHabit, CustomHabit
├── HT.Application/
│   ├── Habits/                         # CatalogService, CatalogAdministrationService, CustomHabitService,
│   │                                   #   ICatalogHabitStore, ICustomHabitStore, inputs, results, views,
│   │                                   #   HabitTexts, CatalogTexts, CustomHabitTexts
│   ├── Operator/                       # DemoDataService (extended), IDemoDataStore, SeedCatalog,
│   │                                   #   DemoCustomHabits, DemoCatalogResult, DemoCustomHabitsResult, OperatorTexts
│   └── ApplicationServiceCollectionExtensions.cs
├── HT.Persistence/
│   ├── Habits/                         # CatalogHabitRow, SuggestedStartingGoalRow, CustomHabitRow,
│   │                                   #   CatalogHabitStore, CustomHabitStore, DemoDataStore (internal)
│   ├── HtDbContext.cs                  # the three tables, keys, indexes, collations
│   ├── Migrations/                     # + HabitCatalog
│   └── PersistenceServiceCollectionExtensions.cs   # + stores, EF Core log settings (R-14)
└── HT.Ui/
    ├── Operator/StartupTasks.cs        # + Demo catalog / Demo custom habits lines
    ├── Habits/UseCaseScope.cs          # one DI scope per use-case call in a circuit (R-15)
    ├── Components/
    │   ├── Layout/SiteNav.razor        # + Catalog, Admin
    │   ├── Shared/                     # + HabitCard, Tag, Tabs (button and link), SearchField, FilterPanel,
    │   │                               #   SegmentedControl, Stepper, DayPicker, Dialog, DataTable, TextArea,
    │   │                               #   BackLink; HtIcon + 24 habit icons and the others it needs
    │   ├── Habits/                     # CatalogTabs, CustomHabitForm, CatalogHabitForm, SuggestedStartingGoalEditor
    │   └── Pages/                      # + Catalog, CustomHabits, CatalogHabitDetail, CustomHabit, Admin,
    │                                   #   AdminCatalogHabit; Home, Profile changed
    └── wwwroot/css/                    # copies of design/css, if the design step changes them

tests/
├── HT.UnitTests/                       # Domain/Habits, Application/Habits, Application/Operator (DemoLoadingTests
│                                       #   rewritten); Fakes/ (+3 stores, FakeDemoDataStore over FakeAccountStore)
├── HT.ArchitectureTests/               # port fences, UseCaseParameterTests, culture members in ClockReadTests
├── HT.IntegrationTests/Habits/         # stores, races, owner scoping, demo transactions, log capture (HP-015)
├── HT.UiTests/                         # components, pages, host routes, icon drawings, startup log;
│                                       #   slice 1 tests updated
└── HT.AcceptanceTests/                 # outside the solution; written by the AC workflow

design/                                 # changed first (Work order step 1)
docs/adr/0014-one-habit-identifier.md   # research R-02, accepted 2026-11-14
docs/roadmap.md                         # status
```

**Structure Decision**: the four production projects stay; each gets a `Habits` folder, and the demo data
stays in the existing `Operator` folders. The three ports sit with the services that use them, public and
fenced by architecture rules (ADR 0010); their implementations are internal to `HT.Persistence`. No new
project.

## Work order

The order `/speckit-tasks` should follow, because later steps need earlier ones:

1. **Design first** (constitution IV, ADR 0008): the spec's "Design changes before build" list — the "Habit
   icons" group and the 14 new SVGs (downloaded from `lucide-static@0.460.0` with the owner's approval,
   research R-16), the "Suggested starting goal: …" label everywhere, the slice-2 states of HabitCard, the
   Catalog pages (with "My custom habits" at its own address), the detail page, the custom habit page,
   Admin, Profile, both Homes, TopNav and MobileNav, and the new admin catalog habit form page;
   `design/pages.md` and `design/README.md`. Also the link variant of `design/components/Tabs` and, in
   `Tabs.md`, the example address of "My custom habits" (`/catalog?view=mine` today; `/custom-habits` once the
   owner confirms "Spec discrepancies" 4) (research R-11). Then copy any changed `design/css` file to
   `src/HT.Ui/wwwroot/css`.
2. **Domain**: the `HT.Domain.Habits` types and checks, with unit tests for every boundary, the name key,
   order and search (under `tr-TR`), the FR-016 order inside the field checks, the state transitions and
   every FR-017 example.
3. **Application** and 4. **Persistence**, verified and committed **together**: after step 3 alone,
   `AddHtApplication` registers services whose ports have no registration, so `ValidateOnBuild` fails every
   integration test and demo loading fails at host start (critic plan pass 1, MINOR-2).
   - 3: the three ports, the three services (with the Home preview and the state compare-and-swap loop),
     inputs, results, views and texts; `SeedCatalog` (content drafted here, HP-014) and `DemoCustomHabits`;
     `DemoDataService` extended; unit tests with the fake stores, including the rewritten `DemoLoadingTests`
     (research R-13); the architecture rules of R-12.
   - 4: rows, `HtDbContext` mapping (keys, binary-collation keys, unique indexes, owned list), the three
     stores with R-14's conditional custom-habit statements and exception handling, the EF Core log
     settings, the migration `HabitCatalog`; `HT.IntegrationTests` for the stores, HP-018 races with log
     capture (HP-015), owner scoping and both demo transactions; the empty-catalog tests on their own
   database `HT-IT-SEED` (research R-13, "Starting states").
5. **Host**: `StartupTasks` log lines (HP-017) and their tests; `UseCaseScope` and its test; the slice 1
   tests the spec lists (`StartupTasksDemoTests` gains the new lines; `StartupLogTests` re-run).
6. **Pages**: shared components and `HtIcon` (with the icon-drawing test, R-16), `SiteNav`, the visitor and
   signed-in Home, both Catalog tabs, detail, custom habit page, Profile tabs, Admin, admin form; bUnit tests
   and host tests (routes, redirects, 404 Not found); the slice 1 UI tests the spec lists (`SiteNavTests`,
   `HomeVisitorTests`, `HomeSignedInTests`, `ProfilePageTests`).
7. **Docs**: roadmap status as the slice moves on; `README.md` status line;
   `CONTEXT.md` only if implementation sharpens a term.
8. **AC tests**: `/speckit-ac-tests-write` and `/speckit-ac-tests-analyze`, started by the owner after
   `/speckit-implement`.

## Gates before the next step

The plan took positions that the spec leaves open (constitution, Decision Discipline). **Answered by the
owner on 2026-11-14: all six accepted.**

1. **"Spec discrepancies" 1 to 5** below: accepted as the plan states them. Recorded in the spec's
   `## Clarifications`, "Owner answers 2026-11-14 (plan)". That edit of `spec.md` comes after the `sources`
   lock, so `ac_lock.py verify sources` reports spec drift until the owner runs `ac_lock.py confirm-spec
   sources`; the locked acceptance criteria are not changed by any of the answers.
2. **Research R-02, one `HabitId` for catalog and custom habits**: it spans slices (slice 4's goals reference
   either kind) and rejects a real alternative (two identifier types). Accepted; recorded as
   [ADR 0014](../../docs/adr/0014-one-habit-identifier.md), with a line in the same Clarifications block, so
   the decision exists before the Domain step builds `HabitId`.

The only step left before `/speckit-tasks` is the owner's `ac_lock.py confirm-spec sources`.

No roadmap decision is due before this slice's `/speckit-tasks`; the slice's decision rows are marked
decided in `docs/roadmap.md`.

## Spec discrepancies

Found while planning. Each point has the plan's position, within the spec's words; the owner accepted all
five on 2026-11-14, and the spec's `## Clarifications` records them (see the gates).

1. **Values that do not belong to the measurement or schedule.** FR-007 refuses a yes/no with an amount or a
   unit, but names no refusal for a duration unit on a count, a count unit on a duration, or days given with
   "N times". **Plan position:** not refused and not stored (research R-05); the editor shows only the
   controls of the chosen measurement and schedule, so the page never sends them.
2. **A yes/no with a blank unit.** "a yes/no with … a unit" could include a count unit of only spaces.
   **Plan position:** a unit that is empty after trimming is no unit, as FR-005 treats an empty description;
   only an amount, a non-blank count unit or a duration unit refuses a yes/no.
3. **What a "character" is.** FR-004 to FR-007 count characters without saying how. **Plan position:**
   UTF-16 code units, the unit of the `nvarchar` columns (research R-03), so the domain limit and the
   column never disagree. The AC items that test a length limit use ASCII filler (`x`, `t`, `u`), where
   every reading agrees; the one non-ASCII name of the AC items (`A045 Éclair`, AC-045) is far below the
   limit.
4. **Where "My custom habits" is.** HP-007 returns to "My custom habits" after Save and Delete; US4-AS12 puts
   that list on the Catalog page as a tab, US4-AS14 another on Profile, and US4-AS15 gives "the address of …
   My custom habits" its own access rules. **Plan position:** the "My custom habits" tab of the Catalog page
   has its own address, `/custom-habits`, with `[Authorize]` (an anonymous visitor goes to Sign in, an
   administrator gets Not found), and HP-007 returns there (research R-11). This departs from
   `design/components/Tabs.md`, which says tabs switch between "views of the same page: catalog vs. my custom
   habits" and gives the example address `/catalog?view=mine`. That design alternative keeps one page, but the
   page itself would then apply US4-AS15's access rules to one query value of a public address; the plan
   prefers the framework's `[Authorize]` on an address of its own, and the design step changes the
   example (Work order step 1).
5. **Missing number of times.** FR-007 refuses "times out of range" but does not say what a "N times"
   schedule with no number gives. **Plan position:** "Enter a number of times from 1 to 20." (`TimesOutOfRange`),
   as a missing amount gives the amount's range message (data-model.md, `SuggestedStartingGoal`).

## Risks

- **`NavigationManager.NotFound()`** is new in .NET 10. During prerendering it sets 404 and renders the
  Router's `NotFoundPage`; after the circuit starts it renders the same page without a status. Host tests
  pin the prerendered 404 for every Not found case (research R-11); if the API behaves otherwise, the pages
  fall back to rendering the `NotFound` component themselves with the status set through the cascading
  `HttpContext`.
- **Slice 1's locked demo AC scenarios** reset `HT-AC` and run demo loading, which now also seeds the catalog
  and adds demo custom habits. The spec's "What acceptance tests can reach" plans for it: F002 scenarios
  check by inclusion and own name prefixes, and the slice 1 scenarios check only account results, whose
  shape gains members but loses none. Both features run in `/speckit-ac-tests-analyze`.
- **Key-range locks** (research R-14): concurrent custom-habit writes of one owner and one name wait for each
  other; at demo scale the wait is one statement long.
- **Serializable seed transaction.** Two hosts starting at once against one empty database can deadlock on
  the empty-catalog check; the loser rolls back and reports "catalog not empty", and the winner's seed is
  complete (research R-09). Demo scale has one host.
- **EF Core error events ignored** (research R-14): a database fault is logged by the host through the
  store's own exception, which names the SQL error number but not the statement's values. Diagnosing a
  fault may need the SQL Server log.
- **A local `HT` database from slice 1** gets no demo custom habits (ADR 0013); quickstart says how to drop it.
- **Interactive circuits** (research R-15): five pages open circuits where slice 1 had one (`SiteNav`). Each
  use-case call gets its own scope; the acting user is read from the revalidating provider, so a session
  ended by a password change stops working within 30 minutes in an open circuit, as slice 1's HP-007 accepts.

## Complexity Tracking

No Constitution Check violation needs justifying.
