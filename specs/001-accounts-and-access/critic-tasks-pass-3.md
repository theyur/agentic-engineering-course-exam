# Critic pass 3: tasks for 001-accounts-and-access

**Verdict**: APPROVE
**Date**: 2026-11-11
**Reviewed**: `specs/001-accounts-and-access/tasks.md` (T001–T127), against `spec.md`, `plan.md`, `research.md`,
`data-model.md`, `quickstart.md`, `contracts/application-api.md`, `contracts/host.md`, `acceptance.md` and
`tests/HT.AcceptanceTests/Features/F001/` (coverage context), the constitution, ADRs 0003–0009, `SPEC.md`,
`ARCHITECTURE.md`, `CLAUDE.md`, `docs/roadmap.md`, `README.md`, the existing code, tests, hook and CI workflow,
`.claude/skills/process-pr-feedback/SKILL.md`, and the AC extension (`.specify/extensions/ac/scripts/export_refs.py`,
`commands/speckit.ac.tests-write.md`, `commands/speckit.ac.init.md`, `.workflow/config.json`).

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| Pass 2 MAJOR-1 `InternalsVisibleTo` in AC surface projects stops the reference export | Resolved | T003 (line 87) and T004 (line 88) add no `InternalsVisibleTo`; the only item is in `HT.Ui.csproj` (T006, line 91). `HT.Ui` is not in `acSurfaceProjects` and is not a project dependency of one (`HT.Persistence.csproj` → `HT.Application` → `HT.Domain`), so `export_refs.py` line 115 never scans it. The port is public (T026, line 166), the three service constructors are public (T028, T087, T116), and every non-public declaration left in a surface project is a whole type (`HtDbContext` T034, the factory T035, `AccountStore` T037, the migration T040), which `public_only` (lines 73–100) drops. No task puts a non-public member on a public type of a surface project, so the check at lines 133–135 has nothing to find. T049, T111 and T114 no longer use `HtDbContext`. T025 adds `AcSurfaceTests`. Two follow-on gaps: MINOR-1 and MINOR-4. |
| Pass 2 MINOR-1 US4 stated as independent | Resolved | Lines 698–701: US1–US3 have no story dependency; US4's tests need T071 and T063, "Build it after US1 and US2". Line 692 ("some also on another story") and lines 737–738 agree. |
| Pass 2 MINOR-2 `RevalidationInterval` has no access path | Resolved | T111 (line 593) reads the protected property by reflection. The stated reason (an override cannot widen access) is correct for `protected abstract TimeSpan RevalidationInterval`. |
| Pass 2 MINOR-3 two more places name `SONAR_TOKEN` alone | Resolved | T121 (lines 659–660) covers the inline comment (`sonar.yml` line 93) and the skill (`SKILL.md` lines 57 and 64). The only other mention is the accepted ADR 0001, a dated record. |
| Pass 2 MINOR-4 host-level startup-log check placed in a fixture | Resolved | T090 (line 493) creates `tests/HT.UiTests/Host/StartupLogTests.cs` in the host collection and says when the host has started; T120 (line 646) adds the demo lines of HP-021 to it. |

No pass-1 note has regressed: T014, the checkpoints, the harness of T050, T104, the race-limit tests, the rule scope
of T089 and T118, T058, the [P] markers and the pinned `10.11.0` are unchanged. Pass-1 MINOR-9 (test access) is now
answered through public members, plain SQL, reflection and the `HT.Ui` `InternalsVisibleTo`.

Rationales written into the tasks, judged:

- T040 and T035 (internal migration and internal design-time factory are still found): sound. In EF Core 10.0.8,
  `MigrationsAssembly` and `DbContextOperations` enumerate `Assembly.GetConstructibleTypes()` and create the types
  with `Activator.CreateInstance`, with no visibility filter.
- T041 (`Microsoft.Data.SqlClient` "comes with `HT.Persistence`'s SQL Server provider"): sound; the package
  reference flows through the project reference.
- "Plan discrepancies" (the test writer "is still limited by ADR 0003"): sound; ADR 0003 lines 38–39 limit step
  definitions to the use cases and the two entry points, and ADR 0003 is a `specInputs` entry.
- Conventions line 55 (`HT.Ui` may grant `InternalsVisibleTo`): sound, see the first table row.
- T025 ("catches what `export_refs.py` would refuse") and T049 ("the value `Program.cs` passes to
  `AddHtPersistence`"): not fully sound, see MINOR-1 and MINOR-2.

The owner decision of 2026-11-11 is implemented consistently in the tasks themselves: no task still relies on an
internal port, an internal constructor or access to `HtDbContext`. What is incomplete is its record (MINOR-4).

## Notes

### MINOR-1: `AcSurfaceTests` checks the attribute, but `export_refs.py` refuses the text

- **Where**: tasks.md T025 (line 162: "so the pre-commit gate catches what `export_refs.py` would refuse later");
  Conventions lines 51–53; T003 (line 87) and T004 (line 88).
- **Against**: `.specify/extensions/ac/scripts/export_refs.py` lines 57–65 and 115–116;
  `.specify/extensions/ac/commands/speckit.ac.tests-write.md` line 67 (step 5).
- **Continues**: pass 2 MAJOR-1 (follow-on).
- **Reasoning**: `check_no_internals_visible_to` does not look at assemblies. It reads every `.cs`, `.csproj`,
  `.props` and `.targets` file under each surface project folder (and under the folders of their project
  dependencies), plus the root `Directory.Build.props` and `Directory.Build.targets`, and exits when the string
  `InternalsVisibleTo` occurs anywhere in the text. A comment or a string literal is enough. T025's test asserts
  that the three assemblies carry no `InternalsVisibleToAttribute`, which is a narrower condition: a source or
  project file of `HT.Domain`, `HT.Application` or `HT.Persistence` that only mentions the word passes
  `AcSurfaceTests` and the pre-commit gate, and still stops `/speckit-ac-tests-write` at its export step. T003 and
  T004 are instructions about exactly these two project files and are worded "No `InternalsVisibleTo`"; nothing in
  the tasks says that the word itself must not appear there. So the failure of pass 2 MAJOR-1 can still occur
  with a green suite, and the claim in T025 is only partly true.

### MINOR-2: The `HT-UI` guard test (T049) no longer checks the database the host uses

- **Where**: tasks.md T049 (line 255); T034 (line 185); "Plan discrepancies" (lines 58–77).
- **Against**: research.md R-16 lines 321–327 ("values added through `ConfigureAppConfiguration` would arrive too
  late … A guard test resolves `HtDbContext` from the factory and asserts that its database is `HT-UI`, so a
  missed override fails at once"); plan.md Risks, "Database drop in `ResetAsync`".
- **Continues**: new (T049 was rewritten in this revision).
- **Reasoning**: R-16's guard asks the context the host built which database it has. T049 instead reads
  `ConnectionStrings:HabitTracker` from the factory's `IConfiguration` and opens that string to run
  `SELECT DB_NAME()`. Both values come from the configuration after the host is built, not from what `Program.cs`
  read before building the services. They differ in the very case R-16 names: a value that arrives after the
  top-level read is present in the final configuration, so T049 passes, while `AddHtPersistence` received another
  value. `SELECT DB_NAME()` on a connection opened from the configured string returns the database named in that
  string, so it adds no information about the host's context. T049 describes the configuration value as "the
  value `Program.cs` passes to `AddHtPersistence`", which is the property the guard exists to verify, not a
  given. This second departure from the plan (R-16's guard) is also not listed under "Plan discrepancies".

### MINOR-3: Dropping `HT-UI-MIGRATE` and observing "no migration" have no stated mechanism now that the context is out of reach

- **Where**: tasks.md T050 (line 259: "It drops `HT-UI-MIGRATE` first"), T051 (line 263: "with `Testing`, no
  migration is applied"), T048 (line 254: helpers "delete a row by `UserName`, check that a table exists"),
  Conventions lines 53–55 ("read or change database rows with plain SQL").
- **Against**: research.md R-16 lines 328–331 and R-12 lines 253–254; tasks.md T039 (lines 205–207), T034
  (line 185); `SqlServerDatabaseCreator.Delete` / `DeleteAsync` in `Microsoft.EntityFrameworkCore.SqlServer`
  10.0.8.
- **Continues**: new (a consequence of removing test access to `HtDbContext`); same tests as pass 1 MINOR-2.
- **Reasoning**:
  - The only public `HT.Persistence` member that drops a database is `HtDatabase.ResetAsync`, which also applies
    all migrations. Used by the harness, it would make T051's "count above 0" impossible. `HtDbContext` is
    internal and `HT.UiTests` has no access to it, and the SQL helpers T048 lists work on rows and tables. So the
    harness has to drop a database with SQL of its own, which no task says.
  - That drop is not a one-line statement. Each harness run builds services against `HT-UI-MIGRATE` in the same
    process, so the connection pool still holds connections to it when the next test drops it; SQL Server then
    refuses the drop because the database is in use. EF Core's own delete path calls
    `SqlConnection.ClearAllPools()` before its drop commands for this reason. The tasks give the harness neither
    that path nor a note about it.
  - With `Testing` after a drop, the database does not exist. The "check that a table exists" helper cannot
    connect to it, so T051's second assertion has no named way to be observed other than the absence of a log
    line.

### MINOR-4: The record of the owner decision is incomplete, and artifacts the implementation step reads still say the opposite

- **Where**: tasks.md "Plan discrepancies" (lines 58–77: names "research R-13, data-model.md 'Application port'";
  "The tasks do not edit the plan artifacts"); Conventions lines 35–37 ("Use the names in
  contracts/application-api.md and data-model.md"); T089 (line 483).
- **Against**: contracts/application-api.md line 28 ("The account store port `IAccountStore` is internal … and
  not part of this contract"); research.md R-16 lines 325–326 and 339–340; plan.md line 63 (Constitution Check
  III: "The account store port is internal, so no component can reach its lookups"), lines 94 and 145;
  data-model.md lines 69–71; constitution, Decision Discipline (lines 138–143) and Governance (lines 161–162);
  `docs/roadmap.md` lines 142–146; `.claude/skills/speckit-implement/SKILL.md` lines 97–101.
- **Continues**: pass 2 MAJOR-1 (follow-on).
- **Reasoning**:
  - The section lists two places. The same decision is contradicted in at least five more: the contract (which a
    Convention of the same file tells every task to follow), R-16 (the guard through `HtDbContext`, and
    `InternalsVisibleTo` for `HT.UiTests`), and three lines of plan.md. `/speckit-implement` reads plan.md as
    required input and data-model.md, contracts and research.md when present, so the implementer gets both
    statements, and only the two listed ones are marked as overridden.
  - plan.md's Constitution Check marks principle III as met because the port is internal. With the public port
    the reason given there no longer holds; the replacement (T052's rule over the `HT.Ui` assembly) is stated
    only in tasks.md. `/speckit-analyze`, the next step of the workflow, compares these artifacts.
  - The decision rejects an alternative that R-13 had chosen and makes "no `InternalsVisibleTo` in the AC
    surface projects" a standing rule with its own architecture test. The constitution asks for an owner answer
    to be recorded in the spec's `## Clarifications` with its date, and for an ADR when the decision "spans
    features, is hard to reverse, or rejects real alternatives"; the roadmap repeats this for technical rows. The
    decision is recorded only in tasks.md, and no task or owner action carries it further.
  - T089 still says that `AddHtApplication` "constructs the service". With the public constructors of T028, T087
    and T116 the registration no longer constructs anything; the sentence is a leftover of the internal design.

## Summary

0 MAJOR, 4 MINOR.

The revision resolves the pass-2 MAJOR note and all four pass-2 MINOR notes, and no earlier note has regressed.
The public port, the public constructors, the internal context, factory, store and migration, and the plain-SQL
test access are applied consistently across T003–T006, T025–T052, T087, T111, T114 and T116; with them the AC
reference export has no `InternalsVisibleTo` to refuse and no non-public member to report. Task numbering (127
tasks, no gaps), the cross-references between tasks, the story dependencies, the parallel groups and the
63-scenario count check out. The remaining notes are risks and imprecisions, not blockers: the new architecture
test is narrower than the export's text check it is said to mirror, the `HT-UI` guard verifies the configuration
instead of the host's database, the `StartupTasks` harness lost its way to drop a database, and the owner
decision is recorded in tasks.md only while the plan artifacts still describe the internal port. The tasks are
ready for `/speckit-analyze` and `/speckit-implement`.
