# Research: Note editing

**Feature**: specs/006-note-editing · **Plan**: [plan.md](plan.md) · **Spec**: [spec.md](spec.md)

The spec leaves no `[NEEDS CLARIFICATION]` marker open: D-1 and D-2 are decided (spec `## Clarifications`,
[ADR 0019](../../docs/adr/0019-note-editing.md)). This file settles how the code meets the spec. Each entry gives the
decision, why, and the alternatives that were rejected. The current code was read for each one:
`src/HT.Application/Records/RecordingService.cs`, `IRecordStore.cs`, `src/HT.Persistence/Records/RecordStore.cs`,
`tests/HT.UnitTests/Fakes/FakeRecordStore.cs`, and `src/HT.Ui/Components/Records/HistoryCard.razor`,
`RecordDialog.razor`, `NoteDialog.razor` and `GoalDetailBody.razor`.

## R-01: Three note cases on the record store port

**Decision**: a new port type, `NoteWrite`, in `HT.Application.Records`, with three values: `Keep`, `Remove` and
`Replace(NoteText)`. `IRecordStore.AddAsync` and `ChangeAsync` take `NoteWrite note` in place of `NoteText? note`, at
the same position. `SaveNoteAsync`, `DeleteAsync` and the reads do not change.

`NoteWrite` is a sealed class with a private constructor, as `RecordStoreWrite` is:

- `Kind` (`NoteWriteKind`: `Keep`, `Remove`, `Replace`);
- `Text` (`NoteText?`), set only for `Replace`;
- the static properties `Keep` and `Remove`, and the factory `Replace(NoteText text)`, which refuses null.

**Why**: FR-001 with D-1 has three outcomes for a record's write: no note given keeps the note, an empty one removes
it, and one with text replaces it. Today the port has two (`null` keeps, a value replaces), because `NoteText` cannot
be empty (`NoteText.TryCreate` folds null, empty and only spaces into "no note"). FR-003 and ADR 0019 need the removal
in the record's own transaction, so that a refused or failed save changes neither the records nor the note. One
explicit value per case states this in the port's signature. A caller cannot mean "keep" and get "remove" by passing
null.

**Alternatives rejected**:

- **`NoteText? note` plus `bool removeNote`**: two parameters for one decision, with a combination that means nothing
  (a note and `removeNote`).
- **Remove the note with a second call to `SaveNoteAsync` after the record's write**: two transactions. A fault
  between them leaves the record written and the old note in place, which FR-003 and ADR 0019 forbid. The goal row's
  lock would also be taken twice.
- **Let `NoteText` hold an empty text**: it breaks the Domain invariant "trimmed, 1 to 500 characters", which the
  `GoalNotes` table, the demo data and every read rely on (slice 5 data-model).
- **Leave `null` as "keep" and add a sentinel `NoteText` for "remove"**: a hidden special value in a Domain type.

## R-02: The use cases map the input to a `NoteWrite`

**Decision**: `RecordAsync` and `ChangeRecordAsync` keep their signatures and their check order (slice 5 FR-005:
access, record, date, value, note, limit). At the note step, `RecordingService` maps `RecordInput.Note`:

| `RecordInput.Note` | `NoteText.TryCreate` | `NoteWrite` |
|---|---|---|
| null | not called | `Keep` (D-1) |
| `""`, only spaces | true, no note | `Remove` |
| text, at most 500 characters after trimming | true, a note | `Replace(note)` |
| more than 500 characters after trimming | false, `tooLong` | none: `NoteTooLong`, nothing is written |

A `Replace` with the text the entry already has writes the same text (AC-037 row 08, AC-071's first row). A `Remove`
on an entry without a note deletes nothing. `RecordInput.Note`'s XML comment states the three cases.

`DeleteRecordAsync` already never touches the note: `RecordStore.DeleteAsync` deletes the record row only, and the
fake does the same. FR-004 needs no code change, only the tests of R-07.

**Why**: the use case decides what the input means. The store only applies one of three instructions. `SaveNoteAsync`
(FR-002) keeps `NoteText?`, where null deletes, because slice 5's rule for saving a note on its own does not change.

**Alternatives rejected**: a new use case or a new `RecordInput` member (for example `bool KeepNote`). The input
already distinguishes null from empty, and FR-007 of this spec keeps every slice 1 to 5 signature.

## R-03: `RecordStore` applies the note inside the goal's transaction

**Decision**: one private helper, `ApplyNoteAsync(Guid goal, DateOnly key, NoteWrite note, CancellationToken)`, called
inside `InGoalTransactionAsync` after the record row is written:

- `Keep`: nothing;
- `Remove`: `ExecuteDeleteAsync` on `GoalNotes` where `GoalId` and `EntryDate` match (the statement `SaveNoteAsync`
  already uses);
- `Replace`: the existing `UpsertNoteAsync`.

`SaveNoteAsync` calls the same helper (`Remove` for null, `Replace` otherwise), so all note writes share one code
path. The `LimitReached` and `RecordNotFound` paths return before the helper, as they return before the upsert today,
so they write nothing. The schema and the migrations do not change.

**Why**: the transaction already holds the goal row's update lock (slice 5 research R-05), so the note's delete runs
under the same lock, and a fault rolls back the record and the note together.

## R-04: The fake store

**Decision**: `FakeRecordStore.AddAsync` and `ChangeAsync` take `NoteWrite` and apply it as R-03 does (`Remove` removes
the key, `Replace` sets it), after their own `LimitReached` and `RecordNotFound` checks. The other members are
unchanged. The fake is also the store of the bUnit page services (`tests/HT.UiTests/Components/Pages/PageServices.cs`,
linked in `HT.UiTests.csproj`), so the page tests see the new rule with no other change.

The test-only wrapper in `tests/HT.IntegrationTests/Records/RecordLogTests.cs`, which implements `IRecordStore` to
inject faults, changes its two signatures the same way.

## R-05: Where the record dialog gets the current note

**Decision**: from the `HistoryEntry` the dialog already holds. No Application read changes.

- `GetHistoryAsync` and `GetRecordableAsync` both build their entries with `EntryView`, which already carries
  `HistoryEntry.Note`, the entry's one note.
- In each of the three modes the dialog has the entry: `Recordable.Entries[...]` from the page head, `Row` from a
  row's "Add record", and `Row` (the record's commitment or period) in "Edit".
- `RecordDialog.Reset` sets the field to `entry.Note ?? string.Empty` in place of `null`. `ChooseEntry` (a change of
  "For") sets it to the chosen entry's note, replacing what was typed (HP-003, spec edge cases). `ChooseDate` (another
  date of the same week) leaves the field alone, because the week's note does not change with the date.
- After any save, the page reads what can be recorded and the history again before the dialog closes (slice 5
  `GoalDetailBody`), so the next opening has the current note.

**Why**: the note is already on the page. A second read would add a round trip and could disagree with the row shown.

## R-06: The page always sends the field's content

**Decision**: "Save record" builds `RecordInput.Note` from the field as `_note ?? string.Empty`, never null. So an
empty field is `Remove`, and an unchanged field is `Replace` with the same text, which keeps the note. Only callers
other than the page can send null, which keeps the note (D-1). "Delete record" calls `DeleteRecordAsync`, which takes
no note, so the field is ignored (HP-003, FR-004).

**Why**: HP-003 says that what the field shows when saving is what is stored. A field the user emptied must remove the
note even if the TextArea gives back null for an empty value.

## R-07: The help text of the note field

**Decision**: a new member of the shared formatter, `RecordingText.NoteHelp(GoalHistory history)`, with the texts that
`design/` confirms (HP-003):

| Goal | Text |
|---|---|
| commitment-based | "Clear the field to remove the note." |
| period-based, per day | "This note is for the whole day. Clear the field to remove it." |
| period-based, per week | "This note is for the whole week. Clear the field to remove it." |

The RecordDialog passes it as the TextArea's `Help`, in place of the literal "An empty field keeps the existing
note.". The shared `TextArea` already ties the help to the field with `aria-describedby` and shows a refusal in place
of the help (`src/HT.Ui/Components/Shared/TextArea.razor`), so "The note can have at most 500 characters." replaces it
with no new code (HP-003, HP-006).

**Why**: the text depends on the goal's schedule kind and period, as `RecordingText.ReduceHelp` and `PartialNote` do.
Keeping it in `RecordingText` makes it unit-testable and keeps the decision out of the component (constitution IV,
no business rules in Blazor). `GoalHistory` carries `GoalScheduleKind` and `Period` in all three modes.

## R-08: The History row's note control

**Decision**, in `HistoryCard`:

- **Note column**: the note as text, or "—", in a plain `td` with `ht-muted` text. `--ink-muted` (#565d73) is listed in
  `design/tokens.json` as 5.5:1 or more on `bg`, `surface` and `surface-2`, so HP-006's 4.5:1 holds with no new token.
  The Note button and its `_noteButtons` entry for that column go.
- **Actions column**, per row:
  - a commitment without a record: "Add note" or "Edit note", then "Add record";
  - a commitment with a record: "Edit" only;
  - a period without a record: "Add note" or "Edit note", then "Add record";
  - a period with records: "Add record" while it has fewer than 50 records, and no note control.
- **Names**: "Add note for {date}" and "Edit note of {date}", in the actions column's ghost `ht-btn--sm` style with
  `aria-haspopup="dialog"`, as slice 5's controls are.
- **Callback**: `OnNote` keeps its name and type (`EventCallback<HistoryEntry>`). It is raised only from the new
  control. `FocusNoteAsync(DateOnly row)` gives the focus to that control as the row shows it after the save ("Add
  note" or "Edit note", HP-004), and does nothing when the row has no such control.

"Add record" comes first, because recording is the row's main action. The spec says "next to 'Add record'" and names
no order. `design/` confirms the order in the slice 5 state block of `PageGoalDetail` (Work order step 1).

Amended 2026-11-20 (owner's decision after implementation): the note control now comes before "Add record", so the
record control ("Add record" or "Edit") is always the rightmost, for visual consistency across rows. The decision
above, "Add record" first, was reversed by the owner. The order is in the markup (DOM order, visual order and Tab
order are the same), not in CSS. Focus is unchanged: after the RecordDialog, the record's button or the row's record
control; after the NoteDialog, the row's note control.

**Why the control is hidden rather than refused below the UI**: this is a page rule, not a privacy rule. FR-002 keeps
`SaveNoteAsync` as slice 5 specifies it, for any begun date with or without records (AC-035, AC-036 and AC-039 stay
locked). The page offers one way to write a row's note on its own, and the Application keeps accepting the call. This
is what ADR 0019 decides ("A row without a record changes its note on its own in the NoteDialog").

## R-09: The NoteDialog's title

**Decision**: when it opens, `NoteDialog` captures whether `Row.Note` is set. The title is "Edit note · " and the goal
name when it is, and "Add note · " and the goal name when it is not. The flag is not recomputed while the dialog is
open, so a save that removes the note does not retitle the closing dialog. The constant `TitlePrefix` becomes two
constants. The sub line, help, "Save note", the 500-character refusal and the focus handling stay as they are (HP-004).

## R-10: Tests that change, and the new ones

The existing tests that encode slice 5's old rule change in the same commits as the code (spec "Code and tests"):

- **Unit, `tests/HT.UnitTests/Application/Records`**:
  - `NoteTests.US4_AS3_a_Yes_with_no_note_or_a_note_of_only_spaces_keeps_the_weeks_note` splits: no note keeps it,
    and `""` or only spaces removes it;
  - `RecordTests.FR_007_a_record_without_a_note_or_with_only_spaces_leaves_the_existing_note` splits the same way;
  - `CorrectionTests.FR_007_a_change_without_a_note_or_with_only_spaces_leaves_the_entrys_note` (line 486, a theory
    over null, `""` and `"   "`) splits the same way: null keeps the note, `""` and `"   "` remove it.
- **New unit tests**:
  - for a change: an empty note and one of only spaces remove the note, and the same note keeps it (US4-AS3);
  - the first record of a Missed commitment with a note, with the same note, `""` and no note (US4-AS9);
  - a change of a record of an entry without a note, with a note, adds it (US4-AS10);
  - deleting the only record keeps the note, both before and after the grace period ends (US4-AS8);
  - a change refused for its value or for a note of 501 characters leaves the record and the note as they were
    (FR-003);
  - `RecordingText.NoteHelp` for each of the three kinds.
- **Integration, `tests/HT.IntegrationTests/Records/RecordStoreTests.cs`**:
  - `A_note_with_a_record_is_upserted_with_text_and_left_unchanged_without` and
    `A_change_with_a_note_with_text_replaces_the_entrys_note_and_one_without_or_with_only_spaces_leaves_it` cover
    `Keep`, `Remove` and `Replace` for an add and for a change;
  - new: a `Remove` that meets `LimitReached` or `RecordNotFound` leaves the note (FR-003);
  - new: `Remove` on an entry without a note writes nothing else;
  - new, in `RecordPrivacyTests.AssertEveryMemberGivesNothingAsync`: `AddAsync` and `ChangeAsync` with
    `NoteWrite.Remove` from another owner give `GoalNotFound` and leave A's note. This is the new delete path, next to
    the existing `SaveNoteAsync(..., null, ...)` case (line 129), so every way to delete a note has a store-level
    negative test (constitution III);
  - `A_delete_removes_only_that_record_and_leaves_the_note` stays as the FR-004 store test.
- **Mechanical changes** (`null` becomes `NoteWrite.Keep`, a note becomes `NoteWrite.Replace(...)`):
  - the store calls in `RecordReadTests`, `RecordRaceTests`, `RecordPrivacyTests`, `RecordNoteStoreTests` and
    `RecordLogTests`;
  - the wrapper in `RecordLogTests` (R-04);
  - `tests/HT.UiTests/Host/GoalPrivacyHostTests.cs` line 114;
  - `tests/HT.UiTests/Host/GoalsHostTests.cs` lines 213 and 214, which pass `null` to `AddAsync`. With `NoteWrite`
    non-nullable, that `null` is CS8625, which fails the build under warnings as errors. It becomes
    `NoteWrite.Keep`.

  `RecordLogTests` gets a `Remove` case among its log-capture writes, so HP-007 also covers the new path.
- **bUnit, `tests/HT.UiTests/Components`**:
  - `Records/HistoryCardTests`: the Note column is text or "—" with no button, and a theory over the row kinds of
    R-08 (commitment or period, with or without records, with or without a note, every state shown) counts the note
    controls: none with a record, exactly one without (SC-002). Its accessible names and `FocusNoteAsync` are covered
    too. These existing tests change:
    - line 172 (the Note cell is "—", not "Add note");
    - lines 173 to 178, which assert a single control in every actions cell. A row without a record now has two;
    - lines 387 to 392, including line 392's single button in every Note cell. The Note cell now has none;
    - lines 404 and 405;
    - `The_Note_cells_buttons_raise_their_row` (lines 409 to 419), which clicks the note of "Tue 3 Nov", a row with a
      record. That row has no note control now, so the test raises `OnNote` from the "Add note" or "Edit note" of
      rows without a record;
    - `FocusNoteAsync_gives_the_focus_to_the_rows_Note_button_and_nothing_for_a_row_not_shown` (lines 424 to 430);
  - `Records/RecordDialogTests`:
    - the field opens with the note in each mode, and empty without one;
    - a change of "For" replaces typed text with the new entry's note;
    - the three help texts, and the refusal in place of the help;
    - saving a cleared field removes the note, and an unchanged field keeps it (through `FakeRecordStore`, SC-001,
      SC-003);
    - "Delete record" leaves the note whatever the field shows.

    The constant `NoteHelp` (line 27) goes;
  - `Records/NoteDialogTests`: a row with a note gets the title "Edit note · " and the goal name (line 156), and a
    test of a save that removes the note keeps that title until the dialog closes. The rows without a note keep
    "Add note · " (lines 124 and 140);
  - `Pages/GoalDetailPageTests`, lines 286 to 317: "Add note" in the actions column of the Missed row; after a save,
    "Edit note" and the note as text, with the focus on "Edit note". Its "…, edit note of {date}" assertion goes;
  - `Pages/GoalDetailPageTests`, new, for SC-004: on a row with a record and a note, "Edit", clear the field and
    "Save record", three actions, remove the note. On a row without a record, "Edit note", change the text and "Save
    note", three actions, replace it. Each test counts exactly those three interactions before checking the row.

The smoke check does not change (HP-008). `PageReader.ReadCardText` reads the History card's text, and the note
"Fell asleep early." is still in it, now as cell text. `tests/HT.UiTests/SmokeChecks/PageReaderTests.cs` parses
fixed HTML and keeps passing as it is. The demo data and its tests do not change.

## R-11: Architecture rules and the AC reference surface

**Decision**:

- `NoteWrite` joins the slice 5 list in `tests/HT.ArchitectureTests/PublicSurfaceTests.cs` (no NodaTime type in a
  public signature).
- It also joins `IRecordStore` in the `HT.Ui` fence of `UiDependencyTests` (line 80), because it is port plumbing that
  no component needs.
- `UseCaseParameterTests.Every_method_of_the_record_port_takes_the_owner_first_and_the_goal_second_with_no_exception`
  keeps passing unchanged, since the owner and the goal stay first.

**AC reference surface**: `HT.Application` is exported to `artifacts/refs` for the AC project (ADR 0003). The
exported surface changes in four places:

- the two changed members of `IRecordStore`, `AddAsync` and `ChangeAsync`;
- the new public types `NoteWrite` and `NoteWriteKind`;
- the new public member `RecordingText.NoteHelp`.

The two changed port members are the only change that can break existing callers. The rest are additions. Every use
case signature stays the same.
`/speckit-ac-tests-write 005` runs on refs exported after the change. Whether any AC support code calls the port
directly cannot be checked here, because the AC project's `StepDefinitions/` and `Support/` are not read (CLAUDE.md).
`/speckit-ac-tests-analyze` builds and runs all four features, so a break shows up there (plan "Risks").
