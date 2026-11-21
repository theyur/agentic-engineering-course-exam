# Contract: Host, pages, operator log and smoke check of excusals and pauses

**Feature**: specs/008-excusals-and-pauses · **Plan**: [../plan.md](../plan.md)

These are outside the AC surface (spec HP-001 to HP-012). They are verified by unit, bUnit, host and integration
tests, and by the smoke check. Texts and structure follow `design/`, which changes first (constitution IV,
ADR 0008). Every text comes from `ReportText`, `ExcusalTexts` or `PauseTexts`, so no page builds one (HP-008).

## Routes

No route is added. `/goals` accepts `?tab=paused`. Any other value, or none, shows Active. `/`, `/goals` and
`/goals/{Id:guid}` change. The page shells stay static SSR, and the islands stay prerendered (slice 6 research R-12).

## Goal page (`design/pages/PageGoalDetail`, slice-7 state; HP-001)

`GoalDetailBody` (an island) adds one read, `PauseService.GetPausesAsync`, next to its slice 5 and 6 reads. After
any write in a dialog or card it reads everything again, so the Notice, the cards and the figures agree.

- **Page head**: the secondary button "Pause" (`aria-haspopup="dialog"`), before "Record", opens the PauseDialog. It
  is absent while a pause is `Current`.
- **Notice**, under the head, while a pause is `Current`: "Paused until Sun 22 Nov · Resume now". "Resume now" is a
  button that calls `ResumeAsync` and shows a refusal in the Notice's place.
- **Pauses card**, in the right column after Settings, absent when the goal has no pause:
  - one row per `PauseView`: `RangeText`, the reason in `ht-muted` when it has one, and "Resume now" on the current
    pause or "Cancel pause" on an upcoming one (`CancelPauseAsync`);
  - a past pause has no action.
- **History card**:
  - the StatusChips "Excused" and "Paused";
  - an Excused row shows its reason after the chip (`ht-muted`) and the ghost action "Remove excusal" while
    `CanRemoveExcusal`, and has no "Add record";
  - a Paused row has no "Add record", but a record kept on it keeps its edit action;
  - both rows keep their note actions.
- **GoalStats**: the strip's Excused and Paused cells, and the legend Met, Failed, Missed, Excused, Paused, Pending.
- **TrendChart**: a bar with `Mark` is drawn as the design's paused bar (`ht-bar--paused`) or Excused bar (dashed,
  told apart in its `title` and in the chart's `aria-label`).

## PauseDialog (`design/components/PauseDialog`; HP-002, HP-007)

- The title is "Pause · {goal name}". The effect comes first: "Paused days are not counted. Your streak stays as it
  is. Final results do not change."
- The fields:
  - "From" (a date, by default the page's current date, `min` today);
  - "Until" (required);
  - "Reason (optional)".
- The buttons are the primary "Pause goal" and "Cancel".
- A refusal shows under the field it is about, one at a time, with the result's message as it is:
  - `DaysMissing` under the empty field;
  - `StartsBeforeToday` under "From";
  - `LastBeforeFirst` and `Overlaps` under "Until";
  - `ReasonTooLong` under "Reason".
- The component (T062, `src/HT.Ui/Components/Exemptions/PauseDialog.razor`) takes `Open`, `OnClose`, `ReturnFocus`,
  `FocusOnClose`, `GoalId`, `GoalName`, `Today` (the page's current date: `PausesResult.PauseDate`, research R-03),
  `ActingUser` and `OnSaved` (raised on "paused"). The Pauses card (T063, `src/HT.Ui/Components/Exemptions/PausesCard.razor`)
  takes `Pauses`, `OnResume` and `OnCancel` (each raised with its `PauseView`) and `Refusal` (shown under its title).
  The goal page reads the pauses first, for every goal, a goal with no current date included (T064).

## Excuse dialog (`design/components/ExcuseDialog`; HP-003, HP-007)

- The title is "Excuse · {goal name}". The sub line gives the date and state, "Wed 11 Nov · Pending, 22 h left"
  (from `RecordingText`). Then the effect: "An excused commitment is not counted. Your streak stays as it is."
- The field "Reason (optional)", and the buttons, the primary "Excuse commitment" and "Cancel".
- It opens from "Excuse" on the Home's Pending rows, and from "Excuse instead" (ghost, right) in the RecordDialog.
  "Excuse instead" shows only for a commitment-based goal whose selected "For" entry is Due or Pending with no record.
- A refusal shows in the dialog with the result's message. After "excused", the opener reads its data again.
- The component (T048, `src/HT.Ui/Components/Exemptions/ExcuseDialog.razor`) takes `Open`, `OnClose`, `ReturnFocus`,
  `FocusOnClose`, `GoalId`, `Date`, `GoalName`, `SubLine` (the opener formats it with `RecordingText.ForLabel`),
  `ActingUser` and `OnSaved` (raised on "excused"). The components it touches gain (T049 to T051):
  `RecordDialog.OnExcuseInstead` (the button shows only when the opener handles it; the goal page, the Home's
  `TodayPanel` and the Goals page's `GoalsList` all do), `HistoryCard.OnRemoveExcusal`, `HistoryCard.Refusal` (the
  "Remove excusal" refusal under the card head), `HistoryCard.FocusRecordOrNoteAsync`, `CommitmentRow.ActionKind.Excuse`
  and `CommitmentRow.FocusExcuseAsync`.
- "Remove excusal" on the goal page reads everything again after the result, also after a refusal, and the focus goes to
  the row's "Add record", or to its note control when the row has none.

## Goals page (`design/pages/PageGoals`, slice-7 state; HP-004)

- **Tabs**: "Active" and "Paused" (the existing `Tabs` component), with no "Ended". The selected tab is in
  `?tab=paused`.
- **`GoalsList` island**: it reads `GetGoalsOverviewAsync` once.
  - Active lists the goals with no `PausedUntil`, as slice 6 HP-002 does.
  - Paused lists the goals with `PausedUntil`, in the same order. Each has the meta "Paused until Sun 22 Nov"
    (`ReportText.PausedUntil`), rendered by the shared `GoalItem` in its Paused form, the Pending StatusChip when `HasPending`, its figures and strip, and the secondary
    action "Resume" (`ResumeAsync`, then the island reads the overview again).
  - An empty Paused tab says "No paused goals.".
  - A user with no goals keeps slice 4's EmptyState in place of the tabs.
- **Footnote**: "Each goal is counted on its own. Consistency counts only finalized results; pending, excused and
  paused ones are left out." (`ReportText.GoalsFootnote`; the empty Paused tab's text is `ReportText.NoPausedGoals`)
- The components (T076, T077): `Goals.razor` reads `tab` with `[SupplyParameterFromQuery(Name = "tab")]` and passes it
  to the island as `GoalsList.Tab` (a string, its only parameter; "paused" shows Paused, compared ordinally). `GoalItem`
  adds `OnResume` (raised with its `OverviewGoal`); its `FocusAsync` focuses "Resume" in the Paused form. The shared
  `Tabs` adds `FocusAsync(string text)`, which focuses the tab with those words (none: nothing), so the island can give
  the focus to the "Paused" tab after the last "Resume". A refused "Resume" shows its text as
  `p.ht-field__error[role=alert]` between the Tabs and the list, and the island reads the overview again.
- An Active tab with no goal, because every goal is paused, shows no list, only the Tabs and the footnote (settled
  here, T077: HP-004 and the design give no text for it).

## Signed-in Home (`design/pages/PageHomeSignedIn`, slice-7 state; HP-005)

- **`TodayPanel`** and **`CommitmentRow`**: the ghost action "Excuse" on Pending rows of commitment-based goals with no record. It opens the
  Excuse dialog for that row's goal and date. After "excused", the row leaves the list and the "Pending · still can
  record" tile drops by 1.
- **Tiles**: the Active goals tile shows `Counts.Goals`, which leaves out paused goals.
- **Active goals card**: it lists the `DashboardGoal`s with no `PausedUntil`.
- **No goals**: the page chooses slice 4's EmptyState by `Dashboard.Goals` being empty, not by `Counts.Goals` (T078),
  so a user whose goals are all paused keeps the dashboard, with the tile at 0 and no goal in the card.
- Nothing else changes.

## Shared components (HP-006)

| Component | Change |
|---|---|
| `StatusChip` | + "Excused" (`ht-status--excused`), "Paused" (`ht-status--paused`), each with its word |
| `ConsistencyStrip` | + `ht-cell--excused`, `ht-cell--paused`; `aria-label` from `ReportText.StripLabel`, e.g. "Last 17 commitments: 10 met, 1 missed, 1 excused, 3 paused, 2 pending" |
| `TrendChart` | marked bars; each bar's `title` says "Excused" or "Paused" in place of a value (`ReportText.BarTitle`, "Mon 9 Nov: Excused"), and the chart's `aria-label` counts them apart from bars with no record (`ReportText.TrendLabel`, "…; target 10; 1 excused, 3 paused") |
| `PauseDialog`, `ExcuseDialog`, `PausesCard` | new, each implemented once and reused |

A state is never shown by colour alone: every chip and cell carries its word or shape. The dialogs use `HtDialog`.
The first field gets focus on open, Esc and × close, and focus returns to the opener, or to the nearest remaining
control when the opener is gone (HP-007).

## Operator log (HP-010)

`StartupTasks` runs, in this order: administrator creation, demo loading, demo history loading, demo report
loading, demo pause loading.

| Situation | Lines |
|---|---|
| no demo password, or one that fails the password rule | `Demo pause loading: no demo password configured` (or `… demo password does not meet the password rule`) |
| created | `Demo account demo6@example.com: created`, `Demo goals demo6@example.com: demo goals added`, `Demo records demo6@example.com: demo records added` |
| already exists | `Demo account demo6@example.com: already exists` |
| fault | `Demo pause loading failed; the app starts without it` (ADR 0007), with no secret and no value; the start goes on |

The host tests that compare "Demo " lines exactly expect `demo6@example.com` after `demo5@example.com` (spec
"Earlier requirements and tests this slice changes").

## Smoke check (`tools/HT.SmokeCheck`; HP-011)

| Check | Signs in as | Finds |
|---|---|---|
| 8 demo pause (new) | `demo6@example.com` | "Excused" among the status chips (`PageReader.ReadStatusChips`) of "Meditation, 10 min daily"'s page; on `/goals?tab=paused`, the goal name "Drinking water, 8 glasses a day" (`PageReader.ReadGoalNames`) and a meta text starting "Paused until" in the same item (a new reader, `PageReader.ReadGoalMetas`, with `PageReaderTests` cases) |

Check 8 prints `PASS demo pause demo6@example.com (N of N)` or its FAIL line, never a goal name, reason, note or
figure.

Coverage: `IDemoPauseStore.CreateDemoPauseAccountAsync` gets 4 entries (email, password, profile, goals) mapped to
check 8. That brings the total from 14 to 18. `specs/003-demo-hosting/contracts/smoke-check.md` gains the rows and
check 8. In the same change: `RunnerBehaviourTests.GoodSite()`, `SmokeCheckHostTests`' exact lines and "(N of N)",
and `CoverageTests`.

## Logs and errors (HP-009)

No log line, exception message or error page of this slice contains an excusal or pause reason, a note, a recorded
value, a goal or habit name. `ExcusalService`, `PauseService` and the new store members log nothing. Store faults
name the SQL error number only.
