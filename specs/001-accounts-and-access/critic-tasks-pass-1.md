# Critic pass 1: tasks for 001-accounts-and-access

**Verdict**: REVISE
**Date**: 2026-11-11
**Reviewed**: `specs/001-accounts-and-access/tasks.md` (T001–T124), against `spec.md`, `plan.md`, `research.md`,
`data-model.md`, `quickstart.md`, `contracts/application-api.md`, `contracts/host.md`, `acceptance.md` (coverage
context), the constitution, ADRs 0003–0009, `design/`, `docs/roadmap.md`, `README.md`, `CONTEXT.md`, and the
existing code, tests, hooks and CI workflow.

## Notes

### MAJOR-1: No design task changes `design/css/ht-components.css`, but the slice needs new styles, and T051 removes the template rules that hide the error bar and the reconnect states

- **Where**: tasks.md T009 (lines 85–88), T011 (line 90), T013 (line 92): each names only `.html` and `.md` files.
  T051 (lines 221–224), T053 (line 232), T088 (lines 453–459), T121 (lines 601–608).
- **Against**: constitution IV ("`HT.Ui` MUST use only the `css/tokens.css` variables and `ht-` classes … A needed
  visual change MUST be made in `design/` first"); `design/README.md` lines 12–13 and 19–20; plan.md Constitution
  Check row IV; `design/css/ht-components.css`; `src/HT.Ui/Components/Layout/MainLayout.razor.css` and
  `ReconnectModal.razor.css`.
- **Reasoning**: Three things this slice builds have no class in `design/css/ht-components.css` today:
  - **The wide-screen avatar menu (T011, T088).** The stylesheet has `.ht-avatar` as a plain 32px badge (line 41),
    `.ht-mobile-menu` as a full-width panel (line 301) and `.ht-menu` as an in-page side menu (line 189). It has no
    floating menu anchored to the avatar. T088 closes a menu "on a click outside (a backdrop element)", but the only
    backdrop class, `.ht-backdrop` (line 266), is the dialog overlay: it paints `var(--overlay)` and centres its
    child.
  - **The unexpected-error bar (T013, T053).** `#blazor-error-ui` is hidden only by
    `MainLayout.razor.css` lines 1–13 (`display: none`). T051 deletes that file. The design stylesheet has no rule
    for it (no match for `blazor` in the file).
  - **The reconnect notice (T013, T053).** Which text and button show in each reconnect state is decided only by
    `ReconnectModal.razor.css` lines 1–20. T051 deletes that file too, and the design stylesheet has no match for
    `reconnect`.

  After T051 the two copied files are the app's only stylesheets, so the rules for all three must exist in
  `design/css/ht-components.css` before the copy. No task lists that file (or `tokens.json`) as changed. Followed
  as written, the tasks lead to one of two results: the error bar and every reconnect text are visible on each
  page, or the missing rules are written in `src/HT.Ui` (inline or in the `wwwroot/css` copy), which is the
  divergence constitution IV forbids. T121 would not catch the second case: it searches `src/HT.Ui` only "outside
  `wwwroot/css/`", and no task compares `wwwroot/css` with `design/css`. T009, T011 and T013 are also marked [P]
  as "different files", which does not hold if each of them has to edit the one shared stylesheet.

### MINOR-1: Story independence and several checkpoints are stated but do not hold

- **Where**: tasks.md lines 626–627 ("each depends only on Phase 2"), 632–633 ("US1, US2, US3, US4, US7:
  independent of each other"), 662–663, checkpoints at lines 300, 365 and 694; T062, T088, T091, T104, T114, T115,
  T117.
- **Against**: spec.md US1-AS10 and HP-002 (after registering the user sees the signed-in Home), US2-AS13 and
  HP-006 (Sign out shows the visitor Home); tasks.md lines 634–637; `src/HT.Ui/Components/Pages/Home.razor`.
- **Reasoning**:
  - Line 626 says every story depends only on Phase 2, but lines 634–637 of the same section say US6 uses US3's use
    case and accessor and US2's Sign in page, and US5 needs US2's endpoint.
  - US7 is listed as independent of US4, but T114 places its step "right after administrator creation" (T085),
    T115 "extend[s] T086's rule", and T117 asserts lines "after the `Administrator creation:` line".
  - The Phase 3 checkpoint says "US1 passes on its own", yet US1-AS10 needs the signed-in Home, which T104 builds in
    Phase 8. Until T091 and T104, `/` is the template page ("Hello, world!").
  - The Phase 4 checkpoint ("A user can register, sign out and sign in again") and the MVP stop at line 694
    ("register, sign out and sign in in the running app") need a Sign out control. T071 adds only the endpoint; the
    first control that posts to it is in T088 (Phase 7).

  An implementer who stops at a checkpoint cannot observe what it claims, and the "side by side" plan at lines
  662–663 would start US7 before the US4 tasks it edits after.

### MINOR-2: The `StartupTasks` tests leave their configuration source and their starting database state open

- **Where**: tasks.md T045 (lines 201–205), T049 (lines 214–216), T085 (line 426), T087 (lines 431–437), T114
  (lines 568–572), T117 (lines 578–583); Conventions lines 44–46.
- **Against**: research.md R-11 (signature `RunAsync(IServiceProvider, IHostEnvironment, ILogger)`) and R-16
  (services "built from `AddHtApplication().AddHtPersistence(...)`"); spec.md FR-016 (fixed demo emails);
  ADR 0006 (databases `HT-IT` and `HT-UI`).
- **Reasoning**:
  - T085 and T114 read the operator values "from `IConfiguration`", but `RunAsync` receives no configuration, and
    the service set T049 builds contains none. T087 and T117 need "configured values" and "no values"; no task says
    how a test supplies them.
  - T049, T087 and T117 share one collection and one database. T049 needs a database with no applied migration
    ("a count above 0"), T087 needs the administrator email to be absent ("administrator created"), and T117 needs
    all three fixed demo emails to be absent ("created", then "already exists" on a second run). Only T049 says
    "dropped first", and it does not say whether that is per test or per class; xUnit v2 does not order classes
    inside a collection.
  - The Conventions name two test databases and say nothing else is touched. `HT-UI-MIGRATE` is a third one, and
    how its connection string is derived when `HT_UI_CONNECTION` is set (the CI case of T118) is not stated.
  - T046 creates `ListLoggerProvider.cs`; T049 uses a "`ListLogger`" with no file path.

### MINOR-3: `ChangePasswordAsync` for an acting user whose account no longer exists is neither built nor tested as the contract states

- **Where**: tasks.md T101 (line 511), T098 (lines 498–502).
- **Against**: contracts/application-api.md lines 46–47 ("`GetAccountDetailsAsync` and `ChangePasswordAsync` … An
  acting user whose account no longer exists gives `NotSignedIn`").
- **Reasoning**: T078 and T074 cover this clause for account details. For the password change, T101 lists the
  steps as `CheckPasswordAsync`, then `PasswordRule.IsMet`, then the store's `ChangePasswordAsync`, with no
  lookup of the account, and T098 has no such case. With those steps a missing account reaches
  `CheckPasswordAsync`, whose only results are true and false, so the outcome would be `CurrentPasswordWrong`,
  not `NotSignedIn`.

### MINOR-4: The "10 lost races" unit test can pass without reaching the limit, and the limit is tested on one side only

- **Where**: tasks.md T028 (line 137), T065 (lines 318–320).
- **Against**: research.md R-05 (step 5 and "After 10 lost races in a row the result is `Failed`");
  data-model.md `SignInBlock` transition table; constitution II ("both sides of each boundary").
- **Reasoning**: T028 defines `LoseNextSaves(n)` as changing the stored state "the way a parallel attempt would".
  If that is a parallel failed attempt, the count rises by one per lost race, so by the transition table the
  account is blocked after at most five of them, and R-05 step 5 then returns `Failed` because the re-read state
  is blocked. The test "10 lost races give `Failed`" would be green through that path and would not exercise the
  fail-closed limit. The tasks do not say which state changes keep the account unblocked for 10 races. The
  limit also has only the cases "one lost race" and "10 lost races"; the last retry that still succeeds is not
  listed.

### MINOR-5: The rule "only types in `HT.Ui.Operator` depend on `OperatorService`" is false for the architecture T024 loads

- **Where**: tasks.md T086 (line 427), T115 (line 573), T024 (line 124), T084 (line 425), T113 (line 567).
- **Against**: T027 (line 131: "DI cannot call the internal constructor", so registration is a factory lambda);
  research.md R-13.
- **Reasoning**: T024's shared `Architecture` holds all four production assemblies. T084 and T113 register
  `OperatorService` and `DemoDataService` in `ApplicationServiceCollectionExtensions`, which must construct them
  through their internal constructors, so a type in `HT.Application` depends on both. Read as written, T086 and
  T115 fail on that type. Whether the rule covers all loaded types or only types of `HT.Ui` is not stated, and
  the pre-commit hook runs these tests on every commit.

### MINOR-6: The Error page's "no request id, exception detail or configuration value" has no test

- **Where**: tasks.md T054 (line 233), T056 (line 238).
- **Against**: spec.md HP-017 ("Logs, error pages and the results … MUST NOT contain a password or other
  secret"); constitution V; `src/HT.Ui/Components/Pages/Error.razor` lines 9–25 (request id and Development-mode
  text today).
- **Reasoning**: T054 states the property, and T056 tests only the 404 path. T122 checks tracked files and log
  templates, not rendered pages. So the error-page part of HP-017 rests on the implementation task alone, while
  the log part has tests (T087, T117).

### MINOR-7: The new CI secret is not carried into the documents that describe the gate, and the owner's prerequisite is not surfaced

- **Where**: tasks.md T118 (lines 591–595), T119 (lines 596–599).
- **Against**: plan.md Risks, "CI secret" ("only the owner can create … the PR cannot pass its gates");
  `README.md` line 84 ("a run without `SONAR_TOKEN` is *skipped*, not green");
  `.github/workflows/sonar.yml` lines 3–10 (setup notes and "The job skips cleanly while SONAR_TOKEN is not set").
- **Reasoning**: After T118 the job also skips without `CI_SQL_SA_PASSWORD`. T119's README list (prerequisites,
  getting started, layer-rules and status sentences) does not include the PR-review-flow sentence, and T118 does
  not mention the workflow's own setup comment, so both stay untrue. No task or note tells the owner that the
  secret must exist before the PR can pass, although the plan records that only the owner can create it.

### MINOR-8: Some [P] markers contradict the definition in the same file

- **Where**: tasks.md lines 27–28 (definition), T035 (line 152), T055 (line 234), T056 (line 238), "Parallel
  opportunities" lines 658 and 660.
- **Against**: the definition: "different files, no dependency on an unfinished task".
- **Reasoning**: T035 is [P] and says "(depends on T031)"; line 658 lists T031 and T035 as parallel, but
  `HtUserClaimsPrincipalFactory` derives from a type over `HtUser`, which T031 creates. Line 660 lists T052, T055
  and T056 as parallel, but T055 tests the components T052 creates, and T056 asserts the NotFound content that
  T054 builds.

### MINOR-9: Two tests depend on access that no task provides

- **Where**: tasks.md T033 (lines 146–150), T048 (line 213), T111 (line 560), T105 (line 523), T108 (line 532),
  T003 (line 56).
- **Against**: research.md R-12 ("three public static methods in `HT.Persistence`") and R-16 ("A unit test in
  `HT.UiTests` drives the revalidating authentication-state provider"); ADR 0006 ("no interface exists only for
  tests").
- **Reasoning**:
  - T048 resolves `HtDbContext` in `HT.UiTests`, and T111 runs `ExecuteDeleteAsync` on `AspNetUsers` in
    `HT.IntegrationTests`. T033 does not state the context's accessibility, T036 makes the store `internal`, and
    T003 grants `InternalsVisibleTo` only for `HT.Application`.
  - The framework's `ValidateAuthenticationStateAsync` is `protected`, and `HT.Ui` grants no internals access,
    so T108 cannot call T105's check directly. T108 also does not say which user store or database backs
    `UserManager` in the test ("a deleted account" needs one), nor which collection it joins. If it uses `HT-UI`
    outside the host collection, it runs in parallel with the fixture that drops that database.

### MINOR-10: "`Microsoft.Extensions.TimeProvider.Testing` (10.0.x)" does not match that package's version line

- **Where**: tasks.md T005 (line 58); T040 (line 174) and T046 (line 207) give no version.
- **Against**: `tests/HT.AcceptanceTests/HT.AcceptanceTests.csproj` (`Version="10.11.0"`); research.md R-01
  ("the current 10.0.x patch of every Microsoft package").
- **Reasoning**: The AC project already pins this package at 10.11.0, so its releases do not follow the 10.0.x
  patch line the task names. Taken literally, T005 pins an older release than the AC project uses for the same
  fake clock, and the three test projects that add the package can end up on different versions.

## Summary

1 MAJOR, 10 MINOR.

Coverage of the FR, HP, SC items, the user stories and the edge cases is close to complete, and task identifiers,
requirement references, file paths and the 63-scenario count check out against the sources. The blocking gap is in
the design-first phase: the tasks never touch the design stylesheet, although the avatar menu, the error bar and
the reconnect notice need rules there and T051 deletes the template files that hold the current ones. The minor
notes are mostly places where a test's inputs, starting state or access are left open, plus dependency and
checkpoint statements that contradict the tasks themselves. The tasks are not ready for `/speckit-implement`
until the MAJOR note is resolved.
