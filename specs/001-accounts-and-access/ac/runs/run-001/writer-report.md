# Writer report — F001 / run-001

## Build

failure, 3 attempts.

Remaining errors (both from one missing package, see "Missing packages"):

- `Support/F001/AccountsDriver.cs(6,28): error CS0234: The type or namespace name 'DependencyInjection' does not exist in the namespace 'Microsoft.Extensions'`
- `Support/F001/AccountsDriver.cs(24,22): error CS0246: The type or namespace name 'ServiceProvider' could not be found`

Attempt 2 replaced the DI composition with a temporary stub, and the build succeeded with 0 warnings and
0 errors. So every other file (all step definitions, context, hooks, parsing) compiles against the
reference assemblies. The stub was removed afterwards and the real composition is back in place. In
normal mode the package probably arrives transitively through the `HT.Persistence` project reference
(EF Core), but ref mode references only `artifacts/refs/*.dll`.

## Coverage

All 63 scenarios in `Features/F001/accounts-and-access.feature` are bound (Scenario Outlines count once).
Files are under `tests/HT.AcceptanceTests/StepDefinitions/F001/`. Shared state and helpers are in
`Support/F001/` (`F001Context.cs`, `AccountsDriver.cs`, `F001Text.cs`, `F001Database.cs`, `F001Hooks.cs`).

| AC tag | Scenario name | Step definition file(s) |
|---|---|---|
| AC-001 | Registration creates a registered user | RegistrationSteps, AccountDetailsSteps |
| AC-002 | Registration refuses a password that breaks the password rule | RegistrationSteps, ResultSteps |
| AC-003 | Registration accepts a password at the lower bound of the password rule | RegistrationSteps, SignInSteps |
| AC-004 | Registration refuses an email that is not valid | RegistrationSteps, ResultSteps, SignInSteps |
| AC-005 | Registration accepts the shortest valid email and a domain of three parts | RegistrationSteps, SignInSteps |
| AC-006 | Registration keeps the email in its lower-case form | RegistrationSteps, AccountDetailsSteps |
| AC-061 | An account registered with spaces around the email has the trimmed email | RegistrationSteps, AccountDetailsSteps |
| AC-007 | Registration refuses an email that already has an account, in any case and with spaces | RegistrationSteps, ResultSteps, SignInSteps |
| AC-008 | Registration checks the password rule before whether the email is taken | RegistrationSteps, ResultSteps |
| AC-009 | Registration checks the email before the password rule | RegistrationSteps, ResultSteps |
| AC-010 | Sign-in matches the email whatever its case | RegistrationSteps, SignInSteps, AccountDetailsSteps |
| AC-011 | Sign-in ignores spaces around the email | RegistrationSteps, SignInSteps, AccountDetailsSteps |
| AC-012 | Sign-in with an email that has no account fails with the single message | SignInSteps, ResultSteps |
| AC-013 | Sign-in with a wrong password fails with the single message | RegistrationSteps, SignInSteps, ResultSteps |
| AC-014 | An unknown email, a wrong password and a blocked account give the same result | RegistrationSteps, SignInSteps |
| AC-015 | A blocked account still fails with the correct password 4 minutes 59 seconds after the fifth failure | RegistrationSteps, SignInSteps, ResultSteps |
| AC-016 | The block ends exactly 5 minutes after the fifth failure | RegistrationSteps, SignInSteps |
| AC-017 | An attempt during the block does not extend it | RegistrationSteps, SignInSteps |
| AC-018 | The end of a block resets the count of failed sign-ins | RegistrationSteps, SignInSteps |
| AC-019 | A successful sign-in resets the count of failed sign-ins | RegistrationSteps, SignInSteps |
| AC-020 | Four failed sign-ins in a row do not block | RegistrationSteps, SignInSteps |
| AC-021 | Five failed sign-ins in a row block however far apart they are | RegistrationSteps, SignInSteps, ResultSteps |
| AC-022 | A failed attempt during the block does not count after the block | RegistrationSteps, SignInSteps |
| AC-023 | Five failed attempts during the block do not start a new block | RegistrationSteps, SignInSteps |
| AC-024 | A block applies only to its own account | RegistrationSteps, SignInSteps |
| AC-025 | Failed sign-ins for an email that has no account block nothing | SignInSteps, RegistrationSteps |
| AC-026 | The anonymous visitor cannot read account details | RegistrationSteps, AccountDetailsSteps, ResultSteps |
| AC-027 | A registered user reads their own account details | RegistrationSteps, SignInSteps, AccountDetailsSteps |
| AC-028 | An administrator reads their own account details | AdministratorCreationSteps, SignInSteps, AccountDetailsSteps |
| AC-029 | A registered user gets only their own account details | RegistrationSteps, SignInSteps, AccountDetailsSteps |
| AC-030 | An administrator gets only their own account details | RegistrationSteps, AdministratorCreationSteps, SignInSteps, AccountDetailsSteps |
| AC-031 | A signed-in user changes their password | RegistrationSteps, SignInSteps, PasswordChangeSteps |
| AC-032 | A password change accepts a new password at the lower bound of the password rule | RegistrationSteps, SignInSteps, PasswordChangeSteps |
| AC-033 | A password change with a wrong current password is refused | RegistrationSteps, SignInSteps, PasswordChangeSteps, ResultSteps |
| AC-034 | A password change to a password that breaks the password rule is refused | RegistrationSteps, SignInSteps, PasswordChangeSteps, ResultSteps |
| AC-035 | A password change checks the current password before the password rule | RegistrationSteps, SignInSteps, PasswordChangeSteps, ResultSteps |
| AC-036 | The anonymous visitor cannot change a password, whatever the new password | RegistrationSteps, PasswordChangeSteps, ResultSteps |
| AC-037 | A registered user cannot change another account's password | RegistrationSteps, SignInSteps, PasswordChangeSteps, ResultSteps |
| AC-038 | An administrator cannot change another account's password | RegistrationSteps, AdministratorCreationSteps, SignInSteps, PasswordChangeSteps, ResultSteps |
| AC-039 | Administrator creation creates an administrator | AdministratorCreationSteps, ResultSteps, AccountDetailsSteps |
| AC-040 | Administrator creation accepts a password at the lower bound of the password rule | AdministratorCreationSteps, ResultSteps, AccountDetailsSteps |
| AC-041 | Administrator creation leaves an existing administrator unchanged | AdministratorCreationSteps, ResultSteps, SignInSteps |
| AC-042 | Administrator creation does not turn a registered user into an administrator | RegistrationSteps, AdministratorCreationSteps, ResultSteps, AccountDetailsSteps |
| AC-043 | Administrator creation without an email or a password configures nothing | AdministratorCreationSteps, ResultSteps |
| AC-044 | A password of only spaces creates no account | AdministratorCreationSteps, RegistrationSteps, AccountDetailsSteps |
| AC-045 | Administrator creation refuses an email that is not valid | AdministratorCreationSteps, ResultSteps |
| AC-046 | Administrator creation refuses a password that breaks the password rule | AdministratorCreationSteps, ResultSteps, RegistrationSteps, AccountDetailsSteps |
| AC-047 | Administrator creation finds an existing administrator before it checks the password rule | AdministratorCreationSteps, ResultSteps, SignInSteps |
| AC-048 | Administrator creation checks for a missing password before the email | AdministratorCreationSteps, ResultSteps |
| AC-049 | Administrator creation checks the email before the password rule | AdministratorCreationSteps, ResultSteps |
| AC-050 | Administrator creation finds a registered user before it checks the password rule | RegistrationSteps, AdministratorCreationSteps, ResultSteps, SignInSteps |
| AC-051 | Administrator creation checks for a missing password before it looks for an existing administrator | AdministratorCreationSteps, ResultSteps, SignInSteps |
| AC-052 | Administrator creation checks for a missing password before it looks for a registered user | RegistrationSteps, AdministratorCreationSteps, ResultSteps, AccountDetailsSteps |
| AC-053 | A new email creates an additional administrator | AdministratorCreationSteps, ResultSteps, AccountDetailsSteps |
| AC-054 | Administrator creation matches the email whatever its case and spaces | AdministratorCreationSteps, ResultSteps, SignInSteps |
| AC-062 | Administrator creation keeps the email in its trimmed, lower-case form | AdministratorCreationSteps, ResultSteps, AccountDetailsSteps |
| AC-063 | Administrator creation finds a registered user whatever the case and spaces of the email | RegistrationSteps, AdministratorCreationSteps, ResultSteps, AccountDetailsSteps |
| AC-055 | Demo loading without a demo password creates no account | DemoLoadingSteps, ResultSteps |
| AC-056 | Demo loading with a password that breaks the password rule creates no account | DemoLoadingSteps, ResultSteps |
| AC-057 | Demo loading creates the three demo accounts | DemoLoadingSteps, AccountDetailsSteps |
| AC-058 | Demo loading accepts a demo password at the lower bound of the password rule | DemoLoadingSteps, SignInSteps |
| AC-059 | Demo loading run again changes nothing | DemoLoadingSteps, SignInSteps, PasswordChangeSteps |
| AC-060 | Demo loading decides per demo email, and a demo email registered earlier keeps its owner's password | DemoLoadingSteps, RegistrationSteps, SignInSteps |

## Unexpressible steps

None.

## Cross-feature changes

None. No other feature folders exist yet, and `Support/Shared/` was not changed.

## Missing packages

- `Microsoft.Extensions.DependencyInjection` (version matching the .NET 10 / EF Core packages the
  implementation uses) — needed in ref mode to compose the application as ADR 0003 prescribes:
  `new ServiceCollection()`, `AddHtPersistence(connectionString)`, `AddHtApplication()`,
  `BuildServiceProvider()`, and resolving `IAccountStore` from a scope. The public API offers no other way
  to get an `IAccountStore`, because the implementation is not public, and `AddHtPersistence` and
  `AddHtApplication` take an `IServiceCollection`. Without this package, ref mode cannot compile the
  composition. This is the only remaining build error.

## Interpretations and assumptions

- All items: composition. Each scenario builds its own service provider from `AddHtPersistence` (with the
  ADR 0003 connection string: `HT_AC_CONNECTION`, or the ADR default) and `AddHtApplication`. Each use
  case call runs in its own DI scope, like one request. `AccountService`, `OperatorService` and
  `DemoDataService` are created through their public constructors (ADR 0010), with the scoped
  `IAccountStore`. The scenario's `FakeTimeProvider` is passed to `AccountService` and also registered as
  `TimeProvider` in DI, so no part reads the system clock.
- All items: database reset at the start of the run. ADR 0003 says every run starts empty, and
  `[BeforeTestRun]` is not allowed. So a `[BeforeFeature]` hook scoped to `@F001` (`Support/F001/F001Hooks.cs`)
  calls `HtDatabase.ResetAsync` once before this feature's scenarios. Without it, a second run would fail,
  because the fixed emails would already have accounts. If several features later run in one test run,
  this hook resets the database between features. Every scenario arranges its own data, so that is
  harmless.
- All items: time. The scenario clock starts at `2026-11-11T00:00:00Z`, which is earlier than every time
  written in the feature, so `FakeTimeProvider` only moves forward. The acceptance conventions allow any
  current time for steps without one. Timed steps set the clock to the written instant before calling the
  use case.
- "the result is \"<text>\"" (AC-002, AC-004, AC-007–009, AC-012, AC-013, AC-015, AC-021, AC-026, AC-033–
  AC-036, AC-037, AC-038, AC-039–AC-056): reads the result of the scenario's last When step. It asserts that
  the result's `Message` equals the text exactly, and that its named outcome is the one the API names for
  that text (for example "Enter a valid email address." → `RegistrationOutcome.InvalidEmail`). The spec
  says every result has both. The text-to-outcome table is in `Support/F001/F001Text.cs`.
- Given steps that state a result ("registered with ...", "is signed in with ...", "sign-ins ... failed
  at ..., each with ...", "administrator creation ran ... and gave ...", "demo loading ran ... and gave ...",
  "changed its password ..."): they assert that result, as the acceptance conventions say: registration
  succeeded, sign-in succeeded or failed with the given text, and the password change gave `Changed`.
- AC-014: "the three results are the same". The three sign-ins run at the same instant. The step asserts
  that all three have the same named outcome, which is `SignInOutcome.Failed` (the outcome of that text),
  the exact text, and no acting user.
- "sign-in succeeds" and "sign-in with X and P succeeds": outcome `SignedIn` with a non-anonymous acting
  user. "fails with M": outcome `Failed`, message M, and no acting user. A successful sign-in in a Then
  makes X the acting user, as the acceptance conventions say.
- "account details for X signed in with P give ...": sign-in with X and P must succeed, and then that
  acting user's account details must be `Found` with the stated email and/or role.
- AC-029, AC-030: "the result does not contain \"other\"". The returned email is not the other email,
  compared case-insensitively. The other email is not part of the details' string form, which shows Email
  and Role. The result message does not contain it either.
- AC-036: "they change a password" uses the anonymous visitor from the Given. The step checks that the
  acting user is anonymous before the call. AC-031–AC-035, AC-037, AC-038: "they" is the account signed in
  by the preceding Given.
- AC-059: "\"demo3@example.com\" changed its password ..." uses the acting user from the earlier
  "\"demo3@example.com\" is signed in" step, by email.
- AC-032, AC-040, AC-058: the parenthetical note "(8 characters, with a digit)" after a quoted password is
  documentation and is not part of the value.
- AC-043, AC-048, AC-051, AC-052, AC-055: `absent` passes `null`. `""` passes the empty text, and `"   "`
  passes three spaces.
- AC-055–AC-060: per-email demo results. Demo loading must give `DemoLoadingOutcome.Loaded`. Each named
  email must appear exactly once in `Accounts`, with the stated text ("created" or "already exists") as
  `Message` and the matching `DemoAccountOutcome`. "names no other email" also asserts that the set of
  emails in `Accounts` is exactly the named ones. In the "demo loading run afterwards ... gives ..." Then
  (AC-055, AC-056) the step does not say "names no other email", so only the named emails are asserted.
- AC-055–AC-060: "the AC database is reset" calls `HtDatabase.ResetAsync` with the AC connection string.
- Differences between the `.feature` file and acceptance.md: none found. The `.feature` file uses quoted
  values where acceptance.md uses back-ticks.

## Feedback addressed

Not applicable: no feedback file was given.
