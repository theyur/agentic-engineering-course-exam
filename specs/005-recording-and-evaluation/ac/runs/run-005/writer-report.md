# Writer report — F005 / run-005

## Build
success, 1 attempt (`dotnet build tests/HT.AcceptanceTests/HT.AcceptanceTests.csproj -p:AcRefMode=true -nologo -v q`: 0 warnings, 0 errors). No remaining errors.

## Coverage
All step definition files are in `tests/HT.AcceptanceTests/StepDefinitions/F005/`. Their shared support is in `tests/HT.AcceptanceTests/Support/F005/` (F005Arrange, F005Actions, F005Checks, F005Entries, F005Driver, F005Context, F005Text, F005Database, F005Hooks). The files are listed by step family: Given steps, When steps, Then steps on results, Then steps on history/notes/recordable, and demo steps.

| AC tag | Scenario name | Step definition file(s) |
|---|---|---|
| AC-001 | A partial amount recorded for today is kept and the commitment is Due | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |
| AC-002 | Recording again for a commitment replaces its one record | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |
| AC-003 | A per-week period holds one record per recording, listed in date order | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |
| AC-004 | Records on the same date of a per-day period keep the order in which they were stored | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |
| AC-005 | A date that belongs to no commitment or period cannot be recorded | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs |
| AC-006 | A day that has not begun on the goal's timeline cannot be recorded | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs |
| AC-007 | A value that does not fit the goal's measurement is refused with its message | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs |
| AC-008 | The lowest and highest amounts are accepted and kept | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |
| AC-009 | A value that does not belong to the goal's measurement is not stored | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |
| AC-010 | Recording checks the acting user, the goal, the date, the value and the note in that order | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs |
| AC-011 | A note given with a record is trimmed | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |
| AC-012 | A commitment is Upcoming before its start instant and Due from it | GivenSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-013 | An unrecorded commitment is Pending for 48 hours after its end and then Missed by itself | GivenSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-014 | A final Build duration commitment is Met at or above its target and Failed below it | GivenSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-015 | A final Reduce count commitment is Met at or below its limit and Failed above it | GivenSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-016 | A final yes/no commitment is Met on Yes for Build and on No for Reduce | GivenSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-017 | A final per-week Build yes/no period is Met with at least N Yes records | GivenSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-018 | A final per-week Build count period counts only the records that reach the amount | GivenSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-019 | A final per-day Reduce count period is Met when its total is within the limit | GivenSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-020 | A final per-day yes/no period counts its Yes records | GivenSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-021 | An ended commitment or period stays Pending during its grace period, even when its records reach the target | GivenSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-022 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit | GivenSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-023 | Around a clock change the grace period lasts exactly 48 elapsed hours | GivenSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-024 | A record added to a Missed commitment is a correction that makes it Met | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs |
| AC-025 | Lowering the record of a Met commitment below its target is a correction that makes it Failed | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs |
| AC-026 | Deleting the only record of a final commitment is a correction that makes it Missed | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs |
| AC-027 | A change is a correction only from the instant its grace period ends | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs |
| AC-028 | A missing record added to a Failed week is a correction that makes it Met | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs |
| AC-029 | Results corrected after the grace period equal the same records entered on time | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |
| AC-030 | After a time zone change, each commitment keeps the instants and grace period of its own settings version | GivenSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-031 | A commitment in the old time zone becomes final, and is corrected, at the end of its own grace period | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |
| AC-032 | A record identifier that is not a record of the goal named in the call does not exist | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |
| AC-033 | An evaluation period takes at most 50 records, but its records can still be changed or deleted | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs |
| AC-034 | A record belongs to the week of its date, not to the week of the instant it is saved | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |
| AC-035 | A note can be added to a Missed commitment and leaves it Missed | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |
| AC-036 | A note saved on its own replaces the note, and an empty one removes it | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |
| AC-037 | A note given with a record is the note; no note leaves it unchanged | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |
| AC-038 | A note has at most 500 characters | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs |
| AC-039 | Any begun date of a week names that week's one note | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |
| AC-040 | The recent notes are the 5 with the latest dates, newest first, each with its state | GivenSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-041 | A per-week note gives its week's first and last date, and a goal with fewer notes gives only those | GivenSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-042 | The history gives every commitment of the range in date order, each with its state at the supplied time | GivenSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-043 | The history's range has 1 to 366 days and is checked after the goal | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |
| AC-044 | Progress is given at any state, as the goal's kind of target counts it | GivenSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-045 | What can be recorded now is every Due and every Pending commitment or period, newest first | GivenSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-046 | After a move east, the old and the new day are both Due while they overlap | GivenSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-047 | Another registered user gets "This goal does not exist." from every use case of this slice | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs |
| AC-048 | Another account's record identifier does not exist on one's own goal | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs |
| AC-049 | An administrator gets "Goals are for registered users." from every use case of this slice | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs |
| AC-050 | The anonymous visitor gets "You are not signed in." from every use case of this slice | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs |
| AC-051 | Demo history loading creates the demo account with history and its three demo goals | DemoSteps.cs |
| AC-052 | The demo goal "Reading, 20 min daily" shows Met, Failed with a partial amount, Missed with a note, Pending and Due | DemoSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-053 | The demo goal "Sugar, at most 1 sweet a day" shows Met, Failed with a note, Missed, Pending and Due | DemoSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-054 | The demo goal "Yoga, 3 times a week" shows a Met, a Failed and a Missed week and a Due week when R is a Wednesday | DemoSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-055 | When R is a Monday, the demo Yoga goal's last full week is Pending and it has a week before week 3 | DemoSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-056 | The demo goals' recent notes give the demo notes with their states | DemoSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-057 | Demo history loading run again leaves the existing demo account with history unchanged | DemoSteps.cs, HistorySteps.cs |
| AC-058 | An account that already has the demo email gets no demo goals and keeps its password | DemoSteps.cs, GivenSteps.cs |
| AC-059 | Demo history loading's results name the demo email and no goal, habit, note, unit or amount | DemoSteps.cs |
| AC-060 | A demo goal is left out when its catalog habit is not Published, and the results stay the same | DemoSteps.cs, GivenSteps.cs |
| AC-061 | Demo history loading without a demo password creates no account | DemoSteps.cs |
| AC-062 | Demo loading still names only the three slice-4 demo accounts, and their goals get no records | DemoSteps.cs, HistorySteps.cs |
| AC-063 | The evaluation calendar still gives no state | GivenSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-064 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |
| AC-065 | A goal whose catalog habit is Unpublished is still recorded and given its states | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |
| AC-066 | The other kinds of target, just inside, at and just past their target | GivenSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-067 | After a move east, the old and the new day can both be recorded while they overlap | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |
| AC-068 | The demo Yoga goal's weeks when R is a Sunday or a Tuesday | DemoSteps.cs, WhenSteps.cs, HistorySteps.cs |
| AC-069 | Demo history loading takes a demo password only with at least 8 characters and a digit | DemoSteps.cs |
| AC-070 | Deleting the only record of a commitment keeps its note | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |
| AC-071 | The first record of a Missed commitment that has a note: the note given is the note, no note keeps it | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |
| AC-072 | Changing the record of a commitment without a note, with a note, adds the note | GivenSteps.cs, WhenSteps.cs, ResultSteps.cs, HistorySteps.cs |

## Unexpressible steps
None.

## Cross-feature changes
None.

## Missing packages
None.

## Interpretations and assumptions
- AC-010, AC-043, AC-047: `<n>` inside an Examples cell (the actor cells of AC-010 and AC-043, the goal cells of AC-047). Following acceptance.md, Conventions, "Examples", it means the row's value in the column `n`. It is replaced with `ScenarioContext.ScenarioInfo.Arguments["n"]` before any account or goal lookup. If a text holds `<n>` and the scenario has no `n` column, the step fails with an explicit error; it does not silently use the literal text.

## Feedback addressed
- F005/AC-010 rows 02-07, F005/AC-043 rows 01-05, F005/AC-047 rows 01-07: `<n>` inside an Examples cell was not resolved. I added `F005Arrange.WithRow(string)`, which replaces `<n>` with the current row's `n`. `F005Arrange` now takes Reqnroll's `ScenarioContext` by constructor injection. It is applied centrally in `ActorAsync` (actor token) and `GoalPhraseAsync` (goal phrase), the two entry points reached from `F005Actions.RunAsync`, `ReadAsync` and `CallAsync`. It is also applied in `AccountAsync` and `GoalOf`, so any other path that gets an email from a cell resolves the same way; applying it twice changes nothing. The actor's email that `ActorAsync` returns is the resolved one. So the acting user, the goal lookup ("the goal" and "the first/second goal" use the actor's email) and the sign-in cache all use the row's own accounts.
- Afterwards: "nothing is recorded" (AC-010) reads the item's only goal (`F005Arrange.OnlyGoal`) as that goal's owner. Both come from the Given step texts `f5.ac010.owner.<n>@example.com`, which Reqnroll fills in with the row's `n`, so the check refers to the row's own owner and goal. The owner's history read in AC-047 ("… as `f5.ac047.owner.<n>@example.com` gives …") names its reader in step text, also filled in by Reqnroll, so it reads as the row's own owner. In AC-047 rows 02 and 03, the owner's record identifier is looked up through `RecordIdAsync(goal, …)`, which reads as `goal.OwnerEmail`, the row's own owner.
