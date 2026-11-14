# Research: Habit catalog

**Feature**: specs/002-habit-catalog · **Date**: 2026-11-14 · **Plan**: [plan.md](plan.md)

The spec's `## Clarifications` and ADRs 0011 to 0013 settle the product questions of this slice. Slice 1's
technical choices stay in force (its [research.md](../001-accounts-and-access/research.md) as amended by
[ADR 0010](../../docs/adr/0010-public-account-store-port.md)). This file records only the choices this
slice adds, each as decision, rationale and alternatives. Nothing here reopens a recorded decision.

## R-01 Packages

- **Decision**: no new package in any production or test project. The slice uses what slice 1 brought:
  EF Core 10 and Identity's EF stores in `HT.Persistence`, `Microsoft.Extensions.DependencyInjection.Abstractions`
  in `HT.Application`, the ASP.NET Core shared framework in `HT.Ui`, and xUnit, ArchUnitNET, bUnit,
  `Microsoft.AspNetCore.Mvc.Testing` and `Microsoft.Extensions.TimeProvider.Testing` in the tests. The 14 new
  Lucide icons (spec, "Design changes before build") are SVG files from Lucide 0.460.0, the version
  `design/icons/` already uses; no icon package.
- **Rationale**: nothing in the slice needs a library the stack lacks. ADR 0003: a package added to the AC
  surface would also have to be added to the AC project; there is none.
- **Alternatives**: a Lucide NuGet or npm package (adds a dependency and a build step for 24 static
  drawings; rejected).

## R-02 One identifier type for catalog and custom habits

Accepted by the owner on 2026-11-14: [ADR 0014](../../docs/adr/0014-one-habit-identifier.md).

- **Decision**: `HT.Domain.Habits.HabitId`, a readonly record struct over a `Guid`, identifies catalog habits
  and custom habits alike. Each kind lives in its own table with its own `Guid` key, created by
  `Guid.NewGuid()` in the store, so an identifier names at most one habit of either kind.
- **Rationale**:
  - The spec treats a custom habit's identifier given to a catalog use case, and a catalog habit's identifier
    given to a custom-habit use case, as ordinary inputs with a fixed result (US2-AS5, US3-AS13, FR-013,
    FR-022). With one type the call is written as the spec says it; with two, a test would convert one type
    into the other, which the spec does not describe.
  - SPEC §2 and §5.1: a goal is "based on a catalog or custom habit". Slice 4 can reference either kind
    with one type.
- **Alternatives**: `CatalogHabitId` and `CustomHabitId` (type safety between the kinds, but the spec's
  cross-kind cases need a conversion in every caller; rejected).

## R-03 Names: trimming, length, uniqueness, order and search

- **Decision**:
  - `HabitName` (Domain) holds the trimmed text (`string.Trim()`, as slice 1 trims emails). Length is
    counted in UTF-16 code units (`string.Length`), the unit SQL Server's `nvarchar(60)` counts, so the
    domain limit and the column agree. The same counting applies to the description (500) and the count
    unit (20).
  - `HabitName.Key` is `Value.ToUpperInvariant()`. Two names are the same name when their keys are equal
    ordinally; name order compares keys with `string.CompareOrdinal`; search compares keys too (below).
    This one key function is how the plan reads FR-004's "ordinally, ignoring case (culture-invariant)":
    invariant upper-casing, then an ordinal comparison, with no current culture read (constitution II).
    It is not identical to `StringComparison.OrdinalIgnoreCase`: on the .NET 10 runtime with ICU,
    `"ſ".ToUpperInvariant()` is `"S"` (U+017F), while `OrdinalIgnoreCase` keeps them apart (critic plan pass
    1, MINOR-3, the one difference found in the Basic Multilingual Plane). Because uniqueness, order, search
    and the database index all use the key, they never disagree with each other; a unit test pins `ſ` and `S`
    as the same name. `ToUpperInvariant` maps code units one to one, so the key has the name's length. Keys
    never tie inside one set, because names are unique there.
  - The database enforces uniqueness on the key: `CatalogHabits.NameKey` with a unique index, and
    `CustomHabits (OwnerId, NameKey)` with a unique index. Both key columns use the binary collation
    `Latin1_General_100_BIN2` (as slice 1's `NormalizedUserName`), so the database's "same key" is exactly
    the domain's. How a lost race becomes "name taken" without an error that carries the name: R-14 (HP-015,
    HP-018).
  - Search (FR-009, FR-012): the trimmed search text matches a name when
    `name.Key.Contains(text.Trim().ToUpperInvariant(), StringComparison.Ordinal)`; an empty or missing text
    matches every name.
  - Ordering and filtering run in `HT.Application` over the habits the store returns (R-08): the store
    returns every Published habit for browsing, every catalog habit for the administrator's list, and the
    owner's custom habits for their list. The catalog has tens of habits.
- **Rationale**: one key function gives uniqueness, order and the database index the same meaning, and the
  rules stay in Domain and Application, where the unit tests reach them. Spec AC items that use `Éclair`
  and `éclair` (AC-045) tell ordinal from culture order whichever way case is folded. Filtering in memory
  avoids translating `OrdinalIgnoreCase` into SQL, where collations decide case and accent rules.
- **Alternatives**: SQL `ORDER BY` and `LIKE` over a case-insensitive collation (the collation's culture
  rules would decide FR-004, not the code; rejected). A computed column for the key (the key rule would
  live in SQL; rejected). Paging in the store (no scale needs it; constitution VI; rejected).

## R-04 Field checks live in Domain, in the spec's order

- **Decision**:
  - `HabitDefinition.TryCreate(name, description, category, intent, icon)` (Domain) checks FR-005's fields
    in its order and returns either the definition or the first `HabitFieldProblem`: `NameMissing`,
    `NameTooLong`, `DescriptionTooLong`, `CategoryMissing`, `IntentMissing`, `IconMissing` (missing or not
    a habit icon). A description that is empty after trimming becomes no description.
  - `SuggestedStartingGoals.TryCreate(inputs)` (Domain) checks FR-006's list rules first (more than 5, then
    the mark), then each item in list order with `SuggestedStartingGoal.TryCreate`, which checks FR-007's
    fields in its order. A problem carries its `SuggestedStartingGoalProblem` and, for an item, its position
    counted from 1.
  - The use cases call these checks after the acting user and the identifier (FR-016 steps 1 to 3), map
    each problem to its named outcome and text (R-07), and check name uniqueness last (step 5): first by
    reading the names in the store, then by the store's write itself, which refuses a taken name (R-14).
- **Rationale**: the order of checks is a domain rule with many boundaries (constitution II asks for both
  sides of each); keeping it in Domain makes it unit-testable without a store, and the same checks serve
  catalog habits, custom habits and the seed catalog (R-10).
- **Alternatives**: data annotations on input classes (cannot express the order or the position; ties the
  rule to a framework; rejected). Validation in the Blazor forms (constitution III: the page is never the
  protection; rejected).

## R-05 The fields of a suggested starting goal

- **Decision** (ADR 0011, FR-007; types in [data-model.md](data-model.md)):
  - A habit's direction is `Intent` (Build, Reduce), the glossary term. A suggested starting goal's Build
    or Reduce is a different concept: `CONTEXT.md` keeps Intent apart from "a personal goal's Build or
    Reduce", and a suggested starting goal is described by "the same fields a goal starts from". So it has
    its own Domain enum `BuildOrReduce` (Build, Reduce), named after SPEC §2's term "Build / Reduce", which
    slice 4's goals reuse (critic plan pass 1, MINOR-8). The card's marked one is "the marked suggested
    starting goal", never a "suggestion": SPEC §4 and §8 use that word for Suggest for catalog (slice 10).
  - `Measurement` (YesNo, Count, Duration); `DurationUnit` (Minutes, Hours); the amount an `int`; the count
    unit the trimmed text.
  - `Schedule` is either fixed days (a non-empty set of `DayOfWeek`) or N times (1 to 20) per `SchedulePeriod`
    (Day, Week). `DayOfWeek` is the base library's enum, which reads no culture.
  - The input (`SuggestedStartingGoalInput`, Application) has one nullable value per form control, so every
    "no …" case of FR-007 can be expressed: `BuildOrReduce?`, `Measurement?`, `Amount int?`, `CountUnit string?`,
    `DurationUnit?`, `ScheduleKind?` (FixedDays, Times), `Days`, `Times int?`, `Period?`, `IsMarked`.
  - "A yes/no with an amount or a unit" means an amount, a count unit that is not empty after trimming, or a
    duration unit. Values that do not belong to the chosen measurement or schedule kind (a duration unit on a
    count, a count unit on a duration, days with "times") are not refused, because FR-007 names no refusal
    for them, and are not stored (see plan, "Spec discrepancies" 1).
- **Rationale**: the input mirrors the New goal form (`design/pages.md`), which slice 4 pre-fills; nullable
  fields let the use case, not the form, decide every refusal.
- **Alternatives**: a polymorphic input (one class per measurement; a missing measurement could not be
  sent; rejected). Storing days as text (needs parsing; a bit mask is exact; see R-08).

## R-06 Text and kind of a suggested starting goal

- **Decision**: `SuggestedStartingGoalText` (Domain), a static pure function from a `SuggestedStartingGoal`
  to its text and kind by the rules of FR-017. It is the one shared formatter ADR 0011 requires; browsing,
  details and the administrator's read return its output, and no page formats a suggested starting goal
  itself. Day runs: Monday-first order, no wrap from Sunday to Monday, a run of three or more as
  "first–last" (U+2013), other days on their own, comma-separated.
- **Rationale**: the text is never stored or typed (ADR 0011), so it must come from one function; unit tests
  cover every example of US2-AS1, US2-AS3 and US3-AS17 plus the boundaries of the run rule (two consecutive
  days, three, a Sunday–Monday pair, all seven).
- **Alternatives**: a formatter in `HT.Ui` (the AC tests could not check the texts the spec promises;
  rejected). Storing the text (two sources of truth; rejected by ADR 0011).

## R-07 Use-case services, acting user and results

- **Decision** (signatures in [contracts/application-api.md](contracts/application-api.md)):
  - Three services in `HT.Application.Habits`, one per sub-area (ADR 0005):
    - `CatalogService`: browsing, the Home preview, the category filter and details. These take **no
      acting user**: FR-008 and FR-010 give every audience the same result, and ADR 0005 says use cases for
      anonymous visitors take none. A parameter the method must ignore would suggest the result depends on
      it. The Home preview is its own use case (`GetHomePreviewAsync`): FR-008's "first four habits of this
      result with no filters, fewer when fewer exist" is an application rule on the AC surface, which AC-012
      and AC-072 call ("asks for the Home preview"); the page only renders it (critic plan pass 1, MAJOR-1).
    - `CatalogAdministrationService`: the administrator's list, read, create, edit, publish and unpublish.
      The acting user comes first; FR-016's order is: anonymous, then `Role.Administrator`, then the
      identifier, then fields or the state rule, then uniqueness.
    - `CustomHabitService`: list, read, create, edit and delete. The acting user comes first; an
      administrator is refused at step 2 (FR-025); every store call passes the acting user's `AccountId` as
      the owner (FR-021, R-08).
  - The role is the acting user's role. The host builds it from the revalidated session (slice 1, R-07, R-09)
    and the tests from a sign-in result; roles never change in v1, so no store read is needed.
  - Results follow slice 1 (research R-06 there): one sealed result class per use case, an `Outcome` enum, a
    `Message` with the spec's text for every refusal and `null` for successes the spec gives no text, and
    typed values for success. Refusals about one suggested starting goal also carry `Position`.
  - Texts are `public const string` in `HabitTexts` (field messages shared by both kinds), `CatalogTexts`
    and `CustomHabitTexts`; `OperatorTexts` gains the three demo results (FR-030, FR-031). "You are not
    signed in." stays `AccountTexts.NotSignedIn`.
  - All three services are scoped (they use scoped stores). How interactive pages call them: R-15.
- **Rationale**: ADR 0005 (services by area, explicit acting user, results with text). Splitting by
  sub-area keeps each service's audience rule in one place.
- **Alternatives**: one `HabitService` for all 15 use cases (mixes three audience rules; rejected).
  Browsing that takes an `ActingUser` and ignores it (rejected, above).

## R-08 Stores, tables and the migration

- **Decision**:
  - Ports in `HT.Application.Habits`, public and fenced by architecture rules as ADR 0010 prescribes for
    later ports: `ICatalogHabitStore` and `ICustomHabitStore`. In `HT.Application.Operator`:
    `IDemoDataStore` (R-09). Implementations are `internal` in `HT.Persistence.Habits` and registered by
    `AddHtPersistence`.
  - `ICustomHabitStore` has no lookup by identifier alone: every member takes the owner's `AccountId` first
    and filters by it in the same query (`WHERE OwnerId = @owner AND Id = @id`), so another owner's habit and
    no habit give the same "not found" (FR-022, constitution III). An architecture test checks the first
    parameter of every member (R-12).
  - Tables (new migration `HabitCatalog`; columns in [data-model.md](data-model.md)):
    - `CatalogHabits` (key `Id`), unique index on `NameKey`;
    - `CatalogHabitSuggestedStartingGoals` (key `CatalogHabitId`, `Position`), mapped as an owned
      collection of `CatalogHabits`, cascade delete;
    - `CustomHabits` (key `Id`), `OwnerId` with a foreign key to `AspNetUsers.Id` (cascade delete, though no
      account is deleted in v1), unique index on `(OwnerId, NameKey)`.
    - Enums are stored as strings (`HasConversion<string>()`, as slice 1 stores `Role`); days as an `int` bit
      mask (Monday 1 … Sunday 64).
  - Reads are no-tracking. A catalog edit loads the tracked habit with its suggested starting goals,
    replaces the fields and the whole list, and saves once, so the row and its list change in one
    transaction. The change tracker is cleared before and after each write, as in slice 1's store.
  - State changes (critic plan pass 1, MINOR-4): the **service** decides, the store only writes. The service
    reads the habit (`FindAsync`; none is `NotFound`, FR-016 step 3), asks Domain for the transition
    (`CatalogHabitStates.Publish` / `Unpublish`: a refusal is `NotPublished`, the same state is a success
    with no write, otherwise the next state), and calls `TrySetStateAsync(id, expected, next)`, one
    `ExecuteUpdateAsync` with `WHERE Id = @id AND State = @expected` that returns whether a row changed.
    When it did not, another administrator acted in between: the service reads again and decides again, at
    most 10 times (as slice 1's sign-in, research R-05 there), then fails with a fault. The store has no rule
    of its own.
  - Custom-habit writes are single conditional statements scoped by owner and identifier (R-14); no row means
    "does not exist" for an edit or delete.
- **Rationale**: the owner parameter makes constitution III structural in the port, not only in the
  service. The compare-and-swap keeps FR-011 true when two administrators act at once, and keeps the rule in
  Domain and the decision in Application (ARCHITECTURE §3). An owned collection keeps the list's order
  (`Position`) and its replacement in one save. The binary-collation keys and the unique indexes cover
  HP-018.
- **Alternatives**: a JSON column for the suggested starting goals (EF Core 10 supports it, but the column
  type depends on the server's compatibility level, and the local server's version is not pinned; rejected).
  Optimistic concurrency tokens on catalog edits (two administrators editing one habit is not a spec case;
  the last save wins; not needed). A generic unit-of-work port (more surface than the two atomic operations
  need; R-09).

## R-09 Demo loading: seed catalog and demo custom habits, each as one unit

- **Decision**:
  - `IDemoDataStore` (Application, `HT.Application.Operator`) has two members, each one database transaction:
    - `CreateDemoAccountAsync(Email, string password, IReadOnlyList<HabitDefinition> customHabits)`: creates
      the registered-user account through Identity's user manager and its custom habits in the same
      transaction, or nothing. An email that already has an account gives "email taken", nothing created.
      Each custom habit is written with R-14's conditional insert; one that inserts no row (a name already
      taken) is a fault, which rolls back the account too and names no habit.
    - `TryCreateSeedCatalogAsync(IReadOnlyList<SeedCatalogHabit> habits)`: in a serializable transaction,
      creates every seed habit with its state only when `CatalogHabits` is empty; returns false and creates
      nothing otherwise. A unique-index violation (a concurrent start or an administrator who created a
      habit at that moment) rolls back and also returns false, and so does being chosen as a deadlock victim
      (SQL error 1205) by a concurrent start: the other start's seed is then complete or the catalog was
      already not empty, so "catalog not empty" is true.
  - `DemoDataService.LoadAsync` keeps slice 1's order: the password checks, then each demo email (an account
    found by email is "already exists", with no custom-habit result and no further call; otherwise
    `CreateDemoAccountAsync`), then the seed catalog. The demo custom habits per email are fixed data in
    `DemoCustomHabits` (FR-031), and the seed content is fixed data in `SeedCatalog` (R-10).
  - Only `HT.Application.Operator` depends on `IDemoDataStore`, and no `HT.Ui` type does (R-12). It has no
    read of private data: its only read is whether `CatalogHabits` is empty.
- **Rationale**: FR-030 and FR-031 ask for "as a whole or not at all" and "as a whole with the account";
  only one transaction per unit gives that, and the slice-1 account store's `CreateAsync` saves on its own.
  A port named for demo data makes ADR 0013's exception (private writes without an acting user, only into a
  just-created account) visible and fenced.
- **Alternatives**: calling `CustomHabitService` as each demo account (needs an acting user without a
  sign-in; rejected by ADR 0013). `TransactionScope` around the existing stores (ambient transactions across
  `UserManager` and a second store are easy to break silently; rejected). A seed migration with `InsertData`
  (seeds every database, not only demo deployments, and cannot be skipped when the catalog is not empty;
  rejected by FR-030).

## R-10 Seed catalog content

- **Decision**: `SeedCatalog` (Application, `HT.Application.Operator`) is a static list of 22 habits: 20
  Published, one Draft and one Unpublished, written during implementation by Claude following
  `design/brand-book.md` and HP-014, and reviewed by the owner in the PR (Clarifications). A unit test checks
  every FR-030 property: at least 20 Published covering all six categories, at least 4 of them Reduce, a
  description on each, 22 different icons, unique names, all three measurements and both schedule kinds
  among the suggested starting goals, at least one Published habit with none and one with three or more,
  exactly one Draft and one Unpublished, and every habit passing the Domain checks of R-04.
- **Rationale**: data, not behaviour, so a static list is enough; the test makes FR-030 an executed check
  (constitution V) instead of a review claim. 22 habits fit the 24 icons with one icon each.
- **Alternatives**: a JSON file read at start (a parse step and a file to deploy; rejected).

## R-11 Pages, render modes, addresses and Not found

- **Decision**:
  - Routes ([contracts/host.md](contracts/host.md)): `/catalog` (the Public catalog tab), `/custom-habits`
    (the "My custom habits" tab), `/catalog/{id:guid}`, `/custom-habits/new`, `/custom-habits/{id:guid}`,
    `/admin`, `/admin/catalog/new`, `/admin/catalog/{id:guid}`, and `/profile?tab=custom-habits`.
  - "My custom habits" has its own address with `[Authorize]`, so US4-AS15 holds for it as for the custom
    habit page: an anonymous visitor is sent to Sign in, an administrator gets Not found (critic plan pass 1,
    MAJOR-3). Both tabs render the same Catalog page head and Tabs; the Tabs are links between the two
    addresses.
  - Tabs that change the address are a link variant of `design/components/Tabs` (`a.ht-tab` with
    `aria-current="page"` inside a `nav`), added in `design/` first (Work order step 1). The Catalog tabs and
    Profile's tabs (`?tab=custom-habits`) use it, so Profile stays static and slice 1's change-password form
    keeps working. The button variant stays for in-page tabs.
  - Interactive Server with prerendering for the pages that change without a full request: both Catalog
    tabs (filters and a debounced search apply at once, HP-003), the custom habit page (field refusals, the
    Delete Dialog with focus management, HP-007), Admin (row Publish and Unpublish show the new state at
    once, HP-009) and the administrator's catalog habit form (adding, removing, reordering and marking
    suggested starting goals). The detail page, Home and Profile stay static server-rendered.
  - Filters live in the address: `q`, `category` (`body` … `other`) and `intent` (`build`, `reduce`) on
    `/catalog`, `q` on `/admin`. The page reads them with `[SupplyParameterFromQuery]` and writes them with
    `NavigationManager.NavigateTo(…, replace: true)`; an unknown value is ignored. The search waits 300 ms
    after the last keystroke.
  - Signed-in pages use `[Authorize]`, so an anonymous visitor is sent to Sign in with `returnUrl` (slice 1).
    A page whose use case refuses or does not find the habit, and an administrator on a custom-habit address
    or a registered user on an Admin address, calls `NavigationManager.NotFound()` (.NET 10), which renders
    the Router's `NotFoundPage` (slice 1's `NotFound` page, already set in `Routes.razor`) with status 404
    during prerendering. The use cases refuse on their own (FR-014, FR-025); the page only chooses what to
    show.
- **Rationale**: interactivity only where the spec asks for an immediate change; the address keeps a shared
  or reloaded catalog the same (HP-003). `NotFound()` gives one Not found page for every audience (HP-006)
  without a redirect that would reveal a habit's existence.
- **Alternatives**: a query value on `/catalog` for "My custom habits", as `design/components/Tabs.md`'s
  example `/catalog?view=mine` suggests (critic plan pass 2, MINOR-4): it keeps one page, but `/catalog` is
  for anyone, so the page itself would have to send an anonymous visitor to Sign in and give an
  administrator Not found for one query value, outside the framework's `[Authorize]`; rejected in favour of
  an address of its own, and the design step changes that example to `/custom-habits` (plan, "Spec
  discrepancies" 4, for the owner). Static forms with JavaScript for filtering (a second rendering path for
  the list; rejected). `[Authorize(Roles = "Administrator")]` on Admin (a registered user would get the access-denied
  redirect, not the Not found page HP-010 requires; rejected). Interactive WebAssembly (no second client;
  ARCHITECTURE §2; rejected).

## R-12 Architecture rules added

- **Decision** (constitution V: same class of rule as slice 1, extended to the new types; critic plan
  pass 1, MINOR-6 and MINOR-7):
  - `UiDependencyTests` (ArchUnitNET, scoped to the `HT.Ui` assembly like its existing port rule): no `HT.Ui`
    type depends on `ICatalogHabitStore`, `ICustomHabitStore` or `IDemoDataStore` (ADR 0010's pattern).
  - `ApplicationDependencyTests` (ArchUnitNET, scoped to the `HT.Application` assembly): inside
    `HT.Application`, only types in `HT.Application.Operator` depend on `IDemoDataStore`. `HT.Persistence`
    implements and registers it and the test fakes implement it; the rule does not cover those assemblies.
  - A new `UseCaseParameterTests` class in `HT.ArchitectureTests` (plain reflection over the loaded
    assemblies, no ArchUnitNET): every public method of `CatalogAdministrationService` and
    `CustomHabitService` takes `ActingUser` as its first parameter and no `AccountId` anywhere; no public
    method of `CatalogService` takes an `ActingUser` or an `AccountId`; every method of `ICustomHabitStore`
    takes `AccountId` as its first parameter (no lookup of private data by identifier alone, constitution
    III). Slice 1's `SC_005_…` test stays where it is, in `HT.UnitTests`.
  - `ClockReadTests` gains the culture-sensitive members that read the current culture inside the base
    library: `CultureInfo.get_CurrentUICulture`, the parameterless `string.ToUpper()` and `string.ToLower()`,
    `char.ToUpper(char)` and `char.ToLower(char)`, `string.Compare(string, string)` and its `bool ignoreCase`
    overload, `string.CompareTo`, and `StringComparer.get_CurrentCulture` / `get_CurrentCultureIgnoreCase`.
    So FR-004's "never by the current culture" is enforced in Domain and Application, not only claimed.
  - The existing Domain, Application and Persistence rules cover the new namespaces unchanged.
- **Rationale**: the privacy boundary of the first private data is enforced by executed rules, not by review
  alone.
- **Alternatives**: internal ports with `InternalsVisibleTo` (ruled out by ADR 0010).

## R-13 Tests

- **Decision** (projects and databases as ADR 0006 and slice 1's research R-16):
  - **Unit** (`HT.UnitTests`): `HabitName` keys, order and search (including `Éclair` / `éclair`, `ß`,
    `ſ` / `S`, Turkish `İ`/`ı`), run with the current culture set to `tr-TR`, where a culture-sensitive call
    would give a different result than the invariant one; every FR-005, FR-006 and FR-007 boundary on both
    sides (name 60/61, description 500/501, 5/6 suggested starting goals, Build amount 0/1/999/1000, Reduce
    amount -1/0/999/1000, unit 20/21, times 0/1/20/21), the FR-006 list order, the FR-016 order across steps, every
    FR-017 example, `CatalogHabitState` transitions, the seed catalog properties (R-10), and the three
    services with hand-written fake stores (`FakeCatalogHabitStore`, `FakeCustomHabitStore`,
    `FakeDemoDataStore`), including the negative audiences of SC-006 for every use case, a lost
    uniqueness race (the store refuses after the service's own check passed), a lost state race (the
    compare-and-swap of R-08 fails once), and the Home preview with 0, 3, 4 and 5 Published habits.
  - **Slice 1 unit tests rewritten** (critic plan pass 1, MINOR-1): `tests/HT.UnitTests/Application/Operator/DemoLoadingTests.cs`
    builds `DemoDataService` from `FakeAccountStore` alone and asserts that store's `CreateCount`,
    `PasswordOf` and `TakeEmailOnNextCreate`. With R-09 the constructor also takes `IDemoDataStore`, and
    accounts are created through it. `FakeDemoDataStore` therefore wraps the test's `FakeAccountStore`
    (it creates the account there, so `AccountService.SignInAsync` over the same fake still signs it in, and
    honours `TakeEmailOnNextCreate` as the lost race) and records the custom habits per account. The class
    keeps every slice 1 case and gains the FR-030 and FR-031 cases.
  - **Integration** (`HT.IntegrationTests`, `HT-IT`): the stores against SQL Server: name keys under the
    binary collation, parallel creates and renames with one name (HP-018) for both kinds, owner scoping of
    every custom-habit statement, the owned list's order and replacement, the state compare-and-swap,
    `CreateDemoAccountAsync` and `TryCreateSeedCatalogAsync` rolling back completely when one habit of the
    given list cannot be written (a duplicate name in the list). **HP-015**: the custom-habit race tests and
    the demo rollback test run with a capturing logger provider at `Trace` for every category, on custom
    habits with a description; they assert that no log entry, no exception message and no inner exception
    message contains the custom habit's name, its key or its description (R-14).
  - **Starting states** (critic plan pass 2, MINOR-2; ADR 0006 has no reset between tests, and catalog
    habits cannot be deleted): the shared `HT-IT` database gets the seed catalog whenever slice 1's
    integration test `tests/HT.IntegrationTests/Operator/DemoLoadingTests.cs` runs demo loading. That test
    keeps its assertions (account results, sign-ins), which this slice does not change, and gains none of
    the catalog's; every other store test uses names of its own and checks by inclusion, so it holds with or
    without the seed. The tests that need an empty catalog — `TryCreateSeedCatalogAsync`'s success path,
    "catalog not empty" after an existing habit, and SC-007 (demo loading twice gives exactly the same
    catalog and custom-habit rows) — run against a database of their own, `HT-IT-SEED` on the same server,
    reset with `HtDatabase.ResetAsync` at the start of each such test, as slice 1's HP-022 test uses
    `HT-UI-MIGRATE` (its research R-16). They share one xUnit collection, so they run one at a time.
  - **UI** (`HT.UiTests`): bUnit for `HabitCard` and the other new shared components, the Catalog page
    (filters, address, Tabs, EmptyStates), the custom habit form (refusals under their field, the "does not
    exist" Notice, the Delete Dialog's focus handling), the admin form's editor, `SiteNav`, Home (including
    the "Popular in the catalog" section absent with no Published habit, HP-012, over an empty fake catalog
    store, since the one host of the host tests always has the seed catalog) and Profile; host tests for the
    routes, `[Authorize]` redirects and the 404 Not found responses (HP-006, HP-010, US4-AS15 for
    `/custom-habits`, `/custom-habits/new` and `/custom-habits/{id}`, US5-AS4), the visitor Home with the
    seed catalog (four cards, "See all N →"), and the startup log lines (HP-017). The icon test of R-16.
  - **Slice 1 tests changed** as the spec lists ("Slice 1 requirements and tests this slice changes"); the
    `StartupLogTests` count stays valid and is re-run.
  - **Acceptance**: the 73 scenarios of `tests/HT.AcceptanceTests/Features/F002/`, written by the AC
    workflow after implementation.
- **Rationale**: constitution II (both sides of every boundary), III (negative tests per use case), V.
- **Alternatives**: a mocking library (ruled out by ADR 0006).

## R-14 No custom-habit name in logs or exceptions (HP-015), also on a lost race (HP-018)

- **Decision** (critic plan pass 1, MAJOR-2):
  - SQL Server's texts for errors 2601 and 2627 end with "The duplicate key value is …", which for
    `IX_CustomHabits_OwnerId_NameKey` is the owner's identifier and the upper-cased name, and EF Core logs
    such a failure at Error level before the store can catch it. So custom-habit writes never rely on the
    unique index to refuse a taken name:
    - **create**: one parameterized statement, `INSERT INTO CustomHabits (…) SELECT … WHERE NOT EXISTS
      (SELECT 1 FROM CustomHabits WITH (UPDLOCK, HOLDLOCK) WHERE OwnerId = @owner AND NameKey = @key)`;
    - **edit**: `UPDATE CustomHabits SET … WHERE Id = @id AND OwnerId = @owner AND NOT EXISTS (SELECT 1 FROM
      CustomHabits WITH (UPDLOCK, HOLDLOCK) WHERE OwnerId = @owner AND NameKey = @key AND Id <> @id)`.
    The key-range lock makes a second statement with the same key wait for the first and then insert or
    update no row, so a lost race gives "name taken" with no SQL error. For an edit that changed no row, the
    store reads whether (`OwnerId`, `Id`) exists: no is "does not exist", yes is "name taken". The statements
    run through `ExecuteSqlInterpolatedAsync` (parameters, never concatenated text) inside `HT.Persistence`.
    The unique index stays as a backstop.
  - Backstop: a database exception from any custom-habit write is caught in the store and replaced by
    `InvalidOperationException("Saving a custom habit failed (SQL error {number}).")` with no inner
    exception, so the host's fault log carries no name.
  - `AddHtPersistence` configures the context with `ConfigureWarnings(w => w.Ignore(CoreEventId.SaveChangesFailed,
    RelationalEventId.CommandError))`, so EF Core never writes a database error text to the log; the stores'
    own exceptions, which carry no field value, are what the host logs. Sensitive-data logging stays off.
  - Catalog-habit names are public (FR-008), so catalog writes keep slice 1's pattern: a unique-index
    violation is mapped to "name taken".
- **Rationale**: HP-015 and constitution III forbid custom-habit names in logs and exception messages; the
  only robust way is not to produce an error that contains one. The integration tests of R-13 capture every
  log entry at `Trace` and check it, so HP-015 is an executed check (constitution V).
- **Alternatives**: hashing the key column (the hash of a short name is guessable, and the column would no
  longer give the binary-collation equality R-03 needs; rejected). Only filtering EF Core's log categories
  (the exception text would still reach the host's fault log; rejected). A serializable transaction around
  read and insert (the same locks, two round trips; rejected).

## R-15 Use cases called from interactive circuits

- **Decision** (critic plan pass 1, MINOR-5): interactive pages never inject the use-case services into the
  component. They call them through `UseCaseScope` (`HT.Ui/Habits`), which creates a new
  `AsyncServiceScope` for each call, resolves the service there, awaits the call and disposes the scope. So
  every use-case call of a circuit has its own `HtDbContext` and stores, as a request has in a static page,
  and two handlers that overlap on an `await` never share a context. The Catalog page also cancels a
  pending debounced search when a newer input arrives and ignores a result that is older than the latest
  request. Static pages keep injecting the services (one request, one scope).
- **Rationale**: slice 1 ran every use case in a request scope and read the session in a new scope from the
  circuit (`IdentityRevalidatingAuthenticationStateProvider`). This slice is the first to call use cases
  from a circuit, whose DI scope lives as long as the page; `DbContext` is not safe for concurrent use.
- **Alternatives**: `OwningComponentBase` (one scope per component, still shared by its handlers; rejected).
  `AddDbContextFactory` in Persistence (changes every store and the AC composition for a host concern;
  rejected).

## R-16 The 14 new icons and their check

- **Decision** (critic plan pass 1, MINOR-11):
  - Source: the files `icons/<name>.svg` of the npm package `lucide-static` at exactly version `0.460.0`
    (`https://cdn.jsdelivr.net/npm/lucide-static@0.460.0/icons/<name>.svg`), the version and ISC license
    `design/icons/` already carries (`design/icons/LICENSE`). The download happens in the design step, with
    the owner's approval, into `design/icons/`; the version is part of the URL, so no other version can be
    fetched by mistake.
  - `HtIcon` keeps inline drawings (slice 1) and gains the 24 habit icons and the other icons the new
    components use (`search`, `layout-grid`, `plus`, `pencil`, `trash-2`, `chevron-left`, all already in
    `design/icons/`).
  - Check, in `HT.UiTests`: for every name `HtIcon` knows, `design/icons/<name>.svg` exists, and the child
    elements of its root `svg` (elements and attributes, whitespace and attribute order normalised) equal
    what bUnit renders inside `HtIcon`'s `svg`. A further check: every name of `HabitIcon.All` is one `HtIcon`
    knows. So a drawing that differs from the design file, or a habit icon without a drawing, fails the
    build gate, as `DesignSyncTests` does for the CSS.
- **Rationale**: constitution IV (design is the source of truth) and V (an enforcing check rather than a
  review claim); the spec names Lucide 0.460.0.
- **Alternatives**: an `<img>` per icon from `wwwroot` (the icons inherit no `currentColor` and need one
  request each; rejected).
