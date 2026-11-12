# Critic pass 1: spec for 001-accounts-and-access

**Verdict**: REVISE
**Date**: 2026-11-11
**Reviewed**: `specs/001-accounts-and-access/spec.md`, `specs/001-accounts-and-access/checklists/requirements.md`

## Notes

### MAJOR-1: Thirteen functional requirements have no behaviour on the AC surface, and the next step rejects that

- **Where**: `spec.md` FR-006, FR-011, FR-012, FR-019, FR-020, FR-021, FR-022, FR-023, FR-025, FR-026, FR-027,
  FR-030, FR-031 (lines 266–341); partly FR-002 (help text), FR-005 ("show the signed-in Home"), FR-010 (browser
  session, visitor Home), FR-015 (page redirect), FR-024 (header). `checklists/requirements.md` line 17
  ("Requirements are testable and unambiguous") and line 27 ("All functional requirements have clear acceptance
  criteria").
- **Against**: `docs/adr/0003-acceptance-tests.md` lines 23–27 ("`HT.Ui` is a host and is not tested by AC tests")
  and 38–39 (assert only through Application use cases and the two `HT.Persistence` entry points);
  `.specify/extensions/ac/templates/acceptance-template.md` line 18 ("every FR-NNN needs at least one item; an FR
  cannot be marked 'not testable' (if it cannot be tested through the public API, the spec or the API must
  change)"); `.specify/extensions/ac/scripts/check_acceptance.py` lines 140–142 (an uncovered FR is a failure;
  only scenarios may be listed as not testable); `.specify/extensions/ac/commands/speckit.ac.translate.md` step 5
  (lines 66–69: the only AC sources are `spec.md`, the constitution and `specInputs`) and step 8 (the check must
  pass); `docs/roadmap.md` lines 48–49 and 145–147.
- **Reasoning**: The flow after this spec is `/speckit-clarify` then `/speckit-ac-translate`. That command needs an
  AC item, with concrete literal values and reachable through the public API, for every `FR-NNN`. The listed FRs
  describe only what pages and the shell render (FR-006, FR-019–FR-023, FR-026, FR-027, FR-031), browser
  navigation and session persistence (FR-011, FR-012), or the order of edits to repository files and their content
  (FR-011 last sentence, FR-022 last sentence, FR-025, FR-030). None of these can be observed through an
  Application use case, and three of them (FR-025, FR-030, the `design/` sentences of FR-011 and FR-022) are not
  system behaviour at all. Several also take their wording from `design/` ("with the wording of
  `design/pages/PageHomeVisitor`", "follow the structure and wording of `design/pages/PageSignIn`"), which is not
  an allowed AC source, so no literal values are available to an AC drafter. As written, `check_acceptance.py`
  cannot pass for this spec. The spec also does not state which of its behaviour is on the AC surface, although
  the roadmap asks a spec to record when it finds none (lines 48–49); the checklist marks testability as passed
  without mentioning the AC surface.

### MAJOR-2: Decisions the roadmap requires before `/speckit-ac-translate` are assigned to `/speckit-plan`

- **Where**: `spec.md` Assumptions, lines 390–392 ("The account model's location, how the acting user reaches use
  cases, the use-case convention, the integration-test database, the test-project layout and the demo-data
  seeding mechanism are technical decisions for `/speckit-plan` and its ADRs (roadmap decision table)");
  `checklists/requirements.md` lines 41–43.
- **Against**: `docs/roadmap.md` lines 128–131 ("Each decision is settled once, in the slice named here, before
  that slice's acceptance criteria are drafted (`/speckit-ac-translate`)"), lines 140–149 ("Technical rows …
  Decide them with the owner and write an ADR … Before `/speckit-ac-translate`, add one dated line to the spec's
  `## Clarifications` that names the decision and links the ADR"), the five rows still `open` for
  `accounts-and-access` (lines 157, 158, 161–163), slice note 1 lines 68–69 ("That answer also decides how much of
  this slice AC tests can reach"); `CLAUDE.md` "Acceptance tests" flow (specify → clarify → ac-translate → plan).
- **Reasoning**: The spec cites the roadmap decision table as the basis for leaving these five decisions to the
  plan, but the table's own rules place them before acceptance criteria are drafted, and the plan comes after
  `/speckit-ac-translate` in the project flow. `## Clarifications` has no line for any of them. Two of them (where
  the account model lives and how the acting user reaches use cases; the use-case convention) decide whether
  registration, sign-in, lockout, the acting-user role and the administrator bootstrap (FR-001–FR-009, FR-013–
  FR-018) are Application use cases or host behaviour. Until they are answered, an AC drafter cannot tell which
  scenarios are reachable. FR-014 and Key Entities ("It comes from the signed-in session only", line 351) add to
  this: ADR 0003 has AC tests build the application without `HT.Ui`, where no signed-in session exists, so the
  literal requirement cannot be exercised on the AC surface. The assumption also shortens the roadmap row: the
  row covers "the acting user, the administrator role and the deactivated state" (line 157); the spec names only
  the acting user.

### MAJOR-3: Lockout and other sign-in particulars are assumed, not decided, and their values arrive too late for acceptance criteria

- **Where**: `spec.md` FR-009 (lines 274–275), US2 scenario 5 (lines 97–98), Edge Cases line 243–244, FR-011
  "for a limited period" (line 280), Assumptions lines 376–379 and 383; `checklists/requirements.md` line 16
  ("No [NEEDS CLARIFICATION] markers remain") and lines 39–40.
- **Against**: Constitution, Decision Discipline (lines 136–139: a spec that touches an ARCHITECTURE §9 deferred
  choice "MUST mark it … instead of assuming an answer"; "The answer comes from the project owner … recorded in
  the spec's `## Clarifications`"); `ARCHITECTURE.md` §9 ("Authentication UX particulars … deliberate unknowns,
  not invitations for an implementation agent to silently choose them"); `CLAUDE.md` "Source of truth";
  `docs/roadmap.md` lines 135–139 (rows beyond the three markers "are asked in conversation and recorded the same
  way") and line 160 (the decided row lists only email verification, password recovery, outbound email and "keep
  me signed in"); `speckit.ac.translate.md` step 6 ("concrete literal values only") and step 5 (never `plan.md`).
- **Reasoning**: The owner's recorded answer on authentication UX (`spec.md` lines 27–30) does not mention a
  temporary block after failed sign-ins, how long a kept session lasts, or whether registration may disclose that
  an email already has an account. All three are authentication UX particulars. The spec introduces the first as
  a MUST (FR-009) and settles all three under Assumptions ("following the framework's standard lockout", "using
  the framework's standard persistent session", "is acceptable"), which is the pattern Decision Discipline
  forbids. Nothing in SPEC, ARCHITECTURE or `design/` asks for lockout; `design/pages/PageSignIn` shows no blocked
  state. Separately, the spec says "the exact numbers are fixed in the plan", but acceptance criteria are drafted
  before the plan, only from the spec, and need literal values. "Fails repeatedly in a row" and "temporarily
  blocked" therefore cannot be turned into an AC item for FR-009, which (MAJOR-1) must have one.

### MAJOR-4: The account's community state is both in scope and out of scope, and FR-004 / FR-028 cannot be observed

- **Where**: `spec.md` Key Entities lines 345–347 (Account "Holds … its community state (participation off, no
  public display name). Later slices add … community consent (slice 9)"); Assumptions line 389 ("Community consent
  and the public display name are out of scope (slice 9)"); FR-004 (lines 262–263); FR-028 (lines 336–337); US1
  Independent Test (lines 51–53) and scenario 2 (lines 60–61, "When anyone looks at its community state"); US6
  scenario 1 (lines 226–227).
- **Against**: `SPEC.md` §7.1; constitution VI (line 128–129, demo data obeys the consent rules); `docs/roadmap.md`
  slice 9 row and slice note 9 ("Consent and the public display name live on the Profile 'Community' tab");
  `acceptance-template.md` line 18; `spec.md` FR-024 (Profile shows only the Account tab).
- **Reasoning**: One reading has slice 1 store and expose a community-participation state and a public display
  name per account (Key Entities, FR-004, FR-028). The other has both arrive in slice 9 (Assumptions, the second
  sentence of the same Key Entities bullet). Two implementers can build different things from this. Under the
  second reading, "community participation is off" is true only because the concept does not exist, and the
  required confirmations (US1 Independent Test, US1 scenario 2, US6 scenario 1) have nothing to look at: no page,
  tab or capability of this slice shows community state, and scenario 2 names neither an actor nor a place. Under
  the first reading the slice builds part of slice 9's consent model, which the Assumptions exclude. Either way
  FR-004 and FR-028 need AC items through the public API (MAJOR-1) and the spec gives no observable for them.

### MAJOR-5: FR-016 contradicts itself and US3 scenario 7 on when an administrator is created

- **Where**: `spec.md` FR-016 (lines 294–299), US3 scenario 7 (lines 135–137), Edge Cases lines 245–247.
- **Against**: internal consistency; `SPEC.md` §3 and §8 and `design/pages/PageSignIn.md` line 7 (all speak of
  "administrators" in the plural); `ARCHITECTURE.md` §4 (authorization distinguishes administrators).
- **Reasoning**: FR-016's first sentence makes creation depend on the email: create "if no account with the
  configured administrator email exists yet". Its second sentence forbids the outcome regardless of email: "MUST
  NOT create a second administrator". US3 scenario 7 uses a third condition: "a system with no administrator".
  These agree only while the configured email never changes. If an administrator exists and the configured email
  is later changed to an unused address, the first sentence requires a new administrator account and the second
  forbids it. The Edge Cases cover a changed configured password (lines 246–247) but not a changed configured
  email. The edge case "The administrator role belongs to one account" (line 245) can be read as "one
  administrator in the whole system" or as "one account per person"; the spec nowhere states that version 1 has
  at most one administrator, although FR-016 and FR-017 together leave no other way to get one, while SPEC and
  the design refer to administrators in the plural.

### MINOR-1: Demonstration accounts: no credential source in this slice, and shared-account cases are not covered

- **Where**: `spec.md` US6 (lines 213–231), FR-028–FR-030 (lines 336–341), Assumptions lines 384–385 and 393.
- **Against**: constitution VI (lines 124–129); `SPEC.md` §10; `docs/roadmap.md` rows "Re-runnable demo-data
  seeding" (line 163) and "How reviewers get demo sign-ins" (line 174); `spec.md` FR-009, FR-011, FR-024, US3
  scenario 8.
- **Reasoning**: FR-030 forbids demo passwords in source control and defers the hosted answer to slice 3, but the
  US6 Independent Test requires signing in as a demo account in this slice. For the administrator the spec names a
  source ("the deployment's secret configuration", locally "the developer's secret store"); for demo accounts it
  names none, and says nothing about loading demo data when no password is available. "Existing demo accounts
  keep working" (FR-029, US6 scenario 2) does not say whether a reload restores a demo password that someone
  changed on the Account tab; demo accounts are shared by reviewers, there is no password recovery (FR-011), and
  anyone can trigger the temporary block of FR-009 for a known demo email. With no email verification, anyone can
  register an `example.com` address before the demo data is loaded; the spec covers this collision for the
  administrator email (US3 scenario 8) but not for demo emails. The demo data also contains no administrator, so
  the administrator navigation variant (US4 scenario 3) is a state of this slice that the demo data does not show.

### MINOR-2: Email is classified as private from administrators, against the design of slice 11

- **Where**: `spec.md` US3 scenario 4 (lines 129–130), FR-018 (lines 301–303), SC-005 (lines 363–365); the
  uncommitted `CONTEXT.md` Account entry ("the email is never shown to others").
- **Against**: `design/pages/PageAdminAccounts.md` line 5 and `design/pages.md` line 85 (the administrator's
  accounts table has an email column, plus joined date, community state and status); `SPEC.md` §3 and §8 (the
  administrator boundary lists private goals, activity logs, journals and personal reports; administrators
  deactivate accounts); constitution III lines 71–72.
- **Reasoning**: FR-018 limits itself to "in this slice", but US3 scenario 4 calls another account's email and
  account details "private information", and the glossary entry added with this spec states without a qualifier
  that the email is never shown to others. The existing design for slice 11 shows exactly those fields to an
  administrator, and SPEC's private-data list does not include the email. A later slice that cites this spec or
  the glossary inherits a conflict with the design. SC-005 also asks for negative tests "for another registered
  user, an administrator and an anonymous visitor", but this slice has no use case that reads another account's
  data, so the spec does not say what those tests are run against.

### MINOR-3: The message shown while sign-in is blocked is ambiguous

- **Where**: `spec.md` US2 scenario 5 (lines 97–98), Edge Cases lines 243–244, FR-009, SC-004, Assumptions lines
  376–377.
- **Against**: internal consistency; `design/pages/PageSignIn.md` line 6 ("one message under the password field").
- **Reasoning**: Scenario 5 says "the message shown still does not reveal whether the email exists"; the edge case
  speaks of a "block message" that "is the same whether or not the email exists". A distinct block message that
  also appears for unknown emails requires counting failures for emails that have no account, which conflicts
  with "block sign-in for that account" (FR-009) and with the assumed framework lockout. A block message shown
  only for existing accounts reveals that the account exists, which FR-009 forbids. The spec does not say whether
  the blocked state reuses "Email or password is not correct." or has its own text, and the design has no such
  state.

### MINOR-4: Design-first is applied to some deviations from the previews and not to others

- **Where**: `spec.md` FR-026 and US5 scenario 1 (heading "Today"); FR-024 and US5 scenario 2 (avatar "initials
  from the email", no name, "only the Account tab"); US4 scenario 2 and FR-020 (avatar "opens Profile and 'Sign
  out'"); FR-011 (removal of "Forgot password?").
- **Against**: constitution IV (lines 87–91: a needed change "MUST be made in `design/` first"; pages "MUST follow
  the structure and wording of their previews"); decision R7 (`docs/roadmap.md` line 208: "only the sections and
  links whose slice has shipped, with no placeholder content"); `design/pages/PageHomeSignedIn.html` line 11;
  `design/pages/PageProfile.html` line 11 and `design/pages.md` line 29; `design/components/Tabs.md` ("Use 2–5
  tabs"); `design/components/TopNav.html` (the avatar is a non-interactive `span`) and
  `design/css/ht-components.css` (no class for an account menu); `design/README.md` "Not designed yet"
  ("password reset pages (use the Sign in layout)"); `design/pages/PageSignIn.md` line 5.
- **Reasoning**: The spec requires `design/` to change first for "Forgot password?" (FR-011), the Privacy page
  (FR-022) and the Account tab (FR-025). It does not do so for other deviations of the same kind. The "Today"
  page heading is new wording that replaces the previewed greeting; it is a substitute, not an omitted section,
  so R7 does not cover it. The Profile header changes the avatar rule (the preview derives "YD" from a name) to
  "initials from the email" without saying which characters or how many. A Profile with one tab conflicts with
  the Tabs guide's "2–5 tabs". The avatar menu on wide screens is required behaviour, but the design has only one
  sentence for it and no markup or class. The decision to have no password recovery leaves the design README
  still announcing password reset pages. None of these is acknowledged as an exception to constitution IV.

### MINOR-5: Whether the signed-in Home is a separate page from the visitor Home is not stated

- **Where**: `spec.md` US3 scenario 1 (lines 123–124), US2 scenario 3 (lines 93–94), FR-010, FR-015, US4 scenario 1
  (product name in the navigation).
- **Against**: internal consistency; `design/pages.md` lines 5–16 (two Home pages).
- **Reasoning**: US3 scenario 1 sends an anonymous visitor who opens the signed-in Home to Sign in. US2 scenario 3
  and FR-010 show the visitor Home to someone who is signed out. If both Homes share one address, the first
  scenario cannot occur for Home; if they have different addresses, the spec does not say where the product name
  link or a sign-out leads. The outcome of "open Home while signed out" is therefore readable two ways.

### MINOR-6: Security-relevant cases around password change and administrator credentials are missing

- **Where**: `spec.md` US5 scenarios 3–4, FR-024, Edge Cases (lines 237–248), FR-016, US3 scenario 9.
- **Against**: `ARCHITECTURE.md` §4 (session invalidation/revalidation is to be covered in planning and tests);
  `spec.md` FR-002, FR-011 ("Keep me signed in"), Assumptions line 380–381 (password change is the only way to
  replace a password).
- **Reasoning**: The edge cases cover sign-out in one of two browsers but not what happens to a user's other
  sessions, including kept ones, after a password change, although the spec makes password change the only remedy
  for a password someone else knows. FR-016 covers configured and absent administrator credentials but not
  incomplete ones (an email without a password) or a configured password that fails the rule of FR-002, or a
  configured email that is not a valid address.

### MINOR-7: Cross-feature decisions are recorded without an ADR and without noting that one is due

- **Where**: `spec.md` Clarifications (lines 20–30: R7, first administrator, no outbound email / no password
  recovery); `checklists/requirements.md` Notes.
- **Against**: Constitution, Decision Discipline line 139–140 ("A decision that spans features, is hard to
  reverse, or rejects real alternatives MUST also become an ADR in `docs/adr/`"); `docs/adr/` (contains only
  0001–0003).
- **Reasoning**: R7 fixes how every later slice applies constitution IV to partly built pages, and it rejected a
  named alternative (`docs/roadmap.md` line 208). The administrator bootstrap and the "no outbound email"
  decision also reject real alternatives and bind later slices (11 and any that would send email). No ADR exists
  for any of them, and neither artifact mentions that one is required.

### MINOR-8: The roadmap edits made with this spec leave contradictions with it

- **Where**: `spec.md` FR-026 and Clarifications lines 34–38; `checklists/requirements.md` lines 34–36 ("Roadmap
  decisions R1 and R7 were answered before the spec was written").
- **Against**: `docs/roadmap.md` line 113 ("`PageHomeSignedIn` | 1 (greeting)"), lines 207 and 210 (R6 and R9:
  "Before: slice 1", owner answer "—"), line 198 ("*Before* says which slice cannot start until the decision is
  made"), line 202 (R1: "README's example folder `001-habit-catalog/` changes too"); `README.md` line 53.
- **Reasoning**: The spec removes the greeting from slice 1, but the roadmap's "Pages across slices" table still
  says slice 1 creates the signed-in Home with its greeting. R6 and R9 are gates for slice 1 by the roadmap's own
  definition and are unanswered while the slice is already `specified`; the checklist reports only R1 and R7.
  R1's recorded consequence for the README has not been applied: it still names `001-habit-catalog/`.

### MINOR-9: Edge cases state requirements that no FR carries; one cites the wrong constitution clause

- **Where**: `spec.md` Edge Cases lines 239 (invalid email rejected), 240 (concurrent registrations), 241–242
  (two browsers, Back button), 248 ("Error pages and logs never show a password or other account details
  (constitution III)"); `checklists/requirements.md` line 27.
- **Against**: Constitution III line 79–80 ("Logs, exception messages, and error pages MUST NOT contain notes,
  custom-habit names, or recorded amounts"); `spec.md` FR-001, FR-016 (line 299), US3 scenario 8.
- **Reasoning**: Email format validation, the concurrency guarantee and the Back-button rule appear only as edge
  cases; FR-001 does not require a valid address, so these have no FR to trace to. Constitution III's log rule
  lists notes, custom-habit names and recorded amounts, not passwords or account details, so the citation does not
  support the sentence. The sentence is also wider than FR-016, which restricts only "the password [and] any other
  secret" in reports and logs, and it sits beside US3 scenario 8, where the conflict on the administrator email
  is reported to the operator; whether an email is an "account detail" that may appear in that report is not
  stated.

### MINOR-10: New terms defined as Key Entities are not in the glossary; "visitor" is used loosely

- **Where**: `spec.md` Key Entities lines 348–351 ("Role", "Acting user"); US3 scenario 6 (line 133, "Given a
  visitor, When they try to make an account an administrator through registration or the profile"); US2 scenario
  2 and US4 scenario 4 ("a visitor").
- **Against**: Constitution I lines 33–41 (vocabulary used verbatim; a new domain term gets a `CONTEXT.md` entry
  with a NOT-boundary in the same change); `CONTEXT.md` (entries added for Account, Administrator, Anonymous
  visitor and Registered user only).
- **Reasoning**: The spec treats "Role" and "Acting user" as entities of the feature, but the glossary change that
  accompanies it defines neither. "Visitor" is used both for an anonymous visitor and for anyone using the app;
  in US3 scenario 6 the "visitor" acts through the profile, which only a signed-in user can reach.

### MINOR-11: Numbering is out of order and two scenarios hold two behaviours each

- **Where**: `spec.md` US4 scenarios, numbered 1, 2, 3, 4, 5, 8, 6, 7 (lines 160–183); FR-031 placed between
  FR-027 and FR-028 (line 332); US2 scenario 7 and US3 scenario 7 (two Given/When/Then sequences each).
- **Against**: `.specify/extensions/ac/scripts/check_acceptance.py` lines 59–61 (scenario ids `US<N>-AS<M>` are
  taken from the written number); `acceptance-template.md` line 11 ("One item per behaviour").
- **Reasoning**: A Markdown renderer renumbers the US4 list in sequence, so the rendered scenario "6" is the
  Privacy scenario while the id `US4-AS6` taken from the source is the visitor Home scenario. References made
  from the rendered view and from the source then point to different scenarios. Scenarios that combine two
  behaviours cannot be covered by one AC item.

### MINOR-12: Checklist items are passed without qualification where the spec does not meet them

- **Where**: `checklists/requirements.md` lines 9, 11, 17, 30 and Notes lines 37–38.
- **Against**: `spec.md` FR-014 ("Every application use case MUST receive the acting user"), FR-016 ("At
  startup", "secret configuration", "source control"), Assumptions lines 376–377 and 383 ("the framework's
  standard lockout", "the framework's standard persistent session"), FR-011, FR-022 and FR-025 (edits to files
  under `design/`).
- **Reasoning**: The checklist marks "No implementation details", "Written for non-technical stakeholders" and
  "No implementation details leak into specification" as passed. Its notes explain only the `design/` references.
  The spec also prescribes application structure (FR-014), startup and configuration mechanics (FR-016),
  framework defaults (Assumptions) and repository process steps (FR-011, FR-022, FR-025). "Requirements are
  testable and unambiguous" is marked as passed despite the points in MAJOR-3, MAJOR-4, MAJOR-5 and MINOR-3.

## Summary

5 MAJOR, 12 MINOR.

The spec covers the product scope of slice 1 (SPEC §3, ARCHITECTURE §4, the roadmap's slice note) and its quoted
design wording matches the previews. It is not ready for the next step. `/speckit-ac-translate` requires an AC
item through the public API for every FR, and thirteen FRs describe only UI, browser or repository-process
matters; the decisions that determine what is on the AC surface are deferred to the plan, which comes later in
the flow. Lockout and related sign-in particulars were assumed without an owner decision and without the values
that acceptance criteria need. The account's community state and the administrator creation rule each allow two
incompatible readings. The MINOR notes concern demo accounts, the status of email towards administrators,
uneven use of the design-first rule, missing ADRs, and smaller inconsistencies with the roadmap, the glossary and
the checklist.
