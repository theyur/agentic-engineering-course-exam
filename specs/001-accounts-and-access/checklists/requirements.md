# Specification Quality Checklist: Accounts and access

**Purpose**: Validate specification completeness and quality before proceeding to planning
**Created**: 2026-11-11
**Feature**: [spec.md](../spec.md)

## Content Quality

- [ ] No implementation details (languages, frameworks, APIs) — two sections name project parts (see qualifications)
- [x] Focused on user value and business needs
- [ ] Written for non-technical stakeholders — see qualifications
- [x] All mandatory sections completed

## Requirement Completeness

- [x] No [NEEDS CLARIFICATION] markers remain — see qualifications
- [x] Requirements are testable and unambiguous — see qualifications
- [x] Success criteria are measurable
- [x] Success criteria are technology-agnostic (no implementation details)
- [x] All acceptance scenarios are defined
- [x] Edge cases are identified
- [x] Scope is clearly bounded
- [x] Dependencies and assumptions identified — see qualifications

## Feature Readiness

- [x] All functional requirements have clear acceptance criteria
- [x] User scenarios cover primary flows
- [x] Feature meets measurable outcomes defined in Success Criteria
- [ ] No implementation details leak into specification — same exception as above

## Notes

- Pass 1 (2026-11-11): three markers (first administrator, authentication UX, Privacy page) plus one question
  asked in conversation (signed-in Home head and Profile header) were answered by the owner and recorded in
  `## Clarifications`. Roadmap decisions R1 and R7 were answered before the spec was written.
- Revision after `critic-spec-pass-1.md`: owner answers added for the sign-in block, "Keep me signed in", the
  taken-email message, a changed administrator email, R6 and R9.
- Revision after `critic-spec-pass-2.md`: owner answers added for which account behaviour is Application use
  cases (the AC surface) and for how fast other sessions lose access after a password change.
- Revision after `critic-spec-pass-3.md` (APPROVE, 9 MINOR applied): owner answers added for the kept session
  (framework sliding renewal), the block as the app's own rule with supplied time, the block details and the
  email rule (confirmed), and R7 (sections and links only; preview wording stays).
- `/speckit-clarify` (2026-11-11): five owner answers, one per technical decision the roadmap places before
  `/speckit-ac-translate` (account model, application surface, result text, integration tests with the
  test-project layout, demo loading), recorded in `## Clarifications` and detailed in ADRs 0004 to 0007.
  ADRs 0008 (R7) and 0009 (no outbound email) record two earlier owner answers.
- Revision after `critic-spec-pass-4.md` (APPROVE, 6 MINOR applied): owner answers added for the local schema
  at start (HP-022) and for UI tests and mocking (UI-test project with bUnit and host tests; hand-written
  fakes, no mocking library).
- Qualifications to the items above:
  - *Non-technical stakeholders* and both *no implementation details* items: not met on purpose. The sections
    "What acceptance tests can reach" and "Decisions due before acceptance criteria", and the technical answers
    in `## Clarifications`, are written for the agents and the owner that run the AC workflow, and name ADRs,
    project names, Spec Kit commands and framework defaults. The requirements themselves (FR-NNN, HP-NNN) name
    no technology beyond the design files, except HP-022 ("schema migrations", "Development environment").
  - *No markers*: no `[NEEDS CLARIFICATION]` marker remains. Every ARCHITECTURE §9 decision this slice owns is
    answered in `## Clarifications` and linked to its ADR ("Decisions due before acceptance criteria").
  - *Testable*: FR-NNN are application behaviours that acceptance tests reach through the use cases (owner
    answer, ADR 0003), with literal values. Page, host, configuration, log and concurrency rules are HP-NNN and
    are verified by unit, integration and selective UI tests. Scenarios marked *(page)* are expected to be "Not
    testable through the API" in `acceptance.md`. FR-008 needs the current time to be supplied to the
    application, as constitution II requires.
  - *Design references*: references to `design/` are the project's UI source of truth (constitution IV).
    FR-010 restates constitution III and names the scenarios that show it. HP-018 says when and from where
    administrator creation gets its values, as the owner decided; how is left to the plan.
  - *Dependencies*: the technical decisions the roadmap places before `/speckit-ac-translate` are answered by
    the owner and recorded in ADRs 0004 to 0009. Decisions owned by later slices (hosting, migration rollout
    on deployment, how reviewers sign in to the hosted demo, account name, time zones) are named where the spec
    depends on them.

### Critic pass 1: verification

| Note | Verdict | Evidence and change |
|---|---|---|
| MAJOR-1 | confirmed | `acceptance-template.md` and `check_acceptance.py` require an AC item for every `**FR-NNN**`. Page, browser, log and design-process rules moved to HP-NNN and "Design changes before build"; FRs reworded as use-case behaviour; new section "What acceptance tests can reach". |
| MAJOR-2 | partly | Roadmap lines 128–149 place these decisions before `/speckit-ac-translate`, not in `/speckit-plan`: the assumption was wrong and is replaced by "Decisions due before acceptance criteria". They are taken with the owner between `/speckit-clarify` and `/speckit-ac-translate`, as the roadmap describes. |
| MAJOR-3 | confirmed | The block, the kept-session length and the taken-email message are now owner answers with literal values. |
| MAJOR-4 | confirmed | Community state removed from the Account entity, FR-004, the demo data and US1. |
| MAJOR-5 | confirmed | FR-014 case by case; a changed email creates an additional administrator (owner answer). |
| MINOR-1–MINOR-12 | confirmed | Changes as described in pass 2's resolution table. |

### Critic pass 2: verification

| Note | Verdict | Evidence and change |
|---|---|---|
| MAJOR-1 | confirmed | The FR/HP split assumed an answer to part of the open account-model row (roadmap slice note 1). Asked the owner: account behaviour is Application use cases; recorded in `## Clarifications`; roadmap slice note and row updated; `PageSignIn`'s "Built on ASP.NET Core Identity pages" note added to "Design changes before build"; operator capabilities (FR-014, FR-016) are called only by the host and tests, and FR-004 no longer depends on "startup". |
| MINOR-1 | confirmed | HP-008 covers Profile only; the Back-button edge case names both outcomes. |
| MINOR-2 | confirmed | FR-016 uses fixed demo emails and per-email results "created" / "already exists", so no demo marker is needed; a weak demo password is covered (US7-AS4); shared-account risk accepted in Edge Cases. |
| MINOR-3 | confirmed | FR-010 restated (constitution III) with the scenarios that show it; startup, configuration and concurrency moved to HP-018–HP-020. |
| MINOR-4 | confirmed | Literal results for invalid email, administrator creation and demo loading; email validity rule in FR-003; US4-AS4 split into AS4–AS7, US2-AS4/AS6 split into AS4–AS7; US3-AS6 removed (covered by US1-AS1 and US4-AS3). |
| MINOR-5 | confirmed | FR-008 states the boundary (4:59 blocked, 5:00 ended), that attempts during the block neither count nor extend it, and that time is supplied (constitution II); US2-AS4–AS6 use literal times. |
| MINOR-6 | confirmed | Design list adds `PageRegister` error states and notice, `pages.md`, README page list, TopNav avatar letter; HP-001 carries the "Sign in" link; "initials" is the first letter of the email (HP-014); R7 applied to sentences pointing to unshipped controls (Assumptions). |
| MINOR-7 | confirmed | Owner answer: new page loads need a new sign-in at once, open pages lose access within 30 minutes (HP-007); HP-004 says "page load". |
| MINOR-8 | confirmed | SC-005 names what the negative tests attempt; US6-AS5 added. |
| MINOR-9 | confirmed | "What acceptance tests can reach" states that scenarios share one database and need their own emails (completed after pass 3, MINOR-6). |
| MINOR-10 | confirmed | "Operator" and "Sign-in block" added to `CONTEXT.md`; "Anonymous visitor" boundary fixed; "visitor" no longer used for any user. |
| MINOR-11 | confirmed | Two items unticked with reasons; qualifications rewritten. |

### Critic pass 3: verification (APPROVE; MINOR notes applied at the owner's request)

| Note | Verdict | Evidence and change |
|---|---|---|
| MINOR-1 | confirmed | Every refused example was also too short. US1-AS3 (length only) and US1-AS4 (`walkingfar`, digit only) separate the two rules; weak examples elsewhere are 8+ characters without a digit. FR-008: "attempts neither count" dropped as indistinguishable once the end of a block resets the count; US2-AS7 added for that reset. |
| MINOR-2 | confirmed | US6-AS7 added (administrator with another account's current password); SC-005 cites it. |
| MINOR-3 | confirmed | Check order stated in FR-003, FR-013 and FR-014, with "missing" defined; US1-AS6, US4-AS8 and US6-AS4 show the order; FR-011 has the literal result "You are not signed in." |
| MINOR-4 | confirmed | Framework sliding renewal checked against the installed reference (renewal past half the window): owner chose the framework rule (HP-004, US2-AS11 now "within 7 days"). Framework lockout reads the clock itself: owner chose the app's own block rule (FR-008). Revalidation on every page load is listed as a constraint for the account-model ADR. |
| MINOR-5 | confirmed | Block details and the tightened email rule confirmed by the owner and recorded in `## Clarifications`; `ana@.` and `ana @example.com` are now refused (FR-003, US1-AS5). R7 answer: sections and links only, so the Register notice keeps its preview wording and no sentence is reworded. |
| MINOR-6 | confirmed | Each scenario now has its own emails; the demo scenarios' order dependence is stated in "What acceptance tests can reach"; "no account is created" is shown by a later registration (US4-AS5, US4-AS7) or by "still have no account" (US7-AS3, US7-AS4). |
| MINOR-7 | confirmed | FR-012 limited to use cases for signed-in users; operator results are about the supplied email (Edge Cases); who calls administrator creation and demo loading is stated once, in "What acceptance tests can reach". |
| MINOR-8 | confirmed | Roadmap page tracker: Shell row updated, `PagePrivacy` row added (the Register sentence stays, so nothing returns in slice 9). `MobileNav` added to the design list; HP-014 and US6-AS9 give the change-password states. |
| MINOR-9 | confirmed | Both "no implementation details" items unticked with the same reason; qualifications rewritten; rows MINOR-3 and MINOR-9 of pass 2 corrected. |

### Critic pass 4: verification (APPROVE; MINOR notes applied at the owner's request)

| Note | Verdict | Evidence and change |
|---|---|---|
| MINOR-1 | confirmed | Item annotation, the *No markers*, *Dependencies* and *no implementation details* qualifications rewritten; Notes entries added for `/speckit-clarify` and for this revision. |
| MINOR-2 | confirmed | Identity's default password options (upper case, non-alphanumeric required) and its allowed user-name characters would refuse values FR-002 and FR-003 accept. ADR 0004 now sets Identity no stricter than the domain rules and adds an integration-test consequence. |
| MINOR-3 | confirmed | A cookie cannot be reissued over an interactive circuit. ADR 0004 now makes Sign in, Sign out and change password server-rendered form posts, reissues the cookie after a password change (refresh sign-in), names the security-stamp validator with a zero interval, and limits the Identity user type in `HT.Ui` to composition code and the account endpoints. |
| MINOR-4 | confirmed | Owner answer: in Development the host applies pending migrations at start (HP-022, ADR 0007); other environments stay with slice 3. The fault rule of ADR 0007 is now in HP-018 and HP-021. |
| MINOR-5 | confirmed | Owner answers: `tests/HT.UiTests` with bUnit and `WebApplicationFactory` host tests on database `HT-UI`, no browser automation in this slice, no mocking library (ADR 0006); roadmap library rows decided. |
| MINOR-6 | confirmed | Roadmap slice note 1 and the spec's AC-surface answer no longer say "open"; the technical clarification lines are shortened to the decision plus its ADR link (the AC-relevant facts of ADR 0005 stay, because ADRs are not among the `specInputs`); the first-administrator lines link ADR 0007; ADR 0007 limits the shared demo accounts to the local demo and points to the open hosted-sign-in row; ADR 0009 describes the design changes as still to be made. |
