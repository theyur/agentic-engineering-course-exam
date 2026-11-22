# PageEditGoalSettings

The Edit settings page: changes a goal's target, measurement, schedule within its kind, and name, from the goal's next commitment or evaluation period (SPEC §5.4). New page, designed in slice 8 from `PageNewGoal` (HP-002). It replaces the earlier note in `pages.md` that the page reuses the New goal form "with the title 'Edit goal' and a Notice: 'Changes apply from the next commitment. …'", whose texts disagreed with the spec.

- Opened from the goal page's ghost "Edit settings" (`pages/PageGoalDetail.md`, "Slice 8 state"). Saving returns to the goal page, whose Settings card then shows the later settings with their date ("From Thu 12 Nov").
- Left: the New goal form card without its Habit section and without the start date. Right: the New goal page's sticky summary card.
- The schedule keeps its kind: fixed days, times per day or times per week (D-1). Another kind needs a new attempt.

## Slice 8 state

The preview shows the page as slice 8 builds it (HP-002, US1-AS7, US1-AS12, US1-AS15, contracts/host.md "Edit settings page", ADR 0008); the full preview and the "Slice 8 state" block show the same page. Today is Wed 11 Nov 2026, 10:00 in the full preview, for "Reading, 15 min daily" (spec US1).

- Address: `/goals/{id}/edit`, for registered users. Another account's goal or an unknown goal shows the Not found page; an ended goal returns to its goal page (`/goals/{id}`). An administrator gets the Not found page.
- BackLink "← {goal name}" to the goal page ("← Reading, 15 min daily"), then `heading-1` "Edit settings", in `ht-page-head ht-detail-head`. No muted line (settled here, T004: the BackLink names the goal, and the spec gives no other line; the heading repeats the goal page's action).
- `NewGoalForm` in its edit mode, filled with the latest settings (the later version when there is one, else the settings in effect) and the goal's own name:
  1. "Do you want to build or reduce it?" with its help line and the SegmentedControl Build | Reduce (group label "Build or reduce"), as on the New goal page;
  2. "How do you measure it?" with its help line and the ChoiceCards Yes / no, Count and Duration;
  3. Target, for Count and Duration only: "At least" or "At most", the Stepper and the unit, as on the New goal page;
  4. "When?", limited to the goal's schedule kind (settled here, T004):
     - no ChoiceCards "On fixed days" / "A number of times": the goal's kind is the only one offered;
     - fixed days: the DayPicker, then "Each selected day is one commitment.";
     - a number of times: the Stepper (1 to 20) and "time" or "times", then the note of the goal's period, "Each day is evaluated as a whole." or "Each week, Monday to Sunday, is evaluated as a whole.". No "Per day" / "Per week" control: the period is part of the schedule kind (D-1), and the note names it;
  5. "Name (optional)" alone in its `ht-field-pair` (the start date's place stays empty, so the field keeps the New goal page's width).

  No Habit section and no start date: a goal's habit and start date never change (spec "Assumptions").
- Above the actions, in its own `ht-form-section`, the neutral Notice (`ht-notice ht-notice--neutral`, "a rule people must know", `components/Notice.md`): "Changes apply from Thu 12 Nov. Past results keep their rules." The date is `GoalDetails.ChangesApplyFrom`: the next boundary of the goal (the next day, or the Monday after the current week for a goal counted per week), or the start date of a goal that has not begun. The text comes from `GoalChangeTexts` (HP-008). The Notice style is settled here (T004): the spec calls it a line, and a neutral Notice is the design's container for one rule.
- Actions (`ht-form-actions`): the primary "Save changes" and the secondary "Cancel" (a link back to the goal page). "Save changes" asks for no confirmation. "settings changed" and "settings unchanged" both return to the goal page.
- The summary card (`ht-summary`, the New goal page's, `pages/PageNewGoal.md` "Summary card"): "Your goal", the sentence, Evaluated, "Met when", "Streak counted in" and "Late records", rewritten on every change through the same formatter (`GoalSummaryText`). The sentence's date is the date the change applies from ("Reading: at least 15 min daily, starting Thu 12 Nov."), as the edit form has no start date (settled here, T004). No catalog suggestion Notice (the edit form has no suggestion). The first-week Notice follows the New goal page's rule with that date, so it shows only for a per-week goal that has not begun and whose start date is not a Monday. While the form is not complete, the card shows "Complete the form to see your goal here.", as on the New goal page.
- Refusals: one at a time (FR-003), as `ht-field__error` under its field (`SettingsChangeResult.Field`), with the result's message as it is, `aria-invalid="true"` on an input and `aria-describedby` on the input or the group:
  - every target refusal of the New goal page, under the same field and in the same order (`pages/PageNewGoal.md`, "Refusals", from "Choose Build or Reduce." to "Choose per day or per week."), then "A Reduce goal with an amount has 1 time per period." under the times Stepper (the preview's second example);
  - "A change keeps the schedule kind: fixed days, times per day or times per week. End the goal and start a new attempt to change it." under "When?" (`Schedule`). The page offers no other kind, so it shows only if a request carries one;
  - "The name can have at most 60 characters." under "Name (optional)".

  A refusal about no field (the account, the goal, "This goal has ended." when it was ended elsewhere while the page was open) shows as an `ht-field__error` paragraph with `role="alert"` above the form's first section (settled here, T004).
- Focus moves to the first field ("Build" of the Build or reduce control) when the page opens (form conventions of slices 4 and 7, HP-009).
- The preview's states: the full preview (fixed days, every day, a change from Thu 12 Nov); a goal counted per week, yes/no, with 4 times entered and the change from Mon 23 Nov (spec US1-AS7); the refusal "A Reduce goal with an amount has 1 time per period."; and the Notice of a goal that has not begun, whose change applies from its start date Mon 23 Nov (spec US1-AS12).
