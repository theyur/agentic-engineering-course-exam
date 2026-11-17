# Feature Specification: Goals

**Feature Branch**: `claude/speckit-specify-slice-4-934803`

**Created**: 2026-11-17

**Status**: Draft

**Input**: User description: "slice 4"

Roadmap outcome (slice 4, `goals`): a user turns a catalog or custom habit into a measurable Build or Reduce
goal and sees it in the goal list and detail (SPEC §5.1). The roadmap adds: this slice fixes the time model the
later slices depend on (schedules, evaluation boundaries and time zones), stores goal settings as rule versions
from the start (constitution II), and decides whether a goal starts as its first attempt.

## Clarifications

### Session 2026-11-17

- Q: Roadmap decision R4: are goal changes (SPEC §5.4: edit settings, end, restart as a new attempt) split out
  of this slice into slice 8? → A: Yes. In this slice a goal cannot be edited, paused, ended, restarted or
  deleted; slice 8 adds changes, ending and restarting (FR-017).
- Q: May an administrator account track goals of its own? → A: No. Goals are for registered users; every goal
  use case refuses an administrator with "Goals are for registered users." and returns no goal of any account
  (FR-021). Administrators keep having no custom habits (slice 2 FR-025), and the navigation gives them no
  Goals link (HP-009).
- Q: Commitment deadlines and time zones (SPEC §11.1 first part): whose time zone decides when a goal's days
  begin and end? → A: The account's own time zone, a profile setting: taken from the browser at registration,
  changeable in Profile. A change applies to each goal from its next commitment or evaluation period; earlier
  ones keep the time zone they had. The owner asked why the zone is kept with the goal; answer: the user only
  chooses it in the profile, and each goal keeps the zone in effect in its settings version so that a change
  never moves past days (constitution II). Accounts without a time zone of their own use the installation's
  default time zone, an operator setting (FR-011, FR-024 to FR-027). Detail:
  [ADR 0016](../../docs/adr/0016-time-zones.md).
- Q: When does a goal with a number of times per day or per week count as met, for Build and for Reduce? → A:
  Count the times. Build is met when the period has at least N records that each reach the amount (yes/no: N
  "done" records); Reduce yes/no is met when the period has at most N "done" records; Reduce count or duration
  limits the period's total to the amount, and its N must be 1 (FR-004, FR-006).
- Q: When a goal counted per week (Monday to Sunday) starts in the middle of a week, which week is its first
  evaluation period? → A: The first full Monday-to-Sunday week that starts on or after the start date; the days
  before it are not evaluated. The user is told, with no extra step: the New goal summary card, the goal list
  and the goal page say when the first week starts, and "Create goal" needs no confirmation (FR-013, FR-014,
  FR-016, HP-002 to HP-004).
- Q: Is a new goal created as its first attempt ("Attempt 1"), or does it get an attempt only when it is
  restarted later? → A: Attempt 1 from the start: creating a goal also starts attempt 1 on its start date, and
  the goal page shows "Attempt 1 · started 14 Jul" (or "· starts 23 Nov"); slice 8's restart adds attempt 2
  onwards (FR-016, FR-018, HP-004).
- Q: When a catalog habit is unpublished, how does that show to the people who have goals on it? → A: The goal
  keeps working; its page shows the Notice "This habit is no longer in the public catalog. Your goal keeps
  working."; the goal list shows nothing extra; no new goal can be created on that habit, by anyone ("This
  habit is not in the catalog.") (FR-016, FR-033, HP-004).
- Q: Should accounts get a personal name in this slice, and what does the top of the signed-in Home show? → A:
  Name, greeting and date. Register and Profile gain an optional "Name"; Home shows "Good morning", "Good
  afternoon" or "Good evening", followed by ", <name>" when a name is set, and today's date; Profile shows the
  name and "member since" (FR-028 to FR-030, HP-010, HP-015, HP-017). The hour boundaries of the greeting and
  the absence of "member since" for accounts created before this slice are this spec's defaults, stated in
  FR-029 and FR-030.

No decision of this slice is open; see
[Decisions due before acceptance criteria](#decisions-due-before-acceptance-criteria).

## Decisions this slice cites

Recorded in earlier slices and not decided again here:

- Use cases take the acting user as an explicit parameter and return named outcomes that carry the literal
  text this spec gives; expected refusals are outcomes, not errors
  ([ADR 0005](../../docs/adr/0005-use-case-convention.md)). An anonymous visitor who calls a use case for
  signed-in users gets "You are not signed in." (slice 1, FR-011).
- The administrator role grants no access to private data (slice 1, FR-015; SPEC §3, §8).
- Partly built pages show only the sections and links whose slice has shipped; sentences keep the wording of
  the previews ([ADR 0008](../../docs/adr/0008-partly-built-pages.md)).
- A suggested starting goal has the fields of the New goal form: Build or Reduce, the measurement, the amount
  and unit for count and duration, and the schedule (fixed days, or N times per day or per week), with the
  ranges and messages of slice 2 FR-007 and the text and kind of slice 2 FR-017
  ([ADR 0011](../../docs/adr/0011-suggested-starting-goals.md)). This slice keeps that vocabulary, so no
  stored suggestion needs a migration.
- Editing or unpublishing a catalog habit never changes, ends or fails a goal based on it; goals show the
  habit's current name; unpublishing a habit that goals use asks the administrator to confirm, showing only
  the number of goals ([ADR 0012](../../docs/adr/0012-goals-on-changed-catalog-habit.md); slice 2 FR-015).
- One identifier type names catalog habits and custom habits; each use case looks an identifier up only among
  its own kind, so a wrong kind gives the same result as an unknown identifier
  ([ADR 0014](../../docs/adr/0014-one-habit-identifier.md)).
- Demo loading writes private demo data only for the demo accounts it creates in the same run, and reads no
  private data ([ADR 0013](../../docs/adr/0013-private-demo-data.md); [ADR 0007](../../docs/adr/0007-startup-operator-tasks.md)).
- Integration and UI tests: [ADR 0006](../../docs/adr/0006-integration-tests.md). Acceptance tests:
  [ADR 0003](../../docs/adr/0003-acceptance-tests.md).
- Administrators have no custom habits (slice 2 FR-025); slice 2 recorded that slice 4 reopens this only if
  administrators get goals.
- No time-dependent text until this slice decides time zones (slice 1, Clarifications). This slice is the
  first to read the current time; it enters as a supplied value (constitution II).

## Slice 1 and slice 2 behaviour these tests use

Restated here, unchanged, so that this spec alone gives the rules and texts its acceptance tests depend on:

- **Accounts** (slice 1 FR-001 to FR-007): an anonymous visitor registers with an email and a password of at
  least 8 characters with a digit; a new account is a registered user; signing in makes the account, with its
  role, the acting user. Administrator creation (slice 1 FR-014) creates an administrator account.
- **Catalog habits** (slice 2 FR-001, FR-008 to FR-012): an administrator creates a catalog habit (Draft),
  publishes and unpublishes it; only Published habits are in the catalog; reading the details of anything else
  gives "This habit is not in the catalog.".
- **Custom habits** (slice 2 FR-020 to FR-025): a registered user creates, lists, reads, edits and deletes
  their own custom habits; an identifier that is not one of the acting user's custom habits gives "This custom
  habit does not exist."; an administrator gets "Custom habits are for registered users.".
- **Suggested starting goal fields and text** (slice 2 FR-007, FR-017): as cited above. FR-017's text is
  what this spec calls the *target text*, for example "15 min, 5 days a week", "at most 1 sweet a day" or
  "3 times a week".
- **Demo loading** (slice 1 FR-016, slice 2 FR-030, FR-031): creates `demo1@example.com` to
  `demo3@example.com` with the demo password (account result "created" or "already exists"), the seed
  catalog into an empty catalog, and each created account's demo custom habits. This slice adds demo goals
  (FR-040).

## What acceptance tests can reach

ADR 0003 limits acceptance tests to the Application use cases, the two `HT.Persistence` entry points and the
Domain types they use. As in slices 1 and 2:

- **FR-NNN** are application behaviours, reachable through Application use cases, so acceptance tests can
  verify them. Demo loading (FR-040) is an operator capability that acceptance tests call directly.
- **HP-NNN** are host, page and integration requirements, verified by unit, integration and selective UI
  tests, not by acceptance tests.
- Scenarios marked *(page)* describe page or browser behaviour and are expected to be listed as "Not testable
  through the API" in `acceptance.md`.
- The current time is supplied to the use cases that depend on it (constitution II), so acceptance tests fix
  "now" and the time zone instead of waiting for real time.

Test data: all acceptance scenarios share one database in one run (ADR 0003). Goals are never deleted in this
slice. Scenarios therefore use their own accounts, habit names and goal names, and check goal lists by
inclusion, exclusion and relative order, never by totals over the whole database. The demo scenarios start
from a reset database, as slice 1's and slice 2's do.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create a goal from a catalog habit (Priority: P1)

A registered user finds a catalog habit, starts from one of its suggested starting goals or from scratch,
adjusts Build or Reduce, the measurement, the target and the schedule, chooses a start date and optionally a
name, and creates the goal. A summary in plain words shows what the goal means before it is created.

**Why this priority**: it is the outcome of the slice; nothing in slices 5 to 9 works without a goal.

**Independent Test**: a registered user creates a goal from "Reading" with its marked suggested starting goal,
then reads the goal back with the same settings.

**Acceptance Scenarios**:

1. **Given** a Published catalog habit "Reading" whose marked suggested starting goal is "15 min, 5 days a
   week" (Duration, fixed days Mon–Fri, Build), **When** a registered user starts a new goal from that habit
   without choosing a suggested starting goal, **Then** the starting values are: habit "Reading" (catalog),
   Build, Duration, 15 minutes, fixed days Monday to Friday, start date today, no name, and the catalog
   suggestion text "15 min, 5 days a week".
2. **Given** the same habit, whose second suggested starting goal is "20 pages, 3 times a week", **When** the
   user starts a new goal from suggested starting goal 2, **Then** the starting values are Build, Count, 20
   "pages", 3 times per week, start date today, and the catalog suggestion text "20 pages, 3 times a week".
3. **Given** a Published catalog habit with no suggested starting goal and intent Reduce, **When** the user
   starts a new goal from it, **Then** only the habit, Reduce and today's start date are filled in; the
   measurement, target and schedule are empty and there is no catalog suggestion text.
4. **Given** a Published catalog habit with 3 suggested starting goals, **When** the user starts a new goal
   from suggested starting goal 4, **Then** the result is "This suggested starting goal does not exist.".
5. **Given** starting values, **When** the user creates the goal with them unchanged, **Then** the goal exists,
   is owned by the user, is based on that catalog habit, has exactly those settings, and is in the user's goal
   list.
6. **Given** the starting values of AS1, **When** the user changes them to Reduce, Count, at most 2 "cups",
   1 time per day, and creates the goal, **Then** the goal has those settings and its target text is "at most
   2 cups a day".
7. **Given** any field values, **When** the user creates a goal, **Then** each field refusal of FR-006 is given
   with its message, one at a time in FR-006 order, and no goal is created (one scenario per message).
8. **Given** a Draft catalog habit, an Unpublished catalog habit, a custom habit's identifier given as a
   catalog habit, or an identifier that belongs to nothing, **When** a registered user starts or creates a goal
   on it as a catalog habit, **Then** the result is "This habit is not in the catalog." and no goal is created.
9. **Given** a goal created without a name on catalog habit "Reading" with the settings of AS1, **When** the
   user reads it, **Then** its name is "Reading, 15 min, 5 days a week"; **given** a name "Morning pages"
   instead, its name is "Morning pages".
10. **Given** the user already has a goal on "Reading", **When** they create a second goal on "Reading" with
    the same or other settings, **Then** both goals exist independently (SPEC §5.1).
11. *(page)* **Given** the New goal page, **When** the user changes any field, **Then** the summary card
    rewrites the goal as one sentence and lists how it is evaluated (HP-002), and Reduce switches the target
    word to "At most".

---

### User Story 2 - See my goals (Priority: P1)

A registered user opens Goals and sees every goal they created, each with its name, Build or Reduce, schedule
and when its next commitment is or when it starts. Opening a goal shows its settings, its attempt line and how
it is evaluated.

**Why this priority**: the roadmap outcome includes seeing the goal in the list and detail; it makes the goal
demonstrable before recording exists.

**Independent Test**: a user with two goals lists them in name order and opens one.

**Acceptance Scenarios**:

1. **Given** a user with goals "Yoga, 3 times a week" and "Reading, 15 min, 5 days a week", **When** they list
   their goals, **Then** both are returned in name order (slice 2 FR-004 comparison), each with its name,
   habit name and kind (catalog or custom), Build or Reduce, measurement, target text, schedule kind
   (Commitment-based or Period-based) and its next-commitment or start information (FR-014).
2. **Given** a goal on fixed days Monday, Wednesday and Friday that started earlier, **When** the user lists
   goals on a Wednesday, **Then** its next commitment is that Wednesday (today); on a Thursday, it is the next
   Friday.
3. **Given** a goal whose start date is after today, **When** the user lists goals, **Then** the goal says it
   starts on its start date and gives no next commitment.
4. **Given** a goal, **When** its owner reads it, **Then** the result has its name, the habit's current name,
   kind and category, Build or Reduce, measurement, amount and unit, schedule, schedule kind, start date, the
   date from which its settings apply, its time zone, and its attempt (FR-016).
5. **Given** a user with no goals, **When** they list their goals, **Then** the list is empty.
6. *(page)* **Given** the Goals page, **When** the user has goals, **Then** each GoalListItem shows the name
   (linking to the goal page), the intent Tag and the meta line ("Mon · Wed · Fri · next today",
   "Flexible · 3 times a week", "starts Mon 23 Nov"); with none, the EmptyState "No goals yet" (HP-003).
7. *(page)* **Given** a goal page, **Then** it shows "← Goals", the name, the tags (category, Build or Reduce,
   Commitment-based or Period-based), the attempt line and the Settings card (HP-004).

---

### User Story 3 - Goal privacy (Priority: P1)

Goals are private: only their owner can list, read or use them. Another registered user, an administrator and
an anonymous visitor get nothing, and an error never reveals that someone else's goal exists.

**Why this priority**: SPEC §3 and constitution III; it is a precondition of every goal use case.

**Independent Test**: user A creates a goal; user B, an administrator and an anonymous visitor each try to read
it, list it and read its evaluation calendar.

**Acceptance Scenarios**:

1. **Given** user A's goal, **When** registered user B reads it or reads its evaluation calendar by its
   identifier, **Then** the result is "This goal does not exist.", the same as for an identifier that belongs
   to nothing.
2. **Given** user A's goals, **When** user B lists their own goals, **Then** none of A's goals is included.
3. **Given** user A's goal, **When** an administrator calls any goal use case, **Then** the result is the
   administrator refusal of FR-021 and no goal data is returned.
4. **Given** any goal, **When** an anonymous visitor calls any goal use case, **Then** the result is "You are
   not signed in.".
5. **Given** user B's custom habit, **When** user A starts or creates a goal on it, **Then** the result is
   "This custom habit does not exist." and no goal is created.
6. **Given** user A's goals on catalog habit "Reading", **When** user B browses the catalog or reads the
   details of "Reading", **Then** B's own goal count for "Reading" is 0 and nothing about A's goals is
   returned (FR-031).

---

### User Story 4 - Commitments and evaluation periods (Priority: P1)

The goal's schedule decides what will be evaluated and when each piece ends: each scheduled day of a
fixed-days goal is one commitment; a goal with N times per day or per week is evaluated per day or per week.
Every boundary falls at midnight in the goal's time zone.

**Why this priority**: the roadmap puts the time model in this slice; slices 5 to 9 build on these
boundaries.

**Independent Test**: for a fixed "now" and time zone, read the evaluation calendar of one goal of each
schedule kind over two weeks, including a day on which the clocks change.

**Acceptance Scenarios**:

1. **Given** a goal on fixed days Monday, Wednesday and Friday starting on a Wednesday, **When** its owner
   reads its evaluation calendar for the 14 days from that Monday, **Then** it lists one commitment per
   Monday, Wednesday and Friday on or after the start date (5 commitments, the first on the start date), each
   starting at midnight of its day and ending at the next midnight in the goal's time zone.
2. **Given** a goal of N times per day starting on a given day, **When** its owner reads the calendar for 7
   days from that day, **Then** it lists 7 evaluation periods, one per day.
3. **Given** a goal of N times per week, **When** its owner reads the calendar over three weeks, **Then** it
   lists one evaluation period per week, Monday to Sunday; for a goal that starts on a Wednesday, the first one
   is the following Monday to Sunday, and the Wednesday to Sunday before it belong to no period.
8. **Given** a per-week goal whose start date is a Wednesday, **When** its owner lists or reads it before that
   week's end, **Then** it gives the date its first week starts (the following Monday); **given** a start date
   on a Monday, the first week starts on the start date.
4. **Given** a goal and a range that includes the day on which the goal's time zone changes its clocks,
   **When** the calendar is read, **Then** that day's commitment or period still starts and ends at local
   midnight, so it lasts 23 or 25 hours.
5. **Given** a range that ends before the goal's start date, **When** the calendar is read, **Then** it is
   empty.
6. **Given** a range whose last date is before its first date, or longer than 366 days, **When** the calendar
   is read, **Then** the result is "Choose a range of 1 to 366 days.".
7. **Given** the supplied current time is 23:30 on a Monday in the user's time zone, **When** the user creates
   a goal starting "today", **Then** its start date is that Monday; at 00:30 the next day it is Tuesday.

---

### User Story 5 - Goals on custom habits (Priority: P2)

A registered user creates a goal on one of their own custom habits, including straight from the custom habit
form. A custom habit that goals use cannot be deleted.

**Why this priority**: SPEC §5.1 allows goals on custom habits; the catalog is the main path.

**Independent Test**: a user creates custom habit "Cold shower", creates a yes/no daily goal on it, then tries
to delete the habit.

**Acceptance Scenarios**:

1. **Given** the user's custom habit "Cold shower" with intent Build, **When** they start a new goal from it,
   **Then** the starting values are the habit (custom), Build and today's start date, and nothing else.
2. **Given** those values completed as Yes/no, fixed days on all seven days, **When** the user creates the
   goal, **Then** it is based on the custom habit and its name is "Cold shower, daily".
3. **Given** a custom habit that at least one of the user's goals uses, **When** the user deletes it, **Then**
   the result is "Goals use this custom habit, so it cannot be deleted." and nothing changes.
4. **Given** a custom habit that no goal uses, **When** the user deletes it, **Then** it is deleted (slice 2
   behaviour, unchanged).
5. **Given** a goal on custom habit "Cold shower", **When** the user renames the habit to "Cold water", **Then**
   the goal, if it has no name of its own, is named "Cold water, daily", and its settings are unchanged; a
   change of the habit's usual intent does not change the goal's Build or Reduce.
6. *(page)* **Given** the custom habit form, **When** the user chooses "Save and create goal", **Then** the
   habit is saved and the New goal page opens with that habit (HP-007).

---

### User Story 6 - My goals in the catalog (Priority: P2)

In the catalog, a registered user sees which habits they already track and how many goals they have on each,
can show only those habits, and on a habit's details sees their goals on it and can start from any suggested
starting goal.

**Why this priority**: the catalog is the entry point to goals; the design shows these parts on the pages
slice 2 built.

**Independent Test**: a user with two goals on "Reading" browses the catalog with and without "Only habits I
track".

**Acceptance Scenarios**:

1. **Given** a user with 2 goals on "Reading" and none on "Yoga", **When** they browse the catalog, **Then**
   "Reading" carries their goal count 2, "Yoga" 0, and the result says how many habits they track.
2. **Given** the same user, **When** they browse with "Only habits I track", **Then** only the Published
   catalog habits with at least one of their goals are returned, with the other filters still applied.
3. **Given** the same user, **When** they read the details of "Reading", **Then** the result includes their
   goal count 2 and their goals on it (identifier and name), in name order.
4. **Given** an anonymous visitor or an administrator, **When** they browse or read details, **Then** the
   result is the slice 2 result with no goal count and no goals; "Only habits I track" is refused for them
   with "You are not signed in." (visitor) or the FR-021 refusal (administrator).
5. *(page)* **Given** the signed-in catalog, **Then** a tracked habit's card shows "N goals" and "Add another
   goal", the others "Use this habit"; the habit details show "Use this habit", "You track this · N goals",
   "Start with this" on each suggested starting goal and "Your goals on this habit" (HP-005, HP-006).

---

### User Story 7 - Unpublishing a habit that goals use (Priority: P2)

An administrator who unpublishes a catalog habit that goals use is first told how many goals use it, and
confirms. The goals keep working for their owners.

**Why this priority**: slice 2 FR-015 promised it to this slice (ADR 0012).

**Independent Test**: two users each create a goal on a habit; the administrator unpublishes it, first without
and then with confirmation.

**Acceptance Scenarios**:

1. **Given** a Published catalog habit that 2 goals use, **When** an administrator unpublishes it without
   confirming, **Then** the result is "confirmation needed" with the count 2 and the text "2 goals use this
   habit.", and the habit stays Published.
2. **Given** the same habit, **When** the administrator unpublishes it with confirmation, **Then** it is
   Unpublished.
3. **Given** a Published habit that 1 goal uses, **Then** the text is "1 goal uses this habit.".
4. **Given** a Published habit that no goal uses, **When** it is unpublished without confirmation, **Then** it
   is Unpublished at once (slice 2 behaviour).
5. **Given** the "confirmation needed" result, **Then** it carries the count and its text only: no owner,
   email, goal name, goal identifier or setting.
6. **Given** a goal on a habit that became Unpublished, **When** its owner lists or reads it, **Then** it is
   returned with the habit's current name and settings unchanged, and reading it says the habit is no longer
   in the catalog; **when** the owner or anyone else starts or creates a new goal on that habit, **then** the
   result is "This habit is not in the catalog.".
8. *(page)* **Given** that goal's page, **Then** it shows the Notice "This habit is no longer in the public
   catalog. Your goal keeps working."; the goal list shows nothing extra (HP-004).
7. **Given** a goal on a catalog habit, **When** an administrator renames the habit, **Then** the goal's owner
   sees the new name in the goal (and in its derived name, FR-010).

---

### User Story 8 - Goals in the navigation and on Home (Priority: P3)

*(page)* Registered users reach Goals and "+ New goal" from the navigation. "+ New goal" opens the New goal
page with no habit chosen yet, and the user picks a habit there. Home no longer says that goals come later.

**Why this priority**: navigation only; the pages themselves are covered by US1 and US2.

**Independent Test**: UI tests of TopNav, MobileNav and Home for each audience.

**Acceptance Scenarios**:

1. *(page)* **Given** a registered user, **Then** TopNav shows "Goals" and "+ New goal", and MobileNav lists
   Goals (HP-009).
2. *(page)* **Given** an administrator or an anonymous visitor, **Then** neither link is shown (FR-021).
3. *(page)* **Given** "+ New goal", **When** the page opens, **Then** the Habit section says to choose a habit
   and offers the habit chooser (HP-001).
4. **Given** a registered user, **When** they start a new goal without a habit, **Then** the starting values
   have no habit and today's start date; creating with no habit gives "Choose a habit.".
5. *(page)* **Given** a registered user with no goals, **Then** Home shows the slice-4 EmptyState (HP-010).

---

### User Story 9 - Demo goals (Priority: P3)

Reviewers who sign in as a demo account find goals already there, covering both schedule kinds, both Build and
Reduce, catalog and custom habits, and a goal that has not started yet.

**Why this priority**: constitution VI; needed for the hosted demo but not for the core flow.

**Independent Test**: reset the database, run demo loading with a valid demo password, sign in as each demo
account and list goals.

**Acceptance Scenarios**:

1. **Given** an empty database and a valid demo password, **When** demo loading runs, **Then** each created
   demo account has the goals of FR-040, and the goal result for each is "demo goals added".
2. **Given** demo accounts that already exist, **When** demo loading runs again, **Then** no goal result is
   given for them and their goals are unchanged.
3. **Given** a catalog in which a catalog habit named in FR-040 is not Published when the account is created,
   **When** demo loading runs, **Then** that one demo goal is left out and the others are created.
4. **Given** demo loading's results and log, **Then** they name the demo email but never a goal name, habit
   name or setting.

### User Story 10 - My time zone (Priority: P2)

Each account has its own time zone, taken from the browser at registration. A user who moves changes it in
Profile; their goals follow from their next day or week, and days already begun or past keep the time zone
they had.

**Why this priority**: it decides every goal's days (FR-011); most users never change it.

**Independent Test**: register with "Europe/Kyiv", create goals of each schedule kind, change the time zone to
"America/New_York" on a Wednesday, and read the evaluation calendars across the change.

**Acceptance Scenarios**:

1. **Given** registration with the browser time zone "Europe/Kyiv", **When** the user reads their time zone,
   **Then** it is "Europe/Kyiv".
2. **Given** registration with no browser time zone, or with "Mars/Olympus", **When** the user reads their time
   zone, **Then** it is the installation's default.
3. **Given** a user in "Europe/Kyiv", **When** they create a goal, **Then** its days run from midnight to
   midnight in "Europe/Kyiv".
4. **Given** that user changes their time zone to "America/New_York" on Wednesday 18 November (today in
   Kyiv), **When** the calendar of their Mon/Wed/Fri goal is read, **Then** Wednesday 18 November and earlier
   commitments keep Kyiv midnights and Friday 20 November onwards uses New York midnights.
5. **Given** the same change, **Then** their per-day goal uses New York from Thursday 19 November, their
   per-week goal from Monday 23 November, and a goal starting on 1 December from its start date.
6. **Given** the same change, **When** the user reads a goal, **Then** all its settings except the time zone are
   unchanged.
7. **Given** a value that is not a named time zone, **When** the user changes their time zone to it, **Then**
   the result is "Choose a time zone from the list." and nothing changes.
8. **Given** an administrator, **When** they read or change their own time zone, **Then** it works as for a
   registered user; **given** an anonymous visitor, the result is "You are not signed in.".
9. **Given** user A's time zone, **Then** no use case gives it to user B or to an administrator.

---

### User Story 11 - My name and the Home greeting (Priority: P3)

An account can have a private name. Home greets the user by the time of day in their own time zone and shows
today's date; Profile shows the name and since when they are a member.

**Why this priority**: presentation only; it uses the time zone of US10.

**Independent Test**: register with and without a name, then read the Home heading at fixed times on both
sides of each greeting boundary.

**Acceptance Scenarios**:

1. **Given** registration with the name "  Yuri  ", **When** the user reads their profile, **Then** the name is
   "Yuri"; **given** no name or only spaces, the account has no name.
2. **Given** a name of 61 characters, **When** the user registers or changes their name, **Then** the result
   is "The name can have at most 60 characters." and nothing is created or changed; 60 characters are accepted.
3. **Given** a user named "Yuri" in "Europe/Kyiv", **When** they read the Home heading at 04:59, 05:00,
   11:59, 12:00, 17:59 and 18:00 Kyiv time, **Then** the greetings are "Good evening, Yuri", "Good morning,
   Yuri", "Good morning, Yuri", "Good afternoon, Yuri", "Good afternoon, Yuri" and "Good evening, Yuri".
4. **Given** a user with no name, **When** they read the Home heading in the evening, **Then** the greeting is
   "Good evening".
5. **Given** the supplied time 23:30 UTC on Monday 16 November and a user in "Europe/Kyiv", **When** they read
   the Home heading, **Then** the date is "Tuesday, 17 November" and the greeting is "Good evening" (01:30 in
   Kyiv falls between 18:00 and 04:59).
6. **Given** an account registered on 17 November 2026 in "Europe/Kyiv", **When** its owner reads the
   profile, **Then** it gives "member since November 2026"; **given** an account created before this slice,
   no "member since".
7. **Given** user A's name, **Then** no use case gives it to user B or to an administrator; an administrator
   can set and read their own name.
8. **Given** an anonymous visitor, **When** they read the Home heading or change a name, **Then** the result is
   "You are not signed in.".

---

### Edge Cases

- A time zone change on a day the user already started a goal: the goal's first version keeps the old zone
  until the change applies (FR-026).
- Two time zone changes before the first one applies (for example on Tuesday and again on Wednesday of the
  same week for a per-week goal): the later change replaces the earlier pending one; only one version starts on
  that date.
- A goal on fixed days whose start date is not a scheduled day: its first commitment is the first scheduled
  day after the start date.
- A goal created at 23:59 local time: "today" is decided in the user's time zone at the supplied time, never by
  the server's clock or zone (constitution II).
- A day on which the clocks change: that day's commitment or period lasts 23 or 25 hours (US4-AS4).
  How the 48-hour grace period behaves around it is slice 5's decision (SPEC §11.1, second part).
- A weekly goal starting mid-week: its first evaluation period is the next full Monday-to-Sunday week; the days
  between the start date and that Monday are not evaluated, and the user is told before creating (HP-002).
- A Reduce goal with amount 0 ("at most 0 cigarettes a day") is valid (slice 2 FR-007 range 0 to 999).
- A yes/no goal of N times per day with N greater than 1 is valid ("Take vitamins, 2 times a day").
- A suggested starting goal that the administrator removes or changes after the user opened the New goal page:
  the goal is created from the values the user submits; it keeps no link to the suggestion.
- The habit's catalog suggestion text in the summary is for orientation only; the goal never follows later
  changes to the suggestion.
- Two goals with the same name for one user are allowed; the list shows both, in name order and then by
  creation order.
- A custom habit deleted between starting and creating a goal: creating gives "This custom habit does not
  exist.".
- A catalog habit unpublished between starting and creating a goal: creating gives "This habit is not in the
  catalog.".
- An administrator who unpublishes after the confirmation was shown, while another goal was created in
  between: the confirmed unpublish acts; the count is informative, not a lock.

## Requirements *(mandatory)*

### Functional Requirements (application behaviour, on the AC surface)

**Goal settings**

- **FR-001**: A registered user MUST be able to create a personal goal based on exactly one habit: a Published
  catalog habit or one of their own custom habits (SPEC §5.1). The goal records which habit and which kind
  (ADR 0014). One habit may support any number of independent goals.
- **FR-002**: A goal's settings MUST be: Build or Reduce; the measurement (yes/no, count or duration); for
  count and duration the amount and the unit; the schedule (fixed days of the week, or N times per day or per
  week); the start date; an optional name; and the goal's time zone (FR-011). The fields, ranges and units are
  those of slice 2 FR-007. A Build target means at least the amount, a Reduce target at most the amount (SPEC
  §5.2).
- **FR-003**: A goal on fixed days MUST be *commitment-based*: each scheduled day on or after the start date
  is one commitment. A goal of N times per day or per week MUST be *period-based*: each day or each week is one
  evaluation period. Each goal uses one approach at a time (SPEC §5.1).
- **FR-004**: What a target means MUST be stated per schedule kind, so that slice 5 evaluates against it:
  - commitment-based: a commitment is met when that day's recorded result reaches the target: Build at least
    the amount (yes/no: done), Reduce at most the amount (yes/no: not done);
  - period-based (Clarifications 2026-11-17): Build is met when the period has at least N records that each
    reach the amount (yes/no: N "done" records); Reduce yes/no is met when the period has at most N "done"
    records; Reduce count or duration is met when the period's total is at most the amount, and its N must be
    1 (FR-006).

  How a record is evaluated, Pending and Missed, and an unrecorded Reduce period (SPEC §11.4) are slice 5's.
- **FR-005**: The goal's settings MUST be kept as its first settings version, with the date from which it
  applies (the start date), so that later changes (slice 8) add versions instead of overwriting, and every
  evaluation can be recomputed against the version in effect for its date (constitution II). In this slice
  the only later version is the one a change of the owner's time zone adds (FR-026); it changes nothing but
  the time zone.
- **FR-006**: Creating a goal MUST give one result, checking in this order: no habit "Choose a habit."; then
  the habit lookup (FR-008); then the fields with the messages of slice 2 FR-007, in its order, with "goal" in
  place of "suggested starting goal" ("A yes/no goal has no amount or unit."); then a Reduce count or duration
  goal with N times other than 1 "A Reduce goal with an amount has 1 time per period." (FR-004); then no start date "Choose a start date."; a start date before today (FR-012) "The
  start date cannot be in the past."; a start date more than 365 days after today "Choose a
  start date within a year from today."; then a name longer than 60 characters after trimming "The name can
  have at most 60 characters.". Values that do not belong to the chosen measurement or schedule are not
  refused and not stored (slice 2, owner answer 2026-11-14). A refusal carries its message and the field it
  is about.
- **FR-007**: Starting a new goal MUST return starting values for the New goal form without creating
  anything:
  - from a catalog habit and a suggested starting goal position (counted from 1): that suggestion's fields;
  - from a catalog habit without a position: its marked suggested starting goal's fields, or, with none, only
    Build or Reduce from the habit's intent;
  - from a custom habit: only Build or Reduce from the habit's intent;
  - without a habit: nothing but the start date.

  In every case the start date is today (FR-012), there is no name, and, when a suggestion was
  used, the catalog suggestion text is its target text (slice 2 FR-017). A position that the habit does not
  have gives "This suggested starting goal does not exist.". Every suggested starting goal of the seed catalog
  gives starting values that can be created unchanged.
- **FR-008**: A goal's habit MUST be looked up only among its kind (ADR 0014): as a catalog habit only among
  Published catalog habits, otherwise "This habit is not in the catalog."; as a custom habit only among the
  acting user's custom habits, otherwise "This custom habit does not exist.". Starting and creating use the
  same lookup.
- **FR-009**: Names MUST be trimmed; an empty name after trimming means no name. Goal names need not be
  unique.
- **FR-010**: A goal without a name of its own MUST be named by the habit's current name, ", " and the goal's
  target text (slice 2 FR-017), produced when read, so that a renamed habit renames it (ADR 0012): "Reading,
  15 min, 5 days a week", "Cold shower, daily", "Sugar, at most 1 sweet a day".

**Time model**

- **FR-011**: A goal's days and weeks MUST be decided by its owner's time zone (FR-024), not by a time zone
  chosen per goal (Clarifications 2026-11-17). Each settings version keeps the time zone in effect when it was
  created; a goal is created with its owner's current time zone. A later change of the owner's time zone adds
  a version (FR-026) and never moves a commitment or evaluation period that has already begun or ended.
- **FR-012**: A day MUST run from midnight to the next midnight in the time zone of the settings version in
  effect for that day, so a day on which the clocks change lasts 23 or 25 hours. A week MUST run from Monday to
  Sunday. "Today" is the date in the acting user's current time zone at the supplied current time; Domain and
  Application never read the clock or the machine's time zone (constitution II).
- **FR-013**: Reading a goal's evaluation calendar for a range of dates (first and last date included, 1 to
  366 days, otherwise "Choose a range of 1 to 366 days.") MUST return, in date order:
  - for a commitment-based goal, one commitment per scheduled day in the range on or after the start date,
    with its date;
  - for a period-based goal per day, one evaluation period per day in the range on or after the start date;
  - for a period-based goal per week, one evaluation period per week that overlaps the range, with its first
    and last date; the first is the first Monday-to-Sunday week that starts on or after the start date
    (Clarifications 2026-11-17), so no week is shortened;

  each with the instant at which it starts and the instant at which it ends (FR-012). It returns no status:
  Pending, Met, Failed and Missed are slice 5's.
- **FR-014**: Listing goals MUST give each goal, besides FR-016's summary fields: when its start date is after
  today, that it starts on that date; otherwise, for a commitment-based goal, the date of its next commitment
  on or after today (today included). A per-week goal whose first week (FR-013) has not begun also gives the
  date on which its first week starts.

**Listing and reading (owner only)**

- **FR-015**: Listing MUST return all of the acting user's goals, in name order (slice 2 FR-004 comparison)
  and then by creation order, each with its identifier, name, habit name and kind, Build or Reduce,
  measurement, target text, schedule kind and FR-014 information. All goals are active in this slice (pauses:
  slice 7; ending: slice 8).
- **FR-016**: Reading a goal MUST return its identifier, name, whether the name is its own, the habit's current
  name, kind and category, whether the habit is still in the catalog (catalog habits only, FR-033), Build
  or Reduce, measurement, amount and unit, schedule, schedule kind, target text, start date, the date from
  which its settings apply, its time zone, for a per-week goal the date its first week starts (FR-013), and
  its attempt (FR-018).
- **FR-017**: Goals MUST NOT be edited, paused, ended, restarted or deleted in this slice (roadmap decision R4,
  Clarifications 2026-11-17). Pausing comes in slice 7; editing settings, ending and restarting in slice 8.
- **FR-018**: Creating a goal MUST also start its first attempt: attempt number 1, beginning on the goal's start
  date, not ended (Clarifications 2026-11-17). Every goal of this slice has exactly this one attempt; reading
  a goal returns its number and start date. Commitments and evaluation periods belong to the attempt in which
  they fall, so slice 8's restart adds attempts without moving existing ones.

**Privacy**

- **FR-020**: Every goal use case MUST take the acting user and act only on that user's own goals
  (constitution III). No use case returns or changes another account's goal, and none looks a goal up by
  identifier alone. A goal identifier that is not one of the acting user's goals gives "This goal does not
  exist.", so the result never reveals that someone else's goal exists.
- **FR-021**: Goals MUST be for registered users only (Clarifications 2026-11-17): every goal use case
  refuses an administrator with "Goals are for registered users." and returns no goal of any account.
- **FR-022**: Every goal use case MUST refuse an anonymous visitor with "You are not signed in.".
- **FR-023**: Every use case of this slice MUST give one result, checking in this order: (1) an anonymous
  visitor where a signed-in user is needed: "You are not signed in."; (2) the role (FR-021, and slice 2 FR-014
  for catalog administration); (3) the identifier: goal (FR-020), habit (FR-008), suggested starting goal
  position (FR-007); (4) the field checks (FR-006) or the calendar range (FR-013).

**Account time zone (Clarifications 2026-11-17)**

- **FR-024**: Every account MUST have a time zone, one of the standard named time zones (for example
  "Europe/Kyiv", "America/New_York"). It is account data, readable and changeable only by the account itself
  (constitution III); no use case shows it to anyone else, administrators included.
- **FR-025**: Registration (slice 1 FR-001) MUST accept the browser's time zone as an optional value. A named
  time zone becomes the account's time zone; an absent or unknown value is not refused, and the account gets
  the installation's default time zone. Accounts created before this slice, the configured administrator and
  the demo accounts have the installation's default time zone. The default is an operator setting; when it is
  absent or not a named time zone, it is UTC.
- **FR-026**: A signed-in account MUST be able to read its time zone and change it. A value that is not a named
  time zone gives "Choose a time zone from the list." and changes nothing; the same time zone is a success that
  changes nothing. A change made on date C (today in the old time zone) applies to each of the account's goals
  from the start of its next commitment or evaluation period: for a commitment-based goal and a per-day goal
  from the day after C, for a per-week goal from the Monday after C's week, and for a goal whose start date is
  after C from its start date. It adds a settings version effective from that date with the new time zone and
  all other settings unchanged. Commitments and periods before that date keep the old time zone.
- **FR-027**: Where the old day ends and the new day begins at different instants (a gap after moving west, an
  overlap after moving east), the evaluation calendar MUST give each commitment or period the instants of its
  own version's time zone; no commitment or period is merged, split or dropped.

**Account name, Home greeting and "member since" (Clarifications 2026-11-17)**

- **FR-028**: An account MAY have a name: optional at registration (slice 1 FR-001) and changeable in Profile
  by the account itself, registered user or administrator. It is trimmed; empty after trimming means no name;
  longer than 60 characters gives "The name can have at most 60 characters." (at registration, after slice 1's
  checks) and changes nothing. The name is private account data: no use case gives it to another account,
  administrators included, and it is not the public display name of community participation (slice 9), which
  stays separate.
- **FR-029**: Reading the Home heading MUST give the signed-in acting user a greeting and today's date at the
  supplied current time in the account's time zone (FR-024): "Good morning" from 05:00 to 11:59, "Good
  afternoon" from 12:00 to 17:59, "Good evening" from 18:00 to 04:59; with a name, followed by ", " and the
  name ("Good evening, Yuri"). The date is the weekday, day and month in English, independent of the server's
  culture ("Tuesday, 17 November"). An anonymous visitor gets "You are not signed in.".
- **FR-030**: Each account created from this slice on MUST record when it was created. Reading one's own
  profile gives the name, the email and "member since" with the month and year of creation in the account's
  time zone ("member since November 2026"). Accounts created before this slice have no recorded creation time
  and give no "member since".

**Catalog and custom habits with goals**

- **FR-030**: Browsing the catalog MUST accept "Only habits I track" for a registered user, returning only the
  Published catalog habits with at least one of the acting user's goals, together with the other filters
  (slice 2 FR-009). For a registered user, browsing MUST give each habit the acting user's own goal count and
  the number of returned habits the user tracks; details MUST give the user's goal count and their goals on
  that habit (identifier and name, in name order).
- **FR-031**: Goal counts and goals in catalog results MUST be the acting user's own only. For an anonymous
  visitor and an administrator, browsing and details give the slice 2 result with no goal information; "Only
  habits I track" is refused with "You are not signed in." or the FR-021 refusal.
- **FR-032**: Unpublishing a Published catalog habit that at least one goal uses MUST, without confirmation,
  give "confirmation needed" with the number of goals and the text "1 goal uses this habit." or "N goals use
  this habit.", and leave the habit Published; with confirmation it unpublishes. The result carries only the
  number and the text (ADR 0012). A habit that no goal uses is unpublished at once, with or without
  confirmation.
- **FR-033**: A goal on an Unpublished catalog habit MUST keep working for its owner, with the habit's current
  name and unchanged settings; reading it says the habit is no longer in the catalog (FR-016). No new goal can
  be based on that habit, by its existing goals' owners or anyone else (FR-008). If an administrator publishes
  it again, it is in the catalog again for new goals (FR-008).
- **FR-034**: Deleting a custom habit that at least one goal uses MUST give "Goals use this custom habit, so it
  cannot be deleted." and change nothing. Editing a custom habit never changes a goal's settings; the goal shows
  the habit's current name (FR-010).

**Demonstration data**

- **FR-040**: Demo loading MUST give each demo account that it creates in a run its demo goals, in the same run
  and as a whole with the account (ADR 0013), after the seed catalog and the demo custom habits:
  - `demo1@example.com`: on catalog habit "Reading", Build, Duration 15 minutes, fixed days Monday to Friday;
    on custom habit "Cold shower", Build, Yes/no, fixed days on all seven days;
  - `demo2@example.com`: on catalog habit "Yoga", Build, Yes/no, 3 times per week; on custom habit "Evening
    stretch", Build, Duration 10 minutes, fixed days Monday, Wednesday and Friday, starting 7 days after the
    day of the run;
  - `demo3@example.com`: on catalog habit "Sugar", Reduce, Count at most 1 "sweet", 1 time per day; on custom
    habit "Late snacks", Reduce, Yes/no, 1 time per day.

  Unless stated, a demo goal starts on the day of the run, has no name of its own and uses the demo account's
  time zone, the installation's default (FR-025). A demo goal on a catalog habit that is not Published at that point is left out. For each created
  account the goal result is "demo goals added". An account that already exists gets no goal result and none
  of its private data is read or changed. Results name the demo email but never a goal or habit name.
  Time-relative demo results (Pending, Met, Missed) are slice 5's.

### Host, page and integration requirements (outside the AC surface)

- **HP-001**: The New goal page (`/goals/new`, with the habit and suggested starting goal in the address) MUST
  follow `design/pages/PageNewGoal`: "← " back to where the user came from, "New goal", the line naming the
  habit ("Based on the catalog habit Reading. Change anything you like." or the custom-habit wording of the
  design change), the form sections Habit (with Change and the habit chooser), "Do you want to build or reduce
  it?", "How do you measure it?", Target ("At least" / "At most", Stepper, unit), "When?" (fixed days with
  DayPicker, or a number of times per day or per week), Start date, "Name (optional)", "Create goal" and
  Cancel. Refusals sit under their field, one at a time. "Create goal" opens the new goal's page.
- **HP-002**: The summary card MUST rewrite the goal as one sentence and list Evaluated ("Each scheduled day",
  "Each day", "Each week"), "Met when", "Streak counted in" (commitments, days or weeks) and "Late records"
  ("Up to 48 h after each day" or "… after each week"), and, when a suggestion was used, "Catalog suggestion:
  … An example to adjust, not a prescribed target." For a per-week goal whose start date is not a Monday, it
  adds "Your first week starts on Mon 23 Nov. Days before it are not evaluated."; "Create goal" asks for no
  confirmation. Its texts come from one shared formatter (constitution IV).
- **HP-003**: The Goals page (`/goals`) MUST follow `design/pages/PageGoals` with only the parts this slice
  ships: "Goals", no Tabs (Paused and Ended come in slices 7 and 8), one `ht-list` of GoalListItems with name
  link, intent Tag and meta line (schedule, then "next today", "next Wed 18 Nov", "starts Mon 23 Nov" or
  "first week starts Mon 23 Nov"); no
  consistency, streak, strip or action (slices 5 and 6). With no goals, the EmptyState "No goals yet" with
  "Browse the catalog" and "Create custom habit".
- **HP-004**: The goal page (`/goals/{id}`) MUST follow `design/pages/PageGoalDetail` with only the parts this
  slice ships: "← Goals", the name, the tags, the attempt line ("Attempt 1 · started 14 Jul", or "Attempt 1 ·
  starts 23 Nov"), and the Settings card (Habit with "catalog" or "custom", Intent, Measure, Target, Schedule,
  Evaluated, Started) with its sentence "Changes apply from the next commitment. Past results keep their
  rules." (ADR 0008). For a goal on an Unpublished catalog habit, the Notice "This habit is no longer in the
  public catalog. Your goal keeps working." under the page head (FR-033). No actions, GoalStats, trend,
  history, pauses, notes or earlier attempts. Another account's or an unknown goal shows the Not found page.
- **HP-005**: The signed-in catalog (`design/pages/PageCatalogSignedIn`) MUST add, for registered users: the
  "N goals" tag and "Add another goal" on tracked habits' cards, "Use this habit" on the others, "Show: Only
  habits I track" in the FilterPanel (kept in the address), and "· you track N" after the count.
- **HP-006**: The habit details (`design/pages/PageHabitDetail`) MUST add, for registered users: the head
  button "Use this habit", the tag "You track this · N goals" when N is at least 1, "Start with this" on each
  suggested starting goal, and "Your goals on this habit" with each goal's name linking to its page. No
  consistency or streak there (slice 6), no community card (slice 9).
- **HP-007**: The custom habit page (`design/pages/PageCustomHabit`) MUST add "Save and create goal", which
  saves the habit and opens the New goal page with it, and show "Delete" only when no goal uses the habit.
- **HP-008**: The administrator's Unpublish MUST open a Dialog with "N goals use this habit." and a sentence
  that the goals keep working for their owners, before the confirm button (`design/components/Dialog`).
- **HP-009**: TopNav and MobileNav MUST add "Goals" and "+ New goal" for registered users only (FR-021).
- **HP-010**: The signed-in Home MUST have the page head of `design/pages/PageHomeSignedIn`: the greeting of
  FR-029 as `heading-1` and today's date, for registered users and administrators (it replaces the slice-1
  heading "Today"). A registered user with no goals then sees the slice-4 EmptyState (the current text "Goals
  come in a later version." goes); with goals, nothing more until slice 6 (ADR 0008).
- **HP-011**: Logs, exception messages and error pages MUST NOT contain goal names, custom-habit names, account
  names, units or amounts (constitution III).
- **HP-012**: Every page of this slice MUST meet constitution IV: tokens and `ht-` classes only, visible focus,
  status never by color alone, the habit chooser Dialog managing focus, sentence-case text per
  `design/brand-book.md`.
- **HP-013**: The host MUST log demo loading's goal results as "Demo goals …" lines with the demo email and the
  result text only.
- **HP-014**: `CONTEXT.md` MUST gain, in the change that settles the open decisions: Personal goal, Attempt,
  Commitment, Evaluation period, Commitment-based and Period-based goal, Goal settings (version), the goal's
  time zone, the account's time zone, the account name, and the evaluation calendar; each with a NOT-boundary (constitution I).
- **HP-015**: The Profile Account tab (`design/pages/PageProfile`) MUST add a "Time zone" field with the named
  time zones and Save, for every account. After a change it shows the Notice "Your time zone is changed. Your
  goals use it from their next day or week; earlier days keep their time zone."
- **HP-016**: The Register page MUST send the browser's time zone with the registration, with no visible field;
  a browser that gives none registers with the default (FR-025).
- **HP-017**: The Register page MUST add an optional "Name" field, and the Profile Account tab a "Name" field
  with Save and the Notice "Your name is changed."; the Profile header shows the name (when set), the email and
  "member since …" (when known), as `design/pages/PageProfile` shows. The avatar letter is the first letter of
  the name, or of the email without a name.

### Slice 1 and slice 2 requirements and tests this slice changes

Constitution IV and ADR 0008: these slice-2 states end when this slice ships, and their tests change in the
same change.

- Slice 2 FR-015 (unpublishing with goals): built by FR-032 and FR-033. Tests: catalog administration
  integration tests and `tests/HT.UiTests/Components/Pages/AdminPageTests.cs`.
- Slice 2 FR-020/FR-022 (deleting a custom habit): extended by FR-034.
- Slice 2 HP-002 (signed-in catalog without goal tags or goal actions), HP-005 (no head button for signed-in
  users), HP-007 (custom habit page without "Save and create goal"): amended by HP-005 to HP-007. Tests:
  `CatalogPageTests.cs`, `CatalogHabitDetailTests.cs`, `CustomHabitPageTests.cs`.
- Slice 2 HP-011 (navigation without Goals): amended by HP-009. Test: `SiteNavTests.cs`.
- Slice 2 HP-013 (Home EmptyState "Goals come in a later version."): amended by HP-010. Test:
  `HomeSignedInTests.cs`.
- Slice 2 FR-030/FR-031 and HP-017 (demo loading): extended by FR-040 and HP-013. Test:
  `tests/HT.UiTests/Operator/StartupTasksDemoTests.cs` gains the "Demo goals" lines.
- Slice 1 FR-001 (registration): extended by FR-025, and the Register page by HP-016. Slice 1 HP-014 and slice
  2 HP-008 (the Profile Account tab: email and Change password): extended by HP-015. Accounts that exist when
  this slice ships get the default time zone through a migration.
- Slice 1 FR-001 and the Register page: extended by FR-028 and HP-017 (optional name). Slice 1 HP-013 and
  slice 2 HP-013 (signed-in Home heading "Today"): amended by HP-010 (greeting and date). Slice 1 HP-014
  (Profile header with the avatar letter and the email): extended by HP-017. Tests:
  `tests/HT.UiTests/Components/Pages/HomeSignedInTests.cs`, the Register and Profile page tests.

### Design changes before build

Constitution IV and ADR 0008 require `design/` to change first:

- `design/pages/PageNewGoal`: the custom-habit and no-habit states of the head line and Habit section, the habit
  chooser (a Dialog with a SearchField over Published catalog habits and the user's custom habits), the
  refusal states of FR-006, a future start date, and the summary card texts of HP-002 for each schedule kind.
- `design/pages/PageGoals`: the slice-4 state (no Tabs; GoalListItem with name, intent Tag and meta line only)
  and the EmptyState. `design/components/GoalListItem`: that reduced state.
- `design/pages/PageGoalDetail`: the slice-4 state of HP-004, with a Target row in Settings and the "no longer
  in the catalog" Notice of FR-033.
- `design/pages/PageCatalogSignedIn`, `PageHabitDetail`, `PageCustomHabit`: the slice-4 states of HP-005 to
  HP-007.
- `design/pages/PageAdmin`: the unpublish confirmation Dialog of HP-008.
- `design/pages/PageHomeSignedIn`: the slice-4 EmptyState, and the head with the greeting without a name
  (FR-029).
- `design/pages/PageProfile`: the "Time zone" and "Name" fields and their Notices on the Account tab, and the
  header without a name or without "member since" (HP-015, HP-017).
- `design/pages/PageRegister`: the optional "Name" field (HP-017).
- `design/components/TopNav`, `MobileNav`: "Goals" and "+ New goal" for registered users.
- Texts that disagree with slice 2 FR-017: the previews' goal names "Yoga, 3 times a week" for fixed days
  Mon/Wed/Fri (FR-010 gives "Yoga, 3 days a week") and "Coffee, at most 2 cups" ("Coffee, at most 2 cups a
  day"), and "Streak counted in Days" / "Streak, sessions" for commitment-based goals (HP-002 says
  "commitments", the SPEC §2 term; constitution I).
- `design/pages.md` and `design/README.md`: the slice-4 structure of these pages.

### Key Entities

- **Personal goal**: one registered user's measurable commitment based on one catalog or custom habit; owned
  by that user and visible only to them. Has a name of its own or a derived one, and its settings versions.
- **Goal settings (version)**: Build or Reduce, measurement, amount and unit, schedule, time zone and the date
  from which the version applies. This slice creates the first version only.
- **Attempt**: a continuous pursuit of a goal from its beginning until it is ended (SPEC §2): number and start
  date; attempt 1 starts with the goal (FR-018). Ending and restarting are slice 8's.
- **Commitment**: one scheduled day of a commitment-based goal, from midnight to midnight in the goal's time
  zone.
- **Evaluation period**: one day or one Monday-to-Sunday week of a period-based goal.
- **Evaluation calendar**: the commitments or evaluation periods of one goal over a range of dates, with their
  start and end instants (FR-013).
- **Account time zone**: the named time zone of one account, chosen in its profile, that decides its goals'
  days and weeks from the next commitment or period after each change (FR-024 to FR-026).
- **Account name**: an optional, private name of one account, used in the Home greeting and the Profile header
  (FR-028). Not the public display name of community participation.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A registered user can create a goal from a catalog habit's suggested starting goal, without
  changing any field, in under 1 minute and in at most 3 actions from the habit's details.
- **SC-002**: Every goal use case has negative tests (constitution III): another registered user, an
  administrator and an anonymous visitor are denied, and none of them gets any goal of another account (US3).
- **SC-003**: For each of the three schedule kinds, the evaluation calendar's boundaries are exact on both
  sides of midnight and on both clock-change days of the goal's time zone, in 100% of the boundary tests
  (constitution II).
- **SC-004**: Every suggested starting goal of the seed catalog can be turned into a goal without changing a
  field (FR-007).
- **SC-005**: The unpublish confirmation carries the number of goals and nothing else, in 100% of cases
  (US7-AS5).
- **SC-006**: After a fresh deployment, each demo account shows at least two goals; together they cover both
  schedule kinds, Build and Reduce, catalog and custom habits, and one goal that starts in the future.
- **SC-007**: Every page of this slice passes the accessibility rules of constitution IV (HP-012).

## Decisions due before acceptance criteria

The roadmap places these decisions in this slice, before `/speckit-ac-translate`. Each is answered by the owner
and recorded as a dated line in `## Clarifications`, plus an ADR where it spans features.

- Decided 2026-11-17 (see `## Clarifications`): roadmap decision R4, goal changes go to slice 8 (FR-017);
  administrators have no goals (FR-021); commitment deadlines and time zones, SPEC §11.1 first part: the
  account's own time zone, editable in Profile (FR-011, FR-024 to FR-027,
  [ADR 0016](../../docs/adr/0016-time-zones.md)).
- The installation's default time zone for the hosted demo is set by the operator (FR-025); it is
  configuration, not a product decision.
- Asked in `/speckit-clarify` on 2026-11-17:
  - **D-1** (FR-018): whether a goal is created as its first attempt. Decided 2026-11-17: yes, attempt 1 from
    the start (see `## Clarifications`).
  - **D-2** (FR-004): what the target of a period-based goal means. Decided 2026-11-17: count the times (see
    `## Clarifications`).
  - **D-3** (FR-013): the first evaluation period of a weekly goal that starts mid-week. Decided 2026-11-17:
    the first full week, with the user told and no confirmation (see `## Clarifications`).
  - **D-4** (FR-028 to FR-030): whether an account has a name, and the Home greeting and Profile "member
    since". Decided 2026-11-17: name, greeting and date (see `## Clarifications`).
  - **D-5** (FR-016, FR-033): how a goal on an Unpublished catalog habit looks to its owner. Decided
    2026-11-17: a Notice on the goal page and no new goals on that habit (see `## Clarifications`).
- No decision of this slice is open.

## Assumptions

- The greeting's hour boundaries (05:00, 12:00, 18:00) and the 60-character limit of the account name are
  this spec's defaults. Accounts created before this slice get no "member since", since no creation time was
  stored for them; the hosted demo's accounts are created again at every deployment, so they have one.

- Decision R7 (ADR 0008) applies: parts owned by slices 5 to 11 are absent, not disabled; sentences keep the
  previews' wording.
- Recording, evaluation, Pending and Missed (slice 5), reports, consistency and streaks (slice 6), excusals and
  pauses (slice 7), and Suggest for catalog (slice 10) are out of scope.
- A goal's start date is today or later, at most 365 days ahead; a start date in the past would create
  commitments that slice 5 would have to treat as already due. Demo goals start on the day of the run or later
  for the same reason.
- The ranges and units of slice 2 FR-007 (amount 1 or 0 to 999, unit up to 20 characters, 1 to 20 times; per
  day or per week only) are kept for goals; periods longer than a week ("or other defined interval", SPEC §2)
  stay out of version 1.
- Goal names follow the 60-character limit of habit names.
- "Delete only when no goals use it" for custom habits comes from the roadmap's page tracker (FR-034); it keeps
  a goal from losing its habit.
- No account is deactivated yet (slice 11), so this slice has no rule for a deactivated owner's goals.
- The 48-hour grace period is described on the summary card only; its behaviour, including around clock
  changes, is slice 5's.
