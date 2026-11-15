# Feature Specification: Demo hosting

**Feature Branch**: `claude/speckit-specify-slice-3-b938fd`

**Created**: 2026-11-15

**Status**: Draft

**Input**: User description: "slice 3 from the roadmap"

Roadmap outcome (slice 3, `demo-hosting`): reviewers open the app in a browser, with demonstration data,
without installing anything (SPEC §10, ARCHITECTURE §7). The roadmap calls this slice infrastructure, not a
feature: an ADR for hosting and deployment, then the deployment itself and the hosted demo data. By the
owner's answer to roadmap decision R2 (Clarifications), it is built at the end, after the product slices,
and starts no later than 2026-11-20, whatever has shipped by then.

This slice adds no product behaviour. It makes the behaviour of the slices shipped by its deployment, and of
any slice deployed after it, reachable for reviewers.

## Clarifications

### Session 2026-11-15

- Q: How do reviewers get the demo sign-ins (`demo1@example.com` to `demo3@example.com` and the demo
  password), with no secret in source control? → A: Only with the exam submission, outside the repository.
  The hosted site shows no sign-in information, so no page or design change is needed.
- Q: Do reviewers get administrator access on the hosted app, to see catalog administration? → A: Yes: the
  sign-in of the configured administrator (ADR 0007) is delivered the same way as the demo sign-ins. No
  separate demo administrator, so no new application behaviour. What a reviewer changes as administrator is
  undone by the next demo refresh; an administrator still sees no private data (SPEC §3).
- Q: When is the hosted demo data brought back to its known state? → A: On every deployment, when the hosted
  database starts empty, and whenever the owner asks for it. Accounts and data that reviewers add do not
  survive a deployment. The mechanism is part of the hosting ADR.
- Q: Does a merge to `master` deploy to the hosted demo automatically, or only when the owner starts a
  deployment? → A: Only when the owner starts it, with one action (SC-003). A merge alone never deploys, so the
  owner decides when the demo is reset; the hosted app may lag behind `master` until then.
- Q: Until when must the hosted demo stay reachable? → A: From its first deployment until 2026-12-31: the
  exam on 2026-11-23 plus time for late review. After that the owner may stop it.
- Q: Is a slow first page after the hosted app has been idle acceptable? → A: Yes: up to 30 seconds for the
  first page after idling and 3 seconds for later pages (SC-001). Hosting tiers that put an idle app to sleep
  are allowed.
- Q: Roadmap decision R2: host the demo right after `habit-catalog`, or at the end? → A: At the end, after the
  product slices. This spec is kept and parked until then.
- Q: What if not every slice has shipped by the exam? → A: Hosting starts no later than a fixed date before the
  exam, whatever has shipped by then: 2026-11-20 (the example date of the question; the owner may move it in
  the roadmap). SPEC §10 is then met before the exam on 2026-11-23.

## What acceptance tests can reach

Nothing in this slice. ADR 0003 limits acceptance tests to the Application use cases, the two
`HT.Persistence` entry points and the Domain types they use. This slice adds no use case and changes none:
it hosts the existing app, configures it, rolls out its schema and makes the existing demo loading (ADR 0007,
ADR 0013) run against the hosted database. Following the roadmap, the slice skips the acceptance-test states
(`acceptance locked`, `features locked`, `AC tests green`) and its requirements are host, deployment and
operation requirements (**HP-NNN**), verified by integration tests, deployment checks and a scripted smoke
check of the hosted app.

The clarifications above add no application behaviour: the reviewers' administrator is the existing
configured administrator, and a demo refresh starts the hosted app on an empty database, where the existing
administrator creation and demo loading run as at every start. If the plan finds that a refresh needs a new
use case after all, this section is revisited before `/speckit-plan`, and that behaviour gets FR-NNN
requirements and the acceptance-test states.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Open the hosted app in a browser (Priority: P1)

A reviewer on Windows or macOS opens one public web address in a current browser and uses the app as it is
on `master`: the visitor Home, the catalog, Register and Sign in. Nothing is installed, and no local database
is needed.

**Why this priority**: SPEC §10 and ARCHITECTURE §7 require it; without it nothing else in this slice has
value, and later slices cannot be shown to reviewers.

**Independent Test**: From a machine that has never run the app, open the published address in a browser
and walk the visitor pages and the catalog; register a new account and sign in with it.

**Acceptance Scenarios**:

1. **Given** the hosted app is deployed, **When** a reviewer opens the published address, **Then** the
   visitor Home is shown over an encrypted connection, with the navigation and sections of the shipped slices.
2. **Given** a reviewer opens the address with an unencrypted `http://` link, **When** the page loads,
   **Then** the reviewer ends up on the encrypted address and sees the same page.
3. **Given** the hosted app is deployed, **When** a reviewer opens the catalog, **Then** it shows the seed
   catalog (at least 20 Published habits in all six categories, slice 2 FR-030), and no Draft or Unpublished
   habit.
4. **Given** the hosted app is deployed, **When** a reviewer registers a new account and signs in with it,
   **Then** the signed-in Home is shown, as it is locally.
5. **Given** the hosted app has been idle for a while, **When** a reviewer opens the address, **Then** the
   first page appears within the time in SC-001, without an error page.

---

### User Story 2 - Explore with demonstration data (Priority: P1)

A reviewer signs in to a demo account and sees representative, visibly synthetic demo data from every
slice shipped by the time of deployment (at least the demo accounts of slice 1 and their custom habits from
slice 2), without waiting for real activity.

**Why this priority**: SPEC §10 asks for demo data "sufficient to explore" the app; an empty hosted app does
not meet it.

**Independent Test**: Using only the sign-in information the reviewer is given (see HP-007), sign in to each
demo account on the hosted app and check that its demo data is there.

**Acceptance Scenarios**:

1. **Given** the hosted app is freshly deployed on an empty database, **When** it has started, **Then** the
   demo accounts `demo1@example.com` to `demo3@example.com` exist with their demo data, and the seed catalog
   is loaded.
2. **Given** a reviewer has the demo sign-in information, **When** they sign in to a demo account, **Then**
   they see that account's demo data from every shipped slice (at least its custom habits) and nothing of
   another account's private data.
3. **Given** the hosted app restarts, **When** demo loading runs again, **Then** no demo data is duplicated
   and existing data is left unchanged (ADR 0007).
4. **Given** a reviewer has the exam submission, **When** they look for the demo sign-ins, **Then** the
   submission gives the published address, the demo emails and the demo password, and neither the
   repository nor any page of the hosted app contains the demo password.
5. **Given** a reviewer has the administrator sign-in from the exam submission, **When** they sign in with
   it, **Then** they reach catalog administration (slice 2) and see no private data of any account
   (SPEC §3).

---

### User Story 3 - Deploy a new version (Priority: P2)

The owner puts the current `master` on the hosted app. The schema is brought up to date, the administrator
and the demo data are created as at every start, and the owner can see whether the deployment and the
startup tasks succeeded.

**Why this priority**: the first deployment happens only days before the exam (start no later than
2026-11-20, exam 2026-11-23), and fixes after it are deployed the same way; a deployment that needs manual
database work or hides its failures would eat into that margin.

**Independent Test**: Merge a change that adds a schema migration, deploy it, and check that the hosted app
serves the new version with the new schema, with no manual database step.

**Acceptance Scenarios**:

1. **Given** a new version on `master` with a pending schema migration, **When** the owner deploys it,
   **Then** the hosted database gets the migration before the new version serves requests, and no manual
   database step is needed.
2. **Given** a deployment has finished, **When** the owner checks it, **Then** they can tell which commit of
   `master` is deployed and whether it succeeded.
3. **Given** the hosted app has started, **When** the owner reads the operator log, **Then** it shows the
   results of administrator creation and demo loading (ADR 0007) with emails and result texts, and no
   password, secret, note or custom-habit name.
4. **Given** a deployment fails (build, migration or start), **When** the owner checks it, **Then** the
   failure and its reason are visible to the owner, and no secret value appears in what they see.
5. **Given** the hosted configuration lacks the administrator or demo password, **When** the app starts,
   **Then** it starts and logs "no administrator configured" or "no demo password configured" (ADR 0007).

---

### User Story 4 - Keep the demo usable (Priority: P3)

Reviewers can change the demo: register accounts, add custom habits, change a demo account's password or
lock it with failed sign-ins (ADR 0007, Consequences). The owner can bring the hosted demo back to its known
state so the next reviewer sees the full demo data.

**Why this priority**: the demo is public and shared; without a way back to a known state, one reviewer can
spoil it for the next.

**Independent Test**: Change a demo account's password and unpublish a catalog habit on the hosted app, ask
for a demo refresh, and sign in to that account again with the original demo sign-in information.

**Acceptance Scenarios**:

1. **Given** a reviewer has changed a demo account's password or its demo data, **When** the owner asks for a
   demo refresh, **Then** every demo account signs in with the original demo sign-in information and shows
   its full demo data.
2. **Given** a reviewer has changed the catalog or the administrator's password, **When** the hosted demo is
   refreshed, **Then** the catalog shows its full seed again and the administrator signs in with the
   configured sign-in.
3. **Given** a new version is deployed, **When** the deployment has finished, **Then** the hosted demo has
   been refreshed: accounts and data that reviewers added are gone, and the demo data is complete.
4. **Given** the hosted demo is refreshed, **When** it is done, **Then** the seed catalog and the demo data of
   every shipped slice are present, as on a freshly deployed empty database (ADR 0013).

---

### Edge Cases

- A reviewer changes a demo account's password or locks it out with failed sign-ins: other reviewers cannot
  use that account until the next refresh (User Story 4). The same holds for the administrator's password,
  which every reviewer shares.
- A reviewer uses the administrator sign-in to unpublish or edit seed catalog habits: other reviewers see the
  changed catalog until the next refresh.
- The hosted database is unreachable at start: the app still starts and logs the fault without secrets
  (ADR 0007); pages that need the database show the app's error page, which contains no secret, note or
  custom-habit name.
- A deployment happens while reviewers are signed in: the database is refreshed, so their next interaction
  asks them to sign in again, and accounts they registered are gone.
- A deployment's migration fails: the deployment stops, the new version does not serve requests against a
  partial schema, and the failure is visible to the owner (US3-AS4).
- The app is restarted or scaled back up after idling, without a deployment: the data is kept, and demo
  loading reports existing data and duplicates nothing (US2-AS3).
- A demo email registered by a reviewer before demo loading runs: cannot happen on the hosted demo, because
  every deployment and refresh starts on an empty database and demo loading runs before the app serves
  requests. Elsewhere ADR 0013 applies.
- The hosted app is opened from a phone: the pages behave as they do locally at that width; no extra mobile
  work is in scope.
- The hosting provider's free or lowest-cost tier sleeps after inactivity: the first request is slower
  (SC-001) but succeeds.

## Requirements *(mandatory)*

### Functional Requirements (host, deployment and operation; outside the AC surface)

- **HP-001**: The app MUST be reachable at one stable public web address from current desktop browsers on
  Windows and macOS, with nothing installed by the reviewer.
- **HP-002**: The hosted app MUST be served only over an encrypted connection; an unencrypted request MUST be
  redirected to the encrypted address. The secure session cookie of slice 1 MUST keep working.
- **HP-003**: The hosted app MUST run the same code as the deployed commit of `master`, with the same
  behaviour as locally, apart from configuration values and the environment-specific behaviour ADR 0007
  names (no migrations at start outside Development).
- **HP-004**: Each deployment MUST bring the hosted database schema up to date with the deployed version's
  schema migrations before that version serves requests, with no manual database step. A failed migration
  MUST stop the deployment, and the new version MUST NOT serve requests against a partial schema.
- **HP-005**: Every deployment MUST start the hosted app on an empty database, so that administrator creation
  and demo loading create the administrator and every shipped slice's demo data (ADR 0013). Accounts and
  data added on the hosted demo do not survive a deployment.
- **HP-006**: At every start, the hosted app MUST run administrator creation and demo loading as ADR 0007
  describes, with the administrator email and password and the demo password taken from the hosted app's
  secret configuration.
- **HP-007**: Reviewers MUST be able to sign in to the demo accounts with the published address, the demo
  emails and the demo password given only with the exam submission. No page of the hosted app shows the demo
  password. No secret value (passwords, connection strings, keys, tokens) MAY be in source control,
  deployment definitions, build output, logs or error pages.
- **HP-008**: Reviewers MUST be able to sign in as the configured administrator with the sign-in given the
  same way as the demo sign-ins. An administrator still has no access to private goals, activity, notes or
  custom habits of other accounts (SPEC §3, constitution III).
- **HP-009**: The owner MUST be able to see, for each deployment, which commit was deployed, whether it
  succeeded, and the reason when it failed.
- **HP-010**: The owner MUST be able to read the hosted app's operator log, including the results of
  administrator creation and demo loading. The log MUST NOT contain passwords, secrets, notes, custom-habit
  names or recorded amounts (constitution III, slice 1 HP-017).
- **HP-011**: Besides every deployment (HP-005), the owner MUST be able to refresh the hosted demo at any
  time, without a new version and without editing the database by hand. A refresh brings the hosted demo to
  the state of a freshly deployed empty database, including the catalog seed and the configured
  administrator's sign-in.
- **HP-012**: Deploying a new version MUST NOT require changes to source code or committed files beyond the
  version itself; secret and environment values are set once, outside source control, and reused.
- **HP-016**: A deployment MUST start only when the owner starts it, with one action, for a chosen commit of
  `master`. A merge or push alone MUST NOT deploy or reset the hosted demo.
- **HP-013**: The hosted demo MUST use the lowest-cost options that meet the other HP requirements, SC-001
  and SC-006; no redundancy, autoscaling, multi-region setup or disaster recovery (SPEC §9, ARCHITECTURE §7).
  Nothing has to keep running after 2026-12-31.
- **HP-014**: The hosted demo data MUST stay visibly synthetic: emails only in `example.com`, no real people
  (constitution VI).
- **HP-015**: The repository MUST document, for the owner, how to deploy, where to read the deployment result
  and the operator log, and how to refresh the demo data, without stating any secret value.

### Key Entities

- **Hosted demo**: the one public instance of the app that reviewers use; it has a public address, a
  database and secret configuration.
- **Deployment**: one rollout of a commit of `master` to the hosted demo; it has the commit, a result
  (succeeded or failed) and, on failure, a reason.
- **Secret configuration**: the hosted demo's administrator email and password, demo password and database
  connection; held outside source control, never shown in logs.
- **Demo sign-in information**: the published address, the demo emails, the demo password and the
  administrator sign-in; given to reviewers only with the exam submission.
- **Demo refresh**: bringing the hosted demo's data back to the state of a freshly deployed empty database;
  happens on every deployment and whenever the owner asks for it.
- **Operator log**: the hosted app's log of startup tasks and faults, readable by the owner only.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A reviewer who opens the published address sees the visitor Home within 30 seconds after the
  app has been idle, and every later page within 3 seconds, in at least 9 of 10 attempts.
- **SC-002**: A reviewer who has never seen the app signs in to a demo account and sees its demo data within
  3 minutes of receiving the address and the sign-in information, without installing anything and without
  contacting the owner.
- **SC-003**: Deploying a new version of `master`, including a schema migration, takes the owner at most one
  action to start and finishes within 15 minutes, with no manual database step.
- **SC-004**: A search of the repository, the deployment definitions and the hosted app's logs finds no
  secret value.
- **SC-005**: After a demo refresh, all three demo accounts sign in with the original sign-in information and
  show their full demo data, and the catalog shows its full seed, in 100% of checks.
- **SC-006**: The hosted demo is reachable on every check from its first deployment until 2026-12-31 (the
  exam is on 2026-11-23), apart from the minutes of an owner-started deployment or refresh.
- **SC-007**: The monthly hosting cost stays at or under the budget the owner sets in the hosting ADR.
- **SC-008**: The hosted demo, with every slice shipped by then, is reachable before the exam on 2026-11-23;
  work on this slice starts no later than 2026-11-20.

## Decisions due before `/speckit-plan`

The roadmap places these technical decisions in this slice. This slice has no acceptance-test stage, so each
is decided with the owner before `/speckit-plan`, written as an ADR where the roadmap row or the
constitution's test asks for one, and recorded as a dated line in `## Clarifications` that links the ADR.
`research.md` then cites the answers instead of choosing them.

- Hosting services, deployment, pipelines, monitoring and secrets mechanics (ADR), including the budget of
  SC-007. That the owner starts each deployment is decided (Clarifications, HP-016); how is the ADR's. Scheduled
  evaluation (slice 5) may need more later; changing this then needs a superseding ADR.
- Schema migration rollout on each deployment (HP-004). Locally the host applies migrations at start in
  Development only (ADR 0007).
- How the hosted demo data is refreshed. When is decided (Clarifications: every deployment and on the
  owner's demand); the mechanism goes into the ADR. Time-relative demo data (slice 5) may reopen it.
- How reviewers get demo sign-ins with no secret in source control. The channel is decided (Clarifications:
  only with the exam submission); how the hosted app receives the demo and administrator passwords is part
  of the secrets mechanics above.
- Roadmap decision R2 (when to host the demo) is decided: at the end, starting no later than 2026-11-20
  (Clarifications). The technical decisions above are taken when the slice resumes, so they can account for
  the slices shipped by then (for example scheduled evaluation from slice 5).

## Assumptions

- Reviewers use a current desktop browser (Chrome, Edge, Firefox or Safari) on Windows or macOS; phone
  layouts work as they do locally but are not checked for this slice.
- One hosted instance is enough; there is no separate staging environment. The owner verifies changes
  locally and through the tests before deploying.
- The hosted app may sleep when idle (Clarifications); SC-001 allows for a slow first request.
- The public address may be the hosting provider's default address; a custom domain is not required.
- The hosted app keeps open registration as locally: anyone can register an account. Such accounts last
  only until the next deployment or refresh.
- No new page, banner or design element is added in this slice; the demo sign-in information lives only in
  the exam submission.
- The exam submission text is written by the owner outside the repository; this slice does not produce it,
  but HP-015's documentation lists what it has to contain (address, demo emails, where the passwords come
  from), without the values.
- Demo loading and administrator creation keep their slice 1 behaviour; this slice only supplies their
  configuration and database on the hosted app.
- Monitoring means the owner can read deployment results and the operator log; alerting, dashboards and
  uptime monitoring are out of scope.
- No new domain term is introduced; "hosted demo", "deployment" and "demo refresh" are operational terms and
  do not go into `CONTEXT.md`.
