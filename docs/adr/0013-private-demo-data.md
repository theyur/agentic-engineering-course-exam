# 0013 — Private demo data only for demo accounts created in the same run

**Status:** Accepted, 2026-11-12. Owner choice in slice 2, `specs/002-habit-catalog` (critic revision, FR-031),
after critic passes 1 and 2 of its spec. Extends the "Later slices" rule of
[ADR 0007](0007-startup-operator-tasks.md) for private demo data. Applies to every later slice that adds
private demo data (goals, records, notes, pauses).

## Context

- ADR 0007: demo loading runs at every start, creates the demo accounts `demo1@example.com` to
  `demo3@example.com`, and later slices extend it with their demo data; existing data is reported and left
  unchanged.
- Slice 2 is the first slice with private data: custom habits. Constitution III requires every use case that
  reads or changes private data to take the acting user and act only on that owner's data.
- ADR 0005: operator capabilities, demo loading among them, take no acting user. CONTEXT.md: an acting user is
  established only by signing in. Demo loading cannot sign in as a demo account, because the account's password
  may differ from the demo password (slice 1, US7-AS2).
- A demo email can belong to a real person who registered it before demo loading ran (slice 1, FR-016 edge
  case), or to an administrator.

## Options

1. **Write private demo data only for accounts demo loading creates in the same run (chosen).**
2. Act as each demo account through the owner-scoped use cases and add demo data to any demo account that has
   none. Needs an acting user without a sign-in, contradicting ADR 0005 and CONTEXT.md, and writes into
   accounts that real people may own. Rejected.
3. Add demo data to every existing demo account that lacks it. Same problems as 2, and deleted demo data comes
   back at every start. Rejected.

## Decision

- Demo loading creates a demo account's private demo data only in the run in which it creates that account
  (account result "created"), together with the account, as one unit.
- For an account that already exists, whoever owns it and whatever its role, demo loading reads and changes no
  private data. Deleted demo data is therefore never created again.
- Demo loading stays an operator capability without an acting user. Its private-data writes go only to the
  account it has just created. It has no read of private data. Only the host's operator code may call it (the
  architecture rule of slice 1 stays).
- Results and log lines name the demo email and a result text, never private content (constitution III).

## Consequences

- Constitution III holds: no use case reads private data without its owner, and the only private-data writes
  without an acting user are to accounts nobody else can own yet.
- A database whose demo accounts already exist (for example a local database from slice 1) gets no new private
  demo data from later slices until those accounts are removed or the database is recreated. The hosted demo
  starts from an empty database (slice 3), so it gets every slice's demo data.
- Slices whose demo data must be relative to the current time (slice 5) still need ADR 0007's planned
  follow-up decision.
