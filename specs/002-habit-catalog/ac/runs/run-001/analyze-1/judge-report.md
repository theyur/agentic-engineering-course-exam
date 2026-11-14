# Judge report — F002 / run-001 / analyze-1

All 53 failed scenarios are current (F002). All 93 regression scenarios (F001) passed. Every failure is a Reqnroll
`BindingException: Parameter count mismatch` raised before the step body runs, so no failure says anything about the
system. They share one root cause in the test code: step regexes have **optional capture groups**
(`(?: with (.+))?`, `( and publishes it)?`, `(?: of `([^`]+)`)?`, `(?: and `([^`]+)`)?`, and the `Actor` group inside
`(?:Actor|then)`). Reqnroll passes only the groups that matched as arguments. When the optional part is absent from the
step text, the binding gets fewer arguments than the method declares. Where the optional part is present, the same
bindings work. For example, AC-054 n05's Given "... who created the custom habit `Secret` with the category Mind" bound
fine.

## Summary
| Feature/AC | Scope (current / regression) | Verdict | Confidence | One-line reason |
|---|---|---|---|---|
| F002/AC-044 (n01–n28, 28 rows) | current | TEST_FAULT | high | `WhenCreatesTheCatalogHabit` has optional capture groups. "... with the icon X" leaves `( and publishes it)?` unmatched, giving 3 args for 4 params |
| F002/AC-018 | current | TEST_FAULT | high | Same binding (`WhenCreatesTheCatalogHabit`). No "and publishes it", so 3 args for 4 params |
| F002/AC-030 (n03) | current | TEST_FAULT | high | Same binding. "creates the catalog habit `A030 03 New`" leaves both optional groups unmatched, giving 2 args for 4 params |
| F002/AC-031 (n03) | current | TEST_FAULT | high | Same binding. 2 args for 4 params |
| F002/AC-006 (n01–n04) | current | TEST_FAULT | high | `GivenTheRegisteredUserWhoCreatedTheCustomHabit` has an optional `with (.+)` group. With no "with ..." it gets 2 args for 3 params |
| F002/AC-012 | current | TEST_FAULT | high | Same Given binding: 2 args for 3 params |
| F002/AC-017 (n01–n03) | current | TEST_FAULT | high | Same Given binding: 2 args for 3 params |
| F002/AC-048 | current | TEST_FAULT | high | Same Given binding (second user, `C habit`): 2 args for 3 params |
| F002/AC-052 | current | TEST_FAULT | high | Same Given binding: 2 args for 3 params |
| F002/AC-055 (n01–n05) | current | TEST_FAULT | high | Same Given binding: 2 args for 3 params |
| F002/AC-026 (n04–n06) | current | TEST_FAULT | high | `WhenEditsWithTheName` has an optional `of `owner`` group. Without it, 3 args for 4 params |
| F002/AC-051 (n02) | current | TEST_FAULT | high | Same edit binding (`WhenEditsWithTheName`): 3 args for 4 params |
| F002/AC-049 | current | TEST_FAULT | high | `WhenEditsItKeepingTheName` takes the actor from `(?:Actor|then)`. For "then edits it, ..." the actor group is unmatched, giving 2 args for 3 params |
| F002/AC-054 (n05) | current | TEST_FAULT | high | `WhenCreatesTheCustomHabit` has an optional `with (.+)` group. "creates the custom habit `Admin habit`" gives 2 args for 3 params |
| F002/AC-066 | current | TEST_FAULT | high | `GivenSignedInDeleted` has an optional `and `second`` group. "deleted `Cold shower`" gives 3 args for 4 params |

## Details (for the user)

### F002/AC-044 Only the 24 habit icons are accepted (all 28 example rows)
- Failing step: `When the administrator creates the catalog habit `A044 <n>` with the icon <icon>`. Binding error:
  `WhenCreatesTheCatalogHabit(String, String, String, String)` "should have 3 parameters".
- What the test does: `CatalogAdministrationSteps.cs` line 45 binds
  `^Actor creates the catalog habit `([^`]+)`(?: with (.+?))?( and publishes it)?$` to a 4-parameter method
  (`actor, name, clause, andPublish`). In this step the trailing `( and publishes it)?` group does not take part, so
  Reqnroll gives 3 arguments and refuses to call the method.
- What the AC item / scenario / spec requires: the `.feature` matches `acceptance.md` AC-044 word for word. The
  administrator creates the habit with the given icon, and the result is success for the 24 icons, or "Choose an icon."
  for the others (FR-003).
- Verdict and reasoning: TEST_FAULT. The scenario is faithful. The step definition cannot bind to the step text it is
  meant for. The system was never called.
- Suggested action: the writer fixes the binding (see feedback), then the tests are run again.

### F002/AC-018 A new catalog habit is a Draft that the public catalog does not show
- Failing step: `When the administrator creates the catalog habit `A018 Stretching` with the category Body, ... and the
  suggested starting goal Build, duration 10 minutes, 1 time per day`. Binding error: "should have 3 parameters".
- What the test does: the same `WhenCreatesTheCatalogHabit` binding. The `with (.+?)` group matches, but
  `( and publishes it)?` does not, so the binding gets 3 arguments for 4 parameters.
- What the AC item / scenario / spec requires: a create with those fields gives success, the habit has the state Draft
  in the administrator's list, and browsing does not show it (FR-011, FR-012).
- Verdict and reasoning: TEST_FAULT. This is the same binding defect.
- Suggested action: fix the binding and run the tests again.

### F002/AC-030 (n03) A registered user cannot manage the catalog
- Failing step: `When the registered user `ac030.user.03@example.com` creates the catalog habit `A030 03 New``.
  Binding error: "should have 2 parameters".
- What the test does: the same `WhenCreatesTheCatalogHabit` binding. Both optional groups are unmatched, so it gets 2
  arguments for 4 parameters.
- What the AC item / scenario / spec requires: the result is "Only administrators can manage the catalog.", and nothing
  changes (FR-014).
- Verdict and reasoning: TEST_FAULT. This is the same binding defect. The other example rows of AC-030 bind to other
  steps and pass.
- Suggested action: fix the binding and run the tests again.

### F002/AC-031 (n03) The anonymous visitor cannot manage the catalog
- Failing step: `When the anonymous visitor creates the catalog habit `A031 03 New``. Binding error: "should have 2
  parameters".
- What the test does: the same `WhenCreatesTheCatalogHabit` binding gets 2 arguments for 4 parameters.
- What the AC item / scenario / spec requires: the result is "You are not signed in.", and nothing changes (FR-014).
- Verdict and reasoning: TEST_FAULT. This is the same binding defect.
- Suggested action: fix the binding and run the tests again.

### F002/AC-006 (n01–n04), AC-012, AC-017 (n01–n03), AC-048, AC-052, AC-055 (n01–n05): Given "the registered user `x`, who created the custom habit `N`"
- Failing step: the Given (or And) `the registered user `<email>`, who created the custom habit `<name>``, with no
  "with ..." clause. Binding error: `GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)` "should
  have 2 parameters".
- What the test does: `ArrangeSteps.cs` line 51 binds
  `^the registered user `([^`]+)`, who created the custom habit `([^`]+)`(?: with (.+))?$` to
  `(email, name, clause)`. With no "with ..." clause, the third group is unmatched and only 2 arguments are passed. The
  same binding works in AC-049 and AC-054, where a "with ..." clause is present.
- What the AC item / scenario / spec requires: arrange a registered user who owns a custom habit with the default fields.
  Each scenario then checks something different: browsing never shows custom habits (AC-006), the Home preview excludes
  them (AC-012), catalog details refuse their identifiers (AC-017), a user lists only their own habits (AC-048), the same
  name is allowed across users and the catalog (AC-052), and the anonymous visitor is refused (AC-055).
- Verdict and reasoning: TEST_FAULT for all 15 scenarios. The `.feature` steps match the acceptance criteria. The Given
  binding cannot bind its own step text when the optional clause is absent, so the scenario's subject was never tested.
- Suggested action: fix the binding and run the tests again.

### F002/AC-026 (n04, n05, n06) A catalog-habit name is taken in every state, ignoring case and spaces
- Failing step: `When the administrator edits `A026 0x Other` with the name `" a026 0x READING "``. Binding error:
  `WhenEditsWithTheName(String, String, String, String)` "should have 3 parameters".
- What the test does: `EditSteps.cs` line 19 binds `^Actor edits `([^`]+)`(?: of `([^`]+)`)? with the name (.+)$` to
  `(actor, name, ownerEmail, newName)`. The optional `of `owner`` group is absent, so 3 arguments are passed.
- What the AC item / scenario / spec requires: the rename is refused with "A catalog habit with this name already
  exists.", and the list is unchanged (FR-004, FR-005).
- Verdict and reasoning: TEST_FAULT. This is the binding defect again. The create rows n01–n03 use a different step and
  pass.
- Suggested action: fix the binding and run the tests again.

### F002/AC-051 (n02) A user's custom-habit name is taken, ignoring case and spaces
- Failing step: `When the registered user edits `Evening walk` with the name `" cold SHOWER "``. Binding error: "should
  have 3 parameters".
- What the test does: the same `WhenEditsWithTheName` binding gets 3 arguments for 4 parameters.
- What the AC item / scenario / spec requires: the result is "You already have a custom habit with this name.", and the
  list stays `Cold shower`, `Evening walk`.
- Verdict and reasoning: TEST_FAULT. This is the same binding defect.
- Suggested action: fix the binding and run the tests again.

### F002/AC-049 A registered user edits every field of their custom habit
- Failing step: `And then edits it, keeping the name `Cold shower, 2 min`, to the category Health, the icon `moon` and
  the description "In the morning."`. Binding error: `WhenEditsItKeepingTheName(String, String, String)` "should have 2
  parameters".
- What the test does: `EditSteps.cs` line 58 binds `^(?:Actor|then) edits it, keeping the name `([^`]+)`, to (.+)$`.
  `Actor` (from `F002Text.Actor`) is itself a capture group. When the text starts with "then", that group is unmatched,
  so only `name` and `target` are passed to the 3-parameter method.
- What the AC item / scenario / spec requires: two successive edits by the owner, both succeed, and the final read gives
  every edited field (FR-020).
- Verdict and reasoning: TEST_FAULT. This is the binding defect again. The first edit step bound because the actor
  matched there.
- Suggested action: fix the binding and run the tests again.

### F002/AC-054 (n05) An administrator has no access to custom habits, whatever the fields
- Failing step: `When the administrator creates the custom habit `Admin habit``. Binding error:
  `WhenCreatesTheCustomHabit(String, String, String)` "should have 2 parameters".
- What the test does: `CustomHabitSteps.cs` line 21 binds `^Actor creates the custom habit `([^`]+)`(?: with (.+))?$` to
  `(actor, name, clause)`. The clause is absent, so 2 arguments are passed.
- What the AC item / scenario / spec requires: the refusal "Custom habits are for registered users." with no custom
  habit, and the owner's habit is unchanged (FR-025).
- Verdict and reasoning: TEST_FAULT. This is the same binding defect.
- Suggested action: fix the binding and run the tests again.

### F002/AC-066 Deleted demo custom habits are not created again
- Failing step: `And `demo1@example.com`, signed in with `example2026`, deleted `Cold shower``. Binding error:
  `GivenSignedInDeleted(String, String, String, String)` "should have 3 parameters".
- What the test does: `DemoLoadingSteps.cs` line 39 binds
  ``^`([^`]+)`, signed in with `([^`]+)`, deleted `([^`]+)`(?: and `([^`]+)`)?$`` to
  `(email, password, firstName, secondName)`. With a single deleted name, the optional group is unmatched and 3
  arguments are passed.
- What the AC item / scenario / spec requires: the `.feature` matches `acceptance.md` AC-066. The deleted demo custom
  habits are not created again by a second demo loading (FR-031, US6-AS7).
- Verdict and reasoning: TEST_FAULT. This is the binding defect again.
- Suggested action: fix the binding and run the tests again.

### Note for the user
Every failure stopped at binding, so the later steps of these 53 scenarios never ran. Their step bodies and assertions
were not exercised in this run. After the binding fix, new failures may show up in them, and they need judging on their
own.

<!-- WRITER-FEEDBACK:BEGIN -->
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
<!-- WRITER-FEEDBACK:END -->
