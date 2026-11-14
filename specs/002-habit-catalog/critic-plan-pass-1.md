# Critic pass 1: plan for 002-habit-catalog

**Verdict**: REVISE
**Date**: 2026-11-14
**Reviewed**: `plan.md`, `research.md`, `data-model.md`, `quickstart.md`, `contracts/application-api.md`,
`contracts/host.md` (all in `specs/002-habit-catalog/`)

Checked against: `spec.md`, `acceptance.md`, `tests/HT.AcceptanceTests/Features/F002/habit-catalog.feature`,
the constitution, `CLAUDE.md`, `SPEC.md`, `ARCHITECTURE.md`, `CONTEXT.md`, `docs/roadmap.md`, ADRs 0003 to
0013, `design/`, `specs/001-accounts-and-access/`, and the code and tests in `src/` and `tests/` (without
`tests/HT.AcceptanceTests` apart from the F002 feature file). Nothing was built or run; two read-only checks
were made on this machine and are named where they are used.

## Notes

### MAJOR-1: The Home preview has no Application use case, so FR-008's rule lives in the page and AC-012 / AC-072 have nothing to call

- **Where**: `contracts/application-api.md`, "`CatalogService`", lines 25-26: "The Home preview is the first
  four habits of `BrowseAsync(new CatalogFilter())` (FR-008); there is no separate method."
  `contracts/host.md`, route `/` (HP-012).
- **Against**:
  - `spec.md` FR-008 (a functional requirement "on the AC surface"): "The Home preview is the first four
    habits of this result with no filters, fewer when fewer exist."
  - `spec.md`, "What acceptance tests can reach": "**FR-NNN** are application behaviours, reachable through
    Application use cases, so acceptance tests can verify them."
  - Locked `acceptance.md` AC-012 and AC-072, and the feature file lines 132 and 891: "When the anonymous
    visitor asks for the Home preview" / "Then it is exactly …". `acceptance.md`'s conventions define
    "browsing" and "the administrator's list" as use cases, and give no other meaning to "asks for the Home
    preview".
  - ADR 0003: "step definitions arrange and assert only through Application use cases".
  - `CLAUDE.md` ("No business rules in Blazor components") and `ARCHITECTURE.md` §3 ("Keep business rules
    out of Blazor components").
- **Reasoning**: With no Application member for the Home preview, the rule "first four, fewer when fewer
  exist" (an owner decision, spec Clarifications) is implemented only in `Home.razor`, which is outside the
  AC surface. The step definition for "asks for the Home preview" has no use case to call; it can only take
  the first four of the browse result itself, so AC-012 and AC-072 would assert the test's own cut, not the
  product's. The part of FR-008 that the spec places on the AC surface is then verified by no acceptance
  test, and a product rule sits in a Blazor component. The contract states the absence as a choice but
  gives no reason against the spec's FR / HP split.

### MAJOR-2: A lost uniqueness race writes the custom-habit name into the application log (HP-015, constitution III); no test for HP-015 is planned

- **Where**: `research.md` R-03 ("A unique-index violation (SQL error 2601 or 2627) is reported as the name
  being taken (HP-018)"; unique index on `CustomHabits (OwnerId, NameKey)`, `NameKey` the upper-cased name);
  R-08 (custom-habit edits as `ExecuteUpdateAsync`); R-09 (`CreateDemoAccountAsync`); R-13 (integration
  tests: "parallel creates and renames with one name (HP-018) for both kinds", "rolling back completely on a
  forced unique-index violation (a duplicate name in the given list)"). `plan.md`, Constitution Check row
  III: "No custom-habit name in logs or demo results (HP-015, HP-017)"; Technical Context, Constraints.
- **Against**: `spec.md` HP-015: "Logs, exception messages and error pages MUST NOT contain custom-habit
  names or descriptions (constitution III)". Constitution III, last bullet.
- **Reasoning**:
  - SQL Server's own texts for the two errors the plan relies on carry the key value. Read on the local
    instance from `sys.messages` (language 1033): 2601 "Cannot insert duplicate key row in object '%.*ls'
    with unique index '%.*ls'. The duplicate key value is %ls."; 2627 "Violation of %ls constraint '%.*ls'.
    Cannot insert duplicate key in object '%.*ls'. The duplicate key value is %ls." For
    `IX_CustomHabits_OwnerId_NameKey` that value is the owner's identifier and the upper-cased custom-habit
    name.
  - EF Core logs the failure before the store's `catch` sees it. The EF Core 10.0.15 assembly in the
    package cache holds the template "An exception occurred in the database while saving changes for
    context type '{contextType}'.{newline}{error}" (the error text is part of the message), and the
    relational assembly "Failed executing DbCommand".
  - `src/HT.Ui/appsettings.json` sets `Default: Information` and lowers only `Microsoft.AspNetCore`, so
    these Error entries are written.
  - So the path the plan designs for HP-018 (two creates or renames of one name by the same owner, for
    example from two tabs), and the forced violation in `CreateDemoAccountAsync`, put a custom-habit name
    into the log. `data-model.md` (last paragraph) covers only the plan's own `InvalidOperationException`
    messages.
  - R-13 lists tests for HP-006, HP-010, HP-017 and HP-018, and none for HP-015, so the Constitution Check
    claim is asserted, not verified (constitution V).

### MAJOR-3: US4-AS15 is not covered for the address of "My custom habits"

- **Where**: `plan.md`, "Spec discrepancies" 4 ("My custom habits" is `/catalog?tab=custom`).
  `contracts/host.md`, Routes: `/catalog` is for "anyone", with "`tab=custom` (registered users)" and the
  requirements "HP-001 to HP-004, US1-AS10, US1-AS11, US4-AS12"; "an unknown value is ignored"; US4-AS15 is
  cited only on the two `/custom-habits/…` rows. `research.md` R-11: Not found is called for "an
  administrator on a custom-habit page".
- **Against**: `spec.md` US4-AS15: "**Given** the address of the custom habit page or of "My custom
  habits", **When** an anonymous visitor opens it, **Then** they are sent to Sign in; **when** an
  administrator opens it … **then** the Not found page is shown." HP-007: "Its addresses follow US4-AS15."
- **Reasoning**: The plan fixes the address of "My custom habits" as `/catalog?tab=custom`, but the
  contract says nothing about that address for an anonymous visitor or an administrator. `/catalog` has no
  `[Authorize]`, so nothing sends a visitor to Sign in, and by the contract's other rules (the page is for
  anyone, the administrator variant has no Tabs, HP-002) both audiences would get the public catalog. That
  is not what US4-AS15 requires. R-13 names US4-AS15 among the host tests, but the contract gives those
  tests no expected result for this address.

### MINOR-1: Slice 1 tests that the change to demo loading breaks are not acknowledged

- **Where**: `research.md` R-09 (account creation moves to `IDemoDataStore.CreateDemoAccountAsync`); R-13
  ("Slice 1 tests changed as the spec lists"); `plan.md`, Work order 5 and 6.
- **Against**: `tests/HT.UnitTests/Application/Operator/DemoLoadingTests.cs`: `new DemoDataService(_store)`
  with a `FakeAccountStore`; asserts `_store.CreateCount`, `_store.Accounts`, `_store.PasswordOf`,
  `_store.TakeEmailOnNextCreate()` and signs the created accounts in through `AccountService` over the
  same fake. `spec.md`, "Slice 1 requirements and tests this slice changes", lists only `HT.UiTests` files.
- **Reasoning**: With R-09 the constructor changes and accounts are no longer created through
  `IAccountStore.CreateAsync`, so this test class stops compiling, and its sign-in and lost-race cases need
  the new `FakeDemoDataStore` to share accounts with `FakeAccountStore`. Neither the plan nor the spec's
  list names this file or that coupling between the two fakes, so `/speckit-tasks` has no source for it.

### MINOR-2: The work order leaves the integration and host tests failing between steps 3 and 4

- **Where**: `plan.md`, Work order 3 (services, `DemoDataService` extended) and 4 (stores registered by
  `AddHtPersistence`).
- **Against**: `tests/HT.IntegrationTests/DatabaseFixture.cs` builds the provider with `ValidateScopes =
  true, ValidateOnBuild = true`; `src/HT.Ui/Program.cs` resolves `DemoDataService` at start through
  `StartupTasks`. Constitution V: done only when "the full test suite [has] passed in the tree being
  committed".
- **Reasoning**: After step 3 `AddHtApplication` registers three services and a `DemoDataService` whose
  ports have no registration until step 4. `ValidateOnBuild` then fails every integration test, and demo
  loading fails at host start. The plan says the order exists "because later steps need earlier ones" but
  does not say that steps 3 and 4 cannot be verified or committed apart.

### MINOR-3: `ToUpperInvariant` keys are not "the definition of `OrdinalIgnoreCase`"; uniqueness and search fold case differently

- **Where**: `research.md` R-03: "`HabitName.Key` is `Value.ToUpperInvariant()` … This is the definition of
  `StringComparison.OrdinalIgnoreCase` in .NET", and search by `Contains(text,
  StringComparison.OrdinalIgnoreCase)`. `data-model.md`, `HabitName` (`Key`, `Order`, `Matches`).
- **Against**: `spec.md` FR-004 and FR-009 (one comparison, "ordinally, ignoring case", for uniqueness,
  order and search). Checked on this machine's .NET 10.0.14 runtime (the app's target, ICU):
  `"ſ".ToUpperInvariant()` is `"S"` (U+017F to U+0053), while `string.Equals("ſ", "S", OrdinalIgnoreCase)`
  is false and `"ſun".Contains("S", OrdinalIgnoreCase)` is false. A scan of the Basic Multilingual Plane
  found this one character where the two disagree.
- **Reasoning**: The stated equivalence is false for U+017F. By the key, "ſun" and "Sun" are the same name
  (the second is refused as taken and the unique index agrees), but the search rule treats `ſ` and `s` as
  different letters. The plan therefore has two case-folding definitions where the rationale says "one key
  function gives uniqueness, order and the database index the same meaning", and the unit tests of R-13
  (`Éclair`, `ß`, Turkish `İ`/`ı`) would not show it.

### MINOR-4: Who decides the result of publish and unpublish is described three ways

- **Where**: `research.md` R-08: "When no row changes, the store re-reads: no habit gives not found, a
  Draft gives "not published"" and "the store applies the states it is given". `data-model.md`, Ports:
  `TrySetStateAsync(HabitId, IReadOnlySet<CatalogHabitState> from, CatalogHabitState to)` "(changed or
  not)". `data-model.md`, `CatalogHabitState`: `CatalogHabitStates.Publish/Unpublish` "return the next state
  or the refusal; the store applies them with conditional updates".
- **Against**: `CLAUDE.md` / `ARCHITECTURE.md` §3 (`HT.Persistence`: "no business rules or workflow
  decisions"); `spec.md` FR-016 (identifier before the FR-011 check).
- **Reasoning**: R-08 has the store tell "not found" from "not published", the port returns only whether a
  row changed, and the Domain function already returns the refusal. The three texts do not say whether the
  service or the store reads the habit and chooses between `NotFound`, `NotPublished` and success, so the
  port's result type and the place of the FR-011 rule are open for `/speckit-tasks`.

### MINOR-5: Use cases run inside interactive circuits on circuit-scoped stores; the plan's risks do not cover it

- **Where**: `research.md` R-11 (four interactive pages; filters "at once", search after 300 ms; row
  Publish and Unpublish); R-07 ("All three services are scoped"); `plan.md`, Risks, last bullet (only the
  30-minute revalidation).
- **Against**: `src/HT.Persistence/PersistenceServiceCollectionExtensions.cs` (`AddDbContext<HtDbContext>`,
  scoped; stores scoped). Slice 1 ran every use case in a request scope: its research R-08 kept pages
  static with `SiteNav` as "the only interactive component", and
  `IdentityRevalidatingAuthenticationStateProvider` "Reads in a new scope, as the circuit outlives any one
  request".
- **Reasoning**: This slice is the first to call use cases from a circuit, where one `HtDbContext` lives
  as long as the page is open and serves every handler of that page. Two handlers that overlap on an
  `await` (a debounced search finishing while a filter changes, two row actions) use that one context at
  the same time. The plan neither states the lifetime it intends for the stores inside a circuit nor lists
  this among its risks, and bUnit tests over fake stores cannot show it.

### MINOR-6: Two of the architecture rules of R-12 do not match the code they are to be added to

- **Where**: `research.md` R-12: "only types in `HT.Application.Operator` depend on `IDemoDataStore`";
  "reflection test, extending slice 1's for `AccountService`", listed under `HT.ArchitectureTests`.
- **Against**: `plan.md`, Project Structure (`HT.Persistence/Habits/DemoDataStore` implements the port,
  `PersistenceServiceCollectionExtensions` registers it; unit and UI tests use `FakeDemoDataStore`).
  Slice 1's reflection test is `SC_005_no_public_method_of_AccountService_takes_an_account_identifier` in
  `tests/HT.UnitTests/Application/Accounts/AccountDetailsTests.cs`, not in `HT.ArchitectureTests`.
- **Reasoning**: As worded, the first rule fails on the plan's own implementation and registration; the
  assemblies it covers are not stated (the existing rule in `UiDependencyTests` scopes itself to the
  `HT.Ui` assembly). The second names as its base a test that is in another project, so "extending" it
  does not say where the new rule goes.

### MINOR-7: The claim that the existing rule guards the culture-free name comparison is stronger than the rule

- **Where**: `plan.md`, Constitution Check row II: "The existing clock-and-culture architecture rule covers
  the new namespaces." `research.md` R-13 (unit tests "including `Éclair` / `éclair`, `ß`, Turkish
  `İ`/`ı`").
- **Against**: `tests/HT.ArchitectureTests/ClockReadTests.cs`: the only culture entry of `ForbiddenMembers`
  is `System.Globalization.CultureInfo::get_CurrentCulture(`. `spec.md` FR-004: "never by the current
  culture". Constitution II.
- **Reasoning**: The rule sees a direct read of `CultureInfo.CurrentCulture`, not a culture-sensitive
  overload such as `string.ToUpper()`, `string.Compare(a, b)` or a default string comparer, which read the
  culture inside the base library. R-13 does not say that the name tests run under a current culture other
  than the machine's; under an English or invariant culture a Turkish `İ`/`ı` case gives the same result
  for the culture-sensitive and the invariant call. So neither mechanism named in the plan would catch the
  defect FR-004 rules out.

### MINOR-8: Names that stretch glossary terms (constitution I)

- **Where**: `research.md` R-05: "`Intent` (Build, Reduce) is the same Domain enum for a habit's intent
  and a suggested starting goal's Build or Reduce"; `data-model.md` (`SuggestedStartingGoal.Intent`);
  `contracts/application-api.md` (`SuggestedStartingGoalIntentMissing`, `SuggestedStartingGoalView.Intent`,
  `CardSuggestion`, `CardSuggestionNotMarked`). `plan.md`, Constitution Check row I ("met"; "`CONTEXT.md`
  gets no entry").
- **Against**: `CONTEXT.md`: "Intent — Build or Reduce as the direction a catalog habit or custom habit is
  usually pursued in … NOT a personal goal's Build or Reduce, which the goal itself defines". "Suggested
  starting goal — … described by the same fields a goal starts from: Build or Reduce, measurement, …".
  `spec.md` FR-007 never calls this field an intent. `SPEC.md` §3, §4, §8 use "suggest" / "suggestion" for
  a custom habit offered to the catalog. Constitution I: glossary terms verbatim, no synonyms, and a change
  that sharpens a term updates `CONTEXT.md` in the same change.
- **Reasoning**: The glossary separates a habit's Intent from the Build or Reduce a goal starts from; the
  plan gives the suggested starting goal's field the name `Intent` and so widens the term without a
  `CONTEXT.md` change. `CardSuggestion` shortens "suggested starting goal" to a word SPEC uses for another
  thing (slice 10's suggestions). Both names are on the AC surface and will be read by slice 4.

### MINOR-9: Design-first step and component list miss things the plan itself introduces

- **Where**: `research.md` R-11: "Profile's Tabs are links (`?tab=`)". `plan.md`, Constitution Check row
  IV (the list of new components) and Project Structure ("`HtIcon` + 24 habit icons"); Work order 1 ("the
  spec's "Design changes before build" list").
- **Against**: `design/components/Tabs.md`: "`div.ht-tabs[role=tablist]` with `button.ht-tab[role=tab]`";
  `design/css/ht-components.css` line 151 styles `.ht-tab` as a button (`background:none; border:0;
  cursor:pointer`). `design/components/BackLink` exists, and US2-AS6 ("← Catalog") and
  `design/pages/PageCustomHabit` ("← My custom habits") use it. `design/components/SearchField.html` uses
  the `search` icon and `MobileNav.html` the `layout-grid` icon for Catalog; `HtIcon.razor` knows seven
  names and throws for any other. Constitution IV: a visual change is made in `design/` first; each design
  component is built once.
- **Reasoning**: Tabs as links is a markup change to a design component that neither the spec's design
  list nor Work order 1 contains. BackLink is a design component two pages of this slice need, and it is
  not in the list of components the plan says it builds once. The non-habit icons the new components need
  are not mentioned. The tasks step has no source for these three items.

### MINOR-10: Plan positions on spec gaps, and a cross-slice choice, are not routed through Decision Discipline

- **Where**: `plan.md`, "Spec discrepancies" 1 to 4 ("The owner may decide otherwise before
  `/speckit-tasks`"); "Gates before the next step" ("No roadmap decision is due"); Constitution Check,
  Decision Discipline ("met … need no new ADR"). `research.md` R-02 (one `HabitId` for both kinds; "Slice 4
  can reference either kind with one type"; the two-type alternative rejected). Also `data-model.md`,
  `SuggestedStartingGoal.TryCreate` order 10 ("times missing or outside 1–20"), which FR-007 does not state
  and the discrepancy list does not include.
- **Against**: Constitution, Decision Discipline: answers come from the owner and are recorded in the
  spec's `## Clarifications`; "A decision that spans features, is hard to reverse, or rejects real
  alternatives MUST also become an ADR". `docs/roadmap.md`, "Technical rows".
- **Reasoning**: Discrepancies 1 to 3 decide results on the AC surface (what is refused, how length is
  counted). The plan takes a position on each and waits for no answer: the gate section names no gate for
  them, and the Constitution Check row does not mention them. R-02 is justified by slice 4 and rejects a
  real alternative, which is the constitution's test for an ADR; the row says no ADR is needed without
  addressing it.

### MINOR-11: No stated source or check for the 14 new icon files

- **Where**: `research.md` R-01 ("SVG files from Lucide 0.460.0 … no icon package"); R-13 ("every name of
  `HabitIcon.All` has a file in `design/icons/` and renders in `HtIcon`").
- **Against**: `spec.md`, "Design changes before build" ("added from Lucide 0.460.0");
  `design/brand-book.md` and `design/components/Icons.md` (Lucide 0.460.0, ISC). `HtIcon.razor` holds the
  drawings as inline copies; `tests/HT.UiTests/DesignSyncTests.cs` compares only the two CSS files.
- **Reasoning**: The plan does not say where the files of exactly that version come from or how the
  version is confirmed. The planned test checks that a file exists and that a name renders, not that the
  drawing in `HtIcon` is the one in `design/icons/`, so a wrong version or a copy that differs from the
  design file passes.

### MINOR-12: Small gaps in `quickstart.md` and `contracts/host.md`

- **Where**: `quickstart.md`, step ""+ New catalog habit": a name, a category, an icon and two suggested
  starting goals, mark the second" with the result "saved as Draft". `contracts/host.md`, route `/` and
  "Page behaviour the tests check".
- **Against**: `spec.md` FR-005 (no intent gives "Choose Build or Reduce.");
  `design/components/SegmentedControl.md` ("or none until the user chooses"). `spec.md` HP-012 ("With no
  Published habit the section is absent"; "See all N →") and HP-013.
- **Reasoning**: The quickstart step as written omits Build or Reduce, so following it gives a refusal,
  not a Draft. The host contract lists the tested behaviour of Catalog, both forms and the Admin table, but
  not of Home: the absent section with no Published habit, "See all N →" and the registered user's
  EmptyState have no entry.

## Summary

3 MAJOR, 12 MINOR.

The plan covers the spec broadly, and its texts, outcomes, tables and counts (73 and 63 scenarios, 14 use
cases, 22 seed habits on 24 icons, the 14 missing icon files) agree with the spec, the locked acceptance
criteria and the code. It is not ready for `/speckit-tasks` yet. One locked acceptance behaviour (the Home
preview) has no use case and its rule ends up in a page. The privacy rule for logs (HP-015) is claimed as
met while the plan's own uniqueness path writes custom-habit names to the log, with no test planned. One
acceptance scenario of the spec (US4-AS15) is not covered for the address the plan itself chose. The MINOR
notes are mostly places where the tasks step would have to guess: slice 1 tests and an ordering of work
that the plan does not acknowledge, an unverified claim about case folding, unclear responsibilities at
the store port, the store lifetime inside circuits, architecture rules as worded, and design items missing
from the design-first step.
