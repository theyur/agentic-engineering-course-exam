# Dialog

A modal window for short tasks that belong to the current page: Record, Add note, Excuse, Pause, End goal.

## Parts
- `ht-backdrop` (fixed, `overlay` colour) holding one `ht-dialog[role=dialog][aria-modal=true]`, 480px wide at most.
- `ht-dialog__head`: `heading-3` title + one muted sub line with the habit, date and state; a close button (×) with `aria-label="Close"`.
- `ht-dialog__body`: fields, 16px apart. `ht-dialog__foot`: primary action first, then Cancel; an optional ghost action pushed right with `ht-spacer`.

## Rules
- Title says the action and the object: "Record · Reading", "Excuse this commitment?".
- The title's `id` (target of `aria-labelledby`) must be unique on the page; in Blazor generate it per `HtDialog` instance, never a fixed string.
- Explain the effect in plain words before a confirm button (Excuse, End goal, Pause).
- Focus goes to the first field on open, Esc and × close it, focus returns to the button that opened it.
- On phones the dialog takes the full width minus 16px.
- Blazor: one `HtDialog` component with `Title`, `Subtitle`, `ChildContent`, `Footer` render fragments.
