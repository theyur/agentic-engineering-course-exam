# Judge report — F004 / run-001 / analyze-1

Current feature: 299 scenarios, 49 passed, 103 failed, 147 pending. Regression (F001 to F003): 353 scenarios, all passed.
All 250 non-passed scenarios are current (F004). Every one stops at a `Reqnroll.BindingException` ("Parameter count mismatch") or a `Reqnroll.MissingStepDefinitionException`. No use-case assertion failed anywhere in this run.

## Summary
| Feature/AC | Scope (current / regression) | Verdict | Confidence | One-line reason |
|---|---|---|---|---|
| F004/AC-001 (2 rows) | current | TEST_FAULT | high | `WhenStartsANewGoal`: optional `At` / position capture groups not matched, so the arguments don't fit the binding |
| F004/AC-002 | current | TEST_FAULT | high | `WhenStartsANewGoal`: optional `At` capture group not matched, so the arguments don't fit the binding |
| F004/AC-003 | current | TEST_FAULT | high | `WhenStartsANewGoal`: optional capture groups not matched, so the arguments don't fit the binding |
| F004/AC-004 (2 rows) | current | TEST_FAULT | high | `WhenStartsANewGoal`: optional `At` capture group not matched, so the arguments don't fit the binding |
| F004/AC-006 | current | TEST_FAULT | high | `WhenCreatesAGoal`: optional `At` capture group not matched, so the arguments don't fit the binding |
| F004/AC-007 (22 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in`/`who` groups not matched; Reqnroll skips the binding and the step is undefined |
| F004/AC-008 (14 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-009 (8 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-010 (5 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-011 (3 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-012 (6 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-013 (6 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-014 (8 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-015 (2 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-016 (4 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-017 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in` group not matched, so the arguments don't fit the binding |
| F004/AC-018 (23 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-019 | current | TEST_FAULT | high | `WhenStartsANewGoal`: optional capture groups not matched, so the arguments don't fit the binding |
| F004/AC-020 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-021 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-022 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-023 (5 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-024 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-025 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in` group not matched, so the arguments don't fit the binding |
| F004/AC-026 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-027 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-028 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in` group not matched, so the arguments don't fit the binding |
| F004/AC-029 (4 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in` group not matched, so the arguments don't fit the binding |
| F004/AC-030 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in` group not matched, so the arguments don't fit the binding |
| F004/AC-031 (6 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in` group not matched, so the arguments don't fit the binding |
| F004/AC-032 (6 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in` group not matched, so the arguments don't fit the binding |
| F004/AC-033 (5 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-034 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in` group not matched, so the arguments don't fit the binding |
| F004/AC-035 (7 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-036 | current | TEST_FAULT | high | `GivenCreatedAGoal`: optional `at` capture group not matched, so the arguments don't fit the binding |
| F004/AC-037 | current | TEST_FAULT | high | `GivenCreatedAGoal`: optional `at` capture group not matched, so the arguments don't fit the binding |
| F004/AC-038 (4 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-039 (4 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-040 (6 rows) | current | TEST_FAULT | high | `GivenCreatedAGoal`: optional `at` capture group not matched, so the arguments don't fit the binding |
| F004/AC-041 | current | TEST_FAULT | high | `GivenCreatedAGoal`: optional `at` capture group not matched, so the arguments don't fit the binding |
| F004/AC-042 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-043 (4 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-045 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-046 (2 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-047 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-048 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-049 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-050 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-051 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-052 (3 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in` group not matched, so the arguments don't fit the binding |
| F004/AC-053 (3 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in` group not matched, so the arguments don't fit the binding |
| F004/AC-054 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in` group not matched, so the arguments don't fit the binding |
| F004/AC-055 (6 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in` group not matched, so the arguments don't fit the binding |
| F004/AC-056 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in` group not matched, so the arguments don't fit the binding |
| F004/AC-057 (3 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUsers`: optional `who each created` groups not matched, so the arguments don't fit the binding |
| F004/AC-058 | current | TEST_FAULT | high | `ThenTheHabitIsUnpublished`: optional capture group not matched, so the arguments don't fit the binding |
| F004/AC-060 (5 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in` group not matched, so the arguments don't fit the binding |
| F004/AC-061 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in` group not matched, so the arguments don't fit the binding |
| F004/AC-062 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in` group not matched, so the arguments don't fit the binding |
| F004/AC-063 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-065 | current | TEST_FAULT | high | `GivenDemoLoadingRan`: optional catalog-result group not matched, so the arguments don't fit the binding |
| F004/AC-069 | current | TEST_FAULT | high | `GivenDemoLoadingRan`: optional catalog-result group not matched, so the arguments don't fit the binding |
| F004/AC-070 (17 rows) | current | TEST_FAULT | high | `GivenTheAnonymousVisitorRegistered`: optional `at` group not matched, so the arguments don't fit the binding |
| F004/AC-080 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in` group not matched, so the arguments don't fit the binding |
| F004/AC-084 (4 rows) | current | TEST_FAULT | high | `GivenTheAnonymousVisitorRegistered`: optional `at` group not matched, so the arguments don't fit the binding |
| F004/AC-085 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in` group not matched, so the arguments don't fit the binding |
| F004/AC-086 (7 rows) | current | TEST_FAULT | high | `GivenTheAnonymousVisitorRegistered`: optional `at` group not matched, so the arguments don't fit the binding |
| F004/AC-087 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional groups not matched; step undefined |
| F004/AC-088 (3 rows) | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in` group not matched, so the arguments don't fit the binding |
| F004/AC-090 | current | TEST_FAULT | high | `GivenTheAnonymousVisitorRegistered`: optional `at` group not matched, so the arguments don't fit the binding |
| F004/AC-091 | current | TEST_FAULT | high | `WhenReadsTheHomeHeading`: optional `At` group not matched, so the arguments don't fit the binding |
| F004/AC-092 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in` group not matched, so the arguments don't fit the binding |
| F004/AC-093 | current | TEST_FAULT | high | `GivenTheRegisteredUser`: optional `in` group not matched, so the arguments don't fit the binding |
| F004/AC-094 | current | TEST_FAULT | high | `WhenTheVisitorRegisters`: optional second-registration groups not matched, so the arguments don't fit the binding |

"(N rows)" is the number of non-passed Examples rows of that item. Rows of an item not listed here passed. Items with no row in this table passed in full.

## Details (for the user)

### Common root cause (applies to every item below)

- **The mechanism.** Reqnroll builds a regex binding's arguments only from the capture groups that took part in the match. A capture group inside an optional part (`(?: … (…))?`, or `(…)?`) that does not match gives no argument. Reqnroll does not pass null or an empty string for it. The method then has more parameters than Reqnroll has arguments. Reqnroll handles this in one of two ways:
  - If no other binding matches the step, it reports `BindingException: Parameter count mismatch! … should have N parameters` (the 103 failed scenarios).
  - If an out-of-scope binding also matches, it discards the F004 binding and reports "There are matching step definitions, but none of them have matching scope". The out-of-scope bindings here are the `[Scope(Tag = "F002")]` `ArrangeSteps.GivenTheRegisteredUser`, `GivenTheRegisteredUserWhoCreatedTheCustomHabit` and `GivenTheRegisteredUserWhoCreatedTheCustomHabits`. This gives the 147 pending scenarios.
- **The evidence.**
  - The trace for AC-036 shows `ArrangeSteps.GivenTheRegisteredUser("…", "Europe/Kyiv", "created the custo...")` **done** when all three groups match. The same binding fails whenever the `in` or `who` part is absent.
  - `GivenTheRegisteredUsers` is done in AC-058 with all four groups, and fails in AC-057, where the `who each created …` part is absent.
  - The F002 writer already documents the rule in `StepDefinitions/F002/ArrangeSteps.cs` line 51: "The ' with ...' clause always takes part in the match; an empty capture means the step gives no fields."
- **Affected F004 bindings, as seen in this run:**
  - `ArrangeSteps.GivenTheRegisteredUser` (line 94)
  - `ArrangeSteps.GivenTheRegisteredUsers` (111)
  - `ArrangeSteps.GivenTheAnonymousVisitorRegistered` (213)
  - `ArrangeSteps.GivenCreatedAGoal` (289)
  - `ArrangeSteps.GivenDemoLoadingRan` (362)
  - `GoalWhenSteps.WhenStartsANewGoal` (19)
  - `GoalWhenSteps.WhenCreatesAGoal` (95)
  - `AccountSteps.WhenReadsTheHomeHeading` (266)
  - `AccountSteps.WhenTheVisitorRegisters` (130)
  - `CatalogSteps.ThenTheHabitIsUnpublished` (232)
- **Bindings with the same pattern that had no chance to fail yet:**
  - `GoalWhenSteps.WhenListsTheirGoals`, `WhenReadsTheGoal` and `WhenReadsTheEvaluationCalendarOf`, all prefixed with the optional capturing `At`
  - `CatalogSteps.ThenTheDetailsGiveTheGoalCount`, with `( and no goals)?`
  - `ArrangeSteps.GivenTheAdministratorUnpublished`, with `( and then published it again)?`
- **Fidelity.** Each failing step's regex matches its Gherkin text, and the conventions in `acceptance.md` (Accounts, Time, Goal use cases) are what the step bodies are meant to do. The failures come purely from how the test code binds arguments, not from what it does. So none of these is a translation fault or an implementation fault. Every failing step is a Given or When setup/action step, or the first Then in AC-058, and nothing after it ran. So this run says nothing about whether the system meets any of these 250 scenarios. The later steps' faithfulness, beyond the bindings read, could only be judged once they execute.
- **Suggested action for all items.** Fix the listed bindings so every capture group always takes part in the match, then run the tests again. Either:
  - wrap the optional text inside the group, e.g. `((?: in `[^`]+`)?)` or `((?:at `[^`]+` )?)`, and treat an empty string as "not given"; or
  - split each binding into separate bindings, one per step form.

  The "Feedback for the test writer" section has the details.

### F004/AC-001 Starting from a catalog habit without a position gives its marked suggested starting goal
- Failing step: When `…` starts a new goal from `A4.001 …` (`GoalWhenSteps.WhenStartsANewGoal`, binding error).
- What the test does: the regex has the optional capturing `At` prefix and an optional position group. Neither is present, so Reqnroll supplies 2 arguments to a 4-parameter method.
- What the AC item / scenario / spec requires: the start-new-goal use case with no position, then the listed starting values.
- Verdict and reasoning: TEST_FAULT. The binding cannot run the step as written.
- Suggested action: fix `WhenStartsANewGoal` (see feedback).

### F004/AC-002 Starting from a suggested starting goal position gives that suggestion
- Failing step: When `…` starts a new goal from `A4.002 …` with the suggested starting goal position … (`WhenStartsANewGoal`, binding error; `At` absent, so 3 arguments for 4 parameters).
- What the test does: same binding problem as AC-001.
- What the AC item / scenario / spec requires: start a new goal with a position, then that suggestion.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `WhenStartsANewGoal`.

### F004/AC-003 A catalog habit without suggested starting goals gives only Build or Reduce from its intent
- Failing step: When … starts a new goal from … (`WhenStartsANewGoal`, binding error).
- What the test does: optional groups absent, so too few arguments.
- What the AC item / scenario / spec requires: starting values with Build/Reduce from the intent and nothing else.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `WhenStartsANewGoal`.

### F004/AC-004 A suggested starting goal position the habit does not have is refused (2 rows)
- Failing step: When … starts a new goal from … with the suggested starting goal position … (`WhenStartsANewGoal`, binding error).
- What the test does: `At` absent, so too few arguments.
- What the AC item / scenario / spec requires: the refusal for an unknown position.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `WhenStartsANewGoal`.

### F004/AC-006 Creating a goal with changed values keeps the changed values
- Failing step: When `f4.ac006.user@example.com` creates a goal on `A4.006 Reading` with Reduce, count 2 `cups`, 1 time per day (`GoalWhenSteps.WhenCreatesAGoal`, binding error, "should have 3 parameters").
- What the test does: the optional capturing `At` prefix is absent, so 3 arguments for 4 parameters.
- What the AC item / scenario / spec requires: create with changed settings, then read them back.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `WhenCreatesAGoal`.

### F004/AC-007 Each field refusal of goal creation, with its message (22 rows)
- Failing step: Given the registered user `f4.ac007.user.NN@example.com` (pending: "none of them have matching scope", pointing at F002's `GivenTheRegisteredUser`).
- What the test does: F004 `ArrangeSteps.GivenTheRegisteredUser(email, timeZone, who)` matches the text, but only one group takes part. Reqnroll discards the binding, and only the out-of-scope F002 binding is left.
- What the AC item / scenario / spec requires: the "registered user" convention (register with `walking42`, no time zone, no name, sign in).
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-008 Goal creation gives the first refusal in FR-006 order (14 rows)
- Failing step: Given the registered user `…` (pending, as AC-007).
- What the test does: same as AC-007.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-009 The edges of every target range are accepted (8 rows)
- Failing step: Given the registered user `…` (pending). Same cause as AC-007. Verdict: TEST_FAULT. Action: fix `GivenTheRegisteredUser`.
- What the test does / requires / reasoning: as AC-007.
- Suggested action: as AC-007.

### F004/AC-010 The start date is today or up to 365 days later (5 rows)
- Failing step: Given the registered user `…` (pending).
- What the test does: as AC-007.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-011 A goal name has at most 60 characters after trimming (3 rows)
- Failing step: And the registered user `f4.ac011.user.NN@example.com` (pending).
- What the test does: as AC-007.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-012 A Reduce goal with an amount has 1 time per period (6 rows)
- Failing step: Given the registered user `…` (pending).
- What the test does: as AC-007.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-013 Values that do not belong to the chosen measurement or schedule are neither refused nor stored (6 rows)
- Failing step: Given the registered user `…` (pending).
- What the test does: as AC-007.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-014 A habit that is not a Published catalog habit is refused as a catalog habit (8 rows)
- Failing step: And the registered user `…`, who created the custom habit `Cold shower` (pending; the F002 `GivenTheRegisteredUserWhoCreatedTheCustomHabit` matches but is out of scope).
- What the test does: the F004 binding gets 2 arguments (email, who) for 3 parameters because the `in` group is absent, so it is discarded.
- What the AC item / scenario / spec requires: the registered user plus the custom habit convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-015 A habit that changed between starting and creating is looked up again (2 rows)
- Failing step: Given the registered user `…` (pending).
- What the test does: as AC-007.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-016 A goal without a name of its own is named after its habit and target (4 rows)
- Failing step: Given the registered user `…` (pending).
- What the test does: as AC-007.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-017 Several goals on one habit are independent
- Failing step: Given the registered user `…`, who … (`GivenTheRegisteredUser`, binding error, "should have 2 parameters").
- What the test does: the `in` group is absent, so 2 arguments for 3 parameters.
- What the AC item / scenario / spec requires: the account and goal setup.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception (23 rows)
- Failing step: Given the registered user `f4.ac018.user.NN@example.com`, who created the custom habit `Practice` (pending).
- What the test does: as AC-014.
- What the AC item / scenario / spec requires: the registered user and the custom habit, then create and read.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-019 The catalog suggestion text of the starting values follows the goal rules
- Failing step: When `f4.ac019.user@example.com` starts a new goal from `A4.019 Phone` (`WhenStartsANewGoal`, binding error, "should have 2 parameters").
- What the test does: `At` and the position group are absent, so 2 arguments for 4 parameters.
- What the AC item / scenario / spec requires: starting values with the catalog suggestion text.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `WhenStartsANewGoal`.

### F004/AC-020 Every suggested starting goal of the seed catalog can be created unchanged
- Failing step: Given the registered user `…` (pending).
- What the test does: as AC-007.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-021 Listing gives every goal with its summary, in name order
- Failing step: Given the registered user `…` (pending).
- What the test does: as AC-007/AC-014.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-022 Goals are listed by name ignoring case, then in creation order
- Failing step: Given the registered user `…` (pending).
- What the test does: as AC-007/AC-014.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-023 The next commitment is the first commitment that has not ended (5 rows)
- Failing step: Given the registered user `…` (pending).
- What the test does: as AC-007/AC-014.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-024 A goal that has not started gives its start date
- Failing step: Given the registered user `f4.ac024.user@example.com`, who created the custom habits `Water` and `Stretch` (pending; F002 `GivenTheRegisteredUserWhoCreatedTheCustomHabits` is out of scope).
- What the test does: the F004 binding is discarded because only 2 of its 3 groups take part.
- What the AC item / scenario / spec requires: the account and two custom habits.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-025 Reading a goal gives all its fields, its settings version and its attempt
- Failing step: Given the registered user `…`, who … (`GivenTheRegisteredUser`, binding error).
- What the test does: the `in` group is absent, so too few arguments.
- What the AC item / scenario / spec requires: the account and goal setup.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-026 Reading a goal that has not started gives its first version and attempt from its start date
- Failing step: Given the registered user `…` (pending).
- What the test does: as AC-007/AC-014.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-027 A user with no goals gets an empty list
- Failing step: Given the registered user `…` (pending).
- What the test does: as AC-007.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-028 A goal stays as it was created
- Failing step: Given the registered user `…`, who … (`GivenTheRegisteredUser`, binding error).
- What the test does: the `in` group is absent.
- What the AC item / scenario / spec requires: the account and goal setup.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-029 Another user's goal does not exist for them (4 rows)
- Failing step: Given the registered user `…`, who … (`GivenTheRegisteredUser`, binding error).
- What the test does: the `in` group is absent.
- What the AC item / scenario / spec requires: two users and one goal.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-030 A user's list never contains another user's goals
- Failing step: Given the registered user `…`, who … (`GivenTheRegisteredUser`, binding error).
- What the test does: as AC-029.
- What the AC item / scenario / spec requires: two users with goals.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-031 An administrator gets no goal use case (6 rows)
- Failing step: Given the registered user `…`, who … (`GivenTheRegisteredUser`, binding error).
- What the test does: the `in` group is absent.
- What the AC item / scenario / spec requires: a user's goal setup, then administrator calls.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-032 An anonymous visitor gets no goal use case (6 rows)
- Failing step: Given the registered user `…`, who … (`GivenTheRegisteredUser`, binding error).
- What the test does: as AC-031.
- What the AC item / scenario / spec requires: a user's goal setup, then anonymous calls.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-033 A custom habit is looked up only among the acting user's custom habits (5 rows)
- Failing step: Given the registered user `…` (pending).
- What the test does: as AC-007/AC-014.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-034 Catalog goal counts are the acting user's own
- Failing step: Given the registered user `…`, who … (`GivenTheRegisteredUser`, binding error).
- What the test does: the `in` group is absent.
- What the AC item / scenario / spec requires: users with goals on catalog habits.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-035 Use cases check the acting user, the role, the identifier and the fields, in this order (7 rows)
- Failing step: Given the registered user `…` (pending).
- What the test does: as AC-007.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-036 A fixed-days goal has one commitment per scheduled day, from midnight to midnight in its time zone
- Failing step: And `f4.ac036.user@example.com` created a goal on their custom habit `Yoga` with … and the start date 2026-11-18 (`ArrangeSteps.GivenCreatedAGoal`, binding error, "should have 3 parameters").
- What the test does: the optional `(?:at `([^`]+)` )?` group is absent, so 3 arguments for 4 parameters. The step before it, with all groups present, ran.
- What the AC item / scenario / spec requires: create the goal, then the calendar's 5 commitments.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenCreatedAGoal`.

### F004/AC-037 A per-day goal has one evaluation period per day
- Failing step: `…` created a goal on … (`GivenCreatedAGoal`, binding error).
- What the test does: as AC-036.
- What the AC item / scenario / spec requires: the goal setup, then periods.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenCreatedAGoal`.

### F004/AC-038 A per-week goal has one evaluation period per Monday-to-Sunday week, from its first full week (4 rows)
- Failing step: Given the registered user `…` (pending).
- What the test does: as AC-007/AC-014.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-039 A per-week goal tells when its first week starts (4 rows)
- Failing step: Given the registered user `…` (pending).
- What the test does: as AC-007/AC-014.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-040 A day or week with a clock change keeps local midnights and lasts 23 or 25 hours (6 rows)
- Failing step: `…` created a goal on … (`GivenCreatedAGoal`, binding error).
- What the test does: the `at` group is absent.
- What the AC item / scenario / spec requires: the goal setup, then the calendar.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenCreatedAGoal`.

### F004/AC-041 The days around a clock change meet at local midnight
- Failing step: `…` created a goal on … (`GivenCreatedAGoal`, binding error).
- What the test does: as AC-040.
- What the AC item / scenario / spec requires: the goal setup, then the calendar.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenCreatedAGoal`.

### F004/AC-042 A range before the start date gives an empty calendar
- Failing step: Given the registered user `…` (pending).
- What the test does: as AC-007/AC-014.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-043 The calendar range has 1 to 366 days (4 rows)
- Failing step: Given the registered user `…` (pending).
- What the test does: as AC-007/AC-014.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-045 A fixed-days goal starting on an unscheduled day begins at its next scheduled day
- Failing step: Given the registered user `…` (pending).
- What the test does: as AC-007/AC-014.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-046 Starting from a custom habit gives only Build or Reduce from its intent (2 rows)
- Failing step: Given the registered user `…`, who created the custom habit `Cold shower` with the intent … (pending).
- What the test does: as AC-014.
- What the AC item / scenario / spec requires: the account and the custom habit with the intent.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-047 A goal on a custom habit is based on it and named after it
- Failing step: Given the registered user `…`, who created the custom habit … (pending).
- What the test does: as AC-014.
- What the AC item / scenario / spec requires: the account and the custom habit.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-048 A custom habit that goals use cannot be deleted
- Failing step: Given the registered user `…`, who created the custom habit … (pending).
- What the test does: as AC-014.
- What the AC item / scenario / spec requires: the account and the custom habit.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-049 A custom habit that no goal uses is deleted
- Failing step: Given the registered user `…`, who created the custom habit … (pending).
- What the test does: as AC-014.
- What the AC item / scenario / spec requires: the account and the custom habit.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-050 Editing a custom habit renames its goals but never changes their settings
- Failing step: Given the registered user `…`, who created the custom habit … (pending).
- What the test does: as AC-014.
- What the AC item / scenario / spec requires: the account and the custom habit.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-051 Reading a custom habit gives the number of goals that use it
- Failing step: Given the registered user `…`, who created the custom habit … (pending).
- What the test does: as AC-014.
- What the AC item / scenario / spec requires: the account and the custom habit.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-052 Browsing gives a registered user their goal count per habit and the tracked count (3 rows)
- Failing step: Given the registered user `…`, who created … (`GivenTheRegisteredUser`, binding error).
- What the test does: the `in` group is absent.
- What the AC item / scenario / spec requires: a user with goals on catalog habits.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-053 "Only habits I track" returns the Published habits with the user's goals, with the other filters (3 rows)
- Failing step: And the registered user `f4.ac053.user.NN@example.com`, who created two goals on … (`GivenTheRegisteredUser`, binding error, "should have 2 parameters").
- What the test does: the `in` group is absent.
- What the AC item / scenario / spec requires: the user's goals, then the filtered browse.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-054 A habit's details give the user's goal count and their goals on it, in name order
- Failing step: Given the registered user `…`, who … (`GivenTheRegisteredUser`, binding error).
- What the test does: as AC-052.
- What the AC item / scenario / spec requires: the user's goals, then the details.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-055 Anonymous visitors and administrators get the catalog without goal information (6 rows)
- Failing step: Given the registered user `…`, who … (`GivenTheRegisteredUser`, binding error).
- What the test does: as AC-052.
- What the AC item / scenario / spec requires: the user's goals, then the browse/details calls.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-056 Browsing and details without an acting user give the slice 2 result
- Failing step: Given the registered user `…`, who … (`GivenTheRegisteredUser`, binding error).
- What the test does: as AC-052.
- What the AC item / scenario / spec requires: the user's goals, then the slice 2 calls.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-057 Unpublishing a habit that goals use needs confirmation and gives only the number of goals (3 rows)
- Failing step: And the registered users `f4.ac057.a.NN@example.com` and `f4.ac057.b.NN@example.com` (`ArrangeSteps.GivenTheRegisteredUsers`, binding error, "should have 2 parameters").
- What the test does: the optional `, who each created a goal on (…) with (…)` groups are absent, so 2 arguments for 4 parameters. The same binding ran in AC-058, where they are present.
- What the AC item / scenario / spec requires: two registered users, then their goals and the unpublish.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUsers`.

### F004/AC-058 Unpublishing with confirmation unpublishes a habit that goals use
- Failing step: Then `A4.058 Reading` is Unpublished (`CatalogSteps.ThenTheHabitIsUnpublished`, binding error, "should have 1 parameters").
- What the test does: the optional capturing `(, with no "confirmation needed" result)?` is absent, so 1 argument for 2 parameters. The Given and When steps ran, but no assertion was ever made.
- What the AC item / scenario / spec requires: the habit is Unpublished, and browsing no longer contains it.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `ThenTheHabitIsUnpublished`.

### F004/AC-060 A goal on an Unpublished habit keeps working, and no new goal can use that habit (5 rows)
- Failing step: Given the registered user `…`, who … (`GivenTheRegisteredUser`, binding error).
- What the test does: the `in` group is absent.
- What the AC item / scenario / spec requires: the user's goal setup.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-061 A habit published again is in the catalog again for goals
- Failing step: Given the registered user `…`, who … (`GivenTheRegisteredUser`, binding error).
- What the test does: as AC-060.
- What the AC item / scenario / spec requires: the user's goal setup.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-062 A renamed catalog habit renames the goals based on it
- Failing step: Given the registered user `…`, who … (`GivenTheRegisteredUser`, binding error).
- What the test does: as AC-060.
- What the AC item / scenario / spec requires: the user's goal setup.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-063 A new goal can start without a habit, but not be created without one
- Failing step: Given the registered user `…` (pending).
- What the test does: as AC-007.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-065 Demo loading run again gives existing demo accounts no goals
- Failing step: And demo loading ran (`ArrangeSteps.GivenDemoLoadingRan`, binding error, "should have 0 parameters").
- What the test does: the optional ` and its catalog result was "(…)"` group is absent, so 0 arguments for 1 parameter.
- What the AC item / scenario / spec requires: a first demo loading, then a second.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenDemoLoadingRan`.

### F004/AC-069 The demo shows a goal on an Unpublished habit, a goal with its own name and an account name
- Failing step: And demo loading ran (`GivenDemoLoadingRan`, binding error).
- What the test does: as AC-065.
- What the AC item / scenario / spec requires: demo loading, then the demo reads.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenDemoLoadingRan`.

### F004/AC-070 Registration keeps a named browser time zone, and otherwise gives the default (17 rows)
- Failing step: Given the anonymous visitor registered the email `…` with the password `walking42` and the browser time zone `…`, and signed in (`ArrangeSteps.GivenTheAnonymousVisitorRegistered`, binding error, "should have 3 parameters").
- What the test does: the optional `(?:at `([^`]+)` )?` prefix is absent, so 3 arguments for 4 parameters.
- What the AC item / scenario / spec requires: register with the given browser time zone, then read the time zone.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheAnonymousVisitorRegistered`.

### F004/AC-080 A time zone is given only to its own account
- Failing step: Given the registered user `…` in `…` (`GivenTheRegisteredUser`, binding error).
- What the test does: the `who` group is absent, so 2 arguments for 3 parameters.
- What the AC item / scenario / spec requires: two accounts.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-084 An account name is trimmed, and empty means no name (4 rows)
- Failing step: Given the anonymous visitor registered … , and signed in (`GivenTheAnonymousVisitorRegistered`, binding error).
- What the test does: as AC-070.
- What the AC item / scenario / spec requires: registration with or without a name.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheAnonymousVisitorRegistered`.

### F004/AC-085 An account name has at most 60 characters
- Failing step: Given the registered user `…` … (`GivenTheRegisteredUser`, binding error).
- What the test does: an optional group is absent.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-086 The Home greeting follows the time of day in the account's time zone (7 rows)
- Failing step: Given the anonymous visitor registered the email `f4.ac086.user.NN@example.com` with the password `walking42`, the browser time zone `Europe/Kyiv` and the name "Yuri", and signed in (`GivenTheAnonymousVisitorRegistered`, binding error).
- What the test does: as AC-070.
- What the AC item / scenario / spec requires: registration with a time zone and a name, then the Home heading at a given time.
- Verdict and reasoning: TEST_FAULT. The When step's binding `WhenReadsTheHomeHeading` has the same optional `At` group, but here the step gives a time, so it would bind.
- Suggested action: fix `GivenTheAnonymousVisitorRegistered`.

### F004/AC-087 Without a name the greeting has no name
- Failing step: Given the registered user `f4.ac087.user@example.com` (pending).
- What the test does: as AC-007. The later "reads the Home heading" step, if it has no time, would also hit the `WhenReadsTheHomeHeading` fault.
- What the AC item / scenario / spec requires: the account setup convention.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser` and `WhenReadsTheHomeHeading`.

### F004/AC-088 The Home date is today in the account's time zone, in English whatever the culture (3 rows)
- Failing step: Given the registered user `…` … (`GivenTheRegisteredUser`, binding error).
- What the test does: an optional group is absent.
- What the AC item / scenario / spec requires: the account setup.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-090 An account name is given only to its own account; administrators have their own
- Failing step: Given the anonymous visitor registered … , and signed in (`GivenTheAnonymousVisitorRegistered`, binding error).
- What the test does: as AC-070.
- What the AC item / scenario / spec requires: registration with a name.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheAnonymousVisitorRegistered`.

### F004/AC-091 An anonymous visitor gets no Home heading, no name change and no profile (1 of 3 rows)
- Failing step: When the anonymous visitor reads the Home heading (`AccountSteps.WhenReadsTheHomeHeading`, binding error, "should have 1 parameters").
- What the test does: the optional `At` prefix is absent, so 1 argument for 2 parameters.
- What the AC item / scenario / spec requires: the anonymous Home heading call, refused with "You are not signed in."
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `WhenReadsTheHomeHeading`.

### F004/AC-092 A goal keeps the values it was created with, whatever happens to the suggested starting goal
- Failing step: And the registered user `f4.ac092.user@example.com`, who started a new goal from `A4.092 Reading`, and the starting values were … (`GivenTheRegisteredUser`, binding error, "should have 2 parameters").
- What the test does: the `in` group is absent.
- What the AC item / scenario / spec requires: start, edit the suggestion, create, edit again, read.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-093 A confirmed unpublish acts even when goals were added after the count was given
- Failing step: Given the registered user `…`, who … (`GivenTheRegisteredUser`, binding error).
- What the test does: the `in` group is absent.
- What the AC item / scenario / spec requires: the user's goal setup.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `GivenTheRegisteredUser`.

### F004/AC-094 At registration, the name is checked after slice 1's checks
- Failing step: When the anonymous visitor registers the email `f4.ac094.user@example.com` with the password `short` and a name of 61 characters (`AccountSteps.WhenTheVisitorRegisters`, binding error, "should have 3 parameters").
- What the test does: the optional `, then registers the same email …` groups are absent, so 3 arguments for 5 parameters.
- What the AC item / scenario / spec requires: one registration; it does not succeed, and its result is not the name-length text.
- Verdict and reasoning: TEST_FAULT.
- Suggested action: fix `WhenTheVisitorRegisters`.

<!-- WRITER-FEEDBACK:BEGIN -->
## Feedback for the test writer

All 250 non-passed F004 scenarios fail before any use case is checked. Reqnroll gives a regex binding one argument per capture group **that took part in the match**. A capture group inside an optional part that does not match yields no argument at all, not null or an empty string. The method then has more parameters than arguments:
- If no other binding matches, the step fails with "Parameter count mismatch".
- If an out-of-scope binding also matches (the F002 `ArrangeSteps` "registered user" bindings), Reqnroll drops the F004 binding and the step is reported as undefined.

`StepDefinitions/F002/ArrangeSteps.cs` line 51 shows the working pattern: keep the group, make its content optional, and treat an empty capture as "not given".

Fix each binding below so every capture group always takes part. Either use `((?:…)?)` with an empty-string check, or split the binding into one binding per step form.

1. `ArrangeSteps.GivenTheRegisteredUser` (line 94), `^the registered user `([^`]+)`(?: in `([^`]+)`)?(?:, who (.+))?$`.
   - Steps without ` in `…``, without `, who …`, or without both, must bind.
   - The scenario requires the "registered user" convention of acceptance.md: no time zone unless ` in `Z`` is given.
   - Affected: AC-007 to AC-018, AC-020 to AC-035, AC-038, AC-039, AC-042, AC-043, AC-045 to AC-056, AC-060 to AC-063, AC-080, AC-085, AC-087, AC-088, AC-092, AC-093.
2. `ArrangeSteps.GivenTheRegisteredUsers` (line 111).
   - "the registered users `a` and `b`" without the `, who each created a goal on …` part must bind (AC-057).
3. `ArrangeSteps.GivenTheAnonymousVisitorRegistered` (line 213), optional `(?:at `([^`]+)` )?`.
   - Steps that give no time must bind (AC-070, AC-084, AC-086, AC-090).
4. `ArrangeSteps.GivenCreatedAGoal` (line 289), optional `(?:at `([^`]+)` )?`.
   - "`x` created a goal on … with …" without a time must bind (AC-036, AC-037, AC-040, AC-041).
5. `ArrangeSteps.GivenDemoLoadingRan` (line 362).
   - "demo loading ran" without ` and its catalog result was "…"` must bind (AC-065, AC-069).
6. `GoalWhenSteps.WhenStartsANewGoal` (line 19), with the `At` prefix and the optional position group.
   - Steps without a time, without a position, or without both must bind (AC-001 to AC-004, AC-019).
7. `GoalWhenSteps.WhenCreatesAGoal` (line 95), `At` prefix.
   - Steps without a time must bind (AC-006, and every other item once its Given steps run).
8. `AccountSteps.WhenReadsTheHomeHeading` (line 266), `At` prefix.
   - "the anonymous visitor reads the Home heading" with no time must bind (AC-091; likely AC-087 too).
9. `AccountSteps.WhenTheVisitorRegisters` (line 130).
   - A single registration without the `, then registers the same email …` part must bind (AC-094).
10. `CatalogSteps.ThenTheHabitIsUnpublished` (line 232), `(, with no "confirmation needed" result)?`.
    - "`X` is Unpublished" alone must bind (AC-058).
11. Same pattern, not yet reached in this run; fix these too:
    - `GoalWhenSteps.WhenListsTheirGoals`, `WhenReadsTheGoal` and `WhenReadsTheEvaluationCalendarOf` (all with the `At` prefix)
    - `CatalogSteps.ThenTheDetailsGiveTheGoalCount`, with `( and no goals)?`
    - `ArrangeSteps.GivenTheAdministratorUnpublished`, with `( and then published it again)?`
<!-- WRITER-FEEDBACK:END -->
