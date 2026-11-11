# Feature Specification: Accounts and access

**Feature Branch**: `feature/001-accounts-and-access`

**Created**: 2026-11-11

**Status**: Draft

**Input**: User description: "Slice 1 `accounts-and-access` from docs/roadmap.md. SPECIFY_FEATURE_DIRECTORY=specs/001-accounts-and-access"

Roadmap outcome: people can register, sign in and sign out. The app tells anonymous visitors apart from
signed-in registered users and administrators (SPEC §3; ARCHITECTURE §4).

## Clarifications

### Session 2026-11-11

- Q: Roadmap R1: should accounts be their own first slice, ahead of the habit catalog? → A: Yes. Accounts
  and access is slice 1 (`001-accounts-and-access`); the habit catalog follows as slice 2.
- Q: Roadmap R7: what do partly built pages and navigation show before the slices that complete them have
  shipped? → A: A page or the navigation shows only the sections and links whose slice has shipped, with no
  placeholder content. The full design preview is reached when the last extending slice ships.
- Q: Does R7 also remove sentences that point to a control that has not shipped (for example "You can opt in
  later in your profile")? → A: No. R7 covers sections and links only; sentences keep the wording of the
  previews.
- Q: Roadmap R6: do other slices need an idea brief? → A: Optional. This slice has none.
- Q: Roadmap R9: should `CLAUDE.md` point to `docs/roadmap.md`? → A: Yes, one line under "Docs and git
  workflow", added in this change.
- Q: Which account behaviour is application behaviour that acceptance tests can check? → A: Registration, the
  sign-in check with its block, password change, account details, administrator creation and demo loading
  are Application use cases. The host keeps only the browser session. Sign in and Register are the app's own
  pages, styled with the design, not the framework's built-in account pages. Where the account model lives
  and how the acting user reaches the use cases stays for the ADR (see "Decisions due before acceptance
  criteria").
- Q: How does the first administrator account come to exist? → A: At startup, the system creates the
  administrator account if it does not exist yet, from an email and password held in the deployment's secret
  configuration (never in source control). There is no UI for it, and registration never creates an
  administrator.
- Q: The configured administrator email is later changed to an address that has no account. What happens?
  → A: Startup creates an administrator for the new email; the earlier administrator stays an administrator.
  Version 1 can therefore have several administrators; removing the role is out of scope.
- Q: Authentication UX (ARCHITECTURE §9): email verification, password recovery, outbound email, "Keep me
  signed in"? → A: No outbound email in this slice: no email verification and no password recovery. "Forgot
  password?" is removed from `design/pages/PageSignIn` first. "Keep me signed in" stays and keeps the session
  across browser restarts. Users change their password on the Profile Account tab.
- Q: How long does "Keep me signed in" last? → A: 14 days, with the framework's sliding renewal: a kept
  session ends at least 7 and at most 14 days after the last visit. No custom session handling.
- Q: Is there a temporary block after failed sign-ins, and what does the user see? → A: Yes. After 5 failed
  sign-ins in a row for one account, sign-in for that account is blocked for 5 minutes. The message stays
  "Email or password is not correct.", so the block never reveals that the email has an account. A
  successful sign-in resets the count.
- Q: Where does the block rule live, given that the framework's own lockout reads the system clock? → A: The
  block is the app's own rule and is judged against the supplied current time (constitution II); the
  framework's lockout is not used. The framework still stores passwords and keeps sessions.
- Q: The block's details: when does it end, and what do attempts during it do? → A: It ends exactly 5 minutes
  after the fifth failure (blocked at 4:59, ended at 5:00). Attempts during the block do not extend it, and
  the end of a block resets the count to 0.
- Q: What is a valid email address? → A: After trimming: no spaces, exactly one "@", at least one character
  before it, and after it a domain of at least two non-empty parts separated by dots (`ana@example.com` is
  valid; `ana@`, `ana@example`, `ana@.` and `ana @example.com` are not).
- Q: What does Register say for an email that already has an account? → A: "An account with this email
  already exists." with a link to Sign in. Without email verification this cannot be hidden anyway.
- Q: After a password change, how fast do the account's other sessions lose access? → A: Any new page load
  needs a new sign-in at once; a page that is already open in another browser loses access within 30
  minutes.
- Q: The footer links to a Privacy page that has no design. Is it part of this slice? → A: Yes. A short,
  static Privacy page is designed in `design/` first (`PagePrivacy`), then built. It states what stays
  private, that community comparison is opt-in, and that administrators cannot see private tracking data.
- Q: The design's signed-in Home head ("Good evening, Yuri" and a date) and Profile header (name, "member
  since") need a name that Register does not collect and a time-zone rule that slice 4 (`goals`) decides.
  What does this slice show? → A: No name and no time-dependent text. The signed-in Home head is a plain
  "Today" heading; the Profile header shows initials and email, without "member since". The greeting, the
  date and "member since" arrive once time zones are decided; whether accounts get a name stays undecided.

## What acceptance tests can reach

ADR 0003 limits acceptance tests to the Application use cases, the two `HT.Persistence` entry points and the
Domain types they use; `HT.Ui` is not tested by them. Following the owner's answer above, this spec splits
its requirements:

- **FR-NNN** are application behaviours. Each is reachable through an Application use case, so acceptance
  tests can verify it. Administrator creation (FR-014) and demo loading (FR-016) are operator capabilities:
  no page offers them. The host runs administrator creation at every start (HP-018); how and when demo
  loading runs is decided with demo-data seeding (see "Decisions due before acceptance criteria"). Acceptance
  tests call both directly.
- **HP-NNN** are host, page and integration requirements: what pages show, navigation, browser sessions,
  startup, configuration, logs and concurrency. They are verified by unit, integration and selective UI tests,
  not by acceptance tests.
- Scenarios marked *(page)* describe page, browser or host behaviour. They are expected to be listed as "Not
  testable through the API" in `acceptance.md`, with this section as the reason.

Test data: an acceptance-test run starts empty, but its scenarios share one database and nothing in this
slice removes an account. Every scenario below therefore uses emails of its own, except the demo scenarios:
the demo emails (FR-016) are fixed, so US7-AS1, US7-AS3 and US7-AS4 need a run in which demo loading has not
yet created the demo accounts, and US7-AS2 needs the state US7-AS1 leaves. Acceptance criteria may order
these scenarios or combine them into one sequence.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Register an account (Priority: P1)

An anonymous visitor creates an account with an email address and a password. After registering they are
signed in as a registered user and land on the signed-in Home.

**Why this priority**: Without an account no other slice can deliver personal value. Every later feature
(custom habits, goals, records, reports, community opt-in) belongs to a registered user.

**Independent Test**: Register a new account, confirm that it is a registered user and not an administrator,
and confirm that it can sign in later with the same email and password.

**Acceptance Scenarios**:

1. **Given** no account with the email `new.user@example.com`, **When** an anonymous visitor registers with
   that email and the password `walking42`, **Then** an account with that email exists, its role is
   registered user, and it can sign in with `walking42`.
2. **Given** an account with the email `taken@example.com`, **When** an anonymous visitor registers with
   ` Taken@Example.com ` and `walking42`, **Then** no account is created and the result is "An account with
   this email already exists."
3. **Given** an anonymous visitor, **When** they register `short.pw@example.com` with the password `walk4`
   (5 characters), **Then** no account is created and the result is "At least 8 characters, with a number."
4. **Given** an anonymous visitor, **When** they register `no.digit@example.com` with the password
   `walkingfar` (10 characters, no digit), **Then** no account is created and the result is "At least 8
   characters, with a number."
5. **Given** an anonymous visitor, **When** they register with the email `ana.example.com`, `ana@`,
   `ana@example`, `ana@.` or `ana @example.com`, **Then** no account is created and the result is "Enter a
   valid email address."
6. **Given** an account with the email `taken2@example.com`, **When** an anonymous visitor registers with
   `taken2@example.com` and `walk4`, **Then** the result is "At least 8 characters, with a number." (FR-003
   order: a valid email is checked first, then the password rule, then whether the email is taken).
7. *(page)* **Given** an anonymous visitor on the Register page, **When** they look at the form, **Then** they
   see the password rule as help text and the neutral notice "Community comparison is off. You can opt in
   later in your profile."
8. *(page)* **Given** an anonymous visitor on the Register page, **When** the repeated password differs,
   **Then** "The passwords do not match." is shown next to it and the email they entered is kept.
9. *(page)* **Given** an anonymous visitor on the Register page, **When** registration is refused because the
   email is taken, **Then** the message is shown next to the email field with a "Sign in" link, and the email
   is kept.
10. *(page)* **Given** an anonymous visitor who registers successfully, **When** the account is created,
    **Then** they are signed in and see the signed-in Home.

---

### User Story 2 - Sign in and sign out (Priority: P1)

A registered user signs in with their email and password, uses the app as themselves, and signs out when
they are done. A failed sign-in never reveals whether the email or the password was wrong, or that an account
is blocked.

**Why this priority**: Registration alone is not usable without a way to come back. Sign-in and sign-out
complete the minimal account loop and are needed by every reviewer who opens the demo.

**Independent Test**: With an existing account, sign in and confirm that the app acts as that account; fail
five times and confirm the block; sign out and confirm that the app treats the user as an anonymous visitor
again.

**Acceptance Scenarios**:

1. **Given** an account `ana@example.com`, **When** someone signs in with `ANA@example.com` and the correct
   password, **Then** sign-in succeeds and the acting user is that account, with its role.
2. **Given** no account `nobody@example.com`, **When** someone signs in with that email, **Then** sign-in
   fails with "Email or password is not correct."
3. **Given** an account `wrong.pw@example.com`, **When** someone signs in with that email and a wrong
   password, **Then** sign-in fails with "Email or password is not correct."
4. **Given** an account `block1@example.com` whose fifth failed sign-in in a row happened at 10:00:00, **When**
   someone signs in with the correct password at 10:04:59, **Then** sign-in fails with "Email or password is
   not correct."
5. **Given** an account `block2@example.com` whose fifth failed sign-in in a row happened at 10:00:00, **When**
   someone signs in with the correct password at 10:05:00, **Then** sign-in succeeds.
6. **Given** an account `block3@example.com` blocked since 10:00:00 and a failed sign-in attempt at 10:03:00,
   **When** someone signs in with the correct password at 10:05:00, **Then** sign-in succeeds: the attempt
   during the block did not extend it.
7. **Given** an account `block4@example.com` whose block started at 10:00:00, and 4 failed sign-ins between
   10:05:00 and 10:05:30, **When** someone signs in with the correct password at 10:06:00, **Then** sign-in
   succeeds: the end of the block reset the count to 0.
8. **Given** an account `reset@example.com` with 4 failed sign-ins in a row followed by a successful one,
   **When** 4 more sign-ins fail and then one with the correct password is made, **Then** that sign-in
   succeeds: the success reset the count to 0.
9. *(page)* **Given** a user on the Sign in page, **When** sign-in fails, **Then** the single message is
   shown under the password field and the email they entered is kept.
10. *(page)* **Given** the Sign in page, **When** someone looks at the form, **Then** they see "Keep me signed
    in" and no "Forgot password?" link.
11. *(page)* **Given** a user who signed in with "Keep me signed in", **When** they close and reopen the
    browser within 7 days of their last visit, **Then** they are still signed in.
12. *(page)* **Given** a user who signed in without "Keep me signed in", **When** they close and reopen the
    browser, **Then** they are signed out.
13. *(page)* **Given** a signed-in user, **When** they choose "Sign out", **Then** their session in that
    browser ends and they see the visitor Home.
14. *(page)* **Given** an anonymous visitor who opened Profile and was sent to Sign in, **When** they sign in,
    **Then** they are taken to Profile.

---

### User Story 3 - Access by audience (Priority: P1)

The app distinguishes three audiences: anonymous visitors, registered users and administrators. The
distinction holds in the application itself, not only on the screens: every use case knows who is acting and
in which role. Capabilities for signed-in users are closed to anonymous visitors, and being an administrator
never grants access to another person's private data.

**Why this priority**: Every later slice depends on this boundary. The privacy promise of SPEC §3 and the
"no admin bypass" rule of ARCHITECTURE §4 must be in place before the first private data exists.

**Independent Test**: As an anonymous visitor, a registered user and an administrator, read account details.
Confirm the email and role returned for each signed-in user, that each reads only their own account, and
that the anonymous visitor is refused.

**Acceptance Scenarios**:

1. **Given** an anonymous visitor, **When** they ask for account details, **Then** the result is "You are not
   signed in."
2. **Given** a signed-in registered user `details@example.com`, **When** they ask for account details,
   **Then** the result is the email `details@example.com` and the role registered user.
3. **Given** a signed-in administrator `lead.admin@example.com`, **When** they ask for account details,
   **Then** the result is the email `lead.admin@example.com` and the role administrator.
4. **Given** registered users `cara@example.com` and `ben@example.com`, **When** `ben@example.com` asks for
   account details, **Then** the result is `ben@example.com`'s own email and role, never `cara@example.com`'s.
5. **Given** an administrator `audit.admin@example.com` and a registered user `dana@example.com`, **When** the
   administrator asks for account details, **Then** the result is the administrator's own email and role,
   never `dana@example.com`'s.
6. *(page)* **Given** an anonymous visitor, **When** they open Profile, **Then** they are sent to the Sign in
   page and nothing from Profile is shown.
7. *(page)* **Given** a signed-in user, **When** they open the Sign in or Register page, **Then** they are
   taken to Home.

---

### User Story 4 - First administrator (Priority: P2)

The operator of a deployment puts an administrator email and password into the deployment's secret
configuration. Each time the system starts, administrator creation runs with those values.

**Why this priority**: Slice 2 needs an administrator to curate the catalog, and this slice must already tell
administrators apart. Without a defined way to get one, no administrator can exist.

**Independent Test**: Run administrator creation with an email and password, sign in as the administrator,
run it again and confirm that nothing changed.

**Acceptance Scenarios**:

1. **Given** no account `first.admin@example.com`, **When** administrator creation runs with
   `first.admin@example.com` and `curator2026`, **Then** the result is "administrator created", and
   `first.admin@example.com` signs in with `curator2026` and has the role administrator.
2. **Given** an administrator `second.admin@example.com` with the password `curator2026`, **When**
   administrator creation runs with `second.admin@example.com` and `other2026`, **Then** the result is
   "administrator already exists", and the administrator still signs in with `curator2026`, not with
   `other2026`.
3. **Given** a registered user `claim@example.com`, **When** administrator creation runs with
   `claim@example.com` and `curator2026`, **Then** the result is "email belongs to a registered user" and
   `claim@example.com` stays a registered user.
4. **Given** no administrator email and no password, **When** administrator creation runs, **Then** the result
   is "no administrator configured".
5. **Given** the email `fifth.admin@example.com` and a password of only spaces, **When** administrator creation
   runs, **Then** the result is "no administrator configured", and registering `fifth.admin@example.com`
   afterwards succeeds.
6. **Given** the email `admin.example.com` and the password `curator2026`, **When** administrator creation
   runs, **Then** the result is "configured email is not valid".
7. **Given** the email `seventh.admin@example.com` and the password `curatorship`, **When** administrator
   creation runs, **Then** the result is "configured password does not meet the password rule", and
   registering `seventh.admin@example.com` afterwards succeeds.
8. **Given** an administrator `eighth.admin@example.com`, **When** administrator creation runs with
   `eighth.admin@example.com` and `curatorship`, **Then** the result is "administrator already exists"
   (FR-014 order: the existing administrator is found before the password rule is checked).
9. **Given** an administrator `ninth.admin@example.com` and no account `tenth.admin@example.com`, **When**
   administrator creation runs with `tenth.admin@example.com` and `curator2026`, **Then** the result is
   "administrator created" and both accounts have the role administrator.
10. *(page)* **Given** administrator credentials in the secret configuration, **When** the system starts,
    **Then** administrator creation runs with them, its result is written to the operator log without any
    password, and the system starts normally whatever the result.

---

### User Story 5 - Shared shell and visitor Home (Priority: P2)

Every page shares one shell: a top navigation bar (with a menu on narrow screens) and a footer. An
anonymous visitor's Home introduces the product and invites them to register. Following decision R7, the
shell and the visitor Home show only what has shipped.

**Why this priority**: Reviewers enter through the visitor Home and the navigation. The shell is reused by
every later slice, so building it once now avoids rework.

**Independent Test**: Open Home as an anonymous visitor and as a signed-in user, on a wide and on a narrow
screen. Confirm the navigation variant, the footer, the Privacy page and the visitor Home sections.

**Acceptance Scenarios**:

1. *(page)* **Given** an anonymous visitor, **When** they open any page, **Then** the top navigation shows the
   product name "Habit Tracker" and, on the right, "Sign in" and "Register", and no Catalog or Community link.
2. *(page)* **Given** a signed-in registered user, **When** they open any page, **Then** the top navigation
   shows the product name, the "Today" link and their avatar, which opens a menu with "Profile" and "Sign
   out"; it shows no "Goals" or "+ New goal".
3. *(page)* **Given** a signed-in administrator, **When** they open any page, **Then** the top navigation shows
   the product name, the "Today" link and their avatar with "Profile" and "Sign out", and no link to an Admin
   page or to any private page.
4. *(page)* **Given** a screen narrower than 860px, **When** a user opens the menu, **Then** it lists the same
   items as the wide navigation for their audience.
5. *(page)* **Given** the open menu on a narrow screen, **When** the user chooses a link, presses Esc or taps
   outside, **Then** the menu closes.
6. *(page)* **Given** any page, **When** it is shown, **Then** the footer shows the project line ("Habit Tracker
   · Agentic Engineering course project") and a "Privacy" link, and no "Send feedback" link.
7. *(page)* **Given** an anonymous visitor, **When** they open the Privacy page, **Then** they can read it
   without signing in, and it states what stays private, that community comparison is opt-in, and that
   administrators cannot see private tracking data.
8. *(page)* **Given** an anonymous visitor, **When** they open Home, **Then** they see the hero (headline "Keep
   the habits you choose", its one sentence, the primary "Register free" button and the privacy line "Your
   goals and notes stay private. Community comparison is opt-in.") followed by the three "How it works" steps,
   and no "Browse the catalog", example goal card, catalog preview or community bands.
9. *(page)* **Given** an anonymous visitor, **When** they choose "Register free" or "Register", **Then** they
   reach the Register page.
10. *(page)* **Given** an anonymous visitor, **When** they choose "Sign in", **Then** they reach the Sign in
    page.

---

### User Story 6 - Signed-in Home and Profile Account tab (Priority: P3)

A signed-in user has a Home (Today) and a Profile. In this slice the Home shows only its heading, and the
Profile shows the account's email and lets the user change their password.

**Why this priority**: These are the landing places after registering and signing in. They are minimal now
and are filled by later slices (catalog, goals, reports, community).

**Independent Test**: Sign in, read the account details, change the password, and sign in with the new
password.

**Acceptance Scenarios**:

1. **Given** a signed-in user `change@example.com` with the password `walking42`, **When** they change their
   password from `walking42` to `running42`, **Then** sign-in with `running42` succeeds and sign-in with
   `walking42` fails.
2. **Given** a signed-in user `wrong.current@example.com` with the password `walking42`, **When** they change
   their password giving `wrong4242` as the current password, **Then** the password is not changed and the
   result is "The current password is not correct."
3. **Given** a signed-in user `weak.new@example.com` with the password `walking42`, **When** they change their
   password from `walking42` to `runningfar`, **Then** the password is not changed and the result is "At least
   8 characters, with a number."
4. **Given** a signed-in user `both.wrong@example.com` with the password `walking42`, **When** they change
   their password giving `wrong4242` as the current password and `run` as the new one, **Then** the result is
   "The current password is not correct." (FR-013 order: the current password is checked first).
5. **Given** an anonymous visitor, **When** they try to change a password, **Then** the result is "You are not
   signed in."
6. **Given** users `erin@example.com` (password `walking42`) and `finn@example.com` (password `cycling42`),
   **When** `finn@example.com` changes their password giving `walking42` as the current password, **Then** the
   result is "The current password is not correct.", `erin@example.com` still signs in with `walking42`, and
   `finn@example.com` still signs in with `cycling42`.
7. **Given** a registered user `gale@example.com` (password `walking42`) and an administrator
   `keeper.admin@example.com` (password `curator2026`), **When** the administrator changes their password
   giving `walking42` as the current password, **Then** the result is "The current password is not
   correct.", `gale@example.com` still signs in with `walking42`, and the administrator still signs in with
   `curator2026`.
8. *(page)* **Given** a user signed in in two browsers, **When** they change their password in one, **Then**
   any new page load in the other browser needs a new sign-in, and a page already open there loses access
   within 30 minutes; this also holds for a session kept with "Keep me signed in".
9. *(page)* **Given** a signed-in user on the Account content, **When** the change succeeds, **Then** the notice
   "Your password is changed." is shown and the form's fields are empty; **When** it is refused, the message
   is shown next to the field it concerns.
10. *(page)* **Given** a signed-in user, **When** they open Home, **Then** they see the heading "Today" with no
    greeting, name or date, and no stat tiles, Today list, goal summary or notes.
11. *(page)* **Given** a signed-in user `ana@example.com`, **When** they open Profile, **Then** they see a
    header with the avatar "A" and the email `ana@example.com`, without a name or "member since", and the
    Account content (email and the change-password form); no Community, Goal history or Custom habits tab.

---

### User Story 7 - Demonstration accounts (Priority: P3)

The app can be filled with synthetic demonstration accounts, so that later slices have users to attach their
demo data to and reviewers can sign in without registering.

**Why this priority**: Constitution VI requires every user-facing slice to extend the demo data. Accounts are
the foundation the demo data of every later slice builds on.

**Independent Test**: Load the demo data with a demo password, sign in as a demo account, then load it again
and confirm that nothing is duplicated or changed.

**Acceptance Scenarios**:

1. **Given** none of the demo emails has an account, **When** demo loading runs with the demo password
   `example2026`, **Then** the result is "created" for `demo1@example.com`, `demo2@example.com` and
   `demo3@example.com`, and each signs in with `example2026` as a registered user.
2. **Given** the state after US7-AS1 and `demo3@example.com` changed its password to `changed2026`, **When**
   demo loading runs again with `example2026`, **Then** the result is "already exists" for all three demo
   emails, no account is added, and `demo3@example.com` still signs in with `changed2026`.
3. **Given** none of the demo emails has an account and no demo password, **When** demo loading runs, **Then**
   the result is "no demo password configured" and the demo emails still have no account.
4. **Given** none of the demo emails has an account and the demo password `examplepass`, **When** demo loading
   runs, **Then** the result is "demo password does not meet the password rule" and the demo emails still have
   no account.
5. *(page)* **Given** the demo data, **When** someone inspects it, **Then** no demo account uses a real person's
   name or an address outside the reserved `example.com` domain, and the demo password is not in source
   control.

---

### Edge Cases

- An email with surrounding spaces or in mixed case is the same email as its trimmed, lower-case form, for
  registration, sign-in, administrator creation and demo loading (FR-003, FR-006).
- Two registrations with the same email at almost the same time result in exactly one account (HP-020).
- Failed sign-ins for an email that has no account block nothing and give the same message (FR-007, FR-008).
- A demo email that someone registered before demo loading ran gets "already exists" and keeps its owner's
  password (FR-016).
- Anyone who knows a demo email can trigger its 5-minute block or change its password; reviewers share these
  accounts, and this is accepted for the local demo. How reviewers sign in to the hosted demo is slice 3's
  decision.
- A user signed in in two browsers who signs out in one stays signed in in the other (HP-006).
- After signing out, the browser's Back button does not show Profile's content; going back to Home shows the
  visitor Home (HP-008, HP-009).
- A user who forgets their password cannot recover the account in this slice; this is accepted for the demo.
- Results of administrator creation and demo loading are about the email the operator supplied; they may name
  that email but never a password (HP-017).

## Requirements *(mandatory)*

### Functional Requirements (application behaviour, on the AC surface)

**Registration**

- **FR-001**: The system MUST let an anonymous visitor create an account with an email address and a password.
- **FR-002**: Every password the system sets (registration, password change, administrator, demo accounts)
  MUST have at least 8 characters and contain at least one digit. A password that does not is refused with
  "At least 8 characters, with a number."
- **FR-003**: The system MUST treat an email as its trimmed, case-insensitive form and accept it only if it has
  no spaces, exactly one "@", at least one character before it, and after it a domain of at least two
  non-empty parts separated by dots; otherwise the result is "Enter a valid email address." Emails MUST be
  unique: a registration with an email that already belongs to an account is refused with "An account with
  this email already exists." Registration gives one result, checking in this order: valid email, password
  rule (FR-002), email not taken.
- **FR-004**: A newly registered account MUST have the role registered user. No capability available to an
  anonymous visitor or a signed-in user may give an account the administrator role.

**Sign-in**

- **FR-005**: The system MUST let an account sign in with its email and password; a successful sign-in makes
  that account, with its role, the acting user.
- **FR-006**: Sign-in MUST match the email case-insensitively and without surrounding spaces.
- **FR-007**: Every failed sign-in MUST give the same result, "Email or password is not correct.", whether the
  email is unknown, the password is wrong, or the account is blocked.
- **FR-008**: After 5 failed sign-ins in a row for one account, sign-in for that account MUST be blocked from
  the fifth failure until exactly 5 minutes later: at 4 minutes 59 seconds it is still blocked, at 5 minutes 0
  seconds it has ended. During the block even the correct password fails as in FR-007, and attempts do not
  extend the block. A successful sign-in, and the end of a block, reset the count to 0. The block is the app's
  own rule, judged against the current time supplied to the application (constitution II).

**Audiences and access**

- **FR-009**: The system MUST distinguish three audiences: anonymous visitor, registered user and
  administrator. Every account has exactly one role: registered user or administrator.
- **FR-010**: Use cases for signed-in users MUST act only on the account of the acting user they are given
  (constitution III); none of them takes another account's identifier. This is shown by US3-AS4, US3-AS5,
  US6-AS6 and US6-AS7.
- **FR-011**: Use cases for signed-in users MUST refuse an anonymous visitor with the result "You are not
  signed in."
- **FR-012**: A signed-in user MUST be able to read their own account details: email and role. No use case for
  signed-in users may return another account's details, whatever the acting user's role.
- **FR-013**: A signed-in user MUST be able to change their own password by giving the current password and a
  new one. The change gives one result, checking in this order: a wrong current password is refused with "The
  current password is not correct."; a new password that fails FR-002 is refused with its rule. After a
  change, only the new password signs in.

**Administrator creation**

- **FR-014**: Administrator creation MUST take an email and a password and give exactly one result, checking
  in this order:
  1. "no administrator configured" when the email or the password is missing (absent, empty or only spaces):
     nothing is created;
  2. "configured email is not valid" (FR-003): nothing is created;
  3. "administrator already exists" when an administrator has that email: nothing changes, including its
     password;
  4. "email belongs to a registered user" when a registered user has that email: nothing changes;
  5. "configured password does not meet the password rule" (FR-002): nothing is created;
  6. "administrator created": an account with that email, that password and the role administrator is
     created.

  Each new email therefore creates an additional administrator; earlier administrators keep the role. No page
  offers administrator creation; the host runs it at startup (HP-018).
- **FR-015**: The administrator role MUST NOT grant access to any other account's private data. In this slice
  that means FR-010 and FR-012 hold for administrators exactly as for registered users.

**Demonstration data**

- **FR-016**: Demo loading MUST take a demo password. Without one (absent, empty or only spaces) the result is
  "no demo password configured", and with one that fails FR-002 it is "demo password does not meet the
  password rule"; in both cases no account is created. Otherwise, for each of the demo emails
  `demo1@example.com`, `demo2@example.com` and `demo3@example.com`, the result is "created" (a registered-user
  account with that password is created) or "already exists" (the existing account is left unchanged). No
  page offers demo loading.

### Host, page and integration requirements (outside the AC surface)

- **HP-001**: The Register page MUST follow `design/pages/PageRegister`: email, password with the rule as help
  text, repeated password, the neutral notice "Community comparison is off. You can opt in later in your
  profile.", and "Create account". A repeated password that differs shows "The passwords do not match." next
  to it; a taken email shows FR-003's message next to the email field with a "Sign in" link; an invalid email
  and the password rule show their messages next to their fields. Errors keep the entered email. The page
  asks for nothing else.
- **HP-002**: After a successful registration the new user MUST be signed in and see the signed-in Home.
- **HP-003**: The Sign in page MUST follow `design/pages/PageSignIn` without "Forgot password?": email,
  password, "Keep me signed in", "Sign in", and "New here? Create an account". The single failure message
  appears under the password field and the entered email is kept.
- **HP-004**: With "Keep me signed in", the session MUST survive browser restarts and use the framework's
  14-day sliding renewal: it ends at least 7 and at most 14 days after the last visit. Without it, the session
  MUST end when the browser closes.
- **HP-005**: After a sign-in that followed a redirect, the user MUST be returned to the page they first asked
  for, but only to a page inside the app.
- **HP-006**: "Sign out" MUST end the session in that browser and show the visitor Home; sessions in other
  browsers stay.
- **HP-007**: After a password change, every other session of that account, including kept ones, MUST need a
  new sign-in for any new page load, and a page already open in another browser MUST lose access within 30
  minutes. The session that made the change stays signed in.
- **HP-008**: Profile MUST send an anonymous visitor to Sign in without showing its content, also after
  sign-out through the browser's Back button.
- **HP-009**: Home MUST have one address: it shows the visitor Home to an anonymous visitor and the signed-in
  Home to a signed-in user. The product name links to Home. A signed-in user who opens Sign in or Register is
  taken to Home.
- **HP-010**: Every page MUST use the shared shell (`design/components/TopNav`, `MobileNav`, `Footer`) and show
  only links whose slice has shipped (R7). In this slice: visitor — "Sign in", "Register"; registered user
  and administrator — "Today" and the avatar menu with "Profile" and "Sign out"; footer — project line and
  "Privacy".
- **HP-011**: The visitor Home MUST show the hero (headline, sentence, "Register free", privacy line) and the
  three "How it works" steps with the wording of `design/pages/PageHomeVisitor`, and nothing else from that
  preview.
- **HP-012**: A Privacy page that anyone can open MUST state what stays private (goals, records, notes, custom
  habits), that community comparison is opt-in and off by default, and that administrators cannot see private
  tracking data. It states only promises that SPEC makes.
- **HP-013**: The signed-in Home MUST show only the heading "Today".
- **HP-014**: The avatar, in the navigation, the narrow-screen menu and the Profile header, MUST show the
  first letter of the email in upper case. Profile MUST show the avatar, the email and the Account content: the
  email and the change-password form (current password, new password, repeated new password, with the rule as
  help text). A repeated new password that differs shows "The passwords do not match." next to it; FR-013's
  refusals appear next to the current or the new password; a successful change shows the notice "Your
  password is changed." and empties the form. Profile shows no name, no "member since" and no other tab.
- **HP-015**: Pages of this slice MUST NOT show any date or time of day that depends on the user's time zone.
- **HP-016**: All user-facing text MUST follow `design/brand-book.md`, and every page MUST meet the
  accessibility rules of constitution IV.
- **HP-017**: Logs, error pages and the results of administrator creation and demo loading MUST NOT contain a
  password or other secret.
- **HP-018**: At every start, the host MUST run administrator creation with the email and password from the
  deployment's secret configuration, write the result to the operator log, and start normally whatever the
  result.
- **HP-019**: The administrator credentials and the demo password MUST come from configuration outside source
  control.
- **HP-020**: Two registrations with the same email at almost the same time MUST result in exactly one
  account; this is verified by an integration test against the real database.

### Design changes before build

Constitution IV requires `design/` to change before the app does. Before the affected pages are built:

- `design/pages/PageSignIn`: remove "Forgot password?" and replace the note "Built on ASP.NET Core Identity
  pages" (Sign in is the app's own page).
- `design/pages/PageRegister`: the error states of HP-001 (taken email with "Sign in" link, passwords that do
  not match, invalid email, password rule).
- `design/pages/PageProfile`: the slice-1 state — header without name and "member since", the avatar letter,
  the Account content with its error and success states (HP-014), and how Profile looks with a single section
  (`design/components/Tabs` asks for 2–5 tabs).
- `design/pages/PageHomeSignedIn`: the slice-1 state with the heading "Today".
- `design/components/TopNav` and `design/components/MobileNav`: the avatar menu (Profile, Sign out) and the
  avatar letter.
- `design/pages/PagePrivacy`: new page.
- `design/pages.md`: the slice-1 structure of the pages above and the new Privacy page.
- `design/README.md`: add `PagePrivacy` to the page list and remove "password reset pages" from "Not designed
  yet".

### Key Entities

- **Account**: one person's way into the app. Holds the email (unique, case-insensitive), the password
  credential, the role, and its sign-in block state (the count of failed sign-ins and the time of any current
  block). Later slices add community consent and the public display name (slice 9) and the deactivated state
  (slice 11).
- **Role**: registered user or administrator. Every account has exactly one. An anonymous visitor has no
  account and no role.
- **Acting user**: the account, with its role, on whose behalf a use case runs, or the anonymous visitor.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A first-time visitor can go from the visitor Home to a signed-in Home by registering in under
  2 minutes.
- **SC-002**: A registered user can sign in from the Sign in page in under 30 seconds.
- **SC-003**: 100% of attempts by an anonymous visitor to use a capability for signed-in users are refused
  (FR-011, HP-008).
- **SC-004**: In 100% of failed sign-ins the result is identical for an unknown email, a wrong password and a
  blocked account (FR-007).
- **SC-005**: Reading account details and changing a password have negative tests (constitution III): an
  anonymous visitor is refused (US3-AS1, US6-AS5); another registered user and an administrator get only their
  own account's details (US3-AS4, US3-AS5) and cannot change someone else's password, even with that
  account's correct current password (US6-AS6, US6-AS7).
- **SC-006**: Running demo loading twice in a row leaves exactly the same accounts, with the same passwords, as
  running it once (FR-016).
- **SC-007**: Every page of this slice passes the accessibility rules of constitution IV: visible focus ring,
  text contrast of at least 4.5:1, labelled icon-only buttons, and a narrow-screen menu that closes with Esc.

## Decisions due before acceptance criteria

The roadmap places these decisions in this slice, before `/speckit-ac-translate`. Each is decided with the
owner and recorded as a dated line in `## Clarifications` that links its ADR; the detail stays in the ADR.

- Where the account model lives, and how the acting user, the administrator role and (for slice 11) the
  deactivated state reach Application use cases (ADR). Already fixed by owner answers and to be respected:
  account behaviour is Application use cases; the sign-in block is the app's own rule with supplied time, not
  the framework's lockout (FR-008); a password change must take effect at the next page load of other
  sessions (HP-007), which needs session revalidation on every page load rather than the framework's default
  30-minute interval.
- The use-case convention: the shape of the Application surface that the UI and acceptance tests call (ADR).
- The integration-test database approach (ADR).
- The test-project layout.
- Re-runnable demo-data seeding: how and when demo loading is run, and where the demo password is configured.
- ADRs for decisions of this spec that bind later slices: R7 (partly built pages, sections and links only),
  no outbound email and no password recovery, administrator creation at startup, and account behaviour as
  Application use cases.

## Assumptions

- Decision R7 applies to every page and the navigation in this slice; links and sections owned by later slices
  are absent, not disabled or shown as placeholders. Sentences keep the wording of the previews.
- The password rule is the one shown in `design/pages/PageRegister`: at least 8 characters, with a number.
- Locally the administrator credentials and the demo password come from the developer's secret store. How they
  reach the hosted app is part of slice 3's secrets decision.
- The demo data contains no administrator of its own; the administrator variant of the navigation is shown by
  signing in with the configured administrator.
- An account's email is not public. Whether administrators see emails is decided in slice 11, where
  `design/pages/PageAdminAccounts` shows them; in this slice nobody sees another account's email (FR-012).
- Whether an administrator account can also track its own goals is settled in slice 4 (`goals`).
- Deactivating accounts, the deactivated notice on Sign in, feedback and catalog administration are out of
  scope (slices 2 and 11). Community consent and the public display name are out of scope (slice 9).
- Hosting is out of scope (slice 3).
