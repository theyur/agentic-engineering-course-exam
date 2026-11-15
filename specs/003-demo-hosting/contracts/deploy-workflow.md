# Contract: deployment workflow `.github/workflows/deploy-demo.yml`

[ADR 0015](../../../docs/adr/0015-demo-hosting.md), research R-04, R-05. The step numbers below are the only
numbering; the ADR, the research and the plan use them.

## Trigger and inputs

- Trigger: `workflow_dispatch` only. No `push`, `pull_request` or `schedule` trigger (HP-016). GitHub offers the
  event only for a workflow file that exists on the default branch, so the first run follows the merge of the PR
  that adds it. A run can still be started against another branch, with that branch's version of the file, so:
  - the job's first statement fails unless `github.ref` is `refs/heads/master`;
  - the environment `demo` has a deployment branch policy that allows only `master`, so a job on another ref
    gets no Azure token (the federated credential is bound to the environment, research R-03).
- Inputs:

  | Input | Values | Default | Meaning |
  |---|---|---|---|
  | `mode` | `deploy`, `refresh`, `check` | `deploy` | `deploy` rolls out `commit`; `refresh` empties the database and restarts the deployed commit (HP-011); `check` changes nothing and runs only the smoke check (SC-006) |
  | `commit` | a full or short SHA, or `master` | `master` | the commit to deploy; ignored in `refresh` and `check` |

- `environment: demo`; `permissions: id-token: write, contents: read`;
  `concurrency: { group: deploy-demo, cancel-in-progress: false }`: a second run waits for the first.

## Steps

| # | Step | Modes | Fails when | Hosted demo after a failure here |
|---|---|---|---|---|
| 1 | Check the ref is `master`; Azure sign-in (OIDC) | all | another ref; sign-in error | unchanged |
| 2 | Check out `master` with full history and resolve the commit: `deploy` checks with `git merge-base --is-ancestor` that it is on `master` and resolves it to a full SHA; `refresh` and `check` read `Deployment__Commit` | all | not on `master`; `Deployment__Commit` empty | unchanged |
| 3 | Read the connection string, the two passwords and the administrator email from the web app and mask each with `::add-mask::` at once; an empty value fails with `configuration incomplete: <key name>` | all | a value missing or empty | unchanged |
| 4 | Check out the resolved commit; build the smoke check; `deploy` also publishes `src/HT.Ui` (`-c Release -r linux-x64 --self-contained false`); `deploy` and `refresh` also build the migration bundle (R-05) | all | build or bundle error | unchanged |
| 5 | Add the firewall rule `deploy-demo-runner` for the runner's address (replacing a leftover one), then delete the database if it exists | deploy, refresh | rule or delete error | the deployed version up, without a database (error page) |
| 6 | Start the web app (no effect when it runs) | deploy | start error | as step 5, or stopped |
| 7 | Set `Deployment__Commit` to empty, zip-deploy the publish output, then set `Deployment__Commit` to the full SHA | deploy | deployment error | an unknown version up, without a database; `Deployment__Commit` empty, so only a new `deploy` repairs it |
| 8 | Stop the web app (no effect when it is stopped) | deploy, refresh | stop error | as step 5 |
| 9 | Create the database, Basic tier | deploy, refresh | create error | stopped |
| 10 | Run the bundle with `HT_DESIGN_CONNECTION`; when it cannot connect, retry up to 3 times, 60 seconds apart (a new firewall rule can take up to five minutes to apply, and the rule was added in step 5) | deploy, refresh | bundle exit code not 0 after the retries | stopped, partial or empty schema, never served (HP-004) |
| 11 | Start the web app; write the UTC start time to the summary | deploy, refresh | start error | stopped |
| 12 | Smoke check against `DEMO_BASE_URL` with the three masked values as environment variables (contracts/smoke-check.md) | all | exit code not 0 | running, with what the startup tasks created (HP-017: failed) |
| 13 | `if: always()`: remove the firewall rule `deploy-demo-runner` if it exists; write the summary: mode, full commit SHA, result (succeeded or failed), failed step number and name, the start time of step 11 | all | — | — |

## Re-running after a failure

Every step tolerates the states that a failed run can leave, so the recovery for HP-017 and US3-AS4 is the
next run:

- step 5 skips a missing database; step 6 and step 8 do nothing when the app is already started or stopped;
- step 6 starts a web app that an earlier failure left stopped, so step 7 never deploys into a stopped web app
  (research R-04); the started app has no database and serves only the error page;
- step 7 clears `Deployment__Commit` before it deploys, so a `refresh` after a failed step 7 fails at step 2
  instead of migrating for a commit that may not be the one on the web app;
- step 13 always removes the firewall rule; a rule left by a cancelled run has the fixed name
  `deploy-demo-runner` and is replaced in step 5 of the next run.

## Startup tasks before the database exists

In `deploy` mode the app restarts in steps 6 and 7, after step 5 has deleted the database. Those starts fault
in administrator creation and demo loading and log "… failed; the app starts without it" (ADR 0007). They
serve no request against a schema, partial or not. Only the start of step 11 runs on the full schema, and its
lines follow the start time in the summary (plan, Spec discrepancies).

## Guarantees

- A merge or push never starts a run (HP-016).
- A succeeded `deploy` or `refresh` leaves the hosted app on a new, empty database filled only by the startup
  tasks of step 11 (HP-004, HP-005, HP-011).
- No secret value reaches the log: every value read in step 3 is masked when it is read, the administrator
  email included, and none is passed as a command-line argument (HP-007).
- The run's result is the result of the deployment, refresh or check (HP-009, HP-017). The run log shows step
  names, non-secret identifiers, the commit and the smoke check's lines, and nothing else from the hosted app.
- Expected duration of `deploy` about 10 minutes, at most 15 (SC-003).
