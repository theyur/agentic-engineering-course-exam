# Translation report — F002 / translate-001

## Build

success, 1 attempt (`dotnet build tests/HT.AcceptanceTests/HT.AcceptanceTests.csproj -p:AcRefMode=true -nologo -v q`: 0 warnings, 0 errors). No remaining errors.

Feature file: `tests/HT.AcceptanceTests/Features/F002/habit-catalog.feature`. It has one `Feature: Habit catalog` tagged `@F002`, with 73 scenarios: 43 `Scenario Outline` and 30 `Scenario`.

Golden samples: no AC item names a golden sample. The folder `tests/HT.AcceptanceTests/Samples/F002/` does not exist, and no scenario needs it.

## Items

| AC id | Status (translated / issue) | Scenario name |
|---|---|---|
| AC-001 | translated | Browsing returns only Published habits, with their card content, to every audience |
| AC-002 | translated | Browsing orders habits by name, ignoring case |
| AC-003 | translated | Browsing by category returns only that category |
| AC-004 | translated | Browsing by intent returns only that intent |
| AC-005 | translated | Search matches part of the name, trimmed and ignoring case |
| AC-006 | translated | Custom habits never appear in browsing, for any audience |
| AC-007 | translated | Browsing with no match gives an empty result |
| AC-008 | translated | The category filter starts with All and follows the fixed order |
| AC-009 | translated | The category filter counts only Published catalog habits |
| AC-010 | translated | Category, intent and search apply together |
| AC-011 | translated | Browsing gives the count of the habits it returns |
| AC-012 | translated | The Home preview is the first four Published habits in name order |
| AC-013 | translated | Details give every suggested starting goal in order, with its text and kind |
| AC-014 | translated | A habit without suggested starting goals is shown without one |
| AC-015 | translated | The text and kind of a suggested starting goal follow fixed rules |
| AC-016 | translated | Details of a habit that is not Published give the same refusal to every audience |
| AC-017 | translated | Details with a custom habit's identifier give "not in the catalog" |
| AC-018 | translated | A new catalog habit is a Draft that the public catalog does not show |
| AC-019 | translated | Publishing a Draft makes it public |
| AC-020 | translated | An edit of a Published habit is public on the next read |
| AC-021 | translated | Unpublishing a Published habit takes it out of the catalog at once |
| AC-022 | translated | An Unpublished habit can be published again |
| AC-023 | translated | A Draft cannot be unpublished |
| AC-024 | translated | Publishing a Published habit is a success that changes nothing |
| AC-025 | translated | Unpublishing an Unpublished habit is a success that changes nothing |
| AC-026 | translated | A catalog-habit name is taken in every state, ignoring case and spaces |
| AC-027 | translated | Field checks refuse a catalog habit with a message per field |
| AC-028 | translated | A name of 60 characters and a description of 500 characters are accepted |
| AC-029 | translated | Field checks come in a fixed order |
| AC-030 | translated | A registered user cannot manage the catalog |
| AC-031 | translated | The anonymous visitor cannot manage the catalog |
| AC-032 | translated | An unknown identifier does not exist for the administrator |
| AC-033 | translated | A custom habit is not a catalog habit for the administrator |
| AC-034 | translated | Suggested starting goals keep their order and the card shows the marked one |
| AC-035 | translated | The list of suggested starting goals is checked for its length first, then for its mark |
| AC-036 | translated | Each field of a suggested starting goal has its own refusal, with the position |
| AC-037 | translated | The bounds of a suggested starting goal's fields are accepted |
| AC-038 | translated | A Reduce amount of 0 is accepted and shown as "at most 0" |
| AC-039 | translated | The role and the identifier are checked before the fields |
| AC-040 | translated | The administrator's list is in name order and can be searched |
| AC-041 | translated | Suggested starting goals are checked in list order, and each by its first failing field |
| AC-042 | translated | The administrator reads a Draft with all its fields and its state |
| AC-043 | translated | Each of the six categories can be given to a catalog habit |
| AC-044 | translated | Only the 24 habit icons are accepted |
| AC-045 | translated | Names are ordered ordinally, ignoring case, not by the current culture |
| AC-046 | translated | Publish and unpublish check the acting user and the role before the identifier |
| AC-047 | translated | A registered user creates a custom habit |
| AC-048 | translated | A user's list holds only their own custom habits, in name order ignoring case |
| AC-049 | translated | A registered user edits every field of their custom habit |
| AC-050 | translated | A registered user deletes their custom habit |
| AC-051 | translated | A user's custom-habit name is taken, ignoring case and spaces |
| AC-052 | translated | Another user, and a catalog habit, may have the same name |
| AC-053 | translated | Another registered user cannot reach a custom habit |
| AC-054 | translated | An administrator has no access to custom habits, whatever the fields |
| AC-055 | translated | The anonymous visitor has no access to custom habits |
| AC-056 | translated | Field checks refuse a custom habit with the same messages |
| AC-057 | translated | For custom habits the acting user and ownership come first, the name check last |
| AC-058 | translated | A catalog habit's identifier and a deleted custom habit do not exist as custom habits |
| AC-059 | translated | A custom habit with a name of 60 characters and a description of 500 characters is accepted |
| AC-060 | translated | Demo loading loads the seed catalog into an empty catalog |
| AC-061 | translated | Demo loading run again leaves the seed catalog unchanged |
| AC-062 | translated | Demo loading adds no seed to a catalog that has a habit |
| AC-063 | translated | Demo loading without a valid demo password creates no catalog and no custom habit |
| AC-064 | translated | Demo accounts that demo loading creates get their demo custom habits |
| AC-065 | translated | Demo loading run again leaves the demo accounts and their custom habits unchanged |
| AC-066 | translated | Deleted demo custom habits are not created again |
| AC-067 | translated | A demo email that a person registered first keeps its owner's data untouched |
| AC-068 | translated | A demo email held by an administrator gets no custom habit |
| AC-069 | translated | A Draft and an Unpublished habit can be edited and keep their state |
| AC-070 | translated | Names and descriptions are trimmed before they are checked and stored |
| AC-071 | translated | The administrator edits the description, intent and icon of a catalog habit |
| AC-072 | translated | The Home preview has fewer than four habits when fewer are Published |
| AC-073 | translated | Custom-habit names and descriptions are trimmed before they are checked and stored |

## Issues

No issues.

I checked these against the spec and found them consistent: the expected texts and kinds in AC-013, AC-015 (all 16 rows), AC-034 and AC-038 against FR-017; the check orders in AC-029, AC-035, AC-036, AC-039, AC-041, AC-046, AC-054 and AC-057 against FR-005, FR-006, FR-007 and FR-016; the expected name orders in AC-002, AC-011, AC-012, AC-040, AC-045, AC-048, AC-052 and AC-072 against FR-004, with ordinal comparison ignoring case; and the demo-loading results in AC-060 to AC-068 against FR-030, FR-031 and slice 1 FR-016 as restated in the spec. AC-012 and AC-072 also reset the database, which the spec does not list among the known-state scenarios. The acceptance-criteria conventions explain this (the Home preview depends on every Published habit), and it does not conflict with the spec.

## Interpretations

- All items: I dropped each Markdown list bullet (`- `) and made each bullet one Gherkin step, keeping its keyword (`Given` / `And` / `When` / `Then`) and its wording unchanged. Each heading `### AC-NNN (refs) Title` became tags `@AC-NNN` plus one tag per traceability reference, and the scenario name is the heading's title. Values in backticks and quotes are copied unchanged.
- Items with an `Examples:` table (AC-001, 004, 005, 006, 009, 015, 016, 017, 026, 027, 029, 030, 031, 032, 033, 035, 036, 037, 039, 041, 043, 044, 046, 051, 053, 054, 055, 056, 057, 058, 063, 070, 073): these became a `Scenario Outline`. Each table now sits under the outline as Gherkin `Examples:`, without the Markdown header separator row (`|---|---|`), and its columns are aligned. Column names, including names with spaces (`acting user`, `demo password`, `stored unit`, `stored name`, `stored description`) and the result columns `Sugar` / `Water` / `Morning` / `Evening`, stay as written and are used as the `<placeholder>` names in the steps.
- AC-032, AC-033, AC-053: the `action` cells that end with a trailing comma (for example "edits, with the name `A032 02 Name`, … the icon `book-open`,") are copied with that comma, so the step reads as the item writes it.
- AC-064: the table under "And signed in with `example2026`, each demo account lists exactly these custom habits:" became a Gherkin data table on that step. I removed its Markdown separator row. The two `And` steps after the table follow it as separate steps.
- AC-014, AC-043: the `When` line names two actions ("browses … and reads the details …", "creates … and publishes it"). I kept it as one `When` step, as written.
- AC-034, AC-049: the second action line ("And the administrator publishes …", "And then edits it, …") is an `And` step after the `When`, with its wording unchanged, "then" included.
- AC-045: the parenthetical about `É` / `é` coming after A–Z when compared ordinally stays part of the `Then` step text, unchanged.
- AC-021, AC-063: the qualifiers "as one call with nothing before or after it" and "where absent means that no value is supplied" stay in the `When` step text, unchanged.
