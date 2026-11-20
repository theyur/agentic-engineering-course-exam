# AC results

## Current feature (F005)

Total: 284 · passed: 266 · failed: 18 · pending: 0 · skipped: 0

| Feature | Scenario | Tags | Outcome | Duration |
|---|---|---|---|---|
| F005 | A final yes/no commitment is Met on Yes for Build and on No for Reduce(n: "03", intent: "Reduce", value: "No", state: "Met", __pickleIndex: "64", exampleTags: []) | @F005 @AC-016 @FR-014 @US2-AS6 | passed | 00:00:00.0829406 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "03", measurement: "count 5 `pages`", before: "5", change: "the record of 2026-11-18 to 10000", result: "the result is \"Enter a whole number from 0 to 999"···, __pickleIndex: "227", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.0883686 |
| F005 | A final per-week Build count period counts only the records that reach the amount(n: "04", records: "20 for 2026-11-16, 20 for 2026-11-17 and 20 for 20"···, state: "Met", progress: "progress 3 of 3 times", __pickleIndex: "73", exampleTags: []) | @F005 @AC-018 @FR-014 @US2-AS8 | passed | 00:00:00.1000545 |
| F005 | Any begun date of a week names that week's one note(n: "02", action: "saves the note \"Week note.\" for 2026-11-16 and t"···, result: "both succeed and the history from 2026-11-16 to 20"···, __pickleIndex: "153", exampleTags: []) | @F005 @AC-039 @FR-002 @FR-007 @US4-AS5 | passed | 00:00:00.0884634 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "25", settings: "Reduce, count 3 `sweets`, 1 time per week", records: "1 for 2026-11-16 and 1 for 2026-11-18", state: "Met", progress: "progress total 2 of at most 3", __pickleIndex: "261", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0906772 |
| F005 | A per-week period holds one record per recording, listed in date order | @F005 @AC-003 @FR-003 @FR-009 @FR-014 @US1-AS3 | passed | 00:00:00.0971854 |
| F005 | An unrecorded commitment is Pending for 48 hours after its end and then Missed by itself(n: "03", time: "`2026-11-21T00:00:00Z`", entry: "Missed, no grace end, correction", __pickleIndex: "46", exampleTags: []) | @F005 @AC-013 @FR-010 @FR-016 @US2-AS2 @US2-AS3 | passed | 00:00:00.1179632 |
| F005 | An ended commitment or period stays Pending during its grace period, even when its records reach the target(n: "04", settings: "duration 20 minutes, fixed days on all seven days", value: "12", time: "`2026-11-20T23:59:59Z`", entry: "Pending, grace ends `2026-11-21T00:00:00Z`, progre"···, __pickleIndex: "89", exampleTags: []) | @F005 @AC-021 @FR-012 @US2-AS11 | passed | 00:00:00.0849763 |
| F005 | A note can be added to a Missed commitment and leaves it Missed | @F005 @AC-035 @FR-007 @US4-AS1 | passed | 00:00:00.0885822 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "06", settings: "Build, duration 20 minutes, 1 time per day", records: "19 for 2026-11-18", state: "Failed", progress: "progress 0 of 1 times", __pickleIndex: "242", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0838457 |
| F005 | A date that belongs to no commitment or period cannot be recorded(n: "04", schedule: "3 times per week", createdAt: "`2026-11-18T10:00:00Z`", startDate: "2026-11-18", date: "2026-11-18", why: "before the first week, 2026-11-23 to 2026-11-29", __pickleIndex: "7", exampleTags: []) | @F005 @AC-005 @FR-002 @US1-AS5 | passed | 00:00:00.0814582 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "09", measurement: "count 5 `pages`", before: "5", change: "the record of 2026-11-18 to the amount 7 together "···, result: "changing succeeds with the state Met, a correction"···, __pickleIndex: "233", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.0979039 |
| F005 | The first record of a Missed commitment that has a note: the note given is the note, no note keeps it(n: "03", action: "records 20 for 2026-11-18 with the note `\"   \"`", note: "no note", __pickleIndex: "281", exampleTags: []) | @F005 @AC-071 @FR-007 @US4-AS9 | passed | 00:00:00.0923802 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "03", settings: "Build, count 5 `pages`, fixed days on all seven da"···, records: "6 for 2026-11-18", state: "Met", progress: "progress 6 of 5", __pickleIndex: "239", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0873875 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "33", settings: "Build, duration 20 minutes, 3 times per week", records: "60 for 2026-11-16, 1 for 2026-11-17 and 59 for 202"···, state: "Failed", progress: "progress 2 of 3 times", __pickleIndex: "269", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0963944 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "20", settings: "Build, count 5 `pages`, 2 times per day", records: "6 and 6 for 2026-11-18", state: "Met", progress: "progress 2 of 2 times", __pickleIndex: "256", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0900152 |
| F005 | A final Build duration commitment is Met at or above its target and Failed below it(n: "03", duration: "duration 20 minutes", value: "21", state: "Met", progress: "progress 21 of 20 minutes", __pickleIndex: "50", exampleTags: []) | @F005 @AC-014 @FR-004 @FR-014 @US2-AS4 | passed | 00:00:00.0882553 |
| F005 | A final per-day Reduce count period is Met when its total is within the limit(n: "02", records: "1 for 2026-11-18 and 1 for 2026-11-18", state: "Failed", progress: "progress total 2 of at most 1", __pickleIndex: "76", exampleTags: []) | @F005 @AC-019 @FR-014 @US2-AS9 | passed | 00:00:00.0884369 |
| F005 | Recording checks the acting user, the goal, the date, the value and the note in that order(n: "06", actor: "`f5.ac010.owner.<n>@example.com`", date: "2026-11-18", value: "no value", note: "a text of 501 characters", result: "\"Enter a whole number from 0 to 9999.\"", __pickleIndex: "38", exampleTags: []) | @F005 @AC-010 @FR-005 @FR-018 @US1-AS7 | failed | 00:00:00.2158755 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "09", settings: "Build, duration 20 minutes, 3 times per week", records: "20 for 2026-11-16, 20 for 2026-11-17 and 20 for 20"···, state: "Met", progress: "progress 3 of 3 times", __pickleIndex: "245", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0941282 |
| F005 | A date that belongs to no commitment or period cannot be recorded(n: "03", schedule: "1 time per day", createdAt: "`2026-11-18T10:00:00Z`", startDate: "2026-11-18", date: "2026-11-17", why: "before the start date", __pickleIndex: "6", exampleTags: []) | @F005 @AC-005 @FR-002 @US1-AS5 | passed | 00:00:00.0839878 |
| F005 | A final per-day yes/no period counts its Yes records(n: "04", settings: "Build, yes/no, 2 times per day", records: "Yes for 2026-11-18 and Yes for 2026-11-18", state: "Met", progress: "progress 2 of 2 times", __pickleIndex: "82", exampleTags: []) | @F005 @AC-020 @FR-014 @US2-AS10 | passed | 00:00:00.0917188 |
| F005 | A goal whose catalog habit is Unpublished is still recorded and given its states | @F005 @AC-065 @FR-001 @FR-010 | passed | 00:00:00.2196013 |
| F005 | Progress is given at any state, as the goal's kind of target counts it(n: "03", settings: "Reduce, count 1 `sweet`, 1 time per day", records: "1 for 2026-11-18 and 1 for 2026-11-18", progress: "progress total 2 of at most 1", __pickleIndex: "169", exampleTags: []) | @F005 @AC-044 @FR-014 @US5-AS2 | passed | 00:00:00.0876416 |
| F005 | Demo history loading takes a demo password only with at least 8 characters and a digit(n: "04", demoPassword: "`exampl12`", why: "8 characters, with a digit", result: "its results are, in this order, \"created\", \"dem"···, __pickleIndex: "276", exampleTags: []) | @F005 @AC-069 @FR-030 @US7-AS6 | passed | 00:00:00.4138703 |
| F005 | A partial amount recorded for today is kept and the commitment is Due | @F005 @AC-001 @FR-001 @FR-004 @FR-006 @FR-009 @FR-010 @US1-AS1 | passed | 00:00:00.0867303 |
| F005 | A final per-week Build count period counts only the records that reach the amount(n: "05", records: "60 for 2026-11-16", state: "Failed", progress: "progress 1 of 3 times", __pickleIndex: "74", exampleTags: []) | @F005 @AC-018 @FR-014 @US2-AS8 | passed | 00:00:00.0881987 |
| F005 | A note has at most 500 characters(n: "05", action: "saves the note a padded text of 500 characters for"···, result: "saving the note succeeds and the history from 2026"···, __pickleIndex: "150", exampleTags: []) | @F005 @AC-038 @FR-005 @FR-007 @US4-AS4 | passed | 00:00:00.0887248 |
| F005 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit(n: "06", settings: "yes/no, fixed days on all seven days", records: "No for 2026-11-18", state: "Met", __pickleIndex: "98", exampleTags: []) | @F005 @AC-022 @FR-013 @US2-AS12 | passed | 00:00:00.0862287 |
| F005 | A record identifier that is not a record of the goal named in the call does not exist(n: "04", action: "deletes a record identifier that belongs to no rec"···, __pickleIndex: "124", exampleTags: []) | @F005 @AC-032 @FR-005 @FR-017 @US3-AS7 | passed | 00:00:00.1007328 |
| F005 | Any begun date of a week names that week's one note(n: "01", action: "saves the note \"Week note.\" for 2026-11-18", result: "saving the note succeeds and the history from 2026"···, __pickleIndex: "152", exampleTags: []) | @F005 @AC-039 @FR-002 @FR-007 @US4-AS5 | passed | 00:00:00.0980135 |
| F005 | What can be recorded now is every Due and every Pending commitment or period, newest first(n: "06", schedule: "3 times per week", startDate: "2026-11-16", time: "`2026-11-25T00:00:00Z`", entries: "exactly one entry: 2026-11-23 to 2026-11-29, Due", __pickleIndex: "179", exampleTags: []) | @F005 @AC-045 @FR-011 @US5-AS3 | passed | 00:00:00.0829742 |
| F005 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit(n: "04", settings: "duration 30 minutes, 1 time per day", records: "nothing", state: "Missed, no record", __pickleIndex: "96", exampleTags: []) | @F005 @AC-022 @FR-013 @US2-AS12 | passed | 00:00:00.0841484 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "21", settings: "Build, duration 20 minutes, 1 time per day", records: "21 for 2026-11-18", state: "Met", progress: "progress 1 of 1 times", __pickleIndex: "257", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0917661 |
| F005 | A demo goal is left out when its catalog habit is not Published, and the results stay the same(n: "01", habits: "a Published `Yoga`, a Draft `Reading` and an Unpub"···, goals: "exactly one goal, on the catalog habit `Yoga`, Bui"···, __pickleIndex: "217", exampleTags: []) | @F005 @AC-060 @FR-030 | passed | 00:00:00.4842979 |
| F005 | Demo history loading takes a demo password only with at least 8 characters and a digit(n: "03", demoPassword: "`abc`", why: "3 characters, no digit", result: "its result is \"demo password does not meet the pa"···, __pickleIndex: "275", exampleTags: []) | @F005 @AC-069 @FR-030 @US7-AS6 | passed | 00:00:00.3528232 |
| F005 | A commitment is Upcoming before its start instant and Due from it(n: "03", time: "`2026-11-19T00:00:00Z`", first: "Pending, grace ends `2026-11-21T00:00:00Z`", second: "Due, grace ends `2026-11-22T00:00:00Z`", __pickleIndex: "43", exampleTags: []) | @F005 @AC-012 @FR-010 @US2-AS1 | passed | 00:00:00.0901610 |
| F005 | Another registered user gets "This goal does not exist." from every use case of this slice(n: "09", useCase: "history", goal: "a goal identifier that belongs to no goal", __pickleIndex: "191", exampleTags: []) | @F005 @AC-047 @FR-017 @FR-018 @US6-AS1 | passed | 00:00:00.1620947 |
| F005 | Demo history loading's results name the demo email and no goal, habit, note, unit or amount | @F005 @AC-059 @FR-030 @US7-AS4 | passed | 00:00:00.3557341 |
| F005 | An administrator gets "Goals are for registered users." from every use case of this slice(n: "05", useCase: "history", __pickleIndex: "198", exampleTags: []) | @F005 @AC-049 @FR-018 @US6-AS3 | passed | 00:00:00.1548398 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "01", measurement: "count 5 `pages`", before: "5", change: "the record of 2026-11-18 to no value", result: "the result is \"Enter a whole number from 0 to 999"···, __pickleIndex: "225", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.0884016 |
| F005 | After a move east, the old and the new day can both be recorded while they overlap | @F005 @AC-067 @FR-002 @FR-011 @US5-AS3 | passed | 00:00:00.1347556 |
| F005 | A day that has not begun on the goal's timeline cannot be recorded(n: "05", timeZone: "UTC", schedule: "fixed days on all seven days", time: "`2026-11-19T00:00:00Z`", date: "2026-11-19", result: "recording succeeds with the state Due, not a corre"···, __pickleIndex: "13", exampleTags: []) | @F005 @AC-006 @FR-002 @US1-AS6 | passed | 00:00:00.0853810 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "08", settings: "Build, duration 20 minutes, 3 times per week", records: "20 for 2026-11-16, 19 for 2026-11-17 and 20 for 20"···, state: "Failed", progress: "progress 2 of 3 times", __pickleIndex: "244", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0922442 |
| F005 | Progress is given at any state, as the goal's kind of target counts it(n: "02", settings: "Build, count 20 `pages`, 3 times per week", records: "20 for 2026-11-16, 25 for 2026-11-17 and 12 for 20"···, progress: "progress 2 of 3 times", __pickleIndex: "168", exampleTags: []) | @F005 @AC-044 @FR-014 @US5-AS2 | passed | 00:00:00.0935055 |
| F005 | A value that does not fit the goal's measurement is refused with its message(n: "03", measurement: "count 5 `pages`", value: "-1", message: "\"Enter a whole number from 0 to 9999.\"", __pickleIndex: "19", exampleTags: []) | @F005 @AC-007 @FR-005 @US1-AS7 | passed | 00:00:00.0833710 |
| F005 | The lowest and highest amounts are accepted and kept(n: "05", measurement: "duration 1 in hours", value: "1440", __pickleIndex: "29", exampleTags: []) | @F005 @AC-008 @FR-004 @FR-005 | passed | 00:00:00.0884612 |
| F005 | The demo Yoga goal's weeks when R is a Sunday or a Tuesday(n: "01", runTime: "`2026-11-22T10:00:00Z`", r: "Sunday 2026-11-22", startDate: "2026-10-25", lastDate: "2026-11-22", entries: "4 entries: 2026-10-26 to 2026-11-01, Missed, no re"···, __pickleIndex: "271", exampleTags: []) | @F005 @AC-068 @FR-030 @US7-AS2 | passed | 00:00:00.4910476 |
| F005 | The lowest and highest amounts are accepted and kept(n: "04", measurement: "duration 20 minutes", value: "1440", __pickleIndex: "28", exampleTags: []) | @F005 @AC-008 @FR-004 @FR-005 | passed | 00:00:00.0858372 |
| F005 | The anonymous visitor gets "You are not signed in." from every use case of this slice(n: "05", useCase: "history", __pickleIndex: "205", exampleTags: []) | @F005 @AC-050 @FR-018 @US6-AS4 | passed | 00:00:00.0893813 |
| F005 | A record belongs to the week of its date, not to the week of the instant it is saved(n: "03", time: "`2026-11-23T00:00:00Z`", date: "2026-11-23", state: "Due", firstWeek: "no record", secondWeek: "records (2026-11-23, Yes)", __pickleIndex: "132", exampleTags: []) | @F005 @AC-034 @FR-001 @FR-009 | passed | 00:00:00.0854898 |
| F005 | A change is a correction only from the instant its grace period ends(n: "02", before: "nothing", time: "`2026-11-21T00:00:00Z`", action: "records 20 for 2026-11-18", result: "recording succeeds with the state Met, a correctio"···, __pickleIndex: "111", exampleTags: []) | @F005 @AC-027 @FR-006 | passed | 00:00:00.0846370 |
| F005 | A value that does not fit the goal's measurement is refused with its message(n: "01", measurement: "yes/no", value: "no value", message: "\"Choose Yes or No.\"", __pickleIndex: "17", exampleTags: []) | @F005 @AC-007 @FR-005 @US1-AS7 | passed | 00:00:00.0868920 |
| F005 | A final per-day yes/no period counts its Yes records(n: "02", settings: "Reduce, yes/no, 1 time per day", records: "Yes for 2026-11-18 and Yes for 2026-11-18", state: "Failed", progress: "progress 2 of at most 1", __pickleIndex: "80", exampleTags: []) | @F005 @AC-020 @FR-014 @US2-AS10 | passed | 00:00:00.0908352 |
| F005 | Progress is given at any state, as the goal's kind of target counts it(n: "07", settings: "Reduce, duration 30 minutes, 1 time per day", records: "10 and 15 for 2026-11-18", progress: "progress total 25 of at most 30", __pickleIndex: "173", exampleTags: []) | @F005 @AC-044 @FR-014 @US5-AS2 | passed | 00:00:00.0868310 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "11", measurement: "count 5 `pages`", before: "5", change: "the record of 2026-11-18 to -1 with the note a tex"···, result: "the result is \"Enter a whole number from 0 to 999"···, __pickleIndex: "235", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.0888951 |
| F005 | An account that already has the demo email gets no demo goals and keeps its password | @F005 @AC-058 @FR-030 @US7-AS3 | passed | 00:00:00.4344551 |
| F005 | A note saved on its own replaces the note, and an empty one removes it(n: "04", note: "\"  New note.  \"", result: "the note \"New note.\"", __pickleIndex: "137", exampleTags: []) | @F005 @AC-036 @FR-007 @US4-AS2 | passed | 00:00:00.0891215 |
| F005 | A value that does not belong to the goal's measurement is not stored(n: "02", measurement: "count 5 `pages`", given: "the amount 3 together with Yes", stored: "(2026-11-18, 3), with neither Yes nor No", __pickleIndex: "31", exampleTags: []) | @F005 @AC-009 @FR-004 | passed | 00:00:00.0860085 |
| F005 | A note saved on its own replaces the note, and an empty one removes it(n: "01", note: "`\"\"`", result: "no note", __pickleIndex: "134", exampleTags: []) | @F005 @AC-036 @FR-007 @US4-AS2 | passed | 00:00:00.0866657 |
| F005 | A final per-week Build count period counts only the records that reach the amount(n: "03", records: "19 for 2026-11-16, 19 for 2026-11-17 and 19 for 20"···, state: "Failed", progress: "progress 0 of 3 times", __pickleIndex: "72", exampleTags: []) | @F005 @AC-018 @FR-014 @US2-AS8 | passed | 00:00:00.0926155 |
| F005 | What can be recorded now is every Due and every Pending commitment or period, newest first(n: "04", schedule: "fixed days Monday, Wednesday and Friday", startDate: "2026-11-16", time: "`2026-11-21T10:00:00Z`", entries: "exactly one entry: 2026-11-20, Pending, grace ends"···, __pickleIndex: "177", exampleTags: []) | @F005 @AC-045 @FR-011 @US5-AS3 | passed | 00:00:00.0846155 |
| F005 | The demo goal "Yoga, 3 times a week" shows a Met, a Failed and a Missed week and a Due week when R is a Wednesday | @F005 @AC-054 @FR-030 @US7-AS2 | passed | 00:00:00.4306532 |
| F005 | The anonymous visitor gets "You are not signed in." from every use case of this slice(n: "07", useCase: "recent notes", __pickleIndex: "207", exampleTags: []) | @F005 @AC-050 @FR-018 @US6-AS4 | passed | 00:00:00.1023923 |
| F005 | A date that belongs to no commitment or period cannot be recorded(n: "02", schedule: "fixed days on all seven days", createdAt: "`2026-11-18T10:00:00Z`", startDate: "2026-11-18", date: "2026-11-17", why: "before the start date", __pickleIndex: "5", exampleTags: []) | @F005 @AC-005 @FR-002 @US1-AS5 | passed | 00:00:00.0845397 |
| F005 | Deleting the only record of a commitment keeps its note(n: "01", time: "`2026-11-18T12:00:00Z`", result: "deleting succeeds with the state Due, not a correc"···, state: "Due", __pickleIndex: "277", exampleTags: []) | @F005 @AC-070 @FR-007 @US4-AS8 | passed | 00:00:00.1130208 |
| F005 | An ended commitment or period stays Pending during its grace period, even when its records reach the target(n: "01", settings: "duration 20 minutes, fixed days on all seven days", value: "20", time: "`2026-11-19T00:00:00Z`", entry: "Pending, grace ends `2026-11-21T00:00:00Z`, progre"···, __pickleIndex: "86", exampleTags: []) | @F005 @AC-021 @FR-012 @US2-AS11 | passed | 00:00:00.0864674 |
| F005 | Another registered user gets "This goal does not exist." from every use case of this slice(n: "04", useCase: "save note", goal: "the goal of `f5.ac047.owner.<n>@example.com`", __pickleIndex: "186", exampleTags: []) | @F005 @AC-047 @FR-017 @FR-018 @US6-AS1 | failed | 00:00:00.1842183 |
| F005 | An administrator gets "Goals are for registered users." from every use case of this slice(n: "03", useCase: "delete, with the owner's record identifier", __pickleIndex: "196", exampleTags: []) | @F005 @AC-049 @FR-018 @US6-AS3 | passed | 00:00:00.1609068 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "10", measurement: "yes/no", before: "No", change: "the record of 2026-11-18 to Yes together with the "···, result: "changing succeeds with the state Met, a correction"···, __pickleIndex: "234", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.0935700 |
| F005 | The history's range has 1 to 366 days and is checked after the goal(n: "03", actor: "`f5.ac043.owner.<n>@example.com`", first: "2026-11-16", last: "2027-11-17", result: "the result is \"Choose a range of 1 to 366 days.\"", __pickleIndex: "163", exampleTags: []) | @F005 @AC-043 @FR-009 @FR-018 @US5-AS4 | failed | 00:00:00.1511730 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "28", settings: "Reduce, duration 60 minutes, 1 time per week", records: "30 for 2026-11-16 and 29 for 2026-11-18", state: "Met", progress: "progress total 59 of at most 60", __pickleIndex: "264", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0906049 |
| F005 | Another registered user gets "This goal does not exist." from every use case of this slice(n: "07", useCase: "recent notes", goal: "the goal of `f5.ac047.owner.<n>@example.com`", __pickleIndex: "189", exampleTags: []) | @F005 @AC-047 @FR-017 @FR-018 @US6-AS1 | failed | 00:00:00.1609855 |
| F005 | Another account's record identifier does not exist on one's own goal(n: "01", action: "changes the record to 12", __pickleIndex: "192", exampleTags: []) | @F005 @AC-048 @FR-017 @US6-AS2 | passed | 00:00:00.2360513 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "10", settings: "Reduce, duration 30 minutes, fixed days on all sev"···, records: "30 for 2026-11-18", state: "Met", progress: "progress 30 of 30", __pickleIndex: "246", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0881277 |
| F005 | Progress is given at any state, as the goal's kind of target counts it(n: "05", settings: "Build, duration 1 in hours, fixed days on all seve"···, records: "45 for 2026-11-18", progress: "progress 45 of 60 minutes", __pickleIndex: "171", exampleTags: []) | @F005 @AC-044 @FR-014 @US5-AS2 | passed | 00:00:00.0867367 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "13", settings: "Reduce, duration 30 minutes, 1 time per day", records: "20 and 11 for 2026-11-18", state: "Failed", progress: "progress total 31 of at most 30", __pickleIndex: "249", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0942635 |
| F005 | Demo history loading takes a demo password only with at least 8 characters and a digit(n: "01", demoPassword: "`exampl1`", why: "7 characters, with a digit", result: "its result is \"demo password does not meet the pa"···, __pickleIndex: "273", exampleTags: []) | @F005 @AC-069 @FR-030 @US7-AS6 | passed | 00:00:00.3595572 |
| F005 | A day that has not begun on the goal's timeline cannot be recorded(n: "01", timeZone: "UTC", schedule: "fixed days on all seven days", time: "`2026-11-18T10:00:00Z`", date: "2026-11-19", result: "the result is \"You cannot record a day that has n"···, __pickleIndex: "9", exampleTags: []) | @F005 @AC-006 @FR-002 @US1-AS6 | passed | 00:00:00.0851446 |
| F005 | A note given with a record is the note; no note leaves it unchanged(n: "03", action: "records Yes for 2026-11-18 with the note `\"\"`", note: "no note", __pickleIndex: "140", exampleTags: []) | @F005 @AC-037 @FR-007 @US4-AS3 | passed | 00:00:00.0936324 |
| F005 | The anonymous visitor gets "You are not signed in." from every use case of this slice(n: "03", useCase: "delete, with the owner's record identifier", __pickleIndex: "203", exampleTags: []) | @F005 @AC-050 @FR-018 @US6-AS4 | passed | 00:00:00.0937534 |
| F005 | Deleting the only record of a commitment keeps its note(n: "02", time: "`2026-11-22T10:00:00Z`", result: "deleting succeeds with the state Missed, a correct"···, state: "Missed", __pickleIndex: "278", exampleTags: []) | @F005 @AC-070 @FR-007 @US4-AS8 | passed | 00:00:00.1438842 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "02", settings: "Build, count 5 `pages`, fixed days on all seven da"···, records: "5 for 2026-11-18", state: "Met", progress: "progress 5 of 5", __pickleIndex: "238", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0877922 |
| F005 | A final yes/no commitment is Met on Yes for Build and on No for Reduce(n: "04", intent: "Reduce", value: "Yes", state: "Failed", __pickleIndex: "65", exampleTags: []) | @F005 @AC-016 @FR-014 @US2-AS6 | passed | 00:00:00.0867470 |
| F005 | An ended commitment or period stays Pending during its grace period, even when its records reach the target(n: "03", settings: "duration 20 minutes, fixed days on all seven days", value: "20", time: "`2026-11-21T00:00:00Z`", entry: "Met, no grace end, progress 20 of 20 minutes", __pickleIndex: "88", exampleTags: []) | @F005 @AC-021 @FR-012 @US2-AS11 | passed | 00:00:00.1078211 |
| F005 | A final Reduce count commitment is Met at or below its limit and Failed above it(n: "03", count: "count 2 `cups`", value: "2", state: "Met", __pickleIndex: "58", exampleTags: []) | @F005 @AC-015 @FR-014 @US2-AS5 | passed | 00:00:00.0858914 |
| F005 | An administrator gets "Goals are for registered users." from every use case of this slice(n: "01", useCase: "record", __pickleIndex: "194", exampleTags: []) | @F005 @AC-049 @FR-018 @US6-AS3 | passed | 00:00:00.1562896 |
| F005 | Any begun date of a week names that week's one note(n: "04", action: "saves the note \"Week note.\" for 2026-11-15", result: "the result is \"Nothing is scheduled on this date."···, __pickleIndex: "155", exampleTags: []) | @F005 @AC-039 @FR-002 @FR-007 @US4-AS5 | passed | 00:00:00.0858532 |
| F005 | A day that has not begun on the goal's timeline cannot be recorded(n: "07", timeZone: "Europe/Kyiv", schedule: "fixed days on all seven days", time: "`2026-11-18T22:00:00Z`", date: "2026-11-19", result: "recording succeeds with the state Due, not a corre"···, __pickleIndex: "15", exampleTags: []) | @F005 @AC-006 @FR-002 @US1-AS6 | passed | 00:00:00.1006255 |
| F005 | A change is a correction only from the instant its grace period ends(n: "06", before: "12 for 2026-11-18", time: "`2026-11-21T00:00:00Z`", action: "changes the record of 2026-11-18 to 20", result: "changing succeeds with the state Met, a correction", __pickleIndex: "115", exampleTags: []) | @F005 @AC-027 @FR-006 | passed | 00:00:00.0923784 |
| F005 | A final Reduce count commitment is Met at or below its limit and Failed above it(n: "04", count: "count 2 `cups`", value: "3", state: "Failed", __pickleIndex: "59", exampleTags: []) | @F005 @AC-015 @FR-014 @US2-AS5 | passed | 00:00:00.0839237 |
| F005 | An unrecorded commitment is Pending for 48 hours after its end and then Missed by itself(n: "01", time: "`2026-11-19T00:00:00Z`", entry: "Pending, grace ends `2026-11-21T00:00:00Z`, no cor"···, __pickleIndex: "44", exampleTags: []) | @F005 @AC-013 @FR-010 @FR-016 @US2-AS2 @US2-AS3 | passed | 00:00:00.1184738 |
| F005 | A day that has not begun on the goal's timeline cannot be recorded(n: "08", timeZone: "UTC", schedule: "3 times per week", time: "`2026-11-18T10:00:00Z`", date: "2026-11-20", result: "the result is \"You cannot record a day that has n"···, __pickleIndex: "16", exampleTags: []) | @F005 @AC-006 @FR-002 @US1-AS6 | passed | 00:00:00.0836158 |
| F005 | A note saved on its own replaces the note, and an empty one removes it(n: "03", note: "\"New note.\"", result: "the note \"New note.\"", __pickleIndex: "136", exampleTags: []) | @F005 @AC-036 @FR-007 @US4-AS2 | passed | 00:00:00.0893839 |
| F005 | An ended commitment or period stays Pending during its grace period, even when its records reach the target(n: "05", settings: "duration 20 minutes, fixed days on all seven days", value: "12", time: "`2026-11-21T00:00:00Z`", entry: "Failed, no grace end, progress 12 of 20 minutes", __pickleIndex: "90", exampleTags: []) | @F005 @AC-021 @FR-012 @US2-AS11 | passed | 00:00:00.0868495 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "07", measurement: "count 5 `pages`", before: "5", change: "a record identifier that belongs to no record to n"···, result: "the result is \"This record does not exist.\" (the"···, __pickleIndex: "231", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.0867898 |
| F005 | Results corrected after the grace period equal the same records entered on time | @F005 @AC-029 @FR-006 @FR-015 @US3-AS5 | passed | 00:00:00.1172617 |
| F005 | Any begun date of a week names that week's one note(n: "06", action: "saves the note a text of 501 characters for 2026-1"···, result: "the result is \"Nothing is scheduled on this date."···, __pickleIndex: "157", exampleTags: []) | @F005 @AC-039 @FR-002 @FR-007 @US4-AS5 | passed | 00:00:00.0843489 |
| F005 | A commitment in the old time zone becomes final, and is corrected, at the end of its own grace period(n: "02", time: "`2026-11-19T22:00:00Z`", state: "Met", correction: "a correction", __pickleIndex: "120", exampleTags: []) | @F005 @AC-031 @FR-006 @FR-015 @US3-AS6 | passed | 00:00:00.1313361 |
| F005 | A day that has not begun on the goal's timeline cannot be recorded(n: "06", timeZone: "Europe/Kyiv", schedule: "fixed days on all seven days", time: "`2026-11-18T21:59:59Z`", date: "2026-11-19", result: "the result is \"You cannot record a day that has n"···, __pickleIndex: "14", exampleTags: []) | @F005 @AC-006 @FR-002 @US1-AS6 | passed | 00:00:00.0824100 |
| F005 | The first record of a Missed commitment that has a note: the note given is the note, no note keeps it(n: "04", action: "records 20 for 2026-11-18", note: "note \"Away for the weekend.\"", __pickleIndex: "282", exampleTags: []) | @F005 @AC-071 @FR-007 @US4-AS9 | passed | 00:00:00.0871463 |
| F005 | Demo history loading run again leaves the existing demo account with history unchanged | @F005 @AC-057 @FR-030 @US7-AS3 | passed | 00:00:00.4403030 |
| F005 | A change is a correction only from the instant its grace period ends(n: "01", before: "nothing", time: "`2026-11-20T23:59:59Z`", action: "records 20 for 2026-11-18", result: "recording succeeds with the state Pending, not a c"···, __pickleIndex: "110", exampleTags: []) | @F005 @AC-027 @FR-006 | passed | 00:00:00.0993354 |
| F005 | The history's range has 1 to 366 days and is checked after the goal(n: "05", actor: "`f5.ac043.other.<n>@example.com`", first: "2026-11-16", last: "2027-11-17", result: "the result is \"This goal does not exist.\"", __pickleIndex: "165", exampleTags: []) | @F005 @AC-043 @FR-009 @FR-018 @US5-AS4 | failed | 00:00:00.2668988 |
| F005 | A final per-day yes/no period counts its Yes records(n: "06", settings: "Build, yes/no, 2 times per day", records: "Yes for 2026-11-18 and No for 2026-11-18", state: "Failed", progress: "progress 1 of 2 times", __pickleIndex: "84", exampleTags: []) | @F005 @AC-020 @FR-014 @US2-AS10 | passed | 00:00:00.0911404 |
| F005 | A final per-day Reduce count period is Met when its total is within the limit(n: "01", records: "0 for 2026-11-18 and 1 for 2026-11-18", state: "Met", progress: "progress total 1 of at most 1", __pickleIndex: "75", exampleTags: []) | @F005 @AC-019 @FR-014 @US2-AS9 | passed | 00:00:00.0876440 |
| F005 | A note given with a record is trimmed | @F005 @AC-011 @FR-007 @US1-AS8 | passed | 00:00:00.1211954 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "32", settings: "Build, duration 20 minutes, 3 times per week", records: "60 for 2026-11-16", state: "Failed", progress: "progress 1 of 3 times", __pickleIndex: "268", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0868749 |
| F005 | A note given with a record is the note; no note leaves it unchanged(n: "07", action: "changes the record of 2026-11-16 to No with the no"···, note: "no note", __pickleIndex: "144", exampleTags: []) | @F005 @AC-037 @FR-007 @US4-AS3 | passed | 00:00:00.1268242 |
| F005 | Around a clock change the grace period lasts exactly 48 elapsed hours(n: "02", createdAt: "`2026-10-24T08:00:00Z`", date: "2026-10-24", time: "`2026-10-26T21:00:00Z`", start: "`2026-10-23T21:00:00Z`", end: "`2026-10-24T21:00:00Z`", entry: "Missed, no grace end", __pickleIndex: "103", exampleTags: []) | @F005 @AC-023 @FR-016 @US2-AS13 | passed | 00:00:00.0836463 |
| F005 | A commitment is Upcoming before its start instant and Due from it(n: "02", time: "`2026-11-18T23:59:59Z`", first: "Due, grace ends `2026-11-21T00:00:00Z`", second: "Upcoming, no grace end", __pickleIndex: "42", exampleTags: []) | @F005 @AC-012 @FR-010 @US2-AS1 | passed | 00:00:00.0824729 |
| F005 | A final per-day yes/no period counts its Yes records(n: "03", settings: "Reduce, yes/no, 1 time per day", records: "No for 2026-11-18", state: "Met", progress: "progress 0 of at most 1", __pickleIndex: "81", exampleTags: []) | @F005 @AC-020 @FR-014 @US2-AS10 | passed | 00:00:00.0900394 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "15", settings: "Reduce, count 3 `sweets`, 1 time per week", records: "2 for 2026-11-16 and 2 for 2026-11-18", state: "Failed", progress: "progress total 4 of at most 3", __pickleIndex: "251", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0887178 |
| F005 | Another registered user gets "This goal does not exist." from every use case of this slice(n: "02", useCase: "change, with the owner's record identifier", goal: "the goal of `f5.ac047.owner.<n>@example.com`", __pickleIndex: "184", exampleTags: []) | @F005 @AC-047 @FR-017 @FR-018 @US6-AS1 | failed | 00:00:00.1574848 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "14", settings: "Reduce, count 3 `sweets`, 1 time per week", records: "1 for 2026-11-16 and 2 for 2026-11-18", state: "Met", progress: "progress total 3 of at most 3", __pickleIndex: "250", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0946702 |
| F005 | A final Reduce count commitment is Met at or below its limit and Failed above it(n: "02", count: "count 2 `cups`", value: "1", state: "Met", __pickleIndex: "57", exampleTags: []) | @F005 @AC-015 @FR-014 @US2-AS5 | passed | 00:00:00.0835074 |
| F005 | A note has at most 500 characters(n: "06", action: "records 20 for 2026-11-18 with the note a padded t"···, result: "recording succeeds with the state Due, not a corre"···, __pickleIndex: "151", exampleTags: []) | @F005 @AC-038 @FR-005 @FR-007 @US4-AS4 | passed | 00:00:00.0897025 |
| F005 | The anonymous visitor gets "You are not signed in." from every use case of this slice(n: "06", useCase: "recordable now", __pickleIndex: "206", exampleTags: []) | @F005 @AC-050 @FR-018 @US6-AS4 | passed | 00:00:00.0902920 |
| F005 | A change is a correction only from the instant its grace period ends(n: "05", before: "12 for 2026-11-18", time: "`2026-11-20T23:59:59Z`", action: "changes the record of 2026-11-18 to 20", result: "changing succeeds with the state Pending, not a co"···, __pickleIndex: "114", exampleTags: []) | @F005 @AC-027 @FR-006 | passed | 00:00:00.0856179 |
| F005 | When R is a Monday, the demo Yoga goal's last full week is Pending and it has a week before week 3 | @F005 @AC-055 @FR-030 @US7-AS2 | passed | 00:00:00.5878868 |
| F005 | A day that has not begun on the goal's timeline cannot be recorded(n: "04", timeZone: "UTC", schedule: "fixed days on all seven days", time: "`2026-11-18T23:59:59Z`", date: "2026-11-19", result: "the result is \"You cannot record a day that has n"···, __pickleIndex: "12", exampleTags: []) | @F005 @AC-006 @FR-002 @US1-AS6 | passed | 00:00:00.0831270 |
| F005 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit(n: "07", settings: "yes/no, 1 time per day", records: "No for 2026-11-18", state: "Met", __pickleIndex: "99", exampleTags: []) | @F005 @AC-022 @FR-013 @US2-AS12 | passed | 00:00:00.0867161 |
| F005 | Recording checks the acting user, the goal, the date, the value and the note in that order(n: "03", actor: "`f5.ac010.other.<n>@example.com`", date: "2026-11-17", value: "no value", note: "a text of 501 characters", result: "\"This goal does not exist.\"", __pickleIndex: "35", exampleTags: []) | @F005 @AC-010 @FR-005 @FR-018 @US1-AS7 | failed | 00:00:00.2210588 |
| F005 | Lowering the record of a Met commitment below its target is a correction that makes it Failed(n: "01", action: "changes the record of 2026-11-18 to 12", result: "changing", __pickleIndex: "107", exampleTags: []) | @F005 @AC-025 @FR-006 @US3-AS2 | passed | 00:00:00.0948362 |
| F005 | Records on the same date of a per-day period keep the order in which they were stored | @F005 @AC-004 @FR-003 @FR-009 @FR-014 @US1-AS4 | passed | 00:00:00.0901665 |
| F005 | An administrator gets "Goals are for registered users." from every use case of this slice(n: "06", useCase: "recordable now", __pickleIndex: "199", exampleTags: []) | @F005 @AC-049 @FR-018 @US6-AS3 | passed | 00:00:00.1577205 |
| F005 | A note given with a record is the note; no note leaves it unchanged(n: "01", action: "records Yes for 2026-11-18", note: "the note \"Knee was sore.\"", __pickleIndex: "138", exampleTags: []) | @F005 @AC-037 @FR-007 @US4-AS3 | passed | 00:00:00.0954275 |
| F005 | Another account's record identifier does not exist on one's own goal(n: "02", action: "deletes the record", __pickleIndex: "193", exampleTags: []) | @F005 @AC-048 @FR-017 @US6-AS2 | passed | 00:00:00.3074014 |
| F005 | Deleting the only record of a final commitment is a correction that makes it Missed | @F005 @AC-026 @FR-006 @US3-AS3 | passed | 00:00:00.2472676 |
| F005 | A commitment is Upcoming before its start instant and Due from it(n: "01", time: "`2026-11-18T10:00:00Z`", first: "Due, grace ends `2026-11-21T00:00:00Z`", second: "from `2026-11-19T00:00:00Z` to `2026-11-20T00:00:0"···, __pickleIndex: "41", exampleTags: []) | @F005 @AC-012 @FR-010 @US2-AS1 | passed | 00:00:00.0827785 |
| F005 | An unrecorded commitment is Pending for 48 hours after its end and then Missed by itself(n: "02", time: "`2026-11-20T23:59:59Z`", entry: "Pending, grace ends `2026-11-21T00:00:00Z`, no cor"···, __pickleIndex: "45", exampleTags: []) | @F005 @AC-013 @FR-010 @FR-016 @US2-AS2 @US2-AS3 | passed | 00:00:00.1138421 |
| F005 | A final Build duration commitment is Met at or above its target and Failed below it(n: "07", duration: "duration 1 in hours", value: "60", state: "Met", progress: "progress 60 of 60 minutes", __pickleIndex: "54", exampleTags: []) | @F005 @AC-014 @FR-004 @FR-014 @US2-AS4 | passed | 00:00:00.0923916 |
| F005 | The demo goals' recent notes give the demo notes with their states | @F005 @AC-056 @FR-008 @FR-030 @US7-AS2 | passed | 00:00:00.4410344 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "22", settings: "Build, duration 20 minutes, 3 times per week", records: "21 for 2026-11-16, 21 for 2026-11-17 and 21 for 20"···, state: "Met", progress: "progress 3 of 3 times", __pickleIndex: "258", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0930845 |
| F005 | A commitment in the old time zone becomes final, and is corrected, at the end of its own grace period(n: "01", time: "`2026-11-19T21:59:59Z`", state: "Pending", correction: "not a correction", __pickleIndex: "119", exampleTags: []) | @F005 @AC-031 @FR-006 @FR-015 @US3-AS6 | passed | 00:00:00.1442922 |
| F005 | Recording checks the acting user, the goal, the date, the value and the note in that order(n: "02", actor: "the administrator `f5.ac010.admin.<n>@example.com`", date: "2026-11-17", value: "no value", note: "a text of 501 characters", result: "\"Goals are for registered users.\"", __pickleIndex: "34", exampleTags: []) | @F005 @AC-010 @FR-005 @FR-018 @US1-AS7 | failed | 00:00:00.2221858 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "27", settings: "Reduce, yes/no, 2 times per day", records: "Yes and No for 2026-11-18", state: "Met", progress: "progress 1 of at most 2", __pickleIndex: "263", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1065978 |
| F005 | A final per-week Build yes/no period is Met with at least N Yes records(n: "04", records: "Yes for 2026-11-16 and Yes for 2026-11-18", state: "Failed", progress: "progress 2 of 3 times", __pickleIndex: "69", exampleTags: []) | @F005 @AC-017 @FR-014 @US2-AS7 | passed | 00:00:00.0873426 |
| F005 | A note has at most 500 characters(n: "02", action: "saves the note a text of 500 characters for 2026-1"···, result: "saving the note succeeds and the history from 2026"···, __pickleIndex: "147", exampleTags: []) | @F005 @AC-038 @FR-005 @FR-007 @US4-AS4 | passed | 00:00:00.0922427 |
| F005 | A record identifier that is not a record of the goal named in the call does not exist(n: "02", action: "deletes the second goal's record of 2026-11-18", __pickleIndex: "122", exampleTags: []) | @F005 @AC-032 @FR-005 @FR-017 @US3-AS7 | passed | 00:00:00.0945408 |
| F005 | A value that does not fit the goal's measurement is refused with its message(n: "05", measurement: "duration 20 minutes", value: "no value", message: "\"Enter a whole number of minutes from 0 to 1440.\"···, __pickleIndex: "21", exampleTags: []) | @F005 @AC-007 @FR-005 @US1-AS7 | passed | 00:00:00.0850733 |
| F005 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit(n: "05", settings: "count 0 `sweets`, fixed days on all seven days", records: "nothing", state: "Missed, no record", __pickleIndex: "97", exampleTags: []) | @F005 @AC-022 @FR-013 @US2-AS12 | passed | 00:00:00.0824809 |
| F005 | A final per-day Reduce count period is Met when its total is within the limit(n: "03", records: "0 for 2026-11-18", state: "Met", progress: "progress total 0 of at most 1", __pickleIndex: "77", exampleTags: []) | @F005 @AC-019 @FR-014 @US2-AS9 | passed | 00:00:00.0862388 |
| F005 | A record identifier that is not a record of the goal named in the call does not exist(n: "01", action: "changes the second goal's record of 2026-11-18 to "···, __pickleIndex: "121", exampleTags: []) | @F005 @AC-032 @FR-005 @FR-017 @US3-AS7 | passed | 00:00:00.2911605 |
| F005 | A final per-day yes/no period counts its Yes records(n: "05", settings: "Build, yes/no, 2 times per day", records: "Yes for 2026-11-18", state: "Failed", progress: "progress 1 of 2 times", __pickleIndex: "83", exampleTags: []) | @F005 @AC-020 @FR-014 @US2-AS10 | passed | 00:00:00.0856116 |
| F005 | A final Build duration commitment is Met at or above its target and Failed below it(n: "02", duration: "duration 20 minutes", value: "20", state: "Met", progress: "progress 20 of 20 minutes", __pickleIndex: "49", exampleTags: []) | @F005 @AC-014 @FR-004 @FR-014 @US2-AS4 | passed | 00:00:00.0935570 |
| F005 | Any begun date of a week names that week's one note(n: "05", action: "saves the note a text of 501 characters for 2026-1"···, result: "the result is \"You cannot record a day that has n"···, __pickleIndex: "156", exampleTags: []) | @F005 @AC-039 @FR-002 @FR-007 @US4-AS5 | passed | 00:00:00.0854707 |
| F005 | A final per-week Build count period counts only the records that reach the amount(n: "01", records: "20 for 2026-11-16, 25 for 2026-11-17 and 12 for 20"···, state: "Failed", progress: "progress 2 of 3 times", __pickleIndex: "70", exampleTags: []) | @F005 @AC-018 @FR-014 @US2-AS8 | passed | 00:00:00.0935123 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "04", measurement: "duration 20 minutes", before: "20", change: "the record of 2026-11-18 to 1441", result: "the result is \"Enter a whole number of minutes fr"···, __pickleIndex: "228", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.0860529 |
| F005 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit(n: "09", settings: "count 0 `sweets`, fixed days on all seven days", records: "0 for 2026-11-18", state: "Met", __pickleIndex: "101", exampleTags: []) | @F005 @AC-022 @FR-013 @US2-AS12 | passed | 00:00:00.0838053 |
| F005 | A final per-week Build count period counts only the records that reach the amount(n: "02", records: "20 for 2026-11-16, 25 for 2026-11-17, 12 for 2026-"···, state: "Met", progress: "progress 3 of 3 times", __pickleIndex: "71", exampleTags: []) | @F005 @AC-018 @FR-014 @US2-AS8 | passed | 00:00:00.0936305 |
| F005 | A final per-week Build yes/no period is Met with at least N Yes records(n: "03", records: "Yes for 2026-11-16, Yes for 2026-11-17, Yes for 20"···, state: "Met", progress: "progress 4 of 3 times", __pickleIndex: "68", exampleTags: []) | @F005 @AC-017 @FR-014 @US2-AS7 | passed | 00:00:00.0908383 |
| F005 | An administrator gets "Goals are for registered users." from every use case of this slice(n: "07", useCase: "recent notes", __pickleIndex: "200", exampleTags: []) | @F005 @AC-049 @FR-018 @US6-AS3 | passed | 00:00:00.1589179 |
| F005 | After a time zone change, each commitment keeps the instants and grace period of its own settings version | @F005 @AC-030 @FR-015 @FR-016 | passed | 00:00:00.1252282 |
| F005 | A note given with a record is the note; no note leaves it unchanged(n: "08", action: "changes the record of 2026-11-16 to No with the no"···, note: "the note \"Knee was sore.\"", __pickleIndex: "145", exampleTags: []) | @F005 @AC-037 @FR-007 @US4-AS3 | passed | 00:00:00.0905400 |
| F005 | The first record of a Missed commitment that has a note: the note given is the note, no note keeps it(n: "01", action: "records 20 for 2026-11-18 with the note \"Away for"···, note: "note \"Away for the weekend.\"", __pickleIndex: "279", exampleTags: []) | @F005 @AC-071 @FR-007 @US4-AS9 | passed | 00:00:00.0904674 |
| F005 | A final Build duration commitment is Met at or above its target and Failed below it(n: "01", duration: "duration 20 minutes", value: "19", state: "Failed", progress: "progress 19 of 20 minutes", __pickleIndex: "48", exampleTags: []) | @F005 @AC-014 @FR-004 @FR-014 @US2-AS4 | passed | 00:00:00.0879907 |
| F005 | Changing the record of a commitment without a note, with a note, adds the note | @F005 @AC-072 @FR-007 @US4-AS10 | passed | 00:00:00.0956143 |
| F005 | Demo history loading without a demo password creates no account(n: "03", demoPassword: "`\"   \"`", __pickleIndex: "222", exampleTags: []) | @F005 @AC-061 @FR-030 @US7-AS6 | passed | 00:00:00.3670983 |
| F005 | A final per-day Reduce count period is Met when its total is within the limit(n: "04", records: "2 for 2026-11-18", state: "Failed", progress: "progress total 2 of at most 1", __pickleIndex: "78", exampleTags: []) | @F005 @AC-019 @FR-014 @US2-AS9 | passed | 00:00:00.0850688 |
| F005 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit(n: "03", settings: "count 1 `sweet`, 1 time per day", records: "nothing", state: "Missed, no record", __pickleIndex: "95", exampleTags: []) | @F005 @AC-022 @FR-013 @US2-AS12 | passed | 00:00:00.0833055 |
| F005 | Another registered user gets "This goal does not exist." from every use case of this slice(n: "01", useCase: "record", goal: "the goal of `f5.ac047.owner.<n>@example.com`", __pickleIndex: "183", exampleTags: []) | @F005 @AC-047 @FR-017 @FR-018 @US6-AS1 | failed | 00:00:00.1618478 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "11", settings: "Reduce, duration 30 minutes, fixed days on all sev"···, records: "31 for 2026-11-18", state: "Failed", progress: "progress 31 of 30", __pickleIndex: "247", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0907752 |
| F005 | Progress is given at any state, as the goal's kind of target counts it(n: "06", settings: "Build, yes/no, 2 times per day", records: "Yes and No for 2026-11-18", progress: "progress 1 of 2 times", __pickleIndex: "172", exampleTags: []) | @F005 @AC-044 @FR-014 @US5-AS2 | passed | 00:00:00.0887680 |
| F005 | A value that does not fit the goal's measurement is refused with its message(n: "06", measurement: "duration 20 minutes", value: "-1", message: "\"Enter a whole number of minutes from 0 to 1440.\"···, __pickleIndex: "22", exampleTags: []) | @F005 @AC-007 @FR-005 @US1-AS7 | passed | 00:00:00.0824896 |
| F005 | The history's range has 1 to 366 days and is checked after the goal(n: "02", actor: "`f5.ac043.owner.<n>@example.com`", first: "2026-11-16", last: "2027-11-16", result: "it gives 364 entries, the first 2026-11-18 and the"···, __pickleIndex: "162", exampleTags: []) | @F005 @AC-043 @FR-009 @FR-018 @US5-AS4 | failed | 00:00:00.1520583 |
| F005 | Recording checks the acting user, the goal, the date, the value and the note in that order(n: "01", actor: "the anonymous visitor", date: "2026-11-17", value: "no value", note: "a text of 501 characters", result: "\"You are not signed in.\"", __pickleIndex: "33", exampleTags: []) | @F005 @AC-010 @FR-005 @FR-018 @US1-AS7 | passed | 00:00:00.2205906 |
| F005 | A final Reduce count commitment is Met at or below its limit and Failed above it(n: "05", count: "count 0 `cigarettes`", value: "0", state: "Met", __pickleIndex: "60", exampleTags: []) | @F005 @AC-015 @FR-014 @US2-AS5 | passed | 00:00:00.0841791 |
| F005 | An evaluation period takes at most 50 records, but its records can still be changed or deleted(n: "03", action: "records no value for 2026-11-18", result: "the result is \"Choose Yes or No.\" and the histor"···, __pickleIndex: "127", exampleTags: []) | @F005 @AC-033 @FR-003 @FR-005 | passed | 00:00:00.1654622 |
| F005 | Another registered user gets "This goal does not exist." from every use case of this slice(n: "05", useCase: "history", goal: "the goal of `f5.ac047.owner.<n>@example.com`", __pickleIndex: "187", exampleTags: []) | @F005 @AC-047 @FR-017 @FR-018 @US6-AS1 | failed | 00:00:00.1597908 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "06", measurement: "count 5 `pages`", before: "5", change: "the record of 2026-11-18 to 3 with the note a text"···, result: "the result is \"The note can have at most 500 char"···, __pickleIndex: "230", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.0878071 |
| F005 | The anonymous visitor gets "You are not signed in." from every use case of this slice(n: "04", useCase: "save note", __pickleIndex: "204", exampleTags: []) | @F005 @AC-050 @FR-018 @US6-AS4 | passed | 00:00:00.0909718 |
| F005 | A date that belongs to no commitment or period cannot be recorded(n: "05", schedule: "3 times per week", createdAt: "`2026-11-18T10:00:00Z`", startDate: "2026-11-18", date: "2026-11-22", why: "before the first week, and not begun: the date che"···, __pickleIndex: "8", exampleTags: []) | @F005 @AC-005 @FR-002 @US1-AS5 | passed | 00:00:00.0857289 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "01", settings: "Build, count 5 `pages`, fixed days on all seven da"···, records: "4 for 2026-11-18", state: "Failed", progress: "progress 4 of 5", __pickleIndex: "237", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0852478 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "05", measurement: "yes/no", before: "Yes", change: "the record of 2026-11-18 to no value", result: "the result is \"Choose Yes or No.\" and the histor"···, __pickleIndex: "229", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.0875575 |
| F005 | A value that does not fit the goal's measurement is refused with its message(n: "08", measurement: "duration 1 in hours", value: "1441", message: "\"Enter a whole number of minutes from 0 to 1440.\"···, __pickleIndex: "24", exampleTags: []) | @F005 @AC-007 @FR-005 @US1-AS7 | passed | 00:00:00.0852054 |
| F005 | A final per-day yes/no period counts its Yes records(n: "01", settings: "Reduce, yes/no, 1 time per day", records: "Yes for 2026-11-18", state: "Met", progress: "progress 1 of at most 1", __pickleIndex: "79", exampleTags: []) | @F005 @AC-020 @FR-014 @US2-AS10 | passed | 00:00:00.0845703 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "19", settings: "Reduce, yes/no, 2 times per day", records: "Yes, Yes and Yes for 2026-11-18", state: "Failed", progress: "progress 3 of at most 2", __pickleIndex: "255", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0925803 |
| F005 | A note has at most 500 characters(n: "01", action: "saves the note a text of 501 characters for 2026-1"···, result: "the result is \"The note can have at most 500 char"···, __pickleIndex: "146", exampleTags: []) | @F005 @AC-038 @FR-005 @FR-007 @US4-AS4 | passed | 00:00:00.0859877 |
| F005 | An evaluation period takes at most 50 records, but its records can still be changed or deleted(n: "01", action: "records Yes for 2026-11-18", result: "the result is \"This evaluation period already has"···, __pickleIndex: "125", exampleTags: []) | @F005 @AC-033 @FR-003 @FR-005 | passed | 00:00:00.1717445 |
| F005 | After a move east, the old and the new day are both Due while they overlap(n: "02", time: "`2026-11-19T05:00:00Z`", entries: "2026-11-19, Due, grace ends `2026-11-21T22:00:00Z`"···, __pickleIndex: "182", exampleTags: []) | @F005 @AC-046 @FR-011 @US5-AS3 | passed | 00:00:00.2027832 |
| F005 | A final Reduce count commitment is Met at or below its limit and Failed above it(n: "06", count: "count 0 `cigarettes`", value: "1", state: "Failed", __pickleIndex: "61", exampleTags: []) | @F005 @AC-015 @FR-014 @US2-AS5 | passed | 00:00:00.0848569 |
| F005 | A value that does not fit the goal's measurement is refused with its message(n: "07", measurement: "duration 20 minutes", value: "1441", message: "\"Enter a whole number of minutes from 0 to 1440.\"···, __pickleIndex: "23", exampleTags: []) | @F005 @AC-007 @FR-005 @US1-AS7 | passed | 00:00:00.0832739 |
| F005 | An ended commitment or period stays Pending during its grace period, even when its records reach the target(n: "06", settings: "yes/no, 1 time per day", value: "Yes", time: "`2026-11-20T23:59:59Z`", entry: "Pending, grace ends `2026-11-21T00:00:00Z`, progre"···, __pickleIndex: "91", exampleTags: []) | @F005 @AC-021 @FR-012 @US2-AS11 | passed | 00:00:00.0852439 |
| F005 | A record belongs to the week of its date, not to the week of the instant it is saved(n: "02", time: "`2026-11-23T00:00:00Z`", date: "2026-11-22", state: "Pending", firstWeek: "records (2026-11-22, Yes)", secondWeek: "no record", __pickleIndex: "131", exampleTags: []) | @F005 @AC-034 @FR-001 @FR-009 | passed | 00:00:00.0854924 |
| F005 | Around a clock change the grace period lasts exactly 48 elapsed hours(n: "01", createdAt: "`2026-10-24T08:00:00Z`", date: "2026-10-24", time: "`2026-10-26T20:59:59Z`", start: "`2026-10-23T21:00:00Z`", end: "`2026-10-24T21:00:00Z`", entry: "Pending, grace ends `2026-10-26T21:00:00Z` (23:00 "···, __pickleIndex: "102", exampleTags: []) | @F005 @AC-023 @FR-016 @US2-AS13 | passed | 00:00:00.1170440 |
| F005 | After a move east, the old and the new day are both Due while they overlap(n: "01", time: "`2026-11-18T23:00:00Z`", entries: "2026-11-19 from `2026-11-18T22:00:00Z` to `2026-11"···, __pickleIndex: "181", exampleTags: []) | @F005 @AC-046 @FR-011 @US5-AS3 | passed | 00:00:00.1390769 |
| F005 | An ended commitment or period stays Pending during its grace period, even when its records reach the target(n: "07", settings: "yes/no, 1 time per day", value: "Yes", time: "`2026-11-21T00:00:00Z`", entry: "Met, no grace end, progress 1 of 1 times", __pickleIndex: "92", exampleTags: []) | @F005 @AC-021 @FR-012 @US2-AS11 | passed | 00:00:00.0874387 |
| F005 | A change is a correction only from the instant its grace period ends(n: "04", before: "12 for 2026-11-18", time: "`2026-11-20T23:59:59Z`", action: "deletes the record of 2026-11-18", result: "deleting succeeds with the state Pending, not a co"···, __pickleIndex: "113", exampleTags: []) | @F005 @AC-027 @FR-006 | passed | 00:00:00.0907439 |
| F005 | A value that does not fit the goal's measurement is refused with its message(n: "04", measurement: "count 5 `pages`", value: "10000", message: "\"Enter a whole number from 0 to 9999.\"", __pickleIndex: "20", exampleTags: []) | @F005 @AC-007 @FR-005 @US1-AS7 | passed | 00:00:00.0897322 |
| F005 | Demo history loading without a demo password creates no account(n: "02", demoPassword: "`\"\"`", __pickleIndex: "221", exampleTags: []) | @F005 @AC-061 @FR-030 @US7-AS6 | passed | 00:00:00.3676276 |
| F005 | A note given with a record is the note; no note leaves it unchanged(n: "06", action: "changes the record of 2026-11-16 to No with the no"···, note: "the note \"Better today.\"", __pickleIndex: "143", exampleTags: []) | @F005 @AC-037 @FR-007 @US4-AS3 | passed | 00:00:00.0950347 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "16", settings: "Reduce, yes/no, 2 times per week", records: "Yes for 2026-11-16, Yes for 2026-11-17 and No for "···, state: "Met", progress: "progress 2 of at most 2", __pickleIndex: "252", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0908614 |
| F005 | A final yes/no commitment is Met on Yes for Build and on No for Reduce(n: "02", intent: "Build", value: "No", state: "Failed", __pickleIndex: "63", exampleTags: []) | @F005 @AC-016 @FR-014 @US2-AS6 | passed | 00:00:00.0870824 |
| F005 | A value that does not belong to the goal's measurement is not stored(n: "01", measurement: "yes/no", given: "Yes together with the amount 5", stored: "(2026-11-18, Yes), with no amount", __pickleIndex: "30", exampleTags: []) | @F005 @AC-009 @FR-004 | passed | 00:00:00.0850117 |
| F005 | A final Build duration commitment is Met at or above its target and Failed below it(n: "05", duration: "duration 20 minutes", value: "0", state: "Failed", progress: "progress 0 of 20 minutes", __pickleIndex: "52", exampleTags: []) | @F005 @AC-014 @FR-004 @FR-014 @US2-AS4 | passed | 00:00:00.0885713 |
| F005 | Demo history loading without a demo password creates no account(n: "01", demoPassword: "absent", __pickleIndex: "220", exampleTags: []) | @F005 @AC-061 @FR-030 @US7-AS6 | passed | 00:00:00.3598323 |
| F005 | A note given with a record is the note; no note leaves it unchanged(n: "04", action: "records Yes for 2026-11-18 with the note \"Better "···, note: "the note \"Better today.\"", __pickleIndex: "141", exampleTags: []) | @F005 @AC-037 @FR-007 @US4-AS3 | passed | 00:00:00.0904744 |
| F005 | The demo goal "Reading, 20 min daily" shows Met, Failed with a partial amount, Missed with a note, Pending and Due | @F005 @AC-052 @FR-030 @US7-AS2 | passed | 00:00:00.4380292 |
| F005 | Recording checks the acting user, the goal, the date, the value and the note in that order(n: "07", actor: "`f5.ac010.owner.<n>@example.com`", date: "2026-11-18", value: "3", note: "a text of 501 characters", result: "\"The note can have at most 500 characters.\"", __pickleIndex: "39", exampleTags: []) | @F005 @AC-010 @FR-005 @FR-018 @US1-AS7 | failed | 00:00:00.2271778 |
| F005 | A note has at most 500 characters(n: "03", action: "records 20 for 2026-11-18 with the note a text of "···, result: "the result is \"The note can have at most 500 char"···, __pickleIndex: "148", exampleTags: []) | @F005 @AC-038 @FR-005 @FR-007 @US4-AS4 | passed | 00:00:00.0874535 |
| F005 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit(n: "02", settings: "yes/no, 1 time per day", records: "nothing", state: "Missed, no record", __pickleIndex: "94", exampleTags: []) | @F005 @AC-022 @FR-013 @US2-AS12 | passed | 00:00:00.0839490 |
| F005 | The lowest and highest amounts are accepted and kept(n: "03", measurement: "duration 20 minutes", value: "0", __pickleIndex: "27", exampleTags: []) | @F005 @AC-008 @FR-004 @FR-005 | passed | 00:00:00.0846113 |
| F005 | The history's range has 1 to 366 days and is checked after the goal(n: "04", actor: "`f5.ac043.owner.<n>@example.com`", first: "2026-11-19", last: "2026-11-18", result: "the result is \"Choose a range of 1 to 366 days.\"", __pickleIndex: "164", exampleTags: []) | @F005 @AC-043 @FR-009 @FR-018 @US5-AS4 | failed | 00:00:00.1488358 |
| F005 | A final Build duration commitment is Met at or above its target and Failed below it(n: "06", duration: "duration 1 in hours", value: "59", state: "Failed", progress: "progress 59 of 60 minutes", __pickleIndex: "53", exampleTags: []) | @F005 @AC-014 @FR-004 @FR-014 @US2-AS4 | passed | 00:00:00.0871987 |
| F005 | Demo history loading takes a demo password only with at least 8 characters and a digit(n: "02", demoPassword: "`examples`", why: "8 characters, no digit", result: "its result is \"demo password does not meet the pa"···, __pickleIndex: "274", exampleTags: []) | @F005 @AC-069 @FR-030 @US7-AS6 | passed | 00:00:00.3669719 |
| F005 | A missing record added to a Failed week is a correction that makes it Met | @F005 @AC-028 @FR-006 @FR-014 @US3-AS4 | passed | 00:00:00.0944567 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "23", settings: "Reduce, duration 30 minutes, fixed days on all sev"···, records: "29 for 2026-11-18", state: "Met", progress: "progress 29 of 30", __pickleIndex: "259", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0889852 |
| F005 | A per-week note gives its week's first and last date, and a goal with fewer notes gives only those | @F005 @AC-041 @FR-008 | passed | 00:00:00.1032738 |
| F005 | The evaluation calendar still gives no state | @F005 @AC-063 @FR-019 | passed | 00:00:00.1457990 |
| F005 | Around a clock change the grace period lasts exactly 48 elapsed hours(n: "03", createdAt: "`2027-03-27T08:00:00Z`", date: "2027-03-27", time: "`2027-03-29T21:59:59Z`", start: "`2027-03-26T22:00:00Z`", end: "`2027-03-27T22:00:00Z`", entry: "Pending, grace ends `2027-03-29T22:00:00Z` (01:00 "···, __pickleIndex: "104", exampleTags: []) | @F005 @AC-023 @FR-016 @US2-AS13 | passed | 00:00:00.0968271 |
| F005 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit(n: "08", settings: "count 1 `sweet`, 1 time per day", records: "0 for 2026-11-18", state: "Met", __pickleIndex: "100", exampleTags: []) | @F005 @AC-022 @FR-013 @US2-AS12 | passed | 00:00:00.0852673 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "07", settings: "Build, duration 20 minutes, 1 time per day", records: "20 for 2026-11-18", state: "Met", progress: "progress 1 of 1 times", __pickleIndex: "243", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0881460 |
| F005 | An evaluation period takes at most 50 records, but its records can still be changed or deleted(n: "05", action: "deletes the first record of 2026-11-18 and then re"···, result: "deleting and recording both succeed with the state"···, __pickleIndex: "129", exampleTags: []) | @F005 @AC-033 @FR-003 @FR-005 | passed | 00:00:00.1762552 |
| F005 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit(n: "01", settings: "yes/no, fixed days on all seven days", records: "nothing", state: "Missed, no record", __pickleIndex: "93", exampleTags: []) | @F005 @AC-022 @FR-013 @US2-AS12 | passed | 00:00:00.0856466 |
| F005 | A final per-week Build yes/no period is Met with at least N Yes records(n: "01", records: "Yes for 2026-11-16, Yes for 2026-11-18 and Yes for"···, state: "Met", progress: "progress 3 of 3 times", __pickleIndex: "66", exampleTags: []) | @F005 @AC-017 @FR-014 @US2-AS7 | passed | 00:00:00.0890201 |
| F005 | A final Build duration commitment is Met at or above its target and Failed below it(n: "04", duration: "duration 20 minutes", value: "45", state: "Met", progress: "progress 45 of 20 minutes", __pickleIndex: "51", exampleTags: []) | @F005 @AC-014 @FR-004 @FR-014 @US2-AS4 | passed | 00:00:00.0861908 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "05", settings: "Build, count 5 `pages`, 2 times per day", records: "5 and 4 for 2026-11-18", state: "Failed", progress: "progress 1 of 2 times", __pickleIndex: "241", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0919265 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "12", settings: "Reduce, duration 30 minutes, 1 time per day", records: "20 and 10 for 2026-11-18", state: "Met", progress: "progress total 30 of at most 30", __pickleIndex: "248", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0930996 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "24", settings: "Reduce, duration 30 minutes, 1 time per day", records: "20 and 9 for 2026-11-18", state: "Met", progress: "progress total 29 of at most 30", __pickleIndex: "260", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0898980 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "30", settings: "Reduce, duration 60 minutes, 1 time per week", records: "30 for 2026-11-16 and 31 for 2026-11-18", state: "Failed", progress: "progress total 61 of at most 60", __pickleIndex: "266", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0890967 |
| F005 | What can be recorded now is every Due and every Pending commitment or period, newest first(n: "05", schedule: "3 times per week", startDate: "2026-11-16", time: "`2026-11-24T10:00:00Z`", entries: "in this order: 2026-11-23 to 2026-11-29, Due, grac"···, __pickleIndex: "178", exampleTags: []) | @F005 @AC-045 @FR-011 @US5-AS3 | passed | 00:00:00.0821475 |
| F005 | An unrecorded commitment is Pending for 48 hours after its end and then Missed by itself(n: "04", time: "`2026-11-21T00:00:01Z`", entry: "Missed, no grace end, correction", __pickleIndex: "47", exampleTags: []) | @F005 @AC-013 @FR-010 @FR-016 @US2-AS2 @US2-AS3 | passed | 00:00:00.1130188 |
| F005 | Another registered user gets "This goal does not exist." from every use case of this slice(n: "08", useCase: "record", goal: "a goal identifier that belongs to no goal", __pickleIndex: "190", exampleTags: []) | @F005 @AC-047 @FR-017 @FR-018 @US6-AS1 | passed | 00:00:00.1628470 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "08", measurement: "count 5 `pages`", before: "5", change: "the record of 2026-11-18 to 9999", result: "changing succeeds with the state Met, a correction"···, __pickleIndex: "232", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.0898660 |
| F005 | A record identifier that is not a record of the goal named in the call does not exist(n: "03", action: "changes a record identifier that belongs to no rec"···, __pickleIndex: "123", exampleTags: []) | @F005 @AC-032 @FR-005 @FR-017 @US3-AS7 | passed | 00:00:00.0993432 |
| F005 | A change is a correction only from the instant its grace period ends(n: "03", before: "12 for 2026-11-18", time: "`2026-11-18T12:00:00Z`", action: "deletes the record of 2026-11-18", result: "deleting succeeds with the state Due, not a correc"···, __pickleIndex: "112", exampleTags: []) | @F005 @AC-027 @FR-006 | passed | 00:00:00.0899171 |
| F005 | A note has at most 500 characters(n: "04", action: "records 20 for 2026-11-18 with the note a text of "···, result: "recording succeeds with the state Due, not a corre"···, __pickleIndex: "149", exampleTags: []) | @F005 @AC-038 @FR-005 @FR-007 @US4-AS4 | passed | 00:00:00.0885519 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "29", settings: "Reduce, duration 60 minutes, 1 time per week", records: "30 for 2026-11-16 and 30 for 2026-11-18", state: "Met", progress: "progress total 60 of at most 60", __pickleIndex: "265", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0946207 |
| F005 | A value that does not fit the goal's measurement is refused with its message(n: "02", measurement: "count 5 `pages`", value: "no value", message: "\"Enter a whole number from 0 to 9999.\"", __pickleIndex: "18", exampleTags: []) | @F005 @AC-007 @FR-005 @US1-AS7 | passed | 00:00:00.0806897 |
| F005 | The first record of a Missed commitment that has a note: the note given is the note, no note keeps it(n: "02", action: "records 20 for 2026-11-18 with the note `\"\"`", note: "no note", __pickleIndex: "280", exampleTags: []) | @F005 @AC-071 @FR-007 @US4-AS9 | passed | 00:00:00.0922128 |
| F005 | What can be recorded now is every Due and every Pending commitment or period, newest first(n: "07", schedule: "fixed days Friday", startDate: "2026-11-20", time: "`2026-11-18T10:00:00Z`", entries: "no entry", __pickleIndex: "180", exampleTags: []) | @F005 @AC-045 @FR-011 @US5-AS3 | passed | 00:00:00.0840586 |
| F005 | The lowest and highest amounts are accepted and kept(n: "01", measurement: "count 5 `pages`", value: "0", __pickleIndex: "25", exampleTags: []) | @F005 @AC-008 @FR-004 @FR-005 | passed | 00:00:00.0835812 |
| F005 | The history's range has 1 to 366 days and is checked after the goal(n: "06", actor: "the anonymous visitor", first: "2026-11-16", last: "2027-11-17", result: "the result is \"You are not signed in.\"", __pickleIndex: "166", exampleTags: []) | @F005 @AC-043 @FR-009 @FR-018 @US5-AS4 | passed | 00:00:00.1712143 |
| F005 | Progress is given at any state, as the goal's kind of target counts it(n: "04", settings: "Reduce, yes/no, 1 time per day", records: "Yes, Yes and No for 2026-11-18", progress: "progress 2 of at most 1", __pickleIndex: "170", exampleTags: []) | @F005 @AC-044 @FR-014 @US5-AS2 | passed | 00:00:00.0896785 |
| F005 | Around a clock change the grace period lasts exactly 48 elapsed hours(n: "04", createdAt: "`2027-03-27T08:00:00Z`", date: "2027-03-27", time: "`2027-03-29T22:00:00Z`", start: "`2027-03-26T22:00:00Z`", end: "`2027-03-27T22:00:00Z`", entry: "Missed, no grace end", __pickleIndex: "105", exampleTags: []) | @F005 @AC-023 @FR-016 @US2-AS13 | passed | 00:00:00.0846538 |
| F005 | An administrator gets "Goals are for registered users." from every use case of this slice(n: "04", useCase: "save note", __pickleIndex: "197", exampleTags: []) | @F005 @AC-049 @FR-018 @US6-AS3 | passed | 00:00:00.1578888 |
| F005 | What can be recorded now is every Due and every Pending commitment or period, newest first(n: "03", schedule: "fixed days Monday, Wednesday and Friday", startDate: "2026-11-16", time: "`2026-11-19T10:00:00Z`", entries: "exactly one entry: 2026-11-18, Pending, grace ends"···, __pickleIndex: "176", exampleTags: []) | @F005 @AC-045 @FR-011 @US5-AS3 | passed | 00:00:00.0840613 |
| F005 | An ended commitment or period stays Pending during its grace period, even when its records reach the target(n: "02", settings: "duration 20 minutes, fixed days on all seven days", value: "20", time: "`2026-11-20T23:59:59Z`", entry: "Pending, grace ends `2026-11-21T00:00:00Z`, progre"···, __pickleIndex: "87", exampleTags: []) | @F005 @AC-021 @FR-012 @US2-AS11 | passed | 00:00:00.0854148 |
| F005 | Another registered user gets "This goal does not exist." from every use case of this slice(n: "03", useCase: "delete, with the owner's record identifier", goal: "the goal of `f5.ac047.owner.<n>@example.com`", __pickleIndex: "185", exampleTags: []) | @F005 @AC-047 @FR-017 @FR-018 @US6-AS1 | failed | 00:00:00.1592233 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "04", settings: "Build, count 5 `pages`, 2 times per day", records: "5 and 5 for 2026-11-18", state: "Met", progress: "progress 2 of 2 times", __pickleIndex: "240", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0868572 |
| F005 | The anonymous visitor gets "You are not signed in." from every use case of this slice(n: "01", useCase: "record", __pickleIndex: "201", exampleTags: []) | @F005 @AC-050 @FR-018 @US6-AS4 | passed | 00:00:00.0888830 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "18", settings: "Reduce, yes/no, 2 times per day", records: "Yes and Yes for 2026-11-18", state: "Met", progress: "progress 2 of at most 2", __pickleIndex: "254", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0919776 |
| F005 | What can be recorded now is every Due and every Pending commitment or period, newest first(n: "01", schedule: "fixed days on all seven days", startDate: "2026-11-16", time: "`2026-11-19T10:00:00Z`", entries: "in this order: 2026-11-19, Due, grace ends `2026-1"···, __pickleIndex: "174", exampleTags: []) | @F005 @AC-045 @FR-011 @US5-AS3 | passed | 00:00:00.0826503 |
| F005 | Progress is given at any state, as the goal's kind of target counts it(n: "01", settings: "Build, duration 20 minutes, fixed days on all seve"···, records: "12 for 2026-11-18", progress: "progress 12 of 20 minutes", __pickleIndex: "167", exampleTags: []) | @F005 @AC-044 @FR-014 @US5-AS2 | passed | 00:00:00.0865316 |
| F005 | The demo goal "Sugar, at most 1 sweet a day" shows Met, Failed with a note, Missed, Pending and Due | @F005 @AC-053 @FR-030 @US7-AS2 | passed | 00:00:00.4234012 |
| F005 | A value that does not belong to the goal's measurement is not stored(n: "03", measurement: "duration 20 minutes", given: "the amount 12 together with No", stored: "(2026-11-18, 12), with neither Yes nor No", __pickleIndex: "32", exampleTags: []) | @F005 @AC-009 @FR-004 | passed | 00:00:00.0851807 |
| F005 | An evaluation period takes at most 50 records, but its records can still be changed or deleted(n: "04", action: "changes the first record of 2026-11-18 to No", result: "changing succeeds with the state Due, not a correc"···, __pickleIndex: "128", exampleTags: []) | @F005 @AC-033 @FR-003 @FR-005 | passed | 00:00:00.1763447 |
| F005 | A final Build duration commitment is Met at or above its target and Failed below it(n: "08", duration: "duration 1 in hours", value: "61", state: "Met", progress: "progress 61 of 60 minutes", __pickleIndex: "55", exampleTags: []) | @F005 @AC-014 @FR-004 @FR-014 @US2-AS4 | passed | 00:00:00.0866662 |
| F005 | A record added to a Missed commitment is a correction that makes it Met | @F005 @AC-024 @FR-006 @US3-AS1 | passed | 00:00:00.1267425 |
| F005 | Lowering the record of a Met commitment below its target is a correction that makes it Failed(n: "02", action: "records 12 for 2026-11-18", result: "recording", __pickleIndex: "108", exampleTags: []) | @F005 @AC-025 @FR-006 @US3-AS2 | passed | 00:00:00.0901698 |
| F005 | A date that belongs to no commitment or period cannot be recorded(n: "01", schedule: "fixed days Monday, Wednesday and Friday", createdAt: "`2026-11-16T08:00:00Z`", startDate: "2026-11-16", date: "2026-11-17", why: "a Tuesday", __pickleIndex: "4", exampleTags: []) | @F005 @AC-005 @FR-002 @US1-AS5 | passed | 00:00:00.0847665 |
| F005 | The recent notes are the 5 with the latest dates, newest first, each with its state | @F005 @AC-040 @FR-008 @US4-AS6 | passed | 00:00:00.1646416 |
| F005 | A note given with a record is the note; no note leaves it unchanged(n: "05", action: "changes the record of 2026-11-16 to No", note: "the note \"Knee was sore.\"", __pickleIndex: "142", exampleTags: []) | @F005 @AC-037 @FR-007 @US4-AS3 | passed | 00:00:00.0937574 |
| F005 | Demo history loading creates the demo account with history and its three demo goals | @F005 @AC-051 @FR-030 @US7-AS1 | passed | 00:00:00.4574978 |
| F005 | Recording checks the acting user, the goal, the date, the value and the note in that order(n: "05", actor: "`f5.ac010.owner.<n>@example.com`", date: "2026-11-19", value: "no value", note: "a text of 501 characters", result: "\"You cannot record a day that has not begun.\"", __pickleIndex: "37", exampleTags: []) | @F005 @AC-010 @FR-005 @FR-018 @US1-AS7 | failed | 00:00:00.2198220 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "02", measurement: "count 5 `pages`", before: "5", change: "the record of 2026-11-18 to -1", result: "the result is \"Enter a whole number from 0 to 999"···, __pickleIndex: "226", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.0912232 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "31", settings: "Build, count 5 `pages`, 2 times per day", records: "10 for 2026-11-18", state: "Failed", progress: "progress 1 of 2 times", __pickleIndex: "267", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0879759 |
| F005 | Any begun date of a week names that week's one note(n: "03", action: "saves the note \"Week note.\" for 2026-11-20", result: "the result is \"You cannot record a day that has n"···, __pickleIndex: "154", exampleTags: []) | @F005 @AC-039 @FR-002 @FR-007 @US4-AS5 | passed | 00:00:00.0842347 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "26", settings: "Reduce, yes/no, 2 times per week", records: "Yes for 2026-11-16 and No for 2026-11-18", state: "Met", progress: "progress 1 of at most 2", __pickleIndex: "262", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0890509 |
| F005 | A note given with a record is the note; no note leaves it unchanged(n: "02", action: "records Yes for 2026-11-18 with the note `\"   \"`", note: "no note", __pickleIndex: "139", exampleTags: []) | @F005 @AC-037 @FR-007 @US4-AS3 | passed | 00:00:00.0953335 |
| F005 | What can be recorded now is every Due and every Pending commitment or period, newest first(n: "02", schedule: "1 time per day", startDate: "2026-11-16", time: "`2026-11-19T10:00:00Z`", entries: "in this order: 2026-11-19, Due; 2026-11-18, Pendin"···, __pickleIndex: "175", exampleTags: []) | @F005 @AC-045 @FR-011 @US5-AS3 | passed | 00:00:00.0850288 |
| F005 | Recording again for a commitment replaces its one record | @F005 @AC-002 @FR-003 @US1-AS2 | passed | 00:00:00.1274719 |
| F005 | The history gives every commitment of the range in date order, each with its state at the supplied time | @F005 @AC-042 @FR-009 @FR-010 @US5-AS1 | passed | 00:00:00.0968708 |
| F005 | Another registered user gets "This goal does not exist." from every use case of this slice(n: "06", useCase: "recordable now", goal: "the goal of `f5.ac047.owner.<n>@example.com`", __pickleIndex: "188", exampleTags: []) | @F005 @AC-047 @FR-017 @FR-018 @US6-AS1 | failed | 00:00:00.1640969 |
| F005 | An evaluation period takes at most 50 records, but its records can still be changed or deleted(n: "02", action: "records Yes for 2026-11-18 with the note a text of"···, result: "the result is \"The note can have at most 500 char"···, __pickleIndex: "126", exampleTags: []) | @F005 @AC-033 @FR-003 @FR-005 | passed | 00:00:00.1797799 |
| F005 | The demo Yoga goal's weeks when R is a Sunday or a Tuesday(n: "02", runTime: "`2026-11-24T10:00:00Z`", r: "Tuesday 2026-11-24", startDate: "2026-10-27", lastDate: "2026-11-29", entries: "4 entries: 2026-11-02 to 2026-11-08, Missed, no re"···, __pickleIndex: "272", exampleTags: []) | @F005 @AC-068 @FR-030 @US7-AS2 | passed | 00:00:00.4369944 |
| F005 | An administrator gets "Goals are for registered users." from every use case of this slice(n: "02", useCase: "change, with the owner's record identifier", __pickleIndex: "195", exampleTags: []) | @F005 @AC-049 @FR-018 @US6-AS3 | passed | 00:00:00.1649206 |
| F005 | The anonymous visitor gets "You are not signed in." from every use case of this slice(n: "02", useCase: "change, with the owner's record identifier", __pickleIndex: "202", exampleTags: []) | @F005 @AC-050 @FR-018 @US6-AS4 | passed | 00:00:00.0895080 |
| F005 | A demo goal is left out when its catalog habit is not Published, and the results stay the same(n: "03", habits: "a Draft `Reading`, a Draft `Sugar` and a Draft `Yo"···, goals: "empty", __pickleIndex: "219", exampleTags: []) | @F005 @AC-060 @FR-030 | passed | 00:00:00.4474140 |
| F005 | A note saved on its own replaces the note, and an empty one removes it(n: "02", note: "`\"   \"`", result: "no note", __pickleIndex: "135", exampleTags: []) | @F005 @AC-036 @FR-007 @US4-AS2 | passed | 00:00:00.0946877 |
| F005 | A final per-day yes/no period counts its Yes records(n: "07", settings: "Build, yes/no, 2 times per day", records: "Yes, Yes and Yes for 2026-11-18", state: "Met", progress: "progress 3 of 2 times", __pickleIndex: "85", exampleTags: []) | @F005 @AC-020 @FR-014 @US2-AS10 | passed | 00:00:00.0951548 |
| F005 | The lowest and highest amounts are accepted and kept(n: "02", measurement: "count 5 `pages`", value: "9999", __pickleIndex: "26", exampleTags: []) | @F005 @AC-008 @FR-004 @FR-005 | passed | 00:00:00.0837951 |
| F005 | A demo goal is left out when its catalog habit is not Published, and the results stay the same(n: "02", habits: "a Published `reading`, a Draft `Sugar` and a Draft"···, goals: "exactly one goal, on the catalog habit `reading`, "···, __pickleIndex: "218", exampleTags: []) | @F005 @AC-060 @FR-030 | passed | 00:00:00.4690287 |
| F005 | A final Reduce count commitment is Met at or below its limit and Failed above it(n: "01", count: "count 2 `cups`", value: "0", state: "Met", __pickleIndex: "56", exampleTags: []) | @F005 @AC-015 @FR-014 @US2-AS5 | passed | 00:00:00.0841020 |
| F005 | A day that has not begun on the goal's timeline cannot be recorded(n: "02", timeZone: "UTC", schedule: "1 time per day", time: "`2026-11-18T10:00:00Z`", date: "2026-11-19", result: "the result is \"You cannot record a day that has n"···, __pickleIndex: "10", exampleTags: []) | @F005 @AC-006 @FR-002 @US1-AS6 | passed | 00:00:00.0861715 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "17", settings: "Reduce, yes/no, 2 times per week", records: "Yes for 2026-11-16, Yes for 2026-11-17 and Yes for"···, state: "Failed", progress: "progress 3 of at most 2", __pickleIndex: "253", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0935974 |
| F005 | A day that has not begun on the goal's timeline cannot be recorded(n: "03", timeZone: "UTC", schedule: "fixed days Monday, Wednesday and Friday", time: "`2026-11-18T10:00:00Z`", date: "2026-11-19", result: "the result is \"Nothing is scheduled on this date."···, __pickleIndex: "11", exampleTags: []) | @F005 @AC-006 @FR-002 @US1-AS6 | passed | 00:00:00.0859706 |
| F005 | The history's range has 1 to 366 days and is checked after the goal(n: "01", actor: "`f5.ac043.owner.<n>@example.com`", first: "2026-11-18", last: "2026-11-18", result: "it gives exactly one entry, 2026-11-18", __pickleIndex: "161", exampleTags: []) | @F005 @AC-043 @FR-009 @FR-018 @US5-AS4 | failed | 00:00:00.1851167 |
| F005 | Demo loading still names only the three slice-4 demo accounts, and their goals get no records | @F005 @AC-062 @FR-019 @FR-031 @US7-AS5 | passed | 00:00:00.5195625 |
| F005 | A final per-week Build yes/no period is Met with at least N Yes records(n: "02", records: "Yes for 2026-11-16, Yes for 2026-11-18 and No for "···, state: "Failed", progress: "progress 2 of 3 times", __pickleIndex: "67", exampleTags: []) | @F005 @AC-017 @FR-014 @US2-AS7 | passed | 00:00:00.0938605 |
| F005 | A record belongs to the week of its date, not to the week of the instant it is saved(n: "01", time: "`2026-11-22T23:59:59Z`", date: "2026-11-22", state: "Due", firstWeek: "records (2026-11-22, Yes)", secondWeek: "no record", __pickleIndex: "130", exampleTags: []) | @F005 @AC-034 @FR-001 @FR-009 | passed | 00:00:00.0895715 |
| F005 | A final yes/no commitment is Met on Yes for Build and on No for Reduce(n: "01", intent: "Build", value: "Yes", state: "Met", __pickleIndex: "62", exampleTags: []) | @F005 @AC-016 @FR-014 @US2-AS6 | passed | 00:00:00.0881282 |
| F005 | Recording checks the acting user, the goal, the date, the value and the note in that order(n: "04", actor: "`f5.ac010.owner.<n>@example.com`", date: "2026-11-17", value: "no value", note: "a text of 501 characters", result: "\"Nothing is scheduled on this date.\"", __pickleIndex: "36", exampleTags: []) | @F005 @AC-010 @FR-005 @FR-018 @US1-AS7 | failed | 00:00:00.2237982 |

### F005/AC-010 Recording checks the acting user, the goal, the date, the value and the note in that order(n: "06", actor: "`f5.ac010.owner.<n>@example.com`", date: "2026-11-18", value: "no value", note: "a text of 501 characters", result: "\"Enter a whole number from 0 to 9999.\"", __pickleIndex: "38", exampleTags: []) — failed

Step trace:
```
Given the registered user `f5.ac010.owner.06@example.com`, who created the custom habit `Reading`
-> done: GivenSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit("f5.ac010.owner.06...", "", "Reading") (0,0s)
And `f5.ac010.owner.06@example.com` created a goal on their custom habit `Reading` with Build, count 5 `pages`, 1 time per day and the start date 2026-11-18
-> done: GivenSteps.GivenTheUserCreatedAGoal("", "f5.ac010.owner.06...", "their custom habi...", "Build, count 5 `p...", "2026-11-18", "") (0,1s)
And the registered user `f5.ac010.other.06@example.com`
-> done: GivenSteps.GivenTheRegisteredUser("f5.ac010.other.06...") (0,1s)
And the administrator `f5.ac010.admin.06@example.com`
-> done: GivenSteps.GivenTheAdministrator("f5.ac010.admin.06...") (0,1s)
When `f5.ac010.owner.<n>@example.com` records no value for 2026-11-18 with the note a text of 501 characters on the goal of `f5.ac010.owner.06@example.com`
-> error: No account `f5.ac010.owner.<n>@example.com` signed in in this scenario. (0,0s)
Then the result is "Enter a whole number from 0 to 9999."
-> skipped because of previous errors
And nothing is recorded
-> skipped because of previous errors
```

Exception:
```
System.InvalidOperationException : No account `f5.ac010.owner.<n>@example.com` signed in in this scenario.
```

Stack trace (AC project frames only):
```
at HT.AcceptanceTests.Support.F005.F005Arrange.AccountAsync(String email) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 128
at HT.AcceptanceTests.Support.F005.F005Arrange.ActorAsync(String token) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 149
at HT.AcceptanceTests.Support.F005.F005Actions.RunAsync(String actorToken, String action) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Actions.cs:line 239
at HT.AcceptanceTests.StepDefinitions.F005.WhenSteps.WhenTheActorWrites(String time, String actor, String action) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\StepDefinitions\F005\WhenSteps.cs:line 22
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F005.RecordingAndEvaluationFeature.RecordingChecksTheActingUserTheGoalTheDateTheValueAndTheNoteInThatOrder(String n, String actor, String date, String value, String note, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Features\F005\recording-and-evaluation.feature:line 128
[non-AC frame(s) hidden]
```

### F005/AC-047 Another registered user gets "This goal does not exist." from every use case of this slice(n: "04", useCase: "save note", goal: "the goal of `f5.ac047.owner.<n>@example.com`", __pickleIndex: "186", exampleTags: []) — failed

Step trace:
```
Given the registered user `f5.ac047.owner.04@example.com`, who created the custom habit `Reading`
-> done: GivenSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit("f5.ac047.owner.04...", "", "Reading") (0,0s)
And `f5.ac047.owner.04@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
-> done: GivenSteps.GivenTheUserCreatedAGoal("", "f5.ac047.owner.04...", "their custom habi...", "Build, duration 2...", "2026-11-18", "") (0,1s)
And `f5.ac047.owner.04@example.com` recorded 20 for 2026-11-18 with the note "Private note."
-> done: GivenSteps.GivenTheUserRecorded("", "f5.ac047.owner.04...", "20 for 2026-11-18...") (0,0s)
And the registered user `f5.ac047.other.04@example.com`
-> done: GivenSteps.GivenTheRegisteredUser("f5.ac047.other.04...") (0,1s)
When `f5.ac047.other.04@example.com` calls save note with the goal of `f5.ac047.owner.<n>@example.com`
-> error: `f5.ac047.owner.<n>@example.com` created no goal in this scenario. (0,0s)
Then the result is "This goal does not exist." and it gives no record, note or state
-> skipped because of previous errors
And reading the history of the goal from 2026-11-18 to 2026-11-18 as `f5.ac047.owner.04@example.com` gives records (2026-11-18, 20) and the note "Private note."
-> skipped because of previous errors
```

Exception:
```
System.InvalidOperationException : `f5.ac047.owner.<n>@example.com` created no goal in this scenario.
```

Stack trace (AC project frames only):
```
at HT.AcceptanceTests.Support.F005.F005Arrange.GoalOf(String ownerEmail) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 258
at HT.AcceptanceTests.Support.F005.F005Arrange.GoalPhraseAsync(String phrase, String actorEmail) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 289
at HT.AcceptanceTests.Support.F005.F005Actions.CallAsync(String actorToken, String useCase, String goalPhrase) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Actions.cs:line 462
at HT.AcceptanceTests.StepDefinitions.F005.WhenSteps.WhenTheActorCallsAUseCase(String actor, String useCase, String goal) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\StepDefinitions\F005\WhenSteps.cs:line 36
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F005.RecordingAndEvaluationFeature.AnotherRegisteredUserGetsThisGoalDoesNotExist_FromEveryUseCaseOfThisSlice(String n, String useCase, String goal, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Features\F005\recording-and-evaluation.feature:line 675
[non-AC frame(s) hidden]
```

### F005/AC-043 The history's range has 1 to 366 days and is checked after the goal(n: "03", actor: "`f5.ac043.owner.<n>@example.com`", first: "2026-11-16", last: "2027-11-17", result: "the result is \"Choose a range of 1 to 366 days.\"", __pickleIndex: "163", exampleTags: []) — failed

Step trace:
```
Given the registered user `f5.ac043.owner.03@example.com`, who created the custom habit `Reading`
-> done: GivenSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit("f5.ac043.owner.03...", "", "Reading") (0,0s)
And `f5.ac043.owner.03@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
-> done: GivenSteps.GivenTheUserCreatedAGoal("", "f5.ac043.owner.03...", "their custom habi...", "Build, duration 2...", "2026-11-18", "") (0,1s)
And the registered user `f5.ac043.other.03@example.com`
-> done: GivenSteps.GivenTheRegisteredUser("f5.ac043.other.03...") (0,1s)
When `f5.ac043.owner.<n>@example.com` reads the history of the goal of `f5.ac043.owner.03@example.com` from 2026-11-16 to 2027-11-17
-> error: No account `f5.ac043.owner.<n>@example.com` signed in in this scenario. (0,0s)
Then the result is "Choose a range of 1 to 366 days."
-> skipped because of previous errors
```

Exception:
```
System.InvalidOperationException : No account `f5.ac043.owner.<n>@example.com` signed in in this scenario.
```

Stack trace (AC project frames only):
```
at HT.AcceptanceTests.Support.F005.F005Arrange.AccountAsync(String email) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 128
at HT.AcceptanceTests.Support.F005.F005Arrange.ActorAsync(String token) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 149
at HT.AcceptanceTests.Support.F005.F005Actions.ReadAsync(String actorToken, String what) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Actions.cs:line 379
at HT.AcceptanceTests.StepDefinitions.F005.WhenSteps.WhenTheActorReads(String time, String actor, String what) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\StepDefinitions\F005\WhenSteps.cs:line 29
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F005.RecordingAndEvaluationFeature.TheHistorysRangeHas1To366DaysAndIsCheckedAfterTheGoal(String n, String actor, String first, String last, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Features\F005\recording-and-evaluation.feature:line 608
[non-AC frame(s) hidden]
```

### F005/AC-047 Another registered user gets "This goal does not exist." from every use case of this slice(n: "07", useCase: "recent notes", goal: "the goal of `f5.ac047.owner.<n>@example.com`", __pickleIndex: "189", exampleTags: []) — failed

Step trace:
```
Given the registered user `f5.ac047.owner.07@example.com`, who created the custom habit `Reading`
-> done: GivenSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit("f5.ac047.owner.07...", "", "Reading") (0,0s)
And `f5.ac047.owner.07@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
-> done: GivenSteps.GivenTheUserCreatedAGoal("", "f5.ac047.owner.07...", "their custom habi...", "Build, duration 2...", "2026-11-18", "") (0,1s)
And `f5.ac047.owner.07@example.com` recorded 20 for 2026-11-18 with the note "Private note."
-> done: GivenSteps.GivenTheUserRecorded("", "f5.ac047.owner.07...", "20 for 2026-11-18...") (0,0s)
And the registered user `f5.ac047.other.07@example.com`
-> done: GivenSteps.GivenTheRegisteredUser("f5.ac047.other.07...") (0,1s)
When `f5.ac047.other.07@example.com` calls recent notes with the goal of `f5.ac047.owner.<n>@example.com`
-> error: `f5.ac047.owner.<n>@example.com` created no goal in this scenario. (0,0s)
Then the result is "This goal does not exist." and it gives no record, note or state
-> skipped because of previous errors
And reading the history of the goal from 2026-11-18 to 2026-11-18 as `f5.ac047.owner.07@example.com` gives records (2026-11-18, 20) and the note "Private note."
-> skipped because of previous errors
```

Exception:
```
System.InvalidOperationException : `f5.ac047.owner.<n>@example.com` created no goal in this scenario.
```

Stack trace (AC project frames only):
```
at HT.AcceptanceTests.Support.F005.F005Arrange.GoalOf(String ownerEmail) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 258
at HT.AcceptanceTests.Support.F005.F005Arrange.GoalPhraseAsync(String phrase, String actorEmail) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 289
at HT.AcceptanceTests.Support.F005.F005Actions.CallAsync(String actorToken, String useCase, String goalPhrase) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Actions.cs:line 462
at HT.AcceptanceTests.StepDefinitions.F005.WhenSteps.WhenTheActorCallsAUseCase(String actor, String useCase, String goal) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\StepDefinitions\F005\WhenSteps.cs:line 36
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F005.RecordingAndEvaluationFeature.AnotherRegisteredUserGetsThisGoalDoesNotExist_FromEveryUseCaseOfThisSlice(String n, String useCase, String goal, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Features\F005\recording-and-evaluation.feature:line 675
[non-AC frame(s) hidden]
```

### F005/AC-043 The history's range has 1 to 366 days and is checked after the goal(n: "05", actor: "`f5.ac043.other.<n>@example.com`", first: "2026-11-16", last: "2027-11-17", result: "the result is \"This goal does not exist.\"", __pickleIndex: "165", exampleTags: []) — failed

Step trace:
```
Given the registered user `f5.ac043.owner.05@example.com`, who created the custom habit `Reading`
-> done: GivenSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit("f5.ac043.owner.05...", "", "Reading") (0,0s)
And `f5.ac043.owner.05@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
-> done: GivenSteps.GivenTheUserCreatedAGoal("", "f5.ac043.owner.05...", "their custom habi...", "Build, duration 2...", "2026-11-18", "") (0,1s)
And the registered user `f5.ac043.other.05@example.com`
-> done: GivenSteps.GivenTheRegisteredUser("f5.ac043.other.05...") (0,2s)
When `f5.ac043.other.<n>@example.com` reads the history of the goal of `f5.ac043.owner.05@example.com` from 2026-11-16 to 2027-11-17
-> error: No account `f5.ac043.other.<n>@example.com` signed in in this scenario. (0,0s)
Then the result is "This goal does not exist."
-> skipped because of previous errors
```

Exception:
```
System.InvalidOperationException : No account `f5.ac043.other.<n>@example.com` signed in in this scenario.
```

Stack trace (AC project frames only):
```
at HT.AcceptanceTests.Support.F005.F005Arrange.AccountAsync(String email) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 128
at HT.AcceptanceTests.Support.F005.F005Arrange.ActorAsync(String token) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 149
at HT.AcceptanceTests.Support.F005.F005Actions.ReadAsync(String actorToken, String what) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Actions.cs:line 379
at HT.AcceptanceTests.StepDefinitions.F005.WhenSteps.WhenTheActorReads(String time, String actor, String what) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\StepDefinitions\F005\WhenSteps.cs:line 29
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F005.RecordingAndEvaluationFeature.TheHistorysRangeHas1To366DaysAndIsCheckedAfterTheGoal(String n, String actor, String first, String last, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Features\F005\recording-and-evaluation.feature:line 608
[non-AC frame(s) hidden]
```

### F005/AC-047 Another registered user gets "This goal does not exist." from every use case of this slice(n: "02", useCase: "change, with the owner's record identifier", goal: "the goal of `f5.ac047.owner.<n>@example.com`", __pickleIndex: "184", exampleTags: []) — failed

Step trace:
```
Given the registered user `f5.ac047.owner.02@example.com`, who created the custom habit `Reading`
-> done: GivenSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit("f5.ac047.owner.02...", "", "Reading") (0,0s)
And `f5.ac047.owner.02@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
-> done: GivenSteps.GivenTheUserCreatedAGoal("", "f5.ac047.owner.02...", "their custom habi...", "Build, duration 2...", "2026-11-18", "") (0,1s)
And `f5.ac047.owner.02@example.com` recorded 20 for 2026-11-18 with the note "Private note."
-> done: GivenSteps.GivenTheUserRecorded("", "f5.ac047.owner.02...", "20 for 2026-11-18...") (0,0s)
And the registered user `f5.ac047.other.02@example.com`
-> done: GivenSteps.GivenTheRegisteredUser("f5.ac047.other.02...") (0,1s)
When `f5.ac047.other.02@example.com` calls change, with the owner's record identifier with the goal of `f5.ac047.owner.<n>@example.com`
-> error: `f5.ac047.owner.<n>@example.com` created no goal in this scenario. (0,0s)
Then the result is "This goal does not exist." and it gives no record, note or state
-> skipped because of previous errors
And reading the history of the goal from 2026-11-18 to 2026-11-18 as `f5.ac047.owner.02@example.com` gives records (2026-11-18, 20) and the note "Private note."
-> skipped because of previous errors
```

Exception:
```
System.InvalidOperationException : `f5.ac047.owner.<n>@example.com` created no goal in this scenario.
```

Stack trace (AC project frames only):
```
at HT.AcceptanceTests.Support.F005.F005Arrange.GoalOf(String ownerEmail) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 258
at HT.AcceptanceTests.Support.F005.F005Arrange.GoalPhraseAsync(String phrase, String actorEmail) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 289
at HT.AcceptanceTests.Support.F005.F005Actions.CallAsync(String actorToken, String useCase, String goalPhrase) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Actions.cs:line 462
at HT.AcceptanceTests.StepDefinitions.F005.WhenSteps.WhenTheActorCallsAUseCase(String actor, String useCase, String goal) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\StepDefinitions\F005\WhenSteps.cs:line 36
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F005.RecordingAndEvaluationFeature.AnotherRegisteredUserGetsThisGoalDoesNotExist_FromEveryUseCaseOfThisSlice(String n, String useCase, String goal, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Features\F005\recording-and-evaluation.feature:line 675
[non-AC frame(s) hidden]
```

### F005/AC-010 Recording checks the acting user, the goal, the date, the value and the note in that order(n: "03", actor: "`f5.ac010.other.<n>@example.com`", date: "2026-11-17", value: "no value", note: "a text of 501 characters", result: "\"This goal does not exist.\"", __pickleIndex: "35", exampleTags: []) — failed

Step trace:
```
Given the registered user `f5.ac010.owner.03@example.com`, who created the custom habit `Reading`
-> done: GivenSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit("f5.ac010.owner.03...", "", "Reading") (0,0s)
And `f5.ac010.owner.03@example.com` created a goal on their custom habit `Reading` with Build, count 5 `pages`, 1 time per day and the start date 2026-11-18
-> done: GivenSteps.GivenTheUserCreatedAGoal("", "f5.ac010.owner.03...", "their custom habi...", "Build, count 5 `p...", "2026-11-18", "") (0,1s)
And the registered user `f5.ac010.other.03@example.com`
-> done: GivenSteps.GivenTheRegisteredUser("f5.ac010.other.03...") (0,1s)
And the administrator `f5.ac010.admin.03@example.com`
-> done: GivenSteps.GivenTheAdministrator("f5.ac010.admin.03...") (0,1s)
When `f5.ac010.other.<n>@example.com` records no value for 2026-11-17 with the note a text of 501 characters on the goal of `f5.ac010.owner.03@example.com`
-> error: No account `f5.ac010.other.<n>@example.com` signed in in this scenario. (0,0s)
Then the result is "This goal does not exist."
-> skipped because of previous errors
And nothing is recorded
-> skipped because of previous errors
```

Exception:
```
System.InvalidOperationException : No account `f5.ac010.other.<n>@example.com` signed in in this scenario.
```

Stack trace (AC project frames only):
```
at HT.AcceptanceTests.Support.F005.F005Arrange.AccountAsync(String email) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 128
at HT.AcceptanceTests.Support.F005.F005Arrange.ActorAsync(String token) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 149
at HT.AcceptanceTests.Support.F005.F005Actions.RunAsync(String actorToken, String action) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Actions.cs:line 239
at HT.AcceptanceTests.StepDefinitions.F005.WhenSteps.WhenTheActorWrites(String time, String actor, String action) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\StepDefinitions\F005\WhenSteps.cs:line 22
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F005.RecordingAndEvaluationFeature.RecordingChecksTheActingUserTheGoalTheDateTheValueAndTheNoteInThatOrder(String n, String actor, String date, String value, String note, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Features\F005\recording-and-evaluation.feature:line 128
[non-AC frame(s) hidden]
```

### F005/AC-010 Recording checks the acting user, the goal, the date, the value and the note in that order(n: "02", actor: "the administrator `f5.ac010.admin.<n>@example.com`", date: "2026-11-17", value: "no value", note: "a text of 501 characters", result: "\"Goals are for registered users.\"", __pickleIndex: "34", exampleTags: []) — failed

Step trace:
```
Given the registered user `f5.ac010.owner.02@example.com`, who created the custom habit `Reading`
-> done: GivenSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit("f5.ac010.owner.02...", "", "Reading") (0,0s)
And `f5.ac010.owner.02@example.com` created a goal on their custom habit `Reading` with Build, count 5 `pages`, 1 time per day and the start date 2026-11-18
-> done: GivenSteps.GivenTheUserCreatedAGoal("", "f5.ac010.owner.02...", "their custom habi...", "Build, count 5 `p...", "2026-11-18", "") (0,1s)
And the registered user `f5.ac010.other.02@example.com`
-> done: GivenSteps.GivenTheRegisteredUser("f5.ac010.other.02...") (0,1s)
And the administrator `f5.ac010.admin.02@example.com`
-> done: GivenSteps.GivenTheAdministrator("f5.ac010.admin.02...") (0,1s)
When the administrator `f5.ac010.admin.<n>@example.com` records no value for 2026-11-17 with the note a text of 501 characters on the goal of `f5.ac010.owner.02@example.com`
-> error: No account `f5.ac010.admin.<n>@example.com` signed in in this scenario. (0,0s)
Then the result is "Goals are for registered users."
-> skipped because of previous errors
And nothing is recorded
-> skipped because of previous errors
```

Exception:
```
System.InvalidOperationException : No account `f5.ac010.admin.<n>@example.com` signed in in this scenario.
```

Stack trace (AC project frames only):
```
at HT.AcceptanceTests.Support.F005.F005Arrange.AccountAsync(String email) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 128
at HT.AcceptanceTests.Support.F005.F005Arrange.ActorAsync(String token) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 149
at HT.AcceptanceTests.Support.F005.F005Actions.RunAsync(String actorToken, String action) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Actions.cs:line 239
at HT.AcceptanceTests.StepDefinitions.F005.WhenSteps.WhenTheActorWrites(String time, String actor, String action) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\StepDefinitions\F005\WhenSteps.cs:line 22
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F005.RecordingAndEvaluationFeature.RecordingChecksTheActingUserTheGoalTheDateTheValueAndTheNoteInThatOrder(String n, String actor, String date, String value, String note, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Features\F005\recording-and-evaluation.feature:line 128
[non-AC frame(s) hidden]
```

### F005/AC-047 Another registered user gets "This goal does not exist." from every use case of this slice(n: "01", useCase: "record", goal: "the goal of `f5.ac047.owner.<n>@example.com`", __pickleIndex: "183", exampleTags: []) — failed

Step trace:
```
Given the registered user `f5.ac047.owner.01@example.com`, who created the custom habit `Reading`
-> done: GivenSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit("f5.ac047.owner.01...", "", "Reading") (0,0s)
And `f5.ac047.owner.01@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
-> done: GivenSteps.GivenTheUserCreatedAGoal("", "f5.ac047.owner.01...", "their custom habi...", "Build, duration 2...", "2026-11-18", "") (0,1s)
And `f5.ac047.owner.01@example.com` recorded 20 for 2026-11-18 with the note "Private note."
-> done: GivenSteps.GivenTheUserRecorded("", "f5.ac047.owner.01...", "20 for 2026-11-18...") (0,0s)
And the registered user `f5.ac047.other.01@example.com`
-> done: GivenSteps.GivenTheRegisteredUser("f5.ac047.other.01...") (0,1s)
When `f5.ac047.other.01@example.com` calls record with the goal of `f5.ac047.owner.<n>@example.com`
-> error: `f5.ac047.owner.<n>@example.com` created no goal in this scenario. (0,0s)
Then the result is "This goal does not exist." and it gives no record, note or state
-> skipped because of previous errors
And reading the history of the goal from 2026-11-18 to 2026-11-18 as `f5.ac047.owner.01@example.com` gives records (2026-11-18, 20) and the note "Private note."
-> skipped because of previous errors
```

Exception:
```
System.InvalidOperationException : `f5.ac047.owner.<n>@example.com` created no goal in this scenario.
```

Stack trace (AC project frames only):
```
at HT.AcceptanceTests.Support.F005.F005Arrange.GoalOf(String ownerEmail) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 258
at HT.AcceptanceTests.Support.F005.F005Arrange.GoalPhraseAsync(String phrase, String actorEmail) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 289
at HT.AcceptanceTests.Support.F005.F005Actions.CallAsync(String actorToken, String useCase, String goalPhrase) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Actions.cs:line 462
at HT.AcceptanceTests.StepDefinitions.F005.WhenSteps.WhenTheActorCallsAUseCase(String actor, String useCase, String goal) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\StepDefinitions\F005\WhenSteps.cs:line 36
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F005.RecordingAndEvaluationFeature.AnotherRegisteredUserGetsThisGoalDoesNotExist_FromEveryUseCaseOfThisSlice(String n, String useCase, String goal, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Features\F005\recording-and-evaluation.feature:line 675
[non-AC frame(s) hidden]
```

### F005/AC-043 The history's range has 1 to 366 days and is checked after the goal(n: "02", actor: "`f5.ac043.owner.<n>@example.com`", first: "2026-11-16", last: "2027-11-16", result: "it gives 364 entries, the first 2026-11-18 and the"···, __pickleIndex: "162", exampleTags: []) — failed

Step trace:
```
Given the registered user `f5.ac043.owner.02@example.com`, who created the custom habit `Reading`
-> done: GivenSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit("f5.ac043.owner.02...", "", "Reading") (0,0s)
And `f5.ac043.owner.02@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
-> done: GivenSteps.GivenTheUserCreatedAGoal("", "f5.ac043.owner.02...", "their custom habi...", "Build, duration 2...", "2026-11-18", "") (0,1s)
And the registered user `f5.ac043.other.02@example.com`
-> done: GivenSteps.GivenTheRegisteredUser("f5.ac043.other.02...") (0,1s)
When `f5.ac043.owner.<n>@example.com` reads the history of the goal of `f5.ac043.owner.02@example.com` from 2026-11-16 to 2027-11-16
-> error: No account `f5.ac043.owner.<n>@example.com` signed in in this scenario. (0,0s)
Then it gives 364 entries, the first 2026-11-18 and the last 2027-11-16
-> skipped because of previous errors
```

Exception:
```
System.InvalidOperationException : No account `f5.ac043.owner.<n>@example.com` signed in in this scenario.
```

Stack trace (AC project frames only):
```
at HT.AcceptanceTests.Support.F005.F005Arrange.AccountAsync(String email) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 128
at HT.AcceptanceTests.Support.F005.F005Arrange.ActorAsync(String token) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 149
at HT.AcceptanceTests.Support.F005.F005Actions.ReadAsync(String actorToken, String what) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Actions.cs:line 379
at HT.AcceptanceTests.StepDefinitions.F005.WhenSteps.WhenTheActorReads(String time, String actor, String what) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\StepDefinitions\F005\WhenSteps.cs:line 29
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F005.RecordingAndEvaluationFeature.TheHistorysRangeHas1To366DaysAndIsCheckedAfterTheGoal(String n, String actor, String first, String last, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Features\F005\recording-and-evaluation.feature:line 608
[non-AC frame(s) hidden]
```

### F005/AC-047 Another registered user gets "This goal does not exist." from every use case of this slice(n: "05", useCase: "history", goal: "the goal of `f5.ac047.owner.<n>@example.com`", __pickleIndex: "187", exampleTags: []) — failed

Step trace:
```
Given the registered user `f5.ac047.owner.05@example.com`, who created the custom habit `Reading`
-> done: GivenSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit("f5.ac047.owner.05...", "", "Reading") (0,0s)
And `f5.ac047.owner.05@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
-> done: GivenSteps.GivenTheUserCreatedAGoal("", "f5.ac047.owner.05...", "their custom habi...", "Build, duration 2...", "2026-11-18", "") (0,1s)
And `f5.ac047.owner.05@example.com` recorded 20 for 2026-11-18 with the note "Private note."
-> done: GivenSteps.GivenTheUserRecorded("", "f5.ac047.owner.05...", "20 for 2026-11-18...") (0,0s)
And the registered user `f5.ac047.other.05@example.com`
-> done: GivenSteps.GivenTheRegisteredUser("f5.ac047.other.05...") (0,1s)
When `f5.ac047.other.05@example.com` calls history with the goal of `f5.ac047.owner.<n>@example.com`
-> error: `f5.ac047.owner.<n>@example.com` created no goal in this scenario. (0,0s)
Then the result is "This goal does not exist." and it gives no record, note or state
-> skipped because of previous errors
And reading the history of the goal from 2026-11-18 to 2026-11-18 as `f5.ac047.owner.05@example.com` gives records (2026-11-18, 20) and the note "Private note."
-> skipped because of previous errors
```

Exception:
```
System.InvalidOperationException : `f5.ac047.owner.<n>@example.com` created no goal in this scenario.
```

Stack trace (AC project frames only):
```
at HT.AcceptanceTests.Support.F005.F005Arrange.GoalOf(String ownerEmail) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 258
at HT.AcceptanceTests.Support.F005.F005Arrange.GoalPhraseAsync(String phrase, String actorEmail) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 289
at HT.AcceptanceTests.Support.F005.F005Actions.CallAsync(String actorToken, String useCase, String goalPhrase) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Actions.cs:line 462
at HT.AcceptanceTests.StepDefinitions.F005.WhenSteps.WhenTheActorCallsAUseCase(String actor, String useCase, String goal) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\StepDefinitions\F005\WhenSteps.cs:line 36
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F005.RecordingAndEvaluationFeature.AnotherRegisteredUserGetsThisGoalDoesNotExist_FromEveryUseCaseOfThisSlice(String n, String useCase, String goal, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Features\F005\recording-and-evaluation.feature:line 675
[non-AC frame(s) hidden]
```

### F005/AC-010 Recording checks the acting user, the goal, the date, the value and the note in that order(n: "07", actor: "`f5.ac010.owner.<n>@example.com`", date: "2026-11-18", value: "3", note: "a text of 501 characters", result: "\"The note can have at most 500 characters.\"", __pickleIndex: "39", exampleTags: []) — failed

Step trace:
```
Given the registered user `f5.ac010.owner.07@example.com`, who created the custom habit `Reading`
-> done: GivenSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit("f5.ac010.owner.07...", "", "Reading") (0,0s)
And `f5.ac010.owner.07@example.com` created a goal on their custom habit `Reading` with Build, count 5 `pages`, 1 time per day and the start date 2026-11-18
-> done: GivenSteps.GivenTheUserCreatedAGoal("", "f5.ac010.owner.07...", "their custom habi...", "Build, count 5 `p...", "2026-11-18", "") (0,1s)
And the registered user `f5.ac010.other.07@example.com`
-> done: GivenSteps.GivenTheRegisteredUser("f5.ac010.other.07...") (0,1s)
And the administrator `f5.ac010.admin.07@example.com`
-> done: GivenSteps.GivenTheAdministrator("f5.ac010.admin.07...") (0,1s)
When `f5.ac010.owner.<n>@example.com` records 3 for 2026-11-18 with the note a text of 501 characters on the goal of `f5.ac010.owner.07@example.com`
-> error: No account `f5.ac010.owner.<n>@example.com` signed in in this scenario. (0,0s)
Then the result is "The note can have at most 500 characters."
-> skipped because of previous errors
And nothing is recorded
-> skipped because of previous errors
```

Exception:
```
System.InvalidOperationException : No account `f5.ac010.owner.<n>@example.com` signed in in this scenario.
```

Stack trace (AC project frames only):
```
at HT.AcceptanceTests.Support.F005.F005Arrange.AccountAsync(String email) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 128
at HT.AcceptanceTests.Support.F005.F005Arrange.ActorAsync(String token) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 149
at HT.AcceptanceTests.Support.F005.F005Actions.RunAsync(String actorToken, String action) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Actions.cs:line 239
at HT.AcceptanceTests.StepDefinitions.F005.WhenSteps.WhenTheActorWrites(String time, String actor, String action) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\StepDefinitions\F005\WhenSteps.cs:line 22
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F005.RecordingAndEvaluationFeature.RecordingChecksTheActingUserTheGoalTheDateTheValueAndTheNoteInThatOrder(String n, String actor, String date, String value, String note, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Features\F005\recording-and-evaluation.feature:line 128
[non-AC frame(s) hidden]
```

### F005/AC-043 The history's range has 1 to 366 days and is checked after the goal(n: "04", actor: "`f5.ac043.owner.<n>@example.com`", first: "2026-11-19", last: "2026-11-18", result: "the result is \"Choose a range of 1 to 366 days.\"", __pickleIndex: "164", exampleTags: []) — failed

Step trace:
```
Given the registered user `f5.ac043.owner.04@example.com`, who created the custom habit `Reading`
-> done: GivenSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit("f5.ac043.owner.04...", "", "Reading") (0,0s)
And `f5.ac043.owner.04@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
-> done: GivenSteps.GivenTheUserCreatedAGoal("", "f5.ac043.owner.04...", "their custom habi...", "Build, duration 2...", "2026-11-18", "") (0,1s)
And the registered user `f5.ac043.other.04@example.com`
-> done: GivenSteps.GivenTheRegisteredUser("f5.ac043.other.04...") (0,1s)
When `f5.ac043.owner.<n>@example.com` reads the history of the goal of `f5.ac043.owner.04@example.com` from 2026-11-19 to 2026-11-18
-> error: No account `f5.ac043.owner.<n>@example.com` signed in in this scenario. (0,0s)
Then the result is "Choose a range of 1 to 366 days."
-> skipped because of previous errors
```

Exception:
```
System.InvalidOperationException : No account `f5.ac043.owner.<n>@example.com` signed in in this scenario.
```

Stack trace (AC project frames only):
```
at HT.AcceptanceTests.Support.F005.F005Arrange.AccountAsync(String email) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 128
at HT.AcceptanceTests.Support.F005.F005Arrange.ActorAsync(String token) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 149
at HT.AcceptanceTests.Support.F005.F005Actions.ReadAsync(String actorToken, String what) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Actions.cs:line 379
at HT.AcceptanceTests.StepDefinitions.F005.WhenSteps.WhenTheActorReads(String time, String actor, String what) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\StepDefinitions\F005\WhenSteps.cs:line 29
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F005.RecordingAndEvaluationFeature.TheHistorysRangeHas1To366DaysAndIsCheckedAfterTheGoal(String n, String actor, String first, String last, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Features\F005\recording-and-evaluation.feature:line 608
[non-AC frame(s) hidden]
```

### F005/AC-047 Another registered user gets "This goal does not exist." from every use case of this slice(n: "03", useCase: "delete, with the owner's record identifier", goal: "the goal of `f5.ac047.owner.<n>@example.com`", __pickleIndex: "185", exampleTags: []) — failed

Step trace:
```
Given the registered user `f5.ac047.owner.03@example.com`, who created the custom habit `Reading`
-> done: GivenSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit("f5.ac047.owner.03...", "", "Reading") (0,0s)
And `f5.ac047.owner.03@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
-> done: GivenSteps.GivenTheUserCreatedAGoal("", "f5.ac047.owner.03...", "their custom habi...", "Build, duration 2...", "2026-11-18", "") (0,1s)
And `f5.ac047.owner.03@example.com` recorded 20 for 2026-11-18 with the note "Private note."
-> done: GivenSteps.GivenTheUserRecorded("", "f5.ac047.owner.03...", "20 for 2026-11-18...") (0,0s)
And the registered user `f5.ac047.other.03@example.com`
-> done: GivenSteps.GivenTheRegisteredUser("f5.ac047.other.03...") (0,1s)
When `f5.ac047.other.03@example.com` calls delete, with the owner's record identifier with the goal of `f5.ac047.owner.<n>@example.com`
-> error: `f5.ac047.owner.<n>@example.com` created no goal in this scenario. (0,0s)
Then the result is "This goal does not exist." and it gives no record, note or state
-> skipped because of previous errors
And reading the history of the goal from 2026-11-18 to 2026-11-18 as `f5.ac047.owner.03@example.com` gives records (2026-11-18, 20) and the note "Private note."
-> skipped because of previous errors
```

Exception:
```
System.InvalidOperationException : `f5.ac047.owner.<n>@example.com` created no goal in this scenario.
```

Stack trace (AC project frames only):
```
at HT.AcceptanceTests.Support.F005.F005Arrange.GoalOf(String ownerEmail) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 258
at HT.AcceptanceTests.Support.F005.F005Arrange.GoalPhraseAsync(String phrase, String actorEmail) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 289
at HT.AcceptanceTests.Support.F005.F005Actions.CallAsync(String actorToken, String useCase, String goalPhrase) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Actions.cs:line 462
at HT.AcceptanceTests.StepDefinitions.F005.WhenSteps.WhenTheActorCallsAUseCase(String actor, String useCase, String goal) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\StepDefinitions\F005\WhenSteps.cs:line 36
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F005.RecordingAndEvaluationFeature.AnotherRegisteredUserGetsThisGoalDoesNotExist_FromEveryUseCaseOfThisSlice(String n, String useCase, String goal, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Features\F005\recording-and-evaluation.feature:line 675
[non-AC frame(s) hidden]
```

### F005/AC-010 Recording checks the acting user, the goal, the date, the value and the note in that order(n: "05", actor: "`f5.ac010.owner.<n>@example.com`", date: "2026-11-19", value: "no value", note: "a text of 501 characters", result: "\"You cannot record a day that has not begun.\"", __pickleIndex: "37", exampleTags: []) — failed

Step trace:
```
Given the registered user `f5.ac010.owner.05@example.com`, who created the custom habit `Reading`
-> done: GivenSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit("f5.ac010.owner.05...", "", "Reading") (0,0s)
And `f5.ac010.owner.05@example.com` created a goal on their custom habit `Reading` with Build, count 5 `pages`, 1 time per day and the start date 2026-11-18
-> done: GivenSteps.GivenTheUserCreatedAGoal("", "f5.ac010.owner.05...", "their custom habi...", "Build, count 5 `p...", "2026-11-18", "") (0,1s)
And the registered user `f5.ac010.other.05@example.com`
-> done: GivenSteps.GivenTheRegisteredUser("f5.ac010.other.05...") (0,1s)
And the administrator `f5.ac010.admin.05@example.com`
-> done: GivenSteps.GivenTheAdministrator("f5.ac010.admin.05...") (0,1s)
When `f5.ac010.owner.<n>@example.com` records no value for 2026-11-19 with the note a text of 501 characters on the goal of `f5.ac010.owner.05@example.com`
-> error: No account `f5.ac010.owner.<n>@example.com` signed in in this scenario. (0,0s)
Then the result is "You cannot record a day that has not begun."
-> skipped because of previous errors
And nothing is recorded
-> skipped because of previous errors
```

Exception:
```
System.InvalidOperationException : No account `f5.ac010.owner.<n>@example.com` signed in in this scenario.
```

Stack trace (AC project frames only):
```
at HT.AcceptanceTests.Support.F005.F005Arrange.AccountAsync(String email) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 128
at HT.AcceptanceTests.Support.F005.F005Arrange.ActorAsync(String token) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 149
at HT.AcceptanceTests.Support.F005.F005Actions.RunAsync(String actorToken, String action) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Actions.cs:line 239
at HT.AcceptanceTests.StepDefinitions.F005.WhenSteps.WhenTheActorWrites(String time, String actor, String action) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\StepDefinitions\F005\WhenSteps.cs:line 22
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F005.RecordingAndEvaluationFeature.RecordingChecksTheActingUserTheGoalTheDateTheValueAndTheNoteInThatOrder(String n, String actor, String date, String value, String note, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Features\F005\recording-and-evaluation.feature:line 128
[non-AC frame(s) hidden]
```

### F005/AC-047 Another registered user gets "This goal does not exist." from every use case of this slice(n: "06", useCase: "recordable now", goal: "the goal of `f5.ac047.owner.<n>@example.com`", __pickleIndex: "188", exampleTags: []) — failed

Step trace:
```
Given the registered user `f5.ac047.owner.06@example.com`, who created the custom habit `Reading`
-> done: GivenSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit("f5.ac047.owner.06...", "", "Reading") (0,0s)
And `f5.ac047.owner.06@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
-> done: GivenSteps.GivenTheUserCreatedAGoal("", "f5.ac047.owner.06...", "their custom habi...", "Build, duration 2...", "2026-11-18", "") (0,1s)
And `f5.ac047.owner.06@example.com` recorded 20 for 2026-11-18 with the note "Private note."
-> done: GivenSteps.GivenTheUserRecorded("", "f5.ac047.owner.06...", "20 for 2026-11-18...") (0,0s)
And the registered user `f5.ac047.other.06@example.com`
-> done: GivenSteps.GivenTheRegisteredUser("f5.ac047.other.06...") (0,1s)
When `f5.ac047.other.06@example.com` calls recordable now with the goal of `f5.ac047.owner.<n>@example.com`
-> error: `f5.ac047.owner.<n>@example.com` created no goal in this scenario. (0,0s)
Then the result is "This goal does not exist." and it gives no record, note or state
-> skipped because of previous errors
And reading the history of the goal from 2026-11-18 to 2026-11-18 as `f5.ac047.owner.06@example.com` gives records (2026-11-18, 20) and the note "Private note."
-> skipped because of previous errors
```

Exception:
```
System.InvalidOperationException : `f5.ac047.owner.<n>@example.com` created no goal in this scenario.
```

Stack trace (AC project frames only):
```
at HT.AcceptanceTests.Support.F005.F005Arrange.GoalOf(String ownerEmail) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 258
at HT.AcceptanceTests.Support.F005.F005Arrange.GoalPhraseAsync(String phrase, String actorEmail) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 289
at HT.AcceptanceTests.Support.F005.F005Actions.CallAsync(String actorToken, String useCase, String goalPhrase) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Actions.cs:line 462
at HT.AcceptanceTests.StepDefinitions.F005.WhenSteps.WhenTheActorCallsAUseCase(String actor, String useCase, String goal) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\StepDefinitions\F005\WhenSteps.cs:line 36
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F005.RecordingAndEvaluationFeature.AnotherRegisteredUserGetsThisGoalDoesNotExist_FromEveryUseCaseOfThisSlice(String n, String useCase, String goal, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Features\F005\recording-and-evaluation.feature:line 675
[non-AC frame(s) hidden]
```

### F005/AC-043 The history's range has 1 to 366 days and is checked after the goal(n: "01", actor: "`f5.ac043.owner.<n>@example.com`", first: "2026-11-18", last: "2026-11-18", result: "it gives exactly one entry, 2026-11-18", __pickleIndex: "161", exampleTags: []) — failed

Step trace:
```
Given the registered user `f5.ac043.owner.01@example.com`, who created the custom habit `Reading`
-> done: GivenSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit("f5.ac043.owner.01...", "", "Reading") (0,0s)
And `f5.ac043.owner.01@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
-> done: GivenSteps.GivenTheUserCreatedAGoal("", "f5.ac043.owner.01...", "their custom habi...", "Build, duration 2...", "2026-11-18", "") (0,1s)
And the registered user `f5.ac043.other.01@example.com`
-> done: GivenSteps.GivenTheRegisteredUser("f5.ac043.other.01...") (0,1s)
When `f5.ac043.owner.<n>@example.com` reads the history of the goal of `f5.ac043.owner.01@example.com` from 2026-11-18 to 2026-11-18
-> error: No account `f5.ac043.owner.<n>@example.com` signed in in this scenario. (0,0s)
Then it gives exactly one entry, 2026-11-18
-> skipped because of previous errors
```

Exception:
```
System.InvalidOperationException : No account `f5.ac043.owner.<n>@example.com` signed in in this scenario.
```

Stack trace (AC project frames only):
```
at HT.AcceptanceTests.Support.F005.F005Arrange.AccountAsync(String email) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 128
at HT.AcceptanceTests.Support.F005.F005Arrange.ActorAsync(String token) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 149
at HT.AcceptanceTests.Support.F005.F005Actions.ReadAsync(String actorToken, String what) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Actions.cs:line 379
at HT.AcceptanceTests.StepDefinitions.F005.WhenSteps.WhenTheActorReads(String time, String actor, String what) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\StepDefinitions\F005\WhenSteps.cs:line 29
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F005.RecordingAndEvaluationFeature.TheHistorysRangeHas1To366DaysAndIsCheckedAfterTheGoal(String n, String actor, String first, String last, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Features\F005\recording-and-evaluation.feature:line 608
[non-AC frame(s) hidden]
```

### F005/AC-010 Recording checks the acting user, the goal, the date, the value and the note in that order(n: "04", actor: "`f5.ac010.owner.<n>@example.com`", date: "2026-11-17", value: "no value", note: "a text of 501 characters", result: "\"Nothing is scheduled on this date.\"", __pickleIndex: "36", exampleTags: []) — failed

Step trace:
```
Given the registered user `f5.ac010.owner.04@example.com`, who created the custom habit `Reading`
-> done: GivenSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit("f5.ac010.owner.04...", "", "Reading") (0,0s)
And `f5.ac010.owner.04@example.com` created a goal on their custom habit `Reading` with Build, count 5 `pages`, 1 time per day and the start date 2026-11-18
-> done: GivenSteps.GivenTheUserCreatedAGoal("", "f5.ac010.owner.04...", "their custom habi...", "Build, count 5 `p...", "2026-11-18", "") (0,1s)
And the registered user `f5.ac010.other.04@example.com`
-> done: GivenSteps.GivenTheRegisteredUser("f5.ac010.other.04...") (0,1s)
And the administrator `f5.ac010.admin.04@example.com`
-> done: GivenSteps.GivenTheAdministrator("f5.ac010.admin.04...") (0,1s)
When `f5.ac010.owner.<n>@example.com` records no value for 2026-11-17 with the note a text of 501 characters on the goal of `f5.ac010.owner.04@example.com`
-> error: No account `f5.ac010.owner.<n>@example.com` signed in in this scenario. (0,0s)
Then the result is "Nothing is scheduled on this date."
-> skipped because of previous errors
And nothing is recorded
-> skipped because of previous errors
```

Exception:
```
System.InvalidOperationException : No account `f5.ac010.owner.<n>@example.com` signed in in this scenario.
```

Stack trace (AC project frames only):
```
at HT.AcceptanceTests.Support.F005.F005Arrange.AccountAsync(String email) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 128
at HT.AcceptanceTests.Support.F005.F005Arrange.ActorAsync(String token) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Arrange.cs:line 149
at HT.AcceptanceTests.Support.F005.F005Actions.RunAsync(String actorToken, String action) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Support\F005\F005Actions.cs:line 239
at HT.AcceptanceTests.StepDefinitions.F005.WhenSteps.WhenTheActorWrites(String time, String actor, String action) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\StepDefinitions\F005\WhenSteps.cs:line 22
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F005.RecordingAndEvaluationFeature.RecordingChecksTheActingUserTheGoalTheDateTheValueAndTheNoteInThatOrder(String n, String actor, String date, String value, String note, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\issue-review-aa6233\tests\HT.AcceptanceTests\Features\F005\recording-and-evaluation.feature:line 128
[non-AC frame(s) hidden]
```

## Regression (other features)

Total: 652 · passed: 652 · failed: 0 · pending: 0 · skipped: 0

| Feature | Scenario | Tags | Outcome | Duration |
|---|---|---|---|---|
| F004 | Each field refusal of goal creation, with its message(n: "22", settings: "Reduce, count 2 `cups`, 2 times per day", message: "\"A Reduce goal with an amount has 1 time per peri"···, __pickleIndex: "29", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1548441 |
| F002 | Browsing with no match gives an empty result | @F002 @AC-007 @FR-008 @FR-009 @US1-AS7 | passed | 00:00:00.0964124 |
| F002 | The role and the identifier are checked before the fields(n: "03", actingUser: "the administrator `ac039.admin.03@example.com`", action: "edits an unknown identifier with the name `\"   \""···, result: "\"This catalog habit does not exist.\"", __pickleIndex: "151", exampleTags: []) | @F002 @AC-039 @FR-016 @US3-AS18 | passed | 00:00:00.1491608 |
| F002 | For custom habits the acting user and ownership come first, the name check last(n: "03", actingUser: "the registered user `ac057.b.03@example.com`", action: "edits an unknown identifier with the name `\"   \""···, result: "\"This custom habit does not exist.\"", __pickleIndex: "228", exampleTags: []) | @F002 @AC-057 @FR-005 @FR-016 @US4-AS11 | passed | 00:00:00.1536269 |
| F004 | An anonymous visitor gets no Home heading, no name change and no profile(n: "02", action: "changes their name to \"Yuri\"", __pickleIndex: "294", exampleTags: []) | @F004 @AC-091 @FR-023 @FR-028 @FR-029 @FR-030 @US11-AS8 | passed | 00:00:00.0017221 |
| F001 | Registration refuses a password that breaks the password rule(email: "\"seven.pw@example.com\"", password: "\"walkin4\"", whyItBreaksTheRule: "7 characters, with a digit", __pickleIndex: "2", exampleTags: []) | @F001 @AC-002 @FR-002 @US1-AS3 @US1-AS4 | passed | 00:00:00.0374592 |
| F004 | Browsing gives a registered user their goal count per habit and the tracked count(n: "01", search: "`A4.052 01`", result: "`A4.052 01 Reading` has the goal count 2, `A4.052 "···, __pickleIndex: "188", exampleTags: []) | @F004 @AC-052 @FR-031 @US6-AS1 | passed | 00:00:00.1634229 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"spaces.admin@example.com\"", password: "\"   \"", __pickleIndex: "68", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0002497 |
| F002 | A habit without suggested starting goals is shown without one | @F002 @AC-014 @FR-008 @FR-010 @US2-AS2 | passed | 00:00:00.0863028 |
| F004 | Reading a goal gives the version of its current date and the one later version(n: "06", schedule: "1 time per day", startDate: "2026-12-01", time: "`2026-11-18T12:00:00Z`", versions: "the time zone `America/New_York` from 2026-12-01 a"···, __pickleIndex: "248", exampleTags: []) | @F004 @AC-073 @FR-016 @FR-026 @US10-AS6 | passed | 00:00:00.1327666 |
| F004 | Each field refusal of goal creation, with its message(n: "14", settings: "Build, count 20 with a unit of 21 characters, 3 ti"···, message: "\"The unit can have at most 20 characters.\"", __pickleIndex: "21", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1535012 |
| F004 | The start date is today or up to 365 days later(n: "01", startDate: "the start date 2026-11-18", result: "creating succeeds", __pickleIndex: "52", exampleTags: []) | @F004 @AC-010 @FR-006 @FR-012 | passed | 00:00:00.1590344 |
| F004 | Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "01", submitted: "Build, count 20 `pages` with the duration unit hou"···, stored: "count 20 `pages`, no duration unit", targetText: "\"20 pages, 3 times a week\"", __pickleIndex: "66", exampleTags: []) | @F004 @AC-013 @FR-002 @FR-006 | passed | 00:00:00.1555155 |
| F002 | Browsing by intent returns only that intent(n: "03", intent: "both", sugar: "contains", water: "contains", __pickleIndex: "7", exampleTags: []) | @F002 @AC-004 @FR-009 @US1-AS4 | passed | 00:00:00.0866257 |
| F004 | Starting from a custom habit gives only Build or Reduce from its intent(n: "01", intent: "Build", __pickleIndex: "181", exampleTags: []) | @F004 @AC-046 @FR-007 @US5-AS1 | passed | 00:00:00.0793578 |
| F002 | Suggested starting goals are checked in list order, and each by its first failing field(n: "01", goals: "(1) no Build or Reduce, no measurement, no schedul"···, result: "\"Choose Build or Reduce.\"", position: "1", __pickleIndex: "154", exampleTags: []) | @F002 @AC-041 @FR-007 | passed | 00:00:00.0784035 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"   \"", password: "\"curator2026\"", __pickleIndex: "67", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0006514 |
| F001 | Registration refuses a password that breaks the password rule(email: "\"no.digit@example.com\"", password: "\"walkingfar\"", whyItBreaksTheRule: "10 characters, no digit", __pickleIndex: "3", exampleTags: []) | @F001 @AC-002 @FR-002 @US1-AS3 @US1-AS4 | passed | 00:00:00.0358576 |
| F004 | An anonymous visitor gets no goal use case(n: "06", action: "browses the catalog as the anonymous visitor with "···, __pickleIndex: "140", exampleTags: []) | @F004 @AC-032 @FR-022 @US3-AS4 | passed | 00:00:00.1581580 |
| F002 | An unknown identifier does not exist for the administrator(n: "01", action: "reads", __pickleIndex: "100", exampleTags: []) | @F002 @AC-032 @FR-013 @US3-AS12 | passed | 00:00:00.0762547 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "02", browserTimeZone: "no browser time zone", timeZone: "`UTC`", __pickleIndex: "223", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0793755 |
| F002 | The category filter counts only Published catalog habits(n: "02", actingUser: "the registered user `ac009.user.02@example.com`", __pickleIndex: "21", exampleTags: []) | @F002 @AC-009 @FR-002 @FR-009 @FR-024 @US1-AS9 | passed | 00:00:00.3423745 |
| F001 | Administrator creation checks for a missing password before it looks for a registered user | @F001 @AC-052 @FR-014 | passed | 00:00:00.0890853 |
| F004 | Each field refusal of goal creation, with its message(n: "04", settings: "Build, yes/no with the count unit `pages`, fixed d"···, message: "\"A yes/no goal has no amount or unit.\"", __pickleIndex: "11", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1538370 |
| F004 | Each field refusal of goal creation, with its message(n: "18", settings: "Build, yes/no, no number of times, per week", message: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "25", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1541806 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "03", count: "6", marks: "none marked", result: "\"A habit can have at most 5 suggested starting go"···, after: "no suggested starting goal", __pickleIndex: "111", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0801336 |
| F004 | The edges of every target range are accepted(n: "03", settings: "Reduce, count 0 `cups`, 1 time per day", targetText: "\"at most 0 cups a day\"", __pickleIndex: "46", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | passed | 00:00:00.1549116 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "04", browserTimeZone: "the browser time zone `FLE Standard Time`", timeZone: "`UTC`", __pickleIndex: "225", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0897431 |
| F004 | A per-week goal tells when its first week starts(n: "04", startDate: "2026-11-23", time: "`2026-11-18T10:00:00Z`", list: "first week starts 2026-11-23, start date 2026-11-2"···, __pickleIndex: "163", exampleTags: []) | @F004 @AC-039 @FR-014 @FR-016 @US4-AS4 | passed | 00:00:00.0903747 |
| F004 | A goal name has at most 60 characters after trimming(n: "03", name: "two spaces, a text of 60 characters and two spaces", result: "creating succeeds, and reading the goal gives the "···, __pickleIndex: "59", exampleTags: []) | @F004 @AC-011 @FR-006 @FR-009 | passed | 00:00:00.1581693 |
| F004 | Each goal follows a time zone change from its own next day or week(n: "03", schedule: "1 time per day", startDate: "2026-12-01", from: "2026-12-01", to: "2026-12-01", entries: "2026-12-01 from `2026-12-01T05:00:00Z` to `2026-12"···, __pickleIndex: "242", exampleTags: []) | @F004 @AC-072 @FR-026 @FR-027 @US10-AS5 | passed | 00:00:00.1210368 |
| F002 | The anonymous visitor cannot manage the catalog(n: "05", action: "publishes `A031 05 Published`", __pickleIndex: "98", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0942463 |
| F002 | Only the 24 habit icons are accepted(n: "16", icon: "`salad`", result: "success", __pickleIndex: "179", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0729088 |
| F002 | Unpublishing a Published habit takes it out of the catalog at once | @F002 @AC-021 @FR-011 @FR-015 @US3-AS4 | passed | 00:00:00.0958391 |
| F002 | The role and the identifier are checked before the fields(n: "02", actingUser: "the anonymous visitor", action: "creates a catalog habit with the name `\"   \"`", result: "\"You are not signed in.\"", __pickleIndex: "150", exampleTags: []) | @F002 @AC-039 @FR-016 @US3-AS18 | passed | 00:00:00.1515488 |
| F002 | For custom habits the acting user and ownership come first, the name check last(n: "05", actingUser: "the registered user `ac057.a.05@example.com`", action: "edits `Secret` to the name `TAKEN` and the icon `m"···, result: "\"Choose an icon.\"", __pickleIndex: "230", exampleTags: []) | @F002 @AC-057 @FR-005 @FR-016 @US4-AS11 | passed | 00:00:00.1593272 |
| F002 | Custom-habit names and descriptions are trimmed before they are checked and stored(n: "01", name: "`\"   Cold shower   \"`", description: "`\"  In the morning.  \"`", storedName: "`Cold shower`", storedDescription: "the description \"In the morning.\"", __pickleIndex: "256", exampleTags: []) | @F002 @AC-073 @FR-004 @FR-005 | passed | 00:00:00.0936361 |
| F004 | A habit that no goal uses is unpublished at once(n: "01", confirmation: "without confirming", __pickleIndex: "206", exampleTags: []) | @F004 @AC-059 @FR-019 @FR-033 @US7-AS4 | passed | 00:00:00.0798352 |
| F002 | Demo accounts that demo loading creates get their demo custom habits | @F002 @AC-064 @FR-024 @FR-031 @US6-AS5 | passed | 00:00:00.5212939 |
| F004 | Another user's goal does not exist for them(n: "02", action: "reads the evaluation calendar from 2026-11-18 to 2"···, goal: "`f4.ac029.a.02@example.com`'s goal", __pickleIndex: "125", exampleTags: []) | @F004 @AC-029 @FR-020 @US3-AS1 | passed | 00:00:00.2248600 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "08", settings: "Build, duration 2 hours, 3 times per day", targetText: "\"2 h, 3 times a day\"", __pickleIndex: "94", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0889315 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "02", action: "edits `Gone` by its identifier, with the name `Bac"···, __pickleIndex: "232", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.1585456 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "05", second: "Build, count with no amount and the unit `pages`, "···, result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "120", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0782119 |
| F004 | Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "06", submitted: "Build, count 20 `\"  pages  \"`, 3 times per week", stored: "count 20 `pages`", targetText: "\"20 pages, 3 times a week\"", __pickleIndex: "71", exampleTags: []) | @F004 @AC-013 @FR-002 @FR-006 | passed | 00:00:00.1587837 |
| F004 | A suggested starting goal position the habit does not have is refused(n: "01", position: "4", __pickleIndex: "4", exampleTags: []) | @F004 @AC-004 @FR-007 @US1-AS4 | passed | 00:00:00.1508375 |
| F002 | An Unpublished habit can be published again | @F002 @AC-022 @FR-011 @US3-AS5 | passed | 00:00:00.0880829 |
| F004 | A second change replaces a version not yet begun, and follows a version already in effect(n: "01", schedule: "3 times per week", startDate: "2026-11-16", from: "2026-11-16", to: "2026-11-29", versions: "the time zone `Europe/Kyiv` from 2026-11-16 and a "···, entries: "2026-11-16 to 2026-11-22 from `2026-11-15T22:00:00"···, __pickleIndex: "268", exampleTags: []) | @F004 @AC-081 @FR-026 @US10-AS10 | passed | 00:00:00.0992453 |
| F004 | The demo shows a goal on an Unpublished habit, a goal with its own name and an account name | @F004 @AC-069 @FR-028 @FR-034 @FR-040 @US9-AS5 | passed | 00:00:00.4551210 |
| F001 | A password change accepts a new password at the lower bound of the password rule | @F001 @AC-032 @FR-002 @FR-013 | passed | 00:00:00.2332701 |
| F002 | Names are ordered ordinally, ignoring case, not by the current culture | @F002 @AC-045 @FR-004 | passed | 00:00:00.0815020 |
| F002 | Suggested starting goals are checked in list order, and each by its first failing field(n: "03", goals: "(1) Build, yes/no, 1 time per day (marked); (2) Bu"···, result: "\"Choose fixed days or how many times.\"", position: "3", __pickleIndex: "156", exampleTags: []) | @F002 @AC-041 @FR-007 | passed | 00:00:00.0786104 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "05", settings: "Build, yes/no, 1 time per day", targetText: "\"once a day\"", __pickleIndex: "91", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0836261 |
| F002 | Each of the six categories can be given to a catalog habit(n: "06", category: "Other", __pickleIndex: "163", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0957742 |
| F004 | An account name is given only to its own account; administrators have their own | @F004 @AC-090 @FR-028 @FR-029 @US11-AS7 | passed | 00:00:00.2316360 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "06", fields: "Build, count 10 `pages`, fixed day Saturday", text: "\"10 pages, once a week\"", kind: "\"Count · fixed days Sat\"", __pickleIndex: "33", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.1029770 |
| F002 | Field checks come in a fixed order(n: "08", fields: "the name `A029 08 List`, the category Body, the in"···, result: "\"Mark one suggested starting goal for the card.\"", __pickleIndex: "86", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0723854 |
| F002 | Demo loading without a valid demo password creates no catalog and no custom habit(n: "01", demoPassword: "absent", result: "\"no demo password configured\"", __pickleIndex: "241", exampleTags: []) | @F002 @AC-063 @FR-030 @FR-031 @US6-AS4 | passed | 00:00:00.3179956 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "09", fields: "Build, count 5 with the unit `\"  \"` followed by "···, storedUnit: "that unit of 20 characters, without the spaces", __pickleIndex: "146", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0841899 |
| F002 | A registered user cannot manage the catalog(n: "05", action: "publishes `A030 05 Draft`", __pickleIndex: "92", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1611965 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "04", settings: "Build, yes/no, fixed days on all seven days", targetText: "\"daily\"", __pickleIndex: "90", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0858998 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "01", action: "starts a new goal from", habit: "`A4.014 01 Draft`", __pickleIndex: "72", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | passed | 00:00:00.1541542 |
| F001 | Registration creates a registered user | @F001 @AC-001 @FR-001 @FR-004 @FR-009 @US1-AS1 | passed | 00:00:00.0810155 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "09", fields: "Build, yes/no, fixed days Saturday and Sunday", text: "\"2 days a week\"", kind: "\"Yes/no · fixed days Sat, Sun\"", __pickleIndex: "36", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.1012084 |
| F004 | Use cases check the acting user, the role, the identifier and the fields, in this order(n: "04", actingUser: "the anonymous visitor", action: "creates a goal on `A4.035 04 Draft` with no Build "···, message: "\"You are not signed in.\"", __pickleIndex: "150", exampleTags: []) | @F004 @AC-035 @FR-023 | passed | 00:00:00.1604284 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "21", second: "Build, duration 15 minutes, 21 times per week", result: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "136", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0791969 |
| F004 | A custom habit that goals use cannot be deleted | @F004 @AC-048 @FR-035 @US5-AS3 | passed | 00:00:00.0897873 |
| F004 | Anonymous visitors and administrators get the catalog without goal information(n: "05", actingUser: "the anonymous visitor", action: "browses the catalog as the anonymous visitor with "···, result: "the result is \"You are not signed in.\"", __pickleIndex: "199", exampleTags: []) | @F004 @AC-055 @FR-032 @US6-AS4 | passed | 00:00:00.1570899 |
| F002 | Field checks come in a fixed order(n: "06", fields: "the name `A029 06 Taken`, the category Body, the i"···, result: "\"Choose an icon.\"", __pickleIndex: "84", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0752556 |
| F002 | A demo email that a person registered first keeps its owner's data untouched | @F002 @AC-067 @FR-031 @US6-AS8 | passed | 00:00:00.3611123 |
| F002 | Details with a custom habit's identifier give "not in the catalog"(n: "03", actingUser: "the anonymous visitor", __pickleIndex: "55", exampleTags: []) | @F002 @AC-017 @FR-010 @FR-024 @US2-AS5 | passed | 00:00:00.1517631 |
| F001 | Administrator creation refuses a password that breaks the password rule(email: "\"seventh.admin@example.com\"", password: "\"curatorship\"", whyItBreaksTheRule: "11 characters, no digit", __pickleIndex: "72", exampleTags: []) | @F001 @AC-046 @FR-002 @FR-014 @US4-AS7 | passed | 00:00:00.0781268 |
| F004 | The Home greeting follows the time of day in the account's time zone(n: "04", time: "`2026-11-18T10:00:00Z` (12:00 in Kyiv)", greeting: "\"Good afternoon, Yuri\"", __pickleIndex: "281", exampleTags: []) | @F004 @AC-086 @FR-029 @US11-AS3 | passed | 00:00:00.0779302 |
| F004 | A goal without a name of its own is named after its habit and target(n: "04", name: "the name `\"   \"`", goalName: "\"A4.016 04 Reading, 15 min, 5 days a week\"", own: "not its own", __pickleIndex: "85", exampleTags: []) | @F004 @AC-016 @FR-009 @FR-010 @US1-AS9 | passed | 00:00:00.2174267 |
| F002 | A name of 60 characters and a description of 500 characters are accepted | @F002 @AC-028 @FR-005 @US3-AS10 | passed | 00:00:00.0790995 |
| F001 | Administrator creation leaves an existing administrator unchanged | @F001 @AC-041 @FR-014 @US4-AS2 | passed | 00:00:00.1139990 |
| F001 | An account registered with spaces around the email has the trimmed email | @F001 @AC-061 @FR-003 @FR-012 | passed | 00:00:00.0803369 |
| F002 | Only the 24 habit icons are accepted(n: "05", icon: "`piggy-bank`", result: "success", __pickleIndex: "168", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0744729 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "03", action: "edits `Secret` by its identifier, with the name `S"···, __pickleIndex: "209", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1561041 |
| F001 | Registration refuses an email that already has an account, in any case and with spaces | @F001 @AC-007 @FR-003 @US1-AS2 | passed | 00:00:00.1138265 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "16", browserTimeZone: "the browser time zone `UTC`", timeZone: "`UTC`", __pickleIndex: "237", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0799324 |
| F002 | The anonymous visitor has no access to custom habits(n: "02", action: "reads `Secret` by its identifier", __pickleIndex: "215", exampleTags: []) | @F002 @AC-055 @FR-023 @US4-AS9 | passed | 00:00:00.0878981 |
| F002 | A registered user cannot manage the catalog(n: "03", action: "creates the catalog habit `A030 03 New`", __pickleIndex: "90", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1564360 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "03", name: "a name of 61 characters with the prefix `A027 03`", description: "none", category: "Body", intent: "Build", icon: "`book-open`", result: "\"The name can have at most 60 characters.\"", __pickleIndex: "72", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0781049 |
| F004 | Another user's goal does not exist for them(n: "03", action: "reads the goal", goal: "an identifier that the test creates and that belon"···, __pickleIndex: "126", exampleTags: []) | @F004 @AC-029 @FR-020 @US3-AS1 | passed | 00:00:00.2309635 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "19", settings: "Reduce, yes/no, fixed days Saturday and Sunday", targetText: "\"avoided on Sat, Sun\"", __pickleIndex: "105", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0928525 |
| F002 | Publish and unpublish check the acting user and the role before the identifier(n: "01", actingUser: "the registered user `ac046.user.01@example.com`", action: "publishes", result: "\"Only administrators can manage the catalog.\"", __pickleIndex: "193", exampleTags: []) | @F002 @AC-046 @FR-016 | passed | 00:00:00.0747568 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "01", count: "2", marks: "none marked", result: "\"Mark one suggested starting goal for the card.\"", after: "no suggested starting goal", __pickleIndex: "109", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0762364 |
| F001 | Registration checks the email before the password rule | @F001 @AC-009 @FR-003 | passed | 00:00:00.0008604 |
| F004 | A custom habit is looked up only among the acting user's custom habits(n: "01", action: "starts a new goal from", habit: "`f4.ac033.b.01@example.com`'s custom habit `Cold s"···, __pickleIndex: "141", exampleTags: []) | @F004 @AC-033 @FR-008 @US3-AS5 | passed | 00:00:00.2225645 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "04", search: "`\"a005 04 e\"`", morning: "does not contain", evening: "contains", __pickleIndex: "11", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.0840608 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "04", goal: "on `A4.008 04 Reading` with Build, yes/no with the"···, message: "\"A yes/no goal has no amount or unit.\"", __pickleIndex: "33", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1547175 |
| F004 | A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "01", schedule: "fixed days Sunday", date: "2027-03-28", entry: "2027-03-28 from `2027-03-27T22:00:00Z` to `2027-03"···, __pickleIndex: "164", exampleTags: []) | @F004 @AC-040 @FR-012 @FR-013 @US4-AS5 | passed | 00:00:00.0839892 |
| F004 | Each field refusal of goal creation, with its message(n: "01", settings: "no Build or Reduce, count 20 `pages`, 3 times per "···, message: "\"Choose Build or Reduce.\"", __pickleIndex: "8", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1499630 |
| F004 | An administrator gets no goal use case(n: "04", action: "reads `f4.ac031.user.04@example.com`'s goal", __pickleIndex: "132", exampleTags: []) | @F004 @AC-031 @FR-021 @US3-AS3 | passed | 00:00:00.1526254 |
| F004 | A per-week goal tells when its first week starts(n: "01", startDate: "2026-11-18", time: "`2026-11-18T10:00:00Z`", list: "first week starts 2026-11-23, no start date and no"···, __pickleIndex: "160", exampleTags: []) | @F004 @AC-039 @FR-014 @FR-016 @US4-AS4 | passed | 00:00:00.0844456 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "02", action: "starts a new goal from", habit: "`A4.014 02 Gone`", __pickleIndex: "73", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | passed | 00:00:01.2561514 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "01", search: "`\"  a005 01 morn  \"`", morning: "contains", evening: "does not contain", __pickleIndex: "8", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.0839894 |
| F001 | Demo loading without a demo password creates no account(demoPassword: "\"\"", __pickleIndex: "85", exampleTags: []) | @F001 @AC-055 @FR-016 @US7-AS3 | passed | 00:00:00.3021504 |
| F004 | A goal stays as it was created | @F004 @AC-028 @FR-017 @FR-018 | passed | 00:00:00.1680876 |
| F002 | Only the 24 habit icons are accepted(n: "17", icon: "`utensils`", result: "success", __pickleIndex: "180", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0757214 |
| F002 | The anonymous visitor cannot manage the catalog(n: "04", action: "edits `A031 04 Published` to the name `A031 04 Ren"···, __pickleIndex: "97", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0964978 |
| F004 | The catalog suggestion text of the starting values follows the goal rules | @F004 @AC-019 @FR-007 | passed | 00:00:00.1592146 |
| F004 | Browsing gives a registered user their goal count per habit and the tracked count(n: "02", search: "`A4.052 02 R`", result: "the result is `A4.052 02 Reading` only, with the g"···, __pickleIndex: "189", exampleTags: []) | @F004 @AC-052 @FR-031 @US6-AS1 | passed | 00:00:00.1639432 |
| F001 | The block ends exactly 5 minutes after the fifth failure | @F001 @AC-016 @FR-008 @US2-AS5 | passed | 00:00:00.2825752 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "01", name: "`\"   \"`", description: "none", category: "Body", intent: "Build", icon: "`book-open`", result: "\"Enter a name.\"", __pickleIndex: "70", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0754537 |
| F002 | An unknown identifier does not exist for the administrator(n: "02", action: "edits, with the name `A032 02 Name`, the category "···, __pickleIndex: "101", exampleTags: []) | @F002 @AC-032 @FR-013 @US3-AS12 | passed | 00:00:00.0786964 |
| F004 | Reading a goal that has not started gives its first version and attempt from its start date | @F004 @AC-026 @FR-005 @FR-016 @FR-018 | passed | 00:00:00.0834754 |
| F002 | The Home preview has fewer than four habits when fewer are Published | @F002 @AC-072 @FR-008 @US5-AS2 | passed | 00:00:00.3814861 |
| F001 | Administrator creation without an email or a password configures nothing(email: "absent", password: "absent", __pickleIndex: "63", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0001847 |
| F002 | A custom habit with a name of 60 characters and a description of 500 characters is accepted | @F002 @AC-059 @FR-005 | passed | 00:00:00.0821772 |
| F002 | Another registered user cannot reach a custom habit(n: "03", action: "deletes", __pickleIndex: "206", exampleTags: []) | @F002 @AC-053 @FR-021 @FR-022 @US4-AS7 | passed | 00:00:00.1586215 |
| F004 | Browsing gives a registered user their goal count per habit and the tracked count(n: "03", search: "`A4.052 03 Y`", result: "the result is `A4.052 03 Yoga` only, with the goal"···, __pickleIndex: "190", exampleTags: []) | @F004 @AC-052 @FR-031 @US6-AS1 | passed | 00:00:00.1769911 |
| F002 | Each of the six categories can be given to a catalog habit(n: "03", category: "Health", __pickleIndex: "160", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0955508 |
| F004 | "Today" is the date in the user's own time zone(n: "01", timeZone: "`Europe/Kyiv`", time: "`2026-11-16T21:30:00Z`", startDate: "2026-11-16", result: "creating succeeds", __pickleIndex: "176", exampleTags: []) | @F004 @AC-044 @FR-006 @FR-007 @FR-012 @US4-AS8 | passed | 00:00:00.0853869 |
| F004 | The days around a clock change meet at local midnight | @F004 @AC-041 @FR-012 @FR-013 @US4-AS5 | passed | 00:00:00.0846748 |
| F004 | A custom habit is looked up only among the acting user's custom habits(n: "02", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "`f4.ac033.b.02@example.com`'s custom habit `Cold s"···, __pickleIndex: "142", exampleTags: []) | @F004 @AC-033 @FR-008 @US3-AS5 | passed | 00:00:00.2243990 |
| F002 | Publish and unpublish check the acting user and the role before the identifier(n: "04", actingUser: "the anonymous visitor", action: "unpublishes", result: "\"You are not signed in.\"", __pickleIndex: "196", exampleTags: []) | @F002 @AC-046 @FR-016 | passed | 00:00:00.0751797 |
| F002 | Details with a custom habit's identifier give "not in the catalog"(n: "02", actingUser: "the administrator `ac017.admin.02@example.com`", __pickleIndex: "54", exampleTags: []) | @F002 @AC-017 @FR-010 @FR-024 @US2-AS5 | passed | 00:00:00.1544655 |
| F002 | Browsing returns only Published habits, with their card content, to every audience(n: "01", actingUser: "the anonymous visitor", __pickleIndex: "0", exampleTags: []) | @F002 @AC-001 @FR-008 @US1-AS1 | passed | 00:00:00.1685343 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "15", settings: "Build, yes/no, fixed days Monday, Wednesday and Fr"···, targetText: "\"3 days a week\"", __pickleIndex: "101", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0835791 |
| F002 | Only the 24 habit icons are accepted(n: "14", icon: "`languages`", result: "success", __pickleIndex: "177", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0770832 |
| F004 | Starting from a catalog habit without a position gives its marked suggested starting goal(n: "02", suggestions: "1. Build, count 20 `pages`, 3 times per week; 2. B"···, __pickleIndex: "1", exampleTags: []) | @F004 @AC-001 @FR-007 @US1-AS1 | passed | 00:00:00.1510902 |
| F004 | The Home greeting follows the time of day in the account's time zone(n: "01", time: "`2026-11-18T02:59:00Z` (04:59 in Kyiv)", greeting: "\"Good evening, Yuri\"", __pickleIndex: "278", exampleTags: []) | @F004 @AC-086 @FR-029 @US11-AS3 | passed | 00:00:00.0783339 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "14", second: "Reduce, duration 1000 hours, 1 time per day", result: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "129", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0799237 |
| F004 | Reading a goal gives the version of its current date and the one later version(n: "03", schedule: "fixed days Monday, Wednesday and Friday", startDate: "2026-11-16", time: "`2026-11-19T05:00:00Z`", versions: "the time zone `America/New_York` from 2026-11-19 a"···, __pickleIndex: "245", exampleTags: []) | @F004 @AC-073 @FR-016 @FR-026 @US10-AS6 | passed | 00:00:00.1209322 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "05", value: "`EST`", __pickleIndex: "253", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.0833606 |
| F004 | A user with no goals gets an empty list | @F004 @AC-027 @FR-015 @US2-AS5 | passed | 00:00:00.0785510 |
| F002 | Field checks refuse a custom habit with the same messages(n: "07", name: "`Menu icon`", description: "none", category: "Body", intent: "Build", icon: "`menu`", result: "\"Choose an icon.\"", __pickleIndex: "225", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0852889 |
| F004 | Each field refusal of goal creation, with its message(n: "17", settings: "Build, yes/no, fixed days with no day", message: "\"Choose at least one day.\"", __pickleIndex: "24", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1533966 |
| F001 | Registration refuses an email that is not valid(email: "\"   \"", whyItIsNotValid: "only spaces, empty after trimming", __pickleIndex: "20", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0025496 |
| F001 | Registration accepts the shortest valid email and a domain of three parts(email: "\"a@b.c\"", __pickleIndex: "21", exampleTags: []) | @F001 @AC-005 @FR-001 @FR-003 | passed | 00:00:00.0705310 |
| F002 | Names and descriptions are trimmed before they are checked and stored(n: "02", name: "`\"A070 02 Blank\"`", description: "`\"   \"`", storedName: "`A070 02 Blank`", storedDescription: "no description", __pickleIndex: "251", exampleTags: []) | @F002 @AC-070 @FR-004 @FR-005 | passed | 00:00:00.0742480 |
| F001 | A blocked account still fails with the correct password 4 minutes 59 seconds after the fifth failure | @F001 @AC-015 @FR-007 @FR-008 @US2-AS4 | passed | 00:00:00.2185594 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "03", goal: "on `A4.008 03 Reading` with no Build or Reduce, no"···, message: "\"Choose Build or Reduce.\"", __pickleIndex: "32", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1515181 |
| F002 | A Draft and an Unpublished habit can be edited and keep their state | @F002 @AC-069 @FR-012 | passed | 00:00:00.0920243 |
| F001 | A failed attempt during the block does not count after the block | @F001 @AC-022 @FR-008 | passed | 00:00:00.3987925 |
| F004 | A custom habit that no goal uses is deleted | @F004 @AC-049 @FR-019 @FR-035 @US5-AS4 | passed | 00:00:00.1163793 |
| F002 | Only the 24 habit icons are accepted(n: "23", icon: "`sparkles`", result: "success", __pickleIndex: "186", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0750606 |
| F004 | The edges of every target range are accepted(n: "08", settings: "Build, duration 999 hours, 1 time per day", targetText: "\"999 h a day\"", __pickleIndex: "51", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | passed | 00:00:00.1555933 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "07", settings: "Build, yes/no, 1 time per week", targetText: "\"once a week\"", __pickleIndex: "93", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0834775 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "05", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "`A4.014 05 Draft`", __pickleIndex: "76", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | passed | 00:00:00.2130631 |
| F001 | Administrator creation without an email or a password configures nothing(email: "absent", password: "\"curator2026\"", __pickleIndex: "65", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0001689 |
| F004 | The calendar range has 1 to 366 days(n: "04", from: "2026-11-18", to: "2026-11-18", result: "a calendar of 1 commitment, on 2026-11-18", __pickleIndex: "175", exampleTags: []) | @F004 @AC-043 @FR-013 @US4-AS7 | passed | 00:00:00.0864184 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "23", settings: "Reduce, yes/no, fixed days Wednesday", targetText: "\"avoided on Wed\"", __pickleIndex: "109", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0873392 |
| F002 | A user's custom-habit name is taken, ignoring case and spaces(n: "02", action: "edits `Evening walk`", __pickleIndex: "202", exampleTags: []) | @F002 @AC-051 @FR-004 @FR-005 @US4-AS5 | passed | 00:00:00.0833484 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "10", fields: "Build, count 2 `glasses`, 3 times per day", text: "\"2 glasses, 3 times a day\"", kind: "\"Count · flexible\"", __pickleIndex: "37", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0999107 |
| F001 | A password change with a wrong current password is refused | @F001 @AC-033 @FR-013 @US6-AS2 | passed | 00:00:00.1802149 |
| F004 | A habit that changed between starting and creating is looked up again(n: "02", habit: "their custom habit `Cold shower`", change: "`f4.ac015.user.02@example.com` deleted their custo"···, message: "\"This custom habit does not exist.\"", __pickleIndex: "81", exampleTags: []) | @F004 @AC-015 @FR-008 @FR-034 | passed | 00:00:00.1556017 |
| F004 | A fixed-days goal starting on an unscheduled day begins at its next scheduled day | @F004 @AC-045 @FR-003 @FR-013 | passed | 00:00:00.0877060 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "13", settings: "Reduce, yes/no, 3 times per week", targetText: "\"at most 3 times a week\"", __pickleIndex: "99", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0915804 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "06", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "`A4.014 06 Gone`", __pickleIndex: "77", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | passed | 00:00:00.1933753 |
| F002 | Field checks come in a fixed order(n: "04", fields: "the name `A029 04 Cat`, no category, no intent", result: "\"Choose a category.\"", __pickleIndex: "82", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0745917 |
| F002 | Publishing a Published habit is a success that changes nothing | @F002 @AC-024 @FR-011 @US3-AS7 | passed | 00:00:00.0893639 |
| F002 | Only the 24 habit icons are accepted(n: "07", icon: "`coffee`", result: "success", __pickleIndex: "170", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0758354 |
| F002 | Field checks refuse a custom habit with the same messages(n: "04", name: "`No category`", description: "none", category: "none", intent: "Build", icon: "`droplet`", result: "\"Choose a category.\"", __pickleIndex: "222", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0910855 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "07", second: "Build, count 1000 `pages`, 1 time per day", result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "122", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0803261 |
| F002 | The anonymous visitor cannot manage the catalog(n: "06", action: "unpublishes `A031 06 Published`", __pickleIndex: "99", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0951893 |
| F004 | Anonymous visitors and administrators get the catalog without goal information(n: "04", actingUser: "`f4.ac055.admin.04@example.com`", action: "reads the details of `A4.055 04 Reading` as `f4.ac"···, result: "the details have no goal count and no goals", __pickleIndex: "198", exampleTags: []) | @F004 @AC-055 @FR-032 @US6-AS4 | passed | 00:00:00.1571430 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "01", actingUser: "the anonymous visitor", habit: "`A016 01 Draft habit`", __pickleIndex: "44", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1617708 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "04", action: "reads a custom habit with the identifier of `A058 "···, __pickleIndex: "234", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.1569027 |
| F002 | Each of the six categories can be given to a catalog habit(n: "01", category: "Body", __pickleIndex: "158", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0983372 |
| F002 | An edit of a Published habit is public on the next read | @F002 @AC-020 @FR-012 @US3-AS3 | passed | 00:00:00.0892590 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"sixth.admin@example.com\"", password: "\"\"", __pickleIndex: "69", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0018906 |
| F004 | The Home greeting follows the time of day in the account's time zone(n: "06", time: "`2026-11-18T16:00:00Z` (18:00 in Kyiv)", greeting: "\"Good evening, Yuri\"", __pickleIndex: "283", exampleTags: []) | @F004 @AC-086 @FR-029 @US11-AS3 | passed | 00:00:00.0828302 |
| F004 | A habit's details give the user's goal count and their goals on it, in name order | @F004 @AC-054 @FR-031 @US6-AS3 | passed | 00:00:00.1888273 |
| F004 | A per-week goal tells when its first week starts(n: "03", startDate: "2026-11-18", time: "`2026-11-23T00:00:00Z`", list: "no first week starts, no start date and no next co"···, __pickleIndex: "162", exampleTags: []) | @F004 @AC-039 @FR-014 @FR-016 @US4-AS4 | passed | 00:00:00.0869044 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "08", browserTimeZone: "the browser time zone `GMT`", timeZone: "`UTC`", __pickleIndex: "229", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0930470 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "06", value: "`+02:00`", __pickleIndex: "254", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.0826437 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "10", fields: "Build, duration 1 hours, fixed day Sunday", storedUnit: "hours", __pickleIndex: "147", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0848522 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "08", actingUser: "the administrator `ac016.admin.08@example.com`", habit: "`A016 08 Old habit`", __pickleIndex: "51", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1704797 |
| F004 | An anonymous visitor gets no Home heading, no name change and no profile(n: "03", action: "reads their profile", __pickleIndex: "295", exampleTags: []) | @F004 @AC-091 @FR-023 @FR-028 @FR-029 @FR-030 @US11-AS8 | passed | 00:00:00.0010510 |
| F002 | Only the 24 habit icons are accepted(n: "18", icon: "`cigarette-off`", result: "success", __pickleIndex: "181", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0741982 |
| F002 | Demo loading run again leaves the seed catalog unchanged | @F002 @AC-061 @FR-030 @US6-AS2 | passed | 00:00:00.4010216 |
| F001 | Sign-in matches the email whatever its case | @F001 @AC-010 @FR-005 @FR-006 @US2-AS1 | passed | 00:00:00.0798170 |
| F004 | A per-day goal has one evaluation period per day | @F004 @AC-037 @FR-003 @FR-013 @US4-AS2 | passed | 00:00:00.0890632 |
| F001 | A registered user gets only their own account details | @F001 @AC-029 @FR-010 @FR-012 @US3-AS4 | passed | 00:00:00.1166807 |
| F004 | Unpublishing a habit that goals use needs confirmation and gives only the number of goals(n: "03", goals: "`f4.ac057.a.03@example.com` created two goals and "···, count: "3", text: "\"3 goals use this habit.\"", __pickleIndex: "204", exampleTags: []) | @F004 @AC-057 @FR-033 @US7-AS1 @US7-AS3 @US7-AS5 | passed | 00:00:00.2274395 |
| F001 | Sign-in with a wrong password fails with the single message | @F001 @AC-013 @FR-007 @US2-AS3 | passed | 00:00:00.0726149 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "04", action: "deletes `Secret` by its identifier", __pickleIndex: "210", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1579215 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@.example.com\"", whyItIsNotValid: "empty domain part before two non-empty ones", __pickleIndex: "14", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002698 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "01", action: "reads `Gone` by its identifier", __pickleIndex: "231", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.1788508 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "06", name: "`A027 06 No intent`", description: "none", category: "Body", intent: "none", icon: "`book-open`", result: "\"Choose Build or Reduce.\"", __pickleIndex: "75", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0760311 |
| F004 | Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "04", submitted: "Build, yes/no, fixed days Monday and Wednesday, wi"···, stored: "fixed days Monday and Wednesday, no number of time"···, targetText: "\"2 days a week\"", __pickleIndex: "69", exampleTags: []) | @F004 @AC-013 @FR-002 @FR-006 | passed | 00:00:00.1553134 |
| F004 | Each field refusal of goal creation, with its message(n: "19", settings: "Build, yes/no, 0 times per week", message: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "26", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1557054 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "12", goal: "on `A4.008 12 Reading` with Build, yes/no, fixed d"···, message: "\"Choose a start date.\"", __pickleIndex: "41", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1533956 |
| F002 | Custom habits never appear in browsing, for any audience(n: "02", actingUser: "the registered user `ac006.other.02@example.com`", __pickleIndex: "15", exampleTags: []) | @F002 @AC-006 @FR-024 @US1-AS6 | passed | 00:00:00.2671570 |
| F002 | An unknown identifier does not exist for the administrator(n: "04", action: "unpublishes", __pickleIndex: "103", exampleTags: []) | @F002 @AC-032 @FR-013 @US3-AS12 | passed | 00:00:00.0785285 |
| F002 | A custom habit is not a catalog habit for the administrator(n: "04", action: "unpublishes", __pickleIndex: "107", exampleTags: []) | @F002 @AC-033 @FR-013 @FR-024 @US3-AS13 | passed | 00:00:00.1520242 |
| F004 | "Today" follows the new time zone at once | @F004 @AC-076 @FR-012 @FR-026 | passed | 00:00:00.0881261 |
| F002 | The category filter counts only Published catalog habits(n: "03", actingUser: "the administrator `ac009.admin.03@example.com`", __pickleIndex: "22", exampleTags: []) | @F002 @AC-009 @FR-002 @FR-009 @FR-024 @US1-AS9 | passed | 00:00:00.3483883 |
| F001 | The end of a block resets the count of failed sign-ins | @F001 @AC-018 @FR-008 @US2-AS7 | passed | 00:00:00.3926584 |
| F001 | An attempt during the block does not extend it | @F001 @AC-017 @FR-008 @US2-AS6 | passed | 00:00:00.2559596 |
| F002 | The category filter counts only Published catalog habits(n: "01", actingUser: "the anonymous visitor", __pickleIndex: "20", exampleTags: []) | @F002 @AC-009 @FR-002 @FR-009 @FR-024 @US1-AS9 | passed | 00:00:00.3373413 |
| F002 | Another registered user cannot reach a custom habit(n: "01", action: "reads", __pickleIndex: "204", exampleTags: []) | @F002 @AC-053 @FR-021 @FR-022 @US4-AS7 | passed | 00:00:00.1557102 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "14", browserTimeZone: "the browser time zone `US/Eastern`", timeZone: "`US/Eastern`", __pickleIndex: "235", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0795975 |
| F002 | A registered user edits every field of their custom habit | @F002 @AC-049 @FR-004 @FR-020 @US4-AS3 | passed | 00:00:00.1010952 |
| F001 | Five failed attempts during the block do not start a new block | @F001 @AC-023 @FR-008 | passed | 00:00:00.2760428 |
| F002 | Names and descriptions are trimmed before they are checked and stored(n: "04", name: "`\"A070 04 Long\"`", description: "`\"  \"` + a text of 500 characters + `\"  \"` (50"···, storedName: "`A070 04 Long`", storedDescription: "the 500-character description", __pickleIndex: "253", exampleTags: []) | @F002 @AC-070 @FR-004 @FR-005 | passed | 00:00:00.0867883 |
| F002 | A user's list holds only their own custom habits, in name order ignoring case | @F002 @AC-048 @FR-004 @FR-020 @US4-AS2 | passed | 00:00:00.2372947 |
| F004 | Each field refusal of goal creation, with its message(n: "12", settings: "Build, count 20 with the unit `\"   \"`, 3 times p"···, message: "\"Enter a unit, for example pages.\"", __pickleIndex: "19", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1501533 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "11", settings: "Reduce, count 0 `cigarettes`, 1 time per day", targetText: "\"at most 0 cigarettes a day\"", __pickleIndex: "97", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0832907 |
| F004 | Each field refusal of goal creation, with its message(n: "03", settings: "Build, yes/no with the amount 5, fixed days Monday", message: "\"A yes/no goal has no amount or unit.\"", __pickleIndex: "10", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1496068 |
| F004 | A renamed catalog habit renames the goals based on it | @F004 @AC-062 @FR-010 @FR-016 @US7-AS7 | passed | 00:00:00.1637936 |
| F001 | A successful sign-in resets the count of failed sign-ins | @F001 @AC-019 @FR-008 @US2-AS8 | passed | 00:00:00.3945061 |
| F002 | Publish and unpublish check the acting user and the role before the identifier(n: "02", actingUser: "the registered user `ac046.user.02@example.com`", action: "unpublishes", result: "\"Only administrators can manage the catalog.\"", __pickleIndex: "194", exampleTags: []) | @F002 @AC-046 @FR-016 | passed | 00:00:00.0782048 |
| F004 | A Reduce goal with an amount has 1 time per period(n: "01", settings: "Reduce, count 2 `cups`, 1 time per week", result: "creating succeeds", __pickleIndex: "60", exampleTags: []) | @F004 @AC-012 @FR-004 @FR-006 | passed | 00:00:00.1554555 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "03", settings: "Reduce, count 1 `sweet`, 1 time per day", targetText: "\"at most 1 sweet a day\"", __pickleIndex: "89", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0854406 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "20", second: "Build, duration 15 minutes, 0 times per week", result: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "135", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0811637 |
| F004 | A goal on an Unpublished habit keeps working, and no new goal can use that habit(n: "02", actingUser: "`f4.ac060.owner.02@example.com`", action: "reads the goal", result: "the result has the habit name `A4.060 02 Reading`,"···, __pickleIndex: "209", exampleTags: []) | @F004 @AC-060 @FR-016 @FR-034 @US7-AS6 | passed | 00:00:00.2329045 |
| F002 | Custom habits never appear in browsing, for any audience(n: "01", actingUser: "the registered user `ac006.owner.01@example.com`", __pickleIndex: "14", exampleTags: []) | @F002 @AC-006 @FR-024 @US1-AS6 | passed | 00:00:00.2709520 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "14", fields: "Build, yes/no, fixed days Monday, Tuesday and Sund"···, text: "\"3 days a week\"", kind: "\"Yes/no · fixed days Mon, Tue, Sun\"", __pickleIndex: "41", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.1045601 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "05", name: "`A027 05 No category`", description: "none", category: "none", intent: "Build", icon: "`book-open`", result: "\"Choose a category.\"", __pickleIndex: "74", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0788454 |
| F004 | An anonymous visitor gets no goal use case(n: "03", action: "lists their goals", __pickleIndex: "137", exampleTags: []) | @F004 @AC-032 @FR-022 @US3-AS4 | passed | 00:00:00.1510621 |
| F004 | A goal keeps the values it was created with, whatever happens to the suggested starting goal | @F004 @AC-092 @FR-007 | passed | 00:00:00.1772253 |
| F004 | A new version's first period that has already ended is kept | @F004 @AC-083 @FR-027 | passed | 00:00:00.0895731 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "12", fields: "Build, duration 20 minutes, 1 time per day", text: "\"20 min a day\"", kind: "\"Duration · flexible\"", __pickleIndex: "39", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.1045204 |
| F004 | Use cases check the acting user, the role, the identifier and the fields, in this order(n: "02", actingUser: "`f4.ac035.admin.02@example.com`", action: "reads the evaluation calendar from 2026-11-24 to 2"···, message: "\"Goals are for registered users.\"", __pickleIndex: "148", exampleTags: []) | @F004 @AC-035 @FR-023 | passed | 00:00:00.1531397 |
| F004 | A per-week goal has one evaluation period per Monday-to-Sunday week, from its first full week(n: "01", startDate: "2026-11-18", from: "2026-11-16", to: "2026-12-06", periods: "2 periods: 2026-11-23 to 2026-11-29 from `2026-11-"···, __pickleIndex: "156", exampleTags: []) | @F004 @AC-038 @FR-013 @US4-AS3 | passed | 00:00:00.0824216 |
| F004 | An anonymous visitor gets no Home heading, no name change and no profile(n: "01", action: "reads the Home heading", __pickleIndex: "293", exampleTags: []) | @F004 @AC-091 @FR-023 @FR-028 @FR-029 @FR-030 @US11-AS8 | passed | 00:00:00.0003542 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "19", second: "Build, duration 15 minutes, fixed days with no day", result: "\"Choose at least one day.\"", __pickleIndex: "134", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0799166 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "02", action: "reads `Secret` by its identifier", __pickleIndex: "208", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1573707 |
| F001 | Registration refuses a password that breaks the password rule(email: "\"eight.nodigit@example.com\"", password: "\"walkingx\"", whyItBreaksTheRule: "8 characters, no digit", __pickleIndex: "4", exampleTags: []) | @F001 @AC-002 @FR-002 @US1-AS3 @US1-AS4 | passed | 00:00:00.0363596 |
| F001 | Failed sign-ins for an email that has no account block nothing | @F001 @AC-025 @FR-007 @FR-008 | passed | 00:00:00.0813320 |
| F001 | Demo loading without a demo password creates no account(demoPassword: "\"   \"", __pickleIndex: "86", exampleTags: []) | @F001 @AC-055 @FR-016 @US7-AS3 | passed | 00:00:00.3146357 |
| F004 | Listing gives every goal with its summary, in name order | @F004 @AC-021 @FR-014 @FR-015 @US2-AS1 | passed | 00:00:00.1707670 |
| F004 | An administrator gets no goal use case(n: "05", action: "reads the evaluation calendar of `f4.ac031.user.05"···, __pickleIndex: "133", exampleTags: []) | @F004 @AC-031 @FR-021 @US3-AS3 | passed | 00:00:00.1540640 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "10", settings: "Build, count 10 `pages`, 1 time per day", targetText: "\"10 pages a day\"", __pickleIndex: "96", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0843598 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "03", search: "`\"ning pa\"`", morning: "contains", evening: "does not contain", __pickleIndex: "10", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.0874860 |
| F002 | Only the 24 habit icons are accepted(n: "22", icon: "`sprout`", result: "success", __pickleIndex: "185", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0748479 |
| F002 | A demo email held by an administrator gets no custom habit | @F002 @AC-068 @FR-025 @FR-031 | passed | 00:00:00.3569159 |
| F002 | Only the 24 habit icons are accepted(n: "21", icon: "`receipt`", result: "success", __pickleIndex: "184", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0751421 |
| F001 | A password of only spaces creates no account | @F001 @AC-044 @FR-014 @US4-AS5 | passed | 00:00:00.0784746 |
| F004 | The edges of every target range are accepted(n: "01", settings: "Build, count 1 `pages`, 3 times per week", targetText: "\"1 pages, 3 times a week\"", __pickleIndex: "44", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | passed | 00:00:00.1549151 |
| F002 | Field checks refuse a custom habit with the same messages(n: "01", name: "`\"   \"`", description: "none", category: "Body", intent: "Build", icon: "`droplet`", result: "\"Enter a name.\"", __pickleIndex: "219", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0865139 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "07", action: "edits `Secret` by its identifier, with the name `\"···, __pickleIndex: "213", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1590843 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "02", search: "`\"MORNING\"`", morning: "contains", evening: "does not contain", __pickleIndex: "9", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.0880305 |
| F002 | The category filter starts with All and follows the fixed order | @F002 @AC-008 @FR-002 @FR-009 @US1-AS8 | passed | 00:00:00.0936187 |
| F002 | Custom habits never appear in browsing, for any audience(n: "04", actingUser: "the anonymous visitor", __pickleIndex: "17", exampleTags: []) | @F002 @AC-006 @FR-024 @US1-AS6 | passed | 00:00:00.2625216 |
| F004 | The start date is today or up to 365 days later(n: "05", startDate: "no start date", result: "\"Choose a start date.\"", __pickleIndex: "56", exampleTags: []) | @F004 @AC-010 @FR-006 @FR-012 | passed | 00:00:00.1522827 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "22", settings: "Reduce, yes/no, fixed days Sunday and Monday", targetText: "\"avoided on Mon, Sun\"", __pickleIndex: "108", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0886173 |
| F004 | Goals are listed by name ignoring case, then in creation order | @F004 @AC-022 @FR-009 @FR-015 | passed | 00:00:00.0962221 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "01", value: "`Mars/Olympus`", __pickleIndex: "249", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.0871915 |
| F002 | Field checks refuse a custom habit with the same messages(n: "06", name: "`No icon`", description: "none", category: "Body", intent: "Build", icon: "none", result: "\"Choose an icon.\"", __pickleIndex: "224", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0844229 |
| F004 | The Home greeting follows the time of day in the account's time zone(n: "05", time: "`2026-11-18T15:59:00Z` (17:59 in Kyiv)", greeting: "\"Good afternoon, Yuri\"", __pickleIndex: "282", exampleTags: []) | @F004 @AC-086 @FR-029 @US11-AS3 | passed | 00:00:00.0781298 |
| F002 | Each of the six categories can be given to a catalog habit(n: "05", category: "Home", __pickleIndex: "162", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0946084 |
| F004 | A Reduce goal with an amount has 1 time per period(n: "02", settings: "Reduce, duration 30 minutes, 2 times per week", result: "\"A Reduce goal with an amount has 1 time per peri"···, __pickleIndex: "61", exampleTags: []) | @F004 @AC-012 @FR-004 @FR-006 | passed | 00:00:00.1520353 |
| F004 | A goal on an Unpublished habit keeps working, and no new goal can use that habit(n: "05", actingUser: "`f4.ac060.other.05@example.com`", action: "creates a goal on `A4.060 05 Reading` with Build, "···, result: "the result is \"This habit is not in the catalog.\"···, __pickleIndex: "212", exampleTags: []) | @F004 @AC-060 @FR-016 @FR-034 @US7-AS6 | passed | 00:00:00.2254057 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "20", settings: "Reduce, yes/no, fixed days on all seven days", targetText: "\"avoided every day\"", __pickleIndex: "106", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0835549 |
| F004 | A habit that no goal uses is unpublished at once(n: "02", confirmation: "with confirmation", __pickleIndex: "207", exampleTags: []) | @F004 @AC-059 @FR-019 @FR-033 @US7-AS4 | passed | 00:00:00.0862950 |
| F004 | A demo goal finds its catalog habit by name ignoring case, and only "Coffee" may be Unpublished | @F004 @AC-067 @FR-040 | passed | 00:00:00.6820357 |
| F002 | Details give every suggested starting goal in order, with its text and kind | @F002 @AC-013 @FR-001 @FR-010 @FR-017 @US2-AS1 | passed | 00:00:00.0907637 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "07", count: "1", marks: "none marked", result: "\"Mark one suggested starting goal for the card.\"", after: "no suggested starting goal", __pickleIndex: "115", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0900026 |
| F002 | Browsing by intent returns only that intent(n: "01", intent: "Reduce", sugar: "contains", water: "does not contain", __pickleIndex: "5", exampleTags: []) | @F002 @AC-004 @FR-009 @US1-AS4 | passed | 00:00:00.0805974 |
| F001 | Five failed sign-ins in a row block however far apart they are | @F001 @AC-021 @FR-008 | passed | 00:00:00.2183469 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@example\"", whyItIsNotValid: "domain has one part", __pickleIndex: "9", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002548 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "01", fields: "Build, count 1 `page`, 1 time per day", storedUnit: "`page`", __pickleIndex: "138", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0795872 |
| F004 | Catalog goal counts are the acting user's own | @F004 @AC-034 @FR-032 @US3-AS6 | passed | 00:00:00.2417593 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "07", name: "`A027 07 No icon`", description: "none", category: "Body", intent: "Build", icon: "none", result: "\"Choose an icon.\"", __pickleIndex: "76", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0767473 |
| F001 | Demo loading accepts a demo password at the lower bound of the password rule | @F001 @AC-058 @FR-002 @FR-016 | passed | 00:00:00.3438320 |
| F004 | The calendar range has 1 to 366 days(n: "02", from: "2026-11-18", to: "2027-11-19", result: "\"Choose a range of 1 to 366 days.\"", __pickleIndex: "173", exampleTags: []) | @F004 @AC-043 @FR-013 @US4-AS7 | passed | 00:00:00.0869473 |
| F002 | Browsing orders habits by name, ignoring case | @F002 @AC-002 @FR-004 @FR-008 @US1-AS2 | passed | 00:00:00.0793114 |
| F001 | Registration accepts the shortest valid email and a domain of three parts(email: "\"ana@sub.example.com\"", __pickleIndex: "22", exampleTags: []) | @F001 @AC-005 @FR-001 @FR-003 | passed | 00:00:00.0740074 |
| F002 | Details with a custom habit's identifier give "not in the catalog"(n: "01", actingUser: "the registered user `ac017.owner.01@example.com`", __pickleIndex: "53", exampleTags: []) | @F002 @AC-017 @FR-010 @FR-024 @US2-AS5 | passed | 00:00:00.1492842 |
| F002 | Browsing returns only Published habits, with their card content, to every audience(n: "02", actingUser: "the registered user `ac001.user.02@example.com`", __pickleIndex: "1", exampleTags: []) | @F002 @AC-001 @FR-008 @US1-AS1 | passed | 00:00:00.1705453 |
| F004 | Anonymous visitors and administrators get the catalog without goal information(n: "03", actingUser: "the anonymous visitor", action: "reads the details of `A4.055 03 Reading` as the an"···, result: "the details have no goal count and no goals", __pickleIndex: "197", exampleTags: []) | @F004 @AC-055 @FR-032 @US6-AS4 | passed | 00:00:00.1566062 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "13", fields: "Build, yes/no, fixed days Monday to Saturday", text: "\"6 days a week\"", kind: "\"Yes/no · fixed days Mon–Sat\"", __pickleIndex: "40", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.1058440 |
| F004 | Starting from a suggested starting goal position gives that suggestion | @F004 @AC-002 @FR-007 @US1-AS2 | passed | 00:00:00.1564508 |
| F001 | Registration refuses a password that breaks the password rule(email: "\"short.pw@example.com\"", password: "\"walk4\"", whyItBreaksTheRule: "5 characters", __pickleIndex: "1", exampleTags: []) | @F001 @AC-002 @FR-002 @US1-AS3 @US1-AS4 | passed | 00:00:00.0384965 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "11", fields: "Build, yes/no, 2 times per day", text: "\"2 times a day\"", kind: "\"Yes/no · flexible\"", __pickleIndex: "38", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0983089 |
| F004 | An anonymous visitor gets no goal use case(n: "02", action: "creates a goal on `A4.032 02 Reading` with Build, "···, __pickleIndex: "136", exampleTags: []) | @F004 @AC-032 @FR-022 @US3-AS4 | passed | 00:00:00.1508328 |
| F001 | Administrator creation finds a registered user before it checks the password rule | @F001 @AC-050 @FR-014 | passed | 00:00:00.0790325 |
| F002 | Only the 24 habit icons are accepted(n: "10", icon: "`droplet`", result: "success", __pickleIndex: "173", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0743106 |
| F001 | A password change to a password that breaks the password rule is refused(email: "\"weak.new7@example.com\"", newPassword: "\"runnin4\"", whyItBreaksTheRule: "7 characters, with a digit", __pickleIndex: "53", exampleTags: []) | @F001 @AC-034 @FR-002 @FR-013 @US6-AS3 | passed | 00:00:00.1484503 |
| F004 | Reading a goal gives the version of its current date and the one later version(n: "04", schedule: "fixed days Monday, Wednesday and Friday", startDate: "2026-11-16", time: "`2026-11-20T12:00:00Z`", versions: "the time zone `America/New_York` from 2026-11-19 a"···, __pickleIndex: "246", exampleTags: []) | @F004 @AC-073 @FR-016 @FR-026 @US10-AS6 | passed | 00:00:00.1298343 |
| F002 | Only the 24 habit icons are accepted(n: "01", icon: "`book-open`", result: "success", __pickleIndex: "164", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0755017 |
| F002 | A registered user cannot manage the catalog(n: "04", action: "edits `A030 04 Draft` to the name `A030 04 Renamed"···, __pickleIndex: "91", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1581650 |
| F002 | For custom habits the acting user and ownership come first, the name check last(n: "02", actingUser: "the registered user `ac057.b.02@example.com`", action: "edits `Secret` of `ac057.a.02@example.com` with th"···, result: "\"This custom habit does not exist.\"", __pickleIndex: "227", exampleTags: []) | @F002 @AC-057 @FR-005 @FR-016 @US4-AS11 | passed | 00:00:00.1519915 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "06", search: "`\"   \"`", morning: "contains", evening: "contains", __pickleIndex: "13", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.1205324 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "08", second: "Build, duration with no amount and the unit minute"···, result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "123", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0817263 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "11", second: "Reduce, count 1000 `sweets`, 1 time per day", result: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "126", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0775793 |
| F004 | The Home greeting follows the time of day in the account's time zone(n: "07", time: "`2026-11-18T21:59:59Z` (23:59:59 in Kyiv)", greeting: "\"Good evening, Yuri\"", __pickleIndex: "284", exampleTags: []) | @F004 @AC-086 @FR-029 @US11-AS3 | passed | 00:00:00.0800565 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "09", browserTimeZone: "the browser time zone `+02:00`", timeZone: "`UTC`", __pickleIndex: "230", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0776101 |
| F004 | Unpublishing a habit that goals use needs confirmation and gives only the number of goals(n: "02", goals: "`f4.ac057.a.02@example.com` created one goal", count: "1", text: "\"1 goal uses this habit.\"", __pickleIndex: "203", exampleTags: []) | @F004 @AC-057 @FR-033 @US7-AS1 @US7-AS3 @US7-AS5 | passed | 00:00:00.2265927 |
| F002 | Only the 24 habit icons are accepted(n: "26", icon: "`trash-2`", result: "\"Choose an icon.\"", __pickleIndex: "189", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0741685 |
| F002 | Field checks come in a fixed order(n: "02", fields: "a name of 61 characters with the prefix `A029 02`,"···, result: "\"The name can have at most 60 characters.\"", __pickleIndex: "80", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0729741 |
| F004 | An anonymous visitor gets no goal use case(n: "05", action: "reads the evaluation calendar of `f4.ac032.user.05"···, __pickleIndex: "139", exampleTags: []) | @F004 @AC-032 @FR-022 @US3-AS4 | passed | 00:00:00.1588207 |
| F004 | The next commitment is the first commitment that has not ended(n: "04", time: "`2026-11-19T00:00:00Z`", next: "2026-11-20", __pickleIndex: "117", exampleTags: []) | @F004 @AC-023 @FR-014 @US2-AS2 | passed | 00:00:00.1175136 |
| F004 | A suggested starting goal position the habit does not have is refused(n: "02", position: "0", __pickleIndex: "5", exampleTags: []) | @F004 @AC-004 @FR-007 @US1-AS4 | passed | 00:00:00.1626457 |
| F002 | Demo loading without a valid demo password creates no catalog and no custom habit(n: "03", demoPassword: "`walk4`", result: "\"demo password does not meet the password rule\"", __pickleIndex: "243", exampleTags: []) | @F002 @AC-063 @FR-030 @FR-031 @US6-AS4 | passed | 00:00:00.3126706 |
| F004 | A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "05", schedule: "1 time per week", date: "2027-03-28", entry: "2027-03-22 to 2027-03-28 from `2027-03-21T22:00:00"···, __pickleIndex: "168", exampleTags: []) | @F004 @AC-040 @FR-012 @FR-013 @US4-AS5 | passed | 00:00:00.0868067 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "02", goal: "on `A4.008 02 Draft` with Build, no measurement, f"···, message: "\"This habit is not in the catalog.\"", __pickleIndex: "31", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1519459 |
| F002 | Each of the six categories can be given to a catalog habit(n: "02", category: "Mind", __pickleIndex: "159", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0944509 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"\"", password: "\"curator2026\"", __pickleIndex: "66", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0002899 |
| F001 | Administrator creation does not turn a registered user into an administrator | @F001 @AC-042 @FR-014 @US4-AS3 | passed | 00:00:00.0798985 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "21", settings: "Reduce, yes/no, fixed days Monday, Tuesday, Wednes"···, targetText: "\"avoided on Mon–Wed, Sat\"", __pickleIndex: "107", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0852763 |
| F002 | A user's custom-habit name is taken, ignoring case and spaces(n: "01", action: "creates a custom habit", __pickleIndex: "201", exampleTags: []) | @F002 @AC-051 @FR-004 @FR-005 @US4-AS5 | passed | 00:00:00.0846845 |
| F002 | A registered user cannot manage the catalog(n: "06", action: "unpublishes `A030 06 Draft`", __pickleIndex: "93", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1668628 |
| F004 | "Today" is the date in the user's own time zone(n: "02", timeZone: "`Europe/Kyiv`", time: "`2026-11-16T22:30:00Z`", startDate: "2026-11-17", result: "\"The start date cannot be in the past.\"", __pickleIndex: "177", exampleTags: []) | @F004 @AC-044 @FR-006 @FR-007 @FR-012 @US4-AS8 | passed | 00:00:00.0813098 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "05", goal: "on `A4.008 05 Reading` with Build, count 0 with no"···, message: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "34", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1551180 |
| F002 | Another registered user cannot reach a custom habit(n: "02", action: "edits, with the name `Mine now`, the category Body"···, __pickleIndex: "205", exampleTags: []) | @F002 @AC-053 @FR-021 @FR-022 @US4-AS7 | passed | 00:00:00.1502232 |
| F004 | The edges of every target range are accepted(n: "05", settings: "Build, count 5 with a unit of 20 characters, 1 tim"···, targetText: "\"5 uuuuuuuuuuuuuuuuuuuu a day\"", __pickleIndex: "48", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | passed | 00:00:00.1540760 |
| F002 | Only the 24 habit icons are accepted(n: "02", icon: "`person-standing`", result: "success", __pickleIndex: "165", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0755366 |
| F004 | Use cases check the acting user, the role, the identifier and the fields, in this order(n: "03", actingUser: "`f4.ac035.user.03@example.com`", action: "reads the evaluation calendar from 2026-11-24 to 2"···, message: "\"This goal does not exist.\"", __pickleIndex: "149", exampleTags: []) | @F004 @AC-035 @FR-023 | passed | 00:00:00.1504062 |
| F004 | A goal without a name of its own is named after its habit and target(n: "02", name: "the name \"Morning pages\"", goalName: "\"Morning pages\"", own: "its own", __pickleIndex: "83", exampleTags: []) | @F004 @AC-016 @FR-009 @FR-010 @US1-AS9 | passed | 00:00:00.1928494 |
| F004 | An administrator gets no goal use case(n: "06", action: "browses the catalog as `f4.ac031.admin.06@example."···, __pickleIndex: "134", exampleTags: []) | @F004 @AC-031 @FR-021 @US3-AS3 | passed | 00:00:00.1528399 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "06", second: "Build, count 0 `pages`, 1 time per day", result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "121", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0799532 |
| F004 | Use cases check the acting user, the role, the identifier and the fields, in this order(n: "01", actingUser: "the anonymous visitor", action: "reads the evaluation calendar from 2026-11-24 to 2"···, message: "\"You are not signed in.\"", __pickleIndex: "147", exampleTags: []) | @F004 @AC-035 @FR-023 | passed | 00:00:00.1501435 |
| F002 | The anonymous visitor has no access to custom habits(n: "01", action: "lists custom habits", __pickleIndex: "214", exampleTags: []) | @F002 @AC-055 @FR-023 @US4-AS9 | passed | 00:00:00.0920722 |
| F004 | A habit that changed between starting and creating is looked up again(n: "01", habit: "`A4.015 01 Reading`", change: "the administrator unpublished `A4.015 01 Reading` "···, message: "\"This habit is not in the catalog.\"", __pickleIndex: "80", exampleTags: []) | @F004 @AC-015 @FR-008 @FR-034 | passed | 00:00:00.1574033 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "02", fields: "Reduce, duration 2 hours, 1 time per day", text: "\"at most 2 h a day\"", kind: "\"Duration · flexible\"", __pickleIndex: "29", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0969909 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "17", second: "Build, duration 15 with no unit, 1 time per day", result: "\"Choose minutes or hours.\"", __pickleIndex: "132", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0786532 |
| F004 | A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "06", schedule: "1 time per week", date: "2027-10-31", entry: "2027-10-25 to 2027-10-31 from `2027-10-24T21:00:00"···, __pickleIndex: "169", exampleTags: []) | @F004 @AC-040 @FR-012 @FR-013 @US4-AS5 | passed | 00:00:00.0861509 |
| F002 | Category, intent and search apply together | @F002 @AC-010 @FR-009 | passed | 00:00:00.0869733 |
| F002 | Field checks come in a fixed order(n: "09", fields: "the name `a029 09 taken`, the category Body, the i"···, result: "\"A catalog habit with this name already exists.\"", __pickleIndex: "87", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0767977 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "05", action: "deletes a custom habit with the identifier of `A05"···, __pickleIndex: "235", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.1579200 |
| F002 | Only the 24 habit icons are accepted(n: "11", icon: "`bike`", result: "success", __pickleIndex: "174", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0736451 |
| F002 | Only the 24 habit icons are accepted(n: "03", icon: "`smartphone`", result: "success", __pickleIndex: "166", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0733823 |
| F002 | The anonymous visitor cannot manage the catalog(n: "03", action: "creates the catalog habit `A031 03 New`", __pickleIndex: "96", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0934757 |
| F002 | A registered user creates a custom habit | @F002 @AC-047 @FR-001 @FR-020 @US4-AS1 | passed | 00:00:00.0914156 |
| F004 | A custom habit is looked up only among the acting user's custom habits(n: "05", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "an identifier that the test creates and that belon"···, __pickleIndex: "145", exampleTags: []) | @F004 @AC-033 @FR-008 @US3-AS5 | passed | 00:00:00.2219156 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "06", fields: "Build, duration 999 hours, 1 time per week", storedUnit: "hours", __pickleIndex: "143", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0820565 |
| F004 | Each field refusal of goal creation, with its message(n: "02", settings: "Build, no measurement, 3 times per week", message: "\"Choose how it is measured.\"", __pickleIndex: "9", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1539496 |
| F002 | Only the 24 habit icons are accepted(n: "09", icon: "`dumbbell`", result: "success", __pickleIndex: "172", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0769687 |
| F002 | A Draft cannot be unpublished | @F002 @AC-023 @FR-011 @US3-AS6 | passed | 00:00:00.0789427 |
| F004 | An anonymous visitor gets no goal use case(n: "04", action: "reads `f4.ac032.user.04@example.com`'s goal", __pickleIndex: "138", exampleTags: []) | @F004 @AC-032 @FR-022 @US3-AS4 | passed | 00:00:00.1576218 |
| F004 | A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "03", schedule: "1 time per day", date: "2027-03-28", entry: "2027-03-28 from `2027-03-27T22:00:00Z` to `2027-03"···, __pickleIndex: "166", exampleTags: []) | @F004 @AC-040 @FR-012 @FR-013 @US4-AS5 | passed | 00:00:00.0848158 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "12", settings: "Build, yes/no, 2 times per day", targetText: "\"2 times a day\"", __pickleIndex: "98", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0838069 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "07", browserTimeZone: "the browser time zone `CET`", timeZone: "`UTC`", __pickleIndex: "228", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0779004 |
| F004 | The Home greeting follows the time of day in the account's time zone(n: "03", time: "`2026-11-18T09:59:00Z` (11:59 in Kyiv)", greeting: "\"Good morning, Yuri\"", __pickleIndex: "280", exampleTags: []) | @F004 @AC-086 @FR-029 @US11-AS3 | passed | 00:00:00.0776596 |
| F004 | A habit published again is in the catalog again for goals | @F004 @AC-061 @FR-034 | passed | 00:00:00.2305015 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@example..com\"", whyItIsNotValid: "empty domain part between two non-empty ones", __pickleIndex: "13", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0004132 |
| F004 | After a large move east, a version already begun is in effect, and a second change follows it | @F004 @AC-082 @FR-026 @FR-027 | passed | 00:00:00.0909467 |
| F002 | Field checks come in a fixed order(n: "05", fields: "the name `A029 05 Int`, the category Body, no inte"···, result: "\"Choose Build or Reduce.\"", __pickleIndex: "83", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0730861 |
| F004 | Each goal follows a time zone change from its own next day or week(n: "02", schedule: "3 times per week", startDate: "2026-11-16", from: "2026-11-16", to: "2026-11-29", entries: "2026-11-16 to 2026-11-22 from `2026-11-15T22:00:00"···, __pickleIndex: "241", exampleTags: []) | @F004 @AC-072 @FR-026 @FR-027 @US10-AS5 | passed | 00:00:00.1227520 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "04", state: "Draft", action: "edits `A026 04 Other`", __pickleIndex: "67", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | passed | 00:00:00.0813196 |
| F001 | Registration checks the password rule before whether the email is taken | @F001 @AC-008 @FR-003 @US1-AS6 | passed | 00:00:00.0364911 |
| F004 | Each field refusal of goal creation, with its message(n: "15", settings: "Build, duration 15 with no duration unit, 3 times "···, message: "\"Choose minutes or hours.\"", __pickleIndex: "22", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1515797 |
| F002 | Only the 24 habit icons are accepted(n: "28", icon: "`rocket`", result: "\"Choose an icon.\"", __pickleIndex: "191", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0741310 |
| F002 | A custom habit is not a catalog habit for the administrator(n: "02", action: "edits, with the name `A033 02 Taken over`, the cat"···, __pickleIndex: "105", exampleTags: []) | @F002 @AC-033 @FR-013 @FR-024 @US3-AS13 | passed | 00:00:00.1511039 |
| F002 | Custom habits never appear in browsing, for any audience(n: "03", actingUser: "the administrator `ac006.admin.03@example.com`", __pickleIndex: "16", exampleTags: []) | @F002 @AC-006 @FR-024 @US1-AS6 | passed | 00:00:00.2648765 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "01", browserTimeZone: "the browser time zone `Europe/Kyiv`", timeZone: "`Europe/Kyiv`", __pickleIndex: "222", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0870036 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "02", actingUser: "the anonymous visitor", habit: "`A016 02 Old habit`", __pickleIndex: "45", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1594255 |
| F001 | An administrator cannot change another account's password | @F001 @AC-038 @FR-010 @FR-013 @FR-015 @US6-AS7 | passed | 00:00:00.2208352 |
| F004 | The Home date is today in the account's time zone, in English whatever the culture(n: "01", culture: "the invariant culture", __pickleIndex: "286", exampleTags: []) | @F004 @AC-088 @FR-029 @US11-AS5 | passed | 00:00:00.0786115 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "01", action: "lists custom habits", __pickleIndex: "207", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1614929 |
| F001 | The anonymous visitor cannot change a password, whatever the new password(newPassword: "\"running42\"", whyItMatters: "meets the password rule", __pickleIndex: "55", exampleTags: []) | @F001 @AC-036 @FR-011 @FR-013 @US6-AS5 | passed | 00:00:00.0004109 |
| F004 | The edges of every target range are accepted(n: "04", settings: "Reduce, count 999 `cups`, 1 time per day", targetText: "\"at most 999 cups a day\"", __pickleIndex: "47", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | passed | 00:00:00.1590234 |
| F002 | The administrator edits the description, intent and icon of a catalog habit | @F002 @AC-071 @FR-012 | passed | 00:00:00.1218722 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "02", count: "2", marks: "both marked", result: "\"Mark one suggested starting goal for the card.\"", after: "no suggested starting goal", __pickleIndex: "110", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0780942 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "07", actingUser: "the administrator `ac016.admin.07@example.com`", habit: "`A016 07 Draft habit`", __pickleIndex: "50", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1630840 |
| F004 | Anonymous visitors and administrators get the catalog without goal information(n: "02", actingUser: "`f4.ac055.admin.02@example.com`", action: "browses the catalog as `f4.ac055.admin.02@example."···, result: "the result contains `A4.055 02 Reading` with no go"···, __pickleIndex: "196", exampleTags: []) | @F004 @AC-055 @FR-032 @US6-AS4 | passed | 00:00:00.1588964 |
| F001 | A new email creates an additional administrator | @F001 @AC-053 @FR-014 @US4-AS9 | passed | 00:00:00.1484679 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "09", second: "Build, duration 0 minutes, 1 time per day", result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "124", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0817053 |
| F002 | A custom habit is not a catalog habit for the administrator(n: "01", action: "reads", __pickleIndex: "104", exampleTags: []) | @F002 @AC-033 @FR-013 @FR-024 @US3-AS13 | passed | 00:00:00.1710090 |
| F004 | The profile gives "member since" with the month and year of creation in the account's time zone(n: "03", time: "`2026-11-30T22:30:00Z`", browserTimeZone: "no browser time zone", memberSince: "\"member since November 2026\"", __pickleIndex: "291", exampleTags: []) | @F004 @AC-089 @FR-030 @US11-AS6 | passed | 00:00:00.0791294 |
| F004 | "Today" is the date in the user's own time zone(n: "04", timeZone: "`America/New_York`", time: "`2026-11-17T05:00:00Z`", startDate: "2026-11-17", result: "\"The start date cannot be in the past.\"", __pickleIndex: "179", exampleTags: []) | @F004 @AC-044 @FR-006 @FR-007 @FR-012 @US4-AS8 | passed | 00:00:00.0822398 |
| F002 | Names and descriptions are trimmed before they are checked and stored(n: "01", name: "`\"   A070 01 Trimmed   \"`", description: "`\"  Read daily.  \"`", storedName: "`A070 01 Trimmed`", storedDescription: "the description \"Read daily.\"", __pickleIndex: "250", exampleTags: []) | @F002 @AC-070 @FR-004 @FR-005 | passed | 00:00:00.0777145 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "05", state: "Published", action: "edits `A026 05 Other`", __pickleIndex: "68", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | passed | 00:00:00.0802455 |
| F004 | Administrators read and change their own time zone; anonymous visitors cannot | @F004 @AC-079 @FR-026 @US10-AS8 | passed | 00:00:00.1255065 |
| F004 | After a move east, a commitment in progress is still the next one(n: "01", time: "`2026-11-18T16:00:00Z`", next: "2026-11-18", __pickleIndex: "263", exampleTags: []) | @F004 @AC-078 @FR-014 @FR-026 | passed | 00:00:00.0881455 |
| F002 | A Reduce amount of 0 is accepted and shown as "at most 0" | @F002 @AC-038 @FR-007 @FR-017 @US3-AS17 | passed | 00:00:00.0815660 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "08", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "an identifier that the test creates and that belon"···, __pickleIndex: "79", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | passed | 00:00:00.1543742 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "06", count: "0", marks: "none marked", result: "success", after: "no suggested starting goal", __pickleIndex: "114", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0996095 |
| F004 | Each field refusal of goal creation, with its message(n: "10", settings: "Reduce, count 1000 `cups`, 1 time per day", message: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "17", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1518522 |
| F002 | The anonymous visitor has no access to custom habits(n: "05", action: "deletes `Secret` by its identifier", __pickleIndex: "218", exampleTags: []) | @F002 @AC-055 @FR-023 @US4-AS9 | passed | 00:00:00.0943539 |
| F004 | Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "05", submitted: "Build, yes/no, 2 times per day, with the fixed day"···, stored: "2 times per day, no fixed days", targetText: "\"2 times a day\"", __pickleIndex: "70", exampleTags: []) | @F004 @AC-013 @FR-002 @FR-006 | passed | 00:00:00.1568188 |
| F001 | Demo loading creates the three demo accounts | @F001 @AC-057 @FR-016 @US7-AS1 | passed | 00:00:00.4343741 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "13", goal: "on `A4.008 13 Reading` with Build, yes/no, fixed d"···, message: "\"The start date cannot be in the past.\"", __pickleIndex: "42", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1518625 |
| F002 | Browsing by intent returns only that intent(n: "02", intent: "Build", sugar: "does not contain", water: "contains", __pickleIndex: "6", exampleTags: []) | @F002 @AC-004 @FR-009 @US1-AS4 | passed | 00:00:00.0812496 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "01", goal: "with no habit, no Build or Reduce, yes/no, fixed d"···, message: "\"Choose a habit.\"", __pickleIndex: "30", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1524377 |
| F002 | Field checks come in a fixed order(n: "03", fields: "the name `A029 03 Long`, a description that is a t"···, result: "\"The description can have at most 500 characters."···, __pickleIndex: "81", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0748277 |
| F004 | A goal on an Unpublished habit keeps working, and no new goal can use that habit(n: "03", actingUser: "`f4.ac060.owner.03@example.com`", action: "starts a new goal from `A4.060 03 Reading`", result: "the result is \"This habit is not in the catalog.\"···, __pickleIndex: "210", exampleTags: []) | @F004 @AC-060 @FR-016 @FR-034 @US7-AS6 | passed | 00:00:00.2276667 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "10", goal: "on `A4.008 10 Reading` with Build, yes/no, 21 time"···, message: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "39", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1523652 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "06", goal: "on `A4.008 06 Reading` with Build, count 20 with n"···, message: "\"Enter a unit, for example pages.\"", __pickleIndex: "35", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1531637 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "06", action: "edits a custom habit with the identifier of `A058 "···, __pickleIndex: "236", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.1599189 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "16", second: "Build, count 5 with a unit of 21 characters, 1 tim"···, result: "\"The unit can have at most 20 characters.\"", __pickleIndex: "131", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0804111 |
| F004 | The start date is today or up to 365 days later(n: "02", startDate: "the start date 2026-11-17", result: "\"The start date cannot be in the past.\"", __pickleIndex: "53", exampleTags: []) | @F004 @AC-010 @FR-006 @FR-012 | passed | 00:00:00.1485103 |
| F004 | A catalog habit without suggested starting goals gives only Build or Reduce from its intent | @F004 @AC-003 @FR-007 @US1-AS3 | passed | 00:00:00.1520482 |
| F004 | Another user's goal does not exist for them(n: "04", action: "reads the evaluation calendar from 2026-11-18 to 2"···, goal: "an identifier that the test creates and that belon"···, __pickleIndex: "127", exampleTags: []) | @F004 @AC-029 @FR-020 @US3-AS1 | passed | 00:00:00.2320636 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "15", browserTimeZone: "the browser time zone `America/Argentina/Buenos_Ai"···, timeZone: "`America/Argentina/Buenos_Aires`", __pickleIndex: "236", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0854905 |
| F004 | A Reduce goal with an amount has 1 time per period(n: "04", settings: "Reduce, yes/no, 3 times per week", result: "creating succeeds", __pickleIndex: "63", exampleTags: []) | @F004 @AC-012 @FR-004 @FR-006 | passed | 00:00:00.1528774 |
| F002 | A new catalog habit is a Draft that the public catalog does not show | @F002 @AC-018 @FR-011 @FR-012 @US3-AS1 | passed | 00:00:00.0888048 |
| F001 | A block applies only to its own account | @F001 @AC-024 @FR-008 | passed | 00:00:00.2881067 |
| F001 | Registration refuses an email that is not valid(email: "\"ana @example.com\"", whyItIsNotValid: "a space inside", __pickleIndex: "16", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002394 |
| F001 | An administrator gets only their own account details | @F001 @AC-030 @FR-010 @FR-012 @FR-015 @US3-AS5 | passed | 00:00:00.1143568 |
| F004 | A Reduce goal with an amount has 1 time per period(n: "03", settings: "Reduce, count 2 `cups`, 3 times per day", result: "\"A Reduce goal with an amount has 1 time per peri"···, __pickleIndex: "62", exampleTags: []) | @F004 @AC-012 @FR-004 @FR-006 | passed | 00:00:00.1519970 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "04", fields: "Build, yes/no, 1 time per week", text: "\"once a week\"", kind: "\"Yes/no · flexible\"", __pickleIndex: "31", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.1319371 |
| F001 | Administrator creation creates an administrator | @F001 @AC-039 @FR-014 @US4-AS1 | passed | 00:00:00.0809209 |
| F004 | Each field refusal of goal creation, with its message(n: "21", settings: "Build, yes/no, 3 times with no period", message: "\"Choose per day or per week.\"", __pickleIndex: "28", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1541793 |
| F004 | After a move east, a commitment in progress is still the next one(n: "03", time: "`2026-11-19T05:00:00Z`", next: "2026-11-20", __pickleIndex: "265", exampleTags: []) | @F004 @AC-078 @FR-014 @FR-026 | passed | 00:00:00.0862934 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "12", browserTimeZone: "the browser time zone `Europe/Kiev`", timeZone: "`Europe/Kiev`", __pickleIndex: "233", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0768272 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "03", state: "Unpublished", action: "creates a catalog habit", __pickleIndex: "66", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | passed | 00:00:00.0805291 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "07", fields: "Reduce, yes/no, 1 time per day", text: "\"at most once a day\"", kind: "\"Yes/no · flexible\"", __pickleIndex: "34", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.1019318 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "04", name: "`A027 04 Long text`", description: "a text of 501 characters", category: "Body", intent: "Build", icon: "`book-open`", result: "\"The description can have at most 500 characters."···, __pickleIndex: "73", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0778708 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "17", settings: "Reduce, duration 1 hours, 1 time per day", targetText: "\"at most 1 h a day\"", __pickleIndex: "103", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.1068699 |
| F002 | Only the 24 habit icons are accepted(n: "04", icon: "`footprints`", result: "success", __pickleIndex: "167", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0753057 |
| F004 | A fixed-days goal has one commitment per scheduled day, from midnight to midnight in its time zone | @F004 @AC-036 @FR-003 @FR-011 @FR-012 @FR-013 @US4-AS1 @US10-AS3 | passed | 00:00:00.0851619 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "02", settings: "Build, count 20 `pages`, 3 times per week", targetText: "\"20 pages, 3 times a week\"", __pickleIndex: "88", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0820136 |
| F002 | For custom habits the acting user and ownership come first, the name check last(n: "01", actingUser: "the anonymous visitor", action: "creates a custom habit with the name `\"   \"`", result: "\"You are not signed in.\"", __pickleIndex: "226", exampleTags: []) | @F002 @AC-057 @FR-005 @FR-016 @US4-AS11 | passed | 00:00:00.1520318 |
| F002 | Only the 24 habit icons are accepted(n: "08", icon: "`moon`", result: "success", __pickleIndex: "171", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0752810 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "05", actingUser: "the registered user `ac016.user.05@example.com`", habit: "`A016 05 Old habit`", __pickleIndex: "48", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1622170 |
| F001 | Administrator creation finds a registered user whatever the case and spaces of the email | @F001 @AC-063 @FR-003 @FR-014 | passed | 00:00:00.0795014 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "08", fields: "Build, count 5 with the unit `\"  Pages  \"` (5 ch"···, storedUnit: "`Pages`", __pickleIndex: "145", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0810954 |
| F001 | A password change to a password that breaks the password rule is refused(email: "\"weak.new@example.com\"", newPassword: "\"runningfar\"", whyItBreaksTheRule: "10 characters, no digit", __pickleIndex: "52", exampleTags: []) | @F001 @AC-034 @FR-002 @FR-013 @US6-AS3 | passed | 00:00:00.1521471 |
| F002 | The anonymous visitor cannot manage the catalog(n: "01", action: "lists the catalog habits as an administrator", __pickleIndex: "94", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0956861 |
| F002 | The role and the identifier are checked before the fields(n: "04", actingUser: "the registered user `ac039.user.04@example.com`", action: "edits an unknown identifier with the name `\"   \""···, result: "\"Only administrators can manage the catalog.\"", __pickleIndex: "152", exampleTags: []) | @F002 @AC-039 @FR-016 @US3-AS18 | passed | 00:00:00.1495483 |
| F004 | A range before the start date gives an empty calendar | @F004 @AC-042 @FR-013 @US4-AS6 | passed | 00:00:00.0832838 |
| F004 | Editing a custom habit renames its goals but never changes their settings | @F004 @AC-050 @FR-010 @FR-035 @US5-AS5 | passed | 00:00:00.0911227 |
| F004 | A goal on an Unpublished habit keeps working, and no new goal can use that habit(n: "04", actingUser: "`f4.ac060.owner.04@example.com`", action: "creates a goal on `A4.060 04 Reading` with Build, "···, result: "the result is \"This habit is not in the catalog.\"···, __pickleIndex: "211", exampleTags: []) | @F004 @AC-060 @FR-016 @FR-034 @US7-AS6 | passed | 00:00:00.2260670 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "01", fields: "Build, duration 30 minutes, fixed days Monday to S"···, text: "\"30 min daily\"", kind: "\"Duration · fixed days every day\"", __pickleIndex: "28", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.1094506 |
| F002 | Demo loading without a valid demo password creates no catalog and no custom habit(n: "02", demoPassword: "`\"   \"`", result: "\"no demo password configured\"", __pickleIndex: "242", exampleTags: []) | @F002 @AC-063 @FR-030 @FR-031 @US6-AS4 | passed | 00:00:00.3146469 |
| F004 | A confirmed unpublish acts even when goals were added after the count was given | @F004 @AC-093 @FR-033 | passed | 00:00:00.2431577 |
| F004 | A Reduce goal with an amount has 1 time per period(n: "06", settings: "Reduce, count 2 `cups`, fixed days Monday, Wednesd"···, result: "creating succeeds", __pickleIndex: "65", exampleTags: []) | @F004 @AC-012 @FR-004 @FR-006 | passed | 00:00:00.1555335 |
| F004 | A goal without a name of its own is named after its habit and target(n: "03", name: "the name \"  Morning pages  \"", goalName: "\"Morning pages\"", own: "its own", __pickleIndex: "84", exampleTags: []) | @F004 @AC-016 @FR-009 @FR-010 @US1-AS9 | passed | 00:00:00.2094898 |
| F004 | A time zone is given only to its own account | @F004 @AC-080 @FR-024 @US10-AS9 | passed | 00:00:00.2299463 |
| F004 | A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "04", schedule: "1 time per day", date: "2027-10-31", entry: "2027-10-31 from `2027-10-30T21:00:00Z` to `2027-10"···, __pickleIndex: "167", exampleTags: []) | @F004 @AC-040 @FR-012 @FR-013 @US4-AS5 | passed | 00:00:00.0817061 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "06", actingUser: "the registered user `ac016.user.06@example.com`", habit: "an unknown identifier", __pickleIndex: "49", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1610051 |
| F004 | An account name is trimmed, and empty means no name(n: "04", name: "the name `\"\"`", result: "no name", __pickleIndex: "276", exampleTags: []) | @F004 @AC-084 @FR-019 @FR-028 @US11-AS1 | passed | 00:00:00.0783942 |
| F004 | A goal on a custom habit is based on it and named after it | @F004 @AC-047 @FR-001 @FR-010 @US5-AS2 | passed | 00:00:00.0899019 |
| F004 | Several goals on one habit are independent | @F004 @AC-017 @FR-001 @US1-AS10 | passed | 00:00:00.1777977 |
| F004 | Demo loading gives each created demo account its demo goals | @F004 @AC-064 @FR-040 @US9-AS1 | passed | 00:00:00.4528790 |
| F004 | Use cases check the acting user, the role, the identifier and the fields, in this order(n: "06", actingUser: "`f4.ac035.user.06@example.com`", action: "starts a new goal from `A4.035 06 Draft` with the "···, message: "\"This habit is not in the catalog.\"", __pickleIndex: "152", exampleTags: []) | @F004 @AC-035 @FR-023 | passed | 00:00:00.1510682 |
| F001 | A password change checks the current password before the password rule | @F001 @AC-035 @FR-013 @US6-AS4 | passed | 00:00:00.1206231 |
| F004 | Each field refusal of goal creation, with its message(n: "05", settings: "Build, count with no amount and the unit `pages`, "···, message: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "12", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1532040 |
| F002 | The administrator reads a Draft with all its fields and its state | @F002 @AC-042 @FR-001 @FR-012 | passed | 00:00:00.0816955 |
| F002 | The administrator's list is in name order and can be searched | @F002 @AC-040 @FR-004 @FR-012 @US3-AS19 | passed | 00:00:00.0920366 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@.example\"", whyItIsNotValid: "domain has one non-empty part, after an empty one", __pickleIndex: "11", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0004384 |
| F002 | A registered user cannot manage the catalog(n: "01", action: "lists the catalog habits as an administrator", __pickleIndex: "88", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1800972 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "14", settings: "Reduce, yes/no, 1 time per day", targetText: "\"at most once a day\"", __pickleIndex: "100", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0843961 |
| F002 | Only the 24 habit icons are accepted(n: "12", icon: "`brain`", result: "success", __pickleIndex: "175", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0737341 |
| F004 | The edges of every target range are accepted(n: "02", settings: "Build, count 999 `pages`, 3 times per week", targetText: "\"999 pages, 3 times a week\"", __pickleIndex: "45", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | passed | 00:00:00.1563304 |
| F004 | Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "02", submitted: "Build, duration 30 minutes with the count unit `pa"···, stored: "duration 30 minutes, no count unit", targetText: "\"30 min, 3 times a week\"", __pickleIndex: "67", exampleTags: []) | @F004 @AC-013 @FR-002 @FR-006 | passed | 00:00:00.1539609 |
| F001 | Sign-in ignores spaces around the email | @F001 @AC-011 @FR-006 | passed | 00:00:00.0828310 |
| F004 | Creating a goal with the starting values unchanged | @F004 @AC-005 @FR-001 @FR-002 @FR-005 @FR-015 @US1-AS5 | passed | 00:00:00.1741281 |
| F001 | Administrator creation checks for a missing password before the email | @F001 @AC-048 @FR-014 | passed | 00:00:00.0009694 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "04", actingUser: "the registered user `ac016.user.04@example.com`", habit: "`A016 04 Draft habit`", __pickleIndex: "47", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1648161 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "07", fields: "Build, count 5 with a unit of 20 characters, 1 tim"···, storedUnit: "that unit of 20 characters", __pickleIndex: "144", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0787967 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "02", second: "Build, no measurement, 1 time per day", result: "\"Choose how it is measured.\"", __pickleIndex: "117", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0805015 |
| F001 | The anonymous visitor cannot change a password, whatever the new password(newPassword: "\"run\"", whyItMatters: "breaks the password rule, which is checked only fo"···, __pickleIndex: "56", exampleTags: []) | @F001 @AC-036 @FR-011 @FR-013 @US6-AS5 | passed | 00:00:00.0016543 |
| F004 | A named time zone is kept as given, and the same time zone changes nothing(n: "01", value: "`Europe/Kyiv`", later: "no later version", __pickleIndex: "257", exampleTags: []) | @F004 @AC-075 @FR-024 @FR-026 | passed | 00:00:00.0862897 |
| F002 | Suggested starting goals are checked in list order, and each by its first failing field(n: "02", goals: "(1) Build, yes/no, 1 time per day (marked); (2) Bu"···, result: "\"Choose how it is measured.\"", position: "2", __pickleIndex: "155", exampleTags: []) | @F002 @AC-041 @FR-007 | passed | 00:00:00.0856857 |
| F004 | A second change replaces a version not yet begun, and follows a version already in effect(n: "02", schedule: "1 time per day", startDate: "2026-11-16", from: "2026-11-18", to: "2026-11-20", versions: "the time zone `Asia/Tokyo` from 2026-11-20 and no "···, entries: "2026-11-18 from `2026-11-17T22:00:00Z` to `2026-11"···, __pickleIndex: "269", exampleTags: []) | @F004 @AC-081 @FR-026 @US10-AS10 | passed | 00:00:00.0898273 |
| F002 | Field checks refuse a custom habit with the same messages(n: "05", name: "`No intent`", description: "none", category: "Body", intent: "none", icon: "`droplet`", result: "\"Choose Build or Reduce.\"", __pickleIndex: "223", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0878218 |
| F004 | An account name is trimmed, and empty means no name(n: "01", name: "the name \"  Yuri  \"", result: "the name \"Yuri\"", __pickleIndex: "273", exampleTags: []) | @F004 @AC-084 @FR-019 @FR-028 @US11-AS1 | passed | 00:00:00.0796347 |
| F004 | An account name is trimmed, and empty means no name(n: "03", name: "the name `\"   \"`", result: "no name", __pickleIndex: "275", exampleTags: []) | @F004 @AC-084 @FR-019 @FR-028 @US11-AS1 | passed | 00:00:00.0767431 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "13", browserTimeZone: "the browser time zone `Asia/Calcutta`", timeZone: "`Asia/Calcutta`", __pickleIndex: "234", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0904617 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "03", action: "deletes `Gone` by its identifier", __pickleIndex: "233", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.1534532 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "13", second: "Reduce, count with no amount and the unit `sweets`"···, result: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "128", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0809761 |
| F004 | A goal that has not started gives its start date | @F004 @AC-024 @FR-013 @FR-014 @US2-AS3 | passed | 00:00:00.0943696 |
| F002 | Demo loading loads the seed catalog into an empty catalog | @F002 @AC-060 @FR-030 @US6-AS1 | passed | 00:00:00.5737681 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "08", value: "`\"\"`", __pickleIndex: "256", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.0844152 |
| F004 | A custom habit is looked up only among the acting user's custom habits(n: "04", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "the identifier of `A4.033 04 Reading`", __pickleIndex: "144", exampleTags: []) | @F004 @AC-033 @FR-008 @US3-AS5 | passed | 00:00:00.2241280 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "05", fields: "Build, duration 1 minutes, 1 time per week", storedUnit: "minutes", __pickleIndex: "142", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0994538 |
| F004 | Demo loading run again gives existing demo accounts no goals | @F004 @AC-065 @FR-040 @US9-AS2 | passed | 00:00:00.4475376 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "03", action: "starts a new goal from", habit: "the identifier of their custom habit `Cold shower`", __pickleIndex: "74", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | passed | 00:00:00.2066861 |
| F002 | Suggested starting goals keep their order and the card shows the marked one | @F002 @AC-034 @FR-006 @US3-AS14 | passed | 00:00:00.1141722 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "04", second: "Build, yes/no with the unit `pages`, 1 time per da"···, result: "\"A yes/no suggested starting goal has no amount o"···, __pickleIndex: "119", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0783655 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "15", second: "Build, count 5 with the unit `\"  \"`, 1 time per "···, result: "\"Enter a unit, for example pages.\"", __pickleIndex: "130", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0798891 |
| F002 | For custom habits the acting user and ownership come first, the name check last(n: "04", actingUser: "the registered user `ac057.a.04@example.com`", action: "creates a custom habit with the name `taken` and n"···, result: "\"Choose a category.\"", __pickleIndex: "229", exampleTags: []) | @F002 @AC-057 @FR-005 @FR-016 @US4-AS11 | passed | 00:00:00.1508304 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "16", settings: "Reduce, count 2 `cups`, fixed days Monday to Frida"···, targetText: "\"at most 2 cups, 5 days a week\"", __pickleIndex: "102", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.1009914 |
| F004 | The next commitment is the first commitment that has not ended(n: "05", time: "`2026-11-21T00:00:00Z`", next: "2026-11-23", __pickleIndex: "118", exampleTags: []) | @F004 @AC-023 @FR-014 @US2-AS2 | passed | 00:00:00.1131320 |
| F004 | The start date is today or up to 365 days later(n: "03", startDate: "the start date 2027-11-18", result: "creating succeeds", __pickleIndex: "54", exampleTags: []) | @F004 @AC-010 @FR-006 @FR-012 | passed | 00:00:00.1506423 |
| F004 | Every suggested starting goal of the seed catalog can be created unchanged | @F004 @AC-020 @FR-007 | passed | 00:00:00.4852250 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "11", browserTimeZone: "the browser time zone `\"\"`", timeZone: "`UTC`", __pickleIndex: "232", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0790475 |
| F002 | Only the 24 habit icons are accepted(n: "24", icon: "`tv`", result: "success", __pickleIndex: "187", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0731204 |
| F001 | Registration accepts a password at the lower bound of the password rule(email: "\"eight.pw@example.com\"", password: "\"walking4\"", whyItMeetsTheRule: "8 characters, with a digit", __pickleIndex: "5", exampleTags: []) | @F001 @AC-003 @FR-001 @FR-002 | passed | 00:00:00.0733253 |
| F004 | "Only habits I track" returns the Published habits with the user's goals, with the other filters(n: "03", filter: "the category Body", result: "no habit", __pickleIndex: "193", exampleTags: []) | @F004 @AC-053 @FR-031 @US6-AS2 | passed | 00:00:00.1645604 |
| F004 | Each field refusal of goal creation, with its message(n: "13", settings: "Build, count 20 with no unit, 3 times per week", message: "\"Enter a unit, for example pages.\"", __pickleIndex: "20", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1507677 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "05", search: "`\"\"`", morning: "contains", evening: "contains", __pickleIndex: "12", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.0834464 |
| F004 | A named time zone is kept as given, and the same time zone changes nothing(n: "02", value: "`Europe/Kiev`", later: "a later version `Europe/Kiev` from 2026-11-19", __pickleIndex: "258", exampleTags: []) | @F004 @AC-075 @FR-024 @FR-026 | passed | 00:00:00.0868115 |
| F004 | Reading a custom habit gives the number of goals that use it | @F004 @AC-051 @FR-035 | passed | 00:00:00.0916780 |
| F001 | Administrator creation checks the email before the password rule | @F001 @AC-049 @FR-003 @FR-014 | passed | 00:00:00.0010188 |
| F004 | A goal on an Unpublished habit keeps working, and no new goal can use that habit(n: "01", actingUser: "`f4.ac060.owner.01@example.com`", action: "lists their goals", result: "the list contains \"A4.060 01 Reading, 15 min, 5 d"···, __pickleIndex: "208", exampleTags: []) | @F004 @AC-060 @FR-016 @FR-034 @US7-AS6 | passed | 00:00:00.2336475 |
| F001 | A signed-in user changes their password | @F001 @AC-031 @FR-013 @US6-AS1 | passed | 00:00:00.2466235 |
| F004 | A per-week goal has one evaluation period per Monday-to-Sunday week, from its first full week(n: "04", startDate: "2026-11-23", from: "2026-11-23", to: "2026-11-29", periods: "1 period: 2026-11-23 to 2026-11-29 from `2026-11-2"···, __pickleIndex: "159", exampleTags: []) | @F004 @AC-038 @FR-013 @US4-AS3 | passed | 00:00:00.0857146 |
| F002 | Custom-habit names and descriptions are trimmed before they are checked and stored(n: "04", name: "`\"Long\"`", description: "`\"  \"` + a text of 500 characters + `\"  \"` (50"···, storedName: "`Long`", storedDescription: "the 500-character description", __pickleIndex: "259", exampleTags: []) | @F002 @AC-073 @FR-004 @FR-005 | passed | 00:00:00.1010482 |
| F004 | An account name has at most 60 characters | @F004 @AC-085 @FR-028 @US11-AS2 | passed | 00:00:00.1595946 |
| F001 | Administrator creation finds an existing administrator before it checks the password rule | @F001 @AC-047 @FR-014 @US4-AS8 | passed | 00:00:00.0793960 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@\"", whyItIsNotValid: "no domain", __pickleIndex: "8", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002502 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "17", browserTimeZone: "the browser time zone `America/New_York`", timeZone: "`America/New_York`", __pickleIndex: "238", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0795471 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "02", state: "Published", action: "creates a catalog habit", __pickleIndex: "65", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | passed | 00:00:00.0791480 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "15", fields: "Build, yes/no, fixed days Monday, Tuesday, Wednesd"···, text: "\"6 days a week\"", kind: "\"Yes/no · fixed days Mon–Wed, Fri–Sun\"", __pickleIndex: "42", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.1002374 |
| F004 | Reading a goal gives all its fields, its settings version and its attempt | @F004 @AC-025 @FR-005 @FR-016 @FR-018 @US2-AS4 | passed | 00:00:00.1622691 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "08", goal: "on `A4.008 08 Reading` with Build, duration 15 wit"···, message: "\"Choose minutes or hours.\"", __pickleIndex: "37", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1546153 |
| F001 | Administrator creation refuses a password that breaks the password rule(email: "\"short.admin@example.com\"", password: "\"curat42\"", whyItBreaksTheRule: "7 characters, with a digit", __pickleIndex: "73", exampleTags: []) | @F001 @AC-046 @FR-002 @FR-014 @US4-AS7 | passed | 00:00:00.0790575 |
| F004 | At registration, the name is checked after slice 1's checks | @F004 @AC-094 @FR-028 | passed | 00:00:00.0390545 |
| F001 | An administrator reads their own account details | @F001 @AC-028 @FR-009 @FR-012 @FR-015 @US3-AS3 | passed | 00:00:00.0792904 |
| F001 | Demo loading without a demo password creates no account(demoPassword: "absent", __pickleIndex: "84", exampleTags: []) | @F001 @AC-055 @FR-016 @US7-AS3 | passed | 00:00:00.2955801 |
| F001 | Demo loading decides per demo email, and a demo email registered earlier keeps its owner's password | @F001 @AC-060 @FR-003 @FR-016 | passed | 00:00:00.4560852 |
| F004 | After a move west, a commitment already over is not the next one(n: "02", time: "`2026-11-18T23:00:00Z`", next: "2026-11-20", __pickleIndex: "262", exampleTags: []) | @F004 @AC-077 @FR-014 @FR-026 | passed | 00:00:00.0960606 |
| F002 | The anonymous visitor has no access to custom habits(n: "04", action: "edits `Secret` by its identifier, with the name `S"···, __pickleIndex: "217", exampleTags: []) | @F002 @AC-055 @FR-023 @US4-AS9 | passed | 00:00:00.0963837 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "03", actingUser: "the anonymous visitor", habit: "an unknown identifier", __pickleIndex: "46", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1620047 |
| F004 | Reading a goal gives the version of its current date and the one later version(n: "05", schedule: "3 times per week", startDate: "2026-11-16", time: "`2026-11-18T12:00:00Z`", versions: "the time zone `Europe/Kyiv` from 2026-11-16 and a "···, __pickleIndex: "247", exampleTags: []) | @F004 @AC-073 @FR-016 @FR-026 @US10-AS6 | passed | 00:00:00.1244856 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@example.\"", whyItIsNotValid: "domain has one non-empty part, before an empty one", __pickleIndex: "12", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002559 |
| F001 | An unknown email, a wrong password and a blocked account give the same result | @F001 @AC-014 @FR-007 @FR-008 | passed | 00:00:00.3081141 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "09", actingUser: "the administrator `ac016.admin.09@example.com`", habit: "an unknown identifier", __pickleIndex: "52", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1616190 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "09", settings: "Build, duration 30 minutes, fixed days on all seve"···, targetText: "\"30 min daily\"", __pickleIndex: "95", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0869885 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "03", browserTimeZone: "the browser time zone `Mars/Olympus`", timeZone: "`UTC`", __pickleIndex: "224", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0792236 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "10", second: "Build, duration 1000 minutes, 1 time per day", result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "125", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0806517 |
| F004 | A second change replaces a version not yet begun, and follows a version already in effect(n: "03", schedule: "1 time per day", startDate: "2026-12-01", from: "2026-12-01", to: "2026-12-01", versions: "the time zone `Asia/Tokyo` from 2026-12-01 and no "···, entries: "2026-12-01 from `2026-11-30T15:00:00Z` to `2026-12"···, __pickleIndex: "270", exampleTags: []) | @F004 @AC-081 @FR-026 @US10-AS10 | passed | 00:00:00.0923250 |
| F002 | Field checks come in a fixed order(n: "07", fields: "the name `A029 07 Taken`, the category Body, the i"···, result: "\"Mark one suggested starting goal for the card.\"", __pickleIndex: "85", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0748650 |
| F002 | Field checks refuse a custom habit with the same messages(n: "03", name: "`Long text`", description: "a text of 501 characters", category: "Body", intent: "Build", icon: "`droplet`", result: "\"The description can have at most 500 characters."···, __pickleIndex: "221", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0867742 |
| F004 | Each field refusal of goal creation, with its message(n: "11", settings: "Reduce, duration with no amount and the unit minut"···, message: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "18", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1525271 |
| F001 | Registration keeps the email in its lower-case form | @F001 @AC-006 @FR-003 @FR-006 @FR-012 | passed | 00:00:00.0789150 |
| F002 | Browsing returns only Published habits, with their card content, to every audience(n: "03", actingUser: "the administrator `ac001.admin.03@example.com`", __pickleIndex: "2", exampleTags: []) | @F002 @AC-001 @FR-008 @US1-AS1 | passed | 00:00:00.1966144 |
| F004 | The next commitment is the first commitment that has not ended(n: "01", time: "`2026-11-16T08:00:00Z`", next: "2026-11-16", __pickleIndex: "114", exampleTags: []) | @F004 @AC-023 @FR-014 @US2-AS2 | passed | 00:00:00.1097148 |
| F004 | The next commitment is the first commitment that has not ended(n: "03", time: "`2026-11-18T23:59:59Z`", next: "2026-11-18", __pickleIndex: "116", exampleTags: []) | @F004 @AC-023 @FR-014 @US2-AS2 | passed | 00:00:00.1147966 |
| F004 | A per-week goal has one evaluation period per Monday-to-Sunday week, from its first full week(n: "02", startDate: "2026-11-18", from: "2026-11-25", to: "2026-12-01", periods: "2 periods: 2026-11-23 to 2026-11-29 from `2026-11-"···, __pickleIndex: "157", exampleTags: []) | @F004 @AC-038 @FR-013 @US4-AS3 | passed | 00:00:00.0841561 |
| F004 | The start date is today or up to 365 days later(n: "04", startDate: "the start date 2027-11-19", result: "\"Choose a start date within a year from today.\"", __pickleIndex: "55", exampleTags: []) | @F004 @AC-010 @FR-006 @FR-012 | passed | 00:00:00.1498732 |
| F002 | Each of the six categories can be given to a catalog habit(n: "04", category: "Money", __pickleIndex: "161", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0941592 |
| F004 | After a move west, a commitment already over is not the next one(n: "01", time: "`2026-11-18T21:59:59Z`", next: "2026-11-18", __pickleIndex: "261", exampleTags: []) | @F004 @AC-077 @FR-014 @FR-026 | passed | 00:00:00.0920051 |
| F002 | A registered user deletes their custom habit | @F002 @AC-050 @FR-020 @US4-AS4 | passed | 00:00:00.0883048 |
| F002 | A custom habit is not a catalog habit for the administrator(n: "03", action: "publishes", __pickleIndex: "106", exampleTags: []) | @F002 @AC-033 @FR-013 @FR-024 @US3-AS13 | passed | 00:00:00.1551624 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@example.com.\"", whyItIsNotValid: "empty domain part after two non-empty ones", __pickleIndex: "15", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0026342 |
| F001 | A registered user reads their own account details | @F001 @AC-027 @FR-009 @FR-012 @US3-AS2 | passed | 00:00:00.0824112 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@@example.com\"", whyItIsNotValid: "two \"@\"", __pickleIndex: "17", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002692 |
| F002 | Only the 24 habit icons are accepted(n: "13", icon: "`pen-line`", result: "success", __pickleIndex: "176", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0755272 |
| F002 | Only the 24 habit icons are accepted(n: "20", icon: "`wallet`", result: "success", __pickleIndex: "183", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0755976 |
| F004 | The Home date is today in the account's time zone, in English whatever the culture(n: "03", culture: "`de-DE`", __pickleIndex: "288", exampleTags: []) | @F004 @AC-088 @FR-029 @US11-AS5 | passed | 00:00:00.0801744 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "10", browserTimeZone: "the browser time zone `europe/kyiv`", timeZone: "`UTC`", __pickleIndex: "231", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0801337 |
| F004 | The profile gives "member since" with the month and year of creation in the account's time zone(n: "02", time: "`2026-11-30T22:30:00Z`", browserTimeZone: "the browser time zone `Europe/Kyiv`", memberSince: "\"member since December 2026\"", __pickleIndex: "290", exampleTags: []) | @F004 @AC-089 @FR-030 @US11-AS6 | passed | 00:00:00.0782380 |
| F002 | Only the 24 habit icons are accepted(n: "15", icon: "`music`", result: "success", __pickleIndex: "178", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0743399 |
| F004 | Each field refusal of goal creation, with its message(n: "06", settings: "Build, count 0 `pages`, 3 times per week", message: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "13", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1534325 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "03", value: "`Etc/GMT+3`", __pickleIndex: "251", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.0835188 |
| F004 | A goal without a name of its own is named after its habit and target(n: "01", name: "no name", goalName: "\"A4.016 01 Reading, 15 min, 5 days a week\"", own: "not its own", __pickleIndex: "82", exampleTags: []) | @F004 @AC-016 @FR-009 @FR-010 @US1-AS9 | passed | 00:00:00.2113176 |
| F002 | Field checks come in a fixed order(n: "01", fields: "the name `\"   \"`, no category and no icon", result: "\"Enter a name.\"", __pickleIndex: "79", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0752118 |
| F004 | Without a name the greeting has no name | @F004 @AC-087 @FR-029 @US11-AS4 | passed | 00:00:00.0806863 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "06", browserTimeZone: "the browser time zone `EST`", timeZone: "`UTC`", __pickleIndex: "227", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0815710 |
| F004 | An administrator gets no goal use case(n: "03", action: "lists their goals", __pickleIndex: "131", exampleTags: []) | @F004 @AC-031 @FR-021 @US3-AS3 | passed | 00:00:00.1497867 |
| F004 | The edges of every target range are accepted(n: "06", settings: "Build, yes/no, 1 time per week", targetText: "\"once a week\"", __pickleIndex: "49", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | passed | 00:00:00.1567204 |
| F004 | An account name is trimmed, and empty means no name(n: "02", name: "no name", result: "no name", __pickleIndex: "274", exampleTags: []) | @F004 @AC-084 @FR-019 @FR-028 @US11-AS1 | passed | 00:00:00.0812045 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "03", fields: "Reduce, count 0 `sweets`, 1 time per day", storedUnit: "`sweets`", __pickleIndex: "140", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0815512 |
| F002 | Only the 24 habit icons are accepted(n: "06", icon: "`candy`", result: "success", __pickleIndex: "169", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0725905 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "04", count: "6", marks: "the first marked", result: "\"A habit can have at most 5 suggested starting go"···, after: "no suggested starting goal", __pickleIndex: "112", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0935893 |
| F002 | Publish and unpublish check the acting user and the role before the identifier(n: "03", actingUser: "the anonymous visitor", action: "publishes", result: "\"You are not signed in.\"", __pickleIndex: "195", exampleTags: []) | @F002 @AC-046 @FR-016 | passed | 00:00:00.0768466 |
| F002 | Publishing a Draft makes it public | @F002 @AC-019 @FR-011 @US3-AS2 | passed | 00:00:00.0810498 |
| F001 | Administrator creation refuses an email that is not valid | @F001 @AC-045 @FR-003 @FR-014 @US4-AS6 | passed | 00:00:00.0009985 |
| F004 | After a move east, a commitment in progress is still the next one(n: "02", time: "`2026-11-19T04:59:59Z`", next: "2026-11-18", __pickleIndex: "264", exampleTags: []) | @F004 @AC-078 @FR-014 @FR-026 | passed | 00:00:00.0845636 |
| F001 | Demo loading run again changes nothing | @F001 @AC-059 @FR-016 @US7-AS2 | passed | 00:00:00.6023378 |
| F004 | The calendar range has 1 to 366 days(n: "01", from: "2026-11-24", to: "2026-11-18", result: "\"Choose a range of 1 to 366 days.\"", __pickleIndex: "172", exampleTags: []) | @F004 @AC-043 @FR-013 @US4-AS7 | passed | 00:00:00.0844187 |
| F004 | Reading a goal gives the version of its current date and the one later version(n: "01", schedule: "fixed days Monday, Wednesday and Friday", startDate: "2026-11-16", time: "`2026-11-18T12:00:00Z`", versions: "the time zone `Europe/Kyiv` from 2026-11-16 and a "···, __pickleIndex: "243", exampleTags: []) | @F004 @AC-073 @FR-016 @FR-026 @US10-AS6 | passed | 00:00:00.1286416 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "01", state: "Draft", action: "creates a catalog habit", __pickleIndex: "64", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | passed | 00:00:00.0775520 |
| F004 | Starting from a custom habit gives only Build or Reduce from its intent(n: "02", intent: "Reduce", __pickleIndex: "182", exampleTags: []) | @F004 @AC-046 @FR-007 @US5-AS1 | passed | 00:00:00.0814459 |
| F001 | Administrator creation checks for a missing password before it looks for an existing administrator | @F001 @AC-051 @FR-014 | passed | 00:00:00.1001663 |
| F002 | Names and descriptions are trimmed before they are checked and stored(n: "03", name: "`\"   \"` + `A070 03 ` followed by `x` up to 60 ch"···, description: "none", storedName: "the 60-character name", storedDescription: "no description", __pickleIndex: "252", exampleTags: []) | @F002 @AC-070 @FR-004 @FR-005 | passed | 00:00:00.0762220 |
| F004 | A goal name has at most 60 characters after trimming(n: "02", name: "a text of 61 characters", result: "\"The name can have at most 60 characters.\"", __pickleIndex: "58", exampleTags: []) | @F004 @AC-011 @FR-006 @FR-009 | passed | 00:00:00.1523013 |
| F001 | Administrator creation keeps the email in its trimmed, lower-case form | @F001 @AC-062 @FR-003 @FR-012 @FR-014 | passed | 00:00:00.0872399 |
| F004 | The Home date is today in the account's time zone, in English whatever the culture(n: "02", culture: "`uk-UA`", __pickleIndex: "287", exampleTags: []) | @F004 @AC-088 @FR-029 @US11-AS5 | passed | 00:00:00.0797030 |
| F004 | A per-week goal has one evaluation period per Monday-to-Sunday week, from its first full week(n: "03", startDate: "2026-11-18", from: "2026-11-18", to: "2026-11-22", periods: "no period", __pickleIndex: "158", exampleTags: []) | @F004 @AC-038 @FR-013 @US4-AS3 | passed | 00:00:00.0826258 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "08", name: "`A027 08 Menu icon`", description: "none", category: "Body", intent: "Build", icon: "`menu`", result: "\"Choose an icon.\"", __pickleIndex: "77", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0775812 |
| F002 | Unpublishing an Unpublished habit is a success that changes nothing | @F002 @AC-025 @FR-011 @US3-AS8 | passed | 00:00:00.0818958 |
| F002 | The anonymous visitor has no access to custom habits(n: "03", action: "creates the custom habit `Visitor habit`", __pickleIndex: "216", exampleTags: []) | @F002 @AC-055 @FR-023 @US4-AS9 | passed | 00:00:00.0985495 |
| F004 | Demo loading results name the demo email but never a goal, habit or account name | @F004 @AC-068 @FR-040 @US9-AS4 | passed | 00:00:00.3402233 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "12", second: "Reduce, count -1 `sweets`, 1 time per day", result: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "127", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0787988 |
| F002 | Custom-habit names and descriptions are trimmed before they are checked and stored(n: "02", name: "`\"Blank\"`", description: "`\"   \"`", storedName: "`Blank`", storedDescription: "no description", __pickleIndex: "257", exampleTags: []) | @F002 @AC-073 @FR-004 @FR-005 | passed | 00:00:00.0931999 |
| F004 | Each field refusal of goal creation, with its message(n: "07", settings: "Build, count 1000 `pages`, 3 times per week", message: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "14", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1530225 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "08", fields: "Build, yes/no, fixed days Monday, Tuesday, Wednesd"···, text: "\"4 days a week\"", kind: "\"Yes/no · fixed days Mon–Wed, Sat\"", __pickleIndex: "35", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.8355957 |
| F001 | A registered user cannot change another account's password | @F001 @AC-037 @FR-010 @FR-013 @US6-AS6 | passed | 00:00:00.2192172 |
| F002 | Only the 24 habit icons are accepted(n: "19", icon: "`wine-off`", result: "success", __pickleIndex: "182", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0783731 |
| F004 | Another user's goal does not exist for them(n: "01", action: "reads the goal", goal: "`f4.ac029.a.01@example.com`'s goal", __pickleIndex: "124", exampleTags: []) | @F004 @AC-029 @FR-020 @US3-AS1 | passed | 00:00:00.2258131 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "01", second: "no Build or Reduce, yes/no, 1 time per day", result: "\"Choose Build or Reduce.\"", __pickleIndex: "116", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0792365 |
| F002 | Another user, and a catalog habit, may have the same name | @F002 @AC-052 @FR-004 @US4-AS6 | passed | 00:00:00.2384547 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "04", action: "starts a new goal from", habit: "an identifier that the test creates and that belon"···, __pickleIndex: "75", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | passed | 00:00:00.1565275 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "02", fields: "Build, count 999 `pages`, 20 times per week", storedUnit: "`pages`", __pickleIndex: "139", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0816009 |
| F004 | "Today" is the date in the user's own time zone(n: "03", timeZone: "`America/New_York`", time: "`2026-11-17T03:00:00Z`", startDate: "2026-11-16", result: "creating succeeds", __pickleIndex: "178", exampleTags: []) | @F004 @AC-044 @FR-006 @FR-007 @FR-012 @US4-AS8 | passed | 00:00:00.0826365 |
| F004 | Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "03", submitted: "Build, yes/no with the count unit `\"   \"`, fixed"···, stored: "yes/no, no amount and no unit", targetText: "\"daily\"", __pickleIndex: "68", exampleTags: []) | @F004 @AC-013 @FR-002 @FR-006 | passed | 00:00:00.1587864 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"fourth.admin@example.com\"", password: "absent", __pickleIndex: "64", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0001884 |
| F002 | Deleted demo custom habits are not created again | @F002 @AC-066 @FR-031 @US6-AS7 | passed | 00:00:00.4839946 |
| F004 | "Only habits I track" returns the Published habits with the user's goals, with the other filters(n: "01", filter: "no category", result: "`A4.053 01 Reading` and `A4.053 01 Sugar`, and not"···, __pickleIndex: "191", exampleTags: []) | @F004 @AC-053 @FR-031 @US6-AS2 | passed | 00:00:00.1682753 |
| F004 | Demo goals are left out when their catalog habit is not Published | @F004 @AC-066 @FR-040 @US9-AS3 | passed | 00:00:00.5017026 |
| F004 | Creating a goal with changed values keeps the changed values | @F004 @AC-006 @FR-002 @FR-010 @US1-AS6 | passed | 00:00:00.1624925 |
| F004 | A named time zone is kept as given, and the same time zone changes nothing(n: "03", value: "`UTC`", later: "a later version `UTC` from 2026-11-19", __pickleIndex: "259", exampleTags: []) | @F004 @AC-075 @FR-024 @FR-026 | passed | 00:00:00.0932171 |
| F004 | The next commitment is the first commitment that has not ended(n: "02", time: "`2026-11-18T00:00:00Z`", next: "2026-11-18", __pickleIndex: "115", exampleTags: []) | @F004 @AC-023 @FR-014 @US2-AS2 | passed | 00:00:00.1120149 |
| F004 | "Only habits I track" returns the Published habits with the user's goals, with the other filters(n: "02", filter: "the category Health", result: "`A4.053 02 Sugar` only", __pickleIndex: "192", exampleTags: []) | @F004 @AC-053 @FR-031 @US6-AS2 | passed | 00:00:00.1649224 |
| F004 | The calendar range has 1 to 366 days(n: "03", from: "2026-11-18", to: "2027-11-18", result: "a calendar of 53 commitments, the first on 2026-11"···, __pickleIndex: "174", exampleTags: []) | @F004 @AC-043 @FR-013 @US4-AS7 | passed | 00:00:00.0841258 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "07", value: "`europe/kyiv`", __pickleIndex: "255", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.0846768 |
| F004 | Unpublishing a habit that goals use needs confirmation and gives only the number of goals(n: "01", goals: "`f4.ac057.a.01@example.com` created one goal and `"···, count: "2", text: "\"2 goals use this habit.\"", __pickleIndex: "202", exampleTags: []) | @F004 @AC-057 @FR-033 @US7-AS1 @US7-AS3 @US7-AS5 | passed | 00:00:00.2321018 |
| F001 | Four failed sign-ins in a row do not block | @F001 @AC-020 @FR-008 | passed | 00:00:00.2177767 |
| F004 | A user's list never contains another user's goals | @F004 @AC-030 @FR-020 @US3-AS2 | passed | 00:00:00.2287399 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "06", action: "creates a custom habit with the name `\"   \"` and"···, __pickleIndex: "212", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1912674 |
| F002 | Demo loading run again leaves the demo accounts and their custom habits unchanged | @F002 @AC-065 @FR-031 @US6-AS6 | passed | 00:00:00.4406305 |
| F002 | Browsing by category returns only that category | @F002 @AC-003 @FR-009 @US1-AS3 | passed | 00:00:00.0842297 |
| F002 | Demo loading adds no seed to a catalog that has a habit | @F002 @AC-062 @FR-024 @FR-030 @US6-AS3 | passed | 00:00:00.4197988 |
| F001 | Sign-in with an email that has no account fails with the single message | @F001 @AC-012 @FR-007 @US2-AS2 | passed | 00:00:00.0026567 |
| F001 | Demo loading with a password that breaks the password rule creates no account(demoPassword: "\"exampl1\"", whyItBreaksTheRule: "7 characters, with a digit", __pickleIndex: "88", exampleTags: []) | @F001 @AC-056 @FR-002 @FR-016 @US7-AS4 | passed | 00:00:00.3057017 |
| F004 | Unpublishing with confirmation unpublishes a habit that goals use | @F004 @AC-058 @FR-033 @US7-AS2 | passed | 00:00:00.2308124 |
| F004 | Anonymous visitors and administrators get the catalog without goal information(n: "01", actingUser: "the anonymous visitor", action: "browses the catalog as the anonymous visitor with "···, result: "the result contains `A4.055 01 Reading` with no go"···, __pickleIndex: "195", exampleTags: []) | @F004 @AC-055 @FR-032 @US6-AS4 | passed | 00:00:00.1556577 |
| F001 | Demo loading with a password that breaks the password rule creates no account(demoPassword: "\"examplepass\"", whyItBreaksTheRule: "11 characters, no digit", __pickleIndex: "87", exampleTags: []) | @F001 @AC-056 @FR-002 @FR-016 @US7-AS4 | passed | 00:00:00.3023642 |
| F002 | An unknown identifier does not exist for the administrator(n: "03", action: "publishes", __pickleIndex: "102", exampleTags: []) | @F002 @AC-032 @FR-013 @US3-AS12 | passed | 00:00:00.0755900 |
| F004 | Browsing and details without an acting user give the slice 2 result | @F004 @AC-056 @FR-019 @FR-031 | passed | 00:00:00.1573512 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "04", value: "`Etc/UTC`", __pickleIndex: "252", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.1176766 |
| F004 | Each field refusal of goal creation, with its message(n: "20", settings: "Build, yes/no, 21 times per week", message: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "27", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1532322 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "01", settings: "Build, duration 15 minutes, fixed days Monday to F"···, targetText: "\"15 min, 5 days a week\"", __pickleIndex: "87", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0870900 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@.\"", whyItIsNotValid: "domain has no non-empty part", __pickleIndex: "10", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0003995 |
| F004 | Starting from a catalog habit without a position gives its marked suggested starting goal(n: "01", suggestions: "1. Build, duration 15 minutes, fixed days Monday t"···, __pickleIndex: "0", exampleTags: []) | @F004 @AC-001 @FR-007 @US1-AS1 | passed | 00:00:00.1618807 |
| F004 | An administrator gets no goal use case(n: "02", action: "creates a goal on `A4.031 02 Reading` with Build, "···, __pickleIndex: "130", exampleTags: []) | @F004 @AC-031 @FR-021 @US3-AS3 | passed | 00:00:00.1517514 |
| F002 | Only the 24 habit icons are accepted(n: "25", icon: "`menu`", result: "\"Choose an icon.\"", __pickleIndex: "188", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0746954 |
| F004 | An administrator gets no goal use case(n: "01", action: "starts a new goal from `A4.031 01 Reading`", __pickleIndex: "129", exampleTags: []) | @F004 @AC-031 @FR-021 @US3-AS3 | passed | 00:00:00.1562417 |
| F002 | The Home preview is the first four Published habits in name order | @F002 @AC-012 @FR-008 @FR-024 @US5-AS2 | passed | 00:00:00.4699230 |
| F004 | Each goal follows a time zone change from its own next day or week(n: "01", schedule: "1 time per day", startDate: "2026-11-16", from: "2026-11-18", to: "2026-11-19", entries: "2026-11-18 from `2026-11-17T22:00:00Z` to `2026-11"···, __pickleIndex: "240", exampleTags: []) | @F004 @AC-072 @FR-026 @FR-027 @US10-AS5 | passed | 00:00:00.1210013 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "16", fields: "Reduce, count 5 `cups`, 2 times per week", text: "\"at most 5 cups, 2 times a week\"", kind: "\"Count · flexible\"", __pickleIndex: "43", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.1130098 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "02", value: "`FLE Standard Time`", __pickleIndex: "250", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.0855526 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "18", second: "Build, duration 15 minutes, no schedule", result: "\"Choose fixed days or how many times.\"", __pickleIndex: "133", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0794789 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "05", count: "5", marks: "the third marked", result: "success", after: "5 suggested starting goals, the third marked", __pickleIndex: "113", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0863690 |
| F002 | A registered user cannot manage the catalog(n: "02", action: "reads `A030 02 Draft` as an administrator", __pickleIndex: "89", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1580874 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "22", second: "Build, duration 15 minutes, 3 times with no period", result: "\"Choose per day or per week.\"", __pickleIndex: "137", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0808555 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "03", fields: "Reduce, count 1 `sweet`, 1 time per day", text: "\"at most 1 sweet a day\"", kind: "\"Count · flexible\"", __pickleIndex: "30", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.1022010 |
| F001 | Administrator creation matches the email whatever its case and spaces | @F001 @AC-054 @FR-003 @FR-014 | passed | 00:00:00.0786053 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "05", action: "creates the custom habit `Admin habit`", __pickleIndex: "211", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1664851 |
| F004 | A new goal can start without a habit, but not be created without one | @F004 @AC-063 @FR-006 @FR-007 @US8-AS4 | passed | 00:00:00.0846541 |
| F004 | A per-week goal tells when its first week starts(n: "02", startDate: "2026-11-18", time: "`2026-11-22T23:59:59Z`", list: "first week starts 2026-11-23, no start date and no"···, __pickleIndex: "161", exampleTags: []) | @F004 @AC-039 @FR-014 @FR-016 @US4-AS4 | passed | 00:00:00.0877250 |
| F004 | An anonymous visitor gets no goal use case(n: "01", action: "starts a new goal from `A4.032 01 Reading`", __pickleIndex: "135", exampleTags: []) | @F004 @AC-032 @FR-022 @US3-AS4 | passed | 00:00:00.1654604 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "06", state: "Unpublished", action: "edits `A026 06 Other`", __pickleIndex: "69", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | passed | 00:00:00.0795579 |
| F001 | Administrator creation accepts a password at the lower bound of the password rule | @F001 @AC-040 @FR-002 @FR-014 | passed | 00:00:00.0783155 |
| F004 | Each field refusal of goal creation, with its message(n: "09", settings: "Reduce, count -1 `cups`, 1 time per day", message: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "16", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1537806 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "07", goal: "on `A4.008 07 Reading` with Build, count 20 with a"···, message: "\"The unit can have at most 20 characters.\"", __pickleIndex: "36", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1539102 |
| F001 | Registration refuses an email that is not valid(email: "\"@example.com\"", whyItIsNotValid: "nothing before the \"@\"", __pickleIndex: "18", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002309 |
| F002 | Field checks refuse a custom habit with the same messages(n: "02", name: "a name of 61 characters with the prefix `Habit`", description: "none", category: "Body", intent: "Build", icon: "`droplet`", result: "\"The name can have at most 60 characters.\"", __pickleIndex: "220", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0900178 |
| F001 | Registration accepts a password at the lower bound of the password rule(email: "\"digits.pw@example.com\"", password: "\"12345678\"", whyItMeetsTheRule: "8 characters, all digits", __pickleIndex: "6", exampleTags: []) | @F001 @AC-003 @FR-001 @FR-002 | passed | 00:00:00.0712255 |
| F001 | The anonymous visitor cannot read account details | @F001 @AC-026 @FR-011 @US3-AS1 | passed | 00:00:00.0008511 |
| F004 | A custom habit is looked up only among the acting user's custom habits(n: "03", action: "starts a new goal from", habit: "the identifier of `A4.033 03 Reading`", __pickleIndex: "143", exampleTags: []) | @F004 @AC-033 @FR-008 @US3-AS5 | passed | 00:00:00.2227903 |
| F004 | A goal name has at most 60 characters after trimming(n: "01", name: "a text of 60 characters", result: "creating succeeds, and reading the goal gives that"···, __pickleIndex: "57", exampleTags: []) | @F004 @AC-011 @FR-006 @FR-009 | passed | 00:00:00.1602617 |
| F004 | Use cases check the acting user, the role, the identifier and the fields, in this order(n: "05", actingUser: "`f4.ac035.admin.05@example.com`", action: "creates a goal on `A4.035 05 Draft` with no Build "···, message: "\"Goals are for registered users.\"", __pickleIndex: "151", exampleTags: []) | @F004 @AC-035 @FR-023 | passed | 00:00:00.1463202 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "11", goal: "on `A4.008 11 Reading` with Reduce, count 2 `cups`"···, message: "\"A Reduce goal with an amount has 1 time per peri"···, __pickleIndex: "40", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1966714 |
| F002 | Only the 24 habit icons are accepted(n: "27", icon: "`search`", result: "\"Choose an icon.\"", __pickleIndex: "190", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0730512 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "04", fields: "Reduce, duration 999 minutes, 20 times per day", storedUnit: "minutes", __pickleIndex: "141", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0818844 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "06", settings: "Build, yes/no, fixed days Sunday", targetText: "\"once a week\"", __pickleIndex: "92", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0864958 |
| F004 | Reading a goal gives the version of its current date and the one later version(n: "02", schedule: "fixed days Monday, Wednesday and Friday", startDate: "2026-11-16", time: "`2026-11-19T04:59:59Z`", versions: "the time zone `Europe/Kyiv` from 2026-11-16 and a "···, __pickleIndex: "244", exampleTags: []) | @F004 @AC-073 @FR-016 @FR-026 @US10-AS6 | passed | 00:00:00.1251597 |
| F004 | Each field refusal of goal creation, with its message(n: "08", settings: "Build, duration 0 minutes, 3 times per week", message: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "15", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1500815 |
| F001 | Registration refuses an email that is not valid(email: "\"\"", whyItIsNotValid: "empty", __pickleIndex: "19", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0003736 |
| F004 | Each field refusal of goal creation, with its message(n: "16", settings: "Build, yes/no, no schedule", message: "\"Choose fixed days or how many times.\"", __pickleIndex: "23", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1509469 |
| F004 | The profile gives "member since" with the month and year of creation in the account's time zone(n: "01", time: "`2026-11-17T10:00:00Z`", browserTimeZone: "the browser time zone `Europe/Kyiv`", memberSince: "\"member since November 2026\"", __pickleIndex: "289", exampleTags: []) | @F004 @AC-089 @FR-030 @US11-AS6 | passed | 00:00:00.0784925 |
| F002 | Custom-habit names and descriptions are trimmed before they are checked and stored(n: "03", name: "`\"   \"` + `Habit ` followed by `x` up to 60 char"···, description: "none", storedName: "the 60-character name", storedDescription: "no description", __pickleIndex: "258", exampleTags: []) | @F002 @AC-073 @FR-004 @FR-005 | passed | 00:00:00.0933382 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "02", name: "`\"\"`", description: "none", category: "Body", intent: "Build", icon: "`book-open`", result: "\"Enter a name.\"", __pickleIndex: "71", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0833210 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "05", fields: "Build, duration 1 hours, fixed days Monday, Wednes"···, text: "\"1 h, 3 days a week\"", kind: "\"Duration · fixed days Mon, Wed, Fri\"", __pickleIndex: "32", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.1021937 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "18", settings: "Reduce, yes/no, fixed days Monday to Friday", targetText: "\"avoided on Mon–Fri\"", __pickleIndex: "104", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0827218 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "03", second: "Build, yes/no with the amount 5, 1 time per day", result: "\"A yes/no suggested starting goal has no amount o"···, __pickleIndex: "118", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0762942 |
| F001 | Registration refuses an email that is not valid(email: "\"ana.example.com\"", whyItIsNotValid: "no \"@\"", __pickleIndex: "7", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002294 |
| F004 | After a move west, a fixed-days goal keeps the old time zone up to its current date | @F004 @AC-071 @FR-011 @FR-026 @FR-027 @US10-AS4 | passed | 00:00:00.0906055 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "14", goal: "on `A4.008 14 Reading` with Build, yes/no, fixed d"···, message: "\"Choose a start date within a year from today.\"", __pickleIndex: "43", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1530758 |
| F004 | Use cases check the acting user, the role, the identifier and the fields, in this order(n: "07", actingUser: "`f4.ac035.admin.07@example.com`", action: "starts a new goal from `A4.035 07 Draft` with the "···, message: "\"Goals are for registered users.\"", __pickleIndex: "153", exampleTags: []) | @F004 @AC-035 @FR-023 | passed | 00:00:00.1948049 |
| F002 | Browsing gives the count of the habits it returns | @F002 @AC-011 @FR-008 | passed | 00:00:00.0862725 |
| F002 | The role and the identifier are checked before the fields(n: "01", actingUser: "the registered user `ac039.user.01@example.com`", action: "creates a catalog habit with the name `\"   \"`", result: "\"Only administrators can manage the catalog.\"", __pickleIndex: "149", exampleTags: []) | @F002 @AC-039 @FR-016 @US3-AS18 | passed | 00:00:00.1509966 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "09", goal: "on `A4.008 09 Reading` with Build, yes/no, fixed d"···, message: "\"Choose at least one day.\"", __pickleIndex: "38", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1520244 |
| F004 | The Home greeting follows the time of day in the account's time zone(n: "02", time: "`2026-11-18T03:00:00Z` (05:00 in Kyiv)", greeting: "\"Good morning, Yuri\"", __pickleIndex: "279", exampleTags: []) | @F004 @AC-086 @FR-029 @US11-AS3 | passed | 00:00:00.0779788 |
| F004 | Anonymous visitors and administrators get the catalog without goal information(n: "06", actingUser: "`f4.ac055.admin.06@example.com`", action: "browses the catalog as `f4.ac055.admin.06@example."···, result: "the result is \"Goals are for registered users.\"", __pickleIndex: "200", exampleTags: []) | @F004 @AC-055 @FR-032 @US6-AS4 | passed | 00:00:00.1606523 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "07", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "the identifier of their custom habit `Cold shower`", __pickleIndex: "78", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | passed | 00:00:00.1770220 |
| F002 | The anonymous visitor cannot manage the catalog(n: "02", action: "reads `A031 02 Published` as an administrator", __pickleIndex: "95", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0971846 |
| F004 | The edges of every target range are accepted(n: "07", settings: "Build, yes/no, 20 times per week", targetText: "\"20 times a week\"", __pickleIndex: "50", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | passed | 00:00:00.1565209 |
| F004 | A Reduce goal with an amount has 1 time per period(n: "05", settings: "Build, count 20 `pages`, 3 times per day", result: "creating succeeds", __pickleIndex: "64", exampleTags: []) | @F004 @AC-012 @FR-004 @FR-006 | passed | 00:00:00.1544696 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "05", browserTimeZone: "the browser time zone `Etc/GMT+3`", timeZone: "`UTC`", __pickleIndex: "226", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0791337 |
| F004 | A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "02", schedule: "fixed days Sunday", date: "2027-10-31", entry: "2027-10-31 from `2027-10-30T21:00:00Z` to `2027-10"···, __pickleIndex: "165", exampleTags: []) | @F004 @AC-040 @FR-012 @FR-013 @US4-AS5 | passed | 00:00:00.0843839 |
