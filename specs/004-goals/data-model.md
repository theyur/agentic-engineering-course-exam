# Data model: Goals

**Feature**: specs/004-goals · **Date**: 2026-11-18 · **Plan**: [plan.md](plan.md) · **Research**: [research.md](research.md)

Types are named in the vocabulary of `CONTEXT.md` (constitution I). Every public member uses BCL types only;
NodaTime is internal to `HT.Domain` (research R-01).

## Domain: time (`HT.Domain.Time`)

| Type | Kind | Members and rules |
|---|---|---|
| `NamedTimeZone` | readonly record struct | `Id` (string, as given). `static Utc`. `static bool TryParse(string?, out NamedTimeZone)`, FR-024: exactly `UTC`, or a tzdb identifier (zone or link) containing `/` and not starting with `Etc/`. Ordinal, case-sensitive, not trimmed. Public, with BCL types: `DateAt(DateTimeOffset) → DateOnly` ("today"), `TimeAt(DateTimeOffset) → TimeOnly` (greeting hour), `StartOfDay(DateOnly) → DateTimeOffset` (UTC). NodaTime stays behind them (research R-02). |
| `NamedTimeZones` | static class | `Choices`: `UTC`, then the canonical tzdb zones that are named time zones, in ordinal order (HP-015). |

## Domain: goals (`HT.Domain.Goals`)

| Type | Kind | Members and rules |
|---|---|---|
| `GoalId` | readonly record struct | `Guid Value`, created by the store. |
| `HabitKind` | enum | `Catalog`, `Custom` (ADR 0014: one `HabitId`, the kind says where it is looked up). |
| `GoalHabit` | readonly record struct | `HabitId Id`, `HabitKind Kind`. |
| `GoalTargetFields` | sealed record | One nullable value per form control: `BuildOrReduce`, `Measurement`, `Amount`, `CountUnit`, `DurationUnit`, `ScheduleKind`, `Days`, `Times`, `Period`. |
| `GoalTarget` | sealed class | `BuildOrReduce`, `Measurement`, `Amount?`, `CountUnit?` (trimmed), `DurationUnit?`, `Schedule`. Values that do not belong to the measurement or schedule are not stored (spec "Target fields"). `ScheduleKindOf` → `CommitmentBased` (fixed days) or `PeriodBased` (times), plus `PeriodLength` (day or week; null for fixed days). Only the public `TryCreate(GoalTargetFields, out GoalTarget?, out GoalProblem)` makes one (target checks, then `ReduceAmountNeedsOneTime`); Persistence rebuilds a stored target through it. Value equality. |
| `GoalScheduleKind` | enum | `CommitmentBased`, `PeriodBased` (FR-003). |
| `GoalSettings` | sealed record | `GoalTarget Target`, `NamedTimeZone TimeZone`. |
| `GoalSettingsVersion` | sealed record | `GoalSettings Settings`, `DateOnly AppliesFrom` (FR-005). |
| `GoalAttempt` | sealed record | `int Number`, `DateOnly StartDate`; this slice has only attempt 1, not ended (FR-018). |
| `GoalName` | readonly record struct | Trimmed text, 1 to 60 UTF-16 code units; `TryCreate(string?, out GoalName?, out bool tooLong)`, where empty means none (FR-009). |
| `GoalProblem` | enum | The FR-006 order: `None`, `HabitMissing`, then `BuildOrReduceMissing`, `MeasurementMissing`, `YesNoHasAmountOrUnit`, `BuildAmountOutOfRange`, `ReduceAmountOutOfRange`, `CountUnitMissing`, `CountUnitTooLong`, `DurationUnitMissing`, `ScheduleMissing`, `DaysMissing`, `TimesOutOfRange`, `PeriodMissing`, then `ReduceAmountNeedsOneTime`, `StartDateMissing`, `StartDateInPast`, `StartDateTooFar`, `NameTooLong`. |
| `GoalField` | enum | `Habit`, `BuildOrReduce`, `Measurement`, `Amount`, `Unit`, `Schedule`, `Days`, `Times`, `Period`, `StartDate`, `Name`. Each `GoalProblem` maps to one. |
| `GoalDraft` | static class | `CheckHabit(GoalHabit?)`. `TryCreate(GoalTargetFields, DateOnly? startDate, DateOnly today, string? name, out GoalTarget, out DateOnly, out GoalName?)` → `GoalProblem`, in the order above; the habit lookup runs between the two (research R-07). |
| `Goal` | sealed class | `GoalId Id`, `AccountId Owner`, `GoalHabit Habit`, `DateOnly StartDate`, `GoalName? OwnName`, `GoalAttempt Attempt`, `IReadOnlyList<GoalSettingsVersion> Versions` (ordered by `AppliesFrom`; the first applies from `StartDate`). No persistence detail: creation order is the order of the store's lists (`StoredGoal`, Application). |
| `GoalText` | static class | `TargetText(GoalTarget)`, `Kind(GoalTarget)`, `Name(Goal goal, string habitName)`: own name, or `habitName + ", " + TargetText`. The one exception: Reduce, yes/no, fixed days gives "avoided on Mon–Fri" / "avoided every day" (spec, "Target text and kind"). `ScheduleText(GoalTarget)`, the Goals meta line and the Settings card's "Schedule" row (HP-003, HP-004): fixed days "Every day" for all seven, otherwise `Kind`'s day groups joined by " · " ("Mon · Wed · Fri", "Mon–Fri", "Mon–Wed · Sat"); 1 time per day "Every day", N per day "N times a day"; per week "Flexible · once a week" or "Flexible · N times a week". |
| `GoalTimeline` | static class | Research R-06. `MaxCalendarDays` = 366. `VersionOf(goal, date)`, `CurrentDate(goal, instant) → DateOnly?`, `TryCalendar(goal, first, last, out EvaluationCalendar)` (false when `last` < `first` or the range has more than 366 days), `NextCommitment(goal, instant) → DateOnly?`, `FirstWeekStart(goal) → DateOnly?`, `ChangeOf(goal, instant, NamedTimeZone) → GoalVersionChange`. |
| `Commitment` | sealed record | `DateOnly Date`, `DateTimeOffset Starts`, `DateTimeOffset Ends` (UTC). |
| `EvaluationPeriod` | sealed record | `DateOnly FirstDate`, `DateOnly LastDate` (equal for a day), `Starts`, `Ends`. |
| `EvaluationCalendar` | sealed class | `GoalScheduleKind Kind`, and `Commitments` or `Periods` in date order. No status (FR-013). |
| `GoalVersionChange` | sealed record | `None`, `ReplaceFirst(zone)`, `ReplacePending(zone, appliesFrom)`, `Add(appliesFrom, zone)` (FR-026), with `Kind` (`GoalVersionChangeKind`), `TimeZone?` and `AppliesFrom?`. `ApplyTo(Goal) → Goal`: ReplaceFirst changes the first version's zone, ReplacePending the last version's zone and date, Add appends a version with the same target. |

### Invariants (unit-tested)

- `Versions` is non-empty. The first version applies from `StartDate`. Dates strictly increase. Only the time zone
  differs between versions in this slice (FR-005).
- At any instant, at most one version applies after the current date. A version applies from a date after the
  start date only where the goal's next boundary falls: any day for commitment-based and per-day goals, a Monday for
  per-week goals (FR-026).
- A Reduce count or duration target has N = 1 when its schedule is N times (FR-004).
- The start date is from "today" to "today" + 365, checked only at creation (FR-006). A stored goal is never
  re-checked against a later "today".

## Domain: accounts (`HT.Domain.Accounts`, extended)

| Type | Change |
|---|---|
| `Account` | `init` properties: `string? StoredTimeZone` (null only for accounts created before this slice, research R-04), `string? Name`, `DateTimeOffset? CreatedAt`. The positional constructor is unchanged. |
| `AccountName` | New readonly record struct: trimmed, 1 to 60 code units, empty means none (FR-028). |

## Application inputs and views

| Type | Namespace | Members |
|---|---|---|
| `GoalInput` | `HT.Application.Goals` | `GoalHabit? Habit`, the target fields (as `GoalTargetFields`), `DateOnly? StartDate`, `string? Name`. |
| `GoalStartingValues` | `HT.Application.Goals` | `GoalHabitView? Habit`, `BuildOrReduce?`, `Measurement?`, `Amount?`, `CountUnit?`, `DurationUnit?`, `ScheduleKind?`, `Days`, `Times?`, `Period?`, `DateOnly StartDate`, `string? Name` (always null), `string? CatalogSuggestionText` (FR-007, the goal rules' text). |
| `GoalHabitView` | `HT.Application.Goals` | `HabitId Id`, `HabitKind Kind`, `string Name` (current), `Category Category`, `bool? IsInCatalog` (catalog only, FR-034). |
| `GoalListItem` | `HT.Application.Goals` | `GoalId Id`, `string Name`, `string HabitName`, `HabitKind HabitKind`, `BuildOrReduce`, `Measurement`, `string TargetText`, `string ScheduleText` (`GoalText.ScheduleText`, the meta line's schedule: "Mon · Wed · Fri", HP-003, US2-AS6), `GoalScheduleKind ScheduleKind`, `DateOnly? StartDate` (only when there is no current date), `DateOnly? NextCommitment` (commitment-based only), `DateOnly? FirstWeekStarts` (per-week, only while that week has not begun). Fields are FR-015 and FR-014; amount, unit and days are not list fields. |
| `GoalDetails` | `HT.Application.Goals` | `Id`, `Name`, `bool HasOwnName`, `GoalHabitView Habit`, `BuildOrReduce`, `Measurement`, `Amount?`, `CountUnit?`, `DurationUnit?`, `ScheduleKind`, `Days`, `Times?`, `Period?`, `GoalScheduleKind GoalScheduleKind`, `string TargetText`, `string ScheduleText` (`GoalText.ScheduleText`, the Settings card's "Schedule" row, HP-004), `string Kind`, `DateOnly StartDate`, `DateOnly? FirstWeekStarts`, `string TimeZone` with `DateOnly TimeZoneAppliesFrom` (the current date's version, or the first version), `string? LaterTimeZone` with `DateOnly? LaterTimeZoneAppliesFrom`, `int AttemptNumber` with `DateOnly AttemptStartDate` (FR-016, FR-018). |
| `StoredGoal` | `HT.Application.Goals` | What `IGoalStore` reads: `Goal Goal`, `GoalHabitView Habit`. Lists come in creation order. |
| `OwnerGoals` | `HT.Application.Goals` | One consistent read for a time-zone change: `string? StoredTimeZone`, `int GoalsRevision`, `IReadOnlyList<StoredGoal> Goals` (research R-08). |
| `NewGoal` | `HT.Application.Goals` | What creation writes: `GoalHabit Habit`, `DateOnly StartDate`, `GoalName? OwnName`, `GoalTarget Target`. |
| `GoalWriteOutcome` | `HT.Application.Goals` | `Created(GoalId)` · `HabitGone` · `TimeZoneChanged` (the store's outcome, not a use-case outcome). |
| `GoalVersionWrite` | `HT.Application.Goals` | `GoalId Goal`, `GoalVersionChange Change`: one write of a time-zone change. |
| `GoalCreateOutcome` | `HT.Application.Goals` | The use-case outcomes of `GoalService.CreateAsync`, in check order: `NotSignedIn`, `NotForAdministrators`, `HabitMissing`, `NotInCatalog`, `CustomHabitNotFound`, the 12 target outcomes with `GoalProblem`'s names, `ReduceAmountNeedsOneTime`, `StartDateMissing`, `StartDateInPast`, `StartDateTooFar`, `NameTooLong`, `Created`. Each other `GoalService` method has its own outcome enum, as in slice 2. |
| `GoalCalendarView` | `HT.Application.Goals` | `GoalScheduleKind Kind`, `IReadOnlyList<Commitment> Commitments`, `IReadOnlyList<EvaluationPeriod> Periods`. |
| `GoalOnHabit` | `HT.Application.Habits` | `GoalId Id`, `string Name` (details of a catalog habit, FR-031). |
| `ProfileView` | `HT.Application.Accounts` | `string? Name`, `string Email`, `string? MemberSince` ("member since November 2026", or null, FR-030). |
| `HomeHeading` | `HT.Application.Accounts` | `string Greeting`, `string Date` (FR-029). |
| `InstallationTimeZone` | `HT.Application.Accounts` | `NamedTimeZone Value`; `From(string?)` gives UTC when absent or invalid (research R-03). |

## Demo data (`HT.Application.Operator`)

| Type | Members |
|---|---|
| `DemoGoal` | `string? CatalogHabitName` or `int? CustomHabitPosition` (the index into the same call's `customHabits`), `IReadOnlySet<CatalogHabitState> AcceptedStates` (catalog only), `GoalTarget Target`, `GoalName? OwnName`, `DateOnly StartDate`. |
| `DemoGoals` | `For(email, DateOnly today) → IReadOnlyList<DemoGoal>` (FR-040), `DisplayNames(email) → IReadOnlyList<string>` for the smoke check. |
| `DemoAccountProfile` | `string? Name`, `NamedTimeZone TimeZone`, `DateTimeOffset CreatedAt`. |
| `DemoGoalsResult` | `Outcome` `Added`, `Message` "demo goals added". |

## Ports

### `IGoalStore` (`HT.Application.Goals`, new)

| Member | Notes |
|---|---|
| `CreateAsync(AccountId owner, NewGoal goal, NamedTimeZone expectedZone, NamedTimeZone defaultZone, CancellationToken)` → `GoalWriteOutcome` | One transaction (research R-08). `TimeZoneChanged` when the account's zone, with `NULL` read as `defaultZone`, is not `expectedZone`. Otherwise it writes version 1 with `expectedZone` and attempt 1, sets a `NULL` account zone, and increments `GoalsRevision`. |
| `ReadOwnerGoalsAsync(AccountId owner, …)` → `OwnerGoals` | Reads the account row first, then the goals. That order makes the revision check safe under locking read committed and under read committed snapshot (research R-08). |
| `ListAsync(AccountId owner, …)` → `IReadOnlyList<StoredGoal>` in creation order, each with its habit's current name, category and state | Joined in one query. |
| `FindAsync(AccountId owner, GoalId id, …)` → `StoredGoal?` | |
| `CountByCatalogHabitAsync(AccountId owner, …)` → `IReadOnlyDictionary<HabitId, int>` | Cards and "Only habits I track". |
| `ListOnCatalogHabitAsync(AccountId owner, HabitId habit, …)` | Details, "Your goals on this habit". |
| `CountUsingCustomHabitAsync(AccountId owner, HabitId habit, …)` | Custom habit page (FR-035). |
| `CountUsingCatalogHabitAsync(HabitId habit, …)` → `int` | The one member without an owner: a count across owners for the unpublish confirmation (ADR 0012). |
| `TryChangeTimeZoneAsync(AccountId owner, string? expectedStoredZone, int expectedRevision, NamedTimeZone next, IReadOnlyList<GoalVersionWrite> writes, …)` → `bool` | One transaction under the account row's update lock. False, with nothing written, when the stored zone or `GoalsRevision` changed. Otherwise it writes the zone and every write, and increments `GoalsRevision` (research R-08). |

### `IAccountStore` (extended)

| Member | Notes |
|---|---|
| `CreateAsync(Email, Role, string password, NewAccountProfile profile, …)` | Replaces slice 1's signature (a port, research R-04, R-07). `NewAccountProfile` is `AccountName? Name`, `NamedTimeZone TimeZone` and `DateTimeOffset CreatedAt`, all supplied by Application. |
| `TrySetNameAsync(AccountId, AccountName?, …)` → `bool` | False when there is no such account. |
| `FindByIdAsync` / `FindByEmailAsync` | Now fill `StoredTimeZone`, `Name` and `CreatedAt`. |

### `ICustomHabitStore` (extended)

| Member | Notes |
|---|---|
| `DeleteAsync` | Outcome gains `UsedByGoals`: a conditional delete (research R-08). |

### `IDemoDataStore` (extended)

| Member | Notes |
|---|---|
| `CreateDemoAccountAsync(Email, string password, IReadOnlyList<HabitDefinition> customHabits, DemoAccountProfile profile, IReadOnlyList<DemoGoal> goals, …)` | Replaces the slice 2 member. One transaction, which looks up the goals' habits by name key and state (research R-10). Each new parameter gets a `SmokeCheckCoverage` entry (HP-018). |

## Tables (`HT.Persistence`, migration `Goals`)

Conventions of slice 2 hold: no check constraints, no default values except `IDENTITY` and `GoalsRevision`'s `0`, enums stored as
`varchar(16)` strings, and binary collation for keys.

### `AspNetUsers` (changed)

| Column | Type | Notes |
|---|---|---|
| `TimeZone` | `nvarchar(64) NULL` | `NULL` only for accounts that existed before the migration (research R-04). |
| `GoalsRevision` | `int NOT NULL DEFAULT 0` | Incremented by every goal creation and time-zone change, so the two serialize (research R-08). A default, as `FailedSignInCount` has. |
| `Name` | `nvarchar(60) NULL` | FR-028. |
| `CreatedAt` | `datetimeoffset NULL` | FR-030. |

All four are mapped with `SetAfterSaveBehavior(Ignore)`, so Identity's full-row `UpdateAsync` (on a password change) never writes them. After the insert, only the stores' conditional statements do (research R-04, R-08).

### `Goals`

| Column | Type | Notes |
|---|---|---|
| `Id` | `uniqueidentifier` PK | `ValueGeneratedNever`. |
| `OwnerId` | `uniqueidentifier` FK → `AspNetUsers.Id`, cascade | Index `IX_Goals_OwnerId`. |
| `CatalogHabitId` | `uniqueidentifier NULL` FK → `CatalogHabits.Id`, no action | Index. Exactly one of the two habit columns is set (mapping). |
| `CustomHabitId` | `uniqueidentifier NULL` FK → `CustomHabits.Id`, no action | Index. |
| `StartDate` | `date` | |
| `Name` | `nvarchar(60) NULL` | The own name. |
| `CreationSequence` | `bigint IDENTITY` | Creation order (FR-015). Unique index. |

Accounts are never deleted in this slice. Slice 11 deactivates and keeps the data, so the cascade only matters for
the test reset.

### `GoalSettingsVersions`

| Column | Type | Notes |
|---|---|---|
| `GoalId` | FK → `Goals.Id`, cascade | PK (`GoalId`, `AppliesFrom`). |
| `AppliesFrom` | `date` | |
| `BuildOrReduce`, `Measurement` | `varchar(16)` | |
| `Amount` | `int NULL` | |
| `CountUnit` | `nvarchar(20) NULL` | |
| `DurationUnit` | `varchar(16) NULL` | |
| `ScheduleKind` | `varchar(16)` | |
| `Days` | `int NULL` | Bit mask Monday 1 to Sunday 64, as in `CatalogHabitSuggestedStartingGoals`. |
| `Times` | `int NULL` | |
| `Period` | `varchar(16) NULL` | |
| `TimeZone` | `nvarchar(64)` | The version's named time zone (FR-011). |

### `GoalAttempts`

| Column | Type | Notes |
|---|---|---|
| `GoalId` | FK → `Goals.Id`, cascade | PK (`GoalId`, `Number`). |
| `Number` | `int` | 1 in this slice. |
| `StartDate` | `date` | The goal's start date (FR-018). Slice 8 adds end columns. |

## State transitions

- Goal: created, then active. No other state in this slice (FR-017).
- Settings versions: version 1 at creation. A time-zone change replaces the first version, replaces the pending
  version, or adds one (`GoalVersionChange`).
- Catalog habit (slice 2), extended: `Published → Unpublished` needs `confirmed` when `CountUsingCatalogHabitAsync`
  is more than 0 (FR-033).
- Custom habit (slice 2), extended: delete is refused while a goal uses it (FR-035).
