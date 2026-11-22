# PageNewAttempt

The Start new attempt page: starts a new attempt of an ended goal, with the earlier settings or new ones, its own history and a streak from zero (SPEC §5.4). New page, designed in slice 8 from `PageNewGoal` (HP-004).

- Opened from "Start new attempt": the ended goal's Notice on its goal page (`pages/PageGoalDetail.md`, "Slice 8 state") and its item on the Goals page's Ended tab (`components/GoalListItem.md`, "Slice 8 state"). "Start attempt" opens the goal page.
- Left: the New goal form card without its Habit section and without the name. Right: the New goal page's sticky summary card.
- A new page rather than a state of `PageNewGoal` (HP-004 allows either; settled here, T005, and recorded in `pages.md`): its address, its head line and its missing fields differ from the New goal page's, and it starts from an existing goal, not from a habit.

## Slice 8 state

The preview shows the page as slice 8 builds it (HP-004, US3-AS1, US3-AS5, US3-AS6, US3-AS14, contracts/host.md "Start new attempt page", ADR 0008); the full preview and the "Slice 8 state" block show the same page. Today is Wed 18 Nov 2026 in the full preview, for "Meditation, 10 min daily", ended on Mon 16 Nov (spec US2, US3).

- Address: `/goals/{id}/new-attempt`, for registered users. Another account's goal or an unknown goal shows the Not found page; a goal that is not ended returns to its goal page (`/goals/{id}`). An administrator gets the Not found page.
- BackLink "← {goal name}" to the goal page ("← Meditation, 10 min daily"), then `heading-1` "Start new attempt" and, as the head's muted line (where the New goal page names the habit), "Attempt 2 starts with its own history and a streak from zero." (`GoalChangeTexts`, with the next attempt number, `NewAttemptValues.AttemptNumber`). The line's place is settled here (T005): the spec gives the line and not its place.
- `NewGoalForm` in its attempt mode, filled with the starting values (`NewAttemptValues`, FR-011): the settings of the ended attempt's last version and the start date, the later of the account's today and the day after the ending date:
  1. "Do you want to build or reduce it?" with the SegmentedControl Build | Reduce;
  2. "How do you measure it?" with the ChoiceCards Yes / no, Count and Duration;
  3. Target, for Count and Duration only;
  4. "When?" with every schedule kind, as on the New goal page: the ChoiceCards "On fixed days" and "A number of times", the DayPicker, or the times Stepper and "Per day" | "Per week". A new attempt may change the schedule kind (D-1), so a new attempt counted per week counts its streak in weeks (the preview's first state);
  5. "Start date", a date input alone in its `ht-field-pair` (the name's place stays empty, so the field keeps the New goal page's width).

  No Habit section (the goal's habit never changes) and no name (the goal's own name belongs to the goal and applies to every attempt, FR-004; it is changed on the Edit settings page).
- Actions (`ht-form-actions`): the primary "Start attempt" and the secondary "Cancel" (a link back to the goal page). "Start attempt" asks for no confirmation; "attempt started" opens the goal page.
- The summary card (`ht-summary`), as on the New goal page (`pages/PageNewGoal.md`, "Summary card"): the habit's name and the sentence with the start date ("Meditation: at least 10 min daily, starting Wed 18 Nov."), Evaluated, "Met when", "Streak counted in", "Late records", and the first-week Notice for a per-week attempt whose start date is not a Monday ("Your first week starts on Mon 23 Nov. Days before it are not evaluated."). No catalog suggestion Notice. While the form is not complete, "Complete the form to see your goal here.".
- Refusals: one at a time (FR-012), as `ht-field__error` under its field (`AttemptStartResult.Field`), with the result's message as it is:
  - every target refusal of the New goal page, under the same field and in the same order (`pages/PageNewGoal.md`, "Refusals"), with "A Reduce goal with an amount has 1 time per period." under the times Stepper;
  - under "Start date", in this order: "Choose a start date."; "The start date cannot be in the past."; "A new attempt starts after the day the goal ended."; "Choose a start date within a year from today.".

  A refusal about no field (the account, the goal, "Only an ended goal can start a new attempt." when the goal was restarted elsewhere while the page was open) shows as an `ht-field__error` paragraph with `role="alert"` above the form's first section (settled here, T005).
- Focus moves to the first field ("Build" of the Build or reduce control) when the page opens (form conventions of slices 4 and 7, HP-009).
- The preview's states: the full preview (the starting values unchanged, spec US3-AS1); another schedule kind, 3 times a week from a Wednesday, with the first-week Notice (spec US3-AS5); "A new attempt starts after the day the goal ended." on the day of the ending (spec US3-AS6); "The start date cannot be in the past.".
