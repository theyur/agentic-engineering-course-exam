# Translation report — F004 / translate-001

## Build

success, 1 attempt, no remaining errors (`dotnet build tests/HT.AcceptanceTests/HT.AcceptanceTests.csproj -p:AcRefMode=true -nologo -v q`: 0 warnings, 0 errors).

Feature file: `tests/HT.AcceptanceTests/Features/F004/goals.feature` (one `Feature: Goals`, tagged `@F004`, 94 scenarios).

## Items

| AC id | Status (translated / issue) | Scenario name |
|---|---|---|
| AC-001 | translated | Starting from a catalog habit without a position gives its marked suggested starting goal |
| AC-002 | translated | Starting from a suggested starting goal position gives that suggestion |
| AC-003 | translated | A catalog habit without suggested starting goals gives only Build or Reduce from its intent |
| AC-004 | translated | A suggested starting goal position the habit does not have is refused |
| AC-005 | translated | Creating a goal with the starting values unchanged |
| AC-006 | translated | Creating a goal with changed values keeps the changed values |
| AC-007 | translated | Each field refusal of goal creation, with its message |
| AC-008 | translated | Goal creation gives the first refusal in FR-006 order |
| AC-009 | translated | The edges of every target range are accepted |
| AC-010 | translated | The start date is today or up to 365 days later |
| AC-011 | translated | A goal name has at most 60 characters after trimming |
| AC-012 | translated | A Reduce goal with an amount has 1 time per period |
| AC-013 | translated | Values that do not belong to the chosen measurement or schedule are neither refused nor stored |
| AC-014 | translated | A habit that is not a Published catalog habit is refused as a catalog habit |
| AC-015 | translated | A habit that changed between starting and creating is looked up again |
| AC-016 | translated | A goal without a name of its own is named after its habit and target |
| AC-017 | translated | Several goals on one habit are independent |
| AC-018 | translated | The target text follows the slice 2 rules, with the "avoided on" exception |
| AC-019 | translated | The catalog suggestion text of the starting values follows the goal rules |
| AC-020 | translated | Every suggested starting goal of the seed catalog can be created unchanged |
| AC-021 | translated | Listing gives every goal with its summary, in name order |
| AC-022 | translated | Goals are listed by name ignoring case, then in creation order |
| AC-023 | translated | The next commitment is the first commitment that has not ended |
| AC-024 | translated | A goal that has not started gives its start date |
| AC-025 | translated | Reading a goal gives all its fields, its settings version and its attempt |
| AC-026 | translated | Reading a goal that has not started gives its first version and attempt from its start date |
| AC-027 | translated | A user with no goals gets an empty list |
| AC-028 | translated | A goal stays as it was created |
| AC-029 | translated | Another user's goal does not exist for them |
| AC-030 | translated | A user's list never contains another user's goals |
| AC-031 | translated | An administrator gets no goal use case |
| AC-032 | translated | An anonymous visitor gets no goal use case |
| AC-033 | translated | A custom habit is looked up only among the acting user's custom habits |
| AC-034 | translated | Catalog goal counts are the acting user's own |
| AC-035 | translated | Use cases check the acting user, the role, the identifier and the fields, in this order |
| AC-036 | translated | A fixed-days goal has one commitment per scheduled day, from midnight to midnight in its time zone |
| AC-037 | translated | A per-day goal has one evaluation period per day |
| AC-038 | translated | A per-week goal has one evaluation period per Monday-to-Sunday week, from its first full week |
| AC-039 | translated | A per-week goal tells when its first week starts |
| AC-040 | translated | A day or week with a clock change keeps local midnights and lasts 23 or 25 hours |
| AC-041 | translated | The days around a clock change meet at local midnight |
| AC-042 | translated | A range before the start date gives an empty calendar |
| AC-043 | translated | The calendar range has 1 to 366 days |
| AC-044 | translated | "Today" is the date in the user's own time zone |
| AC-045 | translated | A fixed-days goal starting on an unscheduled day begins at its next scheduled day |
| AC-046 | translated | Starting from a custom habit gives only Build or Reduce from its intent |
| AC-047 | translated | A goal on a custom habit is based on it and named after it |
| AC-048 | translated | A custom habit that goals use cannot be deleted |
| AC-049 | translated | A custom habit that no goal uses is deleted |
| AC-050 | translated | Editing a custom habit renames its goals but never changes their settings |
| AC-051 | translated | Reading a custom habit gives the number of goals that use it |
| AC-052 | translated | Browsing gives a registered user their goal count per habit and the tracked count |
| AC-053 | translated | "Only habits I track" returns the Published habits with the user's goals, with the other filters |
| AC-054 | translated | A habit's details give the user's goal count and their goals on it, in name order |
| AC-055 | translated | Anonymous visitors and administrators get the catalog without goal information |
| AC-056 | translated | Browsing and details without an acting user give the slice 2 result |
| AC-057 | translated | Unpublishing a habit that goals use needs confirmation and gives only the number of goals |
| AC-058 | translated | Unpublishing with confirmation unpublishes a habit that goals use |
| AC-059 | translated | A habit that no goal uses is unpublished at once |
| AC-060 | translated | A goal on an Unpublished habit keeps working, and no new goal can use that habit |
| AC-061 | translated | A habit published again is in the catalog again for goals |
| AC-062 | translated | A renamed catalog habit renames the goals based on it |
| AC-063 | translated | A new goal can start without a habit, but not be created without one |
| AC-064 | translated | Demo loading gives each created demo account its demo goals |
| AC-065 | translated | Demo loading run again gives existing demo accounts no goals |
| AC-066 | translated | Demo goals are left out when their catalog habit is not Published |
| AC-067 | translated | A demo goal finds its catalog habit by name ignoring case, and only "Coffee" may be Unpublished |
| AC-068 | translated | Demo loading results name the demo email but never a goal, habit or account name |
| AC-069 | translated | The demo shows a goal on an Unpublished habit, a goal with its own name and an account name |
| AC-070 | translated | Registration keeps a named browser time zone, and otherwise gives the default |
| AC-071 | translated | After a move west, a fixed-days goal keeps the old time zone up to its current date |
| AC-072 | translated | Each goal follows a time zone change from its own next day or week |
| AC-073 | translated | Reading a goal gives the version of its current date and the one later version |
| AC-074 | translated | A value that is not a named time zone is refused and changes nothing |
| AC-075 | translated | A named time zone is kept as given, and the same time zone changes nothing |
| AC-076 | translated | "Today" follows the new time zone at once |
| AC-077 | translated | After a move west, a commitment already over is not the next one |
| AC-078 | translated | After a move east, a commitment in progress is still the next one |
| AC-079 | translated | Administrators read and change their own time zone; anonymous visitors cannot |
| AC-080 | translated | A time zone is given only to its own account |
| AC-081 | translated | A second change replaces a version not yet begun, and follows a version already in effect |
| AC-082 | translated | After a large move east, a version already begun is in effect, and a second change follows it |
| AC-083 | translated | A new version's first period that has already ended is kept |
| AC-084 | translated | An account name is trimmed, and empty means no name |
| AC-085 | translated | An account name has at most 60 characters |
| AC-086 | translated | The Home greeting follows the time of day in the account's time zone |
| AC-087 | translated | Without a name the greeting has no name |
| AC-088 | translated | The Home date is today in the account's time zone, in English whatever the culture |
| AC-089 | translated | The profile gives "member since" with the month and year of creation in the account's time zone |
| AC-090 | translated | An account name is given only to its own account; administrators have their own |
| AC-091 | translated | An anonymous visitor gets no Home heading, no name change and no profile |
| AC-092 | translated | A goal keeps the values it was created with, whatever happens to the suggested starting goal |
| AC-093 | translated | A confirmed unpublish acts even when goals were added after the count was given |
| AC-094 | translated | At registration, the name is checked after slice 1's checks |

## Issues

No issues.

## Interpretations

- All items: removed the leading list markers (`- `) from the step lines, indented the steps under their `Scenario` / `Scenario Outline`, and used the item's title (without the id and traceability) as the scenario name. Tags were built from the item's id and its traceability references, in the order the item gives them (`AC-005 (FR-001; FR-002; FR-005; FR-015; US1-AS5)` → `@AC-005 @FR-001 @FR-002 @FR-005 @FR-015 @US1-AS5`). `@F004` sits only on the `Feature:` line.
- AC-001, AC-004, AC-007 to AC-016, AC-018, AC-023, AC-029, AC-031 to AC-033, AC-035, AC-038 to AC-040, AC-043, AC-044, AC-046, AC-052, AC-053, AC-055, AC-057, AC-059, AC-060, AC-070, AC-072 to AC-075, AC-077, AC-078, AC-081, AC-084, AC-086, AC-088, AC-089, AC-091: each item with an `Examples:` table became a `Scenario Outline` with one indented `Examples:` table that has the same columns, rows and cell text (only the column padding changed). Column headers with spaces (`target text`, `start date`, `acting user`, `time zone`, `browser time zone`, `member since`, `goal name`) were kept as they are, as `<…>` placeholders.
- AC-008, AC-014, AC-015: Gherkin does not fill in placeholders inside Examples cells, so `<n>` written in a cell (for example `` `A4.008 <n> Draft` ``, `` `A4.014 <n> Gone` ``, `` the administrator unpublished `A4.015 <n> Reading` without confirming ``, `` `f4.ac015.user.<n>@example.com` deleted … ``) was replaced by the row's own `n` value (`` `A4.008 02 Draft` ``, `` `f4.ac015.user.02@example.com` ``). This is the value the conventions ("Examples": each email and catalog-habit name contains the row's `n`) define, and it is how AC-016, AC-029, AC-031 and others already write their cells.
- AC-020: "When, for every catalog habit …" became "When for every catalog habit …". The comma right after the keyword would stop Gherkin from recognising `When`.
- AC-021, AC-036, AC-041, AC-071: kept "Then … in this order:" as one step ending in a colon, with each listed entry as its own `And` step, as written.
- AC-085: the second line of the action ("And `f4.ac085.named@example.com` changes their name …") stays an `And` step after `When`, before `Then`, as written.
- AC-091: the item has no Given step; the scenario has only its `When` and `Then` steps. The `n` column is kept even though no step uses it.
- AC-024: no normalization. The last `And` step ("listed at `2026-11-25T00:00:00Z`, …") gives its own time and was kept word for word. The conventions' first time rule says "A step runs at the time the item gives for it".
- No item names a golden sample, so no Glob check of `tests/HT.AcceptanceTests/Samples/F004/` was needed. That folder does not exist.
