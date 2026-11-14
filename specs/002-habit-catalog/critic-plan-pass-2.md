# Critic pass 2: plan for 002-habit-catalog

**Verdict**: APPROVE
**Date**: 2026-11-14
**Reviewed**: `plan.md`, `research.md`, `data-model.md`, `quickstart.md`, `contracts/application-api.md`,
`contracts/host.md` (all in `specs/002-habit-catalog/`), as revised after `critic-plan-pass-1.md`

Checked against: `spec.md`, `acceptance.md`, `tests/HT.AcceptanceTests/Features/F002/habit-catalog.feature`,
the constitution, `CLAUDE.md`, `SPEC.md`, `ARCHITECTURE.md`, `CONTEXT.md`, `docs/roadmap.md`, ADRs 0003 to
0013, `design/`, `specs/001-accounts-and-access/`, `.specify/extensions/ac/scripts/ac_lock.py`, and the code
and tests in `src/` and `tests/` (without `tests/HT.AcceptanceTests` apart from the F002 feature file).
Nothing was built or run. Statements about runtime behaviour of .NET, EF Core and SQL Server that could not
be executed here are not used as the basis of any note.

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 Home preview has no use case | Resolved | `contracts/application-api.md`, `CatalogService`: `GetHomePreviewAsync()` with `Habits` and `PublishedCount`; `research.md` R-07 (own use case, AC-012 and AC-072); `contracts/host.md`, "Home" (the page renders `Habits`); R-13 (unit test with 0, 3, 4 and 5 Published habits). The step "asks for the Home preview" (feature lines 132, 891) now has a member to call. |
| MAJOR-2 Custom-habit name in the log on a lost race; no HP-015 test | Resolved | `research.md` R-14: conditional `INSERT` / `UPDATE` so the unique index is not what refuses a taken name; replacement exception without inner exception; `CoreEventId.SaveChangesFailed` and `RelationalEventId.CommandError` (the two events pass 1 cited) ignored. R-09: the demo account's custom habits use the same insert. R-13: race and demo-rollback tests capture every log entry at `Trace`. `plan.md` row III and V cite both. Residual, narrower point: MINOR-3. |
| MAJOR-3 US4-AS15 not covered for "My custom habits" | Resolved | `contracts/host.md`, route `/custom-habits` with `[Authorize]` and Not found for an administrator; `research.md` R-11; R-13 host tests name US4-AS15 for `/custom-habits`, `/custom-habits/new` and `/custom-habits/{id}`; `quickstart.md` has both checks. The address is a plan position that waits for the owner ("Spec discrepancies" 4); see MINOR-4. |
| MINOR-1 Slice 1 unit `DemoLoadingTests` not acknowledged | Resolved | `research.md` R-13, "Slice 1 unit tests rewritten"; `contracts/application-api.md`, Composition (`DemoDataService` takes `IAccountStore` and `IDemoDataStore`); `plan.md`, Project Structure and Work order 3. The members R-13 names (`CreateCount`, `PasswordOf`, `TakeEmailOnNextCreate`) exist in `tests/HT.UnitTests/Fakes/FakeAccountStore.cs`. A different slice 1 file is the subject of MINOR-2. |
| MINOR-2 Steps 3 and 4 cannot be verified apart | Resolved | `plan.md`, Work order "3. Application and 4. Persistence, verified and committed together", with the reason; Constitution Check row V. |
| MINOR-3 `ToUpperInvariant` is not `OrdinalIgnoreCase`; two foldings | Resolved | `research.md` R-03: one key for uniqueness, order, search and the index; the U+017F difference stated; a unit test pins `ſ` and `S`. `data-model.md`, `HabitName.Matches` uses the key. The rationale given (FR-004 says "culture-invariant"; all four uses agree with each other) is sound. The reading is not among the positions the gates send to the owner. |
| MINOR-4 Who decides publish and unpublish | Resolved | `research.md` R-08 ("the service decides, the store only writes"); `data-model.md`, `TrySetStateAsync(HabitId id, CatalogHabitState expected, CatalogHabitState next)` and `CatalogHabitState`; `contracts/application-api.md`, the publish/unpublish note. The three texts agree. |
| MINOR-5 Use cases in circuits on circuit-scoped stores | Resolved | `research.md` R-15 (`UseCaseScope`, one scope per call); `contracts/host.md` ("Interactive pages call every use case in a scope of its own"); `plan.md`, Project Structure, Work order 5 and Risks. |
| MINOR-6 Two architecture rules did not match the code | Resolved | `research.md` R-12: the `IDemoDataStore` rule scoped to the `HT.Application` assembly; new `UseCaseParameterTests` in `HT.ArchitectureTests`; "Slice 1's `SC_005_…` test stays where it is, in `HT.UnitTests`". |
| MINOR-7 Existing rule does not guard culture-free comparison | Resolved | `research.md` R-12 (culture-sensitive members added to `ClockReadTests`); R-13 (name tests under `tr-TR`, with `Éclair` / `éclair`, where a culture-sensitive call differs). No current Domain or Application code calls the added members (searched `src/HT.Domain`, `src/HT.Application`). The list leaves out the culture-sensitive `StartsWith(string)`, `EndsWith(string)`, `IndexOf(string)` family and default string comparers; the name tests cover order and equality by behaviour. |
| MINOR-8 Names that stretch glossary terms | Resolved | `BuildOrReduce` in `data-model.md`, `research.md` R-05 and both contracts; `MarkedSuggestedStartingGoal`, `SuggestedStartingGoalNotMarked`, `BuildOrReduceMissing`. No "suggestion" for a suggested starting goal remains in the six artifacts (searched). |
| MINOR-9 Design-first step and component list incomplete | Resolved | `plan.md`, Work order 1 (link variant of Tabs), Constitution Check row IV (BackLink in the list), `research.md` R-16 (`search`, `layout-grid`, `plus`, `pencil`, `trash-2`, `chevron-left`; all six files exist in `design/icons/`). One design text the new address touches is not named: MINOR-4. |
| MINOR-10 Plan positions not routed through Decision Discipline | Partly | `plan.md`, "Gates before the next step" (discrepancies 1 to 5 and R-02 go to the owner; `ac_lock.py confirm-spec sources` exists as described, `ac_lock.py` lines 3, 109-111, 127); "Spec discrepancies" 5 added for the missing number of times. Not settled: when and where the R-02 decision is recorded. Continued as MINOR-1. |
| MINOR-11 No source or check for the 14 icon files | Resolved | `research.md` R-16: `lucide-static@0.460.0` (the existing files carry `@license lucide-static v0.460.0 - ISC`), and a test that compares each `HtIcon` drawing with its `design/icons/` file. The seven drawings `HtIcon.razor` has today equal their files' child elements (compared here). |
| MINOR-12 Gaps in `quickstart.md` and `contracts/host.md` | Resolved | `quickstart.md`, the "+ New catalog habit" row now has "Build or Reduce"; `contracts/host.md`, "Page behaviour the tests check", "Home" entry (absent section, "See all N →", registered user's EmptyState). |

No older plan review exists besides pass 1, so nothing else can have regressed.

## Notes

### MINOR-1: When and where the R-02 decision (one `HabitId`) is recorded is stated two ways, and one required record is missing

- **Where**: `plan.md`, "Gates before the next step" 2 ("on acceptance it becomes ADR 0014"); Constitution
  Check, Decision Discipline ("R-02 becomes ADR 0014 when accepted"); Project Structure
  (`docs/adr/0014-…`, "once the owner accepts research R-02"); Work order 7 ("**Docs**: ADR 0014 (R-02, once
  accepted)"), after steps 2 to 6. Gate 1 records only the answers to discrepancies 1 to 5 in the spec's
  `## Clarifications`.
- **Against**: Constitution, Decision Discipline: the answer "is recorded in the spec's `## Clarifications`
  section with its date. A decision that spans features … MUST also become an ADR". `docs/roadmap.md`,
  "Technical rows": "add one dated line to the spec's `## Clarifications` that names the decision and links
  the ADR". Precedent: `specs/001-accounts-and-access/tasks.md` T002 wrote ADR 0010 in Phase 1 and drafted
  its `## Clarifications` line with it.
- **Continues**: pass 1 MINOR-10
- **Reasoning**: The gate says the owner decides R-02 before `/speckit-tasks`, but the only place the work
  order puts the ADR is step 7, after the Domain step (2) has already built `HabitId` and after every layer
  that depends on it. Between the answer and step 7 the cross-slice decision exists only in the plan's gate
  text. For R-02 no `## Clarifications` line is planned at all: gate 1's dated block is for the five
  discrepancies, gate 2 names only the ADR. So `/speckit-tasks` gets two different times for the ADR and no
  source for the spec line the constitution and the roadmap ask for.

### MINOR-2: Two planned tests assume a database state that the shared test databases will not have, and a slice 1 integration test that changes that state is not named

- **Where**: `research.md` R-13: host tests for "the Home preview section (absent with no Published habit,
  HP-012)"; integration tests for `TryCreateSeedCatalogAsync` and "demo loading twice giving the same rows
  (SC-007)"; "Slice 1 tests changed as the spec lists" and "Slice 1 unit tests rewritten" (only
  `tests/HT.UnitTests/Application/Operator/DemoLoadingTests.cs`). `plan.md`, Work order 4 to 6.
- **Against**:
  - `tests/HT.UiTests/Host/HostFixture.cs`: one host per run, the database reset once (line 49), and
    `Operator:DemoPassword` set (line 79); `src/HT.Ui/Program.cs` line 57 runs the startup tasks before the
    host serves requests. With FR-030, that start loads the seed catalog into `HT-UI`.
  - `tests/HT.IntegrationTests/Operator/DemoLoadingTests.cs` lines 54-58: resolves `DemoDataService` from the
    shared fixture and calls `LoadAsync` on `HT-IT`.
  - ADR 0006: "Each test arranges its own data, with values of its own …, instead of resetting between
    tests". `spec.md` FR-011: catalog habits cannot be deleted.
- **Continues**: new (same class as pass 1 MINOR-1)
- **Reasoning**: After this slice the one host of the host tests always has at least 20 Published seed
  habits, so the state "no Published habit" that R-13 assigns to a host test does not occur there; R-13 does
  not say how that test gets it. In `HT-IT`, the slice 1 integration test above will, after this slice,
  also load the seed catalog and the demo custom habits at whatever point of the run it executes, and the
  other planned store tests create catalog habits that no use case can remove. The planned tests of
  `TryCreateSeedCatalogAsync`'s success path and of SC-007 need a known (empty) `CatalogHabits`; "values of
  its own" cannot give that. Neither R-13 nor the spec's list names the integration `DemoLoadingTests` file
  or says how these tests reach their starting state, so the tasks step has no source for it.

### MINOR-3: Some planned checks are narrower than the claims they support

- **Where**: `plan.md`, Constitution Check row II ("Unit tests cover both sides of every FR-005 and FR-007
  boundary") and row III ("integration tests capture every log entry to prove HP-015"). `research.md` R-13:
  "every FR-005 and FR-007 boundary on both sides (60/61, 500/501, Build amount 0/1/999/1000, Reduce
  0/999/1000, unit 20/21, times 0/1/20/21), the FR-006 list order"; "they assert that no log entry … contains
  the custom habit's name or its key". `data-model.md`, `SuggestedStartingGoal`: "Boundaries tested on both
  sides: … Reduce 0 and 999/1000".
- **Against**: `spec.md` FR-007 ("from 0 to 999 for Reduce"), FR-006 (more than 5 is refused), HP-015
  ("custom-habit names or descriptions"). Constitution II: unit tests "MUST cover both sides of each
  boundary". Feature file line 441 (AC-036 row 12, "Reduce, count -1 `sweets`") and lines 415-417 (AC-035,
  5 and 6 items) show these inputs are reachable through the use cases.
- **Continues**: new
- **Reasoning**: The enumerated unit-test values give the Reduce lower bound only its accepted side (0); the
  refused side (-1) is not listed in R-13 or in `data-model.md`, although the input is an `int?` and the
  locked AC use it. The limit of 5 suggested starting goals has no both-sides entry either (R-13 names only
  "the FR-006 list order"). The HP-015 assertion is stated for the name and its key, while the requirement
  also covers descriptions. In each case the claim in the Constitution Check is wider than the check the
  research describes.

### MINOR-4: The position on the address of "My custom habits" leaves out what `design/` already says about tab addresses

- **Where**: `plan.md`, "Spec discrepancies" 4 (position: own address `/custom-habits`) and Work order 1
  ("Also the link variant of `design/components/Tabs`"). `research.md` R-11 (decision and "Alternatives",
  which list no other address for the tab).
- **Against**: `design/components/Tabs.md`: "Tabs switch between views of the same page: catalog vs. my
  custom habits, profile sections, admin queues" and "Each tab should have its own URL (for example
  `/catalog?view=mine`) so links and the back button work." Constitution IV (`design/` is the source of
  truth; pages follow their previews). Constitution, Decision Discipline (a decision that "rejects real
  alternatives").
- **Continues**: pass 1 MAJOR-3 and MINOR-9
- **Reasoning**: The discrepancy is put to the owner as a gap between US4-AS12, US4-AS14 and US4-AS15 only.
  The design component it changes already names an address form for exactly this tab, as a view of the same
  page, and the plan's choice (a second route and page, `CustomHabits`) departs from both statements. That
  design text is neither mentioned in the discrepancy nor listed as an alternative in R-11, and Work order 1
  names only the link markup, so after the design step `Tabs.md` would still give `/catalog?view=mine` as
  the example while the app uses `/custom-habits`. The owner is asked to confirm the position without the
  design evidence that bears on it.

### MINOR-5: Small inconsistencies between the artifacts and with the locked feature file

- **Where / Against**:
  1. `research.md` R-07, Alternatives: "one `HabitService` for all 14 use cases". `plan.md`, Scale/Scope:
     "15 user-facing use cases"; `contracts/application-api.md` lists 4 + 6 + 5 = 15.
  2. `plan.md`, "Spec discrepancies" 3: "The AC items use ASCII only". Feature file line 575 (AC-045) uses
     `A045 Éclair` (stored as U+00C9, bytes `c3 89`).
  3. `plan.md`, Risks, "Serializable seed transaction": on a deadlock "the loser rolls back and reports
     "catalog not empty" … (research R-09)". `research.md` R-09 maps only "a unique-index violation" to
     `false`; it says nothing of a deadlock victim.
  4. `contracts/host.md`, Routes, column Render: `/catalog` and `/custom-habits` are "interactive
     (prerendered)", the other five interactive rows only "interactive". `research.md` R-11 gives
     prerendering to all of them, and R-11 and `plan.md` Risks rest the 404 status of every Not found case
     on prerendering.
- **Continues**: new
- **Reasoning**: (1) is a count left from before `GetHomePreviewAsync` was added. (2) is a factual slip in
  the text the owner is asked to confirm; AC-045's name is far from the length limit, so the conclusion of
  discrepancy 3 is not affected, but the stated reason is not true as written. (3) attributes to R-09 a
  behaviour R-09 does not describe, so what a start reports after a deadlock is open. (4) leaves it unclear
  from the contract alone whether the custom habit page, Admin and the admin form are prerendered, which is
  what their host tests for 404 depend on.

## Summary

0 MAJOR, 5 MINOR.

The three MAJOR notes of pass 1 are resolved: the Home preview is a use case the locked scenarios can call,
custom-habit writes are designed so that no database error carrying a name is produced and a log-capturing
test is planned for HP-015, and "My custom habits" has an address with stated access rules and host tests.
Eleven of the twelve MINOR notes are resolved and one is partly resolved. The artifacts agree with each
other on names, outcomes, routes, tables and counts apart from the points of MINOR-5, and the public API in
`contracts/application-api.md` is sufficient for the steps of the locked feature file. The remaining notes
are about recording the R-02 decision, test starting states in the shared test databases, a few checks that
are narrower than their claims, and the design text behind the tab address. By the plan's own "Gates before
the next step", `/speckit-tasks` still waits for the owner's answers on "Spec discrepancies" 1 to 5 and on
research R-02; those answers were not available to this review.
