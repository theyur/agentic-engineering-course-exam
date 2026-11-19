# NoteDialog

A dialog to add or edit a private note on one commitment or period, for example to explain a Missed or partial result.

- Title names the goal ("Add note · Meditation"); the sub line gives the date and the state ("Tue 27 Oct · Missed").
- Help text: notes are private and do not change the result.
- A note can be added at any time, also after the result is finalized.

## Slice 5 state

The last part of the preview shows the dialog as slice 5 builds it (HP-003, ADR 0008).

- It opens from a History row on the goal page: from "Add note" (the field is empty) or from the row's note (the field holds the note). The Recent notes card does not open it.
- Title: "Add note · " and the goal's name ("Add note · Reading, 20 min daily"), with or without a note. There is no other title.
- Sub line: the row's date, " · " and its state word as the StatusChip writes it: "Sat 14 Nov · Missed", "27 Jul – 2 Aug · Failed" (a week: its date range), "Tue 17 Nov · Due today".
- Body: the TextArea "Note" with the help "Notes are private. They do not change the result.". A note has at most 500 characters; a longer one gives "The note can have at most 500 characters." under the field, and nothing changes.
- Footer: primary "Save note" and secondary "Cancel". Saving it empty (or with only spaces) removes the note.
- A note belongs to the commitment or period, not to one record, and never changes its state.
- Focus moves to the TextArea on open; Esc and × close the dialog, and focus returns to the button that opened it.
