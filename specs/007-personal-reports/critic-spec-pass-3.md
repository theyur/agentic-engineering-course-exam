# Critic pass 3: spec for 007-personal-reports

**Verdict**: APPROVE
**Date**: 2026-11-20
**Reviewed**: specs/007-personal-reports/spec.md, specs/007-personal-reports/checklists/requirements.md (with the
working-tree changes to CONTEXT.md and docs/adr/0020-derived-figures-on-read.md)

I recomputed the rewritten material first:

- **FR-006 against slice 5 FR-014** (`specs/005-recording-and-evaluation/spec.md:571-576`): every bar kind now
  matches slice 5's progress.
  - Per-week and per-day bars take the period's progress. Their target is N for Build and Reduce yes/no, and the
    amount for Reduce count or duration.
  - The closing paragraph (lines 516–522) applies minutes only to amounts: a commitment-based count or duration
    goal, or a Reduce count or duration period. A Build count or duration period counts the records that reach the
    amount against N.
  - US4-AS7's "30, 25 and 40 minutes → 2, target 3" follows from that rule. HP-001's "Target {n}" / "Target {n}
    min" split (lines 657–660) and the Edge Case on hours (lines 444–446) agree with it.
  - The rationale for hiding a 25-minute ride from the bar is sound against SPEC §6. The spec states it, and the
    History still shows the minutes.
- **When a bar begins** (FR-006 lines 509–511, US4-AS5): a commitment-based yes/no week begins with its first
  commitment. A Tuesday/Thursday goal read on a Monday therefore has no bar for the current week, so only one
  reading remains.
- **FR-032 clock change** (lines 634–637): R − 3's grace period ends 48 elapsed hours after R − 2 00:00.
  - After a change back it ends at R − 1 23:00. That falls before the week only when R is a Monday, as stated.
  - After a change forward it ends at R 01:00, so a run between 00:00 and 01:00 finds R − 3 Pending, as stated.
  - US8-AS11 stays pinned to UTC.
- **demo5 Walking on Wednesday 2026-11-18** (k = 0 to 84 on Monday, Wednesday and Friday):
  - There are 37 commitments. k = 0 is Due and k = 2 is Pending, which leaves 35 final.
  - k = 21, 23 and 26 are Failed, and k = 44, 47 and 49 are Missed, so 29 are Met: "82% (29 of 35)".
  - The current streak is 7 (k = 19 down to 5). The longest is 15 (k = 84 down to 51).
- **demo5 Strength training**: the Monday, Tuesday and Wednesday-to-Sunday figures in FR-032 lines 638–641 are
  correct.
- **demo5 trends**: the bar counts of 14, 12 and 14 hold.
- **SC-006** (lines 831–837) now says that the smoke check confirms presence only, and "not the figures
  themselves".
- **The startup fault test**: the fault test at `tests/HT.UiTests/Operator/StartupTasksDemoTests.cs:186-200` is
  named in lines 768–772.

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 Build period-based duration goals get "N min" and a minutes rule | Resolved | FR-006 lines 516–522 limit minutes to amounts and keep slice 5's "records that reach the amount, against N" for Build count or duration periods, with the Cycling example. HP-001 lines 657–660 give "Target 3" for that goal. The Edge Case (lines 444–446), US4-AS7 (lines 275–276), Key Entities Trend (line 811), HP-010 (line 724) and the CONTEXT.md Trend entry all say "an amount, or a number of records". The partial-amount point is acknowledged ("its minutes are in the History"). One leftover wording in CONTEXT.md and the design is noted in MINOR-2. |
| MINOR-1 SC-006 says the smoke check confirms figures that HP-008 does not check | Resolved | SC-006 lines 834–837 split the measurement: the acceptance tests check the figures at fixed times, the daily refresh re-creates the data, and the smoke check confirms "only that the hosted demo has this data and shows its reports (the presence texts of HP-008), not the figures themselves". HP-008 lines 715–716 say the same. |
| MINOR-2 Smoke-check and startup-fault tests that break are not listed | Partly | Lines 758–762 now name `SmokeCheckHostTests.cs` (the check-name list and "(11 of 11)") and `RunnerBehaviourTests.cs` (exact lines and `lines[7]`). Lines 768–772 name the history-fault test and add a test for a fault in demo report loading. `tests/HT.UiTests/SmokeChecks/CoverageTests.cs` is still missing; see MINOR-1. |
| MINOR-3 The "one exception" leaves out spring-forward | Resolved | FR-032 lines 634–637 give both cases: a change back, where a Monday run finds R − 3 final before the week, and a change forward, where a run between 00:00 and 01:00 finds it Pending. Both are correct, as recomputed above, and consistent with Clarifications line 53. |
| MINOR-4 When a commitment-based yes/no week's bar "has begun" | Resolved | FR-006 lines 509–511 ("a week of a commitment-based yes/no goal begins when its first commitment begins, so on a Monday a Tuesday-and-Thursday goal has no bar for the current week yet") and US4-AS5 lines 268–269 test it. |

No pass-1 note has regressed:

- R − 3 is still unrecorded (line 625).
- "Due now" is unchanged (HP-003 line 675).
- SC-007 is still an integration test (lines 838–841), and ADR 0020 agrees.
- The tie-breaks (FR-012 lines 553–555, 566–567), the Friday boundary (Clarifications lines 49–53, US6-AS4) and the
  decisions summary (lines 846–849) are unchanged.

## Notes

### MINOR-1: `CoverageTests`, which counts the coverage entries exactly and lists the ports it checks, is not among the tests that change
- **Where**: spec.md, "Slice 4 and 5 requirements and tests this slice changes", lines 758–762: the smoke-check
  tests that change are only `SmokeCheckHostTests.cs` and `RunnerBehaviourTests.cs`. HP-008, lines 716–717:
  "Every data parameter of demo report loading's port gets its `SmokeCheckCoverage` entry (slice 3 'Coverage
  rule')". Checklist iteration 5 (requirements.md lines 57–61) says "the smoke-check … tests that change are
  listed".
- **Against**: `tests/HT.UiTests/SmokeChecks/CoverageTests.cs`:
  - line 19: `Ports = [typeof(IDemoDataStore), typeof(IDemoHistoryStore)]`, a fixed list of the ports whose
    parameters the coverage rule checks;
  - line 98: `Assert.Equal(10, SmokeCheckCoverage.Entries.Count)` in `Slice_5_adds_the_demo_history_port_with_check_6`;
  - line 97: an exact list of the `IDemoHistoryStore` entries.
- **Continues**: MINOR-2 (pass 2), which continued MINOR-8 (pass 1).
- **Reasoning**: HP-008 adds `SmokeCheckCoverage` entries for demo report loading's port. That breaks the exact
  count of 10 at line 98. If the port is `IDemoHistoryStore` itself, the exact list at line 97 breaks as well.
  - If the port is new and `Ports` is not extended, the coverage theory at lines 32–46 never enumerates it. The
    rule HP-008 cites then passes without checking the new port.
  - This is the same kind of exact-count smoke-check test the section now lists for the host and runner tests,
    and the checklist says these tests are all listed.

### MINOR-2: CONTEXT.md's Recent results entry and the TrendChart design still say the trend shows "amounts"
- **Where**:
  - `CONTEXT.md` line 66, the new Recent results entry: "NOT the trend (which shows amounts, not states)".
  - spec.md "Design changes before build", lines 785–788: the TrendChart change covers the bar kinds, titles, sub
    lines, empty bars and the weeks rule only.
  - spec.md HP-010 (lines 722–729), which says these entries are now final.
- **Against**:
  - The revised Trend definition in the same change (`CONTEXT.md` line 75: "an amount, or a number of records for
    goals counted in records") and FR-006 lines 516–522.
  - `design/components/TrendChart.md` lines 3 and 6 ("A simple bar chart of recorded amounts per day or week",
    "Bars show amounts, not verdicts").
  - Constitution I: a sharpened term updates its `CONTEXT.md` entry in the same change, and UI and design wording
    use the defined terms precisely.
- **Continues**: new. It is left over from the MAJOR-1 (pass 2) fix.
- **Reasoning**: After this revision, the bars of a yes/no goal and of a Build count or duration period are
  numbers of records, not amounts.
  - The Recent results NOT-boundary describes the trend in the words the Trend entry has just narrowed.
  - The TrendChart page, which constitution IV makes the UI source of truth, keeps "recorded amounts" for every
    bar, and the Design changes list does not include that line.
  - The glossary therefore contradicts itself on what a trend bar is, and so does the design note that the
    acceptance and UI work read.

## Summary

0 MAJOR and 2 MINOR notes. Of the 5 pass-2 notes, 4 are resolved, 1 is partly resolved and none is unresolved.

The blocking pass-2 issue is fixed. FR-006 now separates bars that are amounts, which are in minutes, from bars
that count records against N, as slice 5 FR-014 does. HP-001's label, the Edge Case, US4-AS7, Key Entities,
HP-010 and the CONTEXT.md Trend entry agree with it, and the cost to partial amounts is stated with a sound
rationale. The rewritten clock-change paragraph, SC-006, the bar-begin rule and the listed startup and smoke-check
tests check out, and the demo5 figures recompute correctly. Two MINOR points remain: one exact-count coverage test
is not listed, and the word "amounts" is left in the CONTEXT.md Recent results entry and the TrendChart design
note. Neither blocks `/speckit-ac-translate`.
