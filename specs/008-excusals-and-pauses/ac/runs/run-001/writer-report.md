# Writer report — F008 / run-001

## Build

success, 1 attempt (`dotnet build tests/HT.AcceptanceTests/HT.AcceptanceTests.csproj -p:AcRefMode=true -nologo -v q`: 0 warnings, 0 errors). No remaining errors.

## Coverage

Step definitions: `tests/HT.AcceptanceTests/StepDefinitions/F008/` — `GivenSteps.cs` (G), `WhenSteps.cs` (W), `ReadSteps.cs` (R), `ResultThenSteps.cs` (RT), `ReportThenSteps.cs` (PT), `DemoSteps.cs` (DS). Support: `tests/HT.AcceptanceTests/Support/F008/` (`F008Driver`, `F008Context`, `F008Arrange`, `F008Text`, `F008Checks`, `F008Database`, `F008Hooks`).

| AC tag | Scenario name | Step definition file(s) |
|---|---|---|
| AC-001 | Excusing a Pending commitment with a reason | G, W, RT |
| AC-002 | Excusing a Due commitment without a reason | G, W, RT |
| AC-003 | The reason is trimmed, and a reason that is empty after trimming is none | G, W, RT |
| AC-004 | An excused commitment stays Excused after its grace period would have ended | G, R, RT |
| AC-005 | An excusal applies at every supplied time, also one earlier than the excusing call | G, R, RT |
| AC-006 | A final commitment cannot be excused | G, W, RT |
| AC-007 | Excusing just before, at and just after the end of the grace period | G, W, RT |
| AC-008 | An Upcoming commitment cannot be excused, also with a reason that is too long | G, W, RT |
| AC-009 | A commitment of a period-based goal cannot be excused | G, W, RT |
| AC-010 | A date that is not a commitment of the goal cannot be excused | G, W, RT |
| AC-011 | A date in a pause cannot be excused, also when it has not begun | G, W, RT |
| AC-012 | A commitment excused before a pause covered it is refused as paused, not as already excused | G, W, RT |
| AC-013 | A commitment that is already excused cannot be excused again | G, W, RT |
| AC-014 | A commitment that has a record cannot be excused | G, W, RT |
| AC-015 | After its record is deleted, a commitment can be excused | G, W, RT |
| AC-016 | A reason has at most 200 characters | G, W, RT |
| AC-017 | An excusal can be removed while the grace period is open | G, W, RT |
| AC-018 | Removing an excusal just before, at and just after the end of the grace period | G, W, RT |
| AC-019 | Removing an excusal that does not exist | G, W, RT |
| AC-020 | A commitment whose excusal is removed is evaluated as if it had never been excused | G, R, RT |
| AC-021 | An excused commitment cannot be recorded | G, W, RT |
| AC-022 | The record checks for a pause and an excusal come after the date check, the pause first | G, W, RT |
| AC-023 | Pausing a goal from today | G, W, RT |
| AC-024 | A pause cannot start before today | G, W, RT |
| AC-025 | The last day cannot be before the first day | G, W, RT |
| AC-026 | A pause cannot share a date with another pause of the goal, but may touch it | G, W, RT |
| AC-027 | A pause needs a first and a last day | G, W, RT |
| AC-028 | The pause checks come in their order | G, W, RT |
| AC-029 | For a goal that has not started, "today" is the account's today | G, W, RT |
| AC-030 | The first day is compared with the current date on the goal's own timeline | G, W, RT |
| AC-031 | Pausing a commitment-based goal makes its commitments in the range Paused | G, W, RT |
| AC-032 | Resuming a commitment-based goal makes today's commitment Due again | G, W, RT |
| AC-033 | A per-week period is Paused as a whole while one of its dates is paused | G, W, RT |
| AC-034 | A date of a Paused week that is not itself paused cannot be recorded | G, W, RT |
| AC-035 | Resuming a per-week goal in a new week leaves the earlier partly paused week Paused | G, W, RT |
| AC-036 | One paused day exempts its whole week, also when its records reach the target | G, R, RT, PT |
| AC-037 | An excusal under a pause is kept and shows again when the pause is removed | G, W, RT |
| AC-038 | A pause never changes a result that was Pending or final when it was set | G, W, RT |
| AC-039 | A Due entry whose records already exceed a Reduce limit becomes Paused and keeps its records | G, W, RT |
| AC-040 | A date in a pause cannot be recorded | G, W, RT |
| AC-041 | A record kept on a Paused date can be changed or deleted, and the date stays Paused | G, W, RT |
| AC-042 | Resuming during a pause ends it on the day before today | G, W, RT |
| AC-043 | Resuming on a pause's first day removes the pause | G, W, RT |
| AC-044 | A goal without a current pause cannot be resumed | G, W, RT |
| AC-045 | Resuming in the middle of a week leaves that week Paused, while the goal is not paused | G, W, RT |
| AC-046 | A pause later in the current week pauses that week, while the goal is not paused and is counted | G, R, PT, RT |
| AC-047 | Cancelling the upcoming pause that paused the current week makes the week Due again | G, W, RT |
| AC-048 | An upcoming pause can be cancelled | G, W, RT |
| AC-049 | A current or past pause cannot be cancelled | G, W, RT |
| AC-050 | Cancelling just before and on a pause's first day | G, W, RT |
| AC-051 | A pause is found only through its own goal | G, W, RT |
| AC-052 | The pauses of a goal are listed with the latest first day first | G, R, RT |
| AC-053 | Consistency leaves Excused and Paused results out | G, R, PT |
| AC-054 | Excused and Paused results neither extend nor reset a streak | G, R, PT |
| AC-055 | Recent results include Excused and Paused results and count each state | G, R, PT |
| AC-056 | The trend marks Excused and Paused bars, which have no value | G, R, PT |
| AC-057 | Removing an excusal and recording the commitment late gives the figures of an on-time record | G, R, PT |
| AC-058 | A goal whose every ended result is exempt has "N/A" and streaks of 0 | G, R, PT |
| AC-059 | A Paused week of a per-week goal is not counted and does not break the streak | G, R, PT |
| AC-060 | The trend of a per-week goal marks a Paused week | G, R, PT |
| AC-061 | The trend of a per-day goal marks its Paused days | G, R, PT |
| AC-062 | A yes/no week's target and value leave a Paused commitment out | G, R, PT |
| AC-063 | A yes/no week whose only record is on a Paused commitment has no value | G, R, PT |
| AC-064 | A yes/no week whose commitments are all Excused is marked Excused | G, R, PT |
| AC-065 | A yes/no week whose commitments are all Excused or Paused, at least one Paused, is marked Paused | G, R, PT |
| AC-066 | The goals overview says which goal is paused and until when | G, R, RT |
| AC-067 | The dashboard expects nothing of a paused goal and does not count it | G, R, PT |
| AC-068 | An excused commitment leaves the Pending count and the Today list | G, R, PT |
| AC-069 | The Upcoming entry skips a Paused commitment | G, R, PT |
| AC-070 | An Excused commitment whose grace period would have ended this week is not Missed | G, R, PT |
| AC-071 | Recent notes show Excused and Paused as the state | G, R, RT, PT |
| AC-072 | Another registered user cannot use any use case of this slice on someone else's goal | G, W, RT |
| AC-073 | An administrator cannot use any use case of this slice | G, W, RT |
| AC-074 | An anonymous visitor cannot use any use case of this slice | G, W, RT |
| AC-075 | A goal without excusals or pauses keeps slice 6's figures and is counted | G, R, PT, RT |
| AC-076 | Demo pause loading creates `demo6@example.com` and names no private data in its results | G, DS |
| AC-077 | The demo figures of `demo6@example.com` hold on every weekday | G, R, PT, RT |
| AC-078 | The demo excusals, pauses, states and notes of `demo6@example.com` | G, R, RT |
| AC-079 | Demo pause loading leaves an existing `demo6@example.com` unchanged | G, DS, RT |
| AC-080 | Demo pause loading without a valid demo password creates nothing | G, DS |
| AC-081 | A demo goal whose catalog habit is not Published is left out | G, DS, RT |
| AC-082 | The earlier demo accounts get no excusal and no pause | G, R, DS |
| AC-083 | A yes/no week's target leaves an Excused commitment out | G, R, PT |
| AC-084 | A date in a pause that is not a commitment is refused as not scheduled | G, W, RT |
| AC-085 | An Excused Due commitment is not counted or listed as Due | G, R, PT |
| AC-086 | For a goal that has not started, a pause is current or past by the account's today | G, R, RT |
| AC-087 | A goal that has not started can be resumed from its current pause | G, W, RT |
| AC-088 | A pause of several months is allowed and leaves the streak before it unchanged | G, R, PT, RT |

## Unexpressible steps

None.

## Cross-feature changes

None. Nothing in another feature's folders and nothing in `Support/Shared/` was changed. F008 has its own driver, context and helpers (patterned on F007's), so no other feature's code is referenced.

## Missing packages

None.

## Interpretations and assumptions

- All items, time: setup that gives no time (registration, sign-in, administrator creation, custom habits, unpublishing) runs at the time of the item's first step that gives a time. In every F008 item that is the earliest time, as acceptance.md requires: the goal creation (default 2026-11-02T08:00:00Z, start date 2026-11-02) or the demo run. A Given step that records and gives no time runs at the When step's time, before the When action, in the order written. Then steps that read run at the When step's time unless they give their own.
- All items: "the goal" in a When or Given step is the only goal of the account that acts. In a Then step it is the only goal of the item. A Then read with no "read by" runs as the goal's owner. The goals overview and dashboard in Then steps are read as the When step's actor.
- All items, history and what can be recorded now: "gives …" or "contains …" checks that each listed entry exists, matched by its exact first and last date, with the listed state and facts ("An item checks only what it writes"). "no entry for D" checks that no entry covers D. "record V" means exactly one record with that value; "records V, V, V" means exactly those values; "no record" means no records.
- All items, pauses: "the pauses of … are (…) and (…)" or "they are …" checks the exact list of pauses in the order written (days, reason, current/upcoming/past). acceptance.md says a pause read carries the identifier its "paused" result gave, so when a pause read has the same first day as a pause created by a "paused" result in this item, its identifier must equal that one. After a resume the first day stays the same, so this check applies there too.
- AC-006 row 01 "`x` records nothing": calls no use case (acceptance.md, "Recording"). The step only checks that `x` is an account of the item.
- AC-022, AC-034, AC-040, AC-021: "records V for D" in a When step is slice 5's record use case. The result is checked through its message.
- AC-041: "the result is the state Paused, correction no" checks the `Entry.State` and the `IsCorrection` of the change or delete result.
- AC-051: "cancels the pause P through their goal `G`" uses the identifier of the pause with those days (set on a's `Yoga, 3 days a week`) and calls the use case for the actor's own goal `G`.
- AC-055: "the recent results are, oldest first: …" checks that `Recent.Results` holds exactly those 17 results in that order (oldest first), each with first date = last date = the date and the state written. "the number of each state among them is …" checks only the counts listed.
- AC-056, AC-060, AC-061: "the trend is, with the target N: …" checks the exact bar list in order. Each bar has the written dates, the value (none for "no value", "Excused" and "Paused"), the mark (Excused/Paused, or none) and the target N. "the trend contains (…)" finds the bar by its dates.
- AC-046, AC-067 to AC-070, AC-085: "the Today list contains (…)" checks inclusion of each (goal name, dates, state). "no entry for D" means no Today entry covers D. "no entry of `G` for D" is the same, limited to G. "no Due and no Upcoming entry of `G`" means G has no Today entry in either state.
- AC-046, AC-067: "the summary of `G` says it is paused / not paused" checks whether `DashboardGoal.PausedUntil` has a value.
- AC-079, AC-081: "the goals overview of `x` lists exactly …" compares the set of goal names, ignoring order ("no goal" means empty).
- AC-076: "no result contains …" checks both the email and the text of every result line.
- AC-077, AC-078: demo goals are found by name in slice 4's goal list of `demo6@example.com`, which signs in with `example2026`.
- AC-082: the accounts are those that the results of demo loading, demo history loading and demo report loading give with outcome Created. The step checks that the three named accounts are among them. Each account signs in, lists its goals, and reads each goal's pauses and its history from 2026-08-01 to 2026-11-18; both reads must be found. The Then steps require at least one goal read.
- "the demo run at T": after the earlier demo tasks, demo pause loading runs at T with `example2026`. The Given checks that its result names exactly one account, `demo6@example.com` (FR-031).
- Feature vs acceptance criteria: no difference found in the items checked.
