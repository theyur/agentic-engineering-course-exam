# Writer report — F002 / run-001

## Build

success, 2 attempts (the first, a baseline build before any F002 code, failed only in F001's `AccountsDriver.cs` because
`DemoDataService` now takes an `IDemoDataStore`; fixed, see "Cross-feature changes"). No remaining errors, 0 warnings.

## Coverage

Step definition files are in `tests/HT.AcceptanceTests/StepDefinitions/F002/`. Support code is in
`tests/HT.AcceptanceTests/Support/F002/`: `F002Context`, `F002Driver`, `F002Arrange`, `F002Text`, `F002Assert`,
`HabitSpec`, `F002Database` and `F002Hooks` (a `[BeforeFeature]` reset, scoped to F002). Every scenario also uses
`ArrangeSteps.cs` (Given) and, where it checks a result, `ResultSteps.cs`.

| AC tag | Scenario name | Step definition file(s) |
|---|---|---|
| AC-001 | Browsing returns only Published habits, with their card content, to every audience | ArrangeSteps, BrowsingSteps |
| AC-002 | Browsing orders habits by name, ignoring case | ArrangeSteps, BrowsingSteps |
| AC-003 | Browsing by category returns only that category | ArrangeSteps, BrowsingSteps |
| AC-004 | Browsing by intent returns only that intent | ArrangeSteps, BrowsingSteps |
| AC-005 | Search matches part of the name, trimmed and ignoring case | ArrangeSteps, BrowsingSteps |
| AC-006 | Custom habits never appear in browsing, for any audience | ArrangeSteps, BrowsingSteps |
| AC-007 | Browsing with no match gives an empty result | ArrangeSteps, BrowsingSteps |
| AC-008 | The category filter starts with All and follows the fixed order | ArrangeSteps, BrowsingSteps |
| AC-009 | The category filter counts only Published catalog habits | ArrangeSteps, BrowsingSteps |
| AC-010 | Category, intent and search apply together | ArrangeSteps, BrowsingSteps |
| AC-011 | Browsing gives the count of the habits it returns | ArrangeSteps, BrowsingSteps |
| AC-012 | The Home preview is the first four Published habits in name order | ArrangeSteps, BrowsingSteps |
| AC-013 | Details give every suggested starting goal in order, with its text and kind | ArrangeSteps, BrowsingSteps |
| AC-014 | A habit without suggested starting goals is shown without one | ArrangeSteps, BrowsingSteps |
| AC-015 | The text and kind of a suggested starting goal follow fixed rules | ArrangeSteps, BrowsingSteps |
| AC-016 | Details of a habit that is not Published give the same refusal to every audience | ArrangeSteps, BrowsingSteps, ResultSteps |
| AC-017 | Details with a custom habit's identifier give "not in the catalog" | ArrangeSteps, BrowsingSteps, ResultSteps |
| AC-018 | A new catalog habit is a Draft that the public catalog does not show | ArrangeSteps, CatalogAdministrationSteps, BrowsingSteps, ResultSteps |
| AC-019 | Publishing a Draft makes it public | ArrangeSteps, CatalogAdministrationSteps, BrowsingSteps, ResultSteps |
| AC-020 | An edit of a Published habit is public on the next read | ArrangeSteps, EditSteps, BrowsingSteps, CatalogAdministrationSteps, ResultSteps |
| AC-021 | Unpublishing a Published habit takes it out of the catalog at once | ArrangeSteps, CatalogAdministrationSteps, BrowsingSteps, ResultSteps |
| AC-022 | An Unpublished habit can be published again | ArrangeSteps, CatalogAdministrationSteps, BrowsingSteps, ResultSteps |
| AC-023 | A Draft cannot be unpublished | ArrangeSteps, CatalogAdministrationSteps, ResultSteps |
| AC-024 | Publishing a Published habit is a success that changes nothing | ArrangeSteps, CatalogAdministrationSteps, BrowsingSteps, ResultSteps |
| AC-025 | Unpublishing an Unpublished habit is a success that changes nothing | ArrangeSteps, CatalogAdministrationSteps, BrowsingSteps, ResultSteps |
| AC-026 | A catalog-habit name is taken in every state, ignoring case and spaces | ArrangeSteps, CatalogAdministrationSteps, EditSteps, ResultSteps |
| AC-027 | Field checks refuse a catalog habit with a message per field | ArrangeSteps, CatalogAdministrationSteps, ResultSteps |
| AC-028 | A name of 60 characters and a description of 500 characters are accepted | ArrangeSteps, CatalogAdministrationSteps, ResultSteps |
| AC-029 | Field checks come in a fixed order | ArrangeSteps, CatalogAdministrationSteps, ResultSteps |
| AC-030 | A registered user cannot manage the catalog | ArrangeSteps, CatalogAdministrationSteps, EditSteps, ResultSteps |
| AC-031 | The anonymous visitor cannot manage the catalog | ArrangeSteps, CatalogAdministrationSteps, EditSteps, ResultSteps |
| AC-032 | An unknown identifier does not exist for the administrator | ArrangeSteps, CatalogAdministrationSteps, EditSteps, ResultSteps |
| AC-033 | A custom habit is not a catalog habit for the administrator | ArrangeSteps, CatalogAdministrationSteps, EditSteps, CustomHabitSteps, ResultSteps |
| AC-034 | Suggested starting goals keep their order and the card shows the marked one | ArrangeSteps, EditSteps, CatalogAdministrationSteps, BrowsingSteps |
| AC-035 | The list of suggested starting goals is checked for its length first, then for its mark | ArrangeSteps, EditSteps, CatalogAdministrationSteps, ResultSteps |
| AC-036 | Each field of a suggested starting goal has its own refusal, with the position | ArrangeSteps, EditSteps, CatalogAdministrationSteps, ResultSteps |
| AC-037 | The bounds of a suggested starting goal's fields are accepted | ArrangeSteps, EditSteps, CatalogAdministrationSteps, ResultSteps |
| AC-038 | A Reduce amount of 0 is accepted and shown as "at most 0" | ArrangeSteps, EditSteps, BrowsingSteps, ResultSteps |
| AC-039 | The role and the identifier are checked before the fields | ArrangeSteps, CatalogAdministrationSteps, EditSteps, ResultSteps |
| AC-040 | The administrator's list is in name order and can be searched | ArrangeSteps, CatalogAdministrationSteps |
| AC-041 | Suggested starting goals are checked in list order, and each by its first failing field | ArrangeSteps, EditSteps, ResultSteps |
| AC-042 | The administrator reads a Draft with all its fields and its state | ArrangeSteps, CatalogAdministrationSteps |
| AC-043 | Each of the six categories can be given to a catalog habit | ArrangeSteps, CatalogAdministrationSteps, BrowsingSteps |
| AC-044 | Only the 24 habit icons are accepted | ArrangeSteps, CatalogAdministrationSteps, ResultSteps |
| AC-045 | Names are ordered ordinally, ignoring case, not by the current culture | ArrangeSteps, BrowsingSteps |
| AC-046 | Publish and unpublish check the acting user and the role before the identifier | ArrangeSteps, CatalogAdministrationSteps, ResultSteps |
| AC-047 | A registered user creates a custom habit | ArrangeSteps, CustomHabitSteps, ResultSteps |
| AC-048 | A user's list holds only their own custom habits, in name order ignoring case | ArrangeSteps, CustomHabitSteps |
| AC-049 | A registered user edits every field of their custom habit | ArrangeSteps, EditSteps, CustomHabitSteps, ResultSteps |
| AC-050 | A registered user deletes their custom habit | ArrangeSteps, CustomHabitSteps, ResultSteps |
| AC-051 | A user's custom-habit name is taken, ignoring case and spaces | ArrangeSteps, CustomHabitSteps, EditSteps, ResultSteps |
| AC-052 | Another user, and a catalog habit, may have the same name | ArrangeSteps, CustomHabitSteps, BrowsingSteps, ResultSteps |
| AC-053 | Another registered user cannot reach a custom habit | ArrangeSteps, CustomHabitSteps, ResultSteps |
| AC-054 | An administrator has no access to custom habits, whatever the fields | ArrangeSteps, CustomHabitSteps, ResultSteps |
| AC-055 | The anonymous visitor has no access to custom habits | ArrangeSteps, CustomHabitSteps, ResultSteps |
| AC-056 | Field checks refuse a custom habit with the same messages | ArrangeSteps, CustomHabitSteps, ResultSteps |
| AC-057 | For custom habits the acting user and ownership come first, the name check last | ArrangeSteps, CustomHabitSteps, EditSteps, ResultSteps |
| AC-058 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits | ArrangeSteps, CustomHabitSteps, BrowsingSteps, ResultSteps |
| AC-059 | A custom habit with a name of 60 characters and a description of 500 characters is accepted | ArrangeSteps, CustomHabitSteps, ResultSteps |
| AC-060 | Demo loading loads the seed catalog into an empty catalog | ArrangeSteps, DemoLoadingSteps, BrowsingSteps, CatalogAdministrationSteps |
| AC-061 | Demo loading run again leaves the seed catalog unchanged | ArrangeSteps, DemoLoadingSteps, CatalogAdministrationSteps |
| AC-062 | Demo loading adds no seed to a catalog that has a habit | ArrangeSteps, DemoLoadingSteps, CatalogAdministrationSteps |
| AC-063 | Demo loading without a valid demo password creates no catalog and no custom habit | ArrangeSteps, DemoLoadingSteps, BrowsingSteps, ResultSteps |
| AC-064 | Demo accounts that demo loading creates get their demo custom habits | ArrangeSteps, DemoLoadingSteps |
| AC-065 | Demo loading run again leaves the demo accounts and their custom habits unchanged | ArrangeSteps, DemoLoadingSteps |
| AC-066 | Deleted demo custom habits are not created again | ArrangeSteps, DemoLoadingSteps |
| AC-067 | A demo email that a person registered first keeps its owner's data untouched | ArrangeSteps, DemoLoadingSteps |
| AC-068 | A demo email held by an administrator gets no custom habit | ArrangeSteps, DemoLoadingSteps |
| AC-069 | A Draft and an Unpublished habit can be edited and keep their state | ArrangeSteps, EditSteps, CatalogAdministrationSteps, BrowsingSteps, ResultSteps |
| AC-070 | Names and descriptions are trimmed before they are checked and stored | ArrangeSteps, CatalogAdministrationSteps, ResultSteps |
| AC-071 | The administrator edits the description, intent and icon of a catalog habit | ArrangeSteps, EditSteps, BrowsingSteps, ResultSteps |
| AC-072 | The Home preview has fewer than four habits when fewer are Published | ArrangeSteps, BrowsingSteps |
| AC-073 | Custom-habit names and descriptions are trimmed before they are checked and stored | ArrangeSteps, CustomHabitSteps, ResultSteps |

## Unexpressible steps

None.

## Cross-feature changes

| File | Change | Reason |
|---|---|---|
| `tests/HT.AcceptanceTests/Support/F001/AccountsDriver.cs` | `LoadDemoDataAsync` now resolves `IAccountStore` and `IDemoDataStore` from the use-case scope and calls `new DemoDataService(store, demoData)` instead of `new DemoDataService(store)`. It still runs `LoadAsync(demoPassword)` in its own scope. No step, value or assertion changed. | API change: the `DemoDataService` constructor now needs an `IDemoDataStore` (CS7036). |

Nothing in *Shared support* changed.

## Missing packages

None.

## Interpretations and assumptions

- All items: these acceptance-criteria conventions are applied literally. "the administrator `a`" means administrator creation with `admin2026` followed by a sign-in. "the registered user `x`" means registration with `walking42` followed by a sign-in. A Draft, Published or Unpublished habit is created by the item's administrator, then published, then unpublished, as its state requires. A field a step does not give is: no description, Other, Build, `book-open`, and no suggested starting goal. "a text of N characters" is `t`×N, "a unit of N characters" is `u`×N, and "a name of N characters with the prefix `P`" is P, a space, then `x` up to N. A list of one suggested starting goal has it marked. Given steps assert that their own arrangement succeeded.
- AC-001, AC-006, AC-009, AC-016, AC-017: `<acting user>` browses, asks for the category filter, or reads details. `CatalogService` (browse, category filter, Home preview, details) takes no acting user, and FR-008 gives every audience the same result. The step therefore checks that the named account exists in the scenario and then calls the same use case for every audience.
- "the result contains / is / has ..." after a browse When step reads that step's browse result. "browsing ..." in a Then step runs the browse use case at that point: anonymous, with no filter unless the step names one. "the administrator's list" is `ListAsync(item's administrator, no search)`, run at that point.
- "the result is "X"" checks the literal `Message` and also the named outcome that carries the text (ADR 0005). The text-to-outcome mapping is in `F002Text.OutcomeFor`. "Choose Build or Reduce." maps to `IntentMissing` for a habit-level result. For a result "with the position N" it maps to `BuildOrReduceMissing` (FR-005 vs FR-007). "the result is success" checks the success outcome of the use case: Created, Saved, Published, Unpublished, Deleted, Found or Loaded.
- AC-039 n04 vs AC-057 n03: the step "the registered user `x` edits an unknown identifier with the name `"   "`" is written the same way in both items. In AC-039 (US3-AS18) it is the catalog administration edit, with the expected result "Only administrators ...". In AC-057 (US4-AS11) it is the custom-habit edit, with the expected result "This custom habit does not exist.". The binding chooses the use case from the scenario's `AC-057` tag.
- Edits that name a habit ("edits `N` to ...", "edits `N` with the name ...", "edits it ...") use the edit of the habit's kind: catalog administration for a catalog habit, custom-habit edit for a custom habit. They keep every field the step does not change. This covers AC-030 and AC-031, where a registered user or the visitor edits a catalog habit, and AC-051 and AC-057, where a user edits a custom habit. Edits by identifier only start from the item defaults plus the step's fields: an unknown identifier (AC-032, AC-039) or another kind's identifier (AC-033, AC-058).
- AC-013, AC-042: "with the fields given above" / "that suggested starting goal" compares every FR-007 field of the view with the fields the Given sent. That includes Days being empty for an N-times schedule and Times and Period being null for fixed days (owner answers: values that do not belong are not stored).
- AC-034, AC-037: "given above" means the goals that the edit step sent. AC-037's "the unit X" checks `CountUnit` for count units and `DurationUnit` for minutes and hours. "that unit of 20 characters" (with or without the spaces) is `u`×20.
- AC-026: "exactly one habit named `N`" counts list entries whose name equals N under FR-004 (trimmed, ordinal, ignoring case), and that entry must be exactly N. So a created or renamed duplicate such as "a026 01 READING" would fail the check.
- AC-040: "the total number of catalog habits" must equal both the `Total` of the list with no search and the number of habits in that list (FR-012: the list has every catalog habit in every state).
- AC-028, AC-059, AC-070, AC-073: "that name", "that 500-character description", "the 60-character name" and "the 500-character description" are the texts the step built, without the whitespace-only parts it puts around them. The step also checks that they have 60 and 500 characters. "no description" is a null `Description`.
- AC-060: "the details of the Published habits" are the details of every habit that browsing (no filter) returns. "no two habits ... the same icon" reads each listed habit as the administrator, because the list items carry no icon.
- AC-064: "no result text of demo loading contains any of the names" checks every text of the result: its message, the catalog message, and each account's email, message and custom-habit message. The check ignores case.
- AC-065, AC-066: "`demoN@example.com` lists exactly ..." signs in with the password the scenario last used for that account, or else with the last demo password.
- AC-069: "reading them" means the two catalog habits of the item, in the order the Given created them (`A069 Draft`, then `A069 Old`).
- Feature vs acceptance criteria: no difference found. The `.feature` file matches `acceptance.md` item by item. Some names differ from the spec's own scenarios, for example US1-AS2 "Ac2 Cycling" vs AC-002 "A002 apple". Those are spec-vs-AC differences and are bound as the `.feature` file writes them.
- Database: as in F001, a `[BeforeFeature]` hook scoped to F002 resets HT-AC (ADR 0003: every run starts empty), so re-running the feature never meets its own earlier emails or names. Scenarios that need a known state also reset through their Given step.
