## Feedback for the test writer

Root cause for every item: Reqnroll passes as method arguments only the regex capture groups that matched. An optional
capture group that is absent from the step text drops its argument, and the binding fails with a parameter count
mismatch before the step runs. Fix each binding so that its parameter count is the same for every step text it must
match. Either make each group always take part (for example `((?: with .+?)?)` or `( and publishes it|)`, capturing an
empty string, and then treat empty as absent), or split the binding into one step definition per form. Keep the step
texts in the `.feature` file unchanged.

1. `StepDefinitions/F002/CatalogAdministrationSteps.cs`, `WhenCreatesTheCatalogHabit` (line 45, regex
   `... creates the catalog habit `([^`]+)`(?: with (.+?))?( and publishes it)?$`, 4 parameters). It must bind:
   "creates the catalog habit `N`" with nothing after it (AC-030 n03, AC-031 n03), "... with <fields>" without
   "and publishes it" (AC-018, AC-044 all rows), and the forms that have "and publishes it".
2. `StepDefinitions/F002/ArrangeSteps.cs`, `GivenTheRegisteredUserWhoCreatedTheCustomHabit` (line 51, optional
   `(?: with (.+))?`, 3 parameters). It must bind "the registered user `x`, who created the custom habit `N`" with no
   "with" clause (AC-006, AC-012, AC-017, AC-048, AC-052, AC-055), and the form with the clause (AC-049, AC-054).
3. `StepDefinitions/F002/EditSteps.cs`, `WhenEditsWithTheName` (line 19, optional `(?: of `([^`]+)`)?`, 4 parameters).
   It must bind "<actor> edits `N` with the name ..." without an "of `owner`" part (AC-026 n04–n06, AC-051 n02).
4. `StepDefinitions/F002/EditSteps.cs`, `WhenEditsItKeepingTheName` (line 58, `(?:Actor|then)`, 3 parameters). The
   `Actor` capture group does not take part when the step starts with "then", as in AC-049 "And then edits it, keeping
   the name ...". The same pattern is in `WhenEditsItTo` (line 49), which only passed because AC-049's first edit names
   the actor. Check every step that starts with "then edits it ...".
5. `StepDefinitions/F002/CustomHabitSteps.cs`, `WhenCreatesTheCustomHabit` (line 21, optional `(?: with (.+))?`, 3
   parameters). It must bind "<actor> creates the custom habit `N`" with no clause (AC-054 n05, AC-055 n03).
6. `StepDefinitions/F002/DemoLoadingSteps.cs`, `GivenSignedInDeleted` (line 39, optional ``(?: and `([^`]+)`)?``, 4
   parameters). It must bind "... deleted `N`" with a single name (AC-066, `demo1@example.com`) as well as the form with
   two names.

Also check every other F002 step definition for an optional capture group, or for a capture group inside an optional
or alternated part. Any of them fails the same way whenever its optional part is absent.
