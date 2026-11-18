# Critic pass 2: tasks for 004-goals

**Verdict**: APPROVE
**Date**: 2026-11-18
**Reviewed**: specs/004-goals/tasks.md (against spec.md, plan.md, research.md, data-model.md,
contracts/application-api.md, contracts/host.md, the constitution, specs/003-demo-hosting/contracts/smoke-check.md,
tools/HT.SmokeCheck, design/, and the current code and tests in src/ and tests/)

## Previous notes: resolution status
| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 (meta line has no schedule source) | Resolved | T026 adds `GoalText.ScheduleText`, using the same day grouping as `SuggestedStartingGoalText.Days` (a run of three or more as "first–last"). T039 puts `ScheduleText` on `GoalListItem` and `GoalDetails` and records it in data-model.md and the contract. T069 and T070 use it, and T031, T064 and T065 test it. |
| MAJOR-2 (US9 smoke check fails before the US11 Profile name) | Resolved | US11 is now Phase 12 and US9 Phase 13. T131 depends on T120. T126 names `SmokeCheckHostTests`. "Story dependencies" and "Parallel opportunities" say US9 waits for US11. `DeleteOneCustomHabitAsync` deletes one of demo2's habits, "Evening stretch" or "Plan tomorrow", and T122 says no demo goal uses either, so that host case still fails only its own check. |
| MINOR-1 (bUnit page tests broken by new dependencies) | Partly | T040 now names the page tests and has `AddCatalog` and `AddCustomHabits` call `AddGoals`. As specified, though, the wiring still leaves the Home tests unresolvable. See MINOR-1. |
| MINOR-2 (grep convention misses target-typed instances) | Partly | The convention now greps for the type name. But the literal command also searches `tests/HT.AcceptanceTests`, and T091 still omits `CatalogHabitStateTests.cs`. See MINOR-3. |
| MINOR-3 ("tests first" contradicts dependencies) | Resolved | "Within each story" now says a test task starts after the use case it calls, and that "Nothing here requires a test to be written before its use case". The constitution has no test-first clause. |
| MINOR-4 (ClockReadTests canary and `BclDateTimeZone`) | Partly | T035 now has the canary call each new entry, says where NodaTime comes from, and adds a separate rule for `BclDateTimeZone`. One canary call does not compile under `TreatWarningsAsErrors`. See MINOR-4. |
| MINOR-5 (host tests that cannot observe the behaviour) | Resolved | T087 uses bUnit's `NavigationManager` and drops the host test. T092 defines the visitor and administrator behaviour for `tracked=true` and records it in contracts/host.md. T093 asserts it. |
| MINOR-6 (baseline in the scratchpad) | Resolved | T002 has a fallback that regenerates the baseline in a temporary worktree. A small residual risk is in MINOR-8. |
| MINOR-7 (`GoalListItem` name collision) | Resolved | T069 names the component `GoalItem` and gives the CS0104 reason. T131 reads `ht-goalitem__name`, which matches `design/components/GoalListItem.html` and `ht-components.css` line 348. |
| MINOR-8 (texts with two homes) | Resolved | T036 now defines `NameTooLong` once in `AccountTexts`, matching contract "Texts" ("`NameTooLong` (goal, account)"). `MemberSince` is built on `DateTexts.MonthYear` with no month names of its own. |
| MINOR-9 (T055 covered Draft and Unpublished seed habits) | Resolved | T055 now covers only Published habits and names "Music practice" and "Coffee" as excluded, matching `SeedCatalog.cs` lines 109-112. |
| MINOR-10 (US4-AS8 creation, FR-013 overlap) | Resolved | T054 adds creation at 23:30 and the refusal at 00:30. T073 adds a Wednesday-to-Tuesday range that returns both whole weeks. |
| MINOR-11 (no "14 Jul 2026" format) | Resolved | T037 adds `DateTexts.DayMonthYear` and records it in research R-11. T070 uses it. |
| MINOR-12 (missing acting-user account) | Partly | T058 and T103 now define the outcome as `NotSignedIn`, and T053 and T100 test it. The test fixtures that now hit this rule are not adjusted, and the rule is not recorded in the contract. See MINOR-2. |
| MINOR-13 (time-zone writes not owner-scoped) | Resolved | T102 requires every version row to belong to a goal of `owner`, with a rollback otherwise. T101 tests a mixed write. |
| MINOR-14 (nothing keeps NodaTime out of Application) | Resolved | T035 adds the `ApplicationDependencyTests` rule against any `NodaTime` namespace. |
| MINOR-15 (smoke-check contract row, page reading) | Resolved | T131 adds `PageReader.ReadGoalNames` and uses the existing `ReadFirstHeading` for `/profile`. The `h1` is the first heading in `Profile.razor` line 28. T131 also extends check 5's contract row, and T126 covers `PageReaderTests`. A new ambiguity about counts is in MINOR-7. |
| MINOR-16 (small inaccuracies) | Partly | T124 now reads `IsInCatalog` through `GetAsync`, and T127 creates `DemoAccounts`. T050's rewritten sentence makes a new factual error. See MINOR-5. |

No older note has regressed. The renumbered cross-references point at the intended tasks:

- T040 → T111, T119;
- T049 → T107, T114;
- T058 → T103, T116;
- T106 → T136;
- T120 → T131;
- T122–T126 → T127–T131;
- T131 → T069, T120;
- every entry in "Shared files".

## Notes

### MINOR-1: `AddGoals` as specified cannot give the Home tests a resolvable `GoalService`
- **Where**: tasks.md T040 ("`AddGoals(...)` ... With `TryAdd`, so that calling it twice is harmless ... `AddCatalog` and `AddCustomHabits` call it"); T058 (`GoalService` takes `ICustomHabitStore`; added to `AddGoals`); T111 (Home injects `GoalService`); T084 (`FakeCustomHabitStore` gets "a `FakeGoalStore` reference").
- **Against**: `tests/HT.UiTests/Components/Pages/PageServices.cs` (`AddCatalog` registers no `ICustomHabitStore`); `HomeVisitorTests.cs` line 22 and `HomeSignedInTests.cs` line 21, which call only `AddCatalog`; `CustomHabitPageTests.cs` lines 24-25 and `CatalogPageTests.cs` lines 21-26, which call `AddCatalog` before `AddCustomHabits`; `src/HT.Ui/Components/Pages/Home.razor` line 5 (an `@inject`, resolved for every audience).
- **Continues**: MINOR-1.
- **Reasoning**: `AddCatalog` calls `AddGoals` first. At that point no custom-habit store exists, so the `FakeGoalStore` it registers cannot be wired to one. The later call from `AddCustomHabits` is a no-op by design (`TryAdd`). Two consequences follow:
  - In the Home tests, which never call `AddCustomHabits`, `GoalService` has no `ICustomHabitStore` to resolve. Once T111 injects `GoalService` into Home, both Home test classes fail at DI resolution, the visitor ones included. This is the breakage T040 says it prevents.
  - T040 has `FakeGoalStore` read `FakeCustomHabitStore` (for `HabitGone`), and T084 has `FakeCustomHabitStore` hold a `FakeGoalStore` (for `UsedByGoals`). Neither task says how the two fakes get each other without a construction cycle, in the unit tests or in `PageServices`.

### MINOR-2: The new missing-account rule meets fixtures whose acting users have no account
- **Where**: tasks.md T058 (an acting user whose account is not found gets "You are not signed in."), T103, T119; T040 (only `GoalInputs`' acting users are said to exist in `FakeAccountStore`); T062, T071, T109, T121 (bUnit tests of New goal, Goals, Home); T081, T088, T094 (habit unit tests that arrange goals); Notes ("A goal or profile use case whose acting user has no stored account ...").
- **Against**: `PageServices.SignIn` (`var id = Guid.NewGuid()`, with no account added to any store); `tests/HT.UnitTests/Application/Habits/CatalogInputs.cs` lines 10-12 and `CustomHabitServiceTests.cs` lines 12-14 (acting users with random identifiers); contracts/application-api.md "Texts" and the outcome lists, which do not mention this case.
- **Continues**: MINOR-12.
- **Reasoning**: every bUnit page test signs in with a fresh identifier that no fake account store holds. Under T058 and T103, `StartNewGoalAsync`, `ListAsync` and `GetHomeHeadingAsync` all answer `NotSignedIn` for such a user. So T062's New goal page, T071's Goals page, T109's EmptyState and T121's greeting cannot show what those tests expect unless the sign-in also creates the account in the fake store. No task says so; T040 covers only `GoalInputs`. The same applies to habit unit tests that create goals through `GoalService` with the `CatalogInputs` or `CustomHabitServiceTests` users. The rule itself is listed in Notes, but unlike the other additions it is not given a task to record it in the contract.

### MINOR-3: The grep command searches the off-limits acceptance-test folders, and one hand-built `CatalogService` is not named
- **Where**: tasks.md "Conventions for every task" (`grep -rnw "<Service>" tests --include=*.cs`, never searching `tests/HT.AcceptanceTests`); T091 (its list of hand-built `CatalogService` instances); "Shared files".
- **Against**: CLAUDE.md "Acceptance tests" (never read, search or list `StepDefinitions/` and `Support/` of the AC project); `tests/HT.UnitTests/Application/Habits/CatalogHabitStateTests.cs` lines 16 and 42 (`new CatalogService(_store)`).
- **Continues**: MINOR-2.
- **Reasoning**: `grep -rnw ... tests` recurses into `tests/HT.AcceptanceTests`, including the forbidden folders. The prose next to the command forbids that, but the command itself does not exclude the folder (for example with `--exclude-dir`). An agent that runs it as written breaks a binding project rule. Separately, T091 lists four files but not `CatalogHabitStateTests.cs`, which builds `CatalogService` directly. T097 also edits that file, and it is missing from the "Shared files" list, unlike `CatalogHabitFieldTests.cs`.

### MINOR-4: The canary's call of `DateTimeZoneProviders.Serialization` raises an obsolete warning that fails the build
- **Where**: tasks.md T035 (`NodaTime.DateTimeZoneProviders::get_Serialization(` added to `ForbiddenMembers`; "the `Canary` calls each new entry once").
- **Against**: NodaTime 3.2.x API (nodatime.org/3.2.x/api/NodaTime.DateTimeZoneProviders.html): `Serialization` carries `[Obsolete("This property exists primarily for binary backward compatibility. ...")]`; CLAUDE.md (`TreatWarningsAsErrors` is on); `ClockReadTests.cs` `Canary`, whose `#pragma warning disable` lists only CA1304, CA1305, CA1307, CA1309, CA1310 and CA1311.
- **Continues**: MINOR-4.
- **Reasoning**: calling the obsolete property in the canary produces CS0618, and with warnings as errors the architecture test project does not compile. CLAUDE.md says "fix it, don't suppress it". The canary needs this exact call, though, so the task has to say whether CS0618 joins the canary's suppression list or the entry is handled another way. As written, T035 cannot be done without an unstated decision.

### MINOR-5: T050 claims T133 would catch a public `GoalStore`, but T133 never looks at `HT.Persistence`
- **Where**: tasks.md T050 ("T133's API comparison would show it if it became public"); T002 and T133 (baselines and comparison of `HT.Application` and `HT.Domain` only).
- **Against**: T002 runs `roslynator list-symbols ... --projects HT.Application` "and the same for `HT.Domain`", and T133 compares only those two projects.
- **Continues**: MINOR-16.
- **Reasoning**: `GoalStore` lives in `HT.Persistence`, which neither baseline covers, so nothing in the tasks would show it becoming public. The sentence replaced the earlier inaccurate claim with another one, and it suggests a safeguard that does not exist.

### MINOR-6: `ScheduleText` gives "Flexible · once a day" where the full preview shows "Every day" for the same goal
- **Where**: tasks.md T026 ("N times per period: 'Flexible · ' and 'once a day' ..."); T006 (the slice-4 meta line lists "Flexible · once a day"); T017 (its grep list of wrong texts).
- **Against**: `design/pages/PageGoals.html` line 26 ("Coffee, at most 2 cups" with "Every day · 1 of 2 today"); US1-AS6 and T054 (Coffee is Reduce, Count, at most 2 "cups", 1 time per day, which is a times-per-period schedule); tasks.md "Design first" (the full previews "stay whole, because they are the design source of slices 5 to 11").
- **Continues**: new.
- **Reasoning**: under T026's rule, the Coffee goal's schedule is "Flexible · once a day". The full preview, which later slices build from, still shows "Every day" for it. T006 fixes only that goal's name ("at most 2 cups a day"), and T017's list of texts to fix does not include this schedule text. After slice 4, the slice-4 state and the full preview give the same goal two different schedule texts. Slice 5 or 6 then has to pick one, and the design source does not say which.

### MINOR-7: Check 5's `(<found> of <expected>)` count is undefined once the check covers three kinds of name
- **Where**: tasks.md T131 ("Each is still one `PASS`/`FAIL` line per account, with the counts of the contract's Output rules").
- **Against**: `specs/003-demo-hosting/contracts/smoke-check.md` "Output" (lines 77-78: `PASS <name> (<found> of <expected>)`, where `<found>` counts the expected names present on the page); `SmokeCheckRunner.NamesLine`, which counts one list against one page.
- **Continues**: new.
- **Reasoning**: check 5 now compares custom-habit names on `/custom-habits`, goal names on `/goals`, and demo1's name on `/profile`. The Output rule defines the count for one list on one page. T131 does not say whether the line counts all three together, only custom habits, or something else. It also does not say which reason a `FAIL` gives when only the name or only a goal is missing. T126's failing cases and the contract row T131 extends both depend on that choice.

### MINOR-8: Small inaccuracies
- **Where / Against / Reasoning**:
  - T002's fallback regenerates the baseline "from `master`". If `master` gains public API from another merge while this slice is in progress, the branch lacks those members. T133 then reports them as removals, which are defects under its rule ("Report any other difference as a defect"). The branch's merge base with `master` is the state the original baseline captured. Continues MINOR-6.
  - T036's `MemberSince` builds on `DateTexts` ("T037 comes first"), but T036 declares no dependency on T037. "Parallel opportunities" ("T037, T038 and T039, then T040 and T041") does not place T036 at all, and T037 is marked [P].

## Summary

0 MAJOR, 8 MINOR. Previous notes: 13 Resolved, 5 Partly, 0 Unresolved.

Both MAJOR notes of pass 1 are resolved, and none of the 18 has regressed:

- The schedule text now has one tested formatter and a field on both views.
- US9 runs after US11, so the in-process smoke-check test can stay green at the US9 checkpoint.
- The renumbering is consistent throughout.

The remaining notes are about test plumbing and precision, not requirement coverage. Three of them will surface at the first test run:

- `AddGoals` cannot give the Home tests a `GoalService`.
- bUnit's sign-in helper creates acting users with no account, which the new missing-account rule refuses.
- The NodaTime canary hits an obsolete warning.

The others need a sentence each: the grep command reaches the off-limits acceptance-test folders, T050 overstates T133's reach, the schedule text disagrees with the full Goals preview, and check 5's count is undefined. The task list is ready for `/speckit-implement` once the author has decided how to handle these.
