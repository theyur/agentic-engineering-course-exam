# 0027 — Attempts and per-attempt settings versions: a goal holds its attempts, each with its own versions

**Status:** Accepted, 2026-11-22, by the owner with the plan of slice 8. Comes from the plan of slice 8,
`specs/009-goal-changes` (research R-01 and R-05; plan, Constitution Check "Decision Discipline"), and was written
in its design step before any code. Applies
[ADR 0025](0025-goal-change-rules.md) (a change keeps the schedule kind; a new attempt may have any settings; an
ending keeps the attempt's first version) to the Domain model and the storage. Keeps
[ADR 0016](0016-time-zones.md) (settings versions with their time zone, at most one waiting after the current
date), [ADR 0018](0018-evaluation-on-read-and-demo-history.md) and [ADR 0020](0020-derived-figures-on-read.md)
(states and figures derived on read) as they are.

## Context

- SPEC §5.4: restarting an ended goal creates a new attempt with its own history and a streak from zero. ADR 0025
  lets a new attempt have any settings, another schedule kind included, while every settings version of one
  attempt keeps one schedule kind.
- Slices 4 to 7 store one attempt per goal (`GoalAttempts`, attempt 1) and one list of settings versions per goal
  (`GoalSettingsVersions`, keyed by goal and applies-from date). Every reader asks `GoalTimeline` for "the goal's"
  version of a date.
- An attempt that ends before it begins keeps its first version (ADR 0025, spec FR-009). That version can apply
  from a date after the next attempt's start: attempt 1 from Mon 23 Nov, ended on Tue 17 Nov, attempt 2 from Wed
  18 Nov (spec AC-035). So one goal-level list of versions would no longer be in date order.
- The goal keeps its identifier, habit and own name across attempts (spec FR-012), and records are keyed by goal
  and date (slice 5).
- The locked acceptance code of slices 1 to 7 may not be read, and every existing constructor, method signature,
  positional record and port member must keep compiling and meaning what it means for a goal with one attempt
  (spec FR-024).
- Constitution, Decision Discipline: the choice spans features (slice 9's bands and catalog aggregates read goals
  through it), is hard to reverse (once a second attempt is stored, the migration cannot be undone), and rejects
  real modelling alternatives.

## Options

1. **A goal holds an ordered list of attempts, each with its own settings versions (chosen).** Each attempt is
   timed from its own start date with its own versions, and cut at its last date.
2. **One goal-level version list with an attempt number on each version.** The dates of one list would not
   increase once an attempt that ended before it began keeps its first version, so `VersionOf(goal, date)` would
   pick the wrong attempt's version, and every reader would have to filter by attempt anyway. Rejected.
3. **Each new attempt as a new goal row linked to the first.** The goal must keep its identifier, habit and name
   (FR-012). Every page, the record keys and the History would have to follow a chain of goals. Rejected.
4. **`GoalAttempt` carries its versions.** Changes a record type's equality and adds a list to a type the locked
   acceptance code may compare. Rejected.

## Decision

**Domain (`HT.Domain.Goals`, research R-01).**

- `Goal` holds an ordered list of attempts. Each attempt has its number, its start date, its own settings versions
  in date order, and once ended a `GoalEnding`: the ending date, the `Outcome` and an optional `EndingNote`. The last
  date is not stored; `GoalTimeline` derives it (spec FR-008, FR-026, ADR 0020).
- A new public constructor takes the attempts as `GoalAttemptVersions` (an attempt and its versions). Its
  invariants: attempt numbers run 1, 2, … with no gap; every attempt but the latest is ended; attempt 1's first
  version applies from the goal's start date, and every later attempt's from its own start date; dates strictly
  increase within an attempt; all of an attempt's versions have one schedule kind; a later version of a per-week
  attempt starts on a Monday; each attempt starts after the previous attempt's ending date; an ended attempt has no
  version after its last date except its first. Each violation is an `ArgumentException` whose message names no
  value.
- **The kept constructor.** Slice 4's `Goal(id, owner, habit, startDate, ownName, attempt, versions)` stays and builds
  a goal with that one attempt, through the same checks. Its rule "only the time zone differs between versions"
  becomes "the same schedule kind" (ADR 0025).
- **Existing members keep their meaning for a goal with one attempt:** `StartDate` is attempt 1's start date (the
  goal's start date, slice 4 FR-016); `Attempt` is the latest attempt; `Versions` are the latest attempt's versions.
  New members: `Attempts`, `IsEnded` (the latest attempt has an ending) and `VersionsOf(number)`.
- **`StartDate` is no longer anyone's "first date".** A later attempt can start before it (the example above). Every
  reader that used `goal.StartDate` as the goal's first date or as a fallback switches to the attempt it means:
  attempt 1 is timed from the goal's start date, every later attempt from its own start date.
- `GoalAttempt` only gains an init-only `Ending`, which keeps its constructor and value equality.

**Storage (migration `GoalChanges`, research R-05, data-model.md "Tables").**

- `GoalAttempts` gains `EndingDate` (`date`, null), `Outcome` (`varchar(16)`, null, the enum as a string like the
  other enums) and `EndingNote` (`nvarchar(500)`, null). The mapping, not a check constraint, checks that
  `EndingDate` and `Outcome` are both set or both null, and that `EndingNote` is null without them.
- `GoalSettingsVersions` gains `AttemptNumber` (`int`, not null; every existing row is attempt 1's, the only attempt
  slices 4 to 7 write). Its key becomes (`GoalId`, `AttemptNumber`, `AppliesFrom`), so two attempts can have versions
  from the same date.
- A foreign key (`GoalId`, `AttemptNumber`) → `GoalAttempts` (`GoalId`, `Number`) makes an orphan version
  impossible. It is **no action** on delete (`DeleteBehavior.NoAction`), not a cascade. The existing cascades
  `Goals` → `GoalSettingsVersions` and `Goals` → `GoalAttempts` stay. A cascade on the new key would give SQL
  Server two cascade paths to `GoalSettingsVersions`, and SQL Server refuses such a constraint ("may cause cycles or
  multiple cascade paths"). Deleting a goal still works: both existing cascades run in the one delete statement, and
  SQL Server checks a no-action constraint only at the end of that statement, when the versions are gone too. An
  integration test deletes a goal with two attempts.
- The migration's order on a populated database: add `AttemptNumber` with the default 1; drop the old key, then add
  the new key and the foreign key; add the three nullable attempt columns.
- **The migration is one-way in practice.** `Down` reverses the steps, but it is valid only while every version
  belongs to attempt 1, as for every migration that narrows a key. Once a second attempt is stored, two versions of
  one goal can share an applies-from date, and the old key (`GoalId`, `AppliesFrom`) cannot be restored without
  deleting data. Rolling back after that needs a new forward migration.
- The three conditional writes (`TryChangeSettingsAsync`, `TryEndAsync`, `TryStartAttemptAsync`) lock the account
  row and then the goal row, check the stored zone and goal revision, and re-check their own precondition before
  writing, as goal creation and the time zone change do (research R-05).

## Consequences

- Every timeline rule runs per attempt with slice 4's code: `GoalTimeline` builds a one-attempt view of each
  attempt internally (research R-02), and the Application never filters versions by attempt itself.
- The locked acceptance code of slices 1 to 7 keeps compiling and passing: no existing constructor, signature,
  positional record or port member changes, and every member gives today's answer for a goal with one attempt, no
  ending and no change.
- Slice 9 (community) and later slices read a goal's attempts and versions through this model. A different model
  needs a superseding ADR and a new forward migration.
- A database that has stored a second attempt cannot be migrated down past `GoalChanges` without losing data. The
  hosted demo is rebuilt from an empty database on every deployment and refresh (ADR 0015, ADR 0018), so it is not
  affected.
- The extra key column and foreign key cost one more index column on `GoalSettingsVersions`. Goal deletion relies on
  SQL Server checking the no-action constraint at the end of the statement, which an integration test pins.
