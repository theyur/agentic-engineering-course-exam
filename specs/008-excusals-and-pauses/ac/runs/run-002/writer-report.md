# Writer report — F008 / run-002

## Build
success, 1 attempt (`dotnet build tests/HT.AcceptanceTests/HT.AcceptanceTests.csproj -p:AcRefMode=true -nologo -v q`), 0 warnings, 0 errors.

## Coverage
All 88 scenarios (AC-001 to AC-088) of `Features/F008/excusals-and-pauses.feature` are bound. File abbreviations, all in `tests/HT.AcceptanceTests/StepDefinitions/F008/`: G = GivenSteps.cs, W = WhenSteps.cs, R = ReadSteps.cs, RT = ResultThenSteps.cs, RP = ReportThenSteps.cs, DM = DemoSteps.cs. Support: `tests/HT.AcceptanceTests/Support/F008/`.

| AC tag | Scenario name | Step definition file(s) |
|---|---|---|
| AC-001 | Excusing a Pending commitment with a reason | G, W, RT |
| AC-002 | Excusing a Due commitment without a reason | G, W, RT |
| AC-003 | The reason is trimmed, and a reason that is empty after trimming is none | G, W, RT |
| AC-004 | An excused commitment stays Excused after its grace period would have ended | G, W, RT |
| AC-005 | An excusal applies at every supplied time, also one earlier than the excusing call | G, W, RT |
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
| AC-020 | A commitment whose excusal is removed is evaluated as if it had never been excused | G, W, RT |
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
| AC-036 | One paused day exempts its whole week, also when its records reach the target | G, W, RT |
| AC-037 | An excusal under a pause is kept and shows again when the pause is removed | G, W, RT |
| AC-038 | A pause never changes a result that was Pending or final when it was set | G, W, RT |
| AC-039 | A Due entry whose records already exceed a Reduce limit becomes Paused and keeps its records | G, W, RT |
| AC-040 | A date in a pause cannot be recorded | G, W, RT |
| AC-041 | A record kept on a Paused date can be changed or deleted, and the date stays Paused | G, W, RT |
| AC-042 | Resuming during a pause ends it on the day before today | G, W, RT |
| AC-043 | Resuming on a pause's first day removes the pause | G, W, RT |
| AC-044 | A goal without a current pause cannot be resumed | G, W, RT |
| AC-045 | Resuming in the middle of a week leaves that week Paused, while the goal is not paused | G, W, RT |
| AC-046 | A pause later in the current week pauses that week, while the goal is not paused and is counted | G, W, R, RT, RP |
| AC-047 | Cancelling the upcoming pause that paused the current week makes the week Due again | G, W, RT |
| AC-048 | An upcoming pause can be cancelled | G, W, RT |
| AC-049 | A current or past pause cannot be cancelled | G, W, RT |
| AC-050 | Cancelling just before and on a pause's first day | G, W, RT |
| AC-051 | A pause is found only through its own goal | G, W, RT |
| AC-052 | The pauses of a goal are listed with the latest first day first | G, R, RT |
| AC-053 | Consistency leaves Excused and Paused results out | G, R, RP |
| AC-054 | Excused and Paused results neither extend nor reset a streak | G, R, RP |
| AC-055 | Recent results include Excused and Paused results and count each state | G, R, RP |
| AC-056 | The trend marks Excused and Paused bars, which have no value | G, R, RP |
| AC-057 | Removing an excusal and recording the commitment late gives the figures of an on-time record | G, W, R, RT, RP |
| AC-058 | A goal whose every ended result is exempt has "N/A" and streaks of 0 | G, R, RP |
| AC-059 | A Paused week of a per-week goal is not counted and does not break the streak | G, R, RP |
| AC-060 | The trend of a per-week goal marks a Paused week | G, R, RP |
| AC-061 | The trend of a per-day goal marks its Paused days | G, R, RP |
| AC-062 | A yes/no week's target and value leave a Paused commitment out | G, R, RP |
| AC-063 | A yes/no week whose only record is on a Paused commitment has no value | G, R, RP |
| AC-064 | A yes/no week whose commitments are all Excused is marked Excused | G, R, RP |
| AC-065 | A yes/no week whose commitments are all Excused or Paused, at least one Paused, is marked Paused | G, R, RP |
| AC-066 | The goals overview says which goal is paused and until when | G, R, RT |
| AC-067 | The dashboard expects nothing of a paused goal and does not count it | G, R, RP |
| AC-068 | An excused commitment leaves the Pending count and the Today list | G, R, RP |
| AC-069 | The Upcoming entry skips a Paused commitment | G, R, RP |
| AC-070 | An Excused commitment whose grace period would have ended this week is not Missed | G, R, RP |
| AC-071 | Recent notes show Excused and Paused as the state | G, R, RT, RP |
| AC-072 | Another registered user cannot use any use case of this slice on someone else's goal | G, W, RT |
| AC-073 | An administrator cannot use any use case of this slice | G, W, RT |
| AC-074 | An anonymous visitor cannot use any use case of this slice | G, W, RT |
| AC-075 | A goal without excusals or pauses keeps slice 6's figures and is counted | G, R, RP, RT |
| AC-076 | Demo pause loading creates `demo6@example.com` and names no private data in its results | G, DM |
| AC-077 | The demo figures of `demo6@example.com` hold on every weekday | G, R, RP, RT |
| AC-078 | The demo excusals, pauses, states and notes of `demo6@example.com` | G, R, RT |
| AC-079 | Demo pause loading leaves an existing `demo6@example.com` unchanged | G, DM, RT |
| AC-080 | Demo pause loading without a valid demo password creates nothing | G, DM |
| AC-081 | A demo goal whose catalog habit is not Published is left out | G, DM, RT |
| AC-082 | The earlier demo accounts get no excusal and no pause | G, R, DM |
| AC-083 | A yes/no week's target leaves an Excused commitment out | G, R, RP |
| AC-084 | A date in a pause that is not a commitment is refused as not scheduled | G, W, RT |
| AC-085 | An Excused Due commitment is not counted or listed as Due | G, R, RP |
| AC-086 | For a goal that has not started, a pause is current or past by the account's today | G, R, RT |
| AC-087 | A goal that has not started can be resumed from its current pause | G, W, RT |
| AC-088 | A pause of several months is allowed and leaves the streak before it unchanged | G, R, RP |

## Unexpressible steps
None.

## Cross-feature changes
None.

## Missing packages
None.

## Interpretations and assumptions
- All: "the registered user `x`[ in `Z`], who created …" — the user is registered and signed in with the time zone `Z` only when " in `Z`" is written (no zone otherwise), then the goal(s) are created at the written time with the written start date, or at the default 2026-11-02T08:00:00Z with the start date 2026-11-02.
- All: "at T `x` paused … with the result "R" with the first day D and the last day D" and "at T `x` removed the excusal of D, with the result "R" and the state S" — the variant with the result clause is now its own binding. It runs the same setup, which must succeed as in the plain variant, and then also asserts the written result text and days or state. The plain variant asserts only that the setup succeeded. This replaces the earlier `if (...) Assert` on the optional clause.
- All: history and pauses Then steps — with no "at T" the read runs at the When step's time; with no ", read by `x`," the goal's owner reads; with no " of `x`" the goal is found by name among this item's goals, or among the When actor's goals.

## Feedback addressed
- 1. `GivenTheRegisteredUserWhoCreated` — the regex is now `^the registered user `([^`]+)`((?: in `[^`]+`)?), who created (.+)$`. The zone group always takes part, so it is empty when no zone is written. `BacktickedOrNull` reads the zone from it: null when empty, the backticked name otherwise. The goals tail is still parsed by the private `Created` regex.
- 2. `GivenTheUserExcused` — the optional " of `G`" is now `((?: of `[^`]+`)?)` and is parsed by `BacktickedOrNull`. With no goal name the user's only goal is used.
- 3. `GivenTheUserPaused` — split into two bindings, `GivenTheUserPaused` (no result clause) and `GivenTheUserPausedWithTheResult` (result text plus first and last day). Neither has an optional capture group. Both share `PauseSucceedsAsync`.
- 4. `GivenTheUserRemovedTheExcusal` — split into `GivenTheUserRemovedTheExcusal` and `GivenTheUserRemovedTheExcusalWithTheResult` (result text plus state). Both share `RemoveExcusalSucceedsAsync`.
- 5. `GivenTheUserRecordsForEachDate` — the optional clause is now `((?: except <DateList>)?)`. It is always captured, empty when not written. The skipped dates are every date found in that text, so with no "except" every date of the range is recorded.
- 6. `ThenTheHistoryGives` — the regex is now `^((?:at T )?)the history of … from D to D((?:, read by `[^`]+`,)?) gives (.+)$`. `TimeOrNull` reads the time and `BacktickedOrNull` the reader. If no time is written the read runs at the When step's time. If no reader is written the owner reads.
- 7. `ThenThePausesAre` — the regex is now `^((?:at T )?)the pauses of (the goal|`G`)((?: of `[^`]+`)?)((?:, read by `[^`]+`,)?) are(?:, in this order,)? (.+)$`. All three optional parts are always captured and parsed the same way. The pauses are still checked as an exact ordered list.
- Re-check of the other F008 bindings: no capture group is left inside an optional group in `StepDefinitions/F008/`. The `(?:, in this order,)?` parts and the `(?: and \"[^\"]+\")?` inside the always-captured catalog-habits group contain no capture group of their own.
