---

description: "Task list for slice 2, habit catalog"
---

# Tasks: Habit catalog

**Input**: Design documents from `specs/002-habit-catalog/`: [plan.md](plan.md), [spec.md](spec.md),
[research.md](research.md), [data-model.md](data-model.md), [contracts/application-api.md](contracts/application-api.md),
[contracts/host.md](contracts/host.md), [quickstart.md](quickstart.md); [ADR 0014](../../docs/adr/0014-one-habit-identifier.md).

**Prerequisites**: `acceptance.md` and `tests/HT.AcceptanceTests/Features/F002/` are locked. No task edits
them, the lock files, the run records or the AC project.

**Tests**: required. The constitution (II, III, V) and `CLAUDE.md` make unit, integration, architecture and
selective UI tests part of done, so every story phase has test tasks. xUnit v2 everywhere, hand-written fakes,
no mocking library (ADR 0006). Acceptance step definitions are not tasks here: the owner starts
`/speckit-ac-tests-write` and `/speckit-ac-tests-analyze` after `/speckit-implement` (plan, Work order 8).

**Organization**: Phase 2 follows the plan's Work order steps 1 to 4 for what every story needs: design, the
whole `HT.Domain.Habits`, the shared Application types, the migration and the catalog-habit store, and the
shared components. Each story phase then adds its use cases, its store when only it needs one, its tests and
its pages. Phase 3+ order is the spec's priority order.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: can run in parallel with the other [P] tasks of its group (different files), once the tasks it
  names as dependencies are done.
- **[Story]**: the user story from spec.md (US1–US6).
- Paths are relative to the repository root.

## Conventions for every task

- Build with `dotnet build HabitTracker.slnx`. `TreatWarningsAsErrors` is on, so a warning is a failure.
- Use the names of [contracts/application-api.md](contracts/application-api.md) and [data-model.md](data-model.md).
  Every Application and port method is `async` and takes a `CancellationToken` last.
- Take message texts from `HabitTexts`, `CatalogTexts`, `CustomHabitTexts`, `OperatorTexts` and
  `AccountTexts`, never retyped. Tests assert the literal text from spec.md, so a typo in a constant fails a
  test.
- No clock or culture read in `HT.Domain` or `HT.Application`: names use `HabitName.Key` (invariant
  upper-casing, compared ordinally), never `ToUpper()`, `ToLower()`, `string.Compare(a, b)`, `CompareTo` or a
  culture comparer (constitution II, research R-03, R-12).
- Custom habits are reached only through `CustomHabitService`; every `ICustomHabitStore` call passes the acting
  user's `AccountId` as the owner (constitution III). No custom-habit name, key or description goes into a
  log template, an exception message or an error page (HP-015, research R-14).
- Pages use only `design/` tokens and `ht-` classes, and follow the structure and wording of their
  `design/pages/` preview after the design tasks of Phase 2 (constitution IV, HP-016). Parts of slices 4, 9,
  10 and 11 are absent, not disabled (ADR 0008).
- Interactive pages call use cases only through `UseCaseScope` (research R-15). Static pages inject the
  services.
- Never put a secret in a file, a log, a test name or a command line. Tests use throwaway literal values.
- No `InternalsVisibleTo` in `HT.Domain`, `HT.Application` or `HT.Persistence` (slice 1, ADR 0010;
  `AcSurfaceTests` enforces it). Tests use only public members of those assemblies and arrange or inspect
  database rows with plain SQL (`tests/HT.IntegrationTests/Sql.cs`) or through the public ports resolved from
  the application's services. `HT.Ui` may keep its `InternalsVisibleTo` for `HT.UiTests`.
- Databases: integration tests use `HT-IT` (`DatabaseFixture`); the empty-catalog tests use `HT-IT-SEED`
  (T003); host tests use `HT-UI`. No test touches `HT` or `HT-AC`. Tests in shared databases use names with
  their own prefix and check by inclusion, exclusion and relative order, never by catalog totals (research
  R-13, "Starting states").
- Commit only at a checkpoint, with the full suite green (constitution V). A service and the store
  registration it resolves land in the same commit, so `ValidateOnBuild` never meets an unregistered port
  (plan, Work order 3 and 4).

---

## Phase 1: Setup (shared infrastructure)

**Purpose**: the roadmap state, the icon files the design step needs, and two test helpers.

- [X] T001 Set slice 2's Status to `implementing` and `updated_at` to today in `docs/roadmap.md`. In the same file's slice note 2, change "at least 20 published habits in at least 4 categories" to the FR-030 rule (at least 20 Published habits covering all six categories)
- [X] T002 [P] Download the 14 Lucide icons of the spec's "Design changes before build" (`bike`, `brain`, `pen-line`, `languages`, `music`, `salad`, `utensils`, `cigarette-off`, `wine-off`, `wallet`, `receipt`, `sprout`, `sparkles`, `tv`) into `design/icons/` (research R-16):
  - **ask the owner first** (file names, source and approximate size, a few hundred bytes each) and download only after an explicit yes;
  - source: `https://cdn.jsdelivr.net/npm/lucide-static@0.460.0/icons/<name>.svg`, exactly that version, the one `design/icons/LICENSE` already covers;
  - check that each file's root `svg` attributes match those of an existing file such as `design/icons/book-open.svg`, and that `design/icons/LICENSE` needs no change.
- [X] T003 [P] Create `tests/HT.IntegrationTests/SeedDatabaseFixture.cs` (research R-13, "Starting states"):
  - its connection string is `HT_IT_CONNECTION` (or `DatabaseFixture`'s local default) with the database name replaced by `HT-IT-SEED` through `SqlConnectionStringBuilder`, so CI needs no extra variable;
  - the same guard as `DatabaseFixture` against `HT` and `HT-AC`;
  - `ResetAndBuildAsync()` resets the database with `HtDatabase.ResetAsync` and returns a new `ServiceProvider` built as in `DatabaseFixture` (`ValidateScopes`, `ValidateOnBuild`), which each test calls at its start and disposes;
  - a collection `SeedDatabase` with `DisableParallelization = true`, so its tests run one at a time.
- [X] T004 [P] Create `tests/HT.IntegrationTests/LogCapture.cs` for HP-015 (research R-13, R-14):
  - a capturing `ILoggerProvider` at `LogLevel.Trace` for every category, thread-safe, that keeps each entry's category, level, formatted message and the full text of its exception (message, inner exceptions and `ToString()`);
  - a helper that builds a `ServiceProvider` over a given connection string with `AddLogging` (minimum level `Trace`, only this provider), `AddHtApplication` and `AddHtPersistence`;
  - `AssertContainsNone(params string[] values)`: fails, naming only the entry's category and level (never the value), when any entry or exception text contains one of the values ordinally ignoring case.

**Checkpoint**: `dotnet build HabitTracker.slnx` passes.

---

## Phase 2: Foundational (blocking prerequisites)

**Purpose**: the design changes, the whole `HT.Domain.Habits`, the texts and inputs, the migration, the
catalog-habit store, `UseCaseScope` and the shared components. Every story needs them.

**⚠️ CRITICAL**: no story work starts until this phase is complete.

### Design first (constitution IV, ADR 0008; spec "Design changes before build")

Two kinds of edit, as in slice 1. The label "Suggested starting goal: …" (constitution I) and the FR-017
texts **replace** the old wording everywhere. A **slice-2 state** is **added** beside the full preview, as a
"Slice 2 state" block after it (the precedent of slice 1 in `design/pages/PageProfile.html` and
`PageHomeSignedIn.html`). The full previews stay whole (`design/pages.md`), because they are the design
source of slices 4, 9, 10 and 11 (roadmap, "Pages across slices"). "No …" in a task below describes the
slice-2 state; it never deletes anything from the full preview.

- [X] T005 [P] In `design/components/Icons.html` and `.md`, add a "Habit icons" group with the 24 names of FR-003 in FR-003's order, drawn from `design/icons/` (depends on T002). The brand book's pointer to the group is T006's, so the two tasks never edit one file
- [X] T006 [P] In `design/brand-book.md`, point Iconography to the "Habit icons" group of `design/components/Icons` (T005). Replace "Suggested start" by "Suggested starting goal: …" (constitution I) in the content rule of `design/brand-book.md` and in the notes of `design/pages/PageNewGoal.md`. In `design/components/Stepper.md`, change the rule "Minimum 0 when recording, 1 for targets" so that a Reduce target's minimum is 0 (FR-007: "from 0 to 999 for Reduce", US3-AS17), a Build target's 1, and recording 0; maximum 999 for targets. This also holds for slice 4's New goal form, which shares the Stepper. The other files that still say "Suggested start" are changed by T007, T009, T014 and T017
- [X] T007 [P] Update `design/components/HabitCard.html` and `.md`:
  - the label "Suggested starting goal: …" with FR-017 texts (for example "20 pages, 3 times a week"), and no such line for a habit without one;
  - the visitor card with "See details", the signed-in catalog card with "Details" only (no "Use this habit", "Add another goal" or "N goals");
  - the custom-habit card with the "Private" tag (text, not color alone) and an edit action, without "Suggest for catalog".
- [X] T008 [P] Add the link variant to `design/components/Tabs.html` and `.md` (research R-11): a `nav` with `a.ht-tab` links, the current one with `aria-current="page"`, for tabs that change the address. Keep the button variant for in-page tabs. In `Tabs.md`, change the example address of "My custom habits" from `/catalog?view=mine` to `/custom-habits` (plan, "Spec discrepancies" 4, accepted). The styles go in T016
- [X] T009 [P] Update `design/pages/PageCatalogVisitor.html` and `.md` and `design/pages/PageCatalogSignedIn.html` and `.md`:
  - the new card label in both;
  - the visitor page as HP-001 (heading "Habit catalog", SearchField "Search habits", the Notice "Ideas to start from. Register to turn any habit into your own goal." with Register, FilterPanel with Category as radio buttons starting with "All" and Intent as the two checkboxes "Build" and "Reduce", as `design/components/FilterPanel` already has them, the count with "Sort: A–Z" as a plain label);
  - the signed-in slice-2 state (HP-002): "+ Create custom habit", the link Tabs "Public catalog" (`/catalog`) and "My custom habits" (`/custom-habits`) with counts, no "Only habits I track", no goal tags or goal actions;
  - the "My custom habits" tab with custom-habit cards and its EmptyState;
  - the no-results EmptyState with a clear-filters action (HP-004);
  - the administrator's view without Tabs, without the custom-habit button and without the register Notice.

  Say in the notes what each tab's count counts.
- [X] T010 [P] Add the slice-2 state to `design/pages/PageHabitDetail.html` and `.md` (US2-AS6, US2-AS7): "← Catalog", the glyph, name and tags, the description, "Suggested starting goals" with "Examples to adjust, not prescribed targets.", each with its FR-017 text and kind, and the head button "Register to use this habit" for visitors only; that state has no "Your goals", no community card and no "Start with this". In the full preview too, change the texts to FR-017 ("15 min", not "15 minutes") and replace the example "1 book a month · monthly period", which FR-007 cannot express, with one it can
- [X] T011 [P] Add the slice-2 state to `design/pages/PageCustomHabit.html` and `.md` (US4-AS13, HP-007), and make the full preview's icon picker the "Habit icons" group:
  - the form: Name, Description, Category, "Usually you want to" Build or Reduce, and the icon picker from the "Habit icons" group; "Save habit", Cancel and the privacy card;
  - no "Save and create goal", no "Suggest for catalog";
  - the edit state with "Delete" and its confirmation Dialog;
  - the refusal states of FR-005, one at a time under its field;
  - "This custom habit does not exist." as a Notice above the form with a link to "My custom habits";
  - the BackLink "← My custom habits".
- [X] T012 [P] Add the slice-2 state to `design/pages/PageAdmin.html` and `.md` (US3-AS20, HP-009), keeping the full preview with its Tabs and "Updated" column for slices 10 and 11: heading "Administration", "+ New catalog habit", the privacy Notice, SearchField "Search catalog", "Showing N of M", the catalog table (Habit, Category, Intent, Status as a Tag, Edit and Publish or Unpublish per row), one section, no Tabs, no "Updated" column. Create `design/pages/PageAdminCatalogHabit.html` and `.md`, the administrator's catalog habit form (create and edit):
  - the FR-001 fields as the custom habit form has them;
  - the editor for up to 5 suggested starting goals: add (not offered at 5), remove, move up and down, and mark the card's one; each with Build or Reduce, the measurement, the amount and unit, and the schedule, using the New goal form's SegmentedControl, Stepper and DayPicker (`design/pages/PageNewGoal`);
  - a refusal under a suggested starting goal's field, and the list refusals above the list;
  - Save and Cancel, and where Save leads (the Admin table).
- [X] T013 [P] Update `design/pages/PageProfile.html` and `.md`: for registered users the link Tabs "Account" and "Custom habits" (`/profile?tab=custom-habits`) and the Custom habits tab content (the user's custom habits with a way to create and edit them); the administrator's Profile stays Account only, without Tabs (HP-008)
- [X] T014 [P] Update `design/pages/PageHomeVisitor.html` and `.md` (the new card label; the hero's secondary button "Browse the catalog"; after "How it works", "Popular in the catalog" with four HabitCards and "See all N →"; the section absent with no Published habit) and `design/pages/PageHomeSignedIn.html` and `.md` (a registered user's EmptyState under "Today" with "Browse the catalog"; the administrator's state unchanged) (HP-012, HP-013)
- [X] T015 [P] Add "Catalog" for every audience and "Admin" for administrators to `design/components/TopNav.html` and `.md` and `design/components/MobileNav.html` and `.md`, with no "Goals", "Community" or "+ New goal" (HP-011)
- [X] T016 Add the styles this slice needs to `design/css/ht-components.css` from existing tokens: the link variant of Tabs (T008), and any rule that the admin form's editor rows (T012) or the other changed previews need and the file lacks. Change `design/tokens.json` and `design/css/tokens.css` only if a new token is needed (depends on T007–T015)
- [X] T017 Update `design/pages.md` and `design/README.md`: the slice-2 structure of Catalog (both tabs and their addresses), detail, custom habit, Admin, the new admin form page, Profile, both Homes and the navigation; the label "Suggested starting goal: …" in `design/README.md`. In `docs/roadmap.md` ("Pages across slices"), add the row `PageAdminCatalogHabit` (new, designed in 2), created in 2, as slice 1 recorded `PagePrivacy` (ADR 0008) (depends on T005–T016)
- [X] T018 Copy every `design/css` file that T016 changed to `src/HT.Ui/wwwroot/css/` and run `dotnet test tests/HT.UiTests --filter "FullyQualifiedName~DesignSyncTests"` (depends on T016)

### Domain (`src/HT.Domain/Habits/`, namespace `HT.Domain.Habits`; [data-model.md](data-model.md#domain-htdomain-namespace-htdomainhabits))

- [X] T019 [P] Create `HabitId.cs` (readonly record struct over a `Guid`, ADR 0014), `Category.cs` (`Body`, `Mind`, `Health`, `Money`, `Home`, `Other`, in that order), `Intent.cs` (`Build`, `Reduce`) and `BuildOrReduce.cs` (`Build`, `Reduce`; a suggested starting goal's, never a habit's `Intent`, research R-05) in `src/HT.Domain/Habits/`
- [X] T020 [P] Create `HabitIcon.cs` in `src/HT.Domain/Habits/`: a value over the icon name; `HabitIcon.All` with the 24 names in FR-003's order; `TryParse(string? name, out HabitIcon icon)` matching exactly and case-sensitively, so `menu`, `trash-2`, `Book-open`, `""` and `null` fail
- [X] T021 [P] Create `HabitFieldProblem.cs` (`NameMissing`, `NameTooLong`, `DescriptionTooLong`, `CategoryMissing`, `IntentMissing`, `IconMissing`) and `HabitName.cs` in `src/HT.Domain/Habits/`:
  - `TryCreate(string? text, out HabitName name, out HabitFieldProblem problem)`: trims with `string.Trim()`; empty is `NameMissing`; over 60 UTF-16 code units is `NameTooLong`;
  - `Value`, and `Key` = `Value.ToUpperInvariant()`; equality is ordinal equality of keys;
  - `HabitName.Order`, an `IComparer<HabitName>` with `string.CompareOrdinal` over keys;
  - `Matches(string? search)`: true for a missing or blank search, otherwise `Key.Contains(search.Trim().ToUpperInvariant(), StringComparison.Ordinal)` (research R-03).
- [X] T022 Create `HabitDefinition.cs` in `src/HT.Domain/Habits/`: immutable `Name`, `Description` (trimmed, null when empty after trimming, at most 500 code units), `Category`, `Intent`, `Icon`; `TryCreate(string? name, string? description, Category? category, Intent? intent, string? icon, out HabitDefinition definition, out HabitFieldProblem problem)` checking in the order of data-model.md's table (depends on T019–T021)
- [X] T023 [P] Create the suggested starting goal types in `src/HT.Domain/Habits/` (data-model.md `SuggestedStartingGoal`, research R-05):
  - `Measurement` (`YesNo`, `Count`, `Duration`), `DurationUnit` (`Minutes`, `Hours`), `ScheduleKind` (`FixedDays`, `Times`), `SchedulePeriod` (`Day`, `Week`);
  - `Schedule`: fixed days with a non-empty `IReadOnlySet<DayOfWeek>`, or times with `Count` 1 to 20 and a `Period`;
  - `SuggestedStartingGoalFields` with one nullable value per form control;
  - `SuggestedStartingGoalProblem` with the twelve problems of data-model.md (eleven check steps, two problems at step 4);
  - `SuggestedStartingGoal` (immutable) and `TryCreate(SuggestedStartingGoalFields fields, out SuggestedStartingGoal goal, out SuggestedStartingGoalProblem problem)`, checking in data-model.md's order. A count unit is stored trimmed with its case and spelling unchanged. Values that do not belong to the measurement or schedule kind are neither refused nor stored (plan, "Spec discrepancies" 1). A count unit empty after trimming is no unit for a yes/no (discrepancy 2). A missing number of times is `TimesOutOfRange` (discrepancy 5).

  Depends on T019.
- [X] T024 Create `SuggestedStartingGoals.cs` in `src/HT.Domain/Habits/`: an ordered list of 0 to 5 with `MarkedIndex` (null exactly when empty) and `Marked`; `TryCreate(IReadOnlyList<(SuggestedStartingGoalFields Fields, bool IsMarked)> items, out SuggestedStartingGoals goals, out SuggestedStartingGoalsProblem problem)`, where the problem is `TooMany`, `MarkMissing` (none or several marks on a non-empty list) or an item problem with its `Position` counted from 1, checked in that order and the items in list order (depends on T023)
- [X] T025 Create `SuggestedStartingGoalText.cs` in `src/HT.Domain/Habits/`: the static pure `Format(SuggestedStartingGoal goal)` returning `(string Text, string Kind)` by FR-017's rules (amount part, schedule part, the joining rule, "at most " for Reduce, the kind with day runs: Monday first, no wrap, a run of three or more as first–last with U+2013, other days alone, comma-separated, day names Mon … Sun) (research R-06; depends on T023)
- [X] T026 [P] Create `CatalogHabitState.cs` (`Draft`, `Published`, `Unpublished`) and `CatalogHabitStates.cs` in `src/HT.Domain/Habits/`: `Publish(state)` and `Unpublish(state)` return the next state (equal to the current one for Published → Publish and Unpublished → Unpublish) or the refusal `NotPublished` for Draft → Unpublish, as data-model.md's transition table
- [X] T027 Create `CatalogHabit.cs` (`Id`, `Definition`, `SuggestedStartingGoals`, `State`) and `CustomHabit.cs` (`Id`, `Owner` as `AccountId`, `Definition`; no suggested starting goals) in `src/HT.Domain/Habits/`, both immutable (depends on T022, T024, T026)
- [X] T028 [P] Create `tests/HT.UnitTests/CultureScope.cs`, an `IDisposable` that sets `CultureInfo.CurrentCulture` and `CurrentUICulture` and restores them. Then create `tests/HT.UnitTests/Domain/Habits/HabitNameTests.cs`, every test inside `CultureScope("tr-TR")` (research R-13):
  - trimming; `" ac17 reading "` and `"Ac17 Reading"` have one key;
  - 60 code units accepted, 61 refused, after trimming; `"   "` and `null` are `NameMissing`;
  - order: `"A habit"` before `"b habit"`; `"Ac2 Breathing"`, `"Ac2 Cycling"`, `"Ac2 walking"`; `"Éclair"` after `"Zest"`; `"Éclair"` and `"éclair"` have one key;
  - `"ſun"` and `"Sun"` have one key; `"ß"` keeps its key; Turkish `"İ"` and `"ı"` give the invariant result, not the `tr-TR` one;
  - `Matches`: `" morning "` matches `"Ac5 Morning pages"` and not `"Ac5 Evening walk"`; blank and `null` match everything.
- [X] T029 [P] Create `tests/HT.UnitTests/Domain/Habits/HabitIconTests.cs`: `All` has the 24 FR-003 names in order with no duplicate; each parses; `menu`, `trash-2`, `Book-open`, `" book-open"`, `""` and `null` fail
- [X] T030 [P] Create `tests/HT.UnitTests/Domain/Habits/HabitDefinitionTests.cs`: each problem of FR-005 alone; the order when several fields are wrong (for example a blank name with no icon gives `NameMissing`); description 500 accepted and 501 refused after trimming; a blank description becomes `null`; an icon not in FR-003 is `IconMissing`
- [X] T031 [P] Create `tests/HT.UnitTests/Domain/Habits/SuggestedStartingGoalTests.cs` with every row of data-model.md's check table and both sides of every boundary (constitution II):
  - Build amount 0/1 and 999/1000, a missing Build amount; Reduce amount -1/0 and 999/1000;
  - count unit 20/21 after trimming, `"  "` refused, `"  pages  "` stored as `"pages"`, `"Pages"` kept as entered;
  - times 0/1 and 20/21, missing times; times without a period; fixed days with no day;
  - a yes/no with an amount, with a count unit `"x"`, with a duration unit (refused), and with a count unit `"  "` (accepted, no unit);
  - a duration unit on a count, a count unit on a duration and days with times are neither refused nor stored;
  - the order when several fields are wrong.
- [X] T032 [P] Create `tests/HT.UnitTests/Domain/Habits/SuggestedStartingGoalsTests.cs`: empty list (no mark, `MarkedIndex` null); 5 with one mark accepted and 6 with none gives `TooMany` (count first); two with none and two with both marked give `MarkMissing`; an item problem in the second item gives position 2; a problem in items 2 and 3 gives the first; the order is kept
- [X] T033 [P] Create `tests/HT.UnitTests/Domain/Habits/SuggestedStartingGoalTextTests.cs` with every example of data-model.md's `SuggestedStartingGoalText` table (US2-AS1, US2-AS3, US3-AS17), plus the run-rule boundaries: two consecutive days (`"Mon, Tue"`), three (`"Mon–Wed"`), the Sunday–Monday pair (`"Mon, Sun"`), all seven (`"every day"`, text `"daily"`), N times a day, and the hours unit
- [X] T034 [P] Create `tests/HT.UnitTests/Domain/Habits/CatalogHabitStatesTests.cs` with each of the six cells of the transition table
- [X] T035 [P] Extend `tests/HT.ArchitectureTests/ClockReadTests.cs` with the culture-sensitive members of research R-12 (`CultureInfo.get_CurrentUICulture`, the parameterless `string.ToUpper()` and `string.ToLower()`, `char.ToUpper(char)`, `char.ToLower(char)`, `string.Compare(string, string)` and its `bool ignoreCase` overload, `string.CompareTo`, `StringComparer.get_CurrentCulture` and `get_CurrentCultureIgnoreCase`), forbidden in `HT.Domain` and `HT.Application`. Check that the existing slice-1 code still passes

### Application: texts, inputs and the catalog-habit port (`src/HT.Application/Habits/`)

- [X] T036 [P] Create `HabitTexts.cs`, `CatalogTexts.cs` and `CustomHabitTexts.cs` in `src/HT.Application/Habits/` as `public static class` with the `public const string` texts of [contracts/application-api.md](contracts/application-api.md#texts), worded exactly as there. `OperatorTexts` gains its texts in US6 (T119)
- [X] T037 [P] Create the inputs of data-model.md's "Application inputs" in `src/HT.Application/Habits/`: `HabitInput`, `CatalogHabitInput`, `SuggestedStartingGoalInput` and `CatalogFilter`
- [X] T038 Create `src/HT.Application/Habits/ICatalogHabitStore.cs`, a `public` interface with the members of data-model.md's "Ports", and its outcome types, named verb first as slice 1's store outcome `CreateAccountOutcome`: `CreateCatalogHabitOutcome` (`NameTaken`, or `Created` with the `HabitId`) and `UpdateCatalogHabitOutcome` (`NotFound`, `NameTaken`, `Saved`). The use-case results keep slice 1's other pattern, `<UseCase>Result` with a `<UseCase>Outcome` enum (for example `CatalogHabitCreateResult` and `CatalogHabitCreateOutcome`), so the two never share a name. The store has no rule of its own (research R-08) (depends on T027)
- [X] T039 [P] Create `tests/HT.UnitTests/Fakes/FakeCatalogHabitStore.cs`, an in-memory `ICatalogHabitStore`, shared with `HT.UiTests` by a link (T064), as slice 1 shares `FakeAccountStore`:
  - new identifiers with `Guid.NewGuid()`; uniqueness on `HabitName.Key` over every state, the edited habit excluded;
  - `Add(CatalogHabit)` for arranging a habit in any state directly;
  - `RefuseNameOnNextWrite()`, so the next create or update gives `NameTaken` after the service's own check passed (the lost race of HP-018);
  - `FailNextStateSwaps(int count)`, so `TrySetStateAsync` returns false that many times and leaves the stored state unchanged (the lost state race of research R-08);
  - `ChangeStateOnNextFailedSwap(CatalogHabitState state)`, so the next failed swap also sets the stored state to `state`, as another administrator's write between the swap and the re-read would (slice 1's `FakeAccountStore.BlockOnNextSave` is the same kind of member);
  - `WriteCount`, for "nothing changes".

  Depends on T038.

### Persistence: tables, migration and the catalog-habit store (`src/HT.Persistence/`)

- [X] T040 [P] Create the internal rows in `src/HT.Persistence/Habits/`: `CatalogHabitRow`, `SuggestedStartingGoalRow` and `CustomHabitRow`, with the columns of [data-model.md](data-model.md#tables-htpersistence-migration-habitcatalog)
- [X] T041 Map the three tables in `src/HT.Persistence/HtDbContext.cs`:
  - `CatalogHabits` with key `Id`, `NameKey` with `UseCollation("Latin1_General_100_BIN2")` and the unique index `IX_CatalogHabits_NameKey`;
  - `CatalogHabitSuggestedStartingGoals` as an owned collection of `CatalogHabits` (`OwnsMany`) with key (`CatalogHabitId`, `Position`) and cascade delete;
  - `CustomHabits` with key `Id`, `OwnerId` with a foreign key to the Identity user table (cascade delete), `NameKey` binary collation and the unique index `IX_CustomHabits_OwnerId_NameKey` on (`OwnerId`, `NameKey`);
  - enums as strings (`HasConversion<string>()`, the column types of data-model.md), days as the `int` bit mask (Monday 1 … Sunday 64); no check constraints, no default values, no stored text or kind.

  Depends on T040.
- [X] T042 Add the migration `HabitCatalog` from the repository root with `dotnet ef migrations add HabitCatalog --project src/HT.Persistence --startup-project src/HT.Persistence --output-dir Migrations` (slice 1's T041 command and its design-time factory). Change the generated migration class from `public` to `internal`, as `InitialAccounts` is (ADR 0010: the migration stays internal, so the schema stays out of the AC reference listing). Read the generated migration and its designer file: the three tables, the two binary collations, the two unique indexes, the foreign keys and nothing else; no data. Run `HtDatabase.ResetAsync` through the existing integration tests to apply it (depends on T041)
- [X] T043 Create `src/HT.Persistence/Habits/CatalogHabitStore.cs` (internal, `ICatalogHabitStore`), following slice 1's `AccountStore`:
  - reads no-tracking; `ListPublishedAsync` returns every Published habit, `ListAllAsync` every habit, `FindPublishedAsync` only a Published one;
  - the mapping between rows and domain types lives here only, through the Domain `TryCreate` checks; a row that fails them is an `InvalidOperationException` with no field value in its message;
  - create writes `Draft`, `NameKey` = `HabitName.Key` and the list positions 1 to n; update loads the tracked habit with its list, replaces the fields and the whole list, and saves once;
  - a unique-index violation on `IX_CatalogHabits_NameKey` (SQL error 2601 or 2627) is `NameTaken`; catalog names are public, so slice 1's pattern applies (research R-14);
  - `TrySetStateAsync` is one `ExecuteUpdateAsync` with `WHERE Id = @id AND State = @expected`, true when a row changed;
  - `NameKeyExistsAsync` compares the key column; the change tracker is cleared before and after each write.

  Depends on T038 and T042.
- [X] T044 In `src/HT.Persistence/PersistenceServiceCollectionExtensions.cs`, register `ICatalogHabitStore` (scoped) and configure the context with `ConfigureWarnings(w => w.Ignore(CoreEventId.SaveChangesFailed, RelationalEventId.CommandError))`; sensitive-data logging stays off (research R-14; depends on T043)
- [X] T045 [P] Create `tests/HT.IntegrationTests/Habits/CatalogHabitStoreTests.cs` (collection `Database`, names with the prefix `It-Cat`):
  - create, then `FindAsync` returns every field, the list in order and the mark, state `Draft`; `FindPublishedAsync` and `ListPublishedAsync` do not return it;
  - update with a longer, a shorter and a reordered list, and with an empty one; the rows of `CatalogHabitSuggestedStartingGoals` match (plain SQL);
  - the key under the binary collation: after `"It-Cat Key"`, `" it-cat KEY "` is `NameTaken`, also when the first is Unpublished; after `"It-Cat ſun"`, `"It-Cat Sun"` is `NameTaken`; renaming a habit to its own name in other case is `Saved`;
  - `TrySetStateAsync` with the right and with a wrong expected state;
  - HP-018: two parallel creates with one name, each in its own scope, give one `Created` and one `NameTaken`, and one row; the same for two parallel renames to one name.

  Depends on T044.
- [X] T046 Extend `tests/HT.ArchitectureTests/UiDependencyTests.cs`: no `HT.Ui` type depends on `ICatalogHabitStore` (research R-12, ADR 0010). This file is extended again by T098 and T122

### Host and shared components (`src/HT.Ui/`)

- [X] T047 Create `src/HT.Ui/Habits/UseCaseScope.cs` (research R-15): `RunAsync<TService, TResult>(Func<TService, CancellationToken, Task<TResult>> call, CancellationToken cancellationToken)` creates a new `AsyncServiceScope` from `IServiceScopeFactory` for each call, resolves the service there, awaits the call and disposes the scope, also when the call throws. Register it in `src/HT.Ui/Program.cs`. Create `tests/HT.UiTests/Habits/UseCaseScopeTests.cs`: two calls get two different scoped instances, two overlapping calls never share one, and the scope is disposed after a call that throws
- [X] T048 In `src/HT.Ui/Components/Shared/HtIcon.razor`, add the drawings of the 24 habit icons and of `search`, `layout-grid`, `plus`, `pencil`, `trash-2` and `chevron-left`, each copied from its `design/icons/<name>.svg`, and a public static list of the names it knows (research R-16; depends on T002)
- [X] T049 [P] Create `tests/HT.UiTests/Components/Shared/HtIconDrawingTests.cs` (research R-16): for every name `HtIcon` knows, `design/icons/<name>.svg` exists and the child elements of its root `svg` (elements and attributes, whitespace and attribute order normalised) equal what bUnit renders inside `HtIcon`'s `svg`; every name of `HabitIcon.All` is one `HtIcon` knows. Find the repository root as `DesignSyncTests` does (depends on T048)
- [X] T050 [P] Create `src/HT.Ui/Components/Shared/Tag.razor` per `design/components/Tag` (text always present, never color alone)
- [X] T051 [P] Create `src/HT.Ui/Components/Shared/Tabs.razor` per `design/components/Tabs` with its button variant and the link variant of T008 (`nav` with `a.ht-tab`, `aria-current="page"` on the current one)
- [X] T052 [P] Create `src/HT.Ui/Components/Shared/SearchField.razor` per `design/components/SearchField`: a labelled search input that reports each change; the debounce belongs to the page
- [X] T053 [P] Create `src/HT.Ui/Components/Shared/FilterPanel.razor` per `design/components/FilterPanel`: native inputs inside `label.ht-check`, Category as a radio group and Intent as two checkboxes ("Build", "Reduce"), each group titled, reporting a change at once, with no Apply button. "Only habits I track" is slice 4's and absent
- [X] T054 [P] Create `src/HT.Ui/Components/Shared/BackLink.razor` per `design/components/BackLink`
- [X] T055 [P] Create `src/HT.Ui/Components/Shared/TextArea.razor` per `design/components/TextArea`, labelled, with its error text under the field as `TextField.razor` has it
- [X] T056 Create `src/HT.Ui/Components/Shared/HabitCard.razor` per `design/components/HabitCard` after T007: icon, name, Tags for category and intent, the description, the line "Suggested starting goal: …" only when a text is given, and the variants visitor ("See details"), signed-in ("Details") and custom habit ("Private" Tag and an edit action). The card receives plain values; it formats nothing itself (ADR 0011) (depends on T048, T050)
- [X] T057 [P] Create `tests/HT.UiTests/Components/Shared/HabitComponentTests.cs` (bUnit) for T050–T056: each HabitCard variant and its links; no suggested starting goal line without a text; the label text exactly; Tag text; the link Tabs' `aria-current`; the SearchField label and change callback; the FilterPanel's radio group and two checkboxes, their labels and change callbacks; BackLink; TextArea's label and error (depends on T050–T056)

**Checkpoint**: `dotnet build HabitTracker.slnx` and `dotnet test HabitTracker.slnx` pass. Commit (design, Domain, the
catalog-habit store and its registration together).

---

## Phase 3: User Story 1 - Browse the catalog (Priority: P1) 🎯 MVP

**Goal**: anyone browses the Published catalog habits in name order, filtered by category, intent and a name
search, with each habit's marked suggested starting goal (FR-008, FR-009).

**Independent Test**: arrange Published, Draft and Unpublished habits in different categories; browse as an
anonymous visitor and confirm that only the Published ones appear, in name order, and that each filter narrows
the list; open `/catalog` and change the filters.

### Tests for User Story 1

> Write these first; they must fail before T061–T064 exist.

- [X] T058 [P] [US1] Create `tests/HT.UnitTests/Application/Habits/CatalogBrowseTests.cs` with `FakeCatalogHabitStore`:
  - US1-AS1: only Published habits, each with category, intent, icon, description and the marked suggested starting goal with its text and kind; no Draft, no Unpublished;
  - US1-AS2: name order ignoring case; ordinal order (`"Éclair"` after `"Zest"`);
  - US1-AS3, US1-AS4, US1-AS5 (`" morning "`), and all three filters together;
  - US1-AS7: no match gives `Found` with an empty list and `Count` 0;
  - US2-AS2 for browsing: a habit with no suggested starting goal has `MarkedSuggestedStartingGoal` null;
  - US1-AS8 and US1-AS9: the category filter is "All" first, then only categories with a Published habit, in FR-002 order; a Draft's category is absent.
- [X] T059 [P] [US1] Create `tests/HT.IntegrationTests/Habits/CatalogBrowseTests.cs` (collection `Database`, prefix `It-Br`) through `CatalogService` against SQL Server, arranging habits through `ICatalogHabitStore`: US1-AS1 to US1-AS5 by inclusion, exclusion and relative order
- [X] T060 [P] [US1] Create `tests/HT.ArchitectureTests/UseCaseParameterTests.cs` (plain reflection, research R-12): no public method of `CatalogService` takes an `ActingUser` or an `AccountId`. T077 and T097 extend it

### Implementation for User Story 1

- [X] T061 [US1] Create in `src/HT.Application/Habits/`: the views `CatalogHabitSummary`, `SuggestedStartingGoalView` and `CategoryFilterOption` of [contracts/application-api.md](contracts/application-api.md#view-types), and an internal `CatalogViews` that maps a `CatalogHabit` to them, taking each suggested starting goal's `Text` and `Kind` from `SuggestedStartingGoalText` (ADR 0011)
- [X] T062 [US1] Create the results `CatalogBrowseResult` (`Outcome` `Found`, `Habits`, `Count`, `Message` null) and `CategoryFilterResult` (`Found`, `Options`) in `src/HT.Application/Habits/`, following slice 1's result shape (its research R-06)
- [X] T063 [US1] Create `src/HT.Application/Habits/CatalogService.cs` with `BrowseAsync(CatalogFilter filter, CancellationToken)` and `GetCategoryFilterAsync(CancellationToken)`, taking no acting user (research R-07): read `ListPublishedAsync`, filter in memory (category, intent, `HabitName.Matches`), order with `HabitName.Order`. Register it (scoped) in `src/HT.Application/ApplicationServiceCollectionExtensions.cs` (depends on T061, T062)
- [X] T064 [US1] In `tests/HT.UiTests/HT.UiTests.csproj`, link T039's `..\HT.UnitTests\Fakes\FakeCatalogHabitStore.cs` (`<Compile Include … Link="Fakes\FakeCatalogHabitStore.cs" />`), as the project already links `FakeAccountStore.cs`, so the bUnit tests use the real services over the same fake (ADR 0006)
- [X] T065 [US1] Create `src/HT.Ui/Components/Pages/Catalog.razor` at `/catalog`, Interactive Server with prerendering (research R-11, [contracts/host.md](contracts/host.md)):
  - `q`, `category` (lower-case category name) and `intent` (`build`, `reduce`) read with `[SupplyParameterFromQuery]`; an unknown value is ignored. Intent follows the two checkboxes: only "Build" checked is `intent=build` (filter Build), only "Reduce" is `intent=reduce`, and none or both checked is no `intent` value (both, FR-009);
  - a filter change applies at once and writes the address with `NavigationManager.NavigateTo(…, replace: true)`; the search does so 300 ms after the last keystroke, cancels a pending search on newer input, and ignores a result older than the latest request (research R-15);
  - every call through `UseCaseScope`;
  - the visitor page of HP-001: heading "Habit catalog", SearchField "Search habits", the register Notice with Register, FilterPanel with Category (the options of `GetCategoryFilterAsync`, "All" first) and the Intent checkboxes, the count with "Sort: A–Z" as a plain label, a HabitCard per habit with "See details" linking to `/catalog/{id}`;
  - registered users and administrators: cards with "Details" and no register Notice (HP-002; the Tabs and the custom-habit button come in US4, T104);
  - with no match, an EmptyState that says nothing matches and clears the filters (HP-004).

  Depends on T063.
- [X] T066 [P] [US1] Create `tests/HT.UiTests/Components/Pages/CatalogPageTests.cs` (bUnit, over T064's fake): the visitor structure of US1-AS10; a category and an intent change the cards without an Apply button and replace the address with the query (US1-AS11); checking both Intent boxes, or none, shows both intents and drops `intent` from the address; query values set the filters on open; an unknown value is ignored; the debounced search; the EmptyState and its clear action; the signed-in and administrator variants without the Notice (depends on T065)
- [X] T067 [P] [US1] Create `tests/HT.UiTests/Host/CatalogHostTests.cs`: `/catalog` answers 200 for an anonymous visitor with the prerendered heading and cards; `/catalog?category=money&intent=reduce&q=…` prerenders only matching habits (arranged through `ICatalogHabitStore` from the host's services, prefix `Ui-Br`) (depends on T065)

**Checkpoint**: full suite green; `/catalog` usable in the browser. Commit.

---

## Phase 4: User Story 2 - Catalog habit details (Priority: P1)

**Goal**: anyone reads a Published habit's details with all its suggested starting goals; everything else is
"This habit is not in the catalog." and the Not found page (FR-010, HP-005, HP-006).

**Independent Test**: open a Published habit and check its full content; open a Draft, an Unpublished and an
unknown habit and get the same result and the same Not found page.

### Tests for User Story 2

- [X] T068 [P] [US2] Create `tests/HT.UnitTests/Application/Habits/CatalogDetailsTests.cs`: US2-AS1 (`Ac7 Reading`, three suggested starting goals in order with fields, mark, texts and kinds); US2-AS2 (none); two of US2-AS3's goals through the service (the rest are T033's); US2-AS4 Draft, Unpublished and an unknown identifier give `NotInCatalog` with `CatalogTexts.NotInCatalog`
- [X] T069 [P] [US2] Create `tests/HT.IntegrationTests/Habits/CatalogDetailsTests.cs` (prefix `It-De`): US2-AS1 and US2-AS4 against SQL Server

### Implementation for User Story 2

- [X] T070 [US2] Create the view `CatalogHabitDetails` and the result `CatalogHabitDetailsResult` (`NotInCatalog` with its text, `Found` with the details) in `src/HT.Application/Habits/`, and extend `CatalogViews` (T061)
- [X] T071 [US2] Add `GetDetailsAsync(HabitId id, CancellationToken)` to `src/HT.Application/Habits/CatalogService.cs` over `FindPublishedAsync` (depends on T070)
- [X] T072 [US2] Create `src/HT.Ui/Components/Pages/CatalogHabitDetail.razor` at `/catalog/{id:guid}`, static server rendering, per `design/pages/PageHabitDetail` after T010: BackLink "← Catalog", glyph, name and tags, description, "Suggested starting goals" with "Examples to adjust, not prescribed targets." and each goal's text and kind in order; the head button "Register to use this habit" (→ `/register`) for anonymous visitors only. `NotInCatalog` calls `NavigationManager.NotFound()` (research R-11) (depends on T071)
- [X] T073 [P] [US2] Create `tests/HT.UiTests/Components/Pages/CatalogHabitDetailTests.cs` (bUnit): US2-AS6 for a visitor, US2-AS7 for a signed-in user (no head button), a habit with no suggested starting goal, and no "Your goals", community card or "Start with this" (depends on T072)
- [X] T074 [P] [US2] Create `tests/HT.UiTests/Host/CatalogHabitDetailHostTests.cs`: a Published habit answers 200 with its content; a Draft, an Unpublished and an unknown identifier answer 404 with slice 1's Not found page, for an anonymous visitor, a registered user and an administrator (US2-AS8, HP-006); `/catalog/not-a-guid` answers 404. If `NotFound()` does not set 404 during prerendering, apply the fallback of plan.md's Risks and record it in the PR (depends on T072)

**Checkpoint**: full suite green; catalog cards open their details. Commit.

---

## Phase 5: User Story 3 - Administer the catalog (Priority: P1)

**Goal**: administrators list, read, create, edit, publish and unpublish catalog habits; registered users and
visitors are refused (FR-011 to FR-016, HP-009, HP-010).

**Independent Test**: as an administrator create, edit, publish and unpublish one habit, browsing after each
step; as a registered user and a visitor, each action is refused.

### Tests for User Story 3

- [X] T075 [P] [US3] Create `tests/HT.UnitTests/Application/Habits/CatalogAdministrationAccessTests.cs` with `FakeCatalogHabitStore` (SC-006):
  - US3-AS11: a registered user gets `NotAdministrator` with "Only administrators can manage the catalog." and an anonymous visitor `NotSignedIn` with "You are not signed in.", for each of the six use cases, with `WriteCount` unchanged;
  - US3-AS12: an unknown identifier gives `NotFound` with "This catalog habit does not exist." for read, edit, publish and unpublish;
  - US3-AS18: FR-016's order across steps (a registered user with a blank name; an administrator editing an unknown identifier with a blank name).
- [X] T076 [P] [US3] Create `tests/HT.UnitTests/Application/Habits/CatalogHabitFieldTests.cs`:
  - US3-AS1: create gives `Created` in Draft; the administrator's list shows it Draft; browsing does not;
  - US3-AS3, US3-AS14 (two suggested starting goals, the second marked, in order; browsing after publish shows the second);
  - US3-AS9: `" ac17 reading "` is `NameTaken` on create and on rename, for a habit in each state, with nothing changed; renaming a habit to its own name in other case is `Saved`;
  - US3-AS10: each FR-005 refusal, and 60 and 500 accepted;
  - US3-AS15 and US3-AS16: every list and item refusal with `Position` 2 and its text, and every accepted boundary; US3-AS17 (Reduce 0);
  - a lost uniqueness race (`RefuseNameOnNextWrite`) gives `NameTaken`;
  - US3-AS19: the search `"ac28 ro"` lists `"Ac28 Rowing"` with its state and not `"Ac28 Running"`, and `Total` counts every catalog habit;
  - FR-012: the list holds every state in name order, ignoring case and ordinal (`"b Draft"` after `"A Published"`, `"Éclair"` after `"Zest"`, with Draft, Published and Unpublished habits mixed); an edit of a Draft and of an Unpublished habit is `Saved` and keeps its state.
- [X] T077 [P] [US3] Create `tests/HT.UnitTests/Application/Habits/CatalogHabitStateTests.cs`: US3-AS2, AS4, AS5; AS6 (Draft → `NotPublished`, stays Draft); AS7 and AS8 (success with no write); the race limit on both sides (constitution II; research R-08, as slice 1's sign-in limit): `FailNextStateSwaps(9)` then a won swap gives `Published`, and `FailNextStateSwaps(10)` ends in an `InvalidOperationException` with no swap won; the same two cases for unpublish; one lost race whose re-read finds the other administrator's state already the wanted one (`FailNextStateSwaps(1)` with `ChangeStateOnNextFailedSwap(Published)` on a Draft being published) gives success with no further write. Extend `tests/HT.ArchitectureTests/UseCaseParameterTests.cs`: every public method of `CatalogAdministrationService` takes `ActingUser` first and no `AccountId`
- [X] T078 [P] [US3] Create `tests/HT.IntegrationTests/Habits/CatalogAdministrationTests.cs` (prefix `It-Ad`), through the service against SQL Server: US3-AS1 to US3-AS5 with browsing after each step (SC-003); US3-AS9; an edit of a Draft keeps it Draft and out of browsing (FR-012); the list's relative name order of its own habits; two administrators creating one name in parallel get one `Created` and one `NameTaken` (HP-018); two parallel publishes of one Draft both succeed and leave it Published

### Implementation for User Story 3

- [X] T079 [US3] Create in `src/HT.Application/Habits/`: the views `CatalogHabitListItem` and `CatalogHabitAdminDetails`; the results `CatalogHabitListResult` (`Habits`, `Total`), `CatalogHabitAdminResult`, `CatalogHabitCreateResult`, `CatalogHabitEditResult`, `CatalogHabitPublishResult` and `CatalogHabitUnpublishResult`, each with its `<UseCase>Outcome` enum (T038) holding the outcomes in the order of the contract table, `Message` and, for a suggested starting goal's refusal, `Position`
- [X] T080 [US3] Create an internal `CatalogHabitInputCheck` in `src/HT.Application/Habits/` that turns a `CatalogHabitInput` into a `HabitDefinition` and `SuggestedStartingGoals` with the Domain checks, or into the named outcome and text of the first problem; `BuildOrReduceMissing` uses `HabitTexts.IntentMissing` and keeps its own outcome with the position (contract, "Texts") (depends on T079)
- [X] T081 [US3] Create `src/HT.Application/Habits/CatalogAdministrationService.cs` with `ListAsync`, `GetAsync`, `CreateAsync`, `EditAsync`, `PublishAsync` and `UnpublishAsync`, the acting user first, checking in FR-016's order; the list over `ListAllAsync`, filtered with `HabitName.Matches` and ordered with `HabitName.Order`; an edit never changes the state (FR-012); uniqueness first by `NameKeyExistsAsync`, then by the store's outcome; publish and unpublish read, decide with `CatalogHabitStates` and write with `TrySetStateAsync`, in at most 10 attempts in all (a constant `MaxStateSwaps = 10`, as slice 1's `MaxSignInSaves`), then fail with an `InvalidOperationException` (research R-08). Register it (scoped) in `ApplicationServiceCollectionExtensions.cs` (depends on T080)
- [X] T082 [P] [US3] Create `DataTable.razor`, `SegmentedControl.razor`, `Stepper.razor` and `DayPicker.razor` in `src/HT.Ui/Components/Shared/` per their `design/components/` files, each labelled for assistive technology, and their bUnit tests in `tests/HT.UiTests/Components/Shared/FormControlTests.cs`
- [X] T083 [US3] Create `src/HT.Ui/Components/Habits/SuggestedStartingGoalEditor.razor` and `src/HT.Ui/Components/Habits/CatalogHabitForm.razor` per `design/pages/PageAdminCatalogHabit` (T012): the FR-001 fields (TextArea for the description, the icon picker of the "Habit icons" group); up to 5 suggested starting goals with add (not offered at 5), remove, move up and down and mark; each with the FR-007 controls, only those of the chosen measurement and schedule shown (plan, "Spec discrepancies" 1), the amount's Stepper from 0 for Reduce and from 1 for Build, up to 999 (T006's Stepper rule); a refusal under its field, and under the suggested starting goal of its `Position` (HP-009) (depends on T082)
- [X] T084 [US3] Create `src/HT.Ui/Components/Pages/Admin.razor` at `/admin`, `[Authorize]`, Interactive Server with prerendering, per `design/pages/PageAdmin` (US3-AS20): heading "Administration", "+ New catalog habit" (→ `/admin/catalog/new`), the privacy Notice, SearchField "Search catalog" (`q` in the address, 300 ms debounce), "Showing N of M", and the table with Status as a Tag, Edit, and Publish or Unpublish per row showing the new state at once. `NotAdministrator` calls `NavigationManager.NotFound()` (HP-010). Every call through `UseCaseScope` (depends on T081)
- [X] T085 [US3] Create `src/HT.Ui/Components/Pages/AdminCatalogHabit.razor` at `/admin/catalog/new` and `/admin/catalog/{id:guid}`, `[Authorize]`, Interactive Server with prerendering, over `CatalogHabitForm`: create, or edit a habit in any state; after a save, back to `/admin`; `NotAdministrator` and `NotFound` call `NavigationManager.NotFound()` (depends on T083, T084)
- [X] T086 [P] [US3] Create `tests/HT.UiTests/Components/Pages/AdminPageTests.cs` and `tests/HT.UiTests/Components/Habits/CatalogHabitFormTests.cs` (bUnit): the Admin structure and "Showing N of M"; Publish and Unpublish change the row's Tag at once; the editor's add, remove, move and mark, no add at 5; only the controls of the chosen measurement and schedule; a refusal with position 2 under the second suggested starting goal (depends on T084, T085)
- [X] T087 [P] [US3] Create `tests/HT.UiTests/Host/AdminHostTests.cs` for `/admin`, `/admin/catalog/new` and `/admin/catalog/{id}`: an anonymous visitor is sent to `/sign-in?returnUrl=…`, a registered user gets 404 with the Not found page, an administrator gets 200; an unknown identifier gives 404 (HP-010, US5-AS4) (depends on T085)

**Checkpoint**: full suite green; the MVP (US1–US3) is demonstrable with an administrator's own habits.
Commit.

---

## Phase 6: User Story 4 - Private custom habits (Priority: P2)

**Goal**: a registered user creates, lists, reads, edits and deletes their own custom habits; nobody else sees
them, and they never appear in a catalog result (FR-020 to FR-025, HP-007, HP-008, HP-015, HP-018).

**Independent Test**: as one user create, list, edit and delete a custom habit; as another user, an
administrator and a visitor, confirm that none of them can list, read, edit or delete it.

### Tests for User Story 4

- [X] T088 [P] [US4] Create `tests/HT.UnitTests/Fakes/FakeCustomHabitStore.cs` (in-memory `ICustomHabitStore`, owner-scoped like the real one, uniqueness per owner on the key, `RefuseNameOnNextWrite()`, `WriteCount`) and `tests/HT.UnitTests/Application/Habits/CustomHabitServiceTests.cs`:
  - US4-AS1 to US4-AS6 (`" cold SHOWER "` taken; another user's same name and a catalog habit's name allowed);
  - SC-006, for each of the five use cases: another registered user (US4-AS7, `NotFound` with "This custom habit does not exist."), an administrator (US4-AS8, `NotForAdministrators` with "Custom habits are for registered users.") and an anonymous visitor (US4-AS9, `NotSignedIn`), each with nothing changed and no result carrying the habit;
  - US4-AS10 and US4-AS11 (FR-016 order); a lost uniqueness race gives `NameTaken`.

  The fake implements T092's port, so T092 is written first; the tests fail until T096 exists. Link the fake into `tests/HT.UiTests/HT.UiTests.csproj` as T064 links the catalog fake, for T106 (depends on T092).
- [X] T089 [P] [US4] Create `tests/HT.IntegrationTests/Habits/CustomHabitStoreTests.cs` (prefix `It-Cu`, accounts with emails of their own):
  - every statement is owner-scoped: user B's find, update and delete of user A's habit give not found and change no row (plain SQL check); a catalog habit's identifier gives not found;
  - `" it-cu cold SHOWER "` is taken for the same owner, free for another owner and free beside a catalog habit of that name;
  - an edit refused because the owner's other habit has the name gives `NameTaken`, an edit of a missing identifier gives `NotFound`, and an edit that keeps every value gives `Saved`;
  - delete is permanent.

  Depends on T094 (the store and its registration).
- [X] T090 [P] [US4] Create `tests/HT.IntegrationTests/Habits/CustomHabitRaceTests.cs` with `LogCapture` (T004) (HP-018, HP-015): two parallel creates with one name for one owner, and two parallel renames to one name, give one success and one `NameTaken`, on habits with a description; then `AssertContainsNone` with the name, its key and the description; no exception was thrown (depends on T094)
- [X] T091 [P] [US4] Create `tests/HT.IntegrationTests/Habits/CrossKindTests.cs` (prefix `It-X`): with a custom habit `"It-X Cold shower"` and a Published `"It-X Cold water"`, browsing and searching `"It-X"` return only the catalog habit (US1-AS6); catalog details with the custom habit's identifier give `NotInCatalog` (US2-AS5); the administrator's list excludes it and read, edit, publish and unpublish with its identifier give `NotFound` (US3-AS13); a custom-habit read with a catalog habit's identifier gives `NotFound` (FR-022). The cases that depend on the whole catalog go to `tests/HT.IntegrationTests/Habits/CrossKindEmptyCatalogTests.cs` (collection `SeedDatabase`, T003, from a reset database): with a Published habit in Body only and a custom habit in Home, the category filter is "All", "Body" (FR-024, the custom-habit side of US1-AS9). Depends on T096 (the custom habits are created through `CustomHabitService`)

### Implementation for User Story 4

- [X] T092 [US4] Create `src/HT.Application/Habits/ICustomHabitStore.cs`, a `public` interface whose members all take the owner `AccountId` first (data-model.md "Ports"), and its outcomes, verb first as in T038: `CreateCustomHabitOutcome` (`NameTaken`, `Created` with the `HabitId`), `UpdateCustomHabitOutcome` (`NotFound`, `NameTaken`, `Saved`) and `DeleteCustomHabitOutcome` (`Deleted`, `NotFound`)
- [X] T093 [US4] Create `src/HT.Persistence/Habits/CustomHabitStore.cs` (internal) per research R-14:
  - reads filter `WHERE OwnerId = @owner AND Id = @id` in the same query, no-tracking;
  - create is one `ExecuteSqlInterpolatedAsync` statement, `INSERT … SELECT … WHERE NOT EXISTS (SELECT 1 FROM CustomHabits WITH (UPDLOCK, HOLDLOCK) WHERE OwnerId = @owner AND NameKey = @key)`, with the `Id` from `Guid.NewGuid()`; no row inserted is `NameTaken`;
  - edit is `UPDATE … WHERE Id = @id AND OwnerId = @owner AND NOT EXISTS (… AND Id <> @id)`; no row changed, then a read of (`OwnerId`, `Id`) tells `NotFound` from `NameTaken`;
  - delete is owner-scoped; no row is `NotFound`;
  - every database exception of a write is replaced by `InvalidOperationException("Saving a custom habit failed (SQL error {number}).")` with no inner exception.

  Depends on T092.
- [X] T094 [US4] Register `ICustomHabitStore` (scoped) in `src/HT.Persistence/PersistenceServiceCollectionExtensions.cs` (depends on T093)
- [X] T095 [US4] Create the view `CustomHabitView` (no owner) and the results `CustomHabitListResult`, `CustomHabitResult`, `CustomHabitCreateResult`, `CustomHabitEditResult` and `CustomHabitDeleteResult` in `src/HT.Application/Habits/`, each with its `<UseCase>Outcome` enum (T038) holding the outcomes of the contract table, and their texts
- [X] T096 [US4] Create `src/HT.Application/Habits/CustomHabitService.cs` with `ListAsync`, `GetAsync`, `CreateAsync`, `EditAsync` and `DeleteAsync`, the acting user first, in FR-016's order (anonymous, then `Role.Administrator`, then the identifier, then FR-005 through `HabitDefinition.TryCreate`, then uniqueness by `NameKeyExistsAsync` and the store's outcome); every store call passes `actingUser.AccountId` as the owner; the list in `HabitName.Order`. Register it (scoped) in `ApplicationServiceCollectionExtensions.cs` together with T094 (depends on T094, T095)
- [X] T097 [US4] Extend `tests/HT.ArchitectureTests/UseCaseParameterTests.cs`: every public method of `CustomHabitService` takes `ActingUser` first and no `AccountId`; every method of `ICustomHabitStore` takes `AccountId` first (research R-12)
- [X] T098 [US4] Extend `tests/HT.ArchitectureTests/UiDependencyTests.cs`: no `HT.Ui` type depends on `ICustomHabitStore`
- [X] T099 [US4] Run `tests/HT.UnitTests/Application/Habits/CustomHabitServiceTests.cs` and `tests/HT.IntegrationTests/Habits/CustomHabitStoreTests.cs`, `CustomHabitRaceTests.cs` and `CrossKindTests.cs` (T088–T091) against T092–T098, and fix the code, never the tests, until they pass
- [X] T100 [P] [US4] Create `src/HT.Ui/Components/Shared/HtDialog.razor` per `design/components/Dialog.md`: the `Title`, `Subtitle`, `ChildContent` and `Footer` render fragments; `aria-labelledby` pointing to a title `id` generated per instance, never a fixed string; focus moves into it on open, Esc and × close it, and focus returns to the opening control. Its bUnit tests go in `tests/HT.UiTests/Components/Shared/HtDialogTests.cs`, including two dialogs on one page with different title ids
- [X] T101 [US4] Create `src/HT.Ui/Components/Habits/CustomHabitForm.razor` per `design/pages/PageCustomHabit` (T011): Name, Description (TextArea), Category, "Usually you want to" Build or Reduce (the SegmentedControl of T082), the icon picker, "Save habit", Cancel and the privacy card; one refusal at a time under its field (depends on T082)
- [X] T102 [US4] Create `src/HT.Ui/Components/Pages/CustomHabit.razor` at `/custom-habits/new` and `/custom-habits/{id:guid}`, `[Authorize]`, Interactive Server with prerendering, over `CustomHabitForm` (HP-007, US4-AS13, US4-AS15):
  - BackLink "← My custom habits";
  - editing adds "Delete", which opens the `HtDialog` first;
  - after Save or Delete, back to `/custom-habits`;
  - `NotFound` from Save or Delete shows "This custom habit does not exist." as a Notice above the form with a link to "My custom habits", and the form is no longer saved;
  - opening another user's, a deleted or an unknown habit, and an administrator on either address, call `NavigationManager.NotFound()`;
  - every call through `UseCaseScope`.

  Depends on T096, T100, T101.
- [X] T103 [US4] Create `src/HT.Ui/Components/Habits/CatalogTabs.razor` (the link Tabs "Public catalog" → `/catalog` and "My custom habits" → `/custom-habits`, each with the count T009 defines) and `src/HT.Ui/Components/Pages/CustomHabits.razor` at `/custom-habits`, `[Authorize]`, Interactive Server with prerendering: the Catalog page head, the Tabs, "+ Create custom habit" (→ `/custom-habits/new`), the user's custom habits as HabitCards with the "Private" Tag and an edit action, or an EmptyState; an administrator gets `NavigationManager.NotFound()` (US4-AS12, US4-AS15) (depends on T096)
- [X] T104 [US4] Extend `src/HT.Ui/Components/Pages/Catalog.razor` for registered users: `CatalogTabs` and "+ Create custom habit"; administrators keep the Public catalog without Tabs, without the button and without the Notice (HP-002) (depends on T103)
- [X] T105 [US4] Extend `src/HT.Ui/Components/Pages/Profile.razor` (static) for registered users: the link Tabs "Account" and "Custom habits" (`?tab=custom-habits`); Account keeps its slice-1 content and the change-password form; the Custom habits tab lists the user's custom habits with links to create and edit. An administrator's Profile stays Account only, without Tabs (HP-008, US4-AS14) (depends on T096)
- [X] T106 [P] [US4] Create `tests/HT.UiTests/Components/Pages/CustomHabitPageTests.cs` and `tests/HT.UiTests/Components/Pages/CustomHabitsPageTests.cs` (bUnit): the form structure and the absent slice-4 and slice-10 actions; a refusal under its field; the Dialog's focus handling around Delete; the "does not exist" Notice and the disabled form; the return to `/custom-habits`; the list with "Private" Tags and its EmptyState; the counts on the Tabs. Extend `tests/HT.UiTests/Components/Pages/CatalogPageTests.cs`: Tabs and the button for a registered user, neither for an administrator (depends on T102–T104)
- [X] T107 [P] [US4] Create `tests/HT.UiTests/Host/CustomHabitHostTests.cs` (US4-AS15): `/custom-habits`, `/custom-habits/new` and `/custom-habits/{id}` send an anonymous visitor to Sign in and give an administrator 404; another user's, a deleted and an unknown identifier give the owner 404; the owner's own habit gives 200. Update `tests/HT.UiTests/Host/ProfilePageTests.cs` (slice 1 HP-014, amended by HP-008): the Tabs for a registered user, `?tab=custom-habits` shows the list, the administrator's Profile is unchanged, and the change-password form still works (depends on T102, T105)

**Checkpoint**: full suite green, including HP-015's log capture. Commit.

---

## Phase 7: User Story 5 - Catalog in the navigation and on Home (Priority: P3)

**Goal**: "Catalog" and "Admin" in the navigation; the visitor Home's "Browse the catalog" and "Popular in the
catalog"; a registered user's Home points to the catalog (FR-008's Home preview, HP-011 to HP-013).

**Independent Test**: as a visitor, a registered user and an administrator, follow the navigation and the Home
links to the catalog and to Admin.

### Tests for User Story 5

- [X] T108 [P] [US5] Create `tests/HT.UnitTests/Application/Habits/HomePreviewTests.cs`: with 0, 3, 4 and 5 Published habits (and Drafts and Unpublished ones beside them), `Habits` is the first four of browsing with no filters, fewer when fewer exist, and `PublishedCount` counts every Published habit. Create `tests/HT.IntegrationTests/Habits/HomePreviewTests.cs` (collection `SeedDatabase`, from a reset database): with a custom habit `"A0 Custom"` and five Published habits whose names sort after it, the preview holds four catalog habits and no custom habit, and `PublishedCount` is 5 (FR-024, SC-005). The integration test needs US4's `CustomHabitService` (T096)

### Implementation for User Story 5

- [X] T109 [US5] Add `GetHomePreviewAsync(CancellationToken)` and its result `HomePreviewResult` (`Found`, `Habits`, `PublishedCount`) to `src/HT.Application/Habits/CatalogService.cs` (research R-07)
- [X] T110 [US5] Add "Catalog" (→ `/catalog`) for every audience and "Admin" (→ `/admin`) for administrators to `src/HT.Ui/Components/Layout/SiteNav.razor`, in the order and markup of TopNav and MobileNav after T015 (HP-011). Update `tests/HT.UiTests/Components/Layout/SiteNavTests.cs` (slice 1 HP-010, amended): the links per audience, no "Goals", "Community" or "+ New goal"
- [X] T111 [US5] Extend `src/HT.Ui/Components/Pages/Home.razor` (static) per T014:
  - visitor: the hero's secondary button "Browse the catalog" (→ `/catalog`); after "How it works", "Popular in the catalog" with `GetHomePreviewAsync`'s habits as visitor HabitCards and "See all N →" (→ `/catalog`) with N = `PublishedCount`; no section when `Habits` is empty (HP-012);
  - registered user: under "Today", an EmptyState with "Browse the catalog" (HP-013); administrator unchanged.

  Depends on T109.
- [X] T112 [P] [US5] Update `tests/HT.UiTests/Components/Pages/HomeVisitorTests.cs` (slice 1 HP-011, amended): "Browse the catalog"; with 5 Published habits in T064's fake, four cards and "See all 5 →"; with none, no "Popular in the catalog". Update `tests/HT.UiTests/Components/Pages/HomeSignedInTests.cs` (slice 1 HP-013, amended): the registered user's EmptyState, the administrator's "Today" only (depends on T111)
- [X] T113 [P] [US5] Create `tests/HT.UiTests/Host/HomeCatalogHostTests.cs`: with at least four Published habits arranged (prefix `Ui-Ho`), the visitor Home shows four cards and "See all N →" with N equal to `GetHomePreviewAsync`'s `PublishedCount` from the host's services (depends on T111)

**Checkpoint**: full suite green; the catalog is reachable from every page. Commit.

---

## Phase 8: User Story 6 - Seed catalog and demo custom habits (Priority: P3)

**Goal**: demo loading creates the seed catalog in an empty catalog and the demo custom habits of each demo
account it creates, each as one transaction, and the host logs the results (FR-030, FR-031, HP-017,
ADR 0013).

**Independent Test**: run demo loading against an empty database and check the seed catalog and the demo
custom habits; run it again and check that nothing changed.

### Tests for User Story 6

- [X] T114 [P] [US6] Create `tests/HT.UnitTests/Application/Operator/SeedCatalogTests.cs` (research R-10): every property of FR-030 on `SeedCatalog.Habits` (at least 20 Published covering all six categories, at least 4 of them Reduce, a description on each, 22 different icons, unique keys, all three measurements and both schedule kinds among the suggested starting goals, at least one Published habit with none and one with three or more, exactly one Draft and one Unpublished), and every habit passing the Domain checks. Also test `DemoCustomHabits.For` for the three demo emails (FR-031 table) (depends on T117, T118)
- [X] T115 [P] [US6] Create `tests/HT.UnitTests/Fakes/FakeDemoDataStore.cs`, which wraps the test's `FakeAccountStore` (creates the account there, honours `TakeEmailOnNextCreate` as the lost race, records the custom habits per account, and records the seed catalog once when it has no habit), and rewrite `tests/HT.UnitTests/Application/Operator/DemoLoadingTests.cs` over it (research R-13; the fake implements T119's port, so T119 comes first): keep every slice-1 case; add the catalog result ("seed catalog loaded", "catalog not empty", null for the two refusals), the custom-habit result per created account ("demo custom habits added", null for "already exists"), the order of work, and that no result text contains a custom-habit name. The new cases fail until T120 exists
- [X] T116 [P] [US6] Create `tests/HT.IntegrationTests/Operator/SeedCatalogLoadingTests.cs` (collection `SeedDatabase`, T003, each test from a reset database):
  - US6-AS1: "seed catalog loaded"; browsing meets FR-030; the administrator's list has exactly one Draft and one Unpublished seed habit;
  - US6-AS2 and SC-007: a second run gives "catalog not empty", and the rows of `CatalogHabits`, `CatalogHabitSuggestedStartingGoals` and `CustomHabits` equal those after the first run (plain SQL);
  - US6-AS3: with `"Ac29 Rowing"` created first, "catalog not empty" and only that habit;
  - US6-AS4: no demo password creates no account, no catalog habit and no custom habit;
  - US6-AS5 to US6-AS8 through `AccountService.SignInAsync` and `CustomHabitService.ListAsync`, and none of the demo custom habits in browsing or the administrator's list;
  - rollback: `CreateDemoAccountAsync` with two custom habits of one name creates no account and no habit, and `TryCreateSeedCatalogAsync` with two habits of one name returns false and creates nothing; the first runs with `LogCapture` and `AssertContainsNone` with that name.

  Needs T119's types; passes once T121 exists.

### Implementation for User Story 6

- [X] T117 [US6] Create `src/HT.Application/Operator/SeedCatalogHabit.cs` and `src/HT.Application/Operator/SeedCatalog.cs`: the fixed list of 22 habits (20 Published, one Draft, one Unpublished; one icon each) written by Claude in the tone of `design/brand-book.md`, with neutral descriptions and suggested starting goals that make no health, medical or coaching claim and read as examples (HP-014). The owner reviews the content in the PR (Clarifications)
- [X] T118 [P] [US6] Create `src/HT.Application/Operator/DemoCustomHabits.cs`: `For(string demoEmail)` returns the two `HabitDefinition`s of FR-031 for each demo email, with no description
- [X] T119 [US6] Create `src/HT.Application/Operator/IDemoDataStore.cs` (public, `CreateDemoAccountAsync(Email, string password, IReadOnlyList<HabitDefinition> customHabits)` returning `CreateDemoAccountOutcome` (`Created`, or `EmailTaken`, which the service reports as "already exists" just as slice 1 reports the account store's lost race), and `TryCreateSeedCatalogAsync(IReadOnlyList<SeedCatalogHabit>)` returning `bool`, research R-09), `DemoCatalogResult` and `DemoCustomHabitsResult`; add the three texts to `src/HT.Application/Operator/OperatorTexts.cs`; add `CustomHabits` to `DemoAccountResult` and `Catalog` to `DemoLoadingResult` (contract, "Demo loading"), losing no slice-1 member
- [X] T120 [US6] Extend `src/HT.Application/Operator/DemoDataService.cs`: the public constructor takes `IAccountStore` and `IDemoDataStore`; `LoadAsync` keeps the password checks, then for each demo email an existing account is "already exists" with no custom-habit result and no further call, otherwise `CreateDemoAccountAsync` with `DemoCustomHabits.For(email)`; then, after the account step, `TryCreateSeedCatalogAsync(SeedCatalog.Habits)` (depends on T117–T119)
- [X] T121 [US6] Create `src/HT.Persistence/Habits/DemoDataStore.cs` (internal) per research R-09:
  - `CreateDemoAccountAsync`: one transaction on the scoped `HtDbContext` that Identity's user manager also uses; the registered-user account with the password; each custom habit with T093's conditional insert; an insert of no row is a fault that rolls everything back and names no habit; an email that already has an account gives "email taken" with nothing created;
  - `TryCreateSeedCatalogAsync`: a serializable transaction that creates every seed habit with its state only when `CatalogHabits` is empty, otherwise returns false; a unique-index violation (2601, 2627) or a deadlock (1205) rolls back and returns false.

  Register `IDemoDataStore` (scoped) in `PersistenceServiceCollectionExtensions.cs` in the same commit as T120 (depends on T119, T093)
- [X] T122 [US6] Extend `tests/HT.ArchitectureTests/ApplicationDependencyTests.cs` (inside `HT.Application`, only types in `HT.Application.Operator` depend on `IDemoDataStore`) and `tests/HT.ArchitectureTests/UiDependencyTests.cs` (no `HT.Ui` type depends on `IDemoDataStore`) (research R-12)
- [X] T123 [US6] Build and check that slice 1's `tests/HT.IntegrationTests/Operator/DemoLoadingTests.cs` still passes unchanged on `HT-IT` (it now also seeds that database; research R-13, "Starting states") and that every other `HT-IT` test still passes with the seed present (depends on T120, T121)
- [X] T124 [US6] Extend `src/HT.Ui/Operator/StartupTasks.cs` with two source-generated log methods (contracts/host.md, "Operator log"): `Demo custom habits {Email}: {Result}` right after that email's `Demo account {Email}: {Result}` line, only for accounts with a custom-habit result, and `Demo catalog: {Result}` once after the account lines; nothing new for `NoPassword` and `PasswordRuleNotMet` (HP-017) (depends on T120)
- [X] T125 [US6] Update `tests/HT.UiTests/Operator/StartupTasksDemoTests.cs` (slice 1 HP-021, extended): the lines starting with "Demo " after the administrator line gain the custom-habit lines and the catalog line in that order, and no line contains a demo custom-habit name. Re-run `tests/HT.UiTests/Host/StartupLogTests.cs` unchanged (depends on T124)

**Checkpoint**: full suite green; a start against an empty database shows the seed catalog and the demo
custom habits. Commit.

---

## Phase 9: Polish and cross-cutting concerns

- [ ] T126 Review the accessibility and wording of every new and changed page and component (SC-008, HP-014, HP-016): a visible focus ring from tokens; text contrast of at least 4.5:1 from the token pairs; labelled icon-only buttons; the Dialog's focus; Draft, Published, Unpublished and Private always as text; brand-book wording. Search `src/HT.Ui` for hard-coded colours or sizes outside `wwwroot/css/` and remove them
- [ ] T127 [P] Check HP-015 across the code: no log template, exception message or error page in `src/` names a custom-habit field; `StartupTasks`' fault log carries only the store's own exception texts
- [ ] T128 [P] Update `README.md`'s status line for slice 2. Update `CONTEXT.md` only if implementation sharpened a term (plan, Work order 7)
- [ ] T129 Run `dotnet build HabitTracker.slnx` and `dotnet test HabitTracker.slnx`. Every project must pass: unit, architecture, integration and UI. Record the counts for the report (constitution V)
- [ ] T130 Walk through [quickstart.md](quickstart.md) section 3 with `dotnet run --project src/HT.Ui --launch-profile https`. Ask the owner before dropping the local `HT` database (quickstart, Prerequisites); check the startup log lines and each row of the browser table; note anything left unverified
- [ ] T131 Leave `docs/roadmap.md` at `implementing`; the owner's `/speckit-ac-tests-write` and `/speckit-ac-tests-analyze` move it to `AC tests green`, and opening the PR to `shipped` (roadmap, "Updating this file")

---

## Dependencies and execution order

### Phase dependencies

- **Setup (Phase 1)**: no dependencies. T002 needs the owner's yes before the download.
- **Foundational (Phase 2)**: depends on Setup and blocks every story. Within it, the plan's Work order:
  - design (T005–T018) first; T005 and T048 need T002;
  - Domain (T019–T035);
  - Application texts, inputs and port (T036–T039);
  - Persistence (T040–T046);
  - host and shared components (T047–T057).
- **Stories (Phases 3–8)**: all depend on Phase 2. In priority order: US1, US2, US3 (P1, the MVP), then US4
  (P2), then US5 and US6 (P3).
- **Polish (Phase 9)**: after the stories that are to ship.

### Story dependencies

- **US1**: none. Its integration and host tests arrange catalog habits through `ICatalogHabitStore`.
- **US2**: after US1. T070 extends US1's `CatalogViews`, T071 adds to US1's `CatalogService.cs`, T073 uses
  T064's linked fake, and its cards link from US1's page.
- **US3**: after US1. Its code needs no US1 code, but its tests do: T076 and T078 browse through US1's
  `BrowseAsync`, and T086 uses the fake that T064 links into `HT.UiTests`. T087 covers US5-AS4 for the
  Admin addresses.
- **US4**: after US3. T101 uses US3's SegmentedControl (T082); T091 calls US1's, US2's and US3's use cases;
  T104 changes US1's page.
- **US5**: after US1 and US4. T109 adds to US1's `CatalogService.cs`, T112 uses T064's linked fake, T108's
  integration test creates a custom habit through US4's `CustomHabitService`, and T110's "Admin" link needs
  US3's page in the running app.
- **US6**: after US4 (T121 reuses T093's conditional insert; T116 lists custom habits through
  `CustomHabitService`) and US3 (T116 reads the administrator's list).
- **Shared files** need sequential edits, so their tasks never run in parallel with each other:
  - `CatalogService.cs`: T063, T071, T109;
  - `ApplicationServiceCollectionExtensions.cs`: T063, T081, T096;
  - `PersistenceServiceCollectionExtensions.cs`: T044, T094, T121;
  - `UseCaseParameterTests.cs`: T060, T077, T097;
  - `UiDependencyTests.cs`: T046, T098, T122;
  - `Catalog.razor`: T065, T104; `CatalogPageTests.cs`: T066, T106;
  - `Profile.razor` and `ProfilePageTests.cs`: T105, T107;
  - `Home.razor`: T111.

### Within each story

- Tests marked [P] are written first, after the port or the types they compile against (the dependencies
  each names), and fail before the use case exists. Then result types, use case, store and registration
  together, components, page, host tests.
- The story's checkpoint runs the full suite before the next story, and is the commit point.

### Parallel opportunities

- Phase 1: T002–T004.
- Design: T005–T015, then T016, T017, T018.
- Domain: T019–T021 and T026, then T023; T028–T035 once their types exist.
- Application and Persistence: T036, T037 and T040; then T039 after T038, and T045 after T044.
- Components: T050–T055, then T057.
- In each story, every task marked [P].
- After Phase 2, US3's code can be built beside US1, but its tests wait for US1; US2 follows US1. US4
  follows US3; US5 and US6 both follow US4 and can run side by side, apart from the shared files above.

---

## Parallel example: User Story 1

```text
Task: "T058 [US1] Browse unit tests in tests/HT.UnitTests/Application/Habits/CatalogBrowseTests.cs"
Task: "T059 [US1] Browse integration tests in tests/HT.IntegrationTests/Habits/CatalogBrowseTests.cs"
Task: "T060 [US1] CatalogService parameter rule in tests/HT.ArchitectureTests/UseCaseParameterTests.cs"

# after T065:
Task: "T066 [US1] Catalog page bUnit tests in tests/HT.UiTests/Components/Pages/CatalogPageTests.cs"
Task: "T067 [US1] Catalog host tests in tests/HT.UiTests/Host/CatalogHostTests.cs"
```

## Parallel example: User Story 3

```text
Task: "T075 [US3] Access tests in tests/HT.UnitTests/Application/Habits/CatalogAdministrationAccessTests.cs"
Task: "T076 [US3] Field tests in tests/HT.UnitTests/Application/Habits/CatalogHabitFieldTests.cs"
Task: "T078 [US3] Integration tests in tests/HT.IntegrationTests/Habits/CatalogAdministrationTests.cs"
Task: "T082 [US3] DataTable, SegmentedControl, Stepper, DayPicker in src/HT.Ui/Components/Shared/"
```

## Parallel example: User Story 4

```text
# after T094 (T091 after T096):
Task: "T089 [US4] Store integration tests in tests/HT.IntegrationTests/Habits/CustomHabitStoreTests.cs"
Task: "T090 [US4] Race and log tests in tests/HT.IntegrationTests/Habits/CustomHabitRaceTests.cs"
Task: "T091 [US4] Cross-kind tests in tests/HT.IntegrationTests/Habits/CrossKindTests.cs"
Task: "T100 [US4] HtDialog in src/HT.Ui/Components/Shared/HtDialog.razor"
```

---

## Implementation strategy

### MVP first (User Stories 1–3)

1. Phase 1 and Phase 2 (design first, then Domain, the migration and the catalog store).
2. US1: browsing works on `/catalog` with habits arranged through the store.
3. US2: details and the Not found page.
4. US3: administrators fill the catalog themselves; the idea brief's scenario 3 is demonstrable.
5. **Stop and validate**: full suite, then a browser check of browse, details and Admin.

### Incremental delivery

1. US4: private custom habits, the first private data (constitution III), with HP-015's log checks.
2. US5: navigation and Home make the catalog reachable without typing addresses.
3. US6: the seed catalog and demo custom habits make the demo full at the first start (SC-002).
4. Polish, then the owner runs the AC workflow.

One PR for the slice; each checkpoint is a commit on the feature branch.

---

## Notes

- [P] tasks touch different files and start once the tasks they name as dependencies are done (Format).
- The AC project, its features, the locks and the run records are never edited; the owner starts
  `/speckit-ac-tests-write` after `/speckit-implement`.
- The plan's positions ("Spec discrepancies" 1–5) and ADR 0014 are accepted; the tasks implement them as stated.
- FR-015 has nothing to build here beyond an unpublish that acts at once with no confirmation (T081, T084);
  slice 4 adds the goal count and the confirmation.
- Two page details the spec leaves to the design step are settled there, not in code: where the admin form's
  Save leads (T012 proposes the Admin table) and what each Catalog tab's count counts (T009).
- Stop at any checkpoint to validate a story on its own.
