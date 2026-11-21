# Acceptance criteria: Excusals and pauses

**Feature**: specs/008-excusals-and-pauses · **Tag**: F008 · **Spec**: spec.md

Conventions used by every item:

- **Time.** Every use case takes the supplied current time (constitution II; spec "What acceptance tests can
  reach"). Steps run in the order they are written. A step runs at the time the item gives for it. Setup that
  gives no time (administrator creation, registration, sign-in, creating a custom habit) runs at the earliest time
  the item gives, counting the default goal-creation time below when the item uses it, so it always runs before the
  goal is created. A Given step that records, changes, deletes or saves a note and gives no time runs at the time
  of the item's When step. A Then step, and every And or But step after it, checks the outcome of the item's When
  step; to check it, such a step may also read (a history, what can be recorded now, the pauses, the recent notes,
  a report, the goals overview or the dashboard), which changes nothing. It runs at the When step's time unless
  it gives its own time. Instants are written in UTC (`2026-11-16T09:00:00Z`); dates are written `2026-11-16`.
  16 November 2026 is a Monday, so 2026-11-02, 2026-11-09, 2026-11-16 and 2026-11-23 are Mondays.
- **Accounts.**
  - "the anonymous visitor" is the acting user who is not signed in.
  - "the registered user `x`" means the anonymous visitor registered the email `x` with the password
    `walking42`, with no time zone and no name, registration succeeded, and sign-in with `x` and `walking42`
    succeeded; `x` is then the acting user for the steps that name it. Its time zone is the default, `UTC`.
  - "the registered user `x` in `Z`" is the same, but registration also gave the time zone `Z`.
  - "the administrator `a`" means administrator creation ran with the email `a` and the password `admin2026`
    and created the administrator, and sign-in with `a` and `admin2026` succeeded.
- **Names in the database.** Emails start with `f8.acNNN.`, NNN being the item's number, so they differ from
  every other item and from the other features' acceptance tests in the same database (ADR 0003). The demo
  emails `demo1@example.com` to `demo6@example.com` appear only in items that start from a reset database.
- **Examples.** An item with an `Examples:` table runs once per row, each row as its own scenario. Every email of
  such an item contains `<n>`, the row's value in the column `n`, so rows never share data. The one exception is
  an item that starts from a reset database: it resets the database for every row, so its demo emails carry no
  `<n>`.
- **Habits and goals.** "`x` created the custom habit `C`" means `x` created it with the intent Build, the
  category Other and the icon `book-open`. "`x` created a goal on their custom habit `C` with S and the start date
  D" is slice 4's create-goal use case with the settings S, the start date D and no name, at the step's time;
  creating succeeded. Settings are written as: Build or Reduce; the measurement (yes/no, count or duration); for
  count the amount and the unit in backticks (`count 8 `glasses``); for duration the amount in minutes; the
  schedule: fixed days with the days named ("fixed days on all seven days" is Monday to Sunday), or N times per
  day or per week. The goals of this file and their names (spec "Earlier behaviour these tests use"):
  - "the goal `Yoga, 3 days a week`": on the custom habit `Yoga`, Build, yes/no, fixed days on Monday, Wednesday
    and Friday;
  - "the goal `Yoga, 3 times a week`": on the custom habit `Yoga`, Build, yes/no, 3 times per week;
  - "the goal `Drinking water, 8 glasses a day`": on the custom habit `Drinking water`, Build, count 8 `glasses`,
    1 time per day;
  - "the goal `Meditation, 10 min daily`": on the custom habit `Meditation`, Build, duration 10 minutes, fixed days
    on all seven days;
  - "the goal `Reading, 20 pages daily`": on the custom habit `Reading`, Build, count 20 `pages`, fixed days on all
    seven days;
  - "the goal `Sugar, at most 1 sweet a day`": on the custom habit `Sugar`, Reduce, count 1 `sweet`, 1 time per day.

  "`x` created the goal `G` at T with the start date D" means `x` created the custom habit of G (unless `x` already
  has it), then at T created a goal on it with G's settings and the start date D. Unless an item says otherwise, a
  goal is created at `2026-11-02T08:00:00Z` with the start date `2026-11-02`. With one goal, "the goal" is that
  goal; with several, a goal is named by its name.
- **Recording** (slice 5): "records V for D" is slice 5's record use case for the goal, the date D and the value V,
  with no note; V is `Yes` or `No` for a yes/no goal and a whole number for a count goal (in its unit) or a duration
  goal (in minutes). "records V for each date from D1 to D2" records once for every date of that range. "changes
  the record of D to V" and "deletes the record of D" are slice 5's change-record and delete-record use cases for
  the only record of the date D. "saves the note N for D" is slice 5's save-note use case for the commitment or
  period that contains D. "records nothing" calls no use case. Each of these succeeds unless the item gives its
  result. The result of recording,
  changing or deleting is written "the state S, correction yes/no" (slice 5 FR-006).
- **Use cases of this slice.**
  - "excuses D with the reason R" / "excuses D without a reason" is FR-001's use case for the goal and the date D.
    A reason written "`x` repeated 201 times" is that one character repeated that many times.
  - "removes the excusal of D" is FR-004's use case for the goal and the date D.
  - "pauses the goal from D1 until D2 with the reason R" / "without a reason" is FR-006's use case; "with no first
    day" or "with no last day" leaves that day out.
  - "resumes the goal" is FR-010's use case; "cancels the pause P" is FR-011's use case for the goal and the
    identifier of the pause P, written by its days ("the pause 2026-11-23 – 2026-11-27"), the identifier being the
    one the "paused" result gave.
  - "reads the pauses of the goal" is FR-012's use case. A pause is written "(first day, last day, reason R or no
    reason, current, upcoming or past)"; every pause read also carries its identifier, equal to the one the
    "paused" result gave.
- **Earlier reads.**
  - "reads the history of the goal from D1 to D2" is slice 5's history use case (FR-009, FR-017).
  - "reads what can be recorded now" is slice 5's use case (FR-011).
  - "reads the recent notes of the goal" is slice 5's recent notes use case (FR-008, FR-020).
  - "reads the report of the goal" is slice 6's goal report use case; "reads the goals overview" and "reads the
    dashboard" are slice 6's overview and dashboard use cases (FR-013 to FR-019).
- **Results and figures.**
  - A refusal is written by its text; a success by its result text and, as far as the item gives them, its
    values ("excused" with the date; "excusal removed" with the state; "paused" with the first and last day).
  - A history entry or result is written as its date (a per-week period: its first and last date, "2026-11-16 –
    2026-11-22"), then its state (Upcoming, Due, Pending, Met, Failed, Missed, Excused or Paused), then, as far as
    the item gives them: "record V" or "records V, V, V" or "no record"; "grace ends T" or "no grace end"; "correction no"; for an Excused entry "reason R" or "no reason", and
    "removable" or "not removable" (whether the excusal can still be removed). An item checks only what it writes.
  - A consistency is written by its text ("90% (10 of 11)" or "N/A"); a streak by its text ("2 commitments",
    "0 weeks").
  - A trend bar is written "(D, V)": its date or dates D and its value V, or "no value", or "Paused" or "Excused"
    for a marked bar, which has no value. The target is written once when every bar has it, otherwise per bar as
    "(D, V, target T)".
  - A dashboard entry (Today list) is written "(goal, date, state)"; a recent note "(date, note, state)" or, on the
    dashboard, "(goal, date, note, state)". Inside these tuples a goal name is in backticks, as its comma is part
    of the name. A goal of the overview is written "paused until D" or "not paused".
- **Demo data.** "the AC database is reset" means the reset entry point of `HT.Persistence` (ADR 0003) drops
  and rebuilds the schema, so no account and no habit exists. "the earlier demo tasks run at T" means: the AC
  database is reset, then demo loading, demo history loading and demo report loading run in that order at T with
  the demo password `example2026`. "the demo run at T" means: the earlier demo tasks run at T, then demo pause
  loading runs at T with the demo password `example2026`. A demo account signs in with its email and
  `example2026`. The acceptance tests configure no default time zone, so the installation's default time zone is
  `UTC` and the run date R is the UTC date of the run time. A result of a demo task is written (email, text), or
  (text) for a result that names no account ("no demo password configured", "demo password does not meet the
  password rule").

## Coverage

| Requirement / scenario | AC items | Note |
|---|---|---|
| FR-001 | AC-001, AC-002, AC-003, AC-016 | |
| FR-002 | AC-001, AC-002, AC-006, AC-007, AC-008, AC-009, AC-010, AC-011, AC-012, AC-013, AC-014, AC-015, AC-016, AC-084 | |
| FR-003 | AC-004, AC-005 | |
| FR-004 | AC-017, AC-018, AC-019, AC-020, AC-057 | |
| FR-005 | AC-021, AC-022, AC-034 | |
| FR-006 | AC-023, AC-024, AC-025, AC-026, AC-027, AC-028, AC-029, AC-030, AC-031, AC-088 | |
| FR-007 | AC-011, AC-012, AC-023, AC-031, AC-033, AC-036, AC-037, AC-045, AC-046 | |
| FR-008 | AC-038, AC-039 | |
| FR-009 | AC-022, AC-034, AC-039, AC-040, AC-041 | |
| FR-010 | AC-032, AC-035, AC-037, AC-042, AC-043, AC-044, AC-045, AC-087 | |
| FR-011 | AC-047, AC-048, AC-049, AC-050, AC-051 | |
| FR-012 | AC-042, AC-052, AC-086 | |
| FR-013 | AC-036, AC-053, AC-057, AC-058, AC-059 | |
| FR-014 | AC-054, AC-057, AC-058, AC-059, AC-088 | |
| FR-015 | AC-055 | The progress of an exempt result is not checked: its format (slice 5 FR-014) is not restated in spec.md. The records kept on exempt entries are checked (AC-036, AC-039, AC-041). |
| FR-016 | AC-056, AC-060, AC-061, AC-062, AC-063, AC-064, AC-065, AC-083 | |
| FR-017 | AC-001, AC-004, AC-041 | The progress is not checked, for the reason given at FR-015. |
| FR-018 | AC-023, AC-033, AC-042, AC-045, AC-046, AC-066 | "Its order is unchanged" is not checked: the overview's order rule (slice 6 FR-011) is not restated in spec.md, and it depends on whether a goal has a Pending or a Due result, while a pause from the current date makes the Due result Paused, so the order before and after a pause may rightly differ. |
| FR-019 | AC-046, AC-067, AC-068, AC-069, AC-070, AC-085 | |
| FR-020 | AC-071 | |
| FR-021 | AC-051, AC-072 | |
| FR-022 | AC-073, AC-074 | |
| FR-023 | AC-075 | The locked F001, F002, F004, F005 and F007 scenarios are run unchanged as well. |
| FR-024 | AC-020, AC-043, AC-047, AC-057 | |
| FR-025 | AC-005 | That nothing else is stored is not observable through the API; AC-005 checks that an excusal has no time of its own. |
| FR-030 | AC-082 | The results and data of the earlier demo tasks are also checked by the locked F001, F004, F005 and F007 scenarios, run unchanged (FR-023). |
| FR-031 | AC-076, AC-077, AC-078, AC-079, AC-080, AC-081 | The new account's name, custom habits and time zone are checked only through its figures, which assume `UTC`. The case-insensitive look-up of a catalog habit and of an existing email is not checked. |
| US1-AS1 | AC-001 | |
| US1-AS2 | AC-002 | |
| US1-AS3 | AC-004 | |
| US1-AS4 | AC-006 | |
| US1-AS5 | AC-008 | |
| US1-AS6 | AC-009 | |
| US1-AS7 | AC-010 | |
| US1-AS8 | AC-017, AC-018 | |
| US1-AS9 | AC-021 | |
| US1-AS10 | AC-014, AC-015 | |
| US1-AS11 | AC-016 | |
| US1-AS12 | — | Not testable through the API: the signed-in Home's "Excuse" action, the Excuse dialog and the tile (page, HP-003, HP-005). |
| US2-AS1 | AC-023 | |
| US2-AS2 | AC-024 | |
| US2-AS3 | AC-025 | |
| US2-AS4 | AC-026 | |
| US2-AS5 | AC-042 | |
| US2-AS6 | AC-043 | |
| US2-AS7 | AC-048, AC-049 | |
| US2-AS8 | AC-038 | |
| US2-AS9 | AC-040 | |
| US2-AS10 | AC-041 | |
| US2-AS11 | AC-031, AC-032 | |
| US2-AS12 | AC-033, AC-034, AC-035 | |
| US2-AS13 | AC-052 | |
| US2-AS14 | — | Not testable through the API: the goal page's Pause action, PauseDialog, Notice and Pauses card (page, HP-001, HP-002). |
| US3-AS1 | AC-053 | |
| US3-AS2 | AC-054 | |
| US3-AS3 | AC-055 | |
| US3-AS4 | AC-056 | |
| US3-AS5 | AC-057 | |
| US3-AS6 | AC-058 | |
| US3-AS7 | AC-059, AC-060 | |
| US3-AS8 | — | Not testable through the API: the strip's cells, its legend and accessible label (page, HP-001, HP-006). |
| US4-AS1 | AC-066 | |
| US4-AS2 | AC-067 | |
| US4-AS3 | AC-068 | |
| US4-AS4 | AC-069 | |
| US4-AS5 | — | Not testable through the API: the Goals page's Tabs, Paused items and footnote (page, HP-004). |
| US4-AS6 | — | Not testable through the API: the Goals page's empty Paused tab (page, HP-004). |
| US5-AS1 | AC-072 | |
| US5-AS2 | AC-073 | |
| US5-AS3 | AC-074 | |
| US5-AS4 | AC-051 | |
| US5-AS5 | — | Not testable through the API: log lines, exception messages and error pages are host output (HP-009). |
| US6-AS1 | AC-076 | |
| US6-AS2 | AC-077, AC-078 | |
| US6-AS3 | AC-079 | |
| US6-AS4 | AC-080 | |
| US6-AS5 | AC-082 | |

Edge cases (spec "Edge Cases"), in the spec's order:

- Excusing and pausing the same date: a paused date cannot be excused (AC-011, AC-012); recording a date that is
  both excused and paused gives the pause's refusal (AC-022 row 01); a pause removed by resuming shows the
  excusal again (AC-037).
- A pause over today's begun entry: AC-023, AC-039, AC-041.
- Resuming early: AC-032, AC-035, AC-042, AC-045.
- One paused day in a week, also with the target reached: AC-036.
- A Paused week of a goal that is not paused: AC-033 (upcoming pause), AC-044 row 01, AC-045 (mid-week resume),
  AC-046 (dashboard and overview), AC-047 (cancelling the upcoming pause).
- A pause before the start date: AC-029 row 01 pauses a goal that has not started from a date before its start
  date. That the pause then covers only dates from the start date on is not checked separately.
- A goal that has not started: AC-029 (pausing), AC-086 (current or past), AC-087 (resuming).
- Time zone changes: not testable from this spec: the time zone change (slice 4 FR-026) is not restated in
  spec.md, so its inputs and results are not available to the acceptance tests. AC-030 checks that a pause's first
  day is compared with the current date on the goal's own timeline.
- Grace period around a clock change: not checked; AC-007 and AC-018 check the end of the window at its exact
  instant in `UTC`, and slice 5's acceptance criteria check grace ends across clock changes.
- Long pauses: AC-088 (a pause of more than three months; the streak before it unchanged). The edge case's "N/A"
  is not checked: spec.md's consistency counts every final result of the current attempt ("Earlier behaviour",
  Consistency), so a goal with counted results before the pause still has a percentage, and the edge case does
  not say which figure reads "N/A".
- Notes on Excused and Paused entries: AC-071.
- Several pauses, adjacent ones allowed: AC-026 row 04, AC-052.

Success criteria: SC-002 is checked by AC-053 to AC-059. SC-003: excusing just before, at and just after the grace
end (AC-007) and the start (AC-008 rows 01 and 02); removing just before, at and just after the grace end (AC-018); pausing before,
on and after the current date (AC-024, AC-023, AC-033, AC-030); resuming on the first day (AC-043) and later
(AC-042, AC-035); cancelling just before and at the first day (AC-050). SC-004: AC-038, AC-039. SC-005: AC-072
to AC-074, AC-051. SC-006 on the AC surface: AC-076 to AC-078; the hosted demo and the smoke check are outside it
(HP-011). SC-001 and SC-007 are page and accessibility outcomes outside the AC surface (HP-001 to HP-007).

## Acceptance criteria

### AC-001 (FR-001; FR-002; FR-017; US1-AS1) Excusing a Pending commitment with a reason

- Given the registered user `f8.ac001.user@example.com`, who created the goal `Yoga, 3 days a week`
- When at 2026-11-12T10:00:00Z `f8.ac001.user@example.com` excuses 2026-11-11 with the reason "Ill"
- Then the result is "excused" with the date 2026-11-11
- And the history of the goal from 2026-11-09 to 2026-11-15 gives 2026-11-11 Excused, no record, no grace end, correction no, reason "Ill", removable
- And what can be recorded now contains no entry for 2026-11-11

### AC-002 (FR-001; FR-002; US1-AS2) Excusing a Due commitment without a reason

- Given the registered user `f8.ac002.user@example.com`, who created the goal `Yoga, 3 days a week`
- When at 2026-11-13T07:00:00Z `f8.ac002.user@example.com` excuses 2026-11-13 without a reason
- Then the result is "excused" with the date 2026-11-13
- And the history of the goal from 2026-11-13 to 2026-11-13 gives 2026-11-13 Excused, no reason, removable
- And what can be recorded now contains no entry for 2026-11-13

### AC-003 (FR-001) The reason is trimmed, and a reason that is empty after trimming is none

- Given the registered user `f8.ac003.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
- When at 2026-11-12T10:00:00Z `f8.ac003.<n>.user@example.com` excuses 2026-11-11 with the reason <given>
- Then the result is "excused" with the date 2026-11-11
- And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Excused, <stored>

Examples:

| n | given | stored |
|---|---|---|
| 01 | "  Ill  " | reason "Ill" |
| 02 | "   " | no reason |

### AC-004 (FR-003; FR-017; US1-AS3) An excused commitment stays Excused after its grace period would have ended

- Given the registered user `f8.ac004.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-12T10:00:00Z `f8.ac004.<n>.user@example.com` excused 2026-11-11 with the reason "Ill"
- When at <time> `f8.ac004.<n>.user@example.com` reads the history of the goal from 2026-11-11 to 2026-11-11
- Then it gives 2026-11-11 Excused, no record, no grace end, correction no, reason "Ill", not removable

Examples:

| n | time |
|---|---|
| 01 | 2026-11-14T00:00:00Z |
| 02 | 2026-12-01T00:00:00Z |

### AC-005 (FR-003; FR-025) An excusal applies at every supplied time, also one earlier than the excusing call

- Given the registered user `f8.ac005.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-12T10:00:00Z `f8.ac005.user@example.com` excused 2026-11-11 without a reason
- When at 2026-11-12T09:00:00Z `f8.ac005.user@example.com` reads the history of the goal from 2026-11-11 to 2026-11-11
- Then it gives 2026-11-11 Excused, no reason

### AC-006 (FR-002; US1-AS4) A final commitment cannot be excused

- Given the registered user `f8.ac006.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
- And `f8.ac006.<n>.user@example.com` <setup>
- When at 2026-11-12T10:00:00Z `f8.ac006.<n>.user@example.com` excuses <date> without a reason
- Then the result is "This commitment is final and can no longer be excused."
- And the history of the goal from <date> to <date> gives <date> <state>

Examples:

| n | setup | date | state |
|---|---|---|---|
| 01 | records nothing | 2026-11-09 | Missed |
| 02 | records Yes for 2026-11-04 | 2026-11-04 | Met |
| 03 | records No for 2026-11-06 | 2026-11-06 | Failed |

### AC-007 (FR-002) Excusing just before, at and just after the end of the grace period

- Given the registered user `f8.ac007.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
- When at <time> `f8.ac007.<n>.user@example.com` excuses 2026-11-11 without a reason
- Then the result is <result>
- And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 <state>

Examples:

| n | time | result | state |
|---|---|---|---|
| 01 | 2026-11-13T23:59:59Z | "excused" | Excused |
| 02 | 2026-11-14T00:00:00Z | "This commitment is final and can no longer be excused." | Missed |
| 03 | 2026-11-14T00:00:01Z | "This commitment is final and can no longer be excused." | Missed |

### AC-008 (FR-002; US1-AS5) An Upcoming commitment cannot be excused, also with a reason that is too long

- Given the registered user `f8.ac008.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
- When at <time> `f8.ac008.<n>.user@example.com` excuses 2026-11-13 with the reason <reason>
- Then the result is <result>

Examples:

| n | time | reason | result |
|---|---|---|---|
| 01 | 2026-11-12T23:59:59Z | "Away" | "You cannot excuse a commitment that has not begun." |
| 02 | 2026-11-13T00:00:00Z | "Away" | "excused" |
| 03 | 2026-11-12T10:00:00Z | `x` repeated 201 times | "You cannot excuse a commitment that has not begun." |

### AC-009 (FR-002; US1-AS6) A commitment of a period-based goal cannot be excused

- Given the registered user `f8.ac009.<n>.user@example.com`, who created the goal <goal>
- When at 2026-11-12T10:00:00Z `f8.ac009.<n>.user@example.com` excuses 2026-11-11 without a reason
- Then the result is "Only commitments can be excused. This goal is evaluated by period."
- And the history of the goal from 2026-11-11 to 2026-11-11 gives <entry>

Examples:

| n | goal | entry |
|---|---|---|
| 01 | `Yoga, 3 times a week` | 2026-11-09 – 2026-11-15 Due |
| 02 | `Drinking water, 8 glasses a day` | 2026-11-11 Pending |

### AC-010 (FR-002; US1-AS7) A date that is not a commitment of the goal cannot be excused

- Given the registered user `f8.ac010.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
- When at 2026-11-12T10:00:00Z `f8.ac010.<n>.user@example.com` excuses <date> without a reason
- Then the result is "Nothing is scheduled on this date."

Examples:

| n | date |
|---|---|
| 01 | 2026-11-10 |
| 02 | 2026-10-30 |

### AC-011 (FR-002; FR-007) A date in a pause cannot be excused, also when it has not begun

- Given the registered user `f8.ac011.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-16T08:00:00Z `f8.ac011.<n>.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 without a reason
- When at 2026-11-16T10:00:00Z `f8.ac011.<n>.user@example.com` excuses <date> without a reason
- Then the result is "This goal is paused on this date."
- And the history of the goal from <date> to <date> gives <date> Paused

Examples:

| n | date |
|---|---|
| 01 | 2026-11-16 |
| 02 | 2026-11-18 |

### AC-012 (FR-002; FR-007) A commitment excused before a pause covered it is refused as paused, not as already excused

- Given the registered user `f8.ac012.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-16T07:00:00Z `f8.ac012.user@example.com` excused 2026-11-16 without a reason
- And at 2026-11-16T08:00:00Z `f8.ac012.user@example.com` paused the goal from 2026-11-16 until 2026-11-16 without a reason
- When at 2026-11-16T09:00:00Z `f8.ac012.user@example.com` excuses 2026-11-16 without a reason
- Then the result is "This goal is paused on this date."
- And the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Paused

### AC-013 (FR-002) A commitment that is already excused cannot be excused again

- Given the registered user `f8.ac013.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-12T10:00:00Z `f8.ac013.<n>.user@example.com` excused 2026-11-11 with the reason "Ill"
- When at <time> `f8.ac013.<n>.user@example.com` excuses 2026-11-11 with the reason "Away"
- Then the result is "This commitment is already excused."
- And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Excused, reason "Ill"

Examples:

| n | time |
|---|---|
| 01 | 2026-11-12T11:00:00Z |
| 02 | 2026-11-14T10:00:00Z |

### AC-014 (FR-002; US1-AS10) A commitment that has a record cannot be excused

- Given the registered user `f8.ac014.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-12T09:00:00Z `f8.ac014.<n>.user@example.com` recorded No for 2026-11-11
- When at 2026-11-12T10:00:00Z `f8.ac014.<n>.user@example.com` excuses 2026-11-11 with the reason <reason>
- Then the result is "This commitment has a record. Delete the record to excuse it."
- And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Pending, record No

Examples:

| n | reason |
|---|---|
| 01 | "Ill" |
| 02 | `x` repeated 201 times |

### AC-015 (FR-002; US1-AS10) After its record is deleted, a commitment can be excused

- Given the registered user `f8.ac015.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-12T09:00:00Z `f8.ac015.user@example.com` recorded No for 2026-11-11
- And at 2026-11-12T10:00:00Z `f8.ac015.user@example.com` deleted the record of 2026-11-11
- When at 2026-11-12T10:05:00Z `f8.ac015.user@example.com` excuses 2026-11-11 without a reason
- Then the result is "excused" with the date 2026-11-11
- And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Excused, no record

### AC-016 (FR-001; FR-002; US1-AS11) A reason has at most 200 characters

- Given the registered user `f8.ac016.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
- When at 2026-11-12T10:00:00Z `f8.ac016.<n>.user@example.com` excuses 2026-11-11 with the reason <reason>
- Then the result is <result>
- And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 <entry>

Examples:

| n | reason | result | entry |
|---|---|---|---|
| 01 | `x` repeated 200 times | "excused" | Excused, reason `x` repeated 200 times |
| 02 | `x` repeated 201 times | "Keep the reason to 200 characters or fewer." | Pending |

### AC-017 (FR-004; US1-AS8) An excusal can be removed while the grace period is open

- Given the registered user `f8.ac017.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-12T10:00:00Z `f8.ac017.user@example.com` excused 2026-11-11 with the reason "Ill"
- When at 2026-11-13T23:59:00Z `f8.ac017.user@example.com` removes the excusal of 2026-11-11
- Then the result is "excusal removed" with the state Pending
- And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Pending, no record, grace ends 2026-11-14T00:00:00Z
- And what can be recorded now contains 2026-11-11 Pending

### AC-018 (FR-004; US1-AS8) Removing an excusal just before, at and just after the end of the grace period

- Given the registered user `f8.ac018.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-12T10:00:00Z `f8.ac018.<n>.user@example.com` excused 2026-11-11 with the reason "Ill"
- When at <time> `f8.ac018.<n>.user@example.com` removes the excusal of 2026-11-11
- Then the result is <result>
- And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 <state>

Examples:

| n | time | result | state |
|---|---|---|---|
| 01 | 2026-11-13T23:59:59Z | "excusal removed" with the state Pending | Pending |
| 02 | 2026-11-14T00:00:00Z | "This excusal can no longer be removed." | Excused |
| 03 | 2026-11-14T00:00:01Z | "This excusal can no longer be removed." | Excused |

### AC-019 (FR-004) Removing an excusal that does not exist

- Given the registered user `f8.ac019.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
- When at 2026-11-12T10:00:00Z `f8.ac019.<n>.user@example.com` removes the excusal of <date>
- Then the result is "This commitment is not excused."
- And the history of the goal from <date> to <date> gives <date> <state>

Examples:

| n | date | state |
|---|---|---|
| 01 | 2026-11-11 | Pending |
| 02 | 2026-11-09 | Missed |

### AC-020 (FR-004; FR-024) A commitment whose excusal is removed is evaluated as if it had never been excused

- Given the registered user `f8.ac020.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-13T07:00:00Z `f8.ac020.user@example.com` excused 2026-11-13 without a reason
- And at 2026-11-13T08:00:00Z `f8.ac020.user@example.com` removed the excusal of 2026-11-13, with the result "excusal removed" and the state Due
- And at 2026-11-13T09:00:00Z `f8.ac020.user@example.com` recorded Yes for 2026-11-13
- When at 2026-11-16T00:00:00Z `f8.ac020.user@example.com` reads the history of the goal from 2026-11-13 to 2026-11-13
- Then it gives 2026-11-13 Met, record Yes

### AC-021 (FR-005; US1-AS9) An excused commitment cannot be recorded

- Given the registered user `f8.ac021.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-12T10:00:00Z `f8.ac021.user@example.com` excused 2026-11-11 with the reason "Ill"
- When at 2026-11-12T11:00:00Z `f8.ac021.user@example.com` records Yes for 2026-11-11
- Then the result is "This commitment is excused. Remove the excusal to record it."
- And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Excused, no record

### AC-022 (FR-005; FR-009) The record checks for a pause and an excusal come after the date check, the pause first

- Given the registered user `f8.ac022.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-16T07:00:00Z `f8.ac022.<n>.user@example.com` excused 2026-11-16 without a reason
- And at 2026-11-16T08:00:00Z `f8.ac022.<n>.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 without a reason
- When at 2026-11-17T10:00:00Z `f8.ac022.<n>.user@example.com` records Yes for <date>
- Then the result is <result>
- And the history of the goal from 2026-11-16 to 2026-11-18 gives 2026-11-16 Paused, no record and 2026-11-18 Paused, no record, and no entry for 2026-11-17

Examples:

| n | date | result |
|---|---|---|
| 01 | 2026-11-16 | "This goal is paused on this date." |
| 02 | 2026-11-17 | "Nothing is scheduled on this date." |

### AC-023 (FR-006; FR-007; FR-018; US2-AS1) Pausing a goal from today

- Given the registered user `f8.ac023.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
- When at 2026-11-16T09:00:00Z `f8.ac023.user@example.com` pauses the goal from 2026-11-16 until 2026-11-22 with the reason "Family visit"
- Then the result is "paused" with the first day 2026-11-16 and the last day 2026-11-22
- And the goals overview gives `Drinking water, 8 glasses a day` paused until 2026-11-22
- And the history of the goal from 2026-11-15 to 2026-11-16 gives 2026-11-15 Pending and 2026-11-16 Paused, no grace end
- And what can be recorded now contains 2026-11-15 Pending and no entry for 2026-11-16

### AC-024 (FR-006; US2-AS2) A pause cannot start before today

- Given the registered user `f8.ac024.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
- When at 2026-11-16T09:00:00Z `f8.ac024.user@example.com` pauses the goal from 2026-11-15 until 2026-11-22 without a reason
- Then the result is "A pause cannot start before today."
- And the pauses of the goal are empty

### AC-025 (FR-006; US2-AS3) The last day cannot be before the first day

- Given the registered user `f8.ac025.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
- When at 2026-11-16T09:00:00Z `f8.ac025.user@example.com` pauses the goal from 2026-11-20 until 2026-11-19 without a reason
- Then the result is "The last day cannot be before the first day."
- And the pauses of the goal are empty

### AC-026 (FR-006; US2-AS4) A pause cannot share a date with another pause of the goal, but may touch it

- Given the registered user `f8.ac026.<n>.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
- And at 2026-11-16T09:00:00Z `f8.ac026.<n>.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 with the reason "Family visit"
- When at 2026-11-16T10:00:00Z `f8.ac026.<n>.user@example.com` pauses the goal from <first> until <last> without a reason
- Then the result is <result>

Examples:

| n | first | last | result |
|---|---|---|---|
| 01 | 2026-11-21 | 2026-11-24 | "This goal already has a pause on these dates." |
| 02 | 2026-11-22 | 2026-11-22 | "This goal already has a pause on these dates." |
| 03 | 2026-11-17 | 2026-11-18 | "This goal already has a pause on these dates." |
| 04 | 2026-11-23 | 2026-11-29 | "paused" with the first day 2026-11-23 and the last day 2026-11-29 |

### AC-027 (FR-006) A pause needs a first and a last day

- Given the registered user `f8.ac027.<n>.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
- When at 2026-11-16T09:00:00Z `f8.ac027.<n>.user@example.com` pauses the goal <days> without a reason
- Then the result is "Choose the first and the last day."
- And the pauses of the goal are empty

Examples:

| n | days |
|---|---|
| 01 | with no first day until 2026-11-22 |
| 02 | from 2026-11-16 with no last day |
| 03 | with no first day and no last day |

### AC-028 (FR-006) The pause checks come in their order

- Given the registered user `f8.ac028.<n>.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
- And at 2026-11-16T08:00:00Z `f8.ac028.<n>.user@example.com` paused the goal from 2026-11-23 until 2026-11-25 without a reason
- When at 2026-11-16T09:00:00Z `f8.ac028.<n>.user@example.com` pauses the goal <days> with the reason <reason>
- Then the result is <result>

Examples:

| n | days | reason | result |
|---|---|---|---|
| 01 | from 2026-11-15 with no last day | "Away" | "Choose the first and the last day." |
| 02 | from 2026-11-15 until 2026-11-14 | "Away" | "A pause cannot start before today." |
| 03 | from 2026-11-24 until 2026-11-23 | `x` repeated 201 times | "The last day cannot be before the first day." |
| 04 | from 2026-11-24 until 2026-11-30 | `x` repeated 201 times | "This goal already has a pause on these dates." |
| 05 | from 2026-11-17 until 2026-11-18 | `x` repeated 201 times | "Keep the reason to 200 characters or fewer." |
| 06 | from 2026-11-17 until 2026-11-18 | `x` repeated 200 times | "paused" with the first day 2026-11-17 and the last day 2026-11-18 |

### AC-029 (FR-006) For a goal that has not started, "today" is the account's today

- Given the registered user `f8.ac029.<n>.user@example.com`, who created the goal `Drinking water, 8 glasses a day` at 2026-11-16T08:00:00Z with the start date 2026-11-20
- When at 2026-11-16T09:00:00Z `f8.ac029.<n>.user@example.com` pauses the goal from <first> until 2026-11-18 without a reason
- Then the result is <result>

Examples:

| n | first | result |
|---|---|---|
| 01 | 2026-11-16 | "paused" with the first day 2026-11-16 and the last day 2026-11-18 |
| 02 | 2026-11-15 | "A pause cannot start before today." |

### AC-030 (FR-006) The first day is compared with the current date on the goal's own timeline

- Given the registered user `f8.ac030.<n>.user@example.com` in `<zone>`, who created the goal `Drinking water, 8 glasses a day`
- When at 2026-11-15T23:00:00Z `f8.ac030.<n>.user@example.com` pauses the goal from 2026-11-15 until 2026-11-20 without a reason
- Then the result is <result>

Examples:

| n | zone | result |
|---|---|---|
| 01 | UTC | "paused" with the first day 2026-11-15 and the last day 2026-11-20 |
| 02 | Europe/Kyiv | "A pause cannot start before today." |

### AC-031 (FR-006; FR-007; US2-AS11) Pausing a commitment-based goal makes its commitments in the range Paused

- Given the registered user `f8.ac031.user@example.com`, who created the goal `Yoga, 3 days a week`
- When at 2026-11-18T08:00:00Z `f8.ac031.user@example.com` pauses the goal from 2026-11-18 until 2026-11-20 without a reason
- Then the result is "paused" with the first day 2026-11-18 and the last day 2026-11-20
- And the history of the goal from 2026-11-16 to 2026-11-22 gives 2026-11-16 Pending, 2026-11-18 Paused and 2026-11-20 Paused

### AC-032 (FR-010; US2-AS11) Resuming a commitment-based goal makes today's commitment Due again

- Given the registered user `f8.ac032.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-18T08:00:00Z `f8.ac032.user@example.com` paused the goal from 2026-11-18 until 2026-11-20 without a reason
- When at 2026-11-20T08:00:00Z `f8.ac032.user@example.com` resumes the goal
- Then the result is "resumed"
- And the pauses of the goal are (2026-11-18, 2026-11-19, no reason, past)
- And the history of the goal from 2026-11-18 to 2026-11-20 gives 2026-11-18 Paused and 2026-11-20 Due

### AC-033 (FR-007; FR-018; US2-AS12) A per-week period is Paused as a whole while one of its dates is paused

- Given the registered user `f8.ac033.user@example.com`, who created the goal `Yoga, 3 times a week`
- When at 2026-11-18T08:00:00Z `f8.ac033.user@example.com` pauses the goal from 2026-11-19 until 2026-11-25 without a reason
- Then the result is "paused" with the first day 2026-11-19 and the last day 2026-11-25
- And the history of the goal from 2026-11-16 to 2026-11-29 gives 2026-11-16 – 2026-11-22 Paused and 2026-11-23 – 2026-11-29 Paused
- And the pauses of the goal are (2026-11-19, 2026-11-25, no reason, upcoming)
- And the goals overview gives `Yoga, 3 times a week` not paused

### AC-034 (FR-005; FR-009; US2-AS12) A date of a Paused week that is not itself paused cannot be recorded

- Given the registered user `f8.ac034.user@example.com`, who created the goal `Yoga, 3 times a week`
- And at 2026-11-18T08:00:00Z `f8.ac034.user@example.com` paused the goal from 2026-11-19 until 2026-11-25 without a reason
- When at 2026-11-18T09:00:00Z `f8.ac034.user@example.com` records Yes for 2026-11-18
- Then the result is "This week is paused."
- And the history of the goal from 2026-11-16 to 2026-11-22 gives 2026-11-16 – 2026-11-22 Paused, no record

### AC-035 (FR-010; US2-AS12) Resuming a per-week goal in a new week leaves the earlier partly paused week Paused

- Given the registered user `f8.ac035.user@example.com`, who created the goal `Yoga, 3 times a week`
- And at 2026-11-18T08:00:00Z `f8.ac035.user@example.com` paused the goal from 2026-11-19 until 2026-11-25 without a reason
- When at 2026-11-23T10:00:00Z `f8.ac035.user@example.com` resumes the goal
- Then the result is "resumed"
- And the pauses of the goal are (2026-11-19, 2026-11-22, no reason, past)
- And the history of the goal from 2026-11-16 to 2026-11-29 gives 2026-11-16 – 2026-11-22 Paused and 2026-11-23 – 2026-11-29 Due

### AC-036 (FR-007; FR-013) One paused day exempts its whole week, also when its records reach the target

- Given the registered user `f8.ac036.user@example.com`, who created the goal `Yoga, 3 times a week`
- And at 2026-11-18T09:00:00Z `f8.ac036.user@example.com` recorded Yes for each date from 2026-11-16 to 2026-11-18
- And at 2026-11-18T10:00:00Z `f8.ac036.user@example.com` paused the goal from 2026-11-22 until 2026-11-22 without a reason
- When at 2026-11-25T12:00:00Z `f8.ac036.user@example.com` reads the history of the goal from 2026-11-16 to 2026-11-22
- Then it gives 2026-11-16 – 2026-11-22 Paused, records Yes, Yes, Yes
- And the report of the goal gives the consistency "0% (0 of 2)"

### AC-037 (FR-007; FR-010) An excusal under a pause is kept and shows again when the pause is removed

- Given the registered user `f8.ac037.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-16T07:00:00Z `f8.ac037.user@example.com` excused 2026-11-16 with the reason "Ill"
- And at 2026-11-16T08:00:00Z `f8.ac037.user@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
- When at 2026-11-16T09:00:00Z `f8.ac037.user@example.com` resumes the goal
- Then the result is "resumed"
- And the pauses of the goal are empty
- And the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Excused, reason "Ill"

### AC-038 (FR-008; US2-AS8) A pause never changes a result that was Pending or final when it was set

- Given the registered user `f8.ac038.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
- And at 2026-11-13T20:00:00Z `f8.ac038.user@example.com` recorded 6 for 2026-11-13
- And at 2026-11-14T20:00:00Z `f8.ac038.user@example.com` recorded 8 for 2026-11-14
- When at 2026-11-16T09:00:00Z `f8.ac038.user@example.com` pauses the goal from 2026-11-16 until 2026-11-22 without a reason
- Then the result is "paused" with the first day 2026-11-16 and the last day 2026-11-22
- And the history of the goal from 2026-11-13 to 2026-11-16 gives 2026-11-13 Failed, 2026-11-14 Pending, 2026-11-15 Pending and 2026-11-16 Paused
- And at 2026-11-18T12:00:00Z the history of the goal from 2026-11-13 to 2026-11-15 gives 2026-11-13 Failed, 2026-11-14 Met and 2026-11-15 Missed

### AC-039 (FR-008; FR-009) A Due entry whose records already exceed a Reduce limit becomes Paused and keeps its records

- Given the registered user `f8.ac039.user@example.com`, who created the goal `Sugar, at most 1 sweet a day`
- And at 2026-11-16T08:00:00Z `f8.ac039.user@example.com` recorded 3 for 2026-11-16
- When at 2026-11-16T09:00:00Z `f8.ac039.user@example.com` pauses the goal from 2026-11-16 until 2026-11-16 without a reason
- Then the result is "paused" with the first day 2026-11-16 and the last day 2026-11-16
- And the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Paused, record 3
- And at 2026-11-19T00:00:00Z the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Paused, record 3

### AC-040 (FR-009; US2-AS9) A date in a pause cannot be recorded

- Given the registered user `f8.ac040.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
- And at 2026-11-16T09:00:00Z `f8.ac040.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 without a reason
- When at 2026-11-17T10:00:00Z `f8.ac040.user@example.com` records 8 for 2026-11-17
- Then the result is "This goal is paused on this date."
- And the history of the goal from 2026-11-17 to 2026-11-17 gives 2026-11-17 Paused, no record

### AC-041 (FR-009; FR-017; US2-AS10) A record kept on a Paused date can be changed or deleted, and the date stays Paused

- Given the registered user `f8.ac041.<n>.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
- And at 2026-11-16T08:00:00Z `f8.ac041.<n>.user@example.com` recorded 8 for 2026-11-16
- And at 2026-11-16T09:00:00Z `f8.ac041.<n>.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 without a reason
- When at 2026-11-16T10:00:00Z `f8.ac041.<n>.user@example.com` <action>
- Then the result is the state Paused, correction no
- And the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Paused, <records>, correction no

Examples:

| n | action | records |
|---|---|---|
| 01 | changes the record of 2026-11-16 to 9 | record 9 |
| 02 | deletes the record of 2026-11-16 | no record |

### AC-042 (FR-010; FR-012; FR-018; US2-AS5) Resuming during a pause ends it on the day before today

- Given the registered user `f8.ac042.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
- And at 2026-11-16T09:00:00Z `f8.ac042.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 with the reason "Family visit"
- When at 2026-11-18T08:00:00Z `f8.ac042.user@example.com` resumes the goal
- Then the result is "resumed"
- And the pauses of the goal are (2026-11-16, 2026-11-17, reason "Family visit", past)
- And the history of the goal from 2026-11-16 to 2026-11-18 gives 2026-11-16 Paused, 2026-11-17 Paused and 2026-11-18 Due
- And what can be recorded now contains 2026-11-18 Due
- And the goals overview gives `Drinking water, 8 glasses a day` not paused

### AC-043 (FR-010; FR-024; US2-AS6) Resuming on a pause's first day removes the pause

- Given the registered user `f8.ac043.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
- And at 2026-11-16T08:00:00Z `f8.ac043.user@example.com` recorded 8 for 2026-11-16
- And at 2026-11-16T09:00:00Z `f8.ac043.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 with the reason "Family visit"
- When at 2026-11-16T18:00:00Z `f8.ac043.user@example.com` resumes the goal
- Then the result is "resumed"
- And the pauses of the goal are empty
- And the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Due, record 8
- And at 2026-11-19T00:00:00Z the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Met, record 8

### AC-044 (FR-010) A goal without a current pause cannot be resumed

- Given the registered user `f8.ac044.<n>.user@example.com`, who created the goal `Yoga, 3 times a week`
- And at 2026-11-16T08:00:00Z `f8.ac044.<n>.user@example.com` paused the goal from <first> until <last> without a reason
- When at <time> `f8.ac044.<n>.user@example.com` resumes the goal
- Then the result is "This goal is not paused."
- And the pauses of the goal are (<first>, <last>, no reason, <kind>)

Examples:

| n | first | last | time | kind |
|---|---|---|---|---|
| 01 | 2026-11-19 | 2026-11-20 | 2026-11-16T10:00:00Z | upcoming |
| 02 | 2026-11-16 | 2026-11-16 | 2026-11-17T10:00:00Z | past |

### AC-045 (FR-007; FR-010; FR-018) Resuming in the middle of a week leaves that week Paused, while the goal is not paused

- Given the registered user `f8.ac045.user@example.com`, who created the goal `Yoga, 3 times a week`
- And at 2026-11-16T08:00:00Z `f8.ac045.user@example.com` paused the goal from 2026-11-16 until 2026-11-29 without a reason
- When at 2026-11-18T10:00:00Z `f8.ac045.user@example.com` resumes the goal
- Then the result is "resumed"
- And the pauses of the goal are (2026-11-16, 2026-11-17, no reason, past)
- And the history of the goal from 2026-11-16 to 2026-11-29 gives 2026-11-16 – 2026-11-22 Paused and 2026-11-23 – 2026-11-29 Upcoming
- And the goals overview gives `Yoga, 3 times a week` not paused

### AC-046 (FR-007; FR-018; FR-019) A pause later in the current week pauses that week, while the goal is not paused and is counted

- Given the registered user `f8.ac046.user@example.com`, who created the goal `Yoga, 3 times a week`
- And at 2026-11-16T08:00:00Z `f8.ac046.user@example.com` paused the goal from 2026-11-19 until 2026-11-20 without a reason
- When at 2026-11-16T09:00:00Z `f8.ac046.user@example.com` reads the dashboard
- Then the number of goals is 1
- And the Today list contains (`Yoga, 3 times a week`, 2026-11-09 – 2026-11-15, Pending) and no entry for 2026-11-16 – 2026-11-22
- And the summary of `Yoga, 3 times a week` says it is not paused
- And the goals overview gives `Yoga, 3 times a week` not paused

### AC-047 (FR-011; FR-024) Cancelling the upcoming pause that paused the current week makes the week Due again

- Given the registered user `f8.ac047.user@example.com`, who created the goal `Yoga, 3 times a week`
- And at 2026-11-16T08:00:00Z `f8.ac047.user@example.com` paused the goal from 2026-11-19 until 2026-11-20 without a reason
- When at 2026-11-16T09:00:00Z `f8.ac047.user@example.com` cancels the pause 2026-11-19 – 2026-11-20
- Then the result is "pause cancelled"
- And the pauses of the goal are empty
- And the history of the goal from 2026-11-16 to 2026-11-22 gives 2026-11-16 – 2026-11-22 Due

### AC-048 (FR-011; US2-AS7) An upcoming pause can be cancelled

- Given the registered user `f8.ac048.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
- And at 2026-11-16T08:00:00Z `f8.ac048.user@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
- When at 2026-11-18T10:00:00Z `f8.ac048.user@example.com` cancels the pause 2026-11-23 – 2026-11-27
- Then the result is "pause cancelled"
- And the pauses of the goal are empty

### AC-049 (FR-011; US2-AS7) A current or past pause cannot be cancelled

- Given the registered user `f8.ac049.<n>.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
- And at 2026-11-16T09:00:00Z `f8.ac049.<n>.user@example.com` paused the goal from 2026-11-16 until <last> with the reason "Family visit"
- When at 2026-11-18T10:00:00Z `f8.ac049.<n>.user@example.com` cancels the pause 2026-11-16 – <last>
- Then the result is "Only a pause that has not begun can be cancelled. Resume the goal instead."
- And the pauses of the goal are (2026-11-16, <last>, reason "Family visit", <kind>)

Examples:

| n | last | kind |
|---|---|---|
| 01 | 2026-11-22 | current |
| 02 | 2026-11-17 | past |

### AC-050 (FR-011) Cancelling just before and on a pause's first day

- Given the registered user `f8.ac050.<n>.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
- And at 2026-11-16T08:00:00Z `f8.ac050.<n>.user@example.com` paused the goal from 2026-11-17 until 2026-11-20 without a reason
- When at <time> `f8.ac050.<n>.user@example.com` cancels the pause 2026-11-17 – 2026-11-20
- Then the result is <result>

Examples:

| n | time | result |
|---|---|---|
| 01 | 2026-11-16T23:59:59Z | "pause cancelled" |
| 02 | 2026-11-17T00:00:00Z | "Only a pause that has not begun can be cancelled. Resume the goal instead." |

### AC-051 (FR-011; FR-021; US5-AS4) A pause is found only through its own goal

- Given the registered user `f8.ac051.<n>.a@example.com`, who created the goal `Yoga, 3 days a week` and the goal `Drinking water, 8 glasses a day`
- And at 2026-11-16T08:00:00Z `f8.ac051.<n>.a@example.com` paused `Yoga, 3 days a week` from 2026-11-16 until 2026-11-20 without a reason
- And at 2026-11-16T08:01:00Z `f8.ac051.<n>.a@example.com` paused `Yoga, 3 days a week` from 2026-11-23 until 2026-11-27 without a reason
- And the registered user `f8.ac051.<n>.b@example.com`, who created the goal `Drinking water, 8 glasses a day`
- When at 2026-11-16T09:00:00Z `<actor>` cancels the pause <pause> through their goal `Drinking water, 8 glasses a day`
- Then the result is "This pause does not exist."
- And the pauses of `Yoga, 3 days a week` of `f8.ac051.<n>.a@example.com` are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)

Examples:

| n | actor | pause |
|---|---|---|
| 01 | f8.ac051.01.a@example.com | 2026-11-23 – 2026-11-27 |
| 02 | f8.ac051.02.b@example.com | 2026-11-23 – 2026-11-27 |
| 03 | f8.ac051.03.a@example.com | 2026-11-16 – 2026-11-20 |
| 04 | f8.ac051.04.b@example.com | 2026-11-16 – 2026-11-20 |

### AC-052 (FR-012; US2-AS13) The pauses of a goal are listed with the latest first day first

- Given the registered user `f8.ac052.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
- And at 2026-11-16T09:00:00Z `f8.ac052.user@example.com` paused the goal from 2026-11-16 until 2026-11-17 with the reason "Ill"
- And at 2026-11-16T09:01:00Z `f8.ac052.user@example.com` paused the goal from 2026-11-19 until 2026-11-22 with the reason "Family visit"
- And at 2026-11-16T09:02:00Z `f8.ac052.user@example.com` paused the goal from 2026-11-25 until 2026-11-26 without a reason
- When at 2026-11-20T10:00:00Z `f8.ac052.user@example.com` reads the pauses of the goal
- Then they are, in this order, (2026-11-25, 2026-11-26, no reason, upcoming), (2026-11-19, 2026-11-22, reason "Family visit", current) and (2026-11-16, 2026-11-17, reason "Ill", past)

### AC-053 (FR-013; US3-AS1) Consistency leaves Excused and Paused results out

- Given the registered user `f8.ac053.user@example.com`, who created the goal `Meditation, 10 min daily`
- And at 2026-11-10T08:00:00Z `f8.ac053.user@example.com` excused 2026-11-09 without a reason
- And at 2026-11-13T08:00:00Z `f8.ac053.user@example.com` paused the goal from 2026-11-13 until 2026-11-15 without a reason
- And `f8.ac053.user@example.com` records 10 for each date from 2026-11-02 to 2026-11-16 except 2026-11-09, 2026-11-11, 2026-11-13, 2026-11-14 and 2026-11-15
- When at 2026-11-19T12:00:00Z `f8.ac053.user@example.com` reads the report of the goal
- Then the consistency is "90% (10 of 11)"

### AC-054 (FR-014; US3-AS2) Excused and Paused results neither extend nor reset a streak

- Given the registered user `f8.ac054.user@example.com`, who created the goal `Meditation, 10 min daily`
- And at 2026-11-10T08:00:00Z `f8.ac054.user@example.com` excused 2026-11-09 without a reason
- And at 2026-11-13T08:00:00Z `f8.ac054.user@example.com` paused the goal from 2026-11-13 until 2026-11-15 without a reason
- And `f8.ac054.user@example.com` records 10 for each date from 2026-11-02 to 2026-11-16 except 2026-11-09, 2026-11-11, 2026-11-13, 2026-11-14 and 2026-11-15
- When at 2026-11-19T12:00:00Z `f8.ac054.user@example.com` reads the report of the goal
- Then the current streak is "2 commitments"
- And the longest streak is "8 commitments"

### AC-055 (FR-015; US3-AS3) Recent results include Excused and Paused results and count each state

- Given the registered user `f8.ac055.user@example.com`, who created the goal `Meditation, 10 min daily`
- And at 2026-11-10T08:00:00Z `f8.ac055.user@example.com` excused 2026-11-09 without a reason
- And at 2026-11-13T08:00:00Z `f8.ac055.user@example.com` paused the goal from 2026-11-13 until 2026-11-15 without a reason
- And `f8.ac055.user@example.com` records 10 for each date from 2026-11-02 to 2026-11-16 except 2026-11-09, 2026-11-11, 2026-11-13, 2026-11-14 and 2026-11-15
- When at 2026-11-19T12:00:00Z `f8.ac055.user@example.com` reads the report of the goal
- Then the recent results are, oldest first: 2026-11-02 to 2026-11-08 each Met; 2026-11-09 Excused; 2026-11-10 Met; 2026-11-11 Missed; 2026-11-12 Met; 2026-11-13, 2026-11-14 and 2026-11-15 each Paused; 2026-11-16 Met; 2026-11-17 and 2026-11-18 each Pending
- And the number of each state among them is Met 10, Missed 1, Excused 1, Paused 3, Pending 2

### AC-056 (FR-016; US3-AS4) The trend marks Excused and Paused bars, which have no value

- Given the registered user `f8.ac056.user@example.com`, who created the goal `Meditation, 10 min daily`
- And at 2026-11-10T08:00:00Z `f8.ac056.user@example.com` excused 2026-11-09 without a reason
- And at 2026-11-13T08:00:00Z `f8.ac056.user@example.com` paused the goal from 2026-11-13 until 2026-11-15 without a reason
- And `f8.ac056.user@example.com` records 10 for each date from 2026-11-02 to 2026-11-16 except 2026-11-09, 2026-11-11, 2026-11-13, 2026-11-14 and 2026-11-15
- When at 2026-11-19T12:00:00Z `f8.ac056.user@example.com` reads the report of the goal
- Then the trend is, with the target 10: (2026-11-06, 10), (2026-11-07, 10), (2026-11-08, 10), (2026-11-09, Excused), (2026-11-10, 10), (2026-11-11, no value), (2026-11-12, 10), (2026-11-13, Paused), (2026-11-14, Paused), (2026-11-15, Paused), (2026-11-16, 10), (2026-11-17, no value), (2026-11-18, no value), (2026-11-19, no value)

### AC-057 (FR-004; FR-013; FR-014; FR-024; US3-AS5) Removing an excusal and recording the commitment late gives the figures of an on-time record

- Given the registered user `f8.ac057.user@example.com`, who created the goal `Meditation, 10 min daily`
- And at 2026-11-10T08:00:00Z `f8.ac057.user@example.com` excused 2026-11-09 without a reason
- And at 2026-11-10T12:00:00Z `f8.ac057.user@example.com` removed the excusal of 2026-11-09
- And at 2026-11-13T08:00:00Z `f8.ac057.user@example.com` paused the goal from 2026-11-13 until 2026-11-15 without a reason
- And `f8.ac057.user@example.com` records 10 for each date from 2026-11-02 to 2026-11-16 except 2026-11-09, 2026-11-11, 2026-11-13, 2026-11-14 and 2026-11-15
- And `f8.ac057.user@example.com` records 4 for 2026-11-09
- When at 2026-11-19T12:00:00Z `f8.ac057.user@example.com` reads the report of the goal
- Then the consistency is "83% (10 of 12)"
- And the longest streak is "7 commitments"
- And the current streak is "2 commitments"

### AC-058 (FR-013; FR-014; US3-AS6) A goal whose every ended result is exempt has "N/A" and streaks of 0

- Given the registered user `f8.ac058.user@example.com`, who created the goal `Meditation, 10 min daily`
- And at 2026-11-03T08:00:00Z `f8.ac058.user@example.com` excused 2026-11-02 without a reason
- And at 2026-11-03T08:00:00Z `f8.ac058.user@example.com` paused the goal from 2026-11-03 until 2026-11-08 without a reason
- When at 2026-11-09T12:00:00Z `f8.ac058.user@example.com` reads the report of the goal
- Then the consistency is "N/A"
- And the current streak is "0 commitments"
- And the longest streak is "0 commitments"

### AC-059 (FR-013; FR-014; US3-AS7) A Paused week of a per-week goal is not counted and does not break the streak

- Given the registered user `f8.ac059.user@example.com`, who created the goal `Yoga, 3 times a week`
- And at 2026-11-09T08:00:00Z `f8.ac059.user@example.com` paused the goal from 2026-11-09 until 2026-11-15 without a reason
- And `f8.ac059.user@example.com` records Yes for each date from 2026-11-02 to 2026-11-04
- And `f8.ac059.user@example.com` records Yes for each date from 2026-11-16 to 2026-11-18
- When at 2026-11-25T12:00:00Z `f8.ac059.user@example.com` reads the report of the goal
- Then the consistency is "100% (2 of 2)"
- And the current streak is "2 weeks"
- And the longest streak is "2 weeks"

### AC-060 (FR-016; US3-AS7) The trend of a per-week goal marks a Paused week

- Given the registered user `f8.ac060.user@example.com`, who created the goal `Yoga, 3 times a week`
- And at 2026-11-09T08:00:00Z `f8.ac060.user@example.com` paused the goal from 2026-11-09 until 2026-11-15 without a reason
- And `f8.ac060.user@example.com` records Yes for each date from 2026-11-02 to 2026-11-04
- And `f8.ac060.user@example.com` records Yes for each date from 2026-11-16 to 2026-11-18
- When at 2026-11-25T12:00:00Z `f8.ac060.user@example.com` reads the report of the goal
- Then the trend is, with the target 3: (2026-11-02 – 2026-11-08, 3), (2026-11-09 – 2026-11-15, Paused), (2026-11-16 – 2026-11-22, 3), (2026-11-23 – 2026-11-29, no value)

### AC-061 (FR-016) The trend of a per-day goal marks its Paused days

- Given the registered user `f8.ac061.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
- And at 2026-11-16T09:00:00Z `f8.ac061.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 without a reason
- And `f8.ac061.user@example.com` records 8 for each date from 2026-11-02 to 2026-11-15
- When at 2026-11-19T10:00:00Z `f8.ac061.user@example.com` reads the report of the goal
- Then the trend is, with the target 1: (2026-11-06, 1), (2026-11-07, 1), (2026-11-08, 1), (2026-11-09, 1), (2026-11-10, 1), (2026-11-11, 1), (2026-11-12, 1), (2026-11-13, 1), (2026-11-14, 1), (2026-11-15, 1), (2026-11-16, Paused), (2026-11-17, Paused), (2026-11-18, Paused), (2026-11-19, Paused)

### AC-062 (FR-016) A yes/no week's target and value leave a Paused commitment out

- Given the registered user `f8.ac062.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-16T08:00:00Z `f8.ac062.user@example.com` recorded Yes for 2026-11-16
- And at 2026-11-16T09:00:00Z `f8.ac062.user@example.com` paused the goal from 2026-11-16 until 2026-11-16 without a reason
- And `f8.ac062.user@example.com` records Yes for 2026-11-09, 2026-11-11, 2026-11-13, 2026-11-18 and 2026-11-20
- When at 2026-11-23T10:00:00Z `f8.ac062.user@example.com` reads the report of the goal
- Then the trend is (2026-11-02 – 2026-11-08, no value, target 3), (2026-11-09 – 2026-11-15, 3, target 3), (2026-11-16 – 2026-11-22, 2, target 2), (2026-11-23 – 2026-11-29, no value, target 3)

### AC-063 (FR-016) A yes/no week whose only record is on a Paused commitment has no value

- Given the registered user `f8.ac063.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-16T08:00:00Z `f8.ac063.user@example.com` recorded Yes for 2026-11-16
- And at 2026-11-16T09:00:00Z `f8.ac063.user@example.com` paused the goal from 2026-11-16 until 2026-11-16 without a reason
- When at 2026-11-23T10:00:00Z `f8.ac063.user@example.com` reads the report of the goal
- Then the trend contains (2026-11-16 – 2026-11-22, no value, target 2)

### AC-064 (FR-016) A yes/no week whose commitments are all Excused is marked Excused

- Given the registered user `f8.ac064.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-16T07:00:00Z `f8.ac064.user@example.com` excused 2026-11-16 without a reason
- And at 2026-11-18T07:00:00Z `f8.ac064.user@example.com` excused 2026-11-18 without a reason
- And at 2026-11-20T07:00:00Z `f8.ac064.user@example.com` excused 2026-11-20 without a reason
- When at 2026-11-23T10:00:00Z `f8.ac064.user@example.com` reads the report of the goal
- Then the trend contains (2026-11-16 – 2026-11-22, Excused)

### AC-065 (FR-016) A yes/no week whose commitments are all Excused or Paused, at least one Paused, is marked Paused

- Given the registered user `f8.ac065.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-16T07:00:00Z `f8.ac065.user@example.com` excused 2026-11-16 without a reason
- And at 2026-11-16T08:00:00Z `f8.ac065.user@example.com` paused the goal from 2026-11-17 until 2026-11-22 without a reason
- When at 2026-11-23T10:00:00Z `f8.ac065.user@example.com` reads the report of the goal
- Then the trend contains (2026-11-16 – 2026-11-22, Paused)

### AC-066 (FR-018; US4-AS1) The goals overview says which goal is paused and until when

- Given the registered user `f8.ac066.user@example.com`, who created the goal `Drinking water, 8 glasses a day` and the goal `Meditation, 10 min daily`
- And at 2026-11-16T09:00:00Z `f8.ac066.user@example.com` paused `Drinking water, 8 glasses a day` from 2026-11-16 until 2026-11-22 without a reason
- And `f8.ac066.user@example.com` records on target for each date from 2026-11-02 to 2026-11-13: 8 for `Drinking water, 8 glasses a day` and 10 for `Meditation, 10 min daily`
- When at 2026-11-16T10:00:00Z `f8.ac066.user@example.com` reads the goals overview
- Then it gives `Drinking water, 8 glasses a day` paused until 2026-11-22
- And it gives `Meditation, 10 min daily` not paused

### AC-067 (FR-019; US4-AS2) The dashboard expects nothing of a paused goal and does not count it

- Given the registered user `f8.ac067.user@example.com`, who created the goal `Drinking water, 8 glasses a day` and the goal `Meditation, 10 min daily`
- And at 2026-11-16T09:00:00Z `f8.ac067.user@example.com` paused `Drinking water, 8 glasses a day` from 2026-11-16 until 2026-11-22 without a reason
- And `f8.ac067.user@example.com` records on target for each date from 2026-11-02 to 2026-11-13: 8 for `Drinking water, 8 glasses a day` and 10 for `Meditation, 10 min daily`
- When at 2026-11-16T10:00:00Z `f8.ac067.user@example.com` reads the dashboard
- Then the number of goals is 1
- And the Due count is 1 and the Pending count is 4
- And the Today list contains (`Drinking water, 8 glasses a day`, 2026-11-14, Pending), (`Drinking water, 8 glasses a day`, 2026-11-15, Pending) and (`Meditation, 10 min daily`, 2026-11-16, Due)
- And the Today list contains no Due and no Upcoming entry of `Drinking water, 8 glasses a day`
- And the summary of `Drinking water, 8 glasses a day` says it is paused, and the summary of `Meditation, 10 min daily` says it is not paused

### AC-068 (FR-019; US4-AS3) An excused commitment leaves the Pending count and the Today list

- Given the registered user `f8.ac068.user@example.com`, who created the goal `Drinking water, 8 glasses a day` and the goal `Meditation, 10 min daily`
- And at 2026-11-16T09:00:00Z `f8.ac068.user@example.com` paused `Drinking water, 8 glasses a day` from 2026-11-16 until 2026-11-22 without a reason
- And `f8.ac068.user@example.com` records on target for each date from 2026-11-02 to 2026-11-13: 8 for `Drinking water, 8 glasses a day` and 10 for `Meditation, 10 min daily`
- And at 2026-11-16T10:00:00Z `f8.ac068.user@example.com` excused 2026-11-15 of `Meditation, 10 min daily` without a reason
- When at 2026-11-16T10:01:00Z `f8.ac068.user@example.com` reads the dashboard
- Then the Pending count is 3
- And the Today list contains (`Meditation, 10 min daily`, 2026-11-14, Pending) and no entry of `Meditation, 10 min daily` for 2026-11-15

### AC-069 (FR-019; US4-AS4) The Upcoming entry skips a Paused commitment

- Given the registered user `f8.ac069.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-16T08:00:00Z `f8.ac069.user@example.com` paused the goal from 2026-11-18 until 2026-11-18 without a reason
- When at 2026-11-17T10:00:00Z `f8.ac069.user@example.com` reads the dashboard
- Then the Today list contains (`Yoga, 3 days a week`, 2026-11-16, Pending) and (`Yoga, 3 days a week`, 2026-11-20, Upcoming)
- And the Today list contains no entry for 2026-11-18
- And the number of goals is 1

### AC-070 (FR-019) An Excused commitment whose grace period would have ended this week is not Missed

- Given the registered user `f8.ac070.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-13T07:00:00Z `f8.ac070.user@example.com` excused 2026-11-13 without a reason
- When at 2026-11-18T10:00:00Z `f8.ac070.user@example.com` reads the dashboard
- Then the Missed count is 0
- And the Today list contains (`Yoga, 3 days a week`, 2026-11-16, Pending) and (`Yoga, 3 days a week`, 2026-11-18, Due) and no entry for 2026-11-13

### AC-071 (FR-020) Recent notes show Excused and Paused as the state

- Given the registered user `f8.ac071.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-13T07:00:00Z `f8.ac071.user@example.com` excused 2026-11-13 without a reason
- And at 2026-11-13T08:00:00Z `f8.ac071.user@example.com` saved the note "Travel" for 2026-11-13
- And at 2026-11-16T08:00:00Z `f8.ac071.user@example.com` paused the goal from 2026-11-16 until 2026-11-16 without a reason
- And at 2026-11-16T09:00:00Z `f8.ac071.user@example.com` saved the note "Visit" for 2026-11-16
- When at 2026-11-16T10:00:00Z `f8.ac071.user@example.com` reads the recent notes of the goal
- Then they are, in this order, (2026-11-16, "Visit", Paused) and (2026-11-13, "Travel", Excused)
- And the dashboard's recent notes are, in this order, (`Yoga, 3 days a week`, 2026-11-16, "Visit", Paused) and (`Yoga, 3 days a week`, 2026-11-13, "Travel", Excused)

### AC-072 (FR-021; US5-AS1) Another registered user cannot use any use case of this slice on someone else's goal

- Given the registered user `f8.ac072.<n>.a@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-12T10:00:00Z `f8.ac072.<n>.a@example.com` excused 2026-11-11 with the reason "Ill"
- And at 2026-11-16T08:00:00Z `f8.ac072.<n>.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
- And at 2026-11-16T08:01:00Z `f8.ac072.<n>.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
- And the registered user `f8.ac072.<n>.b@example.com`
- When at 2026-11-16T10:00:00Z `f8.ac072.<n>.b@example.com` <action> on the goal of `f8.ac072.<n>.a@example.com`
- Then the result is "This goal does not exist."
- And the pauses of the goal, read by `f8.ac072.<n>.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
- And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac072.<n>.a@example.com`, gives 2026-11-11 Excused, reason "Ill"

Examples:

| n | action |
|---|---|
| 01 | excuses 2026-11-18 without a reason |
| 02 | removes the excusal of 2026-11-11 |
| 03 | pauses the goal from 2026-11-28 until 2026-11-29 without a reason |
| 04 | resumes the goal |
| 05 | cancels the pause 2026-11-23 – 2026-11-27 |
| 06 | reads the pauses of the goal |

### AC-073 (FR-022; US5-AS2) An administrator cannot use any use case of this slice

- Given the registered user `f8.ac073.<n>.a@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-12T10:00:00Z `f8.ac073.<n>.a@example.com` excused 2026-11-11 with the reason "Ill"
- And at 2026-11-16T08:00:00Z `f8.ac073.<n>.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
- And at 2026-11-16T08:01:00Z `f8.ac073.<n>.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
- And the administrator `f8.ac073.<n>.admin@example.com`
- When at 2026-11-16T10:00:00Z `f8.ac073.<n>.admin@example.com` <action> on the goal of `f8.ac073.<n>.a@example.com`
- Then the result is "Goals are for registered users."
- And the pauses of the goal, read by `f8.ac073.<n>.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
- And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac073.<n>.a@example.com`, gives 2026-11-11 Excused, reason "Ill"

Examples:

| n | action |
|---|---|
| 01 | excuses 2026-11-18 without a reason |
| 02 | removes the excusal of 2026-11-11 |
| 03 | pauses the goal from 2026-11-28 until 2026-11-29 without a reason |
| 04 | resumes the goal |
| 05 | cancels the pause 2026-11-23 – 2026-11-27 |
| 06 | reads the pauses of the goal |

### AC-074 (FR-022; US5-AS3) An anonymous visitor cannot use any use case of this slice

- Given the registered user `f8.ac074.<n>.a@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-12T10:00:00Z `f8.ac074.<n>.a@example.com` excused 2026-11-11 with the reason "Ill"
- And at 2026-11-16T08:00:00Z `f8.ac074.<n>.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
- And at 2026-11-16T08:01:00Z `f8.ac074.<n>.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
- When at 2026-11-16T10:00:00Z the anonymous visitor <action> on the goal of `f8.ac074.<n>.a@example.com`
- Then the result is "You are not signed in."
- And the pauses of the goal, read by `f8.ac074.<n>.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
- And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac074.<n>.a@example.com`, gives 2026-11-11 Excused, reason "Ill"

Examples:

| n | action |
|---|---|
| 01 | excuses 2026-11-18 without a reason |
| 02 | removes the excusal of 2026-11-11 |
| 03 | pauses the goal from 2026-11-28 until 2026-11-29 without a reason |
| 04 | resumes the goal |
| 05 | cancels the pause 2026-11-23 – 2026-11-27 |
| 06 | reads the pauses of the goal |

### AC-075 (FR-023) A goal without excusals or pauses keeps slice 6's figures and is counted

- Given the registered user `f8.ac075.user@example.com`, who created the goal `Meditation, 10 min daily` at 2026-11-01T08:00:00Z with the start date 2026-11-01
- And `f8.ac075.user@example.com` records 10 for each date from 2026-11-01 to 2026-11-20 except 2026-11-05 and 2026-11-09
- And `f8.ac075.user@example.com` records 4 for 2026-11-05
- When at 2026-11-23T10:00:00Z `f8.ac075.user@example.com` reads the report of the goal
- Then the consistency is "90% (18 of 20)"
- And the current streak is "11 commitments" and the longest streak is "11 commitments"
- And the goals overview gives `Meditation, 10 min daily` not paused
- And the dashboard's number of goals is 1

### AC-076 (FR-031; US6-AS1) Demo pause loading creates `demo6@example.com` and names no private data in its results

- Given the earlier demo tasks run at 2026-11-18T10:00:00Z
- When demo pause loading runs at 2026-11-18T10:00:00Z with the demo password `example2026`
- Then its results are, in this order, (demo6@example.com, created), (demo6@example.com, demo goals added) and (demo6@example.com, demo records added)
- And no result contains "Meditation", "Drinking water", "Travelling.", "Ill with flu.", "Family visit.", "Overslept.", "Busy day.", "glasses" or "min"
- And sign-in with `demo6@example.com` and `example2026` succeeds

### AC-077 (FR-031; US6-AS2) The demo figures of `demo6@example.com` hold on every weekday

- Given the demo run at <time>
- When at <time> `demo6@example.com` reads the report of `Meditation, 10 min daily` and the report of `Drinking water, 8 glasses a day`
- Then `Meditation, 10 min daily` has the consistency "95% (22 of 23)", the current streak "10 commitments" and the longest streak "12 commitments"
- And `Drinking water, 8 glasses a day` has the consistency "94% (18 of 19)", the current streak "2 days" and the longest streak "16 days"
- And the goals overview gives `Drinking water, 8 glasses a day` paused until <last> and `Meditation, 10 min daily` not paused
- And the dashboard gives the Due count 1, the Pending count 2, the Missed count 0 and the number of goals 1

Examples:

| n | time | last |
|---|---|---|
| 01 | 2026-11-16T10:00:00Z | 2026-11-21 |
| 02 | 2026-11-17T10:00:00Z | 2026-11-22 |
| 03 | 2026-11-18T10:00:00Z | 2026-11-23 |
| 04 | 2026-11-19T10:00:00Z | 2026-11-24 |
| 05 | 2026-11-20T10:00:00Z | 2026-11-25 |
| 06 | 2026-11-21T10:00:00Z | 2026-11-26 |
| 07 | 2026-11-22T10:00:00Z | 2026-11-27 |

### AC-078 (FR-031; US6-AS2) The demo excusals, pauses, states and notes of `demo6@example.com`

- Given the demo run at 2026-11-18T10:00:00Z
- When at 2026-11-18T10:00:00Z `demo6@example.com` reads the history of `Meditation, 10 min daily` from 2026-10-21 to 2026-11-18
- Then it gives 2026-10-29 Excused, reason "Travelling."; 2026-10-30 Excused, reason "Travelling."; 2026-11-04 Missed, no record; 2026-11-10 Excused, no reason; 2026-11-15 Met, record 10; 2026-11-16 Excused, no reason; 2026-11-17 Pending, no record; 2026-11-18 Due, no record
- And the recent notes of `Meditation, 10 min daily` are (2026-11-04, "Overslept.", Missed)
- And the history of `Drinking water, 8 glasses a day` from 2026-10-30 to 2026-11-18 gives 2026-10-30 Met, record 8; 2026-10-31 to 2026-11-06 each Paused, no record; 2026-11-07 Met, record 8; 2026-11-13 Failed, record 6; 2026-11-16 Pending, record 8; 2026-11-17 Paused, no record; 2026-11-18 Paused, no record
- And the recent notes of `Drinking water, 8 glasses a day` are (2026-11-13, "Busy day.", Failed)
- And the pauses of `Drinking water, 8 glasses a day` are, in this order, (2026-11-17, 2026-11-23, reason "Family visit.", current) and (2026-10-31, 2026-11-06, reason "Ill with flu.", past)

### AC-079 (FR-031; US6-AS3) Demo pause loading leaves an existing `demo6@example.com` unchanged

- Given the demo run at 2026-11-18T10:00:00Z
- When demo pause loading runs at 2026-11-19T10:00:00Z with the demo password `example2026`
- Then its results are (demo6@example.com, already exists)
- And at 2026-11-19T10:00:00Z the pauses of `Drinking water, 8 glasses a day` of `demo6@example.com` are, in this order, (2026-11-17, 2026-11-23, reason "Family visit.", current) and (2026-10-31, 2026-11-06, reason "Ill with flu.", past)
- And at 2026-11-19T10:00:00Z the goals overview of `demo6@example.com` lists exactly `Meditation, 10 min daily` and `Drinking water, 8 glasses a day`

### AC-080 (FR-031; US6-AS4) Demo pause loading without a valid demo password creates nothing

- Given the earlier demo tasks run at 2026-11-18T10:00:00Z
- When demo pause loading runs at 2026-11-18T10:00:00Z <password>
- Then its results are (<result>)
- And sign-in with `demo6@example.com` and `example2026` does not succeed

Examples:

| n | password | result |
|---|---|---|
| 01 | with no demo password | no demo password configured |
| 02 | with the demo password `abc` | demo password does not meet the password rule |

### AC-081 (FR-031) A demo goal whose catalog habit is not Published is left out

- Given the earlier demo tasks run at 2026-11-18T10:00:00Z
- And the administrator `f8.ac081.<n>.admin@example.com`
- And `f8.ac081.<n>.admin@example.com` unpublished <habits> with confirmation
- When demo pause loading runs at 2026-11-18T10:00:00Z with the demo password `example2026`
- Then its results are, in this order, (demo6@example.com, created), (demo6@example.com, demo goals added) and (demo6@example.com, demo records added)
- And at 2026-11-18T10:00:00Z the goals overview of `demo6@example.com` lists exactly <goals>

Examples:

| n | habits | goals |
|---|---|---|
| 01 | the catalog habit "Drinking water" | `Meditation, 10 min daily` |
| 02 | the catalog habits "Meditation" and "Drinking water" | no goal |

### AC-082 (FR-030; US6-AS5) The earlier demo accounts get no excusal and no pause

- Given the demo run at 2026-11-18T10:00:00Z
- When at 2026-11-18T10:00:00Z each account that a result of demo loading, demo history loading or demo report loading gives as created, `demo1@example.com`, `demo4@example.com` and `demo5@example.com` included, signs in and reads the pauses and the history from 2026-08-01 to 2026-11-18 of each of its goals
- Then no goal of those accounts has a pause
- And no history entry of those goals is Excused or Paused

### AC-083 (FR-016) A yes/no week's target leaves an Excused commitment out

- Given the registered user `f8.ac083.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-16T07:00:00Z `f8.ac083.user@example.com` excused 2026-11-16 without a reason
- And `f8.ac083.user@example.com` records Yes for 2026-11-18 and 2026-11-20
- When at 2026-11-23T10:00:00Z `f8.ac083.user@example.com` reads the report of the goal
- Then the trend contains (2026-11-16 – 2026-11-22, 2, target 2)

### AC-084 (FR-002) A date in a pause that is not a commitment is refused as not scheduled

- Given the registered user `f8.ac084.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-16T08:00:00Z `f8.ac084.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 without a reason
- When at 2026-11-17T10:00:00Z `f8.ac084.user@example.com` excuses 2026-11-17 without a reason
- Then the result is "Nothing is scheduled on this date."
- And the history of the goal from 2026-11-16 to 2026-11-18 gives 2026-11-16 Paused and 2026-11-18 Paused, and no entry for 2026-11-17

### AC-085 (FR-019) An Excused Due commitment is not counted or listed as Due

- Given the registered user `f8.ac085.user@example.com`, who created the goal `Yoga, 3 days a week`
- And at 2026-11-13T07:00:00Z `f8.ac085.user@example.com` excused 2026-11-13 without a reason
- When at 2026-11-13T08:00:00Z `f8.ac085.user@example.com` reads the dashboard
- Then the Due count is 0 and the Pending count is 1
- And the Today list contains (`Yoga, 3 days a week`, 2026-11-11, Pending) and no entry for 2026-11-13
- And the number of goals is 1

### AC-086 (FR-012) For a goal that has not started, a pause is current or past by the account's today

- Given the registered user `f8.ac086.<n>.user@example.com`, who created the goal `Drinking water, 8 glasses a day` at 2026-11-16T08:00:00Z with the start date 2026-11-20
- And at 2026-11-16T09:00:00Z `f8.ac086.<n>.user@example.com` paused the goal from 2026-11-16 until 2026-11-18 without a reason
- When at <time> `f8.ac086.<n>.user@example.com` reads the pauses of the goal
- Then they are (2026-11-16, 2026-11-18, no reason, <kind>)

Examples:

| n | time | kind |
|---|---|---|
| 01 | 2026-11-16T10:00:00Z | current |
| 02 | 2026-11-19T10:00:00Z | past |

### AC-087 (FR-010) A goal that has not started can be resumed from its current pause

- Given the registered user `f8.ac087.user@example.com`, who created the goal `Drinking water, 8 glasses a day` at 2026-11-16T08:00:00Z with the start date 2026-11-20
- And at 2026-11-16T09:00:00Z `f8.ac087.user@example.com` paused the goal from 2026-11-16 until 2026-11-18 without a reason
- When at 2026-11-17T10:00:00Z `f8.ac087.user@example.com` resumes the goal
- Then the result is "resumed"
- And the pauses of the goal are (2026-11-16, 2026-11-16, no reason, past)

### AC-088 (FR-006; FR-014) A pause of several months is allowed and leaves the streak before it unchanged

- Given the registered user `f8.ac088.user@example.com`, who created the goal `Meditation, 10 min daily` at 2026-08-03T08:00:00Z with the start date 2026-08-03
- And at 2026-08-07T20:00:00Z `f8.ac088.user@example.com` recorded 10 for each date from 2026-08-03 to 2026-08-07
- And at 2026-08-08T08:00:00Z `f8.ac088.user@example.com` paused the goal from 2026-08-08 until 2026-11-15 without a reason, with the result "paused" with the first day 2026-08-08 and the last day 2026-11-15
- When at 2026-11-16T10:00:00Z `f8.ac088.user@example.com` reads the report of the goal
- Then the current streak is "5 commitments" and the longest streak is "5 commitments"
- And the pauses of the goal are (2026-08-08, 2026-11-15, no reason, past)
