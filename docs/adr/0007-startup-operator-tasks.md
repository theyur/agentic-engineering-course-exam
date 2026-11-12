# 0007 — Startup operator tasks: administrator creation and demo loading

**Status:** Accepted, 2026-11-11. Settles the roadmap row "Re-runnable demo-data seeding" (ARCHITECTURE §7)
and records the spec's decision on how the first administrator comes to exist, in slice 1,
`specs/001-accounts-and-access`. Applies to the demo data of every later slice.

## Context

- An administrator must exist for slice 2 to curate the catalog, but registration never creates one (FR-004).
  The owner decided that the system creates the administrator at startup from secret configuration, with no
  UI. A changed configured email creates an additional administrator (spec `## Clarifications`, FR-014).
- SPEC §10 and constitution VI need representative, visibly synthetic demo data, extended by every user-facing
  slice. Slice 1 adds three demo accounts (FR-016).
- Both operations are Application use cases that acceptance tests call directly (ADR 0003, ADR 0005), and both
  can be re-run safely: an existing account is reported, not changed.
- Secrets never go into source control, logs or results (HP-017, HP-019, CLAUDE.md).
- How the hosted demo is deployed, how its secrets are supplied, how migrations roll out and how hosted demo
  data is refreshed are slice 3's decisions (`demo-hosting`).

## Options

1. **At every start, after administrator creation (chosen).** Opt-in through configuration: a deployment
   without a demo password gets no demo accounts.
2. **At start, in the Development environment only.** The hosted demo would need a mechanism of its own.
3. **A separate operator command, never at startup.** Explicit control, but one more step on every deployment,
   which slice 3 would have to script.

## Decision

- **When.** At every start, before the host serves requests, it runs:
  1. in the Development environment only, the pending EF Core migrations (HP-022), so a local database gets
     its schema without a manual step;
  2. administrator creation (FR-014), with the administrator email and password from configuration;
  3. demo loading (FR-016), with the demo password from configuration.
- **Where the values come from.** Configuration outside source control (HP-019). Locally that is the .NET
  User Secrets store of `HT.Ui`. For the hosted app, slice 3 decides. The configuration key names are chosen by
  the plan.
- **Results.** The host writes each result to the operator log: its text, and the email it is about, never a
  password (HP-017). Missing values give the results "no administrator configured" and "no demo password
  configured". Whatever the result, the host starts normally (HP-018, HP-021). An unexpected fault in either
  task, such as an unreachable database, is logged without secrets and does not stop the start either.
- **Schema.** Both tasks need the schema in place. Locally the Development step above provides it (owner
  answer, 2026-11-11, recorded in slice 1's spec). Outside Development the host applies no migrations; how
  they roll out to the hosted app is slice 3's decision (roadmap, "Schema migration rollout on each
  deployment").
- **Later slices.** Each later slice extends demo loading with its own demo data. The same rules apply:
  - re-runnable: existing data is reported and left unchanged, never duplicated;
  - visibly synthetic: no real people, emails only in `example.com`;
  - the same consent and privacy rules as real data (constitution VI).

  Demo data that must be relative to the current time (slice 5) may need a new or superseding ADR, together
  with slice 3's refresh decision.

## Consequences

- Every start does a few account lookups before serving requests.
- A forgotten or weak configured password never blocks the start. The operator log shows the reason instead.
- Several administrators can exist, and removing the role is out of scope for version 1.
- Anyone who knows a demo email can trigger its block or change its password. The spec accepts this for the
  local demo (Edge Cases); how reviewers sign in to the hosted demo is slice 3's decision (roadmap, "How
  reviewers get demo sign-ins").
- In Development, a database that a developer has changed by hand is migrated at start like any other.
