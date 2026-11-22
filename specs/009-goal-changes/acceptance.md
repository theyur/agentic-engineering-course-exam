# Acceptance criteria: Goal changes

**Feature**: specs/009-goal-changes · **Tag**: F009 · **Spec**: spec.md

Conventions used by every item:

- **Time.** Every use case takes the supplied current time (constitution II; spec "What acceptance tests can
  reach"). Steps run in the order they are written. A step runs at the time the item gives for it. Setup that
  gives no time (administrator creation, registration, sign-in, creating a custom habit) runs at the earliest time
  the item gives, counting the default goal-creation time below when the item uses it, so it always runs before the
  goal is created. A Then step, and every And or But step after it, checks the outcome of the item's When step; to
  check it, such a step may also read (a goal, its attempts, one attempt, a history, what can be recorded now, the
  pauses, the recent notes, a report, the goals overview, the dashboard, the goal history, the starting values of a
  new attempt or the catalog), which changes nothing. It runs at the When step's time unless it gives its own time.
  Instants are written in UTC (`2026-11-16T09:00:00Z`); dates are written `2026-11-16`. 16 November 2026 is a
  Monday, so 2026-11-02, 2026-11-09, 2026-11-16, 2026-11-23 and 2026-11-30 are Mondays.
- **Accounts.**
  - "the anonymous visitor" is the acting user who is not signed in.
  - "the registered user `x`" means the anonymous visitor registered the email `x` with the password
    `walking42`, with no time zone and no name, registration succeeded, and sign-in with `x` and `walking42`
    succeeded; `x` is then the acting user for the steps that name it. Its time zone is the default, `UTC`.
  - "the registered user `x` in `Z`" is the same, but registration also gave the time zone `Z`.
  - "the administrator `a`" means administrator creation ran with the email `a` and the password `admin2026`
    and created the administrator, and sign-in with `a` and `admin2026` succeeded.
  - "`x` changes the account's time zone to `Z`" is slice 4's time zone change use case (spec "Decisions this
    slice cites", Time zones); it succeeds.
- **Names in the database.** Emails start with `f9.acNNN.`, NNN being the item's number, so they differ from
  every other item and from the other features' acceptance tests in the same database (ADR 0003). The demo
  emails `demo1@example.com` to `demo7@example.com` appear only in items that start from a reset database.
- **Examples.** An item with an `Examples:` table runs once per row, each row as its own scenario. Every email of
  such an item contains `<n>`, the row's value in the column `n`, so rows never share data. The one exception is
  an item that starts from a reset database: it resets the database for every row, so its demo emails carry no
  `<n>`.
- **Habits and goals.** "`x` created the custom habit `C`" means `x` created it with the intent Build, the
  category Other and the icon `book-open`. Settings are written as: Build or Reduce; the measurement (yes/no, count
  or duration); for count the amount and the unit in backticks (`count 20 `pages``); for duration the amount and
  its time unit (`duration 15 minutes`); the schedule: fixed days with the days named ("fixed days on all seven
  days" is Monday to Sunday), or N times per day or per week. A missing or wrong value is written as it is: "no
  Build or Reduce", "no measurement", "yes/no with the amount 5", "count 20 with no unit", "duration 15 with no
  time unit", "no schedule", "fixed days with no day", "3 times with no period". The goals of this file and their
  names (spec "Earlier behaviour these tests use"):
  - "the goal `Reading, 15 min daily`": on the custom habit `Reading`, Build, duration 15 minutes, fixed days on
    all seven days;
  - "the goal `Meditation, 10 min daily`": on the custom habit `Meditation`, Build, duration 10 minutes, fixed days
    on all seven days;
  - "the goal `Yoga, 3 times a week`": on the custom habit `Yoga`, Build, yes/no, 3 times per week;
  - "the goal `Sugar, at most 1 sweet a day`": on the custom habit `Sugar`, Reduce, count 1 `sweet`, 1 time per
    day.

  "`x` created the goal `G` at T with the start date D" means `x` created the custom habit of G (unless `x` already
  has it), then at T created a goal on it with G's settings, the start date D and no name (slice 4's create-goal
  use case); creating succeeded. Unless an item says otherwise, a goal is created at `2026-11-02T08:00:00Z` with
  the start date `2026-11-02`. With one goal, "the goal" is that goal; with several, a goal is named by its name.
  The spec's scenarios put these goals on catalog habits of the same names. A goal's habit being a custom habit or
  a catalog habit changes no rule of this slice except the catalog's goal counts (FR-020) and a restart on an
  Unpublished catalog habit (FR-012), and the items for those two use catalog habits.
- **Shared setups.**
  - "`x` has the ended Meditation goal" means: `x` created the goal `Meditation, 10 min daily`; at
    2026-11-14T08:00:00Z `x` records 10 for each date from 2026-11-02 to 2026-11-13; at 2026-11-16T10:00:00Z `x`
    ends the goal with the outcome Abandoned and the note "Too busy this month.", which gives "goal ended" (spec
    User Story 2).
  - "`x` has the three goals" means: `x` created the goal `Reading, 15 min daily`, the goal
    `Meditation, 10 min daily` and the goal `Sugar, at most 1 sweet a day`; at 2026-11-14T08:00:00Z `x` records
    on target for each date from 2026-11-02 to 2026-11-13: 15 for `Reading, 15 min daily`, 10 for
    `Meditation, 10 min daily` and 0 for `Sugar, at most 1 sweet a day`; at 2026-11-14T09:00:00Z `x` ends
    `Sugar, at most 1 sweet a day` with the outcome Achieved and the note "Three weeks without extra sweets.";
    at 2026-11-16T09:00:00Z `x` starts a new attempt of `Sugar, at most 1 sweet a day` with Reduce, count 1
    `sweet`, 1 time per day and the start date 2026-11-16; at 2026-11-16T10:00:00Z `x` ends
    `Meditation, 10 min daily` with the outcome Abandoned and no note (spec User Story 4). Each of these
    succeeds.
- **Recording** (slice 5): "records V for D" is slice 5's record use case for the goal, the date D and the value V,
  with no note; V is `Yes` or `No` for a yes/no goal and a whole number for a count goal (in its unit) or a duration
  goal (in minutes). "records V for each date from D1 to D2" records once for every date of that range; "records V
  for each of D1, D2 and D3" records once for each date named. "changes the record of D to V" and "deletes the
  record of D" are slice 5's change-record and delete-record use cases for the only record of the date D. "saves
  the note N for D" is slice 5's save-note use case for the commitment or period that contains D. Each of these
  succeeds unless the item gives its result. The result of recording, changing or deleting is written "the state
  S, correction yes/no". "excuses D without a reason" and "removes the excusal of D" are slice 7's excusal use
  cases; "pauses the goal from D1 until D2 without a reason" is slice 7's pause use case.
- **Use cases of this slice.**
  - "changes the settings of the goal to S" is FR-001's use case with the settings S and no name; "changes the
    settings of the goal to S with the name N" gives the name N. A name written "`x` repeated 61 times" is that one
    character repeated that many times. "changes the amount to A minutes" is FR-001's use case with the goal's
    latest settings (those of its latest settings version) with the amount A, and the name it has (its own name, or
    none).
  - "ends the goal with the outcome O and the note N" / "and no note" is FR-007's use case; "with no outcome"
    leaves the outcome out. A note written "`x` repeated 501 times" is that one character repeated that many
    times.
  - "reads the starting values of a new attempt" is FR-011's use case.
  - "starts a new attempt with S and the start date D" is FR-012's use case; "with no start date" leaves the date
    out.
  - "reads the attempts of the goal" is FR-014's use case for every attempt; "reads attempt N of the goal" is
    FR-014's use case for one attempt.
  - "reads the goal history" is FR-015's use case.
- **Earlier reads.**
  - "reads the goal" is slice 4's read-goal use case, with FR-005 and FR-017.
  - "reads the history of the goal from D1 to D2" is slice 5's history use case (with FR-016).
  - "reads what can be recorded now" is slice 5's use case (with FR-016).
  - "reads the recent notes of the goal" is slice 5's recent notes use case.
  - "reads the report of the goal" is slice 6's goal report use case; its recent results and its trend are the
    parts of that report (with FR-016). "reads the goals overview" and "reads the dashboard" are slice 6's
    overview and dashboard use cases (with FR-018 and FR-019).
  - "reads the evaluation calendar of the goal from D1 to D2" is slice 4's evaluation calendar use case (with
    FR-016). Its entries are written as their date, or a per-week period as its first and last date.
  - "reads the pauses of the goal" is slice 7's pauses use case.
  - "browses the catalog" is the catalog browsing use case (slice 4 FR-031, with FR-020), "with Only habits I
    track" sets that filter; "reads the details of the catalog habit H" is the catalog details use case.
  - "renames the custom habit C to N" is slice 4's custom habit edit use case with only the name changed; "reads
    the custom habit C" and "deletes the custom habit C" are slice 4's custom habit read and delete use cases.
- **Results and figures.**
  - A refusal is written by its text. The one exception is the "used by goals" result of slice 4's unpublish and
    delete use cases, whose texts spec.md does not restate (spec Edge Cases): it is written by its effect, "the
    habit is not unpublished" or "the custom habit is not deleted", and the item then checks that effect with a
    read. A success is written by its result text and, as far as the item gives them,
    its values: "settings changed" with the date from which the settings apply, or "settings changed" with no date;
    "settings unchanged"; "goal ended" with the ending date and the last date; "attempt started" with the number
    and the start date.
  - Goal settings are written "Build, duration 15 minutes, fixed days on all seven days, from 2026-11-02", the
    settings followed by the date from which the version applies; a time zone is added ("time zone
    Europe/Kyiv") when the item checks it. "the settings in effect" and "the later settings" are FR-005's two
    versions; "no later settings" means the goal has no version that takes effect after its current date.
  - Reading a goal gives, as far as the item writes them: its name, whether the name is its own, the goal's start
    date, its latest attempt's number and start date, "not ended" or "ended" with the ending date, last date,
    outcome and note ("no note"), the settings in effect and the later settings.
  - An attempt (FR-014) is written as its fields: "attempt N", its name in backticks, its start date, "not ended"
    or its ending date, last date, outcome and note, then its consistency, current streak and longest streak.
  - A goal history entry (FR-015) is written "(`name`, attempt N, ending date D, outcome, note "N" or no note,
    the goal's latest attempt or not the goal's latest attempt)"; each entry also carries the identifier of its
    goal, which an item checks by naming the goal.
  - A history entry or result is written as its date (a per-week period: its first and last date, "2026-11-16 –
    2026-11-22"), then its state (Upcoming, Due, Pending, Met, Failed, Missed, Excused or Paused), then, as far as
    the item gives them: "record V", "records V, V, V" or "no record". An item checks only what it writes.
  - An entry of what can be recorded now or of the dashboard's Today list is written "(`goal`, date, state)". An
    entry of an ended attempt adds what FR-016 and FR-019 give: "attempt N", "the latest attempt" or "not the
    latest attempt", on the dashboard also "goal ended" or "goal not ended", and for an attempt that is not the
    goal's latest, Build or Reduce; the goal field of such an entry is that attempt's name. A recent note is
    written "(date, note, state)" or, on the dashboard, "(`goal`, date, note, state)". Inside these tuples a goal
    name is in backticks, as its comma is part of the name.
  - A pause is written "(first day, last day, no reason, current, upcoming or past)".
  - A consistency is written by its text ("90% (10 of 11)" or "N/A"); a streak by its text ("2 commitments",
    "0 weeks").
  - A goal of the overview is written "ended on D with the outcome O" or "not ended", and "paused" or "not
    paused".
- **Demo data.** "the AC database is reset" means the reset entry point of `HT.Persistence` (ADR 0003) drops
  and rebuilds the schema, so no account and no habit exists. "the catalog is seeded at T" means: the AC database
  is reset, then demo loading runs at T with the demo password `example2026`, which loads the seed catalog with
  its Published catalog habits "Reading", "Meditation" and "Sugar". "the earlier demo tasks run at T" means: the
  AC database is reset, then demo loading, demo history loading, demo report loading and demo pause loading run
  in that order at T with the demo password `example2026`. A demo account signs in with its email and
  `example2026`. The acceptance tests configure no default time zone, so the installation's default time zone is
  `UTC` and the run date R is the UTC date of the run time. A result of a demo task is written (email, text), or
  (text) for a result that names no account ("no demo password configured", "demo password does not meet the
  password rule").

## Coverage

| Requirement / scenario | AC items | Note |
|---|---|---|
| FR-001 | AC-001, AC-010, AC-019, AC-020 | |
| FR-002 | AC-001, AC-003, AC-006, AC-007, AC-008, AC-009, AC-017, AC-018, AC-070 | |
| FR-003 | AC-001, AC-010, AC-011, AC-012, AC-013, AC-014, AC-016, AC-021, AC-032, AC-100 | |
| FR-004 | AC-002, AC-014, AC-015, AC-016, AC-038, AC-045, AC-048, AC-065, AC-103 | |
| FR-005 | AC-001, AC-002, AC-003, AC-005, AC-007, AC-017, AC-020, AC-035, AC-037, AC-045, AC-048, AC-070, AC-083 | |
| FR-006 | AC-004, AC-008 | |
| FR-007 | AC-023, AC-028, AC-029, AC-030, AC-031, AC-035, AC-036, AC-042, AC-061 | |
| FR-008 | AC-023, AC-024, AC-025, AC-034, AC-035, AC-036, AC-039, AC-099 | |
| FR-009 | AC-033, AC-035, AC-037, AC-038 | The removal of excusals of dates after the last date is not checked: such a date belongs to no attempt, now or after a restart (a new attempt starts after the ending date), so whether its excusal was removed is not observable through the API. The records kept on such dates are not observable either (FR-016). |
| FR-010 | AC-026, AC-027, AC-032, AC-040, AC-049, AC-055 | |
| FR-011 | AC-035, AC-037, AC-038, AC-044, AC-045, AC-054, AC-055, AC-061, AC-104 | |
| FR-012 | AC-046, AC-048, AC-050, AC-051, AC-052, AC-053, AC-054, AC-062, AC-063, AC-064, AC-065, AC-066, AC-082, AC-104 | |
| FR-013 | AC-025, AC-046, AC-047, AC-055, AC-067, AC-069, AC-077, AC-097, AC-098 | |
| FR-014 | AC-042, AC-048, AC-049, AC-056, AC-057, AC-058, AC-060, AC-066, AC-068, AC-101, AC-102 | |
| FR-015 | AC-042, AC-075, AC-076, AC-087, AC-103 | Two ended attempts with the same ending date and the same name (the tie broken by the attempt number) are not checked: the spec does not say whether the number sorts up or down. |
| FR-016 | AC-023, AC-024, AC-041, AC-059, AC-067, AC-069, AC-079, AC-080, AC-096, AC-097, AC-098 | |
| FR-017 | AC-023, AC-046, AC-077 | |
| FR-018 | AC-033, AC-071 | "The order rules are unchanged" is not checked: the overview's order rule (slice 6 FR-011) is not restated in spec.md. |
| FR-019 | AC-041, AC-059, AC-072, AC-073, AC-074, AC-080 | |
| FR-020 | AC-078, AC-105, AC-106 | AC-105 and AC-106 check the other side of FR-020 (spec Edge Cases): a custom or catalog habit used only by an ended goal is still "used by goals", so the not-ended filter of the catalog counts does not apply there. |
| FR-021 | AC-081, AC-082, AC-083 | |
| FR-022 | AC-058, AC-084, AC-087, AC-104 | |
| FR-023 | AC-085, AC-086 | |
| FR-024 | AC-088 | The locked F001, F002, F004, F005, F007 and F008 scenarios are run unchanged as well. |
| FR-025 | AC-005, AC-026, AC-057, AC-060 | |
| FR-026 | AC-022, AC-043 | That nothing derived is stored is not observable through the API; AC-022 and AC-043 check that a change and an ending have no time of making of their own. |
| FR-027 | AC-041 | |
| FR-030 | AC-095 | The results and data of the earlier demo tasks are also checked by the locked F001, F004, F005, F007 and F008 scenarios, run unchanged (FR-024). |
| FR-031 | AC-089, AC-090, AC-091, AC-092, AC-093, AC-094 | The new account's name, custom habits and time zone are checked only through its figures, which assume `UTC`. The case-insensitive look-up of a catalog habit and of an existing email is not checked. That the results name no amount is checked for every demo amount: 0, 1, 5, 10, 15, 20 and 25 (AC-089). |
| US1-AS1 | AC-001 | |
| US1-AS2 | AC-002 | |
| US1-AS3 | AC-003 | |
| US1-AS4 | AC-004 | |
| US1-AS5 | AC-005 | |
| US1-AS6 | AC-006 | |
| US1-AS7 | AC-007, AC-008 | |
| US1-AS8 | AC-010 | |
| US1-AS9 | AC-011 | |
| US1-AS10 | AC-013 | |
| US1-AS11 | AC-014, AC-015 | |
| US1-AS12 | AC-017 | |
| US1-AS13 | AC-018 | |
| US1-AS14 | AC-019 | |
| US1-AS15 | — | Not testable through the API: the goal page's "Edit settings", the Edit settings page and its line, and the Settings card (page, HP-001, HP-002). |
| US2-AS1 | AC-023 | |
| US2-AS2 | AC-024 | |
| US2-AS3 | AC-025 | |
| US2-AS4 | AC-026 | |
| US2-AS5 | AC-027 | |
| US2-AS6 | AC-028 | |
| US2-AS7 | AC-029, AC-030 | |
| US2-AS8 | AC-031 | |
| US2-AS9 | AC-032 | |
| US2-AS10 | AC-033 | |
| US2-AS11 | AC-034 | The two records of the week 16–22 November are kept as stored facts, but no attempt contains their dates, so they are not observable through the API (FR-009, FR-016). |
| US2-AS12 | AC-035 | |
| US2-AS13 | — | Not testable through the API: the goal page's "End goal", the EndGoalDialog and the ended Notice (page, HP-001, HP-003). |
| US3-AS1 | AC-044 | |
| US3-AS2 | AC-046 | |
| US3-AS3 | AC-047 | |
| US3-AS4 | AC-048, AC-049, AC-097 | "Its next commitment is Monday 23 November" is checked through the evaluation calendar (AC-097) and the dates refused before it (AC-055, AC-069). |
| US3-AS5 | AC-050 | |
| US3-AS6 | AC-051, AC-052, AC-053 | |
| US3-AS7 | AC-054 | |
| US3-AS8 | AC-055 | |
| US3-AS9 | AC-056 | |
| US3-AS10 | AC-057 | |
| US3-AS11 | AC-058 | |
| US3-AS12 | AC-059, AC-060 | |
| US3-AS13 | AC-061, AC-062, AC-063 | |
| US3-AS14 | — | Not testable through the API: the Ended tab's and the goal page's "Start new attempt" and the Start new attempt page (page, HP-004, HP-005). Its start date is FR-011's, checked by AC-044. |
| US3-AS15 | — | Not testable through the API: the Earlier attempts card, the attempt view and its RecordDialog (page, HP-001, HP-014). The reads and records behind it are checked by AC-057 and AC-058. |
| US4-AS1 | AC-071 | |
| US4-AS2 | AC-072, AC-073 | |
| US4-AS3 | AC-075 | |
| US4-AS4 | AC-077 | |
| US4-AS5 | AC-078 | |
| US4-AS6 | — | Not testable through the API: the Goals page's Tabs, Ended items and empty text (page, HP-005). |
| US4-AS7 | — | Not testable through the API: the Profile's Goal history tab and its empty text (page, HP-006). Its order is checked by AC-075. |
| US4-AS8 | — | Not testable through the API: the goal page's Earlier attempts card (page, HP-001). Its data is checked by AC-056. |
| US5-AS1 | AC-084 | |
| US5-AS2 | AC-085 | |
| US5-AS3 | AC-086 | |
| US5-AS4 | AC-087 | |
| US5-AS5 | — | Not testable through the API: log lines, exception messages and error pages are host output (HP-010). |
| US6-AS1 | AC-089 | |
| US6-AS2 | AC-090, AC-091 | |
| US6-AS3 | AC-092 | |
| US6-AS4 | AC-093 | |
| US6-AS5 | AC-095 | |

Edge cases (spec "Edge Cases"), in the spec's order:

- A change on the day it is made: AC-001, AC-003, AC-007, AC-009.
- A change while a later version is waiting: AC-006 (a second change), AC-018 (merged with a time zone change, in
  both orders).
- A change to a goal or attempt that has not begun: AC-017 (goal), AC-070 (new attempt).
- Build to Reduce, or a change of measurement: AC-020.
- Ending on the start date, or before it: AC-036, AC-035.
- Ending a paused goal: AC-033.
- Ending a goal whose current commitment is excused or recorded: AC-039 (a record on the ending date is not
  counted). That the excusal of the ending date is removed is not observable (see FR-009).
- Pending results at the ending: AC-024, AC-026, AC-040, AC-072, AC-073.
- Ending and restarting on the same day: AC-051 row 06, AC-052 row 02.
- The account's today and the ending date differ: AC-061 to AC-063.
- Earlier attempt's Pending results after a restart: AC-059, AC-060, AC-080.
- A habit unpublished before a restart: AC-064.
- Restarting a per-week goal: AC-067, AC-098.
- Several attempts: AC-066.
- A time zone change on an ended goal: AC-081, AC-082.
- Ending a goal with a later version waiting: AC-037.
- Ending on the day a change took effect: AC-038.
- A habit unpublished, or a custom habit used only by ended goals, is still "used by goals": AC-105 (a custom
  habit used only by an ended goal cannot be deleted) and AC-106 (a catalog habit used only by an ended goal is
  not unpublished without confirmation). AC-106's habit is created by its own administrator, so no other goal
  uses it. The texts of those results (slice 4 FR-033, FR-035) are not restated in spec.md, so both items check
  the effect, not the text.
- Undoing an ending: there is no use case for it, so nothing can be called.

Success criteria: SC-002 is checked by AC-003, AC-005, AC-008. SC-003 by AC-024, AC-025, AC-034, AC-039, AC-099. SC-004 by
AC-046, AC-047, AC-057, AC-060. SC-005: a change just before and just after midnight and before and after a
per-week boundary (AC-009, AC-002), before a waiting version takes effect (AC-006, AC-018); an ending before the
start date (AC-035), on it (AC-036) and later (AC-023); a restart on the ending date and on the day after it
(AC-051 row 06, AC-052 row 02), also with the account's today a day behind the goal's timeline (AC-062, AC-063).
SC-006 on the AC surface: AC-084 to AC-087. SC-007's figures: AC-090, AC-091; the hosted demo and the smoke check
are outside the AC surface (HP-012). SC-001 and SC-008 are page and accessibility outcomes outside the AC surface
(HP-001 to HP-009, HP-014).

## Acceptance criteria

### AC-001 (FR-001; FR-002; FR-003; FR-005; US1-AS1) A change applies from the next date and keeps the name until then

- Given the registered user `f9.ac001.user@example.com`, who created the goal `Reading, 15 min daily`
- When at 2026-11-11T10:00:00Z `f9.ac001.user@example.com` changes the settings of the goal to Build, duration 20 minutes, fixed days on all seven days
- Then the result is "settings changed" with the date 2026-11-12
- And reading the goal gives the settings in effect Build, duration 15 minutes, fixed days on all seven days, from 2026-11-02
- And reading the goal gives the later settings Build, duration 20 minutes, fixed days on all seven days, from 2026-11-12
- And reading the goal gives the name `Reading, 15 min daily`

### AC-002 (FR-004; FR-005; US1-AS2) The changed settings and name take effect at midnight

- Given the registered user `f9.ac002.<n>.user@example.com`, who created the goal `Reading, 15 min daily`
- And at 2026-11-11T10:00:00Z `f9.ac002.<n>.user@example.com` changed the amount to 20 minutes
- When at <time> `f9.ac002.<n>.user@example.com` reads the goal
- Then its name is <name>
- And the settings in effect are <in effect>
- And it has <later>

Examples:

| n | time | name | in effect | later |
|---|---|---|---|---|
| 01 | 2026-11-11T23:59:59Z | `Reading, 15 min daily` | Build, duration 15 minutes, fixed days on all seven days, from 2026-11-02 | the later settings Build, duration 20 minutes, fixed days on all seven days, from 2026-11-12 |
| 02 | 2026-11-12T00:00:00Z | `Reading, 20 min daily` | Build, duration 20 minutes, fixed days on all seven days, from 2026-11-12 | no later settings |

### AC-003 (FR-002; FR-005; US1-AS3) Dates begun before a change keep the old target, later dates are judged by the new one

- Given the registered user `f9.ac003.user@example.com`, who created the goal `Reading, 15 min daily`
- And at 2026-11-11T09:00:00Z `f9.ac003.user@example.com` records 15 for each date from 2026-11-02 to 2026-11-11
- And at 2026-11-11T10:00:00Z `f9.ac003.user@example.com` changed the amount to 20 minutes
- And at 2026-11-16T09:00:00Z `f9.ac003.user@example.com` records 15 for each date from 2026-11-12 to 2026-11-15
- When at 2026-11-16T12:00:00Z `f9.ac003.user@example.com` reads the history of the goal from 2026-11-02 to 2026-11-16
- Then it gives 2026-11-02 to 2026-11-11 each Met, record 15
- And it gives 2026-11-12 Failed, record 15 and 2026-11-13 Failed, record 15
- And it gives 2026-11-14 Pending, record 15 and 2026-11-15 Pending, record 15
- And it gives 2026-11-16 Due, no record

### AC-004 (FR-006; US1-AS4) Consistency and streaks run across settings versions

- Given the registered user `f9.ac004.user@example.com`, who created the goal `Reading, 15 min daily`
- And at 2026-11-11T09:00:00Z `f9.ac004.user@example.com` records 15 for each date from 2026-11-02 to 2026-11-11
- And at 2026-11-11T10:00:00Z `f9.ac004.user@example.com` changed the amount to 20 minutes
- And at 2026-11-16T09:00:00Z `f9.ac004.user@example.com` records 15 for each date from 2026-11-12 to 2026-11-15
- When at 2026-11-16T12:00:00Z `f9.ac004.user@example.com` reads the report of the goal
- Then the consistency is "83% (10 of 12)"
- And the current streak is "0 commitments" and the longest streak is "10 commitments"

### AC-005 (FR-005; FR-025; US1-AS5) A correction is judged by the settings of its date

- Given the registered user `f9.ac005.<n>.user@example.com`, who created the goal `Reading, 15 min daily`
- And at 2026-11-11T09:00:00Z `f9.ac005.<n>.user@example.com` records 15 for each date from 2026-11-02 to 2026-11-11
- And at 2026-11-11T10:00:00Z `f9.ac005.<n>.user@example.com` changed the amount to 20 minutes
- And at 2026-11-16T09:00:00Z `f9.ac005.<n>.user@example.com` records 15 for each date from 2026-11-12 to 2026-11-15
- When at 2026-11-16T12:00:00Z `f9.ac005.<n>.user@example.com` changes the record of <date> to <value>
- Then the result is the state <state>, correction yes
- And the history of the goal from <date> to <date> gives <date> <state>, record <value>

Examples:

| n | date | value | state |
|---|---|---|---|
| 01 | 2026-11-09 | 12 | Failed |
| 02 | 2026-11-09 | 17 | Met |
| 03 | 2026-11-13 | 17 | Failed |

### AC-006 (FR-002; US1-AS6) A second change before the first takes effect replaces the waiting version

- Given the registered user `f9.ac006.user@example.com`, who created the goal `Reading, 15 min daily`
- And at 2026-11-11T10:00:00Z `f9.ac006.user@example.com` changed the amount to 20 minutes
- When at 2026-11-11T11:00:00Z `f9.ac006.user@example.com` changes the amount to 25 minutes
- Then the result is "settings changed" with the date 2026-11-12
- And reading the goal gives the settings in effect Build, duration 15 minutes, fixed days on all seven days, from 2026-11-02
- And reading the goal gives the later settings Build, duration 25 minutes, fixed days on all seven days, from 2026-11-12
- And at 2026-11-12T00:00:00Z reading the goal gives the name `Reading, 25 min daily` and no later settings

### AC-007 (FR-002; FR-005; US1-AS7) A per-week goal's change applies from the next Monday

- Given the registered user `f9.ac007.user@example.com`, who created the goal `Yoga, 3 times a week`
- When at 2026-11-18T10:00:00Z `f9.ac007.user@example.com` changes the settings of the goal to Build, yes/no, 4 times per week
- Then the result is "settings changed" with the date 2026-11-23
- And reading the goal gives the settings in effect Build, yes/no, 3 times per week, from 2026-11-02
- And reading the goal gives the later settings Build, yes/no, 4 times per week, from 2026-11-23

### AC-008 (FR-002; FR-006; US1-AS7) The week in progress keeps its target, the next week is judged by the new one

- Given the registered user `f9.ac008.user@example.com`, who created the goal `Yoga, 3 times a week`
- And at 2026-11-18T09:00:00Z `f9.ac008.user@example.com` records Yes for each of 2026-11-16, 2026-11-17 and 2026-11-18
- And at 2026-11-18T10:00:00Z `f9.ac008.user@example.com` changed the settings of the goal to Build, yes/no, 4 times per week
- And at 2026-11-25T09:00:00Z `f9.ac008.user@example.com` records Yes for each of 2026-11-23, 2026-11-24 and 2026-11-25
- When at 2026-12-02T00:00:00Z `f9.ac008.user@example.com` reads the history of the goal from 2026-11-16 to 2026-11-29
- Then it gives 2026-11-16 – 2026-11-22 Met, records Yes, Yes, Yes
- And it gives 2026-11-23 – 2026-11-29 Failed, records Yes, Yes, Yes

### AC-009 (FR-002) The next boundary just before and just after midnight and a week's end

- Given the registered user `f9.ac009.<n>.user@example.com`, who created the goal <goal>
- When at <time> `f9.ac009.<n>.user@example.com` changes the settings of the goal to <settings>
- Then the result is "settings changed" with the date <from>
- And reading the goal gives the later settings <settings>, from <from>

Examples:

| n | goal | time | settings | from |
|---|---|---|---|---|
| 01 | `Reading, 15 min daily` | 2026-11-11T23:59:59Z | Build, duration 20 minutes, fixed days on all seven days | 2026-11-12 |
| 02 | `Reading, 15 min daily` | 2026-11-12T00:00:00Z | Build, duration 20 minutes, fixed days on all seven days | 2026-11-13 |
| 03 | `Yoga, 3 times a week` | 2026-11-22T23:59:59Z | Build, yes/no, 4 times per week | 2026-11-23 |
| 04 | `Yoga, 3 times a week` | 2026-11-23T00:00:00Z | Build, yes/no, 4 times per week | 2026-11-30 |
| 05 | `Sugar, at most 1 sweet a day` | 2026-11-11T10:00:00Z | Reduce, count 2 `sweet`, 1 time per day | 2026-11-12 |

### AC-010 (FR-001; FR-003; US1-AS8) A change cannot switch the schedule kind

- Given the registered user `f9.ac010.<n>.user@example.com`, who created the goal <goal>
- When at 2026-11-11T10:00:00Z `f9.ac010.<n>.user@example.com` changes the settings of the goal to <settings>
- Then the result is "A change keeps the schedule kind: fixed days, times per day or times per week. End the goal and start a new attempt to change it."
- And reading the goal gives the settings in effect <current>, from 2026-11-02, and no later settings

Examples:

| n | goal | settings | current |
|---|---|---|---|
| 01 | `Reading, 15 min daily` | Build, duration 15 minutes, 5 times per week | Build, duration 15 minutes, fixed days on all seven days |
| 02 | `Reading, 15 min daily` | Build, duration 15 minutes, 1 time per day | Build, duration 15 minutes, fixed days on all seven days |
| 03 | `Yoga, 3 times a week` | Build, yes/no, fixed days on Monday, Wednesday and Friday | Build, yes/no, 3 times per week |
| 04 | `Yoga, 3 times a week` | Build, yes/no, 1 time per day | Build, yes/no, 3 times per week |
| 05 | `Sugar, at most 1 sweet a day` | Reduce, count 1 `sweet`, 1 time per week | Reduce, count 1 `sweet`, 1 time per day |

### AC-011 (FR-003; US1-AS9) A change with a value that goal creation refuses is refused with its message

- Given the registered user `f9.ac011.<n>.user@example.com`, who created the goal `Reading, 15 min daily`
- When at 2026-11-11T10:00:00Z `f9.ac011.<n>.user@example.com` changes the settings of the goal to <settings>
- Then the result is <message>
- And reading the goal gives the settings in effect Build, duration 15 minutes, fixed days on all seven days, from 2026-11-02, and no later settings

Examples:

| n | settings | message |
|---|---|---|
| 01 | no Build or Reduce, duration 15 minutes, fixed days on all seven days | "Choose Build or Reduce." |
| 02 | Build, no measurement, fixed days on all seven days | "Choose how it is measured." |
| 03 | Build, yes/no with the amount 5, fixed days on all seven days | "A yes/no goal has no amount or unit." |
| 04 | Build, duration 0 minutes, fixed days on all seven days | "Enter a whole number from 1 to 999." |
| 05 | Build, duration 1000 minutes, fixed days on all seven days | "Enter a whole number from 1 to 999." |
| 06 | Reduce, count 1000 `sweets`, fixed days on all seven days | "Enter a whole number from 0 to 999." |
| 07 | Build, count 20 with no unit, fixed days on all seven days | "Enter a unit, for example pages." |
| 08 | Build, count 20 `abcdefghijklmnopqrstu`, fixed days on all seven days | "The unit can have at most 20 characters." |
| 09 | Build, duration 15 with no time unit, fixed days on all seven days | "Choose minutes or hours." |
| 10 | Build, duration 15 minutes, no schedule | "Choose fixed days or how many times." |
| 11 | Build, duration 15 minutes, fixed days with no day | "Choose at least one day." |
| 12 | Build, duration 15 minutes, 21 times per day | "Enter a number of times from 1 to 20." |
| 13 | Build, duration 15 minutes, 3 times with no period | "Choose per day or per week." |
| 14 | Reduce, count 1 `sweet`, 2 times per day | "A Reduce goal with an amount has 1 time per period." |

### AC-012 (FR-003) A change gives the first refusal in the order of FR-003

- Given the registered user `f9.ac012.<n>.user@example.com`, who created the goal `Reading, 15 min daily`
- When at 2026-11-11T10:00:00Z `f9.ac012.<n>.user@example.com` changes the settings of the goal to <settings> with the name <name>
- Then the result is <message>
- And reading the goal gives the name `Reading, 15 min daily` and no later settings

Examples:

| n | settings | name | message |
|---|---|---|---|
| 01 | no Build or Reduce, no measurement, fixed days on all seven days | "" | "Choose Build or Reduce." |
| 02 | Build, yes/no with the amount 5, 21 times per day | "" | "A yes/no goal has no amount or unit." |
| 03 | Build, duration 1000 minutes, 5 times per week | "" | "Enter a whole number from 1 to 999." |
| 04 | Build, duration 15 minutes, 5 times per week | `x` repeated 61 times | "A change keeps the schedule kind: fixed days, times per day or times per week. End the goal and start a new attempt to change it." |
| 05 | Build, duration 20 minutes, fixed days on all seven days | `x` repeated 61 times | "The name can have at most 60 characters." |

### AC-013 (FR-003; US1-AS10) The same settings and name give "settings unchanged"

- Given the registered user `f9.ac013.user@example.com`, who created the goal `Reading, 15 min daily`
- When at 2026-11-11T10:00:00Z `f9.ac013.user@example.com` changes the settings of the goal to Build, duration 15 minutes, fixed days on all seven days
- Then the result is "settings unchanged"
- And reading the goal gives the settings in effect Build, duration 15 minutes, fixed days on all seven days, from 2026-11-02, and no later settings

### AC-014 (FR-003; FR-004; US1-AS11) A name-only change applies at once and adds no version

- Given the registered user `f9.ac014.user@example.com`, who created the goal `Reading, 15 min daily`
- When at 2026-11-11T10:00:00Z `f9.ac014.user@example.com` changes the settings of the goal to Build, duration 15 minutes, fixed days on all seven days with the name "Morning pages"
- Then the result is "settings changed" with no date
- And reading the goal gives the name "Morning pages", which is its own
- And reading the goal gives the settings in effect Build, duration 15 minutes, fixed days on all seven days, from 2026-11-02, and no later settings

### AC-015 (FR-004; US1-AS11) Clearing the name names the goal by its habit and target text again

- Given the registered user `f9.ac015.user@example.com`, who created the goal `Reading, 15 min daily`
- And at 2026-11-11T10:00:00Z `f9.ac015.user@example.com` changed the settings of the goal to Build, duration 15 minutes, fixed days on all seven days with the name "Morning pages"
- When at 2026-11-11T11:00:00Z `f9.ac015.user@example.com` changes the settings of the goal to Build, duration 15 minutes, fixed days on all seven days
- Then the result is "settings changed" with no date
- And reading the goal gives the name `Reading, 15 min daily`, which is not its own
- And reading the goal gives no later settings

### AC-016 (FR-003; FR-004) The name is trimmed, an empty name is none, and a name changes at once with the settings

- Given the registered user `f9.ac016.<n>.user@example.com`, who created the goal `Reading, 15 min daily`
- When at 2026-11-11T10:00:00Z `f9.ac016.<n>.user@example.com` changes the settings of the goal to <settings> with the name <given>
- Then the result is <result>
- And reading the goal gives the name <name>

Examples:

| n | settings | given | result | name |
|---|---|---|---|---|
| 01 | Build, duration 15 minutes, fixed days on all seven days | "  Morning pages  " | "settings changed" with no date | "Morning pages" |
| 02 | Build, duration 15 minutes, fixed days on all seven days | "   " | "settings unchanged" | `Reading, 15 min daily` |
| 03 | Build, duration 15 minutes, fixed days on all seven days | `x` repeated 60 times | "settings changed" with no date | `x` repeated 60 times |
| 04 | Build, duration 15 minutes, fixed days on all seven days | two spaces, `x` repeated 60 times, two spaces | "settings changed" with no date | `x` repeated 60 times |
| 05 | Build, duration 20 minutes, fixed days on all seven days | "Morning pages" | "settings changed" with the date 2026-11-12 | "Morning pages" |

### AC-017 (FR-002; FR-005; US1-AS12) A change to a goal that has not begun replaces its first version

- Given the registered user `f9.ac017.user@example.com`, who created the goal `Reading, 15 min daily` at 2026-11-16T08:00:00Z with the start date 2026-11-23
- When at 2026-11-17T10:00:00Z `f9.ac017.user@example.com` changes the amount to 30 minutes
- Then the result is "settings changed" with the date 2026-11-23
- And reading the goal gives the settings in effect Build, duration 30 minutes, fixed days on all seven days, from 2026-11-23, and no later settings
- And reading the goal gives the name `Reading, 30 min daily`

### AC-018 (FR-002; US1-AS13) A settings change and a time zone change merge into the one later version

- Given the registered user `f9.ac018.<n>.user@example.com`, who created the goal `Reading, 15 min daily`
- And at 2026-11-11T09:00:00Z `f9.ac018.<n>.user@example.com` <first>
- When at 2026-11-11T10:00:00Z `f9.ac018.<n>.user@example.com` <second>
- Then reading the goal gives the later settings Build, duration 20 minutes, fixed days on all seven days, time zone Europe/Kyiv, from 2026-11-12
- And reading the goal gives the settings in effect Build, duration 15 minutes, fixed days on all seven days, time zone UTC, from 2026-11-02

Examples:

| n | first | second |
|---|---|---|
| 01 | changed the account's time zone to `Europe/Kyiv` | changes the amount to 20 minutes |
| 02 | changed the amount to 20 minutes | changes the account's time zone to `Europe/Kyiv` |

### AC-019 (FR-001; US1-AS14) A pause does not block a change

- Given the registered user `f9.ac019.user@example.com`, who created the goal `Reading, 15 min daily`
- And at 2026-11-11T08:00:00Z `f9.ac019.user@example.com` records 15 for each date from 2026-11-02 to 2026-11-10
- And at 2026-11-11T09:00:00Z `f9.ac019.user@example.com` paused the goal from 2026-11-11 until 2026-11-13 without a reason
- When at 2026-11-11T10:00:00Z `f9.ac019.user@example.com` changes the amount to 20 minutes
- Then the result is "settings changed" with the date 2026-11-12
- And reading the goal gives the later settings Build, duration 20 minutes, fixed days on all seven days, from 2026-11-12

### AC-020 (FR-001; FR-005) A change of measurement or of Build and Reduce keeps each date's own target

- Given the registered user `f9.ac020.<n>.user@example.com`, who created the goal `Reading, 15 min daily`
- And at 2026-11-11T09:00:00Z `f9.ac020.<n>.user@example.com` records 12 for 2026-11-11
- And at 2026-11-11T10:00:00Z `f9.ac020.<n>.user@example.com` changed the settings of the goal to <settings>, which gave "settings changed" with the date 2026-11-12
- And at 2026-11-12T10:00:00Z `f9.ac020.<n>.user@example.com` records 12 for 2026-11-12
- When at 2026-11-15T00:00:00Z `f9.ac020.<n>.user@example.com` reads the history of the goal from 2026-11-11 to 2026-11-12
- Then it gives 2026-11-11 Failed, record 12
- And it gives 2026-11-12 Met, record 12

Examples:

| n | settings |
|---|---|
| 01 | Build, count 10 `pages`, fixed days on all seven days |
| 02 | Reduce, duration 20 minutes, fixed days on all seven days |

### AC-021 (FR-003) A value that does not belong to the chosen schedule is neither refused nor stored

- Given the registered user `f9.ac021.user@example.com`, who created the goal `Reading, 15 min daily`
- When at 2026-11-11T10:00:00Z `f9.ac021.user@example.com` changes the settings of the goal to Build, duration 20 minutes, fixed days on all seven days, with the number of times 21 and the period per week also given
- Then the result is "settings changed" with the date 2026-11-12
- And reading the goal gives the later settings Build, duration 20 minutes, fixed days on all seven days, from 2026-11-12

### AC-022 (FR-026) A settings change has no time of making of its own

- Given the registered user `f9.ac022.user@example.com`, who created the goal `Reading, 15 min daily`
- And at 2026-11-11T10:00:00Z `f9.ac022.user@example.com` changed the amount to 20 minutes
- When at 2026-11-11T09:00:00Z `f9.ac022.user@example.com` reads the goal
- Then it gives the later settings Build, duration 20 minutes, fixed days on all seven days, from 2026-11-12

### AC-023 (FR-007; FR-008; FR-016; FR-017; US2-AS1) Ending a goal stores the ending date and leaves the ending date out of the attempt

- Given the registered user `f9.ac023.user@example.com`, who created the goal `Meditation, 10 min daily`
- And at 2026-11-14T08:00:00Z `f9.ac023.user@example.com` records 10 for each date from 2026-11-02 to 2026-11-13
- When at 2026-11-16T10:00:00Z `f9.ac023.user@example.com` ends the goal with the outcome Abandoned and the note "Too busy this month."
- Then the result is "goal ended" with the ending date 2026-11-16 and the last date 2026-11-15
- And reading the goal gives it ended with the ending date 2026-11-16, the last date 2026-11-15, the outcome Abandoned and the note "Too busy this month."
- And reading the goal gives its latest attempt as attempt 1 with the start date 2026-11-02
- And the history of the goal from 2026-11-14 to 2026-11-16 gives exactly 2026-11-14 Pending and 2026-11-15 Pending

### AC-024 (FR-008; FR-016; US2-AS2) After ending, the Pending dates can still be recorded and the ending date cannot

- Given the registered user `f9.ac024.user@example.com`, who has the ended Meditation goal
- When at 2026-11-16T10:01:00Z `f9.ac024.user@example.com` reads what can be recorded now
- Then it contains (`Meditation, 10 min daily`, 2026-11-14, Pending, attempt 1, the latest attempt) and (`Meditation, 10 min daily`, 2026-11-15, Pending, attempt 1, the latest attempt)
- And it contains no entry of `Meditation, 10 min daily` for 2026-11-16

### AC-025 (FR-008; FR-013; US2-AS3) An ended attempt's Pending dates become Missed as usual and ending adds no failure

- Given the registered user `f9.ac025.user@example.com`, who has the ended Meditation goal
- When at 2026-11-18T12:00:00Z `f9.ac025.user@example.com` reads the report of the goal
- Then the consistency is "85% (12 of 14)"
- And the current streak is "0 commitments" and the longest streak is "12 commitments"
- And the history of the goal from 2026-11-14 to 2026-11-15 gives 2026-11-14 Missed, no record and 2026-11-15 Missed, no record

### AC-026 (FR-010; FR-025; US2-AS4) A correction of an ended attempt's date changes its figures

- Given the registered user `f9.ac026.user@example.com`, who has the ended Meditation goal
- When at 2026-11-18T12:00:00Z `f9.ac026.user@example.com` records 10 for 2026-11-14
- Then the result is the state Met, correction yes
- And the report of the goal gives the consistency "92% (13 of 14)"

### AC-027 (FR-010; US2-AS5) An ended goal has no commitment on or after its ending date

- Given the registered user `f9.ac027.<n>.user@example.com`, who has the ended Meditation goal
- When at <time> `f9.ac027.<n>.user@example.com` records 10 for <date>
- Then the result is "Nothing is scheduled on this date."

Examples:

| n | time | date |
|---|---|---|
| 01 | 2026-11-16T10:05:00Z | 2026-11-16 |
| 02 | 2026-11-18T12:00:00Z | 2026-11-17 |

### AC-028 (FR-007; US2-AS6) Ending without an outcome is refused

- Given the registered user `f9.ac028.user@example.com`, who created the goal `Meditation, 10 min daily`
- When at 2026-11-16T10:00:00Z `f9.ac028.user@example.com` ends the goal with no outcome and no note
- Then the result is "Choose how the goal ended."
- And reading the goal gives it not ended, with its latest attempt attempt 1
- And what can be recorded now contains (`Meditation, 10 min daily`, 2026-11-16, Due)

### AC-029 (FR-007; US2-AS7) An ending note longer than 500 characters is refused

- Given the registered user `f9.ac029.<n>.user@example.com`, who created the goal `Meditation, 10 min daily`
- When at 2026-11-16T10:00:00Z `f9.ac029.<n>.user@example.com` ends the goal with <outcome> and the note `x` repeated 501 times
- Then the result is <message>
- And reading the goal gives it not ended

Examples:

| n | outcome | message |
|---|---|---|
| 01 | the outcome Abandoned | "The note can have at most 500 characters." |
| 02 | no outcome | "Choose how the goal ended." |

### AC-030 (FR-007; US2-AS7) An ending note of 500 characters is accepted, and the note is trimmed

- Given the registered user `f9.ac030.<n>.user@example.com`, who created the goal `Meditation, 10 min daily`
- When at 2026-11-16T10:00:00Z `f9.ac030.<n>.user@example.com` ends the goal with the outcome <outcome> and the note <given>
- Then the result is "goal ended" with the ending date 2026-11-16 and the last date 2026-11-15
- And reading the goal gives it ended with the outcome <outcome> and <stored>

Examples:

| n | outcome | given | stored |
|---|---|---|---|
| 01 | Achieved | `x` repeated 500 times | the note `x` repeated 500 times |
| 02 | No longer relevant | "  Done.  " | the note "Done." |
| 03 | Abandoned | "   " | no note |

### AC-031 (FR-007; US2-AS8) An ended goal cannot be ended again

- Given the registered user `f9.ac031.<n>.user@example.com`, who has the ended Meditation goal
- When at 2026-11-17T10:00:00Z `f9.ac031.<n>.user@example.com` ends the goal with <outcome> and no note
- Then the result is "This goal has already ended."
- And reading the goal gives it ended with the ending date 2026-11-16, the outcome Abandoned and the note "Too busy this month."

Examples:

| n | outcome |
|---|---|
| 01 | the outcome Achieved |
| 02 | no outcome |

### AC-032 (FR-003; FR-010; US2-AS9) An ended goal cannot be changed or paused

- Given the registered user `f9.ac032.<n>.user@example.com`, who has the ended Meditation goal
- When at 2026-11-17T10:00:00Z `f9.ac032.<n>.user@example.com` <action>
- Then the result is "This goal has ended."
- And reading the goal gives the settings in effect Build, duration 10 minutes, fixed days on all seven days, from 2026-11-02, and no later settings
- And the pauses of the goal are none

Examples:

| n | action |
|---|---|
| 01 | changes the settings of the goal to Build, duration 20 minutes, fixed days on all seven days |
| 02 | changes the settings of the goal to Build, duration 0 minutes, fixed days on all seven days |
| 03 | pauses the goal from 2026-11-18 until 2026-11-20 without a reason |

### AC-033 (FR-009; FR-018; US2-AS10) Ending cuts the current pause to the last date and removes upcoming pauses

- Given the registered user `f9.ac033.user@example.com`, who created the goal `Meditation, 10 min daily`
- And at 2026-11-14T07:00:00Z `f9.ac033.user@example.com` records 10 for each date from 2026-11-02 to 2026-11-13
- And at 2026-11-14T08:00:00Z `f9.ac033.user@example.com` paused the goal from 2026-11-14 until 2026-11-20 without a reason
- And at 2026-11-14T08:01:00Z `f9.ac033.user@example.com` paused the goal from 2026-11-23 until 2026-11-25 without a reason
- When at 2026-11-16T10:00:00Z `f9.ac033.user@example.com` ends the goal with the outcome Abandoned and no note
- Then the result is "goal ended" with the ending date 2026-11-16 and the last date 2026-11-15
- And the pauses of the goal are exactly (2026-11-14, 2026-11-15, no reason, past)
- And the goals overview gives `Meditation, 10 min daily` ended on 2026-11-16 with the outcome Abandoned, not paused

### AC-034 (FR-008; US2-AS11) Ending a per-week goal mid-week leaves the week in progress out of the attempt

- Given the registered user `f9.ac034.user@example.com`, who created the goal `Yoga, 3 times a week`
- And at 2026-11-14T08:00:00Z `f9.ac034.user@example.com` records Yes for each of 2026-11-02, 2026-11-04 and 2026-11-06
- And at 2026-11-14T08:01:00Z `f9.ac034.user@example.com` records Yes for each of 2026-11-09, 2026-11-11 and 2026-11-13
- And at 2026-11-17T08:00:00Z `f9.ac034.user@example.com` records Yes for 2026-11-16
- And at 2026-11-17T08:01:00Z `f9.ac034.user@example.com` records Yes for 2026-11-17
- When at 2026-11-18T10:00:00Z `f9.ac034.user@example.com` ends the goal with the outcome No longer relevant and no note
- Then the result is "goal ended" with the ending date 2026-11-18 and the last date 2026-11-15
- And at 2026-11-25T00:00:00Z the report of the goal gives the consistency "100% (2 of 2)", the current streak "2 weeks" and the longest streak "2 weeks"
- And at 2026-11-25T00:00:00Z the history of the goal from 2026-11-16 to 2026-11-22 gives no entry

### AC-035 (FR-005; FR-007; FR-008; FR-009; FR-011; US2-AS12) A goal ended before its start date keeps its first version

- Given the registered user `f9.ac035.user@example.com`, who created the goal `Meditation, 10 min daily` at 2026-11-16T08:00:00Z with the start date 2026-11-23
- When at 2026-11-17T10:00:00Z `f9.ac035.user@example.com` ends the goal with the outcome No longer relevant and no note
- Then the result is "goal ended" with the ending date 2026-11-17 and the last date 2026-11-16
- And the report of the goal gives the consistency "N/A"
- And reading the goal gives the name `Meditation, 10 min daily`
- And reading the goal gives the settings in effect Build, duration 10 minutes, fixed days on all seven days, from 2026-11-23, and no later settings
- And the attempts of the goal are exactly attempt 1, `Meditation, 10 min daily`, start date 2026-11-23, ending date 2026-11-17, last date 2026-11-16, outcome No longer relevant, no note, consistency "N/A"
- And at 2026-11-17T10:01:00Z the starting values of a new attempt are Build, duration 10 minutes, fixed days on all seven days and the start date 2026-11-18

### AC-036 (FR-007; FR-008) A goal ended on its start date has no commitment

- Given the registered user `f9.ac036.user@example.com`, who created the goal `Meditation, 10 min daily` at 2026-11-16T08:00:00Z with the start date 2026-11-16
- When at 2026-11-16T10:00:00Z `f9.ac036.user@example.com` ends the goal with the outcome Abandoned and no note
- Then the result is "goal ended" with the ending date 2026-11-16 and the last date 2026-11-15
- And the report of the goal gives the consistency "N/A"
- And what can be recorded now contains no entry of `Meditation, 10 min daily`
- And the history of the goal from 2026-11-16 to 2026-11-16 gives no entry

### AC-037 (FR-005; FR-009; FR-011) Ending removes a waiting later version

- Given the registered user `f9.ac037.user@example.com`, who created the goal `Meditation, 10 min daily`
- And at 2026-11-16T09:00:00Z `f9.ac037.user@example.com` changed the amount to 20 minutes, which gave "settings changed" with the date 2026-11-17
- When at 2026-11-16T10:00:00Z `f9.ac037.user@example.com` ends the goal with the outcome Abandoned and no note
- Then the result is "goal ended" with the ending date 2026-11-16 and the last date 2026-11-15
- And reading the goal gives the settings in effect Build, duration 10 minutes, fixed days on all seven days, from 2026-11-02, and no later settings
- And reading the goal gives the name `Meditation, 10 min daily`
- And the starting values of a new attempt are Build, duration 10 minutes, fixed days on all seven days

### AC-038 (FR-004; FR-009; FR-011) Ending removes a version that took effect on the ending date or in the week in progress

- Given the registered user `f9.ac038.<n>.user@example.com`, who created the goal <goal>
- And at <change time> `f9.ac038.<n>.user@example.com` changed the settings of the goal to <changed>, which gave "settings changed" with the date <from>
- When at <end time> `f9.ac038.<n>.user@example.com` ends the goal with the outcome Abandoned and no note
- Then the result is "goal ended" with the ending date <ending> and the last date 2026-11-15
- And reading the goal gives the name <goal>
- And reading the goal gives the settings in effect <settings>, from 2026-11-02, and no later settings
- And the starting values of a new attempt are <settings> and the start date <start>

Examples:

| n | goal | change time | changed | from | end time | ending | settings | start |
|---|---|---|---|---|---|---|---|---|
| 01 | `Meditation, 10 min daily` | 2026-11-15T10:00:00Z | Build, duration 20 minutes, fixed days on all seven days | 2026-11-16 | 2026-11-16T10:00:00Z | 2026-11-16 | Build, duration 10 minutes, fixed days on all seven days | 2026-11-17 |
| 02 | `Yoga, 3 times a week` | 2026-11-11T10:00:00Z | Build, yes/no, 4 times per week | 2026-11-16 | 2026-11-18T10:00:00Z | 2026-11-18 | Build, yes/no, 3 times per week | 2026-11-19 |

### AC-039 (FR-008) A record on the ending date is not counted

- Given the registered user `f9.ac039.user@example.com`, who created the goal `Meditation, 10 min daily`
- And at 2026-11-14T08:00:00Z `f9.ac039.user@example.com` records 10 for each date from 2026-11-02 to 2026-11-13
- And at 2026-11-16T09:00:00Z `f9.ac039.user@example.com` records 10 for 2026-11-16
- When at 2026-11-16T10:00:00Z `f9.ac039.user@example.com` ends the goal with the outcome Abandoned and no note
- Then the result is "goal ended" with the ending date 2026-11-16 and the last date 2026-11-15
- And at 2026-11-19T12:00:00Z the report of the goal gives the consistency "85% (12 of 14)"
- And at 2026-11-19T12:00:00Z the history of the goal from 2026-11-16 to 2026-11-16 gives no entry

### AC-040 (FR-010) Excusals and records of an ended attempt's dates keep working

- Given the registered user `f9.ac040.<n>.user@example.com`, who has the ended Meditation goal
- And at 2026-11-16T10:02:00Z `f9.ac040.<n>.user@example.com` excused 2026-11-14 without a reason
- When at 2026-11-16T10:05:00Z `f9.ac040.<n>.user@example.com` <action>
- Then it succeeds
- And the history of the goal from <date> to <date> gives <date> <entry>

Examples:

| n | action | date | entry |
|---|---|---|---|
| 01 | excuses 2026-11-15 without a reason | 2026-11-15 | Excused |
| 02 | removes the excusal of 2026-11-14 | 2026-11-14 | Pending, no record |
| 03 | changes the record of 2026-11-12 to 5 | 2026-11-12 | Failed, record 5 |
| 04 | deletes the record of 2026-11-13 | 2026-11-13 | Missed, no record |

### AC-041 (FR-016; FR-019; FR-027) An ending note is not a note of a commitment, and an ended goal's notes leave the dashboard

- Given the registered user `f9.ac041.user@example.com`, who created the goal `Meditation, 10 min daily`
- And at 2026-11-14T08:00:00Z `f9.ac041.user@example.com` records 10 for each date from 2026-11-02 to 2026-11-13
- And at 2026-11-14T08:05:00Z `f9.ac041.user@example.com` saved the note "Calm" for 2026-11-13
- And at 2026-11-16T10:00:00Z `f9.ac041.user@example.com` ended the goal with the outcome Abandoned and the note "Too busy this month."
- When at 2026-11-16T10:01:00Z `f9.ac041.user@example.com` reads the recent notes of the goal
- Then they are exactly (2026-11-13, "Calm", Met)
- And no entry of the history of the goal from 2026-11-02 to 2026-11-15 carries the note "Too busy this month."
- And the dashboard's recent notes contain no entry of `Meditation, 10 min daily`

### AC-042 (FR-007; FR-014; FR-015) A goal whose new attempt has not begun can be ended

- Given the registered user `f9.ac042.user@example.com`, who has the ended Meditation goal
- And at 2026-11-18T08:00:00Z `f9.ac042.user@example.com` started a new attempt with Build, duration 5 minutes, fixed days on all seven days and the start date 2026-11-23, which gave "attempt started" with the number 2
- When at 2026-11-19T10:00:00Z `f9.ac042.user@example.com` ends the goal with the outcome No longer relevant and no note
- Then the result is "goal ended" with the ending date 2026-11-19 and the last date 2026-11-18
- And the attempts of the goal are, in this order, attempt 2, `Meditation, 5 min daily`, start date 2026-11-23, ending date 2026-11-19, last date 2026-11-18, outcome No longer relevant, no note, consistency "N/A"; and attempt 1, `Meditation, 10 min daily`, start date 2026-11-02, ending date 2026-11-16, last date 2026-11-15, outcome Abandoned, note "Too busy this month."
- And the goal history is, in this order, (`Meditation, 5 min daily`, attempt 2, ending date 2026-11-19, No longer relevant, no note, the goal's latest attempt) and (`Meditation, 10 min daily`, attempt 1, ending date 2026-11-16, Abandoned, note "Too busy this month.", not the goal's latest attempt)

### AC-043 (FR-026) An ending has no time of making of its own

- Given the registered user `f9.ac043.user@example.com`, who has the ended Meditation goal
- When at 2026-11-16T09:00:00Z `f9.ac043.user@example.com` reads the goal
- Then it gives it ended with the ending date 2026-11-16, the last date 2026-11-15 and the outcome Abandoned

### AC-044 (FR-011; US3-AS1) The starting values of a new attempt copy the last settings and start after the ending date

- Given the registered user `f9.ac044.<n>.user@example.com`, who has the ended Meditation goal
- When at <time> `f9.ac044.<n>.user@example.com` reads the starting values of a new attempt
- Then they are Build, duration 10 minutes, fixed days on all seven days and the start date <start>

Examples:

| n | time | start |
|---|---|---|
| 01 | 2026-11-18T10:00:00Z | 2026-11-18 |
| 02 | 2026-11-16T11:00:00Z | 2026-11-17 |

### AC-045 (FR-004; FR-005; FR-011) An ended goal is named and read by its last version, which also gives the starting values

- Given the registered user `f9.ac045.user@example.com`, who created the goal `Meditation, 10 min daily`
- And at 2026-11-11T10:00:00Z `f9.ac045.user@example.com` changed the amount to 20 minutes, which gave "settings changed" with the date 2026-11-12
- And at 2026-11-16T10:00:00Z `f9.ac045.user@example.com` ended the goal with the outcome Abandoned and no note
- When at 2026-11-18T10:00:00Z `f9.ac045.user@example.com` reads the starting values of a new attempt
- Then they are Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
- And reading the goal gives the name `Meditation, 20 min daily`
- And reading the goal gives the settings in effect Build, duration 20 minutes, fixed days on all seven days, from 2026-11-12, and no later settings

### AC-046 (FR-012; FR-013; FR-017; US3-AS2) Starting a new attempt with the starting values

- Given the registered user `f9.ac046.user@example.com`, who has the ended Meditation goal
- When at 2026-11-18T08:00:00Z `f9.ac046.user@example.com` starts a new attempt with Build, duration 10 minutes, fixed days on all seven days and the start date 2026-11-18
- Then the result is "attempt started" with the number 2 and the start date 2026-11-18
- And reading the goal gives it not ended, its latest attempt as attempt 2 with the start date 2026-11-18, and the goal's start date 2026-11-02
- And reading the goal gives the name `Meditation, 10 min daily`
- And the report of the goal gives the consistency "N/A", the current streak "0 commitments" and the longest streak "0 commitments"

### AC-047 (FR-013; US3-AS3) A new attempt's figures count only its own results

- Given the registered user `f9.ac047.user@example.com`, who has the ended Meditation goal
- And at 2026-11-18T08:00:00Z `f9.ac047.user@example.com` started a new attempt with Build, duration 10 minutes, fixed days on all seven days and the start date 2026-11-18
- And at 2026-11-20T10:00:00Z `f9.ac047.user@example.com` records 10 for each date from 2026-11-18 to 2026-11-20
- When at 2026-11-23T12:00:00Z `f9.ac047.user@example.com` reads the report of the goal
- Then the consistency is "100% (3 of 3)"
- And the current streak is "3 commitments" and the longest streak is "3 commitments"

### AC-048 (FR-004; FR-005; FR-012; FR-014; US3-AS4) A new attempt with new settings that starts later

- Given the registered user `f9.ac048.user@example.com`, who has the ended Meditation goal
- When at 2026-11-18T08:00:00Z `f9.ac048.user@example.com` starts a new attempt with Build, duration 5 minutes, fixed days on all seven days and the start date 2026-11-23
- Then the result is "attempt started" with the number 2 and the start date 2026-11-23
- And reading the goal gives it not ended and its latest attempt as attempt 2 with the start date 2026-11-23
- And reading the goal gives the name `Meditation, 5 min daily`
- And reading the goal gives the settings in effect Build, duration 5 minutes, fixed days on all seven days, from 2026-11-23, and no later settings
- And at 2026-11-19T10:00:00Z the attempts of the goal are, in this order, attempt 2, `Meditation, 5 min daily`, start date 2026-11-23, not ended; and attempt 1, `Meditation, 10 min daily`, start date 2026-11-02, ending date 2026-11-16

### AC-049 (FR-010; FR-014; US3-AS4) An earlier attempt's dates can be recorded while the new attempt waits to begin

- Given the registered user `f9.ac049.user@example.com`, who has the ended Meditation goal
- And at 2026-11-18T08:00:00Z `f9.ac049.user@example.com` started a new attempt with Build, duration 5 minutes, fixed days on all seven days and the start date 2026-11-23
- When at 2026-11-19T10:00:00Z `f9.ac049.user@example.com` records 10 for 2026-11-15
- Then the result is the state Met, correction yes
- And the attempts of the goal give attempt 1 the consistency "92% (13 of 14)"

### AC-050 (FR-012; US3-AS5) A new attempt can change the schedule kind

- Given the registered user `f9.ac050.user@example.com`, who has the ended Meditation goal
- When at 2026-11-18T08:00:00Z `f9.ac050.user@example.com` starts a new attempt with Build, duration 10 minutes, 3 times per week and the start date 2026-11-18
- Then the result is "attempt started" with the number 2 and the start date 2026-11-18
- And the report of the goal gives the current streak "0 weeks"

### AC-051 (FR-012; US3-AS6) A start date in the past, on or before the ending date, or more than a year ahead is refused

- Given the registered user `f9.ac051.<n>.user@example.com`, who has the ended Meditation goal
- When at <time> `f9.ac051.<n>.user@example.com` starts a new attempt with Build, duration 10 minutes, fixed days on all seven days and <date>
- Then the result is <message>
- And reading the goal gives it ended, with its latest attempt attempt 1

Examples:

| n | time | date | message |
|---|---|---|---|
| 01 | 2026-11-18T08:00:00Z | no start date | "Choose a start date." |
| 02 | 2026-11-18T08:00:00Z | the start date 2026-11-17 | "The start date cannot be in the past." |
| 03 | 2026-11-18T08:00:00Z | the start date 2026-11-16 | "The start date cannot be in the past." |
| 04 | 2026-11-18T08:00:00Z | the start date 2027-11-19 | "Choose a start date within a year from today." |
| 05 | 2026-11-18T08:00:00Z | the start date 2027-11-20 | "Choose a start date within a year from today." |
| 06 | 2026-11-16T11:00:00Z | the start date 2026-11-16 | "A new attempt starts after the day the goal ended." |
| 07 | 2026-11-16T11:00:00Z | the start date 2026-11-15 | "The start date cannot be in the past." |

### AC-052 (FR-012; US3-AS6) The day after the ending date and a date 365 days ahead are accepted

- Given the registered user `f9.ac052.<n>.user@example.com`, who has the ended Meditation goal
- When at <time> `f9.ac052.<n>.user@example.com` starts a new attempt with Build, duration 10 minutes, fixed days on all seven days and the start date <date>
- Then the result is "attempt started" with the number 2 and the start date <date>

Examples:

| n | time | date |
|---|---|---|
| 01 | 2026-11-18T08:00:00Z | 2027-11-18 |
| 02 | 2026-11-16T11:00:00Z | 2026-11-17 |
| 03 | 2026-11-18T08:00:00Z | 2026-11-18 |

### AC-053 (FR-012; US3-AS6) A new attempt with a value that goal creation refuses is refused with its message

- Given the registered user `f9.ac053.<n>.user@example.com`, who has the ended Meditation goal
- When at 2026-11-18T08:00:00Z `f9.ac053.<n>.user@example.com` starts a new attempt with <settings> and the start date <date>
- Then the result is <message>
- And reading the goal gives it ended, with its latest attempt attempt 1

Examples:

| n | settings | date | message |
|---|---|---|---|
| 01 | no Build or Reduce, duration 10 minutes, fixed days on all seven days | 2026-11-18 | "Choose Build or Reduce." |
| 02 | Build, no measurement, fixed days on all seven days | 2026-11-18 | "Choose how it is measured." |
| 03 | Build, yes/no with the amount 5, fixed days on all seven days | 2026-11-18 | "A yes/no goal has no amount or unit." |
| 04 | Build, duration 0 minutes, fixed days on all seven days | 2026-11-18 | "Enter a whole number from 1 to 999." |
| 05 | Build, duration 1000 minutes, fixed days on all seven days | 2026-11-18 | "Enter a whole number from 1 to 999." |
| 06 | Reduce, count 1000 `sweets`, fixed days on all seven days | 2026-11-18 | "Enter a whole number from 0 to 999." |
| 07 | Build, count 20 with no unit, fixed days on all seven days | 2026-11-18 | "Enter a unit, for example pages." |
| 08 | Build, count 20 `abcdefghijklmnopqrstu`, fixed days on all seven days | 2026-11-18 | "The unit can have at most 20 characters." |
| 09 | Build, duration 10 with no time unit, fixed days on all seven days | 2026-11-18 | "Choose minutes or hours." |
| 10 | Build, duration 10 minutes, no schedule | 2026-11-18 | "Choose fixed days or how many times." |
| 11 | Build, duration 10 minutes, fixed days with no day | 2026-11-18 | "Choose at least one day." |
| 12 | Build, duration 10 minutes, 21 times per day | 2026-11-18 | "Enter a number of times from 1 to 20." |
| 13 | Build, duration 10 minutes, 3 times with no period | 2026-11-18 | "Choose per day or per week." |
| 14 | Reduce, count 1 `sweet`, 2 times per day | 2026-11-18 | "A Reduce goal with an amount has 1 time per period." |
| 15 | no Build or Reduce, duration 10 minutes, fixed days on all seven days | 2026-11-16 | "Choose Build or Reduce." |

### AC-054 (FR-011; FR-012; US3-AS7) Only an ended goal can start a new attempt

- Given the registered user `f9.ac054.<n>.user@example.com`, who created the goal `Meditation, 10 min daily`
- When at 2026-11-16T10:00:00Z `f9.ac054.<n>.user@example.com` <action>
- Then the result is "Only an ended goal can start a new attempt."
- And reading the goal gives it not ended, with its latest attempt attempt 1

Examples:

| n | action |
|---|---|
| 01 | starts a new attempt with Build, duration 10 minutes, fixed days on all seven days and the start date 2026-11-17 |
| 02 | reads the starting values of a new attempt |
| 03 | starts a new attempt with no Build or Reduce, duration 10 minutes, fixed days on all seven days and the start date 2026-11-17 |

### AC-055 (FR-010; FR-011; FR-013; US3-AS8) Dates between two attempts belong to none

- Given the registered user `f9.ac055.<n>.user@example.com`, who has the ended Meditation goal
- And at 2026-11-18T08:00:00Z `f9.ac055.<n>.user@example.com` started a new attempt with Build, duration 5 minutes, fixed days on all seven days and the start date 2026-11-23
- When at 2026-11-24T10:00:00Z `f9.ac055.<n>.user@example.com` records 10 for <date>
- Then the result is "Nothing is scheduled on this date."

Examples:

| n | date |
|---|---|
| 01 | 2026-11-17 |
| 02 | 2026-11-20 |
| 03 | 2026-11-22 |

### AC-056 (FR-014; US3-AS9) Reading a goal's attempts gives every attempt, the latest first

- Given the registered user `f9.ac056.user@example.com`, who has the ended Meditation goal
- And at 2026-11-18T08:00:00Z `f9.ac056.user@example.com` started a new attempt with Build, duration 10 minutes, fixed days on all seven days and the start date 2026-11-18
- When at 2026-11-23T12:00:00Z `f9.ac056.user@example.com` reads the attempts of the goal
- Then they are exactly, in this order, attempt 2 and attempt 1
- And attempt 2 is `Meditation, 10 min daily`, start date 2026-11-18, not ended
- And attempt 1 is `Meditation, 10 min daily`, start date 2026-11-02, ending date 2026-11-16, last date 2026-11-15, outcome Abandoned, note "Too busy this month.", consistency "85% (12 of 14)", current streak "0 commitments", longest streak "12 commitments"

### AC-057 (FR-014; FR-025; US3-AS10) Correcting an earlier attempt changes only that attempt's figures

- Given the registered user `f9.ac057.user@example.com`, who has the ended Meditation goal
- And at 2026-11-18T08:00:00Z `f9.ac057.user@example.com` started a new attempt with Build, duration 10 minutes, fixed days on all seven days and the start date 2026-11-18
- And at 2026-11-20T10:00:00Z `f9.ac057.user@example.com` records 10 for each date from 2026-11-18 to 2026-11-20
- When at 2026-11-23T12:00:00Z `f9.ac057.user@example.com` records 10 for 2026-11-15
- Then the result is the state Met, correction yes
- And the attempts of the goal give attempt 1 the consistency "92% (13 of 14)"
- And the attempts of the goal give attempt 2 the consistency "100% (3 of 3)", the current streak "3 commitments" and the longest streak "3 commitments"

### AC-058 (FR-014; FR-022; US3-AS11) Reading one earlier attempt gives its History and recent results

- Given the registered user `f9.ac058.user@example.com`, who has the ended Meditation goal
- And at 2026-11-18T08:00:00Z `f9.ac058.user@example.com` started a new attempt with Build, duration 10 minutes, fixed days on all seven days and the start date 2026-11-18
- When at 2026-11-23T12:00:00Z `f9.ac058.user@example.com` reads attempt 1 of the goal
- Then it gives attempt 1, `Meditation, 10 min daily`, start date 2026-11-02, ending date 2026-11-16, last date 2026-11-15, outcome Abandoned, note "Too busy this month."
- And its History has exactly 14 entries, newest first: 2026-11-15 Missed, no record; 2026-11-14 Missed, no record; then 2026-11-13 back to 2026-11-02, each Met, record 10
- And its recent results are exactly 14 results, oldest first: 2026-11-02 to 2026-11-13 each Met, then 2026-11-14 Missed and 2026-11-15 Missed
- And reading attempt 3 of the goal gives "This attempt does not exist."

### AC-059 (FR-016; FR-019; US3-AS12) After a restart in the same week, the earlier attempt's Pending date stays recordable and its Missed date is not listed

- Given the registered user `f9.ac059.user@example.com`, who has the ended Meditation goal
- And at 2026-11-17T09:00:00Z `f9.ac059.user@example.com` started a new attempt with Build, duration 10 minutes, fixed days on all seven days and the start date 2026-11-17
- When at 2026-11-17T10:00:00Z `f9.ac059.user@example.com` reads what can be recorded now
- Then it contains (`Meditation, 10 min daily`, 2026-11-15, Pending, attempt 1, not the latest attempt, Build) and (`Meditation, 10 min daily`, 2026-11-17, Due)
- And it contains no entry for 2026-11-14
- And the dashboard's Today list contains (`Meditation, 10 min daily`, 2026-11-15, Pending, attempt 1, not the latest attempt, goal not ended, Build) and (`Meditation, 10 min daily`, 2026-11-17, Due)
- And the dashboard's Today list contains no entry for 2026-11-14
- And the dashboard gives the Pending count 1, the Due count 1 and the Missed count 0

### AC-060 (FR-014; FR-025; US3-AS12) Recording the earlier attempt's Pending date counts in that attempt only

- Given the registered user `f9.ac060.user@example.com`, who has the ended Meditation goal
- And at 2026-11-17T09:00:00Z `f9.ac060.user@example.com` started a new attempt with Build, duration 10 minutes, fixed days on all seven days and the start date 2026-11-17
- When at 2026-11-17T10:05:00Z `f9.ac060.user@example.com` records 10 for 2026-11-15
- Then the result is the state Pending, correction no
- And at 2026-11-18T00:00:00Z the attempts of the goal give attempt 1 the consistency "92% (13 of 14)" and attempt 2 the consistency "N/A"

### AC-061 (FR-007; FR-011; US3-AS13) Ending on the goal's timeline a day ahead of the account's today

- Given the registered user `f9.ac061.user@example.com` in `Europe/Kyiv`, who created the goal `Meditation, 10 min daily`
- And at 2026-11-16T23:30:00Z `f9.ac061.user@example.com` changed the account's time zone to `UTC`
- When at 2026-11-16T23:40:00Z `f9.ac061.user@example.com` ends the goal with the outcome Abandoned and no note
- Then the result is "goal ended" with the ending date 2026-11-17 and the last date 2026-11-16
- And reading the goal gives the settings in effect Build, duration 10 minutes, fixed days on all seven days, time zone Europe/Kyiv, from 2026-11-02, and no later settings
- And at 2026-11-16T23:41:00Z the starting values of a new attempt are Build, duration 10 minutes, fixed days on all seven days and the start date 2026-11-18

### AC-062 (FR-012; US3-AS13) A new attempt cannot start on the ending date when the account's today is a day behind

- Given the registered user `f9.ac062.<n>.user@example.com` in `Europe/Kyiv`, who created the goal `Meditation, 10 min daily`
- And at 2026-11-16T23:30:00Z `f9.ac062.<n>.user@example.com` changed the account's time zone to `UTC`
- And at 2026-11-16T23:40:00Z `f9.ac062.<n>.user@example.com` ended the goal with the outcome Abandoned and no note
- When at 2026-11-16T23:42:00Z `f9.ac062.<n>.user@example.com` starts a new attempt with Build, duration 10 minutes, fixed days on all seven days and the start date <date>
- Then the result is <message>
- And reading the goal gives it ended, with its latest attempt attempt 1

Examples:

| n | date | message |
|---|---|---|
| 01 | 2026-11-17 | "A new attempt starts after the day the goal ended." |
| 02 | 2026-11-16 | "A new attempt starts after the day the goal ended." |
| 03 | 2026-11-15 | "The start date cannot be in the past." |

### AC-063 (FR-012; US3-AS13) The day after the ending date is accepted while the account's today is a day behind

- Given the registered user `f9.ac063.user@example.com` in `Europe/Kyiv`, who created the goal `Meditation, 10 min daily`
- And at 2026-11-16T23:30:00Z `f9.ac063.user@example.com` changed the account's time zone to `UTC`
- And at 2026-11-16T23:40:00Z `f9.ac063.user@example.com` ended the goal with the outcome Abandoned and no note
- When at 2026-11-16T23:42:00Z `f9.ac063.user@example.com` starts a new attempt with Build, duration 10 minutes, fixed days on all seven days and the start date 2026-11-18
- Then the result is "attempt started" with the number 2 and the start date 2026-11-18

### AC-064 (FR-012) A new attempt can start on an Unpublished catalog habit

- Given the catalog is seeded at 2026-11-01T00:00:00Z
- And the registered user `f9.ac064.user@example.com`, who at 2026-11-02T08:00:00Z created a goal on the catalog habit "Meditation" with Build, duration 10 minutes, fixed days on all seven days, the start date 2026-11-02 and no name
- And at 2026-11-16T10:00:00Z `f9.ac064.user@example.com` ended the goal with the outcome Abandoned and no note
- And the administrator `f9.ac064.admin@example.com`, who at 2026-11-17T10:00:00Z unpublished the catalog habit "Meditation" with confirmation
- When at 2026-11-18T08:00:00Z `f9.ac064.user@example.com` starts a new attempt with Build, duration 10 minutes, fixed days on all seven days and the start date 2026-11-18
- Then the result is "attempt started" with the number 2 and the start date 2026-11-18
- And reading the goal gives it not ended, with the name `Meditation, 10 min daily`

### AC-065 (FR-004; FR-012) A new attempt keeps the goal's identifier, habit and own name

- Given the registered user `f9.ac065.user@example.com`, who created the goal `Meditation, 10 min daily`
- And at 2026-11-11T10:00:00Z `f9.ac065.user@example.com` changed the settings of the goal to Build, duration 10 minutes, fixed days on all seven days with the name "Morning calm"
- And at 2026-11-16T10:00:00Z `f9.ac065.user@example.com` ended the goal with the outcome Abandoned and no note
- When at 2026-11-18T08:00:00Z `f9.ac065.user@example.com` starts a new attempt with Build, duration 5 minutes, fixed days on all seven days and the start date 2026-11-18
- Then the result is "attempt started" with the number 2 and the start date 2026-11-18
- And reading the goal by the identifier it was created with gives the name "Morning calm", which is its own, and the habit's name `Meditation`
- And the attempts of the goal are, in this order, attempt 2 named "Morning calm" and attempt 1 named "Morning calm"

### AC-066 (FR-012; FR-014) Attempt numbers count up and are never reused

- Given the registered user `f9.ac066.user@example.com`, who has the ended Meditation goal
- And at 2026-11-17T08:00:00Z `f9.ac066.user@example.com` started a new attempt with Build, duration 10 minutes, fixed days on all seven days and the start date 2026-11-17
- And at 2026-11-20T10:00:00Z `f9.ac066.user@example.com` ended the goal with the outcome Achieved and no note, which gave "goal ended" with the ending date 2026-11-20 and the last date 2026-11-19
- When at 2026-11-21T08:00:00Z `f9.ac066.user@example.com` starts a new attempt with Build, duration 10 minutes, fixed days on all seven days and the start date 2026-11-21
- Then the result is "attempt started" with the number 3 and the start date 2026-11-21
- And the attempts of the goal are exactly, in this order, attempt 3 (start date 2026-11-21, not ended), attempt 2 (start date 2026-11-17, ending date 2026-11-20, last date 2026-11-19, outcome Achieved) and attempt 1 (start date 2026-11-02, ending date 2026-11-16, last date 2026-11-15, outcome Abandoned)

### AC-067 (FR-013; FR-016) A restarted per-week goal's first week starts on the first Monday on or after its start date

- Given the registered user `f9.ac067.user@example.com`, who created the goal `Yoga, 3 times a week`
- And at 2026-11-18T10:00:00Z `f9.ac067.user@example.com` ended the goal with the outcome No longer relevant and no note
- And at 2026-11-19T08:00:00Z `f9.ac067.user@example.com` started a new attempt with Build, yes/no, 3 times per week and the start date 2026-11-19
- When at 2026-11-20T10:00:00Z `f9.ac067.user@example.com` records Yes for 2026-11-20
- Then the result is "Nothing is scheduled on this date."
- And what can be recorded now contains no entry of `Yoga, 3 times a week`
- And at 2026-11-23T10:00:00Z what can be recorded now contains (`Yoga, 3 times a week`, 2026-11-23 – 2026-11-29, Due)

### AC-068 (FR-014) An earlier attempt's History gives at most its newest 366 dates

- Given the registered user `f9.ac068.user@example.com`, who created the goal `Meditation, 10 min daily` at 2025-10-01T08:00:00Z with the start date 2025-10-01
- And at 2026-11-16T10:00:00Z `f9.ac068.user@example.com` ended the goal with the outcome Abandoned and no note
- When at 2026-11-20T10:00:00Z `f9.ac068.user@example.com` reads attempt 1 of the goal
- Then its History has exactly 366 entries, each Missed, no record
- And its History's first entry is 2026-11-15 and its last entry is 2025-11-15
- And its consistency is "0% (0 of 411)"

### AC-069 (FR-013; FR-016) A date of a new attempt cannot be recorded before its day begins

- Given the registered user `f9.ac069.<n>.user@example.com`, who has the ended Meditation goal
- And at 2026-11-18T08:00:00Z `f9.ac069.<n>.user@example.com` started a new attempt with Build, duration 5 minutes, fixed days on all seven days and the start date 2026-11-23
- When at <time> `f9.ac069.<n>.user@example.com` records 5 for 2026-11-23
- Then the result is <result>

Examples:

| n | time | result |
|---|---|---|
| 01 | 2026-11-22T23:59:59Z | "You cannot record a day that has not begun." |
| 02 | 2026-11-23T00:00:00Z | the state Due, correction no |

### AC-070 (FR-002; FR-005) A change to a new attempt that has not begun replaces its first version

- Given the registered user `f9.ac070.user@example.com`, who has the ended Meditation goal
- And at 2026-11-18T08:00:00Z `f9.ac070.user@example.com` started a new attempt with Build, duration 5 minutes, fixed days on all seven days and the start date 2026-11-23
- When at 2026-11-19T10:00:00Z `f9.ac070.user@example.com` changes the amount to 7 minutes
- Then the result is "settings changed" with the date 2026-11-23
- And reading the goal gives the settings in effect Build, duration 7 minutes, fixed days on all seven days, from 2026-11-23, and no later settings
- And reading the goal gives the name `Meditation, 7 min daily`
- And the attempts of the goal give attempt 1 the name `Meditation, 10 min daily`

### AC-071 (FR-018; US4-AS1) The goals overview says which goal is ended, with its outcome and its latest attempt's figures

- Given the registered user `f9.ac071.user@example.com`, who has the three goals
- When at 2026-11-16T12:00:00Z `f9.ac071.user@example.com` reads the goals overview
- Then it gives `Meditation, 10 min daily` ended on 2026-11-16 with the outcome Abandoned, not paused, with the consistency "100% (12 of 12)"
- And it gives `Reading, 15 min daily` not ended
- And it gives `Sugar, at most 1 sweet a day` not ended, with the consistency "N/A"

### AC-072 (FR-019; US4-AS2) The dashboard keeps an ended goal's Pending entries and nothing else of it

- Given the registered user `f9.ac072.user@example.com`, who has the three goals
- When at 2026-11-16T12:00:00Z `f9.ac072.user@example.com` reads the dashboard
- Then the Today list contains (`Meditation, 10 min daily`, 2026-11-14, Pending, attempt 1, the latest attempt, goal ended) and (`Meditation, 10 min daily`, 2026-11-15, Pending, attempt 1, the latest attempt, goal ended)
- And the Today list contains no Due and no Upcoming entry of `Meditation, 10 min daily`
- And the dashboard has no summary of `Meditation, 10 min daily`
- And the number of goals is 2
- And the Pending count is 4 and the Due count is 2

### AC-073 (FR-019; US4-AS2) Once its grace periods have ended, an ended goal leaves the dashboard

- Given the registered user `f9.ac073.user@example.com`, who has the three goals
- When at 2026-11-18T12:00:00Z `f9.ac073.user@example.com` reads the dashboard
- Then the Today list contains no entry of `Meditation, 10 min daily`
- And the dashboard has no summary of `Meditation, 10 min daily`
- And the Pending count is 4 and the Due count is 2
- And the number of goals is 2

### AC-074 (FR-019) An ended attempt's Missed result of the current week is not counted

- Given the registered user `f9.ac074.user@example.com`, who created the goal `Meditation, 10 min daily`
- And at 2026-11-16T08:00:00Z `f9.ac074.user@example.com` records 10 for each date from 2026-11-02 to 2026-11-15
- And at 2026-11-18T08:00:00Z `f9.ac074.user@example.com` records 10 for each of 2026-11-17 and 2026-11-18
- And at 2026-11-19T10:00:00Z `f9.ac074.user@example.com` ended the goal with the outcome Abandoned and no note
- When at 2026-11-20T12:00:00Z `f9.ac074.user@example.com` reads the dashboard
- Then the Missed count is 0
- And the Today list contains (`Meditation, 10 min daily`, 2026-11-18, Pending, attempt 1, the latest attempt, goal ended)
- And the Today list contains no entry for 2026-11-16
- And the Pending count is 1

### AC-075 (FR-015; US4-AS3) The goal history lists every ended attempt, the latest ending first

- Given the registered user `f9.ac075.user@example.com`, who has the three goals
- When at 2026-11-16T12:00:00Z `f9.ac075.user@example.com` reads the goal history
- Then it is exactly, in this order, (`Meditation, 10 min daily`, attempt 1, ending date 2026-11-16, Abandoned, no note, the goal's latest attempt) and (`Sugar, at most 1 sweet a day`, attempt 1, ending date 2026-11-14, Achieved, note "Three weeks without extra sweets.", not the goal's latest attempt)
- And the first entry carries the identifier of `Meditation, 10 min daily` and the second the identifier of `Sugar, at most 1 sweet a day`

### AC-076 (FR-015) Attempts with the same ending date are ordered by name

- Given the registered user `f9.ac076.user@example.com`, who created, in this order, the goal `Reading, 15 min daily`, the goal `Yoga, 3 times a week`, the goal `Meditation, 10 min daily` and the goal `Sugar, at most 1 sweet a day`
- And at 2026-11-16T10:00:00Z `f9.ac076.user@example.com` ended `Reading, 15 min daily` with the outcome Achieved and no note
- And at 2026-11-16T11:00:00Z `f9.ac076.user@example.com` ended `Meditation, 10 min daily` with the outcome Abandoned and no note
- And at 2026-11-16T12:00:00Z `f9.ac076.user@example.com` ended `Yoga, 3 times a week` with the outcome No longer relevant and no note
- And at 2026-11-18T10:00:00Z `f9.ac076.user@example.com` ended `Sugar, at most 1 sweet a day` with the outcome Achieved and no note
- When at 2026-11-18T12:00:00Z `f9.ac076.user@example.com` reads the goal history
- Then its entries are exactly, in this order, `Sugar, at most 1 sweet a day` (ending date 2026-11-18), `Meditation, 10 min daily` (ending date 2026-11-16), `Reading, 15 min daily` (ending date 2026-11-16) and `Yoga, 3 times a week` (ending date 2026-11-16)

### AC-077 (FR-013; FR-017; US4-AS4) A restarted goal is read with its new attempt, whose figures start empty

- Given the registered user `f9.ac077.user@example.com`, who has the three goals
- When at 2026-11-16T12:00:00Z `f9.ac077.user@example.com` reads `Sugar, at most 1 sweet a day`
- Then it gives it not ended and its latest attempt as attempt 2 with the start date 2026-11-16
- And the report of `Sugar, at most 1 sweet a day` gives the consistency "N/A"

### AC-078 (FR-020; US4-AS5) An ended goal is not tracked in the catalog

- Given the catalog is seeded at 2026-11-01T00:00:00Z
- And the registered user `f9.ac078.user@example.com`, who at 2026-11-02T08:00:00Z created a goal on the catalog habit "Reading" with Build, duration 15 minutes, fixed days on all seven days, a goal on the catalog habit "Meditation" with Build, duration 10 minutes, fixed days on all seven days and a goal on the catalog habit "Sugar" with Reduce, count 1 `sweet`, 1 time per day, each with the start date 2026-11-02 and no name
- And at 2026-11-14T09:00:00Z `f9.ac078.user@example.com` ended `Sugar, at most 1 sweet a day` with the outcome Achieved and the note "Three weeks without extra sweets."
- And at 2026-11-16T09:00:00Z `f9.ac078.user@example.com` started a new attempt of `Sugar, at most 1 sweet a day` with Reduce, count 1 `sweet`, 1 time per day and the start date 2026-11-16
- And at 2026-11-16T10:00:00Z `f9.ac078.user@example.com` ended `Meditation, 10 min daily` with the outcome Abandoned and no note
- When at 2026-11-16T12:00:00Z `f9.ac078.user@example.com` browses the catalog with Only habits I track
- Then it contains "Reading" and "Sugar"
- And it does not contain "Meditation"
- And browsing the catalog without that filter gives "Meditation" the goal count 0, "Reading" the goal count 1 and "Sugar" the goal count 1
- And the details of the catalog habit "Meditation" give the goal count 0 and no goal
- And the details of the catalog habit "Sugar" give the goal count 1 and exactly the goal `Sugar, at most 1 sweet a day`

### AC-079 (FR-016) After a restart, a goal's History, recent results and recent notes cover only its latest attempt

- Given the registered user `f9.ac079.user@example.com`, who has the ended Meditation goal
- And at 2026-11-16T10:30:00Z `f9.ac079.user@example.com` saved the note "Calm" for 2026-11-13
- And at 2026-11-18T08:00:00Z `f9.ac079.user@example.com` started a new attempt with Build, duration 10 minutes, fixed days on all seven days and the start date 2026-11-18
- And at 2026-11-20T10:00:00Z `f9.ac079.user@example.com` records 10 for each of 2026-11-18 and 2026-11-19
- And at 2026-11-20T10:01:00Z `f9.ac079.user@example.com` saved the note "Fresh start" for 2026-11-19
- When at 2026-11-20T12:00:00Z `f9.ac079.user@example.com` reads the history of the goal from 2026-11-02 to 2026-11-20
- Then it gives exactly 2026-11-18 Pending, record 10; 2026-11-19 Pending, record 10; and 2026-11-20 Due, no record
- And the recent notes of the goal are exactly (2026-11-19, "Fresh start", Pending)
- And the recent results of the report of the goal contain no date before 2026-11-18
- And the trend of the report of the goal has no bar for a date before 2026-11-18

### AC-080 (FR-016; FR-019) An earlier attempt's Pending entry gives that attempt's name and Build or Reduce

- Given the registered user `f9.ac080.user@example.com`, who has the ended Meditation goal
- And at 2026-11-17T09:00:00Z `f9.ac080.user@example.com` started a new attempt with Reduce, duration 30 minutes, 1 time per day and the start date 2026-11-17
- When at 2026-11-17T10:00:00Z `f9.ac080.user@example.com` reads what can be recorded now
- Then it contains (`Meditation, 10 min daily`, 2026-11-15, Pending, attempt 1, not the latest attempt, Build)
- And the dashboard's Today list contains (`Meditation, 10 min daily`, 2026-11-15, Pending, attempt 1, not the latest attempt, goal not ended, Build)

### AC-081 (FR-021) A time zone change adds no version to an ended goal

- Given the registered user `f9.ac081.user@example.com`, who has the ended Meditation goal
- And at 2026-11-17T10:00:00Z `f9.ac081.user@example.com` changed the account's time zone to `Europe/Kyiv`
- When at 2026-11-17T10:01:00Z `f9.ac081.user@example.com` reads the goal
- Then it gives the settings in effect Build, duration 10 minutes, fixed days on all seven days, time zone UTC, from 2026-11-02, and no later settings

### AC-082 (FR-012; FR-021) A new attempt uses the account's time zone when it starts

- Given the registered user `f9.ac082.user@example.com`, who has the ended Meditation goal
- And at 2026-11-17T10:00:00Z `f9.ac082.user@example.com` changed the account's time zone to `Europe/Kyiv`
- When at 2026-11-18T08:00:00Z `f9.ac082.user@example.com` starts a new attempt with Build, duration 10 minutes, fixed days on all seven days and the start date 2026-11-19
- Then the result is "attempt started" with the number 2 and the start date 2026-11-19
- And reading the goal gives the settings in effect Build, duration 10 minutes, fixed days on all seven days, time zone Europe/Kyiv, from 2026-11-19, and no later settings

### AC-083 (FR-005; FR-021) A time zone change before a new attempt begins updates its first version

- Given the registered user `f9.ac083.user@example.com`, who has the ended Meditation goal
- And at 2026-11-18T08:00:00Z `f9.ac083.user@example.com` started a new attempt with Build, duration 5 minutes, fixed days on all seven days and the start date 2026-11-23
- When at 2026-11-19T10:00:00Z `f9.ac083.user@example.com` changes the account's time zone to `Europe/Kyiv`
- Then reading the goal gives the settings in effect Build, duration 5 minutes, fixed days on all seven days, time zone Europe/Kyiv, from 2026-11-23, and no later settings

### AC-084 (FR-022; US5-AS1) Another registered user cannot use any use case of this slice on someone else's goal

- Given the registered user `f9.ac084.<n>.a@example.com`, who has the ended Meditation goal
- And the registered user `f9.ac084.<n>.b@example.com`
- When at 2026-11-18T10:00:00Z `f9.ac084.<n>.b@example.com` <action>
- Then the result is "This goal does not exist."
- And reading the goal by `f9.ac084.<n>.a@example.com` gives it ended with the ending date 2026-11-16, the outcome Abandoned and the note "Too busy this month.", with its latest attempt attempt 1
- And reading the goal by `f9.ac084.<n>.a@example.com` gives the settings in effect Build, duration 10 minutes, fixed days on all seven days, from 2026-11-02, and no later settings

Examples:

| n | action |
|---|---|
| 01 | changes the settings of the goal of `f9.ac084.<n>.a@example.com` to Build, duration 20 minutes, fixed days on all seven days |
| 02 | ends the goal of `f9.ac084.<n>.a@example.com` with the outcome Achieved and no note |
| 03 | reads the starting values of a new attempt of the goal of `f9.ac084.<n>.a@example.com` |
| 04 | starts a new attempt of the goal of `f9.ac084.<n>.a@example.com` with Build, duration 10 minutes, fixed days on all seven days and the start date 2026-11-18 |
| 05 | reads the attempts of the goal of `f9.ac084.<n>.a@example.com` |
| 06 | reads attempt 1 of the goal of `f9.ac084.<n>.a@example.com` |

### AC-085 (FR-023; US5-AS2) An administrator cannot use any use case of this slice

- Given the registered user `f9.ac085.<n>.a@example.com`, who has the ended Meditation goal
- And the administrator `f9.ac085.<n>.admin@example.com`
- When at 2026-11-18T10:00:00Z `f9.ac085.<n>.admin@example.com` <action>
- Then the result is "Goals are for registered users."
- And reading the goal by `f9.ac085.<n>.a@example.com` gives it ended with the ending date 2026-11-16, the outcome Abandoned and the note "Too busy this month.", with its latest attempt attempt 1
- And reading the goal by `f9.ac085.<n>.a@example.com` gives the settings in effect Build, duration 10 minutes, fixed days on all seven days, from 2026-11-02, and no later settings

Examples:

| n | action |
|---|---|
| 01 | changes the settings of the goal of `f9.ac085.<n>.a@example.com` to Build, duration 20 minutes, fixed days on all seven days |
| 02 | ends the goal of `f9.ac085.<n>.a@example.com` with the outcome Achieved and no note |
| 03 | reads the starting values of a new attempt of the goal of `f9.ac085.<n>.a@example.com` |
| 04 | starts a new attempt of the goal of `f9.ac085.<n>.a@example.com` with Build, duration 10 minutes, fixed days on all seven days and the start date 2026-11-18 |
| 05 | reads the attempts of the goal of `f9.ac085.<n>.a@example.com` |
| 06 | reads attempt 1 of the goal of `f9.ac085.<n>.a@example.com` |
| 07 | reads the goal history |

### AC-086 (FR-023; US5-AS3) An anonymous visitor cannot use any use case of this slice

- Given the registered user `f9.ac086.<n>.a@example.com`, who has the ended Meditation goal
- When at 2026-11-18T10:00:00Z the anonymous visitor <action>
- Then the result is "You are not signed in."
- And reading the goal by `f9.ac086.<n>.a@example.com` gives it ended with the ending date 2026-11-16, the outcome Abandoned and the note "Too busy this month.", with its latest attempt attempt 1
- And reading the goal by `f9.ac086.<n>.a@example.com` gives the settings in effect Build, duration 10 minutes, fixed days on all seven days, from 2026-11-02, and no later settings

Examples:

| n | action |
|---|---|
| 01 | changes the settings of the goal of `f9.ac086.<n>.a@example.com` to Build, duration 20 minutes, fixed days on all seven days |
| 02 | ends the goal of `f9.ac086.<n>.a@example.com` with the outcome Achieved and no note |
| 03 | reads the starting values of a new attempt of the goal of `f9.ac086.<n>.a@example.com` |
| 04 | starts a new attempt of the goal of `f9.ac086.<n>.a@example.com` with Build, duration 10 minutes, fixed days on all seven days and the start date 2026-11-18 |
| 05 | reads the attempts of the goal of `f9.ac086.<n>.a@example.com` |
| 06 | reads attempt 1 of the goal of `f9.ac086.<n>.a@example.com` |
| 07 | reads the goal history |

### AC-087 (FR-015; FR-022; US5-AS4) A user's goal history contains only their own attempts

- Given the registered user `f9.ac087.a@example.com`, who has the ended Meditation goal
- And the registered user `f9.ac087.b@example.com`, who created the goal `Reading, 15 min daily`
- And at 2026-11-16T11:00:00Z `f9.ac087.b@example.com` ended the goal with the outcome Achieved and no note
- When at 2026-11-16T12:00:00Z `f9.ac087.b@example.com` reads the goal history
- Then it is exactly (`Reading, 15 min daily`, attempt 1, ending date 2026-11-16, Achieved, no note, the goal's latest attempt)

### AC-088 (FR-024) A goal without changes, ending or restart keeps slice 6's figures and is counted

- Given the registered user `f9.ac088.user@example.com`, who created the goal `Meditation, 10 min daily` at 2026-11-01T08:00:00Z with the start date 2026-11-01
- And at 2026-11-21T08:00:00Z `f9.ac088.user@example.com` records 10 for each date from 2026-11-01 to 2026-11-20 except 2026-11-05 and 2026-11-09
- And at 2026-11-21T08:01:00Z `f9.ac088.user@example.com` records 4 for 2026-11-05
- When at 2026-11-23T10:00:00Z `f9.ac088.user@example.com` reads the report of the goal
- Then the consistency is "90% (18 of 20)"
- And the current streak is "11 commitments" and the longest streak is "11 commitments"
- And the attempts of the goal are exactly attempt 1, start date 2026-11-01, not ended
- And the goals overview gives `Meditation, 10 min daily` not ended
- And the dashboard's number of goals is 1
- And the goal history is empty

### AC-089 (FR-031; US6-AS1) Demo goal-change loading creates `demo7@example.com` and names no private data in its results

- Given the earlier demo tasks run at 2026-11-18T10:00:00Z
- When demo goal-change loading runs at 2026-11-18T10:00:00Z with the demo password `example2026`
- Then its results are, in this order, (demo7@example.com, created), (demo7@example.com, demo goals added) and (demo7@example.com, demo records added)
- And no result contains "Reading", "Meditation", "Sugar", "Short on time.", "Too busy.", "Three weeks without extra sweets.", "Abandoned", "Achieved", "sweet", "min", "0", "1", "5", "10", "15", "20" or "25"
- And sign-in with `demo7@example.com` and `example2026` succeeds

### AC-090 (FR-031; US6-AS2) The demo figures of `demo7@example.com` hold on every weekday

- Given the earlier demo tasks run at <time>
- And demo goal-change loading ran at <time> with the demo password `example2026`
- When at <time> `demo7@example.com` reads the reports of `Reading, 20 min daily`, `Meditation, 5 min daily` and `Sugar, at most 1 sweet a day`
- Then `Reading, 20 min daily` has the consistency "96% (25 of 26)", the current streak "6 commitments" and the longest streak "19 commitments"
- And `Meditation, 5 min daily` has the consistency "100% (12 of 12)", the current streak "12 commitments" and the longest streak "12 commitments"
- And `Sugar, at most 1 sweet a day` has the consistency "100% (23 of 23)", the current streak "23 days" and the longest streak "23 days"
- And reading `Meditation, 5 min daily` gives its latest attempt as attempt 2, and its attempts give attempt 1, `Meditation, 10 min daily`, outcome Abandoned, note "Too busy.", consistency "52% (11 of 21)", longest streak "11 commitments"
- And the goals overview gives `Sugar, at most 1 sweet a day` ended with the outcome Achieved, and `Reading, 20 min daily` and `Meditation, 5 min daily` not ended
- And the dashboard gives the Due count 2, the Pending count 4, the Missed count 0 and the number of goals 2
- And the goal history is, in this order, `Sugar, at most 1 sweet a day`, attempt 1, the goal's latest attempt; and `Meditation, 10 min daily`, attempt 1, not the goal's latest attempt

Examples:

| n | time |
|---|---|
| 01 | 2026-11-16T10:00:00Z |
| 02 | 2026-11-17T10:00:00Z |
| 03 | 2026-11-18T10:00:00Z |
| 04 | 2026-11-19T10:00:00Z |
| 05 | 2026-11-20T10:00:00Z |
| 06 | 2026-11-21T10:00:00Z |
| 07 | 2026-11-22T10:00:00Z |

### AC-091 (FR-031; US6-AS2) The demo dates, settings, states and notes of `demo7@example.com`

- Given the earlier demo tasks run at 2026-11-18T10:00:00Z
- And demo goal-change loading ran at 2026-11-18T10:00:00Z with the demo password `example2026`
- When at 2026-11-18T10:00:00Z `demo7@example.com` reads `Reading, 20 min daily`
- Then it gives the settings in effect Build, duration 20 minutes, fixed days on all seven days, from 2026-11-05
- And it gives the later settings Build, duration 25 minutes, fixed days on all seven days, from 2026-11-19
- And it gives its latest attempt as attempt 1 with the start date 2026-10-21, not ended
- And the history of `Reading, 20 min daily` from 2026-11-04 to 2026-11-18 gives 2026-11-04 Met, record 15; 2026-11-05 Met, record 20; 2026-11-09 Failed, record 15; 2026-11-15 Met, record 20; 2026-11-16 Pending, record 20; 2026-11-17 Pending, no record; 2026-11-18 Due, no record
- And the recent notes of `Reading, 20 min daily` are exactly (2026-11-09, "Short on time.", Failed)
- And the attempts of `Meditation, 5 min daily` are, in this order, attempt 2, start date 2026-11-04, not ended; and attempt 1, start date 2026-10-07, ending date 2026-10-28, last date 2026-10-27
- And the history of `Meditation, 5 min daily` from 2026-11-15 to 2026-11-18 gives 2026-11-15 Met, record 5; 2026-11-16 Pending, record 5; 2026-11-17 Pending, no record; 2026-11-18 Due, no record
- And reading `Sugar, at most 1 sweet a day` gives it ended with the ending date 2026-11-11, the last date 2026-11-10, the outcome Achieved and the note "Three weeks without extra sweets.", and its latest attempt as attempt 1 with the start date 2026-10-19
- And the goal history is, in this order, (`Sugar, at most 1 sweet a day`, attempt 1, ending date 2026-11-11, Achieved, note "Three weeks without extra sweets.", the goal's latest attempt) and (`Meditation, 10 min daily`, attempt 1, ending date 2026-10-28, Abandoned, note "Too busy.", not the goal's latest attempt)

### AC-092 (FR-031; US6-AS3) Demo goal-change loading leaves an existing `demo7@example.com` unchanged

- Given the earlier demo tasks run at 2026-11-18T10:00:00Z
- And demo goal-change loading ran at 2026-11-18T10:00:00Z with the demo password `example2026`
- When demo goal-change loading runs at 2026-11-19T10:00:00Z with the demo password `example2026`
- Then its results are (demo7@example.com, already exists)
- And at 2026-11-19T10:00:00Z the goals overview of `demo7@example.com` lists exactly `Reading, 25 min daily`, `Meditation, 5 min daily` and `Sugar, at most 1 sweet a day`
- And at 2026-11-19T10:00:00Z the goal history of `demo7@example.com` is, in this order, `Sugar, at most 1 sweet a day`, attempt 1, ending date 2026-11-11; and `Meditation, 10 min daily`, attempt 1, ending date 2026-10-28

### AC-093 (FR-031; US6-AS4) Demo goal-change loading without a valid demo password creates nothing

- Given the earlier demo tasks run at 2026-11-18T10:00:00Z
- When demo goal-change loading runs at 2026-11-18T10:00:00Z <password>
- Then its results are (<result>)
- And sign-in with `demo7@example.com` and `example2026` does not succeed

Examples:

| n | password | result |
|---|---|---|
| 01 | with no demo password | no demo password configured |
| 02 | with the demo password `abc` | demo password does not meet the password rule |

### AC-094 (FR-031) A demo goal whose catalog habit is not Published is left out

- Given the earlier demo tasks run at 2026-11-18T10:00:00Z
- And the administrator `f9.ac094.<n>.admin@example.com`
- And `f9.ac094.<n>.admin@example.com` unpublished <habits> with confirmation
- When demo goal-change loading runs at 2026-11-18T10:00:00Z with the demo password `example2026`
- Then its results are, in this order, (demo7@example.com, created), (demo7@example.com, demo goals added) and (demo7@example.com, demo records added)
- And at 2026-11-18T10:00:00Z the goals overview of `demo7@example.com` lists exactly <goals>
- And at 2026-11-18T10:00:00Z the goal history of `demo7@example.com` lists exactly <history>

Examples:

| n | habits | goals | history |
|---|---|---|---|
| 01 | the catalog habit "Sugar" | `Reading, 20 min daily` and `Meditation, 5 min daily` | `Meditation, 10 min daily`, attempt 1 |
| 02 | the catalog habits "Reading", "Meditation" and "Sugar" | no goal | no entry |

### AC-095 (FR-030; US6-AS5) The earlier demo accounts get no settings change, ending or second attempt

- Given the earlier demo tasks run at 2026-11-18T10:00:00Z
- And demo goal-change loading ran at 2026-11-18T10:00:00Z with the demo password `example2026`
- When at 2026-11-18T10:00:00Z each account that a result of demo loading, demo history loading, demo report loading or demo pause loading gives as created, `demo1@example.com` and `demo4@example.com` to `demo6@example.com` included, signs in and reads the attempts of each of its goals and its goal history
- Then each goal of those accounts has exactly one attempt, attempt 1, not ended
- And the goal history of each of those accounts is empty

### AC-096 (FR-016) An ended goal's evaluation calendar stops at the last date

- Given the registered user `f9.ac096.user@example.com`, who has the ended Meditation goal
- When at 2026-11-16T10:01:00Z `f9.ac096.user@example.com` reads the evaluation calendar of the goal from 2026-11-14 to 2026-11-20
- Then it gives exactly 2026-11-14 and 2026-11-15

### AC-097 (FR-013; FR-016; US3-AS4) The evaluation calendar gives no date between two attempts

- Given the registered user `f9.ac097.user@example.com`, who has the ended Meditation goal
- And at 2026-11-18T08:00:00Z `f9.ac097.user@example.com` started a new attempt with Build, duration 5 minutes, fixed days on all seven days and the start date 2026-11-23
- When at 2026-11-19T10:00:00Z `f9.ac097.user@example.com` reads the evaluation calendar of the goal from 2026-11-16 to 2026-11-29
- Then it gives exactly 2026-11-23, 2026-11-24, 2026-11-25, 2026-11-26, 2026-11-27, 2026-11-28 and 2026-11-29

### AC-098 (FR-013; FR-016) A restarted per-week goal's evaluation calendar gives no week outside an attempt

- Given the registered user `f9.ac098.user@example.com`, who created the goal `Yoga, 3 times a week`
- And at 2026-11-18T10:00:00Z `f9.ac098.user@example.com` ended the goal with the outcome No longer relevant and no note
- And at 2026-11-19T08:00:00Z `f9.ac098.user@example.com` started a new attempt with Build, yes/no, 3 times per week and the start date 2026-11-19
- When at 2026-11-19T10:00:00Z `f9.ac098.user@example.com` reads the evaluation calendar of the goal from 2026-11-16 to 2026-11-29
- Then it gives exactly 2026-11-23 – 2026-11-29

### AC-099 (FR-008) A per-week goal ended on a Sunday or a Monday

- Given the registered user `f9.ac099.<n>.user@example.com`, who created the goal `Yoga, 3 times a week`
- And at 2026-11-14T08:00:00Z `f9.ac099.<n>.user@example.com` records Yes for each of 2026-11-02, 2026-11-04 and 2026-11-06
- And at 2026-11-14T08:01:00Z `f9.ac099.<n>.user@example.com` records Yes for each of 2026-11-09, 2026-11-11 and 2026-11-13
- When at <time> `f9.ac099.<n>.user@example.com` ends the goal with the outcome No longer relevant and no note
- Then the result is "goal ended" with the ending date <ending> and the last date <last>
- And at 2026-11-25T00:00:00Z the report of the goal gives the consistency <consistency>

Examples:

| n | time | ending | last | consistency |
|---|---|---|---|---|
| 01 | 2026-11-22T10:00:00Z | 2026-11-22 | 2026-11-15 | "100% (2 of 2)" |
| 02 | 2026-11-23T10:00:00Z | 2026-11-23 | 2026-11-22 | "66% (2 of 3)" |

### AC-100 (FR-003) "settings unchanged" compares with the latest version, also while it waits

- Given the registered user `f9.ac100.<n>.user@example.com`, who created the goal `Reading, 15 min daily`
- And at 2026-11-11T10:00:00Z `f9.ac100.<n>.user@example.com` changed the amount to 20 minutes, which gave "settings changed" with the date 2026-11-12
- When at 2026-11-11T11:00:00Z `f9.ac100.<n>.user@example.com` changes the settings of the goal to <settings>
- Then the result is <result>
- And reading the goal gives the later settings <later>, from 2026-11-12

Examples:

| n | settings | result | later |
|---|---|---|---|
| 01 | Build, duration 20 minutes, fixed days on all seven days | "settings unchanged" | Build, duration 20 minutes, fixed days on all seven days |
| 02 | Build, duration 15 minutes, fixed days on all seven days | "settings changed" with the date 2026-11-12 | Build, duration 15 minutes, fixed days on all seven days |

### AC-101 (FR-014) Reading the latest attempt while it is not ended gives its History up to the current date

- Given the registered user `f9.ac101.user@example.com`, who has the ended Meditation goal
- And at 2026-11-18T08:00:00Z `f9.ac101.user@example.com` started a new attempt with Build, duration 10 minutes, fixed days on all seven days and the start date 2026-11-18
- And at 2026-11-20T10:00:00Z `f9.ac101.user@example.com` records 10 for each of 2026-11-18 and 2026-11-19
- When at 2026-11-20T12:00:00Z `f9.ac101.user@example.com` reads attempt 2 of the goal
- Then it gives attempt 2, `Meditation, 10 min daily`, start date 2026-11-18, not ended
- And its History is exactly, newest first, 2026-11-20 Due, no record; 2026-11-19 Pending, record 10; 2026-11-18 Pending, record 10

### AC-102 (FR-014) Reading an attempt that ended before it began gives an empty History

- Given the registered user `f9.ac102.user@example.com`, who has the ended Meditation goal
- And at 2026-11-18T08:00:00Z `f9.ac102.user@example.com` started a new attempt with Build, duration 5 minutes, fixed days on all seven days and the start date 2026-11-23
- And at 2026-11-19T10:00:00Z `f9.ac102.user@example.com` ended the goal with the outcome No longer relevant and no note
- When at 2026-11-20T10:00:00Z `f9.ac102.user@example.com` reads attempt 2 of the goal
- Then it gives attempt 2, `Meditation, 5 min daily`, start date 2026-11-23, ending date 2026-11-19, last date 2026-11-18, outcome No longer relevant, no note, consistency "N/A"
- And its History has no entry

### AC-103 (FR-004; FR-015) An earlier attempt is named by its habit's current name

- Given the registered user `f9.ac103.user@example.com`, who has the ended Meditation goal
- And at 2026-11-17T10:00:00Z `f9.ac103.user@example.com` started a new attempt with Build, duration 5 minutes, fixed days on all seven days and the start date 2026-11-18
- And at 2026-11-17T11:00:00Z `f9.ac103.user@example.com` renamed the custom habit `Meditation` to "Mindfulness"
- When at 2026-11-18T10:00:00Z `f9.ac103.user@example.com` reads the attempts of the goal
- Then they are, in this order, attempt 2 named `Mindfulness, 5 min daily` and attempt 1 named `Mindfulness, 10 min daily`
- And the goal history is exactly (`Mindfulness, 10 min daily`, attempt 1, ending date 2026-11-16, Abandoned, note "Too busy this month.", not the goal's latest attempt)

### AC-104 (FR-011; FR-012; FR-022) Another registered user gets "This goal does not exist." before the ended check of a restart

- Given the registered user `f9.ac104.<n>.a@example.com`, who created the goal `Meditation, 10 min daily`
- And the registered user `f9.ac104.<n>.b@example.com`
- When at 2026-11-16T10:00:00Z `f9.ac104.<n>.b@example.com` <action>
- Then the result is "This goal does not exist."
- And reading the goal by `f9.ac104.<n>.a@example.com` gives it not ended, with its latest attempt attempt 1

Examples:

| n | action |
|---|---|
| 01 | reads the starting values of a new attempt of the goal of `f9.ac104.<n>.a@example.com` |
| 02 | starts a new attempt of the goal of `f9.ac104.<n>.a@example.com` with Build, duration 10 minutes, fixed days on all seven days and the start date 2026-11-17 |

### AC-105 (FR-020) A custom habit used only by an ended goal is still used by goals

- Given the registered user `f9.ac105.user@example.com`, who has the ended Meditation goal
- When at 2026-11-17T10:00:00Z `f9.ac105.user@example.com` deletes the custom habit `Meditation`
- Then the custom habit is not deleted
- And reading the custom habit `Meditation` gives it, with 1 goal that uses it
- And reading the goal gives the habit's name `Meditation`

### AC-106 (FR-020) A catalog habit used only by an ended goal is not unpublished without confirmation

- Given the administrator `f9.ac106.admin@example.com`, who created and published the catalog habit "Stretching F9 AC106" with the intent Build, the category Other and the icon `book-open`
- And the registered user `f9.ac106.user@example.com`, who at 2026-11-02T08:00:00Z created a goal on the catalog habit "Stretching F9 AC106" with Build, duration 10 minutes, fixed days on all seven days, the start date 2026-11-02 and no name
- And at 2026-11-16T10:00:00Z `f9.ac106.user@example.com` ended the goal with the outcome Abandoned and no note
- When at 2026-11-17T10:00:00Z `f9.ac106.admin@example.com` unpublishes the catalog habit "Stretching F9 AC106" without confirmation
- Then the habit is not unpublished
- And browsing the catalog as the anonymous visitor contains "Stretching F9 AC106"
