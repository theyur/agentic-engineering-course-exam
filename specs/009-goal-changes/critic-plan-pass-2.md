# Critic pass 2: plan for 009-goal-changes

**Verdict**: REVISE
**Date**: 2026-11-22
**Reviewed**: plan.md, research.md, data-model.md, quickstart.md, contracts/application-api.md, contracts/host.md (and the docs/roadmap.md row)

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 (goal-level settings used for every entry) | Partly | `EntrySettings` on every `HistoryEntry` (data-model.md line 261-262), `RecordPreview.Progress(HistoryEntry, …)`, `RecordingText.Recorded(EntrySettings, …)` and `FieldLabel(EntrySettings)` (research.md R-07 lines 409-420). But five other `RecordingText` helpers that the RecordDialog reads from the goal-level `GoalHistory` get no overload, and the goal-level settings of `AttemptDetails.History` are those of the latest attempt. Carried forward as MAJOR-1. |
| MAJOR-2 (records and notes outside every attempt throw) | Resolved | research.md R-07 lines 430-440: `NothingScheduled` for change and delete, `StoredEntryOf`/`NoteEntryOf` removed. R-05 lines 309-314: the overload with `from` and `through`. contracts/application-api.md lines 140, 151 and 154. The integration test that pins the old throws isn't named (see MINOR-3). |
| MAJOR-3 (calendars across attempts of different kinds) | Resolved | research.md R-02 lines 122-134 defines both lists, the `Kind` rule and the merge in `TryEntries`. data-model.md lines 135 and 166-170. contracts/application-api.md line 149. This fits `GoalService.GetCalendarAsync` (GoalService.cs:259-264), the only consumer of `TryCalendar` outside `GoalTimeline`, and AC-096 to AC-098 (feature lines 1062-1081). |
| MAJOR-4 (`GoalText.Name` takes no time) | Resolved | research.md R-08 lines 489-516: the code fact is corrected, and `Name(goal, habitName, instant)` goes through `VersionInEffect`. The listed callers match the code: GoalListItems.cs:21, GoalService.cs:279, CatalogService.cs:119 and GoalEvaluation.cs:162. data-model.md lines 172-182. |
| MINOR-1 (whether a waiting attempt has begun) | Resolved | `GoalTimeline.LatestAttemptHasBegun`, `GoalDetails.AttemptHasBegun` and the new `GoalListItem.StartDate` rule (data-model.md lines 147, 240, 250-252). contracts/host.md line 25-26. |
| MINOR-2 (`Goal.StartDate` readers, kept constructor) | Partly | The readers are now listed and switched (research.md R-01 lines 70-81; plan.md lines 413-415). The kept constructor still accepts an attempt that begins after the goal's start date, which is the open combination this note raised. Carried forward as MINOR-4. |
| MINOR-3 (two cascade paths) | Resolved | The new foreign key has no delete action (data-model.md line 18; research.md R-05 lines 278-284), with an integration test that deletes a goal with two attempts (R-12 line 660). |
| MINOR-4 (existing Domain tests, `FieldOf`) | Resolved | research.md R-12 lines 640-646; data-model.md line 118-119; plan.md Work order step 2 lines 282-286. |
| MINOR-5 (architecture rules) | Resolved | plan.md Constitution Check V line 152 and research.md R-12 lines 677-689 name all four missing rules. |
| MINOR-6 ("no date") | Resolved | The author kept "no commitment or period" and recorded it as plan.md "Spec discrepancies" 3 (lines 373-378). The rationale is sound. FR-008 states "last date before the start date" only as a case that leads to "no commitment or period", and FR-013 gives the per-week attempt no week. The spec's "Last version" term ("its first version when it has no date") still holds, because `EndingOf` keeps only the first version in that case. Slices 7 and 8 already used "Spec discrepancies" sections the same way (specs/007 plan line 271, specs/008 plan line 299). |
| MINOR-7 (`GoalEnded` with opposite meanings) | Resolved | contracts/application-api.md Texts lines 205-218 and data-model.md lines 199-200 now pair each outcome with the text of the same name. |
| MINOR-8 (earlier Pending entries in the overview) | Resolved | research.md R-07 lines 441-449; data-model.md lines 313-316 and 329; contracts/application-api.md line 156. |
| MINOR-9 (ADR test) | Resolved | plan.md Decision Discipline line 154 applies the three tests and plans ADR 0027 in Work order step 1 (lines 266-267). The number 0027 is free (`docs/adr` ends at 0026). |
| MINOR-10 (plan inconsistencies, roadmap) | Resolved | Constitution IV (line 151) now says `HistoryCard` and the dialogs change. The roadmap row is `planned` (docs/roadmap.md line 38), and the status after each step is listed (plan.md lines 269-275). |

No older note has regressed.

## Notes

### MAJOR-1: The RecordDialog's target line, help texts and progress line still come from the latest settings
- **Where**: research.md R-07, the table of places that read entry settings (lines 409-424), which gives overloads only to `RecordPreview.Progress`, `RecordingText.Recorded` and `RecordingText.FieldLabel`. data-model.md lines 263-265 (`GoalHistory` "shape unchanged"; the new overloads are only `Progress`, `Recorded` and `FieldLabel`). data-model.md lines 285-290 (`AttemptDetails.History` is a `GoalHistory`, and its goal-level settings are not defined). contracts/host.md lines 27-31, 91-93 and 123-126. plan.md Risks lines 401-409.
- **Against**: spec HP-007 and HP-014. The plan's own host contract says that an earlier attempt's row "is shown and recorded with its own settings (AC-080)" and that the attempt view's dialogs use "the attempt's own, never the latest attempt's" settings. FR-010 and FR-014 (records of earlier attempts keep working as for any date). Locked AC-020 (feature lines 232-244) and AC-080 (lines 865-871).
- **Continues**: MAJOR-1 of pass 1.
- **Reasoning**: The RecordDialog reads five more helpers from the goal-level `Recordable` (RecordDialog.razor):
  - line 36: `RecordingText.TargetLine(Recordable)`;
  - line 77: `RecordingText.Progress(Recordable, progress)`;
  - line 83: `RecordingText.PartialNote(Recordable)`;
  - line 87: `RecordingText.ReduceHelp(Recordable)`;
  - line 93: `RecordingText.NoteHelp(Recordable)`.

  Each reads `history.Target`, `BuildOrReduce`, `Measurement`, `CountUnit`, `GoalScheduleKind` or `Period` (RecordingText.cs:51-73, 143-175, 221-250). The artifacts give them no `EntrySettings` overload and don't list them among the places of R-07 or the plan's "Meaning drift" risk.

  The openers pass the goal-level history: `TodayPanel` passes `recordEntry.Goal`, `GoalsList` passes `dialogGoal.Recordable` and `GoalDetailBody` passes `dialogHistory` (TodayPanel.razor:59, GoalsList.razor:65, GoalDetailBody.razor:135). data-model.md line 324 keeps `TodayEntry.Goal` as "the goal's `GoalHistory`".

  In AC-080, attempt 2 is "Reduce, duration 30 minutes, 1 time per day". The dialog for attempt 1's Pending 15 Nov (Build, duration, commitment-based) would then show:
  - the sub line "Reduce · at most 30 min a day";
  - the Reduce help "At most 30 min. Enter 0 if you had none …";
  - no partial-time sentence;
  - the per-day note help ("This note is for the whole day");
  - a progress line in the form "… of at most … min".

  After AC-020 example 02 (Build changed to Reduce), the dialog for 11 Nov would show the Reduce help on a Build date.

  The attempt view has the same problem. `AttemptDetails.History` is a `GoalHistory`, and `EntryViews.HistoryOf` now takes its settings from `VersionInEffect`, the latest attempt's version (research.md R-01 lines 75-76, R-02 lines 146-147). So an earlier attempt's dialog shows the latest attempt's target line and help texts.

  The pages may not build these texts themselves (HP-008, constitution IV "Texts"). So the fix of pass 1's MAJOR-1 still leaves wrong texts in the dialog for exactly the AC-020 and AC-080 cases the plan's component tests target (Work order step 6, lines 332-333).

### MAJOR-2: An earlier attempt's figures take their streak unit from the latest attempt's settings
- **Where**: data-model.md "Internal: `GoalEvaluation` (changed)", lines 309-317: the new attempt overload "evaluates the named attempt", while "`LatestTarget` comes from `VersionInEffect`", with no distinction between the overloads. data-model.md lines 280-283 (`AttemptView.Stats`: "consistency, current and longest streak with unit, over that attempt"). contracts/application-api.md lines 110-111 (`GetAttemptsAsync` through `GoalEvaluation.OfAsync(…, number, …)`).
- **Against**: spec FR-014 ("its consistency, current streak and longest streak with their unit, computed over that attempt"), FR-006 (streaks "in the unit of the attempt's schedule kind"), FR-012 and US3-AS5 (a new attempt may change the schedule kind, and its unit then differs from the earlier one's). HP-014 (the attempt view's GoalStats).
- **Continues**: new.
- **Reasoning**: `GoalEvaluation` derives `Unit = StreakUnits.Of(latest)` and `BuildOrReduce = latest.BuildOrReduce` from the `latest` target it is built with (GoalEvaluation.cs:45-46). Its trend kind and span also come from that target (`TrendBuilder.Of(latest, …)`, line 161). `VersionInEffect` gives the latest attempt's version (research.md R-02 lines 141-148). The data model sources `LatestTarget` from it for the evaluation as a whole and doesn't say that the attempt overload uses the evaluated attempt's own version.

  Take US3-AS5's goal: a daily commitment-based attempt 1, restarted as 3 times per week. Following the data model, attempt 1's `AttemptView.Stats` gives its streaks in "weeks" instead of "commitments", on the Earlier attempts card and in the attempt view. A Build/Reduce change across attempts likewise gives that attempt's evaluation the wrong `BuildOrReduce`. The locked AC-056 and AC-057 don't catch this, because both of their attempts are commitment-based. It is still a wrong value in an AC-surface read that FR-014 requires.

### MINOR-1: The trend rule for a measurement change rests on a false claim about `TrendBuilder`
- **Where**: research.md R-07, "The trend" (lines 425-429): "A change from duration to yes/no in a commitment-based attempt therefore switches it to commitment weeks. Earlier bars keep their own progress. This is the existing `TrendBuilder.Of(latest, …)` rule, now documented rather than assumed away."
- **Against**: TrendBuilder.cs:88-90 and 110-146. Slice 6 FR-006 (a commitment-based yes/no goal's week bar is "the week's 'Yes' records" against the week's commitments).
- **Continues**: new (pass 1 MAJOR-1 named the trend's kind as an affected place).
- **Reasoning**: When the latest target is a commitment-based yes/no target, `TrendBuilder` groups every begun entry into weeks (`CommitmentWeeks`). Each week's value is the sum of `Evaluator.ProgressOf(item.Target, item.Records).Value` (line 135). For an earlier duration commitment that value is the recorded minutes, so a week before the change gets a bar like "75 of 7". Earlier bars don't keep their own progress here. Only the `BarOf` branch keeps it. In the opposite direction (yes/no to duration), earlier yes/no bars land in a `Minutes` trend, whose texts add " min" (ReportText.cs:270). The decision is recorded as the existing behaviour, but the research describes that behaviour wrongly. So the trend's values for a change of measurement are neither specified correctly nor tested.

### MINOR-2: `HistoryEntry.Settings` can't be both non-null and not `required`, and the existing tests that build `HistoryEntry` aren't named
- **Where**: data-model.md line 261 (`Settings` (`EntrySettings`, "always set by `EntryViews.Of`")), data-model.md line 6-7 and plan.md "Constraints" lines 110-115 ("init-only properties that are not `required`"). research.md R-12 "Existing tests this slice changes on purpose" (lines 639-649).
- **Against**: CLAUDE.md (`TreatWarningsAsErrors`, nullable on). HistoryEntry.cs:9-51 (a record with `required` members and no constructor). tests/HT.UiTests/Components/Shared/CommitmentRowTests.cs:51-68 and HabitComponentTests.cs:319 build `HistoryEntry` and `GoalHistory` objects without settings.
- **Continues**: new.
- **Reasoning**: A non-nullable, non-`required` init property with no initializer gives CS8618, which fails the build. Making it `required` breaks the existing object initializers and goes against the plan's own constraint. Making it nullable means the components need a fallback that the artifacts don't define. Either way, `CommitmentRowTests` and `HabitComponentTests` build entries without `Settings`. Once `CommitmentRow`, the RecordDialog and `GoalItem` read `Entry.Settings`, those tests break, and R-12 doesn't list them among the tests changed on purpose.

### MINOR-3: Test doubles of the changed ports and the test that pins the removed throws aren't acknowledged
- **Where**: plan.md Risks, "New port members break only an implementer outside the solution" (lines 393-394). research.md R-12 line 647 ("The slice 5 tests that expect `StoredEntryOf` or `NoteEntryOf` to throw, if any"). plan.md Work order step 3 ("the fakes' new members", line 302).
- **Against**: test-local implementations of the ports, in the solution:
  - `RacingGoalStore : IGoalStore` (tests/HT.IntegrationTests/Goals/TimeZoneChangeTests.cs:347);
  - `BeforeFirstCreate : IGoalStore` (tests/HT.UnitTests/Application/Goals/GoalWorld.cs:80);
  - `FaultingRecordStore : IRecordStore`, twice (RecordLogTests.cs:587, ExemptionLogTests.cs:613).

  tests/HT.IntegrationTests/Records/RecordLogTests.cs:421-429 asserts that `ChangeRecordAsync`, `DeleteRecordAsync` and `GetRecentNotesAsync` fail with "A stored record's date belongs to no commitment or period of its goal." and "A stored note's key belongs to no commitment or period of its goal.". The precedent is specs/006-note-editing/plan.md lines 263-266, which named `RecordLogTests`' wrapper.
- **Continues**: new (it extends pass 1 MINOR-4's class to the Application and integration tests).
- **Reasoning**: The four new `IGoalStore` members and the new `IRecordStore` overload don't compile in these four decorators until each gets the members. R-07 removes the two throws, so the `RecordLogTests` assertions fail by design. The plan's risk statement is factually wrong, and R-12's "if any" leaves out a test that exists.

### MINOR-4: The kept constructor's equivalence claims contradict each other and leave attempt 1's timing date open
- **Where**: data-model.md "Kept constructor" (lines 91-95): "Only the rule 'only the time zone differs' is replaced by 'the same schedule kind' … so no combination that slice 4 accepts is now refused, and none that it refuses is now accepted." data-model.md lines 82-85 and research.md R-01 lines 63-64 and 80-81 ("attempt 1 starts on the goal's start date").
- **Against**: research.md R-12 lines 640-643, which says that the new rule now accepts a version change from 1 to 2 times per day, which slice 4 refused (GoalTests.cs:73-76). Goal.cs:40-43 refuses only an attempt that begins *before* the start date.
- **Continues**: MINOR-2 of pass 1.
- **Reasoning**: The claim that no combination slice 4 refuses is now accepted is false, and R-12 says so. Both constructors still accept an attempt 1 whose start date is later than its first version's date ("the attempt does not begin before it"). R-02 now times each attempt "from its own start date" (line 107), while slice 4 timed the goal from `goal.StartDate`. For such a goal the two differ. The equivalence that R-01 asserts ("For a goal with one attempt every one of these gives today's date") then fails, and the artifacts don't say which date wins or whether that combination is now refused.

### MINOR-5: The Settings card's start row and the attempt line aren't defined for restarted attempts or attempts that never began
- **Where**: plan.md Scale/Scope (line 129: `GoalSettingsCard` changes only by "From …"). contracts/host.md lines 25-26 and 32-33. data-model.md lines 147 and 240 (`LatestAttemptHasBegun`: "the day of the latest attempt's start date has begun", with no regard to an ending).
- **Against**: design/pages/PageGoalDetail.html lines 14 and 41: the target state shows "Attempt 2 · started 14 Jul" and the Settings row "Started 14 Jul 2026", the attempt's date. GoalSettingsCard.razor:36 shows `Details.HasStarted ? "Started" : "Starts"` with `Details.StartDate`, which stays attempt 1's date (research.md R-01 line 77). Spec "A goal that has not started" ("none of its attempts has begun") and HP-001.
- **Continues**: new.
- **Reasoning**: For a restarted goal, the Settings card keeps showing attempt 1's date, which differs from the attempt line and from the design. In AC-035's case (attempt 1 from 23 Nov, ended 17 Nov, attempt 2 from 18 Nov) the card shows "Started 23 Nov 2026" on 19 Nov. And for an attempt that ended before it began, `LatestAttemptHasBegun` turns true once its start date passes. The ended goal's page then shows "Attempt 1 · started 23 Nov" under the Notice "Ended 17 Nov". That also moves `CurrentDate`'s "latest attempt that has begun" zone to an attempt that never had a date. The artifacts don't say whether such an attempt "has begun".

### MINOR-6: Small inconsistencies and factual slips in the plan and research
- **Where and against**:
  - plan.md Summary (lines 34-36) and Project Structure (lines 180-224) omit several members and types that the data model adds or changes:
    - `GoalTimeline.VersionInEffect`, `LatestAttemptHasBegun` and `NewAttemptEarliestStart`;
    - `GoalSettingsChange` and `GoalEndingEffect`;
    - `GoalText.Name(goal, habitName, instant)`;
    - `EntrySettings`, `RecordPreview` and `RecordingText`;
    - `CatalogService`;
    - `GoalSettingsView`;
    - the `RecordDialog`, `NoteDialog` and `RecentNotesCard` changes, which the plan's own Scale/Scope (line 132-133) lists.
  - plan.md Risks line 431, "Only the host runs all six demo tasks": the demo tasks are demo loading, demo history, demo report, demo pause and demo goal-change loading, five in all (StartupTasks.cs:50-53 plus the new step; AC-095).
  - research.md R-01 line 76 says that `GoalService.DetailsOf` uses `VersionOf(goal, current ?? goal.StartDate)` today. It uses `goal.Versions[0]` when there is no current date (GoalService.cs:272).
  - contracts/application-api.md line 154 and data-model.md line 267 define the recent-notes bounds by "the latest attempt's first entry". They don't say what is read when the latest attempt has no entry (an attempt with no date).
  - SC-001 (the action counts, including "starts a new attempt from the Ended tab in two") has no named test or quickstart step. Quickstart step 5 starts from the goal page's Notice.
- **Continues**: new.
- **Reasoning**: Each slip is small. Together they make the plan's own summary and structure an unreliable index for `/speckit-tasks`, which is asked to follow them (Work order, line 255).

## Summary

2 MAJOR, 6 MINOR. Previous notes: 12 Resolved (MINOR-6 by a sound narrowing), 2 Partly, 0 Unresolved.

The revision fixed most of pass 1:
- the calendar across schedule kinds;
- the time-aware name;
- records and notes outside every attempt;
- the waiting attempt's begun flag;
- the foreign-key delete paths;
- the architecture rules;
- the ADR;
- the overview's earlier Pending entries.

The demo figures, the counts and the check orders still match the spec and the locked criteria.

Two gaps remain in how per-entry and per-attempt settings reach the views:
- The RecordDialog and the attempt view still take their target line, help texts and progress line from a goal-level `GoalHistory` that now holds the latest attempt's settings (MAJOR-1).
- An earlier attempt's evaluation would take its streak unit, Build or Reduce and trend from `VersionInEffect` instead of its own settings (MAJOR-2).

The minor notes are a wrong code claim about the trend, the `HistoryEntry.Settings` initialization, test doubles and tests that will break without being named, the kept constructor's claims, the Settings card's start row, and small index slips. These should be settled before `/speckit-tasks`.
