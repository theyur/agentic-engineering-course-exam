# Critic pass 1: tasks for 009-goal-changes

**Verdict**: APPROVE
**Date**: 2026-11-22
**Reviewed**: specs/009-goal-changes/tasks.md (against spec.md, plan.md, research.md, data-model.md, quickstart.md,
contracts/application-api.md, contracts/host.md, acceptance.md, the constitution, CLAUDE.md, the existing code and
tests, and specs/008-excusals-and-pauses/tasks.md as precedent)

## Notes

### MINOR-1: Broken task cross-references
- **Where**: tasks.md T002 (lines 204 and 206: "T127 compares them", "If T127 finds the files missing"); T077 (line 410:
  "a link to `/goals/{id}/new-attempt`, whose page T087 adds").
- **Against**: the task list itself. The IDs run T001 to T120, so T127 does not exist. The public API comparison is
  T117, which says "Compare … with T002's baseline". The new-attempt page is created by T089. T087 is
  `tests/HT.IntegrationTests/Goals/GoalAttemptTests.cs`.
- **Reasoning**: an implementer following T002's fallback, or T077's pointer to where the linked page is created,
  lands on a task that doesn't exist or on an unrelated one. The T002 fallback also creates a fresh temporary
  `git worktree` and runs `roslynator list-symbols` there, but it doesn't say to run `dotnet restore` first.
  CLAUDE.md "C# code analysis" says Roslynator needs `dotnet restore HabitTracker.slnx` first because "Fresh worktrees
  have no `obj/`". T002's own first step does that restore, but the fallback leaves it out, so a regenerated baseline
  can differ from the original one for reasons unrelated to the slice.

### MINOR-2: Wrong count of `CatalogService` constructors
- **Where**: tasks.md Conventions, "Locked AC code" (line 112: "the second `Goal` constructor and the third
  `CatalogService` constructor"); T049 (line 329: "the two existing constructors keep their behaviour").
- **Against**: `src/HT.Application/Habits/CatalogService.cs:25` has a single constructor,
  `CatalogService(ICatalogHabitStore store, IGoalStore goals)`, and so does the exported API listing
  (`artifacts/refs/api/HT.Application.cs:5490`). data-model.md ("The two-argument constructor stays") and research.md
  R-08 describe one existing constructor and one added one.
- **Reasoning**: the task text asserts a third public constructor and a second existing one that don't exist. That
  conflicts with the design artifacts and could mislead T117's "only additions" API diff, and T049's reasoning about
  which constructor `AddScoped<CatalogService>()` resolves.

### MINOR-3: `FakeGoalStore` has no `Calls` list, yet tasks rely on it "like the existing ones"
- **Where**: tasks.md T035 ("Every new member appends its name to `Calls`, like the existing ones"; "Add a
  `Seed(Goal goal)` helper"). Tests that read it: T062 ("`Calls` holds no write"), T100 ("`Calls` holds no write, and
  a refusal before the goal holds no goal read").
- **Against**: `tests/HT.UnitTests/Fakes/FakeGoalStore.cs` exposes counters (`CreateCount`, `TimeZoneWriteCount`,
  `TimeZoneChangeCount`, `Lookups`) and no `Calls`. Only `FakeRecordStore` has `Calls`
  (`tests/HT.UnitTests/Fakes/FakeRecordStore.cs:46`). `FakeGoalStore` already has an arranging helper,
  `Add(AccountId owner, Goal goal, GoalHabitView habit)` (line 124).
- **Reasoning**: the claim about the existing fake is factually wrong. An implementer has to invent a new `Calls`
  property on `FakeGoalStore`, which only the new members feed, while T100's "no goal read" check is the kind the
  existing `GoalAccessTests` make through `Lookups`. Asking for `Seed(Goal)` beside the existing `Add` also leaves two
  overlapping seeding paths, and T035 doesn't say how they relate.

### MINOR-4: Public types listed in R-12 never get their `PublicSurfaceTests` rows
- **Where**: tasks.md T042 (`GoalChangeTexts`), T043 (`EntrySettings`) and T048 (`GoalSettingsView`). None of them,
  and no other task, adds these types to `tests/HT.ArchitectureTests/PublicSurfaceTests.cs`. T032, T061, T083, T093
  and T108 add the other new types.
- **Against**: research.md R-12 "Architecture": "the new public Domain and Application types added to
  `PublicSurfaceTests`' NodaTime theory: … `EntrySettings`, `GoalSettingsView`, `GoalChangeTexts`". Plan,
  Constitution Check V: "the new public types join `PublicSurfaceTests`".
- **Reasoning**: the blanket assembly check still covers these types, but the named-type guard rows the design
  promises are partly missing. The precedent in `PublicSurfaceTests.cs` explains the guard: the named rows exist "so
  moving them cannot make the rule pass on nothing".

### MINOR-5: Dependency section understates what US6 and the shared read path need
- **Where**: tasks.md "Phase dependencies" ("US6 (Phase 8): after Phase 2 for T104 to T110"; "the shared read path
  (T044 to T050) needs T017 to T021, T036 and T043") and "User story dependencies" ("US6 (P2): data and loading need
  Phase 2 only").
- **Against**: T110 reads "`demo7`'s goals, attempts, reports, dashboard and goal history". The attempts come from
  `AttemptService.GetAttemptsAsync` (T082, US3) and the goal history from `GetGoalHistoryAsync` (T093, US4). T046 says
  "after T039", not T036. research.md R-09 also says the FR-031 figures "are proved by unit tests at a fixed run date
  on each weekday". In the tasks, T109 (unit) checks only the structure of `DemoGoalChanges.For`, and the figures
  move to T110 (integration).
- **Reasoning**: anyone using the dependency section to pull US6 forward, or to run it alongside other work, would
  start T110 before its services exist. The move of the figure proof from unit to integration tests departs from
  R-09, and nothing records it.

### MINOR-6: T057 needs ended and restarted goals before any use case can make them
- **Where**: tasks.md T057 (Phase 2): "through the real stores, an ended goal leaves the catalog counts and goal list …;
  the History and recent notes of a restarted goal cover only the latest attempt".
- **Against**: `EndAsync` and `StartAttemptAsync` only arrive in T071 (US2) and T080 (US3). Conventions: "Goals with past
  results are created at an earlier supplied time, then recorded, changed, ended, restarted and read at later ones" and
  "Only Phase 2's seeded goals (T051) and demo goal-change loading write past facts directly". T051's seeding is a
  unit-test fake (`FakeGoalStore`), not the real stores.
- **Reasoning**: in Phase 2 the only way to end or restart a goal on SQL Server is to call the new `IGoalStore`
  members directly with hand-built write records. T057 doesn't say so, and the Conventions' list of who may write
  facts directly doesn't cover it. The task is feasible but its setup is unspecified.

### MINOR-7: The attempt view doesn't say how it handles the History card's excusal controls or how its interactive part is structured
- **Where**: tasks.md T091 (`AttemptDetail.razor`: "`HistoryCard` over `AttemptDetails.History`, whose record and note
  actions open the RecordDialog or NoteDialog fixed to their row").
- **Against**: in the existing code, the History card also carries "Remove excusal" through a parent callback, and the
  RecordDialog carries "Excuse instead", which `GoalDetailBody` handles (specs/008-excusals-and-pauses/tasks.md T049,
  T050, T052). FR-010 says excusing a Pending commitment of the ended attempt and removing an excusal "MUST keep
  working". The goal page is a static page (`GoalDetail.razor`) that hosts the interactive `GoalDetailBody` with the
  identifier only. The Conventions forbid a static page passing a view into an island (slice 6 R-12).
- **Reasoning**: T091 names one page file. It doesn't say whether the excusal controls on the attempt view are wired
  up or hidden, or which interactive component reads `GetAttemptAsync` and owns the dialogs. An unwired callback fails
  silently, so the behaviour of an earlier attempt's Excused or Pending rows on that view is left open.

### MINOR-8: T037's mapping check is stricter than data-model.md for attempt 1
- **Where**: tasks.md T037: the mapping checks "… every attempt but the latest ended, first versions on their attempt's
  start date".
- **Against**: data-model.md "Tables": "attempt 1's first version applies from the goal's start date, and every later
  attempt's first version from that attempt's start date". data-model.md "Goal": "An attempt 1 that starts after
  `startDate` stays accepted, as in slice 4".
- **Reasoning**: T037's wording would make the mapping fault on a stored attempt 1 whose start date differs from the
  goal's start date. The Domain explicitly keeps that combination accepted. The mapping and the constructor would then
  disagree about what valid data is.

### MINOR-9: The Start new attempt form's labels and first-field focus aren't in its test task
- **Where**: tasks.md T092 ("the attempt form's fields and start date, each refusal under its field, the line"),
  compared with T070 for the edit form ("labels and focus on the first field").
- **Against**: spec HP-009 ("Every dialog and form of this slice MUST manage focus (first field on open …)"), SC-008,
  plan Constitution Check IV ("bUnit tests check labels, focus order, Esc and the focus return of every page and
  dialog of this slice").
- **Reasoning**: the general Conventions mention the rule, but the tasks spell it out for the edit form and the dialog
  and leave it out for the attempt form and page (T088, T089, T092). That gap makes it easy to ship HP-009 untested on
  one of the three new pages.

### MINOR-10: Two parallel Domain test tasks touch the same files
- **Where**: tasks.md T027 [P] ("add to `GoalTimelineChangeTests.cs`") and T029 [P] ("run the timeline cases of …
  `GoalTimelineChangeTests.cs` … Reuse the existing cases' data (a shared `TheoryData` or a sample builder in
  `GoalSamples.cs`); do not copy them"). T025 and T026 [P] probably also build their goals through
  `tests/HT.UnitTests/Domain/Goals/GoalSamples.cs`.
- **Against**: the Format rule: "[P]: the task can run in parallel with the other [P] tasks of its group (they touch
  different files)".
- **Reasoning**: T029 has to refactor `GoalTimelineChangeTests.cs` (and possibly `GoalSamples.cs`) to share its cases,
  while T027 edits the same file at the same time. The [P] markers promise file independence that doesn't hold here.

## Summary

0 MAJOR, 10 MINOR. Coverage is complete. Every FR (001 to 027, 030, 031), HP (001 to 014), SC (001 to 008), user
story scenario and edge case maps to at least one implementation or test task. The locked-AC-code rule matches the
plan. The commit grouping of port members with every implementer matches the code: `GoalStore`, `RacingGoalStore`,
`BeforeFirstCreate` and `FakeGoalStore` for `IGoalStore`; `RecordStore`, the two `FaultingRecordStore`s and
`FakeRecordStore` for `IRecordStore`. Every file, test, architecture rule and line reference I checked exists as
described: `RecordLogTests.cs:421-429`, `GoalService.cs:272`, `CoverageTests` counts at lines 97, 115 and 133, and the
architecture test names. The notes are wrong cross-references, two wrong descriptions of existing code
(`CatalogService` constructors, `FakeGoalStore.Calls`), a few guard rows and test details the design promises but the
tasks leave out, an inaccurate dependency section, and some underspecified setup and structure (T057, T091). None of
them blocks `/speckit-implement`, but MINOR-1 to MINOR-3 and MINOR-8 should be corrected so the implementer doesn't
follow false statements.
