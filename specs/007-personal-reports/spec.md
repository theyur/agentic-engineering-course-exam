# Feature Specification: Personal reports

**Feature Branch**: `claude/speckit-specify-slice-6-990a9c`

**Created**: 2026-11-20

**Status**: Draft

**Input**: User description: "slice 6"

Roadmap outcome (slice 6, `personal-reports`): a user sees how consistent they are: a dashboard of upcoming, due
and overdue commitments and Missed results, consistency with its count, strict streaks with their unit, trends
(SPEC §6). The roadmap adds: consistency and streak formatting come from one shared formatter (constitution IV);
a correction's effect on reports is verified here (slice 5 note); the signed-in Home gets its stat tiles, Today
list, GoalSummary and recent notes, the goal page its GoalStats and trend, and the visitor Home its GoalStats card
from demo data (roadmap "Pages across slices").

## Clarifications

### Session 2026-11-20 (specify)

Answered by the owner during `/speckit-specify` (the three open questions of the first draft and
the roadmap's technical row):

- Q: Does a Pending result whose records already reach the target extend the current streak before its grace
  period ends (SPEC §11.7, first part)? → A: No. Pending never extends and never resets a streak; a result
  extends it only once it is Met at the end of its grace period. `CONTEXT.md`'s Pending entry ("never resets a
  streak") stays and gains "and never extends one" (D-1; FR-003, US2-AS4).
- Q: How does the dashboard show upcoming commitments (SPEC §6), given that the design's Home has only Due,
  Pending and Missed rows and four tiles? → A: As "Upcoming" rows at the end of the Today list, with no tile and
  no action: for each goal with no Due result, its earliest Upcoming commitment or period within the next 6 days
  (D-2; FR-012, US6-AS8, HP-003).
- Q: Whose data does the visitor Home's example GoalStats card show? → A: Fixed example figures that belong to no
  account, rendered by the shared GoalStats component and formatter and labelled "Example goal"; no demo
  account's private goal reaches an anonymous visitor (D-3; HP-004).
- Q: Are derived figures cached (ARCHITECTURE §9)? → A: No. Every figure is computed on read from the stored
  records, the settings versions and the supplied time, as every state is (ADR 0018); a later slice that needs
  caching decides it with its own ADR (D-4; HP-009;
  [ADR 0020](../../docs/adr/0020-derived-figures-on-read.md)).
- Q: Roadmap decision R3: do personal reports come before excusals and pauses? → A: Yes. The cost is accepted:
  SPEC §11.2 may rework period evaluation in slice 7, which then also extends this slice's figures with the
  excluded states (roadmap slice note 7).

### Session 2026-11-20 (clarify)

- Q: How is a consistency percentage rounded? → A: Down to a whole percentage: 2 of 3 is "66% (2 of 3)", 199 of 200
  "99% (199 of 200)", so "100%" means every counted result was Met and no goal appears to reach a band threshold
  it has not reached (FR-002, US1-AS4).
- Q: Which Missed results are "Missed this week" on the Home? → A: Those that became Missed this week: their grace
  period ended at or after Monday 00:00 of the current week in the account's time zone. The previous week's
  Friday (its grace period ends exactly at Monday 00:00, which counts, slice 5 FR-016), Saturday, Sunday and
  per-week period therefore appear early in the week, and its Thursday does not. Around a clock change in that
  weekend, Friday's grace end moves to Monday 01:00 (counted) or Sunday 23:00 (not counted) (FR-012, US6-AS4).
- Q: Over which results are a goal's personal figures computed? → A: The whole current attempt (the report range),
  not a rolling window; the community views' rolling 30 days (SPEC §7) are slice 9's own figure ("Terms used
  below", FR-001, FR-003).
- Q: What does a trend bar show for a commitment-based count or duration goal? → A: One bar per commitment, the last
  14, each its recorded amount against the goal's amount; weekly bars are only for per-week goals and
  commitment-based yes/no goals, and the TrendChart design note "weeks for … commitment-based goals" is narrowed to
  yes/no ones (FR-006, US4-AS3).
- Q: Does slice 6 add demo data beyond `demo4`'s 28 days? → A: Yes: a new demo account `demo5@example.com` with
  goals that started 84 days (12 weeks) before the run, so that trends fill their bars and streaks are longer. Slice
  5's locked criteria say demo history loading names `demo4` and no other account (F005 AC-051, AC-059), so `demo5`
  is created by a fourth startup task, *demo report loading*, after demo history loading, by the same rules (ADR
  0007, 0013, 0018); F001, F004 and F005 stay locked. The task's order and the amended "later slices" rule of ADR
  0007 and ADR 0018 are recorded in [ADR 0021](../../docs/adr/0021-demo-report-loading.md) (FR-032, HP-011,
  US8-AS7 to US8-AS10).

## Decisions this slice cites

Recorded in earlier slices and not decided again here:

- Use cases take the acting user as an explicit parameter and return named outcomes that carry the literal text
  this spec gives; expected refusals are outcomes, not errors ([ADR 0005](../../docs/adr/0005-use-case-convention.md)).
- The administrator role grants no access to private data (slice 1 FR-015; SPEC §3, §8). Goals are for
  registered users only (slice 4 FR-021).
- Partly built pages show only the sections and links whose slice has shipped
  ([ADR 0008](../../docs/adr/0008-partly-built-pages.md)). Excused and Paused (slice 7), earlier attempts and
  ended goals (slice 8) and community views (slice 9) are absent.
- Time zones: a day runs from local midnight to local midnight in the time zone of the goal settings version in
  effect for it; the Home's "today" and greeting use the account's time zone
  ([ADR 0016](../../docs/adr/0016-time-zones.md); slice 4 FR-011, FR-012, FR-029).
- Evaluation states, the 48-hour grace period, corrections and progress (slice 5 FR-009 to FR-016): Upcoming,
  Due, Pending, Met, Failed, Missed; final only when the grace period ends, recorded or not; an unrecorded Reduce
  result is Missed.
- Evaluation on read: every state is computed when it is read, from stored facts and the supplied time; later
  slices "compute from the same function and add caching only with their own decision"
  ([ADR 0018](../../docs/adr/0018-evaluation-on-read-and-demo-history.md)).
- Note editing on pages: a row with a record changes its note only in the RecordDialog; a row without one has
  "Add note" or "Edit note" ([ADR 0019](../../docs/adr/0019-note-editing.md); `specs/006-note-editing` HP-001 to
  HP-004).
- Integration and UI tests: [ADR 0006](../../docs/adr/0006-integration-tests.md). Acceptance tests:
  [ADR 0003](../../docs/adr/0003-acceptance-tests.md).

## Slice 4 and 5 behaviour these tests use

Restated here, unchanged, so that this spec alone gives the rules and texts its acceptance tests depend on:

- **Goals and schedule kinds** (slice 4 FR-001 to FR-013): a goal on fixed days of the week is
  *commitment-based*, each scheduled day on or after its start date being one commitment; a goal of N times per
  day or per week is *period-based*, each day, or each Monday-to-Sunday week from the first one that starts on or
  after the start date, being one evaluation period. A goal without a name of its own is named by its habit's name,
  ", " and its target text ("Reading, 20 min daily", "Sugar, at most 1 sweet a day", "Yoga, 3 times a week").
- **States** (slice 5 FR-010, FR-012, FR-016): at the supplied time each commitment or period is Upcoming (not
  begun), Due (begun, not ended), Pending (ended, grace period of exactly 48 elapsed hours still open, with or
  without records), or, once the grace period has ended, Met or Failed (by its records against the target of the
  settings version of its date) or Missed (no record). Met, Failed and Missed are *final*.
- **Progress** (slice 5 FR-014): a commitment's recorded value against the target (no value while it has no
  record; yes/no: "Yes" counts 1 and "No" 0, against 1); a Build period's number of records that reach the amount
  against N; a Reduce yes/no period's number of "Yes" records against N; a Reduce count or duration period's total
  against the amount. Duration in hours compares minutes with the amount times 60.
- **Corrections** (slice 5 FR-006): any record can be added, changed or deleted at any time; the state is always
  computed from the records as they are now, exactly as if they had been recorded on time.
- **Privacy texts** (slices 1, 4 and 5): an anonymous visitor gets "You are not signed in."; an administrator gets
  "Goals are for registered users."; a goal identifier that is not one of the acting user's goals gives "This
  goal does not exist.".
- **Demo history loading** (slice 5 FR-030): `demo4@example.com` with three goals that start 28 days before the
  run date R, in the installation's default time zone (UTC when none is configured):
  - "Reading, 20 min daily" (Build, duration 20 minutes, all seven days): 20 minutes on each date from R − 28 to
    R − 3, except R − 6: 12 minutes, with the note "Fell asleep early.", and R − 10: no record, with the note
    "Away for the weekend."; R − 2: 25 minutes; R − 1 and R: no record;
  - "Sugar, at most 1 sweet a day" (Reduce, count at most 1 "sweet", once per day): from R − 28 to R − 3 one record,
    0 sweets when k is even and 1 when k is odd, except R − 4: 3 sweets, with the note "Birthday cake at work.",
    and R − 12: no record; R − 2, R − 1 and R: no record;
  - "Yoga, 3 times a week" (Build, yes/no, 3 times per week): the *current week* contains R; week 1 is the week
    before it, week 2 the one before week 1, week 3 the one before week 2. Week 2 has "Yes" on Monday and
    Wednesday only, with the note "Knee was sore."; week 3 has no record; week 1, and a week before week 3 when R
    is a Monday, have "Yes" on Monday, Wednesday and Friday; the current week has no record.

## What acceptance tests can reach

ADR 0003 limits acceptance tests to the Application use cases, the two `HT.Persistence` entry points and the
Domain types they use. As in slices 1 to 5:

- **FR-NNN** are application behaviours, reachable through Application use cases. The figures (consistency,
  streaks, results, trend values) and their texts are returned by use cases, so acceptance tests check them
  without a page. Demo loading, demo history loading and demo report loading (FR-030 to FR-032) are operator
  capabilities that acceptance tests call directly.
- **HP-NNN** are host, page and integration requirements, verified by unit, integration and selective UI tests,
  not by acceptance tests.
- Scenarios marked *(page)* describe page or browser behaviour and are expected to be listed as "Not testable
  through the API" in `acceptance.md`.
- FR numbers are grouped by topic; FR-020 to FR-029 are not used, and demo data starts at FR-030.
- The current time is supplied to every use case (constitution II). A goal's start date cannot be in the past
  (slice 4), so scenarios that need past results create the goal at an earlier supplied time and record and read
  at later ones.

Test data: all acceptance scenarios share one database in one run (ADR 0003). Scenarios use their own accounts
and goals and check lists by inclusion, exclusion and order, never by totals over the whole database. The demo
scenarios start from a reset database.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Consistency of a goal (Priority: P1)

A registered user sees how consistent one goal has been: the share of its final results that were Met, always
with the count it is based on, or "N/A" while nothing is final yet.

**Why this priority**: SPEC §6's central figure; the goal page, the Goals list, the Home and later the community
views (slice 9) all show it.

**Independent Test**: a daily goal with 18 Met, 1 Failed and 1 Missed final days and 2 Pending days reads
"90% (18 of 20)".

**Acceptance Scenarios**:

1. **Given** a goal with 18 Met, 1 Failed and 1 Missed final results, **When** its owner reads its report, **Then**
   its consistency is 90 with 18 successful of 20 counted, and its text is "90% (18 of 20)" (FR-001, FR-002).
2. **Given** the same goal with 2 further Pending and 1 Due result, **When** it is read, **Then** the consistency
   is still "90% (18 of 20)": Pending, Due and Upcoming results are not counted (FR-001).
3. **Given** a goal with no final result yet (not started, or only Due and Pending results), **When** it is read,
   **Then** its consistency text is "N/A" (FR-002).
4. **Given** a goal with 2 Met of 3 final results, **Then** the text is "66% (2 of 3)"; with 1 Met of 3,
   "33% (1 of 3)"; with 199 Met of 200, "99% (199 of 200)"; with all Met, "100% (n of n)" (FR-002 rounds down).
5. **Given** a Reduce goal, **Then** its Met results count as successful and its Failed and Missed ones as
   unsuccessful, exactly as for Build (FR-001).
6. **Given** a per-week goal, **Then** each final week counts once; **given** a commitment-based goal, each final
   commitment counts once; **given** a per-day goal, each final day counts once (FR-001).
7. **Given** two goals on the same habit, **When** their owner reads the reports, **Then** each has its own
   consistency and no figure combines them (FR-010).
8. *(page)* **Given** the goal page, **Then** the GoalStats card shows "Consistency" with the text of FR-002
   (HP-001).

---

### User Story 2 - Strict streaks (Priority: P1)

A registered user sees the current streak and the longest streak of a goal: how many final results in a row were
Met, in the unit of the goal (commitments, days or weeks).

**Why this priority**: SPEC §6's second figure; it rewards continuity without gamification.

**Independent Test**: a daily goal with Met, Met, Failed, Met, Met, Met final days reads "Streak 3 days ·
longest 3".

**Acceptance Scenarios**:

1. **Given** a commitment-based goal whose final results, in date order, are Met, Met, Failed, Met, Met, Met,
   **When** its owner reads its report, **Then** the current streak is 3 and the longest 3, each with the unit
   "commitments" (FR-003, FR-004).
2. **Given** final results Met × 11, Missed, Met × 6, **Then** the current streak is 6 and the longest 11.
3. **Given** a goal whose latest final result is Failed or Missed, **Then** the current streak is 0; the longest
   keeps its value.
4. **Given** a goal whose latest results are Met, Met, then a Pending one and a Due one, **When** it is read,
   **Then** the current streak is 2: Pending and Due results neither extend nor reset it (FR-003). When the
   Pending one has a record that already reaches the target, the current streak is still 2 just before its grace
   period ends and 3 from that instant on, when it is Met (D-1, Clarifications 2026-11-20).
5. **Given** a Pending result that becomes Missed when its grace period ends, **When** the goal is read after
   that instant, **Then** the current streak is 0, with no action by the user or an operator (FR-003, FR-008).
6. **Given** a per-day goal, **Then** the unit is "days" ("1 day" for 1); a per-week goal "weeks" ("1 week"); a
   commitment-based goal "commitments" ("1 commitment"); 0 is "0 days", "0 weeks", "0 commitments" (FR-004).
7. **Given** a goal with no final result, **Then** the current and longest streak are both 0 in its unit.
8. *(page)* **Given** the goal page, **Then** GoalStats shows "Streak, {unit}" and "Longest, {unit}" with their
   values (HP-001); the Home's goal summary shows "Streak 6 commitments · longest 11" (HP-003).

---

### User Story 3 - Corrections change the reports (Priority: P1)

A user corrects an old result; the consistency, the streaks, the recent results and the trend follow on the
next read, as if the corrected value had been recorded on time.

**Why this priority**: constitution II and the slice 5 note: "a correction's effect on reports is verified in
slice 6".

**Independent Test**: let a day become Missed in the middle of a run of Met days, then add a record that reaches
the target, and read the report again.

**Acceptance Scenarios**:

1. **Given** final results Met × 3, Missed, Met × 2 (current streak 2, longest 3, "83% (5 of 6)"), **When** the
   owner adds a record that reaches the target to the Missed day, **Then** the report reads current streak 6,
   longest 6 and "100% (6 of 6)" (FR-008).
2. **Given** a Met commitment, **When** its record is changed to an amount below the target, **Then** it counts
   as Failed in consistency and breaks the streak at its date (FR-008).
3. **Given** a final commitment with its only record deleted, **Then** it counts as Missed (FR-008).
4. **Given** two goals with the same settings, one recorded on time and one corrected later with the same values,
   **When** both reports are read, **Then** every figure is the same (constitution II; FR-008).
5. **Given** a goal whose owner changed time zone, **Then** each result is counted with the settings version of
   its own date, and the order of results is their date order (FR-009).

---

### User Story 4 - Recent results and trend of a goal (Priority: P2)

A user sees the last 30 results of a goal as a strip, and how much was recorded over the last weeks or days,
against the target.

**Why this priority**: SPEC §6 asks for progress and trend views with partial amounts; the strip and trend are
how the user reads them at a glance.

**Independent Test**: a daily duration goal with partial amounts reads its last 14 days of recorded minutes with
the target 20, and its last results with their states.

**Acceptance Scenarios**:

1. **Given** a goal with 40 ended results, **When** its owner reads its report, **Then** its recent results are
   the latest 30 ended commitments or periods (Pending and final), oldest first, each with its date and state;
   Due and Upcoming ones are not among them (FR-005).
2. **Given** the same recent results, **Then** the report gives the number of each state among them (FR-005).
3. **Given** a daily duration goal "Reading, 20 min daily" with records 20, 12, none and 25 minutes on its last
   four days, **When** its trend is read, **Then** it has one value per commitment for the last 14 that have
   begun: 20, 12, no value and 25 for those days, with the target 20 (FR-006).
4. **Given** a per-week goal "Yoga, 3 times a week", **Then** its trend has one value per week for the last 12
   weeks that have begun, each the week's progress (2 for a week with two "Yes"), with the target 3 (FR-006).
5. **Given** a yes/no goal on Monday, Wednesday and Friday, **Then** its trend has one value per Monday-to-Sunday
   week, the number of its commitments recorded "Yes", with the target the number of its commitments in that week
   (FR-006); **given** a yes/no goal on Tuesday and Thursday read on a Monday, its latest bar is the previous
   week's (FR-006, a week's bar begins with its first commitment).
6. **Given** a commitment with a record of 0, **Then** its trend value is 0; **given** one with no record, it has
   no value; **given** a per-day or per-week period with no record, it has no value, also for a Reduce goal
   (FR-006).
7. **Given** a Reduce yes/no goal "avoided on" Monday, Wednesday and Friday with "Yes" on one commitment of a week
   and "No" on the other two, **Then** that week's value is 1 and its target 0 (FR-006); **given** a Build
   duration goal "30 min, 3 times a week" with records of 30, 25 and 40 minutes in a week, that week's value is 2
   and its target 3.
8. *(page)* **Given** the goal page, **Then** the GoalStats card shows the strip with its legend and an accessible
   label that counts each state, and the trend card shows bars with a dashed target line (HP-001, HP-006).

---

### User Story 5 - My goals at a glance (Priority: P1)

On the Goals page, each goal shows its consistency, current streak, its last 15 results and whether something can
be recorded now; the goals that need action come first.

**Why this priority**: the Goals list is the user's overview of every goal; slice 4 left its figures for this
slice.

**Independent Test**: a user with three goals, one with a Pending result, one Due and one not started, reads the
overview in that order with each goal's figures.

**Acceptance Scenarios**:

1. **Given** a user with goals, **When** they read their goals overview, **Then** every goal is listed with its
   consistency (FR-002), its current streak with its unit (FR-003, FR-004), its last 15 ended results (FR-005),
   whether it has a Pending result, and whether something can be recorded now (FR-011).
2. **Given** goals "B" with a Pending result, "A" with a Due one only and "C" with neither, **Then** the order is
   B, A, C; among goals in the same group, name order and then creation order (FR-011).
3. **Given** a goal with a Due commitment or period that has records, **Then** the overview gives its progress
   (FR-011).
4. **Given** the slice 4 goal list (slice 4 FR-014), **When** it is read, **Then** it is unchanged: same entries
   and same order (FR-016).
5. *(page)* **Given** the Goals page, **Then** each item shows the figures with "Consistency" and "Current streak",
   the mini strip, and the primary action "Record" when something can be recorded now (HP-002).

---

### User Story 6 - Today on Home (Priority: P1)

The signed-in Home shows what needs attention now: the counts of Due, Pending and Missed results and of goals,
then a Today list with Pending first, then Due, then this week's Missed results, a summary of each goal and the
latest notes.

**Why this priority**: SPEC §6's dashboard; it is the first page a signed-in user sees.

**Independent Test**: a user with a Pending, a Due and a Missed result reads the dashboard and gets the counts and
the Today list in that order.

**Acceptance Scenarios**:

1. **Given** a user with goals, **When** they read their dashboard at the supplied time, **Then** it gives the
   number of Due results, of Pending results, of Missed results that became final this week, and of their goals
   (FR-012).
2. **Given** the same user, **Then** the Today list has every Pending result first, the one whose grace period
   ends soonest first, then every Due result, then every Missed result that became final this week, newest first;
   entries that tie are in the goal order of FR-011; each with its goal, date, state and progress, and for Pending
   the instant its grace period ends (FR-012).
3. **Given** a per-week goal in its week, **Then** its Due week is in the Today list and is counted among the Due
   results (FR-012).
4. **Given** a Missed result that became final before the Monday of the current week (in the account's time
   zone), **Then** it is not in the Today list and not counted; **given** a commitment on the previous Friday with
   no record, read on Monday at 00:00, **Then** it is Missed this week, and the previous Thursday's is not
   (FR-012; Clarifications 2026-11-20 (clarify)).
5. **Given** a user with goals, **Then** the dashboard lists every goal with its consistency, current streak and
   longest streak with their unit, in the order of FR-011 (FR-012).
6. **Given** notes on several goals, **Then** the dashboard gives the 5 with the latest dates across all goals,
   newest first, each with its goal's name, date and state (FR-012).
7. **Given** a user with no goals, **Then** every count is 0 and every list is empty (FR-013).
8. **Given** a goal on Monday, Wednesday and Friday read on a Tuesday, **Then** the Today list ends with its
   Wednesday commitment as Upcoming, after the Missed results; a goal with a Due result, and one whose next
   commitment or period starts more than 6 days after its current date, get no Upcoming entry; no count is given
   for Upcoming (FR-012; D-2, Clarifications 2026-11-20).
9. *(page)* **Given** the signed-in Home, **Then** it shows the four StatTiles "Due now", "Pending · still can
   record", "Missed this week" and "Active goals", the Today card, the "Active goals" card and the "Recent notes"
   card (HP-003).

---

### User Story 7 - Report privacy (Priority: P1)

Reports are private: only the goal's owner reads them. Another registered user, an administrator and an anonymous
visitor get nothing, and no figure reveals that someone else's goal exists.

**Why this priority**: SPEC §3 and constitution III; personal reports are explicitly owner-only.

**Independent Test**: user A has a goal with results; user B, an administrator and an anonymous visitor each try
to read A's goal report, and each reads their own dashboard and overview, which never contain A's goals.

**Acceptance Scenarios**:

1. **Given** user A's goal, **When** user B reads its report with its identifier, **Then** the result is "This goal
   does not exist." and no figure is returned (FR-014).
2. **Given** user A with goals, **When** user B reads B's dashboard and goals overview, **Then** neither contains
   any of A's goals, results or notes (FR-014).
3. **Given** any goal, **When** an administrator calls any use case of this slice, **Then** the result is "Goals
   are for registered users." and no data is returned (FR-015).
4. **Given** any goal, **When** an anonymous visitor calls any use case of this slice, **Then** the result is "You
   are not signed in." (FR-015).

---

### User Story 8 - Demo reports (Priority: P2)

Reviewers who sign in as a demo account with history find non-trivial consistency and streaks, a Today list
with Due and Pending results, and recent notes, without waiting for real time to pass: `demo4` with 28 days
(slice 5), and `demo5` with 12 weeks, whose trends fill all their bars and whose Today list also has a result
Missed this week. Visitors see an example GoalStats card on the visitor Home.

**Why this priority**: constitution VI and SPEC §10; needed for the hosted demo, not for the core flow.

**Independent Test**: reset the database, run demo loading and demo history loading at a fixed time, and read
`demo4@example.com`'s reports and dashboard at that time.

**Acceptance Scenarios**:

1. **Given** demo loading and demo history loading at a fixed time on run date R, **When** `demo4@example.com`
   reads the report of "Reading, 20 min daily" at that time, **Then** it reads "92% (24 of 26)", current streak 3
   commitments, longest 18 commitments (FR-030).
2. **Given** the same run, **Then** "Sugar, at most 1 sweet a day" reads "92% (24 of 26)", current streak 1 day,
   longest 16 days (FR-030).
3. **Given** the same run on a Wednesday to Sunday, **Then** "Yoga, 3 times a week" reads "33% (1 of 3)", current
   streak 1 week, longest 1 week; on a Tuesday "0% (0 of 2)", 0 weeks and 0 weeks; on a Monday "33% (1 of 3)",
   0 weeks and 1 week (FR-030).
4. **Given** the same run, **When** `demo4@example.com` reads its dashboard, **Then** its Today list has the
   Pending results of Reading and Sugar on R − 2 and R − 1 and their Due results on R, no Missed result and 0
   "Missed this week" (every Missed result of `demo4` became final before the current week), and its recent notes
   include "Fell asleep early." and "Birthday cake at work." (FR-030).
5. **Given** the same run, **Then** demo history loading's results are those of slice 5, unchanged, and the
   slice-4 demo goals of `demo1` to `demo3` read "N/A" and streaks of 0 (FR-031).
6. *(page)* **Given** an anonymous visitor on the visitor Home, **Then** the hero shows a GoalStats card labelled
   "Example goal" (HP-004).
7. **Given** an empty database, a valid demo password and a run at a fixed time, **When** demo loading, demo
   history loading and then demo report loading run, **Then** `demo5@example.com` is created with the three goals
   of FR-032, and demo report loading's results are "created", "demo goals added" and "demo records added" for
   `demo5@example.com`, naming no goal, habit, note, unit or amount (FR-032).
8. **Given** that run on Wednesday 2026-11-18, **When** `demo5@example.com` reads its reports at the run time,
   **Then** "Screen time, at most 120 min a day" reads "93% (77 of 82)", 0 days, longest 34 days; "Strength
   training, 2 times a week" "81% (9 of 11)", 3 weeks, longest 4 weeks; "Walking, 30 min, 3 days a week" "82% (29
   of 35)", 7 commitments, longest 15 commitments; and their trends have 14, 12 and 14 bars (FR-032).
9. **Given** `demo5@example.com` already exists, **When** demo report loading runs again, **Then** its result is
   "already exists", with no goal or record result, and its goals, records and notes are unchanged (FR-032).
10. **Given** no demo password, or one that fails the password rule, **When** demo report loading runs, **Then**
    its result is "no demo password configured" or "demo password does not meet the password rule" and no account
    is created; demo loading's and demo history loading's results never name `demo5@example.com` (FR-031,
    FR-032).
11. **Given** the run of AS-7 on any weekday, with the default time zone UTC, **When** `demo5@example.com` reads
    its dashboard at the run time,
    **Then** "Missed this week" is 1 and the Today list has the Missed result of "Screen time, at most 120 min a
    day" on R − 3 (on a Monday, the previous Friday, whose grace period ends exactly at Monday 00:00) (FR-032).
12. **Given** that run on Wednesday 2026-11-18, **Then** the dashboard counts 3 Due, 3 Pending, 1 Missed this week
    and 3 goals, and its Today list is, in this order: Screen time 16 Nov (Pending), Walking 16 Nov (Pending),
    Screen time 17 Nov (Pending), Screen time 18 Nov (Due), Walking 18 Nov (Due), Strength training 16 – 22 Nov
    (Due), Screen time 15 Nov (Missed), with no Upcoming entry (FR-011, FR-012, FR-032).

---

### Edge Cases

- A goal that has not started: consistency "N/A", streaks 0 in its unit, no recent results, an empty trend.
- A per-week goal started mid-week: the days before its first week belong to no period and appear in no report.
- A per-week goal's Pending week (Monday and Tuesday after it ends) is in the Today list, is excluded from
  consistency, and does not reset the streak.
- A clock change: a day of 23 or 25 hours counts once like any other day; a grace period ending at 23:00 or 01:00
  local time moves a result from Pending to final at that instant (slice 5 FR-016).
- A time zone change after a large move east, where two consecutive commitments overlap: both are counted, in
  date order, each with its own settings version (slice 4 FR-027).
- A goal with more than 30 ended results: consistency and streaks count all of them in the attempt; the strip
  shows only the latest 30 (15 on the Goals page).
- A goal older than the 366 days of the History card: consistency and streaks still count all its results.
- A result corrected from Missed to Met in the middle of a long run joins the runs on each side of it into one
  streak.
- An amount above the target: the trend shows the amount (45 of target 20), and the result counts as Met.
- A duration goal in hours whose bars are amounts (commitment-based, or a Reduce period): trend values and the
  target are in minutes (a target of 1 h is 60, and the page's target line reads "Target 60 min"). A Build
  duration period ("30 min, 3 times a week") has bars that count the records reaching the amount, against N.
- A goal whose catalog habit is Unpublished keeps its reports (slice 4 FR-034).
- Midnight between Sunday and Monday in the account's time zone: Missed results that became final last week leave
  the Today list and the "Missed this week" count.
- An account whose time zone differs from a goal's older settings version: "today" and "this week" on the Home
  use the account's time zone; each result's own instants use its version's time zone.

## Requirements *(mandatory)*

### Functional Requirements (application behaviour, on the AC surface)

**Terms used below**

- *Report range*: the goal's current attempt, from its start date up to the supplied time. Until slice 8 a goal
  has one attempt, started with the goal (slice 4), so the report range is the whole goal. It is never a rolling
  window (Clarifications 2026-11-20 (clarify)).
- *Final result*: a commitment or evaluation period in the report range whose state at the supplied time is Met,
  Failed or Missed (slice 5 FR-010). In this slice every final result is *eligible*; slice 7 excludes Excused and
  Paused ones.
- *Ended result*: a Pending or final one.

**Consistency**

- **FR-001**: The consistency of a goal MUST count its final results in the report range: Met as successful,
  Failed and Missed as unsuccessful, for Build and Reduce alike. Pending, Due and Upcoming results are not counted
  (SPEC §6). Each commitment, each day of a per-day goal and each week of a per-week goal counts once.
- **FR-002**: The consistency MUST be given as the number of successful results, the number of counted results,
  and the whole percentage `successful × 100 / counted` rounded down, with the text "{percentage}% ({successful}
  of {counted})", e.g. "90% (18 of 20)", "66% (2 of 3)"; with no counted result the text is "N/A" and no
  percentage is given. "100%" therefore appears only when every counted result is Met.

**Streaks**

- **FR-003**: Over the final results of the report range in date order (a per-week period by its first date), a
  Met result MUST extend the streak by 1 and a Failed or Missed one MUST reset it to 0. The *current streak* is
  the number of Met results after the latest Failed or Missed one (all of them when there is none); the *longest
  streak* is the largest number of Met results in a row in the report range. Pending, Due and Upcoming results
  neither extend nor reset either streak, also when their records already reach the target: a result extends a
  streak only once it is Met, at the end of its grace period (Clarifications 2026-11-20, D-1; SPEC §11.7 first
  part; slice 5 FR-012; `CONTEXT.md` Pending).
- **FR-004**: Every streak MUST be given with its unit: "commitments" for a commitment-based goal, "days" for a
  per-day goal, "weeks" for a per-week goal; for exactly 1 the singular "commitment", "day" or "week". The text is
  "{n} {unit}", e.g. "6 commitments", "1 day", "0 weeks".

**Recent results and trend**

- **FR-005**: The recent results of a goal MUST be its latest 30 ended results in the report range (15 in the
  goals overview, FR-011), oldest first, each with its date (a per-week period: its first and last date), its
  state and its progress (slice 5 FR-014), together with the number of each state among them.
- **FR-006**: The trend of a goal MUST give one value per bar, each with its date or dates, and the target, for
  the bars that have begun in the report range, oldest first:
  - a per-week goal: one bar per evaluation period, the last 12; the value is the period's progress (slice 5
    FR-014), the target its N (Build, Reduce yes/no) or its amount (Reduce count or duration);
  - a commitment-based yes/no goal: one bar per Monday-to-Sunday week that contains at least one of its
    commitments, the last 12; the value is the number of the week's commitments recorded "Yes", for Build and
    Reduce alike (as slice 5 FR-014 counts "Yes" as 1); the target is the number of its commitments in that week
    (those on or after the start date, Upcoming ones of the current week included) for a Build goal, and 0 for a
    Reduce goal, whose every commitment is to be avoided;
  - a commitment-based count or duration goal: one bar per commitment, the last 14; the value is its recorded
    amount, the target the goal's amount;
  - a per-day goal: one bar per day, the last 14; the value is the day's progress (slice 5 FR-014), the target as
    for a per-week goal.

  A bar begins when its commitment, day or evaluation period begins; a week of a commitment-based yes/no goal
  begins when its first commitment begins, so on a Monday a Tuesday-and-Thursday goal has no bar for the current
  week yet. A bar whose commitment, day or week has no record has no value (not 0), unlike slice 5's progress of
  a period without records, which is 0: an unrecorded Reduce day or week is unknown, not within the limit (slice
  5 FR-013). A bar with at least one record has its value, 0 included. Values above the target are given as
  recorded.

  A bar's value is an *amount* (a count, or minutes) for a commitment-based count or duration goal and for a
  Reduce count or duration period; there durations are in minutes, the values and the target alike (a goal in
  hours: the amount times 60). Everywhere else the value is a *number of records* against a number: "Yes" records
  for yes/no goals, and for a Build count or duration period the records that each reach the amount, against N
  (slice 5 FR-014; "Cycling, 30 min, 3 times a week" has bars such as 2 against the target 3, and a 25-minute ride
  is not counted in them; its minutes are in the History). The value counts the records of every state, Due and
  Pending included: the trend shows recorded progress, not verdicts.

**Goal report**

- **FR-007**: Reading a goal's report MUST return, for one of the acting user's goals at the supplied time, its
  consistency (FR-002), current and longest streak with their unit (FR-003, FR-004), recent results (FR-005) and
  trend (FR-006). It checks in the order: signed in, role, goal (FR-014, FR-015).
- **FR-008**: Every figure of this slice MUST be computed from the records as they are at the supplied time, with
  the states of slice 5, so that a correction, a late record, a deleted record or the end of a grace period
  changes it on the next read, exactly as if the records had been entered on time (constitution II; slice 5
  FR-006). No figure is entered by the user.
- **FR-009**: Each result MUST be counted with the goal settings version in effect for its date (slice 5 FR-015);
  a time zone change never merges, splits or reorders results.
- **FR-010**: Every figure MUST belong to one goal: no use case returns a figure that combines several goals, also
  when they are based on the same habit (SPEC §6).

**Goals overview**

- **FR-011**: Reading the goals overview MUST return every goal of the acting user, each with: its slice 4 list
  entry (name, Build or Reduce, schedule and the start or next information of slice 4 FR-014); its consistency
  (FR-002); its current streak with its unit; its last 15 ended results (FR-005); whether it has a Pending result;
  whether something can be recorded now (slice 5 FR-011 returns at least one entry); and the progress of each Due
  commitment or period. The goals are ordered: those with a Pending result first, then those with a Due result,
  then the others; within each group by name (slice 2 FR-004 comparison) and then in the order they were created.

**Dashboard**

- **FR-012**: Reading the dashboard MUST return, for the acting user at the supplied time, with "this week" being
  the Monday-to-Sunday week that contains the supplied time in the account's time zone:
  - the counts: Due results; Pending results; Missed results whose grace period ended this week; and the number
    of the user's goals;
  - the Today list: every Pending result, the one whose grace period ends soonest first; then every Due result, by
    goal in the order of FR-011; then every Missed result whose grace period ended this week, the latest grace end
    first; Pending or Missed entries whose grace periods end at the same instant are in the goal order of FR-011;
    each with its goal (identifier, name, Build or Reduce), its date (a per-week period: its first and last
    date), its state, its progress, whether it has a record, whether it has a note, and for Pending the instant
    its grace period ends;
    after those, the Upcoming entries (Clarifications 2026-11-20, D-2; SPEC §6): for each goal with no Due
    result, its earliest Upcoming commitment or period, when its first date is at most 6 days after the goal's
    current date; the earliest start first, then in the order of FR-011; each with its goal, its date and the
    state Upcoming. Upcoming entries are not counted;
  - every goal with its consistency (FR-002) and its current and longest streak with their unit, in the order of
    FR-011;
  - the recent notes across all the user's goals: at most 5, those of the commitments or periods with the latest
    first dates, newest first, notes with the same first date in the goal order of FR-011 (the 5 are chosen in
    this order), each with its goal's name, its date (a per-week period: its first and last date),
    the note and the state.
- **FR-013**: A user with no goals MUST get counts of 0 and empty lists, never a refusal.

**Privacy**

- **FR-014**: Every use case of this slice MUST take the acting user and read only that user's own goals, records
  and notes (constitution III). The goal report looks a goal up only among the acting user's goals ("This goal
  does not exist." otherwise, the same as for an identifier that belongs to nothing); the overview and the
  dashboard take no goal identifier and contain only the acting user's goals.
- **FR-015**: Every use case of this slice MUST refuse an anonymous visitor with "You are not signed in." and an
  administrator with "Goals are for registered users.", returning no data, checked in that order before the goal.

**Earlier use cases**

- **FR-016**: The use cases of slices 1 to 5 MUST keep giving their results for every call those slices accept,
  so that their locked acceptance criteria (F001, F002, F004, F005) pass unchanged. The slice 4 goal list keeps
  its entries and name order; the overview (FR-011) adds the figures and the action order.

**Demonstration data**

- **FR-030**: Demo history loading MUST stay as slice 5 FR-030 gives it; its data shows the reports of this
  slice. Read by `demo4@example.com` at the run time on run date R (the figures follow from FR-001 to FR-004 and
  the data restated in "Slice 4 and 5 behaviour these tests use", with Pending results outside every streak,
  D-1):
  - "Reading, 20 min daily": final results R − 28 to R − 3, of which R − 6 Failed and R − 10 Missed: "92% (24 of
    26)", current streak 3 commitments, longest 18 commitments; R − 2 and R − 1 Pending, R Due;
  - "Sugar, at most 1 sweet a day": final results R − 28 to R − 3, of which R − 4 Failed and R − 12 Missed: "92%
    (24 of 26)", current streak 1 day, longest 16 days; R − 2 and R − 1 Pending, R Due;
  - "Yoga, 3 times a week": week 3 Missed, week 2 Failed; week 1 Met from Wednesday of the current week and
    Pending on its Monday and Tuesday; when R is a Monday, the week before week 3 Met. So on Wednesday to Sunday
    "33% (1 of 3)", 1 week, longest 1 week; on Tuesday "0% (0 of 2)", 0 weeks, longest 0 weeks; on Monday "33% (1
    of 3)", 0 weeks, longest 1 week. The current week is Due.
- **FR-031**: Demo loading MUST stay exactly as slices 1, 2 and 4 specify it; the slice-4 demo goals of `demo1` to
  `demo3` start on the run date or later, so at the run time they have no final result: "N/A" and streaks of 0.
- **FR-032**: *Demo report loading*, a new operator capability that runs after demo history loading
  (Clarifications 2026-11-20 (clarify)), MUST take the demo password as demo loading does ("no demo password
  configured", "demo password does not meet the password rule", creating nothing in both cases). With a valid one
  it creates `demo5@example.com`, a registered user with the demo password (result "created"), or leaves an
  existing account of that email unchanged (result "already exists", ignoring case; no goal or record result, and
  none of its private data is read or changed, ADR 0013). The new account has no account name and no custom
  habits, and uses the installation's default time zone. It gets, as a whole with the account, three goals that
  start 84 days before the run date R (defined as in slice 5 FR-030; "R − k" is the date k days before R) and
  their records and notes; each goal is left out when its catalog habit is not Published (looked up by name,
  ignoring case), and its name follows slice 4 FR-010:
  - on catalog habit "Walking": Build, duration 30 minutes, fixed days Monday, Wednesday and Friday ("Walking, 30
    min, 3 days a week"); on each of its commitments R − k with k from 3 to 84 one record: 30 minutes, except k
    from 9 to 15: 35 minutes; k from 20 to 26: 18 minutes; k from 43 to 49: no record, the latest of these
    commitments with the note "On holiday."; no record for k from 0 to 2;
  - on catalog habit "Strength training": Build, yes/no, 2 times per week ("Strength training, 2 times a week").
    The *current week* contains R; week w (w ≥ 1) is the w-th week before it. Every week of the goal before the
    current week has "Yes" on its Monday and Thursday, except week 2: Monday, Wednesday and Thursday; week 4:
    Monday only, with the note "Sore shoulder." on that week; week 7: no record. The current week has no record.
    Weeks 1 to 11 are always weeks of the goal (its first full week starts at most 6 days after its start date),
    and week 12 only when R is a Monday;
  - on catalog habit "Screen time": Reduce, duration at most 120 minutes, once per day ("Screen time, at most 120
    min a day"); on each date R − k with k from 4 to 84 one record: 90 minutes when k is even, 110 when k is odd,
    except k from 30 to 32: 150 minutes, with the note "Long train ride." on R − 31; k = 50: no record; R − 3: no
    record, so that on every weekday a result became Missed this week (its grace period ends at R 00:00, never
    before the Monday of R's week); R − 2: 100 minutes; R − 1 and R: no record.

  Its results for a created account are "created", "demo goals added" and "demo records added" for
  `demo5@example.com`, also when some or all goals are left out; they name the demo email but never a goal,
  habit, note, unit or amount. Read by `demo5@example.com` at the run time:
  - "Screen time, at most 120 min a day" (any weekday): final results R − 84 to R − 3, of which R − 30 to R − 32
    Failed and R − 50 and R − 3 Missed: "93% (77 of 82)", current streak 0 days, longest 34 days (R − 84 to
    R − 51); R − 2 (with 100 minutes) and R − 1 Pending, R Due; on the dashboard, R − 3 is Missed this week
    (US8-AS11). This holds in the default time zone UTC, which has no clock change (the hosted demo uses UTC). In
    a default time zone with a clock change in the 48 hours after R − 2 00:00, R − 3's grace period ends at 23:00
    or 01:00 local time instead: after a change back, a run on a Monday finds it final before the week; after a
    change forward, a run between 00:00 and 01:00 finds it still Pending (slice 5 FR-016);
  - "Strength training, 2 times a week", on a Wednesday to Sunday: weeks 1 to 11 final, week 4 Failed and week 7
    Missed: "81% (9 of 11)", current streak 3 weeks, longest 4 weeks; on a Tuesday week 1 is Pending: "80% (8 of
    10)", 2 weeks, longest 4 weeks; on a Monday week 1 is Pending and the goal also has a week 12 (its start date
    is then a Monday), with "Yes" on Monday and Thursday: "81% (9 of 11)", 2 weeks, longest 5 weeks;
  - "Walking, 30 min, 3 days a week", with R a Wednesday (e.g. 2026-11-18): 35 final commitments, R − 21, R − 23
    and R − 26 Failed, R − 44, R − 47 and R − 49 Missed: "82% (29 of 35)", current streak 7 commitments, longest 15
    commitments; R − 2 Pending, R Due. On other weekdays the figures follow from the same rules.

  Every trend of these goals has its full number of bars: 14 for "Walking" and "Screen time", 12 for "Strength
  training" (11 weeks and the current one).

### Host, page and integration requirements (outside the AC surface)

- **HP-001**: The goal page (`/goals/{id}`, `design/pages/PageGoalDetail`) MUST add, in the left column above the
  History card (ADR 0008):
  - the GoalStats card (`design/components/GoalStats`): the eyebrow "This attempt", the title "Consistency and
    streaks", three stats "Consistency" (FR-002), "Streak, {unit}" and "Longest, {unit}" (FR-003, FR-004, the unit
    in its plural form), then the ConsistencyStrip of the recent results (FR-005) and its legend; the legend and
    cells show only Met, Failed, Missed and Pending (Excused and Paused come in slice 7);
  - the TrendChart card (`design/components/TrendChart`): bars of FR-006 with the dashed target line "Target {n}
    min" when the bars are amounts in minutes (FR-006), in hours too ({n} is FR-006's target in minutes, so the
    page converts nothing), and "Target {n}" otherwise (counts and numbers of records, e.g. "Target 3" for
    "Cycling, 30 min, 3 times a week"), a bar without a value drawn empty, and date labels; the title and sub line
    per goal kind as the design gives them (Design changes before build).

  A goal with no begun entry keeps the slice 5 page without these cards. Pauses and earlier attempts stay absent.
- **HP-002**: The Goals page (`/goals`, `design/pages/PageGoals`) MUST show the full GoalListItem
  (`design/components/GoalListItem`) in the order of FR-011, still without Tabs (Paused and Ended are slices 7 and
  8): the name link, the Build or Reduce Tag, a Pending StatusChip when the goal has a Pending result, the meta
  line of slice 4 HP-003 extended with the progress of a Due entry that has a record ("12 of 20 min today", "1 of
  3 this week"), "Consistency" (FR-002), "Current streak" (FR-004), the mini strip of the last 15 results, and
  one action: "Record" (primary when a Due entry exists, secondary when only Pending ones do) opening the
  RecordDialog of slice 5 HP-002; no action when nothing can be recorded. Under the list the footnote "Each goal
  is counted on its own. Consistency counts only finalized results; pending ones are left out." (slice 7 adds
  "excused and paused").
- **HP-003**: The signed-in Home of a registered user with goals (`design/pages/PageHomeSignedIn`) MUST show,
  under slice 4's page head:
  - four StatTiles (`design/components/StatTile`): "Due now" (it counts Due commitments, days and weeks, whose
    rows read "Due today" or "Due this week", slice 5 HP-004), "Pending · still can record" (`ht-tile--pending`),
    "Missed this week" (`ht-tile--failed`) and "Active goals", with the counts of FR-012; choosing one of the first
    three filters the Today list to that state, choosing it again shows all; "Active goals" links to `/goals`;
  - the Today card with one CommitmentRow (`design/components/CommitmentRow`) per entry of FR-012's Today list: the
    goal's name, a "Reduce" Tag for Reduce goals, the meta line (a Due entry: its schedule without a record, its
    progress with one; a Pending entry: "{date} · {N} h left to record", N rounded up as in slice 5 HP-002; a
    Missed entry: "{date} · no record"; {date} as in the History, "Tue 17 Nov" or "10 – 16 Aug"), the progress bar
    for count and duration goals, the StatusChip, and one action: "Record" for a Due or Pending period, or a commitment without a
    record (opens the RecordDialog fixed to that entry), "Edit" for a commitment with a record (opens
    that record), "Add note" or "Edit note" for a Missed entry (opens the NoteDialog); no "Excuse" (slice 7). An
    Upcoming entry (D-2) is a row with the goal's name, its date as the meta line, the StatusChip "Upcoming" and
    no action. A Today list with no entry shows the sentence "Nothing to record right now.";
  - in the right column the "Active goals" card (`design/components/GoalSummary`): each goal's name linking to its
    page, its consistency, and "Streak {n} {unit} · longest {m}", or with no final result "N/A" and "No finalized
    {unit} yet"; at most 5 goals, then "All goals →" (→ `/goals`);
  - the "Recent notes" card: FR-012's recent notes, each the note in quotes with "{goal name} · {date}" under it;
    absent when there is none.

  A registered user with no goals keeps slice 4's EmptyState; an administrator keeps the page head only.
- **HP-004**: The visitor Home (`design/pages/PageHomeVisitor`) MUST show in the hero the GoalStats card labelled
  "Example goal" with fixed example figures that belong to no account and read no stored data (D-3; constitution
  III, VI): the title "Yoga, 3 days a week", consistency 18 Met of 20 ("90% (18 of 20)"), current streak 6 and
  longest 11 commitments, and a strip of 21 results, oldest first: Met × 11, Missed, Met, Failed, Met × 6,
  Pending, with its legend (Met, Failed, Missed, Pending). The figures and their texts come from the shared
  formatter (HP-005) applied to these fixed results, so the card cannot disagree with the rules of FR-001 to
  FR-005. The page is the same for every visitor and shows no account's data.
- **HP-005**: Consistency, streak and unit texts MUST come from one shared, unit-tested formatter (constitution
  IV), used by every page of this slice and returned by the use cases, so that pages never build them.
- **HP-006**: Every page of this slice MUST meet constitution IV: tokens and `ht-` classes only; the strip and the
  trend with `role="img"` and an `aria-label` that counts each state ("Last 28 commitments: 24 met, 1 failed, 1
  missed, 2 pending") or names the bars and the target ("Minutes per day, last 14 days; target 20"); a state never
  by colour alone (cell shapes, chip words); tiles keyboard-operable with their filter state announced
  (`aria-pressed`).
- **HP-007**: Logs, exception messages and error pages MUST NOT contain notes, recorded values, units, goal names,
  habit names or figures of a user's report (constitution III).
- **HP-008**: The hosted demo's smoke check (`tools/HT.SmokeCheck`) MUST also find, signed in as
  `demo4@example.com`, the word "Consistency" and the unit "commitments" on the page of "Reading, 20 min daily",
  and "Due now" on the Home; and, signed in as `demo5@example.com`, the name of each of its demo goals on its
  Goals page, the unit "weeks" on the page of "Strength training, 2 times a week", and "Missed this week" with a
  row whose StatusChip reads "Missed" on the Home. These are presence checks: the figures themselves are
  checked by the acceptance tests of FR-030 and FR-032 at a fixed run time. Every data parameter of demo
  report loading's port gets its `SmokeCheckCoverage` entry (slice 3 "Coverage rule").
- **HP-009**: Derived figures MUST NOT be cached or stored: every figure of this slice is computed on read from the
  stored records, settings versions and the supplied time (D-4; ARCHITECTURE §9;
  [ADR 0020](../../docs/adr/0020-derived-figures-on-read.md), extending ADR 0018). Within one read, a use case may
  evaluate each goal once and derive all of that read's figures from it; nothing outlives the read.
- **HP-010**: `CONTEXT.md` MUST get the terms of this slice, each with a NOT-boundary (constitution I): Consistency
  (final results only, per goal, never a band), Streak (current and longest, strict, in the goal's unit), Report
  range, Recent results and Trend (recorded progress, not verdicts), Dashboard (the signed-in Home's Today view; in-app only,
  not a reminder), and Demo report loading. The Pending entry gains "and never extends one" (D-1); the Upcoming
  entry stays, and the Home now shows it (D-2); Demo account names `demo5@example.com`, created by demo report
  loading; Demo refresh lists demo report loading among the tasks that run again. These entries were added on
  2026-11-20 in the revision after critic pass 1, together with this spec's decisions (constitution I, "same
  change"), before `/speckit-ac-translate`.
- **HP-011**: The host MUST run demo report loading at every start, after demo history loading, and log its
  results as demo history loading's are logged (slice 5 HP-007): an account line for `demo5@example.com`, then for
  a created account a "Demo goals …" and a "Demo records …" line, each with the demo email and the result text
  only. With no demo password, or one that fails the password rule, it logs the one line "Demo report loading:
  {result}" ("Demo report loading: no demo password configured"), after "Demo history loading: {result}". A fault
  in it is logged without secrets and does not stop the start (ADR 0007). The deployment's success
  check (ADR 0015) also requires its result. The order and the amended "later slices" rule of ADR 0007 and ADR
  0018 are recorded in [ADR 0021](../../docs/adr/0021-demo-report-loading.md). The daily scheduled refresh (slice 5
  HP-009) re-creates `demo5` with the rest of the demo data, so its figures stay as FR-032 gives them.

### Slice 4 and 5 requirements and tests this slice changes

- Slice 4 HP-003 and `design/pages/PageGoals` "Slice 4 state" (reduced items in name order, no figures): amended
  by HP-002; the slice 4 list use case is unchanged (FR-016). Tests: the Goals page's UI tests, and
  `tests/HT.UiTests/Components/Shared/HabitComponentTests.cs`, whose reduced GoalListItem test asserts no
  `ht-strip`, actions, "Record" or "%"; it keeps a reduced item only where slice 6 still renders one, and the full
  item gets its own tests.
- Slice 4 HP-010 and `design/pages/PageHomeSignedIn` "Slice 4 state" (a user with goals sees the page head only
  "until slice 6"): amended by HP-003. Test: the Home's UI tests.
- Slice 5 HP-001 and `design/pages/PageGoalDetail` "Slice 5 state" ("No GoalStats, strip, trend …"): amended by
  HP-001. Test: the goal page's UI tests.
- Slice 2 HP-012 and `design/pages/PageHomeVisitor` "Slice 2 state" ("No GoalStats card yet (it needs demo goals,
  slice 6)"): amended by HP-004. Slice 2 US5-AS8 ("with no Published habit nothing else from the preview is
  shown") is amended: the example card is part of the hero, not of the catalog preview, so it is shown with or
  without Published habits. Test: `tests/HT.UiTests/Components/Pages/HomeVisitorTests.cs`
  (`US5_AS8_HP_012_with_no_Published_habit_nothing_else_from_the_preview_is_shown`), which today asserts that
  "Example goal", `.ht-card`, `.ht-stats` and `.ht-strip` are absent; it changes to assert the absence of the
  catalog preview only, and a new test checks the example card.
- Slice 4 HP-018 and slice 5 HP-008 (smoke check): extended by HP-008. Tests that compare check lines exactly
  change with it: `tests/HT.UiTests/Host/SmokeCheckHostTests.cs` (the list of check names, which gains a
  `demo5@example.com` check, and "PASS demo history demo4@example.com (11 of 11)", whose count grows with
  HP-008's demo4 texts) and `tests/HT.UiTests/SmokeChecks/RunnerBehaviourTests.cs` (exact check lines and their
  positions, such as `lines[7]`). `tests/HT.UiTests/SmokeChecks/CoverageTests.cs` changes too: its `Ports` list
  gains demo report loading's port, so that the coverage rule enumerates that port's parameters, and the exact
  count of `SmokeCheckCoverage` entries (10 since slice 5) grows by that port's entries.
- Slice 5 HP-007 and ADR 0007's startup tasks (administrator creation, demo loading, demo history loading):
  extended by HP-011 with demo report loading as the fourth task. Tests: the host's startup tests
  (`tests/HT.UiTests/Host/StartupLogTests.cs`, `tests/HT.UiTests/Operator/StartupTasksDemoTests.cs`) compare the
  "Demo " lines exactly; they change to expect `demo5@example.com`'s lines after `demo4@example.com`'s, which stay
  as they are, and, with no demo password or a weak one, three lines instead of two: "Demo loading: …", "Demo
  history loading: …" and "Demo report loading: …" (HP-011). The fault test
  `A_fault_in_demo_history_loading_is_logged_as_an_error_after_demo_loadings_lines_and_the_start_goes_on`
  asserts that the history fault is the last "Demo " line; demo report loading runs after it, so the test
  changes to expect `demo5@example.com`'s lines after the fault (the start goes on), and a new test covers a
  fault in demo report loading.
- The locked acceptance criteria of slices 1, 2, 4 and 5 (`tests/HT.AcceptanceTests/Features/F001`, `F002`, `F004`,
  `F005`) are not changed and must keep passing (FR-016, FR-030, FR-031); `demo5` comes from a separate task
  because F005 AC-051 and AC-059 let demo history loading name `demo4` only.

### Design changes before build

Constitution IV and ADR 0008 require `design/` to change first:

- `design/pages/PageGoalDetail`: a slice-6 state with GoalStats and TrendChart above History; the legend without
  Excused and Paused.
- `design/components/GoalStats` and `ConsistencyStrip`: a slice-6 state with only Met, Failed, Missed and Pending
  cells and legend entries.
- `design/components/TrendChart`: the bar kinds of FR-006 with their titles and sub lines (for example "Minutes
  per day · last 14 days", "Times per week · last 12 weeks", "Days done per week · last 12 weeks"), an empty bar
  for no value, no paused bars until slice 7; its rule "Use weeks for weekly and commitment-based goals" becomes
  "weeks for per-week goals and commitment-based yes/no goals; commitments or days otherwise"; its wording
  "recorded amounts per day or week" and "Bars show amounts, not verdicts" becomes recorded progress, an amount or
  a number of records (FR-006), not verdicts; the target line "Target {n} min" or "Target {n}" (HP-001).
- `design/pages/PageGoals` and `components/GoalListItem`: a slice-6 state without Tabs, the meta line of HP-002
  (no "missed Tue"), and the footnote without "excused and paused".
- `design/pages/PageHomeSignedIn`, `components/CommitmentRow` and `StatTile`: a slice-6 state with the rows and
  actions of HP-003: the first tile labelled "Due now" instead of "Due today" (it also counts per-week periods,
  which are "Due this week", constitution I); "Record" or "Edit" instead of "Add time" and "+1", no "Excuse", dates as "Tue 17 Nov", the
  sentence for an empty Today list, the tile filter, Upcoming rows (D-2); "Recent notes" items with "{goal name} ·
  {date}".
- `design/components/StatusChip`: the Upcoming chip reads "Upcoming" (constitution I; `CONTEXT.md` Upcoming is NOT
  Due), replacing "Due Mon"; the date is in the row's meta line.
- `design/components/GoalSummary`: "No finalized {unit} yet" per unit, at most 5 goals, "All goals →".
- `design/pages/PageHomeVisitor` and its `.md`: the example card of HP-004 (fixed example figures, not "a real
  GoalStats card from demo data"), with the strip and legend of slice 6.
- `design/pages.md` and `design/README.md`: the slice-6 structure of these pages.

### Key Entities

- **Consistency**: the share of a goal's final results in its report range that were Met, with the counts it is
  based on; "N/A" when nothing is final.
- **Streak**: the number of Met final results in a row, current (after the latest Failed or Missed) or longest, in
  the goal's unit.
- **Report range**: the goal's current attempt up to the supplied time.
- **Recent results**: the latest 30 (or 15) ended results of a goal with their states.
- **Trend**: recorded progress (an amount, or a number of records) per commitment, day or week against the target; not verdicts.
- **Dashboard**: the counts, Today list, goal summaries and recent notes of one user at a supplied time.

All are derived on read from records, settings versions and the supplied time; none is entered or stored by the
user.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A signed-in user sees what needs recording now and each goal's consistency and streak on the first
  page after sign-in, with no further action.
- **SC-002**: In 100% of the report tests, every consistency is shown with its count or as "N/A" and every streak
  with its unit (constitution IV).
- **SC-003**: The tests cover consistency and streaks just before, at and just after a grace period's end, and at,
  below and above the target, for each schedule kind and for Build and Reduce (constitution II).
- **SC-004**: A report read after a correction has the same figures as one where the same records were entered on
  time, in 100% of the correction tests.
- **SC-005**: Every use case of this slice has negative tests for another registered user, an administrator and
  an anonymous visitor, and none returns another account's goal, result, note or figure (constitution III).
- **SC-006**: On the hosted demo, on any day until 2027-01-31, `demo4@example.com` shows a consistency with its
  count, a non-zero current streak and a longest streak above 10 for at least one goal, and a Today list with Due
  and Pending results, and `demo5@example.com` shows every trend with its full number of bars (14 or 12) and a
  result Missed this week. Measured in two parts: the acceptance tests of FR-030 and FR-032 prove these figures
  for a run at a fixed time on each weekday kind they name, and the daily refresh (slice 5 HP-009) re-creates the
  same data relative to each day; after every deployment the smoke check confirms only that the hosted demo has
  this data and shows its reports (the presence texts of HP-008), not the figures themselves.
- **SC-007**: For a user with three goals, one of them with 366 days of daily results, the goal report, the goals
  overview and the dashboard are each returned in under 2 seconds, measured by an integration test against SQL
  Server (ADR 0006) that creates the goal at a supplied time a year earlier; the hosted demo's goals are at most
  84 days old, so it cannot measure this.
- **SC-008**: Every page of this slice passes the accessibility rules of constitution IV (HP-006).

## Decisions due before acceptance criteria

The roadmap places three decisions in this slice (D-1, D-4 and R3); this spec added D-2 and D-3 in
`/speckit-specify` and D-5 in `/speckit-clarify`. All six were answered by the owner on 2026-11-20 (see
`## Clarifications`), as were the four further clarify answers (rounding, "Missed this week", report range, the
trend of commitment-based count and duration goals).

- **Product row of the roadmap**:
  - **D-1** (FR-003, US2-AS4; SPEC §11.7 first part, `CONTEXT.md` Pending): a Pending result in the current
    streak. Decided: Pending never extends and never resets a streak, also when its records already reach the
    target; Met at the grace end extends it. Rejected: a provisional extension, which could drop again and would
    bring back a "Met before final" that slice 5 D-2 rejected.
- **Decisions of this spec**:
  - **D-2** (FR-012, US6-AS8, HP-003; SPEC §6): upcoming commitments on the dashboard. Decided: "Upcoming" rows at
    the end of the Today list, each goal's earliest Upcoming entry within 6 days when it has no Due one, with no
    tile and no action. Rejected: an extra tile; upcoming only on the Goals page.
  - **D-3** (HP-004; constitution III, VI): the visitor Home's example GoalStats card. Decided: fixed example
    figures belonging to no account. Rejected: a demo account's real goal (private data to anonymous visitors);
    leaving the card out.
- **Technical row of the roadmap**:
  - **D-4** (HP-009; ARCHITECTURE §9, ADR 0018): caching of derived figures. Decided: no caching; every figure is
    computed on read. It spans features (community, slice 9, computes from the same figures), so it is recorded
    in [ADR 0020](../../docs/adr/0020-derived-figures-on-read.md), which extends ADR 0018; the roadmap row links
    it.
- **Roadmap decision R3** ("Reports before excusals and pauses", due before slice 6): decided "Yes" by the owner
  on 2026-11-20; the roadmap's R3 row records it.
- **Decided in `/speckit-clarify`** (2026-11-20):
  - **D-5** (FR-032, HP-011; constitution VI, ADR 0007, 0013, 0018): longer demo history. Decided: a new account
    `demo5@example.com` with 12 weeks of history, created by a fourth startup task, demo report loading. Rejected:
    keeping only `demo4` (short trends); unlocking F005 so that demo history loading creates `demo5`. It amends
    ADR 0007's and ADR 0018's "later slices" rule (extend demo loading or demo history loading), so it is
    recorded in [ADR 0021](../../docs/adr/0021-demo-report-loading.md).

## Assumptions

- Excused and Paused (slice 7), goal changes, ended goals and earlier attempts (slice 8), and community views
  (slice 9) are out of scope; their parts of the pages are absent (ADR 0008). Slice 7 extends FR-001, FR-003 and
  FR-005 with the excluded states.
- The report range (the whole current attempt), the rounding of percentages and "Missed this week" are decided in
  `## Clarifications` (Session 2026-11-20 (clarify)).
- "This week" on the Home is the account's Monday-to-Sunday week, as evaluation periods are.
- The dashboard's goal summaries list every goal; the Home page shows at most 5 of them.
- Demo history loading and demo loading do not change (FR-030, FR-031). Demo report loading (FR-032) adds
  `demo5@example.com` for the longer history that trends and streaks need (constitution VI).
- A local database keeps `demo5`'s history relative to the day it was first created, as `demo4`'s (ADR 0018);
  only the hosted demo is refreshed daily.
- No account is deactivated yet (slice 11); no rule for a deactivated owner's reports.
- Reports are in-app only; nothing is sent by email or any other channel (SPEC §9).
