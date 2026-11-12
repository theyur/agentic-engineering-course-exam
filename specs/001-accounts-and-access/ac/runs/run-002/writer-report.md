# Writer report — F001 / run-002

## Build

success, 1 attempt (`dotnet build tests/HT.AcceptanceTests/HT.AcceptanceTests.csproj -p:AcRefMode=true -nologo -v q`:
0 warnings, 0 errors). No remaining errors.

The step definitions and support code in `StepDefinitions/F001/` and `Support/F001/` were already in place from
run-001. This run checked every binding again against the feature file, the spec, ADR 0003 and the current API
listings (`HT.Application`, `HT.Domain`, `HT.Persistence`). No change was needed: the public API has not changed in a
way that touches them, and no feedback file was given.

Files (all `[Binding]` classes carry `[Scope(Tag = "F001")]`):

- `tests/HT.AcceptanceTests/StepDefinitions/F001/RegistrationSteps.cs`
- `tests/HT.AcceptanceTests/StepDefinitions/F001/SignInSteps.cs`
- `tests/HT.AcceptanceTests/StepDefinitions/F001/AccountDetailsSteps.cs`
- `tests/HT.AcceptanceTests/StepDefinitions/F001/PasswordChangeSteps.cs`
- `tests/HT.AcceptanceTests/StepDefinitions/F001/AdministratorCreationSteps.cs`
- `tests/HT.AcceptanceTests/StepDefinitions/F001/DemoLoadingSteps.cs`
- `tests/HT.AcceptanceTests/StepDefinitions/F001/ResultSteps.cs`
- `tests/HT.AcceptanceTests/Support/F001/AccountsDriver.cs` (builds the app with `AddHtPersistence` + `AddHtApplication`,
  one service scope per use case, `FakeTimeProvider` as the current time)
- `tests/HT.AcceptanceTests/Support/F001/F001Context.cs` (per-scenario state)
- `tests/HT.AcceptanceTests/Support/F001/F001Database.cs` (ADR 0003 connection: `HT_AC_CONNECTION` or the ADR default)
- `tests/HT.AcceptanceTests/Support/F001/F001Hooks.cs` (`[BeforeFeature]` reset of HT-AC, scoped to F001)
- `tests/HT.AcceptanceTests/Support/F001/F001Text.cs` (parsing of step values; spec text → named outcome table)

## Coverage

| AC tag | Scenario name | Step definition file(s) |
|---|---|---|
| AC-001 | Registration creates a registered user | RegistrationSteps, AccountDetailsSteps |
| AC-002 | Registration refuses a password that breaks the password rule (4 rows) | RegistrationSteps, ResultSteps |
| AC-003 | Registration accepts a password at the lower bound of the password rule (2 rows) | RegistrationSteps, SignInSteps |
| AC-004 | Registration refuses an email that is not valid (14 rows) | RegistrationSteps, ResultSteps, SignInSteps |
| AC-005 | Registration accepts the shortest valid email and a domain of three parts (2 rows) | RegistrationSteps, SignInSteps |
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
| AC-034 | A password change to a password that breaks the password rule is refused (2 rows) | RegistrationSteps, SignInSteps, PasswordChangeSteps, ResultSteps |
| AC-035 | A password change checks the current password before the password rule | RegistrationSteps, SignInSteps, PasswordChangeSteps, ResultSteps |
| AC-036 | The anonymous visitor cannot change a password, whatever the new password (2 rows) | RegistrationSteps, PasswordChangeSteps, ResultSteps |
| AC-037 | A registered user cannot change another account's password | RegistrationSteps, SignInSteps, PasswordChangeSteps, ResultSteps |
| AC-038 | An administrator cannot change another account's password | RegistrationSteps, AdministratorCreationSteps, SignInSteps, PasswordChangeSteps, ResultSteps |
| AC-039 | Administrator creation creates an administrator | AdministratorCreationSteps, ResultSteps, AccountDetailsSteps |
| AC-040 | Administrator creation accepts a password at the lower bound of the password rule | AdministratorCreationSteps, ResultSteps, AccountDetailsSteps |
| AC-041 | Administrator creation leaves an existing administrator unchanged | AdministratorCreationSteps, ResultSteps, SignInSteps |
| AC-042 | Administrator creation does not turn a registered user into an administrator | RegistrationSteps, AdministratorCreationSteps, ResultSteps, AccountDetailsSteps |
| AC-043 | Administrator creation without an email or a password configures nothing (7 rows) | AdministratorCreationSteps, ResultSteps |
| AC-044 | A password of only spaces creates no account | AdministratorCreationSteps, RegistrationSteps, AccountDetailsSteps |
| AC-045 | Administrator creation refuses an email that is not valid | AdministratorCreationSteps, ResultSteps |
| AC-046 | Administrator creation refuses a password that breaks the password rule (2 rows) | AdministratorCreationSteps, ResultSteps, RegistrationSteps, AccountDetailsSteps |
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
| AC-055 | Demo loading without a demo password creates no account (3 rows) | DemoLoadingSteps, ResultSteps |
| AC-056 | Demo loading with a password that breaks the password rule creates no account (2 rows) | DemoLoadingSteps, ResultSteps |
| AC-057 | Demo loading creates the three demo accounts | DemoLoadingSteps, AccountDetailsSteps |
| AC-058 | Demo loading accepts a demo password at the lower bound of the password rule | DemoLoadingSteps, SignInSteps |
| AC-059 | Demo loading run again changes nothing | DemoLoadingSteps, SignInSteps, PasswordChangeSteps |
| AC-060 | Demo loading decides per demo email, and a demo email registered earlier keeps its owner's password | DemoLoadingSteps, RegistrationSteps, SignInSteps |

63 scenarios (scenario outlines counted once), all bound.

## Unexpressible steps

None.

## Cross-feature changes

None. No other feature folders exist yet, and `Support/Shared/` was not changed.

## Missing packages

None.

## Interpretations and assumptions

- All: "the result is "<text>"" is checked two ways. The result's `Message` must equal the literal text. The result's
  named outcome must be the one the spec gives that text for the use case of the last When step. The text-to-outcome
  table in `F001Text` lists only texts from the spec. It follows the spec Clarification "every refusal and operator
  result has a named outcome and carries the literal text".
- All: the application is composed through `AddHtPersistence(connection string)` and `AddHtApplication()` (ADR 0003).
  `IAccountStore` is resolved from a new service scope for each use case. `AccountService`, `OperatorService` and
  `DemoDataService` are built through their public constructors (ADR 0010). `AccountService` is given the scenario's
  `FakeTimeProvider` (constitution II).
- All: the connection string comes from `HT_AC_CONNECTION`. When that is not set, it is ADR 0003's default for `HT-AC`
  on `.\COLL1`.
- All: ADR 0003 says every run starts empty, and test-run hooks are not allowed. So an F001-scoped `[BeforeFeature]`
  hook calls `HtDatabase.ResetAsync` once before the feature's first scenario. Every other scenario arranges its own
  accounts, as acceptance.md states.
- AC-055 to AC-060: "the AC database is reset, so no account exists" calls `HtDatabase.ResetAsync` through the public
  reset entry point.
- AC-014 to AC-025: steps without a time run at the scenario clock's start, 2026-11-11T00:00:00Z. Timed steps set the
  `FakeTimeProvider` to the written UTC instant. Every written instant is later than the clock's start, and within a
  scenario they only move forward. acceptance.md says "steps without a time may use any current time".
- AC-014: "the three results are the same: the same named outcome, with the text ..." checks three things for each of
  the three results: the outcome is `SignInOutcome.Failed`, the message is the literal, and there is no acting user.
  It also checks that the three outcomes are equal to one another.
- "<x> is signed in with <p>" and "sign-in with <x> and <p> succeeds" require `SignInOutcome.SignedIn` and a
  non-anonymous acting user. That acting user is then "they" for the steps that follow (acceptance.md conventions).
- "account details for <x> signed in with <p> give ..." signs in with x and p, requires success, then asks for account
  details as that acting user. It checks `Found` plus the stated email and/or role.
- "the result does not contain <other email>" (AC-029, AC-030) checks the returned details' `Email`, their
  `ToString()` and the result's `Message`. None of them may contain the other email, ignoring case.
- "registration succeeds" requires `RegistrationOutcome.Registered` and a returned `AccountId`. Given steps
  "<x> registered with the password <p>" require the same.
- "they register ..." and "they change a password ..." assert that the acting user is the anonymous visitor, as set by
  "Given the anonymous visitor". "they change their password ..." uses the account signed in by an earlier step.
- AC-043: `absent` is passed as `null`; `""` and `"   "` are passed as typed. AC-048, AC-051 and AC-052: "the password
  absent" is passed as a `null` password. AC-055: `absent` is a `null` demo password.
- Notes in parentheses after a value, such as "(8 characters, with a digit)" in AC-032, AC-040 and AC-058, are
  descriptions and are not passed to the use case.
- Demo loading: a step that lists per-email results ("created" for ..., "already exists" for ...) also requires the
  overall outcome `DemoLoadingOutcome.Loaded`. The spec names per-email results only for loading that is not refused
  (FR-016). For each listed email there must be exactly one per-email result, with the literal message and the
  matching `DemoAccountOutcome`. "and names no other email" also requires that the set of emails in the result is
  exactly the listed ones. In a list, an email without its own result text takes the text before it, as the Gherkin
  reads ("created" for a, for b and for c).

## Feedback addressed

No feedback file was given for this run.
