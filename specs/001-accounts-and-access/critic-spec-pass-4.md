# Critic pass 4: spec for 001-accounts-and-access

**Verdict**: APPROVE
**Date**: 2026-11-11
**Reviewed**: `specs/001-accounts-and-access/spec.md`, `specs/001-accounts-and-access/checklists/requirements.md`
(with the ADRs the spec links as the detail of its clarifications, `docs/adr/0004` to `0009`, and the related
uncommitted edits to `docs/roadmap.md`)

## Previous notes: resolution status

Status of the pass 3 notes (`critic-spec-pass-3.md`).

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MINOR-1 (digit rule never shown alone; two FR-008 clauses without a scenario) | Resolved | US1-AS3 (`walk4`, length only) and US1-AS4 (`walkingfar`, digit only) separate the two halves of FR-002; the other weak examples are 8+ characters without a digit (`curatorship`, `runningfar`, `examplepass`). FR-008 (lines 469–473) no longer says attempts "do not count"; US2-AS7 (lines 200–202) shows the reset at the end of a block: without the reset, the first failure after 10:05:00 would start a new block and the sign-in at 10:06:00 would fail. The author's reason for dropping "neither count" (not observable once the end of a block resets the count) is sound. |
| MINOR-2 (SC-005 administrator case for password change) | Resolved | US6-AS7 (lines 373–377) has an administrator give another account's current password; SC-005 (lines 619–622) cites US6-AS6 and US6-AS7. |
| MINOR-3 (result order; missing literal) | Resolved | Order stated in FR-003 (lines 457–458), FR-013 (lines 487–489) and FR-014 (lines 493–503), with "missing" defined (line 495); US1-AS6, US4-AS8 and US6-AS4 show the order; FR-011 has the literal "You are not signed in." (lines 482–483). |
| MINOR-4 (three requirements differ from framework defaults) | Resolved | Owner answers at lines 47–48 (framework sliding renewal; HP-004 and US2-AS11 now say "at least 7 and at most 14 days" and "within 7 days") and lines 53–55 (the block is the app's own rule). Revalidation on every page load is recorded in ADR 0004 (lines 63–66). Two further framework defaults that the same ADR leaves unstated are MINOR-2 and MINOR-3 below. |
| MINOR-5 (author extensions of owner decisions; unshipped sentences) | Resolved | Block details and the email rule are recorded as owner answers (lines 53–61). R7 is limited to sections and links (lines 23–25, ADR 0008 lines 30–32), so no sentence is reworded and the Register notice keeps its preview wording (HP-001; `design/pages/PageRegister.html` line 15). |
| MINOR-6 (fixed demo emails and reused example emails) | Resolved | Apart from the demo emails, the only email used by two scenarios is `ana@example.com` (US2-AS1 and the *(page)* scenario US6-AS11). The order dependence of US7-AS1 to AS4 is stated at lines 120–124. "Nothing is created" is shown by a later registration (US4-AS5, US4-AS7); for US7-AS3 and US7-AS4 it can be shown by the "created" results of US7-AS1 in the sequence that line 123 allows. |
| MINOR-7 (FR-012 against operator results; who calls the operator capabilities) | Resolved | FR-012 (lines 484–485) is limited to use cases for signed-in users. Who runs the operator capabilities is stated at lines 111–113 and agrees with FR-014 (line 506), HP-018, HP-021 and ADR 0005 (lines 45–47). |
| MINOR-8 (page tracker, design list, change-password states) | Resolved | `docs/roadmap.md` lines 111–113 (Shell row, `PagePrivacy` row); `MobileNav` in the design list (spec line 591); HP-014 (lines 557–562) and US6-AS9 give the change-password states. |
| MINOR-9 (checklist states more than the spec supports) | Partly | Both "no implementation details" items are unticked (lines 9 and 30) and the two verification rows are corrected. The qualifications are out of date again after this iteration: see MINOR-1. |

Older notes: none has regressed. Checked: FR, HP and scenario numbering is sequential and unique (the regular
expressions of `check_acceptance.py` find 16 FRs and 68 scenarios, 29 of them marked *(page)*; 21 HPs); every
`FR-NNN`, `HP-NNN` and `US<N>-AS<M>` reference in the spec points to an existing id; the community state stays
out of the Account entity (lines 600–603); FR-014 is case by case; the AC-surface owner answer (lines 29–34)
stands; the decisions that pass 1 MAJOR-2 and pass 3's summary listed as due before `/speckit-ac-translate`
are all recorded as decided, in the spec (lines 633–646) and in `docs/roadmap.md` (lines 159–166); the quoted
design wording still matches `design/pages/PageSignIn`, `PageRegister`, `PageHomeVisitor` and
`design/components/Footer`.

## Notes

### MINOR-1: The checklist ticks "Dependencies and assumptions identified" while its own text still says the decisions are open

- **Where**: `checklists/requirements.md` line 23 ("- [x] Dependencies and assumptions identified — open
  decisions remain (see qualifications)"), line 16 with qualification lines 49–52, qualification line 61,
  qualification lines 45–48, Notes lines 34–43.
- **Against**: `spec.md` lines 628–646 ("Decided, with their ADRs") and lines 75–102; `docs/roadmap.md` lines
  159–165 (status `decided`); Constitution, Decision Discipline (lines 138–139: the answer comes from the owner
  and is recorded with its date).
- **Continues**: pass 3 MINOR-9 (pass 2 MINOR-11)
- **Reasoning**: The item was ticked in this iteration, but its annotation still reads "open decisions remain",
  and the qualification it points to says "the technical decisions the roadmap places before
  `/speckit-ac-translate` are still open" (line 61). The "No markers" qualification (lines 49–52) says the open
  ARCHITECTURE §9 decisions "must be answered before `/speckit-ac-translate`" and that "when and how demo loading
  runs is one of them"; the spec now answers all of them, including demo loading (HP-021). The qualification for
  the two unticked "no implementation details" items names two sections; `## Clarifications` now also names
  application services, a mediator, a store on the framework's user records, an integration-test project, CI
  and a SQL Server instance (lines 75–96), and is not mentioned. The Notes record, for every earlier revision,
  which answers came from the owner; there is no entry for this iteration, although it adds five clarification
  lines and six ADRs. The spec says each decision was "decided with the owner" (line 630); the checklist, which
  carried that evidence so far, does not show it for these.

### MINOR-2: ADR 0004 turns off one Identity default and is silent on two others that refuse values the spec accepts

- **Where**: `docs/adr/0004-account-model.md` lines 51–59 (Persistence: "implements the store with Identity's
  user manager", "the user name is the account's normalized email", "Identity's lockout is turned off");
  `spec.md` lines 75–79 (the clarification that links this ADR as its detail).
- **Against**: `spec.md` FR-002 (lines 450–452) and FR-003 (lines 453–458); the passwords of US1-AS1
  (`walking42`), US4-AS1 (`curator2026`) and US7-AS1 (`example2026`); ADR 0005 lines 50–52 (every expected
  refusal is a named outcome with the spec's text); the installed framework reference
  (`Microsoft.AspNetCore.App.Ref` 10.0.14, `Microsoft.Extensions.Identity.Core.xml`):
  `PasswordOptions.RequireUppercase` and `RequireNonAlphanumeric` "Defaults to true", `RequiredLength`
  "Defaults to 6", `UserOptions.AllowedUserNameCharacters` "Defaults to
  abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+".
- **Continues**: pass 3 MINOR-4 (same class: framework defaults that do not meet the requirements)
- **Reasoning**: The ADR lists what it changes in Identity (lockout off, security stamp renewed, role tables
  unused). It does not mention Identity's own password rule or user-name rule, which the user manager applies
  when it creates a user or sets a password. With the defaults, every password the spec uses as a valid example
  is refused (no upper-case letter, no non-alphanumeric character), and a 6-character password with all four
  character classes passes Identity's rule while FR-002 refuses it. Because the user name is the email, an email
  that FR-003 accepts but that contains a character outside the default list (for example an apostrophe) is
  refused by Identity with an error that is none of the results FR-003 allows. The ADR presents itself as the
  detail of how the domain rules sit on Identity's user records, and for these two rules it does not say which
  side decides.

### MINOR-3: HP-007 keeps the session that changed the password signed in; ADR 0004's mechanism ends it unless something the ADR does not name happens

- **Where**: `spec.md` HP-007 (lines 538–540, "The session that made the change stays signed in"), US6-AS8 and
  US6-AS9 (lines 378–383); `docs/adr/0004-account-model.md` line 59 ("a password change also renews the user's
  security stamp"), lines 60–66 (Host, Session revalidation).
- **Against**: `ARCHITECTURE.md` §2 (Blazor Interactive Server) and §6 ("UI must not depend on persistence
  types"); `spec.md` lines 31–32 ("The host keeps only the browser session"); the installed framework reference
  (`Microsoft.AspNetCore.Identity.xml`): `SecurityStampValidator.ValidateAsync` "rebuilds the identity if the
  validation succeeds, otherwise rejects the identity"; `SignInManager.RefreshSignInAsync` ("Signs in the
  specified user, whilst preserving the existing AuthenticationProperties"); the existing test
  `tests/HT.ArchitectureTests/UiDependencyTests.cs` (covers only `HT.Ui.Components`).
- **Continues**: new
- **Reasoning**: The ADR explains how other sessions end: the stamp is renewed and the host validates it on
  every page load. The session that made the change holds the old stamp too, so the same check rejects it at
  its next page load unless its cookie is issued again in the HTTP exchange that changes the password. The ADR
  says nothing about this half of HP-007, and it matters for the page design: an interactive server component
  handles the form over the circuit, where no response exists to carry a new cookie. The ADR also does not say
  where the stamp comparison lives. The domain account has no stamp (lines 37–39), the store examples have none
  (lines 47–48), and the host "validates the security stamp"; that leaves host code working with the Identity
  user type that the ADR places in `HT.Persistence`, next to an ARCHITECTURE rule and an architecture test that
  keep UI code away from persistence types. The spec lists this decision as settled (lines 634–636).

### MINOR-4: The startup tasks need a schema, how a local database gets one is assigned to nobody, and ADR 0007 adds a host rule the spec does not have

- **Where**: `spec.md` HP-018 (lines 568–570), HP-021 (lines 575–577), US4-AS10 (lines 294–296), US7-AS6
  (lines 419–421), the clarification at lines 97–102; `docs/adr/0007-startup-operator-tasks.md` lines 36–41.
- **Against**: `ARCHITECTURE.md` §5 ("migration rollout procedures are future design work") and §9;
  `docs/roadmap.md` line 174 ("Schema migration rollout on each deployment", `demo-hosting`, open) and line 62
  (slice 1 brings "the first migration"); `CLAUDE.md` "Source of truth" (deferred choices are not picked
  silently); `spec.md` SC-001.
- **Continues**: new
- **Reasoning**: ADR 0007 says "Both tasks need the schema in place. Applying migrations is not part of this
  decision; for the hosted app it is slice 3's." For the local database of this slice no document says who
  applies the first migration, or when, yet HP-018 and HP-021 run account lookups "at every start", before the
  host serves requests. The ADR then adds: "An unexpected fault in either task is logged without secrets and
  does not stop the start." HP-018 and HP-021 only say the host starts normally "whatever the result", and a
  fault is none of the results FR-014 and FR-016 define. So the behaviour for the most likely first start (no
  schema yet) exists only in the ADR, and by that rule the host serves pages against a database on which
  registration cannot work. The spec calls this decision settled (lines 641–642), while the plan still has to
  choose how a local database reaches its schema, which is a deferred choice with no owning row for the local
  case.

### MINOR-5: The test-project layout is recorded as decided, but it has no place for the tests most HP requirements need

- **Where**: `spec.md` lines 90–96 and 639–640 (layout decided, ADR 0006), lines 114–116 (HP-NNN "are verified
  by unit, integration and selective UI tests"); `docs/adr/0006-integration-tests.md` lines 29–31 and 44–50.
- **Against**: `spec.md` HP-018, HP-021, HP-017 (host start, operator log), HP-001 to HP-016 and the 29
  scenarios marked *(page)*; `CLAUDE.md` "Testing" ("A feature is not done without tests"); `docs/roadmap.md`
  lines 152–153 and 192–193 (UI-test library: "the first slice that needs one", open).
- **Continues**: new
- **Reasoning**: In ADR 0006 the unit-test project covers "Domain and Application rules, without a database",
  and the integration-test project builds the application "without `HT.Ui`". Neither can run the host, so
  neither can verify that the host runs the two operator tasks at start, writes their results to the operator
  log and starts whatever the result (HP-018, HP-021, US4-AS10, US7-AS6), nor any page requirement. The ADR
  defers "where selective UI tests go" to the moment "a plan first needs one". This slice's plan needs it at
  once: 21 HP requirements and 29 scenarios are outside the AC surface, and the spec names exactly these test
  types as their verification. The roadmap's procedure for that moment exists (stop, ask the owner, confirm as
  spec drift), so nothing is blocked, but the spec presents the layout as settled while the part this slice
  depends on is still open.

### MINOR-6: Statements in the spec, the roadmap and the new ADRs that are stale or disagree with each other

- **Where**: `spec.md` lines 32–34, 75–102, 630–631, 641–642, Edge Cases lines 433–435; `docs/roadmap.md` lines
  68–70 and 142–146; `docs/adr/0007-startup-operator-tasks.md` lines 55–56;
  `docs/adr/0009-no-outbound-email.md` lines 12 and 31–32.
- **Against**: `docs/roadmap.md` lines 159–160 (both rows `decided`) and line 176 ("How reviewers get demo
  sign-ins", `demo-hosting`, open); `design/README.md` line 123 and `design/pages/PageSignIn.html` line 14
  (current state of the design); internal consistency.
- **Continues**: new
- **Reasoning**: Four small disagreements, none of which changes a requirement.
  (1) The roadmap's slice note 1 still says the account model and the way the acting user reaches the use cases
  are "still open (see the decision table)", while the table in the same file marks both rows decided; the spec
  keeps the matching sentence "stays for the ADR" (lines 32–34) above the lines that record the answer.
  (2) The roadmap's procedure for technical rows is "one dated line … that names the decision and links the ADR;
  the detail stays in the ADR, since the spec holds no implementation detail", and the spec repeats "the detail
  stays in the ADR" (lines 630–631). The new clarification lines restate the detail instead (for integration
  tests: project, databases, CI instance, schema rebuild, sequential runs), so the same decisions now have two
  texts that must stay in step.
  (3) The spec says each decision is recorded in a clarification line "that links its ADR" and lists
  "administrator creation at startup" under ADR 0007; the clarification lines about the first administrator
  (lines 35–41) carry no link.
  (4) The spec accepts shared demo accounts "for the local demo" and leaves hosted sign-in to slice 3; ADR 0007
  states "Reviewers share the demo accounts" and "this is accepted for the demo", citing that edge case, while
  the roadmap row on how reviewers get demo sign-ins is open. ADR 0009 describes two design changes as already
  made ("showed", "no longer lists them"); `design/` still shows "Forgot password?" and still lists "password
  reset pages", and the spec schedules both changes under "Design changes before build".

## Summary

0 MAJOR, 6 MINOR.

All nine notes of pass 3 are resolved in the spec; only the checklist has fallen behind again. The material
added in this iteration is consistent where it matters for the next step: the seven clarification lines agree
with ADRs 0004 to 0009, HP-021 and US7-AS6 match the demo-loading clarification and ADR 0007, "What acceptance
tests can reach" states once who runs the operator capabilities, and every decision the roadmap places before
`/speckit-ac-translate` is recorded as decided in both the spec and the roadmap. The spec itself carries what
acceptance criteria need (explicit acting user, named outcomes with literal text, supplied time), which matters
because ADRs 0004 to 0009 are not among the `specInputs` the AC agents can read. The parser of
`check_acceptance.py` still finds 16 FRs and 68 scenarios with sequential numbering. The remaining notes are a
checklist that contradicts the spec, three gaps in the new ADRs that the plan will meet (Identity's own
password and user-name rules, the current session after a password change, the schema at the first local
start), a test-project layout that defers the part this slice needs, and stale or diverging sentences across
the spec, the roadmap and the ADRs. None of them blocks `/speckit-ac-translate`; MINOR-2 to MINOR-5 affect
`/speckit-plan`.
