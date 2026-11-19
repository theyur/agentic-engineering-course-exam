# Critic pass 1: tasks for 005-recording-and-evaluation

**Verdict**: REVISE
**Date**: 2026-11-19
**Reviewed**: specs/005-recording-and-evaluation/tasks.md

## Notes

### MAJOR-1: The Phase 2 checkpoint cannot pass with a `RecordingService` that has no public use case

- **Where**: tasks.md T028 ("with no public use case yet", primary constructor `RecordingService(IRecordStore records, IGoalStore goals, IAccountStore accounts, TimeProvider clock)`), T029 ("`RecordingService` joins the theory of services whose public methods take `ActingUser` first"), the Phase 2 checkpoint (line 298: "the full suite pass ... Commit"). The first public method comes only in T042 (Phase 3).
- **Against**: `tests/HT.ArchitectureTests/UseCaseParameterTests.cs` lines 58-70: the `SignedInServices` theory runs `var methods = PublicMethods(service).ToList(); Assert.NotEmpty(methods);`, where `PublicMethods` takes the public, declared-only, non-special methods. Constitution V and tasks.md line 100 ("Commit only at a checkpoint, with the full suite green"). `Directory.Build.props`: `TreatWarningsAsErrors` is true.
- **Reasoning**: After T028 and T029, `RecordingService` has no public method. Its row in the theory fails `Assert.NotEmpty`, so the Phase 2 suite is red, and Phase 2 is the commit that must pair `RecordingService` with `IRecordStore` for `ValidateOnBuild`. The build is also at risk. T028 prescribes a primary constructor, and the private helpers it lists don't read `records` or `clock`: `CheckAccessAsync` uses the goal and account stores, and `EntryView` takes `now` as a parameter. An unread primary-constructor parameter gives warning CS9113, which `TreatWarningsAsErrors` turns into a build error. The ordering doesn't acknowledge either effect.

### MAJOR-2: The RecordDialog's live progress has no source that respects the layer rules

- **Where**: tasks.md T045 ("the progress under the field, live, by `RecordingText.Progress`") and T049 ("the live progress"). T024 defines `Progress(GoalHistory, Progress)`, where the second argument is a Domain `Progress`.
- **Against**: HP-002 ("the progress under the field"). CLAUDE.md "Layout and layer rules": "No business rules in Blazor components". data-model.md: `GoalHistory` carries the target's fields but no `GoalTarget`, and `HistoryEntry.Records` are `RecordView`s, not `RecordValue`s. contracts/application-api.md has no use case that evaluates a value that hasn't been saved.
- **Reasoning**: The formatter can only print a `Progress` it is given. A Domain `Progress` is produced only by `Evaluator.Evaluate`, from a `GoalTarget` and `RecordValue`s, or by the use cases after a write. To show "12 of 20 min" or "2 of 3 times" while the user types, before saving, the component would have to rebuild a `GoalTarget` and the record values from view data and call the evaluator. Otherwise it would have to re-implement FR-014's counting itself: Build periods count the records that reach the amount, Reduce periods sum their totals, and hours become minutes. Both put evaluation logic in a Blazor component. No task gives the dialog a progress preview from Application or Domain. Neither do plan, research R-12 or the contracts, which only say "live, by `RecordingText`". T045 cannot be implemented as written without breaking the layer rule.

### MINOR-1: No task covers an entry range with no entries

- **Where**: tasks.md T043 and T053 ("one `ReadAsync` from the first entry's `FirstDate` to the last entry's `LastDate`") and T051 ("one store read per call").
- **Against**: `GoalTimeline.TryCalendar`/`Weeks` (`src/HT.Domain/Goals/GoalTimeline.cs` lines 77-98 and 215-225) returns an empty calendar in these cases, while `CurrentDate` is non-null:
  - a range of unscheduled days only;
  - a range before the start date;
  - a per-week goal before its first full week, for example started on a Wednesday and read on the Thursday.
- **Reasoning**: With no entries there is no first `FirstDate` and no last `LastDate`. T043's window (`current − 9` to `current`) is empty for a per-week goal in its first partial week. So `GetRecordableAsync` and `GetHistoryAsync` have an undefined step, and the T051 assertion "one store read per call" contradicts the natural empty result. Neither the task nor its tests say what happens.

### MINOR-2: Dialog modes needed by later tasks are not assigned to any task

- **Where**: tasks.md T045 (only "opened from 'Record'"), T063 (only the "Edit" mode), T066 ("A row's 'Add record' opens the RecordDialog with 'For' fixed to that row") and T067 (tests "the fixed 'For' from a row"). The shared-files list (line 582) names only T045 and T063 for `RecordDialog.razor`. T073 says nothing about the TextArea holding the existing note when it is opened from a note button.
- **Against**: HP-002 ("Opened from a History row ... 'For' is fixed to that row (for a period, its dates up to the current date)"). HP-003 and contracts/host.md "NoteDialog": opened from the row's note, and saving it empty removes the note.
- **Reasoning**: The fixed-"For" mode is tested in T067, but no task adds it to `RecordDialog.razor`. T066 only edits `HistoryCard.razor`, so the dialog change has no task and no entry in the shared-file ordering. The tasks don't say whether the NoteDialog opened on an existing note starts with that note's text. If it starts empty, saving without retyping removes the note (FR-007), so the editing behaviour is left to chance.

### MINOR-3: The RecentNotesCard's StatusChip lacks the inputs StatusChip needs

- **Where**: tasks.md T037 (StatusChip parameters `State`, `ScheduleKind`, `Period`) and T074 ("each note with its date ... and its StatusChip").
- **Against**: data-model.md: `RecentNote` is `FirstDate, LastDate, Note, State`, and `RecentNotesResult` has only `Notes`. HP-004: "Due today" or "Due this week", by schedule kind.
- **Reasoning**: A Due per-week note must read "Due this week". The recent-notes result carries no schedule kind or period, and T074 doesn't say where the card gets them, for example from `GetRecordableAsync`'s `GoalHistory` in `GoalDetailBody`. The data flow is left unspecified.

### MINOR-4: `RecordingText` leaves several texts undefined, and the smoke check cannot reach its recorded-value formatter

- **Where**: tasks.md T024 and T090.
- **Against**: HP-002 and HP-011, and `design/components/RecordDialog.html` line 11, which has the sub line "Build · at least 20 min · every day" and the sentence "Partial time is kept. Below 20 min the day counts as Failed when it is finalized.".
- **Reasoning**:
  - `ForLabel` defines labels only for single dates. It doesn't say what a per-week Due or Pending entry reads, its `WeekRange` or its Monday's short date, though T004 and T045 offer per-week entries in "For".
  - "the yes/no commitment forms" of `Progress` are not given.
  - The text of a Reduce yes/no period ("Yes records against at most N") is not given.
  - The partial-progress sentence and the sub-line target text, both built from the goal's target, are assigned to no formatter.
  - T090 takes "12 min" from `RecordingText`, but the only matching member is `Recorded(GoalHistory, RecordView)`. The smoke check, which reaches the app only over HTTP, would have to build a whole `GoalHistory` and a `RecordView` with a `RecordId` just to format an amount.

### MINOR-5: The History card's structure differs from its design preview

- **Where**: tasks.md T003 ("a DataTable with Date, Recorded, Result (StatusChip) and Note ... the sentence ... under the table"), T054 (four columns) and T066 ("The sentence ... under the table").
- **Against**: `design/pages/PageGoalDetail.html` lines 33-35:
  - the History card head is an `ht-inline` with the `h2` and the muted sentence "Corrections are evaluated with the rules of that date." beside it;
  - the table has a fifth, empty-headed actions column (`<th></th>`, `td.ht-actions`).

  Constitution IV says "Pages MUST follow the structure and wording of their previews". `src/HT.Ui/Components/Shared/HtCard.razor` renders the title `h2` directly in the `section`.
- **Reasoning**: The tasks move the sentence under the table. They don't say which column holds the row actions ("Add record", "Edit"), while the record and note buttons go into the Recorded and Note cells. Building the History card on `HtCard` with `Title` cannot produce the design's head layout. Nothing acknowledges the deviation from the full preview.

### MINOR-6: The "Within limit" replacement misses one design file

- **Where**: tasks.md T005 ("'Within limit' becomes 'Met' everywhere"). It lists `StatusChip.html` and `.md`, `CommitmentRow.html` and `PageHomeSignedIn.html`.
- **Against**: `design/tokens.json` line 70: `"usage": "Status Met / Within limit: ..."`. The design-change preamble (tasks.md line 146) says these texts are "replaced everywhere".
- **Reasoning**: Once the listed files are done, the token description still uses the retired term. That contradicts HP-004 and constitution I, and the task's own "everywhere".

### MINOR-7: SC-002's grace-period boundaries are not planned per combination

- **Where**: tasks.md T016 (every combination, "read after the grace end"), T017 (state boundaries "with no record, with a record below and with one that reaches the target", not per combination) and T058 ("the Domain tests T016 covering the full matrix").
- **Against**: SC-002: "For every combination of Build or Reduce, measurement and schedule kind, the tests cover the target at, just below and just above its amount, and the state just before, at and just after the end of the grace period". Constitution II: both sides of each boundary.
- **Reasoning**: T016 checks the target boundaries for every combination, but only after the grace end. T017 checks the grace-end boundaries without naming the combinations. T058 defers the full matrix to T016. No task plans the "before/at/after the grace end" side for every combination, though SC-002 asks for it in "100% of the boundary tests".

### MINOR-8: HP-006 log capture runs before demo history loading exists, and its faults are never checked

- **Where**: tasks.md T079 (US6, "run every use case and store member of this slice, refusals and faults included"), T083 and T087 (rollback, and "its exception names no goal, habit, note or value"), and T089 (the fault is logged "with the exception").
- **Against**: HP-006, SC-006 and contracts/host.md "Operator log" ("Error, with the exception"). The phase order puts US6 before US7 (tasks.md lines 39-42 and 568).
- **Reasoning**: When T079 runs, `DemoHistoryService` and `DemoHistoryStore` don't exist yet, so "every use case and store member of this slice" can't include them. No US7 task captures logs or exception messages for a `DemoHistoryStore` fault. T083 only checks the rollback. Yet that exception is written to the operator log by `RunStepAsync`. T087's "names no goal, habit, note or value" is never verified.

### MINOR-9: Edge cases of the spec have no unit or integration task, and one planned case cannot exercise what it names

- **Where**: tasks.md T019 ("Sunday 23:59 and Monday 00:00 instants: the entry follows the date, never the instant") and T018 (`EntryOf(Goal goal, DateOnly date)`, which takes no instant). No task covers the edge cases listed below.
- **Against**: spec.md "Edge Cases". CLAUDE.md "Testing": "A feature is not done without tests: unit ..., integration (workflows against real SQL Server)".
- **Reasoning**: No task tests these edge cases:
  - a goal on an Unpublished catalog habit is still recorded and evaluated. Only the locked AC-065 covers it, and step definitions are not tasks here;
  - after a large move east, the first commitment of the new version has already ended when the change is made, and is Pending or final by the same rules;
  - two consecutive commitments are each Pending with different grace ends.

  The Sunday and Monday case in T019 targets `EntryOf`, which has no instant parameter, so the test cannot show "never the instant of saving". That case belongs at the `RecordAsync` level, as AC-034 does.

### MINOR-10: The smoke-check contract and the host test are updated only in part

- **Where**: tasks.md T091 (only "the coverage table in `specs/003-demo-hosting/contracts/smoke-check.md` ... gains the 4 rows of contracts/host.md"; the `SmokeCheckHostTests` changes listed don't include its forbidden-text list).
- **Against**: `specs/003-demo-hosting/contracts/smoke-check.md`:
  - "Checks, in order" lists checks 1 to 5;
  - "Output" says "a failed `home` does not stop checks 2–5", and check 5's reason reads "`GET <path> gave status <code>`";
  - "Coverage rule" says "All demo data is written through the demo data port `IDemoDataStore`";
  - its table has the columns "Port method | Parameter | Check", while contracts/host.md's rows are "Port | Method | Parameter | Type | Check".

  `tests/HT.UiTests/Host/SmokeCheckHostTests.cs` lines 116-124 forbid only demo1 to demo3's names in the output.
- **Reasoning**: After T091 the slice 3 contract describes five checks and one demo port, and the new rows don't fit its table layout. Check 6's reason format ("`GET /goals gave <status>`") differs from check 5's wording. The in-process host test never asserts that demo4's goal names, notes or amounts are absent from the output, so check 6's "never printed" rule is tested only against scripted pages.

### MINOR-11: The "no existing constructor changes" convention conflicts with T090

- **Where**: tasks.md line 105 ("No existing constructor, method, port member or result type changes ... If a task seems to need one, stop and ask the owner") and T090 ("`SmokeCheckCoverage.CoverageEntry` gains the port type as its first member").
- **Against**: `tools/HT.SmokeCheck/SmokeCheckCoverage.cs` line 15 has the top-level positional record `CoverageEntry(string Method, string Parameter, Type Type, string Check)`, which is not nested in `SmokeCheckCoverage`. `tests/HT.UiTests/SmokeChecks/CoverageTests.cs` lines 66-71 construct it positionally.
- **Reasoning**: T090 changes an existing positional constructor, which the blanket convention forbids without asking. The convention is meant for FR-019's use cases and ports, but it is worded for every constructor, so the implementing agent gets contradictory instructions. The task also names the type by a qualified name it doesn't have.

### MINOR-12: T093 asks for roadmap edits that are already made

- **Where**: tasks.md T093 ("the roadmap rows 'When and how hosted demo data is refreshed' and 'Hosting services …' in `docs/roadmap.md`").
- **Against**: `docs/roadmap.md` lines 178 and 180 already say "Slice 5 adds a daily scheduled trigger in refresh mode ... (ADR 0018)" and "Slice 5 adds a daily refresh on a schedule until 2027-01-31 (ADR 0018)", with ADR 0018 linked. They were added in commit dbc5a50.
- **Reasoning**: The task describes work that is already done, so the Polish phase checks against an outdated picture of the repository.

## Summary

2 MAJOR, 12 MINOR. The task list is detailed and traces most requirements, but two points block implementation as written:

- **MAJOR-1**: Phase 2's mandatory green commit fails, because `RecordingService` joins a reflection theory that requires public methods before it has any, and its unread constructor parameters can also break the build.
- **MAJOR-2**: The RecordDialog's live progress has no Application or Domain source, so implementing it means putting evaluation rules in a Blazor component.

The MINOR notes are gaps and inconsistencies that should be cleaned up but don't break a requirement on their own:

- what happens with no entries;
- dialog modes and recent-note data that no task owns;
- formatter texts that are not defined;
- the History card's deviation from the design;
- coverage of SC-002, HP-006 for demo history loading, and the spec's edge cases;
- the partial update of the slice 3 smoke-check contract;
- a convention that conflicts with T090;
- one stale Polish task.
