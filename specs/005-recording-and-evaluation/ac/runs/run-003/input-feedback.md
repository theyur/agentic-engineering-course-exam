## Feedback for the test writer

Every F005 binding below has an optional part (`(?:…)?`) that contains a capture group. Reqnroll leaves out a capture
group that does not take part in the match. When a step omits the optional part, the binding has fewer arguments than
the method has parameters. Reqnroll then either reports the step as undefined or raises "Parameter count mismatch".
As a result, almost every F005 scenario stops at its first Given step.

Fix this the way F002 and F004 do (see the comments in `StepDefinitions/F004/ArrangeSteps.cs` and
`StepDefinitions/F002/ArrangeSteps.cs`). Use either of these approaches:

- Make each optional part a capture group that always takes part in the match, and parse it in code. For example,
  `((?:at `[^`]+` )?)` gives an empty capture when there is no time, and `((?:, and .+)?)` does the same for an
  optional ending.
- Split each optional variant into its own binding.

Keep every method's parameter count equal to the number of capture groups that always take part. Then check the
Gherkin forms each binding has to accept, with and without the optional part.

1. **`GivenSteps.cs` line 20, `GivenTheRegisteredUserWhoCreatedTheCustomHabit`:** the optional
   `(?: (in `…`|in …))?` capture. It must bind "the registered user `u`, who created the custom habit `H`" with no
   time zone (AC-001 and most other scenarios) as well as "… in UTC, …" and "… in `America/New_York`, …".
2. **`GivenSteps.cs` line 70, `GivenTheUserCreatedAGoal`:** the optional At capture and the optional second-goal
   clause (three captures). It must bind a single-goal step with or without "at `T`" (AC-001, AC-006, AC-065,
   AC-066, AC-067) as well as the AC-029 two-goal form.
3. **`GivenSteps.cs` lines 90, 109 and 116 (`GivenTheUserCreatedTwoGoals`, `GivenTheUserRecorded`,
   `GivenTheUserSavedNotes`):** the optional At capture. Steps such as "`u` recorded 61 for 2026-11-18" (AC-014) have
   no "at `T`".
4. **`WhenSteps.cs` lines 17 and 24 (records/changes/deletes/saves, and reads):** the optional At capture. Steps such
   as "When `f5.ac001.user@example.com` records 12 for 2026-11-18" (AC-001) have no "at `T`".
5. **`DemoSteps.cs` line 29, `GivenDemoLoadingRan`:** the optional At capture. "And demo loading ran" (AC-058,
   AC-059, AC-061, AC-069) has no time.
6. **`DemoSteps.cs` line 49, `GivenDemoLoadingAndDemoHistoryLoadingRan`:** the optional `(?: \((.+)\))?` capture
   (and At). AC-056 and AC-057 have no parenthesis.
7. **`DemoSteps.cs` lines 63 and 70 (`WhenDemoHistoryLoadingRuns`, `WhenDemoLoadingAndDemoHistoryLoadingRun`):** the
   optional At capture. "When demo history loading runs" (AC-058, AC-059, AC-060) has no time.
8. **`DemoSteps.cs` line 87, `ThenItsResultsAreInThisOrder`:** the optional
   `(?:, and (no other result|sign-in …))?` capture. AC-060's Then step ends after "each for `demo4@example.com`".
9. **`DemoSteps.cs` line 118, `ThenItsResultIs`:** the optional
   `(?:, and (demo history loading run afterwards .+))?` capture. AC-061's Then step "its result is "no demo
   password configured", and no other result" has no such ending.
