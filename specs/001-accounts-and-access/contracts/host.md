# Contract: Host routes, forms, configuration and operator log

**Feature**: specs/001-accounts-and-access · **Plan**: [../plan.md](../plan.md)

What `HT.Ui` exposes to browsers and operators in this slice. Verified by `HT.UiTests` (ADR 0006), not by
acceptance tests.

## Routes

| Path | Who | Render | Behaviour | Requirement |
|---|---|---|---|---|
| `/` | anyone | static | visitor Home or signed-in Home ("Today") | HP-009, HP-011, HP-013 |
| `/sign-in` | anonymous | static form | `returnUrl` query parameter (local paths only); a signed-in user is redirected to `/` | HP-003, HP-005, HP-009 |
| `/register` | anonymous | static form | after success: signed in, redirected to `/`; a signed-in user is redirected to `/` | HP-001, HP-002, HP-009 |
| `/profile` | signed in (`[Authorize]`) | static form | header and Account content; change-password form | HP-008, HP-014 |
| `/privacy` | anyone | static | Privacy page | HP-012 |
| `POST /account/sign-out` | signed in | endpoint | ends the cookie, redirects to `/` | HP-006 |
| `/not-found`, `/Error` | anyone | static | EmptyState in the normal layout, with brand-book wording designed in `design/` first | HP-016 |

An anonymous request to `/profile` is redirected to `/sign-in?returnUrl=%2Fprofile` without rendering Profile.
`returnUrl` is followed only when `RedirectHttpResult.IsLocalUrl` accepts it (research R-10).

## Navigation (`SiteNav`, the only interactive component)

| Audience | Wide screen | Narrow-screen menu (under 860px) | Requirement |
|---|---|---|---|
| anonymous visitor | "Habit Tracker", "Sign in", "Register" | Sign in, Register | US5-AS1, HP-010 |
| registered user | "Habit Tracker", "Today", avatar menu: Profile, Sign out | Today, Profile, Sign out | US5-AS2, HP-010 |
| administrator | same as registered user, no Admin link | same as registered user | US5-AS3, HP-010 |

- The avatar shows the first letter of the `Name` claim (the account's email), upper-cased (HP-014).
- Each menu button has `aria-expanded`, `aria-controls` and an `aria-label`; a menu closes on Esc, a click
  outside it, or choosing an item, and focus returns to its button (US5-AS5, SC-007).
- Sign out in either menu is a form posting to `POST /account/sign-out` with an antiforgery token.

## `AccountSession` (`HT.Ui/Account`, the only code that writes the cookie)

| Member | Used by | Does |
|---|---|---|
| `SignInAsync(ActingUser signedIn, bool keepSignedIn)` | Sign in and Register pages, with the `ActingUser` of a `SignedIn` or `Registered` result | loads the `HtUser` by `signedIn.AccountId` and signs it in, persistent when `keepSignedIn` |
| `RefreshSignInAsync(ActingUser actingUser)` | Profile page after `Changed` | re-issues the cookie with the new security stamp, keeping its persistence |
| `SignOutAsync()` | `POST /account/sign-out` | ends the cookie |

Registration returns an `AccountId`; the Register page builds `ActingUser.ForAccount(id, Role.RegisteredUser)`
from it and calls `SignInAsync(…, keepSignedIn: false)`.

## Forms (static server-rendered, antiforgery on every form)

Each form is a component (`SignInForm`, `RegisterForm`, `ChangePasswordForm`) that calls its use case and
renders the refusals; its page calls `AccountSession` when the form reports success (research R-08).

| Form | Fields | On success | On refusal |
|---|---|---|---|
| Sign in | `Email`, `Password`, `KeepSignedIn` | persistent cookie if `KeepSignedIn`, else session cookie; redirect to `returnUrl` or `/` | "Email or password is not correct." under the password field; email kept |
| Register | `Email`, `Password`, `RepeatPassword` | signed in with a session cookie; redirect to `/` | message next to its field; taken email adds a "Sign in" link; "The passwords do not match." next to the repeated password (checked on the page, before the use case); email kept |
| Change password | `CurrentPassword`, `NewPassword`, `RepeatNewPassword` | cookie refreshed with the new security stamp; "Your password is changed."; fields empty | message next to the current or new password; "The passwords do not match." next to the repeated password |
| Sign out | none | redirect to `/` | — |

## Session

- Cookie scheme `Identity.Application`, `Secure`, `HttpOnly`, `SameSite=Lax`; when kept, a persistent cookie
  re-issued with a full 14-day window on every authenticated request (so it ends 14 days after the last visit,
  HP-004); a session cookie otherwise. The app is served over HTTPS (`https` launch profile).
- Security stamp checked on every request; the interactive circuit revalidated every 30 minutes (HP-007).
- `Cache-Control: no-store` on every response to a signed-in user, set by the cookie handler when it re-issues
  the cookie, static assets included (research R-09); asserted by a host test (HP-008).

## Configuration

| Key | Where it comes from | Used by |
|---|---|---|
| `ConnectionStrings:HabitTracker` | `appsettings.Development.json` locally (integrated security, no secret); slice 3 for hosting | `AddHtPersistence` |
| `Operator:AdministratorEmail` | User Secrets locally (HP-019) | administrator creation |
| `Operator:AdministratorPassword` | User Secrets locally (HP-019) | administrator creation |
| `Operator:DemoPassword` | User Secrets locally (HP-019) | demo loading |

## Operator log (category `HT.Ui.Operator.StartupTasks`)

| Event | Level | Message template |
|---|---|---|
| migrations applied (Development) | Information | `Schema migrations applied: {Count}` (the value `MigrateAsync` returns) |
| administrator creation result | Information | `Administrator creation: {Result} ({Email})`; `({Email})` omitted when no email is configured |
| demo loading refused | Information | `Demo loading: {Result}` (for `NoPassword` and `PasswordRuleNotMet`) |
| demo loading done | Information | `Demo account {Email}: {Result}`, one line per demo email (for `Loaded`) |
| fault in a step | Error | `{Step} failed; the app starts without it`, with the exception |

No message contains a password or any configuration value other than the administrator email (HP-017).
