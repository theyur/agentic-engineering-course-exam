# Critic pass 2: tasks for 001-accounts-and-access

**Verdict**: REVISE
**Date**: 2026-11-11
**Reviewed**: `specs/001-accounts-and-access/tasks.md` (T001–T127), against `spec.md`, `plan.md`, `research.md`,
`data-model.md`, `quickstart.md`, `contracts/application-api.md`, `contracts/host.md`, `acceptance.md` and
`tests/HT.AcceptanceTests/Features/F001/` (coverage context), the constitution, ADRs 0003–0009, `SPEC.md`,
`ARCHITECTURE.md`, `CLAUDE.md`, `CONTEXT.md`, `README.md`, `docs/roadmap.md`, `design/`, the existing code, tests,
hooks and CI workflow, and the AC extension's scripts and commands (`.specify/extensions/ac/`,
`.workflow/config.json`).

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 no design task changes `ht-components.css` | Resolved | T014 (lines 97–103) adds the avatar menu, the click-away layer, `#blazor-error-ui` and the reconnect rules to `design/css/ht-components.css`. T011 and T013 point to it and no longer need the stylesheet, so their [P] holds. T053 depends on T014, T059 compares the app copy with `design/css/`, T124 cites T059. The rule list matches `MainLayout.razor.css` lines 1–20 and `ReconnectModal.razor.css` lines 1–20. |
| MINOR-1 story independence and checkpoints | Partly | Lines 658–676 now give the US5, US6 and US7 dependencies; the checkpoints (lines 321–323, 388–390) and the MVP stop (lines 733–734) no longer claim a Sign out control or the signed-in Home. The statement for US4 still does not hold: MINOR-1 below. |
| MINOR-2 `StartupTasks` tests: configuration and starting state | Resolved | T046 reads `IConfiguration` from the provider; T050 gives the harness (drop per run, in-memory configuration, no-drop overload, one collection); Conventions lines 44–48 name the third database and its derivation; T047 names `Fakes/ListLogger.cs`. |
| MINOR-3 password change for a deleted account | Resolved | T104 (line 537) looks the account up first and returns `NotSignedIn`; T101 (line 526) tests it. Matches `contracts/application-api.md` ("An acting user whose account no longer exists gives `NotSignedIn`"). |
| MINOR-4 race-limit test | Resolved | T029 (lines 148–149): lost saves alternate between two unblocked states, plus `BlockOnNextSave`. T068 (lines 342–343): 9 lost races then success, 10 lost races fail. Consistent with research R-05 and T071. |
| MINOR-5 architecture rule scope | Resolved | T089 (line 452) and T118 (line 603) limit the rule to types of the `HT.Ui` assembly. The stated rationale is sound: `Program.cs` calls only `AddHtApplication` and `StartupTasks`, so no `HT.Ui` type outside `HT.Ui.Operator` needs the operator services. |
| MINOR-6 Error page has no test | Resolved | T058 (lines 256–258) requests `/Error` and asserts the absent texts and values. |
| MINOR-7 CI secret not carried into the gate documents | Partly | T121 (line 626), T122 (line 628) and "Owner actions" (lines 746–750) cover the workflow's setup comment, the README row and the owner's prerequisite. Two more places still name `SONAR_TOKEN` alone: MINOR-3 below. |
| MINOR-8 [P] markers | Resolved | T036 has no [P]; T057 and T058 have no [P] and state their dependencies; "Parallel opportunities" (lines 693–699) matches. |
| MINOR-9 test access | Partly | Access is now stated: T004 and T006 (`InternalsVisibleTo`), T034 (`internal` context), T108 (`internal` stamp check), T111 (host collection, `HT-UI`). Two problems remain: the access mechanism chosen for `HT.Persistence` is the subject of MAJOR-1, and one assertion of T111 still has no access path (MINOR-2). |
| MINOR-10 `TimeProvider.Testing` version | Resolved | Conventions lines 49–50 and T005, T041, T047 pin `10.11.0`, the version in `tests/HT.AcceptanceTests/HT.AcceptanceTests.csproj`. |

Pass 1 is the only earlier review of the tasks, so no older note could regress. The author rejected no pass-1
finding. Two rationales written into the tasks were judged: T089's scope (sound, see MINOR-5 above) and T004's
"stays out of the public surface the AC tests see" (not sound, see MAJOR-1).

## Notes

### MAJOR-1: `InternalsVisibleTo` in `HT.Application` and `HT.Persistence` makes the AC reference export fail, so `/speckit-ac-tests-write` cannot start

- **Where**: tasks.md T003 (line 60) and T004 (line 61). Tasks that rely on them: T026 (line 139), T028
  (lines 141–143), T029, T034 (line 158), T037, T047 (line 220), T048 (line 225), T049 (line 226), T087
  (line 450), T111 (lines 558–562), T114 (line 590), T116 (line 597). "Incremental delivery" item 4
  (lines 741–742).
- **Against**: `.specify/extensions/ac/scripts/export_refs.py` lines 7–9, 57–65, 116 and 133–135;
  `.workflow/config.json` (`acSurfaceProjects`); `.claude/skills/speckit-ac-tests-write/SKILL.md` line 72 (step 5);
  `.specify/extensions/ac/commands/speckit.ac.init.md` lines 130–131; `CLAUDE.md` "Acceptance tests" (the flow
  `/speckit-implement` → `/speckit-ac-tests-write` → `/speckit-ac-tests-analyze`); plan.md work order step 8;
  ADR 0003.
- **Continues**: new. T003 follows research R-13; the `InternalsVisibleTo` items of T004 were added in this revision
  in answer to pass-1 MINOR-9.
- **Reasoning**:
  - `.workflow/config.json` lists `src/HT.Application`, `src/HT.Domain` and `src/HT.Persistence` as the AC surface
    projects.
  - For each of them, `export_refs.py` (`check_no_internals_visible_to`, lines 57–65, called at line 116) reads
    every `.cs`, `.csproj`, `.props` and `.targets` file of the project folder and exits with
    "FAIL: InternalsVisibleTo puts internal members into the reference assembly" when the text
    `InternalsVisibleTo` occurs. T003 and T004 put `<InternalsVisibleTo>` items into `HT.Application.csproj` and
    `HT.Persistence.csproj`.
  - A second check (lines 133–135) fails when the listing of a public type still declares a non-public member.
    With the attribute present the reference assembly keeps internal members (the script's own message says so),
    and T028, T087 and T116 give the public services `internal` constructors.
  - `/speckit-ac-tests-write` runs `export_refs.py` as step 5: "If it fails: stop and report the exact problem it
    prints." So after `/speckit-implement` the AC step stops before the test writer starts. The expectation in
    lines 741–742 (the 63 scenarios pass against `HT-AC`), plan work order step 8 and the roadmap status
    `AC tests green` cannot be reached with the tasks as written.
  - The export has passed so far only because no surface project contains the attribute today (no match in `src/`
    or `Directory.Build.props`).
  - The attribute is not a detail of one task. The internal `IAccountStore` is implemented in another assembly
    (T037) and by the fake (T029, linked in T047); the internal constructors are called from three test projects;
    the internal `HtDbContext` is reached from T049, T111 and T114. No task, note or owner action mentions the
    export check.
  - T004 states that the internal context "stays out of the public surface the AC tests see (ADR 0003)". The
    extension states the opposite about the mechanism used: lines 7–9 and the failure message of
    `export_refs.py`, and `speckit.ac.init.md` lines 130–131 ("no `InternalsVisibleTo` in AC surface projects (it
    leaks internals into the reference assemblies the test writer sees)").

### MINOR-1: "US1, US2, US3, US4: no dependency on another story" does not hold for US4

- **Where**: tasks.md lines 664–665 and 701–702; T084 (lines 436–441), T085 (lines 442–445); US4's Independent
  Test (lines 431–432).
- **Against**: tasks.md T063 (line 291, `RegisterAsync`, US1) and T071 (line 354, `SignInAsync`, US2); spec.md
  US4-AS1, US4-AS2, US4-AS5 and US4-AS7.
- **Continues**: pass-1 MINOR-1 (new instance of the same statement).
- **Reasoning**: Lines 664–665 say that each of these stories has "its own `AccountService` or operator method,
  and its tests create accounts through the store or the fake store", and lines 701–702 let US1 to US4 be built
  side by side. But T085 states "a created administrator signs in through `AccountService.SignInAsync`", which
  T071 adds in US2, and T084 requires "registering `fifth.admin@example.com` and `seventh.admin@example.com`
  afterwards succeeds", which needs `RegisterAsync` from T063 in US1. US4-AS1 and AS2 ("signs in with
  `curator2026`") need the sign-in check as well. US1's Independent Test (lines 270–271) was reworded to avoid
  sign-in; US4's still says "sign in as the administrator". Built before or beside US1 and US2, T084 and T085 do
  not compile.

### MINOR-2: T111 asserts `RevalidationInterval`, a protected member with no stated access path

- **Where**: tasks.md T111 (line 562: "`RevalidationInterval` is 30 minutes"), T108 (line 549), T006 (line 64).
- **Against**: `RevalidatingServerAuthenticationStateProvider` in `Microsoft.AspNetCore.Components.Server` 10.0.14
  (`protected abstract TimeSpan RevalidationInterval { get; }`, read from the installed reference assembly with
  `ilspycmd`); research R-16 ("and its `RevalidationInterval` is 30 minutes").
- **Continues**: pass-1 MINOR-9, second bullet.
- **Reasoning**: The revision answers the protected `ValidateAuthenticationStateAsync` with one internal method,
  `IsSecurityStampCurrentAsync`, and T006 grants `InternalsVisibleTo` "for T108's stamp check". The interval
  property is protected in the base class too, and an override cannot widen it. T108 adds no other member, so the
  fourth assertion of T111 has the same gap the pass-1 note described for the validation method.

### MINOR-3: Two more places still say the Sonar job skips only without `SONAR_TOKEN`

- **Where**: tasks.md T121 (line 626: "the file's setup comment"), T122 (line 628).
- **Against**: `.github/workflows/sonar.yml` line 93 ("a run without SONAR_TOKEN succeeds without analyzing");
  `.claude/skills/process-pr-feedback/SKILL.md` line 57 ("`SKIPPED` (Sonar ran without `SONAR_TOKEN` - **not
  green**)") and line 64 ("restore `SONAR_TOKEN`"); research R-14.
- **Continues**: pass-1 MINOR-7.
- **Reasoning**: After T121 a skipped run has two possible causes. T121 names only the setup comment at the top of
  the workflow, and T122 only the README row. The inline comment at line 93 of the same file and the skill that
  drives the PR gates keep the single cause. The skill is what tells the user the "likely user action" for a
  `SKIPPED` gate, and it is used on this slice's own PR, where the missing secret is the new one ("Owner
  actions", lines 748–750).

### MINOR-4: The only host-level check of the startup log is an assertion placed in a fixture

- **Where**: tasks.md T090 (line 462: "Also assert in `HostFixture` that the host's start logged an
  `Administrator creation:` line"); T047 (line 221); T120 (lines 608–613).
- **Against**: research R-16 ("Host tests cover … the startup tasks' operator log"); ADR 0006 (host tests "for
  what needs the running host: the startup tasks and their operator log"); spec.md HP-018 and HP-021 ("At every
  start, the host MUST run …").
- **Continues**: new (T090 was rewritten in this revision).
- **Reasoning**: T090's file is in the `StartupTasks` collection, and `HostFixture` is the fixture of the host
  collection, so the sentence cannot be a test in T090's file. It puts the check into the fixture itself, which
  has no test of its own: a failure there shows up as a fixture error on every host test (T049, T058, T067, T076,
  T100, T111, T112). The task does not say at which point of the fixture the host has started. All other
  `StartupTasks` tests call `StartupTasks.RunAsync` through the harness, so this sentence is the only check that
  `Program.cs` runs the startup tasks at all (T046, T088, T117), and it has no named test. T120 has no host-level
  counterpart for the demo lines of HP-021.

## Summary

1 MAJOR, 4 MINOR.

The revision resolves the pass-1 MAJOR note and seven of the ten MINOR notes; coverage of the FR, HP and SC items,
the task numbering (127 tasks, no gaps), the cross-references and the 63-scenario count check out. The blocking
problem is new and concerns the step after implementation: the tasks add `InternalsVisibleTo` to two AC surface
projects, and the AC extension's reference export refuses any surface project that contains it, so
`/speckit-ac-tests-write` would stop at its export step. The internal store port, the internal service
constructors and the internal `HtDbContext` all depend on that attribute, so the conflict reaches many tasks and
is cheaper to settle before `/speckit-implement` than after it. The MINOR notes are a dependency statement that
is still false for US4, one test assertion without access, two leftover gate descriptions and an unplaced
host-level log check.
