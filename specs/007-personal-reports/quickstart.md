# Quickstart: validate personal reports

**Feature**: specs/007-personal-reports · **Plan**: [plan.md](plan.md)

How to prove this feature works end to end. The rules are in [contracts/](contracts/) and the types are in
[data-model.md](data-model.md). This guide only runs them.

The prerequisites are slice 5's ([quickstart](../005-recording-and-evaluation/quickstart.md#prerequisites)):

- the .NET 10 SDK;
- SQL Server at `.\COLL1`, or `HT_IT_CONNECTION` and `HT_UI_CONNECTION` pointing elsewhere;
- the User Secrets of `HT.Ui` set (never print their values);
- `git config core.hooksPath .githooks`.

## 1. Build and the solution's tests

```bash
dotnet build HabitTracker.slnx
```

```bash
dotnet test HabitTracker.slnx --nologo -v q --logger "console;verbosity=minimal"
```

Expected: no warning, no error, and every test passes. This feature's tests are listed in research R-14:

| What | Where | Proves |
|---|---|---|
| Consistency, streaks, unit, trend | `HT.UnitTests/Domain/Reports` | FR-001 to FR-006, rounding down, D-1, SC-003 boundaries |
| The whole attempt | `HT.UnitTests/Domain/Goals` (`EntriesThrough`) | the report range beyond 366 days, no week doubled at a window edge |
| The three use cases | `HT.UnitTests/Application/Reports` | FR-007 to FR-016: refusal order, ordering, the Today list parts, Upcoming, "this week", notes, no goals; SC-004 correction equality; SC-005 |
| The formatter and the example card | `HT.UnitTests/Application/Reports` (`ReportTextTests`, `ExampleGoalTests`) | HP-004, HP-005, SC-002 |
| Demo report data | `HT.UnitTests/Application/Operator` (`DemoReportTests`) | FR-032's records, notes and figures for each weekday |
| Real stores, owner scoping, timing | `HT.IntegrationTests/Reports` | FR-014, SC-007 (each read under 2 s for 366 daily results) |
| Demo report loading | `HT.IntegrationTests/Operator` | created, already exists, a habit that is not Published left out, rollback |
| No private data in logs | `HT.IntegrationTests` log capture | HP-007 |
| Pages and components | `HT.UiTests/Components` | HP-001 to HP-004, HP-006, SC-001, SC-008 |
| Startup and smoke check | `HT.UiTests/Host`, `HT.UiTests/Operator`, `HT.UiTests/SmokeChecks` | HP-008, HP-011 |
| Architecture | `HT.ArchitectureTests` | the clock rule, the port fences, the public surface |

To run one area on its own:

```bash
dotnet test HabitTracker.slnx --nologo -v q --filter "FullyQualifiedName~HT.UnitTests.Domain.Reports"
```

## 2. Acceptance tests

After `/speckit-implement`, the owner runs `/speckit-ac-tests-write` and then `/speckit-ac-tests-analyze`.

Expected: every F007 scenario passes, and F001, F002, F004 and F005 pass unchanged (FR-016).

## 3. Run the app and look

Demo history is relative to the day each demo account was first created (ADR 0018, ADR 0021). The exact figures in
steps 3 to 5 therefore hold only when `demo4` and `demo5` were created today. So first drop the database that
`HT.Ui`'s connection string names, so that the next start creates every account anew. On an older local database
the same rules give other figures: fewer Pending results, and more than one Missed result this week for `demo5`.

Then start the app:

```bash
dotnet run --project src/HT.Ui --launch-profile https
```

Open `https://localhost:7194`.

1. **The operator log.** It has "Demo account demo5@example.com: created" (or "already exists") after demo4's
   lines.
2. **The visitor Home.** It shows the "Example goal" card: "90% (18 of 20)", streak 6, longest 11, a strip of 21
   cells with its legend.
3. **`demo4@example.com`'s pages.** Sign in with the demo password.
   - The Home shows the four tiles, the Today list with Pending results first, and the "Recent notes" card with
     "Fell asleep early.". Choosing "Pending · still can record" filters the list, and choosing it again shows all.
   - "Reading, 20 min daily" shows GoalStats ("Consistency", "Streak, commitments") and a trend of 14 bars with
     "Target 20 min".
4. **`demo5@example.com`'s pages.** Sign in as `demo5@example.com`.
   - `/goals` lists its three goals with their figures and mini strips.
   - "Strength training, 2 times a week" has 12 weekly bars and the unit "weeks".
   - The Home shows "Missed this week" 1 with a "Missed" row for Screen time.
5. **A correction.** As `demo4`, open "Reading, 20 min daily". Record 20 minutes for the Missed date R − 10 from its History
   row. Expected: the consistency changes from "92% (24 of 26)" to "96% (25 of 26)" and the longest streak
   grows, on the same page without reloading it.

## 4. Unverified until the hosted demo runs

- **The hosted smoke check after a deployment**: check 6's three new texts and check 7 for `demo5`. They run in
  the deploy workflow, which the owner starts.
- **The daily refresh re-creating `demo5`.** Proven only by the next scheduled run.
