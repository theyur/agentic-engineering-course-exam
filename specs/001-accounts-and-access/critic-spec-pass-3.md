# Critic pass 3: spec for 001-accounts-and-access

**Verdict**: APPROVE
**Date**: 2026-11-11
**Reviewed**: `specs/001-accounts-and-access/spec.md`, `specs/001-accounts-and-access/checklists/requirements.md`
(with the related uncommitted edits to `CONTEXT.md`, `docs/roadmap.md`, `README.md`, `CLAUDE.md`)

## Previous notes: resolution status

Status of the pass 2 notes (`critic-spec-pass-2.md`).

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 (AC surface fixed without an owner answer) | Resolved | `spec.md` lines 26–31 record the owner answer, dated, in `## Clarifications`. `docs/roadmap.md` lines 68–70 and 158 now say the same. "Design changes before build" (lines 490–491) lists the `PageSignIn` note "Built on ASP.NET Core Identity pages". FR-004 (lines 380–381) no longer depends on "startup"; lines 69–71 name administrator creation and demo loading as operator capabilities. The ADR for this decision is listed as due (lines 546–547). The author's rationale is sound. A leftover inconsistency in the wording of FR-012 is MINOR-7. |
| MINOR-1 (HP-008 against HP-009) | Resolved | HP-008 (lines 453–454) covers Profile only; the Back-button edge case (lines 360–361) names both outcomes. |
| MINOR-2 (demo accounts: identity and observability) | Partly | FR-016 (lines 428–432) uses three fixed emails and per-email results; US7-AS4 covers a weak demo password; the shared-account risk is accepted (lines 356–358). The fixed emails create a new precondition problem in the shared test database (MINOR-6). |
| MINOR-3 (FR parts that no use case can show) | Resolved | Startup, configuration and concurrency are HP-018–HP-020. Every FR now has at least one behaviour a use case can show. FR-010's second sentence (line 401) and the last sentences of FR-014 and FR-016 still describe shape or pages; see MINOR-9 for the checklist's description of this. |
| MINOR-4 (missing literals; scenarios with several behaviours) | Partly | Invalid email, administrator creation and demo loading have literal results; US2 and US4 scenarios are split; old US3-AS6 is removed. Still open: which result applies when several refusal conditions hold, and the result of FR-011 (MINOR-3). |
| MINOR-5 (block boundaries, source of time) | Resolved | FR-008 (lines 390–394) states both sides of the boundary, the behaviour during the block and the supplied time; US2-AS4–AS6 use literal times. Two clauses of the new text have no scenario that isolates them (MINOR-1). |
| MINOR-6 (incomplete design list; owner answer not carried) | Partly | The list (lines 490–502) adds `PageRegister`, `pages.md`, the README page list and the TopNav avatar letter; HP-001 carries the "Sign in" link; HP-014 defines the avatar letter. The answer for sentences that point to unshipped controls is an assumption, applied to one sentence only (MINOR-5); smaller gaps are in MINOR-8. |
| MINOR-7 ("next request", "visit") | Resolved | Owner answer at lines 51–53; HP-007 (lines 450–452) and US6-AS6 use "new page load" and "within 30 minutes"; HP-004 uses "page load". |
| MINOR-8 (SC-005 denials) | Partly | SC-005 (lines 525–528) says what the tests attempt, and US6-AS5 is a real negative test. The administrator case for password change that SC-005 claims has no scenario (MINOR-2). |
| MINOR-9 (absent-data preconditions) | Partly | Lines 78–80 state that scenarios share one database and need their own emails. The fixed demo emails and the reused `admin@example.com` are not resolved by that sentence (MINOR-6). |
| MINOR-10 ("visitor", "block", "operator") | Resolved | `CONTEXT.md` lines 36 and 39 add "Operator" and "Sign-in block"; line 30 fixes the "Anonymous visitor" boundary. In `spec.md`, "visitor" without "anonymous" remains only in "visitor Home", the navigation variant label (line 459) and "first-time visitor" (SC-001). |
| MINOR-11 (checklist overstates) | Partly | Two items are unticked with reasons (lines 11 and 23) and the qualifications are rewritten (lines 41–56). Some claims still state more than the spec supports (MINOR-9). |

Pass 1 notes: none has regressed. Checked: scenario and FR numbering is sequential and unique (the parser of
`check_acceptance.py` finds 16 FRs and 60 scenarios, 27 of them marked *(page)*); the community state stays out
of the Account entity (lines 506–509); FR-014 is case by case; the email is "not public" with the administrator
question left to slice 11 (lines 560–561); the roadmap, `README.md` line 53 and the `CLAUDE.md` roadmap pointer
match the spec; the ADRs due are listed (lines 546–547).

## Notes

### MINOR-1: No scenario exercises the digit rule on its own, one example is described wrongly, and two clauses of FR-008 have no scenario

- **Where**: `spec.md` US1-AS3 (lines 103–104), US4-AS7 (line 227), US6-AS3 (line 299), US7-AS4 (line 340);
  FR-002 (lines 373–375); FR-008 (lines 392–393); US2-AS4–AS7 (lines 140–149); `checklists/requirements.md`
  line 27 ("All functional requirements have clear acceptance criteria").
- **Against**: `spec.md` FR-002 ("at least 8 characters and contain at least one digit"); FR-008 ("attempts
  neither count as failures nor extend the block. A successful sign-in, and the end of a block, reset the count
  to 0"); `acceptance-template.md` lines 11–14 (one item per behaviour, literal values);
  `speckit.ac.translate.md` step 5 (the spec is the source of AC content).
- **Continues**: MINOR-5 (text added for it); otherwise new
- **Reasoning**: Every refused password in the spec is shorter than 8 characters: `walking` (7), `walk4` (5),
  `curator` (7), `running` (7), `example` (7). US1-AS3 labels `walking` as "(no digit)", but it also fails the
  length rule. No scenario refuses a password of 8 or more characters without a digit, so the second half of
  FR-002 is never shown by itself, and acceptance criteria drafted from these examples cannot tell a
  length-only check from the full rule. In FR-008, "the end of a block reset[s] the count to 0" has no scenario:
  US2-AS5 signs in with the correct password, which resets the count anyway, so nothing shows what one failed
  sign-in after a block has ended does. "Attempts [during the block do not] count as failures" is not isolated
  either: US2-AS6 shows only that the block is not extended.

### MINOR-2: SC-005 claims an administrator negative test for password change that no scenario provides

- **Where**: `spec.md` SC-005 (lines 525–528), US6-AS4 and US6-AS5 (lines 301–307), US3-AS5 (lines 189–191),
  FR-015 (lines 423–424).
- **Against**: Constitution III (lines 71–72: "another registered user, an administrator, and an anonymous
  visitor are all denied"); `spec.md` SC-005 itself.
- **Continues**: MINOR-8
- **Reasoning**: SC-005 states that "another registered user and an administrator … cannot change someone else's
  password, even with that account's correct current password" and cites US3-AS4, US3-AS5 and US6-AS5. US6-AS5
  uses two registered users. US3-AS5 is about reading account details. No scenario has an administrator attempt
  a password change with another account's current password, so for password change the administrator case of
  constitution III, which SC-005 says is covered, has no scenario behind it.

### MINOR-3: "Exactly one result" is not defined when several refusal conditions hold, and one refusal has no literal result

- **Where**: `spec.md` FR-014 (lines 411–419), FR-002 and FR-003 (lines 373–379), FR-013 (lines 405–407), FR-011
  (line 402), US3-AS1 (lines 181–182), US6-AS4 (lines 301–302), US4-AS4 and US4-AS5 (lines 221–224).
- **Against**: `acceptance-template.md` lines 13–14 ("Every value is a concrete literal"); internal consistency
  of FR-014 ("give exactly one of these results").
- **Continues**: MINOR-4
- **Reasoning**: FR-014 promises exactly one of six results, but its conditions are not exclusive and no order is
  given: an existing administrator's email with a password that fails FR-002, a registered user's email with
  such a password, an invalid email together with a missing or weak password. The first of these is a realistic
  configuration. "Missing" is not defined (absent, empty, or only spaces, which FR-003 would trim to nothing).
  The same question exists for registration (a taken email with a weak password; an invalid email with a weak
  password) and for FR-013 (a wrong current password with a new password that fails FR-002): the spec does not
  say whether the use case gives one result or several, while HP-001 places messages next to different fields.
  Every other refusal has a literal text; the refusal of an anonymous visitor (FR-011, US3-AS1, US6-AS4) is
  only "refused because they are not signed in".

### MINOR-4: Three requirements differ from the defaults of the framework that ARCHITECTURE selects, and the spec does not say so

- **Where**: `spec.md` FR-008 (lines 393–394), HP-004 (lines 444–445), US2-AS10 (lines 154–155), HP-007
  (lines 450–452), "Decisions due before acceptance criteria" (lines 539–541).
- **Against**: `ARCHITECTURE.md` §2 ("ASP.NET Core Identity … Use established framework mechanisms rather than
  custom password/session management") and §4; `CLAUDE.md` "Security / privacy boundary" ("Use framework auth
  (Identity); no custom crypto, password storage, or session handling"); `docs/roadmap.md` line 61 (the slice
  brings "ASP.NET Core Identity on SQL Server"); constitution II (lines 50–51); the installed framework
  reference (`Microsoft.AspNetCore.App.Ref` 10.0.14): `Microsoft.AspNetCore.Authentication.Cookies.xml`
  (`SlidingExpiration`), `Microsoft.AspNetCore.Identity.xml` (`SecurityStampValidatorOptions.ValidationInterval`),
  `Microsoft.Extensions.Identity.Core.dll`.
- **Continues**: new
- **Reasoning**: HP-004 and US2-AS10 require each page load to renew a kept session, so that it lasts 14 days
  from the last page load. The framework's sliding expiration re-issues the cookie only for "a request which is
  more than halfway through the expiration window", so with the default behaviour a session can end less than
  14 days after the last page load. HP-007 requires a new sign-in "at once" for any new page load after a
  password change; the framework re-validates sessions on an interval that "defaults to 30 minutes". FR-008
  requires the block to be judged against a supplied current time, and US2-AS4–AS6 need that on the AC surface;
  the framework's lockout lives in `Microsoft.Extensions.Identity.Core`, which contains no reference to
  `TimeProvider` and reads the UTC clock itself. The spec leaves "where the account model lives" to an ADR and
  describes that decision as open, but these three requirements already narrow it, and nothing in the spec or in
  the list of decisions due records that they need behaviour beyond the framework defaults, or how that stands
  with the "framework mechanisms" rule.

### MINOR-5: Owner decisions are extended by the author without a recorded answer, and the rule for unshipped sentences is applied to one sentence only

- **Where**: `spec.md` Assumptions (lines 551–554), HP-011 (lines 462–464), HP-003 (lines 441–443), "Design
  changes before build" (lines 490–493), FR-008 (lines 390–394), FR-003 (lines 376–378), Clarifications lines
  20–22 and 45–48; `checklists/requirements.md` lines 39–40 and table rows MINOR-5 and MINOR-6 (lines 78–79).
- **Against**: Constitution, Decision Discipline (lines 136–139) and principle IV (lines 90–91, pages follow "the
  structure and wording of their previews"); `ARCHITECTURE.md` §9 ("Authentication UX particulars");
  `docs/roadmap.md` line 209 (R7 answer: "only shipped sections and links");
  `design/pages/PageHomeVisitor.html` line 33; `design/pages/PageSignIn.html` line 10.
- **Continues**: MINOR-6; MINOR-5 (text added for it)
- **Reasoning**: The checklist names two owner answers added in this revision (AC surface, other sessions). The
  other additions are the author's: the exact block boundary, that attempts during a block neither count nor
  extend it, that the end of a block resets the count (FR-008), and the rule for a valid email (FR-003). The
  block is an authentication particular that the owner decided with three values (lines 45–48); the added
  details are not in `## Clarifications`. The email rule, read literally, accepts `ana@.` and an address with a
  space before the "@". R7 as answered covers sections and links. Its extension to "sentences that send the user
  to a control that has not shipped" is recorded as an assumption and changes preview wording (the Register
  notice). The criterion is then applied to that one sentence. HP-011 keeps the "How it works" steps "with the
  wording of `PageHomeVisitor`", whose first step reads "Start from the catalog or write your own.", and HP-003
  follows `PageSignIn`, which says "Welcome back. Your goals are waiting."; in this slice neither a catalog nor
  goals exist. The spec does not say whether these sentences fall under the assumption, and they are not in the
  list of design changes.

### MINOR-6: Fixed demo emails and reused example emails leave preconditions that one shared database cannot hold together

- **Where**: `spec.md` "Test data" (lines 78–80), US7-AS1–AS4 (lines 332–341), US4-AS1, US4-AS2, US4-AS5 and
  US4-AS8 (lines 212–231), US3-AS3 and US3-AS5 (lines 185–191); `checklists/requirements.md` line 82
  ("administrator scenarios use distinct emails").
- **Against**: ADR 0003 lines 30–33 (schema rebuilt per run, not per scenario; "Each scenario arranges the data
  it needs") and lines 38–39; `spec.md` line 79 ("nothing in this slice removes an account").
- **Continues**: MINOR-9, MINOR-2
- **Reasoning**: The note says each scenario needs emails of its own and that the demo emails are fixed. For the
  demo emails this leaves the conflict open: US7-AS1 needs "none of the demo emails has an account", US7-AS2
  needs them loaded with one changed password, and US7-AS3 and US7-AS4 expect "no account is created", which
  cannot be observed once any earlier scenario has loaded the same three emails. The spec does not say how these
  preconditions are reached within one run. For the other scenarios the checklist claims distinct administrator
  emails, but `admin@example.com` is "no account" in US4-AS1 and an existing administrator in US4-AS2, US4-AS8,
  US3-AS3 and US3-AS5; US4-AS4 and US4-AS5 expect "no account is created" with nothing that could show it.

### MINOR-7: FR-012 forbids what FR-014 and FR-016 return, and two sentences about who calls the operator capabilities disagree

- **Where**: `spec.md` FR-012 (lines 403–404), FR-014 (lines 414–416, 421–422), FR-016 (lines 428–430), "What
  acceptance tests can reach" (lines 69–71), Assumptions line 561, "Decisions due" line 545.
- **Against**: internal consistency.
- **Continues**: MAJOR-1 (residue)
- **Reasoning**: FR-012 says "No use case of this slice may return another account's details, whatever the acting
  user's role", and defines account details as email and role. Administrator creation answers, for an email the
  caller supplies, "administrator already exists" or "email belongs to a registered user", which is that
  account's role; demo loading answers "already exists" per email. Both are use cases of this slice with no
  acting user. The intent is clear from the edge case at lines 363–364, but the letter of FR-012 excludes these
  results. Lines 69–71 say "the host and the tests call" the operator capabilities; FR-014 says "Only the host
  calls administrator creation"; and for demo loading no requirement has the host call it, since when and how it
  runs is listed as open (line 545).

### MINOR-8: The roadmap's page tracker and the design list miss changes the spec makes, and the change-password form has no page states

- **Where**: `docs/roadmap.md` lines 111–112 ("Pages across slices"); `spec.md` Assumptions lines 552–554,
  "Design changes before build" (lines 490–502), HP-014 (lines 469–472), US6-AS8 (lines 313–315), HP-001
  (lines 436–439).
- **Against**: decision R7 (`docs/roadmap.md` line 209: "The full preview is reached when the last extending
  slice ships"); constitution IV (lines 87–91); `design/components/MobileNav.html` line 8 (avatar "YD");
  `design/pages/PageProfile.html` (only the Community tab is drawn).
- **Continues**: MINOR-6
- **Reasoning**: The spec removes "You can opt in later in your profile" from Register "until slice 9", but the
  roadmap row for `PageSignIn`, `PageRegister` lists only slice 11 as extending them, so nothing tracks the
  return of that sentence. The same table has no row for the new Privacy page, and its Shell row still says
  "'Privacy' has no page design yet". HP-014 changes the avatar content "in the navigation"; the list of design
  changes names `TopNav` and `PageProfile` but not `MobileNav`, whose preview shows the two-letter avatar. For
  Register, HP-001 gives each error its text and place. For the change-password form, HP-014 lists the fields
  only: there is no text for a repeated new password that differs, no statement of where the two refusals of
  FR-013 appear, and nothing on what the user sees after a successful change; US6 has no *(page)* scenario for
  these states, and the Account tab has no preview to take them from.

### MINOR-9: Checklist items and verification rows still state more than the spec supports

- **Where**: `checklists/requirements.md` lines 9, 16, 17, 27, 30, qualifications lines 45–47 and 53–55, table
  rows MINOR-3 (line 76) and MINOR-9 (line 82).
- **Against**: `spec.md` lines 63–80 and 534–547; Constitution, Decision Discipline lines 136–137 and 141;
  `spec.md` FR-010 (lines 400–401).
- **Continues**: MINOR-11
- **Reasoning**: "Written for non-technical stakeholders" is unticked because two sections name ADR 0003, project
  names and Spec Kit commands, yet both "no implementation details" items (lines 9 and 30) stay ticked, and
  their qualification does not mention those sections. The qualification for "No markers" says the open
  ARCHITECTURE §9 decisions "do not change what the requirements say"; the constitution's rule is that a spec
  that touches such a choice marks it, and one of the open decisions (when and how demo loading runs) is a
  missing requirement, not only a technical detail. The deviation is acknowledged here but not in the spec. Row
  MINOR-3 says FR-010 is "restated as observable behaviour" while line 54 says it "restates constitution III",
  and its second sentence is about what the use cases take as input. Row MINOR-9 claims distinct administrator
  emails (see MINOR-6). "Requirements are testable and unambiguous" and "All functional requirements have clear
  acceptance criteria" are ticked despite MINOR-1 and MINOR-3.

## Summary

0 MAJOR, 9 MINOR.

The blocking note of pass 2 is resolved: the reach of acceptance tests is now an owner answer in
`## Clarifications`, the roadmap agrees with it, the design note it conflicts with is scheduled for change, and
the operator capabilities are separated from what visitors and signed-in users can do. The FR/HP split holds
under the parser of `check_acceptance.py` (16 FRs, 60 scenarios, 27 marked *(page)*), the scenarios carry
literal values, and the quoted design wording matches the previews. The remaining notes are gaps in scenario
coverage (the digit rule, two clauses of the block, the administrator case of SC-005), undefined result order
when several refusals apply, three requirements that the framework defaults do not meet as they stand, author
extensions of owner decisions, test-data preconditions for the fixed demo emails, and tracking and checklist
inaccuracies. None of them prevents `/speckit-clarify`. The spec itself lists five technical decisions that are
still open and must be answered before `/speckit-ac-translate`; MINOR-1, MINOR-3 and MINOR-6 affect the
acceptance criteria that step will draft.
