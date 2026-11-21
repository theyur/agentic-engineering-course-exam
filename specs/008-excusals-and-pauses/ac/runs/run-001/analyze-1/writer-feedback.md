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
