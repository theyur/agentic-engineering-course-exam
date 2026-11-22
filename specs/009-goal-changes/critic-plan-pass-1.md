# Critic pass 1: plan for 009-goal-changes

**Verdict**: REVISE
**Date**: 2026-11-22
**Reviewed**: plan.md, research.md, data-model.md, quickstart.md, contracts/application-api.md, contracts/host.md

## Notes

### MAJOR-1: The views that carry one settings version for the whole goal are not handled, so past dates and earlier attempts are recorded and shown with the wrong settings
- **Where**: plan.md "Risks", "Meaning drift of existing members" (lines 366-371), which names only `Goal.Target`, `GoalVersionChange.ApplyTo` and excuse step 4 as places that relied on "only the time zone differs". data-model.md "Application: HT.Application.Records" (line 233: `HistoryEntry` gains only `AttemptName` and `AttemptBuildOrReduce`) and "Changed views" (line 285: `TodayEntry` changes only `GoalName`, `BuildOrReduce` and `GoalEnded`). data-model.md line 255 / contracts/application-api.md lines 111-119 (`AttemptDetails.History` is a `GoalHistory`). contracts/host.md "Attempt view" and "Signed-in Home".
- **Against**: spec FR-005 (dates already begun keep the version of their date, corrections included), FR-010 and FR-014 (records of ended and earlier attempts "MUST keep working as for any goal" / "as for any date"), the edge case "Build to Reduce, or a change of measurement: allowed within one schedule kind ... Records entered under the old measurement stay with their dates", FR-012 (a new attempt can have any schedule kind), HP-007, HP-014. Locked AC-020 (feature lines 232-244: duration changed to count, or Build changed to Reduce, inside one attempt) and AC-080 (lines 865-871: attempt 2 is Reduce, 1 time per day, while attempt 1's Pending entry is listed).
- **Reasoning**: Today the pages take the target, measurement, unit and schedule kind from one goal-level `GoalHistory`, built from the version of the goal's current date (`EntryViews.HistoryOf`, EntryViews.cs:59-78). Its doc comment says "In this slice every version has the same target (FR-015)" (GoalHistory.cs:38-42). These all read it:
  - the RecordDialog's field and its live preview (`RecordFields Measurement="Recordable.Measurement"`, RecordDialog.razor:74; `RecordPreview.Progress` → `Evaluator.ProgressOf(history.Target, …)`, RecordPreview.cs:32-37);
  - the History card's Recorded cell (`RecordingText.Recorded` formats amounts with `history.Measurement` and `history.CountUnit`, RecordingText.cs:102-108);
  - the History card's and NoteDialog's state words (`History.GoalScheduleKind`, `History.Period`, HistoryCard.razor:75-79 and 232);
  - each Today row's `Goal = evaluation.Recordable` and `ScheduleText` from `LatestTarget` (ReportService.cs:223-231);
  - the trend's kind and span, which come from the latest target only, while each bar keeps its own target (TrendBuilder.cs:86-94).

  After AC-020's change, the goal page's History would show 11 November's "12 min" as "12 pages", and the RecordDialog for that row would offer a count field and judge the preview against the new target. In AC-080, the Home's row for attempt 1's Pending 15 November (Build, duration, commitment-based) would open a dialog built from attempt 2 (Reduce, per day). The attempt view has the same problem, because its `GoalHistory` would carry the latest attempt's settings. The plan doesn't say that these views need per-entry or per-attempt settings, and the data model adds none. So the "record and note actions" of HP-014 and HP-007 can't work correctly for these cases, and past dates are shown under the wrong measurement.

### MAJOR-2: Records and notes outside every attempt make existing recording paths throw, and the plan doesn't cover this
- **Where**: research.md R-07, "Recent notes read through the new first-date overload from the latest attempt's start date" (line 346). data-model.md line 234 and contracts/application-api.md line 140 (`ReadRecentNoteKeysAsync(owner, goal, from, count, ct)`, with a lower bound only). contracts/application-api.md "Changed reads" (lines 151 and 154).
- **Against**: spec FR-009 ("Records and notes of dates after the last date are kept as stored facts: they are not shown or counted while no attempt contains their date"), FR-016, FR-027, US2-AS11 (the records of the week in progress are kept), ADR 0025 Consequences ("A commitment recorded on the ending date is not counted ... the record is kept as a stored fact, outside every attempt"), locked AC-034 and AC-039 (records on the ending date or in the week in progress).
- **Reasoning**: `GetRecentNotesAsync` maps every returned note key through `NoteEntryOf`, which throws `InvalidOperationException` when the key belongs to no commitment or period (RecordingService.cs:304-311 and 447-449). Take an ended goal with a note on the ending date E, or a per-week goal with a note on the Monday of the week in progress. Both are possible, because the user could write the note before ending. The new overload, read "from the latest attempt's start date", returns that key, because E is after the start date. The call then throws, and the goal page of the ended goal loads its recent notes on every render (GoalDetailBody.razor:272-273). In the same way, `ChangeRecordAsync` and `DeleteRecordAsync` call `StoredEntryOf`, which throws for a record whose date is outside every attempt (RecordingService.cs:162 and 216, 438-440). That happens with the kept records of AC-034 and AC-039 when a page still holds their identifiers. The contract says only "Work for every attempt's dates". The plan gives no outcome for dates outside every attempt, gives the overload no upper bound, and lists neither the comments nor the code paths that rely on "a stored record's date always belongs to an entry".

### MAJOR-3: Calendars and entry lists across attempts with different schedule kinds aren't defined, and the locked types can hold only one kind
- **Where**: research.md R-02, "The existing `Goal`-taking members keep their signatures and now work through the goal's attempts" (lines 81-87). data-model.md `GoalTimeline` table, `TryCalendar`, `TryEntries`, `EntryOf` (line 129). contracts/application-api.md "Changed reads": `GetCalendarAsync` (line 149) and `GetHistoryAsync` (line 152).
- **Against**: spec FR-012 and D-1 (a new attempt may have any schedule kind; US3-AS5 restarts a daily goal as 3 times a week), FR-013 and FR-016 (each attempt has its own commitments or periods, and the calendar and History give none outside an attempt), FR-024 (the locked AC code of F004 uses `GoalCalendarView`).
- **Reasoning**: `EvaluationCalendar` has one `Kind`, "the other list is empty" (EvaluationCalendar.cs:4-23). The public positional record `GoalCalendarView(Kind, Commitments, Periods)` is described the same way (GoalCalendarView.cs:5-10), and its shape can't change under the plan's own locked-AC constraint. `TryEntries` keeps only the list that matches `calendar.Kind` (GoalTimeline.cs:135-137), and `GetHistoryAsync`, `EntryOf`, `GetRecordableAsync` and the evaluation all build on it. The goal page reads the History from the current date − 365 to the current date (GoalDetailBody.razor:265-266). After a restart from commitment-based to per-week, that range spans both kinds. The artifacts don't say which `Kind` such a calendar gets, whether both lists are filled, or how `TryEntries` keeps the latest attempt's periods when the first attempt in the range is commitment-based. The research note "every member gives exactly what it gives today" covers only one-attempt goals. This is an open design point in the core timeline rule (R-02), and it blocks a correct implementation of FR-013 and FR-016 for US3-AS5.

### MAJOR-4: `GoalText.Name` takes no time, and the research misstates the current code
- **Where**: research.md R-08 (lines 372-376): "`GoalText.Name` keeps naming the goal by its own name or by its habit's current name and the target text of the settings in effect on its current date". data-model.md "GoalText (changed)" (lines 160-161): "`Name(goal, habitName)`: unchanged rule."
- **Against**: spec US1-AS1 and AS2 and FR-004. Locked AC-001 and AC-002 ("A change applies from the next date and keeps the name until then", "The changed settings and name take effect at midnight"): the name is "Reading, 15 min daily" at 11 Nov 10:00 and "Reading, 20 min daily" from 12 Nov 00:00.
- **Reasoning**: The current method is `Name(Goal goal, string habitName)` and returns `habitName + ", " + TargetText(goal.Target)`, the first version's target (GoalText.cs:56-61; `Goal.Target => Versions[0].Settings.Target`, Goal.cs:74). It never used "the settings in effect on its current date", so the research's code fact is wrong. Once a settings change exists, the derived name depends on the instant. A signature with no instant or date can't give "15 min" before the boundary and "20 min" after it. The data model keeps the signature and the "unchanged rule", lists no new overload, and names no new parameter for its callers (`GoalEvaluation.OfAsync`, `GoalListItems.Of`, `GoalService.DetailsOf`, and the name sort of `GoalService.ListAsync`). An implementation that follows the data model fails AC-001 and AC-002.

### MINOR-1: Nothing tells the pages whether a waiting new attempt has begun
- **Where**: contracts/host.md "Goal page", attempt line ("· starts {23 Nov}" while the latest attempt has not begun). quickstart.md step 3.4 ("Attempt 2 · starts {tomorrow}"). data-model.md "Changed views" for `GoalDetails` and `GoalListItem`.
- **Against**: spec HP-001 ("Attempt 2 · started 18 Nov", or "· starts 23 Nov"). The slice 4 design rule: the page says "started" or "starts" without reading a clock (`GoalDetails.HasStarted` doc, GoalDetails.cs:52-56).
- **Reasoning**: `HasStarted` means "the goal has a current date". Under the extended current-date rule, a goal whose attempt 1 began and whose attempt 2 waits has a current date, so the page would say "started". The data model adds no field that says whether the latest attempt has begun. `GoalListItem.StartDate` is likewise attempt 1's date, and only while the goal has no current date (GoalListItems.cs:29). So the Goals list can't show when a waiting new attempt starts, and in AC-035's case it would show attempt 1's date, which is later than attempt 2's.

### MINOR-2: `Goal.StartDate` stays attempt 1's date, but a new attempt can start before it, and the readers that fall back to it aren't reviewed
- **Where**: data-model.md `Goal` members table (`StartDate`: "attempt 1's start date") and "Kept constructor" (lines 88-90). research.md R-01.
- **Against**: locked AC-035 (attempt 1 from 23 Nov, ended 17 Nov; the starting values give a new attempt from 18 Nov). The existing checks (GoalTests.cs:94-98).
- **Reasoning**: A new attempt can start before attempt 1's start date. `GoalEvaluation.OfAsync` (line 139), `EntryViews.HistoryOf` (line 61), `GoalListItems.Of` (lines 17 and 29), `GoalService.DetailsOf` and `GoalTimeline` (the `CurrentDate` candidate filter `candidate >= goal.StartDate`, and `EntriesThrough`'s `MondayOnOrBefore(goal.StartDate)`) all use `goal.StartDate` as the goal's first date or as their fallback. The plan reviews only the `GoalTimeline` members. Separately, the kept constructor "keeps its checks", which allow `attempt.StartDate` to be later than `startDate`, while the new invariant requires each attempt's first version to apply from that attempt's start date. The data model doesn't say which date wins, or whether that combination is now rejected.

### MINOR-3: The new foreign key gives SQL Server two cascade paths
- **Where**: data-model.md "Tables": a foreign key (`GoalId`, `AttemptNumber`) → `GoalAttempts` (`GoalId`, `Number`), "cascade with the goal" (line 18). research.md R-05.
- **Against**: HtDbContext.cs:138-139 (`Goals` → `GoalSettingsVersions` and `Goals` → `GoalAttempts`, both `DeleteBehavior.Cascade`).
- **Reasoning**: The existing `GoalSettingsVersions.GoalId` → `Goals` foreign key stays (the key keeps `GoalId`). If the new foreign key also cascades, deleting a goal reaches `GoalSettingsVersions` both directly and through `GoalAttempts`. SQL Server refuses to create a constraint like that ("may cause cycles or multiple cascade paths"). The plan's migration risk section covers only the default value and the order of the key rebuild, so the delete behaviour of the two foreign keys is left open.

### MINOR-4: Existing unit tests that break aren't acknowledged, and the field of the new problem is missing
- **Where**: plan.md "Risks" and Work order step 2; data-model.md "GoalProblem (enum, changed)" and `AttemptStartResult` ("`Field` on a refusal").
- **Against**: GoalTests.cs:73-76 (`Only_the_time_zone_differs_between_versions` expects one daily target and "2 times per day" to throw; both are times per day, so the new rule allows them). GoalDraftTests.cs:272-279 (every `GoalProblem` but `None` must have a `GoalDraft.FieldOf` mapping). GoalDraft.cs:60-81 (`FieldOf` throws for an unmapped problem).
- **Reasoning**: The plan names the absence tests that change (UI) and a test that creation's problems all map. It doesn't name these two Domain tests, which fail as soon as step 2 lands. `StartDateNotAfterEnding` also needs a `FieldOf` entry (presumably `GoalField.StartDate`) so that `AttemptStartResult.Field` can be filled, and the data model doesn't mention it.

### MINOR-5: The architecture rules planned are fewer than the contract and past slices call for
- **Where**: plan.md Constitution Check V and Work order step 3 (lines 287-288); research.md R-12 "Architecture" (lines 521-524).
- **Against**: contracts/application-api.md line 194 ("Architecture rules keep every HT.Ui type, and every HT.Application type other than `DemoGoalChangeService`, away from it"). The slice 7 precedent:
  - `UiDependencyTests.NoUiTypeMayDependOnTheDemoPauseStorePort` (UiDependencyTests.cs:113-119);
  - `OnlyTheHostsOperatorTypesMayDependOnTheOperatorServices`, which lists each demo service (lines 121-129);
  - `PersistenceDependencyTests.TheDemoAccountWriterTheDemoReportStoreAndTheDemoPauseStoreAreInternal` (lines 48-59);
  - the new public types in `PublicSurfaceTests` (lines 37-77).
- **Reasoning**: The plan lists only the widened `IGoalStore` rule, an Application-side fence for `IDemoGoalChangeStore`, and `UseCaseParameterTests`. Not planned:
  - the HT.Ui fence that the contract promises;
  - adding `DemoGoalChangeService` to the operator-services rule;
  - the internal-type check for `DemoGoalChangeStore`;
  - the NodaTime surface entries for the new public Domain and Application types.

  The plan doesn't say these are left out on purpose.

### MINOR-6: "No date" is defined differently from the spec
- **Where**: data-model.md `GoalTimeline.LastDate` ("A last date before the attempt's first entry means it has no date", line 139) and `AttemptView.HasNoDate` (line 248).
- **Against**: spec FR-008 ("A last date before the attempt's start date means the attempt has no commitment or period") and HP-001 / HP-014 ("an attempt with no date (a last date before its start date)").
- **Reasoning**: The two definitions differ for a per-week attempt that starts mid-week and ends before its first Monday-to-Sunday week ends. Example: start Wed 18 Nov, ending date Tue 24 Nov, last date Sun 22 Nov. The last date is not before the start date, but it is before the first entry (the week of 23 Nov). By the spec's wording, the page shows "Attempt n · 18 Nov – 22 Nov". By the data model's rule, it shows "ended before it began". The artifacts don't say which one is meant.

### MINOR-7: The name `GoalEnded` means opposite things
- **Where**: contracts/application-api.md "Texts" (`GoalEnded` (result) = "goal ended"; `Ended` = "This goal has ended."). data-model.md: `SettingsChangeResult` outcome `GoalEnded`, `PauseService` outcome `GoalEnded` ("This goal has ended."), `ExemptionStoreWrite` outcome `GoalEnded`, and `TodayEntry.GoalEnded` (bool).
- **Against**: constitution I and ADR 0005 (each named outcome carries its literal text).
- **Reasoning**: The text constant `GoalChangeTexts.GoalEnded` holds the success text of ending ("goal ended"). The outcomes named `GoalEnded` are refusals whose text is held by `GoalChangeTexts.Ended`. When a name refers to the success text in one place and the refusal in another, the wrong text is easy to map, and such a mismatch would be caught only by a text-asserting test.

### MINOR-8: How earlier Pending entries affect the overview's order, Pending flag and recordable list isn't specified
- **Where**: research.md R-07 (`EarlierPending` is added to "what can be recorded now" and the dashboard). data-model.md "Changed views" (only `DashboardCounts.Pending` and the Today list are named).
- **Against**: spec FR-016 (what can be recorded now includes the open Pending entries of an ended attempt), FR-018 ("The order rules are unchanged"). The overview's `Recordable` is "as `RecordingService.GetRecordableAsync` gives it at the same instant" (ReportService.cs:84-91). `GoalOrder` groups by `evaluation.Pending` (GoalOrder.cs:28-29).
- **Reasoning**: For a restarted goal with an open Pending entry from attempt 1, the artifacts don't say any of the following:
  - whether `OverviewGoal.HasPending`, `CanRecord` and `Recordable` include it;
  - whether the goal sorts into the "Pending" group of the overview and the dashboard;
  - whether the Goals page's "Record" offers it.

  If `Recordable` leaves it out, it no longer equals `GetRecordableAsync`, which breaks slice 6's stated equality. If `Recordable` includes it, the order and the flags change for a goal that isn't ended.

### MINOR-9: Narrower ADR test than the constitution's, for a decision that is hard to reverse
- **Where**: plan.md Constitution Check, "Decision Discipline" row (line 153): "None spans features or rejects a product alternative, so there is no new ADR."
- **Against**: constitution "Decision Discipline" ("A decision that spans features, is hard to reverse, or rejects real alternatives MUST also become an ADR"). CLAUDE.md (ARCHITECTURE §9 "schema" is a deliberate unknown, to be recorded "in the feature's spec or an ADR"). data-model.md line 37 (`Down` "is only valid while every version belongs to attempt 1").
- **Reasoning**: The constitution's test is "real alternatives", not "product alternatives". R-01 rejects two real modelling alternatives. Once any attempt 2 exists, the data model itself says the migration can't be rolled back. The same row also says slice 9 reads the attempt model through the same `Goal` members. The check rewrites the criterion instead of applying it. Earlier slices also recorded their schema only in research.md, so this may be accepted practice, but the plan should say that rather than restate the rule.

### MINOR-10: Small inconsistencies inside the plan
- **Where**: plan.md Constitution Check IV ("`HistoryCard`, `GoalStats`, `Tag`, `Tabs`, `DataTable` and `Notice` are reused as they are") against Scale/Scope ("Changed components: … `HistoryCard`, which also serves the attempt view") and Project Structure ("HistoryCard (attempt view)"). plan.md Work order step 8, which updates the roadmap status only in the docs step.
- **Against**: docs/roadmap.md "Updating this file" ("the feature branch updates its row's Status after each step"; the status values include `acceptance locked`, `features locked` and `planned`). The roadmap row for slice 8 still says `clarified` (roadmap.md line 38), although `sources` and `features` are locked (plan "Gates").
- **Reasoning**: The plan describes `HistoryCard` both as unchanged and as changed. Its Work order also leaves the roadmap row stale through tasks and implementation, against the roadmap's own rule.

## Summary

4 MAJOR, 10 MINOR. The plan is detailed and mostly matches the spec, the ADRs and the locked criteria:
- the demo figures check out (74 records, 6 versions, 2 endings);
- the counts check out (7 use cases, 106 F009 scenarios);
- the check orders of FR-003, FR-007, FR-011 and FR-012 are carried into the contracts correctly.

It still has four real gaps. The code still assumes, in several places, that every version shares one target and that every stored record or note belongs to an entry:
- the goal-level `GoalHistory` used by the dialogs, the History card and the Today rows (MAJOR-1);
- the `NoteEntryOf` / `StoredEntryOf` throws (MAJOR-2);
- the single-kind `EvaluationCalendar` / `GoalCalendarView` (MAJOR-3);
- the time-free `GoalText.Name` (MAJOR-4).

This slice breaks both assumptions on purpose: settings changes within a kind, new attempts of any kind, and records kept outside every attempt. The plan's risk analysis names only three of the places affected. Following the data model as written would produce wrong pages for AC-020 and AC-080, faults on ended goals' pages, and failures of the locked AC-001 and AC-002. These should be resolved in the plan before `/speckit-tasks`.
