# 0021 — Demo report loading: a fourth startup task for longer demo history

**Status:** Accepted, 2026-11-20. Owner choice in slice 6, `specs/007-personal-reports` (`/speckit-clarify`,
spec D-5). Amends the "Later slices" rule of [ADR 0007](0007-startup-operator-tasks.md) as
[ADR 0018](0018-evaluation-on-read-and-demo-history.md) restated it, and ADR 0007's list of startup tasks. Keeps
[ADR 0013](0013-private-demo-data.md) (private demo data only with the account), ADR 0018's run date and daily
refresh, and [ADR 0015](0015-demo-hosting.md) otherwise as they are.

## Context

- Constitution VI: every user-facing slice extends the demo data so that a reviewer sees its states without
  waiting for real time to pass. Slice 6 adds consistency, current and longest streaks and trends of up to 14
  commitments or days and 12 weeks per goal (spec FR-001 to FR-006).
- The only demo account with history, `demo4@example.com` (slice 5, ADR 0018), has goals that start 28 days before
  the run. It shows every slice-6 state, but its per-week goal has 3 or 4 weekly bars of 12, and its longest
  streaks are bounded by 4 weeks.
- ADR 0018's rule: "each later slice extends demo loading or demo history loading with its demo data, by the same
  rules". Neither can take a new account without changing locked acceptance criteria:
  - demo loading's results name `demo1@example.com` to `demo3@example.com` "and no other email" (F001 AC-057 to
    AC-060), which is why ADR 0018 added demo history loading in the first place;
  - demo history loading's results are "created", "demo goals added" and "demo records added", "each for
    `demo4@example.com`, and no other result" (F005 AC-051), and "every result names `demo4@example.com`" (F005
    AC-059).
- ADR 0013: private demo data (goals, records, notes) is written only together with the account it belongs to;
  an account that already exists is left unchanged.

## Options

1. **A fourth startup task, demo report loading (chosen).** It runs after demo history loading and creates
   `demo5@example.com` with goals that start 84 days before the run date and their records and notes, by the same
   rules as demo history loading. No locked criteria change.
2. **Unlock F005 and extend demo history loading.** One task fewer, but slice 5's demo criteria (AC-051, AC-059
   and any that list its exact results) would be unlocked, re-translated, re-locked and re-run, for a shipped
   slice whose behaviour does not change. Rejected.
3. **Lengthen `demo4`'s history.** Its goals' start dates and records are fixed by F005 (AC-051 to AC-057), so
   this is option 2 with more criteria to unlock. Rejected.
4. **No new demo data.** Short trends and streaks; constitution VI is met only weakly for the trend. Rejected by the
   owner.

## Decision

- **Startup tasks.** At every start, before the host serves requests, the host runs (ADR 0007's list, extended):
  1. in Development only, the pending migrations;
  2. administrator creation;
  3. demo loading;
  4. demo history loading (ADR 0018);
  5. demo report loading (this ADR).
- **Demo report loading.** An operator capability with the same contract as demo history loading: it takes the
  demo password (the same results for a missing or weak one, creating nothing), takes no acting user, reads no
  private data, and leaves an account that already exists unchanged ("already exists", no goal or record
  result). With a valid password it creates `demo5@example.com` and, as one unit with the account (ADR 0013), its
  demo goals, records and notes, relative to the run date R of ADR 0018 (the date at the run time in the
  installation's default time zone). Its results name the demo email and a result text only. The exact goals and
  records are the spec's (slice 6 FR-032).
- **Results and checks.** The host logs its results as demo history loading's are logged (one account line, then
  "Demo goals …" and "Demo records …" for a created account). A fault in it is logged without secrets and does not
  stop the start (ADR 0007). The deployment's success check (ADR 0015) also requires its result, and the smoke
  check signs in as `demo5@example.com` (slice 6 HP-008, HP-011).
- **Later slices (amended rule).** Each later slice extends demo loading, demo history loading or demo report
  loading with its demo data, or adds a further task by a new ADR when locked criteria would otherwise change. The
  rules stay: re-runnable, visibly synthetic (`example.com` only), the same consent and privacy rules as real
  data, and private demo data only together with its account.

## Consequences

- F001, F004 and F005 stay locked and unchanged; slice 6's own criteria cover demo report loading.
- The hosted demo shows trends with all their bars and streaks of several weeks for `demo5`, relative to each day,
  because the daily scheduled refresh (ADR 0018) re-creates it with the rest of the demo data.
- One more startup task, a few more account lookups at every start, and more lines in the startup log; the host's
  startup tests that compare the "Demo " lines exactly change to expect `demo5`'s lines after `demo4`'s.
- One more sign-in for reviewers, delivered with the exam submission like the others (ADR 0015); the demo password
  is shared by all demo accounts, so no new secret.
- A local database keeps `demo5`'s history relative to the day it was first created, as it does for `demo4`.
- The number of demo tasks grows with each locked set of demo criteria; a later slice that needs another demo
  account weighs extending demo report loading (whose criteria slice 6 locks) against a fifth task.
