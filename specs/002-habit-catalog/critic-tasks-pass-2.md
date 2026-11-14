# Critic pass 2: tasks for 002-habit-catalog

**Verdict**: APPROVE
**Date**: 2026-11-14
**Reviewed**: `specs/002-habit-catalog/tasks.md` (T001–T131, revised after pass 1), against spec.md, plan.md,
research.md, data-model.md, quickstart.md, contracts/application-api.md, contracts/host.md, acceptance.md
(titles and the items cited below), the constitution, CLAUDE.md, SPEC.md, ARCHITECTURE.md, docs/roadmap.md,
ADRs 0006, 0008 and 0010, `design/` (components, pages, pages.md, README.md, css, icons), the code in `src/`,
the tests in `tests/` (without `tests/HT.AcceptanceTests`) and slice 1's tasks.md.

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 Intent filter differs from `design/` | Resolved | T053 (line 236) builds Category as a radio group and Intent as two checkboxes; T009 (line 114) keeps them "as `design/components/FilterPanel` already has them" (`FilterPanel.html` line 11, both Catalog previews line 16); T065 (line 277) maps the checkboxes to `intent` (one checked is that intent, none or both is no value, which is FR-009's "both" and `CatalogFilter.Intent` null); T057 and T066 test both boxes and none. No "All" option for Intent is left in tasks.md. |
| MAJOR-2 T042 migration public, no startup project | Resolved | T042 (line 207) uses slice 1's T041 command with `--startup-project src/HT.Persistence --output-dir Migrations` (slice 1 tasks.md line 242) and has the step that makes the class `internal`, as `InitialAccounts.cs` line 9 is. `dotnet-ef` 10.0.15 is in the tool manifest. |
| MINOR-1 Design tasks read as removals | Resolved | The new paragraph under "Design first" (lines 98–103) separates replacements from added "Slice 2 state" blocks and says "No …" never deletes from a full preview. Its precedent holds: `PageProfile.html` lines 33–35, `PageHomeSignedIn.html` lines 35–37, `design/pages.md` line 98. T010, T011 and T012 now say "Add the slice-2 state"; T012 keeps the Tabs and "Updated" column for slices 10 and 11. |
| MINOR-2 T091 depends on the whole catalog; no Home preview test for FR-024 | Resolved | T091 (line 381) moves the category-filter case to `CrossKindEmptyCatalogTests.cs` in collection `SeedDatabase` on a reset database; T108 (line 431) adds an integration test with a custom habit that sorts first and five Published habits. One new dependency follows from T108: see MINOR-3. |
| MINOR-3 bUnit page tests lacked fakes | Resolved | One `FakeCatalogHabitStore` (T039, with `Add` for any state) is linked into `HT.UiTests` by T064, as the csproj already links `FakeAccountStore.cs`; T088 links `FakeCustomHabitStore` for T106. |
| MINOR-4 Outcome names collide; `CreateDemoAccountAsync` had no return type | Resolved | Store outcomes are verb first (`CreateCatalogHabitOutcome`, `UpdateCatalogHabitOutcome`, T038; `CreateCustomHabitOutcome`, `UpdateCustomHabitOutcome`, `DeleteCustomHabitOutcome`, T092), matching `CreateAccountOutcome`; result enums are `<UseCase>Outcome` (T079, T095), matching `RegistrationResult` / `RegistrationOutcome`. No two share a name. T119 gives `CreateDemoAccountOutcome` (`Created`, `EmailTaken`) and `bool`. |
| MINOR-5 Retry limit one-sided and ambiguous | Resolved | T081 (line 346): "at most 10 attempts in all", constant `MaxStateSwaps = 10`, as `AccountService.MaxSignInSaves = 10` (loop of 10 saves). T077 (line 339): 9 lost then won, and 10 lost, for publish and unpublish. A new gap in the fake that T077 needs: see MINOR-1. |
| MINOR-6 Statements on independence, test-first order and [P] | Partly | US2, US4 and US5 now name their dependencies (lines 522–529); the [P] definition (lines 27–28) allows named dependencies; T101 depends on T082. Still not accurate in several places: see MINOR-3. |
| MINOR-7 Roadmap page tracker | Resolved | T017 (line 138) adds the `PageAdminCatalogHabit` row in the form of the `PagePrivacy` row (roadmap line 113); T001 (line 70) changes slice note 2 (roadmap line 73) to FR-030's six categories. |
| MINOR-8 Dialog name | Resolved | T100 (line 400) creates `HtDialog.razor` with the four render fragments and a title `id` per instance, as `design/components/Dialog.md` lines 12 and 16; T102 and the parallel example use the name. |
| MINOR-9 Two text errors | Resolved | T023 (line 155): "twelve problems … (eleven check steps, two problems at step 4)", as data-model.md lines 84–95. T089 (line 378) now states the three edit cases, consistent with T093. |

No finding of pass 1 was rejected in the revised artifact; each was acted on in tasks.md. No resolved note has
regressed.

## Notes

### MINOR-1: One case of T077 needs a fake behaviour that T039 does not define

- **Where**: tasks.md T077 (line 339: "one lost race whose re-read finds the other administrator's state
  already the wanted one gives success with no further write"), T039 (lines 188–195, the members of
  `FakeCatalogHabitStore`).
- **Against**: research R-08 ("another administrator acted in between: the service reads again and decides
  again"); the tasks' own T077 cases `FailNextStateSwaps(9)` and `FailNextStateSwaps(10)`; slice 1's
  `FakeAccountStore`, which has a separate member for this kind of case (`BlockOnNextSave`, line 60, beside
  `LoseNextSaves`, line 51).
- **Continues**: MINOR-5 of pass 1 (the rewritten T077), and the class of MINOR-3 of pass 1 (a test task
  needs a fake that no task provides).
- **Reasoning**: T039 defines `FailNextStateSwaps(int count)` as "`TrySetStateAsync` returns false that many
  times". The 9 and 10 cases of T077 only work if a failed swap leaves the stored state unchanged: otherwise
  the re-read after the first failure would already find the wanted state and the tenth failure could never
  be reached. The third case needs the opposite: the stored state must change to the wanted one between the
  failed swap and the re-read, inside one call of the use case. `Add`, `RefuseNameOnNextWrite`,
  `FailNextStateSwaps` and `WriteCount` cannot arrange that, so the case cannot be written with the fake as
  T039 describes it.

### MINOR-2: The Stepper's design rule does not allow the Reduce amount 0, and no design task addresses it

- **Where**: tasks.md T012 (line 131: the editor "using the New goal form's SegmentedControl, Stepper and
  DayPicker"), T082 (line 347: `Stepper.razor` "per their `design/components/` files"), T083 (line 348).
- **Against**: `design/components/Stepper.md` line 6 ("Minimum 0 when recording, 1 for targets"); spec FR-007
  (amount "from 0 to 999 for Reduce"), US3-AS17 ("Reduce, count 0 sweets" is accepted), HP-009 (the form has
  "the FR-007 fields"); constitution IV ("A needed visual change MUST be made in `design/` first").
- **Continues**: new.
- **Reasoning**: The amount of a suggested starting goal is a target. The component guide that T082 follows
  gives targets a minimum of 1, while FR-007 lets a Reduce amount be 0, and US3-AS17 is such a goal. The
  design tasks list what the new admin form page shows but do not mention the amount's range, and no task
  touches `design/components/Stepper`. So T082 and T083 either follow the guide, and the form cannot enter a
  value the spec accepts, or allow 0 and differ from the guide with no design change first. The tasks do not
  say which.

### MINOR-3: Dependency and parallel statements are still inaccurate in five places

- **Where**: tasks.md "Story dependencies" (lines 524–529), "Within each story" (lines 544–546), "Parallel
  opportunities" (lines 557–558), "Notes" (line 617), T005 and T006 (lines 105–106), T076 (lines 331–338),
  T086 (line 351), T089–T091 (lines 375–381), T108 (line 431).
- **Against**: the tasks themselves; the [P] definition (lines 27–28: "different files", "once the tasks it
  names as dependencies are done").
- **Continues**: MINOR-6 of pass 1.
- **Reasoning**:
  - T108's new integration test needs a custom habit ("with a custom habit `"A0 Custom"`"), which exists only
    through US4's port and service (T092–T096). "US5: after US1" (lines 528–529) does not name US4, and line
    558 says "US5 can run beside US4".
  - "US3: after US1 for its tests only: its integration tests browse …" (lines 524–525) names only the
    integration tests. T076, a unit test, also browses (US3-AS1 "browsing does not", US3-AS14 "browsing after
    publish"), and T086 needs the fake that T064 [US1] links into `HT.UiTests`. Line 557 says US1 and US3 "can
    be built side by side", while line 544 has a story's [P] tests written first.
  - T005 and T006 are both [P] in the design group and both edit `design/brand-book.md` (Iconography, and the
    content rule). The definition says [P] tasks touch different files.
  - Line 617 still gives the old definition ("[P] tasks … depend on no unfinished task"), beside the new one
    at lines 27–28, under which T088, T114 and T115 are [P] with dependencies.
  - Line 544 says each [P] test names the port or types it compiles against. T088 names T092; T089, T090 and
    T091, which call `ICustomHabitStore` and `CustomHabitService`, name nothing, and the parallel example for
    US4 (lines 586–588) starts them with no task before them.

### MINOR-4: Two behaviours of FR-012 have no unit or integration test task

- **Where**: tasks.md T076 (lines 331–338), T078 (line 340), T045 (lines 218–225), T081 (line 346).
- **Against**: spec FR-012 ("list all catalog habits in every state, in name order"; "edit every field of
  FR-001 except the state in any state"); contracts/application-api.md line 37 (`Habits` … "name order,
  filtered"); acceptance.md AC-040 ("The administrator's list is in name order") and AC-069 ("A Draft and an
  Unpublished habit can be edited and keep their state"); tasks.md line 15 (unit and integration tests are
  part of done) and CLAUDE.md "Testing".
- **Continues**: new.
- **Reasoning**: T063 and T096 state the order of their lists (`HabitName.Order`) and T058 and T088 test it.
  For the administrator's list, T081 does not mention the order, and T076's list cases check only the state,
  the search and `Total`. No task tests that an edit of a Draft or an Unpublished habit leaves its state as it
  was; T045's update cases check the list rows, and T076 edits a Published habit (US3-AS3). Both behaviours
  are checked only by the acceptance tests, which are written after implementation by a separate workflow.

## Summary

0 MAJOR, 4 MINOR.

Both MAJOR notes of pass 1 are resolved: the Intent filter is now the two checkboxes of `design/`, with the
address rule and tests to match, and T042 uses slice 1's command and makes the migration internal. Eight of
the nine MINOR notes are resolved and one is partly resolved; the author rejected none. The facts the
revision cites were checked against the repository (the slice 1 state blocks in `design/pages/`, the
`FakeAccountStore` link, `MaxSignInSaves`, the roadmap rows and slice note, `Dialog.md`, the `sources` lock,
which matches spec.md and acceptance.md) and hold. Every FR, HP, success criterion and user story still has
tasks, and no existing test was found that the slice breaks without a task for it. The four notes left are a
fake member one test case needs, a design rule of the Stepper that FR-007's Reduce amount 0 does not fit,
remaining inaccuracies in the dependency and [P] statements, and two FR-012 behaviours tested only at the
acceptance level. None blocks `/speckit-implement`.
