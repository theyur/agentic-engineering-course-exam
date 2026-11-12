# Habit Tracker

Exam project for the Agentic Engineering course: a browser-based **personal consistency tracker**, built to practice agentic software development and the SDLC with Claude Code. Learning the workflow comes first; product value is secondary, but the app must be coherent and demonstrable.

## What it does

Users define habits to **build** (e.g. yoga three times a week) or **reduce** (e.g. at most 3 cigarettes a day), record what actually happened, and see how consistent they are over time.

- **Habit catalog** — public, reusable habits (Reading, Yoga, …) visible to anonymous visitors, plus private custom habits that users can suggest for the catalog.
- **Personal goals** — yes/no, count, or duration targets on fixed schedules or flexible periodic targets; excusals, pauses, prospective goal changes, ending and restarting goals.
- **Recording** — partial progress is kept; a 48-hour grace period for late entries; corrections to historical records with recalculated reports; notes/journaling.
- **Reports** — strict streaks, consistency percentage (e.g. `90% (18 of 20)`), trends, and an in-app dashboard of due and overdue commitments.
- **Community (opt-in)** — Consistency Bands (90%+, 70%+, 50%+, below 50%; names listed alphabetically, no ranking) and aggregate catalog-habit performance with minimum-contributor thresholds.
- **Administration** — catalog curation, suggestion and feedback review, account deactivation. Admins have **no access** to users' private tracking data.

Full product behavior, non-goals, and open questions: [SPEC.md](SPEC.md).

## Technology

- C# / .NET 10
- Blazor Interactive Server (no separate REST API in v1)
- Entity Framework Core + Microsoft SQL Server
- ASP.NET Core Identity (local email/password)
- xUnit, ArchUnitNET for architecture tests
- Azure hosting for a low-cost public demo

## Architecture

Clean / Onion Architecture with four production projects:

```text
HT.Domain  ←  HT.Application  ←  HT.Persistence
                     ↑                  ↑
                     └──── HT.Ui ───────┘   (composition root)
```

| Project | Responsibility |
| --- | --- |
| `HT.Domain` | Business terminology, invariants, domain rules. No dependencies. |
| `HT.Application` | Use cases, workflow coordination, inward-facing abstractions. |
| `HT.Persistence` | EF Core / SQL Server implementation of persistence contracts. |
| `HT.Ui` | Blazor UI, authentication/authorization, application startup. |

Layer rules are enforced by the ArchUnitNET tests in `tests/HT.ArchitectureTests/`, which also run before every commit (`.githooks/pre-commit`). Details and rationale: [ARCHITECTURE.md](ARCHITECTURE.md).

## Repository layout

```text
HabitTracker.slnx        solution
Directory.Build.props    shared build settings (net10.0, nullable, warnings as errors)
src/                     production projects
tests/                   test projects
docs/adr/                architecture decision records (0001-initial-setup.md)
specs/                   Spec Kit feature folders: spec, plan, tasks (001-accounts-and-access/)
.specify/                Spec Kit templates, scripts and the project constitution
```

## Getting started

Prerequisites:

- [.NET 10 SDK](https://dotnet.microsoft.com/download), then `dotnet tool restore`.
- SQL Server reachable as `.\COLL1` with integrated security. On another server, set the environment
  variables `HT_IT_CONNECTION` (integration tests), `HT_UI_CONNECTION` (UI and host tests) and
  `HT_AC_CONNECTION` (acceptance tests); the app itself uses `ConnectionStrings:HabitTracker` from
  `appsettings.Development.json`.
- The operator values in the User Secrets store of `HT.Ui`, kept out of source control. Choose your own
  values; each password needs at least 8 characters, with a number:
  - `Operator:AdministratorEmail` and `Operator:AdministratorPassword`: the first administrator, created at
    start;
  - `Operator:DemoPassword`: the password of the demo accounts `demo1@example.com` to `demo3@example.com`,
    created at start.

  ```bash
  dotnet user-secrets set "Operator:DemoPassword" "<your value>" --project src/HT.Ui
  ```

```bash
dotnet build HabitTracker.slnx
```

```bash
dotnet test HabitTracker.slnx
```

```bash
dotnet run --project src/HT.Ui --launch-profile https
```

Open `https://localhost:7194`; the session cookie is `Secure`, so use HTTPS. In Development the app applies
pending schema migrations at start, then creates the administrator and the demo accounts; the console shows
the results, never a password. The full walk-through:
[specs/001-accounts-and-access/quickstart.md](specs/001-accounts-and-access/quickstart.md).

Warnings are treated as errors — the build must stay warning-free.

## Development workflow

Developed primarily through an agentic SDLC workflow with Claude Code (agent guidance in [CLAUDE.md](CLAUDE.md)):

1. Describe the feature or fix with [Spec Kit](https://github.com/github/spec-kit) (`/speckit-specify`, then `/speckit-clarify`, `/speckit-plan`, `/speckit-tasks`); artifacts land in `specs/NNN-<name>/`, checked against the [constitution](.specify/memory/constitution.md).
2. Resolve any relevant open questions from SPEC.md / ARCHITECTURE.md; record significant architectural decisions as ADRs in `docs/adr/`.
3. Implement on a dedicated branch, with unit, integration, and architecture tests as part of the feature.
4. Open a pull request against `master` and run `/process-pr-feedback` — see [PR review flow](#pr-review-flow).

## PR review flow

Two static analyzers run in parallel on every PR revision and on pushes to `master`:

| Gate | Source | Green means |
| --- | --- | --- |
| SonarQube Cloud | `.github/workflows/sonar.yml` → check `SonarCloud Code Analysis` | quality gate passed on the PR head (a run without `SONAR_TOKEN` or `CI_SQL_SA_PASSWORD` is *skipped*, not green) |
| CodeQL | GitHub code scanning default setup (C#, JS/TS, Actions) → check `CodeQL` | analysis finished and the PR adds no alert at or above the failure threshold |

The Sonar job also runs the integration and host tests against a SQL Server container. It needs two
repository secrets: `SONAR_TOKEN` and `CI_SQL_SA_PASSWORD` (a throwaway SA password for that container).

Then a human-controlled AI review:

1. Push the branch and open the PR.
2. In Claude Code run `/process-pr-feedback [PR number]`. It waits (bounded) for both gates on the **current head SHA**, reports the findings, fixes the justified ones and pushes (max two rounds per stage before it asks you).
3. When both gates are green it reports **Ready for manual Copilot review** and stops. Request the review yourself: `gh pr edit <N> --add-reviewer @copilot`, or *Reviewers → Copilot* in the PR page.
4. When Copilot has reviewed, run `/process-pr-feedback <N>` again; it triages Copilot's threads the same way and replies in each one (`Fixed in <sha>: …`, `Not changed: …`).
5. After you re-request Copilot on the fixed head, `/process-pr-feedback <N>` resolves the earlier threads with `tools/pr-gates/pr-resolve.ps1 -AcceptApproval -Apply`. A thread is resolved when its last reply is `Fixed in <sha>` with that commit in the head, and Copilot's review of the head either lists the finding as resolved or recommends approval without raising it again. Rejected, partial or disputed threads, re-raised findings and "Changes recommended" reviews stay open with a reason. See [ADR 0002](docs/adr/0002-copilot-thread-resolution.md).
6. Merge manually.

Gate status alone: `pwsh tools/pr-gates/pr-gates.ps1 -Pr <N>` (add `-Wait` to block until both gates settle). Greptile and the global `/pr-review` command are not used in this repository — see [ADR 0001](docs/adr/0001-pr-quality-gates.md).

## Status

Slice 1, accounts and access ([specs/001-accounts-and-access](specs/001-accounts-and-access/spec.md)), is implemented: registration, sign-in with a temporary block after failed attempts, sign-out, Profile with password change, the visitor and signed-in Home, the first administrator and the demo accounts at start. The next slices follow [docs/roadmap.md](docs/roadmap.md); features are implemented task by task.
