# Research: Demo hosting

**Feature**: [spec.md](spec.md) · **Plan**: [plan.md](plan.md) · **Date**: 2026-11-15 · Revised after
[critic-plan-pass-1.md](critic-plan-pass-1.md) and, for its MINOR notes, [critic-plan-pass-2.md](critic-plan-pass-2.md)

The four ARCHITECTURE §9 choices of this slice are owner answers (spec `## Clarifications`, plan session
2026-11-15) recorded in [ADR 0015](../../docs/adr/0015-demo-hosting.md). This file cites them and decides only
the details below them. Workflow step numbers refer to [contracts/deploy-workflow.md](contracts/deploy-workflow.md).

## R-01 Owner decisions this plan builds on

- **Decision**: cite, don't decide again:
  - the critic-revision decisions (Operator, secret boundary, HP-017 success rule, HP-018 sessions,
    `example.com` administrator, no extra seed content): confirmed;
  - services and budget: App Service B1 (Linux, Always On) and Azure SQL Database Basic, USD 25 a month;
  - deployment, migrations and refresh: one manual GitHub Actions workflow with OIDC, migration bundle, smoke
    check;
  - secrets and sessions: App Service app settings; Data Protection keys on App Service's persistent storage.
- **Rationale**: constitution, Decision Discipline. A plan must not choose an ARCHITECTURE §9 item.
- **Alternatives**: in ADR 0015, Options.

## R-02 The Azure resources and their settings

- **Decision**: one resource group with the resources of ADR 0015. The names are non-secret values the
  Operator chooses at provisioning and puts in the GitHub environment `demo`
  ([contracts/hosted-configuration.md](contracts/hosted-configuration.md)). Web app:
  - runtime stack `DOTNETCORE:10.0`; `httpsOnly` true; `minTlsVersion` 1.2; `alwaysOn` true;
    `webSocketsEnabled` true; one instance;
  - app settings `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` (R-08), `DataProtection__KeysDirectory` (R-07),
    `Deployment__Commit` (written by step 7);
  - container logging to the file system, kept 30 days (R-09);
  - the secret values of R-03.
  SQL: one logical server with SQL authentication, public network access on, firewall rules only for the web
  app's `possibleOutboundIpAddresses` (and, from step 5 to step 13, the runner); one Basic database.
- **Rationale**: lowest cost that meets HP-001, HP-002, HP-013, SC-001 and SC-006 (ADR 0015). Rules for the web
  app's possible outbound addresses are free and narrower than "allow all Azure services".
- **Alternatives**: Entra-only SQL authentication with the web app's managed identity. Every recreated database
  would need `CREATE USER ... FROM EXTERNAL PROVIDER` before the app starts, one more step in every run; the
  owner chose app settings. Rejected.

## R-03 Provisioning script and secret entry

- **Decision**: `tools/hosting/provision.ps1`, PowerShell 7 with the Azure CLI, run by the Operator after
  `az login`. Idempotent: each resource is created when missing and its settings are set again.
  - Parameters: subscription, region, resource group, plan, web app, SQL server and database names, GitHub
    repository (`owner/name`), and the switch `-RotateSqlPassword`. None of them is secret.
  - It first checks `az webapp list-runtimes --os linux` for `DOTNETCORE:10.0` and stops if the runtime is
    missing.
  - The administrator email, administrator password and demo password are read with `Read-Host -MaskInput`.
    The email is refused unless it ends in `@example.com` (HP-014). On a second run an empty answer keeps the
    stored value.
  - Every secret value reaches Azure through the Azure CLI's `@<file>` argument form: the value is written to a
    file in the user's temporary folder, passed as `--admin-password @<file>` or `--settings @<file>`, and the
    file is deleted in `finally`. No secret value is a command-line argument.
  - SQL administrator password: generated with `RandomNumberGenerator` when the server is created, set with
    `az sql server create --admin-password @<file>`, and stored only in the connection string app setting. On
    a later run the server exists and the connection string is kept unchanged. With `-RotateSqlPassword` the
    script generates a new password, sets it with `az sql server update --admin-password @<file>` and writes the
    new connection string; the Operator then starts a refresh.
  - It creates the Entra application registration, its service principal, a federated credential with subject
    `repo:<owner>/<name>:environment:demo`, and a Contributor role assignment on the resource group. With the
    GitHub CLI it creates the environment `demo` with a deployment branch policy that allows only `master`, and
    sets its variables (contracts/hosted-configuration.md). It prints no secret value or administrator email.
  - It sets the web app's file-system log retention (R-09) and reads the setting back.
  - No Azure budget alert: the Operator compares the cost with USD 25 at each weekly check (R-12).
  - The pieces that run without Azure are functions with a self-test, `tools/hosting/selftest.ps1`, in the
    style of `tools/pr-gates/selftest.ps1`: the `example.com` check (accepts `admin@example.com`, refuses
    `admin@example.org`, `admin@sub.example.com` and an empty value) and the secret-file helper (the file is
    gone after a normal return and after a thrown error). It must print `PASSED`.
  `tools/hosting/teardown.ps1` deletes the resource group, the application registration and the environment
  after 2026-12-31.
- **Rationale**: HP-007, HP-012 (set once, reused), HP-014, CLAUDE.md (no secrets in arguments or source
  control). PowerShell matches the repository's other tools (`tools/pr-gates`, `tools/guard-oracle`).
- **Alternatives**: Bicep or Terraform (a script still enters the secrets, and one more language for about ten
  resources); the Az PowerShell module's `PSCredential` parameters (a second Azure toolset next to the CLI the
  workflow uses). Rejected.

## R-04 The deployment workflow

- **Decision**: `.github/workflows/deploy-demo.yml` as in its contract: 13 numbered steps, modes `deploy`,
  `refresh` and `check`.
  - **Ref.** Step 1 fails on any ref but `master`, and the environment's branch policy gives other refs no Azure
    token: GitHub lets a run start against any branch with that branch's file, once the file is on the default
    branch.
  - **Order.** Check the ref and sign in (1) before anything is read from Azure; step 2 checks out `master` with
    its history before `git merge-base` runs. Every secret is read and masked in step 3,
    before any step that changes something; a missing value stops the run there with the key's name, and the
    hosted demo stays as it was. The database is deleted (5) before the new version is deployed (7), and the
    web app is stopped (8) only after the deployment, before the database exists again (9). So no version
    serves against a partial schema, and nothing is deployed into a stopped web app: a zip deployment into a
    stopped App Service app on Linux is not documented (its deployment endpoint is a second container).
  - **Re-running after a failure.** Step 5 skips a missing database, steps 6 and 8 are no-ops in the target
    state, and step 6 starts a web app that an earlier failure left stopped. Step 7 clears
    `Deployment__Commit` before it deploys and sets it after, so a refresh never migrates for code it cannot
    name. The firewall rule has a fixed name and is removed in step 13 (`always()`). The next run is therefore
    the recovery for every failure state the contract lists (HP-017, US3-AS4).
  - **Firewall delay.** Azure documents that a new server firewall rule can take up to five minutes to apply.
    The runner's rule is added in step 5, several minutes before the bundle connects in step 10, and the bundle
    is retried up to 3 times, 60 seconds apart, when it cannot connect. A failed connection applies nothing, and
    EF Core records each applied migration, so a retry continues where the last attempt stopped.
  - **Commit check**: `git merge-base --is-ancestor <sha> origin/master`.
  - **Masking**: `::add-mask::` for each value right after it is read, the administrator email included (the
    run log is public).
  - **Summary**: mode, full SHA, result, failed step, and the UTC time of step 11's start (HP-009).
- **Rationale**: HP-004, HP-005, HP-009, HP-011, HP-012, HP-016, HP-017, SC-003. Estimated `deploy` time:
  build 3–4 minutes, database delete and create 2–3, zip deploy 1–2, migrations under 1, start and smoke check
  1–2: about 10 minutes. With all three bundle retries (3 minutes more) it stays within SC-003's 15.
- **Alternatives**: deploying into the stopped web app (undocumented on Linux); deployment slots (need
  Standard, outside the budget); emptying the database by reverting all migrations (depends on every `Down`
  method; deleting guarantees an empty database, HP-005); letting the smoke check report missing configuration
  (it would restart the demo without the value first, and give a usage error instead of a reason). Rejected.

## R-05 Migration bundle

- **Decision**: the existing tool manifest `dotnet-tools.json` already pins `dotnet-ef` 10.0.15, the version of
  the EF Core packages. Step 4 runs `dotnet tool restore` and, with the startup project that slices 1 and 2 use
  for migrations, `dotnet ef migrations bundle --project src/HT.Persistence --startup-project src/HT.Persistence
  --self-contained -r linux-x64 --output efbundle`. The bundle gets the connection from the design-time
  factory's `HT_DESIGN_CONNECTION` environment variable, set only on step 10 from the masked value. Its exit
  code decides step 10.
  The bundle is tried before Azure exists: quickstart A3 builds it for the local platform and applies it to an
  empty local database, and the implementation tasks include that run, so HP-004's mechanism is not first run
  in the first deployment.
- **Rationale**: HP-004; `HtDbContextFactory` already reads that variable, so no new code, and the connection
  is not an argument. The pinned tool keeps the bundle's EF version equal to the app's. The context, the
  factory and the migrations are internal, which `migrations add` already handles; the local trial confirms the
  same for `migrations bundle`.
- **Alternatives**: `dotnet ef database update` on the runner (needs the SDK and source at apply time, same
  result); an idempotent SQL script run with `sqlcmd` (one more tool); migrations at start (rejected in ADR
  0015).

## R-06 Smoke check

- **Decision**: a console project `tools/HT.SmokeCheck` in `HabitTracker.slnx` under a `/tools/` folder, built
  with warnings as errors. It references `HT.Application` only, for the expected data
  (`DemoDataService.DemoEmails`, `DemoCustomHabits.For`, `SeedCatalog.Habits`). Its checks are public types
  (`SmokeCheck`, `PageReader`) that take a client factory, so host tests run them in process
  ([contracts/smoke-check.md](contracts/smoke-check.md)). Checks:
  1. `home` (with warm-up retries);
  2. `https-redirect`;
  3. `catalog`: every Published seed habit name is on `/catalog`;
  4. `administrator`: sign in and reach `/admin`;
  5. `demo <email>`: sign in and find every demo custom habit name of that account on `/custom-habits`.
  Each sign-in uses a new client with empty cookies. Checks pass when all expected items are present, so
  reviewers' additions don't fail them. Names are compared in memory and never printed. Timings are reported,
  not judged.
  **Coverage rule** (one definition, in the contract): all demo data is written through the port
  `IDemoDataStore` (ADR 0013). A test reflects over the port and fails when a method or a data parameter (name
  and type) has no entry in the public map `SmokeCheckCoverage`. Today: `CreateDemoAccountAsync` (`email`,
  `password`, `customHabits`) and `TryCreateSeedCatalogAsync` (`habits`). A slice that adds demo data, private
  data included, adds a method or a parameter to the port or changes a parameter's type, so its tests fail until
  it extends the smoke check. The earlier idea of matching result types was dropped: a slice can report new
  demo data through the existing `DemoAccountResult` without a new type, but it cannot write it without the
  port. Slices that ship before this slice is implemented are covered by the tasks gate (plan, Gates), because
  the test does not exist yet.
- **Rationale**: HP-017 and SC-005 need a check of the startup tasks' outcome that the workflow can run without
  reading the operator log (HP-010; the run log is public). Expected values come from the code that seeds them,
  and the coverage test turns "extend the check" into a failing test (constitution V). HTTP checks follow how
  the host tests already sign in (`HostClient`).
- **Alternatives**: a health endpoint reporting startup results (new public surface next to private operator
  data, HP-010); reading the operator log in the workflow (public run log); Playwright (new dependency for a few
  GETs and one POST); equal counts (fail on reviewers' additions, and a missing and an extra item cancel out).
  Rejected.

## R-07 Data Protection keys across restarts

- **Decision**: `Program.cs` reads `DataProtection:KeysDirectory`; when it is set, it calls
  `AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(value)).SetApplicationName("HabitTracker")`.
  When unset (local, tests), nothing changes. The hosted value is `/home/data-protection-keys`.
- **Rationale**: HP-018. App Service on Linux documents no automatic key persistence; the framework's Linux
  default is `$HOME/.aspnet/DataProtection-Keys`, which depends on the platform's `HOME`. An explicit directory
  on `/home`, App Service's persistent storage, does not. The owner's option (App Service's own storage, no new
  resource) made explicit in one setting; ADR 0015 records it.
- **Alternatives**: rely on the default (undocumented on Linux); keys in the database (rejected by the owner);
  Blob Storage with Key Vault (new resources). Rejected.

## R-08 HTTPS behind App Service

- **Decision**: HTTPS only at App Service redirects `http://` (HP-002). `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`
  lets the host take the `https` scheme from `X-Forwarded-Proto`, so redirects, cookies and `UseHsts` see
  HTTPS. `UseHttpsRedirection` stays; with no HTTPS port in the container it does nothing. The session cookie
  keeps `CookieSecurePolicy.Always`.
- **Rationale**: no code change; the framework's switch for proxies.
- **Alternatives**: `UseForwardedHeaders` in code with known proxies (App Service's proxy addresses are not
  fixed). Rejected.

## R-09 Operator log

- **Decision**: the host's console logger is the operator log. With container logging to the file system
  (`az webapp log config --docker-container-logging filesystem`), App Service on Linux writes the container's
  output to files under `/home/LogFiles` and shows it live in Log stream.
  - **Keeping it 30 days** (longer than the weekly check interval): `az webapp log config` has no retention
    parameter. The provisioning script sets the retention in the web app's log settings (the `config/logs`
    resource, file-system retention in days and size quota) and reads it back. If the platform ignores the
    retention for container logs, the files are kept until the size quota (35 MB by default) is reached; at a
    few lines per start that is far more than 30 days. Quickstart B5 checks that the first deployment's lines are
    still there at the first weekly check.
  - **Reading it**: live with `az webapp log tail` or the portal's Log stream; stored files through the web
    app's Kudu site (`https://<app>.scm.azurewebsites.net`, Azure sign-in), folder `LogFiles`, or its
    `api/logs/docker` endpoint. `az webapp log download` is documented as possibly not working on Linux, so the
    Operator documentation names Kudu as the way to the stored log. All of these need an Azure sign-in.
  - The workflow never prints it. In `deploy` mode the entries before the start time
  in the run summary come from the starts without a database (contract, "Startup tasks before the database
  exists"). No Application Insights.
- **Rationale**: HP-010, US3-AS3, Edge Cases (a fault at an unplanned restart is found by the next weekly check
  and must still be in the log). Alerting and dashboards are out of scope (spec Assumptions).
- **Alternatives**: Application Insights or Log Analytics (cost, and a second store to keep free of private
  data); 3 days (shorter than the weekly interval); `az webapp log download` as the only way to the stored log
  (Linux caveat). Rejected.

## R-10 Operator documentation

- **Decision**: `docs/operations/hosted-demo.md`, linked from `README.md`. It covers: provisioning and secret
  rotation; the GitHub environment; starting `deploy`, `refresh` and `check` runs; reading the run summary and
  the operator log, including the entries before the final start; the weekly and exam-day checks (`check` mode,
  SC-006) and the cost check (SC-007); the SC-004 search; the items the exam submission gives reviewers
  (published address, demo emails, demo password, administrator email, administrator password), with only the
  address and demo emails written out; teardown after 2026-12-31. The check history is the workflow's run
  history, not a file, so the weekly checks need no commits.
- **Rationale**: HP-015.
- **Alternatives**: a README section (too long); a committed check log (a commit per week). Rejected.

## R-11 Tests and what only the hosted app can show

- **Decision**:
  - **Test hosts and their schema.** Every new factory is a `WebApplicationFactory<Program>` of its own in
    environment `Testing`, like the shared host (slice 1 R-16): no User Secrets, no migrations at start, every
    value through `UseSetting`. Its database is the host-test connection string with the database name replaced,
    as `StartupTasksHarness` does for `HT-UI-MIGRATE`, so CI needs no extra variable. The schema comes from
    `HtDatabase.ResetAsync` (drop, then all migrations) before the host is built, as `HostFixture` does; the
    startup tasks then seed it at the host's start. ADR 0006 gives host tests the database `HT-UI`; these
    databases are derived from it on the same server for the same reason `HT-UI-MIGRATE` is (they need settings
    or a database state the shared host cannot have), and the shared host's tests are untouched.
  - **Smoke check, in process** (`SmokeCheckHostTests`, database `HT-UI-SMOKE`, reset before each host): all
    checks pass on a freshly seeded host; a host with a demo password that fails the password rule (no demo
    accounts and no seed catalog, `DemoDataService.LoadAsync`) fails `catalog` and every `demo` check; a host whose
    demo account lost a custom habit (deleted through the store) fails that account's check only. Check 2 is
    skipped. Own xUnit collection, parallelization off.
  - **Session keys** (`DataProtectionKeysTests`, database `HT-UI-KEYS`): one reset, then two hosts in turn over
    that database (the second without a reset, so the account still exists) and the same temporary key
    directory: a sign-in on the first writes a key file there, and the second accepts the first one's session
    cookie (HP-018). A host without the setting writes nothing there. Same collection rules.
  - **Unit tests** (`HT.UiTests`, which already references `HT.Ui` and gets the tool reference): `PageReader`
    on fixed HTML (token, handler, names after decoding, linked stylesheets and scripts);
    `SmokeCheckCommand.RunAsync` for arguments and environment (exit code 2 for each missing value, no secret in
    the output); the coverage test of R-06. `HT.UnitTests` stays limited to Domain and Application (ADR 0006).
    The tool's `Program` stays internal with no `InternalsVisibleTo`, so `HT.UiTests` sees only `HT.Ui`'s
    `Program`.
  - **Provisioning self-test**: `pwsh tools/hosting/selftest.ps1` prints `PASSED` (R-03); run with the suite,
    like the other tool self-tests in CLAUDE.md.
  - **Coverage in Sonar**: `.github/workflows/sonar.yml` excludes `tools/**` from coverage, which ADR 0001
    justifies for scripts. The exclusion is narrowed to the script folders (`tools/pr-gates/**`,
    `tools/guard-oracle/**`, `tools/hosting/**`, `.claude/hooks/**`), so the smoke check, the .NET code that
    decides HP-017, counts in the PR gate's coverage. ADR 0001's rule (scripts excluded) is unchanged.
  - **Architecture test** (`HT.ArchitectureTests`): `HT.SmokeCheck` depends on `HT.Application` and `HT.Domain`
    only, not on `HT.Persistence` or `HT.Ui` (CLAUDE.md: layer rules are enforced by tests).
  - **Migration bundle**: quickstart A3, run during implementation (R-05).
  - **Not testable before Azure exists**: the workflow, the Azure settings, forwarded headers and the timings
    (SC-001, SC-003). They are checked by quickstart Part B and reported as unverified until then
    (constitution V).
- **Rationale**: ADR 0006, constitution V.
- **Alternatives**: `act` to run the workflow locally (needs Docker and still Azure). Rejected.

## R-12 Cost

- **Decision**: expected about USD 18 a month (B1 Linux about 13, SQL Basic about 5). The Operator compares the
  resource group's cost in Cost Management with USD 25 at each weekly check. No budget alert resource.
- **Rationale**: SC-007; one fewer prompt and resource.
- **Alternatives**: an Azure budget with an email alert (needs an email address in configuration). Rejected.

## R-13 Time-relative demo data and later slices

- **Decision**: none had shipped when this slice was planned (slices 1 and 2). Nothing is built for it. A
  later slice that adds time-relative demo data or scheduled evaluation decides how it stays current on the
  hosted demo until 2026-12-31, citing ADR 0015 (spec Edge Cases, ADR 0007). Its demo data is still covered by
  the smoke check through the coverage rule (R-06).
- **Rationale**: constitution VI (no infrastructure for hypothetical needs).
- **Alternatives**: a scheduled refresh now; nothing would use it. Rejected.

## R-14 One account lookup per request of a signed-in user

- **Decision**: accept slice 1's handover (slice 1 research R-09): every request of a signed-in user, static
  assets included, does one security-stamp lookup by key and gets `no-store` headers. On the hosted demo that
  lookup goes to the 5-DTU database. The app's router is not interactive; interactivity is set per page, so
  every page navigation is a new HTTP request with its stylesheets and scripts, each with a lookup, and an
  interactive page opens a new circuit. A lookup by primary key on a table of a few accounts takes
  milliseconds, so a page with its few assets stays well under 3 seconds by estimate. No change in this slice.
  The estimate is measured: the smoke check's `later` timing is exactly that case (a signed-in page with its
  linked assets, no cache; contract), reported at every run. If SC-001's 3 seconds are missed, the stamp
  interval is the first thing to revisit, in a change of its own (it also keeps Back after sign-out safe,
  slice 1 R-09).
- **Rationale**: SC-001; the interval is part of slice 1's session rules (HP-007), which this slice does not
  change.
- **Alternatives**: a longer `ValidationInterval` (changes HP-007's immediate sign-out of other sessions);
  caching static assets outside authentication (a pipeline change in slice 1's area). Rejected for now.

## R-15 Dates and pull requests

- **Decision**: R2 is the owner's answer: built at the end, after the product slices, starting no later than
  2026-11-20; SC-008 needs the hosted demo before 2026-11-23. This plan does not move the start: it is written
  now and parked like the spec, and the owner starts the implementation on any day up to 2026-11-20 (an earlier
  start only adds margin). The sequence for the latest start:
  - 2026-11-20: `/speckit-tasks`, work-order steps 1–7, PR 1 opened (roadmap row `implementing`, the rule for a
    slice with several PRs). Provisioning (step 8's first half) needs no merge: the Operator runs the script
    from the branch the same day;
  - 2026-11-21: the PR gates (Sonar, CodeQL), the owner's review and merge; the first deployment right after
    the merge (the workflow file must be on `master`), then quickstart Part B;
  - 2026-11-22: margin for one fix PR and a second deployment;
  - PR 2: the results of Part B, any fix, the roadmap row `shipped` with both PRs listed;
  - 2026-11-23: the exam-day `check` run (SC-006).
  The slice is small (one production line, one tool, two scripts, one workflow, one document), which is what
  makes one day of implementation realistic. If PR 1 is not merged by 2026-11-22, SC-008 is missed; the risk
  table names it, and the owner can start earlier to avoid it.
- **Rationale**: SC-008, R2, roadmap "Updating this file" (several PRs), constitution V (the PR names what is
  unverified).
- **Alternatives**: one PR with hosted results recorded afterwards (the PR would claim `shipped` with every
  hosted requirement unverified); a plan that starts the slice before the product slices (departs from the
  owner's R2 answer). Rejected.
