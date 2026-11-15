# Implementation Plan: Demo hosting

**Branch**: `claude/speckit-specify-slice-3-b938fd` | **Date**: 2026-11-15 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/003-demo-hosting/spec.md`, with the owner answers of the plan session
(2026-11-15) recorded in its `## Clarifications` and in [ADR 0015](../../docs/adr/0015-demo-hosting.md). No
acceptance criteria: the slice is off the AC surface (spec "What acceptance tests can reach", ADR 0003). Revised
after [critic-plan-pass-1.md](critic-plan-pass-1.md) and, for its MINOR notes,
[critic-plan-pass-2.md](critic-plan-pass-2.md).

## Summary

The existing app is hosted on Azure App Service (Basic B1, Linux, Always On) with an Azure SQL Basic database,
for at most USD 25 a month until 2026-12-31. The Operator provisions it once with a PowerShell script that takes
the secret values from prompts and stores them as App Service settings. Each deployment, demo refresh and
weekly check is one manual run of a GitHub Actions workflow in mode `deploy`, `refresh` or `check`. The run
signs in with OpenID Connect and masks the secrets it reads. In `deploy` and `refresh` it deletes the database,
deploys the chosen `master` commit (deploy only), stops the app, creates the database again, applies that
commit's migrations with an EF Core migration bundle, and starts the app, so the ADR 0007 startup tasks seed it.
A smoke check then decides success: a small .NET tool that signs in like a browser and takes its expected demo
data from `HT.Application`, with a test that fails when the demo data port gains data the check doesn't
cover. Every step tolerates what a failed run leaves, so the next run is the recovery. The only production change
is one optional host setting that keeps the session keys on App Service's persistent storage, so an idle restart
keeps sessions. No domain type, use case, page or migration is added.

## Technical Context

**Language/Version**: C# on .NET 10 (`net10.0`), warnings as errors (`Directory.Build.props`); PowerShell 7 for
the Operator scripts; GitHub Actions YAML.

**Primary Dependencies**: ASP.NET Core 10 Data Protection (shared framework, no new package); EF Core 10 migration
bundle through the pinned `dotnet-ef` 10.0.15 (`dotnet-tools.json`); Azure CLI; `azure/login` with OpenID Connect.
Research [R-02](research.md#r-02-the-azure-resources-and-their-settings) to
[R-08](research.md#r-08-https-behind-app-service).

**Storage**: Azure SQL Database Basic, recreated empty at every deployment and refresh; schema unchanged. Session
keys as files in `/home/data-protection-keys` ([data-model.md](data-model.md)). New local test databases
`HT-UI-SMOKE` and `HT-UI-KEYS`, derived from `HT_UI_CONNECTION` ([R-11](research.md#r-11-tests-and-what-only-the-hosted-app-can-show)).

**Testing**: xUnit host tests with factories of their own on the two new databases; unit tests for the smoke
check's page reading, invocation and coverage; one ArchUnitNET rule for the new project; a self-test of the
provisioning script's offline parts; a local run of the migration bundle; the hosted scenarios of
[quickstart.md](quickstart.md) Part B, run by the Operator.

**Target Platform**: Azure App Service on Linux (.NET 10 runtime), reached from current desktop browsers over HTTPS
on the default `*.azurewebsites.net` address; CI on `ubuntu-latest`.

**Project Type**: web application hosting and operations, over the existing layered solution.

**Performance Goals**: SC-001 (first page within 30 s after idling, later pages within 3 s, 9 of 10), reported by
every smoke check run ([R-14](research.md#r-14-one-account-lookup-per-request-of-a-signed-in-user)); SC-003
(deployment within 15 minutes, about 10 expected).

**Constraints**: no secret value in the repository, arguments, run log, operator log or error pages (HP-007); the
run log is public, so the administrator email is masked too; Operator-started runs only (HP-016); never serve
against a partial schema (HP-004); USD 25 a month (SC-007); hosted until 2026-12-31; slice started by 2026-11-20
(R2) and reachable before 2026-11-23 (SC-008), dates in [R-15](research.md#r-15-dates-and-pull-requests).

**Scale/Scope**: one instance, a handful of concurrent reviewers; 1 production file changed (`Program.cs`); 1 new
tool project; 2 Operator scripts; 1 workflow with 3 modes; 1 Operator document; 2 PRs.

## Constitution Check

*GATE: checked before Phase 0, after the owner answers closed every marker, after Phase 1, and after the revision
for critic plan pass 1.*

| Principle | Status | Evidence |
|---|---|---|
| Decision Discipline | met | The four ARCHITECTURE §9 markers and the critic-revision confirmation are owner answers of 2026-11-15, in spec `## Clarifications` and ADR 0015 (hard to reverse, spans slices). No marker remains. Research cites them (R-01) and decides only details below them. |
| I. Ubiquitous Domain Language | met | "Deployment", "Hosted demo", "Demo refresh" and "Operator" are in `CONTEXT.md`. No new domain term. The smoke check's output uses "catalog", "administrator", "demo" and "custom habits" as defined. |
| II. Deterministic, Reproducible Evaluation | met (not touched) | No evaluation logic. The new host code reads configuration, not the clock. |
| III. Privacy Enforced Below the UI | met | No new data path. The smoke check signs in as each account and reads only that account's page; it compares habit names in memory and prints counts and demo emails only (contract). The operator log is unchanged and stays behind an Azure sign-in (R-09). The administrator gets no bypass (HP-008). |
| IV. Design-System Fidelity and Accessible UI | met (not touched) | No page, component or text changes. |
| V. Verified, Not Asserted | met, with a labelled gap | Host, unit and architecture tests, the provisioning self-test and a local run of the migration bundle, for everything that runs without Azure (R-03, R-05, R-11); the smoke check counts in Sonar's coverage. The coverage test over the demo data port is the enforcing mechanism for demo-data drift (R-06). The workflow, the Azure settings and the timings are verified only by quickstart Part B, reported in PR 2; until then they are labelled unverified (R-15). |
| VI. Demonstrable Vertical Slices | met | Makes every shipped slice's demo data reachable for reviewers, checked by the coverage rule. No infrastructure for hypothetical needs: no slots, scaling, monitoring stack, budget resource or scheduled refresh (R-12, R-13). The administrator email must be `example.com` (HP-014). |

No exception, so Complexity Tracking stays empty.

## Project Structure

### Documentation (this feature)

```text
specs/003-demo-hosting/
├── spec.md
├── plan.md                         # this file
├── research.md                     # R-01 … R-15
├── data-model.md                   # no schema change; operational records; database states
├── quickstart.md                   # Part A local, Part B hosted (Operator)
├── contracts/
│   ├── deploy-workflow.md          # the only step numbering
│   ├── smoke-check.md
│   └── hosted-configuration.md
├── checklists/requirements.md
├── critic-plan-pass-1.md
└── tasks.md                        # /speckit-tasks, not created here
docs/adr/0015-demo-hosting.md       # the owner's hosting decisions
```

### Source Code (repository root)

```text
src/HT.Ui/Program.cs                              # changed: optional DataProtection:KeysDirectory (R-07)
tools/HT.SmokeCheck/                              # new console project, in HabitTracker.slnx under /tools/
├── HT.SmokeCheck.csproj                          # references src/HT.Application only
├── Program.cs                                    # internal: calls SmokeCheckCommand.RunAsync
├── SmokeCheckCommand.cs                          # public: arguments, environment, exit codes (contract)
├── SmokeCheckOptions.cs                          # public: SmokeCheckOptions, SmokeCheckParseResult
├── SmokeCheckRunner.cs                           # public: the checks over a client factory (contract)
├── SmokeCheckResult.cs                           # public: check lines, timings, Passed
├── PageReader.cs                                 # public: antiforgery token, handler, names
└── SmokeCheckCoverage.cs                         # public: SmokeCheckCoverage, CoverageEntry: IDemoDataStore parameters, checks (R-06)
tools/hosting/
├── HostingFunctions.ps1                          # the offline parts: example.com check, secret-file helper, parse check
├── provision.ps1                                 # Operator, once, after changes, and for rotation (R-03)
├── teardown.ps1                                  # after 2026-12-31
└── selftest.ps1                                  # tests HostingFunctions.ps1, parses both scripts; prints PASSED
.github/workflows/deploy-demo.yml                 # workflow_dispatch only, modes deploy/refresh/check (contract)
.github/workflows/sonar.yml                       # coverage exclusion narrowed to the script folders (R-11)
docs/operations/hosted-demo.md                    # Operator documentation (HP-015, R-10)
README.md, CLAUDE.md                              # layout: tools/HT.SmokeCheck; Operator document; selftest command
tests/HT.UiTests/HT.UiTests.csproj                # + reference to tools/HT.SmokeCheck
tests/HT.UiTests/Host/DerivedDatabaseHost.cs      # host factory on a derived database (HT-UI-SMOKE, HT-UI-KEYS)
tests/HT.UiTests/Host/SmokeCheckHostTests.cs      # own factory, database HT-UI-SMOKE
tests/HT.UiTests/Host/DataProtectionKeysTests.cs  # two factories, database HT-UI-KEYS
tests/HT.UiTests/SmokeChecks/PageReaderTests.cs
tests/HT.UiTests/SmokeChecks/InvocationTests.cs
tests/HT.UiTests/SmokeChecks/RunnerBehaviourTests.cs   # scripted site, no network
tests/HT.UiTests/SmokeChecks/ScriptedHandler.cs        # fake HttpMessageHandler for RunnerBehaviourTests
tests/HT.UiTests/SmokeChecks/CoverageTests.cs
tests/HT.ArchitectureTests/SmokeCheckDependencyTests.cs   # + HT.SmokeCheck in the architecture model
```

**Structure Decision**: production code stays in the four `src/` projects; the only production change is in
`HT.Ui`'s composition root. Operational tooling goes under `tools/` like `tools/pr-gates`; the smoke check is a
.NET project so it can reference `HT.Application` for the expected demo data and be tested in process. Its tests
live in `HT.UiTests`, which already holds the host tests; `HT.UnitTests` keeps its Domain and Application scope
(ADR 0006). `CLAUDE.md` and the README's layout name the new folder, and an architecture test enforces its
dependencies.

## Requirements to mechanisms

Quickstart step ids: A1–A4 local, B1–B10 hosted. Workflow steps: the contract's numbers.

| Requirement | Mechanism | Verified by |
|---|---|---|
| HP-001, SC-002 | web app on its default address (R-02) | B3 |
| HP-002 | HTTPS only, TLS 1.2, forwarded headers (R-08) | smoke check 2; B3 |
| HP-003 | the deployed commit's publish output, environment Production | B2 summary; B3 |
| HP-004 | steps 5–11: database deleted, app stopped, bundle on the new empty database before the final start (R-04, R-05) | A3; B2 |
| HP-005, HP-011 | steps 5 and 9 in both `deploy` and `refresh` | B6 |
| HP-006 | app settings `Operator__*` (hosted-configuration) | B2 smoke checks 4–5; B5 |
| HP-007, SC-004 | prompts and `@file` in provisioning, masking at step 3, no arguments (R-03, R-04) | A1 self-test (secret files); A4; B1; B9 |
| HP-008 | the existing administrator, no bypass | smoke check 4; B3 |
| HP-009 | run summary: mode, SHA, result, failed step, start time | B2; B7 |
| HP-010 | console log on App Service storage, 30 days, behind an Azure sign-in (R-09) | B5 |
| HP-012 | settings set once; runs take only `mode` and `commit` | B2; B6 |
| HP-013, SC-007 | B1 and SQL Basic, USD 25 (R-12) | B10 |
| HP-014 | provisioning refuses an administrator email outside `example.com` (R-03) | A1 self-test; B1 |
| HP-015 | `docs/operations/hosted-demo.md` (R-10) | review in PR 1 |
| HP-016 | `workflow_dispatch` only; `master` only (step 1, environment branch policy) | B1 (policy); B8 |
| HP-017, SC-005 | smoke check decides the run; coverage test over the demo data port (R-06) | host and coverage tests; B2; B7 (a run failing at step 12) |
| HP-018 | `DataProtection:KeysDirectory` on `/home` (R-07) | key-directory host test; B4 |
| US2-AS3 | demo loading's existing behaviour (ADR 0007) on a restart; name uniqueness (slice 2) | B4 (operator log) |
| US3-AS4, US3-AS5, US4-AS5 | failure states and re-runs (contract); missing settings stop at step 3; a faulted demo loading fails step 12 | B7 |
| SC-001 | Always On; `first` and `later` timings of the smoke check (contract, R-14) | B10 (at least 10 `check` runs) |
| SC-002 | the published address and the exam submission's sign-ins | B3 (timed) |
| SC-003 | about 10-minute `deploy` (R-04) | B2 |
| SC-006 | `check` mode on 2026-11-23 and weekly | B10 |
| SC-008 | dates and PRs (R-15) | B2 by 2026-11-22 |

## Work order

1. **Session keys** (HP-018): the optional `DataProtection:KeysDirectory` setting in `Program.cs`, with its host
   tests written first, failing.
2. **Smoke check** (HP-017, SC-005): `tools/HT.SmokeCheck` in the solution; page-reader, invocation and coverage
   tests; host tests on `HT-UI-SMOKE`; the architecture rule.
3. **Migration bundle trial** (HP-004): quickstart A3.
4. **Provisioning and teardown scripts** (R-03): with `selftest.ps1` for the parts that run without Azure.
5. **Workflow** (R-04, R-05): `deploy-demo.yml` per its contract.
6. **Documentation and CI**: `docs/operations/hosted-demo.md`; README and CLAUDE.md layout, link and self-test
   command; the narrowed coverage exclusion in `sonar.yml`.
7. **Full build and test run and the self-test**, then PR 1 with the roadmap row at `implementing`
   (constitution V).
8. **Operator**: provisioning (possible from the PR 1 branch), then, after PR 1 is merged, the first deployment
   and quickstart Part B, at the latest 2026-11-22.
9. **PR 2**: Part B results and anything the first deployment needed; roadmap row `shipped` with both PRs.

Steps 1, 2–3 and 4–6 touch separate files and can run in parallel; 7 needs all of them; 8 needs the merge; 9
needs 8. Dates: the owner starts the slice on any day up to 2026-11-20 (R2); R-15 gives the sequence for that
latest start.

## Gates before the next step

- `/speckit-tasks` may start: the Constitution Check passes and no marker is open.
- When `/speckit-tasks` runs, it lists the demo data of every slice shipped by then (from those slices' specs
  and their changes to `IDemoDataStore`) and adds a smoke check and coverage entry for each. Once the coverage
  test exists, it fails if one is missed (R-06).
- ADR 0015, the roadmap and the checklist are consistent with this plan (this change). The spec is too, apart
  from the entries under Spec discrepancies.

## Spec discrepancies

- **Session keys, "no code change".** The owner's chosen option was described as App Service's built-in key
  storage with no code change. App Service on Linux documents no automatic persistence (R-07), so the plan adds
  one optional host setting that puts the keys on the same storage. The owner's intent (App Service storage, no
  new resource) is kept; ADR 0015 and the spec's clarification record the correction. No requirement changes.
- **Starts without a database (HP-004, critic revision).** HP-004 asks for the schema before administrator
  creation and demo loading run, and a faulted startup task makes a run fail. In `deploy` mode the new version
  starts twice while the database is deleted (contract steps 6–7), and its startup tasks fault there. The plan
  reads HP-004 and HP-017 as being about the start that serves the deployed version: the start of step 11, after
  the migrations. The earlier starts serve no request against any schema, and their faults precede the start
  time in the run summary (R-09). The alternative, deploying into a stopped web app, rests on undocumented
  behaviour (R-04).
- **Missing passwords (US3-AS5).** The scenario has the app start without a password, log the result and be
  reported as failed. The workflow detects the missing value at step 3, before it changes anything, and fails
  with the key's name, so the hosted demo keeps running. The app's own behaviour when it starts without the value
  (it starts and logs "no demo password configured", ADR 0007) is unchanged and shown in quickstart B7 by
  removing the setting outside a run.
- **Order of the workflow in the clarification.** The plan-session clarification (and ADR 0015's option text,
  now annotated) lists the deployment after the migrations, applied while the app is stopped. In that order the
  new version would be deployed into a stopped web app, which is undocumented on Linux (R-04). The contract
  deploys first, into a running app without a database, then stops it and migrates. The owner's intent (the
  database emptied, migrations while stopped, no start on a partial schema) is kept; the clarification's
  summary should be read through ADR 0015's numbered steps, and is corrected the next time the spec changes.
- **R2 timing.** The roadmap builds this slice last. Writing the plan on 2026-11-15 changes nothing in that
  order: the plan is parked like the spec, and the owner starts the implementation by 2026-11-20 (R-15). Any
  slice that ships before then adds its demo data to the hosted demo through demo loading, and its smoke check
  and coverage entry through the tasks gate above.

## Risks

| Risk | Effect | Mitigation |
|---|---|---|
| The slice starts only on 2026-11-20 | one day for implementation, one for review, merge and the first deployment (SC-008) | R-15's sequence; provisioning before the merge; the owner can start earlier |
| PR 1 not merged by 2026-11-22 | SC-008 missed | the owner's merge is the critical step; R-15 |
| A new firewall rule takes up to five minutes to apply | step 10 cannot connect | the rule is added in step 5; the bundle retries 3 times, 60 s apart (R-04) |
| `az webapp log download` does not work on Linux | stored operator log hard to read | Kudu `LogFiles` is the documented way (R-09) |
| .NET 10 runtime missing for App Service Linux in the chosen region | provisioning stops | the script checks the runtime list first; another region |
| A zip deploy into a web app without a database behaves unexpectedly | step 7 fails | the app tolerates a missing database at start (ADR 0007); the run fails visibly and the next run starts over (contract, re-running) |
| Database create is slow at the Basic tier | SC-003 at risk | about 10 minutes expected with margin; measured at B2 |
| The "what is my IP" service is down | step 10 fails | the run fails before anything is served; rerun; the rule is removed in `always()` |
| Page markup changes break the smoke check | false failures | its host tests run in the normal suite, so the slice that changes the markup sees it |
| A secret reaches the public run log | HP-007 broken | masked at step 3 as read; no `set -x`; B9 searches the downloaded logs |
| The outbound addresses of the web app change | the app loses the database | they change only with a plan change; provisioning sets the rules again |
| Security-stamp lookups on 5 DTU slow later pages | SC-001 | timings in every run (R-14); the interval is revisited in a change of its own if needed |
| Cost over budget | SC-007 | weekly check; teardown after 2026-12-31 |

## Complexity Tracking

No Constitution Check violation; nothing to justify.
