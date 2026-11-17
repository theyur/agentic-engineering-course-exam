# Acceptance criteria: Goals

**Feature**: specs/004-goals · **Tag**: F004 · **Spec**: spec.md

Conventions used by every item:

- **Time.** Every use case takes the supplied current time (constitution II). Steps run in the order they are
  written. A step runs at the time the item gives for it. The setup of accounts and habits that gives no time
  (administrator creation, registration, sign-in, and creating, publishing or unpublishing a catalog habit or
  creating a custom habit, in a Given step) runs at `2026-11-18T10:00:00Z` (Wednesday 18 November 2026), or at
  the earliest time the item gives when that is earlier. A Then step, and every And or But step after it,
  checks the results of the item's When step and runs at that step's time. Any other Given or When step that
  gives no time runs at `2026-11-18T10:00:00Z` or at the latest time of the steps before it, whichever is
  later. A step that calls no use case (such as setting the current culture) has no time.
  Instants are written in UTC (`2026-11-17T22:00:00Z`); dates are written `2026-11-18`. 16 November 2026 is a
  Monday.
- **Accounts.**
  - "the anonymous visitor" is the acting user who is not signed in.
  - "the registered user `x`" means the anonymous visitor registered the email `x` with the password
    `walking42`, with no time zone and no name, registration succeeded, and sign-in with `x` and `walking42`
    succeeded; `x` is then the acting user for the steps that name it. Its time zone is the default, `UTC`
    (spec FR-025: acceptance tests supply no default).
  - "the registered user `x` in `Z`" is the same, but registration also gave the browser time zone `Z`.
  - "the administrator `a`" means administrator creation (spec, "Slice 1 and slice 2 behaviour these tests
    use") ran with the email `a` and the password `admin2026` and created the administrator, and sign-in with
    `a` and `admin2026` succeeded; `a` is then the acting user for the steps that name it.
- **Names in the database.** Every email and every catalog-habit name belongs to one item only: emails start
  with `f4.acNNN.` and catalog-habit names with `A4.NNN `, NNN being the item's number, so they also differ from
  the emails and habit names of the other features' acceptance tests in the same database (ADR 0003).
  Custom-habit names belong to one account. The demo emails `demo1@example.com` to `demo3@example.com` and the catalog-habit names without a
  prefix appear only in items that start from a reset database.
- **Examples.** An item with an `Examples:` table runs once per row, each row as its own scenario in the same
  database. Every email and catalog-habit name of such an item contains `<n>`, the row's value in the column
  `n`, so rows never share data.
- **Catalog habits.** "a Draft `N`" means the item's administrator created the catalog habit `N` and nothing
  else happened to it; "a Published `N`": created and then published; "an Unpublished `N`": created,
  published, then unpublished. Fields an item does not give are: no description, the category Other, the
  intent Build, the icon `book-open`, no suggested starting goal.
- **Custom habits.** "`x` created the custom habit `C`" means `x` created it with the intent Build, the
  category Other and the icon `book-open`, unless the item gives other values.
- **Settings.** Goal settings and suggested starting goals are written as: Build or Reduce; the measurement
  (yes/no, count or duration); for count and duration the amount and the unit (a count unit in backticks, a
  duration unit as minutes or hours); the schedule (fixed days with the days named, or N times per day or per
  week). A suggested starting goal marked "(marked)" is the one marked for the card; a list with one suggested
  starting goal has it marked.
- **Goal use cases.**
  - "starts a new goal from `H`" is the start-new-goal use case for the catalog habit `H` (or "from their
    custom habit `C`"), with no suggested starting goal position unless the item gives one. "the starting
    values" are its result.
  - "creates a goal on `H` with S" is the create-goal use case for the catalog habit `H` (or "on their custom
    habit `C`") with the settings S, the start date today in the acting user's time zone and no name, unless
    the item gives a start date or a name. "creating succeeds" means the result is a created goal with its
    identifier.
  - "lists their goals", "reads the goal" and "reads the evaluation calendar of the goal from D1 to D2" are the
    list, read and evaluation-calendar use cases. An entry of the calendar is written as its date (or first and
    last date for a week) "from" its start instant "to" its end instant.
  - In list results, "next commitment D", "start date D" and "first week starts D" are the FR-014 information;
    "no X" means the result does not give it.
  - In read results, "the time zone `Z` from D" is the time zone of the settings version of the goal's current
    date and the date from which that version applies; "a later version `Z` from D" is the version that takes
    effect after the current date; "no later version" means there is none (spec FR-016).
- **Catalog use cases.** "browses the catalog as `x`" passes `x` as the acting user; "browses the catalog
  without an acting user" is slice 2's call. The same holds for reading a habit's details. "tracked count" is
  the number of returned habits the user tracks; "goal count" is the acting user's goal count of one habit.
  Unpublishing is the administrator's unpublish use case, "without confirming" or "with confirmation".
- **Account use cases.** "reads their time zone", "changes their time zone to `Z`", "changes their name to
  `N`", "reads their profile" and "reads the Home heading" are the account use cases of spec FR-026 and FR-028
  to FR-030.
- **Texts.** `"   "` is three spaces. "a text of N characters" is `x` repeated N times; "a unit of N
  characters" is `u` repeated N times.
- **Demo loading.** "the AC database is reset" means the reset entry point of `HT.Persistence` (ADR 0003)
  drops and rebuilds the schema, so no account and no habit exists. Demo loading runs with the demo password
  `example2026` (a valid demo password, spec "Demo loading"); a demo account signs in with its email and
  `example2026`. The demo custom habits and the seed catalog habits the items name are those of the spec's
  "Demo loading" bullet.
- **Not checked here.** The spec gives no literal names for "the field" a refusal is about (FR-006), so the
  items check the message only.

## Coverage

| Requirement / scenario | AC items | Note |
|---|---|---|
| FR-001 | AC-005, AC-017, AC-047 | |
| FR-002 | AC-005, AC-006, AC-009, AC-013 | |
| FR-003 | AC-036, AC-037, AC-045 | |
| FR-004 | AC-012, AC-018 | The meaning of a target is evaluated in slice 5; this slice tests its consequences: the Reduce "1 time per period" rule and the "avoided on" target text. |
| FR-005 | AC-005, AC-025, AC-026 | |
| FR-006 | AC-007, AC-008, AC-009, AC-010, AC-011, AC-012, AC-013, AC-044, AC-063 | The field a refusal is about is not checked: the spec names no literal field values. |
| FR-007 | AC-001, AC-002, AC-003, AC-004, AC-019, AC-020, AC-044, AC-046, AC-063, AC-092 | "Every suggested starting goal of the seed catalog" (also SC-004) is checked for its Published habits only: no new goal can be based on the seed's Draft and Unpublished habits (FR-008). |
| FR-008 | AC-014, AC-015, AC-033 | |
| FR-009 | AC-011, AC-016, AC-022 | |
| FR-010 | AC-006, AC-016, AC-018, AC-047, AC-050, AC-062 | |
| FR-011 | AC-036, AC-071 | |
| FR-012 | AC-010, AC-036, AC-040, AC-041, AC-044, AC-076 | |
| FR-013 | AC-024, AC-036, AC-037, AC-038, AC-040, AC-041, AC-042, AC-043, AC-045 | |
| FR-014 | AC-021, AC-023, AC-024, AC-039, AC-077, AC-078 | |
| FR-015 | AC-005, AC-021, AC-022, AC-027 | |
| FR-016 | AC-025, AC-026, AC-039, AC-060, AC-062, AC-073 | |
| FR-017 | AC-028 | No use case edits, pauses, ends, restarts or deletes a goal; tested as "a goal stays as created". |
| FR-018 | AC-025, AC-026, AC-028 | |
| FR-019 | AC-049, AC-056, AC-059, AC-070, AC-084 | The locked F001 and F002 scenarios are run unchanged as well. |
| FR-020 | AC-029, AC-030 | |
| FR-021 | AC-031 | |
| FR-022 | AC-032 | |
| FR-023 | AC-008, AC-035, AC-091 | |
| FR-024 | AC-070, AC-074, AC-075, AC-080 | |
| FR-025 | AC-070 | |
| FR-026 | AC-071, AC-072, AC-073, AC-074, AC-075, AC-076, AC-077, AC-078, AC-079, AC-081, AC-082 | |
| FR-027 | AC-071, AC-072, AC-082, AC-083 | |
| FR-028 | AC-069, AC-084, AC-085, AC-090, AC-091, AC-094 | |
| FR-029 | AC-086, AC-087, AC-088, AC-090, AC-091 | |
| FR-030 | AC-089, AC-091 | |
| FR-031 | AC-052, AC-053, AC-054, AC-056 | |
| FR-032 | AC-034, AC-055 | |
| FR-033 | AC-057, AC-058, AC-059, AC-093 | |
| FR-034 | AC-015, AC-060, AC-061, AC-069 | |
| FR-035 | AC-048, AC-049, AC-050, AC-051 | |
| FR-040 | AC-064, AC-065, AC-066, AC-067, AC-068, AC-069 | |
| US1-AS1 | AC-001 | |
| US1-AS2 | AC-002 | |
| US1-AS3 | AC-003 | |
| US1-AS4 | AC-004 | |
| US1-AS5 | AC-005 | |
| US1-AS6 | AC-006 | |
| US1-AS7 | AC-007 | |
| US1-AS8 | AC-014 | |
| US1-AS9 | AC-016 | |
| US1-AS10 | AC-017 | |
| US1-AS11 | — | Not testable through the API: the New goal page's summary card and its wording (page, HP-002). |
| US2-AS1 | AC-021 | |
| US2-AS2 | AC-023 | |
| US2-AS3 | AC-024 | |
| US2-AS4 | AC-025 | |
| US2-AS5 | AC-027 | |
| US2-AS6 | — | Not testable through the API: the Goals page's list items, meta line and EmptyState (page, HP-003). |
| US2-AS7 | — | Not testable through the API: the goal page's layout (page, HP-004). |
| US3-AS1 | AC-029 | |
| US3-AS2 | AC-030 | |
| US3-AS3 | AC-031 | |
| US3-AS4 | AC-032 | |
| US3-AS5 | AC-033 | |
| US3-AS6 | AC-034 | |
| US4-AS1 | AC-036 | |
| US4-AS2 | AC-037 | |
| US4-AS3 | AC-038 | |
| US4-AS4 | AC-039 | |
| US4-AS5 | AC-040, AC-041 | |
| US4-AS6 | AC-042 | |
| US4-AS7 | AC-043 | |
| US4-AS8 | AC-044 | |
| US5-AS1 | AC-046 | |
| US5-AS2 | AC-047 | |
| US5-AS3 | AC-048 | |
| US5-AS4 | AC-049 | |
| US5-AS5 | AC-050 | |
| US5-AS6 | — | Not testable through the API: the custom habit page's "Save and create goal" button (page, HP-007). |
| US6-AS1 | AC-052 | |
| US6-AS2 | AC-053 | |
| US6-AS3 | AC-054 | |
| US6-AS4 | AC-055 | |
| US6-AS5 | — | Not testable through the API: the catalog cards' and habit details' labels and buttons (page, HP-005, HP-006). |
| US7-AS1 | AC-057 | |
| US7-AS2 | AC-058 | |
| US7-AS3 | AC-057 | |
| US7-AS4 | AC-059 | |
| US7-AS5 | AC-057 | |
| US7-AS6 | AC-060 | |
| US7-AS7 | AC-062 | |
| US7-AS8 | — | Not testable through the API: the goal page's Notice and the goal list's look (page, HP-004). |
| US8-AS1 | — | Not testable through the API: TopNav and MobileNav links (page, HP-009). |
| US8-AS2 | — | Not testable through the API: navigation links per audience (page, HP-009). |
| US8-AS3 | — | Not testable through the API: the New goal page's Habit section and habit chooser (page, HP-001). |
| US8-AS4 | AC-063 | |
| US8-AS5 | — | Not testable through the API: the Home EmptyState (page, HP-010). |
| US9-AS1 | AC-064 | |
| US9-AS2 | AC-065 | |
| US9-AS3 | AC-066 | |
| US9-AS4 | AC-068 | The host's log lines are HP-013 and are tested outside the AC surface. |
| US9-AS5 | AC-069 | |
| US10-AS1 | AC-070 | |
| US10-AS2 | AC-070 | |
| US10-AS3 | AC-036 | |
| US10-AS4 | AC-071 | |
| US10-AS5 | AC-072 | |
| US10-AS6 | AC-073 | |
| US10-AS7 | AC-074 | |
| US10-AS8 | AC-079 | |
| US10-AS9 | AC-080 | |
| US10-AS10 | AC-081 | |
| US11-AS1 | AC-084 | |
| US11-AS2 | AC-085 | |
| US11-AS3 | AC-086 | |
| US11-AS4 | AC-087 | |
| US11-AS5 | AC-088 | |
| US11-AS6 | AC-089 | Only the first half. Not testable through the API: an account created before this slice exists only through the migration of existing rows (spec US11-AS6, HP-019). |
| US11-AS7 | AC-090 | |
| US11-AS8 | AC-091 | |

Success criteria and edge cases (spec "Success Criteria", "Edge Cases"):

- SC-001 (time and number of actions on the page) and SC-007 (accessibility) are page outcomes, not testable
  through the API. SC-002: AC-029 to AC-034. SC-003: AC-036 to AC-041. SC-004: AC-020. SC-005: AC-057. SC-006:
  AC-064 and AC-069; the smoke check after deployment is HP-018.
- Edge cases: two time zone changes before the first applies, AC-081; a fixed-days goal starting on an
  unscheduled day, AC-024 and AC-045; a goal created at 23:59 local time, AC-044; a clock-change day, AC-040
  and AC-041 (the grace period is slice 5's); a weekly goal starting mid-week, AC-038 and AC-039; a Reduce goal
  with amount 0, AC-009 and AC-018; a yes/no goal of 2 times per day, AC-018; a suggestion changed after
  starting, and a goal that never follows later changes to it, AC-092; two goals with the same name, AC-017
  and AC-022; a custom habit deleted or a catalog habit unpublished between starting and creating, AC-015; a
  confirmed unpublish after the count changed, AC-093; a time zone change on the day a goal started, AC-082.

## Acceptance criteria

### AC-001 (FR-007; US1-AS1) Starting from a catalog habit without a position gives its marked suggested starting goal

- Given the administrator `f4.ac001.admin.<n>@example.com` and the registered user `f4.ac001.user.<n>@example.com`
- And a Published `A4.001 <n> Reading` with the suggested starting goals <suggestions>, in this order
- When `f4.ac001.user.<n>@example.com` starts a new goal from `A4.001 <n> Reading`
- Then the starting values are: the habit `A4.001 <n> Reading` (catalog), Build, duration 15 minutes, fixed days Monday to Friday, the start date 2026-11-18, no name, and the catalog suggestion text "15 min, 5 days a week"

Examples:

| n | suggestions |
|---|---|
| 01 | 1. Build, duration 15 minutes, fixed days Monday to Friday (marked); 2. Build, count 20 `pages`, 3 times per week |
| 02 | 1. Build, count 20 `pages`, 3 times per week; 2. Build, duration 15 minutes, fixed days Monday to Friday (marked) |

### AC-002 (FR-007; US1-AS2) Starting from a suggested starting goal position gives that suggestion

- Given the administrator `f4.ac002.admin@example.com` and the registered user `f4.ac002.user@example.com`
- And a Published `A4.002 Reading` with the suggested starting goals 1. Build, duration 15 minutes, fixed days Monday to Friday (marked); 2. Build, count 20 `pages`, 3 times per week
- When `f4.ac002.user@example.com` starts a new goal from `A4.002 Reading` with the suggested starting goal position 2
- Then the starting values are: the habit `A4.002 Reading` (catalog), Build, count 20 `pages`, 3 times per week, the start date 2026-11-18, no name, and the catalog suggestion text "20 pages, 3 times a week"

### AC-003 (FR-007; US1-AS3) A catalog habit without suggested starting goals gives only Build or Reduce from its intent

- Given the administrator `f4.ac003.admin@example.com` and the registered user `f4.ac003.user@example.com`
- And a Published `A4.003 Sugar` with the intent Reduce and no suggested starting goal
- When `f4.ac003.user@example.com` starts a new goal from `A4.003 Sugar`
- Then the starting values are: the habit `A4.003 Sugar` (catalog), Reduce, the start date 2026-11-18, and nothing else: no measurement, no amount, no unit, no schedule, no name and no catalog suggestion text

### AC-004 (FR-007; US1-AS4) A suggested starting goal position the habit does not have is refused

- Given the administrator `f4.ac004.admin.<n>@example.com` and the registered user `f4.ac004.user.<n>@example.com`
- And a Published `A4.004 <n> Reading` with the suggested starting goals 1. Build, duration 15 minutes, fixed days Monday to Friday (marked); 2. Build, count 20 `pages`, 3 times per week; 3. Build, yes/no, fixed days on all seven days
- When `f4.ac004.user.<n>@example.com` starts a new goal from `A4.004 <n> Reading` with the suggested starting goal position <position>
- Then the result is "This suggested starting goal does not exist."

Examples:

| n | position |
|---|---|
| 01 | 4 |
| 02 | 0 |

### AC-005 (FR-001; FR-002; FR-005; FR-015; US1-AS5) Creating a goal with the starting values unchanged

- Given the administrator `f4.ac005.admin@example.com` and the registered user `f4.ac005.user@example.com`
- And a Published `A4.005 Reading` with the suggested starting goal Build, duration 15 minutes, fixed days Monday to Friday
- And `f4.ac005.user@example.com` started a new goal from `A4.005 Reading`
- When `f4.ac005.user@example.com` creates a goal with those starting values unchanged
- Then creating succeeds
- And reading the goal gives the habit `A4.005 Reading` (catalog), Build, duration 15 minutes, fixed days Monday to Friday, the schedule kind Commitment-based, the start date 2026-11-18, no name of its own, and the time zone `UTC` from 2026-11-18
- And `f4.ac005.user@example.com`'s goal list contains the goal with the name "A4.005 Reading, 15 min, 5 days a week"

### AC-006 (FR-002; FR-010; US1-AS6) Creating a goal with changed values keeps the changed values

- Given the administrator `f4.ac006.admin@example.com` and the registered user `f4.ac006.user@example.com`
- And a Published `A4.006 Reading` with the suggested starting goal Build, duration 15 minutes, fixed days Monday to Friday
- And `f4.ac006.user@example.com` started a new goal from `A4.006 Reading`
- When `f4.ac006.user@example.com` creates a goal on `A4.006 Reading` with Reduce, count 2 `cups`, 1 time per day
- Then creating succeeds
- And reading the goal gives Reduce, count 2 `cups`, 1 time per day, the schedule kind Period-based, the target text "at most 2 cups a day" and the name "A4.006 Reading, at most 2 cups a day"

### AC-007 (FR-006; US1-AS7) Each field refusal of goal creation, with its message

- Given the administrator `f4.ac007.admin.<n>@example.com` and a Published `A4.007 <n> Reading`
- And the registered user `f4.ac007.user.<n>@example.com`
- When `f4.ac007.user.<n>@example.com` creates a goal on `A4.007 <n> Reading` with <settings>
- Then the result is <message>
- And `f4.ac007.user.<n>@example.com`'s goal list is empty

Examples:

| n | settings | message |
|---|---|---|
| 01 | no Build or Reduce, count 20 `pages`, 3 times per week | "Choose Build or Reduce." |
| 02 | Build, no measurement, 3 times per week | "Choose how it is measured." |
| 03 | Build, yes/no with the amount 5, fixed days Monday | "A yes/no goal has no amount or unit." |
| 04 | Build, yes/no with the count unit `pages`, fixed days Monday | "A yes/no goal has no amount or unit." |
| 05 | Build, count with no amount and the unit `pages`, 3 times per week | "Enter a whole number from 1 to 999." |
| 06 | Build, count 0 `pages`, 3 times per week | "Enter a whole number from 1 to 999." |
| 07 | Build, count 1000 `pages`, 3 times per week | "Enter a whole number from 1 to 999." |
| 08 | Build, duration 0 minutes, 3 times per week | "Enter a whole number from 1 to 999." |
| 09 | Reduce, count -1 `cups`, 1 time per day | "Enter a whole number from 0 to 999." |
| 10 | Reduce, count 1000 `cups`, 1 time per day | "Enter a whole number from 0 to 999." |
| 11 | Reduce, duration with no amount and the unit minutes, 1 time per day | "Enter a whole number from 0 to 999." |
| 12 | Build, count 20 with the unit `"   "`, 3 times per week | "Enter a unit, for example pages." |
| 13 | Build, count 20 with no unit, 3 times per week | "Enter a unit, for example pages." |
| 14 | Build, count 20 with a unit of 21 characters, 3 times per week | "The unit can have at most 20 characters." |
| 15 | Build, duration 15 with no duration unit, 3 times per week | "Choose minutes or hours." |
| 16 | Build, yes/no, no schedule | "Choose fixed days or how many times." |
| 17 | Build, yes/no, fixed days with no day | "Choose at least one day." |
| 18 | Build, yes/no, no number of times, per week | "Enter a number of times from 1 to 20." |
| 19 | Build, yes/no, 0 times per week | "Enter a number of times from 1 to 20." |
| 20 | Build, yes/no, 21 times per week | "Enter a number of times from 1 to 20." |
| 21 | Build, yes/no, 3 times with no period | "Choose per day or per week." |
| 22 | Reduce, count 2 `cups`, 2 times per day | "A Reduce goal with an amount has 1 time per period." |

### AC-008 (FR-006; FR-023) Goal creation gives the first refusal in FR-006 order

- Given the administrator `f4.ac008.admin.<n>@example.com`, a Published `A4.008 <n> Reading` and a Draft `A4.008 <n> Draft`
- And the registered user `f4.ac008.user.<n>@example.com`
- When `f4.ac008.user.<n>@example.com` creates a goal <goal>
- Then the result is <message>
- And `f4.ac008.user.<n>@example.com`'s goal list is empty

Examples:

| n | goal | message |
|---|---|---|
| 01 | with no habit, no Build or Reduce, yes/no, fixed days Monday | "Choose a habit." |
| 02 | on `A4.008 <n> Draft` with Build, no measurement, fixed days Monday | "This habit is not in the catalog." |
| 03 | on `A4.008 <n> Reading` with no Build or Reduce, no measurement, fixed days Monday | "Choose Build or Reduce." |
| 04 | on `A4.008 <n> Reading` with Build, yes/no with the amount 5, no schedule | "A yes/no goal has no amount or unit." |
| 05 | on `A4.008 <n> Reading` with Build, count 0 with no unit, 3 times per week | "Enter a whole number from 1 to 999." |
| 06 | on `A4.008 <n> Reading` with Build, count 20 with no unit, no schedule | "Enter a unit, for example pages." |
| 07 | on `A4.008 <n> Reading` with Build, count 20 with a unit of 21 characters, no schedule | "The unit can have at most 20 characters." |
| 08 | on `A4.008 <n> Reading` with Build, duration 15 with no duration unit, no schedule | "Choose minutes or hours." |
| 09 | on `A4.008 <n> Reading` with Build, yes/no, fixed days with no day, and the start date 2026-11-17 | "Choose at least one day." |
| 10 | on `A4.008 <n> Reading` with Build, yes/no, 21 times with no period | "Enter a number of times from 1 to 20." |
| 11 | on `A4.008 <n> Reading` with Reduce, count 2 `cups`, 2 times per day, and no start date | "A Reduce goal with an amount has 1 time per period." |
| 12 | on `A4.008 <n> Reading` with Build, yes/no, fixed days Monday, no start date and a name of 61 characters | "Choose a start date." |
| 13 | on `A4.008 <n> Reading` with Build, yes/no, fixed days Monday, the start date 2026-11-17 and a name of 61 characters | "The start date cannot be in the past." |
| 14 | on `A4.008 <n> Reading` with Build, yes/no, fixed days Monday, the start date 2027-11-19 and a name of 61 characters | "Choose a start date within a year from today." |

### AC-009 (FR-002; FR-006) The edges of every target range are accepted

- Given the administrator `f4.ac009.admin.<n>@example.com` and a Published `A4.009 <n> Reading`
- And the registered user `f4.ac009.user.<n>@example.com`
- When `f4.ac009.user.<n>@example.com` creates a goal on `A4.009 <n> Reading` with <settings>
- Then creating succeeds
- And reading the goal gives the target text <target text>

Examples:

| n | settings | target text |
|---|---|---|
| 01 | Build, count 1 `pages`, 3 times per week | "1 pages, 3 times a week" |
| 02 | Build, count 999 `pages`, 3 times per week | "999 pages, 3 times a week" |
| 03 | Reduce, count 0 `cups`, 1 time per day | "at most 0 cups a day" |
| 04 | Reduce, count 999 `cups`, 1 time per day | "at most 999 cups a day" |
| 05 | Build, count 5 with a unit of 20 characters, 1 time per day | "5 uuuuuuuuuuuuuuuuuuuu a day" |
| 06 | Build, yes/no, 1 time per week | "once a week" |
| 07 | Build, yes/no, 20 times per week | "20 times a week" |
| 08 | Build, duration 999 hours, 1 time per day | "999 h a day" |

### AC-010 (FR-006; FR-012) The start date is today or up to 365 days later

- Given the administrator `f4.ac010.admin.<n>@example.com` and a Published `A4.010 <n> Reading`
- And the registered user `f4.ac010.user.<n>@example.com`
- When `f4.ac010.user.<n>@example.com` creates a goal on `A4.010 <n> Reading` with Build, yes/no, fixed days Monday and <start date>
- Then the result is <result>

Examples:

| n | start date | result |
|---|---|---|
| 01 | the start date 2026-11-18 | creating succeeds |
| 02 | the start date 2026-11-17 | "The start date cannot be in the past." |
| 03 | the start date 2027-11-18 | creating succeeds |
| 04 | the start date 2027-11-19 | "Choose a start date within a year from today." |
| 05 | no start date | "Choose a start date." |

### AC-011 (FR-006; FR-009) A goal name has at most 60 characters after trimming

- Given the administrator `f4.ac011.admin.<n>@example.com` and a Published `A4.011 <n> Reading`
- And the registered user `f4.ac011.user.<n>@example.com`
- When `f4.ac011.user.<n>@example.com` creates a goal on `A4.011 <n> Reading` with Build, yes/no, fixed days Monday and the name <name>
- Then the result is <result>

Examples:

| n | name | result |
|---|---|---|
| 01 | a text of 60 characters | creating succeeds, and reading the goal gives that name as its own |
| 02 | a text of 61 characters | "The name can have at most 60 characters." |
| 03 | two spaces, a text of 60 characters and two spaces | creating succeeds, and reading the goal gives the text of 60 characters as its own name |

### AC-012 (FR-004; FR-006) A Reduce goal with an amount has 1 time per period

- Given the administrator `f4.ac012.admin.<n>@example.com` and a Published `A4.012 <n> Coffee`
- And the registered user `f4.ac012.user.<n>@example.com`
- When `f4.ac012.user.<n>@example.com` creates a goal on `A4.012 <n> Coffee` with <settings>
- Then the result is <result>

Examples:

| n | settings | result |
|---|---|---|
| 01 | Reduce, count 2 `cups`, 1 time per week | creating succeeds |
| 02 | Reduce, duration 30 minutes, 2 times per week | "A Reduce goal with an amount has 1 time per period." |
| 03 | Reduce, count 2 `cups`, 3 times per day | "A Reduce goal with an amount has 1 time per period." |
| 04 | Reduce, yes/no, 3 times per week | creating succeeds |
| 05 | Build, count 20 `pages`, 3 times per day | creating succeeds |
| 06 | Reduce, count 2 `cups`, fixed days Monday, Wednesday and Friday | creating succeeds |

### AC-013 (FR-002; FR-006) Values that do not belong to the chosen measurement or schedule are neither refused nor stored

- Given the administrator `f4.ac013.admin.<n>@example.com` and a Published `A4.013 <n> Reading`
- And the registered user `f4.ac013.user.<n>@example.com`
- When `f4.ac013.user.<n>@example.com` creates a goal on `A4.013 <n> Reading` with <submitted>
- Then creating succeeds
- And reading the goal gives <stored> and the target text <target text>

Examples:

| n | submitted | stored | target text |
|---|---|---|---|
| 01 | Build, count 20 `pages` with the duration unit hours, 3 times per week | count 20 `pages`, no duration unit | "20 pages, 3 times a week" |
| 02 | Build, duration 30 minutes with the count unit `pages`, 3 times per week | duration 30 minutes, no count unit | "30 min, 3 times a week" |
| 03 | Build, yes/no with the count unit `"   "`, fixed days on all seven days | yes/no, no amount and no unit | "daily" |
| 04 | Build, yes/no, fixed days Monday and Wednesday, with 5 times per week | fixed days Monday and Wednesday, no number of times | "2 days a week" |
| 05 | Build, yes/no, 2 times per day, with the fixed day Monday | 2 times per day, no fixed days | "2 times a day" |
| 06 | Build, count 20 `"  pages  "`, 3 times per week | count 20 `pages` | "20 pages, 3 times a week" |

### AC-014 (FR-008; US1-AS8) A habit that is not a Published catalog habit is refused as a catalog habit

- Given the administrator `f4.ac014.admin.<n>@example.com`, a Draft `A4.014 <n> Draft` and an Unpublished `A4.014 <n> Gone`
- And the registered user `f4.ac014.user.<n>@example.com`, who created the custom habit `Cold shower`
- When `f4.ac014.user.<n>@example.com` <action> <habit> given as a catalog habit
- Then the result is "This habit is not in the catalog."
- And `f4.ac014.user.<n>@example.com`'s goal list is empty

Examples:

| n | action | habit |
|---|---|---|
| 01 | starts a new goal from | `A4.014 <n> Draft` |
| 02 | starts a new goal from | `A4.014 <n> Gone` |
| 03 | starts a new goal from | the identifier of their custom habit `Cold shower` |
| 04 | starts a new goal from | an identifier that the test creates and that belongs to no habit |
| 05 | creates a goal with Build, yes/no, fixed days Monday on | `A4.014 <n> Draft` |
| 06 | creates a goal with Build, yes/no, fixed days Monday on | `A4.014 <n> Gone` |
| 07 | creates a goal with Build, yes/no, fixed days Monday on | the identifier of their custom habit `Cold shower` |
| 08 | creates a goal with Build, yes/no, fixed days Monday on | an identifier that the test creates and that belongs to no habit |

### AC-015 (FR-008; FR-034) A habit that changed between starting and creating is looked up again

- Given the administrator `f4.ac015.admin.<n>@example.com` and a Published `A4.015 <n> Reading` with the suggested starting goal Build, duration 15 minutes, fixed days Monday to Friday
- And the registered user `f4.ac015.user.<n>@example.com`, who created the custom habit `Cold shower`
- And `f4.ac015.user.<n>@example.com` started a new goal from <habit>
- And <change>
- When `f4.ac015.user.<n>@example.com` creates a goal on <habit> with Build, duration 15 minutes, fixed days Monday to Friday
- Then the result is <message>
- And `f4.ac015.user.<n>@example.com`'s goal list is empty

Examples:

| n | habit | change | message |
|---|---|---|---|
| 01 | `A4.015 <n> Reading` | the administrator unpublished `A4.015 <n> Reading` without confirming | "This habit is not in the catalog." |
| 02 | their custom habit `Cold shower` | `f4.ac015.user.<n>@example.com` deleted their custom habit `Cold shower` | "This custom habit does not exist." |

### AC-016 (FR-009; FR-010; US1-AS9) A goal without a name of its own is named after its habit and target

- Given the administrator `f4.ac016.admin.<n>@example.com` and a Published `A4.016 <n> Reading`
- And the registered user `f4.ac016.user.<n>@example.com`
- When `f4.ac016.user.<n>@example.com` creates a goal on `A4.016 <n> Reading` with Build, duration 15 minutes, fixed days Monday to Friday and <name>
- Then reading the goal gives the name <goal name>, and the name is <own>

Examples:

| n | name | goal name | own |
|---|---|---|---|
| 01 | no name | "A4.016 01 Reading, 15 min, 5 days a week" | not its own |
| 02 | the name "Morning pages" | "Morning pages" | its own |
| 03 | the name "  Morning pages  " | "Morning pages" | its own |
| 04 | the name `"   "` | "A4.016 04 Reading, 15 min, 5 days a week" | not its own |

### AC-017 (FR-001; US1-AS10) Several goals on one habit are independent

- Given the administrator `f4.ac017.admin@example.com` and a Published `A4.017 Reading`
- And the registered user `f4.ac017.user@example.com`, who created a goal on `A4.017 Reading` with Build, duration 15 minutes, fixed days Monday to Friday
- When `f4.ac017.user@example.com` creates a goal on `A4.017 Reading` with Build, count 20 `pages`, 3 times per week, and then a goal on `A4.017 Reading` with Build, duration 15 minutes, fixed days Monday to Friday
- Then both creations succeed with identifiers different from each other and from the first goal
- And the goal list has 3 goals on `A4.017 Reading`: two named "A4.017 Reading, 15 min, 5 days a week" and one named "A4.017 Reading, 20 pages, 3 times a week"
- And reading the first goal still gives Build, duration 15 minutes, fixed days Monday to Friday

### AC-018 (FR-004; FR-010) The target text follows the slice 2 rules, with the "avoided on" exception

- Given the registered user `f4.ac018.user.<n>@example.com`, who created the custom habit `Practice`
- When `f4.ac018.user.<n>@example.com` creates a goal on their custom habit `Practice` with <settings>
- Then reading the goal gives the target text <target text> and the name "Practice, " followed by that target text

Examples:

| n | settings | target text |
|---|---|---|
| 01 | Build, duration 15 minutes, fixed days Monday to Friday | "15 min, 5 days a week" |
| 02 | Build, count 20 `pages`, 3 times per week | "20 pages, 3 times a week" |
| 03 | Reduce, count 1 `sweet`, 1 time per day | "at most 1 sweet a day" |
| 04 | Build, yes/no, fixed days on all seven days | "daily" |
| 05 | Build, yes/no, 1 time per day | "once a day" |
| 06 | Build, yes/no, fixed days Sunday | "once a week" |
| 07 | Build, yes/no, 1 time per week | "once a week" |
| 08 | Build, duration 2 hours, 3 times per day | "2 h, 3 times a day" |
| 09 | Build, duration 30 minutes, fixed days on all seven days | "30 min daily" |
| 10 | Build, count 10 `pages`, 1 time per day | "10 pages a day" |
| 11 | Reduce, count 0 `cigarettes`, 1 time per day | "at most 0 cigarettes a day" |
| 12 | Build, yes/no, 2 times per day | "2 times a day" |
| 13 | Reduce, yes/no, 3 times per week | "at most 3 times a week" |
| 14 | Reduce, yes/no, 1 time per day | "at most once a day" |
| 15 | Build, yes/no, fixed days Monday, Wednesday and Friday | "3 days a week" |
| 16 | Reduce, count 2 `cups`, fixed days Monday to Friday | "at most 2 cups, 5 days a week" |
| 17 | Reduce, duration 1 hours, 1 time per day | "at most 1 h a day" |
| 18 | Reduce, yes/no, fixed days Monday to Friday | "avoided on Mon–Fri" |
| 19 | Reduce, yes/no, fixed days Saturday and Sunday | "avoided on Sat, Sun" |
| 20 | Reduce, yes/no, fixed days on all seven days | "avoided every day" |
| 21 | Reduce, yes/no, fixed days Monday, Tuesday, Wednesday and Saturday | "avoided on Mon–Wed, Sat" |
| 22 | Reduce, yes/no, fixed days Sunday and Monday | "avoided on Mon, Sun" |
| 23 | Reduce, yes/no, fixed days Wednesday | "avoided on Wed" |

### AC-019 (FR-007) The catalog suggestion text of the starting values follows the goal rules

- Given the administrator `f4.ac019.admin@example.com` and the registered user `f4.ac019.user@example.com`
- And a Published `A4.019 Phone` with the intent Reduce and the suggested starting goal Reduce, yes/no, fixed days Monday to Friday
- When `f4.ac019.user@example.com` starts a new goal from `A4.019 Phone`
- Then the starting values are Reduce, yes/no, fixed days Monday to Friday, with the catalog suggestion text "avoided on Mon–Fri"
- And reading the details of `A4.019 Phone` without an acting user still gives that suggested starting goal the text "at most 5 days a week"

### AC-020 (FR-007) Every suggested starting goal of the seed catalog can be created unchanged

- Given the AC database is reset, so no account and no habit exists
- And demo loading ran and its catalog result was "seed catalog loaded"
- And the registered user `f4.ac020.user@example.com`
- When, for every catalog habit that browsing the catalog without an acting user returns and for every position of its suggested starting goals from 1 to their number, `f4.ac020.user@example.com` starts a new goal from that habit with that position and creates a goal with the starting values unchanged
- Then every creation succeeds
- And `f4.ac020.user@example.com`'s goal list has one goal per creation

### AC-021 (FR-014; FR-015; US2-AS1) Listing gives every goal with its summary, in name order

- Given the administrator `f4.ac021.admin@example.com`, a Published `A4.021 Yoga` and a Published `A4.021 Reading`
- And the registered user `f4.ac021.user@example.com`, who created the custom habit `Cold shower`
- And `f4.ac021.user@example.com` created a goal on `A4.021 Yoga` with Build, yes/no, 3 times per week, then a goal on `A4.021 Reading` with Build, duration 15 minutes, fixed days Monday to Friday, then a goal on their custom habit `Cold shower` with Build, yes/no, fixed days on all seven days
- When `f4.ac021.user@example.com` lists their goals
- Then the list is, in this order:
- And "A4.021 Reading, 15 min, 5 days a week": habit `A4.021 Reading`, catalog, Build, duration, target text "15 min, 5 days a week", Commitment-based, next commitment 2026-11-18, no start date, no first week starts
- And "A4.021 Yoga, 3 times a week": habit `A4.021 Yoga`, catalog, Build, yes/no, target text "3 times a week", Period-based, no next commitment, no start date, first week starts 2026-11-23
- And "Cold shower, daily": habit `Cold shower`, custom, Build, yes/no, target text "daily", Commitment-based, next commitment 2026-11-18, no start date, no first week starts
- And each entry has its goal's identifier

### AC-022 (FR-009; FR-015) Goals are listed by name ignoring case, then in creation order

- Given the registered user `f4.ac022.user@example.com`, who created the custom habit `Walk`
- And `f4.ac022.user@example.com` created goals on their custom habit `Walk` with Build, yes/no, fixed days Monday, named "beta" at `2026-11-18T10:00:01Z`, "Alpha" at `2026-11-18T10:00:02Z`, "Äpfel" at `2026-11-18T10:00:03Z`, "alpha" at `2026-11-18T10:00:04Z` and "zebra" at `2026-11-18T10:00:05Z`, in this order
- When at `2026-11-18T10:00:06Z` `f4.ac022.user@example.com` lists their goals
- Then the names are "Alpha", "alpha", "beta", "zebra", "Äpfel", in this order

### AC-023 (FR-014; US2-AS2) The next commitment is the first commitment that has not ended

- Given the registered user `f4.ac023.user.<n>@example.com`, who created the custom habit `Yoga`
- And at `2026-11-16T08:00:00Z` `f4.ac023.user.<n>@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days Monday, Wednesday and Friday and the start date 2026-11-16
- When at <time> `f4.ac023.user.<n>@example.com` lists their goals
- Then the goal gives next commitment <next> and no start date

Examples:

| n | time | next |
|---|---|---|
| 01 | `2026-11-16T08:00:00Z` | 2026-11-16 |
| 02 | `2026-11-18T00:00:00Z` | 2026-11-18 |
| 03 | `2026-11-18T23:59:59Z` | 2026-11-18 |
| 04 | `2026-11-19T00:00:00Z` | 2026-11-20 |
| 05 | `2026-11-21T00:00:00Z` | 2026-11-23 |

### AC-024 (FR-013; FR-014; US2-AS3) A goal that has not started gives its start date

- Given the registered user `f4.ac024.user@example.com`, who created the custom habits `Water` and `Stretch`
- And `f4.ac024.user@example.com` created a goal on their custom habit `Water` with Build, yes/no, 1 time per day and the start date 2026-11-25
- And `f4.ac024.user@example.com` created a goal on their custom habit `Stretch` with Build, yes/no, fixed days Monday, Wednesday and Friday and the start date 2026-11-24
- When `f4.ac024.user@example.com` lists their goals
- Then "Water, once a day" gives start date 2026-11-25 and no next commitment
- And "Stretch, 3 days a week" gives start date 2026-11-24 and next commitment 2026-11-25
- And listed at `2026-11-25T00:00:00Z`, "Water, once a day" gives no start date, no next commitment and no first week starts

### AC-025 (FR-005; FR-016; FR-018; US2-AS4) Reading a goal gives all its fields, its settings version and its attempt

- Given the administrator `f4.ac025.admin@example.com` and a Published `A4.025 Reading` with the category Mind
- And the registered user `f4.ac025.user@example.com`, who created a goal on `A4.025 Reading` with Build, duration 15 minutes, fixed days Monday to Friday
- When `f4.ac025.user@example.com` reads the goal
- Then the result has the goal's identifier, the name "A4.025 Reading, 15 min, 5 days a week", which is not its own, the habit name `A4.025 Reading`, the kind catalog, the category Mind, and that the habit is in the catalog
- And Build, duration 15 minutes, fixed days Monday to Friday, the schedule kind Commitment-based, the target text "15 min, 5 days a week" and the start date 2026-11-18
- And no first week starts date
- And the time zone `UTC` from 2026-11-18 and no later version
- And attempt 1, started on 2026-11-18

### AC-026 (FR-005; FR-016; FR-018) Reading a goal that has not started gives its first version and attempt from its start date

- Given the registered user `f4.ac026.user@example.com`, who created the custom habit `Run`
- And `f4.ac026.user@example.com` created a goal on their custom habit `Run` with Build, yes/no, 3 times per week and the start date 2026-12-02
- When `f4.ac026.user@example.com` reads the goal
- Then the result has the start date 2026-12-02, first week starts 2026-12-07, the time zone `UTC` from 2026-12-02, no later version, and attempt 1, started on 2026-12-02

### AC-027 (FR-015; US2-AS5) A user with no goals gets an empty list

- Given the registered user `f4.ac027.user@example.com`
- When `f4.ac027.user@example.com` lists their goals
- Then the list is empty

### AC-028 (FR-017; FR-018) A goal stays as it was created

- Given the administrator `f4.ac028.admin@example.com` and a Published `A4.028 Reading`
- And the registered user `f4.ac028.user@example.com`, who created a goal G on `A4.028 Reading` with Build, duration 15 minutes, fixed days Monday to Friday and the name "Morning pages"
- And at `2026-11-25T10:00:00Z` `f4.ac028.user@example.com` listed their goals, read G, read the evaluation calendar of G from 2026-11-16 to 2026-11-29, started a new goal from `A4.028 Reading` and created a goal on `A4.028 Reading` with Build, yes/no, 1 time per day
- When at `2026-11-25T10:00:00Z` `f4.ac028.user@example.com` reads G
- Then the result has the name "Morning pages", Build, duration 15 minutes, fixed days Monday to Friday, the start date 2026-11-18, the time zone `UTC` from 2026-11-18, no later version, and attempt 1, started on 2026-11-18

### AC-029 (FR-020; US3-AS1) Another user's goal does not exist for them

- Given the administrator `f4.ac029.admin.<n>@example.com` and a Published `A4.029 <n> Reading`
- And the registered user `f4.ac029.a.<n>@example.com`, who created a goal on `A4.029 <n> Reading` with Build, yes/no, fixed days Monday
- And the registered user `f4.ac029.b.<n>@example.com`
- When `f4.ac029.b.<n>@example.com` <action> <goal>
- Then the result is "This goal does not exist."

Examples:

| n | action | goal |
|---|---|---|
| 01 | reads the goal | `f4.ac029.a.01@example.com`'s goal |
| 02 | reads the evaluation calendar from 2026-11-18 to 2026-11-24 of | `f4.ac029.a.02@example.com`'s goal |
| 03 | reads the goal | an identifier that the test creates and that belongs to no goal |
| 04 | reads the evaluation calendar from 2026-11-18 to 2026-11-24 of | an identifier that the test creates and that belongs to no goal |

### AC-030 (FR-020; US3-AS2) A user's list never contains another user's goals

- Given the administrator `f4.ac030.admin@example.com` and a Published `A4.030 Reading`
- And the registered user `f4.ac030.a@example.com`, who created a goal on `A4.030 Reading` with Build, yes/no, fixed days Monday and the name "A4.030 goal of A"
- And the registered user `f4.ac030.b@example.com`, who created a goal on `A4.030 Reading` with Build, yes/no, fixed days Monday and the name "A4.030 goal of B"
- When `f4.ac030.b@example.com` lists their goals
- Then the list is exactly "A4.030 goal of B"

### AC-031 (FR-021; US3-AS3) An administrator gets no goal use case

- Given the administrator `f4.ac031.admin.<n>@example.com` and a Published `A4.031 <n> Reading`
- And the registered user `f4.ac031.user.<n>@example.com`, who created a goal on `A4.031 <n> Reading` with Build, yes/no, fixed days Monday
- When `f4.ac031.admin.<n>@example.com` <action>
- Then the result is "Goals are for registered users." and no goal is returned

Examples:

| n | action |
|---|---|
| 01 | starts a new goal from `A4.031 01 Reading` |
| 02 | creates a goal on `A4.031 02 Reading` with Build, yes/no, fixed days Monday |
| 03 | lists their goals |
| 04 | reads `f4.ac031.user.04@example.com`'s goal |
| 05 | reads the evaluation calendar of `f4.ac031.user.05@example.com`'s goal from 2026-11-18 to 2026-11-24 |
| 06 | browses the catalog as `f4.ac031.admin.06@example.com` with "Only habits I track" |

### AC-032 (FR-022; US3-AS4) An anonymous visitor gets no goal use case

- Given the administrator `f4.ac032.admin.<n>@example.com` and a Published `A4.032 <n> Reading`
- And the registered user `f4.ac032.user.<n>@example.com`, who created a goal on `A4.032 <n> Reading` with Build, yes/no, fixed days Monday
- When the anonymous visitor <action>
- Then the result is "You are not signed in."

Examples:

| n | action |
|---|---|
| 01 | starts a new goal from `A4.032 01 Reading` |
| 02 | creates a goal on `A4.032 02 Reading` with Build, yes/no, fixed days Monday |
| 03 | lists their goals |
| 04 | reads `f4.ac032.user.04@example.com`'s goal |
| 05 | reads the evaluation calendar of `f4.ac032.user.05@example.com`'s goal from 2026-11-18 to 2026-11-24 |
| 06 | browses the catalog as the anonymous visitor with "Only habits I track" |

### AC-033 (FR-008; US3-AS5) A custom habit is looked up only among the acting user's custom habits

- Given the administrator `f4.ac033.admin.<n>@example.com` and a Published `A4.033 <n> Reading`
- And the registered user `f4.ac033.b.<n>@example.com`, who created the custom habit `Cold shower`
- And the registered user `f4.ac033.a.<n>@example.com`
- When `f4.ac033.a.<n>@example.com` <action> <habit> given as a custom habit
- Then the result is "This custom habit does not exist."
- And `f4.ac033.a.<n>@example.com`'s goal list is empty

Examples:

| n | action | habit |
|---|---|---|
| 01 | starts a new goal from | `f4.ac033.b.01@example.com`'s custom habit `Cold shower` |
| 02 | creates a goal with Build, yes/no, fixed days Monday on | `f4.ac033.b.02@example.com`'s custom habit `Cold shower` |
| 03 | starts a new goal from | the identifier of `A4.033 03 Reading` |
| 04 | creates a goal with Build, yes/no, fixed days Monday on | the identifier of `A4.033 04 Reading` |
| 05 | creates a goal with Build, yes/no, fixed days Monday on | an identifier that the test creates and that belongs to no habit |

### AC-034 (FR-032; US3-AS6) Catalog goal counts are the acting user's own

- Given the administrator `f4.ac034.admin@example.com` and a Published `A4.034 Reading`
- And the registered user `f4.ac034.a@example.com`, who created two goals on `A4.034 Reading` with Build, yes/no, fixed days Monday
- And the registered user `f4.ac034.b@example.com`
- When `f4.ac034.b@example.com` browses the catalog as `f4.ac034.b@example.com` with the search `A4.034`, and reads the details of `A4.034 Reading` as `f4.ac034.b@example.com`
- Then browsing gives `A4.034 Reading` the goal count 0 and the tracked count 0
- And the details give the goal count 0 and no goals

### AC-035 (FR-023) Use cases check the acting user, the role, the identifier and the fields, in this order

- Given the administrator `f4.ac035.admin.<n>@example.com` and a Draft `A4.035 <n> Draft`
- And the registered user `f4.ac035.user.<n>@example.com`
- When <acting user> <action>
- Then the result is <message>

Examples:

| n | acting user | action | message |
|---|---|---|---|
| 01 | the anonymous visitor | reads the evaluation calendar from 2026-11-24 to 2026-11-18 of an identifier that belongs to no goal | "You are not signed in." |
| 02 | `f4.ac035.admin.02@example.com` | reads the evaluation calendar from 2026-11-24 to 2026-11-18 of an identifier that belongs to no goal | "Goals are for registered users." |
| 03 | `f4.ac035.user.03@example.com` | reads the evaluation calendar from 2026-11-24 to 2026-11-18 of an identifier that belongs to no goal | "This goal does not exist." |
| 04 | the anonymous visitor | creates a goal on `A4.035 04 Draft` with no Build or Reduce | "You are not signed in." |
| 05 | `f4.ac035.admin.05@example.com` | creates a goal on `A4.035 05 Draft` with no Build or Reduce | "Goals are for registered users." |
| 06 | `f4.ac035.user.06@example.com` | starts a new goal from `A4.035 06 Draft` with the suggested starting goal position 9 | "This habit is not in the catalog." |
| 07 | `f4.ac035.admin.07@example.com` | starts a new goal from `A4.035 07 Draft` with the suggested starting goal position 9 | "Goals are for registered users." |

### AC-036 (FR-003; FR-011; FR-012; FR-013; US4-AS1; US10-AS3) A fixed-days goal has one commitment per scheduled day, from midnight to midnight in its time zone

- Given the registered user `f4.ac036.user@example.com` in `Europe/Kyiv`, who created the custom habit `Yoga`
- And `f4.ac036.user@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days Monday, Wednesday and Friday and the start date 2026-11-18
- When `f4.ac036.user@example.com` reads the evaluation calendar of the goal from 2026-11-16 to 2026-11-29
- Then it has 5 commitments, in this order:
- And 2026-11-18 from `2026-11-17T22:00:00Z` to `2026-11-18T22:00:00Z`
- And 2026-11-20 from `2026-11-19T22:00:00Z` to `2026-11-20T22:00:00Z`
- And 2026-11-23 from `2026-11-22T22:00:00Z` to `2026-11-23T22:00:00Z`
- And 2026-11-25 from `2026-11-24T22:00:00Z` to `2026-11-25T22:00:00Z`
- And 2026-11-27 from `2026-11-26T22:00:00Z` to `2026-11-27T22:00:00Z`
- And no entry has a status

### AC-037 (FR-003; FR-013; US4-AS2) A per-day goal has one evaluation period per day

- Given the registered user `f4.ac037.user@example.com` in `America/New_York`, who created the custom habit `Water`
- And `f4.ac037.user@example.com` created a goal on their custom habit `Water` with Build, yes/no, 2 times per day and the start date 2026-11-18
- When `f4.ac037.user@example.com` reads the evaluation calendar of the goal from 2026-11-16 to 2026-11-24
- Then it has 7 evaluation periods, one for each date from 2026-11-18 to 2026-11-24, in date order
- And each runs from 05:00:00Z of its date to 05:00:00Z of the next date: the first from `2026-11-18T05:00:00Z` to `2026-11-19T05:00:00Z`, the last from `2026-11-24T05:00:00Z` to `2026-11-25T05:00:00Z`

### AC-038 (FR-013; US4-AS3) A per-week goal has one evaluation period per Monday-to-Sunday week, from its first full week

- Given the registered user `f4.ac038.user.<n>@example.com`, who created the custom habit `Run`
- And `f4.ac038.user.<n>@example.com` created a goal on their custom habit `Run` with Build, yes/no, 3 times per week and the start date <start date>
- When `f4.ac038.user.<n>@example.com` reads the evaluation calendar of the goal from <from> to <to>
- Then it has <periods>

Examples:

| n | start date | from | to | periods |
|---|---|---|---|---|
| 01 | 2026-11-18 | 2026-11-16 | 2026-12-06 | 2 periods: 2026-11-23 to 2026-11-29 from `2026-11-23T00:00:00Z` to `2026-11-30T00:00:00Z`, and 2026-11-30 to 2026-12-06 from `2026-11-30T00:00:00Z` to `2026-12-07T00:00:00Z` |
| 02 | 2026-11-18 | 2026-11-25 | 2026-12-01 | 2 periods: 2026-11-23 to 2026-11-29 from `2026-11-23T00:00:00Z` to `2026-11-30T00:00:00Z`, and 2026-11-30 to 2026-12-06 from `2026-11-30T00:00:00Z` to `2026-12-07T00:00:00Z` |
| 03 | 2026-11-18 | 2026-11-18 | 2026-11-22 | no period |
| 04 | 2026-11-23 | 2026-11-23 | 2026-11-29 | 1 period: 2026-11-23 to 2026-11-29 from `2026-11-23T00:00:00Z` to `2026-11-30T00:00:00Z` |

### AC-039 (FR-014; FR-016; US4-AS4) A per-week goal tells when its first week starts

- Given the registered user `f4.ac039.user.<n>@example.com`, who created the custom habit `Run`
- And `f4.ac039.user.<n>@example.com` created a goal on their custom habit `Run` with Build, yes/no, 3 times per week and the start date <start date>
- When at <time> `f4.ac039.user.<n>@example.com` lists their goals and reads the goal
- Then the list gives the goal <list>
- And reading the goal gives first week starts 2026-11-23

Examples:

| n | start date | time | list |
|---|---|---|---|
| 01 | 2026-11-18 | `2026-11-18T10:00:00Z` | first week starts 2026-11-23, no start date and no next commitment |
| 02 | 2026-11-18 | `2026-11-22T23:59:59Z` | first week starts 2026-11-23, no start date and no next commitment |
| 03 | 2026-11-18 | `2026-11-23T00:00:00Z` | no first week starts, no start date and no next commitment |
| 04 | 2026-11-23 | `2026-11-18T10:00:00Z` | first week starts 2026-11-23, start date 2026-11-23 and no next commitment |

### AC-040 (FR-012; FR-013; US4-AS5) A day or week with a clock change keeps local midnights and lasts 23 or 25 hours

- Given the registered user `f4.ac040.user.<n>@example.com` in `Europe/Kyiv`, who created the custom habit `Practice`
- And `f4.ac040.user.<n>@example.com` created a goal on their custom habit `Practice` with Build, yes/no, <schedule> and the start date 2026-11-18
- When `f4.ac040.user.<n>@example.com` reads the evaluation calendar of the goal from <date> to <date>
- Then it has exactly one entry: <entry>

Examples:

| n | schedule | date | entry |
|---|---|---|---|
| 01 | fixed days Sunday | 2027-03-28 | 2027-03-28 from `2027-03-27T22:00:00Z` to `2027-03-28T21:00:00Z` (23 hours) |
| 02 | fixed days Sunday | 2027-10-31 | 2027-10-31 from `2027-10-30T21:00:00Z` to `2027-10-31T22:00:00Z` (25 hours) |
| 03 | 1 time per day | 2027-03-28 | 2027-03-28 from `2027-03-27T22:00:00Z` to `2027-03-28T21:00:00Z` (23 hours) |
| 04 | 1 time per day | 2027-10-31 | 2027-10-31 from `2027-10-30T21:00:00Z` to `2027-10-31T22:00:00Z` (25 hours) |
| 05 | 1 time per week | 2027-03-28 | 2027-03-22 to 2027-03-28 from `2027-03-21T22:00:00Z` to `2027-03-28T21:00:00Z` (167 hours) |
| 06 | 1 time per week | 2027-10-31 | 2027-10-25 to 2027-10-31 from `2027-10-24T21:00:00Z` to `2027-10-31T22:00:00Z` (169 hours) |

### AC-041 (FR-012; FR-013; US4-AS5) The days around a clock change meet at local midnight

- Given the registered user `f4.ac041.user@example.com` in `Europe/Kyiv`, who created the custom habit `Water`
- And `f4.ac041.user@example.com` created a goal on their custom habit `Water` with Build, yes/no, 1 time per day and the start date 2026-11-18
- When `f4.ac041.user@example.com` reads the evaluation calendar of the goal from 2027-03-27 to 2027-03-29
- Then it has 3 evaluation periods, in this order:
- And 2027-03-27 from `2027-03-26T22:00:00Z` to `2027-03-27T22:00:00Z`
- And 2027-03-28 from `2027-03-27T22:00:00Z` to `2027-03-28T21:00:00Z`
- And 2027-03-29 from `2027-03-28T21:00:00Z` to `2027-03-29T21:00:00Z`

### AC-042 (FR-013; US4-AS6) A range before the start date gives an empty calendar

- Given the registered user `f4.ac042.user@example.com`, who created the custom habit `Yoga`
- And `f4.ac042.user@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days on all seven days and the start date 2026-11-25
- When `f4.ac042.user@example.com` reads the evaluation calendar of the goal from 2026-11-18 to 2026-11-24
- Then the calendar has no entry

### AC-043 (FR-013; US4-AS7) The calendar range has 1 to 366 days

- Given the registered user `f4.ac043.user.<n>@example.com`, who created the custom habit `Yoga`
- And `f4.ac043.user.<n>@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days Wednesday and the start date 2026-11-18
- When `f4.ac043.user.<n>@example.com` reads the evaluation calendar of the goal from <from> to <to>
- Then the result is <result>

Examples:

| n | from | to | result |
|---|---|---|---|
| 01 | 2026-11-24 | 2026-11-18 | "Choose a range of 1 to 366 days." |
| 02 | 2026-11-18 | 2027-11-19 | "Choose a range of 1 to 366 days." |
| 03 | 2026-11-18 | 2027-11-18 | a calendar of 53 commitments, the first on 2026-11-18 and the last on 2027-11-17 |
| 04 | 2026-11-18 | 2026-11-18 | a calendar of 1 commitment, on 2026-11-18 |

### AC-044 (FR-006; FR-007; FR-012; US4-AS8) "Today" is the date in the user's own time zone

- Given the registered user `f4.ac044.user.<n>@example.com` in <time zone>, who created the custom habit `Yoga`
- When at <time> `f4.ac044.user.<n>@example.com` starts a new goal from their custom habit `Yoga`, and then creates a goal on it with Build, yes/no, fixed days on all seven days and the start date 2026-11-16
- Then the starting values have the start date <start date>
- And creating gives <result>

Examples:

| n | time zone | time | start date | result |
|---|---|---|---|---|
| 01 | `Europe/Kyiv` | `2026-11-16T21:30:00Z` | 2026-11-16 | creating succeeds |
| 02 | `Europe/Kyiv` | `2026-11-16T22:30:00Z` | 2026-11-17 | "The start date cannot be in the past." |
| 03 | `America/New_York` | `2026-11-17T03:00:00Z` | 2026-11-16 | creating succeeds |
| 04 | `America/New_York` | `2026-11-17T05:00:00Z` | 2026-11-17 | "The start date cannot be in the past." |

### AC-045 (FR-003; FR-013) A fixed-days goal starting on an unscheduled day begins at its next scheduled day

- Given the registered user `f4.ac045.user@example.com`, who created the custom habit `Yoga`
- And `f4.ac045.user@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days Monday, Wednesday and Friday and the start date 2026-11-24
- When `f4.ac045.user@example.com` reads the evaluation calendar of the goal from 2026-11-23 to 2026-11-29
- Then it has 2 commitments, in this order: 2026-11-25 from `2026-11-25T00:00:00Z` to `2026-11-26T00:00:00Z`, and 2026-11-27 from `2026-11-27T00:00:00Z` to `2026-11-28T00:00:00Z`
- And it has no commitment on 2026-11-23, before the start date

### AC-046 (FR-007; US5-AS1) Starting from a custom habit gives only Build or Reduce from its intent

- Given the registered user `f4.ac046.user.<n>@example.com`, who created the custom habit `Cold shower` with the intent <intent>
- When `f4.ac046.user.<n>@example.com` starts a new goal from their custom habit `Cold shower`
- Then the starting values are: the habit `Cold shower` (custom), <intent>, the start date 2026-11-18, and nothing else: no measurement, no amount, no unit, no schedule, no name and no catalog suggestion text

Examples:

| n | intent |
|---|---|
| 01 | Build |
| 02 | Reduce |

### AC-047 (FR-001; FR-010; US5-AS2) A goal on a custom habit is based on it and named after it

- Given the registered user `f4.ac047.user@example.com`, who created the custom habit `Cold shower`
- When `f4.ac047.user@example.com` creates a goal on their custom habit `Cold shower` with Build, yes/no, fixed days on all seven days
- Then creating succeeds
- And reading the goal gives the name "Cold shower, daily", the habit name `Cold shower`, the kind custom, the category Other, Build, yes/no, fixed days on all seven days and the schedule kind Commitment-based

### AC-048 (FR-035; US5-AS3) A custom habit that goals use cannot be deleted

- Given the registered user `f4.ac048.user@example.com`, who created the custom habit `Cold shower`
- And `f4.ac048.user@example.com` created a goal on their custom habit `Cold shower` with Build, yes/no, fixed days on all seven days
- When `f4.ac048.user@example.com` deletes their custom habit `Cold shower`
- Then the result is "Goals use this custom habit, so it cannot be deleted."
- And reading their custom habit `Cold shower` still gives it
- And reading the goal still gives the habit name `Cold shower`

### AC-049 (FR-019; FR-035; US5-AS4) A custom habit that no goal uses is deleted

- Given the registered user `f4.ac049.user@example.com`, who created the custom habits `Cold shower` and `Plank`
- And `f4.ac049.user@example.com` created a goal on their custom habit `Plank` with Build, yes/no, fixed days Monday
- When `f4.ac049.user@example.com` deletes their custom habit `Cold shower`
- Then the custom habit is deleted
- And reading it gives "This custom habit does not exist."

### AC-050 (FR-010; FR-035; US5-AS5) Editing a custom habit renames its goals but never changes their settings

- Given the registered user `f4.ac050.user@example.com`, who created the custom habit `Cold shower` with the intent Build
- And `f4.ac050.user@example.com` created a goal on their custom habit `Cold shower` with Build, yes/no, fixed days on all seven days, and a second one with the same settings and the name "Morning cold"
- When `f4.ac050.user@example.com` edits their custom habit `Cold shower` to the name `Cold water` and the intent Reduce
- Then reading the first goal gives the name "Cold water, daily", the habit name `Cold water`, Build, yes/no and fixed days on all seven days
- And reading the second goal gives the name "Morning cold", the habit name `Cold water` and Build

### AC-051 (FR-035) Reading a custom habit gives the number of goals that use it

- Given the registered user `f4.ac051.user@example.com`, who created the custom habits `Cold shower` and `Plank`
- And `f4.ac051.user@example.com` created two goals on their custom habit `Cold shower` with Build, yes/no, fixed days Monday
- When `f4.ac051.user@example.com` reads their custom habits `Cold shower` and `Plank`
- Then `Cold shower` gives the number of goals 2 and `Plank` gives 0

### AC-052 (FR-031; US6-AS1) Browsing gives a registered user their goal count per habit and the tracked count

- Given the administrator `f4.ac052.admin.<n>@example.com`, a Published `A4.052 <n> Reading`, a Published `A4.052 <n> Yoga` and a Published `A4.052 <n> Water`
- And the registered user `f4.ac052.user.<n>@example.com`, who created two goals on `A4.052 <n> Reading` and one goal on `A4.052 <n> Water`, each with Build, yes/no, fixed days Monday
- When `f4.ac052.user.<n>@example.com` browses the catalog as `f4.ac052.user.<n>@example.com` with the search <search>
- Then <result>

Examples:

| n | search | result |
|---|---|---|
| 01 | `A4.052 01` | `A4.052 01 Reading` has the goal count 2, `A4.052 01 Yoga` 0 and `A4.052 01 Water` 1, and the tracked count is 2 |
| 02 | `A4.052 02 R` | the result is `A4.052 02 Reading` only, with the goal count 2, and the tracked count is 1 |
| 03 | `A4.052 03 Y` | the result is `A4.052 03 Yoga` only, with the goal count 0, and the tracked count is 0 |

### AC-053 (FR-031; US6-AS2) "Only habits I track" returns the Published habits with the user's goals, with the other filters

- Given the administrator `f4.ac053.admin.<n>@example.com`, a Published `A4.053 <n> Reading` with the category Mind, a Published `A4.053 <n> Yoga` with the category Body, a Published `A4.053 <n> Sugar` with the category Health and a Published `A4.053 <n> Coffee` with the category Health
- And the registered user `f4.ac053.user.<n>@example.com`, who created two goals on `A4.053 <n> Reading`, one goal on `A4.053 <n> Sugar` and one goal on `A4.053 <n> Coffee`, each with Build, yes/no, fixed days Monday
- And the administrator unpublished `A4.053 <n> Coffee` with confirmation
- When `f4.ac053.user.<n>@example.com` browses the catalog as `f4.ac053.user.<n>@example.com` with "Only habits I track", the search `A4.053 <n>` and <filter>
- Then the result is <result>

Examples:

| n | filter | result |
|---|---|---|
| 01 | no category | `A4.053 01 Reading` and `A4.053 01 Sugar`, and nothing else |
| 02 | the category Health | `A4.053 02 Sugar` only |
| 03 | the category Body | no habit |

### AC-054 (FR-031; US6-AS3) A habit's details give the user's goal count and their goals on it, in name order

- Given the administrator `f4.ac054.admin@example.com` and a Published `A4.054 Reading`
- And the registered user `f4.ac054.user@example.com`, who created goals on `A4.054 Reading`, in this order: one named "Morning pages", one named "evening pages", one named "Äpfel pages" and one with no name, each with Build, duration 15 minutes, fixed days Monday to Friday
- When `f4.ac054.user@example.com` reads the details of `A4.054 Reading` as `f4.ac054.user@example.com`
- Then the details give the goal count 4
- And the goals "A4.054 Reading, 15 min, 5 days a week", "evening pages", "Morning pages", "Äpfel pages", in this order, each with its identifier

### AC-055 (FR-032; US6-AS4) Anonymous visitors and administrators get the catalog without goal information

- Given the administrator `f4.ac055.admin.<n>@example.com` and a Published `A4.055 <n> Reading`
- And the registered user `f4.ac055.user.<n>@example.com`, who created a goal on `A4.055 <n> Reading` with Build, yes/no, fixed days Monday
- When <acting user> <action>
- Then <result>

Examples:

| n | acting user | action | result |
|---|---|---|---|
| 01 | the anonymous visitor | browses the catalog as the anonymous visitor with the search `A4.055 01` | the result contains `A4.055 01 Reading` with no goal count, and no tracked count |
| 02 | `f4.ac055.admin.02@example.com` | browses the catalog as `f4.ac055.admin.02@example.com` with the search `A4.055 02` | the result contains `A4.055 02 Reading` with no goal count, and no tracked count |
| 03 | the anonymous visitor | reads the details of `A4.055 03 Reading` as the anonymous visitor | the details have no goal count and no goals |
| 04 | `f4.ac055.admin.04@example.com` | reads the details of `A4.055 04 Reading` as `f4.ac055.admin.04@example.com` | the details have no goal count and no goals |
| 05 | the anonymous visitor | browses the catalog as the anonymous visitor with "Only habits I track" | the result is "You are not signed in." |
| 06 | `f4.ac055.admin.06@example.com` | browses the catalog as `f4.ac055.admin.06@example.com` with "Only habits I track" | the result is "Goals are for registered users." |

### AC-056 (FR-019; FR-031) Browsing and details without an acting user give the slice 2 result

- Given the administrator `f4.ac056.admin@example.com` and a Published `A4.056 Reading`
- And the registered user `f4.ac056.user@example.com`, who created a goal on `A4.056 Reading` with Build, yes/no, fixed days Monday
- When the catalog is browsed without an acting user with the search `A4.056`, and the details of `A4.056 Reading` are read without an acting user
- Then browsing contains `A4.056 Reading` with no goal count, and no tracked count
- And the details have no goal count and no goals

### AC-057 (FR-033; US7-AS1; US7-AS3; US7-AS5) Unpublishing a habit that goals use needs confirmation and gives only the number of goals

- Given the administrator `f4.ac057.admin.<n>@example.com` and a Published `A4.057 <n> Reading`
- And the registered users `f4.ac057.a.<n>@example.com` and `f4.ac057.b.<n>@example.com`
- And <goals>, each on `A4.057 <n> Reading` with Build, yes/no, fixed days Monday
- When `f4.ac057.admin.<n>@example.com` unpublishes `A4.057 <n> Reading` without confirming
- Then the result is "confirmation needed" with the number of goals <count> and the text <text>
- And the result carries nothing else: no email, no goal name, no goal identifier and no setting
- And browsing the catalog without an acting user with the search `A4.057 <n>` still contains `A4.057 <n> Reading`

Examples:

| n | goals | count | text |
|---|---|---|---|
| 01 | `f4.ac057.a.01@example.com` created one goal and `f4.ac057.b.01@example.com` created one goal | 2 | "2 goals use this habit." |
| 02 | `f4.ac057.a.02@example.com` created one goal | 1 | "1 goal uses this habit." |
| 03 | `f4.ac057.a.03@example.com` created two goals and `f4.ac057.b.03@example.com` created one goal | 3 | "3 goals use this habit." |

### AC-058 (FR-033; US7-AS2) Unpublishing with confirmation unpublishes a habit that goals use

- Given the administrator `f4.ac058.admin@example.com` and a Published `A4.058 Reading`
- And the registered users `f4.ac058.a@example.com` and `f4.ac058.b@example.com`, who each created a goal on `A4.058 Reading` with Build, yes/no, fixed days Monday
- When `f4.ac058.admin@example.com` unpublishes `A4.058 Reading` with confirmation
- Then `A4.058 Reading` is Unpublished
- And browsing the catalog without an acting user with the search `A4.058` does not contain it

### AC-059 (FR-019; FR-033; US7-AS4) A habit that no goal uses is unpublished at once

- Given the administrator `f4.ac059.admin.<n>@example.com` and a Published `A4.059 <n> Reading`
- When `f4.ac059.admin.<n>@example.com` unpublishes `A4.059 <n> Reading` <confirmation>
- Then `A4.059 <n> Reading` is Unpublished, with no "confirmation needed" result

Examples:

| n | confirmation |
|---|---|
| 01 | without confirming |
| 02 | with confirmation |

### AC-060 (FR-016; FR-034; US7-AS6) A goal on an Unpublished habit keeps working, and no new goal can use that habit

- Given the administrator `f4.ac060.admin.<n>@example.com` and a Published `A4.060 <n> Reading`
- And the registered user `f4.ac060.owner.<n>@example.com`, who created a goal on `A4.060 <n> Reading` with Build, duration 15 minutes, fixed days Monday to Friday
- And the registered user `f4.ac060.other.<n>@example.com`
- And the administrator unpublished `A4.060 <n> Reading` with confirmation
- When <acting user> <action>
- Then <result>

Examples:

| n | acting user | action | result |
|---|---|---|---|
| 01 | `f4.ac060.owner.01@example.com` | lists their goals | the list contains "A4.060 01 Reading, 15 min, 5 days a week" |
| 02 | `f4.ac060.owner.02@example.com` | reads the goal | the result has the habit name `A4.060 02 Reading`, that the habit is not in the catalog, Build, duration 15 minutes and fixed days Monday to Friday |
| 03 | `f4.ac060.owner.03@example.com` | starts a new goal from `A4.060 03 Reading` | the result is "This habit is not in the catalog." |
| 04 | `f4.ac060.owner.04@example.com` | creates a goal on `A4.060 04 Reading` with Build, yes/no, fixed days Monday | the result is "This habit is not in the catalog." |
| 05 | `f4.ac060.other.05@example.com` | creates a goal on `A4.060 05 Reading` with Build, yes/no, fixed days Monday | the result is "This habit is not in the catalog." |

### AC-061 (FR-034) A habit published again is in the catalog again for goals

- Given the administrator `f4.ac061.admin@example.com` and a Published `A4.061 Reading`
- And the registered user `f4.ac061.owner@example.com`, who created a goal on `A4.061 Reading` with Build, yes/no, fixed days Monday
- And the administrator unpublished `A4.061 Reading` with confirmation and then published it again
- And the registered user `f4.ac061.other@example.com`
- When `f4.ac061.other@example.com` creates a goal on `A4.061 Reading` with Build, yes/no, fixed days Monday
- Then creating succeeds
- And `f4.ac061.owner@example.com` reading their goal gives that the habit is in the catalog

### AC-062 (FR-010; FR-016; US7-AS7) A renamed catalog habit renames the goals based on it

- Given the administrator `f4.ac062.admin@example.com` and a Published `A4.062 Reading`
- And the registered user `f4.ac062.user@example.com`, who created a goal on `A4.062 Reading` with Build, duration 15 minutes, fixed days Monday to Friday, and a second one with the same settings and the name "Morning pages"
- When `f4.ac062.admin@example.com` edits `A4.062 Reading` to the name `A4.062 Reading books`
- Then reading the first goal gives the name "A4.062 Reading books, 15 min, 5 days a week" and the habit name `A4.062 Reading books`
- And reading the second goal gives the name "Morning pages" and the habit name `A4.062 Reading books`

### AC-063 (FR-006; FR-007; US8-AS4) A new goal can start without a habit, but not be created without one

- Given the registered user `f4.ac063.user@example.com`
- When `f4.ac063.user@example.com` starts a new goal with no habit, and then creates a goal with no habit and Build, yes/no, fixed days Monday
- Then the starting values have no habit, the start date 2026-11-18, and nothing else
- And creating gives "Choose a habit."
- And `f4.ac063.user@example.com`'s goal list is empty

### AC-064 (FR-040; US9-AS1) Demo loading gives each created demo account its demo goals

- Given the AC database is reset, so no account and no habit exists
- When demo loading runs
- Then its results are "created", "demo custom habits added" and "demo goals added" for each of `demo1@example.com`, `demo2@example.com` and `demo3@example.com`, and the catalog result "seed catalog loaded"
- And `demo1@example.com`'s goal list is, in this order: "No phone in bed, avoided every day" (habit `No phone in bed`, custom, Reduce, yes/no, Commitment-based) and "Reading, 15 min, 5 days a week" (habit `Reading`, catalog, Build, duration, Commitment-based)
- And `demo2@example.com`'s goal list is, in this order: "Meditation, 10 min, 3 days a week" (habit `Meditation`, catalog, Build, duration, Commitment-based, start date 2026-11-25) and "Morning yoga" (habit `Yoga`, catalog, Build, yes/no, Period-based)
- And `demo3@example.com`'s goal list is, in this order: "Coffee, at most 2 cups a day" (habit `Coffee`, catalog, Reduce, count), "Late snacks, at most once a day" (habit `Late snacks`, custom, Reduce, yes/no) and "Sugar, at most 1 sweet a day" (habit `Sugar`, catalog, Reduce, count), each Period-based
- And reading each demo goal gives: "No phone in bed, avoided every day" fixed days on all seven days; "Reading, 15 min, 5 days a week" duration 15 minutes, fixed days Monday to Friday; "Meditation, 10 min, 3 days a week" duration 10 minutes, fixed days Monday, Wednesday and Friday; "Morning yoga" 3 times per week; "Coffee, at most 2 cups a day" count 2 `cups`, 1 time per day; "Late snacks, at most once a day" 1 time per day; "Sugar, at most 1 sweet a day" count 1 `sweet`, 1 time per day
- And reading each demo goal gives the start date 2026-11-18, except "Meditation, 10 min, 3 days a week" with 2026-11-25, and the time zone `UTC`

### AC-065 (FR-040; US9-AS2) Demo loading run again gives existing demo accounts no goals

- Given the AC database is reset, so no account and no habit exists
- And demo loading ran
- When at `2026-11-19T10:00:00Z` demo loading runs again
- Then its account results are "already exists" for `demo1@example.com`, `demo2@example.com` and `demo3@example.com`, and it gives no goal result
- And each demo account's goal list is the same as after the first run: 2 goals for `demo1@example.com`, 2 for `demo2@example.com` and 3 for `demo3@example.com`, with the same names
- And reading each demo goal gives the same start date as after the first run: 2026-11-18, except "Meditation, 10 min, 3 days a week" with 2026-11-25

### AC-066 (FR-040; US9-AS3) Demo goals are left out when their catalog habit is not Published

- Given the AC database is reset, so no account and no habit exists
- And the administrator `f4.ac066.admin@example.com` created a Published `Yoga`, a Published `Meditation`, a Published `Sugar`, a Draft `Reading` and a Draft `Coffee`
- When demo loading runs
- Then its goal result is "demo goals added" for each of `demo1@example.com`, `demo2@example.com` and `demo3@example.com`
- And `demo1@example.com`'s goal list is exactly "No phone in bed, avoided every day"
- And `demo2@example.com`'s goal list is exactly "Meditation, 10 min, 3 days a week" and "Morning yoga"
- And `demo3@example.com`'s goal list is exactly "Late snacks, at most once a day" and "Sugar, at most 1 sweet a day"

### AC-067 (FR-040) A demo goal finds its catalog habit by name ignoring case, and only "Coffee" may be Unpublished

- Given the AC database is reset, so no account and no habit exists
- And the administrator `f4.ac067.admin@example.com` created a Published `reading`, an Unpublished `YOGA`, a Published `Meditation`, a Published `Sugar` and an Unpublished `coffee`
- When demo loading runs
- Then `demo1@example.com`'s goal list is exactly "No phone in bed, avoided every day" and "reading, 15 min, 5 days a week", in this order
- And `demo2@example.com`'s goal list is exactly "Meditation, 10 min, 3 days a week"
- And `demo3@example.com`'s goal list is exactly "coffee, at most 2 cups a day", "Late snacks, at most once a day" and "Sugar, at most 1 sweet a day", in this order
- And `demo3@example.com` reading the goal "coffee, at most 2 cups a day" gives that the habit is not in the catalog

### AC-068 (FR-040; US9-AS4) Demo loading results name the demo email but never a goal, habit or account name

- Given the AC database is reset, so no account and no habit exists
- When demo loading runs
- Then every goal result names one demo email and the text "demo goals added"
- And no result contains "Reading", "Yoga", "Morning yoga", "Meditation", "Sugar", "Coffee", "No phone in bed", "Late snacks", "Demo One", "sweet" or "cups"

### AC-069 (FR-028; FR-034; FR-040; US9-AS5) The demo shows a goal on an Unpublished habit, a goal with its own name and an account name

- Given the AC database is reset, so no account and no habit exists
- And demo loading ran
- When each demo account reads its goals and its profile, and `demo1@example.com` reads the Home heading
- Then `demo3@example.com`'s goal "Coffee, at most 2 cups a day" exists, and reading it gives the habit name `Coffee` and that the habit is not in the catalog
- And `demo2@example.com`'s goal on `Yoga` has the name "Morning yoga", which is its own
- And `demo1@example.com`'s profile gives the name "Demo One"; the profiles of `demo2@example.com` and `demo3@example.com` give no name
- And `demo1@example.com`'s Home heading gives the greeting "Good morning, Demo One"

### AC-070 (FR-019; FR-024; FR-025; US10-AS1; US10-AS2) Registration keeps a named browser time zone, and otherwise gives the default

- Given the anonymous visitor registered the email `f4.ac070.user.<n>@example.com` with the password `walking42` and <browser time zone>, and signed in
- When `f4.ac070.user.<n>@example.com` reads their time zone
- Then registration succeeded and the time zone is <time zone>

Examples:

| n | browser time zone | time zone |
|---|---|---|
| 01 | the browser time zone `Europe/Kyiv` | `Europe/Kyiv` |
| 02 | no browser time zone | `UTC` |
| 03 | the browser time zone `Mars/Olympus` | `UTC` |
| 04 | the browser time zone `FLE Standard Time` | `UTC` |
| 05 | the browser time zone `Etc/GMT+3` | `UTC` |
| 06 | the browser time zone `EST` | `UTC` |
| 07 | the browser time zone `CET` | `UTC` |
| 08 | the browser time zone `GMT` | `UTC` |
| 09 | the browser time zone `+02:00` | `UTC` |
| 10 | the browser time zone `europe/kyiv` | `UTC` |
| 11 | the browser time zone `""` | `UTC` |
| 12 | the browser time zone `Europe/Kiev` | `Europe/Kiev` |
| 13 | the browser time zone `Asia/Calcutta` | `Asia/Calcutta` |
| 14 | the browser time zone `US/Eastern` | `US/Eastern` |
| 15 | the browser time zone `America/Argentina/Buenos_Aires` | `America/Argentina/Buenos_Aires` |
| 16 | the browser time zone `UTC` | `UTC` |
| 17 | the browser time zone `America/New_York` | `America/New_York` |

### AC-071 (FR-011; FR-026; FR-027; US10-AS4) After a move west, a fixed-days goal keeps the old time zone up to its current date

- Given the registered user `f4.ac071.user@example.com` in `Europe/Kyiv`, who created the custom habit `Yoga`
- And at `2026-11-16T08:00:00Z` `f4.ac071.user@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days Monday, Wednesday and Friday and the start date 2026-11-16
- And at `2026-11-18T10:00:00Z` (12:00 in Kyiv, 05:00 in New York) `f4.ac071.user@example.com` changed their time zone to `America/New_York`, and the change succeeded
- When at `2026-11-18T10:00:00Z` `f4.ac071.user@example.com` reads the evaluation calendar of the goal from 2026-11-16 to 2026-11-23
- Then it has 4 commitments, in this order:
- And 2026-11-16 from `2026-11-15T22:00:00Z` to `2026-11-16T22:00:00Z`
- And 2026-11-18 from `2026-11-17T22:00:00Z` to `2026-11-18T22:00:00Z`
- And 2026-11-20 from `2026-11-20T05:00:00Z` to `2026-11-21T05:00:00Z`
- And 2026-11-23 from `2026-11-23T05:00:00Z` to `2026-11-24T05:00:00Z`

### AC-072 (FR-026; FR-027; US10-AS5) Each goal follows a time zone change from its own next day or week

- Given the registered user `f4.ac072.user.<n>@example.com` in `Europe/Kyiv`, who created the custom habit `Practice`
- And at `2026-11-16T08:00:00Z` `f4.ac072.user.<n>@example.com` created a goal on their custom habit `Practice` with Build, yes/no, <schedule> and the start date <start date>
- And at `2026-11-18T10:00:00Z` `f4.ac072.user.<n>@example.com` changed their time zone to `America/New_York`
- When at `2026-11-18T10:00:00Z` `f4.ac072.user.<n>@example.com` reads the evaluation calendar of the goal from <from> to <to>
- Then it has, in this order, <entries>

Examples:

| n | schedule | start date | from | to | entries |
|---|---|---|---|---|---|
| 01 | 1 time per day | 2026-11-16 | 2026-11-18 | 2026-11-19 | 2026-11-18 from `2026-11-17T22:00:00Z` to `2026-11-18T22:00:00Z`; 2026-11-19 from `2026-11-19T05:00:00Z` to `2026-11-20T05:00:00Z` |
| 02 | 3 times per week | 2026-11-16 | 2026-11-16 | 2026-11-29 | 2026-11-16 to 2026-11-22 from `2026-11-15T22:00:00Z` to `2026-11-22T22:00:00Z`; 2026-11-23 to 2026-11-29 from `2026-11-23T05:00:00Z` to `2026-11-30T05:00:00Z` |
| 03 | 1 time per day | 2026-12-01 | 2026-12-01 | 2026-12-01 | 2026-12-01 from `2026-12-01T05:00:00Z` to `2026-12-02T05:00:00Z` |

### AC-073 (FR-016; FR-026; US10-AS6) Reading a goal gives the version of its current date and the one later version

- Given the registered user `f4.ac073.user.<n>@example.com` in `Europe/Kyiv`, who created the custom habit `Practice`
- And at `2026-11-16T08:00:00Z` `f4.ac073.user.<n>@example.com` created a goal on their custom habit `Practice` with Build, yes/no, <schedule> and the start date <start date>
- And at `2026-11-18T10:00:00Z` `f4.ac073.user.<n>@example.com` changed their time zone to `America/New_York`
- When at <time> `f4.ac073.user.<n>@example.com` reads the goal
- Then the result has <versions>
- And Build, yes/no, <schedule> and the start date <start date>, unchanged

Examples:

| n | schedule | start date | time | versions |
|---|---|---|---|---|
| 01 | fixed days Monday, Wednesday and Friday | 2026-11-16 | `2026-11-18T12:00:00Z` | the time zone `Europe/Kyiv` from 2026-11-16 and a later version `America/New_York` from 2026-11-19 |
| 02 | fixed days Monday, Wednesday and Friday | 2026-11-16 | `2026-11-19T04:59:59Z` | the time zone `Europe/Kyiv` from 2026-11-16 and a later version `America/New_York` from 2026-11-19 |
| 03 | fixed days Monday, Wednesday and Friday | 2026-11-16 | `2026-11-19T05:00:00Z` | the time zone `America/New_York` from 2026-11-19 and no later version |
| 04 | fixed days Monday, Wednesday and Friday | 2026-11-16 | `2026-11-20T12:00:00Z` | the time zone `America/New_York` from 2026-11-19 and no later version |
| 05 | 3 times per week | 2026-11-16 | `2026-11-18T12:00:00Z` | the time zone `Europe/Kyiv` from 2026-11-16 and a later version `America/New_York` from 2026-11-23 |
| 06 | 1 time per day | 2026-12-01 | `2026-11-18T12:00:00Z` | the time zone `America/New_York` from 2026-12-01 and no later version |

### AC-074 (FR-024; FR-026; US10-AS7) A value that is not a named time zone is refused and changes nothing

- Given the registered user `f4.ac074.user.<n>@example.com` in `Europe/Kyiv`, who created the custom habit `Yoga`
- And at `2026-11-16T08:00:00Z` `f4.ac074.user.<n>@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days Monday, Wednesday and Friday and the start date 2026-11-16
- When `f4.ac074.user.<n>@example.com` changes their time zone to <value>
- Then the result is "Choose a time zone from the list."
- And reading their time zone gives `Europe/Kyiv`
- And reading the goal gives the time zone `Europe/Kyiv` from 2026-11-16 and no later version

Examples:

| n | value |
|---|---|
| 01 | `Mars/Olympus` |
| 02 | `FLE Standard Time` |
| 03 | `Etc/GMT+3` |
| 04 | `Etc/UTC` |
| 05 | `EST` |
| 06 | `+02:00` |
| 07 | `europe/kyiv` |
| 08 | `""` |

### AC-075 (FR-024; FR-026) A named time zone is kept as given, and the same time zone changes nothing

- Given the registered user `f4.ac075.user.<n>@example.com` in `Europe/Kyiv`, who created the custom habit `Yoga`
- And at `2026-11-16T08:00:00Z` `f4.ac075.user.<n>@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days Monday, Wednesday and Friday and the start date 2026-11-16
- When `f4.ac075.user.<n>@example.com` changes their time zone to <value>
- Then the change succeeds and reading their time zone gives <value>
- And reading the goal gives the time zone `Europe/Kyiv` from 2026-11-16 and <later>

Examples:

| n | value | later |
|---|---|---|
| 01 | `Europe/Kyiv` | no later version |
| 02 | `Europe/Kiev` | a later version `Europe/Kiev` from 2026-11-19 |
| 03 | `UTC` | a later version `UTC` from 2026-11-19 |

### AC-076 (FR-012; FR-026) "Today" follows the new time zone at once

- Given the registered user `f4.ac076.user@example.com` in `Europe/Kyiv`, who created the custom habit `Yoga`
- And at `2026-11-18T23:30:00Z` (01:30 on 19 November in Kyiv) `f4.ac076.user@example.com` started a new goal from their custom habit `Yoga`, and the starting values had the start date 2026-11-19
- When at `2026-11-18T23:30:00Z` `f4.ac076.user@example.com` changes their time zone to `America/New_York`, starts a new goal from their custom habit `Yoga`, and creates a goal on it with Build, yes/no, fixed days on all seven days and the start date 2026-11-18
- Then the starting values have the start date 2026-11-18
- And creating succeeds

### AC-077 (FR-014; FR-026) After a move west, a commitment already over is not the next one

- Given the registered user `f4.ac077.user.<n>@example.com` in `Europe/Kyiv`, who created the custom habit `Yoga`
- And at `2026-11-16T08:00:00Z` `f4.ac077.user.<n>@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days Monday, Wednesday and Friday and the start date 2026-11-16
- And at `2026-11-18T10:00:00Z` `f4.ac077.user.<n>@example.com` changed their time zone to `America/New_York`
- When at <time> `f4.ac077.user.<n>@example.com` lists their goals
- Then the goal gives next commitment <next>

Examples:

| n | time | next |
|---|---|---|
| 01 | `2026-11-18T21:59:59Z` | 2026-11-18 |
| 02 | `2026-11-18T23:00:00Z` | 2026-11-20 |

### AC-078 (FR-014; FR-026) After a move east, a commitment in progress is still the next one

- Given the registered user `f4.ac078.user.<n>@example.com` in `America/New_York`, who created the custom habit `Yoga`
- And at `2026-11-16T12:00:00Z` `f4.ac078.user.<n>@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days Monday, Wednesday and Friday and the start date 2026-11-16
- And at `2026-11-18T10:00:00Z` `f4.ac078.user.<n>@example.com` changed their time zone to `Asia/Tokyo`
- When at <time> `f4.ac078.user.<n>@example.com` lists their goals
- Then the goal gives next commitment <next>

Examples:

| n | time | next |
|---|---|---|
| 01 | `2026-11-18T16:00:00Z` | 2026-11-18 |
| 02 | `2026-11-19T04:59:59Z` | 2026-11-18 |
| 03 | `2026-11-19T05:00:00Z` | 2026-11-20 |

### AC-079 (FR-026; US10-AS8) Administrators read and change their own time zone; anonymous visitors cannot

- Given the administrator `f4.ac079.admin@example.com`
- When `f4.ac079.admin@example.com` reads their time zone, changes it to `Europe/Kyiv` and reads it again, and the anonymous visitor reads a time zone and changes a time zone to `Europe/Kyiv`
- Then the administrator's first read gives `UTC`, the change succeeds and the second read gives `Europe/Kyiv`
- And both of the anonymous visitor's calls give "You are not signed in."

### AC-080 (FR-024; US10-AS9) A time zone is given only to its own account

- Given the registered user `f4.ac080.a@example.com` in `Europe/Kyiv`
- And the registered user `f4.ac080.b@example.com` and the administrator `f4.ac080.admin@example.com`
- When `f4.ac080.b@example.com` and `f4.ac080.admin@example.com` each read their time zone
- Then each gets `UTC`, their own, and neither result contains `Europe/Kyiv`

### AC-081 (FR-026; US10-AS10) A second change replaces a version not yet begun, and follows a version already in effect

- Given the registered user `f4.ac081.user.<n>@example.com` in `Europe/Kyiv`, who created the custom habit `Practice`
- And at `2026-11-16T08:00:00Z` `f4.ac081.user.<n>@example.com` created a goal on their custom habit `Practice` with Build, yes/no, <schedule> and the start date <start date>
- And at `2026-11-18T10:00:00Z` `f4.ac081.user.<n>@example.com` changed their time zone to `America/New_York`
- And at `2026-11-19T17:00:00Z` (12:00 in New York) `f4.ac081.user.<n>@example.com` changed their time zone to `Asia/Tokyo`
- When at `2026-11-19T17:00:00Z` `f4.ac081.user.<n>@example.com` reads the goal and reads its evaluation calendar from <from> to <to>
- Then reading gives <versions>
- And the calendar has, in this order, <entries>

Examples:

| n | schedule | start date | from | to | versions | entries |
|---|---|---|---|---|---|---|
| 01 | 3 times per week | 2026-11-16 | 2026-11-16 | 2026-11-29 | the time zone `Europe/Kyiv` from 2026-11-16 and a later version `Asia/Tokyo` from 2026-11-23 | 2026-11-16 to 2026-11-22 from `2026-11-15T22:00:00Z` to `2026-11-22T22:00:00Z`; 2026-11-23 to 2026-11-29 from `2026-11-22T15:00:00Z` to `2026-11-29T15:00:00Z` |
| 02 | 1 time per day | 2026-11-16 | 2026-11-18 | 2026-11-20 | the time zone `Asia/Tokyo` from 2026-11-20 and no later version | 2026-11-18 from `2026-11-17T22:00:00Z` to `2026-11-18T22:00:00Z`; 2026-11-19 from `2026-11-19T05:00:00Z` to `2026-11-20T05:00:00Z`; 2026-11-20 from `2026-11-19T15:00:00Z` to `2026-11-20T15:00:00Z` |
| 03 | 1 time per day | 2026-12-01 | 2026-12-01 | 2026-12-01 | the time zone `Asia/Tokyo` from 2026-12-01 and no later version | 2026-12-01 from `2026-11-30T15:00:00Z` to `2026-12-01T15:00:00Z` |

### AC-082 (FR-026; FR-027) After a large move east, a version already begun is in effect, and a second change follows it

- Given the registered user `f4.ac082.user@example.com` in `America/New_York`, who created the custom habit `Water`
- And at `2026-11-18T06:00:00Z` `f4.ac082.user@example.com` created a goal on their custom habit `Water` with Build, yes/no, 1 time per day and the start date 2026-11-18
- And at `2026-11-18T16:00:00Z` `f4.ac082.user@example.com` changed their time zone to `Asia/Tokyo`, whose 19 November began at `2026-11-18T15:00:00Z`
- And at `2026-11-18T16:30:00Z` `f4.ac082.user@example.com` changed their time zone to `Europe/Kyiv`
- When at `2026-11-18T16:30:00Z` `f4.ac082.user@example.com` reads the goal and reads its evaluation calendar from 2026-11-18 to 2026-11-20
- Then reading gives the time zone `Asia/Tokyo` from 2026-11-19 and a later version `Europe/Kyiv` from 2026-11-20
- And the calendar has 3 evaluation periods, in this order: 2026-11-18 from `2026-11-18T05:00:00Z` to `2026-11-19T05:00:00Z`; 2026-11-19 from `2026-11-18T15:00:00Z` to `2026-11-19T15:00:00Z`; 2026-11-20 from `2026-11-19T22:00:00Z` to `2026-11-20T22:00:00Z`

### AC-083 (FR-027) A new version's first period that has already ended is kept

- Given the registered user `f4.ac083.user@example.com` in `Pacific/Pago_Pago`, who created the custom habit `Water`
- And at `2026-11-18T12:00:00Z` `f4.ac083.user@example.com` created a goal on their custom habit `Water` with Build, yes/no, 1 time per day and the start date 2026-11-18
- And at `2026-11-19T10:30:00Z` (23:30 on 18 November in Pago Pago) `f4.ac083.user@example.com` changed their time zone to `Pacific/Kiritimati`, whose 19 November ended at `2026-11-19T10:00:00Z`
- When at `2026-11-19T10:30:00Z` `f4.ac083.user@example.com` reads the evaluation calendar of the goal from 2026-11-18 to 2026-11-20 and reads the goal
- Then the calendar has 3 evaluation periods, in this order: 2026-11-18 from `2026-11-18T11:00:00Z` to `2026-11-19T11:00:00Z`; 2026-11-19 from `2026-11-18T10:00:00Z` to `2026-11-19T10:00:00Z`; 2026-11-20 from `2026-11-19T10:00:00Z` to `2026-11-20T10:00:00Z`
- And reading gives the time zone `Pacific/Kiritimati` from 2026-11-19 and no later version

### AC-084 (FR-019; FR-028; US11-AS1) An account name is trimmed, and empty means no name

- Given the anonymous visitor registered the email `f4.ac084.user.<n>@example.com` with the password `walking42` and <name>, and signed in
- When `f4.ac084.user.<n>@example.com` reads their profile
- Then registration succeeded and the profile gives <result> and the email `f4.ac084.user.<n>@example.com`

Examples:

| n | name | result |
|---|---|---|
| 01 | the name "  Yuri  " | the name "Yuri" |
| 02 | no name | no name |
| 03 | the name `"   "` | no name |
| 04 | the name `""` | no name |

### AC-085 (FR-028; US11-AS2) An account name has at most 60 characters

- Given the registered user `f4.ac085.named@example.com`, who changed their name to "Yuri"
- When the anonymous visitor registers the email `f4.ac085.new@example.com` with the password `walking42` and a name of 61 characters, then registers the same email with the password `walking42` and a name of 60 characters
- And `f4.ac085.named@example.com` changes their name to a text of 61 characters, then reads their profile, then changes their name to a text of 60 characters and reads their profile again
- Then the first registration gives "The name can have at most 60 characters." and the second registration succeeds
- And signed in, `f4.ac085.new@example.com`'s profile gives the text of 60 characters as the name
- And `f4.ac085.named@example.com`'s first change gives "The name can have at most 60 characters.", the first profile read gives "Yuri", the second change succeeds and the second profile read gives the text of 60 characters

### AC-086 (FR-029; US11-AS3) The Home greeting follows the time of day in the account's time zone

- Given the anonymous visitor registered the email `f4.ac086.user.<n>@example.com` with the password `walking42`, the browser time zone `Europe/Kyiv` and the name "Yuri", and signed in
- When at <time> `f4.ac086.user.<n>@example.com` reads the Home heading
- Then the greeting is <greeting> and the date is "Wednesday, 18 November"

Examples:

| n | time | greeting |
|---|---|---|
| 01 | `2026-11-18T02:59:00Z` (04:59 in Kyiv) | "Good evening, Yuri" |
| 02 | `2026-11-18T03:00:00Z` (05:00 in Kyiv) | "Good morning, Yuri" |
| 03 | `2026-11-18T09:59:00Z` (11:59 in Kyiv) | "Good morning, Yuri" |
| 04 | `2026-11-18T10:00:00Z` (12:00 in Kyiv) | "Good afternoon, Yuri" |
| 05 | `2026-11-18T15:59:00Z` (17:59 in Kyiv) | "Good afternoon, Yuri" |
| 06 | `2026-11-18T16:00:00Z` (18:00 in Kyiv) | "Good evening, Yuri" |
| 07 | `2026-11-18T21:59:59Z` (23:59:59 in Kyiv) | "Good evening, Yuri" |

### AC-087 (FR-029; US11-AS4) Without a name the greeting has no name

- Given the registered user `f4.ac087.user@example.com`
- When at `2026-11-18T19:00:00Z` `f4.ac087.user@example.com` reads the Home heading
- Then the greeting is "Good evening" and the date is "Wednesday, 18 November"

### AC-088 (FR-029; US11-AS5) The Home date is today in the account's time zone, in English whatever the culture

- Given the registered user `f4.ac088.user.<n>@example.com` in `Europe/Kyiv`
- And the current culture of the test is <culture>
- When at `2026-11-16T23:30:00Z` `f4.ac088.user.<n>@example.com` reads the Home heading
- Then the date is "Tuesday, 17 November" and the greeting is "Good evening"

Examples:

| n | culture |
|---|---|
| 01 | the invariant culture |
| 02 | `uk-UA` |
| 03 | `de-DE` |

### AC-089 (FR-030; US11-AS6) The profile gives "member since" with the month and year of creation in the account's time zone

- Given at <time> the anonymous visitor registered the email `f4.ac089.user.<n>@example.com` with the password `walking42` and <browser time zone>, and signed in
- When `f4.ac089.user.<n>@example.com` reads their profile
- Then the profile gives <member since>

Examples:

| n | time | browser time zone | member since |
|---|---|---|---|
| 01 | `2026-11-17T10:00:00Z` | the browser time zone `Europe/Kyiv` | "member since November 2026" |
| 02 | `2026-11-30T22:30:00Z` | the browser time zone `Europe/Kyiv` | "member since December 2026" |
| 03 | `2026-11-30T22:30:00Z` | no browser time zone | "member since November 2026" |

### AC-090 (FR-028; FR-029; US11-AS7) An account name is given only to its own account; administrators have their own

- Given the anonymous visitor registered the email `f4.ac090.a@example.com` with the password `walking42` and the name "Yuri", and signed in
- And the registered user `f4.ac090.b@example.com` and the administrator `f4.ac090.admin@example.com`
- When `f4.ac090.b@example.com` reads their profile, and `f4.ac090.admin@example.com` changes their name to "Ada Admin", reads their profile and reads the Home heading
- Then `f4.ac090.b@example.com`'s profile gives no name, the email `f4.ac090.b@example.com`, and does not contain "Yuri"
- And the administrator's change succeeds, their profile gives the name "Ada Admin" and the email `f4.ac090.admin@example.com`, and the Home heading gives the greeting "Good morning, Ada Admin" (10:00 in `UTC`)

### AC-091 (FR-023; FR-028; FR-029; FR-030; US11-AS8) An anonymous visitor gets no Home heading, no name change and no profile

- When the anonymous visitor <action>
- Then the result is "You are not signed in."

Examples:

| n | action |
|---|---|
| 01 | reads the Home heading |
| 02 | changes their name to "Yuri" |
| 03 | reads their profile |

### AC-092 (FR-007) A goal keeps the values it was created with, whatever happens to the suggested starting goal

- Given the administrator `f4.ac092.admin@example.com` and a Published `A4.092 Reading` with the suggested starting goal Build, duration 15 minutes, fixed days Monday to Friday
- And the registered user `f4.ac092.user@example.com`, who started a new goal from `A4.092 Reading`, and the starting values were Build, duration 15 minutes, fixed days Monday to Friday
- And `f4.ac092.admin@example.com` edited `A4.092 Reading` so that its only suggested starting goal is Build, count 20 `pages`, 3 times per week
- And `f4.ac092.user@example.com` created a goal on `A4.092 Reading` with Build, duration 15 minutes, fixed days Monday to Friday
- And `f4.ac092.admin@example.com` edited `A4.092 Reading` so that it has no suggested starting goal
- When `f4.ac092.user@example.com` reads the goal
- Then the result has Build, duration 15 minutes, fixed days Monday to Friday and the target text "15 min, 5 days a week"

### AC-093 (FR-033) A confirmed unpublish acts even when goals were added after the count was given

- Given the administrator `f4.ac093.admin@example.com` and a Published `A4.093 Reading`
- And the registered user `f4.ac093.a@example.com`, who created a goal on `A4.093 Reading` with Build, yes/no, fixed days Monday
- And `f4.ac093.admin@example.com` unpublished `A4.093 Reading` without confirming, and the result was "confirmation needed" with the number of goals 1 and the text "1 goal uses this habit."
- And the registered user `f4.ac093.b@example.com`, who then created a goal on `A4.093 Reading` with Build, yes/no, fixed days Monday
- When `f4.ac093.admin@example.com` unpublishes `A4.093 Reading` with confirmation
- Then `A4.093 Reading` is Unpublished
- And both goals still exist for their owners

### AC-094 (FR-028) At registration, the name is checked after slice 1's checks

- When the anonymous visitor registers the email `f4.ac094.user@example.com` with the password `short` and a name of 61 characters
- Then registration does not succeed, and its result is not "The name can have at most 60 characters."
- And registering `f4.ac094.user@example.com` afterwards with the password `walking42` and no name succeeds
