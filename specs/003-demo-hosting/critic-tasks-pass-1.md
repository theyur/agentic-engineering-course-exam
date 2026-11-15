# Critic pass 1: tasks for 003-demo-hosting

**Verdict**: REVISE
**Date**: 2026-11-15
**Reviewed**: `specs/003-demo-hosting/tasks.md` (T001–T046), against `spec.md`, `checklists/requirements.md`,
`plan.md`, `research.md`, `data-model.md`, `quickstart.md`, `contracts/deploy-workflow.md`,
`contracts/smoke-check.md`, `contracts/hosted-configuration.md`, `critic-plan-pass-1.md`, `critic-plan-pass-2.md`,
ADRs 0001, 0006 and 0015, the constitution, `SPEC.md`, `ARCHITECTURE.md`, `CLAUDE.md`, `CONTEXT.md`, `README.md`,
`docs/roadmap.md`, `specs/002-habit-catalog/tasks.md` (conventions), and the existing code and tests:
`src/HT.Ui/Program.cs`, `src/HT.Ui/Operator/StartupTasks.cs`, `src/HT.Ui/Components/` (App, Catalog,
CustomHabits, SignIn, HabitCard), `src/HT.Application/Operator/`, `src/HT.Persistence/HtDbContextFactory.cs`,
`HtDatabase.cs` and `Migrations/`, `HabitTracker.slnx`, `Directory.Build.props`, `dotnet-tools.json`,
`.github/workflows/sonar.yml`, `.githooks/pre-commit`, `tests/HT.UiTests` (Host, Operator, csproj) and
`tests/HT.ArchitectureTests`.

Nothing was built and no test was run (critic rules). One command quoted in the tasks was executed against an
existing script, read-only, to check that it works (MINOR-3).

## Notes

### MAJOR-1: T007 cannot be completed where it is placed, and the Phase 2 checkpoint cannot be reached before Phase 4

- **Where**: tasks.md T007 (line 69); T001 (line 48) and T011 (line 79); the Phase 2 gate (line 63) and
  checkpoint (line 98); "Within phases" (line 227); "Parallel opportunities" and the Phase 2 example (lines 233,
  240–244).
- **Against**: the tasks' own rules ("Test tasks come first in each phase and must fail before their
  implementation task", line 14; "no story phase starts before this phase is complete", line 63);
  `.githooks/pre-commit` (runs `tests/HT.ArchitectureTests` on every commit); CLAUDE.md, Commits ("Never use
  `--no-verify`; fix the failing test") and constitution V (the gate is not bypassed);
  `src/HT.Application/Operator/IDemoDataStore.cs` lines 20–28 and `DemoDataService.cs` line 26.
- **Reasoning**: Two separate problems in the same task.
  1. T007 adds `typeof(HT.SmokeCheck.SmokeCheck).Assembly` to `ArchitectureModel.cs` and then says "run the
     whole architecture suite and confirm the existing 23 rules still pass". The type `HT.SmokeCheck.SmokeCheck`
     does not exist at that point: T001 creates only the stub `SmokeCheckCommand`, and `SmokeCheck.cs` is
     created by T011, four tasks later. T007 is under "Tests first", is marked [P] with T005 and T006, and is
     listed in the Phase 2 parallel example. With the new field, `HT.ArchitectureTests` does not compile, so
     none of the 23 existing cases runs and the confirmation T007 asks for is impossible. T005 and T006 state
     that they do not compile until T009 and T010; T007 says nothing of the kind, and "Within phases" (line
     227) has no T007 entry.
  2. T007's second assertion is that the tool's referenced assemblies among the solution's are "exactly
     `HT.Application` and `HT.Domain`", by `GetReferencedAssemblies()`. The compiler writes an assembly
     reference only for an assembly whose types the code uses. The tool code of Phase 2 (T009 `PageReader`,
     T010 command and options, T011 checks 1 and 2 with the sign-in, T012 the timing) uses no `HT.Domain`
     type; the only `HT.Application` member it could touch is `DemoDataService.DemoEmails`, a list of strings
     (`DemoDataService.cs` line 26). The first uses of Domain types are in Phase 4: T021 (`Email`,
     `IReadOnlyList<HabitDefinition>`, the parameter types of `IDemoDataStore`) and T022 (`CatalogHabitState`).
     So the assertion fails at the Phase 2 checkpoint, which states that `dotnet test HabitTracker.slnx` passes
     including T007 (line 98). Line 63 forbids starting Phase 3 or 4 before Phase 2 is complete, and Phase 2
     cannot be complete before T021/T022 of Phase 4. The stated order cannot be followed.
  The effect is not limited to the checkpoint text. The pre-commit hook runs this test project, so from T007
  until T021/T022 no commit passes the architecture gate, including a commit at the Phase 2 and Phase 3
  checkpoints. An implementer following the tasks must either reorder them without guidance or weaken the
  assertion, and the assertion is the enforcing mechanism the plan promises for the new project (research
  R-11, CLAUDE.md "Layer rules must be enforced by architecture tests").

### MINOR-1: Check 2 and the warm-up retry of check 1 have no test, although T004 makes the tool count in the coverage gate

- **Where**: tasks.md T011 (line 79), T020 (line 131), T005–T006 (lines 67–68), T004 (line 51).
- **Against**: constitution V; research R-11 ("Check 2 is skipped"); plan "Requirements to mechanisms"
  (HP-002 verified by "smoke check 2; B3"; SC-001 by the `first` timing); contracts/smoke-check.md, Timings
  (`first <ms> ms (retried)` "does not count for SC-001"); ADR 0001, Consequences (Sonar gate on
  `new_coverage`, 80%).
- **Reasoning**: Every test that runs the checks (T020) passes `--skip-http-redirect` and "a short warm-up" on
  a host that already serves, and T006 stops at usage errors. So no task tests: the status codes and the
  `Location` comparison of check 2; the retry loop of check 1, its end at the warm-up time, and the
  `(retried)` marker that decides whether a sample counts for SC-001; and the branch of
  `SmokeCheckCommand.RunAsync` that builds the real client factory and runs the checks (T010). Check 2 is not
  skipped on Azure: a defect in it fails step 12 of every `deploy` and `refresh`, and the first time that code
  runs is the first deployment, which R-15 places on the last days before 2026-11-23. T004 narrows the
  coverage exclusion so that this code is measured, which makes the untested branches visible in the PR
  gate as well. Research R-11 lists what cannot be tested before Azure exists (the workflow, the Azure
  settings, forwarded headers, the timings); the logic of check 2 and of the retry is not in that list, and
  the `Func<HttpClient>` factory of the contract is the stated way to run checks without a network.

### MINOR-2: PR 2 sets the slice to `shipped` without the results of B10, and nothing labels them unverified; one part of B5 has no task

- **Where**: tasks.md T046 (line 208), T045 (line 207), T042 (line 204), Phase 8 order (line 223),
  "Incremental delivery" (lines 267–270).
- **Against**: constitution V ("Anything not executed MUST be labelled unverified") and Workflow and Quality
  Gates (a PR description gives "anything left unverified"); plan, Constitution Check V ("the timings are
  verified only by quickstart Part B, reported in PR 2"); plan "Requirements to mechanisms" (SC-001, SC-006,
  SC-007, HP-013 verified by B10); quickstart B5 (lines 76–80) and B10 (lines 98–103).
- **Reasoning**: T046 puts "Part B results from T039–T044" in PR 2 and sets the roadmap row to `shipped`;
  T045 (B10) is excluded and its runs "stay in the workflow's run history". B10 is the only evidence for
  SC-001 (at least 10 idle `check` runs), SC-006 and SC-007. T038 names an "unverified until Phase 8" list for
  PR 1, but T046 has no such list for what is still open when PR 2 is written, so the slice is recorded as
  shipped with three success criteria neither verified nor labelled. This also contradicts the plan's
  Constitution Check, which says the timings are reported in PR 2. Separately, quickstart B5 ends with "At
  the first weekly check, the first deployment's lines are still there", the check research R-09 relies on
  for the 30-day retention (HP-010). T042 covers B5 but must finish before T046, at most a day or two after
  the first deployment, and T045 lists only the `check` runs, the timings and the cost. No task carries that
  part of B5.

### MINOR-3: The parse check of T015, T016 and T036 does not work as written, and its nearest working form cannot fail

- **Where**: tasks.md T015 (line 95), T016 (line 96), the Phase 2 checkpoint (line 98), T036 (line 188).
- **Against**: constitution V (executed checks); the command itself, run read-only against
  `tools/pr-gates/selftest.ps1` in this worktree.
- **Reasoning**: The command is given in double quotes:
  `pwsh -NoProfile -Command "[...Parser]::ParseFile('…',[ref]$null,[ref]$e) | Out-Null; $e.Count"`. Run from
  PowerShell and from Git Bash, the calling shell expands `$null` and `$e` before `pwsh` sees them; the result
  both times was `MethodException: Argument: '2' should be a System.Management.Automation.PSReference` and
  `.Count: The term '.Count' is not recognized`, and no `0`. With single quotes, so that nothing is expanded,
  the result was `InvalidOperation: [ref] cannot be applied to a variable that does not exist.` followed by
  `0`: `$e` was never assigned, the parser was not called, and `$null.Count` is 0. That form prints the
  expected `0` for any file, including one with syntax errors. "Both scripts parse" in the Phase 2
  checkpoint and in T036 therefore rests on a check that either errors or always passes.

### MINOR-4: T024 and T037 need the local secret values, and no task says who performs them

- **Where**: tasks.md T024 (line 138), T037 (line 189); "Rules for every task" (lines 36–37); Notes (lines
  278–279); Phase 4 checkpoint (line 140); dependencies (line 222).
- **Against**: the user's CLAUDE.md, Security ("Never read, print, ... transmit ... secret values ...
  shell commands"; "reference it indirectly ... rather than retrieving or embedding its value"); project
  CLAUDE.md, Build and test ("never read or print their values"); quickstart A2 and A4, which address the
  Operator ("by your own means", "with your own tool").
- **Reasoning**: T024 runs the tool with the three `HT_SMOKE_*` variables "set from your User Secrets by your
  own means", and T037 searches the repository "for the values in your local User Secrets with your own
  tool". Both sentences are copied from the quickstart, where "you" is the Operator. In tasks.md they sit in
  Phases 4 and 7, which the agent executes; only Phase 8 is assigned to the Operator, and the Notes say only
  that the agent never reads the *hosted* values. The first rule for every task forbids the agent to read a
  secret value or pass one as an argument, so the agent cannot perform either task as written, and the tasks
  do not say that the user does. T024 is part of the Phase 4 checkpoint ("passes in process and locally") and
  T037 precedes PR 1. T024 also asks for "an emptied local database" without saying which database or how it
  is emptied; the app's local database is the developer's `HT` database (README, Getting started), which no
  test or tool of the repository is allowed to reset (`HostFixture.EnsureResettable`, ADR 0006).

### MINOR-5: T015 calls every step idempotent, but a run that stops after creating the SQL server never writes the connection string

- **Where**: tasks.md T015, the list header (line 86), step 4 (line 90) and step 6 (line 92); T032 (line 184).
- **Against**: research R-03 ("Idempotent: each resource is created when missing and its settings are set
  again"); HP-012 (values set once and reused); contracts/hosted-configuration.md (the SQL password is
  "stored nowhere else than in the connection string"); contracts/deploy-workflow.md step 3.
- **Reasoning**: The SQL administrator password is generated in memory in step 4 and written to the web app
  only in step 6, "only when the server was created or rotated", after the database, the firewall rules and
  three interactive prompts (one of which refuses input until it is valid). If the run stops anywhere between
  (a failed `az` call in step 5, or the Operator leaving a prompt), the password is gone. On the next run the
  server exists, so by step 4 "the server and the stored connection string are left as they are" and by step
  6 no connection string is written: the web app has none, and the script finishes without saying so. The
  first sign is workflow step 3 failing with `configuration incomplete`. The only way out is
  `-RotateSqlPassword`, which T015 and T032 describe as secret rotation, not as the recovery for this state.
  With provisioning planned for the same day as the implementation (R-15), a first run that stops midway is a
  realistic case.

### MINOR-6: The smoke check's output is undefined in cases that T020 and quickstart B7 produce

- **Where**: tasks.md T011 (line 79), T012 (line 80), T020 (line 131), T022–T023 (lines 136–137).
- **Against**: contracts/smoke-check.md, Checks (line 31), Timings (lines 42–51) and Output (lines 74–77);
  quickstart A2 (line 41) and B7 (lines 88–91).
- **Reasoning**: T011 says "Output lines exactly as the contract's Output section", and the tests of T020 are
  written before the implementation, so both need the same definition. Four cases have none.
  (1) `later` is measured "after the first demo account's sign-in" (T012). In T020 (b) and in B7 the demo
  password fails the password rule, no demo account exists and that sign-in fails; neither T012 nor the
  contract says what the `TIMING` line contains then. (2) The format `PASS <name> (<found> of <expected>)`
  has counts; `home`, `https-redirect` and `administrator` compare no list, and no task says what their line
  looks like (T022 defines it for `catalog` only). (3) With `--skip-http-redirect`, T011 says the check is
  "skipped" but not whether a line is printed; quickstart A2 expects `PASS` for `home`, `catalog`,
  `administrator` and the demo emails only. (4) T020 (a) asserts "every line is `PASS`", which the `TIMING`
  and `RESULT` lines of the same output do not satisfy. In addition, check 1 passes on "200 with the visitor
  Home" (T011), but no task says what in the page identifies the visitor Home, and the `PageReader` API of
  T005/T009 has no member for it.

### MINOR-7: T026 cites a convention of `sonar.yml` that the file does not have

- **Where**: tasks.md T026 (line 157), T027 (line 158, "`actions/setup-dotnet` 10.0.x").
- **Against**: `.github/workflows/sonar.yml` lines 44, 91, 100 and 106, and its comment at lines 98–99.
- **Reasoning**: T026 says "Pin every action by full commit SHA with a version comment, as
  `.github/workflows/sonar.yml` does." That workflow pins nothing by SHA: it uses `actions/checkout@v5`,
  `actions/setup-java@v5`, `actions/setup-dotnet@v4` and `actions/cache@v4`. No input artifact (ADR 0015,
  research R-04, the workflow contract) asks for SHA pinning either, so the task introduces a rule and
  attributes it to an existing practice that is not there. The one convention `sonar.yml` does record for
  these actions is not carried over: its comment explains that `setup-dotnet@v5` failed to download the SDK
  (HTTP 409) and that v4 is used for that reason. T027 names the action without a version, so the SHA an
  implementer picks may be the version the repository deliberately avoids, in step 4 of every run.

### MINOR-8: T029 asks for one named step per contract step, but contract steps 1, 2 and 4 each need several workflow steps

- **Where**: tasks.md T027 (line 158) and T029 (line 160, "Every step has `name: \"<n> <name>\"` matching the
  contract so the summary can name it").
- **Against**: contracts/deploy-workflow.md, Steps 1, 2, 4 and 13 ("failed step number and name"); GitHub
  Actions step syntax (a step has either `uses` or `run`).
- **Reasoning**: As T027 describes them, step 1 is a ref check (a command) and `azure/login` (an action), step
  2 is `actions/checkout` and git commands, and step 4 is a checkout, `actions/setup-dotnet`, a tool restore
  and up to three builds. An action and a command cannot share one step, so these three contract steps become
  at least seven workflow steps. T029 does not say how they are named or numbered, nor how step 13 derives
  "the number and name of the first failed step" from `steps.*.outcome` when several workflow steps belong
  to one contract number. HP-009 (the reason of a failure) and the row-by-row review of T030 depend on that
  mapping.

### MINOR-9: User Stories 1 and 2 are called independent and parallel, but both compile in `HT.UiTests`

- **Where**: tasks.md "Phase dependencies" (line 219, "Independent of US1"), "Parallel opportunities" (line
  234), the parallel example (lines 246–251), T018 (line 115), T019–T020 (lines 130–131).
- **Against**: the tasks' own definition of [P] and of parallel work (line 30, "no dependency on an
  unfinished task"); `tests/HT.UiTests/HT.UiTests.csproj` (one project for all these tests).
- **Reasoning**: The statement "touch different files and can run in parallel" is true for the files but not
  for the build. T019 and T020 are written first and do not compile until T021–T023 exist
  (`SmokeCheckCoverage`, checks 3–5). While they do not compile, `HT.UiTests` does not build, so T017 cannot be
  run to show that it fails before T018, and T018's condition "T017 passes; the whole `HT.UiTests` suite
  still passes" cannot be checked. The same holds inside Phase 2 for T005/T006 against T008. The parallel
  example pairs exactly these tasks (T017/T018 with T019/T021).

### MINOR-10: The test namespace `HT.UiTests.SmokeCheck` hides the type `SmokeCheck` in the host tests of T020

- **Where**: tasks.md T005 (line 67, namespace `HT.UiTests.SmokeCheck`), T011 (line 79, `public sealed class
  SmokeCheck`), T020 (line 131, file in `tests/HT.UiTests/Host/`, "running `SmokeCheck`").
- **Against**: C# name lookup (members of an enclosing namespace are found before the using directives of the
  compilation unit); the existing namespace `HT.UiTests.Host` (`HostFixture.cs` line 10).
- **Reasoning**: T005 puts the new unit tests in the namespace `HT.UiTests.SmokeCheck`. From then on the
  namespace `HT.UiTests` has a member named `SmokeCheck`. In `SmokeCheckHostTests`, in `HT.UiTests.Host`, the
  simple name `SmokeCheck` therefore binds to that namespace, not to the tool's class, whatever `using`
  directives the file has, and the use the task describes is a compile error (a namespace used like a type).
  The class also has the name of its own namespace, `HT.SmokeCheck`. The tasks fix both names and do not
  mention the clash. Not built; derived from the lookup rules.

### MINOR-11: The roadmap row is updated late and with a Horizon that contradicts its status

- **Where**: tasks.md T035 (line 187), Phase 7 dependencies (line 222).
- **Against**: `docs/roadmap.md`, Horizons (lines 52–53: "*Now* = a Spec Kit folder exists and the slice is
  being built"), "Updating this file" (lines 244–246: "the feature branch updates its row's Status after each
  step") and row 3 (line 32: status `planned`, Horizon "Later (built last; starts by 2026-11-20, R2)");
  `specs/002-habit-catalog/tasks.md` T001 (the status is set to `implementing` as the first task).
- **Reasoning**: T035 sets row 3 to `implementing` in Phase 7, after the implementation of Phases 1–6, and
  says to "keep Horizon ... unchanged". During the whole implementation the branch's roadmap therefore says
  `planned`, and afterwards it shows a slice that is `implementing` with Horizon "Later", which the roadmap
  defines as directional and not being built. Slice 2 set the status in its first task. The row also still
  reads `planned` now that tasks.md exists; the status `tasks ready` of the roadmap's list is set by no task
  and by no other change in the working tree (`git status`: only `tasks.md` is new).

### MINOR-12: T038 names principles II–VI as touched; the plan marks II and IV "not touched" and lists I

- **Where**: tasks.md T038 (line 190, "constitution principles touched II–VI").
- **Against**: plan.md, Constitution Check (lines 69–75); constitution, Workflow and Quality Gates (a PR
  description "name[s] the principles it touches").
- **Reasoning**: The plan's table marks II (Deterministic, Reproducible Evaluation) and IV (Design-System
  Fidelity) as "met (not touched)": the slice has no evaluation logic and changes no page. It marks I as met
  with evidence from this slice (the smoke check's output vocabulary, the `CONTEXT.md` terms). T038 tells the
  PR description to name II–VI, which includes the two principles the plan says are untouched and leaves out
  I. PR review checks the principles named in the description (constitution, Governance), so the list
  matters.

## Summary

1 MAJOR, 12 MINOR.

The tasks cover the requirements, success criteria and quickstart steps of the inputs, and their facts about the
existing code hold where checked (the demo data port and its parameter types, table and column names, the
sign-in form, the prerendered catalog and custom-habit pages, the 23 architecture cases, the derived-database
pattern, the demo-data gate against `origin/master`, the task counts). They are not ready for
`/speckit-implement` as ordered: the architecture test of T007 cannot compile where it is placed and cannot pass
before Phase 4, while Phase 2 must be green before Phase 4 may start, and the pre-commit gate runs that test.
The MINOR notes concern verification that is thinner than it looks (check 2 and the retry path untested, a
parse check that cannot fail, B10 and part of B5 outside PR 2), two tasks without an actor because they need
secret values, a provisioning rerun that loses the connection string, undefined smoke-check output in tested
cases, and several inconsistencies with `sonar.yml`, the roadmap and the plan's Constitution Check.
