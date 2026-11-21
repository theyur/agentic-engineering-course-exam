# Quickstart: validate excusals and pauses

**Feature**: specs/008-excusals-and-pauses · **Plan**: [plan.md](plan.md)

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

Expected: no warning, no error, and every test passes. This feature's tests are listed in research R-11:

| What | Where | Proves |
|---|---|---|
| Reasons, pauses, exemption of an entry, pause rules | `HT.UnitTests/Domain/Exemptions` | FR-001, FR-006, FR-007, FR-010, FR-012; D-1; SC-003 boundaries |
| Consistency, streaks and trend with exemptions | `HT.UnitTests/Domain/Reports` | FR-013, FR-014, FR-016; SC-002 |
| Excusal and pause use cases, record checks 4a to 4c | `HT.UnitTests/Application/Exemptions`, `Application/Records` | FR-002 to FR-012, FR-021, FR-022; SC-003 to SC-005 |
| Reports and dashboard with exemptions | `HT.UnitTests/Application/Reports` | FR-015, FR-017 to FR-020, FR-024 |
| Demo pause data for every weekday | `HT.UnitTests/Application/Operator` | FR-031 |
| Store scoping, re-checks under the lock, migration, demo pause loading, logs | `HT.IntegrationTests` | FR-021, FR-025, D-3, HP-009 |
| Dialogs, pages, startup lines, smoke check | `HT.UiTests` | HP-001 to HP-011 |
| Layers, clock reads, port fences, parameter order | `HT.ArchitectureTests` | constitution II, III, V |

Mutation testing over the new Domain and Application code (ADR 0022), from `tests/HT.UnitTests`:

```bash
dotnet stryker --project HT.Domain.csproj
```

```bash
dotnet stryker --project HT.Application.csproj
```

In this worktree, run the whole project: `--since` finds nothing in a git worktree.

## 2. Acceptance tests

The owner starts `/speckit-ac-tests-write` and then `/speckit-ac-tests-analyze` after `/speckit-implement`. The
analysis runs all six features with acceptance tests. F001, F002, F004, F005 and F007 must still pass unchanged (FR-023), and F008's 88
scenarios must pass.

## 3. Run the app and look

```bash
dotnet run --project src/HT.Ui --launch-profile https
```

Open `https://localhost:7194` and sign in as `demo6@example.com` with the demo password from User Secrets. Never
print the password.

| Where | Expected |
|---|---|
| `/goals` | "Meditation, 10 min daily" under Active; the Paused tab has "Drinking water, 8 glasses a day" with "Paused until {R + 5}" and "Resume"; the footnote names excused and paused |
| "Meditation, 10 min daily" page | consistency "95% (22 of 23)", streaks 10 and 12 commitments; the History shows R − 20, R − 19 (reason "Travelling."), R − 8 and R − 2 as Excused; R − 2 offers "Remove excusal"; the strip has Excused cells |
| "Drinking water, 8 glasses a day" page | the Notice "Paused until … · Resume now"; no "Pause" button; the Pauses card lists the current pause ("Family visit.", "Resume now") and the past one ("Ill with flu.", no action); "94% (18 of 19)", streaks 2 and 16 days; Paused trend bars |
| `/` | tiles 1 Due, 2 Pending, 0 Missed this week, 1 Active goal; the Active goals card lists only "Meditation, 10 min daily" |

Then, as a new registered user:

1. Create a commitment-based goal starting today.
2. Pause it from today for 2 days, check the Notice, then choose "Resume now". The pause is removed, because it began
   today.
3. Pause it from tomorrow, then cancel the pause in the Pauses card.
4. Open "Record" and check that "Excuse instead" shows for today's Due commitment.
5. Choose "Excuse instead" and confirm. The History shows Excused with "Remove excusal". Remove the excusal.
6. Check every dialog with the keyboard: focus on the first field, Esc closes, and focus returns to the opener.

## 4. Unverified until the hosted demo runs

- After deployment, the smoke check's check 8 passes against the hosted app (ADR 0015).
- After the daily refresh, `demo6@example.com` is re-created, still shows Excused results and a paused goal, and
  the startup log shows the five steps in order with no reason, note or value.
