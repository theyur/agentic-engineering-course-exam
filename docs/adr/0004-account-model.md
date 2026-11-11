# 0004 — Account model: a domain account behind an Identity-backed store

**Status:** Accepted, 2026-11-11. Settles the account-model row of the roadmap (ARCHITECTURE §3, §4, §9) for
slice 1, `specs/001-accounts-and-access`. Also records the spec's decision that account behaviour is
Application use cases. The shape of those use cases is ADR 0005.

## Context

Slice 1 adds accounts: registration, sign-in with a temporary block, password change, account details,
administrator creation and demo loading. The rules that constrain where the account lives:

- ARCHITECTURE §2, §4 and CLAUDE.md: ASP.NET Core Identity for local email/password; no custom crypto,
  password storage or session handling.
- Owner answers in the spec's `## Clarifications`:
  - account behaviour is Application use cases, so acceptance tests can check it (ADR 0003); the host keeps
    only the browser session;
  - the sign-in block is the app's own rule, judged against the time supplied to the application, not
    Identity's lockout (FR-008);
  - a password change must take effect at the next page load of every other session (HP-007).
- Constitution II: Domain and Application never read the clock; time is supplied.
- Constitution III: use cases take the acting user as input and act only on that account.
- Slice 11 adds a deactivated state that must stop sign-in and end open sessions (ARCHITECTURE §4).

## Options

1. **Domain account, Identity-backed store (chosen).** `HT.Domain` holds the account and its rules;
   `HT.Application` defines a store; `HT.Persistence` implements it on Identity's user records.
2. **The Identity user is the account.** No domain account; the Identity user type in `HT.Persistence` is
   the model and the rules live in Application services. Fewer types, but the rules sit next to the
   framework and the acceptance-test surface would expose nothing of the account.
3. **Two records.** A domain-owned account table linked one-to-one to Identity's user table, which keeps only
   the login and the password hash. Every account is two rows that must stay in step, and concurrent
   registration (HP-020) needs a transaction across both.

## Decision

- **Domain.** `HT.Domain` has the account: its identifier, its email, its role (registered user or
  administrator, exactly one) and its sign-in block state (the count of failed sign-ins in a row and the start
  of a current block). The domain also owns the rules, as functions of their inputs and the supplied time:
  - the email rule and its normal form (FR-003): trimmed, compared case-insensitively;
  - the password rule (FR-002), checked on the candidate password; the domain never stores or hashes it;
  - the sign-in block (FR-008): 5 failures in a row start a 5-minute block that ends exactly 5 minutes after
    the fifth failure; attempts during the block do not extend it; a success or the end of a block resets the
    count.

  Slice 11 adds the deactivated state to the same account.
- **Application.** `HT.Application` defines the account store it needs (for example: find by email, create
  with a password, verify a password, change a password, save the block state). The account use cases live in
  Application and are the only code that changes accounts: registration, the sign-in check, password change,
  account details, administrator creation and demo loading.
- **Persistence.** `HT.Persistence` implements the store with Identity's user manager and its own user type
  (derived from Identity's user), stored through the application's EF Core context and mapped to the domain
  account:
  - the role and the block state are columns of that user type. Identity's role tables are not used, because
    an account has exactly one role and Identity's roles allow several;
  - the user name is the account's normalized email, so Identity's unique user-name index makes emails unique
    in the database (HP-020);
  - Identity's lockout is turned off; the domain block rule replaces it;
  - the domain rules decide, so Identity's own rules never refuse what they accept. Identity's password
    options are set no stricter than FR-002 (at least 8 characters with a digit; no upper-case,
    lower-case or non-alphanumeric requirement), and its allowed user-name characters are not restricted,
    since the email rule (FR-003) decides which emails are valid. The use cases check the domain rules first,
    so an Identity refusal never reaches a user;
  - a password change also renews the user's security stamp.
- **Host.** `HT.Ui` keeps only the browser session. After the sign-in use case succeeds, it issues the
  authentication cookie, with the role as a claim; it never checks a password itself. It builds the acting
  user from the authenticated session and passes it to the use cases (ADR 0005).
  - A cookie can be set only in an HTTP response, not over an interactive circuit. So Sign in, Sign out and
    the change-password form are server-rendered forms that post to the host.
  - After the password-change use case succeeds, the same response issues the cookie again with the new
    security stamp (Identity's refresh sign-in), so the session that made the change stays signed in
    (HP-007).
- **Session revalidation.** The host uses Identity's security-stamp validator with a zero interval, so the
  stamp is checked on every page load instead of every 30 minutes. A password change therefore ends other
  sessions at their next page load. An interactive page that is already open is revalidated at least every
  30 minutes (HP-007). Slice 11 uses the same check for the deactivated state.
- **Where the Identity user type is used in `HT.Ui`.** Only in the host's composition code and in the
  account endpoints that issue, refresh and end the cookie. Blazor components never use it. Architecture
  tests keep components away from `HT.Persistence` (ARCHITECTURE §6).

## Consequences

- The account exists in two shapes, the domain account and the Identity user type, with a mapping between
  them in `HT.Persistence`. The mapping is covered by integration tests (ADR 0006).
- An Identity default that is stricter than a domain rule would refuse valid input with an error the spec
  does not define. Integration tests therefore create accounts with the spec's example passwords and with an
  email outside Identity's default user-name characters (for example one with an apostrophe).
- Identity packages are referenced only by `HT.Persistence` and `HT.Ui`. Architecture tests enforce that
  `HT.Domain` and `HT.Application` do not depend on Identity, EF Core or ASP.NET Core (constitution V).
- The `HT.Persistence` composition entry point of ADR 0003 registers Identity's core services and the store,
  so acceptance and integration tests can build the application without `HT.Ui`.
- The block state is written on every failed or successful sign-in, so sign-in is a read and a write.
- Revalidating on every page load costs one lookup per request. That is accepted for a demo-scale app.
