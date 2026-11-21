# PauseDialog

Pauses a goal (commitment-based or period-based) for a date range.

- Fields: From, Until (required), Reason (optional). From cannot be before today.
- Explain the effect first: paused days are not counted, the streak stays, final results do not change.
- While paused the goal page shows a Notice "Paused until Sun 22 Nov · Resume now".

Changed 2026-11-21 for slice 7 (specs/008-excusals-and-pauses, HP-002, HP-007): the full preview's texts disagreed with the spec and were replaced: the title was "Pause goal · Yoga" with the sub line "Commitment-based · Mon · Wed · Fri", the effect "While a goal is paused, its commitments are not counted. Your streak stays as it is. Results that are already final do not change.", the Reason placeholder "For example: holiday" and the help "You can resume earlier at any time."; the Notice example was "Paused until Sun 8 Nov · Resume now".

## Slice 7 state

The last part of the preview shows the dialog as slice 7 builds it (HP-002, HP-007, ADR 0008); the full preview above is the same dialog. One Razor component, `PauseDialog`, built on `HtDialog`.

- It opens from the goal page's secondary "Pause" (`aria-haspopup="dialog"`) in the page head (`pages/PageGoalDetail.md`, "Slice 7 state").
- Title: "Pause · " and the goal's name: "Pause · Yoga, 3 days a week", "Pause · Drinking water, 8 glasses a day". No sub line: HP-002 names none (settled here, T004).
- Body, in this order:
  1. the effect, as a paragraph: "Paused days are not counted. Your streak stays as it is. Final results do not change.";
  2. "From" and "Until" side by side (`ht-field-pair`, stacked under 480px), both `input.ht-input[type=date]`:
     - "From": by default the page's current date (the goal's current date, or for a goal that has not started the account's today, research R-03), with `min` that date; the page reads it as `PausesResult.PauseDate` and gives it to the dialog (settled here, T062);
     - "Until": empty on open, required (`aria-required="true"`). No native `required` and no `min`: the use case decides, so its message shows under the field (FR-006);
  3. "Reason (optional)": `input.ht-input[type=text]`, "(optional)" in `ht-muted`, empty on open, no placeholder and no `maxlength` (the use case trims and refuses more than 200 characters).

  No help line (settled here, T004: the spec gives no text for one).
- Footer: the primary "Pause goal" and the secondary "Cancel".
- Refusals: one at a time, under the field it is about (`ht-field__error`, the input `aria-invalid="true"` and `aria-describedby` the error's id), with the result's message as it is (`PauseTexts`):

  | Result | Message | Under |
  |---|---|---|
  | `DaysMissing` | "Choose the first and the last day." | the empty field: "From" when "From" is empty, else "Until" |
  | `StartsBeforeToday` | "A pause cannot start before today." | "From" |
  | `LastBeforeFirst` | "The last day cannot be before the first day." | "Until" |
  | `Overlaps` | "This goal already has a pause on these dates." | "Until" |
  | `ReasonTooLong` | "Keep the reason to 200 characters or fewer." | "Reason (optional)" |

  The fields keep what was entered. A refusal that is about no field (the account or the goal, FR-006 steps 1 to 3) is not expected on the goal page; it shows as an `ht-field__error` paragraph with `role="alert"` above the fields (settled here, T004).
- After "paused", the dialog closes and the goal page reads everything again: the Notice "Paused until …" when the pause is current (and "Pause" leaves the page head), the Pauses card, and the figures. Focus returns to "Pause", or, when the re-read removed it (the pause is current), to the nearest remaining control, the Notice's "Resume now" (HP-007).
- Focus moves to the first field ("From") on open; Esc and × close the dialog, and focus returns to "Pause".
- Every text comes from `PauseTexts` and the shared texts (HP-008); the dialog builds none.
