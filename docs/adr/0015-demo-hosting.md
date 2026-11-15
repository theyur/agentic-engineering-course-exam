# 0015 — Hosting, deployment, migration rollout, demo refresh and secrets of the hosted demo

**Status:** Accepted, 2026-11-15. Owner answers in slice 3, `specs/003-demo-hosting` (`## Clarifications`,
plan session 2026-11-15). Settles the four ARCHITECTURE §9 rows of the roadmap that slice 3 owns: hosting
services and secrets, schema migration rollout, demo refresh, and how the hosted app gets the demo sign-ins.
Leaves [ADR 0007](0007-startup-operator-tasks.md) unchanged: the host still applies no migrations outside
Development.

## Context

- SPEC §10 and ARCHITECTURE §7: reviewers open an Azure-hosted app in a browser, with demo data, at low cost; no
  redundancy, autoscaling or disaster recovery (SPEC §9).
- Slice 3's owner answers: the Operator starts each deployment with one action and a merge never deploys
  (HP-016); every deployment and every Operator-started refresh starts the hosted app on an empty database
  (HP-005, HP-011); a deployment or refresh succeeds only when the startup tasks created the administrator and
  all demo data (HP-017); sessions survive a restart that is not a deployment or refresh (HP-018); the hosted
  demo stays reachable until 2026-12-31.
- ADR 0007: administrator creation and demo loading run at every start, are non-fatal, and log their results to
  the operator log. Outside Development the host applies no migrations.
- The repository is public, so GitHub Actions run logs are public.
- On Linux, App Service documents no automatic persistence of the ASP.NET Core Data Protection keys (the
  documented `%HOME%\ASP.NET\DataProtection-Keys` behaviour is described for App Service in general, with a
  Windows path; the framework's Linux default is `$HOME/.aspnet/DataProtection-Keys`). Without persisted keys, a
  restart would end every session.

## Options

Services and budget:

1. **App Service Basic B1 (Linux, Always On) and Azure SQL Database Basic (5 DTU), budget USD 25 a month
   (chosen).** About USD 18 a month; nothing sleeps, Blazor's WebSocket connections are not capped.
2. App Service Free F1 and the Azure SQL free offer, USD 0. F1 allows 60 CPU minutes a day and few WebSocket
   connections; the free database auto-pauses and can take about a minute to resume, against SC-001's 30
   seconds. Rejected.
3. Azure Container Apps (consumption) and the SQL free offer, about USD 5. Scale to zero and cold starts, plus a
   container image and registry to maintain. Rejected.

Deployment, migrations and refresh:

1. **A manual GitHub Actions workflow (chosen).** One `workflow_dispatch` run per deployment or refresh, signed
   in to Azure with OpenID Connect; it empties the database, deploys, applies migrations with an EF Core
   migration bundle while the app is stopped, starts it and runs the smoke check. The run shows the commit and
   the result. (The owner's answer listed the deployment after the migrations; the Decision deploys first,
   because deploying into a stopped web app on Linux is not documented. The owner's intent, no start against a
   partial schema, is unchanged.)
2. A local PowerShell script run by the Operator. The result lives only on the Operator's console. Rejected.
3. The host applies migrations at start outside Development. Changes ADR 0007, and a failed migration would need
   a special case to stop serving. Rejected.

Secrets and sessions:

1. **App Service app settings, and Data Protection keys on App Service's persistent storage (chosen).** No new
   Azure resource.
2. Azure Key Vault references and keys in the app's database (new table and package). Same result for this demo,
   more resources and code. Rejected.

## Decision

- **Azure resources.** One resource group in one region the Operator picks, holding:
  - an App Service plan, Linux, Basic B1;
  - one web app on the .NET 10 runtime, on its default `*.azurewebsites.net` address, with HTTPS only, minimum
    TLS 1.2, Always On, WebSockets on, one instance, and container logging to the App Service file system,
    kept 30 days;
  - one Azure SQL logical server with SQL authentication, and one database at the Basic tier;
  - server firewall rules for the web app's possible outbound addresses only (not "allow all Azure services").

  The budget of SC-007 is **USD 25 a month**. The Operator checks the actual cost at each weekly smoke check.
  The resource group is deleted after 2026-12-31.
- **Provisioning.** A PowerShell script in the repository creates or updates the resources with the Azure CLI.
  It is idempotent and run by the Operator once, and again after a change. It reads the secret values from
  prompts, never from arguments or files in the repository, and refuses an administrator email outside
  `example.com` (HP-014). It also creates the Entra application, with a
  federated credential, that the deployment workflow uses, with Contributor on the resource group only.
- **Deployment and refresh.** A GitHub Actions workflow with only a `workflow_dispatch` trigger, two inputs (the
  mode `deploy`, `refresh` or `check`, and a commit of `master` for `deploy`) and one concurrency group, in the
  GitHub environment `demo`. It signs in to Azure with OpenID Connect, so no Azure credential is stored in
  GitHub; the environment allows only `master`. Its steps, numbered in `specs/003-demo-hosting/contracts/deploy-workflow.md`:
  1. stop unless the run is on `master`, then sign in to Azure;
  2. check out `master` and check that the commit is on it (deploy), or read the deployed commit from the web app;
  3. read the secret values from the web app and mask them; stop when one is missing, before any change;
  4. build that commit's smoke check, publish output and self-contained EF Core migration bundle;
  5. open a firewall rule for the runner's address, then delete the database if it exists, so no version serves
     requests against it;
  6. deploy only: start the web app if a failed run left it stopped;
  7. deploy only: zip-deploy the new version and record the commit in an app setting;
  8. stop the web app;
  9. create the database again, empty, at the Basic tier;
  10. run the migration bundle, retrying while the new firewall rule takes effect;
  11. start the web app, so administrator creation and demo loading run on the full schema (ADR 0007);
  12. run the smoke check, which decides success (HP-017);
  13. always: remove the firewall rule, and write the mode, the commit, the result and the start time to the run
      summary.

  A failed step fails the run, and the web app is left as the failed step left it: stopped, or without a
  database. It never serves against a partial schema (HP-004). Every step tolerates what a failed run leaves,
  so the next run is the recovery. A refresh skips steps 6 and 7; `check` runs only steps 1 to 4, 12 and 13.
  No application behaviour is added, so slice 3 stays off the acceptance-test surface (ADR 0003).
- **Migration rollout.** The workflow applies the deployed commit's EF Core migrations to the new, empty
  database, while the web app is stopped, before the startup tasks run. ADR 0007 is unchanged.
- **Success detection.** A smoke check in the repository, a .NET console tool that runs over HTTPS like a
  browser, decides whether a deployment or refresh succeeded. It checks:
  - the visitor Home is served, and an `http://` request is redirected to `https://`;
  - the catalog shows every Published seed habit;
  - the configured administrator signs in and reaches Admin;
  - each demo account signs in with the demo password and shows its demo data of every shipped slice.

  It takes the expected demo data from `HT.Application`, and a test fails when the demo data port
  (`IDemoDataStore`, ADR 0013) writes data the check does not cover, so a slice that extends demo loading must
  extend the check. It prints
  emails of demo accounts and counts only. It never prints a password, the administrator email, a
  custom-habit name or a note. The Operator runs it weekly and on 2026-11-23 through the workflow's `check`
  mode (SC-006).
- **Secrets.** The connection string `HabitTracker` and the settings `Operator:AdministratorEmail`,
  `Operator:AdministratorPassword` and `Operator:DemoPassword` are App Service app settings and a connection
  string (encrypted at rest, set by the provisioning script). The workflow reads the connection string and the
  passwords from the web app at run time and masks them in the log. GitHub holds only non-secret identifiers as
  environment variables. The administrator email is masked too, because the run log is public.
- **Sessions across restarts.** The host stores its Data Protection keys in the directory named by the setting
  `DataProtection:KeysDirectory` when it is set. On the hosted app that is `/home/data-protection-keys`, on App
  Service's persistent storage, so a restart keeps sessions (HP-018). Every deployment and refresh empties the
  database, so the sessions of that run's accounts end anyway: an account no longer exists. Locally the setting
  is unset and the framework default applies.
- **Forwarded headers.** The web app sets `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`, so the host sees the
  `https` scheme of the App Service front end. HTTPS only at App Service gives the redirect of HP-002.
- **Operator log.** The host's console log, on the App Service file system, kept 30 days (or until its size
  quota, far longer at demo volume), read live with `az webapp log tail` or Log stream and stored through the
  web app's Kudu site, all behind an Azure sign-in. The workflow never prints it.
- **Not decided here.** Time-relative demo data and scheduled evaluation (slice 5 onward) did not exist when this
  ADR was accepted. The slice that adds them decides, citing this ADR, how they stay current until 2026-12-31,
  and supersedes the relevant part if it needs a different mechanism.

## Consequences

- One action and about 10 minutes per deployment or refresh. The hosted demo is unavailable for those minutes,
  and after a failed run until the next successful one.
- In deploy mode the new version starts twice without a database before the final start, and its startup tasks
  log faults for those starts. The run summary gives the time of the final start, after which the operator
  log shows the real results.
- Reviewers' accounts and changes last until the next deployment or refresh, as the spec intends.
- The connection string and the passwords exist in one place, the web app's settings. Changing one means
  running the provisioning script again, then a refresh.
- The smoke check depends on page markup: the sign-in form, habit card links, and Admin's address. A slice
  that changes them changes the check in the same change, and the check's host test catches a miss.
- The runner's address is looked up through a public "what is my IP" service and opened for one step only.
- Keys on the file system are not encrypted at rest. They protect only demo sessions, and the storage is
  private to the web app.
