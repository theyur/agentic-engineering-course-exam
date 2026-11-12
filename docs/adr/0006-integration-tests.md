# 0006 — Integration and UI tests: own projects and databases, run locally and in CI

**Status:** Accepted, 2026-11-11. Settles the integration-test database, test-project-layout, UI-test library
and mocking library rows of the roadmap (ARCHITECTURE §6, §9) in slice 1, `specs/001-accounts-and-access`.
ADR 0003 settled the acceptance-test database and left this one open.

## Context

ARCHITECTURE §6 requires integration tests of application workflows against real SQL Server; SQLite is ruled
out (§5). Slice 1 is the first slice that stores data. It needs, among others, HP-020 (two registrations with
the same email at almost the same time give exactly one account) and the mapping between the domain account
and Identity's user records (ADR 0004).

Today the solution `HabitTracker.slnx` holds `tests/HT.UnitTests` and `tests/HT.ArchitectureTests`;
`tests/HT.AcceptanceTests` is outside it (ADR 0003). CI (`.github/workflows/sonar.yml`) runs
`dotnet test HabitTracker.slnx` on an Ubuntu runner without SQL Server, under SonarQube Cloud coverage.

## Options

1. **Own project, run locally and in CI (chosen).** The local SQL Server instance locally, a SQL Server service
   container in CI.
2. **Own project, local only.** CI filters the tests out by a trait. Simpler CI, but persistence behaviour is
   never checked on a PR, and Sonar coverage misses it.
3. **Testcontainers everywhere.** A SQL Server container per run, locally and in CI. Identical environments,
   but every developer machine needs Docker, and runs are slower.

For selective UI tests: bUnit with host tests (chosen); Playwright end-to-end tests against a running host
(closest to what reviewers see, but browsers must be installed locally and in CI, and runs are slow); or both.
For stand-ins: hand-written fakes (chosen), or a mocking library such as NSubstitute or Moq, which would need
interfaces or virtual methods on the Application services.

## Decision

- **Project.** `tests/HT.IntegrationTests`, xUnit, in `HabitTracker.slnx`. It references `HT.Application`,
  `HT.Domain` and `HT.Persistence`, and builds the application through their composition entry points
  (ADR 0003, ADR 0005), without `HT.Ui`.
- **Local database.** SQL Server database `HT-IT` on `.\COLL1`. The connection string comes from the
  environment variable `HT_IT_CONNECTION`. When it is not set, the tests use
  `Server=.\COLL1;Database=HT-IT;Integrated Security=True;TrustServerCertificate=True`, so no secret is
  involved locally. Integration tests never use `HT` (the application's development database) or `HT-AC`.
- **CI database.** The test job gets a SQL Server service container. Its password is a throwaway value held as
  a GitHub Actions secret, and the job sets `HT_IT_CONNECTION` from it. The value is never printed.
- **Fresh schema per run.** At the start of a run the schema is dropped and rebuilt with the EF Core
  migrations, through the `HT.Persistence` reset entry point (ADR 0003). So every run also checks that the
  migrations apply.
- **One test at a time.** Integration tests run sequentially, because they share one database. Each test
  arranges its own data, with values of its own (for example its own emails), instead of resetting between
  tests.
- **Selective UI tests.** `tests/HT.UiTests`, xUnit, in the solution (owner answer, 2026-11-11, recorded in
  slice 1's spec):
  - bUnit for components: forms and their messages, the navigation variants, the narrow-screen menu;
  - host tests with `WebApplicationFactory` (`Microsoft.AspNetCore.Mvc.Testing`) for what needs the running
    host: the startup tasks and their operator log, redirects, cookies and session revalidation;
  - host tests use database `HT-UI`, with the connection string from `HT_UI_CONNECTION` and the same local
    default and CI service container as the integration tests. A separate database is needed because test
    projects may run at the same time, and each rebuilds its schema at the start of its run;
  - no browser automation in slice 1. A later slice that needs it decides with a new ADR.
- **No mocking library.** Tests use hand-written fakes of the inward-defined ports (for example the account
  store of ADR 0004) and a fake `TimeProvider`. Pages in bUnit tests get the real Application services with
  those fakes, so no interface exists only for tests.
- **Test-project layout.**
  - `tests/HT.UnitTests`: Domain and Application rules, without a database.
  - `tests/HT.ArchitectureTests`: layer dependency rules (ArchUnitNET).
  - `tests/HT.IntegrationTests`: workflows against real SQL Server.
  - `tests/HT.UiTests`: components and the running host.
  - `tests/HT.AcceptanceTests`: outside the solution (ADR 0003).

## Consequences

- `.github/workflows/sonar.yml` gets the SQL Server service container, `HT_IT_CONNECTION` and
  `HT_UI_CONNECTION`. CI runs take longer by the container start, the integration tests and the host tests.
- Slice 1's host and page requirements (HP-NNN) have a place to be tested from the start.
- The pre-commit hook still runs only the architecture tests, so committing needs no database.
- Running the whole solution locally needs `.\COLL1`, or `HT_IT_CONNECTION` pointing to another SQL Server.
- Concurrency tests such as HP-020 run against the real unique index, not against a mocked store.
