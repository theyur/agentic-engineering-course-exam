# Critic pass 2: tasks for 003-demo-hosting

**Verdict**: APPROVE
**Date**: 2026-11-15
**Reviewed**: `specs/003-demo-hosting/tasks.md` (T001–T049, rewritten after pass 1), against `spec.md`,
`checklists/requirements.md`, `plan.md`, `research.md`, `data-model.md`, `quickstart.md`,
`contracts/deploy-workflow.md`, `contracts/smoke-check.md`, `contracts/hosted-configuration.md`,
`critic-tasks-pass-1.md`, `critic-plan-pass-1.md`, `critic-plan-pass-2.md`, ADRs 0001, 0006 and 0015, the
constitution, `SPEC.md`, `ARCHITECTURE.md`, `CLAUDE.md`, `CONTEXT.md`, `README.md`, `docs/roadmap.md`,
`specs/002-habit-catalog/tasks.md` (conventions), and the existing code and tests: `src/HT.Ui/Program.cs`,
`src/HT.Ui/Operator/StartupTasks.cs`, `src/HT.Ui/HT.Ui.csproj`, `src/HT.Ui/Components/` (App, Home, SignIn,
SignInForm, Catalog, CustomHabits, HabitCard), `src/HT.Application/Operator/` (`IDemoDataStore`,
`DemoDataService`, `DemoCustomHabits`, `SeedCatalog`, `SeedCatalogHabit`), `src/HT.Persistence/HtDbContextFactory.cs`,
`HtDatabase.cs` and `Migrations/`, `HabitTracker.slnx`, `Directory.Build.props`, `dotnet-tools.json`,
`.editorconfig`, `.gitattributes`, `.github/workflows/sonar.yml`, `.githooks/pre-commit`,
`tools/pr-gates/selftest.ps1`, `.workflow/config.json`, `tests/HT.UiTests` (csproj, `Host/HostFixture.cs`,
`Host/HostClient.cs`, `Host/HostDatabaseGuardTests.cs`, `Operator/StartupTasksHarness.cs`) and all of
`tests/HT.ArchitectureTests`.

Nothing was built and no test was run (critic rules). Two read-only checks were executed: the PowerShell
parser call of T016 (an input with a syntax error gave 1 error, `tools/pr-gates/selftest.ps1` gave 0), and a
rule lookup on the project's SonarQube Cloud instance (MINOR-2).

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 architecture test cannot compile where placed; Phase 2 checkpoint unreachable | Resolved | T002 creates the stub `SmokeCheckCommand` and T004 the project reference in Phase 1; T009 uses `typeof(HT.SmokeCheck.SmokeCheckCommand)`, says it is a guard that passes from the start, and asserts a subset of `HT.Application` and `HT.Domain`, which holds with zero, one or two references. "Within phases" names T009. The 23 existing cases were recounted (4 + 3 + 2 + 2 + 1 + 7 + 4) and none of their rules is scoped so that a fifth loaded assembly changes it. |
| MINOR-1 check 2 and the warm-up retry untested | Resolved | T008 (a)–(d) test the retry, the `(retried)` marker, the end of the warm-up time, the four redirect codes, the `Location` comparison and the skip, through `RunAsync` with a fake handler (T002's fourth parameter, T012's factory). The sequencing of T008 (e) and (f) is MINOR-1 below. |
| MINOR-2 PR 2 ships without B10; last part of B5 has no task | Resolved | T046 lists SC-001, SC-006, SC-007 and the 30-day retention as still unverified with their tasks; T047 carries the last sentence of quickstart B5; T048 is B10; T049 records the outcome. A side effect is MINOR-5 below. |
| MINOR-3 the parse check cannot fail | Resolved | T016 declares both `[ref]` variables first and returns `$errors.Count -eq 0`; T015 asserts false for a file with a syntax error and true for a valid one, then that both scripts parse. Executed read-only as described above. |
| MINOR-4 T024 and T037 need secret values and have no actor | Resolved | T026 and T038 are marked (user), defined in "Format" (lines 35–36) and in Notes; T026 names the database `HT-SMOKE-LOCAL` with an integrated-security connection and excludes `HT`. `StartupTasks.RunAsync` migrates in Development, so the database is created at start as T026 says. |
| MINOR-5 a stopped provisioning run loses the SQL password | Resolved | T017 step 4 writes the connection string right after the server is created or updated (the web app exists from step 2), and stops with a message naming `-RotateSqlPassword` when the server exists without a stored connection string; T034 documents that recovery. |
| MINOR-6 smoke-check output undefined in tested cases | Resolved | "Output lines fixed by these tasks" defines the lines without a list, `SKIP https-redirect`, `later n/a`, "every check line", and the visitor Home by its first `h1`, which matches `Home.razor` line 30; `PageReader.ReadFirstHeading` is in T006 and T011. The contract itself is unchanged: MINOR-4 below. |
| MINOR-7 SHA pinning attributed to `sonar.yml` | Resolved | T028 pins by major tag with the versions of `sonar.yml` (lines 44, 100) and carries its comment on `setup-dotnet@v4`. A consequence for `azure/login` is MINOR-2 below. |
| MINOR-8 one named step per contract step | Partly | T029–T031 define ids `s<n>_<k>` and names `"<n>.<k> <text>"`. How step 13.2 gets the name and the first failed step from the `steps` context is still not determined: MINOR-3 below. |
| MINOR-9 US1 and US2 called parallel in one test project | Resolved | The "One test project" section states the constraint; "Phase dependencies" and "Parallel opportunities" no longer pair Phase 3 with Phase 4. |
| MINOR-10 namespace `HT.UiTests.SmokeCheck` hides the type | Resolved | Class `SmokeCheckRunner`, namespace `HT.UiTests.SmokeChecks` ("Names fixed by these tasks"). The stated reason is sound: in any namespace under `HT` other than `HT.SmokeCheck`, the simple name is found as the namespace member of `HT` before the compilation unit's using directives are considered. |
| MINOR-11 roadmap row updated late, Horizon contradicts status | Resolved | T001 is the first task and sets `implementing` with Horizon `Now`; the working tree's row 3 reads `tasks ready`. |
| MINOR-12 principles named in the PR description | Resolved | T039 names I, III, V and VI and marks II and IV as not touched, as plan.md lines 70–75 do. |

No finding of pass 1 was rejected by the author, so there is no rejection rationale to judge. Pass 1 is the only
earlier pass on the tasks; none of its notes has regressed.

## Notes

### MINOR-1: The `later` timing of T014 has no demo sign-in to follow in Phase 2, and T008's scripted "good site" is described once for two different outputs

- **Where**: tasks.md T008 (e) and (f) (line 96, "Fails until T013 and T014"); T013 (line 107, "Checks 3–5 come
  in Phase 4"); T014 (line 108, "T008 passes"); T024 and T025 (lines 164–165); the Phase 2 checkpoint (line
  126); "Within phases" (line 257).
- **Against**: the tasks' own rule that a test fails before its implementation (lines 14–16);
  contracts/smoke-check.md, Timings (lines 47–51: `later` is measured "for the first demo account, after its
  sign-in"); plan.md, "Requirements to mechanisms" (SC-001 rests on the `first` and `later` timings).
- **Continues**: new (in material added for pass-1 MINOR-1 and MINOR-6).
- **Reasoning**: In Phase 2 no task makes the runner sign in a demo account. T013 gives it a private
  `SignInAsync` and defers checks 3–5; T014 times the page "after the first demo account that signs in"; the
  sign-ins themselves are added by T025 in Phase 4, which says the timing "uses the first demo account that
  signs in". At T014 therefore:
  - T008 (e) ("when every demo sign-in returns to `/sign-in`, the timing line has `later n/a`") holds without
    any sign-in being attempted, so it passes before the behaviour it describes exists and never fails first;
  - T008 (f) ("a fully scripted good site gives exit 0 and the output lines of 'Output lines fixed by these
    tasks'") cannot show `later <ms> ms (<n> requests)`, nor any line with a list, because only checks 1 and 2
    exist.
  T014 nevertheless states "T008 passes", and the Phase 2 checkpoint counts T008 among the passing tests. In
  Phase 4, T024 and T025 change both what the same scripted site must serve to exit 0 (`/catalog` with every
  Published seed name, `/admin`, `/custom-habits` with the demo names) and the lines it prints. No task says
  that T008's fake or its expectations change then; T025 only requires that "the whole `HT.UiTests` suite"
  passes. So either the tests written in Phase 2 are weaker than T008 describes and are rewritten in Phase 4
  without a task, or T014 builds part of check 5 that T013 assigns to Phase 4. The affected line is the one
  SC-001 is measured with: T022 asserts `later n/a` only in case (b) and nothing about the `TIMING` line in
  case (a), so the format `later <ms> ms (<n> requests)` has no test with a defined place in the order.

### MINOR-2: T028 pins `azure/login@v2` by tag "as `sonar.yml` does", but `sonar.yml` has no third-party action, and the Sonar instance has a rule against exactly this

- **Where**: tasks.md T028 (line 185); T032 (line 189); T039 (line 217, `/process-pr-feedback`).
- **Against**: `.github/workflows/sonar.yml` lines 44, 91, 100 and 106 (only `actions/checkout`,
  `actions/setup-java`, `actions/setup-dotnet`, `actions/cache`); ADR 0001 (only `GREEN` + `GREEN` on the head
  counts as ready; the loop never weakens a gate; Sonar reports issues in files outside the .NET projects);
  research R-15 (gates, review and merge of PR 1 on 2026-11-21; SC-008 is missed if PR 1 is not merged by
  2026-11-22); SonarQube Cloud rule `githubactions:S7637`.
- **Continues**: new (follows from the change made for pass-1 MINOR-7).
- **Reasoning**: The rule lookup on the project's SonarQube Cloud instance returns `githubactions:S7637`,
  "External GitHub Actions and workflows should be pinned to a commit hash": type Vulnerability, security impact
  High, with the default trusted prefixes `./,$/,actions/,github/`. Every action in `sonar.yml` is under
  `actions/`, which the rule trusts, so the existing workflow is no precedent for a tag on an action of another
  owner. `azure/login` is not under a trusted prefix, and it is the step that receives the OpenID Connect token
  (`id-token: write`). If the rule is active in the project's quality profile (not checked: the lookup shows
  the rule, not the profile), the new workflow file raises a vulnerability on new code in PR 1, on the Sonar
  gate that `/process-pr-feedback` waits for, and the merge of PR 1 is the step R-15 calls critical for SC-008.
  T028 states the tag as an existing convention and neither T028 nor T032 mentions the rule.

### MINOR-3: Step 13.2 cannot take the failed step's name from the `steps` context, and "the first step whose outcome is failure" has no defined order

- **Where**: tasks.md T031, step 13.2 (line 188); T029 (line 186, ids and names); T032 (line 189).
- **Against**: contracts/deploy-workflow.md step 13 ("failed step number and name") and Guarantees (lines
  67–70); HP-009 (the reason of a failure); GitHub Actions `steps` context (per step id: `outputs`, `outcome`,
  `conclusion`).
- **Continues**: pass-1 MINOR-8.
- **Reasoning**: T031 finds "the contract step number and the workflow step name of the first step whose
  outcome is `failure`" by "reading `${{ toJSON(steps) }}` in the order of the `s<n>_<k>` ids". Three points
  are open.
  1. The `steps` context holds no step name, only outputs, outcome and conclusion per id. The number can be
     read from the id; the name `"<n>.<k> <text>"` of T029 is not in the data the task names as its source, so
     the summary needs a second list of names that nothing ties to the `name:` values.
  2. "In the order of the ids" does not say which order. Compared as strings, `s10_1` to `s13_1` come before
     `s1_1` (`3` sorts before `_`), and before `s2_1` to `s9_1`. The order matters whenever two steps have
     outcome `failure`, which the `always()` step 13.1 makes possible: after a failed sign-in in 1.2 it runs
     without an Azure session.
  3. The same context contains `steps.s3_1.outputs`, the four values T029 writes to step outputs. T031 does not
     say how the context reaches the script of 13.2, while T032's review rule is that secrets reach commands
     only through `env:`.

### MINOR-4: The renamed class, the new public types and the new output lines exist only in tasks.md; the contract and the plan keep the old ones, and one sentence of the new section is not true

- **Where**: tasks.md "Names fixed by these tasks" (lines 47–53) and "Output lines fixed by these tasks" (lines
  55–65); T002 (fourth parameter `createHandler`); T012–T013 (`SmokeCheckOptions`, `SmokeCheckResult`); T023
  (`CoverageEntry`); T021 (its failure message points to the contract); Phase 7, T034–T036.
- **Against**: contracts/smoke-check.md lines 24–27, 74–77 and 86–90; plan.md lines 108 and 122–124;
  research.md R-06 (line 137); tasks.md lines 8–11, which name these documents as inputs.
- **Continues**: pass-1 MINOR-10 and MINOR-6 (how they were resolved).
- **Reasoning**: The contract's "Public surface" lists `SmokeCheckCommand`, `SmokeCheck`, `PageReader` and
  `SmokeCheckCoverage`, a three-argument `RunAsync`, and an output of `PASS`, `FAIL`, `TIMING` and `RESULT`
  lines. The tasks build `SmokeCheckRunner` instead of `SmokeCheck`, add the public types `SmokeCheckOptions`,
  `SmokeCheckResult` and `CoverageEntry`, a fourth parameter, the line `SKIP https-redirect` and the value
  `later n/a`, and move the tests to `tests/HT.UiTests/SmokeChecks/`. The reason given for the rename is
  sound, but the sentence "The other public types keep the contract's names: `SmokeCheckCommand`,
  `SmokeCheckOptions`, `SmokeCheckResult`, `PageReader`, `SmokeCheckCoverage`" is not: `SmokeCheckOptions` and
  `SmokeCheckResult` appear in no input document. No task of Phase 7 touches the contract, the plan's source
  tree or the research. After PR 1 the contract, which T021's failure message sends a later slice to and which
  ADR 0015's Consequences rely on ("a slice that changes them changes the check"), describes a class that does
  not exist and an output without the `SKIP` line.

### MINOR-5: T049 adds a change after the slice is recorded as shipped with "both PRs", which the plan and the roadmap rule do not provide for

- **Where**: tasks.md T049 (line 238, "in a small change of its own"); T046 (line 235, "row 3 `shipped` … and
  both PRs in Shipped"); T034 (line 212, the empty "Results" section); "Incremental delivery" (lines 304–307).
- **Against**: `docs/roadmap.md`, "Updating this file" (lines 252–253: "the last PR sets `shipped` and lists
  all of the slice's PRs in Shipped"); plan.md, Scale/Scope ("2 PRs") and work order step 9; research R-10
  ("The check history is the workflow's run history, not a file", with a committed check log rejected) and
  R-15; CLAUDE.md, "Docs and git workflow" and "PR review" (one branch per task, PRs against `master`,
  `/process-pr-feedback` after opening a PR).
- **Continues**: pass-1 MINOR-2 (how it was resolved).
- **Reasoning**: T046 makes PR 2 the slice's last PR: it sets `shipped` and lists two PRs. T049 then changes
  `docs/operations/hosted-demo.md` after 2026-12-31 "in a small change of its own". By the repository's rules
  that is a third pull request of this slice, so PR 2 is not the last one, the Shipped entry does not list all
  of the slice's PRs, and the plan's count of two PRs no longer holds. T049 does not say whether the change is
  a PR, whether the review flow applies, or whether the roadmap is touched. It also puts check results into a
  committed file, where R-10 decided that the run history is the record; one entry at the end is not the weekly
  log R-10 rejected, but the tasks do not mention the difference.

### MINOR-6: PR 1's description leaves out the result of the migration bundle trial

- **Where**: tasks.md T039 (line 217: "test counts; the results of T026 and T038"); T027 (line 181).
- **Against**: constitution, Workflow and Quality Gates (a PR description gives "the verification evidence");
  plan.md, "Requirements to mechanisms" (HP-004 verified by "A3; B2") and Constitution Check V ("a local run of
  the migration bundle"); research R-05.
- **Continues**: new.
- **Reasoning**: T039 lists what the description of PR 1 carries: test counts, the results of T026 (quickstart
  A2) and T038 (A4), and the unverified items. T027 (quickstart A3) is the only evidence for HP-004 that exists
  before Azure, and the plan's Constitution Check cites it. It is neither a test with a count nor one of the
  two named results, so by T039's list it is not reported, while the workflow steps that depend on it (10.1)
  are listed as unverified until Phase 8.

## Summary

0 MAJOR, 6 MINOR. Previous notes: 12 Resolved, 1 Partly, 0 Unresolved.

The rewrite resolves the pass-1 MAJOR: the architecture guard compiles from Phase 1 on and its assertion holds
at every checkpoint, so the pre-commit gate stays usable throughout. The facts the tasks state about the
existing code were checked and hold: the 23 architecture cases, the `IDemoDataStore` signatures and the four
coverage entries, `DemoDataService.LoadAsync` returning before the seed catalog when the demo password fails the
password rule, the `CustomHabits.OwnerId` column, the sign-in form and its field names, the visitor Home's first
`h1`, the habit card's name element, the stylesheet and script links of `App.razor`, the derived-database
pattern, the exclusion string in `sonar.yml`, the demo-data gate against `origin/master`, and the task counts
(49). Every HP requirement, success criterion and quickstart step has a task. The remaining notes do not block
`/speckit-implement`: the `later` timing and T008's last two cases have no consistent place between Phase 2 and
Phase 4, the tag on `azure/login` may meet a Sonar rule on the PR gate that R-15 puts on the critical path, the
run summary's failed-step lookup is underdetermined, and the contract, the roadmap rule and PR 1's evidence
list are not aligned with what the tasks now build.
