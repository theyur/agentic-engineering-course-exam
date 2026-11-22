# Quickstart: validate goal changes

**Feature**: specs/009-goal-changes · **Plan**: [plan.md](plan.md)

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

Expected: no warning, no error, and every test passes. This feature's tests are listed in research R-12:

| What | Where | Proves |
|---|---|---|
| Attempts, endings, attempt-aware timeline, last dates, settings changes, names | `HT.UnitTests/Domain/Goals` | FR-001, FR-002, FR-004, FR-008, FR-009, FR-013; D-1, D-2; SC-002, SC-003, SC-005 boundaries |
| Equivalence of one-attempt goals with slices 4 to 7 | `HT.UnitTests/Domain/Goals`, `Domain/Reports` | FR-024 |
| Change, end, starting values, start attempt, the retry on `Stale` | `HT.UnitTests/Application/Goals` | FR-003, FR-005 to FR-007, FR-011, FR-012, FR-022, FR-023; SC-004, SC-006 |
| Attempts, one attempt, goal history, earlier Pending entries | `HT.UnitTests/Application/Reports` | FR-014 to FR-019 |
| History range, recordable, recent notes, pause and excuse changes | `HT.UnitTests/Application/Records`, `Application/Exemptions` | FR-010, FR-016, FR-027 |
| Demo goal-change data on every weekday | `HT.UnitTests/Application/Operator` | FR-031 |
| Migration, conditional writes, cuts, catalog counts, pause re-check, scoping, demo goal-change loading, logs, SC-007 timing | `HT.IntegrationTests` | FR-009, FR-020, FR-021, FR-022, FR-026; HP-010 |
| Goal page, Edit settings, EndGoalDialog, Start new attempt, attempt view, Ended tab, Goal history, Home Tags, startup lines, smoke check | `HT.UiTests` | HP-001 to HP-012, HP-014 |
| Layers, clock reads, port fences, parameter order | `HT.ArchitectureTests` | constitution II, III, V |

Mutation testing over the new and changed Domain and Application code (ADR 0022), from `tests/HT.UnitTests`:

```bash
dotnet stryker --project HT.Domain.csproj
```

```bash
dotnet stryker --project HT.Application.csproj
```

In this worktree, run the whole project: `--since` finds nothing in a git worktree.

## 2. Acceptance tests

The owner starts `/speckit-ac-tests-write` and then `/speckit-ac-tests-analyze` after `/speckit-implement`. The
analysis runs all seven features with acceptance tests:

- F001, F002, F004, F005, F007 and F008 must still pass unchanged (FR-024);
- F009's 106 scenarios must pass.

## 3. Run the app and look

```bash
dotnet run --project src/HT.Ui --launch-profile https
```

Open `https://localhost:7194` and sign in as `demo7@example.com` with the demo password from User Secrets. Never
print the password. On a database that already has `demo7`, its dates are relative to the day it was first created
(ADR 0026). The figures below hold on a fresh database.

| Where | Expected |
|---|---|
| `/goals` | Active lists "Reading, 20 min daily" and "Meditation, 5 min daily"; the Ended tab lists "Sugar, at most 1 sweet a day" with the Tag "Achieved", "Ended {R − 7}" and "Start new attempt" |
| "Reading, 20 min daily" page | "96% (25 of 26)", streaks 6 and 19 commitments; the Settings card shows 20 min from R − 13 and the row "From {R + 1}" with "25 min daily"; "Edit settings" and "End goal" in the head |
| "Meditation, 5 min daily" page | "Attempt 2 · started {R − 14}"; "100% (12 of 12)"; the Earlier attempts card has "Attempt 1 · {R − 42} – {R − 22}", "Meditation, 10 min daily", "Abandoned", "52% (11 of 21)" and "Too busy." |
| Attempt 1's view | its History from R − 22 down to R − 42, with R − 31 to R − 22 Missed; no Record, Edit settings, Pause or End goal |
| "Sugar, at most 1 sweet a day" page | the Notice "Ended {R − 7} · Achieved" with "Start new attempt"; "100% (23 of 23)", streaks 23 days |
| `/profile?tab=history` | "Sugar, at most 1 sweet a day", attempt 1, "Achieved"; then "Meditation, 10 min daily", attempt 1, "Abandoned" |
| `/` | tiles 2 Due, 4 Pending, 0 Missed this week, 2 active goals |

Then, as a new registered user:

1. Create a goal on all seven days, starting today. Choose "Edit settings", raise the amount and save. The Settings
   card shows "From {tomorrow}" with the new target. Today's History row keeps the old target.
2. Record today with a duration, then choose "Edit settings" again and change the measurement to a count in pages.
   The Settings card's "From {tomorrow}" row now shows the count target. Today's History row still shows its record in
   minutes, and its RecordDialog still offers a duration field (AC-020, research R-07).
3. Open "Edit settings" again and check that the schedule choice offers only fixed days, the goal's kind (HP-002). The refusal "A change keeps the schedule kind: …" can't be reached from the page. Unit tests and AC-010 prove it through the use case.
4. Choose "End goal", choose "End goal" in the dialog with no outcome, and check "Choose how the goal ended." under
   "How did it end?". Then choose "Abandoned" and confirm. The Notice shows "Ended {today} · Abandoned", and the goal
   is on the Ended tab. Today's commitment is gone from the Home.
5. On the Goals page's Ended tab, choose "Start new attempt". The start date is tomorrow, and today gives "A new
   attempt starts after the day the goal ended.". Keeping the starting values would take two actions, "Start new
   attempt" and "Start attempt" (SC-001). Instead, start it tomorrow with "3 times a week". The goal page shows "Attempt 2 ·
   starts {tomorrow}", the Settings card "Starts {tomorrow}", and the Earlier attempts card with attempt 1 ("ended
   before it began", as it ended on its start date).
6. Check the dialog and the forms with the keyboard: focus on the first field, Esc closes the dialog, and focus
   returns to the opener, or to "Start new attempt" after ending.

## 4. Unverified until the hosted demo runs

- After deployment, the smoke check's check 9 passes against the hosted app (ADR 0015).
- After the daily refresh, `demo7@example.com` is re-created relative to the new day. The startup log shows the startup
  steps in order, demo goal-change loading last, with no ending note, note, value, goal or habit name.
