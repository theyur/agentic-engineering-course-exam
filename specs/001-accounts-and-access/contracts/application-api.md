# Contract: Application surface and Persistence entry points

**Feature**: specs/001-accounts-and-access · **Plan**: [../plan.md](../plan.md)

The public API that `HT.Ui`, the integration tests and the acceptance tests call (ADR 0003, ADR 0005). Domain
types are in [../data-model.md](../data-model.md). Names below are the contract; parameter names may differ.
Every method is `async` and takes a `CancellationToken` last.

## Composition

```csharp
// HT.Application
public static IServiceCollection AddHtApplication(this IServiceCollection services);
// Registers AccountService, OperatorService and DemoDataService as scoped services (they use the
// scoped account store), and TimeProvider.System as a singleton with TryAddSingleton, so a test can
// register a fake TimeProvider before or after it (the last registration wins for injection).

// HT.Persistence
public static IServiceCollection AddHtPersistence(this IServiceCollection services, string connectionString);
// Registers the DbContext and the account store (scoped) and Identity's user services.
public static class HtDatabase
{
    public static Task ResetAsync(string connectionString, CancellationToken ct);           // drop + all migrations; tests only
    public static Task<int> MigrateAsync(IServiceProvider services, CancellationToken ct);  // applies pending migrations, returns how many; host in Development
}
```

The account store port `IAccountStore` is internal (research R-13) and not part of this contract.

A test builds the application with `new ServiceCollection().AddHtApplication().AddHtPersistence(cs)`, plus
logging, and resolves the services from a scope.

## `AccountService` (anonymous visitors and signed-in users)

| Method | Acting user | Outcomes, in check order | Requirement |
|---|---|---|---|
| `RegisterAsync(string? email, string? password)` | none | `InvalidEmail` · `PasswordRuleNotMet` · `EmailTaken` · `Registered` (with `AccountId`) | FR-001 to FR-004 |
| `SignInAsync(string? email, string? password)` | none | `Failed` · `SignedIn` (with `ActingUser`) | FR-005 to FR-008 |
| `GetAccountDetailsAsync(ActingUser actingUser)` | first parameter | `NotSignedIn` · `Found` (with `AccountDetails`: `Email` as `string` in normal form, `Role`) | FR-009 to FR-012, FR-015 |
| `ChangePasswordAsync(ActingUser actingUser, string? currentPassword, string? newPassword)` | first parameter | `NotSignedIn` · `CurrentPasswordWrong` · `PasswordRuleNotMet` · `Changed` | FR-002, FR-010, FR-011, FR-013 |

- `SignInAsync` reads the current time from the injected `TimeProvider` (FR-008). Unknown email, wrong
  password and blocked account all give `Failed` with the same text (FR-007, AC-014); nothing is saved for an
  unknown email. Parallel attempts for one account are each counted once (compare-and-swap with retry,
  research R-05).
- `GetAccountDetailsAsync` and `ChangePasswordAsync` act only on `actingUser.AccountId` and take no other
  account identifier (FR-010). An acting user whose account no longer exists gives `NotSignedIn`.

## `OperatorService` (host and tests only)

| Method | Outcomes, in check order | Requirement |
|---|---|---|
| `CreateAdministratorAsync(string? email, string? password)` | `NotConfigured` · `InvalidEmail` · `AlreadyExists` · `BelongsToRegisteredUser` · `PasswordRuleNotMet` · `Created` | FR-014 |

"Missing" is `null`, empty, or only whitespace, for either value.

## `DemoDataService` (host and tests only)

| Method | Outcomes | Requirement |
|---|---|---|
| `LoadAsync(string? demoPassword)` | `NoPassword` · `PasswordRuleNotMet` · `Loaded` with one `DemoAccountResult` per demo email (`Email` as `string` in normal form, `Outcome`: `Created` or `AlreadyExists`, `Message`); `Loaded` itself has no text | FR-016 |

- The demo emails are `demo1@example.com`, `demo2@example.com`, `demo3@example.com`, in that order, exposed as
  `DemoDataService.DemoEmails`.
- An existing account (any role) is left unchanged and reported as `AlreadyExists`.
- Later slices add their demo data to `LoadAsync` (ADR 0007).

## Result shape

Every method returns its own sealed result class:

```csharp
public sealed class RegistrationResult
{
    public RegistrationOutcome Outcome { get; }   // enum with the outcome names above
    public string? Message { get; }               // the spec's text; null for Registered (no text in the spec)
    public AccountId? AccountId { get; }          // set only for Registered
}
```

The other results follow the same pattern: `SignInResult` (`ActingUser?`), `AccountDetailsResult`
(`AccountDetails?`), `PasswordChangeResult`, `AdministratorCreationResult`, `DemoLoadingResult`
(`IReadOnlyList<DemoAccountResult>`). No refusal is an exception (ADR 0005).

## Texts

Defined once as `public const string` in `HT.Application` (ADR 0005), worded as in the spec:

| Class | Constant | Text |
|---|---|---|
| `AccountTexts` | `InvalidEmail` | Enter a valid email address. |
| `AccountTexts` | `PasswordRule` | At least 8 characters, with a number. |
| `AccountTexts` | `EmailTaken` | An account with this email already exists. |
| `AccountTexts` | `SignInFailed` | Email or password is not correct. |
| `AccountTexts` | `NotSignedIn` | You are not signed in. |
| `AccountTexts` | `CurrentPasswordWrong` | The current password is not correct. |
| `OperatorTexts` | `AdministratorNotConfigured` | no administrator configured |
| `OperatorTexts` | `AdministratorEmailInvalid` | configured email is not valid |
| `OperatorTexts` | `AdministratorAlreadyExists` | administrator already exists |
| `OperatorTexts` | `EmailBelongsToRegisteredUser` | email belongs to a registered user |
| `OperatorTexts` | `AdministratorPasswordRule` | configured password does not meet the password rule |
| `OperatorTexts` | `AdministratorCreated` | administrator created |
| `OperatorTexts` | `DemoPasswordNotConfigured` | no demo password configured |
| `OperatorTexts` | `DemoPasswordRule` | demo password does not meet the password rule |
| `OperatorTexts` | `DemoAccountCreated` | created |
| `OperatorTexts` | `DemoAccountAlreadyExists` | already exists |

`Message` carries the text above for every outcome that has one. It is `null` for the five successes the spec
gives no text: `Registered`, `SignedIn`, `Found`, `Changed` (password change) and `Loaded`. For those, the
pages show the signed-in Home (HP-002), the account details and "Your password is changed." (HP-014), and the
operator log lists the per-email results.
