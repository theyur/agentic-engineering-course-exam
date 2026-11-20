# Critic pass 2: tasks for 007-personal-reports

**Verdict**: APPROVE
**Date**: 2026-11-20
**Reviewed**: specs/007-personal-reports/tasks.md (against spec.md, plan.md, research.md, data-model.md, contracts/application-api.md, contracts/host.md, the constitution, CLAUDE.md, docs/roadmap.md, docs/operations/hosted-demo.md, design/, and the code and tests in src/, tests/ (outside HT.AcceptanceTests) and tools/HT.SmokeCheck)

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 shared fixture clock | Resolved | Conventions lines 103-114 forbid moving `DatabaseFixture.Clock` / `SeedDatabaseFixture.Clock` and require an own provider over `ConnectionString`, as `RecordReadTests.BuildServices` (`Records/RecordReadTests.cs:211-219`) does. On `HT-IT-SEED` the reset goes through `HtDatabase.ResetAsync` (`src/HT.Persistence/HtDatabase.cs:10`), as `ResetAndBuildAsync` (`SeedDatabaseFixture.cs:43-53`) does. T035, T043, T063, T064 and T083 use their own clocks. T068 keeps the fixture clock unmoved, and T069 runs each weekday as its own test with its own clock. The `SeedDatabase` collection runs serially and each existing test resets at its start, so T069's reset leaves no state behind. `RecordLogTests.BuildServices(capture, clock, faults)` (line 86) already shows how to combine `LogCapture` with an own clock (T064). |
| MAJOR-2 dialog focus return | Resolved | The new Conventions bullet (lines 121-125), T051 and T059 now name the target, the fallbacks and the tests (`VerifyFocusAsyncInvoke`, as `GoalDetailPageTests.cs:255` does). T086 sweeps dialog focus. A remaining gap in the mechanism is new MINOR-2. |
| MINOR-1 Upcoming lookup | Resolved | T030 states that `NextUpcoming` reads its own `TryEntries(goal, windowStart, windowStart + 6)` window, which matches research R-06 and data-model.md:82. |
| MINOR-2 split `<small>` values | Partly | The Conventions (lines 65-70), T004, T006 and T008 settle the question: each value is one whole text. The page previews of T003, T007 and T009 carry the same split and are not covered (continued in MINOR-1). |
| MINOR-3 `FakeGoalStore` lookups | Resolved | T062 adds a test-only `Lookups` count. `FakeGoalStore.FindAsync` and `ListAsync` (lines 252-266) are the members to count. |
| MINOR-4 Upcoming order and Unpublished habit | Resolved | T053 covers the order by start instant with goal-order ties (AC-038), and an Unpublished habit. T033 and T048 cover an Unpublished habit through `CatalogHabits.TrySetStateAsync`, as `GoalReadTests.cs:160` does. |
| MINOR-5 hours conversion | Resolved | T015 now says that the recorded amount stays as stored and only the target is converted, as `Evaluator.AmountInRecordUnits` (`Evaluator.cs:100`) does. |
| MINOR-6 per-day title | Resolved | T005 gives "Minutes per day" to a Reduce duration per-day goal and record-count bars to a Build per-day goal. This matches research R-05's `Kind` rule. |
| MINOR-7 mini strip | Resolved | T004 adds `ht-strip--mini` with a counting label. T032 adds the `Mini` flag. `design/css/ht-components.css:356-357` has the class. |
| MINOR-8 startup whole-list tests | Resolved | T070 names `Assert.Equal(4, demoLines.Count)` (`StartupLogTests.cs:45`), the records and goals line tests (`StartupTasksDemoTests.cs:75-110`) and the fault test, and asks for a grep that also finds `The_started_host_has_logged_demo_history_loading_after_demo_loading_with_no_fault`. |
| MINOR-9 smoke check depends on T076 | Resolved | T079 now lists T076 and gives the reason. The Dependencies section (lines 511-513) agrees. |
| MINOR-10 conventions contradicting tasks | Resolved | Line 147 names both new members. Lines 54-56 make the formatters and the fixed data synchronous, as the contract gives them. |
| MINOR-11 `PageServices` rationale | Resolved | Lines 143-145 and T074 now match `PageServices.cs`: services registered by hand, no operator or demo service, and `FakeDemoHistoryStore` used only in `HT.UnitTests`. |
| MINOR-12 runbook check 6 | Resolved | T081 changes `docs/operations/hosted-demo.md:178` to "(14 of 14)" (11 + 3) and the description at `:187`, and adds check 7's line. |
| MINOR-13 analyzer comparison | Resolved | T085 runs Roslynator solution-wide, the scope of CLAUDE.md's baseline. It sorts the new diagnostics into expected Razor and test-only growth and real findings, and updates the baseline. |
| MINOR-14 skipped roadmap statuses | Resolved | T001 acknowledges the four statuses that were not recorded (`docs/roadmap.md:36` still reads `clarified`) and gives its reason. "Updating this file" (lines 255-257) asks for the row to show the current state, which a single edit restores. |

No older note has regressed. The specs' own passes are not re-litigated here.

## Notes

### MINOR-1: Three page previews keep the split `<small>` value that the Conventions forbid in code
- **Where**: Conventions lines 65-70 ("the slice-6 states of T004, T006 and T008 show the whole text"). T003 (PageGoalDetail), T007 (PageHomeSignedIn, the GoalSummary card) and T009 (PageHomeVisitor) say nothing about it.
- **Against**:
  - `design/pages/PageGoalDetail.html:24` (`90% <small>(18 of 20)</small>`);
  - `design/pages/PageHomeSignedIn.html:25-26` (`ht-goal__pct` with `75% <small>(15 of 20)</small>`);
  - `design/pages/PageHomeVisitor.html:21` (`90% <small>(18 of 20)</small>`);
  - constitution IV ("Pages MUST follow the structure … of their previews").
- **Continues**: MINOR-2 of pass 1.
- **Reasoning**: the whole-text decision is recorded only for the component previews (GoalStats, GoalListItem with PageGoals, GoalSummary). The slice-6 states of the three pages are built from the full previews, which split the value. As written, T003, T007 and T009 leave those pages showing the split. The built pages, which render the whole `ReportText` text, would then differ from their own slice-6 previews, and the sweep in T086 checks the pages against those previews.

### MINOR-2: The components that hold the focus targets get no way to be focused
- **Where**: T051 (the focus goes back to "that goal's 'Record' button … or the goal's name link"); T059 (back to "the same row's action", or the "Due now" tile); T050 (`GoalItem`), T056 (`CommitmentRow`) and T055 (`StatTile`). These render those controls, and their parameter lists and test lists name no focus member.
- **Against**: the Conventions' focus bullet (lines 121-125: "focuses the control again by its key after the re-read, as `GoalDetailBody` does with `FocusRowAsync`"). In the existing code that pattern works only because the buttons belong to `HistoryCard`, which exposes `FocusAsync(DateOnly, RecordId?)` and `FocusNoteAsync(DateOnly)` (`HistoryCard.razor:145-171`). `GoalDetailBody` calls these through `@ref` (`GoalDetailBody.razor:55`, `:196-200`).
- **Continues**: new (it follows from the fix for MAJOR-2 of pass 1).
- **Reasoning**:
  - The "Record" button and the name link are inside `GoalItem`, the row actions inside `CommitmentRow`, and the "Due now" button inside `StatTile`.
  - An island cannot reach an `ElementReference` inside a child component unless that component exposes one, or a focus method as `HistoryCard` does.
  - T050, T055 and T056 add no such member, and their component tests do not cover one. T051's and T059's focus tests therefore depend on a public component member that no task defines.

### MINOR-3: T037 cites a goal-page test assertion that does not exist, and leaves out the ones that will break
- **Where**: T037 (line 280: "its slice-5-state assertions ('no GoalStats, strip, trend') change to the slice-6 state"); T047 (adds the trend card to the same page).
- **Against**:
  - `tests/HT.UiTests/Components/Pages/GoalDetailPageTests.cs`. It contains no "GoalStats" text (grep). The slice-5 assertions that the GoalStats and TrendChart cards break are:
    - lines 101-103: the exact first card title of each column, `["History", "Settings"]`;
    - line 114: "Consistency" and "streak" in the absent list;
    - lines 289 and 326: the exact list of every card title on the page, `["History", "Settings"]`, for a goal with begun entries.
  - The design GoalStats card's title has the class `ht-card__title` ("Consistency and streaks"), and so does the TrendChart card's title. The quoted words come from the design page's slice-5 state (spec line 776), not from the test file.
- **Continues**: new (the same class as MINOR-8 of pass 1).
- **Reasoning**: an implementer who follows T037 finds nothing that matches its description. The exact-list assertions at lines 102, 289 and 326 fail as soon as T037 lands, and again when T047 adds the trend title. Neither task names them.

## Summary

0 MAJOR, 3 MINOR. Previous notes: 15 Resolved, 1 Partly, 0 Unresolved.

The revision fixes both blocking problems of pass 1:

- **Time in integration tests.** The tests now use their own providers and clocks over the fixture's connection string, following existing patterns (`RecordReadTests`, `RecordLogTests`, `ResetAndBuildAsync`), and the fixture clocks are never moved.
- **Dialog focus.** Both new dialog hosts return the focus, with explicit fallbacks and tests.

The other minor notes are closed with evidence that matches the code. That includes:

- the startup-log test names;
- the coverage count from 10 to 14;
- check 6 growing from 11 to 14;
- the `FakeGoalStore` lookup count;
- the clock-change instants in Kyiv and Berlin;
- demo4's Yoga figures per weekday, re-derived from `DemoHistory.YogaGoal`.

What remains is sharpening:

- the split value in three page previews;
- a focus member that `GoalItem`, `CommitmentRow` and `StatTile` need but no task defines;
- an inaccurate description of the goal-page test assertions that slice 6 breaks.

None of these blocks implementation.
