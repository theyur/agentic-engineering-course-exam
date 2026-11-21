# Contract: Application API of excusals and pauses

The public surface this slice adds to `HT.Application` and `HT.Domain`, which the acceptance tests and the pages
call. Types are in [data-model.md](../data-model.md). Existing constructors, method signatures and positional records
of slices 1 to 6 are unchanged (FR-023). What they now give for excused or paused entries is listed under "Changed
reads".

## Common rules

- **Access** (FR-021, FR-022), checked first and in this order:
  1. no account id → "You are not signed in.";
  2. role is not registered user → "Goals are for registered users.";
  3. account not stored → "You are not signed in.";
  4. the goal is not among the owner's goals → "This goal does not exist.".

  A refusal returns no view and changes nothing.
- **Time**: `TimeProvider.GetUtcNow()`, read once per call.
- **Pause date** (R-03): the goal's current date at that instant, or the account's today when the goal has not
  started (stored zone, else the installation default).
- **Owner**: every store call passes the acting user's account and the goal named in the call. A pause is looked up
  only among that goal's pauses.
- **Logging**: none. No result, exception or log names a reason, note, value, goal or habit name (HP-009).

## `ExcusalService` (`HT.Application.Exemptions`)

```csharp
public sealed class ExcusalService
{
    public ExcusalService(IRecordStore records, IGoalStore goals, IAccountStore accounts, TimeProvider clock);

    public Task<ExcuseResult> ExcuseAsync(
        ActingUser actingUser, GoalId goal, DateOnly date, string? reason, CancellationToken cancellationToken);
    public Task<RemoveExcusalResult> RemoveExcusalAsync(
        ActingUser actingUser, GoalId goal, DateOnly date, CancellationToken cancellationToken);
}
```

### `ExcuseAsync` (FR-001 to FR-003)

After access, checking in this order:

| Step | Check | Outcome, text |
|---|---|---|
| 4 | the goal is period-based (its versions differ only in their time zone, so every version has the same schedule kind) | `NotACommitment`, "Only commitments can be excused. This goal is evaluated by period." |
| 5 | `GoalTimeline.EntryOf(goal, date)` is null | `NothingScheduled`, "Nothing is scheduled on this date." |
| 6 | `exemptions.IsPaused(date)` | `DatePaused`, "This goal is paused on this date." |
| 7 | `exemptions.IsExcused(date)` | `AlreadyExcused`, "This commitment is already excused." |
| 8 | the state without exemption is Upcoming / Met, Failed or Missed | `NotBegun`, "You cannot excuse a commitment that has not begun." / `Final`, "This commitment is final and can no longer be excused." |
| 9 | the commitment has a record | `HasRecord`, "This commitment has a record. Delete the record to excuse it." |
| 10 | the reason is over 200 characters after trimming | `ReasonTooLong`, "Keep the reason to 200 characters or fewer." |
| — | `IRecordStore.ExcuseAsync` | `Excused`, "excused", with `Date` |

If the store reports `AlreadyExcused`, `HasRecord` or `Paused` (a concurrent write), the use case gives step 7's,
step 9's or step 6's refusal. `GoalNotFound` gives "This goal does not exist.".

### `RemoveExcusalAsync` (FR-004)

| Step | Check | Outcome, text |
|---|---|---|
| 4 | no excusal of `date` | `NotExcused`, "This commitment is not excused." |
| 5 | `now >= GracePeriod.EndOf(entry.Ends)` | `TooLate`, "This excusal can no longer be removed." |
| — | `IRecordStore.RemoveExcusalAsync` | `Removed`, "excusal removed", with `State` evaluated after the change (Due, Pending, or Paused under a pause) |

## `PauseService` (`HT.Application.Exemptions`)

```csharp
public sealed class PauseService
{
    public PauseService(
        IRecordStore records, IGoalStore goals, IAccountStore accounts, TimeProvider clock, InstallationTimeZone defaultZone);

    public Task<PauseResult> PauseAsync(
        ActingUser actingUser, GoalId goal, DateOnly? firstDay, DateOnly? lastDay, string? reason,
        CancellationToken cancellationToken);
    public Task<ResumeResult> ResumeAsync(ActingUser actingUser, GoalId goal, CancellationToken cancellationToken);
    public Task<CancelPauseResult> CancelPauseAsync(
        ActingUser actingUser, GoalId goal, PauseId pause, CancellationToken cancellationToken);
    public Task<PausesResult> GetPausesAsync(ActingUser actingUser, GoalId goal, CancellationToken cancellationToken);
}
```

### `PauseAsync` (FR-006 to FR-008)

| Step | Check | Outcome, text |
|---|---|---|
| 4 | `firstDay` or `lastDay` is null | `DaysMissing`, "Choose the first and the last day." |
| 5 | `firstDay < pause date` | `StartsBeforeToday`, "A pause cannot start before today." |
| 6 | `lastDay < firstDay` | `LastBeforeFirst`, "The last day cannot be before the first day." |
| 7 | it shares a date with another pause of the goal | `Overlaps`, "This goal already has a pause on these dates." |
| 8 | the reason is over 200 characters after trimming | `ReasonTooLong`, "Keep the reason to 200 characters or fewer." |
| — | `IRecordStore.AddPauseAsync` | `Paused`, "paused", with the `PauseView` (identifier, first and last day) |

Steps 4 to 7 come from `PauseRules.Check`. A store `Overlaps` (a concurrent pause) gives step 7's refusal.

### `ResumeAsync` (FR-010)

| Step | Check | Outcome, text |
|---|---|---|
| 4 | no pause is current at the pause date | `NotPaused`, "This goal is not paused." |
| — | `PauseRules.Resume`: `DeletePauseAsync` when the pause began on the pause date, else `SetPauseLastDayAsync(pause date − 1)` | `Resumed`, "resumed" |

### `CancelPauseAsync` (FR-011)

| Step | Check | Outcome, text |
|---|---|---|
| 4 | `pause` is not one of this goal's pauses (another goal's, another account's, or none) | `PauseNotFound`, "This pause does not exist." |
| 5 | its timing at the pause date is `Current` or `Past` | `HasBegun`, "Only a pause that has not begun can be cancelled. Resume the goal instead." |
| — | `IRecordStore.DeletePauseAsync` | `Cancelled`, "pause cancelled" |

### `GetPausesAsync` (FR-012)

`Found`: every pause of the goal, the latest first day first, each as a `PauseView` with its timing at the pause date.

## `IRecordStore`: new members

Each takes the owner and the goal first. A write runs in the existing transaction that locks the goal row where the
goal is the owner's. `GoalNotFound` writes nothing (R-04).

```csharp
Task<GoalExemptions> ReadExemptionsAsync(AccountId owner, GoalId goal, CancellationToken cancellationToken);
Task<ExemptionStoreWrite> ExcuseAsync(
    AccountId owner, GoalId goal, DateOnly date, ExemptionReason? reason, CancellationToken cancellationToken);
Task<ExemptionStoreWrite> RemoveExcusalAsync(AccountId owner, GoalId goal, DateOnly date, CancellationToken cancellationToken);
Task<ExemptionStoreWrite> AddPauseAsync(
    AccountId owner, GoalId goal, DateOnly firstDay, DateOnly lastDay, ExemptionReason? reason,
    CancellationToken cancellationToken);
Task<ExemptionStoreWrite> SetPauseLastDayAsync(
    AccountId owner, GoalId goal, PauseId pause, DateOnly lastDay, CancellationToken cancellationToken);
Task<ExemptionStoreWrite> DeletePauseAsync(AccountId owner, GoalId goal, PauseId pause, CancellationToken cancellationToken);
```

| Member | Re-checked under the lock |
|---|---|
| `ReadExemptionsAsync` | none. It reads all excusals and pauses of the goal, or `GoalExemptions.None` when the goal is not the owner's |
| `ExcuseAsync` | an excusal of the date → `AlreadyExcused`; a record dated on it → `HasRecord`; a pause covering it → `Paused` |
| `RemoveExcusalAsync` | no excusal → `NotExcused` |
| `AddPauseAsync` | overlap → `Overlaps`; `Written` carries the new `PauseId`. A last day before the first day throws `ArgumentException` before the transaction, as `Pause` does (T031) |
| `SetPauseLastDayAsync`, `DeletePauseAsync` | the pause not among the goal's → `PauseNotFound`. `SetPauseLastDayAsync` also refuses a later last day that would share a date with another pause → `Overlaps`, and throws `ArgumentException` for a last day before the pause's first day; `ResumeAsync` only shortens a current pause, so neither happens through the use cases (T031) |

`AddAsync` (slice 5) also gives `RecordStoreWriteOutcome.Exempt`, writing nothing, when the entry has an excusal on
its key date or a pause on one of its dates.

## `RecordingService` (slice 5): added checks

`RecordAsync` checks after "You cannot record a day that has not begun." and before the value (FR-005):

| Step | Check | Outcome, text |
|---|---|---|
| 4a | the date is in a pause | `DatePaused`, "This goal is paused on this date." |
| 4b | the entry is a week that is Paused, and the date is not in a pause | `WeekPaused`, "This week is paused." |
| 4c | the commitment is excused | `CommitmentExcused`, "This commitment is excused. Remove the excusal to record it." |

A store `Exempt` gives the matching step after the exemptions are read again. `ChangeRecordAsync`,
`DeleteRecordAsync` and `SaveNoteAsync` add no check. Their entry view carries Excused or Paused when one applies
(FR-009, FR-020).

## Changed reads

| Use case | For an excused or paused entry |
|---|---|
| `RecordingService.GetHistoryAsync` | `State` Excused or Paused; `GraceEnds` null; `IsCorrection` false; records, progress and note as before; for Excused, `ExcusalReason` and `CanRemoveExcusal` (FR-017) |
| `RecordingService.GetRecordableAsync` | left out (FR-005, FR-009) |
| `RecordingService.GetRecentNotesAsync` | `State` Excused or Paused (FR-020) |
| `ReportService.GetGoalReportAsync` | consistency and streaks skip it (FR-013, FR-014); exempt results in `Recent` with their counts (FR-015); trend bars marked (FR-016) |
| `ReportService.GetGoalsOverviewAsync` | as the report; `OverviewGoal.PausedUntil` (FR-018) |
| `ReportService.GetDashboardAsync` | never Due, Pending or Missed; Upcoming skips Paused entries; `Counts.Goals` leaves out paused goals; `DashboardGoal.PausedUntil`; notes with Excused or Paused (FR-019, FR-020) |

## `DemoPauseService` (`HT.Application.Operator`, FR-031)

```csharp
public sealed class DemoPauseService
{
    public const string DemoEmail = "demo6@example.com";

    public DemoPauseService(
        IDemoPauseStore demoPause, IAccountStore accounts, TimeProvider clock, InstallationTimeZone defaultZone);

    public Task<DemoPauseLoadingResult> LoadAsync(string? demoPassword, CancellationToken cancellationToken);
}
```

| Case | Result |
|---|---|
| no password, or blank | "no demo password configured"; nothing created |
| password fails the rule | "demo password does not meet the password rule"; nothing created |
| `demo6@example.com` exists (ignoring case, either role), or the store reports `EmailTaken` | "already exists"; nothing read or changed |
| otherwise | "created", "demo goals added", "demo records added" for `demo6@example.com`, also when goals are left out |

The run date R is `defaultZone.DateAt(now)`. The account has no name, the default time zone and the creation time
`now`. `DemoPauses.For(R)` gives FR-031's goals. A goal whose catalog habit is not Published (by name, ignoring case)
is left out with its records, notes, excusals and pauses.

## `IDemoPauseStore` (`HT.Application.Operator`)

```csharp
public interface IDemoPauseStore
{
    Task<CreateDemoAccountOutcome> CreateDemoPauseAccountAsync(
        Email email, string password, DemoAccountProfile profile, IReadOnlyList<DemoPauseGoal> goals,
        CancellationToken cancellationToken);
}
```

The contract is `IDemoReportStore`'s, plus the goal's excusals and pauses, written in the same transaction after its
records and notes. It is public for the AC reference export, and fenced to `DemoPauseService` by architecture rules.
