# Critic pass 2: plan for 004-goals

**Verdict**: REVISE
**Date**: 2026-11-18
**Reviewed**: specs/004-goals/plan.md, specs/004-goals/research.md, specs/004-goals/data-model.md,
specs/004-goals/quickstart.md, specs/004-goals/contracts/application-api.md, specs/004-goals/contracts/host.md,
docs/adr/0017-embedded-time-zone-data.md

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 (time-zone change misses a goal created meanwhile) | Partly | research.md R-08 now serializes goal creation and time-zone changes through `AspNetUsers.GoalsRevision`. Both increment it under the account row's update lock, and the time-zone write is conditioned on it. The per-goal `rowversion` is gone ("There is no per-goal concurrency token"). Version 1 now takes the expected zone that Application checked the start date against, so the start-date half is closed. Two gaps remain. A third writer of the account row, Identity's `UserManager.UpdateAsync`, is not conditioned on the revision (MAJOR-1 below). The "one consistent read" that the optimistic check depends on is not specified (MINOR-2 below). |
| MINOR-1 (internal `NamedTimeZone` conversions vs. no `InternalsVisibleTo`) | Resolved | research.md R-02 "Public conversions" and data-model.md `NamedTimeZone` make `DateAt`, `TimeAt` and `StartOfDay` public with BCL types. R-10 and R-11 use them. |
| MINOR-2 (existing tests that break) | Partly | plan.md Work order steps 3 and 5 and R-14 now name `PageServices.cs`, `RunnerBehaviourTests.GoodSite()`, `CoverageTests` and the slice 3 coverage table. Many more existing callers break and are still unlisted, and R-07 now claims that no caller builds the services by hand (MINOR-1 below). |
| MINOR-3 (default zone for the administrator and demo accounts) | Resolved | R-07 "Constructors change": `OperatorService` and `DemoDataService` gain `InstallationTimeZone`. R-04 and data-model.md `IAccountStore.CreateAsync` replace the old overload with one that takes `NewAccountProfile`. One leftover is a stale line in plan.md (MINOR-4 below). |
| MINOR-4 (demo start dates computed in Persistence) | Resolved | data-model.md `DemoGoal.StartDate` (a `DateOnly`). R-10: "'The day of the run' is `today` … computed once in Application … The store computes no date." |
| MINOR-5 (demo store reads custom habits) | Resolved | R-10 "The port": the store uses the identifier it has just generated for each list position, and the catalog lookup is the public read that HP-018 allows. This matches `DemoDataStore.CreateDemoAccountAsync`, which generates `Guid.NewGuid()` per custom habit (`src/HT.Persistence/Habits/DemoDataStore.cs`). |
| MINOR-6 (Register script and enhanced navigation) | Resolved | R-12 and host.md "Register": `App.razor` loads `time-zone.js` once, and the script uses a document-level capture-phase `submit` listener. `RegisterForm.razor` is a plain `EditForm method="post"` with no enhanced form handling, so the listener fills the input before the browser posts. |
| MINOR-7 (R-01 fallback vs. HP-015 and R-13) | Resolved | R-01 Alternatives, plan.md Gates item 2 and ADR 0017 Options 2 now name the costs: a hand-maintained list, machine-dependent FR-024 checks, and R-13's rules admitting `TimeZoneInfo` inside `NamedTimeZone`. |
| MINOR-8 (MobileNav "New goal") | Resolved | host.md Navigation: "then 'New goal' after the divider, before Profile … hidden on phones (`ht-hide-sm`)". This matches `design/components/MobileNav.md`. |
| MINOR-9 (ChoiceCard not planned) | Resolved | plan.md Constitution IV and Project Structure `Shared/` list ChoiceCard as new. A related inconsistency about KeyValue is in MINOR-4 below. |
| MINOR-10 (ADR 0016 Consequences) | Resolved | plan.md Work order step 7 and Gates item 1 amend the migration sentence (`docs/adr/0016-time-zones.md` line 71) in the same change as the acceptance. |
| MINOR-11 (undefined types, HP-019 test database) | Partly | `GoalVersionWrite`, `NewAccountProfile`, `CreateGoalOutcome`, `GoalCreateOutcome` and `GoalTimeline.TryCalendar` are now defined (data-model.md). The HP-019 test moved to `HT-IT` with a SQL-inserted row. That rationale contradicts the data model's own `GoalsRevision` column and leaves a manual step (MINOR-3 below). |

No note older than pass 1 exists. None of the resolved points has regressed.

## Notes

### MAJOR-1: Identity's user update rewrites the new account columns, including `GoalsRevision`, with stale values
- **Where**: research.md R-08 "The account's goal revision" ("the one fact that serializes goal creation against
  time-zone changes"), "Creating a goal" step 4, and "Changing the time zone" step 3. R-04 (the first goal writes
  the resolved default into `TimeZone`). data-model.md `AspNetUsers` (`TimeZone`, `GoalsRevision`, `Name` on the
  Identity user row). plan.md Project Structure `Identity/` ("HtUser (+TimeZone, Name, CreatedAt)").
- **Against**: `src/HT.Persistence/HtDbContext.cs` lines 37–39. Slice 1 sets
  `SetAfterSaveBehavior(PropertySaveBehavior.Ignore)` on `FailedSignInCount` and `SignInBlockedSince`, with the
  comment "UserManager.UpdateAsync never overwrites it (research R-05)". `src/HT.Persistence/Identity/AccountStore.cs`
  line 120 calls `_users.ChangePasswordAsync`, which saves the whole `HtUser` through Identity's `UpdateAsync`.
  `Context.Update` marks every property modified, and the only check is `ConcurrencyStamp`. Also spec FR-011 and
  FR-026 (each goal keeps the zone in effect, and the account's zone changes at once), and R-04's own rationale
  ("a later change of the operator's default can no longer move that account's 'today' away").
- **Continues**: pass 1 MAJOR-1.
- **Reasoning**: the artifacts add `TimeZone`, `Name` and `GoalsRevision` to the Identity user row. They write
  these columns with conditional statements that do not renew `ConcurrencyStamp`, and they say nothing about
  keeping Identity's full-row update away from them. Take a password change that loads the user, then a goal
  creation or a time-zone change that commits, then the password change saving. The save writes back the old
  `TimeZone`, the old `Name` and the old `GoalsRevision`.
  - The account's zone silently reverts while its goals keep versions in the new zone.
  - A pre-slice account's resolved default reverts to `NULL`, which R-04 says must not happen once it has goals.
  - The revision goes backwards (r+1 back to r). A time-zone change that read r before the creation then passes
    its "`GoalsRevision` still equals" check and misses the new goal. That goal keeps the outdated zone for
    good, the exact outcome pass 1 MAJOR-1 described.

  The claim that the revision serializes every relevant write therefore does not hold. Slice 1 already handled
  this exact problem for its own conditionally written columns. R-14's race list has no case that involves a
  password change.

### MINOR-1: Constructor and port changes break many existing callers, and R-07 says no caller builds the services by hand
- **Where**: research.md R-07 "Constructors change" ("All of them are resolved from DI by every caller (ADR 0003,
  ADR 0006), so no caller builds them by hand. The quickstart's AC build check (step 2) proves that the locked
  F001/F002 step definitions still compile"). plan.md Risks ("the changed constructors and ports are built only by
  DI and `HT.Persistence`"). R-04 and data-model.md (`IAccountStore.CreateAsync` and
  `IDemoDataStore.CreateDemoAccountAsync` "replace" their old signatures). plan.md Work order steps 3 to 5 and
  R-14's "existing tests that change".
- **Against**:
  - `docs/adr/0010-public-account-store-port.md` Consequences: "Unit tests build the services with a hand-written
    fake store through the public constructors".
  - The readable AC run records. `specs/001-accounts-and-access/ac/runs/run-002/writer-report.md` lines 117–120:
    "`AccountService`, `OperatorService` and `DemoDataService` are built through their public constructors (ADR
    0010)". `specs/002-habit-catalog/ac/runs/run-001/writer-report.md` line 99: slice 2's `DemoDataService`
    constructor change broke `Support/F001/AccountsDriver.cs` (CS7036), and a cross-feature change fixed it.
  - Hand-built services in the repository:
    - `AccountService`: `tests/HT.UnitTests/Application/Accounts/{AccountDetails,ChangePassword,Register,SignIn}Tests.cs`,
      `Operator/AdministratorCreationTests.cs:18` and `DemoLoadingTests.cs:28`,
      `tests/HT.UiTests/Components/Account/{ChangePassword,Register,SignIn}FormTests.cs`, and
      `tests/HT.IntegrationTests/Accounts/AccountStoreTests.cs:125`;
    - `OperatorService`: `AdministratorCreationTests.cs:17`;
    - `CatalogService`: `tests/HT.UnitTests/Application/Habits/CatalogHabitStateTests.cs:16,42`.
  - Callers or implementers of the replaced port members:
    - `IAccountStore.CreateAsync`: `tests/HT.UiTests/Host/HostClient.cs:91`,
      `tests/HT.IntegrationTests/Habits/HabitAccounts.cs:18`, `Accounts/ClaimsPrincipalFactoryTests.cs:23`,
      `Accounts/PasswordChangeTests.cs:72`, `Accounts/AccountDetailsTests.cs:41`,
      `Accounts/AccountStoreTests.cs:34,52,217`, the `PasswordChangingStore` wrapper (`AccountStoreTests.cs:179–190`),
      and `tests/HT.UnitTests/Fakes/FakeAccountStore.cs:157`;
    - `IDemoDataStore.CreateDemoAccountAsync`: `tests/HT.IntegrationTests/Operator/SeedCatalogLoadingTests.cs:241`.
      The spec ("Slice 1 and slice 2 requirements and tests this slice changes") says this file "keeps passing".
      Also `tests/HT.UnitTests/Fakes/FakeDemoDataStore.cs:78`.
  - `tests/HT.UiTests/HT.UiTests.csproj` lines 23–26 link each unit-test fake by `<Compile Include>`. The
    `FakeGoalStore` that `PageServices.cs` is to register needs the same link.
- **Continues**: pass 1 MINOR-2.
- **Reasoning**: the premise in R-07 and the Risks entry is false for the repository's own tests, and the run
  records contradict it for the locked F001 support code. Adding `InstallationTimeZone` and `TimeProvider` to
  these constructors stops F001's driver from compiling. The quickstart's AC build check then fails rather than
  "proves" compatibility, until an AC run makes a cross-feature change that no artifact anticipates. Within the
  solution, about 20 call sites outside the plan's list fail to compile at Work order steps 3 and 4. The full
  suite is then red at the step the plan says is "verified and committed together", and `/speckit-tasks` would
  get no task for these changes.

### MINOR-2: The "one consistent read" of the time-zone change is not achievable as written on this database
- **Where**: research.md R-08 "Changing the time zone" step 1 ("in one consistent read: the account's stored zone,
  its `GoalsRevision` and all its goals"). data-model.md `IGoalStore.ReadOwnerGoalsAsync` ("One read in one
  snapshot").
- **Against**: `src/HT.Persistence`. No `READ_COMMITTED_SNAPSHOT`, `ALLOW_SNAPSHOT_ISOLATION` or snapshot
  isolation level exists anywhere. The only explicit isolation level is `Serializable` in `DemoDataStore.cs:98`, so
  reads run under SQL Server's default locking read committed. Also R-08's own closing claim ("A goal created
  between steps 1 and 3 has incremented the revision, so the change re-reads and sees it").
- **Continues**: pass 1 MAJOR-1.
- **Reasoning**: the optimistic check is only safe if the revision is read no later than the goals. Under read
  committed, a read that loads the goals first and the account row second can see the goals without a
  concurrently created goal and the revision after it. The write's revision check then passes, and that goal
  keeps the old zone for good. A single statement, an ordering rule, or an isolation level would close this,
  but the artifacts name none of them, and the database has no snapshot to read from. The race test in R-14
  ("a goal created against a time-zone change") forces only one interleaving, so it would not show which
  order the implementation chose.

### MINOR-3: The account-column description and the HP-019 test contradict the `GoalsRevision` column
- **Where**:
  - plan.md Summary ("three account columns") and Technical Context Storage ("the columns
    `AspNetUsers.TimeZone`, `Name` and `CreatedAt`");
  - research.md R-04 ("`AspNetUsers` gains three columns");
  - research.md R-14 HP-019 test ("adds the three account columns as nullable and writes no data … a review of
    the generated migration confirms it has no `UPDATE` and no default on the new columns");
  - plan.md Constitution V ("HP-011 and HP-019 get executed checks");
  - data-model.md `AspNetUsers` (`GoalsRevision int NOT NULL DEFAULT 0`) and its Tables preamble ("no default
    values except `IDENTITY`").
- **Against**: spec HP-019 ("an integration test checks it on an account created before the migration").
  Constitution V ("Verified, Not Asserted"). The data model's own fourth column.
- **Continues**: pass 1 MINOR-11.
- **Reasoning**:
  - The migration adds four columns, one of them `NOT NULL` with a default. Adding it fills every existing row,
    so "writes no data" and "no default on the new columns" are false for the design as specified. The planned
    review would fail against it.
  - The rationale for skipping a real pre-migration account rests on the migration touching no existing row.
    That premise no longer holds, even if the effect (0) is harmless.
  - The "review" is a manual step, not an executed check, so the Constitution V row overstates what HP-019
    gets.
  - The test as planned creates its account after the migration, which is not what HP-019 literally asks for,
    and the deviation is not listed under "Spec discrepancies".

### MINOR-4: Smaller inconsistencies across the artifacts
- **Where and against**:
  - plan.md Project Structure, `Operator/` line: "OperatorService (+TimeProvider)". research.md R-07 gives
    `OperatorService` and `DemoDataService` both `TimeProvider` and `InstallationTimeZone`. The `Identity/` line
    lists "HtUser (+TimeZone, Name, CreatedAt)" without `GoalsRevision`, which data-model.md puts on `AspNetUsers`.
  - plan.md Constitution IV: "GoalListItem and ChoiceCard are new; the others are reused", and the others include
    KeyValue. `src/HT.Ui/Components/Shared/` has no KeyValue component; only the `.ht-kv` CSS exists
    (`design/css/ht-components.css:230`). Project Structure says "KeyValue (if not yet a component)".
  - research.md R-04: "The account store reads it as the installation's default (R-03)". data-model.md says the
    store returns `Account.StoredTimeZone` as null, and R-04 itself says "the persistence layer never has to know
    the default". Only `IGoalStore.CreateAsync` receives `defaultZone`.
  - data-model.md names the store's outcome `CreateGoalOutcome` and the use case's `GoalCreateOutcome`. The two
    names differ only by word order. contracts/application-api.md still says "the 12 target outcomes (as
    `GoalProblem`)", while data-model.md says they are `GoalCreateOutcome` members "with `GoalProblem`'s names".
- **Continues**: new.
- **Reasoning**: `/speckit-tasks` takes constructor lists, component inventories and type names from these
  lines. Each pair gives two answers, for example whether `OperatorService` needs the default zone (FR-025),
  whether KeyValue is a task, and whether `Account` resolution of the default happens in Persistence or
  Application.

## Summary

1 MAJOR, 4 MINOR. Of the 12 pass 1 notes, 9 are resolved and 3 partly. On the points that pass 1 raised, the
revision is solid:

- the `NamedTimeZone` conversions are public;
- the demo dates and the custom-habit identifiers stay in Application or with the store's own generated
  identifiers;
- the Register script works after enhanced navigation;
- MobileNav, ChoiceCard and the ADR 0016 amendment are planned;
- the fallback's costs are put before the owner.

The `GoalsRevision` design closes the race between goal creation and a time-zone change that pass 1 found, for
the writers it names. It overlooks the one existing writer that saves the whole account row, Identity's
`UpdateAsync` on a password change. That writer can roll back the revision and the zone, which reopens the same
lost-goal outcome. Slice 1 already guards its own conditional columns against this writer. The minor notes are:

- the unspecified isolation of the "consistent read";
- the false premise that no caller builds the services by hand, which the AC run records disprove, together with
  about 20 unlisted test call sites;
- the HP-019 test's description, which contradicts the `GoalsRevision` column;
- several one-line inconsistencies.

These should be settled before `/speckit-tasks`.
