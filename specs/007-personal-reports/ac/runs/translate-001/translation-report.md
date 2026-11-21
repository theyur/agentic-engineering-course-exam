# Translation report — F007 / translate-001

## Build

success, 1 attempt, no remaining errors (`dotnet build tests/HT.AcceptanceTests/HT.AcceptanceTests.csproj -p:AcRefMode=true -nologo -v q`: 0 warnings, 0 errors).

Feature file: `tests/HT.AcceptanceTests/Features/F007/personal-reports.feature` (one `Feature: Personal reports`, tagged `@F007`).

## Items

| AC id | Status (translated / issue) | Scenario name |
|---|---|---|
| AC-001 | translated | Consistency counts Met against Met, Failed and Missed final results |
| AC-002 | translated | Pending and Due results are not counted, also when they reach the target |
| AC-003 | translated | A goal with no final result has the consistency "N/A" |
| AC-004 | translated | The percentage is rounded down to a whole number |
| AC-005 | translated | 199 Met of 200 final results is "99% (199 of 200)" |
| AC-006 | translated | Reduce results count like Build results |
| AC-007 | translated | Each commitment, day or week counts once, however many records it has |
| AC-008 | translated | Two goals on the same habit have their own figures and nothing combines them |
| AC-009 | translated | A Failed result resets the current streak |
| AC-010 | translated | The longest streak keeps the longest run of Met results |
| AC-011 | translated | A latest Failed or Missed result gives a current streak of 0 |
| AC-012 | translated | A Pending result neither extends nor resets the streak until it is Met |
| AC-013 | translated | A Pending result that becomes Missed resets the streak at its grace end |
| AC-014 | translated | Every streak carries the unit of its goal, singular for 1 |
| AC-015 | translated | A goal that has not started has streaks of 0, no recent results and no trend |
| AC-016 | translated | A late record for a Missed day joins the runs on both sides |
| AC-017 | translated | Changing a Met record to an amount below the target makes it Failed |
| AC-018 | translated | Deleting the only record of a final commitment makes it Missed |
| AC-019 | translated | Records entered late give the same report as records entered on time |
| AC-020 | translated | After a time zone change each result is evaluated with the version of its own date |
| AC-021 | translated | The recent results are the latest 30 ended results, oldest first |
| AC-022 | translated | The recent results count each state, and the figures count every result of the attempt |
| AC-023 | translated | A commitment-based duration goal has one bar per commitment, the last 14 |
| AC-024 | translated | A per-week goal has one bar per week, the last 12, with the week's progress |
| AC-025 | translated | A commitment-based yes/no goal has one bar per week with its commitments as the target |
| AC-026 | translated | A week's bar begins with its first commitment |
| AC-027 | translated | A recorded 0 is 0, and a bar without a record has no value, also for Reduce |
| AC-028 | translated | A Reduce yes/no week counts its "Yes" records against 0, a Build duration week counts records that reach the amount |
| AC-029 | translated | Each goal of the overview has its figures, its last 15 results and what can be recorded |
| AC-030 | translated | The overview puts goals with a Pending result first, then those with a Due result, each group in name and creation order |
| AC-031 | translated | The overview gives the progress of each Due entry |
| AC-032 | translated | The dashboard counts Due, Pending, Missed-this-week results and goals |
| AC-033 | translated | The Today list has Pending, then Due, then this week's Missed results |
| AC-034 | translated | "Missed this week" starts on Monday at 00:00 in the account's time zone and includes the previous Friday |
| AC-035 | translated | The dashboard lists every goal with its consistency and streaks in the overview order |
| AC-036 | translated | The recent notes are the 5 with the latest dates across all goals |
| AC-037 | translated | A user with no goals gets counts of 0 and empty lists |
| AC-038 | translated | Upcoming entries end the Today list for goals with no Due result, up to 6 days ahead |
| AC-039 | translated | Another user's goal does not exist for the goal report |
| AC-040 | translated | The dashboard and the overview contain only the acting user's goals |
| AC-041 | translated | An anonymous visitor and an administrator are refused by every use case of this slice |
| AC-042 | translated | demo4's Reading goal reads "92% (24 of 26)" with a streak of 3 and a longest of 18 |
| AC-043 | translated | demo4's Sugar goal reads "92% (24 of 26)" with a streak of 1 day and a longest of 16 days |
| AC-044 | translated | demo4's Yoga goal reads its weeks by the weekday of the run |
| AC-045 | translated | demo4's dashboard has Pending and Due results, no Missed one, and its notes |
| AC-046 | translated | Demo loading and demo history loading are unchanged, and demo1 to demo3 have no final result |
| AC-047 | translated | Demo report loading creates demo5 with its three goals |
| AC-048 | translated | demo5's reports on a Wednesday run |
| AC-049 | translated | demo5's Strength training and Screen time goals on other weekdays |
| AC-050 | translated | A second run of demo report loading leaves demo5 unchanged |
| AC-051 | translated | An account that already has the demo5 email, in any case, is left unchanged |
| AC-052 | translated | Without a valid demo password demo report loading creates nothing |
| AC-053 | translated | demo5 has one result Missed this week on every weekday |
| AC-054 | translated | demo5's dashboard on Wednesday 2026-11-18 |
| AC-055 | translated | A demo goal whose catalog habit is not Published is left out |
| AC-056 | translated | A per-week goal started mid-week reports from its first full week |
| AC-057 | translated | Around a clock change back, a grace period ends at 23:00 local time and a 25-hour day counts once |
| AC-058 | translated | A duration goal in hours has its trend values and target in minutes |
| AC-059 | translated | A goal whose catalog habit is Unpublished keeps its reports |
| AC-060 | translated | A per-week goal's Pending week is in the Today list |
| AC-061 | translated | Around a clock change forward, a grace period ends at 01:00 local time and a 23-hour day counts once |
| AC-062 | translated | A per-week or per-day result counts only from the end of its grace period |
| AC-063 | translated | The slice 4 goal list is unchanged, and the overview carries its entries |
| AC-064 | translated | A commitment-based yes/no trend keeps its last 12 weeks |
| AC-065 | translated | A per-day goal of 2 times has one bar per day with the day's progress against 2 |
| AC-066 | translated | demo5's amounts and notes on a Wednesday run |
| AC-067 | translated | "This week" follows the account's time zone, also for days of an older version |

## Issues

No issues.

Every item has an id and traceability. No item names a golden sample (the folder `tests/HT.AcceptanceTests/Samples/F007/` does not exist, and none is needed). I checked the expected figures of every item against the spec's rules and the demo data of FR-030 and FR-032. This includes grace ends across the Europe/Berlin clock changes (AC-057, AC-061), the Europe/Kyiv versions (AC-020, AC-034, AC-067), the demo4 and demo5 consistencies, streaks, trends and Today lists, and the ordering by FR-011. I found no contradiction.

## Interpretations

- All items: removed the Markdown list markers (`- `) in front of Given / When / Then / And. Kept every step's wording, backticks and double quotes as written. Each scenario's name is the item's title, verbatim. Each item's tags are its id followed by its traceability references in the given order.
- All items with an `Examples:` table (AC-003, AC-004, AC-006, AC-007, AC-011, AC-012, AC-014, AC-015, AC-020, AC-034, AC-039, AC-041, AC-044, AC-048, AC-049, AC-052, AC-053, AC-057, AC-061, AC-062, AC-067): written as a `Scenario Outline` with an `Examples:` table. The table has the same columns, header and cell values, with the Markdown separator row (`|---|`) removed.
- AC-003, AC-044, AC-048, AC-049, AC-052, AC-053: Examples columns that no step uses (`note`, or `n` in the demo items) are kept as given.
- AC-039, AC-041: some Examples cells contain `<n>`, for example "the goal of `f7.ac039.owner.<n>@example.com`" and "the administrator `f7.ac041.admin.<n>@example.com`". Gherkin does not fill in placeholders inside example values, so I wrote each such `<n>` as that row's `n` value (01 to 08). For example, row 05 of AC-041 reads "the administrator `f7.ac041.admin.05@example.com`" and "the goal report for the goal of `f7.ac041.owner.05@example.com`". This follows the acceptance criteria's own "Examples" convention, so the values are the same.
- AC-033, AC-045, AC-054: the nested bullet list under "the Today list is, in this order:" became a one-column data table with the header `entry`. Each bullet is one row, copied verbatim and in the same order.
- AC-035: the nested bullet list under "its goals are, in this order:" became a one-column data table with the header `goal`. Each bullet is one row, verbatim.
- AC-036, AC-045, AC-066: the nested bullet list under "the recent notes are, in this order:" (AC-066: "the dashboard's recent notes are, in this order:") became a one-column data table with the header `recent note`. Each bullet is one row, verbatim.
- AC-046, AC-047: the items have no Given step. "When the demo run at 2026-11-18T10:00:00Z" is kept verbatim as the When step, and the scenario has no Given.
- AC-013, AC-016, AC-017, AC-063: Given steps that read and state what they got ("read the report of the goal and got …", "reads the goals overview, which lists …") are kept verbatim as Given steps, as written.
- AC-016, AC-017, AC-018: When steps that change a record and then read the report ("… and reads the report of the goal") are kept as one When step, as written.
- AC-027: the When step that reads at two supplied times (2026-11-05T10:00:00Z and 2026-11-18T10:00:00Z) is kept as one When step, as written.
