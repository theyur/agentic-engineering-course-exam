# Writer report — F002 / run-002

## Build
success, 1 attempt, no remaining errors (0 warnings, 0 errors).

## Coverage
All 73 scenarios (outlines included) of `Features/F002/habit-catalog.feature` are bound. Every file is in `tests/HT.AcceptanceTests/StepDefinitions/F002/`.

| AC tag | Scenario name | Step definition file(s) |
|---|---|---|
| AC-001 | Browsing returns only Published habits, with their card content, to every audience | ArrangeSteps.cs, BrowsingSteps.cs |
| AC-002 | Browsing orders habits by name, ignoring case | ArrangeSteps.cs, BrowsingSteps.cs |
| AC-003 | Browsing by category returns only that category | ArrangeSteps.cs, BrowsingSteps.cs |
| AC-004 | Browsing by intent returns only that intent | ArrangeSteps.cs, BrowsingSteps.cs |
| AC-005 | Search matches part of the name, trimmed and ignoring case | ArrangeSteps.cs, BrowsingSteps.cs |
| AC-006 | Custom habits never appear in browsing, for any audience | ArrangeSteps.cs, BrowsingSteps.cs |
| AC-007 | Browsing with no match gives an empty result | ArrangeSteps.cs, BrowsingSteps.cs |
| AC-008 | The category filter starts with All and follows the fixed order | ArrangeSteps.cs, BrowsingSteps.cs |
| AC-009 | The category filter counts only Published catalog habits | ArrangeSteps.cs, BrowsingSteps.cs |
| AC-010 | Category, intent and search apply together | ArrangeSteps.cs, BrowsingSteps.cs |
| AC-011 | Browsing gives the count of the habits it returns | ArrangeSteps.cs, BrowsingSteps.cs |
| AC-012 | The Home preview is the first four Published habits in name order | ArrangeSteps.cs, BrowsingSteps.cs |
| AC-013 | Details give every suggested starting goal in order, with its text and kind | ArrangeSteps.cs, BrowsingSteps.cs |
| AC-014 | A habit without suggested starting goals is shown without one | ArrangeSteps.cs, BrowsingSteps.cs |
| AC-015 | The text and kind of a suggested starting goal follow fixed rules | ArrangeSteps.cs, BrowsingSteps.cs |
| AC-016 | Details of a habit that is not Published give the same refusal to every audience | ArrangeSteps.cs, BrowsingSteps.cs, ResultSteps.cs |
| AC-017 | Details with a custom habit's identifier give "not in the catalog" | ArrangeSteps.cs, BrowsingSteps.cs, ResultSteps.cs |
| AC-018 | A new catalog habit is a Draft that the public catalog does not show | ArrangeSteps.cs, CatalogAdministrationSteps.cs, BrowsingSteps.cs, ResultSteps.cs |
| AC-019 | Publishing a Draft makes it public | ArrangeSteps.cs, CatalogAdministrationSteps.cs, BrowsingSteps.cs, ResultSteps.cs |
| AC-020 | An edit of a Published habit is public on the next read | ArrangeSteps.cs, EditSteps.cs, BrowsingSteps.cs, ResultSteps.cs |
| AC-021 | Unpublishing a Published habit takes it out of the catalog at once | ArrangeSteps.cs, CatalogAdministrationSteps.cs, BrowsingSteps.cs, ResultSteps.cs |
| AC-022 | An Unpublished habit can be published again | ArrangeSteps.cs, CatalogAdministrationSteps.cs, BrowsingSteps.cs, ResultSteps.cs |
| AC-023 | A Draft cannot be unpublished | ArrangeSteps.cs, CatalogAdministrationSteps.cs, ResultSteps.cs |
| AC-024 | Publishing a Published habit is a success that changes nothing | ArrangeSteps.cs, CatalogAdministrationSteps.cs, ResultSteps.cs |
| AC-025 | Unpublishing an Unpublished habit is a success that changes nothing | ArrangeSteps.cs, CatalogAdministrationSteps.cs, ResultSteps.cs |
| AC-026 | A catalog-habit name is taken in every state, ignoring case and spaces | ArrangeSteps.cs, CatalogAdministrationSteps.cs, EditSteps.cs, ResultSteps.cs |
| AC-027 | Field checks refuse a catalog habit with a message per field | ArrangeSteps.cs, CatalogAdministrationSteps.cs, ResultSteps.cs |
| AC-028 | A name of 60 characters and a description of 500 characters are accepted | ArrangeSteps.cs, CatalogAdministrationSteps.cs, ResultSteps.cs |
| AC-029 | Field checks come in a fixed order | ArrangeSteps.cs, CatalogAdministrationSteps.cs, ResultSteps.cs |
| AC-030 | A registered user cannot manage the catalog | ArrangeSteps.cs, CatalogAdministrationSteps.cs, EditSteps.cs, ResultSteps.cs |
| AC-031 | The anonymous visitor cannot manage the catalog | ArrangeSteps.cs, CatalogAdministrationSteps.cs, EditSteps.cs, ResultSteps.cs |
| AC-032 | An unknown identifier does not exist for the administrator | ArrangeSteps.cs, CatalogAdministrationSteps.cs, EditSteps.cs, ResultSteps.cs |
| AC-033 | A custom habit is not a catalog habit for the administrator | ArrangeSteps.cs, CatalogAdministrationSteps.cs, EditSteps.cs, ResultSteps.cs |
| AC-034 | Suggested starting goals keep their order and the card shows the marked one | ArrangeSteps.cs, EditSteps.cs, CatalogAdministrationSteps.cs, BrowsingSteps.cs, ResultSteps.cs |
| AC-035 | The list of suggested starting goals is checked for its length first, then for its mark | ArrangeSteps.cs, EditSteps.cs, ResultSteps.cs |
| AC-036 | Each field of a suggested starting goal has its own refusal, with the position | ArrangeSteps.cs, EditSteps.cs, ResultSteps.cs |
| AC-037 | The bounds of a suggested starting goal's fields are accepted | ArrangeSteps.cs, EditSteps.cs, CatalogAdministrationSteps.cs, ResultSteps.cs |
| AC-038 | A Reduce amount of 0 is accepted and shown as "at most 0" | ArrangeSteps.cs, EditSteps.cs, BrowsingSteps.cs, ResultSteps.cs |
| AC-039 | The role and the identifier are checked before the fields | ArrangeSteps.cs, CatalogAdministrationSteps.cs, EditSteps.cs, ResultSteps.cs |
| AC-040 | The administrator's list is in name order and can be searched | ArrangeSteps.cs, CatalogAdministrationSteps.cs |
| AC-041 | Suggested starting goals are checked in list order, and each by its first failing field | ArrangeSteps.cs, EditSteps.cs, ResultSteps.cs |
| AC-042 | The administrator reads a Draft with all its fields and its state | ArrangeSteps.cs, CatalogAdministrationSteps.cs |
| AC-043 | Each of the six categories can be given to a catalog habit | ArrangeSteps.cs, CatalogAdministrationSteps.cs, BrowsingSteps.cs, ResultSteps.cs |
| AC-044 | Only the 24 habit icons are accepted | ArrangeSteps.cs, CatalogAdministrationSteps.cs, ResultSteps.cs |
| AC-045 | Names are ordered ordinally, ignoring case, not by the current culture | ArrangeSteps.cs, BrowsingSteps.cs |
| AC-046 | Publish and unpublish check the acting user and the role before the identifier | ArrangeSteps.cs, CatalogAdministrationSteps.cs, ResultSteps.cs |
| AC-047 | A registered user creates a custom habit | ArrangeSteps.cs, CustomHabitSteps.cs, ResultSteps.cs |
| AC-048 | A user's list holds only their own custom habits, in name order ignoring case | ArrangeSteps.cs, CustomHabitSteps.cs |
| AC-049 | A registered user edits every field of their custom habit | ArrangeSteps.cs, EditSteps.cs, CustomHabitSteps.cs, ResultSteps.cs |
| AC-050 | A registered user deletes their custom habit | ArrangeSteps.cs, CustomHabitSteps.cs, ResultSteps.cs |
| AC-051 | A user's custom-habit name is taken, ignoring case and spaces | ArrangeSteps.cs, CustomHabitSteps.cs, EditSteps.cs, ResultSteps.cs |
| AC-052 | Another user, and a catalog habit, may have the same name | ArrangeSteps.cs, CustomHabitSteps.cs, ResultSteps.cs |
| AC-053 | Another registered user cannot reach a custom habit | ArrangeSteps.cs, CustomHabitSteps.cs, ResultSteps.cs |
| AC-054 | An administrator has no access to custom habits, whatever the fields | ArrangeSteps.cs, CustomHabitSteps.cs, ResultSteps.cs |
| AC-055 | The anonymous visitor has no access to custom habits | ArrangeSteps.cs, CustomHabitSteps.cs, ResultSteps.cs |
| AC-056 | Field checks refuse a custom habit with the same messages | ArrangeSteps.cs, CustomHabitSteps.cs, ResultSteps.cs |
| AC-057 | For custom habits the acting user and ownership come first, the name check last | ArrangeSteps.cs, CustomHabitSteps.cs, EditSteps.cs, ResultSteps.cs |
| AC-058 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits | ArrangeSteps.cs, CustomHabitSteps.cs, ResultSteps.cs |
| AC-059 | A custom habit with a name of 60 characters and a description of 500 characters is accepted | ArrangeSteps.cs, CustomHabitSteps.cs, ResultSteps.cs |
| AC-060 | Demo loading loads the seed catalog into an empty catalog | ArrangeSteps.cs, DemoLoadingSteps.cs, BrowsingSteps.cs, CatalogAdministrationSteps.cs |
| AC-061 | Demo loading run again leaves the seed catalog unchanged | ArrangeSteps.cs, DemoLoadingSteps.cs, CatalogAdministrationSteps.cs |
| AC-062 | Demo loading adds no seed to a catalog that has a habit | ArrangeSteps.cs, DemoLoadingSteps.cs, CatalogAdministrationSteps.cs |
| AC-063 | Demo loading without a valid demo password creates no catalog and no custom habit | ArrangeSteps.cs, DemoLoadingSteps.cs, ResultSteps.cs, BrowsingSteps.cs |
| AC-064 | Demo accounts that demo loading creates get their demo custom habits | ArrangeSteps.cs, DemoLoadingSteps.cs |
| AC-065 | Demo loading run again leaves the demo accounts and their custom habits unchanged | ArrangeSteps.cs, DemoLoadingSteps.cs |
| AC-066 | Deleted demo custom habits are not created again | ArrangeSteps.cs, DemoLoadingSteps.cs |
| AC-067 | A demo email that a person registered first keeps its owner's data untouched | ArrangeSteps.cs, DemoLoadingSteps.cs, CustomHabitSteps.cs |
| AC-068 | A demo email held by an administrator gets no custom habit | ArrangeSteps.cs, DemoLoadingSteps.cs |
| AC-069 | A Draft and an Unpublished habit can be edited and keep their state | ArrangeSteps.cs, EditSteps.cs, CatalogAdministrationSteps.cs, ResultSteps.cs |
| AC-070 | Names and descriptions are trimmed before they are checked and stored | ArrangeSteps.cs, CatalogAdministrationSteps.cs, ResultSteps.cs |
| AC-071 | The administrator edits the description, intent and icon of a catalog habit | ArrangeSteps.cs, EditSteps.cs, BrowsingSteps.cs, ResultSteps.cs |
| AC-072 | The Home preview has fewer than four habits when fewer are Published | ArrangeSteps.cs, BrowsingSteps.cs |
| AC-073 | Custom-habit names and descriptions are trimmed before they are checked and stored | ArrangeSteps.cs, CustomHabitSteps.cs, ResultSteps.cs |

## Unexpressible steps
None.

## Cross-feature changes
None in this run. (`tests/HT.AcceptanceTests/Support/F001/AccountsDriver.cs` was already modified in the working tree when this run started; this run did not touch it.)

## Missing packages
None.

## Interpretations and assumptions
- AC-049: "And then edits it, keeping the name ..." — "then edits it" with no acting user is read as "the acting user of the step before edits the same habit again". The step uses the last acting user recorded in the scenario context, as in run-001.
- AC-018, AC-043, AC-044, AC-030 n03, AC-031 n03: "creates the catalog habit `N` [with ...] [and publishes it]" — a missing " with ..." clause means the habit has only the name, and the other fields keep their defaults. "and publishes it" means the same acting user publishes the habit just created, and the publish result becomes "the result".

## Feedback addressed
- 1. `CatalogAdministrationSteps.WhenCreatesTheCatalogHabit` — the regex is now `` `([^`]+)`((?: with .+?)?)( and publishes it|)$ ``. Both groups always take part in the match, and an empty capture means the part is absent. The parameters are non-nullable strings. The captured " with ..." clause is passed to `F002Text.ApplyFields`, which trims it and removes the leading "with ".
- 2. `ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit` — the regex is now `` `([^`]+)`((?: with .+)?)$ ``. The clause group always takes part, and an empty clause means no fields.
- 3. `EditSteps.WhenEditsWithTheName` — split into two bindings: `` edits `N` with the name ... `` (`WhenEditsWithTheName`, 3 parameters) and `` edits `N` of `owner` with the name ... `` (`WhenEditsOfWithTheName`, 4 parameters). Both share the private helper `EditNameAsync`.
- 4. `EditSteps.WhenEditsItTo` and `WhenEditsItKeepingTheName` — each is split into an actor form (`<Actor> edits it ...`) and a "then" form (`^then edits it ...`, `WhenThenEditsItTo` / `WhenThenEditsItKeepingTheName`). The "then" form has no actor parameter and edits as the last acting user. I checked every "then edits it" step in the feature file: only AC-049 line 612 has one, and it is now bound by the "then" form.
- 5. `CustomHabitSteps.WhenCreatesTheCustomHabit` — the regex is now `` `([^`]+)`((?: with .+)?)$ ``. The clause group always takes part, and an empty clause means no fields.
- 6. `DemoLoadingSteps.GivenSignedInDeleted` — split into `GivenSignedInDeletedOne` (`` deleted `N`$ ``) and `GivenSignedInDeletedTwo` (`` deleted `N` and `M`$ ``). Both share the private helper `SignInAndDeleteAsync`. The single-name pattern cannot match the two-name text, because `[^`]+` cannot cross a backtick, so the two bindings never both match.
- Extra check — I went through every F002 step regex for a capture group inside an optional or alternated part. The remaining optional parts are all non-capturing: `(?: as an administrator)?`, `(?:, as one call ...)?`, `(?:their )?`, `(?:still )?`, `(?:then )?`, `(?:, )?` inside always-present list groups, `(?: \(.*\))?`, and `(?:, keeping its other fields)?`. The shared `F002Text.Actor` group always takes part when it is used directly (its optional email part is non-capturing). The one exception was the `(?:Actor|then)` alternation, fixed in item 4. No other binding has an argument that can drop out of the match.
