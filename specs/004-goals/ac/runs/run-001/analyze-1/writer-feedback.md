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
