# Contract: Application surface of goals

**Feature**: specs/004-goals · **Plan**: [../plan.md](../plan.md)

This is the public API that `HT.Ui`, the integration tests and the acceptance tests call (ADR 0003, ADR 0005).
Types are in [../data-model.md](../data-model.md).

- The names below are the contract; parameter names may differ.
- Every method is `async` and takes a `CancellationToken` last.
- Every result has `Outcome` (an enum) and `Message`: the literal text below, or null on success.
- Methods for signed-in users take `ActingUser` first and check in FR-023's order:
  1. `NotSignedIn`;
  2. the role;
  3. the identifier;
  4. the fields or range.
- The current time is `TimeProvider.GetUtcNow()` from DI. Acceptance tests register their own `TimeProvider` and
  supply no `InstallationTimeZone`, so the default is `UTC` (research R-03).

The surface of slices 1 and 2 stays source-compatible (FR-019, research R-07). Every change to it is listed under
"Changed surface".

## Composition

`AddHtApplication` also registers:

- `GoalService` and `ProfileService`, scoped;
- `InstallationTimeZone` as UTC, with `TryAddSingleton`.

`AddHtPersistence` also registers `IGoalStore`, scoped. `HtDatabase.ResetAsync` applies the migration `Goals`
together with the others.

## `GoalService` (registered users)

| Method | Outcomes, in check order | Requirement |
|---|---|---|
| `StartNewGoalAsync(ActingUser, GoalHabit? habit, int? suggestionPosition)` | `NotSignedIn` · `NotForAdministrators` · `NotInCatalog` · `CustomHabitNotFound` · `SuggestionNotFound` · `Started` with `GoalStartingValues` | FR-007, FR-008, FR-012 |
| `CreateAsync(ActingUser, GoalInput input)` | `NotSignedIn` · `NotForAdministrators` · `HabitMissing` · `NotInCatalog` · `CustomHabitNotFound` · the 12 target outcomes (`GoalCreateOutcome` members named as `GoalProblem`'s) · `ReduceAmountNeedsOneTime` · `StartDateMissing` · `StartDateInPast` · `StartDateTooFar` · `NameTooLong` · `Created` with `GoalId` | FR-001, FR-002, FR-005, FR-006, FR-008, FR-009, FR-011, FR-018 |
| `ListAsync(ActingUser)` | `NotSignedIn` · `NotForAdministrators` · `Found` with `Goals` (`IReadOnlyList<GoalListItem>`, name order, then creation order) | FR-014, FR-015 |
| `GetAsync(ActingUser, GoalId id)` | `NotSignedIn` · `NotForAdministrators` · `NotFound` · `Found` with `GoalDetails` | FR-016, FR-018, FR-020, FR-034 |
| `GetCalendarAsync(ActingUser, GoalId id, DateOnly first, DateOnly last)` | `NotSignedIn` · `NotForAdministrators` · `NotFound` · `RangeOutOfBounds` · `Found` with `GoalCalendarView` | FR-012, FR-013, FR-027 |

The rules behind these outcomes:

- **The habit check of `CreateAsync`** runs before the target checks, as FR-006 orders them:
  - `HabitMissing` comes first, then the habit lookup;
  - a catalog habit is looked up only among Published habits, otherwise `NotInCatalog`;
  - a custom habit is looked up only among the acting user's own, otherwise `CustomHabitNotFound`.

  The same lookup applies to `StartNewGoalAsync` (FR-008).
- **What a refusal carries**:
  - a `Field` (`GoalField`), for the page;
  - no other value, so the result never echoes the input (HP-011).
- **The starting values** of FR-007:
  - the start date is today in the acting user's time zone;
  - `Name` is null;
  - `CatalogSuggestionText` is `GoalText.TargetText` of the suggestion's values, or null when no suggestion
    was used.

  With no habit, only `StartDate` is set (US8-AS4), and a suggestion position is ignored. A custom habit has no
  suggested starting goals, so any position gives `SuggestionNotFound`, as a position below 1 or above the catalog
  habit's count does. A suggestion that is not a valid goal target (a Reduce amount of several times per period)
  keeps slice 2's text as `CatalogSuggestionText`; for every other suggestion that text equals the goal rules' text.
  `GoalHabitView.Icon` is set in the starting values (the Habit section's glyph, HP-001).
- **The schedule text**: `GoalListItem.ScheduleText` and `GoalDetails.ScheduleText` are `GoalText.ScheduleText` of the
  goal's target ("Mon · Wed · Fri", "Every day", "Flexible · 3 times a week"). The list has no days, so the Goals
  page's meta line takes its schedule from it (HP-003, US2-AS6), and the Settings card's "Schedule" row too (HP-004).
- **What `GoalDetails` gives about the habit**:
  - `Habit.IsInCatalog` is false for a goal on an Unpublished catalog habit;
  - it is null for a custom habit.

  The page shows the FR-034 Notice from it.
- **The time zones in `GoalDetails`**:
  - `TimeZone` and `TimeZoneAppliesFrom` are the version of the goal's current date at the supplied time, or the
    first version when the goal has no current date;
  - `LaterTimeZone` and `LaterTimeZoneAppliesFrom` are the one version after that, or null (FR-016).
- **The calendar range**:
  - `first` and `last` are both included;
  - more than 366 days, or `last` before `first`, gives `RangeOutOfBounds`.

  `Commitments` are filled for a commitment-based goal and `Periods` for a period-based one. Every instant is in
  UTC.

## `ProfileService` (any signed-in account)

| Method | Outcomes | Requirement |
|---|---|---|
| `GetTimeZoneAsync(ActingUser)` | `NotSignedIn` · `Found` with `TimeZone` (string) | FR-024, FR-026 |
| `ChangeTimeZoneAsync(ActingUser, string? timeZone)` | `NotSignedIn` · `NotANamedTimeZone` · `Unchanged` (the same zone) · `Changed` | FR-024, FR-026, FR-027 |
| `ChangeNameAsync(ActingUser, string? name)` | `NotSignedIn` · `NameTooLong` · `Changed` (an empty name removes it) | FR-028 |
| `GetProfileAsync(ActingUser)` | `NotSignedIn` · `Found` with `ProfileView` | FR-028, FR-030 |
| `GetHomeHeadingAsync(ActingUser)` | `NotSignedIn` · `Found` with `HomeHeading` | FR-029 |

Administrators use all five on their own account (US10-AS8, US11-AS7). None of them takes another account's
identifier (constitution III).

## Changed surface

**`AccountService`**

| Member | Change |
|---|---|
| `RegisterAsync(string? email, string? password, string? timeZone, string? name)` | New overload. It checks, in order: `InvalidEmail`, `PasswordRuleNotMet`, `EmailTaken`, then `NameTooLong` (new, appended: FR-028 "after slice 1's checks"). A time zone that is not named is not refused; the account gets the default (FR-025). The account records `CreatedAt` as the supplied time. The old overload is `RegisterAsync(email, password, null, null)`. |

**`CatalogService`**

| Member | Change |
|---|---|
| `BrowseAsync(ActingUser, CatalogFilter, bool onlyTracked)` | New overload. With `onlyTracked`, `NotSignedIn` or `NotForAdministrators` (appended to `CatalogBrowseOutcome`). Otherwise `Found`. For a registered user, each `CatalogHabitSummary.GoalCount` is that user's count and `CatalogBrowseResult.TrackedCount` is the number of returned habits with a count of at least 1. For others, both are null. |
| `GetDetailsAsync(ActingUser, HabitId)` | New overload. `NotInCatalog` · `Found`. For a registered user, `CatalogHabitDetails.GoalCount` is set and `Goals` (`IReadOnlyList<GoalOnHabit>`) lists their goals in name order, then creation order. For others, both are null. |
| `BrowseAsync(CatalogFilter)`, `GetDetailsAsync(HabitId)` | Unchanged results; the goal information is null (FR-019). |

**`CatalogAdministrationService`**

| Member | Change |
|---|---|
| `UnpublishAsync(ActingUser, HabitId, bool confirmed)` | New overload. `NotSignedIn` · `NotAdministrator` · `NotFound` · `NotPublished` · `ConfirmationNeeded` (appended) with `GoalCount` and the text "1 goal uses this habit." or "N goals use this habit.", and nothing else (SC-005) · `Unpublished`. |
| `UnpublishAsync(ActingUser, HabitId)` | Means `confirmed: false`. For a habit that no goal uses, the result is as in slice 2 (FR-019, FR-033). |

**`CustomHabitService`**

| Member | Change |
|---|---|
| `DeleteAsync` | `UsedByGoals` (appended), with "Goals use this custom habit, so it cannot be deleted." It comes after `NotFound` (FR-035). |
| `GetAsync` | `CustomHabitView.GoalCount` is the number of the user's goals on that habit (FR-035). |

**`DemoDataService.LoadAsync`**

| Member | Change |
|---|---|
| `LoadAsync` | The seed catalog runs before the accounts (FR-040). Each created account has a `DemoAccountResult.Goals` of "demo goals added"; an existing account has null. |

## Texts

The texts are defined once: `GoalTexts` (new), `AccountTexts` (extended), `CatalogTexts` (extended),
`CustomHabitTexts` (extended) and `OperatorTexts` (extended).

| Outcome | Text |
|---|---|
| `NotSignedIn` | "You are not signed in." (unchanged). A goal or profile use case also gives it, after the role check, when the acting user's account is not stored (deleted after sign-in, or a stale cookie): the spec defines no other outcome for it (FR-023), and it reveals nothing (`GoalService`, T058; `ProfileService`, T103). |
| `NotForAdministrators` (goals, and "Only habits I track") | "Goals are for registered users." |
| `HabitMissing` | "Choose a habit." |
| `NotInCatalog` | "This habit is not in the catalog." (slice 2 text) |
| `CustomHabitNotFound` | "This custom habit does not exist." (slice 2 text) |
| `SuggestionNotFound` | "This suggested starting goal does not exist." |
| `YesNoHasAmountOrUnit` (goal) | "A yes/no goal has no amount or unit." |
| other target outcomes | slice 2's `CatalogTexts` texts, unchanged |
| `ReduceAmountNeedsOneTime` | "A Reduce goal with an amount has 1 time per period." |
| `StartDateMissing` | "Choose a start date." |
| `StartDateInPast` | "The start date cannot be in the past." |
| `StartDateTooFar` | "Choose a start date within a year from today." |
| `NameTooLong` (goal, account) | "The name can have at most 60 characters." |
| `NotFound` (goal) | "This goal does not exist." |
| `RangeOutOfBounds` | "Choose a range of 1 to 366 days." |
| `NotANamedTimeZone` | "Choose a time zone from the list." |
| `UsedByGoals` | "Goals use this custom habit, so it cannot be deleted." |
| `ConfirmationNeeded` | "1 goal uses this habit." · "{N} goals use this habit." |
| greeting | "Good morning", "Good afternoon", "Good evening", each with ", {name}" when the account has a name |
| Home date | "{Weekday}, {d} {Month}", in English ("Tuesday, 17 November") |
| member since | "member since {Month} {yyyy}" ("member since November 2026") |
| demo goal result | "demo goals added" |
