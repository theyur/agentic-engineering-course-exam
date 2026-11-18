# Critic pass 1: plan for 004-goals

**Verdict**: REVISE
**Date**: 2026-11-18
**Reviewed**: specs/004-goals/plan.md, specs/004-goals/research.md, specs/004-goals/data-model.md,
specs/004-goals/quickstart.md, specs/004-goals/contracts/application-api.md, specs/004-goals/contracts/host.md,
docs/adr/0017-embedded-time-zone-data.md

## Notes

### MAJOR-1: The optimistic time-zone change misses a goal created while it is computed
- **Where**: research.md R-08 "Changing the time zone" (steps 1 to 4, and "A goal created meanwhile is either seen
  by the re-read or was created with the new zone"); data-model.md `IGoalStore.TryChangeTimeZoneAsync` ("False when
  the account's zone or any goal's concurrency token changed") and `Goals.Version` ("Concurrency token for
  time-zone changes"); R-14 integration race "a goal created against a time-zone change".
- **Against**: spec FR-011 ("A later change of the owner's time zone adds a version (FR-026)"), FR-026 ("Each of the
  account's goals follows on its own timeline"); plan Summary and R-08 Rationale ("every race the spec names is
  closed in one statement or one transaction").
- **Reasoning**: the write is conditioned only on (a) the account's stored zone and (b) the `rowversion` of the goals
  Application read in step 1. Take this order: the change reads the account and its goals; a goal creation then
  locks the account row, reads the old zone, inserts the goal with version 1 in the old zone and commits. It
  does not write the account row, because R-08 step 3 writes it only when it is `NULL`. The change transaction
  then finds the stored zone unchanged and every token it read unchanged. The new goal is not in its read set,
  so nothing is detected and the change commits. The account is now in the new zone, but the new goal keeps the
  old zone with no later version, for good. So the claim "either seen by the re-read or was created with the new
  zone" is false for every account whose zone is not `NULL`. Separately, a time-zone change writes
  `GoalSettingsVersions` rows, not the `Goals` row, so the `Goals.Version` rowversion does not change unless the
  store also updates the `Goals` row, which no artifact says. The token as described therefore detects nothing.
  The same gap affects the start-date check. `GoalService` computes "today" from a zone it reads before the
  store's locked read, while version 1 takes the zone the store reads under the lock (R-08 "The zone the store
  returns is the one written into version 1"). If a change commits in between, the start date was checked
  against a different zone than the goal's own.

### MINOR-1: Application has to use `NamedTimeZone` conversions that the data model makes internal, and `InternalsVisibleTo` is forbidden
- **Where**: data-model.md `NamedTimeZone`: "Internal: `StartOfDay(DateOnly)`, `DateAt(DateTimeOffset)`,
  `LocalTimeOf(DateTimeOffset)`". research.md R-11 says the greeting hour comes "through `Domain`
  `NamedTimeZone.LocalTimeOf(instant)`" in `HT.Application`.
- **Against**: `tests/HT.ArchitectureTests/AcSurfaceTests.cs` (no `InternalsVisibleTo` in HT.Domain, HT.Application
  or HT.Persistence; the AC export refuses it, ADR 0010). Application also needs "today" in a zone: FR-007 and
  FR-012 (starting values), FR-029 (greeting and date), FR-030 ("member since" month in the account's zone) and
  R-10 ("the day of the run"). No public Domain member in data-model.md provides it: `GoalTimeline` has no
  "today" function.
- **Reasoning**: as designed, `ProfileService`, `GoalService` and `DemoDataService` would call internal Domain
  members, and the only mechanism that allows that is banned by an existing test. The artifacts contradict each
  other on where the zone-to-date conversion is reachable.

### MINOR-2: Existing tests that break are missing from the work order and the changed-tests list
- **Where**: plan.md Work order steps 3 and 4 ("verified and committed together"; the only failure named is
  `ValidateOnBuild`), step 5 (smoke check check 5), step 6 (UI tests listed); research.md R-10 "SmokeCheck".
- **Against**: `tests/HT.UiTests/Components/Pages/PageServices.cs` registers `CatalogService`,
  `CatalogAdministrationService` and `CustomHabitService` over fake stores only. Once R-07 gives their
  constructors `IGoalStore`, every bUnit page test that uses `AddCatalog` / `AddCustomHabits` fails to resolve them
  until a fake `IGoalStore` is registered. That breaks constitution V's full-suite run at steps 3 and 4.
  `tests/HT.UiTests/SmokeChecks/RunnerBehaviourTests.cs` `GoodSite()` scripts only `GET /custom-habits` for check
  5. The new `GET /goals` and `GET /profile` reads of check 5 have no scripted response, so the good-site tests
  fail. `specs/003-demo-hosting/contracts/smoke-check.md` "Coverage rule" holds the coverage table that the two
  new entries extend.
- **Reasoning**: none of these appears in the plan, the spec's "Slice 1 and slice 2 requirements and tests this
  slice changes", or host.md. The work order would produce a red suite at the "verified together" step, or
  leave the slice 3 contract stale.

### MINOR-3: The default time zone for administrator and demo accounts has no path through the listed constructors and ports
- **Where**: research.md R-07 "Constructors change" (`OperatorService` and `DemoDataService` gain only
  `TimeProvider`; only `AccountService` gains `InstallationTimeZone`); data-model.md `IAccountStore.CreateAsync`
  ("The old overload stays, for compatibility, and writes no name, the default zone and no creation time");
  R-04 ("Every account created from this slice on … stores a concrete named time zone. `NULL` exists only on
  accounts created before the migration").
- **Against**: spec FR-025 ("the configured administrator and the demo accounts have the installation's default
  time zone"), FR-030 ("Each account created from this slice on MUST record when it was created"); R-07's own
  rule that ports may change because only `HT.Persistence` implements them.
- **Reasoning**: `OperatorService` (see `src/HT.Application/Operator/OperatorService.cs`) and `DemoDataService` need
  the default zone. One gives it to the administrator, the other to `DemoAccountProfile.TimeZone` and "the day of
  the run" (R-10). Neither gets `InstallationTimeZone`. The kept old `IAccountStore` overload would have to
  know "the default zone" inside Persistence, which never receives it. It would also write no creation time,
  which breaks FR-030 for any caller that still uses it. R-07 says ports may change, so the reason for keeping
  that overload does not hold.

### MINOR-4: Demo goal start dates leave a time-zone calculation to Persistence
- **Where**: data-model.md `DemoGoal` (`int StartOffsetDays`), `DemoAccountProfile` (`DateTimeOffset CreatedAt`,
  `NamedTimeZone TimeZone`), `IDemoDataStore.CreateDemoAccountAsync(…, DemoAccountProfile profile,
  IReadOnlyList<DemoGoal> goals, …)`; research.md R-10 ("'The day of the run' is today in the installation's
  default time zone at the supplied time"; the store "writes the goals whose habit qualifies").
- **Against**: CLAUDE.md layer rules ("`HT.Persistence` implements contracts defined inward; no business rules
  or workflow decisions"); ADR 0017 / R-01 (NodaTime stays inside `HT.Domain`, and the zone conversions are
  internal, as MINOR-1 notes).
- **Reasoning**: the port carries an offset in days and an instant plus a zone, but no start date. To write
  `Goals.StartDate`, version 1's `AppliesFrom` and attempt 1's `StartDate`, the store would have to turn the
  instant into a local date in the zone and add the offset. That is exactly the time rule the plan keeps in
  Domain. No artifact says which layer resolves the date.

### MINOR-5: The demo store reads the new account's custom habits, against ADR 0013's "no read of private data"
- **Where**: research.md R-10 "The port" ("each custom habit by the account's own name key").
- **Against**: ADR 0013 Decision ("It has no read of private data"); spec HP-018 ("it still reads no private data
  (ADR 0013)"), which widens the reads only to "the state of the catalog habits that demo goals name".
- **Reasoning**: looking up custom habits by name is a read of private data, even inside the creating
  transaction. HP-018's allowance covers public catalog data only. The deviation is not acknowledged.

### MINOR-6: The Register page's time-zone script is not placed so that it runs after enhanced navigation
- **Where**: research.md R-12 "Register" (a hidden `timeZone` input "filled by a small script,
  `wwwroot/js/time-zone.js` … before submit"); host.md "Register"; plan.md Technical Context.
- **Against**: spec HP-016; `src/HT.Ui/Components/App.razor`, which loads only `_framework/blazor.web.js`, so
  Blazor's enhanced navigation is on by default for in-app links such as TopNav's "Register". Script tags inside
  page content that arrives by enhanced navigation are not executed.
- **Reasoning**: the artifacts do not say where the script is loaded or how it hooks the form. If it sits on the
  Register page, the usual path (arriving from a link) registers every user with the default zone, silently.
  R-14 and the quickstart already mark HP-016 as unverified by automated tests, so nothing would catch it.

### MINOR-7: The R-01 fallback does not meet HP-015 and its list cannot be built as described
- **Where**: research.md R-01 Alternatives ("the list would be the identifiers that
  `TimeZoneInfo.TryConvertIanaIdToWindowsId` accepts, links included"); plan.md Gates item 2 ("The fallback is in
  R-01"); ADR 0017 Options 2.
- **Against**: spec HP-015 (the list holds "UTC" and the named time zones "as zones (not links)"); R-13, which
  forbids `TimeZoneInfo.FindSystemTimeZoneById` and the `ConvertTime*` members in Domain and Application.
- **Reasoning**: the owner is asked to choose between ADR 0017 and a fallback. That fallback would break HP-015,
  because it includes links. It also has no source to enumerate: `TryConvertIanaIdToWindowsId` checks one given
  identifier and does not list identifiers. And it conflicts with the plan's own R-13 rules. The gate does not
  tell the owner any of this.

### MINOR-8: MobileNav's "New goal" entry is dropped
- **Where**: contracts/host.md "Navigation": "MobileNav lists Goals."
- **Against**: spec HP-009 ("TopNav and MobileNav MUST add 'Goals' and '+ New goal'"); `design/components/MobileNav.md`
  ("'+ New goal' in the bar is hidden on phones (`ht-hide-sm`); it moves into the menu"), and
  `MobileNav.html`, whose menu has "New goal" after the divider.
- **Reasoning**: with the contract as written, phone users get no way to "+ New goal". The UI tests that follow
  the contract would pin that omission.

### MINOR-9: The New goal page's ChoiceCard component is not planned
- **Where**: plan.md Constitution Check IV (the component list: GoalListItem, KeyValue, Notice, Dialog,
  SearchField, SegmentedControl, Stepper, DayPicker); Project Structure `Components/Shared/` ("+ GoalListItem,
  KeyValue").
- **Against**: constitution IV ("Each design component MUST be implemented once as a Razor component … Pages MUST
  follow the structure … of their previews"); `design/pages/PageNewGoal.md` ("measure (ChoiceCard: Yes/no, Count,
  Duration) … When (ChoiceCard fixed days …)"); `design/pages/PageNewGoal.html` uses `ht-choice`;
  `src/HT.Ui/Components/Shared/` has no ChoiceCard component. The slice 2 editors use SegmentedControl for these
  fields.
- **Reasoning**: the constitution row claims "met" with a component list that leaves out the one new design
  component the New goal form needs. Reusing slice 2's SegmentedControl would not follow the preview.

### MINOR-10: ADR 0016 is to become Accepted while its Consequences contradict R-04
- **Where**: plan.md Gates item 1 and Work order step 7 ("ADR 0016 → Accepted"); research.md R-04 (a nullable
  `TimeZone` that means "the default").
- **Against**: `docs/adr/0016-time-zones.md` Consequences ("a migration gives existing accounts the default time
  zone").
- **Reasoning**: if the owner accepts the plan's position on "Spec discrepancies" 1, the ADR is accepted with a
  consequence that the implementation does not follow. No artifact plans to amend that sentence.

### MINOR-11: Types and test resources that are referenced but never defined
- **Where**: data-model.md `IGoalStore.TryChangeTimeZoneAsync(…, IReadOnlyList<GoalVersionWrite> writes, …)`,
  `CreateAsync(…) → CreateGoalOutcome`, `IAccountStore.CreateAsync(…, NewAccountProfile profile, …)`,
  `GoalTimeline.Calendar(…)` "(or `CalendarProblem.RangeOutOfBounds`)"; contracts/application-api.md `CreateAsync`
  "the 12 target outcomes (as `GoalProblem`)"; research.md R-14 "the migration test of HP-019, on
  `HT-UI-MIGRATE`'s pattern" in `HT.IntegrationTests/Goals`.
- **Against**: CLAUDE.md "Build and test" (the derived databases are named: `HT-UI-SMOKE`, `HT-UI-KEYS`; ADR 0006).
  `HT-UI-MIGRATE` belongs to `tests/HT.UiTests/Operator/StartupTasksHarness.cs`, not to the integration tests.
- **Reasoning**: `GoalVersionWrite`, `NewAccountProfile`, `CalendarProblem` and `CreateGoalOutcome` have no
  members anywhere. It is unclear whether `CreateAsync`'s `Outcome` is `GoalProblem` (which has no
  `NotSignedIn`, `NotForAdministrators`, `NotInCatalog` or `CustomHabitNotFound`) or a separate enum. The HP-019
  test names no database, although it must migrate a database to `HabitCatalog` first and cannot share `HT-IT`.
  `/speckit-tasks` would have to invent each of these.

## Summary

1 MAJOR, 11 MINOR. The plan follows the spec closely and checks out on these points:

- Scenario counts: 94 in F004, 73 in F002, 63 in F001.
- The seed catalog: every Reduce count or duration suggestion is 1 time per day.
- The FR-006 problem order matches slice 2's `SuggestedStartingGoalProblem`.
- The compatibility approach for F001 and F002: overloads, `init` properties and appended enum members.
- The timeline rules: they reproduce AC-071 to AC-083.

The blocking issue is the concurrency design for a time-zone change. As written, it loses a goal created while
the change runs, and that goal keeps an outdated zone for good, although the plan says this race is closed. The
minor notes are mostly contradictions to settle before `/speckit-tasks`:

- the visibility of the zone-to-date conversions (MINOR-1);
- which services and ports carry the installation's default time zone (MINOR-3, MINOR-4);
- existing tests and contracts that the work order would break or leave stale (MINOR-2);
- an R-01 fallback the owner should know does not meet HP-015 (MINOR-7).
