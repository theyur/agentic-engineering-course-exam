# Writer report — F005 / run-003

## Build
success, 2 attempts (the second only after changing doc comments), 0 errors, 0 warnings.

## Coverage
All 69 scenarios in `Features/F005/recording-and-evaluation.feature` are bound. The step definition files are in
`tests/HT.AcceptanceTests/StepDefinitions/F005/`. G = GivenSteps.cs, W = WhenSteps.cs, R = ResultSteps.cs,
H = HistorySteps.cs, D = DemoSteps.cs.

| AC tag | Scenario name | Step definition file(s) |
|---|---|---|
| AC-001 | A partial amount recorded for today is kept and the commitment is Due | G, W, R, H |
| AC-002 | Recording again for a commitment replaces its one record | G, W, R, H |
| AC-003 | A per-week period holds one record per recording, listed in date order | G, W, R, H |
| AC-004 | Records on the same date of a per-day period keep the order in which they were stored | G, W, R, H |
| AC-005 | A date that belongs to no commitment or period cannot be recorded | G, W, R |
| AC-006 | A day that has not begun on the goal's timeline cannot be recorded | G, W, R |
| AC-007 | A value that does not fit the goal's measurement is refused with its message | G, W, R |
| AC-008 | The lowest and highest amounts are accepted and kept | G, W, R, H |
| AC-009 | A value that does not belong to the goal's measurement is not stored | G, W, R, H |
| AC-010 | Recording checks the acting user, the goal, the date, the value and the note in that order | G, W, R |
| AC-011 | A note given with a record is trimmed | G, W, R, H |
| AC-012 | A commitment is Upcoming before its start instant and Due from it | G, W, H |
| AC-013 | An unrecorded commitment is Pending for 48 hours after its end and then Missed by itself | G, W, H |
| AC-014 | A final Build duration commitment is Met at or above its target and Failed below it | G, W, H |
| AC-015 | A final Reduce count commitment is Met at or below its limit and Failed above it | G, W, H |
| AC-016 | A final yes/no commitment is Met on Yes for Build and on No for Reduce | G, W, H |
| AC-017 | A final per-week Build yes/no period is Met with at least N Yes records | G, W, H |
| AC-018 | A final per-week Build count period counts only the records that reach the amount | G, W, H |
| AC-019 | A final per-day Reduce count period is Met when its total is within the limit | G, W, H |
| AC-020 | A final per-day yes/no period counts its Yes records | G, W, H |
| AC-021 | An ended commitment or period stays Pending during its grace period, even when its records reach the target | G, W, H |
| AC-022 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit | G, W, H |
| AC-023 | Around a clock change the grace period lasts exactly 48 elapsed hours | G, W, H |
| AC-024 | A record added to a Missed commitment is a correction that makes it Met | G, W, R |
| AC-025 | Lowering the record of a Met commitment below its target is a correction that makes it Failed | G, W, R |
| AC-026 | Deleting the only record of a final commitment is a correction that makes it Missed | G, W, R |
| AC-027 | A change is a correction only from the instant its grace period ends | G, W, R |
| AC-028 | A missing record added to a Failed week is a correction that makes it Met | G, W, R |
| AC-029 | Results corrected after the grace period equal the same records entered on time | G, W, R, H |
| AC-030 | After a time zone change, each commitment keeps the instants and grace period of its own settings version | G, W, H |
| AC-031 | A commitment in the old time zone becomes final, and is corrected, at the end of its own grace period | G, W, R, H |
| AC-032 | A record identifier that is not a record of the goal named in the call does not exist | G, W, R |
| AC-033 | An evaluation period takes at most 50 records, but its records can still be changed or deleted | G, W, R |
| AC-034 | A record belongs to the week of its date, not to the week of the instant it is saved | G, W, R, H |
| AC-035 | A note can be added to a Missed commitment and leaves it Missed | G, W, R, H |
| AC-036 | A note saved on its own replaces the note, and an empty one removes it | G, W, R, H |
| AC-037 | A note given with a record replaces the note only when it has text | G, W, R, H |
| AC-038 | A note has at most 500 characters | G, W, R |
| AC-039 | Any begun date of a week names that week's one note | G, W, R, H |
| AC-040 | The recent notes are the 5 with the latest dates, newest first, each with its state | G, W, H |
| AC-041 | A per-week note gives its week's first and last date, and a goal with fewer notes gives only those | G, W, H |
| AC-042 | The history gives every commitment of the range in date order, each with its state at the supplied time | G, W, H |
| AC-043 | The history's range has 1 to 366 days and is checked after the goal | G, W, R |
| AC-044 | Progress is given at any state, as the goal's kind of target counts it | G, W, H |
| AC-045 | What can be recorded now is every Due and every Pending commitment or period, newest first | G, W, H |
| AC-046 | After a move east, the old and the new day are both Due while they overlap | G, W, H |
| AC-047 | Another registered user gets "This goal does not exist." from every use case of this slice | G, W, R |
| AC-048 | Another account's record identifier does not exist on one's own goal | G, W, R |
| AC-049 | An administrator gets "Goals are for registered users." from every use case of this slice | G, W, R |
| AC-050 | The anonymous visitor gets "You are not signed in." from every use case of this slice | G, W, R |
| AC-051 | Demo history loading creates the demo account with history and its three demo goals | G, D |
| AC-052 | The demo goal "Reading, 20 min daily" shows Met, Failed with a partial amount, Missed with a note, Pending and Due | G, D, H |
| AC-053 | The demo goal "Sugar, at most 1 sweet a day" shows Met, Failed with a note, Missed, Pending and Due | G, D, H |
| AC-054 | The demo goal "Yoga, 3 times a week" shows a Met, a Failed and a Missed week and a Due week when R is a Wednesday | G, D, H |
| AC-055 | When R is a Monday, the demo Yoga goal's last full week is Pending and it has a week before week 3 | G, D, H |
| AC-056 | The demo goals' recent notes give the demo notes with their states | G, D, H |
| AC-057 | Demo history loading run again leaves the existing demo account with history unchanged | G, D, H |
| AC-058 | An account that already has the demo email gets no demo goals and keeps its password | G, D |
| AC-059 | Demo history loading's results name the demo email and no goal, habit, note, unit or amount | G, D |
| AC-060 | A demo goal is left out when its catalog habit is not Published, and the results stay the same | G, D |
| AC-061 | Demo history loading without a demo password creates no account | G, D |
| AC-062 | Demo loading still names only the three slice-4 demo accounts, and their goals get no records | G, D |
| AC-063 | The evaluation calendar still gives no state | G, W, H |
| AC-064 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement | G, W, R, H |
| AC-065 | A goal whose catalog habit is Unpublished is still recorded and given its states | G, W, R, H |
| AC-066 | The other kinds of target, just inside, at and just past their target | G, W, H |
| AC-067 | After a move east, the old and the new day can both be recorded while they overlap | G, W, R, H |
| AC-068 | The demo Yoga goal's weeks when R is a Sunday or a Tuesday | G, D, H |
| AC-069 | Demo history loading takes a demo password only with at least 8 characters and a digit | G, D |

(The file letters show which files each scenario's steps bind to. This run only changed binding patterns, so the
mapping is the same as in run-002. It was not checked step by step again.)

## Unexpressible steps
None.

## Cross-feature changes
None.

## Missing packages
None.

## Interpretations and assumptions
- All ACs with an optional "at `T`" prefix: an empty prefix means the step gives no time. As before,
  `F005Arrange.BeginStepAsync(null)` / `BeginWhenAsync(null)` then handle the step. `F005Text.AtTime` reads the time
  out of the captured "at `T` " prefix.
- AC-006 / AC-023 / AC-030 / AC-046 / AC-067: "the registered user `u` in UTC, …" means the user has no time zone
  at registration (UTC). "… in `Zone`, …" registers that zone. If no clause is given, there is no time zone (UTC).
  `F005Text.ZoneOf` does this; it is unchanged from the earlier run.
- AC-051 / AC-060 / AC-069: "its results are, in this order, …, each for `e`" compares the full result line list
  exactly. So the ", and no other result" ending and the empty ending (AC-060) both check that no other line exists.
  The ", and sign-in with `e` and `p` succeeds" ending (AC-069 example 04) also signs in and asserts the sign-in
  succeeded.
- AC-061 / AC-069: "its result is "M", and no other result" checks the message, that there are no account results,
  and that the result has exactly that one line. The ", and demo history loading run afterwards …" ending (AC-069)
  also runs demo history loading again with the given password and asserts "created" for the named email.
- AC-052..AC-057, AC-068: the parenthesis after "demo loading and then demo history loading ran (…)" only explains
  the dates. It is captured but not used; the Then steps check the dates.

## Feedback addressed
- 1. `GivenSteps` `GivenTheRegisteredUserWhoCreatedTheCustomHabit`: the optional time-zone clause is now the group
  `((?: in `…`| in …)?)`, which always takes part in the match. It binds the forms with no zone, "in UTC" and
  "in `America/New_York`".
- 2. `GivenSteps` `GivenTheUserCreatedAGoal`: the At prefix is now `((?:at `[^`]+` )?)`. The optional second-goal
  clause is now one group that always takes part (6 parameters instead of 8). Code parses that group with an anchored
  regex and throws if it cannot read a non-empty clause. It binds single goals with and without "at `T`", and the
  AC-041 two-goal form. (The two-goal step the feedback calls AC-029 is "created a goal …, and then a second goal …";
  in the `.feature` it is AC-041. AC-029 uses "created two goals", which `GivenTheUserCreatedTwoGoals` binds.)
- 3. `GivenSteps` `GivenTheUserCreatedTwoGoals`, `GivenTheUserRecorded`, `GivenTheUserSavedNotes`: these use the new
  At group, read through `F005Text.AtTime`.
- 4. `WhenSteps` `WhenTheActorWrites`, `WhenTheActorReads`: these use the new At group, read through
  `F005Text.AtTime`.
- 5. `DemoSteps` `GivenDemoLoadingRan`: uses the new At group.
- 6. `DemoSteps` `GivenDemoLoadingAndDemoHistoryLoadingRan`: the parenthesis is now `((?: \(.+\))?)` and the At
  prefix is the new At group. It binds AC-056/AC-057 with no parenthesis, and the forms with a time and a parenthesis.
- 7. `DemoSteps` `WhenDemoHistoryLoadingRuns`, `WhenDemoLoadingAndDemoHistoryLoadingRun`: these use the new At group.
- 8. `DemoSteps` `ThenItsResultsAreInThisOrder`: the ending is now
  `((?:, and no other result|, and sign-in with `…` and `…` succeeds)?)`. An empty ending (AC-060) binds. Code throws
  if it cannot read a non-empty ending, instead of skipping it silently.
- 9. `DemoSteps` `ThenItsResultIs`: the ending is now `((?:, and demo history loading run afterwards .+)?)`. AC-061,
  which has no ending, binds. The afterwards clause is parsed with an anchored regex and throws if it cannot be read.
- I checked the other F005 bindings (`ResultSteps.cs`, `HistorySteps.cs` and the rest of `GivenSteps`, `WhenSteps`
  and `DemoSteps`). None has another optional capture group: the remaining optional parts are non-capturing, or are
  `(.*)` groups that always take part.
