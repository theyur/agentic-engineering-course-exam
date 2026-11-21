# Implementation Plan: Personal reports

**Branch**: `claude/speckit-specify-slice-6-990a9c` | **Date**: 2026-11-20 | **Spec**: [spec.md](spec.md)

**Input**:

- the feature specification `specs/007-personal-reports/spec.md`;
- the acceptance criteria [acceptance.md](acceptance.md) and
  `tests/HT.AcceptanceTests/Features/F007/personal-reports.feature`, both locked;
- [ADR 0020](../../docs/adr/0020-derived-figures-on-read.md) (no caching of derived figures) and
  [ADR 0021](../../docs/adr/0021-demo-report-loading.md) (demo report loading), with ADR 0018 (evaluation on read).

## Summary

A registered user sees how consistent each goal has been, with every figure computed on read from the records,
the settings versions and the supplied time:

- consistency with its count, rounded down, or "N/A";
- strict current and longest streaks in the goal's unit;
- the last 30 ended results with their states;
- a trend of recorded progress against the target.

The Goals page lists every goal with its figures, the goals that need action first. The signed-in Home becomes the
dashboard:

- Due, Pending and Missed-this-week counts;
- a Today list that ends with Upcoming entries;
- each goal's summary;
- the latest notes.

A fourth startup task, demo report loading, creates `demo5@example.com` with 12 weeks of history, so the hosted
demo shows full trends and longer streaks.

The work splits by layer:

- **`HT.Domain`** gains `Reports`: pure folds over evaluated entries for consistency, streaks, the streak unit and
  trend bars. `GoalTimeline` gains `EntriesThrough`, which lists the whole attempt without the 366-day history cap
  (research R-01, R-02, R-05).
- **`HT.Application`** adds `ReportService` with three owner-scoped use cases: goal report, goals overview and
  dashboard. It also adds the shared formatter `ReportText`, the visitor card's `ExampleGoal`, and
  `DemoReportService` with `DemoReport` and the port `IDemoReportStore`. Existing members are unchanged;
  `RecordingText` gains only `HoursLeft`. Three private helpers become shared internals: slice 5's entry view and
  `GoalHistory` builder, and slice 4's list item. The reports then return slice 5's own `GoalHistory` and
  `HistoryEntry` views, so the Home and Goals rows open the record and note dialogs with no extra read (R-03, R-04,
  R-06, R-07, R-09, R-13).
- **`HT.Persistence`** gets no table and no migration. Only `DemoReportStore` is added, sharing an extracted
  `DemoAccountWriter` with `DemoHistoryStore` (R-08, R-09).
- **`HT.Ui`** gets the goal page's GoalStats and TrendChart, the Goals page's full list as an interactive island,
  the signed-in Home's tiles and Today list as an island with static GoalSummary and Recent notes cards, and the
  visitor Home's example card. The host gets the fourth startup task, and the smoke check gets check 7. `design/`
  changes first (constitution IV, ADR 0008; R-10 to R-12).

## Technical Context

**Language/Version**: C# on .NET 10 (`net10.0`), with nullable reference types and implicit usings on, and
warnings as errors. Unchanged.

**Primary Dependencies**: unchanged. ASP.NET Core 10 (Blazor static SSR, with Interactive Server islands), EF
Core 10 with SQL Server, Identity's EF stores, and NodaTime 3.2.x inside `HT.Domain` only (ADR 0017). No new
package.

**Storage**: SQL Server, the databases of slices 1 to 5, with no schema change (HP-009, ADR 0020). Reads go through
the existing `IGoalStore`, `IRecordStore` and `IAccountStore` members. The only write is demo report loading,
through its own port into the existing tables
([data-model.md](data-model.md#tables-no-schema-change)).

**Testing**:

- xUnit v2, ArchUnitNET and reflection rules, bUnit and `WebApplicationFactory` host tests, and `FakeTimeProvider`
  (ADR 0006).
- One timed integration test for SC-007.
- The acceptance tests go through the AC workflow (ADR 0003).
- Details: [research.md R-14](research.md#r-14-tests).

**Target Platform**: a browser-based web app over HTTPS. Windows developer machines, CI on Ubuntu, and the hosted
demo on Azure App Service, refreshed daily (ADR 0015, ADR 0018). Unchanged.

**Project Type**: a web application (Blazor host) over the layered .NET solution.

**Performance Goals**: SC-007. For 3 goals, one with 366 daily results, each of the three reads returns in under
2 seconds against SQL Server. A dashboard or overview read costs about 8 round trips: the account, the goal list,
and per goal the two queries of `RecordStore.ReadAsync`. A goal report costs 4. All of it is evaluated in memory
(research R-08). Everything else is demo scale: no demo goal is older than 84 days.

**Constraints**:

- No clock, time-zone or culture read in Domain or Application (constitution II). Time enters as `TimeProvider`,
  read once per call.
- Nothing derived is stored or cached; within one call each goal is evaluated once (HP-009, ADR 0020).
- Reports reach goals, records and notes only through owner-scoped calls (constitution III, FR-014).
- No note, recorded value, unit, goal or habit name, or figure in any log, exception or error page (HP-007).
- The use cases, constructors and ports of slices 1 to 5 stay as they are, so F001, F002, F004 and F005 pass
  unchanged (FR-016).
- `design/` comes first, and the pages use tokens only (constitution IV).

**Scale/Scope**:

- 3 new use cases in `ReportService`, and 1 operator use case (`DemoReportService`).
- 1 new port, no new table.
- 4 changed pages (goal page, Goals, signed-in Home, visitor Home).
- 6 new shared components: GoalStats, ConsistencyStrip, TrendChart, StatTile, CommitmentRow and GoalSummary.
  The existing `GoalItem` is extended to the full GoalListItem, StatusChip gains "Upcoming", and there are 2
  islands (GoalsList, TodayPanel).
- 1 new startup task and 1 new smoke check (check 6 gains 3 texts).
- 3 demo goals for one new demo account.

## Constitution Check

*GATE: checked before Phase 0 and again after Phase 1. Pass, with no exception. No `[NEEDS CLARIFICATION]` marker
is open. D-1 to D-5 and R3 are decided in the spec's `## Clarifications`, ADR 0020 and ADR 0021. The plan found no
open spec detail that needs the owner (see "Spec discrepancies").*

| Principle | Status | How this plan meets it |
|---|---|---|
| I. Ubiquitous Domain Language | met | **Names.** Type and member names use the glossary: `Consistency`, `Streaks` (`Current`, `Longest`), `StreakUnit`, `Trend`, `TrendBar`, `ReportService`, `GoalReport`, `GoalsOverview`, `Dashboard`, `DemoReportService` (demo report loading).
`StreakUnits` holds `StreakUnit`'s method.<br>**Glossary.** Consistency, Streak, Report range, Recent results, Trend, Dashboard and Demo report loading each got their `CONTEXT.md` entry with a NOT-boundary with the spec (HP-010, done 2026-11-20).<br>**UI words.** The StatusChip reads "Upcoming", never "Due Mon". The first tile reads "Due now", never "Due today". No "score", "rank", "late" or "overdue".<br>**Helpers.** `GoalEvaluation`, `EntryViews`, `TrendBuilder` and `ReportText` are code helpers, not new domain terms. |
| II. Deterministic, Reproducible Evaluation | met | **One function.** Every figure is a fold over the states that slice 5's `Evaluator.Evaluate` gives at one supplied instant, with each entry's own settings version (FR-009). Nothing is stored or cached (HP-009, ADR 0020).<br>**Corrections.** A correction changes the next read through the same function, and a unit test proves report equality for on-time and corrected records (SC-004).<br>**Boundaries.** Unit tests cover rounding down, both sides of each grace end, at, below and above each target, and every schedule kind for Build and Reduce, including the Kyiv and Berlin clock changes (SC-003, R-14).<br>**Clock rule.** `ClockReadTests`' generic rule already forbids clock and culture reads in every Domain and Application type, the new ones included (R-14). |
| III. Privacy Enforced Below the UI | met | **Access.** `ReportService` takes the acting user first and refuses the anonymous visitor and administrators before any identifier (FR-015).<br>**Scoping.** The goal is found only through the owner-scoped `IGoalStore.FindAsync`, and records only through `IRecordStore.ReadAsync(owner, goal, …)`. The overview and the dashboard take no identifier (FR-014).<br>**Tests.** Every use case has negative tests for another registered user, an administrator and the anonymous visitor (SC-005). Log capture proves HP-007.<br>**Demo data.** Demo report loading writes only for the account it creates, in one transaction, through a port fenced to its service (ADR 0013, R-09).<br>**Visitor Home.** Its card reads no store (D-3). |
| IV. Design-System Fidelity and Accessible UI | met | **Design first.** `design/` changes first (Work order step 1, the spec's "Design changes before build").<br>**Components.** Each design component is one Razor component, reused on every page: GoalStats, ConsistencyStrip, TrendChart, StatTile, CommitmentRow, GoalSummary, GoalListItem, StatusChip.<br>**Formatter.** One shared, unit-tested formatter, `ReportText`, gives every consistency, streak, unit, target-line and `aria-label` text. The use cases return the consistency and streak texts (HP-005).<br>**Accessibility.** The strip and the trend are `role="img"` with a counting `aria-label`. States always carry a word or a shape. Tiles are buttons with `aria-pressed`. The dialogs are slice 5's (HP-006, R-12). |
| V. Verified, Not Asserted | met | **Verification.** Build and the full suite run before any report of done, and steps 3 and 4 of the Work order are verified and committed together.<br>**Architecture rules.** The new Domain and Application namespaces and the new port come with their rules (R-14).<br>**Executed checks.** HP-007 has log capture, SC-007 a timed integration test, and HP-008 the extended smoke check.<br>**Unverified.** The hosted smoke check after deployment and the real daily refresh are labelled unverified, each with a quickstart step.<br>**Gate.** The pre-commit gate stays on. |
| VI. Demonstrable Vertical Slices | met | **Slice.** Domain rules, use cases, pages, tests and demo data ship together.<br>**`demo4`.** Its 28 days already give non-trivial figures (FR-030).<br>**`demo5`.** It adds 12 weeks: full trends (14, 12 and 14 bars), streaks above 10, and a result Missed this week on every weekday (FR-032).<br>**Infrastructure.** Nothing beyond one startup task. The daily refresh already re-creates it (R-10). |
| Decision Discipline | met | **Recorded decisions.** SPEC §11.7 (first part) is decided as D-1, ARCHITECTURE §9's caching as D-4 (ADR 0020), and the longer demo history as D-5 (ADR 0021).<br>**This plan.** It decides nothing that spans features. |

## Project Structure

### Documentation (this feature)

```text
specs/007-personal-reports/
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
│   ├── Reports/                        # Consistency, Streaks, StreakUnit, StreakUnits, TrendKind, TrendSpan, TrendBar,
│   │                                   #   Trend, TrendBuilder
│   └── Goals/GoalTimeline.cs           # + EntriesThrough (no 366-day cap; 364-day Monday-aligned windows)
├── HT.Application/
│   ├── Reports/                        # ReportService, GoalEvaluation (internal), GoalReport, GoalStatsView,
│   │                                   #   RecentResults, ResultView, TrendView, GoalsOverview, OverviewGoal,
│   │                                   #   Dashboard, DashboardCounts, TodayEntry, DashboardGoal, DashboardNote,
│   │                                   #   results and outcomes, ReportText, ExampleGoal
│   ├── Records/EntryViews.cs           # internal; RecordingService.EntryView and HistoryOf moved here, unchanged
│   ├── Records/RecordingText.cs        # + HoursLeft, which ForLabel now calls
│   ├── Goals/GoalListItems.cs          # internal; GoalService.ListItemOf moved here, behaviour unchanged
│   ├── Operator/                       # + DemoReportService, DemoReport, IDemoReportStore, DemoReportLoadingResult,
│   │                                   #   DemoReportAccountResult
│   └── ApplicationServiceCollectionExtensions.cs   # + ReportService, DemoReportService
├── HT.Persistence/
│   ├── Habits/DemoAccountWriter.cs     # internal; DemoHistoryStore's body, shared
│   ├── Habits/DemoReportStore.cs       # IDemoReportStore over DemoAccountWriter
│   └── PersistenceServiceCollectionExtensions.cs   # + IDemoReportStore
└── HT.Ui/
    ├── Operator/StartupTasks.cs        # + Demo report loading step and its lines
    └── Components/
        ├── Shared/                     # GoalStats, ConsistencyStrip, TrendChart, StatTile, CommitmentRow, GoalSummary;
        │                               #   GoalItem extended to the full GoalListItem; StatusChip + Upcoming
        ├── Reports/                    # GoalsList (island), TodayPanel (island), RecentNotesList, ExampleGoalCard
        ├── Records/GoalDetailBody.razor # + GoalStats and TrendChart above History, reload after a write
        └── Pages/                      # Goals, Home (signed-in and visitor)

tools/HT.SmokeCheck/                    # check 6 + 3 texts; check 7 demo report (demo5); SmokeCheckCoverage + 4 entries;
                                        #   PageReader + ReadStatLabels, ReadTileLabels, ReadStatusChips

tests/
├── HT.UnitTests/                       # Domain/Reports, Domain/Goals (EntriesThrough), Application/Reports,
│                                       #   Application/Operator (DemoReport); Fakes/ (+FakeDemoReportStore)
├── HT.ArchitectureTests/               # R-14 rules
├── HT.IntegrationTests/                # Reports/ (scoping, SC-007 timing, log capture), Operator/ (demo report)
├── HT.UiTests/                         # components, page host tests, startup and smoke check tests
└── HT.AcceptanceTests/                 # outside the solution; written by the AC workflow

design/                                 # changed first (Work order step 1)
specs/003-demo-hosting/contracts/smoke-check.md   # coverage table + 4 rows, check 7
docs/operations/hosted-demo.md, docs/roadmap.md, README.md   # demo5, status
```

**Structure Decision**: the four production projects stay, and no project is added. Reports get their own
namespace in Domain and in Application, because slice 9 computes from the same Domain functions without these use
cases. Demo report loading sits with the other operator capabilities. Its port is public for the AC reference
export, fenced by architecture rules (ADR 0010, ADR 0013), and implemented internally in `HT.Persistence`.

## Work order

`/speckit-tasks` should follow this order, because later steps need earlier ones:

1. **Design first** (constitution IV, ADR 0008). Make the changes in the spec's "Design changes before build" list:
   - `PageGoalDetail`, `GoalStats`, `ConsistencyStrip`, `TrendChart`;
   - `PageGoals`, `GoalListItem`;
   - `PageHomeSignedIn`, `CommitmentRow`, `StatTile`, `StatusChip` "Upcoming", `GoalSummary`;
   - `PageHomeVisitor`;
   - `design/pages.md` and `design/README.md`.

   Then copy any changed `design/css` file to `src/HT.Ui/wwwroot/css`.
2. **Domain**:
   - `HT.Domain.Reports`;
   - `GoalTimeline.EntriesThrough`;
   - the boundary unit tests (R-14), including the clock changes;
   - the Domain architecture rules.
3. **Application** and 4. **Persistence**, verified and committed **together**. After step 3 alone,
   `AddHtApplication` registers `DemoReportService`, whose port has no implementation. That fails
   `ValidateOnBuild` in every integration and host test, as in slices 2, 4 and 5.
   - Step 3:
     - `EntryViews` and `GoalListItems` extracted, with `RecordingService`'s and `GoalService`'s existing tests
       unchanged and passing;
     - `GoalEvaluation`, `ReportService`, its views, results and `ReportText`, and `ExampleGoal`;
     - `DemoReport`, `DemoReportService` and `IDemoReportStore`;
     - `FakeDemoReportStore` and the unit tests;
     - the Application rules, including the widened `IGoalStore` fence and the new `IDemoReportStore` fence.

     The bUnit page services (`tests/HT.UiTests/Components/Pages/PageServices.cs`) register `ReportService` and
     `FakeDemoReportStore` in the same commit.
   - Step 4:
     - `DemoAccountWriter` extracted from `DemoHistoryStore`, with its integration tests unchanged;
     - `DemoReportStore` and its registration;
     - the integration tests: report scoping, SC-007 timing, demo report loading, and log capture.
5. **Host**:
   - the "Demo report loading" startup step and its log lines (HP-011);
   - `StartupLogTests` and `StartupTasksDemoTests` as the spec lists them, with the new fault test.

   The smoke check waits for step 7: its checks read pages that step 6 builds.
6. **Pages**:
   - the shared components, `GoalItem` extended to the full GoalListItem, and StatusChip's "Upcoming";
   - the goal page's GoalStats and TrendChart in `GoalDetailBody`;
   - the Goals page with the `GoalsList` island;
   - the signed-in Home with the `TodayPanel` island, GoalSummary and Recent notes;
   - the visitor Home's example card.

   Each comes with bUnit and host tests. The existing tests that the spec's "Slice 4 and 5 requirements and tests
   this slice changes" names, and `StatusChipTests`, are updated in the step that changes their page.
7. **Smoke check** (HP-008), after the pages, because `SmokeCheckHostTests` runs the real smoke check against a
   real host and would fail on texts no page renders yet:
   - `PageReader.ReadStatLabels`, `ReadTileLabels` and `ReadStatusChips`, with `PageReaderTests` cases;
   - check 6's three texts and check 7, each matched in its element kind (research R-11);
   - the coverage entries;
   - in the same change:
     - `RunnerBehaviourTests.GoodSite()`;
     - `SmokeCheckHostTests`' exact lines and its "(N of N)";
     - `CoverageTests`;
     - the coverage table and check 7 in `specs/003-demo-hosting/contracts/smoke-check.md`.
8. **Docs**:
   - the roadmap status and the README status line;
   - `docs/operations/hosted-demo.md` (`demo5`, check 7);
   - `CONTEXT.md`, only if implementation sharpens a term (HP-010 is already done).
9. **AC tests**: `/speckit-ac-tests-write` and `/speckit-ac-tests-analyze`, which the owner starts after
   `/speckit-implement`.

## Gates before the next step

No gate is open:

- The spec's decisions are all recorded.
- `sources` and `features` are locked (2026-11-20).
- The features critic approved the translation. Its one MINOR note (supplied time going back between steps in
  AC-027 and AC-040) needs nothing from production code (research R-14).
- No roadmap decision is due before this slice's `/speckit-tasks`.

## Spec discrepancies

None that changes behaviour. The plan reads two passages of the spec as follows, without changing them:

1. **HP-011, "the deployment's success check also requires its result".** ADR 0015's success check is the smoke
   check after step 11's start. Check 7 signs in as `demo5@example.com` and needs its goals, so a deployment
   succeeds only when demo report loading created them. The plan adds no separate log check to the workflow
   (research R-10).
2. **HP-001, "the dashed target line", when bar targets differ.** A commitment-based yes/no Build goal's first
   week can have fewer commitments than later weeks (FR-006; AC-025: 2, 3, 3). The page draws its one dashed line
   at the latest bar's target, which includes the current week's Upcoming commitments and so is the full week's
   count. Each bar keeps its own target, stated in its tooltip and in the trend's `aria-label` (HP-006). The
   TrendChart design gets that note in step 1 (research R-05). Every other goal's bars share one target in this
   slice.

## Risks

- **The locked AC support code of F001, F002, F004 and F005 must keep compiling and passing**, and it may not be
  read. This plan changes no existing constructor, public method, port member or result type.
  - It adds `ReportService`, `DemoReportService`, one port, two DI registrations, and no migration.
  - The extractions (`EntryViews.Of` and `HistoryOf`, `GoalListItems`, `RecordingText.HoursLeft`) and the
    Persistence writer move existing code; only `HoursLeft` is a new public member, and nothing is removed. Their
    behaviour is proven unchanged by the existing unit and integration tests.
  - `/speckit-ac-tests-analyze` runs all five features.
- **F005's demo scenarios** reset the database and run demo loading and demo history loading only. Demo report
  loading is a separate call that they never make, so their results never name `demo5` (FR-031). Only the host
  runs all four tasks.
- **Two islands on pages that were static.** The Goals page and the Home each open a circuit per visit. The rest of
  each page stays static. Prerendering must stay on, or the smoke check and the first paint lose the rows. A host
  test checks the rows in the first HTML.
- **Static cards on the Home after a write.** GoalSummary and Recent notes update on the next page load, not in
  place. FR-008 asks only for the next read, and the island's own tiles and rows update in place.
- **SC-007 on CI.** The timed test runs on the CI SQL Server, whose speed varies. The bound is 2 seconds per read
  for a case that takes about 8 round trips, which leaves a wide margin. A failure is reported, never retried silently.
- **Clock-change tests** depend on NodaTime's embedded tzdb having the Kyiv and Berlin rules for 2026 and 2027,
  which the current package has (ADR 0017).
- **The hosted demo** gains a 12-week account. Each refresh re-creates it: about 135 records more than slice 5,
  in one transaction, which adds well under a second to the start.

## Complexity Tracking

No Constitution Check violation needs justifying.
