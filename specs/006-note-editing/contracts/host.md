# Contract: Host and pages of note editing

**Feature**: specs/006-note-editing · **Plan**: [../plan.md](../plan.md)

This contract gives only what changes in
[slice 5's host contract](../../005-recording-and-evaluation/contracts/host.md): the goal page's History card, the
RecordDialog and the NoteDialog. Implementation amends that document in place, with a dated line citing this feature
and ADR 0019 (Work order step 2). The texts were proposals that `design/` confirms first (constitution IV, Work order
step 1). Checked 2026-11-20 (T037): `design/` confirms every text and order below unchanged (`pages/PageGoalDetail`,
`components/RecordDialog`, `RecordForms`, `NoteDialog`, `pages.md`).

## Unchanged

- The page head, "Record", the Settings card, the Recent notes card (HP-005), the static shell and its 404, and the
  reads and their order (`GoalDetailBody`).
- After any save or delete, the page reads what can be recorded, the history and the recent notes again before the
  dialog closes.
- The startup tasks, their log lines, the smoke check and the demo data (HP-008).
- No new token, class or color. `design/css` does not change, so nothing is copied to `src/HT.Ui/wwwroot/css`.

## History card (`HistoryCard`; HP-001, HP-002)

The columns stay Date, Recorded, Result, Note and the empty-headed actions column.

- **Note**: the row's note as text, or "—" without one, in a plain `td` with `ht-muted` text (`--ink-muted`, 5.5:1 or
  more, `design/tokens.json`). It is not a control. Slice 5's Note button goes.
- **Actions** (`td.ht-actions`). Every control is a ghost `ht-btn--sm` button with `aria-haspopup="dialog"`, on final
  rows too:

  | Row | Controls, in this order |
  |---|---|
  | commitment, no record | "Add note" (no note) or "Edit note" (a note), then "Add record" |
  | commitment, a record | "Edit" |
  | period, no record | "Add note" or "Edit note", then "Add record" |
  | period, 1 to 49 records | "Add record" |
  | period, 50 records | none |

  A period's records stay buttons in the Recorded cell that open "Edit" of that record. The record control ("Add
  record" or "Edit") is always the last button of the row, the rightmost.

  Amended 2026-11-20 (owner's decision after implementation): the note control now comes before "Add record", so the
  record control is always the rightmost. It was "Add record", then the note control.
- **Accessible names**: slice 5's table, with its two note rows replaced:

  | Control | Accessible name |
  |---|---|
  | "Add note" | "Add note for {date}" |
  | "Edit note" | "Edit note of {date}" |

  `{date}` is `RecordingText.EntryDate`: "Sat 14 Nov", or "10 – 16 Aug" for a week.
- **Events**: "Add note" and "Edit note" raise `OnNote` with the row. The page opens the NoteDialog for it. Every
  other control raises `OnAction` as in slice 5.
- **Focus** (constitution IV, HP-004):
  - after the NoteDialog closes, `FocusNoteAsync(row)` gives the focus to the row's "Add note" or "Edit note" as it
    shows after the save. Nothing gets it if the row has neither;
  - after the RecordDialog closes, `FocusAsync` works as in slice 5: the record's button while the row shows it,
    otherwise the row's record control ("Add record").
- On the next read after a row's first record, its note control is gone. After its last record is deleted, it is back
  ("Edit note" when the row still has its note).

## RecordDialog (`RecordDialog`; HP-003)

"Note (optional)", the TextArea with the placeholder "What happened? Only you can see notes.", now:

- **Opens with the current note** of the commitment or period that "For" names, or empty without one, in every mode:
  - from the page head: the first entry of what can be recorded. Choosing another entry in "For" replaces the field's
    content, typed text included, with that entry's note. Choosing another "Date" of the same week leaves it alone;
  - from a row's "Add record": that row's note, also on a row without a record that has a note (its first record);
  - "Edit" of one record: the note of the record's commitment or period.
- **Help**: `RecordingText.NoteHelp(history)`, one text per goal:

  | Goal | Help |
  |---|---|
  | commitment-based | "Clear the field to remove the note." |
  | per-day period | "This note is for the whole day. Clear the field to remove it." |
  | per-week period | "This note is for the whole week. Clear the field to remove it." |

  It is tied to the field with `aria-describedby`. A refusal of the note ("The note can have at most 500
  characters.") takes the help's place, as slice 5's refusals do (`TextArea`).
- **"Save record"** sends the field's content as `RecordInput.Note`, never null: an empty field removes the note, and
  an unchanged one keeps it. A refusal changes neither the record nor the note, and the dialog stays open with the
  message under its field.
- **"Delete record"** ignores the field: the note stays (FR-004).
- Focus, Esc, × and "Cancel" are unchanged.

## NoteDialog (`NoteDialog`; HP-004)

- **Opened** only from a History row without a record, by "Add note" or "Edit note".
- **Title**, fixed when the dialog opens:
  - "Add note · {goal name}" when the row has no note; the field starts empty;
  - "Edit note · {goal name}" when it has one; the field starts with the note.
- Unchanged:
  - the sub line ("Tue 27 Oct · Missed");
  - the help "Notes are private. They do not change the result.";
  - the 500-character refusal under the field;
  - "Save note" (`SaveNoteAsync` with the row's first date; saving it empty removes the note) and "Cancel".
- **Focus**: the field on open. Esc, × and "Cancel" close the dialog. After a save, focus returns to the row's "Add
  note" or "Edit note", whichever the save left (`FocusNoteAsync`).
