# Feature Specification: Recording and evaluation

**Feature Branch**: `claude/speckit-roadmap-slice-5-bd9f2d`

**Created**: 2026-11-18

**Status**: Draft

**Input**: User description: "roadmap slice 5"

Roadmap outcome (slice 5, `recording-and-evaluation`): a user records what actually happened, including partial
amounts and notes. Unrecorded results are Pending, become Missed after the 48-hour grace period, and corrections
are evaluated with the original rules (SPEC §5.2). The roadmap adds: evaluation is a deterministic function of
stored facts and a supplied time; demo data has to show Pending, Met, Failed and Missed results relative to the
current time; a correction's effect on reports is verified in slice 6.

## Clarifications

### Session 2026-11-18 (specify)

The three product rows the roadmap places in this slice, answered by the owner during `/speckit-specify`:

- Q: How does the 48-hour grace period behave around local clock changes (SPEC §11.1, second part)? → A: Exactly
  48 elapsed hours after the commitment's or period's end instant. A clock change in between does not change its
  length, so the deadline can read 23:00 or 01:00 on the local clock (FR-016, US2-AS13).
- Q: When does a recorded result become final: on recording, or when the grace period ends (SPEC §5.2;
  `CONTEXT.md` Pending)? → A: When the grace period ends. Every commitment or period that has ended stays Pending
  for its 48 hours, with or without records, even when its records already reach the target; then it is Met,
  Failed or Missed. `CONTEXT.md`'s Pending entry stays as it is (FR-012, US2-AS11).
- Q: How is an entirely unrecorded Reduce commitment or period evaluated (SPEC §11.4)? → A: Missed, like any
  unrecorded result. A Reduce goal is Met only on an explicit "No" or an amount within the limit, 0 included.
  `CONTEXT.md`'s Missed entry and the RecordDialog's Reduce rule stay; the rule is no longer provisional (FR-013,
  US2-AS12).

### Session 2026-11-18 (clarify)

- Q: Where do the demo results (Met, Failed, Missed, Pending, partial amounts, notes) live: in a new demo account,
  or in the existing demo accounts' goals? → A: A new account `demo4@example.com`, with three goals that started
  28 days before the run and their records and notes; `demo1` to `demo3` stay exactly as in slice 4, so F004
  stays locked (D-6; FR-030, FR-031). Who creates the account was revised on 2026-11-18 (critic revision, below).
- Q: How do records work for fixed-days goals and for goals counted per day or per week? → A: A fixed-days goal
  has at most one record per scheduled day, and recording again replaces its value; a per-day or per-week goal
  holds separate records, up to 50 per period; any record can be changed or deleted at any time; corrections keep
  no history of earlier values (D-7; FR-003, FR-006).
- Q: Does a note belong to the day or week, and can it be written after the result is final? → A: One note per
  commitment or evaluation period, not per record; it can be written for any commitment or period that has
  begun, at any time, final ones included, and never changes the result (D-9; FR-007).
- Q: Are the limits right: count 0 to 9999, duration 0 to 1440 minutes per record, notes up to 500 characters,
  up to 50 records per period? → A: Yes, as written (D-8; FR-005, FR-007).

### Session 2026-11-18 (critic revision)

Answered by the owner after critic pass 1 of this spec; the technical rows are recorded in
[ADR 0018](../../docs/adr/0018-evaluation-on-read-and-demo-history.md):

- Q: The locked F001 criteria AC-057 to AC-060 say that demo loading's results name `demo1` to `demo3` "and no
  other email", so demo loading cannot create `demo4@example.com`. How does `demo4` come in? → A: A separate
  startup task, *demo history loading*, after demo loading, creates `demo4@example.com` with its goals, records
  and notes; demo loading and its results stay exactly as F001 and F004 lock them (D-6; FR-030, FR-031).
- Q: How does the hosted demo keep showing Met, Failed, Missed, Pending and Due results on every day until
  2027-01-31 (D-5)? → A: The hosting workflow also runs its refresh mode once a day on a schedule, shortly after
  midnight UTC, so demo history loading writes `demo4`'s data around each new day (HP-009). Re-asked after critic
  pass 2, which showed that the question had wrongly said Pending and Due would disappear without refreshes:
  every state stays present, but each day since the last load adds an unrecorded, later Missed, day at the top
  of each daily goal's history, so after about a week its pages show mostly Missed rows. → A: Keep the daily
  scheduled refresh.
- Q: Does anything run in the background for evaluation (D-4)? → A: No. Every state is computed when it is read,
  from the stored records, the settings version and the supplied time; no job writes Missed or stores results
  (FR-010, HP-010).
- Q: When a record is saved with no note, or an empty one, what happens to the note already on that commitment
  or period? → A: It stays. A note given with a record adds or replaces the note only when it has text; a note
  is removed only by saving it empty on its own (FR-007).

## Decisions this slice cites

Recorded in earlier slices and not decided again here:

- Use cases take the acting user as an explicit parameter and return named outcomes that carry the literal text
  this spec gives; expected refusals are outcomes, not errors ([ADR 0005](../../docs/adr/0005-use-case-convention.md)).
- The administrator role grants no access to private data (slice 1 FR-015; SPEC §3, §8). Goals are for
  registered users only (slice 4 FR-021).
- Partly built pages show only the sections and links whose slice has shipped
  ([ADR 0008](../../docs/adr/0008-partly-built-pages.md)).
- Time zones: the account's own, kept with each goal settings version; a day runs from local midnight to local
  midnight in the time zone of the version in effect for it ([ADR 0016](../../docs/adr/0016-time-zones.md),
  [ADR 0017](../../docs/adr/0017-embedded-time-zone-data.md); slice 4 FR-011 to FR-013, FR-026, FR-027). ADR 0016
  already accepts that "records belong to a date, not to an instant".
- What a target means per schedule kind (slice 4 FR-004, Clarifications 2026-11-17): restated below and
  evaluated by this slice.
- Demo loading writes private demo data only for the demo accounts it creates in the same run
  ([ADR 0013](../../docs/adr/0013-private-demo-data.md), [ADR 0007](../../docs/adr/0007-startup-operator-tasks.md)).
  ADR 0007, ADR 0013 and ADR 0015 each say that demo data relative to the current time (this slice) needs its
  own decision: [ADR 0018](../../docs/adr/0018-evaluation-on-read-and-demo-history.md) gives it (evaluation on
  read, demo history loading, a daily scheduled refresh).
- Integration and UI tests: [ADR 0006](../../docs/adr/0006-integration-tests.md). Acceptance tests:
  [ADR 0003](../../docs/adr/0003-acceptance-tests.md).

## Slice 4 behaviour these tests use

Restated here, unchanged, so that this spec alone gives the rules and texts its acceptance tests depend on:

- **Goals** (slice 4 FR-001 to FR-010): a registered user creates a goal on a Published catalog habit or one of
  their own custom habits, with Build or Reduce; the measurement yes/no, count or duration; for count and
  duration the amount (Build 1 to 999, Reduce 0 to 999) and the unit (count: 1 to 20 characters; duration:
  minutes or hours); the schedule, fixed days of the week or N times (1 to 20) per day or per week; a start date
  from today to 365 days ahead; and an optional name. A Reduce count or duration goal of N times has N = 1. A goal
  without a name of its own is named by its habit's name, ", " and its target text ("Reading, 15 min, 5 days a
  week", "Sugar, at most 1 sweet a day", "No phone in bed, avoided every day", "Morning yoga" when named).
- **Schedule kinds** (slice 4 FR-003, FR-013): a goal on fixed days is *commitment-based*: each scheduled day on
  or after the start date is one commitment. A goal of N times per day or per week is *period-based*: each day
  on or after the start date, or each Monday-to-Sunday week from the first one that starts on or after the start
  date, is one evaluation period. The days before a per-week goal's first full week belong to no period.
- **Days and time zones** (slice 4 FR-011, FR-012, FR-026, FR-027): every commitment and evaluation period starts
  and ends at local midnight in the time zone of the settings version in effect for its date, so a day on which
  the clocks change lasts 23 or 25 hours. A goal's *current date* at an instant is the latest date whose day of
  the goal has begun. A change of the account's time zone applies to each goal from its next commitment or
  period; earlier ones keep their time zone. Acceptance tests create accounts with a time zone at registration
  ("Europe/Kyiv", "America/New_York", or none for the default "UTC").
- **What the target means** (slice 4 FR-004):
  - commitment-based: a commitment is met when that day's recorded result reaches the target: Build at least the
    amount (yes/no: done), Reduce at most the amount (yes/no: not done);
  - period-based: Build is met when the period has at least N records that each reach the amount (yes/no: N
    "done" records); Reduce yes/no is met when the period has at most N "done" records; Reduce count or duration
    is met when the period's total is at most the amount.
- **Privacy texts** (slices 1 and 4): an anonymous visitor gets "You are not signed in."; an administrator gets
  "Goals are for registered users."; a goal identifier that is not one of the acting user's goals gives "This
  goal does not exist.", the same as one that belongs to nothing. A range of dates is 1 to 366 days, otherwise
  "Choose a range of 1 to 366 days.".
- **Demo loading** (slice 1 FR-016, slice 2 FR-030, slice 4 FR-040): with a valid demo password it loads the seed
  catalog into an empty catalog first, then creates `demo1@example.com` to `demo3@example.com` with their demo
  custom habits and demo goals; every slice-4 demo goal starts on the day of the run or later, so it has no past
  results. Results name the demo email but never a goal, habit or account name. An account that already exists
  gets no further result and none of its private data is read or changed. The seed catalog has, among others,
  the Published habits "Reading" (Mind, Build), "Yoga" (Body, Build) and "Sugar" (Health, Reduce).

## What acceptance tests can reach

ADR 0003 limits acceptance tests to the Application use cases, the two `HT.Persistence` entry points and the
Domain types they use. As in slices 1 to 4:

- **FR-NNN** are application behaviours, reachable through Application use cases. Demo loading and demo history
  loading (FR-030, FR-031) are operator capabilities that acceptance tests call directly.
- **HP-NNN** are host, page and integration requirements, verified by unit, integration and selective UI tests,
  not by acceptance tests.
- Scenarios marked *(page)* describe page or browser behaviour and are expected to be listed as "Not testable
  through the API" in `acceptance.md`.
- FR numbers are grouped by topic; FR-020 to FR-029 are not used, and demo data starts at FR-030.
- The current time is supplied to every use case of this slice (constitution II), so acceptance tests fix "now"
  and move it forward instead of waiting for real time. A goal's start date cannot be in the past (slice 4), so
  scenarios that need past commitments create the goal at an earlier supplied time and then record and read at
  later ones.

Test data: all acceptance scenarios share one database in one run (ADR 0003). Scenarios use their own accounts
and goals, and check lists by inclusion, exclusion and order, never by totals over the whole database. The demo
scenarios start from a reset database.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Record what happened (Priority: P1)

A registered user records the actual result of one commitment or one day or week of a period-based goal: "Yes"
or "No" for a yes/no goal, an amount for a count or duration goal, below, at or above the target, optionally with
a note. The record is kept as entered, including partial amounts.

**Why this priority**: it is the outcome of the slice; consistency and streaks (slice 6) count nothing without
records.

**Independent Test**: a user with a daily duration goal of 20 minutes records 12 minutes today, reads today's
commitment and sees 12 of 20 minutes kept.

**Acceptance Scenarios**:

1. **Given** a commitment-based goal "Reading, 20 min daily" (Build, duration 20 minutes, all seven days),
   **When** its owner records 12 minutes for today, **Then** today's commitment has one record of 12 minutes and
   its state is Due (FR-010).
2. **Given** the same commitment with its record of 12 minutes, **When** the owner records 25 minutes for today,
   **Then** the commitment's one record is 25 minutes (a commitment has at most one record, FR-003).
3. **Given** a period-based goal "Yoga, 3 times a week" (Build, yes/no, 3 times per week), **When** its owner
   records "Yes" on Monday and on Wednesday of the current week, **Then** the week has two records and its
   progress is 2 of 3 times.
4. **Given** a period-based goal "Sugar, at most 1 sweet a day" (Reduce, count, at most 1 "sweet"), **When** the
   owner records 1 sweet and then 2 sweets for today, **Then** the day has two records and its total is 3.
5. **Given** a commitment-based goal on Monday, Wednesday and Friday, **When** the owner records for a Tuesday,
   **Then** the result is "Nothing is scheduled on this date." and nothing is recorded; the same holds for a date
   before the goal's start date and for a day before a per-week goal's first week.
6. **Given** a goal on all seven days, or a per-day goal, **When** the owner records for tomorrow, **Then** the
   result is "You cannot record a day that has not begun." and nothing is recorded; for a Mon/Wed/Fri goal
   whose tomorrow is a Tuesday the result is "Nothing is scheduled on this date." (FR-002 checks that first).
7. **Given** any value checks of FR-005, **When** the owner records, **Then** each refusal is given with its
   message, one at a time in FR-005 order, and nothing is recorded (one scenario per message).
8. **Given** a record with the note "  Back was tired.  ", **When** the owner reads the commitment, **Then** its
   note is "Back was tired." (FR-007).
9. *(page)* **Given** the goal page, **When** the owner chooses "Record", **Then** the RecordDialog opens with the
   "For" select, the field for the goal's measurement, the target in its sub line and the progress under the
   field (HP-002).

---

### User Story 2 - Pending, Met, Failed and Missed (Priority: P1)

Every commitment and evaluation period has a state at the supplied time: Upcoming before it begins, Due while it
runs, Pending for the 48-hour grace period after it ends, and then Met, Failed or Missed for good.

**Why this priority**: SPEC §5.2's evaluation rules are the core of the product; slices 6 to 9 count these
states.

**Independent Test**: create a daily goal at a fixed time, record nothing on day 1, a partial amount on day 2 and
the full amount on day 3, and read the states just before and just after each grace period ends.

**Acceptance Scenarios**:

1. **Given** a commitment that has not begun, **When** its owner reads it, **Then** its state is Upcoming; from
   its start until its end, Due.
2. **Given** a commitment with no record that ended less than 48 hours ago, **When** its owner reads it, **Then**
   its state is Pending and it gives the instant its grace period ends.
3. **Given** a commitment with no record, **When** it is read just before its grace period ends, **Then** it is
   Pending; at the instant the grace period ends and after, it is Missed, without any action by the user or an
   operator.
4. **Given** a Build duration commitment of 20 minutes, **When** its result is final, **Then** a record of 19
   minutes gives Failed, 20 minutes Met and 21 minutes Met; the recorded amount is kept in each case.
5. **Given** a Reduce count commitment of at most 2 cups, **When** its result is final, **Then** 1 cup gives Met,
   2 cups Met, 3 cups Failed.
6. **Given** a Build yes/no commitment, **Then** "Yes" gives Met and "No" gives Failed; **given** a Reduce yes/no
   commitment ("avoided"), "No" gives Met and "Yes" gives Failed.
7. **Given** a per-week Build yes/no goal of 3 times a week, **Then** a final week with 3 "Yes" records is Met,
   with 2 "Yes" and 1 "No" Failed, with 4 "Yes" Met.
8. **Given** a per-week Build count goal "20 pages, 3 times a week", **Then** a final week with records 20, 25
   and 12 pages is Failed (2 of 3 times reach 20) and records 20, 25, 12 and 30 pages Met.
9. **Given** a per-day Reduce count goal "at most 1 sweet a day", **Then** a final day with records 0 and 1 is
   Met (total 1) and records 1 and 1 Failed (total 2).
10. **Given** a per-day Reduce yes/no goal "at most once a day", **Then** a final day with one "Yes" is Met, with
    two "Yes" Failed, with one "No" Met.
11. **Given** a commitment or period with a record that reaches the target, **When** it is read during its grace
    period, **Then** its state is Pending, with its progress; at the end of the grace period it is Met (FR-012).
12. **Given** a Reduce commitment or period with no record at all, **When** its grace period ends, **Then** its
    state is Missed, not Met; with a "No" record, or a record of 0, it is Met (FR-013).
13. **Given** a commitment in "Europe/Kyiv" on Saturday 24 October 2026, the day before the clocks go back,
    **When** its grace period is read, **Then** it ends exactly 48 hours after the commitment's end, at 23:00
    Kyiv time on Monday 26 October, not at midnight; read at 22:59 it is Pending, at 23:00 it is final (FR-016).

---

### User Story 3 - Correct a finalized result (Priority: P1)

A user corrects an old record, adds a record to a Missed commitment, or removes a record made by mistake. The
48-hour deadline is not an edit lock. The corrected result is evaluated with the rules that applied on that
date, exactly as if it had been recorded on time.

**Why this priority**: SPEC §5.2 and constitution II; slice 6 recomputes reports from these results.

**Independent Test**: let a commitment become Missed, then add a record that reaches the target and read it as
Met; let another become Met, change its record below the target and read it as Failed.

**Acceptance Scenarios**:

1. **Given** a Missed commitment, **When** its owner records an amount that reaches the target, **Then** its
   state is Met and the result says that it was a correction.
2. **Given** a Met commitment, **When** its owner changes its record to an amount below the target, **Then** its
   state is Failed and the result says that it was a correction.
3. **Given** a final commitment with a record, **When** its owner deletes the record, **Then** the commitment has
   no record and its state is Missed.
4. **Given** a Failed per-week period, **When** its owner adds the missing "Yes" record for a date in that week,
   **Then** the week is Met.
5. **Given** two goals with the same settings, one recorded on time and one recorded only after its grace period
   by correction with the same values, **When** both are read, **Then** each commitment or period has the same
   state in both (constitution II).
6. **Given** a commitment in the old time zone of a goal whose owner has since changed time zone, **When** it is
   corrected, **Then** it keeps the start, end and grace-period instants of its own settings version (slice 4
   FR-026), not those of the current one.
7. **Given** a record identifier that is not a record of that goal, **When** the owner changes or deletes it,
   **Then** the result is "This record does not exist.".
8. *(page)* **Given** the History table, **Then** a commitment row without a record offers "Add record" and one
   with a record "Edit"; a period row offers "Add record" while it has fewer than 50 records, final or not, and
   each of its records can be opened with "Edit"; the card says "Corrections are evaluated with the rules of
   that date." (HP-001).

---

### User Story 4 - Notes (Priority: P2)

A user writes a private note on a commitment or evaluation period, for example to explain a partial or Missed
result. A note never changes the result and can be added or edited at any time, also after the result is final.

**Why this priority**: SPEC §5.2 lists notes; reports and the Home page show them later (slice 6).

**Independent Test**: let a commitment become Missed, add the note "Away for the weekend.", and read it back
with the state unchanged.

**Acceptance Scenarios**:

1. **Given** a Missed commitment, **When** its owner adds the note "Away for the weekend.", **Then** the
   commitment has that note and is still Missed.
2. **Given** a commitment with a note, **When** the owner saves an empty note or one of only spaces on its own,
   **Then** the note is removed.
3. **Given** a per-week period with the note "Knee was sore.", **When** the owner records "Yes" for another date
   of that week with no note, or with a note of only spaces, **Then** the week's note is still "Knee was
   sore."; with the note "Better today." it becomes "Better today." (FR-007).
4. **Given** a note of 501 characters, **Then** the result is "The note can have at most 500 characters." and
   nothing changes; 500 characters are accepted.
5. **Given** a per-week period, **When** the owner writes a note for any date of that week that has begun,
   **Then** the week has that one note; for a date of the current week that has not begun the result is "You
   cannot record a day that has not begun.".
6. **Given** a goal with notes on 7 commitments, **When** its owner reads its recent notes, **Then** the 5 with
   the latest dates are returned, newest first, each with its date and state.
7. *(page)* **Given** the History table, **Then** each row shows its note; the Recent notes card shows the recent
   notes, each with its date and state; the NoteDialog says that notes are private and do not change the result
   (HP-003).

---

### User Story 5 - History of a goal (Priority: P1)

A user reads the history of a goal over a range of dates: each commitment or period with its state, its records,
its progress against the target, its note, and, while it is not final, when its grace period ends.

**Why this priority**: it is how the user sees the states of US2 and reaches the corrections of US3.

**Independent Test**: read 14 days of a goal with Met, Failed, Missed, Pending, Due and Upcoming entries.

**Acceptance Scenarios**:

1. **Given** a goal and a range, **When** its owner reads the history, **Then** every commitment or period of the
   evaluation calendar in that range is returned, in date order, each with its state at the supplied time
   (FR-009).
2. **Given** a duration commitment of 20 minutes with a record of 12, **Then** its progress is 12 of 20 minutes;
   a per-week Build period of 3 times with 2 records that reach the amount gives 2 of 3 times; a per-day Reduce
   count period gives its total and the limit.
3. **Given** a goal, **When** its owner reads what can be recorded now, **Then** the result is every Due
   commitment or period, if any (two only during the overlap after a move east), and every Pending one, newest
   first (FR-011); on an unscheduled day of a fixed-days goal there is no Due commitment.
4. **Given** a range longer than 366 days, **Then** the result is "Choose a range of 1 to 366 days.".
5. *(page)* **Given** the goal page, **Then** the History card lists the commitments or periods that have begun,
   newest first, with Date, Recorded, Result (StatusChip) and Note (HP-001).

---

### User Story 6 - Recording privacy (Priority: P1)

Records and notes are private: only the goal's owner can record, correct, read or annotate. Another registered
user, an administrator and an anonymous visitor get nothing, and no result reveals that someone else's goal or
record exists.

**Why this priority**: SPEC §3 and constitution III; a precondition of every use case of this slice.

**Independent Test**: user A records with a note; user B, an administrator and an anonymous visitor each try to
read A's history, record on A's goal, change A's record and read A's notes.

**Acceptance Scenarios**:

1. **Given** user A's goal, **When** user B calls any use case of this slice with its identifier, **Then** the
   result is "This goal does not exist." and nothing changes.
2. **Given** user A's record identifier, **When** user B changes or deletes it on one of B's own goals, **Then**
   the result is "This record does not exist.".
3. **Given** any goal, **When** an administrator calls any use case of this slice, **Then** the result is "Goals
   are for registered users." and no data is returned.
4. **Given** any goal, **When** an anonymous visitor calls any use case of this slice, **Then** the result is "You
   are not signed in.".

---

### User Story 7 - Demo results (Priority: P2)

Reviewers who sign in as the demo account with history find Met, Failed, Missed, Pending and Due results, partial
amounts and notes, without waiting for real time to pass.

**Why this priority**: constitution VI and SPEC §10; needed for the hosted demo, not for the core flow.

**Independent Test**: reset the database, run demo loading and then demo history loading at a fixed time, sign in
as the demo account with history and read its goals' histories.

**Acceptance Scenarios**:

1. **Given** an empty database, a valid demo password and a run at a fixed time, **When** demo loading and then
   demo history loading run, **Then** `demo4@example.com` is created with the goals, records and notes of
   FR-030, and demo history loading's results are "created", "demo goals added" and "demo records added" for
   `demo4@example.com`.
2. **Given** that run, **When** `demo4@example.com` reads its goals' histories at the run time, **Then** they show
   at least one Met, Failed (with a partial amount), Missed, Pending and Due result, and the notes of FR-030.
3. **Given** `demo4@example.com` already exists, **When** demo history loading runs again, **Then** its result is
   "already exists", with no goal or record result, and its goals, records and notes are unchanged.
4. **Given** demo history loading's results, **Then** they name the demo email but never a goal, habit, note,
   unit or amount.
5. **Given** demo loading, **Then** its results and the slice-4 demo accounts' goals are those of slices 1 and 4,
   unchanged, and it never names `demo4@example.com` (FR-031).
6. **Given** no demo password, or one that fails the password rule, **When** demo history loading runs, **Then**
   its result is "no demo password configured" or "demo password does not meet the password rule", as demo
   loading's, and no account is created.

---

### Edge Cases

- A record for a day whose time zone version changed: the commitment's instants and its grace period come from its
  own version (slice 4 FR-027); a gap or overlap after a large move does not merge or split commitments, so two
  consecutive commitments can each be Pending with different grace ends.
- After a large move east, the first commitment of the new version may already have ended when the change is
  made (slice 4 FR-027); it is Pending or final by the same rules, from its own end.
- Recording for today on a per-week goal on a Sunday at 23:59, and on Monday at 00:00: the record belongs to the
  date given, so it falls in the week of that date, never in the week of the instant of saving.
- A record saved exactly at the end of a grace period counts as a correction (the result is final at that
  instant, US2-AS3).
- A per-week period whose first days belong to no week (a per-week goal started mid-week): those days take no
  record ("Nothing is scheduled on this date.").
- An amount above the target is kept ("amounts above target", SPEC §5.2): 45 of 20 minutes is Met, and the 45 is
  reported.
- A Reduce goal with amount 0 ("at most 0 cigarettes a day"): a record of 0 is Met, 1 is Failed.
- A duration goal in hours ("1 h"): records are in minutes and compared with 60 minutes.
- A per-day goal of N times with N greater than 1 ("Take vitamins, 2 times a day", yes/no): two "Yes" records on
  the day are Met, one "Yes" Failed.
- Deleting the only record of a commitment that is still Due or Pending makes it unrecorded again; that is not a
  correction.
- A goal whose catalog habit is Unpublished (slice 4 FR-034) keeps being recorded and evaluated as usual.
- A period that reaches the record limit (FR-005) takes no further record, but its records can still be changed
  or deleted.

## Requirements *(mandatory)*

### Functional Requirements (application behaviour, on the AC surface)

**Records**

- **FR-001**: A registered user MUST be able to record, for one of their own goals and one date, what actually
  happened: for a yes/no goal "Yes" (done) or "No" (not done); for a count goal a whole number of the goal's unit;
  for a duration goal a whole number of minutes. A record belongs to that date, not to the instant at which it is
  saved (ADR 0016), and the date decides which commitment or evaluation period it belongs to.
- **FR-002**: A record MUST be accepted only for a date that belongs to a commitment or evaluation period of the
  goal (on or after the start date; for a commitment-based goal a scheduled day; for a per-week goal a date in one
  of its weeks), otherwise "Nothing is scheduled on this date."; and, checked after that, only when that date's
  day has begun on the goal's timeline (the goal's current date or earlier, slice 4 FR-012), otherwise "You
  cannot record a day that has not begun.". There is no earliest date: any date from the start date on can be
  recorded or corrected.
- **FR-003**: A commitment MUST have at most one record: recording for a commitment that has one replaces its
  value. An evaluation period MUST hold any number of records, up to 50, each on a date of the period; recording
  for a period adds a record. Every record has an identifier, by which its owner changes its value or deletes it;
  the date of a record never changes.
- **FR-004**: Records MUST keep the amount as entered, below, at or above the target (partial progress is
  preserved, SPEC §5.2); a record never stores a value that does not belong to the goal's measurement (an amount
  with "Yes", "Yes" with an amount): such values are not refused and not stored.
- **FR-005**: Recording or changing a record MUST give one result, checking in this order: (1) an anonymous visitor
  "You are not signed in."; (2) an administrator "Goals are for registered users."; (3) the goal "This goal does
  not exist." (slice 4 FR-020), and for a change or deletion the record "This record does not exist." (a record
  of another goal, of another account or of nothing); (4) the date (FR-002); (5) the value: yes/no with neither
  "Yes" nor "No" "Choose Yes or No."; a count missing or not a whole number from 0 to 9999 "Enter a whole number
  from 0 to 9999."; a duration missing or not a whole number from 0 to 1440 "Enter a whole number of minutes from
  0 to 1440."; (6) the note (FR-007); (7) a period that already has 50 records "This evaluation period already
  has 50 records." (adding only).
- **FR-006**: Recording, changing and deleting MUST return the state of the commitment or period after the change
  (FR-010) and whether the change was a *correction*: a change to a commitment or period whose result was final
  at the supplied time. A correction keeps no history of earlier values; the result is always computed from the
  records as they are now (constitution II).

**Notes**

- **FR-007**: A commitment or evaluation period MUST have at most one note, private to the goal's owner. A note is
  trimmed; longer than 500 characters it gives "The note can have at most 500 characters." and changes nothing.
  A note can be saved on its own, or given with a record (recording or changing one):
  - saved on its own, a note replaces the existing note, and one that is empty after trimming removes it;
  - given with a record, a note that has text after trimming adds or replaces the note; no note, or one that is
    empty after trimming, leaves the existing note unchanged.

  A note is saved on its own for a date that passes FR-002's checks, whatever the state of its commitment or
  period, including after it is final; for a per-week period, any such date of the week names the period. A note
  never changes the state.
- **FR-008**: Reading a goal's recent notes MUST return at most 5 notes, those of the commitments or periods with
  the latest dates, newest first, each with its date (a per-week period: its first and last date), the note and
  the state at the supplied time.

**Reading the history**

- **FR-009**: Reading a goal's history for a range of dates (first and last date included, 1 to 366 days,
  otherwise "Choose a range of 1 to 366 days.") MUST return, in date order, each commitment or evaluation period
  of the goal's evaluation calendar in that range (slice 4 FR-013) with: its date (a per-week period: its first
  and last date); its start and end instants; its state at the supplied time (FR-010); while it is Due or
  Pending, the instant its grace period ends (FR-016); its records (identifier, date and value), in date order and
  then in the order they were stored; its progress (FR-014); its note; and whether a change now would be a
  correction.
- **FR-010**: The state of a commitment or evaluation period at the supplied time MUST be exactly one of:
  - **Upcoming**: its start instant is after the supplied time;
  - **Due**: it has begun and not ended;
  - **Pending**: it has ended and its grace period (FR-016) has not ended, with or without records (FR-012);
  - **Met**, **Failed** or **Missed**: its grace period has ended, so it is final (FR-012), with Met or Failed by
    FR-014 when it has at least one record, and Missed when it has none, for Build and Reduce alike (FR-013).

  The state is computed from the stored records, the goal settings version in effect for its date and the
  supplied time; it changes from Pending to final without any action by the user or an operator.
- **FR-011**: Reading what can be recorded now MUST return every Due commitment or period of the goal (none on an
  unscheduled day of a fixed-days goal; two only while the old and new days overlap after a move east, slice 4
  FR-027) and every Pending one, newest first, each as FR-009 gives it. It is what the record form offers in its
  "For" choice.

**Evaluation**

- **FR-012**: Every commitment or period that has ended MUST stay Pending until its grace period ends, whether or
  not it has a record and whether or not its records already reach the target (Clarifications 2026-11-18;
  `CONTEXT.md` Pending). It becomes final, Met, Failed or Missed, only when its grace period ends. Before that,
  its progress (FR-014) shows how far it has come, but no Met or Failed is given.
- **FR-013**: A commitment or period of a Reduce goal with no record at its grace period's end MUST be Missed,
  like any unrecorded result (Clarifications 2026-11-18; SPEC §11.4; `CONTEXT.md` Missed). A Reduce goal is Met
  only on an explicit record: "No" for yes/no, or an amount (0 included) within the limit. Nothing recorded is
  never treated as success.
- **FR-014**: A final commitment or period with at least one record MUST be Met when it meets the target of the
  goal settings version in effect for its date (slice 4 FR-004, restated in "What the target means" above), and
  Failed otherwise. Progress is given at any state: a commitment's recorded value against the target; a Build
  period's number of records that reach the amount against N; a Reduce yes/no period's number of "Yes" records
  against N; a Reduce count or duration period's total against the amount. A duration goal in hours compares
  minutes with the amount times 60.
- **FR-015**: Each commitment or period MUST be evaluated with the settings version in effect for its date (slice
  4 FR-005, FR-011), whenever it is recorded, corrected or read; a later version never changes an earlier
  commitment's target, time zone or instants. In this slice versions differ only in their time zone (slice 4
  FR-026); slice 8 adds changes of the other settings and verifies them.
- **FR-016**: The grace period of a commitment or period MUST start at its end instant and last exactly 48
  elapsed hours (Clarifications 2026-11-18; SPEC §11.1, second part). A clock change in between does not lengthen
  or shorten it, so on the local clock of its settings version's time zone the deadline can read one hour before
  or after midnight (23:00 or 01:00). The result is final from the instant the grace period ends (that instant
  included).

**Privacy**

- **FR-017**: Every use case of this slice MUST take the acting user and act only on that user's own goals,
  records and notes (constitution III). No use case returns or changes another account's record or note, and none
  looks a record up by identifier alone: a record is looked up only among the records of the acting user's goal
  named in the call.
- **FR-018**: Every use case of this slice MUST refuse an anonymous visitor with "You are not signed in." and an
  administrator with "Goals are for registered users.", returning no data, and MUST check in the order of FR-005
  (signed in, role, goal, record, date, value, note, limit); reading the history checks its range ("Choose a
  range of 1 to 366 days.") after the goal, as slice 4 FR-023 does for the evaluation calendar.

**Earlier use cases**

- **FR-019**: The use cases of slices 1 to 4 MUST keep giving their results for every call those slices accept,
  so that their locked acceptance criteria (F001, F002, F004) pass unchanged. The evaluation calendar (slice 4
  FR-013) keeps returning no state; the history (FR-009) adds it. Demo loading is among them: this slice does not
  change it (FR-031); its demo data comes from a separate operator capability (FR-030).

**Demonstration data**

- **FR-030**: *Demo history loading*, a new operator capability that runs after demo loading (ADR 0018), MUST
  take the demo password as demo loading does: without one (absent, empty or only spaces) its result is "no demo
  password configured", with one that fails slice 1 FR-002 "demo password does not meet the password rule", and
  in both cases it creates nothing. With a valid one it creates `demo4@example.com`, a registered user with the
  demo password (result "created"), or leaves an existing account of that email unchanged (result "already
  exists", ignoring case as slice 1 FR-003; no goal or record result, and none of its private data is read or
  changed, ADR 0013). The new account has no account name and no custom habits, and uses the installation's
  default time zone. It gets, as a whole with the account (ADR 0013), three goals that start 28 days before the
  date of the run (demo history loading writes them itself, so slice 4's "not in the past" check does not
  apply), and their records and notes relative to the run date R, the date at the run time in the installation's
  default time zone (UTC when none is configured, slice 4 FR-025), where "R − k" is the date k days before R:
  - on catalog habit "Reading": Build, duration 20 minutes, fixed days on all seven days ("Reading, 20 min
    daily"); a record of 20 minutes on each date from R − 28 to R − 3, except R − 6: 12 minutes, with the note
    "Fell asleep early.", and R − 10: no record, with the note "Away for the weekend."; R − 2: 25 minutes; R − 1
    and R: no record;
  - on catalog habit "Sugar": Reduce, count at most 1 "sweet", 1 time per day ("Sugar, at most 1 sweet a day");
    on each date from R − 28 to R − 3 one record: 0 sweets when k is even, 1 when k is odd, except R − 4: 3
    sweets, with the note "Birthday cake at work.", and R − 12: no record; R − 2, R − 1 and R: no record;
  - on catalog habit "Yoga": Build, yes/no, 3 times per week ("Yoga, 3 times a week"). The *current week* is the
    Monday-to-Sunday week that contains R (when R is a Sunday, the week ending on R); week 1 is the week before
    it, week 2 the week before week 1, week 3 the week before week 2. Week 2 has "Yes" on its Monday and
    Wednesday only, with the note "Knee was sore." on that week; week 3 has no record; week 1, and a week before
    week 3 if the goal has one (only when R is a Monday), have "Yes" on their Monday, Wednesday and Friday; the
    current week has no record. Since the goal starts 28 days before R, weeks 1 to 3 are always weeks of the
    goal, and every record is on a date before R.

  A goal is left out when its catalog habit is not Published at that point, as in slice 4 FR-040 (looked up by
  name, ignoring case). Its results for a created account are "demo goals added" and "demo records added" for
  `demo4@example.com`, also when some or all goals are left out. At the run time this shows, at least: Met and
  Failed (with the partial 12 minutes and the 3 sweets) for "Reading" and "Sugar", Missed (R − 10 and R − 12,
  and Yoga's week 3), Pending (R − 2 and R − 1; R − 2 with a record of 25 minutes, Pending by FR-012), Due (R and
  Yoga's current week), and a Failed week (Yoga's week 2); Yoga's week 1 is Pending when R is a Monday or
  Tuesday and Met otherwise. Results name the demo email but never a goal, habit, note, unit or amount.
- **FR-031**: Demo loading MUST stay exactly as slices 1, 2 and 4 specify it: its results name
  `demo1@example.com` to `demo3@example.com` and no other email (slice 1 FR-016; F001 AC-057 to AC-060), and those
  accounts keep exactly their slice-4 goals and results (F004 AC-064 to AC-069) and get no records.

### Host, page and integration requirements (outside the AC surface)

- **HP-001**: The goal page (`/goals/{id}`, `design/pages/PageGoalDetail`) MUST add, for this slice only (ADR
  0008): the primary action "Record" in the page head (the other actions are slices 7 and 8); the History card,
  a DataTable with Date, Recorded, Result (StatusChip) and Note, listing the commitments or periods that have
  begun, newest first, the first 10 with "Show all N →"; the sentence "Corrections are evaluated with the rules of
  that date."; and the Recent notes card in the right column, each note with its date and state (FR-008). No
  GoalStats, strip or trend (slice 6). Row actions, whatever the row's state:
  - a commitment row: "Add record" without a record, "Edit" with one (it opens that one record);
  - a period row: "Add record" while it has fewer than 50 records, and each of its records in the Recorded cell
    opens with "Edit"; so a final period takes a missing record as a correction (US3-AS4);
  - every row: "Add note" without a note, the note itself with one (HP-003).
- **HP-002**: The RecordDialog (`design/components/RecordDialog`, `RecordForms`) MUST follow the design. Opened
  from the page head, its "For" select lists FR-011's entries ("Pending, N h left" on Pending ones), and the
  dialog is not offered when FR-011 returns none; for a per-week entry it lists that week's dates up to the
  goal's current date. Opened from a History row ("Add record"), "For" is fixed to that row (for a period, its
  dates up to the current date). Then: the field for the measurement ("Did you do it?" Yes | No with nothing
  pre-selected; a Stepper with the unit; minutes), the target in the sub line, the progress under the field,
  the partial-progress sentence, for Reduce goals the help text asking for an explicit number or "No" because an
  unrecorded day is Missed (FR-013), and "Note (optional)", empty, whose help says that an empty field keeps the
  existing note (FR-007). "Save record" and Cancel; no "Excuse instead" (slice 7). Refusals sit under their
  field, one at a time. "Edit" opens the same dialog for that one record, with its date fixed, and "Delete
  record".
- **HP-003**: The NoteDialog (`design/components/NoteDialog`) MUST open from the History row's note and from
  "Add note": its title names the goal ("Add note · Reading, 20 min daily"), its sub line the date and the state
  ("Tue 27 Oct · Missed"), as `design/components/NoteDialog.html` shows, and its help text says that notes are
  private and do not change the result.
- **HP-004**: StatusChips MUST use the words of FR-010: "Met", "Failed", "Missed", "Pending", and for Due "Due
  today" (a commitment or per-day period) or "Due this week" (a per-week period); never "done", "skipped",
  "late" or "overdue". Upcoming is not shown on the pages of this slice (the History lists only what has begun);
  `design/components/StatusChip`'s "Due Mon" for Upcoming stays unused until a slice shows Upcoming. A Reduce
  goal's Met is "Met" too (constitution I); `design/components/StatusChip`'s "Within limit" changes first
  (Design changes before build).
- **HP-005**: The New goal form (slice 4 HP-002) MUST add the help line of its "How do you measure it?" section,
  "You can record partial amounts; the target decides the result.", that slice 4 left out, as
  `design/pages/PageNewGoal.html` shows it.
- **HP-006**: Logs, exception messages and error pages MUST NOT contain notes, recorded values, units, goal names
  or habit names (constitution III).
- **HP-007**: The host MUST run demo history loading at every start, after demo loading (ADR 0018, amending ADR
  0007), and log its results as demo loading's are logged (slice 4 HP-013): an account line for
  `demo4@example.com`, then for a created account a "Demo goals …" and a "Demo records …" line, each with the
  demo email and the result text only; no custom-habit line. A fault in it is logged without secrets and does not
  stop the start (ADR 0007). The deployment's success check (ADR 0015, slice 3 HP-017) also requires its result.
- **HP-008**: The hosted demo's smoke check (`tools/HT.SmokeCheck`) MUST also sign in as `demo4@example.com` and
  find the name of each of its demo goals on its Goals page, the words "Met", "Failed" and "Pending" on the page
  of "Reading, 20 min daily", and "Failed" and "Missed" on the page of "Yoga, 3 times a week" (its weeks 2 and 3
  are among the first 10 History rows whatever the weekday); every new data parameter of the demo data port gets
  its `SmokeCheckCoverage` entry (slice 3 "Coverage rule").
- **HP-009**: The hosted demo MUST show demo4's Met, Failed, Missed, Pending and Due results, relative to the
  current day, on every day until 2027-01-31, not only on the day of a deployment: the hosting workflow
  (`.github/workflows/deploy-demo.yml`) gains a daily `schedule` trigger shortly after midnight UTC that runs
  its refresh mode, next to the manual trigger (ADR 0018, amending ADR 0015). The schedule runs just after
  midnight in the hosted demo's default time zone, which its operator sets (ADR 0016); provisioning sets none
  today, so it is UTC and R is the UTC date. A scheduled run on a date after 2027-01-31 does nothing and
  succeeds, and the Operator guide's teardown steps remove the trigger. A late run, or one that fails before
  the database is emptied, leaves the previous day's data with one more unrecorded day at the top; one that
  fails after it leaves the hosted demo down or empty until the next successful run, as a failed manual
  refresh does (slice 3 HP-017), and the Operator can start a refresh by hand.
- **HP-010**: A commitment or period MUST become Missed at the end of its grace period with no user or operator
  action (FR-010). Nothing runs in the background for it: no job evaluates, finalizes or stores results (ADR
  0018); a test reads a commitment after its grace period with no other action in between and gets Missed.
- **HP-011**: Every page and dialog of this slice MUST meet constitution IV: tokens and `ht-` classes only,
  visible focus, a status never by color alone, dialogs managing focus, and the progress and state texts from one
  shared, unit-tested formatter ("12 of 20 min", "2 of 3 times", "1 of at most 2 cups").
- **HP-012**: `CONTEXT.md` MUST get the terms of this slice, each with a NOT-boundary (constitution I), in the
  change that revises this spec after its critic review: Record, Note, Correction, Grace period, Met, Upcoming,
  Due, Evaluation state (the values Upcoming to Missed; the Catalog habit state entry's "a goal's status" then
  names it) and Demo history loading. Pending keeps its entry, which the answers of 2026-11-18 confirm. Missed is
  sharpened: final with no record as the records are now, so a later record turns it into Met or Failed and
  deleting the only record of a final commitment makes it Missed; it applies to Reduce goals too. Demo account
  names `demo4@example.com`, created by demo history loading. Demo refresh and Operator say that the hosted
  demo's refresh is also started once a day by the hosting schedule (HP-009).

### Slice 4 requirements and tests this slice changes

- Slice 4 FR-013 ("It returns no status: Pending, Met, Failed and Missed are slice 5's"): the calendar stays as
  it is; FR-009 adds a history with states.
- Slice 4 HP-004 and `design/pages/PageGoalDetail` "Slice 4 state" (no actions, history or notes): amended by
  HP-001. Test: the goal page's UI tests.
- Slice 4 HP-002 and `design/pages/PageNewGoal` (no "You can record partial amounts"): amended by HP-005.
- Slice 1 FR-016, slice 2 FR-030 and slice 4 FR-040 (demo loading): not changed (FR-031); demo history loading
  (FR-030) is a separate capability. The unit tests that fix demo loading's three demo emails
  (`tests/HT.UnitTests/Application/Operator/DemoLoadingTests.cs`, `SeedCatalogTests.cs`) stay as they are.
- Slice 4 HP-013 and slice 1's startup tasks (ADR 0007: administrator creation, then demo loading): extended by
  HP-007 with demo history loading as the third task. Tests: the host's startup tests
  (`tests/HT.UiTests/Host/StartupLogTests.cs`, `tests/HT.UiTests/Operator/StartupTasksDemoTests.cs`) compare
  every "Demo account" and "Demo goals" line, and the "Demo " lines after the administrator line, exactly; they
  change to expect `demo4@example.com`'s lines after demo loading's, and their demo-loading lines stay as they
  are. `CONTEXT.md`'s Demo account entry names `demo1` to `demo3`; HP-012 updates it.
- Slice 3 (ADR 0015): the hosting workflow's trigger (manual only) gains the daily schedule of HP-009, and its
  success check the demo history loading result (HP-007). The Operator guide (`docs/operations/hosted-demo.md`,
  slice 3 HP-015) describes the schedule and adds removing the trigger to its "Teardown after 2027-01-31";
  `tools/hosting/selftest.ps1` tests provisioning offline and does not cover the workflow. Slice 3's texts that say a demo refresh is started by the
  Operator now also cover the schedule: its "Demo refresh" key entity, SC-006, HP-018 (sessions end at each
  refresh, so at least daily), `CONTEXT.md`'s Demo refresh and Operator entries (HP-012) and the roadmap's rows
  "When and how hosted demo data is refreshed" and "Hosting services …" (a note citing ADR 0018).
- Slice 4 HP-018 (smoke check): extended by HP-008.
- The locked acceptance criteria of slices 1, 2 and 4 (`tests/HT.AcceptanceTests/Features/F001`, `F002`, `F004`)
  are not changed and must keep passing (FR-019, FR-031).

### Design changes before build

Constitution IV and ADR 0008 require `design/` to change first:

- `design/pages/PageGoalDetail`: a slice-5 state with "Record", History, Recent notes and no other actions,
  GoalStats, trend, pauses or earlier attempts.
- `design/components/RecordDialog` and `RecordForms`: without "Excuse instead" (slice 7); the "Provisional"
  mark removed from the Reduce rule, which FR-013 confirms (the "Excuse instead" rule keeps its mark until slice
  7); the "For" select for a per-week goal and fixed to the row when opened from History; the note help "An
  empty field keeps the existing note."; "Edit" for one record, with "Delete record".
- `design/pages/PageGoalDetail`: History row actions per HP-001 ("Add record" on period rows, "Edit" per
  record, "Add note"), and the Recent notes card with each note's date and state.
- `design/components/StatusChip`: "Met" for Reduce goals too, instead of "Within limit" (constitution I); "Due
  this week" for a per-week period.
- `design/components/NoteDialog`: the "Add note" entry from the History row; `NoteDialog.md` says, as the HTML
  shows, that the title names the goal and the sub line the date and state (not "the habit, date and state").
- `design/components/RecordDialog.md`: its "For" rule "today plus every commitment still Pending" becomes
  FR-011's entries (every Due and every Pending one; no "today" on an unscheduled day).
- `design/components/CommitmentRow.html` and `design/pages/PageHomeSignedIn.html`: "Within limit" becomes the
  state word of FR-010 (constitution I), together with StatusChip.
- `design/pages/PageNewGoal.md`: its "Slice 4 state" note drops "No 'You can record partial amounts'" (the HTML
  already has the line).
- `design/pages.md` and `design/README.md`: the slice-5 structure of these pages.

### Key Entities

- **Record**: what actually happened on one date for one goal: "Yes" or "No", or an amount in the goal's unit or
  in minutes; belongs to the commitment or evaluation period of its date; private to the goal's owner.
- **Note**: an optional private text of at most 500 characters on one commitment or evaluation period; never
  changes its state.
- **Evaluation state**: Upcoming, Due, Pending, Met, Failed or Missed of one commitment or period at a supplied
  time, derived, never entered by the user (Excused and Paused come in slice 7).
- **Grace period**: the 48 hours after a commitment or period ends during which it is Pending (FR-016).
- **Correction**: a change of the records of a commitment or period whose result is already final; evaluated with
  the settings version of its date.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user records today's result for a goal in at most 3 actions from the goal page and in under 30
  seconds.
- **SC-002**: For every combination of Build or Reduce, measurement and schedule kind, the tests cover the target
  at, just below and just above its amount, and the state just before, at and just after the end of the grace
  period, including a day with a clock change, in 100% of the boundary tests (constitution II).
- **SC-003**: A result corrected after its grace period has the same state as the same records entered on time, in
  100% of the correction tests.
- **SC-004**: Every use case of this slice has negative tests for another registered user, an administrator and
  an anonymous visitor, and none of them returns or changes another account's record or note (constitution III).
- **SC-005**: On the hosted demo, on any day until 2027-01-31, `demo4@example.com` shows at least one Met, Failed,
  Missed, Pending and Due result, a partial amount and a note; the smoke check confirms it after every deployment.
- **SC-006**: No log line, exception message or error page produced by the tests contains a note, a recorded
  value, a unit, a goal name or a habit name (HP-006).
- **SC-007**: Every page and dialog of this slice passes the accessibility rules of constitution IV (HP-011).

## Decisions due before acceptance criteria

The roadmap places these decisions in this slice. Each is answered by the owner and recorded as a dated line in
`## Clarifications`, plus an ADR where it spans features.

- **Product rows of the roadmap**, decided 2026-11-18 (see `## Clarifications`):
  - **D-1** (FR-016; SPEC §11.1 second part): the 48-hour grace period around local clock changes. Decided:
    exactly 48 elapsed hours.
  - **D-2** (FR-012; SPEC §5.2, `CONTEXT.md` Pending): when a recorded result becomes final. Decided: at the end
    of the grace period, recorded or not.
  - **D-3** (FR-013; SPEC §11.4): an unrecorded Reduce commitment or period. Decided: Missed.
- **Technical rows of the roadmap**, decided with the owner on 2026-11-18, after critic pass 1 and before
  `/speckit-ac-translate` (see `## Clarifications`; [ADR 0018](../../docs/adr/0018-evaluation-on-read-and-demo-history.md)):
  - **D-4** (HP-010; ARCHITECTURE §9): scheduled evaluation and background processing. Decided: nothing runs in
    the background; every state is computed on read from stored facts and the supplied time.
  - **D-5** (FR-030, HP-009; ARCHITECTURE §7, ADR 0007, ADR 0013, ADR 0015): time-relative demo data and how the
    hosted demo keeps it current until 2027-01-31. Decided: demo history loading writes `demo4`'s data relative
    to the run date, and the hosting workflow runs its refresh mode once a day on a schedule.
- **Defaults of this spec**, confirmed by the owner in `/speckit-clarify` on 2026-11-18 (not roadmap rows, but
  they shape the acceptance criteria):
  - **D-6** (FR-030, FR-031): decided 2026-11-18: the demo results go to a new demo account
    `demo4@example.com`, because slice 4's locked AC-064 to AC-067 fix the exact goal lists and start dates of
    `demo1` to `demo3`. Revised after critic pass 1: slice 1's locked AC-057 to AC-060 forbid demo loading to name
    any other email, so `demo4` is created by the separate demo history loading (ADR 0018); demo loading, F001
    and F004 stay unchanged and locked.
  - **D-7** (FR-003, FR-006): decided 2026-11-18: one record per commitment, replaced on re-recording; any
    number (up to 50) per period; records can be changed and deleted at any time; corrections keep no history of
    earlier values.
  - **D-8** (FR-005, FR-007): decided 2026-11-18: the value ranges (count 0 to 9999, duration 0 to 1440 minutes per record), the
    500-character note and the 50-record limit per period.
  - **D-9** (FR-007): decided 2026-11-18: a note belongs to the commitment or period, not to a record, and can be
    written on any that has begun, final ones included.

## Assumptions

- Excused and Paused (slice 7), consistency, streaks, the dashboard and trends (slice 6), and goal changes,
  ending and restarting (slice 8) are out of scope; their parts of the pages are absent (ADR 0008).
- A record's date is the user's statement of when the activity happened; the app does not check it against the
  instant of saving beyond FR-002.
- The 48-hour grace period is system-wide and the same for every schedule kind (SPEC §5.2).
- Records are entered per date; per-week goals record against a date in the week, so the week a record counts in
  never depends on the instant of saving.
- Duration records are in whole minutes for goals in minutes and in hours alike; no seconds and no fractions.
- No account is deactivated yet (slice 11); no rule for a deactivated owner's records.
- Demo data stays visibly synthetic (`example.com`, constitution VI); demo notes are short invented sentences.
- A local database keeps `demo4`'s history relative to the day it was first created and drifts from then on,
  until the database is recreated (ADR 0013, ADR 0018); only the hosted demo is refreshed daily.
