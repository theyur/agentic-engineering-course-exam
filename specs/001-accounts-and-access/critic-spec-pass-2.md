# Critic pass 2: spec for 001-accounts-and-access

**Verdict**: REVISE
**Date**: 2026-11-11
**Reviewed**: `specs/001-accounts-and-access/spec.md`, `specs/001-accounts-and-access/checklists/requirements.md`
(with the related uncommitted edits to `CONTEXT.md`, `docs/roadmap.md`, `README.md`, `CLAUDE.md`)

## Previous notes: resolution status

Only one earlier pass exists (`critic-spec-pass-1.md`), so there are no older notes that could regress.

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 (FRs with no behaviour on the AC surface) | Partly | Page, browser, log and process rules are now HP-001–HP-017 and "Design changes before build"; `check_acceptance.py` reads only `**FR-NNN**` (line 17), and its own parser finds 16 FRs and 55 scenarios with unique, sequential ids, 24 of them marked *(page)*. FRs carry literal messages instead of pointing to `design/`. Residue: FR-010 and parts of FR-003, FR-014 and FR-016 are still not observable through a use case (MINOR-3). |
| MAJOR-2 (decisions due before `/speckit-ac-translate` assigned to `/speckit-plan`) | Partly | The wrong assumption is gone; `spec.md` lines 477–489 list the five open roadmap rows and the ADRs due, and schedule them before `/speckit-ac-translate`, as `docs/roadmap.md` lines 128–131 and 140–149 require. The author's rationale for not deciding them inside the spec is sound on timing: the roadmap asks for them "before `/speckit-ac-translate`", and the next step is `/speckit-clarify`. But the spec does not leave the decision open: it fixes its main consequence without an owner answer (MAJOR-1). |
| MAJOR-3 (lockout and sign-in particulars assumed) | Resolved | `spec.md` lines 37–44 record owner answers with literal values (5 failures, 5 minutes, same message; 14 days renewed by each visit; taken-email message). FR-007, FR-008, HP-004 use them. Roadmap line 160 is `decided`. |
| MAJOR-4 (community state in and out of scope) | Resolved | Key Entities (lines 451–453) no longer hold community state; FR-004, US1 and US7 no longer mention it; Assumptions line 505 keeps it in slice 9. |
| MAJOR-5 (FR-016 self-contradiction on administrator creation) | Resolved | FR-014 (lines 374–383) is case by case; Clarifications lines 30–32 and US4-AS5 settle the changed email; several administrators are stated as possible. |
| MINOR-1 (demo accounts: credential source, shared-account cases) | Partly | FR-016 and US7-AS1–AS4 add the configured demo password, reload, collision and missing-password cases; Assumptions line 499–500 covers the missing demo administrator. New gaps in the same area: MINOR-2. |
| MINOR-2 (email classified as private from administrators) | Partly | FR-012, FR-015 and US3 no longer call the email private; Assumptions lines 501–502 defer to slice 11; `CONTEXT.md` line 27 says "never shown publicly". SC-005 now names the two use cases, but what the "another registered user" and "administrator" tests attempt is still undefined (MINOR-8). |
| MINOR-3 (message while blocked) | Resolved | FR-007 and US2-AS4: the blocked state uses "Email or password is not correct." |
| MINOR-4 (design-first applied unevenly) | Partly | The five deviations named in pass 1 are in "Design changes before build" (lines 436–447). The list is still incomplete, and the checklist claims it is complete (MINOR-6). |
| MINOR-5 (one Home or two) | Partly | HP-009 gives Home one address; US3-AS7 uses Profile. HP-008 still sends an anonymous visitor who opens the signed-in Home to Sign in (MINOR-1). |
| MINOR-6 (password change and administrator credential cases) | Resolved | HP-007 and US6-AS5 (other sessions end); FR-014 fourth bullet and US4-AS4 (missing, invalid or weak configured credentials). A new ambiguity in HP-007's wording is MINOR-7. |
| MINOR-7 (cross-feature decisions without ADR) | Resolved | `spec.md` lines 488–489 state that ADRs are due for R7, no outbound email / no password recovery, and administrator creation. `docs/adr/` still holds only 0001–0003; the ADRs themselves are pending by design. |
| MINOR-8 (roadmap edits contradict the spec) | Resolved | `docs/roadmap.md` line 113 ("Today" heading), lines 207 and 210 (R6, R9 answered), `README.md` line 53 (`001-accounts-and-access/`), `CLAUDE.md` roadmap pointer. One leftover sentence in the roadmap is cited in MAJOR-1. |
| MINOR-9 (edge cases with no FR; wrong constitution citation) | Partly | FR-003 carries email validity and simultaneous registration; HP-008 the Back button; HP-017 the log rule without the wrong citation. Email validity still has no rule and no literal message (MINOR-4). |
| MINOR-10 (terms missing from the glossary; loose "visitor") | Partly | `CONTEXT.md` line 28 adds "Acting user"; Role is described in the Account entry. "Visitor" is still used for any user, and new terms are undefined (MINOR-10). |
| MINOR-11 (numbering; scenarios with two behaviours) | Partly | Numbering is sequential in all seven stories and FR-031 is gone. Several scenarios still hold more than one behaviour, against the checklist's claim (MINOR-4). |
| MINOR-12 (checklist items passed without qualification) | Partly | `requirements.md` lines 40–49 add three qualifications. Other items are still ticked without one, and two rows of the verification table overstate the change (MINOR-11). |

## Notes

### MAJOR-1: The spec fixes how much of the slice is on the AC surface, which is part of an open owner decision, without a recorded owner answer

- **Where**: `spec.md` lines 59–62 ("Each one MUST be reachable through the Application use cases … The technical
  decisions due before `/speckit-ac-translate` … must keep that possible"), line 332 (heading "application
  behaviour, on the AC surface"), lines 482–483 ("It must keep every FR above on the AC surface"), FR-004
  (lines 344–345), FR-010, FR-014, FR-016; `checklists/requirements.md` lines 37–39 and 55–56.
- **Against**: Constitution, Decision Discipline (lines 136–140: a touched ARCHITECTURE §9 choice is marked "instead
  of assuming an answer"; "The answer comes from the project owner … recorded in the spec's `## Clarifications`
  section with its date"); `ARCHITECTURE.md` §9 ("Concrete use-case interfaces, services…", "deliberate
  unknowns") and §3 (`HT.Ui` holds "authentication/authorization integration"); `CLAUDE.md` "Source of truth"
  ("Do not pick an answer silently"); `docs/roadmap.md` lines 68–69 ("Where the account model lives, and how the
  acting user reaches Application use cases, is still open … That answer also decides how much of this slice AC
  tests can reach"), lines 157–158 (both rows `open`), line 160 (`PageSignIn` "says 'Built on ASP.NET Core
  Identity pages'; if the decision differs, `design/` changes first"); `design/pages/PageSignIn.md` line 8.
- **Continues**: MAJOR-2
- **Reasoning**: The roadmap makes the reach of AC tests a result of the open account-model decision. The spec
  reverses this: it declares registration, sign-in, the block, password change, administrator creation and demo
  loading to be Application use cases, and requires the pending ADR to keep them there. That rules out the
  alternative in which the host handles sign-in, which is the one `design/pages/PageSignIn.md` names and the
  roadmap row points to. `## Clarifications` has no entry for this choice. The checklist lists the owner answers
  added in this revision (lines 37–39) and this is not among them; the split is recorded only as the author's
  response to the critic (line 55). The roadmap still describes the question as undecided (lines 68–69), so the
  two documents now disagree, and "Design changes before build" does not list the `PageSignIn.md` sentence the
  choice conflicts with. The choice also has a consequence the spec does not address. Administrator creation
  and demo loading become use cases on the public Application surface that take no acting user. FR-010 and
  FR-011 cover only "use cases that serve a signed-in user", and FR-004 allows the administrator role to be
  given by "administrator creation at startup". "At startup" is not observable on the AC surface, where a test
  calls the use case at any time (ADR 0003 lines 44–47), so FR-004 and US3-AS6 ("no capability lets an account
  give itself or another account the administrator role") do not say what stops any other caller of that use
  case.

### MINOR-1: HP-008 contradicts HP-009 for the signed-in Home

- **Where**: `spec.md` HP-008 (lines 412–413), HP-009 (lines 414–416), SC-003 (lines 465–466), Edge Cases
  lines 324–325.
- **Against**: internal consistency; `checklists/requirements.md` line 64 ("MINOR-5 | confirmed | HP-009: one Home
  address").
- **Continues**: MINOR-5
- **Reasoning**: HP-009 says Home has one address and shows the visitor Home to an anonymous visitor. HP-008 lists
  the signed-in Home among the pages that "MUST send an anonymous visitor to Sign in". Both cannot hold for Home:
  under HP-009 an anonymous visitor can never open the signed-in Home. The Back-button edge case and SC-003 cite
  HP-008, so for Home it is unclear whether the expected result after sign-out is the visitor Home or the Sign in
  page.

### MINOR-2: Demonstration accounts: nothing identifies a demo account, and nothing can observe them

- **Where**: `spec.md` FR-016 (lines 389–393), US7-AS1–AS5 (lines 301–312), SC-006 (lines 472–473), Key Entities
  (lines 451–453), Edge Cases lines 322 and 326.
- **Against**: `spec.md` FR-002, FR-012 (lines 365–367), FR-014 fourth bullet (lines 379–380);
  `acceptance-template.md` lines 13–14 and `speckit.ac.translate.md` step 6 ("concrete literal values only");
  `SPEC.md` §10; constitution VI (lines 124–129).
- **Continues**: MINOR-1
- **Reasoning**: FR-016 treats two cases differently: a demo account that was loaded before (reload "adds nothing",
  US7-AS2) and "a non-demo account" that holds a demo email (left unchanged "and reported", US7-AS3). Both are
  registered-user accounts with the same email, and the Account entity holds nothing that marks an account as a
  demo account, so the spec gives no basis for telling them apart; it also does not say whether a reload reports
  anything. US7-AS1, US7-AS5 and SC-006 ("exactly the same accounts") require knowing which accounts exist, but
  FR-012 forbids any use case that returns another account's details, and the spec names no demo email, so an AC
  item for them has no literal value to sign in with or compare. FR-002 applies the password rule to demo
  accounts, and FR-014 covers a configured administrator password that fails it; FR-016 and US7 cover only a
  missing demo password. Reload keeps a changed demo password (US7-AS2) and there is no recovery (line 326), so
  one reviewer can make a shared demo account unusable for the others; the edge cases accept the block and the
  forgotten password "for the demo" but not this case.

### MINOR-3: FR-010 and parts of FR-003, FR-014 and FR-016 are not behaviour that a use case can show

- **Where**: `spec.md` FR-010 (lines 362–363), FR-014 (lines 374–383), FR-016 (line 390), FR-003 (line 341);
  US3-AS2 and US3-AS3; `checklists/requirements.md` lines 17, 27 and 41–43.
- **Against**: `spec.md` lines 59–62 (every FR "MUST be reachable through the Application use cases");
  `acceptance-template.md` line 18 ("an FR cannot be marked 'not testable'"); ADR 0003 lines 27, 32 and 44–47;
  `ARCHITECTURE.md` §3 (`HT.Ui` is the composition root).
- **Continues**: MAJOR-1
- **Reasoning**: FR-010's first sentence describes the shape of the use cases (constitution III restated), not an
  outcome with values. Its second sentence ("established only by signing in, never by a value the visitor types
  or changes") is a property of the host: by the first sentence the caller passes the acting user in, and on the
  AC surface the caller is the test. No scenario exercises FR-010 on its own; "ask who is acting" (US3-AS2,
  US3-AS3) returns what the caller supplied. FR-014 requires the system to "read … from the deployment's secret
  configuration" "at startup" and to "start normally in every case"; configuration and startup belong to the
  host, which AC tests do not build. FR-016's "configuration outside source control" is a property of the
  repository, the kind of rule pass 1 noted for the former FR-030. FR-003's guarantee for "registrations made at
  the same time" has no scenario, and ADR 0003 runs AC tests sequentially. Each of these FRs has other parts that
  an AC item can cover, so the check can pass, but the checklist marks every FR as testable through the use
  cases without this limit.

### MINOR-4: Several results have no literal value, and several scenarios still hold more than one behaviour

- **Where**: `spec.md` US1-AS4 (lines 91–92), FR-003 (line 340), US4-AS3 (lines 199–201), US4-AS4 (lines 202–205),
  US7-AS3, US7-AS4, HP-017 (line 434), US2-AS4 (lines 124–126), US2-AS6 (lines 129–130), US3-AS6
  (lines 172–173); `checklists/requirements.md` line 70 ("every scenario holds one behaviour").
- **Against**: `acceptance-template.md` lines 11–14 ("One item per behaviour", "Every value is a concrete literal");
  `speckit.ac.translate.md` step 5 (sources are only the spec, the constitution and `specInputs`).
- **Continues**: MINOR-9, MINOR-11
- **Reasoning**: Other refusals have a literal text (FR-002, FR-003 taken email, FR-007, FR-013). An invalid email
  only "says that the email is not valid", and the spec gives no rule for what a valid address is. The reports
  to the operator (administrator conflict, "no administrator was created and why", demo collision, missing demo
  password) have no literal content and no stated channel: as an FR the report must be a use-case result, but
  HP-017 groups "operator reports" with logs and error pages. US4-AS4 joins five different causes with five
  different reasons in one scenario; US2-AS4 and US2-AS6 each contain two actions with two outcomes; US3-AS6
  ("any capability of this slice") is a rule over all capabilities, not one action. The checklist states the
  opposite.

### MINOR-5: The 5-minute block has undefined boundaries and no stated source of time

- **Where**: `spec.md` FR-008 (lines 354–356), US2-AS4–AS6 (lines 124–130), Clarifications lines 39–42.
- **Against**: Constitution II (lines 50–51: time enters "through `TimeProvider` or a parameter"; lines 59–60: both
  sides of each boundary of "a time- or threshold-based rule"); ADR 0003 lines 23–27 and 32–33;
  `acceptance-template.md` lines 13–14.
- **Continues**: new
- **Reasoning**: FR-008 does not say whether a sign-in attempt made during the block (with a wrong or the correct
  password) counts as a failure or restarts the 5 minutes, or whether the block has ended at exactly 5 minutes.
  US2-AS5 is not marked *(page)*, so it is expected on the AC surface, but its precondition ("an account whose
  5-minute block has ended") needs the passing of time. The spec does not say that the block is judged against
  a supplied current time, so an AC item can neither give literal times for the two sides of the boundary nor
  reach the state without waiting.

### MINOR-6: "Design changes before build" is incomplete, and one owner answer is not carried into a requirement

- **Where**: `spec.md` "Design changes before build" (lines 436–447), Clarifications lines 43–44 and 51, HP-001
  (lines 397–399), HP-010, HP-014 (lines 428–430), US1-AS5 (lines 93–95); `checklists/requirements.md` line 63
  ("All deviations from the previews are listed").
- **Against**: Constitution IV (lines 87–91); `design/README.md` line 9 (pages.md gives "the structure of each
  page"), lines 30–48 (page list) and line 123; `design/pages.md` lines 14, 29–30 and 72–74;
  `design/pages/PageRegister.html` lines 12–18; `design/components/Notice.md` line 7;
  `design/components/TopNav.html` line 11; decision R7 (`docs/roadmap.md` line 208).
- **Continues**: MINOR-4
- **Reasoning**: The list changes `PageProfile`, `PageHomeSignedIn` and adds `PagePrivacy`, but not
  `design/pages.md`, which states the same structures ("greeting in `heading-1` and today's date"; "Header: large
  avatar, name, email, member since"), nor the page list in `design/README.md`. The owner's answer for a taken
  email includes "with a link to Sign in" (lines 43–44); FR-003, US1-AS2 and HP-001 carry only the message. The
  Register preview shows no error state at all, and no document gives the wording for a repeated password that
  differs, which HP-001 requires next to its field; `PageRegister` is not in the list. The owner's answer says
  the Profile header shows "initials" (line 51) while HP-014 says the first letter of the email; the content of
  the avatar in the navigation (two letters from a name in the preview) is not stated. US1-AS5 keeps the notice
  "You can opt in later in your profile" while HP-014 gives Profile no community section until slice 9; the spec
  applies R7 to links and sections but does not say whether it applies to sentences that point to a capability
  that has not shipped.

### MINOR-7: "At their next request" and "each visit" are undefined for an interactive server page

- **Where**: `spec.md` HP-007 (lines 410–411), US6-AS5 (lines 278–279), HP-004 (lines 404–405).
- **Against**: `ARCHITECTURE.md` §2 (Blazor Interactive Server) and §4 ("session invalidation/revalidation
  requirements need to be covered in implementation planning and tests"); `src/HT.Ui/Program.cs` lines 6–7 and
  24–25.
- **Continues**: new (arises from the text added for MINOR-6)
- **Reasoning**: On an interactive server page, actions on an already open page travel over the existing
  connection and are not new page requests. HP-007 ends other sessions "at their next request", so it is unclear
  whether a page that is already open in the other browser must lose access at the user's next action, or only
  when a new page is loaded. The same holds for what counts as a "visit" that renews a kept session in HP-004.
  ARCHITECTURE §4 names exactly this area as one that needs explicit requirements.

### MINOR-8: SC-005 asks for denials that no request of this slice can produce

- **Where**: `spec.md` SC-005 (lines 469–471), FR-012 (lines 365–367), FR-013, US3-AS4, US3-AS5.
- **Against**: Constitution III (lines 71–72: "another registered user, an administrator, and an anonymous visitor
  are all denied"); `checklists/requirements.md` line 61 ("SC-005 names the use cases under test").
- **Continues**: MINOR-2
- **Reasoning**: SC-005 now names reading account details and changing a password. Both act only on the acting
  user's own account, and FR-012 states that no lookup by identifier exists. Another registered user or an
  administrator therefore has no way to address someone else's account, and there is nothing to deny; only the
  anonymous visitor is refused (FR-011). The spec still does not say what the two other negative tests attempt
  and what result counts as passing.

### MINOR-9: Preconditions that need absent data cannot be arranged through the use cases within one run

- **Where**: `spec.md` US7-AS1 ("an empty system", line 301), US1-AS1, US2-AS2, US4-AS1 ("no account with the …
  email"), and US4-AS1–AS6, which use `admin@example.com` as absent, as administrator and as registered user.
- **Against**: ADR 0003 lines 30–33 ("Fresh schema per run", "Each scenario arranges the data it needs", one
  shared database, sequential) and lines 38–39; `spec.md` FR-001–FR-016 (no capability removes an account).
- **Continues**: new
- **Reasoning**: ADR 0003 rebuilds the schema at the start of a run, not per scenario, and scenarios arrange data
  only through use cases and the two persistence entry points. The slice has no way to remove an account, so
  "an empty system" and "no account with this email" hold only until an earlier scenario of the same run
  creates one. "What acceptance tests can reach" does not mention this, and the scenarios reuse the same
  addresses with opposite preconditions.

### MINOR-10: "Visitor" is still used for any user; "block" and "operator" are new and undefined

- **Where**: `spec.md` US2-AS7 and US2-AS8 (lines 131–134), US5-AS4 and US5-AS5 (lines 236–239), FR-010 (line 363),
  US4 (line 183, "The operator of a deployment"), Key Entities line 452 ("any current block"); `CONTEXT.md`
  lines 28 and 30.
- **Against**: Constitution I (lines 33–41); `SPEC.md` §3 (three audiences) and §8 (deactivated accounts);
  `checklists/requirements.md` line 69 ("loose 'visitor' wording removed from US3").
- **Continues**: MINOR-10
- **Reasoning**: US5-AS4 says "a visitor opens the menu" and expects the items "for their audience", so "visitor"
  there includes signed-in users; FR-010 and US2 use it the same way. The glossary defines only "Anonymous
  visitor". The blocked state of an account is a new state next to the deactivated state of SPEC §8 and has no
  glossary entry with a boundary. "Operator" is an actor outside the three audiences of SPEC §3 and is defined
  nowhere. In the glossary edit made with this spec, "Anonymous visitor" is "NOT a registered user who is signed
  out of one browser", while "Acting user" (line 28) and the US2 Independent Test treat anyone who is not signed
  in as the anonymous visitor.

### MINOR-11: Checklist items and verification rows state more than the spec supports

- **Where**: `checklists/requirements.md` lines 3, 11, 16, 17, 27, and table rows MINOR-4 (line 63) and MINOR-11
  (line 70).
- **Against**: `spec.md` lines 54–66 and 477–489; `docs/roadmap.md` line 144 ("the spec holds no implementation
  detail"); Constitution, Decision Discipline lines 136–137 and 141.
- **Continues**: MINOR-12
- **Reasoning**: "Written for non-technical stakeholders" is ticked without a qualification, while the spec now
  names ADR 0003, `HT.Persistence`, `HT.Ui`, `/speckit-ac-translate`, `acceptance.md`, the integration-test
  database and the test-project layout. "No [NEEDS CLARIFICATION] markers remain" is ticked while five
  ARCHITECTURE §9 decisions are open; they are listed in a section but carry no marker, so the constitution's
  gate on open markers has nothing to act on, and the checklist's stated purpose is validation "before
  proceeding to planning". "Requirements are testable and unambiguous" and "All functional requirements have
  clear acceptance criteria" are ticked despite MINOR-1 to MINOR-5. The verification rows for MINOR-4 and
  MINOR-11 claim that all deviations are listed and that every scenario holds one behaviour; MINOR-6 and
  MINOR-4 of this pass show otherwise.

## Summary

1 MAJOR, 11 MINOR.

The revision removed the blocking problems of pass 1: the FR/HP split lets `check_acceptance.py` pass in
principle, the sign-in particulars are owner answers with literal values, the community state and the
administrator rule are consistent, and the roadmap, README, `CLAUDE.md` and glossary edits match the spec. The
author's reason for leaving the five technical decisions open until just before `/speckit-ac-translate` agrees
with the roadmap and is sound. The remaining MAJOR is that the spec, while calling those decisions open, already
fixes their main consequence (all account behaviour is on the AC surface, as Application use cases) without an
owner answer in `## Clarifications`, against the roadmap text that makes this a result of the open decision and
against the design note for Sign in. The MINOR notes concern a contradiction between HP-008 and HP-009, the
identity and observability of demo accounts, FR parts that no use case can show, missing literal values, the
block's time boundaries, an incomplete list of design changes, wording that does not fit an interactive server
page, and checklist claims that overstate the revision. The spec can go to `/speckit-clarify`; it is not yet a
sufficient source for acceptance criteria.
