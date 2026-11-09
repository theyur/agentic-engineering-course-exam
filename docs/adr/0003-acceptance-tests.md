# 0003 — Acceptance tests: AC surface and test database

**Status:** Accepted, 2026-11-09. Settles the acceptance-test part of ARCHITECTURE §9 ("libraries for
integration-test databases"). Integration tests stay open.

## Context

Features are specified with Spec Kit (`specs/NNN-<name>/`). Since this ADR, each feature can also carry
acceptance criteria that become acceptance tests (AC tests), through the spec-kit-ac extension
(`/speckit-ac-*` commands).

- Isolated subagents write the AC tests from the feature spec, its acceptance criteria and this ADR. They see
  only the public API of the AC surface (reference assemblies), never the implementation or the unit tests.
- The AC test project is `tests/HT.AcceptanceTests`. It is not part of `HabitTracker.slnx`; the main agent
  builds and runs it in `/speckit-ac-tests-analyze`.

AC tests check behavior as the spec describes it. Much of Habit Tracker's behavior depends on stored data:
ownership, the admin boundary, corrections, streaks over time. So the tests must run the real use cases
against real persistence. SQLite is ruled out (ARCHITECTURE §5).

## Decision

- **AC surface:**
  - `HT.Application`: its use cases are what the AC tests call.
  - `HT.Persistence`: only its public composition entry point and its public reset entry point (below).
  - `HT.Domain`: its types, as the use cases take and return them.
  - `HT.Ui` is a host and is not tested by AC tests.
- **Test database:** SQL Server database `HT-AC` on the local instance `.\COLL1`. AC tests never use `HT`, the
  application's own development database.
- **Fresh schema per run:** at the start of every AC test run, the schema of `HT-AC` is dropped and rebuilt
  with the EF Core migrations, so every run starts empty.
- **One run at a time:** AC tests run sequentially (no parallel test collections), because they share one
  database. Each scenario arranges the data it needs.
- **Connection:** the AC tests read the connection string from the environment variable `HT_AC_CONNECTION`.
  When it is not set, they use
  `Server=.\COLL1;Database=HT-AC;Integrated Security=True;TrustServerCertificate=True`. Integrated security,
  so no secret is involved.
- **Through the use cases only:** step definitions arrange and assert only through Application use cases and
  the two `HT.Persistence` entry points. They never use the EF Core `DbContext` or SQL directly.
- **Local only:** AC tests run on the developer machine, not in CI.

## Consequences

- The first feature that persists data must add two public entry points to `HT.Persistence`:
  - a **composition** entry point that registers persistence for a given connection string, so that a test can
    build the application without `HT.Ui`;
  - a **reset** entry point that drops and rebuilds the schema with the migrations.

  Until then, AC tests can cover only behavior that needs no storage.
- Packages that a feature adds to the AC surface are added to the AC test project by the main agent when the
  test writer reports them as missing.
- A run needs SQL Server reachable as configured. On another machine, set `HT_AC_CONNECTION`.
- `.github/workflows/sonar.yml` excludes `tests/HT.AcceptanceTests/**`. Its files are written by the isolated
  subagents and the main agent may not edit them, so a Sonar issue there could not be fixed in the PR flow
  (ADR 0001). CI does not build the project.
- Integration tests (ARCHITECTURE §6) still decide their own database approach.
