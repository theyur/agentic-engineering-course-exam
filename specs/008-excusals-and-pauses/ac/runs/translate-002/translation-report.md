# Translation report — F008 / translate-002

## Build

success, 1 attempt, no remaining errors.

Feature file: `tests/HT.AcceptanceTests/Features/F008/excusals-and-pauses.feature`.

## Items

| AC id | Status (translated / issue) | Scenario name |
|---|---|---|
| AC-001 | translated | Excusing a Pending commitment with a reason |
| AC-002 | translated | Excusing a Due commitment without a reason |
| AC-003 | translated | The reason is trimmed, and a reason that is empty after trimming is none |
| AC-004 | translated | An excused commitment stays Excused after its grace period would have ended |
| AC-005 | translated | An excusal applies at every supplied time, also one earlier than the excusing call |
| AC-006 | translated | A final commitment cannot be excused |
| AC-007 | translated | Excusing just before, at and just after the end of the grace period |
| AC-008 | translated | An Upcoming commitment cannot be excused, also with a reason that is too long |
| AC-009 | translated | A commitment of a period-based goal cannot be excused |
| AC-010 | translated | A date that is not a commitment of the goal cannot be excused |
| AC-011 | translated | A date in a pause cannot be excused, also when it has not begun |
| AC-012 | translated | A commitment excused before a pause covered it is refused as paused, not as already excused |
| AC-013 | translated | A commitment that is already excused cannot be excused again |
| AC-014 | translated | A commitment that has a record cannot be excused |
| AC-015 | translated | After its record is deleted, a commitment can be excused |
| AC-016 | translated | A reason has at most 200 characters |
| AC-017 | translated | An excusal can be removed while the grace period is open |
| AC-018 | translated | Removing an excusal just before, at and just after the end of the grace period |
| AC-019 | translated | Removing an excusal that does not exist |
| AC-020 | translated | A commitment whose excusal is removed is evaluated as if it had never been excused |
| AC-021 | translated | An excused commitment cannot be recorded |
| AC-022 | translated | The record checks for a pause and an excusal come after the date check, the pause first |
| AC-023 | translated | Pausing a goal from today |
| AC-024 | translated | A pause cannot start before today |
| AC-025 | translated | The last day cannot be before the first day |
| AC-026 | translated | A pause cannot share a date with another pause of the goal, but may touch it |
| AC-027 | translated | A pause needs a first and a last day |
| AC-028 | translated | The pause checks come in their order |
| AC-029 | translated | For a goal that has not started, "today" is the account's today |
| AC-030 | translated | The first day is compared with the current date on the goal's own timeline |
| AC-031 | translated | Pausing a commitment-based goal makes its commitments in the range Paused |
| AC-032 | translated | Resuming a commitment-based goal makes today's commitment Due again |
| AC-033 | translated | A per-week period is Paused as a whole while one of its dates is paused |
| AC-034 | translated | A date of a Paused week that is not itself paused cannot be recorded |
| AC-035 | translated | Resuming a per-week goal in a new week leaves the earlier partly paused week Paused |
| AC-036 | translated | One paused day exempts its whole week, also when its records reach the target |
| AC-037 | translated | An excusal under a pause is kept and shows again when the pause is removed |
| AC-038 | translated | A pause never changes a result that was Pending or final when it was set |
| AC-039 | translated | A Due entry whose records already exceed a Reduce limit becomes Paused and keeps its records |
| AC-040 | translated | A date in a pause cannot be recorded |
| AC-041 | translated | A record kept on a Paused date can be changed or deleted, and the date stays Paused |
| AC-042 | translated | Resuming during a pause ends it on the day before today |
| AC-043 | translated | Resuming on a pause's first day removes the pause |
| AC-044 | translated | A goal without a current pause cannot be resumed |
| AC-045 | translated | Resuming in the middle of a week leaves that week Paused, while the goal is not paused |
| AC-046 | translated | A pause later in the current week pauses that week, while the goal is not paused and is counted |
| AC-047 | translated | Cancelling the upcoming pause that paused the current week makes the week Due again |
| AC-048 | translated | An upcoming pause can be cancelled |
| AC-049 | translated | A current or past pause cannot be cancelled |
| AC-050 | translated | Cancelling just before and on a pause's first day |
| AC-051 | translated | A pause is found only through its own goal |
| AC-052 | translated | The pauses of a goal are listed with the latest first day first |
| AC-053 | translated | Consistency leaves Excused and Paused results out |
| AC-054 | translated | Excused and Paused results neither extend nor reset a streak |
| AC-055 | translated | Recent results include Excused and Paused results and count each state |
| AC-056 | translated | The trend marks Excused and Paused bars, which have no value |
| AC-057 | translated | Removing an excusal and recording the commitment late gives the figures of an on-time record |
| AC-058 | translated | A goal whose every ended result is exempt has "N/A" and streaks of 0 |
| AC-059 | translated | A Paused week of a per-week goal is not counted and does not break the streak |
| AC-060 | translated | The trend of a per-week goal marks a Paused week |
| AC-061 | translated | The trend of a per-day goal marks its Paused days |
| AC-062 | translated | A yes/no week's target and value leave a Paused commitment out |
| AC-063 | translated | A yes/no week whose only record is on a Paused commitment has no value |
| AC-064 | translated | A yes/no week whose commitments are all Excused is marked Excused |
| AC-065 | translated | A yes/no week whose commitments are all Excused or Paused, at least one Paused, is marked Paused |
| AC-066 | translated | The goals overview says which goal is paused and until when |
| AC-067 | translated | The dashboard expects nothing of a paused goal and does not count it |
| AC-068 | translated | An excused commitment leaves the Pending count and the Today list |
| AC-069 | translated | The Upcoming entry skips a Paused commitment |
| AC-070 | translated | An Excused commitment whose grace period would have ended this week is not Missed |
| AC-071 | translated | Recent notes show Excused and Paused as the state |
| AC-072 | translated | Another registered user cannot use any use case of this slice on someone else's goal |
| AC-073 | translated | An administrator cannot use any use case of this slice |
| AC-074 | translated | An anonymous visitor cannot use any use case of this slice |
| AC-075 | translated | A goal without excusals or pauses keeps slice 6's figures and is counted |
| AC-076 | translated | Demo pause loading creates `demo6@example.com` and names no private data in its results |
| AC-077 | translated | The demo figures of `demo6@example.com` hold on every weekday |
| AC-078 | translated | The demo excusals, pauses, states and notes of `demo6@example.com` |
| AC-079 | translated | Demo pause loading leaves an existing `demo6@example.com` unchanged |
| AC-080 | translated | Demo pause loading without a valid demo password creates nothing |
| AC-081 | translated | A demo goal whose catalog habit is not Published is left out |
| AC-082 | translated | The earlier demo accounts get no excusal and no pause |
| AC-083 | translated | A yes/no week's target leaves an Excused commitment out |
| AC-084 | translated | A date in a pause that is not a commitment is refused as not scheduled |
| AC-085 | translated | An Excused Due commitment is not counted or listed as Due |
| AC-086 | translated | For a goal that has not started, a pause is current or past by the account's today |
| AC-087 | translated | A goal that has not started can be resumed from its current pause |
| AC-088 | translated | A pause of several months is allowed and leaves the streak before it unchanged |

## Issues

No issues.

Each item was checked against spec.md (FR check orders, D-1 to D-4, Edge Cases, FR-031 demo figures) and SPEC §5.3 and §6. The
dates, states, grace ends, figures and demo values (R − k dates for every run date in AC-077 and AC-078) agree. No item names a
golden sample, and the folder `tests/HT.AcceptanceTests/Samples/F008/` does not exist; no item needs it.

Note, not an issue: the trend values of AC-060 (per-week yes/no, target 3) and AC-061 (per-day count goal, "with the target 1")
depend on slice 6's trend rule (FR-006), which spec.md cites but does not restate. Nothing in the spec inputs contradicts them,
so they were copied verbatim.

## Interpretations

- All items: each `### AC-NNN (refs) Title` heading became a `Scenario` (or a `Scenario Outline` for items with an `Examples:`
  table), named by the heading's title without the id and traceability. The scenario is tagged with the id and with one tag per
  traceability reference, in the given order (e.g. `AC-001 (FR-001; FR-002; FR-017; US1-AS1)` → `@AC-001 @FR-001 @FR-002 @FR-017 @US1-AS1`).
- All items: the Markdown bullets `- Given` / `- When` / `- Then` / `- And` became Gherkin steps with the same keyword and the
  same wording, indented under the scenario. Backticks, quotes, dates, instants and en dashes (`2026-11-16 – 2026-11-22`) were kept as written.
- All items with `Examples:` (AC-003, AC-004, AC-006 to AC-011, AC-013, AC-014, AC-016, AC-018, AC-019, AC-022, AC-026 to
  AC-030, AC-041, AC-044, AC-049 to AC-051, AC-072 to AC-074, AC-077, AC-080, AC-081, AC-086): the Markdown separator row
  `|---|...|` was removed, the cells were aligned with spaces, and the `Examples:` keyword was indented under the outline.
  The placeholders (`<n>`, `<time>`, `<date>`, ...) are the item's own.
- AC-003: the cells `"  Ill  "` and `"   "` keep their quotes, so Gherkin's trimming of table cells leaves the spaces inside
  the quotes unchanged.
- AC-051: the `actor` column holds the full emails exactly as the item gives them (with the row number written in, not `<n>`).
- AC-077: the `n` column is kept, though no step uses it, as the item gives it.
- The `Feature:` line is tagged `@F008` and titled "Excusals and pauses" (spec.md's title).
