# Judge report — F005 / run-002 / analyze-1

Results: current feature F005 had 275 scenarios: 8 passed, 6 failed, 261 pending. Regression (F001 to F004) had
652 scenarios and all passed. All 267 non-passed F005 scenarios have the same root cause, a defect in the F005 step
definition code, so each one is a `TEST_FAULT`. In every one of them the step that broke is a Given or a When step
that arranges or acts. No Then step ran, so these results say nothing about how the system behaves.

## Summary

| Feature/AC | Scope (current / regression) | Verdict | Confidence | One-line reason |
|---|---|---|---|---|
| F005/AC-001 to F005/AC-050 (all 206 rows) | current | TEST_FAULT | high | The first Given step ("the registered user `u`, who created the custom habit `H`" with no time zone, or "… created a goal on … with … and the start date D" with no second goal) does not bind: an optional capture group in the F005 regex does not take part in the match, so Reqnroll treats the step as undefined |
| F005/AC-056 | current | TEST_FAULT | high | "at `T` demo loading and then demo history loading ran" has no "(…)" part, so the binding's optional capture is missing and Reqnroll raises a parameter count mismatch |
| F005/AC-057 | current | TEST_FAULT | high | Same binding error as AC-056, in the same Given step |
| F005/AC-058 | current | TEST_FAULT | high | "demo loading ran" (no "at `T`") does not bind, because the optional At capture is missing |
| F005/AC-059 | current | TEST_FAULT | high | "demo loading ran" does not bind (same defect) |
| F005/AC-060 (rows 01, 02, 03) | current | TEST_FAULT | high | The When step "demo history loading runs" (no "at `T`") raises a parameter count mismatch in `WhenDemoHistoryLoadingRuns(string)` |
| F005/AC-061 (rows 01, 02, 03) | current | TEST_FAULT | high | "demo loading ran" does not bind (same defect) |
| F005/AC-063 | current | TEST_FAULT | high | The first Given step, "the registered user …, who created the custom habit `Reading`", does not bind (same defect) |
| F005/AC-064 (all 11 rows) | current | TEST_FAULT | high | The first Given step does not bind (same defect) |
| F005/AC-065 | current | TEST_FAULT | high | The "at `T` … created a goal on the catalog habit …" step has no second goal, and `GivenTheUserCreatedAGoal` raises a parameter count mismatch |
| F005/AC-066 (all 33 rows) | current | TEST_FAULT | high | The first Given step does not bind (same defect) |
| F005/AC-067 | current | TEST_FAULT | high | The "at `T` … created a goal …" step has no second goal, so the binding does not match (same defect) |
| F005/AC-069 (rows 01 to 04) | current | TEST_FAULT | high | "demo loading ran" does not bind (same defect) |

There are no regression failures: all 652 F001 to F004 scenarios passed.

## Details (for the user)

### Common root cause (applies to every row below)

The F005 step definitions are regular expressions that contain optional parts with capture groups inside them.
The main ones:

- `At = "(?:at `([^`]+)` )?"`, used by Given, When and Demo bindings
- `(?: (in `[^`]+`|in [A-Za-z_/]+))?` in the registered-user step
- `(?:, and then a second goal on (…) with (.+?) and the start date (…))?` in the goal-creation step
- `(?: \((.+)\))?` in the demo-history Given step

Reqnroll leaves out a capture group that does not take part in the match. When the step text omits the optional
part, the binding produces fewer arguments than the method has parameters. Depending on which other bindings also
match the text, Reqnroll then does one of two things:

- It reports the step as undefined ("there are matching step definitions, but none of them have matching scope":
  only the F002 and F004 `ArrangeSteps` are listed, not the F005 binding). This caused 261 pending scenarios.
- It raises `BindingException: Parameter count mismatch! … should have N parameters`. This caused 6 failed
  scenarios.

The evidence is consistent. The same step text binds when the optional part is present:

- AC-006 row 04 "the registered user … in UTC, who created …" ran
  `GivenSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit`.
- AC-051 to AC-055 and AC-068, whose demo steps carry "at `T`" and "(R is …)", passed.

The earlier features' step code works around this on purpose. The F004 `ArrangeSteps` comment says the "at `T` "
prefix group "always takes part in the match; an empty capture means the step gives no time". The F002
`ArrangeSteps` has a similar comment. The F005 writer did not follow that convention.

The following are not the cause:

- **Translation.** I spot-checked AC-056, AC-057, AC-060, AC-065 and AC-066 against `acceptance.md`, and their
  Gherkin matches the reviewed items word for word.
- **The implementation.** No step that observes the system ran in any non-passed scenario.

The Then steps of these scenarios were never run, so further faults may show up once the bindings are fixed.

### F005/AC-001 to F005/AC-050, AC-063, AC-064, AC-066, AC-067 (recording, evaluation, notes, history, access)

- **Failing step:** the first Given step.
  - For most rows: "Given the registered user `f5.acNNN.….@example.com`, who created the custom habit `H`" (no
    time zone).
  - For rows that give a time zone (for example AC-006 row 04, AC-067): the next step, "… created a goal on … with
    … and the start date D" (no second goal).
  - Both are reported as undefined.
- **What the test does:**
  - `GivenSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit` (GivenSteps.cs line 20) binds a regex whose
    optional "in zone" group holds a capture.
  - `GivenSteps.GivenTheUserCreatedAGoal` (line 70) binds a regex with the optional At capture and an optional
    second-goal clause of three captures.
  - When those parts are absent from the step, the binding does not apply.
- **What the AC item / scenario / spec requires:** these Given steps, as `acceptance.md` writes them, with no time
  zone, no "at `T`" and no second goal. The scenarios are faithful translations.
- **Verdict and reasoning:** `TEST_FAULT`. The step definitions cannot bind the scenarios' own step text, which is
  a bug in the test code.
- **Suggested action:** the test writer fixes the F005 bindings (see the feedback section) and the tests are run
  again.

### F005/AC-056 The demo goals' recent notes give the demo notes with their states

- **Failing step:** "And at `2026-11-18T10:00:00Z` demo loading and then demo history loading ran" (binding error:
  parameter count mismatch on `DemoSteps.GivenDemoLoadingAndDemoHistoryLoadingRan(String, String)`).
- **What the test does:** the regex `^(?:at `…` )?demo loading and then demo history loading ran(?: \((.+)\))?$`
  has an optional capture for the parenthesis, which this step does not have.
- **What the AC item / scenario / spec requires:** AC-056's Given step, exactly as written, with no parenthesis.
- **Verdict and reasoning:** `TEST_FAULT`. The binding cannot run the step text it is meant for.
- **Suggested action:** fix the binding and run the tests again.

### F005/AC-057 Demo history loading run again leaves the existing demo account with history unchanged

- **Failing step:** the same Given step and the same binding error as AC-056.
- **What the test does:** the same as AC-056.
- **What the AC item / scenario / spec requires:** AC-057's Given step, with no parenthesis.
- **Verdict and reasoning:** `TEST_FAULT`.
- **Suggested action:** fix the binding and run the tests again.

### F005/AC-058, AC-059, AC-061 (3 rows), AC-069 (4 rows) (demo history loading, account and password cases)

- **Failing step:** "And demo loading ran", reported as undefined. Only F004's `ArrangeSteps.GivenDemoLoadingRan`
  matches the text, and it is outside the scope.
- **What the test does:** `DemoSteps.GivenDemoLoadingRan(string time)` binds `^(?:at `([^`]+)` )?demo loading
  ran$`. Without "at `T`", the time capture is missing.
- **What the AC item / scenario / spec requires:** "demo loading ran" with no time.
- **Verdict and reasoning:** `TEST_FAULT`.
- **Suggested action:** fix the binding and run the tests again.

### F005/AC-060 (rows 01, 02, 03) A demo goal is left out when its catalog habit is not Published, and the results stay the same

- **Failing step:** "When demo history loading runs" (binding error: parameter count mismatch on
  `DemoSteps.WhenDemoHistoryLoadingRuns(String)`).
- **What the test does:** the binding is `^(?:at `([^`]+)` )?demo history loading runs$`, and this step has no
  "at `T`".
- **What the AC item / scenario / spec requires:** "When demo history loading runs", exactly as written.
- **Verdict and reasoning:** `TEST_FAULT`.
- **Further fault in the same scenario:** its Then step, "its results are, in this order, …, each for
  `demo4@example.com`", has no ", and …" ending. The binding at DemoSteps.cs line 87 has an optional capture for
  that ending, so the Then step will fail the same way next.
- **Suggested action:** fix both bindings and run the tests again.

### F005/AC-065 A goal whose catalog habit is Unpublished is still recorded and given its states

- **Failing step:** "And at `2026-11-16T08:00:00Z` `f5.ac065.user@example.com` created a goal on the catalog habit
  `A5.065 Run` with …" (binding error: parameter count mismatch on `GivenSteps.GivenTheUserCreatedAGoal`, which has
  8 parameters while the step gives 5 arguments).
- **What the test does:** the optional second-goal clause holds three captures, and this step has no second goal.
- **What the AC item / scenario / spec requires:** AC-065's single-goal Given step.
- **Verdict and reasoning:** `TEST_FAULT`.
- **Suggested action:** fix the binding and run the tests again.

<!-- WRITER-FEEDBACK:BEGIN -->
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
<!-- WRITER-FEEDBACK:END -->
