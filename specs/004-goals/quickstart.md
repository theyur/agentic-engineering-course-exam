# Quickstart: validate goals

**Feature**: specs/004-goals · **Plan**: [plan.md](plan.md)

This is how to prove the slice works end to end once it is implemented. Contracts:
[contracts/application-api.md](contracts/application-api.md), [contracts/host.md](contracts/host.md).

## Prerequisites

The prerequisites are the same as for slices 1 and 2 ([../002-habit-catalog/quickstart.md](../002-habit-catalog/quickstart.md)):

- .NET SDK 10.0.x;
- `dotnet tool restore`;
- SQL Server as `.\COLL1`, or the `HT_*_CONNECTION` variables;
- the operator values in the User Secrets store of `HT.Ui`, which you set.

There is one optional new setting, `Operator:DefaultTimeZone`, for example `Europe/Kyiv`. Without it the default
is `UTC` (FR-025). It is not a secret, so `appsettings.Development.json` or an environment variable works too.

Demo goals are written only for demo accounts created in the same run (FR-040, ADR 0013). A local `HT` database
from an earlier slice already has its demo accounts, so it gets no demo goals. Its accounts also read as the
default time zone (HP-019). To see the demo data, drop the database first; the next start in Development
recreates it:

```bash
sqlcmd -S ".\COLL1" -E -Q "DROP DATABASE IF EXISTS [HT]"
```

## 1. Build and the solution's tests

```bash
dotnet build HabitTracker.slnx
```

```bash
dotnet test HabitTracker.slnx --nologo -v q --logger "console;verbosity=minimal"
```

Expected: no warnings, and every test project passes. That includes:

- the new architecture rules: clock and tzdb reads, no NodaTime type on the public surface, owner-first goal store,
  and the fences (research R-13);
- the boundary unit tests of `GoalTimeline` (SC-003);
- the goal store and race integration tests on `HT-IT` and `HT-IT-SEED`;
- the HP-019 migration test;
- the UI tests on `HT-UI`, including the slice 1 and 2 tests the spec lists as changed.

A public API listing of `HT.Application` taken before and after the slice should show only additions and the
documented constructor and port changes (research R-07):

```bash
roslynator list-symbols HabitTracker.slnx --projects HT.Application --visibility public --depth member
```

## 2. Acceptance tests

The AC workflow runs these, not you: `/speckit-ac-tests-write`, then `/speckit-ac-tests-analyze`. Expected:

- the 94 scenarios of `tests/HT.AcceptanceTests/Features/F004/` pass against `HT-AC`;
- the 73 scenarios of `F002` and the 63 of `F001` still pass (FR-019).

The analyze step rebuilds the AC project against freshly exported reference assemblies. A locked F001 or F002
step definition that no longer compiles therefore shows up in its run report and judge report. Nobody reads the
step definitions.

## 3. The running app

```bash
dotnet run --project src/HT.Ui --launch-profile https
```

The operator log on the first start after the database was dropped shows, in this order (HP-013):

1. slice 1's administrator line;
2. `Demo catalog: seed catalog loaded`;
3. for each demo email:
   - `Demo account …: created`;
   - `Demo custom habits …: demo custom habits added`;
   - `Demo goals …: demo goals added`.

On the next start: `catalog not empty`, then `already exists` for each account, with no custom-habit or goal
line.

Then at `https://localhost:7194`:

| Step | Expected |
|---|---|
| Sign in as `demo1@example.com` with the demo password | Home: "Good morning/afternoon/evening, Demo One" and today's date; TopNav shows Goals, "+ New goal" and the avatar letter "D" |
| Open Goals | "No phone in bed, avoided every day" and "Reading, 15 min, 5 days a week", in name order, each with its Build or Reduce Tag and meta line |
| Open "Reading, …" | "← Goals", the tags, "Attempt 1 · started …", and the Settings card with "Changes apply from the next commitment. Past results keep their rules." |
| Catalog | "Reading" shows "1 goal" and "Add another goal"; "· you track 1"; "Show: Only habits I track" leaves only Reading, and the address keeps it |
| Reading's details → "Start with this" on any suggested starting goal | New goal page with that suggestion's values and its "Catalog suggestion: …" line; the summary card updates as you change fields; a per-week schedule with a start date that is not a Monday shows "Your first week starts on Mon …" |
| Set Reduce, Count, 2 `cups`, 1 time per day, then "Create goal" | the goal page; the name is "Reading, at most 2 cups a day" |
| Set the amount to 1000 and create | "Enter a whole number from 1 to 999." (or 0 to 999 for Reduce) under the amount; nothing is created |
| My custom habits → "No phone in bed" | no "Delete" (a goal uses it) |
| Create a custom habit with "Save and create goal" | New goal page with that habit chosen |
| Profile → Account: change the time zone to `America/New_York` | the Notice of HP-015; the goal page now gives the new zone from the next day or week |
| Profile → Account: change the name to "Dee" | "Your name is changed."; the avatar letter is "D", the Home greeting says ", Dee" |
| Sign in as `demo3@example.com`, open its "Coffee, at most 2 cups a day" goal | the Notice "This habit is no longer in the public catalog. Your goal keeps working." |
| Sign in as `demo2@example.com`, open Goals | "Morning yoga" and "Meditation, 10 min, 3 days a week" with "starts …" (7 days from today) |
| Register a new account in a browser set to another time zone, with a name | Profile shows the name and "member since …"; the time zone field shows the browser's zone (HP-016: needs a real browser, not covered by automated tests) |
| As the configured administrator, unpublish "Reading" | a Dialog: "1 goal uses this habit." (or more) and that the goals keep working; confirm, and Reading leaves the public catalog |
| As the administrator, open `/goals` | the Not found page; no Goals link in the navigation |
| Sign out, open `/goals` | sent to Sign in |

## 4. Hosted demo (after deployment, slice 3)

The smoke check (`tools/HT.SmokeCheck`) must pass. Its check 5 now also finds each demo account's goal names on
`/goals`, and "Demo One" on demo1's `/profile` (HP-018, SC-006).
