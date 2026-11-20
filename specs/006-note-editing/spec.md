# Feature Specification: Note editing

**Feature Branch**: `claude/issue-review-aa6233`

**Created**: 2026-11-20

**Status**: Draft

**Input**: User description: "note-editing: one place to edit a row's note (GitHub issue #37, option 3 with 3a).
Redesign of note editing on a goal's History, changing slice 5 (specs/005-recording-and-evaluation) FR-007 and its
acceptance criteria. Rules: (1) The History's Note column shows the period's or commitment's note as plain text,
not clickable, or "—" when there is none. (2) On a row that has a record, the note is added, changed and cleared
only through the record dialog (Edit / Add record). The dialog opens with the current note filled in; what is
saved there is the note, and an empty field removes it (reverses slice 5's "an empty field keeps the existing
note" rule, AC-037). (3) For a week, or a day with several records, the field shows the period's one note, with a
hint that the note is for the whole period (e.g. "This note is for the whole week"). (4) A row without a record
(Missed, Pending, or Due before anything is recorded) keeps "Add note" with the note dialog, so a note can be
written without recording a value (US4-AS1 / AC-035, the demo's "Away for the weekend." on a Missed day). Each row
has exactly one place to edit its note. Must be decided in the spec: what happens to the note-only entry point
when a row with a note later gets a record (note moves into the prefilled record dialog); whether 005's spec.md
FR-007/US4-AS3 is amended or marked superseded by this feature; which of 005's AC-011, AC-035, AC-036, AC-037,
AC-039, AC-064 change. Constraints: notes stay owner-only (no admin access), max 500 characters, trimmed. Must land
before slice 6 (personal-reports) shows notes on reports and Home. Out of scope: notes on reports/Home (slice 6),
excusals/pauses (slice 7)."

Origin: [issue #37](https://github.com/theyur/agentic-engineering-course-exam/issues/37), found during slice 5's
manual quickstart (T097) and deferred by the owner on 2026-11-19 (roadmap, slice note 5). Slice 5 gives a
commitment or evaluation period one note (slice 5 FR-007, D-9) but two places to write it, with opposite rules for
an empty field: the note button on a History row removes the note when saved empty, while the record dialog's
"Note (optional)" field always opens empty, even in "Edit", and an empty field keeps the note. So "Edit" shows an
empty field for a day that has a note, and the field does not show the note it would replace.

The slice 5 rule had a reason (slice 5 Clarifications, critic revision): a week has one note but up to 50 records,
so an empty field must never delete a note the user did not see. This feature keeps that protection by a
different means: the field always shows the note, so what the user sees is what is stored.

## Clarifications

### Session 2026-11-20 (specify)

- Q: Which of issue #37's options? → A: Option 3 with 3a (owner, 2026-11-20): a row with a record changes its note
  only together with a record, in the record dialog, which opens with the current note; a row without a record
  keeps the note dialog ([ADR 0019](../../docs/adr/0019-note-editing.md)).
- Q: What happens to a row without a record that has a note, when it gets its first record? → A: The note stays
  and from then on is edited in the record dialog, which opens with it filled in. Saving that first record with
  the field unchanged keeps the note; clearing the field removes it (FR-001, US2-AS3). Given in the owner's feature
  input.
- Q: A record saved with no note at all, as distinct from an empty note (D-1)? → A: The existing note is left
  unchanged; only an empty note removes it (owner, 2026-11-20; FR-001; ADR 0019). The page always sends the
  field's content (HP-003), so this matters only to other callers of the recording use cases.
- Q: Where do the changed note rule and its acceptance criteria live (D-2)? → A: In slice 5, amended in place
  (owner, 2026-11-20; ADR 0019): slice 5's FR-007 and US4 are rewritten with a dated clarification citing this
  feature, and F005's acceptance criteria are revised. This feature keeps the page requirements and has no
  acceptance criteria of its own.

### Session 2026-11-20 (clarify)

- Q: When the last record of a History row is deleted, what happens to that row's note? → A: It stays: deleting a
  record never changes the note, and the row then offers "Edit note" again (FR-004, US2-AS4, HP-002).
- Q: On a row without a record that has a note, what does the user choose to change the note, now that the Note
  column is plain text? → A: An "Edit note" button in the row's actions column, next to "Add record"; "Add note"
  sits there too when the row has no note. The Note column is plain text on every row (HP-001, HP-002).

### Session 2026-11-20 (critic revision)

Revised after critic pass 1 of this spec ([critic-spec-pass-1.md](critic-spec-pass-1.md)):

- "Exactly one place" (owner input, issue #37) is defined as HP-002 states it: a row offers at most one control
  that changes its note *without* changing its records, and none once it has a record; every dialog that saves a
  note shows the current note first. The record dialog also carries the note when it adds a row's first record,
  because that save is when the note moves into it (US2-AS3, US2-AS5). MAJOR-1.
- Reversing slice 5's recorded note decision, D-1 and D-2 are recorded in
  [ADR 0019](../../docs/adr/0019-note-editing.md) (constitution, Decision Discipline). MAJOR-2.
- The NoteDialog opened from "Edit note" is titled "Edit note · {goal name}" (HP-004); the week and day hints are
  part of the note field's one help text, which a refusal replaces (HP-003). MINOR-9.

## Decisions this feature cites

Recorded earlier and not decided again here:

- One note per commitment or evaluation period, not per record, private to the goal's owner, trimmed, at most 500
  characters, written for any date that has begun, final results included, never changing the state (slice 5
  FR-007, D-8, D-9; `CONTEXT.md` Note).
- Use cases take the acting user and return named outcomes with literal texts
  ([ADR 0005](../../docs/adr/0005-use-case-convention.md)); refusals are checked in slice 5 FR-005's order.
- The administrator role grants no access to private data (slice 1 FR-015; slice 5 FR-017, FR-018).
- Partly built pages show only what has shipped ([ADR 0008](../../docs/adr/0008-partly-built-pages.md)).
- Acceptance tests reach the Application use cases only
  ([ADR 0003](../../docs/adr/0003-acceptance-tests.md)); page behaviour is verified by unit and bUnit tests
  ([ADR 0006](../../docs/adr/0006-integration-tests.md)).

## What acceptance tests can reach

As in slice 5: **FR-NNN** are application behaviours on the AC surface (the recording use cases); **HP-NNN** are
page requirements, verified by bUnit and unit tests, not by acceptance tests. Scenarios marked *(page)* are
verified by bUnit tests.

This feature has behaviour on the AC surface: what a note given with a record does (FR-001) and that deleting a
record keeps the note (FR-004). By D-2 it is written into slice 5's FR-007 and US4 and tested by F005's
acceptance criteria. This feature therefore has no `acceptance.md` and no Coverage table of its own; the table
"Where this feature's scenarios are tested" below maps each scenario instead, and F005's Coverage table maps
slice 5's amended ids. The roadmap row 5a goes through the three AC states (`acceptance locked`, `features
locked`, `AC tests green`) with F005's revised criteria, as "Order of work" says.

The FR and US ids of this spec are its own. Slice 5's amended text uses slice 5's numbering ("Slice 5 requirements
and tests this feature changes" gives the mapping), and F005's criteria cite slice 5's ids.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - See and edit the note where the record is edited (Priority: P1)

A user opens "Edit" on a History row that has a record and a note. The record dialog shows that note in its note
field. The user changes it, clears it or leaves it as it is, and saves: the saved note is exactly what the field
showed.

**Why this priority**: it is the defect issue #37 reports; nobody clears or overwrites a note they cannot see.

**Independent Test**: record 12 minutes with the note "Fell asleep early.", open "Edit", see the note in the field,
clear it, save, and read the row with no note.

**Acceptance Scenarios**:

1. *(page)* **Given** a commitment with a record and the note "Fell asleep early.", **When** its owner opens
   "Edit", **Then** the note field contains "Fell asleep early." (HP-003).
2. **Given** a commitment with a record and the note "Fell asleep early.", **When** the owner changes the record
   with the note "Slept badly.", **Then** the note is "Slept badly." (FR-001).
3. **Given** the same commitment, **When** the owner changes the record with an empty note or one of only spaces,
   **Then** the note is removed and the record is changed (FR-001).
4. **Given** the same commitment, **When** the owner changes the record with the same note, **Then** the note is
   unchanged.
5. **Given** a commitment with a record and no note, **When** the owner changes the record with the note "Back was
   tired.", **Then** the commitment has that note (FR-001).
6. **Given** a change refused for its value or its note (slice 5 FR-005), **Then** neither the record nor the note
   changes (FR-003).
7. *(page)* **Given** the History table, **Then** a row with a record shows its note in the Note column as plain
   text, or "—", and offers no note control of its own (HP-001, HP-002).

---

### User Story 2 - Notes on rows without a record (Priority: P1)

A user adds a note to a commitment or period that has no record, for example "Away for the weekend." on a Missed
day, without recording a value. When the row later gets a record, its note moves with it into the record dialog.

**Why this priority**: recording a value only to write a note would change the result (Missed would become Met or
Failed); slice 5's US4-AS1 and the demo's Missed day with a note depend on it.

**Independent Test**: let a commitment become Missed, add "Away for the weekend." through the note dialog, read it
still Missed; then add a record and see the note in the record dialog's field.

**Acceptance Scenarios**:

1. **Given** a Missed commitment without a record, **When** its owner saves the note "Away for the weekend." on its
   own, **Then** the commitment has that note and is still Missed (slice 5 FR-007, unchanged).
2. *(page)* **Given** the History table, **Then** a row without a record offers "Add note" when it has no note and
   "Edit note" when it has one; both open the note dialog, "Edit note" with the note filled in (HP-002, HP-004).
3. **Given** a commitment without a record and with the note "Away for the weekend.", **When** the owner records a
   value for it with that same note, **Then** it has the record and the note "Away for the weekend."; with an
   empty note it has the record and no note; with no note at all it has the record and the note (FR-001, D-1).
4. **Given** a commitment with one record and a note, **When** the owner deletes the record, **Then** the
   commitment has no record and still has its note (FR-004), and its row offers "Edit note" again (HP-002).
5. *(page)* **Given** a row without a record and with a note, **When** the owner chooses "Add record", **Then** the
   record dialog's note field contains that note (HP-003).

---

### User Story 3 - A period's one note in the record dialog (Priority: P2)

A user records one of several "Yes" records of a per-week goal, or one of several records of a per-day goal. The
note field shows the period's one note and says that it is for the whole week or day, so the user knows that
changing it changes the note of the whole period, not of one record.

**Why this priority**: a period has up to 50 records but one note; without the hint, a user editing one record
could think they edit that record's own note.

**Independent Test**: a per-week goal with "Knee was sore." on its week; open "Add record" for that week and see
the note and the help "This note is for the whole week. Clear the field to remove it.".

**Acceptance Scenarios**:

1. *(page)* **Given** a per-week period with the note "Knee was sore.", **When** its owner opens "Add record" or
   "Edit" for any record of that week, **Then** the note field contains "Knee was sore." with the help "This note
   is for the whole week. Clear the field to remove it." (HP-003).
2. *(page)* **Given** a per-day period ("Sugar, at most 1 sweet a day"), **When** its owner opens "Add record" or
   "Edit" for that day, **Then** the help is "This note is for the whole day. Clear the field to remove it."; a
   commitment's help is "Clear the field to remove the note." (HP-003).
3. **Given** a per-week period with the note "Knee was sore.", **When** the owner records "Yes" for another date of
   that week with the note "Better today.", **Then** the week's note is "Better today."; with an empty note the
   week has no note (FR-001).
4. *(page)* **Given** the record dialog opened from the page head, **When** the owner changes the "For" choice,
   **Then** the note field shows the note of the newly chosen commitment or period (HP-003).

---

### Edge Cases

- A row with a record and a note, opened with "Edit" and then "Delete record": the note is not changed, whatever the
  field shows at that moment (FR-004); the row then offers "Edit note".
- A per-week period with 50 records offers no "Add record", but each record's "Edit" still edits the week's note.
- A note that is only spaces in the field is empty after trimming, so saving it removes the note (FR-001).
- A note of 501 characters in the record dialog refuses the whole save: neither the record nor the note changes,
  and the refusal replaces the note field's help (slice 5 FR-005 step 6, unchanged; HP-003).
- Changing only the note in "Edit" of a final commitment saves the record with its unchanged value. The use case
  reports it as a correction, as any change of a final commitment's record is in slice 5 (its FR-006); the page
  does not show that flag, and the state does not change, since a note never changes it.
- Changing "For" in the page-head dialog after typing a note replaces the typed text with the newly chosen entry's
  note (HP-003); the dialog does not carry typed text from one entry to another.
- Two browser tabs: a note changed in one tab while the other has the dialog open is overwritten by the later save,
  which did not show it. Notes are single-user data; no conflict check is added (ADR 0019, Assumptions). SC-003
  excludes this case.

## Requirements *(mandatory)*

### Functional Requirements (application behaviour, on the AC surface)

- **FR-001**: When a record is saved (recorded or changed) with a note, the note given MUST become the note of the
  record's commitment or evaluation period: a note that has text after trimming adds or replaces the note, and one
  that is empty after trimming removes it. When a record is saved with no note at all (none given, as distinct from
  an empty one), the existing note is left unchanged (D-1).
- **FR-002**: Saving a note on its own MUST stay exactly as slice 5 FR-007 specifies: it replaces the note, an empty
  one removes it, for any date that passes slice 5 FR-002, whatever the state, final results included; for a
  per-week period any begun date of the week names the period.
- **FR-003**: A save that is refused (slice 5 FR-005: signed in, role, goal, record, date, value, note, limit, in
  that order) MUST change neither the records nor the note.
- **FR-004**: Deleting a record MUST NOT change the note of its commitment or period, also when it was the only
  record.
- **FR-005**: A note MUST keep slice 5's rules: at most one per commitment or evaluation period, trimmed, at most
  500 characters ("The note can have at most 500 characters."), never changing the state.
- **FR-006**: The recording use cases MUST keep slice 5's privacy rules (slice 5 FR-017, FR-018): only the goal's
  owner records, changes, deletes or saves a note; another registered user gets "This goal does not exist." or
  "This record does not exist.", an administrator "Goals are for registered users.", an anonymous visitor "You are
  not signed in.", and none of them changes or reads a note.
- **FR-007**: The use cases of slices 1 to 5 MUST keep giving their results for every call they accept, except
  where FR-001 and FR-004 change slice 5's note rule. The locked acceptance criteria of F001, F002 and F004 do not
  change; F005's change as listed under "Slice 5 requirements and tests this feature changes".

### Host and page requirements (outside the AC surface)

- **HP-001**: The History card's Note column (slice 5 HP-001, `design/pages/PageGoalDetail`) MUST show each row's
  note as plain text, or "—" when the row has none. It is not a control: the note button of slice 5 is removed.
- **HP-002**: Each History row MUST offer **at most one control that changes its note on its own**, without
  changing its records, and every dialog that saves a note MUST show the row's current note before it is changed:
  - a row with at least one record: no such control; its note changes only together with a record, in the
    RecordDialog ("Edit" of a commitment's record, a period's record button, or "Add record" on a period);
  - a row without a record: exactly one, "Add note" when it has no note or "Edit note" when it has one, in the
    row's actions column next to "Add record"; it opens the NoteDialog (HP-004). The row's "Add record" opens the
    RecordDialog with the note filled in too (HP-003), because saving that first record is when the note moves
    into the record dialog (Clarifications 2026-11-20).

  Accessible names name the date: "Add note for {date}", "Edit note of {date}". When a row's records change, its
  control follows on the next read: it disappears with the first record and comes back when the last record is
  deleted.
- **HP-003**: The RecordDialog's "Note (optional)" field (slice 5 HP-002, `design/components/RecordDialog`,
  `RecordForms`) MUST open, in every mode (page head, a row's "Add record", "Edit"), with the current note of the
  commitment or period the dialog records for, or empty when it has none. Opened from the page head, changing
  "For" replaces the field's content with the note of the newly chosen entry. Then:
  - "Save record" always sends the field's content as the note, so what the field shows when saving is what is
    stored (FR-001); an unchanged field keeps the note;
  - the field has one help text, which replaces slice 5's "An empty field keeps the existing note.": for a
    commitment "Clear the field to remove the note.", for a per-day period "This note is for the whole day. Clear
    the field to remove it.", for a per-week period "This note is for the whole week. Clear the field to remove
    it.". A refusal of the note ("The note can have at most 500 characters.") replaces the help, as slice 5's
    refusals do;
  - "Delete record" ignores the field (FR-004).

  The texts are proposals that `design/` confirms first (constitution IV).
- **HP-004**: The NoteDialog (slice 5 HP-003) MUST open only from a row without a record. From "Add note" it is
  titled "Add note · {goal name}" and starts empty; from "Edit note" it is titled "Edit note · {goal name}" and
  starts with the note. Its sub line, help text, 500-character rule, "Save note" (empty removes the note) and focus
  handling stay as in slice 5: after a save, the focus returns to the row's note control as the row shows it after
  the save, "Add note" or "Edit note", which the save may have swapped (slice 5 `contracts/host.md`, "the row's
  Note button …, which now shows the note or 'Add note'").
- **HP-005**: The Recent notes card (slice 5 HP-001, FR-008) MUST stay as it is: it shows notes and opens no
  dialog.
- **HP-006**: The changed page parts MUST meet constitution IV: the help text is tied to the note field for
  assistive technology, the plain-text note and "—" have at least 4.5:1 contrast, dialogs keep slice 5's focus
  handling, and no new tokens or colors are added.
- **HP-007**: Logs, exception messages and error pages MUST NOT contain notes (slice 5 HP-006, unchanged).
- **HP-008**: The demo data MUST stay as slice 5 FR-030 gives it; it already shows the cases of this feature: a
  note on a row without a record (`Reading`, R − 10, Missed, "Away for the weekend."), on a commitment with a
  record (`Reading`, R − 6, "Fell asleep early.") and on a week with records (`Yoga`, week 2, "Knee was sore.").
  The smoke check (slice 5 HP-008) does not change.

### Where this feature's scenarios are tested

API scenarios are tested by F005's criteria after its revision (new criteria take the next free ids, AC-070 on);
*(page)* scenarios by bUnit tests of the goal page and its dialogs.

| Scenario | Slice 5 id after the amendment | Tested by |
|---|---|---|
| US1-AS1, US1-AS7, US2-AS2, US2-AS5, US3-AS1, US3-AS2, US3-AS4 | HP-001 to HP-003 of slice 5 | bUnit (HP-001 to HP-004 here) |
| US1-AS2 | US4-AS3 | AC-037 row 06 (a change with a different note that has text) |
| US1-AS3 | US4-AS3 | AC-037 (a new row: a change with an empty note) |
| US1-AS4 | US4-AS3 | AC-037 (a new row: a change with the note the entry already has) |
| US1-AS5 | US4-AS10 (new) | a new F005 criterion: a change of a record on an entry without a note, with a note |
| US1-AS6 | FR-005 | AC-038 row 03, AC-064 rows 06 and 11 |
| US2-AS1 | US4-AS1 | AC-035 |
| US2-AS3 | US4-AS9 (new) | a new F005 criterion |
| US2-AS4 | US4-AS8 (new) | a new F005 criterion; no existing criterion deletes a record of an entry with a note and reads the note (AC-026, AC-027, AC-029, AC-032, AC-033, AC-047 to AC-050 checked) |
| US3-AS3 | US4-AS3 | AC-037 rows 02 to 04 |

### Slice 5 requirements and tests this feature changes

Amended in place (D-2), each with a dated line citing `specs/006-note-editing` and ADR 0019. "Order of work" says
when: slice 5's spec before F005's criteria are revised, the rest with this feature's tasks.

- Slice 5 FR-007: its second bullet becomes this feature's FR-001 ("given with a record, a note that has text after
  trimming adds or replaces the note, one that is empty after trimming removes it, and no note leaves it
  unchanged"), and it adds this feature's FR-004 ("deleting a record never changes the note"). In slice 5's text
  these are part of slice 5 FR-007; no slice 5 FR number changes.
- Slice 5 US4: AS3 becomes "with no note, the week's note is still 'Knee was sore.'; with a note of only spaces, or
  an empty one, the week has no note; with 'Better today.' it becomes 'Better today.'"; new AS8 (US2-AS4 here),
  AS9 (US2-AS3 here) and AS10 (US1-AS5 here). AS7 *(page)* follows HP-001 and HP-002 here.
- Slice 5 Clarifications: a dated session citing this feature, issue #37 and ADR 0019; the critic-revision answer
  on notes is marked as amended, not deleted. Its decisions D-1 to D-9 keep their numbers; the new rule is cited
  as "ADR 0019", not as this feature's D-1 or D-2.
- Slice 5 HP-001, last bullet ("every row: 'Add note' without a note, the note itself with one"), HP-002's note
  field ("empty, whose help says that an empty field keeps the existing note") and HP-003 ("opened from the History
  row's note and from 'Add note'"): each is replaced by a reference to this feature's HP-001 and HP-002, HP-003,
  and HP-004, which govern; slice 5's spec does not restate their texts, so a wording that `design/` changes is
  changed here only.
- Slice 5 "Design changes before build": its bullets on the RecordDialog's note help ("An empty field keeps the
  existing note."), the History row actions ("Add note" on every row) and the NoteDialog's "Add note" entry follow
  this feature's "Design changes before build".
- `specs/005-recording-and-evaluation/contracts/host.md`: the History card's Note column, row actions and
  accessible names, the RecordDialog's note field and the NoteDialog's openers and title.
  `contracts/application-api.md`: the "Record" rule for the note. `data-model.md`: the store port rows of
  `AddAsync` and `ChangeAsync` ("a note with text is upserted").
- `specs/005-recording-and-evaluation/quickstart.md`: step 4 (after "Add record" on the Missed R − 10 row, the
  field shows "Away for the weekend."; after "Delete record" the row is Missed again with its note and "Edit
  note") and step 5 ("Add note" on a Met row no longer exists: the note of a Met row is changed with "Edit"). Both
  steps are re-checked by hand after implementation (issue #37).
- Slice 5's acceptance criteria (`specs/005-recording-and-evaluation/acceptance.md`, locked):
  - **AC-037** changes. Rows 02 and 03 (a record with the note `"   "` or `""` keeps "Knee was sore.") become "no
    note". Rows 01 and 05 (a record or change with no note keeps "Knee was sore.") stay (D-1). Rows 04 and 06
    stay. Rows for a change with an empty note and for a change with the note the week already has are added, and
    the title becomes "A note given with a record is the note; no note leaves it unchanged".
  - **New criteria** for slice 5 US4-AS8 (deleting the only record of a commitment with a note keeps the note),
    US4-AS9 (the first record of a commitment that has a note: with the same note, an empty note and no note) and
    US4-AS10 (changing the record of a commitment without a note, with a note, adds it).
  - **AC-064** stays: row 08 (a change with no note keeps "First note.") holds by D-1, and its other rows are
    refusals (FR-003) or check no note.
  - **AC-011**, **AC-035**, **AC-036**, **AC-038** and **AC-039** stay as they are.
- Code and tests (constraints for the plan, not a design):
  - the record store port (`IRecordStore.AddAsync`, `ChangeAsync`) takes the note as `NoteText?`, where null keeps
    the note and a value replaces it, and `NoteText.TryCreate` folds none, empty and only spaces into "no note".
    FR-001 with D-1 needs three cases (keep, remove, replace) in the record's own write, so that a refused or
    failed save changes neither the records nor the note (FR-003). The port, `RecordStore` and the unit-test fake
    `FakeRecordStore` change; the schema does not;
  - the tests that change with FR-001, FR-004 and HP-001 to HP-004: the unit tests of the recording use cases, the
    integration tests of the record store, and the bUnit tests of the goal page (among them
    `tests/HT.UiTests/Components/Pages/GoalDetailPageTests.cs`, which opens the NoteDialog from the note button
    and checks "…, edit note of {date}"), the History card, the RecordDialog and the NoteDialog.

### Design changes before build

Constitution IV and ADR 0008 require `design/` to change first. The slice 5 state blocks are amended in place, each
with a dated line citing `specs/006-note-editing` and ADR 0019 (D-2); no new slice block is added, so `design/README.md`'s "latest slice
block" stays slice 5's:

- `design/pages/PageGoalDetail` (HTML and `.md`): the Note column as plain text or "—"; "Add note" and "Edit note"
  only on rows without a record, in the actions column.
- `design/components/RecordDialog` and `RecordForms` (HTML and `.md`): the note field filled with the current
  note, and its one help text per kind (HP-003).
- `design/components/NoteDialog` (HTML and `.md`): opened only from a row without a record, by "Add note" (title
  "Add note · {goal name}", empty) or "Edit note" (title "Edit note · {goal name}", filled); the HTML's slice-5
  comment "or from the row's note (filled)" changes with it. Its second slice-5 preview ("from the note of a
  Failed week", sub line "27 Jul – 2 Aug · Failed") and the `.md`'s sub-line example "27 Jul – 2 Aug · Failed"
  show a state in which the dialog no longer opens (a Failed entry has a record, slice 5 FR-010); they become a row
  without a record, such as a Missed week, opened with "Edit note".
- `design/pages.md`: the slice 5 lines of the goal page's History rows, the RecordDialog ("'Note (optional)',
  empty, with 'An empty field keeps the existing note.'") and the NoteDialog ("opened from a History row's 'Add
  note' or its note").

### Key Entities

- **Note**: unchanged from slice 5 (one optional private text per commitment or evaluation period). What changes is
  where it is edited: together with a record in the record dialog when its commitment or period has a record, on
  its own in the note dialog when it has none. `CONTEXT.md`'s Note entry stays as it is: it defines the note, not
  where the page edits it.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: On every History row, a user who opens a dialog that saves the row's note sees the current note
  before changing it, in 100% of the bUnit tests over rows with and without records, commitments and periods.
- **SC-002**: Every History row offers at most one control that changes its note without changing its records:
  none on a row with a record, exactly one ("Add note" or "Edit note") on a row without one, in 100% of the row
  states tested (with or without records, with or without a note, every evaluation state shown).
- **SC-003**: A save removes or replaces only the note that its field or dialog showed for the entry it saves, at
  the moment of saving (in the page-head dialog, the note of the entry chosen in "For"): in 100% of the tests,
  every removal comes from a field or dialog that showed that entry's note and was cleared. A note changed
  elsewhere while the dialog is open (two tabs) is outside this criterion (edge cases).
- **SC-004**: Once a row is on the page, a user changes or clears its note in at most 3 actions (open, edit, save);
  a row after the first 10 needs "Show all N →" first.
- **SC-005**: All locked acceptance criteria of F001, F002 and F004 pass unchanged, and F005's pass after the
  revision listed above.

## Order of work

The AC commands are started by the owner only; each step stops for the owner.

1. Amend slice 5's spec as listed above: FR-007, US4, Clarifications, and the references from its HP-001 to
   HP-003 to this feature (this feature's branch). These are the sources of F005's criteria.
2. The owner asks for `ac_lock.py unlock sources` and `ac_lock.py unlock features` of F005, then runs
   `/speckit-ac-translate` for F005: phase 1 revises `acceptance.md` (owner review, then `sources` locked; row 5a
   `acceptance locked`), phase 2 translates it (`features` locked; row 5a `features locked`).
3. `/speckit-plan` and `/speckit-tasks` for this feature. The tasks put the design changes first, then the
   amendments of slice 5's `contracts/host.md`, `contracts/application-api.md`, `data-model.md` and
   `quickstart.md`, once the design and the plan have settled the texts and the store port.
4. `/speckit-analyze`, then `/speckit-implement`.
5. The owner runs `/speckit-ac-tests-write` for F005, then `/speckit-ac-tests-analyze` (every feature; row 5a `AC
   tests green`).
6. The manual re-check of slice 5's quickstart steps 4 and 5, then the roadmap's "Opening the PR" step.

## Decisions due before acceptance criteria

Both decided by the owner on 2026-11-20 (see `## Clarifications`) and recorded in
[ADR 0019](../../docs/adr/0019-note-editing.md):

- **D-1** (FR-001): a record saved with no note at all, as distinct from an empty note. Decided: the note is left
  unchanged; only an empty note removes it. Rejected: treating none like empty, which would change more F005 rows
  (AC-037 rows 01 and 05, AC-064 row 08) and let any caller that records without a note clear a period's note.
- **D-2** (FR-001, FR-004): where the changed rule and its acceptance criteria live. Decided: slice 5, amended in
  place, so the note rule stays in one document. Rejected: a separate `acceptance.md` for this feature with slice
  5's rows retired, which splits the rule over two specs and runs the AC flow twice.

## Assumptions

- Notes are single-user data: no conflict check between two open dialogs or tabs (edge cases).
- The use cases of slice 5 (`RecordAsync`, `ChangeRecordAsync`, `DeleteRecordAsync`, `SaveNoteAsync`, the history
  and recent notes) keep their names and outcomes; their note input can already tell no note from an empty one.
  The record store port changes as "Code and tests" above says. No new use case and no schema change are expected.
- Notes on reports and the Home page (slice 6), Excused and Paused (slice 7) and goal changes (slice 8) are out of
  scope.
- The help texts and the "Edit note" title are proposals; `design/` confirms them before build.
- This feature lands before slice 6 (`personal-reports`), which shows notes on reports and Home and builds on this
  model.
