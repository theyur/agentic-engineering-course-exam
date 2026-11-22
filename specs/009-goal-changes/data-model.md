# Data model: Goal changes

Phase 1 of [plan.md](plan.md). Tables, Domain types and Application types this slice adds or changes, with their
validation rules and state transitions. Decisions are in [research.md](research.md). The public API is in
[contracts/application-api.md](contracts/application-api.md). Existing constructors, method signatures, positional
records and port members of slices 1 to 7 are unchanged (FR-024). Every added property is init-only and not
`required`.

## Tables (migration `GoalChanges`)

| Table | Change | Notes |
|---|---|---|
| `GoalAttempts` | + `EndingDate` `date` NULL | The ending date E (FR-007). Null while the attempt is not ended. |
| `GoalAttempts` | + `Outcome` `varchar(16)` NULL | `Achieved`, `Abandoned` or `NoLongerRelevant`, as a string like the other enums. |
| `GoalAttempts` | + `EndingNote` `nvarchar(500)` NULL | Trimmed, 1 to 500 characters; null when there is none. Private (HP-010). |
| `GoalSettingsVersions` | + `AttemptNumber` `int` NOT NULL, default 1 for existing rows | Every version of slices 4 to 7 belongs to attempt 1. |
| `GoalSettingsVersions` | key (`GoalId`, `AppliesFrom`) → (`GoalId`, `AttemptNumber`, `AppliesFrom`) | Two attempts can have versions from the same date (research R-01). |
| `GoalSettingsVersions` | + foreign key (`GoalId`, `AttemptNumber`) → `GoalAttempts` (`GoalId`, `Number`), on delete no action | No orphan version. The existing cascades `Goals` → `GoalSettingsVersions` and `Goals` → `GoalAttempts` stay. A cascade here would give SQL Server two cascade paths, which it refuses (research R-05). |

The mapping checks the following, not a check constraint, as for the goal's habit columns:

- `EndingDate` and `Outcome` are both set or both null;
- `EndingNote` is null when they are;
- the attempt numbers of a goal run 1, 2, … with no gap;
- every attempt but the latest is ended;
- attempt 1's first version applies from the goal's start date, and every later attempt's first version from that
  attempt's start date.

A stored row that fails a check is a fault whose message names no value (HP-010). The last date is never stored
(FR-026).

The migration's order on a populated database:

1. add `AttemptNumber` with the default 1;
2. drop the old key, then add the new key and the foreign key;
3. add the three nullable attempt columns.

`Down` reverses it. It is only valid while every version belongs to attempt 1, as in every migration that narrows a
key.

Settled in T037: the default 1 exists only in the migration. Step 1 adds the column with the default 1, which fills
the existing rows, and drops the default constraint again in the same step (an `AlterColumn`), because the model has no
default value: slice 4 keeps default values out of the goal tables, and a default on a key column of the model would
give EF Core's key-default warning. Every writer sets `AttemptNumber` explicitly. EF Core generated the steps in
another order (drop the key, add `AttemptNumber`, add the three ending columns, add the key and the foreign key);
`20261122142029_GoalChanges.cs` has them in the order above.

## Domain: `HT.Domain.Goals` (new types)

### `Outcome` (enum)

`Achieved`, `Abandoned`, `NoLongerRelevant` (FR-007). How an attempt ended. Never an evaluation state, never counted
as a result (Key Entities). Its user-facing texts are "Achieved", "Abandoned" and "No longer relevant"
(`GoalChangeTexts`).

### `EndingNote` (readonly record struct)

- `TryCreate(string? text, out EndingNote? note, out bool tooLong)`, with the same rules as `GoalName.TryCreate`:
  - the text is trimmed;
  - empty or only spaces gives no note and true;
  - more than `RecordLimits.MaxNoteLength` (500) UTF-16 code units after trimming gives false and `tooLong`.
- `Value` holds the text. `ToString()` returns a fixed placeholder, never the value (HP-010).
- It is a type of its own, not `NoteText`, because an ending note is not a note of a commitment or period (FR-027).

### `GoalEnding` (sealed record)

`GoalEnding(DateOnly Date, Outcome Outcome, EndingNote? Note)`: the stored fact of an ending (FR-026). `Date` is the
ending date E. The constructor rejects an undefined `Outcome`.

### `GoalAttemptVersions` (sealed record)

`GoalAttemptVersions(GoalAttempt Attempt, IReadOnlyList<GoalSettingsVersion> Versions)`: one attempt with its own
settings versions, in date order. A code grouping for the `Goal` constructor and `Goal.Attempts`, not a domain term.

## Domain: `HT.Domain.Goals` (changed types)

### `GoalAttempt` (sealed record, changed)

- The constructor `(int number, DateOnly startDate)` is unchanged.
- `Ending` (`GoalEnding?`, init-only) is null while the attempt is not ended.
- Value equality now includes `Ending`.

### `Goal` (sealed class, changed)

- **New constructor:** `Goal(GoalId id, AccountId owner, GoalHabit habit, GoalName? ownName,
  IReadOnlyList<GoalAttemptVersions> attempts)`. Its invariants (research R-01) throw `ArgumentException`:
  - at least one attempt, numbered 1, 2, … in order;
  - every attempt but the last has an `Ending`;
  - each attempt has at least one version, and its dates strictly increase. Attempt 1 keeps slice 4's rules: its
    first version applies from the goal's start date, and the attempt does not begin before it. Every later
    attempt's first version applies from that attempt's start date. The goal's start date is attempt 1's first
    version's date.
  - all versions of one attempt have the same schedule kind (`GoalTarget.HasSameScheduleKind`, D-1);
  - a later version of a per-week attempt applies from a Monday;
  - attempt n + 1 starts after attempt n's ending date (FR-012);
  - an ended attempt has no version after its last date (`GoalTimeline.LastDate`) except its first (FR-009);
  - the habit kind is defined.
- **Kept constructor:** `Goal(id, owner, habit, startDate, ownName, attempt, versions)` keeps its checks. Only the
  rule "only the time zone differs" is replaced by "the same schedule kind". It builds a goal with the one attempt
  `attempt` and the versions `versions`, with `startDate` as its start date. These are exactly the new constructor's
  rules for attempt 1. No combination that slice 4 accepts is now refused. The one combination it refused that is
  now accepted is the intended one: versions of the same schedule kind with different targets (e.g. 1 and then 2
  times per day; research R-12). An attempt 1 that starts after `startDate` stays accepted, as in slice 4, and is
  timed from `startDate` (research R-01, "The timing date of an attempt").
- **Members:**

  | Member | Meaning | For one attempt, no ending |
  |---|---|---|
  | `StartDate` | the goal's start date: attempt 1's first version's date (FR-001). Not the first date of a later attempt, which can be earlier (AC-035); readers that mean an attempt's first date use that attempt's (research R-01) | unchanged |
  | `Attempt` | the latest attempt | unchanged |
  | `Versions` | the latest attempt's versions | unchanged |
  | `Attempts` (new) | every attempt with its versions, in number order | one entry |
  | `IsEnded` (new) | the latest attempt has an `Ending` | false |
  | `VersionsOf(int number)` (new) | that attempt's versions; throws for an unknown number | `Versions` for 1 |

- `Target` (internal) is removed. `WithVersions` (internal) replaces the latest attempt's versions. New internal
  `WithName`, `WithEnding` and `WithAttempt` build the goal after a write, checked again by the constructor.

### `GoalTarget` (changed)

`HasSameScheduleKind(GoalTarget other)`: both are fixed days, or both times with the same `SchedulePeriod` (D-1,
FR-001).

### `GoalDraft` (changed)

`CheckTarget(GoalTargetFields fields, out GoalTarget? target)` returns the first `GoalProblem` of the target steps of
`TryCreate`, in their order (FR-003 step 5), or `None`. `TryCreate` is unchanged. `FieldOf` gains
`StartDateNotAfterEnding → GoalField.StartDate`.

### `GoalProblem` (enum, changed)

Adds `StartDateNotAfterEnding`, placed after `StartDateInPast`. It is used only by starting a new attempt: "A new
attempt starts after the day the goal ended." (FR-012). Goal creation never produces it.

### `GoalTimeline` (static class, changed; research R-02 to R-04)

The existing members keep their signatures and are attempt-aware. For a goal with one attempt, no ending and no
change, they give what they gave in slice 7.

| Member | Slice 8 rule |
|---|---|
| `VersionOf(goal, date)` | the version of the attempt that contains `date`. Outside every attempt, the latest attempt's version in effect then (its first before its start). |
| `CurrentDate(goal, instant)` | the latest attempt's current date while it has begun and is not ended. Otherwise, once any attempt has begun, the date of `instant` in the zone of the last version of the latest attempt that has begun. Otherwise null. Each attempt's candidates start at that attempt's timing date: the goal's start date for attempt 1, its own start date for a later attempt (research R-01). |
| `TryCalendar`, `TryEntries`, `EntryOf` | only commitments and periods of an attempt: from its timing date (per week: the first Monday on or after it) to its last date, or with no end while it is not ended. A range across attempts lists every attempt's entries in it. Commitments go to `Commitments` and periods to `Periods`, each in date order. `Kind` is the kind of the latest attempt with an entry in the range, else of the latest attempt starting on or before the range's last date, else of attempt 1. `TryEntries` merges both lists in date order (research R-02). |
| `HasBegun(goal, date, instant)` | the date belongs to an attempt whose own timeline has begun the date at `instant`. |
| `EntriesThrough(goal, last)` | the latest attempt's entries through `last` (the report range, FR-013). Its first window starts at the Monday on or before the latest attempt's timing date. |
| `NextCommitment`, `FirstWeekStart` | of the latest attempt, from its timing date. None for an ended goal. |
| `ChangeOf(goal, instant, zone)` | on the latest attempt. `None` for an ended goal. `ReplaceFirst` while the latest attempt has not begun. |

New members:

| Member | Returns |
|---|---|
| `LastDate(Goal goal, int number)` | null while the attempt is not ended. Otherwise, from its ending date E: E − 1, or for a per-week attempt the Sunday before E's week (FR-008). A last date before the attempt's first entry means it has no date: no commitment or period. That covers a last date before the start date, and also a per-week attempt ended before its first full week (research R-02, plan "Spec discrepancies"). |
| `VersionInEffect(Goal goal, int number, DateTimeOffset instant)` | One attempt's settings in effect: for an ended attempt its last version (the version of its last date, or its first when it has no date); while it has not begun its first version; otherwise its version at the goal's current date. |
| `VersionInEffect(Goal goal, DateTimeOffset instant)` | The same for the latest attempt: FR-005's settings in effect. For one attempt it equals `VersionOf(goal, CurrentDate(goal, instant) ?? goal.StartDate)`. |
| `LatestAttemptHasBegun(Goal goal, DateTimeOffset instant)` | Whether the latest attempt has begun. A not-ended attempt has begun once the day of its timing date has begun in its first version's zone. An ended attempt has begun when it has a date (a commitment or period up to its last date), so one ended before its first entry never begins (research R-02; plan "Spec discrepancies" 5). `CurrentDate` uses the same rule for "the latest attempt that has begun". |
| `AttemptOf(Goal goal, DateOnly date)` | the number of the attempt whose entries contain `date`, or null. |
| `EntriesThrough(Goal goal, int number, DateOnly last)` | that attempt's entries through `last` or its last date, whichever is earlier. |
| `SettingsChangeOf(Goal goal, DateTimeOffset instant, GoalTarget target, NamedTimeZone zone)` | a `GoalSettingsChange`: the latest attempt's new version list and `AppliesFrom` (research R-03). Throws for an ended goal or another schedule kind; the use case checks both first. |
| `EndingOf(Goal goal, DateOnly endingDate)` | a `GoalEndingEffect`: the last date and the versions the attempt keeps (research R-04). Throws for an ended goal. |
| `NewAttemptEarliestStart(Goal goal, DateOnly accountToday)` | the later of `accountToday` and the ending date + 1 (FR-011). Throws for a goal that is not ended. |

### `GoalSettingsChange` and `GoalEndingEffect` (sealed records, new)

- `GoalSettingsChange(IReadOnlyList<GoalSettingsVersion> Versions, DateOnly AppliesFrom)`.
- `GoalEndingEffect(DateOnly LastDate, IReadOnlyList<GoalSettingsVersion> KeptVersions)`.

Both are results of `GoalTimeline`, written by the store as they are.

### `GoalVersionChange` (changed)

`ApplyTo(goal)` applies to the latest attempt's versions. `ReplaceFirst` changes that attempt's first version
(FR-021).

### `EvaluationCalendar` (changed doc only)

The shape is unchanged. `Commitments` and `Periods` are each "in date order, empty when the range has no attempt of
that kind". Both are non-empty only for a range that spans attempts of both kinds. `Kind` follows `TryCalendar`'s
rule above. The public `GoalCalendarView(Kind, Commitments, Periods)` mirrors it with the same doc change.

### `GoalText` (changed)

- `Name(Goal goal, string habitName, DateTimeOffset instant)` (new overload): the goal's own name, or the habit's
  current name, ", " and the target text of `GoalTimeline.VersionInEffect(goal, instant)` (FR-004, AC-001, AC-002).
  Every production caller uses it with its use case's single time read.
- `Name(goal, habitName)` (kept signature): the latest attempt's first version's target, which is what it gives
  today for every goal of slices 4 to 7 (today's code uses `Goal.Target`, the first version). No production code
  calls it after this slice.
- `AttemptName(Goal goal, int number, string habitName)` (new): the goal's own name if it has one, otherwise
  `habitName`, ", " and the target text of that attempt's last version (FR-004). Used for every attempt that is not
  the latest.

### `TrendBuilder` (changed, `HT.Domain.Reports`; research R-07)

Its public signatures are unchanged. Each begun entry's own target already reaches the builder: both `Of` overloads
take it as the `Target` of each item (`GoalEvaluation` passes the target of the entry's own version), so no new
parameter or item type is needed (T021). The rule of R-07 is applied inside the builder:

- An item whose `Target.Measurement` differs from the trend's target (`latest`) is "of another measurement".
- In a commitment-based yes/no trend such an item counts one "Yes" in its week when it is Met by its own target (the
  internal `Evaluator.Meets`, the final-evaluation rule with a record), and none otherwise. A yes/no item counts its
  "Yes" record as before.
- In every other trend its bar has no value, keeps its mark and its own target, and `Trend.Target` (the line) is the
  target of the latest kept bar that is not marked and is of the trend's measurement.

## Application: `HT.Application.Goals` (research R-03, R-05, R-06)

### Inputs

- `GoalSettingsInput`: `GoalTargetFields Target`, `string? Name`. The fields of goal creation, without habit or start
  date (FR-001).
- `NewAttemptInput`: `GoalTargetFields Target`, `DateOnly? StartDate` (FR-012).

### Results and views

Each result has `Outcome` (an enum below), `Message` (the literal text) and the fields named. A refusal carries no
view.

| Result | Outcomes | Fields on success |
|---|---|---|
| `SettingsChangeResult` | `Changed`, `Unchanged`, `NotSignedIn`, `NotForAdministrators`, `NotFound`, `GoalEnded` (text `GoalChangeTexts.GoalEnded`, "This goal has ended."), the target problems of `GoalProblem` (in its order), `ScheduleKindChanged`, `NameTooLong` | `AppliesFrom` (`DateOnly?`; null for a name-only change), `Field` (`GoalField?`) on a refusal |
| `GoalEndResult` | `Ended` (text `GoalChangeTexts.Ended`, "goal ended"), `NotSignedIn`, `NotForAdministrators`, `NotFound`, `AlreadyEnded`, `OutcomeMissing`, `NoteTooLong` | `EndingDate`, `LastDate` |
| `NewAttemptValuesResult` | `Found`, `NotSignedIn`, `NotForAdministrators`, `NotFound`, `NotEnded` | `Values` (`NewAttemptValues`) |
| `AttemptStartResult` | `Started`, `NotSignedIn`, `NotForAdministrators`, `NotFound`, `NotEnded`, the target problems, `StartDateMissing`, `StartDateInPast`, `StartDateNotAfterEnding`, `StartDateTooFar` | `AttemptNumber`, `StartDate`, `Field` on a refusal |

Settled in T060: `SettingsChangeResult`'s outcome enum is `SettingsChangeOutcome` (in `GoalChangeResults.cs`), with
the members in FR-003's check order and the successes last, as `GoalCreateOutcome` has them: `NotSignedIn`,
`NotForAdministrators`, `NotFound`, `GoalEnded`, the target problems from `BuildOrReduceMissing` to `PeriodMissing`,
`ReduceAmountNeedsOneTime`, `ScheduleKindChanged`, `NameTooLong`, `Unchanged`, `Changed`. `Message` is never null: the
refusal's text, or "settings changed" / "settings unchanged". `Field` is set only for the target refusals
(`GoalDraft.FieldOf`), `ScheduleKindChanged` (`Schedule`) and `NameTooLong` (`Name`). `GoalSettingsInput` is a sealed
record with init-only `Target` (default empty) and `Name`, as `GoalInput`.

Settled in T071: `GoalEndResult`'s outcome enum is `GoalEndOutcome` (in `GoalChangeResults.cs`), with the members in
FR-007's check order and the success last: `NotSignedIn`, `NotForAdministrators`, `NotFound`, `AlreadyEnded`,
`OutcomeMissing`, `NoteTooLong`, `Ended`. `Message` is never null: the refusal's text, or "goal ended". `EndingDate` and
`LastDate` (`DateOnly?`) are set only for `Ended`. An `outcome` that is not a defined `Outcome` value gives
`OutcomeMissing`, as a null one does. Neither the result nor a refusal carries the note.

Settled in T079 and T080: `NewAttemptValuesResult`'s outcome enum is `NewAttemptValuesOutcome` and
`AttemptStartResult`'s is `AttemptStartOutcome` (both in `GoalChangeResults.cs`), with the members in FR-011's and
FR-012's check order and the success last: `NotSignedIn`, `NotForAdministrators`, `NotFound`, `NotEnded`, `Found`; and
`NotSignedIn`, `NotForAdministrators`, `NotFound`, `NotEnded`, the target problems from `BuildOrReduceMissing` to
`PeriodMissing`, `ReduceAmountNeedsOneTime`, `StartDateMissing`, `StartDateInPast`, `StartDateNotAfterEnding`,
`StartDateTooFar`, `Started`. `NewAttemptValuesResult.Message` is null for `Found`, as for the other reads, and `Values`
is set only then. `AttemptStartResult.Message` is never null ("attempt started" on success); `AttemptNumber` and
`StartDate` (`int?`, `DateOnly?`) are set only for `Started`, and `Field` (`GoalField?`) only for the target refusals and
the four start date refusals (`GoalDraft.FieldOf`, `StartDate` for the latter). `NewAttemptInput` is a sealed record
with init-only `Target` (default empty) and `StartDate`, as `GoalInput`. `NewAttemptValues` is a sealed record:
`BuildOrReduce`, `Measurement` and `ScheduleKind` are `required` and not nullable (a stored target always has them),
`Amount`, `CountUnit`, `DurationUnit`, `Times` and `Period` are nullable as in `GoalStartingValues`, `Days` defaults to
an empty set, and `StartDate` and `AttemptNumber` are `required`. The values come from
`GoalTimeline.VersionInEffect(goal, latest number, now)`, which for an ended attempt is its last version. The start date
checks live in `GoalChangeService` (no new Domain member, Locked AC code); `GoalService.TextOf` maps
`StartDateNotAfterEnding` to `GoalChangeTexts.StartDateNotAfterEnding`. `GetNewAttemptValuesAsync` reads through
`ReadOwnerGoalAsync` like the writes, so a goal or account gone between the access check and the read gives the same
refusals.

`NewAttemptValues` has:

- the target fields of the ended attempt's last version, as `GoalStartingValues` gives them: `BuildOrReduce`,
  `Measurement`, `Amount`, `CountUnit`, `DurationUnit`, `ScheduleKind`, `Days`, `Times`, `Period`;
- `StartDate`;
- `AttemptNumber`, the number the new attempt will get (HP-004's line).

`GoalChangeTexts` holds the literal texts of FR-003, FR-007, FR-011, FR-012 and FR-014 and the outcome texts
(contracts/application-api.md, "Texts").

### Port members (`IGoalStore`, new)

| Member | Rule |
|---|---|
| `ReadOwnerGoalAsync(AccountId owner, GoalId goal, CancellationToken)` → `OwnerGoal?` | The account's stored zone and goal revision, then that goal, in that order. Null with no account. `OwnerGoal.Goal` is null when the goal is not the owner's. |
| `TryChangeSettingsAsync(AccountId owner, string? expectedStoredZone, int expectedRevision, GoalSettingsWrite write, CancellationToken)` → `GoalChangeWriteOutcome` | Under the account and goal locks: the zone and revision are as read, and the goal's latest attempt is `write.Attempt` and is not ended. Then it writes the name and, when `write.Versions` is set, replaces that attempt's versions, and increments the revision. |
| `TryEndAsync(AccountId owner, string? expectedStoredZone, int expectedRevision, GoalEndingWrite write, CancellationToken)` → `GoalChangeWriteOutcome` | Under the same locks and checks: it writes the ending and replaces the attempt's versions with the kept ones. It deletes the goal's pauses that begin after `write.LastDate`, ends on `write.LastDate` those that cover it, and deletes its excusals after it. Then it increments the revision. |
| `TryStartAttemptAsync(AccountId owner, string? expectedStoredZone, int expectedRevision, GoalAttemptWrite write, CancellationToken)` → `GoalChangeWriteOutcome` | Under the same locks and checks: the latest attempt is ended and numbered `write.Number` − 1. Then it writes the attempt and its first version, and increments the revision. |

The write records and outcomes:

- `GoalSettingsWrite(GoalId Goal, int Attempt, GoalName? Name, IReadOnlyList<GoalSettingsVersion>? Versions)`;
- `GoalEndingWrite(GoalId Goal, int Attempt, GoalEnding Ending, DateOnly LastDate,
  IReadOnlyList<GoalSettingsVersion> KeptVersions)`;
- `GoalAttemptWrite(GoalId Goal, int Number, GoalSettingsVersion First)`;
- `OwnerGoal(string? StoredTimeZone, int GoalsRevision, StoredGoal? Goal)`;
- `GoalChangeWriteOutcome` (enum): `Written`, `Stale` (the zone, revision or precondition no longer holds; nothing
  written), `GoalNotFound`.

### Changed views

| View | Added (init-only) | Rule |
|---|---|---|
| `GoalDetails` | `Later` (`GoalSettingsView?`): all fields of the version after the current date's, with its date | FR-005. `LaterTimeZone` and `LaterTimeZoneAppliesFrom` stay, from the same version. |
| `GoalDetails` | `IsEnded`, `EndingDate`, `LastDate`, `Outcome`, `EndingNote` | FR-017 |
| `GoalDetails` | `ChangesApplyFrom` (`DateOnly?`) | The date a settings change made at the read's instant would apply from; null for an ended goal (HP-002) |
| `GoalDetails` | `AttemptHasBegun` (`bool`) | `LatestAttemptHasBegun` at the read's instant. The page's attempt line ("Attempt n · started" or "· starts") and the Settings card's start row ("Started" or "Starts" with `AttemptStartDate`, as the design shows the attempt's date) read it (HP-001). An ended latest attempt that has not begun (it has no date) shows "Attempt n · ended before it began" instead, and the Settings card then has no start row. `HasStarted` keeps its meaning, "the goal has a current date", which a waiting attempt 2 after a begun attempt 1 also has. |
| `GoalListItem` | `IsEnded`, `EndingDate`, `Outcome` | FR-018 |

`GoalSettingsView` (new sealed record) has the target fields of `GoalDetails` (`BuildOrReduce` to `Period`),
`TargetText`, `ScheduleText`, `TimeZone` and `AppliesFrom`.

Changed meanings, identical for a goal with one attempt:

- `GoalDetails.AttemptNumber` and `AttemptStartDate` are the latest attempt's. Its target fields are those of
  `VersionInEffect`.
- `GoalListItem.StartDate` is the latest attempt's start date while that attempt has not begun and the goal is not
  ended, and null otherwise. In AC-035's case it gives attempt 2's 18 Nov, not attempt 1's 23 Nov. Its target fields
  and `Name` come from `VersionInEffect` and `GoalText.Name(…, instant)`.

`CatalogService` gains the constructor `CatalogService(ICatalogHabitStore, IGoalStore, TimeProvider)`, so the goal list
on a habit names each goal at one time read (research R-08). The two-argument constructor stays.

Settled in T048 and T049:

- `GoalSettingsView` has exactly the fields named above: `BuildOrReduce`, `Measurement`, `Amount`, `CountUnit`,
  `DurationUnit`, `ScheduleKind`, `Days`, `Times`, `Period`, `TargetText`, `ScheduleText`, `TimeZone` and `AppliesFrom`.
  `GoalDetails.Later` is the latest attempt's one version after the settings in effect, and null for an ended goal.
- `GoalDetails.ChangesApplyFrom` is `SettingsChangeOf(goal, now, target, zone).AppliesFrom` with the latest version's
  own target and zone. Those have the attempt's schedule kind, and the date doesn't depend on the target. Nothing is
  written.
- `GoalListItem.StartDate` is the latest attempt's timing date: the goal's start date for a goal with one attempt, as
  in slice 4, and the latest attempt's own start date otherwise.
- A service built with the two-argument `CatalogService` constructor names goals with `GoalText.Name(goal, habitName)`.
  The host registers `CatalogService` through a factory, `ActivatorUtilities.CreateInstance<CatalogService>(provider,
  clock)`. It passes the registered `TimeProvider`, so the three-argument constructor is always used. The stores are
  resolved by that constructor, so the registration file names no store port. That keeps
  `ApplicationDependencyTests`' `IGoalStore` rule as it is.

## Application: `HT.Application.Records` and `HT.Application.Exemptions` (changed; research R-05, R-07)

| Type | Change |
|---|---|
| `HistoryEntry` | + `AttemptNumber` (`int`, default 1 when not set, as for every entry of slices 5 to 7; T043), `IsLatestAttempt` (`bool`, default true), `AttemptName` (`string?`, set only for an attempt that is not the latest, FR-016, FR-019), `Settings` (`EntrySettings?`, init-only, not `required`; always set by `EntryViews.Of`: the settings of the version of the entry's date). For an attempt that is not the latest, `Settings.BuildOrReduce` is "that attempt's Build or Reduce" (FR-016, FR-019, AC-080). |
| `EntrySettings` (new sealed record) | `GoalScheduleKind`, `Period` (`SchedulePeriod?`), `BuildOrReduce`, `Measurement`, `Amount`, `CountUnit`, `DurationUnit`, `Times`, `Target` (`GoalTarget`), with `GoalHistory`'s meaning per field, for one entry (research R-07). Init-only properties, `required` like `GoalHistory`'s for `GoalScheduleKind`, `BuildOrReduce`, `Measurement` and `Target` (T043). An internal `EntrySettings.Of(GoalTarget)` fills them from a version's target as `EntryViews.HistoryOf` fills `GoalHistory`'s. |
| `GoalHistory` | Shape unchanged, plus the method `SettingsOf(HistoryEntry entry)`: `entry.Settings`, or the history's own goal-level settings as an `EntrySettings` when it is null. Its goal-level settings stay for the goal as a whole, and its doc comments drop "every version has the same target". Every view of one entry reads `SettingsOf(entry)`. |
| `RecordPreview` | `Progress(GoalHistory, HistoryEntry, …)` reads `history.SettingsOf(entry)`. New `Progress(EntrySettings settings, HistoryEntry entry, RecordId? replacing, YesNoAnswer? answer, int? amount)`. |
| `RecordingText` | Each member that takes a `GoalHistory` keeps its signature and gets an `EntrySettings` overload: `Recorded`, `TargetLine`, `Progress`, `FieldLabel`, `PartialNote`, `ReduceHelp` and `NoteHelp`. The pages call only the `EntrySettings` overloads, with the settings of the row they show (research R-07). Each `GoalHistory` member delegates to its overload with the history's goal-level settings (it has no entry), so its results are unchanged. |
| `ReportText` | `ProgressText(EntrySettings, HistoryEntry)` (new overload); `ProgressText(GoalHistory, HistoryEntry)` delegates with `history.SettingsOf(entry)`, as `RecordPreview.Progress` does. The slice 8 texts of contracts/host.md "Shared texts" (T042): `OutcomeText(Outcome)`, `Ended(DateOnly, Outcome)` ("Ended Mon 16 Nov · Abandoned"), `Ended(DateOnly)` ("Ended Mon 16 Nov"), `Attempt(int)` ("Attempt 2", the CommitmentRow Tag and the attempt view's eyebrow), `AttemptStarted(int, DateOnly)`, `AttemptStarts(int, DateOnly)`, `AttemptSpan(int, DateOnly, DateOnly)`, `AttemptWithoutDate(int)`, `From(DateOnly)`, `ChangesApplyFrom(DateOnly)`, `NewAttemptLine(int)`, and the constants `NoEndedGoals`, `NoEndedGoalsYet` and `EndedTag` ("Ended"). |
| `RecordingService.ChangeRecordAsync`, `DeleteRecordAsync` | A record whose date is in no attempt gives `NothingScheduled`, "Nothing is scheduled on this date.", and nothing changes. `StoredEntryOf` and `NoteEntryOf` are removed with their exceptions. |
| `IRecordStore` | + `ReadRecentNoteKeysAsync(AccountId owner, GoalId goal, DateOnly from, DateOnly? through, int count, CancellationToken)`: the newest `count` note keys from `from` through `through` (no upper bound when null). `GetRecentNotesAsync` passes the latest attempt's first entry's first date and, once it is ended, its last entry's last date, and skips a key with no entry. An attempt with no entry (no date) gives an empty list without a store call. |
| `ExemptionStoreWrite` | + outcome `GoalEnded` (appended last to `ExemptionStoreWriteOutcome`, so the existing values keep their numbers) and its static `ExemptionStoreWrite.GoalEnded`, returned by `AddPauseAsync` when the goal is ended under the lock. The ending is re-checked before the overlap (T039). |
| `PauseService.PauseAsync` | new outcome `GoalEnded`, "This goal has ended.", after step (3), before anything else is read; a store `GoalEnded` gives the same result. Settled in T050: `GoalEnded` is appended last to `PauseOutcome`, so the existing values keep their numbers. |
| `ExcusalService.ExcuseAsync` | step 4 uses the schedule kind of the attempt that contains the date. Settled in T050: step 4 reads the kind through `GoalTimeline.VersionOf`, which gives the version of the attempt whose span contains the date. A date after an ended attempt's last date and before the next attempt's start date (or after an ended latest attempt's last date) is in no attempt. It skips step 4 and gets step 5's "Nothing is scheduled on this date.". A date before every attempt keeps slice 7's result. |

## Application: `HT.Application.Reports` (research R-06, R-07)

### `AttemptService` views

`AttemptView` (sealed record) has:

- `Number`, `Name` (FR-004), `StartDate`, `IsLatest`;
- `EndingDate`, `LastDate`, `Outcome`, `EndingNote`, all null while not ended;
- `HasNoDate`: true when the attempt has no commitment or period, i.e. its last date is before its first entry (HP-001's
  "ended before it began"; research R-02, plan "Spec discrepancies");
- `Stats` (`GoalStatsView`): consistency, current and longest streak with unit, over that attempt up to the supplied
  time.

`AttemptDetails` (sealed record) has:

- `Attempt` (`AttemptView`);
- `Recent` (`RecentResults`, slice 6 FR-005 with slice 7 FR-015);
- `History` (`GoalHistory`): the attempt's dates from its start date to its last date, or to the current date while it
  is not ended, newest first, at most 366. Its goal-level fields come from `VersionInEffect(goal, number, now)` of
  that attempt, and its rows carry their own `Settings`.

`EndedAttemptView` (sealed record, one entry of the goal history) has:

- `GoalId`, `Name` (FR-004), `AttemptNumber`;
- `EndingDate`, `Outcome`, `EndingNote`;
- `IsLatestAttempt`.

Results:

| Result | Outcomes | Fields |
|---|---|---|
| `AttemptListResult` | `Found`, `NotSignedIn`, `NotForAdministrators`, `NotFound` | `Attempts`, latest first |
| `AttemptReadResult` | `Found`, `NotSignedIn`, `NotForAdministrators`, `NotFound`, `AttemptNotFound` | `Details` |
| `GoalHistoryListResult` | `Found`, `NotSignedIn`, `NotForAdministrators` | `Entries`: latest ending date first, then name (`HabitName.KeyOf`, ordinal), then attempt number |

Settled in T081 and T082: the outcome enums are `AttemptListOutcome` (`NotSignedIn`, `NotForAdministrators`,
`NotFound`, `Found`) and `AttemptReadOutcome` (`NotSignedIn`, `NotForAdministrators`, `NotFound`, `AttemptNotFound`,
`Found`), in `src/HT.Application/Reports/AttemptResults.cs`, in the check order with the success last, as
`GoalReportOutcome`. `Message` is null for `Found`; `AttemptListResult.Attempts` is empty and
`AttemptReadResult.Details` null for a refusal. `AttemptView` and `AttemptDetails` are sealed records with init-only
properties; the non-nullable ones are `required`. `AttemptView.Outcome` is `Outcome?` and `EndingNote` the trimmed text
(`string?`). `AttemptDetails.Recent` holds the attempt's latest 30 ended results (the goal report's count). Its
`History` holds the evaluated entries (first entry through the earlier of the last date and the goal's current date),
newest first; when that range has more than 366 dates, it keeps the entries that overlap its newest 366 dates (from
the end date − 365 to the end date, the end date being the last date, or the current date while not ended), as slice
5's 366-day range keeps the weeks that overlap it. A number that is not one of the goal's attempts (0 and negative
numbers included) gives `AttemptNotFound`. `GetGoalHistoryAsync` and `EndedAttemptView` come with US4.

### Internal: `GoalEvaluation` (changed)

- `OfAsync(records, owner, stored, now, ct)` evaluates the latest attempt (the report range, FR-013).
- New `OfAsync(records, owner, stored, attemptNumber, now, ct)` evaluates the named attempt, through the earlier of its
  last date and the goal's current date.
- New `EarlierPending`: the Pending entries of ended attempts other than the latest whose grace period is open at
  `now`. Only attempts whose last entry ended within the last 48 hours are evaluated.
- New `Recordable` entries: its own Due and Pending entries plus `EarlierPending`, in slice 6's order. The goal-level
  `Recordable` view (`GoalHistory`) carries them. `OverviewGoal.Recordable`, `HasPending` and `CanRecord` and the
  Pending group of `GoalOrder` read them, so the overview's `Recordable` stays equal to `GetRecordableAsync` at the
  same instant (research R-07). The figures, results and strip stay the latest attempt's.
- The target an evaluation is built with (`LatestTarget`, from which `Unit`, `BuildOrReduce` and the trend's kind and
  span follow) is `VersionInEffect(goal, number, now)` of the evaluated attempt. So an earlier attempt keeps its own
  streak unit and Build or Reduce (FR-006, FR-014, US3-AS5). `Name` is `GoalText.Name(goal, habitName, now)` for the
  latest attempt and `GoalText.AttemptName` for an earlier one.
- The trend draws each bar or week in its kind. An entry of another measurement contributes only its result: one
  "Yes" when Met in commitment weeks, and no value in a value trend (research R-07).
- The records come from one `ReadAsync` over the range the evaluated entries span.

Settled in T044 to T047 (all internal, so no public surface changes):

- `GoalEvaluation` exposes `AttemptNumber`, `IsLatestAttempt`, `EarlierPending`, `RecordableEntries` (the entries of
  `Recordable`, newest first) and `HasPending` (a Pending entry of its own or in `EarlierPending`). `Recordable` is
  built once. The overload with an attempt number gives an empty `EarlierPending`, so the attempt list stays at one
  record read per attempt.
- The current week's Upcoming commitments feed the trend only for the latest attempt while it runs (begun and not
  ended). An ended attempt never gets a later attempt's commitments as Upcoming. A goal with one attempt gives slice 7's
  trend. `NextUpcoming` gives nothing for an ended goal or an earlier attempt's evaluation.
- The internal `HT.Application.Records.AttemptEntries` keeps, in one place, the attempt facts the read path derives from
  `GoalTimeline`'s public members:
  - the attempt of an entry (`AttemptOf`, skipped for a goal with one attempt);
  - an attempt's timing date (its first version's date);
  - its first and last entries (`EntriesThrough(goal, n, last date)` when it is ended, its first 14 days otherwise);
  - "no date" (ended with no entry through its last date);
  - the `EarlierPending` candidates: the entries with an open grace period of every ended attempt but the latest
    whose last entry's grace period is open.

  `GoalEvaluation` and `GetRecordableAsync` share those candidates. So the overview's `Recordable` equals
  `GetRecordableAsync` also for earlier entries. `GetRecordableAsync` keeps its 9-day window for the latest attempt
  and reads the candidates' records in the same one read.
- `EntryViews.Of` takes the `StoredGoal`, because an earlier attempt's entry needs the habit's current name
  (`GoalText.AttemptName`).
- `GetHistoryAsync` and `GetRecordableAsync` keep the latest attempt's entries of their range as those from its timing
  date on. Every earlier attempt ends before the latest one starts.
- The dashboard also leaves ended goals out of the Due list and the Upcoming entries, besides the Missed ones.
  `TodayEntry.GoalEnded` is the goal's `IsEnded`. An earlier attempt's row of an ended goal is marked by both, and the
  row shows "Attempt n" first (contracts/host.md).

### Changed views

| View | Change |
|---|---|
| `TodayEntry` | + `GoalEnded` (`bool`). For an entry of an attempt that is not the latest, `GoalName` is that attempt's name, and `BuildOrReduce` and `ScheduleText` come from `Goal.SettingsOf(Entry)` (FR-019). `Goal` stays the goal's `GoalHistory`, but the row and its RecordDialog read `Goal.SettingsOf(Entry)` for the target line, field, unit, help texts, state words and preview (research R-07). |
| `DashboardCounts.Goals` | counts goals that are neither paused nor ended. |
| `DashboardCounts.Pending` | includes `EarlierPending` and an ended goal's Pending entries. |
| `DashboardCounts.MissedThisWeek` | leaves out ended attempts' Missed entries. |
| `Dashboard.Goals`, `Dashboard.Notes` | leave out ended goals. |
| `OverviewGoal` | through `Item`'s new fields; an ended goal has `PausedUntil` null and no Due entry. `HasPending`, `CanRecord` and `Recordable` include the goal's earlier Pending entries (research R-07). |

## Application: demo goal-change loading (`HT.Application.Operator`, research R-09)

- `DemoGoalChangeService(IDemoGoalChangeStore demoGoalChange, IAccountStore accounts, TimeProvider clock,
  InstallationTimeZone defaultZone)`, mirroring `DemoPauseService`. `LoadAsync(string? demoPassword,
  CancellationToken)` → `DemoGoalChangeLoadingResult`. The run date R is the date of the supplied time, read once, in
  the default zone.
- `DemoGoalChangeLoadingResult` and `DemoGoalChangeAccountResult` mirror slice 7's results: "no demo password
  configured", "demo password does not meet the password rule", "created" or "already exists", "demo goals added",
  "demo records added".
- `DemoGoalChanges.For(DateOnly runDate)` → the three `DemoGoalChangeGoal`s of FR-031.
- `DemoGoalChangeGoal` has:
  - the catalog habit name;
  - the accepted habit states (Published);
  - its `DemoAttempt`s;
  - its records and notes (slice 6's `DemoRecord` and `DemoNote`).
- `DemoAttempt` has its number, its versions (`DemoVersion`: applies-from date and target fields), and an optional
  ending (date, outcome, note).
- `IDemoGoalChangeStore.CreateDemoGoalChangeAccountAsync(Email email, string password, DemoAccountProfile profile,
  IReadOnlyList<DemoGoalChangeGoal> goals, CancellationToken)` → `CreateDemoAccountOutcome`. It writes everything in
  one transaction, as `IDemoPauseStore` does.

## State transitions of an attempt

| From | Action | To | Stored change |
|---|---|---|---|
| — | goal created (slice 4) | attempt 1, not ended | attempt row, version 1 |
| not ended | settings change (FR-002) | not ended | versions of the attempt replaced (first replaced, later replaced, or one added); or the name only |
| not ended | time zone change (slice 4 FR-026, FR-021) | not ended | as slice 4, on the latest attempt |
| not ended | end (FR-007 to FR-009) | ended | ending date, outcome, note; versions after L removed except the first; pauses cut; excusals after L removed |
| ended | time zone change | ended | nothing (FR-021) |
| ended | start new attempt (FR-012) | ended, and a new attempt n + 1, not ended | attempt row n + 1, its version 1 from its start date, in the account's zone |
| ended | change settings, pause, end | ended | nothing; "This goal has ended." or "This goal has already ended." |

An ending cannot be undone (Assumptions). Records, notes and the dates of an ended attempt keep their states and stay
correctable (FR-010).

## Which attempt a date belongs to

For attempt n with start date S, schedule kind K and, once ended, ending date E:

- its first entry is S's commitment or day, or for a per-week attempt the week of the first Monday on or after S;
- its last date L is E − 1 (commitment-based or per day) or the Sunday before E's week (per week), and nothing while it
  is not ended;
- its entries run from the first entry to L.

A date in no attempt's entries belongs to none. It gives "Nothing is scheduled on this date." for a record, appears in
no History or calendar, and is never counted (FR-010, FR-013, FR-016). Attempt n + 1 starts after E, so no date
belongs to two attempts.
