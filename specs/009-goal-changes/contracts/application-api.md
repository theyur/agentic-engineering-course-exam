# Contract: Application API of goal changes

The public surface this slice adds to `HT.Application` and `HT.Domain`, which the acceptance tests and the pages
call. Types are in [data-model.md](../data-model.md). Existing constructors, method signatures and positional records
of slices 1 to 7 are unchanged (FR-024). What they now give for changed, ended and restarted goals is listed under
"Changed reads".

## Common rules

- **Access** (FR-022, FR-023), checked first and in this order:
  1. no account id → "You are not signed in.";
  2. role is not registered user → "Goals are for registered users.";
  3. account not stored → "You are not signed in.";
  4. the goal is not among the owner's goals → "This goal does not exist.".

  A refusal returns no view and changes nothing. The goal history has no goal, so it stops after step 3.
- **Time**: `TimeProvider.GetUtcNow()`, read once per call. The account's today is the date in the account's stored
  zone, else the installation default.
- **Goal's current date**: `GoalTimeline.CurrentDate` (research R-02).
- **Owner**: every store call passes the acting user's account and the goal named in the call. An attempt is looked up
  only by number among that goal's attempts.
- **Writes**: each write reads `IGoalStore.ReadOwnerGoalAsync`, decides in the Domain, and calls its conditional store
  member. `Stale` reads again and decides again, at most `GoalService.MaxCreateAttempts` (10) times. After that it
  throws `InvalidOperationException`, whose message names no value. The checks are made again on each read, so a
  concurrent ending gives "This goal has ended." on the retry.
- **Logging**: none. No result, exception or log names an ending note, note, value, goal or habit name (HP-010).

## `GoalChangeService` (`HT.Application.Goals`)

```csharp
public sealed class GoalChangeService
{
    public GoalChangeService(
        IGoalStore goals, IAccountStore accounts, TimeProvider clock, InstallationTimeZone defaultZone);

    public Task<SettingsChangeResult> ChangeSettingsAsync(
        ActingUser actingUser, GoalId goal, GoalSettingsInput input, CancellationToken cancellationToken);
    public Task<GoalEndResult> EndAsync(
        ActingUser actingUser, GoalId goal, Outcome? outcome, string? note, CancellationToken cancellationToken);
    public Task<NewAttemptValuesResult> GetNewAttemptValuesAsync(
        ActingUser actingUser, GoalId goal, CancellationToken cancellationToken);
    public Task<AttemptStartResult> StartAttemptAsync(
        ActingUser actingUser, GoalId goal, NewAttemptInput input, CancellationToken cancellationToken);
}
```

### `ChangeSettingsAsync` (FR-001 to FR-006)

After access, checking in this order:

| Step | Check | Outcome, text |
|---|---|---|
| 4 | the goal is ended | `GoalEnded`, "This goal has ended." |
| 5 | `GoalDraft.CheckTarget(input.Target)` | the problem's outcome and text, as goal creation gives them (slice 4 FR-006), then `ReduceAmountNeedsOneTime`, "A Reduce goal with an amount has 1 time per period." |
| 6 | `!target.HasSameScheduleKind(latest version's target)` | `ScheduleKindChanged`, "A change keeps the schedule kind: fixed days, times per day or times per week. End the goal and start a new attempt to change it." |
| 7 | `GoalName.TryCreate(input.Name)` too long | `NameTooLong`, "The name can have at most 60 characters." |
| — | the target equals the latest version's target and the name equals the goal's own name | `Unchanged`, "settings unchanged"; nothing is written |
| — | only the name differs | `TryChangeSettingsAsync` with `Versions` null → `Changed`, "settings changed", `AppliesFrom` null |
| — | otherwise | `GoalTimeline.SettingsChangeOf(goal, now, target, account zone)` → `TryChangeSettingsAsync` → `Changed`, "settings changed", `AppliesFrom` |

Values that do not belong to the chosen measurement or schedule are ignored by `CheckTarget` (slice 4 FR-006), so they
are neither refused nor stored (AC-021).

### `EndAsync` (FR-007 to FR-009)

| Step | Check | Outcome, text |
|---|---|---|
| 4 | the goal is ended | `AlreadyEnded`, "This goal has already ended." |
| 5 | `outcome` is null or undefined | `OutcomeMissing`, "Choose how the goal ended." |
| 6 | `EndingNote.TryCreate(note)` too long | `NoteTooLong`, "The note can have at most 500 characters." |
| — | E = the goal's current date, or the account's today when the goal has not started. `GoalTimeline.EndingOf(goal, E)` → `TryEndAsync` | `Ended`, "goal ended", `EndingDate` E, `LastDate` L |

### `GetNewAttemptValuesAsync` (FR-011)

| Step | Check | Outcome, text |
|---|---|---|
| 4 | the goal is not ended | `NotEnded`, "Only an ended goal can start a new attempt." |
| — | the target of the ended attempt's last version, and `NewAttemptEarliestStart(goal, account today)` | `Found`, with `Values` and `Values.AttemptNumber` = the latest number + 1 |

### `StartAttemptAsync` (FR-012)

| Step | Check | Outcome, text |
|---|---|---|
| 4 | the goal is not ended | `NotEnded`, "Only an ended goal can start a new attempt." |
| 5 | `GoalDraft.CheckTarget(input.Target)` | as `ChangeSettingsAsync` step 5 |
| 6a | no start date | `StartDateMissing`, "Choose a start date." |
| 6b | before the account's today | `StartDateInPast`, "The start date cannot be in the past." |
| 6c | on or before the ending date | `StartDateNotAfterEnding`, "A new attempt starts after the day the goal ended." |
| 6d | more than 365 days after the account's today | `StartDateTooFar`, "Choose a start date within a year from today." |
| — | `TryStartAttemptAsync` with the attempt number n + 1 and its first version from the start date in the account's current zone | `Started`, "attempt started", `AttemptNumber`, `StartDate` |

The catalog habit is not looked up, so an Unpublished catalog habit can start a new attempt (FR-012, AC-064).

## `AttemptService` (`HT.Application.Reports`)

```csharp
public sealed class AttemptService
{
    public AttemptService(IGoalStore goals, IRecordStore records, IAccountStore accounts, TimeProvider clock);

    public Task<AttemptListResult> GetAttemptsAsync(
        ActingUser actingUser, GoalId goal, CancellationToken cancellationToken);
    public Task<AttemptReadResult> GetAttemptAsync(
        ActingUser actingUser, GoalId goal, int number, CancellationToken cancellationToken);
    public Task<GoalHistoryListResult> GetGoalHistoryAsync(
        ActingUser actingUser, CancellationToken cancellationToken);
}
```

- **`GetAttemptsAsync`** (FR-014): every attempt, the latest first, as `AttemptView`. Its figures are computed over
  that attempt up to the supplied time through `GoalEvaluation.OfAsync(…, number, …)`.
- **`GetAttemptAsync`** (FR-014): after access, a number that is not one of the goal's attempts gives
  `AttemptNotFound`, "This attempt does not exist.". Otherwise it returns `AttemptDetails`:
  - the attempt's `AttemptView`;
  - its recent results (slice 6 FR-005 with slice 7 FR-015);
  - its History: the attempt's dates from its start date to its last date, or to the current date while it is not
    ended, newest first, the newest 366 when there are more.

  Both are computed at the supplied time. An attempt with no date has an empty History and "N/A" (AC-102).
- **`GetGoalHistoryAsync`** (FR-015): every ended attempt of the acting user's goals, as `EndedAttemptView`. The order
  is the latest ending date first, then the name compared by `HabitName.KeyOf` ordinally, then the attempt number.

## `IGoalStore`: new and changed members

| Member | Contract |
|---|---|
| `ReadOwnerGoalAsync` (new) | The stored zone and `GoalsRevision`, then the goal with all its attempts, versions and endings, read in that order. Null when the account is gone. `Goal` is null for another owner's or an unknown goal. |
| `TryChangeSettingsAsync` (new) | One transaction:<br>1. lock the account row (UPDLOCK, HOLDLOCK);<br>2. check the stored zone and `GoalsRevision`;<br>3. lock the owner's goal row;<br>4. check that the latest attempt is `write.Attempt` and has no ending;<br>5. write the name and, when given, replace that attempt's version rows;<br>6. increment `GoalsRevision`.<br>`Stale` on a failed check, `GoalNotFound` for no such goal of the owner. |
| `TryEndAsync` (new) | The same transaction and checks. Then:<br>• write `EndingDate`, `Outcome` and `EndingNote`;<br>• replace the attempt's version rows with `KeptVersions`;<br>• delete `GoalPauses` rows whose first day is after `LastDate`, set the last day to `LastDate` where the first day is on or before it and the last day after it;<br>• delete `GoalExcusals` rows dated after `LastDate`;<br>• increment `GoalsRevision`. |
| `TryStartAttemptAsync` (new) | The same locks and the zone and revision check. Then it checks that the latest attempt has an ending and is numbered `write.Number` − 1, inserts the attempt row and its first version row, and increments `GoalsRevision`. |
| (all three writes) | Settled in T038: the checks run in the order of the steps above, so a stale zone or revision gives `Stale` before the goal is looked up (also for another owner's goal, and for an account that is gone), and only a fresh zone and revision with no such goal of the owner gives `GoalNotFound`. The name write of `TryChangeSettingsAsync` stores `write.Name` as it is: null removes the goal's own name. |
| `ListAsync`, `FindAsync`, `ReadOwnerGoalsAsync` | Read every attempt with its versions and ending (one query, as before). |
| `TryChangeTimeZoneAsync` | Applies each write to the latest attempt's versions. |
| `CountByCatalogHabitAsync`, `ListOnCatalogHabitAsync` | Leave out ended goals (FR-020). |
| `CountUsingCustomHabitAsync`, `CountUsingCatalogHabitAsync` | Unchanged: ended goals still count as "used by goals" (Edge Cases, AC-105, AC-106). |

## `IRecordStore`: new and changed members

| Member | Contract |
|---|---|
| `ReadRecentNoteKeysAsync(owner, goal, from, through, count, ct)` (new overload) | The newest `count` note keys from `from` through `through` (no upper bound when `through` is null), under the owner scope. Empty for a `count` of 0 or less, as the old overload. |
| `AddPauseAsync` | Also re-checks under the goal lock that the goal's latest attempt has no ending; otherwise it returns `GoalEnded` with nothing written. This re-check comes before the overlap check (T039). |

## Changed reads (FR-013, FR-016 to FR-021)

| Use case | Slice 8 behaviour |
|---|---|
| `GoalService.GetAsync` | The settings in effect (`GoalTimeline.VersionInEffect`), with the full `Later` settings (FR-005). The name at the read's instant (`GoalText.Name(…, now)`, AC-001, AC-002). The latest attempt's number and start date, `AttemptHasBegun`, the ending fields (FR-017) and `ChangesApplyFrom`. For an ended goal, its latest attempt's last version and no later version. For a goal whose attempt has not begun, that attempt's first version. |
| `GoalService.ListAsync` | `IsEnded`, `EndingDate`, `Outcome`. Names and the name order at the read's instant. `StartDate` is the latest attempt's while it has not begun and the goal is not ended. An ended goal has no next commitment and no first-week date. |
| `GoalService.GetCalendarAsync` | No commitment or period outside an attempt (FR-016, AC-096 to AC-098). A range across attempts of both kinds fills both lists. `Kind` is the kind of the latest attempt with an entry in the range (research R-02). |
| `RecordingService.RecordAsync`, `SaveNoteAsync` | A date outside every attempt gives "Nothing is scheduled on this date.". A date of a waiting attempt gives "You cannot record a day that has not begun." until its day begins in that attempt's zone (FR-010). The value is checked against the measurement of the version of its date. |
| `RecordingService.ChangeRecordAsync`, `DeleteRecordAsync` | Work for every attempt's dates (FR-014), each value checked against the measurement of the version of its date. A record kept outside every attempt (FR-009) gives `NothingScheduled`, "Nothing is scheduled on this date.", and nothing changes. They no longer throw. |
| `RecordingService.GetHistoryAsync` | The range check is unchanged. Only the latest attempt's entries within the range are returned (FR-016). Each entry carries its own `Settings`. |
| `RecordingService.GetRecordableAsync` | The latest attempt's Due and Pending entries, plus the open Pending entries of earlier ended attempts, each with its attempt fields and its own `Settings` (FR-016, AC-080). |
| `RecordingService.GetRecentNotesAsync` | Only notes inside the latest attempt's entries, read through the bounded overload. A key with no entry is skipped. Ending notes never appear (FR-027). |
| `ReportService.GetGoalReportAsync` | Over the latest attempt (FR-013). |
| `ReportService.GetGoalsOverviewAsync` | Ended goals keep slice 6's order, with their latest attempt's figures. They are never paused and have no Due entry (FR-018). `HasPending`, `CanRecord`, `Recordable` and the Pending group of the order include earlier Pending entries, so `Recordable` equals `GetRecordableAsync` at the same instant. |
| `ReportService.GetDashboardAsync` | As data-model.md "Changed views" says (FR-019). |
| `CatalogService.BrowseAsync`, `GetDetailsAsync` | Goal counts, "Only habits I track" and the details' goal list leave out ended goals (FR-020). Built with the added `TimeProvider` constructor, the goal list names each goal at one time read. Settled in T049: `AddHtApplication` registers it through `ActivatorUtilities.CreateInstance<CatalogService>(provider, clock)` with the registered `TimeProvider`, so the host never uses the two-argument constructor (data-model.md). |
| `RecordPreview.Progress`, every `RecordingText` member that takes a `GoalHistory` (`Recorded`, `TargetLine`, `Progress`, `FieldLabel`, `PartialNote`, `ReduceHelp`, `NoteHelp`), `ReportText.ProgressText` | Overloads on `EntrySettings` (research R-07). The pages call only those, with `GoalHistory.SettingsOf(entry)`. `RecordPreview.Progress(GoalHistory, HistoryEntry, …)` reads `history.SettingsOf(entry)`. |
| `ProfileService.ChangeTimeZoneAsync` | Adds no version to an ended goal, and updates the first version of a waiting attempt (FR-021). |
| `PauseService.PauseAsync` | Step after (3): an ended goal gives `GoalEnded`, "This goal has ended.". |
| `ExcusalService.ExcuseAsync` | Step 4 uses the schedule kind of the date's attempt. A date between attempts, or after an ended latest attempt, skips it and gets step 5's "Nothing is scheduled on this date." (data-model.md, T050). |

## `DemoGoalChangeService` (`HT.Application.Operator`, FR-031)

```csharp
public sealed class DemoGoalChangeService
{
    public const string DemoEmail = "demo7@example.com";

    public DemoGoalChangeService(
        IDemoGoalChangeStore demoGoalChange, IAccountStore accounts, TimeProvider clock, InstallationTimeZone defaultZone);

    public Task<DemoGoalChangeLoadingResult> LoadAsync(string? demoPassword, CancellationToken cancellationToken);
}
```

The behaviour is the same as `DemoPauseService.LoadAsync`:

- a missing password gives "no demo password configured", and a weak one "demo password does not meet the password
  rule", creating nothing;
- an existing account (ignoring case) gives "already exists" and is neither read nor changed;
- otherwise it creates the account with `DemoGoalChanges.For(R)` and gives "created", "demo goals added" and "demo
  records added" for `demo7@example.com`.

R is the date of the supplied time in the default zone. A goal whose catalog habit is not Published is left out. The
results never name a goal, habit, note, outcome, unit or amount.

## `IDemoGoalChangeStore` (`HT.Application.Operator`)

One member, `CreateDemoGoalChangeAccountAsync(email, password, profile, goals, ct)`, with the contract of
`IDemoPauseStore.CreateDemoPauseAccountAsync`: one transaction, and an existing email gives `EmailTaken`. Each goal is
written with its attempts (rows with their endings), each attempt's versions, its records and its notes, as stored
facts (FR-026). A fault rolls everything back, and its exception names no goal, habit, note, outcome or value.
Architecture rules keep every HT.Ui type (`UiDependencyTests.NoUiTypeMayDependOnTheDemoGoalChangeStorePort`), and
every HT.Application type other than `DemoGoalChangeService`, away from it. `DemoGoalChangeStore` is internal to
HT.Persistence (research R-12).

## Texts (`GoalChangeTexts`)

| Constant | Text |
|---|---|
| `SettingsChanged` | "settings changed" |
| `SettingsUnchanged` | "settings unchanged" |
| `Ended` | "goal ended" (`GoalEndResult.Ended`) |
| `AttemptStarted` | "attempt started" |
| `GoalEnded` | "This goal has ended." (the refusal outcomes named `GoalEnded`: `SettingsChangeResult`, `PauseResult`) |
| `AlreadyEnded` | "This goal has already ended." |
| `ScheduleKindChanged` | "A change keeps the schedule kind: fixed days, times per day or times per week. End the goal and start a new attempt to change it." |
| `OutcomeMissing` | "Choose how the goal ended." |
| `NotEnded` | "Only an ended goal can start a new attempt." |
| `StartDateNotAfterEnding` | "A new attempt starts after the day the goal ended." |
| `AttemptNotFound` | "This attempt does not exist." |
| `Achieved`, `Abandoned`, `NoLongerRelevant` | "Achieved", "Abandoned", "No longer relevant" |

Each outcome and the text constant it maps to share one name, so a wrong mapping shows in review: `GoalEnded` is always
"This goal has ended." and `Ended` is always "goal ended". `TodayEntry.GoalEnded` is a flag, not an outcome, and maps
to no text.

"The note can have at most 500 characters." reuses slice 5's `RecordTexts.NoteTooLong`. The texts of slices 1 and 4
(access, target fields, start date, name) are reused from their existing classes.
