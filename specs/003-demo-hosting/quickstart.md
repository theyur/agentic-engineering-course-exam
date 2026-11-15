# Quickstart: validate demo hosting

**Spec**: [spec.md](spec.md) · **Plan**: [plan.md](plan.md) · Contracts:
[deploy-workflow](contracts/deploy-workflow.md) (step numbers), [smoke-check](contracts/smoke-check.md),
[hosted-configuration](contracts/hosted-configuration.md)

Part A runs locally and in CI. Part B needs the Azure resources and the merged workflow, and is run by the
Operator. Until Part B has run, everything it checks is reported as unverified (constitution V).

## Part A: local

Prerequisites: .NET 10 SDK; SQL Server `.\COLL1`, or `HT_IT_CONNECTION` and `HT_UI_CONNECTION` pointing
elsewhere (ADR 0006). The new host tests derive `HT-UI-SMOKE` and `HT-UI-KEYS` from `HT_UI_CONNECTION`.

1. Build and test:

   ```bash
   dotnet build HabitTracker.slnx
   ```

   ```bash
   dotnet test HabitTracker.slnx --nologo -v q --logger "console;verbosity=minimal"
   ```

   Expected: all pass, including the smoke-check host tests, the key-directory host tests, the coverage test
   and the architecture rule for `HT.SmokeCheck` (research R-11). Then:

   ```bash
   pwsh -NoProfile -File tools/hosting/selftest.ps1
   ```

   Expected: `PASSED` (the `example.com` check and the secret-file helper, R-03).
2. Smoke check against a local run: start the app (`dotnet run --project src/HT.Ui --launch-profile https`) on
   an emptied local database. In a shell where the three `HT_SMOKE_*` variables are set from your User Secrets
   by your own means (never pasted into a shared log), run:

   ```bash
   dotnet run --project tools/HT.SmokeCheck -- https://localhost:7194 --skip-http-redirect
   ```

   Expected: `PASS` for `home`, `catalog`, `administrator` and the three demo emails, a `TIMING` line,
   `RESULT PASS`, exit code 0; no password, administrator email or habit name in the output. Unset
   `HT_SMOKE_DEMO_PASSWORD`: exit code 2. A wrong demo password: the three `demo` checks fail, exit code 1.
3. Migration bundle (HP-004, research R-05): build it for your platform with
   `dotnet ef migrations bundle --project src/HT.Persistence --startup-project src/HT.Persistence --self-contained --output <scratch folder>/efbundle`,
   create an empty local database, set `HT_DESIGN_CONNECTION` to it, and run the bundle. Expected: exit code
   0 and both migrations (`InitialAccounts`, `HabitCatalog`) in `__EFMigrationsHistory`; a second run applies
   nothing and exits 0.
4. Secret scan (SC-004, local part): search the repository, including `.github/workflows/deploy-demo.yml` and
   `tools/hosting/`, for the values in your local User Secrets with your own tool. Expected: no hit.

## Part B: hosted (Operator)

Prerequisites: an Azure subscription; the Azure CLI and the GitHub CLI signed in; PowerShell 7; admin rights on
the GitHub repository. B1 can run from the PR 1 branch; B2 onward needs PR 1 merged (the workflow file must be on
`master`). The steps are in `docs/operations/hosted-demo.md`.
Each step names the requirement it shows.

1. Provision (HP-007, HP-012, HP-014): run `tools/hosting/provision.ps1` with the resource names; enter the
   three values. Expected: an administrator email outside `example.com` is refused; the resources of ADR 0015
   exist; the environment `demo` exists with its variables and a branch policy allowing only `master`; the log
   retention reads back as 30 days; no secret value or administrator email was printed.
2. First deployment (HP-003, HP-004, HP-009, HP-017, SC-003, SC-005): Actions → "Deploy demo" → Run workflow,
   `mode` = `deploy`, `commit` = `master`. Expected: success within 15 minutes; the summary shows the full SHA,
   "succeeded" and the start time; every smoke check passed.
3. Browser (US1, US2, HP-001, HP-002, HP-008, SC-002): from a machine that never ran the app, open the published
   address and time the way from opening it to seeing a demo account's custom habits. Expected: under 3
   minutes; visitor Home over HTTPS; `http://` redirects; the catalog shows the seed habits; register
   and sign in works; each demo account shows its two custom habits and no other account's; the administrator
   reaches Admin and sees no private data.
4. Restart without a deployment (US1-AS6, US2-AS3, HP-018): sign in with "Keep me signed in"; run
   `az webapp restart`; reload. Expected: still signed in; the operator log after the restart shows demo
   loading reporting each demo account as already existing and the catalog as not empty, so it wrote nothing.
   Duplicates cannot arise in any case: catalog habit names and a user's custom habit names are unique (slice 2),
   so a `check` run's counts only confirm that nothing was lost.
5. Operator log (US3-AS3, HP-010): open the stored log through the web app's Kudu site (`LogFiles`, research
   R-09). Expected: after the start time of step 2's summary, administrator creation and demo-loading lines with
   emails and result texts only; before it, the faults of the starts without a database (contract). No page of
   the published address shows the log. At the first weekly check, the first deployment's lines are still
   there.
6. Refresh (US4, HP-011): as a demo account, change its password; as administrator, unpublish a seed habit;
   register an account. Run `mode` = `refresh`. Expected: success; the demo account signs in with the original
   demo password; the catalog is full again; the registered account is gone.
7. Failure and recovery (US3-AS4, US3-AS5, US4-AS5): rename `Operator__DemoPassword` in the app settings (the
   web app restarts). Expected: the app serves pages and the operator log shows "no demo password configured".
   Run a refresh. Expected: the run fails at step 3 with `configuration incomplete: Operator__DemoPassword`,
   the hosted demo is unchanged, no secret is in the log. Restore the setting and run a refresh: success.
   Next, set `Operator__DemoPassword` to a value that fails the password rule (for example one character) and run
   a refresh. Expected (HP-017, US4-AS5): steps 1–11 succeed, the smoke check fails `catalog` and every `demo`
   check, the run fails at step 12 and its summary names it, the operator log shows that the demo password does
   not meet the password rule. Restore the setting and refresh: success. Then stop the web app by hand and run
   `deploy`: it succeeds (step 6 starts the app before deploying).
8. Merge without deploying (US3-AS6, HP-016): merge any change to `master`. Expected: no run starts;
   `Deployment__Commit` is unchanged.
9. Secrets (SC-004, HP-007): with the values shown in the portal's app settings, search the repository, the
   downloaded logs of the runs in steps 2, 6 and 7, and the downloaded operator log. Expected: no password and
   no connection string; the administrator email only in the operator log.
10. Exam day and weekly (SC-001, SC-006, SC-007, SC-008): run `mode` = `check` on 2026-11-23 and at least once a
    week until 2026-12-31, each time after the app has had no requests for at least 30 minutes, and add runs
    until there are at least 10. Expected: every run passes; across those `check` runs (deployment and refresh
    runs measure a cold start and do not count, nor does a `first … (retried)` value), `first` is within 30
    seconds and `later` within 3 seconds in at least 9 of 10; the resource group's month-to-date cost stays
    within USD 25 a month.
