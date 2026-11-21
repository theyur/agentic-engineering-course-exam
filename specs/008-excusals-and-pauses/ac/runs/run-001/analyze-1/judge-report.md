# Judge report — F008 / run-001 / analyze-1

## Summary

Current feature F008: 149 scenarios, 15 passed, 1 failed, 133 pending. Regression (F001–F007): 1060 of 1060 passed, so there is nothing to judge there.

All 134 non-passed F008 scenarios, across 82 AC ids, fail for one reason in the test code. Several F008 step definitions put a capture group inside an optional group, for example `(?: in `([^`]+)`)?`. Reqnroll passes only the groups that took part in the match, so when the optional part is absent the argument count no longer equals the method's parameter count.
- Where an earlier feature has a same-text binding (F007 `ArrangeSteps`, F005 `HistorySteps`), Reqnroll reports this as "no matching step definition … none of them have matching scope". That gives 133 pending scenarios.
- Where no such binding exists, Reqnroll reports "Parameter count mismatch". That gives the 1 failed scenario, AC-079.

No scenario reached a call whose result was checked against the system.

| Feature/AC | Scope (current / regression) | Verdict | Confidence | One-line reason |
|---|---|---|---|---|
| F008/AC-001 | current | TEST_FAULT | high | Given "the registered user …, who created …" (no zone) is unbound: optional capture group in `GivenTheRegisteredUserWhoCreated` |
| F008/AC-002 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-003 (rows 01, 02) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-004 (rows 01, 02) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-005 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-006 (rows 01–03) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-007 (rows 01–03) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-008 (rows 01–03) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-009 (rows 01, 02) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-010 (rows 01, 02) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-011 (rows 01, 02) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-012 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-013 (rows 01, 02) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-014 (rows 01, 02) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-015 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-016 (rows 01, 02) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-017 | current | TEST_FAULT | high | Same unbound "who created" Given (history Then also unbound) |
| F008/AC-018 (rows 01–03) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-019 (rows 01, 02) | current | TEST_FAULT | high | Same unbound "who created" Given (history Then also unbound) |
| F008/AC-020 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-021 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-022 (rows 01, 02) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-023 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-024 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-025 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-026 (rows 01–04) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-027 (rows 01–03) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-028 (rows 01–06) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-029 (rows 01, 02) | current | TEST_FAULT | high | Same unbound "who created" Given (with "at T with the start date D") |
| F008/AC-031 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-032 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-033 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-034 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-035 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-036 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-037 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-038 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-039 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-040 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-041 (rows 01, 02) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-042 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-043 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-044 (rows 01, 02) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-045 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-046 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-047 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-048 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-049 (rows 01, 02) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-050 (rows 01, 02) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-051 (rows 01–04) | current | TEST_FAULT | high | Same unbound "who created" Given (two goals) |
| F008/AC-052 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-053 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-054 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-055 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-056 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-057 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-058 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-059 | current | TEST_FAULT | high | Same unbound "who created" Given ("records … for each date" without "except" also unbound) |
| F008/AC-060 | current | TEST_FAULT | high | Same unbound "who created" Given ("records … for each date" without "except" also unbound) |
| F008/AC-061 | current | TEST_FAULT | high | Same unbound "who created" Given ("records … for each date" without "except" also unbound) |
| F008/AC-062 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-063 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-064 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-065 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-066 | current | TEST_FAULT | high | Same unbound "who created" Given (two goals) |
| F008/AC-067 | current | TEST_FAULT | high | Same unbound "who created" Given (two goals) |
| F008/AC-068 | current | TEST_FAULT | high | Same unbound "who created" Given (two goals) |
| F008/AC-069 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-070 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-071 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-072 (rows 01–06) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-073 (rows 01–06) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-074 (rows 01–06) | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-075 | current | TEST_FAULT | high | Same unbound "who created" Given (with "at T with the start date D") |
| F008/AC-078 | current | TEST_FAULT | high | Then "the history of `G` from D to D gives …" is unbound: optional capture groups in `ThenTheHistoryGives` |
| F008/AC-079 | current | TEST_FAULT | high | Then "at T the pauses of `G` of `x` are …" fails with a parameter count mismatch: optional "read by" capture group in `ThenThePausesAre` |
| F008/AC-083 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-084 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-085 | current | TEST_FAULT | high | Same unbound "who created" Given |
| F008/AC-086 (rows 01, 02) | current | TEST_FAULT | high | Same unbound "who created" Given (with "at T with the start date D") |
| F008/AC-087 | current | TEST_FAULT | high | Same unbound "who created" Given (with "at T with the start date D") |
| F008/AC-088 | current | TEST_FAULT | high | Same unbound "who created" Given (with "at T with the start date D") |

## Details (for the user)

### Shared root cause (applies to every item below)

These F008 step definitions put a capture group inside an optional non-capturing group. All are in `tests/HT.AcceptanceTests/StepDefinitions/F008/`:

| Step definition | Pattern part | Parameters |
|---|---|---|
| `GivenSteps.GivenTheRegisteredUserWhoCreated` | `(?: in `([^`]+)`)?` | 3 |
| `GivenSteps.GivenTheUserExcused` | `(?: of `([^`]+)`)?` | 5 |
| `GivenSteps.GivenTheUserPaused` | `(?:, with the result "([^"]+)" with the first day (D) and the last day (D))?` | 9 |
| `GivenSteps.GivenTheUserRemovedTheExcusal` | `(?:, with the result "([^"]+)" and the state (\w+))?` | 5 |
| `GivenSteps.GivenTheUserRecordsForEachDate` | `(?: except (DateList))?` | 5 |
| `ResultThenSteps.ThenTheHistoryGives` | `(?:at (T) )?` and `(?:, read by `([^`]+)`,)?` | 6 |
| `ResultThenSteps.ThenThePausesAre` | `(?:at (T) )?`, `(?: of `([^`]+)`)?` and `(?:, read by `([^`]+)`,)?` | 5 |

**What goes wrong:** Reqnroll builds the step arguments only from groups that took part in the match. When a step's text leaves out the optional part, the argument count is lower than the method's parameter count, so the strict match pass drops the binding.
- Where an earlier feature has a binding whose text matches but whose scope does not (F007 `ArrangeSteps.GivenTheRegisteredUser`, `ArrangeSteps.GivenTheUserRecords`, F005 `HistorySteps.ThenTheHistory`), Reqnroll reports `MissingStepDefinitionException` ("none of them have matching scope"), and the scenario is pending.
- Where no other binding exists, Reqnroll falls back to the single F008 binding. Execution then fails with `BindingException: Parameter count mismatch`. AC-079 shows this directly: `ThenThePausesAre(String × 5) should have 4 parameters`.

**The results confirm the cause:**
- AC-030 writes the zone (`… in `<zone>`, who created …`), so all three groups match. Both of its rows passed.
- Every other scenario that starts with "who created" leaves out the zone. All 132 of them are pending at that first step.
- The F007 version of the same step avoids the problem with an outer group that always takes part: `((?: in `[^`]+`)?)`.

**Hidden faults:** `GivenTheUserExcused`, `GivenTheUserPaused` and `GivenTheUserRemovedTheExcusal` have the same defect. It does not show yet because their steps were skipped after the first failure. Once the first step is fixed, many scenarios will reach these steps and fail with "Parameter count mismatch" whenever their optional part is left out (for example "excused D without a reason" with no "of `G`").

**Translation check:** I spot-checked the failing steps against `acceptance.md` (AC-001, AC-002, AC-078, AC-079 and the "who created" Given used throughout). The Gherkin matches the reviewed items word for word. The writer report also states that it found no difference.

**Verdict for every item: TEST_FAULT, high confidence.** The scenarios are faithful, but the tests never perform the setup or the checks the scenarios describe, because their bindings do not match the steps. Nothing from the system under test was observed or judged.

**Suggested action for every item:** return the run to the test writer with the feedback below, then run the tests again.

### F008/AC-001 … AC-029, AC-031 … AC-075, AC-083 … AC-088 (80 AC ids, 132 scenario rows; names as in the writer report's coverage table)
- Failing step: the first step, `Given the registered user `…`, who created the goal `…`` (with or without "at T with the start date D", or "and the goal `…`"), which does not name a zone.
- What the test does: nothing. `GivenSteps.GivenTheRegisteredUserWhoCreated` has three parameters, but it gets two arguments because the optional zone group did not match. Reqnroll therefore treats it as non-matching and reports only the out-of-scope F007 binding.
  - In AC-017, AC-019 and others, the Then "the history of the goal from D to D gives …" is reported unbound for the same reason (`ThenTheHistoryGives`).
  - In AC-059, AC-060 and AC-061, "`x` records V for each date from D to D" without "except" is reported unbound too (`GivenTheUserRecordsForEachDate`).
- What the AC item / scenario / spec requires: register the user, sign them in and create the named goal(s) at the given or default time. The `.feature` text is the same as `acceptance.md`.
- Verdict and reasoning: TEST_FAULT (high). This is a step-binding defect in test code that keeps the test from running the scenario.
- Suggested action: writer feedback items 1–5.

### F008/AC-078 The demo excusals, pauses, states and notes of `demo6@example.com`
- Failing step: `And the history of `Drinking water, 8 glasses a day` from 2026-10-30 to 2026-11-18 gives …`. The earlier Given, When and Then steps all passed, including the Meditation history and recent notes.
- What the test does: the step text has neither "at T" nor ", read by `x`,", so two optional capture groups do not match. `ThenTheHistoryGives` (6 parameters) gets 4 arguments and is dropped. Reqnroll reports only F005's out-of-scope `HistorySteps.ThenTheHistory`. The step that follows, "the pauses of `G` are, in this order, …", would then fail through `ThenThePausesAre` for the same reason.
- What the AC item / scenario / spec requires: read the goal's history over the range and check each listed entry, then the recent notes and the ordered pauses (AC-078 = acceptance.md lines 1116–1124, unchanged in the `.feature`).
- Verdict and reasoning: TEST_FAULT (high). This is a binding defect, not a check against the system.
- Suggested action: writer feedback items 6 and 7.

### F008/AC-079 Demo pause loading leaves an existing `demo6@example.com` unchanged
- Failing step: `And at 2026-11-19T10:00:00Z the pauses of `Drinking water, 8 glasses a day` of `demo6@example.com` are, in this order, …`. The step before it, "its results are (demo6@example.com, already exists)", passed.
- What the test does: it calls `ResultThenSteps.ThenThePausesAre(time, goalPhrase, owner, reader, pauses)`. The step has no ", read by `x`,", so only 4 groups match. Reqnroll throws `BindingException: Parameter count mismatch … should have 4 parameters` before the method body runs.
- What the AC item / scenario / spec requires: read the pauses of that goal of `demo6@example.com` and check the exact ordered list, then that the goals overview lists exactly the two goals. The `.feature` text is the same as `acceptance.md`.
- Verdict and reasoning: TEST_FAULT (high). This is a binding defect in the test code.
- Suggested action: writer feedback item 7.

<!-- WRITER-FEEDBACK:BEGIN -->
## Feedback for the test writer

All the problems below have one cause. In Reqnroll, a capture group that sits inside an optional group `(?:…)?` and does not take part in the match produces no argument at all, not an empty string. The binding then has fewer arguments than parameters.
- If another feature has a binding with the same text, the step is reported as unbound ("no matching scope").
- Otherwise it fails at run time with "Parameter count mismatch".

The build cannot detect this. You can fix it in either of these ways:
- Make every capture group always take part. Put the optional part inside a capturing group that may be empty, for example `((?: in `[^`]+`)?)`. F007's `ArrangeSteps` does this for the same Given. Then parse the captured text.
- Capture the whole variable tail in one group and parse it with a private `Regex`, as `GivenTheRegisteredUserWhoCreated` already does for the goals.

Splitting each variant into its own binding also works. Each must keep exactly the meaning of the Gherkin step.

1. `GivenSteps.GivenTheRegisteredUserWhoCreated`, `^the registered user `([^`]+)`(?: in `([^`]+)`)?, who created (.+)$`. Every scenario except AC-030 leaves out "in `Z`", so this first step is unbound in 132 scenarios (AC-001 to AC-075 and AC-083 to AC-088, except AC-030). The step must register and sign in the user, with the zone only when one is written, then create the goal(s) at the written time or the default time.
2. `GivenSteps.GivenTheUserExcused`: the optional `(?: of `([^`]+)`)?` group. Steps like "at T `x` excused D without a reason" (AC-012, AC-017, AC-020, AC-037, AC-057 and others) will hit a parameter count mismatch once item 1 is fixed.
3. `GivenSteps.GivenTheUserPaused`: the optional `(?:, with the result "…" with the first day D and the last day D)?` group has three capture groups. Every "at T `x` paused … from D until D …" that leaves out the result clause will hit a parameter count mismatch.
4. `GivenSteps.GivenTheUserRemovedTheExcusal`: the optional `(?:, with the result "…" and the state …)?` group has two capture groups. Same problem for "at T `x` removed the excusal of D" without the result clause (for example AC-057).
5. `GivenSteps.GivenTheUserRecordsForEachDate`: the optional `(?: except (…))?` group. "`x` records V for each date from D to D" without "except" (AC-059, AC-060, AC-061) is unbound, because F007's same-text binding is out of scope. It must record V for every date in the range, leaving out only the dates written after "except".
6. `ResultThenSteps.ThenTheHistoryGives`: the optional `(?:at (T) )?` and `(?:, read by `([^`]+)`,)?` groups. Every history Then without "at T" or without "read by" is unbound (AC-001, AC-017, AC-019, AC-078 and many more). It must read the history of the named goal over the range, now or at T when written, as the reader when written, otherwise as the owner, and check the listed entries.
7. `ResultThenSteps.ThenThePausesAre`: the optional `(?:at (T) )?`, `(?: of `([^`]+)`)?` and `(?:, read by `([^`]+)`,)?` groups. In AC-079, "at T the pauses of `G` of `x` are, in this order, …" has no "read by" and fails with a parameter count mismatch. AC-078's "the pauses of `G` are, in this order, …" has the same problem. The step must read the pauses of that goal (of `x` when written), as the reader when written, otherwise as the owner, and check the exact ordered list.

After the fix, check the other F008 bindings for any capture group nested in an optional group. As far as I found, the ones above are the only cases in `StepDefinitions/F008/`. The `(?:, in this order,)?` parts contain no capture group and are fine.
<!-- WRITER-FEEDBACK:END -->
