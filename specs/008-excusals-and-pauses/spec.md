# Feature Specification: Excusals and pauses

**Feature Branch**: `claude/speckit-specify-slice-7-c0f9d5`

**Created**: 2026-11-21

**Status**: Draft

**Input**: User description: "slice 7"

Roadmap outcome (slice 7, `excusals-and-pauses`): Excused commitments and paused expectations are left out of
consistency and neither extend nor break the streak (SPEC §5.3). The roadmap adds: this slice extends the
evaluation of slice 5 and the reports of slice 6, and SPEC §11.2 may rework slice 5's period logic, not only add
two excluded states (slice note 7); it settles SPEC §11.2 (partially overlapping pauses) and SPEC §11.3
(excusing after finalization); the goal page gets Pause, the Pauses card and Excused and Paused cells, the Goals
page its Paused tab, and the signed-in Home "Excuse" on Pending rows (roadmap "Pages across slices").

## Clarifications

### Session 2026-11-21 (specify)

Answered by the owner during `/speckit-specify` (the three open questions of the first draft). They reject real
alternatives, and D-1 and D-2 also bind the community slice, so all three are recorded in
[ADR 0024](../../docs/adr/0024-excusal-and-pause-rules.md) (constitution, Decision Discipline; critic pass 1),
together with the early-resume and excusal-removal answers of the clarify session below, which reject real
alternatives too:

- Q: How does a pause that covers only part of a per-week evaluation period affect that period (SPEC §11.2)? → A:
  The whole week is Paused when at least one of its dates is in a pause of its goal. Commitments and per-day
  periods are whole dates and are Paused exactly on the paused dates (D-1; FR-007, FR-009, US2-AS12).
- Q: When can a commitment be excused (SPEC §11.3)? → A: Only while it is Due or Pending: never before it begins
  (Upcoming; a planned absence uses a pause) and never once it is final (Met, Failed or Missed). `CONTEXT.md`'s
  Excused entry, which said "upcoming or Pending", now says "Due or Pending" (D-2; FR-002, US1-AS2, US1-AS4,
  US1-AS5).
- Q: Can a commitment that already has a record be excused? → A: No: "This commitment has a record. Delete the
  record to excuse it." An Excused commitment never has a record, as an excused one cannot be recorded (FR-005)
  (D-3; FR-002, US1-AS10).

### Session 2026-11-21 (clarify)

- Q: Where do the demo excusals and pauses come from, given that the demo data of `demo4` and `demo5` is fixed by
  the locked criteria F005 and F007? → A: A fifth startup task, *demo pause loading*, after demo report loading,
  creates `demo6@example.com` with the goals of FR-031; F001, F004, F005 and F007 stay locked. The task's order and
  the amended "Later slices" rule of ADR 0021 are recorded in
  [ADR 0023](../../docs/adr/0023-demo-pause-loading.md) (D-4; FR-030, FR-031, HP-010).
- Q: When a user resumes a paused goal early, is today expected again? → A: Yes. The pause ends on the day before
  the goal's current date, and a pause that began on the current date is removed; the user records today within
  its grace period as usual. A per-week period that keeps a paused date stays Paused (D-1) (FR-010, US2-AS5,
  US2-AS6).
- Q: Until when can a user take back an excusal? → A: Only while the commitment's grace period is open; from the
  instant it ends (that instant included) the excusal is permanent, as lasting as a final result (FR-004,
  US1-AS8; `CONTEXT.md` Excused).

## Decisions this slice cites

Recorded in earlier slices and not decided again here:

- Use cases take the acting user as an explicit parameter and return named outcomes that carry the literal text
  this spec gives; expected refusals are outcomes, not errors ([ADR 0005](../../docs/adr/0005-use-case-convention.md)).
- The administrator role grants no access to private data (slice 1 FR-015; SPEC §3, §8). Goals are for
  registered users only (slice 4 FR-021).
- Partly built pages show only the sections and links whose slice has shipped
  ([ADR 0008](../../docs/adr/0008-partly-built-pages.md)). Ended goals, earlier attempts and Edit settings
  (slice 8) and community views (slice 9) stay absent.
- Time zones: a day runs from local midnight to local midnight in the time zone of the goal settings version in
  effect for it; a goal's *current date* is the date of the supplied time on its own timeline
  ([ADR 0016](../../docs/adr/0016-time-zones.md); slice 4 FR-011, FR-012).
- Evaluation states, the 48-hour grace period and corrections (slice 5 FR-006, FR-010 to FR-016).
- Evaluation and figures on read: every state and figure is computed when it is read, from stored facts and the
  supplied time; nothing derived is cached or stored ([ADR 0018](../../docs/adr/0018-evaluation-on-read-and-demo-history.md),
  [ADR 0020](../../docs/adr/0020-derived-figures-on-read.md)). Excusals and pauses are stored facts; the
  states Excused and Paused are derived from them on read.
- A Pending result never extends and never resets a streak (slice 6 D-1).
- Demo data: each later slice extends a demo task or adds a further task by a new ADR when locked criteria would
  otherwise change ([ADR 0021](../../docs/adr/0021-demo-report-loading.md), "Later slices").
- Integration and UI tests: [ADR 0006](../../docs/adr/0006-integration-tests.md). Acceptance tests:
  [ADR 0003](../../docs/adr/0003-acceptance-tests.md).

## Earlier behaviour these tests use

Restated here, unchanged, so that this spec alone gives the rules and texts its acceptance tests depend on:

- **Password rule** (slice 1 FR-002): every password the system sets (registration, administrator, demo
  accounts) has at least 8 characters and contains at least one digit; otherwise registration is refused with
  "At least 8 characters, with a number.", and demo loading, demo history loading, demo report loading and demo
  pause loading give "demo password does not meet the password rule" and create nothing (`walking42`,
  `admin2026` and `example2026` meet it; `abc` does not).
- **Seed catalog** (slice 2, demo loading): demo loading loads the seed catalog into an empty catalog, with
  Published catalog habits that include "Meditation" and "Drinking water" (the habits of demo pause loading's
  goals, FR-031). So after demo loading on an empty database, both goals of FR-031 are created; an administrator
  can unpublish a seed habit like any other.
- **Unpublishing a habit that goals use** (slice 4 FR-033, FR-034): unpublishing a Published catalog habit that
  at least one goal uses gives, without confirmation, "confirmation needed" and leaves it Published; with
  confirmation it unpublishes. A goal on an Unpublished habit keeps working for its owner.

- **Goals and schedule kinds** (slice 4 FR-001 to FR-013): a goal on fixed days of the week is
  *commitment-based*, each scheduled day on or after its start date being one commitment; a goal of N times per
  day or per week is *period-based*, each day, or each Monday-to-Sunday week from the first one that starts on or
  after the start date, being one evaluation period. A goal without a name of its own is named by its habit's
  name, ", " and its target text ("Reading, 20 min daily", "Yoga, 3 times a week"). A goal's start date cannot be
  in the past, so scenarios that need past results create the goal at an earlier supplied time.
- **States** (slice 5 FR-010, FR-012, FR-016): at the supplied time each commitment or period is Upcoming (not
  begun), Due (begun, not ended), Pending (ended, grace period of exactly 48 elapsed hours still open, with or
  without records), or, once the grace period has ended (that instant included), Met or Failed (by its records
  against the target of the settings version of its date) or Missed (no record). Met, Failed and Missed are
  *final*. A commitment of Wednesday 11 November ends at Thursday 12 November 00:00 and is final from Saturday 14
  November 00:00.
- **Records** (slice 5 FR-001 to FR-006): a commitment has at most one record, a period up to 50; a record is
  accepted only for a date of a commitment or period of the goal ("Nothing is scheduled on this date.") whose day
  has begun ("You cannot record a day that has not begun."); any record can be added, changed or deleted at any
  time, and the state is always computed from the records as they are now (a *correction* when the result was
  final). Reading what can be recorded now (slice 5 FR-011) returns every Due and every Pending commitment or
  period.
- **Consistency** (slice 6 FR-001, FR-002): the final results of the report range (the current attempt up to the
  supplied time), Met as successful, Failed and Missed as unsuccessful, given as "{percentage}% ({successful} of
  {counted})" with the percentage rounded down, or "N/A" with no counted result.
- **Streaks** (slice 6 FR-003, FR-004): over the final results in date order, Met extends by 1 and Failed or
  Missed resets to 0; the current streak counts the Met results after the latest Failed or Missed one, the
  longest the largest run; units "commitments", "days" or "weeks" (singular for exactly 1).
- **Recent results, trend, overview and dashboard** (slice 6 FR-005, FR-006, FR-011, FR-012): as slice 6 gives
  them, extended by FR-015 to FR-019 below.
- **Privacy texts** (slices 1, 4 and 5): an anonymous visitor gets "You are not signed in."; an administrator gets
  "Goals are for registered users."; a goal identifier that is not one of the acting user's goals gives "This
  goal does not exist.".

## What acceptance tests can reach

ADR 0003 limits acceptance tests to the Application use cases, the two `HT.Persistence` entry points and the
Domain types they use. As in slices 1 to 6:

- **FR-NNN** are application behaviours, reachable through Application use cases. Demo loading tasks (FR-030,
  FR-031) are operator capabilities that acceptance tests call directly.
- **HP-NNN** are host, page and integration requirements, verified by unit, integration and selective UI tests,
  not by acceptance tests.
- Scenarios marked *(page)* describe page or browser behaviour and are expected to be listed as "Not testable
  through the API" in `acceptance.md`.
- FR numbers are grouped by topic; FR-026 to FR-029 are not used, and demo data starts at FR-030.
- The current time is supplied to every use case (constitution II).

Test data: all acceptance scenarios share one database in one run (ADR 0003). Scenarios use their own accounts
and goals and check lists by inclusion, exclusion and order, never by totals over the whole database. The demo
scenarios start from a reset database.

Dates in the scenarios are in 2026 and in the account's time zone, which is UTC unless a scenario says otherwise;
Monday 16 November 2026 is a Monday.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Excuse a commitment (Priority: P1)

A registered user who cannot do a scheduled commitment, for a reason they choose (ill, travelling, a change of
plan), marks that one commitment as Excused, without asking anyone. It is then left out of their consistency and
neither extends nor breaks their streak. While the commitment's grace period is still open they can take the
excusal back and record it after all.

**Why this priority**: SPEC §5.3's first rule. Without it a single unavoidable absence breaks a long streak of a
commitment-based goal, which is the main reason users give up tracking.

**Independent Test**: Create a commitment-based goal at an earlier supplied time, excuse one Pending commitment,
and read its history and report after its grace period has ended: the commitment is Excused, not Missed, and is
not counted.

**Acceptance Scenarios** (goal "Yoga, 3 days a week", Build, yes/no, on Monday, Wednesday and Friday, created on
Monday 2 November with that start date):

1. **Given** the Wednesday 11 November commitment has no record, **When** the user excuses it on Thursday 12
   November at 10:00 (it is Pending) with the reason "Ill", **Then** the result is "excused", the history gives
   Wednesday 11 November as Excused with the reason "Ill", and what can be recorded now no longer contains it.
2. **Given** the Friday 13 November commitment, **When** the user excuses it on that day at 07:00 (it is Due)
   without a reason, **Then** it is Excused with no reason.
3. **Given** the Wednesday 11 November commitment excused as in scenario 1, **When** its history is read on
   Saturday 14 November at 00:00, after its grace period has ended, **Then** it is still Excused, not Missed, and it
   stays Excused at any later time.
4. **Given** the Monday 9 November commitment has no record, **When** the user tries to excuse it on Thursday 12
   November (its grace period ended on Thursday 12 November at 00:00; it is Missed), **Then** the result is "This
   commitment is final and can no longer be excused." and it stays Missed [D-2].
5. **Given** the Friday 13 November commitment, **When** the user tries to excuse it on Thursday 12 November (it is
   Upcoming), **Then** the result is "You cannot excuse a commitment that has not begun." [D-2].
6. **Given** a period-based goal "Yoga, 3 times a week", **When** the user tries to excuse any date of it, **Then**
   the result is "Only commitments can be excused. This goal is evaluated by period." and nothing changes.
7. **Given** the goal, **When** the user tries to excuse Tuesday 10 November (no commitment), **Then** the result is
   "Nothing is scheduled on this date.".
8. **Given** the Wednesday 11 November commitment excused as in scenario 1, **When** the user removes the excusal
   on Friday 13 November at 23:59, **Then** the result is "excusal removed", the commitment is Pending again and can
   be recorded; **When** instead they try it on Saturday 14 November at 00:00, **Then** the result is "This excusal
   can no longer be removed." and it stays Excused.
9. **Given** the Wednesday 11 November commitment excused, **When** the user tries to record it while it is
   excused, **Then** the result is "This commitment is excused. Remove the excusal to record it." and no record is
   stored.
10. **Given** the Wednesday 11 November commitment has a record of "No" and is Pending, **When** the user excuses
    it, **Then** the result is "This commitment has a record. Delete the record to excuse it." and the commitment
    stays Pending with its record [D-3]; **When** the user deletes the record and then excuses it, **Then** it is
    Excused.
11. **Given** a reason of 201 characters, **When** the user excuses a Pending commitment with it, **Then** the
    result is "Keep the reason to 200 characters or fewer." and nothing changes; 200 characters are accepted.
12. *(page)* **Given** a Pending commitment of a commitment-based goal on the signed-in Home, **When** the user
    chooses "Excuse" on its row, **Then** the Excuse dialog opens for that commitment; confirming it removes the row
    from the Today list and lowers the "Pending · still can record" tile by 1.

---

### User Story 2 - Pause and resume a goal (Priority: P1)

A registered user who knows they will not pursue a goal for a while (illness, a holiday, a busy month) pauses the
goal for a date range, with an optional reason. While it is paused nothing is expected: its commitments or
periods are Paused, not Missed, and its streak waits. They can resume early, and cancel a pause that has not
begun.

**Why this priority**: SPEC §5.3's second rule, and the only exemption for period-based goals, which cannot be
excused.

**Independent Test**: Create a per-day goal at an earlier supplied time, pause it from today for a week, read
the history and report during and after the pause: the paused days are Paused and not counted, and the days
before the pause keep their results.

**Acceptance Scenarios** (goal "Drinking water, 8 glasses a day", Build, count at least 8 "glasses", once per day,
created on Monday 2 November with that start date):

1. **Given** the goal, **When** the user pauses it on Monday 16 November at 09:00 from Monday 16 November until
   Sunday 22 November with the reason "Family visit", **Then** the result is "paused", the goal is paused until
   Sunday 22 November, Monday 16 November is Paused and not in what can be recorded now, and Sunday 15 November
   is still Pending and can still be recorded.
2. **Given** the goal, **When** the user tries on Monday 16 November to pause it from Sunday 15 November, **Then**
   the result is "A pause cannot start before today." and nothing changes.
3. **Given** the goal, **When** the user tries to pause it from Friday 20 November until Thursday 19 November,
   **Then** the result is "The last day cannot be before the first day.".
4. **Given** the pause of scenario 1, **When** the user tries to add a pause from Saturday 21 November until
   Tuesday 24 November, **Then** the result is "This goal already has a pause on these dates.".
5. **Given** the pause of scenario 1, **When** the user resumes the goal on Wednesday 18 November at 08:00,
   **Then** the result is "resumed", the pause runs from Monday 16 to Tuesday 17 November, those two days stay
   Paused, Wednesday 18 November is Due and can be recorded, and the goal is no longer paused.
6. **Given** the pause of scenario 1, **When** the user resumes the goal on Monday 16 November at 18:00, the
   pause's first day, **Then** the pause is removed, Monday 16 November is Due again and its records count as if
   the goal had never been paused.
7. **Given** a pause from Monday 23 until Friday 27 November, set on Monday 16 November, **When** the user cancels
   it on Wednesday 18 November, **Then** the result is "pause cancelled" and the goal has no pause on those dates;
   **When** instead they try to cancel the pause of scenario 1 on Wednesday 18 November, **Then** the result is
   "Only a pause that has not begun can be cancelled. Resume the goal instead.".
8. **Given** Friday 13 November recorded with 6 glasses (Failed from Monday 16 November 00:00), **When** the user
   pauses the goal on Monday 16 November, **Then** Friday 13 November stays Failed: a pause never changes a result
   that was final or Pending when it was set.
9. **Given** the pause of scenario 1, **When** the user tries to add a record for Tuesday 17 November on that day,
   **Then** the result is "This goal is paused on this date." and nothing is stored.
10. **Given** Monday 16 November recorded with 8 glasses at 08:00, before the pause of scenario 1 was set at
    09:00, **Then** the record is kept and shown with the Paused day; it can be changed or deleted, and the day
    stays Paused.
11. **Given** the goal "Yoga, 3 days a week" of User Story 1 (Build, yes/no, Monday, Wednesday and Friday, start
    date Monday 2 November), **When** the user pauses it on Wednesday 18 November at 08:00 from Wednesday 18 until
    Friday 20 November, **Then** the result is "paused" and its commitments of Wednesday 18 and Friday 20 November
    are Paused; **When** the user resumes it on Friday 20 November at 08:00, **Then** the pause runs from Wednesday
    18 to Thursday 19 November, Wednesday 18 November stays Paused and Friday 20 November is Due.
12. **Given** a per-week goal "Yoga, 3 times a week" (Build, yes/no, 3 times per week, created on Monday 2 November
    with that start date), **When** the user pauses it on Wednesday 18 November at 08:00 from Thursday 19 until
    Wednesday 25 November, **Then** the weeks 16–22 November and 23–29 November, each partly paused, are both
    Paused as a whole, whatever their records [D-1]; **When** the user tries on Wednesday 18 November at 09:00 to
    add a record for that date (not in the pause, in a Paused week), **Then** the result is "This week is paused."
    and nothing is stored; **When** the user resumes the goal on Monday 23 November at 10:00, **Then** the pause
    runs from Thursday 19 to Sunday 22 November, the week 16–22 November stays Paused and the week 23–29 November
    is Due again.
13. **Given** the goal paused as in scenario 1, **When** the user reads its pauses, **Then** they get the pause
    with its first day, last day, reason "Family visit" and "current"; a pause that has ended is "past", one that
    has not begun "upcoming", newest first day first.
14. *(page)* **Given** the goal page of a goal, **When** the user chooses "Pause", fills "From" and "Until" and
    confirms, **Then** the page shows the Notice "Paused until Sun 22 Nov · Resume now" and the Pauses card lists the
    pause.

---

### User Story 3 - Reports leave excused and paused expectations out (Priority: P1)

The consistency, streaks, recent results and trend of slice 6 treat Excused commitments and Paused commitments
or periods as exempt: they are not counted in consistency, they neither extend nor reset a streak, and they are
shown as Excused or Paused where results are listed.

**Why this priority**: it is the outcome the roadmap names for this slice; without it excusals and pauses would
only change a label.

**Independent Test**: Build one goal with Met, Missed, Excused, Paused and Pending results at known supplied
times and read its report: the figures equal those computed with the exempt results removed from the sequence.

**Acceptance Scenarios** (goal "Meditation, 10 min daily", Build, duration 10 minutes, all seven days
(commitment-based), created on Monday 2 November with that start date; 10 minutes recorded on each date from 2 to
16 November except: 9 November no record, excused on 10 November; 11 November no record; 13 to 15 November no record,
paused on 13 November at 08:00 from 13 until 15 November; nothing recorded from 17 November on; the report read on Thursday 19 November at
12:00):

1. **Given** the goal, **When** its report is read, **Then** the consistency is "90% (10 of 11)": 9 November
   (Excused) and 13 to 15 November (Paused) are not counted, 11 November (Missed) is; 17 and 18 November are
   Pending and 19 November is Due, none of them counted.
2. **Given** the goal, **When** its report is read, **Then** the current streak is "2 commitments" (12 and 16
   November; the Paused days between them neither extend nor reset it) and the longest "8 commitments" (2 to 8 and
   10 November; the Excused 9 November does not break the run).
3. **Given** the goal, **When** its recent results are read, **Then** they are its 17 ended results from 2 to 18
   November, oldest first, with 10 Met, 1 Missed, 1 Excused, 3 Paused and 2 Pending, and the number of each state.
4. **Given** the goal, **When** its trend is read, **Then** its 14 bars run from 6 to 19 November; 9 November is
   marked Excused and 13 to 15 November Paused, all four without a value; 11 November (Missed) and 17 to 19
   November (no record) have no value and no mark; the other six bars (6 to 8, 10, 12 and 16 November) have the
   value 10.
5. **Given** the excusal of 9 November removed on 10 November at 12:00 instead, and 9 November then recorded with 4
   minutes, **When** the report is read, **Then** the consistency is "83% (10 of 12)" and the longest streak "7
   commitments": the figures are the same as if the record had been entered on time.
6. **Given** a second goal "Meditation, 10 min daily" of another account (same settings, created on Monday 2
   November with that start date, nothing recorded), whose Monday 2 November commitment is excused on Tuesday 3
   November at 08:00 and which is paused at that time from Tuesday 3 until Sunday 8 November, **When** its report is
   read on Monday 9 November at 12:00, **Then** every ended result (2 November Excused, 3 to 8 November Paused) is
   exempt and none is counted: the consistency is "N/A" and both streaks are "0 commitments".
7. **Given** a per-week goal "Yoga, 3 times a week" (Build, yes/no, 3 times per week, created on Monday 2 November
   with that start date) with three "Yes" records in the week 2–8 November, paused on Monday 9 November at 08:00
   from 9 until 15 November, and with three "Yes" records in the week 16–22 November, **When** its report is read on
   Wednesday 25 November at 12:00, **Then** the paused week is not counted, the consistency is "100% (2 of 2)" and
   the current streak is "2 weeks".
8. *(page)* **Given** the goal page of the goal, **Then** the strip shows Excused and Paused cells with their shapes,
   the legend lists Met, Failed, Missed, Excused, Paused and Pending, and the strip's label counts each state.

---

### User Story 4 - Paused goals on the Goals page and Home (Priority: P2)

On the Goals page a paused goal moves from the Active tab to the Paused tab, which says until when it is paused
and offers "Resume". The signed-in Home no longer expects anything of a paused goal or an excused commitment.

**Why this priority**: it makes pauses visible where the user starts, but the rules of User Stories 1 to 3 hold
without it.

**Independent Test**: Pause one of two goals and read the goals overview and the dashboard: the paused goal is
flagged with its last paused day, has no Due entry, and is not counted among active goals.

**Acceptance Scenarios** (one account with two goals, both created on Monday 2 November with that start date:
"Drinking water, 8 glasses a day" as in User Story 2, and "Meditation, 10 min daily", Build, duration 10 minutes,
all seven days; both recorded on target on each date from 2 to 13 November and not recorded from 14 November on;
"Drinking water" paused on Monday 16 November at 09:00 from Monday 16 until Sunday 22 November):

1. **Given** the two goals, **When** the goals overview is read on Monday 16 November at 10:00, **Then** "Drinking
   water, 8 glasses a day" is given as paused until Sunday 22 November and "Meditation, 10 min daily" as not
   paused.
2. **Given** the two goals, **When** the dashboard is read on Monday 16 November at 10:00, **Then** "Drinking water"
   has no Due entry and no Upcoming entry (17 to 22 November are Paused), its Pending entries of 14 and 15
   November are still listed, "Meditation" has its Due entry of 16 November, and the count of goals is 1.
3. **Given** the two goals, **When** the user excuses the Pending Sunday 15 November commitment of "Meditation" on
   Monday 16 November at 10:00 and the dashboard is read at 10:01, **Then** that commitment is in neither the
   Pending count nor the Today list, and its 14 November commitment still is.
4. **Given** another account's goal "Yoga, 3 days a week" as in User Story 1, with no records, paused on Monday 16
   November at 08:00 from Wednesday 18 until Wednesday 18 November, **When** the dashboard is read on Tuesday 17 November at 10:00 (no commitment that
   day, so no Due entry), **Then** its Upcoming entry is Friday 20 November, its earliest Upcoming commitment that
   is not Paused.
5. *(page)* **Given** the Goals page, **Then** it shows the Tabs "Active" and "Paused" (no "Ended", slice 8); the
   Paused tab lists each paused goal with the meta "Paused until Sun 22 Nov" and the action "Resume"; the footnote
   reads "Each goal is counted on its own. Consistency counts only finalized results; pending, excused and paused
   ones are left out.".
6. *(page)* **Given** the Goals page with no paused goal, **When** the user opens the Paused tab, **Then** it says
   "No paused goals.".

---

### User Story 5 - Privacy of excusals and pauses (Priority: P1)

Excusals, pauses and their reasons are private to the goal's owner, like records and notes.

**Why this priority**: constitution III and SPEC §3: private tracking data is owner-only, administrators
included.

**Independent Test**: Try every use case of this slice as another registered user, an administrator and an
anonymous visitor on someone else's goal; each is refused and nothing changes.

**Acceptance Scenarios**:

1. **Given** user A's goal, **When** user B tries to excuse, remove an excusal, pause, resume, cancel a pause or
   read the pauses of it, **Then** each result is "This goal does not exist." and nothing changes.
2. **Given** user A's goal, **When** an administrator tries any of them, **Then** each result is "Goals are for
   registered users.".
3. **Given** user A's goal, **When** an anonymous visitor tries any of them, **Then** each result is "You are not
   signed in.".
4. **Given** a pause of user A's goal, **When** user A tries to cancel it through another of their goals, or user B
   through one of B's goals, **Then** the result is "This pause does not exist.".
5. **Given** an excusal or pause with a reason, **Then** the reason appears in no log line, exception message or
   error page (HP-009).

---

### User Story 6 - Demo excusals and pauses (Priority: P2)

A reviewer signs in to the hosted demo and sees Excused and Paused results, a goal in the Paused tab, and figures
that leave them out, without waiting for time to pass.

**Why this priority**: constitution VI and SPEC §10; the slice is not demonstrable without it.

**Independent Test**: Run the demo task on a reset database at a fixed run time and read the demo account's
reports.

**Acceptance Scenarios** (D-4):

1. **Given** a reset database with demo loading, demo history loading and demo report loading run, **When** demo
   pause loading runs with a valid demo password, **Then** its results are "created", "demo goals added" and "demo
   records added" for `demo6@example.com`, naming no goal, habit, note, reason, unit or amount.
2. **Given** demo pause loading has run on run date R, **When** `demo6@example.com` reads its reports at the run
   time, **Then** they are as FR-031 gives them, on every weekday.
3. **Given** `demo6@example.com` exists, **When** demo pause loading runs again, **Then** its result is "already
   exists" and nothing of the account is read or changed.
4. **Given** no demo password, or one that fails the password rule, **When** demo pause loading runs, **Then** its
   result is "no demo password configured" or "demo password does not meet the password rule" and it creates
   nothing.
5. **Given** demo loading, demo history loading and demo report loading, **When** they run, **Then** their results
   and data are unchanged by this slice (FR-030).

---

### Edge Cases

- **Excusing and pausing the same date**: a date in a pause cannot be excused ("This goal is paused on this
  date."). A pause set over an excused commitment makes it Paused while the pause covers it; the excusal is kept,
  so a cancelled or shortened pause shows it as Excused again.
- **A pause over today's begun entry**: a pause may start on the goal's current date, so today's commitment or
  day, already Due and possibly recorded, becomes Paused; its records are kept (US2-AS10).
- **Resuming early** never makes a paused day before the current date countable again: the pause keeps every day
  up to the day before the resume. For a per-week goal, resuming in the middle of a week leaves that week Paused,
  since some of its dates stay paused (D-1); expectations return with the next week.
- **One paused day in a week** exempts the whole week of a per-week goal (D-1), also when the week's records
  already reach the target; the records are kept.
- **A Paused week of a goal that is not paused**: a pause that begins later in the current week pauses that week
  at once (D-1), while the goal itself is not paused, as no current pause covers its current date (FR-018). This is
  intended. The goal stays on the Active tab, is counted among active goals and shows no paused Notice; its
  current week is Paused on the goal page, has no Due entry on the dashboard and cannot be recorded ("This week is
  paused."); "Resume" gives "This goal is not paused."; cancelling the upcoming pause (FR-011) makes the week Due
  again. A mid-week resume leads to the same state: the goal is no longer paused and its week stays Paused.
- **A pause before the start date**: allowed; dates before the start date have no commitment or period, so the
  pause covers only those from the start date on.
- **A goal that has not started** has no current date (slice 4 FR-012): "today" for its pauses is the account's
  today, as for slice 6's Upcoming window.
- **Time zone changes**: excusals and pauses are by date on the goal's own timeline; a time zone change (slice 4
  FR-026) never moves them, and a moved commitment of the same date keeps its excusal.
- **Grace period around a clock change**: the end of the window in which an excusal can be removed is the end of
  the commitment's grace period, exactly 48 elapsed hours after it ends (slice 5 FR-016), so it can read 23:00 or
  01:00 on the local clock.
- **Long pauses**: a pause has no maximum length; a goal paused for months shows "N/A" and its old streak unchanged
  when it resumes.
- **Notes**: an Excused or Paused commitment or period can still get a note (slice 5 FR-007 unchanged); the note is
  not its reason, and recent notes show it with the state Excused or Paused (FR-020).
- **Several pauses**: a goal can have any number of pauses that do not overlap; adjacent pauses (one ends Sunday,
  the next starts Monday) are allowed.

## Requirements *(mandatory)*

### Functional Requirements (application behaviour, on the AC surface)

**Terms used below**

- *Excusal*: the user's exemption of one commitment of a commitment-based goal, stored with its date and an
  optional reason.
- *Pause*: the user's suspension of a goal from a first day to a last day (both included) on the goal's timeline,
  stored with an optional reason. A pause is *current* when the goal's current date is in it, *upcoming* when its
  first day is later, and *past* when its last day is earlier.
- *Excused* and *Paused*: what a commitment or period is, in place of an evaluation state, while an excusal or a
  pause covers it. They are exemptions, not evaluation states (`CONTEXT.md` Evaluation state): an Excused or
  Paused commitment or period has no Upcoming, Due, Pending, Met, Failed or Missed state.
- *Exempt result*: an Excused or Paused commitment or period whose end instant is at or before the supplied time.
- *Counted result*: a final result (slice 6) that is not Excused or Paused.

**Excusals**

- **FR-001**: A registered user MUST be able to excuse one commitment of one of their own commitment-based goals,
  by its date, without approval, with an optional reason: trimmed, empty means none, at most 200 characters.
- **FR-002**: Excusing MUST give one result, checking in this order: (1) "You are not signed in."; (2) "Goals are
  for registered users."; (3) "This goal does not exist."; (4) a period-based goal "Only commitments can be excused.
  This goal is evaluated by period."; (5) a date that is not a commitment of the goal "Nothing is scheduled on this
  date."; (6) a date in a pause of the goal "This goal is paused on this date."; (7) a commitment already excused
  "This commitment is already excused."; (8) the commitment's state at the supplied time, which must be Due or
  Pending (D-2; SPEC §11.3): Upcoming "You cannot excuse a commitment that has not begun.", final (Met, Failed or
  Missed) "This commitment is final and can no longer be excused."; (9) a commitment with a record "This
  commitment has a record. Delete the record to excuse it." (D-3); (10) a reason longer than 200 characters "Keep the reason to 200 characters or fewer.".
  Otherwise the result is "excused", with the commitment's date.
- **FR-003**: An excused commitment MUST be Excused at every supplied time for as long as the excusal exists. As
  for records (slice 5 FR-006), the state is computed from the excusals as they are now; an excusal has no time of
  its own (FR-025). It never becomes Pending, Met, Failed or Missed while excused, also after its grace period
  would have ended.
- **FR-004**: A registered user MUST be able to remove the excusal of one of their commitments while its grace
  period has not ended (slice 5 FR-016; the end instant itself is too late). The commitment is then evaluated as if
  it had never been excused (slice 5 FR-010). Checks: (1) to (3) as FR-002; (4) a commitment that is not excused
  "This commitment is not excused."; (5) a grace period that has ended "This excusal can no longer be removed.".
  Otherwise the result is "excusal removed", with the commitment's state after the change.
- **FR-005**: Adding a record (slice 5 FR-005) MUST check three more steps, after its step (4) (the date) and
  before its step (5) (the value), in this order: (4a) a date in a pause of the goal "This goal is paused on this
  date."; (4b) a date of a Paused per-week period that is not itself in a pause "This week is paused."; (4c) an
  excused commitment "This commitment is excused. Remove the excusal to record it.". So a date that is both
  excused and paused gives (4a), as the pause takes precedence (FR-007). Changing and deleting a record skip (4a)
  to (4c), as they skip step (4): an Excused commitment has no record (D-3), and a record kept on a Paused date
  can be changed or deleted (FR-009). Reading what can be recorded now (slice 5 FR-011) MUST NOT return an Excused
  commitment.

**Pauses**

- **FR-006**: A registered user MUST be able to pause one of their own goals, commitment-based or period-based,
  from a first day to a last day, with an optional reason as FR-001's. Pausing MUST give one result, checking in
  this order: (1) to (3) as FR-002; (4) a first or last day missing "Choose the first and the last day."; (5) a
  first day before the goal's current date (for a goal that has not started, the account's today) "A pause cannot
  start before today."; (6) a last day before the first day "The last day cannot be before the first day."; (7)
  a range that shares a date with another pause of the goal "This goal already has a pause on these dates."; (8)
  the reason as FR-002 step (10). Otherwise the result is "paused", with the pause's identifier, first day and last
  day.
- **FR-007**: A commitment or a per-day evaluation period whose date is in a pause of its goal MUST be Paused at
  every supplied time while that pause covers the date, whatever its records. A per-week evaluation period MUST be
  Paused as a whole, whatever its records, while at least one of its seven dates is in a pause of its goal (D-1;
  SPEC §11.2). A pause takes precedence over an excusal of the same date; the excusal is kept.
- **FR-008**: A pause MUST NOT change a commitment or period that had ended when the pause was set: because a pause
  starts on the goal's current date at the earliest (FR-006), every Pending or final result before it keeps its
  state and its evaluation (SPEC §5.3: a pause cannot turn a finalized failure into an exemption). SPEC §5.3's
  "failures already recorded" are read as final Failed results: a Due commitment or period is not yet a failure
  (slice 5 D-2: a result is final only at the end of its grace period), also when its records already exceed a
  Reduce limit, so a pause from the current date (or, under D-1, a pause later in its week) makes it Paused. Its
  records are kept (FR-009), so no activity is erased (ADR 0024).
- **FR-009**: Adding a record for a date in a pause MUST be refused with "This goal is paused on this date.", and
  for a date of a Paused per-week period that is not itself in a pause with "This week is paused." (FR-005 steps
  (4a) and (4b)). A record stored before the pause covered its date MUST be kept, returned with its Paused
  commitment or period, and can be changed or deleted (slice 5 FR-005, FR-006); it never makes the date countable.
  Changing or deleting such a record returns "Paused" as the state after the change, and the change is never a
  correction, as a Paused commitment or period is not final (slice 5 FR-006). Reading what can be recorded now
  MUST NOT return a Paused commitment or period.
- **FR-010**: A registered user MUST be able to resume a goal whose current pause covers its current date: the
  pause then ends on the day before the current date, or, when it began on the current date, is removed. The
  current date's commitment or period is then evaluated as if it had never been paused, except a per-week period
  that still has a paused date, which stays Paused (FR-007). Checks: (1) to (3) as
  FR-002; (4) no current pause "This goal is not paused.". Otherwise the result is "resumed".
- **FR-011**: A registered user MUST be able to cancel an upcoming pause of their goal, which removes it. Checks:
  (1) to (3) as FR-002; (4) a pause that is not one of this goal's "This pause does not exist."; (5) a current or
  past pause "Only a pause that has not begun can be cancelled. Resume the goal instead.". Otherwise the result is
  "pause cancelled".
- **FR-012**: Reading a goal's pauses MUST return every pause of the goal, the latest first day first, each with its
  identifier, first day, last day, reason and whether it is current, upcoming or past at the supplied time.

**Reports and earlier reads**

- **FR-013**: Consistency (slice 6 FR-001, FR-002) MUST count only counted results: Excused and Paused ones are
  counted neither as successful nor in the total (SPEC §5.3, §6).
- **FR-014**: Streaks (slice 6 FR-003) MUST skip Excused and Paused results: in the date-ordered sequence of final
  results they neither extend nor reset the current or the longest streak (SPEC §5.3, §6).
- **FR-015**: Recent results (slice 6 FR-005) MUST include exempt results, with "Excused" or "Paused" in place of
  the state, their progress as slice 5 FR-014 gives it from their records as before, and the number of each state
  (Excused and Paused included), among the latest 30 (or 15) ended results.
- **FR-016**: The trend (slice 6 FR-006) MUST mark a bar Paused when its commitment, day or week is Paused, and
  Excused when its commitment is Excused; a marked bar has no value. For a commitment-based yes/no goal, a week's
  Build target counts only its commitments that are neither Excused nor Paused, and the week's value (Build and
  Reduce) counts only the "Yes" records of those commitments, so a "Yes" kept on a Paused commitment (FR-009) is
  not counted; for slice 6's rule that a bar without a record has no value, only the records of those commitments
  count, so a week whose only records sit on Paused commitments has no value (not 0). A week whose commitments are
  all Excused or Paused is marked Paused when at least one of them is Paused, and Excused otherwise.
- **FR-017**: Reading a goal's history (slice 5 FR-009) MUST give an Excused or Paused commitment or period with
  "Excused" or "Paused" in place of its state, no grace period end, its records, progress and note as before,
  "no" for whether a change now would be a correction (it is not final), and for an Excused one its reason and
  whether the excusal can still be removed (FR-004).
- **FR-018**: The goals overview (slice 6 FR-011) MUST give, for each goal, whether it is paused (a current pause
  covers its current date) and, if so, the last day of that pause. A goal whose current week is Paused only by an
  upcoming pause or after a mid-week resume is not paused (Edge Cases). Its order is unchanged.
- **FR-019**: The dashboard (slice 6 FR-012) MUST NOT count or list Excused or Paused entries as Due, Pending or
  Missed; a goal's Upcoming entry MUST be its earliest Upcoming commitment or period that is not Paused (the window
  of 6 days unchanged); the count of goals MUST count only goals that are not paused; each goal summary MUST say
  whether the goal is paused.
- **FR-020**: Recent notes (slice 5 FR-008) and the dashboard's recent notes (slice 6 FR-012) MUST give a note of
  an Excused or Paused commitment or period with "Excused" or "Paused" as its state.

**Privacy**

- **FR-021**: Every use case of this slice MUST take the acting user and act only on that user's own goals,
  excusals and pauses (constitution III). A pause is looked up only among the pauses of the acting user's goal
  named in the call; an excusal only by that goal and a date.
- **FR-022**: Every use case of this slice MUST refuse an anonymous visitor with "You are not signed in." and an
  administrator with "Goals are for registered users.", returning and changing nothing, checked in that order
  before the goal.

**Earlier use cases**

- **FR-023**: The use cases of slices 1 to 6 MUST keep giving their results for every call those slices accept on
  goals without excusals or pauses, so that their locked acceptance criteria (F001, F002, F004, F005, F007) pass
  unchanged.
- **FR-024**: Correcting, excusing, removing an excusal, pausing, resuming and cancelling MUST each change the
  figures and reads of FR-013 to FR-020 on the next read, exactly as if the change had been made at the time it concerns
  (constitution II).
- **FR-025**: An excusal and a pause MUST be stored as facts with their dates and reason; their effect is derived
  on read, and nothing else is stored, no time of making included (ADR 0018, ADR 0020).

**Demonstration data** (D-4; [ADR 0023](../../docs/adr/0023-demo-pause-loading.md))

- **FR-030**: Demo loading, demo history loading and demo report loading MUST stay exactly as slices 1 to 6 specify
  them, with no excusal or pause, so that F001, F004, F005 and F007 stay locked and unchanged.
- **FR-031**: *Demo pause loading*, a new operator capability that runs after demo report loading, MUST take the
  demo password as demo loading does ("no demo password configured", "demo password does not meet the password
  rule", creating nothing in both cases). With a valid one it creates `demo6@example.com`, a registered user with
  the demo password (result "created"), or leaves an existing account of that email unchanged (result "already
  exists", ignoring case; none of its private data read or changed, ADR 0013). The new account has no account name
  and no custom habits, and uses the installation's default time zone. It gets, as a whole with the account, two
  goals that start 28 days before the run date R ("R − k" is the date k days before R) and their records, notes,
  excusals and pauses, stored directly as the facts of FR-025 (the rules that limit when a user can excuse or
  pause, FR-002 and FR-006, apply to users, not to this task); each goal is left out when its catalog
  habit is not Published (looked up by name, ignoring case), and its name follows slice 4 FR-010:
  - on catalog habit "Meditation": Build, duration 10 minutes, all seven days ("Meditation, 10 min daily"); 10
    minutes on each date from R − 28 to R − 3, except: R − 20 and R − 19 no record, excused with the reason
    "Travelling."; R − 14 no record, with the note "Overslept."; R − 8 no record, excused without a reason; R − 2
    no record, excused without a reason; R − 1 and R no record;
  - on catalog habit "Drinking water": Build, count at least 8 "glasses", once per day ("Drinking water, 8 glasses
    a day"); a past pause from R − 18 to R − 12 with the reason "Ill with flu." and a current pause from R − 1 to
    R + 5 with the reason "Family visit."; 8 glasses on each date from R − 28 to R − 19 and from R − 11 to R − 2,
    except R − 5: 6 glasses, with the note "Busy day."; no record from R − 18 to R − 12, on R − 1 or on R.

  Its results for a created account are "created", "demo goals added" and "demo records added" for
  `demo6@example.com`, also when some or all goals are left out; they name the demo email but never a goal, habit,
  note, reason, unit or amount. Read by `demo6@example.com` at the run time, on every weekday:
  - "Meditation, 10 min daily": counted results R − 28 to R − 3 except the Excused R − 20, R − 19 and R − 8, of
    which R − 14 Missed: "95% (22 of 23)", current streak 10 commitments, longest 12 commitments; R − 2 Excused,
    R − 1 Pending, R Due;
  - "Drinking water, 8 glasses a day": counted results R − 28 to R − 19 and R − 11 to R − 3, of which R − 5 Failed:
    "94% (18 of 19)", current streak 2 days, longest 16 days (R − 28 to R − 6, across the Paused R − 18 to R − 12);
    R − 2 Pending; R − 1 and R Paused; the goal is paused until R + 5;
  - the dashboard: 1 Due, 2 Pending, 0 Missed this week, 1 goal counted (the paused one is not).

  These figures hold in the default time zone UTC, which has no clock change (the hosted demo uses UTC).

### Host, page and integration requirements (outside the AC surface)

- **HP-001**: The goal page (`/goals/{id}`, `design/pages/PageGoalDetail`) MUST add (ADR 0008):
  - in the page head, the secondary action "Pause" (`aria-haspopup="dialog"`), before "Record", opening the
    PauseDialog; absent while the goal is paused;
  - under the head, while the goal is paused, the Notice "Paused until {Sun 22 Nov} · Resume now", "Resume now"
    being a button that resumes the goal (FR-010);
  - in the right column, after Settings, the "Pauses" card: each pause of FR-012 as "{Mon 16 Nov} – {Sun 22 Nov}",
    its reason (`ht-muted`) when it has one, and "Resume now" on the current pause or "Cancel pause" on an upcoming
    one; absent when the goal has no pause;
  - in the History card, the StatusChips "Excused" and "Paused"; an Excused row shows its reason after the chip
    (`ht-muted`) and, while it can be removed, the ghost action "Remove excusal", but no "Add record"; a Paused
    row offers no "Add record" (FR-005), while a record kept on it keeps its edit action (FR-009) and both rows keep
    their note actions (slice 5 FR-007);
  - in the GoalStats card, Excused and Paused cells in the strip and the legend Met, Failed, Missed, Excused, Paused,
    Pending; in the TrendChart, marked bars (FR-016) drawn as the design gives them.
- **HP-002**: The PauseDialog (`design/components/PauseDialog`) MUST follow the design: the title "Pause · {goal
  name}", the effect first ("Paused days are not counted. Your streak stays as it is. Final results do not
  change."), the fields "From" (today by default, not before today), "Until" (required) and "Reason (optional)",
  the primary "Pause goal" and "Cancel"; refusals under the field they are about, one at a time, with the result's
  message as it is.
- **HP-003**: The Excuse dialog (`design/components/Dialog`, new content in Design changes) MUST show the title
  "Excuse · {goal name}", the sub line with the date and state ("Wed 11 Nov · Pending, 22 h left"), the effect ("An
  excused commitment is not counted. Your streak stays as it is."), "Reason (optional)", the primary "Excuse
  commitment" and "Cancel". It opens from "Excuse" on the Home's Pending rows of commitment-based goals and from
  "Excuse instead" in the RecordDialog (ghost, right), shown only for a commitment-based goal whose "For" entry can
  be excused (FR-002); the RecordDialog's "For" lists no Excused or Paused entry (FR-005, FR-009).
- **HP-004**: The Goals page (`/goals`, `design/pages/PageGoals`) MUST show the Tabs "Active" and "Paused" (no
  "Ended", slice 8): Active lists the goals that are not paused as slice 6 HP-002 does; Paused lists the paused goals
  in the same order, each with the meta "Paused until {Sun 22 Nov}", the Pending StatusChip when it has a Pending
  result, its figures and strip, and the action "Resume" (secondary); an empty Paused tab says "No paused goals.".
  The footnote reads "Each goal is counted on its own. Consistency counts only finalized results; pending, excused
  and paused ones are left out.". The selected tab is kept in the address (`?tab=paused`).
- **HP-005**: The signed-in Home (`design/pages/PageHomeSignedIn`) MUST add the ghost "Excuse" on Pending rows of
  commitment-based goals that can be excused (HP-003); the "Active goals" tile and card count and list only goals
  that are not paused (FR-019). Nothing else changes.
- **HP-006**: StatusChips "Excused" (`ht-status--excused`) and "Paused" (`ht-status--paused`) and strip cells
  `ht-cell--excused` and `ht-cell--paused` MUST come from the existing design classes and the shared text source;
  a state is never shown by colour alone; the strip's `aria-label` counts Excused and Paused too ("Last 17
  commitments: 10 met, 1 missed, 1 excused, 3 paused, 2 pending"); the trend's `aria-label` and bar `title` tell
  marked bars apart from empty ones.
- **HP-007**: Every dialog of this slice MUST manage focus (first field on open, Esc and × close, focus returns to
  the opener, or to the nearest remaining control when the opener is gone) and meet constitution IV.
- **HP-008**: The use cases' texts ("Paused until …", state words, footnote) MUST come from the shared formatter and
  text source of slice 6 HP-005, so pages build none.
- **HP-009**: Logs, exception messages and error pages MUST NOT contain excusal or pause reasons, notes, recorded
  values, goal names or habit names (constitution III).
- **HP-010**: The host MUST run demo pause loading at every start, after demo report loading, and log its results
  as demo report loading's are logged (slice 6 HP-011), with "Demo pause loading: {result}" when the password is
  missing or weak. A fault in it is logged without secrets and does not stop the start. The deployment's success
  check (ADR 0015) also requires its result; the daily scheduled refresh re-creates `demo6` with the rest of the
  demo data. The task's order and the amended "Later slices" rule are recorded in
  [ADR 0023](../../docs/adr/0023-demo-pause-loading.md) (D-4).
- **HP-011**: The hosted demo's smoke check (`tools/HT.SmokeCheck`) MUST also find, signed in as
  `demo6@example.com`, the word "Excused" on the page of "Meditation, 10 min daily", and "Paused until" with the goal
  "Drinking water, 8 glasses a day" on the Goals page's Paused tab. Every data parameter of demo pause loading's
  port gets its `SmokeCheckCoverage` entry (slice 3 "Coverage rule").
- **HP-012**: `CONTEXT.md` MUST get, in the same change as this spec's decisions (constitution I): a new entry
  Pause and a new entry Paused with their NOT-boundaries (not an end of the attempt, not an excusal, never
  retroactive); a new entry Excusal (the stored fact, with no time of its own); the Excused entry sharpened to the
  answers of D-2 and D-3; Evaluation state sharpened so that an Excused or Paused commitment or period has none; Consistency, Streak, Recent results,
  Trend and Dashboard naming Excused and Paused as left out; Demo account listing `demo6@example.com`, a new entry
  for Demo pause loading, and Demo refresh listing it among the tasks that run again. These were added on
  2026-11-21 together with the answers to D-1 to D-4.

### Earlier requirements and tests this slice changes

- Slice 5 FR-005 (record checks) and FR-011 (what can be recorded now): extended by FR-005 and FR-009 with the
  excused and paused refusals and exclusions. Slice 5's locked criteria (F005) use no excusal or pause and pass
  unchanged (FR-023).
- Slice 6 FR-001, FR-003, FR-005, FR-006, FR-011, FR-012: extended by FR-013 to FR-020 as slice 6's Assumptions
  foresaw. F007 uses no excusal or pause and passes unchanged; its "number of the user's goals" is the number of
  goals that are not paused (FR-019), the same for every F007 scenario.
- Slice 6 HP-002 (Goals page without Tabs, footnote without "excused and paused"), HP-001 and HP-003 (no
  Excused, Paused, Pauses card or "Excuse"): amended by HP-001 to HP-005. Tests: the goal page's, Goals page's and
  Home's UI tests, and the GoalStats, strip, TrendChart and StatusChip component tests that assert their absence.
- Slice 5 HP-002 (RecordDialog without "Excuse instead"): amended by HP-003. Test: `RecordDialogTests`
  (`US1_AS9_…_and_no_Excuse_instead`, which asserts that the dialog has no "Excuse").
- Slice 5 FR-010 (the state is exactly one of six): an Excused or Paused commitment or period has none of them
  (FR-003, FR-007); every other one keeps exactly one. Slice 5 FR-018 (the check order of FR-005): extended by
  FR-005's steps (4a) to (4c) between the date and the value.
- Slice 6 HP-008 and HP-011 (smoke check, startup tasks): extended by HP-010 and HP-011; the host's startup tests
  and smoke-check tests that compare "Demo " lines, check names and coverage counts exactly change to expect
  `demo6@example.com` after `demo5@example.com`.
- Slice 5 FR-006 (the state after recording, changing or deleting), FR-008 (recent notes) and FR-009 (history)
  give Excused and Paused (FR-009, FR-017, FR-020); slice 4's evaluation calendar (FR-013) is unchanged: it carries
  no result.

### Design changes before build

Constitution IV and ADR 0008 require `design/` to change first:

- `design/pages/PageGoalDetail`: a slice-7 state with "Pause", the paused Notice, the Pauses card, Excused and
  Paused History rows with "Remove excusal", and the strip legend with Excused and Paused; still no Edit settings,
  End goal or earlier attempts.
- `design/components/PauseDialog`: the texts of HP-002 and the "Until" refusal wording.
- `design/components/Dialog` (or a new `ExcuseDialog` guide): the Excuse content of HP-003.
- `design/components/RecordDialog`: "Excuse instead" confirmed (its "Provisional" mark removed), with the rule of
  D-2.
- `design/pages/PageGoals` and `components/GoalListItem`: a slice-7 state with Tabs Active and Paused, the Paused
  item, "No paused goals." and the footnote with "excused and paused".
- `design/pages/PageHomeSignedIn` and `components/CommitmentRow`: "Excuse" on Pending rows of commitment-based
  goals; the Active goals tile and card without paused goals.
- `design/components/StatusChip`, `ConsistencyStrip`, `GoalStats`: Excused and Paused in their slice-7 states.
- `design/components/TrendChart`: the Excused bar (dashed like the paused bar, told apart in its `title` and
  `aria-label`).
- `design/pages.md` and `design/README.md`: the slice-7 structure of these pages.

### Key Entities

- **Excusal**: one commitment's exemption from evaluation: its goal, its date, an optional reason (at most 200
  characters). At most one per commitment.
- **Pause**: one goal's suspension: its goal, first day, last day, optional reason; pauses of one goal never
  overlap. Current, upcoming or past at a supplied time.
- **Excused / Paused result**: derived on read from excusals and pauses; takes the place of the evaluation state
  and is left out of consistency and streaks.

Both are private to the goal's owner, entered by the user, and never derived figures themselves.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user excuses a Pending commitment from its Home row in two actions ("Excuse", "Excuse
  commitment") and any commitment that can be excused, Due ones included, from the record form in three ("Record"
  or "Add record", "Excuse instead", "Excuse commitment"); they pause a goal from its goal page in two actions
  ("Pause", "Pause goal") after filling "Until"; and the change shows on the next read of every report.
- **SC-002**: In 100% of the report tests, the consistency and both streaks of a goal with Excused or Paused results
  equal those of the same goal with those results removed from its sequence (SPEC §5.3).
- **SC-003**: The tests cover excusing and removing an excusal just before, at and just after a grace period's end,
  pausing on, before and after the goal's current date, and resuming on the pause's first day and later
  (constitution II).
- **SC-004**: No final or Pending result that existed when a pause was set changes its state because of the pause,
  in 100% of the pause tests.
- **SC-005**: Every use case of this slice has negative tests for another registered user, an administrator and an
  anonymous visitor, and none reads or changes another account's excusal, pause or reason (constitution III).
- **SC-006**: On the hosted demo, on any day until 2027-01-31, `demo6@example.com` shows at least one Excused
  commitment, a goal in the Paused tab, and a consistency that leaves them out, without waiting for time to pass.
  The acceptance tests of FR-031 prove the figures at a fixed run time; the smoke check confirms only their
  presence (HP-011).
- **SC-007**: Every page and dialog of this slice passes the accessibility rules of constitution IV (HP-006,
  HP-007).

## Decisions due before acceptance criteria

The roadmap places two product decisions in this slice (D-1, D-2); this spec adds D-3 and D-4. All four were
answered by the owner on 2026-11-21 (see `## Clarifications`). D-1 to D-3 reject real alternatives and D-1 and D-2
bind the community slice (slice 9), so they are recorded in
[ADR 0024](../../docs/adr/0024-excusal-and-pause-rules.md); D-4 is recorded in
[ADR 0023](../../docs/adr/0023-demo-pause-loading.md).

- **Product rows of the roadmap**:
  - **D-1** (FR-007, FR-009, FR-010, US2-AS12; SPEC §11.2): a pause that covers only part of a per-week evaluation
    period. Decided: the whole week is Paused while at least one of its dates is paused. Rejected: Paused only when
    all 7 days are paused, otherwise the full target (no relief for a partial week); a target scaled to the
    unpaused days (rounding rules for Build and Reduce). Accepted cost: one paused day exempts its whole week.
  - **D-2** (FR-002, US1-AS2, US1-AS4, US1-AS5; SPEC §11.3, `CONTEXT.md` Excused): when a commitment can be
    excused. Decided: only while Due or Pending. Rejected: Upcoming commitments too (overlaps with pauses, needs
    a page entry for future commitments); final ones too (rewrites finalized history, against the spirit of SPEC
    §5.3). The glossary's "upcoming or Pending" becomes "Due or Pending".
- **Decisions of this spec**:
  - **D-3** (FR-002 step 9, US1-AS10): excusing a commitment that already has a record. Decided: refused; the user
    deletes the record first, so an Excused commitment never has a record. Rejected: excused with the record kept
    (a record and an exemption on one commitment); excused with the record deleted silently (data loss).
  - **D-4** (FR-030, FR-031, HP-010; constitution VI, ADR 0021): where the demo excusals and pauses live. The
    figures of `demo4` and `demo5` are locked by F005 and F007, so any exemption there changes locked criteria.
    Decided in `/speckit-clarify`: a fifth startup task, demo pause loading, creating `demo6@example.com`,
    recorded in [ADR 0023](../../docs/adr/0023-demo-pause-loading.md), which amends ADR 0021's "Later slices"
    rule. Rejected: unlocking F007 to add excusals and pauses to `demo5`; only a current pause on `demo1` (no
    exemptions in the past, so it fails constitution VI).

## Assumptions

- Goal changes, ending and restarting goals, the Ended tab and earlier attempts (slice 8), and community views
  (slice 9) are out of scope. A later community slice computes its figures with the exemptions of FR-013 and FR-014
  and cites ADR 0024 for them.
- Excusing an Upcoming commitment is not possible (D-2): planned absences use a pause.
- A pause is never edited, only resumed (Clarifications 2026-11-21 (clarify): the current day is expected again)
  or cancelled, and set anew.
- A pause has no maximum length; pauses of one goal must not overlap but may touch.
- The reason of an excusal or a pause is optional, private, and at most 200 characters; it is not a note and does
  not appear in Recent notes.
- Paused goals still appear in the dashboard's goal summaries with their figures; only the active-goals count and
  the Home's "Active goals" card leave them out.
- No account is deactivated yet (slice 11); no rule for a deactivated owner's excusals or pauses.
- Nothing is sent by email or any other channel when a pause begins or ends (SPEC §9).
