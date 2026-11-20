# Critic pass 1: plan for 007-personal-reports

**Verdict**: REVISE
**Date**: 2026-11-20
**Reviewed**: plan.md, research.md, data-model.md, quickstart.md, contracts/application-api.md, contracts/host.md

## Notes

### MAJOR-1: The dashboard and overview views cannot feed the Home rows, the Goals items or their dialogs
- **Where**: data-model.md "`TodayEntry` (FR-012)", "`OverviewGoal` (FR-011)", "`ResultView`"; contracts/host.md "Signed-in Home" (CommitmentRow parts and actions) and "Goals page" ("Record" opens slice 5's RecordDialog); research.md R-12.
- **Against**: spec HP-002 and HP-003; design/components/CommitmentRow.md and .html; existing code `src/HT.Ui/Components/Records/RecordDialog.razor` (parameters `GoalHistory Recordable`, `HistoryEntry? Row`, `RecordView? Editing`), `NoteDialog.razor` (`GoalHistory History`), `StatusChip.razor` and `RecordingText.State(state, scheduleKind, period)`, `RecordingText.Progress(GoalHistory, Progress)`.
- **Reasoning**:
  - `TodayEntry` has only GoalId, GoalName, BuildOrReduce, dates, State, Progress, HasRecord, HasNote and GraceEnds. A HP-003 row also needs:
    - the schedule text for a Due row without a record ("Mon · Wed · Fri");
    - the measurement and count unit for the progress text ("12 of 20 min", "1 of at most 2 cups", which `RecordingText.Progress` builds from a `GoalHistory`) and for "the progress bar, for count and duration goals";
    - the schedule kind and period. `StatusChip` needs them to write "Due today" or "Due this week". The action rule also needs them: "Record" for a period, "Edit" for a commitment with a record.
  - "Edit" opens "that record". `RecordDialog` needs its `RecordView`, with the record id, but `TodayEntry` carries only `HasRecord`.
  - `OverviewGoal` is the same. HP-002's meta line "12 of 20 min today" or "1 of 3 this week" needs the count unit and the day-or-week period. Slice 4's `GoalListItem` has neither (it has `Measurement`, `ScheduleKind` and texts only), and `DueProgress` is a bare `Progress`.
  - Both islands open `RecordDialog` and `NoteDialog`. These require a `GoalHistory` and a `HistoryEntry`, and no slice-6 view supplies them. The plan does not say which extra call (for example `GetRecordableAsync` or `GetHistoryAsync` per opened row) provides them.
  - As specified, the pages of HP-002 and HP-003 cannot be built from the data model and contracts. `/speckit-tasks` would have to invent public fields or extra reads.

### MAJOR-2: The trend's kind cannot tell minutes from counts, so the target line and titles come out wrong
- **Where**: research.md R-05 (last bullet: "The page writes 'Target {n} min' for amount bars and 'Target {n}' otherwise (HP-001). `Trend.Kind` (`Amount` or `RecordCount`) tells it which."); data-model.md `TrendKind` and `TrendView`; contracts/application-api.md `ReportText.TrendTitle/TrendSubLine/TargetLine/TrendLabel(TrendView)`.
- **Against**: spec HP-001 ("Target {n} min" only "when the bars are amounts in minutes", "Target {n}" otherwise, "counts and numbers of records") and FR-006 (`Amount` covers commitment-based count goals and Reduce count periods); the design titles in "Design changes before build" ("Minutes per day · last 14 days", "Times per week · last 12 weeks", "Days done per week · last 12 weeks"); HP-006's label ("Minutes per day, last 14 days; target 20").
- **Reasoning**:
  - By data-model.md, `TrendKind.Amount` covers count goals as well as duration goals. R-05's rule therefore gives "Target 1 min" for demo4's "Sugar, at most 1 sweet a day", a Reduce count per-day goal shown on the hosted demo. It gives "Target 5 min" for AC-027's count goal "Reading, count 5 pages". Both break HP-001.
  - `TrendView` carries only Kind, Bars, Target and texts, with no measurement, unit or period. So `ReportText.TrendTitle(TrendView)` and `TrendSubLine(TrendView)` cannot choose between "Minutes per day", a count unit per day, and "Times per week".
  - `TrendView.Title`, `SubLine` and `TargetLine` are also described as produced by `ReportText` functions that take the `TrendView` itself, which is circular.

### MAJOR-3: The work order runs the smoke check's host test before the pages it checks exist
- **Where**: plan.md "Work order" step 5 (smoke check 6's three texts and check 7, with `SmokeCheckHostTests`' exact lines "in the same change") before step 6 (Pages).
- **Against**: `tests/HT.UiTests/Host/SmokeCheckHostTests.cs` `A_freshly_seeded_host_passes_every_check`, which runs the real smoke check in process against a real host on `HT-UI-SMOKE` and asserts every line is PASS with exact counts; constitution V (build and full suite pass before a change is reported done).
- **Reasoning**: check 6's new texts ("Consistency" and "commitments" on Reading's page, "Due now" on the Home) and check 7's texts ("weeks" on Strength training's page, "Missed this week" and a "Missed" chip on the Home) are rendered only by step 6's GoalStats card and `TodayPanel`. After step 5, the host test's checks 6 and 7 fail, and so does its "(N of N)" assertion. The plan names one such cross-step coupling (steps 3 and 4, `ValidateOnBuild`) and tells the author to commit them together, but it misses this one. If tasks follow this order, they leave a red suite between steps 5 and 6.

### MAJOR-4: The Goals-page "Record" dialog mode is changed without an owner decision
- **Where**: plan.md "Spec discrepancies" 2 ("The RecordDialog opens fixed to the goal's newest Due entry, or its newest Pending one"); research.md R-12 "Goals page"; contracts/host.md "Goals page".
- **Against**: spec HP-002 ("'Record' … opening the RecordDialog of slice 5 HP-002"); `specs/005-recording-and-evaluation/spec.md` HP-002 (opened outside a History row, the dialog's "For" select lists every FR-011 entry; "fixed" only when opened from a row); spec HP-003, which says "fixed to that entry" explicitly for Home rows and not for the Goals page; CLAUDE.md "Source of truth" ("stop and ask; don't silently diverge").
- **Reasoning**: slice 5 HP-002 defines two modes: the page head's select of what can be recorded now, and a History row's fixed entry. The plan creates a third mode, fixed to an automatically chosen entry. On the Goals page a user with a Due entry can then no longer record a Pending one, although the "For" select of slice 5 would offer it. The plan presents this as a reading "without changing" the spec and says no detail needs the owner, but it changes the behaviour the requirement refers to.

### MINOR-1: The full GoalListItem component's name and the fate of GoalItem are left open
- **Where**: plan.md Project Structure (`Shared/ … GoalListItem (full)`); research.md R-12 ("the existing reduced `GoalItem` becomes its slice-4 form or is replaced, as HabitComponentTests decides").
- **Against**: `src/HT.Ui/Components/Shared/GoalItem.razor` header ("Named GoalItem, not GoalListItem, so the view HT.Application.Goals.GoalListItem stays unambiguous in the pages"); constitution IV ("Each design component MUST be implemented once as a Razor component"); spec "Slice 4 and 5 requirements and tests this slice changes" (a reduced item is kept "only where slice 6 still renders one").
- **Reasoning**:
  - A Razor component named `GoalListItem` reintroduces the name clash that slice 4 avoided on purpose. `GoalsList` uses both the component and `OverviewGoal.Item`, which is of type `HT.Application.Goals.GoalListItem`.
  - Whether `GoalItem` stays is deferred to "as HabitComponentTests decides", but a test cannot make that decision. `Goals.razor` is `GoalItem`'s only user, so if both stay, one design component has two Razor implementations.

### MINOR-2: The StatusChip "Upcoming" word has no source, and an existing test asserts there is none
- **Where**: plan.md Scale/Scope and Project Structure ("StatusChip (+ Upcoming)"); contracts/application-api.md `ReportText` (no member for it); plan.md Risks ("This plan changes no existing … public method").
- **Against**: `src/HT.Ui/Components/Shared/StatusChip.razor` (word from `RecordingText.State`, which throws for Upcoming); `tests/HT.UnitTests/Application/Records/RecordingTextTests.cs:88` `Upcoming_has_no_text_in_this_slice`; `tests/HT.UiTests/Components/Shared/StatusChipTests.cs`.
- **Reasoning**: the chip takes its word from `RecordingText.State`. Making Upcoming render either changes that existing public method's behaviour and breaks `Upcoming_has_no_text_in_this_slice`, or needs a new text source and modifier class. The plan names neither, and R-14's list of changed existing tests leaves both tests out.

### MINOR-3: `ReportText.PendingLeft` is planned but missing from the contract, and it has nothing to delegate to
- **Where**: research.md R-13 (`PendingLeft(graceEnds, now)`: "delegates to slice 5's `RecordingText` 'N h left' rule"); contracts/application-api.md `ReportText` (no such member).
- **Against**: `src/HT.Application/Records/RecordingText.cs`. The "N h left" rounding exists only inside `ForLabel(HistoryEntry, DateOnly, DateTimeOffset)`, and no public member returns it alone.
- **Reasoning**: the two artifacts disagree on `ReportText`'s public surface. "Delegates" assumes a member that does not exist, so either `RecordingText` gains a public member, which the plan does not list, or the rule is duplicated, against the single-formatter intent of HP-005.

### MINOR-4: R-05 says every bar has the same target; commitment-based yes/no bars do not
- **Where**: research.md R-05 ("every bar of a goal has the same target, and `Trend.Target` is that of the latest bar"); data-model.md `Trend.Target`; contracts/host.md (one dashed target line from `ReportText.TargetLine`).
- **Against**: spec FR-006 (a Build yes/no week's target is the number of its commitments on or after the start date); acceptance.md AC-025 (bars with targets 2, 3 and 3).
- **Reasoning**: the claim is false for commitment-based yes/no goals. A goal that starts mid-week has a smaller first-week target. The page draws one target line and one aria-label target from `Trend.Target`, and no artifact says how varying per-bar targets are shown.

### MINOR-5: The visitor card's example strip does not fit the GoalStats and StripLabel signatures
- **Where**: contracts/application-api.md `ExampleGoal.Strip` (`IReadOnlyList<EvaluationState>`) and `ReportText.StripLabel(RecentResults, StreakUnit)`; contracts/host.md "Visitor Home" (the shared GoalStats card with "the strip of 21 results").
- **Against**: spec HP-004 (the shared component and formatter, "so the card cannot disagree"); HP-006 (every strip has a counting `aria-label`).
- **Reasoning**: the goal page feeds GoalStats a `RecentResults`, while the example card offers a bare list of states with no `Counts`. `StripLabel` as declared cannot be called for the example card, so either a second strip input or a second label path is needed, which the artifacts don't say.

### MINOR-6: Where the smoke check matches its new texts is not specified, and an unscoped "weeks" would not prove the unit
- **Where**: research.md R-11; contracts/host.md "Smoke check".
- **Against**: `specs/003-demo-hosting/contracts/smoke-check.md` check 6 ("The texts are matched only in the History card: `PageReader.ReadCardText(html, "History")`"; no goal name printed); `tools/HT.SmokeCheck/SmokeCheckRunner.cs` `CheckHistoryPageAsync`; spec HP-008.
- **Reasoning**:
  - "Consistency" and "commitments" are on the GoalStats card, not in the History card that check 6 reads. The plan does not say which card or which page reader the new texts use.
  - For check 7, "weeks" anywhere on the Strength training page would also match a trend sub line such as "last 12 weeks". The check could then pass without the streak unit HP-008 asks for.
  - The output rule that check 7 prints no demo5 goal name, and the matching `SmokeCheckHostTests` assertion, are not mentioned.

### MINOR-7: R-14 misdescribes two existing architecture tests
- **Where**: research.md R-14 "Architecture" (`PublicSurfaceTests` "lists `IDemoReportStore` among the public ports"; `ClockReadTests` "covers" the namespace); plan.md Constitution Check II ("`ClockReadTests` covers `HT.Domain.Reports` and `HT.Application.Reports`").
- **Against**: `tests/HT.ArchitectureTests/PublicSurfaceTests.cs`, which checks only that signatures have no NodaTime type and has no port list; `ClockReadTests.cs`, whose generic rule already covers every Domain and Application type, while its namespace-scoped rule forbids `System.TimeProvider` and `HT.Domain.Time`, which `ReportService` needs (`TimeProvider`, and `NamedTimeZone` for "this week").
- **Reasoning**: the "public ports" list the plan names does not exist. "Covers `HT.Application.Reports`" either adds nothing, through the generic rule, or would fail, through the strict rule. The new architecture rules are therefore underspecified for tasks.

### MINOR-8: Query counts in the performance reasoning are understated
- **Where**: plan.md Technical Context "Performance Goals" ("one goal query plus one record range read per goal") and Risks ("a case that takes four queries"); research.md R-08 ("one goal-list query and three range reads").
- **Against**: `src/HT.Persistence/Records/RecordStore.cs` `ReadAsync`, which runs two queries (records, then notes); every use case also reads the account (`IAccountStore.FindByIdAsync`, R-03).
- **Reasoning**: SC-007's case costs at least eight round trips, not four. The 2-second margin argument may still hold, but the stated basis is wrong.

### MINOR-9: The quickstart's exact figures hold only on the day the demo accounts were created
- **Where**: quickstart.md §3 ("On a database that already has `demo4`, the start adds `demo5`…"), steps 3 to 5 ("Pending results first", "'Missed this week' 1", "from '92% (24 of 26)' to '96% (25 of 26)'").
- **Against**: spec FR-030 and FR-032 (figures at the run time on run date R); ADR 0021 Consequences (a local database keeps the history relative to the day it was first created).
- **Reasoning**: on a database whose `demo4` was created earlier, R − 10 no longer has those figures, no Pending result remains, and `demo5`'s Screen time has more than one Missed result this week. The quickstart's own premise contradicts its expected values. It does not say to start from a fresh database on the run day.

### MINOR-10: `StreakUnit.Of` is a static method on an enum
- **Where**: research.md R-01 (`StreakUnit` "(enum …): `StreakUnit.Of(GoalTarget)`"); data-model.md `StreakUnit`.
- **Against**: C# enums cannot declare members. CLAUDE.md "Known false positives: Roslynator" notes that no C# 14 language feature is used yet and that Roslynator's older bundled Roslyn may misreport one.
- **Reasoning**: as written, `StreakUnit.Of` needs a C# 14 static extension member, a first for the repo with a known tooling caveat, or a different holder type. The plan names neither.

### MINOR-11: The demo report result types do not follow demo history's member for member
- **Where**: data-model.md "`HT.Application.Operator`" table (`DemoReportAccountResult`: Email, Message, Goals, Records; `DemoReportLoadingResult.Accounts (DemoReportAccountResult)`); research.md R-09 ("follows `DemoHistoryService` member for member").
- **Against**: `src/HT.Application/Operator/DemoHistoryAccountResult.cs` (it also has `DemoAccountOutcome Outcome`); `DemoHistoryLoadingResult.Accounts` (`IReadOnlyList<…>`).
- **Reasoning**: the new result drops `Outcome`, and `Accounts` is written in the singular. Both contradict the "member for member" claim and leave the public shape ambiguous for the AC reference export.

## Summary
4 MAJOR, 11 MINOR.

The plan covers every FR, SC and edge case, and its domain approach is sound: folds over slice 5's states, `EntriesThrough` without the 366-day cap, and one evaluation per goal per read. The FR-030 and FR-032 figures, the order rules and the refusal order check out against the code and the locked criteria. It is not ready for `/speckit-tasks`, for four reasons:
- The public views for the dashboard and the overview lack what the Home rows, the Goals items and slice 5's dialogs need.
- The trend's kind and texts would produce wrong target lines, such as "Target 1 min" for demo4's Sugar goal.
- The work order puts the smoke check's real-host test before the pages it checks.
- The Goals-page "Record" mode departs from slice 5 HP-002 without the owner.

The MINOR notes are naming, test-impact and documentation gaps that tasks would otherwise have to resolve ad hoc.
