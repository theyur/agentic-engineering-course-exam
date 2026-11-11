# Critic pass 1: plan for 001-accounts-and-access

**Verdict**: REVISE
**Date**: 2026-11-11
**Reviewed**: `plan.md`, `research.md`, `data-model.md`, `quickstart.md`, `contracts/application-api.md`,
`contracts/host.md` (all in `specs/001-accounts-and-access/`)

Checked against: `spec.md` (with Clarifications), `acceptance.md`, the locked `.feature` file, the constitution,
SPEC.md, ARCHITECTURE.md, CLAUDE.md, CONTEXT.md, README.md, `docs/roadmap.md`, ADRs 0001 to 0009, `design/`,
the existing code under `src/` and `tests/` (not the isolated AC folders), `.github/workflows/sonar.yml`,
`.githooks/pre-commit`, `.workflow/config.json`, and the installed framework assemblies
(`Microsoft.AspNetCore.App` 10.0.14, read with `ilspycmd`).

## Notes

### MAJOR-1: The sign-in block is knowingly not guaranteed under parallel wrong attempts, and the plan accepts this itself
- **Where**: `research.md` R-05 (decision, second bullet: the two values are saved "with a targeted EF Core
  `ExecuteUpdateAsync`"; rationale: saving outside `UpdateAsync` "avoids Identity's concurrency stamp turning
  two near-simultaneous sign-ins into a fault"; alternatives: "Concurrent failed attempts may therefore lose an
  increment"); `data-model.md`, `IAccountStore.SaveSignInBlockAsync(AccountId id, SignInBlock block)`;
  `plan.md` "Risks", first bullet ("accepted for the demo"); `plan.md` "Constitution Check" ("pass, no
  exceptions").
- **Against**: spec FR-008 ("After 5 failed sign-ins in a row for one account, sign-in for that account MUST be
  blocked"), with no exception for concurrent attempts; spec "Edge Cases", which is where the owner's accepted
  risks of this slice are listed (demo emails, forgotten password) and which does not list this one;
  constitution, Governance ("A conflict ... MUST be raised with the project owner and resolved by amending the
  right document").
- **Reasoning**: Each attempt reads the count, checks the password (a slow hash), then writes the whole block
  state. Attempts that overlap all read the same count and all write the same count + 1, so any number of
  parallel wrong passwords costs one increment. Whoever guesses passwords controls the parallelism, so this is
  the normal attack path against the block, not a rare timing accident: FR-008's limit of 5 guesses is not
  enforced for a client that sends its guesses in parallel. R-05 chooses the save path specifically so that the
  one existing mechanism that would detect the lost update (the concurrency stamp) is bypassed. The plan states
  "The spec has no concurrency rule for sign-in", but FR-008 is an unconditional MUST, and the plan, not the
  owner, records the acceptance: "Spec discrepancies" item 1 shows an owner answer with a date, this risk does
  not. No planned test covers the case (HP-020 covers only concurrent registration). The Constitution Check
  therefore reports a pass while a known deviation from a functional requirement stands without an owner
  decision in the spec.

### MAJOR-2: Roadmap decision R8 is still open, and the roadmap makes it a gate for this slice's `/speckit-tasks`
- **Where**: `plan.md` "Work order" ("The order `/speckit-tasks` should follow"), `plan.md` "Constitution
  Check", Decision Discipline row ("met"); `docs/roadmap.md` as changed in this step (slice 1 status set to
  `planned`).
- **Against**: `docs/roadmap.md` "Decisions on this roadmap": "*Before* says which slice cannot start until the
  decision is made"; row R8 ("Delivery time against the exam deadline") has Before = "slice 1's
  `/speckit-tasks`" and an empty owner answer ("—"); row R5 is in turn due "after R8".
- **Reasoning**: The plan hands the work to `/speckit-tasks` and marks the slice `planned` without mentioning
  that the roadmap blocks that next step on an owner decision that has not been made. None of the six artifacts
  names R8. The Decision Discipline row lists only SPEC §11 and ARCHITECTURE §9 items and the email-length
  answer, so the open gate is not visible to whoever starts the next step.

### MINOR-1: Block state can be read stale when several sign-ins share one DbContext scope
- **Where**: `research.md` R-05 (state saved with `ExecuteUpdateAsync`, "not with `UserManager.UpdateAsync`");
  `data-model.md` port table (`FindByEmailAsync` returns the `Account` with its `SignInBlock`);
  `contracts/application-api.md` ("resolves the services from a scope").
- **Against**: FR-008 and the locked scenarios that make 5 or more attempts for one account in one scenario
  (AC-014 to AC-023); ADR 0004 (the store is implemented "with Identity's user manager").
- **Reasoning**: `ExecuteUpdateAsync` writes to the database without touching entities the context already
  tracks, and a tracking query for an entity that is already tracked returns the tracked instance with its old
  values. Identity's user manager returns tracked `HtUser` entities. If the store reads the count through the
  user manager and writes it with `ExecuteUpdateAsync`, a second attempt in the same scope reads the count from
  before the first write, so the count never passes 1. The host is not affected (one scope per request), but
  integration tests and the isolated AC step definitions may well keep one scope per test or scenario; the
  contract even describes a single scope. A later `UpdateAsync` in the same scope (password change) can also
  write the stale block columns back. The artifacts state neither how the block state is read nor which scope
  lifetime the store supports.

### MINOR-2: How the host under test gets `HT-UI` and its operator values is not specified; the Development defaults point at `HT` and User Secrets
- **Where**: `research.md` R-11 (connection string default for Development in `appsettings.Development.json`,
  database `HT`; operator values in the User Secrets of `HT.Ui`; migrations, administrator creation and demo
  loading run at start); `contracts/application-api.md` (`AddHtPersistence(..., string connectionString)`, a
  value fixed at registration time); `contracts/host.md` "Configuration"; `plan.md` "Storage" and "Risks",
  last bullet.
- **Against**: ADR 0006 ("Integration tests never use `HT`"; host tests "use database `HT-UI`, with the
  connection string from `HT_UI_CONNECTION`").
- **Reasoning**: `WebApplicationFactory` starts the host in the Development environment by default, which is
  exactly the environment in which the host loads `appsettings.Development.json` and User Secrets and applies
  migrations. Unless the tests replace these values, the host tests migrate the developer's `HT` database, run
  administrator creation and demo loading against it with the developer's real secrets, and register their
  test accounts there. `HT_UI_CONNECTION` appears in the artifacts only for CI (R-14) and as a prerequisite
  (`quickstart.md`); no artifact says how it reaches the host, and the entry point takes the connection string
  as a plain value that the host must read before the application is built.

### MINOR-3: No stated way for bUnit page tests to stand in for `AccountSession`
- **Where**: `research.md` R-08 (Sign in, Register and Profile "call `AccountSession` (in `HT.Ui/Account`),
  which wraps `SignInManager<HtUser>`"); `plan.md` source tree (`HT.UiTests`: "bUnit components").
- **Against**: ADR 0006 ("bUnit for components: forms and their messages"; "Pages in bUnit tests get the real
  Application services with those fakes, so no interface exists only for tests"; "No mocking library").
- **Reasoning**: ADR 0006's fakes are for the inward-defined ports. `AccountSession` is neither a port nor an
  Application service; it is a concrete host class over `SignInManager<HtUser>`, whose constructor needs a
  user manager, an HTTP context accessor and a claims factory. The three form pages inject it, so a bUnit test
  of any of them needs an instance, also for refusal paths that never call it. The artifacts do not say whether
  these pages are tested in bUnit at all, with what `AccountSession`, or only as host tests, so the split of
  HP-001, HP-003 and HP-014 between bUnit and host tests cannot be derived for `/speckit-tasks`.

### MINOR-4: The design-time path for creating `InitialAccounts` is missing
- **Where**: `research.md` R-01 (`Microsoft.EntityFrameworkCore.Design` "as a private build-time asset" in
  `HT.Persistence`; `HT.Ui`: "the ASP.NET Core shared framework it already has"; tool `dotnet-ef`), R-12
  (registration only through `AddHtPersistence(connectionString)`); `plan.md` source tree (`HtDbContext.cs`,
  `HtDatabase.cs`, `Migrations/`).
- **Against**: CLAUDE.md and ARCHITECTURE §5 ("Schema changes only via EF Core migrations"); feasibility of
  work-order step 4.
- **Reasoning**: `dotnet ef migrations add` needs the Design package in the startup project and a way to build
  an `HtDbContext` at design time. With `HT.Ui` as the startup project the package is absent (private asset in
  `HT.Persistence`, no new package in `HT.Ui`); with `HT.Persistence` as its own startup project nothing in
  the listed files lets the tool construct the context, which is registered only through an entry point that
  needs a connection string. No artifact names the command, the startup project or a design-time connection.

### MINOR-5: R-09's description of the sliding renewal does not match the configuration it chooses
- **Where**: `research.md` R-09 ("the framework renews it once more than half of the window has passed, so a
  kept session ends 7 to 14 days after the last visit"; `ValidationInterval = TimeSpan.Zero`);
  `contracts/host.md` "Session".
- **Against**: framework behaviour in `Microsoft.AspNetCore.Identity.dll` and
  `Microsoft.AspNetCore.Authentication.Cookies.dll` 10.0.14: `SecurityStampValidator<TUser>.ValidateAsync`
  validates whenever the elapsed time exceeds `ValidationInterval`, `SecurityStampVerified` then sets
  `context.ShouldRenew = true`, and `CookieAuthenticationHandler.RequestRefresh` re-issues the ticket with a
  full window from now. HP-004.
- **Reasoning**: With a zero interval every authenticated request re-issues the cookie with a new 14-day
  window, so the half-window rule never comes into play and a kept session ends 14 days after the last visit.
  That is inside HP-004's range, so the requirement holds, but the stated mechanism is wrong, a host test
  written from R-09's wording ("not renewed in the first half") would fail, and the cost named in ADR 0004
  (one lookup per request) grows by a `Set-Cookie` on every response, which the plan does not mention.

### MINOR-6: `SecurePolicy = Always` against the HTTP launch profile and the default test client
- **Where**: `research.md` R-09 (`SecurePolicy = Always`); `quickstart.md` section 3
  (`dotnet run --project src/HT.Ui`, then sign-in steps in a browser).
- **Against**: `src/HT.Ui/Properties/launchSettings.json` (the first profile, which `dotnet run` uses, is
  `http` with `http://localhost:5277` only); ADR 0006 (host tests check "redirects, cookies and session
  revalidation").
- **Reasoning**: Under the default profile the app is served over plain HTTP and the session cookie carries the
  Secure attribute. Whether a browser keeps such a cookie from `http://localhost` depends on the browser, so
  the quickstart's sign-in steps are not reliable as written, and neither the quickstart nor R-11 names the
  `https` profile. The same setting means a test client with an `http` base address does not send the cookie
  back, which the host-test plan does not note.

### MINOR-7: The domain's email identity is not the key Identity and SQL Server compare
- **Where**: `research.md` R-04 ("`NormalizedUserName` ... carries Identity's unique index, which makes emails
  unique"; "Identity's default 256, which fits FR-003's maximum of 254 characters"), R-03 title ("Identity
  options that never refuse what the domain accepts"); `data-model.md` `Email` and the validation table.
- **Against**: FR-003 and `data-model.md` (`Email` value = trimmed text in `ToLowerInvariant()` form, equality
  by value); `UpperInvariantLookupNormalizer.NormalizeName` in `Microsoft.Extensions.Identity.Core.dll`
  10.0.14, which is `name?.Normalize().ToUpperInvariant()`; ADR 0004 ("an Identity refusal never reaches a
  user").
- **Reasoning**: The lookup key is the Unicode-normalized (form C), upper-cased user name, compared by the
  database's collation, in a 256-character column. For ASCII emails this equals the domain form. For other
  input it does not: form C can lengthen a string, so an email of 254 characters that the domain accepts can
  exceed the column and fail as a fault; and two different domain emails can compare equal in the database
  (normalization, and collation rules such as width), so "email taken" and sign-in lookups follow the server
  collation, not `Email` equality. The artifacts name no collation, restrict no characters, and plan a test
  only for an apostrophe. The two claims quoted above are therefore not established for non-ASCII emails.

### MINOR-8: The public store port and the operator services are reachable from components, and no planned rule prevents it
- **Where**: `data-model.md` "Application port" (`IAccountStore.FindByEmailAsync`, `FindByIdAsync`,
  `CheckPasswordAsync` take an email or an account identifier alone); `contracts/application-api.md`
  (`AddHtApplication` registers `OperatorService` and `DemoDataService`); `research.md` R-13 (the list of
  rules); `plan.md` Constitution Check, row III ("no method takes another account's identifier").
- **Against**: constitution III ("lookups of private data by identifier alone MUST NOT exist"; "Hiding a
  control or route in Blazor is never the protection"); FR-004; FR-012; ADR 0005 (operator capabilities "are
  called only by the host and by tests"); spec Assumptions ("in this slice nobody sees another account's
  email").
- **Reasoning**: Row III is true of `AccountService` only. The port must be public for `HT.Persistence` to
  implement it, so it and both operator services can be injected into any Blazor component: a component could
  read any account's email and role, test a password for any account, or create an administrator. R-13 keeps
  components away from `HT.Persistence` and `SignInManager<>` only. ADR 0005 leaves the parameter question to
  negative tests and review, but these members are not use cases and have no negative tests planned, and the
  plan's compliance claim does not mention them.

### MINOR-9: The contracts contradict each other or leave open points the next step needs
- **Where**: `contracts/application-api.md` ("Composition", "`DemoDataService`", "Result shape");
  `contracts/host.md` "Operator log"; `research.md` R-02, R-06, R-11.
- **Against**: internal consistency; HP-018, HP-021, HP-022; ADR 0005 (results carry the spec's text).
- **Reasoning**:
  - `HtDatabase.MigrateAsync` returns `Task`, but the operator log line is `Schema migrations applied:
    {Count}`. The host has no count to log through the entry point as specified.
  - `Demo loading: {Result}` is undefined for the `Loaded` outcome: R-06 allows a null `Message` "for a
    success that has no text", the spec gives no overall text, and `quickstart.md` shows only `Demo loading: …`.
  - `DemoAccountResult.Email` has no stated type (`string` or the domain `Email`), unlike every other member.
  - "Registers AccountService, OperatorService, DemoDataService, and TimeProvider.System with TryAddSingleton"
    can be read as registering all four as singletons. The services depend on a store built on a scoped
    `DbContext`; no lifetime is stated anywhere.
  - R-02 says the host calls `AddSignInManager()` "on the same identity builder", but `AddHtPersistence`
    returns `IServiceCollection`, so no builder reaches the host.

### MINOR-10: The wide-screen avatar menu and the source of the avatar letter are not planned
- **Where**: `research.md` R-08 ("the only interactive piece is `MobileNav`"), R-07 (the cookie carries the
  account identifier and a role claim; `ActingUser` is identifier and role); `plan.md` source tree
  (`Layout/`: `TopNav`, `MobileNav (interactive)`).
- **Against**: US5-AS2 and US5-AS3 (the avatar "opens a menu with 'Profile' and 'Sign out'" on the wide
  navigation); HP-010; HP-014 (the avatar shows the first letter of the email in the navigation, the menu and
  the Profile header); `design/components/TopNav.html`, where the avatar is a `span` with no menu markup, and
  `design/components/MobileNav.md` ("the only piece of TopNav that needs state").
- **Reasoning**: The wide navigation needs an open and closed state for the avatar menu too, yet `TopNav` is
  static and the design has no markup for it yet. The plan defers to the design step but fixes "only MobileNav
  is interactive" beforehand, so how the menu opens, closes and is operated by keyboard is undecided at plan
  level. Separately, nothing the shell receives contains the email: the acting user carries only identifier and
  role, and no artifact says whether the shell calls the account-details use case on every page or reads a
  claim, for the static navigation and for the interactive `MobileNav`.

### MINOR-11: Template UI with hard-coded colours, and the Error page, are outside the plan and outside `design/`
- **Where**: `plan.md` Constitution Check, row IV ("Bootstrap leftovers removed"); `plan.md` source tree
  (`Layout/` lists no `ReconnectModal`); `contracts/host.md` "Routes" ("`/not-found`, `/Error` | existing
  pages, restyled with the shell").
- **Against**: constitution IV ("no hard-coded colors, sizes, or shadows"; a needed visual change is made in
  `design/` first; pages follow their previews); HP-016; `design/README.md` "Not designed yet" (only "Not
  found" has guidance: EmptyState in the normal layout; nothing for the Error page); existing files
  `src/HT.Ui/Components/Layout/ReconnectModal.razor.css` (`#3b6ea2`, `#6b9ed2`, `#0087ff`, `rgba(...)`),
  `MainLayout.razor.css` (`lightyellow`), `wwwroot/app.css` (`#26b050`, `#e50000`, `#b32121`, `#929292`),
  `Components/Pages/Error.razor` (template wording about the Development environment).
- **Reasoning**: Making `MobileNav` interactive on every page opens a circuit on every page, so the reconnect
  modal and the `#blazor-error-ui` bar become UI a reviewer can see. They and the validation colours in
  `app.css` are template styling that is neither Bootstrap nor design tokens, and the plan addresses only
  Bootstrap. The Error page is to be "restyled" although no preview or guidance exists for it, and its
  template text does not follow the brand book. The spec's "Design changes before build" list does not cover
  any of this, and the plan's row IV claims the principle as met.

### MINOR-12: "Spec discrepancies" item 1 is stale and contradicts itself, and the new FR-003 clause has no acceptance criterion
- **Where**: `plan.md` "Spec discrepancies", intro and item 1.
- **Against**: the working tree (`spec.md` is modified: the Clarifications line and FR-003 now carry the
  254-character limit; `ac/sources.lock` already records the hash of that `spec.md`,
  `55df18a4…83aeb7`, which I recomputed); `research.md` R-04 and `data-model.md` (the unique index is on
  `NormalizedUserName`); `acceptance.md` and the `.feature` file (no item mentions 254 or 255); spec "What
  acceptance tests can reach" (every FR "is reachable through an Application use case, so acceptance tests can
  verify it").
- **Reasoning**: The section says "`spec.md` is not edited here" and, three lines later, "It is now in the
  spec's `## Clarifications` and FR-003". It says the change "needs `ac_lock.py confirm-spec sources`", but the
  lock already carries the new hash, so the reader cannot tell whether the owner's confirmation is pending or
  done. It says the `UserName` column "carries the unique index", which the other two artifacts place on
  `NormalizedUserName`. Finally, the length limit is application behaviour inside FR-003, yet the locked
  criteria have no item for either side of the boundary; the plan covers it by unit tests only and does not say
  that this leaves an FR clause outside the acceptance tests.

### MINOR-13: R-10 says no framework check for local URLs is available, but one is
- **Where**: `research.md` R-10 (alternatives: "`Url.IsLocalUrl` from MVC (not available to Razor components
  without MVC services)"; decision: "starts with a single `/`, not `//` or `/\`, and contains no scheme").
- **Against**: `Microsoft.AspNetCore.Http.Results.dll` 10.0.14, which has the public static
  `RedirectHttpResult.IsLocalUrl(string? url)` in the shared framework `HT.Ui` already references; HP-005.
- **Reasoning**: The only alternative weighed is the MVC helper, so the decision to hand-write a
  security-relevant check rests on an incomplete survey. The hand-written rule is also not precise enough to
  test from: "contains no scheme" has no definition (for example for a colon inside the query string).

### MINOR-14: README is not in the documentation step although this slice makes several of its statements untrue
- **Where**: `plan.md` "Work order", step 7 ("`CONTEXT.md` ('Demo account'); roadmap status").
- **Against**: `README.md` line 59 ("SQL Server will be required once persistence is implemented"), line 44
  ("Layer rules will be enforced by architecture tests added with future feature tasks"), line 100
  ("Foundation stage: ... an empty compiling solution"), and "Getting started", which lists
  `dotnet test HabitTracker.slnx` with only the .NET SDK as a prerequisite; ADR 0006, Consequences ("Running
  the whole solution locally needs `.\COLL1`, or `HT_IT_CONNECTION`").
- **Reasoning**: After this slice the documented getting-started commands fail without SQL Server and the
  operator secrets, and the status and layer-rule sentences are out of date. The setup lives only in
  `quickstart.md` inside the feature folder, and the plan's documentation step does not list README.

## Summary

2 MAJOR, 14 MINOR.

Outside the notes above, the result texts, check orders, configuration keys and routes agree across the six
artifacts and with the spec and the locked scenarios, and every FR, HP and success criterion is assigned to a
use case, a host behaviour or a test project. The plan is not ready for `/speckit-tasks` for two reasons: it accepts on its own authority that FR-008's block
can be outrun by parallel wrong attempts, with a save path chosen so that the lost update goes undetected, and
it hands over to `/speckit-tasks` while the roadmap's R8 gate for exactly that step is unanswered. The MINOR
notes are mostly feasibility gaps that will surface in the first days of implementation (stale block state in
a shared scope, the host tests' database and secrets, the stand-in for `AccountSession`, the EF design-time
path) plus a few factual and internal inconsistencies in `research.md`, the contracts and "Spec discrepancies".
