# Quickstart: validate the habit catalog

**Feature**: specs/002-habit-catalog · **Plan**: [plan.md](plan.md)

How to prove the slice works end to end once it is implemented. Contracts:
[contracts/application-api.md](contracts/application-api.md), [contracts/host.md](contracts/host.md).

## Prerequisites

As for slice 1 ([../001-accounts-and-access/quickstart.md](../001-accounts-and-access/quickstart.md)): .NET SDK
10.0.x, `dotnet tool restore`, SQL Server as `.\COLL1` (or the `HT_*_CONNECTION` variables), and the three
operator values in the User Secrets store of `HT.Ui`, set by you.

The seed catalog and the demo custom habits are created only into an empty catalog and only for demo
accounts created in the same run (FR-030, FR-031). A local `HT` database from slice 1 already has its demo
accounts, so it gets the seed catalog but no demo custom habits. To see both, drop the local database
first; the next start in Development recreates it with the migrations:

```bash
sqlcmd -S ".\COLL1" -E -Q "DROP DATABASE IF EXISTS [HT]"
```

## 1. Build and the solution's tests

```bash
dotnet build HabitTracker.slnx
```

```bash
dotnet test HabitTracker.slnx
```

Expected: no warnings; every test project passes, including the new architecture rules (research R-12), the
unit tests of the field checks, the formatter and the seed catalog, the store integration tests on `HT-IT`
and the UI tests on `HT-UI`.

## 2. Acceptance tests

Run by the AC workflow, not by hand: `/speckit-ac-tests-write`, then `/speckit-ac-tests-analyze`. Expected:
the 73 scenarios of `tests/HT.AcceptanceTests/Features/F002/` pass against `HT-AC`, and the 63 of `F001`
still pass.

## 3. The running app

```bash
dotnet run --project src/HT.Ui --launch-profile https
```

Expected in the operator log, on the first start after the database was dropped: slice 1's lines, then
`Demo custom habits demo1@example.com: demo custom habits added` after each `Demo account …: created` line,
then `Demo catalog: seed catalog loaded`. On the next start: `already exists` for each account, no custom
habits line, and `Demo catalog: catalog not empty`.

Then at `https://localhost:7194`:

| Step | Expected |
|---|---|
| Open `/` as a visitor | "Browse the catalog" in the hero; "Popular in the catalog" with four cards and "See all N →" |
| Follow "Catalog" in the navigation | "Habit catalog", the count with "Sort: A–Z", at least 20 cards in name order |
| Pick the category Money, then the intent Reduce, then type part of a name | the cards change at once; the address keeps the filters; reloading shows the same list |
| Filter to nothing | EmptyState with a way to clear the filters |
| Open a card's details | name, tags, description, "Suggested starting goals" with "Examples to adjust, not prescribed targets.", "Register to use this habit" |
| Sign in as `demo1@example.com` with the demo password, open Catalog → "My custom habits" | "Cold shower" and "No phone in bed" with the "Private" tag |
| Create a custom habit, edit it, delete it (confirm in the Dialog) | back on "My custom habits" each time; the habit is gone after Delete |
| Profile → "Custom habits" | the same custom habits, with a way to create and edit |
| Open `/admin` as `demo1@example.com` | the Not found page |
| Sign out, open `/custom-habits` | sent to Sign in |
| Sign in as the configured administrator, open "Admin" | the catalog table with Draft, Published and Unpublished habits; "Showing N of M" |
| "+ New catalog habit": a name, a category, Build or Reduce, an icon and two suggested starting goals, mark the second | saved as Draft; not in the public catalog |
| Publish it from its row, then check `/catalog` in another browser | it appears with the second suggested starting goal on its card |
| Unpublish it | it leaves the public catalog; its details address shows Not found |
| Open `/custom-habits` and `/custom-habits/new` as the administrator | the Not found page |
