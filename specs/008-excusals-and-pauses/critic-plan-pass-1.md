# Critic pass 1: plan for 008-excusals-and-pauses

**Verdict**: APPROVE
**Date**: 2026-11-21
**Reviewed**: plan.md, research.md, data-model.md, quickstart.md, contracts/application-api.md, contracts/host.md

## Notes

### MINOR-1: The plan names an architecture fence that does not exist and misses the one that the new services break
- **Where**: plan.md Work order step 3 (lines 241-243: "the widened `IRecordStore` fence"); research.md R-11
  "Architecture tests" (line 380: "the `IRecordStore` fence admits `ExcusalService` and `PauseService`");
  Constitution Check row V ("the widened `IRecordStore` fence"). Constructors in contracts/application-api.md
  (lines 29 and 69-70) take `IGoalStore`.
- **Against**: tests/HT.ArchitectureTests/ApplicationDependencyTests.cs lines 72-81,
  `OnlyTheGoalTypesTheThreeHabitServicesTheProfileServiceTheRecordingServiceAndTheReportServiceMayDependOnTheGoalStorePort`;
  CLAUDE.md "Commits" (the pre-commit hook runs the architecture tests).
- **Reasoning**: HT.ArchitectureTests has no Application-side `IRecordStore` fence. The only `IRecordStore` rule is
  UiDependencyTests.cs:80, which keeps HT.Ui away from the port and needs no change. The rule that does fence a
  port to named services is the `IGoalStore` rule. It allows only the Goals namespace, three habit services,
  `ProfileService`, `RecordingService` and `ReportService`. `ExcusalService` and `PauseService` live in
  `HT.Application.Exemptions` and take `IGoalStore` in their constructors, so they break this rule. The rule's name
  lists the allowed services, so it has to be renamed as well. The plan does not mention the change, so the step 3
  and 4 commit fails the pre-commit gate. `/speckit-tasks`, which works from this plan, would not get a task for
  the change.

### MINOR-2: R-07 gives two different targets for a fully exempt yes/no week and leaves "all exempt" open for the current week
- **Where**: research.md R-07, "Commitment-based yes/no goals" and "The line" (lines 248-256); data-model.md
  `TrendBar` and `TrendBuilder.Of` rows (lines 96-97).
- **Against**: spec.md FR-016 ("a week's Build target counts only its commitments that are neither Excused nor
  Paused"; "A week whose commitments are all Excused or Paused is marked ..."); the existing
  `ReportText.TrendLabel` (src/HT.Application/Reports/ReportText.cs:202-213), which lists every bar whose target
  differs from the line.
- **Reasoning**:
  - **The target.** R-07 says a yes/no week's Build target "counts the week's commitments ... that are neither
    Excused nor Paused". For a week whose commitments are all exempt, that target is 0. The same section then says
    "A marked bar keeps the target its week or entry would have had, so the line does not drop to 0", and
    data-model.md says a marked bar "keeps its own `Target`". These rules give different values: 0 by the first
    rule, the full commitment count by the second. The chosen value becomes `Trend.Target` when the latest week is
    fully paused, and it changes the trend's `aria-label`. AC-064 and AC-065 don't check the target, so no
    acceptance test decides between the two readings.
  - **"All exempt".** R-07's grouping works on the begun items. Take a current week whose begun commitments are all
    exempt while its Upcoming commitments are not, for example Monday Paused with Wednesday and Friday still to
    come. R-07 can read that week as "all exempt" and mark it, with no value and no target. FR-016 marks a week only
    when its commitments are all Excused or Paused, and Wednesday and Friday are neither.

### MINOR-3: Spec discrepancy 2 and R-06 cite an AC-022 row that the locked criteria do not have
- **Where**: plan.md "Spec discrepancies" 2 (lines 307-309: "AC-022 row 03 expects that order (critic-ac
  MINOR-2)"); research.md R-06 "Adding a record" (lines 206-208: "AC-022 row 03").
- **Against**: acceptance.md AC-022 (lines 477-491) and the locked feature
  (tests/HT.AcceptanceTests/Features/F008/excusals-and-pauses.feature, AC-022), which have only rows 01
  (2026-11-16, paused) and 02 (2026-11-17, "Nothing is scheduled on this date."); critic-ac-pass-1.md MINOR-2,
  which describes an earlier draft.
- **Reasoning**: row 03, which expected "You cannot record a day that has not begun." on a paused date, was removed
  before the lock. So no locked acceptance test fixes whether "not begun" comes before step 4a. The plan's reading
  of slice 5's step (4) may still be right, but the evidence it cites does not exist. The order rests only on the
  plan's own interpretation and on the unit test promised in R-11.

### MINOR-4: "All eight features" is wrong: six features have acceptance tests
- **Where**: plan.md Risks (line 328: "`/speckit-ac-tests-analyze` runs all eight features"); research.md R-11
  "Acceptance tests" (lines 385-386); quickstart.md section 2 (line 53: "The analysis runs all eight features").
- **Against**: spec.md FR-023 (F001, F002, F004, F005 and F007 as the earlier locked criteria). Only
  `specs/001`, `002`, `004`, `005`, `007` and `008` have `acceptance.md` and `ac/`; `specs/003-demo-hosting` and
  `specs/006-note-editing` have neither.
- **Reasoning**: the acceptance-test run covers F001, F002, F004, F005, F007 and F008, which is six features. The
  quickstart itself names five earlier features plus F008, so the "eight" contradicts its own list. Anyone checking
  the analysis report against "eight" would look for two features that don't exist.

### MINOR-5: R-05's excusal check list folds FR-002 step 4 into step 5, against the contract and the spec order
- **Where**: research.md R-05 (lines 174-181: "step 5: `GoalTimeline.EntryOf`, and the goal must be
  commitment-based"; no step 4); contracts/application-api.md `ExcuseAsync` table (lines 42-50: step 4
  period-based, step 5 `EntryOf`).
- **Against**: spec.md FR-002 ((4) period-based "Only commitments can be excused..." before (5) "Nothing is
  scheduled on this date.").
- **Reasoning**: R-05 merges the two checks into one step, and the contract keeps them apart. Take a period-based
  goal and a date with no entry, for example before its start date or before a per-week goal's first week. FR-002
  then requires step 4's text, but a check written from R-05's "step 5" could give "Nothing is scheduled on this
  date.". The two design artifacts describe the same use case in different orders.

### MINOR-6: Two changed components are missing from the source tree and the scope
- **Where**: plan.md Scale/Scope (lines 107-109: "4 changed ones: StatusChip, ConsistencyStrip, TrendChart and
  RecordDialog. 2 changed islands: GoalsList and TodayPanel"); Project Structure, `HT.Ui/Components` (lines
  177-185); contracts/host.md "Goals page" and "Signed-in Home".
- **Against**:
  - `src/HT.Ui/Components/Shared/CommitmentRow.razor` decides each row's single action (`ActionKind`, lines 45-50
    and 90). design/components/CommitmentRow.md:13 and :44 put Pending → "Record" + ghost "Excuse" there and say
    "No ... 'Excuse' (slice 7)".
  - `src/HT.Ui/Components/Reports/GoalsList.razor:23` renders each goal as a `GoalItem`, and
    `src/HT.Ui/Components/Shared/GoalItem.razor` holds the meta line (line 18). The plan's Work order step 1 also
    changes `GoalListItem` and `CommitmentRow` in `design/`.
- **Reasoning**: HP-005's "Excuse" on Pending rows goes into `CommitmentRow`, not only `TodayPanel`.
  `CommitmentRow` now offers one action that is never primary, so it needs a second action. HP-004's "Paused until"
  meta and the secondary "Resume" go into `GoalItem`. Constitution IV requires each design component to be
  implemented once and reused, so this markup belongs in those components. The plan names only the islands, so
  `/speckit-tasks` may leave out the component changes and their bUnit tests, or build the markup inline in the
  islands.

### MINOR-7: The data-model transition table limits "Paused by an upcoming pause" to weeks
- **Where**: data-model.md "State derivation per entry", last row (line 203: "Paused by an upcoming pause (a week
  only) | cancel the pause (FR-011) | as evaluated without it").
- **Against**: spec.md FR-007 (a commitment or per-day period is Paused "at every supplied time while that pause
  covers the date"); FR-011; data-model.md `GoalExemptions.ExemptionOf` (line 75), which applies to every date a
  pause covers, whatever its timing.
- **Reasoning**: commitments and days covered by an upcoming pause are Paused too, for example 23 to 27 November
  under AC-051's upcoming pause. Cancelling that pause returns them to Upcoming. The table limits the transition to
  weeks, so it disagrees with `ExemptionOf` and FR-007. The two would lead to different test expectations.

## Summary
0 MAJOR, 7 MINOR. The plan covers every FR, HP, SC, user story and edge case of the spec. The check orders, texts and
demo figures it restates agree with the spec and the locked AC: I recomputed `demo6`'s 42 records, 2 notes and 4
excusals, both goals' figures for every weekday, and the dashboard counts. R-01's argument that adding the enum
members keeps the locked AC code compiling holds against `TreatWarningsAsErrors` in the root
`Directory.Build.props`, which the AC project imports. The layer and work-order reasoning matches the existing code
(`EntryViews.Of`, `GoalEvaluation`, `ReportService`, `DemoAccountWriter`, `StartupTasks`, and `SmokeCheckCoverage`'s
14 entries). What remains is cleanup:
- a misnamed architecture fence, while the `IGoalStore` rule, which will fail, is not named;
- the trend target rule for fully exempt yes/no weeks;
- citations of an AC row that no longer exists, and of eight features where there are six;
- one check-order inconsistency between research and the contract;
- two UI components missing from the scope;
- one wrong row in the transition table.

None of these blocks `/speckit-tasks`, but MINOR-1, MINOR-2 and MINOR-6 should be settled before tasks are
generated, so the task list carries the right changes.
