# Critic pass 1: plan for 003-demo-hosting

**Verdict**: REVISE
**Date**: 2026-11-15
**Reviewed**: `plan.md`, `research.md`, `data-model.md`, `quickstart.md`, `contracts/deploy-workflow.md`,
`contracts/smoke-check.md`, `contracts/hosted-configuration.md`

Checked against: `spec.md` (with `## Clarifications`), `checklists/requirements.md`, ADR 0015 and ADRs 0003,
0006, 0007, 0013, the constitution, `CLAUDE.md`, `CONTEXT.md`, `README.md`, `docs/roadmap.md`, slices 1 and 2
(spec, plan, research), the code under `src/`, the tests under `tests/HT.UiTests`, `tests/HT.UnitTests`,
`tests/HT.IntegrationTests`, `tests/HT.ArchitectureTests`, `HabitTracker.slnx`, `dotnet-tools.json` and
`.github/workflows/sonar.yml`. Nothing was built or run.

## Notes

### MAJOR-1: A rerun after a failed run is not specified, and in deploy mode it deploys into a stopped web app, the case research R-04 rejects as undocumented

- **Where**: `contracts/deploy-workflow.md`, "Steps and their failure meaning" (rows 5 to 9, column "Hosted
  demo after a failure here") and "Guarantees"; `research.md` R-04 (l.71-75 and Alternatives, l.88);
  `plan.md` Risks (l.174-183).
- **Against**: spec HP-017 (l.322-325: a failed run "MAY leave the hosted demo unavailable or incomplete
  until the next successful one"), US3 "Why this priority" (l.179-181: fixes are "deployed the same way",
  days before the exam), US3-AS4, Edge Cases (l.250-256), SC-008; ADR 0015 Decision ("the web app is left as
  the failed step left it: stopped, or with no database") and Consequences ("after a failed run until the
  next successful one").
- **Reasoning**: The only recovery the spec and the ADR give for a failed deployment or refresh is the next
  run. The contract leaves the web app "stopped" after a failure in step 7, 8 or 9. The next deploy-mode run
  executes step 5 (zip-deploy) and no earlier step starts the app, so it deploys into a stopped web app.
  R-04 chose its step order to avoid exactly that ("deploying into a stopped web app, whose behaviour on
  Linux ... is not documented") and lists "deploy with the web app stopped" among the rejected alternatives.
  The recovery path therefore rests on behaviour the plan itself declares unknown. The second state has the
  same gap: after a failure in step 5, 6 or 7 there is no database, and the rerun's step 4 deletes a database
  that does not exist; the table gives "delete error" as the failure condition and says nothing about a
  missing database. No artifact states that each step tolerates the states a failed run leaves, and the
  Risks table covers neither case (its mitigation "the run fails visibly and is repeated" presumes that the
  repeat works). R-11 says the workflow cannot be tested before Azure exists, so the first evidence would
  come from a failed deployment on the hosted demo, days before the exam.

### MAJOR-2: Demo data of slices that ship before this slice is built is not covered, and the plan's statement that the smoke check picks it up is not correct

- **Where**: `plan.md` "Spec discrepancies", R2 timing (l.167-170) and Constitution Check row V (l.66);
  `contracts/smoke-check.md` Checks (l.19-29); `research.md` R-06 (l.111-124); `data-model.md` (l.5-6,
  l.26-27); `quickstart.md` B3.
- **Against**: spec HP-017, SC-005, HP-005, US2-AS1 and US2-AS2 ("every shipped slice's demo data"); spec
  l.15-16 and Clarifications l.43-47 with roadmap slice note 3 (l.74-78): this slice is built last, after
  the product slices; ADR 0007 "Later slices"; critic-spec-pass-2 MINOR-2 (the spec was changed from "three
  demo accounts" to "every shipped slice" for this case).
- **Reasoning**: The smoke check takes three things from `HT.Application`: `DemoDataService.DemoEmails`,
  `DemoCustomHabits.For` and `SeedCatalog.Habits`. Its five checks sign in, count `/custom-habits/{id}` links
  and count `/catalog/{id}` links. A slice that adds another kind of demo data (goals in slice 4, results in
  slice 5) changes none of the three, so all checks pass whether that data was created or not. The contract's
  rule "A later slice that adds demo data adds its check here ... in the same change" binds only slices built
  after the smoke check exists. By R2 this slice is built last, so a slice that ships between 2026-11-15 and
  the start of this slice's implementation has no smoke check to extend, and nothing in the work order or the
  gates revisits the contract, the data model or the quickstart at that point (all three are written for
  slices 1 and 2: "its two custom habits", "`InitialAccounts`, `HabitCatalog`"). The plan names the case
  ("unless more ship first") and answers that such demo data "is picked up ... by the smoke check through
  `HT.Application`". That holds only for more demo emails, more demo custom habits and a changed seed
  catalog. A run whose demo loading faulted on the newer data would be reported as succeeded, against
  HP-017. Row V's claim that "demo-data drift fails the check instead of being missed" has the same limit.

### MINOR-1: The workflow contract reads from Azure before it signs in, and the step numbers differ between the ADR, the contract, the research and the plan

- **Where**: `contracts/deploy-workflow.md` rows 1 to 3 and Guarantees (l.43-44); `research.md` R-04
  (l.71-73); `plan.md` l.121, l.177, l.179.
- **Against**: ADR 0015 Decision, "Deployment and refresh" (the sign-in is stated before the numbered list,
  which has 10 steps); `contracts/hosted-configuration.md` (`Deployment__Commit` is a web app setting).
- **Reasoning**: In refresh mode step 1 reads `Deployment__Commit` from the web app, but the Azure sign-in
  is step 3, so the order as written cannot run. Step 1's read of the app settings store also comes before
  the masking of step 3, while the guarantee covers only "every value read in step 3". Numbering: the ADR
  has delete 3, deploy 4, stop 5, bundle 7, start 8; the contract has delete 4, deploy 5, stop 6, bundle 8,
  start 9. R-04 uses the ADR's numbers ("deleted first (step 3)", "stopped (step 5)"); `plan.md` uses the
  contract's ("workflow step 8" for HP-004, "step 5 fails", "step 8 fails"). "Step 8" is the start in the
  ADR and the migration bundle in the contract.

### MINOR-2: Every deploy-mode run starts the new version without a database; the conflict with the spec's wording and the effect on the operator log are not acknowledged

- **Where**: `research.md` R-04 (l.71-74); `contracts/deploy-workflow.md` rows 4 to 6; `plan.md` "Spec
  discrepancies" (two items); `quickstart.md` B5.
- **Against**: spec HP-004 (l.283-284: the schema is up to date "before administrator creation and demo
  loading run"), Critic revision (l.78-79: "A start whose startup task faulted is a failed deployment or
  refresh"), US3-AS3; `src/HT.Ui/Program.cs` l.60 and `src/HT.Ui/Operator/StartupTasks.cs` l.96-106,
  l.129-130.
- **Reasoning**: The zip-deploy of step 5 restarts the app on the new version while the database is deleted
  (and writing `Deployment__Commit` restarts it again). Administrator creation and demo loading run on those
  starts and fault, before any schema exists. By the letter that is what HP-004 and the critic-revision
  sentence exclude; the plan counts only the last start, but does not record this as a spec discrepancy.
  The consequence is not stated either: the operator log of every successful deployment holds
  "Administrator creation failed; the app starts without it" and "Demo loading failed; ..." Error entries
  with their exceptions. Quickstart B5 expects "administrator creation and demo-loading lines with emails
  and result texts only", and an Operator reading the log (HP-010) has nothing that tells these entries from
  a real fault.

### MINOR-3: As the weekly check, the smoke check fails on data SC-005 would accept, and its expected values depend on the commit the Operator has checked out

- **Where**: `contracts/smoke-check.md` checks 3 and 5; `research.md` R-06 item 3 (l.119-120), R-10;
  `quickstart.md` B9.
- **Against**: spec SC-005 and SC-006 ("finds every shipped slice's demo data", "finds the full seed
  catalog", a pass on 2026-11-23); US4 (l.208-210: reviewers add custom habits and change the catalog);
  Clarifications l.35-37 and HP-016 (the hosted app may lag behind `master`).
- **Reasoning**: (a) Checks 3 and 5 pass only on equal counts. A reviewer who adds a custom habit to a demo
  account, or as administrator publishes one more catalog habit, makes the check fail although all demo data
  and the whole seed catalog are still found. SC-006 asks for a pass on the exam day, when reviewers share
  these accounts. A count also cannot show R-06's "and none of the others": a missing seed habit and an
  extra habit give the same number. (b) The expected values come from the `HT.Application` of the commit the
  tool is built from. In the workflow that is the deployed commit. For the weekly run "from your machine"
  neither B9 nor R-10 says which commit to use, and the hosted demo may run an older commit than the local
  `master`.

### MINOR-4: The path of a missing password through the workflow (US3-AS5) is not defined, and the host test covers a different situation

- **Where**: `contracts/deploy-workflow.md` rows 3 and 10; `contracts/smoke-check.md` l.14 and Exit codes;
  `quickstart.md` B7; `research.md` R-11 (l.183-186).
- **Against**: spec US3-AS5 (l.198-200), HP-009 (the reason of a failure);
  `src/HT.Application/Operator/DemoDataService.cs` l.37-40 and l.58.
- **Reasoning**: The workflow reads the smoke check's passwords from the same app settings the host uses.
  When the hosted configuration lacks the demo password, the value read in step 3 is empty. The contract
  does not say whether that is a "read error" (the run fails in step 3, the hosted demo stays "unchanged"
  and no refresh happens) or whether the run goes on. If it goes on, `HT_SMOKE_DEMO_PASSWORD` is missing and
  the tool exits with 2, "usage error", without running a check. B7 expects the run to fail "at the smoke
  check". R-11's host test gives the tool a demo password and starts the host without one, which the
  workflow can never produce. R-11's "(no demo accounts)" is also incomplete: `LoadAsync` returns before
  `TryCreateSeedCatalogAsync` when the password is missing, so the `catalog` check fails too, and "the
  expected check name" is not one name.

### MINOR-5: The in-process form of the smoke check gets one `HttpClient`, but each sign-in needs a fresh cookie container

- **Where**: `contracts/smoke-check.md` l.27 against l.43-44; `research.md` R-06 (l.113-114).
- **Against**: `tests/HT.UiTests/Host/HostFixture.cs` l.66-71 (cookies are handled by the client the factory
  creates); `src/HT.Ui/Components/Pages/SignIn.razor` l.26-31 (a signed-in user who opens `/sign-in` is sent
  to Home).
- **Reasoning**: The check signs in four times (the administrator and three demo accounts) and the contract
  requires a fresh cookie container each time. A class that is handed one `HttpClient` cannot make a new
  cookie container for an in-process client. If the one client keeps the cookies, the second `GET /sign-in`
  is redirected to Home and has no form or token to read. The contract does not say who owns the cookies in
  the in-process case, so the host test of R-11 cannot be written from it as it stands.

### MINOR-6: How the SQL admin password reaches the server, and what a second provisioning run does with it, is not described

- **Where**: `research.md` R-03 (l.44, l.47-52) and R-02 (l.32-33); `plan.md` Constraints (l.47);
  `contracts/hosted-configuration.md` "Never".
- **Against**: spec HP-007; the plan's own rule "No secret value is passed as a command-line argument";
  ADR 0015 Decision, Provisioning ("idempotent ... run ... again after a change") and Consequences
  ("Changing one means running the provisioning script again").
- **Reasoning**: R-03 says the generated password is "used only to build the connection string". A logical
  server with SQL authentication also needs that password when it is created. R-03 describes the file route
  only for the app settings and the connection string, so how the password is given to the server without
  being an argument is open. On a second run the server exists, and its password was neither stored nor
  printed. The script "sets the settings again", but it cannot build the same connection string. Whether it
  resets the server's password, keeps the stored connection string or does something else is not said,
  although a second run is the documented way to change any secret.

### MINOR-7: The extra test hosts and their databases are not specified against ADR 0006 and slice 1's one-host rule

- **Where**: `research.md` R-11 (l.183-189); `plan.md` Work order 1 and 2; `quickstart.md` Part A
  prerequisites (l.12).
- **Against**: ADR 0006 ("host tests use database `HT-UI`, with the connection string from
  `HT_UI_CONNECTION`"); `specs/001-accounts-and-access/research.md` R-16 (one `WebApplicationFactory` per
  run; its one extra database `HT-UI-MIGRATE` is named with its reason); `HostFixture.cs` l.17 (sealed),
  l.23 and l.55-61 (reset guard), l.75-79 (fixed settings); `.github/workflows/sonar.yml` (CI sets only
  `HT_IT_CONNECTION` and `HT_UI_CONNECTION`); CLAUDE.md "Build and test".
- **Reasoning**: The plan needs at least a smoke-check host on "a database of its own", a host without a
  demo password on an empty database, and two hosts over one key directory. It does not name the database,
  say how its connection is derived from `HT_UI_CONNECTION` in CI, whether the reset guard covers it, which
  database the two key-directory hosts share (the security-stamp check reads the account, so it must be the
  same one), or which xUnit collection the new tests run in. Neither ADR 0006 nor R-16 is cited for the
  departure. Part A lists only `HT_UI_CONNECTION`, although its step 1 runs the whole solution, including the
  integration tests on `HT_IT_CONNECTION`.

### MINOR-8: The plan turns R2's latest start date into the date of the first deployment

- **Where**: `plan.md` l.49-50, l.135, l.148, l.167-168.
- **Against**: spec l.15-16 and Clarifications l.45-47 ("Hosting starts no later than ... 2026-11-20");
  `docs/roadmap.md` l.32, l.77 and row R2; SC-008.
- **Reasoning**: In the spec and the roadmap 2026-11-20 is the latest day on which work on this slice
  starts. The plan cites R2 for "first deployment by 2026-11-20" and says the first deployment "still
  happens by" that date. Those are different commitments. The plan gives no date for work-order steps 1 to
  6, and step 7 "needs the merge", which in this repository needs the Sonar and CodeQL gates, a manual
  Copilot review and the owner's merge. If the slice starts on the day R2 allows, the first deployment cannot
  be on the same day, and the plan does not show how SC-008 is met then.

### MINOR-9: Work-order step 7 runs after the merge but records its results in the PR, and the slice's status between merge and first deployment is not addressed

- **Where**: `plan.md` Work order 6 and 7 (l.147-151); `quickstart.md` B9; `research.md` R-10.
- **Against**: constitution V and "Workflow and Quality Gates" (the PR description gives the verification
  evidence); `docs/roadmap.md` "Updating this file" (opening the PR sets the row to `shipped`; a slice with
  several PRs keeps `implementing` until the last); CLAUDE.md ("One branch per task; open PRs against
  `master`").
- **Reasoning**: Step 7 needs the merge and then records its results "in `docs/operations/hosted-demo.md`
  and in the PR". The PR is merged by then, and the document is a committed file, so recording needs another
  change to `master` that the work order does not contain. The weekly check log of B9 is the same file, so
  every weekly record is a further change. The roadmap marks the slice `shipped` when its PR opens. At that
  moment every hosted requirement (HP-001, HP-002, HP-004, SC-001, SC-003, SC-008 and others) is still
  unverified. The plan does not say which roadmap rule applies or what the slice's status is until Part B
  has run.

### MINOR-10: Three days of operator log are shorter than the weekly check interval

- **Where**: `research.md` R-02 (l.30) and R-09 (l.163-164).
- **Against**: spec HP-010, US3-AS3, Edge Cases l.245-247 ("the fault shows in the operator log and the next
  smoke check fails"), SC-006 (weekly); ADR 0015 (names no retention period).
- **Reasoning**: The startup task lines are written once per start. With Always On the app does not restart
  between runs, so from the fourth day the results of administrator creation and demo loading are no longer
  in the log the Operator "MUST be able to read". A fault at an unplanned restart is found by the next
  weekly check up to seven days later, when its log entry is gone. The value 3 is not derived from any
  requirement.

### MINOR-11: Several "Verified by" entries are not backed by the step they name

- **Where**: `plan.md` "Requirements to mechanisms" (l.116-135); `quickstart.md` A4, B1, B4, B5, B9.
- **Against**: spec HP-014, SC-001, SC-004, SC-006, US2-AS3.
- **Reasoning**:
  - HP-014 is "verified by quickstart B1", but B1 expects only that the resources exist and no secret was
    printed. Nothing checks that the entered administrator email is in `example.com`.
  - SC-004 covers the repository, the deployment definitions and the operator log, for the hosted values.
    A4 searches for the local User Secrets values ("partly"), B2 searches the run log, and no step searches
    the operator log. The connection's password is generated and never shown (R-03), and no step says how
    the Operator gets the value to search for.
  - SC-001 is "after the app has been idle", in 9 of 10 attempts. B4 measures once, after
    `az webapp restart`. With Always On a restart is not the idle case, and a cold start is a different
    quantity than the one SC-001 bounds.
  - SC-006 names 2026-11-23 besides the weekly checks. B9 lists only "Weekly".
  - US2-AS3 (no duplicates after a restart without a deployment) appears in no step; B4 restarts but checks
    only the session and the timing.

### MINOR-12: The migration bundle is asserted to work with this solution but is not tried anywhere before the first deployment

- **Where**: `research.md` R-05, R-11 (l.191-193); `quickstart.md` Part A; `plan.md` Constitution Check row
  V ("Host and unit tests for everything that runs locally").
- **Against**: constitution V; spec HP-004; `src/HT.Persistence/HtDbContext.cs` l.11 and
  `HtDbContextFactory.cs` l.10 (both internal), the two migrations (internal), `HT.Persistence` as a class
  library that is its own startup project.
- **Reasoning**: R-05 states that `dotnet ef migrations bundle` builds from this setup and that the bundle
  reads `HT_DESIGN_CONNECTION` when it runs. Slices 1 and 2 only ran `migrations add` with it. Building the
  bundle and applying it to a local SQL Server needs no Azure, yet it is in neither Part A nor the tests,
  and R-11 does not list it among the things that cannot be tested locally. The mechanism behind HP-004
  therefore runs for the first time in the first deployment, and row V's claim does not hold for it.

### MINOR-13: The new project's dependency rule is prose only, and the documented layout knows no compiled project outside `src/` and `tests/`

- **Where**: `plan.md` Project Structure (l.95-99) and Structure Decision (l.110-112); `research.md` R-06
  (l.111-112), R-11.
- **Against**: CLAUDE.md ("Layer rules must be enforced by architecture tests, not just prose"; "production
  projects in `src/`, test projects in `tests/`"); `tests/HT.ArchitectureTests/ArchitectureModel.cs` l.17-19
  (the model loads four assemblies); README "Repository layout" (l.48-56).
- **Reasoning**: "References `src/HT.Application` only" is stated three times and enforced nowhere: no
  architecture test is planned in R-11, and the architecture model does not load the new assembly. The plan
  adds a fifth compiled project to `HabitTracker.slnx`, in a folder the layout statements of CLAUDE.md and
  the README do not have, and lists only a README link as documentation work.

### MINOR-14: Two choices are left open for `/speckit-tasks`

- **Where**: `plan.md` l.107 ("or HT.UiTests, wherever the tool reference fits (tasks)"); `research.md` R-12
  (l.201-202, "may add an Azure budget alert").
- **Against**: ADR 0006, Test-project layout (`tests/HT.UnitTests`: "Domain and Application rules");
  `tests/HT.UnitTests/HT.UnitTests.csproj` (references Domain and Application only);
  `src/HT.Ui/HT.Ui.csproj` l.9 and `HostFixture.cs` l.17; `research.md` R-03 and `quickstart.md` B1 ("the
  three values").
- **Reasoning**: Where `PageReaderTests` live decides which test project references the tool, and ADR 0006
  gives `HT.UnitTests` a scope the tool is outside of. The contract makes the checks class public and says
  nothing about `PageReader`. If its internals reach `HT.UiTests`, that project sees two `Program` types
  (it already sees `HT.Ui`'s through `InternalsVisibleTo`, for `WebApplicationFactory<Program>`). The budget
  alert is neither in nor out of scope, and it would add a fourth prompt that R-03 and B1 do not list.

### MINOR-15: Slice 1's note for hosting (a database lookup on every request of a signed-in user) is not taken up, and SC-001's 3 seconds are not examined for the Basic tier

- **Where**: `plan.md` Risks (l.174-183); `research.md` (no item).
- **Against**: `specs/001-accounts-and-access/research.md` R-09 (l.202-206) and `plan.md` l.198-201 ("slice
  3 can revisit it for hosting"); `src/HT.Ui/Program.cs` l.48 (`ValidationInterval = TimeSpan.Zero`); spec
  SC-001; ADR 0015 (Azure SQL Basic, 5 DTU).
- **Reasoning**: Slice 1 handed this to slice 3: every request of a signed-in user, static assets included,
  does a security-stamp lookup and gets a new cookie with no-store headers, so the browser does not cache
  the CSS and scripts. On the hosted demo each lookup goes to a 5-DTU database. The plan neither accepts nor
  revisits it, the Risks table names the database tier only for SC-003, and SC-001's bound for later pages
  is measured once (MINOR-11).

## Summary

2 MAJOR, 15 MINOR. The plan follows ADR 0015, maps all 18 HP requirements to a mechanism, and its claims
about the existing code hold where checked (the sign-in form fields and handler, the page addresses, the
public demo data types, the design-time factory, the pinned `dotnet-ef`, the conditional `Program.cs`
change, no existing test broken by it). It is not ready for `/speckit-tasks`: the only recovery from a failed
run, the next run, goes through a deployment into a stopped web app that the plan's own research calls
undocumented, and the smoke check that decides success does not cover demo data of slices that ship before
this slice is built, although the roadmap builds it last and the plan says the case is handled. The MINOR
notes are contract inconsistencies (step order and numbering, cookie handling, the missing-password path),
underspecified provisioning and test set-up, and verification entries that their quickstart steps do not
back.
