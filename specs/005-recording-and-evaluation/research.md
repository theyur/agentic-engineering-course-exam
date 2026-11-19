# Research: Recording and evaluation

**Feature**: specs/005-recording-and-evaluation · **Plan**: [plan.md](plan.md)

Inputs: [spec.md](spec.md), the locked [acceptance.md](acceptance.md) and
`tests/HT.AcceptanceTests/Features/F005/recording-and-evaluation.feature`, ADR 0018 (evaluation on read, demo
history loading, the daily refresh), ADR 0016 and ADR 0017 (time zones), and the code of slices 1 to 4.

The spec leaves no `[NEEDS CLARIFICATION]` marker. The product rows (D-1 to D-3) and the technical rows (D-4, D-5)
are decided in its `## Clarifications` and ADR 0018. So this research only chooses *how* to build what the spec
says. Where the spec's words left a detail open, the choice is listed in plan.md "Resolved in the spec"; the owner
accepted all ten, and they are now in the spec.

Each section: **Decision**, **Rationale**, **Alternatives considered**.

## R-01 Evaluation as a pure Domain function (FR-010, FR-012 to FR-016, HP-010)

- **Decision**: a new namespace `HT.Domain.Evaluation` with one static entry point,
  `Evaluator.Evaluate(GoalTarget target, DateTimeOffset starts, DateTimeOffset ends, IReadOnlyList<RecordValue>
  records, DateTimeOffset now)`. It returns an `EntryEvaluation`: the `EvaluationState`, the `Progress`, the grace
  end and whether the entry is final. The rules:
  - `GracePeriod.Length` is `TimeSpan.FromHours(48)`. The grace end is `ends + Length`, computed on UTC
    instants. That is "48 elapsed hours" by construction: a clock change in between changes only how the
    instant reads on the local clock (FR-016, US2-AS13).
  - The state at `now`:
    - `Upcoming` when `starts > now`;
    - `Due` when `starts <= now < ends`;
    - `Pending` when `ends <= now < graceEnd`;
    - otherwise final: `Missed` with no record, else `Met` or `Failed` by the target.

    `now == graceEnd` is final. That matches FR-016's "that instant included" and the edge case of a record saved
    exactly at the end of a grace period.
  - The target (FR-014, "What the target means"). `Of` is the amount, in minutes when the duration unit is hours
    (amount × 60):

    | Kind | Build met when | Reduce met when |
    |---|---|---|
    | commitment, yes/no | the record is Yes | the record is No |
    | commitment, count or duration | amount ≥ `Of` | amount ≤ `Of` |
    | period, yes/no | number of Yes ≥ N | number of Yes ≤ N |
    | period, count or duration | number of records with amount ≥ `Of` is ≥ N | sum of amounts ≤ `Of` (N is 1, slice 4 FR-004) |

  - `Progress` has a `ProgressKind`, a `Value` and an `Of`:
    - `RecordedValue`, for a commitment. `Value` is the amount, or 1 for Yes and 0 for No; it is null without a
      record. `Of` is the amount in minutes or units, or 1 for yes/no.
    - `RecordsReachingAmount`, for a Build period: `Value` is the number of records that reach the amount (or the
      number of Yes) against `Of` = N.
    - `YesRecords`, for a Reduce yes/no period: the number of Yes against at most `Of` = N.
    - `Total`, for a Reduce count or duration period: the sum against at most `Of`.

    A period without records has `Value` 0. Progress is given in every state (FR-012, FR-014).
  - Nothing is stored. Whoever reads an entry calls `Evaluate` with the supplied time, so Pending turns final with
    no action (FR-010, HP-010, ADR 0018).
- **Rationale**: constitution II asks for a deterministic function of stored facts, the version and a supplied
  time. Keeping the function in Domain, with no clock and no culture, makes every boundary of SC-002 a plain unit
  test. The function takes the target of the version in effect for the entry's date. The caller looks that up
  with `GoalTimeline.VersionOf(goal, firstDate)`, so a later version never changes an earlier entry (FR-015).
- **Alternatives considered**:
  - Methods on `Commitment` and `EvaluationPeriod`. Rejected: both are slice-4 records without the target, and
    the same rule would be written twice.
  - Stored results with a job that finalizes them. Rejected by ADR 0018.
  - Computing the grace end in local time (end + 2 local days). Rejected by D-1: it lasts 47 or 49 hours across a
    clock change.
  - A static class named `Evaluation` in the namespace `HT.Domain.Evaluation`. Rejected (critic plan pass 1,
    MINOR-5): inside that namespace, and in the test namespace `HT.UnitTests.Domain.Evaluation`, `Evaluation`
    resolves to the namespace, so `Evaluation.Evaluate` does not compile. The class is `Evaluator`.

## R-02 Records, values and notes in Domain (FR-001, FR-003 to FR-005, FR-007)

- **Decision**: a new namespace `HT.Domain.Records`:
  - `RecordId(Guid)`. The store creates it, never a caller.
  - `YesNoAnswer { Yes, No }`.
  - `RecordValue`: an answer or an amount, never both. It is a `sealed class` with get-only properties and a
    private constructor, not a struct, so no `default` value with neither can exist and `with` cannot make one
    (critic plan pass 2, MINOR-5). Ports and demo records take it as non-null. Input from a user goes only through
    `RecordValue.TryCreate(Measurement, YesNoAnswer? answer, int? amount, out RecordValue, out RecordProblem)`:
    - for yes/no, the answer is kept and an amount is dropped; with no answer, `AnswerMissing`;
    - for count, the amount is kept when it is 0 to 9999, otherwise `CountOutOfRange`, and an answer is dropped;
    - for duration, the amount is kept when it is 0 to 1440 minutes, otherwise `DurationOutOfRange`, and an
      answer is dropped.

    This implements FR-004's "such values are not refused and not stored". A value of only the wrong kind is
    "no value" and gets that measurement's message (plan "Resolved in the spec" 2).

    A stored value is rebuilt by two public factories that need no measurement: `RecordValue.FromAnswer(YesNoAnswer)`
    and `RecordValue.FromAmount(int)`. The second throws `ArgumentOutOfRangeException`, naming no value, outside 0
    to 9999, the widest range of any measurement. `RecordMapping` in `HT.Persistence` calls them, because that
    assembly cannot see Domain internals: `AcSurfaceTests` forbid `InternalsVisibleTo` on the AC surface (critic
    plan pass 1, MAJOR-1). They keep the "never both" invariant. The range of a goal's own measurement is checked
    when the value enters, by `TryCreate`, and the store writes only values that passed it.
  - `GoalRecord(RecordId Id, DateOnly Date, RecordValue Value)`.
  - `NoteText`: a trimmed text of 1 to 500 characters. `NoteText.TryCreate(string?, out NoteText?, out bool
    tooLong)`: null, empty or only spaces give no note; more than 500 characters after trimming gives `tooLong`.
    Length is `string.Length` after `Trim()`, the rule of `AccountName` and `GoalName`.
  - `RecordProblem { None, NothingScheduled, DayNotBegun, AnswerMissing, CountOutOfRange, DurationOutOfRange,
    NoteTooLong, RecordLimitReached }`. These are the date, value, note and limit steps of FR-005, in order.
  - `RecordLimits`: `MaxRecordsPerPeriod = 50`, `MaxCount = 9999`, `MaxMinutes = 1440`, `MaxNoteLength = 500`.
- **Rationale**: the value rules belong to the goal's measurement, so they sit next to the target, without a
  clock. One type for a stored value keeps "an answer or an amount, never both" a type invariant.
- **Alternatives considered**:
  - `bool? Done` instead of `YesNoAnswer`. Rejected: the spec and the acceptance criteria say "Yes" and "No", and
    an enum reads the same in tests and pages (constitution I).
  - One `decimal` value for everything. Rejected: whole numbers only (spec Assumptions), and yes/no is not a
    number.

## R-03 Which entry a date belongs to, and whether its day has begun (FR-002, FR-007)

- **Decision**: `GoalTimeline` gains three members, all pure:
  - `EntryOf(Goal goal, DateOnly date)`, which gives a `CalendarEntry` or null. `CalendarEntry(DateOnly
    FirstDate, DateOnly LastDate, DateTimeOffset Starts, DateTimeOffset Ends)` is one commitment or one period.
    It is built from `TryCalendar(goal, date, date)`:
    - a commitment-based goal gives the commitment of `date`, or null when the day is not scheduled or is before
      the start date;
    - a per-day goal gives the day, or null before the start date;
    - a per-week goal gives the week that contains `date`, or null when that week starts before the goal's first
      Monday on or after its start date. So the days before the first full week belong to no period.

    `CalendarEntry.Key` is `FirstDate`: the commitment's date, the day, or the week's Monday. Records and notes are
    filed under it.
  - `HasBegun(Goal goal, DateOnly date, DateTimeOffset instant)`, which is `CurrentDate(goal, instant) is { }
    current && date <= current`. For a per-week goal, the date itself must have begun, not just its week (US4-AS5).
  - `TryEntries(Goal goal, DateOnly first, DateOnly last, out IReadOnlyList<CalendarEntry> entries)`, which gives
    `TryCalendar`'s commitments or periods as `CalendarEntry` values, so the history and FR-011 handle both
    schedule kinds the same way.

  The record use cases check `EntryOf` first ("Nothing is scheduled on this date.") and `HasBegun` second ("You
  cannot record a day that has not begun."), as FR-002 orders them (US1-AS6: a Tuesday tomorrow on a Mon/Wed/Fri
  goal gives the first message).
- **Rationale**: slice 4's `TryCalendar` already places dates in commitments, days and weeks, with every
  time-zone version and gap or overlap. Reusing it means the record checks, the history and the calendar can
  never disagree on what an entry is.
- **Alternatives considered**: storing the entry key that the page sends. Rejected: the date is the user's
  statement (FR-001, ADR 0016), and the week a record counts in must follow from that date alone (edge case
  "Sunday at 23:59").

## R-04 Use cases: `RecordingService` (FR-001 to FR-011, FR-017, FR-018)

- **Decision**: a new service, `HT.Application.Records.RecordingService`, scoped. It holds the seven use cases of
  this slice: record, change record, delete record, save note, history, what can be recorded now, and recent
  notes. Its constructor is `(IRecordStore records, IGoalStore goals, IAccountStore accounts, TimeProvider clock)`.
  `GoalService` is not changed. Every method:
  1. takes `ActingUser` first and checks, in FR-005 and FR-018 order: not signed in; an administrator; the
     account (missing gives "You are not signed in.", as `GoalService` does); the goal through
     `IGoalStore.FindAsync(owner, id)` ("This goal does not exist."); for change and delete, the record among that
     goal's records; then the date, the value, the note and the limit. The history checks its range after the
     goal;
  2. reads the time once, as `_clock.GetUtcNow()`, and uses that instant for every check and every state of the
     call;
  3. returns a result with `Outcome` and `Message` (ADR 0005). A refusal carries no input value (HP-006).

  The check order per use case is in [contracts/application-api.md](contracts/application-api.md). Two points:
  - **Change** has no date step. The record's date never changes (FR-003), and in this slice versions differ only
    in their time zone (FR-015), so a stored record's date always passes FR-002 again (plan "Resolved in the spec"
    1). The note is checked after the value (AC-039 rows 05 and 06, AC-064).
  - **Delete** checks only up to the record. Nothing else is given.
- **Writes return the entry**. Record, change, delete and save note return the entry after the write, as the
  history gives it (FR-006), plus `IsCorrection`. That flag is whether the entry was final at the supplied time:
  `now >= graceEnd`. Finality depends only on time, not on records (FR-012), so this is the same "before" and
  "after" the write. Deleting the only record of a Due or Pending entry is therefore not a correction (edge case).
  A record call also returns the `RecordId` it wrote.
- **What can be recorded now** (FR-011): the entries of `TryCalendar(goal, current − 9, current)` whose state is
  `Due` or `Pending`, newest first by `FirstDate` (plan "Resolved in the spec" 5). The window always holds every
  Due and Pending entry, because at most 48 hours have passed since a Pending entry's end:
  - the current week (Monday to `current`);
  - the previous week, Pending until Wednesday 00:00;
  - an extra day for a gap or overlap (FR-027).

  A goal with no current date has none. After a move east both the old and the new day are Due while they overlap
  (AC-046, AC-067).
- **Recent notes** (FR-008): the store gives the keys of the 5 notes with the latest keys. Each one's entry comes
  from `EntryOf(goal, key)`. Then one `ReadAsync` per entry, from its `FirstDate` to its `LastDate`, gives its
  records and note, and so its state. That is at most 6 store reads in all, and none of them spans the dates
  between two notes.
- **Rationale**: a separate service keeps `GoalService`'s constructor and its locked F004 callers unchanged
  (FR-019). It also groups the use cases that share one owner-scoped port. The tests need the time supplied, and
  `TimeProvider` is how slices 1 to 4 supply it (ADR 0006; the acceptance tests register their own).
- **Alternatives considered**:
  - New methods on `GoalService`. Rejected: its constructor would need `IRecordStore`, which breaks every
    hand-built `GoalService` in the unit, UI and locked AC support code.
  - A time parameter on each method. Rejected: slices 1 to 4 use `TimeProvider`, and the acceptance tests already
    drive it.

## R-05 The record store port and its transactions (FR-003, FR-005, FR-017)

- **Decision**: a new port, `HT.Application.Records.IRecordStore`. It is public for the AC reference export, and
  an architecture rule keeps HT.Ui away from it (ADR 0010). Every member takes `AccountId owner` and `GoalId goal`
  first, and acts only when the goal is one of the owner's, in the same statement (constitution III). It has no
  member without an owner. Its members are in [data-model.md](data-model.md#irecordstore-htapplicationrecords-new):
  - add a record;
  - find a record's date;
  - change a record;
  - delete a record;
  - save a note;
  - read entries.

  **Writes** each run in one transaction:
  1. They lock the goal row with `UPDLOCK, HOLDLOCK` and `WHERE Id = @goal AND OwnerId = @owner`. No row gives
     `GoalNotFound` and writes nothing.
  2. Then they act:
     - **add**, for a commitment: when the entry already has a record, its value is replaced in place. It keeps
       its identifier and stored order (FR-003; plan "Resolved in the spec" 3). Otherwise a record is inserted.
     - **add**, for a period: the period's records are counted, and 50 gives `LimitReached` and writes nothing.
       Otherwise a record is inserted.
     - **change** and **delete** act on `WHERE Id = @record AND GoalId = @goal`. No row gives `RecordNotFound`.
       That covers a record of another goal, of another account, or of nothing (US6-AS2), and a record deleted in
       the meantime.
     - A note given with a record: when it has text, it is upserted under the entry key in the same transaction
       (FR-007).
     - **save note**: upsert, or delete when there is no text.
  3. They read the entry's records and note, in the same transaction, and return them.

  The lock serializes every write on one goal. So two concurrent records on one commitment leave one record, and
  two concurrent adds on a period with 49 records leave 50. The integration tests force both races.
- **Reads** are no-tracking and join `Goals` on `OwnerId`. `ReadAsync(owner, goal, from, to)` reads, in two
  queries, the records with dates from `from` to `to`, in date order and then stored order (`Sequence`), and the
  notes with keys from `from` to `to`. The caller passes `from` as the first entry's `FirstDate` and `to` as the
  last entry's `LastDate`. Every note key is an entry key, so the notes in that range are exactly those of the
  entries read, none lost and none extra (critic plan pass 1, MINOR-2). `ReadRecentNoteKeysAsync` is a
  `TOP (@count)` by key, descending.
- **A time-zone change in between** cannot move a recorded entry. A record's date is on or before the goal's
  current date, and a time-zone change only adds or replaces versions after that date (slice 4 R-06).
- **Faults**: no statement raises a database error that carries a value. The values written already satisfy the
  Domain's ranges, and the note's length fits `nvarchar(500)`. Any database fault becomes an exception that names
  the SQL error number only, as `GoalStore` does (HP-006).
- **Rationale**: the same pattern as slice 4's `GoalStore`: owner scoping in every statement, short
  transactions, and conditional statements instead of errors. Locking the goal row, rather than the account row,
  keeps record writes of different goals apart.
- **Alternatives considered**:
  - An `OwnerId` column on the record and note tables. Rejected: the join on the owner's goal already scopes
    every statement, and a copy of the owner could disagree with the goal.
  - A unique index on (goal, date) for commitments. Rejected: periods share the table and allow several records
    on one date. The lock already makes "at most one" hold.
  - Optimistic concurrency with a row version. Rejected: the limit check counts rows, which a version on one row
    does not protect.

## R-06 Tables and migration (data-model.md "Tables")

- **Decision**: one migration, `Records`, adds two tables:
  - `GoalRecords`:
    - `Id` (`uniqueidentifier`, primary key, nonclustered);
    - `GoalId` (foreign key to `Goals`, cascade delete);
    - `Date` (`date`);
    - `Answer` (`tinyint` null: 1 Yes, 0 No);
    - `Amount` (`int` null);
    - `Sequence` (`bigint` identity: the stored order).

    The clustered index is (`GoalId`, `Date`, `Sequence`), the order the history reads.
  - `GoalNotes`:
    - `GoalId` (foreign key, cascade delete);
    - `EntryDate` (`date`: the entry key of R-03);
    - `Text` (`nvarchar(500)`).

    Its primary key is (`GoalId`, `EntryDate`), so a commitment or period has at most one note.

  Neither table has check constraints. The mapping and the Domain types keep the "exactly one of `Answer` and
  `Amount`" rule (project convention, slice 4 R-08). The cascade exists only for consistency: no use case
  deletes a goal yet (slice 8).
- **Rationale**: dates, not instants, are stored (FR-001, ADR 0016). The entry, its instants and its state are
  derived on read. A record's entry is its date's, so no entry key is stored on a record, and moving between
  versions never re-files records.
- **Alternatives considered**:
  - A per-entry row that holds the note and a record count. Rejected: a second copy of what the records already
    say.
  - Storing `Done` as `bit`. Rejected: `tinyint` maps the enum directly, and the mapping rejects other values.

## R-07 History, progress and the views (FR-009, FR-011, FR-014)

- **Decision**: `GetHistoryAsync(actingUser, goalId, first, last)`:
  1. checks the goal, then the range with `GoalTimeline.TryCalendar` ("Choose a range of 1 to 366 days.", the text
     slice 4 already has as `GoalTexts.RangeOutOfBounds`);
  2. calls `ReadAsync` once, from the first entry's `FirstDate` to the last entry's `LastDate` (R-05). A week that
     overlaps the range gives all of its records, also those outside the range (plan "Resolved in the spec" 4);
  3. returns a `GoalHistory`:
     - the goal's `Measurement`, `BuildOrReduce`, the unit (count unit, or the duration unit with minutes), and
       `GoalScheduleKind` and `SchedulePeriod`, so the page can format progress without a second call;
     - the entries in date order. `GetRecordableAsync` returns the same `GoalHistory` type, but its entries are
       newest first (R-04).

     Each `HistoryEntry` has:
     - `FirstDate`, `LastDate`, `Starts`, `Ends`;
     - `State`;
     - `GraceEnds`: set only while the entry is Due or Pending (FR-009), null when it is Upcoming or final (the
       acceptance criteria's "no grace end");
     - `Records`, as `RecordView(Id, Date, Answer, Amount)`, in date order and then stored order;
     - `Progress`;
     - `Note`;
     - `IsCorrection`, whether a change now would be a correction (FR-009).

  The evaluation calendar of slice 4 (`GoalService.GetCalendarAsync`) stays as it is, without states (FR-019,
  AC-063).
- **Rationale**: one store read and one Domain function per entry. 366 entries, with at most 50 records each, are
  computed in memory at demo scale.
- **Alternatives considered**: a history that keeps only the records with dates in the range. Rejected: a
  per-week period's progress would then depend on the range, which FR-014 does not allow. The acceptance criteria
  avoid partial weeks ("Ranges" convention), so either choice passes them.

## R-08 Demo history loading (FR-030, FR-031, HP-007)

- **Decision**: a new operator service, `HT.Application.Operator.DemoHistoryService`, with
  `LoadAsync(string? demoPassword, CancellationToken)`. Its constructor is `(IAccountStore accounts,
  IDemoHistoryStore demoHistory, TimeProvider clock, InstallationTimeZone defaultZone)`. It uses a new port,
  `IDemoHistoryStore`, with one member, `CreateDemoHistoryAccountAsync`. `DemoDataService` and `IDemoDataStore` are
  not changed (FR-031). Steps:
  1. With no demo password (null, empty or only spaces): "no demo password configured". Without at least 8
     characters and one digit (`PasswordRule.IsMet`): "demo password does not meet the password rule". Both create
     nothing (AC-061, AC-069).
  2. Reads `now` once. R is `defaultZone.DateAt(now)`.
  3. When `demo4@example.com` already has an account (looked up ignoring case, slice 1 FR-003): "already exists",
     with no goal or record result. The account's data is neither read nor changed (ADR 0013).
  4. Otherwise it builds, in `DemoHistory.For(R)`, the three goals of FR-030. Each is a `DemoHistoryGoal`: a slice-4
     `DemoGoal`, with the catalog habit name and `Published` as its only accepted state, and the start date
     R − 28; plus its `DemoRecord(Date, RecordValue)` list and its `DemoNote(EntryDate, NoteText)` list:
     - **Reading**: Build, duration 20 minutes, all seven days.
       - Records: 20 on each date R − 28 to R − 3, except R − 6, which is 12 with the note "Fell asleep early.",
         and R − 10, which has no record and the note "Away for the weekend.".
       - Then 25 on R − 2, and nothing on R − 1 or R.
     - **Sugar**: Reduce, count at most 1 `sweet`, 1 time per day.
       - Records on R − 28 to R − 3: 0 when k is even, 1 when k is odd.
       - Except R − 4, which is 3 with the note "Birthday cake at work.", and R − 12, which has no record.
       - Nothing on R − 2 to R.
     - **Yoga**: Build, yes/no, 3 times per week.
       - The current week's Monday is R minus its days since Monday, so a Sunday R belongs to the week ending on R.
         Week 1 starts 7 days before it, week 2 14 days before, week 3 21 days before, and "the week before week 3"
         28 days before.
       - Week 1 has Yes on Monday, Wednesday and Friday. So does the week before week 3, when its Monday is on or
         after R − 28, which happens only when R is a Monday.
       - Week 2 has Yes on Monday and Wednesday, and the note "Knee was sore." under its Monday.
       - Week 3 has no record, and the current week has none.

     `DemoHistory` has unit tests for R on each weekday, against FR-030's list and against "every record is on a
     date before R". It also exposes what smoke check 6 expects (R-10): `GoalNames`, the three display names in
     creation order, and the constants `PartialMinutes` (12) and `ReadingNote` ("Fell asleep early."). So no
     expected text is typed twice.
  5. Calls the store with the email, the demo password, a profile with no name, the default zone and `now` as
     the creation time, and the goals. In one transaction, the store:
     - creates the registered user;
     - writes each goal whose catalog habit is Published, found by its name key, with settings version 1 in the
       profile's zone, attempt 1, and the goal's records and notes;
     - leaves the other goals out, with no error, together with their records and notes.

     A taken email gives `EmailTaken` and creates nothing, which the service reports as "already exists".
  6. For a created account, the results are "created", "demo goals added" and "demo records added", also when
     goals were left out (FR-030, AC-060). The results name the email and these texts only (AC-059).
- **Rationale**:
  - ADR 0018 chose a separate task.
  - Keeping `DemoDataService`, `IDemoDataStore` and their constructors unchanged leaves the locked F001, F002 and
    F004 support code compiling unchanged (plan "Risks").
  - A separate port keeps each port's coverage map honest (R-10).
  - Writing the goals directly, with start date R − 28, skips slice 4's "not in the past" check on purpose
    (FR-030).
- **Alternatives considered**:
  - A new member on `IDemoDataStore`. Rejected: any implementer of that port outside the solution (the locked AC
    support code, which may not be read) would stop compiling. A second port costs nothing.
  - Reusing `CreateDemoAccountAsync`, then adding records in a second transaction. Rejected by ADR 0013: an account
    and its demo data are one unit.

## R-09 Host: the third startup task and its log (HP-007)

- **Decision**: `StartupTasks.RunAsync` runs `RunStepAsync("Demo history loading", …)` after demo loading. It
  runs in its own scope with its own `try/catch`. A fault is logged as "Demo history loading failed; the app
  starts without it", with the exception and no secret. Log lines, with source-generated log methods and the
  category `HT.Ui.Operator.StartupTasks`:
  - a refusal: `Demo history loading: {Result}`;
  - the account: `Demo account {Email}: {Result}`, the same template as demo loading's account line;
  - for a created account: `Demo goals {Email}: {Result}`, then `Demo records {Email}: {Result}`.

  There is no custom-habit line. The full order is in [contracts/host.md](contracts/host.md#operator-log).
  `StartupLogTests` and `StartupTasksDemoTests` change to expect demo4's lines after demo loading's. Their
  demo-loading lines stay exactly as they are.
- **Rationale**: the existing step pattern (ADR 0007). The account line reuses its template, so log readers and
  the tests treat demo4 like the other demo accounts.
- **Alternatives considered**: one combined line. Rejected: HP-007 asks for the account, goals and records lines,
  as demo loading writes them.

## R-10 Smoke check and its coverage (HP-008)

- **Decision**: the smoke check gains check 6, `demo history demo4@example.com`, after check 5. It confirms
  everything SC-005 names: Met, Failed, Missed, Pending and Due, a partial amount and a note. HP-008's word list
  is a minimum ("MUST also"), and SC-005 asks for more, so check 6 covers both (critic plan pass 1, MAJOR-2). It
  signs in on a new client with the demo password, then checks 11 expected texts:
  1. `GET /goals` shows the 3 names of `DemoHistory.GoalNames` ("Reading, 20 min daily", "Sugar, at most 1 sweet a
     day", "Yoga, 3 times a week"). Each goal page's link is read from that page with `PageReader`.
  2. The History card of "Reading, 20 min daily" contains 6 texts: "Met", "Failed", "Pending", "Due today", the
     partial amount `RecordingText` gives for `DemoHistory.PartialMinutes` ("12 min"), and `DemoHistory.ReadingNote`
     ("Fell asleep early.").
  3. The History card of "Yoga, 3 times a week" contains 2 texts: "Failed" and "Missed".

  The texts are matched only inside the History card (critic plan pass 2, MINOR-1). A new
  `PageReader.ReadCardText(html, title)` finds the first `<section class="ht-card …">` whose `h2.ht-card__title` is
  "History" (the design's card and title classes, so no new class is needed). It returns that section's text up to
  its `</section>`, with tags removed and HTML entities decoded. Each expected text is then found as an ordinal
  substring of it. The Settings card's words cannot satisfy them. The goal page links come from a new
  `PageReader.ReadGoalLinks(html)`, which pairs each `a.ht-goalitem__name`'s text with its `href`, the anchor
  `ReadGoalNames` already uses. On any weekday they lie in the first 10 rows:
  - for Reading: R is Due, R − 1 and R − 2 are Pending, R − 3 is Met, and R − 6 is Failed with 12 minutes and the
    note, in the 7th row;
  - for Yoga: week 2 is Failed and week 3 Missed, in the 3rd and 4th rows.

  Missed is not checked on Reading, because R − 10 is its 11th row.

  Output, as check 5 writes it (critic plan pass 1, MINOR-3):
  - `PASS demo history demo4@example.com (11 of 11)` when every text is found;
  - otherwise `FAIL demo history demo4@example.com: <reason>`, with the first failing part's reason:
    - `GET /goals gave <status>`;
    - `GET goal page <n> gave <status>`, never the path, which holds an identifier read from the page;
    - `no link for goal page <n>`;
    - `<found> of <expected> goal names shown`;
    - `<found> of <expected> texts on goal page <n> shown`.

    n is the goal's position in `GoalNames`.

  No goal name, note, amount or other page content is printed, as slice 3's smoke-check contract requires. `SmokeCheckHostTests` gains `demo history demo4@example.com` in its exact
  list of check names, and its expected line (MINOR-1).

  `SmokeCheckCoverage.Entries` gains a `Port` column. It gets one entry per data parameter of
  `IDemoHistoryStore.CreateDemoHistoryAccountAsync`: `email` and `password` (the sign-in), `profile` (the sign-in;
  the profile has no name to show), and `goals` (the goal names, and the states on the two goal pages). The
  coverage test reflects over both demo ports. Slice 3's coverage table in
  `specs/003-demo-hosting/contracts/smoke-check.md` is extended in the same change. `RunnerBehaviourTests.GoodSite()`
  scripts the new pages.
- **Rationale**: HP-008's list, and slice 3's coverage rule, which says that a slice that adds demo data extends
  the checks.
- **Alternatives considered**:
  - Only HP-008's words. Rejected: SC-005 says the smoke check confirms Due, a partial amount and a note too.
  - Checking every state on every goal page, Sugar included. Rejected: SC-005 is met with the two pages above, and
    Sugar shows its states through the same History card.

## R-11 The daily scheduled refresh (HP-009)

- **Decision**: `.github/workflows/deploy-demo.yml` gains `on.schedule` with `cron: '20 0 * * *'` (00:20 UTC),
  and a first job, `gate`, before the existing job `demo`:
  1. **`gate`** has no `environment`, no Azure sign-in and no secret. It runs one step, "0.1 Check the trigger and
     the demo end date", with two outputs:
     - `mode`: `inputs.mode`, or `refresh` on a scheduled run, where `inputs` is empty;
     - `run`: `false` on a scheduled run whose UTC date is after 2027-01-31, otherwise `true`. A manual run is never
       skipped.

     With `run` false, it writes the summary "Demo refresh: skipped after 2027-01-31".
  2. **`demo`** gets `needs: gate` and `if: needs.gate.outputs.run == 'true'`. A skipped job never starts, so it
     never enters the `demo` environment and never signs in to Azure. A job-level `environment` is resolved
     before any step runs, which is why a step-level gate would not be enough (critic plan pass 1, MAJOR-3). The
     run ends as succeeded with `demo` skipped.

     Every step of `demo` reads `env.MODE`, set from `needs.gate.outputs.mode` at job level, instead of
     `inputs.mode`.
  3. The concurrency group `deploy-demo` stays at workflow level, so a scheduled refresh waits for a running manual
     deployment.

  Scheduled runs use the default branch, `master`, which the `demo` environment's branch policy already allows.
  `specs/003-demo-hosting/contracts/deploy-workflow.md` and `docs/operations/hosted-demo.md` describe the trigger,
  the date gate, and the removal of the trigger in "Teardown after 2027-01-31".
- **Rationale**: ADR 0018. 00:20 UTC is shortly after midnight, and GitHub delays the top of the hour most. R is
  the UTC date there, because provisioning sets no default time zone (HP-009).
- **Alternatives considered**:
  - A separate workflow file for the schedule. Rejected: it would duplicate refresh mode's steps, or call the
    deployment workflow with its secrets.
  - Azure-side scheduling, such as a WebJob or an Automation runbook. Rejected: new infrastructure (constitution
    VI), and the refresh must run the smoke check, which lives in the workflow.
- **Known limits**:
  - GitHub may delay a scheduled run, or drop it under load.
  - GitHub turns off the scheduled workflows of a public repository after 60 days with no repository activity. For
    the schedule to last until 2027-01-31, the repository needs activity on or after 2026-12-02. The exam on
    2026-12-14 (docs/roadmap.md) makes that likely, but it is not guaranteed (critic plan pass 1, MINOR-6). A turned
    off workflow is turned on again only by hand: "Enable workflow" on its page in the Actions tab, or
    `gh workflow enable deploy-demo.yml`. A push alone does not do it (critic plan pass 2, MINOR-4). The Operator
    guide says this, and says to check the workflow's last scheduled run when reviewing the demo. Until it is turned
    on again, a manual refresh keeps the demo current.
- **The job name**: `demo`'s `name: Demo (${{ inputs.mode }})` would read "Demo ()" on a scheduled run. It becomes
  `Demo (${{ needs.gate.outputs.mode }})`, like every other use of the mode (critic plan pass 2, MINOR-4).

## R-12 Pages, components and the shared formatter (HP-001 to HP-005, HP-011)

- **Decision**:
  - **The goal page** `/goals/{id}` keeps its static shell. It looks up the goal and renders Not found with 404
    for an administrator, an unknown goal and another account's goal, as slice 4 does. For the owner's goal it
    renders one interactive component, `GoalDetailBody` (`@rendermode InteractiveServer`, with prerendering on),
    with the goal identifier as its only parameter. The component re-reads the goal and its history through
    `GoalService` and `RecordingService` in its own scope. It renders:
    - the page head with "Record";
    - the slice-4 tags, Notice and Settings card;
    - the History card;
    - the Recent notes card;
    - the dialogs.

    Slice 2's render-mode rule applies: interactive only where the page reacts before submit, and the
    RecordDialog shows the progress while the field changes (HP-002). Prerendering puts the History rows into the
    first HTML, which the smoke check reads (R-10).
  - **Where the page gets the goal's current date** (critic plan pass 2, MINOR-2): `GoalDetailBody` first calls
    `GetRecordableAsync`, which it needs anyway for "Record". That result's `GoalHistory.CurrentDate` is D. It then
    calls `GetHistoryAsync(D − 365, D)`, 366 days. The two calls read the clock separately. When midnight passes
    between them, the history ends a day early until the next render, and nothing is wrong or missing beyond that.
    Entries that are still Upcoming are not shown. `GoalService` and `GoalDetails` stay unchanged.
  - **A goal that has not started** (D is null): no "Record", no History card and no Recent notes card. The page
    shows slice 4's head, Notice and Settings card, which already say when the goal starts.
  - **History card**: the begun entries of the last 366 days up to the goal's current date, newest first, the
    first 10 with "Show all N →", which expands the card in place (plan "Resolved in the spec" 7). The columns are
    Date, Recorded, Result (StatusChip) and Note, with the row actions of HP-001. Under the table: "Corrections
    are evaluated with the rules of that date.".
  - **Recent notes card**: `GetRecentNotesAsync`, each note with its date (a week: its first and last date) and its
    StatusChip.
  - **Components**: 7 new ones (critic plan pass 1, MINOR-8). Each design component becomes one Razor component
    (constitution IV):
    1. `StatusChip`, in `Components/Shared`. It uses only the design's classes `ht-chip ht-status
       ht-status--{met|failed|missed|pending|due}` (`design/components/StatusChip.html`);
    2. `GoalDetailBody`, in `Components/Records`;
    3. `HistoryCard`;
    4. `RecentNotesCard`;
    5. `RecordDialog`;
    6. `RecordFields`, from `RecordForms`: Yes | No with `SegmentedControl` and nothing selected; the existing
       `Stepper` with the unit; minutes;
    7. `NoteDialog`.

    `HtDialog`, `DataTable`, `TextArea`, `HtButton` and `Notice` are reused.
  - **Design fidelity of the new texts** (critic plan pass 1, MINOR-4):
    - The NoteDialog title is the design's "Add note · {goal name}". The design has no other title, so there is no
      "Edit note" variant.
    - A week's date is the design's "10 – 16 Aug" (`PageGoalDetail.html`), or "28 Sep – 4 Oct" across a month
      end. It is written by a new formatter in slice 4's `DateTexts`, `WeekRange(DateOnly first, DateOnly last)`,
      and unit-tested.
    - Every repeated row control has an accessible name that starts with its visible text (WCAG 2.5.3, Label in
      Name) and names its row (critic plan pass 2, MINOR-3):
      - a period's record button: "12 min, edit record 1 of Tue 17 Nov", where the number counts from 1 in the
        row;
      - "Add record": "Add record for Tue 17 Nov";
      - "Edit": "Edit record of Tue 17 Nov";
      - "Add note": "Add note for Tue 17 Nov";
      - a note button: the note's text, then ", edit note of Tue 17 Nov".

      A week's row uses its `WeekRange` text in place of the date.
  - **Formatter**: one shared, unit-tested `HT.Application.Records.RecordingText`:
    - the state words of HP-004: "Met", "Failed", "Missed", "Pending", "Due today" for a commitment or per-day
      period, and "Due this week" for a per-week period. Upcoming has no chip in this slice;
    - the progress texts of HP-011: "12 of 20 min", "2 of 3 times", "1 of at most 2 cups";
    - the Recorded cell: "Yes", "No", "12 min", "3 sweet" (the unit as stored, never inflected);
    - the "For" labels, with "Pending, N h left": the whole hours, rounded up, from the supplied time to the
      grace end (plan "Resolved in the spec" 8);
    - the entry dates, through slice 4's `DateTexts` (`ShortDate`, and the new `WeekRange`).

    The pages only call it. A Reduce goal's Met is "Met" (constitution I). This Application formatter takes the
    goal's measurement and unit from `GoalHistory`, so it never needs a goal name.
  - **New goal** (HP-005): the help line "You can record partial amounts; the target decides the result." in
    "How do you measure it?".
- **Rationale**: the spec's design list, constitution IV, and slice 2 and slice 4's render rules. One interactive
  island keeps 404 handling static and still lets the dialogs react.
- **Alternatives considered**:
  - The whole page interactive. Rejected: slice 4's Not found with 404 for another account's goal relies on the
    static render.
  - Static forms that post the RecordDialog. Rejected: HP-002 shows the progress under the field before saving.

## R-13 Architecture rules added (constitution V)

- **Decision**: the rules gain:
  - **Domain** (`DomainDependencyTests`, `ClockReadTests`): `HT.Domain.Records` and `HT.Domain.Evaluation` read no
    clock, time zone or culture, and depend only on `HT.Domain` and the BCL. The existing assembly-wide rules
    already cover them. A test names the two namespaces, so that moving a type out of them fails.
  - **Ports**:
    - every member of `IRecordStore` takes `AccountId` as its first parameter and `GoalId` as its second
      (`UseCaseParameterTests`' owner rule), with no exception;
    - `IRecordStore` and `IDemoHistoryStore` are used by no HT.Ui type (`UiDependencyTests`);
    - `IDemoHistoryStore` is used by no HT.Application type outside `HT.Application.Operator`;
    - the existing `IGoalStore` fence in `ApplicationDependencyTests`
      (`OnlyTheGoalTypesTheThreeHabitServicesAndTheProfileServiceMayDependOnTheGoalStorePort`) also allows
      `HT.Application.Records.RecordingService`, which reads the goal through `IGoalStore.FindAsync`. The test is
      renamed for it (critic plan pass 1, MINOR-1). `DemoHistoryService` does not use `IGoalStore`, so the fence for
      `HT.Application.Operator` stays.
  - **Use cases**: every public method of `RecordingService` takes `ActingUser` first. `DemoHistoryService` is used
    by no HT.Ui type outside `HT.Ui.Operator`.
  - **Public surface** (`PublicSurfaceTests`): no NodaTime type on the new public types. The persistence rows and
    the store are internal.
- **Rationale**: constitution V, "The first change that adds production types to a layer MUST add the ArchUnitNET
  rules that cover that layer", and constitution III's "no lookup by identifier alone".
- **Alternatives considered**: none. These are the slice-4 rules, extended to the new types.

## R-14 Tests

- **Unit** (`HT.UnitTests`):
  - **`Domain/Evaluation`**: every Build or Reduce × yes/no, count or duration × commitment, per-day or per-week
    combination. Each combination is tested at, just below and just above its target, including duration in hours
    (59, 60, 61 minutes), Reduce amount 0, N greater than 1, and records above the target. The states are tested
    at `starts − 1 tick`, `starts`, `ends − 1 tick`, `ends`, `graceEnd − 1 tick` and `graceEnd`. The clock changes
    are tested in Europe/Kyiv on 2026-10-24/25 and 2027-03-27/28 (23:00 and 01:00 deadlines). Missed with no
    record is tested for Reduce. Progress is tested in every state (SC-002).
  - **`Domain/Records`**: `RecordValue` (the ranges 0, 9999, 10000, −1; 1440, 1441; dropped values),
    `NoteText` (500 and 501 after trimming, padded, all spaces), and `GoalTimeline.EntryOf` and `HasBegun`
    (unscheduled day, before the start, the mid-week start, Sunday 23:59 and Monday 00:00, the overlap after a
    move east).
  - **`Application/Records`**: `RecordingService` with `FakeRecordStore` and the slice-4 fakes. The check order of
    every use case is tested, one refusal at a time. Negative tests for another registered user, an administrator
    and the anonymous visitor are tested for each of the seven use cases (SC-004). Also tested: the correction flag,
    corrections equal to on-time records (SC-003), FR-011's window, and recent notes.
  - **`Application/Operator`**: `DemoHistory.For(R)` for R on each weekday, and `DemoHistoryService`'s order and
    results.
  - **`RecordingText`**: every text.
- **Integration** (`HT.IntegrationTests/Records`, real SQL Server, ADR 0006):
  - owner scoping of every `IRecordStore` member against another owner's goal and record;
  - the stored order;
  - replace-in-place for commitments;
  - the 50 limit;
  - the two races of R-05, forced with two contexts and a barrier;
  - notes, upsert and delete;
  - the migration on `HT-IT-MIGRATE`;
  - demo history loading on a reset database: the account, goals, records and notes, a goal left out when its
    habit is not Published, and the rollback when a write fails;
  - HP-006 log capture: no note, value, unit, goal or habit name in any log line or exception of these tests
    (SC-006).
- **UI** (`HT.UiTests`):
  - bUnit for `StatusChip`, `RecordDialog` (the fields per measurement, the Reduce help text, the note help, one
    refusal at a time, focus on open, Esc), `NoteDialog`, `HistoryCard` (row actions per HP-001, "Show all") and
    `RecentNotesCard`;
  - host tests: the goal page's 404 for another account's goal and an administrator, and the History rows in the
    prerendered HTML;
  - `StartupLogTests` and `StartupTasksDemoTests` with demo4's lines;
  - the New goal help line;
  - `SmokeCheck` runner and coverage tests;
  - `DateTexts.WeekRange`, including a week across a month end.
- **Existing tests this slice changes** (critic plan pass 1, MINOR-1). Each is updated in the step that changes
  the behaviour it pins:
  - `HT.ArchitectureTests/ApplicationDependencyTests`: the `IGoalStore` fence allows `RecordingService` (R-13);
  - `HT.UiTests/Host/SmokeCheckHostTests`: the exact list of check names and the expected lines gain `demo history
    demo4@example.com` (R-10). The failing-password case, which today expects a `FAIL` line for every email of
    `DemoDataService.DemoEmails`, also expects `FAIL demo history demo4@example.com: …` (critic plan pass 2,
    MINOR-6);
  - `HT.UiTests/SmokeChecks/RunnerBehaviourTests`: `GoodSite()` scripts demo4's sign-in, `/goals` and the two goal
    pages, and the runner's line order gains check 6. New cases cover each `FAIL` reason of R-10;
  - `HT.UiTests/SmokeChecks/CoverageTests`: the entries gain the port column and the 4 new rows (R-10);
  - `HT.UiTests/Components/Pages/NewGoalPageTests` (line 193): `Assert.DoesNotContain("partial", …)` becomes a check
    of the HP-005 help line, while "excuse" stays absent;
  - `StartupLogTests` and `StartupTasksDemoTests` (R-09), and the slice-4 goal-page tests (spec "Slice 4
    requirements and tests this slice changes").
- **Architecture**: R-13.
- **Acceptance**: the locked F005 feature, through `/speckit-ac-tests-write` and `/speckit-ac-tests-analyze`, which
  the owner starts after `/speckit-implement`. F001, F002 and F004 run unchanged (FR-019).
- **HP-010**: a unit test and an acceptance item (AC-013) read an entry after its grace period, with no call in
  between, and get Missed. A test also checks that no hosted service is registered in the host.
