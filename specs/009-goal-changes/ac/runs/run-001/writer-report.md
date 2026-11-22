# Writer report — F009 / run-001

## Build

success, 1 attempt, no remaining errors (`dotnet build tests/HT.AcceptanceTests/HT.AcceptanceTests.csproj -p:AcRefMode=true -nologo -v q`: 0 warnings, 0 errors).

## Coverage

All step definition files are in `tests/HT.AcceptanceTests/StepDefinitions/F009/`; support in `tests/HT.AcceptanceTests/Support/F009/`
(F009Driver, F009Context, F009Arrange, F009Text, F009Checks, F009Hooks, F009Database). Every item uses `GivenSteps.cs` and
`WhenSteps.cs`; the table names the Then files.

| AC tag | Scenario name | Step definition file(s) |
|---|---|---|
| AC-001 | A change applies from the next date and keeps the name until then | ResultThenSteps, GoalThenSteps |
| AC-002 | The changed settings and name take effect at midnight | GoalThenSteps |
| AC-003 | Dates begun before a change keep the old target, later dates are judged by the new one | HistoryThenSteps |
| AC-004 | Consistency and streaks run across settings versions | ReportThenSteps |
| AC-005 | A correction is judged by the settings of its date | ResultThenSteps, HistoryThenSteps |
| AC-006 | A second change before the first takes effect replaces the waiting version | ResultThenSteps, GoalThenSteps |
| AC-007 | A per-week goal's change applies from the next Monday | ResultThenSteps, GoalThenSteps |
| AC-008 | The week in progress keeps its target, the next week is judged by the new one | HistoryThenSteps |
| AC-009 | The next boundary just before and just after midnight and a week's end | ResultThenSteps, GoalThenSteps |
| AC-010 | A change cannot switch the schedule kind | ResultThenSteps, GoalThenSteps |
| AC-011 | A change with a value that goal creation refuses is refused with its message | ResultThenSteps, GoalThenSteps |
| AC-012 | A change gives the first refusal in the order of FR-003 | ResultThenSteps, GoalThenSteps |
| AC-013 | The same settings and name give "settings unchanged" | ResultThenSteps, GoalThenSteps |
| AC-014 | A name-only change applies at once and adds no version | ResultThenSteps, GoalThenSteps |
| AC-015 | Clearing the name names the goal by its habit and target text again | ResultThenSteps, GoalThenSteps |
| AC-016 | The name is trimmed, an empty name is none, and a name changes at once with the settings | ResultThenSteps, GoalThenSteps |
| AC-017 | A change to a goal that has not begun replaces its first version | ResultThenSteps, GoalThenSteps |
| AC-018 | A settings change and a time zone change merge into the one later version | GoalThenSteps |
| AC-019 | A pause does not block a change | ResultThenSteps, GoalThenSteps |
| AC-020 | A change of measurement or of Build and Reduce keeps each date's own target | HistoryThenSteps |
| AC-021 | A value that does not belong to the chosen schedule is neither refused nor stored | ResultThenSteps, GoalThenSteps |
| AC-022 | A settings change has no time of making of its own | GoalThenSteps |
| AC-023 | Ending a goal stores the ending date and leaves the ending date out of the attempt | ResultThenSteps, GoalThenSteps, HistoryThenSteps |
| AC-024 | After ending, the Pending dates can still be recorded and the ending date cannot | HistoryThenSteps |
| AC-025 | An ended attempt's Pending dates become Missed as usual and ending adds no failure | ReportThenSteps, HistoryThenSteps |
| AC-026 | A correction of an ended attempt's date changes its figures | ResultThenSteps, ReportThenSteps |
| AC-027 | An ended goal has no commitment on or after its ending date | ResultThenSteps |
| AC-028 | Ending without an outcome is refused | ResultThenSteps, GoalThenSteps, HistoryThenSteps |
| AC-029 | An ending note longer than 500 characters is refused | ResultThenSteps, GoalThenSteps |
| AC-030 | An ending note of 500 characters is accepted, and the note is trimmed | ResultThenSteps, GoalThenSteps |
| AC-031 | An ended goal cannot be ended again | ResultThenSteps, GoalThenSteps |
| AC-032 | An ended goal cannot be changed or paused | ResultThenSteps, GoalThenSteps, HistoryThenSteps |
| AC-033 | Ending cuts the current pause to the last date and removes upcoming pauses | ResultThenSteps, HistoryThenSteps, ReportThenSteps |
| AC-034 | Ending a per-week goal mid-week leaves the week in progress out of the attempt | ResultThenSteps, ReportThenSteps, HistoryThenSteps |
| AC-035 | A goal ended before its start date keeps its first version | ResultThenSteps, ReportThenSteps, GoalThenSteps, AttemptThenSteps |
| AC-036 | A goal ended on its start date has no commitment | ResultThenSteps, ReportThenSteps, HistoryThenSteps |
| AC-037 | Ending removes a waiting later version | ResultThenSteps, GoalThenSteps |
| AC-038 | Ending removes a version that took effect on the ending date or in the week in progress | ResultThenSteps, GoalThenSteps |
| AC-039 | A record on the ending date is not counted | ResultThenSteps, ReportThenSteps, HistoryThenSteps |
| AC-040 | Excusals and records of an ended attempt's dates keep working | ResultThenSteps, HistoryThenSteps |
| AC-041 | An ending note is not a note of a commitment, and an ended goal's notes leave the dashboard | HistoryThenSteps, ReportThenSteps |
| AC-042 | A goal whose new attempt has not begun can be ended | ResultThenSteps, AttemptThenSteps |
| AC-043 | An ending has no time of making of its own | GoalThenSteps |
| AC-044 | The starting values of a new attempt copy the last settings and start after the ending date | GoalThenSteps |
| AC-045 | An ended goal is named and read by its last version, which also gives the starting values | GoalThenSteps |
| AC-046 | Starting a new attempt with the starting values | ResultThenSteps, GoalThenSteps, ReportThenSteps |
| AC-047 | A new attempt's figures count only its own results | ReportThenSteps |
| AC-048 | A new attempt with new settings that starts later | ResultThenSteps, GoalThenSteps, AttemptThenSteps |
| AC-049 | An earlier attempt's dates can be recorded while the new attempt waits to begin | ResultThenSteps, AttemptThenSteps |
| AC-050 | A new attempt can change the schedule kind | ResultThenSteps, ReportThenSteps |
| AC-051 | A start date in the past, on or before the ending date, or more than a year ahead is refused | ResultThenSteps, GoalThenSteps |
| AC-052 | The day after the ending date and a date 365 days ahead are accepted | ResultThenSteps |
| AC-053 | A new attempt with a value that goal creation refuses is refused with its message | ResultThenSteps, GoalThenSteps |
| AC-054 | Only an ended goal can start a new attempt | ResultThenSteps, GoalThenSteps |
| AC-055 | Dates between two attempts belong to none | ResultThenSteps |
| AC-056 | Reading a goal's attempts gives every attempt, the latest first | AttemptThenSteps |
| AC-057 | Correcting an earlier attempt changes only that attempt's figures | ResultThenSteps, AttemptThenSteps |
| AC-058 | Reading one earlier attempt gives its History and recent results | AttemptThenSteps |
| AC-059 | After a restart in the same week, the earlier attempt's Pending date stays recordable and its Missed date is not listed | HistoryThenSteps, ReportThenSteps |
| AC-060 | Recording the earlier attempt's Pending date counts in that attempt only | ResultThenSteps, AttemptThenSteps |
| AC-061 | Ending on the goal's timeline a day ahead of the account's today | ResultThenSteps, GoalThenSteps |
| AC-062 | A new attempt cannot start on the ending date when the account's today is a day behind | ResultThenSteps, GoalThenSteps |
| AC-063 | The day after the ending date is accepted while the account's today is a day behind | ResultThenSteps |
| AC-064 | A new attempt can start on an Unpublished catalog habit | ResultThenSteps, GoalThenSteps |
| AC-065 | A new attempt keeps the goal's identifier, habit and own name | ResultThenSteps, GoalThenSteps, AttemptThenSteps |
| AC-066 | Attempt numbers count up and are never reused | ResultThenSteps, AttemptThenSteps |
| AC-067 | A restarted per-week goal's first week starts on the first Monday on or after its start date | ResultThenSteps, HistoryThenSteps |
| AC-068 | An earlier attempt's History gives at most its newest 366 dates | AttemptThenSteps |
| AC-069 | A date of a new attempt cannot be recorded before its day begins | ResultThenSteps |
| AC-070 | A change to a new attempt that has not begun replaces its first version | ResultThenSteps, GoalThenSteps, AttemptThenSteps |
| AC-071 | The goals overview says which goal is ended, with its outcome and its latest attempt's figures | ReportThenSteps |
| AC-072 | The dashboard keeps an ended goal's Pending entries and nothing else of it | ReportThenSteps |
| AC-073 | Once its grace periods have ended, an ended goal leaves the dashboard | ReportThenSteps |
| AC-074 | An ended attempt's Missed result of the current week is not counted | ReportThenSteps |
| AC-075 | The goal history lists every ended attempt, the latest ending first | AttemptThenSteps |
| AC-076 | Attempts with the same ending date are ordered by name | AttemptThenSteps |
| AC-077 | A restarted goal is read with its new attempt, whose figures start empty | GoalThenSteps, ReportThenSteps |
| AC-078 | An ended goal is not tracked in the catalog | CatalogDemoThenSteps |
| AC-079 | After a restart, a goal's History, recent results and recent notes cover only its latest attempt | HistoryThenSteps, ReportThenSteps |
| AC-080 | An earlier attempt's Pending entry gives that attempt's name and Build or Reduce | HistoryThenSteps, ReportThenSteps |
| AC-081 | A time zone change adds no version to an ended goal | GoalThenSteps |
| AC-082 | A new attempt uses the account's time zone when it starts | ResultThenSteps, GoalThenSteps |
| AC-083 | A time zone change before a new attempt begins updates its first version | GoalThenSteps |
| AC-084 | Another registered user cannot use any use case of this slice on someone else's goal | ResultThenSteps, GoalThenSteps |
| AC-085 | An administrator cannot use any use case of this slice | ResultThenSteps, GoalThenSteps |
| AC-086 | An anonymous visitor cannot use any use case of this slice | ResultThenSteps, GoalThenSteps |
| AC-087 | A user's goal history contains only their own attempts | AttemptThenSteps |
| AC-088 | A goal without changes, ending or restart keeps slice 6's figures and is counted | ReportThenSteps, AttemptThenSteps |
| AC-089 | Demo goal-change loading creates `demo7@example.com` and names no private data in its results | CatalogDemoThenSteps |
| AC-090 | The demo figures of `demo7@example.com` hold on every weekday | ReportThenSteps, GoalThenSteps, AttemptThenSteps |
| AC-091 | The demo dates, settings, states and notes of `demo7@example.com` | GoalThenSteps, HistoryThenSteps, AttemptThenSteps |
| AC-092 | Demo goal-change loading leaves an existing `demo7@example.com` unchanged | CatalogDemoThenSteps, ReportThenSteps, AttemptThenSteps |
| AC-093 | Demo goal-change loading without a valid demo password creates nothing | CatalogDemoThenSteps |
| AC-094 | A demo goal whose catalog habit is not Published is left out | CatalogDemoThenSteps, ReportThenSteps, AttemptThenSteps |
| AC-095 | The earlier demo accounts get no settings change, ending or second attempt | AttemptThenSteps |
| AC-096 | An ended goal's evaluation calendar stops at the last date | HistoryThenSteps |
| AC-097 | The evaluation calendar gives no date between two attempts | HistoryThenSteps |
| AC-098 | A restarted per-week goal's evaluation calendar gives no week outside an attempt | HistoryThenSteps |
| AC-099 | A per-week goal ended on a Sunday or a Monday | ResultThenSteps, ReportThenSteps |
| AC-100 | "settings unchanged" compares with the latest version, also while it waits | ResultThenSteps, GoalThenSteps |
| AC-101 | Reading the latest attempt while it is not ended gives its History up to the current date | AttemptThenSteps |
| AC-102 | Reading an attempt that ended before it began gives an empty History | AttemptThenSteps |
| AC-103 | An earlier attempt is named by its habit's current name | AttemptThenSteps |
| AC-104 | Another registered user gets "This goal does not exist." before the ended check of a restart | ResultThenSteps, GoalThenSteps |
| AC-105 | A custom habit used only by an ended goal is still used by goals | CatalogDemoThenSteps, GoalThenSteps |
| AC-106 | A catalog habit used only by an ended goal is not unpublished without confirmation | CatalogDemoThenSteps |

## Unexpressible steps

None.

## Cross-feature changes

None.

## Missing packages

None.

## Interpretations and assumptions

- All items: "the settings in effect S, from D". `GoalDetails` gives the version in effect field by field but has no date property for it other than `TimeZoneAppliesFrom` (slice 4's date of the version in effect). The steps assert that date as the version's "from" date. "the later settings" are `GoalDetails.Later` (a `GoalSettingsView` with `AppliesFrom`).
- All items: settings reads are compared exactly. A field that does not belong to the measurement or schedule must be absent: no amount for yes/no, no count unit unless the goal is count-based, no duration unit unless it is duration-based, no number of times or period for fixed days, and no days for a times schedule. This follows FR-003, "neither refused nor stored", and makes AC-021 a real check.
- "changes the amount to A minutes" (AC-002, 006, 017, 018, 019, 022, 070, 100 and others): the step reads the goal at the step's time, takes its latest version (`Later`, or the version in effect when there is no later one), sets the amount to A and passes the goal's own name (or none when `HasOwnName` is false). This follows acceptance.md's definition.
- AC-012: `with the name ""` passes an empty string as the name, which is the most literal reading. FR-004 makes it "no name".
- AC-016 row 04: "two spaces, `x` repeated 60 times, two spaces" means two spaces, then 60 `x` characters, then two spaces.
- Setup time: setup that gives no time runs at the earliest time the item gives. When no step has given a time yet (AC-106's administrator, the first step of the item), it runs at the default goal-creation time 2026-11-02T08:00:00Z, which that item uses.
- "what can be recorded now" (AC-024, 028, 036, 059, 067, 080): slice 5's use case takes one goal. The When step reads it for every goal the actor has in the item. "it contains no entry for D" checks every goal read.
- Entry tuples of an attempt that is not the goal's latest (AC-059, 080):
  - For what can be recorded now, the tuple's goal field is checked against `HistoryEntry.AttemptName`, and Build or Reduce against `HistoryEntry.Settings.BuildOrReduce`.
  - For the dashboard's Today list, the goal field is checked against `TodayEntry.GoalName` and Build or Reduce against `TodayEntry.BuildOrReduce`. FR-019 says such an entry gives the attempt's name and Build or Reduce "in place of the goal's current ones".
  - Entries are found by the goal's identifier, date and state.
- History lists: "gives exactly …" compares the set of dates and periods, ignoring order. Slice 5's history order is not restated in spec.md. "newest first" and "oldest first" lists (AC-058, 101) are compared in order.
- "the attempts of … are, in this order, …" without "exactly" (AC-042, 048, 065, 091): the attempts written must appear in that relative order (latest first). Their count is checked only with "exactly". The goal history follows the same rule.
- "no entry of the history … carries the note …" (AC-041): the step also asserts that the history is not empty, so the check cannot pass on an empty list.
- AC-089 "no result contains …": the step checks the result texts of demo goal-change loading. The email of a result line is not included; `demo7@example.com` contains none of the listed strings anyway.
- AC-095: "each account … gives as created" is the set of emails with outcome Created in the results of the four earlier demo tasks. The step asserts that `demo1`, `demo4`, `demo5` and `demo6` are in it. Each account's goals come from slice 4's goal list.
- AC-078: "browsing the catalog without that filter" uses the acting user's browse with an empty filter and `onlyTracked: false`. The catalog details use the acting user's details use case. The catalog service is built with the `TimeProvider` overload.
- AC-106: "browsing the catalog as the anonymous visitor" uses the public browse use case with no acting user (search = the habit name). The catalog habit is created with no description and no suggested starting goals, then published.
- AC-106 "the habit is not unpublished": the unpublish result is not `Unpublished`, and the administrator's catalog list still shows the habit as Published. AC-105 "the custom habit is not deleted": the delete result is not `Deleted`, and the custom habit can still be read.
- AC-103 "renamed the custom habit `Meditation` to "Mindfulness"": the step reads the custom habit and calls the edit use case with its other fields unchanged.
- Goals created on catalog habits (AC-064, 078, 106) get their name from the system. Later steps that name such a goal look it up by that name in slice 4's goal list.
- AC-040 "it succeeds" accepts the success outcome of the use case the When step called: `Excused`, `Removed`, or a record outcome of `Recorded`, `Changed` or `Deleted`.
- No difference between the `.feature` scenarios and the acceptance-criteria items was found while binding.
