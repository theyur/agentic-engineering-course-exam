# Quickstart: validate accounts and access

**Feature**: specs/001-accounts-and-access · **Plan**: [plan.md](plan.md)

How to prove the slice works end to end once it is implemented. Contracts:
[contracts/application-api.md](contracts/application-api.md), [contracts/host.md](contracts/host.md).

## Prerequisites

- .NET SDK 10.0.x; `dotnet tool restore` (brings `dotnet-ef` and `ilspycmd`).
- SQL Server reachable as `.\COLL1` with integrated security, or the environment variables
  `HT_IT_CONNECTION`, `HT_UI_CONNECTION` and `HT_AC_CONNECTION` pointing elsewhere (ADR 0003, ADR 0006).
- Operator values in the User Secrets store of `HT.Ui`. Set them yourself; choose your own values that meet
  the password rule (at least 8 characters, with a number):

  ```bash
  dotnet user-secrets set "Operator:AdministratorEmail" "<an example.com email>" --project src/HT.Ui
  ```

  ```bash
  dotnet user-secrets set "Operator:AdministratorPassword" "<your value>" --project src/HT.Ui
  ```

  ```bash
  dotnet user-secrets set "Operator:DemoPassword" "<your value>" --project src/HT.Ui
  ```

## 1. Build and the solution's tests

```bash
dotnet build HabitTracker.slnx
```

```bash
dotnet test HabitTracker.slnx
```

Expected: the build has no warnings (`TreatWarningsAsErrors`); every test project passes: unit,
architecture, integration (database `HT-IT`) and UI (database `HT-UI`, environment `Testing`, so your User
Secrets and the `HT` database are not touched).

## 2. Acceptance tests

Run by the AC workflow, not by hand: `/speckit-ac-tests-write`, then `/speckit-ac-tests-analyze`.
Expected: the 63 scenarios of `tests/HT.AcceptanceTests/Features/F001/` pass against database `HT-AC`.

## 3. The running app

```bash
dotnet run --project src/HT.Ui --launch-profile https
```

Use the HTTPS address (`https://localhost:7194`): the session cookie is `Secure`. Expected in the console
(operator log), with no password anywhere:

- `Schema migrations applied: …` (Development only);
- `Administrator creation: administrator created (…)` on the first start, `administrator already exists (…)`
  afterwards;
- `Demo account demo1@example.com: created` and the same for demo2 and demo3 on the first start,
  `already exists` afterwards.

Then in a browser:

| Step | Expected |
|---|---|
| Open `/` | visitor Home: hero, three "How it works" steps; navigation "Sign in", "Register"; footer with "Privacy" |
| Open `/profile` | redirected to Sign in |
| Register a new `example.com` email | signed in, Home shows "Today"; navigation "Today" and the avatar letter |
| Avatar → Sign out | visitor Home; Back does not show Profile |
| Sign in as `demo1@example.com` with the demo password | signed-in Home |
| Profile → change the password | "Your password is changed."; the old password no longer signs in |
| Sign in with the configured administrator | same navigation as a registered user, no Admin link |
| Narrow the window below 860px, open the menu, press Esc | the menu lists the same items and closes |
| Five wrong passwords for one account, then the right one | "Email or password is not correct." until 5 minutes have passed |
