# Quickstart: validate recording and evaluation

**Feature**: specs/005-recording-and-evaluation · **Plan**: [plan.md](plan.md)

How to prove slice 5 works end to end. The contracts are in [contracts/](contracts/) and the types in
[data-model.md](data-model.md); this guide only runs them.

## Prerequisites

- .NET 10 SDK; SQL Server at `.\COLL1` with integrated security, or `HT_IT_CONNECTION` and `HT_UI_CONNECTION`
  pointing elsewhere (ADR 0006).
- The User Secrets of `HT.Ui` (`Operator:AdministratorEmail`, `Operator:AdministratorPassword`,
  `Operator:DemoPassword`) set; never print their values.
- `git config core.hooksPath .githooks`, once per clone.

## 1. Build and the solution's tests

```bash
dotnet build HabitTracker.slnx
```

```bash
dotnet test HabitTracker.slnx --nologo -v q --logger "console;verbosity=minimal"
```

Expected: no warning, no error, every test passes. The slice adds tests for these points:

| What | Where | Proves |
|---|---|---|
| Evaluation boundaries | `HT.UnitTests/Domain/Evaluation` | SC-002: at, below and above every target; both sides of the start, end and grace end; the Kyiv clock changes. |
| Records, notes, entries | `HT.UnitTests/Domain/Records`, `Domain/Goals` | FR-002 to FR-005, FR-007 ranges and dates. |
| Use cases | `HT.UnitTests/Application/Records` | The check order, the privacy negatives for all seven use cases (SC-004), and corrections equal to on-time records (SC-003). |
| Demo history | `HT.UnitTests/Application/Operator` | FR-030's data for R on every weekday. |
| Stores | `HT.IntegrationTests/Records` | Owner scoping, the stored order, the 50 limit, the two races, the migration, demo history loading, and the HP-006 log capture (SC-006). |
| Pages | `HT.UiTests` | The History card, the dialogs, StatusChip, the 404 for another account's goal, the startup log lines, and the smoke check. |
| Architecture | `HT.ArchitectureTests` | Research R-13. |

To run one area on its own:

```bash
dotnet test HabitTracker.slnx --nologo -v q --filter "FullyQualifiedName~HT.UnitTests.Domain.Evaluation"
```

## 2. Acceptance tests

They run after `/speckit-implement`, through `/speckit-ac-tests-write` and `/speckit-ac-tests-analyze`. The owner
starts both. Expected: every F005 scenario passes, and F001, F002 and F004 pass unchanged (FR-019).

## 3. The running app

```bash
dotnet run --project src/HT.Ui --launch-profile https
```

Open `https://localhost:7194`. With a fresh database, the startup log shows demo loading's lines, then:

```text
Demo account demo4@example.com: created
Demo goals demo4@example.com: demo goals added
Demo records demo4@example.com: demo records added
```

A database that already had `demo4` shows `Demo account demo4@example.com: already exists` and nothing more. Its
history stays relative to the day it was first created (spec Assumptions).

Check by hand:

1. Sign in as `demo4@example.com` with the demo password. Goals lists "Reading, 20 min daily", "Sugar, at most 1
   sweet a day" and "Yoga, 3 times a week".
2. "Reading, 20 min daily":
   - the first 10 History rows show, newest first: Due today (R), Pending (R − 1, and R − 2 with 25 min), Met rows,
     and Failed with 12 min and the note "Fell asleep early." (R − 6, the 7th row);
   - "Show all N →" lists the rest, including Missed with "Away for the weekend." (R − 10, the 11th row);
   - Recent notes shows both notes with their states.
3. "Record", for today, 12 minutes: the progress reads "12 of 20 min" under the field, the row shows 12 min, and
   it is still "Due today".
4. After "Show all", "Add record" on the Missed row R − 10. The note field holds "Away for the weekend.". Save 20
   minutes with the field unchanged: the row turns Met (a correction), still with the note, and offers "Edit" only.
   "Edit", then "Delete record": the row is Missed again, with "Away for the weekend." and "Edit note".
5. On a Met row, "Edit", type "Test note." and save. The chip stays Met and the note column shows "Test note.".
   "Edit", clear the field and save: the note is gone. A Met row has no "Add note".

   Steps 4 and 5 amended 2026-11-20, specs/006-note-editing, ADR 0019 (issue #37): step 4 did not check the note,
   and step 5 added the note with the Met row's "Add note", which rows with a record no longer have.
6. "Yoga, 3 times a week": one row per week; "Due this week", a Met or Pending week, a Failed week with the note
   "Knee was sore.", and a Missed week.
7. "Sugar, at most 1 sweet a day": Met with 0 or 1, and Failed with 3 sweet (the unit as stored) and "Birthday cake at work.".
8. Keyboard only: Tab reaches "Record", Enter opens the dialog with focus in the first field, Esc closes it, and
   focus returns to "Record".
9. Sign in as another registered user and open the goal URL from step 2: Not found, status 404.
10. New goal: "How do you measure it?" shows "You can record partial amounts; the target decides the result.".

## 4. Hosted demo (after deployment, slice 3)

- A manual deployment or refresh succeeds only when the smoke check passes, now with `PASS demo history
  demo4@example.com (11 of 11)`.
- The next day, the workflow's run history shows a scheduled run at about 00:20 UTC, in mode refresh, which passed.
  `demo4`'s Reading page then shows today as "Due today" and yesterday as Pending.
- After 2027-01-31, a scheduled run ends as succeeded with "Demo refresh: skipped after 2027-01-31" from the `gate`
  job. Its `demo` job is skipped, so the run never enters the `demo` environment. The Operator guide's teardown
  removes the trigger.

What tests cannot cover is checked here by hand and stays labelled unverified until it is: the browser behaviour
of the dialogs beyond bUnit, the real schedule, and the hosted smoke check.
