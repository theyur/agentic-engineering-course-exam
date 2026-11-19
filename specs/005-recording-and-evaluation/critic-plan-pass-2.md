# Critic pass 2: plan for Recording and evaluation

**Verdict**: APPROVE
**Date**: 2026-11-18
**Reviewed**: plan.md, research.md, data-model.md, quickstart.md, contracts/application-api.md, contracts/host.md

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 `RecordValue.FromStored` internal | Resolved | research R-02 (lines 83-88) and data-model.md line 14 now give two public factories, `FromAnswer(YesNoAnswer)` and `FromAmount(int)`, and the row-mapping paragraph (data-model.md lines 150-152) uses them and the public `NoteText.TryCreate`. Neither needs a measurement, so `ReadAsync` needs no measurement parameter. Nothing relies on `InternalsVisibleTo` any more (`tests/HT.ArchitectureTests/AcSurfaceTests.cs`). |
| MAJOR-2 smoke check 6 does not confirm SC-005 | Resolved | R-10 (lines 375-401) and contracts/host.md lines 217-233 now check 11 texts: the 3 names, plus "Met", "Failed", "Pending", "Due today", "12 min" and "Fell asleep early." on Reading, and "Failed" and "Missed" on Yoga. I checked the row positions against FR-030 for R on every weekday. Reading's first 10 rows are R to R − 9, with R − 3 final Met and R − 6 Failed in row 7. Yoga's rows are the current week, then weeks 1 to 3, so week 2 (Failed) and week 3 (Missed) are rows 3 and 4. The Constitution Check row VI (plan.md) now states the extension over HP-008. |
| MAJOR-3 the date gate still enters `demo` | Resolved | R-11 step 1-2 and contracts/host.md lines 248-257 add a separate `gate` job with no `environment`, and `demo` gets `needs: gate` plus a job-level `if:`. A job skipped by its job-level `if:` creates no deployment against its environment, so a post-teardown run cannot recreate `demo`. That matches ADR 0018 lines 81-83. |
| MINOR-1 existing tests broken, not acknowledged | Resolved | R-13 widens the `IGoalStore` fence and renames the test. R-14 "Existing tests this slice changes" names `ApplicationDependencyTests`, `SmokeCheckHostTests`, `CoverageTests` and `NewGoalPageTests` line 193. Plan Work order steps 3, 5 and 6 place each change in its step. A small gap remains in R-14's completeness claim (MINOR-6 below). |
| MINOR-2 history read: one span or per entry | Resolved | `ReadAsync(owner, goal, from, to)` is now the same in R-05 (lines 212-217), R-07 step 2 and data-model.md line 109. Notes are keyed by entry key, and every key lies in [first entry's `FirstDate`, last entry's `LastDate`], so no note is lost. Recent notes read one span per entry (R-04 lines 168-171). |
| MINOR-3 check 6 output and names source | Resolved | The output lines, the counts `(11 of 11)` and the reasons are in R-10 lines 394-400 and host.md lines 226-233. The expected names come from `DemoHistory.GoalNames`, `PartialMinutes`, `ReadingNote` and `RecordingText` in HT.Application (data-model.md line 82). The matching scope raises a new point (MINOR-1 below). |
| MINOR-4 classes and wording not in `design/` | Resolved | The StatusChip table (host.md lines 185-192) uses only `ht-chip ht-status ht-status--{met,failed,missed,pending,due}`, which match `design/css/ht-components.css:63-72`. The NoteDialog title stays the design's "Add note · …" (`design/components/NoteDialog.html:10`). `DateTexts.WeekRange` is planned and unit-tested, and the month-end form "28 Sep – 4 Oct" matches the design's "21 Sep – 4 Oct". The planned `aria-label`s raise a new point (MINOR-3 below). |
| MINOR-5 type `Evaluation` in namespace `Evaluation` | Resolved | The class is `Evaluator` in R-01, data-model.md line 29 and plan.md's Constitution Check II and source tree. The only remaining "Evaluation.Evaluate" is in R-01's "Alternatives considered", where it explains the rejection. |
| MINOR-6 60-day schedule-disable limit | Partly | R-11 "Known limits" and plan "Risks" now name the condition: activity on or after 2026-12-02, which is correct for 2027-01-31 − 60 days. The recovery steps they promise for the Operator guide are not backed by GitHub's documentation (continued in MINOR-4 below). |
| MINOR-7 quickstart puts Missed on the first page | Resolved | quickstart.md section 3 step 2 now puts Missed (R − 10) in the 11th row, after "Show all N →", and step 4 says "After 'Show all'". |
| MINOR-8 counts and orderings | Resolved | Plan Scale/Scope says 7 components, which matches the source tree and R-12. R-03 says "three members". `GoalHistory.Entries` documents both orders, per use case (data-model.md line 57, R-07). |

No older note has regressed.

## Notes

### MINOR-1: Smoke check 6's match scope "inside the History card's element" is undefined, and its FAIL reason prints page content
- **Where**: research R-10 (lines 386-387: "matched only inside the History card's element in the prerendered HTML (R-12)"); contracts/host.md line 226 and lines 229-231 (`GET <path> gave <status>`); research R-12 (History card bullet).
- **Against**: `tools/HT.SmokeCheck/PageReader.cs`, which finds content only through class-anchored regexes (`ht-habit__name`, `ht-goalitem__name`) and strips tags with `Text()`; `design/pages/PageGoalDetail.html:34-35`, where the History card is a plain `ht-card` with no class or id of its own; `specs/003-demo-hosting/contracts/smoke-check.md` line 94 ("No … goal name … note or page content is printed").
- **Continues**: new (it follows from the MAJOR-2 fix).
- **Reasoning**:
  - R-10 cites R-12 for the History card's element, but R-12 and host.md do not say how the element is found: no id, class, `aria-labelledby` or heading anchor. They also do not say whether texts are matched in the raw HTML or in the tag-stripped, decoded text. This matters because the regex approach of `PageReader` needs a unique anchor, and a non-greedy match to `</section>` breaks on nested elements. Raw HTML also carries attribute text. An implementer has to invent the anchor (possibly a class outside `design/`), and `RunnerBehaviourTests.GoodSite()` has no exact markup to script.
  - The planned reason `GET <path> gave <status>` for a goal page prints `/goals/<id>`. That path is read from the Goals page's `href` (R-10 step 1), so it is page content. The repository is public, so the Actions log that contains it is public too.

### MINOR-2: How the goal page gets the goal's current date for the 366-day History range is not specified
- **Where**: research R-12 (GoalDetailBody "re-reads the goal and its history through `GoalService` and `RecordingService`"; History card "the last 366 days up to the goal's current date"); contracts/host.md lines 132-134; data-model.md `GoalHistory.CurrentDate`.
- **Against**: `src/HT.Application/Goals/GoalDetails.cs` (slice 4's goal view has `HasStarted` but no current date); application-api.md `GetHistoryAsync(ActingUser, GoalId, DateOnly first, DateOnly last)`; HP-001; constitution II (one supplied instant per call, Domain logic not in HT.Ui).
- **Continues**: new.
- **Reasoning**: the page must pass `first` and `last` to `GetHistoryAsync`, but the goal's current date is returned only inside a `GoalHistory`, that is after a call that already needed the range (or from `GetRecordableAsync`). None of the artifacts says which call supplies it, or what the History card shows for a goal with no current date (`HasStarted` false). A page that chains two calls reads the clock twice. Around midnight, "Record" (from `GetRecordableAsync`) and the History rows can then come from different current dates. Working the date out in HT.Ui instead would put timeline logic in the UI.

### MINOR-3: The planned record-button `aria-label` drops the visible value, and the other repeated row actions stay identical
- **Where**: contracts/host.md lines 137-140 ("Each button's `aria-label` is 'Edit record {i} of {date}'"); research R-12 "Design fidelity of the new texts", third bullet; host.md lines 143-144 (row actions "Add record", "Edit" and "Add note").
- **Against**: WCAG 2.1 SC 2.5.3 "Label in Name" (a control's accessible name must contain its visible text); constitution IV (accessible UI); spec SC-007.
- **Continues**: new (it follows from the MINOR-4 fix).
- **Reasoning**:
  - Each record button shows its value ("12 min", "Yes"). The planned `aria-label` replaces that with "Edit record 1 of Tue 17 Nov", so a screen reader no longer announces the value, and a speech-input user who says "click 12 min" cannot reach the button.
  - The fix of pass 1 made only the record buttons unique. "Add record", "Edit" and "Add note" are repeated once per History row, up to 10 or more times, with the same accessible name. That is the same ambiguity the unique label was meant to remove.

### MINOR-4: The Operator guide's recovery from the 60-day disable relies on claims that GitHub's documentation does not support, and the job name still reads `inputs.mode`
- **Where**: research R-11 "Known limits", second bullet ("GitHub warns by email before it turns a schedule off"; "'Enable workflow' in the Actions tab, or any push, turns it on again"); contracts/host.md line 264-265; R-11 Decision step 2 ("Every step of `demo` reads `env.MODE`").
- **Against**:
  - GitHub Docs, "Disabling and enabling a workflow": scheduled workflows in a public repository are disabled after 60 days with no activity. The page documents re-enabling only by hand ("Enable workflow", `gh workflow enable`). It mentions neither an email warning nor a push re-enabling a disabled workflow. `gh repo view` shows the repository is PUBLIC.
  - `.github/workflows/deploy-demo.yml:47`: `name: Demo (${{ inputs.mode }})`, a job-level field, not a step.
- **Continues**: MINOR-6 of pass 1.
- **Reasoning**:
  - If the Operator guide says that a push turns the schedule back on, an operator who pushes after the schedule was turned off believes it runs, and HP-009's daily data silently stops. The email warning is also stated as fact without a source.
  - The `demo` job's `name` reads `inputs.mode`, which is empty on a scheduled run, so the run shows "Demo ()". A job name cannot read `env`, so "every step reads `env.MODE`" does not cover it.

### MINOR-5: `default(RecordValue)` breaks the "exactly one is set" invariant
- **Where**: data-model.md line 14 (`RecordValue`: "`readonly record struct` … exactly one is set. Made only by `TryCreate` …") and "Invariants" (lines 40-41).
- **Against**: C# semantics: every struct, `record struct` included, has an accessible parameterless constructor and a `default` value. Public signatures that take `RecordValue`: `DemoRecord(DateOnly, RecordValue)`, `IRecordStore.AddAsync(…, RecordValue value, …)` and `ChangeAsync`; and `TryCreate`'s `out RecordValue` on failure.
- **Continues**: new.
- **Reasoning**: "Made only by `TryCreate`" (and the two factories) cannot hold for a struct. `default(RecordValue)` and `new RecordValue()` have neither an answer nor an amount, and the public port and demo types accept them. Such a value reaching `RecordStore` writes a row that the mapping later turns into a fault on every read of that goal (data-model.md lines 136-138). The invariant the unit tests are meant to pin is therefore not a type invariant, as R-02's rationale claims ("keeps … a type invariant").

### MINOR-6: Stale gate and an incomplete "every existing test" list
- **Where**:
  - plan.md "Gates before the next step" ("`ac_lock.py verify sources` reports spec drift until the owner runs `ac_lock.py confirm-spec sources`");
  - plan.md Work order step 6 ("Research R-14 lists every existing test this slice changes") and research R-14 "Existing tests this slice changes".
- **Against**:
  - `specs/005-recording-and-evaluation/ac/sources.lock`: the recorded spec hash `9b12232e…ebd6` equals the current `sha256` of `spec.md`, and `acceptance.md` and the `.feature` file match their locks. So the spec edit is already confirmed.
  - `tests/HT.UiTests/SmokeChecks/RunnerBehaviourTests.cs:71-147` (`GoodSite()` and the exact line positions such as `lines[6]`), and `SmokeCheckHostTests`' rule-breaking-password case, which asserts which lines fail.
- **Continues**: new (MINOR-1 of pass 1 is otherwise resolved).
- **Reasoning**:
  - The plan tells the owner that a gate is still open when it is not. That can lead to an unneeded `confirm-spec` run, or to confusion before `/speckit-tasks`.
  - R-14's list, which step 6 calls complete, leaves out `RunnerBehaviourTests` (named only in R-10 and plan step 5). It also does not name `SmokeCheckHostTests`' failing-password case. `/speckit-tasks` takes its task list from R-14, so a test that check 6 breaks can be missed.

## Summary

0 MAJOR and 6 MINOR notes. The three blocking points of pass 1 are resolved:

- the persistence mapping now uses public, measurement-free factories that the AC-surface rules allow;
- smoke check 6 confirms every SC-005 item, and the row positions hold for R on every weekday;
- the date gate is a separate job without an environment, so a post-teardown scheduled run cannot enter or recreate `demo`.

Pass 1's minor notes are resolved except the 60-day limit, which is only partly resolved. The remaining notes are detail-level gaps to settle before or during `/speckit-tasks`:

- how check 6 finds the History card, and the goal id its FAIL reason prints;
- where the page gets the goal's current date;
- an accessibility regression in the new `aria-label`s;
- unsupported recovery claims in the Operator guide, and the job name;
- the struct default of `RecordValue`;
- a stale gate and a slightly incomplete list of affected tests.

None of them blocks the next step.
