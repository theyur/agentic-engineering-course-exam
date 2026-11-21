# Critic pass 2: spec for 007-personal-reports

**Verdict**: REVISE
**Date**: 2026-11-20
**Reviewed**: specs/007-personal-reports/spec.md, specs/007-personal-reports/checklists/requirements.md (with the
working-tree changes to CONTEXT.md and docs/adr/0020-derived-figures-on-read.md)

Recomputed first. The rewritten demo figures are correct:

- **demo5 Screen time** (start R − 84, R − 3 now unrecorded): 82 final results (R − 84 to R − 3). R − 30 to R − 32 are
  Failed and R − 50 and R − 3 are Missed, so 77 are Met: "93% (77 of 82)" (93.9 rounded down). The current streak is
  0, because R − 3 is the latest final result. The longest is 34 (R − 84 to R − 51; the other runs are 17 and 26).
- **US8-AS11**: with UTC as the default time zone, R − 3's grace period ends at R 00:00. That is never before the
  Monday of R's week. No other demo5 result is Missed this week on any weekday: Walking's unrecorded k = 0 to 2 are
  Due or Pending, and Strength training's week 7 is weeks old. So "Missed this week" is always 1.
- **US8-AS12**: 2026-11-18 is a Wednesday, and the start date 2026-08-26 is too.
  - Due: Screen time 18 Nov, Walking 18 Nov and Strength training 16 – 22 Nov.
  - Pending: Screen time 16 and 17 Nov, and Walking Mon 16 Nov. Strength training's week 1 became final at Wed
    00:00.
  - Missed this week: Screen time 15 Nov.
  - FR-011 order: Screen time, Walking (Pending group, by name), then Strength training.
  - The two 16 Nov Pending results both end their grace period at 19 Nov 00:00. The tie goes to goal order, which
    gives the stated list. Every goal has a Due result, so there is no Upcoming entry.
- **demo5 Strength training and Walking** for the stated weekdays are unchanged and correct.
- **US8-AS4**: demo4 has no Missed result in R's week (R − 10, R − 12 and Yoga week 3 end their grace periods
  before R − 6).
- **CONTEXT.md and ADR 0020**: CONTEXT.md now has every term of HP-010, each with a NOT boundary. ADR 0020's SC-007
  line matches the new SC-007.

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 No demo account shows a Missed result | Resolved | FR-032 (lines 611–613) leaves R − 3 unrecorded. US8-AS11 and AS12 (lines 414–421) state the Missed row and the count, and HP-008 (lines 698–699) checks a "Missed" row. Recomputed above. The demo row has no note, so its "Add note" action is shown. |
| MINOR-1 FR-006 "no value" against slice 5 progress | Resolved | FR-006 (lines 505–507) says an unrecorded commitment, day or week has no value, "unlike slice 5's progress … which is 0", and cites FR-013. US4-AS6 covers per-day and per-week. The commitment-based yes/no target now names the start date and the Upcoming commitments of the current week (lines 497–498). |
| MINOR-2 Reduce yes/no bar counts "No" or "Yes" | Resolved | FR-006 (lines 496–499) counts "Yes" for Build and Reduce, with target 0 for a commitment-based Reduce goal. The new US4-AS7 (lines 272–273) covers it. |
| MINOR-3 Trend target in minutes and in hours | Partly | Fixed for commitment-based goals and Reduce period goals. The new rule is wrong for Build period-based duration goals; see MAJOR-1. |
| MINOR-4 No tie-breaks | Resolved | FR-012 (lines 542, 553–554) breaks ties among Pending, Missed and notes by FR-011's goal order. US6-AS2 (line 324) does too. The demo4 and demo5 orders are now defined. |
| MINOR-5 Friday missing from "Missed this week" | Resolved | Clarifications (lines 50–53) include the previous Friday and exclude Thursday, and give both clock-change directions. US6-AS4 (lines 328–331) tests both sides. |
| MINOR-6 SC-006 claims more than the smoke check checks | Partly | HP-008 now calls its checks presence checks and adds the demo5 Missed row. SC-006 still says the smoke check "confirms that the hosted demo shows them". See MINOR-1. |
| MINOR-7 SC-007 not measurable on the hosted demo | Resolved | SC-007 (lines 812–815) is now an integration test against SQL Server with 366 daily results. ADR 0020 (lines 52–54) says the same. |
| MINOR-8 Breaking tests not acknowledged; log text missing | Partly | HabitComponentTests, HomeVisitorTests (method name verified at line 73), the change to US5-AS8, the three refusal lines and the refusal log line are now listed. The smoke-check tests and one startup fault test that also break are still missing. See MINOR-2. |
| MINOR-9 "Due today" counts per-week periods | Resolved | The tile is renamed "Due now" in HP-003 (lines 659–660), US6-AS9, HP-008 and Design changes (lines 768–769). |
| MINOR-10 CONTEXT.md not updated | Resolved | The `git diff` of CONTEXT.md adds Consistency, Dashboard, Demo report loading, Recent results, Report range, Streak and Trend. It extends Pending, Demo account and Demo refresh, and changes `updated_at`. HP-010 (lines 711–713) records when. |
| MINOR-11 Checklist and decisions summary predate clarify | Resolved | The checklist has iterations 3 and 4. The decisions intro (lines 820–823) now counts three roadmap decisions, matching roadmap rows 198, 199 and 220, plus D-2, D-3 and D-5: six in all, and four further clarify answers. |

No earlier note has regressed, apart from the side effect of the MINOR-3 fix described in MAJOR-1.

## Notes

### MAJOR-1: Build period-based duration goals get a trend target of "N min" and a minutes rule that does not apply to them
- **Where**: the following parts of spec.md:
  - FR-006, lines 493–494 and 502–503: for a per-week or per-day goal, the value is the period's progress and the
    target is "its N (Build …)". Lines 507–508: "Durations are in minutes, the values and the target alike (a goal
    in hours: the amount times 60)".
  - HP-001, lines 642–645: the target line reads "Target {n} min" for a duration goal, in hours too.
  - Edge Cases, lines 441–442: "A duration goal in hours: trend values and the target are in minutes".
  - CONTEXT.md, the new Trend entry: "the amounts recorded".
  - Checklist iteration 4: "FR-006 defines … minutes for the target", and every item marked as passing.
- **Against**:
  - Slice 5 FR-014 (`specs/005-recording-and-evaluation/spec.md:574-575`), restated at spec.md lines 109–111: a
    Build period's progress is "number of records that reach the amount against N".
  - Slice 4 FR-004 (slice 5 lines 168–170): such goals exist ("at least N records that each reach the amount").
  - The seed catalog offers them as suggested starting goals in `src/HT.Application/Operator/SeedCatalog.cs`:
    - Cycling "30 min, 3 times a week" (line 39);
    - Strength training "20 min, 2 times a week" (line 36);
    - Meditation "5 min, 2 times a day" (line 51).
  - Constitution I: UI labels use the defined terms precisely.
- **Continues**: MINOR-3 (pass 1). The fix introduced this.
- **Reasoning**: For a goal created from "Cycling, 30 min, 3 times a week", FR-006's per-week bullet gives values
  such as 2 (rides of at least 30 minutes) and the target 3. The same requirement's closing paragraph then says the
  values and the target of a duration goal are in minutes. HP-001 prescribes the label "Target 3 min" for that
  bar chart, which is wrong: the target is 3 rides, not 3 minutes.
  - FR-006 contradicts itself for this goal kind. The acceptance criteria drafted from it would have to choose
    between a count and minutes for the value and the target.
  - HP-001 requires a wrong label.
  - The trend also hides partial amounts for this kind: a 25-minute ride counts 0 toward the bar. That contradicts
    FR-006's own "the trend shows amounts, not verdicts" (line 509) and CONTEXT.md's "amounts recorded", and SPEC §6
    asks trends to show partial amounts. The spec does not acknowledge either point.

### MINOR-1: SC-006 still says the smoke check confirms figures that HP-008 does not check
- **Where**: spec.md SC-006 (lines 807–811): "after every deployment the smoke check confirms that the hosted demo
  shows them (the texts of HP-008)". HP-008 (lines 695–701).
- **Against**: HP-008's own checks:
  - for demo4: "Consistency" and "commitments" on Reading's page, and "Due now" on the Home;
  - for demo5: the goal names, "weeks", "Missed this week" and a "Missed" row.
- **Continues**: MINOR-6 (pass 1).
- **Reasoning**: "them" covers the following conditions, none of which HP-008 checks:
  - demo4's non-zero current streak;
  - demo4's longest streak above 10;
  - a Today list with Due and Pending rows;
  - demo5's full number of bars.

  "Due now" is a tile label shown to every user with goals, so finding it does not show that a Due row exists. The
  parenthesis narrows the claim only by implication, and the sentence still says the smoke check "confirms" all of
  them. HP-008's "the figures themselves are checked by the acceptance tests … at a fixed run time" says something
  different: the acceptance tests run on a test database, so they say nothing about the hosted demo "on any day".

### MINOR-2: Existing smoke-check and startup-fault tests that HP-008 and HP-011 break are not listed
- **Where**: spec.md, "Slice 4 and 5 requirements and tests this slice changes":
  - line 742: "Slice 4 HP-018 and slice 5 HP-008 (smoke check): extended by HP-008", with no test named;
  - lines 743–748: the startup tests "change to expect demo5's lines after demo4's" and give three refusal lines.
- **Against**: the following tests:
  - `tests/HT.UiTests/Host/SmokeCheckHostTests.cs:36-45`: an exact list of check names ending in `"demo history "
    + demo4`, and `"PASS demo history demo4@example.com (11 of 11)"`.
  - `tests/HT.UiTests/SmokeChecks/RunnerBehaviourTests.cs:128` and the `lines[7]` assertions at lines 158–246:
    exact check lines and check positions.
  - `tests/HT.UiTests/Operator/StartupTasksDemoTests.cs:189-194`: in
    `A_fault_in_demo_history_loading_is_logged_as_an_error_after_demo_loadings_lines_and_the_start_goes_on`, the
    history fault "is the last demo line".
- **Continues**: MINOR-8 (pass 1).
- **Reasoning**:
  - HP-008 adds checks for demo4 and a demo5 sign-in, which changes the check list, the "(11 of 11)" count or the
    line positions.
  - HP-011 runs demo report loading after a demo-history fault. demo5's "Demo account … created" and the lines
    after it then follow the error line, so `log.Last(...)` no longer returns it.
  - These are the same kind of exact-line tests the section now lists for the refusal case, but they are not
    named, and the checklist says "the earlier tests that change are listed".

### MINOR-3: The "one exception" to R − 3 being Missed this week leaves out the spring-forward case
- **Where**: the following parts of spec.md:
  - FR-032, lines 611–613: R − 3's "grace period ends at R 00:00", so "on every weekday a result became Missed this
    week";
  - lines 620–622: "The one exception: … clock goes back … R − 3's grace period ends at Sunday 23:00".
- **Against**: slice 5 FR-016 (lines 581–585 of its spec): exactly 48 elapsed hours, so a forward clock change
  moves the deadline to 01:00 local. The spec's own Clarifications (line 53) give the "Monday 01:00" case.
- **Continues**: new.
- **Reasoning**: Take a default time zone whose clock goes forward in the 48 hours after R − 2 00:00 (for EU and US
  rules, runs on a Monday or a Tuesday). R − 3's grace period then ends at R 01:00. A run, and a read at the run
  time, between R 00:00 and 01:00 finds R − 3 Pending, not Missed:
  - "Missed this week" is 0;
  - the figures are not "93% (77 of 82)", streak 0;
  - the "(any weekday)" claim fails.

  US8-AS11 is pinned to UTC, and the hosted demo is UTC (`docs/operations/hosted-demo.md:113`), so neither breaks.
  But the requirement states a single exception, and it is not the only one.

### MINOR-4: For a commitment-based yes/no goal it is not stated when a week's bar "has begun"
- **Where**: spec.md FR-006, line 492: "for the bars that have begun". Lines 495–499: "one bar per Monday-to-Sunday
  week that contains at least one of its commitments, the last 12 … Upcoming ones of the current week included".
  US4-AS5, lines 266–268.
- **Against**: the spec's testing rule that lists are checked by inclusion and order (line 149), and US4-AS4 and
  AS5, which pin the bar counts.
- **Continues**: new.
- **Reasoning**: Take a Tuesday/Thursday yes/no goal read on a Monday. The current week has begun (Monday 00:00),
  but each of its commitments is still Upcoming.
  - If the week's start is what counts, the current week is the 12th bar, with no value and target 2.
  - If its first commitment's start is what counts, the current week has no bar, and the 12 bars end with last
    week.

  The text supports both readings. Which 12 weeks a test expects therefore depends on the implementation.

## Summary

1 MAJOR and 4 MINOR notes. Of the 12 pass-1 notes, 9 are resolved, 3 partly and none unresolved.

The revision fixes the blocking demo-coverage gap. demo5's unrecorded R − 3 gives a Missed result this week on
every weekday in UTC, and every rewritten figure checks out: "93% (77 of 82)", streaks 0 and 34, the AS-12 counts
and the Today list order. The tie-breaks, the Friday boundary, the "Due now" label, SC-007 and the CONTEXT.md
entries are in place.

One problem remains blocking. The minutes rule added for the trend target conflicts with slice 5's progress for
Build period-based duration goals, such as the seed suggestion "Cycling, 30 min, 3 times a week". As a result,
FR-006 contradicts itself and HP-001 requires a wrong label. This should be settled before `/speckit-ac-translate`.
The MINOR notes are about precision: SC-006's smoke-check claim, the existing smoke-check and fault tests that break
without being listed, the incomplete clock-change exception, and when a weekly yes/no bar begins.
