# Data model: Recording and evaluation

**Feature**: specs/005-recording-and-evaluation · **Plan**: [plan.md](plan.md) · **Research**: [research.md](research.md)

Names are the contract; members not listed are private. Every instant is a UTC `DateTimeOffset`, and every date
is a `DateOnly`. No type here reads a clock, a machine time zone or the current culture (constitution II).

## Domain: records (`HT.Domain.Records`, new)

| Type | Kind | Members and rules |
|---|---|---|
| `RecordId` | `readonly record struct (Guid Value)` | Created by the record store, never by a caller. |
| `YesNoAnswer` | enum | `Yes`, `No`. |
| `RecordValue` | `sealed class`, get-only properties, private constructor (no `default` value, research R-02) | `Answer` (`YesNoAnswer?`) and `Amount` (`int?`): exactly one is set. Made only by `TryCreate(Measurement, YesNoAnswer? answer, int? amount, out RecordValue, out RecordProblem)`: yes/no keeps the answer and drops an amount (no answer: `AnswerMissing`); count keeps an amount from 0 to 9999 and drops an answer (otherwise `CountOutOfRange`); duration keeps whole minutes from 0 to 1440 and drops an answer (otherwise `DurationOutOfRange`). A stored value is rebuilt by the public factories `FromAnswer(YesNoAnswer)` and `FromAmount(int)`; the second throws outside 0 to 9999, naming no value. `HT.Persistence` calls them, because it cannot see Domain internals (research R-02). |
| `GoalRecord` | `sealed record (RecordId Id, DateOnly Date, RecordValue Value)` | One record of one goal on one date (FR-001, FR-003). |
| `NoteText` | `sealed class`, `Value` | Trimmed, 1 to 500 characters. `TryCreate(string? text, out NoteText? note, out bool tooLong)`: null, empty or only spaces give no note and true; more than 500 after trimming gives `tooLong` and false (FR-007). |
| `RecordProblem` | enum | `None`, `NothingScheduled`, `DayNotBegun`, `AnswerMissing`, `CountOutOfRange`, `DurationOutOfRange`, `NoteTooLong`, `RecordLimitReached`, in FR-005's order from the date on. |
| `RecordLimits` | static class | `MaxRecordsPerPeriod = 50`, `MaxCount = 9999`, `MaxMinutes = 1440`, `MaxNoteLength = 500` (D-8). |

## Domain: evaluation (`HT.Domain.Evaluation`, new)

| Type | Kind | Members and rules |
|---|---|---|
| `EvaluationState` | enum | `Upcoming`, `Due`, `Pending`, `Met`, `Failed`, `Missed` (FR-010). Excused and Paused come in slice 7. |
| `GracePeriod` | static class | `Length = TimeSpan.FromHours(48)`; `EndOf(DateTimeOffset ends) = ends + Length`, on UTC instants, so exactly 48 elapsed hours (FR-016). |
| `ProgressKind` | enum | `RecordedValue` (a commitment), `RecordsReachingAmount` (a Build period), `YesRecords` (a Reduce yes/no period, at most), `Total` (a Reduce count or duration period, at most). |
| `Progress` | `sealed record (ProgressKind Kind, int? Value, int Of)` | `Value` is null only for a commitment without a record. A commitment's `Of` is the amount (count, or minutes: the amount × 60 for hours), or 1 for yes/no, where Yes counts 1 and No 0. A period's `Of` is N, or the amount for `Total` (FR-014). |
| `EntryEvaluation` | `sealed record (EvaluationState State, Progress Progress, DateTimeOffset GraceEnds, bool IsFinal)` | `IsFinal` is `now >= GraceEnds`. |
| `Evaluator` | static class | `Evaluate(GoalTarget target, DateTimeOffset starts, DateTimeOffset ends, IReadOnlyList<RecordValue> records, DateTimeOffset now)`: the rules of research R-01. With no record at the end of its grace period, an entry is `Missed`, for Build and Reduce alike (FR-013). `ProgressOf(GoalTarget target, IReadOnlyList<RecordValue> records)` gives the same `Progress` without instants or a clock; `Evaluate` uses it, and so does `RecordPreview` for the RecordDialog's live progress (T015, T028). Both throw `ArgumentException`, naming no value, for a commitment with more than one record or a record whose kind (answer or amount) does not match the target's measurement: programming errors, since only values made by `TryCreate` with the goal's measurement are written. |

## Domain: goals (`HT.Domain.Goals`, extended)

| Type | Change |
|---|---|
| `CalendarEntry` | New `sealed record (DateOnly FirstDate, DateOnly LastDate, DateTimeOffset Starts, DateTimeOffset Ends)` with `Key => FirstDate`. One commitment (both dates equal) or one evaluation period. |
| `GoalTimeline` | New members: `EntryOf(Goal, DateOnly date)` gives the `CalendarEntry` that `date` belongs to, or null (FR-002, research R-03); `HasBegun(Goal, DateOnly date, DateTimeOffset instant)`; and `TryEntries(Goal, DateOnly first, DateOnly last, out IReadOnlyList<CalendarEntry>)`, the calendar of `TryCalendar` as entries. Existing members are unchanged. The Application evaluates an entry with `GoalTimeline.VersionOf(goal, entry.FirstDate).Settings.Target`, never with a later version, so a later slice's differing versions are already used correctly (FR-015). `Goal` is unchanged. |

### Invariants (unit-tested)

- A `RecordValue` never has both an answer and an amount, and never an amount outside 0 to 9999. A value made by
  `TryCreate` never has an amount outside its measurement's range, and only such values are written.
- An entry's state is a function of `(target, starts, ends, records, now)` only. The same records give the
  same state whether they were written before or after the grace period ended (SC-003, constitution II).
- `GraceEnds - Ends == 48 h` for every entry, including entries whose grace period crosses a clock change.
- `EntryOf(goal, d)` is non-null exactly when `TryCalendar(goal, d, d)` gives one commitment or period that
  contains `d`.

## Application: records (`HT.Application.Records`, new)

### Inputs and views

| Type | Members |
|---|---|
| `RecordInput` | `sealed record`: `Answer` (`YesNoAnswer?`), `Amount` (`int?`), `Note` (`string?`). Values that do not belong to the goal's measurement are dropped (FR-004). |
| `RecordView` | `sealed record (RecordId Id, DateOnly Date, YesNoAnswer? Answer, int? Amount)`. |
| `HistoryEntry` | `FirstDate`, `LastDate`, `Starts`, `Ends`, `State` (`EvaluationState`), `GraceEnds` (`DateTimeOffset?`, set only while Due or Pending), `Records` (`IReadOnlyList<RecordView>`, date order and then stored order), `Progress`, `Note` (`string?`), `IsCorrection` (`bool`: a change now would be a correction). |
| `GoalHistory` | `GoalId`, `GoalScheduleKind`, `Period` (`SchedulePeriod?`), `BuildOrReduce`, `Measurement`, `Amount`, `CountUnit`, `DurationUnit`, `Times`, `CurrentDate` (`DateOnly?`), `Target` (`GoalTarget`: the target of the settings version of `CurrentDate`, or of the start date when there is none; only `RecordPreview` and `RecordingText` read it; in this slice every version has the same target, FR-015), `ReadAt` (`DateTimeOffset`: the instant the use case read the clock, so the pages format "N h left" without reading one), `Entries` (`IReadOnlyList<HistoryEntry>`: date order from `GetHistoryAsync`; newest first from `GetRecordableAsync`). No goal or habit name (T022). |
| `RecentNote` | `FirstDate`, `LastDate`, `Note` (`string`), `State` (`EvaluationState`). |

### Results (ADR 0005)

| Type | Members |
|---|---|
| `RecordWriteOutcome` | `NotSignedIn`, `NotForAdministrators`, `NotFound`, `RecordNotFound`, `NothingScheduled`, `DayNotBegun`, `AnswerMissing`, `CountOutOfRange`, `DurationOutOfRange`, `NoteTooLong`, `RecordLimitReached`, `Recorded`, `Changed`, `Deleted`. Each write use case gives a subset, in this order (contracts/application-api.md). |
| `RecordWriteResult` | `Outcome`, `Message` (null on success), `RecordId` (`RecordId?`: set for `Recorded` and `Changed`), `Entry` (`HistoryEntry?`, after the write), `IsCorrection` (`bool`; false for a refusal). |
| `NoteSaveOutcome` | `NotSignedIn`, `NotForAdministrators`, `NotFound`, `NothingScheduled`, `DayNotBegun`, `NoteTooLong`, `Saved`. |
| `NoteSaveResult` | `Outcome`, `Message`, `Entry` (`HistoryEntry?`). |
| `HistoryOutcome` | `NotSignedIn`, `NotForAdministrators`, `NotFound`, `RangeOutOfBounds`, `Found`. |
| `HistoryResult` | `Outcome`, `Message`, `History` (`GoalHistory?`). |
| `RecordableOutcome` | `NotSignedIn`, `NotForAdministrators`, `NotFound`, `Found`. |
| `RecordableResult` | `Outcome`, `Message`, `History` (`GoalHistory?`, whose `Entries` are the Due and Pending ones, newest first). |
| `RecentNotesOutcome` | `NotSignedIn`, `NotForAdministrators`, `NotFound`, `Found`. |
| `RecentNotesResult` | `Outcome`, `Message`, `Notes` (`IReadOnlyList<RecentNote>`, at most 5, newest first; empty for a refusal). |

Each result has a private constructor and internal static factories (ADR 0005): `Refused(outcome, message)`, which
carries no input value, and one per success (`RecordWriteResult.Recorded(RecordId, HistoryEntry, bool isCorrection)`,
`Changed(RecordId, HistoryEntry, bool isCorrection)`, `Deleted(HistoryEntry, bool isCorrection)`;
`NoteSaveResult.Saved(HistoryEntry)`; `HistoryResult.Found(GoalHistory)`; `RecordableResult.Found(GoalHistory)`;
`RecentNotesResult.Found(IReadOnlyList<RecentNote>)`).

`RecordingText` (static) is the shared formatter of research R-12 (T024). It is culture-invariant: numbers use the
invariant culture, dates come only from `DateTexts`, and the time is a parameter. It takes the goal's measurement and
unit from `GoalHistory`, and only `Title` takes a goal name. Its texts are the design's slice-5 state
(`design/components/RecordDialog.md`, `RecordForms.md`, `StatusChip.md`); a count unit is written as stored, never
inflected ("3 sweet").

| Member | Result |
|---|---|
| `State(EvaluationState state, GoalScheduleKind scheduleKind, SchedulePeriod? period)` | "Met", "Failed", "Missed", "Pending"; "Due today" for a commitment or a per-day period, "Due this week" for a per-week period. Upcoming throws `ArgumentOutOfRangeException` (no page of this slice shows it). |
| `Progress(GoalHistory history, Progress progress)` | `string?`: "12 of 20 min", "1 of at most 2 cups", "2 of 3 times", "0 of 1 time", "1 of at most 2 times"; durations in minutes. Null for a yes/no commitment and for a commitment whose `Value` is null. |
| `ProgressFill(Progress progress)` | `int`, 0 to 100: the value against `Of`, capped at full; `Of` 0 is full above 0 and empty at 0; no value is 0. |
| `Amount(Measurement measurement, string? countUnit, int amount)` | "12 min" (duration, always minutes), "3 sweet", "2 cups" (count, unit as stored). Yes/no throws `ArgumentOutOfRangeException`. |
| `Recorded(GoalHistory history, RecordView record)` | "Yes", "No", or `Amount`. |
| `EntryDate(HistoryEntry entry)` | `DateTexts.ShortDate` for a single date, `DateTexts.WeekRange` for a week. |
| `Title(string goalName)` | "Record · {goal name}". |
| `FieldLabel(GoalHistory history)` | "Did you do it?" (yes/no), "Amount" (count), "Time spent" (duration). |
| `ReduceHelp(GoalHistory history)` | `string?`: "At most {AmountText}. Enter 0 if you had none: an empty day cannot prove you stayed under the limit." (count, duration); "Choose No if you avoided it: an empty day cannot prove you stayed under the limit." (yes/no); null for Build. |
| `ForLabel(HistoryEntry entry, DateOnly currentDate, DateTimeOffset now)` | "Today, Fri 30 Oct" for the Due entry whose single date is `currentDate`; otherwise `EntryDate`; a Pending entry adds " · Pending, N h left", N the whole hours to `GraceEnds`, rounded up. Called with `GoalHistory.CurrentDate` and `ReadAt`. |
| `TargetLine(GoalHistory history)` | The sub line, from `GoalHistory.Target`: "Reduce · " and `GoalText.TargetText`; "Build · at least {AmountText} · {schedule part}"; "Build · {schedule part}" for yes/no. The schedule part is "every day", the day groups of `GoalText.ScheduleText`, "N times a day", "once a week" or "N times a week". |
| `PartialNote(GoalHistory history)` | `string?`: the partial-progress sentence of a Build count or duration goal by schedule kind ("Partial time is kept. Below 20 min the day counts as Failed when it is finalized."); null for yes/no and Reduce. |

`RecordPreview` (static, T028) gives the RecordDialog's live progress, so no component evaluates FR-014:
`Progress(GoalHistory history, HistoryEntry entry, RecordId? replacing, YesNoAnswer? answer, int? amount)` rebuilds
the entry's other records with `RecordValue.FromAnswer` or `FromAmount`, leaving out `replacing` (the record being
edited) and, for a commitment, its existing record in every case, because saving replaces it (FR-003). It adds the
candidate when `RecordValue.TryCreate` accepts it for `history.Measurement`, and returns
`Evaluator.ProgressOf(history.Target, values)`. A missing or invalid candidate gives the progress without it.

## Application: demo history (`HT.Application.Operator`, extended)

| Type | Members |
|---|---|
| `DemoHistoryService` | `LoadAsync(string? demoPassword, CancellationToken)` gives `DemoHistoryLoadingResult` (research R-08). `DemoEmail = "demo4@example.com"`. |
| `DemoHistory` | static: `For(DateOnly runDate)` gives the three `DemoHistoryGoal`s of FR-030. For smoke check 6 (research R-10): `GoalNames`, the three display names in creation order; `PartialMinutes = 12`; `ReadingNote = "Fell asleep early."`. |
| `DemoHistoryGoal` | `Goal` (`DemoGoal`, start date R − 28, accepted state Published), `Records` (`IReadOnlyList<DemoRecord>`, date order), `Notes` (`IReadOnlyList<DemoNote>`). |
| `DemoRecord` | `sealed record (DateOnly Date, RecordValue Value)`. |
| `DemoNote` | `sealed record (DateOnly EntryDate, NoteText Text)`. `EntryDate` is the entry key: the day, or a week's Monday. |
| `DemoHistoryLoadingOutcome` | `NoPassword`, `PasswordRuleNotMet`, `Loaded`. |
| `DemoHistoryLoadingResult` | `Outcome`, `Message` (the refusal text, null for `Loaded`), `Accounts` (`IReadOnlyList<DemoHistoryAccountResult>`: one entry, for demo4, for `Loaded`; empty for a refusal). |
| `DemoHistoryAccountResult` | `Email`, `Outcome` (`DemoAccountOutcome`), `Message` ("created" or "already exists"), `Goals` (`DemoGoalsResult?`: "demo goals added" for a created account, null otherwise), `Records` (`DemoRecordsResult?`: "demo records added" for a created account, null otherwise). |
| `DemoRecordsResult` | `Message`; the one value `Added` ("demo records added"). |
| `OperatorTexts` | + `DemoRecordsAdded = "demo records added"`. |

`DemoDataService`, `DemoLoadingResult`, `DemoAccountResult` and `IDemoDataStore` are unchanged (FR-031).

## Ports

### `IRecordStore` (`HT.Application.Records`, new)

Every member takes `AccountId owner, GoalId goal` first and acts only when the goal is one of the owner's, in the
same statement (constitution III, research R-05). An entry is passed as an `EntrySpan(DateOnly Key, DateOnly
FirstDate, DateOnly LastDate)`.

| Member | Result |
|---|---|
| `AddAsync(owner, goal, EntrySpan entry, bool isCommitment, DateOnly date, RecordValue value, NoteText? note, int limit)` | `RecordStoreWrite`: `GoalNotFound`; `LimitReached` (a period at `limit` records; nothing written); or `Written` with the `RecordId` (a commitment's existing record keeps its identifier and gets the value) and the entry's `StoredEntry`. A note with text is upserted under `entry.Key`. One transaction under the goal row's update lock. |
| `FindRecordDateAsync(owner, goal, RecordId record)` | The record's date, or null when the goal has no such record (another goal's, another account's or none). |
| `ChangeAsync(owner, goal, RecordId record, EntrySpan entry, RecordValue value, NoteText? note)` | `GoalNotFound`, `RecordNotFound`, or `Written` with the `StoredEntry`. |
| `DeleteAsync(owner, goal, RecordId record, EntrySpan entry)` | `GoalNotFound`, `RecordNotFound`, or `Written` with the `StoredEntry`. |
| `SaveNoteAsync(owner, goal, EntrySpan entry, NoteText? note)` | `GoalNotFound`, or `Written` with the `StoredEntry`. No note deletes it (FR-007). |
| `ReadAsync(owner, goal, DateOnly from, DateOnly to)` | `StoredEntries`: the records with dates from `from` to `to`, in date order and then stored order, and the notes with keys from `from` to `to`. Callers pass the first entry's `FirstDate` and the last entry's `LastDate`, so the notes are exactly those of the entries read (research R-05). Empty for another owner's goal. |
| `ReadRecentNoteKeysAsync(owner, goal, int count)` | The keys of the `count` notes with the latest keys, newest first. |

`StoredEntry` is `(IReadOnlyList<GoalRecord> Records, NoteText? Note)`. `StoredEntries` is `(IReadOnlyList<GoalRecord>
Records, IReadOnlyDictionary<DateOnly, NoteText> Notes)`. `EntrySpan` is a `sealed record`.

`RecordStoreWrite` is a sealed class with a private constructor, as slice 4's `GoalWriteOutcome` is: `Outcome`
(`RecordStoreWriteOutcome`: `GoalNotFound`, `RecordNotFound`, `LimitReached`, `Written`), `RecordId` (`RecordId?`: set
for `Written` from add and change) and `Entry` (`StoredEntry?`: set for `Written`). Its public factories, for
`HT.Persistence` and the fakes: the static properties `GoalNotFound`, `RecordNotFound` and `LimitReached`, and
`Written(RecordId, StoredEntry)` (add, change) and `Written(StoredEntry)` (delete, save note). Every member also takes
a `CancellationToken` last; `FindRecordDateAsync` gives `DateOnly?` and `ReadRecentNoteKeysAsync`
`IReadOnlyList<DateOnly>`.

### `IDemoHistoryStore` (`HT.Application.Operator`, new)

| Member | Result |
|---|---|
| `CreateDemoHistoryAccountAsync(Email email, string password, DemoAccountProfile profile, IReadOnlyList<DemoHistoryGoal> goals, CancellationToken)` | `CreateDemoAccountOutcome` (slice 4's enum). One transaction: the registered user with the profile; each goal whose catalog habit (found by name key) is Published, with version 1 in the profile's zone and attempt 1 from its start date, then its records in list order and its notes. A goal whose habit is not Published is left out with its records and notes. A taken email gives `EmailTaken` and creates nothing. A fault rolls everything back; its exception names no goal, habit, note or value (HP-006). |

`IGoalStore`, `IAccountStore` and `IDemoDataStore` are unchanged.

## Tables (`HT.Persistence`, migration `Records`)

### `GoalRecords`

| Column | Type | Notes |
|---|---|---|
| `Id` | `uniqueidentifier` | Primary key, nonclustered. |
| `GoalId` | `uniqueidentifier` | Foreign key to `Goals.Id`, cascade delete. |
| `Date` | `date` | The record's date (FR-001). |
| `Answer` | `tinyint` null | 1 Yes, 0 No; only for yes/no goals. |
| `Amount` | `int` null | Only for count and duration goals (minutes). |
| `Sequence` | `bigint` identity | The stored order (FR-009). |

The clustered index is (`GoalId`, `Date`, `Sequence`). There are no check constraints. The mapping enforces that
exactly one of `Answer` and `Amount` is set, and that `Answer` is 0 or 1, and turns a row that breaks this into a
fault with no value.

### `GoalNotes`

| Column | Type | Notes |
|---|---|---|
| `GoalId` | `uniqueidentifier` | Foreign key to `Goals.Id`, cascade delete. |
| `EntryDate` | `date` | The entry key: the commitment's date, the day, or the week's Monday. |
| `Text` | `nvarchar(500)` | Trimmed, 1 to 500 characters. |

The primary key is (`GoalId`, `EntryDate`), so each commitment or period has at most one note (FR-007).

Rows are `GoalRecordRow` and `GoalNoteRow`, both internal and mapped only by `RecordMapping`, which
`RecordStore` and `DemoHistoryStore` use. A note is rebuilt with the public `NoteText.TryCreate`, and a value
with `RecordValue.FromAnswer` or `FromAmount`.

## State transitions

An entry's state at the supplied time `now` (FR-010, FR-012, FR-016). Nothing is stored, and time alone moves it.

```text
Upcoming --(now >= Starts)--> Due --(now >= Ends)--> Pending --(now >= Ends + 48 h)--> final
final = Missed   when it has no record
      = Met      when its records meet the target of its date's version (FR-014)
      = Failed   otherwise
```

A record added, changed or deleted after the entry is final changes the final state at once (a correction,
FR-006). Adding a record to a Missed entry gives Met or Failed. Deleting the only record of a final entry gives
Missed. A note never changes the state (FR-007).
