# PageNewGoal

The New goal form, opened from "Use this habit" (pre-filled with the suggested starting goal) or "+ New goal".

- Left: one Card form in sections — Habit (with Change), Build / Reduce (SegmentedControl), measure (ChoiceCard: Yes/no, Count, Duration), Target ("At least" / "At most" + Stepper + unit), When (ChoiceCard fixed days with DayPicker, or N times per day/week with a Stepper), Start date and optional name. Actions: Create goal, Cancel.
- Right: a sticky summary card that rewrites the goal as one plain sentence and lists how it is evaluated (evaluated per scheduled day, per day or per week, met when, streak unit, late records within 48 h). Its texts are those of the "Summary card" section below.
- Reduce switches the target word to "At most". Fixed days = commitment-based; a number of times per period = period-based.

## Slice 4 state

The second part of the preview shows the page as slice 4 builds it (HP-001, HP-002, ADR 0008). Today is Tue 17 Nov 2026 in the preview.

- Address: `/goals/new`, with `habit`, `kind` (`catalog` or `custom`) and `suggestion` (counted from 1) when it is opened from a habit. "Use this habit" and "Start with this" open it with a catalog habit, "Save and create goal" with a custom habit, "+ New goal" with no habit.
- BackLink to where the user came from, named after that page: the catalog habit's details ("← Reading"), the custom habit page ("← Cold shower"), or "← Goals" with no habit. Cancel goes to the same page.
- Head: `heading-1` "New goal" and one muted line:
  - catalog habit: "Based on the catalog habit Reading. Change anything you like.";
  - custom habit: "Based on your custom habit Cold shower. Change anything you like.";
  - no habit: "Start from a catalog habit or one of your custom habits.".
- Habit: the habit's glyph, its name, the category Tag (a custom habit adds the strong "Private" Tag) and ghost "Change" (accessible name "Change habit"). With no habit: "Choose the habit this goal is for." and secondary "Choose a habit". Both buttons open the habit chooser.
- Habit chooser: a Dialog "Choose a habit" with the sub line "Catalog habits and your custom habits", a SearchField "Search habits", then the list "Catalog" (the Published catalog habits) and the list "Your custom habits" (only when the user has one), each in name order. Each row is a button with the glyph, the name and "category · intent" ("Mind · Build"). The search matches names as the catalog search does; with no match the Dialog says "No habit matches your search.". Choosing a row closes the Dialog and starts the form again from that habit's starting values (FR-007), as if the page had been opened with it. Cancel, × and Esc close it with no change. Focus moves to the SearchField on open and returns to the button that opened it.
- "Do you want to build or reduce it?" with its help line and the SegmentedControl Build | Reduce, whose group label is "Build or reduce" (never "intent", `CONTEXT.md`). A catalog habit's starting values choose it; with no habit or a custom habit, the habit's intent chooses it.
- "How do you measure it?": ChoiceCards Yes / no ("Did it or not"), Count ("Times, pages, cups") and Duration ("Minutes or hours").
- Target, for Count and Duration only: "At least" (Build) or "At most" (Reduce), the Stepper (Build 1 to 999, Reduce 0 to 999), then the unit: a text input for Count, or the SegmentedControl Minutes | Hours for Duration. Yes / no has no Target section.
- "When?": ChoiceCards "On fixed days" ("For example Mon to Fri") and "A number of times" ("For example 5 times a week").
  - Fixed days: the DayPicker, then "Each selected day is one commitment.".
  - A number of times: the Stepper (1 to 20), "time" for 1 or "times", the SegmentedControl Per day | Per week, then "Each day is evaluated as a whole." or "Each week, Monday to Sunday, is evaluated as a whole.".
- Start date (a date input, today at first) and "Name (optional)", side by side (`ht-field-pair`, one column on phones).
- Actions: primary "Create goal" and secondary "Cancel". "Create goal" asks for no confirmation and opens the new goal's page (`/goals/{id}`).
- No "You can record partial amounts" and no "You can excuse a single day later": recording comes in slice 5, excusals in slice 7.

### Summary card

The sticky card `ht-summary` on the right, rewritten on every change. All its texts come from one formatter (`GoalSummaryText`, constitution IV).

- "Your goal" (eyebrow), then the goal as one sentence (`ht-card__title`): the habit's name, ": ", "at least " for a Build count or duration goal only, the goal's target text, ", starting ", the start date as "Tue 17 Nov", and ".". No habit-specific verb.
  - "Reading: at least 15 min, 5 days a week, starting Tue 17 Nov."
  - "Sugar: at most 1 sweet a day, starting Tue 17 Nov."
  - "Cold shower: daily, starting Tue 17 Nov."
  - "Yoga: 3 times a week, starting Tue 17 Nov."
- Then a KeyValue list, in this order:

  | Row | Fixed days (commitment-based) | N times per day | N times per week |
  |---|---|---|---|
  | Evaluated | Each scheduled day | Each day | Each week |
  | Met when | see below, "that day" | see below, "that day" | see below, "that week" |
  | Streak counted in | Commitments | Days | Weeks |
  | Late records | Up to 48 h after each day | Up to 48 h after each day | Up to 48 h after each week |

- "Met when" (FR-004). *Amount* is the amount part of the target text ("15 min", "1 h", "20 pages", "1 sweet", "0 cigarettes"); *times* is "once" for 1 and "N times" otherwise:

  | Goal | Build | Reduce |
  |---|---|---|
  | Fixed days, yes/no | "Done that day" | "Not done that day" |
  | Fixed days, count or duration | "At least *amount* that day" ("At least 15 min that day") | "At most *amount* that day" ("At most 2 cups that day") |
  | N times per day or per week, yes/no | "Done at least *times* that day" / "… that week" ("Done at least 3 times that week", "Done at least once that day") | "Done at most *times* that day" / "… that week" ("Done at most once that day") |
  | N times per day or per week, count or duration | "*amount* or more, at least *times* that day" / "… that week" ("20 pages or more, at least 3 times that week") | "At most *amount* in total that day" / "… that week" ("At most 1 sweet in total that day"; N is always 1) |

- A per-week goal whose start date is not a Monday adds the Notice "Your first week starts on Mon 23 Nov. Days before it are not evaluated." (the first Monday after the start date). A Monday start date adds nothing.
- When a suggested starting goal gave the starting values, the neutral Notice "Catalog suggestion: 15 min, 5 days a week. An example to adjust, not a prescribed target." follows, with the catalog suggestion text of FR-007 (under the goal rules). It stays while the user changes the form; it is not shown without a suggestion.
- While the form is not complete (no habit chosen, the target fields do not pass the checks of FR-006 up to and including "A Reduce goal with an amount has 1 time per period.", or no start date), the card shows only "Your goal" and the line "Complete the form to see your goal here.": no sentence, no list, no Notice. A start date in the past or too far ahead, and a long name, do not make it incomplete; "Create goal" refuses them.

### Refusals

One result at a time (FR-006), as `ht-field__error` under its field, with `aria-invalid="true"` on an input and `aria-describedby` on the input or the group. The preview shows each one. In FR-006 order, with the field (`GoalField`) each one sits under:

- Habit (`Habit`): "Choose a habit."; "This habit is not in the catalog."; "This custom habit does not exist.". The last two also appear when the address names a habit that cannot be used; the page then opens with no habit chosen.
- "Do you want to build or reduce it?" (`BuildOrReduce`): "Choose Build or Reduce."
- "How do you measure it?" (`Measurement`): "Choose how it is measured."; "A yes/no goal has no amount or unit." (the page sends no amount or unit for Yes / no, so this one shows only if a request carries them).
- Target, the Stepper (`Amount`): "Enter a whole number from 1 to 999." (Build); "Enter a whole number from 0 to 999." (Reduce).
- Target, the unit (`Unit`): "Enter a unit, for example pages."; "The unit can have at most 20 characters."; "Choose minutes or hours.".
- "When?" (`Schedule`): "Choose fixed days or how many times."
- The DayPicker (`Days`): "Choose at least one day."
- The times Stepper (`Times`): "Enter a number of times from 1 to 20."; "A Reduce goal with an amount has 1 time per period.".
- Per day | Per week (`Period`): "Choose per day or per week."
- Start date (`StartDate`): "Choose a start date."; "The start date cannot be in the past."; "Choose a start date within a year from today.".
- Name (`Name`): "The name can have at most 60 characters."
