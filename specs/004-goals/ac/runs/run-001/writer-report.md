# Writer report — F004 / run-001

## Build

success, 2 attempts (the first one, before any F004 code, showed 12 errors in F001/F002 support code caused by the API change; see "Cross-feature changes"). No remaining errors or warnings.

Step definitions: `tests/HT.AcceptanceTests/StepDefinitions/F004/` (ArrangeSteps, GoalWhenSteps, GoalThenSteps, CalendarSteps, CatalogSteps, CustomHabitSteps, AccountSteps, DemoSteps, ResultSteps).
Support: `tests/HT.AcceptanceTests/Support/F004/` (F004Driver, F004Database, F004Context, F004Arrange, F004Text, F004Assert, F004Hooks).
Every `[Binding]` class carries `[Scope(Tag = "F004")]`. The only hook is a scoped `[BeforeFeature]` that resets HT-AC through `HtDatabase.ResetAsync`.

## Coverage

Abbreviations: Arrange = ArrangeSteps.cs, GoalWhen = GoalWhenSteps.cs, GoalThen = GoalThenSteps.cs, Calendar = CalendarSteps.cs, Catalog = CatalogSteps.cs, Custom = CustomHabitSteps.cs, Account = AccountSteps.cs, Demo = DemoSteps.cs, Result = ResultSteps.cs.

| AC tag | Scenario name | Step definition file(s) |
|---|---|---|
| AC-001 | Starting from a catalog habit without a position gives its marked suggested starting goal | Arrange, GoalWhen, GoalThen |
| AC-002 | Starting from a suggested starting goal position gives that suggestion | Arrange, GoalWhen, GoalThen |
| AC-003 | A catalog habit without suggested starting goals gives only Build or Reduce from its intent | Arrange, GoalWhen, GoalThen |
| AC-004 | A suggested starting goal position the habit does not have is refused | Arrange, GoalWhen, Result |
| AC-005 | Creating a goal with the starting values unchanged | Arrange, GoalWhen, GoalThen |
| AC-006 | Creating a goal with changed values keeps the changed values | Arrange, GoalWhen, GoalThen |
| AC-007 | Each field refusal of goal creation, with its message | Arrange, GoalWhen, Result, GoalThen |
| AC-008 | Goal creation gives the first refusal in FR-006 order | Arrange, GoalWhen, Result, GoalThen |
| AC-009 | The edges of every target range are accepted | Arrange, GoalWhen, GoalThen |
| AC-010 | The start date is today or up to 365 days later | Arrange, GoalWhen, GoalThen, Result |
| AC-011 | A goal name has at most 60 characters after trimming | Arrange, GoalWhen, GoalThen, Result |
| AC-012 | A Reduce goal with an amount has 1 time per period | Arrange, GoalWhen, GoalThen, Result |
| AC-013 | Values that do not belong to the chosen measurement or schedule are neither refused nor stored | Arrange, GoalWhen, GoalThen |
| AC-014 | A habit that is not a Published catalog habit is refused as a catalog habit | Arrange, GoalWhen, Result, GoalThen |
| AC-015 | A habit that changed between starting and creating is looked up again | Arrange, GoalWhen, Result, GoalThen |
| AC-016 | A goal without a name of its own is named after its habit and target | Arrange, GoalWhen, GoalThen |
| AC-017 | Several goals on one habit are independent | Arrange, GoalWhen, GoalThen |
| AC-018 | The target text follows the slice 2 rules, with the "avoided on" exception | Arrange, GoalWhen, GoalThen |
| AC-019 | The catalog suggestion text of the starting values follows the goal rules | Arrange, GoalWhen, GoalThen, Catalog |
| AC-020 | Every suggested starting goal of the seed catalog can be created unchanged | Arrange, GoalWhen, GoalThen |
| AC-021 | Listing gives every goal with its summary, in name order | Arrange, GoalWhen, GoalThen |
| AC-022 | Goals are listed by name ignoring case, then in creation order | Arrange, GoalWhen, GoalThen |
| AC-023 | The next commitment is the first commitment that has not ended | Arrange, GoalWhen, GoalThen |
| AC-024 | A goal that has not started gives its start date | Arrange, GoalWhen, GoalThen |
| AC-025 | Reading a goal gives all its fields, its settings version and its attempt | Arrange, GoalWhen, GoalThen |
| AC-026 | Reading a goal that has not started gives its first version and attempt from its start date | Arrange, GoalWhen, GoalThen |
| AC-027 | A user with no goals gets an empty list | Arrange, GoalWhen, GoalThen |
| AC-028 | A goal stays as it was created | Arrange, GoalWhen, GoalThen |
| AC-029 | Another user's goal does not exist for them | Arrange, GoalWhen, Result |
| AC-030 | A user's list never contains another user's goals | Arrange, GoalWhen, GoalThen |
| AC-031 | An administrator gets no goal use case | Arrange, GoalWhen, Catalog, Result |
| AC-032 | An anonymous visitor gets no goal use case | Arrange, GoalWhen, Catalog, Result |
| AC-033 | A custom habit is looked up only among the acting user's custom habits | Arrange, GoalWhen, Result, GoalThen |
| AC-034 | Catalog goal counts are the acting user's own | Arrange, Catalog |
| AC-035 | Use cases check the acting user, the role, the identifier and the fields, in this order | Arrange, GoalWhen, Result |
| AC-036 | A fixed-days goal has one commitment per scheduled day, from midnight to midnight in its time zone | Arrange, GoalWhen, Calendar |
| AC-037 | A per-day goal has one evaluation period per day | Arrange, GoalWhen, Calendar |
| AC-038 | A per-week goal has one evaluation period per Monday-to-Sunday week, from its first full week | Arrange, GoalWhen, Calendar |
| AC-039 | A per-week goal tells when its first week starts | Arrange, GoalWhen, GoalThen |
| AC-040 | A day or week with a clock change keeps local midnights and lasts 23 or 25 hours | Arrange, GoalWhen, Calendar |
| AC-041 | The days around a clock change meet at local midnight | Arrange, GoalWhen, Calendar |
| AC-042 | A range before the start date gives an empty calendar | Arrange, GoalWhen, Calendar |
| AC-043 | The calendar range has 1 to 366 days | Arrange, GoalWhen, Result, Calendar |
| AC-044 | "Today" is the date in the user's own time zone | Arrange, GoalWhen, GoalThen |
| AC-045 | A fixed-days goal starting on an unscheduled day begins at its next scheduled day | Arrange, GoalWhen, Calendar |
| AC-046 | Starting from a custom habit gives only Build or Reduce from its intent | Arrange, GoalWhen, GoalThen |
| AC-047 | A goal on a custom habit is based on it and named after it | Arrange, GoalWhen, GoalThen |
| AC-048 | A custom habit that goals use cannot be deleted | Arrange, Custom, Result, GoalThen |
| AC-049 | A custom habit that no goal uses is deleted | Arrange, Custom |
| AC-050 | Editing a custom habit renames its goals but never changes their settings | Arrange, Custom, GoalThen |
| AC-051 | Reading a custom habit gives the number of goals that use it | Arrange, Custom |
| AC-052 | Browsing gives a registered user their goal count per habit and the tracked count | Arrange, Catalog |
| AC-053 | "Only habits I track" returns the Published habits with the user's goals, with the other filters | Arrange, Catalog |
| AC-054 | A habit's details give the user's goal count and their goals on it, in name order | Arrange, Catalog |
| AC-055 | Anonymous visitors and administrators get the catalog without goal information | Arrange, Catalog, Result |
| AC-056 | Browsing and details without an acting user give the slice 2 result | Arrange, Catalog |
| AC-057 | Unpublishing a habit that goals use needs confirmation and gives only the number of goals | Arrange, Catalog |
| AC-058 | Unpublishing with confirmation unpublishes a habit that goals use | Arrange, Catalog |
| AC-059 | A habit that no goal uses is unpublished at once | Arrange, Catalog |
| AC-060 | A goal on an Unpublished habit keeps working, and no new goal can use that habit | Arrange, GoalWhen, GoalThen, Result |
| AC-061 | A habit published again is in the catalog again for goals | Arrange, GoalWhen, GoalThen |
| AC-062 | A renamed catalog habit renames the goals based on it | Arrange, Catalog, GoalThen |
| AC-063 | A new goal can start without a habit, but not be created without one | Arrange, GoalWhen, GoalThen |
| AC-064 | Demo loading gives each created demo account its demo goals | Arrange, Demo, GoalThen |
| AC-065 | Demo loading run again gives existing demo accounts no goals | Arrange, Demo |
| AC-066 | Demo goals are left out when their catalog habit is not Published | Arrange, Demo, GoalThen |
| AC-067 | A demo goal finds its catalog habit by name ignoring case, and only "Coffee" may be Unpublished | Arrange, Demo, GoalThen |
| AC-068 | Demo loading results name the demo email but never a goal, habit or account name | Arrange, Demo |
| AC-069 | The demo shows a goal on an Unpublished habit, a goal with its own name and an account name | Arrange, Demo |
| AC-070 | Registration keeps a named browser time zone, and otherwise gives the default | Arrange, Account |
| AC-071 | After a move west, a fixed-days goal keeps the old time zone up to its current date | Arrange, GoalWhen, Calendar |
| AC-072 | Each goal follows a time zone change from its own next day or week | Arrange, GoalWhen, Calendar |
| AC-073 | Reading a goal gives the version of its current date and the one later version | Arrange, GoalWhen, GoalThen |
| AC-074 | A value that is not a named time zone is refused and changes nothing | Arrange, Account, Result, GoalThen |
| AC-075 | A named time zone is kept as given, and the same time zone changes nothing | Arrange, Account, GoalThen |
| AC-076 | "Today" follows the new time zone at once | Arrange, Account, GoalThen |
| AC-077 | After a move west, a commitment already over is not the next one | Arrange, GoalWhen, GoalThen |
| AC-078 | After a move east, a commitment in progress is still the next one | Arrange, GoalWhen, GoalThen |
| AC-079 | Administrators read and change their own time zone; anonymous visitors cannot | Arrange, Account |
| AC-080 | A time zone is given only to its own account | Arrange, Account |
| AC-081 | A second change replaces a version not yet begun, and follows a version already in effect | Arrange, GoalWhen, GoalThen, Calendar |
| AC-082 | After a large move east, a version already begun is in effect, and a second change follows it | Arrange, GoalWhen, GoalThen, Calendar |
| AC-083 | A new version's first period that has already ended is kept | Arrange, GoalWhen, Calendar, GoalThen |
| AC-084 | An account name is trimmed, and empty means no name | Arrange, Account |
| AC-085 | An account name has at most 60 characters | Arrange, Account |
| AC-086 | The Home greeting follows the time of day in the account's time zone | Arrange, Account |
| AC-087 | Without a name the greeting has no name | Arrange, Account |
| AC-088 | The Home date is today in the account's time zone, in English whatever the culture | Arrange, Account |
| AC-089 | The profile gives "member since" with the month and year of creation in the account's time zone | Arrange, Account |
| AC-090 | An account name is given only to its own account; administrators have their own | Arrange, Account |
| AC-091 | An anonymous visitor gets no Home heading, no name change and no profile | Account, Result |
| AC-092 | A goal keeps the values it was created with, whatever happens to the suggested starting goal | Arrange, GoalWhen, GoalThen |
| AC-093 | A confirmed unpublish acts even when goals were added after the count was given | Arrange, Catalog, GoalThen |
| AC-094 | At registration, the name is checked after slice 1's checks | Account |

## Unexpressible steps

None.

## Cross-feature changes

| File | Change | Reason |
|---|---|---|
| tests/HT.AcceptanceTests/Support/F001/AccountsDriver.cs | `AccountService`, `OperatorService` and `DemoDataService` are now constructed with the added `TimeProvider` and `InstallationTimeZone` arguments (the existing `FakeTimeProvider` `Time`, and `InstallationTimeZone.From(null)`, i.e. UTC). No step, setup, expected value or assertion changed. | Their constructors gained parameters in this slice (CS7036). FR-025: acceptance tests supply no default time zone. |
| tests/HT.AcceptanceTests/Support/F002/F002Driver.cs | Same constructor fixes for `AccountService`, `OperatorService` and `DemoDataService`; `CatalogService`, `CatalogAdministrationService` and `CustomHabitService` now also get the `IGoalStore` resolved from the container; added `using HT.Application.Goals;`. F002 still calls the slice 2 overloads (`BrowseAsync(filter)`, `GetDetailsAsync(id)`, `UnpublishAsync(actor, id)`), so no step, expected value or assertion changed. | Their constructors gained parameters in this slice (CS7036). |

Shared support (`Support/Shared/`) is unchanged.

## Missing packages

None. (NodaTime, already referenced, is used only to compute a test input: "the start date today in the acting user's time zone".)

## Interpretations and assumptions

- All items, "Time" convention: setup steps that give no time (administrator creation, registration, sign-in, creating, publishing or unpublishing a catalog habit, creating a custom habit) run at `2026-11-18T10:00:00Z`. Reqnroll does not let a step see the steps that come after it, so the convention's "or at the earliest time the item gives when that is earlier" is not applied. No item checks a time-dependent result of such a setup step: the only one, "member since" (AC-089), registers at an explicit time. Any other Given or When step that gives no time runs at the later of `2026-11-18T10:00:00Z` and the latest time given so far. Then steps run at the time of the When step. A fake time provider can only move forward, so a step at an earlier time gets a fresh `FakeTimeProvider` started at that time.
- All create steps: when the step gives no start date, the input is "today" in the acting user's time zone (the zone given at registration or by a successful change, otherwise UTC). The test computes it with NodaTime's tzdb, not with the system under test. This is an input value, not an expected value.
- All items: the installation's default time zone is `InstallationTimeZone.From(null)`, since acceptance tests configure none (FR-025). The application is composed with `AddHtPersistence` and `AddHtApplication`, and each use-case service is constructed per call with the stores from the container, the current fake time and that default.
- All Given steps that state something happened ("created a goal", "changed their time zone", "unpublished …", "started a new goal") assert that the use case succeeded (Created, Changed, Unpublished, Started).
- AC-001/002/019/046 "the starting values are: …": every listed field is asserted. Fields the step does not list are asserted empty only when the step says "and nothing else" (AC-003, AC-046, AC-063). "no name" accepts a null or empty name.
- AC-005 "is owned by the user": checked through the owner's goal list holding the created goal's identifier with the stated name.
- AC-013 "no number of times": asserts `Times` is null. "no fixed days": asserts `Days` is empty.
- AC-016/AC-025 "which is (not) its own" and "the name is (not) its own": checked against `GoalDetails.HasOwnName`.
- AC-017 "the goal list has 3 goals on `A4.017 Reading`": entries are selected by habit name. The names are compared as a multiset.
- AC-020: "every creation succeeds" also requires at least one creation, so the step cannot pass with nothing in it. Browsing gives only the marked suggestion, so the number of suggestions per habit comes from the habit's details (slice 2 call without an acting user).
- AC-021 "the list is, in this order:": the header asserts that the list was found and is not empty. Each following entry line checks the next position. "each entry has its goal's identifier" asserts that every line was used and that the entries' identifiers are exactly the user's created goals, distinct. Together these make the list exact.
- AC-024 "listed at `T`, …": a fresh list by the same account at T.
- AC-036 "no entry has a status": reflection on the returned calendar, commitment and period types; none may have a public property whose name contains "Status".
- AC-037 "each runs from 05:00:00Z of its date to 05:00:00Z of the next date": checked for every period from its own date with the times written in the step, plus the literal first and last instants.
- AC-038/040/045/072/081/082/083: entry lists are exact (count and order). "(N hours)" checks `Ends - Starts`. "N commitments" / "N evaluation periods" also check the calendar's schedule kind.
- AC-039 and other "reading the goal gives …" Then steps make a new read of the scenario's last created goal, as its owner, at the When step's time.
- AC-052 "`A` has the goal count 2, `B` 0 and `C` 1, and the tracked count is 2": the three counts and the tracked count are asserted. The number of returned habits is not asserted (the step does not state it).
- AC-053 "the result is `A` and `B`, and nothing else": the returned names equal exactly {A, B}. "`X` only": exactly one habit.
- AC-055/056 "no goals": `Goals` is null or empty. "no goal count": `GoalCount` is null.
- AC-057 "the result carries nothing else": the unpublish result type has exactly the public properties Outcome, Message and GoalCount; the message contains no "@" and no goal identifier.
- AC-058 "does not contain it": "it" is the habit the When step unpublished, matched by identifier.
- AC-058/059/093 "`X` is Unpublished": the unpublish outcome is Unpublished, and the administrator's read of the habit gives the state Unpublished.
- AC-064 "`demoN`'s goal list is, in this order: …": the list is exact in names and order; the facts in parentheses, and "each Period-based", are checked per entry.
- AC-065 "the same as after the first run": the When step lists each demo account's goal names before it runs demo loading again; the Then step compares the counts written in the step and those names.
- AC-066/067 "is exactly A and B" without "in this order": compared as sets; with "in this order", as sequences.
- AC-069 "each demo account reads its goals": list, then read every listed goal; the Then steps use those reads.
- AC-075 "the change succeeds": Changed or Unchanged (FR-026: "the same time zone is a success that changes nothing").
- AC-079: the administrator's reads and change are checked in call order; both anonymous calls must give NotSignedIn with the text.
- AC-080 "neither result contains `Europe/Kyiv`": checks the time zone and the message of both reads.
- AC-088: the culture is applied (CurrentCulture and CurrentUICulture) only around the Home heading call and restored after it, so it does not leak into other scenarios.
- AC-089 "the profile gives "member since November 2026"": `ProfileView.MemberSince` must equal the whole literal text (`AccountTexts.MemberSince` suggests the use case gives the full phrase).
- AC-090 "does not contain "Yuri"": the name is empty and neither the email nor "member since" contains it.
- AC-094 "registration does not succeed": the outcome is not Registered, there is no account identifier, and the message is not the name-length text.
- AC-007/008 and others: the `GoalField` of a refusal is not checked, as acceptance.md says.
- "a text of N characters" is `x` repeated N times, and "a unit of N characters" is `u` repeated N times (acceptance.md "Texts"). `` `"   "` `` is three spaces and `` `""` `` is the empty string.
- No difference between the `.feature` scenarios and acceptance.md items was found in the parts compared (conventions, AC-022, AC-028, AC-039, AC-071 to AC-094).
