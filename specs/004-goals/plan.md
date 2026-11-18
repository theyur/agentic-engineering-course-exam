# Implementation Plan: Goals

**Branch**: `claude/speckit-specify-slice-4-934803` | **Date**: 2026-11-18 | **Spec**: [spec.md](spec.md)

**Input**:

- the feature specification `specs/004-goals/spec.md`;
- the acceptance criteria [acceptance.md](acceptance.md) and `tests/HT.AcceptanceTests/Features/F004/goals.feature`,
  both locked;
- the time-zone decision [ADR 0016](../../docs/adr/0016-time-zones.md) (Accepted 2026-11-18, with this plan).

Revised after [critic-plan-pass-1.md](critic-plan-pass-1.md), [critic-plan-pass-2.md](critic-plan-pass-2.md) and, for its MINOR
notes, [critic-plan-pass-3.md](critic-plan-pass-3.md).

## Summary

A registered user turns a Published catalog habit or one of their own custom habits into a Build or Reduce goal,
lists it and reads it. The goal's days and weeks come from the account's own time zone. Each goal keeps that time
zone in its first settings version, and a time-zone change adds or replaces one version per goal on that goal's
own timeline. The account also gets an optional private name, a Home greeting with today's date, and "member
since".

The work splits by layer:

- **`HT.Domain`** gains two namespaces, both pure, with no clock and no culture:
  - `Time`: `NamedTimeZone`, with NodaTime's embedded tzdb kept internal (research R-01, [ADR
    0017](../../docs/adr/0017-embedded-time-zone-data.md)).
  - `Goals`: the target checks in FR-006's order, the one target-text formatter, and `GoalTimeline` (current
    date, evaluation calendar, next commitment, time-zone change).
- **`HT.Application`** adds `GoalService` and `ProfileService`, and extends the catalog, custom-habit, account and
  demo use cases. The extensions are additive, so the locked F001 and F002 criteria keep passing (FR-019, research
  R-07).
- **`HT.Persistence`** adds three goal tables and four account columns in one migration, with conditional
  statements that close the races the spec names (research R-08).
- **`HT.Ui`** adds the Goals, goal and New goal pages and changes Home, Profile, Register, the catalog pages,
  Admin and the navigation. `design/` changes first (constitution IV, ADR 0008).

## Technical Context

**Language/Version**: C# on .NET 10 (`net10.0`), with nullable reference types and implicit usings on, and
warnings as errors. Unchanged from slices 1 to 3.

**Primary Dependencies**:

- Unchanged from slices 1 to 3: ASP.NET Core 10 (Blazor static SSR, and Interactive Server for New goal), EF Core
  10 with SQL Server, and Identity's EF stores.
- **New: NodaTime 3.2.x in `HT.Domain` only**, accepted by the owner in ADR 0017 on 2026-11-18 (research R-01). Its
  fallback is the BCL's `TimeZoneInfo`.
- One small static script, `wwwroot/js/time-zone.js`, reads the browser's time zone (research R-12).

**Storage**: SQL Server, the databases of slices 1 to 3. The new migration `Goals` adds:

- the tables `Goals`, `GoalSettingsVersions` and `GoalAttempts`;
- the columns `AspNetUsers.TimeZone`, `Name`, `CreatedAt` and `GoalsRevision`.

Details are in [data-model.md](data-model.md#tables-htpersistence-migration-goals).

**Testing**: xUnit v2, ArchUnitNET and reflection rules, bUnit and `WebApplicationFactory` host tests, `FakeTimeProvider`
(ADR 0006). The acceptance tests go through the AC workflow (ADR 0003). Details: [research.md R-14](research.md#r-14-tests).

**Target Platform**: a browser-based web app over HTTPS. Windows developer machines, CI on Ubuntu, and the hosted
demo on Azure App Service (slice 3, ADR 0015).

**Project Type**: a web application (Blazor host) over the layered .NET solution.

**Performance Goals**: demo scale.

- SC-001 (under a minute, at most 3 actions) is a user-task target.
- An evaluation calendar has at most 366 entries, computed in memory.
- A time-zone change rewrites at most one version per goal of one account, in one transaction.

**Constraints**:

- No clock, time-zone or culture read in Domain or Application (constitution II). The rules are extended to
  NodaTime and `TimeZoneInfo` (research R-13).
- No NodaTime type on the public surface (ADR 0003, R-01).
- Goals are reached only through owner-scoped calls (constitution III). The one exception is the count-only
  unpublish member (R-08).
- No goal, habit or account name, unit or amount in any log or exception (HP-011).
- The slice 1 and slice 2 use cases stay source-compatible (FR-019, R-07).
- `design/` comes first, and the pages use tokens only (constitution IV).

**Scale/Scope**:

- 10 new use cases: 5 in `GoalService` and 5 in `ProfileService`.
- 4 new overloads: browse, details, unpublish and register.
- 2 extended use cases: delete and get a custom habit.
- Demo loading, extended.
- 3 new pages: Goals, goal and New goal.
- 8 changed pages and components: Home, Profile, Register, Catalog, habit details, custom habit, Admin and SiteNav.
- 7 demo goals.

## Constitution Check

*GATE: checked before Phase 0 and again after Phase 1. Pass, with no exception. The owner answered all three
gates on 2026-11-18 (see "Gates before the next step"); no `[NEEDS CLARIFICATION]` marker is open.*

| Principle | Status | How this plan meets it |
|---|---|---|
| I. Ubiquitous Domain Language | met | Type names are glossary terms: `Goal` (personal goal), `GoalSettingsVersion`, `GoalAttempt`, `Commitment`, `EvaluationPeriod`, `EvaluationCalendar`, `GoalScheduleKind` (`CommitmentBased` / `PeriodBased`), `NamedTimeZone`, `AccountName`, target text (`GoalText.TargetText`). A goal's Build or Reduce is `BuildOrReduce`, never `Intent` (`CONTEXT.md`; spec HP-002, design changes). `CONTEXT.md` already has every term of this slice (HP-014). `GoalTimeline`, `GoalDraft` and `GoalVersionChange` are code helpers, not new domain terms, so they get no entry. |
| II. Deterministic, Reproducible Evaluation | met | Time enters only as `TimeProvider` in the services and as an instant parameter in Domain. Time-zone rules come from one embedded tzdb version on every machine (R-01, ADR 0017). Settings are versions from the first goal (FR-005), and a time-zone change never changes a day that has begun (R-06). The calendar is a function of stored facts, not stored rows. Boundary tests cover both sides of every midnight, both clock changes, a missing midnight, every start-date and target limit, and the greeting hours (R-14, SC-003). The rules are extended to NodaTime's clock and BCL providers (R-13). |
| III. Privacy Enforced Below the UI | met | `GoalService` and `ProfileService` take the acting user first. Every `IGoalStore` member takes the owner, except the count-only `CountUsingCatalogHabitAsync` (ADR 0012, named in the rule). An unknown goal and another account's goal give the same "This goal does not exist." (FR-020). Administrators get no goals (FR-021). Catalog goal counts are the acting user's own (FR-032). The unpublish confirmation carries only a number (SC-005). Every use case has negative tests for another registered user, an administrator and the anonymous visitor (SC-002). The time zone and the name are account data that only the account itself reads. Demo goals are written only for accounts created in the run, in their transaction (ADR 0013). The stores raise no database error that carries a value, and log capture proves it (HP-011, R-08, R-14). |
| IV. Design-System Fidelity and Accessible UI | met | `design/` changes first (Work order step 1, the spec's "Design changes before build" list, including the texts that disagree with FR-010 and `CONTEXT.md`). GoalListItem, ChoiceCard (New goal's "How do you measure it?" and "When?", `design/pages/PageNewGoal`), KeyValue (Settings card), Notice, Dialog, SearchField, SegmentedControl, Stepper and DayPicker are each one Razor component. GoalListItem, ChoiceCard and KeyValue are new: today only the `.ht-kv` CSS exists, which the Profile page writes by hand. The others are reused. One formatter each, all unit-tested: `GoalText` (target text, kind, derived name), `GoalSummaryText` (summary card) and `DateTexts` (every date on every page) (R-05, R-11). The habit chooser Dialog manages focus, Build or Reduce is a Tag with text, and the design-sync test keeps the CSS copies equal (HP-012). |
| V. Verified, Not Asserted | met | Build and the full suite run before any report of done. Steps 3 and 4 of the Work order are verified and committed together. New production types in every layer come with their architecture rules (R-13). HP-011 and HP-019 get executed checks: log capture, and an account created before the migration on `HT-IT-MIGRATE` (R-14). The pre-commit gate stays on. What tests cannot cover (the browser script of HP-016) is labelled unverified, with a quickstart step to check it by hand. |
| VI. Demonstrable Vertical Slices | met | Domain rules, use cases, persistence, pages, tests and demo data ship together. Demo loading shows both schedule kinds, Build and Reduce, catalog and custom habits, a goal that starts later, one with its own name and one on an Unpublished habit, plus an account name (FR-040, SC-006). The smoke check confirms them after each deployment (HP-018). There is no infrastructure beyond the transactions the spec's races need. |
| Decision Discipline | met | SPEC §11.1 (first part) is decided in the spec's `## Clarifications` and ADR 0016. This plan does not touch SPEC §11.1's second part (grace period) or §11.4. The owner answered the plan's three gates on 2026-11-18: ADR 0016 is Accepted, ADR 0017 (the time-zone library, which spans features and rejects a real alternative) is Accepted, and the five "Spec discrepancies" positions are accepted. They are recorded in the spec's `## Clarifications`, "Owner answers 2026-11-18 (plan)". |

## Project Structure

### Documentation (this feature)

```text
specs/004-goals/
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
├── HT.Domain/
│   ├── Time/                           # NamedTimeZone, NamedTimeZones (NodaTime internal)
│   ├── Goals/                          # GoalId, HabitKind, GoalHabit, GoalTargetFields, GoalTarget,
│   │                                   #   GoalScheduleKind, GoalSettings, GoalSettingsVersion, GoalAttempt,
│   │                                   #   GoalName, GoalProblem, GoalField, GoalDraft, Goal, GoalText,
│   │                                   #   GoalTimeline, Commitment, EvaluationPeriod, EvaluationCalendar,
│   │                                   #   GoalVersionChange
│   ├── Habits/                         # internal TargetFieldCheck and text parts shared with SuggestedStartingGoal*
│   └── Accounts/                       # Account (+3 init properties), AccountName
├── HT.Application/
│   ├── Goals/                          # GoalService, IGoalStore, StoredGoal, NewGoal, inputs, results, views,
│   │                                   #   GoalTexts, GoalSummaryText
│   ├── Accounts/                       # ProfileService, InstallationTimeZone, DateTexts, results;
│   │                                   #   AccountService (+RegisterAsync overload), IAccountStore (+members)
│   ├── Habits/                         # CatalogService / CatalogAdministrationService / CustomHabitService extended
│   └── Operator/                       # DemoDataService (order, goals), DemoGoals, DemoGoal, DemoAccountProfile,
│                                       #   DemoGoalsResult, IDemoDataStore (extended), OperatorService (+TimeProvider, InstallationTimeZone)
├── HT.Persistence/
│   ├── Goals/                          # GoalRow, GoalSettingsVersionRow, GoalAttemptRow, GoalStore, GoalMapping
│   ├── Habits/                         # CustomHabitStore (conditional delete), DemoDataStore (goals, profile)
│   ├── Identity/                       # HtUser (+TimeZone, Name, CreatedAt, GoalsRevision, all ignored after save), AccountStore, HtUserClaimsPrincipalFactory (+ht:name)
│   ├── HtDbContext.cs                  # the three tables, keys, indexes, identity, GoalsRevision
│   └── Migrations/                     # + Goals
└── HT.Ui/
    ├── Program.cs                      # InstallationTimeZone from Operator:DefaultTimeZone
    ├── Operator/StartupTasks.cs        # new line order, Demo goals lines, default-zone warning
    ├── Components/
    │   ├── Layout/SiteNav.razor        # Goals, + New goal, avatar letter from the name
    │   ├── Shared/                     # + GoalListItem, ChoiceCard, KeyValue; Avatar.LetterOf(name, email)
    │   ├── Goals/                      # NewGoalForm, HabitChooser, GoalSummaryCard, GoalSettingsCard
    │   ├── Account/                    # RegisterForm (+name, hidden timeZone), TimeZoneForm, NameForm
    │   └── Pages/                      # + Goals, GoalDetail, NewGoal; Home, Profile, Register, Catalog,
    │                                   #   CatalogHabitDetail, CustomHabit, Admin changed
    └── wwwroot/js/time-zone.js

tools/HT.SmokeCheck/                    # check 5: goal names, demo1's name; SmokeCheckCoverage +2 entries

tests/
├── HT.UnitTests/                       # Domain/Time, Domain/Goals, Application/Goals, Application/Accounts,
│                                       #   Application/Operator (DemoLoadingTests rewritten); Fakes/ (+FakeGoalStore)
├── HT.ArchitectureTests/               # ClockReadTests, DomainDependencyTests, PublicSurfaceTests (new),
│                                       #   UseCaseParameterTests, Application/UiDependencyTests
├── HT.IntegrationTests/Goals/          # stores, owner scoping, order, races, demo goals, HP-011 log capture,
│                                       #   HP-019 migration
├── HT.UiTests/                         # pages and components, host routes, startup log order, coverage;
│                                       #   slice 1 and 2 tests the spec lists
└── HT.AcceptanceTests/                 # outside the solution; written by the AC workflow

design/                                 # changed first (Work order step 1)
docs/adr/0016-time-zones.md             # Accepted 2026-11-18 (migration sentence amended)
docs/adr/0017-embedded-time-zone-data.md  # research R-01, Accepted 2026-11-18
docs/roadmap.md                         # status
```

**Structure Decision**: the four production projects stay, and no project is added. Goals get their own `Goals`
folder in each layer. The time-zone value lives in `HT.Domain.Time`, because accounts and goals both use it. The
new port `IGoalStore` sits with the service that uses it, public and fenced by architecture rules (ADR 0010).
Its implementation is internal to `HT.Persistence`.

## Work order

`/speckit-tasks` should follow this order, because later steps need earlier ones:

1. **Design first** (constitution IV, ADR 0008). The spec's "Design changes before build" list:
   - the New goal states and summary texts;
   - the slice 4 states of Goals, GoalListItem and the goal page;
   - catalog, details, custom habit, the Admin Dialog, both Homes, Profile and Register;
   - TopNav and MobileNav, including the avatar letter;
   - the texts that disagree with FR-010, HP-002 and `CONTEXT.md`;
   - `design/pages.md` and `design/README.md`.

   Then copy any changed `design/css` file to `src/HT.Ui/wwwroot/css`.
2. **Domain**:
   - add NodaTime (ADR 0017);
   - `HT.Domain.Time`, `HT.Domain.Goals` and the account additions;
   - factor the shared target check and text parts out of slice 2's types, without changing their output (slice
     2's unit tests stay green);
   - the boundary unit tests (R-14);
   - the Domain architecture rules (R-13).
3. **Application** and 4. **Persistence**, verified and committed **together**. After step 3 alone,
   `AddHtApplication` registers services whose `IGoalStore` has no implementation, which fails `ValidateOnBuild`
   in every integration and host test (as in slice 2). The bUnit page tests break too: `CatalogService`,
   `CatalogAdministrationService` and `CustomHabitService` now take `IGoalStore`. So
   `tests/HT.UiTests/Components/Pages/PageServices.cs` registers a `FakeGoalStore` in the same commit (critic plan
   pass 1, MINOR-2). The port changes break their callers inside the solution. Those callers are updated in the
   same commit (research R-07 lists them: `IAccountStore.CreateAsync` in six test files and `FakeAccountStore`, and
   `IDemoDataStore.CreateDemoAccountAsync` in `SeedCatalogLoadingTests` and `FakeDemoDataStore`). The
   `<Compile Include>` link for `FakeGoalStore` goes into `HT.UiTests.csproj`. The service constructors change
   without the old ones kept (R-07), so the hand-built services in the unit, UI and integration tests that R-07
   lists are updated in the same commit too.
   - Step 3:
     - `GoalService`, `ProfileService` and `InstallationTimeZone`;
     - the extended use cases with their overloads and `init` properties (R-07);
     - demo goals and the new loading order;
     - the texts and formatters;
     - `FakeGoalStore` and the unit tests, including the rewritten `DemoLoadingTests`;
     - the Application rules.
   - Step 4:
     - the rows, the mapping and `HtDbContext`;
     - `GoalStore`, with R-08's conditional statements and the optimistic time-zone change;
     - `AccountStore`'s new members;
     - the custom habit store's conditional delete;
     - `DemoDataStore` with goals;
     - the migration `Goals`;
     - the integration tests, including the races, HP-011 log capture and HP-019.
5. **Host**:
   - `InstallationTimeZone` from configuration;
   - the `StartupTasks` line order and the Demo goals lines (HP-013);
   - the `ht:name` claim;
   - the smoke check's check 5 and its coverage entries (HP-018), with `RunnerBehaviourTests.GoodSite()`
     scripting `GET /goals` and `GET /profile`, plus the coverage table in
     `specs/003-demo-hosting/contracts/smoke-check.md` ("Coverage rule") extended in the same change;
   - the slice 1 and 2 tests the spec lists (`StartupTasksDemoTests`).
6. **Pages**:
   - SiteNav;
   - Home;
   - Register (name, the hidden time zone and its script);
   - Profile (name, time zone, header);
   - Goals, goal and New goal (habit chooser, summary card);
   - the catalog, details, custom habit and Admin changes.

   Each comes with bUnit and host tests, and with updates to the UI tests of slices 1 and 2 that the spec lists
   (`SiteNavTests`, `HomeSignedInTests`, `CatalogPageTests`, `CatalogHabitDetailTests`, `CustomHabitPageTests`,
   `AdminPageTests`, and the Register and Profile tests).
7. **Docs**:
   - the roadmap status, and the README status line;
   - `CONTEXT.md`, only if implementation sharpens a term (HP-014).
8. **AC tests**: `/speckit-ac-tests-write` and `/speckit-ac-tests-analyze`, which the owner starts after
   `/speckit-implement`.

## Gates before the next step

The plan took positions where the spec leaves something open (constitution, Decision Discipline). **The owner
answered all three on 2026-11-18, and accepted each one as proposed.**

1. **ADR 0016: Accepted.** Its Consequences sentence on the migration now says what "Spec discrepancies" 1 says
   (critic plan pass 1, MINOR-10).
2. **ADR 0017, NodaTime in `HT.Domain`: Accepted** (research R-01). The BCL fallback is not used.
3. **"Spec discrepancies" 1 to 5: accepted** as stated below. They are recorded in the spec's `## Clarifications`
   as "Owner answers 2026-11-18 (plan)".

That edit of `spec.md` comes after the `sources` lock. So `ac_lock.py verify sources` reports spec drift until
the owner runs `ac_lock.py confirm-spec sources`. None of the answers changes a locked acceptance criterion.

No roadmap decision is due before this slice's `/speckit-tasks`.

## Spec discrepancies

These were found while planning. Each point has the plan's position, within the spec's words; the owner
accepted all five on 2026-11-18.

1. **HP-019: the default time zone of existing accounts.** A migration cannot read the operator's
   `Operator:DefaultTimeZone`, so it cannot write "the installation's default" into existing rows. **Plan
   position** (research R-04):
   - `AspNetUsers.TimeZone` is nullable, and `NULL` exists only on pre-slice accounts;
   - reading such an account gives the configured default;
   - its first goal, created in a transaction, writes the resolved default into the row.

   The alternative is to migrate with `UTC`, which equals the hosted demo's default, and say "UTC" in HP-019.
2. **FR-024 and HP-015: "the installation's time zone data".** With ADR 0017 this is NodaTime's embedded tzdb, the
   same on every machine. It is not the operating system's data. **Plan position**: the list and the check both
   use that one source, and its version changes only with the package.
3. **FR-006: "the field it is about".** The spec names no field values. **Plan position**: a `GoalField` enum
   (`Habit`, `BuildOrReduce`, `Measurement`, `Amount`, `Unit`, `Schedule`, `Days`, `Times`, `Period`,
   `StartDate`, `Name`) on every creation refusal. The acceptance criteria check only the message
   (`acceptance.md`, "Not checked here").
4. **FR-015: "creation order".** **Plan position**: the order in which goals were inserted (an identity column),
   not the supplied creation time. Equal supplied times then still have a fixed order (critic ac pass 1,
   MINOR-2). AC-022's distinct times give the same order either way.
5. **HP-003 "start date is after today" against FR-014 "has no current date".** These two differ only for the
   hours of a time-zone switch, when the account's "today" and the goal's own current date disagree (FR-012).
   **Plan position**: the Goals page shows "starts …" exactly when the list entry carries `StartDate` (FR-014), so
   the page and the use case never disagree.

## Risks

- **The locked F001 support code stops compiling** where it builds `AccountService`, `OperatorService` and
  `DemoDataService` by hand. Their constructors gain `InstallationTimeZone` and, for the last two, `TimeProvider`.
  No one may read that code. The F004 run of `/speckit-ac-tests-write` gets the compile error and makes the
  cross-feature change, as slice 2's run did for `DemoDataService` (R-07). The use-case surface itself stays
  additive, so F001 and F002 steps keep their meaning. `/speckit-ac-tests-analyze` then runs all three
  features.
- **The F002 demo scenarios** reset `HT-AC` and run demo loading, which now loads the seed catalog first and adds
  goals. No locked F001 or F002 scenario asserts the order of the work. No demo goal uses the custom habits that
  F002's AC-066 deletes (FR-040), so that delete still succeeds.
- **NodaTime's tzdb lags the real world** until the package is updated (ADR 0017). The scenarios use only
  identifiers that every current tzdb has (FR-024).
- **`Europe/Kyiv` on the browser side.** Older browsers may send `Europe/Kiev`. Both are named time zones (a link
  is kept as given), so registration accepts either.
- **The optimistic time-zone change** compares the account's `GoalsRevision`, which every goal creation increments
  (research R-08). It therefore retries, at most 10 times, under concurrent goal creation for one account. At
  demo scale a conflict needs two tabs of one user acting at the same second.
- **The count-only cross-owner member** of `IGoalStore` is the one place where goals are touched without an owner.
  The reflection rule names it, and its result type is `int`.
- **The interactive New goal page** opens a circuit per visit. Use cases run in their own scope (slice 2 R-15).
  The acting user comes from the revalidating provider, as slice 2's interactive pages do.

## Complexity Tracking

No Constitution Check violation needs justifying.
