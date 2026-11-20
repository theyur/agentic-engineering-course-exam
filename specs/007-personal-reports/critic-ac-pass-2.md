# Critic pass 2: ac for 007-personal-reports

**Verdict**: APPROVE
**Date**: 2026-11-20
**Reviewed**: specs/007-personal-reports/acceptance.md (no golden samples: the feature has no samples folder and acceptance.md names none)

Every expected figure in the new and rewritten items (AC-020, AC-031, AC-033, AC-034, AC-038, AC-046, AC-047,
AC-054, AC-055, AC-057, AC-059, AC-060 to AC-066) was recomputed from the item's Given steps, the conventions
and spec.md. That covers the 48-hour grace ends, the Europe/Kyiv offset (UTC+2 in November), the Europe/Berlin
changes on 2026-10-25 (01:00Z, CEST to CET, a 25-hour day) and 2026-03-29 (01:00Z, CET to CEST, a 23-hour day),
FR-011 group and name order, FR-012 Today-list order, Upcoming window and recent-note order, and the demo5 data
of FR-032 for R = 2026-11-16 and R = 2026-11-18. Every expected value follows. Checked examples include: AC-020 row 02
"66% (4 of 6)" (Nov 4 is UTC with grace end 2026-11-07T00:00Z, Nov 5 is Kyiv with grace end 2026-11-07T22:00Z, Nov 6
grace end 2026-11-08T22:00Z); AC-057 row 03 "83% (5 of 6)" (Oct 25 ends 2026-10-25T23:00Z, grace end
2026-10-27T23:00Z); AC-061 row 03 "85% (6 of 7)" (Mar 29 ends 2026-03-29T22:00Z, grace end 2026-03-31T22:00Z);
AC-038's order (the Pending tie Walk before Weekly by name, and the Upcoming tie Weekly before Soon by FR-011 group,
with Later at 7 days left out); AC-064's 12 of 13 weeks; AC-066's 14 Walking and 14 Screen time bars and the three demo5 notes
(2026-10-19 week, 2026-10-18, 2026-10-05).

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 (AC-039 rows share emails) | Resolved | AC-039 (lines 670-674) now uses `f7.ac039.owner.<n>@example.com` and `f7.ac039.other.<n>@example.com`; AC-041 (lines 698-700) does the same for the administrator. |
| MAJOR-2 (AC-059 unpublish cannot succeed) | Resolved | Habits convention (lines 35-37) defines "unpublished … with confirmation" as succeeding also when goals use the habit; spec.md lines 116-118 now restate slice 4 FR-033/FR-034; AC-059 line 926 uses the confirmed step; Goals convention (lines 39-40) defines "on the catalog habit `N`". The read at 2026-11-06T10:00Z gives 3 Met (grace ends 2026-11-04, -05, -06 00:00Z), so "100% (3 of 3)", "3 commitments" follows and fails for an implementation that drops the reports. |
| MAJOR-3 (AC-038 Upcoming window undefined) | Resolved | spec.md Clarifications "acceptance review" (lines 69-73) and FR-012 (lines 587-588) now say that a goal that has not started counts from the account's today. AC-038 tests it at the boundary (`Soon` at 6 days included, `Later` at 7 days excluded). |
| MINOR-1 (AC-020 does not discriminate FR-009) | Resolved | Rewritten AC-020 (lines 451-466): evaluating every day in UTC gives row 01 without 2026-11-06 (still Due until 2026-11-07T00:00Z) and row 02 "80% (4 of 5)". Evaluating every day in Kyiv gives row 01 "75% (3 of 4)" (Nov 4 grace end 2026-11-06T22:00Z). Both differ from the expected values. |
| MINOR-2 (Coverage table vs headers) | Resolved | Every FR an item header names is now in that FR's Coverage row (checked for all 66 headers against lines 86-104). |
| MINOR-3 (Then steps contradict Time convention) | Resolved | The Time convention (lines 12-15) now lets Then steps read, at their own time if they give one. AC-046 and AC-047 make the demo run the When step (lines 768, 775). |
| MINOR-4 (edge cases partly covered, unacknowledged) | Resolved | (a) AC-060 puts the Pending week in the Today list. (b) AC-061 covers the grace end at 01:00 and the 23-hour day, and AC-057 row 03 the 25-hour day (see MINOR-1 below). (c) AC-034 rows 03/04 run in Europe/Kyiv: row 04 gives 2 Missed this week if "this week" is computed in UTC. The clarified clock-change weekend variant is skipped, with a reason (lines 183-186). |
| MINOR-5 (SC-003 claim too broad) | Resolved | SC paragraph (lines 189-193) now claims only the boundaries the items test: before and at the grace end for commitment-based Build (AC-012, AC-013, AC-057, AC-061), per-week Build (AC-062 rows 01-03) and per-day Reduce (AC-062 rows 04-06). Each value recomputed. |
| MINOR-6 (AC-031 not literal, two behaviours) | Partly | AC-031 (lines 568-577) now tests one behaviour, and FR-016 has its own item, AC-063, which compares field by field with an earlier read. But FR-011's slice 4 entry in the overview (schedule, start or next information) is now checked nowhere. Continued in MINOR-4. |
| MINOR-7 (FR clauses not discriminated) | Partly | (a) The 12-week cap is reached by AC-064 (13 weeks), and AC-065 covers N > 1 per day. (c) AC-066 checks demo5's 35 and 18 minutes and its three notes. (d) AC-046 checks the longest streak too. (b) AC-033 checks the note and Build/Reduce fields of Today entries, but the goal identifier of a Today entry is still never checked. Continued in MINOR-4. |
| MINOR-8 (Reduce yes/no rule not restated) | Resolved | spec.md lines 122-123 restate it. |

## Notes

### MINOR-1: AC-057 row 03 does not discriminate the 25-hour day's later end
- **Where**: acceptance.md AC-057 (lines 895-910), row 03 (read 2026-10-27T23:00:00Z, expecting 2026-10-25 Met); Edge-cases list (lines 171-172: "a day of 25 hours (AC-057 row 03) … counts once"); SC paragraph (lines 190-191).
- **Against**: spec.md Edge Cases (lines 460-461: a day of 23 or 25 hours "counts once like any other day"); SC-003 (lines 856-857: just before, at and just after a grace end); AC rules (examples must fail for a plausible wrong implementation).
- **Continues**: new (it arises from AC-057's new row 03)
- **Reasoning**: in Europe/Berlin, 2026-10-25 runs from 2026-10-24T22:00Z to 2026-10-25T23:00Z, so its grace period ends at 2026-10-27T23:00Z. Row 03 reads at exactly that instant. A plausible wrong implementation ends every day 24 hours after its local midnight. It ends this day at 2026-10-25T22:00Z and its grace period at 2026-10-27T22:00Z, so it also gives Met, "83% (5 of 6)" and "1 commitment" at 23:00Z. The same bug ends no other day of AC-057 wrongly, so it passes all three rows. "Exactly once" catches only an implementation that lists the date twice. No row reads just before 2026-10-27T23:00Z, where Pending is expected. The mirrored AC-061 row 03 does catch the bug, because the 23-hour day ends earlier than 24 hours, so its correct grace end comes before the wrong one. The longer day therefore has only the "at" side of its boundary checked.

### MINOR-2: The Examples convention is contradicted by the reset-database Examples items
- **Where**: acceptance.md conventions "Examples" (lines 29-31: "Every email and catalog-habit name of such an item contains `<n>`"); AC-044 (lines 731-745), AC-048 (lines 780-792), AC-049 (lines 794-807), AC-052 (lines 826-839), AC-053 (lines 841-858).
- **Against**: internal consistency of acceptance.md.
- **Continues**: new
- **Reasoning**: these five items have `Examples:` tables, and their only emails are `demo4@example.com` or `demo5@example.com`, with no `<n>`. AC-055's catalog habit `Walking` is also a seed name, but AC-055 has no Examples table, so it is consistent. The next sentence ("An item that starts from a reset database resets it for every row") implies an exception, but the rule itself is stated without one. A translator told to report every inconsistency rather than guess can read "every email … contains `<n>`" literally and flag five items.

### MINOR-3: AC-063's When step runs at a time earlier than its last Given step
- **Where**: acceptance.md AC-063 (lines 974-981): Given at 2026-11-18T09:30:00Z (records `Yes` for 2026-11-16 on `B` and reads the overview), then "When at 2026-11-18T09:00:00Z … reads the goal list".
- **Against**: Time convention (lines 7-9: steps run in the order written, each at its own time); spec.md FR-008 (lines 555-557: figures are computed "from the records as they are at the supplied time").
- **Continues**: new
- **Reasoning**: the supplied time goes back 30 minutes between the last Given and the When, which no other item does. Under FR-008's wording, the record entered at 09:30Z does not exist yet at the When step's supplied time of 09:00Z. So the claim in the title, that the list is unchanged "by records", is checked only if the implementation ignores when a record was entered. The convention neither allows nor forbids going back in time, so the translator must guess whether this is intended.

### MINOR-4: Some FR-011 and FR-012 output fields are still not checked
- **Where**: acceptance.md AC-029 (lines 555-558), AC-031 (lines 568-577), AC-063 (lines 974-981); the dashboard-entry convention (lines 71-73); AC-033 (lines 596-602), AC-054 (lines 865-872).
- **Against**: spec.md FR-011 (lines 566-568: each overview goal carries its slice 4 list entry, "name, Build or Reduce, schedule and the start or next information"); FR-012 (lines 582-583: each Today entry has its goal's "identifier, name, Build or Reduce"); AC rules (each FR clause has a checking example).
- **Continues**: MINOR-6 and MINOR-7(b) of pass 1
- **Reasoning**: since AC-031 was narrowed, no item checks the schedule or the start or next information of an overview entry. AC-029 checks only its name and Build. AC-063 compares the slice 4 goal list with itself, not with the overview. An overview that leaves out or garbles these fields therefore passes. Likewise, the dashboard-entry notation has no slot for the goal identifier, and AC-040 checks the identifier only negatively. A Today entry that carries a wrong or missing goal identifier, which the page uses to open the RecordDialog and link the goal, passes every item.

### MINOR-5: The edge-case list misstates one gap and gives no reason for another
- **Where**: acceptance.md Edge-cases list, lines 173-175 ("A time zone change after a large move east, where two consecutive commitments overlap: not checked here") and line 187 ("An account whose time zone differs from a goal's older settings version: not checked here").
- **Against**: spec.md Edge Cases (lines 462-463, 476-477); FR-012 (lines 575-576: "this week" in the account's time zone); AC rules (each scenario is covered, or listed as not testable with a reason).
- **Continues**: new
- **Reasoning**: (a) The overlap edge case is in fact exercised. In AC-020, 2026-11-04 (UTC, 2026-11-04T00:00Z to 2026-11-05T00:00Z) and 2026-11-05 (Kyiv, from 2026-11-04T22:00Z) overlap by two hours, and row 02 counts both, in date order. The list's "not checked here" understates the coverage. (b) The edge case where the account's time zone differs from a goal's older version is testable through the API: a change of the account's time zone, as in AC-020, followed by a dashboard read near Monday 00:00. It is left out with no reason. In every AC-034 row the account and the goal share one time zone. So an implementation that takes "this week", "today" or the Upcoming window from the goal's first settings version instead of the account passes every item.

## Summary

0 MAJOR, 5 MINOR. All three MAJOR notes of pass 1 are resolved. AC-039 now keeps its rows apart with `<n>`. AC-059 unpublishes with confirmation, under a convention and a restated spec rule that make the step succeed. The Upcoming window of a goal that has not started is now decided in spec.md, and AC-038 tests it at the 6- and 7-day boundary. Every recomputed figure in the new and rewritten items matches the conventions and spec.md, including the Kyiv and Berlin offsets and the demo5 data on Monday and Wednesday runs. The remaining notes concern discrimination and bookkeeping: the 25-hour day's later end, two spots where the overview and Today entries are under-checked, a literal conflict in the Examples convention, AC-063's backwards time step, and the edge-case list. None blocks `.feature` translation, but a strict translator may raise MINOR-2 and MINOR-3.
