# RecordForms

The yes/no and count variants of RecordDialog, shown without the backdrop.

- Yes / no: "Yes" is pre-selected only when opened from a "Done" button; otherwise nothing is selected.
- Count for Reduce goals: help text explains why 0 must be entered (an unrecorded day cannot prove you stayed under the limit): "At most 2. Enter 0 if you had none: an empty day cannot prove you stayed under the limit.". For a Reduce yes/no goal it asks for "No" instead of 0. An empty day is Missed (FR-013).
- Same footer as every Dialog: Save record, Cancel, optional "Excuse instead".

## Slice 5 state

The last part of the preview shows the variants as slice 5 builds them (HP-002, ADR 0008), each opened from "Record". The rules for the title, the sub line, "For", the progress line and the help texts are in `RecordDialog.md`, "Slice 5 state"; the texts here follow them.

- Build, yes/no, fixed days: "Record · Yoga, 3 days a week", sub line "Build · Mon · Wed · Fri"; "Did you do it?" Yes | No with nothing selected; no progress line and no help.
- Reduce, count, 1 time a day: "Record · Coffee, at most 2 cups a day", sub line "Reduce · at most 2 cups a day"; the field "Amount" (not "Cups today": the entry may be a Pending day or a week), the Stepper and the unit "cups"; the progress line "1 of at most 2 cups"; the help "At most 2 cups. Enter 0 if you had none: an empty day cannot prove you stayed under the limit.".
- Reduce, yes/no, fixed days: "Record · No phone in bed, avoided every day", sub line "Reduce · avoided every day"; "Did you do it?" with nothing selected; no progress line; the help "Choose No if you avoided it: an empty day cannot prove you stayed under the limit.".
- Reduce, yes/no, N times a day: "Record · Snacking, at most 2 times a day", sub line "Reduce · at most 2 times a day"; "Did you do it?"; the progress line "1 of at most 2 times"; the same help as above.
- Build, count, N times a week: "Record · Reading, 20 pages, 3 times a week", sub line "Build · at least 20 pages · 3 times a week"; "For" with the week ("26 Oct – 1 Nov") and the "Date" select; the field "Amount" with the unit "pages"; the progress line "2 of 3 times"; the help "Partial amounts are kept. With fewer than 3 records of at least 20 pages the week counts as Failed when it is finalized.".

Every variant ends with "Note (optional)", then "Save record" and "Cancel". The note field is empty here because none of these entries has a note; it opens with the entry's current note when there is one (`RecordDialog.md`, "Note, buttons and refusals"). Its help depends on the goal:

- "Clear the field to remove the note." for the commitment-based goals (Yoga, 3 days a week; No phone in bed, avoided every day);
- "This note is for the whole day. Clear the field to remove it." for the per-day periods (Coffee, at most 2 cups a day; Snacking, at most 2 times a day);
- "This note is for the whole week. Clear the field to remove it." for the per-week period (Reading, 20 pages, 3 times a week).

No "Excuse instead" (slice 7); the full preview keeps it, still provisional.

Amended 2026-11-20, specs/006-note-editing, ADR 0019: every variant had the help "An empty field keeps the existing note."; the field now opens with the current note, and an empty field removes it.
