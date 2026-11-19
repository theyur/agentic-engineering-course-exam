---

description: "Task list for slice 5, recording and evaluation"
---

# Tasks: Recording and evaluation

**Input**: Design documents from `specs/005-recording-and-evaluation/`: [plan.md](plan.md), [spec.md](spec.md),
[research.md](research.md), [data-model.md](data-model.md), [contracts/application-api.md](contracts/application-api.md),
[contracts/host.md](contracts/host.md), [quickstart.md](quickstart.md); [ADR 0018](../../docs/adr/0018-evaluation-on-read-and-demo-history.md),
[ADR 0016](../../docs/adr/0016-time-zones.md), [ADR 0017](../../docs/adr/0017-embedded-time-zone-data.md).

**Prerequisites**: `acceptance.md` and `tests/HT.AcceptanceTests/Features/F005/` are locked. No task edits them,
the lock files, the run records or anything under `tests/HT.AcceptanceTests`.

**Tests**: required. The constitution (II, III, V) and `CLAUDE.md` make unit, integration, architecture and
selective UI tests part of done, so every story phase has test tasks. xUnit v2 everywhere, hand-written fakes,
`FakeTimeProvider`, no mocking library (ADR 0006). Acceptance step definitions are not tasks here: the owner starts
`/speckit-ac-tests-write` and `/speckit-ac-tests-analyze` after `/speckit-implement` (plan, Work order 8).

**Organization**: Phase 2 follows the plan's Work order steps 1 and 2, and the shared parts of steps 3 and 4:

- design;
- the whole of `HT.Domain.Records`, `HT.Domain.Evaluation` and the `GoalTimeline` members, with their boundary
  tests;
- the Application types every use case shares: texts, views, results, `RecordingText`, `RecordPreview`,
  `DateTexts.WeekRange`, and the `IRecordStore` port with its fake;
- the tables, the migration `Records` and the whole `RecordStore`, registered with its port;
- `StatusChip`.

`RecordingService` itself is created in US1, together with its first use case and its registration. A service with
no public method would fail `UseCaseParameterTests`' `Assert.NotEmpty`, and unread constructor parameters would
fail the build (CS9113 under `TreatWarningsAsErrors`).

Each story phase then adds its use cases, its store tests, its pages and, for US7, the host, smoke check and
workflow (plan, Work order 5). Inside a story the plan's layer order holds: Application and Persistence, then
host, then pages.

The story phases follow the spec's priorities, with three moves, each because a later story tests what an earlier
one builds:

- P1 runs US1, US5, US2, US3. US5 (history) comes before US2, because US2's states are read through the history.
- US4 (P2, notes) comes before US6 (P1, privacy), because US6's privacy tests cover all seven use cases of
  `RecordingService`, the two note use cases included.
- US7 (P2, demo results) comes last. Its smoke check reads the History card that US5 builds.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: the task can run in parallel with the other [P] tasks of its group (they touch different files), once
  the tasks it names as dependencies are done.
- **[Story]**: the user story from spec.md (US1 to US7).
- Paths are relative to the repository root.

## Conventions for every task

- Build with `dotnet build HabitTracker.slnx`. `TreatWarningsAsErrors` is on, so a warning is a failure.
- Use the names in [contracts/application-api.md](contracts/application-api.md) and [data-model.md](data-model.md).
  Every Application method and every port method is `async` and takes a `CancellationToken` last.
- The design artifacts stay the contract (constitution, Governance: "never by silent divergence"). A task that adds
  a public member or a layout these tasks settled records it in the same task, in the document named:
  - data-model.md: `Evaluator.ProgressOf` (T015), `GoalHistory.Target` and `ReadAt` (T022), `RecordingText`'s
    members (T024), `RecordPreview` (T028);
  - contracts/host.md ("Recorded"), research.md R-12 ("the Recorded cell") and quickstart.md §3 step 7: "3 sweets"
    becomes "3 sweet", the unit as stored (T024);
  - contracts/host.md and research.md R-12: the History card's head sentence, its actions column, its note text
    and its absence without begun entries (T054), the page's two columns (T046), and the RecordDialog's title,
    labels and help texts (T045);
  - contracts/host.md: the type name `CoverageEntry` (T090).
- Take message texts from `RecordTexts`, `GoalTexts`, `AccountTexts` and `OperatorTexts`. Never retype one. Tests
  assert the literal text from spec.md, so a typo in a constant fails a test.
- Time and culture stay out of `HT.Domain` and `HT.Application` (constitution II; research R-01, R-13):
  - Time enters only as `TimeProvider` in the services, read once per call as `_clock.GetUtcNow()`, and as an
    instant parameter in Domain.
  - The grace end is `GracePeriod.EndOf(ends)`, on UTC instants. Never add days in local time.
  - Date and month texts come only from `DateTexts`. Numbers are formatted with the invariant culture, never the
    current one.
- No NodaTime type appears in any public or protected signature of `HT.Domain` or `HT.Application` (ADR 0017).
- Records and notes are private (constitution III, FR-017):
  - They are reached only through `RecordingService` and the owner-scoped `IRecordStore` members. Every call
    passes the acting user's `AccountId` as the owner and the goal named in the call. No member looks a record up
    by identifier alone.
  - Administrators get no record and no note of any account.
  - No note, recorded value, unit, goal name or habit name goes into a log template, an exception message or an
    error page (HP-006). A refusal result carries no input value.
- Test instants are UTC literals worked out by hand from the spec's local times, never computed by the code under
  test. For example:
  - a Europe/Kyiv commitment on Saturday 24 October 2026 starts at `2026-10-23T21:00:00Z`, ends at
    `2026-10-24T21:00:00Z`, and its grace period ends at `2026-10-26T21:00:00Z`, which is 23:00 Kyiv time on Monday
    26 October (US2-AS13);
  - a Europe/Kyiv commitment on Saturday 27 March 2027 ends at `2027-03-27T22:00:00Z`, and its grace period ends at
    `2027-03-29T22:00:00Z`, which is 01:00 Kyiv time on Tuesday 30 March.
- Goals with past commitments are created at an earlier supplied time, then recorded and read at later ones, by
  moving the `FakeTimeProvider` forward. A goal's start date is never in the past (slice 4).
- Pages use only `design/` tokens and `ht-` classes, and follow the structure and wording of their `design/` preview
  after the design tasks of Phase 2 (constitution IV, HP-011). Parts of slices 6 to 8 are absent, not disabled
  (ADR 0008).
- Interactive components call use cases only through `UseCaseScope` (slice 2 research R-15). The goal page's static
  shell injects the services.
- Never read, print or embed a secret. That includes the `Operator:*` User Secrets values and the hosted settings
  the workflow masks.
- No `InternalsVisibleTo` in `HT.Domain`, `HT.Application` or `HT.Persistence` (ADR 0010; `AcSurfaceTests` enforces
  it). Tests use public members only, and arrange or inspect rows with plain SQL (`tests/HT.IntegrationTests/Sql.cs`)
  or through the public ports. `RecordMapping` rebuilds values with the public `RecordValue.FromAnswer` and
  `FromAmount` (research R-02).
- Databases:
  - integration tests use `HT-IT` (`DatabaseFixture`), `HT-IT-SEED` (`SeedDatabaseFixture`) and `HT-IT-MIGRATE`
    (`MigrationDatabaseFixture`);
  - host tests use `HT-UI` and its derived databases;
  - no test touches `HT` or `HT-AC`.

  Tests in shared databases use account emails, habit names and goal names with their own prefix. They check by
  inclusion, exclusion and relative order, never by totals.
- Commit only at a checkpoint, with the full suite green (constitution V). These land in the same commit, so
  `ValidateOnBuild` never meets an unregistered port (plan, Work order 3 and 4):
  - `IRecordStore`, `RecordStore`, its registration, `FakeRecordStore` and its bUnit registration (Phase 2);
  - `RecordingService`, its registration and its bUnit registration, after its port is registered (US1, T042);
  - `DemoHistoryService`, `IDemoHistoryStore`, `DemoHistoryStore` and their two registrations (US7).
- No existing constructor, method, port member or result type of `HT.Domain`, `HT.Application` or `HT.Persistence`
  changes (FR-019, plan "Risks"). If a task seems to need one, stop and ask the owner. The locked F001, F002 and F004
  AC support code must keep compiling, and nobody reads it. The smoke check tool (`tools/HT.SmokeCheck`) is outside
  that rule: T090 changes its `CoverageEntry` record and updates every caller in the same task.
- A search for callers or tests of a type excludes the AC project:
  `grep -rnw "<Type>" tests --include=*.cs --exclude-dir=HT.AcceptanceTests`. The `--exclude-dir` is required:
  without it the search enters the AC project's off-limits folders (`CLAUDE.md`).
- The pre-commit architecture gate stays on. Never use `--no-verify`.

---

## Phase 1: Setup (shared infrastructure)

**Purpose**: the roadmap state and a baseline of the public API.

- [X] T001 Set slice 5's Status to `implementing` and `updated_at` to today in `docs/roadmap.md`
- [X] T002 [P] Save the public API baseline before any code change (plan "Constraints", FR-019):
  - run `dotnet restore HabitTracker.slnx`;
  - write `roslynator list-symbols HabitTracker.slnx --projects HT.Application --visibility public --depth member`, and the same for `HT.Domain` and `HT.Persistence`, into three files in the session scratchpad, never in the repo. T094 compares them.

  The scratchpad does not survive a reboot or a later session. If T094 finds the files missing, it regenerates them from the branch's merge base with `master` (`git merge-base HEAD master`) in a temporary `git worktree` under the scratchpad (restore, then the same commands), and removes that worktree afterwards.

**Checkpoint**: `dotnet build HabitTracker.slnx` passes.

---

## Phase 2: Foundational (blocking prerequisites)

**Purpose**: everything every story needs:

- the design changes;
- `HT.Domain.Records`, `HT.Domain.Evaluation` and the `GoalTimeline` members, with their boundary tests;
- the shared Application types, `RecordPreview`, the `IRecordStore` port and its fake;
- the tables, the migration `Records` and `RecordStore`;
- `StatusChip`.

**⚠️ CRITICAL**: no story work starts until this phase is complete.

### Design first (constitution IV, ADR 0008; spec "Design changes before build")

Two kinds of edit, as in slices 1, 2 and 4:

- Texts that disagree with FR-010, HP-004 or `CONTEXT.md` are **replaced** everywhere ("Within limit", "Provisional"
  on the Reduce rule).
- A **slice-5 state** is **added** beside the full preview, as a "Slice 5 state" block after it. The full previews
  stay whole, because they are the design source of slices 6 to 11. "No …" in a task below describes the slice-5
  state; it never deletes anything from the full preview.

- [X] T003 [P] Add the slice-5 state to `design/pages/PageGoalDetail.html` and `.md` (HP-001, US3-AS8, US4-AS7, US5-AS5):
  - the page head with the primary action "Record" and no other action;
  - slice 4's tags, Notice and Settings card, unchanged;
  - the History card as the full preview builds it: the card head (`ht-inline`) with the title "History" and the muted sentence "Corrections are evaluated with the rules of that date." beside it, then a DataTable with Date, Recorded, Result (StatusChip), Note and the empty-headed actions column (`td.ht-actions`): a commitment row, a per-day row and a per-week row ("10 – 16 Aug"); the first 10 rows, then "Show all N →";
  - the row actions of HP-001: in the actions column, "Add record" on a commitment without a record, "Edit" on one with a record, and "Add record" on a period with fewer than 50 records; in the Recorded cell, each period record as its own button; in the Note cell, "Add note" or the note as a button. Show them on a final row too. In the slice-5 block the Due row's action is "Add record" (HP-001); the full preview keeps its "Record";
  - the layout of the full preview: slice 4's head, tags and Notice on top; the left column with the History card; the right column with the Settings card and then the Recent notes card;
  - no History card and no Recent notes card while the goal has no begun entry (not started, or a current date with no commitment or period yet, such as a per-week goal's first partial week);
  - the Recent notes card in the right column, each note with its date and its StatusChip;
  - no GoalStats, strip, trend, pauses or earlier attempts.

  The `.md` lists the accessible names of contracts/host.md ("12 min, edit record 1 of Tue 17 Nov", "Add record for Tue 17 Nov", "Edit record of Tue 17 Nov", "Add note for Tue 17 Nov", the note's text then ", edit note of Tue 17 Nov").
- [X] T004 [P] Update `design/components/RecordDialog.html` and `.md`, and `design/components/RecordForms.html` and `.md` (HP-002):
  - remove "Excuse instead" from the slice-5 state. The full preview keeps it, still marked "Provisional", until slice 7;
  - remove the "Provisional" mark from the Reduce rule, which FR-013 confirms, and keep its text "At most 2. Enter 0 if you had none: an empty day cannot prove you stayed under the limit.", with "No" in place of 0 for yes/no;
  - the "For" select: opened from "Record", FR-011's entries ("Today, Fri 30 Oct", "Wed 28 Oct · Pending, 22 h left"); for a per-week entry, a second select of that week's dates up to the goal's current date; opened from a row, "For" fixed to that row;
  - "Note (optional)", empty, with the help "An empty field keeps the existing note.";
  - the "Edit" state for one record: its date fixed, its value filled, and "Delete record";
  - `RecordDialog.md`'s "For" rule "today plus every commitment still Pending" becomes "every Due and every Pending commitment or period (FR-011); none on an unscheduled day".

  The design step settles the texts the spec leaves open, in the slice-5 state of `RecordDialog.html`, `RecordForms.html` and their `.md`. T024 and T045 take them from there as written:
  - the title: "Record · {goal name}" ("Record · Reading, 20 min daily"), as the NoteDialog's title names the goal (HP-003), in place of the preview's habit name;
  - the "For" label of a per-week entry: its `WeekRange` text ("26 Oct – 1 Nov"), with " · Pending, N h left" when Pending; "Today, " only for the Due entry whose single date is the goal's current date;
  - the sub line, as the three previews show it ("Build · at least 20 min · every day", "Build · Mon · Wed · Fri", "Reduce · at most 2 cups a day"):
    - a Reduce goal: "Reduce · " and slice 4's `GoalText.TargetText` ("Reduce · at most 2 cups a day", "Reduce · at most 1 sweet a day", "Reduce · avoided every day");
    - a Build count or duration goal: "Build · at least ", slice 4's `GoalText.AmountText` ("20 min", "20 pages"), " · " and the schedule part;
    - a Build yes/no goal: "Build · " and the schedule part;
    - the schedule part: "every day" for all seven days or 1 time a day; the day groups of `GoalText.ScheduleText` as they are ("Mon · Wed · Fri", "Mon–Fri"); "N times a day"; "once a week" or "N times a week" for a per-week goal, without `ScheduleText`'s "Flexible · ". Nothing is lower-cased by code;
  - the field labels: "Did you do it?" for yes/no (HP-002), "Time spent" for duration (the preview's), and "Amount" for count, in place of the preview's "Cups today", which is wrong for a Pending day or a week;
  - the partial-progress sentence for each Build count and duration schedule kind, starting from the design's "Partial time is kept. Below 20 min the day counts as Failed when it is finalized."; no sentence for yes/no or Reduce goals;
  - the Reduce help: for count and duration, "At most {AmountText without 'at most '}. Enter 0 if you had none: an empty day cannot prove you stayed under the limit." ("At most 2 cups. …"); for yes/no, "Choose No if you avoided it: an empty day cannot prove you stayed under the limit.";
  - the progress line and its `ht-progress` bar, filled to `Value / Of` and capped at full: none for a yes/no commitment, whose field already shows the answer (plan "Resolved in the spec" 10); none for a count or duration commitment while it has no value (`Value` null: nothing entered yet, or an entry out of range); "1 of at most 2 times" for a Reduce yes/no period;
  - count amounts with the unit as stored, never inflected, as `GoalText.AmountText` does: "3 sweet", "1 of at most 2 cups". No plural rule exists, and none is added.
- [X] T005 [P] Update `design/components/StatusChip.html` and `.md`, `design/components/CommitmentRow.html`, `design/pages/PageHomeSignedIn.html` and `design/tokens.json` (HP-004, constitution I):
  - "Within limit" becomes "Met" everywhere, the `met` token's `usage` text in `tokens.json` included. `grep -rn "Within limit" design src` finds nothing afterwards;
  - add "Due this week" for a per-week period beside "Due today";
  - `StatusChip.md` says that Upcoming ("Due Mon") is not shown in slice 5, and that the text is always shown with the color.

  Keep the classes `ht-chip ht-status ht-status--{met|failed|missed|pending|due}`. Add no new class.
- [X] T006 [P] Update `design/components/NoteDialog.html` and `.md` (HP-003):
  - the entry from a History row's "Add note" and from a row's note;
  - `NoteDialog.md` says, as the HTML shows, that the title is "Add note · {goal name}" and the sub line the date and the state ("Tue 27 Oct · Missed"), not "the habit, date and state";
  - the help "Notes are private. They do not change the result.", "Save note" and "Cancel", and that saving it empty removes the note.
- [X] T007 [P] In `design/pages/PageNewGoal.md`, drop "No 'You can record partial amounts'" from the "Slice 4 state" note (HP-005). The HTML already has the line
- [X] T008 Update `design/pages.md` and `design/README.md` with the slice-5 structure of the goal page, the RecordDialog, the NoteDialog and StatusChip (after T003 to T007)
- [X] T009 Copy every `design/css` file that T003 to T008 changed to `src/HT.Ui/wwwroot/css`, and run `DesignSyncTests` (`tests/HT.UiTests/DesignSyncTests.cs`). If no CSS changed, say so and run the test anyway

### Domain: records (`src/HT.Domain/Records/`, namespace `HT.Domain.Records`; research R-02)

- [X] T010 [P] Create `RecordId.cs` (`readonly record struct RecordId(Guid Value)`), `YesNoAnswer.cs` (`Yes`, `No`), `RecordProblem.cs` (`None`, `NothingScheduled`, `DayNotBegun`, `AnswerMissing`, `CountOutOfRange`, `DurationOutOfRange`, `NoteTooLong`, `RecordLimitReached`, in that order) and `RecordLimits.cs` (`MaxRecordsPerPeriod = 50`, `MaxCount = 9999`, `MaxMinutes = 1440`, `MaxNoteLength = 500`) in `src/HT.Domain/Records/`
- [X] T011 Create `src/HT.Domain/Records/RecordValue.cs` (after T010). A `sealed class` with get-only `Answer` (`YesNoAnswer?`) and `Amount` (`int?`), exactly one set, and a private constructor:
  - `TryCreate(Measurement measurement, YesNoAnswer? answer, int? amount, [NotNullWhen(true)] out RecordValue? value, out RecordProblem problem)`:
    - yes/no keeps the answer and drops an amount; no answer gives `AnswerMissing`;
    - count keeps an amount from 0 to `MaxCount` and drops an answer; otherwise `CountOutOfRange`;
    - duration keeps an amount from 0 to `MaxMinutes` and drops an answer; otherwise `DurationOutOfRange`;
    - only the wrong kind given is "no value" and gives that measurement's problem (FR-004);
  - `FromAnswer(YesNoAnswer)`, which throws for an undefined enum value;
  - `FromAmount(int)`, which throws `ArgumentOutOfRangeException` outside 0 to `MaxCount`, with a message that names no value;
  - value equality.
- [X] T012 [P] Create `src/HT.Domain/Records/GoalRecord.cs` (`sealed record GoalRecord(RecordId Id, DateOnly Date, RecordValue Value)`, after T011) and `src/HT.Domain/Records/NoteText.cs`: `Value`, trimmed, 1 to 500 characters; `TryCreate(string? text, out NoteText? note, out bool tooLong)` returns true with no note for null, empty or only spaces, and false with `tooLong` for more than 500 characters after `Trim()` (FR-007; the rule of `AccountName` and `GoalName`)
- [X] T013 [P] Write `tests/HT.UnitTests/Domain/Records/RecordValueTests.cs` and `NoteTextTests.cs` (after T011, T012; research R-14):
  - count 0, 9999, 10000 and −1; duration 0, 1440, 1441 and −1;
  - Yes with an amount on yes/no keeps Yes; an amount with Yes on count keeps the amount; only Yes on count gives `CountOutOfRange`; only an amount on yes/no gives `AnswerMissing`; only Yes on duration gives `DurationOutOfRange`;
  - `FromAmount` at 0, 9999, 10000 and −1, with an exception message that contains no digit of the value;
  - notes of 500 and 501 characters after trimming, "  Back was tired.  " trimmed to "Back was tired.", all spaces, empty and null.

### Domain: evaluation (`src/HT.Domain/Evaluation/`, namespace `HT.Domain.Evaluation`; research R-01)

- [X] T014 [P] Create `EvaluationState.cs` (`Upcoming`, `Due`, `Pending`, `Met`, `Failed`, `Missed`), `GracePeriod.cs` (`Length = TimeSpan.FromHours(48)`, `EndOf(DateTimeOffset ends)`, which requires a UTC offset and returns `ends + Length`), `ProgressKind.cs` (`RecordedValue`, `RecordsReachingAmount`, `YesRecords`, `Total`), `Progress.cs` (`sealed record Progress(ProgressKind Kind, int? Value, int Of)`) and `EntryEvaluation.cs` (`sealed record EntryEvaluation(EvaluationState State, Progress Progress, DateTimeOffset GraceEnds, bool IsFinal)`) in `src/HT.Domain/Evaluation/`
- [X] T015 Create `src/HT.Domain/Evaluation/Evaluator.cs` (after T011, T014). A static class, not `Evaluation`, because the namespace is (research R-01). `Evaluate(GoalTarget target, DateTimeOffset starts, DateTimeOffset ends, IReadOnlyList<RecordValue> records, DateTimeOffset now)`:
  - the state: `Upcoming` when `starts > now`; `Due` when `starts <= now < ends`; `Pending` when `ends <= now < graceEnd`; otherwise final. `now == graceEnd` is final (FR-016);
  - final with no record is `Missed`, for Build and Reduce alike (FR-013). Otherwise `Met` or `Failed` by research R-01's table, with `Of` in minutes for a duration in hours (amount × 60);
  - the progress of research R-01, given in every state: a commitment's `RecordedValue` (the amount, or 1 for Yes and 0 for No, null with no record; `Of` the amount or 1); a Build period's records reaching the amount, or its Yes records, against N; a Reduce yes/no period's Yes records against N; a Reduce count or duration period's total against the amount. A period without records has `Value` 0;
  - a commitment-based target with more than one record is a programming error and throws, naming no value;
  - `IsFinal` is `now >= GraceEnds`;
  - `ProgressOf(GoalTarget target, IReadOnlyList<RecordValue> records)`, public, gives the same `Progress` without instants or a clock. `Evaluate` uses it, and `RecordPreview` (T028) uses it for the RecordDialog's live progress, so no component counts records.
- [X] T016 [P] Write `tests/HT.UnitTests/Domain/Evaluation/EvaluatorTargetTests.cs` (after T015; SC-002, US2-AS4 to AS10, edge cases). For every Build or Reduce × yes/no, count or duration × commitment, per-day or per-week combination, at, just below and just above its target, read after the grace end:
  - Build duration 20: 19 Failed, 20 Met, 21 Met, 45 Met with 45 kept;
  - Reduce count at most 2: 1, 2 Met, 3 Failed; Reduce amount 0: 0 Met, 1 Failed;
  - Build yes/no: Yes Met, No Failed; Reduce yes/no: No Met, Yes Failed;
  - per-week Build yes/no 3 times: 3 Yes Met, 2 Yes and 1 No Failed, 4 Yes Met;
  - per-week Build count "20 pages, 3 times": 20, 25, 12 Failed; 20, 25, 12, 30 Met;
  - per-day Reduce count at most 1: 0 and 1 Met; 1 and 1 Failed;
  - per-day Reduce yes/no at most once: one Yes Met, two Yes Failed, one No Met;
  - per-day Build yes/no 2 times: two Yes Met, one Yes Failed;
  - a duration in hours ("1 h"): 59 Failed, 60 Met, 61 Met;
  - no record: Missed for every combination, Reduce included;
  - the progress of every combination, with `Value` null for a commitment without a record and 0 for a period without records, and `ProgressOf` equal to `Evaluate`'s progress in each case.
- [X] T017 [P] Write `tests/HT.UnitTests/Domain/Evaluation/EvaluatorStateTests.cs` (after T015; SC-002, US2-AS1 to AS3, AS11, AS13, FR-016):
  - for every combination of T016 (Build or Reduce × measurement × schedule kind), a theory over the state at `starts − 1 tick`, `starts`, `ends − 1 tick`, `ends`, `graceEnd − 1 tick`, `graceEnd` and `graceEnd + 1 tick`, with no record, with records just below the target and with records at the target. So both SC-002 boundaries are covered in every combination;
  - a record that reaches the target is Pending, with its progress, until the grace end, then Met (FR-012);
  - the Kyiv clock changes, with the instants of the Conventions: Saturday 24 October 2026 is Pending at `2026-10-26T20:59:59Z` (22:59 Kyiv) and final at `2026-10-26T21:00:00Z` (23:00); Saturday 27 March 2027 is final at `2027-03-29T22:00:00Z` (01:00 Kyiv);
  - `GraceEnds − Ends` is exactly 48 hours in every case;
  - the same records give the same result whatever order the list is in, and whatever the instant of saving (SC-003; the function has no such input).

### Domain: goals (`src/HT.Domain/Goals/`; research R-03)

- [X] T018 Create `src/HT.Domain/Goals/CalendarEntry.cs` (`sealed record CalendarEntry(DateOnly FirstDate, DateOnly LastDate, DateTimeOffset Starts, DateTimeOffset Ends)` with `Key => FirstDate`) and add to `src/HT.Domain/Goals/GoalTimeline.cs`, leaving every existing member unchanged:
  - `EntryOf(Goal goal, DateOnly date)`, from `TryCalendar(goal, date, date)`: the commitment of `date`, the day, or the week that contains `date`, or null for an unscheduled day, a date before the start date, or a day before a per-week goal's first full week;
  - `HasBegun(Goal goal, DateOnly date, DateTimeOffset instant)`: `CurrentDate(goal, instant) is { } current && date <= current`;
  - `TryEntries(Goal goal, DateOnly first, DateOnly last, [NotNullWhen(true)] out IReadOnlyList<CalendarEntry>? entries)`: `TryCalendar`'s commitments or periods as entries, in date order, with the same range rule.
- [X] T019 [P] Write `tests/HT.UnitTests/Domain/Goals/GoalTimelineEntryTests.cs` (after T018; FR-002, US1-AS5, US1-AS6, US4-AS5, edge cases), with `GoalSamples`:
  - a Mon/Wed/Fri goal: a Tuesday gives null, a Wednesday its commitment;
  - a date before the start date gives null;
  - a per-week goal started on a Wednesday: its Wednesday to Sunday give null, its first Monday the week;
  - a Sunday gives the week ending on it, and the next Monday the next week;
  - `HasBegun` for today, tomorrow, and a later date of the current week, at the instants just before and at the local midnight that begins the date;
  - the overlap after a move east: two entries, each with its own instants;
  - `EntryOf(goal, d)` agrees with `TryCalendar(goal, d, d)` for every date of a 28-day sample, and `TryEntries` with `TryCalendar` for the same range.
- [X] T020 [P] Extend `tests/HT.ArchitectureTests/DomainDependencyTests.cs` and `ClockReadTests.cs` (after T010 to T018; research R-13, constitution V): a test names `HT.Domain.Records` and `HT.Domain.Evaluation`, fails when either namespace is empty, and checks that they depend only on `HT.Domain` and the BCL and read no clock, time zone or culture

### Application: texts, views, results and the formatter (`src/HT.Application/Records/`; data-model.md "Application: records")

- [X] T021 [P] Create `src/HT.Application/Records/RecordTexts.cs` with the eight constants of contracts/application-api.md "Texts", copied from spec.md FR-002, FR-005 and FR-007
- [X] T022 [P] Create in `src/HT.Application/Records/`, after T010 to T015: `RecordInput.cs`, `RecordView.cs`, `HistoryEntry.cs`, `GoalHistory.cs`, `RecentNote.cs`, and the outcomes and results `RecordWriteOutcome`/`RecordWriteResult`, `NoteSaveOutcome`/`NoteSaveResult`, `HistoryOutcome`/`HistoryResult`, `RecordableOutcome`/`RecordableResult`, `RecentNotesOutcome`/`RecentNotesResult` in `RecordingResults.cs`, with the members of data-model.md. `GoalHistory` also gets `Target` (`GoalTarget`), the target of the version of its `CurrentDate` (or of the start date when there is none), which only `RecordPreview` and `RecordingText` read, and `ReadAt` (`DateTimeOffset`), the instant the use case read the clock, so the pages format "N h left" without reading one. In this slice every version has the same target (FR-015). Each result has static factories for its refusals (no input value) and its success, as `GoalResults.cs` does (ADR 0005)
- [X] T023 [P] Add `WeekRange(DateOnly first, DateOnly last)` to `src/HT.Application/Accounts/DateTexts.cs`: "10 – 16 Aug" within a month, "28 Sep – 4 Oct" across a month end, and "28 Dec – 3 Jan" across a year end, with an en dash between spaces. Test it in `tests/HT.UnitTests/Application/Accounts/DateTextsTests.cs` under a non-English `CultureScope`
- [X] T024 Create `src/HT.Application/Records/RecordingText.cs` (after T004, T022, T023; HP-004, HP-011, research R-12), static, culture-invariant, taking the goal's measurement and unit from `GoalHistory` and never a goal name. The texts T004 settles are used as T004 words them:
  - `State(EvaluationState, GoalScheduleKind, SchedulePeriod?)`: "Met", "Failed", "Missed", "Pending", "Due today" for a commitment or per-day period, "Due this week" for a per-week period; Upcoming throws, because no page of this slice shows it;
  - `Progress(GoalHistory, Progress)`: "12 of 20 min", "2 of 3 times", "1 of at most 2 cups", "0 of at most 1 sweet", "1 of at most 2 times"; null for a yes/no commitment and for a commitment whose `Value` is null (T004), in which case the dialog shows neither the line nor the bar;
  - `Amount(Measurement measurement, string? countUnit, int amount)`: "12 min", "0 sweet", "3 sweet", "2 cups", with the unit as stored (T004); `Recorded(GoalHistory, RecordView)` gives "Yes", "No" or `Amount`. The smoke check calls `Amount` directly (T090);
  - `EntryDate(HistoryEntry)`: `DateTexts.ShortDate` for one date, `DateTexts.WeekRange` for a week;
  - `Title(string goalName)`, `FieldLabel(GoalHistory)`, `ReduceHelp(GoalHistory)` (null for Build) and `ProgressFill(Progress)` (0 to 100, for the bar): as T004 settles them;
  - `ForLabel(HistoryEntry, DateOnly currentDate, DateTimeOffset now)`, called with `GoalHistory.CurrentDate` and `GoalHistory.ReadAt`: "Today, Fri 30 Oct" for the Due entry whose single date is the current date, the short date for another Due single-date entry, the `WeekRange` text for a Due week, and " · Pending, N h left" after the entry's date text for a Pending one ("Wed 28 Oct · Pending, 22 h left"), N being the whole hours to `GraceEnds`, rounded up (HP-002), so 30 minutes left reads "1 h left";
  - `TargetLine(GoalHistory)`: the sub line of T004, from `GoalHistory.Target`;
  - `PartialNote(GoalHistory)`: the partial-progress sentence of T004, or null.
- [X] T025 [P] Write `tests/HT.UnitTests/Application/Records/RecordingTextTests.cs` (after T024): every text above for each measurement, unit and schedule kind, units stored in the singular and in the plural shown as stored ("0 sweet", "1 cups"), the sub lines "Build · at least 20 min · every day", "Build · Mon · Wed · Fri", "Build · 3 times a week", "Build · at least 20 pages · Mon–Fri", "Reduce · at most 2 cups a day" and "Reduce · avoided every day", the null progress of a count commitment with no value, hours goals in minutes, "Met" for a Reduce goal, a per-week "For" label, N h left at exactly 22 h, 21 h 59 min and 1 minute, and the same results under a non-English `CultureScope`

### Application: the record store port and the progress preview (research R-04, R-05)

- [X] T026 Create `src/HT.Application/Records/IRecordStore.cs` with the members of data-model.md "IRecordStore", and in the same folder `EntrySpan.cs` (`EntrySpan(DateOnly Key, DateOnly FirstDate, DateOnly LastDate)`), `StoredEntry.cs`, `StoredEntries.cs` and `RecordStoreWrite.cs` (outcome `GoalNotFound`, `RecordNotFound`, `LimitReached`, `Written`, with `RecordId?` and `StoredEntry?`). Every member takes `AccountId owner, GoalId goal` first (after T012)
- [X] T027 Create `tests/HT.UnitTests/Fakes/FakeRecordStore.cs` (after T026), on the pattern of `FakeGoalStore`: in-memory records keyed by goal with a stored order; every member checks the owner against a goal list it shares with a `FakeGoalStore`; commitments replace in place; the limit; notes upserted and removed; a `Calls` list so that tests can prove that a refusal made no store call
- [X] T028 Create `src/HT.Application/Records/RecordPreview.cs` (after T015, T022; HP-002, MAJOR-2 of critic-tasks-pass-1): static `Progress(GoalHistory history, HistoryEntry entry, RecordId? replacing, YesNoAnswer? answer, int? amount)`. It rebuilds the entry's other records with `RecordValue.FromAnswer` or `FromAmount`, leaving out `replacing` (the record being edited) and, for a commitment entry, its existing record in every case, because saving replaces it (FR-003). So a commitment never gets more than one value. Then it adds the candidate through `RecordValue.TryCreate` with the goal's measurement when it is valid, and returns `Evaluator.ProgressOf(history.Target, values)`. An invalid or missing candidate gives the progress without it. Test it in `tests/HT.UnitTests/Application/Records/RecordPreviewTests.cs`: a commitment without a record; a commitment with 12 minutes and a candidate of 25 with no `replacing` (US1-AS2), which gives 25 of 20 and does not throw; a Build period with a record being edited, a Reduce total, and an out-of-range candidate. The RecordDialog shows its result through `RecordingText.Progress`, so no component evaluates FR-014
- [X] T029 Add to `tests/HT.ArchitectureTests/UseCaseParameterTests.cs`: every member of `IRecordStore` takes `AccountId` first and `GoalId` second, with no exception. Add to `tests/HT.ArchitectureTests/UiDependencyTests.cs`: no HT.Ui type depends on `IRecordStore` (after T026). `RecordingService`'s rules come with the service in T042

### Persistence: tables, migration and the record store (`src/HT.Persistence/Records/`; research R-05, R-06)

- [X] T030 Create `src/HT.Persistence/Records/RecordRows.cs` (internal `GoalRecordRow` and `GoalNoteRow`) and map them in `src/HT.Persistence/HtDbContext.cs` (after T026; data-model.md "Tables"):
  - `GoalRecords`: `Id` primary key, nonclustered; `GoalId` foreign key to `Goals.Id` with cascade delete; `Date` `date`; `Answer` `tinyint` null; `Amount` `int` null; `Sequence` `bigint` identity; the clustered index (`GoalId`, `Date`, `Sequence`);
  - `GoalNotes`: primary key (`GoalId`, `EntryDate`); `GoalId` foreign key with cascade delete; `Text` `nvarchar(500)`;
  - no check constraint.
- [X] T031 Create `src/HT.Persistence/Records/RecordMapping.cs` (after T030): rows to `GoalRecord` through `RecordValue.FromAnswer` (`Answer` 1 Yes, 0 No) or `FromAmount`, and a note through `NoteText.TryCreate`. A row with both or neither of `Answer` and `Amount`, or an `Answer` other than 0 or 1, is a fault whose exception names the row's identifier only
- [X] T032 Add the migration with `dotnet ef migrations add Records --project src/HT.Persistence` (after T030). Read the generated `Up` and `Down`, check them against T030, and check that `HtDbContextModelSnapshot.cs` changed only by the two tables. Leave the `CA1861` hints in the migration code (`CLAUDE.md`)
- [X] T033 Create `src/HT.Persistence/Records/RecordStore.cs` (internal; after T031, T032), with every member of `IRecordStore`:
  - every write in one transaction that first locks the goal row with `UPDLOCK, HOLDLOCK` `WHERE Id = @goal AND OwnerId = @owner`; no row gives `GoalNotFound` and writes nothing;
  - `AddAsync`: a commitment with a record has its value replaced in place, keeping `Id` and `Sequence`; a period at `limit` records gives `LimitReached` and writes nothing; otherwise insert; a note with text is upserted under `entry.Key`;
  - `ChangeAsync` and `DeleteAsync` act on `WHERE Id = @record AND GoalId = @goal`; no row gives `RecordNotFound`; `ChangeAsync`, like `AddAsync`, upserts a note with text under `entry.Key` and leaves the note alone without one (FR-007);
  - `SaveNoteAsync`: upsert, or delete with no note;
  - every write returns the entry's records (date, then `Sequence`) and note, read in the same transaction;
  - `FindRecordDateAsync`, `ReadAsync(owner, goal, from, to)` and `ReadRecentNoteKeysAsync` are no-tracking reads joined to `Goals` on `OwnerId`; `ReadAsync` reads records dated `from` to `to` and notes keyed `from` to `to`;
  - a database fault becomes an exception that names the SQL error number only, as `GoalStore` does (HP-006).

  Register `IRecordStore` as scoped in `src/HT.Persistence/PersistenceServiceCollectionExtensions.cs`.
- [X] T034 Register `FakeRecordStore` as `IRecordStore` in `tests/HT.UiTests/Components/Pages/PageServices.cs` (`AddGoals`, sharing the `FakeGoalStore`'s goals) and add `<Compile Include="..\HT.UnitTests\Fakes\FakeRecordStore.cs" Link="Fakes\FakeRecordStore.cs" />` to `tests/HT.UiTests/HT.UiTests.csproj` (after T027). T042 adds `RecordingService` there
- [X] T035 [P] Extend `tests/HT.ArchitectureTests/PublicSurfaceTests.cs` and `PersistenceDependencyTests.cs` (after T033): no NodaTime type on the new public types of `HT.Domain` and `HT.Application`; `RecordStore`, `RecordMapping` and the rows are internal
- [X] T036 [P] Write `tests/HT.IntegrationTests/Records/RecordsMigrationTests.cs` on `HT-IT-MIGRATE` (after T032), on the pattern of `PreSliceAccountMigrationTests`: migrate to the migration before `Records`, insert a goal with plain SQL, migrate to the latest, and check the two tables, their keys, the clustered index, the identity and that the goal is unchanged. Create `tests/HT.IntegrationTests/Records/RecordSamples.cs` with prefixed accounts and goals for the story tests

### Shared component (`src/HT.Ui/Components/Shared/`)

- [X] T037 [P] Create `src/HT.Ui/Components/Shared/StatusChip.razor` (after T009, T024; HP-004): parameters `State`, `ScheduleKind`, `Period`; the text from `RecordingText.State`; the classes of contracts/host.md "StatusChip", no other class. Test it in `tests/HT.UiTests/Components/Shared/StatusChipTests.cs`: each state's text and classes, "Due this week" for a per-week period, "Met" for a Reduce goal, and the text always present

**Checkpoint**: `dotnet build HabitTracker.slnx` and the full suite pass. `IRecordStore` is registered with `RecordStore`, and `ValidateOnBuild` passes in the integration and host tests. No `RecordingService` exists yet. Commit.

---

## Phase 3: User Story 1 - Record what happened (Priority: P1) 🎯 MVP

**Goal**: a registered user records Yes or No, or an amount below, at or above the target, for one date of one of
their goals, optionally with a note, from the goal page's "Record".

**Independent Test**: a user with "Reading, 20 min daily" records 12 minutes today, and the result's entry has one
record of 12 minutes, progress 12 of 20 minutes and state Due.

### Tests for User Story 1

- [X] T038 [P] [US1] Create `tests/HT.UnitTests/Application/Records/RecordWorld.cs` (after T042), on the pattern of `GoalWorld`: a `FakeTimeProvider`, the slice-4 fakes, `FakeRecordStore`, a registered user, another registered user, an administrator, and helpers that create a goal at one instant and move the clock
- [X] T039 [P] [US1] Write `tests/HT.UnitTests/Application/Records/RecordTests.cs` (after T038, T042; US1-AS1 to AS8, FR-001 to FR-007):
  - AS1: 12 minutes today on "Reading, 20 min daily" gives `Recorded`, one record, Due, 12 of 20, `IsCorrection` false;
  - AS2: 25 minutes then replaces it: one record, the same `RecordId`;
  - AS3: "Yes" on Monday and Wednesday of a "Yoga, 3 times a week" week: two records, 2 of 3;
  - AS4: 1 and then 2 sweets on "Sugar, at most 1 sweet a day": two records, total 3;
  - AS5: a Tuesday of a Mon/Wed/Fri goal, a date before the start date, and a day before a per-week goal's first week give "Nothing is scheduled on this date." with no store write;
  - AS6: tomorrow on an every-day and a per-day goal gives "You cannot record a day that has not begun."; tomorrow as a Tuesday on a Mon/Wed/Fri goal gives "Nothing is scheduled on this date.";
  - AS7: one test per message of FR-005, one refusal at a time in FR-005 order: date before value, value before note, note before limit; the 51st record of a period gives "This evaluation period already has 50 records." and a changed value is still accepted at 50 (edge case);
  - FR-004: Yes with an amount on a count goal keeps the amount; only an amount on a yes/no goal gives "Choose Yes or No.";
  - AS8: the note "  Back was tired.  " is kept as "Back was tired.";
  - edge case: on a per-week goal, a record for Sunday saved at 23:59 local time and one for that Sunday saved at Monday 00:00 both fall in the week ending on that Sunday; one for Monday saved at Monday 00:00 falls in the next week;
  - FR-007 with a record: no note or a note of only spaces leaves an existing note; a note with text replaces it;
  - every refusal's `Message` contains none of the input's values.
- [X] T040 [P] [US1] Write `tests/HT.UnitTests/Application/Records/RecordableTests.cs` (after T038, T043; US5-AS3, FR-011): the Due entry and the Pending ones, newest first by first date; none on an unscheduled day of a fixed-days goal; none before the goal's start; the previous week Pending until its Wednesday 00:00; two Due entries during the overlap after a move east; `GoalHistory.CurrentDate`; a per-week goal in its first partial week (started on a Wednesday, read on the Thursday), which has a current date but no entry in the window: `Found` with no entries and no store read
- [X] T041 [P] [US1] Write `tests/HT.IntegrationTests/Records/RecordStoreTests.cs` and `RecordRaceTests.cs` on `HT-IT` (after T033, T042; research R-05, R-14):
  - the stored order of several records on one date of a period;
  - replace-in-place for a commitment: the same `Id` and `Sequence`;
  - the 50 limit: the 51st add gives `LimitReached` and writes nothing;
  - a note with a record: upserted with text, unchanged without;
  - the two races, forced with two contexts and a barrier: two concurrent records on one commitment leave one record; two concurrent adds on a period with 49 records leave 50;
  - edge case (slice 4 FR-034): through `RecordingService` over the real stores, a goal whose catalog habit is then Unpublished (set with plain SQL) still records and reads its history with states as before.

### Implementation for User Story 1

- [X] T042 [US1] Create `src/HT.Application/Records/RecordingService.cs` with its first use case (after Phase 2; research R-04):
  - `public sealed class RecordingService` with the constructor `(IRecordStore records, IGoalStore goals, IAccountStore accounts, TimeProvider clock)` and private readonly fields, as `GoalService` has, every one of them read by `RecordAsync`;
  - its private helpers, which every later use case reuses:
    - `CheckAccessAsync`: `NotSignedIn` (anonymous, or no stored account, as `GoalService` does), `NotForAdministrators`, then the goal through `IGoalStore.FindAsync(owner, id)` (`NotFound`). It returns the owner and the `Goal`;
    - `EntryView(Goal goal, CalendarEntry entry, IReadOnlyList<GoalRecord> records, NoteText? note, DateTimeOffset now)`: the `HistoryEntry`, evaluated with `GoalTimeline.VersionOf(goal, entry.FirstDate).Settings.Target` (FR-015), with `GraceEnds` only while Due or Pending, and `IsCorrection = now >= GracePeriod.EndOf(entry.Ends)`;
    - `HistoryOf(Goal goal, DateOnly? currentDate, IReadOnlyList<HistoryEntry> entries)`: the `GoalHistory`, with `Target` (T022);
    - `SpanOf(CalendarEntry)`: the `EntrySpan`;
  - `RecordAsync(ActingUser, GoalId, DateOnly date, RecordInput input, CancellationToken)`, in contracts/application-api.md's order: access; `GoalTimeline.EntryOf` (`NothingScheduled`); `HasBegun` (`DayNotBegun`); `RecordValue.TryCreate`; `NoteText.TryCreate`; then `IRecordStore.AddAsync` with `RecordLimits.MaxRecordsPerPeriod` (`RecordLimitReached`). It returns `Recorded` with the `RecordId`, the entry after the write and `IsCorrection`. A `GoalNotFound` from the store (the goal removed in between) gives `NotFound`.

  In the same task:
  - register it as scoped in `src/HT.Application/ApplicationServiceCollectionExtensions.cs`, and in `tests/HT.UiTests/Components/Pages/PageServices.cs`;
  - add it to `UseCaseParameterTests`' theory of services whose public methods take `ActingUser` first;
  - widen the `IGoalStore` fence in `tests/HT.ArchitectureTests/ApplicationDependencyTests.cs` (`OnlyTheGoalTypesTheThreeHabitServicesAndTheProfileServiceMayDependOnTheGoalStorePort`) to allow `HT.Application.Records.RecordingService`, and rename the test for it (research R-13).
- [X] T043 [US1] Add `GetRecordableAsync(ActingUser, GoalId, CancellationToken)` to `RecordingService.cs` (after T042; FR-011, research R-04): `CurrentDate`; none when it is null; otherwise `TryEntries(goal, current − 9, current)`; when that gives no entry, `Found` with no entries and no store read; otherwise one `ReadAsync` from the first entry's `FirstDate` to the last entry's `LastDate`, and the entries that are Due or Pending, by first date descending
- [X] T044 [P] [US1] Create `src/HT.Ui/Components/Records/RecordFields.razor` (after T009; HP-002, `design/components/RecordForms`): yes/no "Did you do it?" with Yes and No in a `SegmentedControl` and nothing selected; count with the existing `Stepper` and the unit; duration in whole minutes. Each field takes its refusal message and shows it under itself
- [X] T045 [US1] Create `src/HT.Ui/Components/Records/RecordDialog.razor` (after T024, T044; HP-002), on `HtDialog`:
  - opened from "Record": the "For" select of `GetRecordableAsync`'s entries, labelled with `RecordingText.ForLabel`, and for a per-week entry a second select of that week's dates up to the current date;
  - the title `RecordingText.Title` with the goal's name, and the field label `RecordingText.FieldLabel`;
  - the sub line from `RecordingText.TargetLine`; the progress under the field, with its bar (`RecordingText.ProgressFill`), live, as `RecordingText.Progress` of `RecordPreview.Progress` (T028) for the value being entered, so the component counts nothing itself; the partial-progress sentence from `RecordingText.PartialNote`; for a Reduce goal the help text `RecordingText.ReduceHelp`; "N h left" from `GoalHistory.ReadAt`, so the component reads no clock;
  - "Note (optional)", empty, with "An empty field keeps the existing note.";
  - "Save record" and "Cancel"; no "Excuse instead";
  - the result's `Message` under the field it is about, one at a time;
  - focus on the first field on open; Esc and × close it, and focus returns to the opener.

  It calls `RecordAsync` through `UseCaseScope` and raises a "saved" event with the entry.
- [X] T046 [US1] Create `src/HT.Ui/Components/Records/GoalDetailBody.razor` (`@rendermode InteractiveServer`, prerendered; after T043, T045; research R-12) with the goal identifier as its only parameter. Through `UseCaseScope`, it reads the goal with `GoalService.GetAsync` and `GetRecordableAsync`, and renders slice 4's head, tags, Notice and Settings card, the primary action "Record" when `GetRecordableAsync` gives at least one entry, and the RecordDialog. A goal with no current date shows no "Record". Move slice 4's goal-page markup from `GoalDetail.razor` into it, with its texts unchanged, in T003's layout: head, tags and Notice on top, a left column (the History card from T055) and a right column with the Settings card (and the Recent notes card from T075). Record the layout in contracts/host.md (Conventions)
- [X] T047 [US1] Change `src/HT.Ui/Components/Pages/GoalDetail.razor` (after T046): the static shell with `[Authorize]` keeps its lookup and its Not found with status 404 and a title that names no goal, for an administrator, an unknown goal and another account's goal; for the owner's goal it renders `GoalDetailBody` with the identifier
- [X] T048 [P] [US1] Add the help line "You can record partial amounts; the target decides the result." to "How do you measure it?" in `src/HT.Ui/Components/Goals/NewGoalForm.razor` (HP-005), as `design/pages/PageNewGoal.html` shows it. In `tests/HT.UiTests/Components/Pages/NewGoalPageTests.cs`, replace `Assert.DoesNotContain("partial", …)` (line 193) with a check of that line, and keep the check that "excuse" is absent
- [X] T049 [P] [US1] Write `tests/HT.UiTests/Components/Records/RecordDialogTests.cs` (after T045; HP-002, US1-AS9): the fields per measurement, nothing pre-selected for yes/no, the Reduce help text with "No" for yes/no, the note help, the "For" labels with "Pending, N h left", the per-week date select, the live progress, one refusal at a time under its field, focus on open, Esc, and no "Excuse instead"
- [X] T050 [US1] Update `tests/HT.UiTests/Components/Pages/GoalDetailPageTests.cs` for the slice-5 page (after T047): slice 4's head, tags, Notice and Settings card still render; "Record" is present with a Due entry and absent before the goal starts; no other action. Add to `tests/HT.UiTests/Host/GoalsHostTests.cs` a host test that the goal page still gives 404 for another account's goal and for an administrator (slice 4 FR-020, FR-021), and that the prerendered HTML contains "Record"

**Checkpoint**: the full suite passes. In the running app, "Record" on a goal page records today's value. Commit.

---

## Phase 4: User Story 5 - History of a goal (Priority: P1)

**Goal**: a user reads a goal's history over a range: each commitment or period with its state, records, progress,
note and, while not final, its grace end; the goal page shows it in the History card.

**Independent Test**: read 14 days of a goal with Met, Failed, Missed, Pending, Due and Upcoming entries, in date
order, each with its state.

### Tests for User Story 5

- [X] T051 [P] [US5] Write `tests/HT.UnitTests/Application/Records/HistoryTests.cs` (after T053; US5-AS1, AS2, AS4, FR-009, FR-014):
  - 14 days with Met, Failed, Missed, Pending, Due and Upcoming entries, in date order, each with its state, start and end;
  - `GraceEnds` only on Due and Pending entries;
  - records in date order, then stored order;
  - progress: 12 of 20 minutes; a per-week Build period of 3 with 2 reaching records, 2 of 3; a per-day Reduce count period's total and limit;
  - a per-week period that overlaps the range gives all of its records, also those outside it;
  - 367 days gives "Choose a range of 1 to 366 days.", checked after the goal (an unknown goal with a bad range gives "This goal does not exist.");
  - `IsCorrection` per entry;
  - one store read per call with entries; a range with no entries (unscheduled days only, days before the start date, or a per-week goal's days before its first week) gives `Found` with no entries and no store read.
- [X] T052 [P] [US5] Write `tests/HT.IntegrationTests/Records/RecordReadTests.cs` on `HT-IT` (after T033, T053): `ReadAsync` returns the records dated in the range and the notes keyed in it, in date and stored order; a week read from its Monday to its Sunday gets its note and all its records; `GetHistoryAsync` against the real store gives the same entries as against the fake

### Implementation for User Story 5

- [X] T053 [US5] Add `GetHistoryAsync(ActingUser, GoalId, DateOnly first, DateOnly last, CancellationToken)` to `RecordingService.cs` (after T043; FR-009, research R-07): access; `TryEntries` (`RangeOutOfBounds` with `GoalTexts.RangeOutOfBounds`); with no entries, no store read; otherwise one `ReadAsync` from the first entry's `FirstDate` to the last entry's `LastDate`; each entry through `EntryView`; the `GoalHistory` with `CurrentDate` and the entries in date order
- [X] T054 [US5] Create `src/HT.Ui/Components/Records/HistoryCard.razor` (after T037, T053; HP-001, US5-AS5), with the markup of T003's History card: a `section.ht-card` whose head is the `ht-inline` with `h2.ht-card__title` "History" and the muted sentence "Corrections are evaluated with the rules of that date." (not `HtCard`'s `Title`, which cannot render that head), then a `DataTable` with Date (`RecordingText.EntryDate`), Recorded (each record by `RecordingText.Recorded`, side by side, or "—"), Result (`StatusChip`), Note (the note's text, as plain text; US4 makes it a button) and the empty-headed actions column; the begun entries, newest first; the first 10, then "Show all N →", which expands the card in place. With no begun entry, the card is not rendered (T003). Row actions come in US3 and US4. Record the head, the actions column, the note text and the absence in contracts/host.md and research R-12 (Conventions)
- [X] T055 [US5] In `GoalDetailBody.razor`, after `GetRecordableAsync`, call `GetHistoryAsync(D − 365, D)` with its `CurrentDate` D and render the HistoryCard in the left column; a goal with no current date, or with no begun entry, shows no History card. After a save, re-read both (after T054; research R-12)
- [X] T056 [P] [US5] Write `tests/HT.UiTests/Components/Records/HistoryCardTests.cs` (after T054): the card head with its title and sentence, the five columns, the note text in the Note cell, no card without a begun entry, newest first, only begun entries, "—" without a record, a week's "10 – 16 Aug", the first 10 rows and "Show all N →" with N the total, the expansion in place, and no "Show all" with 10 rows or fewer
- [X] T057 [US5] Add to `tests/HT.UiTests/Host/GoalsHostTests.cs` (after T055): the prerendered HTML of the owner's goal page contains the History card's title, its first rows and a StatusChip's text, so the smoke check and the first paint see them (plan, Risks)

**Checkpoint**: the full suite passes. The goal page lists the history. Commit.

---

## Phase 5: User Story 2 - Pending, Met, Failed and Missed (Priority: P1)

**Goal**: every commitment and period has its state at the supplied time: Upcoming, Due, Pending for exactly 48
elapsed hours after its end, then Met, Failed or Missed, with no action by anyone.

**Independent Test**: a daily goal with nothing on day 1, a partial amount on day 2 and the full amount on day 3,
read just before and just after each grace end.

### Tests for User Story 2

- [ ] T058 [P] [US2] Write `tests/HT.UnitTests/Application/Records/StateTests.cs` through `GetHistoryAsync` and `RecordAsync` (after T053; US2-AS1 to AS13):
  - AS1 to AS3: Upcoming, Due, Pending with its grace end, Missed at the grace end and after, with no call in between (HP-010);
  - AS4 to AS10: one representative final case each, the Domain tests T016 covering the full matrix;
  - AS11: a record that reaches the target is Pending with its progress during the grace period, then Met;
  - AS12: a Reduce commitment and a Reduce period with no record are Missed; with "No" or 0, Met;
  - AS13: an account in "Europe/Kyiv", a goal on Saturday 24 October 2026, read at `2026-10-26T20:59:00Z` (Pending, grace end `2026-10-26T21:00:00Z`) and at `2026-10-26T21:00:00Z` (final);
  - edge cases (slice 4 FR-027): after a large move east, the first commitment of the new version that has already ended when the change is made is Pending or final by the same rules from its own end; across a gap or an overlap, two consecutive commitments are each Pending with different grace ends;
  - the independent test of the story: days 1 to 3, just before and just after each grace end.
- [ ] T059 [P] [US2] Write `tests/HT.UiTests/Host/NoBackgroundWorkTests.cs` (HP-010, research R-14): the host's service provider registers no `IHostedService` beyond those of the framework that slices 1 to 4 already have, listed by type name in the test

### Implementation for User Story 2

- [ ] T060 [US2] Fix what T058 and T059 find in `RecordingService.cs`, `Evaluator.cs` or `HistoryCard.razor`. If they pass at once, record that here and change nothing

**Checkpoint**: the full suite passes. Commit.

---

## Phase 6: User Story 3 - Correct a finalized result (Priority: P1)

**Goal**: a user adds a record to a Missed entry, changes a final record, or deletes one, at any time; the result
is evaluated with the rules of that date, as if recorded on time.

**Independent Test**: a Missed commitment gets a record that reaches the target and reads Met; a Met one has its
record changed below the target and reads Failed; both results say they were corrections.

### Tests for User Story 3

- [ ] T061 [P] [US3] Write `tests/HT.UnitTests/Application/Records/CorrectionTests.cs` (after T064, T065; US3-AS1 to AS7, FR-005, FR-006):
  - AS1: a record on a Missed commitment gives Met with `IsCorrection` true;
  - AS2: a Met record changed below the target gives Failed with `IsCorrection` true;
  - AS3: deleting the record of a final commitment gives Missed;
  - AS4: adding the missing "Yes" to a Failed week gives Met;
  - AS5 (SC-003): two goals with the same settings, one recorded on time and one only by correction after its grace period with the same values, have the same state for every entry;
  - AS6: after the owner's time-zone change, a commitment of the old version keeps its start, end and grace end when corrected;
  - AS7: a record identifier of another goal, of another account and of nothing gives "This record does not exist.";
  - deleting the only record of a Due or Pending entry is not a correction; a record saved exactly at the grace end is;
  - change checks no date and checks the note after the value; a change with a note with text replaces the note, and one without leaves it (FR-007); delete checks nothing after the record;
  - a period at 50 records still takes a change and a delete.
- [ ] T062 [P] [US3] Add change and delete to `tests/HT.IntegrationTests/Records/RecordStoreTests.cs` (after T064, T065): a change keeps `Id`, `Date` and `Sequence`; a change with a note with text replaces the entry's note, and one with no note or only spaces leaves it; a delete removes only that record; both give `RecordNotFound` for a record of another goal of the same owner. The matching `ChangeRecordAsync` note cases are T061's

### Implementation for User Story 3

- [ ] T063 [US3] Add two modes to `RecordDialog.razor` (after T045; HP-002):
  - the row mode, opened from a History row's "Add record": "For" fixed to that row, and for a period a select of its dates up to the goal's current date; `RecordAsync` as from the head;
  - the "Edit" mode for one record: its date fixed, its value filled, its progress preview through `RecordPreview` with that record as `replacing`, "Save record" (`ChangeRecordAsync`), "Delete record" (`DeleteRecordAsync`) and "Cancel".
- [ ] T064 [US3] Add `ChangeRecordAsync(ActingUser, GoalId, RecordId, RecordInput, CancellationToken)` to `RecordingService.cs` (after T053; contracts/application-api.md): access; `FindRecordDateAsync` (`RecordNotFound`); the entry of that date; the value; the note; `ChangeAsync`. No date step (plan "Resolved in the spec" 1). It returns `Changed` with the `RecordId`, the entry and `IsCorrection`
- [ ] T065 [US3] Add `DeleteRecordAsync(ActingUser, GoalId, RecordId, CancellationToken)` to `RecordingService.cs` (after T064): access; `FindRecordDateAsync` (`RecordNotFound`); `DeleteAsync`. It returns `Deleted` with the entry and `IsCorrection`
- [ ] T066 [US3] Add the row actions of HP-001 to `HistoryCard.razor` (after T054, T063, T065): in the actions column, "Add record" on a commitment without a record, "Edit" on one with a record, and "Add record" on a period with fewer than 50 records; in the Recorded cell, each period record as a button that opens "Edit"; offered whatever the state, final included. The accessible names of contracts/host.md, each starting with the visible text. A row's "Add record" opens the RecordDialog's row mode (T063)
- [ ] T067 [P] [US3] Extend `tests/HT.UiTests/Components/Records/HistoryCardTests.cs` and `RecordDialogTests.cs` (after T066; US3-AS8): the row actions per row kind and record count, in the actions column, on final rows too, every accessible name, the fixed "For" from a row with a period's dates, and "Edit" with its filled value, its preview and "Delete record"

**Checkpoint**: the full suite passes. In the running app, a Missed row takes a record and turns Met. Commit.

---

## Phase 7: User Story 4 - Notes (Priority: P2)

**Goal**: a private note per commitment or period, added or edited at any time, final ones included, never changing
the state; the 5 most recent notes on the goal page.

**Independent Test**: a Missed commitment gets the note "Away for the weekend." and is still Missed when read back.

### Tests for User Story 4

- [ ] T068 [P] [US4] Write `tests/HT.UnitTests/Application/Records/NoteTests.cs` (after T071; US4-AS1 to AS5, FR-007):
  - AS1: a note on a Missed commitment, which stays Missed;
  - AS2: saving an empty note or one of only spaces removes it;
  - AS3: a week with "Knee was sore." keeps it after a "Yes" with no note or a note of only spaces, and gets "Better today." when that is given (through `RecordAsync`);
  - AS4: 501 characters gives "The note can have at most 500 characters." and changes nothing; 500 are accepted;
  - AS5: any begun date of a week names the week; a date of the current week that has not begun gives "You cannot record a day that has not begun.";
  - an unscheduled date gives "Nothing is scheduled on this date.".
- [ ] T069 [P] [US4] Write `tests/HT.UnitTests/Application/Records/RecentNotesTests.cs` (after T072; US4-AS6, FR-008): notes on 7 commitments give the 5 with the latest dates, newest first, each with its date and state; a week's note is ordered by its Monday and carries its first and last date; at most 6 store reads
- [ ] T070 [P] [US4] Write `tests/HT.IntegrationTests/Records/RecordNoteStoreTests.cs` on `HT-IT` (after T033): `SaveNoteAsync` upserts and deletes; one note per entry key; `ReadRecentNoteKeysAsync` gives the latest keys, newest first

### Implementation for User Story 4

- [ ] T071 [US4] Add `SaveNoteAsync(ActingUser, GoalId, DateOnly date, string? note, CancellationToken)` to `RecordingService.cs` (after T065): access; `EntryOf`; `HasBegun`; `NoteText.TryCreate` (`NoteTooLong`); `IRecordStore.SaveNoteAsync` under the entry's key. It returns `Saved` with the entry
- [ ] T072 [US4] Add `GetRecentNotesAsync(ActingUser, GoalId, CancellationToken)` to `RecordingService.cs` (after T071; research R-04): access; `ReadRecentNoteKeysAsync(owner, goal, 5)`; for each key its entry by `EntryOf` and one `ReadAsync` over that entry; each `RecentNote` with its first and last date, note and state
- [ ] T073 [P] [US4] Create `src/HT.Ui/Components/Records/NoteDialog.razor` (after T009; HP-003), on `HtDialog` and `TextArea`: the title "Add note · {goal name}", the sub line date and state ("Tue 27 Oct · Missed"), at most 500 characters, the help "Notes are private. They do not change the result.", "Save note" and "Cancel", the refusal under the field, and the dialog's focus handling. Opened from a row's note, the TextArea starts with that note's text, so saving without a change keeps it; opened from "Add note", it starts empty. It calls `SaveNoteAsync` through `UseCaseScope`
- [ ] T074 [P] [US4] Create `src/HT.Ui/Components/Records/RecentNotesCard.razor` (after T037, T072; HP-001): each note with its date (a week: `DateTexts.WeekRange`) and its StatusChip; absent when there are none. `RecentNote` carries no schedule kind, so the card takes `ScheduleKind` and `Period` as parameters, which `GoalDetailBody` passes from its `GoalHistory` (the goal's schedule is the same for every note in this slice, FR-015)
- [ ] T075 [US4] Add the Note cell's actions to `HistoryCard.razor` ("Add note", or the note as a button, with the accessible names of contracts/host.md), opening the NoteDialog, and render the RecentNotesCard in the right column of `GoalDetailBody.razor`, read with `GetRecentNotesAsync` and re-read after a save; no card for a goal with no current date (after T066, T073, T074)
- [ ] T076 [P] [US4] Write `tests/HT.UiTests/Components/Records/NoteDialogTests.cs` and `RecentNotesCardTests.cs`, and extend `HistoryCardTests.cs` (after T075; US4-AS7): the title, sub line, help and buttons; saving empty; the 500 limit; focus; the cards' dates and states; no card without notes; the note cell's actions and names; the NoteDialog opened from a note starts with its text, and from "Add note" empty; a Due per-week note reads "Due this week"

**Checkpoint**: the full suite passes. Commit.

---

## Phase 8: User Story 6 - Recording privacy (Priority: P1)

**Goal**: only the goal's owner records, corrects, reads or annotates; another registered user, an administrator
and an anonymous visitor get nothing, and no result reveals that another account's goal or record exists.

**Independent Test**: user A records with a note; user B, an administrator and an anonymous visitor each try every
use case on A's goal and record, and get the refusals of US6.

### Tests for User Story 6

- [ ] T077 [P] [US6] Write `tests/HT.UnitTests/Application/Records/RecordingAccessTests.cs` (US6-AS1 to AS4, SC-004, FR-017, FR-018): for each of the seven use cases, another registered user gets "This goal does not exist.", an administrator "Goals are for registered users." and an anonymous visitor "You are not signed in.", each with no data and no store call beyond the goal lookup; user B changing or deleting A's record on B's own goal gets "This record does not exist."; an account that is not stored gets "You are not signed in."
- [ ] T078 [P] [US6] Write `tests/HT.IntegrationTests/Records/RecordPrivacyTests.cs` on `HT-IT`: every `IRecordStore` member, called with another owner for A's goal, gives `GoalNotFound`, null or empty and writes nothing; a record of another goal of the same owner gives `RecordNotFound`; A's rows are unchanged afterwards, checked with plain SQL
- [ ] T079 [P] [US6] Write `tests/HT.IntegrationTests/Records/RecordLogTests.cs` with `LogCapture` (HP-006, SC-006): run every use case of `RecordingService` and every `IRecordStore` member, refusals and faults included (demo history loading is covered by T083), with a note, values, a unit, a goal name and a habit name that each carry a unique marker, and check that no captured log line or exception message contains any marker
- [ ] T080 [P] [US6] Extend `tests/HT.UiTests/Host/GoalPrivacyHostTests.cs`: B opening A's goal page gets Not found with 404 and none of A's notes or values in the HTML; an administrator likewise

**Checkpoint**: the full suite passes. Commit.

---

## Phase 9: User Story 7 - Demo results (Priority: P2)

**Goal**: `demo4@example.com`, created by demo history loading after demo loading, shows Met, Failed with partial
amounts, Missed, Pending and Due results and notes relative to the run date; the hosted demo is refreshed daily
and the smoke check confirms it.

**Independent Test**: reset the database, run demo loading and then demo history loading at a fixed time, sign in
as `demo4@example.com` and read its goals' histories.

### Tests for User Story 7

- [ ] T081 [P] [US7] Write `tests/HT.UnitTests/Application/Operator/DemoHistoryTests.cs` (after T085; FR-030): `DemoHistory.For(R)` for R on each weekday (seven theory rows): the three goals, start R − 28, their records and notes as FR-030 lists them; every record on a date before R; Yoga's week before week 3 only when R is a Monday; `GoalNames`, `PartialMinutes` and `ReadingNote`
- [ ] T082 [P] [US7] Write `tests/HT.UnitTests/Application/Operator/DemoHistoryLoadingTests.cs` with `FakeDemoHistoryStore` (after T086; US7-AS1, AS3, AS4, AS6): no password, empty, only spaces, and one without a digit give their refusal and no store call; a valid one gives "created", "demo goals added" and "demo records added", also when the store leaves goals out; a taken email gives "already exists" with no goal or record result; R is the date in the default zone; no result text contains a goal, habit, note, unit or amount
- [ ] T083 [P] [US7] Write `tests/HT.IntegrationTests/Records/DemoHistoryLoadingTests.cs` on `HT-IT-SEED` (after T087; US7-AS1 to AS3, AS5): after demo loading and demo history loading at a fixed time, demo4's goals, records and notes are FR-030's, and its histories at the run time show Met, Failed with 12 minutes and 3 sweets, Missed, Pending and Due; a second run gives "already exists" and changes nothing; a goal whose habit is not Published is left out with its records and notes; a write failure rolls back the account, forced as slice 4's `DemoGoalsTests` forces one, with two `DemoNote`s of one goal under the same entry key (the `GoalNotes` primary key), and, with `LogCapture` (HP-006, SC-006), neither the exception message nor any captured log line contains a goal name, habit name, note text or amount of the call; demo loading's results and demo1 to demo3 are unchanged and never name demo4 (FR-031)
- [ ] T084 [P] [US7] Update `tests/HT.UiTests/Host/StartupLogTests.cs` and `tests/HT.UiTests/Operator/StartupTasksDemoTests.cs` (after T089; HP-007, research R-09): demo4's lines come after demo loading's, in contracts/host.md's order; demo loading's lines stay exactly as they are; a refusal gives one "Demo history loading: …" line; a fault gives "Demo history loading failed; the app starts without it" and the start goes on; `No_log_entry_contains_a_demo_custom_habit_goal_or_account_name` also checks `DemoHistory.GoalNames` and the four demo notes of FR-030 (HP-006)

### Implementation for User Story 7

- [ ] T085 [US7] Create in `src/HT.Application/Operator/`: `DemoHistoryGoal.cs`, `DemoRecord.cs`, `DemoNote.cs`, `DemoRecordsResult.cs`, `DemoHistoryLoadingResult.cs` (with `DemoHistoryLoadingOutcome`), `DemoHistoryAccountResult.cs`, and `DemoHistory.cs` with `For(DateOnly runDate)`, `GoalNames`, `PartialMinutes = 12` and `ReadingNote = "Fell asleep early."` (research R-08). Add `DemoRecordsAdded = "demo records added"` to `OperatorTexts.cs`. Goals are built with `DemoGoal.OnCatalogHabit(…, [CatalogHabitState.Published], …)`, values with `RecordValue.TryCreate`, notes with `NoteText.TryCreate`
- [ ] T086 [US7] Create `src/HT.Application/Operator/IDemoHistoryStore.cs` (`CreateDemoHistoryAccountAsync(Email, string password, DemoAccountProfile, IReadOnlyList<DemoHistoryGoal>, CancellationToken)` returning `CreateDemoAccountOutcome`) and `DemoHistoryService.cs` (`DemoHistoryService(IAccountStore, IDemoHistoryStore, TimeProvider, InstallationTimeZone)`, `DemoEmail`, `LoadAsync(string? demoPassword, CancellationToken)` with research R-08's steps), and `tests/HT.UnitTests/Fakes/FakeDemoHistoryStore.cs`. `DemoDataService` and `IDemoDataStore` are not touched (after T085)
- [ ] T087 [US7] Create `src/HT.Persistence/Habits/DemoHistoryStore.cs` (internal; after T031, T086): one transaction that creates the registered user with the profile, each goal whose catalog habit (by name key) is Published with version 1 in the profile's zone and attempt 1, and its records in list order and notes through `RecordMapping`; it leaves other goals out with their records and notes; a taken email gives `EmailTaken` and creates nothing; a fault rolls back and its exception names no goal, habit, note or value. Register `IDemoHistoryStore` in `PersistenceServiceCollectionExtensions.cs` and `DemoHistoryService` in `ApplicationServiceCollectionExtensions.cs`, both scoped, in the same commit
- [ ] T088 [P] [US7] Add the demo-history rules (after T087; research R-13): in `ApplicationDependencyTests.cs`, `IDemoHistoryStore` is used by no HT.Application type outside `HT.Application.Operator`, and the fence that keeps `HT.Application.Operator` from `IGoalStore` stays; in `UiDependencyTests.cs`, no HT.Ui type depends on `IDemoHistoryStore`, and `DemoHistoryService` joins the operator services that only `HT.Ui.Operator` may use
- [ ] T089 [US7] Add the "Demo history loading" step to `src/HT.Ui/Operator/StartupTasks.cs` after demo loading (after T087; HP-007, research R-09): its own scope and `RunStepAsync`; source-generated log methods for `Demo history loading: {Result}`, `Demo records {Email}: {Result}`, and the existing account and goals templates; no custom-habit line; no secret in any line
- [ ] T090 [US7] Add smoke check 6 to `tools/HT.SmokeCheck/` (after T055, T086; HP-008, SC-005, research R-10, contracts/host.md "Smoke check"):
  - `PageReader.ReadGoalLinks(html)` (the `a.ht-goalitem__name` anchors with their `href`) and `PageReader.ReadCardText(html, "History")` (the first `section.ht-card` whose `h2.ht-card__title` is "History", tag-stripped and entity-decoded, up to its `</section>`);
  - `CheckDemoHistoryAsync` in `SmokeCheckRunner.cs` after check 5: sign in on a new client as `DemoHistoryService.DemoEmail`, `GET /goals`, the 3 `DemoHistory.GoalNames`, then Reading's 6 texts and Yoga's 2, as ordinal substrings of the History card text: the state words from `RecordingText.State`, "12 min" from `RecordingText.Amount(Measurement.Duration, null, DemoHistory.PartialMinutes)`, and `DemoHistory.ReadingNote`;
  - the output `PASS demo history demo4@example.com (11 of 11)`, or `FAIL demo history demo4@example.com: <reason>` with the first failing part's reason of contracts/host.md, never a path, name, note or amount;
  - the top-level record `CoverageEntry` in `SmokeCheckCoverage.cs` (contracts/host.md's `SmokeCheckCoverage.CoverageEntry`, corrected there in this task) gains the port type as its first positional member; the existing entries name `IDemoDataStore`; the 4 rows for `IDemoHistoryStore.CreateDemoHistoryAccountAsync`; every positional construction of it (`grep -rnw "CoverageEntry" tools tests --include=*.cs --exclude-dir=HT.AcceptanceTests`) is updated in this task.
- [ ] T091 [US7] Update the smoke-check tests (after T090; research R-14 "Existing tests this slice changes"):
  - `tests/HT.UiTests/SmokeChecks/PageReaderTests.cs`: `ReadGoalLinks` and `ReadCardText`, including a Settings card whose words must not count;
  - `RunnerBehaviourTests.cs`: `GoodSite()` scripts demo4's sign-in, `/goals` and the two goal pages; the line order gains check 6; one case per FAIL reason, none of whose lines contains a scripted name, note or path;
  - `CoverageTests.cs`: reflection over both demo ports' data parameters, with the port column;
  - `tests/HT.UiTests/Host/SmokeCheckHostTests.cs`: the exact list of check names gains `demo history demo4@example.com` and its PASS line against the in-process host; the failing-password case also expects `FAIL demo history demo4@example.com: …`; the forbidden-text list gains `DemoHistory.GoalNames`, the four demo notes of FR-030 ("Fell asleep early.", "Away for the weekend.", "Birthday cake at work.", "Knee was sore.") and the amount texts the History cards show ("12 min", "25 min", "3 sweet");
  - `specs/003-demo-hosting/contracts/smoke-check.md`, in full, with a note citing ADR 0018: "Checks, in order" gains check 6 with its parts and reasons as contracts/host.md gives them; "Output" says a failed `home` does not stop checks 2–6; "Coverage rule" covers both demo ports, `IDemoDataStore` and `IDemoHistoryStore`; its table takes contracts/host.md's columns (Port, Method, Parameter, Type, Check), the existing rows naming `IDemoDataStore`, plus the 4 new rows. Check 5's wording is not changed.
- [ ] T092 [US7] Change `.github/workflows/deploy-demo.yml` (HP-009, research R-11, contracts/host.md "Hosting workflow"):
  - `on.schedule` with `cron: '20 0 * * *'` next to `workflow_dispatch`;
  - a first job `gate` with no `environment`, no Azure sign-in and no secret, and its step "0.1 Check the trigger and the demo end date" with outputs `mode` (`inputs.mode`, or `refresh` on a scheduled run) and `run` (`false` on a scheduled run whose UTC date is after 2027-01-31); with `run` false, the summary "Demo refresh: skipped after 2027-01-31";
  - job `demo`: `needs: gate`, `if: needs.gate.outputs.run == 'true'`, the name `Demo (${{ needs.gate.outputs.mode }})`, a job-level `env.MODE` from `needs.gate.outputs.mode`, and every `inputs.mode` in its steps, `if:` conditions included, replaced by `env.MODE`;
  - unchanged: the concurrency group, the `demo` environment, the steps and the smoke check.

  Check the file with `actionlint` if it is installed; otherwise say that it was not checked offline. Update `specs/003-demo-hosting/contracts/deploy-workflow.md` and `docs/operations/hosted-demo.md` as contracts/host.md lists: the daily refresh, a late or failed run, a refresh by hand, GitHub's 60-day rule with the recovery by "Enable workflow" or `gh workflow enable deploy-demo.yml`, and removing the trigger in "Teardown after 2027-01-31".

**Checkpoint**: the full suite passes, the startup log shows demo4's lines, and the in-process smoke check passes check 6. Commit.

---

## Phase 10: Polish and cross-cutting concerns

**Purpose**: documentation, the API and analyzer comparisons, the accessibility sweep and the manual quickstart.

- [ ] T093 [P] Update the slice 5 status line of `README.md` ("Status") and slice 3's texts that the spec lists, through notes that cite ADR 0018 (spec "Slice 4 requirements and tests this slice changes"): slice 3's "Demo refresh" key entity, SC-006 and HP-018 in `specs/003-demo-hosting/spec.md`. The roadmap rows "When and how hosted demo data is refreshed" and "Hosting services …" already cite ADR 0018 (commit dbc5a50): check them and change nothing unless the implementation differs. Check `CONTEXT.md`: change an entry only if the implementation sharpened one of HP-012's terms
- [ ] T094 Compare the public API of `HT.Domain`, `HT.Application` and `HT.Persistence` with T002's baseline. The only differences allowed are additions, and the new `DateTexts.WeekRange`. Report any other difference as a defect (FR-019)
- [ ] T095 [P] Run `roslynator analyze HabitTracker.slnx --projects HT.Domain HT.Application HT.Persistence`, and `jb inspectcode HabitTracker.slnx --stdout -f=Text -e=WARNING` with its cache in the session scratchpad. Compare both with the `CLAUDE.md` baselines, and fix real findings only; the documented false positives stay. Never point either tool at `tests/HT.AcceptanceTests`
- [ ] T096 [P] Sweep the goal page, the RecordDialog and the NoteDialog for constitution IV and HP-011 (SC-007): tokens and `ht-` classes only, with no literal colors; visible focus; status never by color alone; the dialogs' focus handling; icon-only buttons with an `aria-label`; the accessible names of contracts/host.md; sentence case per `design/brand-book.md`. Then rerun `DesignSyncTests`
- [ ] T097 Run the quickstart's §3 in the running app after dropping the local `HT` database. Report each step's result. SC-001 (3 actions, under 30 seconds) is timed by hand. The browser behaviour of the dialogs beyond bUnit, the real schedule and the hosted smoke check (quickstart §4) stay labelled unverified until the owner checks them
- [ ] T098 Run the full build and `dotnet test HabitTracker.slnx`, and report `PASS` with the counts. Leave slice 5's roadmap Status at `implementing`: `AC tests green` comes after the owner's `/speckit-ac-tests-analyze`. Commit

---

## Dependencies and execution order

### Phase dependencies

- **Setup (Phase 1)**: no dependencies.
- **Foundational (Phase 2)**: after Setup. It blocks every story. Design (T003 to T009) comes before every page task.
- **User stories (Phases 3 to 9)**: after Phase 2, in this order: US1, US5, US2, US3, US4, US6, US7.
- **Polish (Phase 10)**: after the stories that are to ship.

### Story dependencies

- **US1**: none beyond Phase 2.
- **US5**: after US1. `GetHistoryAsync` reuses `GetRecordableAsync`'s reading, and the History card goes into US1's `GoalDetailBody`.
- **US2**: after US5. Its tests read states through the history.
- **US3**: after US5. Its row actions go into the History card, and its tests read corrections back through the history.
- **US4**: after US3. `SaveNoteAsync` follows `DeleteRecordAsync` in `RecordingService.cs`, and the note cell joins US3's row actions.
- **US6**: after US4. Its tests cover all seven use cases.
- **US7**: after US5. Smoke check 6 reads the History card, whose Note cell shows the note's text from T054 on, and its texts come from `RecordingText`. It touches no file of US2, US3, US4 or US6 apart from the shared files below, so it can run beside them once US5 is done.
- **Shared files** need sequential edits, so their tasks never run in parallel with each other:
  - `RecordingService.cs`: T042, T043, T053, T064, T065, T071, T072;
  - `RecordDialog.razor`: T045, T063; `RecordDialogTests.cs`: T049, T067;
  - `HistoryCard.razor`: T054, T066, T075; `HistoryCardTests.cs`: T056, T067, T076;
  - `GoalDetailBody.razor`: T046, T055, T075;
  - `GoalsHostTests.cs`: T050, T057;
  - `RecordStoreTests.cs`: T041, T062;
  - `ApplicationServiceCollectionExtensions.cs`: T042, T087;
  - `PersistenceServiceCollectionExtensions.cs`: T033, T087;
  - `tests/HT.UiTests/Components/Pages/PageServices.cs`: T034, T042;
  - `UseCaseParameterTests.cs`: T029, T042;
  - `ApplicationDependencyTests.cs`: T042, T088; `UiDependencyTests.cs`: T029, T088;
  - `docs/roadmap.md`: T001, T093;
  - `specs/003-demo-hosting/contracts/smoke-check.md`: T091 only.

### Within each story

- A test task starts once the tasks it names as dependencies are done, usually the use case it calls, because a
  test that does not compile cannot fail meaningfully. It then runs in parallel with the story's remaining work.
  The order of the implementation is: the use case, the components, the page, and the host tests.
- The story's checkpoint runs the full suite before the next story, and is its commit point.

### Parallel opportunities

- Phase 1: T002 with T001.
- Design: T003 to T007, then T008, then T009.
- Domain: T010 and T014; then T011, T012; then T015 and T018; then the tests T013, T016, T017, T019 and T020.
- Application foundation: T021, T022 and T023 together; then T024 and T026; then T025, T027 and T028.
- Persistence: T030, T031, T032, T033 in order; then T034, T035 and T036 together; T037 beside them.
- In each story, every task marked [P].
- After US5, US7 can run beside US2, US3, US4 and US6.

---

## Parallel example: Phase 2 Domain

```text
Task: "T010 Record value types in src/HT.Domain/Records/"
Task: "T014 Evaluation types in src/HT.Domain/Evaluation/"

# after T015 and T018:
Task: "T016 Target tests in tests/HT.UnitTests/Domain/Evaluation/EvaluatorTargetTests.cs"
Task: "T017 State and clock-change tests in tests/HT.UnitTests/Domain/Evaluation/EvaluatorStateTests.cs"
Task: "T019 Entry tests in tests/HT.UnitTests/Domain/Goals/GoalTimelineEntryTests.cs"
Task: "T020 Domain namespace rules in tests/HT.ArchitectureTests/"
```

## Parallel example: User Story 1

```text
# after T042 and T043:
Task: "T039 [US1] Record tests in tests/HT.UnitTests/Application/Records/RecordTests.cs"
Task: "T040 [US1] Recordable tests in tests/HT.UnitTests/Application/Records/RecordableTests.cs"
Task: "T041 [US1] Store and race tests in tests/HT.IntegrationTests/Records/"
Task: "T044 [US1] RecordFields in src/HT.Ui/Components/Records/RecordFields.razor"
Task: "T048 [US1] New goal help line in src/HT.Ui/Components/Goals/NewGoalForm.razor"
```

## Parallel example: User Story 6

```text
Task: "T077 [US6] Access tests in tests/HT.UnitTests/Application/Records/RecordingAccessTests.cs"
Task: "T078 [US6] Store privacy tests in tests/HT.IntegrationTests/Records/RecordPrivacyTests.cs"
Task: "T079 [US6] HP-006 log tests in tests/HT.IntegrationTests/Records/RecordLogTests.cs"
Task: "T080 [US6] Host privacy tests in tests/HT.UiTests/Host/GoalPrivacyHostTests.cs"
```

## Parallel example: User Story 7

```text
# after T085 to T089:
Task: "T081 [US7] DemoHistory tests in tests/HT.UnitTests/Application/Operator/DemoHistoryTests.cs"
Task: "T082 [US7] Demo history loading order in tests/HT.UnitTests/Application/Operator/DemoHistoryLoadingTests.cs"
Task: "T083 [US7] Demo history on HT-IT-SEED in tests/HT.IntegrationTests/Records/DemoHistoryLoadingTests.cs"
Task: "T084 [US7] Log order in tests/HT.UiTests/Host/StartupLogTests.cs and Operator/StartupTasksDemoTests.cs"
Task: "T092 [US7] Workflow schedule and gate in .github/workflows/deploy-demo.yml"
```

---

## Implementation strategy

### MVP first (the P1 stories: US1, US5, US2, US3)

1. Phase 1 and Phase 2: design first, then Domain with its boundary tests, the Application and Persistence
   foundation, and StatusChip.
2. US1: recording from the goal page's "Record".
3. US5: the history and the History card.
4. US2: the states proven through the history, with no background work.
5. US3: corrections from the History rows.
6. **Stop and validate**: run the full suite, then check in the browser that a day is recorded, turns Pending,
   and can be corrected.

### Incremental delivery

1. US4 and US6: notes, then privacy proven for all seven use cases (SC-004, SC-006).
2. US7: demo history loading, the startup step, smoke check 6 and the daily schedule.
3. Polish. Then the owner runs `/speckit-ac-tests-write` and `/speckit-ac-tests-analyze` for F005, with F001, F002
   and F004 expected to pass unchanged (FR-019).

One PR for the slice. Each checkpoint is a commit on the feature branch.

---

## Notes

- [P] tasks touch different files, and start once the tasks they name as dependencies are done (Format).
- The AC project, its features, the locks and the run records are never edited.
- The ten points of plan.md "Resolved in the spec" are in the spec; the tasks implement them as stated.
- The task order differs from the plan's Work order in one way: the plan orders by layer (Application and
  Persistence, then host, then pages), these tasks order by story, and keep the layer order inside each story.
  Phase 2 does the plan's steps 1 and 2 in full, and the parts of steps 3 and 4 that every story shares, so the
  `ValidateOnBuild` pairing of the plan holds at every checkpoint.
- `GetRecordableAsync` (FR-011, US5-AS3) is built in US1, because it is what the RecordDialog's "For" offers.
- These details were settled while writing and reviewing the tasks ([critic-tasks-pass-1.md](critic-tasks-pass-1.md)), within the design artifacts:
  - `RecordingService` is created in US1 (T042), not in Phase 2: a service without a public method fails `UseCaseParameterTests`, and unread constructor parameters fail the build (MAJOR-1);
  - the RecordDialog's live progress comes from `RecordPreview.Progress` (T028), which calls the new `Evaluator.ProgressOf` (T015) with the new `GoalHistory.Target` (T022), so no component evaluates FR-014 (MAJOR-2);
  - a range or window with no entries gives `Found` with no entries and no store read (T043, T053);
  - the History card follows `design/pages/PageGoalDetail.html`: the sentence "Corrections are evaluated with the rules of that date." sits in the card head and the row actions in a fifth column. The design governs (constitution IV, T003), and T054 replaces contracts/host.md's and research R-12's "under the table" with it;
  - the RecordDialog's title, labels, sub line, help texts, progress bar and per-week "For" label, and count units shown as stored with no plural rule, are settled by the design step T004 ([critic-tasks-pass-2.md](critic-tasks-pass-2.md) MINOR-2 to MINOR-4);
  - `RecordPreview` always leaves a commitment's existing record out, because saving replaces it (pass 2 MAJOR-1);
  - the goal page takes the full preview's two columns, and shows no History card without a begun entry (T003, T046, T054);
  - every addition above is recorded in data-model.md, the contracts or research.md by the task that makes it (Conventions);
  - the RecentNotesCard takes the schedule kind from `GoalHistory`, because `RecentNote` carries none (T074);
  - `RecordingText.State` throws for Upcoming, because no page of this slice shows it (HP-004);
  - `Evaluator.Evaluate` throws for a commitment with more than one record, which the store prevents;
  - `DateTexts.WeekRange` also covers a week across a year end ("28 Dec – 3 Jan");
  - US2's Application tests sample one case per scenario, because T016 and T017 carry the full SC-002 matrix.
- Stop at any checkpoint to validate a story on its own.
