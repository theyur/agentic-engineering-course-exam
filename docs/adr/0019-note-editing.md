# 0019 — Note editing: the record dialog carries the note of a row with a record

**Status:** Accepted, 2026-11-20. Owner choices for `specs/006-note-editing` (issue #37, option 3 with 3a; spec
D-1 and D-2). Changes slice 5's recorded decision on notes given with a record (slice 5 spec, Clarifications,
Session 2026-11-18 (critic revision), last answer) and slice 5 FR-007. Keeps slice 5's other note decisions (D-8,
D-9: one note per commitment or evaluation period, at most 500 characters, written at any time) as they are.

## Context

- Slice 5 gives a commitment or evaluation period one note (FR-007, D-9). It can be written in two places with
  opposite rules for an empty field: the NoteDialog, opened from the History row's note button, removes the note
  when saved empty; the RecordDialog's "Note (optional)" field always opens empty, also in "Edit", and an empty
  field keeps the note.
- The slice 5 rule ("a note given with a record adds or replaces the note only when it has text; a note is
  removed only by saving it empty on its own") protects a week's one note from being cleared by one of its up to
  50 records. Its cost, found in slice 5's manual quickstart (T097): "Edit" shows an empty note field for a day
  that has a note, so the field does not show the note it would replace.
- Slice 6 shows notes on reports and the Home page and should build on the final model.
- The recording use cases (`RecordAsync`, `ChangeRecordAsync`) take the note in `RecordInput`; the record store
  port (`IRecordStore.AddAsync`, `ChangeAsync`) takes `NoteText? note`, where null leaves the note unchanged and a
  value replaces it. `NoteText.TryCreate` gives no note for null, empty and only spaces alike.

## Options

1. **Show the current note read-only above the record dialog's field** (issue #37, option 1). A page change only;
   the two places and their opposite empty-field rules stay.
2. **Remove the note field from "Edit"** (option 2). A page change only; "Record" and "Add record" keep the
   empty-keeps field.
3. **The record dialog owns the note of a row with a record** (option 3): its field opens with the current note,
   and what is saved there is the note, an empty field removing it. For rows without a record:
   - 3a: they keep the NoteDialog, so a Missed row can get a note without a recorded value;
   - 3b: the record dialog saves a note with no value ("note only"), which gives the dialog two meanings.

For a record saved with no note at all, as distinct from an empty note (D-1):

- A. the note is left unchanged; only an empty note removes it;
- B. none and empty are the same: the note is removed.

For where the changed rule and its acceptance criteria live (D-2):

- in slice 5, amended in place (its FR-007 and F005's acceptance criteria are revised);
- in `specs/006-note-editing`, with its own `acceptance.md`, slice 5's rule marked superseded and F005's
  conflicting rows retired.

## Decision

- **Option 3 with 3a.** A History row with at least one record changes its note only together with a record, in
  the RecordDialog, which opens in every mode with the current note; what is saved is the note, and a note empty
  after trimming removes it. A row without a record changes its note on its own in the NoteDialog ("Add note",
  "Edit note"). The Note column shows the note as plain text. Deleting a record never changes the note. The
  owner's "one place per row" means: a row has at most one control that changes its note without changing its
  records (none once it has a record), and every dialog that saves a note shows the current note first; the
  RecordDialog also carries the note when it adds a row's first record.
- **D-1: option A.** A record saved with no note at all leaves the note unchanged. The page always sends the
  field's content, so this applies to other callers of the recording use cases. It keeps a caller that records
  without thinking about notes from clearing a period's note, and it keeps more of F005's locked criteria valid
  (AC-037 rows 01 and 05, AC-064 row 08).
- **D-2: amend slice 5 in place.** The note rule stays in one document, slice 5's FR-007; F005's acceptance
  criteria are revised through the AC flow (unlock, `/speckit-ac-translate`, lock, `/speckit-ac-tests-write`,
  `/speckit-ac-tests-analyze`). `specs/006-note-editing` holds the page requirements and has no `acceptance.md`.

Rejected: options 1 and 2, which keep two places with opposite rules; 3b, which brings back a dialog with two
meanings; D-1 option B, which changes more locked criteria and lets any caller clear a note it did not show; a
separate `acceptance.md` for 006, which splits the note rule over two specs and runs the AC flow twice.

## Consequences

- The record store port needs three note cases for a record's write: keep, remove, replace. The removal happens
  in the same transaction as the record, so a refused or failed save changes neither the records nor the note.
  `IRecordStore`, `RecordStore` and the unit-test fake change; the schema does not.
- Slice 5's spec (FR-007, US4, HP-001 to HP-003, its design list), contracts, data model and quickstart, and the
  design previews of the goal page, RecordDialog, RecordForms and NoteDialog are amended in place, each with a
  dated line citing `specs/006-note-editing` and this ADR.
- F005's AC-037 changes (rows 02 and 03 now give no note; rows for a change with an empty note and with the
  unchanged note are added), and F005 gets criteria for deleting a record of an entry with a note, for the first
  record of an entry that has a note, and for changing a record of an entry without a note with a note. AC-011,
  AC-035, AC-036, AC-038, AC-039 and AC-064 stay.
- The roadmap row 5a goes through the AC states with F005's revised criteria.
- Two open dialogs or tabs can still overwrite each other's note: notes are single-user data and no conflict
  check is added.
