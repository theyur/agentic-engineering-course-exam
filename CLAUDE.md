# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

Habit Tracker — personal habit/consistency tracker web app, built as an agentic-engineering course exam project.

## Source of truth

- @SPEC.md — what the app does (behavior, roles, rules, non-goals, open product questions).
- @ARCHITECTURE.md — how it is built (stack, layer boundaries, quality constraints).
- If a request conflicts with either file, stop and ask; don't silently diverge. Material architectural changes go in an ADR.
- SPEC §11 (open product questions) and ARCHITECTURE §9 (deferred technical choices) are **deliberate unknowns**. Do not pick an answer silently (timezones, grace-period edge cases, mocking/UI-test libraries, Azure SKUs, schema, etc.) — raise it during feature planning and record the decision in the task file or an ADR.

## Stack

C# / .NET 10, Blazor Interactive Server, EF Core + SQL Server (only provider — no SQLite), ASP.NET Core Identity (local email/password), Azure hosting. No REST API in v1: Blazor calls Application use cases via DI.

## Build and test

- Build: `dotnet build HabitTracker.slnx`
- Test: `dotnet test HabitTracker.slnx`; single test: `dotnet test HabitTracker.slnx --filter "FullyQualifiedName~<TestName>"`
- Run UI: `dotnet run --project src/HT.Ui`
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

- Commits run the architecture tests via `.githooks/pre-commit`. Enable it once per clone: `git config core.hooksPath .githooks`. Never use `--no-verify` / `-n`; fix the failing test.
- A Claude Code guard (`.claude/hooks/architecture-gate.ps1`) blocks commit commands it cannot fully check. Write them plainly: `git commit -m "..."` or `git commit -F - <<'EOF'` (quoted delimiter); no `sh -c` / `eval`, variables or `-c` options in git commands; use absolute paths inside the clone for `cd` / `git -C`.
- Hook scripts load from the main checkout (`$CLAUDE_PROJECT_DIR`): changes under `.claude/hooks/` take effect only once they're in the main checkout.

## Docs and git workflow

- `docs/tasks/` — task specs, date-prefixed: `2026-10-11-implement-auth.md`.
- `docs/adr/` — numbered ADRs: `0001-initial-setup.md`.
- One branch per task; open PRs against `master`.
