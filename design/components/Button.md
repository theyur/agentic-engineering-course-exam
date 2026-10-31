# Button

Buttons start an action; one primary button per card or page area, for the thing that area is for.

## Variants
- `ht-btn ht-btn--primary`: the main action (Register, Record, Use this habit, Approve & publish). `accent` fill, `on-accent` text.
- `ht-btn ht-btn--secondary`: other actions next to a primary (Browse the catalog, Add time, Cancel). White with a `line-strong` border.
- `ht-btn ht-btn--ghost`: low-weight actions inside rows and cards (Excuse, Add note, See details, Sign in).
- `ht-btn ht-btn--danger`: destructive admin actions (Deactivate account, Reject). Always ask for confirmation.
- Size `ht-btn--sm` (32px) inside rows, tables and the nav; default is 38px (`control-height`). `ht-btn--block` for full width.

## Rules
- Write a verb first, sentence case, no full stop: "Record", "Use this habit", "Add another goal".
- Use `<button>` for actions and `<a class="ht-btn">` for navigation.
- Disabled: the `disabled` attribute (shown at 50% opacity). Prefer explaining why over disabling.
- Focus ring: 2px `accent` outline, 2px offset (from `.ht :focus-visible`).

## Blazor
Make one `HtButton.razor` with `Variant` (Primary, Secondary, Ghost, Danger) and `Small` parameters that renders these classes.
