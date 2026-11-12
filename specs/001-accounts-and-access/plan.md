# Implementation Plan: Accounts and access

**Branch**: `feature/001-accounts-and-access` | **Date**: 2026-11-11 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/001-accounts-and-access/spec.md`; acceptance criteria
[acceptance.md](acceptance.md) and `tests/HT.AcceptanceTests/Features/F001/` (both locked). Revised after
[critic-plan-pass-1.md](critic-plan-pass-1.md) and [critic-plan-pass-2.md](critic-plan-pass-2.md).

## Summary

People register, sign in (with a 5-minute block after 5 failures), sign out and change their password; the
app tells anonymous visitors, registered users and administrators apart; an administrator and three demo
accounts are created at startup from secret configuration. The account and its rules live in `HT.Domain`;
the use cases are Application services with an explicit acting user and results that carry the spec's text;
`HT.Persistence` implements the account store on ASP.NET Core Identity's user records over EF Core and SQL
Server; `HT.Ui` keeps only the browser session and renders the slice's pages in the shared shell (ADR 0004,
ADR 0005). The slice adds the first migration, the two Persistence entry points the AC tests need, the
architecture rules for every layer, and the integration and UI test projects (ADR 0006).

## Technical Context

**Language/Version**: C# on .NET 10 (`net10.0`, SDK 10.0.4xx), nullable and implicit usings on, warnings as
errors (`Directory.Build.props`).

**Primary Dependencies**: ASP.NET Core 10 (Blazor static server rendering plus one interactive component),
ASP.NET Core Identity (user store, password hasher, security stamp, sign-in manager), EF Core 10 with the SQL
Server provider. Package list: [research.md R-01](research.md#r-01-packages-and-versions).

**Storage**: SQL Server. Databases: `HT` (development app), `HT-IT` (integration tests), `HT-UI` (host tests),
`HT-AC` (acceptance tests), all on `.\COLL1` locally with integrated security (ADR 0003, ADR 0006). Schema via
EF Core migrations; first migration `InitialAccounts` (research R-15).

**Testing**: xUnit v2 everywhere; ArchUnitNET for layer rules; `Microsoft.Extensions.TimeProvider.Testing`
for the clock; bUnit (form components with their messages, navigation, pages without a session) and
`WebApplicationFactory` host tests (cookies, redirects, revalidation, startup log) in `HT.UiTests`, as ADR
0006 splits them and research R-16 details; hand-written fakes, no mocking library (ADR 0006). Acceptance tests: Reqnroll in `tests/HT.AcceptanceTests`, outside the
solution (ADR 0003).

**Target Platform**: browser-based web app over HTTPS; developer machines on Windows; CI on Ubuntu with a SQL
Server started as a step in the CI job (research R-14). Hosting is slice 3.

**Project Type**: web application (Blazor host) over a layered .NET solution.

**Performance Goals**: demo scale. SC-001 (register in under 2 minutes) and SC-002 (sign in in under 30
seconds) are user-task times, met by single-request forms; no throughput target.

**Constraints**: no clock or culture reads in Domain or Application (constitution II); no custom crypto,
password storage or session handling (CLAUDE.md); no secret in source control, logs or results (HP-017,
HP-019); pages only from `design/` tokens and `ht-` classes (constitution IV); every page meets constitution
IV's accessibility rules (SC-007).

**Scale/Scope**: a handful of accounts (demo plus reviewers); 5 pages (Home in two variants, Sign in,
Register, Profile, Privacy) and the shell; 4 Application use cases for users, 2 operator capabilities.

## Constitution Check

*GATE: checked before Phase 0 and again after Phase 1. Both times: pass, no exceptions.*

| Principle | Status | How this plan meets it |
|---|---|---|
| I. Ubiquitous Domain Language | met | Type names use the glossary: `Account`, `ActingUser`, `Role.RegisteredUser` / `Role.Administrator`, `SignInBlock`, "operator". `CONTEXT.md` already has Account, Acting user, Administrator, Anonymous visitor, Operator, Registered user and Sign-in block; the implementation adds "Demo account" (synthetic account created by demo loading; NOT a real person's account) in the same change. |
| II. Deterministic, Reproducible Evaluation | met | The block rule is a pure function of stored state and a supplied time; `AccountService` takes `TimeProvider` by injection. An architecture rule forbids clock, time-zone and current-culture reads in Domain and Application (research R-13). Unit tests cover both sides of each boundary: 4:59 / 5:00 after the fifth failure, 4 / 5 failures, 7 / 8 characters, 254 / 255 characters. |
| III. Privacy Enforced Below the UI | met | Account details and password change take the acting user first and act only on its account; no method takes another account's identifier (FR-010). The account store port is internal, so no component can reach its lookups; an architecture rule keeps components away from the operator services (research R-13). Negative tests for anonymous visitor, another registered user and an administrator exist at unit level and in AC-026 to AC-038. Logs contain no password (HP-017). |
| IV. Design-System Fidelity and Accessible UI | met | `design/` changes first: the spec's "Design changes before build" list, plus the avatar menu markup, the reconnect notice, the unexpected-error bar and the Error page (EmptyState in the normal layout). Then `tokens.css` and `ht-components.css` are copied to `wwwroot/css`, and every template style with hard-coded values goes (`app.css` validation colours, `MainLayout.razor.css`, `ReconnectModal.razor.css`). Each design component used is built once as a Razor component. Focus ring, 4.5:1 contrast, labelled menu and avatar buttons, Esc and outside click close both menus (SC-007). |
| V. Verified, Not Asserted | met | Build and the full suite run before every report of done. This slice adds the first production types to Domain, Application and Persistence, so it adds their ArchUnitNET rules (research R-13). The pre-commit gate stays on. |
| VI. Demonstrable Vertical Slices | met | Domain rule, use case, persistence, UI and tests ship together; demo loading adds the first synthetic `example.com` accounts; no infrastructure beyond what ADRs 0003, 0006 and 0007 require. |
| Decision Discipline | met | No SPEC §11 question is touched (HP-015 keeps time-zone text out). The ARCHITECTURE §9 choices this slice needs are decided in ADRs 0004 to 0009. The one spec gap found while planning (maximum email length) was answered by the owner (see "Spec discrepancies"). The roadmap gate R8 before `/speckit-tasks` is answered (see "Gates before the next step"). |

## Project Structure

### Documentation (this feature)

```text
specs/001-accounts-and-access/
├── spec.md              # /speckit-specify, /speckit-clarify
├── acceptance.md        # /speckit-ac-translate (locked)
├── ac/                  # locks and AC run records
├── plan.md              # this file
├── research.md          # Phase 0
├── data-model.md        # Phase 1
├── quickstart.md        # Phase 1
├── contracts/
│   ├── application-api.md
│   └── host.md
└── tasks.md             # /speckit-tasks (not created here)
```

### Source Code (repository root)

```text
src/
├── HT.Domain/Accounts/                 # AccountId, Role, Email, PasswordRule, SignInBlock, Account, ActingUser
├── HT.Application/
│   ├── Accounts/                       # AccountService, IAccountStore (internal), results, AccountTexts
│   ├── Operator/                       # OperatorService, DemoDataService, results, OperatorTexts
│   └── ApplicationServiceCollectionExtensions.cs   # AddHtApplication
├── HT.Persistence/
│   ├── Identity/                       # HtUser, HtUserClaimsPrincipalFactory, EmailLookupNormalizer,
│   │                                   #   AccountStore (IAccountStore)
│   ├── HtDbContext.cs
│   ├── HtDbContextFactory.cs           # design-time factory for dotnet-ef
│   ├── HtDatabase.cs                   # ResetAsync, MigrateAsync
│   ├── Migrations/                     # InitialAccounts
│   └── PersistenceServiceCollectionExtensions.cs   # AddHtPersistence
└── HT.Ui/
    ├── Account/                        # AccountSession, ActingUserAccessor, sign-out endpoint,
    │                                   #   revalidating auth state provider
    ├── Operator/StartupTasks.cs
    ├── Components/
    │   ├── Layout/                     # MainLayout, SiteNav (interactive: TopNav + MobileNav + avatar menu),
    │   │                               #   Footer, ReconnectModal (restyled)
    │   ├── Account/                    # SignInForm, RegisterForm, ChangePasswordForm (use cases, messages)
    │   ├── Shared/                     # HtButton, HtCard, TextField, Notice, Avatar, EmptyState, …
    │   └── Pages/                      # Home, SignIn, Register, Profile, Privacy, NotFound, Error
    ├── wwwroot/css/                    # tokens.css, ht-components.css (copied from design/)
    └── Program.cs

tests/
├── HT.UnitTests/                       # Domain rules, Application services with fakes
├── HT.ArchitectureTests/               # one rule class per layer, clock-read rule
├── HT.IntegrationTests/                # new: store, mapping, unique index race, parallel block, migrations
├── HT.UiTests/                         # new: bUnit components, WebApplicationFactory host tests
└── HT.AcceptanceTests/                 # outside the solution; written by the AC workflow

design/                                 # changed first (see Work order, step 1)
.github/workflows/sonar.yml             # SQL Server step, skip check, test connection variables
dotnet-tools.json                       # adds dotnet-ef
README.md                               # prerequisites, getting started, status
```

**Structure Decision**: the four production projects of ARCHITECTURE §3 stay as they are, each gaining its
first feature folder (`Accounts`, `Operator`); two test projects join the solution as ADR 0006 lays out.
`HT.Ui/Account` holds the only code that touches `HtUser` and `SignInManager<HtUser>` besides the host's
composition in `Program.cs` (ADR 0004), outside `HT.Ui.Components`, so the architecture rule keeps components
away from Persistence.

## Work order

The order `/speckit-tasks` should follow, because later steps need earlier ones:

1. **Design first** (constitution IV): the spec's "Design changes before build" list; the avatar menu's
   markup and states in `TopNav` and `MobileNav`; guidance for the reconnect notice, the unexpected-error
   bar and the Error page (EmptyState in the normal layout, brand-book wording).
2. **Domain**: the account types and rules, with unit tests; Domain architecture rules.
3. **Application**: the services, the internal store port, results and texts, with unit tests against a fake
   store and a fake clock (including lost-race retries); Application architecture rules and the clock-read
   rule.
4. **Persistence**: `HtUser`, `HtDbContext` with the collation and save-behaviour settings, the normalizer, the
   store, the claims factory, the design-time factory, `InitialAccounts`, the three entry points;
   `HT.IntegrationTests` (including 10 parallel wrong attempts, HP-020 and an apostrophe email); Persistence
   architecture rules.
5. **Host**: cookie, sign-in manager, revalidation, `AccountSession`, acting user, sign-out endpoint, startup
   tasks (one scope per step), configuration and User Secrets id. Create `HT.UiTests` here with its host-test
   collection fixture, the `HT-UI` guard test, the HP-022 `StartupTasks` tests, the revalidation-provider
   test and the host tests for this step (startup log, sign-out, security-stamp check). Ui architecture rules:
   components away from `SignInManager<>` and the operator services; only `HT.Ui.Operator` uses the
   operator services.
6. **Pages and shell**: CSS and removal of template styles, shared components, the three form components,
   `SiteNav`, Footer, the five pages, NotFound and Error; bUnit tests for the form components, `SiteNav`,
   Footer, Home and Privacy; host tests for the pages' cookies, redirects, `returnUrl` and `no-store`.
7. **CI and docs**: SQL Server as a conditional step and the extended skip check in `sonar.yml` (research
   R-14); `CONTEXT.md` ("Demo account"); `README.md` (prerequisites: SQL Server and the operator secrets;
   getting started; the layer-rules and status sentences); `CLAUDE.md` "Build and test" (run with
   `--launch-profile https`; the solution's tests need SQL Server); roadmap status.
8. **AC tests**: `/speckit-ac-tests-write` and `/speckit-ac-tests-analyze`, started by the owner after
   `/speckit-implement`.

## Gates before the next step

- **Roadmap decision R8** ("Delivery time against the exam deadline") had to be answered before slice 1's
  `/speckit-tasks` (`docs/roadmap.md`, "Decisions on this roadmap"). Answered by the owner on 2026-11-11: the
  deadline is out of scope for planning; whatever has shipped by the exam on 2026-11-23 is presented. R5
  follows: no cut, slices ship in roadmap order. The gate is clear.

## Spec discrepancies

Found while planning. The plan does not edit `spec.md`; the owner's answers are recorded there.

1. **Maximum email length (resolved 2026-11-11).** FR-003 gave no length limit, but the email is stored in
   Identity's 256-character `UserName` column, and the unique index is on `NormalizedUserName`, which has the
   same length (research R-04). A longer email would have failed in the database as a fault. The owner
   decided that an email longer than 254 characters after trimming is not valid; the spec's
   `## Clarifications` and FR-003 now say so, and `sources` was confirmed against that spec
   (`ac_lock.py confirm-spec sources`, 2026-11-11). The locked acceptance criteria have no item for this clause,
   so it is covered by unit tests (254 accepted, 255 refused) and not by AC tests.
2. **Register's session.** HP-002 says the new user is signed in after registering, but Register has no "Keep
   me signed in". **Plan position:** a session cookie, as for a sign-in without "Keep me signed in". This is
   within the spec's words; noted so the host tests can name it.

## Risks

- **CI secret.** The CI job needs the repository secret `CI_SQL_SA_PASSWORD`, which only the owner can create.
  Until it exists, the job skips with a notice, like a run without `SONAR_TOKEN`, so the Sonar gate stays
  unanswered (skipped, not green) and the PR cannot pass its gates.
- **Identity defaults.** A default stricter than FR-002 or FR-003 would refuse valid input as a fault. The
  options of research R-03, the normalizer of R-04 and the integration tests named in ADR 0004 (apostrophe
  email, the spec's example passwords) cover it.
- **Renewal on every authenticated request.** The zero security-stamp interval re-issues the cookie on each
  request of a signed-in user, static assets included (research R-09): one lookup, a `Set-Cookie` and
  no-store headers per response, so a signed-in browser re-fetches the CSS and scripts. Accepted at demo
  scale; slice 3 can revisit it for hosting.
- **Database drop in `ResetAsync`.** It drops the database named in the connection string. Only tests call it,
  each with its own database (`HT-IT`, `HT-UI`, `HT-AC`); the host never calls it, and host tests run in the
  `Testing` environment against `HT-UI` (research R-16).

## Complexity Tracking

No Constitution Check violation needs justifying.
