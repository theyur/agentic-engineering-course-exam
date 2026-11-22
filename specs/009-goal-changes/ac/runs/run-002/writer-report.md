# Writer report — F009 / run-002

## Build
success, 1 attempt, no remaining errors (`dotnet build tests/HT.AcceptanceTests/HT.AcceptanceTests.csproj -p:AcRefMode=true -nologo -v q`: 0 warnings, 0 errors).

The step definitions and support code for F009 already existed. In this run I checked every step of every scenario
in the current `goal-changes.feature` (AC-001 to AC-106, outlines included) against the bound regular expressions
and the phrase readers in `Support/F009/F009Text.cs` and `F009Checks.cs`. Each step matches exactly one binding in
the F009 scope, and every phrase the readers parse (settings, history entries, tuples, attempts, goal history
entries, goal state clauses, figures) is read in full: a reader throws on any leftover text, so no part of a step
is silently dropped. No code change was needed.

## Coverage
| AC tag | Scenario name | Step definition file(s) |
|---|---|---|
| AC-001 | A change applies from the next date and keeps the name until then | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-002 | The changed settings and name take effect at midnight | GivenSteps.cs, WhenSteps.cs, GoalThenSteps.cs |
| AC-003 | Dates begun before a change keep the old target, later dates are judged by the new one | GivenSteps.cs, WhenSteps.cs, HistoryThenSteps.cs |
| AC-004 | Consistency and streaks run across settings versions | GivenSteps.cs, WhenSteps.cs, ReportThenSteps.cs |
| AC-005 | A correction is judged by the settings of its date | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, HistoryThenSteps.cs |
| AC-006 | A second change before the first takes effect replaces the waiting version | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-007 | A per-week goal's change applies from the next Monday | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-008 | The week in progress keeps its target, the next week is judged by the new one | GivenSteps.cs, WhenSteps.cs, HistoryThenSteps.cs |
| AC-009 | The next boundary just before and just after midnight and a week's end | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-010 | A change cannot switch the schedule kind | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-011 | A change with a value that goal creation refuses is refused with its message | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-012 | A change gives the first refusal in the order of FR-003 | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-013 | The same settings and name give "settings unchanged" | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-014 | A name-only change applies at once and adds no version | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-015 | Clearing the name names the goal by its habit and target text again | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-016 | The name is trimmed, an empty name is none, and a name changes at once with the settings | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-017 | A change to a goal that has not begun replaces its first version | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-018 | A settings change and a time zone change merge into the one later version | GivenSteps.cs, WhenSteps.cs, GoalThenSteps.cs |
| AC-019 | A pause does not block a change | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-020 | A change of measurement or of Build and Reduce keeps each date's own target | GivenSteps.cs, WhenSteps.cs, HistoryThenSteps.cs |
| AC-021 | A value that does not belong to the chosen schedule is neither refused nor stored | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-022 | A settings change has no time of making of its own | GivenSteps.cs, WhenSteps.cs, GoalThenSteps.cs |
| AC-023 | Ending a goal stores the ending date and leaves the ending date out of the attempt | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs, HistoryThenSteps.cs |
| AC-024 | After ending, the Pending dates can still be recorded and the ending date cannot | GivenSteps.cs, WhenSteps.cs, HistoryThenSteps.cs |
| AC-025 | An ended attempt's Pending dates become Missed as usual and ending adds no failure | GivenSteps.cs, WhenSteps.cs, ReportThenSteps.cs, HistoryThenSteps.cs |
| AC-026 | A correction of an ended attempt's date changes its figures | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, ReportThenSteps.cs |
| AC-027 | An ended goal has no commitment on or after its ending date | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs |
| AC-028 | Ending without an outcome is refused | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs, HistoryThenSteps.cs |
| AC-029 | An ending note longer than 500 characters is refused | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-030 | An ending note of 500 characters is accepted, and the note is trimmed | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-031 | An ended goal cannot be ended again | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-032 | An ended goal cannot be changed or paused | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs, HistoryThenSteps.cs |
| AC-033 | Ending cuts the current pause to the last date and removes upcoming pauses | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, HistoryThenSteps.cs, ReportThenSteps.cs |
| AC-034 | Ending a per-week goal mid-week leaves the week in progress out of the attempt | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, ReportThenSteps.cs, HistoryThenSteps.cs |
| AC-035 | A goal ended before its start date keeps its first version | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, ReportThenSteps.cs, GoalThenSteps.cs, AttemptThenSteps.cs |
| AC-036 | A goal ended on its start date has no commitment | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, ReportThenSteps.cs, HistoryThenSteps.cs |
| AC-037 | Ending removes a waiting later version | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-038 | Ending removes a version that took effect on the ending date or in the week in progress | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-039 | A record on the ending date is not counted | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, ReportThenSteps.cs, HistoryThenSteps.cs |
| AC-040 | Excusals and records of an ended attempt's dates keep working | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, HistoryThenSteps.cs |
| AC-041 | An ending note is not a note of a commitment, and an ended goal's notes leave the dashboard | GivenSteps.cs, WhenSteps.cs, HistoryThenSteps.cs, ReportThenSteps.cs |
| AC-042 | A goal whose new attempt has not begun can be ended | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, AttemptThenSteps.cs |
| AC-043 | An ending has no time of making of its own | GivenSteps.cs, WhenSteps.cs, GoalThenSteps.cs |
| AC-044 | The starting values of a new attempt copy the last settings and start after the ending date | GivenSteps.cs, WhenSteps.cs, GoalThenSteps.cs |
| AC-045 | An ended goal is named and read by its last version, which also gives the starting values | GivenSteps.cs, WhenSteps.cs, GoalThenSteps.cs |
| AC-046 | Starting a new attempt with the starting values | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs, ReportThenSteps.cs |
| AC-047 | A new attempt's figures count only its own results | GivenSteps.cs, WhenSteps.cs, ReportThenSteps.cs |
| AC-048 | A new attempt with new settings that starts later | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs, AttemptThenSteps.cs |
| AC-049 | An earlier attempt's dates can be recorded while the new attempt waits to begin | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, AttemptThenSteps.cs |
| AC-050 | A new attempt can change the schedule kind | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, ReportThenSteps.cs |
| AC-051 | A start date in the past, on or before the ending date, or more than a year ahead is refused | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-052 | The day after the ending date and a date 365 days ahead are accepted | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs |
| AC-053 | A new attempt with a value that goal creation refuses is refused with its message | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-054 | Only an ended goal can start a new attempt | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-055 | Dates between two attempts belong to none | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs |
| AC-056 | Reading a goal's attempts gives every attempt, the latest first | GivenSteps.cs, WhenSteps.cs, AttemptThenSteps.cs |
| AC-057 | Correcting an earlier attempt changes only that attempt's figures | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, AttemptThenSteps.cs |
| AC-058 | Reading one earlier attempt gives its History and recent results | GivenSteps.cs, WhenSteps.cs, AttemptThenSteps.cs |
| AC-059 | After a restart in the same week, the earlier attempt's Pending date stays recordable and its Missed date is not listed | GivenSteps.cs, WhenSteps.cs, HistoryThenSteps.cs, ReportThenSteps.cs |
| AC-060 | Recording the earlier attempt's Pending date counts in that attempt only | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, AttemptThenSteps.cs |
| AC-061 | Ending on the goal's timeline a day ahead of the account's today | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-062 | A new attempt cannot start on the ending date when the account's today is a day behind | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-063 | The day after the ending date is accepted while the account's today is a day behind | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs |
| AC-064 | A new attempt can start on an Unpublished catalog habit | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-065 | A new attempt keeps the goal's identifier, habit and own name | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs, AttemptThenSteps.cs |
| AC-066 | Attempt numbers count up and are never reused | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, AttemptThenSteps.cs |
| AC-067 | A restarted per-week goal's first week starts on the first Monday on or after its start date | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, HistoryThenSteps.cs |
| AC-068 | An earlier attempt's History gives at most its newest 366 dates | GivenSteps.cs, WhenSteps.cs, AttemptThenSteps.cs |
| AC-069 | A date of a new attempt cannot be recorded before its day begins | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs |
| AC-070 | A change to a new attempt that has not begun replaces its first version | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs, AttemptThenSteps.cs |
| AC-071 | The goals overview says which goal is ended, with its outcome and its latest attempt's figures | GivenSteps.cs, WhenSteps.cs, ReportThenSteps.cs |
| AC-072 | The dashboard keeps an ended goal's Pending entries and nothing else of it | GivenSteps.cs, WhenSteps.cs, ReportThenSteps.cs |
| AC-073 | Once its grace periods have ended, an ended goal leaves the dashboard | GivenSteps.cs, WhenSteps.cs, ReportThenSteps.cs |
| AC-074 | An ended attempt's Missed result of the current week is not counted | GivenSteps.cs, WhenSteps.cs, ReportThenSteps.cs |
| AC-075 | The goal history lists every ended attempt, the latest ending first | GivenSteps.cs, WhenSteps.cs, AttemptThenSteps.cs |
| AC-076 | Attempts with the same ending date are ordered by name | GivenSteps.cs, WhenSteps.cs, AttemptThenSteps.cs |
| AC-077 | A restarted goal is read with its new attempt, whose figures start empty | GivenSteps.cs, WhenSteps.cs, GoalThenSteps.cs, ReportThenSteps.cs |
| AC-078 | An ended goal is not tracked in the catalog | GivenSteps.cs, WhenSteps.cs, CatalogDemoThenSteps.cs |
| AC-079 | After a restart, a goal's History, recent results and recent notes cover only its latest attempt | GivenSteps.cs, WhenSteps.cs, HistoryThenSteps.cs, ReportThenSteps.cs |
| AC-080 | An earlier attempt's Pending entry gives that attempt's name and Build or Reduce | GivenSteps.cs, WhenSteps.cs, HistoryThenSteps.cs, ReportThenSteps.cs |
| AC-081 | A time zone change adds no version to an ended goal | GivenSteps.cs, WhenSteps.cs, GoalThenSteps.cs |
| AC-082 | A new attempt uses the account's time zone when it starts | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-083 | A time zone change before a new attempt begins updates its first version | GivenSteps.cs, WhenSteps.cs, GoalThenSteps.cs |
| AC-084 | Another registered user cannot use any use case of this slice on someone else's goal | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-085 | An administrator cannot use any use case of this slice | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-086 | An anonymous visitor cannot use any use case of this slice | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-087 | A user's goal history contains only their own attempts | GivenSteps.cs, WhenSteps.cs, AttemptThenSteps.cs |
| AC-088 | A goal without changes, ending or restart keeps slice 6's figures and is counted | GivenSteps.cs, WhenSteps.cs, ReportThenSteps.cs, AttemptThenSteps.cs |
| AC-089 | Demo goal-change loading creates `demo7@example.com` and names no private data in its results | GivenSteps.cs, WhenSteps.cs, CatalogDemoThenSteps.cs |
| AC-090 | The demo figures of `demo7@example.com` hold on every weekday | GivenSteps.cs, WhenSteps.cs, ReportThenSteps.cs, GoalThenSteps.cs, AttemptThenSteps.cs |
| AC-091 | The demo dates, settings, states and notes of `demo7@example.com` | GivenSteps.cs, WhenSteps.cs, GoalThenSteps.cs, HistoryThenSteps.cs, AttemptThenSteps.cs |
| AC-092 | Demo goal-change loading leaves an existing `demo7@example.com` unchanged | GivenSteps.cs, WhenSteps.cs, CatalogDemoThenSteps.cs, ReportThenSteps.cs, AttemptThenSteps.cs |
| AC-093 | Demo goal-change loading without a valid demo password creates nothing | GivenSteps.cs, WhenSteps.cs, CatalogDemoThenSteps.cs |
| AC-094 | A demo goal whose catalog habit is not Published is left out | GivenSteps.cs, WhenSteps.cs, CatalogDemoThenSteps.cs, ReportThenSteps.cs, AttemptThenSteps.cs |
| AC-095 | The earlier demo accounts get no settings change, ending or second attempt | GivenSteps.cs, WhenSteps.cs, AttemptThenSteps.cs |
| AC-096 | An ended goal's evaluation calendar stops at the last date | GivenSteps.cs, WhenSteps.cs, HistoryThenSteps.cs |
| AC-097 | The evaluation calendar gives no date between two attempts | GivenSteps.cs, WhenSteps.cs, HistoryThenSteps.cs |
| AC-098 | A restarted per-week goal's evaluation calendar gives no week outside an attempt | GivenSteps.cs, WhenSteps.cs, HistoryThenSteps.cs |
| AC-099 | A per-week goal ended on a Sunday or a Monday | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, ReportThenSteps.cs |
| AC-100 | "settings unchanged" compares with the latest version, also while it waits | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-101 | Reading the latest attempt while it is not ended gives its History up to the current date | GivenSteps.cs, WhenSteps.cs, AttemptThenSteps.cs |
| AC-102 | Reading an attempt that ended before it began gives an empty History | GivenSteps.cs, WhenSteps.cs, AttemptThenSteps.cs |
| AC-103 | An earlier attempt is named by its habit's current name | GivenSteps.cs, WhenSteps.cs, AttemptThenSteps.cs |
| AC-104 | Another registered user gets "This goal does not exist." before the ended check of a restart | GivenSteps.cs, WhenSteps.cs, ResultThenSteps.cs, GoalThenSteps.cs |
| AC-105 | A custom habit used only by an ended goal is still used by goals | GivenSteps.cs, WhenSteps.cs, CatalogDemoThenSteps.cs, GoalThenSteps.cs |
| AC-106 | A catalog habit used only by an ended goal is not unpublished without confirmation | GivenSteps.cs, WhenSteps.cs, CatalogDemoThenSteps.cs |

All files are in `tests/HT.AcceptanceTests/StepDefinitions/F009/`. Shared state, the use-case driver, phrase readers,
assertions and the feature hook are in `tests/HT.AcceptanceTests/Support/F009/` (F009Context, F009Driver,
F009Arrange, F009Text, F009Checks, F009Database, F009Hooks).

## Unexpressible steps
None.

## Cross-feature changes
None.

## Missing packages
None.

## Interpretations and assumptions
- All items, time: setup that gives no time (registration, sign-in, administrator creation, custom habit creation) runs at the earliest time the item gives, counting the default goal-creation time 2026-11-02T08:00:00Z when the item uses it (acceptance.md "Time"). Then steps run at the When step's time unless they give their own. A step at an earlier time than the step before it gets a fresh `FakeTimeProvider` started at that time.
- AC-002, AC-006, AC-017, AC-018, AC-019, AC-022, AC-037, AC-045, AC-070, AC-100: "changes/changed the amount to A minutes" is FR-001's use case with the goal's latest settings version, read just before (the later version if there is one, otherwise the version in effect), with only the amount replaced, and the goal's own name, or no name when it has none (acceptance.md "Use cases of this slice").
- All "the settings in effect …, from D" steps: the date is checked against `GoalDetails.TimeZoneAppliesFrom`. That is the only applies-from date `GoalDetails` gives for the version in effect, and FR-005 says the version is "no longer only the time zone". The later version is checked against `GoalDetails.Later` (`GoalSettingsView.AppliesFrom`). A time zone is checked only when the step writes one.
- All settings comparisons: values that do not belong to the measurement or the schedule are expected to be absent from the read (FR-003, "neither refused nor stored"). For example, AC-021 expects no number of times and no period on the fixed-days later version.
- AC-012 rows 01–03: the name `""` is passed as an empty string, as written. AC-016 row 02 (`"   "`) passes three spaces. The trimming is left to the system.
- AC-024, AC-059, AC-080 "it contains (…)": the When step reads what can be recorded now for every goal of the actor in the item. Each tuple is looked up in its goal's entries by its date or period. For an entry of an attempt that is not the latest, the tuple's goal name is checked against the entry's attempt name (acceptance.md: "the goal field of such an entry is that attempt's name").
- AC-059 "it contains no entry for D": checked across every goal of the actor that the When step read.
- AC-059, AC-072–AC-074, AC-088, AC-090 dashboard counts: "the Missed count" is `DashboardCounts.MissedThisWeek`, the dashboard's only Missed count. "the number of goals" is `DashboardCounts.Goals`.
- AC-033, AC-071, AC-088, AC-090 overview: "not paused" means `PausedUntil` is null. "ended on D with the outcome O" checks `IsEnded`, `EndingDate` and `Outcome` of the overview item.
- AC-064, AC-078: "the catalog is seeded at T" resets the AC database first, then runs demo loading (acceptance.md "Demo data"). Catalog habit identifiers come from the anonymous catalog browse, by exact name.
- AC-065: "reading the goal by the identifier it was created with" reads with the `GoalId` that goal creation returned, and checks that the read gives the same identifier.
- AC-095: the accounts are those that demo loading, demo history loading, demo report loading and demo pause loading give as created (the four results of the Given step). Each signs in with `example2026`. Its goals come from slice 4's goal list, and each goal's attempts from FR-014's use case. The step fails unless `demo1@example.com` and `demo4@example.com` to `demo6@example.com` are among them.
- AC-105 "the custom habit is not deleted" and AC-106 "the habit is not unpublished": the use case's outcome is not Deleted / Unpublished, and a read confirms the effect. For AC-105 the custom habit can still be read. For AC-106 the administrator's catalog list still shows the habit as Published (acceptance.md "Results and figures").
- AC-040 "it succeeds": the outcome of the When step's use case (excusal, excusal removal, record change or record deletion) is its success outcome.
- F009Hooks: one `[BeforeFeature]` hook, scoped to F009, resets the HT-AC schema through the public reset entry point before the feature's first scenario (ADR 0003). There is no test-run hook.

## Feedback addressed
- "No feedback." — no change required.
