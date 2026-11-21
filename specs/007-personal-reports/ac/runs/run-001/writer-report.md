# Writer report — F007 / run-001

## Build

success, 4 attempts (attempt 2 was a deliberate compile error that confirmed the new F007 files are compiled; attempt 3 reverted it; attempt 4 followed a binding fix). No remaining errors or warnings.

## Coverage

Step definition files (all in `tests/HT.AcceptanceTests/StepDefinitions/F007/`): **A** = `ArrangeSteps.cs`, **R** = `ReadSteps.cs`, **RT** = `ReportThenSteps.cs`, **OD** = `OverviewDashboardThenSteps.cs`, **DS** = `DemoSteps.cs`. Support: `tests/HT.AcceptanceTests/Support/F007/` (`F007Driver`, `F007Context`, `F007Arrange`, `F007Checks`, `F007Text`, `F007Database`, `F007Hooks`).

| AC tag | Scenario name | Step definition file(s) |
|---|---|---|
| AC-001 | Consistency counts Met against Met, Failed and Missed final results | A, R, RT |
| AC-002 | Pending and Due results are not counted, also when they reach the target | A, R, RT |
| AC-003 | A goal with no final result has the consistency "N/A" | A, R, RT |
| AC-004 | The percentage is rounded down to a whole number | A, R, RT |
| AC-005 | 199 Met of 200 final results is "99% (199 of 200)" | A, R, RT |
| AC-006 | Reduce results count like Build results | A, R, RT |
| AC-007 | Each commitment, day or week counts once, however many records it has | A, R, RT |
| AC-008 | Two goals on the same habit have their own figures and nothing combines them | A, R, RT, OD |
| AC-009 | A Failed result resets the current streak | A, R, RT |
| AC-010 | The longest streak keeps the longest run of Met results | A, R, RT |
| AC-011 | A latest Failed or Missed result gives a current streak of 0 | A, R, RT |
| AC-012 | A Pending result neither extends nor resets the streak until it is Met | A, R, RT |
| AC-013 | A Pending result that becomes Missed resets the streak at its grace end | A, R, RT |
| AC-014 | Every streak carries the unit of its goal, singular for 1 | A, R, RT |
| AC-015 | A goal that has not started has streaks of 0, no recent results and no trend | A, R, RT |
| AC-016 | A late record for a Missed day joins the runs on both sides | A, R, RT |
| AC-017 | Changing a Met record to an amount below the target makes it Failed | A, R, RT |
| AC-018 | Deleting the only record of a final commitment makes it Missed | A, R, RT |
| AC-019 | Records entered late give the same report as records entered on time | A, R, RT |
| AC-020 | After a time zone change each result is evaluated with the version of its own date | A, R, RT |
| AC-021 | The recent results are the latest 30 ended results, oldest first | A, R, RT |
| AC-022 | The recent results count each state, and the figures count every result of the attempt | A, R, RT |
| AC-023 | A commitment-based duration goal has one bar per commitment, the last 14 | A, R, RT |
| AC-024 | A per-week goal has one bar per week, the last 12, with the week's progress | A, R, RT |
| AC-025 | A commitment-based yes/no goal has one bar per week with its commitments as the target | A, R, RT |
| AC-026 | A week's bar begins with its first commitment | A, R, RT |
| AC-027 | A recorded 0 is 0, and a bar without a record has no value, also for Reduce | A, R, RT |
| AC-028 | A Reduce yes/no week counts its "Yes" records against 0, a Build duration week counts records that reach the amount | A, R, RT |
| AC-029 | Each goal of the overview has its figures, its last 15 results and what can be recorded | A, R, OD |
| AC-030 | The overview puts goals with a Pending result first, then those with a Due result, each group in name and creation order | A, R, OD |
| AC-031 | The overview gives the progress of each Due entry | A, R, OD |
| AC-032 | The dashboard counts Due, Pending, Missed-this-week results and goals | A, R, OD |
| AC-033 | The Today list has Pending, then Due, then this week's Missed results | A, R, OD |
| AC-034 | "Missed this week" starts on Monday at 00:00 in the account's time zone and includes the previous Friday | A, R, OD |
| AC-035 | The dashboard lists every goal with its consistency and streaks in the overview order | A, R, OD |
| AC-036 | The recent notes are the 5 with the latest dates across all goals | A, R, OD |
| AC-037 | A user with no goals gets counts of 0 and empty lists | A, R, OD |
| AC-038 | Upcoming entries end the Today list for goals with no Due result, up to 6 days ahead | A, R, OD |
| AC-039 | Another user's goal does not exist for the goal report | A, R, RT |
| AC-040 | The dashboard and the overview contain only the acting user's goals | A, R, OD |
| AC-041 | An anonymous visitor and an administrator are refused by every use case of this slice | A, R, RT |
| AC-042 | demo4's Reading goal reads "92% (24 of 26)" with a streak of 3 and a longest of 18 | DS, R, RT |
| AC-043 | demo4's Sugar goal reads "92% (24 of 26)" with a streak of 1 day and a longest of 16 days | DS, R, RT |
| AC-044 | demo4's Yoga goal reads its weeks by the weekday of the run | DS, R, RT |
| AC-045 | demo4's dashboard has Pending and Due results, no Missed one, and its notes | DS, R, OD |
| AC-046 | Demo loading and demo history loading are unchanged, and demo1 to demo3 have no final result | DS |
| AC-047 | Demo report loading creates demo5 with its three goals | DS |
| AC-048 | demo5's reports on a Wednesday run | DS, R, RT |
| AC-049 | demo5's Strength training and Screen time goals on other weekdays | DS, R, RT |
| AC-050 | A second run of demo report loading leaves demo5 unchanged | DS, RT |
| AC-051 | An account that already has the demo5 email, in any case, is left unchanged | DS |
| AC-052 | Without a valid demo password demo report loading creates nothing | DS |
| AC-053 | demo5 has one result Missed this week on every weekday | DS, R, OD |
| AC-054 | demo5's dashboard on Wednesday 2026-11-18 | DS, R, OD |
| AC-055 | A demo goal whose catalog habit is not Published is left out | DS, A |
| AC-056 | A per-week goal started mid-week reports from its first full week | A, R, RT |
| AC-057 | Around a clock change back, a grace period ends at 23:00 local time and a 25-hour day counts once | A, R, RT |
| AC-058 | A duration goal in hours has its trend values and target in minutes | A, R, RT |
| AC-059 | A goal whose catalog habit is Unpublished keeps its reports | A, R, RT |
| AC-060 | A per-week goal's Pending week is in the Today list | DS, R, OD |
| AC-061 | Around a clock change forward, a grace period ends at 01:00 local time and a 23-hour day counts once | A, R, RT |
| AC-062 | A per-week or per-day result counts only from the end of its grace period | A, R, RT |
| AC-063 | The slice 4 goal list is unchanged, and the overview carries its entries | A, R, OD |
| AC-064 | A commitment-based yes/no trend keeps its last 12 weeks | A, R, RT |
| AC-065 | A per-day goal of 2 times has one bar per day with the day's progress against 2 | A, R, RT |
| AC-066 | demo5's amounts and notes on a Wednesday run | DS, R, RT, OD |
| AC-067 | "This week" follows the account's time zone, also for days of an older version | A, R, OD |

## Unexpressible steps

None.

## Cross-feature changes

None.

## Missing packages

None.

## Interpretations and assumptions

- All items: time. A step runs at the time it gives. Setup that gives no time (registration, sign-in, administrator creation, custom habits, unpublishing) runs at the time of the item's first step that gives a time. In every F007 item that step is also the earliest time the item gives, which is what acceptance.md "Time" asks for. With no time at all, setup runs at 2026-11-18T10:00:00Z. A Given step that records or saves a note and gives no time is held and runs at the When step's time, just before the When step's own calls (acceptance.md: such a step "runs at the time of the item's When step"). Then steps run at the When step's time unless they give their own.
- AC-027: "records … on the `Sugar` goal, at 2026-11-05T09:00:00Z" — the time written at the end of the step is that step's time, the same as the prefix form "at T".
- AC-034, AC-057, AC-061: "the registered user `x` in `Z`" — registration gets the time zone text `Z` literally, including `UTC` (acceptance.md: "registration also gave the time zone Z").
- Results and trend bars written with one date ("2026-11-04 Met", "(2026-11-04, 20)") — checked as first date = last date = that date. Per-week periods ("D1 – D2") are checked by both dates.
- "no record" for a recent result (AC-013, AC-018, AC-021) — checked as a progress with no recorded value. Slice 5 FR-014: a commitment has "no value while it has no record". All of these results are commitments. For Today entries, "record" and "no record" are checked through `TodayEntry.HasRecord`, and "note" and "no note" through `HasNote`.
- "the current streak is "S" with the number N and the unit "U"" (AC-009, AC-014) — checks the text, `CurrentStreak` = N, `Unit` = the StreakUnit the word names, and that the shared formatter `ReportText.Unit(unit, N)` gives "U". The report exposes the unit only as an enum, and HP-005 makes this formatter the source of the unit text.
- "with the target T" written once for a trend — checks `TrendView.Target` and every bar's `Target`. A per-bar "target T" checks only that bar.
- "the trend is / has N bars, in this order" — the exact list of bars. "the trend contains the bar (D, V)" — exactly one bar starts on D, and it has value V.
- "the recent results contain D S exactly once" (AC-057, AC-061) — exactly one recent result for date D, and it has state S. This reads "counts once" from the scenario titles.
- "contain no entry for D" (AC-002, AC-021) — no recent result whose dates include D. AC-033's "no entry for 2026-11-12" — no single-day Today entry dated 2026-11-12. The `Yoga` week is checked separately by its goal and its two dates.
- AC-029, AC-031: "its Due entry is D, progress A of B" and "no Due entry" — read from `OverviewGoal.Recordable`, the "what can be recorded now" history FR-011 adds, as its single entry in state Due. "something can be recorded now" is `CanRecord`, and "a Pending result" is `HasPending`.
- AC-030: "the `A` created third" — the goal at creation position 3 of the item; the check confirms it is named `A`.
- AC-038: "every entry gives the identifier of the goal it names, as creating that goal returned it, and Build" — each entry's `GoalId` equals the identifier that creating the goal with that entry's `GoalName` returned, and each entry is Build. The list is checked as non-empty.
- AC-039: "This goal does not exist." — checks the outcome NotFound, the message, and no report. AC-041: "the result is X and no data is returned" — the call's outcome is not Found, its message is X, and its payload (report, overview or dashboard) is null.
- AC-040: "belongs to that goal" — the acting user's only goal, as the overview gives it. "no entry contains "Private note."" is checked on recent notes and on Today entries' notes.
- AC-046: "every goal of its dashboard has …" — checked for every goal of the three demo dashboards. The step also asserts that the three dashboards list at least one goal between them, so the check cannot pass with no goals: FR-031 says demo1 to demo3 have slice 4's demo goals.
- AC-050: "at T the report of "Screen time, at most 120 min a day" has …" — read by `demo5@example.com`, the account the item is about, since the step names no account.
- AC-050: "lists exactly the goals A, B and C" (no "in this order") — compared as a set of names. AC-047 and AC-055 ("in this order") are compared in order.
- AC-055: "`a` unpublished the catalog habit `Walking` with confirmation" — the seed habit's identifier comes from the administrator's catalog list (searched by name, exact name match). The unpublish runs at setup time and must succeed.
- Given "the demo run at T" — reset, then demo loading, demo history loading and demo report loading at T with `example2026`. Each must give Loaded, since it is the item's arrangement. The When form ("When the demo run at T", AC-046 and AC-047) stores the results for the Then steps without asserting them.
- AC-063: "each entry is equal, field by field" — `GoalListItem` value equality. Every property of that type is a scalar. "the same name, Build or Reduce, schedule and start or next information" — compares `Name`, `BuildOrReduce`, `ScheduleText`, `ScheduleKind`, `StartDate`, `NextCommitment` and `FirstWeekStarts`.
- .feature compared with acceptance.md: I bound the .feature as written. I did not compare it item by item with acceptance.md, and I found no difference in the conventions I consulted.
