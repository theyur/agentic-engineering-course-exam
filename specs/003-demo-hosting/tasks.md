---

description: "Task list for slice 3, demo hosting"
---

# Tasks: Demo hosting

**Input**: Design documents from `specs/003-demo-hosting/`: [plan.md](plan.md), [spec.md](spec.md),
[research.md](research.md), [data-model.md](data-model.md), [quickstart.md](quickstart.md), and the contracts
[deploy-workflow](contracts/deploy-workflow.md) (the only step numbering), [smoke-check](contracts/smoke-check.md)
and [hosted-configuration](contracts/hosted-configuration.md). Decisions: [ADR 0015](../../docs/adr/0015-demo-hosting.md).
Revised after [critic-tasks-pass-1.md](critic-tasks-pass-1.md) and, for its MINOR notes,
[critic-tasks-pass-2.md](critic-tasks-pass-2.md).

**Tests**: required. The constitution (V) makes tests part of done, and the plan names them (research R-11).
Test tasks come first in each phase and must fail before their implementation task, unless a task says it is a
guard that passes from the start. No acceptance tests: the slice is off the AC surface (spec "What acceptance
tests can reach", ADR 0003).

**Organization**: one phase per user story of the spec, in priority order. The stories share one workflow file,
one tool and one test project, so they are built in order (see Dependencies). Phase 8 is hosted verification on
Azure after PR 1 is merged; it carries story labels for traceability.

**Plan gate (demo data of shipped slices)**: checked on 2026-11-15. Shipped: slice 1 (`accounts-and-access`, demo
accounts, FR-016) and slice 2 (`habit-catalog`, demo custom habits FR-031, seed catalog FR-030). `origin/master`
has no other slice and no change to `IDemoDataStore`. The smoke check therefore covers exactly the three kinds in
the contract's coverage table. **If another slice ships before this slice's implementation starts**, run this
check again first: add a check and a `SmokeCheckCoverage` entry for each new `IDemoDataStore` method or
parameter, to T021, T022, T023 and T025.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: can run in parallel (different files, no dependency on an unfinished task)
- **[Story]**: the user story of spec.md (US1–US4)
- Paths are repository-relative. Workflow "step n" means the step of contracts/deploy-workflow.md.
- **(user)** marks a task the user performs because it needs a secret value or Azure access; the agent only
  prepares the commands and records the outcome the user reports.

## Rules for every task

- The agent never reads, prints or passes as an argument a secret value (CLAUDE.md). The operator values of local
  runs are User Secrets of `HT.Ui`; tests use throwaway values like `HostFixture`'s.
- Never point Roslynator or `jb` at `tests/HT.AcceptanceTests`; never touch the AC project.
- Report test runs as `PASS` with counts, or the failing test, assertion and `file:line` (CLAUDE.md).
- Commit only when the user asks; never `--no-verify`. The pre-commit hook runs `tests/HT.ArchitectureTests`, so
  that project must compile and pass at every commit.

## Names fixed by these tasks

The contract calls the checks class `SmokeCheck`. That name equals the last part of the tool's namespace
`HT.SmokeCheck`, which is a member of namespace `HT`, so from any `HT.*` namespace the simple name `SmokeCheck`
binds to the namespace, not the class. The class is therefore `SmokeCheckRunner`, and the new test folder and
namespace are `tests/HT.UiTests/SmokeChecks/` and `HT.UiTests.SmokeChecks`. The contract also names
`SmokeCheckCommand`, `PageReader` and `SmokeCheckCoverage`, which keep their names. `SmokeCheckOptions`,
`SmokeCheckResult` and `CoverageEntry`, and the optional fourth parameter `createHandler` of
`SmokeCheckCommand.RunAsync` (for tests without a network), are added by these tasks. T037 brings
contracts/smoke-check.md and plan.md's source tree in line with these names and with the output lines below.

## Output lines fixed by these tasks

The contract's "Output" section, made exact for the cases it leaves open:

- a check with a list: `PASS <name> (<found> of <expected>)` or `FAIL <name>: <reason>`;
- a check without a list (`home`, `https-redirect`, `administrator`): `PASS <name>` or `FAIL <name>: <reason>`;
- check 2 with `--skip-http-redirect`: `SKIP https-redirect`;
- timing: `TIMING first <ms> ms[ (retried)], later <ms> ms (<n> requests)`, or `later n/a` when no demo account
  signed in;
- last line `RESULT PASS` or `RESULT FAIL`. "Every check line" means the lines before `TIMING`.
- The visitor Home is recognised by its first `h1`, `Keep the habits you choose` (`src/HT.Ui/Components/Pages/Home.razor`).

---

## Phase 1: Setup (shared infrastructure)

**Purpose**: the roadmap shows the slice as being built; the new tool project exists, builds in the solution and
is referenced by the test projects.

- [X] T001 In `docs/roadmap.md`, set row 3's Status to `implementing` and its Horizon to `Now`, set `updated_at` to today, and keep the R2 text in slice note 3 (the start date stays the owner's). Roadmap rule: *Now* = the slice is being built.
- [X] T002 Create `tools/HT.SmokeCheck/HT.SmokeCheck.csproj`: SDK `Microsoft.NET.Sdk`, `<OutputType>Exe</OutputType>`, `<IsPackable>false</IsPackable>`, one `ProjectReference` to `../../src/HT.Application/HT.Application.csproj`, no package. Do not repeat `TargetFramework`, `Nullable` or `ImplicitUsings` (root `Directory.Build.props`). Add `tools/HT.SmokeCheck/SmokeCheckCommand.cs` (namespace `HT.SmokeCheck`) with a stub `public static class SmokeCheckCommand` whose `public static Task<int> RunAsync(string[] args, System.Collections.IDictionary environment, TextWriter output, Func<HttpMessageHandler>? createHandler = null)` returns `Task.FromResult(2)`, and `tools/HT.SmokeCheck/Program.cs` with only `return await HT.SmokeCheck.SmokeCheckCommand.RunAsync(args, Environment.GetEnvironmentVariables(), Console.Out);`. `Program` stays internal (top-level statements); no `InternalsVisibleTo`.
- [X] T003 Add the project to `HabitTracker.slnx` in a new `<Folder Name="/tools/">` after `/tests/`; run `dotnet build HabitTracker.slnx` (warnings are errors) and fix any warning at its cause.
- [X] T004 Add `<ProjectReference Include="..\..\tools\HT.SmokeCheck\HT.SmokeCheck.csproj" />` to `tests/HT.UiTests/HT.UiTests.csproj` and `<ProjectReference Include="../../tools/HT.SmokeCheck/HT.SmokeCheck.csproj" />` to `tests/HT.ArchitectureTests/HT.ArchitectureTests.csproj`; build the solution and confirm `HT.UiTests` still resolves `WebApplicationFactory<Program>` to `HT.Ui`'s `Program`.
- [X] T005 [P] In `.github/workflows/sonar.yml`, replace `sonar.coverage.exclusions="tools/**,.claude/hooks/**"` with `"tools/pr-gates/**,tools/guard-oracle/**,tools/hosting/**,.claude/hooks/**"` and update the comment above it: scripts stay excluded (ADR 0001), the .NET smoke check is measured (research R-11).

**Checkpoint**: `dotnet build HabitTracker.slnx` succeeds; the architecture suite passes (23 cases).

---

## Phase 2: Foundational (blocking prerequisites)

**Purpose**: the pieces every story needs: the smoke check's core (page reading, invocation, sign-in, checks 1
and 2, output, timings), the architecture rule for the tool, the shared test host on a derived database, and
the provisioning scripts that create the hosted demo.

**⚠️ CRITICAL**: no story phase starts before this phase is complete.

### Tests first

- [X] T006 [P] Write `tests/HT.UiTests/SmokeChecks/PageReaderTests.cs` (namespace `HT.UiTests.SmokeChecks`) against fixed HTML strings, for the `PageReader` API of T011: reads the `__RequestVerificationToken` value and the `_handler` value in both attribute orders (as `HostClient`'s patterns do); throws an error naming the path when no token is present; `ReadFirstHeading` returns the decoded text of the first `h1`; `ReadHabitNames` returns the names of habit cards after HTML decoding (`&amp;` → `&`, `&#x27;` → `'`); `ReadAssetUrls` returns the `href` of every `<link rel="stylesheet">` and the `src` of every `<script src>` in document order, ignoring inline scripts; `ContainsAll` is true when every expected name is present and extra names exist, false when one is missing. Fails to compile until T011.
- [X] T007 [P] Write `tests/HT.UiTests/SmokeChecks/InvocationTests.cs` for `SmokeCheckCommand.Parse` and `RunAsync` with a `Hashtable` environment and a `StringWriter` output: exit code 2 with a usage line when the base URL is missing, not absolute or not `https://`, and when each of `HT_SMOKE_DEMO_PASSWORD`, `HT_SMOKE_ADMIN_EMAIL`, `HT_SMOKE_ADMIN_PASSWORD` is missing or whitespace (one case each); an unknown option gives 2; `--skip-http-redirect` and `--warm-up-seconds 5` parse into `SmokeCheckOptions`; the output never contains the values given for the three variables (sentinels such as `sentinel-pw-1`). Fails until T012.
- [X] T008 [P] Write `tests/HT.UiTests/SmokeChecks/RunnerBehaviourTests.cs` with a hand-written fake `HttpMessageHandler` (scripted responses per method and path, no network), run through `SmokeCheckCommand.RunAsync(..., createHandler: () => fake)`: (a) check 1 gets 503, then the visitor Home: `PASS home` and `TIMING first … (retried)`; (b) check 1 never gets Home within `--warm-up-seconds 1`: `FAIL home`, exit 1; (c) check 2 passes for 301, 302, 307 and 308 to `https://<host>/` (theory), fails for 200 and for a `Location` on another host; (d) with `--skip-http-redirect` the line is `SKIP https-redirect`; (e) a scripted good site (Home and the redirect only, as long as checks 1 and 2 are the only ones) gives exit 0, the lines `PASS home`, `PASS https-redirect`, `TIMING first <ms> ms, later n/a`, `RESULT PASS`. T024 and T025 extend this scripted site and add the cases that need demo sign-ins. Fails until T013 and T014.
- [X] T009 [P] Write `tests/HT.ArchitectureTests/SmokeCheckDependencyTests.cs`, a guard that passes from the start and keeps passing: add `public static readonly System.Reflection.Assembly SmokeCheck = typeof(HT.SmokeCheck.SmokeCheckCommand).Assembly;` to `tests/HT.ArchitectureTests/ArchitectureModel.cs` and load it with the other four; an ArchUnitNET rule that types of that assembly depend on no type of `HT.Persistence` or `HT.Ui`; and a reflection check that its referenced assemblies among the solution's (`HT.*`) are a subset of `HT.Application` and `HT.Domain` (the compiler records a reference only when a type is used, so the set grows during Phase 4). Run the whole architecture suite: the existing 23 cases and the new ones pass.

### Shared test host

- [X] T010 Create `tests/HT.UiTests/Host/DerivedDatabaseHost.cs`: `public sealed class DerivedDatabaseHost : WebApplicationFactory<Program>` for host tests that need their own database (research R-11). Constructor takes the database name (`HT-UI-SMOKE`, `HT-UI-KEYS`) and a settings dictionary; the connection string is `HT_UI_CONNECTION` (or `HostFixture`'s local default) with `InitialCatalog` replaced, as `StartupTasksHarness` derives `HT-UI-MIGRATE`; it calls `HostFixture.EnsureResettable` on the derived name. `ConfigureWebHost` sets environment `Testing`, `ConnectionStrings:HabitTracker`, and every dictionary entry through `builder.UseSetting`, and adds a `ListLoggerProvider`. `public static Task ResetAsync(string databaseName)` calls `HtDatabase.ResetAsync` on the derived connection string. `CreateBrowser()` mirrors `HostFixture.CreateBrowser` (HTTPS base address, cookies, no auto-redirect). Add `[CollectionDefinition("DerivedDatabaseHosts", DisableParallelization = true)]`. The class comment says why it departs from ADR 0006's single `HT-UI` (settings or a database state the shared host cannot have) and that CI needs no extra variable.

### Smoke check core

- [X] T011 Implement `tools/HT.SmokeCheck/PageReader.cs` (public static, `[GeneratedRegex]` patterns): `ReadToken(string html, string path)`, `ReadHandlers(string html)`, `ReadFirstHeading(string html)`, `ReadHabitNames(string html)` (the visible name of each habit card as `HabitCard` renders it; read `src/HT.Ui/Components/**/HabitCard.razor`), `ReadAssetUrls(string html)`, `ContainsAll(IEnumerable<string> found, IEnumerable<string> expected)` with ordinal comparison. T006 passes.
- [X] T012 Implement `SmokeCheckCommand` in `tools/HT.SmokeCheck/SmokeCheckCommand.cs` and the public record `SmokeCheckOptions` in `tools/HT.SmokeCheck/SmokeCheckOptions.cs`: `Parse(string[] args, IDictionary environment)` returns options or a usage error; `RunAsync` writes the usage line and returns 2 on error; otherwise it builds a client factory (each call: a new handler from `createHandler`, or a new `HttpClientHandler` with its own `CookieContainer` and `AllowAutoRedirect = false`; base address = the URL) and runs `SmokeCheckRunner`, returning 0 or 1. It never writes an environment value. T007 passes.
- [X] T013 Implement `tools/HT.SmokeCheck/SmokeCheckRunner.cs` and the public record `SmokeCheckResult` (check lines, timings, `Passed`): `public sealed class SmokeCheckRunner(Func<HttpClient> createClient, SmokeCheckOptions options, TextWriter output)` with `RunAsync()`. Check 1 `home`: `GET /` until 200 whose first `h1` is the visitor Home's, or until the warm-up time ends (5-second pause, shortened to the remaining time), recording the duration of the first request and whether a retry was needed. Check 2 `https-redirect`: `GET http://<host>/` expects 301, 302, 307 or 308 with `Location` `https://<host>/`; `SKIP` with `--skip-http-redirect`. A private `SignInAsync(HttpClient, email, password)`: `GET /sign-in`, token and the `sign-in` handler, `POST /sign-in` with `Input.Email`, `Input.Password`, `__RequestVerificationToken`, `_handler`; success is a redirect whose `Location` is not `/sign-in`. Output exactly as "Output lines fixed by these tasks"; a `FAIL` reason never contains a password, the administrator email or page content. Checks 3–5 come in Phase 4.
- [X] T014 Implement the `TIMING` line in `tools/HT.SmokeCheck/SmokeCheckRunner.cs` (contract "Timings"): `first` from check 1, with `(retried)` when a retry was needed; `later` stays `n/a` until T025 adds the demo sign-ins that measure it. Timings never fail a check. T008 passes.

### Provisioning (creates the hosted demo)

- [X] T015 [P] Write `tools/hosting/selftest.ps1` first, in the style of `tools/pr-gates/selftest.ps1`: dot-source `tools/hosting/HostingFunctions.ps1` and assert that `Test-DemoAdministratorEmail` accepts `admin@example.com` and `Admin@Example.com`, and refuses `admin@example.org`, `admin@sub.example.com`, `admin@example.com.evil`, `a@b@example.com`, an empty and a whitespace value; that `Invoke-WithSecretFile` passes a file path containing the value to its script block and the file is gone after a normal return and after the block throws; that `Test-PowerShellParses` returns false for a temporary file with a syntax error and true for a valid one; and that `tools/hosting/provision.ps1` and `tools/hosting/teardown.ps1` parse. Print `PASSED` or `FAILED` with the failing assertion; exit 1 on failure. Fails until T016–T018.
- [X] T016 [P] Create `tools/hosting/HostingFunctions.ps1` with `Test-DemoAdministratorEmail` (exactly one `@`, domain `example.com` case-insensitive, HP-014), `Invoke-WithSecretFile([string]$Value, [scriptblock]$Action)` (writes the value without a trailing newline to a new file in `[IO.Path]::GetTempPath()`, invokes `$Action` with the path, deletes the file in `finally`), and `Test-PowerShellParses([string]$Path)` (`[System.Management.Automation.Language.Parser]::ParseFile($Path, [ref]$tokens, [ref]$errors)` with both variables declared first; true when `$errors.Count -eq 0`).
- [X] T017 Create `tools/hosting/provision.ps1` (research R-03, R-02, R-09; contracts/hosted-configuration.md), PowerShell 7, `Set-StrictMode -Version Latest`, `$ErrorActionPreference = 'Stop'`, dot-sourcing `HostingFunctions.ps1`. Parameters: `-Subscription`, `-Location`, `-ResourceGroup`, `-PlanName`, `-WebAppName`, `-SqlServerName`, `-SqlDatabaseName`, `-Repository` (`owner/name`), `-RotateSqlPassword`. In order, each idempotent (show before create; settings set again on every run):
  1. `az account set`; stop unless `az webapp list-runtimes --os linux` contains `DOTNETCORE:10.0`;
  2. resource group; App Service plan `--is-linux --sku B1`; web app `--runtime "DOTNETCORE:10.0"`;
  3. web app config: `httpsOnly` true, `minTlsVersion` 1.2, `alwaysOn` true, `webSocketsEnabled` true, one instance; `az webapp log config --docker-container-logging filesystem`; file-system log retention 30 days through the web app's `config/logs` resource (`az resource update`), read back, stop if not 30;
  4. SQL server with SQL authentication. When the server is missing, or with `-RotateSqlPassword`: generate a password (`RandomNumberGenerator`, 32 letters and digits), create the server (`--admin-password @<file>`) or update it (`az sql server update --admin-password @<file>`), both through `Invoke-WithSecretFile`, and **at once** write the connection string `HabitTracker` (type `SQLAzure`, the server's FQDN, database name, `Encrypt=True`) with `az webapp config connection-string set ... --settings @<file>`, before any further step, so a later failure cannot lose the password. When the server exists and the web app has no `HabitTracker` connection string, stop with "the SQL password is unknown: run again with -RotateSqlPassword";
  5. the Basic database (`--service-objective Basic`) when missing; firewall rules `webapp-out-<n>` for each address of the web app's `possibleOutboundIpAddresses`; no "allow Azure services" rule;
  6. app settings `DataProtection__KeysDirectory=/home/data-protection-keys` and `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`; for `Operator__AdministratorEmail` (prompt `Read-Host -MaskInput`, repeated until `Test-DemoAdministratorEmail` accepts it), `Operator__AdministratorPassword` and `Operator__DemoPassword` (prompts; an empty answer keeps a stored value, and is refused when none is stored): each through `az webapp config appsettings set --settings @<file>` with a JSON file from `Invoke-WithSecretFile`;
  7. Entra application registration, service principal, federated credential with subject `repo:<Repository>:environment:demo`, role assignment Contributor on the resource group;
  8. with `gh`: environment `demo` with a deployment branch policy allowing only `master`, and the variables `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `AZURE_RESOURCE_GROUP`, `AZURE_WEBAPP_NAME`, `AZURE_SQL_SERVER`, `AZURE_SQL_DATABASE`, `DEMO_BASE_URL`.
  It prints step names and non-secret identifiers only: never a secret value or the administrator email. No Azure call in tests.
- [X] T018 [P] Create `tools/hosting/teardown.ps1`: parameters `-Subscription`, `-ResourceGroup`, `-Repository`, `-ClientId`; asks for confirmation by typing the resource group name; deletes the resource group, the application registration and the GitHub environment `demo`. Then `pwsh -NoProfile -File tools/hosting/selftest.ps1` prints `PASSED` (it parses both scripts).

**Checkpoint**: `dotnet test HabitTracker.slnx` passes (T006–T009 and the existing suite), `selftest.ps1` prints `PASSED`.

---

## Phase 3: User Story 1 - Open the hosted app in a browser (Priority: P1) 🎯 MVP

**Goal**: the host keeps sessions across a restart that is not a deployment or refresh (HP-018, US1-AS6); HTTPS
behind App Service needs only configuration (R-08, set in T017).

**Independent Test**: the key-directory host test passes; on Azure, quickstart B3 and B4 (Phase 8).

### Tests for User Story 1

- [X] T019 [US1] Write `tests/HT.UiTests/Host/DataProtectionKeysTests.cs` in collection `DerivedDatabaseHosts`, on database `HT-UI-KEYS` (T010): (a) reset once; host 1 with `DataProtection:KeysDirectory` set to a new temporary directory and throwaway `Operator:*` values; sign in to a demo account with "Keep me signed in" through `/sign-in` (as `HostClient` does); assert a `key-*.xml` file exists in the directory; dispose host 1; host 2 with the same directory and database, no reset; send host 1's session cookie to `/profile` and assert 200 (not a redirect to `/sign-in`). (b) a host without the setting: after a sign-in its unused temporary directory stays empty. Delete the temporary directories in `Dispose`. Run: (a) fails before T020.

### Implementation for User Story 1

- [X] T020 [US1] In `src/HT.Ui/Program.cs`, before `builder.Build()`: read `builder.Configuration["DataProtection:KeysDirectory"]`; when it is not empty, `builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(value)).SetApplicationName("HabitTracker");` with a comment citing HP-018 and research R-07. Nothing changes when unset. T019 passes; the whole `HT.UiTests` suite passes.

**Checkpoint**: sessions survive a host restart over the same key directory.

---

## Phase 4: User Story 2 - Explore with demonstration data (Priority: P1)

**Goal**: the smoke check proves that the administrator, every demo account with its demo data and the seed
catalog exist (HP-017, SC-005), and a test forces later slices to extend it.

**Independent Test**: `SmokeCheckHostTests` and `CoverageTests` pass; quickstart A2 against a local run.

### Tests for User Story 2

- [X] T021 [P] [US2] Write `tests/HT.UiTests/SmokeChecks/CoverageTests.cs`: reflect over `HT.Application.Operator.IDemoDataStore`; for every method and every parameter except `CancellationToken`, assert that `SmokeCheckCoverage.Entries` has an entry with the same method name, parameter name and parameter type, and that every entry still matches a parameter (no stale entry). The failure message names the missing method and parameter and points to `specs/003-demo-hosting/contracts/smoke-check.md` "Coverage rule". Fails to compile until T023.
- [X] T022 [US2] Write `tests/HT.UiTests/Host/SmokeCheckHostTests.cs` in collection `DerivedDatabaseHosts`, on `HT-UI-SMOKE` (T010), each case resetting the database before building its host and running `SmokeCheckRunner` with `host.CreateBrowser` as the client factory, `--skip-http-redirect` and a short warm-up: (a) throwaway valid operator values: `Passed` is true, every check line is `PASS` or `SKIP https-redirect`; (b) `Operator:DemoPassword` = `x` (fails the password rule, `DemoDataService.LoadAsync`): `catalog` and the three `demo` checks fail, `administrator` passes, timing shows `later n/a`; (a) also shows `later <ms> ms (<n> requests)` with n of at least 1; (c) valid values, then delete one `CustomHabits` row of `demo2@example.com` with SQL (join `AspNetUsers` on `OwnerId`): only `demo demo2@example.com` fails; (d) in every case the captured output contains no password, no administrator email and no custom-habit name of `DemoCustomHabits`. Fails before T023–T025.

### Implementation for User Story 2

- [X] T023 [US2] Create `tools/HT.SmokeCheck/SmokeCheckCoverage.cs`: `public static class SmokeCheckCoverage` with `public static IReadOnlyList<CoverageEntry> Entries` holding the contract's table: (`CreateDemoAccountAsync`, `email`, `Email`, check 5 sign-in), (`CreateDemoAccountAsync`, `password`, `string`, check 5 sign-in), (`CreateDemoAccountAsync`, `customHabits`, `IReadOnlyList<HabitDefinition>`, check 5 names), (`TryCreateSeedCatalogAsync`, `habits`, `IReadOnlyList<SeedCatalogHabit>`, check 3). `CoverageEntry` is a public record (method, parameter, `Type`, check). T021 passes.
- [X] T024 [US2] Add check 3 `catalog` to `tools/HT.SmokeCheck/SmokeCheckRunner.cs`: anonymous `GET /catalog`; expected names = `SeedCatalog.Habits` whose `State` is Published (read `CatalogHabitState` for the member name); pass when `PageReader.ContainsAll`; line `PASS catalog (<found> of <expected>)`. Extend T008's scripted good site with a `/catalog` page listing those names, and add a T008 case where one is missing: `FAIL catalog`.
- [X] T025 [US2] Add checks 4 and 5 to `tools/HT.SmokeCheck/SmokeCheckRunner.cs`: `administrator` signs in with the configured email and password on a new client and expects `GET /admin` 200; `demo <email>` for each of `DemoDataService.DemoEmails`, on a new client each, signs in with the demo password and expects every name of `DemoCustomHabits.For(email)` on `GET /custom-habits`. Then the `later` timing: after the first demo account that signs in, time `GET /custom-habits` plus each URL of `PageReader.ReadAssetUrls`, fetched one after another with the same client; `later n/a` when none signs in. Extend T008: the scripted good site serves `/sign-in`, `/admin`, `/custom-habits` and one stylesheet and one script, and gives `later <ms> ms (3 requests)`; a case where every demo sign-in returns to `/sign-in` gives the `demo` failures and `later n/a`. T022 (whose case (a) also asserts a `later <ms> ms` value) passes; the whole `HT.UiTests` suite and the architecture suite pass (T009 now sees `HT.Domain` referenced).
- [X] T026 [US2] (user) Quickstart A2 on a database of its own: the agent prepares the commands; the user drops `HT-SMOKE-LOCAL` on `.\COLL1` if it exists, starts the app with `ConnectionStrings__HabitTracker=Server=.\COLL1;Database=HT-SMOKE-LOCAL;Integrated Security=True;TrustServerCertificate=True` (a connection without a secret; Development migrates it at start, ADR 0007) and `dotnet run --project src/HT.Ui --launch-profile https`, then runs `dotnet run --project tools/HT.SmokeCheck -- https://localhost:7194 --skip-http-redirect` with the three `HT_SMOKE_*` variables set from the User Secrets by their own means. Expected: `RESULT PASS`, exit 0, no secret or habit name in the output; one variable unset: exit 2; a wrong demo password: the `demo` checks fail, exit 1. The user reports the result lines; the agent records them for the PR description. Never the developer's `HT` database.
  - Result 2026-11-15 (reported by the user): valid values: `PASS home`, `SKIP https-redirect`, `PASS catalog (20 of 20)`, `PASS administrator`, `PASS demo demo1@example.com (2 of 2)` (same for demo2 and demo3), `TIMING first 265 ms, later 42 ms (5 requests)`, `RESULT PASS`; `HT_SMOKE_ADMIN_PASSWORD` unset: `usage error: the environment variable HT_SMOKE_ADMIN_PASSWORD is missing or empty.` and the usage line; wrong demo password: the three `demo` checks `FAIL ...: the sign-in was refused`, `later n/a`, `RESULT FAIL`. No secret or habit name in the output. Exit codes were not printed; the usage error and `RESULT FAIL` paths return 2 and 1 (InvocationTests, RunnerBehaviourTests).

**Checkpoint**: the smoke check passes in process and locally, and fails exactly where demo data is missing.

---

## Phase 5: User Story 3 - Deploy a new version (Priority: P2)

**Goal**: one manual run deploys a chosen commit of `master`: empty database, migrations before the final
start, smoke check, summary (HP-003, HP-004, HP-009, HP-012, HP-016, HP-017, SC-003).

**Independent Test**: quickstart A3 locally; on Azure, quickstart B2, B5, B7, B8 (Phase 8).

### Tests for User Story 3

- [X] T027 [US3] Quickstart A3, the migration bundle trial (research R-05), with integrated security only: `dotnet tool restore`, then `dotnet ef migrations bundle --project src/HT.Persistence --startup-project src/HT.Persistence --self-contained --output <scratchpad>/efbundle/efbundle.exe` (local platform). Create an empty local database `HT-BUNDLE` on `.\COLL1`, set `HT_DESIGN_CONNECTION=Server=.\COLL1;Database=HT-BUNDLE;Integrated Security=True;TrustServerCertificate=True` for that process only, run the bundle: exit 0 and `InitialAccounts` and `HabitCatalog` in `__EFMigrationsHistory`; run it again: exit 0, nothing applied. Drop `HT-BUNDLE`. If the bundle cannot be built or does not read `HT_DESIGN_CONNECTION`, stop and report: research R-05 and the workflow contract change before T029.

### Implementation for User Story 3

- [X] T028 [US3] Create `.github/workflows/deploy-demo.yml` with the trigger, inputs, environment, permissions and concurrency of contracts/deploy-workflow.md ("Trigger and inputs"): `on: workflow_dispatch` only, inputs `mode` (choice `deploy`, `refresh`, `check`, default `deploy`) and `commit` (string, default `master`); `environment: demo`; `permissions: { id-token: write, contents: read }`; `concurrency: { group: deploy-demo, cancel-in-progress: false }`; one job on `ubuntu-latest` with `DOTNET_NOLOGO` and `DOTNET_CLI_TELEMETRY_OPTOUT`. Pin `actions/*` by major version tag as `.github/workflows/sonar.yml` does: `actions/checkout@v5`, `actions/setup-dotnet@v4` (copy sonar.yml's comment on why not v5). Pin the third-party `azure/login` by the full commit SHA of its latest v2 release with a `# v2.x.y` comment: the project's SonarQube rule `githubactions:S7637` trusts only first-party owners, and PR 1's Sonar gate is on the critical path (research R-15). Read the SHA from the release's tag on GitHub; do not guess it. No `set -x` anywhere.
- [X] T029 [US3] Add contract steps 1–4 to `.github/workflows/deploy-demo.yml`. Each contract step is one or more workflow steps with `id: s<n>_<k>` and `name: "<n>.<k> <text>"`, `<n>` the contract number: (1.1) fail unless `github.ref == 'refs/heads/master'`; (1.2) `azure/login` with the three `vars.AZURE_*` ids; (2.1) `actions/checkout` of `master` with `fetch-depth: 0`; (2.2) in `deploy`, `git merge-base --is-ancestor <commit> origin/master` and `git rev-parse` to a full SHA; in `refresh`/`check`, read `Deployment__Commit` with `az webapp config appsettings list --query` and fail with "no deployed commit; run deploy" when empty; (3.1) read the connection string `HabitTracker` and `Operator__AdministratorEmail`, `Operator__AdministratorPassword`, `Operator__DemoPassword`, each followed at once by `echo "::add-mask::$value"` before it is written to a step output; an empty value fails with `configuration incomplete: <key>` (key name only); (4.1) `git checkout` of the resolved SHA; (4.2) `actions/setup-dotnet` 10.0.x; (4.3) `dotnet tool restore` and Release builds: always `tools/HT.SmokeCheck`; in `deploy` `dotnet publish src/HT.Ui -c Release -r linux-x64 --self-contained false -o publish` zipped to `publish.zip`; in `deploy`/`refresh` `dotnet ef migrations bundle --project src/HT.Persistence --startup-project src/HT.Persistence --self-contained -r linux-x64 --output efbundle`.
- [X] T030 [US3] Add contract steps 5–11 to `.github/workflows/deploy-demo.yml`, same naming: (5.1) runner address from `https://api.ipify.org`, delete a leftover rule `deploy-demo-runner`, `az sql server firewall-rule create` named `deploy-demo-runner`; (5.2) `az sql db delete --yes` only if `az sql db show` finds the database; (6.1) `deploy` only: `az webapp start`; (7.1) `deploy` only: set `Deployment__Commit` to empty, `az webapp deploy --type zip --src-path publish.zip`, set `Deployment__Commit` to the full SHA; (8.1) `az webapp stop`; (9.1) `az sql db create --service-objective Basic`; (10.1) run `./efbundle` with `HT_DESIGN_CONNECTION` from the masked output in `env:` only, retried up to 3 times 60 seconds apart when it fails; (11.1) `az webapp start`, then write `date -u` to an output.
- [X] T031 [US3] Add contract steps 12 and 13 to `.github/workflows/deploy-demo.yml`: (12.1) run the built smoke check against `vars.DEMO_BASE_URL` with `HT_SMOKE_DEMO_PASSWORD`, `HT_SMOKE_ADMIN_EMAIL`, `HT_SMOKE_ADMIN_PASSWORD` from the masked outputs in `env:` only; (13.1) `if: always()`: delete the firewall rule `deploy-demo-runner` if present; (13.2) `if: always()`: write to `$GITHUB_STEP_SUMMARY` the mode, the full SHA, `succeeded` or `failed`, the start time of 11.1, and for a failure the first failed workflow step. 13.2 never reads `toJSON(steps)`, which would also carry the step-3 outputs: its `env:` has one variable `OUTCOMES` written out in the workflow file as lines `<n>.<k> <name>=${{ steps.s<n>_<k>.outcome }}`, one per step from 1.1 to 13.1 in contract order, with the same names as the steps; the script prints the first line whose value is `failure` (a skipped step is `skipped`, not a failure). Only non-secret outputs (the resolved SHA, the start time) are read by name.
- [X] T032 [US3] Review `.github/workflows/deploy-demo.yml` against contracts/deploy-workflow.md row by row (Modes column, "Re-running after a failure", "Guarantees") and the hosted-configuration "Never" list: every secret masked before use, none in a `with:` input or a command argument (secrets reach commands only through `env:`), none echoed. If `actionlint` is available run it; otherwise record "workflow not executed: verified in Phase 8" for the PR description.

**Checkpoint**: the workflow file exists and matches its contract; it can run only after PR 1 is merged.

---

## Phase 6: User Story 4 - Keep the demo usable (Priority: P3)

**Goal**: the Operator refreshes the hosted demo and runs the exam-day and weekly checks (HP-011, US4,
SC-006).

**Independent Test**: on Azure, quickstart B6, B7 and B10 (Phase 8).

- [X] T033 [US4] In `.github/workflows/deploy-demo.yml`, make the `refresh` path skip contract steps 6 and 7 (`if: inputs.mode == 'deploy'`) and run steps 5 and 8–13; make the `check` path run only steps 1–4 (the smoke check build only) and 12–13. Cross-check every step's `if:` against the Modes column of contracts/deploy-workflow.md.

**Checkpoint**: one workflow serves deployment, refresh and check.

---

## Phase 7: Polish and cross-cutting concerns

**Purpose**: Operator documentation (HP-015), repository docs, the full verification and PR 1.

- [X] T034 [P] Write `docs/operations/hosted-demo.md` (research R-10): provisioning with `tools/hosting/provision.ps1` and its prompts; recovery when a first run stopped after creating the SQL server (`-RotateSqlPassword`); secret rotation (`-RotateSqlPassword`, then a refresh); the GitHub environment `demo`; running `deploy`, `refresh` and `check` (Actions → Deploy demo → Run workflow); reading the run summary (start time, failed step) and the operator log (Kudu `LogFiles` of the web app, `az webapp log tail`; entries before the start time are the starts without a database); the exam-day (2026-11-23) and weekly `check` runs and the SC-001 rule (only idle `check` runs, not `(retried)`); the monthly cost against USD 25; the SC-004 search; the items the exam submission gives reviewers: published address and demo emails written out (`demo1@example.com`–`demo3@example.com`), demo password, administrator email and administrator password named but never written; teardown after 2026-12-31. No secret value, no administrator email.
- [X] T035 [P] Update `README.md`: add `tools/` (`HT.SmokeCheck` smoke check; `hosting/` provisioning scripts; existing `pr-gates/`, `guard-oracle/`) to "Repository layout", and link `docs/operations/hosted-demo.md` from a short "Hosted demo" paragraph.
- [X] T036 [P] Update `CLAUDE.md`: in "Layout and layer rules", note `tools/HT.SmokeCheck` (a .NET tool, references `HT.Application` only, enforced by `SmokeCheckDependencyTests`); in "Build and test", add "Changed `tools/hosting/`? Run `pwsh -NoProfile -File tools/hosting/selftest.ps1` — it must print `PASSED`." and that host tests also use the derived databases `HT-UI-SMOKE` and `HT-UI-KEYS`.
- [X] T037 [P] Align the plan artifacts with the names and output lines fixed above: in `specs/003-demo-hosting/contracts/smoke-check.md`, the checks class `SmokeCheckRunner`, the types `SmokeCheckOptions`, `SmokeCheckResult` and `CoverageEntry`, the `createHandler` parameter, and the lines `PASS <name>` without counts, `SKIP https-redirect` and `later n/a`; in `specs/003-demo-hosting/plan.md` "Source Code", `SmokeCheckRunner.cs`, `SmokeCheckOptions.cs`, the test folder `SmokeChecks/` and `RunnerBehaviourTests.cs`, `HostingFunctions.ps1` and `DerivedDatabaseHost.cs`. The coverage rule that T021's message cites is unchanged.
- [X] T038 Run the full verification in the tree to be committed: `dotnet build HabitTracker.slnx`, `dotnet test HabitTracker.slnx --nologo -v q --logger "console;verbosity=minimal"`, `pwsh -NoProfile -File tools/hosting/selftest.ps1`. Report counts as `PASS` lines or the failing test with `file:line` (constitution V).
- [X] T039 (user) Quickstart A4: the user searches the repository, `.github/workflows/deploy-demo.yml` and `tools/hosting/` for the values in their local User Secrets with their own tool, and reports hit or no hit. Expected: no hit.
  - Result 2026-11-15 (reported by the user, values never shown): `Operator:AdministratorPassword` and `Operator:DemoPassword` in 0 tracked files. `Operator:AdministratorEmail` in 43 files: the local value is the repository's placeholder `admin@example.com` (confirmed by an equality check printing `True`), used by tests and docs, so no secret is exposed. Consequence for T041: the hosted administrator email must be another `example.com` address that appears nowhere in the repository (HP-015).
- [ ] T040 When the user asks: commit, open PR 1 against `master` (description: links to spec, plan and ADR 0015; constitution principles touched I, III, V, VI, with II and IV not touched as in the plan's Constitution Check; test counts; the results of T026, T027 and T039; "unverified until Phase 8": the workflow, Azure settings, forwarded headers, timings, SC-001, SC-003, SC-006, SC-007, SC-008), then `/process-pr-feedback`. Never merge.

**Checkpoint**: PR 1 open, all local checks green, hosted items listed as unverified.

---

## Phase 8: Hosted verification (after PR 1 is merged)

**Purpose**: quickstart Part B on Azure; the evidence for every hosted requirement; PR 2. Every task here is
**(user)**: the Operator runs it; the agent prepares commands and records results. Latest date for T042:
2026-11-22 (SC-008, research R-15).

- [X] T041 [US1] (user) Quickstart B1: run `tools/hosting/provision.ps1` (possible from the PR 1 branch, before the merge) and confirm: `example.com` refusal, resources of ADR 0015, environment `demo` with its branch policy and variables, log retention 30, nothing secret printed.
  - Result 2026-11-15: Pay-As-You-Go, North Europe. `rg-ht-demo`; plan `plan-ht-demo` (Linux B1, 1 instance); web app `ht-demo-kbfh` (https://ht-demo-kbfh.azurewebsites.net; HTTPS only, TLS 1.2, Always On, WebSockets, .NET 10); SQL server `sql-ht-demo-kbfh` (SQL authentication); database `habittracker` (Basic); firewall `webapp-out-1` to `webapp-out-18`, no allow-Azure rule; log retention read back 30 days; Entra app `ht-demo-kbfh-github-deploy` with Contributor on `rg-ht-demo` only; environment `demo` (master only) with its 8 variables. The agent created the non-secret parts with the script's own commands; the user ran `provision.ps1` for the SQL server, database, firewall and the three values. `admin@example.org` was refused at the prompt. The first administrator email entered was the repository placeholder and was replaced on a second run (HP-015). The console showed no secret and no administrator email. First-run fix: the plan check read `reserved` (null in the current Azure CLI) and now reads `kind`.
- [X] T042 [US3] (user) Quickstart B2: first `deploy` run of `master`; record duration (SC-003), summary and smoke check lines. Fix anything it shows on a branch for PR 2 and repeat.
  - Result 2026-11-15: run 43727785166, `deploy` of `master` at `5d4b1e1`, succeeded in 6 min 53 s (job 15:04:15–15:11:08 UTC; SC-003: within 15 minutes). Every step 1.1–13.2 succeeded; the bundle applied `InitialAccounts` and `HabitCatalog` on the first attempt. Smoke check: `PASS home`, `PASS https-redirect`, `PASS catalog (20 of 20)`, `PASS administrator`, `PASS demo demo1@example.com (2 of 2)` (same for demo2 and demo3), `TIMING first 13902 ms (retried), later 1160 ms (5 requests)` (the cold start after deployment, not an SC-001 sample), `RESULT PASS`. Secrets appear as `***` in the log. `Deployment__Commit` holds the full SHA; `http://` gives 301 to `https://`. Two earlier attempts (run 43724903852) failed at 1.2 (OIDC subject, AADSTS700213) and at 4.5 (`dotnet tool restore` on the .NET 10 SDK); fixed in PR #27.
- [X] T043 [US1] (user) Quickstart B3 and B4: browser walk from a machine that never ran the app, timed (SC-002); `http://` redirect; `az webapp restart` and still signed in (HP-018); operator log shows demo loading reporting existing accounts (US2-AS3).
  - Result 2026-11-15, completed: from a fresh InPrivate window, opening the address, signing in as demo1 and seeing its custom habits took about 15 s (SC-002: under 3 minutes). The catalog shows the seed habits; a new account registers and signs in; each demo account sees only its own two custom habits; the administrator reaches Admin and sees no private data.
  - Earlier partial result 2026-11-15: `http://` gives 301 to `https://`; the visitor Home is served; demo accounts show their custom habits; the administrator (the hosted address, not the placeholder) signs in. `az webapp restart` at 15:53:57 UTC, new process started 15:55:44; the user, signed in with "Keep me signed in", stayed signed in after reloading (HP-018). The operator log for that start shows the administrator and the three demo accounts as already existing and the catalog as not empty (US2-AS3). Observed: at 15:47:20 one sign-in request of demo1 failed with a transient `SqlException` (connection reset during the SQL login); the user saw a delay of about 10 s and retried successfully. Still open: the SC-002 timing from a fresh browser, the catalog, registration, each demo account seeing only its own habits, and the administrator seeing no private data.
- [ ] T044 [US3] (user) Quickstart B5 (first part) and B8: operator log through Kudu after the summary's start time (HP-010, US3-AS3); a merge without a run changes nothing (HP-016).
- [ ] T045 [US4] (user) Quickstart B6 and B7: refresh after reviewer changes (HP-011); failure at step 3 with a renamed setting; failure at step 12 with a demo password that fails the password rule (HP-017, US4-AS5); recovery from a stopped web app with `deploy`.
- [ ] T046 [US2] (user) Quickstart B9: SC-004 search of the repository, the downloaded run logs of T042 and T045 and the operator log for the hosted secret values.
- [ ] T047 (user) PR 2 when the user asks: the results of T041–T046 in the description, any fixes, `docs/roadmap.md` row 3 `shipped` with Horizon `—` and both PRs in Shipped. The description lists as **still unverified, with their dates**: SC-001 and SC-006 (T049), SC-007 (T049), and the 30-day log retention (T048).
- [ ] T048 [US3] (user) Quickstart B5, last part, at the first weekly check: the first deployment's operator log lines are still there (HP-010, research R-09). If they are gone, report it: R-09's retention needs another mechanism.
- [ ] T049 [US4] (user) Quickstart B10: `check` run on 2026-11-23 and at least weekly until 2026-12-31, after at least 30 minutes without requests, until at least 10 such runs exist; month-to-date cost against USD 25 (SC-001, SC-006, SC-007).
- [ ] T050 (user) Close-out after the last T049 run: the user posts one line per criterion (SC-001 tally, SC-006 pass/fail per date, SC-007 highest monthly cost, T048's outcome) as a comment on PR 2; the evidence stays in the workflow's run history (research R-10), so no further change to the repository and no third PR. Then `tools/hosting/teardown.ps1` after 2026-12-31.

---

## Dependencies & Execution Order

### Phase dependencies

- **Phase 1 (Setup)**: none. T001 first.
- **Phase 2 (Foundational)**: after Phase 1. Blocks every story.
- **Phase 3 (US1)**: after Phase 2 (uses T010).
- **Phase 4 (US2)**: after Phase 2 (extends T013/T014, uses T010).
- **Phase 5 (US3)**: after Phase 4 (step 12 runs the complete smoke check) and T017 (variables, settings).
- **Phase 6 (US4)**: after Phase 5 (same workflow file).
- **Phase 7 (Polish)**: T034–T037 any time after Phase 2; T038–T040 after Phases 3–6.
- **Phase 8 (Hosted)**: T041 after T017; T042–T047 after PR 1 is merged, in order T042 → T043/T044 → T045 → T046 → T047; T048 at the first weekly check; T049 on its dates; T050 last.

### Within phases

- Tests before implementation: T006 → T011, T007 → T012, T008 → T013/T014, T015 → T016–T018, T019 → T020, T021 → T023, T022 → T024/T025. T009 is a guard and passes from the start.
- `SmokeCheckRunner.cs` is edited by T013, T014, T024 and T025 in that order; `deploy-demo.yml` by T028–T033 in order.

### One test project

All new tests compile in `HT.UiTests`. A test written first that does not compile yet (T006–T008, T021, T022)
stops every other test in that project from running. So a test-first task and its implementation are done
together before another story's tests are started: Phase 3 and Phase 4 touch different files, but run one after
the other (US1, then US2) unless the not-yet-compiling test file is kept out of the build until its turn.

### Parallel opportunities

- Phase 1: T005 alongside T002–T004.
- Phase 2: T009, T015, T016, T018 alongside the smoke check work; T006, T007 and T008 can be written together,
  then T011–T014 make them compile in one go.
- Phase 7: T034, T035, T036, T037 together.

## Parallel Example: Phase 2

```text
Task: "Write the three smoke check test files (T006, T007, T008), then T011–T014"
Task: "Write tools/hosting/selftest.ps1 and HostingFunctions.ps1 (T015, T016), then provision.ps1 and teardown.ps1 (T017, T018)"
Task: "Write tests/HT.ArchitectureTests/SmokeCheckDependencyTests.cs (T009)"
```

## Parallel Example: Polish

```text
Task: "Write docs/operations/hosted-demo.md (T034)"
Task: "Update README.md (T035)"
Task: "Update CLAUDE.md (T036)"
Task: "Align contracts/smoke-check.md and plan.md with the fixed names (T037)"
```

---

## Implementation Strategy

### MVP first

1. Phases 1 and 2: the roadmap row, the tool core, the test host, the provisioning scripts.
2. Phase 3 (US1): sessions survive restarts.
3. Phase 4 (US2): the smoke check proves the demo data. **Stop and validate**: T026 (quickstart A2).
4. Phase 5 (US3): the workflow can deploy. Without it there is no hosted demo, so the reviewer-facing MVP is
   Phases 1–5 together with T041–T043 on Azure.

### Incremental delivery

1. PR 1 = Phases 1–7 (code, scripts, workflow, docs), by the dates of research R-15.
2. Operator: provisioning (T041), first deployment (T042), hosted checks (T043–T046) by 2026-11-22.
3. PR 2 = hosted results and fixes, with the time-bound criteria listed as unverified; roadmap `shipped`.
4. First weekly check (T048), exam day and weekly checks (T049) until 2026-12-31, close-out (T050).

---

## Notes

- 50 tasks: Setup 5, Foundational 13, US1 2 (+2 hosted), US2 6 (+1 hosted), US3 6 (+3 hosted), US4 1 (+2 hosted),
  Polish 7, hosted without a story 2 (PR 2, close-out).
- The agent never runs `provision.ps1`, `teardown.ps1` or the workflow, and never reads a local or hosted secret
  value; tasks marked (user) are the user's.
- The demo-data gate at the top is re-checked if any slice ships before implementation starts.
