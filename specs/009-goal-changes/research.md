# Research: Goal changes

Phase 0 of [plan.md](plan.md). Each item records a decision, why it was chosen and the alternatives considered. The
product rules come from [spec.md](spec.md) and [ADR 0025](../../docs/adr/0025-goal-change-rules.md) (D-1, D-2 and
its two consequences) and [ADR 0026](../../docs/adr/0026-demo-goal-change-loading.md) (D-3). Nothing here decides a
product question again. No `[NEEDS CLARIFICATION]` remains.

The code facts below were read from the tree at `1e3eea0`/`ec5f576` (slice 7 shipped):

- `Goal` holds one `GoalAttempt` and one ordered list of `GoalSettingsVersion`s. Its constructor checks that only the
  time zone differs between versions. `Goal.Target` (internal) is the first version's target.
- `GoalTimeline` derives every date fact from `Goal.StartDate`, `Goal.Target` and `Goal.Versions`: `CurrentDate`,
  `TryCalendar`, `TryEntries`, `EntryOf`, `HasBegun`, `EntriesThrough`, `NextCommitment`, `FirstWeekStart` and
  `ChangeOf` (the time zone change).
- Every reader of slices 5 to 7 goes through those members:
  - `RecordingService` (record, note, history, what can be recorded now, recent notes);
  - `GoalEvaluation.OfAsync`, which every report shares;
  - `GoalListItems`;
  - `GoalService.DetailsOf` and `GetCalendarAsync`;
  - `ProfileService`'s time zone change;
  - `ExcusalService` and `PauseService`.
- Storage: `Goals`, `GoalSettingsVersions` (key goal + applies-from) and `GoalAttempts` (key goal + number, only a
  start date). The goal store serializes goal creation and time zone changes on the account row's update lock and
  the account's `GoalsRevision`. The record store serializes record, note, excusal and pause writes on the goal row's
  update lock. Both `GoalSettingsVersions` and `GoalAttempts` cascade from `Goals` (HtDbContext.cs:138-139).
- Four more assumptions that this slice breaks on purpose (critic pass 1):
  - **One target per goal.** `GoalHistory` carries one goal-level target, measurement, unit and schedule kind, built
    from the version of the current date (`EntryViews.HistoryOf`). The RecordDialog, `RecordPreview`,
    `RecordingText.Recorded`, the History card, the NoteDialog and the Today rows read it for every entry. Only the
    evaluation takes each entry's own target. Resolved in R-07.
  - **Every stored record and note belongs to an entry.** `RecordingService.StoredEntryOf` and `NoteEntryOf` throw
    otherwise. Resolved in R-05 and R-07.
  - **One schedule kind per calendar.** `EvaluationCalendar` and the public `GoalCalendarView` hold one `Kind`, and
    `TryEntries` keeps the list of that kind. Resolved in R-02.
  - **A name with no time.** `GoalText.Name(goal, habitName)` names the goal by the first version's target and takes
    no instant. Resolved in R-08.

## R-01 Attempts in the Domain: a goal holds its attempts, each with its own settings versions

**Decision.**

- `Goal` holds an ordered list of attempts. Each attempt has:
  - its number;
  - its start date;
  - its own settings versions;
  - once ended, a `GoalEnding`: the ending date, the `Outcome` and the optional `EndingNote`.

  The last date is not stored. `GoalTimeline` derives it (FR-008, FR-026).
- A new public constructor takes the attempts as `GoalAttemptVersions` (an attempt and its versions).
- The slice 4 constructor `Goal(id, owner, habit, startDate, ownName, attempt, versions)` stays, with its checks, and
  builds a goal with that one attempt.
- Existing members keep their meaning for a goal with one attempt:
  - `StartDate` is attempt 1's start date (FR-001's "goal's start date");
  - `Attempt` is the latest attempt;
  - `Versions` are the latest attempt's versions.
- New members:
  - `Attempts`;
  - `IsEnded` (the latest attempt has an ending);
  - `VersionsOf(number)`.
- The constructor's invariants:
  - attempt numbers run 1, 2, … with no gap;
  - every attempt but the latest is ended;
  - attempt 1 keeps slice 4's rules: its first version applies from the goal's start date, and the attempt does not
    begin before it. Every later attempt's first version applies from that attempt's start date. Within an attempt
    the dates strictly increase;
  - all of an attempt's versions have the same schedule kind: fixed days, times per day or times per week (D-1);
  - a later version of a per-week attempt starts on a Monday;
  - each attempt starts after the previous attempt's ending date (FR-012);
  - an ended attempt has no version after its last date except its first (FR-009).
- **`StartDate` stays attempt 1's date, and nothing reads it as "the first date" any more.** A later attempt can start
  before it: attempt 1 from 23 Nov, ended on 17 Nov before it began, attempt 2 from 18 Nov (AC-035). So every reader
  that used `goal.StartDate` as the goal's first date or as a fallback date switches to the attempt it means:
  - `GoalTimeline.CurrentDate` (the candidate filter), `EntriesThrough` (its first window), `NextCommitment`'s scan
    start and `FirstWeekStart` use the attempt's own timing date (below; R-02);
  - `EntryViews.HistoryOf`, `GoalListItems.Of` and `GoalEvaluation.OfAsync` take the settings through the new
    `GoalTimeline.VersionInEffect` (R-02), not through `VersionOf(goal, current ?? goal.StartDate)`. So does
    `GoalService.DetailsOf`, not through its `current is { } date ? VersionOf(goal, date) : goal.Versions[0]`
    (GoalService.cs:272);
  - `GoalDetails.StartDate` keeps returning `goal.StartDate`, the goal's start date (slice 4 FR-016).
    `AttemptStartDate` gives the latest attempt's.

- **The timing date of an attempt.** Attempt 1 is timed from the goal's start date (its first version's date), exactly
  as slice 4 times a goal. Every later attempt is timed from its own start date, which its first version applies
  from. Slice 4 also accepts an attempt 1 whose start date is later than the goal's start date (`Goal.cs:40-43`
  refuses only an earlier one). No store writes one, because creation starts attempt 1 on the goal's start date
  (slice 4 FR-018). The plan doesn't refuse it either, because a new refusal could break locked AC code. Such a goal
  is timed from the goal's start date, as slice 4 times it. So for every goal with one attempt each of these readers
  gives exactly today's date.

**Why.** A new attempt can have another schedule kind (D-1, FR-012). So "the goal's target" and "the goal's
versions" stop being one list. Keeping each attempt's versions together lets every timeline rule run per attempt
with today's code. The kept constructor and the unchanged meaning of `StartDate`, `Attempt` and `Versions` keep the
locked AC code of F001 to F008 compiling and passing (FR-024, plan "Constraints"). Storing the ending date rather than
the last date follows FR-026 and ADR 0020: the last date is derived.

**Alternatives.**

- **One goal-level version list with an attempt number on each version.** Rejected for two reasons:
  - An attempt that ended before it began keeps its first version (FR-009). That version can apply from a date
    *after* the next attempt's start (attempt 1 from 23 Nov, ended on 17 Nov, attempt 2 from 18 Nov). So the dates of
    one goal-level list would not increase, and `VersionOf(goal, date)` would pick the wrong attempt's version.
  - Every reader would have to filter by attempt anyway.
- **Each new attempt as a new goal row linked to the first.** Rejected because FR-012 requires that "the goal keeps
  its identifier, habit and name". Every page, the record keys and the History would have to follow a chain.
- **Changing `GoalAttempt` to carry its versions.** Rejected because it changes a record type's equality and adds a
  list to a type the locked AC code may compare. `GoalAttempt` only gains an init-only `Ending`, which keeps value
  equality.

## R-02 `GoalTimeline` becomes attempt-aware, so every existing reader respects attempt boundaries

**Decision.**

- The existing `Goal`-taking members keep their signatures and now work through the goal's attempts. Each attempt is
  timed exactly as slice 4 times a goal: its own timing date (R-01), its own versions, the first Monday on or after its start
  date for a per-week schedule. The attempt is then cut at its last date:
  - `TryCalendar`, `TryEntries` and `EntryOf` give only commitments and periods inside an attempt. Dates between
    attempts belong to none (FR-013, FR-016).
  - `VersionOf(goal, date)` gives the version of the attempt that contains the date. For a date outside every
    attempt it gives the latest attempt's version in effect then, as slice 4 does before the start date.
  - `CurrentDate` follows the spec's extended rule ("Terms used below"):
    - while the latest attempt has begun and is not ended, it is that attempt's current date, as in slice 4;
    - otherwise, once any attempt has begun, it is the date of the instant in the zone of the last version of the
      latest attempt that has begun;
    - otherwise it is null ("a goal that has not started").
  - `HasBegun(goal, date, instant)` requires the date's own attempt to have begun at the date in that attempt's own
    zone. So a date of a waiting attempt is refused with "You cannot record a day that has not begun." until its day
    begins, whatever the goal's current date (spec "Current date").
  - `NextCommitment` and `FirstWeekStart` use the latest attempt. An ended goal has no next commitment.
  - **A range that spans attempts of different schedule kinds.** `TryCalendar` lists every attempt's entries in the
    range: the commitments of commitment-based attempts in `Commitments` and the periods of period-based attempts in
    `Periods`, each list in date order. Both lists can be non-empty only when the range spans attempts of both kinds
    (e.g. US3-AS5, a daily goal restarted as 3 times a week). `Kind` is the kind of the latest attempt with an entry
    in the range, or, when there is none, of the latest attempt that starts on or before the range's last date, or
    else of attempt 1. `TryEntries` merges both lists into one list in date order. Attempts never overlap, so the
    merge is a concatenation in attempt order. `EntryOf` reads one date, so it always meets a single attempt.
    `GoalCalendarView(Kind, Commitments, Periods)` keeps its positional shape and mirrors this. Only its doc comment
    changes ("the other list is empty" becomes "empty unless the range spans attempts of both kinds"). For a goal
    with one attempt the result is exactly slice 4's. The readers that build on `TryEntries` never need the mixed
    case for a figure: the History and the reports cover the latest attempt only (FR-016, R-07), and an attempt's own
    reads cover one attempt. The earlier-Pending lookup reads each ended attempt on its own
    (`EntriesThrough(goal, number, last)`). So the mixed case reaches only `GetCalendarAsync`.
  - **No date.** An attempt has no date when it has no commitment or period: its last date is before its first
    entry. That is the case when the last date is before the start date (FR-008's example). It is also the case for a
    per-week attempt that starts mid-week and ends before its first full week. For example, it starts Wed 18 Nov and
    is ended Tue 24 Nov, so its last date is Sun 22 Nov and its first week would be 23 to 29 Nov. That attempt has no
    period, so it shows "ended before it began" and an empty History. See plan "Spec discrepancies".
- New members for the settings in effect and the waiting attempt:
  - `VersionInEffect(goal, number, instant)`: the settings in effect of one attempt, in one place:
    - for an ended attempt, its last version (the spec's "Last version": the version of its last date, or its first
      when it has no date);
    - while the attempt has not begun, its first version;
    - otherwise, its version at the goal's current date.

    `VersionInEffect(goal, instant)` is the same for the latest attempt: FR-005's settings in effect.
    `GoalText.Name`, `GoalService.DetailsOf`, `GoalListItems.Of`, `EntryViews.HistoryOf` and `GoalEvaluation` read
    it, each for the attempt it shows (R-07). For a goal with one attempt it gives what those readers compute today:
    `VersionOf(goal, current)` when there is a current date. With no current date it gives the first version, as
    `DetailsOf` (`goal.Versions[0]`) and the others (`VersionOf(goal, goal.StartDate)`) do.
  - **When an attempt has begun.** A not-ended attempt has begun once the day of its timing date (R-01: attempt 1
    the goal's start date, a later attempt its own start date) has begun in the zone of its first version.

    An ended attempt has begun when it has a date, i.e. a commitment or period up to its last date. The time of an
    ending is not stored (FR-026), so the rule must follow from stored dates. "Start date on or before the ending
    date" would be wrong: a waiting attempt in a zone west of the earlier attempt's can be ended on its start date,
    taken from the earlier attempt's current date, before its own start day has begun. Its last date is then before
    its start date, so it has no date and counts as not begun. An attempt that has a date has had its first entry
    begin before its ending, because that entry's first date is on or before its last date, which is before the
    ending date. So the rule never calls an attempt begun when it wasn't.

    The other way round, an attempt that ended with no date counts as not begun, even if its start day had begun.
    Examples: one ended on its own start date, or a per-week attempt ended before its first full week. That changes
    nothing a user can see. Such an attempt has no commitment, period, record or figure, and its page says "ended
    before it began". The only other effect is that it never lends its zone to `CurrentDate`. So in AC-035, attempt
    1 (from 23 Nov, ended 17 Nov) never gives the goal a current date or its zone, and its page never says "started"
    or "starts". See plan "Spec discrepancies" 5.
  - `LatestAttemptHasBegun(goal, instant)` applies this rule to the latest attempt. It drives the attempt line and the
    Settings card's start row (`GoalDetails.AttemptHasBegun`), and the waiting attempt's start date in the Goals list
    (`GoalListItem.StartDate`), without the pages reading a clock. `CurrentDate`'s "latest attempt that has begun"
    uses the same rule. For an ended latest attempt that has not begun, the attempt line is "Attempt n · ended before
    it began" and the Settings card has no start row: there is no start to announce.
  - `ChangeOf` (time zone, slice 4 FR-026) works on the latest attempt:
    - `None` for an ended goal (FR-021);
    - `ReplaceFirst` while the latest attempt has not begun, also when an earlier attempt gives the goal a current
      date (FR-005, FR-021).
- New members:
  - `LastDate(goal, number)`: null while not ended. Otherwise E − 1 day, or for a per-week attempt the Sunday before
    E's week (FR-008).
  - `AttemptOf(goal, date)`.
  - `EntriesThrough(goal, number, last)`: one attempt's entries through `last` or its last date, whichever is earlier.
  - `SettingsChangeOf` (R-03).
  - `EndingOf` (R-04).

**Why.** Every rule of FR-010, FR-013 and FR-016 is then enforced where slices 5 to 7 already look:

- a record for a date between attempts gets "Nothing is scheduled on this date." through the unchanged `EntryOf` call
  of slice 5's step (4);
- the evaluation calendar gives nothing outside an attempt;
- an ended goal has no Due entry.

For a goal with one attempt, no ending and no change, every member gives exactly what it gives today. Unit tests
compare both on the slice 4 and slice 7 timeline cases (FR-024).

**Alternatives.**

- **New attempt-taking members, with each caller choosing the attempt.** Rejected because it changes about 20 call
  sites in five services. One missed site would accept a record between attempts.
- **Building a one-attempt `Goal` per attempt in each caller.** Rejected because it puts the attempt rules in the
  Application. It is kept only as the internal technique inside `GoalTimeline`.

## R-03 Settings changes: one Domain function decides the versions (FR-001 to FR-006)

**Decision.** `GoalTimeline.SettingsChangeOf(goal, instant, target, zone)` returns the latest attempt's new version
list and the date from which the new settings apply. `zone` is the account's current time zone. The Application has
already checked that the goal is not ended and that the target has the attempt's schedule kind.

- **The latest attempt has not begun** (no date of it has begun, also when an earlier attempt gives the goal a
  current date): its first version is replaced, keeping its date, with the new target and `zone`.
- **It has begun**, at the goal's current date D. The next boundary B is D + 1 for a commitment-based or per-day
  attempt, and the Monday after D's week for a per-week one (the rule of `ChangeOf`).
  - If a version applies after D (at most one), it is replaced by the new target and `zone` from B.
  - Otherwise a version from B is added.

Other rules:

- **The same schedule kind (D-1)** means the same `ScheduleKind` and, for times, the same `SchedulePeriod`. It is a
  new `GoalTarget.HasSameScheduleKind`.
- **Order of the checks (FR-003).** The use case checks the target fields first, through `GoalDraft.CheckTarget` (the
  target steps of `GoalDraft.TryCreate`, without the start date or the name), then the schedule kind, then the name.
- **"Settings unchanged"** compares the new target with the target of the latest version, the waiting one included
  (AC-100), and the trimmed name with the goal's own name. The time zone is not a field the user sets, so it is not
  compared.
- **A name-only change** writes only the goal's name and returns no date.
- **The time zone.** The written version takes the account's current zone, so a settings change and a time zone
  change merge into the one later version (US1-AS13, AC-018).

**Why.**

- One pure function, next to `ChangeOf`, keeps both kinds of change on the same boundary rule. A goal then never has
  more than one later version, and no begun date changes its version (FR-002, SC-002).
- Comparing with the latest version follows FR-003's wording, and AC-100 tells it apart from the version in effect.
- The ended-goal check comes before the target check (FR-003 step 4), so `SettingsChangeOf` never sees an ended goal.

**Alternatives.**

- **Applying the change from the current date when its entry has not begun yet** (e.g. a per-day goal changed before
  midnight). Rejected because FR-002 fixes the next boundary at the goal's current date.
- **Allowing another schedule kind and starting the streak again.** Rejected by D-1.

## R-04 Ending: the Domain gives the last date, the store applies the cuts (FR-007 to FR-010)

**Decision.** The ending date E is:

- the goal's current date at the instant (R-02's extended rule);
- for a goal that has not started, the account's today (FR-007, "Ending date").

`GoalTimeline.EndingOf(goal, endingDate)` returns:

- the last date L (R-02);
- the versions the ended attempt keeps: those that apply on or before L, and always the first (FR-009, the confirmed
  consequence of D-2).

The store writes, in one transaction:

- the ending;
- the kept versions;
- every pause of the goal cut to L: one that begins after L is deleted, and one that covers L ends on L;
- every excusal after L deleted.

Records and notes are never deleted (FR-009).

`Outcome` is an enum: `Achieved`, `Abandoned`, `NoLongerRelevant`. An outcome the user did not give is a null in the
input, and gives "Choose how the goal ended.". `EndingNote` is trimmed: empty means none, and it has at most
`RecordLimits.MaxNoteLength` (500) characters. It is a type of its own, not `NoteText`, because an ending note is not
a note of a commitment or period (FR-027). Like `NoteText`, its `ToString` names no value (HP-010).

**Why.**

- The ending date is the one stored fact (FR-026), and L is a pure function of it and the schedule kind.
- The pause and excusal cuts change stored facts of slice 7. They go in the same transaction as the ending, so no
  reader sees an ended goal with a pause after its last date.
- Keeping the first version gives the ended goal its name, settings and starting values (FR-004, FR-005, FR-011),
  also for an attempt with no date (US2-AS12, AC-035).

**Alternatives.**

- **Storing L.** Rejected because L is derived (ADR 0020).
- **Keeping pauses and excusals after L and filtering them on read.** Rejected because FR-009 says ending removes and
  cuts them. A kept pause after L would also cover a new attempt that starts on its dates.
- **Reusing `NoteText` for the ending note.** Rejected because the type name would say "note of an entry", which
  FR-027 rules out.

## R-05 Storage: attempt endings and per-attempt versions, one migration, three conditional writes

**Decision.**

- **Migration `GoalChanges`:**
  - `GoalAttempts` gains:
    - `EndingDate` (`date`, null);
    - `Outcome` (`varchar(16)`, null, the enum as a string like the other enums; "NoLongerRelevant" has 16
      characters);
    - `EndingNote` (`nvarchar(500)`, null).

    As for the goal's habit columns, the mapping (not a check constraint) checks that `EndingDate` and `Outcome` are
    both set or both null, and that `EndingNote` is null without them.
  - `GoalSettingsVersions` gains `AttemptNumber` (`int`, not null, existing rows 1).
    - Its key becomes (goal, attempt number, applies-from).
    - A foreign key (goal, attempt number) to `GoalAttempts` (goal, number) is added with **no action** on delete
      (`DeleteBehavior.NoAction`). The existing cascading foreign keys `Goals` → `GoalSettingsVersions` and `Goals` →
      `GoalAttempts` stay, and with them `GoalRow.Versions`. A second cascade would give SQL Server two cascade paths
      to `GoalSettingsVersions` (HtDbContext.cs:138-139), and it refuses that constraint ("may cause cycles or
      multiple cascade paths"). Deleting a goal still works: both existing cascades run in the one delete statement,
      and SQL Server checks a no-action constraint only at the end of that statement, when the versions are gone too.
      An integration test deletes a goal with two attempts.
    - Every existing version belongs to attempt 1, the only attempt slices 4 to 7 write.
- **`IGoalStore` reads:**
  - one new member, `ReadOwnerGoalAsync(owner, goal)`: the account's stored zone and goal revision, then that one goal,
    in that order (the read-order rule of `ReadOwnerGoalsAsync`, for one goal);
  - `ListAsync`, `FindAsync` and `ReadOwnerGoalsAsync` read every attempt with its versions and ending.
- **Three conditional writes**, `TryChangeSettingsAsync`, `TryEndAsync` and `TryStartAttemptAsync`. Each:
  - locks the account row (UPDLOCK, HOLDLOCK), then the goal row, as goal creation and the time zone change do;
  - checks that the stored zone and the goal revision are the ones read;
  - re-checks its own precondition there:
    - the latest attempt is the one changed, and is not ended (change, end);
    - the latest attempt is ended, and its number is the new one − 1 (start);
  - writes;
  - increments `GoalsRevision`.

  A stale zone or revision gives `Stale` with nothing written. The use case reads again and retries, at most
  `GoalService.MaxCreateAttempts` (10) times, as creation does. Another owner's goal gives `GoalNotFound`.
- **Changed queries:**
  - `CountByCatalogHabitAsync` and `ListOnCatalogHabitAsync` leave ended goals out (FR-020). A goal is ended when its
    latest attempt has an ending date; it is not ended when it has an attempt without one.
  - `CountUsingCustomHabitAsync` and `CountUsingCatalogHabitAsync` keep counting ended goals, which are still "used
    by goals" (Edge Cases).
- **`TryChangeTimeZoneAsync`** applies `ReplaceFirst`, `ReplacePending` and `Add` to the latest attempt's versions.
- **`IRecordStore.AddPauseAsync`** re-checks under the goal lock that the goal is not ended, and gives a new
  `ExemptionStoreWrite` outcome, `GoalEnded`. Slice 7's re-checks are otherwise unchanged.
- **`IRecordStore.ReadRecentNoteKeysAsync`** gets an overload with a first and an optional last date,
  `(owner, goal, from, through, count, ct)`. Recent notes then count only notes inside the latest attempt (FR-016):
  - `from` is the first date of the latest attempt's first entry;
  - `through` is the last date of its last entry once it is ended, and null while it is not;
  - an attempt with no entry (an attempt with no date) has no note to read, so `GetRecentNotesAsync` gives an empty
    list without calling the store.

  So neither a note between two attempts nor a kept note after an ending's last date is read (FR-009).

**Why.**

- The account row and `GoalsRevision` already serialize goal creation against time zone changes. Putting the new
  writes on the same lock and token means:
  - a settings change and a time zone change on the same goal cannot both add a later version;
  - an ending cannot race a time zone change that adds a version after L.
- The goal row lock serializes the new writes with slice 5 to 7's record and exemption writes. The lock order is
  account, then goal, in the goal store, and goal only in the record store, so no deadlock is possible.
- The pause re-check closes the one race that matters: a pause written after a concurrent ending could cover the dates
  of a later new attempt. A record or an excusal written after a concurrent ending lands on the ending date or later.
  That date is outside the ended attempt (L ≤ E − 1), and every new attempt starts after E (FR-012), so no attempt
  ever reads it.

**Alternatives.**

- **A row version on `Goals` as the token.** Rejected because the account revision already orders time zone changes
  against every goal of the owner, and a time zone change writes several goals at once.
- **One generic "replace the goal" write.** Rejected because three narrow writes give three contracts that integration
  tests can pin, and none of them can overwrite another attempt.
- **Deleting and recreating the version key in place without a foreign key.** Rejected because the foreign key makes
  an orphan version impossible at no cost.

## R-06 Application: `GoalChangeService` and `AttemptService`

**Decision.** Two new services, each taking the acting user first. Each checks access in FR-023's order (signed in,
not an administrator, the stored account), then the goal (FR-022).

- **`GoalChangeService`** (`HT.Application.Goals`):
  - `ChangeSettingsAsync` (FR-001 to FR-006);
  - `EndAsync` (FR-007 to FR-009);
  - `GetNewAttemptValuesAsync` (FR-011);
  - `StartAttemptAsync` (FR-012).

  It depends on `IGoalStore`, `IAccountStore`, `TimeProvider` and `InstallationTimeZone`.
- **`AttemptService`** (`HT.Application.Reports`):
  - `GetAttemptsAsync` (FR-014, the list);
  - `GetAttemptAsync` (FR-014, one attempt with its recent results and History);
  - `GetGoalHistoryAsync` (FR-015).

  It depends on `IGoalStore`, `IRecordStore`, `IAccountStore` and `TimeProvider`, and evaluates attempts through the
  shared `GoalEvaluation` (R-07).
- **Starting a new attempt** checks the target through `GoalDraft.CheckTarget`, then the start date, in FR-012's order:
  missing; before the account's today; on or before the ending date; more than 365 days after the account's today.
  The third check is a new `GoalProblem.StartDateNotAfterEnding`. Goal creation never produces it, so
  `GoalCreateOutcome` is unchanged. The new attempt's first version uses the account's current zone.
- **The starting values** are the ended attempt's last version (`GoalTimeline.VersionOf` at L, or the first version
  for an attempt with no date). The start date is the later of the account's today and E + 1.
- **The architecture rule** `OnlyTheGoalTypes…MayDependOnTheGoalStorePort` admits both services. `GoalChangeService`
  is in `HT.Application.Goals`, which the rule already allows. `AttemptService` is added by name, as slice 7 added the
  exemption services.

**Why.**

- `GoalService` already has 443 lines and five use cases. The four changes share their write-and-retry loop and their
  texts.
- The three reads need the record store and the report evaluation, which live in `HT.Application.Reports`.
- Separate services keep each dependency set minimal and each fence explicit.

**Alternatives.**

- **Adding everything to `GoalService` and `ReportService`.** Rejected because `GoalService` would gain a record-store
  dependency it does not need for its own use cases.
- **One `AttemptService` for everything.** Rejected because writes and reads would share one large dependency set.

## R-07 Earlier reads: one evaluation per attempt, the latest attempt as the report range (FR-013, FR-016 to FR-020)

**Decision.**

- **`GoalEvaluation.OfAsync`** gets an overload with an attempt number. It evaluates that attempt's entries:
  - through the earlier of its last date and the goal's current date;
  - or, while it is the latest attempt and not ended, through the goal's current date as before.

  The existing overload evaluates the latest attempt, which is the report range (FR-013). So consistency, streaks,
  recent results, the trend, the Today entries and the notes of every report count only the latest attempt.
  - **Each evaluation takes the settings of the attempt it evaluates.** `GoalEvaluation` derives its streak `Unit`,
    its `BuildOrReduce` and its trend's kind and span from the one target it is built with (GoalEvaluation.cs:45-46,
    161). That target is `GoalTimeline.VersionInEffect(goal, number, instant)` of the evaluated attempt (R-02):
    - an ended attempt's last version (its first when it has no date);
    - a waiting attempt's first version;
    - otherwise the version at the goal's current date.

    The existing overload passes the latest attempt. So attempt 1 of US3-AS5 (daily, commitment-based) keeps
    "commitments" as its streak unit on the Earlier attempts card and in the attempt view, while attempt 2 (3 times
    a week) counts "weeks" (FR-006, FR-014). Its `Name` is `GoalText.AttemptName` for an attempt that is not the
    latest. A unit test evaluates both attempts of a goal restarted with another schedule kind and with Build changed
    to Reduce.
  - An evaluation also exposes `EarlierPending`: the Pending entries of the goal's ended attempts other than the
    latest whose grace period is still open. Only attempts whose last entry ends within the last 48 hours are
    evaluated for it, so this costs nothing for most goals.
  - The records of both come from one `ReadAsync` over the range they span.
- **`EntryViews.Of`** sets the new `HistoryEntry` fields (data-model.md):
  - the attempt number;
  - whether it is the latest attempt;
  - for an attempt that is not the latest, its name (FR-004);
  - `Settings`, an `EntrySettings` with the settings of the version of the entry's date: Build or Reduce, the
    measurement, the amount, the units, the schedule kind, the period and N. For an entry of an attempt that is not
    the latest, its `Settings.BuildOrReduce` is FR-016's and FR-019's "that attempt's Build or Reduce".

  `Settings` is declared `EntrySettings?`, init-only and not `required`. A non-nullable property that is not
  `required` fails the build (CS8618 with `TreatWarningsAsErrors`), and `required` would break every existing object
  initializer of `HistoryEntry` (plan "Constraints"). `EntryViews.Of` always sets it. Readers never read it directly.
  They call the new `GoalHistory.SettingsOf(HistoryEntry entry)`, which gives `entry.Settings`, or, when that is
  null, the history's own goal-level settings as an `EntrySettings`. So entries built without it, in
  `CommitmentRowTests`, `HabitComponentTests` and the locked AC code, behave exactly as in slice 7.
- **Every view of an entry takes the entry's own settings, not the goal-level ones.** Until now the pages took the
  measurement, unit, schedule kind and target from the one goal-level `GoalHistory`, built from the version of the
  current date. Its doc comment still says "In this slice every version has the same target". That no longer holds:
  - a change within one attempt can switch the measurement or Build and Reduce (AC-020);
  - an earlier attempt's Pending entry can have another kind than the latest attempt (AC-080).

  So every place that shows or records one entry reads that entry's settings (`history.SettingsOf(entry)`). The
  rule covers every member that takes a `GoalHistory` for one entry's sake, not a list of examples:

  | Place | Today | Slice 8 |
  |---|---|---|
  | `RecordPreview.Progress` | `history.Measurement`, `history.Target` | the entry's settings: its measurement for `RecordValue.TryCreate`, its target for `Evaluator.ProgressOf` |
  | `RecordingText.Recorded` | `history.Measurement`, `history.CountUnit` | the entry's measurement and unit |
  | `RecordingText.TargetLine` (the dialog's sub line) | `history.Target`, `BuildOrReduce` | the chosen row's target and Build or Reduce |
  | `RecordingText.Progress` (the dialog's progress line) | `history.BuildOrReduce`, `Measurement`, `CountUnit` | the chosen row's settings |
  | `RecordingText.FieldLabel`, `PartialNote`, `ReduceHelp` | `history.Measurement`, `BuildOrReduce`, `Target` | the chosen row's settings |
  | `RecordingText.NoteHelp` | `history.GoalScheduleKind`, `Period` | the chosen row's settings |
  | `ReportText.ProgressText` (`CommitmentRow`, `GoalItem`) | `history.Target`, `Measurement` | the row's settings. It is called only for Due entries, which belong to the latest attempt and use the version of the current date, so its text doesn't change today. It takes the row's settings anyway, so the rule has no exception. |
  | `RecordDialog` (`RecordFields` measurement and unit, the commitment test, the help lines) | `Recordable.Measurement`, `.CountUnit`, `.GoalScheduleKind` | the chosen row's settings; choosing another row in the chooser switches them |
  | `HistoryCard`, `NoteDialog`, `CommitmentRow` (state words, `StatusChip`, the progress bar test) | `History.GoalScheduleKind`, `.Period`, `Entry.Goal.*` | the row's settings |
  | `TodayEntry.ScheduleText` | `LatestTarget` | the schedule text of the entry's settings for an entry of an attempt that is not the latest; otherwise as before |

  Each `RecordingText` member above that takes a `GoalHistory` (`Recorded`, `TargetLine`, `Progress`, `FieldLabel`,
  `PartialNote`, `ReduceHelp`, `NoteHelp`) keeps its signature and gets an overload on `EntrySettings`. The pages
  call only the `EntrySettings` overloads. `RecordPreview.Progress(GoalHistory, HistoryEntry, …)` keeps its
  signature and reads `history.SettingsOf(entry)`, and a new `RecordPreview.Progress(EntrySettings, HistoryEntry,
  …)` is added. A bUnit test pins it: it renders the RecordDialog for AC-080's attempt 1 row under an attempt 2 of
  Reduce, per day, and asserts the Build sub line, the duration field and the commitment-based note help. The goal-level fields of `GoalHistory` stay for what is about the goal as a whole (the
  page head, `CurrentDate`, `ReadAt`). Their doc comments drop the claim that every version shares one target.
  Component tests also render AC-020's History (11 Nov "12 min", 12 Nov "12 pages", and the Build help on 11 Nov
  after example 02's change to Reduce).
- **The goal-level settings of an attempt's History.** `AttemptDetails.History` is a `GoalHistory` whose goal-level
  fields come from `VersionInEffect(goal, number, instant)` of that attempt, not of the latest attempt
  (`EntryViews.HistoryOf` gets the version to use as a parameter). Its rows carry their own `Settings` in any case.
- **The trend** covers the latest attempt only, so its schedule kind never mixes (D-1). Its kind and span keep coming
  from the attempt's version in effect. A change of measurement within the attempt can still put entries of two
  measurements in one trend. Today's `TrendBuilder` doesn't handle that:
  - `CommitmentWeeks` sums each entry's `Evaluator.ProgressOf(...).Value` into a weekly count of "Yes" records, so an
    earlier duration commitment would add its minutes ("75 of 7");
  - `BarOf` draws an earlier yes/no bar in a `Minutes` trend with " min".

  Decision: a bar or a week is drawn in the trend's kind. An entry whose own measurement differs from the trend's
  contributes only its result:
  - in commitment weeks, a Met entry counts as one "Yes" and any other entry as none;
  - in a value trend (minutes or count), such an entry's bar has no value, exactly like an entry without a record,
    and keeps its exemption mark.

  The trend line takes only bars of the trend's measurement. Unit tests cover a duration-to-yes/no change and a
  yes/no-to-duration change in a commitment-based attempt, and a count-to-duration change per day. For a goal
  without such a change, every bar is as in slice 7.
- **Records and notes outside every attempt are never mapped to an entry.** Slice 5 assumed "a stored record's date
  always belongs to an entry" and threw otherwise (`RecordingService.StoredEntryOf` and `NoteEntryOf`). Records and
  notes kept after an ending's last date (FR-009, AC-034, AC-039) break that assumption. So:
  - `ChangeRecordAsync` and `DeleteRecordAsync` on a record whose date is in no attempt give `NothingScheduled`,
    "Nothing is scheduled on this date.", the text slice 5's step (4) gives for adding such a record (FR-010). They
    change nothing. The record stays a stored fact, and a later attempt never contains its date (FR-012).
  - `GetRecentNotesAsync` reads through the bounded overload (R-05), so it gets no such key. It also skips a key
    with no entry instead of throwing, as it already skips a note removed between its two reads.
  - `StoredEntryOf` and `NoteEntryOf` and their "belongs to no commitment or period" exceptions are removed. Unit
    tests cover a record and a note on the ending date of a commitment-based goal, and in the week in progress of
    a per-week goal.
- **Earlier Pending entries in the overview.** `GoalEvaluation` exposes `Recordable` (new): its own Due and Pending
  entries plus `EarlierPending`, in slice 6's order. The following read `Recordable`:
  - `OverviewGoal.Recordable`, `HasPending` and `CanRecord`, so `Recordable` stays equal to `GetRecordableAsync` at
    the same instant (slice 6) and the Goals page's "Record" offers the earlier entry;
  - `GoalOrder`'s Pending group: "a goal with a Pending entry" keeps its meaning, and an earlier attempt's Pending
    entry is still the goal's (FR-018: the order rules are unchanged);
  - the dashboard's Pending count and Today list (FR-019).

  The figures, the results and the strip stay those of the latest attempt (FR-013, FR-018).
- **What can be recorded now** (`GetRecordableAsync`) and the dashboard's Today list and Pending count add
  `EarlierPending` to the latest attempt's Due and Pending entries, in slice 6's order (FR-016, FR-019).
- **The dashboard** leaves the rest of ended attempts out:
  - no Missed entry of them is counted or listed;
  - an ended goal has no Due or Upcoming entry and no summary;
  - the count of goals counts goals that are neither paused nor ended;
  - recent notes leave out ended goals' notes.

  A `TodayEntry` of an ended goal says so, through the new `GoalEnded`.
- **The goals overview** keeps slice 6's order. Each `GoalListItem` gives `IsEnded`, `EndingDate` and `Outcome`
  (FR-018). An ended goal is never paused and has no Due entry.
- **The History** (`GetHistoryAsync`) keeps its range check (1 to 366 days) and gives only the latest attempt's
  entries within the range (FR-016).
- **Recent notes** read through the new bounded overload, over the latest attempt's entries (R-05).
- **Reading a goal** (`GetAsync`) gives:
  - the settings in effect and the full later settings (FR-005);
  - the latest attempt's number and start date;
  - when ended, its ending date, last date, outcome and note (FR-017).

  For an ended goal, the settings are its latest attempt's last version, with no later version. For a goal whose
  latest attempt has not begun, they are that attempt's first version.
- **Pausing** (slice 7 FR-006) gains the step "This goal has ended." after step (3).
- **Excusing** judges "the goal is period-based" by the schedule kind of the attempt that contains the date. A date in
  no attempt keeps step 5's "Nothing is scheduled on this date.".

**Why.**

- `GoalEvaluation` is already the single evaluation behind every report figure (slice 6 research R-04). Selecting an
  attempt there keeps every figure consistent with every other.
- The 48-hour filter keeps the dashboard's cost at slice 7's for every goal that has no recent ending.

**Alternatives.**

- **Filtering by attempt in each report method.** Rejected because the figures would drift apart.
- **Evaluating every ended attempt for the dashboard.** Rejected because it costs a full evaluation per attempt, and
  FR-019 needs only open grace periods.

## R-08 Names of attempts (FR-004, FR-014, FR-015)

**Code fact.** Today `GoalText.Name(Goal goal, string habitName)` returns the goal's own name, or the habit's name,
", " and `TargetText(goal.Target)`. `Goal.Target` is the *first* version's target (`Versions[0]`, Goal.cs). Until now
that was correct, because every version shared one target. The method takes no date or instant, so it cannot name a
goal whose target depends on the time.

**Decision.**

- A new overload `GoalText.Name(Goal goal, string habitName, DateTimeOffset instant)` names the goal at an instant:
  its own name if it has one, otherwise the habit's current name, ", " and the target text of
  `GoalTimeline.VersionInEffect(goal, instant)` (R-02). That gives:
  - the version of the goal's current date: "Reading, 15 min daily" at 11 Nov 23:59:59 and "Reading, 20 min daily"
    from 12 Nov 00:00 (AC-001, AC-002);
  - for an ended goal, its latest attempt's last version;
  - for a goal whose latest attempt has not begun, that attempt's first version (FR-004).
- Every caller passes the instant its use case read once: `GoalEvaluation.OfAsync`, `GoalListItems.Of` (and with it
  the name sort of `GoalService.ListAsync`), `GoalService.DetailsOf` and `CatalogService`'s goal list on a habit.
- `CatalogService` reads no clock today (`CatalogService(ICatalogHabitStore, IGoalStore)`). It gets an added
  constructor `CatalogService(ICatalogHabitStore, IGoalStore, TimeProvider)`, which the host registers. Its
  `GetDetailsAsync(actingUser, …)` reads the time once and names each goal at it. The two-argument constructor stays
  (FR-024). A service built with it names goals with the two-argument `Name`, exactly as slice 4 does. No
  production code uses it.
- The two-argument `Name(goal, habitName)` keeps its signature for compatibility (public Domain surface, FR-024). It
  now names the goal by the latest attempt's first version, which is what it gives today for every goal of slices 4
  to 7. No production code calls it after this slice.
- A new `GoalText.AttemptName(goal, number, habitName)` names an earlier attempt: the goal's own name if it has one,
  otherwise the habit's current name and the target text of that attempt's last version. The goal history and the
  attempts read use it for every attempt that is not the latest. The latest attempt gets the goal's name at the
  read's instant.

**Why.** FR-004 makes the derived name depend on the settings in effect, which now change at a boundary, so the name
needs the instant. Naming at read time with the habit's current name passes AC-103 (a habit renamed after the ending).
Nothing derived is stored (ADR 0020).

**Alternatives.**

- **Storing the attempt's name at its ending.** Rejected because it keeps the old habit name, and AC-103 fails.
- **A `DateOnly? currentDate` parameter instead of the instant.** Rejected because the settings in effect also
  depend on whether the latest attempt has begun and on whether the goal is ended. `VersionInEffect` decides both from
  the instant, in one place.

## R-09 Demo goal-change loading (FR-030, FR-031, HP-011, ADR 0026)

**Decision.**

- **The operator capability** is `DemoGoalChangeService` (`HT.Application.Operator`). It has the contract of
  `DemoPauseService`: the demo password checks, "already exists" ignoring case, and no private read.
- **The goals** come from `DemoGoalChanges`, which builds the three goals of FR-031 relative to the run date R.
- **The port** is `IDemoGoalChangeStore`, a new port fenced to its service (ADR 0013). It is implemented by
  `DemoGoalChangeStore` over slice 6's `DemoAccountWriter`, which gains attempts, per-attempt versions and endings.
- Each goal is left out when its catalog habit is not Published (looked up by name, ignoring case), as in slice 7.
- The results are "created", "demo goals added" and "demo records added", and never name a goal, habit, note,
  outcome, unit or amount.
- `DemoGoalChanges`' figures are proved by unit tests at a fixed run date on each weekday, against FR-031's list:
  - Reading: "96% (25 of 26)", 6 and 19;
  - Meditation: attempt 2 "100% (12 of 12)", attempt 1 "52% (11 of 21)";
  - Sugar: "100% (23 of 23)";
  - the dashboard: 2 Due, 4 Pending, 0 Missed, 2 goals;
  - the goal history order.

**Why.**

- The same shape as slices 6 and 7 keeps one demo writer, one transaction per account and one fence per port.
- The facts are written directly (FR-026). The user-facing limits (FR-002, FR-007, FR-012) apply to users only
  (ADR 0026).

**Alternatives.**

- **Building `demo7` through the use cases at faked times.** Rejected because it would need a clock override in
  production code. ADR 0026 also says the facts are stored directly.

## R-10 Host and smoke check (HP-011, HP-012)

**Decision.**

- **Startup.** `StartupTasks` adds step 7, "Demo goal-change loading", after demo pause loading. It logs the account
  result lines as demo pause loading does, and "Demo goal-change loading: {result}" for a missing or weak password. A
  fault is logged without secrets and the start continues (ADR 0007).
- **Smoke check.** `tools/HT.SmokeCheck` adds check 9, "demo goal changes", signed in as `demo7@example.com`:
  - the Ended tab lists "Sugar, at most 1 sweet a day";
  - the page of "Meditation, 5 min daily" shows "Attempt 2";
  - the page of "Reading, 20 min daily" shows the Settings card's "From …" row with "25 min daily";
  - the Profile's "Goal history" tab shows "Abandoned".

  Each data parameter of `IDemoGoalChangeStore` gets its `SmokeCheckCoverage` entry.
- **Deployment.** "The deployment's success check also requires its result" is read as in slices 6 and 7: ADR 0015's
  success check is the smoke check, and check 9 needs `demo7`'s goals.

**Why.** It is the existing pattern. The smoke check confirms presence only; the figures are proved by the
acceptance tests (SC-007).

## R-11 Pages (HP-001 to HP-010, HP-014)

**Decision.** `design/` changes first (constitution IV, ADR 0008), as the spec's "Design changes before build" lists.
Then:

- **The goal page** (`GoalDetailBody`):
  - "Edit settings" and "End goal" in the head of a goal that is not ended;
  - the attempt line;
  - the Settings card's "From {date}" row (`GoalSettingsCard`);
  - the ended Notice with "Start new attempt", and no Record, Edit settings, Pause or End goal;
  - the new `EarlierAttemptsCard` after Recent notes.
- **Edit settings** (`/goals/{id}/edit`, new page `EditGoalSettings`):
  - reuses `NewGoalForm` in an "edit" mode, without the habit chooser or the start date;
  - limits the schedule choice to the goal's kind;
  - shows "Changes apply from {date}. Past results keep their rules." from the new `GoalDetails.ChangesApplyFrom`:
    the date `SettingsChangeOf` would give at the read's instant (the next boundary, or the start date of an attempt
    that has not begun). Reading the goal computes it, and nothing is written.
- **EndGoalDialog** (`Components/Goals/EndGoalDialog`): ChoiceCards with none chosen when it opens, the note field
  (`TextArea`), and the refusal shown under its field. It is built on `HtDialog` for focus management.
- **Start new attempt** (`/goals/{id}/new-attempt`, new page `NewAttempt`): reuses `NewGoalForm` in an "attempt" mode
  with every schedule kind and the start date, without the habit chooser or the name.
- **The attempt view** (`/goals/{id}/attempts/{n}`, new page `AttemptDetail`):
  - the head;
  - `GoalStats` with the eyebrow "Attempt {n}";
  - `HistoryCard` over the attempt's dates, whose record and note actions open the RecordDialog and NoteDialog fixed
    to their row.

  The latest attempt's number shows the Not found page.
- **Goals** (`?tab=ended`): `GoalsList` gets the Ended tab. `GoalItem` gets its ended form: the outcome `Tag`, "Ended
  {date}", "Start new attempt", and no "Record".
- **Profile** (`?tab=history`): the Goal history tab, a `DataTable` in FR-015's order, with the empty text. An
  administrator's Profile is unchanged.
- **Signed-in Home:** `TodayPanel` and `CommitmentRow` show the `Tag` "Ended" or "Attempt {n}". The Active goals card
  and the other tiles leave ended goals out.
- **Texts.** Every text comes from `ReportText` and a new `GoalChangeTexts`: outcome texts, "Ended …", the attempt
  line, "From …" (HP-008). Outcome Tags always carry their word.

**Why.** One form component serves three pages, so the fields and their refusals stay identical (HP-002, HP-004).
Pages follow their previews in `design/pages/` (constitution IV).

## R-12 Tests

**Decision.**

- **Unit, Domain** (with boundaries on both sides, constitution II, SC-005):
  - `Goal` invariants;
  - `GoalTimeline`:
    - attempt cuts;
    - the extended current date;
    - `HasBegun` for a waiting attempt in a zone west of the earlier one;
    - `LastDate` for commitment-based, per-day and per-week attempts ended on a Monday, Wednesday and Sunday;
    - `SettingsChangeOf` at 23:59:59 and 00:00 local, on a Sunday and a Monday for per-week, with a waiting version;
    - `EndingOf` with a version that took effect on E or on the Monday of the week in progress;
    - an ending on, before and after the start date;
  - `ChangeOf` on ended and waiting attempts;
  - `TryCalendar` and `TryEntries` over a range spanning a commitment-based and a per-week attempt (US3-AS5);
  - `VersionInEffect` (both overloads) and `LatestAttemptHasBegun` on running, waiting and ended attempts, also for a
    new attempt that starts before attempt 1's start date (AC-035), and for an attempt ended with no date, which
    never begins;
  - a one-attempt goal whose attempt 1 starts after the goal's start date, timed as in slice 4;
  - a waiting attempt in New York ended on its start date, taken from attempt 1's Kyiv current date: it has not
    begun, and `CurrentDate` keeps Kyiv's zone;
  - `GoalText.Name` at an instant on both sides of a boundary (AC-001, AC-002), and `GoalText.AttemptName`.
  - Equivalence tests run the slice 4 to 7 timeline cases on a one-attempt goal built through both constructors.
- **Existing tests this slice changes on purpose**, in the step that changes their rule:
  - `GoalTests.Only_the_time_zone_differs_between_versions` (GoalTests.cs:73-76) expects a version with "2 times
    per day" after one with 1 time per day to throw. Both are times per day, so the new rule allows them. The test
    becomes "versions of one attempt keep the schedule kind": fixed days after times per day throws, and per week
    after per day throws.
  - `GoalDraftTests.Every_refusal_has_a_field_and_None_has_none` (GoalDraftTests.cs:272-279) requires a `FieldOf`
    entry for every `GoalProblem`. `GoalDraft.FieldOf` gains `StartDateNotAfterEnding → GoalField.StartDate`, which
    also fills `AttemptStartResult.Field`, and the test's `Fields` data gains that row.
  - `RecordLogTests` (tests/HT.IntegrationTests/Records/RecordLogTests.cs:421-429) asserts that `ChangeRecordAsync`,
    `DeleteRecordAsync` and `GetRecentNotesAsync` fail on a record and a note stored outside every entry, with the
    two "belongs to no commitment or period" messages. R-07 removes those throws. The block becomes: change and delete
    give `NothingScheduled` and change nothing, recent notes leave the stray note out, and no log line names a value.
  - The test doubles that implement the changed ports gain the new members (each forwards or throws, as it does for
    its other members):
    - `RacingGoalStore : IGoalStore` (tests/HT.IntegrationTests/Goals/TimeZoneChangeTests.cs);
    - `BeforeFirstCreate : IGoalStore` (tests/HT.UnitTests/Application/Goals/GoalWorld.cs);
    - `FaultingRecordStore : IRecordStore` in `RecordLogTests` and in `ExemptionLogTests`;
    - the shared fakes `FakeGoalStore` and `FakeRecordStore` (tests/HT.UnitTests/Fakes), which the bUnit page services also register.
  - `CommitmentRowTests` and `HabitComponentTests` build `HistoryEntry` and `GoalHistory` objects without `Settings`.
    They keep compiling (`Settings` is nullable and not `required`), and keep passing through
    `GoalHistory.SettingsOf`'s fallback (R-07). They stay unchanged as the proof of that fallback.
  - The `GoalHistory` doc claim "every version has the same target" (R-07).
  - The absence tests named in spec "Earlier requirements and tests this slice changes" (UI).
- **Unit, Application:** every outcome and check order of the seven use cases; the retry on `Stale`; the
  `GoalEvaluation` attempt overload and `EarlierPending`; dashboard, overview, recordable and history changes; the
  pause and excuse changes; `DemoGoalChanges` figures on every weekday. Fakes gain the new members.
- **Negative privacy tests:** another user, an administrator and an anonymous visitor on each of the seven use cases
  (SC-006).
- **Integration** (SQL Server, ADR 0006):
  - the migration on a slice 7 database (existing versions become attempt 1);
  - each conditional write's re-checks and `Stale` outcome;
  - the ending's pause and excusal cuts in one transaction;
  - the version key with two attempts sharing an applies-from date;
  - deleting a goal with two attempts, through the existing cascades and the new no-action foreign key;
  - `ReadRecentNoteKeysAsync` with both bounds, leaving out a note between two attempts and one after a last date;
    `GetRecentNotesAsync` on an attempt with no date reads nothing and gives no note;
  - the catalog counts;
  - `AddPauseAsync` refused after an ending;
  - demo goal-change loading;
  - log capture (HP-010);
  - the re-run SC-007 timing test of slice 6.
- **UI** (bUnit and host):
  - the goal page states;
  - Edit settings, EndGoalDialog, Start new attempt, the attempt view;
  - the Ended tab, Goal history, the Home Tags;
  - each row's own settings in the History card, the RecordDialog (sub line, field, progress line, help texts), the
    NoteDialog and `CommitmentRow` (AC-020, AC-080);
  - the Settings card's attempt start row and the attempt line, also for an attempt ended before it began;
  - SC-001's action counts: three actions to change a target, three to end, two to start a new attempt from the
    Ended tab;
  - focus management (HP-009);
  - startup lines;
  - smoke check 9.

  The tests that assert absences (spec "Earlier requirements and tests this slice changes") are updated in the step
  that changes their page.
- **Architecture**, as slice 7 extended them:
  - the `IGoalStore` rule widened to `AttemptService`;
  - a new Application fence keeping `IDemoGoalChangeStore` to `DemoGoalChangeService`;
  - `UiDependencyTests.NoUiTypeMayDependOnTheDemoGoalChangeStorePort`, the HT.Ui fence the contract promises;
  - `DemoGoalChangeService` added to `OnlyTheHostsOperatorTypesMayDependOnTheOperatorServices`;
  - `HT.Persistence.Habits.DemoGoalChangeStore` added to the internal-type theory of `PersistenceDependencyTests`
    (renamed to name all four demo stores);
  - the new public Domain and Application types added to `PublicSurfaceTests`' NodaTime theory:
    - `Outcome`, `EndingNote`, `GoalEnding`, `GoalAttemptVersions`, `GoalSettingsChange`, `GoalEndingEffect`;
    - `GoalChangeService` and its results;
    - `AttemptService` and its views;
    - `EntrySettings`, `GoalSettingsView`, `GoalChangeTexts`;
  - `UseCaseParameterTests` for the new use cases and store members.
- **Mutation:** Stryker over the new and changed Domain and Application code (ADR 0022).
- **Acceptance:** F009 through the AC workflow (ADR 0003), and all features with acceptance tests re-run: F001, F002,
  F004, F005, F007, F008 and F009.

## R-13 Privacy and logging (FR-022, FR-023, HP-010)

**Decision.**

- Every new use case checks access before the goal and passes the owner to every store call.
- An attempt is looked up only among the attempts of the acting user's goal named in the call ("This attempt does not
  exist."). There is no attempt identifier.
- The goal history reads only the acting user's goals.
- `EndingNote` and `Outcome` never appear in exception messages. Mapping faults name no value.
- Results carry texts and fields only.
- Log capture tests prove that no ending note, note, value, goal name, habit name or unit is logged (HP-010).
- Demo goal-change loading writes only for the account it creates (ADR 0013).

**Why.** Constitution III, and the same mechanisms as slices 4 to 7.
