# NoteDialog

A dialog to add or edit a private note on one commitment or period, for example to explain a Missed or partial result.

- Title names the goal ("Add note · Meditation"); the sub line gives the date and the state ("Tue 27 Oct · Missed").
- Help text: notes are private and do not change the result.
- A note can be added at any time, also after the result is finalized.

## Slice 5 state

The last part of the preview shows the dialog as slice 5 builds it (HP-003, ADR 0008).

- It opens only from a History row without a record on the goal page: from its "Add note" (the row has no note; the field is empty) or its "Edit note" (the row has a note; the field holds it). A row with a record has neither: its note changes in the RecordDialog. The Recent notes card does not open it.
- Title, fixed when the dialog opens: "Add note · " and the goal's name when the row has no note ("Add note · Reading, 20 min daily"), "Edit note · " and the goal's name when it has one ("Edit note · Yoga, 3 times a week"). A save that removes the note does not retitle the closing dialog.
- Sub line: the row's date, " · " and its state word as the StatusChip writes it: "Sat 14 Nov · Missed", "27 Jul – 2 Aug · Missed" (a week: its date range), "Tue 17 Nov · Due today".
- Body: the TextArea "Note" with the help "Notes are private. They do not change the result.". A note has at most 500 characters; a longer one gives "The note can have at most 500 characters." under the field, and nothing changes.
- Footer: primary "Save note" and secondary "Cancel". Saving it empty (or with only spaces) removes the note.
- A note belongs to the commitment or period, not to one record, and never changes its state.
- Focus moves to the TextArea on open; Esc and × close the dialog, and focus returns to the button that opened it. After a save, focus goes to the row's "Add note" or "Edit note" as the row shows it after the save (the save may swap one for the other).
- Amended 2026-11-20, specs/006-note-editing, ADR 0019: it was opened from a History row's "Add note" or from the row's note, on any row, always titled "Add note · "; the preview's second example was the note of a Failed week ("27 Jul – 2 Aug · Failed"), a row with a record that no longer opens this dialog.
