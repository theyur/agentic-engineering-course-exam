# Implementation Plan: Note editing

**Branch**: `claude/issue-review-aa6233` | **Date**: 2026-11-20 | **Spec**: [spec.md](spec.md)

**Input**:

- the feature specification `specs/006-note-editing/spec.md`, revised after
  [critic-spec-pass-1.md](critic-spec-pass-1.md) and [critic-spec-pass-2.md](critic-spec-pass-2.md);
- [ADR 0019](../../docs/adr/0019-note-editing.md) (option 3 with 3a, D-1, D-2);
- slice 5 as amended on 2026-11-20: `specs/005-recording-and-evaluation/spec.md` (FR-007, US4-AS3 and AS7 to AS10),
  and F005's revised criteria, both locked: `acceptance.md` (AC-037, AC-070 to AC-072) and
  `tests/HT.AcceptanceTests/Features/F005/recording-and-evaluation.feature`.

This feature has no `acceptance.md` of its own (D-2). F005's criteria are the acceptance tests of what it builds.

## Summary

A History row with a record changes its note only together with a record. The RecordDialog opens with the
commitment's or period's current note, and saves the field as it is: an empty field removes the note. A row without
a record keeps a single "Add note" or "Edit note" control, which opens the NoteDialog. The Note column becomes plain
text.

The work splits by layer:

- **`HT.Domain`**: no change.
- **`HT.Application`**:
  - a port type `NoteWrite` (`Keep`, `Remove`, `Replace`) replaces `NoteText?` in `IRecordStore.AddAsync` and
    `ChangeAsync` (research R-01);
  - `RecordingService` maps `RecordInput.Note` to it: null keeps, empty removes, text replaces (R-02);
  - `RecordingText.NoteHelp` gives the field's help (R-07).

  No use case signature changes.
- **`HT.Persistence`**: `RecordStore` applies the three cases in the record's own transaction (R-03). No schema
  change and no migration.
- **`HT.Ui`**:
  - `HistoryCard` shows the note as text and moves the note control into the actions column, on rows without a
    record only (R-08);
  - `RecordDialog` fills its note field and sends it as it is (R-05, R-06);
  - `NoteDialog` gets the title "Edit note · …" (R-09).

  `design/` changes first (constitution IV, ADR 0008).

Deleting a record already leaves the note (FR-004). That gets tests, not code.

## Technical Context

**Language/Version**: C# on .NET 10 (`net10.0`), with nullable reference types and implicit usings on, and warnings as
errors. Unchanged.

**Primary Dependencies**: unchanged: ASP.NET Core 10 (Blazor Interactive Server for the goal page's body), EF Core 10
with SQL Server, and NodaTime in `HT.Domain` only. No new package.

**Storage**: SQL Server, slice 5's `GoalRecords` and `GoalNotes`, unchanged. No migration.

**Testing**: xUnit v2, ArchUnitNET and reflection rules, bUnit with `FakeRecordStore`, integration tests against SQL
Server (ADR 0006). The acceptance tests go through the AC workflow for F005 (ADR 0003). Details: research R-10, R-11.

**Target Platform**: unchanged. A browser-based web app over HTTPS: Windows developer machines, CI on Ubuntu, and the
hosted demo on Azure App Service.

**Project Type**: a web application (Blazor host) over the layered .NET solution.

**Performance Goals**: unchanged. A record's write adds at most one note statement to its existing transaction (a
delete or an upsert under the same goal lock). The dialogs read nothing new: the note is already on each
`HistoryEntry`.

**Constraints**:

- The use cases, results and texts of slices 1 to 5 keep their signatures (FR-007). F001, F002 and F004 pass
  unchanged.
- The exported AC surface changes in four places (research R-11). Two port members change: `IRecordStore.AddAsync`
  and `ChangeAsync`. Two types are added, `NoteWrite` and `NoteWriteKind`, and one member, `RecordingText.NoteHelp`.
  Only the two changed members can break an existing caller.
- Notes stay owner-only, through the owner-scoped port, and never reach a log, an exception or an error page (FR-006,
  HP-007).
- No business rule in a component. What an input does to the note is `RecordingService`'s; the help text is
  `RecordingText`'s.
- `design/` comes first. Tokens only, with no new token, class or color (HP-006).

**Scale/Scope**:

- 2 new public port types (`NoteWrite` and its `NoteWriteKind`) and 2 changed port members.
- 1 new `RecordingText` member.
- 3 changed components (HistoryCard, RecordDialog, NoteDialog) and 1 comment in GoalDetailBody.
- 4 design blocks and `design/pages.md`.
- 4 slice 5 documents amended in place.
- No new page, use case, table, startup task or smoke check.

## Constitution Check

*GATE: checked before Phase 0 and again after Phase 1. Pass, with no exception.*

No `[NEEDS CLARIFICATION]` marker is open. The feature touches no SPEC §11 or ARCHITECTURE §9 item. D-1 and D-2 are
decided in the spec's `## Clarifications` and ADR 0019. The plan decides only details inside them (see "Decided in
the plan").

| Principle | Status | How this plan meets it |
|---|---|---|
| I. Ubiquitous Domain Language | met | The texts say note, commitment, period, week and day. `NoteWrite` and `NoteWriteKind` are port plumbing, not domain terms, so `CONTEXT.md` does not change. Its Note entry defines the note, not where the page edits it (spec Key Entities). The buttons start with a verb: "Add note", "Edit note", "Add record", "Edit" (brand book). |
| II. Deterministic, Reproducible Evaluation | met | No evaluation rule changes. A note never changes a state (FR-005). Deleting a record still evaluates the entry by the same function. A correction stays a correction, also when only the note changes (spec edge cases). No clock read is added: the dialogs use the instant in `GoalHistory.ReadAt`. |
| III. Privacy Enforced Below the UI | met | Every note write still goes through an owner-scoped use case and port member, and the owner and the goal stay their first parameters (`UseCaseParameterTests`). The page's "at most one note control" is a page rule, not a protection. The Application keeps refusing another user, an administrator and the anonymous visitor (FR-006, slice 5's negative tests unchanged). `NoteWrite` does not override `ToString`, and `RecordLogTests` gains a `Remove` case (HP-007). |
| IV. Design-System Fidelity and Accessible UI | met | `design/` changes first (Work order step 1). The note text uses `ht-muted` (`--ink-muted`, 5.5:1 or more), the controls use the existing ghost button, and no token or class is added. The help comes from `RecordingText`, the shared formatter, and is tied to the field by the shared `TextArea`. The dialogs keep their focus handling: the first field on open, Esc and × close, and focus returns to the row's control as it shows after the save (HP-004, HP-006). |
| V. Verified, Not Asserted | met | Build and the full suite run before any report of done. The Application and Persistence changes are one compile unit, verified and committed together (Work order step 3). The old rule's tests change in the same commits, not after them (R-10). `NoteWrite` joins the existing architecture rules (R-11). The manual re-check of slice 5's quickstart steps 4 and 5 is labelled unverified until it is run. |
| VI. Demonstrable Vertical Slices | met | Rule, port, store, pages and tests ship together. The demo data already shows each case (HP-008): a note on a row without a record (Reading R − 10, "Edit note"), on a commitment with a record (Reading R − 6, in "Edit") and on a week (Yoga week 2, the week help). So it needs no new demo record. The smoke check does not change. |
| Decision Discipline | met | The reversed slice 5 decision is recorded in ADR 0019, cited and not decided again. The details in "Decided in the plan" stay inside the spec's words and change no locked criterion. |

**Re-check after Phase 1**: pass. The contracts and the data model add no type to Domain, no table, no use case and
no token. They also change no locked acceptance criterion.

## Project Structure

### Documentation (this feature)

```text
specs/006-note-editing/
├── spec.md              # /speckit-specify, /speckit-clarify, critic revisions
├── checklists/requirements.md
├── critic-spec-pass-1.md, critic-spec-pass-2.md
├── plan.md              # this file
├── research.md          # Phase 0
├── data-model.md        # Phase 1 (changes to slice 5's model only)
├── quickstart.md        # Phase 1
├── contracts/
│   ├── application-api.md   # changes to slice 5's Application contract
│   └── host.md              # changes to slice 5's host contract
└── tasks.md             # /speckit-tasks (not created here)
```

### Source Code (repository root)

```text
src/
├── HT.Application/Records/
│   ├── NoteWrite.cs                    # new: NoteWrite, NoteWriteKind
│   ├── IRecordStore.cs                 # AddAsync, ChangeAsync take NoteWrite
│   ├── RecordingService.cs             # RecordInput.Note -> NoteWrite
│   ├── RecordInput.cs                  # XML comment: null keeps, empty removes, text replaces
│   └── RecordingText.cs                # + NoteHelp
├── HT.Persistence/Records/RecordStore.cs   # ApplyNoteAsync in the goal transaction; SaveNoteAsync uses it
└── HT.Ui/Components/Records/
    ├── HistoryCard.razor               # Note as text; Add note / Edit note in the actions column
    ├── RecordDialog.razor              # field filled per mode and per "For"; NoteHelp; sends the field as it is
    ├── NoteDialog.razor                # "Add note · " or "Edit note · ", fixed on open
    └── GoalDetailBody.razor            # comment only

tests/
├── HT.UnitTests/
│   ├── Fakes/FakeRecordStore.cs        # NoteWrite
│   └── Application/Records/            # NoteTests, RecordTests, CorrectionTests (as needed), RecordingTextTests
├── HT.IntegrationTests/Records/        # RecordStoreTests; mechanical: RecordReadTests, RecordRaceTests,
│                                       #   RecordPrivacyTests, RecordNoteStoreTests, RecordLogTests (+ wrapper)
├── HT.UiTests/
│   ├── Components/Records/             # HistoryCardTests, RecordDialogTests, NoteDialogTests
│   ├── Components/Pages/GoalDetailPageTests.cs
│   └── Host/GoalPrivacyHostTests.cs    # mechanical
├── HT.ArchitectureTests/               # PublicSurfaceTests, UiDependencyTests: + NoteWrite
└── HT.AcceptanceTests/                 # outside the solution; F005 step definitions by the AC workflow

design/                                 # Work order step 1
specs/005-recording-and-evaluation/     # contracts/host.md, contracts/application-api.md, data-model.md,
                                        #   quickstart.md amended in place (Work order step 2)
docs/roadmap.md                         # row 5a status
```

**Structure Decision**: the four production projects stay, and no project, folder or namespace is added. `NoteWrite`
sits with the port that takes it, in `HT.Application.Records`, public for the AC reference export and fenced from
`HT.Ui` like `IRecordStore` (ADR 0010).

## Work order

`/speckit-tasks` should follow this order, because later steps need earlier ones:

1. **Design first** (constitution IV, ADR 0008; spec "Design changes before build"). The slice 5 state blocks are
   amended in place, each with a dated line citing `specs/006-note-editing` and ADR 0019:
   - `design/pages/PageGoalDetail.html` and `.md`:
     - the Note column as `ht-muted` text or "—";
     - the actions per row kind (host contract table): "Add record" first, then "Add note" or "Edit note", on rows
       without a record only;
     - the accessible names "Add note for {date}" and "Edit note of {date}";
   - `design/components/RecordDialog.html` and `.md`, `RecordForms.html` and `.md`:
     - the note field filled, with its help per goal kind (the three texts of research R-07);
     - `RecordDialog.md`'s "always empty on open (also for 'Edit')" goes;
   - `design/components/NoteDialog.html` and `.md`:
     - opened only from a row without a record;
     - the "Add note" and "Edit note" titles;
     - the second preview and the `.md`'s sub-line example become a Missed week opened with "Edit note", in place of
       "27 Jul – 2 Aug · Failed";
   - `design/pages.md`: the slice 5 lines of the goal page's History rows, the RecordDialog and the NoteDialog.

   `design/css` does not change.
2. **Slice 5's documents**, amended in place with a dated line citing this feature and ADR 0019. They take this
   feature's contracts and data model:
   - `specs/005-recording-and-evaluation/contracts/host.md`: the History card's Note column, actions and accessible
     names, the RecordDialog's note field, and the NoteDialog's openers, title and focus;
   - `contracts/application-api.md`: the "Record" note rule and "Delete";
   - `data-model.md`: the `IRecordStore` rows of `AddAsync` and `ChangeAsync`, `NoteWrite`, `RecordInput.Note` and
     `RecordingText.NoteHelp`;
   - `quickstart.md`: steps 4 and 5, as section 3 of this feature's quickstart gives them.
3. **Application and Persistence, verified and committed together**. After the port change, `RecordStore`, the fake
   and every test that calls the port must change too, or the solution does not compile.
   - Application:
     - `NoteWrite`;
     - `IRecordStore` (its XML comments: the three cases, and that refusals write no note);
     - `RecordingService`'s mapping;
     - `RecordInput`'s comment;
     - `RecordingText.NoteHelp`.
   - Persistence: `RecordStore.ApplyNoteAsync`, used by `AddAsync`, `ChangeAsync` and `SaveNoteAsync`.
   - Tests: `FakeRecordStore`, and research R-10's unit and integration tests, both the changed and the new ones.
     Also the mechanical port-call changes, among them `RecordLogTests`' wrapper and `GoalPrivacyHostTests` line
     114. The architecture rules of R-11 go in the same commit.
4. **Pages**:
   - `HistoryCard` (R-08);
   - `RecordDialog` (R-05 to R-07);
   - `NoteDialog` (R-09);
   - `GoalDetailBody`'s header comment ("A History row's 'Add note' or note opens the NoteDialog" becomes "'Add note'
     or 'Edit note' of a row without a record").

   Each comes with its bUnit tests (R-10): HistoryCardTests, RecordDialogTests, NoteDialogTests and
   GoalDetailPageTests lines 286 to 317. The components' header comments describe the new state, as slice 5's do.
5. **Docs**: the roadmap row 5a follows the status values of `docs/roadmap.md`:
   - `planned` after this plan (set on 2026-11-20);
   - `tasks ready` after `/speckit-tasks`;
   - `implementing` while `/speckit-implement` runs;
   - `AC tests green` once `/speckit-ac-tests-analyze` passes for every feature;
   - `shipped` when the PR opens.

   `CONTEXT.md`, the README and the constitution do not change.
6. **AC tests**: the owner runs `/speckit-ac-tests-write 005`, then `/speckit-ac-tests-analyze` for every feature,
   after `/speckit-implement`.
7. **Manual check**: this feature's quickstart section 3, which includes slice 5's quickstart steps 4 and 5 (issue
   #37). Then the roadmap's "Opening the PR" step.

## Decided in the plan

These are details the spec's words leave to the plan. None changes a locked acceptance criterion, spans features or
rejects a real alternative the owner weighed, so none needs an ADR or the owner's answer. `/speckit-analyze` can
check them.

1. **The order of the row controls**: "Add record" comes before "Add note" or "Edit note". The spec says "next to 'Add
   record'". `design/` confirms the order in step 1 (research R-08).
2. **The NoteDialog's title is fixed when it opens** (R-09). A save that removes the note does not retitle the
   dialog as it closes.
3. **The page sends `""` for an empty field, never null** (R-06), so only callers other than the page keep a note by
   sending none (D-1).
4. **`SaveNoteAsync` keeps accepting entries with records** (R-08, FR-002). "At most one note control" is the page's
   rule (HP-002). The Application's rule for saving a note on its own does not change, and AC-035, AC-036 and AC-039
   stay locked as they are.
5. **"For" in the page-head dialog replaces typed text with the chosen entry's note. Another "Date" of the same week
   keeps it** (R-05), because the week's note is the same for every date of the week.

## Gates before the next step

None is open. The spec has no `[NEEDS CLARIFICATION]` marker. F005's `sources` and `features` are locked with the
revised criteria. Roadmap row 5a went from `features locked` to `planned` with this plan. `ac_lock.py verify sources --feature 005` passed on 2026-11-20,
before phase 2 of `/speckit-ac-translate`. The next step is `/speckit-tasks`.

## Spec discrepancies

None found. The spec's "Code and tests" list matches the code that was read: the port takes `NoteText?`, the fake is
`FakeRecordStore`, and `GoalDetailPageTests` opens the NoteDialog from the note button. Its list does not name these
callers of the port: `RecordLogTests`' wrapper, `GoalPrivacyHostTests` and the integration tests other than
`RecordStoreTests`. Research R-10 adds them. They are mechanical changes, not new requirements.

## Risks

- **The AC project and the changed port**: F005's existing step definitions were written against slice 5's
  `IRecordStore`. If any AC step definition or support code calls the port directly, the AC project stops compiling
  until `/speckit-ac-tests-write 005` adapts it. A broken compile affects the runs of all four features, not only
  F005. This cannot be checked here, because the step definitions and support code are not read (CLAUDE.md). The use
  case signatures, which acceptance tests are meant to use (ADR 0003), do not change. `/speckit-ac-tests-analyze`
  runs all four features and shows any break.
- **F005 before `/speckit-ac-tests-write`**: until the owner runs that command, the revised F005 criteria may have no
  matching or passing step definitions:
  - AC-037 as a whole: its title is new, and its `Then` line now ends in "with <note>", which changes the step text of
    all eight rows. Rows 02, 03 and 07 now expect no note. Row 08 is new;
  - AC-070, AC-071 and AC-072, which are new.

  Implementation reports the solution's tests only. Roadmap row 5a reaches `AC tests green` only after
  `/speckit-ac-tests-analyze`.
- **The last record of a period with a note**: deleting it brings back "Edit note" with the note (FR-004, HP-002).
  Focus returns through `FocusAsync`, to the row's first action ("Add record"), not to "Edit note". This matches slice
  5's rule that the focus goes to the record's button while the row shows it, otherwise the row's action. A bUnit test
  checks it.
- **Two tabs**: a save in one tab overwrites a note changed in another tab that its field did not show. The spec
  accepts this (edge cases, ADR 0019) and SC-003 excludes it. No conflict check is added.
- **The manual check**: browser behaviour beyond bUnit (the real focus ring and the screen-reader reading of the help)
  is checked only by quickstart section 3, and is labelled unverified until it is run.

## Complexity Tracking

No Constitution Check violation needs justifying.
