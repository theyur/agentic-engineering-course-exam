# Feature Specification: Goal changes

**Feature Branch**: `claude/speckit-specify-slice-8-4f4bd7`

**Created**: 2026-11-22

**Status**: Draft

**Input**: User description: "slice 8"

Roadmap outcome (slice 8, `goal-changes`): A user changes a goal from the next evaluation boundary, ends it with an
outcome, and restarts it as a new attempt, with earlier attempts kept in history (SPEC §5.4). The roadmap adds:
this slice needs evaluations (slice 5) and streaks (slice 6) to verify its rules, which are that completed
evaluations keep their rules, ending creates no failure, and a new attempt's streak starts at zero (slice note 8). It
settles SPEC §11.7, second part (what a streak means after goal settings change). It also owns the goal page's Edit
settings, End goal and earlier attempts, the Goals page's Ended tab and the Profile's Goal history tab (roadmap
"Pages across slices").

## Clarifications

### Session 2026-11-22 (specify)

Answered by the owner during `/speckit-specify` (the two open questions of the first draft). Both reject real
alternatives, and D-1 also binds the community slice, so both are recorded in
[ADR 0025](../../docs/adr/0025-goal-change-rules.md) (constitution, Decision Discipline):

- Q: What does a streak mean after goal settings change, and can a change switch the schedule kind (SPEC §11.7,
  second part)? → A: A change keeps the schedule kind (fixed days, times per day or times per week), so the streak
  keeps its unit. The streak runs across settings versions, each result judged by the settings of its date, and
  consistency counts all of them. A different schedule kind needs ending the goal and starting a new attempt, which
  can have any settings (D-1; FR-001, FR-003 step 6, FR-006, US1-AS4, US1-AS8, US3-AS5).
- Q: Which results belong to an ended attempt? → A: The commitment or period in progress at the ending, and
  everything after it, leave the attempt: they are not counted and never become Failed or Missed, and their records
  are kept. Pending results of the attempt stay Pending, can still be recorded, and finalize as usual. So ending
  itself never creates a failure (D-2; FR-008, FR-009, US2-AS1 to AS3, AS5, AS11, SC-003).

## Decisions this slice cites

These were recorded in earlier slices and are not decided again here:

- Use cases take the acting user as an explicit parameter and return named outcomes that carry the literal text
  this spec gives. Expected refusals are outcomes, not errors ([ADR 0005](../../docs/adr/0005-use-case-convention.md)).
- The administrator role grants no access to private data (slice 1 FR-015; SPEC §3, §8). Goals are for
  registered users only (slice 4 FR-021).
- Partly built pages show only the sections and links whose slice has shipped
  ([ADR 0008](../../docs/adr/0008-partly-built-pages.md)). Community views (slice 9) stay absent.
- Time zones: a day runs from local midnight to local midnight in the time zone of the goal settings version in
  effect for it. A goal's *current date* is the date of the supplied time on its own timeline. A time zone change
  applies from the goal's next commitment or evaluation period, and a goal never has more than one settings
  version that takes effect after its current date ([ADR 0016](../../docs/adr/0016-time-zones.md); slice 4
  FR-011, FR-012, FR-026).
- Goal settings are stored as versions from the start, each with the date from which it applies, and every
  commitment or period is judged by the version of its date (slice 4 FR-005; constitution II). A goal starts as
  attempt 1 on its start date, and commitments and periods belong to the attempt in which they fall (slice 4
  FR-018).
- Evaluation states, the 48-hour grace period and corrections (slice 5 FR-006, FR-010 to FR-016).
- Evaluation and figures on read: every state and figure is computed when it is read, from stored facts and the
  supplied time. Nothing derived is cached or stored ([ADR 0018](../../docs/adr/0018-evaluation-on-read-and-demo-history.md),
  [ADR 0020](../../docs/adr/0020-derived-figures-on-read.md)).
- The report range is the goal's current attempt (slice 6 Clarifications). A Pending result never extends and
  never resets a streak (slice 6 D-1).
- Excusals and pauses, their limits and their effect on figures ([ADR 0024](../../docs/adr/0024-excusal-and-pause-rules.md);
  slice 7 FR-001 to FR-025).
- Demo data: each later slice extends a demo task, or adds a further task by a new ADR when locked criteria would
  otherwise change ([ADR 0023](../../docs/adr/0023-demo-pause-loading.md), "Later slices").
- Integration and UI tests: [ADR 0006](../../docs/adr/0006-integration-tests.md). Acceptance tests:
  [ADR 0003](../../docs/adr/0003-acceptance-tests.md).

## Earlier behaviour these tests use

Restated here, unchanged, so that this spec alone gives the rules and texts its acceptance tests depend on:

- **Password rule** (slice 1 FR-002): every password the system sets (registration, administrator, demo
  accounts) has at least 8 characters and contains at least one digit. Otherwise registration is refused with
  "At least 8 characters, with a number.", and every demo task gives "demo password does not meet the password
  rule" and creates nothing (`walking42`, `admin2026` and `example2026` meet the rule; `abc` does not).
- **Seed catalog** (slice 2, demo loading): demo loading loads the seed catalog into an empty catalog. Its
  Published catalog habits include "Reading", "Meditation" and "Sugar", the habits of demo goal-change loading's
  goals (FR-031). An administrator can unpublish a seed habit like any other.
- **Goals and schedule kinds** (slice 4 FR-001 to FR-013): a goal on fixed days of the week is
  *commitment-based*: each scheduled day on or after its start date is one commitment. A goal of N times per day
  or per week is *period-based*: each day is one evaluation period, or each Monday-to-Sunday week from the first
  one that starts on or after the start date. A goal without a name of its own is named by its habit's name, ", "
  and its target text ("Reading, 15 min daily", "Yoga, 3 times a week"). The target fields and their checks, in
  order: "Choose Build or Reduce."; "Choose how it is measured."; "A yes/no goal has no amount or unit."; "Enter
  a whole number from 1 to 999." (Build) or "Enter a whole number from 0 to 999." (Reduce); "Enter a unit, for
  example pages."; "The unit can have at most 20 characters."; "Choose minutes or hours."; "Choose fixed days or
  how many times."; "Choose at least one day."; "Enter a number of times from 1 to 20."; "Choose per day or per
  week."; then "A Reduce goal with an amount has 1 time per period.". The start date checks: "Choose a start
  date."; "The start date cannot be in the past."; "Choose a start date within a year from today.". A name longer
  than 60 characters after trimming gives "The name can have at most 60 characters.". A goal's start date
  cannot be in the past, so scenarios that need past results create the goal at an earlier supplied time.
- **States** (slice 5 FR-010, FR-012, FR-016): at the supplied time each commitment or period is in one of these
  states:
  - Upcoming: it has not begun.
  - Due: it has begun and not ended.
  - Pending: it has ended and its grace period of exactly 48 elapsed hours is still open, with or without records.
  - Met or Failed, once the grace period has ended (that instant included): decided by its records against the
    target of the settings version of its date.
  - Missed, once the grace period has ended: it has no record.

  Met, Failed and Missed are *final*. A commitment of Wednesday 11 November ends at Thursday 12 November 00:00
  and is final from Saturday 14 November 00:00.
- **Records and notes** (slice 5 FR-001 to FR-007): a record is accepted only for a date of a commitment or period
  of the goal ("Nothing is scheduled on this date.") whose day has begun. Any record can be added, changed or
  deleted at any time, and the state is always computed from the records as they are now (a *correction* when the
  result was final). A note has at most 500 characters ("The note can have at most 500 characters.").
- **Consistency and streaks** (slice 6 FR-001 to FR-004; slice 7 FR-013, FR-014): the counted results of the report
  range (final, not Excused or Paused), Met as successful, Failed and Missed as unsuccessful, given as
  "{percentage}% ({successful} of {counted})" with the percentage rounded down, or "N/A" with no counted result. Over
  the counted results in date order, Met extends a streak by 1 and Failed or Missed resets it to 0. The current
  streak counts the Met results after the latest Failed or Missed one, the longest the largest run. Units:
  "commitments", "days" or "weeks", singular for exactly 1.
- **Pauses** (slice 7 FR-006 to FR-012): a pause runs from a first day to a last day on the goal's timeline. It
  is *current*, *upcoming* or *past*.
- **Privacy texts** (slices 1, 4 and 5): an anonymous visitor gets "You are not signed in.". An administrator gets
  "Goals are for registered users.". A goal identifier that is not one of the acting user's goals gives "This
  goal does not exist.".

## What acceptance tests can reach

ADR 0003 limits acceptance tests to the Application use cases, the two `HT.Persistence` entry points and the
Domain types they use. As in slices 1 to 7:

- **FR-NNN** are application behaviours, reachable through Application use cases. Demo goal-change loading
  (FR-031) is an operator capability that acceptance tests call directly.
- **HP-NNN** are host, page and integration requirements, verified by unit, integration and selective UI tests,
  not by acceptance tests.
- Scenarios marked *(page)* describe page or browser behaviour and are expected to be listed as "Not testable
  through the API" in `acceptance.md`.
- FR numbers are grouped by topic. FR-028 and FR-029 are not used, and demo data starts at FR-030.
- The current time is supplied to every use case (constitution II).

Test data: all acceptance scenarios share one database in one run (ADR 0003). Scenarios use their own accounts
and goals, and check lists by inclusion, exclusion and order, never by totals over the whole database. The demo
scenarios start from a reset database.

Dates in the scenarios are in 2026 and in the account's time zone, which is UTC unless a scenario says otherwise.
Monday 16 November 2026 is a Monday.

Scenarios whose figures depend on a decision of this slice name it ([D-1], [D-2]).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Change a goal's settings (Priority: P1)

A registered user finds that their target no longer fits: 15 minutes of reading has become easy, or three times a
week is too much. They change the goal's settings. The change applies from the goal's next commitment or
evaluation period. Everything already begun keeps the rules it had, so their past results do not change.

**Why this priority**: SPEC §5.4's first rule. Without it the only way to adjust a target is to start a new goal
and lose the streak and history. Until this slice, a mistaken goal could not be fixed at all (roadmap R4).

**Independent Test**: Create a goal at an earlier supplied time, record on target for several days, raise the
target, and read the history and figures: the dates before the change keep their results under the old target,
and later dates are judged by the new one.

**Acceptance Scenarios** (goal "Reading, 15 min daily": catalog habit "Reading", Build, duration 15 minutes, all
seven days (commitment-based), created on Monday 2 November with that start date; 15 minutes recorded on each
date from 2 to 15 November):

1. **Given** the goal, **When** the user changes the amount to 20 minutes on Wednesday 11 November at 10:00,
   **Then** the result is "settings changed", applying from Thursday 12 November. Reading the goal at that time
   gives the settings in effect (15 minutes, from Monday 2 November) and the later ones (20 minutes, from Thursday
   12 November), and its name is still "Reading, 15 min daily".
2. **Given** the change of scenario 1, **When** the goal is read on Thursday 12 November at 00:00, **Then** its
   name is "Reading, 20 min daily" and the settings in effect are 20 minutes from Thursday 12 November.
3. **Given** the change of scenario 1, **When** the history is read on Monday 16 November at 12:00, **Then** 2 to
   11 November are Met (judged by 15 minutes, Wednesday 11 November included, as it had begun before the change),
   12 and 13 November are Failed (15 of 20 minutes), and 14 and 15 November are Pending.
4. **Given** the change of scenario 1, **When** the report is read on Monday 16 November at 12:00, **Then** the
   consistency is "83% (10 of 12)", the current streak is "0 commitments" and the longest is "10 commitments"
   [D-1].
5. **Given** the change of scenario 1, **When** the user corrects Monday 9 November to 12 minutes on Monday 16
   November, **Then** 9 November is Failed: a correction is judged by the settings of its date (15 minutes), not
   by the current ones.
6. **Given** the change of scenario 1, **When** the user changes the amount again on Wednesday 11 November at
   11:00, to 25 minutes, **Then** the result is "settings changed", applying from Thursday 12 November, and the
   goal has exactly one later version: 25 minutes from Thursday 12 November. The 20 minutes version is gone.
7. **Given** a per-week goal "Yoga, 3 times a week" (Build, yes/no, 3 times per week, created on Monday 2 November
   with that start date), **When** the user changes it to 4 times per week on Wednesday 18 November, **Then** the
   change applies from Monday 23 November, and the week 16–22 November keeps the target of 3.
8. **Given** the goal, **When** the user tries to change its schedule from all seven days to 5 times per week,
   **Then** the result is "A change keeps the schedule kind: fixed days, times per day or times per week. End the
   goal and start a new attempt to change it." and nothing changes [D-1].
9. **Given** the goal, **When** the user changes it with any field value that creation refuses, **Then** each
   target refusal of "Earlier behaviour these tests use" is given with its message, one at a time in that order,
   and nothing changes (one scenario per message).
10. **Given** the goal, **When** the user changes it with exactly its latest settings and name, **Then** the result
    is "settings unchanged" and no version is added.
11. **Given** the goal, **When** the user gives it the name "Morning pages" on Wednesday 11 November, **Then** the
    result is "settings changed" with no date. The name is "Morning pages" at once, and no version is added.
    **When** the user later clears the name, **Then** the goal is again named by its habit and target text.
12. **Given** a goal created on Monday 16 November with the start date Monday 23 November, **When** the user
    changes it to 30 minutes on Tuesday 17 November, **Then** the result is "settings changed", applying from
    Monday 23 November, and the goal has a single version, from its start date.
13. **Given** the goal, and the account's time zone changed on Wednesday 11 November at 09:00 to "Europe/Kyiv"
    (a later version from Thursday 12 November, slice 4 FR-026), **When** the user changes the amount to 20
    minutes at 10:00 that day, **Then** the goal has one later version, from Thursday 12 November, with "Europe/Kyiv"
    and 20 minutes.
14. **Given** the goal paused from Wednesday 11 until Friday 13 November, **When** the user changes its amount on
    Wednesday 11 November, **Then** the change is accepted as in scenario 1. A pause does not block a change.
15. *(page)* **Given** the goal page, **When** the user chooses "Edit settings", **Then** the Edit settings page
    opens with the latest settings filled in and the line "Changes apply from Thu 12 Nov. Past results keep their
    rules."; saving returns to the goal page, whose Settings card shows the later settings with their date.

---

### User Story 2 - End a goal (Priority: P1)

A registered user is done with a goal: they reached it, gave it up, or it no longer matters. They end it and say
how it ended, optionally with a note. Nothing new is expected of the goal. Ending is not counted as a failure, and
its history stays.

**Why this priority**: SPEC §5.4's second rule. Without it a finished goal stays on the active list and keeps
producing Missed results.

**Independent Test**: Create a goal at an earlier supplied time, end it, and read its history and figures after
its last grace period: no commitment exists after the end, and the figures count only the results of the ended
attempt.

**Acceptance Scenarios** (goal "Meditation, 10 min daily": catalog habit "Meditation", Build, duration 10 minutes,
all seven days, created on Monday 2 November with that start date; 10 minutes recorded on each date from 2 to 13
November, nothing on 14 November or later; ended on Monday 16 November at 10:00 with the outcome "Abandoned" and
the note "Too busy this month."):

1. **Given** the goal, **When** the user ends it, **Then** the result is "goal ended", with the ending date Monday
   16 November and the attempt's last date Sunday 15 November. Monday 16 November is no longer a commitment of the
   goal, and nothing after it is [D-2].
2. **Given** the goal ended, **When** what can be recorded now is read on Monday 16 November at 10:01, **Then** it
   gives the Pending 14 and 15 November and not Monday 16 November [D-2].
3. **Given** the goal ended, **When** its report is read on Wednesday 18 November at 12:00, after both grace
   periods have ended, **Then** 14 and 15 November are Missed, the consistency is "85% (12 of 14)", the current
   streak is "0 commitments" and the longest is "12 commitments" [D-2].
4. **Given** the goal ended, **When** the user records 10 minutes for 14 November on Wednesday 18 November,
   **Then** 14 November is Met (a correction) and the consistency is "92% (13 of 14)". Corrections stay possible
   after ending.
5. **Given** the goal ended, **When** the user tries to add a record for Monday 16 November, **Then** the result is
   "Nothing is scheduled on this date." [D-2].
6. **Given** the goal, **When** the user tries to end it without an outcome, **Then** the result is "Choose how the
   goal ended." and nothing changes.
7. **Given** a note of 501 characters, **When** the user ends the goal with it, **Then** the result is "The note
   can have at most 500 characters." and nothing changes. 500 characters are accepted.
8. **Given** the goal ended, **When** the user tries to end it again, **Then** the result is "This goal has already
   ended.".
9. **Given** the goal ended, **When** the user tries to change its settings or pause it, **Then** each result is
   "This goal has ended." and nothing changes.
10. **Given** the goal paused from Saturday 14 until Friday 20 November (set on Saturday 14 November) and an
    upcoming pause from Monday 23 until Wednesday 25 November, **When** the user ends it on Monday 16 November,
    **Then** the first pause runs from Saturday 14 until Sunday 15 November and the upcoming one is removed.
11. **Given** a per-week goal "Yoga, 3 times a week" (Build, yes/no, 3 times per week, created on Monday 2 November
    with that start date) with two "Yes" records on Monday 16 and Tuesday 17 November, **When** the user ends it
    on Wednesday 18 November with the outcome "No longer relevant", **Then** the attempt's last date is Sunday 15
    November. The week 16–22 November is not part of the attempt, is not counted and is not Failed. Its two
    records are kept [D-2].
12. **Given** a goal created on Monday 16 November with the start date Monday 23 November, **When** the user ends
    it on Tuesday 17 November with the outcome "No longer relevant", **Then** the result is "goal ended". Its
    attempt has no commitment and its consistency is "N/A".
13. *(page)* **Given** the goal page, **When** the user chooses "End goal", picks "Abandoned", writes a note and
    confirms "End goal", **Then** the goal page shows the Notice "Ended Mon 16 Nov · Abandoned" with "Start new
    attempt", and has no "Record", "Edit settings", "Pause" or "End goal".

---

### User Story 3 - Start a new attempt (Priority: P1)

A registered user wants to try an ended goal again. They start a new attempt, with the earlier settings or new
ones. The new attempt has its own history and a streak from zero. The earlier attempt keeps its figures and stays
available.

**Why this priority**: SPEC §5.4's third rule. It is the only way back to an ended goal, and it is how the
product keeps each try's figures apart.

**Independent Test**: End a goal, start a new attempt, record on target, and read both attempts' figures: the
new attempt's streak starts at zero and counts only its own results, and the earlier attempt's figures are
unchanged.

**Acceptance Scenarios** (the goal "Meditation, 10 min daily" of User Story 2, ended on Monday 16 November):

1. **Given** the ended goal, **When** the user asks for the starting values of a new attempt on Wednesday 18
   November, **Then** they get the settings of the ended attempt's last version (Build, duration 10 minutes, all
   seven days) and the start date Wednesday 18 November.
2. **Given** the ended goal, **When** the user starts a new attempt with those values unchanged on Wednesday 18
   November at 08:00, **Then** the result is "attempt started", with attempt number 2 and start date Wednesday 18
   November. The goal is no longer ended, its attempt line is "Attempt 2 · started 18 Nov", its consistency is
   "N/A" and its current streak is "0 commitments".
3. **Given** attempt 2 of scenario 2 with 10 minutes recorded on 18, 19 and 20 November, **When** the report is read
   on Monday 23 November at 12:00, **Then** the consistency is "100% (3 of 3)" and the current and longest streak
   are "3 commitments". Attempt 1's Missed 14 and 15 November are not counted.
4. **Given** the ended goal, **When** the user starts a new attempt on Wednesday 18 November with new settings (5
   minutes, all seven days) and the start date Monday 23 November, **Then** attempt 2 starts on Monday 23
   November with those settings. Its name is "Meditation, 5 min daily" from that date. Until then the goal is
   not ended, and its next commitment is Monday 23 November.
5. **Given** the ended goal, **When** the user starts a new attempt on Wednesday 18 November with the schedule 3
   times per week, **Then** it is accepted. A new attempt can change the schedule kind, and its streak unit is
   "weeks" [D-1].
6. **Given** the ended goal, **When** the user tries to start a new attempt with the start date Tuesday 17
   November (in the past), or Saturday 20 November 2027 (more than 365 days ahead), **Then** the result is "The
   start date cannot be in the past." or "Choose a start date within a year from today.", and nothing changes.
   Each target refusal of "Earlier behaviour these tests use" is given as for a change (one scenario per message).
7. **Given** a goal that is not ended, **When** the user tries to start a new attempt, or to read the starting
   values of one, **Then** the result is "Only an ended goal can start a new attempt.".
8. **Given** attempt 2 started on Monday 23 November as in scenario 4, **When** the user tries on Tuesday 24
   November to add a record for Friday 20 November (between the attempts), **Then** the result is "Nothing is
   scheduled on this date.".
9. **Given** attempt 2 of scenario 2, **When** the user reads the goal's attempts on Monday 23 November, **Then**
   they get attempt 2 (started 18 November, not ended) and attempt 1 (started 2 November, ended 16 November,
   last date 15 November, "Abandoned", "Too busy this month.", consistency "85% (12 of 14)", longest streak "12
   commitments"), latest first.
10. **Given** attempt 2 of scenario 2, **When** the user records 10 minutes for 15 November (attempt 1) on Monday 23
    November, **Then** attempt 1's consistency becomes "92% (13 of 14)" and attempt 2's figures do not change.
11. *(page)* **Given** an ended goal on the Goals page's Ended tab or on its goal page, **When** the user chooses
    "Start new attempt", **Then** the Start new attempt page opens with the earlier settings filled in and the
    start date today. Its line reads "Attempt 2 starts with its own history and a streak from zero.".
    Confirming "Start attempt" opens the goal page.

---

### User Story 4 - Ended goals and earlier attempts in lists (Priority: P2)

Ended goals leave the active-goal view but stay reachable: the Goals page lists them on its Ended tab with their
outcome, the Profile lists every ended attempt on its Goal history tab, and the goal page lists earlier attempts
with their outcome and figures.

**Why this priority**: SPEC §5.4 requires ended attempts to remain accessible in history. The rules of User
Stories 1 to 3 hold without these views.

**Independent Test**: End one of two goals and restart a third, then read the goals overview, the dashboard and
the goal history: the ended goal is flagged with its outcome and left out of the dashboard, and both ended
attempts are in the goal history.

**Acceptance Scenarios** (one account with three goals, all created on Monday 2 November with that start date and
recorded on target on each date from 2 to 13 November: "Reading, 15 min daily" (Reading, Build, duration 15
minutes, all seven days); "Meditation, 10 min daily" (as in User Story 2), ended on Monday 16 November at 10:00 with
"Abandoned"; "Sugar, at most 1 sweet a day" (Sugar, Reduce, count at most 1 "sweet", 1 time per day), ended on
Saturday 14 November at 09:00 with "Achieved" and the note "Three weeks without extra sweets.", and restarted on
Monday 16 November at 09:00 with unchanged settings from that date):

1. **Given** the three goals, **When** the goals overview is read on Monday 16 November at 12:00, **Then**
   "Meditation, 10 min daily" is given as ended on Monday 16 November with "Abandoned", and the other two as not
   ended. The order rules of slice 6 FR-011 are unchanged.
2. **Given** the three goals, **When** the dashboard is read on Monday 16 November at 12:00, **Then** it has no
   entry of "Meditation" (neither its Pending 14 and 15 November nor anything later) and no summary of it, and
   the count of goals is 2.
3. **Given** the three goals, **When** the goal history is read on Monday 16 November at 12:00, **Then** it gives
   "Meditation, 10 min daily", attempt 1, ended Monday 16 November, "Abandoned", no note; then "Sugar, at most 1
   sweet a day", attempt 1, ended Saturday 14 November, "Achieved", "Three weeks without extra sweets.". The
   latest ending comes first.
4. **Given** the three goals, **When** "Sugar" is read, **Then** its attempt is number 2, started Monday 16
   November. Its report counts only attempt 2: the consistency is "N/A".
5. **Given** the three goals, **When** the catalog is browsed with "Only habits I track", **Then** "Reading" and
   "Sugar" are included and "Meditation" is not, and the goal count of "Meditation" is 0. An ended goal is not
   tracked.
6. *(page)* **Given** the Goals page, **Then** it shows the Tabs "Active", "Paused" and "Ended". The Ended tab lists
   "Meditation, 10 min daily" with the outcome Tag "Abandoned", the meta "Ended Mon 16 Nov", its figures and
   strip, and the action "Start new attempt". An empty Ended tab says "No ended goals.".
7. *(page)* **Given** the Profile, **Then** its Goal history tab shows a table with the columns Goal, Attempt,
   Ended, Outcome and Note, in the order of scenario 3. With no ended attempt it says "No ended goals yet.".
8. *(page)* **Given** the goal page of "Sugar", **Then** its right column has the "Earlier attempts" card with
   "Attempt 1 · 2 Nov – 13 Nov", the outcome Tag "Achieved", the consistency and the note.

---

### User Story 5 - Privacy of goal changes (Priority: P1)

Settings changes, endings, outcomes, ending notes and attempts are private to the goal's owner, like the goal
itself.

**Why this priority**: constitution III and SPEC §3: private tracking data is owner-only, administrators
included.

**Independent Test**: Try every use case of this slice as another registered user, an administrator and an
anonymous visitor on someone else's goal. Each is refused and nothing changes.

**Acceptance Scenarios**:

1. **Given** user A's goal, **When** user B tries to change its settings, end it, read the starting values of a
   new attempt, start one, or read its attempts, **Then** each result is "This goal does not exist." and nothing
   changes.
2. **Given** user A's goal, **When** an administrator tries any of them or reads the goal history, **Then** each
   result is "Goals are for registered users.".
3. **Given** user A's goal, **When** an anonymous visitor tries any of them or reads the goal history, **Then** each
   result is "You are not signed in.".
4. **Given** user A's ended attempt, **When** user B reads their own goal history, **Then** it contains none of
   user A's attempts.
5. **Given** an ending note, **Then** it appears in no log line, exception message or error page (HP-010).

---

### User Story 6 - Demo goal changes (Priority: P2)

A reviewer signs in to the hosted demo and sees a goal whose target changed, an ended goal on the Ended tab, a
restarted goal with an earlier attempt, and the Profile's goal history, without waiting for time to pass.

**Why this priority**: constitution VI and SPEC §10. The slice cannot be demonstrated without it.

**Independent Test**: Run the demo task on a reset database at a fixed run time and read the demo account's
reports.

**Acceptance Scenarios** (D-3):

1. **Given** a reset database with demo loading, demo history loading, demo report loading and demo pause loading
   run, **When** demo goal-change loading runs with a valid demo password, **Then** its results are "created",
   "demo goals added" and "demo records added" for `demo7@example.com`. They name no goal, habit, note, outcome,
   unit or amount.
2. **Given** demo goal-change loading has run on run date R, **When** `demo7@example.com` reads its goals, attempts,
   reports and goal history at the run time, **Then** they are as FR-031 gives them, on every weekday.
3. **Given** `demo7@example.com` exists, **When** demo goal-change loading runs again, **Then** its result is
   "already exists" and nothing of the account is read or changed.
4. **Given** no demo password, or one that fails the password rule, **When** demo goal-change loading runs,
   **Then** its result is "no demo password configured" or "demo password does not meet the password rule", and
   it creates nothing.
5. **Given** the earlier demo tasks, **When** they run, **Then** their results and data are unchanged by this slice
   (FR-030).

---

### Edge Cases

- **A change on the day it is made**: the current date's commitment or period has begun, so it keeps the settings
  in effect. The change applies from the goal's next commitment or period: the next date for a commitment-based
  or per-day goal, the next Monday for a per-week goal (FR-002). The same rule as a time zone change.
- **A change while a later version is waiting**: a goal has at most one version after its current date. A second
  change before that version takes effect replaces it, and a time zone change updates it (slice 4 FR-026). The
  two kinds of change merge into that one version (US1-AS6, US1-AS13).
- **A change to a goal or attempt that has not begun**: no result can be affected, so the change replaces the
  attempt's first version, from its start date (US1-AS12).
- **Build to Reduce, or a change of measurement**: allowed within one schedule kind [D-1]. Past results keep their
  old target and measurement. Records entered under the old measurement stay with their dates.
- **Ending on the start date, or before it**: the attempt has no commitment or period and its figures are "N/A"
  (US2-AS12).
- **Ending a paused goal**: allowed. A current pause is cut to the attempt's last date (removed when it begins
  later), and upcoming pauses are removed (FR-009, US2-AS10).
- **Ending a goal whose current commitment is excused or recorded**: the current commitment or period leaves the
  attempt [D-2]. Its excusal is removed and its records are kept with their date (FR-009). Records are never
  deleted by ending.
- **Pending results at the ending**: they belong to the ended attempt, can still be recorded or excused during
  their grace period, and become final as usual [D-2]. The dashboard no longer lists them, but the goal page does
  (FR-019).
- **Ending and restarting on the same day**: allowed. The new attempt may start on the ending date, which is no
  longer part of the ended attempt [D-2]. Records kept on that date then belong to the new attempt.
- **Restarting a per-week goal**: the new attempt's first week is the first Monday-to-Sunday week that starts on or
  after its start date (slice 4 FR-013).
- **Several attempts**: any number. Each has its own number, figures and streak, and its own settings versions.
  Attempt numbers are never reused.
- **A time zone change on an ended goal**: adds no version, as no date of the goal is after its last date. A new
  attempt uses the account's time zone at the time it starts.
- **Ending a goal with a later version waiting**: the later version is removed, as its first date is after the
  attempt's last date.
- **A habit unpublished, or a custom habit used only by ended goals**: still "used by goals" (slice 4 FR-033,
  FR-035). Ended goals keep their habit.
- **Undoing an ending**: not possible. The user starts a new attempt.

## Requirements *(mandatory)*

### Functional Requirements (application behaviour, on the AC surface)

**Terms used below**

- *Current attempt*: the goal's latest attempt while it is not ended. A goal has at most one attempt that is not
  ended.
- *Ending date*: the goal's current date when the user ends it (for a goal that has not started, the account's
  today). It is stored with the attempt.
- *Last date* of an ended attempt: the last date that belongs to it [D-2]. It is derived from the ending date as
  FR-008 gives it.
- *Latest attempt*: the current attempt, or for an ended goal its last ended attempt.
- *Next boundary* at the goal's current date D: D + 1 day for a commitment-based or per-day goal, and the Monday
  after D's week for a per-week goal (as in slice 4 FR-026).

**Changing settings**

- **FR-001**: A registered user MUST be able to change the settings of one of their own goals that is not ended:
  Build or Reduce, the measurement, the amount and unit, and the schedule, with the fields and values of goal
  creation (slice 4 FR-002). They MUST also be able to change the goal's own name. The habit, the start date and
  the attempt cannot be changed. The settings MUST stay within the goal's schedule kind (fixed days, times per
  day or times per week), so that the streak keeps its unit (D-1; SPEC §11.7, second part).
- **FR-002**: A settings change MUST take effect from the next boundary at the goal's current date, and add a
  settings version from that date with the new settings and the account's current time zone. When the goal
  already has a version that takes effect after its current date (at most one, slice 4 FR-026), that version
  gets the new settings and the date of the next boundary instead, so that a goal never has more than one later
  version. For a goal whose current attempt has not begun (no date of it has begun), the change replaces that
  attempt's first version, keeping its date. A change never alters the version of a date whose day has begun.
- **FR-003**: Changing settings MUST give one result, checking in this order:
  1. "You are not signed in.";
  2. "Goals are for registered users.";
  3. "This goal does not exist.";
  4. an ended goal "This goal has ended.";
  5. the target fields with the checks and messages of slice 4 FR-006, in that order, then "A Reduce goal with an
     amount has 1 time per period.";
  6. a schedule of another kind than the goal's "A change keeps the schedule kind: fixed days, times per day or
     times per week. End the goal and start a new attempt to change it." [D-1];
  7. a name longer than 60 characters after trimming "The name can have at most 60 characters.".

  Otherwise:
  - settings and name equal to the goal's latest version and name give "settings unchanged", and nothing changes;
  - a change of the name only gives "settings changed" with no date, and the name changes at once;
  - any other change gives "settings changed" with the date from which the settings apply (FR-002).

  Values that do not belong to the chosen measurement or schedule are neither refused nor stored (slice 4
  FR-006).
- **FR-004**: The name MUST be trimmed, and an empty name means no name (slice 4 FR-009). The goal's own name
  applies at once to every attempt. A goal without a name of its own is named by its habit's current name and the
  target text of its settings version in effect on its current date (for an ended goal, its latest attempt's
  last version; for a goal whose attempt has not begun, that attempt's first version).
- **FR-005**: Reading a goal (slice 4 FR-016) MUST return the settings version in effect on its current date,
  with the date from which it applies, and the version that takes effect after its current date, if any, with all
  its settings and its date. These are no longer only the time zone. Completed and begun commitments and periods
  MUST keep being judged by the version of their date (slice 5 FR-010), corrections included.
- **FR-006**: Streaks across settings versions: the current and longest streak of an attempt MUST run over its
  counted results in date order across all its settings versions, each result judged by the version of its date,
  in the unit of the attempt's schedule kind [D-1]. Consistency counts all counted results of the attempt, also
  across versions.

**Ending a goal**

- **FR-007**: A registered user MUST be able to end one of their own goals that is not ended, with an outcome,
  "Achieved", "Abandoned" or "No longer relevant", and an optional note: trimmed, empty means none, at most 500
  characters. Ending MUST give one result, checking in this order:
  1. to 3. as FR-003;
  4. an ended goal "This goal has already ended.";
  5. no outcome "Choose how the goal ended.";
  6. a note longer than 500 characters "The note can have at most 500 characters.".

  Otherwise the result is "goal ended", with the ending date and the attempt's last date. A goal that has not
  started, or whose current attempt has not begun, can be ended. Its attempt then has no commitment or period.
- **FR-008**: The ended attempt's last date MUST be derived from the ending date E (D-2): for a
  commitment-based or per-day goal E − 1 day, and for a per-week goal the Sunday before E's week. A last date
  before the attempt's start date means the attempt has no commitment or period. So the commitment or period in progress at the ending, and everything after it, is
  not part of the attempt: it is neither expected nor counted and never becomes Failed or Missed. Commitments and
  periods up to the last date keep their states. A Pending one stays Pending until its grace period ends, can
  still be recorded, corrected or excused, and then becomes Met, Failed or Missed as usual (SPEC §5.4: ending
  stops future commitments without creating a failure).
- **FR-009**: Ending MUST also cut every pause of the goal to the attempt's last date. A pause that begins after
  it is removed, and a pause that covers it ends on it. It also removes every excusal of a date after it, and
  every settings version that takes effect after it. Records and notes of dates after the last date are kept
  as stored facts: they are not shown or counted while no attempt contains their date (FR-016).
- **FR-010**: An ended goal MUST have no commitment or period after its latest attempt's last date. Adding a record
  for such a date gives "Nothing is scheduled on this date." (slice 5 FR-005, step (4)). Changing settings and
  pausing it give "This goal has ended.". Excusing a Pending commitment of the ended attempt, removing an excusal,
  and adding, changing or deleting records of the ended attempt's dates MUST keep working as for any goal (SPEC
  §5.2: the grace period is not an edit lock).

**Starting a new attempt**

- **FR-011**: Reading the starting values of a new attempt MUST return the settings of the ended attempt's last
  version and the start date today, the account's today (SPEC §5.4: optionally copying prior settings). Checks:
  1. to 3. as FR-003;
  4. a goal that is not ended "Only an ended goal can start a new attempt.".
- **FR-012**: A registered user MUST be able to start a new attempt of one of their own ended goals, with settings
  of any schedule kind (the fields and checks of goal creation) and a start date. Checks:
  1. to 4. as FR-011;
  5. the target fields as FR-003 step 5;
  6. the start date checks of slice 4 FR-006 ("Choose a start date.", "The start date cannot be in the past.",
     "Choose a start date within a year from today.").

  Otherwise the result is "attempt started", with the attempt's number (the previous number + 1) and its start
  date. The new attempt starts on its start date with its first settings version from that date, in the
  account's current time zone. It is not ended, and the goal is no longer ended. The goal keeps its identifier,
  habit and name.
- **FR-013**: Each attempt MUST have its own commitments or periods, from its start date (for a per-week schedule,
  the first week that starts on or after it) to its last date, or with no end while it is not ended. No date
  belongs to two attempts, and dates between attempts belong to none. The report range (slice 6) is the latest
  attempt, up to the supplied time. So a new attempt's consistency and streaks count only its own results and
  start from "N/A" and 0 (SPEC §5.4).
- **FR-014**: Reading a goal's attempts MUST return every attempt, the latest first, each with:
  - its number and start date;
  - when it is ended, its ending date, last date, outcome and note;
  - its consistency, current streak and longest streak with their unit, computed over that attempt up to the
    supplied time as slice 6 FR-001 to FR-004 and slice 7 FR-013, FR-014 give them.

  A correction of a date of an earlier attempt changes that attempt's figures only.

**Lists and earlier reads**

- **FR-015**: Reading the goal history MUST return every ended attempt of the acting user's goals, each with the
  goal's identifier and name, the attempt's number, ending date, outcome and note, the latest ending date first,
  then by goal name (slice 2 FR-004 comparison) and attempt number.
- **FR-016**: Reading a goal's history, recent results, trend, recent notes and what can be recorded now (slices 5
  to 7) MUST cover only dates of its latest attempt. The history range of slice 5 FR-009 is limited to them. The
  goal's current date (slice 4 FR-012) and the evaluation calendar (slice 4 FR-013) MUST give no commitment or
  period outside an attempt.
- **FR-017**: Reading a goal (slice 4 FR-016) MUST return its latest attempt's number and start date, and, when
  the goal is ended, its ending date, last date, outcome and note.
- **FR-018**: The goals overview (slice 6 FR-011) MUST give, for each goal, whether it is ended and, if so, its
  ending date and outcome. An ended goal's figures, results and strip are those of its latest attempt. It is never
  paused (slice 7 FR-018) and has no Due entry. The order rules are unchanged.
- **FR-019**: The dashboard (slice 6 FR-012) MUST leave ended goals out entirely: none of their entries are counted
  or listed (Pending and Missed ones included), they have no summary, and the count of goals counts only goals
  that are neither paused nor ended. Its recent notes leave out notes of ended goals.
- **FR-020**: The catalog's goal counts and "Only habits I track" (slice 4 FR-031, FR-032) MUST count only goals
  that are not ended.
- **FR-021**: A time zone change (slice 4 FR-026) MUST add no version to an ended goal. For a goal whose current
  attempt has not begun, it updates that attempt's first version, as it does for a goal that has not started.

**Privacy**

- **FR-022**: Every use case of this slice MUST take the acting user and act only on that user's own goals and
  attempts (constitution III). An attempt is looked up only among the attempts of the acting user's goal named in
  the call ("This attempt does not exist." otherwise).
- **FR-023**: Every use case of this slice MUST refuse an anonymous visitor with "You are not signed in." and an
  administrator with "Goals are for registered users.", returning and changing nothing, checked in that order
  before the goal.

**Earlier use cases**

- **FR-024**: The use cases of slices 1 to 7 MUST keep giving their results for every call those slices accept on
  goals with one attempt, no ending and no settings change, so that their locked acceptance criteria (F001, F002,
  F004, F005, F007, F008) pass unchanged.
- **FR-025**: Changing settings, ending, starting a new attempt and correcting a record of any attempt MUST each
  change the figures and reads of FR-013 to FR-020 on the next read, exactly as if the change had been made at the
  time it concerns (constitution II).
- **FR-026**: A settings change, an ending (ending date, outcome, note) and an attempt MUST be stored as facts.
  Last dates, states and figures are derived on read, and no time of making is stored (ADR 0018, ADR 0020).
- **FR-027**: Ending notes are not notes of commitments or periods (slice 5 FR-007). They MUST NOT appear in recent
  notes or the History.

**Demonstration data** (D-3)

- **FR-030**: Demo loading, demo history loading, demo report loading and demo pause loading MUST stay exactly as
  slices 1 to 7 specify them, with no settings change, ending or second attempt, so that F001, F004, F005, F007
  and F008 stay locked and unchanged.
- **FR-031**: *Demo goal-change loading* is a new operator capability that runs after demo pause loading. It
  MUST take the demo password as demo loading does ("no demo password configured", "demo password does not meet
  the password rule", creating nothing in both cases). With a valid password it creates `demo7@example.com`, a
  registered user with the demo password (result "created"). An existing account of that email is left unchanged
  (result "already exists", ignoring case; none of its private data is read or changed, ADR 0013). The new account
  has no account name and no custom habits, and uses the installation's default time zone. It gets, as a whole
  with the account, three goals and their settings versions, attempts, endings, records and notes, stored directly
  as the facts of FR-026. The limits on when a user can change, end or restart a goal apply to users, not to this
  task. "R − k" is the date k days before the run date R. Each goal is left out when its catalog habit is not
  Published (looked up by name, ignoring case), and its name follows FR-004:
  - **On catalog habit "Reading"**: Build, duration, all seven days. Version 1 from R − 28 with 15 minutes, and
    version 2 from R − 13 with 20 minutes. One attempt, not ended. Records: 15 minutes on each date from R − 28
    to R − 14; 20 minutes on each date from R − 13 to R − 2, except R − 9 with 15 minutes and the note "Short on
    time."; no record on R − 1 or R.
  - **On catalog habit "Meditation"**: two attempts.
    - Attempt 1 from R − 42: Build, duration 10 minutes, all seven days, ended with the ending date R − 21, the
      outcome "Abandoned" and the note "Too busy.". Records: 10 minutes on each date from R − 42 to R − 32, none
      from R − 31 on.
    - Attempt 2 from R − 14: Build, duration 5 minutes, all seven days. Records: 5 minutes on each date from
      R − 14 to R − 2, none on R − 1 or R.
  - **On catalog habit "Sugar"**: Reduce, count at most 1 "sweet", 1 time per day. One attempt from R − 30, ended
    with the ending date R − 7, the outcome "Achieved" and the note "Three weeks without extra sweets.". Records:
    0 sweets on each date from R − 30 to R − 8.

  Its results for a created account are "created", "demo goals added" and "demo records added" for
  `demo7@example.com`, also when some or all goals are left out. They name the demo email but never a goal,
  habit, note, outcome, unit or amount. Read by `demo7@example.com` at the run time, on every weekday (by the rules
  of D-1 and D-2):
  - **"Reading, 20 min daily"**:
    - counted results R − 28 to R − 3, of which R − 9 is Failed (15 of 20 minutes, while R − 14 with 15 minutes
      is Met under version 1);
    - "96% (25 of 26)", current streak "6 commitments", longest "19 commitments" (R − 28 to R − 10, across the
      change);
    - R − 2 and R − 1 Pending, R Due;
    - the later settings: none.
  - **"Meditation, 5 min daily"**:
    - attempt line "Attempt 2", started R − 14;
    - "100% (12 of 12)", current and longest streak "12 commitments";
    - R − 2 and R − 1 Pending, R Due;
    - attempt 1: ended R − 21, last date R − 22, "Abandoned", "Too busy.", "52% (11 of 21)", longest streak "11
      commitments".
  - **"Sugar, at most 1 sweet a day"**:
    - ended R − 7 with "Achieved";
    - "100% (23 of 23)", current and longest streak "23 days".
  - **The dashboard**: 2 Due, 4 Pending, 0 Missed this week, 2 goals counted (the ended one is not).
  - **The goal history**: "Sugar, at most 1 sweet a day", attempt 1, then "Meditation, 5 min daily", attempt 1.

  These figures hold in the default time zone UTC, which has no clock change (the hosted demo uses UTC).

### Host, page and integration requirements (outside the AC surface)

- **HP-001**: The goal page (`/goals/{id}`, `design/pages/PageGoalDetail`) MUST add the following (ADR 0008):
  - in the page head of a goal that is not ended, the ghost action "Edit settings" (first) and the ghost action
    "End goal" (`aria-haspopup="dialog"`, after "Pause");
  - the attempt line for the latest attempt ("Attempt 2 · started 18 Nov", or "· starts 23 Nov");
  - in the Settings card, the settings in effect, and when a later version exists, the row "From {Thu 12 Nov}"
    with that version's target text;
  - for an ended goal, under the head, the Notice "Ended {Mon 16 Nov} · {Abandoned}" with the action "Start new
    attempt", and no "Record", "Edit settings", "Pause" or "End goal". The History card keeps its record and
    note actions for the ended attempt's dates (FR-010);
  - in the right column, after Recent notes, the "Earlier attempts" card: each ended attempt that is not the
    latest, as "Attempt {n} · {2 Nov} – {15 Nov}" (start date to last date), its outcome Tag, its consistency, and
    its note (`ht-muted`) when it has one. The card is absent when there is none.
- **HP-002**: The Edit settings page (`/goals/{id}/edit`, new design page, see "Design changes before build") MUST
  reuse the New goal form's fields for Build or Reduce, measurement, target and schedule, and the name. The
  schedule choice is limited to the goal's kind [D-1]. It has no habit chooser and no start date. Above the
  actions it shows "Changes apply from {Thu 12 Nov}. Past results keep their rules." (from FR-002's date), and the
  summary card of the New goal page. It has the primary "Save changes" and the secondary "Cancel". Refusals are
  shown under their field, one at a time, with the result's message as it is. Another account's goal, an unknown
  goal or an ended goal shows the Not found page or, for an ended goal, returns to its goal page.
- **HP-003**: The EndGoalDialog (`design/components/EndGoalDialog`) MUST follow the design:
  - the title "End goal · {goal name}" and the sub line with the attempt line;
  - the effect first: "Ending stops new commitments. Your history stays and nothing is counted as failed. To try
    again later you start a new attempt.";
  - "How did it end?" as ChoiceCards Achieved, Abandoned and No longer relevant, in one column, and "Note
    (optional)";
  - the primary "End goal" and "Cancel".

  Refusals are shown under the field they are about.
- **HP-004**: The Start new attempt page (`/goals/{id}/new-attempt`, new design page) MUST reuse the New goal form
  (all schedule kinds, start date) without the habit chooser and the name. It opens with FR-011's starting
  values and has the line "Attempt {n} starts with its own history and a streak from zero.". It has the primary
  "Start attempt" and the secondary "Cancel", and confirming opens the goal page.
- **HP-005**: The Goals page (`/goals`, `design/pages/PageGoals`) MUST show the Tabs "Active", "Paused" and "Ended"
  (`?tab=ended`). Active and Paused leave ended goals out. Ended lists the ended goals in the overview order, each
  with the outcome Tag, the meta "Ended {Mon 16 Nov}", its figures and strip, and the secondary "Start new
  attempt". An empty Ended tab says "No ended goals.".
- **HP-006**: The Profile (`design/pages/PageProfile`) MUST add the tab "Goal history" after "Account" (Community
  comes in slice 9): a DataTable with the columns Goal (a link to the goal page), Attempt, Ended ("Mon 16 Nov"),
  Outcome (Tag) and Note (`ht-muted`, or "—"), in FR-015's order. An empty table is replaced by "No ended goals
  yet.". The selected tab is kept in the address (`?tab=history`).
- **HP-007**: The signed-in Home (`design/pages/PageHomeSignedIn`) MUST leave ended goals out of its tiles, Today
  list, Active goals card and recent notes (FR-019). Nothing else changes.
- **HP-008**: The outcome texts, "Ended …", the attempt line and the Settings card's "From …" MUST come from the
  shared formatter and text source of slice 6 HP-005, so pages build none. Outcome Tags are never shown by colour
  alone.
- **HP-009**: Every dialog and form of this slice MUST manage focus (first field on open; Esc and × close
  dialogs; focus returns to the opener, or to the nearest remaining control when the opener is gone) and meet
  constitution IV.
- **HP-010**: Logs, exception messages and error pages MUST NOT contain ending notes, notes, recorded values, goal
  names, habit names or units (constitution III).
- **HP-011**: The host MUST run demo goal-change loading at every start, after demo pause loading, and log its
  results as demo pause loading's are logged (slice 7 HP-010), with "Demo goal-change loading: {result}" when the
  password is missing or weak. A fault in it is logged without secrets and does not stop the start. The
  deployment's success check (ADR 0015) also requires its result, and the daily scheduled refresh re-creates
  `demo7` with the rest of the demo data.
- **HP-012**: The hosted demo's smoke check (`tools/HT.SmokeCheck`) MUST also check, signed in as
  `demo7@example.com`:
  - "Sugar, at most 1 sweet a day" on the Goals page's Ended tab;
  - "Attempt 2" on the page of "Meditation, 5 min daily";
  - "Goal history" with "Abandoned" on the Profile.

  Every data parameter of demo goal-change loading's port gets its `SmokeCheckCoverage` entry (slice 3 "Coverage
  rule").
- **HP-013**: `CONTEXT.md` MUST get the following in the same change as this spec's decisions (constitution I):
  - a new entry for Outcome (Achieved, Abandoned, No longer relevant; NOT an evaluation state, NOT a failure);
  - a new entry for Ended goal (NOT paused, NOT deleted);
  - a new entry for Settings change (from the next boundary; NOT a correction, NOT retroactive);
  - Attempt sharpened with the last date and the ending date [D-2];
  - Goal settings version with "a change within one schedule kind" [D-1];
  - Streak with SPEC §11.7's answer [D-1];
  - Report range as "the latest attempt";
  - Dashboard leaving ended goals out;
  - Demo account listing `demo7@example.com`, a new entry for Demo goal-change loading, and Demo refresh listing
    it among the tasks that run again.

  All entries except the three demo ones were added on 2026-11-22 together with the answers to D-1 and D-2. The
  demo entries follow D-3.

### Earlier requirements and tests this slice changes

- Slice 4 FR-016 (reading a goal: later version's time zone only), FR-017 (no edit, end, restart), FR-018 (one
  attempt), FR-015 ("all goals are active"), FR-026 (only time zone versions) and FR-031/FR-032 (catalog counts):
  extended or replaced by FR-001 to FR-021. F004 uses none of this slice's changes and passes unchanged (FR-024).
- Slice 4 HP (goal page head without actions, Profile without Goal history): amended by HP-001 and HP-006.
- Slice 5 FR-005 step (4), FR-009 (history range), FR-011 (what can be recorded now): limited to attempt dates by
  FR-010 and FR-016. F005 uses one attempt and passes unchanged.
- Slice 6 FR-011, FR-012 and the report range: extended by FR-013, FR-018 and FR-019. F007's "number of the user's
  goals" counts goals that are neither paused nor ended, the same for every F007 scenario.
- Slice 7 FR-006 (pausing) gains the step "This goal has ended." after step (3). FR-018 and FR-019 are extended by
  FR-018 and FR-019 here. HP-004 (Goals page without "Ended") is amended by HP-005. F008 uses no ended goal and
  passes unchanged.
- Tests that assert absences change: the goal page's, Goals page's and Profile's UI tests that assert no Edit
  settings, End goal, earlier attempts, Ended tab or Goal history tab, and `GoalListItem` tests that assert no
  "Start new attempt" or outcome Tag.
- Slice 7 HP-010 and HP-011 (startup tasks, smoke check): extended by HP-011 and HP-012. The host's startup tests
  and smoke-check tests that compare "Demo " lines, check names and coverage counts exactly change to expect
  `demo7@example.com` after `demo6@example.com`.

### Design changes before build

Constitution IV and ADR 0008 require `design/` to change first:

- `design/pages/PageGoalDetail`: a slice-8 state with "Edit settings" and "End goal", the Settings card's "From …"
  row, the ended Notice with "Start new attempt", and the Earlier attempts card.
- A new design page for Edit settings (from `PageNewGoal`, without the habit chooser and start date, with the
  "Changes apply from …" line), and a new design page, or a state of `PageNewGoal`, for Start new attempt.
- `design/components/EndGoalDialog`: the texts of HP-003 confirmed and the refusal placement.
- `design/pages/PageGoals` and `components/GoalListItem`: the Ended tab, the ended item with its outcome Tag, "Ended
  …" and "Start new attempt", and "No ended goals.".
- `design/pages/PageProfile`: the Goal history tab in its slice-8 state, without the Community tab, with its empty
  text.
- `design/pages.md` and `design/README.md`: the slice-8 structure of these pages.

### Key Entities

- **Settings change**: a new goal settings version from the next boundary, or a replacement of the one later
  version. Within one schedule kind [D-1].
- **Attempt**: one continuous pursuit of a goal: its number, start date, and, once ended, its ending date, outcome
  and optional note; its last date is derived [D-2]. Its own settings versions, commitments or periods, and figures.
- **Outcome**: how an attempt ended: Achieved, Abandoned or No longer relevant. It is never counted as a result.
- **Goal history**: the list of a user's ended attempts across goals.

All of them are private to the goal's owner and entered by the user. None of them is a derived figure.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user changes a goal's target from its goal page in three actions ("Edit settings", the field,
  "Save changes"), ends a goal in three ("End goal", the outcome, "End goal"), and starts a new attempt from the
  Ended tab in two ("Start new attempt", "Start attempt"). Each change shows on the next read of every report.
- **SC-002**: In 100% of the change tests, no commitment or period whose day had begun when the change was made
  changes its result or target (SPEC §5.4).
- **SC-003**: In 100% of the ending tests, no result of the commitment or period in progress at the ending, or
  later, is counted, and ending adds no Failed or Missed result (SPEC §5.4) [D-2].
- **SC-004**: In 100% of the restart tests, the new attempt's figures start at "N/A" and "0", and the earlier
  attempt's figures are unchanged by anything recorded in the new one.
- **SC-005**: The tests cover a change made just before and just after midnight, before and after a per-week
  boundary, and before a waiting version takes effect, and an ending on the start date, before it and later
  (constitution II).
- **SC-006**: Every use case of this slice has negative tests for another registered user, an administrator and an
  anonymous visitor, and none reads or changes another account's goal, attempt, outcome or ending note
  (constitution III).
- **SC-007**: On the hosted demo, on any day until 2027-01-31, `demo7@example.com` shows a goal whose target
  changed, an ended goal on the Ended tab, a goal on attempt 2 with its earlier attempt, and a filled Goal history,
  without waiting for time to pass. The acceptance tests of FR-031 prove the figures at a fixed run time, and the
  smoke check confirms only their presence (HP-012).
- **SC-008**: Every page and dialog of this slice passes the accessibility rules of constitution IV (HP-008,
  HP-009).

## Decisions due before acceptance criteria

The roadmap places one product decision in this slice (D-1, SPEC §11.7 second part). This spec adds D-2 and D-3.
The owner answered D-1 and D-2 on 2026-11-22 (see `## Clarifications`). They reject real alternatives and D-1 binds
the community slice (slice 9), so both are recorded in [ADR 0025](../../docs/adr/0025-goal-change-rules.md).

- **Product rows of the roadmap**:
  - **D-1** (FR-001, FR-003 step 6, FR-006, US1-AS4, US1-AS8, US3-AS5; SPEC §11.7, second part): what a streak
    means after goal settings change. Decided: a change keeps the schedule kind, so the streak keeps its unit and
    runs across versions, each result judged by the version of its date; a different schedule kind needs a new
    attempt. Rejected: any change allowed, with the current streak restarting when the unit changes (extra rules
    for mixed units in consistency and the longest streak); any change allowed, with every change restarting the
    current streak (raising a target wipes out a long streak). Accepted cost: going from "daily" to "3 times a
    week" starts a new attempt and a streak from zero.
- **Decisions of this spec**:
  - **D-2** (FR-008, FR-009, US2-AS1 to AS3, AS5, AS11, SC-003): which results belong to an ended attempt.
    Decided: the commitment or period in progress at the ending, and everything after it, leave the attempt;
    Pending ones finalize as usual. Rejected: the attempt includes the ending date (a per-week goal ended mid-week
    gets a Failed week, so ending creates a failure, against SPEC §5.4); unrecorded Pending results left out too
    (days already missed vanish from the figures). Accepted cost: a commitment recorded on the ending date is not
    counted.
  - **D-3** (FR-030, FR-031, HP-011): where the demo goal changes live. The figures of `demo1` to `demo6` are
    locked by F001, F004, F005, F007 and F008, so any change there changes locked criteria. The recommended
    answer, following ADR 0023's "Later slices" rule, is a sixth startup task, demo goal-change loading, which
    creates `demo7@example.com`. It needs a new ADR that amends ADR 0023's list of startup tasks. This is a
    technical decision with a recommended answer, not a marker. It is confirmed in `/speckit-clarify`.

## Assumptions

- Community views (slice 9) are out of scope. A later community slice uses each goal's latest attempt, and leaves
  out ended goals unless its own spec decides otherwise.
- The habit and the start date of a goal cannot be changed. A different habit is a different goal, and a goal
  created with the wrong start date is ended ("No longer relevant") and created again.
- An ending cannot be undone. The user starts a new attempt instead.
- The goal page shows its latest attempt in full (figures, trend, History with corrections, recent notes).
  Earlier attempts show their figures, outcome and note in the Earlier attempts card. Their dates stay
  correctable through the record use cases, but no page lists an earlier attempt's History in this slice.
- The ending note is private, at most 500 characters like a note, and is not a note of a commitment or period.
- A new attempt can have any settings, including another schedule kind, as it starts a new streak [D-1].
- Nothing is sent by email or any other channel when a goal is changed, ended or restarted (SPEC §9).
- No account is deactivated yet (slice 11), so there is no rule yet for a deactivated owner's goals.
