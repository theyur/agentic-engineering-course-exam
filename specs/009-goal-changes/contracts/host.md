# Contract: Host, pages, operator log and smoke check of goal changes

What `HT.Ui` and `tools/HT.SmokeCheck` add for this slice (HP-001 to HP-014). These are verified by bUnit, host and
smoke-check tests, not by acceptance tests. Texts come from `ReportText` and `GoalChangeTexts` (HP-008). Dates are
written as the shared formatter writes them ("Thu 12 Nov", "2 Nov"). `design/` changes first, and each page follows
its preview (constitution IV, ADR 0008).

## Routes

| Route | Page | Access |
|---|---|---|
| `/goals/{id}/edit` | Edit settings (new) | Registered users. Another account's goal or an unknown goal shows Not found. An ended goal redirects to `/goals/{id}`. |
| `/goals/{id}/new-attempt` | Start new attempt (new) | Registered users. Another account's or an unknown goal shows Not found. A goal that is not ended redirects to `/goals/{id}`. |
| `/goals/{id}/attempts/{n}` | Attempt view (new) | Registered users. The latest attempt's number, an unknown number, another account's goal or an unknown goal shows Not found. |
| `/goals?tab=ended` | Goals, Ended tab | as `/goals` |
| `/profile?tab=history` | Profile, Goal history tab | Registered users. An administrator's Profile has no Tabs and ignores `tab`. |

The pages call the use cases with the signed-in acting user. Hiding a control is never the protection (constitution
III).

## Goal page (`design/pages/PageGoalDetail`, slice-8 state; HP-001)

- **Head, goal not ended**: the ghost "Edit settings" (first, links to `/goals/{id}/edit`), "Pause" (slice 7), and the
  ghost "End goal" (`aria-haspopup="dialog"`, opens EndGoalDialog).
- **Attempt line** under the title: "Attempt {n} · started {18 Nov}", or "· starts {23 Nov}" while the latest attempt
  has not begun. The page chooses between them from `GoalDetails.AttemptHasBegun`, not `HasStarted` and not a clock.
  An ended latest attempt that has not begun (it has no date) shows "Attempt {n} · ended before it began" (research
  R-02; plan "Spec discrepancies" 5).
- **History card and dialogs**: each row's Recorded cell, state words and record actions use that row's settings
  (`History.SettingsOf(entry)`). The RecordDialog and the NoteDialog use the settings of the row they are fixed to,
  and the RecordDialog's chooser uses those of the chosen row. They take from them the sub line
  (`RecordingText.TargetLine`), the field label, unit, progress line, partial note, Reduce help, note help, preview
  and commitment rule, all through the `EntrySettings` overloads, never from the goal-level `GoalHistory`. So after
  a change of measurement, 11 Nov shows "12 min" and 12 Nov "12 pages" (AC-020).
- **Settings card** (`GoalSettingsCard`): the settings in effect, and when `GoalDetails.Later` is set, the row "From
  {Thu 12 Nov}" with the later target text. Its start row shows the latest attempt's date, as the design does:
  "Started" or "Starts" from `AttemptHasBegun` with `AttemptStartDate`, not `HasStarted` with `StartDate`. An ended
  goal whose latest attempt has not begun has no start row, so the card never announces a start under the Ended
  Notice (AC-035's goal).
- **Ended goal**:
  - under the head, the Notice "Ended {Mon 16 Nov} · {Abandoned}" with the action "Start new attempt" (links to
    `/goals/{id}/new-attempt`);
  - no "Record", "Edit settings", "Pause" or "End goal";
  - the History card keeps its record and note actions for the ended attempt's dates (FR-010).
- **Earlier attempts card** (`EarlierAttemptsCard`), in the right column after Recent notes. For each ended attempt
  that is not the latest, latest first:
  - "Attempt {n} · {2 Nov} – {15 Nov}", or "Attempt {n} · ended before it began" for an attempt with no date, as a link
    to `/goals/{id}/attempts/{n}`;
  - its name, when it differs from the goal's current name;
  - its outcome `Tag`;
  - its consistency;
  - its note (`ht-muted`) when it has one.

  The card is absent when there is none. Its data comes from `AttemptService.GetAttemptsAsync`.

## Edit settings page (new design page; HP-002)

- `NewGoalForm` in its edit mode:
  - Build or Reduce, measurement, target and schedule, and the name, filled with the latest settings;
  - no habit chooser and no start date;
  - the schedule choice limited to the goal's kind.
- The summary card of the New goal page, and above the actions the line "Changes apply from
  {GoalDetails.ChangesApplyFrom}. Past results keep their rules.".
- The primary "Save changes" and the secondary "Cancel" (back to the goal page).
- A refusal is shown under its field (`SettingsChangeResult.Field`), one at a time, with the result's message as it
  is. "settings changed" and "settings unchanged" both return to the goal page.

## EndGoalDialog (`design/components/EndGoalDialog`; HP-003, HP-009)

- The title "End goal · {goal name}" and the sub line with the attempt line.
- The effect first: "Ending stops new commitments. Your history stays and nothing is counted as failed. To try again
  later you start a new attempt.".
- "How did it end?": the ChoiceCards Achieved, Abandoned and No longer relevant, in one column, none chosen when the
  dialog opens. Then "Note (optional)" (`TextArea`).
- The primary "End goal" and "Cancel".
- Refusals show under their field: "Choose how the goal ended." under "How did it end?", and "The note can have at most
  500 characters." under the note.
- Built on `HtDialog`: the first field gets focus on open; Esc and × close it; focus returns to "End goal" in the head,
  or to the Notice's "Start new attempt" after a successful ending, because the opener is gone (HP-009).

## Start new attempt page (new design page or `PageNewGoal` state; HP-004)

- `NewGoalForm` in its attempt mode:
  - every schedule kind and the start date;
  - no habit chooser and no name;
  - filled with `NewAttemptValues`.
- The line "Attempt {n} starts with its own history and a streak from zero.".
- The primary "Start attempt" and the secondary "Cancel". "attempt started" opens the goal page.
- A refusal is shown under its field, one at a time, as it is.

## Attempt view (new design page; HP-014)

- "← {goal name}" back to the goal page.
- The heading "Attempt {n} · {2 Nov} – {15 Nov}", or "Attempt {n} · ended before it began" with an empty History.
- The attempt's name, the outcome `Tag`, and the note (`ht-muted`) when it has one.
- `GoalStats` with the eyebrow "Attempt {n}", its figures and strip.
- `HistoryCard` over `AttemptDetails.History`, with the record and note actions of the goal page. They open the
  RecordDialog or the NoteDialog fixed to their row, with that row's `Settings` (the attempt's own, never the latest
  attempt's).
- No "Record", "Edit settings", "Pause", "End goal", trend, Settings, Pauses or Recent notes card.

## Goals page (`design/pages/PageGoals`, slice-8 state; HP-005)

- The Tabs "Active", "Paused" and "Ended" (`?tab=ended`). Active and Paused leave ended goals out.
- The Ended tab lists ended goals in the overview's order. `GoalItem`'s ended form shows:
  - the outcome `Tag`;
  - the meta "Ended {Mon 16 Nov}";
  - the figures and strip (Pending results show as Pending);
  - the secondary "Start new attempt";
  - no "Record".
- An empty Ended tab says "No ended goals.".

## Profile (`design/pages/PageProfile`, slice-8 state; HP-006)

- The tab "Goal history" after "Account" (`?tab=history`). There is no Community tab (slice 9).
- A `DataTable` with the columns:
  - Goal: the attempt's name, as a link to `/goals/{id}/attempts/{n}`, or to `/goals/{id}` when `IsLatestAttempt`;
  - Attempt;
  - Ended ("Mon 16 Nov");
  - Outcome (`Tag`);
  - Note (`ht-muted`, or "—").

  The rows are in `GetGoalHistoryAsync`'s order.
- An empty table is replaced by "No ended goals yet.".
- An administrator's Profile keeps its Account content without Tabs.

## Signed-in Home (`design/pages/PageHomeSignedIn`, slice-8 state; HP-007)

- `TodayPanel` keeps the Pending rows of ended attempts, and the "Pending · still can record" tile counts them.
- `CommitmentRow` and the RecordDialog it opens use the row's settings (`Entry.Goal.SettingsOf(Entry)`) for the state words, the progress bar
  test, the field and the unit. An earlier attempt's row of another kind than the latest attempt is shown and recorded
  with its own settings (AC-080).
- `CommitmentRow` shows the `Tag` after the goal name:
  - "Attempt {n}" for a row whose attempt is not the goal's latest (`IsLatestAttempt` false);
  - otherwise "Ended" for a row of an ended goal (`TodayEntry.GoalEnded`).
- The other tiles, the Active goals card and recent notes leave ended goals out (FR-019). Nothing else changes.

## Shared texts (HP-008)

`ReportText` / `GoalChangeTexts` give:

- the outcome words;
- "Ended {date} · {outcome}" and "Ended {date}";
- "Attempt {n} · started {date}" and "Attempt {n} · starts {date}";
- "Attempt {n} · {first} – {last}" and "Attempt {n} · ended before it began";
- "From {date}";
- "Changes apply from {date}. Past results keep their rules.";
- "Attempt {n} starts with its own history and a streak from zero.";
- "No ended goals." and "No ended goals yet.".

Pages build none of them.

## Operator log (HP-011)

Startup step 7, after "Demo pause loading", logs:

- "Demo goal-change loading: no demo password configured" or "Demo goal-change loading: demo password does not meet
  the password rule" for a missing or weak password;
- otherwise the account lines through the existing `LogHistoryAccountLines`, as demo pause loading logs them:
  - "Demo account demo7@example.com: created" (or "already exists", with no further line);
  - "Demo goals demo7@example.com: demo goals added";
  - "Demo records demo7@example.com: demo records added".

The step runs through `RunStepAsync("Demo goal-change loading", …)`, so a fault logs "Demo goal-change loading failed;
the app starts without it" through the existing `LogStepFailed`, without secrets, and the start continues
(ADR 0007). `StartupLogTests` pins the lines after `demo6@example.com`'s.

## Smoke check (`tools/HT.SmokeCheck`; HP-012)

Check 9, "demo goal changes", signed in as `demo7@example.com` with the demo password:

1. `/goals?tab=ended` lists "Sugar, at most 1 sweet a day";
2. the goal page of "Meditation, 5 min daily" shows "Attempt 2";
3. the goal page of "Reading, 20 min daily" shows the Settings card's "From …" row with "25 min daily";
4. `/profile?tab=history` shows "Goal history" with "Abandoned".

Each data parameter of `IDemoGoalChangeStore` gets its `SmokeCheckCoverage` entry. The coverage table and check 9 are
added to `specs/003-demo-hosting/contracts/smoke-check.md`.

## Logs and errors (HP-010)

No log line, exception message or error page contains:

- an ending note, a note or a recorded value;
- a goal name, a habit name or a unit.

Outcomes are not logged either. Log capture tests cover the seven use cases, the three pages and the startup step.
