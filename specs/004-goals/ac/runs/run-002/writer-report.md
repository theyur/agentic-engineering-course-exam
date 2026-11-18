# Writer report — F004 / run-002

## Build
success, 1 attempt, no remaining errors (0 warnings, 0 errors).

## Coverage
All 94 scenarios in `tests/HT.AcceptanceTests/Features/F004/goals.feature` are bound. The paths are relative to `tests/HT.AcceptanceTests/StepDefinitions/F004/`. The shared support code is in `tests/HT.AcceptanceTests/Support/F004/`: `F004Context`, `F004Driver`, `F004Arrange`, `F004Text`, `F004Assert`, `F004Database` and `F004Hooks`.

| AC tag | Scenario name | Step definition file(s) |
|---|---|---|
| AC-001 | Starting from a catalog habit without a position gives its marked suggested starting goal | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-002 | Starting from a suggested starting goal position gives that suggestion | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-003 | A catalog habit without suggested starting goals gives only Build or Reduce from its intent | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-004 | A suggested starting goal position the habit does not have is refused | ArrangeSteps.cs, GoalWhenSteps.cs, ResultSteps.cs |
| AC-005 | Creating a goal with the starting values unchanged | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-006 | Creating a goal with changed values keeps the changed values | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-007 | Each field refusal of goal creation, with its message | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs, ResultSteps.cs |
| AC-008 | Goal creation gives the first refusal in FR-006 order | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs, ResultSteps.cs |
| AC-009 | The edges of every target range are accepted | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-010 | The start date is today or up to 365 days later | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs, ResultSteps.cs |
| AC-011 | A goal name has at most 60 characters after trimming | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs, ResultSteps.cs |
| AC-012 | A Reduce goal with an amount has 1 time per period | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs, ResultSteps.cs |
| AC-013 | Values that do not belong to the chosen measurement or schedule are neither refused nor stored | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-014 | A habit that is not a Published catalog habit is refused as a catalog habit | ArrangeSteps.cs, GoalWhenSteps.cs, ResultSteps.cs, GoalThenSteps.cs |
| AC-015 | A habit that changed between starting and creating is looked up again | ArrangeSteps.cs, GoalWhenSteps.cs, ResultSteps.cs, GoalThenSteps.cs |
| AC-016 | A goal without a name of its own is named after its habit and target | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-017 | Several goals on one habit are independent | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-018 | The target text follows the slice 2 rules, with the "avoided on" exception | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-019 | The catalog suggestion text of the starting values follows the goal rules | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-020 | Every suggested starting goal of the seed catalog can be created unchanged | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-021 | Listing gives every goal with its summary, in name order | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-022 | Goals are listed by name ignoring case, then in creation order | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-023 | The next commitment is the first commitment that has not ended | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-024 | A goal that has not started gives its start date | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-025 | Reading a goal gives all its fields, its settings version and its attempt | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-026 | Reading a goal that has not started gives its first version and attempt from its start date | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-027 | A user with no goals gets an empty list | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-028 | A goal stays as it was created | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-029 | Another user's goal does not exist for them | ArrangeSteps.cs, GoalWhenSteps.cs, ResultSteps.cs, GoalThenSteps.cs |
| AC-030 | A user's list never contains another user's goals | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-031 | An administrator gets no goal use case | ArrangeSteps.cs, GoalWhenSteps.cs, ResultSteps.cs |
| AC-032 | An anonymous visitor gets no goal use case | ArrangeSteps.cs, GoalWhenSteps.cs, ResultSteps.cs |
| AC-033 | A custom habit is looked up only among the acting user's custom habits | ArrangeSteps.cs, GoalWhenSteps.cs, ResultSteps.cs, GoalThenSteps.cs |
| AC-034 | Catalog goal counts are the acting user's own | ArrangeSteps.cs, CatalogSteps.cs |
| AC-035 | Use cases check the acting user, the role, the identifier and the fields, in this order | ArrangeSteps.cs, GoalWhenSteps.cs, ResultSteps.cs |
| AC-036 | A fixed-days goal has one commitment per scheduled day, from midnight to midnight in its time zone | ArrangeSteps.cs, GoalWhenSteps.cs, CalendarSteps.cs |
| AC-037 | A per-day goal has one evaluation period per day | ArrangeSteps.cs, GoalWhenSteps.cs, CalendarSteps.cs |
| AC-038 | A per-week goal has one evaluation period per Monday-to-Sunday week, from its first full week | ArrangeSteps.cs, GoalWhenSteps.cs, CalendarSteps.cs |
| AC-039 | A per-week goal tells when its first week starts | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-040 | A day or week with a clock change keeps local midnights and lasts 23 or 25 hours | ArrangeSteps.cs, GoalWhenSteps.cs, CalendarSteps.cs |
| AC-041 | The days around a clock change meet at local midnight | ArrangeSteps.cs, GoalWhenSteps.cs, CalendarSteps.cs |
| AC-042 | A range before the start date gives an empty calendar | ArrangeSteps.cs, GoalWhenSteps.cs, CalendarSteps.cs |
| AC-043 | The calendar range has 1 to 366 days | ArrangeSteps.cs, GoalWhenSteps.cs, CalendarSteps.cs, ResultSteps.cs |
| AC-044 | "Today" is the date in the user's own time zone | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs, ResultSteps.cs |
| AC-045 | A fixed-days goal starting on an unscheduled day begins at its next scheduled day | ArrangeSteps.cs, GoalWhenSteps.cs, CalendarSteps.cs |
| AC-046 | Starting from a custom habit gives only Build or Reduce from its intent | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-047 | A goal on a custom habit is based on it and named after it | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-048 | A custom habit that goals use cannot be deleted | ArrangeSteps.cs, CustomHabitSteps.cs, ResultSteps.cs |
| AC-049 | A custom habit that no goal uses is deleted | ArrangeSteps.cs, CustomHabitSteps.cs |
| AC-050 | Editing a custom habit renames its goals but never changes their settings | ArrangeSteps.cs, CustomHabitSteps.cs, GoalThenSteps.cs |
| AC-051 | Reading a custom habit gives the number of goals that use it | ArrangeSteps.cs, CustomHabitSteps.cs |
| AC-052 | Browsing gives a registered user their goal count per habit and the tracked count | ArrangeSteps.cs, CatalogSteps.cs |
| AC-053 | "Only habits I track" returns the Published habits with the user's goals, with the other filters | ArrangeSteps.cs, CatalogSteps.cs |
| AC-054 | A habit's details give the user's goal count and their goals on it, in name order | ArrangeSteps.cs, CatalogSteps.cs |
| AC-055 | Anonymous visitors and administrators get the catalog without goal information | ArrangeSteps.cs, CatalogSteps.cs |
| AC-056 | Browsing and details without an acting user give the slice 2 result | ArrangeSteps.cs, CatalogSteps.cs |
| AC-057 | Unpublishing a habit that goals use needs confirmation and gives only the number of goals | ArrangeSteps.cs, CatalogSteps.cs |
| AC-058 | Unpublishing with confirmation unpublishes a habit that goals use | ArrangeSteps.cs, CatalogSteps.cs |
| AC-059 | A habit that no goal uses is unpublished at once | ArrangeSteps.cs, CatalogSteps.cs |
| AC-060 | A goal on an Unpublished habit keeps working, and no new goal can use that habit | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs, ResultSteps.cs |
| AC-061 | A habit published again is in the catalog again for goals | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-062 | A renamed catalog habit renames the goals based on it | ArrangeSteps.cs, CatalogSteps.cs, GoalThenSteps.cs |
| AC-063 | A new goal can start without a habit, but not be created without one | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs, ResultSteps.cs |
| AC-064 | Demo loading gives each created demo account its demo goals | ArrangeSteps.cs, DemoSteps.cs, GoalThenSteps.cs |
| AC-065 | Demo loading run again gives existing demo accounts no goals | ArrangeSteps.cs, DemoSteps.cs, GoalThenSteps.cs |
| AC-066 | Demo goals are left out when their catalog habit is not Published | ArrangeSteps.cs, DemoSteps.cs, GoalThenSteps.cs |
| AC-067 | A demo goal finds its catalog habit by name ignoring case, and only "Coffee" may be Unpublished | ArrangeSteps.cs, DemoSteps.cs, GoalThenSteps.cs |
| AC-068 | Demo loading results name the demo email but never a goal, habit or account name | ArrangeSteps.cs, DemoSteps.cs |
| AC-069 | The demo shows a goal on an Unpublished habit, a goal with its own name and an account name | ArrangeSteps.cs, DemoSteps.cs |
| AC-070 | Registration keeps a named browser time zone, and otherwise gives the default | ArrangeSteps.cs, AccountSteps.cs |
| AC-071 | After a move west, a fixed-days goal keeps the old time zone up to its current date | ArrangeSteps.cs, GoalWhenSteps.cs, CalendarSteps.cs |
| AC-072 | Each goal follows a time zone change from its own next day or week | ArrangeSteps.cs, GoalWhenSteps.cs, CalendarSteps.cs, GoalThenSteps.cs |
| AC-073 | Reading a goal gives the version of its current date and the one later version | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-074 | A value that is not a named time zone is refused and changes nothing | ArrangeSteps.cs, AccountSteps.cs, ResultSteps.cs |
| AC-075 | A named time zone is kept as given, and the same time zone changes nothing | ArrangeSteps.cs, AccountSteps.cs |
| AC-076 | "Today" follows the new time zone at once | ArrangeSteps.cs, AccountSteps.cs, GoalThenSteps.cs |
| AC-077 | After a move west, a commitment already over is not the next one | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-078 | After a move east, a commitment in progress is still the next one | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs |
| AC-079 | Administrators read and change their own time zone; anonymous visitors cannot | ArrangeSteps.cs, AccountSteps.cs |
| AC-080 | A time zone is given only to its own account | ArrangeSteps.cs, AccountSteps.cs |
| AC-081 | A second change replaces a version not yet begun, and follows a version already in effect | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs, CalendarSteps.cs |
| AC-082 | After a large move east, a version already begun is in effect, and a second change follows it | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs, CalendarSteps.cs |
| AC-083 | A new version's first period that has already ended is kept | ArrangeSteps.cs, GoalWhenSteps.cs, CalendarSteps.cs |
| AC-084 | An account name is trimmed, and empty means no name | ArrangeSteps.cs, AccountSteps.cs |
| AC-085 | An account name has at most 60 characters | ArrangeSteps.cs, AccountSteps.cs |
| AC-086 | The Home greeting follows the time of day in the account's time zone | ArrangeSteps.cs, AccountSteps.cs |
| AC-087 | Without a name the greeting has no name | ArrangeSteps.cs, AccountSteps.cs |
| AC-088 | The Home date is today in the account's time zone, in English whatever the culture | ArrangeSteps.cs, AccountSteps.cs |
| AC-089 | The profile gives "member since" with the month and year of creation in the account's time zone | ArrangeSteps.cs, AccountSteps.cs |
| AC-090 | An account name is given only to its own account; administrators have their own | ArrangeSteps.cs, AccountSteps.cs |
| AC-091 | An anonymous visitor gets no Home heading, no name change and no profile | ArrangeSteps.cs, AccountSteps.cs, ResultSteps.cs |
| AC-092 | A goal keeps the values it was created with, whatever happens to the suggested starting goal | ArrangeSteps.cs, GoalWhenSteps.cs, GoalThenSteps.cs, CatalogSteps.cs |
| AC-093 | A confirmed unpublish acts even when goals were added after the count was given | ArrangeSteps.cs, CatalogSteps.cs |
| AC-094 | At registration, the name is checked after slice 1's checks | AccountSteps.cs |

## Unexpressible steps
None.

## Cross-feature changes
None. This run changed only files in `StepDefinitions/F004/` and `Support/F004/`. The working tree also shows changes to `Support/F001/AccountsDriver.cs`, `Support/F002/F002Driver.cs` and the project file. They come from before this run, and this run did not touch them.

## Missing packages
None.

## Interpretations and assumptions
- All items: an optional "at `T` " or "at `T` (…) " prefix. A step without the prefix runs at the default time or the latest time an earlier step gave (unchanged from run-001). The anonymous visitor's registration Given still runs at the setup time when it gives no time.
- AC-007 to AC-093 (every "the registered user `x`" step): a step without " in `Z`" registers the user with no time zone, as acceptance.md's "registered user" convention says. With " in `Z`" the user registers with the time zone `Z`.
- AC-058: "`X` is Unpublished" (alone) checks that the last unpublish result is Unpublished and that the administrator's read of the habit gives the state Unpublished. The ", with no "confirmation needed" result" form (AC-059) also checks that the outcome is not ConfirmationNeeded.
- "the result is a calendar of N commitments, …": no scenario in `goals.feature` uses it. Its two forms are now split into two bindings, so neither has a group that can skip the match.

## Feedback addressed
- Root cause (a capture group in an optional part or an alternative passes no argument when it does not match) — every F004 binding now has only groups that always take part in the match. Each binding either keeps an always-participating group that is empty when the part is not given, `((?:…)?)`, or is split into one binding per step form. The new shared prefix `F004Text.AtPrefix` = `((?:at `[^`]+`(?: \([^)]*\))? )?)` and its parser `F004Text.AtTime` replace every `(?:at `([^`]+)`…)?` form. `F004Text.BacktickedIn` reads an optional backticked clause.
- 1 `ArrangeSteps.GivenTheRegisteredUser` — the pattern is now `^the registered user `([^`]+)`((?: in `[^`]+`)?)((?:, who .+)?)$`. An empty " in" clause means no time zone, and an empty ", who" clause means no extra setup.
- 2 `ArrangeSteps.GivenTheRegisteredUsers` — split into "the registered users `a` and `b`" and a separate `GivenTheRegisteredUsersWhoEachCreatedAGoal` for the ", who each created a goal on …" form.
- 3 `ArrangeSteps.GivenTheAnonymousVisitorRegistered` — now uses `F004Text.AtPrefix`. An empty prefix still runs at the setup time.
- 4 `ArrangeSteps.GivenCreatedAGoal` — now uses `F004Text.AtPrefix`.
- 5 `ArrangeSteps.GivenDemoLoadingRan` — split into "demo loading ran" and "demo loading ran and its catalog result was "…"". The catalog-result assertion is no longer conditional.
- 6 `GoalWhenSteps.WhenStartsANewGoal` — now uses the always-participating `At` prefix and is split into a binding without a position and `WhenStartsANewGoalWithThePosition`.
- 7 `GoalWhenSteps.WhenCreatesAGoal` — now uses the always-participating `At` prefix.
- 8 `AccountSteps.WhenReadsTheHomeHeading` — now uses the always-participating `At` prefix.
- 9 `AccountSteps.WhenTheVisitorRegisters` — split into a single registration and `WhenTheVisitorRegistersTwice` for the ", then registers the same email …" form.
- 10 `CatalogSteps.ThenTheHabitIsUnpublished` — split into "`X` is Unpublished" and `ThenTheHabitIsUnpublishedWithNoConfirmationNeeded`. Neither has a conditional assertion any more.
- 11 `GoalWhenSteps.WhenListsTheirGoals`, `WhenReadsTheGoal` and `WhenReadsTheEvaluationCalendarOf` — now use the always-participating `At` prefix. `CatalogSteps.ThenTheDetailsGiveTheGoalCount` is split into "… count N" and `ThenTheDetailsGiveTheGoalCountAndNoGoals`, with no conditional assertion. `ArrangeSteps.GivenTheAdministratorUnpublished` now uses the always-participating `((?: and then published it again)?)`.
- Same fault found and fixed in two bindings the feedback did not list:
  - `GoalWhenSteps.WhenUsesAHabitGivenAsAKind` had `(starts a new goal from|creates a goal with (.+?) on)`, whose inner group does not take part in the "starts" form (AC-014, AC-033). It is split into `WhenStartsFromAHabitGivenAsAKind` and `WhenCreatesOnAHabitGivenAsAKind`.
  - `CalendarSteps.ThenTheResultIsACalendarOf` had alternative capture groups. It is split into two bindings, one per form.
