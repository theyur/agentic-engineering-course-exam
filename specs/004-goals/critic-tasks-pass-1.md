# Critic pass 1: tasks for 004-goals

**Verdict**: REVISE
**Date**: 2026-11-18
**Reviewed**: specs/004-goals/tasks.md (against spec.md, plan.md, research.md, data-model.md, quickstart.md,
contracts/application-api.md, contracts/host.md, the constitution, ADRs 0016/0017, the F004 feature file, and the
current code and tests in src/, tools/ and tests/)

## Notes

### MAJOR-1: The Goals page's meta line needs the schedule, but no task gives the page the days to write it
- **Where**: tasks.md T069 (line 589: "The meta line is the schedule, then the first that applies of ..."), T067
  (lines 578-582, which adds only `Today` and `HasStarted` to the views), T070 (line 592), T071 (line 600).
- **Against**: spec HP-003 ("meta line: the schedule, then exactly one of ...") and US2-AS6 ("Mon · Wed · Fri · next
  today", "Flexible · 3 times a week"); `design/components/GoalListItem.html` line 10 and
  `design/pages/PageGoals.html` lines 14-38 ("Mon · Wed · Fri · next today", "Every day · …", "Flexible · …");
  data-model.md line 65, where `GoalListItem` explicitly has no days ("amount, unit and days are not list fields");
  constitution IV (one shared, unit-tested formatter per text).
- **Reasoning**: `GoalListItem` (the only input the Goals page gets from `GoalService.ListAsync`) carries `TargetText`
  and `GoalScheduleKind`, but not the fixed days, the times or the period. A fixed-days goal's "Mon · Wed · Fri" (or
  "Every day") cannot be produced from it: `TargetText` gives "3 days a week", not which days. T067 is the task that
  amends the list view and the contract, and it adds only `Today`. No task names the formatter that writes the
  schedule part either: `GoalText.Kind` gives "Yes/no · fixed days Mon, Wed, Fri", `DateTexts` writes only dates, and
  T069's bUnit tests "for every variant" have no specified source for that text. As written, T069 cannot meet HP-003
  or US2-AS6 without an unrecorded change to the list contract (or an N+1 `GetAsync` per goal, which no task
  describes). The same gap affects the Settings card's "Schedule" row in T070, which also has no named formatter,
  although `GoalDetails` at least carries the days there.

### MAJOR-2: US9's smoke-check change makes the in-process smoke-check test fail until US11 ships the Profile name
- **Where**: tasks.md T121 (lines 956-959: check 5 reads "`/profile` for "Demo One" on demo1"), T116 (lines
  926-928), the US9 checkpoint (lines 961-962, "full suite green"), "Story dependencies" (line 1068: "US9: after US2
  ... It needs no other story"), "Parallel opportunities" (lines 1105-1106).
- **Against**: `tests/HT.UiTests/Host/SmokeCheckHostTests.cs` lines 26-37 (`A_freshly_seeded_host_passes_every_check`
  runs the real `SmokeCheckRunner` against a seeded host and asserts every line is PASS) and lines 54-63
  (`A_demo_account_that_lost_a_custom_habit_fails_only_its_own_check` asserts exactly one FAIL line); T130 (lines
  1004-1008, Phase 13, US11) is the first task that puts the account name on the Profile page; today
  `src/HT.Ui/Components/Pages/Profile.razor` lines 26 and 75 show only the avatar and the email; constitution V
  (commit only with the full suite green).
- **Reasoning**: after T121 in Phase 12, check 5 for demo1 requests `/profile` and expects "Demo One", but the
  Profile header shows the name only after T130 in Phase 13. `SmokeCheckHostTests` is not named by any task, and it
  then fails on both cases above, so the US9 checkpoint cannot be green and its commit is blocked. The dependency
  section states the opposite ("It needs no other story"), and the parallel section allows US9 to run before US11.
  `RunnerBehaviourTests` (T116) uses a scripted site and would not reveal this.

### MINOR-1: Existing bUnit page tests that the new constructor and page dependencies break are not named
- **Where**: tasks.md T040 (line 353: a separate `AddGoals(...)` in `PageServices.cs`), T085 (lines 693-698), T091
  (lines 730-738), T097 (lines 784-791), T111 (line 887), T129 (line 1003).
- **Against**: `tests/HT.UiTests/Components/Pages/PageServices.cs` lines 18-41 (`AddCatalog` registers
  `CatalogService` and `CatalogAdministrationService`; `AddCustomHabits` registers `CustomHabitService`; neither
  registers `IGoalStore`, `IAccountStore` or a `TimeProvider`); `HomeVisitorTests.cs` lines 22-28,
  `AdminCatalogHabitPageTests.cs` line 17, `CustomHabitsPageTests.cs` lines 22-26, which call only
  `AddCatalog`/`AddCustomHabits`; plan.md Work order step 3 (the `FakeGoalStore` registration exists so that these
  page tests keep resolving).
- **Reasoning**: once the three habit services take `IGoalStore` (T085, T091, T097) and `Home.razor` injects
  `GoalService` and `ProfileService` (T111, T129), every bUnit test that resolves them through `AddCatalog` or
  `AddCustomHabits` fails at DI resolution, unless those methods also register the goal fakes. T040 adds a separate
  `AddGoals`, and the tasks name only the page tests whose expectations change (T087, T093, T099, T109, T131).
  `HomeVisitorTests`, `AdminCatalogHabitPageTests` and `CustomHabitsPageTests` are not acknowledged anywhere.

### MINOR-2: The grep convention for hand-built services misses most of them
- **Where**: tasks.md "Conventions for every task" (lines 89-90: `grep -rn "new <Service>(" tests`); T085, T091 and
  T097 ("Update the hand-built instances", naming only `CatalogHabitStateTests.cs`).
- **Against**: the target-typed constructions `private CatalogService Service => new(_store);` in
  `tests/HT.UnitTests/Application/Habits/CatalogBrowseTests.cs:12`, `CatalogDetailsTests.cs:12`,
  `HomePreviewTests.cs:12`, `CatalogHabitFieldTests.cs:13-14` (both services), `CatalogAdministrationAccessTests.cs:21`,
  `CatalogHabitStateTests.cs:13` and `CustomHabitServiceTests.cs:17`.
- **Reasoning**: the stated grep finds no `CatalogAdministrationService` or `CustomHabitService` instance and only
  one `CatalogService` file. The build will reveal the rest, but the convention presents the grep as the way to find
  every caller, and `CatalogHabitFieldTests.cs` is touched by both US6 and US7 without appearing in the shared-files
  list (lines 1070-1087).

### MINOR-3: The "tests first" rule contradicts the tasks' own dependencies
- **Where**: tasks.md "Within each story" (lines 1091-1093: tests "fail before the use case exists"), against T053
  and T054 (depend on T058), T064/T065 (T067, T068), T073 (T075), T080/T081 (T085), T088 (T091), T094 (T097), T100
  (T103), T112-T116 (T117-T121); "Parallel example: User Story 1" (line 1126, "# after T058") and "User Story 9"
  (line 1147, "# after T117 to T121").
- **Against**: internal consistency of tasks.md.
- **Reasoning**: nearly every story test task depends on the use case it tests, and the parallel examples schedule
  them after the implementation. An implementer cannot follow both the general rule and the per-task dependencies.

### MINOR-4: T035's new forbidden members collide with ClockReadTests' own self-check
- **Where**: tasks.md T035 (lines 325-327).
- **Against**: `tests/HT.ArchitectureTests/ClockReadTests.cs`, `EveryForbiddenMemberIsRecognisedInACall`, which
  fails for any entry of `ForbiddenMembers` that its `Canary` does not call, and matches member calls only.
- **Reasoning**: if the NodaTime and `TimeZoneInfo` entries are added to the existing list, the canary must call each
  of them, which needs the architecture test project to compile against NodaTime. `BclDateTimeZone` is a type, not
  a member call, so the existing call-matching mechanism cannot detect it at all. T035 mentions neither point, so the
  rule as described either fails its own self-check or silently misses one of the four NodaTime items.

### MINOR-5: Two host-test steps ask for behaviour that the host cannot show, or that no artifact defines
- **Where**: tasks.md T087 (line 702: "Extend `CustomHabitHostTests.cs` with the redirect to `/goals/new`"); T093
  (line 750: "`tracked=true` ... its refusal for a visitor").
- **Against**: `src/HT.Ui/Components/Pages/CustomHabit.razor` (`@rendermode InteractiveServer`: "Save and create
  goal" runs over the circuit, so there is no HTTP redirect); `tests/HT.UiTests/Host/CustomHabitHostTests.cs` (only
  prerendered GETs); T092 (line 749: "Visitors and administrators keep slice 2's pages"); contracts/host.md (no page
  behaviour for a refused `tracked=true`).
- **Reasoning**: a `WebApplicationFactory` test sees no navigation from an interactive save. And for
  `/catalog?tracked=true` as a visitor, no artifact says whether the page shows the refusal text, ignores the
  filter or shows slice 2's list. So T093 has no expected result to assert.

### MINOR-6: The API baseline lives where T133 may no longer find it
- **Where**: tasks.md T002 (line 106: baseline written "into two files in the session scratchpad"), T133 (line
  1133).
- **Against**: the environment's scratchpad (session-specific; a reboot or cleanup can clear it); the slice spans 14
  phases and many commits, which probably means several sessions.
- **Reasoning**: T133 depends on a file that a later session cannot read. No fallback is given, for example
  regenerating the baseline from `master`.

### MINOR-7: The Razor component `GoalListItem` collides with the Application view `GoalListItem`
- **Where**: tasks.md T069 (`src/HT.Ui/Components/Shared/GoalListItem.razor`), T039 (`HT.Application.Goals.GoalListItem`),
  T070 (`Goals.razor` uses both).
- **Against**: `src/HT.Ui/Components/_Imports.razor` (`@using HT.Ui.Components.Shared` for every component).
- **Reasoning**: inside the component, the simple name `GoalListItem` resolves to the component itself, so its item
  parameter must be fully qualified. In `Goals.razor`, with `@using HT.Application.Goals`, a reference such as
  `IReadOnlyList<GoalListItem>` is ambiguous (CS0104). With `TreatWarningsAsErrors` this does not compile until
  someone qualifies or aliases the name, and no task mentions it.

### MINOR-8: Texts that are to be "defined once" have two homes
- **Where**: tasks.md T036 (lines 333-334: `GoalTexts` gets "the goal texts", while `AccountTexts` gets "The name can
  have at most 60 characters." and `MemberSince(int year, int month)`); "Conventions" (line 51: "Date and month
  texts come only from `DateTexts`"); T126 (line 1000: "member since" from `DateTexts.MonthYear`).
- **Against**: contracts/application-api.md "Texts" (lines 127-128, "defined once"; line 144: the same text for the
  goal and the account `NameTooLong`).
- **Reasoning**: T036 does not say whether the goal's `NameTooLong` text reuses `AccountTexts` or gets its own
  constant in `GoalTexts`. `AccountTexts.MemberSince(year, month)` must produce a month name, which either duplicates
  `DateTexts`' month array (against the convention) or is a wrapper that T036 does not describe.

### MINOR-9: T055 covers "every seed catalog habit", including the Draft and the Unpublished one
- **Where**: tasks.md T055 (line 492).
- **Against**: `src/HT.Application/Operator/SeedCatalog.cs` lines 108-112 (Draft "Music practice", Unpublished
  "Coffee"); spec FR-008 (only Published habits are found); F004 `goals.feature` line 301 (the AC restricts to
  habits that browsing returns).
- **Reasoning**: `StartNewGoalAsync` on those two habits gives "This habit is not in the catalog.", so the test as
  worded cannot give `Created` for every seed habit and every position.

### MINOR-10: Two acceptance details have no unit or integration test task
- **Where**: tasks.md T054 (line 488: only the 00:30 refusal of US4-AS8); T032 and T073 (lines 305-314, 617-623).
- **Against**: spec US4-AS8 ("creating the goal with that Monday as its start date succeeds" at 23:30); FR-013
  ("one evaluation period per week that overlaps the range").
- **Reasoning**: the success half of US4-AS8 at 23:30 is tested only for the starting values (T053), not for
  creation. No calendar test reads a range that starts or ends inside a week, which is the case that tells
  "overlaps the range" from "falls inside the range".

### MINOR-11: The Settings card's "Started" date has no `DateTexts` format
- **Where**: tasks.md T070 (line 591), T007 (line 174), T037 (line 338: `Weekday`, `LongDate`, `ShortDate`, `DayMonth`,
  `MonthYear`); "Conventions" (line 51).
- **Against**: `design/pages/PageGoalDetail.html` line 41 ("Started: 14 Jul 2026").
- **Reasoning**: none of the five `DateTexts` formats gives "14 Jul 2026", and dates may come only from `DateTexts`.
  Neither T007 nor T070 says whether the preview changes to an existing format or `DateTexts` gains one.

### MINOR-12: No task says what the goal and profile use cases do when the acting user's account is missing
- **Where**: tasks.md T058 (line 510: "today" is `(StoredTimeZone ?? InstallationTimeZone).DateAt(now)`), T103
  (line 840), T126.
- **Against**: spec FR-023 (one result per call); `tests/HT.UnitTests/Application/Habits/CatalogInputs.cs` lines 10-12
  (acting users with random identifiers and no stored account, as the slice 2 unit tests use them).
- **Reasoning**: every `GoalService` and `ProfileService` method now reads the acting user's account. No artifact
  defines the outcome when `FindByIdAsync` returns null, so the implementation must choose between a refusal and a
  fault. Unit tests that reuse the existing acting-user samples will reach that case.

### MINOR-13: T102 does not scope the time-zone version writes to the owner's goals
- **Where**: tasks.md T102 (lines 831-838).
- **Against**: constitution III ("lookups of private data by identifier alone MUST NOT exist"); the tasks'
  privacy convention (lines 56-58).
- **Reasoning**: `TryChangeTimeZoneAsync` takes the owner first, but each `GoalVersionWrite` names a goal only by
  `GoalId`. T102 conditions the transaction on the account's zone and revision, but never requires the version
  rows it replaces or inserts to belong to goals of that owner. T101 does not test this either.

### MINOR-14: Nothing enforces that NodaTime stays inside HT.Domain
- **Where**: tasks.md T035 (lines 323-328).
- **Against**: research R-01 ("NodaTime stays internal to `HT.Domain`"); data-model.md line 6.
- **Reasoning**: the three rules forbid NodaTime's clock and BCL providers, and NodaTime types in public signatures.
  None stops `HT.Application` from using NodaTime internally through the transitive reference, for example in
  `GoalSummaryText` or `ProfileService`.

### MINOR-15: The smoke-check changes leave its contract's check 5 and its page reading unspecified
- **Where**: tasks.md T121 (lines 956-959), T116 (lines 926-928).
- **Against**: `specs/003-demo-hosting/contracts/smoke-check.md` line 35 (check 5 is described as `GET /custom-habits`
  only); `tools/HT.SmokeCheck/PageReader.cs` lines 44-46 and 118 (names are read only from `<h3 class="ht-habit__name">`);
  `tests/HT.UiTests/SmokeChecks/PageReaderTests.cs`.
- **Reasoning**: T121 extends only the coverage table of the slice 3 contract. The check 5 row it describes becomes
  wrong. Neither task says how goal names are read from `/goals` (GoalListItem uses its own markup), or how
  "Demo One" is found on `/profile`. They do not mention a `PageReader` change or its tests.

### MINOR-16: Small factual inaccuracies
- **Where / Against / Reasoning**:
  - T050 (line 443) says to check that `GoalStore` is internal "as the persistence rules require", but
    `tests/HT.ArchitectureTests/PersistenceDependencyTests.cs` has one rule, Persistence must not depend on UI. No
    rule requires internal stores, so it is unclear whether T050 adds a test or makes a manual check.
  - T114 (line 917) expects demo3's "Coffee" goal "with `IsInCatalog` false" through `GoalService.ListAsync`.
    `IsInCatalog` exists only on `GoalHabitView` inside `GoalDetails` (data-model.md lines 64-66), not on
    `GoalListItem`.
  - T117 (line 938) puts `ProfileFor(email)` "beside `DemoDataService.DemoEmails`", while research R-10 (line 517)
    names it `DemoAccounts.ProfileFor(email)`. No `DemoAccounts` type exists in `src/HT.Application/Operator/`.

## Summary

2 MAJOR, 16 MINOR. The task list is thorough. It traces almost every FR, HP, SC and acceptance scenario to a test
or an implementation task, and its dates, UTC literals, file paths and type names match the code and the design
artifacts. Two things stop it as written:

- The Goals page's meta line (HP-003, US2-AS6) cannot be built from the specified `GoalListItem`, because the list
  carries no days and no formatter is assigned for the schedule text.
- The US9 smoke-check change breaks the existing in-process smoke-check test until the US11 Profile header exists,
  so the US9 checkpoint cannot pass. This contradicts the stated story independence.

The MINOR notes are mostly unacknowledged test breakages (bUnit DI registrations, the grep convention), ordering
claims that contradict the per-task dependencies, and small gaps in what a task must produce.
