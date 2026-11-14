# Critic pass 1: tasks for 002-habit-catalog

**Verdict**: REVISE
**Date**: 2026-11-14
**Reviewed**: `specs/002-habit-catalog/tasks.md` (T001–T131), against spec.md, plan.md, research.md,
data-model.md, quickstart.md, contracts/application-api.md, contracts/host.md, the constitution, CLAUDE.md,
SPEC.md, ARCHITECTURE.md, CONTEXT.md, docs/roadmap.md, ADRs 0003, 0005, 0006, 0008, 0010 to 0014, `design/`,
the code in `src/`, the tests in `tests/` (without `tests/HT.AcceptanceTests`) and slice 1's tasks and critic
passes.

## Notes

### MAJOR-1: The Intent filter the tasks build is not the one `design/` shows, and no design task changes it

- **Where**: tasks.md T053 (line 229: "labelled groups of options with one selected each"), T065 (line 273:
  "FilterPanel Category (from `GetCategoryFilterAsync`) and Intent (All, Build, Reduce)"), T057 and T066
  (their tests), T009 (lines 105–113, the design task for both Catalog previews).
- **Against**: constitution IV ("Pages MUST follow the structure and wording of their previews", "A needed
  visual change MUST be made in `design/` first"); `design/components/FilterPanel.md` line 5 ("Radio buttons
  for Category, checkboxes for Intent"); `design/components/FilterPanel.html` line 11,
  `design/pages/PageCatalogVisitor.html` line 16 and `design/pages/PageCatalogSignedIn.html` line 16 (two
  checkboxes, "Build" and "Reduce", with no "All"); spec FR-009 ("Build, Reduce, or both") and HP-001.
- **Reasoning**: In the design, Intent is two checkboxes. T053 says every group has one selected option, and
  T065 gives Intent the three options "All, Build, Reduce". Nothing in spec.md, plan.md, research.md or the
  contracts asks for that; the wording appears only in tasks.md. T009 updates both Catalog previews but keeps
  "FilterPanel Category and Intent" as it is, and no task touches `design/components/FilterPanel`. So either
  the page is built as T053 and T065 say and differs from its preview, with T057 and T066 fixing that in
  tests, or it is built as the preview shows and does not match the tasks. The tasks' convention line 46–48
  ("follow the structure and wording of their `design/pages/` preview after the design tasks of Phase 2")
  cannot hold for this control.

### MAJOR-2: T042 leaves the new migration public and its command cannot find a startup project

- **Where**: tasks.md T042 (line 200).
- **Against**: ADR 0010, Decision ("`AccountStore`, `HtDbContext`, the design-time context factory and the
  migration stay `internal` to `HT.Persistence`"); ADR 0003 (the AC surface of `HT.Persistence` is only its
  two entry points; the test writers see the public API in reference assemblies); slice 1 tasks.md T041
  (line 242); `src/HT.Persistence/Migrations/20261111201111_InitialAccounts.cs` line 9
  (`internal partial class InitialAccounts`); the `dotnet-ef` 10.0.15 tool pinned in `dotnet-tools.json`.
- **Reasoning**:
  - `dotnet ef migrations add` writes `public partial class <Name> : Migration`. Slice 1's T041 therefore had
    the step "Change the generated migration class from `public` to `internal`", with the reason "a public
    migration would put the schema, and a `typeof(HtDbContext)` attribute naming an internal type, into the
    AC reference listing". T042 has no such step; it only reads the tables, collations, indexes and foreign
    keys. A public `HabitCatalog` class compiles, and no test notices it: `AcSurfaceTests` checks only
    `InternalsVisibleTo`, and `PersistenceDependencyTests` has one rule about `HT.Ui`. The result contradicts
    ADR 0010 and shows the three tables and their columns to the isolated AC test writer.
  - T042's command is `dotnet tool run dotnet-ef migrations add HabitCatalog --project src/HT.Persistence`.
    Slice 1 also passed `--startup-project src/HT.Persistence --output-dir Migrations`. The tool's own option
    text (read from the installed 10.0.15 package) is "The startup project to use. Defaults to the current
    working directory.", and tasks.md says paths are relative to the repository root, which holds
    `HabitTracker.slnx` and no project file. The tool's message for that case is "No project was found.
    Change the current working directory or use the --project option."

### MINOR-1: Several design tasks read as removing later slices' parts from the full previews

- **Where**: tasks.md T010 (line 114: "Remove "Your goals", the community card and "Start with this""), T011
  (lines 115–121), T012 (line 122: "Update … to the slice-2 state … one section, no Tabs, no "Updated"
  column"), T007, T009, T015.
- **Against**: `design/pages.md` line 98 ("the full previews stay as they are"); the slice 1 precedent in
  `design/pages/PageProfile.html` lines 33–35 and `design/pages/PageHomeSignedIn.html` lines 35–37 (a
  "Slice 1 state" block added after the full preview); ADR 0008, Decision ("A slice may need a page state
  that no preview shows … That state is designed in `design/` first"); docs/roadmap.md "Pages across slices"
  (`PageHabitDetail` extended in 4 and 9, `PageCustomHabit` in 4 and 10, `PageAdmin` in 10 and 11).
- **Reasoning**: The tasks mix two kinds of design edit without telling them apart. The label "Suggested
  starting goal: …" is a real replacement (constitution I). The slice-2 states are, by slice 1's precedent,
  additions beside a preview that stays whole. T010 says "Remove", and T011 and T012 say "Update … to the
  slice-2 state"; read as written, they delete from the previews the "Your goals" card, the community card,
  "Start with this", "Save and create goal", "Suggest for catalog", the Admin Tabs and the "Updated" column,
  which are the design source of slices 4, 9, 10 and 11. `design/pages/PageAdminSuggestions.md` also refers
  to "the same … Tabs as `PageAdmin`". The tasks do not say which reading is meant.

### MINOR-2: One case of T091 depends on the whole catalog of the shared database, and FR-024's Home preview has no test

- **Where**: tasks.md T091 (line 374: "a custom habit in a category with no Published habit adds no category
  to the filter (FR-024)"), the conventions (lines 56–59), T123 (line 477), T108 and T113.
- **Against**: the tasks' own convention ("Tests in shared databases … check by inclusion, exclusion and
  relative order, never by catalog totals"); research R-13 "Starting states" ("every other store test …
  holds with or without the seed"); spec FR-030 (the seed has Published habits in all six categories), spec
  "What acceptance tests can reach" (US1-AS9 needs a reset database for the same reason); FR-024 and SC-005
  (both name the Home preview).
- **Reasoning**:
  - The category filter is a property of the whole catalog. `HT-IT` is reset once per run
    (`DatabaseFixture.InitializeAsync`), catalog habits cannot be deleted, slice 1's
    `Operator/DemoLoadingTests` loads the seed with all six categories there (T123 says so), and T059 and
    T078 publish habits in several categories. Whether "a category with no Published habit"
    exists when `CrossKindTests` runs depends on the order of the test classes. With the seed present the
    case cannot be arranged at all. T099 then asks to "fix the code, never the tests".
  - T091 lists browsing, search, the category filter, details and the administrator's list, but not the Home
    preview, which does not exist until T109 (US5). T108 uses the fake catalog store, which has no custom
    habits, and T113 only counts cards. So no task checks that a custom habit stays out of the Home preview.

### MINOR-3: The bUnit page tests of US3 and US4 need fakes that no task provides

- **Where**: tasks.md T064 (line 268), T086 (line 344), T106 (line 407), T039 and T088 (the unit-test fakes).
- **Against**: `tests/HT.UiTests/HT.UiTests.csproj` (it compiles `..\HT.UnitTests\Fakes\FakeAccountStore.cs`
  as a link, with the comment "The same hand-written fake as the unit tests (ADR 0006)", and has no
  reference to `HT.UnitTests`); ADR 0006 ("Pages in bUnit tests get the real Application services with those
  fakes"); slice 1 critic-tasks-pass-1 MINOR-9 (the same class of finding).
- **Reasoning**:
  - T106 renders the custom habit page, the "My custom habits" page and the Catalog Tabs with their counts.
    These call `CustomHabitService`, which needs an `ICustomHabitStore`. The only fake of that port is
    T088's, in `tests/HT.UnitTests/Fakes/`; no task links it into `HT.UiTests` or creates another.
  - T086 tests Publish and Unpublish on rows and a table with Draft rows. T064's fake is described as "small
    … with an `Add` for Published habits", so the task that defines it does not cover what T086 needs.
  - T064 also creates a second `FakeCatalogHabitStore` beside T039's, where slice 1 shared one fake file
    between the two projects. The tasks do not say why the two differ.

### MINOR-4: The store outcome names collide with the result enums that slice 1's naming gives, and one port member has no return type

- **Where**: tasks.md T038 (line 180: `CatalogHabitCreateOutcome`, `CatalogHabitUpdateOutcome`), T092
  (line 378: `CustomHabitCreateOutcome`, `CustomHabitUpdateOutcome`, `CustomHabitDeleteOutcome`), T079
  (line 337) and T095 (line 388), T119 (line 469) and T121 (lines 471–475).
- **Against**: the code's naming in `src/HT.Application` (`RegistrationResult` with `RegistrationOutcome`,
  `SignInResult` with `SignInOutcome`, `PasswordChangeResult` with `PasswordChangeOutcome`,
  `DemoLoadingResult` with `DemoLoadingOutcome`; the store's own type is `CreateAccountOutcome`);
  contracts/application-api.md (it names outcome values but no enum type); data-model.md "Ports" (no outcome
  type names, no return type for `CreateDemoAccountAsync`); the convention line 35 ("Use the names of
  contracts/application-api.md and data-model.md").
- **Reasoning**:
  - T079 creates `CatalogHabitCreateResult` and T095 creates `CustomHabitCreateResult` and
    `CustomHabitDeleteResult`, "each with its outcomes". By slice 1's pattern their enums are
    `CatalogHabitCreateOutcome`, `CustomHabitCreateOutcome` and `CustomHabitDeleteOutcome`: the names T038
    and T092 already give to the store outcome types in the same namespace `HT.Application.Habits`. The
    tasks name neither the result enums nor a way to keep the two apart.
  - T119 gives `CreateDemoAccountAsync` its parameters only. T121 says an existing email "gives "email
    taken"" and T115's fake must report a lost race, but no artifact says what type carries that. T038 and
    T092 name the outcome types of the other two ports.

### MINOR-5: The retry limit of publish and unpublish is tested on one side, and its count can be read two ways

- **Where**: tasks.md T077 (line 332: "`FailNextStateSwaps(1)` re-reads and decides again; ten lost races in
  a row end in an `InvalidOperationException`"), T081 (line 339: "retry at most 10 times").
- **Against**: constitution II (both sides of each boundary); research R-08 ("reads again and decides again,
  at most 10 times … then fails with a fault"); slice 1 critic-tasks-pass-1 MINOR-4 and its fix in slice 1
  tasks.md line 417 ("9 lost races then a won save gives `SignedIn`; 10 lost races give `Failed`");
  constitution V (a finding of the same class appearing twice).
- **Reasoning**: The cases are one lost race and ten. The last lost race that still succeeds is not listed,
  which is the gap slice 1's critic found for the sign-in limit. "Retry at most 10 times" can mean ten
  attempts or one attempt and ten retries; in the second reading ten failed swaps are followed by an
  eleventh attempt that succeeds, and T077's test fails. The tasks do not fix the count of attempts.

### MINOR-6: Several statements about story independence, test-first order and [P] do not hold

- **Where**: tasks.md "Story dependencies" (lines 514–521), "Within each story" (line 534), "Parallel
  opportunities" (line 546), the [P] definition (lines 27–28), T070, T071, T073, T088, T101, T102, T109,
  T112, T114, T115, T116.
- **Against**: the tasks themselves; `design/pages/PageCustomHabit.md` ("usual intent (SegmentedControl)");
  slice 1 critic-tasks-pass-1 MINOR-1 and MINOR-8, and pass 2 MINOR-1 (the same class of finding).
- **Reasoning**:
  - "US2: none for its code": T070 extends `CatalogViews`, which T061 [US1] creates; T071 adds a method to
    `CatalogService.cs`, which T063 [US1] creates; T073 needs T064's fake [US1]. Line 546 says US1, US2 and
    US3 "can be built side by side".
  - "US5: … its tests need no other story": T109 adds to US1's `CatalogService.cs` and T112 uses T064's fake.
  - "US4: none for its use cases" is true for use cases, but T101's form needs the SegmentedControl that
    T082 [US3] creates. T102 lists T096, T100 and T101 as dependencies, not T082, and the Phase 2 purpose
    says the shared components every story needs are built there.
  - [P] is defined as "no dependency on an unfinished task", and line 534 says [P] tests "are written first".
    T088 is [P] and says T092, a later task without [P], "is written first". T114 is [P] and "depends on
    T117, T118"; T115 says "T119 comes first"; T116 "needs T119's types".

### MINOR-7: The roadmap's page tracker is not updated for the new design page

- **Where**: tasks.md T012 (line 122, creates `design/pages/PageAdminCatalogHabit`), T017 (line 131), T001
  and T131 (the only tasks that change `docs/roadmap.md`, both for the Status).
- **Against**: ADR 0008, Decision ("`docs/roadmap.md` ("Pages across slices") tracks which slice adds what
  to each page"); docs/roadmap.md line 113 (`PagePrivacy` (new, designed in 1), the slice 1 precedent) and
  line 123 (`PageAdmin`, with no row for the administrator's form); CLAUDE.md ("`docs/roadmap.md` … keep it
  current").
- **Reasoning**: T012 adds a page that no preview had, and T017 records it in `design/pages.md` and
  `design/README.md`. No task adds it to the roadmap's "Pages across slices" table, where slice 1 recorded
  its own new page. The same file's slice note 2 (line 73) still says "at least 20 published habits in at
  least 4 categories", while FR-030 and T114 use all six; no task touches it either.

### MINOR-8: T100 names the dialog component differently from the design guide it cites

- **Where**: tasks.md T100 (line 393: "`src/HT.Ui/Components/Shared/Dialog.razor` per
  `design/components/Dialog` (… labelled)"), line 578.
- **Against**: `design/components/Dialog.md` lines 12 and 16 ("generate it per `HtDialog` instance, never a
  fixed string"; "Blazor: one `HtDialog` component with `Title`, `Subtitle`, `ChildContent`, `Footer` render
  fragments"); `design/README.md` line 21 (`HtDialog` among the components to build once).
- **Reasoning**: The guide T100 points to names the component `HtDialog` and gives its parameters and the
  rule that the title's id is unique per instance. T100 names the file `Dialog.razor` and reduces the rule to
  "labelled". The existing `HtButton` and `HtIcon` carry the names the design files give them
  (`design/components/Button.md` line 19, `design/components/Icons.md` line 6). The task and its cited source
  disagree, and the task does not say which one wins.

### MINOR-9: Two small errors in task texts

- **Where**: tasks.md T023 (line 148: "the eleven problems of data-model.md"), T089 (line 371: "an edit of
  an unchanged row and of a missing row tell "name taken" from "not found"").
- **Against**: data-model.md `SuggestedStartingGoal` table (lines 84–95: twelve problem values over eleven
  check steps, two of them at step 4) and contracts/application-api.md (twelve suggested-starting-goal
  outcomes); tasks.md T093 (line 382: "no row changed, then a read of (`OwnerId`, `Id`) tells `NotFound` from
  `NameTaken`").
- **Reasoning**: `SuggestedStartingGoalProblem` has twelve values, from `BuildOrReduceMissing` to
  `PeriodMissing`; T031 and the contract need all twelve. In T089, "an edit of an unchanged row" reads as an
  edit that changes no value, which by T093's statement is `Saved`, not "name taken"; the case meant seems
  to be an edit refused for a taken name, and the text does not say so.

## Summary

2 MAJOR, 9 MINOR.

Every FR, HP and user story of the spec has tasks; the one coverage gap found is FR-024's Home preview
(MINOR-2). The slice 1 tests the spec names each have a task, and no other existing test was found that the
slice breaks without a task for it. Two points block approval. The Intent filter of T053 and T065 differs from the
control `design/` shows, with no design task for the change, so constitution IV cannot be met as the tasks
stand. T042 drops slice 1's step that makes the migration internal (ADR 0010) and its command, run from the
repository root, has no startup project. The MINOR notes are about the wording of the design tasks, one
integration case that depends on the shared database, missing test fakes in `HT.UiTests`, type names left
open or colliding, a one-sided retry-limit test and dependency statements that do not hold; none of them
changes the scope of the slice.
