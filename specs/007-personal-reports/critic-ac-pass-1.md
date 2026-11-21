# Critic pass 1: ac for 007-personal-reports

**Verdict**: REVISE
**Date**: 2026-11-20
**Reviewed**: specs/007-personal-reports/acceptance.md (no golden samples: the feature has no samples folder and acceptance.md names none)

Every expected figure in AC-001 to AC-059 was recomputed from the item's Given steps, the conventions at the top of
acceptance.md and spec.md (48-hour grace period, final states, report range, FR-001 to FR-016, FR-030 to FR-032 and
the restated slice 1 to 5 behaviour). All consistency texts, percentages, counts, streaks, recent-result ranges and
state counts, trend bars and targets, dashboard counts, Today lists and their order, grace-end instants, recent notes
and their order, and the demo figures per weekday follow from those rules. The notes below are about how items run,
what they leave undefined, and what they fail to discriminate, not about wrong arithmetic.

## Notes

### MAJOR-1: AC-039's two Examples rows register the same emails in the shared database
- **Where**: acceptance.md AC-039 (lines 649-663): `f7.ac039.owner@example.com` and `f7.ac039.other@example.com`, Examples rows 01 and 02.
- **Against**: acceptance.md conventions "Examples" (lines 27-29: "Every email of such an item contains `<n>` ... so rows never share data") and "Accounts" (lines 17-19: "the registered user `x`" means registration *succeeded*); spec.md "Test data" (line 160: one database per run, scenarios use their own accounts); ADR 0003 (one shared `HT-AC` database per run, no reset between scenarios).
- **Reasoning**: AC-039 is the only Examples item outside the reset-database items whose emails do not contain `<n>`. Row 01 registers both emails; row 02 runs as its own scenario in the same database and registers them again. Registration of an existing email does not succeed, so row 02's Given "the registered user ..." cannot hold, and the row fails in its setup instead of checking FR-014's "This goal does not exist." for an identifier that belongs to nothing. A judge would see a failure that is neither the implementation's nor the spec's fault.

### MAJOR-2: AC-059's unpublish step cannot do what the item needs under the restated rules
- **Where**: acceptance.md AC-059 (lines 902-910), Given "`f7.ac059.admin@example.com` unpublished the catalog habit `A7.059 Rowing`"; conventions "Habits" (lines 30-33: "unpublished the catalog habit `N`" is "slice 2's unpublish use case").
- **Against**: slice 4 FR-033 (specs/004-goals/spec.md): unpublishing a Published habit that at least one goal uses gives, without confirmation, "confirmation needed" and leaves the habit Published; spec.md "Slice 4 and 5 behaviour these tests use" (lines 105-109) restates only that "an administrator can unpublish a seed habit like any other" and does not restate the confirmation step; spec.md Edge Cases (line 459) and slice 4 FR-034.
- **Reasoning**: in AC-059 the user's goal already uses `A7.059 Rowing` when the administrator unpublishes it. The item's own convention defines the step as slice 2's use case with no confirmation, so the habit stays Published. Either the step is read as "succeeded" and fails, or it passes and the habit is never Unpublished. In the second case the item checks nothing about FR-034's edge case: an implementation that drops the reports of a goal on an Unpublished habit would still pass. AC-055 does not have this problem because no goal uses "Walking" there. The conventions also never define "created a goal on the catalog habit `N`" (lines 34-41 define only "on their custom habit `C`").

### MAJOR-3: AC-038 locks the Upcoming window for goals that have no current date, which the spec leaves undefined
- **Where**: acceptance.md AC-038 (lines 639-647): `Soon` (start 2026-11-23) expected as "(`Soon`, 2026-11-23, Upcoming)" and `Later` (start 2026-11-24) expected to have no entry, read at 2026-11-17T10:00:00Z.
- **Against**: spec.md FR-012 (lines 571-574: an Upcoming entry "when its first date is at most 6 days after the goal's current date") and US6-AS8 (lines 352-355); slice 4 FR-012 (specs/004-goals/spec.md, cited by spec.md line 80-82): "a goal whose start date's day has not begun has no current date". spec.md itself never defines "current date".
- **Reasoning**: `Soon` and `Later` have not started at the read time, so under the definition the spec relies on they have no current date and FR-012's 6-day test has no reference point. The item's expected Today list (given exactly, in order) assumes the window is measured from the account's "today" (2026-11-17). Other readings are plausible: no Upcoming entry without a current date, or no limit at all. An implementation that follows either of those fails AC-038 while matching the written spec, so the item fixes a rule that the spec has not decided. It also bears on the order of the whole list, because `Soon` is its last entry.

### MINOR-1: AC-020 does not discriminate FR-009
- **Where**: acceptance.md AC-020 (lines 441-449); Coverage table row FR-009 (line 89) and the Edge-cases note (lines 166-169).
- **Against**: spec.md FR-009 (lines 545-546: each result "counted with the goal settings version in effect for its date"); AC rules (examples must fail for a plausible wrong implementation).
- **Reasoning**: every date from 2026-11-01 to 2026-11-06 is recorded and the read is at 2026-11-09T10:00:00Z, far from every grace end. Evaluating every day in `UTC` (ignoring the new version) or every day in `Europe/Kyiv` gives the same six Met results, the same Pending 2026-11-07 and 2026-11-08, and the same date order. FR-009's only item would therefore pass for an implementation that ignores settings versions. The note says AC-020 checks that nothing is merged, split or reordered, but even that holds for both wrong readings here.

### MINOR-2: The Coverage table disagrees with the item headers
- **Where**: acceptance.md Coverage table (lines 80-98) and these headers: AC-005 (FR-003), AC-022 (FR-001, FR-003), AC-045 (FR-012), AC-053 (FR-012), AC-056 (FR-001), AC-059 (FR-008, FR-001).
- **Against**: internal consistency of acceptance.md.
- **Reasoning**: these items name an FR in their header that the table does not list for that FR. For example, the FR-012 row lists AC-032 to AC-038 and AC-054 but not AC-045 or AC-053, and the FR-008 row omits AC-059. Coverage can't be traced reliably from one place.

### MINOR-3: Several Then steps act, or check Given results, contrary to the Time convention
- **Where**: acceptance.md conventions "Time" (lines 12-13: a Then step "checks the results of the item's When step and runs at that step's time"); AC-046 (lines 752-753), AC-047 (lines 759-761), AC-050 (lines 797-798), AC-051 (line 807), AC-052 (line 815).
- **Against**: internal consistency of acceptance.md.
- **Reasoning**: AC-046 and AC-047 check results of demo loading, demo history loading and demo report loading, which run in the Given ("the demo run"), not in the When. AC-050, AC-051 and AC-052 perform new actions in And steps (reading an overview or a report, signing in), and AC-050 gives those steps their own time. The steps can be read as intended, but they contradict the convention every translation is told to apply.

### MINOR-4: Some spec edge cases and clarified rules are only partly covered, and the gaps are not acknowledged
- **Where**: acceptance.md Edge cases list (lines 159-179) and the AC-034 items (lines 588-602).
- **Against**: spec.md Edge Cases (lines 444-448, 460-463); Clarifications "Missed this week" (lines 49-53); FR-012 (line 562: "this week" in the account's time zone).
- **Reasoning**: (a) The edge case "a per-week goal's Pending week ... is in the Today list" is mapped to AC-049 row 01 and AC-045. AC-049 checks only consistency and streaks, and AC-045 has no Pending week, so the Today-list part has no item. (b) The clock-change edge case names "23:00 or 01:00" and "a day of 23 or 25 hours counts once". AC-057 covers only the 23:00 grace end, and its 25-hour day (2026-10-25) is still Pending at both read times. The list doesn't say that the 01:00 case and day counting are left out. (c) Every "Missed this week" item runs in `UTC`, including the clarified variant where a clock change moves Friday's grace end to Monday 01:00 or Sunday 23:00. So an implementation that computes "this week" in UTC instead of the account's time zone passes every item. The list mentions only the goal-version variant of this.

### MINOR-5: The SC-003 coverage claim is broader than the items
- **Where**: acceptance.md Success-criteria paragraph (lines 181-184).
- **Against**: spec.md SC-003 (lines 841-842: before, at and after a grace end "for each schedule kind and for Build and Reduce").
- **Reasoning**: the grace-boundary items AC-012, AC-013, AC-034 and AC-057 all use commitment-based Build goals. No item reads consistency or streaks around the grace end of a per-day period, a per-week period or a Reduce goal. AC-006, AC-007 and AC-014, which the paragraph cites for "each schedule kind and for Build and Reduce", read far from any grace end. The claim implies a boundary coverage that the items don't provide.

### MINOR-6: AC-031's goal-list check is not given as literal values
- **Where**: acceptance.md AC-031 (line 561): "each with the same name, Build or Reduce, schedule and start or next information as slice 4 FR-014 gives".
- **Against**: AC rules (every value a concrete literal); spec.md FR-016 (lines 594-596).
- **Reasoning**: the expected schedule and start or next information is delegated to another slice's rule instead of being written out, for example the start date 2026-11-20 of `Later pages`, the next commitment of `Pages` and the first week of `Yoga week`. The test writer has to derive the values, and FR-016's "unchanged" check has no fixed expectation. The item also tests two behaviours (FR-011's Due progress and order, and FR-016's unchanged list).

### MINOR-7: Some FR clauses have no item that would catch a wrong implementation
- **Where**: acceptance.md AC-025 and AC-026 (lines 491-505), AC-033 (lines 572-586), AC-046 (line 751), AC-048 and AC-054 (lines 763-856).
- **Against**: spec.md FR-006 (lines 511-519), FR-012 (lines 567-569), FR-031 (lines 612-613), FR-032 (lines 624-638), US8-AS5 (line 411).
- **Reasoning**: (a) FR-006's cap of "the last 12" weeks for commitment-based yes/no goals is never reached: AC-025 has 3 bars and AC-026 has 2. A per-day goal with N > 1 (Build count, duration or yes/no) has no trend item. (b) The Today-list fields "whether it has a note", the goal's identifier and Build or Reduce (FR-012) are never checked. (c) `demo5`'s notes "On holiday.", "Sore shoulder." and "Long train ride.", and its 35-minute and 18-minute amounts, appear in no expected value, so a demo report loading without them passes. (d) AC-046 checks only the current streak of `demo1` to `demo3`, while FR-031 and US8-AS5 say "streaks of 0".

### MINOR-8: Items depend on a Reduce yes/no commitment rule that spec.md does not restate
- **Where**: acceptance.md AC-006 row 02 (lines 252-264: `No` gives Met and `Yes` gives Failed on a Reduce yes/no commitment); AC-028 (lines 520-529).
- **Against**: spec.md "Slice 4 and 5 behaviour these tests use" (lines 97-98: restated "so that this spec alone gives the rules and texts its acceptance tests depend on"; lines 116-123 give only "yes/no: 'Yes' counts 1 and 'No' 0, against 1"); slice 5 US-AS6 (specs/005-recording-and-evaluation/spec.md lines 272-273), where the rule actually lives.
- **Reasoning**: read with SPEC §5.2's Reduce rule ("at or below the permitted amount"), spec.md's restated progress ("Yes" is 1 against 1) would make a Reduce "Yes" Met. The expected 2026-11-02 Failed in AC-006 row 02 follows only from slice 5's rule, which spec.md does not restate. The values are correct, but spec.md alone doesn't support them, against its own stated intent.

## Summary

3 MAJOR, 8 MINOR. The expected figures are arithmetically sound throughout. Every consistency, streak, state count, trend bar, Today-list order, grace-end instant, recent-note order and demo figure per weekday was recomputed and matches. Coverage of the FRs and user-story scenarios is complete on paper. Three items cannot be relied on as written. AC-039's second row fails in its own setup because it reuses the first row's emails. AC-059's unpublish step either fails or leaves the habit Published, so the item checks nothing. AC-038 fixes the Upcoming window for goals with no current date, which the spec does not define. The MINOR notes are about non-discriminating examples (FR-009, FR-006 caps, "this week" in non-UTC zones), coverage bookkeeping, Then steps that break the Time convention, and claims for SC-003 and the edge cases that go beyond what the items check.
