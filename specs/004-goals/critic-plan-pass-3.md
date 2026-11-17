# Critic pass 3: plan for 004-goals

**Verdict**: APPROVE
**Date**: 2026-11-18
**Reviewed**: specs/004-goals/plan.md, specs/004-goals/research.md, specs/004-goals/data-model.md,
specs/004-goals/quickstart.md, specs/004-goals/contracts/application-api.md, specs/004-goals/contracts/host.md,
docs/adr/0017-embedded-time-zone-data.md

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 (Identity's full-row update rewrites the new account columns) | Resolved | R-04 "Identity's full-row update never touches the new columns" and data-model.md `AspNetUsers` ("All four are mapped with `SetAfterSaveBehavior(Ignore)`") cover `TimeZone`, `Name`, `CreatedAt` and `GoalsRevision`. This is the mechanism slice 1 uses in `src/HT.Persistence/HtDbContext.cs` lines 37–39. Inserts still write the values: `AccountStore.CreateAsync` and `DemoDataStore.CreateDemoAccountAsync` both insert through `UserManager.CreateAsync`, which the after-save rule does not affect. The only later writers are `ExecuteUpdate`-style statements (R-04), and R-14 adds the password-change race test. I found no other full-row writer of `HtUser` in `src/`. |
| MINOR-1 (constructor and port changes break callers; "no caller builds the services by hand") | Partly | R-07 "Constructors change, and the slice 1 and 2 constructors stay" now cites both writer reports and keeps every shipped constructor. R-07's "Port changes" list matches a grep of the solution: every caller and implementer of `IAccountStore.CreateAsync` and `IDemoDataStore.CreateDemoAccountAsync` (`HostClient`, `HabitAccounts`, the four `Accounts/` test files including `PasswordChangingStore`, `FakeAccountStore`, `SeedCatalogLoadingTests`, `FakeDemoDataStore`). The `HT.UiTests.csproj` link is planned. Some statements still say the opposite (MINOR-2). The kept constructors also bring a new risk (MINOR-1). |
| MINOR-2 (the "one consistent read" is not achievable on this database) | Resolved | R-08 "Changing the time zone" step 1 and data-model.md `ReadOwnerGoalsAsync` fix the order: account row first, then the goals. The argument holds. A goal missing from the goals read was not committed when the goals were read. Its creation then either had not yet incremented the revision when the account row was read, so the write's check fails, or it holds the row's exclusive lock, so the account read waits for its commit. The test that is meant to pin this, and the "no snapshot" premise, are in MINOR-4. |
| MINOR-3 (the account-column text and the HP-019 test contradict `GoalsRevision`) | Partly | Every artifact now names four columns (plan.md Summary, Technical Context Storage; R-04; data-model.md). The HP-019 check now expects `GoalsRevision` `0` and is an executed reflection test (R-14), and plan.md Constitution V describes it correctly. The preamble of data-model.md Tables admits the default. The literal deviation is now listed as plan.md "Spec discrepancies" 6. The rationale given for that deviation does not hold (MINOR-3). |
| MINOR-4 (smaller inconsistencies) | Resolved | plan.md `Operator/` line: "OperatorService (+TimeProvider, InstallationTimeZone; old constructors kept)". `Identity/` line: "+TimeZone, Name, CreatedAt, GoalsRevision". Constitution IV and Project Structure both make KeyValue new. R-04: "The account store returns it as null … Application resolves null". The store outcome is renamed `GoalWriteOutcome`, and contracts/application-api.md's `CreateAsync` row refers to `GoalCreateOutcome` members. |

Pass 1 notes: none has regressed. The default zone for administrator and demo accounts (pass 1 MINOR-3) still holds on every DI path. The demo start dates (pass 1 MINOR-4) are still computed in Application. The public `NamedTimeZone` conversions (pass 1 MINOR-1) are unchanged.

## Notes

### MINOR-1: The kept slice 1 and 2 constructors compile but quietly drop the supplied clock and the goal rules
- **Where**: research.md R-07 "Constructors change, and the slice 1 and 2 constructors stay":
  - `OperatorService(store)` and `DemoDataService(accounts, demoData)` "add `TimeProvider.System` and UTC";
  - `CatalogService(store)`, `CatalogAdministrationService(store)` and `CustomHabitService(store)` give "a goal
    count of 0 for unpublishing and deleting";
  - the rationale: "This is exactly the state of a database without goals, which is all such callers have".

  Also plan.md Constitution Check II ("Time enters only as `TimeProvider` in the services") and plan.md Risks.
- **Against**:
  - Constitution II: "Domain and Application code MUST NOT read the system clock … directly; time enters through
    `TimeProvider` or a parameter".
  - Spec FR-033 (unpublishing a habit that goals use needs confirmation).
  - The locked F004 criteria. `goals.feature` AC-064 (line 794: start date 2026-11-18, Meditation 2026-11-25),
    AC-065 (line 800: "When at `2026-11-19T10:00:00Z` demo loading runs again") and AC-057 (line 712:
    "unpublishes … without confirming").
  - `specs/001-accounts-and-access/ac/runs/run-002/writer-report.md` ("`AccountService`, `OperatorService` and
    `DemoDataService` are built through their public constructors").
  - `specs/002-habit-catalog/ac/runs/run-001/writer-report.md` line 99: the shared
    `Support/F001/AccountsDriver.cs` `LoadDemoDataAsync` calls `new DemoDataService(store, demoData)`, which is
    exactly the constructor R-07 keeps.
- **Continues**: new (a consequence of the fix to pass 2 MINOR-1).
- **Reasoning**:
  - The existing AC helper for demo loading builds the two-argument `DemoDataService`. After this plan, that
    constructor fixes the clock to `TimeProvider.System`. So the demo start dates and `CreatedAt` come from the
    real clock, not from the scenario's `FakeTimeProvider`.
  - An F004 step that reuses this helper therefore compiles and fails AC-064's and AC-065's dates. The failure
    gives no compile error that would point at the cause.
  - Likewise, a hand-built `CatalogAdministrationService(store)` gives `Unpublished` where FR-033 and AC-057
    expect `ConfirmationNeeded`. The rationale ("all such callers have" a database without goals) does not
    hold for the AC project. It is the one hand-building caller the plan names, and F004's scenarios do create
    goals.
  - Choosing `TimeProvider.System` inside an Application constructor is also Application picking the system
    clock. The Constitution II row does not mention this. The existing `TryAddSingleton(TimeProvider.System)` is
    composition that a caller can override; a delegating constructor's choice cannot be overridden.
  - plan.md Risks covers only compile breaks of the step definitions. It does not mention this silent
    behaviour change, which `/speckit-ac-tests-write` would need to know about.

### MINOR-2: Statements left over from before the kept constructors contradict R-07
- **Where and against**:
  - plan.md Risks, first entry: "the changed constructors and ports are built only by DI and `HT.Persistence`".
    R-07 now says the opposite (hand-built by the F001 support code and by unit tests). The test fakes and
    `PasswordChangingStore` also implement the ports.
  - research.md R-07 "Compatibility": "Only `HT.Persistence` implements the ports, and only DI builds the
    services (ADR 0003, ADR 0010)". The same section's later bullet, and the "Port changes" list of test
    implementers, contradict it.
  - plan.md Work order step 3: "The bUnit page tests break too: `CatalogService`, `CatalogAdministrationService`
    and `CustomHabitService` now take `IGoalStore`". `PageServices.cs` registers them with `AddScoped<…>()`,
    and R-07 says "DI picks the longest constructor it can satisfy". So without an `IGoalStore` registration,
    the container picks the kept one-argument constructor and nothing breaks. The same paragraph ends with "The
    service constructors of slices 1 and 2 stay, so their hand-built callers still compile". Registering
    `FakeGoalStore` is needed for goal behaviour, not for resolution. A page test that forgets it gets slice 2's
    results silently.
- **Continues**: pass 2 MINOR-1.
- **Reasoning**: `/speckit-tasks` takes risks and task reasons from these lines. Each pair gives two answers to
  whether hand-built callers exist and whether the bUnit tests fail without `FakeGoalStore`.

### MINOR-3: The rationale of "Spec discrepancies" 6 for not testing HP-019 literally does not hold
- **Where**: plan.md "Spec discrepancies" 6 ("The test cannot do this literally without a
  migrate-to-an-older-migration entry point in `HT.Persistence`, and the AC surface keeps exactly two entry
  points (ADR 0003)"). research.md R-14 (the same sentence at the end of the HP-019 checks). plan.md Gates item 3
  (the owner decides on this basis).
- **Against**:
  - Spec HP-019 ("an integration test checks it on an account created before the migration").
  - `docs/adr/0003-acceptance-tests.md` lines 25 and 39. These limit what the *acceptance tests* use, not what
    `HT.Persistence` exposes. `docs/adr/0010-public-account-store-port.md` line 33 already keeps a third public
    member, `HtDatabase.MigrateAsync`.
  - R-14's own technique. It reaches the internal `Goals` migration by reflection without `InternalsVisibleTo`.
    The internal `HtDbContext` is registered by `AddHtPersistence` (`PersistenceServiceCollectionExtensions.cs`)
    and derives from the public `DbContext`. So an integration test can resolve it by type and migrate to a
    target migration through EF's public `IMigrator`, with no new entry point.
  - The derived-database precedent, `tests/HT.IntegrationTests/SeedDatabaseFixture.cs` (`HT-IT-SEED`).
- **Continues**: pass 2 MINOR-3.
- **Reasoning**: a literal pre-migration test is feasible on a derived database: migrate to `HabitCatalog`,
  create the account, migrate to the latest. It needs neither a new entry point nor a change to the AC surface.
  The plan presents the deviation as forced, and the owner is asked to accept it on that premise (Gates item 3).
  The two planned checks may still be adequate, but the stated reason for departing from HP-019's wording is
  factually wrong.

### MINOR-4: The read-order race test has no seam, and "there is no snapshot" is not true of the hosted database
- **Where**: research.md R-08 "Changing the time zone" step 1:
  - "both under the database's default read committed … There is no snapshot";
  - "an integration test pins it. The test holds a goal creation open between the two reads and expects the
    change to retry".

  Also data-model.md `ReadOwnerGoalsAsync`.
- **Against**:
  - `docs/adr/0015-demo-hosting.md` lines 30 and 64: the host is Azure SQL Database, where
    `READ_COMMITTED_SNAPSHOT` is on by default. Local and CI SQL Server databases have it off.
  - `src/HT.Persistence/PersistenceServiceCollectionExtensions.cs`: `AddDbContext` options are fixed inside the
    entry point. There is no interceptor or other hook, and no `Interceptor` exists anywhere in `src/` or
    `tests/`. The planned `GoalStore` is internal (plan.md Structure Decision).
- **Continues**: new (follows from the fix to pass 2 MINOR-2).
- **Reasoning**:
  - The ordering argument holds under both isolation modes, because the write re-checks under the update
    lock. But the artifacts state one mode as fact, and it is the wrong one for the hosted demo. The integration
    tests only ever run the locking mode.
  - The pinned interleaving needs the creation to increment the revision and commit after the store's account
    read and before its goals read. Both reads happen inside one internal store call, and the plan gives no
    point at which a test could act between them.
  - Under locking read committed, a creation that is merely "held open" makes the change's goals read (or its
    account read, once the revision is updated) wait for that creation's commit. So the test as described would
    either block or exercise a different interleaving than the one it claims to pin.
  - `/speckit-tasks` would get a test task that cannot be written as specified without an extension point
    that no artifact plans.

### MINOR-5: Smaller gaps and inconsistencies
- **Where and against**:
  - Goal creation retries: research.md R-08 "Creating a goal" step 2 ("Application then re-reads the account and
    checks the start date again, at most 10 times") gives no result when the tenth attempt still gets
    `TimeZoneChanged`. `GoalCreateOutcome` (data-model.md) and the `CreateAsync` row in
    contracts/application-api.md have no outcome for it. For the time-zone change, step 4 states the end ("throws
    a fault exception that names no value"); for creation, nothing is stated.
  - Number of owner answers due: plan.md Constitution Check preamble ("Three answers are due"), Decision
    Discipline row ("Two technical positions need the owner … the time-zone library … and the HP-019 reading of
    'Spec discrepancies' 1"), research.md intro ("Two choices need the owner's answer"), and Gates items 1–3,
    which cover ADR 0016, ADR 0017 and discrepancies 1 to 6. The Decision Discipline row leaves out
    discrepancies 2 to 6, including the new 6.
- **Continues**: new.
- **Reasoning**: the first gap leaves an implementer to choose between an exception and an unlisted outcome on a
  use case whose outcome list is a contract. The second gives the owner differing lists of what must be answered
  before `/speckit-tasks`.

## Summary

0 MAJOR, 5 MINOR. Of the 5 pass 2 notes, 3 are resolved and 2 partly; no older note has regressed.

The pass 2 major finding is closed. Identity's full-row update can no longer roll back the zone, the name or
`GoalsRevision`, through the same after-save rule slice 1 uses. The fixed account-then-goals read order makes the
optimistic time-zone change sound, and the port-change call-site list matches the repository.

What remains is cleanup and risk disclosure:

- the kept constructors silently use the system clock and skip the goal rules, which can mislead the F004 AC
  writer who reuses the existing demo-loading helper;
- several stale sentences still say only DI builds the services;
- the HP-019 deviation rests on a premise that the plan's own reflection technique disproves;
- the read-order race test has no seam, and the hosted database is described inaccurately;
- creation has no stated end to its retries.

None of these blocks `/speckit-tasks` once the owner has answered the gates. They should be settled so that the
tasks do not inherit contradictory reasons.
