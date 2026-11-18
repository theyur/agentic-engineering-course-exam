# 0018 — Evaluation on read, demo history loading and the daily demo refresh

**Status:** Accepted, 2026-11-18. Owner choices in slice 5, `specs/005-recording-and-evaluation` (critic
revision after spec pass 1; spec D-4, D-5 and D-6). Amends the "Later slices" rule of
[ADR 0007](0007-startup-operator-tasks.md) and the trigger of [ADR 0015](0015-demo-hosting.md); keeps
[ADR 0013](0013-private-demo-data.md) and [ADR 0016](0016-time-zones.md) (the operator sets the default time
zone) as they are. Decides the roadmap rows "Scheduled evaluation and background
processing" (ARCHITECTURE §9) and "Time-relative demo-data generation" (ARCHITECTURE §7, constitution VI).

## Context

- Constitution II: an evaluation is a deterministic function of stored facts, the settings version for each
  date and a supplied time. Slice 5 (FR-010) requires a commitment or evaluation period to become Missed at the
  end of its grace period without any action by a user or an operator.
- Slice 5 needs demo results relative to the current time: Met, Failed, Missed, Pending and Due, partial amounts
  and notes (SPEC §10, constitution VI). Records cannot be made for a day that has not begun, so demo records
  end on the day they are written. Every later day still ends Pending and then Missed (an unrecorded day is
  Pending during its grace period, FR-012), so every state stays present; but each day since the run adds an
  unrecorded day at the top of every daily goal's history. On day R + 4 the "Reading" goal's Failed day has
  left the first 10 History rows, on day R + 8 its last Met has too, and figures over a rolling 30 days
  (slices 6 and 9) drift towards Missed.
- ADR 0007 has later slices extend demo loading with their demo data. The locked slice-1 criteria (F001 AC-057
  to AC-060) say that demo loading's results name `demo1@example.com` to `demo3@example.com` "and no other
  email", and the locked slice-4 criteria (F004 AC-064 to AC-069) fix those accounts' goals, all starting on the
  day of the run. Neither the results nor those accounts can carry past results without unlocking shipped
  criteria.
- ADR 0015: the hosted demo is deployed and refreshed by a manual GitHub Actions workflow (`workflow_dispatch`
  only); a refresh empties the database and runs the startup tasks and the smoke check. The hosted demo runs
  until 2027-01-31. Its section "Not decided here" leaves time-relative demo data and scheduled evaluation to
  this slice.

## Options

Evaluation (D-4):

1. **Nothing runs in the background (chosen).** Every state is computed when it is read, from the stored
   records, the settings version of its date and the supplied time. No job writes Missed or stores results.
2. A scheduled job stores each final result when its grace period ends. A second copy of the truth, which
   every correction and late record has to keep in step; a missed or late job run shows a stale state; more to
   test. Rejected.

Where the demo history comes from (D-6):

1. **A separate startup task, demo history loading, that creates `demo4@example.com` (chosen).** Demo loading
   and its results stay exactly as F001 and F004 lock them.
2. Demo loading itself creates `demo4@example.com`. Needs F001 (and possibly F004) unlocked and re-translated.
   Rejected.
3. Records on the goals of `demo1` to `demo3`. Breaks F004's goal lists and start dates. Rejected.

Keeping the hosted demo current (D-5):

1. **A daily scheduled refresh (chosen).** The workflow of ADR 0015 also runs its refresh mode once a day, so
   `demo4` always has recent records and reads as an active user.
2. Manual refreshes only. Every state stays present, and reviewers' data lasts until the next deployment or
   refresh, but between refreshes the newest rows turn Missed: after about a week the goal pages show mostly
   Missed rows, and the smoke check could not rely on the first History page. Rejected by the owner on
   2026-11-18, after critic pass 2 corrected an earlier statement that Pending and Due would disappear.
3. Re-basing `demo4`'s records at every start. Writes private data into an account that already exists,
   against ADR 0013. Rejected.

## Decision

- **Evaluation on read.** No background processing evaluates, finalizes or stores results. The state of a
  commitment or evaluation period is derived at every read from stored facts and the supplied time; the passage
  of time alone turns Pending into Met, Failed or Missed. Later slices (reports, streaks, community statistics)
  compute from the same function and add caching only with their own decision.
- **Demo history loading.** A third startup operator task, after administrator creation and demo loading (ADR
  0007's order otherwise unchanged). With a valid demo password it creates `demo4@example.com` and, as one unit
  with the account (ADR 0013), its three demo goals that start 28 days before the date of the run and their
  records and notes, relative to that date. It takes no acting user, reads no private data, and leaves an account
  that already exists unchanged. Its results and log lines name the demo email and a result text only. ADR
  0007's "Later slices" rule now reads: each later slice extends demo loading or demo history loading with its
  demo data, by the same rules.
- **Run date.** The run date R of demo history loading is the date, at the run time, in the installation's
  default time zone, which is also `demo4`'s time zone. The hosted demo's operator sets that default (ADR 0016);
  today `tools/hosting/provision.ps1` sets no `Operator:DefaultTimeZone`, so it is UTC there and matches the
  schedule's UTC midnight. An operator who sets another default moves the schedule's time to just after that
  zone's midnight.
- **Daily refresh.** The hosting workflow gains a daily `schedule` trigger shortly after midnight UTC that runs
  refresh mode (empty database, migrations, restart, startup tasks, smoke check), next to its manual trigger.
  A demo refresh is then started either by the Operator or by this schedule. A scheduled run on a date after
  2027-01-31 does nothing and succeeds, so the trigger left in the workflow file after the teardown of the Azure
  resources never touches them or the `demo` environment; removing the trigger is a step of the Operator
  guide's "Teardown after 2027-01-31" (`docs/operations/hosted-demo.md`), which also describes the schedule.

## Consequences

- One source of truth for results; corrections and late records change states immediately, with nothing to
  re-synchronize. Every read recomputes, which is cheap at demo scale.
- F001 and F004 stay locked and unchanged. The startup log and the deployment's success check (ADR 0015, HP-017)
  gain demo history loading's lines; the smoke check also checks `demo4`.
- The hosted demo shows `demo4`'s Met, Failed, Missed, Pending and Due results on every day, among its first
  History rows. A late scheduled run, or one that fails before the database is emptied, leaves the previous
  day's data, which only gains a day of unrecorded rows at the top. A run that fails after the database is
  emptied leaves the hosted demo down or without data until the next successful run, as a failed manual refresh
  does today (ADR 0015, slice 3 HP-017); the failed run shows in the workflow's history, and the Operator can
  start a refresh by hand.
- Reviewers' own accounts and changes on the hosted demo last until the next refresh, at most about a day;
  sessions end at each refresh (slice 3 HP-018 keeps sessions only across other restarts); the hosted demo is
  unavailable for about 10 minutes each day.
- Slice 3's texts that say a demo refresh is started by the Operator (its "Demo refresh" entity, SC-006,
  `CONTEXT.md` Demo refresh and Operator, the roadmap's refresh row) now also cover the schedule; slice 5's spec
  lists them.
- A local database keeps `demo4`'s history relative to the day it was first created; it drifts until the
  database is recreated (ADR 0013's consequence for existing demo accounts).
