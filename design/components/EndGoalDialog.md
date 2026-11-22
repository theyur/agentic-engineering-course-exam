# EndGoalDialog

Ends the current attempt of a goal and records its outcome.

- Outcome is required: Achieved, Abandoned or No longer relevant (ChoiceCards, one column). Note is optional.
- Say clearly: history is kept, ending is not a failure, restarting creates a new attempt with a streak from zero.
- After ending, the goal leaves the active list and appears in Goals → Ended and Profile → Goal history.

Changed 2026-11-22 for slice 8 (specs/009-goal-changes, HP-003): the full preview's texts disagreed with the spec and were replaced: "Achieved" was marked as chosen when the dialog opened (now none is chosen), and the title was "End goal · Yoga", the habit's name rather than the goal's name (now "End goal · Yoga, 3 days a week").

## Slice 8 state

The last part of the preview shows the dialog as slice 8 builds it (HP-003, HP-009, US2-AS6, US2-AS7, US2-AS13, ADR 0008); the full preview above is the same dialog. One Razor component, `EndGoalDialog`, built on `HtDialog`.

- It opens from the goal page's ghost "End goal" (`aria-haspopup="dialog"`) in the page head of a goal that is not ended (`pages/PageGoalDetail.md`, "Slice 8 state").
- Title: "End goal · " and the goal's name: "End goal · Meditation, 10 min daily".
- Sub line (`ht-dialog__sub`): the goal page's attempt line, "Attempt 1 · started 2 Nov", or "Attempt 1 · starts 23 Nov" for an attempt that has not begun (`ReportText`, HP-008).
- Body, in this order:
  1. the effect, as a paragraph: "Ending stops new commitments. Your history stays and nothing is counted as failed. To try again later you start a new attempt.";
  2. "How did it end?" (`span.ht-field__label` with an id), then the ChoiceCards in one column (`ht-choices` with `grid-template-columns:1fr`, `role="radiogroup"`, `aria-labelledby` the label): "Achieved" ("I reached what I wanted"), "Abandoned" ("I stopped trying") and "No longer relevant" ("My plans changed"). **None is chosen when the dialog opens.** The titles are the outcome words of `GoalChangeTexts`; the one-line descriptions are the design's own (a ChoiceCard has a title and one line of description, `components/ChoiceCard.md`), and they stay as the full preview has them (settled here, T007: the spec names the cards and gives no other text for them);
  3. "Note (optional)": `textarea.ht-input.ht-textarea` (`components/TextArea.md`), "(optional)" in `ht-muted`, empty on open, with the placeholder "What happened? Only you can see notes." (the full preview's; settled here, T007) and no `maxlength` (the use case trims it and refuses more than 500 characters).
- Footer: the primary "End goal" and the secondary "Cancel".
- Refusals: one at a time, under the field it is about (`ht-field__error`), with the result's message as it is:

  | Result | Message | Under |
  |---|---|---|
  | `OutcomeMissing` | "Choose how the goal ended." (`GoalChangeTexts`) | "How did it end?": the error follows the ChoiceCards, and the radiogroup gets `aria-describedby` the error's id |
  | `NoteTooLong` | "The note can have at most 500 characters." (`RecordTexts.NoteTooLong`) | "Note (optional)": the textarea gets `aria-invalid="true"` and `aria-describedby` the error's id |

  The choice and the note keep what was entered. A refusal that is about no field (the account, the goal, "This goal has already ended." when it was ended elsewhere while the dialog was open) shows as an `ht-field__error` paragraph with `role="alert"` above "How did it end?" (settled here, T007, as the PauseDialog does).
- After "goal ended", the dialog closes and the goal page reads everything again: the Notice "Ended Mon 16 Nov · Abandoned" with "Start new attempt" under the head, and no "Record", "Edit settings", "Pause" or "End goal". The opener is gone, so focus goes to the nearest remaining control, the Notice's "Start new attempt" (HP-009).
- Focus moves to the first field (the first ChoiceCard's radio, "Achieved") on open; Esc and × close the dialog with no change, and focus returns to the head's "End goal".
- Every text comes from `GoalChangeTexts`, `RecordTexts` and `ReportText` (HP-008); the dialog builds none, and logs no outcome or note (HP-010).
- The preview: "Meditation, 10 min daily" (spec US2) opened on Mon 16 Nov 2026, 10:00, with nothing chosen; "End goal" with no outcome; "Abandoned" with a note of 541 characters.
