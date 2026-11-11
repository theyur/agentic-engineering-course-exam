# Data model: Accounts and access

**Feature**: specs/001-accounts-and-access · **Date**: 2026-11-11 · **Plan**: [plan.md](plan.md)

Two shapes of one account (ADR 0004): the domain types in `HT.Domain`, and the Identity user type with its
table in `HT.Persistence`. The mapping between them is the only place both appear.

## Domain (`HT.Domain`, namespace `HT.Domain.Accounts`)

### `AccountId`

- Value: a `Guid`. Readonly record struct. Created by the store, never by a use-case caller.

### `Role`

- Enum: `RegisteredUser`, `Administrator`. Every account has exactly one (FR-009).

### `Email`

- Value: the normal form, a non-empty string (FR-003).
- `Email.TryParse(string? text, out Email email)`: trims, then accepts only text of at most 254 characters
  with no whitespace character, exactly one `@`, at least one character before it, and a domain of at least two parts separated
  by `.`, every part non-empty; the value is the trimmed text in `ToLowerInvariant()` form.
- Equality is by value (ordinal), so two emails that differ only in case or surrounding spaces are equal;
  other Unicode differences (for example composed and decomposed letters) are not folded.
- Examples (AC-004 to AC-006, AC-061): `ana@example.com`, `a@b.c`, `ana@sub.example.com` valid;
  `ana@example..com`, `ana@.example`, `ana @example.com`, `""`, `"   "` invalid.

### `PasswordRule`

- `PasswordRule.IsMet(string? password)`: true when the text has at least 8 characters and at least one
  character `'0'`–`'9'` (FR-002). Nothing else; no trimming. The domain never stores a password.

### `SignInBlock`

- Fields: `FailedCount` (int, 0–5), `BlockedSince` (`DateTimeOffset?`). Readonly record struct;
  `SignInBlock.None` is (0, null).
- Constants: `FailuresToBlock = 5`, `Duration = 5 minutes`.
- `IsBlockedAt(DateTimeOffset now)`: `BlockedSince` is set and `now < BlockedSince + Duration`.
- `SettledAt(DateTimeOffset now)`: returns `None` when a block has ended at `now`, otherwise itself.
- `AfterFailureAt(DateTimeOffset now)`: on a settled, unblocked state, adds 1; at 5 sets `BlockedSince = now`.
- `AfterSuccess()`: `None`.
- Transitions (FR-008):

  | State before (settled at `t`) | Attempt at `t` | State after | Result |
  |---|---|---|---|
  | blocked (`t < BlockedSince + 5 min`) | any password | unchanged | failed |
  | (n, none), n < 4 | wrong password | (n + 1, none) | failed |
  | (4, none) | wrong password | (5, `t`) | failed |
  | (n, none) | correct password | (0, none) | signed in |

  "Settled" means a block that ended at or before `t` was first reset to (0, none).

### `Account`

- Fields: `Id` (`AccountId`), `Email` (`Email`), `Role` (`Role`), `SignInBlock` (`SignInBlock`).
- Immutable; the use cases produce a new value and save it through the store.
- Slice 9 adds community consent and the public display name; slice 11 adds the deactivated state.

### `ActingUser`

- Either the anonymous visitor (`ActingUser.Anonymous`) or a signed-in account
  (`ActingUser.ForAccount(AccountId id, Role role)`).
- `IsAnonymous`, `AccountId` (null for the visitor), `Role` (null for the visitor).
- Built by the host from the session and by tests from a successful sign-in result; never from user input.

## Application port (`HT.Application`, namespace `HT.Application.Accounts`)

`IAccountStore` is defined by Application and implemented by Persistence (ADR 0004). It is `internal`, visible
only to `HT.Persistence`, `HT.UnitTests`, `HT.UiTests` and `HT.IntegrationTests` (research R-13), so no
component can inject it. Every method takes a `CancellationToken` last. Account reads are no-tracking queries,
so they return the database's current state; the methods that need an `HtUser` for the user manager clear the
change tracker before and after their work (research R-05).

| Method | Returns | Notes |
|---|---|---|
| `FindByEmailAsync(Email email)` | `Account?` | lookup by the normal form |
| `FindByIdAsync(AccountId id)` | `Account?` | used only with the acting user's own identifier |
| `CreateAsync(Email email, Role role, string password)` | `CreateAccountOutcome` (`Created` with the `Account`, or `EmailTaken`) | a unique-index violation becomes `EmailTaken` (HP-020) |
| `CheckPasswordAsync(AccountId id, string password)` | `bool` | Identity's password hasher |
| `ChangePasswordAsync(AccountId id, string currentPassword, string newPassword)` | `bool` | renews the security stamp; `false` only if the current password no longer matches |
| `TrySaveSignInBlockAsync(AccountId id, SignInBlock expected, SignInBlock next)` | `bool` | compare-and-swap of the two columns; `false` when the stored state is no longer `expected` (research R-05) |

Integration tests use the real store; unit tests use a hand-written in-memory fake (ADR 0006).

## Persistence (`HT.Persistence`)

### `HtUser : IdentityUser<Guid>` (namespace `HT.Persistence.Identity`)

Identity columns used as is: `Id`, `UserName`, `NormalizedUserName` (unique index), `Email`,
`NormalizedEmail`, `PasswordHash`, `SecurityStamp`, `ConcurrencyStamp`. Unused Identity columns stay at their
defaults (`EmailConfirmed` false, `LockoutEnabled` false, phone and two-factor columns empty).

Added columns:

| Column | Type | Null | Meaning |
|---|---|---|---|
| `Role` | `nvarchar(32)`, from the `Role` enum name | no | registered user or administrator |
| `FailedSignInCount` | `int`, default 0 | no | `SignInBlock.FailedCount` |
| `SignInBlockedSince` | `datetimeoffset` | yes | `SignInBlock.BlockedSince` |

Mapping: `UserName` = `Email` = `NormalizedUserName` = `NormalizedEmail` = the domain normal form (through
`EmailLookupNormalizer`, research R-04); `Id` ↔ `AccountId`.

Column settings:

- `NormalizedUserName` and `NormalizedEmail`: collation `Latin1_General_100_BIN2`, so the unique index compares
  exactly as `Email` equality does (research R-04).
- `FailedSignInCount` and `SignInBlockedSince`: `PropertySaveBehavior.Ignore` after insert, so only
  `TrySaveSignInBlockAsync` writes them and `UserManager.UpdateAsync` never overwrites them (research R-05).

### `HtDbContext : IdentityUserContext<HtUser, Guid>`

- Tables: `AspNetUsers`, `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserTokens` (Identity's user
  tables only; no role tables, research R-02).
- Migration `InitialAccounts` creates them with the added columns.

## Validation rules, by where they are checked

| Rule | Checked in | Requirement |
|---|---|---|
| email form and normal form | `Email.TryParse` | FR-003 |
| password rule | `PasswordRule.IsMet` | FR-002 |
| email unique | Identity's unique index on `NormalizedUserName` (binary collation); `CreateAsync` → `EmailTaken` | FR-003, HP-020 |
| one role per account | `Role` column, not null | FR-009 |
| sign-in block | `SignInBlock`; saved by compare-and-swap with retry in `AccountService.SignInAsync` | FR-008 |
| check order of each use case | the Application services | FR-003, FR-013, FR-014, FR-016 |
