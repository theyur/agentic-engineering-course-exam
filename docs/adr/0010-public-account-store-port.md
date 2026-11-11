# 0010 — Public account-store port, fenced by architecture rules

**Status:** Accepted, 2026-11-11. Decided by the owner while reviewing the tasks of slice 1,
`specs/001-accounts-and-access` ("Plan discrepancies" 1 in `tasks.md`). Supersedes the choice of research
R-13 (an internal port) for this and later slices.

## Context

- The plan of slice 1 (research R-13, `data-model.md` "Application port") makes `IAccountStore` internal to
  `HT.Application`. The services take it through internal constructors, and `InternalsVisibleTo` opens it to
  `HT.Persistence` and three test projects. The aim is constitution III: no Blazor component can reach the
  port's lookups, so the only Application members a component can call are the use cases, each scoped to the
  acting user.
- `HT.Domain`, `HT.Application` and `HT.Persistence` are the AC surface projects of `.workflow/config.json`
  (ADR 0003). The AC extension's reference export (`export_refs.py`) refuses an AC surface project that
  carries `InternalsVisibleTo`, in an attribute or anywhere in its source and project files. With the plan's
  internal port, `/speckit-ac-tests-write` could not run.

## Options

1. **Public port, fenced by architecture rules (chosen).**
2. **Internal port with `InternalsVisibleTo`, as research R-13 chose.** It fails the AC reference export, so
   the acceptance step definitions could not be written. Rejected.
3. **Change the speckit-ac package to accept `InternalsVisibleTo`.** The package is generic and shared with
   other repositories, and its rule keeps the isolated test writer limited to what the public surface shows.
   Rejected.

## Decision

- `IAccountStore` is `public`, and so are the constructors of `AccountService`, `OperatorService` and
  `DemoDataService`. `AddHtApplication` still wires them.
- `AccountStore`, `HtDbContext`, the design-time context factory and the migration stay `internal` to
  `HT.Persistence`. The two Persistence entry points of ADR 0003 and `HtDatabase.MigrateAsync` stay public.
- No `InternalsVisibleTo` in `HT.Domain`, `HT.Application` or `HT.Persistence`. Tests use only their public
  members, and read or change database rows with plain SQL over their own connection string, never through
  `HtDbContext`. `HT.Ui` is not an AC surface project and may grant `InternalsVisibleTo` to `HT.UiTests`.
- Architecture rules replace the visibility fence:
  - no type in the `HT.Ui` assembly depends on `IAccountStore` (T053). This covers the whole host, not only
    `HT.Ui.Components`, which answers R-13's reason for rejecting a public port ("the port's id-only lookups
    would still be reachable from non-component host code");
  - only `HT.Ui.Operator` depends on `OperatorService` and `DemoDataService` (T090, T119);
  - `AcSurfaceTests` (T026) checks what the export checks: no `InternalsVisibleToAttribute` on the three
    assemblies, and no `InternalsVisibleTo` text in their source or project files or the root
    `Directory.Build.*` files. The pre-commit run fails before the export would.
- How constitution III still holds with a public `IAccountStore.FindByIdAsync`:
  - the port is persistence plumbing that only `HT.Application` calls and `HT.Persistence` implements. It is
    not a use case, and no `HT.Ui` type may depend on it (T053);
  - every use case for signed-in users still takes the acting user and acts only on its account, and none
    takes an `AccountId` (T078's reflection test);
  - the plan's internal port had the same method, so no new lookup is added.

## Consequences

- The AC reference listings show `IAccountStore`. The isolated test writer is still limited by ADR 0003 to
  the use cases and the two Persistence entry points, and the judge sees its step definitions' results.
- Unit tests build the services with a hand-written fake store through the public constructors (ADR 0006).
- Integration and host tests cannot inspect `HtDbContext`. The host test guard that checks which database the
  host uses reaches the context through Identity's public user store instead (research R-16 as revised in
  `tasks.md`, "Plan discrepancies" 2).
- A later slice that adds a port follows the same pattern: public, fenced by a rule over the whole `HT.Ui`
  assembly.
- The statements of research R-13, R-16, `data-model.md`, `contracts/application-api.md` and `plan.md` that
  describe the internal port are superseded by this ADR; `tasks.md` lists them.
