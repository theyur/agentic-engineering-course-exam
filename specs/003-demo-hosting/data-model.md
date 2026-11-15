# Data model: Demo hosting

**Spec**: [spec.md](spec.md) · **Plan**: [plan.md](plan.md)

This slice adds **no domain type, no use case, no table and no migration**. The hosted database has exactly the
schema of the deployed commit's migrations (when planned, slices 1 and 2: `InitialAccounts`, `HabitCatalog`;
any slice shipped before the first deployment adds its own). The spec's Key
Entities are operational records, stored outside the app:

| Spec entity | Where it lives | Fields | Rules |
|---|---|---|---|
| Hosted demo | the Azure resource group of ADR 0015 | published address, web app, database, settings | one instance (spec Assumptions); deleted after 2026-12-31 |
| Deployment | one run of `deploy-demo.yml` in `deploy` mode | commit SHA, result (succeeded or failed), failed step, start time of the final start | started by the Operator only (HP-016); succeeded only when the smoke check passed (HP-017) |
| Demo refresh | one run of `deploy-demo.yml` in `refresh` mode | deployed commit SHA, result, failed step, start time | same success rule |
| Smoke check run (SC-006) | one run of `deploy-demo.yml` in `check` mode | deployed commit SHA, result, timings | changes nothing on the hosted demo; the run history is the check record |
| Secret configuration | web app app settings and connection string | [contracts/hosted-configuration.md](contracts/hosted-configuration.md) | set once by the provisioning script (HP-012); never in the repository or a log (HP-007) |
| Demo sign-in information | the exam submission, outside the repository | published address, demo emails, demo password, administrator email and password | the documentation names the items, never the secret values or the administrator email (HP-015) |
| Operator log | the web app's container log on its file system, kept 30 days | the startup task lines of ADR 0007 | read only after an Azure sign-in (HP-010) |

## State of the hosted database

```text
(no database) --create, migrations--> (empty schema) --start: startup tasks--> (seeded) --reviewers--> (changed)
      ^                                                                                                  |
      +------------------------------ next deployment or refresh: delete ------------------------------+
```

- *seeded*: the configured administrator, the demo accounts with their demo custom habits, and the seed catalog
  (ADR 0007, ADR 0013); what the smoke check verifies.
- *changed*: anything reviewers did; kept across restarts that are not a deployment or refresh (HP-018).

## Session keys

Data Protection keys: XML files in `/home/data-protection-keys` on the web app's persistent storage (research
R-07). Not in the database, so emptying the database does not delete them; sessions of the deleted accounts end
because the accounts no longer exist (slice 1's security-stamp check).
