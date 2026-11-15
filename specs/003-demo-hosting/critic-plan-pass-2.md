# Critic pass 2: plan for 003-demo-hosting

**Verdict**: APPROVE
**Date**: 2026-11-15
**Reviewed**: `plan.md`, `research.md`, `data-model.md`, `quickstart.md`, `contracts/deploy-workflow.md`,
`contracts/smoke-check.md`, `contracts/hosted-configuration.md`

Checked against: `spec.md` (with `## Clarifications`), `checklists/requirements.md`, ADR 0015 and ADRs 0001,
0003, 0006, 0007, 0013, the constitution, `CLAUDE.md`, `CONTEXT.md`, `README.md`, `docs/roadmap.md`, slices 1
and 2 (plan, research), the code under `src/`, the tests under `tests/HT.UiTests` and
`tests/HT.ArchitectureTests`, `HabitTracker.slnx`, `Directory.Build.props`, `dotnet-tools.json`,
`.github/workflows/sonar.yml` and `.githooks/pre-commit`. Three external facts were read from the vendors'
public documentation on 2026-11-15 (Azure CLI reference `az webapp log`, Azure SQL "IP firewall rules", GitHub
Docs "Events that trigger workflows"); they are named where used. Nothing was built or run.

## Previous notes: resolution status

Only one earlier plan pass exists (`critic-plan-pass-1.md`), so there is no older note that could regress.

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 rerun after a failed run | Resolved | `contracts/deploy-workflow.md` step 6 starts the web app before step 7 deploys; "Re-running after a failure" states what steps 5, 6, 8 and 10 tolerate; step 7 clears `Deployment__Commit` first; R-04 l.81-85; plan Risks l.215; quickstart B7 runs `deploy` on a web app stopped by hand. The missing-database state is covered by the contract only, not by a Part B step. |
| MAJOR-2 demo data of slices shipped before this slice is built | Partly | Coverage rule (R-06 l.132-138, `smoke-check.md` l.45-52), `SmokeCheckCoverage.cs`, tasks gate (plan l.182-184), ADR 0015 l.102-103. The rule is defined in two different ways and rests on a convention no input requires; continued as MINOR-1. |
| MINOR-1 read before sign-in; step numbers | Resolved | Sign-in is step 1; ADR 0015 l.75-87 lists the same 13 steps; R-02, R-04, R-05, the plan and the quickstart use the contract's numbers. A new order defect of the same class is MINOR-9. |
| MINOR-2 starts without a database not acknowledged | Resolved | Plan "Spec discrepancies" l.193-199, contract "Startup tasks before the database exists", ADR 0015 Consequences l.129-131, R-09, quickstart B5. The stated rationale (the alternative order deploys into a stopped web app) is consistent with R-04; HP-004's text is unchanged and the reading rests on the plan and the ADR. |
| MINOR-3 equal counts; commit of the expected values | Resolved | Presence by name (`smoke-check.md` l.33-35, l.39-41); the tool is built from `Deployment__Commit` in every mode (l.19-20); `check` mode runs in the workflow. |
| MINOR-4 path of a missing password | Resolved | Step 3 fails with `configuration incomplete: <key name>`; exit code 2 is a broken invocation (`smoke-check.md` l.16-18); R-11 l.201-203 now names `catalog` and every `demo` check, which matches `DemoDataService.LoadAsync` l.37-40; plan discrepancy l.200-204; B7. The rationale is sound: the run is reported as failed with its reason before anything changes. |
| MINOR-5 one `HttpClient`, several cookie containers | Resolved | `Func<HttpClient>` client factory (`smoke-check.md` l.24-27). |
| MINOR-6 SQL admin password | Resolved | R-03 l.55-59 (`@<file>`, kept on a later run, `-RotateSqlPassword`); `hosted-configuration.md` "SQL server". |
| MINOR-7 extra test hosts and databases | Partly | `HT-UI-SMOKE` and `HT-UI-KEYS`, their derivation, the collections and the departure from slice 1 R-16 are stated (R-11 l.198-211; quickstart l.12-13). Environment, schema creation and ADR 0006 remain open; continued as MINOR-8. |
| MINOR-8 R2's latest start date read as the first deployment | Partly | R-15 l.257-258 now separates the latest start from the first deployment. The planned dates and the R2 case are continued as MINOR-5. |
| MINOR-9 results recorded after the merge; status until Part B | Resolved | Two PRs (work order 7-9, R-15), roadmap rule for a slice with several PRs, the check history is the workflow's run history (R-10 l.190-191). |
| MINOR-10 three days of operator log | Resolved | 30 days in R-02 l.31, R-09 l.172-174, `data-model.md` l.18 and ADR 0015 l.59-60, with the reason. How the log is read and how the period is set is a new note, MINOR-2. |
| MINOR-11 "Verified by" entries | Partly | HP-014 (B1 expects the refusal), SC-004 (B9 searches the run logs and the operator log with the hosted values) and SC-006 (B10 names 2026-11-23) are resolved. SC-001 and US2-AS3 are continued as MINOR-4 and MINOR-6. |
| MINOR-12 migration bundle not tried | Resolved | Quickstart A3, work order 3, R-05 l.106-108. |
| MINOR-13 dependency rule in prose; layout documents | Resolved | `tests/HT.ArchitectureTests/SmokeCheckDependencyTests.cs` and the fifth assembly in the model (plan l.120, R-11 l.216-217); README and CLAUDE.md layout (plan l.113). The existing rules are scoped by assembly (`UiDependencyTests.cs`, `ApplicationDependencyTests.cs`), so a fifth assembly breaks none of them. |
| MINOR-14 two open choices | Resolved | Tests in `HT.UiTests`, `HT.UnitTests` keeps its scope (plan l.126-127, R-11 l.212-215); no budget alert (R-03 l.63, R-12). The first choice leaves a gap of its own, MINOR-7. |
| MINOR-15 slice 1's handover; SC-001's 3 seconds | Partly | R-14 accepts the handover and the Risks table names it (l.221). Its reasoning and the measurement it relies on are continued as MINOR-4. |

## Notes

### MINOR-1: The coverage rule is defined in two different ways, and both rest on a convention that no input requires

- **Where**: `research.md` R-06 (l.132-138); `contracts/smoke-check.md` "Coverage rule" (l.45-52); `plan.md`
  l.107 (`SmokeCheckCoverage.cs`: "the demo-data kinds"), Gates (l.182-184), "R2 timing" (l.205-207).
- **Against**: spec HP-017 (l.322-325) and SC-005 ("every shipped slice's demo data"); ADR 0007, Decision,
  "Results" and "Later slices"; ADR 0013, Decision ("together with the account, as one unit");
  `src/HT.Application/Operator/IDemoDataStore.cs` l.20-21, `DemoAccountResult.cs`, `DemoLoadingResult.cs`.
- **Continues**: pass-1 MAJOR-2.
- **Reasoning**: R-06 has the test list the public types named `Demo*Result` in `HT.Application.Operator` and
  compare them with a list of four type names. The contract has the test start from `DemoLoadingResult` and
  `DemoAccountResult`, follow "the result types they reference", and fail when one "reports a kind of demo
  data" that a list of three kinds (demo accounts, demo custom habits, seed catalog) does not name. These are
  different inputs (a name pattern against a reference graph, which also reaches the three `Demo*Outcome`
  enums) and different lists (types against kinds), and "reports a kind" is not something a test can decide
  from a type. Both versions depend on R-06's premise that "a slice that adds demo data adds a result type
  (ADR 0007 makes demo loading report each kind)". ADR 0007 asks for a result text and the email in the
  operator log, not for a type per kind. ADR 0013 has private demo data created with the account as one unit,
  and `CreateDemoAccountAsync` already takes the account's custom habits as a parameter; a slice that passes
  one more list there and reports through the existing `DemoAccountResult` adds no `Demo*Result` type, and the
  test stays green while the smoke check does not look at the new data. The contract's sentence that such a
  slice "cannot pass its tests ... whether it ships before or after this slice is implemented" does not hold
  for "before": until this slice is implemented neither the tool nor the test exists. That case rests on the
  tasks gate alone, whose last sentence ("The coverage test fails if one is missed") has the same premise.

### MINOR-2: The plan's way to read the stored operator log is documented as unreliable on Linux, and nothing says how the 30 days are set

- **Where**: `research.md` R-09 (l.172-175), R-02 (l.31), R-03 (the script uses the Azure CLI);
  `quickstart.md` B5 (l.67-69), B9 (l.80-82); `data-model.md` l.18; ADR 0015 l.59-60, l.119-120.
- **Against**: spec HP-010 (l.301-304), US3-AS3, SC-004 (the operator log is one of the three places searched);
  Azure CLI reference `az webapp log`: for `az webapp log download`, "This command may not work with web apps
  running on Linux."; `az webapp log config` lists `--docker-container-logging {filesystem, off}` and no
  retention or quota parameter; `az webapp log tail` is described as live log tracing.
- **Continues**: new (follows from the resolution of pass-1 MINOR-10).
- **Reasoning**: The hosted app is App Service on Linux. R-09 gives two ways to read the log, `tail` and
  `download`. B5 reads the lines of a start that has already happened, and B9 searches "the downloaded operator
  log" for the secret values; both need the stored log, not the live stream, and the only command named for it
  carries the vendor's Linux caveat. No artifact mentions the caveat or another way to the stored log. The 30
  days are stated as a property of the web app in four artifacts, but the CLI command that turns container
  logging on has no parameter for a retention period, and R-02 and R-03 do not say how the provisioning script
  sets it. The edge case that motivated the 30 days (a fault at an unplanned restart, read a week later)
  depends on both points.

### MINOR-3: Step 10 opens a firewall rule and connects in the same step, and the documented delay of such a rule is not among the risks

- **Where**: `contracts/deploy-workflow.md` step 10 (l.33); `research.md` R-04 (l.90-92, "migrations under 1"),
  R-05 (l.103-105); `plan.md` Risks (l.211-222).
- **Against**: spec SC-003 (15 minutes), HP-004; Azure SQL documentation "IP firewall rules" (sections on the
  latency of firewall updates and on troubleshooting: a change can take up to five minutes to take effect);
  `src/HT.Persistence/HtDbContextFactory.cs` l.17-21 (`UseSqlServer` without retry options).
- **Continues**: new.
- **Reasoning**: The bundle gets its context from the design-time factory, which configures no retry. If the
  runner's rule is not yet in effect when the bundle connects, the bundle exits with an error, step 10 fails
  and the web app stays stopped until the next run. The Risks table has a row for the address lookup service
  of the same step but none for the rule's delay, and the time estimate of R-04 assumes the rule works at
  once. The first evidence comes from the first deployment (R-11: the workflow cannot be tested before Azure
  exists), in the days before the exam.

### MINOR-4: R-14's argument for SC-001 does not match how this app renders, and the timing it relies on leaves out the requests the handover is about

- **Where**: `research.md` R-14 (l.243-249); `contracts/smoke-check.md` l.42-43, l.56-57; `plan.md` l.48-49,
  l.155, Risks l.221; `quickstart.md` B10 (l.83-87), B3.
- **Against**: spec SC-001 (l.352-353); `specs/001-accounts-and-access/research.md` R-09 (l.202-206: static
  assets included, not cached by a signed-in browser); `src/HT.Ui/Components/App.razor` (`<Routes />` has no
  render mode); `@rendermode InteractiveServer` on single pages (`Catalog.razor` l.2, `CustomHabits.razor` l.3,
  `Admin.razor` l.3); `SignInForm.razor` (static SSR form); `src/HT.Ui/Program.cs` l.48.
- **Continues**: pass-1 MINOR-15 and MINOR-11 (SC-001).
- **Reasoning**: R-14 says that "after the first load Blazor Server works over its circuit, not over new HTTP
  requests". The router of this app is not interactive; interactivity is set per page. Every page navigation
  is therefore a new HTTP request (and a new circuit on an interactive page), each with the security-stamp
  lookup, so the argument for "a few such requests" does not hold as written. The mitigation is that the smoke
  check reports later-page timings. The tool times single HTML requests; it requests no static asset and opens
  no circuit. The cost slice 1 handed over is on exactly those static-asset requests of a signed-in user, so
  the `TIMING` line cannot show whether a reviewer's later page stays within 3 seconds under it, and the only
  browser step (B3) measures no time. The first-page value is also undefined: check 1 retries for up to 300
  seconds and "records the time of its first successful response", which can mean the time since the first
  attempt or the duration of the request that succeeded. In `deploy` and `refresh` runs, which B10 counts
  among its ten, the two differ by the whole warm-up.

### MINOR-5: The planned dates start the slice before R2's date and before any product slice, and for the R2 case the plan shows no way to SC-008

- **Where**: `research.md` R-15 (l.257-267); `plan.md` l.54-55, work order 7-8 (l.170-173), "R2 timing"
  (l.205-207), Risks l.213.
- **Against**: spec l.15-16 and Clarifications l.43-47 ("At the end, after the product slices. This spec is kept
  and parked until then"; "starts no later than"); `docs/roadmap.md` l.32-40 (slice 3 "built last", slices 4
  to 11 `not started`), slice note 3 (l.76-78), row R2 (l.208); spec SC-008.
- **Continues**: pass-1 MINOR-8.
- **Reasoning**: R-15 reads R2 correctly as the latest start. Its planned dates then put the tasks, work-order
  steps 1 to 6 and PR 1 "by 2026-11-18", which needs a start two or more days before R2's date, at a time when
  no slice after 2 has started. That is not "built last, after the product slices" unless product work stops,
  while the plan's own discrepancy entry says that planning now "changes nothing in that order". For the case
  R2 allows, a start on 2026-11-20, the plan gives no sequence that reaches SC-008: PR review, the Sonar and
  CodeQL gates, the owner's merge, provisioning and the first deployment fall into two days, and the answer is
  that "the owner decides then whether that is enough". So either the schedule departs from the owner's R2
  answer or SC-008 is not shown to be met; the plan does not say which it chooses.

### MINOR-6: Four "Verified by" entries are still not backed by the step they name

- **Where**: `plan.md` "Requirements to mechanisms" (l.136, l.148, l.151, l.153, l.154), Constitution Check row
  V (l.72), work order 4 (l.167); `quickstart.md` B3 (l.59-62), B4 (l.63-66), B7 (l.73-77).
- **Against**: spec US2-AS3 (l.161-162), HP-017 (l.322-325), US4-AS5 (l.230-232), SC-002 (l.354-356), HP-014;
  `contracts/smoke-check.md` l.39-41, l.56; constitution V; CLAUDE.md (the self-tests of `tools/pr-gates` and
  `tools/guard-oracle`).
- **Continues**: pass-1 MINOR-11.
- **Reasoning**:
  - US2-AS3 is verified by B4, whose evidence is that a `check` run "passes with the same counts as before
    (nothing duplicated)". The check counts how many of the expected names are on the page (`<found> of
    <expected>`). A duplicated custom habit or seed habit gives the same line, so the counts cannot show a
    duplicate. Only the operator-log half of B4 bears on the scenario.
  - HP-017 and US4-AS5 name B7. Both failing runs of Part B stop at step 3. No step makes step 12 fail on the
    hosted demo, so the case the rule is about (the app starts, a startup task did not do its work, the run is
    reported as failed with its step) is shown only in process, by the host tests of the tool, and not for
    the workflow.
  - SC-002 is mapped to B3, which expects nothing about the 3 minutes.
  - Row V claims tests "for everything that runs without Azure". The provisioning script is only
    parse-checked. Its refusal of an administrator email outside `example.com` is the whole mechanism of
    HP-014 and runs without Azure, as does the deletion of the secret files; neither has a test, although the
    repository's other tools have self-tests.

### MINOR-7: The invocation tests have no public type to call

- **Where**: `plan.md` l.104 (`Program.cs`: "arguments, environment variables, exit codes (internal)"), l.118
  (`InvocationTests.cs`); `research.md` R-11 (l.212-215); `contracts/smoke-check.md` l.15-18, l.63-64.
- **Against**: R-11 itself ("The tool's `Program` stays internal with no `InternalsVisibleTo`"); R-06 (the
  public types are `SmokeCheck` and `PageReader`; the plan adds `SmokeCheckCoverage`).
- **Continues**: new (follows from the resolution of pass-1 MINOR-14).
- **Reasoning**: R-11 lists unit tests in `HT.UiTests` for "argument and environment handling (exit code 2)".
  The plan puts that handling into the tool's internal `Program.cs`, and none of the three public types is
  described as holding it. A test in `HT.UiTests` cannot call it, and starting the built tool as a process is
  not mentioned. The test file cannot be written from the artifacts as they stand.

### MINOR-8: R-11 does not say how the new test hosts get a schema, and ADR 0006's database rule is still not cited

- **Where**: `research.md` R-11 (l.198-211); `plan.md` l.36-37, l.39.
- **Against**: `src/HT.Ui/Operator/StartupTasks.cs` l.30-37 (migrations at start only in Development);
  `specs/001-accounts-and-access/research.md` R-16 (l.320-321: `Testing`, "no start-up migrations"; l.327: the
  fixture calls `HtDatabase.ResetAsync`); `tests/HT.UiTests/Host/HostFixture.cs` l.49, l.75;
  `src/HT.Ui/Program.cs` l.42, l.48; ADR 0006, Decision ("host tests use database `HT-UI`").
- **Continues**: pass-1 MINOR-7.
- **Reasoning**: For the smoke-check hosts R-11 says "The database is dropped before each host starts". It
  names neither the environment of the new factories nor who builds the schema. The existing host runs in
  `Testing`, where the start applies no migrations, and gets its schema from a reset before the host starts.
  A host that starts on a dropped database outside Development faults in both startup tasks, and the first
  case ("all checks pass on a freshly seeded host") cannot pass. For the key-directory test, two hosts run "in
  turn over the same database" under the "same collection rules"; if the drop applies to the second host too,
  the account of the first sign-in is gone and the security-stamp check rejects its cookie, so the test can
  never pass. R-11 does not say that the second start keeps the database. The departure is justified against
  slice 1's R-16 only; ADR 0006, which gives host tests the database `HT-UI`, is cited for other points.

### MINOR-9: Two statements of the workflow contract do not hold as written

- **Where**: `contracts/deploy-workflow.md` l.8-9, step 2 (l.25), step 4 (l.27); `research.md` R-04 (l.86),
  R-03 (l.60-61), R-15 (l.261); `quickstart.md` l.48; ADR 0015 l.76-78.
- **Against**: spec HP-016 (l.320-321) and HP-003 ("the deployed commit of `master`"); GitHub Docs, "Events
  that trigger workflows", `workflow_dispatch`; `contracts/hosted-configuration.md` "GitHub environment `demo`".
- **Continues**: the first point is new, of the same class as pass-1 MINOR-1; the second is new.
- **Reasoning**:
  - Step 2 checks in `deploy` mode that the commit is an ancestor of `origin/master`, with `git merge-base`
    (R-04). The commit is checked out in step 4. At step 2 the runner has no repository, so the order as
    written cannot run.
  - The contract says GitHub runs a `workflow_dispatch` workflow "only from the default branch", and the
    quickstart and R-15 repeat that the workflow "runs only from `master`". GitHub's documentation requires
    the workflow file to exist on the default branch for the event to trigger, and allows a run to be started
    against any branch or tag, which then uses that ref's version of the file. The conclusion drawn (PR 1 must
    be merged before the first run) holds; the statement does not. The check that makes a deployment "a chosen
    commit of `master`" is therefore the check of whichever version of the workflow file runs. The contracts
    set no branch rule on the environment `demo`, and the federated credential's subject names only the
    environment.

### MINOR-10: The owner's recorded answer and the ADR's chosen option give an order of steps that the contract reverses, and the plan's gate calls them consistent

- **Where**: ADR 0015 Options (l.40-42) against its Decision (l.75-87); `plan.md` Gates (l.185), "Spec
  discrepancies" (l.187-207).
- **Against**: spec Clarifications l.53-56 (plan session 2026-11-15: "it empties the database, applies the
  deployed commit's migrations ... while the app is stopped, deploys (not on a refresh), starts the app");
  `research.md` R-04 (l.77-80, Alternatives l.93).
- **Continues**: new.
- **Reasoning**: The spec's clarification and the text of the ADR's chosen option list the deployment after
  the migrations, which are applied while the app is stopped. In that order the new version is deployed into
  a stopped web app, the alternative R-04 rejects as undocumented. The ADR's numbered list and the contract
  deploy at step 7, before the stop (8) and the migrations (10). The plan lists four spec discrepancies and not
  this one, and its gate states that "ADR 0015, the spec, the roadmap and the checklist are consistent with
  this plan". A later slice that cites the clarification gets the rejected order.

### MINOR-11: The new .NET project sits under a coverage exclusion that ADR 0001 justifies with "repo scripts"

- **Where**: `plan.md` Project Structure (l.102-107), Structure Decision (l.123-128); `research.md` R-06
  (l.119-120), R-11.
- **Against**: `.github/workflows/sonar.yml` l.130-131 and l.140 (`sonar.coverage.exclusions="tools/**,
  .claude/hooks/**"`); ADR 0001 l.52-53 ("Coverage is collected for .NET only, so repo scripts (`tools/**`,
  `.claude/hooks/**`) are in `sonar.coverage.exclusions`"); constitution V.
- **Continues**: new.
- **Reasoning**: The plan puts the smoke check, compiled .NET code in `HabitTracker.slnx`, under `tools/`
  "like `tools/pr-gates`". The exclusion for that folder was recorded for scripts, on the ground that coverage
  is measured for .NET only. With the new project the same pattern removes .NET code from the coverage metric
  of the PR gate, and that code is the mechanism that decides HP-017. The plan lists neither `sonar.yml` nor
  ADR 0001 among the files it touches or the inputs it considered, so the effect is neither chosen nor
  recorded.

## Summary

0 MAJOR, 11 MINOR. Of the 17 notes of pass 1, 12 are resolved and 5 partly resolved; none is unresolved. Both
MAJOR notes of pass 1 are answered in substance: a rerun no longer deploys into a stopped web app and every
step states what it tolerates, and demo data of other slices is tied to the smoke check by a coverage test and
a tasks gate. The claims about the existing code that the revision added hold where checked: the four
`Demo*Result` types, the public expected-data types, `/admin` answering Not found to a non-administrator, the
pages rendering their data in the first HTTP response, the sign-in form's handler and fields, new account ids
on every start from an empty database, the assembly-scoped architecture rules, the pinned `dotnet-ef` and the
`HT-UI-MIGRATE` precedent for derived databases. No existing test was found that the plan breaks without
saying so. The remaining notes are imprecise or doubly defined mechanisms (the coverage rule, the invocation
tests, the schema of the new test hosts, two contract statements), three unlisted operational risks or
dependencies (reading the stored log on Linux, the firewall rule's delay, the coverage exclusion), SC-001
evidence that does not measure what R-14 accepts, verification entries their steps do not back, and dates that
sit beside the owner's R2 answer. None of them blocks `/speckit-tasks`; several of them (MINOR-1, 7, 8, 9)
decide what the tasks for the smoke check, its tests and the workflow can state.
