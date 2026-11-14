# PageAdminCatalogHabit

The administrator's catalog habit form, for creating and editing a catalog habit (slice 2, HP-009). New page, designed in slice 2.

- TopNav (administrator), then "← Administration", the heading "New catalog habit" or "Edit catalog habit" and a muted line: "Created as a Draft. Publish it from the catalog table." for a new habit; the name and the state for an edit, with "Saving keeps its state." (an edit never changes the state).
- One Card form, at most 760px wide. Its first sections are the fields of a habit, as the custom habit form has them (`PageCustomHabit`): Name, Description (optional), Category, "Usually you want to" Build or Reduce, and the icon picker with the 24 "Habit icons".
- Then the section "Suggested starting goals" with "Examples to adjust, not prescribed targets. Up to 5, in the order you give; mark the one the catalog card shows."
- Each suggested starting goal is one editor item (`ht-editor-item`): the title "Suggested starting goal N", the radio "Shown on the card" (exactly one is marked when the list is not empty), and ghost "Move up" (not on the first), "Move down" (not on the last) and "Remove". Each button's accessible name says which suggested starting goal it acts on.
- The fields of an item reuse the New goal form's controls (`PageNewGoal`), so slice 4's goals look alike:
  - "Build or reduce": SegmentedControl Build / Reduce;
  - "Measured as": SegmentedControl Yes/no / Count / Duration;
  - "Amount", only for count and duration: "At least" (Build) or "At most" (Reduce), a Stepper (from 1 for Build, from 0 for Reduce, up to 999, as `components/Stepper`), then for count a text field for the unit ("pages"), for duration a SegmentedControl Minutes / Hours;
  - "Schedule": SegmentedControl "Fixed days" / "A number of times"; fixed days show the DayPicker, a number of times shows a Stepper (1 to 20) and a SegmentedControl "per day" / "per week".
  - Only the controls of the chosen measurement and schedule are shown, so the form never sends a value that does not belong to them.
- Under the list, secondary "+ Add suggested starting goal". With five items it is not offered; the line "A habit can have at most 5 suggested starting goals." takes its place.
- Refusals: a refusal about one suggested starting goal sits under its field inside that item ("Enter a unit, for example pages." under the Amount of suggested starting goal 2). The list refusals ("A habit can have at most 5 suggested starting goals.", "Mark one suggested starting goal for the card.") sit above the list as `ht-field__error` with `role="alert"`. The habit field refusals sit under their fields as on the custom habit form, and "A catalog habit with this name already exists." under Name. One refusal at a time.
- Actions: primary "Save" and ghost "Cancel". Save leads back to the Admin table (`PageAdmin`), which shows the habit in its row; Cancel goes there too without saving.
- An unknown catalog habit's address, and both addresses for anyone who is not an administrator, show the Not found page (a visitor is first sent to Sign in).
