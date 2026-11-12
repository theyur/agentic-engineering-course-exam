---

description: "Task list for slice 1, accounts and access"
---

# Tasks: Accounts and access

**Input**: Design documents from `specs/001-accounts-and-access/`: [plan.md](plan.md), [spec.md](spec.md),
[research.md](research.md), [data-model.md](data-model.md), [contracts/application-api.md](contracts/application-api.md),
[contracts/host.md](contracts/host.md), [quickstart.md](quickstart.md).

**Prerequisites**: `acceptance.md` and `tests/HT.AcceptanceTests/Features/F001/` are locked. No task edits
them, the lock files, the run records or the AC project.

**Tests**: required. The constitution (II, III, V) and `CLAUDE.md` make unit, integration, architecture
and selective UI tests part of done, so every story phase has test tasks. The test framework is xUnit v2
everywhere. There is no mocking library; tests use hand-written fakes (ADR 0006). Acceptance step
definitions are not tasks here. The owner starts them with `/speckit-ac-tests-write` after
`/speckit-implement`.

**Organization**: Phase 2 follows the plan's work order for the parts every story needs: design first,
then Domain, Application, Persistence, the host and the shell. Each story phase then adds its use case,
its tests, its form or page and its host tests.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: can run in parallel with the other [P] tasks of its group (different files, no dependency on an
  unfinished task).
- **[Story]**: the user story from spec.md (US1–US7).
- Paths are relative to the repository root.

## Conventions for every task

- Build with `dotnet build HabitTracker.slnx`. `TreatWarningsAsErrors` is on, so a warning is a failure.
- Use the names in [contracts/application-api.md](contracts/application-api.md) and
  [data-model.md](data-model.md). Every Application method is `async` and takes a `CancellationToken`
  last.
- Take message texts from `AccountTexts` and `OperatorTexts`, never retyped. Tests assert the literal
  text from spec.md, so a typo in a constant fails a test.
- Pages use only `design/` tokens and `ht-` classes, and follow the structure and wording of their
  `design/pages/` preview (constitution IV, HP-016).
- Never put a secret in a file, a log, a test name or a command line (HP-017, HP-019). Tests use throwaway
  literal values.
- Integration tests use `HT_IT_CONNECTION`, or `Server=.\COLL1;Database=HT-IT;Integrated Security=True;TrustServerCertificate=True`
  when it is not set. Host tests use `HT_UI_CONNECTION`, or the same string with database `HT-UI`. The
  `StartupTasks` tests use a third database, `HT-UI-MIGRATE`: `HT_UI_CONNECTION` (or its default) with
  the database name replaced by `SqlConnectionStringBuilder`, so CI needs no extra variable. No test
  touches `HT` or `HT-AC`.
- `Microsoft.Extensions.TimeProvider.Testing` is versioned on its own line, not with the 10.0.x patches:
  pin `10.11.0` everywhere, the version `tests/HT.AcceptanceTests/HT.AcceptanceTests.csproj` already uses.
- No `InternalsVisibleTo` in `HT.Domain`, `HT.Application` or `HT.Persistence`, the AC surface projects of
  `.workflow/config.json`. The AC reference export (`export_refs.py`) refuses any of them that contains it,
  which would stop `/speckit-ac-tests-write` (see "Plan discrepancies"). Tests use only public members of
  these three assemblies, and they read or change database rows with plain SQL (`Microsoft.Data.SqlClient`)
  over their own connection string, never through `HtDbContext`. `HT.Ui` is not an AC surface project and
  may grant `InternalsVisibleTo` to `HT.UiTests`.

## Plan discrepancies

Found while reviewing the tasks ([critic-tasks-pass-2.md](critic-tasks-pass-2.md), MAJOR-1). The tasks do
not edit the plan artifacts; the owner's decision is recorded here.

1. **Internal account-store port (research R-13, data-model.md "Application port").** The plan makes
   `IAccountStore` internal, gives the services internal constructors, and grants `InternalsVisibleTo` to
   `HT.Persistence` and three test projects. The AC extension's `export_refs.py` fails on `InternalsVisibleTo`
   in any AC surface project, so `/speckit-ac-tests-write` could not run. **Owner decision (2026-11-11):** a
   public port with architecture rules, the alternative R-13 had rejected:
   - `IAccountStore` is public, and so are the constructors of `AccountService`, `OperatorService` and
     `DemoDataService`;
   - `AccountStore`, `HtDbContext` and `HtUser`'s mapping stay inside `HT.Persistence` (`AccountStore` and
     `HtDbContext` internal);
   - R-13's reason for rejecting this ("the port's id-only lookups would still be reachable from non-component
     host code") is answered by a rule over the whole `HT.Ui` assembly, not only `HT.Ui.Components`: no `HT.Ui`
     type depends on `IAccountStore` (T053).

   The AC listings now show the port. The isolated test writer is still limited by ADR 0003 to the use cases
   and the two Persistence entry points, and the judge sees its step definitions' results.

   **Superseded statements.** Where these plan files say otherwise, this section wins. The implementer reads
   them with that in mind:
   - `contracts/application-api.md`: "The account store port `IAccountStore` is internal (research R-13)";
   - research.md R-13: the internal port, `InternalsVisibleTo` and the internal constructors;
   - research.md R-16: the guard that resolves `HtDbContext`, and `InternalsVisibleTo` for `HT.UiTests` on
     `IAccountStore`;
   - data-model.md, "Application port": "It is `internal`, visible only to …";
   - plan.md, Constitution Check row III: "The account store port is internal, so no component can reach its
     lookups". Principle III stays met by the public port with T053's rule over every `HT.Ui` type, plus the
     operator-service rule of T090 and T119;
   - plan.md, Source Code ("`IAccountStore (internal)`") and Work order step 3 ("the internal store port").

   The decision rejects an alternative that R-13 had chosen, so the constitution (Decision Discipline) asks
   for an ADR (T002) and a dated line in the spec's `## Clarifications` ("Owner actions").
2. **The `HT-UI` guard (research R-16).** R-16 resolves `HtDbContext` from the factory. With the context
   internal and no `InternalsVisibleTo`, T050 reaches the same context through Identity's public user store
   (`Context`, read as `DbContext`). It checks the same property: the database the host actually uses.

---

## Phase 1: Setup (shared infrastructure)

**Purpose**: packages, tools and configuration the later phases build on.

- [X] T001 Set slice 1's Status to `implementing` and `updated_at` to today in `docs/roadmap.md`
- [X] T002 [P] Write `docs/adr/0010-public-account-store-port.md` in the format of ADR 0009 (Status Accepted, 2026-11-11; Context; Options; Decision; Consequences), recording "Plan discrepancies" 1:
  - the AC export refuses `InternalsVisibleTo` in AC surface projects;
  - the port and the service constructors become public, while `AccountStore`, `HtDbContext`, the design-time factory and the migration stay internal;
  - the replacement rules are T053's rule (no `HT.Ui` type depends on `IAccountStore`) and T090's rule (only `HT.Ui.Operator` uses the operator services), with T026's `AcSurfaceTests`;
  - how constitution III still holds with a public `IAccountStore.FindByIdAsync`: the port is persistence plumbing that only `HT.Application` calls and `HT.Persistence` implements, not a use case. No `HT.Ui` type may depend on it (T053). Every use case for signed-in users still takes the acting user and acts only on its account, and none takes an `AccountId` (T078's reflection test). The plan's internal port had the same method, so no new lookup is added;
  - tests reach the database with plain SQL;
  - the rejected options: the internal port of research R-13 (fails the export), and changing the speckit-ac package to accept `InternalsVisibleTo`.

  It supersedes research R-13's choice for this and later slices. Link it from `docs/roadmap.md`'s decision rows only if the roadmap has a row for it.

  Also draft the dated `## Clarifications` line below and show it to the owner. Do not write it into `spec.md` until the owner approves it, and do it before T127:

  `- Q: Is the account store port internal, given that the AC reference export refuses InternalsVisibleTo? → A: No. IAccountStore and the service constructors are public; architecture rules keep HT.Ui away from the port and from the operator services. Detail: [ADR 0010](../../docs/adr/0010-public-account-store-port.md).`
- [X] T003 [P] Add `dotnet-ef` (the current 10.0.x, `rollForward: false`, like `ilspycmd`) to `dotnet-tools.json`, then run `dotnet tool restore`
- [X] T004 [P] In `src/HT.Application/HT.Application.csproj`, add `Microsoft.Extensions.DependencyInjection.Abstractions` (10.0.x). No `InternalsVisibleTo` (see Conventions and "Plan discrepancies" 1)
- [X] T005 [P] In `src/HT.Persistence/HT.Persistence.csproj`, add `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore` and `Microsoft.EntityFrameworkCore.Design` (`PrivateAssets="all"`). Use the same 10.0.x patch for all three (research R-01). No `InternalsVisibleTo` (see Conventions)
- [X] T006 [P] Add `Microsoft.Extensions.TimeProvider.Testing` (`10.11.0`, see Conventions) to `tests/HT.UnitTests/HT.UnitTests.csproj`
- [X] T007 [P] Configure the host (research R-11):
  - in `src/HT.Ui/HT.Ui.csproj`, add a `<UserSecretsId>` with a new GUID, and an `<InternalsVisibleTo>` item for `HT.UiTests` (for T109's stamp check);
  - in `src/HT.Ui/appsettings.Development.json`, add `ConnectionStrings:HabitTracker` = `Server=.\COLL1;Database=HT;Integrated Security=True;TrustServerCertificate=True`. It holds no secret.

**Checkpoint**: `dotnet build HabitTracker.slnx` passes.

---

## Phase 2: Foundational (blocking prerequisites)

**Purpose**: the design changes, the domain types, the store port and its implementation, the first
migration, the host's session composition, the two new test projects and the restyled shell. Every story
needs them.

**⚠️ CRITICAL**: no story work starts until this phase is complete.

### Design first (constitution IV; spec "Design changes before build")

- [X] T008 [P] In `design/pages/PageSignIn.html` and `.md`, remove "Forgot password?". Replace the note "Built on ASP.NET Core Identity pages" with one saying that Sign in is the app's own page. Show the failure message under the password field with the email kept (HP-003)
- [X] T009 [P] Add the error states of HP-001 to `design/pages/PageRegister.html` and `.md`:
  - taken email: the message next to the email field, with a "Sign in" link;
  - "The passwords do not match." next to the repeated password;
  - invalid email;
  - the password rule.

  The entered email is kept in every state.
- [X] T010 [P] Add the slice-1 state to `design/pages/PageProfile.html` and `.md`:
  - a header with the avatar letter and the email, without a name or "member since";
  - the Account content: the email and the change-password form, with its error states and the notice "Your password is changed." (HP-014);
  - how Profile looks with a single section and no Tabs, because `design/components/Tabs` asks for 2–5 tabs.
- [X] T011 [P] Add the slice-1 state to `design/pages/PageHomeSignedIn.html` and `.md`: the heading "Today" only (HP-013)
- [X] T012 [P] Add the avatar menu (Profile, Sign out), the avatar letter, and the menu's open, closed and focus states to `design/components/TopNav.html` and `.md` and `design/components/MobileNav.html` and `.md`. Include `aria-expanded`, `aria-controls` and `aria-label`, and closing on Esc, on an outside click and on choosing an item (US5-AS2 to AS5). The styles go in T015
- [X] T013 [P] Create `design/pages/PagePrivacy.html` and `.md`. It states what stays private (goals, records, notes, custom habits), that community comparison is opt-in and off by default, and that administrators cannot see private tracking data. Use only SPEC's promises and the brand-book wording (HP-012)
- [X] T014 [P] Add design guidance for the reconnect notice and the unexpected-error bar to `design/components/Notice.html` and `.md`. Add guidance for the Error and NotFound pages (EmptyState in the normal layout, brand-book wording) to `design/components/EmptyState.md`. The styles go in T015
- [X] T015 Add the styles this slice needs to `design/css/ht-components.css`, from existing tokens, and update `design/tokens.json` and `design/css/tokens.css` only if a new token is needed. After T054 the copied design files are the app's only stylesheets, so every rule the shell needs must exist here first (constitution IV). The styles:
  - a floating avatar menu anchored to the TopNav avatar (T012); `.ht-avatar`, `.ht-menu` and `.ht-mobile-menu` do not cover it;
  - a transparent click-away layer for both menus; `.ht-backdrop` is the dialog overlay and is not reused;
  - the unexpected-error bar `#blazor-error-ui`, hidden by default (Blazor shows it by setting `display: block`), with its dismiss control (T014);
  - the reconnect notice `#components-reconnect-modal`, with the per-state visibility rules for the `components-reconnect-*`, `components-pause-visible` and `components-resume-failed-visible` classes that `ReconnectModal.razor.css` holds today (T014).

  This task depends on T012 and T014.
- [X] T016 Update `design/pages.md` with the slice-1 structure of Sign in, Register, Profile, the signed-in Home, the visitor Home (hero and "How it works" only, ADR 0008) and Privacy. In `design/README.md`, add `PagePrivacy` to the page list and remove "password reset pages" from "Not designed yet" (depends on T008–T014 and T015)

### Domain (`src/HT.Domain/Accounts/`, namespace `HT.Domain.Accounts`)

- [X] T017 [P] Create `AccountId.cs` (a readonly record struct over a `Guid`) and `Role.cs` (enum `RegisteredUser`, `Administrator`) in `src/HT.Domain/Accounts/`
- [X] T018 [P] Create `Email.cs` in `src/HT.Domain/Accounts/`. `Email.TryParse(string? text, out Email email)` follows [data-model.md](data-model.md#email):
  - trim, then check at most 254 characters, no `char.IsWhiteSpace`, exactly one `@`, at least one character before it, and a domain split on `.` into at least two parts, all non-empty;
  - the value is the trimmed text in `ToLowerInvariant()` form;
  - equality is ordinal by value (FR-003, FR-006).
- [X] T019 [P] Create `PasswordRule.cs` in `src/HT.Domain/Accounts/`. `IsMet(string? password)` is true for at least 8 characters with at least one `'0'`–`'9'`. No trimming (FR-002)
- [X] T020 [P] Create `SignInBlock.cs` in `src/HT.Domain/Accounts/` as a readonly record struct with:
  - `FailedCount` and `BlockedSince`;
  - `None`, `FailuresToBlock = 5` and `Duration = 5 minutes`;
  - `IsBlockedAt`, `SettledAt`, `AfterFailureAt` and `AfterSuccess`, exactly as the transition table of [data-model.md](data-model.md#signinblock) gives them.

  It reads no clock (FR-008).
- [X] T021 Create `Account.cs` (immutable: `Id`, `Email`, `Role`, `SignInBlock`) and `ActingUser.cs` (`Anonymous`, `ForAccount(AccountId, Role)`, `IsAnonymous`, nullable `AccountId` and `Role`) in `src/HT.Domain/Accounts/` (depends on T017–T020)
- [X] T022 [P] Create `tests/HT.UnitTests/Domain/Accounts/EmailTests.cs`:
  - the valid and invalid examples of data-model.md and US1-AS5;
  - `ana@example..com`, `ana@.example.com` and `ana@example.com.` are invalid;
  - ` Taken@Example.com ` equals `taken@example.com`;
  - a trimmed text of 254 characters is accepted and one of 255 is refused (constitution II, both sides);
  - `null`, `""` and `"   "` are refused.
- [X] T023 [P] Create `tests/HT.UnitTests/Domain/Accounts/PasswordRuleTests.cs`: 7 and 8 characters with a digit, `walkingfar` (no digit), `walk4`, `null`, and leading or trailing spaces that count as characters
- [X] T024 [P] Create `tests/HT.UnitTests/Domain/Accounts/SignInBlockTests.cs` with every row of the transition table, plus:
  - 4 failures are unblocked and the 5th blocks at `t`;
  - blocked at `t + 4:59`, ended at `t + 5:00`;
  - a failure during the block leaves the state unchanged;
  - `SettledAt` at the end resets to `None`;
  - `AfterSuccess` gives `None`.
- [X] T025 [P] Create `tests/HT.UnitTests/Domain/Accounts/ActingUserTests.cs`: `Anonymous` has no id and no role; `ForAccount` keeps both
- [X] T026 [P] Create `tests/HT.ArchitectureTests/ArchitectureModel.cs` with one shared `Architecture` that loads the HT.Domain, HT.Application, HT.Persistence and HT.Ui assemblies, and switch `UiDependencyTests.cs` to it. Then create `tests/HT.ArchitectureTests/DomainDependencyTests.cs`: `HT.Domain` depends on no other `HT.*` assembly and on no `Microsoft.*` type (research R-13). Also create `tests/HT.ArchitectureTests/AcSurfaceTests.cs`: the gate checks both conditions `export_refs.py` checks, so the pre-commit run catches what the export would refuse later (Conventions):
  - the `HT.Domain`, `HT.Application` and `HT.Persistence` assemblies carry no `InternalsVisibleToAttribute`;
  - no `.cs`, `.csproj`, `.props` or `.targets` file under `src/HT.Domain`, `src/HT.Application` or `src/HT.Persistence` (outside `bin/` and `obj/`), and neither root `Directory.Build.props` nor `Directory.Build.targets`, contains the text `InternalsVisibleTo`, not even in a comment. The export rejects the bare word. The test finds the repository root as the folder that holds `HabitTracker.slnx`.

### Application port, texts and composition (`src/HT.Application/`)

- [X] T027 Create `src/HT.Application/Accounts/IAccountStore.cs` as a `public` interface ("Plan discrepancies" 1) with the six methods of [data-model.md](data-model.md#application-port-htapplication-namespace-htapplicationaccounts), and `CreateAccountOutcome.cs` (`Created` with the `Account`, or `EmailTaken`)
- [X] T028 [P] Create `src/HT.Application/Accounts/AccountTexts.cs` and `src/HT.Application/Operator/OperatorTexts.cs`: `public static class` with the `public const string` texts of [contracts/application-api.md](contracts/application-api.md#texts), worded exactly as there
- [X] T029 Create `src/HT.Application/Accounts/AccountService.cs`: a public sealed class with a public constructor `(IAccountStore store, TimeProvider timeProvider)` and no use cases yet. Create `src/HT.Application/ApplicationServiceCollectionExtensions.cs`, where `AddHtApplication` does two things:
  - registers `AccountService` scoped;
  - registers `TimeProvider.System` with `TryAddSingleton`.
- [X] T030 Create `tests/HT.UnitTests/Fakes/FakeAccountStore.cs`, a hand-written in-memory `IAccountStore`:
  - it keys accounts by `Email` and keeps passwords as given;
  - `ChangePasswordAsync` returns `false` on a wrong current password;
  - `TrySaveSignInBlockAsync` compares against `expected`;
  - `LoseNextSaves(int n)` makes the next `n` saves return `false`. Before each lost save it moves the stored state to another unblocked value, alternating between (1, none) and (0, none) as a parallel failure and a parallel success would, so the account never becomes blocked and only the race limit can end the retries;
  - `BlockOnNextSave(DateTimeOffset at)` makes the next save return `false` after storing (5, `at`), as a parallel fifth failure would;
  - `TakeEmailOnNextCreate()` makes the next `CreateAsync` store the account as a parallel registration would, then return `EmailTaken`. `FindByEmailAsync` before it still finds nothing, so `RegisterAsync` reaches the store's `EmailTaken` branch;
  - counters for saves and password checks let tests assert "nothing saved" and "password not checked".
- [X] T031 [P] Create `tests/HT.ArchitectureTests/ApplicationDependencyTests.cs`: `HT.Application` depends only on `HT.Domain` among `HT.*`, and on no type in `Microsoft.EntityFrameworkCore`, `Microsoft.AspNetCore` or `Microsoft.Extensions.Identity`
- [X] T032 [P] Create `tests/HT.ArchitectureTests/ClockReadTests.cs`: no type in `HT.Domain` or `HT.Application` calls `DateTime.Now`, `DateTime.UtcNow`, `DateTime.Today`, `DateTimeOffset.Now`, `DateTimeOffset.UtcNow`, `TimeZoneInfo.Local` or `CultureInfo.CurrentCulture` (constitution II)

### Persistence (`src/HT.Persistence/`)

- [X] T033 [P] Create `src/HT.Persistence/Identity/HtUser.cs`: `HtUser : IdentityUser<Guid>` with `Role`, `FailedSignInCount` and `SignInBlockedSince` (data-model.md)
- [X] T034 [P] Create `src/HT.Persistence/Identity/EmailLookupNormalizer.cs`: an `ILookupNormalizer` that returns the name and the email unchanged (research R-04)
- [X] T035 Create `src/HT.Persistence/HtDbContext.cs`: `internal sealed class HtDbContext : IdentityUserContext<HtUser, Guid>` (depends on T033). No test uses it; tests use plain SQL (Conventions). In `OnModelCreating`:
  - `NormalizedUserName` and `NormalizedEmail` use collation `Latin1_General_100_BIN2`;
  - `Role` is stored as its enum name in `nvarchar(32)`, not null;
  - `FailedSignInCount` defaults to 0;
  - `FailedSignInCount` and `SignInBlockedSince` have `SetAfterSaveBehavior(PropertySaveBehavior.Ignore)` (research R-05).
- [X] T036 Create `src/HT.Persistence/HtDbContextFactory.cs`: an `internal sealed` `IDesignTimeDbContextFactory<HtDbContext>` that reads `HT_DESIGN_CONNECTION`, or uses the local `HT` default (research R-15) (depends on T035)
- [X] T037 Create `src/HT.Persistence/Identity/HtUserClaimsPrincipalFactory.cs`: a `UserClaimsPrincipalFactory<HtUser>` that adds a role claim with the `Role` name to Identity's `NameIdentifier`, `Name` (from `UserName`) and security-stamp claims (research R-07) (depends on T033)
- [X] T038 Create `src/HT.Persistence/Identity/AccountStore.cs`: an `internal sealed` `IAccountStore` with a public constructor taking `HtDbContext` and `UserManager<HtUser>` (research R-05):
  - Reads (`FindByEmailAsync`, `FindByIdAsync`) are `AsNoTracking` and map `HtUser` to `Account`.
  - `CreateAsync`, `CheckPasswordAsync` and `ChangePasswordAsync` call `ChangeTracker.Clear()` before and after their work, also after a failure.
  - `CreateAsync` sets `UserName` and `Email` to the normal form and returns `EmailTaken` in two cases: an Identity `DuplicateUserName` error, or a `DbUpdateException` from the unique index (SQL error 2601 or 2627). Any other Identity error is thrown as a fault.
  - `ChangePasswordAsync` uses `UserManager.ChangePasswordAsync`, which renews the security stamp.
  - `TrySaveSignInBlockAsync` is one `ExecuteUpdateAsync`, filtered on `Id`, the expected count and the expected `SignInBlockedSince` (`IS NULL` for none). It returns whether exactly one row changed.
- [X] T039 Create `src/HT.Persistence/PersistenceServiceCollectionExtensions.cs` with `AddHtPersistence(this IServiceCollection, string connectionString)`. It registers:
  - `HtDbContext` on SQL Server;
  - `AddIdentityCore<HtUser>` with the options of research R-03 and `AddEntityFrameworkStores<HtDbContext>()`;
  - `EmailLookupNormalizer` as `ILookupNormalizer` and `HtUserClaimsPrincipalFactory`;
  - `AccountStore` as a scoped `IAccountStore`.

  It returns `IServiceCollection` and adds no token providers, roles or sign-in manager (research R-02).
- [X] T040 Create `src/HT.Persistence/HtDatabase.cs`:
  - `ResetAsync(string connectionString, CancellationToken)` drops the database and applies all migrations;
  - `Task<int> MigrateAsync(IServiceProvider services, CancellationToken)` counts the pending migrations in a scope, applies them and returns that count (research R-12).
- [X] T041 Run `dotnet ef migrations add InitialAccounts --project src/HT.Persistence --startup-project src/HT.Persistence --output-dir Migrations` to create `src/HT.Persistence/Migrations/`. Change the generated migration class from `public` to `internal` (the model snapshot already is): EF Core finds non-public migrations, and a public migration would put the schema, and a `typeof(HtDbContext)` attribute naming an internal type, into the AC reference listing. Check that the migration:
  - creates `AspNetUsers`, `AspNetUserClaims`, `AspNetUserLogins` and `AspNetUserTokens`, and no role table;
  - creates the three added columns;
  - creates both binary collations and the unique index on `NormalizedUserName`.
- [X] T042 Create the integration-test project:
  - `tests/HT.IntegrationTests/HT.IntegrationTests.csproj`, with xUnit 2.9.3 as in `HT.UnitTests`, `Microsoft.Extensions.TimeProvider.Testing` (`10.11.0`), `Microsoft.Extensions.Logging`, and references to `HT.Domain`, `HT.Application` and `HT.Persistence`. Add it to the `/tests/` folder of `HabitTracker.slnx`.
  - `tests/HT.IntegrationTests/DatabaseFixture.cs`: a collection fixture that calls `HtDatabase.ResetAsync` once per run, and builds services with `new ServiceCollection().AddLogging().AddHtApplication().AddHtPersistence(cs)` and a fake `TimeProvider` registered last. Mark the collection `DisableParallelization = true` (ADR 0006).
  - `tests/HT.IntegrationTests/Sql.cs`: small plain-SQL helpers over the fixture's connection string (`Microsoft.Data.SqlClient`, which comes with `HT.Persistence`'s SQL Server provider). They read an `AspNetUsers` row's columns, delete rows by `UserName`, and check whether a table exists (Conventions).
- [X] T043 Create `tests/HT.IntegrationTests/Accounts/AccountStoreTests.cs` (depends on T038–T042). Each test uses emails of its own:
  - `CreateAsync`, then `FindByEmailAsync` and `FindByIdAsync`, return the same account with role and `SignInBlock.None`;
  - `UserName`, `Email`, `NormalizedUserName` and `NormalizedEmail` all hold the normal form (read with `Sql.cs`);
  - a second `CreateAsync` with the same email gives `EmailTaken`;
  - an apostrophe email (`o'neil@example.com`) and a 254-character email are created;
  - `CheckPasswordAsync` is true for the right password and false for a wrong one;
  - `TrySaveSignInBlockAsync` returns true on the stored state and false on a stale `expected`;
  - a no-tracking read in the same scope sees a block written by `TrySaveSignInBlockAsync`;
  - `ChangePasswordAsync` leaves a stored count of 3 at 3;
  - the database has no `AspNetRoles` table (`Sql.cs`).
- [X] T044 [P] Create `tests/HT.ArchitectureTests/PersistenceDependencyTests.cs`: `HT.Persistence` does not depend on `HT.Ui`

### Host composition (`src/HT.Ui/`)

- [X] T045 Create `src/HT.Ui/Account/AccountSession.cs`, the only code that writes the cookie. It wraps `SignInManager<HtUser>` and `UserManager<HtUser>` with three members ([contracts/host.md](contracts/host.md#accountsession-htuiaccount-the-only-code-that-writes-the-cookie)):
  - `SignInAsync(ActingUser signedIn, bool keepSignedIn)` loads the user by id and calls `SignInAsync(user, isPersistent: keepSignedIn)`;
  - `RefreshSignInAsync(ActingUser actingUser)`;
  - `SignOutAsync()`.
- [X] T046 Compose the session in `src/HT.Ui/Program.cs` (research R-02 and R-09):
  - read `ConnectionStrings:HabitTracker` from `builder.Configuration` before building the services, then call `AddHtApplication()` and `AddHtPersistence(cs)`;
  - add `new IdentityBuilder(typeof(HtUser), builder.Services).AddSignInManager()`;
  - add cookie authentication under `IdentityConstants.ApplicationScheme` with the R-09 options and `OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync`, and set `SecurityStampValidatorOptions.ValidationInterval = TimeSpan.Zero`;
  - add `AddAuthorization`, `AddCascadingAuthenticationState` and scoped `AccountSession`;
  - call `UseAuthentication` and `UseAuthorization` before `UseAntiforgery`;
  - end the file with `public partial class Program;` for `WebApplicationFactory`.
- [X] T047 Create `src/HT.Ui/Operator/StartupTasks.cs` with `RunAsync(IServiceProvider, IHostEnvironment, ILogger)` (logger category `HT.Ui.Operator.StartupTasks`):
  - it reads every operator value from `IConfiguration`, resolved from the `IServiceProvider` it is given (the host registers it; tests register an in-memory one);
  - in Development only, it runs `HtDatabase.MigrateAsync` in its own scope and logs `Schema migrations applied: {Count}`;
  - a fault is caught and logged at Error as `{Step} failed; the app starts without it`, with the exception, and the start goes on (HP-018, HP-022).

  Call it from `Program.cs` after `Build()` and before `Run()`.
- [X] T048 Create the UI-test project:
  - `tests/HT.UiTests/HT.UiTests.csproj`, with xUnit 2.9.3, `bunit` (current stable), `Microsoft.AspNetCore.Mvc.Testing` (10.0.x), `Microsoft.Extensions.TimeProvider.Testing` (`10.11.0`), and references to `HT.Ui`, `HT.Application` and `HT.Persistence`. Link `../HT.UnitTests/Fakes/FakeAccountStore.cs` as a `Compile` item, and add the project to `HabitTracker.slnx`.
  - `tests/HT.UiTests/Host/HostFixture.cs`: one `WebApplicationFactory<Program>` per run in one xUnit collection (research R-16). It sets the environment `Testing` and passes `ConnectionStrings:HabitTracker` and throwaway `Operator:*` values with `builder.UseSetting`. It calls `HtDatabase.ResetAsync` before the host starts, adds a hand-written `ListLoggerProvider` to capture logs (`tests/HT.UiTests/Fakes/ListLogger.cs`, which holds both `ListLoggerProvider` and `ListLogger`), and creates clients with base address `https://localhost`, `AllowAutoRedirect = false` and cookies handled.
- [X] T049 Create `tests/HT.UiTests/Host/HostClient.cs`, helpers for host tests:
  - GET a page, read its `__RequestVerificationToken` and `_handler` form name, and POST the form fields with them;
  - read `Set-Cookie` attributes;
  - create an account directly through the host's public `IAccountStore`, so that a story's host tests do not depend on another story's page;
  - plain-SQL helpers over a given connection string, like `tests/HT.IntegrationTests/Sql.cs` (delete a row by `UserName`, check that a table exists), for the host and `StartupTasks` tests (Conventions).
- [X] T050 [P] Create `tests/HT.UiTests/Host/HostDatabaseGuardTests.cs`. It checks the database the host's own context uses, not the configuration. In a scope of the factory's services:
  - resolve `IUserStore<HtUser>` (Identity's EF store, which `AddHtPersistence` built over `HtDbContext`);
  - read its public `Context` property by reflection as a `DbContext`;
  - take `context.Database.GetDbConnection().Database`.

  Assert that this is the host-test database (`HT-UI`, or the one in `HT_UI_CONNECTION`) and never `HT`. So a setting that reached the configuration too late for `Program.cs`'s top-level read still fails the guard (research R-16; "Plan discrepancies" 2)
- [X] T051 Create `tests/HT.UiTests/Operator/StartupTasksHarness.cs`, used by every `StartupTasks` test, plus a hand-written `tests/HT.UiTests/Fakes/FakeHostEnvironment.cs`:
  - an xUnit collection `StartupTasks` with `DisableParallelization = true`;
  - the `HT-UI-MIGRATE` connection string, derived as in Conventions;
  - `RunAsync(environmentName, IDictionary<string, string?> operatorValues, string? connectionString = null)`. It drops `HT-UI-MIGRATE` first, so every test starts from no database whatever order xUnit runs the classes in. `HtDatabase.ResetAsync` cannot do this, because it also migrates, so the harness drops with SQL of its own:
  - `SqlConnection.ClearAllPools()` first, because earlier runs in the process still pool connections to the database;
  - then, on a connection to `master`: `IF DB_ID(N'HT-UI-MIGRATE') IS NOT NULL BEGIN ALTER DATABASE [HT-UI-MIGRATE] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [HT-UI-MIGRATE]; END`.

  It also offers `DatabaseExists()` (`SELECT DB_ID(N'HT-UI-MIGRATE')` on `master`). It builds services from `new ServiceCollection().AddLogging().AddHtApplication().AddHtPersistence(cs)` plus `AddSingleton<IConfiguration>` over `ConfigurationBuilder().AddInMemoryCollection(operatorValues)`. It calls `StartupTasks.RunAsync` with a `ListLogger` and returns the captured entries;
  - a test that runs startup twice passes its second run through an overload that does not drop the database.
- [X] T052 Create `tests/HT.UiTests/Operator/StartupTasksMigrationTests.cs` (HP-022) in the `StartupTasks` collection, using T051's harness (depends on T051):
  - with `Development`, the log has `Schema migrations applied:` with a count above 0, and `AspNetUsers` exists (T049's SQL helper);
  - with `Testing` and no operator values, no migration is applied: there is no `Schema migrations applied:` line, and `DatabaseExists()` is false. Administrator creation and demo loading return their "not configured" results before they touch the store.
- [X] T053 [P] In `tests/HT.ArchitectureTests/UiDependencyTests.cs`, add two rules:
  - types in `HT.Ui.Components` do not depend on `SignInManager<>` or `UserManager<>` (research R-13);
  - no type of the `HT.Ui` assembly depends on `IAccountStore`, which replaces R-13's internal port ("Plan discrepancies" 1).

### Shell and shared components (`src/HT.Ui/`)

- [X] T054 Replace the template styles (depends on T015):
  - copy `design/css/tokens.css` and `design/css/ht-components.css` to `src/HT.Ui/wwwroot/css/` unchanged; any later style change is made in `design/css/` and copied again;
  - in `src/HT.Ui/Components/App.razor`, link both instead of `app.css` and `HT.Ui.styles.css`;
  - delete `src/HT.Ui/wwwroot/app.css`, `src/HT.Ui/Components/Layout/MainLayout.razor.css` and `src/HT.Ui/Components/Layout/ReconnectModal.razor.css`.
- [X] T055 [P] Create the shared components in `src/HT.Ui/Components/Shared/` from their `design/components/` markup:
  - `HtButton.razor`;
  - `HtCard.razor`;
  - `TextField.razor`: label, help text, an error message next to the field, and `aria-describedby` that links help and error;
  - `Notice.razor`;
  - `Avatar.razor`: the first character of the email, upper-cased with `ToUpperInvariant`, and an `aria-label`;
  - `EmptyState.razor`.
- [X] T056 Restyle the layout from the design and T014's guidance: `src/HT.Ui/Components/Layout/MainLayout.razor` (a `main` landmark, a navigation slot and a footer slot), `ReconnectModal.razor` (the reconnect notice) and the unexpected-error bar (depends on T054, T055)
- [X] T057 Rebuild `src/HT.Ui/Components/Pages/NotFound.razor` and `Error.razor` as EmptyState in the normal layout, with brand-book wording. The Error page shows no request id, exception detail or configuration value (HP-016, HP-017) (depends on T055)
- [X] T058 Create `tests/HT.UiTests/Components/Shared/SharedComponentTests.cs` (bUnit) (depends on T055):
  - `TextField` renders its label, help text and error with `aria-describedby`;
  - `Avatar` shows "A" for `ana@example.com`;
  - `EmptyState` renders its heading and text.
- [X] T059 Create `tests/HT.UiTests/Host/ErrorPagesTests.cs` (depends on T057):
  - an unknown path gives 404 with the NotFound content inside the layout;
  - GET `/Error` renders the EmptyState in the layout, without "Request ID", "Development Mode", `ASPNETCORE_ENVIRONMENT`, the host's connection string or any `Operator:*` value from `HostFixture` (HP-017).
- [X] T060 [P] Create `tests/HT.UiTests/DesignSyncTests.cs`: `src/HT.Ui/wwwroot/css/tokens.css` and `ht-components.css` equal their `design/css/` sources, compared with line endings normalised. The test finds the repository root as the folder that holds `HabitTracker.slnx`. A style written only in the app copy therefore fails the build gate (constitution IV, V) (depends on T054)

**Checkpoint**: `dotnet build HabitTracker.slnx` and `dotnet test HabitTracker.slnx` pass. A local `dotnet run --project src/HT.Ui --launch-profile https` migrates the `HT` database and serves the restyled shell.

---

## Phase 3: User Story 1 - Register an account (Priority: P1) 🎯 MVP

**Goal**: an anonymous visitor registers with an email and a password, gets a registered-user account and
is signed in.

**Independent Test**: register `new.user@example.com` with `walking42`. The account exists with the role
registered user, and the store accepts `walking42` as its password. Each refusal of US1-AS2 to AS6 gives
its exact text.

### Tests for User Story 1

- [X] T061 [P] [US1] Create `tests/HT.UnitTests/Application/Accounts/RegisterTests.cs`. Use `AccountService` over `FakeAccountStore` and a fake clock:
  - each of US1-AS1 to AS6 with its literal values and texts;
  - the check order InvalidEmail → PasswordRuleNotMet → EmailTaken;
  - ` Taken@Example.com ` is refused as taken;
  - a new account is stored in normal form with `Role.RegisteredUser`;
  - 254 characters are accepted and 255 refused;
  - a lost race (`TakeEmailOnNextCreate`) gives `EmailTaken` with FR-003's text, and the store holds exactly one account for that email;
  - `Message` is null and `AccountId` is set for `Registered`.
- [X] T062 [P] [US1] Create `tests/HT.IntegrationTests/Accounts/RegistrationTests.cs` against the real database:
  - registering `Mixed.Case@Example.com` stores `mixed.case@example.com`, with the role registered user;
  - HP-020: two `RegisterAsync` calls with one email, started together in two scopes, give exactly one `Registered`, one `EmailTaken` and one row.

### Implementation for User Story 1

- [X] T063 [US1] Create `src/HT.Application/Accounts/RegistrationResult.cs`, with `RegistrationOutcome` (`InvalidEmail`, `PasswordRuleNotMet`, `EmailTaken`, `Registered`), `Message` and `AccountId?` ([contracts/application-api.md](contracts/application-api.md#result-shape))
- [X] T064 [US1] Add `RegisterAsync(string? email, string? password, CancellationToken)` to `src/HT.Application/Accounts/AccountService.cs`, following FR-001 to FR-004. It checks `Email.TryParse`, then `PasswordRule.IsMet`, then `FindByEmailAsync`, then `CreateAsync(email, Role.RegisteredUser, password)`. A store `EmailTaken` gives `EmailTaken` (depends on T063)
- [X] T065 [US1] Create `src/HT.Ui/Components/Account/RegisterForm.razor`, a static SSR `EditForm` with `method="post"`, `FormName="register"` and `[SupplyParameterFromForm]`. Initialise its model when it is null. It has:
  - the fields `Email`, `Password` (the password rule as help text) and `RepeatPassword`;
  - the neutral notice "Community comparison is off. You can opt in later in your profile.";
  - the button "Create account".

  On submit:
  - it checks the repeated password first and shows "The passwords do not match." next to it;
  - otherwise it calls `AccountService.RegisterAsync` and shows each refusal next to its field, with a "Sign in" link (`/sign-in`) for a taken email;
  - it keeps the entered email in every case;
  - on `Registered` it raises `OnSucceeded` with the result (HP-001, research R-08).
- [X] T066 [US1] Create `src/HT.Ui/Components/Pages/Register.razor` (`@page "/register"`), following `design/pages/PageRegister`:
  - it redirects a signed-in user to `/`;
  - it hosts `RegisterForm`;
  - in `OnSucceeded` it calls `AccountSession.SignInAsync(ActingUser.ForAccount(id, Role.RegisteredUser), keepSignedIn: false)` and redirects to `/` (HP-002; plan, spec discrepancy 2).

### UI tests for User Story 1

- [X] T067 [P] [US1] Create `tests/HT.UiTests/Components/Account/RegisterFormTests.cs` (bUnit, the real `AccountService` over `FakeAccountStore`):
  - the help text and the neutral notice are shown;
  - each refusal appears next to its field, with the email kept;
  - a taken email shows the "Sign in" link;
  - mismatched passwords show "The passwords do not match." and the use case is not called;
  - `OnSucceeded` fires once on success.
- [X] T068 [P] [US1] Create `tests/HT.UiTests/Host/RegisterPageTests.cs`:
  - a valid POST gives 302 to `/` and an `Identity.Application` cookie that is `Secure`, `HttpOnly`, `SameSite=Lax` and has no `Expires` (session cookie);
  - a signed-in GET `/register` gives 302 to `/`;
  - an invalid POST gives 200 with the message and the email kept;
  - two parallel POSTs of one new email leave exactly one account (HP-020).

**Checkpoint**: US1's tests pass on their own. In the running app, registering creates the account, sets
the session cookie and redirects to `/`. Until T108 (US6), `/` still shows the template page instead of
the signed-in Home of US1-AS10 and HP-002, so that scenario is verified only at US6's checkpoint.

---

## Phase 4: User Story 2 - Sign in and sign out (Priority: P1)

**Goal**: an account signs in by email and password with the 5-minute block, and signs out. Every failure
gives the same message.

**Independent Test**: sign in to an existing account; fail five times and see the block until exactly 5
minutes have passed; sign out and be anonymous again.

### Tests for User Story 2

- [X] T069 [P] [US2] Create `tests/HT.UnitTests/Application/Accounts/SignInTests.cs` (`FakeAccountStore`, `FakeTimeProvider` set to each scenario's times):
  - US2-AS1 to AS8 with their literal values;
  - an unknown email gives `Failed` and saves nothing;
  - a blocked account gives `Failed` with no password check;
  - one lost race is retried with the same password verdict;
  - `BlockOnNextSave`: a re-read that finds the account blocked gives `Failed` with nothing more saved;
  - the race limit, both sides (constitution II), with the correct password and `LoseNextSaves`: 9 lost races then a won save gives `SignedIn`; 10 lost races give `Failed` with no save won;
  - the text is identical for an unknown email, a wrong password and a block (SC-004);
  - `SignedIn` carries the account's `ActingUser` with its role, and its `Message` is null.
- [X] T070 [P] [US2] Create `tests/HT.IntegrationTests/Accounts/SignInBlockTests.cs`:
  - 10 wrong attempts started in parallel in separate scopes at one fake time leave count 5 and `SignInBlockedSince` set;
  - the correct password at that time fails, and succeeds at +5:00;
  - `ANA@example.com` signs in to `ana@example.com`'s account.

### Implementation for User Story 2

- [X] T071 [US2] Create `src/HT.Application/Accounts/SignInResult.cs`, with `SignInOutcome` (`Failed`, `SignedIn`), `Message` and `ActingUser?`
- [X] T072 [US2] Add `SignInAsync(string? email, string? password, CancellationToken)` to `src/HT.Application/Accounts/AccountService.cs`. It follows the five steps of research R-05:
  - it reads `t` from the `TimeProvider` once per attempt;
  - an invalid or unknown email gives `Failed`;
  - it settles the state, and a blocked account gives `Failed`;
  - it checks the password once;
  - it saves the next state with compare-and-swap, re-reading and re-settling at the same `t` after a lost race;
  - after 10 lost races it fails closed (FR-005 to FR-008; the text is the same for every failure, FR-007).

  It depends on T071.
- [X] T073 [US2] Create `src/HT.Ui/Components/Account/SignInForm.razor`, a static SSR `EditForm` with `FormName="sign-in"`. It has:
  - the fields `Email`, `Password` and `KeepSignedIn` (labelled "Keep me signed in"), the button "Sign in", and "New here? Create an account" (`/register`);
  - no "Forgot password?".

  It calls `AccountService.SignInAsync`. A failure shows the single message under the password field, with the email kept. On `SignedIn` it raises `OnSucceeded` with the result and `KeepSignedIn` (HP-003).
- [X] T074 [US2] Create `src/HT.Ui/Components/Pages/SignIn.razor` (`@page "/sign-in"`):
  - it reads the `returnUrl` query parameter and redirects a signed-in user to `/`;
  - in `OnSucceeded` it calls `AccountSession.SignInAsync(actingUser, keepSignedIn)`;
  - it then redirects to `returnUrl` only when `RedirectHttpResult.IsLocalUrl(returnUrl)` is true, otherwise to `/` (HP-005, research R-10).
- [X] T075 [US2] Create `src/HT.Ui/Account/SignOutEndpoint.cs`, with `MapSignOut(this IEndpointRouteBuilder)`. It maps `POST /account/sign-out` with antiforgery validation, calls `AccountSession.SignOutAsync()` and redirects to `/`. Call it in `src/HT.Ui/Program.cs` (HP-006)

### UI tests for User Story 2

- [X] T076 [P] [US2] Create `tests/HT.UiTests/Components/Account/SignInFormTests.cs` (bUnit):
  - "Keep me signed in" is present and "Forgot password?" is absent;
  - a failure shows "Email or password is not correct." under the password field, with the email kept;
  - `OnSucceeded` carries the `KeepSignedIn` value.
- [X] T077 [P] [US2] Create `tests/HT.UiTests/Host/SignInPageTests.cs`:
  - with "Keep me signed in", the cookie has an `Expires` about 14 days ahead; without it, there is no `Expires`;
  - an authenticated request re-issues the cookie with a full window (HP-004);
  - `returnUrl` `/profile` goes to `/profile`, while `//evil.example`, `/\evil.example`, `https://evil.example` and an empty value go to `/`;
  - a signed-in GET `/sign-in` gives 302 to `/`;
  - `POST /account/sign-out` clears the cookie and redirects to `/`, while a second client signed in to the same account stays signed in (HP-006);
  - a sign-out POST without an antiforgery token is refused.

**Checkpoint**: US1's and US2's tests pass, including the sign-out host test (T077). In the running app a
user can register and sign in. Sign out has no control on a page until `SiteNav` (T092, US5), so a manual
sign-out waits for US5.

---

## Phase 5: User Story 3 - Access by audience (Priority: P1)

**Goal**: every use case for signed-in users knows the acting user. The anonymous visitor is refused, and
no role reads another account.

**Independent Test**: read account details as an anonymous visitor (refused), as a registered user and as
an administrator (each gets only their own email and role).

### Tests for User Story 3

- [X] T078 [P] [US3] Create `tests/HT.UnitTests/Application/Accounts/AccountDetailsTests.cs`:
  - US3-AS1 to AS5 with their literal values;
  - an acting user whose account no longer exists gets `NotSignedIn`;
  - a reflection test checks that no public method of `AccountService` takes an `AccountId` or `Guid` parameter, which keeps FR-010 visible (SC-005).
- [X] T079 [P] [US3] Create `tests/HT.IntegrationTests/Accounts/AccountDetailsTests.cs`: US3-AS4 and AS5 against the real store. A registered user and an administrator each get only their own normal-form email and role
- [X] T080 [P] [US3] Create `tests/HT.IntegrationTests/Accounts/ClaimsPrincipalFactoryTests.cs`: the `IUserClaimsPrincipalFactory<HtUser>` from `AddHtPersistence` issues `NameIdentifier` = the id, `Name` = the normal-form email, the role claim for each role, and the security-stamp claim

### Implementation for User Story 3

- [X] T081 [US3] Create `src/HT.Application/Accounts/AccountDetailsResult.cs` (`AccountDetailsOutcome`: `NotSignedIn`, `Found`; `Message`; `AccountDetails?`) and `AccountDetails.cs` (`Email` as `string` in normal form, `Role`)
- [X] T082 [US3] Add `GetAccountDetailsAsync(ActingUser actingUser, CancellationToken)` to `src/HT.Application/Accounts/AccountService.cs`. An anonymous visitor, or an account that is not found, gives `NotSignedIn` with "You are not signed in.". Otherwise it is `Found` with the store's account for `actingUser.AccountId` only (FR-009 to FR-012, FR-015; SC-003) (depends on T081)
- [X] T083 [US3] Create `src/HT.Ui/Account/ActingUserAccessor.cs`. It turns the current `ClaimsPrincipal` into `ActingUser`: `ForAccount` from `NameIdentifier` and the role claim, and `Anonymous` when the principal is not authenticated or a claim is missing or invalid (research R-07). Register it scoped in `src/HT.Ui/Program.cs`

### UI tests for User Story 3

- [X] T084 [P] [US3] Create `tests/HT.UiTests/Account/ActingUserAccessorTests.cs`: an authenticated principal for each role, an anonymous principal, and principals with a missing or malformed id or role

**Checkpoint**: US1–US3 (the P1 MVP) pass. The page side of US3 (Profile's redirect, AS6) is tested with
Profile in US6; AS7 is covered by T068 and T077.

---

## Phase 6: User Story 4 - First administrator (Priority: P2)

**Goal**: at every start, administrator creation runs with the configured email and password and logs its
result, never a password.

**Independent Test**: run administrator creation with an email and a password, sign in as the
administrator, run it again and see that nothing changed.

### Tests for User Story 4

- [X] T085 [P] [US4] Create `tests/HT.UnitTests/Application/Operator/AdministratorCreationTests.cs`:
  - US4-AS1 to AS9 with their literal values and texts;
  - the check order of FR-014;
  - "missing" is `null`, `""` or spaces only, for either value;
  - an existing administrator's password is unchanged;
  - registering `fifth.admin@example.com` and `seventh.admin@example.com` afterwards succeeds.
- [X] T086 [P] [US4] Create `tests/HT.IntegrationTests/Operator/AdministratorCreationTests.cs`:
  - a created administrator signs in through `AccountService.SignInAsync` with the role administrator;
  - a second run gives `AlreadyExists` and the first password still signs in;
  - a new email adds a second administrator, and both keep the role.

### Implementation for User Story 4

- [X] T087 [US4] Create `src/HT.Application/Operator/AdministratorCreationResult.cs`, with `AdministratorCreationOutcome` (`NotConfigured`, `InvalidEmail`, `AlreadyExists`, `BelongsToRegisteredUser`, `PasswordRuleNotMet`, `Created`) and `Message`
- [X] T088 [US4] Create `src/HT.Application/Operator/OperatorService.cs` with a public constructor `(IAccountStore)` and `CreateAdministratorAsync(string? email, string? password, CancellationToken)`, checking in FR-014's order. Register it scoped in `src/HT.Application/ApplicationServiceCollectionExtensions.cs` (depends on T087)
- [X] T089 [US4] Add the administrator step to `src/HT.Ui/Operator/StartupTasks.cs`. After migrations, in its own scope and `try/catch`, it reads `Operator:AdministratorEmail` and `Operator:AdministratorPassword` from `IConfiguration` and calls `CreateAdministratorAsync`. It logs `Administrator creation: {Result} ({Email})`, leaving out `({Email})` when no email is configured. A fault is logged as in T047 (HP-018)
- [X] T090 [P] [US4] Add a rule to `tests/HT.ArchitectureTests/UiDependencyTests.cs`, scoped to the types of the `HT.Ui` assembly: among them, only types in `HT.Ui.Operator` depend on `OperatorService`, so `HT.Ui.Components` cannot. `HT.Application`'s own `AddHtApplication`, which registers the service, is outside the rule's scope (research R-13)

### UI tests for User Story 4

- [X] T091 [US4] Create `tests/HT.UiTests/Operator/StartupTasksAdministratorTests.cs`, in the `StartupTasks` collection, using T051's harness. Each test starts from a dropped `HT-UI-MIGRATE` and runs `Development`, so migrations run first:
  - `Operator:AdministratorEmail` and `Operator:AdministratorPassword` set log `Administrator creation: administrator created (…)`;
  - an empty dictionary logs `Administrator creation: no administrator configured` without parentheses;
  - no captured log entry contains the configured password;
  - with an unreachable connection string passed to the harness (a server name that does not resolve, `Connect Timeout=2`), `RunAsync` returns normally after an Error entry `… failed; the app starts without it`.

  Also create `tests/HT.UiTests/Host/StartupLogTests.cs` in the host collection: once the factory's host has started (after the fixture's first `CreateClient`), the fixture's captured log has an `Administrator creation:` line. This is the check that `Program.cs` runs the startup tasks at all (HP-018). `HostFixture`'s throwaway operator values meet the password rule.

**Checkpoint**: US4 passes. A first start of the running app logs the administrator result.

---

## Phase 7: User Story 5 - Shared shell and visitor Home (Priority: P2)

**Goal**: one shell (TopNav, the narrow-screen menu and Footer) shows only shipped links for each
audience. The visitor Home and Privacy are readable by anyone.

**Independent Test**: open Home as an anonymous visitor and as a signed-in user, on a wide and a narrow
screen. Check the navigation variant, the footer, Privacy and the visitor Home sections.

### Implementation for User Story 5

- [X] T092 [US5] Create `src/HT.Ui/Components/Layout/SiteNav.razor`, the only interactive component (`@rendermode InteractiveServer`). It renders TopNav and MobileNav from T012's markup and reads the cascading authentication state (research R-08):
  - the visitor sees "Habit Tracker" (→ `/`), "Sign in" and "Register";
  - a registered user or an administrator sees "Habit Tracker", "Today" and the `Avatar` menu with "Profile" and "Sign out", and no Admin, Catalog, Community, Goals or "+ New goal";
  - the narrow-screen menu lists the same items;
  - each menu button has `aria-expanded`, `aria-controls` and `aria-label`;
  - a menu closes on Esc (`@onkeydown`), on a click outside (a backdrop element) and on choosing an item, and focus returns to its button (`ElementReference.FocusAsync`);
  - Sign out is a form with `<AntiforgeryToken />` posting to `/account/sign-out` (HP-010).
- [X] T093 [P] [US5] Create `src/HT.Ui/Components/Layout/Footer.razor`, with the project line "Habit Tracker · Agentic Engineering course project" and a "Privacy" link (`/privacy`), and no "Send feedback"
- [X] T094 [US5] Put `SiteNav` and `Footer` in their slots in `src/HT.Ui/Components/Layout/MainLayout.razor` (depends on T092, T093)
- [X] T095 [US5] Build the visitor variant of `src/HT.Ui/Components/Pages/Home.razor` (`@page "/"`, the `NotAuthorized` content of an `AuthorizeView`) with the wording of `design/pages/PageHomeVisitor`. It has:
  - the hero: the headline "Keep the habits you choose", its one sentence, the primary "Register free" (→ `/register`) and the privacy line;
  - the three "How it works" steps;
  - nothing else from the preview (HP-009, HP-011, ADR 0008).
- [X] T096 [P] [US5] Create `src/HT.Ui/Components/Pages/Privacy.razor` (`@page "/privacy"`, anonymous access) from `design/pages/PagePrivacy` (HP-012)

### UI tests for User Story 5

- [X] T097 [P] [US5] Create `tests/HT.UiTests/Components/Layout/SiteNavTests.cs` (bUnit with test authorization):
  - the items for each of the three audiences (US5-AS1 to AS3), with the absent links asserted;
  - the narrow menu lists the same items (AS4);
  - the avatar menu shows "A" for `ana@example.com`;
  - Esc, an outside click and choosing an item close each menu (AS5), and focus returns to its button;
  - `aria-expanded` follows the menu state;
  - the sign-out form posts to `/account/sign-out` with an antiforgery field.
- [X] T098 [P] [US5] Create `tests/HT.UiTests/Components/Layout/FooterTests.cs`: the project line and the "Privacy" link, and no "Send feedback" (US5-AS6)
- [X] T099 [P] [US5] Create `tests/HT.UiTests/Components/Pages/HomeVisitorTests.cs`: the hero texts, "Register free" links to `/register`, the three steps, and the absence of "Browse the catalog", the example goal card, the catalog preview and the bands (US5-AS8, AS9)
- [X] T100 [P] [US5] Create `tests/HT.UiTests/Components/Pages/PrivacyTests.cs`: the page states the three promises of HP-012
- [X] T101 [P] [US5] Create `tests/HT.UiTests/Host/ShellTests.cs`:
  - an anonymous GET `/` returns 200 with "Sign in", "Register" and the footer's "Privacy";
  - GET `/privacy` returns 200 without a cookie (US5-AS7, AS10).

**Checkpoint**: US5 passes. Every page has the shell.

---

## Phase 8: User Story 6 - Signed-in Home and Profile Account tab (Priority: P3)

**Goal**: a signed-in user lands on "Today" and changes their password on Profile. Their other sessions lose
access.

**Independent Test**: sign in, read the account details on Profile, change the password, then sign in with
the new password.

### Tests for User Story 6

- [X] T102 [P] [US6] Create `tests/HT.UnitTests/Application/Accounts/ChangePasswordTests.cs`:
  - US6-AS1 to AS7 with their literal values and texts;
  - the check order NotSignedIn → CurrentPasswordWrong → PasswordRuleNotMet;
  - an acting user whose account no longer exists gets `NotSignedIn`, not `CurrentPasswordWrong` ([contracts/application-api.md](contracts/application-api.md#accountservice-anonymous-visitors-and-signed-in-users));
  - only the acting user's password changes;
  - `Message` is null for `Changed` (FR-011, SC-005).
- [X] T103 [P] [US6] Create `tests/HT.IntegrationTests/Accounts/PasswordChangeTests.cs`:
  - after a change, the new password signs in and the old one fails;
  - `SecurityStamp` changed;
  - a stored failed count of 3 is still 3 (research R-05).

### Implementation for User Story 6

- [X] T104 [US6] Create `src/HT.Application/Accounts/PasswordChangeResult.cs`, with `PasswordChangeOutcome` (`NotSignedIn`, `CurrentPasswordWrong`, `PasswordRuleNotMet`, `Changed`) and `Message`
- [X] T105 [US6] Add `ChangePasswordAsync(ActingUser actingUser, string? currentPassword, string? newPassword, CancellationToken)` to `src/HT.Application/Accounts/AccountService.cs`. An anonymous visitor, or an `actingUser.AccountId` for which `FindByIdAsync` finds no account, gives `NotSignedIn`. It then checks in FR-013's order: `CheckPasswordAsync`, then `PasswordRule.IsMet`, then the store's `ChangePasswordAsync`. A `false` from the store gives `CurrentPasswordWrong` (depends on T104)
- [X] T106 [US6] Create `src/HT.Ui/Components/Account/ChangePasswordForm.razor` (`FormName="change-password"`) with `CurrentPassword`, `NewPassword` (the rule as help text) and `RepeatNewPassword`:
  - a mismatch shows "The passwords do not match." next to the repeated password, before the use case is called;
  - refusals appear next to the current or the new password;
  - on `Changed` it shows "Your password is changed.", empties all three fields and raises `OnSucceeded`.

  It takes the `ActingUser` as a parameter (HP-014).
- [X] T107 [US6] Create `src/HT.Ui/Components/Pages/Profile.razor` (`@page "/profile"`, `[Authorize]`), following T010's design. It:
  - gets the `ActingUser` from `ActingUserAccessor` and calls `GetAccountDetailsAsync`;
  - shows the header (the `Avatar` letter and the email, no name, no "member since") and the Account content (the email and `ChangePasswordForm`), as a single section without Tabs;
  - in `OnSucceeded`, calls `AccountSession.RefreshSignInAsync(actingUser)` (HP-007, HP-014).
- [X] T108 [US6] Add the signed-in variant to `src/HT.Ui/Components/Pages/Home.razor` (the `Authorized` content): the heading "Today" only, with no greeting, name, date, stat tiles, Today list, goal summary or notes (HP-013, HP-015)
- [X] T109 [US6] Create `src/HT.Ui/Account/IdentityRevalidatingAuthenticationStateProvider.cs`, a `RevalidatingServerAuthenticationStateProvider` with `RevalidationInterval = 30 minutes`. Its protected `ValidateAuthenticationStateAsync` calls an `internal` method `IsSecurityStampCurrentAsync(ClaimsPrincipal, CancellationToken)`, which compares the principal's security-stamp claim with `UserManager.GetSecurityStampAsync` in a new scope and is false for a missing account. `HT.UiTests` reaches it through T007's `InternalsVisibleTo`. Register it in `src/HT.Ui/Program.cs` (HP-007, research R-09)

### UI tests for User Story 6

- [X] T110 [P] [US6] Create `tests/HT.UiTests/Components/Account/ChangePasswordFormTests.cs` (bUnit):
  - each refusal appears next to its field;
  - a mismatch shows "The passwords do not match." and the use case is not called;
  - success shows "Your password is changed." and empty fields, and `OnSucceeded` fires.
- [X] T111 [P] [US6] Create `tests/HT.UiTests/Components/Pages/HomeSignedInTests.cs`: a signed-in user sees the heading "Today" and none of the absent parts of US6-AS10
- [X] T112 [P] [US6] Create `tests/HT.UiTests/Account/RevalidationProviderTests.cs` in the host collection (T048), using the factory's services and the `HT-UI` database. It creates an account through `IAccountStore`, builds its principal with the claims factory, and calls `IsSecurityStampCurrentAsync`:
  - an unchanged stamp is current;
  - after the store's `ChangePasswordAsync`, the stamp is not current;
  - after the account's row is deleted with T049's SQL helper, the stamp is not current;
  - `RevalidationInterval` is 30 minutes, read by reflection from the protected property (`BindingFlags.NonPublic | BindingFlags.Instance`), because an override cannot widen its access.
- [X] T113 [P] [US6] Create `tests/HT.UiTests/Host/ProfilePageTests.cs`. Sign in through the Sign in page of US2 using `HostClient`:
  - an anonymous GET `/profile` gives 302 to `/sign-in?returnUrl=%2Fprofile` with no Profile content (US3-AS6, HP-008);
  - a signed-in GET `/profile` shows the avatar letter and the email, and no "member since";
  - `/profile` and `/` for a signed-in user have `Cache-Control` with `no-store` (HP-008, research R-09);
  - after client A changes the password, client A stays signed in, while client B (signed in with "Keep me signed in") is redirected to `/sign-in` on its next GET `/profile` (HP-007);
  - after sign-out, GET `/profile` is redirected again.

**Checkpoint**: US6 passes. Profile works and a password change ends the other sessions.

---

## Phase 9: User Story 7 - Demonstration accounts (Priority: P3)

**Goal**: demo loading creates three synthetic `example.com` accounts at every start, re-runnably, and
logs per-email results without the password.

**Independent Test**: load the demo data with a demo password, sign in as a demo account, load again and
see that nothing is duplicated or changed.

### Tests for User Story 7

- [X] T114 [P] [US7] Create `tests/HT.UnitTests/Application/Operator/DemoLoadingTests.cs`:
  - US7-AS1 to AS4 with their literal values and texts;
  - an existing account of either role is reported `AlreadyExists` and left unchanged;
  - a second run leaves the same accounts and passwords (SC-006);
  - `DemoEmails` are exactly `demo1@example.com`, `demo2@example.com` and `demo3@example.com`, in that order, all in `example.com` (US7-AS5);
  - `Message` is null for `Loaded`.
- [X] T115 [P] [US7] Create `tests/HT.IntegrationTests/Operator/DemoLoadingTests.cs` on the real database. Clear the three demo emails first with `Sql.cs` (`DELETE` by `UserName`), so the test is independent of other tests:
  - load, change `demo3@example.com`'s password, then load again;
  - the second load gives "already exists" three times, still three accounts, and the changed password still signs in.

### Implementation for User Story 7

- [X] T116 [US7] Create `src/HT.Application/Operator/DemoLoadingResult.cs` (`DemoLoadingOutcome`: `NoPassword`, `PasswordRuleNotMet`, `Loaded`; `Message`; `IReadOnlyList<DemoAccountResult>`) and `DemoAccountResult.cs` (`Email` as `string`, `Outcome`: `Created` or `AlreadyExists`, `Message`)
- [X] T117 [US7] Create `src/HT.Application/Operator/DemoDataService.cs` with a public constructor `(IAccountStore)`, `public static IReadOnlyList<string> DemoEmails`, and `LoadAsync(string? demoPassword, CancellationToken)`, following FR-016. Register it scoped in `src/HT.Application/ApplicationServiceCollectionExtensions.cs` (depends on T116)
- [X] T118 [US7] Add the demo step to `src/HT.Ui/Operator/StartupTasks.cs`, right after administrator creation, in its own scope and `try/catch`. It reads `Operator:DemoPassword` and logs:
  - `Demo loading: {Result}` for `NoPassword` and `PasswordRuleNotMet`;
  - `Demo account {Email}: {Result}`, one line per email, for `Loaded`.

  A fault is logged as in T047 (HP-021).
- [X] T119 [P] [US7] In `tests/HT.ArchitectureTests/UiDependencyTests.cs`, extend T090's rule to `DemoDataService`, with the same scope: the types of the `HT.Ui` assembly
- [X] T120 [P] [US7] Add "Demo account" to `## Glossary` in `CONTEXT.md`: a synthetic account created by demo loading, NOT a real person's account (constitution I)

### UI tests for User Story 7

- [X] T121 [US7] Create `tests/HT.UiTests/Operator/StartupTasksDemoTests.cs`, in the `StartupTasks` collection, using T051's harness. Each test starts from a dropped `HT-UI-MIGRATE`, so the three demo emails have no account:
  - `Development` with `Operator:DemoPassword` set logs three `Demo account demo1@example.com: created`-style lines after the `Administrator creation:` line;
  - a second run through the harness's no-drop overload logs `already exists` for all three;
  - no `Operator:DemoPassword` logs `Demo loading: no demo password configured`;
  - no captured entry contains the demo password;
  - a fault in the demo step still lets `RunAsync` return (HP-021).

  Also add to `tests/HT.UiTests/Host/StartupLogTests.cs` (T091): the host's start logged three `Demo account …:` lines after the `Administrator creation:` line (HP-021).

**Checkpoint**: all seven stories pass. The running app logs the full startup sequence of quickstart.md.

---

## Phase 10: Polish and cross-cutting concerns

- [X] T122 Update `.github/workflows/sonar.yml` (research R-14):
  - the `Check prerequisites` step also requires the secret `CI_SQL_SA_PASSWORD` and skips with a notice when it or `SONAR_TOKEN` is missing;
  - after that check, under the same condition, a step runs `mcr.microsoft.com/mssql/server:2022-latest` with `ACCEPT_EULA=Y` and the SA password from the secret, then waits in a readiness loop;
  - the test step gets `HT_IT_CONNECTION` and `HT_UI_CONNECTION` built from the secret;
  - nothing echoes the value;
  - the file's setup comment lists the new repository secret and says the job also skips without it, and the inline comment at the analysis step ("a run without SONAR_TOKEN succeeds without analyzing") names both secrets;
  - `.claude/skills/process-pr-feedback/SKILL.md`: the `SKIPPED` gate state and its likely user action name both `SONAR_TOKEN` and `CI_SQL_SA_PASSWORD`.
- [X] T123 [P] Update `README.md`:
  - the "PR review flow" gate row: a run without `SONAR_TOKEN` or `CI_SQL_SA_PASSWORD` is skipped, not green;
  - prerequisites: SQL Server `.\COLL1` or the `HT_*_CONNECTION` variables, and the three `Operator:*` User Secrets names, without values;
  - getting started: `dotnet run --project src/HT.Ui --launch-profile https`;
  - the layer-rules and status sentences for slice 1.
- [X] T124 [P] Update "Build and test" in `CLAUDE.md`: run the UI with `--launch-profile https`; the solution's tests need SQL Server (`.\COLL1` or `HT_IT_CONNECTION` and `HT_UI_CONNECTION`)
- [X] T125 Review the accessibility and wording of every page and of `SiteNav` (SC-007, HP-016):
  - a visible focus ring from tokens;
  - text contrast of at least 4.5:1, from the token pairs;
  - labelled icon-only buttons;
  - the narrow menu closes with Esc;
  - brand-book wording.

  Also search `src/HT.Ui` for hard-coded colours or sizes outside `wwwroot/css/` and remove them. T060 keeps `wwwroot/css/` equal to `design/css/` (constitution IV).
- [X] T126 Check for secrets (HP-017, HP-019): no password or connection secret in a tracked file, in `appsettings*.json` or in a log template. Only names in `README.md` and `quickstart.md`
- [X] T127 Run `dotnet build HabitTracker.slnx` and `dotnet test HabitTracker.slnx`. Every project must pass: unit, architecture, integration and UI. Record the counts for the report (constitution V)
- [X] T128 Walk through [quickstart.md](quickstart.md) section 3 with `dotnet run --project src/HT.Ui --launch-profile https`: check the startup log lines and each row of its browser table, including that registering (SC-001) and signing in (SC-002) are single-form steps; note anything left unverified

---

## Dependencies and execution order

### Phase dependencies

- **Setup (Phase 1)**: no dependencies.
- **Foundational (Phase 2)**: depends on Setup. It blocks every story. Within it:
  - the design tasks (T008–T016) come first;
  - then Domain (T017–T026), then the Application port (T027–T032), then Persistence (T033–T044);
  - then the host (T045–T053), then the shell (T054–T060).

  This is the plan's work order, steps 1–5.
- **Stories (Phases 3–9)**: all depend on Phase 2, and some also on another story (below). In priority
  order: US1, US2 and US3 (P1, the MVP), then US4 and US5 (P2), then US6 and US7 (P3).
- **Polish (Phase 10)**: after the stories that are to ship.

### Story dependencies

- **US1, US2, US3**: no dependency on another story. Each has its own result type and its own
  `AccountService` method, and its tests create accounts through the store or the fake store.
- **US4**: its code needs no other story, but its tests do: T085 and T086 sign in through US2's
  `SignInAsync` (T072) and register through US1's `RegisterAsync` (T064). Build it after US1 and US2.
- **US5**: none for its tests. In the running app, its Sign out form needs US2's endpoint, and its links
  need the US1 and US2 pages.
- **US6**: uses US3's `GetAccountDetailsAsync` and `ActingUserAccessor`. Its host tests sign in through
  US2's Sign in page.
- **US7**: after US4. T118 adds its step after T089's administrator step, T119 extends T090's rule, and
  T121 asserts the demo lines after the `Administrator creation:` line.
- **What a checkpoint can show in the browser**:
  - the signed-in Home of US1-AS10 and HP-002 needs T108 (US6);
  - a Sign out control needs T092 (US5).

  Until then, those scenarios are covered by host tests only.
- **Shared files** need sequential edits, so their tasks never run in parallel with each other:
  - `AccountService.cs`: T064, T072, T082, T105;
  - `ApplicationServiceCollectionExtensions.cs`: T088, T117;
  - `StartupTasks.cs`: T089, T118;
  - `Home.razor`: T095, T108;
  - `UiDependencyTests.cs`: T026, T053, T090, T119;
  - `Program.cs`: T046, T075, T083, T109.

### Within each story

- Result type, then use case, then form, then page. The tests marked [P] can be written first and must
  fail before the use case exists.
- The story's checkpoint runs the full suite before the next story.

### Parallel opportunities

- Phase 1: T002–T007.
- Design: T008–T014.
- Domain: T017–T020, then T022–T026.
- Application: T028, T031, T032.
- Persistence: T033, T034, T044.
- Host: T050, T053.
- Shell: T055, then T060 once T054 is done.
- In each story, every task marked [P].
- After Phase 2, US1, US2 and US3 can be built side by side, apart from the shared files listed
  above. US4 follows US1 and US2, and US7 follows US4.

---

## Parallel example: User Story 1

```text
Task: "T061 [US1] Register unit tests in tests/HT.UnitTests/Application/Accounts/RegisterTests.cs"
Task: "T062 [US1] Registration integration tests in tests/HT.IntegrationTests/Accounts/RegistrationTests.cs"

# after T063–T066:
Task: "T067 [US1] RegisterForm bUnit tests in tests/HT.UiTests/Components/Account/RegisterFormTests.cs"
Task: "T068 [US1] Register host tests in tests/HT.UiTests/Host/RegisterPageTests.cs"
```

## Parallel example: User Story 2

```text
Task: "T069 [US2] Sign-in unit tests in tests/HT.UnitTests/Application/Accounts/SignInTests.cs"
Task: "T070 [US2] Parallel block test in tests/HT.IntegrationTests/Accounts/SignInBlockTests.cs"
```

---

## Implementation strategy

### MVP first (the P1 stories)

1. Phase 1 and Phase 2 (design first, then the layers in order).
2. US1, then check registration on its own.
3. US2 and US3: the minimal account loop and the audience boundary (SPEC §3, ARCHITECTURE §4).
4. **Stop and validate**: the full suite passes, including the register, sign-in and sign-out host tests.
   In the running app, register and sign in. A manual sign-out and the signed-in Home wait for US5 and US6.

### Incremental delivery

1. US4 (an administrator exists) and US5 (the shell and the visitor Home) make the app presentable.
2. US6 (Profile and password change) and US7 (demo accounts) complete the slice.
3. Polish: CI, docs, the accessibility review, the full suite and the quickstart run.
4. The owner then runs `/speckit-ac-tests-write` and `/speckit-ac-tests-analyze` (plan, work order step 8).
   Expect the 63 scenarios of `tests/HT.AcceptanceTests/Features/F001/` to pass against `HT-AC`.

---

## Owner actions

- Create the repository secret `CI_SQL_SA_PASSWORD` (a throwaway SA password) before the PR's CI run.
  Without it, T122's job skips. The Sonar gate then stays unanswered (skipped, not green), and the PR cannot
  pass its gates (plan, Risks). Only the owner can create it; no task here sets or reads its value.
- Approve the dated `## Clarifications` line that T002 drafts (under `### Session 2026-11-11` in `spec.md`). It names the public-port decision and links
  ADR 0010 (T002), as the constitution's Decision Discipline asks. `spec.md` is under the `sources` lock, so
  after the edit, `ac_lock.py verify sources` reports spec drift (exit 3). The owner confirms it with
  `ac_lock.py confirm-spec sources`, as was done on 2026-11-11 for the email length. The line names a
  technical decision only; it changes no requirement and no acceptance criterion.

## Notes

- Never edit `.feature` files, `specs/*/ac/*.lock`, `specs/*/ac/runs/`, `artifacts/refs` or
  `.workflow/active-run.json`. Never read the AC project's step definitions or support code.
- Commit after each phase or logical group, in the format `<type>(<scope>): <emoji> <subject>`. The
  pre-commit hook runs the architecture tests, and `--no-verify` is never used.
- A failing test is fixed in the code, never by loosening its assertion.
