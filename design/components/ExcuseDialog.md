# ExcuseDialog

Excuses one commitment of a commitment-based goal: it is then Excused, left out of consistency, and neither extends nor breaks the streak (SPEC §5.3).

- A guide of its own, not a variant of `Dialog`, because the code has a component of this name (plan, Work order 1). `Dialog.md` links here.
- Only a commitment that is Due or Pending and has no record can be excused (D-2, D-3): never an Upcoming one (a planned absence uses a pause) and never a final one.
- Explain the effect before the confirm button. The reason is optional and private; it is not a note and does not appear in Recent notes.

## Slice 7 state

The preview shows the dialog as slice 7 builds it (HP-003, HP-007, D-2, D-3, ADR 0008); the full preview and the "Slice 7 state" block show the same dialog. One Razor component, `ExcuseDialog`, built on `HtDialog`.

- Openers:
  - the ghost "Excuse" on a Pending row of a commitment-based goal with no record on the signed-in Home (`components/CommitmentRow.md`, "Slice 7 state");
  - the ghost "Excuse instead" in the RecordDialog, for the commitment its "For" names, when that commitment is Due or Pending with no record (`components/RecordDialog.md`, "Slice 7 state"). The Excuse dialog takes the RecordDialog's place: the RecordDialog closes, and the Excuse dialog's opener is the control that opened the RecordDialog (settled here, T005).
- Title: "Excuse · " and the goal's name: "Excuse · Yoga, 3 days a week".
- Sub line (`ht-dialog__sub`): the commitment's date and state as `RecordingText` writes the RecordDialog's "For" entry: "Wed 11 Nov · Pending, 22 h left" for a Pending commitment (N the whole hours to the end of its grace period, rounded up), "Today, Fri 13 Nov" for the Due one (settled here, T005: a commitment is Due only on the goal's current date, and that is how `RecordingText` writes it).
- Body, in this order:
  1. the effect, as a paragraph: "An excused commitment is not counted. Your streak stays as it is.";
  2. "Reason (optional)": `input.ht-input[type=text]`, "(optional)" in `ht-muted`, empty on open, no placeholder and no `maxlength` (the use case trims it and refuses more than 200 characters).
- Footer: the primary "Excuse commitment" and the secondary "Cancel".
- Refusals, one at a time, with the result's message as it is (`ExcusalTexts`):
  - "Keep the reason to 200 characters or fewer." under "Reason (optional)" (`ht-field__error`, the input `aria-invalid="true"` and `aria-describedby` the error's id);
  - any other refusal, for example "This commitment is final and can no longer be excused." when its grace period ended while the dialog was open, or "This commitment has a record. Delete the record to excuse it.", as an `ht-field__error` paragraph with `role="alert"` between the effect and the field (settled here, T005: the spec says only that it shows in the dialog).

  The field keeps what was entered.
- After "excused", the dialog closes and the opener reads its data again: on the Home the row leaves the Today list and the "Pending · still can record" tile drops by 1; on the goal page the History row shows "Excused". Focus returns to the opener, or, when the re-read removed it (the Home row is gone), to the nearest remaining control (HP-007). The nearest remaining control, per page (settled here, T051, T052):
  - Home: the action of the row now in the excused row's place in the Today list as shown, else of the row before it (an Upcoming row has no action and is skipped), else the "Due now" tile, as slice 6 falls back;
  - goal page: the head's "Record" when it opened the RecordDialog and is still there; otherwise the History row's "Add record", or, on the now Excused row, its note control;
  - Goals page ("Excuse instead" from an item's "Record"): the goal's "Record", or its name link when nothing is left to record, as slice 6 does for the RecordDialog.
- The RecordDialog's "Excuse instead" is offered wherever the RecordDialog opens: the goal page, the Home's rows ("Record" of a Due commitment) and the Goals page's items (settled here, T050; `RecordDialog.md`, "Slice 7 state").
- Focus moves to the first field ("Reason (optional)") on open; Esc and × close the dialog, and focus returns to the opener.
- Every text comes from `ExcusalTexts`, `RecordingText` and the shared texts (HP-008); the dialog builds none.
- The preview: "Yoga, 3 days a week" (spec US1) opened from the Home on Fri 13 Nov 2026, 02:00 for the Pending Wed 11 Nov, and from "Excuse instead" at 07:00 for the Due Fri 13 Nov; "Reading, 20 min daily" with a reason of more than 200 characters, and with a refusal about the commitment.
