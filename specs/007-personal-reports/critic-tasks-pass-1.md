# Critic pass 1: tasks for 007-personal-reports

**Verdict**: REVISE
**Date**: 2026-11-20
**Reviewed**: specs/007-personal-reports/tasks.md (against spec.md, plan.md, research.md, data-model.md, quickstart.md, contracts/application-api.md, contracts/host.md, acceptance.md, the constitution, CLAUDE.md, docs/roadmap.md, docs/adr/0020 and 0021, design/, and the code and tests in src/, tests/ (outside HT.AcceptanceTests) and tools/HT.SmokeCheck)

## Notes

### MAJOR-1: Integration tasks change the time on a clock that other tests share and that cannot go back, which breaks existing tests depending on test order
- **Where**: tasks.md Conventions, lines 97-98 ("Goals with past results are created at an earlier supplied time ... by moving the `FakeTimeProvider` forward") and lines 109-112 (integration tests use `HT-IT` (`DatabaseFixture`) and `HT-IT-SEED` (`SeedDatabaseFixture`)). Also T035 (line 244, "created at a supplied time 30 days earlier"), T043, T063, T064, T068 (line 406, "at a fixed time ... The existing `DemoHistoryLoadingTests` pass unchanged"), T069 (lines 407-411, "the three tasks run at `2026-11-16T12:00:00Z` ..., `2026-11-17...` and `2026-11-18...`") and T083 (line 447, "created at a supplied time a year earlier").
- **Against**: the existing fixtures and the tests that depend on them. Constitution V (the full suite must pass at every checkpoint).
- **Reasoning**:
  - Each fixture has exactly one `FakeTimeProvider`, and it is wired into the services the fixture hands out:
    - `tests/HT.IntegrationTests/SeedDatabaseFixture.cs:40` creates a clock at `2026-11-14T09:00Z`, and `ResetAndBuildAsync` (lines 43-53) registers that same instance. This is the only builder the fixture provides.
    - `DatabaseFixture.cs:33` and `:44` do the same at `2026-11-11T09:00Z`.
  - Existing tests in the same collections hard-code dates that assume the clock never moved:
    - `Records/DemoHistoryLoadingTests.cs:31-32` (`Start = 17 Oct`, `RunDate = 14 Nov`);
    - `Operator/DemoGoalsTests.cs:27` (`DayOfTheRun = 2026-11-14`);
    - `Goals/GoalCreationTests.cs:29` (`Today = 2026-11-11`, on `database.Clock`).
  - `FakeTimeProvider.SetUtcNow` refuses to move backwards. So T069's runs on Mon 16, Tue 17 and Wed 18 November would leave the shared seed clock at 18 November or later.
  - xUnit does not promise an order for the classes in a collection. Any of the tests above that runs after T069 then gets the wrong run date and fails. The same applies to "a year earlier" (T083) and "30 days earlier" (T035), which cannot be set on the shared clock at all without going back.
  - The repo already has a pattern that avoids this: a private clock and its own provider per test (`Records/RecordReadTests.cs:176`, `:211-217`; `RecordStoreTests.cs:323`). But no task asks for it. T068 and T069 explicitly name `HT-IT-SEED`, whose fixture offers only the shared-clock builder.
  - As written, the tasks lead to order-dependent failures of existing tests. Neither the tasks nor the plan's "Risks" mention this.

### MAJOR-2: No task requires focus to return to the opener for the two new dialog hosts (constitution IV)
- **Where**: T051 (line 337, `GoalsList` opens `RecordDialog` and reads the overview again after a save), T059 (line 368, `TodayPanel` opens `RecordDialog` and `NoteDialog` and reads the dashboard again after a save), and T086 (line 450, the constitution IV / HP-006 sweep).
- **Against**: constitution IV ("dialogs MUST manage focus (first field on open, Esc and × close, focus returns to the opener)"), and the existing dialog code.
- **Reasoning**:
  - `src/HT.Ui/Components/Shared/HtDialog.razor:109-121` gives the focus back only through the page's `FocusOnClose` or `ReturnFocus`. If the page passes neither, the focus goes nowhere. `RecordDialog.razor:129-137` and `NoteDialog.razor:59-67` take these parameters as optional.
  - `GoalDetailBody.razor` needed explicit logic (`FocusRowAsync`, `FocusNoteRowAsync`), because a re-read replaces the row's button.
  - Both new islands also re-read and re-render their rows after a save. In the overview the goals can even change order, because FR-011 orders them by group.
  - Neither T051 nor T059 says how the focus returns, and neither tests it (their test lists name only order, opening, refresh and the empty sentence). T086 lists the constitution IV checks it sweeps and leaves dialog focus out.
  - Following the tasks as written produces two dialog hosts that break a constitution MUST.

### MINOR-1: Where `GoalEvaluation` finds Upcoming entries is underspecified, and a literal reading yields none
- **Where**: T030 (line 224, `NextUpcoming(windowStart)`: "the first Upcoming entry whose first date is at most 6 days after `windowStart`"; "A goal with no current date has no entry and no store read"), and T054 (line 363).
- **Against**: FR-012 / US6-AS8 / AC-038, and research R-06 ("They are found with `GoalTimeline.TryEntries(goal, from, from + 6)`").
- **Reasoning**:
  - T030 builds the evaluation's entries from `GoalTimeline.EntriesThrough(goal, currentDate)`, which by definition stops at the current date (T016).
  - The Upcoming entries FR-012 needs lie after the current date: AC-038's Wednesday 18 Nov for a Monday-Wednesday-Friday goal read on Tuesday, and the Monday 23 Nov entries. For a goal that has not started, the evaluation has no entries at all.
  - T030 does not say that `NextUpcoming` reads its own `TryEntries` window, so a lookup among `Entries` returns nothing. Only research R-06 says where these entries come from.

### MINOR-2: The design splits figure and unit into separate elements, but every component is forbidden to build figure texts
- **Where**: Conventions line 63-64 ("Every consistency, streak ... text comes from `ReportText` ... No component builds one"); T036 (line 245, values `ConsistencyText`, `CurrentStreak`, ...), T050 (line 336), T057 (line 366).
- **Against**:
  - `design/components/GoalStats.html:13` (`90% <small>(18 of 20)</small>`);
  - `design/pages/PageGoals.html:15-16` (`6 <small>commitments</small>`);
  - `design/components/GoalSummary.md` (`ht-goal__pct`, "count in `small`");
  - constitution IV ("Pages MUST follow the structure ... of their previews");
  - contracts/application-api.md "ReportText", which has no member for the percentage part or the count part.
- **Reasoning**: to follow the design's markup, a component must split "90% (18 of 20)" or "6 commitments" into parts, or build them from the numbers. Either way it builds a figure text, which the Conventions forbid. No task settles whether `ReportText` gains part members or whether the design is changed. The Conventions require such a choice to be recorded ("settles a text or layout the design artifacts leave open").

### MINOR-3: T062 cannot check "before any goal lookup" with the existing fakes
- **Where**: T062 (line 385: "each with no view and before any goal lookup (the fakes' calls show none)").
- **Against**: `tests/HT.UnitTests/Fakes/FakeGoalStore.cs`. It counts only `CreateAsync` and the time-zone writes (`CreateCount` at line 46, `TimeZoneWriteCount`/`TimeZoneChangeCount`), and records nothing for `FindAsync` or `ListAsync` (lines 252-266). Only `FakeRecordStore` has a `Calls` log (`FakeRecordStore.cs:34`).
- **Reasoning**: the test as described needs a call log or a wrapper around the goal store, and no task adds one. FR-015's "checked ... before the goal" therefore has no stated way to be proven through the goal port.

### MINOR-4: Coverage gaps: Upcoming order across goals, and reports for a goal on an Unpublished habit
- **Where**: T053 (line 357: the Upcoming cases cover one goal's entry, exclusion, and a goal that has not started); T033, T039 and T041 to T045 (no Unpublished case).
- **Against**:
  - contracts/application-api.md "GetDashboardAsync" ("ordered by start instant, then goal order") and AC-038 (`Weekly` and `Soon` on the same date, in goal order);
  - spec Edge Cases line 473 ("A goal whose catalog habit is Unpublished keeps its reports") and AC-059.
- **Reasoning**:
  - The order of several Upcoming entries, including the tie-break by goal order, has no unit or integration test.
  - The Unpublished edge case is covered only by the AC suite, which is written after implementation.
  - CLAUDE.md "Testing" requires unit and integration tests as part of done.

### MINOR-5: T015's wording for hours can be read as multiplying the recorded amount by 60
- **Where**: T015 (line 201: "commitment-based count or duration: the recorded amount (minutes; hours times 60) against the amount").
- **Against**:
  - `src/HT.Domain/Records/RecordValue.cs` ("The amount of a count or duration record (minutes for a duration)");
  - `Evaluator.AmountInRecordUnits`, which converts only the target;
  - research R-05's table ("its recorded amount (minutes for a duration) | the amount (× 60 for hours)").
- **Reasoning**: records are already stored in minutes. The parenthesis sits on "the recorded amount", so it invites converting the value instead of the target. That would make AC-058's values 60 times too large.

### MINOR-6: T005 pairs a per-day goal with a title for amount bars
- **Where**: T005 (line 173: "per-day ('Minutes per day · last 14 days')").
- **Against**:
  - FR-006 / research R-05: a Build per-day period's bars are numbers of records against N (`RecordCount`);
  - research R-13 ("its measurement and count unit give the title of an amount bar ('Minutes per day', 'Pages per day')");
  - US4-AS3: "Reading, 20 min daily", the spec's "Minutes per day" example, is commitment-based on all seven days.
- **Reasoning**: the preview list that T005's table and T028 must implement mislabels which goal kind gets "Minutes per day". This risks a wrong title for Build per-day goals (AC-065's "2 times" per-day goal).

### MINOR-7: The mini strip's modifier and accessible label are left open
- **Where**: T032 (line 229: the parameters of `ConsistencyStrip` are the states, the unit and the legend), T050 (line 336, "the mini strip of 15"), and T006 (line 180).
- **Against**: `design/pages/PageGoals.html:17` (`ht-strip ht-strip--mini`, `aria-label="Last 15 results"`), `design/css/ht-components.css:356-357`, and HP-006 (a label "that counts each state").
- **Reasoning**:
  - `ConsistencyStrip` gets no mini variant, so it cannot render `ht-strip--mini`.
  - The design's mini label does not count states, while `ReportText.StripLabel` does.
  - T006 does not say which label the slice-6 design state uses. The component would then differ from its preview.

### MINOR-8: T070 does not name all the startup tests that the demo5 lines break
- **Where**: T070 (line 412).
- **Against**:
  - `tests/HT.UiTests/Operator/StartupTasksDemoTests.cs:75-82`: `Every_demo_records_line_holds_the_demo_email_and_the_result_text_only` expects exactly one records line, demo4's;
  - `StartupTasksDemoTests.cs:104-110`: `Every_demo_goals_line_holds_the_demo_email_and_the_result_text_only` expects `Emails.Append(Demo4)`;
  - `tests/HT.UiTests/Host/StartupLogTests.cs:45-48`: `Assert.Equal(4, demoLines.Count)`.
- **Reasoning**: these tests compare complete lists, not order. T070's items ("demo5's lines come after demo4's", the second start, the refusal lines, the fault tests) do not cover them explicitly. They fail as soon as T076 lands.

### MINOR-9: T079 and T080 do not depend on T076, though the real-host smoke check needs demo5
- **Where**: T079 (line 424, "after T047, T052, T061, T074") and T080 (line 429, "after T079").
- **Against**: T080's `SmokeCheckHostTests` run the real smoke check against a host whose startup tasks must create `demo5@example.com` (contracts/host.md "Smoke check"; plan Work order 7). That account comes only from T076.
- **Reasoning**: the dependency lists let T079 and T080 run before T076. The host test then fails on check 7's sign-in. Only the phase checkpoint hides the gap.

### MINOR-10: Two Convention statements contradict the tasks
- **Where**:
  - Conventions lines 121-123: "The only new member of an existing type is `RecordingText.HoursLeft`";
  - lines 54-55: "Every Application method and every port method is `async` and takes a `CancellationToken` last".
- **Against**:
  - T016 and T084 add `GoalTimeline.EntriesThrough`, a new member of an existing `HT.Domain` type;
  - contracts/application-api.md gives `ReportText`, `ExampleGoal` and `RecordingText.HoursLeft` as synchronous static members, and data-model.md gives `DemoReport.For` the same way.
- **Reasoning**: a rule that an implementer is told to stop on ("If a task seems to need more, stop and ask the owner") contradicts T016. The async rule, read literally, contradicts the contract's formatter. Both make the conventions an unreliable checklist.

### MINOR-11: Registering `FakeDemoReportStore` in the bUnit page services rests on a wrong reason
- **Where**: Conventions lines 116-120 (the pairings "so `ValidateOnBuild` never meets an unregistered service or port", including "the bUnit registration of `FakeDemoReportStore`") and T074 (line 419).
- **Against**: `tests/HT.UiTests/Components/Pages/PageServices.cs:57-85`.
- **Reasoning**:
  - The bUnit services are registered by hand. They include no operator service and no demo store (`FakeDemoHistoryStore` is not registered there), and they are not built with `ValidateOnBuild`. `ValidateOnBuild` is used only in `DatabaseFixture`, `SeedDatabaseFixture` and the host.
  - No page resolves `DemoReportService`. So the registration does nothing, and the stated reason is false.

### MINOR-12: T081 leaves the hosted-demo runbook's check 6 text stale
- **Where**: T081 (line 435: demo5, the fourth task with its log lines, and check 7 only).
- **Against**: `docs/operations/hosted-demo.md:178` (`PASS demo history demo4@example.com (11 of 11)`) and `:187` ("checks demo4's 3 goal names on Goals and 8 texts in the History cards"). T079 and T080 grow check 6 by 3 texts (HP-008).
- **Reasoning**: after the slice, the runbook's sample output and its description of check 6 no longer match the smoke check.

### MINOR-13: T085's comparison with the analyzer baseline does not compare like with like
- **Where**: T085 (line 449).
- **Against**: CLAUDE.md "C# code analysis".
  - The Roslynator baseline (333 diagnostics, 37 not from the compiler) is a solution-wide run, and is "almost all Razor phantoms" from the bUnit tests.
  - "If a run shows far more than that, something changed."
- **Reasoning**:
  - T085 runs Roslynator only on `HT.Domain`, `HT.Application` and `HT.Persistence`, which the solution-wide baseline cannot be compared with.
  - The `jb` run is solution-wide, and slice 6 adds many bUnit tests, so its phantom count will rise as slice 4's did.
  - No task records new baselines. The comparison, as specified, cannot tell real findings from expected growth.

### MINOR-14: The roadmap row skipped the step statuses before T001
- **Where**: T001 (line 137: set Status to `implementing`).
- **Against**:
  - `docs/roadmap.md:36`: slice 6's row is still `clarified`, although `acceptance.md` and the features are locked and the plan and the tasks exist;
  - `docs/roadmap.md:255-257`: "the feature branch updates its row's Status after each step".
- **Reasoning**: T001 jumps the row from `clarified` to `implementing`. The step statuses that the "Updating this file" rule asks for were never recorded, and the tasks do not acknowledge that.

## Summary

2 MAJOR, 14 MINOR.

The task list follows the spec, plan and contracts closely. Coverage of FR-001 to FR-032, HP-001 to HP-011 and SC-001 to SC-008 is nearly complete, and the hand-computed instants, demo figures and counts checked here are correct:

- the Kyiv and Berlin clock-change instants and the grace ends;
- demo4's and demo5's figures, and HP-004's 21 states;
- the coverage count from 10 to 14, and check 6's count growing by 3.

The tasks are not ready to implement as written, for two reasons:

- **Time in integration tests.** The integration tasks move a fixture clock that is shared and cannot go back. Existing tests hard-code dates on that clock (`DemoHistoryLoadingTests`, `DemoGoalsTests`, `GoalCreationTests`), so following T035, T068, T069 and T083 literally makes the suite order-dependent and red.
- **Dialog focus.** The two new dialog hosts, `GoalsList` and `TodayPanel`, have no task or test for constitution IV's focus return.

The minor notes are mostly sharpening:

- where Upcoming entries come from;
- the design's split value markup against the "no component builds text" rule;
- dependencies and test lists that are incomplete;
- convention statements the tasks themselves contradict;
- stale operator documentation.
