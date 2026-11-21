# Writer report — F008 / run-003
## Build
success, 1 attempt (`dotnet build tests/HT.AcceptanceTests/HT.AcceptanceTests.csproj -p:AcRefMode=true -nologo -v q`: 0 warnings, 0 errors). No remaining errors.

The step definitions and support code from the earlier runs were checked step by step against the current
`.feature` file, the spec, acceptance.md's conventions and the current API listings. Every step is bound, the
code compiles against the current reference assemblies, and the feedback file has no points. So no code
changed in this run.

## Coverage
Files are in `tests/HT.AcceptanceTests/StepDefinitions/F008/` (G = GivenSteps.cs, W = WhenSteps.cs, R = ReadSteps.cs,
RT = ResultThenSteps.cs, RPT = ReportThenSteps.cs, D = DemoSteps.cs). Shared state, driver, parsing, checks and hooks
are in `tests/HT.AcceptanceTests/Support/F008/`.

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
| AC-036 | One paused day exempts its whole week, also when its records reach the target | G, R, RT, RPT |
| AC-037 | An excusal under a pause is kept and shows again when the pause is removed | G, W, RT |
| AC-038 | A pause never changes a result that was Pending or final when it was set | G, W, RT |
| AC-039 | A Due entry whose records already exceed a Reduce limit becomes Paused and keeps its records | G, W, RT |
| AC-040 | A date in a pause cannot be recorded | G, W, RT |
| AC-041 | A record kept on a Paused date can be changed or deleted, and the date stays Paused | G, W, RT |
| AC-042 | Resuming during a pause ends it on the day before today | G, W, RT |
| AC-043 | Resuming on a pause's first day removes the pause | G, W, RT |
| AC-044 | A goal without a current pause cannot be resumed | G, W, RT |
| AC-045 | Resuming in the middle of a week leaves that week Paused, while the goal is not paused | G, W, RT |
| AC-046 | A pause later in the current week pauses that week, while the goal is not paused and is counted | G, R, RPT, RT |
| AC-047 | Cancelling the upcoming pause that paused the current week makes the week Due again | G, W, RT |
| AC-048 | An upcoming pause can be cancelled | G, W, RT |
| AC-049 | A current or past pause cannot be cancelled | G, W, RT |
| AC-050 | Cancelling just before and on a pause's first day | G, W, RT |
| AC-051 | A pause is found only through its own goal | G, W, RT |
| AC-052 | The pauses of a goal are listed with the latest first day first | G, R, RT |
| AC-053 | Consistency leaves Excused and Paused results out | G, R, RPT |
| AC-054 | Excused and Paused results neither extend nor reset a streak | G, R, RPT |
| AC-055 | Recent results include Excused and Paused results and count each state | G, R, RPT |
| AC-056 | The trend marks Excused and Paused bars, which have no value | G, R, RPT |
| AC-057 | Removing an excusal and recording the commitment late gives the figures of an on-time record | G, R, RPT |
| AC-058 | A goal whose every ended result is exempt has "N/A" and streaks of 0 | G, R, RPT |
| AC-059 | A Paused week of a per-week goal is not counted and does not break the streak | G, R, RPT |
| AC-060 | The trend of a per-week goal marks a Paused week | G, R, RPT |
| AC-061 | The trend of a per-day goal marks its Paused days | G, R, RPT |
| AC-062 | A yes/no week's target and value leave a Paused commitment out | G, R, RPT |
| AC-063 | A yes/no week whose only record is on a Paused commitment has no value | G, R, RPT |
| AC-064 | A yes/no week whose commitments are all Excused is marked Excused | G, R, RPT |
| AC-065 | A yes/no week whose commitments are all Excused or Paused, at least one Paused, is marked Paused | G, R, RPT |
| AC-066 | The goals overview says which goal is paused and until when | G, R, RT |
| AC-067 | The dashboard expects nothing of a paused goal and does not count it | G, R, RPT |
| AC-068 | An excused commitment leaves the Pending count and the Today list | G, R, RPT |
| AC-069 | The Upcoming entry skips a Paused commitment | G, R, RPT |
| AC-070 | An Excused commitment whose grace period would have ended this week is not Missed | G, R, RPT |
| AC-071 | Recent notes show Excused and Paused as the state | G, R, RT, RPT |
| AC-072 | Another registered user cannot use any use case of this slice on someone else's goal | G, W, RT |
| AC-073 | An administrator cannot use any use case of this slice | G, W, RT |
| AC-074 | An anonymous visitor cannot use any use case of this slice | G, W, RT |
| AC-075 | A goal without excusals or pauses keeps slice 6's figures and is counted | G, R, RPT, RT |
| AC-076 | Demo pause loading creates `demo6@example.com` and names no private data in its results | G, D |
| AC-077 | The demo figures of `demo6@example.com` hold on every weekday | G, R, RPT, RT |
| AC-078 | The demo excusals, pauses, states and notes of `demo6@example.com` | G, R, RT |
| AC-079 | Demo pause loading leaves an existing `demo6@example.com` unchanged | G, D, RT |
| AC-080 | Demo pause loading without a valid demo password creates nothing | G, D |
| AC-081 | A demo goal whose catalog habit is not Published is left out | G, D, RT |
| AC-082 | The earlier demo accounts get no excusal and no pause | G, R, D |
| AC-083 | A yes/no week's target leaves an Excused commitment out | G, R, RPT |
| AC-084 | A date in a pause that is not a commitment is refused as not scheduled | G, W, RT |
| AC-085 | An Excused Due commitment is not counted or listed as Due | G, R, RPT |
| AC-086 | For a goal that has not started, a pause is current or past by the account's today | G, R, RT |
| AC-087 | A goal that has not started can be resumed from its current pause | G, W, RT |
| AC-088 | A pause of several months is allowed and leaves the streak before it unchanged | G, R, RPT, RT |

## Unexpressible steps
None.

## Cross-feature changes
None.

## Missing packages
None.

## Interpretations and assumptions
- All items: time. A step that gives a time runs at that time. Setup that gives no time (registration, sign-in, administrator creation, custom habits, unpublishing) runs at the item's earliest time. That is the goal creation time (default 2026-11-02T08:00:00Z), the time the item gives for the goal creation, or the demo run time. Given steps that record and give no time ("`x` records V for …") are deferred and run at the When step's time, in the order written (acceptance.md, "Time"). Then steps run at the When step's time unless they give "at T". `FakeTimeProvider` only moves forward, so a step at an earlier time than the one before it gets a fresh provider started at its time.
- Given steps that excuse, pause, remove an excusal, record, delete a record or save a note: each asserts that it succeeds, because acceptance.md says "each of these succeeds unless the item gives its result". The "…, with the result …" variants (AC-020, AC-088) also assert the result text and the state or days given.
- AC-006: "`x` records nothing" calls no use case (acceptance.md, "Recording"). The step only checks that `x` is an account of the item.
- AC-003 and AC-008/014/016/028: a reason in quotes is passed exactly as written, spaces included, so the trimming is left to the system. "`x` repeated N times" becomes that character N times.
- AC-027/028: "with no first day" / "with no last day" pass `null` for that day.
- AC-051: "the pause D1 – D2" is the identifier that the "paused" result gave for those days, whichever account set the pause. It is passed through the actor's own goal `Drinking water, 8 glasses a day`. The step asserts that this goal is not the pause's goal.
- AC-072 to AC-074: "<action> on the goal of `a`" calls the use case for `a`'s only goal with the other user, the administrator or the anonymous visitor as the acting user. The follow-up Then steps read as `a`, as written (", read by `a`,").
- The pauses checks ("the pauses … are …", "they are …"): the list must match exactly and in order (first day, last day, reason, timing). For each pause read, if exactly one "paused" result of the item gave a pause with that first day, the identifier must equal the one that result gave (acceptance.md: "every pause read also carries its identifier, equal to the one the 'paused' result gave"). After a resume shortens a pause, the pause is matched by its first day.
- History checks: each entry written must exist with exactly that date or period, and only the facts the item writes are checked ("An item checks only what it writes"). "no entry for D" fails if any entry overlaps D. "what can be recorded now contains …" uses the same rules on the recordable read.
- Trend: "with the target N" is checked on every bar, as acceptance.md says the target is written once when every bar has it. "the trend is …" compares the full bar list in order. "the trend contains …" checks only the bars named.
- Today list: "contains (…)" checks that each named entry is included (goal name, dates, state). "no entry for D", "no entry of `G` for D" and "no Due and no Upcoming entry of `G`" assert absence. Other entries in the list are allowed, as the spec checks lists by inclusion.
- Dashboard summary "says it is paused / not paused": read as the summary's `PausedUntil` having a value or not.
- "the goals overview of `x` lists exactly …" (AC-079, AC-081): the set of goal names must equal the named ones, in any order. "no goal" is the empty set.
- AC-076 "no result contains …": checked against both the text and the email of every result line.
- AC-077 "the demo run at T": reset, then demo loading, demo history loading and demo report loading, then demo pause loading, all at T with `example2026`. The Given asserts that demo pause loading reported exactly the account `demo6@example.com`.
- AC-082: every account that any of the three earlier demo tasks reports as created signs in with the demo password (`example2026`). The step asserts that `demo1`, `demo4` and `demo5` are among them. Then it reads the pauses and the history of the range for each of their goals (goal list from slice 4).
- Database: like F001 to F007, a `[BeforeFeature]` hook scoped to F008 resets the AC schema through `HtDatabase.ResetAsync`. The demo scenarios reset it again in their own Given step.
- No differences between the `.feature` scenarios and their acceptance-criteria items were found in the parts checked.

## Feedback addressed
- "No feedback." — nothing to change. The bindings were re-checked against the current feature file and API, and the build passes.
