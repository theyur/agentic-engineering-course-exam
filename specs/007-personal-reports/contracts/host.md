# Contract: Host, pages, operator log and smoke check of personal reports

**Feature**: specs/007-personal-reports · **Plan**: [../plan.md](../plan.md)

These are outside the AC surface (spec HP-001 to HP-011). They are verified by unit, bUnit, host and integration
tests, and by the smoke check. Texts and structure follow `design/`, which changes first.

## Routes

No route is added. `/`, `/goals` and `/goals/{Id:guid}` change. Each page shell stays static SSR. Interactive
islands are prerendered, so the first HTML contains every row (research R-12).

## Goal page (`design/pages/PageGoalDetail`, slice-6 state; HP-001)

- `GoalDetailBody` adds one read, `ReportService.GetGoalReportAsync`, next to slice 5's reads.
- In the left column, above History:
  - the **GoalStats** card:
    - the eyebrow "This attempt" and the title "Consistency and streaks";
    - the stats "Consistency" (`Stats.ConsistencyText`), "Streak, {unit}" and "Longest, {unit}" (plural unit,
      `ReportText.StreakLabel` and `LongestLabel`);
    - the ConsistencyStrip of `Recent` (30) with its legend, showing Met, Failed, Missed and Pending only;
  - the **TrendChart** card:
    - the bars of `Trend`; a bar with no value is drawn empty;
    - the dashed target line `ReportText.TargetLine` ("Target 20 min" or "Target 3");
    - date labels, and the title and sub line from `ReportText`.
- Neither card is shown when `HasBegunEntry` is false. After a record, a change, a delete or a note is saved in a
  dialog, the body reads the report again, so the figures follow the correction.
- Accessibility (HP-006):
  - the strip has `role="img"` and `aria-label` = `ReportText.StripLabel`;
  - the chart has `role="img"` and `aria-label` = `ReportText.TrendLabel`;
  - cell states are shown by shape and legend word, never by colour alone.

## Goals page (`design/pages/PageGoals`, slice-6 state; HP-002)

- The page head is slice 4's, with no Tabs.
- A `GoalsList` island, which reads `GetGoalsOverviewAsync` itself, renders one full GoalListItem per `OverviewGoal`, in the overview's order. The Razor
  component keeps slice 4's name `GoalItem`, extended to the full design item and replacing the reduced form
  (research R-12). Each item has:
  - the name link and the Build or Reduce Tag;
  - a Pending StatusChip when `HasPending`;
  - slice 4's meta line, plus the progress of a Due entry with a record (`ReportText.ProgressText`: "12 of 20 min
    today", "1 of 3 this week");
  - "Consistency" and "Current streak";
  - the mini strip of the last 15 results;
  - one action, "Record": primary when a Due entry exists, secondary when only Pending ones do, absent otherwise.
    It opens slice 5's RecordDialog in its page-head mode with `OverviewGoal.Recordable`: the "For" select lists
    every entry that can be recorded now, as on the goal page (slice 5 HP-002). After a save the island reads the
    overview again.
- Under the list, the footnote: "Each goal is counted on its own. Consistency counts only finalized results;
  pending ones are left out."
- A user with no goals keeps slice 4's EmptyState.

## Signed-in Home (`design/pages/PageHomeSignedIn`, slice-6 state; HP-003)

For a registered user with goals, under slice 4's page head:

- **`TodayPanel` island** (reads `GetDashboardAsync` itself in `OnInitializedAsync`, as `GoalDetailBody` does;
  research R-12):
  - four StatTiles:
    - "Due now" (`Counts.Due`);
    - "Pending · still can record" (`ht-tile--pending`, `Counts.Pending`);
    - "Missed this week" (`ht-tile--failed`, `Counts.MissedThisWeek`);
    - "Active goals" (`Counts.Goals`, a link to `/goals`).

    The first three are `<button>`s with `aria-pressed`. Choosing one filters the Today list to its state, and
    choosing it again shows all.
  - the **Today** card: one CommitmentRow per `TodayEntry`. Each row has:
    - the goal's name and a "Reduce" Tag for Reduce goals;
    - the meta line:
      - Due: the schedule, or its progress when it has a record;
      - Pending: "{date} · " and `ReportText.PendingLeft` ("5 h left to record", N rounded up by
        `RecordingText.HoursLeft`, the rule slice 5's "For" labels use);
      - Missed: "{date} · no record";
      - {date} is `DateTexts.ShortDate` ("Tue 17 Nov") or `WeekRange` ("10 – 16 Aug");
    - the progress bar, for count and duration goals;
    - the StatusChip, whose word comes from `ReportText.State` ("Upcoming" for an Upcoming row);
    - one action:
      - "Record" for a Due or Pending period, or a commitment without a record (opens the RecordDialog fixed to
        that entry, with the entry's `Goal` as `Recordable` and its `Entry` as the row);
      - "Edit" for a commitment with a record (opens that record);
      - "Add note" or "Edit note" for a Missed entry (opens the NoteDialog).

    An Upcoming row has the goal's name, its date as the meta line, the StatusChip "Upcoming" and no action. With
    no entry, the card shows "Nothing to record right now.". After a save the island reads the dashboard again.
- **"Active goals" card** (GoalSummary, static), right column:
  - each goal's name, linking to its page;
  - `Stats.ConsistencyText`;
  - `ReportText.SummaryLine` ("Streak 6 commitments · longest 11"), or with no final result "N/A" and
    `ReportText.NoFinalYet`;
  - at most 5 goals, then "All goals →" (`/goals`).
- **"Recent notes" card** (static): each `DashboardNote` as the note in quotes with "{goal name} · {date}" under
  it. The card is absent when there is no note.
- A registered user with no goals keeps slice 4's EmptyState. An administrator keeps the page head only.

## Visitor Home (`design/pages/PageHomeVisitor`, slice-6 state; HP-004)

- The hero shows a static GoalStats card labelled "Example goal", from `ExampleGoal`:
  - the title "Yoga, 3 days a week";
  - "90% (18 of 20)";
  - "Streak, commitments" 6 and "Longest, commitments" 11;
  - the strip of 21 results with its legend.
- It reads no store and is the same for every visitor. It is shown with or without Published habits, because it
  is part of the hero, not of the catalog preview (amended slice 2 US5-AS8).

## Operator log (HP-011)

`StartupTasks` runs its steps in this order: administrator creation, demo loading, demo history loading, demo
report loading. Demo report loading's lines follow demo history loading's:

| Situation | Lines |
|---|---|
| no demo password, or one that fails the password rule | `Demo report loading: no demo password configured` (or `… demo password does not meet the password rule`) |
| created | `Demo account demo5@example.com: created`, `Demo goals demo5@example.com: demo goals added`, `Demo records demo5@example.com: demo records added` |
| already exists | `Demo account demo5@example.com: already exists` |
| fault | `Demo report loading failed; the app starts without it` (ADR 0007), with no secret and no value; the start goes on |

With no or a weak demo password, a start logs three refusal lines: "Demo loading: …", "Demo history loading: …"
and "Demo report loading: …". A fault in demo history loading is followed by demo report loading's lines.

## Smoke check (`tools/HT.SmokeCheck`; HP-008)

| Check | Signs in as | Finds |
|---|---|---|
| 6 demo history (extended) | `demo4@example.com` | slice 5's texts; plus the stat labels "Consistency" and "Streak, commitments" (`PageReader.ReadStatLabels`) of "Reading, 20 min daily"'s page; "Due now" among the Home's tile labels (`PageReader.ReadTileLabels`) |
| 7 demo report (new) | `demo5@example.com` | on `/goals` (`PageReader.ReadGoalNames`): "Walking, 30 min, 3 days a week", "Strength training, 2 times a week", "Screen time, at most 120 min a day"; the stat label "Streak, weeks" of "Strength training, 2 times a week"'s page (the trend's "last 12 weeks" does not count); on `/`: "Missed this week" among the tile labels and "Missed" among the status chips (`PageReader.ReadStatusChips`; on the Home only Today rows carry chips) |

Every expected text must equal a whole label or chip text (`PageReader.ContainsAll`, ordinal), as in checks 1 to 6.
`PageReader` gains `ReadStatLabels`, `ReadTileLabels` and `ReadStatusChips`, readers of leaf elements, with
`PageReaderTests` cases: the existing `ReadCardText` matches only a `section.ht-card` with an `h2` title, which
neither the GoalStats card nor a StatTile has (research R-11).

Check 7 prints `PASS demo report demo5@example.com (N of N)` or its FAIL line, never a goal name, note or figure, as
check 6 does (slice 3 smoke-check contract). The checks and their host test change only after the pages exist
(plan, Work order step 7).

Coverage (`SmokeCheckCoverage`, slice 3 "Coverage rule"): `IDemoReportStore.CreateDemoReportAccountAsync` gets 4
entries (email, password, profile, goals) mapped to check 7. That brings the total from 10 to 14.
`specs/003-demo-hosting/contracts/smoke-check.md`'s table gains the same rows.

## Logs and errors (HP-007)

No log line, exception message or error page of a report read or of demo report loading contains a note, a
recorded value, a unit, a goal or habit name, or a figure. `ReportService` logs nothing.
