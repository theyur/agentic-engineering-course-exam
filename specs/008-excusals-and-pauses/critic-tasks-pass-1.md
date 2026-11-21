# Critic pass 1: tasks for Excusals and pauses

**Verdict**: REVISE
**Date**: 2026-11-21
**Reviewed**: specs/008-excusals-and-pauses/tasks.md

## Notes

### MAJOR-1: Nothing fixes `RecordingText.State` or the NoteDialog for Excused and Paused rows, so their note action throws
- **Where**: tasks.md T016 (line 214), T035 (line 257), T049 (line 304), T064 (lines 346-350).
- **Against**: spec HP-001 ("both rows keep their note actions"), spec Edge Cases "Notes" (an Excused or Paused entry can still get a note), contracts/host.md "History card" ("both rows keep their note actions"). Code: `src/HT.Application/Records/RecordingText.cs:30-40`, `src/HT.Ui/Components/Records/NoteDialog.razor:94-95`, `src/HT.Ui/Components/Records/HistoryCard.razor:133`.
- **Reasoning**:
  - `RecordingText.State` ends in `_ => throw new ArgumentOutOfRangeException(...)`.
  - `NoteDialog.SubLine` calls `RecordingText.State(Row.State, ...)` directly, not through `ReportText.State`.
  - The NoteDialog opens only from a History row without a record (`HistoryCard.razor:133`). An Excused row never has a record (D-3), and a Paused row often has none. So "Add note" on either row reaches `RecordingText.State(Excused|Paused)` and throws.
  - T016 asks for arms only in switches "that has no discard arm". `RecordingText.State`, `StatusChip.Modifier` and `ConsistencyStrip.CellClass` all have a throwing discard arm, so the rule leaves them out, yet the same task lists them as sites. That contradiction leaves the reader unsure what to change.
  - T042 later fixes `StatusChip` and `ConsistencyStrip`. T035 changes only `ReportText.State`.
  - No task names `NoteDialog` or changes `RecordingText.State`. T049 and T064 test only that the note action is *present* on Excused and Paused rows, never that it opens. The crash would pass the suite and break HP-001 at runtime.

### MAJOR-2: Phase 2 says existing tests stay unchanged, but the new store members break existing test code
- **Where**: tasks.md T028/T029 (lines 241-242, "Existing fakes' members do not change"), T034 (line 251, exemptions "read once per call" in every read and write), T036 (line 259), Checkpoint (shared) (line 275: "the existing tests of slices 5 and 6 unchanged apart from the absence assertions of T042"), Conventions commit list (lines 144-146), T068 (line 372).
- **Against**: constitution V (the build and full suite must pass at each checkpoint commit). Code: `tests/HT.IntegrationTests/Records/RecordLogTests.cs:586` (`private sealed class FaultingRecordStore(IRecordStore inner, FaultSwitch faults) : IRecordStore`); exact `FakeRecordStore.Calls` assertions at `tests/HT.UnitTests/Application/Records/HistoryTests.cs:254` and `:323`, `RecentNotesTests.cs:174`, `RecordableTests.cs:226`, `StateTests.cs:88-89` (all calls `ReadAsync`, exactly 7), `CorrectionTests.cs:426` (`[FindRecordDateAsync, DeleteAsync]`), `tests/HT.IntegrationTests/Records/RecordReadTests.cs:189` and `tests/HT.IntegrationTests/Reports/GoalReportReadTests.cs:238` (`[ReadAsync]`).
- **Reasoning**:
  - `FaultingRecordStore` is a second implementer of `IRecordStore` outside `HT.Persistence` and `FakeRecordStore`. Once T028 adds six members, `HT.IntegrationTests` no longer compiles until that class implements them. The commit group T028 to T031 and every task leave it out.
  - `FakeRecordStore.Calls` is documented as "the names of the members called … in call order". T034 and T036 add a `ReadExemptionsAsync` call to every history, recordable, recent-notes, write and report call.
  - So the nine exact-sequence assertions above fail, unless the fake hides `ReadExemptionsAsync` from `Calls`. T029 does not say which of the two it does.
  - Either way the checkpoint's "unchanged" condition cannot hold. The task list gives no instruction for these files, and some of them pin slice 5's and slice 6's "one read per call" rule, which this slice deliberately changes.

### MAJOR-3: The closed list of allowed additions leaves out the `RecordStoreWrite` factory that `Exempt` needs, so T031 must stop and T097 would fail
- **Where**: tasks.md Conventions "Locked AC code" (lines 87-94: "Only these are added to existing types … If a task seems to need more, stop and ask the owner"), T028 (line 241), T029, T031 (line 244), T097 (line 465: "only those listed in data-model.md and the Conventions; a changed or removed member is a failure").
- **Against**: data-model.md "Application: changed types" (only "`RecordStoreWriteOutcome` + `Exempt`"). Code: `src/HT.Application/Records/RecordStoreWrite.cs`, a sealed class with a private constructor and one public static member per outcome (`GoalNotFound`, `RecordNotFound`, `LimitReached`, `Written(...)`). ADR 0010 / `AcSurfaceTests` (no `InternalsVisibleTo`).
- **Reasoning**:
  - `RecordStore` is in `HT.Persistence`, and `FakeRecordStore` is in `HT.UnitTests`. Neither can build a `RecordStoreWrite` whose `Outcome` is `Exempt` without a new public member on `RecordStoreWrite`, for example a static `Exempt`.
  - That member is on neither the Conventions list nor data-model.md. A literal reader of T029/T031 must therefore stop and ask in the middle of the Phase 2 critical path, and T097's API diff would report the member as a failure.
  - The same gap applies to the outcome enum that `DemoPauseLoadingResult` needs "as slice 6's" (`DemoReportLoadingOutcome` exists, and `StartupTasks.cs:65` switches on it). data-model.md does not list it, and T097 allows only listed additions.

### MAJOR-4: T036 drops the current week's Paused Upcoming commitments, which T019 and FR-016 need for the all-exempt mark
- **Where**: tasks.md T036 first bullet (line 259): "the trend goes through T019's overload, with the exemption of each begun entry and of the current week's Upcoming commitments; `NextUpcoming` and the current week's Upcoming commitments leave out Paused entries"; against T019 (lines 219-224).
- **Against**: spec FR-016 ("A week whose commitments are all Excused or Paused is marked Paused when at least one of them is Paused, and Excused otherwise"); research R-07 (all exempt "the current week's Upcoming ones included … A week with an Upcoming commitment that is not Paused is not all exempt"). Code: `src/HT.Application/Reports/GoalEvaluation.cs:135` and `:186` (`CurrentWeekUpcoming` feeds only `TrendBuilder.Of`).
- **Reasoning**:
  - `GoalEvaluation.CurrentWeekUpcoming` has one consumer: the trend.
  - If T036's "leave out Paused entries" is applied to that list, the overload never sees Paused Upcoming commitments. Example: a current week whose begun Monday is Excused and whose upcoming Wednesday and Friday are covered by a pause. That week comes out all exempt and marked `Excused`, while FR-016 requires `Paused`.
  - T036 also tells the implementer to pass the exemption of those same Upcoming commitments. The bullet contradicts itself.
  - T026 tests the builder directly. T067's "an all-exempt week marked" does not say current week. So the wrong reading would not be caught.

### MINOR-1: T100 sets a roadmap status that does not exist
- **Where**: tasks.md T100 (line 468): "Set slice 7's Status … to `implemented` … as 'Updating this file' gives it".
- **Against**: docs/roadmap.md "Status values" (lines 46-48: … `tasks ready` · `implementing` · `AC tests green` · `shipped`) and "Updating this file". specs/007-personal-reports/tasks.md T088 kept the row at `implementing`.
- **Reasoning**: `implemented` is not a defined status. "Updating this file" gives no such step. The previous slice deliberately left the row at `implementing` until `AC tests green`.

### MINOR-2: Some existing UI, host and smoke-check tests that this slice breaks are not acknowledged, and one task cites assertions that do not exist
- **Where**: tasks.md T051, T064, T075, T079, T093, T069.
- **Against**: spec "Earlier requirements and tests this slice changes". Code:
  - `tests/HT.UiTests/Components/Pages/GoalDetailPageTests.cs:120` (`Assert.Equal(["Record"], …page-head buttons)`);
  - `GoalsPageTests.cs:107` ("Paused" in the absent list);
  - `tests/HT.UiTests/Host/GoalsHostTests.cs:347` (the slice-6 footnote as exact HTML);
  - `tests/HT.UiTests/Components/Shared/CommitmentRowTests.cs:83` and `:141` (`FindAll("button").SingleOrDefault()` on a Pending commitment row with no record);
  - `tests/HT.UiTests/Components/Reports/TodayPanelTests.cs:135` (exact row button list);
  - `tests/HT.UiTests/SmokeChecks/CoverageTests.cs:96` (`Assert.Equal(10, … e.Port != typeof(IDemoReportStore))`);
  - `TrendChartTests.cs` (no paused or excused assertion at all).
- **Reasoning**:
  - **Goal page head**: T064 changes only the absent list of the goal page test. The head-button assertion turns into `["Pause", "Record"]`.
  - **Goals page and host**: T075 replaces only the footnote assertion and not the "Paused" absence. T079 does not mention the exact footnote in `GoalsHostTests`.
  - **Home rows**: T051 only adds tests, but the existing single-button and exact-list assertions fail once "Excuse" appears on Pending commitment rows.
  - **Smoke-check coverage**: T093 names only the 14→18 total. The "not demo report" count of 10 becomes 14.
  - **Trend chart**: T069 asks to convert "the slice-6 assertions that no paused bar exists", but `TrendChartTests` has none. Only `ConsistencyStripTests.cs:70-72` asserts absence.
  - These are found on the first failing run, but the tasks list some absence edits precisely (T042, T050, T052) and miss these.

### MINOR-3: The UI-layer fences for the new operator service and port are missing
- **Where**: tasks.md T088 (line 443), which names only `ApplicationDependencyTests.cs` and `PersistenceDependencyTests.cs`.
- **Against**: constitution III and V (privacy enforced below the UI; architecture rules for new production types). Code: `tests/HT.ArchitectureTests/UiDependencyTests.cs:104` (`NoUiTypeMayDependOnTheDemoReportStorePort`) and `:113-117` (`OnlyTheHostsOperatorTypesMayDependOnTheOperatorServices`, listing `DemoReportService`).
- **Reasoning**: every earlier demo port and operator service has a matching UI rule. T088 adds neither the `IDemoPauseStore` rule nor `DemoPauseService` in the operator-services list. So a Blazor component could call the private-data-writing port or service without a failing test. T088's own grep would show `UiDependencyTests.cs`, but its instruction is scoped to Persistence.

### MINOR-4: The fake has no way to simulate a concurrent write, which T038 needs, and the store-race refusals of the services are untested
- **Where**: tasks.md T029 (line 242), T038 (line 269: "a store `Exempt` (the fake set to refuse after the use case's checks)"), T044 (line 299), T055 (line 338), T046/T059.
- **Against**: contracts/application-api.md (`ExcuseAsync`: store `AlreadyExcused`, `HasRecord` or `Paused` → steps 7, 9, 6; `PauseAsync`: store `Overlaps` → step 7; `RecordAsync`: store `Exempt` → matching step). Code: `tests/HT.UnitTests/Fakes/FakeRecordStore.cs` has no hook to change state between a use case's read and its write.
- **Reasoning**:
  - T029 specifies seeding and re-checks but no injection point, yet T038 depends on one.
  - The integration race tests cover only `HasRecord`, `Exempt` and overlapping pauses.
  - The excusal store refusals `AlreadyExcused` and `Paused` have no test at any level. T096's Stryker run will flag their mapping branches, but the means to test them is not specified.

### MINOR-5: Some spec edge cases and locked criteria have no solution-level test task
- **Where**: tasks.md T054, T056, T057 (US2 tests); no task names the cases below.
- **Against**:
  - spec Edge Cases "Time zone changes" (excusals and pauses stay on their dates; "a moved commitment of the same date keeps its excusal");
  - spec Edge Cases "Long pauses" (acceptance.md AC-088);
  - acceptance.md AC-086 (a not-started goal's pauses are current or past by the account's today);
  - acceptance.md AC-087 (resuming a not-started goal from its current pause);
  - constitution II (both sides of each boundary).
- **Reasoning**:
  - T056 resumes only started goals, and T057 lists pauses only of started goals. The not-started path of R-03 for resume and list is covered only in T054 (pause) and T073 (overview).
  - No task changes a goal's time zone with an excusal or pause in place.
  - No task pauses for months.
  - The AC workflow tests these at the use-case level later. But the tasks' own unit and integration coverage, which constitution V and II require at each checkpoint, does not include them.

### MINOR-6: Small inconsistencies in ordering, parallelism and wording
- **Where**: tasks.md Conventions commit list (line 149, "US6, T087 to T090") against T088 (line 443, "In the same commit as T085 and T087"); "Parallel opportunities" (line 508, "T037 to T041 (five test files)") against T038/T039 (lines 269-270, built on T037's `ExemptionWorld` but listed only "after T034"); T090 (line 448, "the seed and the first four demo tasks").
- **Against**: internal consistency; spec US6-AS1 ("demo loading, demo history loading and demo report loading run"); contracts/host.md "Operator log" (administrator creation is not a demo task).
- **Reasoning**:
  - **Commit group**: the Conventions group starts at T087 yet names `DemoPauseService` and `IDemoPauseStore`, which T085 creates, and it includes the test tasks T089/T090. T088 names a different group.
  - **Parallelism**: T038 and T039 cannot run in parallel with T037, because they use its world.
  - **Wording**: only three demo tasks run before demo pause loading. "First four" is either wrong or counts administrator creation as a demo task.

## Summary
4 MAJOR, 6 MINOR. The task list is detailed and mostly traceable to the spec, plan and contracts. Its file paths, test names, the 14→18 coverage count, the CSS classes and the existing helpers all check out against the code. But four problems need fixing before `/speckit-implement`:
- Excused and Paused rows would crash the NoteDialog, because `RecordingText.State` is never extended.
- The Phase 2 checkpoint forbids changes to existing tests that the new store members necessarily break: a second `IRecordStore` implementer, and exact store-call assertions.
- The closed list of allowed public additions blocks the `RecordStoreWrite` factory that `Exempt` needs, and T097 would fail on it.
- T036 contradicts T019 and FR-016 on whether the current week's Paused Upcoming commitments reach the trend.

The MINOR notes are acknowledgements of tests that will break, missing UI-layer fences, a test hook, and wording and ordering clean-up.
