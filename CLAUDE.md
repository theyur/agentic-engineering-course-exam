# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

Habit Tracker — personal habit/consistency tracker web app, built as an agentic-engineering course exam project.

## Source of truth

- @SPEC.md — what the app does (behavior, roles, rules, non-goals, open product questions).
- @ARCHITECTURE.md — how it is built (stack, layer boundaries, quality constraints).
- If a request conflicts with either file, stop and ask; don't silently diverge. Material architectural changes go in an ADR.
- SPEC §11 (open product questions) and ARCHITECTURE §9 (deferred technical choices) are **deliberate unknowns**. Do not pick an answer silently (timezones, grace-period edge cases, mocking/UI-test libraries, Azure SKUs, schema, etc.) — raise it during feature planning and record the decision in the feature's spec or an ADR.

## Stack

C# / .NET 10, Blazor Interactive Server, EF Core + SQL Server (only provider — no SQLite), ASP.NET Core Identity (local email/password), Azure hosting. No REST API in v1: Blazor calls Application use cases via DI.

## Build and test

- Build: `dotnet build HabitTracker.slnx`
- Test: `dotnet test HabitTracker.slnx`; single test: `dotnet test HabitTracker.slnx --filter "FullyQualifiedName~<TestName>"`
  - The integration and UI tests need SQL Server: `.\COLL1` with integrated security, or `HT_IT_CONNECTION` and `HT_UI_CONNECTION` pointing elsewhere (ADR 0006).
- Run UI: `dotnet run --project src/HT.Ui --launch-profile https`, then open `https://localhost:7194` (the session cookie is `Secure`). The operator values are User Secrets of `HT.Ui` (`Operator:AdministratorEmail`, `Operator:AdministratorPassword`, `Operator:DemoPassword`); never read or print their values.
- Changed `.claude/hooks/architecture-gate.ps1`? Run `pwsh tools/guard-oracle/oracle.ps1` — it must print `PASSED` (see `tools/guard-oracle/README.md`).
- `TreatWarningsAsErrors` is on — any warning fails the build; fix it, don't suppress it.
- `TargetFramework`, `Nullable`, `ImplicitUsings` live in root `Directory.Build.props`; don't repeat them in `.csproj` files.

## Layout and layer rules

- Solution file at repo root; production projects in `src/`, test projects in `tests/`.
- Four projects: `HT.Domain` → `HT.Application` → `HT.Persistence` / `HT.Ui`.
  - `HT.Domain` references none of the others.
  - `HT.Application` references only `HT.Domain`.
  - `HT.Persistence` implements contracts defined inward; no business rules or workflow decisions.
  - `HT.Ui` is the composition root; components must not touch persistence implementations directly.
- No business rules in Blazor components; no EF/SQL details in Domain or Application.
- Layer rules must be enforced by architecture tests, not just prose.
- Schema changes only via EF Core migrations.

## Security / privacy boundary

- Private goals, activity, notes, and personal reports are owner-only. **Admins must not be able to access them** — no admin bypass in queries, services, or UI.
- Community participation is opt-in (off by default); custom habits never appear in public stats.
- Use framework auth (Identity); no custom crypto, password storage, or session handling. No secrets in source control.

## Testing

A feature is not done without tests: unit (domain rules — targets, grace period, corrections, pauses, consistency, streaks), integration (workflows against real SQL Server), architecture (layer deps), selective UI where useful. Use xUnit for all tests and ArchUnitNET for architecture tests.

## C# code analysis

Two global CLI tools that complement each other: **Roslynator** (Roslyn analyzers, symbol listing, LOC) and **`jb inspectcode`** (ReSharper inspections). **Use `jb` for anything involving `.razor` files and for `HT.Ui` / `HT.UiTests`, because Roslynator can't see Razor components. Use Roslynator for fast (~6 s) analyzer runs and public-API listings over `HT.Domain`, `HT.Application` and `HT.Persistence`.** Both give semantic answers that grep can't. Neither says anything about whether behaviour is correct, so a clean run doesn't replace tests or review.

- Versions checked: Roslynator CLI 0.13.0 (Roslyn 4.14), ReSharper CLI 2026.2.1. Both are global dotnet tools, not part of the repo's tool manifest. Both load `HabitTracker.slnx`.
- **Never** pass `--remove` to `roslynator find-symbol`, which deletes declarations. **Never** run `jb cleanupcode`, which rewrites source files across the solution.
- **Never point either tool at `tests/HT.AcceptanceTests`.** Its output would expose the off-limits `StepDefinitions/` and `Support/` code (see "Acceptance tests" below). The project isn't in `HabitTracker.slnx`, so solution-level runs skip it. Keep it that way, and exclude it from greps too.
- **Roslynator needs `dotnet restore HabitTracker.slnx` first.** Fresh worktrees have no `obj/`. Measured 2026-11-14: 4,896 diagnostics unrestored vs 229 restored, and those 229 are almost all Razor phantoms (below). Re-restore after adding a package. `jb` doesn't need a restore because it builds the solution itself. Its cold and warm runs gave identical output here.
- Commands (these are the ones pre-approved in `.claude/settings.json`):
  - `roslynator analyze HabitTracker.slnx`: analyzer and compiler diagnostics. Scope it with `--projects <Name>`.
  - `roslynator list-symbols HabitTracker.slnx --projects HT.Application --visibility public --depth member`: the real public API surface. Pass `--projects`, because a solution-wide run crashes (below).
  - `roslynator loc HabitTracker.slnx` / `lloc`: size per project. It counts only `.cs`, so `HT.Ui` shows 394 lines while its `.razor` files hold about 3,200 more.
  - `jb inspectcode HabitTracker.slnx --stdout -f=Text -e=WARNING --caches-home=<scratchpad>/jbcache --no-updates`: about 40 s, and the cache is about 15 MB. Always point `--caches-home` outside the repo. The default severity, `SUGGESTION`, is unusable. `-e=ERROR` isn't a quiet mode here: its only output is the 3 false `xUnit1025` hits below. Scope with `--project=<Name>` or `--include=<glob>`.
- Baseline 2026-11-14: Roslynator reported 229 diagnostics, 15 of them not from the compiler. `jb -e=WARNING` reported 95. If a run shows far more than that, something changed.

### Rider MCP (`mcp__rider__*`) and worktrees

Rider's MCP tools work on whatever solution Rider has open, not on this session's folder. They need Rider running with its MCP server enabled, and every call takes a `rootFolder` that Rider matches against its open projects.

- Worktrees live inside the main checkout (`.claude/worktrees/...`). If only the main checkout is open in Rider, a call with a worktree path as `rootFolder` is matched to the parent. It "succeeds" but reads, and for refactoring tools edits, the **main checkout** on another branch.
- In a worktree session, use Rider MCP only after the user has opened that worktree folder (or its `HabitTracker.slnx`) in Rider. Always pass the worktree path as `rootFolder`.
- Before trusting the result, confirm that the worktree itself is listed among Rider's open projects. A tool error lists them as "Currently open projects".
- If the worktree isn't open in Rider, use the `roslynator` / `jb inspectcode` CLI instead. They read the files on disk.
- Never call Rider's editing tools (`rename_refactoring`, `apply_patch`, `reformat_file`, `safe_delete`, …) until that check passes.

### Known false positives: Roslynator (checked 2026-11-14)

Don't "fix" the code for any of these.

- **The Razor source generator doesn't run** under Roslynator's workspace, so `.razor` components don't exist as symbols. That causes about 210 `CS0246`/`CS0103`/`CS1503`/`CS1061`/… errors in `HT.UiTests` (bUnit tests), plus `Program.cs: CS0246 'App' could not be found`. Cascades from the same cause: `xUnit1025` on `SiteNavTests.cs:170-172` (the constants come from `SiteNav.razor`'s `@code`) and `CA1822` on `CatalogHabitDetailTests.RenderHabit`. `find-symbol --unused` is unreliable for anything a component calls. Confirm each hit with grep.
- The BCL generators (`[GeneratedRegex]`, `[LoggerMessage]`) do run. Their partial methods report nothing.
- `CA1861` in `src/HT.Persistence/Migrations/*` is in EF-generated migration code. Leave it.
- `list-symbols` without `--projects` crashes with `InvalidOperationException` in `SymbolDefinitionWriter.WriteAttribute`, triggered by `HT.UnitTests` and `HT.UiTests`. The production projects and the other test projects work.
- No C# 14 `field` keyword is used yet, so the older bundled Roslyn hasn't misreported a language feature. If one is reported later, cross-check with `jb`.

### Known false positives: `jb inspectcode` (checked 2026-11-14)

Don't "fix" the code for any of these.

- The `RazorSourceGenerator ... TypeLoadException` banner at the top of the output is harmless. `jb` still inspects `.razor` with its own engine.
- `xUnit1025` "InlineData duplicate(s)" on `SiteNavTests.cs:170-172` is the same Razor-constant blind spot as in Roslynator. The real build passes.
- "Path '…\wwwroot\catalog' is not found" (16 hits in `SiteNav`, `Footer`, `Home`, `Register`, `SignIn` `.razor`): these `href`s are page routes, not files.
- "Parameter 'h' is only used for precondition check(s)" (18 hits, all in tests): these are predicate lambdas passed to `Assert.Single` / `Assert.All` and similar.

### Where grep still wins

Neither tool covers `.css`, `.js`, `.json`, `.md`, `.feature` or `.ps1` files. For those, and for confirming an "unused" hit, use grep.

## Commits

- Subject line: Conventional Commits with one gitmoji right after the colon: `<type>(<scope>): <emoji> <subject>`, e.g. `feat(ac): ✨ add the acceptance-test workflow`. The type stays first so Conventional Commits tooling can still parse it. Use the emoji that matches the type:
  - `feat` ✨ · `fix` 🐛 · `refactor` ♻️ · `test` ✅ · `docs` 📝 · `chore` / config 🔧 · `ci` 👷 · `build` / dependencies 📦 · `perf` ⚡ · a security fix (`fix`) 🔒
  - The body stays plain text, without emojis.
- Commits run the architecture tests via `.githooks/pre-commit`. Enable it once per clone: `git config core.hooksPath .githooks`. Never use `--no-verify` / `-n`; fix the failing test.
- A Claude Code guard (`.claude/hooks/architecture-gate.ps1`) blocks commit commands it cannot fully check. Write them plainly: `git commit -m "..."` or `git commit -F - <<'EOF'` (quoted delimiter); no `sh -c` / `eval`, variables or `-c` options in git commands; use absolute paths inside the clone for `cd` / `git -C`.
- Hook scripts load from the main checkout (`$CLAUDE_PROJECT_DIR`): changes under `.claude/hooks/` take effect only once they're in the main checkout.

## Docs and git workflow

- `specs/NNN-<name>/` — Spec Kit feature folders (spec, plan, tasks) for features and bug fixes, created by `/speckit-specify`. `docs/tasks/` is legacy: don't add to it.
- `.specify/memory/constitution.md` — engineering principles and quality gates; change only via `/speckit-constitution`.
- `docs/adr/` — numbered ADRs: `0001-initial-setup.md`.
- `docs/roadmap.md` — slice order, status and open decisions; keep it current as its "Updating this file" section says.
- One branch per task; open PRs against `master`.

## PR review

- After opening or updating a PR, use `/process-pr-feedback` (SonarQube Cloud + CodeQL gates on the current head SHA, then manual Copilot review). Flow and gate semantics: README "PR review flow", `docs/adr/0001-pr-quality-gates.md`.
- **Greptile is not used here.** Never run the global `/pr-review`, never post `@greptileai`, never trigger a Greptile review (also blocked via `.claude/settings.json`).
- Never request Copilot (or any AI) review yourself and never merge — the user does both.
- Resolve review threads only via `tools/pr-gates/pr-resolve.ps1 -AcceptApproval -Apply`, after a Copilot re-review of the head and with a verified `Fixed in <sha>` reply. Never resolve by hand or because of a push alone (ADR 0002).
- Changed `tools/pr-gates/` or `.claude/hooks/block-pr-review.ps1`? Run `pwsh -NoProfile -File tools/pr-gates/selftest.ps1` — it must print `PASSED`.

<!-- >>> speckit-ac (managed; do not edit) -->
## Acceptance tests (spec-kit extension `ac`)

This project uses the isolated acceptance-test workflow of spec-kit-ac. These rules are for the main agent; the
AC subagents never load this file.

- Flow per feature: /speckit-specify -> /speckit-clarify -> /speckit-ac-translate (draft `acceptance.md`, user
  review, lock; then translate to `.feature` files, lock) -> /speckit-plan -> /speckit-tasks -> /speckit-implement
  -> /speckit-ac-tests-write -> /speckit-ac-tests-analyze. /speckit-ac-critic reviews on request.
- The /speckit-ac-* commands are started only by the user, who types them. Never start them yourself, and never
  start the AC subagents (ac-feature-translator, ac-test-writer, ac-test-fail-judge) except from those commands,
  with the exact task message their scripts print.
- After each step: report, then stop and wait for the user. Never start the next step on your own.
- Never read, search, list, diff or review the step definitions and AC support code: the `StepDefinitions/` and
  `Support/` folders of the AC project (`acProjectDir` in `.workflow/config.json`). This holds for every tool,
  including Bash, PowerShell, `git diff` / `git show` / `git log -p` and code-review tools. Use the test results
  and the judge report instead.
- Never edit `.feature` files, lock files (`specs/*/ac/*.lock`), run records (`specs/*/ac/runs/`), the reference
  assemblies (`refsDir`) or `.workflow/active-run.json`. Locked acceptance criteria change only after the user
  asks for an unlock.
- Every domain rule the acceptance criteria depend on belongs in the feature's `spec.md` or in the constitution,
  not only in this file.
- The extension's files are in `.specify/extensions/ac/` (see its README.md). Setup and checks: /speckit-ac-init;
  `speckit-ac doctor` in a terminal.
<!-- <<< speckit-ac -->
