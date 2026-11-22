---

description: "Task list for slice 8, goal changes"
---

# Tasks: Goal changes

**Input**: Design documents from `specs/009-goal-changes/`: [plan.md](plan.md), [spec.md](spec.md),
[research.md](research.md), [data-model.md](data-model.md), [contracts/application-api.md](contracts/application-api.md),
[contracts/host.md](contracts/host.md), [quickstart.md](quickstart.md); [ADR 0025](../../docs/adr/0025-goal-change-rules.md),
[ADR 0026](../../docs/adr/0026-demo-goal-change-loading.md), [ADR 0016](../../docs/adr/0016-time-zones.md),
[ADR 0018](../../docs/adr/0018-evaluation-on-read-and-demo-history.md), [ADR 0020](../../docs/adr/0020-derived-figures-on-read.md),
[ADR 0024](../../docs/adr/0024-excusal-and-pause-rules.md).

**Prerequisites**: `acceptance.md` and `tests/HT.AcceptanceTests/Features/F009/` are locked. No task edits them,
the lock files, the run records or anything under `tests/HT.AcceptanceTests`.

**Tests**: required. The constitution (II, III, V) and `CLAUDE.md` make unit, integration, architecture and
selective UI tests part of done, so every story phase has test tasks. xUnit v2 everywhere, hand-written fakes,
`FakeTimeProvider`, no mocking library (ADR 0006). Acceptance step definitions are not tasks here: the owner starts
`/speckit-ac-tests-write` and `/speckit-ac-tests-analyze` after `/speckit-implement` (plan, Work order 9).

**Organization**: Phase 2 holds what several stories share:

- the whole design step and ADR 0027 (plan, Work order 1);
- the whole Domain step: attempts, endings, the attempt-aware `GoalTimeline` and its new members, names and the
  trend, with their boundary and equivalence tests (Work order 2);
- the storage that every story reads and writes: the migration, the four `IGoalStore` members, the `IRecordStore`
  overload, the pause re-check, every implementer of both ports (Work order 3 and 4, store part);
- the shared read path: per-entry settings (`EntrySettings`, `GoalHistory.SettingsOf`, the `RecordPreview`,
  `RecordingText` and `ReportText` overloads), `EntryViews`, `GoalEvaluation`'s attempt overload, `EarlierPending`
  and `Recordable`, and the attempt-aware reads of `RecordingService`, `ReportService`, `GoalService`,
  `GoalListItems`, `CatalogService`, `PauseService` and `ExcusalService` (Work order 3, shared part);
- the shared components that take each row's own settings: `HistoryCard`, `RecordDialog`, `NoteDialog`,
  `CommitmentRow` and `GoalItem` (Work order 6, R-07 part).

After Phase 2 every read already respects attempts, endings and settings versions for goals seeded through the
`Goal` constructor. Each story phase then adds its use cases, their tests, its components and its pages.

The plan's rule that steps 3 and 4 are verified and committed together exists so that `ValidateOnBuild` never
meets a registered service whose port has no implementation, and so that a port member never exists without its
implementations. The tasks keep that rule at a finer grain: each port lands in the same commit as all its
implementers (Phase 2 for `IGoalStore` and `IRecordStore`, US6 for `IDemoGoalChangeStore`), and `GoalChangeService`
and `AttemptService` use only ports that already have one.

The story phases follow the spec's priorities, with two moves, as in slice 7:

- US5 (P1, privacy) comes after US4, because its tests cover all seven use cases of US1 to US4.
- US6 (P2, demo) comes last, because its smoke check reads the Ended tab (US4), the attempt line (US2), the
  Settings card's "From …" row (US1) and the Profile's Goal history (US4).

US1, US2 and US3 are all P1. They are ordered by dependency: US3's starting values and new attempts need an ended
goal, which US2's use case makes; US2's goal page builds on US1's head actions.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: the task can run in parallel with the other [P] tasks of its group (they touch different files), once
  the tasks it names as dependencies are done.
- **[Story]**: the user story from spec.md (US1 to US6).
- Paths are relative to the repository root.

## Conventions for every task

- Build with `dotnet build HabitTracker.slnx`. `TreatWarningsAsErrors` is on, so a warning is a failure.
- Use the names in [contracts/application-api.md](contracts/application-api.md) and [data-model.md](data-model.md).
  Every use-case method and every port method is `async` and takes a `CancellationToken` last. The Domain functions
  and the pure formatters (`ReportText`, `GoalChangeTexts`, `RecordingText`, `DemoGoalChanges.For`) are synchronous.
  Services use an explicit constructor with private fields, not a primary constructor (an unread
  primary-constructor parameter is CS9113, an error here).
- The design artifacts stay the contract (constitution, Governance: "never by silent divergence"). A task that adds
  a public member, or settles a text or layout the design artifacts leave open, records it in the same task, in the
  document it names.
- Take message texts from the constants: `GoalChangeTexts` (new, T042), and the existing `GoalTexts`,
  `RecordTexts`, `AccountTexts`, `OperatorTexts`, `PauseTexts`. Never retype one. Tests assert the literal text from
  spec.md, so a typo in a constant fails a test. "Nothing is scheduled on this date." stays in `RecordTexts`, and
  "The note can have at most 500 characters." is `RecordTexts.NoteTooLong` (contracts/application-api.md "Texts").
- Every outcome word, "Ended …", attempt line, "From …", "Changes apply from …", "Attempt {n} starts with …" and
  empty-tab text comes from `ReportText` or `GoalChangeTexts` (HP-008). No component builds a text or computes a
  figure. Date texts come only from `DateTexts`; numbers use the invariant culture.
- Time and culture stay out of `HT.Domain` and `HT.Application` (constitution II; `ClockReadTests`):
  - time enters only as `TimeProvider` in the services, read once per call as `_clock.GetUtcNow()`, and as an
    instant or date parameter in Domain;
  - **the account's today** is the date of that instant in the account's stored zone, else the installation
    default. **The ending date** is the goal's current date (`GoalTimeline.CurrentDate`), or the account's today for
    a goal that has not started (FR-007). Each service computes each in one place;
  - never add days in local time. Last dates, boundaries and start dates are dates on the goal's own timeline
    (ADR 0016).
- No NodaTime type appears in any public or protected signature of `HT.Domain` or `HT.Application` (ADR 0017).
- Goal changes are private (constitution III, FR-022, FR-023):
  - they are read and written only through the owner-scoped `IGoalStore` and `IRecordStore` members, always passing
    the acting user's `AccountId` as the owner and the goal named in the call. An attempt is found only by number
    among that goal's attempts; there is no attempt identifier;
  - the checks run in the order: no account id, role, stored account, goal. Administrators get nothing of any
    account;
  - no ending note, note, recorded value, unit, goal name, habit name or outcome goes into a log template, an
    exception message or an error page (HP-010). `GoalChangeService`, `AttemptService` and the new store members
    log nothing. `EndingNote.ToString` returns a fixed placeholder. A refusal carries no input value.
- Nothing derived is stored or cached (FR-026, ADR 0018, ADR 0020). Last dates, states, names and figures are
  computed on read through `GoalTimeline` and the one `GoalEvaluation`. The tables hold versions, attempts and
  endings only.
- **Locked AC code** (FR-024, plan "Constraints" and "Risks"): no existing constructor, method signature,
  positional record or port member of `HT.Domain`, `HT.Application` or `HT.Persistence` changes. Only these are
  added to existing types:
  - enum members (`GoalProblem.StartDateNotAfterEnding`, `PauseOutcome.GoalEnded`,
    `ExemptionStoreWriteOutcome.GoalEnded`, `RecordWriteOutcome.NothingScheduled` if it is not already a member;
    check `src/HT.Application/Records/RecordingResults.cs` first);
  - init-only properties that are not `required` (`GoalAttempt.Ending`, `HistoryEntry.AttemptNumber`,
    `IsLatestAttempt`, `AttemptName`, `Settings`, the `GoalDetails` and `GoalListItem` fields of data-model.md,
    `TodayEntry.GoalEnded`);
  - overloads (`GoalTimeline.EntriesThrough`, `VersionInEffect`, `GoalText.Name`, `RecordPreview.Progress`, the
    seven `RecordingText` members, `ReportText.ProgressText`, `GoalEvaluation.OfAsync`,
    `IRecordStore.ReadRecentNoteKeysAsync`), the second `Goal` constructor and the second `CatalogService`
    constructor (`CatalogService` has one today, `CatalogService.cs:25`);
  - new port members and new types.

  A new public type that data-model.md does not list is recorded there by the task that creates it. If a task seems
  to need more, stop and ask the owner. The locked F001, F002, F004, F005, F007 and F008 AC code must keep compiling,
  and nobody reads it. The smoke check tool (`tools/HT.SmokeCheck`) is outside that rule.
- Existing members keep their meaning for a goal with one attempt, no ending and no change (R-01, R-02). A test that
  fails after a Domain or Application change is a regression to fix, unless this file names it as a test that
  changes on purpose (T023, T024, T046, the absence lists of the UI tasks and the startup and smoke-check lines of
  US6).
- Test instants are UTC literals worked out by hand from the spec's local times, never computed by the code under
  test. The spec's scenarios use zone UTC unless they say otherwise. For example:
  - US1 ("Reading, 15 min daily", from Monday 2 November 2026): a change at `2026-11-11T10:00:00Z` applies from
    Thursday 12 November; a change at `2026-11-11T23:59:59Z` still applies from 12 November, and one at
    `2026-11-12T00:00:00Z` from Friday 13 November (SC-005). The name is "Reading, 15 min daily" at
    `2026-11-11T23:59:59Z` and "Reading, 20 min daily" at `2026-11-12T00:00:00Z` (AC-001, AC-002);
  - US2 ("Meditation, 10 min daily", ended at `2026-11-16T10:00:00Z`): 14 November's grace period ends at
    `2026-11-17T00:00:00Z` and 15 November's at `2026-11-18T00:00:00Z`, so at `2026-11-16T10:01:00Z` both are
    Pending and at `2026-11-18T12:00:00Z` both are Missed;
  - US3-AS13 (Europe/Kyiv, UTC+2): the time zone change at `2026-11-16T23:30:00Z` (Tuesday 17 November 01:30 in
    Kyiv) and the ending at `2026-11-16T23:40:00Z` give the ending date Tuesday 17 November while the account's
    today in UTC is Monday 16 November;
  - a per-week change on Sunday 22 November at `2026-11-22T23:59:59Z` applies from Monday 23 November, and one at
    `2026-11-23T00:00:00Z` from Monday 30 November.
- Goals with past results are created at an earlier supplied time, then recorded, changed, ended, restarted and
  read at later ones, by moving a `FakeTimeProvider` forward. A goal's start date is never in the past (slice 4),
  so a past change or ending in a test is made while it was current and then read later. Only Phase 2's seeded
  goals (T051) and demo goal-change loading write past facts directly.
  - In unit tests each test owns its world and its clock.
  - In integration tests, never set or advance the fixture's shared `Clock` (`DatabaseFixture.Clock`,
    `SeedDatabaseFixture.Clock`). A test that needs another time builds its own provider with its own
    `FakeTimeProvider` over the fixture's `ConnectionString`, as `Records/RecordReadTests.BuildServices` does. On
    `HT-IT-SEED`, such a test first resets the database with `HtDatabase.ResetAsync`, as `ResetAndBuildAsync` does.
- Pages use only `design/` tokens and `ht-` classes, and follow the structure and wording of their `design/`
  preview after the design tasks of Phase 2 (constitution IV). Parts of slices 9 to 11 are absent, not disabled
  (ADR 0008): no Community tab on the Profile.
- Interactive components call use cases only through `UseCaseScope` (slice 2 research R-15). An island reads its
  own data in `OnInitializedAsync`, and reads everything again after any write, so the Notice, the cards and the
  figures agree. A static page never passes a view into an island (slice 6 research R-12); it may pass a query value
  such as the selected tab.
- Every dialog of this slice uses `HtDialog`, and every form follows the form conventions of slices 4 and 7: the
  first field gets focus on open, Esc and × close, and focus returns to the opener, or, when a re-read removed the
  opener, to the nearest remaining control (HP-009): after an ending, the Notice's "Start new attempt". bUnit tests
  check it with `JSInterop.VerifyFocusAsyncInvoke`.
- Every page and component of an entry reads that entry's settings through `GoalHistory.SettingsOf(entry)` and the
  `EntrySettings` overloads, never the goal-level fields of `GoalHistory` (R-07). The goal-level fields stay for
  what is about the goal as a whole (page head, `CurrentDate`, `ReadAt`). `RecentNotesCard` keeps the goal-level
  schedule kind (plan "Scale/Scope").
- Never read, print or embed a secret. That includes the `Operator:*` User Secrets values.
- No `InternalsVisibleTo` in `HT.Domain`, `HT.Application` or `HT.Persistence` (ADR 0010; `AcSurfaceTests`).
  `EntryViews`, `GoalEvaluation`, `GoalListItems` and `DemoAccountWriter` are internal and are tested through the
  public use cases and stores.
- Databases:
  - integration tests use `HT-IT` (`DatabaseFixture`), `HT-IT-SEED` (`SeedDatabaseFixture`) and the migration
    fixture (`MigrationDatabaseFixture`);
  - host tests use `HT-UI` and its derived databases;
  - no test touches `HT` or `HT-AC`.

  Tests in shared databases use account emails, habit names and goal names with their own prefix. They check by
  inclusion, exclusion and relative order, never by totals.
- Commit only at a checkpoint, with the full suite green (constitution V). These land in the same commit, so
  `ValidateOnBuild` and the compiler never meet an unregistered service or an unimplemented port member:
  - the four `IGoalStore` members, `FakeGoalStore`'s, `GoalStore`'s and those of the two other implementers in the
    tests, `RacingGoalStore` (`tests/HT.IntegrationTests/Goals/TimeZoneChangeTests.cs`) and `BeforeFirstCreate`
    (`tests/HT.UnitTests/Application/Goals/GoalWorld.cs`) (Phase 2, T033, T035, T037, T038);
  - the `IRecordStore` overload and `GoalEnded` outcome, `FakeRecordStore`'s, `RecordStore`'s and the two
    `FaultingRecordStore`s in `tests/HT.IntegrationTests/Records/RecordLogTests.cs` and `ExemptionLogTests.cs`
    (Phase 2, T034, T036, T039);
  - `GoalChangeService`, its registration and its bUnit registration (US1, T061);
  - `AttemptService`, its registration, its bUnit registration and the widened `IGoalStore` rule (US3, T083);
  - `DemoGoalChangeService`, `IDemoGoalChangeStore`, `FakeDemoGoalChangeStore`, `DemoAccountWriter`'s attempts,
    `DemoGoalChangeStore`, their two registrations and the port's fences (US6, T105 to T108).

  The bUnit services in `tests/HT.UiTests/Components/Pages/PageServices.cs` are registered by hand and are not built
  with `ValidateOnBuild`. They hold what the pages resolve: `GoalChangeService` and `AttemptService` join them. Plan
  Work order step 3 also names `FakeDemoGoalChangeStore` there; it lives in `HT.UnitTests`, no page resolves it,
  and slices 6 and 7 kept every demo store out, so it is not added (the same reading as slice 7's tasks).
- A search for callers or tests of a type excludes the AC project:
  `grep -rnw "<Type>" tests --include=*.cs --exclude-dir=HT.AcceptanceTests`. The `--exclude-dir` is required:
  without it the search enters the AC project's off-limits folders (`CLAUDE.md`).
- The pre-commit architecture gate stays on. Never use `--no-verify`.

---

## Phase 1: Setup (shared infrastructure)

**Purpose**: the roadmap state and a baseline of the public API.

- [X] T001 Set slice 8's Status to `implementing` and `updated_at` to today in `docs/roadmap.md`, in the first code commit (plan, Work order 1, "Roadmap status"). `/speckit-tasks` set it to `tasks ready`
- [X] T002 [P] Save the public API baseline of `HabitTracker.slnx` before any code change (plan "Risks", FR-024):
  - run `dotnet restore HabitTracker.slnx`;
  - write `roslynator list-symbols HabitTracker.slnx --projects HT.Application --visibility public --depth member`, and the same for `HT.Domain` and `HT.Persistence`, into three files in the session scratchpad, never in the repo. T117 compares them.

  If T117 finds the files missing (a reboot or a later session), it regenerates them from the branch's merge base with `master` (`git merge-base HEAD master`) in a temporary `git worktree` under the scratchpad, running `dotnet restore HabitTracker.slnx` there first (Roslynator needs it, `CLAUDE.md`), and removes that worktree afterwards.

**Checkpoint**: `dotnet build HabitTracker.slnx` passes.

---

## Phase 2: Foundational (blocking prerequisites)

**Purpose**: everything several stories need: the design changes and ADR 0027; the Domain with its boundary and
equivalence tests; the storage; the shared read path; the shared components on each row's settings.

**⚠️ CRITICAL**: no story work starts until this phase is complete.

### Design first (constitution IV, ADR 0008; spec "Design changes before build")

Two kinds of edit, as in slices 1, 2 and 4 to 7:

- Texts that disagree with the spec or `CONTEXT.md` are **replaced** everywhere (for example `EndGoalDialog`'s
  "Achieved" marked as chosen when it opens).
- A **slice-8 state** is **added** beside the full preview, as a "Slice 8 state" block after the slice-7 one. The full
  previews stay whole, because they are the design source of slices 9 to 11. "No …" in a task below describes the
  slice-8 state; it never deletes anything from the full preview.

- [X] T003 [P] Add the slice-8 state to `design/pages/PageGoalDetail.html` and `.md` (HP-001, contracts/host.md "Goal page"): in the head of a goal that is not ended the ghost "Edit settings" first, "Pause", then the ghost "End goal" (`aria-haspopup="dialog"`); the attempt line under the title in its three forms, "Attempt 2 · started 18 Nov", "Attempt 2 · starts 23 Nov" and "Attempt 1 · ended before it began" (plan "Spec discrepancies" 5); the Settings card with the row "From Thu 12 Nov" and the later target text, and its start row "Started" or "Starts" with the latest attempt's date, absent for an ended goal that never began; the ended state with the Notice "Ended Mon 16 Nov · Abandoned" and the action "Start new attempt" under the head, and no "Record", "Edit settings", "Pause" or "End goal", the History card keeping its record and note actions; the "Earlier attempts" card after Recent notes ("Attempt 1 · 2 Nov – 13 Nov" as a link, the attempt's name when it differs, the outcome Tag, the consistency, the note in `ht-muted`; "Attempt n · ended before it began" for an attempt with no date), absent with none
- [X] T004 [P] Create `design/pages/PageEditGoalSettings.html` and `.md` from `PageNewGoal` (HP-002): `NewGoalForm`'s fields for Build or Reduce, measurement, target, schedule and name, filled with the latest settings; no habit chooser, no start date; the schedule choice limited to the goal's kind (show a fixed-days goal with only fixed days); the summary card; above the actions "Changes apply from Thu 12 Nov. Past results keep their rules."; the primary "Save changes" and the secondary "Cancel"; one refusal at a time under its field with the result's message as it is (show "A Reduce goal with an amount has 1 time per period." under the schedule)
- [X] T005 [P] Create `design/pages/PageNewAttempt.html` and `.md` from `PageNewGoal` (HP-004). The spec allows a new page or a `PageNewGoal` state; take a new page, because its route, line and missing fields differ, and record the choice in `design/pages.md` (T011): every schedule kind and the start date; no habit chooser, no name; filled with the starting values; the line "Attempt 2 starts with its own history and a streak from zero."; the primary "Start attempt" and the secondary "Cancel"; one refusal at a time under its field (show "A new attempt starts after the day the goal ended." under the start date)
- [X] T006 [P] Create `design/pages/PageAttemptDetail.html` and `.md` from `PageGoalDetail` (HP-014): "← Meditation, 5 min daily" back to the goal page; the heading "Attempt 1 · 2 Nov – 15 Nov", and a second state "Attempt 1 · ended before it began" with an empty History; the attempt's name, the outcome `Tag` and the note in `ht-muted`; `GoalStats` with the eyebrow "Attempt 1", its figures and strip; the History card with the record and note actions of the goal page; no "Record", "Edit settings", "Pause", "End goal", trend, Settings, Pauses or Recent notes card
- [X] T007 [P] Change `design/components/EndGoalDialog.html` and `.md` (HP-003, HP-009): the title "End goal · {goal name}" and the sub line with the attempt line; the effect first, "Ending stops new commitments. Your history stays and nothing is counted as failed. To try again later you start a new attempt."; "How did it end?" as ChoiceCards Achieved, Abandoned and No longer relevant in one column, **none chosen** when it opens (replace the chosen "Achieved"); "Note (optional)" as a `TextArea`; the primary "End goal" and "Cancel"; "Choose how the goal ended." under "How did it end?" and "The note can have at most 500 characters." under the note; the focus return to the Notice's "Start new attempt" after an ending
- [X] T008 [P] Add the slice-8 state to `design/pages/PageGoals.html` / `.md` and `design/components/GoalListItem.html` / `.md` (HP-005): the Tabs "Active", "Paused" and "Ended" (`?tab=ended`); the ended item with the outcome `Tag`, the meta "Ended Mon 16 Nov", its figures and strip (a Pending result shown as Pending), the secondary "Start new attempt" and no "Record"; the empty Ended tab "No ended goals."
- [X] T009 [P] Add the slice-8 state to `design/pages/PageHomeSignedIn.html` / `.md` and `design/components/CommitmentRow.html` / `.md` (HP-007): a Pending row of an ended goal with the `Tag` "Ended" after the goal name, and a Pending row of a restarted goal's earlier attempt with the `Tag` "Attempt 1" after that attempt's name; the "Active goals" tile and card and the recent notes without ended goals; nothing else changes
- [X] T010 [P] Add the slice-8 state to `design/pages/PageProfile.html` and `.md` (HP-006): the tab "Goal history" after "Account" (`?tab=history`), no Community tab; a `DataTable` with the columns Goal (link), Attempt, Ended ("Mon 16 Nov"), Outcome (`Tag`) and Note (`ht-muted`, or "—"); the empty text "No ended goals yet."; an administrator's Profile without Tabs
- [X] T011 Update `design/pages.md` and `design/README.md` with the slice-8 structure of the goal page, the three new pages, the Goals page, the signed-in Home and the Profile, and the choice of T005 (after T003 to T010). Copy every changed file of `design/css/` byte for byte to `src/HT.Ui/wwwroot/css/`. Run `DesignSyncTests` (`tests/HT.UiTests/DesignSyncTests.cs`)
- [X] T012 [P] Write `docs/adr/0027-attempts-and-per-attempt-settings-versions.md` (plan, Constitution Check "Decision Discipline"): R-01's decision (a goal holds its attempts, each with its own versions; the kept constructor; `StartDate` stays attempt 1's) and its three rejected alternatives; R-05's key (`GoalId`, `AttemptNumber`, `AppliesFrom`), the no-action foreign key and why a cascade is refused (two cascade paths); the one-way migration (`Down` valid only while every version is attempt 1's). Link it from `docs/adr/` as the other ADRs are, and from plan.md's Constitution Check if the link is missing

### Domain: attempts and endings (`src/HT.Domain/Goals/`; research R-01, R-04)

- [X] T013 [P] Create `src/HT.Domain/Goals/Outcome.cs` (`Achieved`, `Abandoned`, `NoLongerRelevant`), `EndingNote.cs`, `GoalEnding.cs` and `GoalAttemptVersions.cs` as data-model.md gives them: `EndingNote.TryCreate(string? text, out EndingNote? note, out bool tooLong)` with `GoalName.TryCreate`'s rules and the limit `RecordLimits.MaxNoteLength` (500; read `src/HT.Domain/Records/RecordLimits.cs` and use its constant, adding it there only if it is missing), `ToString` a fixed placeholder; `GoalEnding(DateOnly Date, Outcome Outcome, EndingNote? Note)` rejecting an undefined `Outcome` with `ArgumentOutOfRangeException`; add the init-only `GoalEnding? Ending` to `src/HT.Domain/Goals/GoalAttempt.cs`, keeping its constructor and value equality (FR-007, FR-026, HP-010)
- [X] T014 [P] Add `HasSameScheduleKind(GoalTarget other)` to `src/HT.Domain/Goals/GoalTarget.cs` (both fixed days, or both times with the same `SchedulePeriod`; D-1); add `StartDateNotAfterEnding` after `StartDateInPast` to `src/HT.Domain/Goals/GoalProblem.cs`; add `CheckTarget(GoalTargetFields fields, out GoalTarget? target)` to `src/HT.Domain/Goals/GoalDraft.cs`, returning the first problem of `TryCreate`'s target steps in their order, then `ReduceAmountNeedsOneTime`, or `None` (FR-003 step 5; refactor `TryCreate` to call it so the order exists once, and keep `TryCreate`'s results unchanged); add `StartDateNotAfterEnding → GoalField.StartDate` to `FieldOf`. Check that `GoalCreateOutcome` in `src/HT.Application/Goals/GoalResults.cs` still parses every problem creation can produce (plan "Risks")
- [X] T015 Change `src/HT.Domain/Goals/Goal.cs` (after T013, T014; data-model.md "Goal", R-01):
  - add the constructor `Goal(GoalId id, AccountId owner, GoalHabit habit, GoalName? ownName, IReadOnlyList<GoalAttemptVersions> attempts)` with every invariant of data-model.md, each an `ArgumentException` whose message names no value. The rule "an ended attempt has no version after its last date except its first" needs the last date: compute it with the internal helper that T017's `GoalTimeline.LastDate` also uses, so the rule exists once;
  - the kept constructor builds the one attempt `attempt` with `versions` through the same checks; replace its rule "only the time zone differs between versions" with "the same schedule kind" (`HasSameScheduleKind`). Keep every other check and its message;
  - add `Attempts`, `IsEnded` and `VersionsOf(int number)`; keep `StartDate` (attempt 1's first version's date), `Attempt` (latest) and `Versions` (latest attempt's);
  - remove the internal `Target`; replace each caller in `src/HT.Domain` with the version it means (`grep -rn "\.Target\b" src/HT.Domain --include=*.cs`, then check each hit is `Goal.Target`); `WithVersions` replaces the latest attempt's versions; add the internal `WithName`, `WithEnding` and `WithAttempt`, each building a new `Goal` through the new constructor

### Domain: the attempt-aware timeline (research R-02 to R-04)

- [X] T016 Make the existing members of `src/HT.Domain/Goals/GoalTimeline.cs` attempt-aware, keeping every signature (after T015; data-model.md "GoalTimeline" table, R-02):
  - time each attempt from its own timing date (attempt 1: the goal's start date; a later attempt: its own start date) with its own versions, and cut it at its last date. Do it with the internal technique of R-02 (a one-attempt view per attempt inside `GoalTimeline`), never in the Application;
  - `VersionOf`, `CurrentDate` (the extended rule, "the latest attempt that has begun" by T017's rule), `TryCalendar`, `TryEntries`, `EntryOf` (only entries inside an attempt; a range across attempts of both kinds fills both lists, `Kind` by R-02's rule, `TryEntries` merges in date order), `HasBegun` (the date's own attempt and zone), `EntriesThrough(goal, last)` (the latest attempt; its first window from the Monday on or before that attempt's timing date), `NextCommitment` and `FirstWeekStart` (the latest attempt; none for an ended goal);
  - replace every use of `goal.StartDate` as the goal's first date with the attempt's timing date (R-01; `grep -n "StartDate" src/HT.Domain/Goals/GoalTimeline.cs`);
  - change the doc comments of `src/HT.Domain/Goals/EvaluationCalendar.cs` and `src/HT.Application/Goals/GoalCalendarView.cs` ("empty unless the range spans attempts of both kinds")

  For a goal with one attempt, no ending and no change, every member gives exactly what it gives today. Every existing test in `tests/HT.UnitTests/Domain/Goals/` passes unchanged
- [X] T017 Add to `src/HT.Domain/Goals/GoalTimeline.cs` (after T016; data-model.md "New members"): `LastDate(goal, number)` (E − 1, or the Sunday before E's week for per week; null while not ended), `AttemptOf(goal, date)`, `EntriesThrough(goal, number, last)`, `VersionInEffect(goal, number, instant)` and `VersionInEffect(goal, instant)` (ended: the last version, the version of its last date or the first when it has no date; not begun: the first; otherwise the version at the goal's current date), and `LatestAttemptHasBegun(goal, instant)` (not ended: the timing date's day has begun in the first version's zone; ended: the attempt has a date, i.e. its last date is on or after its first entry's first date). "No date" is exactly "the last date is before the attempt's first entry" (plan "Spec discrepancies" 3), in one internal helper that the constructor rule of T015, `VersionInEffect`, `LatestAttemptHasBegun` and `AttemptView.HasNoDate` (T082) use
- [X] T018 Change `ChangeOf` and `src/HT.Domain/Goals/GoalVersionChange.cs` to work on the latest attempt (`None` for an ended goal; `ReplaceFirst` while the latest attempt has not begun, also when an earlier attempt gives the goal a current date; FR-021), and add `SettingsChangeOf(goal, instant, target, zone)` returning the new `GoalSettingsChange(IReadOnlyList<GoalSettingsVersion> Versions, DateOnly AppliesFrom)` (after T017; R-03): not begun → the first version replaced, keeping its date; begun at D → the next boundary B (D + 1, or the Monday after D's week for per week), replacing the one waiting version or adding one from B; throws `InvalidOperationException` for an ended goal and `ArgumentException` for another schedule kind (the use case checks both first)
- [X] T019 Add `EndingOf(goal, endingDate)` returning the new `GoalEndingEffect(DateOnly LastDate, IReadOnlyList<GoalSettingsVersion> KeptVersions)` (the versions on or before L, and always the first; throws for an ended goal) and `NewAttemptEarliestStart(goal, accountToday)` (the later of `accountToday` and E + 1; throws for a goal that is not ended) to `src/HT.Domain/Goals/GoalTimeline.cs` (after T017; R-04, FR-008, FR-009, FR-011)

### Domain: names and the trend (research R-07, R-08)

- [X] T020 [P] Add `Name(Goal goal, string habitName, DateTimeOffset instant)` (own name, or the habit name, ", " and the target text of `VersionInEffect(goal, instant)`) and `AttemptName(Goal goal, int number, string habitName)` (own name, or the habit name and the target text of that attempt's last version) to `src/HT.Domain/Goals/GoalText.cs`; the kept `Name(goal, habitName)` uses the latest attempt's first version (after T017; FR-004, R-08)
- [X] T021 [P] Change `src/HT.Domain/Reports/TrendBuilder.cs` (after T015; R-07 "The trend"): every bar and week is drawn in the trend's kind; an entry whose own measurement differs from the trend's contributes only its result: in commitment weeks one "Yes" when Met and none otherwise; in a value trend no value, exactly like an entry without a record, keeping its exemption mark; the trend line takes only bars of the trend's measurement. Read how each entry's target reaches the builder first; if it does not, pass it with the begun items through the existing exemption overload's item type, without changing a public signature, and record what you chose in data-model.md. Every bar of a goal without such a change stays as in slice 7

### Domain tests (constitution II, SC-002, SC-003, SC-005; research R-12)

- [X] T022 [P] Write `tests/HT.UnitTests/Domain/Goals/EndingNoteTests.cs` and `GoalEndingTests.cs` (after T013): null, empty and blank give no note; "  Too busy.  " gives "Too busy."; 500 characters accepted, 501 refused with `tooLong`, 501 with surrounding spaces that trim to 500 accepted; a surrogate pair counts as two units; `ToString` never returns the text; an undefined `Outcome` throws; `GoalAttempt` equality includes `Ending`
- [X] T023 [P] Add to `tests/HT.UnitTests/Domain/Goals/GoalDraftTests.cs` (after T014): `CheckTarget` gives each target problem in `TryCreate`'s order, one case per message, and `None` for a valid target; values outside the chosen measurement or schedule are ignored (AC-021); `HasSameScheduleKind` for fixed days / per day / per week pairs, and times per day with different N; the `Fields` data of `Every_refusal_has_a_field_and_None_has_none` gains `StartDateNotAfterEnding → StartDate` (a change on purpose, R-12); every problem `TryCreate` can produce parses as a `GoalCreateOutcome` (a test in `tests/HT.UnitTests/Application/Goals/GoalCreateTests.cs`)
- [X] T024 [P] Add to `tests/HT.UnitTests/Domain/Goals/GoalTests.cs` (after T015): every invariant of the new constructor, each with its `ArgumentException` (no attempt; numbers 1, 3; an attempt before the latest without an ending; no version; dates not increasing; attempt 1's first version not on the goal's start date, refused as slice 4 refuses it; a later attempt's first version not on its start date; two schedule kinds in one attempt; a per-week later version not on a Monday; attempt n + 1 on attempt n's ending date; an ended attempt with a version after its last date that is not its first; an ended attempt with no date keeping its first version is accepted); `Attempts`, `IsEnded`, `VersionsOf` (unknown number throws); the kept constructor's results equal the new constructor's for one attempt. Change `Only_the_time_zone_differs_between_versions` on purpose (R-12): it becomes "versions of one attempt keep the schedule kind": fixed days after times per day throws, per week after per day throws, 2 times per day after 1 time per day is accepted
- [X] T025 [P] Write `tests/HT.UnitTests/Domain/Goals/GoalTimelineAttemptTests.cs` (after T017): the attempt cuts (US2's goal ended 16 Nov has no entry from 16 Nov; US3's attempt 2 from 23 Nov leaves 16 to 22 Nov in no attempt); `AttemptOf` on both sides of each cut; the extended `CurrentDate` (running, ended, waiting attempt after a begun attempt 1, never begun → null); `HasBegun` for a waiting attempt in New York after attempt 1 in Kyiv (refused until the day begins in New York, at the instant before and at it); a waiting attempt in New York ended on its start date, taken from attempt 1's Kyiv current date: not begun, and `CurrentDate` keeps Kyiv's zone (R-12); `VersionInEffect` (both overloads) and `LatestAttemptHasBegun` on running, waiting and ended attempts, for AC-035 (attempt 1 from 23 Nov ended 17 Nov, attempt 2 from 18 Nov) and for an attempt ended with no date, which never begins; a one-attempt goal whose attempt 1 starts after the goal's start date is timed from the goal's start date (R-01); `NextCommitment` and `FirstWeekStart` none for an ended goal
- [X] T026 [P] Write `tests/HT.UnitTests/Domain/Goals/GoalTimelineEndingTests.cs` (after T019; FR-008, FR-009, SC-003, SC-005): `LastDate` for commitment-based, per-day and per-week attempts ended on a Monday, a Wednesday and a Sunday (per week: ended Sunday 22 Nov gives Sunday 15 Nov; ended Monday 23 Nov gives Sunday 22 Nov); an ending on, before and after the start date; a per-week attempt from Wed 18 Nov ended Tue 24 Nov has no date (plan "Spec discrepancies" 3); `EndingOf` with a waiting later version (removed), a version that took effect on E (removed), one on the Monday of the week in progress for per week (removed), and the first version always kept; `NewAttemptEarliestStart` on the ending date, the day after, and with the account's today a day behind the ending date (US3-AS13); both throws
- [X] T027 [P] Write `tests/HT.UnitTests/Domain/Goals/GoalTimelineSettingsChangeTests.cs` and add to `GoalTimelineChangeTests.cs` (after T018; FR-002, SC-002, SC-005): `SettingsChangeOf` at `2026-11-11T23:59:59Z` and `2026-11-12T00:00:00Z` local in UTC and in Kyiv; per week on Sunday and Monday; with a waiting version (replaced, one later version); on a waiting attempt (first version replaced, date kept; US1-AS12); the zone of the written version is the given one; no begun date changes its version (check `VersionOf` of every date up to D before and after); both throws. `ChangeOf` on an ended goal (`None`) and on a waiting attempt 2 after a begun attempt 1 (`ReplaceFirst`)
- [X] T028 [P] Write `tests/HT.UnitTests/Domain/Goals/GoalTimelineMixedCalendarTests.cs` (after T016; US3-AS5, R-02): a daily attempt 1 ended 16 Nov and a 3-times-a-week attempt 2 from 18 Nov: a range from 2 to 29 Nov fills `Commitments` (2 to 15 Nov) and `Periods` (the weeks from 23 Nov), `Kind` is per week, `TryEntries` merges in date order; a range inside attempt 1 only gives commitment-based with no periods; a range between the attempts gives no entry and the kind of R-02's fallback
- [X] T029 Write `tests/HT.UnitTests/Domain/Goals/GoalTimelineEquivalenceTests.cs` (after T016, T017, and after T027, which edits `GoalTimelineChangeTests.cs` that this task reuses; FR-024, R-12): run the timeline cases of `GoalTimelineCalendarTests.cs`, `GoalTimelineEntryTests.cs`, `GoalTimelineEntriesThroughTests.cs` and `GoalTimelineChangeTests.cs` on a one-attempt goal built through both constructors and assert identical results for every member, also `VersionInEffect(goal, instant)` against `VersionOf(goal, CurrentDate(goal, instant) ?? goal.StartDate)`. Reuse the existing cases' data (a shared `TheoryData` or a sample builder in `GoalSamples.cs`); do not copy them
- [X] T030 [P] Add to `tests/HT.UnitTests/Domain/Goals/GoalTextTests.cs` (after T020; FR-004, AC-001, AC-002, AC-103): `Name` at `2026-11-11T23:59:59Z` and `2026-11-12T00:00:00Z` around a change; for an ended goal (its last version); for a waiting attempt (its first version, US3-AS4 "Meditation, 5 min daily"); the own name wins; `AttemptName` by that attempt's last version, with a habit renamed after the ending; the two-argument `Name` gives today's result for a one-attempt goal
- [X] T031 [P] Write `tests/HT.UnitTests/Domain/Reports/TrendBuilderMeasurementChangeTests.cs` (after T021; R-07): a duration-to-yes/no change and a yes/no-to-duration change in a commitment-based attempt (weekly counts take one "Yes" per Met entry of the other measurement, never its minutes); a count-to-duration change per day (the earlier bars have no value, the trend line only the trend's measurement); an exemption mark kept on a bar of the other measurement; every case of `TrendBuilderTests.cs` and `TrendBuilderExemptionTests.cs` unchanged
- [X] T032 Add the Domain rows to `tests/HT.ArchitectureTests/PublicSurfaceTests.cs` (after T013 to T021; R-12): `Outcome`, `EndingNote`, `GoalEnding`, `GoalAttemptVersions`, `GoalSettingsChange`, `GoalEndingEffect` join the rows that check for no NodaTime type in their signatures. `DomainDependencyTests` and `ClockReadTests` need no change; run them

**Checkpoint (Domain)**: the full suite passes, every existing test unchanged apart from T023's `Fields` row and
T024's renamed schedule-kind test. Commit.

### Storage (`HT.Application.Goals` and `HT.Application.Records` ports, `HT.Persistence`; research R-05)

- [X] T033 Create `src/HT.Application/Goals/OwnerGoal.cs`, `GoalSettingsWrite.cs`, `GoalEndingWrite.cs`, `GoalAttemptWrite.cs` and `GoalChangeWriteOutcome.cs` (`Written`, `Stale`, `GoalNotFound`) as data-model.md gives them, and add the four members of contracts/application-api.md "IGoalStore: new and changed members" to `src/HT.Application/Goals/IGoalStore.cs`, each with the owner first and a doc comment stating its re-checks (after T015; FR-022, FR-026). In the same commit, add the four members to `RacingGoalStore` in `tests/HT.IntegrationTests/Goals/TimeZoneChangeTests.cs` and `BeforeFirstCreate` in `tests/HT.UnitTests/Application/Goals/GoalWorld.cs`, each forwarding or throwing as it does for its other members. Lands in one commit with T035, T037 and T038
- [X] T034 Add the overload `ReadRecentNoteKeysAsync(AccountId owner, GoalId goal, DateOnly from, DateOnly? through, int count, CancellationToken)` to `src/HT.Application/Records/IRecordStore.cs` and `GoalEnded` to `ExemptionStoreWriteOutcome` in `src/HT.Application/Records/ExemptionStoreWrite.cs` (contracts/application-api.md "IRecordStore"). In the same commit, add the overload to both `FaultingRecordStore`s (`tests/HT.IntegrationTests/Records/RecordLogTests.cs`, `ExemptionLogTests.cs`), delegating behind the same fault switch as their other members. Lands in one commit with T036 and T039
- [X] T035 Implement the four members in `tests/HT.UnitTests/Fakes/FakeGoalStore.cs` (after T033), with the contract's re-checks and outcomes (zone and revision as read, the latest attempt's number and ending; `Stale` with nothing written; another owner's goal behaves like none), the pause and excusal cuts of `TryEndAsync` applied to the `FakeRecordStore` it shares when one is attached (read how the two fakes share state first), `ListAsync`/`FindAsync`/`ReadOwnerGoalsAsync` with every attempt, the catalog counts and the catalog goal list leaving out ended goals, and `TryChangeTimeZoneAsync` on the latest attempt. Make the existing `Add(AccountId owner, Goal goal, GoalHabitView habit)` helper accept a goal built through the new constructor with any attempts and endings (no second seeding helper), so tests can build past attempts the use cases would refuse, and add a hook that runs once inside the next conditional write, before its re-check (for example `Action? BeforeNextWrite`), so a unit test can simulate a concurrent write or a stale revision. `FakeGoalStore` has no `Calls` list: it counts calls per member (`CreateCount`, `TimeZoneWriteCount`, `TimeZoneChangeCount`, `Lookups`). Follow that: add `OwnerGoalReads` (calls of `ReadOwnerGoalAsync`) and `ChangeWriteCount` (calls of the three conditional writes, refused ones included), and count `ReadOwnerGoalAsync` in `Lookups` too, so the access tests can prove a refusal looked up no goal. The existing members keep their results for one-attempt goals
- [X] T036 Implement the overload in `tests/HT.UnitTests/Fakes/FakeRecordStore.cs` (newest `count` keys from `from` through `through`, owner-scoped) and the `GoalEnded` refusal of `AddPauseAsync` when the goal's latest attempt is ended (read from the `FakeGoalStore` it shares) (after T034). Appends to `Calls` as the other members do
- [X] T037 [P] Change `src/HT.Persistence/Goals/GoalRows.cs`, `GoalMapping.cs` and `src/HT.Persistence/HtDbContext.cs` exactly as data-model.md "Tables" gives them (the three nullable attempt columns, `AttemptNumber` with the default 1, the key (`GoalId`, `AttemptNumber`, `AppliesFrom`), the foreign key (`GoalId`, `AttemptNumber`) → `GoalAttempts` with `DeleteBehavior.NoAction`, the existing cascades kept) and the mapping's checks (both ending columns or neither, no note without them, numbers 1, 2, … , every attempt but the latest ended, every later attempt's first version on that attempt's start date; attempt 1 keeps slice 4's rule only, so an attempt 1 that starts after the goal's start date stays valid, data-model.md "Goal"), each failure a fault naming no value (after T015). Add the migration with `dotnet ef migrations add GoalChanges --project src/HT.Persistence`. Read the generated `Up` and `Down`, check their order against data-model.md "The migration's order" (the column with its default before the key and foreign key) and edit the order by hand if EF generated another, and check that `HtDbContextModelSnapshot.cs` changed only by these columns and keys. Leave the `CA1861` hints in the migration code (`CLAUDE.md`)
- [X] T038 Implement on `src/HT.Persistence/Goals/GoalStore.cs` (after T033, T037; R-05): `ListAsync`, `FindAsync` and `ReadOwnerGoalsAsync` read every attempt with its versions and ending in one query, as before; `ReadOwnerGoalAsync` reads the zone and revision, then the goal, in that order, owner-scoped in the statement; `TryChangeSettingsAsync`, `TryEndAsync` and `TryStartAttemptAsync` in one transaction each, locking the account row (`UPDLOCK, HOLDLOCK`), checking the zone and `GoalsRevision`, locking the owner's goal row, re-checking their precondition, writing, and incrementing `GoalsRevision`; `TryEndAsync` deletes the goal's `GoalPauses` rows that begin after `LastDate`, sets the last day to `LastDate` where one covers it, and deletes `GoalExcusals` rows after it, in the same transaction; `TryChangeTimeZoneAsync` applies each write to the latest attempt's versions; `CountByCatalogHabitAsync` and `ListOnCatalogHabitAsync` leave out goals whose latest attempt has an ending date; `CountUsingCustomHabitAsync` and `CountUsingCatalogHabitAsync` unchanged. Faults name the SQL error number only, as now
- [X] T039 Implement on `src/HT.Persistence/Records/RecordStore.cs` (after T034, T037): the `ReadRecentNoteKeysAsync` overload (both bounds, owner scope in the statement; the old overload unchanged); `AddPauseAsync` re-checks under the goal lock that the goal's latest attempt has no ending date, else `GoalEnded` with nothing written
- [X] T040 [P] Write `tests/HT.IntegrationTests/Goals/GoalChangesMigrationTests.cs` on `MigrationDatabaseFixture` (after T037; R-05, R-12): migrate to the migration before `GoalChanges`, write a slice 7 goal with two versions (a time zone change), records and an excusal, then migrate: every version is attempt 1, the key and foreign key exist with no delete action, the three ending columns are null, and the goal reads back unchanged through `GoalStore`; follow `ExemptionsMigrationTests.cs` and `PreSliceAccountMigrationTests.cs`
- [X] T041 [P] Write `tests/HT.IntegrationTests/Goals/GoalChangeStoreTests.cs` on `HT-IT`, with the test's own provider and clock (after T038, T039; R-05, R-12): each write's happy path; each re-check (stale zone, stale revision, the latest attempt not `write.Attempt`, already ended, not ended for a start, wrong number for a start) gives `Stale` with nothing written; another owner's goal gives `GoalNotFound` and `ReadOwnerGoalAsync` gives a null `Goal`; `TryEndAsync`'s cuts in one transaction (a pause covering L cut, one after L deleted, an excusal after L deleted, records and notes after L kept); two attempts with versions from the same date; deleting a goal with two attempts (and deleting its account) through the existing cascades and the no-action key; `ReadRecentNoteKeysAsync` with both bounds leaving out a note between two attempts and one after a last date; `AddPauseAsync` after an ending gives `GoalEnded`; the catalog counts and goal list leave out an ended goal while the "used by goals" counts keep it

### Application: texts and per-entry settings (research R-07)

- [X] T042 [P] Create `src/HT.Application/Goals/GoalChangeTexts.cs` with every text of contracts/application-api.md "Texts", word for word, and add to `src/HT.Application/Reports/ReportText.cs` (HP-008, contracts/host.md "Shared texts"): the outcome words; "Ended {date} · {outcome}" and "Ended {date}"; "Attempt {n} · started {date}", "Attempt {n} · starts {date}", "Attempt {n} · {first} – {last}" and "Attempt {n} · ended before it began"; "From {date}"; "Changes apply from {date}. Past results keep their rules."; "Attempt {n} starts with its own history and a streak from zero."; "No ended goals." and "No ended goals yet.". Dates through `DateTexts` in the formats of contracts/host.md. Record each new member in data-model.md and contracts/host.md if missing. Add `GoalChangeTexts` to the NodaTime theory of `tests/HT.ArchitectureTests/PublicSurfaceTests.cs` (R-12)
- [X] T043 Create `src/HT.Application/Records/EntrySettings.cs` as data-model.md gives it; add the init-only `AttemptNumber`, `IsLatestAttempt` (default true), `AttemptName` and `Settings` (`EntrySettings?`, not `required`) to `src/HT.Application/Records/HistoryEntry.cs`; add `SettingsOf(HistoryEntry entry)` to `src/HT.Application/Records/GoalHistory.cs` (the entry's settings, or the history's goal-level settings as an `EntrySettings`) and drop "every version has the same target" from its doc comments, which also name the *Goal history* / *History* distinction (plan, Constitution Check I); change `src/HT.Application/Records/RecordPreview.cs` (`Progress(GoalHistory, …)` reads `history.SettingsOf(entry)`; add `Progress(EntrySettings, HistoryEntry, RecordId?, YesNoAnswer?, int?)`), add the `EntrySettings` overloads of `Recorded`, `TargetLine`, `Progress`, `FieldLabel`, `PartialNote`, `ReduceHelp` and `NoteHelp` to `src/HT.Application/Records/RecordingText.cs`, each `GoalHistory` member delegating to its overload, and the `EntrySettings` overload of `ProgressText` to `src/HT.Application/Reports/ReportText.cs` (after T015; R-07 table). Add `EntrySettings` to the NodaTime theory of `tests/HT.ArchitectureTests/PublicSurfaceTests.cs` (R-12)

### Application: the shared read path (research R-07, R-08)

- [X] T044 Change the internal `src/HT.Application/Records/EntryViews.cs` (after T017, T043): `Of` sets `AttemptNumber`, `IsLatestAttempt`, `AttemptName` (`GoalText.AttemptName` for an attempt that is not the latest) and `Settings` (the version of the entry's date, through `GoalTimeline.VersionOf`); `HistoryOf` takes the version whose goal-level fields it builds from as a parameter, which every caller passes from `VersionInEffect` of the attempt it shows (R-07 "The goal-level settings of an attempt's History")
- [X] T045 Change the internal `src/HT.Application/Reports/GoalEvaluation.cs` (after T017, T020, T021, T044; R-07):
  - the existing `OfAsync` evaluates the latest attempt; add `OfAsync(records, owner, stored, int attemptNumber, now, ct)`, evaluating that attempt through the earlier of its last date and the goal's current date;
  - the target it is built with (`LatestTarget`, and with it `Unit`, `BuildOrReduce` and the trend's kind and span) is `VersionInEffect(goal, number, now)` of the evaluated attempt; `Name` is `GoalText.Name(goal, habitName, now)` for the latest attempt and `GoalText.AttemptName` for an earlier one;
  - `EarlierPending`: the Pending entries of ended attempts other than the latest whose grace period is open at `now`, evaluating only attempts whose last entry ended within the last 48 hours (`EntriesThrough(goal, number, last)`);
  - `Recordable`: its own Due and Pending entries plus `EarlierPending`, in slice 6's order;
  - the records of all of them from one `ReadAsync` over the range they span

  Where `GoalEvaluation` reads `goal.StartDate` as the first date (`grep -n "StartDate" src/HT.Application/Reports/GoalEvaluation.cs`), it uses the evaluated attempt's timing date (R-01)
- [X] T046 Change `src/HT.Application/Records/RecordingService.cs` (after T039, T044, T045; FR-010, FR-014, FR-016, R-07). The constructor and every public signature stay as they are:
  - `GetHistoryAsync` keeps its range check and gives only the latest attempt's entries within the range, built with `HistoryOf` and the latest attempt's `VersionInEffect`;
  - `GetRecordableAsync` gives the latest attempt's Due and Pending entries plus `EarlierPending`, each with its attempt fields and `Settings`;
  - `GetRecentNotesAsync` reads through the bounded overload (`from` the latest attempt's first entry's first date, `through` its last entry's last date once ended), gives an empty list without a store call for an attempt with no date, and skips a key with no entry;
  - `RecordAsync` and `SaveNoteAsync`: a date outside every attempt gives `RecordTexts`' "Nothing is scheduled on this date." through the unchanged `EntryOf` step; a date of a waiting attempt gives "You cannot record a day that has not begun." until its day begins in that attempt's zone; the value is checked against the measurement of the version of its date;
  - `ChangeRecordAsync` and `DeleteRecordAsync`: a record whose date is in no attempt gives `NothingScheduled`, "Nothing is scheduled on this date.", and changes nothing; remove `StoredEntryOf` and `NoteEntryOf` with their exceptions (add `NothingScheduled` to `RecordWriteOutcome` only if it is not already there).

  Change `tests/HT.IntegrationTests/Records/RecordLogTests.cs` near lines 421-429 on purpose (R-12): change and delete of the stray record give `NothingScheduled` and change nothing, recent notes leave the stray note out, and no log line names a value. Any exact `FakeRecordStore.Calls` list that changes because recent notes now call the overload changes to name it, and nothing else; list them first with `grep -rn "ReadRecentNoteKeysAsync" tests/HT.UnitTests tests/HT.IntegrationTests --include=*.cs --exclude-dir=HT.AcceptanceTests`
- [X] T047 Change `src/HT.Application/Reports/ReportService.cs` and its views (after T042, T045; FR-013, FR-018, FR-019, R-07). The constructor and every public signature stay as they are:
  - the goal report is the latest attempt (the existing `OfAsync`);
  - the overview: each item's ending fields come from `GoalListItems` (T048); an ended goal has `PausedUntil` null and no Due entry; `OverviewGoal.Recordable`, `HasPending`, `CanRecord` and the Pending group of `GoalOrder` (`src/HT.Application/Reports/GoalOrder.cs`) read the evaluation's `Recordable`, so the overview's `Recordable` equals `GetRecordableAsync` at the same instant;
  - the dashboard: the Today list and `Pending` count include `EarlierPending` and an ended goal's Pending entries; `MissedThisWeek` leaves out ended attempts' Missed entries; an ended goal has no Due or Upcoming entry and no summary; `Goals` counts goals neither paused nor ended; `Dashboard.Goals` and `Dashboard.Notes` leave out ended goals;
  - add the init-only `GoalEnded` to `TodayEntry` (`src/HT.Application/Reports/Dashboard.cs`); for an entry of an attempt that is not the latest, `GoalName` is that attempt's name and `BuildOrReduce` and `ScheduleText` come from `Goal.SettingsOf(Entry)`.

  Any exact store-call list of `tests/HT.UnitTests/Application/Reports/` or `tests/HT.IntegrationTests/Reports/GoalReportReadTests.cs` that changes stays at one record read per goal for a goal with no recent ending (plan "Performance Goals"); a change beyond that is a regression
- [X] T048 Change `src/HT.Application/Goals/GoalService.cs`, `GoalDetails.cs`, `GoalListItem.cs` and the internal `GoalListItems.cs` (after T017, T018, T020, T042; FR-004, FR-005, FR-017, FR-018, R-07, R-08): create `src/HT.Application/Goals/GoalSettingsView.cs`; `DetailsOf` takes the settings from `VersionInEffect(goal, now)` (not `goal.Versions[0]`, today near `GoalService.cs:272`), the name from `GoalText.Name(goal, habitName, now)`, and sets `Later` (all fields of the waiting version with its date; `LaterTimeZone` and `LaterTimeZoneAppliesFrom` from the same version), `IsEnded`, `EndingDate`, `LastDate`, `Outcome`, `EndingNote` (its value), `ChangesApplyFrom` (the date `SettingsChangeOf` would give at `now`; null for an ended goal; nothing written) and `AttemptHasBegun` (`LatestAttemptHasBegun`); `AttemptNumber` and `AttemptStartDate` are the latest attempt's; `StartDate` stays the goal's. `GoalListItems.Of` sets `IsEnded`, `EndingDate` and `Outcome`, names and sorts at `now`, sets `StartDate` to the latest attempt's start date while it has not begun and the goal is not ended (else null; AC-035), and gives an ended goal no next commitment and no first-week date. `GetCalendarAsync` needs no code change beyond T016; check it. Add `GoalSettingsView` to the NodaTime theory of `tests/HT.ArchitectureTests/PublicSurfaceTests.cs` (R-12)
- [X] T049 Add the constructor `CatalogService(ICatalogHabitStore, IGoalStore, TimeProvider)` to `src/HT.Application/Habits/CatalogService.cs` (after T020; FR-020, R-08): `GetDetailsAsync` reads the time once and names each goal with `GoalText.Name(…, now)`; the existing two-argument constructor (the only one today) keeps its behaviour. The registration `AddScoped<CatalogService>()` in `src/HT.Application/ApplicationServiceCollectionExtensions.cs` resolves the longest constructor whose parameters are registered; make the choice explicit with a factory that passes the `TimeProvider`, so the host never falls back to the two-argument one. Leave `PageServices.cs`' registration as it is unless a page test fails
- [X] T050 Change `src/HT.Application/Exemptions/PauseService.cs` and `PauseResults.cs` (`PauseOutcome.GoalEnded`, "This goal has ended." from `GoalChangeTexts.GoalEnded`, checked after step 3; a store `GoalEnded` gives the same result) and `src/HT.Application/Exemptions/ExcusalService.cs` (step 4 judges "the goal is period-based" by the schedule kind of the attempt that contains the date; a date in no attempt keeps step 5's "Nothing is scheduled on this date.") (after T017, T036, T042; FR-010, plan "Spec discrepancies" 2). `ProfileService` needs no code change (its `ChangeOf` is now attempt-aware, FR-021); check it

### Application tests (shared)

- [X] T051 [P] Create `tests/HT.UnitTests/Application/Goals/AttemptWorld.cs`, building on `GoalWorld.cs`, `Records/RecordWorld.cs`, `Reports/ReportWorld.cs` and `Exemptions/ExemptionWorld.cs`: goals seeded through the new `Goal` constructor on `FakeGoalStore` (T035) with any attempts, versions and endings, or created at an earlier supplied time through `GoalService`; records through `RecordingService`; the clock moved forward; reads through `GoalService`, `RecordingService`, `ReportService`, `CatalogService` and `ProfileService`. Once US1 to US3 exist, their tests make changes, endings and attempts through `GoalChangeService` instead of seeding (after T035, T036)
- [X] T052 [P] Add to `tests/HT.UnitTests/Application/Records/RecordingTextTests.cs`, `RecordPreviewTests.cs` and `tests/HT.UnitTests/Application/Reports/ReportTextTests.cs` (after T042, T043; R-07, HP-008): each `EntrySettings` overload gives what its `GoalHistory` member gives for the same settings; a duration entry under a count history gives "12 min", a count entry "12 pages" (AC-020); the Build help on a Build entry under a Reduce history; `SettingsOf` falls back to the goal-level settings for an entry without `Settings`; every new `ReportText` and `GoalChangeTexts` text word for word, and with another culture's separator unchanged (`CultureScope`)
- [X] T053 [P] Write `tests/HT.UnitTests/Application/Records/AttemptRecordingTests.cs` with `AttemptWorld` (after T046, T051; FR-009, FR-010, FR-014, FR-016, R-07): the History of a restarted goal covers only the latest attempt; what can be recorded now gives the latest attempt's entries plus an earlier attempt's open Pending entry with its attempt fields, name and Build or Reduce, and drops it at its grace end; a record and a note on the ending date of a commitment-based goal, and in the week in progress of a per-week goal, are kept, never mapped to an entry, and left out of recent notes; changing and deleting such a record gives "Nothing is scheduled on this date." and changes nothing; adding a record between two attempts gives that text; a date of a waiting attempt in a west zone gives "You cannot record a day that has not begun." until its day begins; a record of an earlier attempt's date is accepted and checked against its own measurement; recent notes of an attempt with no date read nothing (`Calls` empty after the access and goal reads); an ending note never appears in recent notes or the History (FR-027)
- [X] T054 [P] Write `tests/HT.UnitTests/Application/Reports/AttemptReportTests.cs` with `AttemptWorld` (after T047, T051; FR-013, FR-018, FR-019, R-07): a report after a restart counts only the latest attempt; an earlier attempt's `EarlierPending` appears in the overview's `Recordable`, `HasPending`, `CanRecord` and Pending group and equals `GetRecordableAsync` at the same instant; an attempt whose last entry ended more than 48 hours ago is not evaluated for it (one record read per goal); the dashboard's Today list and Pending count keep an ended goal's Pending entries marked `GoalEnded`, and an earlier attempt's marked with its number, name and Build or Reduce; Missed entries of ended attempts are neither listed nor counted; an ended goal has no Due, Upcoming or summary; the goal count leaves out paused and ended goals; recent notes leave out ended goals; one-attempt goals give slice 7's results
- [X] T055 [P] Write `tests/HT.UnitTests/Application/Goals/GoalAttemptReadTests.cs` with `AttemptWorld` (after T048, T051; FR-004, FR-005, FR-017, FR-018): `GetAsync` gives the settings in effect and the full `Later` settings (US1-AS1, AS2), the name at the instant on both sides of a boundary, the latest attempt's number and start date, `AttemptHasBegun` for running, waiting and ended-before-it-began attempts, `ChangesApplyFrom` (next boundary, a waiting attempt's start date, null when ended), the ending fields of an ended goal, its last version with no `Later`, and a waiting attempt's first version; `ListAsync` gives `IsEnded`, `EndingDate`, `Outcome`, `StartDate` by AC-035's rule, names and order at the instant, and no next commitment for an ended goal; `GetCalendarAsync` over two attempts of both kinds (AC-096 to AC-098)
- [X] T056 [P] Add to `tests/HT.UnitTests/Application/Habits/` (the catalog details tests; find them with `grep -rln "CatalogService" tests/HT.UnitTests --include=*.cs`), `tests/HT.UnitTests/Application/Exemptions/PauseAsyncTests.cs`, `ExcuseTests.cs` and the profile time zone tests (find them with `grep -rln "ChangeTimeZoneAsync" tests/HT.UnitTests --include=*.cs`) (after T049, T050, T051): the catalog counts, "Only habits I track" and the details' goal list leave out an ended goal, and the details name each goal at one time read through the three-argument constructor; pausing an ended goal gives "This goal has ended." after the access and goal checks, and a store `GoalEnded` gives the same; excusing a commitment of an earlier commitment-based attempt of a goal now per week is accepted, and a date between attempts gives "Nothing is scheduled on this date."; a time zone change adds no version to an ended goal and updates the first version of a waiting attempt (FR-021)
- [X] T057 [P] Add to `tests/HT.IntegrationTests/Habits/CatalogGoalInfoTests.cs` and `tests/HT.IntegrationTests/Records/RecordReadTests.cs` (after T038, T046, T049): the use cases that end and restart do not exist yet, so arrange the ended and restarted goals by calling `GoalStore.TryEndAsync` and `TryStartAttemptAsync` directly with the `GoalEndingWrite` and `GoalAttemptWrite` that `GoalTimeline.EndingOf` and a first version give, after `ReadOwnerGoalAsync` (the stored zone and revision); then, through the real stores, an ended goal leaves the catalog counts and goal list and stays "used by goals"; the History and recent notes of a restarted goal cover only the latest attempt

### Shared components on each row's settings (research R-07)

- [X] T058 Change `src/HT.Ui/Components/Records/HistoryCard.razor`, `RecordDialog.razor` (with `RecordFields.razor`), `NoteDialog.razor`, `src/HT.Ui/Components/Shared/CommitmentRow.razor` and `GoalItem.razor` (after T043, T046, T047; R-07 table): every state word, `StatusChip`, progress bar test, Recorded cell, sub line (`RecordingText.TargetLine`), field label, unit, progress line, partial note, Reduce help, note help, preview and commitment test reads the row's `History.SettingsOf(entry)` (or `Entry.Goal.SettingsOf(Entry)`) through the `EntrySettings` overloads; the RecordDialog's chooser switches them when another row is chosen. No component reads a goal-level target field for an entry (`grep -n "\.Measurement\|\.CountUnit\|\.GoalScheduleKind\|\.Target\b\|\.BuildOrReduce" <each file>` before and after)
- [X] T059 Add to `tests/HT.UiTests/Components/Records/HistoryCardTests.cs`, `RecordDialogTests.cs`, `NoteDialogTests.cs` and `tests/HT.UiTests/Components/Shared/CommitmentRowTests.cs` (after T058; AC-020, AC-080, R-07): AC-020's History shows 11 Nov "12 min" and 12 Nov "12 pages", and the Build help on 11 Nov after the change to Reduce; the RecordDialog for AC-080's attempt 1 row under an attempt 2 of Reduce per day shows the Build sub line, the duration field and the commitment-based note help, and switches when the chooser picks an attempt 2 row; the NoteDialog's help of an earlier row; a `CommitmentRow` of an earlier attempt shows its own state words and field. The existing tests that build `HistoryEntry` and `GoalHistory` without `Settings` (`CommitmentRowTests`, `HabitComponentTests`) stay unchanged as the proof of the fallback

**Checkpoint (shared)**: the full suite passes. The existing tests of slices 4 to 7 are unchanged apart from the
named edits: T023's `Fields` row, T024's schedule-kind test, T046's `RecordLogTests` block and any recent-notes call
list. Commit (T033 to T039 in this commit, Conventions).

---

## Phase 3: User Story 1 - Change a goal's settings (Priority: P1) 🎯 MVP

**Goal**: a registered user changes the settings of a goal that is not ended within its schedule kind, from the
next boundary at the goal's current date, and renames it at once; past results keep their rules.

**Independent Test**: create "Reading, 15 min daily" at an earlier supplied time, record 15 minutes from 2 to 15
November, raise the target to 20 minutes on Wednesday 11 November at 10:00, and read the history and report on
Monday 16 November at 12:00: 2 to 11 November Met, 12 and 13 Failed, 14 and 15 Pending, "83% (10 of 12)", current
streak 0 and longest 10 commitments.

### Implementation for User Story 1

- [X] T060 [US1] Create `src/HT.Application/Goals/GoalSettingsInput.cs` and the result `SettingsChangeResult` with its outcome enum `SettingsChangeOutcome` (in a new `src/HT.Application/Goals/GoalChangeResults.cs`, following `GoalResults.cs`; the target problems in `GoalProblem`'s order, `Field` from `GoalDraft.FieldOf`, `ScheduleKindChanged → GoalField.Schedule`, `NameTooLong → GoalField.Name`) as data-model.md gives them; record the enum's name in data-model.md (after T042)
- [X] T061 [US1] Create `src/HT.Application/Goals/GoalChangeService.cs` with the constructor of contracts/application-api.md and `ChangeSettingsAsync` (after T048, T060; FR-001 to FR-006, contracts/application-api.md "ChangeSettingsAsync"): access in FR-023's order, then `ReadOwnerGoalAsync`; steps 4 to 7 in order; "settings unchanged" against the latest version (the waiting one included, AC-100) and the trimmed own name; a name-only change with `Versions` null and `AppliesFrom` null; otherwise `SettingsChangeOf` with the account's current zone and `TryChangeSettingsAsync`. Put the write-and-retry loop in one private method that T071 and T080 reuse: on `Stale` read again and decide again, at most `GoalService.MaxCreateAttempts` times, then throw `InvalidOperationException` naming no value. In the same commit: register it in `src/HT.Application/ApplicationServiceCollectionExtensions.cs` and in `tests/HT.UiTests/Components/Pages/PageServices.cs`; add it and its results to `tests/HT.ArchitectureTests/PublicSurfaceTests.cs` and its member to `UseCaseParameterTests.cs` (the acting user first, the token last). The `IGoalStore` rule already admits `HT.Application.Goals`; run it

### Tests for User Story 1

- [X] T062 [P] [US1] Write `tests/HT.UnitTests/Application/Goals/ChangeSettingsTests.cs` with `AttemptWorld` (after T061; US1-AS1, AS2, AS6 to AS14, FR-003): every scenario of US1 at its instant; the order of FR-003 (an ended goal with an invalid target gives "This goal has ended."; an invalid target with another schedule kind gives the target's text; another kind with a long name gives the kind's text); each target refusal one case per message, nothing written (`ChangeWriteCount` unchanged); "settings unchanged" with exactly the latest settings and name, also against a waiting version (AC-100); a name-only change, then clearing the name; values outside the chosen measurement ignored and not stored (AC-021); a change on a waiting goal replaces its first version (AS12); a change after a time zone change merges into one later version with Kyiv (AS13); a pause does not block (AS14); a Build-to-Reduce change and a measurement change within the schedule kind accepted
- [X] T063 [P] [US1] Write `tests/HT.UnitTests/Application/Goals/ChangeSettingsEffectTests.cs` with `AttemptWorld` (after T061; US1-AS3 to AS5, AS7, FR-006, SC-002, SC-005): the history and report after the change (AS3, AS4), a correction of 9 November judged by 15 minutes (AS5), the per-week change from Monday 23 November keeping the week 16 to 22 November at 3 (AS7), a streak running across two versions in one unit; a change at `2026-11-11T23:59:59Z` and at `2026-11-12T00:00:00Z`, and per week on a Sunday and a Monday, leave every begun date's result and target unchanged (read before and after, compare every entry)
- [X] T064 [P] [US1] Write `tests/HT.UnitTests/Application/Goals/GoalChangeRetryTests.cs` (after T061; R-05, contracts/application-api.md "Writes"): a `Stale` once (through T035's hook) reads again and succeeds; ten `Stale`s throw `InvalidOperationException` whose message names no value; a concurrent ending between the read and the write gives "This goal has ended." on the retry; a concurrent time zone change merges into the one later version
- [X] T065 [P] [US1] Write `tests/HT.IntegrationTests/Goals/GoalSettingsChangeTests.cs` on `HT-IT`, with the test's own provider and clock (after T061; R-05): the change through the real store; a settings change and a time zone change in both orders leave one later version with the new target and the latest zone; follow `TimeZoneChangeTests.cs`

### Pages for User Story 1 (HP-001, HP-002)

- [X] T066 [US1] Add an edit mode to `src/HT.Ui/Components/Goals/NewGoalForm.razor` (after T011; HP-002): no habit chooser and no start date; filled from `GoalDetails`' latest settings (`Later` when there is one, else the settings in effect) and own name; the schedule choice limited to the goal's kind; refusals shown under the field of the result's `Field`, one at a time, with the result's message as it is. Creation mode is unchanged (`tests/HT.UiTests/Components/Pages/NewGoalPageTests.cs` passes unchanged)
- [X] T067 [P] [US1] Change `src/HT.Ui/Components/Goals/GoalSettingsCard.razor` (after T042, T048; HP-001, contracts/host.md "Settings card"): when `GoalDetails.Later` is set, the row "From {Thu 12 Nov}" with the later target text (`ReportText`); the start row "Started" or "Starts" from `AttemptHasBegun` with `AttemptStartDate`; no start row for an ended goal whose latest attempt has not begun
- [X] T068 [US1] Create `src/HT.Ui/Components/Pages/EditGoalSettings.razor` at `/goals/{id}/edit` (after T061, T066, T067; HP-002, contracts/host.md "Edit settings page"): reads the goal with `GoalService.GetAsync`; another account's or an unknown goal shows Not found; an ended goal redirects to `/goals/{id}`; the form in edit mode, the summary card, "Changes apply from {ChangesApplyFrom}. Past results keep their rules." above the actions, "Save changes" and "Cancel" (back to the goal page); "settings changed" and "settings unchanged" both return to the goal page
- [X] T069 [US1] Add "Edit settings" (ghost, first in the head, a link to `/goals/{id}/edit`) to the head of a goal that is not ended in `src/HT.Ui/Components/Records/GoalDetailBody.razor` (after T068; HP-001). In `tests/HT.UiTests/Components/Pages/GoalDetailPageTests.cs`, the absent list (today it holds "Edit settings", "End goal" and "Earlier attempts") loses "Edit settings" and the page-head list gains it, on purpose (spec "Earlier requirements and tests this slice changes")
- [X] T070 [US1] Write bUnit tests in `tests/HT.UiTests/Components/GoalForms/NewGoalFormEditTests.cs` (create the folder; not `Components/Goals`, whose namespace would hide the `Goals` page type, CS0118), `tests/HT.UiTests/Components/Pages/EditGoalSettingsPageTests.cs` and add to `GoalDetailPageTests.cs` (after T067 to T069; HP-002, HP-009, SC-008): the edit form's fields and limited schedule choice, labels and focus on the first field; each refusal under its field; the "Changes apply from" line; the Settings card's "From Thu 12 Nov" row and its start row forms; the page's Not found and redirect states. Then write `tests/HT.UiTests/Host/GoalChangeHostTests.cs` on `HT-UI`: `/goals/{id}/edit` for the owner, another account (Not found), an ended goal (redirect); SC-001's change in three actions from the goal page ("Edit settings", the amount, "Save changes") and the Settings card's later row after it; follow `GoalsHostTests.cs`

**Checkpoint**: the full suite passes; a goal's settings can be changed from its goal page. Commit.

---

## Phase 4: User Story 2 - End a goal (Priority: P1)

**Goal**: a registered user ends a goal with an outcome and an optional note; the commitment or period in progress
and everything after it leave the attempt, nothing is counted as failed, and the history stays correctable.

**Independent Test**: create "Meditation, 10 min daily" at an earlier supplied time, record from 2 to 13 November,
end it on Monday 16 November at 10:00 with "Abandoned", and read its report on Wednesday 18 November at 12:00:
14 and 15 November Missed, nothing from 16 November on, "85% (12 of 14)", current 0 and longest 12 commitments.

### Implementation for User Story 2

- [X] T071 [US2] Add `GoalEndResult` with `GoalEndOutcome` to `src/HT.Application/Goals/GoalChangeResults.cs` and `EndAsync` to `GoalChangeService` (after T061; FR-007 to FR-009, contracts/application-api.md "EndAsync"): access, goal, steps 4 to 6 in order; E as the Conventions give it; `EndingOf(goal, E)` and `TryEndAsync` through the retry loop; `Ended`, "goal ended", with `EndingDate` and `LastDate`. Add the member to `UseCaseParameterTests.cs` and the result to `PublicSurfaceTests.cs`

### Tests for User Story 2

- [X] T072 [P] [US2] Write `tests/HT.UnitTests/Application/Goals/EndGoalTests.cs` with `AttemptWorld` (after T071; US2-AS1, AS6 to AS10, AS12, FR-007, FR-009): the result, ending date and last date (AS1); the order of the checks (an ended goal with no outcome gives "This goal has already ended."; no outcome with a 501-character note gives "Choose how the goal ended."); a 500-character note accepted, 501 refused; ending again; changing settings and pausing after the end each give "This goal has ended." and write nothing; the pause cuts (AS10, through the shared fakes); ending a goal that has not started uses the account's today, keeps its first version, name and starting values (AS12); ending a waiting attempt 2 after a begun attempt 1
- [X] T073 [P] [US2] Write `tests/HT.UnitTests/Application/Goals/EndGoalBoundaryTests.cs` with `AttemptWorld` (after T071; FR-008, SC-003, SC-005, US2-AS11, US3-AS13): an ending on the start date, before it (a waiting goal) and later; a per-week goal ended on a Wednesday (the week in progress not counted, not Failed, its two "Yes" records kept; AS11), on a Sunday and on a Monday; an ending with a waiting later version (removed) and on the day a change took effect (removed unless first; the name and settings from the last version); the Kyiv ending of US3-AS13 (ending date Tuesday 17 November, last date Monday 16 November, the waiting time zone version removed); in every case no result of the entry in progress or later is counted and no Failed or Missed is added (compare the figures before and after at the same instant)
- [X] T074 [P] [US2] Write `tests/HT.UnitTests/Application/Goals/EndedGoalReadTests.cs` with `AttemptWorld` (after T071; US2-AS2 to AS5, FR-010, FR-025): what can be recorded now at `2026-11-16T10:01:00Z` gives 14 and 15 November and not 16 (AS2); the report at `2026-11-18T12:00:00Z` (AS3); recording 14 November after the end makes it Met, "92% (13 of 14)" (AS4); adding a record for 16 November gives "Nothing is scheduled on this date." (AS5); excusing a Pending commitment of the ended attempt and removing the excusal still work
- [X] T075 [P] [US2] Write `tests/HT.IntegrationTests/Goals/GoalEndingTests.cs` on `HT-IT`, with the test's own provider and clock (after T071; R-05): the ending through the real stores with its pause and excusal cuts read back; an ending and a pause in both orders (the pause after the ending gives "This goal has ended." from `AddPauseAsync`'s re-check, the pause before it is cut); an ending and a time zone change in both orders (no version after the last date remains)

### Pages for User Story 2 (HP-001, HP-003, HP-009)

- [X] T076 [US2] Create `src/HT.Ui/Components/Goals/EndGoalDialog.razor` on `HtDialog` (after T011, T071; HP-003, contracts/host.md "EndGoalDialog"): the title "End goal · {goal name}", the attempt line as sub line, the effect text, the ChoiceCards Achieved, Abandoned and No longer relevant in one column with none chosen on open, "Note (optional)" as `TextArea`, "End goal" and "Cancel"; a refusal under its field ("Choose how the goal ended." under "How did it end?", the note's under the note); focus on the first field on open, Esc and × close
- [X] T077 [US2] Change `src/HT.Ui/Components/Records/GoalDetailBody.razor` (after T069, T076; HP-001): "End goal" (ghost, `aria-haspopup="dialog"`, after "Pause") opening the dialog; the attempt line under the title from `AttemptHasBegun` ("Attempt {n} · started …" or "· starts …"), or "Attempt {n} · ended before it began" for an ended latest attempt that has not begun; for an ended goal the Notice "Ended {date} · {outcome}" with the action "Start new attempt" (a link to `/goals/{id}/new-attempt`, whose page T089 adds), and no "Record", "Edit settings", "Pause" or "End goal"; the History card keeps its record and note actions; after a successful ending the island reads everything again and focus moves to the Notice's "Start new attempt". In `GoalDetailPageTests.cs`, the absent list loses "End goal" on purpose, and every head-button list of a goal that is not ended gains "End goal" (same reason)
- [X] T078 [US2] Write bUnit tests `tests/HT.UiTests/Components/GoalForms/EndGoalDialogTests.cs` and add to `GoalDetailPageTests.cs` (after T077; HP-003, HP-009, SC-008): the dialog's texts, no choice on open, each refusal under its field, focus on open, Esc and ×, focus back to "End goal" on cancel and to "Start new attempt" after an ending; the goal page's ended state (Notice, no head actions, History actions kept) and the three attempt-line forms. Add to `tests/HT.UiTests/Host/GoalChangeHostTests.cs`: SC-001's ending in three actions ("End goal", the outcome, "End goal") and the Notice after it

**Checkpoint**: the full suite passes; a goal can be ended from its goal page. Commit.

---

## Phase 5: User Story 3 - Start a new attempt (Priority: P1)

**Goal**: a registered user starts a new attempt of an ended goal, with the earlier settings or new ones of any
schedule kind; it has its own history and a streak from zero, and earlier attempts keep their figures in an attempt
view.

**Independent Test**: end "Meditation, 10 min daily", start attempt 2 on Wednesday 18 November, record 18 to 20
November, and read on Monday 23 November: attempt 2 "100% (3 of 3)" and 3 commitments; attempt 1 "85% (12 of 14)"
and longest 12 commitments, unchanged.

### Implementation for User Story 3

- [X] T079 [US3] Create `src/HT.Application/Goals/NewAttemptInput.cs` and `NewAttemptValues.cs`, and add `NewAttemptValuesResult` and `AttemptStartResult` with their outcome enums to `src/HT.Application/Goals/GoalChangeResults.cs`, as data-model.md gives them (after T071); record the enum names in data-model.md
- [X] T080 [US3] Add `GetNewAttemptValuesAsync` and `StartAttemptAsync` to `GoalChangeService` (after T079; FR-011, FR-012, contracts/application-api.md): the starting values from the ended attempt's last version (as `GoalStartingValues` gives target fields), `NewAttemptEarliestStart(goal, account today)` and the next number; the start checks in order (not ended, `CheckTarget`, missing, before the account's today, on or before the ending date with `GoalProblem.StartDateNotAfterEnding`, more than 365 days ahead); `TryStartAttemptAsync` through the retry loop with the first version from the start date in the account's current zone; no catalog lookup (AC-064). Add both members to `UseCaseParameterTests.cs` and the results to `PublicSurfaceTests.cs`
- [X] T081 [US3] Create `src/HT.Application/Reports/AttemptView.cs`, `AttemptDetails.cs` and the results `AttemptListResult` and `AttemptReadResult` with their outcome enums (`src/HT.Application/Reports/AttemptResults.cs`), as data-model.md gives them; record the enum names in data-model.md (after T045)
- [X] T082 [US3] Create `src/HT.Application/Reports/AttemptService.cs` with the constructor of contracts/application-api.md, `GetAttemptsAsync` and `GetAttemptAsync` (after T081; FR-014, contracts/application-api.md "AttemptService"): access, goal, then every attempt latest first, each evaluated through `GoalEvaluation.OfAsync(…, number, …)` at one time read, `Name` by FR-004 (`GoalText.Name` at the instant for the latest, `AttemptName` otherwise), `HasNoDate` from T017's helper; one attempt with `AttemptNotFound`, "This attempt does not exist." for an unknown number, its recent results, and its History (start date to last date or current date, newest first, the newest 366) built with `HistoryOf` and that attempt's `VersionInEffect`
- [X] T083 [US3] In the same commit as T082: register `AttemptService` in `src/HT.Application/ApplicationServiceCollectionExtensions.cs` and `tests/HT.UiTests/Components/Pages/PageServices.cs`; admit it in the `IGoalStore` rule of `tests/HT.ArchitectureTests/ApplicationDependencyTests.cs` (today `OnlyTheGoalTypesTheThreeHabitServicesTheProfileServiceTheRecordingServiceTheReportServiceAndTheTwoExemptionServicesMayDependOnTheGoalStorePort`), renaming it and its doc comment to name the attempt service; add `AttemptService` and its views and results to `PublicSurfaceTests.cs` and its members to `UseCaseParameterTests.cs`

### Tests for User Story 3

- [X] T084 [P] [US3] Write `tests/HT.UnitTests/Application/Goals/StartAttemptTests.cs` with `AttemptWorld` (after T080; US3-AS1, AS2, AS4 to AS8, FR-011, FR-012): the starting values on 18 November and on the day of the ending (AS1); starting with them unchanged (AS2: number 2, start 18 November, not ended, "N/A", 0 commitments); new settings from 23 November (AS4: the name "Meditation, 5 min daily" at once, next commitment 23 November, attempt 1 named by its own settings, the current date 19 November, recording 15 November accepted); another schedule kind accepted with the streak unit "weeks" (AS5); each start refusal in order, one per message, and each target refusal (AS6); a goal not ended gives "Only an ended goal can start a new attempt." for both use cases (AS7); a record between the attempts gives "Nothing is scheduled on this date." (AS8); an Unpublished catalog habit can start a new attempt (AC-064); `AttemptStartResult.Field` for each refusal
- [X] T085 [P] [US3] Write `tests/HT.UnitTests/Application/Goals/StartAttemptBoundaryTests.cs` with `AttemptWorld` (after T080; SC-004, SC-005, US3-AS13): a restart on the ending date refused and on the day after accepted; the Kyiv case of US3-AS13 (starting values Wednesday 18 November while the account's today is Monday 16 November; Tuesday 17 November refused); the new attempt's figures start at "N/A" and 0, and recording in it never changes the earlier attempt's figures (read both before and after); restarting a per-week goal starts its first week on the first Monday on or after the start date
- [X] T086 [P] [US3] Write `tests/HT.UnitTests/Application/Reports/AttemptReadTests.cs` with `AttemptWorld` (after T082; US3-AS3, AS9 to AS12, FR-014, FR-016, FR-019): attempt 2's report on 23 November (AS3); the attempts list latest first with attempt 1's ending fields, outcome, note, figures and longest streak (AS9); a correction of 15 November changes attempt 1's figures only (AS10); attempt 1's History (2 to 15 November, newest first, nothing from 16 November) and recent results oldest first, and attempt 3 gives "This attempt does not exist." (AS11); an earlier attempt keeps "commitments" as its unit when attempt 2 counts "weeks", and its own Build after a restart as Reduce; an attempt with no date has an empty History and "N/A" (AC-102); a History longer than 366 dates gives the newest 366; the restart of AS12 (recordable and dashboard give attempt 1's Pending 15 November marked as attempt 1 and attempt 2's Due 17 November, attempt 1's Missed 14 November neither listed nor counted, recording 15 November counts in attempt 1 only)
- [X] T087 [P] [US3] Write `tests/HT.IntegrationTests/Goals/GoalAttemptTests.cs` on `HT-IT`, with the test's own provider and clock (after T083; R-05, FR-014): ending, restarting and reading attempts through the real stores; two starts racing (the second gives `Stale`, reads again and gives "Only an ended goal can start a new attempt."); an ending and a start racing in both orders; the attempt list costs one record read per attempt

### Pages for User Story 3 (HP-001, HP-004, HP-014)

- [X] T088 [US3] Add an attempt mode to `src/HT.Ui/Components/Goals/NewGoalForm.razor` (after T066, T080; HP-004): every schedule kind and the start date, no habit chooser and no name, filled with `NewAttemptValues`; refusals under their field, one at a time
- [X] T089 [US3] Create `src/HT.Ui/Components/Pages/NewAttempt.razor` at `/goals/{id}/new-attempt` (after T088; HP-004, contracts/host.md "Start new attempt page"): reads the starting values; another account's or an unknown goal shows Not found; a goal not ended redirects to `/goals/{id}`; the line "Attempt {n} starts with its own history and a streak from zero."; "Start attempt" and "Cancel"; "attempt started" opens the goal page
- [X] T090 [P] [US3] Create `src/HT.Ui/Components/Goals/EarlierAttemptsCard.razor` (after T042, T082; HP-001): for each ended attempt that is not the latest, latest first, "Attempt {n} · {first} – {last}" or "Attempt {n} · ended before it began" as a link to `/goals/{id}/attempts/{n}`, its name when it differs from the goal's current name, its outcome `Tag` (with its word), its consistency and its note in `ht-muted`; absent when there is none. Add it to the right column of `GoalDetailBody.razor` after Recent notes, reading `AttemptService.GetAttemptsAsync` in the island's read. In `GoalDetailPageTests.cs`, the absent list loses "Earlier attempts" on purpose
- [X] T091 [US3] Create `src/HT.Ui/Components/Pages/AttemptDetail.razor` at `/goals/{id}/attempts/{n}` (after T082, T058; HP-014, contracts/host.md "Attempt view"): the latest attempt's number, an unknown number, another account's or an unknown goal shows Not found; "← {goal name}"; the heading, name, outcome `Tag` and note; `GoalStats` with the eyebrow "Attempt {n}"; `HistoryCard` over `AttemptDetails.History`, whose record and note actions open the RecordDialog or NoteDialog fixed to their row with that row's `Settings`; no "Record", "Edit settings", "Pause", "End goal", trend, Settings, Pauses or Recent notes card. The page is static, like `GoalDetail.razor`, and its body is a new island `src/HT.Ui/Components/Records/AttemptDetailBody.razor` (`@rendermode InteractiveServer`, as `GoalDetailBody.razor`), which reads `GetAttemptAsync` in `OnInitializedAsync` and again after every write. FR-010 keeps excusals working for an ended attempt's dates, so the History card's "Remove excusal" and the RecordDialog's "Excuse instead" are wired as on the goal page (through `ExcusalService`, then a re-read); `HistoryCard` and `RecordDialog` are reused unchanged
- [X] T092 [US3] Write bUnit tests `tests/HT.UiTests/Components/GoalForms/NewGoalFormAttemptTests.cs`, `EarlierAttemptsCardTests.cs`, `tests/HT.UiTests/Components/Pages/NewAttemptPageTests.cs` and `AttemptDetailPageTests.cs`, and add the attempt view as an opener to `RecordDialogTests.cs` and `NoteDialogTests.cs` (after T089 to T091; HP-004, HP-009, HP-014, SC-008): the attempt form's fields, labels and start date, focus on the first field when the page opens, each refusal under its field, the line; the card's two heading forms, name only when it differs, Tag, consistency, note, absence; the attempt view's content and absences, its empty History for an attempt with no date, "Add record" on 14 November opening the RecordDialog for that date (US3-AS15), "Remove excusal" on an excused row of the ended attempt, focus return. Add to `tests/HT.UiTests/Host/GoalChangeHostTests.cs`: the new routes' Not found and redirect states, starting a new attempt from the goal page's Notice in two actions ("Start new attempt", "Start attempt"; US3-AS14), and the attempt view of attempt 1

**Checkpoint**: the full suite passes; an ended goal can be restarted, and earlier attempts open in their view.
Commit.

---

## Phase 6: User Story 4 - Ended goals and earlier attempts in lists (Priority: P2)

**Goal**: ended goals leave the active views but stay on the Goals page's Ended tab and in the Profile's Goal
history; the Home keeps their Pending rows with a Tag.

**Independent Test**: end one of three goals and restart a third, then read the overview, the dashboard and the goal
history on Monday 16 November at 12:00: the ended goal flagged with "Abandoned", the dashboard keeping only its
Pending entries, both ended attempts in the goal history, latest ending first.

### Implementation for User Story 4

- [X] T093 [US4] Create `src/HT.Application/Reports/EndedAttemptView.cs` and add `GoalHistoryListResult` with its outcome enum to `src/HT.Application/Reports/AttemptResults.cs`, and `GetGoalHistoryAsync` to `AttemptService` (after T083; FR-015): access only (no goal), the owner's goals from `ReadOwnerGoalsAsync` (one goal list read), every ended attempt with `AttemptName` (or the goal's name at the instant for its latest attempt), the latest ending date first, then `HabitName.KeyOf` ordinally, then the number. Add the member to `UseCaseParameterTests.cs` and the types to `PublicSurfaceTests.cs`; record the enum name in data-model.md

### Tests for User Story 4

- [X] T094 [P] [US4] Write `tests/HT.UnitTests/Application/Reports/GoalHistoryTests.cs` with `AttemptWorld` (after T093; US4-AS3, FR-015, AC-103): US4's three goals give Meditation's attempt 1 then Sugar's attempt 1 with their fields and `IsLatestAttempt` (true, false); two endings on one date ordered by name, then by number; a habit renamed after the ending names the attempt with the new name; an empty history
- [X] T095 [P] [US4] Write `tests/HT.UnitTests/Application/Reports/EndedGoalListsTests.cs` with `AttemptWorld`, making US4's three goals through `GoalChangeService` (after T080, T093; US4-AS1, AS2, AS4, AS5): the overview flags Meditation ended on 16 November with "Abandoned" in slice 6's order; the dashboard at 12:00 on 16 November and on 18 November; Sugar's attempt 2 with "N/A"; the catalog with "Only habits I track", Meditation's count 0 and no goals in its details

### Pages for User Story 4 (HP-005 to HP-007)

- [X] T096 [US4] Change `src/HT.Ui/Components/Reports/GoalsList.razor` and `src/HT.Ui/Components/Shared/GoalItem.razor` and `src/HT.Ui/Components/Pages/Goals.razor` (after T008, T011, T042, T047; HP-005): the Tabs "Active", "Paused" and "Ended" (`?tab=ended`); Active and Paused leave ended goals out; Ended lists them in the overview's order; `GoalItem`'s ended form with the outcome `Tag`, "Ended {date}", figures and strip, the secondary "Start new attempt" (a link to `/goals/{id}/new-attempt`) and no "Record"; "No ended goals."
- [X] T097 [P] [US4] Change `src/HT.Ui/Components/Pages/Profile.razor` (after T010, T011, T093; HP-006): the tab "Goal history" after "Account" (`?tab=history`), a `DataTable` with Goal (a link to `/goals/{id}/attempts/{n}`, or to `/goals/{id}` when `IsLatestAttempt`), Attempt, Ended, Outcome (`Tag`) and Note (`ht-muted`, or "—") in `GetGoalHistoryAsync`'s order; "No ended goals yet."; an administrator's Profile keeps its Account content without Tabs and ignores `tab`
- [X] T098 [P] [US4] Change `src/HT.Ui/Components/Reports/TodayPanel.razor` and `src/HT.Ui/Components/Shared/CommitmentRow.razor` (after T009, T011, T047, T058; HP-007): the `Tag` after the goal name, "Attempt {n}" when `IsLatestAttempt` is false, otherwise "Ended" when `TodayEntry.GoalEnded`; the Pending rows of ended attempts stay in the list and the "Pending · still can record" tile; the other tiles, the Active goals card and recent notes leave ended goals out (through the dashboard's data, no filtering in the component)
- [X] T099 [US4] Write bUnit tests (after T096 to T098; HP-005 to HP-007, SC-008): add to `tests/HT.UiTests/Components/Reports/GoalsListTests.cs` (the Ended tab, its item, the empty text, the tab in the address), the `GoalItem` tests (find them with `grep -rln "GoalItem" tests/HT.UiTests --include=*.cs`), `TodayPanelTests.cs` and `CommitmentRowTests.cs` (both Tags, never by colour alone), and the Profile's page tests (find them with `grep -rln "Profile" tests/HT.UiTests/Components --include=*.cs`; create `tests/HT.UiTests/Components/Pages/ProfilePageGoalHistoryTests.cs` if none renders the Profile in bUnit). The tests that assert no Ended tab, no Goal history tab, no "Start new attempt" or no outcome Tag change on purpose (spec "Earlier requirements and tests this slice changes"); list them first with `grep -rn "Ended\|Goal history\|Start new attempt" tests/HT.UiTests --include=*.cs`. Then add to `tests/HT.UiTests/Host/GoalsHostTests.cs`, `ProfilePageTests.cs` and `HomeDashboardHostTests.cs`: `/goals?tab=ended`, `/profile?tab=history` for a user and an administrator, the Home's Tags; and to `GoalChangeHostTests.cs`: SC-001's new attempt from the Ended tab in two actions ("Start new attempt", "Start attempt")

**Checkpoint**: the full suite passes; ended goals are on the Ended tab and in the Goal history. Commit.

---

## Phase 7: User Story 5 - Privacy of goal changes (Priority: P1)

**Goal**: settings changes, endings, outcomes, ending notes and attempts are private to the goal's owner,
administrators included.

**Independent Test**: try each of the seven use cases as another registered user, an administrator and an anonymous
visitor on someone else's goal; each is refused with its text and nothing changes.

### Tests for User Story 5

- [ ] T100 [P] [US5] Write `tests/HT.UnitTests/Application/Goals/GoalChangeAccessTests.cs` and `tests/HT.UnitTests/Application/Reports/AttemptAccessTests.cs` (after T080, T093; US5-AS1 to AS4, FR-022, FR-023, SC-006): for each of `ChangeSettingsAsync`, `EndAsync`, `GetNewAttemptValuesAsync`, `StartAttemptAsync`, `GetAttemptsAsync` and `GetAttemptAsync`: another user gives "This goal does not exist.", an administrator "Goals are for registered users.", an anonymous visitor and an account that is not stored "You are not signed in."; each refusal returns no view and writes nothing (`FakeGoalStore.ChangeWriteCount` unchanged and `FakeRecordStore.Calls` without a write; a refusal before the goal leaves `Lookups` at 0); `GetGoalHistoryAsync` for an administrator and an anonymous visitor; user B's goal history contains none of user A's attempts; follow `GoalAccessTests.cs` and `ReportAccessTests.cs`
- [ ] T101 [P] [US5] Write `tests/HT.IntegrationTests/Goals/GoalChangePrivacyTests.cs` on `HT-IT` (after T083, T093; FR-022, SC-006): through the real stores, user B's calls on user A's goal change nothing of it (read A's goal, versions, attempts, endings, pauses and excusals before and after), `GetAttemptAsync` with A's goal and a valid number gives "This goal does not exist." to B, and B's goal history leaves out A's attempts; follow `GoalPrivacyTests.cs`
- [ ] T102 [P] [US5] Write `tests/HT.IntegrationTests/Goals/GoalChangeLogTests.cs` with `LogCapture` (after T083, T093; US5-AS5, HP-010): run the seven use cases with an ending note, a note, a value, a goal name, a habit name and a unit made of unique markers, including the `Stale`-exhaustion throw and a mapping fault from a corrupted ending row; no log line or exception message contains any marker or an outcome word; follow `GoalLogTests.cs` and `ExemptionLogTests.cs`
- [ ] T103 [P] [US5] Add to `tests/HT.UiTests/Host/GoalPrivacyHostTests.cs` (after T070, T092, T099; HP-002, HP-004, HP-006, HP-010, HP-014): `/goals/{id}/edit`, `/goals/{id}/new-attempt` and `/goals/{id}/attempts/{n}` of another account's goal show Not found, for an anonymous visitor they go to sign-in, and an administrator gets nothing of the goal; with log capture over these requests and the three pages' writes, no log line or error page contains the ending note, the goal name or the habit name

**Checkpoint**: the full suite passes. Commit.

---

## Phase 8: User Story 6 - Demo goal changes (Priority: P2)

**Goal**: a seventh startup task creates `demo7@example.com` with a changed target and a waiting change, an ended
goal and a goal on attempt 2; the smoke check finds them on the hosted demo.

**Independent Test**: run demo goal-change loading on a reset database at a fixed run time and read `demo7`'s
goals, attempts, reports and goal history: Reading "96% (25 of 26)", Meditation attempt 2 "100% (12 of 12)" and
attempt 1 "52% (11 of 21)", Sugar "100% (23 of 23)", the dashboard 2 Due, 4 Pending, 0 Missed this week, 2 goals.

### Implementation for User Story 6 (data and port)

- [ ] T104 [P] [US6] Create `src/HT.Application/Operator/DemoVersion.cs`, `DemoAttempt.cs`, `DemoGoalChangeGoal.cs` and `DemoGoalChanges.cs` as data-model.md gives them (after T013; FR-031, R-09): `DemoGoalChanges.For(DateOnly runDate)` builds FR-031's three goals with every date as R − k; the note and ending-note constants "Short on time.", "Too busy." and "Three weeks without extra sweets."; the goal names; reuse slice 6's `DemoRecord` and `DemoNote`
- [ ] T105 [US6] Create `src/HT.Application/Operator/IDemoGoalChangeStore.cs`, `DemoGoalChangeLoadingResult.cs` (with its own outcome enum `DemoGoalChangeLoadingOutcome`, as `DemoPauseLoadingResult` has one), `DemoGoalChangeAccountResult.cs` and `DemoGoalChangeService.cs` (after T104; FR-031, contracts/application-api.md "DemoGoalChangeService"): follow `DemoPauseService.cs` and its results; R is `defaultZone.DateAt(now)`; the password checks and "already exists" (ignoring case, either role, or the store's `EmailTaken`) with no read of the existing account (ADR 0013); a goal whose catalog habit is not Published (by name, ignoring case) is left out with its attempts, records and notes; the texts from `OperatorTexts`; the results name only the email. Record the enum in data-model.md "Application: demo goal-change loading"
- [ ] T106 [P] [US6] Create `tests/HT.UnitTests/Fakes/FakeDemoGoalChangeStore.cs` (after T105), following `FakeDemoPauseStore.cs`
- [ ] T107 [US6] Add internal methods for attempts (rows with their endings) and per-attempt versions to `src/HT.Persistence/Habits/DemoAccountWriter.cs`, in the same transaction as the goal, its records and notes, and create `src/HT.Persistence/Habits/DemoGoalChangeStore.cs`, an `internal sealed` class implementing `IDemoGoalChangeStore` over it, following `DemoPauseStore.cs` (after T037, T105; R-09). The rows pass `GoalMapping`'s checks of T037
- [ ] T108 [US6] In the same commit as T105 to T107: register `DemoGoalChangeService` in `src/HT.Application/ApplicationServiceCollectionExtensions.cs` and `IDemoGoalChangeStore` in `src/HT.Persistence/PersistenceServiceCollectionExtensions.cs`; add `OnlyTheDemoGoalChangeServiceMayDependOnTheDemoGoalChangeStorePort` to `tests/HT.ArchitectureTests/ApplicationDependencyTests.cs`, as the `IDemoPauseStore` rule does; in `PersistenceDependencyTests.cs`, add `[InlineData("HT.Persistence.Habits.DemoGoalChangeStore")]` to `TheDemoAccountWriterTheDemoReportStoreAndTheDemoPauseStoreAreInternal`, renaming it and its doc comment to name all four demo stores; in `UiDependencyTests.cs`, add `NoUiTypeMayDependOnTheDemoGoalChangeStorePort` as `NoUiTypeMayDependOnTheDemoPauseStorePort` does, and add `DemoGoalChangeService` to `OnlyTheHostsOperatorTypesMayDependOnTheOperatorServices`; add the new public types to `PublicSurfaceTests.cs`. Check every place with `grep -n "IDemoPauseStore\|DemoPauseService" tests/HT.ArchitectureTests/*.cs`

### Tests for User Story 6

- [ ] T109 [P] [US6] Write `tests/HT.UnitTests/Application/Operator/DemoGoalChangesTests.cs` and `DemoGoalChangeLoadingTests.cs` (after T051, T105, T106, T082, T093; FR-031, US6-AS1, AS3, AS4, R-09): `DemoGoalChanges.For(R)` gives exactly FR-031's versions, attempts, endings, dates, values and notes for R on each of the seven weekdays; every goal passes the `Goal` constructor's invariants when built from it; every note passes `NoteText` and `EndingNote`; loading with no password, a blank one and a weak one gives its result and creates nothing; "created", "demo goals added", "demo records added" for `demo7@example.com`, also with some or all habits not Published; "already exists" for an existing account, any case, either role, and for the store's `EmailTaken`; no result names a goal, habit, note, outcome, unit or amount; follow `DemoPauseLoadingTests.cs` and `DemoPausesTests.cs`. R-09 also has FR-031's figures proved by unit tests: build `Goal`s from `DemoGoalChanges.For(R)` on `AttemptWorld` (T051) with the demo records, and read them at the run time on each weekday through `ReportService`, `AttemptService` and `GetGoalHistoryAsync` (Reading "96% (25 of 26)", 6 and 19; Meditation attempt 2 "100% (12 of 12)" and attempt 1 "52% (11 of 21)"; Sugar "100% (23 of 23)"; the dashboard 2 Due, 4 Pending, 0 Missed, 2 goals; the goal history order). T110 proves the same through the real stores
- [ ] T110 [P] [US6] Write `tests/HT.IntegrationTests/Operator/DemoGoalChangeLoadingTests.cs` on `HT-IT-SEED` (after T108, T082, T093; FR-030, FR-031, US6-AS1 to AS5, SC-007): after a reset, the seed catalog and the four earlier demo tasks, with the test's own provider at a run time on each of the seven weekdays: `demo7`'s goals, attempts, reports, dashboard and goal history read at the run time give FR-031's figures (Reading "96% (25 of 26)", 6 and 19 commitments, R − 2 and R − 1 Pending, R Due, 20 minutes from R − 13 and the later 25 minutes from R + 1; Meditation attempt 2 from R − 14, "100% (12 of 12)", 12 and 12, attempt 1 ended R − 21, last date R − 22, "Abandoned", "Too busy.", "52% (11 of 21)", longest 11; Sugar ended R − 7, "Achieved", "100% (23 of 23)", 23 and 23 days; the dashboard 2 Due, 4 Pending, 0 Missed this week, 2 goals; the goal history Sugar's attempt 1 then "Meditation, 10 min daily"'s attempt 1); a fault in the middle of the write leaves no row of the account; a second run gives "already exists" and changes nothing; the earlier demo tasks leave no ending, second attempt or settings change and their accounts' figures unchanged (FR-030); follow `DemoPauseLoadingTests.cs`

### Host and smoke check (HP-011, HP-012)

- [ ] T111 [US6] Add the step "Demo goal-change loading" after "Demo pause loading" to `src/HT.Ui/Operator/StartupTasks.cs` through `RunStepAsync`, with a `LoggerMessage` "Demo goal-change loading: {Result}" for a missing or weak password, the account lines through `LogHistoryAccountLines` as demo pause loading logs them, and the fault line "Demo goal-change loading failed; the app starts without it" through `LogStepFailed`, with no secret or value (after T108; HP-011, contracts/host.md "Operator log"). Update `tests/HT.UiTests/Host/StartupLogTests.cs` and `tests/HT.UiTests/Operator/StartupTasksDemoTests.cs`, whose exact "Demo " lines now expect `demo7@example.com` after `demo6@example.com` (a change on purpose), and add the missing-password, weak-password and fault cases (the fault does not stop the start)
- [ ] T112 [US6] Add check 9 "demo goal changes" to `tools/HT.SmokeCheck/SmokeCheckRunner.cs`, reading with `tools/HT.SmokeCheck/PageReader.cs` (add a reader there only if none of its existing ones finds the Settings card's "From …" row, the attempt line or the Goal history table, with its cases in `tests/HT.UiTests/SmokeChecks/PageReaderTests.cs`), and the `SmokeCheckCoverage` entries for every data parameter of `IDemoGoalChangeStore.CreateDemoGoalChangeAccountAsync` (email, password, profile, goals) in `tools/HT.SmokeCheck/SmokeCheckCoverage.cs` (after T099, T111; HP-012, contracts/host.md "Smoke check"): signed in as `demo7@example.com`, "Sugar, at most 1 sweet a day" on `/goals?tab=ended`, "Attempt 2" on the page of "Meditation, 5 min daily", the "From …" row with "25 min daily" on the page of "Reading, 20 min daily", and "Goal history" with "Abandoned" on `/profile?tab=history`; it prints `PASS demo goal changes demo7@example.com (N of N)` or its FAIL line, never a goal name, note, outcome or figure. In the same change: `RunnerBehaviourTests.GoodSite()`, `SmokeCheckHostTests`' exact lines and its "(N of N)", `CoverageTests` (`IDemoGoalChangeStore` joins its `Ports` list and the class's doc comment; 18 to 22 entries; each count today near lines 97, 115 and 133 either gains the new port's four entries or leaves them out, whichever states its rule), and the coverage table and check 9 in `specs/003-demo-hosting/contracts/smoke-check.md`

**Checkpoint**: the full suite passes; the host runs demo goal-change loading as startup step 7, after demo pause
loading, and the smoke check has check 9. Commit.

---

## Phase 9: Polish & cross-cutting concerns

- [ ] T113 [P] Update `docs/operations/hosted-demo.md` (`demo7@example.com`, the new startup task, check 9) and the Status section of `README.md` (slice 8's line, as slice 7's reads) (plan, Work order 8)
- [ ] T114 [P] Update `CONTEXT.md` only if implementation sharpened a term; HP-013's entries are already there. Record "no change" in the task's note otherwise
- [ ] T115 Run `tests/HT.IntegrationTests/Reports/ReportTimingTests.cs` (slice 6's SC-007 timing: 3 goals, one with 366 daily results, each read under 2 seconds) unchanged; it must pass (plan "Performance Goals")
- [ ] T116 Run Stryker from `tests/HT.UnitTests`: `dotnet stryker --project HT.Domain.csproj` and `dotnet stryker --project HT.Application.csproj`, the whole project (in a worktree `--since` finds nothing; ADR 0022). Fix every surviving mutant in the new and changed code (`Goal`, `GoalAttempt`, `EndingNote`, `GoalEnding`, `GoalTarget.HasSameScheduleKind`, `GoalDraft.CheckTarget`, `GoalTimeline`, `GoalVersionChange`, `GoalText`, `TrendBuilder`, `GoalChangeService`, `AttemptService`, `EntryViews`, `GoalEvaluation`, `RecordingService`'s changed paths, `ReportService`'s changed reads, `GoalService.DetailsOf`, `GoalListItems`, `CatalogService`, `PauseService`, `ExcusalService`, `RecordPreview`, `RecordingText`, `ReportText`, `DemoGoalChanges`, `DemoGoalChangeService`) with a test; a `// Stryker disable` comment only for an equivalent mutant, with its reason. Report the scores
- [ ] T117 Compare the public API of `HabitTracker.slnx` with T002's baseline (FR-024): run the three `roslynator list-symbols` commands again and diff them. Only additions are allowed, and only those listed in data-model.md and the Conventions, plus the removal of the internal `Goal.Target` (not public); a changed or removed public member is a failure to fix, not to accept
- [ ] T118 [P] Run `roslynator analyze HabitTracker.slnx` and `jb inspectcode HabitTracker.slnx --stdout -f=Text -e=WARNING --caches-home=<scratchpad>/jbcache --no-updates` and compare with the `CLAUDE.md` baseline (692 diagnostics, 51 not from the compiler; 231 `jb` warnings, 2026-11-21). Fix every new finding in this slice's code that is not one of the known false positives; update the baseline numbers and date in `CLAUDE.md`, saying where the change comes from
- [ ] T119 Run `dotnet build HabitTracker.slnx` and `dotnet test HabitTracker.slnx --nologo -v q --logger "console;verbosity=minimal"`: no warning, every test passes. Then walk quickstart.md section 3 against the running app as far as the session allows (sign-in as `demo7@example.com` uses the demo password from User Secrets, which is never read or printed; if it cannot be typed by the owner, label that part unverified). The hosted steps of quickstart.md section 4 stay unverified until the deployment (constitution V)
- [ ] T120 Keep slice 8's Status in `docs/roadmap.md` at `implementing` and set its `updated_at` to today, as slice 7 did at this point. The next status, `AC tests green`, is set after `/speckit-ac-tests-analyze`, which the owner starts

---

## Dependencies & execution order

### Phase dependencies

- **Setup (Phase 1)**: no dependency.
- **Foundational (Phase 2)**: after Setup. Blocks every story. Inside it: design (T003 to T012) and Domain (T013 to
  T032) can run side by side; the storage (T033 to T041) needs T015; the texts and per-entry settings (T042, T043)
  need T015; the shared read path (T044 to T050) needs T017 to T021, T034, T036 and T043, and T046 also needs T039
  (the real `RecordStore` overload that `RecordLogTests` runs); its tests (T051 to T057) follow their tasks; the
  shared components (T058, T059) need T043, T046 and T047.
- **US1 (Phase 3)**: after Phase 2.
- **US2 (Phase 4)**: after US1 (`EndAsync` joins `GoalChangeService` and its retry loop, T061; the goal page's head,
  T069).
- **US3 (Phase 5)**: after US2 (an ended goal is made through `EndAsync`; the Notice's link, T077).
- **US4 (Phase 6)**: after US3 (`GetGoalHistoryAsync` joins `AttemptService`; the Ended tab's "Start new attempt"
  opens T089's page).
- **US5 (Phase 7)**: after US1 to US4 (its tests cover all seven use cases and the three new pages).
- **US6 (Phase 8)**: after Phase 2 for T104 to T108; T109 and T110 also after US3 and US4, because they read
  attempts (`AttemptService`, T082) and the goal history (T093); T111 after T108; T112 after US1 to US4 (the
  "From …" row, the attempt line, the Ended tab and the Goal history).
- **Polish (Phase 9)**: after every story.

### User story dependencies

- **US1 (P1)**: Phase 2 only. MVP.
- **US2 (P1)**: needs US1's service and goal page head.
- **US3 (P1)**: needs US2 for an ended goal.
- **US4 (P2)**: needs US3's `AttemptService` and new-attempt page; its read rules are in Phase 2.
- **US5 (P1)**: needs US1 to US4.
- **US6 (P2)**: the data, port and service need Phase 2 only; their figure tests (T109, T110) need US3 and US4; the
  smoke check needs US1 to US4.

### Within each story

Application first, then its tests (which name the task they follow), then the components, then the page, then the
host tests. Tests written before their implementation task fail first, then pass.

## Parallel opportunities

- Phase 2: T003 to T010 and T012 (nine design and ADR files); T013 and T014 side by side; T020 and T021 beside T018
  and T019; T022 to T028, T030 and T031 (nine test files) once their tasks are done, then T029 after T027; T037
  beside T035 and T036; T040 and T041 side by side; T042 beside the storage tasks; T051 to T057 side by side once their tasks are done (T053 to T056 use T051's
  world).
- US1: T062 to T065 once T061 is done; T067 beside T066.
- US2: T072 to T075 once T071 is done.
- US3: T084 to T087 once T080 and T083 are done; T090 beside T088 and T089.
- US4: T094 and T095 once T093 is done; T097 and T098 beside T096.
- US5: T100 to T103 side by side.
- US6: T104 beside the earlier phases; T106 once T105 is done; T109 once T106 and US4 are done; T110 once T108 and
  US4 are done.

### Parallel example: Phase 2 Domain tests

```text
After T013 to T021:
  T022 EndingNote/GoalEnding     T023 GoalDraftTests          T024 GoalTests
  T025 GoalTimelineAttempt       T026 GoalTimelineEnding      T027 GoalTimelineSettingsChange
  T028 MixedCalendar             T030 GoalTextTests
  T031 TrendBuilderMeasurementChange
Then T029 Equivalence after T027 (both touch GoalTimelineChangeTests.cs).
```

### Parallel example: User Story 3

```text
After T080 (GoalChangeService) and T083 (AttemptService):
  T084 StartAttemptTests.cs        T085 StartAttemptBoundaryTests.cs
  T086 AttemptReadTests.cs         T087 GoalAttemptTests.cs (integration)
  T090 EarlierAttemptsCard.razor   (beside T088 NewGoalForm attempt mode -> T089 NewAttempt.razor)
Then T091 (AttemptDetail), then T092 (bUnit and host).
```

## Implementation strategy

### MVP first (User Story 1)

1. Phase 1 and Phase 2: the design, ADR 0027, the Domain rules, the storage and the shared read path. After Phase 2
   every read already respects attempts, endings and settings versions for seeded goals.
2. Phase 3 (US1): changing settings from the goal page.
3. **Stop and validate**: change a target, read the history and report on both sides of the boundary.

### Incremental delivery

1. US1, then US2, then US3: change, end and restart, each usable from the pages.
2. US4: the Ended tab, the Goal history and the Home's Tags.
3. US5: privacy across all seven use cases and the three new pages.
4. US6: the demo account, the startup step and check 9.
5. Polish: the timing re-run, Stryker, the API comparison, the analyzers, the docs and the full run.

Each checkpoint leaves the full suite green and is committed. The acceptance tests of all seven features with
acceptance tests (F001, F002, F004, F005, F007, F008, F009) run after `/speckit-implement`, when the owner starts
`/speckit-ac-tests-write` and `/speckit-ac-tests-analyze`.
