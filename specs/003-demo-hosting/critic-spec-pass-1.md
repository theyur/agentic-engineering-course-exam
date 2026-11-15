# Critic pass 1: spec for 003-demo-hosting

**Verdict**: REVISE
**Date**: 2026-11-15
**Reviewed**: `specs/003-demo-hosting/spec.md`, `specs/003-demo-hosting/checklists/requirements.md`

Checked against: `.specify/memory/constitution.md`, `SPEC.md`, `ARCHITECTURE.md`, `CLAUDE.md`, `CONTEXT.md`,
`docs/roadmap.md`, ADR 0003 / 0005 / 0006 / 0007 / 0013, `specs/001-accounts-and-access/spec.md`,
`specs/002-habit-catalog/spec.md`, `README.md`, `.github/workflows/sonar.yml`, and the production code in
`src/HT.Ui` (`Program.cs`, `Operator/StartupTasks.cs`, `appsettings*.json`, `Properties/launchSettings.json`,
`Components/Pages/Error.razor`, `Components/Layout/MainLayout.razor`) and `src/HT.Persistence/HtDatabase.cs`.
No secret value was read; nothing was built or run.

## Notes

### MAJOR-1: A deployment or refresh can end with no demo data and still count as succeeded, and the state after a failed deployment is undefined

- **Where**: spec.md HP-004 (l.217-219), HP-005 (l.220-222), HP-006 (l.223-225), HP-009 (l.233-234), HP-011
  (l.238-241), US3-AS4 (l.148-149), US3-AS5 (l.150-151), US4-AS3 (l.175-176), US2-AS1 (l.108-110), Edge Cases
  (l.189-191, l.194-195, l.198-200), SC-006 (l.283-284), Key Entities "Deployment" (l.258-259), "What
  acceptance tests can reach" (l.58-60).
- **Against**: ADR 0007 Decision, "Results" and "Schema" (l.38-45: both startup tasks need the schema in place;
  a fault "is logged without secrets and does not stop the start either"); `src/HT.Ui/Operator/StartupTasks.cs`
  l.96-106 (`RunStepAsync` catches every exception and goes on) and `src/HT.Ui/Program.cs` l.60-63 (the tasks
  run once, before `RunAsync`, and nothing runs them again until the next start); slice 2 FR-030 (the seed
  catalog is loaded only with a valid demo password); slice 1 HP-018 / HP-021.
- **Reasoning**: The spec makes every deployment wipe the database (HP-005) and relies on the unchanged
  startup tasks to rebuild the administrator, the demo accounts, their private demo data and the seed catalog.
  Those tasks are non-fatal by design, so three outcomes exist that the spec does not classify:
  1. *Startup-task fault.* US3-AS4 lists the kinds of deployment failure as "(build, migration or start)". A
     start in which administrator creation or demo loading faulted (for example because the database was not
     yet reachable, or the schema was not yet there) is a successful start under ADR 0007, so it is none of the
     three. HP-009's "whether it succeeded" and the Deployment entity's "result (succeeded or failed)" give no
     definition that includes the demo data. US4-AS3 and US2-AS1 nevertheless require complete demo data "when
     the deployment has finished". The same deployment is therefore "succeeded" under US3-AS4 / HP-009 and
     failing under US4-AS3 / US2-AS1 (a P1 story).
  2. *Ordering.* HP-004 only requires the migrations "before that version serves requests". The startup tasks
     run at start, before requests are served, and need the schema (ADR 0007 "Schema"). A rollout that applies
     the migrations after the process has started but before traffic reaches it satisfies HP-004 and HP-006
     literally and leaves an empty demo, because the tasks have already faulted and do not run again. The spec's
     statement that "demo loading runs before the app serves requests" (l.199-200, used to declare an edge case
     impossible) holds only when the tasks succeed; US3-AS5 itself describes an accepted start without demo
     accounts, in which a reviewer can register a demo email.
  3. *After a failure.* HP-004 and the edge case at l.194-195 say what must not happen (the new version serving
     a partial schema) but not what the hosted demo is afterwards. Because HP-005 has already emptied the
     database, the previous version has no data to serve either. SC-006 exempts only "the minutes of an
     owner-started deployment or refresh", so a failed deployment a few days before the exam (US3's own
     rationale, l.131-133) leaves SC-006 and SC-008 either failed or undefined. A demo refresh (HP-011) has no
     visible result at all: HP-009 covers deployments only.

  These are outcome requirements, not mechanism, so they are not covered by "the mechanism is part of the
  hosting ADR" (l.33). Without them `/speckit-plan` has to choose what "succeeded" means and what a failed
  deployment leaves behind.

### MAJOR-2: The spec contradicts itself on what is secret: the administrator email is "never shown in logs" and is required in the log

- **Where**: spec.md Key Entities "Secret configuration" (l.260-261) and "Demo sign-in information"
  (l.262-263); US3-AS3 (l.145-147); HP-006 (l.223-225); HP-007 (l.226-229); HP-010 (l.235-237); HP-015
  (l.251-252); SC-004 (l.279-280); Clarifications (l.24-26); Assumptions (l.322-324).
- **Against**: ADR 0007 "Results" (l.38-39: the log holds the result text "and the email it is about");
  `src/HT.Ui/Operator/StartupTasks.cs` l.89-93 and l.114-115 (`Administrator creation: {Result} ({Email})` logs
  the configured administrator email); `README.md` l.71 and `CONTEXT.md` l.35 (the demo emails are in the
  repository); `CLAUDE.md` "Build and test" (the three `Operator:*` values, the administrator email among them,
  are treated as values never to be read or printed).
- **Reasoning**: Key Entities defines the secret configuration as "the hosted demo's administrator email and
  password, demo password and database connection; held outside source control, never shown in logs". US3-AS3
  requires the operator log to show the results of administrator creation "with emails and result texts", and
  the existing code, which the spec says is unchanged, writes the configured administrator email to that log.
  HP-010 ("MUST NOT contain passwords, secrets") and SC-004 ("finds no secret value") are then unsatisfiable
  under the spec's own entity definition, or satisfiable only under HP-007's narrower parenthetical
  ("passwords, connection strings, keys, tokens"), which the spec never reconciles with the entity. The same
  unclear boundary shows in two more places: HP-007 and the first clarification say the demo emails are given
  "only with the exam submission", while they are already in the repository (README, CONTEXT.md, this spec);
  and HP-015 / Assumptions say the repository documents the address "without the values", which leaves open
  whether the public address and the administrator email may appear in the repository or in deployment
  definitions. SC-004 cannot be evaluated until it is known which of these values count as secret.

### MAJOR-3: "the owner" replaces the glossary term "Operator" (constitution I)

- **Where**: spec.md US3 (l.127-129, l.140-149), US4 (l.158-159, l.169), HP-009, HP-010, HP-011, HP-015, HP-016
  (l.233-252), Key Entities "Operator log ... readable by the owner only" (l.266), SC-003, SC-006, Assumptions
  (l.314-315, l.327-330).
- **Against**: constitution I ("spec text ... MUST use the vocabulary of SPEC §2 and the `CONTEXT.md` glossary
  verbatim"; "Synonyms for defined terms MUST NOT appear"; a new or sharpened term "MUST add or update its
  `CONTEXT.md` entry ... in the same change"); `CONTEXT.md` l.40 ("Operator — the person who runs a deployment
  and holds its secret configuration (administrator credentials, demo password), and reads its operator log");
  `specs/001-accounts-and-access/spec.md` US4 (l.266-267, "The operator of a deployment") and Edge Cases
  (l.449-450); constitution III and `CONTEXT.md` l.28, l.30, l.34, where "owner" means the owner of private
  data.
- **Reasoning**: Every action the spec gives to "the owner" in its stories and requirements (starting a
  deployment, holding the secret configuration, reading the operator log, refreshing the demo) is the glossary
  definition of the Operator, the term slice 1's spec and ADR 0007 use. "Owner" is used in the constitution and
  the roadmap for the project owner who answers decisions, and in constitution III, CLAUDE.md and CONTEXT.md
  for the owner of private data ("owner-only"); "readable by the owner only" for the operator log reuses that
  privacy wording for a different person. The spec's own compliance statement (l.329-330, "No new domain term is
  introduced") does not mention the existing term it sets aside. That statement also excludes "hosted demo",
  "deployment" and "demo refresh" from `CONTEXT.md` as "operational terms", although the glossary already holds
  operational terms of the same kind (Operator l.40, Demo account l.35, Seed catalog l.43), and "demo refresh"
  is given a specific meaning in Key Entities (l.264-265) that later slices are expected to cite (roadmap
  decision row "When and how hosted demo data is refreshed").

### MINOR-1: Demo data that depends on the current time goes stale during the required lifetime, and the decision text still assumes hosting comes before slice 5

- **Where**: spec.md Clarifications (l.31-33, l.37-38, l.42-43), HP-011 (l.238-241), SC-005, SC-006
  (l.283-284), "Decisions due before `/speckit-plan`" first and third bullets (l.296-298, l.301-302) against the
  fifth (l.306-308).
- **Against**: constitution VI ("a reviewer sees its states in the hosted app without waiting for real time to
  pass (e.g. Pending, Met, Failed, Missed ...)"); roadmap slice note 5 ("Demo data has to show Pending, Met,
  Failed and Missed results relative to the current time") and slice note 9; SPEC §5.2 (48-hour grace period),
  §7.2 (rolling 30-day window); ADR 0007 "Later slices" last paragraph; ADR 0013 Consequences last bullet.
- **Reasoning**: By the R2 answer recorded in this spec, hosting is built after the product slices, so slices
  5, 6 and 9 may have shipped when it is deployed. The refresh triggers are only "every deployment" and "the
  owner asks". The demo must stay reachable until 2026-12-31, more than 30 days after the latest start date:
  Pending demo results become Missed 48 hours after a load, and a rolling 30-day community window empties
  without a refresh. The spec only says slice 5 "may reopen" the refresh decision. The first bullet still reads
  "Scheduled evaluation (slice 5) may need more later; changing this then needs a superseding ADR", which
  describes hosting before slice 5, while the fifth bullet says the decisions are taken after slice 5 may have
  shipped. The two bullets describe opposite orders.

### MINOR-2: Open ARCHITECTURE §9 choices are not marked as the constitution requires

- **Where**: spec.md "Decisions due before `/speckit-plan`" (l.289-308); checklist "No [NEEDS CLARIFICATION]
  markers remain" (l.16) and its fourth note (l.43-45).
- **Against**: constitution, Decision Discipline ("A spec that touches ... an ARCHITECTURE §9 deferred choice
  MUST mark it `[NEEDS CLARIFICATION: ... ARCHITECTURE §9]`"; "A plan MUST NOT pass its Constitution Check
  while a marker affecting it remains open"); ARCHITECTURE §9 (Azure service SKUs, deployment infrastructure,
  pipelines, monitoring); roadmap decision rows for `demo-hosting`, all still `open`.
- **Reasoning**: Four deferred technical choices are open (hosting services with the SC-007 budget, migration
  rollout, refresh mechanism, secrets mechanics). The spec lists them in prose and the checklist records that
  they are "not ... markers", but neither names the constitution clause it departs from. The plan gate the
  constitution attaches to an open marker therefore has nothing to trip on; only the spec's own sentence at
  l.291-294 stands between the open decisions and `/speckit-plan`.

### MINOR-3: "SPEC §10 is then met" overstates what a deployment on 2026-11-20 delivers

- **Where**: spec.md Clarifications, last answer (l.44-46); SC-008 (l.286-287).
- **Against**: SPEC §10 (demo data "sufficient to explore progress reports, the catalog, and community views");
  roadmap R5 ("SPEC §10 requires the demo to show community views"; owner answer: what has shipped is what the
  exam shows, "SPEC stays unchanged").
- **Reasoning**: With hosting started "whatever has shipped by then", progress reports (slice 6) and community
  views (slice 9) may be absent. The hosted app then meets the browser-access part of SPEC §10 and not its
  demo-data part. The clarification records the whole section as met.

### MINOR-4: The administrator account handed to reviewers is outside the "visibly synthetic" requirement

- **Where**: spec.md Clarifications (l.27-30), HP-008 (l.230-232), HP-014 (l.249-250).
- **Against**: constitution VI ("Demo data MUST be visibly synthetic (no real people, reserved domains such as
  `example.com`)"); `CONTEXT.md` l.35 (the demo data has no administrator of its own);
  `specs/001-accounts-and-access/spec.md` Assumptions (l.669-670).
- **Reasoning**: The clarification makes the configured administrator a shared reviewer account in all but
  name. HP-014 limits "hosted demo data" to `example.com` emails and no real people, but the administrator
  email comes from configuration and nothing in the spec constrains it. Whether the account every reviewer
  signs in to falls under HP-014 is not stated.

### MINOR-5: Whether a reviewer's session survives an idle restart is not addressed

- **Where**: spec.md Clarifications (l.39-41), HP-002 ("The secure session cookie of slice 1 MUST keep
  working", l.212-213), HP-003 (l.214-216), Edge Cases (l.196-197, l.203-204), Assumptions (l.316).
- **Against**: `specs/001-accounts-and-access/spec.md` HP-004 (a kept session lasts 7 to 14 days) and HP-007;
  `src/HT.Ui/Program.cs` l.29-47 (cookie sessions; no key-storage configuration anywhere under `src/`).
- **Reasoning**: The spec allows hosting that stops the app when idle and describes that restart only in terms
  of data ("the data is kept"). Session cookies and antiforgery tokens depend on keys the host keeps between
  starts; locally they persist by default. "MUST keep working" and "same behaviour as locally" imply slice 1
  HP-004 holds across an idle restart, but no scenario, edge case or listed decision covers it, so it is
  neither required explicitly nor verified.

### MINOR-6: Several success criteria and one scenario have no defined way to be checked

- **Where**: spec.md US2-AS4 (l.116-118), SC-002, SC-004, SC-005, SC-006, SC-007 (l.274-285), "What acceptance
  tests can reach" (l.54-56), Assumptions (l.322-324, l.327-328); checklist "Success criteria are measurable"
  (l.18) against its last note (l.46).
- **Against**: spec.md Assumptions (the exam submission "is written by the owner outside the repository; this
  slice does not produce it"; "uptime monitoring [is] out of scope"); user and project `CLAUDE.md` (secret
  values are never read, printed or put on a command line); ADR 0006 (no browser automation; a later slice
  that needs it decides with a new ADR).
- **Reasoning**: US2-AS4's first outcome tests the content of the exam submission, which the slice declares
  out of scope. SC-005 ("in 100% of checks") and US2 / US4 need sign-ins with the demo and administrator
  passwords, and SC-004 needs to know the secret values to search for them; the spec names "a scripted smoke
  check" but not who runs it or how it gets the values under the repository's secret rules. SC-006 says
  "reachable on every check" while uptime monitoring is out of scope, so no check is defined. SC-007 has no
  number; the checklist ticks "Success criteria are measurable" and notes in the same file that SC-007 is not
  yet measurable.

### MINOR-7: HP-003's list of environment differences is incomplete, and the database-fault edge case names the wrong surface

- **Where**: spec.md HP-003 (l.214-216); Edge Cases (l.189-191).
- **Against**: `src/HT.Ui/Program.cs` l.66-71 (`UseExceptionHandler("/Error")` and `UseHsts()` run only outside
  Development) and l.15-19 (the host throws at start without the `HabitTracker` connection string, which only
  `appsettings.Development.json` supplies); `src/HT.Ui/Components/Layout/MainLayout.razor` l.12-17
  (`blazor-error-ui`); `@rendermode InteractiveServer` on `Catalog`, `Admin`, `AdminCatalogHabit`,
  `CustomHabit`, `CustomHabits` and `SiteNav`.
- **Reasoning**: HP-003 allows only "configuration values and the environment-specific behaviour ADR 0007
  names (no migrations at start outside Development)". The code has two more environment-specific behaviours:
  the error page and HSTS exist only outside Development. The spec's own edge case depends on the first of
  them. That edge case also says pages that need the database "show the app's error page"; a fault inside an
  interactive component shows the layout's error bar instead of `/Error`.

### MINOR-8: "master", "current master", "a chosen commit" and "pending migration" are used inconsistently

- **Where**: spec.md US1 (l.68-69, "uses the app as it is on `master`"), US3 (l.127, "the current `master`";
  l.135-136 and l.140, "a pending schema migration"), SC-003 (l.277), against Clarifications (l.35-36, "the
  hosted app may lag behind `master`"), HP-003 (l.214, "the deployed commit"), HP-016 (l.244-245, "a chosen
  commit of `master`") and HP-005 (l.220-222).
- **Against**: internal consistency.
- **Reasoning**: US1 promises the state of `master`, while the clarification and HP-016 allow the hosted app
  to lag and to run any chosen commit. US3-AS1 and US3's Independent Test are built on "a pending schema
  migration", but under HP-005 every deployment starts from an empty database, so every migration is pending
  on every deployment and the scenario does not distinguish the case it describes.

### MINOR-9: The fallback for a refresh that needs a use case is ordered impossibly

- **Where**: spec.md "What acceptance tests can reach" (l.60-62).
- **Against**: roadmap, "Decisions each slice must settle" ("For a slice with no AC stage, decide before
  `/speckit-plan`"); constitution, Workflow (specify → clarify → plan).
- **Reasoning**: "If the plan finds that a refresh needs a new use case after all, this section is revisited
  before `/speckit-plan`". The plan is the output of `/speckit-plan`, so it cannot find anything before that
  command runs. The refresh mechanism is one of the decisions the spec defers to the hosting ADR, so whether
  the slice really has nothing on the AC surface (and may skip the three AC states) is still open when the
  spec asserts it.

### MINOR-10: The spec never states that hosting is on Azure

- **Where**: spec.md Edge Cases (l.203, "The hosting provider's free or lowest-cost tier"), HP-013
  (l.246-248), Assumptions (l.317), "Decisions due" first bullet (l.296, "Hosting services").
- **Against**: SPEC §10 ("The Azure-hosted version 1 application"); ARCHITECTURE §2 (Hosting: Azure) and §7;
  ARCHITECTURE §9, which defers only "Azure service SKUs, deployment infrastructure, pipelines, and
  monitoring"; CLAUDE.md "Source of truth".
- **Reasoning**: The word "Azure" does not appear in the spec. Its provider-neutral wording presents the
  provider as part of the open hosting decision, while SPEC and ARCHITECTURE have fixed it and left only the
  services and tiers open.

### MINOR-11: The start date is fixed in five places although the spec says the roadmap owns it

- **Where**: spec.md l.15, l.45-46, l.132, l.287 (SC-008), l.306.
- **Against**: spec.md Clarifications (l.45-46, "the owner may move it in the roadmap"); roadmap l.8-9
  ("There are **no dates**").
- **Reasoning**: If the date moves in the roadmap as the clarification allows, five statements in the spec,
  one of them a success criterion, become wrong. SC-008's second half ("work on this slice starts no later
  than 2026-11-20") is also a process deadline, not an outcome that can be measured on the hosted demo. The
  clarification points to the roadmap as the home of a date while the roadmap's own header rules dates out.

### MINOR-12: The checklist is stale and inaccurate against the spec

- **Where**: checklists/requirements.md l.18, l.27, l.34-40, l.43-45; spec.md l.244-245.
- **Against**: spec.md Clarifications (l.34-36), HP-012, HP-014, HP-015, HP-016, l.297.
- **Reasoning**: (a) The fourth note lists the "deployment trigger" among the technical decisions still to be
  taken, while the spec records it as decided (Clarifications, HP-016). (b) The checklist stops at
  "Iteration 2 ... All items pass" and does not cover the five later clarifications that added HP-016, SC-006
  and SC-008. (c) Iteration 1 names `design/` among the references the spec cites; the spec contains no
  `design/` reference. (d) "All functional requirements have clear acceptance criteria" is ticked, but HP-012,
  HP-014, HP-015 and the second sentence of HP-016 ("A merge or push alone MUST NOT deploy") have no acceptance
  scenario. (e) In the spec itself HP-016 stands between HP-012 and HP-013, out of numeric order (l.244-245).

### MINOR-13: Two statements about earlier slices are imprecise or leave a handed-over point unanswered

- **Where**: spec.md Assumptions (l.325-326); Key Entities "Operator log" (l.266).
- **Against**: `specs/002-habit-catalog/spec.md` FR-030, FR-031 and Assumptions (l.871-872: "Seed content
  added later (for example for the hosted demo, slice 3) needs its own decision"); spec.md HP-010.
- **Reasoning**: "Demo loading and administrator creation keep their slice 1 behaviour" omits that slice 2
  changed demo loading (seed catalog, demo custom habits), which US1-AS3 and US2 depend on. Slice 2 handed
  slice 3 the question of extra seed content for the hosted demo; the spec implies "none" but records no answer
  in Clarifications. Key Entities says the operator log is "readable by the owner only", but no requirement
  restricts who can read it; HP-010 only requires that the owner can.

## Summary

3 MAJOR, 13 MINOR. The spec covers the roadmap outcome, the four `demo-hosting` decision rows and R2, and its
claim that the existing startup tasks can carry the hosted demo is consistent with ADR 0007, ADR 0013 and the
code. It is not ready for the decisions and `/speckit-plan` that follow: the result of a deployment or refresh
is not defined against the non-fatal startup tasks and the wipe-first rule, so a P1 outcome and SC-006 can
fail without any requirement saying so; the boundary of "secret" contradicts itself and the unchanged logging
behaviour, which leaves HP-010 and SC-004 undecidable; and the actor of most requirements is named with a
synonym for a glossary term, against constitution I. The MINOR notes are mostly consistency and verification
gaps, the most consequential being time-relative demo data over the lifetime to 2026-12-31.
