# Research: Accounts and access

**Feature**: specs/001-accounts-and-access · **Date**: 2026-11-11 · **Plan**: [plan.md](plan.md)

The spec's `## Clarifications` and ADRs 0004 to 0009 settle the product and architecture questions of this
slice. This file records the technical choices they leave to the plan, each as decision, rationale and
alternatives. Nothing here reopens a recorded decision. Revised after
[critic-plan-pass-1.md](critic-plan-pass-1.md) and [critic-plan-pass-2.md](critic-plan-pass-2.md).

## R-01 Packages and versions

- **Decision**:
  - `HT.Domain`: no package references.
  - `HT.Application`: `Microsoft.Extensions.DependencyInjection.Abstractions` (for its registration entry
    point, ADR 0005) only. `TimeProvider` is part of the base library.
  - `HT.Persistence`: `Microsoft.EntityFrameworkCore.SqlServer`,
    `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (Identity's EF Core stores; it brings
    `Microsoft.Extensions.Identity.Stores`, not the ASP.NET Core shared framework), and
    `Microsoft.EntityFrameworkCore.Design` with `PrivateAssets="all"` for migrations (R-15).
  - `HT.Ui`: the ASP.NET Core shared framework it already has (Identity's `SignInManager`, cookie
    authentication, the security-stamp validator and `RedirectHttpResult.IsLocalUrl` are part of it).
  - Tests: `Microsoft.Extensions.TimeProvider.Testing` (fake clock) in unit and integration tests; `bunit` and
    `Microsoft.AspNetCore.Mvc.Testing` in `HT.UiTests` (ADR 0006).
  - Tool: `dotnet-ef` added to `dotnet-tools.json`, pinned like `ilspycmd`.
  - Versions: the current 10.0.x patch of every Microsoft package at implementation time, the same patch for
    all EF Core and Identity packages; the current stable bUnit.
- **Rationale**: the fewest references that satisfy ADR 0004's layer rule (Domain and Application without
  Identity, EF Core or ASP.NET Core). DI abstractions are not a framework dependency in that sense.
- **Alternatives**: `Microsoft.AspNetCore.Identity` in Persistence (brings the ASP.NET Core framework
  reference into the layer the AC tests load; rejected). A logging package in Application (not needed: only
  the host logs, ADR 0007).

## R-02 Identity registration split between Persistence and the host

- **Decision**:
  - `HT.Persistence`'s composition entry point calls `AddIdentityCore<HtUser>()` with
    `AddEntityFrameworkStores<HtDbContext>()`, its options (R-03), the lookup normalizer (R-04) and the
    claims-principal factory (R-07). It adds no token providers, roles or sign-in manager.
  - The host builds its own `new IdentityBuilder(typeof(HtUser), services)` over the same collection and calls
    `AddSignInManager()`, then adds cookie authentication with `IdentityConstants.ApplicationScheme` and the
    security-stamp validator. This is host composition code, where ADR 0004 allows `HtUser`.
  - `HtDbContext` derives from `IdentityUserContext<HtUser, Guid>`, which has no role tables.
- **Rationale**: acceptance and integration tests build the application without `HT.Ui` (ADR 0003, ADR 0004),
  so everything the use cases need comes from the Persistence entry point; the cookie is the host's only job.
  `IdentityUserContext` is how "Identity's role tables are not used" (ADR 0004) is expressed.
- **Alternatives**: `AddIdentity<,>()` in the host (adds role services and tables; rejected). Returning
  `IdentityBuilder` from `AddHtPersistence` (puts an Identity type into the composition signature the AC
  tests see; rejected). `IdentityDbContext` (creates the role tables; rejected).

## R-03 Identity options that never refuse what the domain accepts

- **Decision**: in the Persistence entry point:
  - `Password`: `RequiredLength = 8`, `RequireDigit = true`, `RequireLowercase = false`,
    `RequireUppercase = false`, `RequireNonAlphanumeric = false`, `RequiredUniqueChars = 1`;
  - `User.AllowedUserNameCharacters = ""` (Identity checks characters only when the set is not empty);
    `User.RequireUniqueEmail = false` (uniqueness comes from the user name, ADR 0004);
  - `Lockout.AllowedForNewUsers = false`;
  - `SignIn.RequireConfirmedAccount = false`, `RequireConfirmedEmail = false` (ADR 0009).
- **Rationale**: ADR 0004 requires Identity's rules to be no stricter than FR-002 and FR-003. The use cases
  check the domain rules first; an Identity refusal after that is a fault.
- **Alternatives**: a custom `IPasswordValidator` that always passes (equivalent but less visible; rejected).

## R-04 Email normal form, uniqueness, comparison and length

- **Decision**:
  - The domain normal form (FR-003) is `Trim()` then `ToLowerInvariant()`. The rule is checked on the trimmed
    text: at most 254 characters, no whitespace character (`char.IsWhiteSpace`), exactly one `@`, at least
    one character before it, and a domain split on `.` into at least two parts, all non-empty.
    `ToLowerInvariant` maps UTF-16 code units one to one, so the normal form has the trimmed length.
  - `UserName` and `Email` hold the normal form. A Persistence `ILookupNormalizer` (`EmailLookupNormalizer`)
    returns the name and email unchanged, so `NormalizedUserName` and `NormalizedEmail` are the domain normal
    form too, not Identity's default form-C upper-case.
  - `NormalizedUserName` and `NormalizedEmail` use the binary collation `Latin1_General_100_BIN2`, so the
    database compares exactly what `Email` equality compares. Identity's unique index on `NormalizedUserName`
    therefore makes emails unique in the domain's sense (HP-020).
  - Column length: Identity's default 256, which fits the 254-character maximum (owner decision, 2026-11-11).
  - Unicode text is compared as given: a composed and a decomposed spelling of the same letter are different
    emails. FR-003 asks for trimming and case only.
- **Rationale**: Identity's default normalizer applies Unicode form C and upper-cases, and the server's
  default collation ignores width and similar differences. Either would make the database's idea of "the
  same email" differ from the domain's, and form C can make a 254-character email longer than the column.
  An identity normalizer plus a binary collation removes both gaps without restricting characters.
- **Alternatives**: keeping Identity's normalizer (two notions of equality; rejected). Restricting emails to
  ASCII (a rule FR-003 does not have; rejected). A separate unique index on `Email` (duplicate; rejected).

## R-05 Sign-in block: state, reads and concurrent attempts

- **Decision**:
  - The domain `SignInBlock` value holds `FailedCount` (0 to 5) and `BlockedSince` (an instant or none). For
    an attempt at time `t` ([data-model.md](data-model.md#signinblock)):
    1. a block whose end (`BlockedSince + 5 min`) is at or before `t` has ended: the state becomes (0, none)
       before the attempt is judged;
    2. a block that has not ended refuses the attempt without checking the password and without changing the
       state;
    3. otherwise a wrong password adds 1; the fifth failure sets `BlockedSince = t`; a correct password resets
       the state to (0, none).
  - **Fresh reads.** The store reads accounts (email, role, block state) with no-tracking queries, so every
    read sees the database's current state, also when several calls share one scope (integration and
    acceptance tests).
  - **Fresh user instances for Identity calls.** `CreateAsync`, `CheckPasswordAsync` and `ChangePasswordAsync`
    need an `HtUser` for the user manager. Each of them first clears the context's change tracker, then
    creates the user or loads it with `UserManager.FindByIdAsync`, and clears the tracker again when it is
    done, also after a failure. So no instance outlives one store call, and a failed save leaves nothing
    behind for the next call in the same scope.
  - **Compare-and-swap writes.** `IAccountStore.TrySaveSignInBlockAsync(id, expected, next)` runs one
    `ExecuteUpdateAsync` with `WHERE Id = @id AND FailedSignInCount = @expectedCount AND SignInBlockedSince`
    equal to the expected value (`IS NULL` for none), and returns whether one row changed.
  - **One attempt, in order.** `AccountService.SignInAsync` reads `t` from `TimeProvider` once per attempt,
    then:
    1. reads the account by email; none: `Failed`, nothing saved;
    2. settles the state at `t`; blocked: `Failed`, no password check, nothing saved (rule 2);
    3. checks the password once;
    4. computes the next state and tries to save it against the settled state;
    5. on a lost race, re-reads the state, settles it at the same `t` and goes back to step 4 with the same
       password verdict; if the re-read state is blocked, the result is `Failed` with nothing saved.
    After 10 lost races in a row the result is `Failed` with nothing saved (fails closed).
  - **Entity writes never touch the block.** The two block columns have `PropertySaveBehavior.Ignore` after
    insert, so `UserManager.UpdateAsync` (password change, security stamp) can never write a stale count back.
- **Rationale**: FR-008 holds for parallel attempts. A save succeeds only on the state it was computed from, so
  every failed attempt that saves is counted once, and the only attempts that end without saving are those
  that find the account blocked (the block exists already) and those that lose 10 races in a row (they fail
  closed). Neither path lets a sixth guess through. The rule itself stays in the domain (ADR 0004). The
  integration test runs 10 wrong attempts in parallel and then expects the correct password to fail; unit
  tests drive steps 1 to 5 with a fake store that can be told to lose a race.
- **Alternatives**: `UserManager.UpdateAsync` with Identity's concurrency stamp (a lost race surfaces as an
  Identity error and also renews the stamp; rejected). A serializable transaction around read, hash check
  and write (holds a lock during the slow hash; rejected). An atomic SQL increment (moves the block rule into
  SQL; rejected). Identity's lockout (reads the system clock and extends on attempts; ruled out by FR-008 and
  ADR 0004).

## R-06 Use-case result shape and service lifetimes

- **Decision**: one sealed result class per use case with an `Outcome` property of a use-case-specific enum,
  a `Message` string, and typed values for success (for example `AccountDetails`, with `Email` as `string` in
  normal form and `Role`). `Message` is the spec's literal text for every outcome the spec gives a text (every
  refusal and every operator result, including "administrator created", "created" and "already exists"), and
  `null` for the successes the spec gives no text: `Registered`, `SignedIn`, `Found`, `Changed` and `Loaded`. Texts are `public const string` fields of one static class per area in
  `HT.Application` (`AccountTexts`, `OperatorTexts`). Demo loading returns one entry per demo email, with the
  email as `string` in normal form. `AccountService`, `OperatorService` and `DemoDataService` are registered
  scoped (they use the scoped store); `TimeProvider` singleton. Shapes:
  [contracts/application-api.md](contracts/application-api.md).
- **Rationale**: ADR 0005 asks for named outcomes that carry the text, no exceptions for refusals, and texts
  defined once. Enums and constants survive in reference assemblies, so the isolated test writer sees every
  outcome name and text (ADR 0003).
- **Alternatives**: a generic `Result<T, TError>` (outcome names hidden behind type parameters in API
  listings; rejected). Exceptions for refusals (ruled out by ADR 0005).

## R-07 Acting user and the avatar letter in the host

- **Decision**: after a successful sign-in the cookie carries the account identifier (`NameIdentifier`), the
  account's normal-form email (`Name`, from `UserName`) and a role claim, added by a claims-principal factory
  for `HtUser` in Persistence. The host's `ActingUserAccessor` (in `HT.Ui/Account`, outside
  `HT.Ui.Components`) turns the principal into the domain `ActingUser`, and pages pass it to the use cases.
  The shell reads the `Name` claim for the avatar letter (first character, upper-cased invariantly, HP-014)
  from the cascading authentication state, in static and interactive rendering alike.
- **Rationale**: ADR 0004 and ADR 0005: the host keeps only the session and builds the acting user from it.
  Email and role never change in this slice (no email change, no role removal, FR-014), so the claims cannot
  go stale here; slice 11's deactivation uses the security-stamp check (ADR 0004). The claim is the user's own
  email in the user's own cookie, so no other account's data is exposed.
- **Alternatives**: calling the account-details use case on every page for the letter (a query per page; not
  needed while the email cannot change).

## R-08 Pages, render modes, forms and the navigation menus

- **Decision**:
  - Pages of this slice are static server-rendered Razor components. The only interactive component is
    `SiteNav` (`@rendermode InteractiveServer`), which renders TopNav and MobileNav and holds the state of both
    menus: the narrow-screen menu and the wide-screen avatar menu (Profile, Sign out). Each menu button has
    `aria-expanded` and `aria-controls`; a menu closes on Esc, on a click outside it and when an item is
    chosen, and focus returns to its button (US5-AS2 to US5-AS5, constitution IV). The markup comes from
    `design/components/TopNav` and `MobileNav` after the design step adds the avatar menu (spec, "Design
    changes before build").
  - Each of the three forms is a form component (`SignInForm`, `RegisterForm`, `ChangePasswordForm`): an
    `EditForm` with `method="post"`, a `FormName` and `[SupplyParameterFromForm]`, handled in the same
    request. The form component calls the Application use case, renders every refusal next to its field with
    the entered email kept (HP-001, HP-003, HP-014), and on success raises an `OnSucceeded` callback with the
    result. The page (`SignIn`, `Register`, `Profile`) hosts the form and, in that callback, calls
    `AccountSession` (in `HT.Ui/Account`), which wraps `SignInManager<HtUser>` (members in
    [contracts/host.md](contracts/host.md)), then redirects. So the form components need no session and can be
    tested in bUnit; only the pages touch the session.
  - Sign out is a form in both menus (with `<AntiforgeryToken />`) posting to the minimal-API endpoint
    `POST /account/sign-out`, which calls `AccountSession.SignOutAsync` and redirects to `/`.
- **Rationale**: a cookie can only be written in an HTTP response (ADR 0004). Static SSR forms post to the host
  and keep errors on the page without putting an email into a URL. Components use `AccountSession`, never
  `HtUser` or `SignInManager<HtUser>` (ADR 0004; architecture rule). One interactive nav component keeps the
  two menus consistent and is the only circuit a page opens.
- **Alternatives**: the Identity template's account pages (built on `SignInManager<TUser>` inside components;
  ruled out by the spec). Posting to endpoints that redirect back with the error in the query string (puts the
  email into a URL; rejected). A `<details>` element for the avatar menu (no Esc or outside-click close
  without script; rejected).

## R-09 Cookie, "Keep me signed in" and session revalidation

- **Decision**:
  - Application cookie: `ExpireTimeSpan = 14 days`, `SlidingExpiration = true`, `LoginPath = "/sign-in"`,
    `ReturnUrlParameter = "returnUrl"`, `HttpOnly`, `SecurePolicy = Always`, `SameSite = Lax`. Without "Keep
    me signed in" the cookie is a session cookie.
  - `SecurityStampValidatorOptions.ValidationInterval = TimeSpan.Zero` (checked on every request, ADR 0004).
    Every successful validation sets `ShouldRenew`, so the cookie is re-issued with a full 14-day window on
    every authenticated request: a kept session ends 14 days after the last visit, inside HP-004's range of 7
    to 14 days.
  - **Cost of that renewal.** Authentication runs for every request of a signed-in user, static assets
    (`MapStaticAssets`) included. Each such response gets one security-stamp lookup, a new cookie and the
    cookie handler's `Cache-Control: no-cache,no-store`, `Pragma: no-cache` and an expired `Expires`, so a
    signed-in browser does not cache the CSS and scripts either. Accepted at demo scale (a few small assets
    per page); slice 3 can revisit it for hosting.
  - **No-store for signed-in pages.** Those same headers are what keeps Back after sign-out from showing
    Profile or the signed-in Home from the browser cache (HP-008, HP-009), so no extra middleware is added. A
    host test asserts `no-store` on `/profile` and on `/` for a signed-in user, so a change to the stamp
    interval cannot silently remove it.
  - A `RevalidatingServerAuthenticationStateProvider` for the interactive circuit with
    `RevalidationInterval = 30 minutes`, checking the security stamp (HP-007).
  - The app is run and tested over HTTPS: the `https` launch profile locally, `https://localhost` as the base
    address of host-test clients, because the cookie is `Secure`.
- **Rationale**: framework session handling only (spec Clarifications, CLAUDE.md). The interval values come
  from the spec; the renewal behaviour was checked in `SecurityStampValidator<TUser>` (ASP.NET Core 10.0.14).
- **Alternatives**: a custom session store (custom session handling; ruled out). `SecurePolicy =
  SameAsRequest` (would send the session cookie over plain HTTP; rejected).

## R-10 Return URL after sign-in

- **Decision**: the Sign in page accepts `returnUrl` and redirects there after success only when
  `RedirectHttpResult.IsLocalUrl(returnUrl)` is true; otherwise it goes to `/`. Host tests cover `/profile`,
  `//evil.example`, `/\evil.example`, `https://evil.example` and an empty value.
- **Rationale**: HP-005; the framework's own local-URL check, already in the shared framework `HT.Ui` uses.
- **Alternatives**: a hand-written check (security-relevant code that the framework already has; rejected).
  `Url.IsLocalUrl` from MVC (needs MVC services; rejected).

## R-11 Startup tasks and configuration keys

- **Decision**:
  - Configuration keys (outside source control, HP-019): `Operator:AdministratorEmail`,
    `Operator:AdministratorPassword`, `Operator:DemoPassword`. Locally they live in the User Secrets store of
    `HT.Ui` (a `UserSecretsId` is added to `HT.Ui.csproj`); User Secrets load only in the Development
    environment.
  - The host reads `ConnectionStrings:HabitTracker` from configuration before building the services and
    passes it to `AddHtPersistence`. The Development value (`.\COLL1`, database `HT`, integrated security, no
    secret) goes in `appsettings.Development.json`; other environments are slice 3's decision.
  - `StartupTasks.RunAsync(IServiceProvider, IHostEnvironment, ILogger)` in `HT.Ui/Operator` runs after
    `Build()` and before `Run()`: Development-only migrations through the Persistence entry point, then
    administrator creation, then demo loading. Each step runs in its own service scope and its own
    `try/catch`, so a fault in one step leaves nothing in a context the next step uses. Log lines:
    [contracts/host.md](contracts/host.md). The start continues whatever the result (HP-018, HP-021, HP-022).
- **Rationale**: ADR 0007 leaves the key names to the plan; the `Operator` section matches the glossary term.
- **Alternatives**: an `IHostedService` (runs concurrently with the first requests unless awaited specially;
  rejected for ordering clarity).

## R-12 Persistence entry points

- **Decision**: three public static methods in `HT.Persistence`:
  - `AddHtPersistence(this IServiceCollection, string connectionString)`: the composition entry point
    (ADR 0003, ADR 0004);
  - `HtDatabase.ResetAsync(string connectionString, CancellationToken)`: drops the database and applies all
    migrations (ADR 0003, ADR 0006); used only by tests;
  - `HtDatabase.MigrateAsync(IServiceProvider, CancellationToken)`: applies pending migrations and returns
    how many it applied; used only by the host in Development (HP-022).

  The first migration is `InitialAccounts`.
- **Rationale**: ADR 0003 names the first two as the only Persistence members the AC tests use; the third
  is public because the host calls it, and it is not part of the AC surface.
- **Alternatives**: migrating inside `AddHtPersistence` (a registration method that touches the database;
  rejected).

## R-13 Architecture rules and what the UI may reach

- **Decision**:
  - `IAccountStore` is `internal` to `HT.Application`, visible through `InternalsVisibleTo` to
    `HT.Persistence` (which implements it), `HT.UnitTests` (the fake), `HT.UiTests` (the same fake, linked)
    and `HT.IntegrationTests`. The services
    take it through internal constructors, wired by `AddHtApplication`. So no component can inject the port.
  - ArchUnitNET rules, one test class per layer (constitution V):
    - `HT.Domain` depends on no other `HT.*` assembly and on no `Microsoft.*` type;
    - `HT.Application` depends only on `HT.Domain` among `HT.*`, and on no type of
      `Microsoft.EntityFrameworkCore`, `Microsoft.AspNetCore` or `Microsoft.Extensions.Identity`;
    - `HT.Persistence` does not depend on `HT.Ui`;
    - `HT.Ui.Components` does not depend on `HT.Persistence` (existing), on `SignInManager<>`, or on
      `OperatorService` and `DemoDataService`; only `HT.Ui.Operator` depends on the operator services;
    - no type in `HT.Domain` or `HT.Application` calls `DateTime.Now`, `DateTime.UtcNow`, `DateTime.Today`,
      `DateTimeOffset.Now`, `DateTimeOffset.UtcNow`, `TimeZoneInfo.Local` or `CultureInfo.CurrentCulture`
      (constitution II).
- **Rationale**: constitution III ("Hiding a control or route in Blazor is never the protection"): with the
  port internal and the operator services fenced by a rule, the only Application members a component can call
  are `AccountService`'s, each scoped to the acting user. Constitution V requires the rules for each layer that
  gets its first production types; constitution II asks for an architecture test against clock reads.
- **Alternatives**: a public port with a rule only (the port's id-only lookups would still be reachable from
  non-component host code; rejected). A Roslyn analyzer (more setup; not needed yet).

## R-14 CI database for integration and host tests

- **Decision**: in `.github/workflows/sonar.yml`:
  - the `Check prerequisites` step also checks the repository secret `CI_SQL_SA_PASSWORD` (a throwaway value);
    when it or `SONAR_TOKEN` is missing, the job skips cleanly with a notice, as it does today without
    `SONAR_TOKEN`;
  - SQL Server starts as a step after that check, under the same condition, not as a job-level service
    container (which would start before any step and fail without the secret): `docker run` of
    `mcr.microsoft.com/mssql/server:2022-latest` with `ACCEPT_EULA=Y` and the SA password from the secret, then
    a readiness loop;
  - the test step gets `HT_IT_CONNECTION` and `HT_UI_CONNECTION` built from that secret. The value is never
    echoed.
- **Rationale**: ADR 0006; keeps the job's documented skip behaviour (README "PR review flow": a run without
  the secrets is skipped, not green).
- **Alternatives**: a job-level `services:` container (fails before the skip check; rejected). Testcontainers
  (ruled out by ADR 0006).

## R-15 Creating the migration

- **Decision**: `HT.Persistence` has an `IDesignTimeDbContextFactory<HtDbContext>` that builds the context
  with the connection string from `HT_DESIGN_CONNECTION`, or the local `HT` default when it is not set
  (adding a migration does not connect). Command, with `HT.Persistence` as its own startup project:
  `dotnet ef migrations add InitialAccounts --project src/HT.Persistence --startup-project src/HT.Persistence
  --output-dir Migrations`.
- **Rationale**: keeps the Design package and the design-time path inside the layer that owns the schema;
  `HT.Ui` needs no EF tooling.
- **Alternatives**: `HT.Ui` as the startup project (needs the Design package in the host; rejected).

## R-16 Test hosts, databases and stand-ins

- **Decision**:
  - **Host tests** use one `WebApplicationFactory<Program>` per run, shared by every host test through one
    xUnit collection, so they run one at a time against one host. Its `ConfigureWebHost` sets the environment
    to `Testing` (no User Secrets, no start-up migrations) and passes every value with `builder.UseSetting`:
    `ConnectionStrings:HabitTracker` = `HT_UI_CONNECTION` or the local `HT-UI` default, and throwaway
    `Operator:*` values. `UseSetting` values are host settings, which the top-level code of `Program.cs`
    already sees when it reads `builder.Configuration` before building the services; values added through
    `ConfigureAppConfiguration` would arrive too late (research R-11). A guard test resolves `HtDbContext`
    from the factory and asserts that its database is `HT-UI`, so a missed override fails at once. The
    collection fixture calls `HtDatabase.ResetAsync` once before the host starts.
  - **HP-022** is tested without a host: a test calls `StartupTasks.RunAsync` with services built from
    `AddHtApplication().AddHtPersistence(...)` and a hand-written `IHostEnvironment` fake, against its own
    database `HT-UI-MIGRATE` on the same server, which it drops first. With `Development` it expects a count
    above 0, the schema in place and the operator log lines; with `Testing` it expects no migration. No
    developer secret or `HT` database is involved. (ADR 0006 names `HT-UI` for host tests; this one test needs
    an empty database at its start, which a shared `HT-UI` cannot be.)
  - **bUnit** (ADR 0006: "forms and their messages, the navigation variants, the narrow-screen menu") covers
    the three form components with the real Application services over the in-memory fake store and a fake
    clock: every refusal message next to its field, the kept email, "The passwords do not match.", "Your
    password is changed." and the emptied form, and that `OnSucceeded` fires. It also covers `SiteNav` (three
    audiences, narrow menu, avatar menu, Esc, outside click, focus return), `Footer`, the visitor and signed-in
    Home, and Privacy. The fake store is the one in `HT.UnitTests`, linked into `HT.UiTests` as a source file;
    `HT.UiTests` is added to `IAccountStore`'s `InternalsVisibleTo`.
  - **Host tests** cover what needs the running host: the pages' cookies and redirects after a successful
    form, `returnUrl`, Profile's redirect, signed-in redirects away from Sign in and Register, sign-out,
    `no-store`, the security-stamp check on the next request after a password change, the startup tasks'
    operator log, and the HP-020 race through two parallel registrations.
  - **HP-007, the open page.** A unit test in `HT.UiTests` drives the revalidating authentication-state
    provider: with an unchanged security stamp the state stays valid, with a changed one it does not; and its
    `RevalidationInterval` is 30 minutes.
  - **Integration tests** build services with `AddHtApplication().AddHtPersistence(HT_IT_CONNECTION or the
    HT-IT default)`, reset once per run, and run one at a time (ADR 0006).
- **Rationale**: ADR 0006 (own databases, never `HT`, bUnit for forms and messages, no mocking library, no
  interface only for tests). Writing a cookie needs a real HTTP response, so only that part of the pages is a
  host test.
- **Alternatives**: testing the form pages only as host tests (moves "forms and their messages" out of bUnit,
  against ADR 0006; rejected). An `IAccountSession` interface with a fake (an interface only for tests;
  rejected by ADR 0006). Starting a host in `Development` for HP-022 (loads the developer's User Secrets;
  rejected).
