# Contract: Host, pages, operator log, smoke check and schedule of recording and evaluation

**Feature**: specs/005-recording-and-evaluation · **Plan**: [../plan.md](../plan.md)

These are outside the AC surface (spec HP-001 to HP-012). They are verified by unit, bUnit, host and integration
tests, and by the smoke check.

## Routes

No route is added. `/goals/{Id:guid}` changes:

- The page shell stays static SSR with `[Authorize]`. An administrator, an unknown goal and another account's goal
  get the Not found page with status 404, and a title that names no goal, as in slice 4 (FR-017, slice 4 FR-020,
  FR-021).
- For the owner's goal it renders `GoalDetailBody` (`@rendermode InteractiveServer`, prerendered), whose only
  parameter is the goal identifier (research R-12). The prerendered HTML contains the page head, the History
  card's first 10 rows and the Recent notes card.

## Goal page (`design/pages/PageGoalDetail`, slice-5 state; HP-001)

- **Page head**: the name and tags of slice 4, and the primary action "Record". "Record" is absent when
  `GetRecordableAsync` gives no entry. No other action (slices 7 and 8).
- **Layout** (`design/pages/PageGoalDetail`, slice-5 state; T046): `GoalDetailBody` renders slice 4's "← Goals",
  head and tags, then slice 4's Notice, on top; then two columns in `ht-grid-2`:
  - left (`ht-stack`): the History card;
  - right (`ht-stack`): slice 4's Settings card, then the Recent notes card.

  A goal with no current date, or with no begun entry, has no History card and the design's single-card layout:
  the Settings card alone in `ht-grid-2`, as in slice 4 (T054, T055).
- **Reads**: `GetRecordableAsync` first, then `GetHistoryAsync(D − 365, D)` with that result's `CurrentDate` D
  (research R-12), and `GetRecentNotesAsync`. A goal with no current date (not started) shows no "Record", no
  History card and no Recent notes card, only slice 4's content.
- **History card** (`HistoryCard`, T054): a `section.ht-card` whose head (`ht-card__head`) has the title
  `h2.ht-card__title` "History", `ht-spacer` and the muted sentence "Corrections are evaluated with the rules of
  that date." (`p.ht-card__sub`) beside it, as `design/pages/PageGoalDetail.html` has it (not `HtCard`'s `Title`,
  which cannot render that head). Then a DataTable (`aria-label="History"`) with Date, Recorded, Result, Note and an
  empty-headed actions column (`td.ht-actions`).
  - The begun entries of the last 366 days up to the goal's current date, newest first; Upcoming entries have no
    row. The first 10, then the ghost button "Show all N →" (`aria-expanded="false"`, N counts all the rows), which
    shows the rest in place and is then gone (plan "Resolved in the spec" 7).
  - With no begun entry the card is not rendered (`design/pages/PageGoalDetail`, slice-5 state).
  - **Date**: "Tue 17 Nov" for a commitment or a day (`DateTexts.ShortDate`), and the design's "10 – 16 Aug" for a
    week, or "28 Sep – 4 Oct" across a month end (the new `DateTexts.WeekRange`).
  - **Recorded**: each record by `RecordingText`, for example "12 min", "Yes" or "3 sweet" (the unit as stored). A period's records sit
    side by side, each a button with its value. The cell is "—" without a record.
  - **Accessible names** of the repeated row controls. Each starts with its visible text (WCAG 2.5.3) and names its
    row ({date}: "Tue 17 Nov" or "10 – 16 Aug"):

    | Control | Accessible name |
    |---|---|
    | a period's record button | "12 min, edit record 1 of {date}" (the number counts from 1 in the row) |
    | "Add record" | "Add record for {date}" |
    | "Edit" | "Edit record of {date}" |
    | "Add note" | "Add note for {date}" |
    | a note button | the note's text, then ", edit note of {date}" |
  - **Result**: `StatusChip`.
  - **Note** (US4, T075): on every row, whatever its state, one ghost `ht-btn--sm` button with
    `aria-haspopup="dialog"`: "Add note" without a note, or the note's text with one, named as the table above
    says. Both open the NoteDialog for that row (`HistoryCard.OnNote`). After a save the page reads what can be
    recorded, the history and the recent notes again before the dialog closes, and the focus goes back to the
    row's Note button (`HistoryCard.FocusNoteAsync`), which now shows the note or "Add note".
  - **Row actions**, in the empty-headed fifth column: "Add record" on a commitment without a record; "Edit" on a
    commitment with one; "Add record" on a period with fewer than 50 records. They are offered whatever the state,
    final included (US3, T066). A period's records are buttons side by side (`ht-inline`) in the Recorded cell, each
    opening "Edit" of that record; a commitment's record is text. Every row control is a ghost `ht-btn--sm` button
    with `aria-haspopup="dialog"`. "Add record" opens the RecordDialog's row mode, "Edit" and a record button its
    "Edit" mode. After a save or a delete the page reads the history again before the dialog closes, and the focus
    goes back to the record's button while the row still shows it, otherwise to the row's action.
- **Recent notes card** (right column, after Settings; `RecentNotesCard`, T074, T075): `GetRecentNotesAsync`, read
  after the history and only for a goal with a current date. An `HtCard` titled "Recent notes", then each note,
  newest first, as an `ht-goal` item: the note in quotes and its `StatusChip` on the top line (`ht-goal__top`,
  `ht-spacer` before the chip), its date under it (`ht-goal__meta`: `DateTexts.ShortDate`, or for a week
  `DateTexts.WeekRange`). `RecentNote` carries no schedule kind, so the page passes the history's `GoalScheduleKind`
  and `Period` to the card, and a Due week's chip reads "Due this week". The card is absent when there are none,
  and it does not open the NoteDialog.
- **Absent**: GoalStats, the strip, the trend, pauses and earlier attempts (slices 6 to 8, ADR 0008).

## Dialogs

- **RecordDialog** (HP-002, `design/components/RecordDialog`, `RecordForms`; the texts are `RecordingText`'s, T045):
  - **Title**: "Record · " and the goal's name ("Record · Reading, 20 min daily"), the same for "Edit"
    (`RecordingText.Title`).
  - **Sub line** (`RecordingText.TargetLine`): "Reduce · " and the target text for a Reduce goal ("Reduce · at most
    2 cups a day", "Reduce · avoided every day"); "Build · at least ", the amount, " · " and the schedule part for a
    Build count or duration goal ("Build · at least 20 min · every day"); "Build · " and the schedule part for a Build
    yes/no goal ("Build · Mon · Wed · Fri", "Build · 3 times a week").
  - **Opened from "Record"**: "For" is a select of `GetRecordableAsync`'s entries, newest first, worded as the design
    shows them (`RecordingText.ForLabel`: "Today, Fri 30 Oct", "Wed 28 Oct · Pending, 22 h left", a week's "26 Oct –
    1 Nov"). For a per-week entry, a second select "Date" (`ht-field-pair`) lists that week's dates up to the goal's
    current date, newest first, the first one chosen. Focus moves to "For" on open.
  - **Opened from a row's "Add record"**: "For" is fixed to that row, shown as text (`RecordingText.EntryDate`: "Wed
    28 Oct", "12 – 18 Oct"), and for a week the "Date" select lists the row's dates up to the current date, newest
    first. "Save record" calls `RecordAsync` as from the head (T063).
  - **"Edit"** (a commitment row's "Edit" or a period's record button): "For" is fixed to the record's date, shown as
    text ("Wed 28 Oct"), with no "Date" select; the field holds the record's value; the progress is
    `RecordPreview.Progress` with that record as `replacing`. "Save record" calls `ChangeRecordAsync`, "Delete record"
    `DeleteRecordAsync`; "This record does not exist." sits under "For" (T063).
  - **Field**, by measurement, labelled by `RecordingText.FieldLabel` (`RecordFields`):
    - yes/no: "Did you do it?", with Yes and No in a `SegmentedControl` and nothing selected;
    - count: "Amount", a `Stepper` (from 0, step 1) with the unit as stored after it;
    - duration: "Time spent", a `Stepper` of whole minutes (from 0, step 5) with "minutes" after it, also for a goal
      in hours.

    The Stepper's input has the label as its accessible name.
  - **Text around the field**:
    - the target in the sub line;
    - the progress under the field (`ht-target` with an `ht-progress` bar filled to `RecordingText.ProgressFill`),
      live: `RecordingText.Progress` of `RecordPreview.Progress` for the value being entered ("12 of 20 min", "1 of
      at most 2 cups", "2 of 3 times"); none for a yes/no commitment, and none for a count or duration commitment
      without a value;
    - the partial-progress sentence of a Build count or duration goal (`RecordingText.PartialNote`), as
      `design/components/RecordDialog.html` words it ("Partial time is kept. Below 20 min the day counts as Failed
      when it is finalized.");
    - for a Reduce goal, the help text (`RecordingText.ReduceHelp`): "At most 2 cups. Enter 0 if you had none: an
      empty day cannot prove you stayed under the limit." for count and duration, and "Choose No if you avoided it:
      an empty day cannot prove you stayed under the limit." for yes/no (FR-013);
    - "Note (optional)", empty, with the placeholder "What happened? Only you can see notes." and the help "An empty
      field keeps the existing note.".
  - **Buttons**: "Save record" and "Cancel". "Edit" opens the same dialog for one record, with its date fixed and
    its value filled, and adds "Delete record". There is no "Excuse instead" (slice 7).
  - **Refusals**: the result's `Message`, under the field it is about, one at a time: a value refusal under the
    field, "The note can have at most 500 characters." under the note, and the date, limit and record refusals under
    "For".
  - **Focus**: on open it moves to the first field: "For" from the head; from a row, the week's "Date" select or else
    the value (the chosen Yes or No, or Yes while none is chosen; the Stepper's number). Esc and × close the dialog,
    and focus returns to the opener (constitution IV).
- **NoteDialog** (HP-003):
  - opened from a row's note or "Add note";
  - its title is the design's "Add note · {goal name}", with or without a note (the design has no other title);
  - its sub line is the date and the state ("Tue 27 Oct · Missed");
  - a TextArea "Note" of at most 500 characters, with the design's help "Notes are private. They do not change the
    result.". Opened from a row's note it starts with that note, so saving without a change keeps it; from "Add
    note" it starts empty. The limit is the use case's (there is no `maxlength`, so a note trimmed to 500 characters
    is never cut short): a longer note gives "The note can have at most 500 characters." under the field, in place
    of the help, and changes nothing;
  - buttons: "Save note" and "Cancel". "Save note" calls `SaveNoteAsync` with the row's first date (a week's
    Monday, which names the week). Saving it empty removes the note;
  - **Focus**: on open it moves to the TextArea (`TextArea.FocusAsync`); Esc and × close the dialog, and focus
    returns to the row's Note button (constitution IV).

## StatusChip (HP-004)

| State | Text | Classes |
|---|---|---|
| Met | Met | `ht-chip ht-status ht-status--met` |
| Failed | Failed | `ht-chip ht-status ht-status--failed` |
| Missed | Missed | `ht-chip ht-status ht-status--missed` |
| Pending | Pending | `ht-chip ht-status ht-status--pending` |
| Due, a commitment or a per-day period | Due today | `ht-chip ht-status ht-status--due` |
| Due, a per-week period | Due this week | `ht-chip ht-status ht-status--due` |

These are the classes of `design/components/StatusChip.html`, and no new class is added. The text is always shown,
so color is never the only signal. Upcoming has no chip on these pages. A Reduce goal's Met is "Met".

## New goal (HP-005)

"How do you measure it?" gains the help line "You can record partial amounts; the target decides the result.".

## Operator log (category `HT.Ui.Operator.StartupTasks`)

The lines of slices 1 to 4 are unchanged in text and in order. After demo loading's lines, demo history loading
adds (HP-007):

- for a refusal: `Demo history loading: {Result}`;
- for `Loaded`:
  1. `Demo account {Email}: {Result}`, with `demo4@example.com`;
  2. only for a created account: `Demo goals {Email}: {Result}`, then `Demo records {Email}: {Result}` (new).
- for a fault: `Demo history loading failed; the app starts without it` (Error, with the exception and no secret
  value). The start goes on.

No line contains a note, a recorded value, a unit, a goal name or a habit name (HP-006).

## Smoke check (`tools/HT.SmokeCheck`, slice 3 contract extended)

Check 6, `demo history demo4@example.com`, runs after check 5 (research R-10). It confirms SC-005 in full and
includes HP-008's list. It has 11 expected texts, all taken from `HT.Application` (`DemoHistory`, `RecordingText`):

1. sign in on a new client with the demo password;
2. `GET /goals` shows the 3 names of `DemoHistory.GoalNames`;
3. the History card of "Reading, 20 min daily" contains "Met", "Failed", "Pending", "Due today", "12 min" and "Fell
   asleep early.";
4. the History card of "Yoga, 3 times a week" contains "Failed" and "Missed".

The goal page links come from the new `PageReader.ReadGoalLinks` (the `a.ht-goalitem__name` anchors). The texts are
matched only in the text of the History card:

- the new `PageReader.ReadCardText(html, "History")` finds the first `section.ht-card` whose `h2.ht-card__title`
  is "History";
- it takes the text up to that section's `</section>`, with tags removed and entities decoded;
- each expected text must be an ordinal substring of it.

Output, in check 5's form:

- `PASS demo history demo4@example.com (11 of 11)`;
- or `FAIL demo history demo4@example.com: <reason>`, where the reason is one of:
  - `the sign-in was refused`, as check 5 words a refused sign-in (added in T090: step 1 can fail too, for
    example when demo history loading refused the demo password);
  - `GET /goals gave <status>`;
  - `GET goal page <n> gave <status>`;
  - `no link for goal page <n>`;
  - `<found> of <expected> goal names shown`;
  - `<found> of <expected> texts on goal page <n> shown`.

  n is the position in `GoalNames`.

No goal name, note, amount, goal path or other page content is ever printed (slice 3 smoke-check contract).

`CoverageEntry` gains the port type as its first member. (Corrected in T090: `CoverageEntry` is a top-level record
in `SmokeCheckCoverage.cs`, not a type nested in `SmokeCheckCoverage`; it is now
`CoverageEntry(Type Port, string Method, string Parameter, Type Type, string Check)`.) `SmokeCheckCoverage.Entries`
covers both demo ports. The
coverage table in `specs/003-demo-hosting/contracts/smoke-check.md` ("Coverage rule") gains the same rows:

| Port | Method | Parameter | Type | Check |
|---|---|---|---|---|
| `IDemoHistoryStore` | `CreateDemoHistoryAccountAsync` | `email` | `Email` | 6 demo history: the sign-in |
| `IDemoHistoryStore` | `CreateDemoHistoryAccountAsync` | `password` | `string` | 6 demo history: the sign-in |
| `IDemoHistoryStore` | `CreateDemoHistoryAccountAsync` | `profile` | `DemoAccountProfile` | 6 demo history: the sign-in |
| `IDemoHistoryStore` | `CreateDemoHistoryAccountAsync` | `goals` | `IReadOnlyList<DemoHistoryGoal>` | 6 demo history: the goal names, and the states, partial amount and note on the goal pages |

## Hosting workflow (`.github/workflows/deploy-demo.yml`, slice 3 contract extended; HP-009)

- **Triggers**: `workflow_dispatch`, unchanged, and `schedule` at `cron: '20 0 * * *'` (00:20 UTC, daily).
- **New first job `gate`**: no `environment`, no Azure sign-in, no secret. Its step "0.1 Check the trigger and
  the demo end date" has two outputs:
  - `mode`: `inputs.mode`, or `refresh` on a scheduled run;
  - `run`: `false` on a scheduled run whose UTC date is after 2027-01-31, otherwise `true`. A manual run is never
    skipped.

  With `run` false it writes the summary "Demo refresh: skipped after 2027-01-31".
- **Job `demo`**: `needs: gate` and `if: needs.gate.outputs.run == 'true'`, so a skipped run never enters the
  `demo` environment (research R-11). Its job-level `env.MODE` comes from `needs.gate.outputs.mode`, and every
  step reads `env.MODE` instead of `inputs.mode`. Its name becomes `Demo (${{ needs.gate.outputs.mode }})`, so a
  scheduled run reads "Demo (refresh)". The run succeeds with `demo` skipped.
- **Unchanged**: the concurrency group `deploy-demo`, the `demo` environment and its master-only branch policy,
  the refresh steps and the smoke check, which is the success check (slice 3 HP-017), now with check 6.
- **Documents updated in the same change**:
  - `specs/003-demo-hosting/contracts/deploy-workflow.md`: the trigger, the `gate` job and the mode;
  - `docs/operations/hosted-demo.md`:
    - the daily refresh, what a late or failed run leaves, and how to start a refresh by hand;
    - GitHub's 60-day rule for scheduled workflows: the repository needs activity on or after 2026-12-02. A turned
      off workflow comes back only by hand, with "Enable workflow" in the Actions tab or `gh workflow enable
      deploy-demo.yml`, not with a push (research R-11);
    - "Teardown after 2027-01-31", which removes the `schedule` trigger.
