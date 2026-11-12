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
