# Critic pass 2: tasks for Excusals and pauses

**Verdict**: REVISE
**Date**: 2026-11-21
**Reviewed**: specs/008-excusals-and-pauses/tasks.md

## Previous notes: resolution status
| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 (RecordingText.State / NoteDialog throw on Excused and Paused) | Resolved | T016 (tasks.md:219-224) gives `RecordingText.State` the final arms "Excused" and "Paused", keeps the Upcoming throw, and names `NoteDialog.razor:94-95` as a caller. T049 (:316) and T064 (:362) now test that "Add note" *opens* the NoteDialog with the sub lines "Wed 11 Nov · Excused" and "… · Paused". That matches `RecordingText.EntryDate` + `" · "` + `State` (`NoteDialog.razor:95`; the existing `NoteDialogTests.cs:126` format is "Mon 16 Nov · Missed"). The three kinds of switch no longer contradict each other. |
| MAJOR-2 (Phase 2 "unchanged" against tests the new store members break) | Partly | Resolved: `FaultingRecordStore` (`RecordLogTests.cs:586`, the only other `IRecordStore` implementer besides `FakeRecordStore` and `RecordStore`) is in T028 and in the commit group (:150-151). T029 records the new members in `Calls`. T034 (:266) and T036 (:274) name the exact call lists that change, and the Checkpoint (shared) (:285-287) permits those edits. Not resolved: T034's file list still leaves out `RecordTests.cs:336`, and it does not say where the exemption read goes relative to the slice-5 "no store read" tests. Carried forward as MINOR-1. The new legend break in MAJOR-1 is a different cause of the same checkpoint failure. |
| MAJOR-3 (`RecordStoreWrite.Exempt` and the demo outcome enum outside the allowed list) | Resolved | The Conventions "Locked AC code" (:87-96) now lists `RecordStoreWrite.Exempt` and the `RecordingText.State` arms, and lets a task record a new type such as `DemoPauseLoadingOutcome`. T028 (:251) and T085 (:452) record them in data-model.md. T097 (:477) allows "those listed in data-model.md and the Conventions". `RecordStore.AddAsync` already receives `EntrySpan entry` (`RecordLogTests.cs:589` mirrors the signature), so T031's "stop and ask" fallback will not trigger. |
| MAJOR-4 (T036 drops the current week's Paused Upcoming commitments) | Resolved (in tasks.md) | T036 (:269) now passes "each of the current week's Upcoming commitments, Paused ones included" to T019's overload. Only `NextUpcoming` leaves out Paused entries. T067 (:383) tests the current-week `Paused` mark. `GoalEvaluation.cs:135/186` confirms that `CurrentWeekUpcoming` feeds only the trend. The source artifact research.md R-06 still says the opposite (MINOR-4). |
| MINOR-1 (T100 roadmap status `implemented`) | Resolved | T100 (:480) keeps `implementing`, which matches `docs/roadmap.md:46-48`. |
| MINOR-2 (unacknowledged breaking UI, host and smoke-check tests) | Partly | Verified against the code: `GoalDetailPageTests.cs:115/308` (T064), `GoalsPageTests.cs:104/107` (T075), `GoalsHostTests.cs:347` (T079), `CommitmentRowTests.cs:83/141` (T051), `TodayPanelTests.cs:135` (T051), `CoverageTests.cs:96` (T093), and T069's note that `TrendChartTests` has no absence assertion. Still unacknowledged: `CoverageTests.Ports` (MINOR-2). |
| MINOR-3 (UI-layer fences for the new port and service) | Resolved | T088 (:455) adds the `UiDependencyTests` rule for `IDemoPauseStore` and adds `DemoPauseService` to `OnlyTheHostsOperatorTypesMayDependOnTheOperatorServices` (`UiDependencyTests.cs:104-117`). A separate Persistence gap is in MINOR-3. |
| MINOR-4 (no fake hook for concurrent writes; store refusals untested) | Resolved | T029 (:252) adds the `BeforeNextWrite` hook. T043 (:302) covers the store's `AlreadyExcused`, `HasRecord` and `Paused`, T054 (:338) covers `Overlaps`, and T038 (:279) covers `Exempt`. |
| MINOR-5 (edge cases and ACs without solution-level tests) | Resolved | T054 covers AC-088 and long pauses. T056 covers AC-087 (matches `acceptance.md:1210-1216`). T057 covers AC-086. T058 covers the time zone change through `ProfileService`, which is feasible under slice 4 FR-026 (`specs/004-goals/spec.md:819-827`). |
| MINOR-6 (ordering, parallelism, wording) | Resolved | The commit group is "T085 to T088" (:155), consistent with T088 (:455). The parallel line (:520-521) puts T038 and T039 after T037. T090 (:460) says "three earlier demo tasks". |

No older note (earlier tasks passes) has regressed.

## Notes

### MAJOR-1: T042's legend change breaks three page and component tests at the Phase 2 checkpoint, and the task list fixes them only in later phases or never
- **Where**: tasks.md T042 (line 283; its edits are limited to `StatusChipTests.cs` and `ConsistencyStripTests.cs`, and its grep to `tests/HT.UiTests/Components/Shared/*.cs` with the pattern `Excused|Paused`); Checkpoint (shared) (lines 285-287: the existing tests are "unchanged apart from three named kinds of edit"); T052 (line 319), T064 (line 362), T069 (line 385).
- **Against**: constitution V (the full suite passes at each checkpoint commit; Conventions line 148). Code:
  - `src/HT.Ui/Components/Shared/ConsistencyStrip.razor` renders the legend whenever `!Mini`.
  - `src/HT.Ui/Components/Shared/GoalStats.razor:19` always uses the full strip.
  - `GoalDetailBody.razor:63` renders GoalStats on every goal page, and so does the visitor Home's example card.
  - The tests that break:
    - `tests/HT.UiTests/Components/Pages/GoalDetailPageTests.cs:116-118`: `Assert.DoesNotContain` over the whole markup for "Pause" and "Excuse";
    - `tests/HT.UiTests/Components/Shared/GoalStatsTests.cs:72`: the exact legend `["Met", "Failed", "Missed", "Pending"]`;
    - `tests/HT.UiTests/Components/Pages/HomeVisitorTests.cs:116`: the same exact legend on the visitor Home's example card.
- **Continues**: new (same class as pass-1 MAJOR-2).
- **Reasoning**:
  - T042 is in Phase 2. It makes the full strip's legend list Excused and Paused after Missed. From then on, every goal page's markup contains "Excused" and "Paused", even with no excusal or pause:
    - `GoalDetailPageTests:116-118` fails, because "Paused" contains "Pause" and "Excused" contains "Excuse" (ordinal substring match).
    - `GoalStatsTests:72` fails on the legend list.
    - `HomeVisitorTests:116` fails on the legend list.
  - None of the three is in T042's edit list or its grep:
    - `GoalStatsTests:72` does not contain the grep's words.
    - The two page tests are outside `Components/Shared`.
  - Where the fixes are placed instead:
    - The goal page's absent list is changed only in T052 (US1, "Excuse") and T064 (US2, "Pause").
    - The GoalStats legend is changed only in T069 (US3).
    - `HomeVisitorTests` is named by no task.
  - T052's reason ("the slice-6 absent list holds 'Excuse', which an Excused row's chip now contains") also misses that, after T042, the legend alone already contains the word.
  - Result: the Phase 2 shared checkpoint cannot be green as written. The checkpoint text forbids the edits that would make it green. A literal implementer must either commit with a red suite (constitution V) or make edits that the task list assigns to other phases or to nobody.

### MINOR-1: T034's list of changed store-call assertions misses `RecordTests.cs`, and leaves the place of the exemption read open against the slice-5 "no store read" tests
- **Where**: tasks.md T034 (line 266: the files that change, and "A test whose access refusal comes before the goal keeps its empty call list"); Checkpoint (shared) (line 286).
- **Against**:
  - `tests/HT.UnitTests/Application/Records/RecordTests.cs:336`: `FR_005_a_note_of_more_than_500_characters_is_refused_and_nothing_is_recorded` asserts `Assert.Empty(_world.Records.Calls)`.
  - The order: `RecordingService.cs:76-90` checks not begun, then the value, then the note. contracts/application-api.md:145 puts 4a to 4c after "not begun" and before the value.
  - `HistoryTests.cs:336/349/362` and `RecordableTests.cs:126/176` assert `Assert.Empty(Calls)` under names ending "…no store read".
- **Continues**: MAJOR-2 (pass 1).
- **Reasoning**:
  - **RecordTests**: steps 4a to 4c need the exemptions, so `RecordAsync` must call `ReadExemptionsAsync` before the value and note checks. The note-too-long refusal therefore no longer leaves `Calls` empty. T034 does not name `RecordTests.cs`, and its only remark about empty lists covers refusals before the goal.
  - **The "no store read" tests**: they pin slice 5's rule that a goal with no entries reads nothing. They pass only if the exemption read happens after the entries are known to be non-empty. T034 says "every read … read once per call" without that condition.
  - The checkpoint's "exact store-call lists of T034" would then let an implementer rewrite these slice-5 assertions instead of keeping their rule.

### MINOR-2: T093 does not add `IDemoPauseStore` to `CoverageTests.Ports`, so the coverage-entry test fails
- **Where**: tasks.md T093 (line 466), which names only the counts in `CoverageTests` (14 to 18, and 10 or 14).
- **Against**: `tests/HT.UiTests/SmokeChecks/CoverageTests.cs:20` (`Ports = [IDemoDataStore, IDemoHistoryStore, IDemoReportStore]`) and `:49-56` (`Every_coverage_entry_matches_a_data_parameter_of_its_demo_data_port`, which matches entries against `DataParameters()` built from `Ports`). Also research R-09 "Coverage".
- **Continues**: MINOR-2 (pass 1).
- **Reasoning**:
  - The four new `IDemoPauseStore` entries match no parameter of a port in `Ports`, so that test fails.
  - Without the port in `Ports`, the theory `Every_data_parameter_of_a_demo_data_port_has_a_coverage_entry` never checks the new port. That theory is the rule that "a slice that adds demo data … fails until it extends the smoke check" (`CoverageTests.cs:12-13`).
  - The class's doc comment, which lists the three ports, also goes stale.

### MINOR-3: T088's Persistence instruction does not reach the rule that keeps the demo store internal
- **Where**: tasks.md T088 (line 455): "add the port to `PersistenceDependencyTests.cs` wherever `IDemoReportStore` appears", checked with `grep -n "IDemoReportStore\|DemoReportService"`.
- **Against**: `tests/HT.ArchitectureTests/PersistenceDependencyTests.cs:42-58`. There `IDemoReportStore` appears only in a doc comment (line 44). The rule itself is the theory `TheDemoAccountWriterAndTheDemoReportStoreAreInternal` with `[InlineData("HT.Persistence.Habits.DemoReportStore")]`, which neither grep term matches. Also T087 ("following `DemoReportStore.cs`") and constitution III / ADR 0010 (private-data writers reached only through the port).
- **Continues**: new.
- **Reasoning**:
  - Read literally, T088 adds nothing to `PersistenceDependencyTests.cs` (the port is not a Persistence type).
  - No task states that `DemoPauseStore` must be internal, or that it joins the theory's rows.
  - So a public `DemoPauseStore`, which writes private excusals and pauses without an acting user, would pass every architecture test. Slice 6 fenced its demo store in exactly this way.

### MINOR-4: research.md R-06 still says `CurrentWeekUpcoming` leaves out Paused entries, which T036 now contradicts, and no task records the correction
- **Where**: tasks.md T036 (line 269); Conventions (lines 59-61: "The design artifacts stay the contract … records it in the same task, in the document it names").
- **Against**: research.md:222 ("`NextUpcoming` and `CurrentWeekUpcoming` leave out Paused entries (FR-019, FR-016)") against research.md:254-257 (R-07) and spec FR-016.
- **Continues**: MAJOR-4 (pass 1).
- **Reasoning**:
  - T036 now follows R-07 and FR-016, which is right. But the bullet of R-06 that caused pass-1 MAJOR-4 is still in the design artifact, and no task says to correct it.
  - The plan cites R-06 as the source of T036 ("research R-06"). A reader who checks T036 against its source finds the two disagreeing.
  - This is the "silent divergence" that the Conventions and constitution Governance rule out.

### MINOR-5: Two factual slips in task texts
- **Where**: tasks.md T016 (line 219), T088 (line 455).
- **Against**: code.
- **Continues**: new.
- **Reasoning**:
  - **T016's list**: the "today" list of `EvaluationState` users leaves out `src/HT.Application/Records/EntryViews.cs:31`, `RecordingService.cs:379`, `src/HT.Ui/Components/Records/HistoryCard.razor:191` and `src/HT.Ui/Components/Shared/GoalItem.razor:22/59/73`, which its own grep returns. The grep is authoritative, so this is only imprecise.
  - **T088's exclusions**: T088 says to exclude `IDemoPauseStore` "from the other operator rules that `IDemoReportStore` is excluded from". No such exclusion exists: `ApplicationDependencyTests.cs:40-56` (the demo data and demo history rules) do not mention `IDemoReportStore`, so the instruction points at nothing.

## Summary
1 MAJOR, 5 MINOR. The revision resolves pass 1's four MAJOR notes in tasks.md:
- the `RecordingText.State` arms and the NoteDialog tests;
- `FaultingRecordStore` and the exact store-call lists;
- `RecordStoreWrite.Exempt` and `DemoPauseLoadingOutcome` on the allowed list;
- the current week's Paused Upcoming commitments in the trend.

Most of the MINOR notes are resolved too, and every cited line number and file was confirmed against the code.

One blocking problem remains. T042, in Phase 2, adds Excused and Paused to the full strip's legend, and that legend is on every goal page and on the visitor Home. So `GoalDetailPageTests`, `GoalStatsTests` and `HomeVisitorTests` fail at the Phase 2 shared checkpoint. The task list fixes them only in US1 to US3, or not at all, while the checkpoint text forbids touching them.

The MINOR notes are residual omissions in the acknowledged-test lists (`RecordTests.cs:336` and the "no store read" tests, `CoverageTests.Ports`), a missing internal-type fence for `DemoPauseStore`, a stale research bullet, and two wording slips.
