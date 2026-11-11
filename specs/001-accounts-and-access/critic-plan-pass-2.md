# Critic pass 2: plan for 001-accounts-and-access

**Verdict**: APPROVE
**Date**: 2026-11-11
**Reviewed**: `plan.md`, `research.md`, `data-model.md`, `quickstart.md`, `contracts/application-api.md`,
`contracts/host.md` (all in `specs/001-accounts-and-access/`)

Checked against: `spec.md` (with Clarifications), `acceptance.md`, the locked `.feature` file (63 scenarios),
the constitution, SPEC.md, ARCHITECTURE.md, CLAUDE.md, CONTEXT.md, README.md, `docs/roadmap.md`, ADRs 0003 to
0009, `design/README.md` and the navigation component guides, the existing code under `src/` and `tests/` (not
the isolated AC folders), `.github/workflows/sonar.yml`, `.githooks/pre-commit`, `.workflow/config.json`,
`ac/sources.lock`, and framework assemblies read with `ilspycmd`: `Microsoft.AspNetCore.App` 10.0.14
(`CookieAuthenticationHandler`, `SecurityStampValidator<TUser>`, `SignInManager<TUser>`, `RedirectHttpResult`,
`AuthenticationMiddleware`, `DefaultAntiforgeryStateProvider`), `Microsoft.EntityFrameworkCore` and
`.Relational` 10.0.12, `Microsoft.Extensions.Identity.Core` 10.0.12. Not available locally, so not decompiled:
`Microsoft.AspNetCore.Identity.EntityFrameworkCore` and `Microsoft.AspNetCore.Mvc.Testing`; statements about
those two rest on their published source and are marked as such. Nothing was built or run.

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 sign-in block under parallel attempts | Resolved | `research.md` R-05: compare-and-swap `TrySaveSignInBlockAsync(id, expected, next)` with retry; `data-model.md` port table; `plan.md` step 4 (10 parallel wrong attempts in `HT.IntegrationTests`); the "accepted for the demo" risk is gone from `plan.md` "Risks". A save succeeds only on the state it was computed from, so overlapping failures are each counted. The wording inside R-05 is not consistent with itself: see MINOR-3. |
| MAJOR-2 roadmap gate R8 | Resolved in the artifacts | `plan.md` "Gates before the next step" names R8, says it has no answer and that it blocks slice 1's `/speckit-tasks`, and names R5 as following from it; the Decision Discipline row repeats it. This matches `docs/roadmap.md` (R8: Before = "slice 1's `/speckit-tasks`", owner answer "—"; R5: "after R8"). The decision itself is still open, so the next step stays blocked until the owner answers. |
| MINOR-1 stale block state in a shared scope | Partly | Reads are no-tracking (R-05, `data-model.md` port intro) and the two block columns are ignored after insert. Verified in EF Core 10.0.12: `InternalEntryBase.SetEntityState` unflags every property whose after-save behaviour is not `Save` when an entity is marked Modified, and `ExecuteUpdate` translation does not consult save behaviour. The write path through the user manager in a shared scope is still not described: MINOR-2. |
| MINOR-2 host under test, `HT-UI` and operator values | Partly | R-16: environment `Testing`, `HT-UI`, throwaway `Operator:*` values. How the connection string reaches the host's early read, and the one host started in `Development`, remain open: MINOR-1. |
| MINOR-3 stand-in for `AccountSession` in bUnit | Resolved | R-16: the three form pages are host tests, bUnit covers components without a session; no stand-in is needed. The new split has its own issue: MINOR-4. |
| MINOR-4 design-time path for `InitialAccounts` | Resolved | R-15: `IDesignTimeDbContextFactory<HtDbContext>`, `HT_DESIGN_CONNECTION`, the command with `HT.Persistence` as startup project; `plan.md` source tree lists `HtDbContextFactory.cs`. |
| MINOR-5 sliding renewal description | Resolved | R-09 and `contracts/host.md` "Session" now say the cookie is re-issued with a full 14-day window on every authenticated request; `plan.md` "Risks" names the `Set-Cookie`. Confirmed again in `SecurityStampValidator<TUser>.SecurityStampVerified` and `CookieAuthenticationHandler.RequestRefresh` (10.0.14). A further effect of the same mechanism is not mentioned: MINOR-5. |
| MINOR-6 `SecurePolicy = Always` against the HTTP profile | Partly | `quickstart.md` section 3 uses `--launch-profile https` and `https://localhost:7194` (matches `launchSettings.json`); R-09 sets `https://localhost` as the base address of host-test clients. CLAUDE.md's run command still starts the `http` profile and is not in the documentation step: MINOR-8. |
| MINOR-7 email identity against Identity and SQL Server | Resolved | R-04: `EmailLookupNormalizer` returns the value unchanged, `NormalizedUserName` and `NormalizedEmail` use `Latin1_General_100_BIN2`, and "compared as given" is stated in R-04 and `data-model.md` (`Email`). The rationale for rejecting an ASCII restriction (a rule FR-003 does not have) is sound. |
| MINOR-8 public port and operator services reachable from components | Resolved | R-13: `IAccountStore` is internal with `InternalsVisibleTo`; a rule keeps `HT.Ui.Components` away from `OperatorService` and `DemoDataService`. `plan.md` row III now states both. The rule has no place in the work order: MINOR-9. |
| MINOR-9 contradictions in the contracts | Resolved (five points) | `MigrateAsync` returns `Task<int>` and the log line names it; `Demo loading: {Result}` is limited to the two refusals and `Loaded` logs per email; `DemoAccountResult.Email` is `string`; lifetimes are stated (scoped services, singleton `TimeProvider`); the host builds its own `IdentityBuilder` (R-02). A new gap of the same kind: MINOR-6. |
| MINOR-10 wide-screen avatar menu and avatar letter | Resolved | R-08: one interactive `SiteNav` holds both menus; R-07: the letter comes from the `Name` claim; `contracts/host.md` "Navigation". |
| MINOR-11 template UI and the Error page | Resolved | `plan.md` row IV and work-order step 1 (reconnect notice, unexpected-error bar, Error page designed first; `app.css`, `MainLayout.razor.css`, `ReconnectModal.razor.css` named); `contracts/host.md` "Routes". |
| MINOR-12 "Spec discrepancies" item 1 | Resolved | The item is consistent now. `ac/sources.lock` records `55df18a4…83aeb7`, which equals the SHA-256 of the current `spec.md` (recomputed). The item states that the 254-character clause has no acceptance criterion and is covered by unit tests only; with the owner's `confirm-spec` this follows the roadmap's rule for spec drift, so the rationale is sound. |
| MINOR-13 framework check for local URLs | Resolved | R-10 uses `RedirectHttpResult.IsLocalUrl` (public static in `Microsoft.AspNetCore.Http.Results.dll` 10.0.14). The cookie handler's own return-URL redirect on the login path uses the same check (`CookieAuthenticationHandler.IsHostRelative` calls `SharedUrlHelper.IsLocalUrl`), so the two do not disagree. |
| MINOR-14 README not in the documentation step | Resolved | `plan.md` step 7 lists README (prerequisites, getting started, layer-rules and status sentences). |

No other earlier pass exists for this stage, so nothing older can have regressed.

## Notes

### MINOR-1: How the test host gets its connection string is still not stated, and one host test runs in Development
- **Where**: `research.md` R-11 ("The host reads `ConnectionStrings:HabitTracker` from configuration before
  building the services"; the Development value is in `appsettings.Development.json`, "other environments are
  slice 3's decision"; "User Secrets load only in the Development environment"), R-16 ("sets
  `ConnectionStrings:HabitTracker` to `HT_UI_CONNECTION` or the local `HT-UI` default"; "The HP-022 test starts
  one host in `Development` with the same overrides, against `HT-UI`"; "The fixture calls
  `HtDatabase.ResetAsync` once per run"); `quickstart.md` section 1 ("environment `Testing`, so your User
  Secrets and the `HT` database are not touched"); `plan.md` "Risks", last bullet ("host tests run in the
  `Testing` environment against `HT-UI`").
- **Against**: ADR 0006 ("Integration tests never use `HT`"; host tests use `HT-UI`; "One test at a time" is
  stated for integration tests only); HP-022 ("so that a new local database works without a manual step");
  internal consistency.
- **Continues**: pass 1 MINOR-2.
- **Reasoning**:
  - R-16 says the factory "sets" the key but not through which mechanism, and R-11 fixes that the host reads
    the key before the services are built. With the minimal hosting model that `Program.cs` uses,
    configuration that a `WebApplicationFactory` adds through its application-configuration callback is applied
    when the host is built, after the top-level code has already read `builder.Configuration` (published
    behaviour of `Microsoft.AspNetCore.Mvc.Testing`; not decompiled here). Whether the override is visible to
    the early read therefore depends on a choice no artifact records.
  - In `Testing` no source for the key is defined at all (R-11 defines only Development), so a missed override
    fails visibly. In the Development host of the HP-022 test the fallback is `appsettings.Development.json`,
    which is `HT`: a missed override there migrates `HT` and runs administrator creation and demo loading
    against it, which is what ADR 0006 rules out and what pass 1 MINOR-2 described.
  - `quickstart.md` and `plan.md` "Risks" say all host tests run in `Testing` and User Secrets are not touched.
    R-16 itself has one host in Development, where User Secrets are loaded (R-11). The three statements do not
    agree.
  - The fixture has already applied all migrations to `HT-UI` before any host starts, so the Development host
    has nothing pending and can only log a count of 0. How HP-022's positive case (a database without the
    schema gets it at start) is shown is not stated; emptying `HT-UI` inside the run would affect the other
    host tests that share it.
  - Every host start runs administrator creation and demo loading against the shared `HT-UI` with the fixed
    demo emails. Whether the host tests in `HT.UiTests` run one at a time is not stated in any artifact.

### MINOR-2: The store's write path through the user manager is not described for a scope that already tracks the user
- **Where**: `data-model.md` "Application port" ("Every read is a no-tracking query"; `CheckPasswordAsync` and
  `ChangePasswordAsync` take an `AccountId`); `research.md` R-05 ("Fresh reads ... also when several sign-ins
  share one scope (integration and acceptance tests)"; "`UserManager.UpdateAsync` (password change, security
  stamp)"), R-11 (`StartupTasks.RunAsync` runs its three steps "in one scope", "Each step is wrapped in its own
  `try/catch`"); `contracts/application-api.md` ("resolves the services from a scope").
- **Against**: ADR 0004 (the store is implemented "with Identity's user manager"); the locked scenarios that
  create an account and change its password in one scenario (AC-031 to AC-035, AC-059); HP-018 and HP-021 (a
  fault in one startup task does not stop the others).
- **Continues**: pass 1 MINOR-1.
- **Reasoning**: R-05 claims support for one scope shared by many calls, but describes only the reads of the
  block state. Creating a user through the user manager leaves that `HtUser` tracked by the scope's context.
  A later password change needs an `HtUser` instance for the user manager: an instance from a no-tracking
  query cannot be attached while another instance with the same key is tracked (Identity's EF Core store
  attaches the instance it is given before it updates; from its published source, not decompiled here), and
  an instance from the user manager's own lookup is the tracked one, so "every read is a no-tracking query"
  does not hold for the password hash and the stamps. Neither artifact says which of the two the store does
  for `CheckPasswordAsync` and `ChangePasswordAsync`, so the shared-scope claim is established for sign-in
  only. The same shared context sits under the three startup tasks: an entity left in the Added state by a
  failed save in one task is still in the context when the next task saves, so the per-step `try/catch` does
  not by itself keep a fault in administrator creation out of demo loading.

### MINOR-3: R-05 contradicts itself on when the password is checked and on "counted exactly once"
- **Where**: `research.md` R-05, rule 2 ("refuses the attempt without checking the password"), "Retry"
  ("`AccountService.SignInAsync` checks the password once, then loops: read the state, settle it at `t`, stop
  with `Failed` if it is blocked"; "After 10 lost races in a row it gives `Failed` without saving"), rationale
  ("every failed attempt is counted exactly once"); `contracts/application-api.md` ("Parallel attempts for one
  account are each counted once"); `plan.md` step 3 (unit tests "including lost-race retries").
- **Against**: internal consistency; FR-008; constitution II (unit tests on both sides of each boundary are
  written from these statements).
- **Continues**: new (arises from the rewrite for pass 1 MAJOR-1).
- **Reasoning**: As written, the password is checked before the state is read, so it is also checked for a
  blocked account, which rule 2 excludes. The result is `Failed` either way, but a unit test with the fake
  store cannot be written from both statements, and whether a blocked attempt costs a password hash is left
  open. Separately, the decision contains a path in which an attempt ends as `Failed` without being counted
  (and in which a correct password fails on an account that is not blocked), while the rationale and the
  contract state without qualification that every attempt is counted once. I could not construct that path
  without several successful sign-ins for the same account racing the attempt (wrong attempts alone change the
  state at most about five times before the block stops further saves), so FR-008 is not at risk; the
  unconditional wording is what does not match the decision. R-05 also does not say whether `t` is read once
  per attempt or again in each loop round.

### MINOR-4: The bUnit / host-test split departs from ADR 0006 without saying so, and HP-007's open-page case has no test home
- **Where**: `research.md` R-16 ("The three form pages (Sign in, Register, Profile) are tested as host tests
  ... asserting the rendered messages, the kept email, cookies and redirects"; bUnit covers "components that
  need no session"), R-09 (`RevalidatingServerAuthenticationStateProvider`, 30 minutes); `plan.md` "Testing"
  ("split as research R-16 says").
- **Against**: ADR 0006 ("bUnit for components: forms and their messages, the navigation variants, the
  narrow-screen menu"; host tests for "the startup tasks and their operator log, redirects, cookies and
  session revalidation"; "Pages in bUnit tests get the real Application services with those fakes");
  constitution, Decision Discipline ("Later features MUST cite a recorded decision rather than decide it
  again"); HP-007 ("a page already open in another browser MUST lose access within 30 minutes").
- **Continues**: new (arises from the answer to pass 1 MINOR-3).
- **Reasoning**: R-16 cites ADR 0006 for "no interface only for tests", which is correct, but the result moves
  "forms and their messages" from bUnit to host tests and leaves no page in bUnit that receives Application
  services with the fake store. That is a different split from the one the ADR records, and no artifact notes
  the difference. A consequence is that every form-message test now needs SQL Server and `HT-UI`. Separately,
  R-16 describes host tests as real requests. The 30-minute revalidation of an open interactive circuit is not
  observable through requests, and it is not in the bUnit list either, so the second half of HP-007 is
  assigned to no test project.

### MINOR-5: The cost of renewing the cookie on every request is understated
- **Where**: `research.md` R-09 ("The cost is a `Set-Cookie` header on every authenticated response";
  "`Cache-Control: no-store` on every response to a signed-in user"); `plan.md` "Risks", third bullet;
  `contracts/host.md` "Session"; `plan.md` source tree ("no-store middleware").
- **Against**: `CookieAuthenticationHandler.FinishResponseAsync` and `ApplyHeaders` in
  `Microsoft.AspNetCore.Authentication.Cookies.dll` 10.0.14; `AuthenticationMiddleware` (authenticates the
  default scheme on every request); `src/HT.Ui/Program.cs` (`app.MapStaticAssets()`: static files are
  endpoints behind the same pipeline); ADR 0004 ("checked on every page load"; "one lookup per request").
- **Continues**: pass 1 MINOR-5 (same mechanism, an effect not covered then).
- **Reasoning**: Whenever the handler re-issues the cookie it also sets `Cache-Control: no-cache,no-store`,
  `Pragma: no-cache` and an expired `Expires` on that response. With a zero validation interval this happens on
  every authenticated request, and authentication runs for static-asset requests too. So for a signed-in user
  every stylesheet and script response carries a database lookup, a new cookie and no-store headers, and the
  browser cannot cache them. The plan's risk entry and ADR 0004 speak of page loads and one lookup per request
  and do not weigh this. The planned no-store middleware also duplicates what the chosen configuration already
  produces, and "every response to a signed-in user" does not say whether assets are meant.

### MINOR-6: The result contract leaves two outcomes without a `Message` rule, and two shapes are unstated
- **Where**: `research.md` R-06 ("`null` only for `Registered`, `Changed` and `Loaded`");
  `contracts/application-api.md` "Result shape", "Texts" (last paragraph), the `AccountService` table
  (`SignedIn`, `Found` with `AccountDetails`: `Email`, `Role`); `contracts/host.md` (no entry for
  `AccountSession`); `research.md` R-08 (pages call `AccountSession`).
- **Against**: internal consistency; ADR 0005 (success "carries its value"; refusals and operator results
  carry text).
- **Continues**: pass 1 MINOR-9.
- **Reasoning**: `SignedIn` and `Found` are successes with no text in the spec and no constant in the texts
  table, yet "null only for `Registered`, `Changed` and `Loaded`" excludes them, so their `Message` is defined
  by neither rule. `AccountDetails.Email` has no stated type, while `DemoAccountResult.Email` was fixed to
  `string` after pass 1. `AccountSession` is the one host type every form page calls, and no artifact gives
  its members or what identifies the account it signs in (an identifier, an `ActingUser`, a sign-in result).

### MINOR-7: The CI change does not address the job's existing skip path
- **Where**: `research.md` R-14 (a `services: mssql` container with the SA password from the secret
  `CI_SQL_SA_PASSWORD`, a health check); `plan.md` "Risks", first bullet ("Until it exists, the integration and
  host tests fail in CI").
- **Against**: `.github/workflows/sonar.yml` line 10 ("The job skips cleanly while SONAR_TOKEN is not set") and
  its `Check prerequisites` step, on which every later step is conditioned; README "PR review flow" ("a run
  without `SONAR_TOKEN` is *skipped*, not green").
- **Continues**: new.
- **Reasoning**: A job-level service container is started before the job's first step (GitHub Actions'
  documented order; not run here), so it does not take part in the step-level skip. Without the secret the
  container gets an empty SA password and cannot pass its health check, so the job fails before `Check
  prerequisites` runs, not only in the tests as the risk entry says. The same holds for any run that has no
  access to repository secrets. R-14 does not say how the documented skip behaviour and the container relate.

### MINOR-8: CLAUDE.md's run and test commands become stale and are not in the documentation step
- **Where**: `plan.md` "Work order", step 7 (CONTEXT.md, README.md, roadmap status); `research.md` R-09 ("The
  app is run and tested over HTTPS: the `https` launch profile locally").
- **Against**: CLAUDE.md "Build and test" (line 22: "Run UI: `dotnet run --project src/HT.Ui`"; line 21: "Test:
  `dotnet test HabitTracker.slnx`"); `src/HT.Ui/Properties/launchSettings.json` (the first profile, which that
  command starts, is `http` on `http://localhost:5277` only); ADR 0006, Consequences (running the whole
  solution needs SQL Server).
- **Continues**: pass 1 MINOR-6 and MINOR-14.
- **Reasoning**: The plan keeps the profile order and changes the command in `quickstart.md` and README
  instead. CLAUDE.md, which every agent session follows, keeps a run command that starts the profile under
  which R-09 itself says sign-in is not reliable, and a test command that from this slice on needs SQL Server,
  without either being mentioned. Step 7 does not list CLAUDE.md.

### MINOR-9: Two pieces of work have no step in the work order
- **Where**: `plan.md` "Work order", steps 2 to 6; `research.md` R-13 (fourth rule: `HT.Ui.Components` does not
  depend on `SignInManager<>`, `OperatorService` or `DemoDataService`; "only `HT.Ui.Operator` depends on the
  operator services").
- **Against**: `plan.md` Constitution Check, row III (relies on that rule); constitution V (rules for a layer
  come with its first production types); `tests/HT.ArchitectureTests/UiDependencyTests.cs` (loads only the
  Persistence and Ui assemblies today).
- **Continues**: new.
- **Reasoning**: Steps 2, 3 and 4 each name their layer's architecture rules; steps 5 and 6 name none, so the
  component rules that row III depends on are assigned nowhere. Step 5 (cookie, revalidation, sign-out,
  no-store, startup tasks) has all its tests in `HT.UiTests`, which is first created in step 6, so step 5
  cannot be verified when it is finished; the order does not say this.

## Summary

0 MAJOR, 9 MINOR.

Both MAJOR notes of pass 1 are resolved in the artifacts: the sign-in block is now saved by compare-and-swap
with retry and has a parallel integration test, and the open roadmap decision R8 is recorded as a gate before
`/speckit-tasks` exactly as the roadmap states it. That gate is still unanswered, so the next step waits for the
owner regardless of this verdict. Eleven of the fourteen MINOR notes are resolved; MINOR-1, MINOR-2 and MINOR-6
are partly resolved and continue as MINOR-2, MINOR-1 and MINOR-8 here. The framework claims in the rewritten
research (save behaviour of the block columns, renewal on every request, the local-URL check, the antiforgery
token inside an interactive component, Identity registration without the ASP.NET Core framework in Persistence)
held up against the assemblies. The Application surface, check orders and texts agree with the spec and with
all 63 locked scenarios. What remains are gaps that `/speckit-tasks` would otherwise inherit: how the test host
receives its connection string and the one Development host (MINOR-1), the store's write path in a shared scope
(MINOR-2), wording in R-05 that tests cannot be written from (MINOR-3), a test split that differs from ADR 0006
with one HP-007 case unassigned (MINOR-4), and smaller omissions in risks, contracts, CI, CLAUDE.md and the
work order (MINOR-5 to MINOR-9).
