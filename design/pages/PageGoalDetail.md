# PageGoalDetail

The goal page: everything about one attempt of one goal — stats, trend, history with corrections, settings, pauses, notes and earlier attempts.

- Page head: BackLink "← Goals", `heading-1` goal name, tags (category, Build or Reduce, Commitment-based or Period-based), attempt line. Actions right: ghost Edit settings, secondary Pause, ghost End goal, primary Record.
- Left column: GoalStats (consistency + streaks + strip), TrendChart, History DataTable (date, recorded amount, StatusChip, note, action). Missed and finalized rows can be corrected ("Add record", "Edit").
- Right column: Settings (KeyValue) with "changes apply from the next commitment", Pauses, Recent notes, Earlier attempts (each with its own consistency and outcome).
- Pause and End goal open a Dialog that explains the effect; End goal asks for the outcome (Achieved, Abandoned, No longer relevant) and an optional note.

## Slice 4 state

The second part of the preview shows the page as slice 4 builds it (HP-004, ADR 0008). Today is Tue 17 Nov 2026 in the preview.

- `/goals/{id}` for the goal's owner; another account's goal, an unknown goal and any goal address for an administrator show the Not found page.
- "← Goals", the goal's name as `heading-1`, then the tags: category, Build or Reduce, and "Commitment-based" or "Period-based", and the attempt line: "Attempt 1 · started 14 Jul" once the goal has started, "Attempt 1 · starts 25 Nov" before.
- A goal on a catalog habit that is no longer in the catalog: the neutral Notice "This habit is no longer in the public catalog. Your goal keeps working." under the page head. The goal list shows nothing extra.
- The Settings card (KeyValue), in this order:
  - Habit: the habit's current name, " · " and "catalog" or "custom" ("Yoga · catalog");
  - Build or reduce: "Build" or "Reduce";
  - Measure: "Yes / no", "Count" or "Duration";
  - Target: the goal's target text ("3 days a week", "15 min, 5 days a week", "at most 2 cups a day", "avoided every day");
  - Schedule: the schedule as on the Goals page ("Mon · Wed · Fri", "Every day", "Flexible · 3 times a week");
  - Evaluated: "Each scheduled day", "Each day" or "Each week", as on the New goal summary card;
  - First week, for a per-week goal only: the date its first week starts ("30 Nov 2026");
  - Started: the start date ("14 Jul 2026"); the row is "Starts" while the goal has not started.
- Under the list: "Changes apply from the next commitment. Past results keep their rules."
- No actions, GoalStats, trend, history, pauses, notes or earlier attempts (slices 5 to 8).
- The full preview's texts follow slice 4 too: the "Build or reduce" row (not "Intent"), the Target row, "Each scheduled day" (not "Each commitment") and the streak unit "commitments" for a commitment-based goal (not "sessions").

## Slice 5 state

The last part of the preview shows the page as slice 5 builds it (HP-001, US3-AS8, US4-AS7, US5-AS5, ADR 0008). Today is Tue 17 Nov 2026 in the first two examples and Tue 18 Aug 2026 in the per-week one.

Amended 2026-11-20, specs/006-note-editing, ADR 0019 (note editing, HP-001 and HP-002 there): the Note cell shows the note as plain text, and a row's note control ("Add note" or "Edit note") is in the actions column, before "Add record", on rows without a record only, so the record control is always the rightmost. A row with a record changes its note only in the RecordDialog, which opens with the note filled in.

That order is the owner's decision after implementation (2026-11-20). The first version of this amendment put the note control after "Add record", because recording is the row's main action.

- Page head: slice 4's "← Goals", name, tags and attempt line, then `ht-spacer` and the primary action "Record" (`aria-haspopup="dialog"`), which opens the RecordDialog with its "For" select (`components/RecordDialog.md`, "Slice 5 state"). No other action (Edit settings, Pause and End goal are slices 7 and 8). "Record" is absent when nothing can be recorded now (no Due and no Pending commitment or period).
- Under the head: slice 4's Notice for a habit no longer in the catalog, unchanged.
- Two columns (`ht-grid-2`), as in the full preview:
  - left: the History card;
  - right: slice 4's Settings card, unchanged, then the Recent notes card.
- History card (`ht-card`):
  - the card head (`ht-card__head`): the title "History", `ht-spacer`, and the muted sentence "Corrections are evaluated with the rules of that date." (`ht-card__sub`) beside it;
  - then a DataTable (`components/DataTable`, `aria-label="History"`) with the columns Date, Recorded, Result, Note and an empty-headed actions column (`td.ht-actions`);
  - the rows: the commitments or periods that have begun in the last 366 days up to the goal's current date, newest first. Upcoming ones are not listed;
  - the first 10 rows, then the ghost button "Show all N →" (`aria-expanded="false"`; N counts all the listed rows), which shows the rest in place.
- The cells:
  - Date: "Tue 17 Nov" for a commitment or a day; "10 – 16 Aug" for a week, or "27 Jul – 2 Aug" across a month end;
  - Recorded: "—" without a record. A commitment shows its record as text ("25 min", "Yes", "3 cups"; a count with its unit as stored, "1 sweet", "2 sweet"). A period shows each of its records as its own ghost button with its value, side by side (`ht-inline`); each opens the RecordDialog's "Edit" state for that record;
  - Result: the StatusChip of the row's state (`components/StatusChip.md`, "Slice 5 state");
  - Note: the row's note as plain text, or "—" without one, in a plain `td` with `ht-muted` text (`--ink-muted`, 5.5:1 or more). It is not a control;
  - actions (`td.ht-actions`), in this order:

    | Row | Controls |
    |---|---|
    | a commitment without a record | "Add note" (no note) or "Edit note" (a note), then "Add record" |
    | a commitment with a record | "Edit" (it opens that one record) |
    | a period without a record | "Add note" or "Edit note", then "Add record" |
    | a period with 1 to 49 records | "Add record" |
    | a period with 50 records | none |

    The record control ("Add record" or "Edit") is always the rightmost, the last button of the row, so it stays in the same place whether or not a note control comes before it. "Add note" and "Edit note" open the NoteDialog (`components/NoteDialog.md`, "Slice 5 state"); the other controls open the RecordDialog, which shows the row's note in its note field (`components/RecordDialog.md`, "Slice 5 state"). In the slice-5 state the Due row's action is "Add record" too; the full preview keeps its primary "Record".
- The row actions are offered whatever the state, final rows included, so a Missed or Failed commitment or period takes a record as a correction (US3-AS4). All row controls are ghost `ht-btn--sm` buttons with `aria-haspopup="dialog"`.
- A row's note control follows its records on the next read: it is gone after the row's first record, and back after its last record is deleted ("Edit note" when the row still has its note). After the NoteDialog closes, the focus goes to the row's "Add note" or "Edit note" as it shows after the save.
- The preview's rows show each case: "Add note" on rows without a record or a note (Mon 16 Nov, Pending; Sun 8 Nov, Missed), "Edit note" on a Missed row with a note (Sat 14 Nov, "Away for the weekend."), and a note as text only on rows with a record (Thu 12 Nov, "Fell asleep early."; the per-day Sun 15 Nov; the week 27 Jul – 2 Aug).
- Accessible names of the repeated row controls. Each starts with its visible text and names its row ({date} is "Tue 17 Nov", or the week's "10 – 16 Aug"):

  | Control | Accessible name |
  |---|---|
  | a period's record button | "12 min, edit record 1 of Tue 17 Nov" (the value, then the number of the record in the row, counted from 1) |
  | "Add record" | "Add record for Tue 17 Nov" |
  | "Edit" | "Edit record of Tue 17 Nov" |
  | "Add note" | "Add note for Tue 17 Nov" |
  | "Edit note" | "Edit note of Sat 14 Nov" |

- Recent notes card (right column, after Settings): the title "Recent notes", then at most 5 notes, newest first, each an `ht-goal` item: the note in quotes and its StatusChip on the top line (`ht-goal__top`, `ht-spacer` before the chip), and its date under it (`ht-goal__meta`: "Thu 12 Nov", or "27 Jul – 2 Aug" for a week). The card is absent while the goal has no note.
- A goal with no begun entry (not started yet, or a current date with no commitment or period yet, such as a per-week goal's days before its first week) shows no "Record", no History card and no Recent notes card: only slice 4's content.
- No GoalStats, strip, trend, pauses or earlier attempts (slices 6 to 8).

## Slice 6 state

The last part of the preview shows the page as slice 6 builds it (HP-001, US1-AS8, US2-AS8, US4-AS8, ADR 0008). Today is Tue 17 Nov 2026; the goal "Reading, 20 min daily" started on Tue 20 Oct.

- Everything of slice 5 stays as it is: the page head with "Record", the History card, the Settings card and the Recent notes card (the "Slice 5 state" above).
- The left column gets two cards above the History card, in this order:
  1. the GoalStats card (`components/GoalStats.md`, "Slice 6 state"): the eyebrow "This attempt", the title "Consistency and streaks" (`h2.ht-card__title`), then three stats:
     - "Consistency" with the consistency as one whole text, "92% (24 of 26)" or "N/A", in `ht-stat__value ht-stat__value--text`;
     - "Streak, commitments" with the current streak, "7";
     - "Longest, commitments" with the longest streak, "9".

     The labels name the goal's unit in its plural form: "commitments", "days" or "weeks". Then the ConsistencyStrip of the last 30 ended results, oldest first, with its legend: Met, Failed, Missed and Pending only (`components/ConsistencyStrip.md`, "Slice 6 state"). Its `aria-label` counts each state: "Last 28 commitments: 24 met, 1 failed, 1 missed, 2 pending".
  2. the TrendChart card (`components/TrendChart.md`, "Slice 6 state"), a `section.ht-card` with an `h2` title: the title and sub line of the goal's kind ("Minutes per day", "Last 14 days"), the bars of FR-006, an empty bar (`ht-bar--empty`) for a commitment, day or week with no record (Sat 7 Nov and Mon 16 Nov here), the dashed target line "Target 20 min" (minutes) or "Target {n}", and the date labels. Its `aria-label` names the bars and the target: "Minutes per day, last 14 days; target 20".
- The figures cover the whole current attempt, from its start date to today; the strip and the trend show its latest results and bars.
- Each value renders whole, as one `ReportText` text (tasks, Conventions): "92% (24 of 26)" is not split into "92%" and a `<small>` count. The full preview above keeps its split value for later slices.
- After a record, a change, a delete or a note is saved in a dialog, the page reads the figures again, so the cards follow the correction.
- A goal with no begun entry (not started yet, or a per-week goal's days before its first week) keeps the slice 5 page without these two cards: no "Record", no History, no Recent notes, no GoalStats and no TrendChart (the last example of the "Slice 5 state").
- No Excused or Paused cell, no paused bar, no Pauses card, no earlier attempts, no Edit settings, Pause or End goal (slices 7 and 8).
- The full preview's trend title "Days done per week" (a commitment-based yes/no goal) replaces "Sessions per week", as the TrendChart's title table gives it.

## Slice 7 state

The last part of the preview shows the page as slice 7 builds it (HP-001, US1, US2-AS14, US3-AS8, contracts/host.md "Goal page", ADR 0008). Three examples: "Meditation, 10 min daily" (spec US3) on Thu 19 Nov 2026, 12:00, not paused, with an upcoming and a past pause; the History card of "Yoga, 3 days a week" (spec US1) on Fri 13 Nov 2026, 07:00, with two Excused rows; and "Drinking water, 8 glasses a day" (specs US2 and US4) on Mon 16 Nov 2026, 10:00, paused from that day, with a record kept on the paused day.

Changed 2026-11-21 in the full preview, whose texts disagreed with the spec: the Pauses card listed its pauses oldest first as "10 – 16 Aug" and "21 Sep – 4 Oct", each with a "Paused" StatusChip; it now lists them latest first as "Mon 21 Sep – Sun 4 Oct" and "Mon 10 Aug – Sun 16 Aug", with no chip (FR-012, HP-001; a StatusChip is for evaluation states, `components/StatusChip.md`).

- Everything of slice 6 stays: the page head with "Record", the GoalStats, TrendChart, History, Settings and Recent notes cards (the "Slice 6 state" above). The additions:
- **Page head**: the secondary action "Pause" (`ht-btn--secondary`, `aria-haspopup="dialog"`), before "Record", both in an `ht-inline` group after `ht-spacer`. It opens the PauseDialog (`components/PauseDialog.md`, "Slice 7 state"). It is absent while the goal is paused (a pause is current). "Record" keeps slice 5's rule: a paused goal can still have Pending entries from before its pause.
- **Notice**, directly under the page head, while the goal is paused: `ht-notice` (the invitation style, which may hold one button) with "Paused until Sun 22 Nov" (`ReportText.PausedUntil`), a separator "·" (`aria-hidden="true"`) and the button "Resume now" (`ht-btn--primary ht-btn--sm`), which resumes the goal (FR-010). A refusal ("This goal is not paused.") is shown in the Notice's place, as an `ht-field__error` paragraph with `role="alert"` directly under the page head (settled here, T064). While the goal is paused and something can still be recorded, the head keeps slice 5's markup, `ht-spacer` then "Record" alone; with "Pause" shown, both buttons sit in the `ht-inline` group (settled here, T064). After "Resume now" (Notice or Pauses card) or "Cancel pause", and after their refusals, the page reads everything again and the focus goes to "Pause", or, while a pause is still current, to the Notice's "Resume now" (settled here, T064). When the goal also has slice 4's Notice "This habit is no longer in the public catalog. Your goal keeps working.", the paused Notice comes first and the catalog Notice second (settled here, T003; `css/ht-components.css` spaces the second one).
- **Pauses card**, in the right column after Settings (before Recent notes); absent when the goal has no pause:
  - the title "Pauses" (`h2.ht-card__title`);
  - one `ht-goal` item per pause (FR-012), the latest first day first: on its top line (`ht-goal__top`) the range "Mon 16 Nov – Sun 22 Nov" (`PauseView.RangeText`, `ht-goal__name`), then `ht-spacer` and its action; under it, when it has one, the reason (`ht-goal__meta ht-muted`, "Family visit.");
  - the action: "Resume now" on the current pause, "Cancel pause" on an upcoming one (FR-011), none on a past one. Both are ghost `ht-btn--sm` buttons without `aria-haspopup` (they act at once), with the accessible names "Resume now, {range}" and "Cancel pause, {range}" ("Cancel pause, Mon 23 Nov – Sun 29 Nov"; settled here, T003). No timing word or chip: the action tells a current or upcoming pause from a past one;
  - a refusal of "Resume now" or "Cancel pause" shows as an `ht-field__error` paragraph with `role="alert"` under the card title (settled here, T003).
- **History card**, the rows of slice 5 and 6, with:
  - the Result cell: the StatusChips "Excused" and "Paused" (`components/StatusChip.md`, "Slice 7 state"); an Excused row shows its reason after the chip (`span.ht-muted` in an `ht-inline` group with the chip: "Excused  Ill"); with no reason, the chip alone;
  - the actions column (`td.ht-actions`), in this order:

    | Row | Controls |
    |---|---|
    | an Excused commitment whose grace period is open | "Add note" (no note) or "Edit note" (a note), then "Remove excusal" |
    | an Excused commitment whose grace period has ended | "Add note" or "Edit note" |
    | a Paused commitment or period without a record | "Add note" or "Edit note" |
    | a Paused commitment with a record | "Edit" (it opens that one record) |
    | a Paused period with records | none: each record stays its own button in the Recorded cell, which opens it in the RecordDialog |

    No "Add record" on an Excused or Paused row (FR-005). An Excused commitment never has a record (D-3). A record kept on a Paused row keeps its edit control (FR-009), and its note is changed in the RecordDialog, as on any row with a record (slice 5). "Remove excusal" takes the place of the record control, as the rightmost control (settled here, T003).
  - "Remove excusal" is a ghost `ht-btn--sm` button without `aria-haspopup`: it removes the excusal at once (FR-004), and the page reads everything again. Its accessible name is "Remove excusal of {date}" ("Remove excusal of Wed 11 Nov"). After it, the focus goes to the row's "Add record", or, when the row has none (a pause covers it), to its note control (settled here, T003). A refusal ("This excusal can no longer be removed.") shows as an `ht-field__error` paragraph with `role="alert"` under the card head.
  - The Note cell, the dates and the other rows are slice 5's. The preview's first example shows Paused commitments without a record (Fri 13 to Sun 15 Nov, "Add note" only); its Excused Mon 9 Nov is the eleventh row, behind "Show all 18 →". The second shows Excused rows with and without a reason and with and without a note. The third shows a Paused day with a kept record ("8 glasses", no action) and the Pending days before the pause.
- **GoalStats**: the strip's Excused and Paused cells and the legend Met, Failed, Missed, Excused, Paused, Pending (`components/GoalStats.md`, "Slice 7 state"); the figures leave both out ("90% (10 of 11)", streak 2, longest 8).
- **TrendChart**: marked bars (`components/TrendChart.md`, "Slice 7 state"): Mon 9 Nov Excused, Fri 13 to Sun 15 Nov Paused, with "{date}: Excused" or "{date}: Paused" titles and "; 1 excused, 3 paused" in the chart's `aria-label`.
- **Recent notes**: unchanged; a note of an Excused or Paused commitment or period carries the "Excused" or "Paused" chip (FR-020).
- After any write in a dialog or card (pause, resume, cancel a pause, excuse, remove an excusal, record, note), the page reads everything again, so the Notice, the head's "Pause", the cards and the figures agree.
- Still no Edit settings, End goal or earlier attempts (slice 8). A goal with no begun entry keeps the slice 5 page, now with "Pause" in its head (a goal that has not started can be paused from the account's today, FR-006) and the Pauses card when it has a pause (settled here, T003).
- Every text comes from `ReportText`, `ExcusalTexts` and `PauseTexts` (HP-008); the page builds none.

## Slice 8 state

The last part of the preview shows the page as slice 8 builds it (HP-001, US1-AS15, US2-AS13, US3-AS14, US3-AS15, US4-AS8, contracts/host.md "Goal page", ADR 0008). Six examples: "Reading, 15 min daily" (spec US1) on Wed 11 Nov 2026, 10:00, right after its change to 20 minutes; "Meditation, 10 min daily" (spec US2) on Mon 16 Nov 2026, 10:01, just ended with "Abandoned"; "Sugar, at most 1 sweet a day" (spec US4) on Mon 16 Nov 2026, 12:00, restarted that day; "Meditation, 5 min daily" (spec US3-AS4) on Thu 19 Nov 2026, whose attempt 2 starts on Mon 23 Nov; the goal of spec US2-AS12, ended before it began, on Tue 17 Nov 2026; and its Earlier attempts card after a restart.

Changed 2026-11-22 in the full preview, whose texts disagreed with the spec: the Earlier attempts card showed "Attempt 1" with the meta "2 Mar – 30 Jun 2026 · Abandoned"; it now shows "Attempt 1 · 2 Mar – 30 Jun" as a link, the outcome Tag "Abandoned" and the consistency (HP-001).

- Everything of slice 7 stays: the page head with "Pause" and "Record", the paused Notice, the GoalStats, TrendChart, History, Settings, Pauses and Recent notes cards (the "Slice 7 state" above). The additions:
- **Page head of a goal that is not ended**, the actions in one `ht-inline` group after `ht-spacer`, in this order:
  1. the ghost "Edit settings" (`a.ht-btn.ht-btn--ghost`, a link to `/goals/{id}/edit`, `pages/PageEditGoalSettings.md`);
  2. slice 7's secondary "Pause" (absent while the goal is paused);
  3. the ghost "End goal" (`button.ht-btn.ht-btn--ghost`, `aria-haspopup="dialog"`), which opens the EndGoalDialog (`components/EndGoalDialog.md`, "Slice 8 state");
  4. slice 5's primary "Record" (absent when nothing can be recorded now).

  "Edit settings" and "End goal" are there whatever the goal's state, paused or not started included (HP-001 names no condition but "not ended"; settled here, T003).
- **Attempt line** (`span.ht-attempt`, after the tags), for the latest attempt (`GoalDetails.AttemptHasBegun` and `AttemptStartDate`, never `HasStarted` or a clock):
  - "Attempt 2 · started 16 Nov" once the latest attempt has begun;
  - "Attempt 2 · starts 23 Nov" while it has not;
  - "Attempt 1 · ended before it began" for an ended latest attempt that never began (it has no date; plan "Spec discrepancies" 5).
- **Settings card**, slice 4's rows with two changes:
  - when a later settings version exists (`GoalDetails.Later`), the row "From Thu 12 Nov" (`dt`, `ReportText`) with that version's target text ("20 min daily", `dd`), placed directly after the Target row, so the two targets read together (settled here, T003). The other rows show the settings in effect;
  - the start row shows the latest attempt's date: "Started" ("16 Nov 2026") once it has begun, "Starts" ("23 Nov 2026") while it has not. An ended goal whose latest attempt never began has no start row, so the card never announces a start under the Ended Notice.

  Slice 4's sentence under the list, "Changes apply from the next commitment. Past results keep their rules.", stays as it is on a goal that is not ended. An ended goal cannot be changed, so its card has no sentence (owner decision on PR 45).
- **Ended goal** (`GoalDetails` with an ending):
  - no actions in the head: no "Record", "Edit settings", "Pause" or "End goal", and no `ht-spacer`;
  - directly under the head, the Notice (`ht-notice`, the invitation style, which may hold one button) with "Ended Mon 16 Nov · Abandoned" (`ReportText`), a separator "·" (`aria-hidden="true"`) and the action "Start new attempt" (`a.ht-btn.ht-btn--primary.ht-btn--sm`, a link to `/goals/{id}/new-attempt`, `pages/PageNewAttempt.md`), as slice 7's paused Notice holds "Resume now";
  - the GoalStats, TrendChart, History, Settings and Recent notes cards of the ended attempt stay. The History card keeps its record and note actions for the ended attempt's dates (FR-010): its Pending rows can still be recorded, and every row can be corrected. Excused rows keep "Remove excusal" while their grace period is open;
  - an ended goal is never paused, so it has no paused Notice and no "Resume now" (its past pauses stay in the Pauses card, with no action);
  - after a successful ending in the EndGoalDialog, the page reads everything again and the focus goes to the Notice's "Start new attempt" (HP-009).
- **Earlier attempts card** (`EarlierAttemptsCard`), in the right column after Recent notes (after Settings and Pauses when there are no notes). Absent when the goal has only one attempt:
  - the title "Earlier attempts" (`h2.ht-card__title`);
  - one `ht-goal` item per ended attempt that is not the latest, latest first (`AttemptService.GetAttemptsAsync`):
    - on its top line (`ht-goal__top`): "Attempt 1 · 2 Nov – 13 Nov" (start date to last date), or "Attempt 1 · ended before it began" for an attempt with no date, as a link to `/goals/{id}/attempts/{n}` (`a.ht-goal__name`, `pages/PageAttemptDetail.md`); then the outcome Tag (`ht-chip`, "Achieved", always with its word); then the consistency as one whole text (`ht-goal__pct`, "100% (12 of 12)", or "N/A");
    - under it (`ht-goal__meta`), the attempt's name (FR-004, "Meditation, 10 min daily"), only when it differs from the goal's current name;
    - then the ending note in `ht-goal__meta ht-muted`, only when it has one.

    The order of the parts is settled here (T003): the link, the Tag and the figure on one line, as the Home's Active goals card puts a goal's name and figure, and the texts under it.
- **History card and dialogs**: each row's Recorded cell, state words and record actions use that row's own settings (`History.SettingsOf(entry)`), and so do the RecordDialog and the NoteDialog fixed to a row: after a change of measurement, 11 Nov shows "12 min" and 12 Nov "12 pages" (AC-020). The layout is slice 5's; only the texts follow each row.
- A restarted goal whose latest attempt has not begun has no begun entry of that attempt: no "Record", GoalStats, TrendChart, History or Recent notes card (slice 5's rule; the goal page covers only the latest attempt's dates, FR-016), only the head with "Edit settings", "Pause" and "End goal", the Settings card with "Starts", and the Earlier attempts card. The earlier attempt's dates are recorded from its attempt view (the fourth example).
- An ended goal that never began: the attempt line "Attempt 1 · ended before it began", the Ended Notice with "Start new attempt", and the Settings card without its start row (the fifth example). It has no begun entry, so no other card.
- After any write (a change on the Edit settings page, an ending, a new attempt, and slice 7's writes), the page reads everything again, so the head, the Notice, the cards and the figures agree.
- Every text comes from `ReportText`, `GoalChangeTexts`, `ExcusalTexts` and `PauseTexts` (HP-008); the page builds none.
