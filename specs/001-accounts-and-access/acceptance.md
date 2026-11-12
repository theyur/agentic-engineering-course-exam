# Acceptance criteria: Accounts and access

**Feature**: specs/001-accounts-and-access · **Tag**: F001 · **Spec**: spec.md

Conventions used by every item:

- "the anonymous visitor" is the acting user who is not signed in.
- "`x` registered with the password `p`" means the anonymous visitor registered the email `x` with the
  password `p`, and registration succeeded.
- "`x` is signed in with `p`" and "sign-in with `x` and `p` succeeds" both mean that sign-in with `x` and `p`
  succeeded; `x` is then the acting user for the steps that follow.
- "account details of the signed-in account give ..." means the acting user asked for their account details.
  "account details for `x` signed in with `p` give ..." means sign-in with `x` and `p` succeeded and `x` then
  asked for their account details.
- Every failed sign-in states its result, "Email or password is not correct." (FR-007), in the step itself,
  also in Given lines.
- Times are instants supplied to the application as the current time (constitution II), written in UTC.
  Steps without a time may use any current time.
- In administrator creation and demo loading, "absent" means no value is supplied, `""` is the empty text and
  `"   "` is three spaces. In registration, `""` and `"   "` are the email typed as given.
- "the AC database is reset" means the reset entry point of `HT.Persistence` (ADR 0003) drops and rebuilds the
  schema, so no account exists. Only the demo-loading items (AC-055 to AC-060) use it: the demo emails are
  fixed and nothing in this slice removes an account (spec, "What acceptance tests can reach"), so each of
  them starts from an empty database. AC tests run one scenario at a time (ADR 0003), and every other item
  arranges its own accounts.
- Every valid email outside the demo items belongs to one item only, and to one Examples row only. Emails
  that are not valid can never get an account and may repeat. So an email has no account before its item
  starts; no step states this, because no use case can check it without changing state (FR-007 gives an
  unknown email and a wrong password the same result).

## Coverage

| Requirement / scenario | AC items | Note |
|---|---|---|
| FR-001 | AC-001, AC-003, AC-005 | |
| FR-002 | AC-002, AC-003, AC-032, AC-034, AC-040, AC-046, AC-056, AC-058 | |
| FR-003 | AC-004, AC-005, AC-006, AC-061, AC-007, AC-008, AC-009, AC-045, AC-049, AC-054, AC-062, AC-063, AC-060 | |
| FR-004 | AC-001 | |
| FR-005 | AC-010 | |
| FR-006 | AC-006, AC-010, AC-011 | |
| FR-007 | AC-012, AC-013, AC-014, AC-015, AC-025 | |
| FR-008 | AC-014, AC-015, AC-016, AC-017, AC-018, AC-019, AC-020, AC-021, AC-022, AC-023, AC-024, AC-025 | |
| FR-009 | AC-001, AC-027, AC-028 | |
| FR-010 | AC-029, AC-030, AC-037, AC-038 | |
| FR-011 | AC-026, AC-036 | |
| FR-012 | AC-006, AC-027, AC-028, AC-029, AC-030, AC-061, AC-062 | |
| FR-013 | AC-031, AC-032, AC-033, AC-034, AC-035, AC-036, AC-037, AC-038 | |
| FR-014 | AC-039, AC-040, AC-041, AC-042, AC-043, AC-044, AC-045, AC-046, AC-047, AC-048, AC-049, AC-050, AC-051, AC-052, AC-053, AC-054, AC-062, AC-063 | |
| FR-015 | AC-028, AC-030, AC-038 | |
| FR-016 | AC-055, AC-056, AC-057, AC-058, AC-059, AC-060 | |
| US1-AS1 | AC-001 | |
| US1-AS2 | AC-007 | |
| US1-AS3 | AC-002 | |
| US1-AS4 | AC-002 | |
| US1-AS5 | AC-004 | |
| US1-AS6 | AC-008 | |
| US1-AS7 | — | Not testable through the API: Register page content (page scenario; spec "What acceptance tests can reach", ADR 0003 excludes HT.Ui) |
| US1-AS8 | — | Not testable through the API: repeated-password check on the Register page (page scenario; ADR 0003 excludes HT.Ui) |
| US1-AS9 | — | Not testable through the API: placement of the message and the "Sign in" link on the Register page (page scenario; ADR 0003 excludes HT.Ui) |
| US1-AS10 | — | Not testable through the API: browser session and landing page after registering (page scenario; ADR 0003 excludes HT.Ui) |
| US2-AS1 | AC-010 | |
| US2-AS2 | AC-012 | |
| US2-AS3 | AC-013 | |
| US2-AS4 | AC-015 | |
| US2-AS5 | AC-016 | |
| US2-AS6 | AC-017 | |
| US2-AS7 | AC-018 | |
| US2-AS8 | AC-019 | |
| US2-AS9 | — | Not testable through the API: message placement on the Sign in page (page scenario; ADR 0003 excludes HT.Ui) |
| US2-AS10 | — | Not testable through the API: Sign in page content (page scenario; ADR 0003 excludes HT.Ui) |
| US2-AS11 | — | Not testable through the API: browser session kept across restarts (page scenario; ADR 0003 excludes HT.Ui) |
| US2-AS12 | — | Not testable through the API: browser session ended on browser close (page scenario; ADR 0003 excludes HT.Ui) |
| US2-AS13 | — | Not testable through the API: sign-out ends the browser session (page scenario; ADR 0003 excludes HT.Ui) |
| US2-AS14 | — | Not testable through the API: return to the requested page after sign-in (page scenario; ADR 0003 excludes HT.Ui) |
| US3-AS1 | AC-026 | |
| US3-AS2 | AC-027 | |
| US3-AS3 | AC-028 | |
| US3-AS4 | AC-029 | |
| US3-AS5 | AC-030 | |
| US3-AS6 | — | Not testable through the API: redirect from Profile to Sign in (page scenario; ADR 0003 excludes HT.Ui) |
| US3-AS7 | — | Not testable through the API: redirect from Sign in and Register to Home (page scenario; ADR 0003 excludes HT.Ui) |
| US4-AS1 | AC-039 | |
| US4-AS2 | AC-041 | |
| US4-AS3 | AC-042 | |
| US4-AS4 | AC-043 | |
| US4-AS5 | AC-043, AC-044 | |
| US4-AS6 | AC-045 | |
| US4-AS7 | AC-046 | |
| US4-AS8 | AC-047 | |
| US4-AS9 | AC-053 | |
| US4-AS10 | — | Not testable through the API: host startup and the operator log (page scenario; ADR 0003 excludes HT.Ui) |
| US5-AS1 | — | Not testable through the API: visitor navigation (page scenario; ADR 0003 excludes HT.Ui) |
| US5-AS2 | — | Not testable through the API: registered-user navigation (page scenario; ADR 0003 excludes HT.Ui) |
| US5-AS3 | — | Not testable through the API: administrator navigation (page scenario; ADR 0003 excludes HT.Ui) |
| US5-AS4 | — | Not testable through the API: narrow-screen menu items (page scenario; ADR 0003 excludes HT.Ui) |
| US5-AS5 | — | Not testable through the API: closing the narrow-screen menu (page scenario; ADR 0003 excludes HT.Ui) |
| US5-AS6 | — | Not testable through the API: footer content (page scenario; ADR 0003 excludes HT.Ui) |
| US5-AS7 | — | Not testable through the API: Privacy page content (page scenario; ADR 0003 excludes HT.Ui) |
| US5-AS8 | — | Not testable through the API: visitor Home content (page scenario; ADR 0003 excludes HT.Ui) |
| US5-AS9 | — | Not testable through the API: links to the Register page (page scenario; ADR 0003 excludes HT.Ui) |
| US5-AS10 | — | Not testable through the API: link to the Sign in page (page scenario; ADR 0003 excludes HT.Ui) |
| US6-AS1 | AC-031 | |
| US6-AS2 | AC-033 | |
| US6-AS3 | AC-034 | |
| US6-AS4 | AC-035 | |
| US6-AS5 | AC-036 | |
| US6-AS6 | AC-037 | |
| US6-AS7 | AC-038 | |
| US6-AS8 | — | Not testable through the API: sessions in other browsers after a password change (page scenario; ADR 0003 excludes HT.Ui) |
| US6-AS9 | — | Not testable through the API: notice and message placement on the Account content (page scenario; ADR 0003 excludes HT.Ui) |
| US6-AS10 | — | Not testable through the API: signed-in Home content (page scenario; ADR 0003 excludes HT.Ui) |
| US6-AS11 | — | Not testable through the API: Profile header and Account content (page scenario; ADR 0003 excludes HT.Ui) |
| US7-AS1 | AC-057 | |
| US7-AS2 | AC-059 | |
| US7-AS3 | AC-055 | |
| US7-AS4 | AC-056 | |
| US7-AS5 | — | Not testable through the API: inspection of the demo data and of source control, not a use-case result (page scenario) |
| US7-AS6 | — | Not testable through the API: host startup and the operator log (page scenario; ADR 0003 excludes HT.Ui) |

## Acceptance criteria

### AC-001 (FR-001; FR-004; FR-009; US1-AS1) Registration creates a registered user

- When the anonymous visitor registers with the email `new.user@example.com` and the password `walking42`
- Then registration succeeds
- And account details for `new.user@example.com` signed in with `walking42` give the email `new.user@example.com` and the role registered user

### AC-002 (FR-002; US1-AS3; US1-AS4) Registration refuses a password that breaks the password rule

- Given the anonymous visitor
- When they register with the email <email> and the password <password>
- Then the result is "At least 8 characters, with a number."
- And registering <email> with the password `walking42` afterwards succeeds

Examples:

| email | password | why it breaks the rule |
|---|---|---|
| `short.pw@example.com` | `walk4` | 5 characters |
| `seven.pw@example.com` | `walkin4` | 7 characters, with a digit |
| `no.digit@example.com` | `walkingfar` | 10 characters, no digit |
| `eight.nodigit@example.com` | `walkingx` | 8 characters, no digit |

### AC-003 (FR-001; FR-002) Registration accepts a password at the lower bound of the password rule

- Given the anonymous visitor
- When they register with the email <email> and the password <password>
- Then registration succeeds
- And sign-in with <email> and <password> succeeds

Examples:

| email | password | why it meets the rule |
|---|---|---|
| `eight.pw@example.com` | `walking4` | 8 characters, with a digit |
| `digits.pw@example.com` | `12345678` | 8 characters, all digits |

### AC-004 (FR-003; US1-AS5) Registration refuses an email that is not valid

- Given the anonymous visitor
- When they register with the email <email> and the password `walking42`
- Then the result is "Enter a valid email address."
- And sign-in with <email> and `walking42` fails with "Email or password is not correct."

Examples:

| email | why it is not valid |
|---|---|
| `ana.example.com` | no "@" |
| `ana@` | no domain |
| `ana@example` | domain has one part |
| `ana@.` | domain has no non-empty part |
| `ana@.example` | domain has one non-empty part, after an empty one |
| `ana@example.` | domain has one non-empty part, before an empty one |
| `ana@example..com` | empty domain part between two non-empty ones |
| `ana@.example.com` | empty domain part before two non-empty ones |
| `ana@example.com.` | empty domain part after two non-empty ones |
| `ana @example.com` | a space inside |
| `ana@@example.com` | two "@" |
| `@example.com` | nothing before the "@" |
| `""` | empty |
| `"   "` | only spaces, empty after trimming |

### AC-005 (FR-001; FR-003) Registration accepts the shortest valid email and a domain of three parts

- Given the anonymous visitor
- When they register with the email <email> and the password `walking42`
- Then registration succeeds
- And sign-in with <email> and `walking42` succeeds

Examples:

| email |
|---|
| `a@b.c` |
| `ana@sub.example.com` |

### AC-006 (FR-003; FR-006; FR-012) Registration keeps the email in its lower-case form

- When the anonymous visitor registers with the email `Mixed.Case@Example.com` and the password `walking42`
- Then registration succeeds
- And account details for `mixed.case@example.com` signed in with `walking42` give the email `mixed.case@example.com` and the role registered user

### AC-061 (FR-003; FR-012) An account registered with spaces around the email has the trimmed email

- When the anonymous visitor registers with the email `  trim.me@example.com  ` and the password `walking42`
- Then registration succeeds
- And account details for `trim.me@example.com` signed in with `walking42` give the email `trim.me@example.com` and the role registered user

### AC-007 (FR-003; US1-AS2) Registration refuses an email that already has an account, in any case and with spaces

- Given `taken@example.com` registered with the password `original42`
- When the anonymous visitor registers with the email ` Taken@Example.com ` and the password `walking42`
- Then the result is "An account with this email already exists."
- And sign-in with `taken@example.com` and `original42` succeeds
- And sign-in with `taken@example.com` and `walking42` fails with "Email or password is not correct."

### AC-008 (FR-003; US1-AS6) Registration checks the password rule before whether the email is taken

- Given `taken2@example.com` registered with the password `walking42`
- When the anonymous visitor registers with the email `taken2@example.com` and the password `walk4`
- Then the result is "At least 8 characters, with a number."

### AC-009 (FR-003) Registration checks the email before the password rule

- Given the anonymous visitor
- When they register with the email `ana@` and the password `walk4`
- Then the result is "Enter a valid email address."

### AC-010 (FR-005; FR-006; US2-AS1) Sign-in matches the email whatever its case

- Given `ana@example.com` registered with the password `walking42`
- When someone signs in with the email `ANA@example.com` and the password `walking42`
- Then sign-in succeeds
- And account details of the signed-in account give the email `ana@example.com` and the role registered user

### AC-011 (FR-006) Sign-in ignores spaces around the email

- Given `spaced@example.com` registered with the password `walking42`
- When someone signs in with the email `  spaced@example.com  ` and the password `walking42`
- Then sign-in succeeds
- And account details of the signed-in account give the email `spaced@example.com` and the role registered user

### AC-012 (FR-007; US2-AS2) Sign-in with an email that has no account fails with the single message

- When someone signs in with the email `nobody@example.com` and the password `walking42`
- Then the result is "Email or password is not correct."

### AC-013 (FR-007; US2-AS3) Sign-in with a wrong password fails with the single message

- Given `wrong.pw@example.com` registered with the password `walking42`
- When someone signs in with the email `wrong.pw@example.com` and the password `wrong4242`
- Then the result is "Email or password is not correct."

### AC-014 (FR-007; FR-008) An unknown email, a wrong password and a blocked account give the same result

- Given `same.wrong@example.com` and `same.block@example.com` registered with the password `walking42`
- And sign-ins with `same.block@example.com` and `wrong4242` failed at `2026-11-11T09:59:00Z`, `2026-11-11T09:59:15Z`, `2026-11-11T09:59:30Z`, `2026-11-11T09:59:45Z` and `2026-11-11T10:00:00Z`, each with "Email or password is not correct."
- When at `2026-11-11T10:01:00Z` someone signs in with `same.unknown@example.com` and `walking42`, with `same.wrong@example.com` and `wrong4242`, and with `same.block@example.com` and `walking42`
- Then the three results are the same: the same named outcome, with the text "Email or password is not correct."

### AC-015 (FR-007; FR-008; US2-AS4) A blocked account still fails with the correct password 4 minutes 59 seconds after the fifth failure

- Given `block1@example.com` registered with the password `walking42`
- And sign-ins with `block1@example.com` and `wrong4242` failed at `2026-11-11T09:59:00Z`, `2026-11-11T09:59:15Z`, `2026-11-11T09:59:30Z`, `2026-11-11T09:59:45Z` and `2026-11-11T10:00:00Z`, each with "Email or password is not correct."
- When someone signs in with `block1@example.com` and `walking42` at `2026-11-11T10:04:59Z`
- Then the result is "Email or password is not correct."

### AC-016 (FR-008; US2-AS5) The block ends exactly 5 minutes after the fifth failure

- Given `block2@example.com` registered with the password `walking42`
- And sign-ins with `block2@example.com` and `wrong4242` failed at `2026-11-11T09:59:00Z`, `2026-11-11T09:59:15Z`, `2026-11-11T09:59:30Z`, `2026-11-11T09:59:45Z` and `2026-11-11T10:00:00Z`, each with "Email or password is not correct."
- When someone signs in with `block2@example.com` and `walking42` at `2026-11-11T10:05:00Z`
- Then sign-in succeeds

### AC-017 (FR-008; US2-AS6) An attempt during the block does not extend it

- Given `block3@example.com` registered with the password `walking42`
- And sign-ins with `block3@example.com` and `wrong4242` failed at `2026-11-11T09:59:00Z`, `2026-11-11T09:59:15Z`, `2026-11-11T09:59:30Z`, `2026-11-11T09:59:45Z` and `2026-11-11T10:00:00Z`, each with "Email or password is not correct."
- And sign-in with `block3@example.com` and `wrong4242` failed at `2026-11-11T10:03:00Z` with "Email or password is not correct."
- When someone signs in with `block3@example.com` and `walking42` at `2026-11-11T10:05:00Z`
- Then sign-in succeeds

### AC-018 (FR-008; US2-AS7) The end of a block resets the count of failed sign-ins

- Given `block4@example.com` registered with the password `walking42`
- And sign-ins with `block4@example.com` and `wrong4242` failed at `2026-11-11T09:59:00Z`, `2026-11-11T09:59:15Z`, `2026-11-11T09:59:30Z`, `2026-11-11T09:59:45Z` and `2026-11-11T10:00:00Z`, each with "Email or password is not correct."
- And sign-ins with `block4@example.com` and `wrong4242` failed at `2026-11-11T10:05:00Z`, `2026-11-11T10:05:10Z`, `2026-11-11T10:05:20Z` and `2026-11-11T10:05:30Z`, each with "Email or password is not correct."
- When someone signs in with `block4@example.com` and `walking42` at `2026-11-11T10:06:00Z`
- Then sign-in succeeds

### AC-019 (FR-008; US2-AS8) A successful sign-in resets the count of failed sign-ins

- Given `reset@example.com` registered with the password `walking42`
- And sign-ins with `reset@example.com` and `wrong4242` failed at `2026-11-11T10:00:00Z`, `2026-11-11T10:00:10Z`, `2026-11-11T10:00:20Z` and `2026-11-11T10:00:30Z`, each with "Email or password is not correct."
- And sign-in with `reset@example.com` and `walking42` succeeded at `2026-11-11T10:00:40Z`
- And sign-ins with `reset@example.com` and `wrong4242` failed at `2026-11-11T10:00:50Z`, `2026-11-11T10:01:00Z`, `2026-11-11T10:01:10Z` and `2026-11-11T10:01:20Z`, each with "Email or password is not correct."
- When someone signs in with `reset@example.com` and `walking42` at `2026-11-11T10:01:30Z`
- Then sign-in succeeds

### AC-020 (FR-008) Four failed sign-ins in a row do not block

- Given `four@example.com` registered with the password `walking42`
- And sign-ins with `four@example.com` and `wrong4242` failed at `2026-11-11T10:00:00Z`, `2026-11-11T10:00:10Z`, `2026-11-11T10:00:20Z` and `2026-11-11T10:00:30Z`, each with "Email or password is not correct."
- When someone signs in with `four@example.com` and `walking42` at `2026-11-11T10:00:40Z`
- Then sign-in succeeds

### AC-021 (FR-008) Five failed sign-ins in a row block however far apart they are

- Given `apart@example.com` registered with the password `walking42`
- And sign-ins with `apart@example.com` and `wrong4242` failed at `2026-11-11T08:00:00Z`, `2026-11-11T09:00:00Z`, `2026-11-11T10:00:00Z`, `2026-11-11T11:00:00Z` and `2026-11-11T12:00:00Z`, each with "Email or password is not correct."
- When someone signs in with `apart@example.com` and `walking42` at `2026-11-11T12:04:59Z`
- Then the result is "Email or password is not correct."

### AC-022 (FR-008) A failed attempt during the block does not count after the block

- Given `during1@example.com` registered with the password `walking42`
- And sign-ins with `during1@example.com` and `wrong4242` failed at `2026-11-11T09:59:00Z`, `2026-11-11T09:59:15Z`, `2026-11-11T09:59:30Z`, `2026-11-11T09:59:45Z` and `2026-11-11T10:00:00Z`, each with "Email or password is not correct."
- And sign-in with `during1@example.com` and `wrong4242` failed at `2026-11-11T10:03:00Z` with "Email or password is not correct."
- And sign-ins with `during1@example.com` and `wrong4242` failed at `2026-11-11T10:05:00Z`, `2026-11-11T10:05:10Z`, `2026-11-11T10:05:20Z` and `2026-11-11T10:05:30Z`, each with "Email or password is not correct."
- When someone signs in with `during1@example.com` and `walking42` at `2026-11-11T10:06:00Z`
- Then sign-in succeeds

### AC-023 (FR-008) Five failed attempts during the block do not start a new block

- Given `during5@example.com` registered with the password `walking42`
- And sign-ins with `during5@example.com` and `wrong4242` failed at `2026-11-11T09:59:00Z`, `2026-11-11T09:59:15Z`, `2026-11-11T09:59:30Z`, `2026-11-11T09:59:45Z` and `2026-11-11T10:00:00Z`, each with "Email or password is not correct."
- And sign-ins with `during5@example.com` and `wrong4242` failed at `2026-11-11T10:01:00Z`, `2026-11-11T10:02:00Z`, `2026-11-11T10:03:00Z`, `2026-11-11T10:04:00Z` and `2026-11-11T10:04:30Z`, each with "Email or password is not correct."
- When someone signs in with `during5@example.com` and `walking42` at `2026-11-11T10:05:00Z`
- Then sign-in succeeds

### AC-024 (FR-008) A block applies only to its own account

- Given `block5@example.com` and `unblocked@example.com` registered with the password `walking42`
- And sign-ins with `block5@example.com` and `wrong4242` failed at `2026-11-11T09:59:00Z`, `2026-11-11T09:59:15Z`, `2026-11-11T09:59:30Z`, `2026-11-11T09:59:45Z` and `2026-11-11T10:00:00Z`, each with "Email or password is not correct."
- When someone signs in with `unblocked@example.com` and `walking42` at `2026-11-11T10:01:00Z`
- Then sign-in succeeds

### AC-025 (FR-007; FR-008) Failed sign-ins for an email that has no account block nothing

- Given sign-ins with `ghost@example.com` and `wrong4242` failed at `2026-11-11T09:59:00Z`, `2026-11-11T09:59:15Z`, `2026-11-11T09:59:30Z`, `2026-11-11T09:59:45Z` and `2026-11-11T10:00:00Z`, each with "Email or password is not correct."
- And `ghost@example.com` registered with the password `walking42` after them
- When someone signs in with `ghost@example.com` and `walking42` at `2026-11-11T10:01:00Z`
- Then sign-in succeeds

### AC-026 (FR-011; US3-AS1) The anonymous visitor cannot read account details

- Given the anonymous visitor
- When they ask for account details
- Then the result is "You are not signed in."

### AC-027 (FR-009; FR-012; US3-AS2) A registered user reads their own account details

- Given `details@example.com` registered with the password `walking42`
- And `details@example.com` is signed in with `walking42`
- When they ask for account details
- Then the result is the email `details@example.com` and the role registered user

### AC-028 (FR-009; FR-012; FR-015; US3-AS3) An administrator reads their own account details

- Given administrator creation ran with `lead.admin@example.com` and `curator2026` and gave "administrator created"
- And `lead.admin@example.com` is signed in with `curator2026`
- When they ask for account details
- Then the result is the email `lead.admin@example.com` and the role administrator

### AC-029 (FR-010; FR-012; US3-AS4) A registered user gets only their own account details

- Given `cara@example.com` and `ben@example.com` registered with the password `walking42`
- And `ben@example.com` is signed in with `walking42`
- When they ask for account details
- Then the result is the email `ben@example.com` and the role registered user
- And the result does not contain `cara@example.com`

### AC-030 (FR-010; FR-012; FR-015; US3-AS5) An administrator gets only their own account details

- Given `dana@example.com` registered with the password `walking42`
- And administrator creation ran with `audit.admin@example.com` and `curator2026` and gave "administrator created"
- And `audit.admin@example.com` is signed in with `curator2026`
- When they ask for account details
- Then the result is the email `audit.admin@example.com` and the role administrator
- And the result does not contain `dana@example.com`

### AC-031 (FR-013; US6-AS1) A signed-in user changes their password

- Given `change@example.com` registered with the password `walking42`
- And `change@example.com` is signed in with `walking42`
- When they change their password giving `walking42` as the current password and `running42` as the new one
- Then the change succeeds
- And sign-in with `change@example.com` and `running42` succeeds
- And sign-in with `change@example.com` and `walking42` fails with "Email or password is not correct."

### AC-032 (FR-002; FR-013) A password change accepts a new password at the lower bound of the password rule

- Given `eight.new@example.com` registered with the password `walking42`
- And `eight.new@example.com` is signed in with `walking42`
- When they change their password giving `walking42` as the current password and `running4` (8 characters, with a digit) as the new one
- Then the change succeeds
- And sign-in with `eight.new@example.com` and `running4` succeeds

### AC-033 (FR-013; US6-AS2) A password change with a wrong current password is refused

- Given `wrong.current@example.com` registered with the password `walking42`
- And `wrong.current@example.com` is signed in with `walking42`
- When they change their password giving `wrong4242` as the current password and `running42` as the new one
- Then the result is "The current password is not correct."
- And sign-in with `wrong.current@example.com` and `walking42` succeeds
- And sign-in with `wrong.current@example.com` and `running42` fails with "Email or password is not correct."

### AC-034 (FR-002; FR-013; US6-AS3) A password change to a password that breaks the password rule is refused

- Given <email> registered with the password `walking42`
- And <email> is signed in with `walking42`
- When they change their password giving `walking42` as the current password and <new password> as the new one
- Then the result is "At least 8 characters, with a number."
- And sign-in with <email> and `walking42` succeeds

Examples:

| email | new password | why it breaks the rule |
|---|---|---|
| `weak.new@example.com` | `runningfar` | 10 characters, no digit |
| `weak.new7@example.com` | `runnin4` | 7 characters, with a digit |

### AC-035 (FR-013; US6-AS4) A password change checks the current password before the password rule

- Given `both.wrong@example.com` registered with the password `walking42`
- And `both.wrong@example.com` is signed in with `walking42`
- When they change their password giving `wrong4242` as the current password and `run` as the new one
- Then the result is "The current password is not correct."

### AC-036 (FR-011; FR-013; US6-AS5) The anonymous visitor cannot change a password, whatever the new password

- Given the anonymous visitor
- When they change a password giving `walking42` as the current password and <new password> as the new one
- Then the result is "You are not signed in."

Examples:

| new password | why it matters |
|---|---|
| `running42` | meets the password rule |
| `run` | breaks the password rule, which is checked only for a signed-in user |

### AC-037 (FR-010; FR-013; US6-AS6) A registered user cannot change another account's password

- Given `erin@example.com` registered with the password `walking42`
- And `finn@example.com` registered with the password `cycling42`
- And `finn@example.com` is signed in with `cycling42`
- When they change their password giving `walking42` as the current password and `running42` as the new one
- Then the result is "The current password is not correct."
- And sign-in with `erin@example.com` and `walking42` succeeds
- And sign-in with `finn@example.com` and `cycling42` succeeds

### AC-038 (FR-010; FR-013; FR-015; US6-AS7) An administrator cannot change another account's password

- Given `gale@example.com` registered with the password `walking42`
- And administrator creation ran with `keeper.admin@example.com` and `curator2026` and gave "administrator created"
- And `keeper.admin@example.com` is signed in with `curator2026`
- When they change their password giving `walking42` as the current password and `running42` as the new one
- Then the result is "The current password is not correct."
- And sign-in with `gale@example.com` and `walking42` succeeds
- And sign-in with `keeper.admin@example.com` and `curator2026` succeeds

### AC-039 (FR-014; US4-AS1) Administrator creation creates an administrator

- When administrator creation runs with the email `first.admin@example.com` and the password `curator2026`
- Then the result is "administrator created"
- And account details for `first.admin@example.com` signed in with `curator2026` give the email `first.admin@example.com` and the role administrator

### AC-040 (FR-002; FR-014) Administrator creation accepts a password at the lower bound of the password rule

- When administrator creation runs with the email `bound.admin@example.com` and the password `curator1` (8 characters, with a digit)
- Then the result is "administrator created"
- And account details for `bound.admin@example.com` signed in with `curator1` give the role administrator

### AC-041 (FR-014; US4-AS2) Administrator creation leaves an existing administrator unchanged

- Given administrator creation ran with `second.admin@example.com` and `curator2026` and gave "administrator created"
- When administrator creation runs with the email `second.admin@example.com` and the password `other2026`
- Then the result is "administrator already exists"
- And sign-in with `second.admin@example.com` and `curator2026` succeeds
- And sign-in with `second.admin@example.com` and `other2026` fails with "Email or password is not correct."

### AC-042 (FR-014; US4-AS3) Administrator creation does not turn a registered user into an administrator

- Given `claim@example.com` registered with the password `walking42`
- When administrator creation runs with the email `claim@example.com` and the password `curator2026`
- Then the result is "email belongs to a registered user"
- And account details for `claim@example.com` signed in with `walking42` give the email `claim@example.com` and the role registered user

### AC-043 (FR-014; US4-AS4; US4-AS5) Administrator creation without an email or a password configures nothing

- Given the email <email> and the password <password>, where absent means that no value is supplied
- When administrator creation runs with them
- Then the result is "no administrator configured"

Examples:

| email | password |
|---|---|
| absent | absent |
| `fourth.admin@example.com` | absent |
| absent | `curator2026` |
| `""` | `curator2026` |
| `"   "` | `curator2026` |
| `spaces.admin@example.com` | `"   "` |
| `sixth.admin@example.com` | `""` |

### AC-044 (FR-014; US4-AS5) A password of only spaces creates no account

- Given administrator creation ran with the email `fifth.admin@example.com` and the password `"   "` and gave "no administrator configured"
- When the anonymous visitor registers with the email `fifth.admin@example.com` and the password `walking42`
- Then registration succeeds
- And account details for `fifth.admin@example.com` signed in with `walking42` give the email `fifth.admin@example.com` and the role registered user

### AC-045 (FR-003; FR-014; US4-AS6) Administrator creation refuses an email that is not valid

- When administrator creation runs with the email `admin.example.com` and the password `curator2026`
- Then the result is "configured email is not valid"

### AC-046 (FR-002; FR-014; US4-AS7) Administrator creation refuses a password that breaks the password rule

- When administrator creation runs with the email <email> and the password <password>
- Then the result is "configured password does not meet the password rule"
- And registering <email> with the password `walking42` afterwards succeeds
- And account details for <email> signed in with `walking42` give the role registered user

Examples:

| email | password | why it breaks the rule |
|---|---|---|
| `seventh.admin@example.com` | `curatorship` | 11 characters, no digit |
| `short.admin@example.com` | `curat42` | 7 characters, with a digit |

### AC-047 (FR-014; US4-AS8) Administrator creation finds an existing administrator before it checks the password rule

- Given administrator creation ran with `eighth.admin@example.com` and `curator2026` and gave "administrator created"
- When administrator creation runs with the email `eighth.admin@example.com` and the password `curatorship`
- Then the result is "administrator already exists"
- And sign-in with `eighth.admin@example.com` and `curator2026` succeeds

### AC-048 (FR-014) Administrator creation checks for a missing password before the email

- When administrator creation runs with the email `admin.example.com` and the password absent, that is, with no password supplied
- Then the result is "no administrator configured"

### AC-049 (FR-003; FR-014) Administrator creation checks the email before the password rule

- When administrator creation runs with the email `admin.example.com` and the password `curatorship`
- Then the result is "configured email is not valid"

### AC-050 (FR-014) Administrator creation finds a registered user before it checks the password rule

- Given `claim2@example.com` registered with the password `walking42`
- When administrator creation runs with the email `claim2@example.com` and the password `curatorship`
- Then the result is "email belongs to a registered user"
- And sign-in with `claim2@example.com` and `walking42` succeeds

### AC-051 (FR-014) Administrator creation checks for a missing password before it looks for an existing administrator

- Given administrator creation ran with `absent.pw.admin@example.com` and `curator2026` and gave "administrator created"
- When administrator creation runs with the email `absent.pw.admin@example.com` and the password absent, that is, with no password supplied
- Then the result is "no administrator configured"
- And sign-in with `absent.pw.admin@example.com` and `curator2026` succeeds

### AC-052 (FR-014) Administrator creation checks for a missing password before it looks for a registered user

- Given `absent.pw.user@example.com` registered with the password `walking42`
- When administrator creation runs with the email `absent.pw.user@example.com` and the password absent, that is, with no password supplied
- Then the result is "no administrator configured"
- And account details for `absent.pw.user@example.com` signed in with `walking42` give the role registered user

### AC-053 (FR-014; US4-AS9) A new email creates an additional administrator

- Given administrator creation ran with `ninth.admin@example.com` and `curator2026` and gave "administrator created"
- When administrator creation runs with the email `tenth.admin@example.com` and the password `curator2026`
- Then the result is "administrator created"
- And account details for `ninth.admin@example.com` signed in with `curator2026` give the role administrator
- And account details for `tenth.admin@example.com` signed in with `curator2026` give the role administrator

### AC-054 (FR-003; FR-014) Administrator creation matches the email whatever its case and spaces

- Given administrator creation ran with `case.admin@example.com` and `curator2026` and gave "administrator created"
- When administrator creation runs with the email ` Case.Admin@Example.com ` and the password `other2026`
- Then the result is "administrator already exists"
- And sign-in with `case.admin@example.com` and `curator2026` succeeds

### AC-062 (FR-003; FR-012; FR-014) Administrator creation keeps the email in its trimmed, lower-case form

- When administrator creation runs with the email ` Boss.Admin@Example.com ` and the password `curator2026`
- Then the result is "administrator created"
- And account details for `boss.admin@example.com` signed in with `curator2026` give the email `boss.admin@example.com` and the role administrator

### AC-063 (FR-003; FR-014) Administrator creation finds a registered user whatever the case and spaces of the email

- Given `claim3@example.com` registered with the password `walking42`
- When administrator creation runs with the email ` Claim3@Example.com ` and the password `curator2026`
- Then the result is "email belongs to a registered user"
- And account details for `claim3@example.com` signed in with `walking42` give the email `claim3@example.com` and the role registered user

### AC-055 (FR-016; US7-AS3) Demo loading without a demo password creates no account

- Given the AC database is reset, so no account exists
- When demo loading runs with the demo password <demo password>, where absent means that no value is supplied
- Then the result is "no demo password configured"
- And demo loading run afterwards with the demo password `example2026` gives "created" for `demo1@example.com`, for `demo2@example.com` and for `demo3@example.com`

Examples:

| demo password |
|---|
| absent |
| `""` |
| `"   "` |

### AC-056 (FR-002; FR-016; US7-AS4) Demo loading with a password that breaks the password rule creates no account

- Given the AC database is reset, so no account exists
- When demo loading runs with the demo password <demo password>
- Then the result is "demo password does not meet the password rule"
- And demo loading run afterwards with the demo password `example2026` gives "created" for `demo1@example.com`, for `demo2@example.com` and for `demo3@example.com`

Examples:

| demo password | why it breaks the rule |
|---|---|
| `examplepass` | 11 characters, no digit |
| `exampl1` | 7 characters, with a digit |

### AC-057 (FR-016; US7-AS1) Demo loading creates the three demo accounts

- Given the AC database is reset, so no account exists
- When demo loading runs with the demo password `example2026`
- Then the result is "created" for `demo1@example.com`, for `demo2@example.com` and for `demo3@example.com`, and names no other email
- And account details for `demo1@example.com` signed in with `example2026` give the email `demo1@example.com` and the role registered user
- And account details for `demo2@example.com` signed in with `example2026` give the email `demo2@example.com` and the role registered user
- And account details for `demo3@example.com` signed in with `example2026` give the email `demo3@example.com` and the role registered user

### AC-058 (FR-002; FR-016) Demo loading accepts a demo password at the lower bound of the password rule

- Given the AC database is reset, so no account exists
- When demo loading runs with the demo password `example1` (8 characters, with a digit)
- Then the result is "created" for `demo1@example.com`, for `demo2@example.com` and for `demo3@example.com`, and names no other email
- And sign-in with `demo1@example.com` and `example1` succeeds

### AC-059 (FR-016; US7-AS2) Demo loading run again changes nothing

- Given the AC database is reset, so no account exists
- And demo loading ran with the demo password `example2026` and gave "created" for `demo1@example.com`, for `demo2@example.com` and for `demo3@example.com`
- And `demo3@example.com` is signed in with `example2026`
- And `demo3@example.com` changed its password giving `example2026` as the current password and `changed2026` as the new one
- When demo loading runs again with the demo password `example2026`
- Then the result is "already exists" for `demo1@example.com`, for `demo2@example.com` and for `demo3@example.com`, and names no other email
- And sign-in with `demo3@example.com` and `changed2026` succeeds
- And sign-in with `demo3@example.com` and `example2026` fails with "Email or password is not correct."
- And sign-in with `demo1@example.com` and `example2026` succeeds
- And sign-in with `demo2@example.com` and `example2026` succeeds

### AC-060 (FR-003; FR-016) Demo loading decides per demo email, and a demo email registered earlier keeps its owner's password

- Given the AC database is reset, so no account exists
- And ` Demo2@Example.com ` registered with the password `owner2026`
- When demo loading runs with the demo password `example2026`
- Then the result is "created" for `demo1@example.com`, "already exists" for `demo2@example.com` and "created" for `demo3@example.com`, and names no other email
- And sign-in with `demo2@example.com` and `owner2026` succeeds
- And sign-in with `demo2@example.com` and `example2026` fails with "Email or password is not correct."
- And sign-in with `demo1@example.com` and `example2026` succeeds
- And sign-in with `demo3@example.com` and `example2026` succeeds
