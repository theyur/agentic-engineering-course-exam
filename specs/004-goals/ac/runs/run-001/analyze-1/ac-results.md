# AC results

## Current feature (F004)

Total: 299 · passed: 49 · failed: 103 · pending: 147 · skipped: 0

| Feature | Scenario | Tags | Outcome | Duration |
|---|---|---|---|---|
| F004 | Creating a goal with the starting values unchanged | @F004 @AC-005 @FR-001 @FR-002 @FR-005 @FR-015 @US1-AS5 | passed | 00:00:00.2219113 |
| F004 | The Home greeting follows the time of day in the account's time zone(n: "04", time: "`2026-11-18T10:00:00Z` (12:00 in Kyiv)", greeting: "\"Good afternoon, Yuri\"", __pickleIndex: "281", exampleTags: []) | @F004 @AC-086 @FR-029 @US11-AS3 | failed | 00:00:00.0001813 |
| F004 | A second change replaces a version not yet begun, and follows a version already in effect(n: "01", schedule: "3 times per week", startDate: "2026-11-16", from: "2026-11-16", to: "2026-11-29", versions: "the time zone `Europe/Kyiv` from 2026-11-16 and a "···, entries: "2026-11-16 to 2026-11-22 from `2026-11-15T22:00:00"···, __pickleIndex: "268", exampleTags: []) | @F004 @AC-081 @FR-026 @US10-AS10 | passed | 00:00:00.1245139 |
| F004 | Starting from a custom habit gives only Build or Reduce from its intent(n: "01", intent: "Build", __pickleIndex: "181", exampleTags: []) | @F004 @AC-046 @FR-007 @US5-AS1 | pending | 00:00:00.0005040 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "12", goal: "on `A4.008 12 Reading` with Build, yes/no, fixed d"···, message: "\"Choose a start date.\"", __pickleIndex: "41", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | pending | 00:00:00.0829608 |
| F004 | A goal name has at most 60 characters after trimming(n: "02", name: "a text of 61 characters", result: "\"The name can have at most 60 characters.\"", __pickleIndex: "58", exampleTags: []) | @F004 @AC-011 @FR-006 @FR-009 | pending | 00:00:00.0763592 |
| F004 | Unpublishing a habit that goals use needs confirmation and gives only the number of goals(n: "03", goals: "`f4.ac057.a.03@example.com` created two goals and "···, count: "3", text: "\"3 goals use this habit.\"", __pickleIndex: "204", exampleTags: []) | @F004 @AC-057 @FR-033 @US7-AS1 @US7-AS3 @US7-AS5 | failed | 00:00:00.0785093 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "05", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "`A4.014 05 Draft`", __pickleIndex: "76", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | pending | 00:00:00.0817349 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "09", browserTimeZone: "the browser time zone `+02:00`", timeZone: "`UTC`", __pickleIndex: "230", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | failed | 00:00:00.0002751 |
| F004 | A goal keeps the values it was created with, whatever happens to the suggested starting goal | @F004 @AC-092 @FR-007 | failed | 00:00:00.0972945 |
| F004 | "Only habits I track" returns the Published habits with the user's goals, with the other filters(n: "01", filter: "no category", result: "`A4.053 01 Reading` and `A4.053 01 Sugar`, and not"···, __pickleIndex: "191", exampleTags: []) | @F004 @AC-053 @FR-031 @US6-AS2 | failed | 00:00:00.0819454 |
| F004 | A goal that has not started gives its start date | @F004 @AC-024 @FR-013 @FR-014 @US2-AS3 | pending | 00:00:00.0011636 |
| F004 | Each field refusal of goal creation, with its message(n: "13", settings: "Build, count 20 with no unit, 3 times per week", message: "\"Enter a unit, for example pages.\"", __pickleIndex: "20", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0820410 |
| F004 | A goal without a name of its own is named after its habit and target(n: "03", name: "the name \"  Morning pages  \"", goalName: "\"Morning pages\"", own: "its own", __pickleIndex: "84", exampleTags: []) | @F004 @AC-016 @FR-009 @FR-010 @US1-AS9 | pending | 00:00:00.0809621 |
| F004 | An administrator gets no goal use case(n: "03", action: "lists their goals", __pickleIndex: "131", exampleTags: []) | @F004 @AC-031 @FR-021 @US3-AS3 | failed | 00:00:00.0809346 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "07", goal: "on `A4.008 07 Reading` with Build, count 20 with a"···, message: "\"The unit can have at most 20 characters.\"", __pickleIndex: "36", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | pending | 00:00:00.0897165 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "06", browserTimeZone: "the browser time zone `EST`", timeZone: "`UTC`", __pickleIndex: "227", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | failed | 00:00:00.0002320 |
| F004 | An administrator gets no goal use case(n: "01", action: "starts a new goal from `A4.031 01 Reading`", __pickleIndex: "129", exampleTags: []) | @F004 @AC-031 @FR-021 @US3-AS3 | failed | 00:00:00.0781815 |
| F004 | A goal name has at most 60 characters after trimming(n: "03", name: "two spaces, a text of 60 characters and two spaces", result: "creating succeeds, and reading the goal gives the "···, __pickleIndex: "59", exampleTags: []) | @F004 @AC-011 @FR-006 @FR-009 | pending | 00:00:00.0818921 |
| F004 | Use cases check the acting user, the role, the identifier and the fields, in this order(n: "04", actingUser: "the anonymous visitor", action: "creates a goal on `A4.035 04 Draft` with no Build "···, message: "\"You are not signed in.\"", __pickleIndex: "150", exampleTags: []) | @F004 @AC-035 @FR-023 | pending | 00:00:00.0771005 |
| F004 | An anonymous visitor gets no goal use case(n: "01", action: "starts a new goal from `A4.032 01 Reading`", __pickleIndex: "135", exampleTags: []) | @F004 @AC-032 @FR-022 @US3-AS4 | failed | 00:00:00.0790388 |
| F004 | Reading a goal gives the version of its current date and the one later version(n: "02", schedule: "fixed days Monday, Wednesday and Friday", startDate: "2026-11-16", time: "`2026-11-19T04:59:59Z`", versions: "the time zone `Europe/Kyiv` from 2026-11-16 and a "···, __pickleIndex: "244", exampleTags: []) | @F004 @AC-073 @FR-016 @FR-026 @US10-AS6 | passed | 00:00:00.0990688 |
| F004 | The edges of every target range are accepted(n: "07", settings: "Build, yes/no, 20 times per week", targetText: "\"20 times a week\"", __pickleIndex: "50", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | pending | 00:00:00.0752467 |
| F004 | Each field refusal of goal creation, with its message(n: "17", settings: "Build, yes/no, fixed days with no day", message: "\"Choose at least one day.\"", __pickleIndex: "24", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0805328 |
| F004 | A habit that no goal uses is unpublished at once(n: "01", confirmation: "without confirming", __pickleIndex: "206", exampleTags: []) | @F004 @AC-059 @FR-019 @FR-033 @US7-AS4 | passed | 00:00:00.0887822 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "11", browserTimeZone: "the browser time zone `\"\"`", timeZone: "`UTC`", __pickleIndex: "232", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | failed | 00:00:00.0002325 |
| F004 | A goal name has at most 60 characters after trimming(n: "01", name: "a text of 60 characters", result: "creating succeeds, and reading the goal gives that"···, __pickleIndex: "57", exampleTags: []) | @F004 @AC-011 @FR-006 @FR-009 | pending | 00:00:00.0767238 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "14", goal: "on `A4.008 14 Reading` with Build, yes/no, fixed d"···, message: "\"Choose a start date within a year from today.\"", __pickleIndex: "43", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | pending | 00:00:00.0828270 |
| F004 | A named time zone is kept as given, and the same time zone changes nothing(n: "02", value: "`Europe/Kiev`", later: "a later version `Europe/Kiev` from 2026-11-19", __pickleIndex: "258", exampleTags: []) | @F004 @AC-075 @FR-024 @FR-026 | passed | 00:00:00.0852919 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "21", settings: "Reduce, yes/no, fixed days Monday, Tuesday, Wednes"···, targetText: "\"avoided on Mon–Wed, Sat\"", __pickleIndex: "107", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0003702 |
| F004 | Browsing gives a registered user their goal count per habit and the tracked count(n: "03", search: "`A4.052 03 Y`", result: "the result is `A4.052 03 Yoga` only, with the goal"···, __pickleIndex: "190", exampleTags: []) | @F004 @AC-052 @FR-031 @US6-AS1 | failed | 00:00:00.0850407 |
| F004 | Anonymous visitors and administrators get the catalog without goal information(n: "04", actingUser: "`f4.ac055.admin.04@example.com`", action: "reads the details of `A4.055 04 Reading` as `f4.ac"···, result: "the details have no goal count and no goals", __pickleIndex: "198", exampleTags: []) | @F004 @AC-055 @FR-032 @US6-AS4 | failed | 00:00:00.0760974 |
| F004 | Editing a custom habit renames its goals but never changes their settings | @F004 @AC-050 @FR-010 @FR-035 @US5-AS5 | pending | 00:00:00.0007279 |
| F004 | Each field refusal of goal creation, with its message(n: "01", settings: "no Build or Reduce, count 20 `pages`, 3 times per "···, message: "\"Choose Build or Reduce.\"", __pickleIndex: "8", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0774051 |
| F004 | A habit that changed between starting and creating is looked up again(n: "01", habit: "`A4.015 01 Reading`", change: "the administrator unpublished `A4.015 01 Reading` "···, message: "\"This habit is not in the catalog.\"", __pickleIndex: "80", exampleTags: []) | @F004 @AC-015 @FR-008 @FR-034 | pending | 00:00:00.0910201 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "10", browserTimeZone: "the browser time zone `europe/kyiv`", timeZone: "`UTC`", __pickleIndex: "231", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | failed | 00:00:00.0004681 |
| F004 | A Reduce goal with an amount has 1 time per period(n: "02", settings: "Reduce, duration 30 minutes, 2 times per week", result: "\"A Reduce goal with an amount has 1 time per peri"···, __pickleIndex: "61", exampleTags: []) | @F004 @AC-012 @FR-004 @FR-006 | pending | 00:00:00.0757868 |
| F004 | Each field refusal of goal creation, with its message(n: "11", settings: "Reduce, duration with no amount and the unit minut"···, message: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "18", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0796560 |
| F004 | Each field refusal of goal creation, with its message(n: "20", settings: "Build, yes/no, 21 times per week", message: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "27", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0803109 |
| F004 | Browsing gives a registered user their goal count per habit and the tracked count(n: "02", search: "`A4.052 02 R`", result: "the result is `A4.052 02 Reading` only, with the g"···, __pickleIndex: "189", exampleTags: []) | @F004 @AC-052 @FR-031 @US6-AS1 | failed | 00:00:00.0857465 |
| F004 | The catalog suggestion text of the starting values follows the goal rules | @F004 @AC-019 @FR-007 | failed | 00:00:00.1520017 |
| F004 | A fixed-days goal has one commitment per scheduled day, from midnight to midnight in its time zone | @F004 @AC-036 @FR-003 @FR-011 @FR-012 @FR-013 @US4-AS1 @US10-AS3 | failed | 00:00:00.0775255 |
| F004 | A catalog habit without suggested starting goals gives only Build or Reduce from its intent | @F004 @AC-003 @FR-007 @US1-AS3 | failed | 00:00:00.1702098 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "01", browserTimeZone: "the browser time zone `Europe/Kyiv`", timeZone: "`Europe/Kyiv`", __pickleIndex: "222", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | failed | 00:00:00.0002361 |
| F004 | An anonymous visitor gets no goal use case(n: "05", action: "reads the evaluation calendar of `f4.ac032.user.05"···, __pickleIndex: "139", exampleTags: []) | @F004 @AC-032 @FR-022 @US3-AS4 | failed | 00:00:00.0773136 |
| F004 | After a move east, a commitment in progress is still the next one(n: "02", time: "`2026-11-19T04:59:59Z`", next: "2026-11-18", __pickleIndex: "264", exampleTags: []) | @F004 @AC-078 @FR-014 @FR-026 | passed | 00:00:00.0874452 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "20", settings: "Reduce, yes/no, fixed days on all seven days", targetText: "\"avoided every day\"", __pickleIndex: "106", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0005476 |
| F004 | An anonymous visitor gets no Home heading, no name change and no profile(n: "02", action: "changes their name to \"Yuri\"", __pickleIndex: "294", exampleTags: []) | @F004 @AC-091 @FR-023 @FR-028 @FR-029 @FR-030 @US11-AS8 | passed | 00:00:00.0015370 |
| F004 | An account name is trimmed, and empty means no name(n: "02", name: "no name", result: "no name", __pickleIndex: "274", exampleTags: []) | @F004 @AC-084 @FR-019 @FR-028 @US11-AS1 | failed | 00:00:00.0002130 |
| F004 | A Reduce goal with an amount has 1 time per period(n: "06", settings: "Reduce, count 2 `cups`, fixed days Monday, Wednesd"···, result: "creating succeeds", __pickleIndex: "65", exampleTags: []) | @F004 @AC-012 @FR-004 @FR-006 | pending | 00:00:00.0722666 |
| F004 | An anonymous visitor gets no goal use case(n: "02", action: "creates a goal on `A4.032 02 Reading` with Build, "···, __pickleIndex: "136", exampleTags: []) | @F004 @AC-032 @FR-022 @US3-AS4 | failed | 00:00:00.0774545 |
| F004 | A new version's first period that has already ended is kept | @F004 @AC-083 @FR-027 | passed | 00:00:00.1379911 |
| F004 | A custom habit is looked up only among the acting user's custom habits(n: "03", action: "starts a new goal from", habit: "the identifier of `A4.033 03 Reading`", __pickleIndex: "143", exampleTags: []) | @F004 @AC-033 @FR-008 @US3-AS5 | pending | 00:00:00.0769591 |
| F004 | Each field refusal of goal creation, with its message(n: "03", settings: "Build, yes/no with the amount 5, fixed days Monday", message: "\"A yes/no goal has no amount or unit.\"", __pickleIndex: "10", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0812429 |
| F004 | A fixed-days goal starting on an unscheduled day begins at its next scheduled day | @F004 @AC-045 @FR-003 @FR-013 | pending | 00:00:00.0009741 |
| F004 | The Home date is today in the account's time zone, in English whatever the culture(n: "02", culture: "`uk-UA`", __pickleIndex: "287", exampleTags: []) | @F004 @AC-088 @FR-029 @US11-AS5 | failed | 00:00:00.0002041 |
| F004 | Use cases check the acting user, the role, the identifier and the fields, in this order(n: "03", actingUser: "`f4.ac035.user.03@example.com`", action: "reads the evaluation calendar from 2026-11-24 to 2"···, message: "\"This goal does not exist.\"", __pickleIndex: "149", exampleTags: []) | @F004 @AC-035 @FR-023 | pending | 00:00:00.0768074 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "15", browserTimeZone: "the browser time zone `America/Argentina/Buenos_Ai"···, timeZone: "`America/Argentina/Buenos_Aires`", __pickleIndex: "236", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | failed | 00:00:00.0002508 |
| F004 | An anonymous visitor gets no Home heading, no name change and no profile(n: "01", action: "reads the Home heading", __pickleIndex: "293", exampleTags: []) | @F004 @AC-091 @FR-023 @FR-028 @FR-029 @FR-030 @US11-AS8 | failed | 00:00:00.0002204 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "06", settings: "Build, yes/no, fixed days Sunday", targetText: "\"once a week\"", __pickleIndex: "92", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0007983 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "04", goal: "on `A4.008 04 Reading` with Build, yes/no with the"···, message: "\"A yes/no goal has no amount or unit.\"", __pickleIndex: "33", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | pending | 00:00:00.0854717 |
| F004 | A range before the start date gives an empty calendar | @F004 @AC-042 @FR-013 @US4-AS6 | pending | 00:00:00.0007549 |
| F004 | Each goal follows a time zone change from its own next day or week(n: "01", schedule: "1 time per day", startDate: "2026-11-16", from: "2026-11-18", to: "2026-11-19", entries: "2026-11-18 from `2026-11-17T22:00:00Z` to `2026-11"···, __pickleIndex: "240", exampleTags: []) | @F004 @AC-072 @FR-026 @FR-027 @US10-AS5 | passed | 00:00:00.0901947 |
| F004 | The Home greeting follows the time of day in the account's time zone(n: "02", time: "`2026-11-18T03:00:00Z` (05:00 in Kyiv)", greeting: "\"Good morning, Yuri\"", __pickleIndex: "279", exampleTags: []) | @F004 @AC-086 @FR-029 @US11-AS3 | failed | 00:00:00.0003945 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "03", browserTimeZone: "the browser time zone `Mars/Olympus`", timeZone: "`UTC`", __pickleIndex: "224", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | failed | 00:00:00.0002454 |
| F004 | Starting from a catalog habit without a position gives its marked suggested starting goal(n: "02", suggestions: "1. Build, count 20 `pages`, 3 times per week; 2. B"···, __pickleIndex: "1", exampleTags: []) | @F004 @AC-001 @FR-007 @US1-AS1 | failed | 00:00:00.1541410 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "14", settings: "Reduce, yes/no, 1 time per day", targetText: "\"at most once a day\"", __pickleIndex: "100", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0003048 |
| F004 | The edges of every target range are accepted(n: "01", settings: "Build, count 1 `pages`, 3 times per week", targetText: "\"1 pages, 3 times a week\"", __pickleIndex: "44", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | pending | 00:00:00.0738682 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "12", settings: "Build, yes/no, 2 times per day", targetText: "\"2 times a day\"", __pickleIndex: "98", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0003253 |
| F004 | Reading a goal gives the version of its current date and the one later version(n: "01", schedule: "fixed days Monday, Wednesday and Friday", startDate: "2026-11-16", time: "`2026-11-18T12:00:00Z`", versions: "the time zone `Europe/Kyiv` from 2026-11-16 and a "···, __pickleIndex: "243", exampleTags: []) | @F004 @AC-073 @FR-016 @FR-026 @US10-AS6 | passed | 00:00:00.0920504 |
| F004 | A per-week goal has one evaluation period per Monday-to-Sunday week, from its first full week(n: "03", startDate: "2026-11-18", from: "2026-11-18", to: "2026-11-22", periods: "no period", __pickleIndex: "158", exampleTags: []) | @F004 @AC-038 @FR-013 @US4-AS3 | pending | 00:00:00.0012359 |
| F004 | After a move west, a commitment already over is not the next one(n: "01", time: "`2026-11-18T21:59:59Z`", next: "2026-11-18", __pickleIndex: "261", exampleTags: []) | @F004 @AC-077 @FR-014 @FR-026 | passed | 00:00:00.1298782 |
| F004 | A goal without a name of its own is named after its habit and target(n: "04", name: "the name `\"   \"`", goalName: "\"A4.016 04 Reading, 15 min, 5 days a week\"", own: "not its own", __pickleIndex: "85", exampleTags: []) | @F004 @AC-016 @FR-009 @FR-010 @US1-AS9 | pending | 00:00:00.0797422 |
| F004 | Anonymous visitors and administrators get the catalog without goal information(n: "01", actingUser: "the anonymous visitor", action: "browses the catalog as the anonymous visitor with "···, result: "the result contains `A4.055 01 Reading` with no go"···, __pickleIndex: "195", exampleTags: []) | @F004 @AC-055 @FR-032 @US6-AS4 | failed | 00:00:00.0797803 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "02", action: "starts a new goal from", habit: "`A4.014 02 Gone`", __pickleIndex: "73", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | pending | 00:00:00.0864591 |
| F004 | The next commitment is the first commitment that has not ended(n: "05", time: "`2026-11-21T00:00:00Z`", next: "2026-11-23", __pickleIndex: "118", exampleTags: []) | @F004 @AC-023 @FR-014 @US2-AS2 | pending | 00:00:00.0005172 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "11", settings: "Reduce, count 0 `cigarettes`, 1 time per day", targetText: "\"at most 0 cigarettes a day\"", __pickleIndex: "97", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0002763 |
| F004 | The Home greeting follows the time of day in the account's time zone(n: "07", time: "`2026-11-18T21:59:59Z` (23:59:59 in Kyiv)", greeting: "\"Good evening, Yuri\"", __pickleIndex: "284", exampleTags: []) | @F004 @AC-086 @FR-029 @US11-AS3 | failed | 00:00:00.0002304 |
| F004 | The edges of every target range are accepted(n: "05", settings: "Build, count 5 with a unit of 20 characters, 1 tim"···, targetText: "\"5 uuuuuuuuuuuuuuuuuuuu a day\"", __pickleIndex: "48", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | pending | 00:00:00.0776564 |
| F004 | Each field refusal of goal creation, with its message(n: "08", settings: "Build, duration 0 minutes, 3 times per week", message: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "15", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0814322 |
| F004 | A Reduce goal with an amount has 1 time per period(n: "01", settings: "Reduce, count 2 `cups`, 1 time per week", result: "creating succeeds", __pickleIndex: "60", exampleTags: []) | @F004 @AC-012 @FR-004 @FR-006 | pending | 00:00:00.0741755 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "16", settings: "Reduce, count 2 `cups`, fixed days Monday to Frida"···, targetText: "\"at most 2 cups, 5 days a week\"", __pickleIndex: "102", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0003658 |
| F004 | Starting from a custom habit gives only Build or Reduce from its intent(n: "02", intent: "Reduce", __pickleIndex: "182", exampleTags: []) | @F004 @AC-046 @FR-007 @US5-AS1 | pending | 00:00:00.0010356 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "09", settings: "Build, duration 30 minutes, fixed days on all seve"···, targetText: "\"30 min daily\"", __pickleIndex: "95", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0003442 |
| F004 | A habit that changed between starting and creating is looked up again(n: "02", habit: "their custom habit `Cold shower`", change: "`f4.ac015.user.02@example.com` deleted their custo"···, message: "\"This custom habit does not exist.\"", __pickleIndex: "81", exampleTags: []) | @F004 @AC-015 @FR-008 @FR-034 | pending | 00:00:00.0757917 |
| F004 | A Reduce goal with an amount has 1 time per period(n: "04", settings: "Reduce, yes/no, 3 times per week", result: "creating succeeds", __pickleIndex: "63", exampleTags: []) | @F004 @AC-012 @FR-004 @FR-006 | pending | 00:00:00.0752057 |
| F004 | Starting from a catalog habit without a position gives its marked suggested starting goal(n: "01", suggestions: "1. Build, duration 15 minutes, fixed days Monday t"···, __pickleIndex: "0", exampleTags: []) | @F004 @AC-001 @FR-007 @US1-AS1 | failed | 00:00:00.1792285 |
| F004 | A goal on a custom habit is based on it and named after it | @F004 @AC-047 @FR-001 @FR-010 @US5-AS2 | pending | 00:00:00.0009931 |
| F004 | Demo loading gives each created demo account its demo goals | @F004 @AC-064 @FR-040 @US9-AS1 | passed | 00:00:00.5248872 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "10", settings: "Build, count 10 `pages`, 1 time per day", targetText: "\"10 pages a day\"", __pickleIndex: "96", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0005230 |
| F004 | A second change replaces a version not yet begun, and follows a version already in effect(n: "03", schedule: "1 time per day", startDate: "2026-12-01", from: "2026-12-01", to: "2026-12-01", versions: "the time zone `Asia/Tokyo` from 2026-12-01 and no "···, entries: "2026-12-01 from `2026-11-30T15:00:00Z` to `2026-12"···, __pickleIndex: "270", exampleTags: []) | @F004 @AC-081 @FR-026 @US10-AS10 | passed | 00:00:00.1499392 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "07", browserTimeZone: "the browser time zone `CET`", timeZone: "`UTC`", __pickleIndex: "228", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | failed | 00:00:00.0002297 |
| F004 | After a move west, a fixed-days goal keeps the old time zone up to its current date | @F004 @AC-071 @FR-011 @FR-026 @FR-027 @US10-AS4 | passed | 00:00:00.0884499 |
| F004 | Browsing gives a registered user their goal count per habit and the tracked count(n: "01", search: "`A4.052 01`", result: "`A4.052 01 Reading` has the goal count 2, `A4.052 "···, __pickleIndex: "188", exampleTags: []) | @F004 @AC-052 @FR-031 @US6-AS1 | failed | 00:00:00.0837037 |
| F004 | A habit that no goal uses is unpublished at once(n: "02", confirmation: "with confirmation", __pickleIndex: "207", exampleTags: []) | @F004 @AC-059 @FR-019 @FR-033 @US7-AS4 | passed | 00:00:00.0823998 |
| F004 | The calendar range has 1 to 366 days(n: "04", from: "2026-11-18", to: "2026-11-18", result: "a calendar of 1 commitment, on 2026-11-18", __pickleIndex: "175", exampleTags: []) | @F004 @AC-043 @FR-013 @US4-AS7 | pending | 00:00:00.0009289 |
| F004 | A goal on an Unpublished habit keeps working, and no new goal can use that habit(n: "04", actingUser: "`f4.ac060.owner.04@example.com`", action: "creates a goal on `A4.060 04 Reading` with Build, "···, result: "the result is \"This habit is not in the catalog.\"···, __pickleIndex: "211", exampleTags: []) | @F004 @AC-060 @FR-016 @FR-034 @US7-AS6 | failed | 00:00:00.0807587 |
| F004 | An administrator gets no goal use case(n: "04", action: "reads `f4.ac031.user.04@example.com`'s goal", __pickleIndex: "132", exampleTags: []) | @F004 @AC-031 @FR-021 @US3-AS3 | failed | 00:00:00.0824518 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "06", value: "`+02:00`", __pickleIndex: "254", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.0900833 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "01", value: "`Mars/Olympus`", __pickleIndex: "249", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.0907337 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "17", browserTimeZone: "the browser time zone `America/New_York`", timeZone: "`America/New_York`", __pickleIndex: "238", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | failed | 00:00:00.0002324 |
| F004 | Use cases check the acting user, the role, the identifier and the fields, in this order(n: "01", actingUser: "the anonymous visitor", action: "reads the evaluation calendar from 2026-11-24 to 2"···, message: "\"You are not signed in.\"", __pickleIndex: "147", exampleTags: []) | @F004 @AC-035 @FR-023 | pending | 00:00:00.0745217 |
| F004 | The next commitment is the first commitment that has not ended(n: "04", time: "`2026-11-19T00:00:00Z`", next: "2026-11-20", __pickleIndex: "117", exampleTags: []) | @F004 @AC-023 @FR-014 @US2-AS2 | pending | 00:00:00.0012500 |
| F004 | Each field refusal of goal creation, with its message(n: "06", settings: "Build, count 0 `pages`, 3 times per week", message: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "13", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0802770 |
| F004 | A goal stays as it was created | @F004 @AC-028 @FR-017 @FR-018 | failed | 00:00:00.0925311 |
| F004 | Unpublishing a habit that goals use needs confirmation and gives only the number of goals(n: "02", goals: "`f4.ac057.a.02@example.com` created one goal", count: "1", text: "\"1 goal uses this habit.\"", __pickleIndex: "203", exampleTags: []) | @F004 @AC-057 @FR-033 @US7-AS1 @US7-AS3 @US7-AS5 | failed | 00:00:00.0818036 |
| F004 | A named time zone is kept as given, and the same time zone changes nothing(n: "03", value: "`UTC`", later: "a later version `UTC` from 2026-11-19", __pickleIndex: "259", exampleTags: []) | @F004 @AC-075 @FR-024 @FR-026 | passed | 00:00:00.0957818 |
| F004 | A Reduce goal with an amount has 1 time per period(n: "03", settings: "Reduce, count 2 `cups`, 3 times per day", result: "\"A Reduce goal with an amount has 1 time per peri"···, __pickleIndex: "62", exampleTags: []) | @F004 @AC-012 @FR-004 @FR-006 | pending | 00:00:00.0770448 |
| F004 | Anonymous visitors and administrators get the catalog without goal information(n: "06", actingUser: "`f4.ac055.admin.06@example.com`", action: "browses the catalog as `f4.ac055.admin.06@example."···, result: "the result is \"Goals are for registered users.\"", __pickleIndex: "200", exampleTags: []) | @F004 @AC-055 @FR-032 @US6-AS4 | failed | 00:00:00.0775546 |
| F004 | A demo goal finds its catalog habit by name ignoring case, and only "Coffee" may be Unpublished | @F004 @AC-067 @FR-040 | passed | 00:00:00.5975575 |
| F004 | The Home greeting follows the time of day in the account's time zone(n: "03", time: "`2026-11-18T09:59:00Z` (11:59 in Kyiv)", greeting: "\"Good morning, Yuri\"", __pickleIndex: "280", exampleTags: []) | @F004 @AC-086 @FR-029 @US11-AS3 | failed | 00:00:00.0001721 |
| F004 | Reading a goal gives all its fields, its settings version and its attempt | @F004 @AC-025 @FR-005 @FR-016 @FR-018 @US2-AS4 | failed | 00:00:00.0772767 |
| F004 | A per-week goal has one evaluation period per Monday-to-Sunday week, from its first full week(n: "02", startDate: "2026-11-18", from: "2026-11-25", to: "2026-12-01", periods: "2 periods: 2026-11-23 to 2026-11-29 from `2026-11-"···, __pickleIndex: "157", exampleTags: []) | @F004 @AC-038 @FR-013 @US4-AS3 | pending | 00:00:00.0005250 |
| F004 | An administrator gets no goal use case(n: "06", action: "browses the catalog as `f4.ac031.admin.06@example."···, __pickleIndex: "134", exampleTags: []) | @F004 @AC-031 @FR-021 @US3-AS3 | failed | 00:00:00.0819023 |
| F004 | Each field refusal of goal creation, with its message(n: "22", settings: "Reduce, count 2 `cups`, 2 times per day", message: "\"A Reduce goal with an amount has 1 time per peri"···, __pickleIndex: "29", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0860325 |
| F004 | Listing gives every goal with its summary, in name order | @F004 @AC-021 @FR-014 @FR-015 @US2-AS1 | pending | 00:00:00.0818244 |
| F004 | The days around a clock change meet at local midnight | @F004 @AC-041 @FR-012 @FR-013 @US4-AS5 | failed | 00:00:00.0798422 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "08", browserTimeZone: "the browser time zone `GMT`", timeZone: "`UTC`", __pickleIndex: "229", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | failed | 00:00:00.0002302 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "02", settings: "Build, count 20 `pages`, 3 times per week", targetText: "\"20 pages, 3 times a week\"", __pickleIndex: "88", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0007164 |
| F004 | After a move east, a commitment in progress is still the next one(n: "03", time: "`2026-11-19T05:00:00Z`", next: "2026-11-20", __pickleIndex: "265", exampleTags: []) | @F004 @AC-078 @FR-014 @FR-026 | passed | 00:00:00.0862847 |
| F004 | A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "04", schedule: "1 time per day", date: "2027-10-31", entry: "2027-10-31 from `2027-10-30T21:00:00Z` to `2027-10"···, __pickleIndex: "167", exampleTags: []) | @F004 @AC-040 @FR-012 @FR-013 @US4-AS5 | failed | 00:00:00.0747662 |
| F004 | The start date is today or up to 365 days later(n: "05", startDate: "no start date", result: "\"Choose a start date.\"", __pickleIndex: "56", exampleTags: []) | @F004 @AC-010 @FR-006 @FR-012 | pending | 00:00:00.0820158 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "13", settings: "Reduce, yes/no, 3 times per week", targetText: "\"at most 3 times a week\"", __pickleIndex: "99", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0002771 |
| F004 | After a move west, a commitment already over is not the next one(n: "02", time: "`2026-11-18T23:00:00Z`", next: "2026-11-20", __pickleIndex: "262", exampleTags: []) | @F004 @AC-077 @FR-014 @FR-026 | passed | 00:00:00.1176694 |
| F004 | Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "03", submitted: "Build, yes/no with the count unit `\"   \"`, fixed"···, stored: "yes/no, no amount and no unit", targetText: "\"daily\"", __pickleIndex: "68", exampleTags: []) | @F004 @AC-013 @FR-002 @FR-006 | pending | 00:00:00.0783135 |
| F004 | Reading a goal gives the version of its current date and the one later version(n: "03", schedule: "fixed days Monday, Wednesday and Friday", startDate: "2026-11-16", time: "`2026-11-19T05:00:00Z`", versions: "the time zone `America/New_York` from 2026-11-19 a"···, __pickleIndex: "245", exampleTags: []) | @F004 @AC-073 @FR-016 @FR-026 @US10-AS6 | passed | 00:00:00.0938029 |
| F004 | A per-week goal tells when its first week starts(n: "03", startDate: "2026-11-18", time: "`2026-11-23T00:00:00Z`", list: "no first week starts, no start date and no next co"···, __pickleIndex: "162", exampleTags: []) | @F004 @AC-039 @FR-014 @FR-016 @US4-AS4 | pending | 00:00:00.0007914 |
| F004 | Each field refusal of goal creation, with its message(n: "16", settings: "Build, yes/no, no schedule", message: "\"Choose fixed days or how many times.\"", __pickleIndex: "23", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0799401 |
| F004 | The start date is today or up to 365 days later(n: "02", startDate: "the start date 2026-11-17", result: "\"The start date cannot be in the past.\"", __pickleIndex: "53", exampleTags: []) | @F004 @AC-010 @FR-006 @FR-012 | pending | 00:00:00.0787907 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "02", goal: "on `A4.008 02 Draft` with Build, no measurement, f"···, message: "\"This habit is not in the catalog.\"", __pickleIndex: "31", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | pending | 00:00:00.0843506 |
| F004 | After a large move east, a version already begun is in effect, and a second change follows it | @F004 @AC-082 @FR-026 @FR-027 | passed | 00:00:00.0937917 |
| F004 | The Home date is today in the account's time zone, in English whatever the culture(n: "01", culture: "the invariant culture", __pickleIndex: "286", exampleTags: []) | @F004 @AC-088 @FR-029 @US11-AS5 | failed | 00:00:00.0008490 |
| F004 | After a move east, a commitment in progress is still the next one(n: "01", time: "`2026-11-18T16:00:00Z`", next: "2026-11-18", __pickleIndex: "263", exampleTags: []) | @F004 @AC-078 @FR-014 @FR-026 | passed | 00:00:00.0900524 |
| F004 | A custom habit is looked up only among the acting user's custom habits(n: "04", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "the identifier of `A4.033 04 Reading`", __pickleIndex: "144", exampleTags: []) | @F004 @AC-033 @FR-008 @US3-AS5 | pending | 00:00:00.0803348 |
| F004 | Browsing and details without an acting user give the slice 2 result | @F004 @AC-056 @FR-019 @FR-031 | failed | 00:00:00.0836955 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "03", action: "starts a new goal from", habit: "the identifier of their custom habit `Cold shower`", __pickleIndex: "74", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | pending | 00:00:00.0802561 |
| F004 | Catalog goal counts are the acting user's own | @F004 @AC-034 @FR-032 @US3-AS6 | failed | 00:00:00.0951073 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "03", value: "`Etc/GMT+3`", __pickleIndex: "251", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.0912667 |
| F004 | Anonymous visitors and administrators get the catalog without goal information(n: "05", actingUser: "the anonymous visitor", action: "browses the catalog as the anonymous visitor with "···, result: "the result is \"You are not signed in.\"", __pickleIndex: "199", exampleTags: []) | @F004 @AC-055 @FR-032 @US6-AS4 | failed | 00:00:00.0742575 |
| F004 | "Today" is the date in the user's own time zone(n: "03", timeZone: "`America/New_York`", time: "`2026-11-17T03:00:00Z`", startDate: "2026-11-16", result: "creating succeeds", __pickleIndex: "178", exampleTags: []) | @F004 @AC-044 @FR-006 @FR-007 @FR-012 @US4-AS8 | passed | 00:00:00.0835946 |
| F004 | A per-week goal has one evaluation period per Monday-to-Sunday week, from its first full week(n: "01", startDate: "2026-11-18", from: "2026-11-16", to: "2026-12-06", periods: "2 periods: 2026-11-23 to 2026-11-29 from `2026-11-"···, __pickleIndex: "156", exampleTags: []) | @F004 @AC-038 @FR-013 @US4-AS3 | pending | 00:00:00.0005112 |
| F004 | A goal without a name of its own is named after its habit and target(n: "02", name: "the name \"Morning pages\"", goalName: "\"Morning pages\"", own: "its own", __pickleIndex: "83", exampleTags: []) | @F004 @AC-016 @FR-009 @FR-010 @US1-AS9 | pending | 00:00:00.0767042 |
| F004 | Each field refusal of goal creation, with its message(n: "19", settings: "Build, yes/no, 0 times per week", message: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "26", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0810168 |
| F004 | A new goal can start without a habit, but not be created without one | @F004 @AC-063 @FR-006 @FR-007 @US8-AS4 | pending | 00:00:00.0007383 |
| F004 | A custom habit that no goal uses is deleted | @F004 @AC-049 @FR-019 @FR-035 @US5-AS4 | pending | 00:00:00.0011982 |
| F004 | The demo shows a goal on an Unpublished habit, a goal with its own name and an account name | @F004 @AC-069 @FR-028 @FR-034 @FR-040 @US9-AS5 | failed | 00:00:00.2384693 |
| F004 | The next commitment is the first commitment that has not ended(n: "03", time: "`2026-11-18T23:59:59Z`", next: "2026-11-18", __pickleIndex: "116", exampleTags: []) | @F004 @AC-023 @FR-014 @US2-AS2 | pending | 00:00:00.0004965 |
| F004 | An anonymous visitor gets no goal use case(n: "06", action: "browses the catalog as the anonymous visitor with "···, __pickleIndex: "140", exampleTags: []) | @F004 @AC-032 @FR-022 @US3-AS4 | failed | 00:00:00.0775930 |
| F004 | A habit published again is in the catalog again for goals | @F004 @AC-061 @FR-034 | failed | 00:00:00.0879158 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "07", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "the identifier of their custom habit `Cold shower`", __pickleIndex: "78", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | pending | 00:00:00.0846362 |
| F004 | A suggested starting goal position the habit does not have is refused(n: "02", position: "0", __pickleIndex: "5", exampleTags: []) | @F004 @AC-004 @FR-007 @US1-AS4 | failed | 00:00:00.1764805 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "01", settings: "Build, duration 15 minutes, fixed days Monday to F"···, targetText: "\"15 min, 5 days a week\"", __pickleIndex: "87", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0002964 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "03", settings: "Reduce, count 1 `sweet`, 1 time per day", targetText: "\"at most 1 sweet a day\"", __pickleIndex: "89", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0003071 |
| F004 | The start date is today or up to 365 days later(n: "03", startDate: "the start date 2027-11-18", result: "creating succeeds", __pickleIndex: "54", exampleTags: []) | @F004 @AC-010 @FR-006 @FR-012 | pending | 00:00:00.0812451 |
| F004 | A custom habit that goals use cannot be deleted | @F004 @AC-048 @FR-035 @US5-AS3 | pending | 00:00:00.0008673 |
| F004 | The edges of every target range are accepted(n: "04", settings: "Reduce, count 999 `cups`, 1 time per day", targetText: "\"at most 999 cups a day\"", __pickleIndex: "47", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | pending | 00:00:00.0762707 |
| F004 | Each goal follows a time zone change from its own next day or week(n: "03", schedule: "1 time per day", startDate: "2026-12-01", from: "2026-12-01", to: "2026-12-01", entries: "2026-12-01 from `2026-12-01T05:00:00Z` to `2026-12"···, __pickleIndex: "242", exampleTags: []) | @F004 @AC-072 @FR-026 @FR-027 @US10-AS5 | passed | 00:00:00.0982168 |
| F004 | Use cases check the acting user, the role, the identifier and the fields, in this order(n: "07", actingUser: "`f4.ac035.admin.07@example.com`", action: "starts a new goal from `A4.035 07 Draft` with the "···, message: "\"Goals are for registered users.\"", __pickleIndex: "153", exampleTags: []) | @F004 @AC-035 @FR-023 | pending | 00:00:00.0862357 |
| F004 | Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "02", submitted: "Build, duration 30 minutes with the count unit `pa"···, stored: "duration 30 minutes, no count unit", targetText: "\"30 min, 3 times a week\"", __pickleIndex: "67", exampleTags: []) | @F004 @AC-013 @FR-002 @FR-006 | pending | 00:00:00.0742479 |
| F004 | The edges of every target range are accepted(n: "08", settings: "Build, duration 999 hours, 1 time per day", targetText: "\"999 h a day\"", __pickleIndex: "51", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | pending | 00:00:00.0768254 |
| F004 | A user's list never contains another user's goals | @F004 @AC-030 @FR-020 @US3-AS2 | failed | 00:00:00.0796510 |
| F004 | The calendar range has 1 to 366 days(n: "03", from: "2026-11-18", to: "2027-11-18", result: "a calendar of 53 commitments, the first on 2026-11"···, __pickleIndex: "174", exampleTags: []) | @F004 @AC-043 @FR-013 @US4-AS7 | pending | 00:00:00.0002715 |
| F004 | "Today" is the date in the user's own time zone(n: "04", timeZone: "`America/New_York`", time: "`2026-11-17T05:00:00Z`", startDate: "2026-11-17", result: "\"The start date cannot be in the past.\"", __pickleIndex: "179", exampleTags: []) | @F004 @AC-044 @FR-006 @FR-007 @FR-012 @US4-AS8 | passed | 00:00:00.0836318 |
| F004 | Each field refusal of goal creation, with its message(n: "18", settings: "Build, yes/no, no number of times, per week", message: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "25", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0776395 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "04", value: "`Etc/UTC`", __pickleIndex: "252", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.3320693 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "05", goal: "on `A4.008 05 Reading` with Build, count 0 with no"···, message: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "34", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | pending | 00:00:00.0819462 |
| F004 | A confirmed unpublish acts even when goals were added after the count was given | @F004 @AC-093 @FR-033 | failed | 00:00:00.0755311 |
| F004 | A goal on an Unpublished habit keeps working, and no new goal can use that habit(n: "02", actingUser: "`f4.ac060.owner.02@example.com`", action: "reads the goal", result: "the result has the habit name `A4.060 02 Reading`,"···, __pickleIndex: "209", exampleTags: []) | @F004 @AC-060 @FR-016 @FR-034 @US7-AS6 | failed | 00:00:00.0799731 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "17", settings: "Reduce, duration 1 hours, 1 time per day", targetText: "\"at most 1 h a day\"", __pickleIndex: "103", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0002878 |
| F004 | At registration, the name is checked after slice 1's checks | @F004 @AC-094 @FR-028 | failed | 00:00:00.0007908 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "05", browserTimeZone: "the browser time zone `Etc/GMT+3`", timeZone: "`UTC`", __pickleIndex: "226", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | failed | 00:00:00.0002504 |
| F004 | Use cases check the acting user, the role, the identifier and the fields, in this order(n: "06", actingUser: "`f4.ac035.user.06@example.com`", action: "starts a new goal from `A4.035 06 Draft` with the "···, message: "\"This habit is not in the catalog.\"", __pickleIndex: "152", exampleTags: []) | @F004 @AC-035 @FR-023 | pending | 00:00:00.0769773 |
| F004 | A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "03", schedule: "1 time per day", date: "2027-03-28", entry: "2027-03-28 from `2027-03-27T22:00:00Z` to `2027-03"···, __pickleIndex: "166", exampleTags: []) | @F004 @AC-040 @FR-012 @FR-013 @US4-AS5 | failed | 00:00:00.0774821 |
| F004 | An account name is trimmed, and empty means no name(n: "04", name: "the name `\"\"`", result: "no name", __pickleIndex: "276", exampleTags: []) | @F004 @AC-084 @FR-019 @FR-028 @US11-AS1 | failed | 00:00:00.0002066 |
| F004 | The calendar range has 1 to 366 days(n: "01", from: "2026-11-24", to: "2026-11-18", result: "\"Choose a range of 1 to 366 days.\"", __pickleIndex: "172", exampleTags: []) | @F004 @AC-043 @FR-013 @US4-AS7 | pending | 00:00:00.0002668 |
| F004 | The start date is today or up to 365 days later(n: "04", startDate: "the start date 2027-11-19", result: "\"Choose a start date within a year from today.\"", __pickleIndex: "55", exampleTags: []) | @F004 @AC-010 @FR-006 @FR-012 | pending | 00:00:00.0783602 |
| F004 | Use cases check the acting user, the role, the identifier and the fields, in this order(n: "02", actingUser: "`f4.ac035.admin.02@example.com`", action: "reads the evaluation calendar from 2026-11-24 to 2"···, message: "\"Goals are for registered users.\"", __pickleIndex: "148", exampleTags: []) | @F004 @AC-035 @FR-023 | pending | 00:00:00.0774486 |
| F004 | Each field refusal of goal creation, with its message(n: "05", settings: "Build, count with no amount and the unit `pages`, "···, message: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "12", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0775438 |
| F004 | Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "01", submitted: "Build, count 20 `pages` with the duration unit hou"···, stored: "count 20 `pages`, no duration unit", targetText: "\"20 pages, 3 times a week\"", __pickleIndex: "66", exampleTags: []) | @F004 @AC-013 @FR-002 @FR-006 | pending | 00:00:00.0756428 |
| F004 | Each field refusal of goal creation, with its message(n: "15", settings: "Build, duration 15 with no duration unit, 3 times "···, message: "\"Choose minutes or hours.\"", __pickleIndex: "22", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0796581 |
| F004 | Reading a goal that has not started gives its first version and attempt from its start date | @F004 @AC-026 @FR-005 @FR-016 @FR-018 | pending | 00:00:00.0009716 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "07", settings: "Build, yes/no, 1 time per week", targetText: "\"once a week\"", __pickleIndex: "93", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0002770 |
| F004 | Every suggested starting goal of the seed catalog can be created unchanged | @F004 @AC-020 @FR-007 | pending | 00:00:00.3472167 |
| F004 | A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "05", schedule: "1 time per week", date: "2027-03-28", entry: "2027-03-22 to 2027-03-28 from `2027-03-21T22:00:00"···, __pickleIndex: "168", exampleTags: []) | @F004 @AC-040 @FR-012 @FR-013 @US4-AS5 | failed | 00:00:00.0777174 |
| F004 | The profile gives "member since" with the month and year of creation in the account's time zone(n: "02", time: "`2026-11-30T22:30:00Z`", browserTimeZone: "the browser time zone `Europe/Kyiv`", memberSince: "\"member since December 2026\"", __pickleIndex: "290", exampleTags: []) | @F004 @AC-089 @FR-030 @US11-AS6 | passed | 00:00:00.0912021 |
| F004 | Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "05", submitted: "Build, yes/no, 2 times per day, with the fixed day"···, stored: "2 times per day, no fixed days", targetText: "\"2 times a day\"", __pickleIndex: "70", exampleTags: []) | @F004 @AC-013 @FR-002 @FR-006 | pending | 00:00:00.0790454 |
| F004 | Another user's goal does not exist for them(n: "04", action: "reads the evaluation calendar from 2026-11-18 to 2"···, goal: "an identifier that the test creates and that belon"···, __pickleIndex: "127", exampleTags: []) | @F004 @AC-029 @FR-020 @US3-AS1 | failed | 00:00:00.0786796 |
| F004 | A per-day goal has one evaluation period per day | @F004 @AC-037 @FR-003 @FR-013 @US4-AS2 | failed | 00:00:00.0817251 |
| F004 | Reading a goal gives the version of its current date and the one later version(n: "06", schedule: "1 time per day", startDate: "2026-12-01", time: "`2026-11-18T12:00:00Z`", versions: "the time zone `America/New_York` from 2026-12-01 a"···, __pickleIndex: "248", exampleTags: []) | @F004 @AC-073 @FR-016 @FR-026 @US10-AS6 | passed | 00:00:00.1002929 |
| F004 | Another user's goal does not exist for them(n: "01", action: "reads the goal", goal: "`f4.ac029.a.01@example.com`'s goal", __pickleIndex: "124", exampleTags: []) | @F004 @AC-029 @FR-020 @US3-AS1 | failed | 00:00:00.0786807 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "01", goal: "with no habit, no Build or Reduce, yes/no, fixed d"···, message: "\"Choose a habit.\"", __pickleIndex: "30", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | pending | 00:00:00.0896777 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "22", settings: "Reduce, yes/no, fixed days Sunday and Monday", targetText: "\"avoided on Mon, Sun\"", __pickleIndex: "108", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0002848 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "08", goal: "on `A4.008 08 Reading` with Build, duration 15 wit"···, message: "\"Choose minutes or hours.\"", __pickleIndex: "37", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | pending | 00:00:00.0843974 |
| F004 | The Home date is today in the account's time zone, in English whatever the culture(n: "03", culture: "`de-DE`", __pickleIndex: "288", exampleTags: []) | @F004 @AC-088 @FR-029 @US11-AS5 | failed | 00:00:00.0004568 |
| F004 | Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "06", submitted: "Build, count 20 `\"  pages  \"`, 3 times per week", stored: "count 20 `pages`", targetText: "\"20 pages, 3 times a week\"", __pickleIndex: "71", exampleTags: []) | @F004 @AC-013 @FR-002 @FR-006 | pending | 00:00:00.0772138 |
| F004 | Goals are listed by name ignoring case, then in creation order | @F004 @AC-022 @FR-009 @FR-015 | pending | 00:00:00.0012677 |
| F004 | The Home greeting follows the time of day in the account's time zone(n: "01", time: "`2026-11-18T02:59:00Z` (04:59 in Kyiv)", greeting: "\"Good evening, Yuri\"", __pickleIndex: "278", exampleTags: []) | @F004 @AC-086 @FR-029 @US11-AS3 | failed | 00:00:00.0001979 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "05", value: "`EST`", __pickleIndex: "253", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.1104254 |
| F004 | Reading a goal gives the version of its current date and the one later version(n: "04", schedule: "fixed days Monday, Wednesday and Friday", startDate: "2026-11-16", time: "`2026-11-20T12:00:00Z`", versions: "the time zone `America/New_York` from 2026-11-19 a"···, __pickleIndex: "246", exampleTags: []) | @F004 @AC-073 @FR-016 @FR-026 @US10-AS6 | passed | 00:00:00.1693313 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "03", goal: "on `A4.008 03 Reading` with no Build or Reduce, no"···, message: "\"Choose Build or Reduce.\"", __pickleIndex: "32", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | pending | 00:00:00.0823281 |
| F004 | "Today" is the date in the user's own time zone(n: "01", timeZone: "`Europe/Kyiv`", time: "`2026-11-16T21:30:00Z`", startDate: "2026-11-16", result: "creating succeeds", __pickleIndex: "176", exampleTags: []) | @F004 @AC-044 @FR-006 @FR-007 @FR-012 @US4-AS8 | passed | 00:00:00.0955286 |
| F004 | Starting from a suggested starting goal position gives that suggestion | @F004 @AC-002 @FR-007 @US1-AS2 | failed | 00:00:00.1575672 |
| F004 | "Today" is the date in the user's own time zone(n: "02", timeZone: "`Europe/Kyiv`", time: "`2026-11-16T22:30:00Z`", startDate: "2026-11-17", result: "\"The start date cannot be in the past.\"", __pickleIndex: "177", exampleTags: []) | @F004 @AC-044 @FR-006 @FR-007 @FR-012 @US4-AS8 | passed | 00:00:00.0821716 |
| F004 | The Home greeting follows the time of day in the account's time zone(n: "06", time: "`2026-11-18T16:00:00Z` (18:00 in Kyiv)", greeting: "\"Good evening, Yuri\"", __pickleIndex: "283", exampleTags: []) | @F004 @AC-086 @FR-029 @US11-AS3 | failed | 00:00:00.0008147 |
| F004 | The next commitment is the first commitment that has not ended(n: "02", time: "`2026-11-18T00:00:00Z`", next: "2026-11-18", __pickleIndex: "115", exampleTags: []) | @F004 @AC-023 @FR-014 @US2-AS2 | pending | 00:00:00.0007275 |
| F004 | A goal on an Unpublished habit keeps working, and no new goal can use that habit(n: "05", actingUser: "`f4.ac060.other.05@example.com`", action: "creates a goal on `A4.060 05 Reading` with Build, "···, result: "the result is \"This habit is not in the catalog.\"···, __pickleIndex: "212", exampleTags: []) | @F004 @AC-060 @FR-016 @FR-034 @US7-AS6 | failed | 00:00:00.0777726 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "04", settings: "Build, yes/no, fixed days on all seven days", targetText: "\"daily\"", __pickleIndex: "90", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0002840 |
| F004 | An administrator gets no goal use case(n: "02", action: "creates a goal on `A4.031 02 Reading` with Build, "···, __pickleIndex: "130", exampleTags: []) | @F004 @AC-031 @FR-021 @US3-AS3 | failed | 00:00:00.0827619 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "06", goal: "on `A4.008 06 Reading` with Build, count 20 with n"···, message: "\"Enter a unit, for example pages.\"", __pickleIndex: "35", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | pending | 00:00:00.0815357 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "13", goal: "on `A4.008 13 Reading` with Build, yes/no, fixed d"···, message: "\"The start date cannot be in the past.\"", __pickleIndex: "42", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | pending | 00:00:00.1118951 |
| F004 | An account name is trimmed, and empty means no name(n: "01", name: "the name \"  Yuri  \"", result: "the name \"Yuri\"", __pickleIndex: "273", exampleTags: []) | @F004 @AC-084 @FR-019 @FR-028 @US11-AS1 | failed | 00:00:00.0010491 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "08", settings: "Build, duration 2 hours, 3 times per day", targetText: "\"2 h, 3 times a day\"", __pickleIndex: "94", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0010409 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "09", goal: "on `A4.008 09 Reading` with Build, yes/no, fixed d"···, message: "\"Choose at least one day.\"", __pickleIndex: "38", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | pending | 00:00:00.0850419 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "23", settings: "Reduce, yes/no, fixed days Wednesday", targetText: "\"avoided on Wed\"", __pickleIndex: "109", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0002977 |
| F004 | A custom habit is looked up only among the acting user's custom habits(n: "01", action: "starts a new goal from", habit: "`f4.ac033.b.01@example.com`'s custom habit `Cold s"···, __pickleIndex: "141", exampleTags: []) | @F004 @AC-033 @FR-008 @US3-AS5 | pending | 00:00:00.0780782 |
| F004 | Another user's goal does not exist for them(n: "03", action: "reads the goal", goal: "an identifier that the test creates and that belon"···, __pickleIndex: "126", exampleTags: []) | @F004 @AC-029 @FR-020 @US3-AS1 | failed | 00:00:00.0761503 |
| F004 | The calendar range has 1 to 366 days(n: "02", from: "2026-11-18", to: "2027-11-19", result: "\"Choose a range of 1 to 366 days.\"", __pickleIndex: "173", exampleTags: []) | @F004 @AC-043 @FR-013 @US4-AS7 | pending | 00:00:00.0006796 |
| F004 | Unpublishing a habit that goals use needs confirmation and gives only the number of goals(n: "01", goals: "`f4.ac057.a.01@example.com` created one goal and `"···, count: "2", text: "\"2 goals use this habit.\"", __pickleIndex: "202", exampleTags: []) | @F004 @AC-057 @FR-033 @US7-AS1 @US7-AS3 @US7-AS5 | failed | 00:00:00.0823033 |
| F004 | Anonymous visitors and administrators get the catalog without goal information(n: "02", actingUser: "`f4.ac055.admin.02@example.com`", action: "browses the catalog as `f4.ac055.admin.02@example."···, result: "the result contains `A4.055 02 Reading` with no go"···, __pickleIndex: "196", exampleTags: []) | @F004 @AC-055 @FR-032 @US6-AS4 | failed | 00:00:00.0736738 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "05", settings: "Build, yes/no, 1 time per day", targetText: "\"once a day\"", __pickleIndex: "91", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0003225 |
| F004 | A suggested starting goal position the habit does not have is refused(n: "01", position: "4", __pickleIndex: "4", exampleTags: []) | @F004 @AC-004 @FR-007 @US1-AS4 | failed | 00:00:00.1711930 |
| F004 | The Home greeting follows the time of day in the account's time zone(n: "05", time: "`2026-11-18T15:59:00Z` (17:59 in Kyiv)", greeting: "\"Good afternoon, Yuri\"", __pickleIndex: "282", exampleTags: []) | @F004 @AC-086 @FR-029 @US11-AS3 | failed | 00:00:00.0002566 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "07", value: "`europe/kyiv`", __pickleIndex: "255", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.1047259 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "19", settings: "Reduce, yes/no, fixed days Saturday and Sunday", targetText: "\"avoided on Sat, Sun\"", __pickleIndex: "105", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0003214 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "02", browserTimeZone: "no browser time zone", timeZone: "`UTC`", __pickleIndex: "223", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | failed | 00:00:00.0009822 |
| F004 | An anonymous visitor gets no Home heading, no name change and no profile(n: "03", action: "reads their profile", __pickleIndex: "295", exampleTags: []) | @F004 @AC-091 @FR-023 @FR-028 @FR-029 @FR-030 @US11-AS8 | passed | 00:00:00.0021638 |
| F004 | A named time zone is kept as given, and the same time zone changes nothing(n: "01", value: "`Europe/Kyiv`", later: "no later version", __pickleIndex: "257", exampleTags: []) | @F004 @AC-075 @FR-024 @FR-026 | passed | 00:00:00.0848899 |
| F004 | The edges of every target range are accepted(n: "03", settings: "Reduce, count 0 `cups`, 1 time per day", targetText: "\"at most 0 cups a day\"", __pickleIndex: "46", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | pending | 00:00:00.0755035 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "13", browserTimeZone: "the browser time zone `Asia/Calcutta`", timeZone: "`Asia/Calcutta`", __pickleIndex: "234", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | failed | 00:00:00.0002432 |
| F004 | Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "04", submitted: "Build, yes/no, fixed days Monday and Wednesday, wi"···, stored: "fixed days Monday and Wednesday, no number of time"···, targetText: "\"2 days a week\"", __pickleIndex: "69", exampleTags: []) | @F004 @AC-013 @FR-002 @FR-006 | pending | 00:00:00.0750840 |
| F004 | Creating a goal with changed values keeps the changed values | @F004 @AC-006 @FR-002 @FR-010 @US1-AS6 | failed | 00:00:00.1770683 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "04", browserTimeZone: "the browser time zone `FLE Standard Time`", timeZone: "`UTC`", __pickleIndex: "225", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | failed | 00:00:00.0002365 |
| F004 | A goal on an Unpublished habit keeps working, and no new goal can use that habit(n: "03", actingUser: "`f4.ac060.owner.03@example.com`", action: "starts a new goal from `A4.060 03 Reading`", result: "the result is \"This habit is not in the catalog.\"···, __pickleIndex: "210", exampleTags: []) | @F004 @AC-060 @FR-016 @FR-034 @US7-AS6 | failed | 00:00:00.0895522 |
| F004 | A custom habit is looked up only among the acting user's custom habits(n: "05", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "an identifier that the test creates and that belon"···, __pickleIndex: "145", exampleTags: []) | @F004 @AC-033 @FR-008 @US3-AS5 | pending | 00:00:00.0796123 |
| F004 | Use cases check the acting user, the role, the identifier and the fields, in this order(n: "05", actingUser: "`f4.ac035.admin.05@example.com`", action: "creates a goal on `A4.035 05 Draft` with no Build "···, message: "\"Goals are for registered users.\"", __pickleIndex: "151", exampleTags: []) | @F004 @AC-035 @FR-023 | pending | 00:00:00.0775455 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "18", settings: "Reduce, yes/no, fixed days Monday to Friday", targetText: "\"avoided on Mon–Fri\"", __pickleIndex: "104", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0003389 |
| F004 | An administrator gets no goal use case(n: "05", action: "reads the evaluation calendar of `f4.ac031.user.05"···, __pickleIndex: "133", exampleTags: []) | @F004 @AC-031 @FR-021 @US3-AS3 | failed | 00:00:00.0769267 |
| F004 | A renamed catalog habit renames the goals based on it | @F004 @AC-062 @FR-010 @FR-016 @US7-AS7 | failed | 00:00:00.0819463 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "15", settings: "Build, yes/no, fixed days Monday, Wednesday and Fr"···, targetText: "\"3 days a week\"", __pickleIndex: "101", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | pending | 00:00:00.0003196 |
| F004 | Each field refusal of goal creation, with its message(n: "07", settings: "Build, count 1000 `pages`, 3 times per week", message: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "14", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0795341 |
| F004 | A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "01", schedule: "fixed days Sunday", date: "2027-03-28", entry: "2027-03-28 from `2027-03-27T22:00:00Z` to `2027-03"···, __pickleIndex: "164", exampleTags: []) | @F004 @AC-040 @FR-012 @FR-013 @US4-AS5 | failed | 00:00:00.0742981 |
| F004 | Unpublishing with confirmation unpublishes a habit that goals use | @F004 @AC-058 @FR-033 @US7-AS2 | failed | 00:00:00.2660763 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "12", browserTimeZone: "the browser time zone `Europe/Kiev`", timeZone: "`Europe/Kiev`", __pickleIndex: "233", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | failed | 00:00:00.0002378 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "16", browserTimeZone: "the browser time zone `UTC`", timeZone: "`UTC`", __pickleIndex: "237", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | failed | 00:00:00.0002538 |
| F004 | A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "06", schedule: "1 time per week", date: "2027-10-31", entry: "2027-10-25 to 2027-10-31 from `2027-10-24T21:00:00"···, __pickleIndex: "169", exampleTags: []) | @F004 @AC-040 @FR-012 @FR-013 @US4-AS5 | failed | 00:00:00.0750670 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "14", browserTimeZone: "the browser time zone `US/Eastern`", timeZone: "`US/Eastern`", __pickleIndex: "235", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | failed | 00:00:00.0002339 |
| F004 | An anonymous visitor gets no goal use case(n: "04", action: "reads `f4.ac032.user.04@example.com`'s goal", __pickleIndex: "138", exampleTags: []) | @F004 @AC-032 @FR-022 @US3-AS4 | failed | 00:00:00.0757174 |
| F004 | A habit's details give the user's goal count and their goals on it, in name order | @F004 @AC-054 @FR-031 @US6-AS3 | failed | 00:00:00.0955847 |
| F004 | A goal on an Unpublished habit keeps working, and no new goal can use that habit(n: "01", actingUser: "`f4.ac060.owner.01@example.com`", action: "lists their goals", result: "the list contains \"A4.060 01 Reading, 15 min, 5 d"···, __pickleIndex: "208", exampleTags: []) | @F004 @AC-060 @FR-016 @FR-034 @US7-AS6 | failed | 00:00:00.0826133 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "08", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "an identifier that the test creates and that belon"···, __pickleIndex: "79", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | pending | 00:00:00.0980632 |
| F004 | An account name has at most 60 characters | @F004 @AC-085 @FR-028 @US11-AS2 | failed | 00:00:00.0011268 |
| F004 | A goal without a name of its own is named after its habit and target(n: "01", name: "no name", goalName: "\"A4.016 01 Reading, 15 min, 5 days a week\"", own: "not its own", __pickleIndex: "82", exampleTags: []) | @F004 @AC-016 @FR-009 @FR-010 @US1-AS9 | pending | 00:00:00.0804618 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "06", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "`A4.014 06 Gone`", __pickleIndex: "77", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | pending | 00:00:00.0811556 |
| F004 | Without a name the greeting has no name | @F004 @AC-087 @FR-029 @US11-AS4 | pending | 00:00:00.0008153 |
| F004 | A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "02", schedule: "fixed days Sunday", date: "2027-10-31", entry: "2027-10-31 from `2027-10-30T21:00:00Z` to `2027-10"···, __pickleIndex: "165", exampleTags: []) | @F004 @AC-040 @FR-012 @FR-013 @US4-AS5 | failed | 00:00:00.0765661 |
| F004 | Each field refusal of goal creation, with its message(n: "02", settings: "Build, no measurement, 3 times per week", message: "\"Choose how it is measured.\"", __pickleIndex: "9", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0786368 |
| F004 | An anonymous visitor gets no goal use case(n: "03", action: "lists their goals", __pickleIndex: "137", exampleTags: []) | @F004 @AC-032 @FR-022 @US3-AS4 | failed | 00:00:00.0750424 |
| F004 | The edges of every target range are accepted(n: "02", settings: "Build, count 999 `pages`, 3 times per week", targetText: "\"999 pages, 3 times a week\"", __pickleIndex: "45", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | pending | 00:00:00.0757256 |
| F004 | Administrators read and change their own time zone; anonymous visitors cannot | @F004 @AC-079 @FR-026 @US10-AS8 | passed | 00:00:00.1110648 |
| F004 | Each goal follows a time zone change from its own next day or week(n: "02", schedule: "3 times per week", startDate: "2026-11-16", from: "2026-11-16", to: "2026-11-29", entries: "2026-11-16 to 2026-11-22 from `2026-11-15T22:00:00"···, __pickleIndex: "241", exampleTags: []) | @F004 @AC-072 @FR-026 @FR-027 @US10-AS5 | passed | 00:00:00.0931180 |
| F004 | "Only habits I track" returns the Published habits with the user's goals, with the other filters(n: "03", filter: "the category Body", result: "no habit", __pickleIndex: "193", exampleTags: []) | @F004 @AC-053 @FR-031 @US6-AS2 | failed | 00:00:00.0814758 |
| F004 | An account name is trimmed, and empty means no name(n: "03", name: "the name `\"   \"`", result: "no name", __pickleIndex: "275", exampleTags: []) | @F004 @AC-084 @FR-019 @FR-028 @US11-AS1 | failed | 00:00:00.0004236 |
| F004 | "Only habits I track" returns the Published habits with the user's goals, with the other filters(n: "02", filter: "the category Health", result: "`A4.053 02 Sugar` only", __pickleIndex: "192", exampleTags: []) | @F004 @AC-053 @FR-031 @US6-AS2 | failed | 00:00:00.0840154 |
| F004 | A time zone is given only to its own account | @F004 @AC-080 @FR-024 @US10-AS9 | failed | 00:00:00.0007955 |
| F004 | A user with no goals gets an empty list | @F004 @AC-027 @FR-015 @US2-AS5 | pending | 00:00:00.0009053 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "01", action: "starts a new goal from", habit: "`A4.014 01 Draft`", __pickleIndex: "72", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | pending | 00:00:00.0855674 |
| F004 | Demo loading results name the demo email but never a goal, habit or account name | @F004 @AC-068 @FR-040 @US9-AS4 | passed | 00:00:00.3870337 |
| F004 | Each field refusal of goal creation, with its message(n: "04", settings: "Build, yes/no with the count unit `pages`, fixed d"···, message: "\"A yes/no goal has no amount or unit.\"", __pickleIndex: "11", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0807542 |
| F004 | The profile gives "member since" with the month and year of creation in the account's time zone(n: "01", time: "`2026-11-17T10:00:00Z`", browserTimeZone: "the browser time zone `Europe/Kyiv`", memberSince: "\"member since November 2026\"", __pickleIndex: "289", exampleTags: []) | @F004 @AC-089 @FR-030 @US11-AS6 | passed | 00:00:00.0782077 |
| F004 | A Reduce goal with an amount has 1 time per period(n: "05", settings: "Build, count 20 `pages`, 3 times per day", result: "creating succeeds", __pickleIndex: "64", exampleTags: []) | @F004 @AC-012 @FR-004 @FR-006 | pending | 00:00:00.0772191 |
| F004 | Each field refusal of goal creation, with its message(n: "21", settings: "Build, yes/no, 3 times with no period", message: "\"Choose per day or per week.\"", __pickleIndex: "28", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0806454 |
| F004 | A per-week goal tells when its first week starts(n: "01", startDate: "2026-11-18", time: "`2026-11-18T10:00:00Z`", list: "first week starts 2026-11-23, no start date and no"···, __pickleIndex: "160", exampleTags: []) | @F004 @AC-039 @FR-014 @FR-016 @US4-AS4 | pending | 00:00:00.0002915 |
| F004 | An account name is given only to its own account; administrators have their own | @F004 @AC-090 @FR-028 @FR-029 @US11-AS7 | failed | 00:00:00.0010062 |
| F004 | A per-week goal has one evaluation period per Monday-to-Sunday week, from its first full week(n: "04", startDate: "2026-11-23", from: "2026-11-23", to: "2026-11-29", periods: "1 period: 2026-11-23 to 2026-11-29 from `2026-11-2"···, __pickleIndex: "159", exampleTags: []) | @F004 @AC-038 @FR-013 @US4-AS3 | pending | 00:00:00.0012511 |
| F004 | The next commitment is the first commitment that has not ended(n: "01", time: "`2026-11-16T08:00:00Z`", next: "2026-11-16", __pickleIndex: "114", exampleTags: []) | @F004 @AC-023 @FR-014 @US2-AS2 | pending | 00:00:00.0007991 |
| F004 | Each field refusal of goal creation, with its message(n: "10", settings: "Reduce, count 1000 `cups`, 1 time per day", message: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "17", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0784055 |
| F004 | Each field refusal of goal creation, with its message(n: "12", settings: "Build, count 20 with the unit `\"   \"`, 3 times p"···, message: "\"Enter a unit, for example pages.\"", __pickleIndex: "19", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0794512 |
| F004 | The profile gives "member since" with the month and year of creation in the account's time zone(n: "03", time: "`2026-11-30T22:30:00Z`", browserTimeZone: "no browser time zone", memberSince: "\"member since November 2026\"", __pickleIndex: "291", exampleTags: []) | @F004 @AC-089 @FR-030 @US11-AS6 | passed | 00:00:00.0866074 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "10", goal: "on `A4.008 10 Reading` with Build, yes/no, 21 time"···, message: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "39", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | pending | 00:00:00.0832778 |
| F004 | A custom habit is looked up only among the acting user's custom habits(n: "02", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "`f4.ac033.b.02@example.com`'s custom habit `Cold s"···, __pickleIndex: "142", exampleTags: []) | @F004 @AC-033 @FR-008 @US3-AS5 | pending | 00:00:00.0779866 |
| F004 | Demo goals are left out when their catalog habit is not Published | @F004 @AC-066 @FR-040 @US9-AS3 | passed | 00:00:00.5172761 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "02", value: "`FLE Standard Time`", __pickleIndex: "250", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.1047585 |
| F004 | Demo loading run again gives existing demo accounts no goals | @F004 @AC-065 @FR-040 @US9-AS2 | failed | 00:00:00.1818605 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "08", value: "`\"\"`", __pickleIndex: "256", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.0968233 |
| F004 | Several goals on one habit are independent | @F004 @AC-017 @FR-001 @US1-AS10 | failed | 00:00:00.0963437 |
| F004 | Each field refusal of goal creation, with its message(n: "14", settings: "Build, count 20 with a unit of 21 characters, 3 ti"···, message: "\"The unit can have at most 20 characters.\"", __pickleIndex: "21", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0776491 |
| F004 | The start date is today or up to 365 days later(n: "01", startDate: "the start date 2026-11-18", result: "creating succeeds", __pickleIndex: "52", exampleTags: []) | @F004 @AC-010 @FR-006 @FR-012 | pending | 00:00:00.0840141 |
| F004 | Reading a custom habit gives the number of goals that use it | @F004 @AC-051 @FR-035 | pending | 00:00:00.0008280 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "04", action: "starts a new goal from", habit: "an identifier that the test creates and that belon"···, __pickleIndex: "75", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | pending | 00:00:00.0825005 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "11", goal: "on `A4.008 11 Reading` with Reduce, count 2 `cups`"···, message: "\"A Reduce goal with an amount has 1 time per peri"···, __pickleIndex: "40", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | pending | 00:00:00.6962971 |
| F004 | A second change replaces a version not yet begun, and follows a version already in effect(n: "02", schedule: "1 time per day", startDate: "2026-11-16", from: "2026-11-18", to: "2026-11-20", versions: "the time zone `Asia/Tokyo` from 2026-11-20 and no "···, entries: "2026-11-18 from `2026-11-17T22:00:00Z` to `2026-11"···, __pickleIndex: "269", exampleTags: []) | @F004 @AC-081 @FR-026 @US10-AS10 | passed | 00:00:00.1229193 |
| F004 | Each field refusal of goal creation, with its message(n: "09", settings: "Reduce, count -1 `cups`, 1 time per day", message: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "16", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | pending | 00:00:00.0815753 |
| F004 | "Today" follows the new time zone at once | @F004 @AC-076 @FR-012 @FR-026 | passed | 00:00:00.1277888 |
| F004 | Reading a goal gives the version of its current date and the one later version(n: "05", schedule: "3 times per week", startDate: "2026-11-16", time: "`2026-11-18T12:00:00Z`", versions: "the time zone `Europe/Kyiv` from 2026-11-16 and a "···, __pickleIndex: "247", exampleTags: []) | @F004 @AC-073 @FR-016 @FR-026 @US10-AS6 | passed | 00:00:00.0938790 |
| F004 | A per-week goal tells when its first week starts(n: "02", startDate: "2026-11-18", time: "`2026-11-22T23:59:59Z`", list: "first week starts 2026-11-23, no start date and no"···, __pickleIndex: "161", exampleTags: []) | @F004 @AC-039 @FR-014 @FR-016 @US4-AS4 | pending | 00:00:00.0003300 |
| F004 | Anonymous visitors and administrators get the catalog without goal information(n: "03", actingUser: "the anonymous visitor", action: "reads the details of `A4.055 03 Reading` as the an"···, result: "the details have no goal count and no goals", __pickleIndex: "197", exampleTags: []) | @F004 @AC-055 @FR-032 @US6-AS4 | failed | 00:00:00.0809559 |
| F004 | Another user's goal does not exist for them(n: "02", action: "reads the evaluation calendar from 2026-11-18 to 2"···, goal: "`f4.ac029.a.02@example.com`'s goal", __pickleIndex: "125", exampleTags: []) | @F004 @AC-029 @FR-020 @US3-AS1 | failed | 00:00:00.0746073 |
| F004 | A per-week goal tells when its first week starts(n: "04", startDate: "2026-11-23", time: "`2026-11-18T10:00:00Z`", list: "first week starts 2026-11-23, start date 2026-11-2"···, __pickleIndex: "163", exampleTags: []) | @F004 @AC-039 @FR-014 @FR-016 @US4-AS4 | pending | 00:00:00.0011944 |
| F004 | The edges of every target range are accepted(n: "06", settings: "Build, yes/no, 1 time per week", targetText: "\"once a week\"", __pickleIndex: "49", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | pending | 00:00:00.0777249 |

### F004/AC-086 The Home greeting follows the time of day in the account's time zone(n: "04", time: "`2026-11-18T10:00:00Z` (12:00 in Kyiv)", greeting: "\"Good afternoon, Yuri\"", __pickleIndex: "281", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac086.user.04@example.com` with the password `walking42`, the browser time zone `Europe/Kyiv` and the name "Yuri", and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When at `2026-11-18T10:00:00Z` (12:00 in Kyiv) `f4.ac086.user.04@example.com` reads the Home heading
-> skipped because of previous errors
Then the greeting is "Good afternoon, Yuri" and the date is "Wednesday, 18 November"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheHomeGreetingFollowsTheTimeOfDayInTheAccountsTimeZone(String n, String time, String greeting, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 1056
[non-AC frame(s) hidden]
```

### F004/AC-046 Starting from a custom habit gives only Build or Reduce from its intent(n: "01", intent: "Build", __pickleIndex: "181", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac046.user.01@example.com`, who created the custom habit `Cold shower` with the intent Build
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Cold shower` with the intent Build")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitColdShowerWithTheIntentBuild(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac046.user.01@example.com` starts a new goal from their custom habit `Cold shower`
-> skipped because of previous errors
Then the starting values are: the habit `Cold shower` (custom), Build, the start date 2026-11-18, and nothing else: no measurement, no amount, no unit, no schedule, no name and no catalog suggestion text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.StartingFromACustomHabitGivesOnlyBuildOrReduceFromItsIntent(String n, String intent, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 602
[non-AC frame(s) hidden]
```

### F004/AC-008 Goal creation gives the first refusal in FR-006 order(n: "12", goal: "on `A4.008 12 Reading` with Build, yes/no, fixed d"···, message: "\"Choose a start date.\"", __pickleIndex: "41", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac008.admin.12@example.com`, a Published `A4.008 12 Reading` and a Draft `A4.008 12 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac008.admin.12...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac008.user.12@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac008.user.12@example.com` creates a goal on `A4.008 12 Reading` with Build, yes/no, fixed days Monday, no start date and a name of 61 characters
-> skipped because of previous errors
Then the result is "Choose a start date."
-> skipped because of previous errors
And `f4.ac008.user.12@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.GoalCreationGivesTheFirstRefusalInFR_006Order(String n, String goal, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 100
[non-AC frame(s) hidden]
```

### F004/AC-011 A goal name has at most 60 characters after trimming(n: "02", name: "a text of 61 characters", result: "\"The name can have at most 60 characters.\"", __pickleIndex: "58", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac011.admin.02@example.com` and a Published `A4.011 02 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac011.admin.02...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac011.user.02@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac011.user.02@example.com` creates a goal on `A4.011 02 Reading` with Build, yes/no, fixed days Monday and the name a text of 61 characters
-> skipped because of previous errors
Then the result is "The name can have at most 60 characters."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AGoalNameHasAtMost60CharactersAfterTrimming(String n, String name, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 158
[non-AC frame(s) hidden]
```

### F004/AC-057 Unpublishing a habit that goals use needs confirmation and gives only the number of goals(n: "03", goals: "`f4.ac057.a.03@example.com` created two goals and "···, count: "3", text: "\"3 goals use this habit.\"", __pickleIndex: "204", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac057.admin.03@example.com` and a Published `A4.057 03 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac057.admin.03...", "a Published `A4.0...") (0,1s)
And the registered users `f4.ac057.a.03@example.com` and `f4.ac057.b.03@example.com`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUsers(String, String, String, String)' should have 2 parameters
And `f4.ac057.a.03@example.com` created two goals and `f4.ac057.b.03@example.com` created one goal, each on `A4.057 03 Reading` with Build, yes/no, fixed days Monday
-> skipped because of previous errors
When `f4.ac057.admin.03@example.com` unpublishes `A4.057 03 Reading` without confirming
-> skipped because of previous errors
Then the result is "confirmation needed" with the number of goals 3 and the text "3 goals use this habit."
-> skipped because of previous errors
And the result carries nothing else: no email, no goal name, no goal identifier and no setting
-> skipped because of previous errors
And browsing the catalog without an acting user with the search `A4.057 03` still contains `A4.057 03 Reading`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUsers(String, String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.UnpublishingAHabitThatGoalsUseNeedsConfirmationAndGivesOnlyTheNumberOfGoals(String n, String goals, String count, String text, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 715
[non-AC frame(s) hidden]
```

### F004/AC-014 A habit that is not a Published catalog habit is refused as a catalog habit(n: "05", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "`A4.014 05 Draft`", __pickleIndex: "76", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac014.admin.05@example.com`, a Draft `A4.014 05 Draft` and an Unpublished `A4.014 05 Gone`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac014.admin.05...", "a Draft `A4.014 0...") (0,1s)
And the registered user `f4.ac014.user.05@example.com`, who created the custom habit `Cold shower`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Cold shower`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitColdShower(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac014.user.05@example.com` creates a goal with Build, yes/no, fixed days Monday on `A4.014 05 Draft` given as a catalog habit
-> skipped because of previous errors
Then the result is "This habit is not in the catalog."
-> skipped because of previous errors
And `f4.ac014.user.05@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AHabitThatIsNotAPublishedCatalogHabitIsRefusedAsACatalogHabit(String n, String action, String habit, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 205
[non-AC frame(s) hidden]
```

### F004/AC-070 Registration keeps a named browser time zone, and otherwise gives the default(n: "09", browserTimeZone: "the browser time zone `+02:00`", timeZone: "`UTC`", __pickleIndex: "230", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac070.user.09@example.com` with the password `walking42` and the browser time zone `+02:00`, and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When `f4.ac070.user.09@example.com` reads their time zone
-> skipped because of previous errors
Then registration succeeded and the time zone is `UTC`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.RegistrationKeepsANamedBrowserTimeZoneAndOtherwiseGivesTheDefault(String n, String browserTimeZone, String timeZone, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 846
[non-AC frame(s) hidden]
```

### F004/AC-092 A goal keeps the values it was created with, whatever happens to the suggested starting goal — failed

Step trace:
```
Given the administrator `f4.ac092.admin@example.com` and a Published `A4.092 Reading` with the suggested starting goal Build, duration 15 minutes, fixed days Monday to Friday
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac092.admin@ex...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac092.user@example.com`, who started a new goal from `A4.092 Reading`, and the starting values were Build, duration 15 minutes, fixed days Monday to Friday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And `f4.ac092.admin@example.com` edited `A4.092 Reading` so that its only suggested starting goal is Build, count 20 `pages`, 3 times per week
-> skipped because of previous errors
And `f4.ac092.user@example.com` created a goal on `A4.092 Reading` with Build, duration 15 minutes, fixed days Monday to Friday
-> skipped because of previous errors
And `f4.ac092.admin@example.com` edited `A4.092 Reading` so that it has no suggested starting goal
-> skipped because of previous errors
When `f4.ac092.user@example.com` reads the goal
-> skipped because of previous errors
Then the result has Build, duration 15 minutes, fixed days Monday to Friday and the target text "15 min, 5 days a week"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AGoalKeepsTheValuesItWasCreatedWithWhateverHappensToTheSuggestedStartingGoal() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 1126
[non-AC frame(s) hidden]
```

### F004/AC-053 "Only habits I track" returns the Published habits with the user's goals, with the other filters(n: "01", filter: "no category", result: "`A4.053 01 Reading` and `A4.053 01 Sugar`, and not"···, __pickleIndex: "191", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac053.admin.01@example.com`, a Published `A4.053 01 Reading` with the category Mind, a Published `A4.053 01 Yoga` with the category Body, a Published `A4.053 01 Sugar` with the category Health and a Published `A4.053 01 Coffee` with the category Health
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac053.admin.01...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac053.user.01@example.com`, who created two goals on `A4.053 01 Reading`, one goal on `A4.053 01 Sugar` and one goal on `A4.053 01 Coffee`, each with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And the administrator unpublished `A4.053 01 Coffee` with confirmation
-> skipped because of previous errors
When `f4.ac053.user.01@example.com` browses the catalog as `f4.ac053.user.01@example.com` with "Only habits I track", the search `A4.053 01` and no category
-> skipped because of previous errors
Then the result is `A4.053 01 Reading` and `A4.053 01 Sugar`, and nothing else
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.OnlyHabitsITrackReturnsThePublishedHabitsWithTheUsersGoalsWithTheOtherFilters(String n, String filter, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 667
[non-AC frame(s) hidden]
```

### F004/AC-024 A goal that has not started gives its start date — pending

Step trace:
```
Given the registered user `f4.ac024.user@example.com`, who created the custom habits `Water` and `Stretch`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabits().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the custom habits `Water` and `Stretch`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheCustomHabitsWaterAndStretch(int p0, int p1)
{
    throw new PendingStepException();
}

And `f4.ac024.user@example.com` created a goal on their custom habit `Water` with Build, yes/no, 1 time per day and the start date 2026-11-25
-> skipped because of previous errors
And `f4.ac024.user@example.com` created a goal on their custom habit `Stretch` with Build, yes/no, fixed days Monday, Wednesday and Friday and the start date 2026-11-24
-> skipped because of previous errors
When `f4.ac024.user@example.com` lists their goals
-> skipped because of previous errors
Then "Water, once a day" gives start date 2026-11-25 and no next commitment
-> skipped because of previous errors
And "Stretch, 3 days a week" gives start date 2026-11-24 and next commitment 2026-11-25
-> skipped because of previous errors
And listed at `2026-11-25T00:00:00Z`, "Water, once a day" gives no start date, no next commitment and no first week starts
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AGoalThatHasNotStartedGivesItsStartDate() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 347
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "13", settings: "Build, count 20 with no unit, 3 times per week", message: "\"Enter a unit, for example pages.\"", __pickleIndex: "20", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.13@example.com` and a Published `A4.007 13 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.13...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.13@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.13@example.com` creates a goal on `A4.007 13 Reading` with Build, count 20 with no unit, 3 times per week
-> skipped because of previous errors
Then the result is "Enter a unit, for example pages."
-> skipped because of previous errors
And `f4.ac007.user.13@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-016 A goal without a name of its own is named after its habit and target(n: "03", name: "the name \"  Morning pages  \"", goalName: "\"Morning pages\"", own: "its own", __pickleIndex: "84", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac016.admin.03@example.com` and a Published `A4.016 03 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac016.admin.03...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac016.user.03@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac016.user.03@example.com` creates a goal on `A4.016 03 Reading` with Build, duration 15 minutes, fixed days Monday to Friday and the name "  Morning pages  "
-> skipped because of previous errors
Then reading the goal gives the name "Morning pages", and the name is its own
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AGoalWithoutANameOfItsOwnIsNamedAfterItsHabitAndTarget(String n, String name, String goalName, String own, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 238
[non-AC frame(s) hidden]
```

### F004/AC-031 An administrator gets no goal use case(n: "03", action: "lists their goals", __pickleIndex: "131", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac031.admin.03@example.com` and a Published `A4.031 03 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac031.admin.03...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac031.user.03@example.com`, who created a goal on `A4.031 03 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When `f4.ac031.admin.03@example.com` lists their goals
-> skipped because of previous errors
Then the result is "Goals are for registered users." and no goal is returned
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnAdministratorGetsNoGoalUseCase(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 409
[non-AC frame(s) hidden]
```

### F004/AC-008 Goal creation gives the first refusal in FR-006 order(n: "07", goal: "on `A4.008 07 Reading` with Build, count 20 with a"···, message: "\"The unit can have at most 20 characters.\"", __pickleIndex: "36", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac008.admin.07@example.com`, a Published `A4.008 07 Reading` and a Draft `A4.008 07 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac008.admin.07...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac008.user.07@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac008.user.07@example.com` creates a goal on `A4.008 07 Reading` with Build, count 20 with a unit of 21 characters, no schedule
-> skipped because of previous errors
Then the result is "The unit can have at most 20 characters."
-> skipped because of previous errors
And `f4.ac008.user.07@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.GoalCreationGivesTheFirstRefusalInFR_006Order(String n, String goal, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 100
[non-AC frame(s) hidden]
```

### F004/AC-070 Registration keeps a named browser time zone, and otherwise gives the default(n: "06", browserTimeZone: "the browser time zone `EST`", timeZone: "`UTC`", __pickleIndex: "227", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac070.user.06@example.com` with the password `walking42` and the browser time zone `EST`, and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When `f4.ac070.user.06@example.com` reads their time zone
-> skipped because of previous errors
Then registration succeeded and the time zone is `UTC`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.RegistrationKeepsANamedBrowserTimeZoneAndOtherwiseGivesTheDefault(String n, String browserTimeZone, String timeZone, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 846
[non-AC frame(s) hidden]
```

### F004/AC-031 An administrator gets no goal use case(n: "01", action: "starts a new goal from `A4.031 01 Reading`", __pickleIndex: "129", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac031.admin.01@example.com` and a Published `A4.031 01 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac031.admin.01...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac031.user.01@example.com`, who created a goal on `A4.031 01 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When `f4.ac031.admin.01@example.com` starts a new goal from `A4.031 01 Reading`
-> skipped because of previous errors
Then the result is "Goals are for registered users." and no goal is returned
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnAdministratorGetsNoGoalUseCase(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 409
[non-AC frame(s) hidden]
```

### F004/AC-011 A goal name has at most 60 characters after trimming(n: "03", name: "two spaces, a text of 60 characters and two spaces", result: "creating succeeds, and reading the goal gives the "···, __pickleIndex: "59", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac011.admin.03@example.com` and a Published `A4.011 03 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac011.admin.03...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac011.user.03@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac011.user.03@example.com` creates a goal on `A4.011 03 Reading` with Build, yes/no, fixed days Monday and the name two spaces, a text of 60 characters and two spaces
-> skipped because of previous errors
Then the result is creating succeeds, and reading the goal gives the text of 60 characters as its own name
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AGoalNameHasAtMost60CharactersAfterTrimming(String n, String name, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 158
[non-AC frame(s) hidden]
```

### F004/AC-035 Use cases check the acting user, the role, the identifier and the fields, in this order(n: "04", actingUser: "the anonymous visitor", action: "creates a goal on `A4.035 04 Draft` with no Build "···, message: "\"You are not signed in.\"", __pickleIndex: "150", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac035.admin.04@example.com` and a Draft `A4.035 04 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac035.admin.04...", "a Draft `A4.035 0...") (0,1s)
And the registered user `f4.ac035.user.04@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When the anonymous visitor creates a goal on `A4.035 04 Draft` with no Build or Reduce
-> skipped because of previous errors
Then the result is "You are not signed in."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.UseCasesCheckTheActingUserTheRoleTheIdentifierAndTheFieldsInThisOrder(String n, String actingUser, String action, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 467
[non-AC frame(s) hidden]
```

### F004/AC-032 An anonymous visitor gets no goal use case(n: "01", action: "starts a new goal from `A4.032 01 Reading`", __pickleIndex: "135", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac032.admin.01@example.com` and a Published `A4.032 01 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac032.admin.01...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac032.user.01@example.com`, who created a goal on `A4.032 01 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When the anonymous visitor starts a new goal from `A4.032 01 Reading`
-> skipped because of previous errors
Then the result is "You are not signed in."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnAnonymousVisitorGetsNoGoalUseCase(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 425
[non-AC frame(s) hidden]
```

### F004/AC-009 The edges of every target range are accepted(n: "07", settings: "Build, yes/no, 20 times per week", targetText: "\"20 times a week\"", __pickleIndex: "50", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac009.admin.07@example.com` and a Published `A4.009 07 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac009.admin.07...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac009.user.07@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac009.user.07@example.com` creates a goal on `A4.009 07 Reading` with Build, yes/no, 20 times per week
-> skipped because of previous errors
Then creating succeeds
-> skipped because of previous errors
And reading the goal gives the target text "20 times a week"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheEdgesOfEveryTargetRangeAreAccepted(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 125
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "17", settings: "Build, yes/no, fixed days with no day", message: "\"Choose at least one day.\"", __pickleIndex: "24", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.17@example.com` and a Published `A4.007 17 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.17...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.17@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.17@example.com` creates a goal on `A4.007 17 Reading` with Build, yes/no, fixed days with no day
-> skipped because of previous errors
Then the result is "Choose at least one day."
-> skipped because of previous errors
And `f4.ac007.user.17@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-070 Registration keeps a named browser time zone, and otherwise gives the default(n: "11", browserTimeZone: "the browser time zone `\"\"`", timeZone: "`UTC`", __pickleIndex: "232", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac070.user.11@example.com` with the password `walking42` and the browser time zone `""`, and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When `f4.ac070.user.11@example.com` reads their time zone
-> skipped because of previous errors
Then registration succeeded and the time zone is `UTC`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.RegistrationKeepsANamedBrowserTimeZoneAndOtherwiseGivesTheDefault(String n, String browserTimeZone, String timeZone, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 846
[non-AC frame(s) hidden]
```

### F004/AC-011 A goal name has at most 60 characters after trimming(n: "01", name: "a text of 60 characters", result: "creating succeeds, and reading the goal gives that"···, __pickleIndex: "57", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac011.admin.01@example.com` and a Published `A4.011 01 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac011.admin.01...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac011.user.01@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac011.user.01@example.com` creates a goal on `A4.011 01 Reading` with Build, yes/no, fixed days Monday and the name a text of 60 characters
-> skipped because of previous errors
Then the result is creating succeeds, and reading the goal gives that name as its own
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AGoalNameHasAtMost60CharactersAfterTrimming(String n, String name, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 158
[non-AC frame(s) hidden]
```

### F004/AC-008 Goal creation gives the first refusal in FR-006 order(n: "14", goal: "on `A4.008 14 Reading` with Build, yes/no, fixed d"···, message: "\"Choose a start date within a year from today.\"", __pickleIndex: "43", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac008.admin.14@example.com`, a Published `A4.008 14 Reading` and a Draft `A4.008 14 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac008.admin.14...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac008.user.14@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac008.user.14@example.com` creates a goal on `A4.008 14 Reading` with Build, yes/no, fixed days Monday, the start date 2027-11-19 and a name of 61 characters
-> skipped because of previous errors
Then the result is "Choose a start date within a year from today."
-> skipped because of previous errors
And `f4.ac008.user.14@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.GoalCreationGivesTheFirstRefusalInFR_006Order(String n, String goal, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 100
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "21", settings: "Reduce, yes/no, fixed days Monday, Tuesday, Wednes"···, targetText: "\"avoided on Mon–Wed, Sat\"", __pickleIndex: "107", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.21@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.21@example.com` creates a goal on their custom habit `Practice` with Reduce, yes/no, fixed days Monday, Tuesday, Wednesday and Saturday
-> skipped because of previous errors
Then reading the goal gives the target text "avoided on Mon–Wed, Sat" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-052 Browsing gives a registered user their goal count per habit and the tracked count(n: "03", search: "`A4.052 03 Y`", result: "the result is `A4.052 03 Yoga` only, with the goal"···, __pickleIndex: "190", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac052.admin.03@example.com`, a Published `A4.052 03 Reading`, a Published `A4.052 03 Yoga` and a Published `A4.052 03 Water`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac052.admin.03...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac052.user.03@example.com`, who created two goals on `A4.052 03 Reading` and one goal on `A4.052 03 Water`, each with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When `f4.ac052.user.03@example.com` browses the catalog as `f4.ac052.user.03@example.com` with the search `A4.052 03 Y`
-> skipped because of previous errors
Then the result is `A4.052 03 Yoga` only, with the goal count 0, and the tracked count is 0
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.BrowsingGivesARegisteredUserTheirGoalCountPerHabitAndTheTrackedCount(String n, String search, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 653
[non-AC frame(s) hidden]
```

### F004/AC-055 Anonymous visitors and administrators get the catalog without goal information(n: "04", actingUser: "`f4.ac055.admin.04@example.com`", action: "reads the details of `A4.055 04 Reading` as `f4.ac"···, result: "the details have no goal count and no goals", __pickleIndex: "198", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac055.admin.04@example.com` and a Published `A4.055 04 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac055.admin.04...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac055.user.04@example.com`, who created a goal on `A4.055 04 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When `f4.ac055.admin.04@example.com` reads the details of `A4.055 04 Reading` as `f4.ac055.admin.04@example.com`
-> skipped because of previous errors
Then the details have no goal count and no goals
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnonymousVisitorsAndAdministratorsGetTheCatalogWithoutGoalInformation(String n, String actingUser, String action, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 688
[non-AC frame(s) hidden]
```

### F004/AC-050 Editing a custom habit renames its goals but never changes their settings — pending

Step trace:
```
Given the registered user `f4.ac050.user@example.com`, who created the custom habit `Cold shower` with the intent Build
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the custom habit `Cold shower` with the intent Build")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheCustomHabitColdShowerWithTheIntentBuild(int p0, int p1)
{
    throw new PendingStepException();
}

And `f4.ac050.user@example.com` created a goal on their custom habit `Cold shower` with Build, yes/no, fixed days on all seven days, and a second one with the same settings and the name "Morning cold"
-> skipped because of previous errors
When `f4.ac050.user@example.com` edits their custom habit `Cold shower` to the name `Cold water` and the intent Reduce
-> skipped because of previous errors
Then reading the first goal gives the name "Cold water, daily", the habit name `Cold water`, Build, yes/no and fixed days on all seven days
-> skipped because of previous errors
And reading the second goal gives the name "Morning cold", the habit name `Cold water` and Build
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EditingACustomHabitRenamesItsGoalsButNeverChangesTheirSettings() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 639
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "01", settings: "no Build or Reduce, count 20 `pages`, 3 times per "···, message: "\"Choose Build or Reduce.\"", __pickleIndex: "8", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.01@example.com` and a Published `A4.007 01 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.01...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.01@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.01@example.com` creates a goal on `A4.007 01 Reading` with no Build or Reduce, count 20 `pages`, 3 times per week
-> skipped because of previous errors
Then the result is "Choose Build or Reduce."
-> skipped because of previous errors
And `f4.ac007.user.01@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-015 A habit that changed between starting and creating is looked up again(n: "01", habit: "`A4.015 01 Reading`", change: "the administrator unpublished `A4.015 01 Reading` "···, message: "\"This habit is not in the catalog.\"", __pickleIndex: "80", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac015.admin.01@example.com` and a Published `A4.015 01 Reading` with the suggested starting goal Build, duration 15 minutes, fixed days Monday to Friday
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac015.admin.01...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac015.user.01@example.com`, who created the custom habit `Cold shower`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Cold shower`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitColdShower(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And `f4.ac015.user.01@example.com` started a new goal from `A4.015 01 Reading`
-> skipped because of previous errors
And the administrator unpublished `A4.015 01 Reading` without confirming
-> skipped because of previous errors
When `f4.ac015.user.01@example.com` creates a goal on `A4.015 01 Reading` with Build, duration 15 minutes, fixed days Monday to Friday
-> skipped because of previous errors
Then the result is "This habit is not in the catalog."
-> skipped because of previous errors
And `f4.ac015.user.01@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AHabitThatChangedBetweenStartingAndCreatingIsLookedUpAgain(String n, String habit, String change, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 226
[non-AC frame(s) hidden]
```

### F004/AC-070 Registration keeps a named browser time zone, and otherwise gives the default(n: "10", browserTimeZone: "the browser time zone `europe/kyiv`", timeZone: "`UTC`", __pickleIndex: "231", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac070.user.10@example.com` with the password `walking42` and the browser time zone `europe/kyiv`, and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When `f4.ac070.user.10@example.com` reads their time zone
-> skipped because of previous errors
Then registration succeeded and the time zone is `UTC`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.RegistrationKeepsANamedBrowserTimeZoneAndOtherwiseGivesTheDefault(String n, String browserTimeZone, String timeZone, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 846
[non-AC frame(s) hidden]
```

### F004/AC-012 A Reduce goal with an amount has 1 time per period(n: "02", settings: "Reduce, duration 30 minutes, 2 times per week", result: "\"A Reduce goal with an amount has 1 time per peri"···, __pickleIndex: "61", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac012.admin.02@example.com` and a Published `A4.012 02 Coffee`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac012.admin.02...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac012.user.02@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac012.user.02@example.com` creates a goal on `A4.012 02 Coffee` with Reduce, duration 30 minutes, 2 times per week
-> skipped because of previous errors
Then the result is "A Reduce goal with an amount has 1 time per period."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AReduceGoalWithAnAmountHas1TimePerPeriod(String n, String settings, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 171
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "11", settings: "Reduce, duration with no amount and the unit minut"···, message: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "18", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.11@example.com` and a Published `A4.007 11 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.11...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.11@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.11@example.com` creates a goal on `A4.007 11 Reading` with Reduce, duration with no amount and the unit minutes, 1 time per day
-> skipped because of previous errors
Then the result is "Enter a whole number from 0 to 999."
-> skipped because of previous errors
And `f4.ac007.user.11@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "20", settings: "Build, yes/no, 21 times per week", message: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "27", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.20@example.com` and a Published `A4.007 20 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.20...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.20@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.20@example.com` creates a goal on `A4.007 20 Reading` with Build, yes/no, 21 times per week
-> skipped because of previous errors
Then the result is "Enter a number of times from 1 to 20."
-> skipped because of previous errors
And `f4.ac007.user.20@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-052 Browsing gives a registered user their goal count per habit and the tracked count(n: "02", search: "`A4.052 02 R`", result: "the result is `A4.052 02 Reading` only, with the g"···, __pickleIndex: "189", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac052.admin.02@example.com`, a Published `A4.052 02 Reading`, a Published `A4.052 02 Yoga` and a Published `A4.052 02 Water`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac052.admin.02...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac052.user.02@example.com`, who created two goals on `A4.052 02 Reading` and one goal on `A4.052 02 Water`, each with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When `f4.ac052.user.02@example.com` browses the catalog as `f4.ac052.user.02@example.com` with the search `A4.052 02 R`
-> skipped because of previous errors
Then the result is `A4.052 02 Reading` only, with the goal count 2, and the tracked count is 1
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.BrowsingGivesARegisteredUserTheirGoalCountPerHabitAndTheTrackedCount(String n, String search, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 653
[non-AC frame(s) hidden]
```

### F004/AC-019 The catalog suggestion text of the starting values follows the goal rules — failed

Step trace:
```
Given the administrator `f4.ac019.admin@example.com` and the registered user `f4.ac019.user@example.com`
-> done: ArrangeSteps.GivenTheAdministratorAndTheRegisteredUser("f4.ac019.admin@ex...", "f4.ac019.user@exa...") (0,1s)
And a Published `A4.019 Phone` with the intent Reduce and the suggested starting goal Reduce, yes/no, fixed days Monday to Friday
-> done: ArrangeSteps.GivenCatalogHabits("a Published `A4.0...") (0,0s)
When `f4.ac019.user@example.com` starts a new goal from `A4.019 Phone`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.GoalWhenSteps.WhenStartsANewGoal(String, String, String, String)' should have 2 parameters
Then the starting values are Reduce, yes/no, fixed days Monday to Friday, with the catalog suggestion text "avoided on Mon–Fri"
-> skipped because of previous errors
And reading the details of `A4.019 Phone` without an acting user still gives that suggested starting goal the text "at most 5 days a week"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.GoalWhenSteps.WhenStartsANewGoal(String, String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheCatalogSuggestionTextOfTheStartingValuesFollowsTheGoalRules() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 294
[non-AC frame(s) hidden]
```

### F004/AC-036 A fixed-days goal has one commitment per scheduled day, from midnight to midnight in its time zone — failed

Step trace:
```
Given the registered user `f4.ac036.user@example.com` in `Europe/Kyiv`, who created the custom habit `Yoga`
-> done: ArrangeSteps.GivenTheRegisteredUser("f4.ac036.user@exa...", "Europe/Kyiv", "created the custo...") (0,1s)
And `f4.ac036.user@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days Monday, Wednesday and Friday and the start date 2026-11-18
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenCreatedAGoal(String, String, String, String)' should have 3 parameters
When `f4.ac036.user@example.com` reads the evaluation calendar of the goal from 2026-11-16 to 2026-11-29
-> skipped because of previous errors
Then it has 5 commitments, in this order:
-> skipped because of previous errors
And 2026-11-18 from `2026-11-17T22:00:00Z` to `2026-11-18T22:00:00Z`
-> skipped because of previous errors
And 2026-11-20 from `2026-11-19T22:00:00Z` to `2026-11-20T22:00:00Z`
-> skipped because of previous errors
And 2026-11-23 from `2026-11-22T22:00:00Z` to `2026-11-23T22:00:00Z`
-> skipped because of previous errors
And 2026-11-25 from `2026-11-24T22:00:00Z` to `2026-11-25T22:00:00Z`
-> skipped because of previous errors
And 2026-11-27 from `2026-11-26T22:00:00Z` to `2026-11-27T22:00:00Z`
-> skipped because of previous errors
And no entry has a status
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenCreatedAGoal(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AFixed_DaysGoalHasOneCommitmentPerScheduledDayFromMidnightToMidnightInItsTimeZone() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 490
[non-AC frame(s) hidden]
```

### F004/AC-003 A catalog habit without suggested starting goals gives only Build or Reduce from its intent — failed

Step trace:
```
Given the administrator `f4.ac003.admin@example.com` and the registered user `f4.ac003.user@example.com`
-> done: ArrangeSteps.GivenTheAdministratorAndTheRegisteredUser("f4.ac003.admin@ex...", "f4.ac003.user@exa...") (0,2s)
And a Published `A4.003 Sugar` with the intent Reduce and no suggested starting goal
-> done: ArrangeSteps.GivenCatalogHabits("a Published `A4.0...") (0,0s)
When `f4.ac003.user@example.com` starts a new goal from `A4.003 Sugar`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.GoalWhenSteps.WhenStartsANewGoal(String, String, String, String)' should have 2 parameters
Then the starting values are: the habit `A4.003 Sugar` (catalog), Reduce, the start date 2026-11-18, and nothing else: no measurement, no amount, no unit, no schedule, no name and no catalog suggestion text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.GoalWhenSteps.WhenStartsANewGoal(String, String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ACatalogHabitWithoutSuggestedStartingGoalsGivesOnlyBuildOrReduceFromItsIntent() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 28
[non-AC frame(s) hidden]
```

### F004/AC-070 Registration keeps a named browser time zone, and otherwise gives the default(n: "01", browserTimeZone: "the browser time zone `Europe/Kyiv`", timeZone: "`Europe/Kyiv`", __pickleIndex: "222", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac070.user.01@example.com` with the password `walking42` and the browser time zone `Europe/Kyiv`, and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When `f4.ac070.user.01@example.com` reads their time zone
-> skipped because of previous errors
Then registration succeeded and the time zone is `Europe/Kyiv`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.RegistrationKeepsANamedBrowserTimeZoneAndOtherwiseGivesTheDefault(String n, String browserTimeZone, String timeZone, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 846
[non-AC frame(s) hidden]
```

### F004/AC-032 An anonymous visitor gets no goal use case(n: "05", action: "reads the evaluation calendar of `f4.ac032.user.05"···, __pickleIndex: "139", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac032.admin.05@example.com` and a Published `A4.032 05 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac032.admin.05...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac032.user.05@example.com`, who created a goal on `A4.032 05 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When the anonymous visitor reads the evaluation calendar of `f4.ac032.user.05@example.com`'s goal from 2026-11-18 to 2026-11-24
-> skipped because of previous errors
Then the result is "You are not signed in."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnAnonymousVisitorGetsNoGoalUseCase(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 425
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "20", settings: "Reduce, yes/no, fixed days on all seven days", targetText: "\"avoided every day\"", __pickleIndex: "106", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.20@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.20@example.com` creates a goal on their custom habit `Practice` with Reduce, yes/no, fixed days on all seven days
-> skipped because of previous errors
Then reading the goal gives the target text "avoided every day" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-084 An account name is trimmed, and empty means no name(n: "02", name: "no name", result: "no name", __pickleIndex: "274", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac084.user.02@example.com` with the password `walking42` and no name, and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When `f4.ac084.user.02@example.com` reads their profile
-> skipped because of previous errors
Then registration succeeded and the profile gives no name and the email `f4.ac084.user.02@example.com`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnAccountNameIsTrimmedAndEmptyMeansNoName(String n, String name, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 1034
[non-AC frame(s) hidden]
```

### F004/AC-012 A Reduce goal with an amount has 1 time per period(n: "06", settings: "Reduce, count 2 `cups`, fixed days Monday, Wednesd"···, result: "creating succeeds", __pickleIndex: "65", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac012.admin.06@example.com` and a Published `A4.012 06 Coffee`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac012.admin.06...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac012.user.06@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac012.user.06@example.com` creates a goal on `A4.012 06 Coffee` with Reduce, count 2 `cups`, fixed days Monday, Wednesday and Friday
-> skipped because of previous errors
Then the result is creating succeeds
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AReduceGoalWithAnAmountHas1TimePerPeriod(String n, String settings, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 171
[non-AC frame(s) hidden]
```

### F004/AC-032 An anonymous visitor gets no goal use case(n: "02", action: "creates a goal on `A4.032 02 Reading` with Build, "···, __pickleIndex: "136", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac032.admin.02@example.com` and a Published `A4.032 02 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac032.admin.02...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac032.user.02@example.com`, who created a goal on `A4.032 02 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When the anonymous visitor creates a goal on `A4.032 02 Reading` with Build, yes/no, fixed days Monday
-> skipped because of previous errors
Then the result is "You are not signed in."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnAnonymousVisitorGetsNoGoalUseCase(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 425
[non-AC frame(s) hidden]
```

### F004/AC-033 A custom habit is looked up only among the acting user's custom habits(n: "03", action: "starts a new goal from", habit: "the identifier of `A4.033 03 Reading`", __pickleIndex: "143", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac033.admin.03@example.com` and a Published `A4.033 03 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac033.admin.03...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac033.b.03@example.com`, who created the custom habit `Cold shower`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.b\.{int}@example\.com`, who created the custom habit `Cold shower`")]
public void GivenTheRegisteredUserF_Ac_B_Example_ComWhoCreatedTheCustomHabitColdShower(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And the registered user `f4.ac033.a.03@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.a\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_A_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac033.a.03@example.com` starts a new goal from the identifier of `A4.033 03 Reading` given as a custom habit
-> skipped because of previous errors
Then the result is "This custom habit does not exist."
-> skipped because of previous errors
And `f4.ac033.a.03@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ACustomHabitIsLookedUpOnlyAmongTheActingUsersCustomHabits(String n, String action, String habit, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 443
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "03", settings: "Build, yes/no with the amount 5, fixed days Monday", message: "\"A yes/no goal has no amount or unit.\"", __pickleIndex: "10", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.03@example.com` and a Published `A4.007 03 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.03...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.03@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.03@example.com` creates a goal on `A4.007 03 Reading` with Build, yes/no with the amount 5, fixed days Monday
-> skipped because of previous errors
Then the result is "A yes/no goal has no amount or unit."
-> skipped because of previous errors
And `f4.ac007.user.03@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-045 A fixed-days goal starting on an unscheduled day begins at its next scheduled day — pending

Step trace:
```
Given the registered user `f4.ac045.user@example.com`, who created the custom habit `Yoga`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the custom habit `Yoga`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheCustomHabitYoga(int p0, int p1)
{
    throw new PendingStepException();
}

And `f4.ac045.user@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days Monday, Wednesday and Friday and the start date 2026-11-24
-> skipped because of previous errors
When `f4.ac045.user@example.com` reads the evaluation calendar of the goal from 2026-11-23 to 2026-11-29
-> skipped because of previous errors
Then it has 2 commitments, in this order: 2026-11-25 from `2026-11-25T00:00:00Z` to `2026-11-26T00:00:00Z`, and 2026-11-27 from `2026-11-27T00:00:00Z` to `2026-11-28T00:00:00Z`
-> skipped because of previous errors
And it has no commitment on 2026-11-23, before the start date
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AFixed_DaysGoalStartingOnAnUnscheduledDayBeginsAtItsNextScheduledDay() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 596
[non-AC frame(s) hidden]
```

### F004/AC-088 The Home date is today in the account's time zone, in English whatever the culture(n: "02", culture: "`uk-UA`", __pickleIndex: "287", exampleTags: []) — failed

Step trace:
```
Given the registered user `f4.ac088.user.02@example.com` in `Europe/Kyiv`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And the current culture of the test is `uk-UA`
-> skipped because of previous errors
When at `2026-11-16T23:30:00Z` `f4.ac088.user.02@example.com` reads the Home heading
-> skipped because of previous errors
Then the date is "Tuesday, 17 November" and the greeting is "Good evening"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheHomeDateIsTodayInTheAccountsTimeZoneInEnglishWhateverTheCulture(String n, String culture, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 1079
[non-AC frame(s) hidden]
```

### F004/AC-035 Use cases check the acting user, the role, the identifier and the fields, in this order(n: "03", actingUser: "`f4.ac035.user.03@example.com`", action: "reads the evaluation calendar from 2026-11-24 to 2"···, message: "\"This goal does not exist.\"", __pickleIndex: "149", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac035.admin.03@example.com` and a Draft `A4.035 03 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac035.admin.03...", "a Draft `A4.035 0...") (0,1s)
And the registered user `f4.ac035.user.03@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac035.user.03@example.com` reads the evaluation calendar from 2026-11-24 to 2026-11-18 of an identifier that belongs to no goal
-> skipped because of previous errors
Then the result is "This goal does not exist."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.UseCasesCheckTheActingUserTheRoleTheIdentifierAndTheFieldsInThisOrder(String n, String actingUser, String action, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 467
[non-AC frame(s) hidden]
```

### F004/AC-070 Registration keeps a named browser time zone, and otherwise gives the default(n: "15", browserTimeZone: "the browser time zone `America/Argentina/Buenos_Ai"···, timeZone: "`America/Argentina/Buenos_Aires`", __pickleIndex: "236", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac070.user.15@example.com` with the password `walking42` and the browser time zone `America/Argentina/Buenos_Aires`, and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When `f4.ac070.user.15@example.com` reads their time zone
-> skipped because of previous errors
Then registration succeeded and the time zone is `America/Argentina/Buenos_Aires`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.RegistrationKeepsANamedBrowserTimeZoneAndOtherwiseGivesTheDefault(String n, String browserTimeZone, String timeZone, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 846
[non-AC frame(s) hidden]
```

### F004/AC-091 An anonymous visitor gets no Home heading, no name change and no profile(n: "01", action: "reads the Home heading", __pickleIndex: "293", exampleTags: []) — failed

Step trace:
```
When the anonymous visitor reads the Home heading
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.AccountSteps.WhenReadsTheHomeHeading(String, String)' should have 1 parameters
Then the result is "You are not signed in."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.AccountSteps.WhenReadsTheHomeHeading(String, String)' should have 1 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnAnonymousVisitorGetsNoHomeHeadingNoNameChangeAndNoProfile(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 1110
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "06", settings: "Build, yes/no, fixed days Sunday", targetText: "\"once a week\"", __pickleIndex: "92", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.06@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.06@example.com` creates a goal on their custom habit `Practice` with Build, yes/no, fixed days Sunday
-> skipped because of previous errors
Then reading the goal gives the target text "once a week" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-008 Goal creation gives the first refusal in FR-006 order(n: "04", goal: "on `A4.008 04 Reading` with Build, yes/no with the"···, message: "\"A yes/no goal has no amount or unit.\"", __pickleIndex: "33", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac008.admin.04@example.com`, a Published `A4.008 04 Reading` and a Draft `A4.008 04 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac008.admin.04...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac008.user.04@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac008.user.04@example.com` creates a goal on `A4.008 04 Reading` with Build, yes/no with the amount 5, no schedule
-> skipped because of previous errors
Then the result is "A yes/no goal has no amount or unit."
-> skipped because of previous errors
And `f4.ac008.user.04@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.GoalCreationGivesTheFirstRefusalInFR_006Order(String n, String goal, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 100
[non-AC frame(s) hidden]
```

### F004/AC-042 A range before the start date gives an empty calendar — pending

Step trace:
```
Given the registered user `f4.ac042.user@example.com`, who created the custom habit `Yoga`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the custom habit `Yoga`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheCustomHabitYoga(int p0, int p1)
{
    throw new PendingStepException();
}

And `f4.ac042.user@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days on all seven days and the start date 2026-11-25
-> skipped because of previous errors
When `f4.ac042.user@example.com` reads the evaluation calendar of the goal from 2026-11-18 to 2026-11-24
-> skipped because of previous errors
Then the calendar has no entry
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ARangeBeforeTheStartDateGivesAnEmptyCalendar() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 560
[non-AC frame(s) hidden]
```

### F004/AC-086 The Home greeting follows the time of day in the account's time zone(n: "02", time: "`2026-11-18T03:00:00Z` (05:00 in Kyiv)", greeting: "\"Good morning, Yuri\"", __pickleIndex: "279", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac086.user.02@example.com` with the password `walking42`, the browser time zone `Europe/Kyiv` and the name "Yuri", and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When at `2026-11-18T03:00:00Z` (05:00 in Kyiv) `f4.ac086.user.02@example.com` reads the Home heading
-> skipped because of previous errors
Then the greeting is "Good morning, Yuri" and the date is "Wednesday, 18 November"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheHomeGreetingFollowsTheTimeOfDayInTheAccountsTimeZone(String n, String time, String greeting, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 1056
[non-AC frame(s) hidden]
```

### F004/AC-070 Registration keeps a named browser time zone, and otherwise gives the default(n: "03", browserTimeZone: "the browser time zone `Mars/Olympus`", timeZone: "`UTC`", __pickleIndex: "224", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac070.user.03@example.com` with the password `walking42` and the browser time zone `Mars/Olympus`, and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When `f4.ac070.user.03@example.com` reads their time zone
-> skipped because of previous errors
Then registration succeeded and the time zone is `UTC`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.RegistrationKeepsANamedBrowserTimeZoneAndOtherwiseGivesTheDefault(String n, String browserTimeZone, String timeZone, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 846
[non-AC frame(s) hidden]
```

### F004/AC-001 Starting from a catalog habit without a position gives its marked suggested starting goal(n: "02", suggestions: "1. Build, count 20 `pages`, 3 times per week; 2. B"···, __pickleIndex: "1", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac001.admin.02@example.com` and the registered user `f4.ac001.user.02@example.com`
-> done: ArrangeSteps.GivenTheAdministratorAndTheRegisteredUser("f4.ac001.admin.02...", "f4.ac001.user.02@...") (0,1s)
And a Published `A4.001 02 Reading` with the suggested starting goals 1. Build, count 20 `pages`, 3 times per week; 2. Build, duration 15 minutes, fixed days Monday to Friday (marked), in this order
-> done: ArrangeSteps.GivenCatalogHabits("a Published `A4.0...") (0,0s)
When `f4.ac001.user.02@example.com` starts a new goal from `A4.001 02 Reading`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.GoalWhenSteps.WhenStartsANewGoal(String, String, String, String)' should have 2 parameters
Then the starting values are: the habit `A4.001 02 Reading` (catalog), Build, duration 15 minutes, fixed days Monday to Friday, the start date 2026-11-18, no name, and the catalog suggestion text "15 min, 5 days a week"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.GoalWhenSteps.WhenStartsANewGoal(String, String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.StartingFromACatalogHabitWithoutAPositionGivesItsMarkedSuggestedStartingGoal(String n, String suggestions, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 9
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "14", settings: "Reduce, yes/no, 1 time per day", targetText: "\"at most once a day\"", __pickleIndex: "100", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.14@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.14@example.com` creates a goal on their custom habit `Practice` with Reduce, yes/no, 1 time per day
-> skipped because of previous errors
Then reading the goal gives the target text "at most once a day" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-009 The edges of every target range are accepted(n: "01", settings: "Build, count 1 `pages`, 3 times per week", targetText: "\"1 pages, 3 times a week\"", __pickleIndex: "44", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac009.admin.01@example.com` and a Published `A4.009 01 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac009.admin.01...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac009.user.01@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac009.user.01@example.com` creates a goal on `A4.009 01 Reading` with Build, count 1 `pages`, 3 times per week
-> skipped because of previous errors
Then creating succeeds
-> skipped because of previous errors
And reading the goal gives the target text "1 pages, 3 times a week"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheEdgesOfEveryTargetRangeAreAccepted(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 125
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "12", settings: "Build, yes/no, 2 times per day", targetText: "\"2 times a day\"", __pickleIndex: "98", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.12@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.12@example.com` creates a goal on their custom habit `Practice` with Build, yes/no, 2 times per day
-> skipped because of previous errors
Then reading the goal gives the target text "2 times a day" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-038 A per-week goal has one evaluation period per Monday-to-Sunday week, from its first full week(n: "03", startDate: "2026-11-18", from: "2026-11-18", to: "2026-11-22", periods: "no period", __pickleIndex: "158", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac038.user.03@example.com`, who created the custom habit `Run`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Run`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitRun(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And `f4.ac038.user.03@example.com` created a goal on their custom habit `Run` with Build, yes/no, 3 times per week and the start date 2026-11-18
-> skipped because of previous errors
When `f4.ac038.user.03@example.com` reads the evaluation calendar of the goal from 2026-11-18 to 2026-11-22
-> skipped because of previous errors
Then it has no period
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.APer_WeekGoalHasOneEvaluationPeriodPerMonday_To_SundayWeekFromItsFirstFullWeek(String n, String startDate, String from, String to, String periods, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 505
[non-AC frame(s) hidden]
```

### F004/AC-016 A goal without a name of its own is named after its habit and target(n: "04", name: "the name `\"   \"`", goalName: "\"A4.016 04 Reading, 15 min, 5 days a week\"", own: "not its own", __pickleIndex: "85", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac016.admin.04@example.com` and a Published `A4.016 04 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac016.admin.04...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac016.user.04@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac016.user.04@example.com` creates a goal on `A4.016 04 Reading` with Build, duration 15 minutes, fixed days Monday to Friday and the name `"   "`
-> skipped because of previous errors
Then reading the goal gives the name "A4.016 04 Reading, 15 min, 5 days a week", and the name is not its own
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AGoalWithoutANameOfItsOwnIsNamedAfterItsHabitAndTarget(String n, String name, String goalName, String own, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 238
[non-AC frame(s) hidden]
```

### F004/AC-055 Anonymous visitors and administrators get the catalog without goal information(n: "01", actingUser: "the anonymous visitor", action: "browses the catalog as the anonymous visitor with "···, result: "the result contains `A4.055 01 Reading` with no go"···, __pickleIndex: "195", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac055.admin.01@example.com` and a Published `A4.055 01 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac055.admin.01...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac055.user.01@example.com`, who created a goal on `A4.055 01 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When the anonymous visitor browses the catalog as the anonymous visitor with the search `A4.055 01`
-> skipped because of previous errors
Then the result contains `A4.055 01 Reading` with no goal count, and no tracked count
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnonymousVisitorsAndAdministratorsGetTheCatalogWithoutGoalInformation(String n, String actingUser, String action, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 688
[non-AC frame(s) hidden]
```

### F004/AC-014 A habit that is not a Published catalog habit is refused as a catalog habit(n: "02", action: "starts a new goal from", habit: "`A4.014 02 Gone`", __pickleIndex: "73", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac014.admin.02@example.com`, a Draft `A4.014 02 Draft` and an Unpublished `A4.014 02 Gone`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac014.admin.02...", "a Draft `A4.014 0...") (0,1s)
And the registered user `f4.ac014.user.02@example.com`, who created the custom habit `Cold shower`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Cold shower`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitColdShower(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac014.user.02@example.com` starts a new goal from `A4.014 02 Gone` given as a catalog habit
-> skipped because of previous errors
Then the result is "This habit is not in the catalog."
-> skipped because of previous errors
And `f4.ac014.user.02@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AHabitThatIsNotAPublishedCatalogHabitIsRefusedAsACatalogHabit(String n, String action, String habit, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 205
[non-AC frame(s) hidden]
```

### F004/AC-023 The next commitment is the first commitment that has not ended(n: "05", time: "`2026-11-21T00:00:00Z`", next: "2026-11-23", __pickleIndex: "118", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac023.user.05@example.com`, who created the custom habit `Yoga`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Yoga`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitYoga(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at `2026-11-16T08:00:00Z` `f4.ac023.user.05@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days Monday, Wednesday and Friday and the start date 2026-11-16
-> skipped because of previous errors
When at `2026-11-21T00:00:00Z` `f4.ac023.user.05@example.com` lists their goals
-> skipped because of previous errors
Then the goal gives next commitment 2026-11-23 and no start date
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheNextCommitmentIsTheFirstCommitmentThatHasNotEnded(String n, String time, String next, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 329
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "11", settings: "Reduce, count 0 `cigarettes`, 1 time per day", targetText: "\"at most 0 cigarettes a day\"", __pickleIndex: "97", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.11@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.11@example.com` creates a goal on their custom habit `Practice` with Reduce, count 0 `cigarettes`, 1 time per day
-> skipped because of previous errors
Then reading the goal gives the target text "at most 0 cigarettes a day" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-086 The Home greeting follows the time of day in the account's time zone(n: "07", time: "`2026-11-18T21:59:59Z` (23:59:59 in Kyiv)", greeting: "\"Good evening, Yuri\"", __pickleIndex: "284", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac086.user.07@example.com` with the password `walking42`, the browser time zone `Europe/Kyiv` and the name "Yuri", and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When at `2026-11-18T21:59:59Z` (23:59:59 in Kyiv) `f4.ac086.user.07@example.com` reads the Home heading
-> skipped because of previous errors
Then the greeting is "Good evening, Yuri" and the date is "Wednesday, 18 November"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheHomeGreetingFollowsTheTimeOfDayInTheAccountsTimeZone(String n, String time, String greeting, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 1056
[non-AC frame(s) hidden]
```

### F004/AC-009 The edges of every target range are accepted(n: "05", settings: "Build, count 5 with a unit of 20 characters, 1 tim"···, targetText: "\"5 uuuuuuuuuuuuuuuuuuuu a day\"", __pickleIndex: "48", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac009.admin.05@example.com` and a Published `A4.009 05 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac009.admin.05...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac009.user.05@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac009.user.05@example.com` creates a goal on `A4.009 05 Reading` with Build, count 5 with a unit of 20 characters, 1 time per day
-> skipped because of previous errors
Then creating succeeds
-> skipped because of previous errors
And reading the goal gives the target text "5 uuuuuuuuuuuuuuuuuuuu a day"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheEdgesOfEveryTargetRangeAreAccepted(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 125
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "08", settings: "Build, duration 0 minutes, 3 times per week", message: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "15", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.08@example.com` and a Published `A4.007 08 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.08...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.08@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.08@example.com` creates a goal on `A4.007 08 Reading` with Build, duration 0 minutes, 3 times per week
-> skipped because of previous errors
Then the result is "Enter a whole number from 1 to 999."
-> skipped because of previous errors
And `f4.ac007.user.08@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-012 A Reduce goal with an amount has 1 time per period(n: "01", settings: "Reduce, count 2 `cups`, 1 time per week", result: "creating succeeds", __pickleIndex: "60", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac012.admin.01@example.com` and a Published `A4.012 01 Coffee`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac012.admin.01...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac012.user.01@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac012.user.01@example.com` creates a goal on `A4.012 01 Coffee` with Reduce, count 2 `cups`, 1 time per week
-> skipped because of previous errors
Then the result is creating succeeds
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AReduceGoalWithAnAmountHas1TimePerPeriod(String n, String settings, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 171
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "16", settings: "Reduce, count 2 `cups`, fixed days Monday to Frida"···, targetText: "\"at most 2 cups, 5 days a week\"", __pickleIndex: "102", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.16@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.16@example.com` creates a goal on their custom habit `Practice` with Reduce, count 2 `cups`, fixed days Monday to Friday
-> skipped because of previous errors
Then reading the goal gives the target text "at most 2 cups, 5 days a week" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-046 Starting from a custom habit gives only Build or Reduce from its intent(n: "02", intent: "Reduce", __pickleIndex: "182", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac046.user.02@example.com`, who created the custom habit `Cold shower` with the intent Reduce
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Cold shower` with the intent Reduce")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitColdShowerWithTheIntentReduce(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac046.user.02@example.com` starts a new goal from their custom habit `Cold shower`
-> skipped because of previous errors
Then the starting values are: the habit `Cold shower` (custom), Reduce, the start date 2026-11-18, and nothing else: no measurement, no amount, no unit, no schedule, no name and no catalog suggestion text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.StartingFromACustomHabitGivesOnlyBuildOrReduceFromItsIntent(String n, String intent, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 602
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "09", settings: "Build, duration 30 minutes, fixed days on all seve"···, targetText: "\"30 min daily\"", __pickleIndex: "95", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.09@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.09@example.com` creates a goal on their custom habit `Practice` with Build, duration 30 minutes, fixed days on all seven days
-> skipped because of previous errors
Then reading the goal gives the target text "30 min daily" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-015 A habit that changed between starting and creating is looked up again(n: "02", habit: "their custom habit `Cold shower`", change: "`f4.ac015.user.02@example.com` deleted their custo"···, message: "\"This custom habit does not exist.\"", __pickleIndex: "81", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac015.admin.02@example.com` and a Published `A4.015 02 Reading` with the suggested starting goal Build, duration 15 minutes, fixed days Monday to Friday
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac015.admin.02...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac015.user.02@example.com`, who created the custom habit `Cold shower`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Cold shower`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitColdShower(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And `f4.ac015.user.02@example.com` started a new goal from their custom habit `Cold shower`
-> skipped because of previous errors
And `f4.ac015.user.02@example.com` deleted their custom habit `Cold shower`
-> skipped because of previous errors
When `f4.ac015.user.02@example.com` creates a goal on their custom habit `Cold shower` with Build, duration 15 minutes, fixed days Monday to Friday
-> skipped because of previous errors
Then the result is "This custom habit does not exist."
-> skipped because of previous errors
And `f4.ac015.user.02@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AHabitThatChangedBetweenStartingAndCreatingIsLookedUpAgain(String n, String habit, String change, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 226
[non-AC frame(s) hidden]
```

### F004/AC-012 A Reduce goal with an amount has 1 time per period(n: "04", settings: "Reduce, yes/no, 3 times per week", result: "creating succeeds", __pickleIndex: "63", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac012.admin.04@example.com` and a Published `A4.012 04 Coffee`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac012.admin.04...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac012.user.04@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac012.user.04@example.com` creates a goal on `A4.012 04 Coffee` with Reduce, yes/no, 3 times per week
-> skipped because of previous errors
Then the result is creating succeeds
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AReduceGoalWithAnAmountHas1TimePerPeriod(String n, String settings, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 171
[non-AC frame(s) hidden]
```

### F004/AC-001 Starting from a catalog habit without a position gives its marked suggested starting goal(n: "01", suggestions: "1. Build, duration 15 minutes, fixed days Monday t"···, __pickleIndex: "0", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac001.admin.01@example.com` and the registered user `f4.ac001.user.01@example.com`
-> done: ArrangeSteps.GivenTheAdministratorAndTheRegisteredUser("f4.ac001.admin.01...", "f4.ac001.user.01@...") (0,2s)
And a Published `A4.001 01 Reading` with the suggested starting goals 1. Build, duration 15 minutes, fixed days Monday to Friday (marked); 2. Build, count 20 `pages`, 3 times per week, in this order
-> done: ArrangeSteps.GivenCatalogHabits("a Published `A4.0...") (0,0s)
When `f4.ac001.user.01@example.com` starts a new goal from `A4.001 01 Reading`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.GoalWhenSteps.WhenStartsANewGoal(String, String, String, String)' should have 2 parameters
Then the starting values are: the habit `A4.001 01 Reading` (catalog), Build, duration 15 minutes, fixed days Monday to Friday, the start date 2026-11-18, no name, and the catalog suggestion text "15 min, 5 days a week"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.GoalWhenSteps.WhenStartsANewGoal(String, String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.StartingFromACatalogHabitWithoutAPositionGivesItsMarkedSuggestedStartingGoal(String n, String suggestions, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 9
[non-AC frame(s) hidden]
```

### F004/AC-047 A goal on a custom habit is based on it and named after it — pending

Step trace:
```
Given the registered user `f4.ac047.user@example.com`, who created the custom habit `Cold shower`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the custom habit `Cold shower`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheCustomHabitColdShower(int p0, int p1)
{
    throw new PendingStepException();
}

When `f4.ac047.user@example.com` creates a goal on their custom habit `Cold shower` with Build, yes/no, fixed days on all seven days
-> skipped because of previous errors
Then creating succeeds
-> skipped because of previous errors
And reading the goal gives the name "Cold shower, daily", the habit name `Cold shower`, the kind custom, the category Other, Build, yes/no, fixed days on all seven days and the schedule kind Commitment-based
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AGoalOnACustomHabitIsBasedOnItAndNamedAfterIt() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 614
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "10", settings: "Build, count 10 `pages`, 1 time per day", targetText: "\"10 pages a day\"", __pickleIndex: "96", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.10@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.10@example.com` creates a goal on their custom habit `Practice` with Build, count 10 `pages`, 1 time per day
-> skipped because of previous errors
Then reading the goal gives the target text "10 pages a day" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-070 Registration keeps a named browser time zone, and otherwise gives the default(n: "07", browserTimeZone: "the browser time zone `CET`", timeZone: "`UTC`", __pickleIndex: "228", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac070.user.07@example.com` with the password `walking42` and the browser time zone `CET`, and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When `f4.ac070.user.07@example.com` reads their time zone
-> skipped because of previous errors
Then registration succeeded and the time zone is `UTC`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.RegistrationKeepsANamedBrowserTimeZoneAndOtherwiseGivesTheDefault(String n, String browserTimeZone, String timeZone, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 846
[non-AC frame(s) hidden]
```

### F004/AC-052 Browsing gives a registered user their goal count per habit and the tracked count(n: "01", search: "`A4.052 01`", result: "`A4.052 01 Reading` has the goal count 2, `A4.052 "···, __pickleIndex: "188", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac052.admin.01@example.com`, a Published `A4.052 01 Reading`, a Published `A4.052 01 Yoga` and a Published `A4.052 01 Water`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac052.admin.01...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac052.user.01@example.com`, who created two goals on `A4.052 01 Reading` and one goal on `A4.052 01 Water`, each with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When `f4.ac052.user.01@example.com` browses the catalog as `f4.ac052.user.01@example.com` with the search `A4.052 01`
-> skipped because of previous errors
Then `A4.052 01 Reading` has the goal count 2, `A4.052 01 Yoga` 0 and `A4.052 01 Water` 1, and the tracked count is 2
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.BrowsingGivesARegisteredUserTheirGoalCountPerHabitAndTheTrackedCount(String n, String search, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 653
[non-AC frame(s) hidden]
```

### F004/AC-043 The calendar range has 1 to 366 days(n: "04", from: "2026-11-18", to: "2026-11-18", result: "a calendar of 1 commitment, on 2026-11-18", __pickleIndex: "175", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac043.user.04@example.com`, who created the custom habit `Yoga`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Yoga`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitYoga(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And `f4.ac043.user.04@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days Wednesday and the start date 2026-11-18
-> skipped because of previous errors
When `f4.ac043.user.04@example.com` reads the evaluation calendar of the goal from 2026-11-18 to 2026-11-18
-> skipped because of previous errors
Then the result is a calendar of 1 commitment, on 2026-11-18
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheCalendarRangeHas1To366Days(String n, String from, String to, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 567
[non-AC frame(s) hidden]
```

### F004/AC-060 A goal on an Unpublished habit keeps working, and no new goal can use that habit(n: "04", actingUser: "`f4.ac060.owner.04@example.com`", action: "creates a goal on `A4.060 04 Reading` with Build, "···, result: "the result is \"This habit is not in the catalog.\"···, __pickleIndex: "211", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac060.admin.04@example.com` and a Published `A4.060 04 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac060.admin.04...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac060.owner.04@example.com`, who created a goal on `A4.060 04 Reading` with Build, duration 15 minutes, fixed days Monday to Friday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And the registered user `f4.ac060.other.04@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.other\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_Other_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And the administrator unpublished `A4.060 04 Reading` with confirmation
-> skipped because of previous errors
When `f4.ac060.owner.04@example.com` creates a goal on `A4.060 04 Reading` with Build, yes/no, fixed days Monday
-> skipped because of previous errors
Then the result is "This habit is not in the catalog."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AGoalOnAnUnpublishedHabitKeepsWorkingAndNoNewGoalCanUseThatHabit(String n, String actingUser, String action, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 749
[non-AC frame(s) hidden]
```

### F004/AC-031 An administrator gets no goal use case(n: "04", action: "reads `f4.ac031.user.04@example.com`'s goal", __pickleIndex: "132", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac031.admin.04@example.com` and a Published `A4.031 04 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac031.admin.04...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac031.user.04@example.com`, who created a goal on `A4.031 04 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When `f4.ac031.admin.04@example.com` reads `f4.ac031.user.04@example.com`'s goal
-> skipped because of previous errors
Then the result is "Goals are for registered users." and no goal is returned
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnAdministratorGetsNoGoalUseCase(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 409
[non-AC frame(s) hidden]
```

### F004/AC-070 Registration keeps a named browser time zone, and otherwise gives the default(n: "17", browserTimeZone: "the browser time zone `America/New_York`", timeZone: "`America/New_York`", __pickleIndex: "238", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac070.user.17@example.com` with the password `walking42` and the browser time zone `America/New_York`, and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When `f4.ac070.user.17@example.com` reads their time zone
-> skipped because of previous errors
Then registration succeeded and the time zone is `America/New_York`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.RegistrationKeepsANamedBrowserTimeZoneAndOtherwiseGivesTheDefault(String n, String browserTimeZone, String timeZone, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 846
[non-AC frame(s) hidden]
```

### F004/AC-035 Use cases check the acting user, the role, the identifier and the fields, in this order(n: "01", actingUser: "the anonymous visitor", action: "reads the evaluation calendar from 2026-11-24 to 2"···, message: "\"You are not signed in.\"", __pickleIndex: "147", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac035.admin.01@example.com` and a Draft `A4.035 01 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac035.admin.01...", "a Draft `A4.035 0...") (0,1s)
And the registered user `f4.ac035.user.01@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When the anonymous visitor reads the evaluation calendar from 2026-11-24 to 2026-11-18 of an identifier that belongs to no goal
-> skipped because of previous errors
Then the result is "You are not signed in."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.UseCasesCheckTheActingUserTheRoleTheIdentifierAndTheFieldsInThisOrder(String n, String actingUser, String action, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 467
[non-AC frame(s) hidden]
```

### F004/AC-023 The next commitment is the first commitment that has not ended(n: "04", time: "`2026-11-19T00:00:00Z`", next: "2026-11-20", __pickleIndex: "117", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac023.user.04@example.com`, who created the custom habit `Yoga`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Yoga`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitYoga(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at `2026-11-16T08:00:00Z` `f4.ac023.user.04@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days Monday, Wednesday and Friday and the start date 2026-11-16
-> skipped because of previous errors
When at `2026-11-19T00:00:00Z` `f4.ac023.user.04@example.com` lists their goals
-> skipped because of previous errors
Then the goal gives next commitment 2026-11-20 and no start date
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheNextCommitmentIsTheFirstCommitmentThatHasNotEnded(String n, String time, String next, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 329
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "06", settings: "Build, count 0 `pages`, 3 times per week", message: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "13", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.06@example.com` and a Published `A4.007 06 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.06...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.06@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.06@example.com` creates a goal on `A4.007 06 Reading` with Build, count 0 `pages`, 3 times per week
-> skipped because of previous errors
Then the result is "Enter a whole number from 1 to 999."
-> skipped because of previous errors
And `f4.ac007.user.06@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-028 A goal stays as it was created — failed

Step trace:
```
Given the administrator `f4.ac028.admin@example.com` and a Published `A4.028 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac028.admin@ex...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac028.user@example.com`, who created a goal G on `A4.028 Reading` with Build, duration 15 minutes, fixed days Monday to Friday and the name "Morning pages"
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And at `2026-11-25T10:00:00Z` `f4.ac028.user@example.com` listed their goals, read G, read the evaluation calendar of G from 2026-11-16 to 2026-11-29, started a new goal from `A4.028 Reading` and created a goal on `A4.028 Reading` with Build, yes/no, 1 time per day
-> skipped because of previous errors
When at `2026-11-25T10:00:00Z` `f4.ac028.user@example.com` reads G
-> skipped because of previous errors
Then the result has the name "Morning pages", Build, duration 15 minutes, fixed days Monday to Friday, the start date 2026-11-18, the time zone `UTC` from 2026-11-18, no later version, and attempt 1, started on 2026-11-18
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AGoalStaysAsItWasCreated() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 379
[non-AC frame(s) hidden]
```

### F004/AC-057 Unpublishing a habit that goals use needs confirmation and gives only the number of goals(n: "02", goals: "`f4.ac057.a.02@example.com` created one goal", count: "1", text: "\"1 goal uses this habit.\"", __pickleIndex: "203", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac057.admin.02@example.com` and a Published `A4.057 02 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac057.admin.02...", "a Published `A4.0...") (0,1s)
And the registered users `f4.ac057.a.02@example.com` and `f4.ac057.b.02@example.com`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUsers(String, String, String, String)' should have 2 parameters
And `f4.ac057.a.02@example.com` created one goal, each on `A4.057 02 Reading` with Build, yes/no, fixed days Monday
-> skipped because of previous errors
When `f4.ac057.admin.02@example.com` unpublishes `A4.057 02 Reading` without confirming
-> skipped because of previous errors
Then the result is "confirmation needed" with the number of goals 1 and the text "1 goal uses this habit."
-> skipped because of previous errors
And the result carries nothing else: no email, no goal name, no goal identifier and no setting
-> skipped because of previous errors
And browsing the catalog without an acting user with the search `A4.057 02` still contains `A4.057 02 Reading`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUsers(String, String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.UnpublishingAHabitThatGoalsUseNeedsConfirmationAndGivesOnlyTheNumberOfGoals(String n, String goals, String count, String text, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 715
[non-AC frame(s) hidden]
```

### F004/AC-012 A Reduce goal with an amount has 1 time per period(n: "03", settings: "Reduce, count 2 `cups`, 3 times per day", result: "\"A Reduce goal with an amount has 1 time per peri"···, __pickleIndex: "62", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac012.admin.03@example.com` and a Published `A4.012 03 Coffee`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac012.admin.03...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac012.user.03@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac012.user.03@example.com` creates a goal on `A4.012 03 Coffee` with Reduce, count 2 `cups`, 3 times per day
-> skipped because of previous errors
Then the result is "A Reduce goal with an amount has 1 time per period."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AReduceGoalWithAnAmountHas1TimePerPeriod(String n, String settings, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 171
[non-AC frame(s) hidden]
```

### F004/AC-055 Anonymous visitors and administrators get the catalog without goal information(n: "06", actingUser: "`f4.ac055.admin.06@example.com`", action: "browses the catalog as `f4.ac055.admin.06@example."···, result: "the result is \"Goals are for registered users.\"", __pickleIndex: "200", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac055.admin.06@example.com` and a Published `A4.055 06 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac055.admin.06...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac055.user.06@example.com`, who created a goal on `A4.055 06 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When `f4.ac055.admin.06@example.com` browses the catalog as `f4.ac055.admin.06@example.com` with "Only habits I track"
-> skipped because of previous errors
Then the result is "Goals are for registered users."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnonymousVisitorsAndAdministratorsGetTheCatalogWithoutGoalInformation(String n, String actingUser, String action, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 688
[non-AC frame(s) hidden]
```

### F004/AC-086 The Home greeting follows the time of day in the account's time zone(n: "03", time: "`2026-11-18T09:59:00Z` (11:59 in Kyiv)", greeting: "\"Good morning, Yuri\"", __pickleIndex: "280", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac086.user.03@example.com` with the password `walking42`, the browser time zone `Europe/Kyiv` and the name "Yuri", and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When at `2026-11-18T09:59:00Z` (11:59 in Kyiv) `f4.ac086.user.03@example.com` reads the Home heading
-> skipped because of previous errors
Then the greeting is "Good morning, Yuri" and the date is "Wednesday, 18 November"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheHomeGreetingFollowsTheTimeOfDayInTheAccountsTimeZone(String n, String time, String greeting, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 1056
[non-AC frame(s) hidden]
```

### F004/AC-025 Reading a goal gives all its fields, its settings version and its attempt — failed

Step trace:
```
Given the administrator `f4.ac025.admin@example.com` and a Published `A4.025 Reading` with the category Mind
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac025.admin@ex...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac025.user@example.com`, who created a goal on `A4.025 Reading` with Build, duration 15 minutes, fixed days Monday to Friday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When `f4.ac025.user@example.com` reads the goal
-> skipped because of previous errors
Then the result has the goal's identifier, the name "A4.025 Reading, 15 min, 5 days a week", which is not its own, the habit name `A4.025 Reading`, the kind catalog, the category Mind, and that the habit is in the catalog
-> skipped because of previous errors
And Build, duration 15 minutes, fixed days Monday to Friday, the schedule kind Commitment-based, the target text "15 min, 5 days a week" and the start date 2026-11-18
-> skipped because of previous errors
And no first week starts date
-> skipped because of previous errors
And the time zone `UTC` from 2026-11-18 and no later version
-> skipped because of previous errors
And attempt 1, started on 2026-11-18
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ReadingAGoalGivesAllItsFieldsItsSettingsVersionAndItsAttempt() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 358
[non-AC frame(s) hidden]
```

### F004/AC-038 A per-week goal has one evaluation period per Monday-to-Sunday week, from its first full week(n: "02", startDate: "2026-11-18", from: "2026-11-25", to: "2026-12-01", periods: "2 periods: 2026-11-23 to 2026-11-29 from `2026-11-"···, __pickleIndex: "157", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac038.user.02@example.com`, who created the custom habit `Run`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Run`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitRun(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And `f4.ac038.user.02@example.com` created a goal on their custom habit `Run` with Build, yes/no, 3 times per week and the start date 2026-11-18
-> skipped because of previous errors
When `f4.ac038.user.02@example.com` reads the evaluation calendar of the goal from 2026-11-25 to 2026-12-01
-> skipped because of previous errors
Then it has 2 periods: 2026-11-23 to 2026-11-29 from `2026-11-23T00:00:00Z` to `2026-11-30T00:00:00Z`, and 2026-11-30 to 2026-12-06 from `2026-11-30T00:00:00Z` to `2026-12-07T00:00:00Z`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.APer_WeekGoalHasOneEvaluationPeriodPerMonday_To_SundayWeekFromItsFirstFullWeek(String n, String startDate, String from, String to, String periods, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 505
[non-AC frame(s) hidden]
```

### F004/AC-031 An administrator gets no goal use case(n: "06", action: "browses the catalog as `f4.ac031.admin.06@example."···, __pickleIndex: "134", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac031.admin.06@example.com` and a Published `A4.031 06 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac031.admin.06...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac031.user.06@example.com`, who created a goal on `A4.031 06 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When `f4.ac031.admin.06@example.com` browses the catalog as `f4.ac031.admin.06@example.com` with "Only habits I track"
-> skipped because of previous errors
Then the result is "Goals are for registered users." and no goal is returned
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnAdministratorGetsNoGoalUseCase(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 409
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "22", settings: "Reduce, count 2 `cups`, 2 times per day", message: "\"A Reduce goal with an amount has 1 time per peri"···, __pickleIndex: "29", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.22@example.com` and a Published `A4.007 22 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.22...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.22@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.22@example.com` creates a goal on `A4.007 22 Reading` with Reduce, count 2 `cups`, 2 times per day
-> skipped because of previous errors
Then the result is "A Reduce goal with an amount has 1 time per period."
-> skipped because of previous errors
And `f4.ac007.user.22@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-021 Listing gives every goal with its summary, in name order — pending

Step trace:
```
Given the administrator `f4.ac021.admin@example.com`, a Published `A4.021 Yoga` and a Published `A4.021 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac021.admin@ex...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac021.user@example.com`, who created the custom habit `Cold shower`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the custom habit `Cold shower`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheCustomHabitColdShower(int p0, int p1)
{
    throw new PendingStepException();
}

And `f4.ac021.user@example.com` created a goal on `A4.021 Yoga` with Build, yes/no, 3 times per week, then a goal on `A4.021 Reading` with Build, duration 15 minutes, fixed days Monday to Friday, then a goal on their custom habit `Cold shower` with Build, yes/no, fixed days on all seven days
-> skipped because of previous errors
When `f4.ac021.user@example.com` lists their goals
-> skipped because of previous errors
Then the list is, in this order:
-> skipped because of previous errors
And "A4.021 Reading, 15 min, 5 days a week": habit `A4.021 Reading`, catalog, Build, duration, target text "15 min, 5 days a week", Commitment-based, next commitment 2026-11-18, no start date, no first week starts
-> skipped because of previous errors
And "A4.021 Yoga, 3 times a week": habit `A4.021 Yoga`, catalog, Build, yes/no, target text "3 times a week", Period-based, no next commitment, no start date, first week starts 2026-11-23
-> skipped because of previous errors
And "Cold shower, daily": habit `Cold shower`, custom, Build, yes/no, target text "daily", Commitment-based, next commitment 2026-11-18, no start date, no first week starts
-> skipped because of previous errors
And each entry has its goal's identifier
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ListingGivesEveryGoalWithItsSummaryInNameOrder() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 315
[non-AC frame(s) hidden]
```

### F004/AC-041 The days around a clock change meet at local midnight — failed

Step trace:
```
Given the registered user `f4.ac041.user@example.com` in `Europe/Kyiv`, who created the custom habit `Water`
-> done: ArrangeSteps.GivenTheRegisteredUser("f4.ac041.user@exa...", "Europe/Kyiv", "created the custo...") (0,1s)
And `f4.ac041.user@example.com` created a goal on their custom habit `Water` with Build, yes/no, 1 time per day and the start date 2026-11-18
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenCreatedAGoal(String, String, String, String)' should have 3 parameters
When `f4.ac041.user@example.com` reads the evaluation calendar of the goal from 2027-03-27 to 2027-03-29
-> skipped because of previous errors
Then it has 3 evaluation periods, in this order:
-> skipped because of previous errors
And 2027-03-27 from `2027-03-26T22:00:00Z` to `2027-03-27T22:00:00Z`
-> skipped because of previous errors
And 2027-03-28 from `2027-03-27T22:00:00Z` to `2027-03-28T21:00:00Z`
-> skipped because of previous errors
And 2027-03-29 from `2027-03-28T21:00:00Z` to `2027-03-29T21:00:00Z`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenCreatedAGoal(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheDaysAroundAClockChangeMeetAtLocalMidnight() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 553
[non-AC frame(s) hidden]
```

### F004/AC-070 Registration keeps a named browser time zone, and otherwise gives the default(n: "08", browserTimeZone: "the browser time zone `GMT`", timeZone: "`UTC`", __pickleIndex: "229", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac070.user.08@example.com` with the password `walking42` and the browser time zone `GMT`, and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When `f4.ac070.user.08@example.com` reads their time zone
-> skipped because of previous errors
Then registration succeeded and the time zone is `UTC`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.RegistrationKeepsANamedBrowserTimeZoneAndOtherwiseGivesTheDefault(String n, String browserTimeZone, String timeZone, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 846
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "02", settings: "Build, count 20 `pages`, 3 times per week", targetText: "\"20 pages, 3 times a week\"", __pickleIndex: "88", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.02@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.02@example.com` creates a goal on their custom habit `Practice` with Build, count 20 `pages`, 3 times per week
-> skipped because of previous errors
Then reading the goal gives the target text "20 pages, 3 times a week" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-040 A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "04", schedule: "1 time per day", date: "2027-10-31", entry: "2027-10-31 from `2027-10-30T21:00:00Z` to `2027-10"···, __pickleIndex: "167", exampleTags: []) — failed

Step trace:
```
Given the registered user `f4.ac040.user.04@example.com` in `Europe/Kyiv`, who created the custom habit `Practice`
-> done: ArrangeSteps.GivenTheRegisteredUser("f4.ac040.user.04@...", "Europe/Kyiv", "created the custo...") (0,1s)
And `f4.ac040.user.04@example.com` created a goal on their custom habit `Practice` with Build, yes/no, 1 time per day and the start date 2026-11-18
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenCreatedAGoal(String, String, String, String)' should have 3 parameters
When `f4.ac040.user.04@example.com` reads the evaluation calendar of the goal from 2027-10-31 to 2027-10-31
-> skipped because of previous errors
Then it has exactly one entry: 2027-10-31 from `2027-10-30T21:00:00Z` to `2027-10-31T22:00:00Z` (25 hours)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenCreatedAGoal(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ADayOrWeekWithAClockChangeKeepsLocalMidnightsAndLasts23Or25Hours(String n, String schedule, String date, String entry, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 534
[non-AC frame(s) hidden]
```

### F004/AC-010 The start date is today or up to 365 days later(n: "05", startDate: "no start date", result: "\"Choose a start date.\"", __pickleIndex: "56", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac010.admin.05@example.com` and a Published `A4.010 05 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac010.admin.05...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac010.user.05@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac010.user.05@example.com` creates a goal on `A4.010 05 Reading` with Build, yes/no, fixed days Monday and no start date
-> skipped because of previous errors
Then the result is "Choose a start date."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheStartDateIsTodayOrUpTo365DaysLater(String n, String startDate, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 143
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "13", settings: "Reduce, yes/no, 3 times per week", targetText: "\"at most 3 times a week\"", __pickleIndex: "99", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.13@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.13@example.com` creates a goal on their custom habit `Practice` with Reduce, yes/no, 3 times per week
-> skipped because of previous errors
Then reading the goal gives the target text "at most 3 times a week" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-013 Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "03", submitted: "Build, yes/no with the count unit `\"   \"`, fixed"···, stored: "yes/no, no amount and no unit", targetText: "\"daily\"", __pickleIndex: "68", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac013.admin.03@example.com` and a Published `A4.013 03 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac013.admin.03...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac013.user.03@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac013.user.03@example.com` creates a goal on `A4.013 03 Reading` with Build, yes/no with the count unit `"   "`, fixed days on all seven days
-> skipped because of previous errors
Then creating succeeds
-> skipped because of previous errors
And reading the goal gives yes/no, no amount and no unit and the target text "daily"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ValuesThatDoNotBelongToTheChosenMeasurementOrScheduleAreNeitherRefusedNorStored(String n, String submitted, String stored, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 188
[non-AC frame(s) hidden]
```

### F004/AC-039 A per-week goal tells when its first week starts(n: "03", startDate: "2026-11-18", time: "`2026-11-23T00:00:00Z`", list: "no first week starts, no start date and no next co"···, __pickleIndex: "162", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac039.user.03@example.com`, who created the custom habit `Run`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Run`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitRun(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And `f4.ac039.user.03@example.com` created a goal on their custom habit `Run` with Build, yes/no, 3 times per week and the start date 2026-11-18
-> skipped because of previous errors
When at `2026-11-23T00:00:00Z` `f4.ac039.user.03@example.com` lists their goals and reads the goal
-> skipped because of previous errors
Then the list gives the goal no first week starts, no start date and no next commitment
-> skipped because of previous errors
And reading the goal gives first week starts 2026-11-23
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.APer_WeekGoalTellsWhenItsFirstWeekStarts(String n, String startDate, String time, String list, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 520
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "16", settings: "Build, yes/no, no schedule", message: "\"Choose fixed days or how many times.\"", __pickleIndex: "23", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.16@example.com` and a Published `A4.007 16 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.16...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.16@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.16@example.com` creates a goal on `A4.007 16 Reading` with Build, yes/no, no schedule
-> skipped because of previous errors
Then the result is "Choose fixed days or how many times."
-> skipped because of previous errors
And `f4.ac007.user.16@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-010 The start date is today or up to 365 days later(n: "02", startDate: "the start date 2026-11-17", result: "\"The start date cannot be in the past.\"", __pickleIndex: "53", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac010.admin.02@example.com` and a Published `A4.010 02 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac010.admin.02...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac010.user.02@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac010.user.02@example.com` creates a goal on `A4.010 02 Reading` with Build, yes/no, fixed days Monday and the start date 2026-11-17
-> skipped because of previous errors
Then the result is "The start date cannot be in the past."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheStartDateIsTodayOrUpTo365DaysLater(String n, String startDate, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 143
[non-AC frame(s) hidden]
```

### F004/AC-008 Goal creation gives the first refusal in FR-006 order(n: "02", goal: "on `A4.008 02 Draft` with Build, no measurement, f"···, message: "\"This habit is not in the catalog.\"", __pickleIndex: "31", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac008.admin.02@example.com`, a Published `A4.008 02 Reading` and a Draft `A4.008 02 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac008.admin.02...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac008.user.02@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac008.user.02@example.com` creates a goal on `A4.008 02 Draft` with Build, no measurement, fixed days Monday
-> skipped because of previous errors
Then the result is "This habit is not in the catalog."
-> skipped because of previous errors
And `f4.ac008.user.02@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.GoalCreationGivesTheFirstRefusalInFR_006Order(String n, String goal, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 100
[non-AC frame(s) hidden]
```

### F004/AC-088 The Home date is today in the account's time zone, in English whatever the culture(n: "01", culture: "the invariant culture", __pickleIndex: "286", exampleTags: []) — failed

Step trace:
```
Given the registered user `f4.ac088.user.01@example.com` in `Europe/Kyiv`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And the current culture of the test is the invariant culture
-> skipped because of previous errors
When at `2026-11-16T23:30:00Z` `f4.ac088.user.01@example.com` reads the Home heading
-> skipped because of previous errors
Then the date is "Tuesday, 17 November" and the greeting is "Good evening"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheHomeDateIsTodayInTheAccountsTimeZoneInEnglishWhateverTheCulture(String n, String culture, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 1079
[non-AC frame(s) hidden]
```

### F004/AC-033 A custom habit is looked up only among the acting user's custom habits(n: "04", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "the identifier of `A4.033 04 Reading`", __pickleIndex: "144", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac033.admin.04@example.com` and a Published `A4.033 04 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac033.admin.04...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac033.b.04@example.com`, who created the custom habit `Cold shower`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.b\.{int}@example\.com`, who created the custom habit `Cold shower`")]
public void GivenTheRegisteredUserF_Ac_B_Example_ComWhoCreatedTheCustomHabitColdShower(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And the registered user `f4.ac033.a.04@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.a\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_A_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac033.a.04@example.com` creates a goal with Build, yes/no, fixed days Monday on the identifier of `A4.033 04 Reading` given as a custom habit
-> skipped because of previous errors
Then the result is "This custom habit does not exist."
-> skipped because of previous errors
And `f4.ac033.a.04@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ACustomHabitIsLookedUpOnlyAmongTheActingUsersCustomHabits(String n, String action, String habit, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 443
[non-AC frame(s) hidden]
```

### F004/AC-056 Browsing and details without an acting user give the slice 2 result — failed

Step trace:
```
Given the administrator `f4.ac056.admin@example.com` and a Published `A4.056 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac056.admin@ex...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac056.user@example.com`, who created a goal on `A4.056 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When the catalog is browsed without an acting user with the search `A4.056`, and the details of `A4.056 Reading` are read without an acting user
-> skipped because of previous errors
Then browsing contains `A4.056 Reading` with no goal count, and no tracked count
-> skipped because of previous errors
And the details have no goal count and no goals
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.BrowsingAndDetailsWithoutAnActingUserGiveTheSlice2Result() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 705
[non-AC frame(s) hidden]
```

### F004/AC-014 A habit that is not a Published catalog habit is refused as a catalog habit(n: "03", action: "starts a new goal from", habit: "the identifier of their custom habit `Cold shower`", __pickleIndex: "74", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac014.admin.03@example.com`, a Draft `A4.014 03 Draft` and an Unpublished `A4.014 03 Gone`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac014.admin.03...", "a Draft `A4.014 0...") (0,1s)
And the registered user `f4.ac014.user.03@example.com`, who created the custom habit `Cold shower`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Cold shower`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitColdShower(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac014.user.03@example.com` starts a new goal from the identifier of their custom habit `Cold shower` given as a catalog habit
-> skipped because of previous errors
Then the result is "This habit is not in the catalog."
-> skipped because of previous errors
And `f4.ac014.user.03@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AHabitThatIsNotAPublishedCatalogHabitIsRefusedAsACatalogHabit(String n, String action, String habit, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 205
[non-AC frame(s) hidden]
```

### F004/AC-034 Catalog goal counts are the acting user's own — failed

Step trace:
```
Given the administrator `f4.ac034.admin@example.com` and a Published `A4.034 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac034.admin@ex...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac034.a@example.com`, who created two goals on `A4.034 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And the registered user `f4.ac034.b@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.b@example\.com`")]
public void GivenTheRegisteredUserF_Ac_BExample_Com(int p0, int p1)
{
    throw new PendingStepException();
}

When `f4.ac034.b@example.com` browses the catalog as `f4.ac034.b@example.com` with the search `A4.034`, and reads the details of `A4.034 Reading` as `f4.ac034.b@example.com`
-> skipped because of previous errors
Then browsing gives `A4.034 Reading` the goal count 0 and the tracked count 0
-> skipped because of previous errors
And the details give the goal count 0 and no goals
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.CatalogGoalCountsAreTheActingUsersOwn() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 460
[non-AC frame(s) hidden]
```

### F004/AC-055 Anonymous visitors and administrators get the catalog without goal information(n: "05", actingUser: "the anonymous visitor", action: "browses the catalog as the anonymous visitor with "···, result: "the result is \"You are not signed in.\"", __pickleIndex: "199", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac055.admin.05@example.com` and a Published `A4.055 05 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac055.admin.05...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac055.user.05@example.com`, who created a goal on `A4.055 05 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When the anonymous visitor browses the catalog as the anonymous visitor with "Only habits I track"
-> skipped because of previous errors
Then the result is "You are not signed in."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnonymousVisitorsAndAdministratorsGetTheCatalogWithoutGoalInformation(String n, String actingUser, String action, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 688
[non-AC frame(s) hidden]
```

### F004/AC-038 A per-week goal has one evaluation period per Monday-to-Sunday week, from its first full week(n: "01", startDate: "2026-11-18", from: "2026-11-16", to: "2026-12-06", periods: "2 periods: 2026-11-23 to 2026-11-29 from `2026-11-"···, __pickleIndex: "156", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac038.user.01@example.com`, who created the custom habit `Run`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Run`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitRun(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And `f4.ac038.user.01@example.com` created a goal on their custom habit `Run` with Build, yes/no, 3 times per week and the start date 2026-11-18
-> skipped because of previous errors
When `f4.ac038.user.01@example.com` reads the evaluation calendar of the goal from 2026-11-16 to 2026-12-06
-> skipped because of previous errors
Then it has 2 periods: 2026-11-23 to 2026-11-29 from `2026-11-23T00:00:00Z` to `2026-11-30T00:00:00Z`, and 2026-11-30 to 2026-12-06 from `2026-11-30T00:00:00Z` to `2026-12-07T00:00:00Z`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.APer_WeekGoalHasOneEvaluationPeriodPerMonday_To_SundayWeekFromItsFirstFullWeek(String n, String startDate, String from, String to, String periods, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 505
[non-AC frame(s) hidden]
```

### F004/AC-016 A goal without a name of its own is named after its habit and target(n: "02", name: "the name \"Morning pages\"", goalName: "\"Morning pages\"", own: "its own", __pickleIndex: "83", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac016.admin.02@example.com` and a Published `A4.016 02 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac016.admin.02...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac016.user.02@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac016.user.02@example.com` creates a goal on `A4.016 02 Reading` with Build, duration 15 minutes, fixed days Monday to Friday and the name "Morning pages"
-> skipped because of previous errors
Then reading the goal gives the name "Morning pages", and the name is its own
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AGoalWithoutANameOfItsOwnIsNamedAfterItsHabitAndTarget(String n, String name, String goalName, String own, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 238
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "19", settings: "Build, yes/no, 0 times per week", message: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "26", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.19@example.com` and a Published `A4.007 19 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.19...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.19@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.19@example.com` creates a goal on `A4.007 19 Reading` with Build, yes/no, 0 times per week
-> skipped because of previous errors
Then the result is "Enter a number of times from 1 to 20."
-> skipped because of previous errors
And `f4.ac007.user.19@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-063 A new goal can start without a habit, but not be created without one — pending

Step trace:
```
Given the registered user `f4.ac063.user@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`")]
public void GivenTheRegisteredUserF_Ac_UserExample_Com(int p0, int p1)
{
    throw new PendingStepException();
}

When `f4.ac063.user@example.com` starts a new goal with no habit, and then creates a goal with no habit and Build, yes/no, fixed days Monday
-> skipped because of previous errors
Then the starting values have no habit, the start date 2026-11-18, and nothing else
-> skipped because of previous errors
And creating gives "Choose a habit."
-> skipped because of previous errors
And `f4.ac063.user@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ANewGoalCanStartWithoutAHabitButNotBeCreatedWithoutOne() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 783
[non-AC frame(s) hidden]
```

### F004/AC-049 A custom habit that no goal uses is deleted — pending

Step trace:
```
Given the registered user `f4.ac049.user@example.com`, who created the custom habits `Cold shower` and `Plank`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabits().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the custom habits `Cold shower` and `Plank`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheCustomHabitsColdShowerAndPlank(int p0, int p1)
{
    throw new PendingStepException();
}

And `f4.ac049.user@example.com` created a goal on their custom habit `Plank` with Build, yes/no, fixed days Monday
-> skipped because of previous errors
When `f4.ac049.user@example.com` deletes their custom habit `Cold shower`
-> skipped because of previous errors
Then the custom habit is deleted
-> skipped because of previous errors
And reading it gives "This custom habit does not exist."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ACustomHabitThatNoGoalUsesIsDeleted() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 631
[non-AC frame(s) hidden]
```

### F004/AC-069 The demo shows a goal on an Unpublished habit, a goal with its own name and an account name — failed

Step trace:
```
Given the AC database is reset, so no account and no habit exists
-> done: ArrangeSteps.GivenTheAcDatabaseIsReset() (0,2s)
And demo loading ran
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenDemoLoadingRan(String)' should have 0 parameters
When each demo account reads its goals and its profile, and `demo1@example.com` reads the Home heading
-> skipped because of previous errors
Then `demo3@example.com`'s goal "Coffee, at most 2 cups a day" exists, and reading it gives the habit name `Coffee` and that the habit is not in the catalog
-> skipped because of previous errors
And `demo2@example.com`'s goal on `Yoga` has the name "Morning yoga", which is its own
-> skipped because of previous errors
And `demo1@example.com`'s profile gives the name "Demo One"; the profiles of `demo2@example.com` and `demo3@example.com` give no name
-> skipped because of previous errors
And `demo1@example.com`'s Home heading gives the greeting "Good morning, Demo One"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenDemoLoadingRan(String)' should have 0 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheDemoShowsAGoalOnAnUnpublishedHabitAGoalWithItsOwnNameAndAnAccountName() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 840
[non-AC frame(s) hidden]
```

### F004/AC-023 The next commitment is the first commitment that has not ended(n: "03", time: "`2026-11-18T23:59:59Z`", next: "2026-11-18", __pickleIndex: "116", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac023.user.03@example.com`, who created the custom habit `Yoga`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Yoga`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitYoga(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at `2026-11-16T08:00:00Z` `f4.ac023.user.03@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days Monday, Wednesday and Friday and the start date 2026-11-16
-> skipped because of previous errors
When at `2026-11-18T23:59:59Z` `f4.ac023.user.03@example.com` lists their goals
-> skipped because of previous errors
Then the goal gives next commitment 2026-11-18 and no start date
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheNextCommitmentIsTheFirstCommitmentThatHasNotEnded(String n, String time, String next, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 329
[non-AC frame(s) hidden]
```

### F004/AC-032 An anonymous visitor gets no goal use case(n: "06", action: "browses the catalog as the anonymous visitor with "···, __pickleIndex: "140", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac032.admin.06@example.com` and a Published `A4.032 06 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac032.admin.06...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac032.user.06@example.com`, who created a goal on `A4.032 06 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When the anonymous visitor browses the catalog as the anonymous visitor with "Only habits I track"
-> skipped because of previous errors
Then the result is "You are not signed in."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnAnonymousVisitorGetsNoGoalUseCase(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 425
[non-AC frame(s) hidden]
```

### F004/AC-061 A habit published again is in the catalog again for goals — failed

Step trace:
```
Given the administrator `f4.ac061.admin@example.com` and a Published `A4.061 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac061.admin@ex...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac061.owner@example.com`, who created a goal on `A4.061 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And the administrator unpublished `A4.061 Reading` with confirmation and then published it again
-> skipped because of previous errors
And the registered user `f4.ac061.other@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.other@example\.com`")]
public void GivenTheRegisteredUserF_Ac_OtherExample_Com(int p0, int p1)
{
    throw new PendingStepException();
}

When `f4.ac061.other@example.com` creates a goal on `A4.061 Reading` with Build, yes/no, fixed days Monday
-> skipped because of previous errors
Then creating succeeds
-> skipped because of previous errors
And `f4.ac061.owner@example.com` reading their goal gives that the habit is in the catalog
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AHabitPublishedAgainIsInTheCatalogAgainForGoals() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 767
[non-AC frame(s) hidden]
```

### F004/AC-014 A habit that is not a Published catalog habit is refused as a catalog habit(n: "07", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "the identifier of their custom habit `Cold shower`", __pickleIndex: "78", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac014.admin.07@example.com`, a Draft `A4.014 07 Draft` and an Unpublished `A4.014 07 Gone`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac014.admin.07...", "a Draft `A4.014 0...") (0,1s)
And the registered user `f4.ac014.user.07@example.com`, who created the custom habit `Cold shower`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Cold shower`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitColdShower(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac014.user.07@example.com` creates a goal with Build, yes/no, fixed days Monday on the identifier of their custom habit `Cold shower` given as a catalog habit
-> skipped because of previous errors
Then the result is "This habit is not in the catalog."
-> skipped because of previous errors
And `f4.ac014.user.07@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AHabitThatIsNotAPublishedCatalogHabitIsRefusedAsACatalogHabit(String n, String action, String habit, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 205
[non-AC frame(s) hidden]
```

### F004/AC-004 A suggested starting goal position the habit does not have is refused(n: "02", position: "0", __pickleIndex: "5", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac004.admin.02@example.com` and the registered user `f4.ac004.user.02@example.com`
-> done: ArrangeSteps.GivenTheAdministratorAndTheRegisteredUser("f4.ac004.admin.02...", "f4.ac004.user.02@...") (0,2s)
And a Published `A4.004 02 Reading` with the suggested starting goals 1. Build, duration 15 minutes, fixed days Monday to Friday (marked); 2. Build, count 20 `pages`, 3 times per week; 3. Build, yes/no, fixed days on all seven days
-> done: ArrangeSteps.GivenCatalogHabits("a Published `A4.0...") (0,0s)
When `f4.ac004.user.02@example.com` starts a new goal from `A4.004 02 Reading` with the suggested starting goal position 0
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.GoalWhenSteps.WhenStartsANewGoal(String, String, String, String)' should have 3 parameters
Then the result is "This suggested starting goal does not exist."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.GoalWhenSteps.WhenStartsANewGoal(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ASuggestedStartingGoalPositionTheHabitDoesNotHaveIsRefused(String n, String position, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 35
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "01", settings: "Build, duration 15 minutes, fixed days Monday to F"···, targetText: "\"15 min, 5 days a week\"", __pickleIndex: "87", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.01@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.01@example.com` creates a goal on their custom habit `Practice` with Build, duration 15 minutes, fixed days Monday to Friday
-> skipped because of previous errors
Then reading the goal gives the target text "15 min, 5 days a week" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "03", settings: "Reduce, count 1 `sweet`, 1 time per day", targetText: "\"at most 1 sweet a day\"", __pickleIndex: "89", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.03@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.03@example.com` creates a goal on their custom habit `Practice` with Reduce, count 1 `sweet`, 1 time per day
-> skipped because of previous errors
Then reading the goal gives the target text "at most 1 sweet a day" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-010 The start date is today or up to 365 days later(n: "03", startDate: "the start date 2027-11-18", result: "creating succeeds", __pickleIndex: "54", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac010.admin.03@example.com` and a Published `A4.010 03 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac010.admin.03...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac010.user.03@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac010.user.03@example.com` creates a goal on `A4.010 03 Reading` with Build, yes/no, fixed days Monday and the start date 2027-11-18
-> skipped because of previous errors
Then the result is creating succeeds
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheStartDateIsTodayOrUpTo365DaysLater(String n, String startDate, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 143
[non-AC frame(s) hidden]
```

### F004/AC-048 A custom habit that goals use cannot be deleted — pending

Step trace:
```
Given the registered user `f4.ac048.user@example.com`, who created the custom habit `Cold shower`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the custom habit `Cold shower`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheCustomHabitColdShower(int p0, int p1)
{
    throw new PendingStepException();
}

And `f4.ac048.user@example.com` created a goal on their custom habit `Cold shower` with Build, yes/no, fixed days on all seven days
-> skipped because of previous errors
When `f4.ac048.user@example.com` deletes their custom habit `Cold shower`
-> skipped because of previous errors
Then the result is "Goals use this custom habit, so it cannot be deleted."
-> skipped because of previous errors
And reading their custom habit `Cold shower` still gives it
-> skipped because of previous errors
And reading the goal still gives the habit name `Cold shower`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ACustomHabitThatGoalsUseCannotBeDeleted() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 623
[non-AC frame(s) hidden]
```

### F004/AC-009 The edges of every target range are accepted(n: "04", settings: "Reduce, count 999 `cups`, 1 time per day", targetText: "\"at most 999 cups a day\"", __pickleIndex: "47", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac009.admin.04@example.com` and a Published `A4.009 04 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac009.admin.04...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac009.user.04@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac009.user.04@example.com` creates a goal on `A4.009 04 Reading` with Reduce, count 999 `cups`, 1 time per day
-> skipped because of previous errors
Then creating succeeds
-> skipped because of previous errors
And reading the goal gives the target text "at most 999 cups a day"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheEdgesOfEveryTargetRangeAreAccepted(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 125
[non-AC frame(s) hidden]
```

### F004/AC-035 Use cases check the acting user, the role, the identifier and the fields, in this order(n: "07", actingUser: "`f4.ac035.admin.07@example.com`", action: "starts a new goal from `A4.035 07 Draft` with the "···, message: "\"Goals are for registered users.\"", __pickleIndex: "153", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac035.admin.07@example.com` and a Draft `A4.035 07 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac035.admin.07...", "a Draft `A4.035 0...") (0,1s)
And the registered user `f4.ac035.user.07@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac035.admin.07@example.com` starts a new goal from `A4.035 07 Draft` with the suggested starting goal position 9
-> skipped because of previous errors
Then the result is "Goals are for registered users."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.UseCasesCheckTheActingUserTheRoleTheIdentifierAndTheFieldsInThisOrder(String n, String actingUser, String action, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 467
[non-AC frame(s) hidden]
```

### F004/AC-013 Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "02", submitted: "Build, duration 30 minutes with the count unit `pa"···, stored: "duration 30 minutes, no count unit", targetText: "\"30 min, 3 times a week\"", __pickleIndex: "67", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac013.admin.02@example.com` and a Published `A4.013 02 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac013.admin.02...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac013.user.02@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac013.user.02@example.com` creates a goal on `A4.013 02 Reading` with Build, duration 30 minutes with the count unit `pages`, 3 times per week
-> skipped because of previous errors
Then creating succeeds
-> skipped because of previous errors
And reading the goal gives duration 30 minutes, no count unit and the target text "30 min, 3 times a week"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ValuesThatDoNotBelongToTheChosenMeasurementOrScheduleAreNeitherRefusedNorStored(String n, String submitted, String stored, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 188
[non-AC frame(s) hidden]
```

### F004/AC-009 The edges of every target range are accepted(n: "08", settings: "Build, duration 999 hours, 1 time per day", targetText: "\"999 h a day\"", __pickleIndex: "51", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac009.admin.08@example.com` and a Published `A4.009 08 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac009.admin.08...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac009.user.08@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac009.user.08@example.com` creates a goal on `A4.009 08 Reading` with Build, duration 999 hours, 1 time per day
-> skipped because of previous errors
Then creating succeeds
-> skipped because of previous errors
And reading the goal gives the target text "999 h a day"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheEdgesOfEveryTargetRangeAreAccepted(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 125
[non-AC frame(s) hidden]
```

### F004/AC-030 A user's list never contains another user's goals — failed

Step trace:
```
Given the administrator `f4.ac030.admin@example.com` and a Published `A4.030 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac030.admin@ex...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac030.a@example.com`, who created a goal on `A4.030 Reading` with Build, yes/no, fixed days Monday and the name "A4.030 goal of A"
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And the registered user `f4.ac030.b@example.com`, who created a goal on `A4.030 Reading` with Build, yes/no, fixed days Monday and the name "A4.030 goal of B"
-> skipped because of previous errors
When `f4.ac030.b@example.com` lists their goals
-> skipped because of previous errors
Then the list is exactly "A4.030 goal of B"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AUsersListNeverContainsAnotherUsersGoals() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 402
[non-AC frame(s) hidden]
```

### F004/AC-043 The calendar range has 1 to 366 days(n: "03", from: "2026-11-18", to: "2027-11-18", result: "a calendar of 53 commitments, the first on 2026-11"···, __pickleIndex: "174", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac043.user.03@example.com`, who created the custom habit `Yoga`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Yoga`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitYoga(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And `f4.ac043.user.03@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days Wednesday and the start date 2026-11-18
-> skipped because of previous errors
When `f4.ac043.user.03@example.com` reads the evaluation calendar of the goal from 2026-11-18 to 2027-11-18
-> skipped because of previous errors
Then the result is a calendar of 53 commitments, the first on 2026-11-18 and the last on 2027-11-17
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheCalendarRangeHas1To366Days(String n, String from, String to, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 567
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "18", settings: "Build, yes/no, no number of times, per week", message: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "25", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.18@example.com` and a Published `A4.007 18 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.18...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.18@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.18@example.com` creates a goal on `A4.007 18 Reading` with Build, yes/no, no number of times, per week
-> skipped because of previous errors
Then the result is "Enter a number of times from 1 to 20."
-> skipped because of previous errors
And `f4.ac007.user.18@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-008 Goal creation gives the first refusal in FR-006 order(n: "05", goal: "on `A4.008 05 Reading` with Build, count 0 with no"···, message: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "34", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac008.admin.05@example.com`, a Published `A4.008 05 Reading` and a Draft `A4.008 05 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac008.admin.05...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac008.user.05@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac008.user.05@example.com` creates a goal on `A4.008 05 Reading` with Build, count 0 with no unit, 3 times per week
-> skipped because of previous errors
Then the result is "Enter a whole number from 1 to 999."
-> skipped because of previous errors
And `f4.ac008.user.05@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.GoalCreationGivesTheFirstRefusalInFR_006Order(String n, String goal, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 100
[non-AC frame(s) hidden]
```

### F004/AC-093 A confirmed unpublish acts even when goals were added after the count was given — failed

Step trace:
```
Given the administrator `f4.ac093.admin@example.com` and a Published `A4.093 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac093.admin@ex...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac093.a@example.com`, who created a goal on `A4.093 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And `f4.ac093.admin@example.com` unpublished `A4.093 Reading` without confirming, and the result was "confirmation needed" with the number of goals 1 and the text "1 goal uses this habit."
-> skipped because of previous errors
And the registered user `f4.ac093.b@example.com`, who then created a goal on `A4.093 Reading` with Build, yes/no, fixed days Monday
-> skipped because of previous errors
When `f4.ac093.admin@example.com` unpublishes `A4.093 Reading` with confirmation
-> skipped because of previous errors
Then `A4.093 Reading` is Unpublished
-> skipped because of previous errors
And both goals still exist for their owners
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AConfirmedUnpublishActsEvenWhenGoalsWereAddedAfterTheCountWasGiven() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 1136
[non-AC frame(s) hidden]
```

### F004/AC-060 A goal on an Unpublished habit keeps working, and no new goal can use that habit(n: "02", actingUser: "`f4.ac060.owner.02@example.com`", action: "reads the goal", result: "the result has the habit name `A4.060 02 Reading`,"···, __pickleIndex: "209", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac060.admin.02@example.com` and a Published `A4.060 02 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac060.admin.02...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac060.owner.02@example.com`, who created a goal on `A4.060 02 Reading` with Build, duration 15 minutes, fixed days Monday to Friday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And the registered user `f4.ac060.other.02@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.other\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_Other_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And the administrator unpublished `A4.060 02 Reading` with confirmation
-> skipped because of previous errors
When `f4.ac060.owner.02@example.com` reads the goal
-> skipped because of previous errors
Then the result has the habit name `A4.060 02 Reading`, that the habit is not in the catalog, Build, duration 15 minutes and fixed days Monday to Friday
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AGoalOnAnUnpublishedHabitKeepsWorkingAndNoNewGoalCanUseThatHabit(String n, String actingUser, String action, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 749
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "17", settings: "Reduce, duration 1 hours, 1 time per day", targetText: "\"at most 1 h a day\"", __pickleIndex: "103", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.17@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.17@example.com` creates a goal on their custom habit `Practice` with Reduce, duration 1 hours, 1 time per day
-> skipped because of previous errors
Then reading the goal gives the target text "at most 1 h a day" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-094 At registration, the name is checked after slice 1's checks — failed

Step trace:
```
When the anonymous visitor registers the email `f4.ac094.user@example.com` with the password `short` and a name of 61 characters
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.AccountSteps.WhenTheVisitorRegisters(String, String, Int32, String, String)' should have 3 parameters
Then registration does not succeed, and its result is not "The name can have at most 60 characters."
-> skipped because of previous errors
And registering `f4.ac094.user@example.com` afterwards with the password `walking42` and no name succeeds
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.AccountSteps.WhenTheVisitorRegisters(String, String, Int32, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AtRegistrationTheNameIsCheckedAfterSlice1SChecks() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 1142
[non-AC frame(s) hidden]
```

### F004/AC-070 Registration keeps a named browser time zone, and otherwise gives the default(n: "05", browserTimeZone: "the browser time zone `Etc/GMT+3`", timeZone: "`UTC`", __pickleIndex: "226", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac070.user.05@example.com` with the password `walking42` and the browser time zone `Etc/GMT+3`, and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When `f4.ac070.user.05@example.com` reads their time zone
-> skipped because of previous errors
Then registration succeeded and the time zone is `UTC`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.RegistrationKeepsANamedBrowserTimeZoneAndOtherwiseGivesTheDefault(String n, String browserTimeZone, String timeZone, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 846
[non-AC frame(s) hidden]
```

### F004/AC-035 Use cases check the acting user, the role, the identifier and the fields, in this order(n: "06", actingUser: "`f4.ac035.user.06@example.com`", action: "starts a new goal from `A4.035 06 Draft` with the "···, message: "\"This habit is not in the catalog.\"", __pickleIndex: "152", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac035.admin.06@example.com` and a Draft `A4.035 06 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac035.admin.06...", "a Draft `A4.035 0...") (0,1s)
And the registered user `f4.ac035.user.06@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac035.user.06@example.com` starts a new goal from `A4.035 06 Draft` with the suggested starting goal position 9
-> skipped because of previous errors
Then the result is "This habit is not in the catalog."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.UseCasesCheckTheActingUserTheRoleTheIdentifierAndTheFieldsInThisOrder(String n, String actingUser, String action, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 467
[non-AC frame(s) hidden]
```

### F004/AC-040 A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "03", schedule: "1 time per day", date: "2027-03-28", entry: "2027-03-28 from `2027-03-27T22:00:00Z` to `2027-03"···, __pickleIndex: "166", exampleTags: []) — failed

Step trace:
```
Given the registered user `f4.ac040.user.03@example.com` in `Europe/Kyiv`, who created the custom habit `Practice`
-> done: ArrangeSteps.GivenTheRegisteredUser("f4.ac040.user.03@...", "Europe/Kyiv", "created the custo...") (0,1s)
And `f4.ac040.user.03@example.com` created a goal on their custom habit `Practice` with Build, yes/no, 1 time per day and the start date 2026-11-18
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenCreatedAGoal(String, String, String, String)' should have 3 parameters
When `f4.ac040.user.03@example.com` reads the evaluation calendar of the goal from 2027-03-28 to 2027-03-28
-> skipped because of previous errors
Then it has exactly one entry: 2027-03-28 from `2027-03-27T22:00:00Z` to `2027-03-28T21:00:00Z` (23 hours)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenCreatedAGoal(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ADayOrWeekWithAClockChangeKeepsLocalMidnightsAndLasts23Or25Hours(String n, String schedule, String date, String entry, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 534
[non-AC frame(s) hidden]
```

### F004/AC-084 An account name is trimmed, and empty means no name(n: "04", name: "the name `\"\"`", result: "no name", __pickleIndex: "276", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac084.user.04@example.com` with the password `walking42` and the name `""`, and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When `f4.ac084.user.04@example.com` reads their profile
-> skipped because of previous errors
Then registration succeeded and the profile gives no name and the email `f4.ac084.user.04@example.com`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnAccountNameIsTrimmedAndEmptyMeansNoName(String n, String name, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 1034
[non-AC frame(s) hidden]
```

### F004/AC-043 The calendar range has 1 to 366 days(n: "01", from: "2026-11-24", to: "2026-11-18", result: "\"Choose a range of 1 to 366 days.\"", __pickleIndex: "172", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac043.user.01@example.com`, who created the custom habit `Yoga`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Yoga`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitYoga(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And `f4.ac043.user.01@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days Wednesday and the start date 2026-11-18
-> skipped because of previous errors
When `f4.ac043.user.01@example.com` reads the evaluation calendar of the goal from 2026-11-24 to 2026-11-18
-> skipped because of previous errors
Then the result is "Choose a range of 1 to 366 days."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheCalendarRangeHas1To366Days(String n, String from, String to, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 567
[non-AC frame(s) hidden]
```

### F004/AC-010 The start date is today or up to 365 days later(n: "04", startDate: "the start date 2027-11-19", result: "\"Choose a start date within a year from today.\"", __pickleIndex: "55", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac010.admin.04@example.com` and a Published `A4.010 04 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac010.admin.04...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac010.user.04@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac010.user.04@example.com` creates a goal on `A4.010 04 Reading` with Build, yes/no, fixed days Monday and the start date 2027-11-19
-> skipped because of previous errors
Then the result is "Choose a start date within a year from today."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheStartDateIsTodayOrUpTo365DaysLater(String n, String startDate, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 143
[non-AC frame(s) hidden]
```

### F004/AC-035 Use cases check the acting user, the role, the identifier and the fields, in this order(n: "02", actingUser: "`f4.ac035.admin.02@example.com`", action: "reads the evaluation calendar from 2026-11-24 to 2"···, message: "\"Goals are for registered users.\"", __pickleIndex: "148", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac035.admin.02@example.com` and a Draft `A4.035 02 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac035.admin.02...", "a Draft `A4.035 0...") (0,1s)
And the registered user `f4.ac035.user.02@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac035.admin.02@example.com` reads the evaluation calendar from 2026-11-24 to 2026-11-18 of an identifier that belongs to no goal
-> skipped because of previous errors
Then the result is "Goals are for registered users."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.UseCasesCheckTheActingUserTheRoleTheIdentifierAndTheFieldsInThisOrder(String n, String actingUser, String action, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 467
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "05", settings: "Build, count with no amount and the unit `pages`, "···, message: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "12", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.05@example.com` and a Published `A4.007 05 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.05...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.05@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.05@example.com` creates a goal on `A4.007 05 Reading` with Build, count with no amount and the unit `pages`, 3 times per week
-> skipped because of previous errors
Then the result is "Enter a whole number from 1 to 999."
-> skipped because of previous errors
And `f4.ac007.user.05@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-013 Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "01", submitted: "Build, count 20 `pages` with the duration unit hou"···, stored: "count 20 `pages`, no duration unit", targetText: "\"20 pages, 3 times a week\"", __pickleIndex: "66", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac013.admin.01@example.com` and a Published `A4.013 01 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac013.admin.01...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac013.user.01@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac013.user.01@example.com` creates a goal on `A4.013 01 Reading` with Build, count 20 `pages` with the duration unit hours, 3 times per week
-> skipped because of previous errors
Then creating succeeds
-> skipped because of previous errors
And reading the goal gives count 20 `pages`, no duration unit and the target text "20 pages, 3 times a week"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ValuesThatDoNotBelongToTheChosenMeasurementOrScheduleAreNeitherRefusedNorStored(String n, String submitted, String stored, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 188
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "15", settings: "Build, duration 15 with no duration unit, 3 times "···, message: "\"Choose minutes or hours.\"", __pickleIndex: "22", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.15@example.com` and a Published `A4.007 15 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.15...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.15@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.15@example.com` creates a goal on `A4.007 15 Reading` with Build, duration 15 with no duration unit, 3 times per week
-> skipped because of previous errors
Then the result is "Choose minutes or hours."
-> skipped because of previous errors
And `f4.ac007.user.15@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-026 Reading a goal that has not started gives its first version and attempt from its start date — pending

Step trace:
```
Given the registered user `f4.ac026.user@example.com`, who created the custom habit `Run`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the custom habit `Run`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheCustomHabitRun(int p0, int p1)
{
    throw new PendingStepException();
}

And `f4.ac026.user@example.com` created a goal on their custom habit `Run` with Build, yes/no, 3 times per week and the start date 2026-12-02
-> skipped because of previous errors
When `f4.ac026.user@example.com` reads the goal
-> skipped because of previous errors
Then the result has the start date 2026-12-02, first week starts 2026-12-07, the time zone `UTC` from 2026-12-02, no later version, and attempt 1, started on 2026-12-02
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ReadingAGoalThatHasNotStartedGivesItsFirstVersionAndAttemptFromItsStartDate() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 365
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "07", settings: "Build, yes/no, 1 time per week", targetText: "\"once a week\"", __pickleIndex: "93", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.07@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.07@example.com` creates a goal on their custom habit `Practice` with Build, yes/no, 1 time per week
-> skipped because of previous errors
Then reading the goal gives the target text "once a week" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-020 Every suggested starting goal of the seed catalog can be created unchanged — pending

Step trace:
```
Given the AC database is reset, so no account and no habit exists
-> done: ArrangeSteps.GivenTheAcDatabaseIsReset() (0,2s)
And demo loading ran and its catalog result was "seed catalog loaded"
-> done: ArrangeSteps.GivenDemoLoadingRan("seed catalog loaded") (0,2s)
And the registered user `f4.ac020.user@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`")]
public void GivenTheRegisteredUserF_Ac_UserExample_Com(int p0, int p1)
{
    throw new PendingStepException();
}

When for every catalog habit that browsing the catalog without an acting user returns and for every position of its suggested starting goals from 1 to their number, `f4.ac020.user@example.com` starts a new goal from that habit with that position and creates a goal with the starting values unchanged
-> skipped because of previous errors
Then every creation succeeds
-> skipped because of previous errors
And `f4.ac020.user@example.com`'s goal list has one goal per creation
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EverySuggestedStartingGoalOfTheSeedCatalogCanBeCreatedUnchanged() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 303
[non-AC frame(s) hidden]
```

### F004/AC-040 A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "05", schedule: "1 time per week", date: "2027-03-28", entry: "2027-03-22 to 2027-03-28 from `2027-03-21T22:00:00"···, __pickleIndex: "168", exampleTags: []) — failed

Step trace:
```
Given the registered user `f4.ac040.user.05@example.com` in `Europe/Kyiv`, who created the custom habit `Practice`
-> done: ArrangeSteps.GivenTheRegisteredUser("f4.ac040.user.05@...", "Europe/Kyiv", "created the custo...") (0,1s)
And `f4.ac040.user.05@example.com` created a goal on their custom habit `Practice` with Build, yes/no, 1 time per week and the start date 2026-11-18
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenCreatedAGoal(String, String, String, String)' should have 3 parameters
When `f4.ac040.user.05@example.com` reads the evaluation calendar of the goal from 2027-03-28 to 2027-03-28
-> skipped because of previous errors
Then it has exactly one entry: 2027-03-22 to 2027-03-28 from `2027-03-21T22:00:00Z` to `2027-03-28T21:00:00Z` (167 hours)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenCreatedAGoal(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ADayOrWeekWithAClockChangeKeepsLocalMidnightsAndLasts23Or25Hours(String n, String schedule, String date, String entry, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 534
[non-AC frame(s) hidden]
```

### F004/AC-013 Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "05", submitted: "Build, yes/no, 2 times per day, with the fixed day"···, stored: "2 times per day, no fixed days", targetText: "\"2 times a day\"", __pickleIndex: "70", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac013.admin.05@example.com` and a Published `A4.013 05 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac013.admin.05...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac013.user.05@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac013.user.05@example.com` creates a goal on `A4.013 05 Reading` with Build, yes/no, 2 times per day, with the fixed day Monday
-> skipped because of previous errors
Then creating succeeds
-> skipped because of previous errors
And reading the goal gives 2 times per day, no fixed days and the target text "2 times a day"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ValuesThatDoNotBelongToTheChosenMeasurementOrScheduleAreNeitherRefusedNorStored(String n, String submitted, String stored, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 188
[non-AC frame(s) hidden]
```

### F004/AC-029 Another user's goal does not exist for them(n: "04", action: "reads the evaluation calendar from 2026-11-18 to 2"···, goal: "an identifier that the test creates and that belon"···, __pickleIndex: "127", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac029.admin.04@example.com` and a Published `A4.029 04 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac029.admin.04...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac029.a.04@example.com`, who created a goal on `A4.029 04 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And the registered user `f4.ac029.b.04@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.b\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_B_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac029.b.04@example.com` reads the evaluation calendar from 2026-11-18 to 2026-11-24 of an identifier that the test creates and that belongs to no goal
-> skipped because of previous errors
Then the result is "This goal does not exist."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnotherUsersGoalDoesNotExistForThem(String n, String action, String goal, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 387
[non-AC frame(s) hidden]
```

### F004/AC-037 A per-day goal has one evaluation period per day — failed

Step trace:
```
Given the registered user `f4.ac037.user@example.com` in `America/New_York`, who created the custom habit `Water`
-> done: ArrangeSteps.GivenTheRegisteredUser("f4.ac037.user@exa...", "America/New_York", "created the custo...") (0,1s)
And `f4.ac037.user@example.com` created a goal on their custom habit `Water` with Build, yes/no, 2 times per day and the start date 2026-11-18
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenCreatedAGoal(String, String, String, String)' should have 3 parameters
When `f4.ac037.user@example.com` reads the evaluation calendar of the goal from 2026-11-16 to 2026-11-24
-> skipped because of previous errors
Then it has 7 evaluation periods, one for each date from 2026-11-18 to 2026-11-24, in date order
-> skipped because of previous errors
And each runs from 05:00:00Z of its date to 05:00:00Z of the next date: the first from `2026-11-18T05:00:00Z` to `2026-11-19T05:00:00Z`, the last from `2026-11-24T05:00:00Z` to `2026-11-25T05:00:00Z`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenCreatedAGoal(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.APer_DayGoalHasOneEvaluationPeriodPerDay() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 498
[non-AC frame(s) hidden]
```

### F004/AC-029 Another user's goal does not exist for them(n: "01", action: "reads the goal", goal: "`f4.ac029.a.01@example.com`'s goal", __pickleIndex: "124", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac029.admin.01@example.com` and a Published `A4.029 01 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac029.admin.01...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac029.a.01@example.com`, who created a goal on `A4.029 01 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And the registered user `f4.ac029.b.01@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.b\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_B_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac029.b.01@example.com` reads the goal `f4.ac029.a.01@example.com`'s goal
-> skipped because of previous errors
Then the result is "This goal does not exist."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnotherUsersGoalDoesNotExistForThem(String n, String action, String goal, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 387
[non-AC frame(s) hidden]
```

### F004/AC-008 Goal creation gives the first refusal in FR-006 order(n: "01", goal: "with no habit, no Build or Reduce, yes/no, fixed d"···, message: "\"Choose a habit.\"", __pickleIndex: "30", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac008.admin.01@example.com`, a Published `A4.008 01 Reading` and a Draft `A4.008 01 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac008.admin.01...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac008.user.01@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac008.user.01@example.com` creates a goal with no habit, no Build or Reduce, yes/no, fixed days Monday
-> skipped because of previous errors
Then the result is "Choose a habit."
-> skipped because of previous errors
And `f4.ac008.user.01@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.GoalCreationGivesTheFirstRefusalInFR_006Order(String n, String goal, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 100
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "22", settings: "Reduce, yes/no, fixed days Sunday and Monday", targetText: "\"avoided on Mon, Sun\"", __pickleIndex: "108", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.22@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.22@example.com` creates a goal on their custom habit `Practice` with Reduce, yes/no, fixed days Sunday and Monday
-> skipped because of previous errors
Then reading the goal gives the target text "avoided on Mon, Sun" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-008 Goal creation gives the first refusal in FR-006 order(n: "08", goal: "on `A4.008 08 Reading` with Build, duration 15 wit"···, message: "\"Choose minutes or hours.\"", __pickleIndex: "37", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac008.admin.08@example.com`, a Published `A4.008 08 Reading` and a Draft `A4.008 08 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac008.admin.08...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac008.user.08@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac008.user.08@example.com` creates a goal on `A4.008 08 Reading` with Build, duration 15 with no duration unit, no schedule
-> skipped because of previous errors
Then the result is "Choose minutes or hours."
-> skipped because of previous errors
And `f4.ac008.user.08@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.GoalCreationGivesTheFirstRefusalInFR_006Order(String n, String goal, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 100
[non-AC frame(s) hidden]
```

### F004/AC-088 The Home date is today in the account's time zone, in English whatever the culture(n: "03", culture: "`de-DE`", __pickleIndex: "288", exampleTags: []) — failed

Step trace:
```
Given the registered user `f4.ac088.user.03@example.com` in `Europe/Kyiv`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And the current culture of the test is `de-DE`
-> skipped because of previous errors
When at `2026-11-16T23:30:00Z` `f4.ac088.user.03@example.com` reads the Home heading
-> skipped because of previous errors
Then the date is "Tuesday, 17 November" and the greeting is "Good evening"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheHomeDateIsTodayInTheAccountsTimeZoneInEnglishWhateverTheCulture(String n, String culture, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 1079
[non-AC frame(s) hidden]
```

### F004/AC-013 Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "06", submitted: "Build, count 20 `\"  pages  \"`, 3 times per week", stored: "count 20 `pages`", targetText: "\"20 pages, 3 times a week\"", __pickleIndex: "71", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac013.admin.06@example.com` and a Published `A4.013 06 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac013.admin.06...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac013.user.06@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac013.user.06@example.com` creates a goal on `A4.013 06 Reading` with Build, count 20 `"  pages  "`, 3 times per week
-> skipped because of previous errors
Then creating succeeds
-> skipped because of previous errors
And reading the goal gives count 20 `pages` and the target text "20 pages, 3 times a week"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ValuesThatDoNotBelongToTheChosenMeasurementOrScheduleAreNeitherRefusedNorStored(String n, String submitted, String stored, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 188
[non-AC frame(s) hidden]
```

### F004/AC-022 Goals are listed by name ignoring case, then in creation order — pending

Step trace:
```
Given the registered user `f4.ac022.user@example.com`, who created the custom habit `Walk`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the custom habit `Walk`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheCustomHabitWalk(int p0, int p1)
{
    throw new PendingStepException();
}

And `f4.ac022.user@example.com` created goals on their custom habit `Walk` with Build, yes/no, fixed days Monday, named "beta" at `2026-11-18T10:00:01Z`, "Alpha" at `2026-11-18T10:00:02Z`, "Äpfel" at `2026-11-18T10:00:03Z`, "alpha" at `2026-11-18T10:00:04Z` and "zebra" at `2026-11-18T10:00:05Z`, in this order
-> skipped because of previous errors
When at `2026-11-18T10:00:06Z` `f4.ac022.user@example.com` lists their goals
-> skipped because of previous errors
Then the names are "Alpha", "alpha", "beta", "zebra", "Äpfel", in this order
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.GoalsAreListedByNameIgnoringCaseThenInCreationOrder() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 322
[non-AC frame(s) hidden]
```

### F004/AC-086 The Home greeting follows the time of day in the account's time zone(n: "01", time: "`2026-11-18T02:59:00Z` (04:59 in Kyiv)", greeting: "\"Good evening, Yuri\"", __pickleIndex: "278", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac086.user.01@example.com` with the password `walking42`, the browser time zone `Europe/Kyiv` and the name "Yuri", and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When at `2026-11-18T02:59:00Z` (04:59 in Kyiv) `f4.ac086.user.01@example.com` reads the Home heading
-> skipped because of previous errors
Then the greeting is "Good evening, Yuri" and the date is "Wednesday, 18 November"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheHomeGreetingFollowsTheTimeOfDayInTheAccountsTimeZone(String n, String time, String greeting, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 1056
[non-AC frame(s) hidden]
```

### F004/AC-008 Goal creation gives the first refusal in FR-006 order(n: "03", goal: "on `A4.008 03 Reading` with no Build or Reduce, no"···, message: "\"Choose Build or Reduce.\"", __pickleIndex: "32", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac008.admin.03@example.com`, a Published `A4.008 03 Reading` and a Draft `A4.008 03 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac008.admin.03...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac008.user.03@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac008.user.03@example.com` creates a goal on `A4.008 03 Reading` with no Build or Reduce, no measurement, fixed days Monday
-> skipped because of previous errors
Then the result is "Choose Build or Reduce."
-> skipped because of previous errors
And `f4.ac008.user.03@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.GoalCreationGivesTheFirstRefusalInFR_006Order(String n, String goal, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 100
[non-AC frame(s) hidden]
```

### F004/AC-002 Starting from a suggested starting goal position gives that suggestion — failed

Step trace:
```
Given the administrator `f4.ac002.admin@example.com` and the registered user `f4.ac002.user@example.com`
-> done: ArrangeSteps.GivenTheAdministratorAndTheRegisteredUser("f4.ac002.admin@ex...", "f4.ac002.user@exa...") (0,1s)
And a Published `A4.002 Reading` with the suggested starting goals 1. Build, duration 15 minutes, fixed days Monday to Friday (marked); 2. Build, count 20 `pages`, 3 times per week
-> done: ArrangeSteps.GivenCatalogHabits("a Published `A4.0...") (0,0s)
When `f4.ac002.user@example.com` starts a new goal from `A4.002 Reading` with the suggested starting goal position 2
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.GoalWhenSteps.WhenStartsANewGoal(String, String, String, String)' should have 3 parameters
Then the starting values are: the habit `A4.002 Reading` (catalog), Build, count 20 `pages`, 3 times per week, the start date 2026-11-18, no name, and the catalog suggestion text "20 pages, 3 times a week"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.GoalWhenSteps.WhenStartsANewGoal(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.StartingFromASuggestedStartingGoalPositionGivesThatSuggestion() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 21
[non-AC frame(s) hidden]
```

### F004/AC-086 The Home greeting follows the time of day in the account's time zone(n: "06", time: "`2026-11-18T16:00:00Z` (18:00 in Kyiv)", greeting: "\"Good evening, Yuri\"", __pickleIndex: "283", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac086.user.06@example.com` with the password `walking42`, the browser time zone `Europe/Kyiv` and the name "Yuri", and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When at `2026-11-18T16:00:00Z` (18:00 in Kyiv) `f4.ac086.user.06@example.com` reads the Home heading
-> skipped because of previous errors
Then the greeting is "Good evening, Yuri" and the date is "Wednesday, 18 November"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheHomeGreetingFollowsTheTimeOfDayInTheAccountsTimeZone(String n, String time, String greeting, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 1056
[non-AC frame(s) hidden]
```

### F004/AC-023 The next commitment is the first commitment that has not ended(n: "02", time: "`2026-11-18T00:00:00Z`", next: "2026-11-18", __pickleIndex: "115", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac023.user.02@example.com`, who created the custom habit `Yoga`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Yoga`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitYoga(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at `2026-11-16T08:00:00Z` `f4.ac023.user.02@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days Monday, Wednesday and Friday and the start date 2026-11-16
-> skipped because of previous errors
When at `2026-11-18T00:00:00Z` `f4.ac023.user.02@example.com` lists their goals
-> skipped because of previous errors
Then the goal gives next commitment 2026-11-18 and no start date
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheNextCommitmentIsTheFirstCommitmentThatHasNotEnded(String n, String time, String next, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 329
[non-AC frame(s) hidden]
```

### F004/AC-060 A goal on an Unpublished habit keeps working, and no new goal can use that habit(n: "05", actingUser: "`f4.ac060.other.05@example.com`", action: "creates a goal on `A4.060 05 Reading` with Build, "···, result: "the result is \"This habit is not in the catalog.\"···, __pickleIndex: "212", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac060.admin.05@example.com` and a Published `A4.060 05 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac060.admin.05...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac060.owner.05@example.com`, who created a goal on `A4.060 05 Reading` with Build, duration 15 minutes, fixed days Monday to Friday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And the registered user `f4.ac060.other.05@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.other\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_Other_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And the administrator unpublished `A4.060 05 Reading` with confirmation
-> skipped because of previous errors
When `f4.ac060.other.05@example.com` creates a goal on `A4.060 05 Reading` with Build, yes/no, fixed days Monday
-> skipped because of previous errors
Then the result is "This habit is not in the catalog."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AGoalOnAnUnpublishedHabitKeepsWorkingAndNoNewGoalCanUseThatHabit(String n, String actingUser, String action, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 749
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "04", settings: "Build, yes/no, fixed days on all seven days", targetText: "\"daily\"", __pickleIndex: "90", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.04@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.04@example.com` creates a goal on their custom habit `Practice` with Build, yes/no, fixed days on all seven days
-> skipped because of previous errors
Then reading the goal gives the target text "daily" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-031 An administrator gets no goal use case(n: "02", action: "creates a goal on `A4.031 02 Reading` with Build, "···, __pickleIndex: "130", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac031.admin.02@example.com` and a Published `A4.031 02 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac031.admin.02...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac031.user.02@example.com`, who created a goal on `A4.031 02 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When `f4.ac031.admin.02@example.com` creates a goal on `A4.031 02 Reading` with Build, yes/no, fixed days Monday
-> skipped because of previous errors
Then the result is "Goals are for registered users." and no goal is returned
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnAdministratorGetsNoGoalUseCase(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 409
[non-AC frame(s) hidden]
```

### F004/AC-008 Goal creation gives the first refusal in FR-006 order(n: "06", goal: "on `A4.008 06 Reading` with Build, count 20 with n"···, message: "\"Enter a unit, for example pages.\"", __pickleIndex: "35", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac008.admin.06@example.com`, a Published `A4.008 06 Reading` and a Draft `A4.008 06 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac008.admin.06...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac008.user.06@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac008.user.06@example.com` creates a goal on `A4.008 06 Reading` with Build, count 20 with no unit, no schedule
-> skipped because of previous errors
Then the result is "Enter a unit, for example pages."
-> skipped because of previous errors
And `f4.ac008.user.06@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.GoalCreationGivesTheFirstRefusalInFR_006Order(String n, String goal, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 100
[non-AC frame(s) hidden]
```

### F004/AC-008 Goal creation gives the first refusal in FR-006 order(n: "13", goal: "on `A4.008 13 Reading` with Build, yes/no, fixed d"···, message: "\"The start date cannot be in the past.\"", __pickleIndex: "42", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac008.admin.13@example.com`, a Published `A4.008 13 Reading` and a Draft `A4.008 13 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac008.admin.13...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac008.user.13@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac008.user.13@example.com` creates a goal on `A4.008 13 Reading` with Build, yes/no, fixed days Monday, the start date 2026-11-17 and a name of 61 characters
-> skipped because of previous errors
Then the result is "The start date cannot be in the past."
-> skipped because of previous errors
And `f4.ac008.user.13@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.GoalCreationGivesTheFirstRefusalInFR_006Order(String n, String goal, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 100
[non-AC frame(s) hidden]
```

### F004/AC-084 An account name is trimmed, and empty means no name(n: "01", name: "the name \"  Yuri  \"", result: "the name \"Yuri\"", __pickleIndex: "273", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac084.user.01@example.com` with the password `walking42` and the name "  Yuri  ", and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When `f4.ac084.user.01@example.com` reads their profile
-> skipped because of previous errors
Then registration succeeded and the profile gives the name "Yuri" and the email `f4.ac084.user.01@example.com`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnAccountNameIsTrimmedAndEmptyMeansNoName(String n, String name, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 1034
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "08", settings: "Build, duration 2 hours, 3 times per day", targetText: "\"2 h, 3 times a day\"", __pickleIndex: "94", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.08@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.08@example.com` creates a goal on their custom habit `Practice` with Build, duration 2 hours, 3 times per day
-> skipped because of previous errors
Then reading the goal gives the target text "2 h, 3 times a day" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-008 Goal creation gives the first refusal in FR-006 order(n: "09", goal: "on `A4.008 09 Reading` with Build, yes/no, fixed d"···, message: "\"Choose at least one day.\"", __pickleIndex: "38", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac008.admin.09@example.com`, a Published `A4.008 09 Reading` and a Draft `A4.008 09 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac008.admin.09...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac008.user.09@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac008.user.09@example.com` creates a goal on `A4.008 09 Reading` with Build, yes/no, fixed days with no day, and the start date 2026-11-17
-> skipped because of previous errors
Then the result is "Choose at least one day."
-> skipped because of previous errors
And `f4.ac008.user.09@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.GoalCreationGivesTheFirstRefusalInFR_006Order(String n, String goal, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 100
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "23", settings: "Reduce, yes/no, fixed days Wednesday", targetText: "\"avoided on Wed\"", __pickleIndex: "109", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.23@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.23@example.com` creates a goal on their custom habit `Practice` with Reduce, yes/no, fixed days Wednesday
-> skipped because of previous errors
Then reading the goal gives the target text "avoided on Wed" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-033 A custom habit is looked up only among the acting user's custom habits(n: "01", action: "starts a new goal from", habit: "`f4.ac033.b.01@example.com`'s custom habit `Cold s"···, __pickleIndex: "141", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac033.admin.01@example.com` and a Published `A4.033 01 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac033.admin.01...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac033.b.01@example.com`, who created the custom habit `Cold shower`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.b\.{int}@example\.com`, who created the custom habit `Cold shower`")]
public void GivenTheRegisteredUserF_Ac_B_Example_ComWhoCreatedTheCustomHabitColdShower(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And the registered user `f4.ac033.a.01@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.a\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_A_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac033.a.01@example.com` starts a new goal from `f4.ac033.b.01@example.com`'s custom habit `Cold shower` given as a custom habit
-> skipped because of previous errors
Then the result is "This custom habit does not exist."
-> skipped because of previous errors
And `f4.ac033.a.01@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ACustomHabitIsLookedUpOnlyAmongTheActingUsersCustomHabits(String n, String action, String habit, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 443
[non-AC frame(s) hidden]
```

### F004/AC-029 Another user's goal does not exist for them(n: "03", action: "reads the goal", goal: "an identifier that the test creates and that belon"···, __pickleIndex: "126", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac029.admin.03@example.com` and a Published `A4.029 03 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac029.admin.03...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac029.a.03@example.com`, who created a goal on `A4.029 03 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And the registered user `f4.ac029.b.03@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.b\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_B_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac029.b.03@example.com` reads the goal an identifier that the test creates and that belongs to no goal
-> skipped because of previous errors
Then the result is "This goal does not exist."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnotherUsersGoalDoesNotExistForThem(String n, String action, String goal, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 387
[non-AC frame(s) hidden]
```

### F004/AC-043 The calendar range has 1 to 366 days(n: "02", from: "2026-11-18", to: "2027-11-19", result: "\"Choose a range of 1 to 366 days.\"", __pickleIndex: "173", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac043.user.02@example.com`, who created the custom habit `Yoga`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Yoga`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitYoga(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And `f4.ac043.user.02@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days Wednesday and the start date 2026-11-18
-> skipped because of previous errors
When `f4.ac043.user.02@example.com` reads the evaluation calendar of the goal from 2026-11-18 to 2027-11-19
-> skipped because of previous errors
Then the result is "Choose a range of 1 to 366 days."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheCalendarRangeHas1To366Days(String n, String from, String to, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 567
[non-AC frame(s) hidden]
```

### F004/AC-057 Unpublishing a habit that goals use needs confirmation and gives only the number of goals(n: "01", goals: "`f4.ac057.a.01@example.com` created one goal and `"···, count: "2", text: "\"2 goals use this habit.\"", __pickleIndex: "202", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac057.admin.01@example.com` and a Published `A4.057 01 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac057.admin.01...", "a Published `A4.0...") (0,1s)
And the registered users `f4.ac057.a.01@example.com` and `f4.ac057.b.01@example.com`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUsers(String, String, String, String)' should have 2 parameters
And `f4.ac057.a.01@example.com` created one goal and `f4.ac057.b.01@example.com` created one goal, each on `A4.057 01 Reading` with Build, yes/no, fixed days Monday
-> skipped because of previous errors
When `f4.ac057.admin.01@example.com` unpublishes `A4.057 01 Reading` without confirming
-> skipped because of previous errors
Then the result is "confirmation needed" with the number of goals 2 and the text "2 goals use this habit."
-> skipped because of previous errors
And the result carries nothing else: no email, no goal name, no goal identifier and no setting
-> skipped because of previous errors
And browsing the catalog without an acting user with the search `A4.057 01` still contains `A4.057 01 Reading`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUsers(String, String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.UnpublishingAHabitThatGoalsUseNeedsConfirmationAndGivesOnlyTheNumberOfGoals(String n, String goals, String count, String text, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 715
[non-AC frame(s) hidden]
```

### F004/AC-055 Anonymous visitors and administrators get the catalog without goal information(n: "02", actingUser: "`f4.ac055.admin.02@example.com`", action: "browses the catalog as `f4.ac055.admin.02@example."···, result: "the result contains `A4.055 02 Reading` with no go"···, __pickleIndex: "196", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac055.admin.02@example.com` and a Published `A4.055 02 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac055.admin.02...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac055.user.02@example.com`, who created a goal on `A4.055 02 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When `f4.ac055.admin.02@example.com` browses the catalog as `f4.ac055.admin.02@example.com` with the search `A4.055 02`
-> skipped because of previous errors
Then the result contains `A4.055 02 Reading` with no goal count, and no tracked count
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnonymousVisitorsAndAdministratorsGetTheCatalogWithoutGoalInformation(String n, String actingUser, String action, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 688
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "05", settings: "Build, yes/no, 1 time per day", targetText: "\"once a day\"", __pickleIndex: "91", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.05@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.05@example.com` creates a goal on their custom habit `Practice` with Build, yes/no, 1 time per day
-> skipped because of previous errors
Then reading the goal gives the target text "once a day" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-004 A suggested starting goal position the habit does not have is refused(n: "01", position: "4", __pickleIndex: "4", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac004.admin.01@example.com` and the registered user `f4.ac004.user.01@example.com`
-> done: ArrangeSteps.GivenTheAdministratorAndTheRegisteredUser("f4.ac004.admin.01...", "f4.ac004.user.01@...") (0,2s)
And a Published `A4.004 01 Reading` with the suggested starting goals 1. Build, duration 15 minutes, fixed days Monday to Friday (marked); 2. Build, count 20 `pages`, 3 times per week; 3. Build, yes/no, fixed days on all seven days
-> done: ArrangeSteps.GivenCatalogHabits("a Published `A4.0...") (0,0s)
When `f4.ac004.user.01@example.com` starts a new goal from `A4.004 01 Reading` with the suggested starting goal position 4
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.GoalWhenSteps.WhenStartsANewGoal(String, String, String, String)' should have 3 parameters
Then the result is "This suggested starting goal does not exist."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.GoalWhenSteps.WhenStartsANewGoal(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ASuggestedStartingGoalPositionTheHabitDoesNotHaveIsRefused(String n, String position, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 35
[non-AC frame(s) hidden]
```

### F004/AC-086 The Home greeting follows the time of day in the account's time zone(n: "05", time: "`2026-11-18T15:59:00Z` (17:59 in Kyiv)", greeting: "\"Good afternoon, Yuri\"", __pickleIndex: "282", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac086.user.05@example.com` with the password `walking42`, the browser time zone `Europe/Kyiv` and the name "Yuri", and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When at `2026-11-18T15:59:00Z` (17:59 in Kyiv) `f4.ac086.user.05@example.com` reads the Home heading
-> skipped because of previous errors
Then the greeting is "Good afternoon, Yuri" and the date is "Wednesday, 18 November"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheHomeGreetingFollowsTheTimeOfDayInTheAccountsTimeZone(String n, String time, String greeting, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 1056
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "19", settings: "Reduce, yes/no, fixed days Saturday and Sunday", targetText: "\"avoided on Sat, Sun\"", __pickleIndex: "105", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.19@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.19@example.com` creates a goal on their custom habit `Practice` with Reduce, yes/no, fixed days Saturday and Sunday
-> skipped because of previous errors
Then reading the goal gives the target text "avoided on Sat, Sun" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-070 Registration keeps a named browser time zone, and otherwise gives the default(n: "02", browserTimeZone: "no browser time zone", timeZone: "`UTC`", __pickleIndex: "223", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac070.user.02@example.com` with the password `walking42` and no browser time zone, and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When `f4.ac070.user.02@example.com` reads their time zone
-> skipped because of previous errors
Then registration succeeded and the time zone is `UTC`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.RegistrationKeepsANamedBrowserTimeZoneAndOtherwiseGivesTheDefault(String n, String browserTimeZone, String timeZone, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 846
[non-AC frame(s) hidden]
```

### F004/AC-009 The edges of every target range are accepted(n: "03", settings: "Reduce, count 0 `cups`, 1 time per day", targetText: "\"at most 0 cups a day\"", __pickleIndex: "46", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac009.admin.03@example.com` and a Published `A4.009 03 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac009.admin.03...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac009.user.03@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac009.user.03@example.com` creates a goal on `A4.009 03 Reading` with Reduce, count 0 `cups`, 1 time per day
-> skipped because of previous errors
Then creating succeeds
-> skipped because of previous errors
And reading the goal gives the target text "at most 0 cups a day"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheEdgesOfEveryTargetRangeAreAccepted(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 125
[non-AC frame(s) hidden]
```

### F004/AC-070 Registration keeps a named browser time zone, and otherwise gives the default(n: "13", browserTimeZone: "the browser time zone `Asia/Calcutta`", timeZone: "`Asia/Calcutta`", __pickleIndex: "234", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac070.user.13@example.com` with the password `walking42` and the browser time zone `Asia/Calcutta`, and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When `f4.ac070.user.13@example.com` reads their time zone
-> skipped because of previous errors
Then registration succeeded and the time zone is `Asia/Calcutta`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.RegistrationKeepsANamedBrowserTimeZoneAndOtherwiseGivesTheDefault(String n, String browserTimeZone, String timeZone, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 846
[non-AC frame(s) hidden]
```

### F004/AC-013 Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "04", submitted: "Build, yes/no, fixed days Monday and Wednesday, wi"···, stored: "fixed days Monday and Wednesday, no number of time"···, targetText: "\"2 days a week\"", __pickleIndex: "69", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac013.admin.04@example.com` and a Published `A4.013 04 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac013.admin.04...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac013.user.04@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac013.user.04@example.com` creates a goal on `A4.013 04 Reading` with Build, yes/no, fixed days Monday and Wednesday, with 5 times per week
-> skipped because of previous errors
Then creating succeeds
-> skipped because of previous errors
And reading the goal gives fixed days Monday and Wednesday, no number of times and the target text "2 days a week"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ValuesThatDoNotBelongToTheChosenMeasurementOrScheduleAreNeitherRefusedNorStored(String n, String submitted, String stored, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 188
[non-AC frame(s) hidden]
```

### F004/AC-006 Creating a goal with changed values keeps the changed values — failed

Step trace:
```
Given the administrator `f4.ac006.admin@example.com` and the registered user `f4.ac006.user@example.com`
-> done: ArrangeSteps.GivenTheAdministratorAndTheRegisteredUser("f4.ac006.admin@ex...", "f4.ac006.user@exa...") (0,2s)
And a Published `A4.006 Reading` with the suggested starting goal Build, duration 15 minutes, fixed days Monday to Friday
-> done: ArrangeSteps.GivenCatalogHabits("a Published `A4.0...") (0,0s)
And `f4.ac006.user@example.com` started a new goal from `A4.006 Reading`
-> done: ArrangeSteps.GivenStartedANewGoal("`f4.ac006.user@ex...", "`A4.006 Reading`") (0,0s)
When `f4.ac006.user@example.com` creates a goal on `A4.006 Reading` with Reduce, count 2 `cups`, 1 time per day
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.GoalWhenSteps.WhenCreatesAGoal(String, String, String, String)' should have 3 parameters
Then creating succeeds
-> skipped because of previous errors
And reading the goal gives Reduce, count 2 `cups`, 1 time per day, the schedule kind Period-based, the target text "at most 2 cups a day" and the name "A4.006 Reading, at most 2 cups a day"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.GoalWhenSteps.WhenCreatesAGoal(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.CreatingAGoalWithChangedValuesKeepsTheChangedValues() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 59
[non-AC frame(s) hidden]
```

### F004/AC-070 Registration keeps a named browser time zone, and otherwise gives the default(n: "04", browserTimeZone: "the browser time zone `FLE Standard Time`", timeZone: "`UTC`", __pickleIndex: "225", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac070.user.04@example.com` with the password `walking42` and the browser time zone `FLE Standard Time`, and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When `f4.ac070.user.04@example.com` reads their time zone
-> skipped because of previous errors
Then registration succeeded and the time zone is `UTC`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.RegistrationKeepsANamedBrowserTimeZoneAndOtherwiseGivesTheDefault(String n, String browserTimeZone, String timeZone, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 846
[non-AC frame(s) hidden]
```

### F004/AC-060 A goal on an Unpublished habit keeps working, and no new goal can use that habit(n: "03", actingUser: "`f4.ac060.owner.03@example.com`", action: "starts a new goal from `A4.060 03 Reading`", result: "the result is \"This habit is not in the catalog.\"···, __pickleIndex: "210", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac060.admin.03@example.com` and a Published `A4.060 03 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac060.admin.03...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac060.owner.03@example.com`, who created a goal on `A4.060 03 Reading` with Build, duration 15 minutes, fixed days Monday to Friday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And the registered user `f4.ac060.other.03@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.other\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_Other_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And the administrator unpublished `A4.060 03 Reading` with confirmation
-> skipped because of previous errors
When `f4.ac060.owner.03@example.com` starts a new goal from `A4.060 03 Reading`
-> skipped because of previous errors
Then the result is "This habit is not in the catalog."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AGoalOnAnUnpublishedHabitKeepsWorkingAndNoNewGoalCanUseThatHabit(String n, String actingUser, String action, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 749
[non-AC frame(s) hidden]
```

### F004/AC-033 A custom habit is looked up only among the acting user's custom habits(n: "05", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "an identifier that the test creates and that belon"···, __pickleIndex: "145", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac033.admin.05@example.com` and a Published `A4.033 05 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac033.admin.05...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac033.b.05@example.com`, who created the custom habit `Cold shower`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.b\.{int}@example\.com`, who created the custom habit `Cold shower`")]
public void GivenTheRegisteredUserF_Ac_B_Example_ComWhoCreatedTheCustomHabitColdShower(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And the registered user `f4.ac033.a.05@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.a\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_A_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac033.a.05@example.com` creates a goal with Build, yes/no, fixed days Monday on an identifier that the test creates and that belongs to no habit given as a custom habit
-> skipped because of previous errors
Then the result is "This custom habit does not exist."
-> skipped because of previous errors
And `f4.ac033.a.05@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ACustomHabitIsLookedUpOnlyAmongTheActingUsersCustomHabits(String n, String action, String habit, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 443
[non-AC frame(s) hidden]
```

### F004/AC-035 Use cases check the acting user, the role, the identifier and the fields, in this order(n: "05", actingUser: "`f4.ac035.admin.05@example.com`", action: "creates a goal on `A4.035 05 Draft` with no Build "···, message: "\"Goals are for registered users.\"", __pickleIndex: "151", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac035.admin.05@example.com` and a Draft `A4.035 05 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac035.admin.05...", "a Draft `A4.035 0...") (0,1s)
And the registered user `f4.ac035.user.05@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac035.admin.05@example.com` creates a goal on `A4.035 05 Draft` with no Build or Reduce
-> skipped because of previous errors
Then the result is "Goals are for registered users."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.UseCasesCheckTheActingUserTheRoleTheIdentifierAndTheFieldsInThisOrder(String n, String actingUser, String action, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 467
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "18", settings: "Reduce, yes/no, fixed days Monday to Friday", targetText: "\"avoided on Mon–Fri\"", __pickleIndex: "104", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.18@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.18@example.com` creates a goal on their custom habit `Practice` with Reduce, yes/no, fixed days Monday to Friday
-> skipped because of previous errors
Then reading the goal gives the target text "avoided on Mon–Fri" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-031 An administrator gets no goal use case(n: "05", action: "reads the evaluation calendar of `f4.ac031.user.05"···, __pickleIndex: "133", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac031.admin.05@example.com` and a Published `A4.031 05 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac031.admin.05...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac031.user.05@example.com`, who created a goal on `A4.031 05 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When `f4.ac031.admin.05@example.com` reads the evaluation calendar of `f4.ac031.user.05@example.com`'s goal from 2026-11-18 to 2026-11-24
-> skipped because of previous errors
Then the result is "Goals are for registered users." and no goal is returned
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnAdministratorGetsNoGoalUseCase(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 409
[non-AC frame(s) hidden]
```

### F004/AC-062 A renamed catalog habit renames the goals based on it — failed

Step trace:
```
Given the administrator `f4.ac062.admin@example.com` and a Published `A4.062 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac062.admin@ex...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac062.user@example.com`, who created a goal on `A4.062 Reading` with Build, duration 15 minutes, fixed days Monday to Friday, and a second one with the same settings and the name "Morning pages"
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When `f4.ac062.admin@example.com` edits `A4.062 Reading` to the name `A4.062 Reading books`
-> skipped because of previous errors
Then reading the first goal gives the name "A4.062 Reading books, 15 min, 5 days a week" and the habit name `A4.062 Reading books`
-> skipped because of previous errors
And reading the second goal gives the name "Morning pages" and the habit name `A4.062 Reading books`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ARenamedCatalogHabitRenamesTheGoalsBasedOnIt() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 775
[non-AC frame(s) hidden]
```

### F004/AC-018 The target text follows the slice 2 rules, with the "avoided on" exception(n: "15", settings: "Build, yes/no, fixed days Monday, Wednesday and Fr"···, targetText: "\"3 days a week\"", __pickleIndex: "101", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac018.user.15@example.com`, who created the custom habit `Practice`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Practice`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitPractice(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac018.user.15@example.com` creates a goal on their custom habit `Practice` with Build, yes/no, fixed days Monday, Wednesday and Friday
-> skipped because of previous errors
Then reading the goal gives the target text "3 days a week" and the name "Practice, " followed by that target text
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheTargetTextFollowsTheSlice2RulesWithTheAvoidedOnException(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 260
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "07", settings: "Build, count 1000 `pages`, 3 times per week", message: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "14", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.07@example.com` and a Published `A4.007 07 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.07...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.07@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.07@example.com` creates a goal on `A4.007 07 Reading` with Build, count 1000 `pages`, 3 times per week
-> skipped because of previous errors
Then the result is "Enter a whole number from 1 to 999."
-> skipped because of previous errors
And `f4.ac007.user.07@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-040 A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "01", schedule: "fixed days Sunday", date: "2027-03-28", entry: "2027-03-28 from `2027-03-27T22:00:00Z` to `2027-03"···, __pickleIndex: "164", exampleTags: []) — failed

Step trace:
```
Given the registered user `f4.ac040.user.01@example.com` in `Europe/Kyiv`, who created the custom habit `Practice`
-> done: ArrangeSteps.GivenTheRegisteredUser("f4.ac040.user.01@...", "Europe/Kyiv", "created the custo...") (0,1s)
And `f4.ac040.user.01@example.com` created a goal on their custom habit `Practice` with Build, yes/no, fixed days Sunday and the start date 2026-11-18
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenCreatedAGoal(String, String, String, String)' should have 3 parameters
When `f4.ac040.user.01@example.com` reads the evaluation calendar of the goal from 2027-03-28 to 2027-03-28
-> skipped because of previous errors
Then it has exactly one entry: 2027-03-28 from `2027-03-27T22:00:00Z` to `2027-03-28T21:00:00Z` (23 hours)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenCreatedAGoal(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ADayOrWeekWithAClockChangeKeepsLocalMidnightsAndLasts23Or25Hours(String n, String schedule, String date, String entry, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 534
[non-AC frame(s) hidden]
```

### F004/AC-058 Unpublishing with confirmation unpublishes a habit that goals use — failed

Step trace:
```
Given the administrator `f4.ac058.admin@example.com` and a Published `A4.058 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac058.admin@ex...", "a Published `A4.0...") (0,1s)
And the registered users `f4.ac058.a@example.com` and `f4.ac058.b@example.com`, who each created a goal on `A4.058 Reading` with Build, yes/no, fixed days Monday
-> done: ArrangeSteps.GivenTheRegisteredUsers("f4.ac058.a@exampl...", "f4.ac058.b@exampl...", "`A4.058 Reading`", "Build, yes/no, fi...") (0,2s)
When `f4.ac058.admin@example.com` unpublishes `A4.058 Reading` with confirmation
-> done: CatalogSteps.WhenUnpublishes("`f4.ac058.admin@e...", "A4.058 Reading", "with confirmation") (0,0s)
Then `A4.058 Reading` is Unpublished
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.CatalogSteps.ThenTheHabitIsUnpublished(String, String)' should have 1 parameters
And browsing the catalog without an acting user with the search `A4.058` does not contain it
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.CatalogSteps.ThenTheHabitIsUnpublished(String, String)' should have 1 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.UnpublishingWithConfirmationUnpublishesAHabitThatGoalsUse() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 729
[non-AC frame(s) hidden]
```

### F004/AC-070 Registration keeps a named browser time zone, and otherwise gives the default(n: "12", browserTimeZone: "the browser time zone `Europe/Kiev`", timeZone: "`Europe/Kiev`", __pickleIndex: "233", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac070.user.12@example.com` with the password `walking42` and the browser time zone `Europe/Kiev`, and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When `f4.ac070.user.12@example.com` reads their time zone
-> skipped because of previous errors
Then registration succeeded and the time zone is `Europe/Kiev`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.RegistrationKeepsANamedBrowserTimeZoneAndOtherwiseGivesTheDefault(String n, String browserTimeZone, String timeZone, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 846
[non-AC frame(s) hidden]
```

### F004/AC-070 Registration keeps a named browser time zone, and otherwise gives the default(n: "16", browserTimeZone: "the browser time zone `UTC`", timeZone: "`UTC`", __pickleIndex: "237", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac070.user.16@example.com` with the password `walking42` and the browser time zone `UTC`, and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When `f4.ac070.user.16@example.com` reads their time zone
-> skipped because of previous errors
Then registration succeeded and the time zone is `UTC`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.RegistrationKeepsANamedBrowserTimeZoneAndOtherwiseGivesTheDefault(String n, String browserTimeZone, String timeZone, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 846
[non-AC frame(s) hidden]
```

### F004/AC-040 A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "06", schedule: "1 time per week", date: "2027-10-31", entry: "2027-10-25 to 2027-10-31 from `2027-10-24T21:00:00"···, __pickleIndex: "169", exampleTags: []) — failed

Step trace:
```
Given the registered user `f4.ac040.user.06@example.com` in `Europe/Kyiv`, who created the custom habit `Practice`
-> done: ArrangeSteps.GivenTheRegisteredUser("f4.ac040.user.06@...", "Europe/Kyiv", "created the custo...") (0,1s)
And `f4.ac040.user.06@example.com` created a goal on their custom habit `Practice` with Build, yes/no, 1 time per week and the start date 2026-11-18
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenCreatedAGoal(String, String, String, String)' should have 3 parameters
When `f4.ac040.user.06@example.com` reads the evaluation calendar of the goal from 2027-10-31 to 2027-10-31
-> skipped because of previous errors
Then it has exactly one entry: 2027-10-25 to 2027-10-31 from `2027-10-24T21:00:00Z` to `2027-10-31T22:00:00Z` (169 hours)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenCreatedAGoal(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ADayOrWeekWithAClockChangeKeepsLocalMidnightsAndLasts23Or25Hours(String n, String schedule, String date, String entry, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 534
[non-AC frame(s) hidden]
```

### F004/AC-070 Registration keeps a named browser time zone, and otherwise gives the default(n: "14", browserTimeZone: "the browser time zone `US/Eastern`", timeZone: "`US/Eastern`", __pickleIndex: "235", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac070.user.14@example.com` with the password `walking42` and the browser time zone `US/Eastern`, and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When `f4.ac070.user.14@example.com` reads their time zone
-> skipped because of previous errors
Then registration succeeded and the time zone is `US/Eastern`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.RegistrationKeepsANamedBrowserTimeZoneAndOtherwiseGivesTheDefault(String n, String browserTimeZone, String timeZone, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 846
[non-AC frame(s) hidden]
```

### F004/AC-032 An anonymous visitor gets no goal use case(n: "04", action: "reads `f4.ac032.user.04@example.com`'s goal", __pickleIndex: "138", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac032.admin.04@example.com` and a Published `A4.032 04 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac032.admin.04...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac032.user.04@example.com`, who created a goal on `A4.032 04 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When the anonymous visitor reads `f4.ac032.user.04@example.com`'s goal
-> skipped because of previous errors
Then the result is "You are not signed in."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnAnonymousVisitorGetsNoGoalUseCase(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 425
[non-AC frame(s) hidden]
```

### F004/AC-054 A habit's details give the user's goal count and their goals on it, in name order — failed

Step trace:
```
Given the administrator `f4.ac054.admin@example.com` and a Published `A4.054 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac054.admin@ex...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac054.user@example.com`, who created goals on `A4.054 Reading`, in this order: one named "Morning pages", one named "evening pages", one named "Äpfel pages" and one with no name, each with Build, duration 15 minutes, fixed days Monday to Friday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When `f4.ac054.user@example.com` reads the details of `A4.054 Reading` as `f4.ac054.user@example.com`
-> skipped because of previous errors
Then the details give the goal count 4
-> skipped because of previous errors
And the goals "A4.054 Reading, 15 min, 5 days a week", "evening pages", "Morning pages", "Äpfel pages", in this order, each with its identifier
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AHabitsDetailsGiveTheUsersGoalCountAndTheirGoalsOnItInNameOrder() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 681
[non-AC frame(s) hidden]
```

### F004/AC-060 A goal on an Unpublished habit keeps working, and no new goal can use that habit(n: "01", actingUser: "`f4.ac060.owner.01@example.com`", action: "lists their goals", result: "the list contains \"A4.060 01 Reading, 15 min, 5 d"···, __pickleIndex: "208", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac060.admin.01@example.com` and a Published `A4.060 01 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac060.admin.01...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac060.owner.01@example.com`, who created a goal on `A4.060 01 Reading` with Build, duration 15 minutes, fixed days Monday to Friday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And the registered user `f4.ac060.other.01@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.other\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_Other_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And the administrator unpublished `A4.060 01 Reading` with confirmation
-> skipped because of previous errors
When `f4.ac060.owner.01@example.com` lists their goals
-> skipped because of previous errors
Then the list contains "A4.060 01 Reading, 15 min, 5 days a week"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AGoalOnAnUnpublishedHabitKeepsWorkingAndNoNewGoalCanUseThatHabit(String n, String actingUser, String action, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 749
[non-AC frame(s) hidden]
```

### F004/AC-014 A habit that is not a Published catalog habit is refused as a catalog habit(n: "08", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "an identifier that the test creates and that belon"···, __pickleIndex: "79", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac014.admin.08@example.com`, a Draft `A4.014 08 Draft` and an Unpublished `A4.014 08 Gone`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac014.admin.08...", "a Draft `A4.014 0...") (0,1s)
And the registered user `f4.ac014.user.08@example.com`, who created the custom habit `Cold shower`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Cold shower`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitColdShower(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac014.user.08@example.com` creates a goal with Build, yes/no, fixed days Monday on an identifier that the test creates and that belongs to no habit given as a catalog habit
-> skipped because of previous errors
Then the result is "This habit is not in the catalog."
-> skipped because of previous errors
And `f4.ac014.user.08@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AHabitThatIsNotAPublishedCatalogHabitIsRefusedAsACatalogHabit(String n, String action, String habit, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 205
[non-AC frame(s) hidden]
```

### F004/AC-085 An account name has at most 60 characters — failed

Step trace:
```
Given the registered user `f4.ac085.named@example.com`, who changed their name to "Yuri"
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When the anonymous visitor registers the email `f4.ac085.new@example.com` with the password `walking42` and a name of 61 characters, then registers the same email with the password `walking42` and a name of 60 characters
-> skipped because of previous errors
And `f4.ac085.named@example.com` changes their name to a text of 61 characters, then reads their profile, then changes their name to a text of 60 characters and reads their profile again
-> skipped because of previous errors
Then the first registration gives "The name can have at most 60 characters." and the second registration succeeds
-> skipped because of previous errors
And signed in, `f4.ac085.new@example.com`'s profile gives the text of 60 characters as the name
-> skipped because of previous errors
And `f4.ac085.named@example.com`'s first change gives "The name can have at most 60 characters.", the first profile read gives "Yuri", the second change succeeds and the second profile read gives the text of 60 characters
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnAccountNameHasAtMost60Characters() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 1050
[non-AC frame(s) hidden]
```

### F004/AC-016 A goal without a name of its own is named after its habit and target(n: "01", name: "no name", goalName: "\"A4.016 01 Reading, 15 min, 5 days a week\"", own: "not its own", __pickleIndex: "82", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac016.admin.01@example.com` and a Published `A4.016 01 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac016.admin.01...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac016.user.01@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac016.user.01@example.com` creates a goal on `A4.016 01 Reading` with Build, duration 15 minutes, fixed days Monday to Friday and no name
-> skipped because of previous errors
Then reading the goal gives the name "A4.016 01 Reading, 15 min, 5 days a week", and the name is not its own
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AGoalWithoutANameOfItsOwnIsNamedAfterItsHabitAndTarget(String n, String name, String goalName, String own, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 238
[non-AC frame(s) hidden]
```

### F004/AC-014 A habit that is not a Published catalog habit is refused as a catalog habit(n: "06", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "`A4.014 06 Gone`", __pickleIndex: "77", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac014.admin.06@example.com`, a Draft `A4.014 06 Draft` and an Unpublished `A4.014 06 Gone`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac014.admin.06...", "a Draft `A4.014 0...") (0,1s)
And the registered user `f4.ac014.user.06@example.com`, who created the custom habit `Cold shower`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Cold shower`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitColdShower(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac014.user.06@example.com` creates a goal with Build, yes/no, fixed days Monday on `A4.014 06 Gone` given as a catalog habit
-> skipped because of previous errors
Then the result is "This habit is not in the catalog."
-> skipped because of previous errors
And `f4.ac014.user.06@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AHabitThatIsNotAPublishedCatalogHabitIsRefusedAsACatalogHabit(String n, String action, String habit, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 205
[non-AC frame(s) hidden]
```

### F004/AC-087 Without a name the greeting has no name — pending

Step trace:
```
Given the registered user `f4.ac087.user@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`")]
public void GivenTheRegisteredUserF_Ac_UserExample_Com(int p0, int p1)
{
    throw new PendingStepException();
}

When at `2026-11-18T19:00:00Z` `f4.ac087.user@example.com` reads the Home heading
-> skipped because of previous errors
Then the greeting is "Good evening" and the date is "Wednesday, 18 November"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.WithoutANameTheGreetingHasNoName() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 1072
[non-AC frame(s) hidden]
```

### F004/AC-040 A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "02", schedule: "fixed days Sunday", date: "2027-10-31", entry: "2027-10-31 from `2027-10-30T21:00:00Z` to `2027-10"···, __pickleIndex: "165", exampleTags: []) — failed

Step trace:
```
Given the registered user `f4.ac040.user.02@example.com` in `Europe/Kyiv`, who created the custom habit `Practice`
-> done: ArrangeSteps.GivenTheRegisteredUser("f4.ac040.user.02@...", "Europe/Kyiv", "created the custo...") (0,1s)
And `f4.ac040.user.02@example.com` created a goal on their custom habit `Practice` with Build, yes/no, fixed days Sunday and the start date 2026-11-18
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenCreatedAGoal(String, String, String, String)' should have 3 parameters
When `f4.ac040.user.02@example.com` reads the evaluation calendar of the goal from 2027-10-31 to 2027-10-31
-> skipped because of previous errors
Then it has exactly one entry: 2027-10-31 from `2027-10-30T21:00:00Z` to `2027-10-31T22:00:00Z` (25 hours)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenCreatedAGoal(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ADayOrWeekWithAClockChangeKeepsLocalMidnightsAndLasts23Or25Hours(String n, String schedule, String date, String entry, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 534
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "02", settings: "Build, no measurement, 3 times per week", message: "\"Choose how it is measured.\"", __pickleIndex: "9", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.02@example.com` and a Published `A4.007 02 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.02...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.02@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.02@example.com` creates a goal on `A4.007 02 Reading` with Build, no measurement, 3 times per week
-> skipped because of previous errors
Then the result is "Choose how it is measured."
-> skipped because of previous errors
And `f4.ac007.user.02@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-032 An anonymous visitor gets no goal use case(n: "03", action: "lists their goals", __pickleIndex: "137", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac032.admin.03@example.com` and a Published `A4.032 03 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac032.admin.03...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac032.user.03@example.com`, who created a goal on `A4.032 03 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When the anonymous visitor lists their goals
-> skipped because of previous errors
Then the result is "You are not signed in."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnAnonymousVisitorGetsNoGoalUseCase(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 425
[non-AC frame(s) hidden]
```

### F004/AC-009 The edges of every target range are accepted(n: "02", settings: "Build, count 999 `pages`, 3 times per week", targetText: "\"999 pages, 3 times a week\"", __pickleIndex: "45", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac009.admin.02@example.com` and a Published `A4.009 02 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac009.admin.02...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac009.user.02@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac009.user.02@example.com` creates a goal on `A4.009 02 Reading` with Build, count 999 `pages`, 3 times per week
-> skipped because of previous errors
Then creating succeeds
-> skipped because of previous errors
And reading the goal gives the target text "999 pages, 3 times a week"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheEdgesOfEveryTargetRangeAreAccepted(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 125
[non-AC frame(s) hidden]
```

### F004/AC-053 "Only habits I track" returns the Published habits with the user's goals, with the other filters(n: "03", filter: "the category Body", result: "no habit", __pickleIndex: "193", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac053.admin.03@example.com`, a Published `A4.053 03 Reading` with the category Mind, a Published `A4.053 03 Yoga` with the category Body, a Published `A4.053 03 Sugar` with the category Health and a Published `A4.053 03 Coffee` with the category Health
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac053.admin.03...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac053.user.03@example.com`, who created two goals on `A4.053 03 Reading`, one goal on `A4.053 03 Sugar` and one goal on `A4.053 03 Coffee`, each with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And the administrator unpublished `A4.053 03 Coffee` with confirmation
-> skipped because of previous errors
When `f4.ac053.user.03@example.com` browses the catalog as `f4.ac053.user.03@example.com` with "Only habits I track", the search `A4.053 03` and the category Body
-> skipped because of previous errors
Then the result is no habit
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.OnlyHabitsITrackReturnsThePublishedHabitsWithTheUsersGoalsWithTheOtherFilters(String n, String filter, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 667
[non-AC frame(s) hidden]
```

### F004/AC-084 An account name is trimmed, and empty means no name(n: "03", name: "the name `\"   \"`", result: "no name", __pickleIndex: "275", exampleTags: []) — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac084.user.03@example.com` with the password `walking42` and the name `"   "`, and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
When `f4.ac084.user.03@example.com` reads their profile
-> skipped because of previous errors
Then registration succeeded and the profile gives no name and the email `f4.ac084.user.03@example.com`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnAccountNameIsTrimmedAndEmptyMeansNoName(String n, String name, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 1034
[non-AC frame(s) hidden]
```

### F004/AC-053 "Only habits I track" returns the Published habits with the user's goals, with the other filters(n: "02", filter: "the category Health", result: "`A4.053 02 Sugar` only", __pickleIndex: "192", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac053.admin.02@example.com`, a Published `A4.053 02 Reading` with the category Mind, a Published `A4.053 02 Yoga` with the category Body, a Published `A4.053 02 Sugar` with the category Health and a Published `A4.053 02 Coffee` with the category Health
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac053.admin.02...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac053.user.02@example.com`, who created two goals on `A4.053 02 Reading`, one goal on `A4.053 02 Sugar` and one goal on `A4.053 02 Coffee`, each with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And the administrator unpublished `A4.053 02 Coffee` with confirmation
-> skipped because of previous errors
When `f4.ac053.user.02@example.com` browses the catalog as `f4.ac053.user.02@example.com` with "Only habits I track", the search `A4.053 02` and the category Health
-> skipped because of previous errors
Then the result is `A4.053 02 Sugar` only
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.OnlyHabitsITrackReturnsThePublishedHabitsWithTheUsersGoalsWithTheOtherFilters(String n, String filter, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 667
[non-AC frame(s) hidden]
```

### F004/AC-080 A time zone is given only to its own account — failed

Step trace:
```
Given the registered user `f4.ac080.a@example.com` in `Europe/Kyiv`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And the registered user `f4.ac080.b@example.com` and the administrator `f4.ac080.admin@example.com`
-> skipped because of previous errors
When `f4.ac080.b@example.com` and `f4.ac080.admin@example.com` each read their time zone
-> skipped because of previous errors
Then each gets `UTC`, their own, and neither result contains `Europe/Kyiv`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ATimeZoneIsGivenOnlyToItsOwnAccount() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 993
[non-AC frame(s) hidden]
```

### F004/AC-027 A user with no goals gets an empty list — pending

Step trace:
```
Given the registered user `f4.ac027.user@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`")]
public void GivenTheRegisteredUserF_Ac_UserExample_Com(int p0, int p1)
{
    throw new PendingStepException();
}

When `f4.ac027.user@example.com` lists their goals
-> skipped because of previous errors
Then the list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AUserWithNoGoalsGetsAnEmptyList() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 371
[non-AC frame(s) hidden]
```

### F004/AC-014 A habit that is not a Published catalog habit is refused as a catalog habit(n: "01", action: "starts a new goal from", habit: "`A4.014 01 Draft`", __pickleIndex: "72", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac014.admin.01@example.com`, a Draft `A4.014 01 Draft` and an Unpublished `A4.014 01 Gone`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac014.admin.01...", "a Draft `A4.014 0...") (0,1s)
And the registered user `f4.ac014.user.01@example.com`, who created the custom habit `Cold shower`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Cold shower`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitColdShower(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac014.user.01@example.com` starts a new goal from `A4.014 01 Draft` given as a catalog habit
-> skipped because of previous errors
Then the result is "This habit is not in the catalog."
-> skipped because of previous errors
And `f4.ac014.user.01@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AHabitThatIsNotAPublishedCatalogHabitIsRefusedAsACatalogHabit(String n, String action, String habit, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 205
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "04", settings: "Build, yes/no with the count unit `pages`, fixed d"···, message: "\"A yes/no goal has no amount or unit.\"", __pickleIndex: "11", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.04@example.com` and a Published `A4.007 04 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.04...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.04@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.04@example.com` creates a goal on `A4.007 04 Reading` with Build, yes/no with the count unit `pages`, fixed days Monday
-> skipped because of previous errors
Then the result is "A yes/no goal has no amount or unit."
-> skipped because of previous errors
And `f4.ac007.user.04@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-012 A Reduce goal with an amount has 1 time per period(n: "05", settings: "Build, count 20 `pages`, 3 times per day", result: "creating succeeds", __pickleIndex: "64", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac012.admin.05@example.com` and a Published `A4.012 05 Coffee`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac012.admin.05...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac012.user.05@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac012.user.05@example.com` creates a goal on `A4.012 05 Coffee` with Build, count 20 `pages`, 3 times per day
-> skipped because of previous errors
Then the result is creating succeeds
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AReduceGoalWithAnAmountHas1TimePerPeriod(String n, String settings, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 171
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "21", settings: "Build, yes/no, 3 times with no period", message: "\"Choose per day or per week.\"", __pickleIndex: "28", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.21@example.com` and a Published `A4.007 21 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.21...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.21@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.21@example.com` creates a goal on `A4.007 21 Reading` with Build, yes/no, 3 times with no period
-> skipped because of previous errors
Then the result is "Choose per day or per week."
-> skipped because of previous errors
And `f4.ac007.user.21@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-039 A per-week goal tells when its first week starts(n: "01", startDate: "2026-11-18", time: "`2026-11-18T10:00:00Z`", list: "first week starts 2026-11-23, no start date and no"···, __pickleIndex: "160", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac039.user.01@example.com`, who created the custom habit `Run`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Run`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitRun(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And `f4.ac039.user.01@example.com` created a goal on their custom habit `Run` with Build, yes/no, 3 times per week and the start date 2026-11-18
-> skipped because of previous errors
When at `2026-11-18T10:00:00Z` `f4.ac039.user.01@example.com` lists their goals and reads the goal
-> skipped because of previous errors
Then the list gives the goal first week starts 2026-11-23, no start date and no next commitment
-> skipped because of previous errors
And reading the goal gives first week starts 2026-11-23
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.APer_WeekGoalTellsWhenItsFirstWeekStarts(String n, String startDate, String time, String list, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 520
[non-AC frame(s) hidden]
```

### F004/AC-090 An account name is given only to its own account; administrators have their own — failed

Step trace:
```
Given the anonymous visitor registered the email `f4.ac090.a@example.com` with the password `walking42` and the name "Yuri", and signed in
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
And the registered user `f4.ac090.b@example.com` and the administrator `f4.ac090.admin@example.com`
-> skipped because of previous errors
When `f4.ac090.b@example.com` reads their profile, and `f4.ac090.admin@example.com` changes their name to "Ada Admin", reads their profile and reads the Home heading
-> skipped because of previous errors
Then `f4.ac090.b@example.com`'s profile gives no name, the email `f4.ac090.b@example.com`, and does not contain "Yuri"
-> skipped because of previous errors
And the administrator's change succeeds, their profile gives the name "Ada Admin" and the email `f4.ac090.admin@example.com`, and the Home heading gives the greeting "Good morning, Ada Admin" (10:00 in `UTC`)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheAnonymousVisitorRegistered(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnAccountNameIsGivenOnlyToItsOwnAccountAdministratorsHaveTheirOwn() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 1105
[non-AC frame(s) hidden]
```

### F004/AC-038 A per-week goal has one evaluation period per Monday-to-Sunday week, from its first full week(n: "04", startDate: "2026-11-23", from: "2026-11-23", to: "2026-11-29", periods: "1 period: 2026-11-23 to 2026-11-29 from `2026-11-2"···, __pickleIndex: "159", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac038.user.04@example.com`, who created the custom habit `Run`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Run`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitRun(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And `f4.ac038.user.04@example.com` created a goal on their custom habit `Run` with Build, yes/no, 3 times per week and the start date 2026-11-23
-> skipped because of previous errors
When `f4.ac038.user.04@example.com` reads the evaluation calendar of the goal from 2026-11-23 to 2026-11-29
-> skipped because of previous errors
Then it has 1 period: 2026-11-23 to 2026-11-29 from `2026-11-23T00:00:00Z` to `2026-11-30T00:00:00Z`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.APer_WeekGoalHasOneEvaluationPeriodPerMonday_To_SundayWeekFromItsFirstFullWeek(String n, String startDate, String from, String to, String periods, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 505
[non-AC frame(s) hidden]
```

### F004/AC-023 The next commitment is the first commitment that has not ended(n: "01", time: "`2026-11-16T08:00:00Z`", next: "2026-11-16", __pickleIndex: "114", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac023.user.01@example.com`, who created the custom habit `Yoga`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Yoga`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitYoga(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at `2026-11-16T08:00:00Z` `f4.ac023.user.01@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days Monday, Wednesday and Friday and the start date 2026-11-16
-> skipped because of previous errors
When at `2026-11-16T08:00:00Z` `f4.ac023.user.01@example.com` lists their goals
-> skipped because of previous errors
Then the goal gives next commitment 2026-11-16 and no start date
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheNextCommitmentIsTheFirstCommitmentThatHasNotEnded(String n, String time, String next, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 329
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "10", settings: "Reduce, count 1000 `cups`, 1 time per day", message: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "17", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.10@example.com` and a Published `A4.007 10 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.10...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.10@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.10@example.com` creates a goal on `A4.007 10 Reading` with Reduce, count 1000 `cups`, 1 time per day
-> skipped because of previous errors
Then the result is "Enter a whole number from 0 to 999."
-> skipped because of previous errors
And `f4.ac007.user.10@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "12", settings: "Build, count 20 with the unit `\"   \"`, 3 times p"···, message: "\"Enter a unit, for example pages.\"", __pickleIndex: "19", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.12@example.com` and a Published `A4.007 12 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.12...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.12@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.12@example.com` creates a goal on `A4.007 12 Reading` with Build, count 20 with the unit `"   "`, 3 times per week
-> skipped because of previous errors
Then the result is "Enter a unit, for example pages."
-> skipped because of previous errors
And `f4.ac007.user.12@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-008 Goal creation gives the first refusal in FR-006 order(n: "10", goal: "on `A4.008 10 Reading` with Build, yes/no, 21 time"···, message: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "39", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac008.admin.10@example.com`, a Published `A4.008 10 Reading` and a Draft `A4.008 10 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac008.admin.10...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac008.user.10@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac008.user.10@example.com` creates a goal on `A4.008 10 Reading` with Build, yes/no, 21 times with no period
-> skipped because of previous errors
Then the result is "Enter a number of times from 1 to 20."
-> skipped because of previous errors
And `f4.ac008.user.10@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.GoalCreationGivesTheFirstRefusalInFR_006Order(String n, String goal, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 100
[non-AC frame(s) hidden]
```

### F004/AC-033 A custom habit is looked up only among the acting user's custom habits(n: "02", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "`f4.ac033.b.02@example.com`'s custom habit `Cold s"···, __pickleIndex: "142", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac033.admin.02@example.com` and a Published `A4.033 02 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac033.admin.02...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac033.b.02@example.com`, who created the custom habit `Cold shower`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.b\.{int}@example\.com`, who created the custom habit `Cold shower`")]
public void GivenTheRegisteredUserF_Ac_B_Example_ComWhoCreatedTheCustomHabitColdShower(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And the registered user `f4.ac033.a.02@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.a\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_A_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac033.a.02@example.com` creates a goal with Build, yes/no, fixed days Monday on `f4.ac033.b.02@example.com`'s custom habit `Cold shower` given as a custom habit
-> skipped because of previous errors
Then the result is "This custom habit does not exist."
-> skipped because of previous errors
And `f4.ac033.a.02@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ACustomHabitIsLookedUpOnlyAmongTheActingUsersCustomHabits(String n, String action, String habit, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 443
[non-AC frame(s) hidden]
```

### F004/AC-065 Demo loading run again gives existing demo accounts no goals — failed

Step trace:
```
Given the AC database is reset, so no account and no habit exists
-> done: ArrangeSteps.GivenTheAcDatabaseIsReset() (0,2s)
And demo loading ran
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenDemoLoadingRan(String)' should have 0 parameters
When at `2026-11-19T10:00:00Z` demo loading runs again
-> skipped because of previous errors
Then its account results are "already exists" for `demo1@example.com`, `demo2@example.com` and `demo3@example.com`, and it gives no goal result
-> skipped because of previous errors
And each demo account's goal list is the same as after the first run: 2 goals for `demo1@example.com`, 2 for `demo2@example.com` and 3 for `demo3@example.com`, with the same names
-> skipped because of previous errors
And reading each demo goal gives the same start date as after the first run: 2026-11-18, except "Meditation, 10 min, 3 days a week" with 2026-11-25
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenDemoLoadingRan(String)' should have 0 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.DemoLoadingRunAgainGivesExistingDemoAccountsNoGoals() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 803
[non-AC frame(s) hidden]
```

### F004/AC-017 Several goals on one habit are independent — failed

Step trace:
```
Given the administrator `f4.ac017.admin@example.com` and a Published `A4.017 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac017.admin@ex...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac017.user@example.com`, who created a goal on `A4.017 Reading` with Build, duration 15 minutes, fixed days Monday to Friday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When `f4.ac017.user@example.com` creates a goal on `A4.017 Reading` with Build, count 20 `pages`, 3 times per week, and then a goal on `A4.017 Reading` with Build, duration 15 minutes, fixed days Monday to Friday
-> skipped because of previous errors
Then both creations succeed with identifiers different from each other and from the first goal
-> skipped because of previous errors
And the goal list has 3 goals on `A4.017 Reading`: two named "A4.017 Reading, 15 min, 5 days a week" and one named "A4.017 Reading, 20 pages, 3 times a week"
-> skipped because of previous errors
And reading the first goal still gives Build, duration 15 minutes, fixed days Monday to Friday
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.SeveralGoalsOnOneHabitAreIndependent() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 254
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "14", settings: "Build, count 20 with a unit of 21 characters, 3 ti"···, message: "\"The unit can have at most 20 characters.\"", __pickleIndex: "21", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.14@example.com` and a Published `A4.007 14 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.14...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.14@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.14@example.com` creates a goal on `A4.007 14 Reading` with Build, count 20 with a unit of 21 characters, 3 times per week
-> skipped because of previous errors
Then the result is "The unit can have at most 20 characters."
-> skipped because of previous errors
And `f4.ac007.user.14@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-010 The start date is today or up to 365 days later(n: "01", startDate: "the start date 2026-11-18", result: "creating succeeds", __pickleIndex: "52", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac010.admin.01@example.com` and a Published `A4.010 01 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac010.admin.01...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac010.user.01@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac010.user.01@example.com` creates a goal on `A4.010 01 Reading` with Build, yes/no, fixed days Monday and the start date 2026-11-18
-> skipped because of previous errors
Then the result is creating succeeds
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheStartDateIsTodayOrUpTo365DaysLater(String n, String startDate, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 143
[non-AC frame(s) hidden]
```

### F004/AC-051 Reading a custom habit gives the number of goals that use it — pending

Step trace:
```
Given the registered user `f4.ac051.user@example.com`, who created the custom habits `Cold shower` and `Plank`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabits().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the custom habits `Cold shower` and `Plank`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheCustomHabitsColdShowerAndPlank(int p0, int p1)
{
    throw new PendingStepException();
}

And `f4.ac051.user@example.com` created two goals on their custom habit `Cold shower` with Build, yes/no, fixed days Monday
-> skipped because of previous errors
When `f4.ac051.user@example.com` reads their custom habits `Cold shower` and `Plank`
-> skipped because of previous errors
Then `Cold shower` gives the number of goals 2 and `Plank` gives 0
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.ReadingACustomHabitGivesTheNumberOfGoalsThatUseIt() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 646
[non-AC frame(s) hidden]
```

### F004/AC-014 A habit that is not a Published catalog habit is refused as a catalog habit(n: "04", action: "starts a new goal from", habit: "an identifier that the test creates and that belon"···, __pickleIndex: "75", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac014.admin.04@example.com`, a Draft `A4.014 04 Draft` and an Unpublished `A4.014 04 Gone`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac014.admin.04...", "a Draft `A4.014 0...") (0,1s)
And the registered user `f4.ac014.user.04@example.com`, who created the custom habit `Cold shower`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Cold shower`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitColdShower(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac014.user.04@example.com` starts a new goal from an identifier that the test creates and that belongs to no habit given as a catalog habit
-> skipped because of previous errors
Then the result is "This habit is not in the catalog."
-> skipped because of previous errors
And `f4.ac014.user.04@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AHabitThatIsNotAPublishedCatalogHabitIsRefusedAsACatalogHabit(String n, String action, String habit, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 205
[non-AC frame(s) hidden]
```

### F004/AC-008 Goal creation gives the first refusal in FR-006 order(n: "11", goal: "on `A4.008 11 Reading` with Reduce, count 2 `cups`"···, message: "\"A Reduce goal with an amount has 1 time per peri"···, __pickleIndex: "40", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac008.admin.11@example.com`, a Published `A4.008 11 Reading` and a Draft `A4.008 11 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac008.admin.11...", "a Published `A4.0...") (0,7s)
And the registered user `f4.ac008.user.11@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac008.user.11@example.com` creates a goal on `A4.008 11 Reading` with Reduce, count 2 `cups`, 2 times per day, and no start date
-> skipped because of previous errors
Then the result is "A Reduce goal with an amount has 1 time per period."
-> skipped because of previous errors
And `f4.ac008.user.11@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.GoalCreationGivesTheFirstRefusalInFR_006Order(String n, String goal, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 100
[non-AC frame(s) hidden]
```

### F004/AC-007 Each field refusal of goal creation, with its message(n: "09", settings: "Reduce, count -1 `cups`, 1 time per day", message: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "16", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac007.admin.09@example.com` and a Published `A4.007 09 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac007.admin.09...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac007.user.09@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac007.user.09@example.com` creates a goal on `A4.007 09 Reading` with Reduce, count -1 `cups`, 1 time per day
-> skipped because of previous errors
Then the result is "Enter a whole number from 0 to 999."
-> skipped because of previous errors
And `f4.ac007.user.09@example.com`'s goal list is empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.EachFieldRefusalOfGoalCreationWithItsMessage(String n, String settings, String message, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 67
[non-AC frame(s) hidden]
```

### F004/AC-039 A per-week goal tells when its first week starts(n: "02", startDate: "2026-11-18", time: "`2026-11-22T23:59:59Z`", list: "first week starts 2026-11-23, no start date and no"···, __pickleIndex: "161", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac039.user.02@example.com`, who created the custom habit `Run`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Run`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitRun(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And `f4.ac039.user.02@example.com` created a goal on their custom habit `Run` with Build, yes/no, 3 times per week and the start date 2026-11-18
-> skipped because of previous errors
When at `2026-11-22T23:59:59Z` `f4.ac039.user.02@example.com` lists their goals and reads the goal
-> skipped because of previous errors
Then the list gives the goal first week starts 2026-11-23, no start date and no next commitment
-> skipped because of previous errors
And reading the goal gives first week starts 2026-11-23
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.APer_WeekGoalTellsWhenItsFirstWeekStarts(String n, String startDate, String time, String list, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 520
[non-AC frame(s) hidden]
```

### F004/AC-055 Anonymous visitors and administrators get the catalog without goal information(n: "03", actingUser: "the anonymous visitor", action: "reads the details of `A4.055 03 Reading` as the an"···, result: "the details have no goal count and no goals", __pickleIndex: "197", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac055.admin.03@example.com` and a Published `A4.055 03 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac055.admin.03...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac055.user.03@example.com`, who created a goal on `A4.055 03 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
When the anonymous visitor reads the details of `A4.055 03 Reading` as the anonymous visitor
-> skipped because of previous errors
Then the details have no goal count and no goals
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnonymousVisitorsAndAdministratorsGetTheCatalogWithoutGoalInformation(String n, String actingUser, String action, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 688
[non-AC frame(s) hidden]
```

### F004/AC-029 Another user's goal does not exist for them(n: "02", action: "reads the evaluation calendar from 2026-11-18 to 2"···, goal: "`f4.ac029.a.02@example.com`'s goal", __pickleIndex: "125", exampleTags: []) — failed

Step trace:
```
Given the administrator `f4.ac029.admin.02@example.com` and a Published `A4.029 02 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac029.admin.02...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac029.a.02@example.com`, who created a goal on `A4.029 02 Reading` with Build, yes/no, fixed days Monday
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
And the registered user `f4.ac029.b.02@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.b\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_B_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac029.b.02@example.com` reads the evaluation calendar from 2026-11-18 to 2026-11-24 of `f4.ac029.a.02@example.com`'s goal
-> skipped because of previous errors
Then the result is "This goal does not exist."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F004.ArrangeSteps.GivenTheRegisteredUser(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.AnotherUsersGoalDoesNotExistForThem(String n, String action, String goal, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 387
[non-AC frame(s) hidden]
```

### F004/AC-039 A per-week goal tells when its first week starts(n: "04", startDate: "2026-11-23", time: "`2026-11-18T10:00:00Z`", list: "first week starts 2026-11-23, start date 2026-11-2"···, __pickleIndex: "163", exampleTags: []) — pending

Step trace:
```
Given the registered user `f4.ac039.user.04@example.com`, who created the custom habit `Run`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`, who created the custom habit `Run`")]
public void GivenTheRegisteredUserF_Ac_User_Example_ComWhoCreatedTheCustomHabitRun(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And `f4.ac039.user.04@example.com` created a goal on their custom habit `Run` with Build, yes/no, 3 times per week and the start date 2026-11-23
-> skipped because of previous errors
When at `2026-11-18T10:00:00Z` `f4.ac039.user.04@example.com` lists their goals and reads the goal
-> skipped because of previous errors
Then the list gives the goal first week starts 2026-11-23, start date 2026-11-23 and no next commitment
-> skipped because of previous errors
And reading the goal gives first week starts 2026-11-23
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.APer_WeekGoalTellsWhenItsFirstWeekStarts(String n, String startDate, String time, String list, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 520
[non-AC frame(s) hidden]
```

### F004/AC-009 The edges of every target range are accepted(n: "06", settings: "Build, yes/no, 1 time per week", targetText: "\"once a week\"", __pickleIndex: "49", exampleTags: []) — pending

Step trace:
```
Given the administrator `f4.ac009.admin.06@example.com` and a Published `A4.009 06 Reading`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("f4.ac009.admin.06...", "a Published `A4.0...") (0,1s)
And the registered user `f4.ac009.user.06@example.com`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user\.{int}@example\.com`")]
public void GivenTheRegisteredUserF_Ac_User_Example_Com(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When `f4.ac009.user.06@example.com` creates a goal on `A4.009 06 Reading` with Build, yes/no, 1 time per week
-> skipped because of previous errors
Then creating succeeds
-> skipped because of previous errors
And reading the goal gives the target text "once a week"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F004.GoalsFeature.TheEdgesOfEveryTargetRangeAreAccepted(String n, String settings, String targetText, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-4-934803\tests\HT.AcceptanceTests\Features\F004\goals.feature:line 125
[non-AC frame(s) hidden]
```

## Regression (other features)

Total: 353 · passed: 353 · failed: 0 · pending: 0 · skipped: 0

| Feature | Scenario | Tags | Outcome | Duration |
|---|---|---|---|---|
| F002 | Only the 24 habit icons are accepted(n: "15", icon: "`music`", result: "success", __pickleIndex: "178", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0753444 |
| F001 | An administrator gets only their own account details | @F001 @AC-030 @FR-010 @FR-012 @FR-015 @US3-AS5 | passed | 00:00:00.1176864 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "06", count: "0", marks: "none marked", result: "success", after: "no suggested starting goal", __pickleIndex: "114", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0769232 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "08", second: "Build, duration with no amount and the unit minute"···, result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "123", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0830086 |
| F002 | A Draft and an Unpublished habit can be edited and keep their state | @F002 @AC-069 @FR-012 | passed | 00:00:00.1269855 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "02", state: "Published", action: "creates a catalog habit", __pickleIndex: "65", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | passed | 00:00:00.0801074 |
| F001 | A registered user reads their own account details | @F001 @AC-027 @FR-009 @FR-012 @US3-AS2 | passed | 00:00:00.0794957 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@.example\"", whyItIsNotValid: "domain has one non-empty part, after an empty one", __pickleIndex: "11", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0004748 |
| F002 | Only the 24 habit icons are accepted(n: "14", icon: "`languages`", result: "success", __pickleIndex: "177", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0763249 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "13", second: "Reduce, count with no amount and the unit `sweets`"···, result: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "128", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0774553 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "11", fields: "Build, yes/no, 2 times per day", text: "\"2 times a day\"", kind: "\"Yes/no · flexible\"", __pickleIndex: "38", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0789362 |
| F001 | A failed attempt during the block does not count after the block | @F001 @AC-022 @FR-008 | passed | 00:00:00.3913081 |
| F002 | The role and the identifier are checked before the fields(n: "03", actingUser: "the administrator `ac039.admin.03@example.com`", action: "edits an unknown identifier with the name `\"   \""···, result: "\"This catalog habit does not exist.\"", __pickleIndex: "151", exampleTags: []) | @F002 @AC-039 @FR-016 @US3-AS18 | passed | 00:00:00.1682271 |
| F002 | Another registered user cannot reach a custom habit(n: "02", action: "edits, with the name `Mine now`, the category Body"···, __pickleIndex: "205", exampleTags: []) | @F002 @AC-053 @FR-021 @FR-022 @US4-AS7 | passed | 00:00:00.1782003 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "18", second: "Build, duration 15 minutes, no schedule", result: "\"Choose fixed days or how many times.\"", __pickleIndex: "133", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0775861 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "06", action: "edits a custom habit with the identifier of `A058 "···, __pickleIndex: "236", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.1573746 |
| F001 | Registration refuses an email that is not valid(email: "\"ana.example.com\"", whyItIsNotValid: "no \"@\"", __pickleIndex: "7", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0003359 |
| F002 | A registered user cannot manage the catalog(n: "03", action: "creates the catalog habit `A030 03 New`", __pickleIndex: "90", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1511313 |
| F002 | A custom habit is not a catalog habit for the administrator(n: "02", action: "edits, with the name `A033 02 Taken over`, the cat"···, __pickleIndex: "105", exampleTags: []) | @F002 @AC-033 @FR-013 @FR-024 @US3-AS13 | passed | 00:00:00.1546887 |
| F002 | A registered user cannot manage the catalog(n: "04", action: "edits `A030 04 Draft` to the name `A030 04 Renamed"···, __pickleIndex: "91", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1530036 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "12", second: "Reduce, count -1 `sweets`, 1 time per day", result: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "127", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0812975 |
| F001 | Administrator creation refuses an email that is not valid | @F001 @AC-045 @FR-003 @FR-014 @US4-AS6 | passed | 00:00:00.0009372 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "07", second: "Build, count 1000 `pages`, 1 time per day", result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "122", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0763616 |
| F001 | Registration accepts the shortest valid email and a domain of three parts(email: "\"a@b.c\"", __pickleIndex: "21", exampleTags: []) | @F001 @AC-005 @FR-001 @FR-003 | passed | 00:00:00.0703818 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "10", fields: "Build, count 2 `glasses`, 3 times per day", text: "\"2 glasses, 3 times a day\"", kind: "\"Count · flexible\"", __pickleIndex: "37", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0772029 |
| F002 | Suggested starting goals keep their order and the card shows the marked one | @F002 @AC-034 @FR-006 @US3-AS14 | passed | 00:00:00.0970487 |
| F001 | Demo loading without a demo password creates no account(demoPassword: "\"\"", __pickleIndex: "85", exampleTags: []) | @F001 @AC-055 @FR-016 @US7-AS3 | passed | 00:00:00.3055329 |
| F001 | The anonymous visitor cannot change a password, whatever the new password(newPassword: "\"running42\"", whyItMatters: "meets the password rule", __pickleIndex: "55", exampleTags: []) | @F001 @AC-036 @FR-011 @FR-013 @US6-AS5 | passed | 00:00:00.0005387 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "03", search: "`\"ning pa\"`", morning: "contains", evening: "does not contain", __pickleIndex: "10", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.0768104 |
| F001 | Administrator creation checks the email before the password rule | @F001 @AC-049 @FR-003 @FR-014 | passed | 00:00:00.0008277 |
| F002 | Only the 24 habit icons are accepted(n: "02", icon: "`person-standing`", result: "success", __pickleIndex: "165", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0747564 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "06", state: "Unpublished", action: "edits `A026 06 Other`", __pickleIndex: "69", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | passed | 00:00:00.0816609 |
| F002 | A custom habit is not a catalog habit for the administrator(n: "03", action: "publishes", __pickleIndex: "106", exampleTags: []) | @F002 @AC-033 @FR-013 @FR-024 @US3-AS13 | passed | 00:00:00.1490995 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "03", second: "Build, yes/no with the amount 5, 1 time per day", result: "\"A yes/no suggested starting goal has no amount o"···, __pickleIndex: "118", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0786337 |
| F001 | Demo loading without a demo password creates no account(demoPassword: "absent", __pickleIndex: "84", exampleTags: []) | @F001 @AC-055 @FR-016 @US7-AS3 | passed | 00:00:00.3061502 |
| F002 | A custom habit is not a catalog habit for the administrator(n: "01", action: "reads", __pickleIndex: "104", exampleTags: []) | @F002 @AC-033 @FR-013 @FR-024 @US3-AS13 | passed | 00:00:00.1584545 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "04", second: "Build, yes/no with the unit `pages`, 1 time per da"···, result: "\"A yes/no suggested starting goal has no amount o"···, __pickleIndex: "119", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0764548 |
| F002 | The anonymous visitor has no access to custom habits(n: "01", action: "lists custom habits", __pickleIndex: "214", exampleTags: []) | @F002 @AC-055 @FR-023 @US4-AS9 | passed | 00:00:00.0785151 |
| F002 | Only the 24 habit icons are accepted(n: "26", icon: "`trash-2`", result: "\"Choose an icon.\"", __pickleIndex: "189", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0750241 |
| F002 | Only the 24 habit icons are accepted(n: "27", icon: "`search`", result: "\"Choose an icon.\"", __pickleIndex: "190", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0745198 |
| F002 | Field checks come in a fixed order(n: "04", fields: "the name `A029 04 Cat`, no category, no intent", result: "\"Choose a category.\"", __pickleIndex: "82", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0768851 |
| F001 | Administrator creation leaves an existing administrator unchanged | @F001 @AC-041 @FR-014 @US4-AS2 | passed | 00:00:00.1142801 |
| F002 | The category filter counts only Published catalog habits(n: "02", actingUser: "the registered user `ac009.user.02@example.com`", __pickleIndex: "21", exampleTags: []) | @F002 @AC-009 @FR-002 @FR-009 @FR-024 @US1-AS9 | passed | 00:00:00.3892197 |
| F001 | Sign-in with an email that has no account fails with the single message | @F001 @AC-012 @FR-007 @US2-AS2 | passed | 00:00:00.0029188 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "04", action: "reads a custom habit with the identifier of `A058 "···, __pickleIndex: "234", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.2087850 |
| F002 | For custom habits the acting user and ownership come first, the name check last(n: "03", actingUser: "the registered user `ac057.b.03@example.com`", action: "edits an unknown identifier with the name `\"   \""···, result: "\"This custom habit does not exist.\"", __pickleIndex: "228", exampleTags: []) | @F002 @AC-057 @FR-005 @FR-016 @US4-AS11 | passed | 00:00:00.1508140 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "15", fields: "Build, yes/no, fixed days Monday, Tuesday, Wednesd"···, text: "\"6 days a week\"", kind: "\"Yes/no · fixed days Mon–Wed, Fri–Sun\"", __pickleIndex: "42", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0755045 |
| F001 | Sign-in matches the email whatever its case | @F001 @AC-010 @FR-005 @FR-006 @US2-AS1 | passed | 00:00:00.0837550 |
| F002 | Details give every suggested starting goal in order, with its text and kind | @F002 @AC-013 @FR-001 @FR-010 @FR-017 @US2-AS1 | passed | 00:00:00.1236152 |
| F002 | The category filter starts with All and follows the fixed order | @F002 @AC-008 @FR-002 @FR-009 @US1-AS8 | passed | 00:00:00.0882710 |
| F002 | A new catalog habit is a Draft that the public catalog does not show | @F002 @AC-018 @FR-011 @FR-012 @US3-AS1 | passed | 00:00:00.0931940 |
| F002 | The anonymous visitor cannot manage the catalog(n: "05", action: "publishes `A031 05 Published`", __pickleIndex: "98", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0761111 |
| F002 | The anonymous visitor has no access to custom habits(n: "04", action: "edits `Secret` by its identifier, with the name `S"···, __pickleIndex: "217", exampleTags: []) | @F002 @AC-055 @FR-023 @US4-AS9 | passed | 00:00:00.0759923 |
| F002 | Field checks refuse a custom habit with the same messages(n: "02", name: "a name of 61 characters with the prefix `Habit`", description: "none", category: "Body", intent: "Build", icon: "`droplet`", result: "\"The name can have at most 60 characters.\"", __pickleIndex: "220", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0746169 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "05", second: "Build, count with no amount and the unit `pages`, "···, result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "120", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0785894 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "06", actingUser: "the registered user `ac016.user.06@example.com`", habit: "an unknown identifier", __pickleIndex: "49", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1495634 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "03", fields: "Reduce, count 1 `sweet`, 1 time per day", text: "\"at most 1 sweet a day\"", kind: "\"Count · flexible\"", __pickleIndex: "30", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0804779 |
| F002 | Details with a custom habit's identifier give "not in the catalog"(n: "03", actingUser: "the anonymous visitor", __pickleIndex: "55", exampleTags: []) | @F002 @AC-017 @FR-010 @FR-024 @US2-AS5 | passed | 00:00:00.1503164 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "17", second: "Build, duration 15 with no unit, 1 time per day", result: "\"Choose minutes or hours.\"", __pickleIndex: "132", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0772970 |
| F002 | Publishing a Draft makes it public | @F002 @AC-019 @FR-011 @US3-AS2 | passed | 00:00:00.0980936 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "01", fields: "Build, count 1 `page`, 1 time per day", storedUnit: "`page`", __pickleIndex: "138", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0793920 |
| F001 | Administrator creation refuses a password that breaks the password rule(email: "\"short.admin@example.com\"", password: "\"curat42\"", whyItBreaksTheRule: "7 characters, with a digit", __pickleIndex: "73", exampleTags: []) | @F001 @AC-046 @FR-002 @FR-014 @US4-AS7 | passed | 00:00:00.0785262 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "02", name: "`\"\"`", description: "none", category: "Body", intent: "Build", icon: "`book-open`", result: "\"Enter a name.\"", __pickleIndex: "71", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0813707 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "07", name: "`A027 07 No icon`", description: "none", category: "Body", intent: "Build", icon: "none", result: "\"Choose an icon.\"", __pickleIndex: "76", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0758800 |
| F001 | A registered user cannot change another account's password | @F001 @AC-037 @FR-010 @FR-013 @US6-AS6 | passed | 00:00:00.2157212 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "06", fields: "Build, duration 999 hours, 1 time per week", storedUnit: "hours", __pickleIndex: "143", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0783853 |
| F001 | Administrator creation creates an administrator | @F001 @AC-039 @FR-014 @US4-AS1 | passed | 00:00:00.0752372 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "07", fields: "Reduce, yes/no, 1 time per day", text: "\"at most once a day\"", kind: "\"Yes/no · flexible\"", __pickleIndex: "34", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0775353 |
| F001 | Registration refuses an email that is not valid(email: "\"@example.com\"", whyItIsNotValid: "nothing before the \"@\"", __pickleIndex: "18", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002514 |
| F002 | Each of the six categories can be given to a catalog habit(n: "01", category: "Body", __pickleIndex: "158", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0790750 |
| F001 | Sign-in ignores spaces around the email | @F001 @AC-011 @FR-006 | passed | 00:00:00.0777919 |
| F002 | Field checks come in a fixed order(n: "01", fields: "the name `\"   \"`, no category and no icon", result: "\"Enter a name.\"", __pickleIndex: "79", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0761859 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "02", actingUser: "the anonymous visitor", habit: "`A016 02 Old habit`", __pickleIndex: "45", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1540742 |
| F002 | Each of the six categories can be given to a catalog habit(n: "02", category: "Mind", __pickleIndex: "159", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0769445 |
| F002 | A registered user creates a custom habit | @F002 @AC-047 @FR-001 @FR-020 @US4-AS1 | passed | 00:00:00.1039364 |
| F001 | Five failed attempts during the block do not start a new block | @F001 @AC-023 @FR-008 | passed | 00:00:00.2666367 |
| F002 | For custom habits the acting user and ownership come first, the name check last(n: "05", actingUser: "the registered user `ac057.a.05@example.com`", action: "edits `Secret` to the name `TAKEN` and the icon `m"···, result: "\"Choose an icon.\"", __pickleIndex: "230", exampleTags: []) | @F002 @AC-057 @FR-005 @FR-016 @US4-AS11 | passed | 00:00:00.1550603 |
| F002 | The category filter counts only Published catalog habits(n: "01", actingUser: "the anonymous visitor", __pickleIndex: "20", exampleTags: []) | @F002 @AC-009 @FR-002 @FR-009 @FR-024 @US1-AS9 | passed | 00:00:00.3963961 |
| F002 | A custom habit with a name of 60 characters and a description of 500 characters is accepted | @F002 @AC-059 @FR-005 | passed | 00:00:00.0768206 |
| F001 | A password change accepts a new password at the lower bound of the password rule | @F001 @AC-032 @FR-002 @FR-013 | passed | 00:00:00.2386087 |
| F002 | Another user, and a catalog habit, may have the same name | @F002 @AC-052 @FR-004 @US4-AS6 | passed | 00:00:00.2515862 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "07", count: "1", marks: "none marked", result: "\"Mark one suggested starting goal for the card.\"", after: "no suggested starting goal", __pickleIndex: "115", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0759749 |
| F002 | Field checks refuse a custom habit with the same messages(n: "05", name: "`No intent`", description: "none", category: "Body", intent: "none", icon: "`droplet`", result: "\"Choose Build or Reduce.\"", __pickleIndex: "223", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0733209 |
| F001 | Administrator creation matches the email whatever its case and spaces | @F001 @AC-054 @FR-003 @FR-014 | passed | 00:00:00.0772575 |
| F002 | Demo loading without a valid demo password creates no catalog and no custom habit(n: "03", demoPassword: "`walk4`", result: "\"demo password does not meet the password rule\"", __pickleIndex: "243", exampleTags: []) | @F002 @AC-063 @FR-030 @FR-031 @US6-AS4 | passed | 00:00:00.3117535 |
| F001 | A password change to a password that breaks the password rule is refused(email: "\"weak.new@example.com\"", newPassword: "\"runningfar\"", whyItBreaksTheRule: "10 characters, no digit", __pickleIndex: "52", exampleTags: []) | @F001 @AC-034 @FR-002 @FR-013 @US6-AS3 | passed | 00:00:00.1513437 |
| F002 | Browsing by intent returns only that intent(n: "03", intent: "both", sugar: "contains", water: "contains", __pickleIndex: "7", exampleTags: []) | @F002 @AC-004 @FR-009 @US1-AS4 | passed | 00:00:00.0917924 |
| F002 | Category, intent and search apply together | @F002 @AC-010 @FR-009 | passed | 00:00:00.0968744 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "04", state: "Draft", action: "edits `A026 04 Other`", __pickleIndex: "67", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | passed | 00:00:00.0847908 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "04", fields: "Build, yes/no, 1 time per week", text: "\"once a week\"", kind: "\"Yes/no · flexible\"", __pickleIndex: "31", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0789791 |
| F001 | Administrator creation keeps the email in its trimmed, lower-case form | @F001 @AC-062 @FR-003 @FR-012 @FR-014 | passed | 00:00:00.0921130 |
| F002 | Only the 24 habit icons are accepted(n: "19", icon: "`wine-off`", result: "success", __pickleIndex: "182", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0734471 |
| F002 | Browsing gives the count of the habits it returns | @F002 @AC-011 @FR-008 | passed | 00:00:00.0929078 |
| F002 | The administrator reads a Draft with all its fields and its state | @F002 @AC-042 @FR-001 @FR-012 | passed | 00:00:00.0863343 |
| F002 | Field checks refuse a custom habit with the same messages(n: "06", name: "`No icon`", description: "none", category: "Body", intent: "Build", icon: "none", result: "\"Choose an icon.\"", __pickleIndex: "224", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0722148 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "03", actingUser: "the anonymous visitor", habit: "an unknown identifier", __pickleIndex: "46", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1522177 |
| F002 | A custom habit is not a catalog habit for the administrator(n: "04", action: "unpublishes", __pickleIndex: "107", exampleTags: []) | @F002 @AC-033 @FR-013 @FR-024 @US3-AS13 | passed | 00:00:00.1503792 |
| F002 | Browsing by intent returns only that intent(n: "01", intent: "Reduce", sugar: "contains", water: "does not contain", __pickleIndex: "5", exampleTags: []) | @F002 @AC-004 @FR-009 @US1-AS4 | passed | 00:00:00.0850909 |
| F002 | Each of the six categories can be given to a catalog habit(n: "04", category: "Money", __pickleIndex: "161", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0774386 |
| F001 | A successful sign-in resets the count of failed sign-ins | @F001 @AC-019 @FR-008 @US2-AS8 | passed | 00:00:00.3872620 |
| F002 | Custom habits never appear in browsing, for any audience(n: "02", actingUser: "the registered user `ac006.other.02@example.com`", __pickleIndex: "15", exampleTags: []) | @F002 @AC-006 @FR-024 @US1-AS6 | passed | 00:00:00.2331338 |
| F002 | Demo loading loads the seed catalog into an empty catalog | @F002 @AC-060 @FR-030 @US6-AS1 | passed | 00:00:00.4403687 |
| F002 | Field checks come in a fixed order(n: "06", fields: "the name `A029 06 Taken`, the category Body, the i"···, result: "\"Choose an icon.\"", __pickleIndex: "84", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0741961 |
| F002 | Each of the six categories can be given to a catalog habit(n: "03", category: "Health", __pickleIndex: "160", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0770290 |
| F001 | Administrator creation finds an existing administrator before it checks the password rule | @F001 @AC-047 @FR-014 @US4-AS8 | passed | 00:00:00.0807500 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@example.com.\"", whyItIsNotValid: "empty domain part after two non-empty ones", __pickleIndex: "15", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002312 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "03", fields: "Reduce, count 0 `sweets`, 1 time per day", storedUnit: "`sweets`", __pickleIndex: "140", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0764053 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "15", second: "Build, count 5 with the unit `\"  \"`, 1 time per "···, result: "\"Enter a unit, for example pages.\"", __pickleIndex: "130", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0796723 |
| F002 | The anonymous visitor has no access to custom habits(n: "03", action: "creates the custom habit `Visitor habit`", __pickleIndex: "216", exampleTags: []) | @F002 @AC-055 @FR-023 @US4-AS9 | passed | 00:00:00.0767176 |
| F001 | Administrator creation accepts a password at the lower bound of the password rule | @F001 @AC-040 @FR-002 @FR-014 | passed | 00:00:00.0767756 |
| F002 | Browsing orders habits by name, ignoring case | @F002 @AC-002 @FR-004 @FR-008 @US1-AS2 | passed | 00:00:00.0970488 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "06", action: "creates a custom habit with the name `\"   \"` and"···, __pickleIndex: "212", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1704342 |
| F002 | Demo accounts that demo loading creates get their demo custom habits | @F002 @AC-064 @FR-024 @FR-031 @US6-AS5 | passed | 00:00:00.5711406 |
| F002 | Browsing returns only Published habits, with their card content, to every audience(n: "03", actingUser: "the administrator `ac001.admin.03@example.com`", __pickleIndex: "2", exampleTags: []) | @F002 @AC-001 @FR-008 @US1-AS1 | passed | 00:00:00.1608502 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "10", second: "Build, duration 1000 minutes, 1 time per day", result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "125", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0757923 |
| F002 | For custom habits the acting user and ownership come first, the name check last(n: "01", actingUser: "the anonymous visitor", action: "creates a custom habit with the name `\"   \"`", result: "\"You are not signed in.\"", __pickleIndex: "226", exampleTags: []) | @F002 @AC-057 @FR-005 @FR-016 @US4-AS11 | passed | 00:00:00.1503172 |
| F001 | Registration refuses an email that is not valid(email: "\"   \"", whyItIsNotValid: "only spaces, empty after trimming", __pickleIndex: "20", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0029332 |
| F001 | Failed sign-ins for an email that has no account block nothing | @F001 @AC-025 @FR-007 @FR-008 | passed | 00:00:00.0834163 |
| F001 | A password change to a password that breaks the password rule is refused(email: "\"weak.new7@example.com\"", newPassword: "\"runnin4\"", whyItBreaksTheRule: "7 characters, with a digit", __pickleIndex: "53", exampleTags: []) | @F001 @AC-034 @FR-002 @FR-013 @US6-AS3 | passed | 00:00:00.1443651 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "02", search: "`\"MORNING\"`", morning: "contains", evening: "does not contain", __pickleIndex: "9", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.0784992 |
| F001 | The anonymous visitor cannot read account details | @F001 @AC-026 @FR-011 @US3-AS1 | passed | 00:00:00.0008246 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "11", second: "Reduce, count 1000 `sweets`, 1 time per day", result: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "126", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0761844 |
| F002 | Only the 24 habit icons are accepted(n: "23", icon: "`sparkles`", result: "success", __pickleIndex: "186", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0737775 |
| F002 | Only the 24 habit icons are accepted(n: "13", icon: "`pen-line`", result: "success", __pickleIndex: "176", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0729284 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "02", action: "edits `Gone` by its identifier, with the name `Bac"···, __pickleIndex: "232", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.1624030 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "14", second: "Reduce, duration 1000 hours, 1 time per day", result: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "129", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0783218 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "01", fields: "Build, duration 30 minutes, fixed days Monday to S"···, text: "\"30 min daily\"", kind: "\"Duration · fixed days every day\"", __pickleIndex: "28", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0780952 |
| F002 | Field checks come in a fixed order(n: "07", fields: "the name `A029 07 Taken`, the category Body, the i"···, result: "\"Mark one suggested starting goal for the card.\"", __pickleIndex: "85", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0738706 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "06", second: "Build, count 0 `pages`, 1 time per day", result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "121", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0776374 |
| F002 | A demo email that a person registered first keeps its owner's data untouched | @F002 @AC-067 @FR-031 @US6-AS8 | passed | 00:00:00.3847300 |
| F001 | The block ends exactly 5 minutes after the fifth failure | @F001 @AC-016 @FR-008 @US2-AS5 | passed | 00:00:00.2489986 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"sixth.admin@example.com\"", password: "\"\"", __pickleIndex: "69", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0027544 |
| F002 | Suggested starting goals are checked in list order, and each by its first failing field(n: "02", goals: "(1) Build, yes/no, 1 time per day (marked); (2) Bu"···, result: "\"Choose how it is measured.\"", position: "2", __pickleIndex: "155", exampleTags: []) | @F002 @AC-041 @FR-007 | passed | 00:00:00.0797972 |
| F002 | Publish and unpublish check the acting user and the role before the identifier(n: "02", actingUser: "the registered user `ac046.user.02@example.com`", action: "unpublishes", result: "\"Only administrators can manage the catalog.\"", __pickleIndex: "194", exampleTags: []) | @F002 @AC-046 @FR-016 | passed | 00:00:00.0727856 |
| F001 | Registration refuses a password that breaks the password rule(email: "\"seven.pw@example.com\"", password: "\"walkin4\"", whyItBreaksTheRule: "7 characters, with a digit", __pickleIndex: "2", exampleTags: []) | @F001 @AC-002 @FR-002 @US1-AS3 @US1-AS4 | passed | 00:00:00.0376331 |
| F002 | Browsing returns only Published habits, with their card content, to every audience(n: "02", actingUser: "the registered user `ac001.user.02@example.com`", __pickleIndex: "1", exampleTags: []) | @F002 @AC-001 @FR-008 @US1-AS1 | passed | 00:00:00.1550197 |
| F002 | An edit of a Published habit is public on the next read | @F002 @AC-020 @FR-012 @US3-AS3 | passed | 00:00:00.0903424 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "04", action: "deletes `Secret` by its identifier", __pickleIndex: "210", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1524677 |
| F002 | Field checks refuse a custom habit with the same messages(n: "01", name: "`\"   \"`", description: "none", category: "Body", intent: "Build", icon: "`droplet`", result: "\"Enter a name.\"", __pickleIndex: "219", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0786053 |
| F002 | The anonymous visitor has no access to custom habits(n: "05", action: "deletes `Secret` by its identifier", __pickleIndex: "218", exampleTags: []) | @F002 @AC-055 @FR-023 @US4-AS9 | passed | 00:00:00.0761476 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "16", second: "Build, count 5 with a unit of 21 characters, 1 tim"···, result: "\"The unit can have at most 20 characters.\"", __pickleIndex: "131", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0764483 |
| F002 | An unknown identifier does not exist for the administrator(n: "02", action: "edits, with the name `A032 02 Name`, the category "···, __pickleIndex: "101", exampleTags: []) | @F002 @AC-032 @FR-013 @US3-AS12 | passed | 00:00:00.0743156 |
| F002 | Browsing returns only Published habits, with their card content, to every audience(n: "01", actingUser: "the anonymous visitor", __pickleIndex: "0", exampleTags: []) | @F002 @AC-001 @FR-008 @US1-AS1 | passed | 00:00:00.1517245 |
| F002 | A Draft cannot be unpublished | @F002 @AC-023 @FR-011 @US3-AS6 | passed | 00:00:00.0910511 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "03", action: "deletes `Gone` by its identifier", __pickleIndex: "233", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.1521515 |
| F001 | A block applies only to its own account | @F001 @AC-024 @FR-008 | passed | 00:00:00.2885026 |
| F002 | A registered user edits every field of their custom habit | @F002 @AC-049 @FR-004 @FR-020 @US4-AS3 | passed | 00:00:00.1136214 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "12", fields: "Build, duration 20 minutes, 1 time per day", text: "\"20 min a day\"", kind: "\"Duration · flexible\"", __pickleIndex: "39", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0765924 |
| F002 | The anonymous visitor cannot manage the catalog(n: "06", action: "unpublishes `A031 06 Published`", __pickleIndex: "99", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0753916 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "05", actingUser: "the registered user `ac016.user.05@example.com`", habit: "`A016 05 Old habit`", __pickleIndex: "48", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1532449 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@example..com\"", whyItIsNotValid: "empty domain part between two non-empty ones", __pickleIndex: "13", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002130 |
| F002 | Field checks come in a fixed order(n: "09", fields: "the name `a029 09 taken`, the category Body, the i"···, result: "\"A catalog habit with this name already exists.\"", __pickleIndex: "87", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0745000 |
| F001 | Demo loading without a demo password creates no account(demoPassword: "\"   \"", __pickleIndex: "86", exampleTags: []) | @F001 @AC-055 @FR-016 @US7-AS3 | passed | 00:00:00.3037476 |
| F002 | Names and descriptions are trimmed before they are checked and stored(n: "01", name: "`\"   A070 01 Trimmed   \"`", description: "`\"  Read daily.  \"`", storedName: "`A070 01 Trimmed`", storedDescription: "the description \"Read daily.\"", __pickleIndex: "250", exampleTags: []) | @F002 @AC-070 @FR-004 @FR-005 | passed | 00:00:00.0797332 |
| F002 | A registered user cannot manage the catalog(n: "01", action: "lists the catalog habits as an administrator", __pickleIndex: "88", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1670950 |
| F002 | Only the 24 habit icons are accepted(n: "01", icon: "`book-open`", result: "success", __pickleIndex: "164", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0745848 |
| F002 | Only the 24 habit icons are accepted(n: "16", icon: "`salad`", result: "success", __pickleIndex: "179", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0738538 |
| F002 | Only the 24 habit icons are accepted(n: "17", icon: "`utensils`", result: "success", __pickleIndex: "180", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0761470 |
| F002 | Custom habits never appear in browsing, for any audience(n: "01", actingUser: "the registered user `ac006.owner.01@example.com`", __pickleIndex: "14", exampleTags: []) | @F002 @AC-006 @FR-024 @US1-AS6 | passed | 00:00:00.2169502 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "01", search: "`\"  a005 01 morn  \"`", morning: "contains", evening: "does not contain", __pickleIndex: "8", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.0803745 |
| F001 | A registered user gets only their own account details | @F001 @AC-029 @FR-010 @FR-012 @US3-AS4 | passed | 00:00:00.1155658 |
| F002 | Browsing by intent returns only that intent(n: "02", intent: "Build", sugar: "does not contain", water: "contains", __pickleIndex: "6", exampleTags: []) | @F002 @AC-004 @FR-009 @US1-AS4 | passed | 00:00:00.0846491 |
| F002 | Unpublishing an Unpublished habit is a success that changes nothing | @F002 @AC-025 @FR-011 @US3-AS8 | passed | 00:00:00.1091172 |
| F002 | Browsing with no match gives an empty result | @F002 @AC-007 @FR-008 @FR-009 @US1-AS7 | passed | 00:00:00.0771033 |
| F001 | Demo loading decides per demo email, and a demo email registered earlier keeps its owner's password | @F001 @AC-060 @FR-003 @FR-016 | passed | 00:00:00.4609365 |
| F002 | Only the 24 habit icons are accepted(n: "06", icon: "`candy`", result: "success", __pickleIndex: "169", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0781949 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "08", name: "`A027 08 Menu icon`", description: "none", category: "Body", intent: "Build", icon: "`menu`", result: "\"Choose an icon.\"", __pickleIndex: "77", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0759879 |
| F002 | Publish and unpublish check the acting user and the role before the identifier(n: "01", actingUser: "the registered user `ac046.user.01@example.com`", action: "publishes", result: "\"Only administrators can manage the catalog.\"", __pickleIndex: "193", exampleTags: []) | @F002 @AC-046 @FR-016 | passed | 00:00:00.0736088 |
| F001 | A new email creates an additional administrator | @F001 @AC-053 @FR-014 @US4-AS9 | passed | 00:00:00.1490682 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@example\"", whyItIsNotValid: "domain has one part", __pickleIndex: "9", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002242 |
| F002 | A registered user cannot manage the catalog(n: "05", action: "publishes `A030 05 Draft`", __pickleIndex: "92", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1530815 |
| F001 | A blocked account still fails with the correct password 4 minutes 59 seconds after the fifth failure | @F001 @AC-015 @FR-007 @FR-008 @US2-AS4 | passed | 00:00:00.2414588 |
| F001 | Administrator creation does not turn a registered user into an administrator | @F001 @AC-042 @FR-014 @US4-AS3 | passed | 00:00:00.0782603 |
| F002 | Custom-habit names and descriptions are trimmed before they are checked and stored(n: "04", name: "`\"Long\"`", description: "`\"  \"` + a text of 500 characters + `\"  \"` (50"···, storedName: "`Long`", storedDescription: "the 500-character description", __pickleIndex: "259", exampleTags: []) | @F002 @AC-073 @FR-004 @FR-005 | passed | 00:00:00.0769067 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "01", second: "no Build or Reduce, yes/no, 1 time per day", result: "\"Choose Build or Reduce.\"", __pickleIndex: "116", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0786026 |
| F002 | Another registered user cannot reach a custom habit(n: "01", action: "reads", __pickleIndex: "204", exampleTags: []) | @F002 @AC-053 @FR-021 @FR-022 @US4-AS7 | passed | 00:00:00.1847100 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "08", fields: "Build, yes/no, fixed days Monday, Tuesday, Wednesd"···, text: "\"4 days a week\"", kind: "\"Yes/no · fixed days Mon–Wed, Sat\"", __pickleIndex: "35", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.1198675 |
| F002 | Each of the six categories can be given to a catalog habit(n: "05", category: "Home", __pickleIndex: "162", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0751632 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "02", second: "Build, no measurement, 1 time per day", result: "\"Choose how it is measured.\"", __pickleIndex: "117", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0799720 |
| F002 | The Home preview has fewer than four habits when fewer are Published | @F002 @AC-072 @FR-008 @US5-AS2 | passed | 00:00:00.2663792 |
| F001 | Administrator creation checks for a missing password before the email | @F001 @AC-048 @FR-014 | passed | 00:00:00.0014378 |
| F002 | Field checks come in a fixed order(n: "03", fields: "the name `A029 03 Long`, a description that is a t"···, result: "\"The description can have at most 500 characters."···, __pickleIndex: "81", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0759056 |
| F002 | Field checks refuse a custom habit with the same messages(n: "07", name: "`Menu icon`", description: "none", category: "Body", intent: "Build", icon: "`menu`", result: "\"Choose an icon.\"", __pickleIndex: "225", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0744681 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "01", actingUser: "the anonymous visitor", habit: "`A016 01 Draft habit`", __pickleIndex: "44", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1528593 |
| F002 | Only the 24 habit icons are accepted(n: "18", icon: "`cigarette-off`", result: "success", __pickleIndex: "181", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0742445 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "06", fields: "Build, count 10 `pages`, fixed day Saturday", text: "\"10 pages, once a week\"", kind: "\"Count · fixed days Sat\"", __pickleIndex: "33", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0759555 |
| F002 | A name of 60 characters and a description of 500 characters are accepted | @F002 @AC-028 @FR-005 @US3-AS10 | passed | 00:00:00.0951782 |
| F002 | Names and descriptions are trimmed before they are checked and stored(n: "04", name: "`\"A070 04 Long\"`", description: "`\"  \"` + a text of 500 characters + `\"  \"` (50"···, storedName: "`A070 04 Long`", storedDescription: "the 500-character description", __pickleIndex: "253", exampleTags: []) | @F002 @AC-070 @FR-004 @FR-005 | passed | 00:00:00.0870750 |
| F002 | Publish and unpublish check the acting user and the role before the identifier(n: "03", actingUser: "the anonymous visitor", action: "publishes", result: "\"You are not signed in.\"", __pickleIndex: "195", exampleTags: []) | @F002 @AC-046 @FR-016 | passed | 00:00:00.0714499 |
| F001 | Registration refuses a password that breaks the password rule(email: "\"eight.nodigit@example.com\"", password: "\"walkingx\"", whyItBreaksTheRule: "8 characters, no digit", __pickleIndex: "4", exampleTags: []) | @F001 @AC-002 @FR-002 @US1-AS3 @US1-AS4 | passed | 00:00:00.0378826 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@.example.com\"", whyItIsNotValid: "empty domain part before two non-empty ones", __pickleIndex: "14", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002116 |
| F002 | The administrator edits the description, intent and icon of a catalog habit | @F002 @AC-071 @FR-012 | passed | 00:00:00.1072599 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "03", name: "a name of 61 characters with the prefix `A027 03`", description: "none", category: "Body", intent: "Build", icon: "`book-open`", result: "\"The name can have at most 60 characters.\"", __pickleIndex: "72", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0754382 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "04", search: "`\"a005 04 e\"`", morning: "does not contain", evening: "contains", __pickleIndex: "11", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.0799625 |
| F002 | Only the 24 habit icons are accepted(n: "03", icon: "`smartphone`", result: "success", __pickleIndex: "166", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0737095 |
| F002 | An unknown identifier does not exist for the administrator(n: "03", action: "publishes", __pickleIndex: "102", exampleTags: []) | @F002 @AC-032 @FR-013 @US3-AS12 | passed | 00:00:00.0739783 |
| F001 | Administrator creation refuses a password that breaks the password rule(email: "\"seventh.admin@example.com\"", password: "\"curatorship\"", whyItBreaksTheRule: "11 characters, no digit", __pickleIndex: "72", exampleTags: []) | @F001 @AC-046 @FR-002 @FR-014 @US4-AS7 | passed | 00:00:00.0785921 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "09", second: "Build, duration 0 minutes, 1 time per day", result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "124", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0782560 |
| F002 | The role and the identifier are checked before the fields(n: "02", actingUser: "the anonymous visitor", action: "creates a catalog habit with the name `\"   \"`", result: "\"You are not signed in.\"", __pickleIndex: "150", exampleTags: []) | @F002 @AC-039 @FR-016 @US3-AS18 | passed | 00:00:00.1759356 |
| F001 | Registration refuses an email that is not valid(email: "\"ana @example.com\"", whyItIsNotValid: "a space inside", __pickleIndex: "16", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002097 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "16", fields: "Reduce, count 5 `cups`, 2 times per week", text: "\"at most 5 cups, 2 times a week\"", kind: "\"Count · flexible\"", __pickleIndex: "43", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0791825 |
| F002 | Custom-habit names and descriptions are trimmed before they are checked and stored(n: "03", name: "`\"   \"` + `Habit ` followed by `x` up to 60 char"···, description: "none", storedName: "the 60-character name", storedDescription: "no description", __pickleIndex: "258", exampleTags: []) | @F002 @AC-073 @FR-004 @FR-005 | passed | 00:00:00.0749856 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@example.\"", whyItIsNotValid: "domain has one non-empty part, before an empty one", __pickleIndex: "12", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002067 |
| F002 | The role and the identifier are checked before the fields(n: "01", actingUser: "the registered user `ac039.user.01@example.com`", action: "creates a catalog habit with the name `\"   \"`", result: "\"Only administrators can manage the catalog.\"", __pickleIndex: "149", exampleTags: []) | @F002 @AC-039 @FR-016 @US3-AS18 | passed | 00:00:00.1781615 |
| F002 | Custom-habit names and descriptions are trimmed before they are checked and stored(n: "02", name: "`\"Blank\"`", description: "`\"   \"`", storedName: "`Blank`", storedDescription: "no description", __pickleIndex: "257", exampleTags: []) | @F002 @AC-073 @FR-004 @FR-005 | passed | 00:00:00.0765049 |
| F002 | Demo loading without a valid demo password creates no catalog and no custom habit(n: "01", demoPassword: "absent", result: "\"no demo password configured\"", __pickleIndex: "241", exampleTags: []) | @F002 @AC-063 @FR-030 @FR-031 @US6-AS4 | passed | 00:00:00.3095630 |
| F001 | An administrator cannot change another account's password | @F001 @AC-038 @FR-010 @FR-013 @FR-015 @US6-AS7 | passed | 00:00:00.2195857 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "03", state: "Unpublished", action: "creates a catalog habit", __pickleIndex: "66", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | passed | 00:00:00.0794771 |
| F001 | The anonymous visitor cannot change a password, whatever the new password(newPassword: "\"run\"", whyItMatters: "breaks the password rule, which is checked only fo"···, __pickleIndex: "56", exampleTags: []) | @F001 @AC-036 @FR-011 @FR-013 @US6-AS5 | passed | 00:00:00.0016029 |
| F001 | Registration checks the email before the password rule | @F001 @AC-009 @FR-003 | passed | 00:00:00.0008731 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "02", fields: "Build, count 999 `pages`, 20 times per week", storedUnit: "`pages`", __pickleIndex: "139", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0779727 |
| F001 | An unknown email, a wrong password and a blocked account give the same result | @F001 @AC-014 @FR-007 @FR-008 | passed | 00:00:00.2995142 |
| F002 | Only the 24 habit icons are accepted(n: "21", icon: "`receipt`", result: "success", __pickleIndex: "184", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0741460 |
| F001 | An attempt during the block does not extend it | @F001 @AC-017 @FR-008 @US2-AS6 | passed | 00:00:00.2543489 |
| F002 | The anonymous visitor cannot manage the catalog(n: "02", action: "reads `A031 02 Published` as an administrator", __pickleIndex: "95", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0776707 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "04", name: "`A027 04 Long text`", description: "a text of 501 characters", category: "Body", intent: "Build", icon: "`book-open`", result: "\"The description can have at most 500 characters."···, __pickleIndex: "73", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0780333 |
| F002 | Only the 24 habit icons are accepted(n: "12", icon: "`brain`", result: "success", __pickleIndex: "175", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0730616 |
| F001 | A signed-in user changes their password | @F001 @AC-031 @FR-013 @US6-AS1 | passed | 00:00:00.2430458 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "10", fields: "Build, duration 1 hours, fixed day Sunday", storedUnit: "hours", __pickleIndex: "147", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0787172 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "05", search: "`\"\"`", morning: "contains", evening: "contains", __pickleIndex: "12", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.0768823 |
| F002 | The anonymous visitor cannot manage the catalog(n: "04", action: "edits `A031 04 Published` to the name `A031 04 Ren"···, __pickleIndex: "97", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0762454 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "04", actingUser: "the registered user `ac016.user.04@example.com`", habit: "`A016 04 Draft habit`", __pickleIndex: "47", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1531535 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"   \"", password: "\"curator2026\"", __pickleIndex: "67", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0004515 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "07", actingUser: "the administrator `ac016.admin.07@example.com`", habit: "`A016 07 Draft habit`", __pickleIndex: "50", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1546868 |
| F002 | Demo loading run again leaves the seed catalog unchanged | @F002 @AC-061 @FR-030 @US6-AS2 | passed | 00:00:00.4486945 |
| F002 | Suggested starting goals are checked in list order, and each by its first failing field(n: "03", goals: "(1) Build, yes/no, 1 time per day (marked); (2) Bu"···, result: "\"Choose fixed days or how many times.\"", position: "3", __pickleIndex: "156", exampleTags: []) | @F002 @AC-041 @FR-007 | passed | 00:00:00.0770018 |
| F001 | Registration refuses a password that breaks the password rule(email: "\"no.digit@example.com\"", password: "\"walkingfar\"", whyItBreaksTheRule: "10 characters, no digit", __pickleIndex: "3", exampleTags: []) | @F001 @AC-002 @FR-002 @US1-AS3 @US1-AS4 | passed | 00:00:00.0360681 |
| F001 | Registration accepts a password at the lower bound of the password rule(email: "\"digits.pw@example.com\"", password: "\"12345678\"", whyItMeetsTheRule: "8 characters, all digits", __pickleIndex: "6", exampleTags: []) | @F001 @AC-003 @FR-001 @FR-002 | passed | 00:00:00.0724363 |
| F002 | Only the 24 habit icons are accepted(n: "07", icon: "`coffee`", result: "success", __pickleIndex: "170", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0809290 |
| F002 | Each of the six categories can be given to a catalog habit(n: "06", category: "Other", __pickleIndex: "163", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0782663 |
| F001 | An account registered with spaces around the email has the trimmed email | @F001 @AC-061 @FR-003 @FR-012 | passed | 00:00:00.0785941 |
| F001 | A password of only spaces creates no account | @F001 @AC-044 @FR-014 @US4-AS5 | passed | 00:00:00.0778445 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "19", second: "Build, duration 15 minutes, fixed days with no day", result: "\"Choose at least one day.\"", __pickleIndex: "134", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0784220 |
| F002 | The Home preview is the first four Published habits in name order | @F002 @AC-012 @FR-008 @FR-024 @US5-AS2 | passed | 00:00:00.3472981 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "06", search: "`\"   \"`", morning: "contains", evening: "contains", __pickleIndex: "13", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.0983279 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "03", action: "edits `Secret` by its identifier, with the name `S"···, __pickleIndex: "209", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1495399 |
| F001 | Administrator creation finds a registered user before it checks the password rule | @F001 @AC-050 @FR-014 | passed | 00:00:00.0787790 |
| F002 | Only the 24 habit icons are accepted(n: "11", icon: "`bike`", result: "success", __pickleIndex: "174", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0728353 |
| F002 | Demo loading run again leaves the demo accounts and their custom habits unchanged | @F002 @AC-065 @FR-031 @US6-AS6 | passed | 00:00:00.4559654 |
| F001 | Sign-in with a wrong password fails with the single message | @F001 @AC-013 @FR-007 @US2-AS3 | passed | 00:00:00.0726094 |
| F002 | The anonymous visitor has no access to custom habits(n: "02", action: "reads `Secret` by its identifier", __pickleIndex: "215", exampleTags: []) | @F002 @AC-055 @FR-023 @US4-AS9 | passed | 00:00:00.0776807 |
| F002 | Details with a custom habit's identifier give "not in the catalog"(n: "02", actingUser: "the administrator `ac017.admin.02@example.com`", __pickleIndex: "54", exampleTags: []) | @F002 @AC-017 @FR-010 @FR-024 @US2-AS5 | passed | 00:00:00.1500037 |
| F001 | Demo loading run again changes nothing | @F001 @AC-059 @FR-016 @US7-AS2 | passed | 00:00:00.6056884 |
| F001 | The end of a block resets the count of failed sign-ins | @F001 @AC-018 @FR-008 @US2-AS7 | passed | 00:00:00.4022632 |
| F002 | Another registered user cannot reach a custom habit(n: "03", action: "deletes", __pickleIndex: "206", exampleTags: []) | @F002 @AC-053 @FR-021 @FR-022 @US4-AS7 | passed | 00:00:00.1844753 |
| F002 | Publishing a Published habit is a success that changes nothing | @F002 @AC-024 @FR-011 @US3-AS7 | passed | 00:00:00.1043703 |
| F002 | Names and descriptions are trimmed before they are checked and stored(n: "02", name: "`\"A070 02 Blank\"`", description: "`\"   \"`", storedName: "`A070 02 Blank`", storedDescription: "no description", __pickleIndex: "251", exampleTags: []) | @F002 @AC-070 @FR-004 @FR-005 | passed | 00:00:00.0767997 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "21", second: "Build, duration 15 minutes, 21 times per week", result: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "136", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0775209 |
| F002 | A registered user cannot manage the catalog(n: "06", action: "unpublishes `A030 06 Draft`", __pickleIndex: "93", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1514950 |
| F002 | Field checks come in a fixed order(n: "08", fields: "the name `A029 08 List`, the category Body, the in"···, result: "\"Mark one suggested starting goal for the card.\"", __pickleIndex: "86", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0765512 |
| F001 | Demo loading with a password that breaks the password rule creates no account(demoPassword: "\"examplepass\"", whyItBreaksTheRule: "11 characters, no digit", __pickleIndex: "87", exampleTags: []) | @F001 @AC-056 @FR-002 @FR-016 @US7-AS4 | passed | 00:00:00.3053156 |
| F002 | Field checks come in a fixed order(n: "02", fields: "a name of 61 characters with the prefix `A029 02`,"···, result: "\"The name can have at most 60 characters.\"", __pickleIndex: "80", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0764191 |
| F002 | An unknown identifier does not exist for the administrator(n: "04", action: "unpublishes", __pickleIndex: "103", exampleTags: []) | @F002 @AC-032 @FR-013 @US3-AS12 | passed | 00:00:00.0752847 |
| F001 | Administrator creation checks for a missing password before it looks for an existing administrator | @F001 @AC-051 @FR-014 | passed | 00:00:00.1037866 |
| F002 | An unknown identifier does not exist for the administrator(n: "01", action: "reads", __pickleIndex: "100", exampleTags: []) | @F002 @AC-032 @FR-013 @US3-AS12 | passed | 00:00:00.0719118 |
| F002 | Custom habits never appear in browsing, for any audience(n: "03", actingUser: "the administrator `ac006.admin.03@example.com`", __pickleIndex: "16", exampleTags: []) | @F002 @AC-006 @FR-024 @US1-AS6 | passed | 00:00:00.2220437 |
| F002 | The anonymous visitor cannot manage the catalog(n: "01", action: "lists the catalog habits as an administrator", __pickleIndex: "94", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0775172 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "04", fields: "Reduce, duration 999 minutes, 20 times per day", storedUnit: "minutes", __pickleIndex: "141", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0786763 |
| F002 | A demo email held by an administrator gets no custom habit | @F002 @AC-068 @FR-025 @FR-031 | passed | 00:00:00.4011935 |
| F002 | Only the 24 habit icons are accepted(n: "10", icon: "`droplet`", result: "success", __pickleIndex: "173", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0750769 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "05", name: "`A027 05 No category`", description: "none", category: "none", intent: "Build", icon: "`book-open`", result: "\"Choose a category.\"", __pickleIndex: "74", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0730003 |
| F001 | Registration refuses a password that breaks the password rule(email: "\"short.pw@example.com\"", password: "\"walk4\"", whyItBreaksTheRule: "5 characters", __pickleIndex: "1", exampleTags: []) | @F001 @AC-002 @FR-002 @US1-AS3 @US1-AS4 | passed | 00:00:00.0392275 |
| F001 | An administrator reads their own account details | @F001 @AC-028 @FR-009 @FR-012 @FR-015 @US3-AS3 | passed | 00:00:00.0784724 |
| F002 | Details with a custom habit's identifier give "not in the catalog"(n: "01", actingUser: "the registered user `ac017.owner.01@example.com`", __pickleIndex: "53", exampleTags: []) | @F002 @AC-017 @FR-010 @FR-024 @US2-AS5 | passed | 00:00:00.1486418 |
| F002 | Only the 24 habit icons are accepted(n: "28", icon: "`rocket`", result: "\"Choose an icon.\"", __pickleIndex: "191", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0715692 |
| F001 | A password change checks the current password before the password rule | @F001 @AC-035 @FR-013 @US6-AS4 | passed | 00:00:00.1205522 |
| F001 | Demo loading accepts a demo password at the lower bound of the password rule | @F001 @AC-058 @FR-002 @FR-016 | passed | 00:00:00.3661444 |
| F002 | Only the 24 habit icons are accepted(n: "04", icon: "`footprints`", result: "success", __pickleIndex: "167", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0743273 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"spaces.admin@example.com\"", password: "\"   \"", __pickleIndex: "68", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0001691 |
| F002 | A registered user cannot manage the catalog(n: "02", action: "reads `A030 02 Draft` as an administrator", __pickleIndex: "89", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1719770 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "02", fields: "Reduce, duration 2 hours, 1 time per day", text: "\"at most 2 h a day\"", kind: "\"Duration · flexible\"", __pickleIndex: "29", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0742739 |
| F001 | Administrator creation checks for a missing password before it looks for a registered user | @F001 @AC-052 @FR-014 | passed | 00:00:00.0890842 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "08", fields: "Build, count 5 with the unit `\"  Pages  \"` (5 ch"···, storedUnit: "`Pages`", __pickleIndex: "145", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0790506 |
| F001 | Registration refuses an email that already has an account, in any case and with spaces | @F001 @AC-007 @FR-003 @US1-AS2 | passed | 00:00:00.1157033 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "01", action: "reads `Gone` by its identifier", __pickleIndex: "231", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.1729568 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "05", fields: "Build, duration 1 minutes, 1 time per week", storedUnit: "minutes", __pickleIndex: "142", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0869328 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "07", fields: "Build, count 5 with a unit of 20 characters, 1 tim"···, storedUnit: "that unit of 20 characters", __pickleIndex: "144", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0764396 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "22", second: "Build, duration 15 minutes, 3 times with no period", result: "\"Choose per day or per week.\"", __pickleIndex: "137", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0812286 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "08", actingUser: "the administrator `ac016.admin.08@example.com`", habit: "`A016 08 Old habit`", __pickleIndex: "51", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1551989 |
| F002 | Names are ordered ordinally, ignoring case, not by the current culture | @F002 @AC-045 @FR-004 | passed | 00:00:00.0826523 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "14", fields: "Build, yes/no, fixed days Monday, Tuesday and Sund"···, text: "\"3 days a week\"", kind: "\"Yes/no · fixed days Mon, Tue, Sun\"", __pickleIndex: "41", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0777173 |
| F002 | Browsing by category returns only that category | @F002 @AC-003 @FR-009 @US1-AS3 | passed | 00:00:00.1014704 |
| F001 | Registration creates a registered user | @F001 @AC-001 @FR-001 @FR-004 @FR-009 @US1-AS1 | passed | 00:00:00.0787579 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "01", state: "Draft", action: "creates a catalog habit", __pickleIndex: "64", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | passed | 00:00:00.0768236 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "01", count: "2", marks: "none marked", result: "\"Mark one suggested starting goal for the card.\"", after: "no suggested starting goal", __pickleIndex: "109", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0757191 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "13", fields: "Build, yes/no, fixed days Monday to Saturday", text: "\"6 days a week\"", kind: "\"Yes/no · fixed days Mon–Sat\"", __pickleIndex: "40", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0824559 |
| F002 | Custom habits never appear in browsing, for any audience(n: "04", actingUser: "the anonymous visitor", __pickleIndex: "17", exampleTags: []) | @F002 @AC-006 @FR-024 @US1-AS6 | passed | 00:00:00.2186387 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "09", fields: "Build, count 5 with the unit `\"  \"` followed by "···, storedUnit: "that unit of 20 characters, without the spaces", __pickleIndex: "146", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0796276 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"fourth.admin@example.com\"", password: "absent", __pickleIndex: "64", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0001778 |
| F001 | Five failed sign-ins in a row block however far apart they are | @F001 @AC-021 @FR-008 | passed | 00:00:00.2185178 |
| F002 | A Reduce amount of 0 is accepted and shown as "at most 0" | @F002 @AC-038 @FR-007 @FR-017 @US3-AS17 | passed | 00:00:00.0817963 |
| F002 | The category filter counts only Published catalog habits(n: "03", actingUser: "the administrator `ac009.admin.03@example.com`", __pickleIndex: "22", exampleTags: []) | @F002 @AC-009 @FR-002 @FR-009 @FR-024 @US1-AS9 | passed | 00:00:00.3930037 |
| F002 | Field checks refuse a custom habit with the same messages(n: "03", name: "`Long text`", description: "a text of 501 characters", category: "Body", intent: "Build", icon: "`droplet`", result: "\"The description can have at most 500 characters."···, __pickleIndex: "221", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0720737 |
| F002 | For custom habits the acting user and ownership come first, the name check last(n: "04", actingUser: "the registered user `ac057.a.04@example.com`", action: "creates a custom habit with the name `taken` and n"···, result: "\"Choose a category.\"", __pickleIndex: "229", exampleTags: []) | @F002 @AC-057 @FR-005 @FR-016 @US4-AS11 | passed | 00:00:00.1478999 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "04", count: "6", marks: "the first marked", result: "\"A habit can have at most 5 suggested starting go"···, after: "no suggested starting goal", __pickleIndex: "112", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0758531 |
| F002 | Only the 24 habit icons are accepted(n: "20", icon: "`wallet`", result: "success", __pickleIndex: "183", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0776782 |
| F001 | A password change with a wrong current password is refused | @F001 @AC-033 @FR-013 @US6-AS2 | passed | 00:00:00.1789334 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "02", action: "reads `Secret` by its identifier", __pickleIndex: "208", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1483972 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "09", fields: "Build, yes/no, fixed days Saturday and Sunday", text: "\"2 days a week\"", kind: "\"Yes/no · fixed days Sat, Sun\"", __pickleIndex: "36", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0748714 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@.\"", whyItIsNotValid: "domain has no non-empty part", __pickleIndex: "10", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002199 |
| F001 | Administrator creation without an email or a password configures nothing(email: "absent", password: "absent", __pickleIndex: "63", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0001901 |
| F002 | A habit without suggested starting goals is shown without one | @F002 @AC-014 @FR-008 @FR-010 @US2-AS2 | passed | 00:00:00.1230814 |
| F002 | Demo loading without a valid demo password creates no catalog and no custom habit(n: "02", demoPassword: "`\"   \"`", result: "\"no demo password configured\"", __pickleIndex: "242", exampleTags: []) | @F002 @AC-063 @FR-030 @FR-031 @US6-AS4 | passed | 00:00:00.2981311 |
| F002 | Field checks refuse a custom habit with the same messages(n: "04", name: "`No category`", description: "none", category: "none", intent: "Build", icon: "`droplet`", result: "\"Choose a category.\"", __pickleIndex: "222", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0731478 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@\"", whyItIsNotValid: "no domain", __pickleIndex: "8", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002769 |
| F002 | Demo loading adds no seed to a catalog that has a habit | @F002 @AC-062 @FR-024 @FR-030 @US6-AS3 | passed | 00:00:00.4459434 |
| F001 | Registration refuses an email that is not valid(email: "\"\"", whyItIsNotValid: "empty", __pickleIndex: "19", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002442 |
| F001 | Registration keeps the email in its lower-case form | @F001 @AC-006 @FR-003 @FR-006 @FR-012 | passed | 00:00:00.0990540 |
| F002 | Only the 24 habit icons are accepted(n: "09", icon: "`dumbbell`", result: "success", __pickleIndex: "172", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0747481 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "05", action: "deletes a custom habit with the identifier of `A05"···, __pickleIndex: "235", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.1547095 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "07", action: "edits `Secret` by its identifier, with the name `\"···, __pickleIndex: "213", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1530612 |
| F001 | Demo loading creates the three demo accounts | @F001 @AC-057 @FR-016 @US7-AS1 | passed | 00:00:00.4470423 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "05", fields: "Build, duration 1 hours, fixed days Monday, Wednes"···, text: "\"1 h, 3 days a week\"", kind: "\"Duration · fixed days Mon, Wed, Fri\"", __pickleIndex: "32", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0786287 |
| F002 | Publish and unpublish check the acting user and the role before the identifier(n: "04", actingUser: "the anonymous visitor", action: "unpublishes", result: "\"You are not signed in.\"", __pickleIndex: "196", exampleTags: []) | @F002 @AC-046 @FR-016 | passed | 00:00:00.0723266 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "01", action: "lists custom habits", __pickleIndex: "207", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1507936 |
| F001 | Four failed sign-ins in a row do not block | @F001 @AC-020 @FR-008 | passed | 00:00:00.2202198 |
| F002 | Custom-habit names and descriptions are trimmed before they are checked and stored(n: "01", name: "`\"   Cold shower   \"`", description: "`\"  In the morning.  \"`", storedName: "`Cold shower`", storedDescription: "the description \"In the morning.\"", __pickleIndex: "256", exampleTags: []) | @F002 @AC-073 @FR-004 @FR-005 | passed | 00:00:00.0730795 |
| F002 | A user's custom-habit name is taken, ignoring case and spaces(n: "02", action: "edits `Evening walk`", __pickleIndex: "202", exampleTags: []) | @F002 @AC-051 @FR-004 @FR-005 @US4-AS5 | passed | 00:00:00.0902044 |
| F002 | Deleted demo custom habits are not created again | @F002 @AC-066 @FR-031 @US6-AS7 | passed | 00:00:00.5098049 |
| F002 | Unpublishing a Published habit takes it out of the catalog at once | @F002 @AC-021 @FR-011 @FR-015 @US3-AS4 | passed | 00:00:00.1075289 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "20", second: "Build, duration 15 minutes, 0 times per week", result: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "135", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0777512 |
| F002 | The administrator's list is in name order and can be searched | @F002 @AC-040 @FR-004 @FR-012 @US3-AS19 | passed | 00:00:00.1047217 |
| F002 | Only the 24 habit icons are accepted(n: "08", icon: "`moon`", result: "success", __pickleIndex: "171", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0859848 |
| F002 | Names and descriptions are trimmed before they are checked and stored(n: "03", name: "`\"   \"` + `A070 03 ` followed by `x` up to 60 ch"···, description: "none", storedName: "the 60-character name", storedDescription: "no description", __pickleIndex: "252", exampleTags: []) | @F002 @AC-070 @FR-004 @FR-005 | passed | 00:00:00.0876011 |
| F002 | A registered user deletes their custom habit | @F002 @AC-050 @FR-020 @US4-AS4 | passed | 00:00:00.1069186 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "03", count: "6", marks: "none marked", result: "\"A habit can have at most 5 suggested starting go"···, after: "no suggested starting goal", __pickleIndex: "111", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0799692 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@@example.com\"", whyItIsNotValid: "two \"@\"", __pickleIndex: "17", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002059 |
| F002 | The role and the identifier are checked before the fields(n: "04", actingUser: "the registered user `ac039.user.04@example.com`", action: "edits an unknown identifier with the name `\"   \""···, result: "\"Only administrators can manage the catalog.\"", __pickleIndex: "152", exampleTags: []) | @F002 @AC-039 @FR-016 @US3-AS18 | passed | 00:00:00.1665362 |
| F002 | Only the 24 habit icons are accepted(n: "25", icon: "`menu`", result: "\"Choose an icon.\"", __pickleIndex: "188", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0729566 |
| F002 | Only the 24 habit icons are accepted(n: "24", icon: "`tv`", result: "success", __pickleIndex: "187", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0744795 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "05", state: "Published", action: "edits `A026 05 Other`", __pickleIndex: "68", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | passed | 00:00:00.0820919 |
| F002 | Field checks come in a fixed order(n: "05", fields: "the name `A029 05 Int`, the category Body, no inte"···, result: "\"Choose Build or Reduce.\"", __pickleIndex: "83", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0747514 |
| F001 | Administrator creation without an email or a password configures nothing(email: "absent", password: "\"curator2026\"", __pickleIndex: "65", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0001750 |
| F001 | Administrator creation finds a registered user whatever the case and spaces of the email | @F001 @AC-063 @FR-003 @FR-014 | passed | 00:00:00.0863065 |
| F001 | Registration accepts the shortest valid email and a domain of three parts(email: "\"ana@sub.example.com\"", __pickleIndex: "22", exampleTags: []) | @F001 @AC-005 @FR-001 @FR-003 | passed | 00:00:00.0727513 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "09", actingUser: "the administrator `ac016.admin.09@example.com`", habit: "an unknown identifier", __pickleIndex: "52", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1522921 |
| F002 | Suggested starting goals are checked in list order, and each by its first failing field(n: "01", goals: "(1) no Build or Reduce, no measurement, no schedul"···, result: "\"Choose Build or Reduce.\"", position: "1", __pickleIndex: "154", exampleTags: []) | @F002 @AC-041 @FR-007 | passed | 00:00:00.0727963 |
| F001 | Demo loading with a password that breaks the password rule creates no account(demoPassword: "\"exampl1\"", whyItBreaksTheRule: "7 characters, with a digit", __pickleIndex: "88", exampleTags: []) | @F001 @AC-056 @FR-002 @FR-016 @US7-AS4 | passed | 00:00:00.3107303 |
| F002 | An Unpublished habit can be published again | @F002 @AC-022 @FR-011 @US3-AS5 | passed | 00:00:00.1038464 |
| F002 | Only the 24 habit icons are accepted(n: "05", icon: "`piggy-bank`", result: "success", __pickleIndex: "168", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0732380 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "06", name: "`A027 06 No intent`", description: "none", category: "Body", intent: "none", icon: "`book-open`", result: "\"Choose Build or Reduce.\"", __pickleIndex: "75", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0717632 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "01", name: "`\"   \"`", description: "none", category: "Body", intent: "Build", icon: "`book-open`", result: "\"Enter a name.\"", __pickleIndex: "70", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0734214 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "02", count: "2", marks: "both marked", result: "\"Mark one suggested starting goal for the card.\"", after: "no suggested starting goal", __pickleIndex: "110", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0784960 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"\"", password: "\"curator2026\"", __pickleIndex: "66", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0002130 |
| F002 | A user's list holds only their own custom habits, in name order ignoring case | @F002 @AC-048 @FR-004 @FR-020 @US4-AS2 | passed | 00:00:00.2304337 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "05", count: "5", marks: "the third marked", result: "success", after: "5 suggested starting goals, the third marked", __pickleIndex: "113", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0866307 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "05", action: "creates the custom habit `Admin habit`", __pickleIndex: "211", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1529452 |
| F002 | For custom habits the acting user and ownership come first, the name check last(n: "02", actingUser: "the registered user `ac057.b.02@example.com`", action: "edits `Secret` of `ac057.a.02@example.com` with th"···, result: "\"This custom habit does not exist.\"", __pickleIndex: "227", exampleTags: []) | @F002 @AC-057 @FR-005 @FR-016 @US4-AS11 | passed | 00:00:00.1496964 |
| F002 | The anonymous visitor cannot manage the catalog(n: "03", action: "creates the catalog habit `A031 03 New`", __pickleIndex: "96", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0743232 |
| F002 | Only the 24 habit icons are accepted(n: "22", icon: "`sprout`", result: "success", __pickleIndex: "185", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0729019 |
| F001 | Registration checks the password rule before whether the email is taken | @F001 @AC-008 @FR-003 @US1-AS6 | passed | 00:00:00.0369039 |
| F001 | Registration accepts a password at the lower bound of the password rule(email: "\"eight.pw@example.com\"", password: "\"walking4\"", whyItMeetsTheRule: "8 characters, with a digit", __pickleIndex: "5", exampleTags: []) | @F001 @AC-003 @FR-001 @FR-002 | passed | 00:00:00.0735019 |
| F002 | A user's custom-habit name is taken, ignoring case and spaces(n: "01", action: "creates a custom habit", __pickleIndex: "201", exampleTags: []) | @F002 @AC-051 @FR-004 @FR-005 @US4-AS5 | passed | 00:00:00.0911325 |
