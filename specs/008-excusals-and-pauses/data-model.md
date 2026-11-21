# Data model: Excusals and pauses

Phase 1 of [plan.md](plan.md). Excusals and pauses are stored facts (FR-025). Excused and Paused, and every figure,
are derived on read (ADR 0018, ADR 0020). Research decisions are cited as R-NN ([research.md](research.md)).

## Tables (migration `ExcusalsAndPauses`)

| Table | Column | Type | Rule |
|---|---|---|---|
| `GoalExcusals` | `GoalId` | uniqueidentifier, FK `Goals`, cascade | part of the key |
| | `Date` | date | the commitment's date; part of the key, so a commitment has at most one excusal |
| | `Reason` | nvarchar(200), null | trimmed; null means none |
| `GoalPauses` | `Id` | uniqueidentifier, key | made by the store |
| | `GoalId` | uniqueidentifier, FK `Goals`, cascade | index `IX_GoalPauses_GoalId_FirstDay` on `(GoalId, FirstDay)` |
| | `FirstDay` | date | ≤ `LastDay` |
| | `LastDay` | date | |
| | `Reason` | nvarchar(200), null | trimmed; null means none |

No column stores when the fact was made or a derived state (FR-025). Pauses of one goal never share a date: the
store re-checks this under the goal-row lock (R-04). Deleting an account cascades through its goals, as for records
and notes.

Existing tables are unchanged. The demo pause loading writes one account with its goals, versions, attempts,
records, notes, excusals and pauses into these tables in one transaction (R-08).

## Domain: `HT.Domain.Evaluation` (changed)

| Type | Change |
|---|---|
| `EvaluationState` | + `Excused`, `Paused` after `Missed` (R-01). The doc comment: the evaluation state of an entry, or the exemption in its place. Only `Upcoming` to `Missed` are evaluation states in `CONTEXT.md`'s sense |
| `Evaluator.Evaluate(target, starts, ends, records, now, EvaluationState? exemption)` | new overload. With `Excused` or `Paused`: `State` is the exemption, `Progress` comes from the records, `GraceEnds` is computed as before, `IsFinal` is false. Any other non-null value throws `ArgumentOutOfRangeException`. The existing 5-argument overload calls it with null and is unchanged |

## Domain: `HT.Domain.Exemptions` (new, R-02)

### `ExemptionReason` (sealed class)

| Member | Rule |
|---|---|
| `Value` | the trimmed text, 1 to 200 UTF-16 code units (`ExemptionLimits.MaxReasonLength` = 200) |
| `TryCreate(string? text, out ExemptionReason? reason, out bool tooLong)` | null, empty or blank gives `true` and no reason; more than 200 after trimming gives `false` and `tooLong` |

Equality is ordinal. `ToString` is not overridden (HP-009).

### `ExemptionLimits` (static class)

`const int MaxReasonLength = 200`: the longest reason of an excusal or a pause, in UTF-16 code units after trimming
(FR-001), which `ExemptionReason.TryCreate` checks.

### `Excusal` (sealed record)

`Excusal(DateOnly Date, ExemptionReason? Reason)`. It is the stored fact of FR-001, with no time of its own (FR-003,
FR-025).

### `PauseId` (readonly record struct) and `Pause` (sealed record)

- `PauseId(Guid Value)`. The store makes it, and a caller never does. `ToString` gives the Guid's text, as
  `GoalId`'s and `RecordId`'s do.
- `Pause(PauseId Id, DateOnly FirstDay, DateOnly LastDay, ExemptionReason? Reason)`. The constructor throws when
  `LastDay < FirstDay`.

| Member | Rule |
|---|---|
| `Covers(DateOnly date)` | `FirstDay ≤ date ≤ LastDay` |
| `Overlaps(DateOnly first, DateOnly last)` | `FirstDay ≤ last && first ≤ LastDay`; touching ranges do not overlap (Edge Cases) |
| `TimingAt(DateOnly today)` | `Upcoming` when `FirstDay > today`, `Past` when `LastDay < today`, else `Current` (FR-012) |

### `PauseTiming` (enum)

`Upcoming`, `Current`, `Past`.

### `GoalExemptions` (sealed class)

One goal's excusals and pauses, as `ReadExemptionsAsync` reads them. `GoalExemptions.None` is empty.

| Member | Rule |
|---|---|
| `GoalExemptions(IEnumerable<Excusal> excusals, IEnumerable<Pause> pauses)` | the constructor (T014), in any order; throws `ArgumentException` for two excusals of one date or two pauses that share a date, which the store never holds (R-04) |
| `Excusals` | by date |
| `Pauses` | latest `FirstDay` first (FR-012) |
| `IsPaused(DateOnly date)` | some pause covers the date |
| `IsExcused(DateOnly date)` / `ExcusalOf(DateOnly date)` | the excusal of the date, or none |
| `ExemptionOf(CalendarEntry entry, GoalScheduleKind kind)` | `Paused` when some date from `entry.FirstDate` to `entry.LastDate` is paused (a week: any of its 7 dates, D-1); else `Excused` when `kind` is commitment-based and the date is excused; else null. So a pause wins over an excusal of the same date, and the excusal is kept (FR-007) |
| `CurrentPause(DateOnly today)` | the pause whose `TimingAt(today)` is `Current`, or null; at most one, since pauses do not overlap |
| `Overlaps(DateOnly first, DateOnly last)` | some pause overlaps the range |

### `PauseRules` (static class)

| Member | Rule |
|---|---|
| `Check(DateOnly? first, DateOnly? last, DateOnly today, GoalExemptions exemptions)` | FR-006 steps 4 to 7, in order: `DaysMissing`; `StartsBeforeToday` (`first < today`); `LastBeforeFirst`; `Overlaps`; or null |
| `Resume(Pause current, DateOnly today)` | `PauseResumption.Remove` when `current.FirstDay == today`, else `PauseResumption.EndOn(today − 1)` (FR-010). Requires `current.TimingAt(today) == Current` |

`PauseProblem` (enum): `DaysMissing`, `StartsBeforeToday`, `LastBeforeFirst`, `Overlaps`.
`PauseResumption` (sealed record): `bool Removes`, `DateOnly? NewLastDay`; the static members `Remove` (`Removes`
true, no new last day) and `EndOn(DateOnly lastDay)` that `Resume` returns (T015). `Resume` of a pause that is not
current on `today` throws `ArgumentException`.

## Domain: `HT.Domain.Reports` (changed)

| Type | Change |
|---|---|
| `Consistency.Of` | `Excused` and `Paused` are skipped like Pending (FR-013) |
| `Streaks.Of` | `Excused` and `Paused` are skipped: they neither extend nor reset (FR-014) |
| `TrendMark` (new enum) | `Excused`, `Paused` |
| `TrendBar` | + `TrendMark? Mark { get; init; }`. A marked bar has a null `Value` and keeps its own `Target` (R-07) |
| `TrendBuilder.Of(GoalTarget latest, IReadOnlyList<(CalendarEntry Entry, GoalTarget Target, IReadOnlyList<RecordValue> Records, EvaluationState? Exemption)> begun, IReadOnlyList<CalendarEntry> currentWeekUpcoming, GoalExemptions exemptions)` | new overload (R-07), final signature (T019). Each begun item carries its own exemption (Excused, Paused or null, as `GoalExemptions.ExemptionOf` gives it; any other state throws `ArgumentOutOfRangeException`); the exemption of each of `currentWeekUpcoming` comes from `exemptions` (`ExemptionOf(entry, CommitmentBased)`), so a Paused Upcoming commitment leaves the week's Build target and counts towards an all-exempt week. `exemptions` is a fourth parameter, not a third list carrying the exemption, because a 3-parameter overload makes the existing calls `TrendBuilder.Of(target, [], [])` ambiguous. Exemption marks; for commitment-based yes/no weeks, only commitments that are neither Excused nor Paused count in the Build target and in the value; a week with no record on a counted commitment has no value; a week whose commitments, the current week's Upcoming ones included, are all Excused or Paused is marked Paused if one is Paused, else Excused, with target 0. A marked bar's target follows the same rules as an unmarked one. `Trend.Target` is the latest unmarked bar's target (0 when none). The old overload calls it with no exemptions |

## Application: `HT.Application.Exemptions` (new, R-05)

### Services

`ExcusalService` and `PauseService`: see [contracts/application-api.md](contracts/application-api.md).

### Results and views

| Type | Members |
|---|---|
| `ExcuseOutcome` (enum) | `NotSignedIn`, `NotForAdministrators`, `NotFound`, `NotACommitment`, `NothingScheduled`, `DatePaused`, `AlreadyExcused`, `NotBegun`, `Final`, `HasRecord`, `ReasonTooLong`, `Excused` |
| `ExcuseResult` | `Outcome`, `Message` (refusals), `DateOnly? Date` (`Excused`) |
| `RemoveExcusalOutcome` (enum) | `NotSignedIn`, `NotForAdministrators`, `NotFound`, `NotExcused`, `TooLate`, `Removed` |
| `RemoveExcusalResult` | `Outcome`, `Message`, `EvaluationState? State` (the state after the change: Due, Pending or Paused) |
| `PauseOutcome` (enum) | `NotSignedIn`, `NotForAdministrators`, `NotFound`, `DaysMissing`, `StartsBeforeToday`, `LastBeforeFirst`, `Overlaps`, `ReasonTooLong`, `Paused` |
| `PauseResult` | `Outcome`, `Message`, `PauseView? Pause` (`Paused`) |
| `ResumeOutcome` (enum) | `NotSignedIn`, `NotForAdministrators`, `NotFound`, `NotPaused`, `Resumed` |
| `ResumeResult` | `Outcome`, `Message` |
| `CancelPauseOutcome` (enum) | `NotSignedIn`, `NotForAdministrators`, `NotFound`, `PauseNotFound`, `HasBegun`, `Cancelled` |
| `CancelPauseResult` | `Outcome`, `Message` |
| `PausesOutcome` (enum) | `NotSignedIn`, `NotForAdministrators`, `NotFound`, `Found` |
| `PausesResult` | `Outcome`, `Message` (null for `Found`), `IReadOnlyList<PauseView> Pauses` (empty on a refusal), `DateOnly? PauseDate` (added in T055: the pause date of the read, R-03, set only for `Found`; the goal page gives it to the PauseDialog as the default and `min` of "From", HP-002, as the page has no other read of the account's today for a goal that has not started) |
| `PauseView` (sealed record) | `PauseId Id`, `DateOnly FirstDay`, `DateOnly LastDay`, `string? Reason`, `PauseTiming Timing` (all `init`; every one but `Reason` `required`), `string RangeText` ("Mon 16 Nov – Sun 22 Nov", HP-001; a computed property, `DateTexts.ShortDate` of both days around " – ", T055) |

`PauseResult`, `ResumeResult`, `CancelPauseResult` and `PausesResult` follow the same pattern, with their outcome enums
and `PauseView`, in `src/HT.Application/Exemptions/PauseResults.cs` (T055); the first three carry the success word as
`Message`.

A refusal carries no input value (HP-009). `ExcuseResult` and `RemoveExcusalResult` are sealed classes with a private
constructor and internal factories, on the pattern of slice 5's results, both in `src/HT.Application/Exemptions/ExcusalResults.cs`
with their outcome enums (T044); their `Message` is never null, as it carries the success word too. The texts come from
`ExcusalTexts` and `PauseTexts`, word for word from the spec:

| Text | Used by |
|---|---|
| "Only commitments can be excused. This goal is evaluated by period." | FR-002 (4) |
| "This goal is paused on this date." | FR-002 (6), FR-005 (4a), FR-009 |
| "This commitment is already excused." | FR-002 (7) |
| "You cannot excuse a commitment that has not begun." | FR-002 (8) |
| "This commitment is final and can no longer be excused." | FR-002 (8) |
| "This commitment has a record. Delete the record to excuse it." | FR-002 (9) |
| "Keep the reason to 200 characters or fewer." | FR-002 (10), FR-006 (8) |
| "excused", "excusal removed", "paused", "resumed", "pause cancelled" | the success results |
| "This commitment is not excused." | FR-004 (4) |
| "This excusal can no longer be removed." | FR-004 (5) |
| "Choose the first and the last day." | FR-006 (4) |
| "A pause cannot start before today." | FR-006 (5) |
| "The last day cannot be before the first day." | FR-006 (6) |
| "This goal already has a pause on these dates." | FR-006 (7) |
| "This goal is not paused." | FR-010 (4) |
| "This pause does not exist." | FR-011 (4) |
| "Only a pause that has not begun can be cancelled. Resume the goal instead." | FR-011 (5) |
| "This week is paused." | FR-005 (4b), FR-009 |
| "This commitment is excused. Remove the excusal to record it." | FR-005 (4c) |

"Nothing is scheduled on this date." stays in `RecordTexts`. The success words are carried as `Message` on the
success results too, as slice 5's results do.

## Application: changed types of slices 5 and 6 (R-06)

| Type | Change |
|---|---|
| `IRecordStore` | + `ReadExemptionsAsync`, `ExcuseAsync`, `RemoveExcusalAsync`, `AddPauseAsync`, `SetPauseLastDayAsync`, `DeletePauseAsync` (R-04; [contracts](contracts/application-api.md#irecordstore-new-members)) |
| `RecordStoreWriteOutcome` | + `Exempt`: `AddAsync` found an excusal or a pause on the entry under the lock. Added last, after `Written`, so the earlier members keep their values (T028) |
| `RecordStoreWrite` | + the static property `RecordStoreWrite.Exempt` (outcome `Exempt`, no record, no entry), like `GoalNotFound`; needed because the constructor is private (T028) |
| `ExemptionStoreWrite` (new, `HT.Application.Records`, T028) | sealed class with a private constructor, on the pattern of `RecordStoreWrite`: `ExemptionStoreWriteOutcome Outcome` (`GoalNotFound`, `AlreadyExcused`, `HasRecord`, `Paused`, `NotExcused`, `Overlaps`, `PauseNotFound`, `Written`) and `PauseId? PauseId` (set only for a `Written` of `AddPauseAsync`); one static property per refusal (`GoalNotFound`, `AlreadyExcused`, `HasRecord`, `Paused`, `NotExcused`, `Overlaps`, `PauseNotFound`), and the factories `Written()` (excusal added or removed, pause changed or deleted) and `Written(PauseId)` (pause added). It lives beside `IRecordStore` in `src/HT.Application/Records/`, as T028 names it (plan.md's tree lists it under `Exemptions/`) |
| `ExcusalTexts`, `PauseTexts` (new, `HT.Application.Exemptions`, T032) | the texts of the table above as `const string` members. `ExcusalTexts`: `NotACommitment`, `AlreadyExcused`, `NotBegun`, `Final`, `HasRecord`, `ReasonTooLong`, `NotExcused`, `TooLateToRemove`, `CommitmentExcused`, and the success words `Excused` ("excused") and `Removed` ("excusal removed"). `PauseTexts`: `DatePaused`, `WeekPaused`, `DaysMissing`, `StartsBeforeToday`, `LastBeforeFirst`, `Overlaps`, `ReasonTooLong` (defined as `ExcusalTexts.ReasonTooLong`, so the text is typed once), `NotPaused`, `PauseNotFound`, `HasBegun`, and the success words `Paused` ("paused"), `Resumed` ("resumed") and `Cancelled` ("pause cancelled") |
| `RecordWriteOutcome` | + `DatePaused`, `WeekPaused`, `CommitmentExcused` (FR-005 steps 4a to 4c). Added last, after `Deleted`, so the earlier members keep their values, as `RecordStoreWriteOutcome.Exempt` is (T034) |
| `HistoryEntry` | `State` may be `Excused` or `Paused`; then `GraceEnds` is null and `IsCorrection` false. + `string? ExcusalReason` and `bool CanRemoveExcusal` (both only for Excused; FR-017) |
| `ResultView` | `State` may be `Excused` or `Paused` (FR-015) |
| `RecentResults.Counts` | + keys `Excused` and `Paused`, only when above 0, so a goal without exemptions keeps slice 6's four keys (FR-023) |
| `RecentNote`, `DashboardNote` | `State` may be `Excused` or `Paused` (FR-020) |
| `OverviewGoal` | + `DateOnly? PausedUntil`: the last day of the current pause at the pause date (R-03), null when not paused (FR-018) |
| `DashboardGoal` | + `DateOnly? PausedUntil` (FR-019) |
| `DashboardCounts.Goals` | counts only goals with no `PausedUntil` (FR-019) |
| `ReportText` | + `PausedUntil(DateOnly)` ("Paused until Sun 22 Nov"), the state words "Excused" and "Paused" (`State`, through `RecordingText.State`), strip labels with "excused" and "paused" (`StripLabel`, order met, failed, missed, excused, paused, pending; HP-006), trend labels and bar titles for marked bars (`TrendLabel`: "…; target {n}; {a} excused, {b} paused", a count of 0 left out, marked bars never in the list of differing targets; `BarTitle`: "{date}: Excused" or "{date}: Paused"; `design/components/TrendChart.md`, "Slice 7 state"), the Goals footnote as `const string GoalsFootnote` (HP-004), "No paused goals." as `const string NoPausedGoals` (T035) |
| `ApplicationServiceCollectionExtensions` | + `ExcusalService`, `PauseService`, `DemoPauseService` |

Unchanged: every existing constructor, public method signature and positional record of slices 1 to 6 (FR-023).

## Application: demo pause loading (`HT.Application.Operator`, R-08)

| Type | Members |
|---|---|
| `DemoPauseService` | `const string DemoEmail = "demo6@example.com"`; the constructor `DemoPauseService(IDemoPauseStore demoPause, IAccountStore accounts, TimeProvider clock, InstallationTimeZone defaultZone)` ([contracts](contracts/application-api.md#demopauseservice-htapplicationoperator-fr-031)); `LoadAsync(string? demoPassword, CancellationToken)` |
| `DemoPauses` (static) | `For(DateOnly runDate)`: FR-031's two goals, Meditation then Drinking water; `GoalNames` ("Meditation, 10 min daily", "Drinking water, 8 glasses a day"); the reason and note constants `TravellingReason` ("Travelling."), `IllnessReason` ("Ill with flu."), `FamilyVisitReason` ("Family visit."), `OversleptNote` ("Overslept.") and `BusyDayNote` ("Busy day.") (T084) |
| `DemoExcusal` (sealed record) | `DateOnly Date`, `ExemptionReason? Reason` (positional) |
| `DemoGoalPause` (sealed record) | `DateOnly FirstDay`, `DateOnly LastDay`, `ExemptionReason? Reason`; its constructor `(firstDay, lastDay, reason)` throws `ArgumentException` when `lastDay < firstDay` (T084) |
| `DemoPauseGoal` (sealed class) | `DemoHistoryGoal History`, `IReadOnlyList<DemoExcusal> Excusals`, `IReadOnlyList<DemoGoalPause> Pauses`; its constructor `(history, excusals, pauses)` throws `ArgumentException` for a null excusal or pause, as `DemoHistoryGoal`'s does for a null record or note (T084), and for two excusals of the same date or two pauses that share a date, which `GoalExemptions` would refuse on every read of the goal (PR 44 review) |
| `IDemoPauseStore` | `CreateDemoPauseAccountAsync(Email email, string password, DemoAccountProfile profile, IReadOnlyList<DemoPauseGoal> goals, CancellationToken)` → `CreateDemoAccountOutcome` |
| `DemoPauseLoadingResult`, `DemoPauseAccountResult` | as slice 6's `DemoReportLoadingResult` and `DemoReportAccountResult` |
| `DemoPauseLoadingOutcome` (enum, T085) | `NoPassword`, `PasswordRuleNotMet`, `Loaded`, as slice 6's `DemoReportLoadingOutcome`; `DemoPauseLoadingResult.Outcome`, which the host switches on (T091) |

Persistence (internal, T087): `DemoPauseStore` implements `IDemoPauseStore` over slice 6's `DemoAccountWriter`, whose new
member `CreateWithExemptionsAsync` writes each written goal's excusals (one save each, so a second excusal on a date is
a primary-key fault) and then its pauses after its records and notes, in the same transaction, with the fault prefix
"Demo pause account creation failed". `DemoAccountWriter.CreateAsync` is unchanged for the other two stores.

## State derivation per entry (FR-003, FR-007, FR-009)

At a supplied time `now`, for one entry of a goal with exemptions `X`:

1. `exemption = X.ExemptionOf(entry, kind)`.
2. With an exemption: the state is that exemption, with no grace end and no correction; the records and progress are
   kept.
3. Without: slice 5's `Evaluator.Evaluate` gives Upcoming, Due, Pending, Met, Failed or Missed.

An entry is an *exempt result* when its state is Excused or Paused and `entry.Ends ≤ now`. Exempt results are listed
among recent results, and never counted in consistency or streaks.

Transitions come only from the user's facts, never from time alone:

| From | Event | To |
|---|---|---|
| Due or Pending, with no record, not paused | excuse (FR-002) | Excused |
| Excused, grace period open | remove the excusal (FR-004) | Due or Pending, or Paused if a pause covers it |
| any entry whose date the pause covers (a week: any of its dates) | a pause from the pause date on (FR-006, FR-008) | Paused |
| Paused, on the current date | resume (FR-010) | as evaluated without the pause, except a week that keeps a paused date |
| Paused by an upcoming pause (any commitment or day on its dates, and any week with one of its dates, D-1) | cancel the pause (FR-011) | as evaluated without it: Upcoming, or Due for the current week |
