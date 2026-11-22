# Critic pass 3: plan for 009-goal-changes

**Verdict**: APPROVE
**Date**: 2026-11-22
**Reviewed**: plan.md, research.md, data-model.md, quickstart.md, contracts/application-api.md, contracts/host.md

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 (RecordDialog target line, help texts and progress line from the latest settings) | Resolved | research.md R-07 lines 444-467: the table now covers `TargetLine`, `Progress`, `FieldLabel`, `PartialNote`, `ReduceHelp` and `NoteHelp`, each with an `EntrySettings` overload, and the pages call only those overloads. data-model.md line 268 lists all seven `RecordingText` members. contracts/host.md lines 29-34 and 95-97. `AttemptDetails.History` takes its goal-level fields from that attempt's `VersionInEffect` (research.md lines 468-470, data-model.md lines 292-294). I checked RecordingText.cs: the seven members named are every `RecordingText` member that takes a `GoalHistory` (lines 51, 102, 126, 143, 162, 221, 239). Two stale lists remain (MINOR-5). |
| MAJOR-2 (an earlier attempt's streak unit from the latest settings) | Resolved | data-model.md lines 321-324 and research.md R-07 lines 408-419: the evaluation is built with `VersionInEffect(goal, number, now)` of the evaluated attempt. `Unit`, `BuildOrReduce` and the trend derive from that target (GoalEvaluation.cs:45-46). A unit test covers both attempts of a goal restarted with another schedule kind. |
| MINOR-1 (trend rule based on a false claim about `TrendBuilder`) | Resolved | research.md lines 471-486 now describe the current behaviour correctly ("75 of 7", " min") and decide it: an entry of another measurement counts only by its result, with unit tests for three directions. data-model.md lines 325-326. plan.md line 39 and Project Structure line 198 place the change in `HT.Domain/Reports/TrendBuilder.cs`, which is where that file is. |
| MINOR-2 (`HistoryEntry.Settings` nullability, tests that build `HistoryEntry`) | Resolved | research.md lines 432-437: `EntrySettings?`, read through `GoalHistory.SettingsOf` with a goal-level fallback. data-model.md lines 264 and 266. R-12 lines 716-718 keep `CommitmentRowTests` and `HabitComponentTests` as proof of the fallback. plan.md Risks lines 418-419. |
| MINOR-3 (test doubles of the changed ports, the `RecordLogTests` throws) | Resolved | research.md R-12 lines 706-715 name `RecordLogTests`' block (RecordLogTests.cs:421-429, verified) and the four decorators. I searched the solution for other implementers of `IGoalStore` and `IRecordStore`. Besides the production stores there are only `FakeGoalStore` and `FakeRecordStore`, which are also named. plan.md Risks lines 414-417 are corrected. |
| MINOR-4 (kept-constructor claims, attempt 1's timing date) | Partly | The claims now agree with R-12 (data-model.md lines 91-97), and R-01 lines 82-88 decide that attempt 1 is timed from the goal's start date. But the `GoalTimeline` table and the "has begun" rule still time every attempt from "its own start date". Carried forward as MINOR-3. |
| MINOR-5 (Settings card start row and attempt line for restarted or never-begun attempts) | Partly | The start row now uses `AttemptHasBegun` with `AttemptStartDate` (contracts/host.md lines 35-37). An attempt ended before its start date never begins (research.md lines 159-163). The attempt line has an "ended before it began" form (host.md lines 27-28). The Settings card of an ended goal that never began still says "Starts". Carried forward as MINOR-1. |
| MINOR-6 (index slips: Summary and Structure, six demo tasks, the `DetailsOf` fact, recent-notes bounds, SC-001) | Resolved | plan.md Summary lines 34-48 and Project Structure lines 189-241 now list the members and types. Risks lines 455-456 say five demo tasks (StartupTasks.cs:50-53 plus the new one). research.md line 77 quotes `goal.Versions[0]` (GoalService.cs:272, verified). research.md lines 329-330 and data-model.md line 270 cover an attempt with no entry. SC-001 is plan.md Work order step 6 lines 352-354, R-12 lines 746-747 and quickstart step 5. |

No older note (pass 1) has regressed. The calendar across schedule kinds, the time-aware name, records and notes outside every attempt, the foreign-key delete paths, the architecture rules, ADR 0027, the `GoalEnded`/`Ended` text pairing (data-model.md lines 202-203; contracts/application-api.md lines 205-218) and the overview's earlier Pending entries all still hold. The demo figures still check out against FR-031: 74 records (27 + 24 + 23), 6 versions, 2 endings, "52% (11 of 21)" and 4 Pending.

## Notes

### MINOR-1: The Settings card of an ended goal that never began says "Starts {date}", and the head's extra state isn't recorded
- **Where**: contracts/host.md lines 35-37 (start row: "Started" or "Starts" from `AttemptHasBegun` with `AttemptStartDate`). data-model.md line 243 (`GoalDetails.AttemptHasBegun`). There, the sentence "An ended latest attempt with no date shows 'Attempt n · ended before it began' instead" applies to the attempt line only. plan.md "Spec discrepancies" (lines 383-403).
- **Against**: locked AC-035 (feature lines 385-394: attempt 1 from 23 Nov, ended 17 Nov). research.md lines 159-163 (such an attempt "never begins, whatever the time"). HP-001, which defines the attempt line only as "Attempt 2 · started 18 Nov" or "· starts 23 Nov". GoalSettingsCard.razor:36.
- **Continues**: MINOR-5 of pass 2.
- **Reasoning**: For AC-035's goal, `AttemptHasBegun` is false for good. So the Settings card shows "Starts 23 Nov 2026" under the Notice "Ended 17 Nov · No longer relevant", which announces a start that can never happen. The plan gives the attempt line a third form, "ended before it began", which HP-001 doesn't have. That's a reasonable reading of HP-001, but it isn't recorded under "Spec discrepancies", where the plan records its other readings. The Settings card row gets no matching rule for this state.

### MINOR-2: The rule for when an ended attempt has begun rests on a false premise, and it differs from the spec in one reachable case
- **Where**: research.md R-02 lines 159-163: "An ended attempt has begun when its start date is on or before its ending date. The ending date is the goal's current date at the ending, so the start day had begun by then." data-model.md line 150. `CurrentDate` uses the same rule (research.md lines 166-167, data-model.md line 136).
- **Against**: spec "Terms used below" (spec.md lines 556-565). "Current date" uses "the time zone of the last version of the latest attempt that has begun". A waiting attempt's date "has not begun ... until its day begins in that attempt's own time zone (which can be later when the new attempt's time zone is west of the earlier one's)". The ending date is "the goal's current date ... (for a goal that has not started, the account's today)" (spec.md line 549). FR-002 explains "has not begun" as "no date of it has begun".
- **Continues**: new.
- **Reasoning**: The premise holds only when the ending date is the ended attempt's own current date. When a waiting attempt is ended, E is the goal's current date taken from an earlier attempt's zone (spec.md lines 559-561). Example: attempt 1's last version is in Kyiv. The account then moves to New York, and attempt 2 starts on 23 Nov in New York's zone (R-06: "the account's current zone"). At 2026-11-22T23:30Z the goal's current date is 23 Nov, from Kyiv. The account's today in New York is 22 Nov. Ending now gives E = 23 Nov = attempt 2's start date. By the plan's rule attempt 2 "has begun", although its start day has not begun in its own zone and it has no date (L = 22 Nov). Afterwards, `CurrentDate` takes the zone of attempt 2's last version (New York, 22 Nov) instead of attempt 1's (Kyiv, 23 Nov). The Settings card's start row says "Started 23 Nov 2026" (contracts/host.md line 37). This case is reachable through the public use cases and isn't covered by the R-12 tests listed at lines 692-694.

### MINOR-3: Attempt 1's timing date (R-01) isn't carried into the `GoalTimeline` rules or the "has begun" rule
- **Where**: research.md R-01 lines 82-88 (attempt 1 is "timed from the goal's start date", also when its attempt start date is later, so that each reader "gives exactly today's date"). Against that:
  - data-model.md line 136 (`CurrentDate`: "Each attempt's candidates start at that attempt's own start date, not at `goal.StartDate`");
  - line 137 (`TryCalendar` "from its start date");
  - line 139 (`EntriesThrough`: "the Monday on or before the latest attempt's start date");
  - line 140 (`NextCommitment`, `FirstWeekStart` "from its own start date");
  - line 150 and research.md line 159 (a not-ended attempt "has begun once the day of its start date has begun").
- **Against**: Goal.cs:40-43, which accepts `attempt.StartDate` later than `startDate`. GoalTimeline.cs:63 (slice 4's candidate filter is `candidate >= goal.StartDate`). The planned unit test "a one-attempt goal whose attempt 1 starts after the goal's start date, timed as in slice 4" (research.md line 695).
- **Continues**: MINOR-4 of pass 2.
- **Reasoning**: research.md now says which date wins, but data-model.md, which `/speckit-tasks` and the implementer follow, still says "its own start date" in every row. Take attempt 1 with `attempt.StartDate` later than the goal's start date, read between the two dates. Under the data model's begun rule the latest attempt hasn't begun, so `CurrentDate` is null. Slice 4 gives a date there. That breaks the equivalence R-01 claims, and the planned test would fail. No store writes such a goal (R-01), so the effect is limited to the constructor's accepted inputs. Still, the two artifacts disagree on a rule they each state as normative.

### MINOR-4: Quickstart step 3 can't be performed on the Edit settings page as the plan defines it
- **Where**: quickstart.md new-user step 3 (line 86): "Try to change the schedule to '3 times a week' and check the refusal under the schedule field". The goal is the one created in step 1, "on all seven days" (fixed days).
- **Against**: contracts/host.md line 59 ("the schedule choice limited to the goal's kind"), research.md R-11 line 649 ("limits the schedule choice to the goal's kind"), spec HP-002 ("The schedule choice is limited to the goal's kind [D-1]").
- **Continues**: new.
- **Reasoning**: The edit form doesn't offer another schedule kind for a fixed-days goal, so a user can't enter "3 times a week" and the `ScheduleKindChanged` refusal can't be produced from the page. The validation step describes an interaction that the contracted page rules out. It verifies nothing as written.

### MINOR-5: A few lists and claims are stale or incomplete
- **Where and against**:
  - contracts/application-api.md line 159 lists only `RecordPreview.Progress`, `RecordingText.Recorded` and `RecordingText.FieldLabel` as getting `EntrySettings` overloads. data-model.md line 268 and research.md lines 459-463 give all seven `RecordingText` members plus `RecordPreview`.
  - plan.md Risks lines 431-433 name only `RecordingText.Recorded` and `FieldLabel`.
  - research.md lines 444-446 say the per-entry rule "covers every member that takes a `GoalHistory` for one entry's sake, not a list of examples". `ReportText.ProgressText(GoalHistory, HistoryEntry)` (ReportText.cs:87) is such a member, called with the goal-level history by CommitmentRow.razor:102 and GoalItem.razor:99, and it isn't mentioned. It is called only for Due entries. Those always belong to the latest attempt and use the version of the current date, so today it gives the same text. The completeness claim is still literally false.
  - The R-07 table (line 456), plan.md Summary line 62 and Scale/Scope lines 141-142 say `RecentNotesCard` takes "each row's own `Settings`". But `RecentNote` carries no settings (RecentNotesCard.razor:5-9 passes the goal's schedule kind from the history), and data-model.md adds none. Recent notes cover only the latest attempt (FR-016), so the goal-level kind is correct, but the claimed change has no data behind it. plan.md Constitution IV (line 160) leaves `RecentNotesCard` out of the components "reused with one change".
  - research.md line 715 and plan.md line 416 list "the bUnit page fakes" as port implementers. The bUnit tests have none of their own. They register `HT.UnitTests.Fakes.FakeGoalStore` and `FakeRecordStore` (tests/HT.UiTests/Components/Pages/PageServices.cs:59-77).
  - SC-008 (spec.md lines 999-1000, accessibility of every page and dialog) is named in no artifact under review. Constitution IV's row and HP-009 cover its substance. Slice 7's plan likewise didn't name its accessibility criterion.
- **Continues**: MAJOR-1 of pass 2, for the first two items. The rest are new.
- **Reasoning**: None of these changes behaviour. But `/speckit-tasks` is told to follow the plan and the contracts as the index (plan.md line 272), and two of those indexes still describe the narrower pass 2 design.

## Summary

0 MAJOR, 5 MINOR. Previous notes (pass 2): 6 Resolved, 2 Partly, 0 Unresolved. No pass 1 note has regressed.

This revision closes both remaining major gaps:
- every view of one entry now reads that entry's own settings through `GoalHistory.SettingsOf` and the `EntrySettings` overloads, and an attempt's History takes that attempt's goal-level settings;
- each evaluation is built with the evaluated attempt's own version in effect, so streak units and Build or Reduce are right for earlier attempts.

The trend decision, the nullable `Settings`, the named test doubles and the `RecordLogTests` change are all correct against the code. What remains is edge-case precision:
- the Settings card of an ended goal that never began;
- a "has begun" rule whose premise fails when a waiting attempt in a west zone is ended on its start date;
- attempt 1's timing date not carried into data-model.md's table;
- a quickstart step the edit form rules out;
- a few stale index lists.

None of them blocks `/speckit-tasks`. They should be cleaned up in the artifacts or settled when the tasks are written.
