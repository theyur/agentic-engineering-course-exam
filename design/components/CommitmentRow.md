# CommitmentRow

One row per commitment or period in the Today list: name, schedule or progress, a status chip and the next action.

## Parts
- `ht-row__name`: habit / goal name (`body-strong`). Add a `Reduce` tag for Reduce goals.
- `ht-row__meta`: schedule ("Mon · Wed · Fri"), progress ("12 of 20 min", "1 of at most 2 cups") or time left ("22 h left to record").
- `ht-progress` for count and duration goals: `<div class="ht-progress"><span style="width:60%"></span></div>` with `role="progressbar"`.
- One `StatusChip`, then one action button.

## Rules
- Order in the Today card: Pending first (time is running out), then Due today, then finalized Missed.
- Action per state: Due → "Record" (primary for yes/no) or "Add time" / "+1"; Pending → "Record" + ghost "Excuse" (commitment-based goals only); Missed → ghost "Add note".
- Partial progress is shown, never hidden: "12 of 20 min" stays visible after the day ends.
