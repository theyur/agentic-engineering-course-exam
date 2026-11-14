# PageCustomHabit

Create or edit a private custom habit.

- Form card: Name (required), Description, Category (select), usual intent (SegmentedControl), Icon (pick from the "Habit icons" group of `components/Icons`, all 24 in that group's order). Actions: primary "Save and create goal" (opens New goal with this habit), secondary "Save habit", ghost Cancel.
- Sticky side card explains privacy and the Suggest for catalog option.
- Editing an existing custom habit adds a ghost "Suggest for catalog" (SuggestDialog) and a danger "Delete" only when no goals use it.

## Slice 2 state

The second part of the preview shows the page as slice 2 builds it (US4-AS13, HP-007, ADR 0008).

- "← My custom habits" (→ `/custom-habits`), the heading "New custom habit" or "Edit custom habit" with "A private habit for your own goals.".
- The form: Name, Description (optional, TextArea), Category (a select that starts at "Choose a category" for a new habit), "Usually you want to" Build or Reduce (SegmentedControl, none chosen for a new habit), and the icon picker with the 24 "Habit icons". Each icon button has the icon's name as its label and `aria-pressed` for the chosen one.
- Actions: primary "Save habit" and ghost "Cancel". No "Save and create goal" (slice 4), no "Suggest for catalog" (slice 10).
- The privacy card: only the owner sees a custom habit; administrators cannot; custom habits never appear in the catalog or in community statistics. No line about Suggest for catalog.
- Editing adds a danger "Delete" at the right of the actions. It opens a Dialog first: "Delete this custom habit?", the effect in plain words ("The habit is deleted for good. This cannot be undone."), danger "Delete" and secondary "Cancel". Focus moves into the Dialog, Esc and × close it, and focus returns to "Delete".
- After "Save habit" or a confirmed "Delete" the page goes to My custom habits (`/custom-habits`), which shows the saved habit.
- Refusals sit under their field as `ht-field__error`, one at a time, in this order: "Enter a name.", "The name can have at most 60 characters.", "The description can have at most 500 characters.", "Choose a category.", "Choose Build or Reduce.", "Choose an icon.", then "You already have a custom habit with this name." under Name.
- "This custom habit does not exist." (the habit was deleted in another browser) belongs to no field: a Notice (`role="alert"`) above the form with a link to "My custom habits". The form can no longer be saved.
- The address of another user's, a deleted or an unknown custom habit, and any custom habit address for an administrator, shows the Not found page.
