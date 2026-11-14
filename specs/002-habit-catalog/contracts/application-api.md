# Contract: Application surface of the habit catalog

**Feature**: specs/002-habit-catalog · **Plan**: [../plan.md](../plan.md)

The public API that `HT.Ui`, the integration tests and the acceptance tests call (ADR 0003, ADR 0005). Domain
and input types are in [../data-model.md](../data-model.md). Names below are the contract; parameter names
may differ. Every method is `async` and takes a `CancellationToken` last. Slice 1's surface is unchanged
except for demo loading (below).

## Composition

`AddHtApplication` also registers `CatalogService`, `CatalogAdministrationService` and `CustomHabitService`
(scoped). `AddHtPersistence` also registers `ICatalogHabitStore`, `ICustomHabitStore` and `IDemoDataStore`
(scoped), and configures EF Core not to log database error texts (research R-14). `DemoDataService`'s public
constructor takes `IAccountStore` and `IDemoDataStore`. `HtDatabase.ResetAsync` applies the new migration with the others. A test builds the application as
in slice 1.

## `CatalogService` (anyone; no acting user, research R-07)

| Method | Outcomes | Requirement |
|---|---|---|
| `BrowseAsync(CatalogFilter filter)` | `Found` with `Habits` (`IReadOnlyList<CatalogHabitSummary>`, name order) and `Count` | FR-008, FR-009, FR-024 |
| `GetHomePreviewAsync()` | `Found` with `Habits` (the first four of `BrowseAsync(new CatalogFilter())`, fewer when fewer exist) and `PublishedCount` (all Published habits, for "See all N →") | FR-008, FR-024 |
| `GetCategoryFilterAsync()` | `Found` with `Options` (`IReadOnlyList<CategoryFilterOption>`): "All" first, then each category that has a Published habit, in FR-002 order | FR-009 |
| `GetDetailsAsync(HabitId id)` | `NotInCatalog` · `Found` with `CatalogHabitDetails` | FR-010, FR-024 |

- `CatalogFilter` with all values null is browsing with no filters. The Home preview is its own use case
  (AC-012, AC-072 "asks for the Home preview"); the visitor Home renders `Habits` and leaves out the section
  when it is empty (HP-012).
- `NotInCatalog` is the result for a Draft, an Unpublished habit, a custom habit's identifier and an unknown
  identifier alike.

## `CatalogAdministrationService` (administrators; acting user first)

| Method | Outcomes, in check order (FR-016) | Requirement |
|---|---|---|
| `ListAsync(ActingUser actingUser, string? search)` | `NotSignedIn` · `NotAdministrator` · `Found` with `Habits` (`IReadOnlyList<CatalogHabitListItem>`, name order, filtered) and `Total` (all catalog habits) | FR-012, FR-014 |
| `GetAsync(ActingUser actingUser, HabitId id)` | `NotSignedIn` · `NotAdministrator` · `NotFound` · `Found` with `CatalogHabitAdminDetails` | FR-012 to FR-014 |
| `CreateAsync(ActingUser actingUser, CatalogHabitInput input)` | `NotSignedIn` · `NotAdministrator` · field outcomes · `NameTaken` · `Created` with `HabitId` (state Draft) | FR-004 to FR-007, FR-011, FR-014 |
| `EditAsync(ActingUser actingUser, HabitId id, CatalogHabitInput input)` | `NotSignedIn` · `NotAdministrator` · `NotFound` · field outcomes · `NameTaken` · `Saved` | FR-004 to FR-007, FR-012 to FR-014 |
| `PublishAsync(ActingUser actingUser, HabitId id)` | `NotSignedIn` · `NotAdministrator` · `NotFound` · `Published` (also when it was already Published) | FR-011, FR-013, FR-014 |
| `UnpublishAsync(ActingUser actingUser, HabitId id)` | `NotSignedIn` · `NotAdministrator` · `NotFound` · `NotPublished` (a Draft) · `Unpublished` (also when it was already Unpublished) | FR-011, FR-013 to FR-015 |

- Publish and unpublish: the service reads the habit, Domain decides the transition, and the store's
  compare-and-swap writes it; a lost race is read and decided again (research R-08).

- **Field outcomes**, in FR-005, FR-006 and FR-007's order: `NameMissing` · `NameTooLong` ·
  `DescriptionTooLong` · `CategoryMissing` · `IntentMissing` · `IconMissing` · `TooManySuggestedStartingGoals`
  · `SuggestedStartingGoalNotMarked` · then, for the first suggested starting goal with a problem, with
  `Position` set: `BuildOrReduceMissing` · `MeasurementMissing` · `YesNoHasAmountOrUnit` ·
  `BuildAmountOutOfRange` · `ReduceAmountOutOfRange` · `CountUnitMissing` · `CountUnitTooLong` ·
  `DurationUnitMissing` · `ScheduleMissing` · `DaysMissing` · `TimesOutOfRange` · `PeriodMissing`.
- `NotFound` covers an identifier of no catalog habit, including a custom habit's (FR-013).
- `NameTaken` compares with every catalog habit in any state, the edited one excluded (FR-004); a lost race
  at the unique index gives the same outcome (HP-018).
- Every refusal changes nothing.

## `CustomHabitService` (registered users; acting user first)

| Method | Outcomes, in check order (FR-016) | Requirement |
|---|---|---|
| `ListAsync(ActingUser actingUser)` | `NotSignedIn` · `NotForAdministrators` · `Found` with `Habits` (`IReadOnlyList<CustomHabitView>`, name order) | FR-020, FR-021, FR-023, FR-025 |
| `GetAsync(ActingUser actingUser, HabitId id)` | `NotSignedIn` · `NotForAdministrators` · `NotFound` · `Found` with `CustomHabitView` | FR-020 to FR-023, FR-025 |
| `CreateAsync(ActingUser actingUser, HabitInput input)` | `NotSignedIn` · `NotForAdministrators` · `NameMissing` … `IconMissing` · `NameTaken` · `Created` with `HabitId` | FR-004, FR-005, FR-020, FR-023, FR-025 |
| `EditAsync(ActingUser actingUser, HabitId id, HabitInput input)` | `NotSignedIn` · `NotForAdministrators` · `NotFound` · `NameMissing` … `IconMissing` · `NameTaken` · `Saved` | FR-004, FR-005, FR-020 to FR-023, FR-025 |
| `DeleteAsync(ActingUser actingUser, HabitId id)` | `NotSignedIn` · `NotForAdministrators` · `NotFound` · `Deleted` | FR-020 to FR-023, FR-025 |

- Every call acts only on custom habits whose owner is `actingUser.AccountId`; another account's habit, a
  catalog habit's identifier and an unknown identifier all give `NotFound` (FR-022). No method takes an
  `AccountId`.
- `NameTaken` compares with the acting user's own custom habits only, the edited one excluded (FR-004).

## View types

| Type | Members |
|---|---|
| `CatalogHabitSummary` | `Id`, `Name` (`string`), `Category`, `Intent`, `Icon` (`string`), `Description` (`string?`), `MarkedSuggestedStartingGoal` (`SuggestedStartingGoalView?`, null when the habit has none) |
| `CatalogHabitDetails` | the `CatalogHabitSummary` members, plus `SuggestedStartingGoals` (`IReadOnlyList<SuggestedStartingGoalView>`, in order) |
| `CatalogHabitListItem` | `Id`, `Name`, `Category`, `Intent`, `State` |
| `CatalogHabitAdminDetails` | the `CatalogHabitDetails` members, plus `State` |
| `SuggestedStartingGoalView` | `Position` (from 1), `IsMarked`, `BuildOrReduce`, `Measurement`, `Amount` (`int?`), `CountUnit` (`string?`), `DurationUnit?`, `ScheduleKind`, `Days` (`IReadOnlySet<DayOfWeek>`), `Times` (`int?`), `Period?`, `Text`, `Kind` (FR-017) |
| `CategoryFilterOption` | `Category` (`Category?`, null for "All"), `Label` (`"All"` or the category name) |
| `CustomHabitView` | `Id`, `Name`, `Description` (`string?`), `Category`, `Intent`, `Icon` (`string`) |

No view of a custom habit carries its owner, and no catalog view carries anything of a custom habit.

## Demo loading (`DemoDataService.LoadAsync`, extended)

| Value | Change | Requirement |
|---|---|---|
| `DemoLoadingResult.Outcome` | unchanged: `NoPassword` · `PasswordRuleNotMet` · `Loaded` | slice 1 FR-016 |
| `DemoLoadingResult.Accounts[i].CustomHabits` | new, `DemoCustomHabitsResult?`: `Added` with `Message` "demo custom habits added" for an account result `Created`; null for `AlreadyExists` | FR-031 |
| `DemoLoadingResult.Catalog` | new, `DemoCatalogResult?`: `SeedCatalogLoaded` ("seed catalog loaded") or `CatalogNotEmpty` ("catalog not empty"); null for the two refusals | FR-030 |

- Order of work: the password checks, then each demo email in `DemoEmails` order (account and its custom
  habits as one unit), then the seed catalog (research R-09).
- The results name the demo email and a result text only, never a custom-habit name (FR-031).

## Texts

Defined once as `public const string` in `HT.Application`, worded as in the spec:

| Class | Constant | Text |
|---|---|---|
| `HabitTexts` | `NameMissing` | Enter a name. |
| `HabitTexts` | `NameTooLong` | The name can have at most 60 characters. |
| `HabitTexts` | `DescriptionTooLong` | The description can have at most 500 characters. |
| `HabitTexts` | `CategoryMissing` | Choose a category. |
| `HabitTexts` | `IntentMissing` | Choose Build or Reduce. |
| `HabitTexts` | `IconMissing` | Choose an icon. |
| `CatalogTexts` | `NotAdministrator` | Only administrators can manage the catalog. |
| `CatalogTexts` | `NotFound` | This catalog habit does not exist. |
| `CatalogTexts` | `NotInCatalog` | This habit is not in the catalog. |
| `CatalogTexts` | `NotPublished` | This catalog habit is not published. |
| `CatalogTexts` | `NameTaken` | A catalog habit with this name already exists. |
| `CatalogTexts` | `TooManySuggestedStartingGoals` | A habit can have at most 5 suggested starting goals. |
| `CatalogTexts` | `SuggestedStartingGoalNotMarked` | Mark one suggested starting goal for the card. |
| `CatalogTexts` | `MeasurementMissing` | Choose how it is measured. |
| `CatalogTexts` | `YesNoHasAmountOrUnit` | A yes/no suggested starting goal has no amount or unit. |
| `CatalogTexts` | `BuildAmountOutOfRange` | Enter a whole number from 1 to 999. |
| `CatalogTexts` | `ReduceAmountOutOfRange` | Enter a whole number from 0 to 999. |
| `CatalogTexts` | `CountUnitMissing` | Enter a unit, for example pages. |
| `CatalogTexts` | `CountUnitTooLong` | The unit can have at most 20 characters. |
| `CatalogTexts` | `DurationUnitMissing` | Choose minutes or hours. |
| `CatalogTexts` | `ScheduleMissing` | Choose fixed days or how many times. |
| `CatalogTexts` | `DaysMissing` | Choose at least one day. |
| `CatalogTexts` | `TimesOutOfRange` | Enter a number of times from 1 to 20. |
| `CatalogTexts` | `PeriodMissing` | Choose per day or per week. |
| `CatalogTexts` | `AllCategories` | All |
| `CustomHabitTexts` | `NotForAdministrators` | Custom habits are for registered users. |
| `CustomHabitTexts` | `NotFound` | This custom habit does not exist. |
| `CustomHabitTexts` | `NameTaken` | You already have a custom habit with this name. |
| `OperatorTexts` | `SeedCatalogLoaded` | seed catalog loaded |
| `OperatorTexts` | `CatalogNotEmpty` | catalog not empty |
| `OperatorTexts` | `DemoCustomHabitsAdded` | demo custom habits added |

`NotSignedIn` uses slice 1's `AccountTexts.NotSignedIn` ("You are not signed in."). The suggested starting
goal's `BuildOrReduceMissing` uses the same text as the habit's `IntentMissing`, "Choose Build or Reduce."
(`HabitTexts.IntentMissing`); the two outcomes stay apart, and only the suggested starting goal's carries a
`Position`. `Message` is `null` for the successes the
spec gives no text: `Found`, `Created`, `Saved`, `Published`, `Unpublished` and `Deleted`.
