# 0026 — Demo goal-change loading: a sixth startup task for goal-change demo data

**Status:** Accepted, 2026-11-22. Owner choice in slice 8, `specs/009-goal-changes` (`/speckit-clarify`, spec
D-3). Amends the list of startup tasks and the "Later slices" rule of
[ADR 0023](0023-demo-pause-loading.md). Keeps [ADR 0013](0013-private-demo-data.md) (private demo data only with
the account), [ADR 0018](0018-evaluation-on-read-and-demo-history.md)'s run date and daily refresh, and
[ADR 0015](0015-demo-hosting.md) otherwise as they are.

## Context

- Constitution VI: every user-facing slice extends the demo data so that a reviewer sees its states without
  waiting for real time to pass. Slice 8 adds settings changes, ended goals with an outcome, new attempts with
  earlier attempts, and the Profile's goal history (spec FR-001 to FR-021).
- Every demo account is fixed by locked acceptance criteria: `demo1` to `demo3` by F001 and F004, `demo4` by F005,
  `demo5` by F007, and `demo6` by F008. A settings change, an ending or a second attempt on any of them changes
  locked figures.
- Demo loading's goals start on the run date, so it cannot show a change that has already taken effect or an
  attempt that ended in the past.
- ADR 0023 asks a later slice that needs another demo account to weigh extending demo pause loading against a
  further task.

## Options

1. **A sixth startup task, demo goal-change loading (chosen).** It runs after demo pause loading and creates
   `demo7@example.com` with three goals relative to the run date: one whose target changed, one on its second
   attempt with an ended first attempt, and one ended. No locked criteria change.
2. **Unlock F008 and extend demo pause loading.** One task fewer, but slice 7's demo criteria and `demo6`'s figures
   would be unlocked, re-translated, re-locked and re-run, for a shipped slice whose behaviour does not change.
   `demo6`'s dashboard figures would also change. Rejected.
3. **Only end one goal of `demo1` through demo loading.** No changed target and no earlier attempt, so
   constitution VI is not met, and F001 or F004 would still change. Rejected.

## Decision

- **Startup tasks.** At every start, before the host serves requests, the host runs (ADR 0023's list, extended):
  1. in Development only, the pending migrations;
  2. administrator creation;
  3. demo loading;
  4. demo history loading (ADR 0018);
  5. demo report loading (ADR 0021);
  6. demo pause loading (ADR 0023);
  7. demo goal-change loading (this ADR).
- **Demo goal-change loading.** An operator capability with the same contract as demo pause loading: it takes the
  demo password (the same results for a missing or weak one, creating nothing), takes no acting user, reads no
  private data, and leaves an account that already exists unchanged ("already exists", no goal or record result).
  With a valid password it creates `demo7@example.com` and, as one unit with the account (ADR 0013), its demo
  goals, settings versions, attempts, endings, records and notes, relative to the run date R of ADR 0018. Settings
  versions, attempts and endings are stored directly as facts (spec FR-026). The rules that limit when a user can
  change, end or restart a goal (spec FR-002, FR-007, FR-012) apply to users, not to this task. Its results name
  the demo email and a result text only, never a goal, habit, note, outcome, unit or amount. The exact goals and
  records are the spec's (slice 8 FR-031).
- **Results and checks.** The host logs its results as demo pause loading's are logged. A fault in it is logged
  without secrets and does not stop the start (ADR 0007). The deployment's success check (ADR 0015) also requires
  its result, and the smoke check signs in as `demo7@example.com` (slice 8 HP-011, HP-012).
- **Later slices (amended rule).** Each later slice extends one of the demo tasks with its demo data, or adds a
  further task by a new ADR when locked criteria would otherwise change. The rules stay: re-runnable, visibly
  synthetic (`example.com` only), the same consent and privacy rules as real data, and private demo data only
  together with its account.

## Consequences

- F001, F004, F005, F007 and F008 stay locked and unchanged. Slice 8's own criteria cover demo goal-change loading.
- The hosted demo shows a changed target, an ended goal on the Ended tab, a goal on attempt 2 and a filled goal
  history for `demo7`, relative to each day, because the daily scheduled refresh (ADR 0018) re-creates it with the
  rest of the demo data.
- One more startup task, account lookup and set of startup-log lines. The host's startup and smoke-check tests that
  compare "Demo " lines, check names and coverage counts exactly change to expect `demo7`'s lines after `demo6`'s.
- One more sign-in for reviewers, delivered with the exam submission like the others (ADR 0015). The demo password
  is shared by all demo accounts, so there is no new secret.
- A local database keeps `demo7`'s history relative to the day it was first created, as for `demo4` to `demo6`.
- The number of demo tasks keeps growing with each locked set of demo criteria. A later slice that needs another
  demo account weighs extending demo goal-change loading (whose criteria slice 8 locks) against a further task.
