# RecordDialog

The form to record what happened for one commitment or period; the field depends on the goal's measure.

## Variants (see also RecordForms)
- **Yes / no**: SegmentedControl "Did you do it?" Yes | No.
- **Count**: Stepper with the unit ("cups", "times").
- **Duration**: Stepper in minutes, with a progress bar and "12 of 20 min".

## Rules
- "For" select: every Due and every Pending commitment or period (FR-011) that is not Excused or Paused (slice 7 FR-005, FR-009); none on an unscheduled day. Finalized dates are corrected from the goal page History table.
- Show the target in the sub line ("Build · at least 20 min · every day") and progress under the field.
- Say what partial progress means: it is kept, and below target the result is Failed when finalized.
- Reduce goals: ask for an explicit number, 0 included, or an explicit "No" for yes/no: "At most 2. Enter 0 if you had none: an empty day cannot prove you stayed under the limit.". An empty day is never treated as success: it is Missed (FR-013).
- "Excuse instead" (ghost, right) only for commitment-based goals and only while the commitment that "For" names is Due or Pending with no record (slice 7 D-2, D-3). It opens the Excuse dialog (`ExcuseDialog.md`). Confirmed in slice 7 (specs/008-excusals-and-pauses, HP-003); it was marked provisional, for upcoming or Pending commitments, until SPEC §11 Q3 was decided.
- Note is optional and private.

## Slice 5 state

The last part of the preview shows the dialog as slice 5 builds it (HP-002, ADR 0008): opened from "Record", as "Edit" of one record, and for a per-week goal from "Record" and from a History row. The "Edit" and the History-row examples have a note, so their note field opens filled. `RecordForms.md` has the other measurements. The texts below are the ones the code uses (`RecordingText`, `RecordTexts`); the examples are written as they appear.

### Title and sub line

- Title: "Record · " and the goal's name: "Record · Reading, 20 min daily", "Record · Yoga, 3 days a week", "Record · Coffee, at most 2 cups a day". The same title for "Edit".
- Sub line: Build or Reduce and the target:
  - a Reduce goal: "Reduce · " and the goal's target text (slice 4's `GoalText.TargetText`): "Reduce · at most 2 cups a day", "Reduce · at most 1 sweet a day", "Reduce · at most 2 times a day", "Reduce · avoided every day";
  - a Build count or duration goal: "Build · at least ", the amount (slice 4's `GoalText.AmountText`: "20 min", "1 h", "20 pages"), " · " and the schedule part: "Build · at least 20 min · every day", "Build · at least 20 pages · 3 times a week";
  - a Build yes/no goal: "Build · " and the schedule part: "Build · Mon · Wed · Fri", "Build · 3 times a week", "Build · every day";
  - the schedule part:
    - "every day" for fixed days on all seven days, and for 1 time a day;
    - otherwise, for fixed days, the day groups of `GoalText.ScheduleText` as they are: "Mon · Wed · Fri", "Mon–Fri", "Mon–Wed · Sat";
    - "N times a day" for N times a day ("2 times a day");
    - "once a week" or "N times a week" for a per-week goal, without `ScheduleText`'s "Flexible · ".

  Nothing is lower-cased by code: every part is written as it is produced.

### "For"

- Opened from the page head's "Record": a select of every Due and every Pending commitment or period (FR-011), newest first (by first date). The dialog is not offered when there is none. Each entry:
  - a commitment or a day: its date ("Wed 28 Oct"), with "Today, " before it only for the Due entry whose single date is the goal's current date ("Today, Fri 30 Oct");
  - a week: its date range ("26 Oct – 1 Nov", "28 Sep – 4 Oct" across a month end), never with "Today, ";
  - a Pending entry adds " · Pending, N h left", N being the whole hours from now to the end of its grace period, rounded up, so the last hour reads "1 h left": "Wed 28 Oct · Pending, 22 h left", "19 – 25 Oct · Pending, 22 h left".
- A week entry adds a second select, "Date", beside "For" (`ht-field-pair`): that week's dates up to the goal's current date, newest first ("Tue 27 Oct", "Mon 26 Oct"), the first one chosen. It changes with the chosen week.
- Opened from a History row's "Add record": "For" is fixed to that row and shown as text, not a select: the row's date ("Wed 28 Oct") or the week's date range ("12 – 18 Oct"). For a week the "Date" select lists the week's dates up to the goal's current date, newest first.
- "Edit" (from a commitment row's "Edit" or a period's record button): "For" is fixed to the record's date, shown as text ("Wed 28 Oct"); no "Date" select.

### The field

By measurement:
- yes/no: "Did you do it?", Yes | No in a SegmentedControl with nothing selected (both `aria-pressed="false"`);
- count: "Amount", a Stepper (minimum 0, step 1) and the goal's unit as stored after it ("cups", "pages", "sweet");
- duration: "Time spent", a Stepper of whole minutes (minimum 0, step 5) and "minutes" after it, also for a goal in hours.

The Stepper's input has the field label as its `aria-label` ("Amount", "Time spent"). "Edit" fills the field with the record's value.

### Progress line

Under the field: `ht-target` with an `ht-progress` bar (`aria-hidden="true"`) and the progress text. It is live: it shows the commitment or period as it would be after "Save record", with the entered value included. The bar is filled to value / target, capped at full (a target of 0, a Reduce "at most 0", is full once the value is above 0 and empty at 0).

| Commitment or period | Progress line |
|---|---|
| a yes/no commitment | none: the field already shows the answer |
| a count or duration commitment with no value yet (nothing entered, or an entry out of range) | none |
| a Build count or duration commitment | "12 of 20 min", "15 of 20 pages", "25 of 20 min" |
| a Reduce count or duration commitment | "1 of at most 2 cups", "45 of at most 60 min" |
| a Build period (yes/no, count or duration) | the records that reach the amount against N: "2 of 3 times" ("0 of 1 time" for N = 1) |
| a Reduce yes/no period | the "Yes" records against N: "1 of at most 2 times" |
| a Reduce count or duration period | the total against the amount: "1 of at most 2 cups", "3 of at most 1 sweet" |

Durations are always in minutes; a goal in hours compares with the amount × 60 ("45 of 60 min" for "1 h"). Count amounts carry the unit as stored, never inflected: "3 of at most 1 sweet", "1 of at most 2 cups". There is no plural rule for units.

### Help under the field (`ht-field__help`)

- A Build count or duration goal: the partial-progress sentence. "Partial time is kept." (duration) or "Partial amounts are kept." (count), then:

  | Schedule | Rest of the sentence | Example |
  |---|---|---|
  | fixed days | "Below {amount} the day counts as Failed when it is finalized." | "Partial time is kept. Below 20 min the day counts as Failed when it is finalized." |
  | 1 time a day | "Without a record of at least {amount} the day counts as Failed when it is finalized." | "Partial amounts are kept. Without a record of at least 20 pages the day counts as Failed when it is finalized." |
  | N times a day | "With fewer than N records of at least {amount} the day counts as Failed when it is finalized." | "Partial time is kept. With fewer than 2 records of at least 10 min the day counts as Failed when it is finalized." |
  | once a week | "Without a record of at least {amount} the week counts as Failed when it is finalized." | "Partial time is kept. Without a record of at least 1 h the week counts as Failed when it is finalized." |
  | N times a week | "With fewer than N records of at least {amount} the week counts as Failed when it is finalized." | "Partial amounts are kept. With fewer than 3 records of at least 20 pages the week counts as Failed when it is finalized." |

  {amount} is `GoalText.AmountText` ("20 min", "1 h", "20 pages").
- A Build yes/no goal: no help.
- A Reduce count or duration goal: "At most {amount}. Enter 0 if you had none: an empty day cannot prove you stayed under the limit." ("At most 2 cups. Enter 0 if you had none: an empty day cannot prove you stayed under the limit.").
- A Reduce yes/no goal: "Choose No if you avoided it: an empty day cannot prove you stayed under the limit."
- Reduce goals have no partial-progress sentence.

### Note, buttons and refusals

- "Note (optional)", a TextArea with the placeholder "What happened? Only you can see notes.". It opens with the current note of the commitment or period that "For" names, or empty without one, in every mode:
  - from the page head's "Record": the note of the first entry of "For". Choosing another entry in "For" replaces the field's content, typed text included, with that entry's note. Choosing another "Date" of the same week leaves the field alone (the note belongs to the week);
  - from a row's "Add record": that row's note, also on a row without a record that has a note;
  - "Edit" of one record: the note of the record's commitment or period.
- The note's help (`ht-field__help`, tied to the TextArea with `aria-describedby`), one text per goal:

  | Goal | Help |
  |---|---|
  | commitment-based | "Clear the field to remove the note." |
  | period-based, per day | "This note is for the whole day. Clear the field to remove it." |
  | period-based, per week | "This note is for the whole week. Clear the field to remove it." |

  A refusal of the note ("The note can have at most 500 characters.") takes the help's place.
- "Save record" stores what the field shows: an unchanged field keeps the note, an empty field (or one of only spaces) removes it. "Delete record" ignores the field: the note stays.
- Amended 2026-11-20, specs/006-note-editing, ADR 0019: the field was always empty on open (also for "Edit"), with the help "An empty field keeps the existing note."; both go.
- Footer: primary "Save record" and secondary "Cancel". "Edit" adds the ghost "Delete record", pushed right (`ht-spacer`). No "Excuse instead" (slice 7).
- Refusals sit under the field they are about (`ht-field__error`), one at a time, with the result's message as it is.
- Focus moves to the first field on open; Esc and × close the dialog, and focus returns to the button that opened it.

## Slice 7 state

The last part of the preview shows the dialog as slice 7 builds it (HP-003, D-2, D-3, FR-005, FR-009, ADR 0008). Everything of slice 5 stays, with two changes.

### "For" without exempt entries

- Opened from the page head's "Record" (goal page) or the Goals page's "Record", "For" lists every Due and every Pending commitment or period that is neither Excused nor Paused (FR-005, FR-009), newest first, with slice 5's labels. A paused goal's Paused today is not listed, so the list can start with a Pending entry and have no "Today, " entry; a goal with nothing left to record offers no "Record", as in slice 5.
- Opened from a History row's "Add record": an Excused or Paused row has no "Add record" (`pages/PageGoalDetail.md`, "Slice 7 state"), so "For" is never fixed to one.
- For a week, the "Date" select lists the week's dates as in slice 5; a Paused week is not listed at all.

### "Excuse instead"

- The ghost button "Excuse instead" (`ht-btn--ghost`, `aria-haspopup="dialog"`), the last button of the footer, pushed right with `ht-spacer`, after "Save record" and "Cancel".
- Shown only when the goal is commitment-based and the commitment the dialog is for (the selected "For" entry, or the fixed one from a History row's "Add record") is Due or Pending and has no record (D-2, D-3). It follows the selection: choosing another "For" entry shows or hides it.
- Never in "Edit" of one record (that commitment has a record, D-3), never for a period (per day or per week; FR-002 step 4), never for an Upcoming or a final commitment.
- It opens the Excuse dialog (`components/ExcuseDialog.md`) for that commitment in the RecordDialog's place: the RecordDialog closes without saving, and the Excuse dialog's opener is the control that opened the RecordDialog. What was typed in the RecordDialog is not kept (settled here, T006).
- No other change: the title, sub line, field, progress line, help, note and refusals are slice 5's.
- The previews: "Yoga, 3 days a week" (spec US1) on Fri 13 Nov 2026, 07:00, with the Due "Today, Fri 13 Nov" chosen and "Excuse instead" shown; "Drinking water, 8 glasses a day" (spec US2) on Mon 16 Nov, 10:00, paused from that day, whose "For" lists only the Pending Sun 15 and Sat 14 Nov, with no "Excuse instead".
