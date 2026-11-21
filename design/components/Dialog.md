# Dialog

A modal window for short tasks that belong to the current page: Record, Add note, Excuse, Pause, End goal.

## Parts
- `ht-backdrop` (fixed, `overlay` colour) holding one `ht-dialog[role=dialog][aria-modal=true]`, 480px wide at most.
- `ht-dialog__head`: `heading-3` title + one muted sub line with the habit, date and state; a close button (×) with `aria-label="Close"`.
- `ht-dialog__body`: fields, 16px apart. `ht-dialog__foot`: primary action first, then Cancel; an optional ghost action pushed right with `ht-spacer`.

## Rules
- Title says the action and the object: "Record · Reading", "Excuse · Yoga, 3 days a week".
- The title's `id` (target of `aria-labelledby`) must be unique on the page; in Blazor generate it per `HtDialog` instance, never a fixed string.
- Explain the effect in plain words before a confirm button (Excuse, End goal, Pause).
- Focus goes to the first field on open, Esc and × close it, focus returns to the button that opened it.
- On phones the dialog takes the full width minus 16px.
- Blazor: one `HtDialog` component with `Title`, `Subtitle`, `ChildContent`, `Footer` render fragments.

## The Excuse dialog has its own guide

Slice 7 (specs/008-excusals-and-pauses, HP-003) builds the Excuse content as a guide of its own, [ExcuseDialog](ExcuseDialog.md) ([preview](ExcuseDialog.html)), because the code has a component of that name (plan, Work order 1); the spec allowed either this guide or a `Dialog` variant. The preview above shows the same content with the spec's texts: the title "Excuse · Yoga, 3 days a week", the sub line "Wed 11 Nov · Pending, 22 h left", the effect "An excused commitment is not counted. Your streak stays as it is.", "Reason (optional)", the primary "Excuse commitment" and "Cancel". Changed 2026-11-21: the preview had the title "Excuse this commitment?", the sub line "Running · Wed 28 Oct · Pending", the effect "An excused commitment is not counted in consistency. Your streak stays as it is.", the placeholder "For example: travel, ill" and the button "Excuse"; the rule above gave "Excuse this commitment?" as its example title.
