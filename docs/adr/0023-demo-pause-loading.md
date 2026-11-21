# 0023 — Demo pause loading: a fifth startup task for Excused and Paused demo data

**Status:** Accepted, 2026-11-21. Owner choice in slice 7, `specs/008-excusals-and-pauses` (`/speckit-clarify`,
spec D-4). Amends the "Later slices" rule and the list of startup tasks of
[ADR 0021](0021-demo-report-loading.md). Keeps [ADR 0013](0013-private-demo-data.md) (private demo data only with
the account), [ADR 0018](0018-evaluation-on-read-and-demo-history.md)'s run date and daily refresh, and
[ADR 0015](0015-demo-hosting.md) otherwise as they are.

## Context

- Constitution VI: every user-facing slice extends the demo data so that a reviewer sees its states without
  waiting for real time to pass. Slice 7 adds Excused commitments and Paused commitments and periods, which are
  left out of consistency and streaks (spec FR-001 to FR-019).
- The demo accounts with history are fixed by locked acceptance criteria: `demo4@example.com` by F005 (demo history
  loading's goals, records and results) and `demo5@example.com` by F007 (demo report loading's goals, records and
  figures). An excusal or a pause on either changes their locked figures.
- Demo loading's goals of `demo1` to `demo3` start on the run date and its results are locked by F001 and F004, so
  it can show neither past excusals nor past pauses.
- ADR 0021 asks a later slice that needs another demo account to weigh extending demo report loading against a
  fifth task.

## Options

1. **A fifth startup task, demo pause loading (chosen).** It runs after demo report loading and creates
   `demo6@example.com` with goals that start 28 days before the run date and their records, notes, excusals and
   pauses, by the same rules as demo report loading. No locked criteria change.
2. **Unlock F007 and extend demo report loading.** One task fewer, but slice 6's demo criteria and `demo5`'s figures
   would be unlocked, re-translated, re-locked and re-run, for a shipped slice whose behaviour does not change.
   Rejected.
3. **Only a current pause on `demo1` through demo loading.** No past Excused or Paused results, so constitution VI
   is not met, and F001 or F004 would still change. Rejected.

## Decision

- **Startup tasks.** At every start, before the host serves requests, the host runs (ADR 0021's list, extended):
  1. in Development only, the pending migrations;
  2. administrator creation;
  3. demo loading;
  4. demo history loading (ADR 0018);
  5. demo report loading (ADR 0021);
  6. demo pause loading (this ADR).
- **Demo pause loading.** An operator capability with the same contract as demo report loading: it takes the demo
  password (the same results for a missing or weak one, creating nothing), takes no acting user, reads no private
  data, and leaves an account that already exists unchanged ("already exists", no goal or record result). With a
  valid password it creates `demo6@example.com` and, as one unit with the account (ADR 0013), its demo goals,
  records, notes, excusals and pauses, relative to the run date R of ADR 0018. Excusals and pauses are stored
  directly as facts with their dates and reasons (spec FR-025); the rules that limit when a user can excuse or pause
  (spec FR-002, FR-006) apply to users, not to this task. Its results name the demo email and a result text only, never a reason. The
  exact goals and records are the spec's (slice 7 FR-031).
- **Results and checks.** The host logs its results as demo report loading's are logged. A fault in it is logged
  without secrets and does not stop the start (ADR 0007). The deployment's success check (ADR 0015) also requires
  its result, and the smoke check signs in as `demo6@example.com` (slice 7 HP-010, HP-011).
- **Later slices (amended rule).** Each later slice extends demo loading, demo history loading, demo report loading
  or demo pause loading with its demo data, or adds a further task by a new ADR when locked criteria would otherwise
  change. The rules stay: re-runnable, visibly synthetic (`example.com` only), the same consent and privacy rules
  as real data, and private demo data only together with its account.

## Consequences

- F001, F004, F005 and F007 stay locked and unchanged; slice 7's own criteria cover demo pause loading.
- The hosted demo shows Excused commitments, a past and a current pause and the Paused tab for `demo6`, relative to
  each day, because the daily scheduled refresh (ADR 0018) re-creates it with the rest of the demo data.
- One more startup task, account lookup and set of startup-log lines; the host's startup and smoke-check tests that
  compare "Demo " lines, check names and coverage counts exactly change to expect `demo6`'s lines after `demo5`'s.
- One more sign-in for reviewers, delivered with the exam submission like the others (ADR 0015); the demo password
  is shared by all demo accounts, so no new secret.
- A local database keeps `demo6`'s history relative to the day it was first created, as for `demo4` and `demo5`.
- The number of demo tasks keeps growing with each locked set of demo criteria. A later slice that needs another
  demo account weighs extending demo pause loading (whose criteria slice 7 locks) against a further task.
