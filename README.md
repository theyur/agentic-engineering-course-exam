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

Layer rules will be enforced by architecture tests added with future feature tasks. Details and rationale: [ARCHITECTURE.md](ARCHITECTURE.md).
## Repository layout

```text
HabitTracker.slnx        solution
Directory.Build.props    shared build settings (net10.0, nullable, warnings as errors)
src/                     production projects
tests/                   test projects
docs/adr/                architecture decision records (0001-initial-setup.md)
docs/tasks/              feature / bug-fix task specs (2026-10-11-implement-auth.md)
```

## Getting started

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download). SQL Server will be required once persistence is implemented.

```bash
dotnet build HabitTracker.slnx
dotnet test HabitTracker.slnx
dotnet run --project src/HT.Ui
```

Warnings are treated as errors — the build must stay warning-free.

## Development workflow

Developed primarily through an agentic SDLC workflow with Claude Code (agent guidance in [CLAUDE.md](CLAUDE.md)):

1. Describe the feature or fix as a task spec in `docs/tasks/`.
2. Resolve any relevant open questions from SPEC.md / ARCHITECTURE.md; record significant architectural decisions as ADRs in `docs/adr/`.
3. Implement on a dedicated branch, with unit, integration, and architecture tests as part of the feature.
4. Open a pull request against `master`; PRs are reviewed by Greptile.

## Status

Foundation stage: specification, architecture, and an empty compiling solution. Features are implemented task by task.
