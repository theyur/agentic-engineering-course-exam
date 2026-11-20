---

description: "Task list for slice 5a, note editing"
---

# Tasks: Note editing

**Input**: Design documents from `specs/006-note-editing/`: [plan.md](plan.md), [spec.md](spec.md),
[research.md](research.md), [data-model.md](data-model.md), [contracts/application-api.md](contracts/application-api.md),
[contracts/host.md](contracts/host.md), [quickstart.md](quickstart.md); [ADR 0019](../../docs/adr/0019-note-editing.md).

**Prerequisites**: F005's `acceptance.md` and `tests/HT.AcceptanceTests/Features/F005/` are locked with the revised
criteria (AC-037, AC-070 to AC-072). No task edits them, the lock files, the run records or anything under
`tests/HT.AcceptanceTests`. This feature has no `acceptance.md` of its own (spec D-2).

**Tests**: required. The constitution (II, III, V) and `CLAUDE.md` make unit, integration, architecture and
selective UI tests part of done, so every story phase has test tasks. xUnit v2 everywhere, hand-written fakes,
`FakeTimeProvider`, no mocking library (ADR 0006). Acceptance step definitions are not tasks here: the owner starts
`/speckit-ac-tests-write 005` and `/speckit-ac-tests-analyze` after `/speckit-implement` (plan, Work order 6).

**Organization**: Phase 2 follows the plan's Work order steps 1 to 3, with one move (see Notes):
`RecordingText.NoteHelp` goes to US1 (T024), next to its only caller. Phase 2 holds:

- design;
- slice 5's four documents, amended in place;
- the port change and everything that must change with it to compile and pass: `NoteWrite`, `IRecordStore`,
  `RecordingService`'s mapping, `RecordStore`, `FakeRecordStore`, every caller of the port in the tests, the slice 5
  tests that encode the old rule, the store tests of the three cases and the architecture rules.

The port change is one compile unit, and the mapping change turns three slice 5 unit tests red. So Phase 2 changes
them in the same commit (constitution V; research R-10). The story phases then add the new use-case tests and the
pages (plan, Work order 4):

- **US1** (P1): the RecordDialog fills and saves the note, with its help text from `RecordingText.NoteHelp`;
- **US2** (P1): the History card's plain-text note and its single note control, the NoteDialog's title, and the goal
  page flows (SC-004);
- **US3** (P2): the week and day help of the RecordDialog on period goals.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: the task can run in parallel with the other [P] tasks of its group (they touch different files), once
  the tasks it names as dependencies are done.
- **[Story]**: the user story from spec.md (US1 to US3).
- Paths are relative to the repository root.

## Conventions for every task

- Build with `dotnet build HabitTracker.slnx`. `TreatWarningsAsErrors` is on, so a warning is a failure.
- Use the names in [data-model.md](data-model.md) and [contracts/](contracts/): `NoteWrite`, `NoteWriteKind`,
  `NoteWrite.Keep`, `NoteWrite.Remove`, `NoteWrite.Replace(NoteText)`, `RecordingText.NoteHelp(GoalHistory)`,
  `HistoryCard.FocusNoteAsync`. Every port method stays `async` and takes a `CancellationToken` last.
- The design artifacts stay the contract (constitution, Governance: "never by silent divergence"). If the design step
  (T003 to T006) changes a text or an order that this feature's documents give (a help text, an accessible name, a
  title, the order of the row controls), the same task changes it in [contracts/host.md](contracts/host.md),
  [data-model.md](data-model.md) ("Application: `RecordingText`"), [research.md](research.md) R-07 to R-09 and
  [quickstart.md](quickstart.md) §3, and in this feature's `spec.md` (HP-002 to HP-004, US3, the Independent Tests),
  with a dated line citing the design change. The spec gives these texts as proposals that `design/` confirms, and
  says that a wording `design/` changes is changed in this spec only, not in slice 5's (spec "Slice 5 requirements
  and tests this feature changes", Assumptions).
- Take message texts from `RecordTexts` and the help texts from `RecordingText`. Never retype one in a component.
  Tests assert the literal texts of spec.md and contracts/host.md, so a typo in a constant fails a test.
- No business rule in a component (constitution IV; `CLAUDE.md`). What an input does to the note is
  `RecordingService`'s; which help text a goal gets is `RecordingText`'s. The page only sends the field's content.
- Notes are private (constitution III, spec FR-006):
  - they are written only through `RecordingService` and the owner-scoped `IRecordStore` members; the owner and the
    goal stay the first two parameters of every port member;
  - administrators get no note of any account;
  - no note goes into a log template, an exception message or an error page (HP-007). `NoteWrite` does not override
    `ToString`.
- Test instants are UTC literals worked out by hand, as slice 5's tasks give them (slice 5 tasks.md, Conventions).
  Reuse `RecordWorld` and its helpers in `tests/HT.UnitTests/Application/Records/`.
- No `InternalsVisibleTo` in `HT.Domain`, `HT.Application` or `HT.Persistence` (ADR 0010; `AcSurfaceTests` enforces
  it). Tests use public members only, and inspect rows with plain SQL (`tests/HT.IntegrationTests/Sql.cs`) or through
  the public ports.
- Databases: integration tests use `HT-IT` (`DatabaseFixture`); host tests use `HT-UI` and its derived databases; no
  test touches `HT` or `HT-AC`. Tests in shared databases use their own prefixes and check by inclusion, never by
  totals.
- No use case, result type or text of slices 1 to 5 changes (spec FR-007). The only public changes allowed are the
  four of research R-11: the two `IRecordStore` members `AddAsync` and `ChangeAsync`, and the additions `NoteWrite`,
  `NoteWriteKind` and `RecordingText.NoteHelp`. If a task seems to need another, stop and ask the owner.
- A search for callers or tests of a type excludes the AC project:
  `grep -rnw "<Type>" tests --include=*.cs --exclude-dir=HT.AcceptanceTests`. The `--exclude-dir` is required:
  without it the search enters the AC project's off-limits folders (`CLAUDE.md`). Never point Roslynator or `jb` at
  `tests/HT.AcceptanceTests`.
- Components keep their header comments current: each changed component's comment describes the new state, as
  slice 5's do.
- Commit only at a checkpoint, with the full suite green (constitution V). T001's documents-only commit is a
  checkpoint too: it runs the full suite first. The subject follows `CLAUDE.md` ("Commits"). The pre-commit architecture gate stays on. Never use `--no-verify`.

---

## Phase 1: Setup (shared infrastructure)

**Purpose**: the planning work committed, the roadmap state, and a baseline of the public API.

- [X] T001 Commit the work made before implementation, as one `docs` commit with no code: `docs/adr/0019-note-editing.md`; `specs/006-note-editing/` (spec, checklist, plan and its artifacts, critic reports, this file); `specs/005-recording-and-evaluation/spec.md` and `acceptance.md`; F005's `ac/sources.lock`, `ac/features.lock` and `ac/runs/translate-002/`; `tests/HT.AcceptanceTests/Features/F005/recording-and-evaluation.feature`. These are committed as they are, never edited. Then set row 5a's Status from `tasks ready` to `implementing`, and `updated_at` to today, in `docs/roadmap.md`, and include it in that commit. Before committing, run the full `dotnet test HabitTracker.slnx` and report `PASS` with the counts; no code has changed, so it passes as on `master`, and the commit meets the checkpoint rule (Conventions)
- [X] T002 [P] Save the public API and analyzer baselines before any code change (plan "Constraints", research R-11):
  - run `dotnet restore HabitTracker.slnx`;
  - write `roslynator list-symbols HabitTracker.slnx --projects HT.Application --visibility public --depth member`, and the same for `HT.Domain` and `HT.Persistence`, into three files in the session scratchpad, never in the repo. T035 compares them;
  - write `roslynator analyze HabitTracker.slnx --projects HT.Domain HT.Application HT.Persistence` and `jb inspectcode HabitTracker.slnx --stdout -f=Text -e=WARNING --caches-home=<scratchpad>/jbcache --no-updates` into two more scratchpad files. T036 compares its runs with these, not with `CLAUDE.md`'s older whole-solution baselines (after slice 4).

  The scratchpad does not survive a reboot or a later session. If T035 or T036 finds the files missing, it regenerates them from the branch's merge base with `master` (`git merge-base HEAD master`) in a temporary `git worktree` under the scratchpad (restore, then the same commands), and removes that worktree afterwards.

**Checkpoint**: `dotnet build HabitTracker.slnx` passes.

---

## Phase 2: Foundational (blocking prerequisites)

**Purpose**: the design, slice 5's documents, and the port change with every test that must change with it.

**⚠️ CRITICAL**: no story work starts until this phase is complete.

### Design first (constitution IV, ADR 0008; spec "Design changes before build"; plan Work order 1)

Each block is slice 5's state block, amended in place with a dated line `2026-11-20, specs/006-note-editing,
ADR 0019`. No new slice block is added, so `design/README.md`'s "latest slice block" stays slice 5's. `design/css`
does not change, so nothing is copied to `src/HT.Ui/wwwroot/css`. Tokens and `ht-` classes only, with no new token,
class or color (HP-006).

- [X] T003 [P] Amend `design/pages/PageGoalDetail.html` and `design/pages/PageGoalDetail.md` (HP-001, HP-002; contracts/host.md "History card"; research R-08):
  - the Note column: the note as plain text in `ht-muted`, or "—"; no button;
  - the actions column per row kind, as the table in contracts/host.md gives it: "Add record" first, then "Add note" (no note) or "Edit note" (a note) on rows without a record; "Edit" only on a commitment with a record; "Add record" only on a period with 1 to 49 records; nothing on a period with 50;
  - every control a ghost `ht-btn--sm` button with `aria-haspopup="dialog"`, and the accessible names "Add note for {date}" and "Edit note of {date}";
  - the preview rows show at least: a Missed row with a note ("Edit note"), a Missed or Pending row without one ("Add note"), and a row with a record and a note (text only).
- [X] T004 [P] Amend `design/components/RecordDialog.html` and `.md`, and `design/components/RecordForms.html` and `.md` (HP-003; contracts/host.md "RecordDialog"; research R-05 to R-07):
  - "Note (optional)" opens with the current note of the entry "For" names, or empty without one, in every mode; `RecordDialog.md`'s "always empty on open (also for 'Edit')" goes;
  - the one help text per goal kind: "Clear the field to remove the note." (commitment), "This note is for the whole day. Clear the field to remove it." (per-day period), "This note is for the whole week. Clear the field to remove it." (per-week period); "An empty field keeps the existing note." goes;
  - a change of "For" replaces the field's content with the chosen entry's note; another "Date" of the same week leaves it;
  - at least one preview shows a filled note field with the week help.
- [X] T005 [P] Amend `design/components/NoteDialog.html` and `.md` (HP-004; contracts/host.md "NoteDialog"; research R-09):
  - opened only from a History row without a record, by "Add note" (title "Add note · {goal name}", empty) or "Edit note" (title "Edit note · {goal name}", filled);
  - the HTML's slice 5 comment "or from the row's note (filled)" changes with it;
  - the second preview ("from the note of a Failed week", sub line "27 Jul – 2 Aug · Failed") and the `.md`'s sub-line example become a Missed week opened with "Edit note" (for example "27 Jul – 2 Aug · Missed").
- [X] T006 Amend `design/pages.md` (after T003 to T005): the slice 5 lines of the goal page's History rows, of the RecordDialog ("'Note (optional)', empty, with 'An empty field keeps the existing note.'") and of the NoteDialog ("opened from a History row's 'Add note' or its note"). Then run `dotnet test HabitTracker.slnx --nologo -v q --filter "FullyQualifiedName~DesignSyncTests"`

### Slice 5's documents, amended in place (spec "Slice 5 requirements and tests this feature changes"; plan Work order 2)

Each change carries a dated line citing `specs/006-note-editing` and ADR 0019. They take this feature's contracts
and data model, with any text T003 to T006 changed.

- [X] T007 [P] Amend `specs/005-recording-and-evaluation/contracts/host.md` (after T006): the History card's Note column, row actions and accessible names (with the "Add note" and note rows of its names table replaced), the RecordDialog's note field and help, and the NoteDialog's openers, title and focus ("the row's Note button …, which now shows the note or 'Add note'" becomes the row's "Add note" or "Edit note"), as [contracts/host.md](contracts/host.md) gives them
- [X] T008 [P] Amend `specs/005-recording-and-evaluation/contracts/application-api.md`: the "Record" rule for the note (no note keeps, empty removes, text replaces; refusals write nothing) and "Delete" (the note stays, also after the only record), as [contracts/application-api.md](contracts/application-api.md) gives them
- [X] T009 [P] Amend `specs/005-recording-and-evaluation/data-model.md`: the `IRecordStore` rows of `AddAsync` and `ChangeAsync` ("a note with text is upserted" becomes the three `NoteWrite` cases), a `NoteWrite` entry, the meaning of `RecordInput.Note`, and `RecordingText.NoteHelp`, as [data-model.md](data-model.md) gives them
- [X] T010 [P] Amend `specs/005-recording-and-evaluation/quickstart.md` steps 4 and 5, as this feature's [quickstart.md](quickstart.md) §3 steps 4 and 5 give them (after "Add record" on the Missed R − 10 row the field shows "Away for the weekend."; after "Delete record" the row is Missed again with its note and "Edit note"; a Met row has no "Add note", its note changes with "Edit")

### Application: the note write of the record store port (`src/HT.Application/Records/`; research R-01, R-02)

- [X] T011 Create `src/HT.Application/Records/NoteWrite.cs` with `NoteWriteKind` (`Keep`, `Remove`, `Replace`) and the sealed class `NoteWrite`, with a private constructor like `RecordStoreWrite`: `Kind`, `Text` (`NoteText?`, set only for `Replace`), the static properties `Keep` and `Remove`, and `Replace(NoteText text)`, which throws `ArgumentNullException` for null. No `ToString` override. XML comments state each case (data-model.md "Application: `NoteWrite`")
- [X] T012 Change `IRecordStore.AddAsync` and `ChangeAsync` in `src/HT.Application/Records/IRecordStore.cs` to take `NoteWrite note` in place of `NoteText? note`, at the same position (after T011). Their XML comments state the three cases in the record's transaction, and that `GoalNotFound`, `LimitReached` and `RecordNotFound` write nothing, the note included (FR-003). `SaveNoteAsync`, `DeleteAsync` and the reads do not change; `DeleteAsync`'s comment says that it never changes the note (FR-004)
- [X] T013 Map `RecordInput.Note` to a `NoteWrite` in `src/HT.Application/Records/RecordingService.cs`, at the note step of `RecordAsync` and `ChangeRecordAsync`, keeping slice 5 FR-005's check order (after T012; research R-02's table): null gives `Keep` and `NoteText.TryCreate` is not called; `""` or only spaces give `Remove`; text gives `Replace` with the trimmed note; more than 500 characters gives `NoteTooLong` and nothing is written. Change the XML comment of `Note` in `src/HT.Application/Records/RecordInput.cs`: null keeps the entry's note, a text empty after trimming removes it, any other text, trimmed, becomes the note

### Persistence and the fake (research R-03, R-04)

- [X] T014 Add the private helper `ApplyNoteAsync(Guid goal, DateOnly key, NoteWrite note, CancellationToken)` to `src/HT.Persistence/Records/RecordStore.cs` (after T012): `Keep` does nothing, `Remove` runs the `ExecuteDeleteAsync` on `GoalNotes` that `SaveNoteAsync` uses today, `Replace` calls `UpsertNoteAsync`. `AddAsync` and `ChangeAsync` call it inside `InGoalTransactionAsync` after the record row is written; their `LimitReached` and `RecordNotFound` paths return before it. `SaveNoteAsync` calls it too (`Remove` for null, `Replace` otherwise), so every note write shares one path. No schema change and no migration
- [X] T015 [P] Change `AddAsync` and `ChangeAsync` of `tests/HT.UnitTests/Fakes/FakeRecordStore.cs` to take `NoteWrite` and apply it as T014 does (`Remove` removes the entry's key, `Replace` sets it), after their own `LimitReached` and `RecordNotFound` checks (after T012). The fake is linked into `HT.UiTests` for the bUnit page services, so nothing else changes there
- [X] T016 Change every other caller of the two port members to `NoteWrite` (after T012; research R-10 "Mechanical changes"): `null` becomes `NoteWrite.Keep`, a note becomes `NoteWrite.Replace(...)`. Find them with `grep -rnE "AddAsync|ChangeAsync" tests --include=*.cs --exclude-dir=HT.AcceptanceTests`:
  - `tests/HT.IntegrationTests/Records/RecordReadTests.cs`, `RecordRaceTests.cs`, `RecordPrivacyTests.cs`, `RecordNoteStoreTests.cs`, `RecordStoreTests.cs` and `RecordLogTests.cs`;
  - the fault-injecting `IRecordStore` wrapper in `RecordLogTests.cs` (its two signatures);
  - `tests/HT.UiTests/Host/GoalPrivacyHostTests.cs` (line 114);
  - `tests/HT.UiTests/Host/GoalsHostTests.cs` (lines 213 and 214, whose `null` would be CS8625 under warnings as errors).

  Build the solution: it must compile with no warning before T017.

### Tests that change with the port and the mapping (research R-10)

- [X] T017 Split the three slice 5 unit tests of the old rule (after T013, T015, T016), each into "no note keeps the note" (null) and "an empty note or one of only spaces removes it" (`""`, `"   "`), with names that cite the amended slice 5 ids:
  - `NoteTests.US4_AS3_a_Yes_with_no_note_or_a_note_of_only_spaces_keeps_the_weeks_note` in `tests/HT.UnitTests/Application/Records/NoteTests.cs` (the week keeps "Knee was sore." with no note, and has no note with `""` or `"   "`; US4-AS3, AC-037 rows 01 to 03);
  - `RecordTests.FR_007_a_record_without_a_note_or_with_only_spaces_leaves_the_existing_note` in `RecordTests.cs` (line 487);
  - `CorrectionTests.FR_007_a_change_without_a_note_or_with_only_spaces_leaves_the_entrys_note` in `CorrectionTests.cs` (line 486; for a change, the record is changed in each case; US1-AS3, AC-037 row 07).
- [X] T018 Cover the three cases in `tests/HT.IntegrationTests/Records/RecordStoreTests.cs` (after T014, T016):
  - `A_note_with_a_record_is_upserted_with_text_and_left_unchanged_without` and `A_change_with_a_note_with_text_replaces_the_entrys_note_and_one_without_or_with_only_spaces_leaves_it` become `Keep`, `Remove` and `Replace`, for an add and for a change;
  - new: an add with `Remove` that meets `LimitReached`, and a change with `Remove` that meets `RecordNotFound`, leave the note and write no record (FR-003);
  - new: `Remove` on an entry without a note writes the record and nothing else;
  - `A_delete_removes_only_that_record_and_leaves_the_note` stays as the FR-004 store test.
- [X] T019 [P] Add to `RecordPrivacyTests.AssertEveryMemberGivesNothingAsync` in `tests/HT.IntegrationTests/Records/RecordPrivacyTests.cs`: `AddAsync` and `ChangeAsync` with `NoteWrite.Remove` from another owner give `GoalNotFound` and leave A's note, next to the existing `SaveNoteAsync(..., null, ...)` case (line 129; constitution III) (after T014, T016)
- [X] T020 [P] Extend `tests/HT.IntegrationTests/Records/RecordLogTests.cs` for the new delete path (after T016; HP-007, FR-003, ADR 0019 "a refused or failed save"):
  - `HP_006_no_store_member_logs_a_note_or_value_when_it_writes_reads_refuses_or_is_cancelled`: add a `ChangeAsync` with `NoteWrite.Remove` among its owner writes;
  - `HP_006_no_fault_of_a_use_case_or_of_the_store_puts_a_note_or_value_in_a_log_or_an_exception` (`DatabaseFaultsAsync`, line 445): add a `ChangeAsync` with `NoteWrite.Remove` to its writes, so the log and exception check covers it. In that block both triggers are on and the `GoalRecords` trigger fires on the record's `UPDATE` first, so it does not reach the note's delete;
  - new, for the rollback: a separate block with only `NotesTrigger` created (no `RecordsTrigger`), on an entry with a record and a note. A `ChangeAsync` with a new value and `NoteWrite.Remove`, and a use-case change with `""`, each write the record row, then fault on the note's delete. Assert that each throws "Saving a record failed (SQL error 50000).", that no note or value reaches a log or the exception, and that the record rows and the note rows equal those read before the calls, so the record is rolled back with the failed note delete. Drop the trigger in a `finally`, as `DatabaseFaultsAsync` does.
- [X] T021 [P] Add `NoteWrite` and `NoteWriteKind` to the slice 5 list of `tests/HT.ArchitectureTests/PublicSurfaceTests.cs`, and `NoteWrite` next to `IRecordStore` in the `HT.Ui` fence of `tests/HT.ArchitectureTests/UiDependencyTests.cs` (line 80) (after T011; research R-11). `UseCaseParameterTests` stays as it is and must pass

**Checkpoint**: `dotnet build HabitTracker.slnx` with no warning, then the full `dotnet test HabitTracker.slnx`, `PASS`
with the counts. A bUnit test of slice 5's old page rule that fails here belongs to US1 or US2 (research R-10): move
that task's test edit forward into this commit, never loosen an assertion. Commit the Application, Persistence, fake,
test and architecture changes together, with the design and slice 5 document changes (plan Work order 3).

---

## Phase 3: User Story 1 - See and edit the note where the record is edited (Priority: P1) 🎯 MVP

**Goal**: "Edit", "Add record" and the page head's "Record" open the RecordDialog with the entry's current note in
its field, and "Save record" stores exactly what the field shows. The defect of issue #37.

**Independent Test**: record 12 minutes with "Fell asleep early.", open "Edit", see the note in the field, clear it,
save, and read the row with no note (spec US1).

### Tests for User Story 1

- [X] T022 [P] [US1] Add to `tests/HT.UnitTests/Application/Records/CorrectionTests.cs` (after Phase 2):
  - a change with "Slept badly." on a commitment with "Fell asleep early." gives "Slept badly." (US1-AS2), if no existing test covers it;
  - a change with the note the entry already has leaves it unchanged (US1-AS4; AC-037 row 08);
  - a change of the record of a commitment without a note, with "Back was tired.", adds that note (US1-AS5; slice 5 US4-AS10, AC-072);
  - a change refused for its value, and one refused for a note of 501 characters, leave the record and the note as they were (US1-AS6, FR-003);
  - a change of a final (Met) commitment's record with its unchanged value and only a new note reports a correction, keeps the state Met and replaces the note (spec edge cases).
- [X] T023 [P] [US1] Add `NoteHelp` tests to `tests/HT.UnitTests/Application/Records/RecordingTextTests.cs` (after T024): the three literal texts for a commitment-based goal, a per-day period and a per-week period (HP-003)

### Implementation for User Story 1

- [X] T024 [US1] Add `RecordingText.NoteHelp(GoalHistory history)` to `src/HT.Application/Records/RecordingText.cs` (research R-07): "Clear the field to remove the note." for a commitment-based goal, "This note is for the whole day. Clear the field to remove it." for a per-day period, "This note is for the whole week. Clear the field to remove it." for a per-week period, with the texts as T004 confirmed them. It reads the schedule kind and period from `GoalHistory`, as `ReduceHelp` does
- [X] T025 [US1] Change `src/HT.Ui/Components/Records/RecordDialog.razor` (after T024; research R-05, R-06; contracts/host.md "RecordDialog"):
  - `Reset` sets `_note = entry.Note ?? string.Empty` for the entry the dialog records for, in every mode: the first entry of what can be recorded from the page head, `Row` from a row's "Add record", the record's commitment or period in "Edit";
  - choosing another entry in "For" sets `_note` to that entry's note, replacing typed text; choosing another "Date" of the same week leaves it;
  - the `TextArea`'s `Help` is `RecordingText.NoteHelp(Recordable)` (the dialog's `GoalHistory` parameter), in place of the literal "An empty field keeps the existing note."; a note refusal still replaces it through `Error`;
  - "Save record" sends `Note = _note ?? string.Empty`, never null; "Delete record" is unchanged and ignores the field;
  - the header comment (line 20) describes the filled field and its help.
- [X] T026 [US1] Update `tests/HT.UiTests/Components/Records/RecordDialogTests.cs` (after T025; research R-10):
  - remove the constant `NoteHelp` (line 27) and assert the help through the literal texts;
  - rename `The_note_is_optional_empty_and_says_that_an_empty_field_keeps_the_existing_note` (line 216) to state the new rule (for example `HP_003_the_note_is_optional_and_says_that_clearing_the_field_removes_the_note`), and assert "Clear the field to remove the note.";
  - the field opens with the entry's note in each of the three modes, and empty for an entry without one (US1-AS1, US2-AS5; SC-001);
  - from the page head, choosing another entry in "For" after typing replaces the text with that entry's note, or empties it; another "Date" of the same week keeps it (US3-AS4; spec edge cases);
  - a commitment-based goal shows "Clear the field to remove the note.", tied to the field with `aria-describedby`; a note of 501 characters shows "The note can have at most 500 characters." in its place, and neither the record nor the note changes (HP-003, HP-006);
  - through `FakeRecordStore`: saving a cleared field removes the note, saving an unchanged field keeps it, and "Delete record" leaves the note whatever the field shows (SC-003, FR-004).

**Checkpoint**: the full suite, `PASS` with the counts. Commit.

---

## Phase 4: User Story 2 - Notes on rows without a record (Priority: P1)

**Goal**: the History card shows each note as plain text. A row without a record offers exactly one note control,
"Add note" or "Edit note", which opens the NoteDialog with the matching title. A row with a record offers none, and
its note moves into the RecordDialog.

**Independent Test**: let a commitment become Missed, add "Away for the weekend." with "Add note", read it still
Missed with "Edit note"; then "Add record" and see the note in the field (spec US2).

### Tests for User Story 2

- [X] T027 [P] [US2] Add to `tests/HT.UnitTests/Application/Records/NoteTests.cs` (after Phase 2):
  - the first record of a Missed commitment with "Away for the weekend.": with that same note it has the record and the note; with `""`, and with `"   "`, it has the record and no note; with no note (null) it has the record and the note (US2-AS3; slice 5 US4-AS9, AC-071);
  - deleting the only record of a commitment with a note leaves no record and the note, both before and after its grace period ends (US2-AS4; slice 5 US4-AS8, AC-070).

### Implementation for User Story 2

- [X] T028 [US2] Change `src/HT.Ui/Components/Records/HistoryCard.razor` (research R-08; contracts/host.md "History card"):
  - the Note column: the note as text, or "—", in a plain `td` with `ht-muted`; the Note button and its `_noteButtons` entry for that column go;
  - the actions column: "Add record", then "Add note" (no note) or "Edit note" (a note) on a commitment or period without a record; "Edit" only on a commitment with a record; "Add record" only on a period with 1 to 49 records; nothing on a period with 50. A period's records stay buttons in the Recorded cell;
  - the record control ("Add record" or "Edit") is always the first button of `td.ht-actions`, rendered at the same place in the markup whether or not a note control follows it, so Blazor keeps its element (and its element reference) when the row gets its first record and the note control disappears. `FocusAsync` keeps working as in slice 5;
  - "Add note" and "Edit note": ghost `ht-btn--sm`, `aria-haspopup="dialog"`, accessible names "Add note for {date}" and "Edit note of {date}" with `{date}` from `RecordingText.EntryDate`; they raise `OnNote` with the row;
  - `FocusNoteAsync(DateOnly row)` gives the focus to the row's "Add note" or "Edit note" as it shows after the read, and does nothing when the row has neither;
  - the header comment describes the new columns.
- [X] T029 [US2] Update `tests/HT.UiTests/Components/Records/HistoryCardTests.cs` (after T028; research R-10):
  - line 172: the Note cell is "—" as text, not "Add note";
  - lines 173 to 178: a row without a record has two controls ("Add record", then the note control), every other row at most one;
  - the helper `Action` (line 250), now `Assert.Single` over `td.ht-actions > button`: it returns the row's record control, the first button of `td.ht-actions` ("Add record" or "Edit"), and asserts that any second button is the row's note control. A new helper `NoteControl(cut, date)` returns that note control, or asserts there is none. This keeps the four tests that use `Action` asserting what they asserted before: `US3_AS8_a_commitment_row_offers_Add_record_without_a_record_and_Edit_with_one_whatever_its_state` (lines 284 to 297), `US3_AS8_a_period_row_offers_Add_record_and_each_record_as_a_button_that_opens_Edit_final_rows_included` (lines 300 to 317), `The_row_controls_raise_the_row_and_for_Edit_the_record` (line 340) and `FocusAsync_gives_the_focus_to_the_record_button_while_the_row_shows_it_and_otherwise_to_the_rows_action` (line 361);
  - line 296's `Assert.Equal(12, cut.FindAll("td.ht-actions > button").Count)`: the count becomes 12 plus the number of rows without a record in that fixture, counted by hand from the fixture and written as a literal with a comment naming those rows;
  - the theory `US3_AS8_a_period_row_offers_Add_record_only_while_it_has_fewer_than_50_records` (lines 319 to 337): its `Assert.Single` over the actions of "17 – 23 Aug", a week without a record, asserts "Add record" through `Action`, and "Add note" through `NoteControl`;
  - the slice 5 note tests are rewritten and renamed for the new rule, with the helper `NoteButton` (line 378) replaced by `NoteControl`:
    - `US4_AS7_every_row_offers_Add_note_without_a_note_and_the_note_as_a_button_with_one_whatever_its_state` (line 382, lines 387 to 392) becomes a test of HP-001 and HP-002: the Note cell holds the note as text or "—" with no button, and only rows without a record offer "Add note" or "Edit note" (for example `HP_002_only_a_row_without_a_record_offers_Add_note_or_Edit_note_and_the_Note_cell_is_text`);
    - `US4_AS7_a_weeks_Note_cell_names_its_date_range` (line 396, lines 404 and 405): the week's Note cell reads "Knee was sore." as text, and "17 – 23 Aug"'s note control is named "Add note for 17 – 23 Aug";
    - `The_Note_cells_buttons_raise_their_row` (line 409): renamed (for example `The_note_controls_raise_their_row`); `OnNote` is raised by the "Add note" and "Edit note" of rows without a record;
    - `FocusNoteAsync_gives_the_focus_to_the_rows_Note_button_and_nothing_for_a_row_not_shown` (line 424): renamed (for example `FocusNoteAsync_gives_the_focus_to_the_rows_note_control_and_nothing_for_a_row_without_one`); the focus goes to the row's "Add note" or "Edit note", and nothing for a row with a record or not shown;
  - new theory (SC-002, US1-AS7, US2-AS2): over a commitment and a period, with no record, one record and 50 records (period), with and without a note, and every state the card shows (Due, Pending, Missed, Met, Failed), it counts the controls that change the note on their own: none with a record, exactly one without, named as above; the Note cell shows the note or "—" and has no control.
- [X] T030 [P] [US2] Change `src/HT.Ui/Components/Records/NoteDialog.razor` (research R-09; contracts/host.md "NoteDialog"): when it opens, capture whether `Row.Note` is set; the title is "Edit note · " plus the goal name when it is and "Add note · " plus the goal name when it is not, not recomputed while open. `TitlePrefix` becomes two constants. Sub line, help, "Save note", the 500-character refusal and focus stay. The header comment says it opens only from a row without a record
- [X] T031 [US2] Update `tests/HT.UiTests/Components/Records/NoteDialogTests.cs` (after T030): line 156, a row with a note, expects "Edit note · {goal name}"; lines 124 and 140, rows without a note, keep "Add note · "; new: a save that removes the note keeps the title "Edit note · " until the dialog closes (HP-004)
- [X] T032 [P] [US2] Change the header comment of `src/HT.Ui/Components/Records/GoalDetailBody.razor`: "A History row's 'Add note' or note opens the NoteDialog" becomes "'Add note' or 'Edit note' of a row without a record opens the NoteDialog" (plan Work order 4). No code change
- [X] T033 [US2] Update `tests/HT.UiTests/Components/Pages/GoalDetailPageTests.cs` (after T025, T028, T030, T032; research R-10):
  - `US3_a_Missed_rows_Add_record_and_then_Edit_open_the_RecordDialog_for_that_row_and_give_the_focus_back_to_it` (lines 219 to 265) keeps its assertions: the focus returns to the same element across "Add record", "Edit" and "Add record" again, which T028's first-button rule keeps. Its `td.ht-actions > button` selector takes the first button, and its comment "The row keeps its one action button" becomes "The row's record control stays its first button". If it fails, T028's markup is wrong, not the test;
  - lines 286 to 317: "Add note" in the actions column of the Missed row; after "Save note", the row shows the note as text and "Edit note", and the focus is on "Edit note"; the "…, edit note of {date}" assertion goes;
  - new: "Add record" on a Missed row with a note opens the RecordDialog with the note in its field (US2-AS5);
  - new: deleting the only record of a row with a note shows the row with its note and "Edit note" again, with the focus on its "Add record" through `FocusAsync` (US2-AS4, plan "Risks");
  - new (SC-004): on a row with a record and a note, "Edit", clear the field, "Save record" removes the note; on a row without a record and with a note, "Edit note", change the text, "Save note" replaces it. Each test performs exactly those three interactions before it checks the row.

**Checkpoint**: the full suite, `PASS` with the counts. Commit.

---

## Phase 5: User Story 3 - A period's one note in the record dialog (Priority: P2)

**Goal**: on a per-week or per-day goal, every way into the RecordDialog shows the period's one note with the help
that it is for the whole week or day.

**Independent Test**: a per-week goal with "Knee was sore." on its week; "Add record" for that week shows the note and
"This note is for the whole week. Clear the field to remove it." (spec US3).

### Tests for User Story 3

- [X] T034 [US3] Add period cases to `tests/HT.UiTests/Components/Records/RecordDialogTests.cs` (after T026; HP-003, US3-AS1, US3-AS2):
  - a per-week goal whose week has "Knee was sore." and records: "Add record" for the week, and "Edit" of any of its records (opened as the record buttons in the Recorded cell open it), show "Knee was sore." with "This note is for the whole week. Clear the field to remove it.";
  - a per-day goal ("Sugar, at most 1 sweet a day") whose day has "Birthday cake at work.": "Add record" and "Edit" of its record show that note with "This note is for the whole day. Clear the field to remove it.";
  - saving one record of the week with "Better today." through `FakeRecordStore` makes it the week's note, and a cleared field leaves the week with no note (US3-AS3);
  - a per-week period with 50 records and the note "Knee was sore.": "Edit" of one of its records shows that note with the week help, and saving it with "Better today." makes that the week's note (spec edge cases).

  US3-AS3's use-case rule is covered by `NoteTests.US4_AS3_a_Yes_with_the_note_Better_today_replaces_the_weeks_note` (unchanged) and T017's split. That the 50-record row offers no "Add record" and no note control is covered by T029.

**Checkpoint**: the full suite, `PASS` with the counts. Commit.

---

## Phase 6: Polish and cross-cutting concerns

**Purpose**: the API and analyzer comparisons, the documents against what shipped, the accessibility sweep and the
manual quickstart.

- [X] T035 Compare the public API of `HT.Domain`, `HT.Application` and `HT.Persistence` with T002's baseline. The only differences allowed are the four of research R-11: `IRecordStore.AddAsync` and `ChangeAsync` taking `NoteWrite`, and the new `NoteWrite`, `NoteWriteKind` and `RecordingText.NoteHelp`. Report any other difference as a defect (FR-007)
- [X] T036 [P] Run `roslynator analyze HabitTracker.slnx --projects HT.Domain HT.Application HT.Persistence`, and `jb inspectcode HabitTracker.slnx --stdout -f=Text -e=WARNING --caches-home=<scratchpad>/jbcache --no-updates`. Compare both with T002's analyzer baselines (the same commands before any code change), and fix real findings only, that is, diagnostics this feature added; the documented false positives stay. Never point either tool at `tests/HT.AcceptanceTests`
- [X] T037 [P] Check that the texts that shipped (the help texts, accessible names, titles and the order of the row controls) match `design/`, this feature's spec.md (HP-002 to HP-004), contracts/host.md, data-model.md, research.md (R-07 to R-09) and quickstart.md, and slice 5's amended contracts/host.md, data-model.md and quickstart.md (T003 to T010). Fix the document that disagrees with the design; fix the code only where it disagrees with the design
- [X] T038 [P] Sweep the History card, the RecordDialog and the NoteDialog for constitution IV and HP-006: tokens and `ht-` classes only, with no literal colors; the note text in `ht-muted`; visible focus; the help tied to the field with `aria-describedby`; the dialogs' focus handling; the accessible names of contracts/host.md; sentence case per `design/brand-book.md`. Then rerun `DesignSyncTests`
- [ ] T039 Ask the owner to run this feature's quickstart §3 in the running app (`dotnet run --project src/HT.Ui --launch-profile https`), which includes slice 5's quickstart steps 4 and 5 (issue #37), and record each step's result here. SC-004's three actions are counted by hand. Until the owner reports, the steps, the real focus ring and the screen-reader reading of the help stay labelled unverified (plan "Risks")
  - 2026-11-20: not run yet. Every step of quickstart §3 (slice 5 steps 4 and 5 among them), SC-004's hand count, the focus ring and the screen-reader reading are unverified until the owner reports.
- [X] T040 Run the full build and `dotnet test HabitTracker.slnx`, and report `PASS` with the counts. Leave row 5a's roadmap Status at `implementing`: `AC tests green` comes after the owner's `/speckit-ac-tests-analyze`, `shipped` when the PR opens. Commit

---

## Dependencies and execution order

### Phase dependencies

- **Setup (Phase 1)**: no dependencies.
- **Foundational (Phase 2)**: after Setup. It blocks every story. Design (T003 to T006) comes before slice 5's
  documents (T007 to T010) and every page task. The port change (T011 to T021) is one compile unit and one commit.
- **User stories (Phases 3 to 5)**: after Phase 2, in this order: US1, US2, US3.
- **Polish (Phase 6)**: after the stories that are to ship.

### Story dependencies

- **US1**: none beyond Phase 2.
- **US2**: after US1. T033 opens the RecordDialog from a row and expects the filled field (T025), and SC-004's "Edit"
  path needs the field to save what it shows.
- **US3**: after US2. It needs only US1's T024 and T025, and touches only `RecordDialogTests.cs`, but each story's
  checkpoint runs the full suite and commits the working tree. In the same tree, US3's checkpoint would meet US2's
  unfinished work, so the stories run one after the other.
- **Shared files** need sequential edits, so their tasks never run in parallel with each other:
  - `RecordDialogTests.cs`: T026, T034;
  - `HistoryCardTests.cs`: T029 only; `GoalDetailPageTests.cs`: T033 only;
  - `RecordStoreTests.cs`: T016, T018;
  - `RecordPrivacyTests.cs`: T016, T019; `RecordLogTests.cs`: T016, T020;
  - `CorrectionTests.cs`: T017, T022; `NoteTests.cs`: T017, T027;
  - `docs/roadmap.md`: T001 only.

### Within each story

- A test task starts once the tasks it names as dependencies are done, usually the code it calls, because a test
  that does not compile cannot fail meaningfully.
- The story's checkpoint runs the full suite before the next story, and is its commit point.

### Parallel opportunities

- Phase 1: T002 with T001.
- Design: T003, T004 and T005 together; then T006.
- Slice 5 documents: T007 to T010 together, after T006.
- Port change: T011, then T012; then T013, T014, T015 and T016 (T015 is [P]; T013, T014 and T016 touch different
  files and can run together too); then T017 and T018; T019, T020 and T021 beside them.
- US1: T022 beside T024 and T025; T023 after T024.
- US2: T027, T030 and T032 beside T028.
- The stories themselves run in order (US1, US2, US3), because each checkpoint commits the shared working tree.

---

## Parallel example: Phase 2 design

```text
Task: "T003 Amend design/pages/PageGoalDetail.html and .md"
Task: "T004 Amend design/components/RecordDialog and RecordForms (.html and .md)"
Task: "T005 Amend design/components/NoteDialog.html and .md"
```

## Parallel example: User Story 2

```text
Task: "T027 [US2] First record and delete-keeps-note tests in tests/HT.UnitTests/Application/Records/NoteTests.cs"
Task: "T028 [US2] Note column and note control in src/HT.Ui/Components/Records/HistoryCard.razor"
Task: "T030 [US2] Title fixed on open in src/HT.Ui/Components/Records/NoteDialog.razor"
Task: "T032 [US2] Header comment of src/HT.Ui/Components/Records/GoalDetailBody.razor"
```

---

## Implementation strategy

### MVP first (US1)

1. Phase 1 and Phase 2: design first, slice 5's documents, then the port change with its tests in one commit.
2. US1: the RecordDialog fills its note field and saves it as shown. This alone fixes the defect issue #37 reports.
3. **Stop and validate**: run the full suite, then check in the browser that "Edit" on Reading's R − 6 shows "Fell
   asleep early." and that clearing it removes the note.

### Incremental delivery

1. US2: the History card's plain-text note and single note control, the NoteDialog's title, and the page flows
   (SC-002, SC-004).
2. US3: the week and day help on period goals.
3. Polish. Then the owner runs `/speckit-ac-tests-write 005` and `/speckit-ac-tests-analyze`, with F001, F002 and F004
   expected to pass unchanged and F005 passing with its revised criteria (SC-005).

One PR for slice 5a. Each checkpoint is a commit on the feature branch.

---

## Notes

- [P] tasks touch different files, and start once the tasks they name as dependencies are done (Format).
- The AC project, its features, the locks and the run records are never edited.
- The five points of plan.md "Decided in the plan" are implemented as stated: "Add record" before the note control
  (T003, T028); the NoteDialog's title fixed on open (T030); the page sends `""`, never null (T025);
  `SaveNoteAsync` keeps accepting entries with records (unchanged, so AC-035, AC-036 and AC-039 hold); "For"
  replaces the note and "Date" keeps it (T025).
- The task order follows the plan's Work order, with two moves:
  - the plan's step 4 (pages) is split over the stories: the RecordDialog in US1, the History card, NoteDialog and
    goal page in US2, the period cases in US3;
  - `RecordingText.NoteHelp`, listed in the plan's step 3, is built in US1 (T024) with its only caller, the
    RecordDialog (T025). It is an addition that no port caller needs, so the step 3 compile unit and its commit
    stay whole without it.
- These details were settled while reviewing the tasks ([critic-tasks-pass-1.md](critic-tasks-pass-1.md)):
  - the History card's record control stays the first button of its row, so the existing `Action` helper, its four
    tests and the goal page's focus test keep their meaning (MAJOR-1; T028, T029, T033);
  - the planning work is committed on its own before any code (T001; MINOR-8);
  - a failed `Remove` rolls back the record with it, tested through the `GoalNotes` fault trigger alone, so the record's write succeeds before the note's delete faults (T020; MINOR-6, pass 2 MINOR-1);
  - a wording that `design/` changes is changed in this feature's spec too, as the spec says, and in every document that repeats it (Conventions, T037; pass 1 MINOR-2, pass 2 MINOR-2);
  - T001's documents-only commit runs the full suite first, and T036 compares with T002's own analyzer baselines (pass 2 MINOR-3, MINOR-4);
  - the History card's slice 5 note tests are renamed for the new rule (T029; pass 2 MINOR-5, MINOR-6). Reports: [critic-tasks-pass-2.md](critic-tasks-pass-2.md).
- Amended 2026-11-20 (owner's decision after implementation): the order of T003 and T028 ("Add record" first, the
  record control the first button of its row) was reversed by the owner after implementation. The note control now
  comes before "Add record", so the record control is always the last button of its row, the rightmost. The finished
  tasks above keep their original wording.
- Stop at any checkpoint to validate a story on its own.
