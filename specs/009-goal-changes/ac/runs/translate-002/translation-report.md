# Translation report — F009 / translate-002

## Build

success, 1 attempt (`dotnet build tests/HT.AcceptanceTests/HT.AcceptanceTests.csproj -p:AcRefMode=true -nologo -v q`: 0 warnings, 0 errors). No remaining errors.

Feature file: `tests/HT.AcceptanceTests/Features/F009/goal-changes.feature` (one `Feature: Goal changes`, tagged `@F009`).

## Items

| AC id | Status (translated / issue) | Scenario name |
|---|---|---|
| AC-001 | translated | A change applies from the next date and keeps the name until then |
| AC-002 | translated | The changed settings and name take effect at midnight |
| AC-003 | translated | Dates begun before a change keep the old target, later dates are judged by the new one |
| AC-004 | translated | Consistency and streaks run across settings versions |
| AC-005 | translated | A correction is judged by the settings of its date |
| AC-006 | translated | A second change before the first takes effect replaces the waiting version |
| AC-007 | translated | A per-week goal's change applies from the next Monday |
| AC-008 | translated | The week in progress keeps its target, the next week is judged by the new one |
| AC-009 | translated | The next boundary just before and just after midnight and a week's end |
| AC-010 | translated | A change cannot switch the schedule kind |
| AC-011 | translated | A change with a value that goal creation refuses is refused with its message |
| AC-012 | translated | A change gives the first refusal in the order of FR-003 |
| AC-013 | translated | The same settings and name give "settings unchanged" |
| AC-014 | translated | A name-only change applies at once and adds no version |
| AC-015 | translated | Clearing the name names the goal by its habit and target text again |
| AC-016 | translated | The name is trimmed, an empty name is none, and a name changes at once with the settings |
| AC-017 | translated | A change to a goal that has not begun replaces its first version |
| AC-018 | translated | A settings change and a time zone change merge into the one later version |
| AC-019 | translated | A pause does not block a change |
| AC-020 | translated | A change of measurement or of Build and Reduce keeps each date's own target |
| AC-021 | translated | A value that does not belong to the chosen schedule is neither refused nor stored |
| AC-022 | translated | A settings change has no time of making of its own |
| AC-023 | translated | Ending a goal stores the ending date and leaves the ending date out of the attempt |
| AC-024 | translated | After ending, the Pending dates can still be recorded and the ending date cannot |
| AC-025 | translated | An ended attempt's Pending dates become Missed as usual and ending adds no failure |
| AC-026 | translated | A correction of an ended attempt's date changes its figures |
| AC-027 | translated | An ended goal has no commitment on or after its ending date |
| AC-028 | translated | Ending without an outcome is refused |
| AC-029 | translated | An ending note longer than 500 characters is refused |
| AC-030 | translated | An ending note of 500 characters is accepted, and the note is trimmed |
| AC-031 | translated | An ended goal cannot be ended again |
| AC-032 | translated | An ended goal cannot be changed or paused |
| AC-033 | translated | Ending cuts the current pause to the last date and removes upcoming pauses |
| AC-034 | translated | Ending a per-week goal mid-week leaves the week in progress out of the attempt |
| AC-035 | translated | A goal ended before its start date keeps its first version |
| AC-036 | translated | A goal ended on its start date has no commitment |
| AC-037 | translated | Ending removes a waiting later version |
| AC-038 | translated | Ending removes a version that took effect on the ending date or in the week in progress |
| AC-039 | translated | A record on the ending date is not counted |
| AC-040 | translated | Excusals and records of an ended attempt's dates keep working |
| AC-041 | translated | An ending note is not a note of a commitment, and an ended goal's notes leave the dashboard |
| AC-042 | translated | A goal whose new attempt has not begun can be ended |
| AC-043 | translated | An ending has no time of making of its own |
| AC-044 | translated | The starting values of a new attempt copy the last settings and start after the ending date |
| AC-045 | translated | An ended goal is named and read by its last version, which also gives the starting values |
| AC-046 | translated | Starting a new attempt with the starting values |
| AC-047 | translated | A new attempt's figures count only its own results |
| AC-048 | translated | A new attempt with new settings that starts later |
| AC-049 | translated | An earlier attempt's dates can be recorded while the new attempt waits to begin |
| AC-050 | translated | A new attempt can change the schedule kind |
| AC-051 | translated | A start date in the past, on or before the ending date, or more than a year ahead is refused |
| AC-052 | translated | The day after the ending date and a date 365 days ahead are accepted |
| AC-053 | translated | A new attempt with a value that goal creation refuses is refused with its message |
| AC-054 | translated | Only an ended goal can start a new attempt |
| AC-055 | translated | Dates between two attempts belong to none |
| AC-056 | translated | Reading a goal's attempts gives every attempt, the latest first |
| AC-057 | translated | Correcting an earlier attempt changes only that attempt's figures |
| AC-058 | translated | Reading one earlier attempt gives its History and recent results |
| AC-059 | translated | After a restart in the same week, the earlier attempt's Pending date stays recordable and its Missed date is not listed |
| AC-060 | translated | Recording the earlier attempt's Pending date counts in that attempt only |
| AC-061 | translated | Ending on the goal's timeline a day ahead of the account's today |
| AC-062 | translated | A new attempt cannot start on the ending date when the account's today is a day behind |
| AC-063 | translated | The day after the ending date is accepted while the account's today is a day behind |
| AC-064 | translated | A new attempt can start on an Unpublished catalog habit |
| AC-065 | translated | A new attempt keeps the goal's identifier, habit and own name |
| AC-066 | translated | Attempt numbers count up and are never reused |
| AC-067 | translated | A restarted per-week goal's first week starts on the first Monday on or after its start date |
| AC-068 | translated | An earlier attempt's History gives at most its newest 366 dates |
| AC-069 | translated | A date of a new attempt cannot be recorded before its day begins |
| AC-070 | translated | A change to a new attempt that has not begun replaces its first version |
| AC-071 | translated | The goals overview says which goal is ended, with its outcome and its latest attempt's figures |
| AC-072 | translated | The dashboard keeps an ended goal's Pending entries and nothing else of it |
| AC-073 | translated | Once its grace periods have ended, an ended goal leaves the dashboard |
| AC-074 | translated | An ended attempt's Missed result of the current week is not counted |
| AC-075 | translated | The goal history lists every ended attempt, the latest ending first |
| AC-076 | translated | Attempts with the same ending date are ordered by name |
| AC-077 | translated | A restarted goal is read with its new attempt, whose figures start empty |
| AC-078 | translated | An ended goal is not tracked in the catalog |
| AC-079 | translated | After a restart, a goal's History, recent results and recent notes cover only its latest attempt |
| AC-080 | translated | An earlier attempt's Pending entry gives that attempt's name and Build or Reduce |
| AC-081 | translated | A time zone change adds no version to an ended goal |
| AC-082 | translated | A new attempt uses the account's time zone when it starts |
| AC-083 | translated | A time zone change before a new attempt begins updates its first version |
| AC-084 | translated | Another registered user cannot use any use case of this slice on someone else's goal |
| AC-085 | translated | An administrator cannot use any use case of this slice |
| AC-086 | translated | An anonymous visitor cannot use any use case of this slice |
| AC-087 | translated | A user's goal history contains only their own attempts |
| AC-088 | translated | A goal without changes, ending or restart keeps slice 6's figures and is counted |
| AC-089 | translated | Demo goal-change loading creates `demo7@example.com` and names no private data in its results |
| AC-090 | translated | The demo figures of `demo7@example.com` hold on every weekday |
| AC-091 | translated | The demo dates, settings, states and notes of `demo7@example.com` |
| AC-092 | translated | Demo goal-change loading leaves an existing `demo7@example.com` unchanged |
| AC-093 | translated | Demo goal-change loading without a valid demo password creates nothing |
| AC-094 | translated | A demo goal whose catalog habit is not Published is left out |
| AC-095 | translated | The earlier demo accounts get no settings change, ending or second attempt |
| AC-096 | translated | An ended goal's evaluation calendar stops at the last date |
| AC-097 | translated | The evaluation calendar gives no date between two attempts |
| AC-098 | translated | A restarted per-week goal's evaluation calendar gives no week outside an attempt |
| AC-099 | translated | A per-week goal ended on a Sunday or a Monday |
| AC-100 | translated | "settings unchanged" compares with the latest version, also while it waits |
| AC-101 | translated | Reading the latest attempt while it is not ended gives its History up to the current date |
| AC-102 | translated | Reading an attempt that ended before it began gives an empty History |
| AC-103 | translated | An earlier attempt is named by its habit's current name |
| AC-104 | translated | Another registered user gets "This goal does not exist." before the ended check of a restart |
| AC-105 | translated | A custom habit used only by an ended goal is still used by goals |
| AC-106 | translated | A catalog habit used only by an ended goal is not unpublished without confirmation |

## Issues

No issues.

## Interpretations

- All items: the leading `- ` of each bullet was removed and the bullet's own keyword (`Given`, `When`, `Then`, `And`) was kept as the Gherkin step keyword. Step text is otherwise verbatim, including backticks, double quotes and the en dash in per-week periods (`2026-11-16 – 2026-11-22`).
- All items: the heading `### AC-NNN (refs) Title` became tags `@AC-NNN` plus one tag per traceability reference, in the order given, and the title became the scenario name verbatim. Items with an `Examples:` table became a `Scenario Outline` with an indented `Examples:` table. Columns were padded for alignment, and the header names and cell values are unchanged.
- Feature: titled "Goal changes" (spec title "Feature Specification: Goal changes") and tagged `@F009`. No Background was used, so each scenario keeps its own Given steps as written.
- AC-084, AC-085, AC-086, AC-104: the `action` cells contained the email `f9.acNNN.<n>.a@example.com`. Gherkin does not fill in placeholders that appear inside Examples cell values, so `<n>` there would have stayed as literal text. Each such `<n>` was replaced by that row's value in the `n` column (`01` to `07`), as the file's "Examples" convention defines `<n>`. So `f9.ac084.<n>.a@example.com` in row 01 became `f9.ac084.01.a@example.com`. `<n>` in the step text itself was left as a placeholder.
- AC-002, AC-038: the column headers containing spaces (`in effect`, `change time`, `end time`) were kept as written, and so were their placeholders (`<in effect>`, `<change time>`, `<end time>`). Reqnroll accepted them.
- AC-012, AC-016, AC-030: cells holding quoted empty or whitespace values (`""`, `"   "`, `"  Morning pages  "`, `"  Done.  "`) were kept with their quotes, so the whitespace survives Gherkin's trimming of cells.
- AC-094: the step "`f9.ac094.<n>.admin@example.com` unpublished <habits> with confirmation" has no time in the item. It was kept as written and no time was added. The file's Time convention covers it: the steps run in order, and setup with no time runs at the item's earliest time.
- Golden samples: no item names a golden sample, and the folder `tests/HT.AcceptanceTests/Samples/F009/` does not exist, so there was nothing to check.
