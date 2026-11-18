# Research: Goals

**Feature**: specs/004-goals · **Date**: 2026-11-18 · **Plan**: [plan.md](plan.md)

The spec's `## Clarifications` and ADR 0016 settle the product questions of this slice. The technical choices of
slices 1 to 3 stay in force: their `research.md` files, as amended by ADRs 0010, 0013 and 0014. This file
records only the choices this slice adds, each with its decision, rationale and alternatives. Nothing here
reopens a recorded decision. The owner answered the plan's three gates on 2026-11-18 (plan.md, "Gates
before the next step"):

- ADR 0016 is Accepted;
- R-01, the time-zone library, is Accepted as ADR 0017;
- the five "Spec discrepancies" positions are accepted, including R-04, how accounts that exist before this
  slice get the default time zone.

## R-01 Time-zone data and date arithmetic: NodaTime inside HT.Domain

Accepted by the owner on 2026-11-18: [ADR 0017](../../docs/adr/0017-embedded-time-zone-data.md).

- **Decision**:
  - `HT.Domain` references **NodaTime 3.2.x**, the latest 3.2 patch when the Domain step starts. The version is
    pinned in `HT.Domain.csproj` and recorded in ADR 0017.
  - All time-zone conversions and all date arithmetic go through NodaTime's embedded time-zone database (tzdb):
    `DateTimeZoneProviders.Tzdb` and `LocalDate`, `DateTimeZone.AtStartOfDay` and `Instant`. That covers named
    time zones, midnights and weekdays.
  - NodaTime stays **internal** to `HT.Domain`. Every public member of Domain and Application uses BCL types
    only:
    - `DateOnly` for dates;
    - `DateTimeOffset` in UTC for instants;
    - `string` for a time-zone identifier inside `NamedTimeZone`.

    A reflection rule enforces this (R-13). So the acceptance-test surface (ADR 0003) carries no NodaTime type,
    the test writer works in the spec's own terms (dates, instants, identifiers), and the AC project needs no
    new package.
- **Rationale**:
  - Constitution II wants evaluation to be a deterministic function of stored facts. With
    `TimeZoneInfo.FindSystemTimeZoneById`, the time-zone rules come from the machine:
    - Windows maps IANA identifiers through its ICU copy;
    - the Linux CI runner and the hosted App Service use their own tzdata package.

    A rule change in one of those (for example a country dropping summer time) would move one goal's
    midnights on one machine and not on another. NodaTime ships one tzdb version inside the package, so every
    machine computes the same instants, and a version bump is a visible package change, reviewed like code.
  - FR-024 needs three checks: "zone or link", "contains `/`" and "not `Etc/`". HP-015 needs the list of
    **zones, not links**. Both are data NodaTime exposes:
    - `TzdbDateTimeZoneSource.Default.ZoneLocations` and `CanonicalIdMap` tell a link from its zone;
    - `GetIds()` gives every identifier, zones and links.

    The BCL has no public way to tell a link from a zone. On Windows, `TimeZoneInfo.GetSystemTimeZones()`
    returns Windows names.
  - `DateTimeZone.AtStartOfDay(LocalDate)` returns the first instant of a day, also where the clocks jump at
    midnight and midnight does not exist (America/Havana, America/Santiago). With `TimeZoneInfo` the code would
    have to detect an invalid local time and resolve it by hand. Every FR-013 boundary and every SC-003 test
    depends on this.
  - `LocalDate` arithmetic (`PlusDays`, `DayOfWeek`, "the Monday after") has no hidden time of day. FR-012 is
    exactly that model.
- **Alternatives**:
  - **BCL `TimeZoneInfo` with IANA identifiers** (.NET 6+ on ICU). No new package, but three costs:
    - the time-zone data differs between the developer machine, CI and the host;
    - there is no "zone or link" information;
    - nonexistent midnights need hand-written handling.

    Rejected, because it breaks constitution II's reproducibility across machines. It is the fallback if the
    owner declines ADR 0017, at a price the owner should weigh (critic plan pass 1, MINOR-7):
    - The BCL cannot list IANA identifiers on Windows, and cannot tell a link from a zone anywhere. So HP-015's
      list ("zones, not links") would be a hand-maintained identifier list in source, reviewed with each change.
      The FR-024 check would accept what `TimeZoneInfo.TryFindSystemTimeZoneById` finds on the running machine
      and is not a Windows name, so it would differ between machines.
    - R-13's rules would allow `TimeZoneInfo.FindSystemTimeZoneById` and `ConvertTime*` inside `NamedTimeZone`
      only.
    - `NamedTimeZone`'s public shape stays the same.
  - **NodaTime types on the public surface** (`LocalDate` in results). This is more precise, but the AC
    project and every page would need NodaTime, and the spec speaks in dates and instants. Rejected.
- **Consequence**: the DomainDependencyTests rule "Domain has no Microsoft types" stays. A new rule admits
  NodaTime as Domain's only external assembly (R-13).

## R-02 Named time zones (FR-024, FR-025, HP-015)

- **Decision**: `HT.Domain.Time.NamedTimeZone` is a readonly record struct holding the identifier as given.
  - `TryParse(string? text, out NamedTimeZone zone)` accepts:
    - exactly `UTC`;
    - or an identifier that is in `Tzdb.GetIds()`, contains `/`, and does not start with `Etc/`.

    Identifiers compare ordinally, with their case, and are not trimmed: `" Europe/Kyiv"` is not a named time
    zone. Spec FR-024 "Identifiers compare exactly".
  - A link is kept as given (`Europe/Kiev`). Its rules come from NodaTime, which resolves the link to its zone,
    but the stored and returned identifier is the link.
  - `NamedTimeZone.Utc` is `UTC`.
  - `NamedTimeZones.Choices` is the Profile list of HP-015:
    - `UTC` first;
    - then every canonical zone identifier (`CanonicalIdMap` value equal to its key) that is a named time zone,
      in ordinal order.

    The page adds the account's current value when it is not among them.
- **Rationale**: the rule is FR-024's, word for word. The list comes from the same data as the check, so every
  entry is accepted.
- **Alternatives**:
  - Trimming the identifier. Rejected: the browser sends an exact identifier, and FR-024 compares exactly.
  - Storing the canonical zone instead of the link. Rejected: FR-024 says "a link is kept as given".
- **Public conversions** (critic plan pass 1, MINOR-1). Application needs "today", the local hour and the month
  of creation, and `InternalsVisibleTo` is forbidden (`AcSurfaceTests`). So `NamedTimeZone` exposes three public
  members, with BCL types only:
  - `DateAt(DateTimeOffset instant) → DateOnly`, which gives "today";
  - `TimeAt(DateTimeOffset instant) → TimeOnly`, which gives the greeting hour;
  - `StartOfDay(DateOnly date) → DateTimeOffset` (UTC).

  NodaTime stays behind them.

## R-03 The installation's default time zone (FR-025)

- **Decision**:
  - `HT.Application.Accounts.InstallationTimeZone` is a sealed class with `NamedTimeZone Value` and the
    factory `From(string? configured)`. The factory gives UTC when the value is absent or not a named time
    zone.
  - `AddHtApplication` registers `InstallationTimeZone.From(null)`, which is UTC, with `TryAddSingleton`.
  - The host registers `InstallationTimeZone.From(configuration["Operator:DefaultTimeZone"])` before calling
    `AddHtApplication`. When the configured value was present but not a named time zone, it logs
    `Default time zone: not a named time zone, using UTC`. The value itself is not logged: it is an operator
    setting, not a secret, but a typo can be a secret pasted into the wrong key.
  - The hosted demo sets it as the App Service setting `Operator__DefaultTimeZone` (ADR 0015). Leaving it out
    gives UTC.
  - `tools/hosting/provision.ps1` is not changed in this slice, so the demo default stays UTC unless the
    operator sets the value.
- **Rationale**:
  - ADR 0005 keeps configuration out of the use-case signatures.
  - `TryAdd` lets the acceptance tests and integration tests register nothing and get UTC. That is what FR-025
    says acceptance tests expect.
  - Follows slice 1's `TimeProvider` pattern.
- **Alternatives**:
  - `IOptions<T>` binding. Rejected: Application would need `Microsoft.Extensions.Options`, and nothing else
    in the app uses options.
  - Reading `IConfiguration` in Application. Rejected: ApplicationDependencyTests forbids it.

## R-04 Accounts: time zone, name and creation time (FR-024 to FR-030, HP-019)

Accepted by the owner on 2026-11-18 (plan.md, "Spec discrepancies" 1).

- **Decision**: `AspNetUsers` gains four columns in the slice's migration: three below, and `GoalsRevision`
  (R-08).
  - `TimeZone nvarchar(64) NULL`. Every account created from this slice on, and every account whose time zone
    is changed, stores a concrete named time zone. `NULL` exists only on accounts created before the
    migration. The account store returns it as null, in `Account.StoredTimeZone`. Application resolves null to
    `InstallationTimeZone` (R-03) wherever it needs the zone, so such an account "has the installation's default
    time zone" (FR-025, HP-019).

    The first goal of such an account writes the resolved default into `TimeZone`, in the transaction that
    creates the goal (R-08). From then on, a later change of the operator's default can no longer move that
    account's "today" away from the time zone its goals keep.
  - `Name nvarchar(60) NULL`: trimmed, and `NULL` when empty (FR-028).
  - `CreatedAt datetimeoffset NULL`: the supplied current time of registration, administrator creation or
    demo loading. `NULL` for accounts created before the migration, which therefore give no "member since"
    (FR-030, HP-019).

  `HT.Domain.Accounts.Account` gains three `init` properties: `string? StoredTimeZone`, `string? Name` and
  `DateTimeOffset? CreatedAt`. Its positional constructor is unchanged (R-07).

  `IAccountStore.CreateAsync` gets a fourth parameter, `NewAccountProfile` (name, zone, creation time), in place
  of its slice 1 signature. Every creator passes a concrete zone and its supplied time:

  - registration;
  - administrator creation;
  - demo loading, through `IDemoDataStore`.

  So no path writes an account without them (FR-030). A port may change (R-07); the persistence layer never has
  to know the default.
- **Identity's full-row update never touches the new columns** (critic plan pass 2, MAJOR-1).
  `UserManager.UpdateAsync`, which a password change calls through `ChangePasswordAsync`, saves every property of
  `HtUser`. `HtDbContext` therefore sets `SetAfterSaveBehavior(PropertySaveBehavior.Ignore)` on `TimeZone`,
  `Name`, `CreatedAt` and `GoalsRevision`, exactly as slice 1 does for `FailedSignInCount` and
  `SignInBlockedSince` (slice 1 research R-05).
  - They are written at insert, by `UserManager.CreateAsync`, which is not affected by the after-save rule.
  - After that, they are written only by the stores' own conditional statements:
    - `IAccountStore.TrySetNameAsync`, as one `ExecuteUpdate` scoped by account;
    - `IGoalStore.CreateAsync` and `TryChangeTimeZoneAsync` (R-08).

    A stale user object saved by Identity can then never roll back the zone, the name or the revision.

  An integration test in R-14 runs a password change that loaded the user before a time-zone change committed,
  and checks that the zone and the revision survive.
- **Rationale**:
  - A migration cannot read the operator's configuration, so it cannot write "the installation's default" into
    existing rows.
  - Writing `UTC` would break HP-019 on any installation whose default is not UTC.
  - A null that means "the default", made concrete when it first matters, gives each pre-slice account the
    default. Each goal still keeps its own concrete time zone in its versions (FR-011).
- **Alternatives**:
  - Migrate with `UTC NOT NULL`. Simpler, and right wherever the default is UTC, which includes the hosted
    demo, whose database is rebuilt at every deployment. But it is wrong for HP-019 in general. This is the
    position to fall back to if the owner prefers it; the spec would then say "UTC" in HP-019.
  - A host start task that fills every `NULL` with the default. That is one more operator step, and it runs
    before the default is known to be correct.

## R-05 Goal settings, target checks and target text in Domain

- **Decision**: the new namespace `HT.Domain.Goals`.
  - **`GoalTarget`**: Build or Reduce, the measurement, amount, count unit, duration unit and `Schedule`.
    - It reuses slice 2's `Measurement`, `DurationUnit`, `Schedule`, `ScheduleKind` and `SchedulePeriod`, and
      `BuildOrReduce`.
    - Its fields come from `GoalTargetFields`, the same nullable shape as `SuggestedStartingGoalFields`.
    - The check runs in FR-006's order through one internal `TargetFieldCheck`, which is shared with
      `SuggestedStartingGoal.TryCreate`. `SuggestedStartingGoal` keeps its public members and its
      `SuggestedStartingGoalProblem` values exactly. Slice 2's unit tests pin it, and so do the locked F002
      criteria.
    - After the shared checks, the goal adds `ReduceAmountNeedsOneTime`: Reduce, count or duration, N times
      other than 1.
  - **`GoalSettings`**: a `GoalTarget` plus the version's `NamedTimeZone`.
  - **`GoalSettingsVersion`**: `GoalSettings` plus `DateOnly AppliesFrom`.
  - **`GoalDraft.TryCreate(...)`** checks a whole new goal in FR-006's order and returns one `GoalProblem`. The
    inputs are:
    - the habit reference, which is present or not (the lookup itself is Application's);
    - the target fields;
    - the start date against "today";
    - the name.

    The `GoalProblem` values are:
    - `HabitMissing`;
    - the 12 target problems, in slice 2's order;
    - `ReduceAmountNeedsOneTime`;
    - `StartDateMissing`, `StartDateInPast` and `StartDateTooFar` (more than 365 days after today);
    - `NameTooLong`.

    Each problem maps to a `GoalField`, which says which field the refusal is about (FR-006: "A refusal carries
    its message and the field it is about"). The habit lookup runs between `HabitMissing` and the target
    checks, so Application calls `GoalDraft` in two steps (R-07).
  - **`GoalName`**: the trimmed own name, at most 60 UTF-16 code units, like `HabitName`. It is not unique, so
    it has no key column. The sort key is `HabitName.KeyOf`.
  - **`GoalText`** is the single formatter of a goal's target text and kind (FR-010, slice 2 FR-017 with the
    goal exception).
    - It shares the internal amount, schedule and day-list parts with `SuggestedStartingGoalText`.
    - It adds one branch: Reduce, yes/no, fixed days gives "avoided on " followed by the days as the kind
      writes them, or "avoided every day".
    - `SuggestedStartingGoalText.Format` keeps its output exactly.
    - The derived name is `habit name + ", " + target text`, produced when read.
- **Rationale**:
  - One checker and one formatter, so the New goal page, the stored goal and the derived name never disagree.
    That is constitution IV's "one shared formatter", and spec FR-007's "never shows two opposite texts".
  - Keeping slice 2's public types unchanged keeps F002 green (FR-019).
- **Alternatives**:
  - A goal that holds a `SuggestedStartingGoal`. Rejected: different yes/no message, different checks (the
    Reduce N check), and a different text exception. A suggestion is an example, not a goal (ADR 0011).
  - Copying the checks. Rejected: two copies of the same order would drift.

## R-06 The goal timeline: days, weeks, current date, calendar and next commitment (FR-012 to FR-014, FR-026, FR-027)

- **Decision**: `HT.Domain.Goals.GoalTimeline` is a pure function of a goal's start date and its ordered
  versions. It has no clock and no culture; the instant is a parameter.
  - **Version of a date** `d` (with `d` ≥ the start date): the version with the greatest `AppliesFrom` ≤ `d`.
  - **Day `d`** starts at `zone(d).AtStartOfDay(d)` and ends at `zone(d).AtStartOfDay(d + 1)`, where `zone(d)`
    is the time zone of `d`'s own version (FR-012, FR-027).
    - On a clock-change day this lasts 23 or 25 hours.
    - At a version switch, the old day's end and the new day's start may differ. That is the gap or overlap;
      nothing is merged, split or dropped.
  - **Week** with Monday `m`: from the start of `m` to `zone(m).AtStartOfDay(m + 7)`.
    - Per-week versions start only on Mondays (FR-026), so the whole week has one version.
    - The first week is the first Monday on or after the start date (FR-013).
  - **Current date at instant `t`**: the greatest `d` ≥ the start date whose day has begun at `t`.
    - Candidates: the local date of `t` in each version's time zone, plus or minus 2 days. Offsets differ by at
      most 26 hours, so the answer is always among them.
    - Filter the candidates by "start of `d` ≤ `t`" and take the maximum. When no candidate passes, there is no
      current date.
    - This holds where the days' starts are not monotonic, after a move from Pago_Pago to Kiritimati (FR-027).
  - **Evaluation calendar** for a first and last date (1 to 366 days, otherwise
    `CalendarRangeOutOfBounds`): commitments, days or weeks, in date order, each with its date (a week with
    its first and last date) and its start and end instants (FR-013). There is no status.
  - **Next commitment at `t`**, for a commitment-based goal: the first scheduled day `d`, on or after the
    start date, whose end is after `t`.
    - The scan starts at `max(start date, current date − 2)`. Overlaps can make yesterday's end later than
      today's start.
    - It runs forward at most 15 days. One scheduled day per week exists, plus at most one version switch.
    - A goal that has not started gives its first scheduled day on or after the start date (FR-014).
  - **First week starts**, for a per-week goal: its first Monday. The list gives it while that week has not
    begun at `t`; reading gives it always (FR-014, FR-016).
  - **Time-zone change at `t` to zone `z`** (FR-026). The result is a `GoalVersionChange`:
    - `None`: `z` equals the zone of every version from the current date on.
    - `ReplaceFirst(z)`: the goal has no current date; its single version gets `z`.
    - `ReplacePending(z, e)`: a version after the current date `D` exists; it gets `z` and the date `e`.
    - `Add(e, z)`: a new version from `e`.

    Here `e` is `D + 1` (commitment-based and per-day goals) or the Monday after `D`'s week (per-week goals).
    The rules hold structurally: never more than one version after `D`, and never two versions on the same
    date.

    `None` is used only when the account's time zone itself does not change. The service returns "changed
    nothing" before computing anything, as FR-026 asks. A change back to the old zone while a version is
    pending gives that version the old zone, as FR-026 says, rather than removing it.
- **Rationale**: FR-012's definitions turned into four functions. Each is unit-testable on both sides of
  every boundary, as constitution II requires. The calendar, the list, reading and the change share the same
  "version of a date" and "start of a day".
- **Alternatives**:
  - Measuring the change from the account's date. Rejected by the spec and ADR 0016, because it moves a day
    that has begun.
  - Storing commitments as rows. Rejected: the calendar is a function of stored facts (constitution II), and
    slice 5 records against dates.

## R-07 Use cases, acting user, results, and the compatibility rule for slices 1 and 2

- **Decision**:
  - **New services**, each scoped and registered in `AddHtApplication`. Methods are listed in
    [contracts/application-api.md](contracts/application-api.md).
    - `HT.Application.Goals.GoalService`: starting values, create, list, read and evaluation calendar.
    - `HT.Application.Accounts.ProfileService`: profile, name, time zone and Home heading. It is a sub-area of
      accounts (ADR 0005: split when a service grows).
  - **Refusal order** (FR-023) is the same in every method:
    1. not signed in;
    2. the role: an administrator gets "Goals are for registered users." on every goal use case;
    3. the identifier: goal, then habit, then suggested starting goal position;
    4. the field checks or the calendar range.
  - **Result shape**: every result has `Outcome` (an enum) and `Message` (the spec's literal text, or null on
    success), as in slices 1 and 2. Goal-creation refusals also carry `Field` (`GoalField`).
  - **Compatibility (FR-019)**: no use-case method, result type, view or input that slices 1 to 3 shipped
    changes its signature, or its result for a call those slices accept. Ports (`IAccountStore`,
    `ICustomHabitStore`, `IDemoDataStore`) and service constructors may change. Their callers are updated in the
    same change: the host composes through DI, and the hand-built callers and test implementers are listed
    below. New behaviour comes in three ways:
    - **New overloads** that take the acting user, or a confirmation or extra input:
      - `CatalogService.BrowseAsync(ActingUser, CatalogFilter, bool onlyTracked, …)`;
      - `CatalogService.GetDetailsAsync(ActingUser, HabitId, …)`;
      - `CatalogAdministrationService.UnpublishAsync(ActingUser, HabitId, bool confirmed, …)`;
      - `AccountService.RegisterAsync(email, password, timeZone, name, …)`.

      The old overloads delegate to the new ones:
      - the old `UnpublishAsync` is "without confirming";
      - the old `RegisterAsync` passes no time zone and no name;
      - the old `BrowseAsync` and `GetDetailsAsync` keep their exact slice 2 result.
    - **`init` properties added to existing records and result classes**, null or 0 for slice 2 calls:
      - `CatalogHabitSummary.GoalCount`;
      - `CatalogBrowseResult.TrackedCount`;
      - `CatalogHabitDetails.GoalCount` and `Goals`;
      - `CustomHabitView.GoalCount`;
      - `DemoAccountResult.Goals`;
      - `Account.StoredTimeZone`, `Name` and `CreatedAt`.

      Positional constructors and existing members stay as they are, and a slice 2 view built by a test still
      equals what slice 2 calls return.
    - **New enum members appended after the existing ones**: `CatalogHabitUnpublishOutcome.ConfirmationNeeded`,
      `CustomHabitDeleteOutcome.UsedByGoals`, `RegistrationOutcome.NameTooLong` and two refusals of
      `CatalogBrowseOutcome`.

      One exception changes the meaning of an existing call, as the spec wants: the old `UnpublishAsync` on a
      habit that goals use now gives `ConfirmationNeeded` (FR-033). No F002 scenario unpublishes a habit that a
      goal uses (spec, "Slice 1 and slice 2 requirements and tests this slice changes").
  - **Constructors change, and every caller passes the new arguments** (critic plan pass 3, MINOR-1). The old
    constructors are **not** kept. A kept constructor would have to pick a clock (`TimeProvider.System`) or drop
    the goal rules. That is a silent behaviour change: a hand-built service would compile and then give real-clock
    demo dates (AC-064, AC-065) or skip the unpublish confirmation (AC-057). A compile error is the louder and
    safer signal.
    The new constructors are these:
    - `OperatorService` and `DemoDataService` gain `TimeProvider` (creation times, demo start dates) and
      `InstallationTimeZone`. The administrator and the demo accounts get the default zone (FR-025), and "the
      day of the run" is today in it (critic plan pass 1, MINOR-3);
    - `CatalogService`, `CatalogAdministrationService` and `CustomHabitService` gain `IGoalStore`, for goal
      counts, the unpublish count and the delete rule;
    - `AccountService` gains `InstallationTimeZone`.

  - **Hand-built callers.** The locked F001 support code builds `AccountService`, `OperatorService` and
    `DemoDataService` through their public constructors
    (`specs/001-accounts-and-access/ac/runs/run-002/writer-report.md`, ADR 0010). Slice 2's change to
    `DemoDataService` broke it once, and the AC writer fixed it as a cross-feature change
    (`specs/002-habit-catalog/ac/runs/run-001/writer-report.md`, "Cross-feature changes").
    - **In the AC project**, this slice's new arguments break it the same way, with a compile error. The F004 run
      of `/speckit-ac-tests-write` makes the same kind of cross-feature change: the supplied `TimeProvider` and
      `InstallationTimeZone`. Nobody else reads or edits that code. plan.md Risks names this, so the owner expects
      it.
    - **Inside the solution**, the hand-built callers are updated in the same commit as steps 3 and 4:
      - `tests/HT.UnitTests/Application/Accounts/{AccountDetails,ChangePassword,Register,SignIn}Tests.cs`;
      - `Operator/AdministratorCreationTests.cs`, `Operator/DemoLoadingTests.cs`;
      - `Habits/CatalogHabitStateTests.cs`;
      - `tests/HT.UiTests/Components/Account/{ChangePassword,Register,SignIn}FormTests.cs`;
      - `tests/HT.IntegrationTests/Accounts/AccountStoreTests.cs`.
  - **Port changes break callers inside the solution**, which the Work order lists as tasks:
    - `IAccountStore.CreateAsync` is called or implemented in:
      - `tests/HT.UiTests/Host/HostClient.cs`;
      - `tests/HT.IntegrationTests/Habits/HabitAccounts.cs`;
      - `Accounts/ClaimsPrincipalFactoryTests.cs`, `PasswordChangeTests.cs`, `AccountDetailsTests.cs` and
        `AccountStoreTests.cs`, including its `PasswordChangingStore` wrapper;
      - `tests/HT.UnitTests/Fakes/FakeAccountStore.cs`.
    - `IDemoDataStore.CreateDemoAccountAsync` is called or implemented in
      `tests/HT.IntegrationTests/Operator/SeedCatalogLoadingTests.cs` and `tests/HT.UnitTests/Fakes/FakeDemoDataStore.cs`.
      `SeedCatalogLoadingTests` keeps its assertions: only the call gains the profile and an empty goal list, so
      it "keeps passing" as the spec says.
    - `tests/HT.UiTests/HT.UiTests.csproj` links each unit-test fake with `<Compile Include>`, and gains the link
      for `FakeGoalStore`.

    These ports are not called by the AC support code (ADR 0003: it uses use cases and the two persistence entry
    points). If one were, the analyze run would report it, and the fix would be a cross-feature change of the AC
    writer, as in slice 2.
- **Rationale**: FR-019 and the locked F001/F002 criteria. Additive changes keep every existing call
  source-compatible; the only behaviour change is the one FR-033 asks for.
- **Alternatives**:
  - Changing `BrowseAsync` to take an acting user. Rejected: it breaks slice 2's calls, and FR-019 forbids
    that.
  - New result types beside the old ones (`PersonalCatalogBrowseResult`). That doubles the views for one extra
    number. Rejected for `init` properties.

## R-08 Stores, tables, transactions and races

- **Decision**:
  - **Port.** One new port, `HT.Application.Goals.IGoalStore` (public, fenced like the slice 2 ports, ADR
    0010). Every member takes the owner first, except `CountUsingCatalogHabitAsync(HabitId)`. That one counts
    goals across owners for the unpublish confirmation and returns only a number (ADR 0012, FR-033), so it
    reveals no private value. The reflection rule names it as the one exception (R-13).
  - **Tables** (migration `Goals`, detail in [data-model.md](data-model.md)):
    - `Goals`, with two nullable foreign keys to `CatalogHabits` and `CustomHabits`, exactly one of them set
      (mapping, no check constraint, slice 2 convention);
    - `GoalSettingsVersions`, keyed (goal, applies-from);
    - `GoalAttempts`, keyed (goal, number).
  - **Creation order** (FR-015): an `IDENTITY` column `Goals.CreationSequence`. The store lists by it, and
    Application sorts by name with a stable sort, so equal names keep creation order. Ties among supplied
    times cannot reorder anything (critic ac pass 1, MINOR-2).
  - **The account's goal revision.** `AspNetUsers.GoalsRevision` is an `int`, `0` for existing rows. It is
    the one fact that serializes goal creation against time-zone changes (critic plan pass 1, MAJOR-1). Every
    goal creation and every time-zone change increments it in its own transaction, under the account row's
    update lock.
  - **Creating a goal** is one transaction, made of conditional statements in the slice 2 style (R-14 there).
    1. Application reads the account first and computes "today" in its zone, the *expected zone*. It checks
       the start date against that "today" (FR-006, FR-012).
    2. The store reads the account row `WITH (UPDLOCK, HOLDLOCK)`. When its zone, with `NULL` resolved to the
       default (R-04), is not the expected zone, it writes nothing and returns `TimeZoneChanged`. Application
       then re-reads the account and checks the start date again, at most 10 times. After the tenth
       `TimeZoneChanged` it throws a fault exception that names no value, as the time-zone change does (step 4
       there). It is a fault, not an outcome: it needs ten time-zone changes of one account during one creation
       (critic plan pass 3, MINOR-5).
    3. For a catalog habit, the insert runs only when the habit is `Published` under the same locks. For a
       custom habit, it runs only when the owner's custom habit exists.
    4. A `NULL` account zone gets the expected zone (R-04), and `GoalsRevision` is incremented.
    5. The goal, its version 1 (with the expected zone) and attempt 1 are inserted.
    6. A habit that is no longer there gives `HabitGone`, which Application maps to FR-008's texts.

    So a concurrent unpublish or custom-habit delete never leaves a goal on a habit that was gone at commit
    (FR-034, FR-035, spec Edge Cases). The start date is checked against the same zone that version 1 keeps.
  - **Deleting a custom habit** becomes `DELETE … WHERE NOT EXISTS (goal on it WITH (UPDLOCK, HOLDLOCK))`.
    Zero rows means a re-read, to tell `UsedByGoals` from `NotFound` (FR-035). The foreign key
    `Goals.CustomHabitId` (no cascade) stays as a backstop.
  - **Changing the time zone** (FR-026) is optimistic, like slice 2's state swap:
    1. Application reads `OwnerGoals`: the account's stored zone, its `GoalsRevision` and all its goals. The
       store reads them in a **fixed order**: the account row first, then the goals, both under the database's
       read committed: locking on the local and CI SQL Server, and with `READ_COMMITTED_SNAPSHOT` on the hosted
       Azure SQL Database, where it is the default (ADR 0015; critic plan pass 2, MINOR-2; pass 3, MINOR-4). Each
       statement sees the data committed when it starts, in both modes.
       - A goal committed before the account row was read is visible in the goals read that follows.
       - A goal committed after it has incremented the revision, so step 3's check fails and the change
         re-reads.

       So a goal can be missing from the read only when the write is bound to fail. The write re-checks under
       the update lock in both modes. The order is fixed in the store's one method and documented there.

       Integration tests pin what makes the argument hold, not one interleaving. That interleaving cannot be
       forced from outside an internal store call:
       - a goal creation increments `GoalsRevision`;
       - `TryChangeTimeZoneAsync` with a stale `expectedRevision` writes nothing and returns false;
       - `ReadOwnerGoalsAsync` returns the revision it read with the goals.
    2. Domain computes each `GoalVersionChange`.
    3. The store writes everything in one transaction:
       - the account's zone;
       - each `GoalVersionWrite`;
       - `GoalsRevision + 1`.

       It writes only if the stored zone and `GoalsRevision` still equal what step 1 read, checked under the
       account row's update lock.
    4. On a conflict, Application re-reads and recomputes, at most 10 times (as slice 1's sign-in saves), then
       throws a fault exception that names no value.

    A goal created between steps 1 and 3 has incremented the revision, so the change re-reads and sees it. A goal
    created after the change commits fails its own expected-zone check, re-reads, and gets the new zone. There is
    no per-goal concurrency token: version rows change, but the `Goals` row does not, so the account's revision is
    the only token.
  - **Unpublishing**:
    1. `UnpublishAsync` without confirmation first counts goals.
    2. With a count of 0, or with confirmation, it runs slice 2's compare-and-swap.

    The count is informative, not a lock (spec Edge Cases): a goal created between the count and a confirmed
    unpublish does not stop it.
- **Rationale**: every race the spec names is closed in one statement or one transaction, without a database
  error that could carry a value (HP-011). The patterns are the ones slice 2's integration tests already
  exercise.
- **Alternatives**:
  - Serializable transactions everywhere. Heavier, with deadlock victims to handle. Rejected for targeted
    key-range locks.
  - A pessimistic lock on the account for the whole time-zone change. It holds locks while Domain computes.
    Rejected for the optimistic check.

## R-09 Catalog with goal information (FR-031, FR-032)

- **Decision**:
  - The new `BrowseAsync` overload checks, in order:
    1. with `onlyTracked`, the anonymous visitor gets "You are not signed in." and an administrator gets
       "Goals are for registered users.";
    2. it runs slice 2's filter;
    3. for a registered user it adds the owner's goal counts, from one `IGoalStore.CountByCatalogHabitAsync(owner)`
       query, and `TrackedCount` (the returned habits with a count ≥ 1).

    Without `onlyTracked`, the visitor and the administrator get slice 2's content with null goal information.
  - `GetDetailsAsync(ActingUser, …)` adds `GoalCount` and `Goals`: identifier and name, the name derived as in
    FR-010, in name order then creation order.
  - `UseCaseParameterTests` changes. `CatalogService`'s old overloads take no acting user; its new ones take it
    first.
- **Rationale**: FR-031 and FR-032, with slice 2's ordering and search code reused as it is.
- **Alternatives**: a separate "my catalog" service. That duplicates filtering; rejected.

## R-10 Demo goals (FR-040, HP-013, HP-018)

- **Decision**:
  - **Order.** `DemoDataService.LoadAsync` now runs:
    1. the password checks;
    2. the seed catalog;
    3. each demo account.

    This reverses slice 2's order (FR-040).
  - **Goal data.** `DemoGoals.For(email, today)` (Application) gives each goal as a `DemoGoal`:
    - the habit: a catalog-habit name, or the position of one of the account's demo custom habits in the
      `customHabits` list passed in the same call;
    - the catalog states it accepts (`Published`; `Coffee`: `Published` or `Unpublished`);
    - a `GoalTarget`;
    - an own name or none;
    - its `DateOnly StartDate`: `today`, or `today + 7` for Meditation.

    `DemoAccounts.ProfileFor(email)` gives the account name ("Demo One" for demo1).
  - **"The day of the run"** is `today`: `InstallationTimeZone.Value.DateAt(now)`, computed once in Application
    (critic plan pass 1, MINOR-4). The store computes no date.
  - **The port.** `IDemoDataStore.CreateDemoAccountAsync` gains two parameters:
    - `DemoAccountProfile profile`: name, time zone and creation time;
    - `IReadOnlyList<DemoGoal> goals`.

    In the same transaction as the account and its custom habits, the store does two things:
    - It looks up each named catalog habit by name key and state. This is the "state of the named catalog
      habits" read that HP-018 allows, and it is public data.
    - For a custom-habit goal, it uses the identifier it has just generated for that list position. It does not
      read the custom habit (ADR 0013: no read of private data; critic plan pass 1, MINOR-5).

    It writes the goals whose habit qualifies and skips the others, with no error.
  - **Results.** `DemoAccountResult.Goals` is "demo goals added" for a created account and null otherwise.
  - **SmokeCheck.** `SmokeCheckCoverage` gains entries for `profile` and `goals`, and `SmokeCheckRunner`'s
    check 5 adds two things:
    - GET `/goals` shows every demo goal's displayed name (`DemoGoals.DisplayNames(email)`, derived with
      `GoalText` and the seed habit names);
    - GET `/profile` shows "Demo One" for demo1.
- **Rationale**: ADR 0013 (private demo data only for accounts created in the run, in one transaction).
  FR-040's skip rules are data in Application; the store applies them inside the transaction, so no
  check-then-act gap remains.
- **Alternatives**:
  - Application reads the catalog first, then passes identifiers. That leaves a race between the read and the
    write, and it splits the unit of ADR 0013. Rejected.
  - Creating demo goals through `GoalService`. FR-040 forbids it: there is no FR-008 lookup and no acting user.

## R-11 Home heading, profile texts and dates (FR-029, FR-030, HP-002 to HP-004)

- **Decision**:
  - **`HT.Application.Accounts.AccountTexts`** gains the greetings and `MemberSince(int year, int month)`.
    Day and month names come from fixed English arrays in one formatter, `DateTexts`, with no culture:
    - `Weekday(DateOnly)`;
    - `LongDate`, which gives "Tuesday, 17 November";
    - `ShortDate`, which gives "Tue 17 Nov";
    - `DayMonth`, which gives "23 Nov";
    - `DayMonthYear`, which gives "14 Jul 2026" (the Settings card's "Started" row, HP-004; added by T037);
    - `MonthYear`, which gives "November 2026".

    The pages and the summary card use the same `DateTexts`.
  - **The greeting hour** is the local hour of the supplied time in the account's time zone, through
    the public `NamedTimeZone.TimeAt(instant)` (R-02). The bands are 05:00 to 11:59, 12:00 to 17:59, and the rest.
  - **The New goal summary card** (HP-002) is built by `HT.Application.Goals.GoalSummaryText.For(…)`. It is
    pure, called by the interactive New goal page on every change, and unit-tested for each schedule kind
    and both "first week" branches.
- **Rationale**:
  - Constitution II forbids culture reads; constitution IV wants one shared formatter.
  - The AC check "in English whatever the culture" (AC-088) passes by construction.
- **Alternatives**: `ToString("dddd, d MMMM", CultureInfo.InvariantCulture)`. That is correct today, but the
  invariant culture's names are a framework detail, and fixed arrays make the rule explicit. Rejected.

## R-12 Pages, render modes and the browser time zone (HP-001 to HP-010, HP-015 to HP-017)

- **Decision**:
  - **Routes**:
    - `/goals` (static SSR, `[Authorize]`);
    - `/goals/{Id:guid}` (static SSR, `[Authorize]`; another account's or an unknown goal renders Not found
      with 404, as slice 2's detail does);
    - `/goals/new?habit={id}&kind=catalog|custom&suggestion={n}` (**Interactive Server**: the summary card
      rewrites on every change, and the habit chooser is a Dialog with a SearchField).

    An administrator on any `/goals` address gets Not found, as slice 2 does for `/custom-habits`.
  - **Use-case calls**: interactive pages call use cases through `UseCaseScope` (slice 2 R-15); static pages
    inject the services.
  - **Register** (static form) gets:
    - a hidden input marked `data-ht-browser-time-zone`;
    - the optional "Name" field.

    `wwwroot/js/time-zone.js` is loaded **once by `App.razor`**, after `blazor.web.js`, not by the Register page.
    A script inside content that arrives by enhanced navigation does not run (critic plan pass 1, MINOR-6). It
    registers one document-level `submit` listener in the capture phase, which fills every marked input of the
    form being submitted with `Intl.DateTimeFormat().resolvedOptions().timeZone`. That works however the page was
    reached: a full load, enhanced navigation, or Back.

    Without script, the zone is empty, and registration gives the default (HP-016). A UI host test checks that
    `App.razor`'s output references the script and that the Register form has the marked input. The browser
    behaviour itself stays a manual quickstart check.
  - **Profile Account tab** gains two static forms (`TimeZoneForm` and `NameForm`) beside `ChangePasswordForm`,
    with the Notices of HP-015 and HP-017. The list is `NamedTimeZones.Choices`, plus the current value.
  - **Avatar**: `Avatar.LetterOf(name, email)` gives the first letter of the name upper-cased with
    `char.ToUpperInvariant`, or of the email. SiteNav reads the name from a claim that
    `HtUserClaimsPrincipalFactory` adds (`ht:name`).

    After a name change, the Profile page refreshes the session cookie, as the password change already does
    (`RefreshSessionAsync`), so TopNav shows the new letter.
  - **Catalog cards, details, custom habit page and Admin's unpublish Dialog** use the new overloads and
    properties (R-07, R-09). Admin's row button opens the confirmation `HtDialog` when the result is
    `ConfirmationNeeded`.
- **Rationale**: slice 2's render-mode rule, which is interactive only where the page reacts before submit.
  The Not found and `[Authorize]` patterns are unchanged.
- **Alternatives**:
  - A visible time-zone field at registration. HP-016 says it has no visible field. Rejected.
  - Reading the zone from a request header. Browsers send none. Rejected.

## R-13 Architecture rules added

- `ClockReadTests`: Domain and Application must not use the following, which forces every conversion through
  the tzdb provider (R-01):
  - `NodaTime.SystemClock`, `DateTimeZoneProviders.Bcl`, `BclDateTimeZone` and
    `DateTimeZoneProviders.Serialization`;
  - `TimeZoneInfo.FindSystemTimeZoneById`, `TimeZoneInfo.GetSystemTimeZones`, every `TimeZoneInfo.ConvertTime*`
    and `TimeZoneInfo.Local`.
- `DomainDependencyTests`: NodaTime is Domain's only non-BCL assembly. Microsoft types stay forbidden.
- New `PublicSurfaceTests`: no public or protected member of `HT.Domain` or `HT.Application` has a NodaTime
  type in its signature (R-01, ADR 0003).
- `UseCaseParameterTests`:
  - `GoalService` and `ProfileService` join `SignedInServices` (acting user first, no `AccountId` parameter).
  - `CatalogService`: each overload takes either no acting user or the acting user first.
  - `IGoalStore` members take the owner first, except the one named `CountUsingCatalogHabitAsync`.
- `ApplicationDependencyTests`: `IGoalStore` is used only by three groups:
  - `HT.Application.Goals`;
  - the three services of `HT.Application.Habits` (cards, details, unpublish count, delete rule);
  - `HT.Application.Accounts.ProfileService` (time-zone change).

  `HT.Application.Operator` does not use it: demo goals go through `IDemoDataStore` (ADR 0013).
- `UiDependencyTests`: no `HT.Ui` type depends on `IGoalStore`.
- `CoverageTests` (UiTests): unchanged rule, two new entries (R-10).

## R-14 Tests

- **Unit** (`HT.UnitTests`, with a `FakeGoalStore` over `FakeAccountStore`):
  - **Domain**: every FR-006 refusal in order. Both sides of each boundary:
    - amount 0, 1, 999 and 1000;
    - unit 20 and 21;
    - times 0, 1, 20 and 21;
    - start date yesterday, today, +365 and +366;
    - name 60 and 61.

    `GoalText` gives every FR-010 example and every "avoided on" form, and slice 2's examples are unchanged.
    `NamedTimeZone` gets every FR-024 example.
  - **`GoalTimeline`**, for each schedule kind:
    - midnight ± 1 second in Kyiv, New York and UTC;
    - both 2027 clock changes in Kyiv (2027-03-28 lasts 23 hours, 2027-10-31 lasts 25) and in New York;
    - a zone whose midnight does not exist;
    - current date and next commitment across moves west and east, including Pago_Pago → Kiritimati;
    - the FR-026 cases (none, replace first, replace pending, add, a second change after the first began).

    These are SC-003's boundary tests.
  - **Application**: refusal order (FR-023) for every method. Negative tests for another registered user, an
    administrator and the anonymous visitor (constitution III, SC-002). Demo loading is rewritten for the
    order and the goals (`DemoLoadingTests`).
- **Integration** (`HT.IntegrationTests/Goals`, database `HT-IT`):
  - the stores, with owner scoping;
  - creation order;
  - a password change whose user was loaded before a time-zone change or a goal creation committed: the zone,
    the name and `GoalsRevision` survive the save (R-04);
  - races, each forced with two contexts:
    - a goal created against an unpublish;
    - a goal created against a custom-habit delete;
    - a goal created against a time-zone change;
    - two time-zone changes;
  - the demo transaction with goals, plus the skip rules on `HT-IT-SEED`;
  - log capture for HP-011: no goal, habit or account name, unit or amount in any log entry;
  - the HP-019 test, literally on an account created before the migration (critic plan pass 3, MINOR-3), on
    its own derived database `HT-IT-MIGRATE`. It follows the `HT-IT-SEED` pattern (`SeedDatabaseFixture`) and
    adds no entry point to `HT.Persistence`:
    1. Compose with `AddHtPersistence`.
    2. Resolve the internal `HtDbContext` by its type name. Reflection needs no `InternalsVisibleTo`, and the
       resolved object is a public `DbContext`.
    3. Migrate to `HabitCatalog` through EF's public `IMigrator`.
    4. Insert an account row with slice 1's columns only.
    5. Apply the remaining migrations with the public `HtDatabase.MigrateAsync`.
    6. Check four things:
       - the account reads as the default zone, with a configured non-UTC `InstallationTimeZone`;
       - it has no name and no "member since";
       - its `GoalsRevision` is 0;
       - its first goal writes the zone into the row.
- **UI** (`HT.UiTests`):
  - bUnit tests of the new components and pages, and of every slice 1 and 2 test the spec lists;
  - host tests for the routes, Not found and redirects;
  - `StartupTasksDemoTests` for the line order of HP-013;
  - existing tests that change with the constructors and check 5 (critic plan pass 1, MINOR-2):
    - `Components/Pages/PageServices.cs` registers a `FakeGoalStore` next to the fake catalog and custom-habit
      stores, in the same commit as steps 3 and 4;
    - `SmokeChecks/RunnerBehaviourTests.GoodSite()` scripts `GET /goals` and `GET /profile`, and gains one failing
      case for a missing goal name;
    - `SmokeChecks/CoverageTests` gets its two entries;
  - the `time-zone.js` hidden field, as markup only (the script runs only in a real browser, so it is
    unverified in tests, and the quickstart checks it by hand).
- **Acceptance**: `/speckit-ac-tests-write` and `/speckit-ac-tests-analyze`, run by the owner. F001 and F002
  must keep passing (FR-019).
