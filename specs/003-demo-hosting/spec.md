# Feature Specification: Demo hosting

**Feature Branch**: `claude/speckit-specify-slice-3-b938fd`

**Created**: 2026-11-15

**Status**: Draft

**Input**: User description: "slice 3 from the roadmap"

Roadmap outcome (slice 3, `demo-hosting`): reviewers open the app in a browser, with demonstration data,
without installing anything (SPEC §10, ARCHITECTURE §7). The app is hosted on Azure (SPEC §10, ARCHITECTURE
§2); which Azure services and tiers is still open (ARCHITECTURE §9). The roadmap calls this slice
infrastructure, not a feature: an ADR for hosting and deployment, then the deployment itself and the hosted
demo data. By the owner's answer to roadmap decision R2 (Clarifications), it is built at the end, after the
product slices, and starts no later than the latest start date recorded there, whatever has shipped by then.

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
  the roadmap's R2 row). The hosted app is then reachable before the exam on 2026-11-23.
- Q (plan, 2026-11-15): Does the owner confirm the author's decisions under "Critic revision 2026-11-15"? →
  A: Yes, all of them. They are owner answers from this date.
- Q (plan, 2026-11-15): Which Azure services and monthly budget? → A: App Service Basic B1 on Linux with Always
  On, and Azure SQL Database Basic; budget USD 25 a month (SC-007). Recorded in
  [ADR 0015](../../docs/adr/0015-demo-hosting.md).
- Q (plan, 2026-11-15): How do a deployment and a demo refresh run, including the schema migrations? → A: A
  manual GitHub Actions workflow, signed in to Azure with OpenID Connect: it empties the database, applies the
  deployed commit's migrations with an EF Core migration bundle while the app is stopped, deploys (not on a
  refresh), starts the app and runs the smoke check, which decides success. [ADR 0015](../../docs/adr/0015-demo-hosting.md).
- Q (plan, 2026-11-15): How does the hosted app get its passwords and connection, and keep sessions across idle
  restarts? → A: App Service app settings set once by the Operator, and the session keys on App Service's
  persistent storage. [ADR 0015](../../docs/adr/0015-demo-hosting.md) adds the setting that puts them there,
  because App Service on Linux documents no automatic key persistence.

### Critic revision 2026-11-15

Decisions made by the author while revising after [critic pass 1](critic-spec-pass-1.md) and
[critic pass 2](critic-spec-pass-2.md). They fill gaps the answers above left open and stay within them. The
owner confirmed all of them on 2026-11-15 (plan session above), so they are recorded decisions. The
requirements built on them are HP-014, HP-017 and HP-018.

- In the requirements, the person who starts deployments and refreshes, holds the secret configuration and
  reads the operator log is the **Operator** (`CONTEXT.md`); the clarifications above call that person the
  owner. "Owner" otherwise means the owner of private data, or the project owner who answers decisions.
- Secret values are passwords, connection strings, keys and tokens. The administrator email is configuration
  but not a secret: it stays out of source control, appears in the operator log (ADR 0007) and is given to
  reviewers with the exam submission. The demo emails and the published address are public and may appear in
  the repository; of the demo sign-ins, only the demo password is given exclusively with the exam submission.
- A deployment or demo refresh counts as succeeded only when the hosted app serves requests and its startup
  tasks created the administrator and every shipped slice's demo data, the seed catalog included. A start
  whose startup task faulted is a failed deployment or refresh, although the app starts (ADR 0007). After a
  failure the hosted demo may be unavailable or incomplete until the next successful deployment or refresh;
  that time counts against SC-006, whose smoke check then fails.
- A restart of the hosted app that is not a deployment or demo refresh, such as waking up after idling, does
  not end reviewers' sessions. Sessions still end the ways slice 1 defines (sign-out, closing the browser
  without "Keep me signed in", another session changing the account's password) and at every deployment or
  refresh.
- The configured administrator of the hosted demo has an `example.com` email and no real person's details,
  like the demo data (constitution VI), because every reviewer shares it.
- The hosted demo gets no extra seed content: it gets the seed catalog and demo data that every new deployment
  gets (answers slice 2's handed-over question, slice 2 Assumptions).
- The SPEC §10 outcome the latest start date secures is browser access before the exam. The demo data covers
  the slices shipped by then, which may not yet include progress reports or community views (roadmap R5).

## What acceptance tests can reach

Nothing in this slice. ADR 0003 limits acceptance tests to the Application use cases, the two
`HT.Persistence` entry points and the Domain types they use. This slice adds no use case and changes none:
it hosts the existing app, configures it, rolls out its schema and makes the existing demo loading (ADR 0007,
ADR 0013) run against the hosted database. Following the roadmap, the slice skips the acceptance-test states
(`acceptance locked`, `features locked`, `AC tests green`) and its requirements are host, deployment and
operation requirements (**HP-NNN**), verified by integration tests, deployment checks and a smoke check of
the hosted app that the Operator runs.

The decisions above add no application behaviour: the reviewers' administrator is the existing configured
administrator, and a demo refresh starts the hosted app on an empty database, where the existing
administrator creation and demo loading run as at every start. The refresh mechanism of
[ADR 0015](../../docs/adr/0015-demo-hosting.md) (a workflow that empties the database, applies the migrations
and restarts the app) needs no new use case, so this stays true.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Open the hosted app in a browser (Priority: P1)

A reviewer on Windows or macOS opens one public web address in a current browser and uses the app as it is in
the deployed commit of `master`: the visitor Home, the catalog, Register and Sign in. Nothing is installed,
and no local database is needed.

**Why this priority**: SPEC §10 and ARCHITECTURE §7 require it; without it nothing else in this slice has
value.

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
6. **Given** a reviewer signed in with "Keep me signed in" and nobody has changed that account's password,
   **When** the hosted app restarts after idling and the reviewer opens a page again, **Then** they are still
   signed in.

---

### User Story 2 - Explore with demonstration data (Priority: P1)

A reviewer signs in to a demo account and sees representative, visibly synthetic demo data from every
slice shipped by the time of deployment (at least the demo accounts of slice 1 and their custom habits from
slice 2), without waiting for real activity.

**Why this priority**: SPEC §10 asks for demo data "sufficient to explore" the app; an empty hosted app does
not meet it.

**Independent Test**: With the demo password from the hosted app's secret configuration, sign in to each
demo account on the hosted app and check that its demo data is there.

**Acceptance Scenarios**:

1. **Given** a deployment of the hosted app has succeeded (HP-017), **When** it has started, **Then** the demo
   accounts (at least `demo1@example.com` to `demo3@example.com`, more if a shipped slice adds them) exist with
   every shipped slice's demo data, and the seed catalog is loaded.
2. **Given** a reviewer has the demo password, **When** they sign in to a demo account, **Then** they see that
   account's demo data from every shipped slice (at least its custom habits) and nothing of another account's
   private data.
3. **Given** the hosted app restarts without a deployment, **When** demo loading runs again, **Then** no demo
   data is duplicated and existing data is left unchanged (ADR 0007).
4. **Given** the repository and the hosted app, **When** they are searched for the demo password and the
   administrator password, **Then** neither is found, and the Operator documentation (HP-015) names the items
   the exam submission gives reviewers (the published address, the demo emails, the administrator email and
   the two passwords) without the values of the administrator email and the passwords.
5. **Given** a reviewer has the administrator sign-in from the exam submission, **When** they sign in with
   it, **Then** they reach catalog administration (slice 2) and see no private data of any account
   (SPEC §3).

---

### User Story 3 - Deploy a new version (Priority: P2)

The Operator puts a chosen commit of `master` on the hosted app. The schema is brought up to date, the
administrator and the demo data are created as at every start, and the Operator can see whether the
deployment, including its startup tasks, succeeded.

**Why this priority**: the first deployment happens only days before the exam, and fixes after it are
deployed the same way; a deployment that needs manual database work or hides its failures would eat into
that margin.

**Independent Test**: Merge a change that adds a schema migration, deploy that commit, and check that the
hosted app serves it with the new schema and complete demo data, with no manual database step.

**Acceptance Scenarios**:

1. **Given** a commit of `master` whose schema differs from the deployed one, **When** the Operator deploys
   it, **Then** the hosted database has that commit's schema before the startup tasks run, and no manual
   database step is needed.
2. **Given** a deployment has finished, **When** the Operator checks it, **Then** they can tell which commit
   of `master` is deployed and whether the deployment succeeded (HP-017).
3. **Given** the hosted app has started, **When** the Operator reads the operator log, **Then** it shows the
   results of administrator creation and demo loading (ADR 0007) with emails and result texts, and no
   password, secret value, note or custom-habit name; no page or public address of the hosted app shows it.
4. **Given** a deployment fails (build, migration, start or a faulted startup task), **When** the Operator
   checks it, **Then** it is reported as failed with its reason, and no secret value appears in what they see.
5. **Given** the hosted configuration lacks the administrator or demo password, **When** the app starts,
   **Then** it starts and logs "no administrator configured" or "no demo password configured" (ADR 0007), and
   the deployment is reported as failed (HP-017).
6. **Given** a commit is merged to `master` and the Operator starts no deployment, **When** a reviewer opens
   the hosted app, **Then** it still runs the previously deployed commit with its data unchanged.

---

### User Story 4 - Keep the demo usable (Priority: P3)

Reviewers can change the demo: register accounts, add custom habits, change a demo account's password or
trigger its sign-in block (ADR 0007, Consequences). The Operator can bring the hosted demo back to its
known state so the next reviewer sees the full demo data.

**Why this priority**: the demo is public and shared; without a way back to a known state, one reviewer can
spoil it for the next.

**Independent Test**: Change a demo account's password and unpublish a catalog habit on the hosted app, start
a demo refresh, and sign in to that account again with the original demo password.

**Acceptance Scenarios**:

1. **Given** a reviewer has changed a demo account's password or its demo data, **When** the Operator starts a
   demo refresh, **Then** every demo account signs in with the original demo password and shows its full demo
   data.
2. **Given** a reviewer has changed the catalog or the administrator's password, **When** the hosted demo is
   refreshed, **Then** the catalog shows its full seed again and the administrator signs in with the
   configured sign-in.
3. **Given** a deployment has succeeded, **When** a reviewer opens the hosted app, **Then** the hosted demo has
   been refreshed: accounts and data that reviewers added are gone, and the demo data is complete.
4. **Given** a demo refresh has succeeded, **When** a reviewer opens the hosted app, **Then** the seed catalog
   and the demo data of every shipped slice are present, as on a freshly deployed empty database (ADR 0013).
5. **Given** a demo refresh fails (for example a startup task faults), **When** the Operator checks it,
   **Then** it is reported as failed with its reason (HP-009, HP-017), and no secret value appears in what they
   see.

---

### Edge Cases

- A reviewer changes a demo account's password: other reviewers cannot sign in to that account until the next
  refresh (User Story 4), and other reviewers already signed in to it lose their session (slice 1 HP-007). The
  same holds for the administrator's password, which every reviewer shares.
- A reviewer triggers a demo account's sign-in block with failed sign-ins: nobody can sign in to that account
  for 5 minutes (slice 1 FR-008); then it works again without a refresh.
- A reviewer uses the administrator sign-in to unpublish or edit seed catalog habits: other reviewers see the
  changed catalog until the next refresh.
- The hosted database is unreachable at start: the app still starts and logs the fault without secrets
  (ADR 0007). At a deployment or refresh, that is reported as failed (HP-017); at a restart without one (for
  example after idling), the fault shows in the operator log and the next smoke check fails. Pages that need
  the database show the app's error page or its error notice, neither of which contains a secret value, note
  or custom-habit name.
- A demo refresh fails: it is reported as failed with its reason (US4-AS5), and the hosted demo may be
  unavailable or incomplete until the next successful refresh or deployment.
- A deployment or refresh happens while reviewers are signed in: the database starts empty, so their next
  interaction asks them to sign in again, and accounts they registered are gone.
- A deployment's migration fails: the deployment stops, the new version does not serve requests against a
  partial schema, and the failure is visible to the Operator (US3-AS4). The hosted demo may be unavailable
  until the next successful deployment.
- The app is restarted or scaled back up after idling, without a deployment: the data and reviewers' sessions
  are kept (HP-018), and demo loading reports existing data and duplicates nothing (US1-AS6, US2-AS3).
- A demo email registered by a reviewer before demo loading runs: cannot happen after a successful deployment
  or refresh, because the database starts empty and demo loading runs before the app serves requests. If
  demo loading faulted, the deployment or refresh has failed (HP-017), and the incomplete hosted demo it leaves
  may let a reviewer register a demo email until the next successful one; demo loading then reports that
  account and writes no private data into it (ADR 0013).
- Demo data that depends on the current time (slice 5 onward, for example Pending results inside the 48-hour
  grace period or a rolling 30-day community window) goes stale between refreshes. If such slices have shipped
  when this slice resumes, the refresh decision covers keeping that data current until 2026-12-31 (Decisions
  due below).
- The hosted app is opened from a phone: the pages behave as they do locally at that width; no extra mobile
  work is in scope.
- The hosting tier sleeps after inactivity: the first request is slower (SC-001) but succeeds.

## Requirements *(mandatory)*

### Functional Requirements (host, deployment and operation; outside the AC surface)

- **HP-001**: The app MUST be reachable on Azure at one stable public web address from current desktop
  browsers on Windows and macOS, with nothing installed by the reviewer.
- **HP-002**: The hosted app MUST be served only over an encrypted connection; an unencrypted request MUST be
  redirected to the encrypted address. The secure session cookie of slice 1 MUST keep working.
- **HP-003**: The hosted app MUST run the code of the deployed commit of `master`, with the same behaviour as
  locally apart from configuration values and the behaviour the app already has outside the Development
  environment: the error page, strict-transport headers, and no migrations at start (ADR 0007).
- **HP-004**: Each deployment MUST bring the hosted database schema up to date with the deployed commit's
  schema migrations before administrator creation and demo loading run, with no manual database step. A failed
  migration MUST stop the deployment, and the new version MUST NOT serve requests against a partial schema.
- **HP-005**: Every deployment MUST start the hosted app on an empty database, so that administrator creation
  and demo loading create the administrator and every shipped slice's demo data (ADR 0013). Accounts and
  data added on the hosted demo do not survive a deployment.
- **HP-006**: At every start, the hosted app MUST run administrator creation and demo loading as ADR 0007
  describes, with the administrator email and password and the demo password taken from the hosted app's
  secret configuration.
- **HP-007**: Secret values (passwords, connection strings, keys and tokens) MUST NOT be in source control,
  deployment definitions, build output, logs or error pages, and no page of the hosted app shows the demo
  password. Reviewers get the demo password only with the exam submission; the demo emails and the published
  address are public.
- **HP-008**: Reviewers MUST be able to sign in as the configured administrator with the administrator email
  and password given with the exam submission. An administrator still has no access to private goals,
  activity, notes or custom habits of other accounts (SPEC §3, constitution III).
- **HP-009**: For each deployment and each demo refresh, the Operator MUST be able to see which commit runs,
  whether it succeeded (HP-017), and the reason when it failed.
- **HP-010**: The Operator MUST be able to read the hosted app's operator log, including the results of
  administrator creation and demo loading; no page or public address of the hosted app exposes it. The log
  MUST NOT contain secret values, notes, custom-habit names or recorded amounts (constitution III, slice 1
  HP-017); it may contain the administrator email and demo emails (ADR 0007).
- **HP-011**: Besides every deployment (HP-005), the Operator MUST be able to refresh the hosted demo at any
  time, without a new version and without editing the database by hand. A refresh brings the hosted demo to
  the state of a freshly deployed empty database, including the seed catalog and the configured
  administrator's sign-in.
- **HP-012**: Deploying a new version MUST NOT require changes to source code or committed files beyond the
  version itself; secret and environment values are set once, outside source control, and reused.
- **HP-013**: The hosted demo MUST use the lowest-cost Azure options that meet the other HP requirements,
  SC-001 and SC-006; no redundancy, autoscaling, multi-region setup or disaster recovery (SPEC §9,
  ARCHITECTURE §7). Nothing has to keep running after 2026-12-31.
- **HP-014**: The hosted demo data and the configured administrator MUST stay visibly synthetic: emails only in
  `example.com`, no real people (constitution VI).
- **HP-015**: The repository MUST document, for the Operator, how to deploy, how to refresh the demo, how to run
  the smoke check, where to read the deployment result and the operator log, and which items the exam
  submission has to give reviewers. It MUST NOT state any secret value or the administrator email; the
  published address and the demo emails may appear.
- **HP-016**: A deployment MUST start only when the Operator starts it, with one action, for a chosen commit of
  `master`. A merge or push alone MUST NOT deploy or reset the hosted demo.
- **HP-017**: A deployment or demo refresh MUST count as succeeded only when the hosted app serves requests and
  its startup tasks created the administrator and every shipped slice's demo data, the seed catalog included;
  otherwise it is reported as failed, even though the app starts. A failed deployment or refresh MAY leave the
  hosted demo unavailable or incomplete until the next successful one.
- **HP-018**: A restart of the hosted app that is not a deployment or demo refresh, such as waking up after
  idling, MUST NOT end any session. Sessions still end as slice 1 defines (HP-004 browser close without "Keep
  me signed in", HP-006 sign-out, HP-007 password change) and at every deployment or refresh.

### Key Entities

- **Hosted demo**: the one public instance of the app on Azure that reviewers use; it has a public address, a
  database and secret configuration.
- **Deployment**: one rollout of a chosen commit of `master` to the hosted demo, started by the Operator; it
  has the commit, a result (succeeded or failed, HP-017) and, on failure, a reason. The hosted demo itself is
  the installation it rolls out to (`CONTEXT.md`, Deployment and Hosted demo).
- **Secret configuration**: the hosted demo's administrator email, administrator password, demo password and
  database connection; held outside source control. Its secret values (the passwords and the connection) never
  appear in logs; the administrator email appears in the operator log.
- **Demo sign-in information**: the published address, the demo emails, the demo password, the administrator
  email and the administrator password; given to reviewers with the exam submission, where the two passwords
  are the only secret values.
- **Demo refresh**: bringing the hosted demo's data back to the state of a freshly deployed empty database;
  happens as part of every deployment, and on its own, without a new commit, whenever the Operator starts a
  refresh; it has a result like a deployment (HP-017).
- **Operator log**: the hosted app's log of startup tasks and faults, readable only by the Operator.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A reviewer who opens the published address sees the visitor Home within 30 seconds after the
  app has been idle, and every later page within 3 seconds, in at least 9 of 10 attempts.
- **SC-002**: A reviewer who has never seen the app signs in to a demo account and sees its demo data within
  3 minutes of receiving the address and the sign-in information, without installing anything and without
  contacting the Operator.
- **SC-003**: Deploying a new commit of `master`, including a schema migration, takes the Operator at most one
  action to start and finishes within 15 minutes, with no manual database step.
- **SC-004**: A search by the Operator of the repository, the deployment definitions and the operator log for
  the values in the secret configuration finds none of the passwords or the connection.
- **SC-005**: After every succeeded deployment and demo refresh, the smoke check signs in to every demo
  account with the demo password and finds every shipped slice's demo data, and finds the full seed catalog.
- **SC-006**: The smoke check passes (it reaches the hosted demo and finds what SC-005 checks) on 2026-11-23
  and on each weekly check from the first deployment until 2026-12-31, apart from the minutes of an
  Operator-started deployment or refresh.
- **SC-007**: The monthly hosting cost stays at or under USD 25, the budget of ADR 0015.
- **SC-008**: The hosted demo, with every slice shipped by then, is reachable before the exam on 2026-11-23.

## Decisions due before `/speckit-plan`

The roadmap places these technical decisions in this slice. They are ARCHITECTURE §9 deferred choices
(constitution, Decision Discipline). All were decided with the owner on 2026-11-15, recorded in
`## Clarifications` and in [ADR 0015](../../docs/adr/0015-demo-hosting.md); `research.md` cites them instead
of choosing them.

- Owner confirmation of the author's decisions under "Critic revision 2026-11-15": confirmed, all of them.
- ARCHITECTURE §9, Azure services and tiers, deployment, pipeline, monitoring and secrets mechanics: decided
  (ADR 0015), budget USD 25 a month (SC-007). Only slices 1 and 2 had shipped when it was decided; what later
  slices need from hosting, such as scheduled evaluation (slice 5), is decided by those slices, citing ADR 0015.
- ARCHITECTURE §9, schema migration rollout on each deployment, before the startup tasks run (HP-004): decided
  (ADR 0015). ADR 0007 is unchanged.
- ARCHITECTURE §9, how the hosted demo is refreshed and how its success is detected (HP-011, HP-017): decided
  (ADR 0015). No time-relative demo data had shipped; the slice that adds it keeps it current (Edge Cases).
- ARCHITECTURE §9, how the hosted app receives the passwords and how sessions survive a restart (HP-007,
  HP-018): decided (ADR 0015).
- Roadmap decision R2 (when to host the demo) is decided: at the end, by the latest start date in
  Clarifications.

## Assumptions

- Reviewers use a current desktop browser (Chrome, Edge, Firefox or Safari) on Windows or macOS; phone
  layouts work as they do locally but are not checked for this slice.
- One hosted instance is enough; there is no separate staging environment. The Operator verifies changes
  locally and through the tests before deploying.
- The hosted app may sleep when idle (Clarifications); SC-001 allows for a slow first request.
- The public address may be Azure's default address for the chosen service; a custom domain is not required.
- The hosted app keeps open registration as locally: anyone can register an account. Such accounts last
  only until the next deployment or refresh.
- No new page, banner or design element is added in this slice; the demo password reaches reviewers only
  through the exam submission (it is also in the hosted demo's secret configuration).
- The exam submission text is written by the owner outside the repository; this slice does not produce it.
  HP-015's documentation names the items it has to contain, without their secret values or the
  administrator email.
- Demo loading and administrator creation keep the behaviour of slices 1 and 2 (demo accounts, their custom
  habits, the seed catalog); this slice only supplies their configuration and database on the hosted app.
- Monitoring means the Operator can read deployment and refresh results and the operator log, and runs the
  smoke check; alerting, dashboards and continuous uptime monitoring are out of scope.
- "Deployment", "Hosted demo" and "Demo refresh" are added to `CONTEXT.md` in the same change, and the
  Operator entry is sharpened to separate the installation from its deployments (constitution I).
