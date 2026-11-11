# 0009 — No outbound email: no verification and no password recovery in version 1

**Status:** Accepted, 2026-11-11. Settles the email part of the authentication-UX row of the roadmap
(ARCHITECTURE §9: "email verification, password recovery, and outbound email infrastructure") in slice 1,
`specs/001-accounts-and-access`. Applies to version 1.

## Context

- ARCHITECTURE §9 leaves authentication UX, email verification, password recovery and outbound email open.
- SPEC §9 excludes email, push, SMS and other externally delivered reminders. SPEC §6 says dashboard
  indicators are in-app, not outbound. Nothing in SPEC needs an email to be sent.
- `design/pages/PageSignIn` shows "Forgot password?", and `design/README.md` lists "password reset pages"
  as not designed yet.
- The app is a low-cost demo for reviewers (ARCHITECTURE §7). Sending email needs a provider, a secret,
  a sender domain and handling for undelivered mail.

## Options

1. **No outbound email (chosen).** No email verification, no password recovery.
2. **Password recovery by email.** Users can reset a forgotten password, but this needs an email provider and
   its secrets, reset pages that are not designed yet, and recovery tokens.
3. **Recovery without email (an administrator resets the password).** This needs no provider, but it gives
   administrators power over other people's accounts, which the privacy boundary (SPEC §3, §8) does not
   provide for.

## Decision

- The app sends no email in version 1.
- **No email verification.** An account can be used as soon as it is registered. An email is a sign-in
  identifier, not a proven address.
- **No password recovery.** There are no reset pages. Before Sign in is built, "Forgot password?" is removed
  from `design/pages/PageSignIn` and "password reset pages" from the "not designed yet" list of
  `design/README.md` (slice 1's spec, "Design changes before build").
- A signed-in user changes their own password on the Profile Account tab (FR-013).
- Because there is no verification, Register says when an email already has an account ("An account with this
  email already exists."). Hiding it would protect nothing, since anyone can try to register any address.

## Consequences

- A user who forgets their password cannot recover the account. This is accepted for the demo. Demo accounts
  get their password from the operator's configuration (ADR 0007).
- Anyone can register an address they do not own. Nothing is sent to that address, but its real owner can no
  longer register it. This is accepted for the demo.
- No email provider, sender secret or delivery monitoring is needed, which keeps slice 3 (`demo-hosting`)
  simpler.
- Adding verification or recovery later needs a superseding ADR, and design pages for it in `design/` first.
