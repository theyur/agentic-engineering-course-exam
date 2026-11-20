# Judge report — F005 / run-004 / analyze-1

## Summary
| Feature/AC | Scope (current / regression) | Verdict | Confidence | One-line reason |
|---|---|---|---|---|
| F005/AC-010 (rows 02-07) | current | TEST_FAULT | high | The actor cell's email contains `<n>`. acceptance.md defines it as the row's `n`, but the test looks the email up with `<n>` still in it, so the call never reaches the system. |
| F005/AC-043 (rows 01-05) | current | TEST_FAULT | high | Same cause: the actor cell's `<n>` is not resolved to the row's `n` before the account lookup. |
| F005/AC-047 (rows 01-07) | current | TEST_FAULT | high | Same cause: the goal cell "the goal of `f5.ac047.owner.<n>@example.com`" is not resolved to the row's `n` before the goal lookup. |

Regression: 652 of 652 passed. No regression verdicts.

## Details (for the user)

### F005/AC-010 Recording checks the acting user, the goal, the date, the value and the note in that order (rows 02, 03, 04, 05, 06, 07)
- Failing step: `When <actor> records <value> for <date> with the note <note> on the goal of ...`. Neither the Then step nor "nothing is recorded" ran.
- What the test does: Reqnroll replaces `<actor>` with the cell text, for example `` `f5.ac010.owner.<n>@example.com` ``. It does not expand a placeholder inside a cell value, so the literal `<n>` stays. `WhenSteps.WhenTheActorWrites` calls `F005Actions.RunAsync`, then `F005Arrange.ActorAsync` (tests/HT.AcceptanceTests/Support/F005/F005Arrange.cs, lines 134-150). That code takes the backticked email as written and passes it to `AccountAsync` (lines 121-131). No account `...<n>@...` was signed in by the Given steps, so the test throws its own `InvalidOperationException` before any use case is called. The system was never exercised.
- What the AC item / scenario / spec requires: acceptance.md, Conventions, "Examples" (lines 31-32): "Every email of such an item contains `<n>`, the row's value in the column `n`, so rows never share data." The `.feature` (lines 120-138) copies the item and its table word for word, so the translation is faithful. In row 06, for example, the actor is `f5.ac010.owner.06@example.com`, the account the Given steps registered.
- Verdict and reasoning: TEST_FAULT. The step definitions do not apply a convention that acceptance.md defines and that the Gherkin carries faithfully. Row 01 (the anonymous visitor, no email in the cell) passes. That fits a lookup fault rather than a fault in the system.
- Suggested action: the test writer should resolve `<n>` in emails taken from example cells (feedback below), then re-run.

### F005/AC-043 The history's range has 1 to 366 days and is checked after the goal (rows 01, 02, 03, 04, 05)
- Failing step: `When <actor> reads the history of the goal of `f5.ac043.owner.NN@example.com` from <first> to <last>`.
- What the test does: the `<actor>` cell becomes the literal `` `f5.ac043.owner.<n>@example.com` `` (or `` `f5.ac043.other.<n>@example.com` `` in row 05). `F005Actions.ReadAsync` calls `F005Arrange.ActorAsync`, which finds no such account and throws before the history use case is called.
- What the AC item / scenario / spec requires: the same "Examples" convention. The actor is the row's own owner or other account, for example `f5.ac043.owner.02@example.com`. The `.feature` (lines 602-617) matches acceptance.md (lines 861-877).
- Verdict and reasoning: TEST_FAULT, for the same cause as AC-010. Row 06 (the anonymous visitor) passes.
- Suggested action: as for AC-010.

### F005/AC-047 Another registered user gets "This goal does not exist." from every use case of this slice (rows 01, 02, 03, 04, 05, 06, 07)
- Failing step: `When `f5.ac047.other.NN@example.com` calls <use case> with <goal>`.
- What the test does: the `<goal>` cell becomes the literal "the goal of `` `f5.ac047.owner.<n>@example.com` ``". `F005Actions.CallAsync` calls `F005Arrange.GoalPhraseAsync` (lines 278-290), which matches "the goal of `x`" and calls `GoalOf(x)` (lines 257-259) with `x` still holding `<n>`. No goal has that owner, so the test throws before the use case is called.
- What the AC item / scenario / spec requires: the same "Examples" convention. The goal is the one `f5.ac047.owner.NN@example.com` created in the row's Given step. The `.feature` (lines 667-687) matches acceptance.md (lines 934-956).
- Verdict and reasoning: TEST_FAULT. Rows 08 and 09 ("a goal identifier that belongs to no goal", no email in the cell) pass, so the fault is limited to emails inside cells.
- Suggested action: as for AC-010.

Note: a translator could instead have written each row's email out in full (`.06@`, and so on). But the `.feature` faithfully carries a convention that acceptance.md defines, and the test writer can fix this without touching the feature, so it is a TEST_FAULT, not a TRANSLATION_FAULT.

<!-- WRITER-FEEDBACK:BEGIN -->
## Feedback for the test writer

1. **F005/AC-010 rows 02-07, F005/AC-043 rows 01-05 and F005/AC-047 rows 01-07: `<n>` inside an Examples cell is not resolved.**
   - What goes wrong: Reqnroll does not expand a placeholder inside a cell value. So when `<actor>` or `<goal>` is substituted, the step gets the literal text `<n>` inside the email, for example `` `f5.ac010.owner.<n>@example.com` `` or "the goal of `` `f5.ac047.owner.<n>@example.com` ``".
   - Where: `F005Arrange.ActorAsync` (Support/F005/F005Arrange.cs) and `F005Arrange.GoalPhraseAsync`, through `GoalOf`, use that email as written. They are reached from `F005Actions.RunAsync`, `F005Actions.ReadAsync` and `F005Actions.CallAsync`. The lookup then finds no account or goal from the Given steps, and the use case is never called.
   - What is required: acceptance.md, Conventions, "Examples", says every email in such an item contains `<n>`, meaning the row's value in the column `n`. Replace `<n>` in emails coming from step arguments with the current row's `n` value before any lookup. The row's `n` is available from the scenario's example arguments (`ScenarioContext.ScenarioInfo.Arguments["n"]`). Do this in one central place, such as a step argument transformation or `ActorAsync`/`GoalPhraseAsync`, so every step that takes an actor or goal phrase from a cell is covered.
   - Afterwards: confirm that "nothing is recorded" (AC-010) and the owner's history read (AC-047) refer to the row's own accounts.
<!-- WRITER-FEEDBACK:END -->
