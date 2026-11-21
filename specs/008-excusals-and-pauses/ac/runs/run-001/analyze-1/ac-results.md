# AC results

## Current feature (F008)

Total: 149 · passed: 15 · failed: 1 · pending: 133 · skipped: 0

| Feature | Scenario | Tags | Outcome | Duration |
|---|---|---|---|---|
| F008 | Removing an excusal and recording the commitment late gives the figures of an on-time record | @F008 @AC-057 @FR-004 @FR-013 @FR-014 @FR-024 @US3-AS5 | pending | 00:00:00.0015576 |
| F008 | A goal without excusals or pauses keeps slice 6's figures and is counted | @F008 @AC-075 @FR-023 | pending | 00:00:00.0010774 |
| F008 | Excusing a Pending commitment with a reason | @F008 @AC-001 @FR-001 @FR-002 @FR-017 @US1-AS1 | pending | 00:00:00.0017428 |
| F008 | The pause checks come in their order(n: "03", days: "from 2026-11-24 until 2026-11-23", reason: "`x` repeated 201 times", result: "\"The last day cannot be before the first day.\"", __pickleIndex: "52", exampleTags: []) | @F008 @AC-028 @FR-006 | pending | 00:00:00.0006145 |
| F008 | A demo goal whose catalog habit is not Published is left out(n: "01", habits: "the catalog habit \"Drinking water\"", goals: "`Meditation, 10 min daily`", __pickleIndex: "139", exampleTags: []) | @F008 @AC-081 @FR-031 | passed | 00:00:00.7090843 |
| F008 | The first day is compared with the current date on the goal's own timeline(n: "02", zone: "Europe/Kyiv", result: "\"A pause cannot start before today.\"", __pickleIndex: "59", exampleTags: []) | @F008 @AC-030 @FR-006 | passed | 00:00:00.1059141 |
| F008 | A Due entry whose records already exceed a Reduce limit becomes Paused and keeps its records | @F008 @AC-039 @FR-008 @FR-009 | pending | 00:00:00.0013296 |
| F008 | A commitment that is already excused cannot be excused again(n: "01", time: "2026-11-12T11:00:00Z", __pickleIndex: "23", exampleTags: []) | @F008 @AC-013 @FR-002 | pending | 00:00:00.0005938 |
| F008 | An anonymous visitor cannot use any use case of this slice(n: "05", action: "cancels the pause 2026-11-23 – 2026-11-27", __pickleIndex: "124", exampleTags: []) | @F008 @AC-074 @FR-022 @US5-AS3 | pending | 00:00:00.0007607 |
| F008 | The record checks for a pause and an excusal come after the date check, the pause first(n: "02", date: "2026-11-17", result: "\"Nothing is scheduled on this date.\"", __pickleIndex: "39", exampleTags: []) | @F008 @AC-022 @FR-005 @FR-009 | pending | 00:00:00.0014813 |
| F008 | A pause is found only through its own goal(n: "01", actor: "f8.ac051.01.a@example.com", pause: "2026-11-23 – 2026-11-27", __pickleIndex: "84", exampleTags: []) | @F008 @AC-051 @FR-011 @FR-021 @US5-AS4 | pending | 00:00:00.0008699 |
| F008 | Another registered user cannot use any use case of this slice on someone else's goal(n: "05", action: "cancels the pause 2026-11-23 – 2026-11-27", __pickleIndex: "112", exampleTags: []) | @F008 @AC-072 @FR-021 @US5-AS1 | pending | 00:00:00.0010399 |
| F008 | A date in a pause that is not a commitment is refused as not scheduled | @F008 @AC-084 @FR-002 | pending | 00:00:00.0015238 |
| F008 | A date of a Paused week that is not itself paused cannot be recorded | @F008 @AC-034 @FR-005 @FR-009 @US2-AS12 | pending | 00:00:00.0014575 |
| F008 | A final commitment cannot be excused(n: "02", setup: "records Yes for 2026-11-04", date: "2026-11-04", state: "Met", __pickleIndex: "8", exampleTags: []) | @F008 @AC-006 @FR-002 @US1-AS4 | pending | 00:00:00.0006678 |
| F008 | A commitment whose excusal is removed is evaluated as if it had never been excused | @F008 @AC-020 @FR-004 @FR-024 | pending | 00:00:00.0012712 |
| F008 | Another registered user cannot use any use case of this slice on someone else's goal(n: "01", action: "excuses 2026-11-18 without a reason", __pickleIndex: "108", exampleTags: []) | @F008 @AC-072 @FR-021 @US5-AS1 | pending | 00:00:00.0007089 |
| F008 | An anonymous visitor cannot use any use case of this slice(n: "03", action: "pauses the goal from 2026-11-28 until 2026-11-29 w"···, __pickleIndex: "122", exampleTags: []) | @F008 @AC-074 @FR-022 @US5-AS3 | pending | 00:00:00.0024943 |
| F008 | Excusing a Due commitment without a reason | @F008 @AC-002 @FR-001 @FR-002 @US1-AS2 | pending | 00:00:00.0015709 |
| F008 | Another registered user cannot use any use case of this slice on someone else's goal(n: "02", action: "removes the excusal of 2026-11-11", __pickleIndex: "109", exampleTags: []) | @F008 @AC-072 @FR-021 @US5-AS1 | pending | 00:00:00.0013854 |
| F008 | An administrator cannot use any use case of this slice(n: "02", action: "removes the excusal of 2026-11-11", __pickleIndex: "115", exampleTags: []) | @F008 @AC-073 @FR-022 @US5-AS2 | pending | 00:00:00.0007512 |
| F008 | An anonymous visitor cannot use any use case of this slice(n: "04", action: "resumes the goal", __pickleIndex: "123", exampleTags: []) | @F008 @AC-074 @FR-022 @US5-AS3 | pending | 00:00:00.0006661 |
| F008 | An anonymous visitor cannot use any use case of this slice(n: "06", action: "reads the pauses of the goal", __pickleIndex: "125", exampleTags: []) | @F008 @AC-074 @FR-022 @US5-AS3 | pending | 00:00:00.0016848 |
| F008 | The demo figures of `demo6@example.com` hold on every weekday(n: "01", time: "2026-11-16T10:00:00Z", last: "2026-11-21", __pickleIndex: "128", exampleTags: []) | @F008 @AC-077 @FR-031 @US6-AS2 | passed | 00:00:00.5945565 |
| F008 | After its record is deleted, a commitment can be excused | @F008 @AC-015 @FR-002 @US1-AS10 | pending | 00:00:00.0009766 |
| F008 | The first day is compared with the current date on the goal's own timeline(n: "01", zone: "UTC", result: "\"paused\" with the first day 2026-11-15 and the l"···, __pickleIndex: "58", exampleTags: []) | @F008 @AC-030 @FR-006 | passed | 00:00:00.1050108 |
| F008 | Demo pause loading without a valid demo password creates nothing(n: "02", password: "with the demo password `abc`", result: "demo password does not meet the password rule", __pickleIndex: "138", exampleTags: []) | @F008 @AC-080 @FR-031 @US6-AS4 | passed | 00:00:00.4639513 |
| F008 | A yes/no week's target leaves an Excused commitment out | @F008 @AC-083 @FR-016 | pending | 00:00:00.0022878 |
| F008 | A current or past pause cannot be cancelled(n: "02", last: "2026-11-17", kind: "past", __pickleIndex: "81", exampleTags: []) | @F008 @AC-049 @FR-011 @US2-AS7 | pending | 00:00:00.0011712 |
| F008 | A date in a pause cannot be excused, also when it has not begun(n: "02", date: "2026-11-18", __pickleIndex: "21", exampleTags: []) | @F008 @AC-011 @FR-002 @FR-007 | pending | 00:00:00.0009735 |
| F008 | For a goal that has not started, a pause is current or past by the account's today(n: "01", time: "2026-11-16T10:00:00Z", kind: "current", __pickleIndex: "145", exampleTags: []) | @F008 @AC-086 @FR-012 | pending | 00:00:00.0008485 |
| F008 | A pause of several months is allowed and leaves the streak before it unchanged | @F008 @AC-088 @FR-006 @FR-014 | pending | 00:00:00.0012867 |
| F008 | The demo figures of `demo6@example.com` hold on every weekday(n: "06", time: "2026-11-21T10:00:00Z", last: "2026-11-26", __pickleIndex: "133", exampleTags: []) | @F008 @AC-077 @FR-031 @US6-AS2 | passed | 00:00:00.5944454 |
| F008 | A pause is found only through its own goal(n: "04", actor: "f8.ac051.04.b@example.com", pause: "2026-11-16 – 2026-11-20", __pickleIndex: "87", exampleTags: []) | @F008 @AC-051 @FR-011 @FR-021 @US5-AS4 | pending | 00:00:00.0005684 |
| F008 | A commitment excused before a pause covered it is refused as paused, not as already excused | @F008 @AC-012 @FR-002 @FR-007 | pending | 00:00:00.0013894 |
| F008 | Pausing a commitment-based goal makes its commitments in the range Paused | @F008 @AC-031 @FR-006 @FR-007 @US2-AS11 | pending | 00:00:00.0013329 |
| F008 | A record kept on a Paused date can be changed or deleted, and the date stays Paused(n: "01", action: "changes the record of 2026-11-16 to 9", records: "record 9", __pickleIndex: "70", exampleTags: []) | @F008 @AC-041 @FR-009 @FR-017 @US2-AS10 | pending | 00:00:00.0009943 |
| F008 | A goal without a current pause cannot be resumed(n: "01", first: "2026-11-19", last: "2026-11-20", time: "2026-11-16T10:00:00Z", kind: "upcoming", __pickleIndex: "74", exampleTags: []) | @F008 @AC-044 @FR-010 | pending | 00:00:00.0005455 |
| F008 | An Excused commitment whose grace period would have ended this week is not Missed | @F008 @AC-070 @FR-019 | pending | 00:00:00.0008073 |
| F008 | The dashboard expects nothing of a paused goal and does not count it | @F008 @AC-067 @FR-019 @US4-AS2 | pending | 00:00:00.0013446 |
| F008 | A yes/no week whose commitments are all Excused or Paused, at least one Paused, is marked Paused | @F008 @AC-065 @FR-016 | pending | 00:00:00.0009506 |
| F008 | An Upcoming commitment cannot be excused, also with a reason that is too long(n: "03", time: "2026-11-12T10:00:00Z", reason: "`x` repeated 201 times", result: "\"You cannot excuse a commitment that has not begu"···, __pickleIndex: "15", exampleTags: []) | @F008 @AC-008 @FR-002 @US1-AS5 | pending | 00:00:00.0012009 |
| F008 | An Upcoming commitment cannot be excused, also with a reason that is too long(n: "01", time: "2026-11-12T23:59:59Z", reason: "\"Away\"", result: "\"You cannot excuse a commitment that has not begu"···, __pickleIndex: "13", exampleTags: []) | @F008 @AC-008 @FR-002 @US1-AS5 | pending | 00:00:00.0003005 |
| F008 | Resuming in the middle of a week leaves that week Paused, while the goal is not paused | @F008 @AC-045 @FR-007 @FR-010 @FR-018 | pending | 00:00:00.0010314 |
| F008 | A date that is not a commitment of the goal cannot be excused(n: "02", date: "2026-10-30", __pickleIndex: "19", exampleTags: []) | @F008 @AC-010 @FR-002 @US1-AS7 | pending | 00:00:00.0010976 |
| F008 | A pause is found only through its own goal(n: "03", actor: "f8.ac051.03.a@example.com", pause: "2026-11-16 – 2026-11-20", __pickleIndex: "86", exampleTags: []) | @F008 @AC-051 @FR-011 @FR-021 @US5-AS4 | pending | 00:00:00.0017852 |
| F008 | Pausing a goal from today | @F008 @AC-023 @FR-006 @FR-007 @FR-018 @US2-AS1 | pending | 00:00:00.0011669 |
| F008 | The trend marks Excused and Paused bars, which have no value | @F008 @AC-056 @FR-016 @US3-AS4 | pending | 00:00:00.0010467 |
| F008 | An Excused Due commitment is not counted or listed as Due | @F008 @AC-085 @FR-019 | pending | 00:00:00.0019952 |
| F008 | A final commitment cannot be excused(n: "03", setup: "records No for 2026-11-06", date: "2026-11-06", state: "Failed", __pickleIndex: "9", exampleTags: []) | @F008 @AC-006 @FR-002 @US1-AS4 | pending | 00:00:00.0039609 |
| F008 | Excused and Paused results neither extend nor reset a streak | @F008 @AC-054 @FR-014 @US3-AS2 | pending | 00:00:00.0019159 |
| F008 | An administrator cannot use any use case of this slice(n: "01", action: "excuses 2026-11-18 without a reason", __pickleIndex: "114", exampleTags: []) | @F008 @AC-073 @FR-022 @US5-AS2 | pending | 00:00:00.0007862 |
| F008 | The pause checks come in their order(n: "01", days: "from 2026-11-15 with no last day", reason: "\"Away\"", result: "\"Choose the first and the last day.\"", __pickleIndex: "50", exampleTags: []) | @F008 @AC-028 @FR-006 | pending | 00:00:00.0003298 |
| F008 | The Upcoming entry skips a Paused commitment | @F008 @AC-069 @FR-019 @US4-AS4 | pending | 00:00:00.0010429 |
| F008 | A goal that has not started can be resumed from its current pause | @F008 @AC-087 @FR-010 | pending | 00:00:00.0012098 |
| F008 | Demo pause loading creates `demo6@example.com` and names no private data in its results | @F008 @AC-076 @FR-031 @US6-AS1 | passed | 00:00:00.5609505 |
| F008 | Excusing just before, at and just after the end of the grace period(n: "02", time: "2026-11-14T00:00:00Z", result: "\"This commitment is final and can no longer be ex"···, state: "Missed", __pickleIndex: "11", exampleTags: []) | @F008 @AC-007 @FR-002 | pending | 00:00:00.0024190 |
| F008 | The goals overview says which goal is paused and until when | @F008 @AC-066 @FR-018 @US4-AS1 | pending | 00:00:00.0013569 |
| F008 | The pauses of a goal are listed with the latest first day first | @F008 @AC-052 @FR-012 @US2-AS13 | pending | 00:00:00.0025715 |
| F008 | A reason has at most 200 characters(n: "01", reason: "`x` repeated 200 times", result: "\"excused\"", entry: "Excused, reason `x` repeated 200 times", __pickleIndex: "28", exampleTags: []) | @F008 @AC-016 @FR-001 @FR-002 @US1-AS11 | pending | 00:00:00.0008700 |
| F008 | A per-week period is Paused as a whole while one of its dates is paused | @F008 @AC-033 @FR-007 @FR-018 @US2-AS12 | pending | 00:00:00.0014165 |
| F008 | An administrator cannot use any use case of this slice(n: "06", action: "reads the pauses of the goal", __pickleIndex: "119", exampleTags: []) | @F008 @AC-073 @FR-022 @US5-AS2 | pending | 00:00:00.0019819 |
| F008 | The demo figures of `demo6@example.com` hold on every weekday(n: "05", time: "2026-11-20T10:00:00Z", last: "2026-11-25", __pickleIndex: "132", exampleTags: []) | @F008 @AC-077 @FR-031 @US6-AS2 | passed | 00:00:00.6245469 |
| F008 | A pause later in the current week pauses that week, while the goal is not paused and is counted | @F008 @AC-046 @FR-007 @FR-018 @FR-019 | pending | 00:00:00.0013082 |
| F008 | A yes/no week's target and value leave a Paused commitment out | @F008 @AC-062 @FR-016 | pending | 00:00:00.0008461 |
| F008 | The trend of a per-day goal marks its Paused days | @F008 @AC-061 @FR-016 | pending | 00:00:00.0013767 |
| F008 | An excused commitment stays Excused after its grace period would have ended(n: "02", time: "2026-12-01T00:00:00Z", __pickleIndex: "5", exampleTags: []) | @F008 @AC-004 @FR-003 @FR-017 @US1-AS3 | pending | 00:00:00.0009304 |
| F008 | The reason is trimmed, and a reason that is empty after trimming is none(n: "01", given: "\"  Ill  \"", stored: "reason \"Ill\"", __pickleIndex: "2", exampleTags: []) | @F008 @AC-003 @FR-001 | pending | 00:00:00.0012250 |
| F008 | An excused commitment stays Excused after its grace period would have ended(n: "01", time: "2026-11-14T00:00:00Z", __pickleIndex: "4", exampleTags: []) | @F008 @AC-004 @FR-003 @FR-017 @US1-AS3 | pending | 00:00:00.0017139 |
| F008 | A pause is found only through its own goal(n: "02", actor: "f8.ac051.02.b@example.com", pause: "2026-11-23 – 2026-11-27", __pickleIndex: "85", exampleTags: []) | @F008 @AC-051 @FR-011 @FR-021 @US5-AS4 | pending | 00:00:00.0005948 |
| F008 | Resuming a commitment-based goal makes today's commitment Due again | @F008 @AC-032 @FR-010 @US2-AS11 | pending | 00:00:00.0011293 |
| F008 | A pause cannot share a date with another pause of the goal, but may touch it(n: "04", first: "2026-11-23", last: "2026-11-29", result: "\"paused\" with the first day 2026-11-23 and the l"···, __pickleIndex: "46", exampleTags: []) | @F008 @AC-026 @FR-006 @US2-AS4 | pending | 00:00:00.0011250 |
| F008 | Demo pause loading leaves an existing `demo6@example.com` unchanged | @F008 @AC-079 @FR-031 @US6-AS3 | failed | 00:00:00.5433132 |
| F008 | An excusal can be removed while the grace period is open | @F008 @AC-017 @FR-004 @US1-AS8 | pending | 00:00:00.0009620 |
| F008 | A pause needs a first and a last day(n: "01", days: "with no first day until 2026-11-22", __pickleIndex: "47", exampleTags: []) | @F008 @AC-027 @FR-006 | pending | 00:00:00.0009336 |
| F008 | The demo figures of `demo6@example.com` hold on every weekday(n: "04", time: "2026-11-19T10:00:00Z", last: "2026-11-24", __pickleIndex: "131", exampleTags: []) | @F008 @AC-077 @FR-031 @US6-AS2 | passed | 00:00:00.5991003 |
| F008 | An administrator cannot use any use case of this slice(n: "03", action: "pauses the goal from 2026-11-28 until 2026-11-29 w"···, __pickleIndex: "116", exampleTags: []) | @F008 @AC-073 @FR-022 @US5-AS2 | pending | 00:00:00.0009930 |
| F008 | For a goal that has not started, "today" is the account's today(n: "02", first: "2026-11-15", result: "\"A pause cannot start before today.\"", __pickleIndex: "57", exampleTags: []) | @F008 @AC-029 @FR-006 | pending | 00:00:00.0011316 |
| F008 | Another registered user cannot use any use case of this slice on someone else's goal(n: "06", action: "reads the pauses of the goal", __pickleIndex: "113", exampleTags: []) | @F008 @AC-072 @FR-021 @US5-AS1 | pending | 00:00:00.0007599 |
| F008 | A pause never changes a result that was Pending or final when it was set | @F008 @AC-038 @FR-008 @US2-AS8 | pending | 00:00:00.0016029 |
| F008 | Removing an excusal just before, at and just after the end of the grace period(n: "02", time: "2026-11-14T00:00:00Z", result: "\"This excusal can no longer be removed.\"", state: "Excused", __pickleIndex: "32", exampleTags: []) | @F008 @AC-018 @FR-004 @US1-AS8 | pending | 00:00:00.0010836 |
| F008 | One paused day exempts its whole week, also when its records reach the target | @F008 @AC-036 @FR-007 @FR-013 | pending | 00:00:00.0012705 |
| F008 | The record checks for a pause and an excusal come after the date check, the pause first(n: "01", date: "2026-11-16", result: "\"This goal is paused on this date.\"", __pickleIndex: "38", exampleTags: []) | @F008 @AC-022 @FR-005 @FR-009 | pending | 00:00:00.0007246 |
| F008 | A goal without a current pause cannot be resumed(n: "02", first: "2026-11-16", last: "2026-11-16", time: "2026-11-17T10:00:00Z", kind: "past", __pickleIndex: "75", exampleTags: []) | @F008 @AC-044 @FR-010 | pending | 00:00:00.0009110 |
| F008 | A pause cannot share a date with another pause of the goal, but may touch it(n: "01", first: "2026-11-21", last: "2026-11-24", result: "\"This goal already has a pause on these dates.\"", __pickleIndex: "43", exampleTags: []) | @F008 @AC-026 @FR-006 @US2-AS4 | pending | 00:00:00.0006144 |
| F008 | Resuming during a pause ends it on the day before today | @F008 @AC-042 @FR-010 @FR-012 @FR-018 @US2-AS5 | pending | 00:00:00.0013752 |
| F008 | A record kept on a Paused date can be changed or deleted, and the date stays Paused(n: "02", action: "deletes the record of 2026-11-16", records: "no record", __pickleIndex: "71", exampleTags: []) | @F008 @AC-041 @FR-009 @FR-017 @US2-AS10 | pending | 00:00:00.0017479 |
| F008 | A commitment that is already excused cannot be excused again(n: "02", time: "2026-11-14T10:00:00Z", __pickleIndex: "24", exampleTags: []) | @F008 @AC-013 @FR-002 | pending | 00:00:00.0010213 |
| F008 | A pause cannot share a date with another pause of the goal, but may touch it(n: "03", first: "2026-11-17", last: "2026-11-18", result: "\"This goal already has a pause on these dates.\"", __pickleIndex: "45", exampleTags: []) | @F008 @AC-026 @FR-006 @US2-AS4 | pending | 00:00:00.0190646 |
| F008 | The demo figures of `demo6@example.com` hold on every weekday(n: "02", time: "2026-11-17T10:00:00Z", last: "2026-11-22", __pickleIndex: "129", exampleTags: []) | @F008 @AC-077 @FR-031 @US6-AS2 | passed | 00:00:00.6172410 |
| F008 | An excusal applies at every supplied time, also one earlier than the excusing call | @F008 @AC-005 @FR-003 @FR-025 | pending | 00:00:00.0011101 |
| F008 | The demo figures of `demo6@example.com` hold on every weekday(n: "07", time: "2026-11-22T10:00:00Z", last: "2026-11-27", __pickleIndex: "134", exampleTags: []) | @F008 @AC-077 @FR-031 @US6-AS2 | passed | 00:00:00.5702772 |
| F008 | The reason is trimmed, and a reason that is empty after trimming is none(n: "02", given: "\"   \"", stored: "no reason", __pickleIndex: "3", exampleTags: []) | @F008 @AC-003 @FR-001 | pending | 00:00:00.0007667 |
| F008 | An Upcoming commitment cannot be excused, also with a reason that is too long(n: "02", time: "2026-11-13T00:00:00Z", reason: "\"Away\"", result: "\"excused\"", __pickleIndex: "14", exampleTags: []) | @F008 @AC-008 @FR-002 @US1-AS5 | pending | 00:00:00.0005944 |
| F008 | A pause needs a first and a last day(n: "02", days: "from 2026-11-16 with no last day", __pickleIndex: "48", exampleTags: []) | @F008 @AC-027 @FR-006 | pending | 00:00:00.0008046 |
| F008 | Cancelling the upcoming pause that paused the current week makes the week Due again | @F008 @AC-047 @FR-011 @FR-024 | pending | 00:00:00.0013329 |
| F008 | A goal whose every ended result is exempt has "N/A" and streaks of 0 | @F008 @AC-058 @FR-013 @FR-014 @US3-AS6 | pending | 00:00:00.0008976 |
| F008 | Another registered user cannot use any use case of this slice on someone else's goal(n: "04", action: "resumes the goal", __pickleIndex: "111", exampleTags: []) | @F008 @AC-072 @FR-021 @US5-AS1 | pending | 00:00:00.0028232 |
| F008 | Recent notes show Excused and Paused as the state | @F008 @AC-071 @FR-020 | pending | 00:00:00.0013425 |
| F008 | Excusing just before, at and just after the end of the grace period(n: "03", time: "2026-11-14T00:00:01Z", result: "\"This commitment is final and can no longer be ex"···, state: "Missed", __pickleIndex: "12", exampleTags: []) | @F008 @AC-007 @FR-002 | pending | 00:00:00.0011002 |
| F008 | A current or past pause cannot be cancelled(n: "01", last: "2026-11-22", kind: "current", __pickleIndex: "80", exampleTags: []) | @F008 @AC-049 @FR-011 @US2-AS7 | pending | 00:00:00.0006944 |
| F008 | A pause needs a first and a last day(n: "03", days: "with no first day and no last day", __pickleIndex: "49", exampleTags: []) | @F008 @AC-027 @FR-006 | pending | 00:00:00.0005590 |
| F008 | The trend of a per-week goal marks a Paused week | @F008 @AC-060 @FR-016 @US3-AS7 | pending | 00:00:00.0013188 |
| F008 | Cancelling just before and on a pause's first day(n: "01", time: "2026-11-16T23:59:59Z", result: "\"pause cancelled\"", __pickleIndex: "82", exampleTags: []) | @F008 @AC-050 @FR-011 | pending | 00:00:00.0006648 |
| F008 | The pause checks come in their order(n: "06", days: "from 2026-11-17 until 2026-11-18", reason: "`x` repeated 200 times", result: "\"paused\" with the first day 2026-11-17 and the l"···, __pickleIndex: "55", exampleTags: []) | @F008 @AC-028 @FR-006 | pending | 00:00:00.0010349 |
| F008 | An excused commitment leaves the Pending count and the Today list | @F008 @AC-068 @FR-019 @US4-AS3 | pending | 00:00:00.0013202 |
| F008 | An administrator cannot use any use case of this slice(n: "05", action: "cancels the pause 2026-11-23 – 2026-11-27", __pickleIndex: "118", exampleTags: []) | @F008 @AC-073 @FR-022 @US5-AS2 | pending | 00:00:00.0007510 |
| F008 | A commitment that has a record cannot be excused(n: "01", reason: "\"Ill\"", __pickleIndex: "25", exampleTags: []) | @F008 @AC-014 @FR-002 @US1-AS10 | pending | 00:00:00.0016709 |
| F008 | A pause cannot start before today | @F008 @AC-024 @FR-006 @US2-AS2 | pending | 00:00:00.0008529 |
| F008 | Resuming on a pause's first day removes the pause | @F008 @AC-043 @FR-010 @FR-024 @US2-AS6 | pending | 00:00:00.0017857 |
| F008 | The pause checks come in their order(n: "05", days: "from 2026-11-17 until 2026-11-18", reason: "`x` repeated 201 times", result: "\"Keep the reason to 200 characters or fewer.\"", __pickleIndex: "54", exampleTags: []) | @F008 @AC-028 @FR-006 | pending | 00:00:00.0003337 |
| F008 | A reason has at most 200 characters(n: "02", reason: "`x` repeated 201 times", result: "\"Keep the reason to 200 characters or fewer.\"", entry: "Pending", __pickleIndex: "29", exampleTags: []) | @F008 @AC-016 @FR-001 @FR-002 @US1-AS11 | pending | 00:00:00.0037342 |
| F008 | A demo goal whose catalog habit is not Published is left out(n: "02", habits: "the catalog habits \"Meditation\" and \"Drinking w"···, goals: "no goal", __pickleIndex: "140", exampleTags: []) | @F008 @AC-081 @FR-031 | passed | 00:00:00.6596928 |
| F008 | Consistency leaves Excused and Paused results out | @F008 @AC-053 @FR-013 @US3-AS1 | pending | 00:00:00.0014459 |
| F008 | A yes/no week whose only record is on a Paused commitment has no value | @F008 @AC-063 @FR-016 | pending | 00:00:00.0008051 |
| F008 | Resuming a per-week goal in a new week leaves the earlier partly paused week Paused | @F008 @AC-035 @FR-010 @US2-AS12 | pending | 00:00:00.0015961 |
| F008 | A Paused week of a per-week goal is not counted and does not break the streak | @F008 @AC-059 @FR-013 @FR-014 @US3-AS7 | pending | 00:00:00.0011570 |
| F008 | The pause checks come in their order(n: "04", days: "from 2026-11-24 until 2026-11-30", reason: "`x` repeated 201 times", result: "\"This goal already has a pause on these dates.\"", __pickleIndex: "53", exampleTags: []) | @F008 @AC-028 @FR-006 | pending | 00:00:00.0003025 |
| F008 | A date that is not a commitment of the goal cannot be excused(n: "01", date: "2026-11-10", __pickleIndex: "18", exampleTags: []) | @F008 @AC-010 @FR-002 @US1-AS7 | pending | 00:00:00.0027181 |
| F008 | For a goal that has not started, "today" is the account's today(n: "01", first: "2026-11-16", result: "\"paused\" with the first day 2026-11-16 and the l"···, __pickleIndex: "56", exampleTags: []) | @F008 @AC-029 @FR-006 | pending | 00:00:00.0006910 |
| F008 | The pause checks come in their order(n: "02", days: "from 2026-11-15 until 2026-11-14", reason: "\"Away\"", result: "\"A pause cannot start before today.\"", __pickleIndex: "51", exampleTags: []) | @F008 @AC-028 @FR-006 | pending | 00:00:00.0003503 |
| F008 | The last day cannot be before the first day | @F008 @AC-025 @FR-006 @US2-AS3 | pending | 00:00:00.0011684 |
| F008 | A commitment of a period-based goal cannot be excused(n: "02", goal: "`Drinking water, 8 glasses a day`", entry: "2026-11-11 Pending", __pickleIndex: "17", exampleTags: []) | @F008 @AC-009 @FR-002 @US1-AS6 | pending | 00:00:00.0016126 |
| F008 | Removing an excusal just before, at and just after the end of the grace period(n: "01", time: "2026-11-13T23:59:59Z", result: "\"excusal removed\" with the state Pending", state: "Pending", __pickleIndex: "31", exampleTags: []) | @F008 @AC-018 @FR-004 @US1-AS8 | pending | 00:00:00.0017168 |
| F008 | Excusing just before, at and just after the end of the grace period(n: "01", time: "2026-11-13T23:59:59Z", result: "\"excused\"", state: "Excused", __pickleIndex: "10", exampleTags: []) | @F008 @AC-007 @FR-002 | pending | 00:00:00.0004593 |
| F008 | A commitment of a period-based goal cannot be excused(n: "01", goal: "`Yoga, 3 times a week`", entry: "2026-11-09 – 2026-11-15 Due", __pickleIndex: "16", exampleTags: []) | @F008 @AC-009 @FR-002 @US1-AS6 | pending | 00:00:00.0007653 |
| F008 | An excused commitment cannot be recorded | @F008 @AC-021 @FR-005 @US1-AS9 | pending | 00:00:00.0013629 |
| F008 | An excusal under a pause is kept and shows again when the pause is removed | @F008 @AC-037 @FR-007 @FR-010 | pending | 00:00:00.0021299 |
| F008 | An anonymous visitor cannot use any use case of this slice(n: "01", action: "excuses 2026-11-18 without a reason", __pickleIndex: "120", exampleTags: []) | @F008 @AC-074 @FR-022 @US5-AS3 | pending | 00:00:00.0013582 |
| F008 | For a goal that has not started, a pause is current or past by the account's today(n: "02", time: "2026-11-19T10:00:00Z", kind: "past", __pickleIndex: "146", exampleTags: []) | @F008 @AC-086 @FR-012 | pending | 00:00:00.0010994 |
| F008 | Removing an excusal that does not exist(n: "01", date: "2026-11-11", state: "Pending", __pickleIndex: "34", exampleTags: []) | @F008 @AC-019 @FR-004 | pending | 00:00:00.0015121 |
| F008 | A pause cannot share a date with another pause of the goal, but may touch it(n: "02", first: "2026-11-22", last: "2026-11-22", result: "\"This goal already has a pause on these dates.\"", __pickleIndex: "44", exampleTags: []) | @F008 @AC-026 @FR-006 @US2-AS4 | pending | 00:00:00.0004662 |
| F008 | Demo pause loading without a valid demo password creates nothing(n: "01", password: "with no demo password", result: "no demo password configured", __pickleIndex: "137", exampleTags: []) | @F008 @AC-080 @FR-031 @US6-AS4 | passed | 00:00:00.4658611 |
| F008 | The earlier demo accounts get no excusal and no pause | @F008 @AC-082 @FR-030 @US6-AS5 | passed | 00:00:00.8122308 |
| F008 | Removing an excusal just before, at and just after the end of the grace period(n: "03", time: "2026-11-14T00:00:01Z", result: "\"This excusal can no longer be removed.\"", state: "Excused", __pickleIndex: "33", exampleTags: []) | @F008 @AC-018 @FR-004 @US1-AS8 | pending | 00:00:00.0008461 |
| F008 | Recent results include Excused and Paused results and count each state | @F008 @AC-055 @FR-015 @US3-AS3 | pending | 00:00:00.0021176 |
| F008 | A yes/no week whose commitments are all Excused is marked Excused | @F008 @AC-064 @FR-016 | pending | 00:00:00.0008423 |
| F008 | A date in a pause cannot be recorded | @F008 @AC-040 @FR-009 @US2-AS9 | pending | 00:00:00.0010617 |
| F008 | A date in a pause cannot be excused, also when it has not begun(n: "01", date: "2026-11-16", __pickleIndex: "20", exampleTags: []) | @F008 @AC-011 @FR-002 @FR-007 | pending | 00:00:00.0013612 |
| F008 | The demo figures of `demo6@example.com` hold on every weekday(n: "03", time: "2026-11-18T10:00:00Z", last: "2026-11-23", __pickleIndex: "130", exampleTags: []) | @F008 @AC-077 @FR-031 @US6-AS2 | passed | 00:00:00.6276748 |
| F008 | An administrator cannot use any use case of this slice(n: "04", action: "resumes the goal", __pickleIndex: "117", exampleTags: []) | @F008 @AC-073 @FR-022 @US5-AS2 | pending | 00:00:00.0007083 |
| F008 | An anonymous visitor cannot use any use case of this slice(n: "02", action: "removes the excusal of 2026-11-11", __pickleIndex: "121", exampleTags: []) | @F008 @AC-074 @FR-022 @US5-AS3 | pending | 00:00:00.0006893 |
| F008 | A final commitment cannot be excused(n: "01", setup: "records nothing", date: "2026-11-09", state: "Missed", __pickleIndex: "7", exampleTags: []) | @F008 @AC-006 @FR-002 @US1-AS4 | pending | 00:00:00.0010869 |
| F008 | Another registered user cannot use any use case of this slice on someone else's goal(n: "03", action: "pauses the goal from 2026-11-28 until 2026-11-29 w"···, __pickleIndex: "110", exampleTags: []) | @F008 @AC-072 @FR-021 @US5-AS1 | pending | 00:00:00.0008126 |
| F008 | An upcoming pause can be cancelled | @F008 @AC-048 @FR-011 @US2-AS7 | pending | 00:00:00.0010959 |
| F008 | The demo excusals, pauses, states and notes of `demo6@example.com` | @F008 @AC-078 @FR-031 @US6-AS2 | pending | 00:00:00.6571811 |
| F008 | Removing an excusal that does not exist(n: "02", date: "2026-11-09", state: "Missed", __pickleIndex: "35", exampleTags: []) | @F008 @AC-019 @FR-004 | pending | 00:00:00.0008279 |
| F008 | A commitment that has a record cannot be excused(n: "02", reason: "`x` repeated 201 times", __pickleIndex: "26", exampleTags: []) | @F008 @AC-014 @FR-002 @US1-AS10 | pending | 00:00:00.0008574 |
| F008 | Cancelling just before and on a pause's first day(n: "02", time: "2026-11-17T00:00:00Z", result: "\"Only a pause that has not begun can be cancelled"···, __pickleIndex: "83", exampleTags: []) | @F008 @AC-050 @FR-011 | pending | 00:00:00.0012105 |

### F008/AC-057 Removing an excusal and recording the commitment late gives the figures of an on-time record — pending

Step trace:
```
Given the registered user `f8.ac057.user@example.com`, who created the goal `Meditation, 10 min daily`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Meditation, {int} min daily`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalMeditationMinDaily(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-10T08:00:00Z `f8.ac057.user@example.com` excused 2026-11-09 without a reason
-> skipped because of previous errors
And at 2026-11-10T12:00:00Z `f8.ac057.user@example.com` removed the excusal of 2026-11-09
-> skipped because of previous errors
And at 2026-11-13T08:00:00Z `f8.ac057.user@example.com` paused the goal from 2026-11-13 until 2026-11-15 without a reason
-> skipped because of previous errors
And `f8.ac057.user@example.com` records 10 for each date from 2026-11-02 to 2026-11-16 except 2026-11-09, 2026-11-11, 2026-11-13, 2026-11-14 and 2026-11-15
-> skipped because of previous errors
And `f8.ac057.user@example.com` records 4 for 2026-11-09
-> skipped because of previous errors
When at 2026-11-19T12:00:00Z `f8.ac057.user@example.com` reads the report of the goal
-> skipped because of previous errors
Then the consistency is "83% (10 of 12)"
-> skipped because of previous errors
And the longest streak is "7 commitments"
-> skipped because of previous errors
And the current streak is "2 commitments"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.RemovingAnExcusalAndRecordingTheCommitmentLateGivesTheFiguresOfAnOn_TimeRecord() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 610
[non-AC frame(s) hidden]
```

### F008/AC-075 A goal without excusals or pauses keeps slice 6's figures and is counted — pending

Step trace:
```
Given the registered user `f8.ac075.user@example.com`, who created the goal `Meditation, 10 min daily` at 2026-11-01T08:00:00Z with the start date 2026-11-01
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Meditation, {int} min daily` at {DateTime}T{int}:{string}Z with the start date {DateTime}")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalMeditationMinDailyAtTZWithTheStartDate(int p0, int p1, int p2, DateTime p3, int p4, string p5, DateTime p6)
{
    throw new PendingStepException();
}

And `f8.ac075.user@example.com` records 10 for each date from 2026-11-01 to 2026-11-20 except 2026-11-05 and 2026-11-09
-> skipped because of previous errors
And `f8.ac075.user@example.com` records 4 for 2026-11-05
-> skipped because of previous errors
When at 2026-11-23T10:00:00Z `f8.ac075.user@example.com` reads the report of the goal
-> skipped because of previous errors
Then the consistency is "90% (18 of 20)"
-> skipped because of previous errors
And the current streak is "11 commitments" and the longest streak is "11 commitments"
-> skipped because of previous errors
And the goals overview gives `Meditation, 10 min daily` not paused
-> skipped because of previous errors
And the dashboard's number of goals is 1
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AGoalWithoutExcusalsOrPausesKeepsSlice6SFiguresAndIsCounted() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 814
[non-AC frame(s) hidden]
```

### F008/AC-001 Excusing a Pending commitment with a reason — pending

Step trace:
```
Given the registered user `f8.ac001.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-12T10:00:00Z `f8.ac001.user@example.com` excuses 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
Then the result is "excused" with the date 2026-11-11
-> skipped because of previous errors
And the history of the goal from 2026-11-09 to 2026-11-15 gives 2026-11-11 Excused, no record, no grace end, correction no, reason "Ill", removable
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Excused, no record, no grace end, correction no, reason {string}, removable")]
public void ThenTheHistoryOfTheGoalFromToGivesExcusedNoRecordNoGraceEndCorrectionNoReasonRemovable(DateTime p0, DateTime p1, DateTime p2, string ill3)
{
    throw new PendingStepException();
}

And what can be recorded now contains no entry for 2026-11-11
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ExcusingAPendingCommitmentWithAReason() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 10
[non-AC frame(s) hidden]
```

### F008/AC-028 The pause checks come in their order(n: "03", days: "from 2026-11-24 until 2026-11-23", reason: "`x` repeated 201 times", result: "\"The last day cannot be before the first day.\"", __pickleIndex: "52", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac028.03.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac028.03.user@example.com` paused the goal from 2026-11-23 until 2026-11-25 without a reason
-> skipped because of previous errors
When at 2026-11-16T09:00:00Z `f8.ac028.03.user@example.com` pauses the goal from 2026-11-24 until 2026-11-23 with the reason `x` repeated 201 times
-> skipped because of previous errors
Then the result is "The last day cannot be before the first day."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ThePauseChecksComeInTheirOrder(String n, String days, String reason, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 303
[non-AC frame(s) hidden]
```

### F008/AC-039 A Due entry whose records already exceed a Reduce limit becomes Paused and keeps its records — pending

Step trace:
```
Given the registered user `f8.ac039.user@example.com`, who created the goal `Sugar, at most 1 sweet a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Sugar, at most {int} sweet a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalSugarAtMostSweetADay(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac039.user@example.com` recorded 3 for 2026-11-16
-> skipped because of previous errors
When at 2026-11-16T09:00:00Z `f8.ac039.user@example.com` pauses the goal from 2026-11-16 until 2026-11-16 without a reason
-> skipped because of previous errors
Then the result is "paused" with the first day 2026-11-16 and the last day 2026-11-16
-> skipped because of previous errors
And the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Paused, record 3
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Paused, record {int}")]
public void ThenTheHistoryOfTheGoalFromToGivesPausedRecord(DateTime p0, DateTime p1, DateTime p2, int p3)
{
    throw new PendingStepException();
}

And at 2026-11-19T00:00:00Z the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Paused, record 3
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ADueEntryWhoseRecordsAlreadyExceedAReduceLimitBecomesPausedAndKeepsItsRecords() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 414
[non-AC frame(s) hidden]
```

### F008/AC-013 A commitment that is already excused cannot be excused again(n: "01", time: "2026-11-12T11:00:00Z", __pickleIndex: "23", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac013.01.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac013.01.user@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
When at 2026-11-12T11:00:00Z `f8.ac013.01.user@example.com` excuses 2026-11-11 with the reason "Away"
-> skipped because of previous errors
Then the result is "This commitment is already excused."
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToGivesExcusedReason(DateTime p0, DateTime p1, DateTime p2, string ill3)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ACommitmentThatIsAlreadyExcusedCannotBeExcusedAgain(String n, String time, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 141
[non-AC frame(s) hidden]
```

### F008/AC-074 An anonymous visitor cannot use any use case of this slice(n: "05", action: "cancels the pause 2026-11-23 – 2026-11-27", __pickleIndex: "124", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac074.05.a@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac074.05.a@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac074.05.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac074.05.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z the anonymous visitor cancels the pause 2026-11-23 – 2026-11-27 on the goal of `f8.ac074.05.a@example.com`
-> skipped because of previous errors
Then the result is "You are not signed in."
-> skipped because of previous errors
And the pauses of the goal, read by `f8.ac074.05.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac074.05.a@example.com`, gives 2026-11-11 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime}, read by `f{int}\.ac{float}\.a@example\.com`, gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToReadByF_Ac_AExample_ComGivesExcusedReason(DateTime p0, DateTime p1, int p2, Decimal p3, DateTime p4, string ill5)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnAnonymousVisitorCannotUseAnyUseCaseOfThisSlice(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 794
[non-AC frame(s) hidden]
```

### F008/AC-022 The record checks for a pause and an excusal come after the date check, the pause first(n: "02", date: "2026-11-17", result: "\"Nothing is scheduled on this date.\"", __pickleIndex: "39", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac022.02.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T07:00:00Z `f8.ac022.02.user@example.com` excused 2026-11-16 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac022.02.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 without a reason
-> skipped because of previous errors
When at 2026-11-17T10:00:00Z `f8.ac022.02.user@example.com` records Yes for 2026-11-17
-> skipped because of previous errors
Then the result is "Nothing is scheduled on this date."
-> skipped because of previous errors
And the history of the goal from 2026-11-16 to 2026-11-18 gives 2026-11-16 Paused, no record and 2026-11-18 Paused, no record, and no entry for 2026-11-17
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Paused, no record and {DateTime} Paused, no record, and no entry for {DateTime}")]
public void ThenTheHistoryOfTheGoalFromToGivesPausedNoRecordAndPausedNoRecordAndNoEntryFor(DateTime p0, DateTime p1, DateTime p2, DateTime p3, DateTime p4)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.TheRecordChecksForAPauseAndAnExcusalComeAfterTheDateCheckThePauseFirst(String n, String date, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 241
[non-AC frame(s) hidden]
```

### F008/AC-051 A pause is found only through its own goal(n: "01", actor: "f8.ac051.01.a@example.com", pause: "2026-11-23 – 2026-11-27", __pickleIndex: "84", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac051.01.a@example.com`, who created the goal `Yoga, 3 days a week` and the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week` and the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeekAndTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2, int p3)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac051.01.a@example.com` paused `Yoga, 3 days a week` from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac051.01.a@example.com` paused `Yoga, 3 days a week` from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
And the registered user `f8.ac051.01.b@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.b@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_BExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-16T09:00:00Z `f8.ac051.01.a@example.com` cancels the pause 2026-11-23 – 2026-11-27 through their goal `Drinking water, 8 glasses a day`
-> skipped because of previous errors
Then the result is "This pause does not exist."
-> skipped because of previous errors
And the pauses of `Yoga, 3 days a week` of `f8.ac051.01.a@example.com` are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.APauseIsFoundOnlyThroughItsOwnGoal(String n, String actor, String pause, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 543
[non-AC frame(s) hidden]
```

### F008/AC-072 Another registered user cannot use any use case of this slice on someone else's goal(n: "05", action: "cancels the pause 2026-11-23 – 2026-11-27", __pickleIndex: "112", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac072.05.a@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac072.05.a@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac072.05.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac072.05.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
And the registered user `f8.ac072.05.b@example.com`
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac072.05.b@example.com` cancels the pause 2026-11-23 – 2026-11-27 on the goal of `f8.ac072.05.a@example.com`
-> skipped because of previous errors
Then the result is "This goal does not exist."
-> skipped because of previous errors
And the pauses of the goal, read by `f8.ac072.05.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac072.05.a@example.com`, gives 2026-11-11 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime}, read by `f{int}\.ac{float}\.a@example\.com`, gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToReadByF_Ac_AExample_ComGivesExcusedReason(DateTime p0, DateTime p1, int p2, Decimal p3, DateTime p4, string ill5)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnotherRegisteredUserCannotUseAnyUseCaseOfThisSliceOnSomeoneElsesGoal(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 753
[non-AC frame(s) hidden]
```

### F008/AC-084 A date in a pause that is not a commitment is refused as not scheduled — pending

Step trace:
```
Given the registered user `f8.ac084.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac084.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 without a reason
-> skipped because of previous errors
When at 2026-11-17T10:00:00Z `f8.ac084.user@example.com` excuses 2026-11-17 without a reason
-> skipped because of previous errors
Then the result is "Nothing is scheduled on this date."
-> skipped because of previous errors
And the history of the goal from 2026-11-16 to 2026-11-18 gives 2026-11-16 Paused and 2026-11-18 Paused, and no entry for 2026-11-17
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Paused and {DateTime} Paused, and no entry for {DateTime}")]
public void ThenTheHistoryOfTheGoalFromToGivesPausedAndPausedAndNoEntryFor(DateTime p0, DateTime p1, DateTime p2, DateTime p3, DateTime p4)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ADateInAPauseThatIsNotACommitmentIsRefusedAsNotScheduled() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 908
[non-AC frame(s) hidden]
```

### F008/AC-034 A date of a Paused week that is not itself paused cannot be recorded — pending

Step trace:
```
Given the registered user `f8.ac034.user@example.com`, who created the goal `Yoga, 3 times a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} times a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaTimesAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-18T08:00:00Z `f8.ac034.user@example.com` paused the goal from 2026-11-19 until 2026-11-25 without a reason
-> skipped because of previous errors
When at 2026-11-18T09:00:00Z `f8.ac034.user@example.com` records Yes for 2026-11-18
-> skipped because of previous errors
Then the result is "This week is paused."
-> skipped because of previous errors
And the history of the goal from 2026-11-16 to 2026-11-22 gives 2026-11-16 – 2026-11-22 Paused, no record
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} – {DateTime} Paused, no record")]
public void ThenTheHistoryOfTheGoalFromToGivesPausedNoRecord(DateTime p0, DateTime p1, DateTime p2, DateTime p3)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ADateOfAPausedWeekThatIsNotItselfPausedCannotBeRecorded() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 367
[non-AC frame(s) hidden]
```

### F008/AC-006 A final commitment cannot be excused(n: "02", setup: "records Yes for 2026-11-04", date: "2026-11-04", state: "Met", __pickleIndex: "8", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac006.02.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And `f8.ac006.02.user@example.com` records Yes for 2026-11-04
-> skipped because of previous errors
When at 2026-11-12T10:00:00Z `f8.ac006.02.user@example.com` excuses 2026-11-04 without a reason
-> skipped because of previous errors
Then the result is "This commitment is final and can no longer be excused."
-> skipped because of previous errors
And the history of the goal from 2026-11-04 to 2026-11-04 gives 2026-11-04 Met
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Met")]
public void ThenTheHistoryOfTheGoalFromToGivesMet(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AFinalCommitmentCannotBeExcused(String n, String setup, String date, String state, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 57
[non-AC frame(s) hidden]
```

### F008/AC-020 A commitment whose excusal is removed is evaluated as if it had never been excused — pending

Step trace:
```
Given the registered user `f8.ac020.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-13T07:00:00Z `f8.ac020.user@example.com` excused 2026-11-13 without a reason
-> skipped because of previous errors
And at 2026-11-13T08:00:00Z `f8.ac020.user@example.com` removed the excusal of 2026-11-13, with the result "excusal removed" and the state Due
-> skipped because of previous errors
And at 2026-11-13T09:00:00Z `f8.ac020.user@example.com` recorded Yes for 2026-11-13
-> skipped because of previous errors
When at 2026-11-16T00:00:00Z `f8.ac020.user@example.com` reads the history of the goal from 2026-11-13 to 2026-11-13
-> skipped because of previous errors
Then it gives 2026-11-13 Met, record Yes
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ACommitmentWhoseExcusalIsRemovedIsEvaluatedAsIfItHadNeverBeenExcused() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 224
[non-AC frame(s) hidden]
```

### F008/AC-072 Another registered user cannot use any use case of this slice on someone else's goal(n: "01", action: "excuses 2026-11-18 without a reason", __pickleIndex: "108", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac072.01.a@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac072.01.a@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac072.01.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac072.01.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
And the registered user `f8.ac072.01.b@example.com`
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac072.01.b@example.com` excuses 2026-11-18 without a reason on the goal of `f8.ac072.01.a@example.com`
-> skipped because of previous errors
Then the result is "This goal does not exist."
-> skipped because of previous errors
And the pauses of the goal, read by `f8.ac072.01.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac072.01.a@example.com`, gives 2026-11-11 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime}, read by `f{int}\.ac{float}\.a@example\.com`, gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToReadByF_Ac_AExample_ComGivesExcusedReason(DateTime p0, DateTime p1, int p2, Decimal p3, DateTime p4, string ill5)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnotherRegisteredUserCannotUseAnyUseCaseOfThisSliceOnSomeoneElsesGoal(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 753
[non-AC frame(s) hidden]
```

### F008/AC-074 An anonymous visitor cannot use any use case of this slice(n: "03", action: "pauses the goal from 2026-11-28 until 2026-11-29 w"···, __pickleIndex: "122", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac074.03.a@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac074.03.a@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac074.03.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac074.03.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z the anonymous visitor pauses the goal from 2026-11-28 until 2026-11-29 without a reason on the goal of `f8.ac074.03.a@example.com`
-> skipped because of previous errors
Then the result is "You are not signed in."
-> skipped because of previous errors
And the pauses of the goal, read by `f8.ac074.03.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac074.03.a@example.com`, gives 2026-11-11 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime}, read by `f{int}\.ac{float}\.a@example\.com`, gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToReadByF_Ac_AExample_ComGivesExcusedReason(DateTime p0, DateTime p1, int p2, Decimal p3, DateTime p4, string ill5)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnAnonymousVisitorCannotUseAnyUseCaseOfThisSlice(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 794
[non-AC frame(s) hidden]
```

### F008/AC-002 Excusing a Due commitment without a reason — pending

Step trace:
```
Given the registered user `f8.ac002.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-13T07:00:00Z `f8.ac002.user@example.com` excuses 2026-11-13 without a reason
-> skipped because of previous errors
Then the result is "excused" with the date 2026-11-13
-> skipped because of previous errors
And the history of the goal from 2026-11-13 to 2026-11-13 gives 2026-11-13 Excused, no reason, removable
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Excused, no reason, removable")]
public void ThenTheHistoryOfTheGoalFromToGivesExcusedNoReasonRemovable(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}

And what can be recorded now contains no entry for 2026-11-13
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ExcusingADueCommitmentWithoutAReason() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 18
[non-AC frame(s) hidden]
```

### F008/AC-072 Another registered user cannot use any use case of this slice on someone else's goal(n: "02", action: "removes the excusal of 2026-11-11", __pickleIndex: "109", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac072.02.a@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac072.02.a@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac072.02.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac072.02.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
And the registered user `f8.ac072.02.b@example.com`
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac072.02.b@example.com` removes the excusal of 2026-11-11 on the goal of `f8.ac072.02.a@example.com`
-> skipped because of previous errors
Then the result is "This goal does not exist."
-> skipped because of previous errors
And the pauses of the goal, read by `f8.ac072.02.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac072.02.a@example.com`, gives 2026-11-11 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime}, read by `f{int}\.ac{float}\.a@example\.com`, gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToReadByF_Ac_AExample_ComGivesExcusedReason(DateTime p0, DateTime p1, int p2, Decimal p3, DateTime p4, string ill5)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnotherRegisteredUserCannotUseAnyUseCaseOfThisSliceOnSomeoneElsesGoal(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 753
[non-AC frame(s) hidden]
```

### F008/AC-073 An administrator cannot use any use case of this slice(n: "02", action: "removes the excusal of 2026-11-11", __pickleIndex: "115", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac073.02.a@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac073.02.a@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac073.02.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac073.02.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
And the administrator `f8.ac073.02.admin@example.com`
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac073.02.admin@example.com` removes the excusal of 2026-11-11 on the goal of `f8.ac073.02.a@example.com`
-> skipped because of previous errors
Then the result is "Goals are for registered users."
-> skipped because of previous errors
And the pauses of the goal, read by `f8.ac073.02.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac073.02.a@example.com`, gives 2026-11-11 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime}, read by `f{int}\.ac{float}\.a@example\.com`, gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToReadByF_Ac_AExample_ComGivesExcusedReason(DateTime p0, DateTime p1, int p2, Decimal p3, DateTime p4, string ill5)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnAdministratorCannotUseAnyUseCaseOfThisSlice(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 774
[non-AC frame(s) hidden]
```

### F008/AC-074 An anonymous visitor cannot use any use case of this slice(n: "04", action: "resumes the goal", __pickleIndex: "123", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac074.04.a@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac074.04.a@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac074.04.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac074.04.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z the anonymous visitor resumes the goal on the goal of `f8.ac074.04.a@example.com`
-> skipped because of previous errors
Then the result is "You are not signed in."
-> skipped because of previous errors
And the pauses of the goal, read by `f8.ac074.04.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac074.04.a@example.com`, gives 2026-11-11 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime}, read by `f{int}\.ac{float}\.a@example\.com`, gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToReadByF_Ac_AExample_ComGivesExcusedReason(DateTime p0, DateTime p1, int p2, Decimal p3, DateTime p4, string ill5)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnAnonymousVisitorCannotUseAnyUseCaseOfThisSlice(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 794
[non-AC frame(s) hidden]
```

### F008/AC-074 An anonymous visitor cannot use any use case of this slice(n: "06", action: "reads the pauses of the goal", __pickleIndex: "125", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac074.06.a@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac074.06.a@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac074.06.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac074.06.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z the anonymous visitor reads the pauses of the goal on the goal of `f8.ac074.06.a@example.com`
-> skipped because of previous errors
Then the result is "You are not signed in."
-> skipped because of previous errors
And the pauses of the goal, read by `f8.ac074.06.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac074.06.a@example.com`, gives 2026-11-11 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime}, read by `f{int}\.ac{float}\.a@example\.com`, gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToReadByF_Ac_AExample_ComGivesExcusedReason(DateTime p0, DateTime p1, int p2, Decimal p3, DateTime p4, string ill5)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnAnonymousVisitorCannotUseAnyUseCaseOfThisSlice(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 794
[non-AC frame(s) hidden]
```

### F008/AC-015 After its record is deleted, a commitment can be excused — pending

Step trace:
```
Given the registered user `f8.ac015.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T09:00:00Z `f8.ac015.user@example.com` recorded No for 2026-11-11
-> skipped because of previous errors
And at 2026-11-12T10:00:00Z `f8.ac015.user@example.com` deleted the record of 2026-11-11
-> skipped because of previous errors
When at 2026-11-12T10:05:00Z `f8.ac015.user@example.com` excuses 2026-11-11 without a reason
-> skipped because of previous errors
Then the result is "excused" with the date 2026-11-11
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Excused, no record
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Excused, no record")]
public void ThenTheHistoryOfTheGoalFromToGivesExcusedNoRecord(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AfterItsRecordIsDeletedACommitmentCanBeExcused() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 168
[non-AC frame(s) hidden]
```

### F008/AC-083 A yes/no week's target leaves an Excused commitment out — pending

Step trace:
```
Given the registered user `f8.ac083.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T07:00:00Z `f8.ac083.user@example.com` excused 2026-11-16 without a reason
-> skipped because of previous errors
And `f8.ac083.user@example.com` records Yes for 2026-11-18 and 2026-11-20
-> skipped because of previous errors
When at 2026-11-23T10:00:00Z `f8.ac083.user@example.com` reads the report of the goal
-> skipped because of previous errors
Then the trend contains (2026-11-16 – 2026-11-22, 2, target 2)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AYesNoWeeksTargetLeavesAnExcusedCommitmentOut() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 900
[non-AC frame(s) hidden]
```

### F008/AC-049 A current or past pause cannot be cancelled(n: "02", last: "2026-11-17", kind: "past", __pickleIndex: "81", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac049.02.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T09:00:00Z `f8.ac049.02.user@example.com` paused the goal from 2026-11-16 until 2026-11-17 with the reason "Family visit"
-> skipped because of previous errors
When at 2026-11-18T10:00:00Z `f8.ac049.02.user@example.com` cancels the pause 2026-11-16 – 2026-11-17
-> skipped because of previous errors
Then the result is "Only a pause that has not begun can be cancelled. Resume the goal instead."
-> skipped because of previous errors
And the pauses of the goal are (2026-11-16, 2026-11-17, reason "Family visit", past)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ACurrentOrPastPauseCannotBeCancelled(String n, String last, String kind, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 516
[non-AC frame(s) hidden]
```

### F008/AC-011 A date in a pause cannot be excused, also when it has not begun(n: "02", date: "2026-11-18", __pickleIndex: "21", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac011.02.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac011.02.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 without a reason
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac011.02.user@example.com` excuses 2026-11-18 without a reason
-> skipped because of previous errors
Then the result is "This goal is paused on this date."
-> skipped because of previous errors
And the history of the goal from 2026-11-18 to 2026-11-18 gives 2026-11-18 Paused
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Paused")]
public void ThenTheHistoryOfTheGoalFromToGivesPaused(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ADateInAPauseCannotBeExcusedAlsoWhenItHasNotBegun(String n, String date, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 119
[non-AC frame(s) hidden]
```

### F008/AC-086 For a goal that has not started, a pause is current or past by the account's today(n: "01", time: "2026-11-16T10:00:00Z", kind: "current", __pickleIndex: "145", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac086.01.user@example.com`, who created the goal `Drinking water, 8 glasses a day` at 2026-11-16T08:00:00Z with the start date 2026-11-20
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day` at {DateTime}T{int}:{string}Z with the start date {DateTime}")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADayAtTZWithTheStartDate(int p0, Decimal p1, int p2, DateTime p3, int p4, string p5, DateTime p6)
{
    throw new PendingStepException();
}

And at 2026-11-16T09:00:00Z `f8.ac086.01.user@example.com` paused the goal from 2026-11-16 until 2026-11-18 without a reason
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac086.01.user@example.com` reads the pauses of the goal
-> skipped because of previous errors
Then they are (2026-11-16, 2026-11-18, no reason, current)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ForAGoalThatHasNotStartedAPauseIsCurrentOrPastByTheAccountsToday(String n, String time, String kind, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 924
[non-AC frame(s) hidden]
```

### F008/AC-088 A pause of several months is allowed and leaves the streak before it unchanged — pending

Step trace:
```
Given the registered user `f8.ac088.user@example.com`, who created the goal `Meditation, 10 min daily` at 2026-08-03T08:00:00Z with the start date 2026-08-03
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Meditation, {int} min daily` at {DateTime}T{int}:{string}Z with the start date {DateTime}")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalMeditationMinDailyAtTZWithTheStartDate(int p0, int p1, int p2, DateTime p3, int p4, string p5, DateTime p6)
{
    throw new PendingStepException();
}

And at 2026-08-07T20:00:00Z `f8.ac088.user@example.com` recorded 10 for each date from 2026-08-03 to 2026-08-07
-> skipped because of previous errors
And at 2026-08-08T08:00:00Z `f8.ac088.user@example.com` paused the goal from 2026-08-08 until 2026-11-15 without a reason, with the result "paused" with the first day 2026-08-08 and the last day 2026-11-15
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac088.user@example.com` reads the report of the goal
-> skipped because of previous errors
Then the current streak is "5 commitments" and the longest streak is "5 commitments"
-> skipped because of previous errors
And the pauses of the goal are (2026-08-08, 2026-11-15, no reason, past)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.APauseOfSeveralMonthsIsAllowedAndLeavesTheStreakBeforeItUnchanged() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 946
[non-AC frame(s) hidden]
```

### F008/AC-051 A pause is found only through its own goal(n: "04", actor: "f8.ac051.04.b@example.com", pause: "2026-11-16 – 2026-11-20", __pickleIndex: "87", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac051.04.a@example.com`, who created the goal `Yoga, 3 days a week` and the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week` and the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeekAndTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2, int p3)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac051.04.a@example.com` paused `Yoga, 3 days a week` from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac051.04.a@example.com` paused `Yoga, 3 days a week` from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
And the registered user `f8.ac051.04.b@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.b@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_BExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-16T09:00:00Z `f8.ac051.04.b@example.com` cancels the pause 2026-11-16 – 2026-11-20 through their goal `Drinking water, 8 glasses a day`
-> skipped because of previous errors
Then the result is "This pause does not exist."
-> skipped because of previous errors
And the pauses of `Yoga, 3 days a week` of `f8.ac051.04.a@example.com` are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.APauseIsFoundOnlyThroughItsOwnGoal(String n, String actor, String pause, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 543
[non-AC frame(s) hidden]
```

### F008/AC-012 A commitment excused before a pause covered it is refused as paused, not as already excused — pending

Step trace:
```
Given the registered user `f8.ac012.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T07:00:00Z `f8.ac012.user@example.com` excused 2026-11-16 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac012.user@example.com` paused the goal from 2026-11-16 until 2026-11-16 without a reason
-> skipped because of previous errors
When at 2026-11-16T09:00:00Z `f8.ac012.user@example.com` excuses 2026-11-16 without a reason
-> skipped because of previous errors
Then the result is "This goal is paused on this date."
-> skipped because of previous errors
And the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Paused
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Paused")]
public void ThenTheHistoryOfTheGoalFromToGivesPaused(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ACommitmentExcusedBeforeAPauseCoveredItIsRefusedAsPausedNotAsAlreadyExcused() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 133
[non-AC frame(s) hidden]
```

### F008/AC-031 Pausing a commitment-based goal makes its commitments in the range Paused — pending

Step trace:
```
Given the registered user `f8.ac031.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-18T08:00:00Z `f8.ac031.user@example.com` pauses the goal from 2026-11-18 until 2026-11-20 without a reason
-> skipped because of previous errors
Then the result is "paused" with the first day 2026-11-18 and the last day 2026-11-20
-> skipped because of previous errors
And the history of the goal from 2026-11-16 to 2026-11-22 gives 2026-11-16 Pending, 2026-11-18 Paused and 2026-11-20 Paused
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Pending, {DateTime} Paused and {DateTime} Paused")]
public void ThenTheHistoryOfTheGoalFromToGivesPendingPausedAndPaused(DateTime p0, DateTime p1, DateTime p2, DateTime p3, DateTime p4)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.PausingACommitment_BasedGoalMakesItsCommitmentsInTheRangePaused() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 341
[non-AC frame(s) hidden]
```

### F008/AC-041 A record kept on a Paused date can be changed or deleted, and the date stays Paused(n: "01", action: "changes the record of 2026-11-16 to 9", records: "record 9", __pickleIndex: "70", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac041.01.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac041.01.user@example.com` recorded 8 for 2026-11-16
-> skipped because of previous errors
And at 2026-11-16T09:00:00Z `f8.ac041.01.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 without a reason
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac041.01.user@example.com` changes the record of 2026-11-16 to 9
-> skipped because of previous errors
Then the result is the state Paused, correction no
-> skipped because of previous errors
And the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Paused, record 9, correction no
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Paused, record {int}, correction no")]
public void ThenTheHistoryOfTheGoalFromToGivesPausedRecordCorrectionNo(DateTime p0, DateTime p1, DateTime p2, int p3)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ARecordKeptOnAPausedDateCanBeChangedOrDeletedAndTheDateStaysPaused(String n, String action, String records, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 431
[non-AC frame(s) hidden]
```

### F008/AC-044 A goal without a current pause cannot be resumed(n: "01", first: "2026-11-19", last: "2026-11-20", time: "2026-11-16T10:00:00Z", kind: "upcoming", __pickleIndex: "74", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac044.01.user@example.com`, who created the goal `Yoga, 3 times a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} times a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaTimesAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac044.01.user@example.com` paused the goal from 2026-11-19 until 2026-11-20 without a reason
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac044.01.user@example.com` resumes the goal
-> skipped because of previous errors
Then the result is "This goal is not paused."
-> skipped because of previous errors
And the pauses of the goal are (2026-11-19, 2026-11-20, no reason, upcoming)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AGoalWithoutACurrentPauseCannotBeResumed(String n, String first, String last, String time, String kind, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 466
[non-AC frame(s) hidden]
```

### F008/AC-070 An Excused commitment whose grace period would have ended this week is not Missed — pending

Step trace:
```
Given the registered user `f8.ac070.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-13T07:00:00Z `f8.ac070.user@example.com` excused 2026-11-13 without a reason
-> skipped because of previous errors
When at 2026-11-18T10:00:00Z `f8.ac070.user@example.com` reads the dashboard
-> skipped because of previous errors
Then the Missed count is 0
-> skipped because of previous errors
And the Today list contains (`Yoga, 3 days a week`, 2026-11-16, Pending) and (`Yoga, 3 days a week`, 2026-11-18, Due) and no entry for 2026-11-13
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnExcusedCommitmentWhoseGracePeriodWouldHaveEndedThisWeekIsNotMissed() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 730
[non-AC frame(s) hidden]
```

### F008/AC-067 The dashboard expects nothing of a paused goal and does not count it — pending

Step trace:
```
Given the registered user `f8.ac067.user@example.com`, who created the goal `Drinking water, 8 glasses a day` and the goal `Meditation, 10 min daily`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day` and the goal `Meditation, {int} min daily`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADayAndTheGoalMeditationMinDaily(int p0, int p1, int p2, int p3)
{
    throw new PendingStepException();
}

And at 2026-11-16T09:00:00Z `f8.ac067.user@example.com` paused `Drinking water, 8 glasses a day` from 2026-11-16 until 2026-11-22 without a reason
-> skipped because of previous errors
And `f8.ac067.user@example.com` records on target for each date from 2026-11-02 to 2026-11-13: 8 for `Drinking water, 8 glasses a day` and 10 for `Meditation, 10 min daily`
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac067.user@example.com` reads the dashboard
-> skipped because of previous errors
Then the number of goals is 1
-> skipped because of previous errors
And the Due count is 1 and the Pending count is 4
-> skipped because of previous errors
And the Today list contains (`Drinking water, 8 glasses a day`, 2026-11-14, Pending), (`Drinking water, 8 glasses a day`, 2026-11-15, Pending) and (`Meditation, 10 min daily`, 2026-11-16, Due)
-> skipped because of previous errors
And the Today list contains no Due and no Upcoming entry of `Drinking water, 8 glasses a day`
-> skipped because of previous errors
And the summary of `Drinking water, 8 glasses a day` says it is paused, and the summary of `Meditation, 10 min daily` says it is not paused
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.TheDashboardExpectsNothingOfAPausedGoalAndDoesNotCountIt() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 703
[non-AC frame(s) hidden]
```

### F008/AC-065 A yes/no week whose commitments are all Excused or Paused, at least one Paused, is marked Paused — pending

Step trace:
```
Given the registered user `f8.ac065.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T07:00:00Z `f8.ac065.user@example.com` excused 2026-11-16 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac065.user@example.com` paused the goal from 2026-11-17 until 2026-11-22 without a reason
-> skipped because of previous errors
When at 2026-11-23T10:00:00Z `f8.ac065.user@example.com` reads the report of the goal
-> skipped because of previous errors
Then the trend contains (2026-11-16 – 2026-11-22, Paused)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AYesNoWeekWhoseCommitmentsAreAllExcusedOrPausedAtLeastOnePausedIsMarkedPaused() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 682
[non-AC frame(s) hidden]
```

### F008/AC-008 An Upcoming commitment cannot be excused, also with a reason that is too long(n: "03", time: "2026-11-12T10:00:00Z", reason: "`x` repeated 201 times", result: "\"You cannot excuse a commitment that has not begu"···, __pickleIndex: "15", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac008.03.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-12T10:00:00Z `f8.ac008.03.user@example.com` excuses 2026-11-13 with the reason `x` repeated 201 times
-> skipped because of previous errors
Then the result is "You cannot excuse a commitment that has not begun."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnUpcomingCommitmentCannotBeExcusedAlsoWithAReasonThatIsTooLong(String n, String time, String reason, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 82
[non-AC frame(s) hidden]
```

### F008/AC-008 An Upcoming commitment cannot be excused, also with a reason that is too long(n: "01", time: "2026-11-12T23:59:59Z", reason: "\"Away\"", result: "\"You cannot excuse a commitment that has not begu"···, __pickleIndex: "13", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac008.01.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-12T23:59:59Z `f8.ac008.01.user@example.com` excuses 2026-11-13 with the reason "Away"
-> skipped because of previous errors
Then the result is "You cannot excuse a commitment that has not begun."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnUpcomingCommitmentCannotBeExcusedAlsoWithAReasonThatIsTooLong(String n, String time, String reason, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 82
[non-AC frame(s) hidden]
```

### F008/AC-045 Resuming in the middle of a week leaves that week Paused, while the goal is not paused — pending

Step trace:
```
Given the registered user `f8.ac045.user@example.com`, who created the goal `Yoga, 3 times a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} times a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaTimesAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac045.user@example.com` paused the goal from 2026-11-16 until 2026-11-29 without a reason
-> skipped because of previous errors
When at 2026-11-18T10:00:00Z `f8.ac045.user@example.com` resumes the goal
-> skipped because of previous errors
Then the result is "resumed"
-> skipped because of previous errors
And the pauses of the goal are (2026-11-16, 2026-11-17, no reason, past)
-> skipped because of previous errors
And the history of the goal from 2026-11-16 to 2026-11-29 gives 2026-11-16 – 2026-11-22 Paused and 2026-11-23 – 2026-11-29 Upcoming
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} – {DateTime} Paused and {DateTime} – {DateTime} Upcoming")]
public void ThenTheHistoryOfTheGoalFromToGivesPausedAndUpcoming(DateTime p0, DateTime p1, DateTime p2, DateTime p3, DateTime p4, DateTime p5)
{
    throw new PendingStepException();
}

And the goals overview gives `Yoga, 3 times a week` not paused
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ResumingInTheMiddleOfAWeekLeavesThatWeekPausedWhileTheGoalIsNotPaused() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 481
[non-AC frame(s) hidden]
```

### F008/AC-010 A date that is not a commitment of the goal cannot be excused(n: "02", date: "2026-10-30", __pickleIndex: "19", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac010.02.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-12T10:00:00Z `f8.ac010.02.user@example.com` excuses 2026-10-30 without a reason
-> skipped because of previous errors
Then the result is "Nothing is scheduled on this date."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ADateThatIsNotACommitmentOfTheGoalCannotBeExcused(String n, String date, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 106
[non-AC frame(s) hidden]
```

### F008/AC-051 A pause is found only through its own goal(n: "03", actor: "f8.ac051.03.a@example.com", pause: "2026-11-16 – 2026-11-20", __pickleIndex: "86", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac051.03.a@example.com`, who created the goal `Yoga, 3 days a week` and the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week` and the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeekAndTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2, int p3)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac051.03.a@example.com` paused `Yoga, 3 days a week` from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac051.03.a@example.com` paused `Yoga, 3 days a week` from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
And the registered user `f8.ac051.03.b@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.b@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_BExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-16T09:00:00Z `f8.ac051.03.a@example.com` cancels the pause 2026-11-16 – 2026-11-20 through their goal `Drinking water, 8 glasses a day`
-> skipped because of previous errors
Then the result is "This pause does not exist."
-> skipped because of previous errors
And the pauses of `Yoga, 3 days a week` of `f8.ac051.03.a@example.com` are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.APauseIsFoundOnlyThroughItsOwnGoal(String n, String actor, String pause, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 543
[non-AC frame(s) hidden]
```

### F008/AC-023 Pausing a goal from today — pending

Step trace:
```
Given the registered user `f8.ac023.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-16T09:00:00Z `f8.ac023.user@example.com` pauses the goal from 2026-11-16 until 2026-11-22 with the reason "Family visit"
-> skipped because of previous errors
Then the result is "paused" with the first day 2026-11-16 and the last day 2026-11-22
-> skipped because of previous errors
And the goals overview gives `Drinking water, 8 glasses a day` paused until 2026-11-22
-> skipped because of previous errors
And the history of the goal from 2026-11-15 to 2026-11-16 gives 2026-11-15 Pending and 2026-11-16 Paused, no grace end
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Pending and {DateTime} Paused, no grace end")]
public void ThenTheHistoryOfTheGoalFromToGivesPendingAndPausedNoGraceEnd(DateTime p0, DateTime p1, DateTime p2, DateTime p3)
{
    throw new PendingStepException();
}

And what can be recorded now contains 2026-11-15 Pending and no entry for 2026-11-16
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.PausingAGoalFromToday() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 255
[non-AC frame(s) hidden]
```

### F008/AC-056 The trend marks Excused and Paused bars, which have no value — pending

Step trace:
```
Given the registered user `f8.ac056.user@example.com`, who created the goal `Meditation, 10 min daily`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Meditation, {int} min daily`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalMeditationMinDaily(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-10T08:00:00Z `f8.ac056.user@example.com` excused 2026-11-09 without a reason
-> skipped because of previous errors
And at 2026-11-13T08:00:00Z `f8.ac056.user@example.com` paused the goal from 2026-11-13 until 2026-11-15 without a reason
-> skipped because of previous errors
And `f8.ac056.user@example.com` records 10 for each date from 2026-11-02 to 2026-11-16 except 2026-11-09, 2026-11-11, 2026-11-13, 2026-11-14 and 2026-11-15
-> skipped because of previous errors
When at 2026-11-19T12:00:00Z `f8.ac056.user@example.com` reads the report of the goal
-> skipped because of previous errors
Then the trend is, with the target 10: (2026-11-06, 10), (2026-11-07, 10), (2026-11-08, 10), (2026-11-09, Excused), (2026-11-10, 10), (2026-11-11, no value), (2026-11-12, 10), (2026-11-13, Paused), (2026-11-14, Paused), (2026-11-15, Paused), (2026-11-16, 10), (2026-11-17, no value), (2026-11-18, no value), (2026-11-19, no value)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.TheTrendMarksExcusedAndPausedBarsWhichHaveNoValue() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 597
[non-AC frame(s) hidden]
```

### F008/AC-085 An Excused Due commitment is not counted or listed as Due — pending

Step trace:
```
Given the registered user `f8.ac085.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-13T07:00:00Z `f8.ac085.user@example.com` excused 2026-11-13 without a reason
-> skipped because of previous errors
When at 2026-11-13T08:00:00Z `f8.ac085.user@example.com` reads the dashboard
-> skipped because of previous errors
Then the Due count is 0 and the Pending count is 1
-> skipped because of previous errors
And the Today list contains (`Yoga, 3 days a week`, 2026-11-11, Pending) and no entry for 2026-11-13
-> skipped because of previous errors
And the number of goals is 1
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnExcusedDueCommitmentIsNotCountedOrListedAsDue() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 917
[non-AC frame(s) hidden]
```

### F008/AC-006 A final commitment cannot be excused(n: "03", setup: "records No for 2026-11-06", date: "2026-11-06", state: "Failed", __pickleIndex: "9", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac006.03.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And `f8.ac006.03.user@example.com` records No for 2026-11-06
-> skipped because of previous errors
When at 2026-11-12T10:00:00Z `f8.ac006.03.user@example.com` excuses 2026-11-06 without a reason
-> skipped because of previous errors
Then the result is "This commitment is final and can no longer be excused."
-> skipped because of previous errors
And the history of the goal from 2026-11-06 to 2026-11-06 gives 2026-11-06 Failed
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Failed")]
public void ThenTheHistoryOfTheGoalFromToGivesFailed(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AFinalCommitmentCannotBeExcused(String n, String setup, String date, String state, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 57
[non-AC frame(s) hidden]
```

### F008/AC-054 Excused and Paused results neither extend nor reset a streak — pending

Step trace:
```
Given the registered user `f8.ac054.user@example.com`, who created the goal `Meditation, 10 min daily`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Meditation, {int} min daily`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalMeditationMinDaily(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-10T08:00:00Z `f8.ac054.user@example.com` excused 2026-11-09 without a reason
-> skipped because of previous errors
And at 2026-11-13T08:00:00Z `f8.ac054.user@example.com` paused the goal from 2026-11-13 until 2026-11-15 without a reason
-> skipped because of previous errors
And `f8.ac054.user@example.com` records 10 for each date from 2026-11-02 to 2026-11-16 except 2026-11-09, 2026-11-11, 2026-11-13, 2026-11-14 and 2026-11-15
-> skipped because of previous errors
When at 2026-11-19T12:00:00Z `f8.ac054.user@example.com` reads the report of the goal
-> skipped because of previous errors
Then the current streak is "2 commitments"
-> skipped because of previous errors
And the longest streak is "8 commitments"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ExcusedAndPausedResultsNeitherExtendNorResetAStreak() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 578
[non-AC frame(s) hidden]
```

### F008/AC-073 An administrator cannot use any use case of this slice(n: "01", action: "excuses 2026-11-18 without a reason", __pickleIndex: "114", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac073.01.a@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac073.01.a@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac073.01.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac073.01.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
And the administrator `f8.ac073.01.admin@example.com`
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac073.01.admin@example.com` excuses 2026-11-18 without a reason on the goal of `f8.ac073.01.a@example.com`
-> skipped because of previous errors
Then the result is "Goals are for registered users."
-> skipped because of previous errors
And the pauses of the goal, read by `f8.ac073.01.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac073.01.a@example.com`, gives 2026-11-11 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime}, read by `f{int}\.ac{float}\.a@example\.com`, gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToReadByF_Ac_AExample_ComGivesExcusedReason(DateTime p0, DateTime p1, int p2, Decimal p3, DateTime p4, string ill5)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnAdministratorCannotUseAnyUseCaseOfThisSlice(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 774
[non-AC frame(s) hidden]
```

### F008/AC-028 The pause checks come in their order(n: "01", days: "from 2026-11-15 with no last day", reason: "\"Away\"", result: "\"Choose the first and the last day.\"", __pickleIndex: "50", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac028.01.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac028.01.user@example.com` paused the goal from 2026-11-23 until 2026-11-25 without a reason
-> skipped because of previous errors
When at 2026-11-16T09:00:00Z `f8.ac028.01.user@example.com` pauses the goal from 2026-11-15 with no last day with the reason "Away"
-> skipped because of previous errors
Then the result is "Choose the first and the last day."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ThePauseChecksComeInTheirOrder(String n, String days, String reason, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 303
[non-AC frame(s) hidden]
```

### F008/AC-069 The Upcoming entry skips a Paused commitment — pending

Step trace:
```
Given the registered user `f8.ac069.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac069.user@example.com` paused the goal from 2026-11-18 until 2026-11-18 without a reason
-> skipped because of previous errors
When at 2026-11-17T10:00:00Z `f8.ac069.user@example.com` reads the dashboard
-> skipped because of previous errors
Then the Today list contains (`Yoga, 3 days a week`, 2026-11-16, Pending) and (`Yoga, 3 days a week`, 2026-11-20, Upcoming)
-> skipped because of previous errors
And the Today list contains no entry for 2026-11-18
-> skipped because of previous errors
And the number of goals is 1
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.TheUpcomingEntrySkipsAPausedCommitment() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 722
[non-AC frame(s) hidden]
```

### F008/AC-087 A goal that has not started can be resumed from its current pause — pending

Step trace:
```
Given the registered user `f8.ac087.user@example.com`, who created the goal `Drinking water, 8 glasses a day` at 2026-11-16T08:00:00Z with the start date 2026-11-20
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day` at {DateTime}T{int}:{string}Z with the start date {DateTime}")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADayAtTZWithTheStartDate(int p0, int p1, int p2, DateTime p3, int p4, string p5, DateTime p6)
{
    throw new PendingStepException();
}

And at 2026-11-16T09:00:00Z `f8.ac087.user@example.com` paused the goal from 2026-11-16 until 2026-11-18 without a reason
-> skipped because of previous errors
When at 2026-11-17T10:00:00Z `f8.ac087.user@example.com` resumes the goal
-> skipped because of previous errors
Then the result is "resumed"
-> skipped because of previous errors
And the pauses of the goal are (2026-11-16, 2026-11-16, no reason, past)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AGoalThatHasNotStartedCanBeResumedFromItsCurrentPause() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 937
[non-AC frame(s) hidden]
```

### F008/AC-007 Excusing just before, at and just after the end of the grace period(n: "02", time: "2026-11-14T00:00:00Z", result: "\"This commitment is final and can no longer be ex"···, state: "Missed", __pickleIndex: "11", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac007.02.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-14T00:00:00Z `f8.ac007.02.user@example.com` excuses 2026-11-11 without a reason
-> skipped because of previous errors
Then the result is "This commitment is final and can no longer be excused."
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Missed
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Missed")]
public void ThenTheHistoryOfTheGoalFromToGivesMissed(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ExcusingJustBeforeAtAndJustAfterTheEndOfTheGracePeriod(String n, String time, String result, String state, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 70
[non-AC frame(s) hidden]
```

### F008/AC-066 The goals overview says which goal is paused and until when — pending

Step trace:
```
Given the registered user `f8.ac066.user@example.com`, who created the goal `Drinking water, 8 glasses a day` and the goal `Meditation, 10 min daily`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day` and the goal `Meditation, {int} min daily`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADayAndTheGoalMeditationMinDaily(int p0, int p1, int p2, int p3)
{
    throw new PendingStepException();
}

And at 2026-11-16T09:00:00Z `f8.ac066.user@example.com` paused `Drinking water, 8 glasses a day` from 2026-11-16 until 2026-11-22 without a reason
-> skipped because of previous errors
And `f8.ac066.user@example.com` records on target for each date from 2026-11-02 to 2026-11-13: 8 for `Drinking water, 8 glasses a day` and 10 for `Meditation, 10 min daily`
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac066.user@example.com` reads the goals overview
-> skipped because of previous errors
Then it gives `Drinking water, 8 glasses a day` paused until 2026-11-22
-> skipped because of previous errors
And it gives `Meditation, 10 min daily` not paused
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.TheGoalsOverviewSaysWhichGoalIsPausedAndUntilWhen() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 691
[non-AC frame(s) hidden]
```

### F008/AC-052 The pauses of a goal are listed with the latest first day first — pending

Step trace:
```
Given the registered user `f8.ac052.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T09:00:00Z `f8.ac052.user@example.com` paused the goal from 2026-11-16 until 2026-11-17 with the reason "Ill"
-> skipped because of previous errors
And at 2026-11-16T09:01:00Z `f8.ac052.user@example.com` paused the goal from 2026-11-19 until 2026-11-22 with the reason "Family visit"
-> skipped because of previous errors
And at 2026-11-16T09:02:00Z `f8.ac052.user@example.com` paused the goal from 2026-11-25 until 2026-11-26 without a reason
-> skipped because of previous errors
When at 2026-11-20T10:00:00Z `f8.ac052.user@example.com` reads the pauses of the goal
-> skipped because of previous errors
Then they are, in this order, (2026-11-25, 2026-11-26, no reason, upcoming), (2026-11-19, 2026-11-22, reason "Family visit", current) and (2026-11-16, 2026-11-17, reason "Ill", past)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ThePausesOfAGoalAreListedWithTheLatestFirstDayFirst() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 559
[non-AC frame(s) hidden]
```

### F008/AC-016 A reason has at most 200 characters(n: "01", reason: "`x` repeated 200 times", result: "\"excused\"", entry: "Excused, reason `x` repeated 200 times", __pickleIndex: "28", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac016.01.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-12T10:00:00Z `f8.ac016.01.user@example.com` excuses 2026-11-11 with the reason `x` repeated 200 times
-> skipped because of previous errors
Then the result is "excused"
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Excused, reason `x` repeated 200 times
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Excused, reason `x` repeated {int} times")]
public void ThenTheHistoryOfTheGoalFromToGivesExcusedReasonXRepeatedTimes(DateTime p0, DateTime p1, DateTime p2, int p3)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AReasonHasAtMost200Characters(String n, String reason, String result, String entry, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 175
[non-AC frame(s) hidden]
```

### F008/AC-033 A per-week period is Paused as a whole while one of its dates is paused — pending

Step trace:
```
Given the registered user `f8.ac033.user@example.com`, who created the goal `Yoga, 3 times a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} times a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaTimesAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-18T08:00:00Z `f8.ac033.user@example.com` pauses the goal from 2026-11-19 until 2026-11-25 without a reason
-> skipped because of previous errors
Then the result is "paused" with the first day 2026-11-19 and the last day 2026-11-25
-> skipped because of previous errors
And the history of the goal from 2026-11-16 to 2026-11-29 gives 2026-11-16 – 2026-11-22 Paused and 2026-11-23 – 2026-11-29 Paused
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} – {DateTime} Paused and {DateTime} – {DateTime} Paused")]
public void ThenTheHistoryOfTheGoalFromToGivesPausedAndPaused(DateTime p0, DateTime p1, DateTime p2, DateTime p3, DateTime p4, DateTime p5)
{
    throw new PendingStepException();
}

And the pauses of the goal are (2026-11-19, 2026-11-25, no reason, upcoming)
-> skipped because of previous errors
And the goals overview gives `Yoga, 3 times a week` not paused
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.APer_WeekPeriodIsPausedAsAWholeWhileOneOfItsDatesIsPaused() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 359
[non-AC frame(s) hidden]
```

### F008/AC-073 An administrator cannot use any use case of this slice(n: "06", action: "reads the pauses of the goal", __pickleIndex: "119", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac073.06.a@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac073.06.a@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac073.06.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac073.06.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
And the administrator `f8.ac073.06.admin@example.com`
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac073.06.admin@example.com` reads the pauses of the goal on the goal of `f8.ac073.06.a@example.com`
-> skipped because of previous errors
Then the result is "Goals are for registered users."
-> skipped because of previous errors
And the pauses of the goal, read by `f8.ac073.06.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac073.06.a@example.com`, gives 2026-11-11 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime}, read by `f{int}\.ac{float}\.a@example\.com`, gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToReadByF_Ac_AExample_ComGivesExcusedReason(DateTime p0, DateTime p1, int p2, Decimal p3, DateTime p4, string ill5)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnAdministratorCannotUseAnyUseCaseOfThisSlice(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 774
[non-AC frame(s) hidden]
```

### F008/AC-046 A pause later in the current week pauses that week, while the goal is not paused and is counted — pending

Step trace:
```
Given the registered user `f8.ac046.user@example.com`, who created the goal `Yoga, 3 times a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} times a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaTimesAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac046.user@example.com` paused the goal from 2026-11-19 until 2026-11-20 without a reason
-> skipped because of previous errors
When at 2026-11-16T09:00:00Z `f8.ac046.user@example.com` reads the dashboard
-> skipped because of previous errors
Then the number of goals is 1
-> skipped because of previous errors
And the Today list contains (`Yoga, 3 times a week`, 2026-11-09 – 2026-11-15, Pending) and no entry for 2026-11-16 – 2026-11-22
-> skipped because of previous errors
And the summary of `Yoga, 3 times a week` says it is not paused
-> skipped because of previous errors
And the goals overview gives `Yoga, 3 times a week` not paused
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.APauseLaterInTheCurrentWeekPausesThatWeekWhileTheGoalIsNotPausedAndIsCounted() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 491
[non-AC frame(s) hidden]
```

### F008/AC-062 A yes/no week's target and value leave a Paused commitment out — pending

Step trace:
```
Given the registered user `f8.ac062.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac062.user@example.com` recorded Yes for 2026-11-16
-> skipped because of previous errors
And at 2026-11-16T09:00:00Z `f8.ac062.user@example.com` paused the goal from 2026-11-16 until 2026-11-16 without a reason
-> skipped because of previous errors
And `f8.ac062.user@example.com` records Yes for 2026-11-09, 2026-11-11, 2026-11-13, 2026-11-18 and 2026-11-20
-> skipped because of previous errors
When at 2026-11-23T10:00:00Z `f8.ac062.user@example.com` reads the report of the goal
-> skipped because of previous errors
Then the trend is (2026-11-02 – 2026-11-08, no value, target 3), (2026-11-09 – 2026-11-15, 3, target 3), (2026-11-16 – 2026-11-22, 2, target 2), (2026-11-23 – 2026-11-29, no value, target 3)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AYesNoWeeksTargetAndValueLeaveAPausedCommitmentOut() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 657
[non-AC frame(s) hidden]
```

### F008/AC-061 The trend of a per-day goal marks its Paused days — pending

Step trace:
```
Given the registered user `f8.ac061.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T09:00:00Z `f8.ac061.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 without a reason
-> skipped because of previous errors
And `f8.ac061.user@example.com` records 8 for each date from 2026-11-02 to 2026-11-15
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheUserRecords().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("`f{int}\.ac{int}\.user@example\.com` records {int} for each date from {DateTime} to {DateTime}")]
public void GivenF_Ac_UserExample_ComRecordsForEachDateFromTo(int p0, int p1, int p2, DateTime p3, DateTime p4)
{
    throw new PendingStepException();
}

When at 2026-11-19T10:00:00Z `f8.ac061.user@example.com` reads the report of the goal
-> skipped because of previous errors
Then the trend is, with the target 8: (2026-11-06, 8), (2026-11-07, 8), (2026-11-08, 8), (2026-11-09, 8), (2026-11-10, 8), (2026-11-11, 8), (2026-11-12, 8), (2026-11-13, 8), (2026-11-14, 8), (2026-11-15, 8), (2026-11-16, Paused), (2026-11-17, Paused), (2026-11-18, Paused), (2026-11-19, Paused)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.TheTrendOfAPer_DayGoalMarksItsPausedDays() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 648
[non-AC frame(s) hidden]
```

### F008/AC-004 An excused commitment stays Excused after its grace period would have ended(n: "02", time: "2026-12-01T00:00:00Z", __pickleIndex: "5", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac004.02.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac004.02.user@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
When at 2026-12-01T00:00:00Z `f8.ac004.02.user@example.com` reads the history of the goal from 2026-11-11 to 2026-11-11
-> skipped because of previous errors
Then it gives 2026-11-11 Excused, no record, no grace end, correction no, reason "Ill", not removable
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnExcusedCommitmentStaysExcusedAfterItsGracePeriodWouldHaveEnded(String n, String time, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 37
[non-AC frame(s) hidden]
```

### F008/AC-003 The reason is trimmed, and a reason that is empty after trimming is none(n: "01", given: "\"  Ill  \"", stored: "reason \"Ill\"", __pickleIndex: "2", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac003.01.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-12T10:00:00Z `f8.ac003.01.user@example.com` excuses 2026-11-11 with the reason "  Ill  "
-> skipped because of previous errors
Then the result is "excused" with the date 2026-11-11
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToGivesExcusedReason(DateTime p0, DateTime p1, DateTime p2, string ill3)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.TheReasonIsTrimmedAndAReasonThatIsEmptyAfterTrimmingIsNone(String n, String given, String stored, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 25
[non-AC frame(s) hidden]
```

### F008/AC-004 An excused commitment stays Excused after its grace period would have ended(n: "01", time: "2026-11-14T00:00:00Z", __pickleIndex: "4", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac004.01.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac004.01.user@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
When at 2026-11-14T00:00:00Z `f8.ac004.01.user@example.com` reads the history of the goal from 2026-11-11 to 2026-11-11
-> skipped because of previous errors
Then it gives 2026-11-11 Excused, no record, no grace end, correction no, reason "Ill", not removable
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnExcusedCommitmentStaysExcusedAfterItsGracePeriodWouldHaveEnded(String n, String time, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 37
[non-AC frame(s) hidden]
```

### F008/AC-051 A pause is found only through its own goal(n: "02", actor: "f8.ac051.02.b@example.com", pause: "2026-11-23 – 2026-11-27", __pickleIndex: "85", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac051.02.a@example.com`, who created the goal `Yoga, 3 days a week` and the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week` and the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeekAndTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2, int p3)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac051.02.a@example.com` paused `Yoga, 3 days a week` from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac051.02.a@example.com` paused `Yoga, 3 days a week` from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
And the registered user `f8.ac051.02.b@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.b@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_BExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-16T09:00:00Z `f8.ac051.02.b@example.com` cancels the pause 2026-11-23 – 2026-11-27 through their goal `Drinking water, 8 glasses a day`
-> skipped because of previous errors
Then the result is "This pause does not exist."
-> skipped because of previous errors
And the pauses of `Yoga, 3 days a week` of `f8.ac051.02.a@example.com` are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.APauseIsFoundOnlyThroughItsOwnGoal(String n, String actor, String pause, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 543
[non-AC frame(s) hidden]
```

### F008/AC-032 Resuming a commitment-based goal makes today's commitment Due again — pending

Step trace:
```
Given the registered user `f8.ac032.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-18T08:00:00Z `f8.ac032.user@example.com` paused the goal from 2026-11-18 until 2026-11-20 without a reason
-> skipped because of previous errors
When at 2026-11-20T08:00:00Z `f8.ac032.user@example.com` resumes the goal
-> skipped because of previous errors
Then the result is "resumed"
-> skipped because of previous errors
And the pauses of the goal are (2026-11-18, 2026-11-19, no reason, past)
-> skipped because of previous errors
And the history of the goal from 2026-11-18 to 2026-11-20 gives 2026-11-18 Paused and 2026-11-20 Due
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Paused and {DateTime} Due")]
public void ThenTheHistoryOfTheGoalFromToGivesPausedAndDue(DateTime p0, DateTime p1, DateTime p2, DateTime p3)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ResumingACommitment_BasedGoalMakesTodaysCommitmentDueAgain() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 350
[non-AC frame(s) hidden]
```

### F008/AC-026 A pause cannot share a date with another pause of the goal, but may touch it(n: "04", first: "2026-11-23", last: "2026-11-29", result: "\"paused\" with the first day 2026-11-23 and the l"···, __pickleIndex: "46", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac026.04.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T09:00:00Z `f8.ac026.04.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 with the reason "Family visit"
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac026.04.user@example.com` pauses the goal from 2026-11-23 until 2026-11-29 without a reason
-> skipped because of previous errors
Then the result is "paused" with the first day 2026-11-23 and the last day 2026-11-29
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.APauseCannotShareADateWithAnotherPauseOfTheGoalButMayTouchIt(String n, String first, String last, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 276
[non-AC frame(s) hidden]
```

### F008/AC-079 Demo pause loading leaves an existing `demo6@example.com` unchanged — failed

Step trace:
```
Given the demo run at 2026-11-18T10:00:00Z
-> done: GivenSteps.GivenTheDemoRun("2026-11-18T10:00:00Z") (0,5s)
When demo pause loading runs at 2026-11-19T10:00:00Z with the demo password `example2026`
-> done: DemoSteps.WhenDemoPauseLoadingRuns("2026-11-19T10:00:00Z", "with the demo pas...") (0,0s)
Then its results are (demo6@example.com, already exists)
-> done: DemoSteps.ThenItsResultsAre("(demo6@example.co...") (0,0s)
And at 2026-11-19T10:00:00Z the pauses of `Drinking water, 8 glasses a day` of `demo6@example.com` are, in this order, (2026-11-17, 2026-11-23, reason "Family visit.", current) and (2026-10-31, 2026-11-06, reason "Ill with flu.", past)
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F008.ResultThenSteps.ThenThePausesAre(String, String, String, String, String)' should have 4 parameters
And at 2026-11-19T10:00:00Z the goals overview of `demo6@example.com` lists exactly `Meditation, 10 min daily` and `Drinking water, 8 glasses a day`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F008.ResultThenSteps.ThenThePausesAre(String, String, String, String, String)' should have 4 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.DemoPauseLoadingLeavesAnExistingDemo6Example_ComUnchanged() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 859
[non-AC frame(s) hidden]
```

### F008/AC-017 An excusal can be removed while the grace period is open — pending

Step trace:
```
Given the registered user `f8.ac017.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac017.user@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
When at 2026-11-13T23:59:00Z `f8.ac017.user@example.com` removes the excusal of 2026-11-11
-> skipped because of previous errors
Then the result is "excusal removed" with the state Pending
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Pending, no record, grace ends 2026-11-14T00:00:00Z
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Pending, no record, grace ends {DateTime}T{int}:{string}Z")]
public void ThenTheHistoryOfTheGoalFromToGivesPendingNoRecordGraceEndsTZ(DateTime p0, DateTime p1, DateTime p2, DateTime p3, int p4, string p5)
{
    throw new PendingStepException();
}

And what can be recorded now contains 2026-11-11 Pending
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnExcusalCanBeRemovedWhileTheGracePeriodIsOpen() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 189
[non-AC frame(s) hidden]
```

### F008/AC-027 A pause needs a first and a last day(n: "01", days: "with no first day until 2026-11-22", __pickleIndex: "47", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac027.01.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-16T09:00:00Z `f8.ac027.01.user@example.com` pauses the goal with no first day until 2026-11-22 without a reason
-> skipped because of previous errors
Then the result is "Choose the first and the last day."
-> skipped because of previous errors
And the pauses of the goal are empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.APauseNeedsAFirstAndALastDay(String n, String days, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 290
[non-AC frame(s) hidden]
```

### F008/AC-073 An administrator cannot use any use case of this slice(n: "03", action: "pauses the goal from 2026-11-28 until 2026-11-29 w"···, __pickleIndex: "116", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac073.03.a@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac073.03.a@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac073.03.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac073.03.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
And the administrator `f8.ac073.03.admin@example.com`
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac073.03.admin@example.com` pauses the goal from 2026-11-28 until 2026-11-29 without a reason on the goal of `f8.ac073.03.a@example.com`
-> skipped because of previous errors
Then the result is "Goals are for registered users."
-> skipped because of previous errors
And the pauses of the goal, read by `f8.ac073.03.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac073.03.a@example.com`, gives 2026-11-11 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime}, read by `f{int}\.ac{float}\.a@example\.com`, gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToReadByF_Ac_AExample_ComGivesExcusedReason(DateTime p0, DateTime p1, int p2, Decimal p3, DateTime p4, string ill5)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnAdministratorCannotUseAnyUseCaseOfThisSlice(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 774
[non-AC frame(s) hidden]
```

### F008/AC-029 For a goal that has not started, "today" is the account's today(n: "02", first: "2026-11-15", result: "\"A pause cannot start before today.\"", __pickleIndex: "57", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac029.02.user@example.com`, who created the goal `Drinking water, 8 glasses a day` at 2026-11-16T08:00:00Z with the start date 2026-11-20
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day` at {DateTime}T{int}:{string}Z with the start date {DateTime}")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADayAtTZWithTheStartDate(int p0, Decimal p1, int p2, DateTime p3, int p4, string p5, DateTime p6)
{
    throw new PendingStepException();
}

When at 2026-11-16T09:00:00Z `f8.ac029.02.user@example.com` pauses the goal from 2026-11-15 until 2026-11-18 without a reason
-> skipped because of previous errors
Then the result is "A pause cannot start before today."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ForAGoalThatHasNotStartedTodayIsTheAccountsToday(String n, String first, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 318
[non-AC frame(s) hidden]
```

### F008/AC-072 Another registered user cannot use any use case of this slice on someone else's goal(n: "06", action: "reads the pauses of the goal", __pickleIndex: "113", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac072.06.a@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac072.06.a@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac072.06.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac072.06.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
And the registered user `f8.ac072.06.b@example.com`
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac072.06.b@example.com` reads the pauses of the goal on the goal of `f8.ac072.06.a@example.com`
-> skipped because of previous errors
Then the result is "This goal does not exist."
-> skipped because of previous errors
And the pauses of the goal, read by `f8.ac072.06.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac072.06.a@example.com`, gives 2026-11-11 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime}, read by `f{int}\.ac{float}\.a@example\.com`, gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToReadByF_Ac_AExample_ComGivesExcusedReason(DateTime p0, DateTime p1, int p2, Decimal p3, DateTime p4, string ill5)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnotherRegisteredUserCannotUseAnyUseCaseOfThisSliceOnSomeoneElsesGoal(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 753
[non-AC frame(s) hidden]
```

### F008/AC-038 A pause never changes a result that was Pending or final when it was set — pending

Step trace:
```
Given the registered user `f8.ac038.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-13T20:00:00Z `f8.ac038.user@example.com` recorded 6 for 2026-11-13
-> skipped because of previous errors
And at 2026-11-14T20:00:00Z `f8.ac038.user@example.com` recorded 8 for 2026-11-14
-> skipped because of previous errors
When at 2026-11-16T09:00:00Z `f8.ac038.user@example.com` pauses the goal from 2026-11-16 until 2026-11-22 without a reason
-> skipped because of previous errors
Then the result is "paused" with the first day 2026-11-16 and the last day 2026-11-22
-> skipped because of previous errors
And the history of the goal from 2026-11-13 to 2026-11-16 gives 2026-11-13 Failed, 2026-11-14 Pending, 2026-11-15 Pending and 2026-11-16 Paused
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Failed, {DateTime} Pending, {DateTime} Pending and {DateTime} Paused")]
public void ThenTheHistoryOfTheGoalFromToGivesFailedPendingPendingAndPaused(DateTime p0, DateTime p1, DateTime p2, DateTime p3, DateTime p4, DateTime p5)
{
    throw new PendingStepException();
}

And at 2026-11-18T12:00:00Z the history of the goal from 2026-11-13 to 2026-11-15 gives 2026-11-13 Failed, 2026-11-14 Met and 2026-11-15 Missed
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.APauseNeverChangesAResultThatWasPendingOrFinalWhenItWasSet() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 405
[non-AC frame(s) hidden]
```

### F008/AC-018 Removing an excusal just before, at and just after the end of the grace period(n: "02", time: "2026-11-14T00:00:00Z", result: "\"This excusal can no longer be removed.\"", state: "Excused", __pickleIndex: "32", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac018.02.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac018.02.user@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
When at 2026-11-14T00:00:00Z `f8.ac018.02.user@example.com` removes the excusal of 2026-11-11
-> skipped because of previous errors
Then the result is "This excusal can no longer be removed."
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Excused
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Excused")]
public void ThenTheHistoryOfTheGoalFromToGivesExcused(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.RemovingAnExcusalJustBeforeAtAndJustAfterTheEndOfTheGracePeriod(String n, String time, String result, String state, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 197
[non-AC frame(s) hidden]
```

### F008/AC-036 One paused day exempts its whole week, also when its records reach the target — pending

Step trace:
```
Given the registered user `f8.ac036.user@example.com`, who created the goal `Yoga, 3 times a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} times a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaTimesAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-18T09:00:00Z `f8.ac036.user@example.com` recorded Yes for each date from 2026-11-16 to 2026-11-18
-> skipped because of previous errors
And at 2026-11-18T10:00:00Z `f8.ac036.user@example.com` paused the goal from 2026-11-22 until 2026-11-22 without a reason
-> skipped because of previous errors
When at 2026-11-25T12:00:00Z `f8.ac036.user@example.com` reads the history of the goal from 2026-11-16 to 2026-11-22
-> skipped because of previous errors
Then it gives 2026-11-16 – 2026-11-22 Paused, records Yes, Yes, Yes
-> skipped because of previous errors
And the report of the goal gives the consistency "0% (0 of 2)"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.OnePausedDayExemptsItsWholeWeekAlsoWhenItsRecordsReachTheTarget() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 385
[non-AC frame(s) hidden]
```

### F008/AC-022 The record checks for a pause and an excusal come after the date check, the pause first(n: "01", date: "2026-11-16", result: "\"This goal is paused on this date.\"", __pickleIndex: "38", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac022.01.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T07:00:00Z `f8.ac022.01.user@example.com` excused 2026-11-16 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac022.01.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 without a reason
-> skipped because of previous errors
When at 2026-11-17T10:00:00Z `f8.ac022.01.user@example.com` records Yes for 2026-11-16
-> skipped because of previous errors
Then the result is "This goal is paused on this date."
-> skipped because of previous errors
And the history of the goal from 2026-11-16 to 2026-11-18 gives 2026-11-16 Paused, no record and 2026-11-18 Paused, no record, and no entry for 2026-11-17
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Paused, no record and {DateTime} Paused, no record, and no entry for {DateTime}")]
public void ThenTheHistoryOfTheGoalFromToGivesPausedNoRecordAndPausedNoRecordAndNoEntryFor(DateTime p0, DateTime p1, DateTime p2, DateTime p3, DateTime p4)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.TheRecordChecksForAPauseAndAnExcusalComeAfterTheDateCheckThePauseFirst(String n, String date, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 241
[non-AC frame(s) hidden]
```

### F008/AC-044 A goal without a current pause cannot be resumed(n: "02", first: "2026-11-16", last: "2026-11-16", time: "2026-11-17T10:00:00Z", kind: "past", __pickleIndex: "75", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac044.02.user@example.com`, who created the goal `Yoga, 3 times a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} times a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaTimesAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac044.02.user@example.com` paused the goal from 2026-11-16 until 2026-11-16 without a reason
-> skipped because of previous errors
When at 2026-11-17T10:00:00Z `f8.ac044.02.user@example.com` resumes the goal
-> skipped because of previous errors
Then the result is "This goal is not paused."
-> skipped because of previous errors
And the pauses of the goal are (2026-11-16, 2026-11-16, no reason, past)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AGoalWithoutACurrentPauseCannotBeResumed(String n, String first, String last, String time, String kind, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 466
[non-AC frame(s) hidden]
```

### F008/AC-026 A pause cannot share a date with another pause of the goal, but may touch it(n: "01", first: "2026-11-21", last: "2026-11-24", result: "\"This goal already has a pause on these dates.\"", __pickleIndex: "43", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac026.01.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T09:00:00Z `f8.ac026.01.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 with the reason "Family visit"
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac026.01.user@example.com` pauses the goal from 2026-11-21 until 2026-11-24 without a reason
-> skipped because of previous errors
Then the result is "This goal already has a pause on these dates."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.APauseCannotShareADateWithAnotherPauseOfTheGoalButMayTouchIt(String n, String first, String last, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 276
[non-AC frame(s) hidden]
```

### F008/AC-042 Resuming during a pause ends it on the day before today — pending

Step trace:
```
Given the registered user `f8.ac042.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T09:00:00Z `f8.ac042.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 with the reason "Family visit"
-> skipped because of previous errors
When at 2026-11-18T08:00:00Z `f8.ac042.user@example.com` resumes the goal
-> skipped because of previous errors
Then the result is "resumed"
-> skipped because of previous errors
And the pauses of the goal are (2026-11-16, 2026-11-17, reason "Family visit", past)
-> skipped because of previous errors
And the history of the goal from 2026-11-16 to 2026-11-18 gives 2026-11-16 Paused, 2026-11-17 Paused and 2026-11-18 Due
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Paused, {DateTime} Paused and {DateTime} Due")]
public void ThenTheHistoryOfTheGoalFromToGivesPausedPausedAndDue(DateTime p0, DateTime p1, DateTime p2, DateTime p3, DateTime p4)
{
    throw new PendingStepException();
}

And what can be recorded now contains 2026-11-18 Due
-> skipped because of previous errors
And the goals overview gives `Drinking water, 8 glasses a day` not paused
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ResumingDuringAPauseEndsItOnTheDayBeforeToday() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 447
[non-AC frame(s) hidden]
```

### F008/AC-041 A record kept on a Paused date can be changed or deleted, and the date stays Paused(n: "02", action: "deletes the record of 2026-11-16", records: "no record", __pickleIndex: "71", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac041.02.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac041.02.user@example.com` recorded 8 for 2026-11-16
-> skipped because of previous errors
And at 2026-11-16T09:00:00Z `f8.ac041.02.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 without a reason
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac041.02.user@example.com` deletes the record of 2026-11-16
-> skipped because of previous errors
Then the result is the state Paused, correction no
-> skipped because of previous errors
And the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Paused, no record, correction no
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Paused, no record, correction no")]
public void ThenTheHistoryOfTheGoalFromToGivesPausedNoRecordCorrectionNo(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ARecordKeptOnAPausedDateCanBeChangedOrDeletedAndTheDateStaysPaused(String n, String action, String records, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 431
[non-AC frame(s) hidden]
```

### F008/AC-013 A commitment that is already excused cannot be excused again(n: "02", time: "2026-11-14T10:00:00Z", __pickleIndex: "24", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac013.02.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac013.02.user@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
When at 2026-11-14T10:00:00Z `f8.ac013.02.user@example.com` excuses 2026-11-11 with the reason "Away"
-> skipped because of previous errors
Then the result is "This commitment is already excused."
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToGivesExcusedReason(DateTime p0, DateTime p1, DateTime p2, string ill3)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ACommitmentThatIsAlreadyExcusedCannotBeExcusedAgain(String n, String time, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 141
[non-AC frame(s) hidden]
```

### F008/AC-026 A pause cannot share a date with another pause of the goal, but may touch it(n: "03", first: "2026-11-17", last: "2026-11-18", result: "\"This goal already has a pause on these dates.\"", __pickleIndex: "45", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac026.03.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T09:00:00Z `f8.ac026.03.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 with the reason "Family visit"
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac026.03.user@example.com` pauses the goal from 2026-11-17 until 2026-11-18 without a reason
-> skipped because of previous errors
Then the result is "This goal already has a pause on these dates."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.APauseCannotShareADateWithAnotherPauseOfTheGoalButMayTouchIt(String n, String first, String last, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 276
[non-AC frame(s) hidden]
```

### F008/AC-005 An excusal applies at every supplied time, also one earlier than the excusing call — pending

Step trace:
```
Given the registered user `f8.ac005.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac005.user@example.com` excused 2026-11-11 without a reason
-> skipped because of previous errors
When at 2026-11-12T09:00:00Z `f8.ac005.user@example.com` reads the history of the goal from 2026-11-11 to 2026-11-11
-> skipped because of previous errors
Then it gives 2026-11-11 Excused, no reason
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnExcusalAppliesAtEverySuppliedTimeAlsoOneEarlierThanTheExcusingCall() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 49
[non-AC frame(s) hidden]
```

### F008/AC-003 The reason is trimmed, and a reason that is empty after trimming is none(n: "02", given: "\"   \"", stored: "no reason", __pickleIndex: "3", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac003.02.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-12T10:00:00Z `f8.ac003.02.user@example.com` excuses 2026-11-11 with the reason "   "
-> skipped because of previous errors
Then the result is "excused" with the date 2026-11-11
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Excused, no reason
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Excused, no reason")]
public void ThenTheHistoryOfTheGoalFromToGivesExcusedNoReason(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.TheReasonIsTrimmedAndAReasonThatIsEmptyAfterTrimmingIsNone(String n, String given, String stored, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 25
[non-AC frame(s) hidden]
```

### F008/AC-008 An Upcoming commitment cannot be excused, also with a reason that is too long(n: "02", time: "2026-11-13T00:00:00Z", reason: "\"Away\"", result: "\"excused\"", __pickleIndex: "14", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac008.02.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-13T00:00:00Z `f8.ac008.02.user@example.com` excuses 2026-11-13 with the reason "Away"
-> skipped because of previous errors
Then the result is "excused"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnUpcomingCommitmentCannotBeExcusedAlsoWithAReasonThatIsTooLong(String n, String time, String reason, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 82
[non-AC frame(s) hidden]
```

### F008/AC-027 A pause needs a first and a last day(n: "02", days: "from 2026-11-16 with no last day", __pickleIndex: "48", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac027.02.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-16T09:00:00Z `f8.ac027.02.user@example.com` pauses the goal from 2026-11-16 with no last day without a reason
-> skipped because of previous errors
Then the result is "Choose the first and the last day."
-> skipped because of previous errors
And the pauses of the goal are empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.APauseNeedsAFirstAndALastDay(String n, String days, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 290
[non-AC frame(s) hidden]
```

### F008/AC-047 Cancelling the upcoming pause that paused the current week makes the week Due again — pending

Step trace:
```
Given the registered user `f8.ac047.user@example.com`, who created the goal `Yoga, 3 times a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} times a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaTimesAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac047.user@example.com` paused the goal from 2026-11-19 until 2026-11-20 without a reason
-> skipped because of previous errors
When at 2026-11-16T09:00:00Z `f8.ac047.user@example.com` cancels the pause 2026-11-19 – 2026-11-20
-> skipped because of previous errors
Then the result is "pause cancelled"
-> skipped because of previous errors
And the pauses of the goal are empty
-> skipped because of previous errors
And the history of the goal from 2026-11-16 to 2026-11-22 gives 2026-11-16 – 2026-11-22 Due
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} – {DateTime} Due")]
public void ThenTheHistoryOfTheGoalFromToGivesDue(DateTime p0, DateTime p1, DateTime p2, DateTime p3)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.CancellingTheUpcomingPauseThatPausedTheCurrentWeekMakesTheWeekDueAgain() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 500
[non-AC frame(s) hidden]
```

### F008/AC-058 A goal whose every ended result is exempt has "N/A" and streaks of 0 — pending

Step trace:
```
Given the registered user `f8.ac058.user@example.com`, who created the goal `Meditation, 10 min daily`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Meditation, {int} min daily`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalMeditationMinDaily(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-03T08:00:00Z `f8.ac058.user@example.com` excused 2026-11-02 without a reason
-> skipped because of previous errors
And at 2026-11-03T08:00:00Z `f8.ac058.user@example.com` paused the goal from 2026-11-03 until 2026-11-08 without a reason
-> skipped because of previous errors
When at 2026-11-09T12:00:00Z `f8.ac058.user@example.com` reads the report of the goal
-> skipped because of previous errors
Then the consistency is "N/A"
-> skipped because of previous errors
And the current streak is "0 commitments"
-> skipped because of previous errors
And the longest streak is "0 commitments"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AGoalWhoseEveryEndedResultIsExemptHasNAAndStreaksOf0() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 620
[non-AC frame(s) hidden]
```

### F008/AC-072 Another registered user cannot use any use case of this slice on someone else's goal(n: "04", action: "resumes the goal", __pickleIndex: "111", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac072.04.a@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac072.04.a@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac072.04.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac072.04.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
And the registered user `f8.ac072.04.b@example.com`
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac072.04.b@example.com` resumes the goal on the goal of `f8.ac072.04.a@example.com`
-> skipped because of previous errors
Then the result is "This goal does not exist."
-> skipped because of previous errors
And the pauses of the goal, read by `f8.ac072.04.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac072.04.a@example.com`, gives 2026-11-11 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime}, read by `f{int}\.ac{float}\.a@example\.com`, gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToReadByF_Ac_AExample_ComGivesExcusedReason(DateTime p0, DateTime p1, int p2, Decimal p3, DateTime p4, string ill5)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnotherRegisteredUserCannotUseAnyUseCaseOfThisSliceOnSomeoneElsesGoal(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 753
[non-AC frame(s) hidden]
```

### F008/AC-071 Recent notes show Excused and Paused as the state — pending

Step trace:
```
Given the registered user `f8.ac071.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-13T07:00:00Z `f8.ac071.user@example.com` excused 2026-11-13 without a reason
-> skipped because of previous errors
And at 2026-11-13T08:00:00Z `f8.ac071.user@example.com` saved the note "Travel" for 2026-11-13
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac071.user@example.com` paused the goal from 2026-11-16 until 2026-11-16 without a reason
-> skipped because of previous errors
And at 2026-11-16T09:00:00Z `f8.ac071.user@example.com` saved the note "Visit" for 2026-11-16
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac071.user@example.com` reads the recent notes of the goal
-> skipped because of previous errors
Then they are, in this order, (2026-11-16, "Visit", Paused) and (2026-11-13, "Travel", Excused)
-> skipped because of previous errors
And the dashboard's recent notes are, in this order, (`Yoga, 3 days a week`, 2026-11-16, "Visit", Paused) and (`Yoga, 3 days a week`, 2026-11-13, "Travel", Excused)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.RecentNotesShowExcusedAndPausedAsTheState() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 741
[non-AC frame(s) hidden]
```

### F008/AC-007 Excusing just before, at and just after the end of the grace period(n: "03", time: "2026-11-14T00:00:01Z", result: "\"This commitment is final and can no longer be ex"···, state: "Missed", __pickleIndex: "12", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac007.03.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-14T00:00:01Z `f8.ac007.03.user@example.com` excuses 2026-11-11 without a reason
-> skipped because of previous errors
Then the result is "This commitment is final and can no longer be excused."
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Missed
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Missed")]
public void ThenTheHistoryOfTheGoalFromToGivesMissed(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ExcusingJustBeforeAtAndJustAfterTheEndOfTheGracePeriod(String n, String time, String result, String state, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 70
[non-AC frame(s) hidden]
```

### F008/AC-049 A current or past pause cannot be cancelled(n: "01", last: "2026-11-22", kind: "current", __pickleIndex: "80", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac049.01.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T09:00:00Z `f8.ac049.01.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 with the reason "Family visit"
-> skipped because of previous errors
When at 2026-11-18T10:00:00Z `f8.ac049.01.user@example.com` cancels the pause 2026-11-16 – 2026-11-22
-> skipped because of previous errors
Then the result is "Only a pause that has not begun can be cancelled. Resume the goal instead."
-> skipped because of previous errors
And the pauses of the goal are (2026-11-16, 2026-11-22, reason "Family visit", current)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ACurrentOrPastPauseCannotBeCancelled(String n, String last, String kind, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 516
[non-AC frame(s) hidden]
```

### F008/AC-027 A pause needs a first and a last day(n: "03", days: "with no first day and no last day", __pickleIndex: "49", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac027.03.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-16T09:00:00Z `f8.ac027.03.user@example.com` pauses the goal with no first day and no last day without a reason
-> skipped because of previous errors
Then the result is "Choose the first and the last day."
-> skipped because of previous errors
And the pauses of the goal are empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.APauseNeedsAFirstAndALastDay(String n, String days, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 290
[non-AC frame(s) hidden]
```

### F008/AC-060 The trend of a per-week goal marks a Paused week — pending

Step trace:
```
Given the registered user `f8.ac060.user@example.com`, who created the goal `Yoga, 3 times a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} times a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaTimesAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-09T08:00:00Z `f8.ac060.user@example.com` paused the goal from 2026-11-09 until 2026-11-15 without a reason
-> skipped because of previous errors
And `f8.ac060.user@example.com` records Yes for each date from 2026-11-02 to 2026-11-04
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheUserRecords().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("`f{int}\.ac{int}\.user@example\.com` records Yes for each date from {DateTime} to {DateTime}")]
public void GivenF_Ac_UserExample_ComRecordsYesForEachDateFromTo(int p0, int p1, DateTime p2, DateTime p3)
{
    throw new PendingStepException();
}

And `f8.ac060.user@example.com` records Yes for each date from 2026-11-16 to 2026-11-18
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheUserRecords().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("`f{int}\.ac{int}\.user@example\.com` records Yes for each date from {DateTime} to {DateTime}")]
public void GivenF_Ac_UserExample_ComRecordsYesForEachDateFromTo(int p0, int p1, DateTime p2, DateTime p3)
{
    throw new PendingStepException();
}

When at 2026-11-25T12:00:00Z `f8.ac060.user@example.com` reads the report of the goal
-> skipped because of previous errors
Then the trend is, with the target 3: (2026-11-02 – 2026-11-08, 3), (2026-11-09 – 2026-11-15, Paused), (2026-11-16 – 2026-11-22, 3), (2026-11-23 – 2026-11-29, no value)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.TheTrendOfAPer_WeekGoalMarksAPausedWeek() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 640
[non-AC frame(s) hidden]
```

### F008/AC-050 Cancelling just before and on a pause's first day(n: "01", time: "2026-11-16T23:59:59Z", result: "\"pause cancelled\"", __pickleIndex: "82", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac050.01.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac050.01.user@example.com` paused the goal from 2026-11-17 until 2026-11-20 without a reason
-> skipped because of previous errors
When at 2026-11-16T23:59:59Z `f8.ac050.01.user@example.com` cancels the pause 2026-11-17 – 2026-11-20
-> skipped because of previous errors
Then the result is "pause cancelled"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.CancellingJustBeforeAndOnAPausesFirstDay(String n, String time, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 528
[non-AC frame(s) hidden]
```

### F008/AC-028 The pause checks come in their order(n: "06", days: "from 2026-11-17 until 2026-11-18", reason: "`x` repeated 200 times", result: "\"paused\" with the first day 2026-11-17 and the l"···, __pickleIndex: "55", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac028.06.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac028.06.user@example.com` paused the goal from 2026-11-23 until 2026-11-25 without a reason
-> skipped because of previous errors
When at 2026-11-16T09:00:00Z `f8.ac028.06.user@example.com` pauses the goal from 2026-11-17 until 2026-11-18 with the reason `x` repeated 200 times
-> skipped because of previous errors
Then the result is "paused" with the first day 2026-11-17 and the last day 2026-11-18
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ThePauseChecksComeInTheirOrder(String n, String days, String reason, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 303
[non-AC frame(s) hidden]
```

### F008/AC-068 An excused commitment leaves the Pending count and the Today list — pending

Step trace:
```
Given the registered user `f8.ac068.user@example.com`, who created the goal `Drinking water, 8 glasses a day` and the goal `Meditation, 10 min daily`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day` and the goal `Meditation, {int} min daily`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADayAndTheGoalMeditationMinDaily(int p0, int p1, int p2, int p3)
{
    throw new PendingStepException();
}

And at 2026-11-16T09:00:00Z `f8.ac068.user@example.com` paused `Drinking water, 8 glasses a day` from 2026-11-16 until 2026-11-22 without a reason
-> skipped because of previous errors
And `f8.ac068.user@example.com` records on target for each date from 2026-11-02 to 2026-11-13: 8 for `Drinking water, 8 glasses a day` and 10 for `Meditation, 10 min daily`
-> skipped because of previous errors
And at 2026-11-16T10:00:00Z `f8.ac068.user@example.com` excused 2026-11-15 of `Meditation, 10 min daily` without a reason
-> skipped because of previous errors
When at 2026-11-16T10:01:00Z `f8.ac068.user@example.com` reads the dashboard
-> skipped because of previous errors
Then the Pending count is 3
-> skipped because of previous errors
And the Today list contains (`Meditation, 10 min daily`, 2026-11-14, Pending) and no entry of `Meditation, 10 min daily` for 2026-11-15
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnExcusedCommitmentLeavesThePendingCountAndTheTodayList() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 713
[non-AC frame(s) hidden]
```

### F008/AC-073 An administrator cannot use any use case of this slice(n: "05", action: "cancels the pause 2026-11-23 – 2026-11-27", __pickleIndex: "118", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac073.05.a@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac073.05.a@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac073.05.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac073.05.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
And the administrator `f8.ac073.05.admin@example.com`
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac073.05.admin@example.com` cancels the pause 2026-11-23 – 2026-11-27 on the goal of `f8.ac073.05.a@example.com`
-> skipped because of previous errors
Then the result is "Goals are for registered users."
-> skipped because of previous errors
And the pauses of the goal, read by `f8.ac073.05.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac073.05.a@example.com`, gives 2026-11-11 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime}, read by `f{int}\.ac{float}\.a@example\.com`, gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToReadByF_Ac_AExample_ComGivesExcusedReason(DateTime p0, DateTime p1, int p2, Decimal p3, DateTime p4, string ill5)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnAdministratorCannotUseAnyUseCaseOfThisSlice(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 774
[non-AC frame(s) hidden]
```

### F008/AC-014 A commitment that has a record cannot be excused(n: "01", reason: "\"Ill\"", __pickleIndex: "25", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac014.01.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T09:00:00Z `f8.ac014.01.user@example.com` recorded No for 2026-11-11
-> skipped because of previous errors
When at 2026-11-12T10:00:00Z `f8.ac014.01.user@example.com` excuses 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
Then the result is "This commitment has a record. Delete the record to excuse it."
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Pending, record No
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Pending, record No")]
public void ThenTheHistoryOfTheGoalFromToGivesPendingRecordNo(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ACommitmentThatHasARecordCannotBeExcused(String n, String reason, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 154
[non-AC frame(s) hidden]
```

### F008/AC-024 A pause cannot start before today — pending

Step trace:
```
Given the registered user `f8.ac024.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-16T09:00:00Z `f8.ac024.user@example.com` pauses the goal from 2026-11-15 until 2026-11-22 without a reason
-> skipped because of previous errors
Then the result is "A pause cannot start before today."
-> skipped because of previous errors
And the pauses of the goal are empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.APauseCannotStartBeforeToday() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 262
[non-AC frame(s) hidden]
```

### F008/AC-043 Resuming on a pause's first day removes the pause — pending

Step trace:
```
Given the registered user `f8.ac043.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac043.user@example.com` recorded 8 for 2026-11-16
-> skipped because of previous errors
And at 2026-11-16T09:00:00Z `f8.ac043.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 with the reason "Family visit"
-> skipped because of previous errors
When at 2026-11-16T18:00:00Z `f8.ac043.user@example.com` resumes the goal
-> skipped because of previous errors
Then the result is "resumed"
-> skipped because of previous errors
And the pauses of the goal are empty
-> skipped because of previous errors
And the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Due, record 8
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Due, record {int}")]
public void ThenTheHistoryOfTheGoalFromToGivesDueRecord(DateTime p0, DateTime p1, DateTime p2, int p3)
{
    throw new PendingStepException();
}

And at 2026-11-19T00:00:00Z the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Met, record 8
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ResumingOnAPausesFirstDayRemovesThePause() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 458
[non-AC frame(s) hidden]
```

### F008/AC-028 The pause checks come in their order(n: "05", days: "from 2026-11-17 until 2026-11-18", reason: "`x` repeated 201 times", result: "\"Keep the reason to 200 characters or fewer.\"", __pickleIndex: "54", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac028.05.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac028.05.user@example.com` paused the goal from 2026-11-23 until 2026-11-25 without a reason
-> skipped because of previous errors
When at 2026-11-16T09:00:00Z `f8.ac028.05.user@example.com` pauses the goal from 2026-11-17 until 2026-11-18 with the reason `x` repeated 201 times
-> skipped because of previous errors
Then the result is "Keep the reason to 200 characters or fewer."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ThePauseChecksComeInTheirOrder(String n, String days, String reason, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 303
[non-AC frame(s) hidden]
```

### F008/AC-016 A reason has at most 200 characters(n: "02", reason: "`x` repeated 201 times", result: "\"Keep the reason to 200 characters or fewer.\"", entry: "Pending", __pickleIndex: "29", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac016.02.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-12T10:00:00Z `f8.ac016.02.user@example.com` excuses 2026-11-11 with the reason `x` repeated 201 times
-> skipped because of previous errors
Then the result is "Keep the reason to 200 characters or fewer."
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Pending
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Pending")]
public void ThenTheHistoryOfTheGoalFromToGivesPending(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AReasonHasAtMost200Characters(String n, String reason, String result, String entry, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 175
[non-AC frame(s) hidden]
```

### F008/AC-053 Consistency leaves Excused and Paused results out — pending

Step trace:
```
Given the registered user `f8.ac053.user@example.com`, who created the goal `Meditation, 10 min daily`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Meditation, {int} min daily`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalMeditationMinDaily(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-10T08:00:00Z `f8.ac053.user@example.com` excused 2026-11-09 without a reason
-> skipped because of previous errors
And at 2026-11-13T08:00:00Z `f8.ac053.user@example.com` paused the goal from 2026-11-13 until 2026-11-15 without a reason
-> skipped because of previous errors
And `f8.ac053.user@example.com` records 10 for each date from 2026-11-02 to 2026-11-16 except 2026-11-09, 2026-11-11, 2026-11-13, 2026-11-14 and 2026-11-15
-> skipped because of previous errors
When at 2026-11-19T12:00:00Z `f8.ac053.user@example.com` reads the report of the goal
-> skipped because of previous errors
Then the consistency is "90% (10 of 11)"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ConsistencyLeavesExcusedAndPausedResultsOut() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 568
[non-AC frame(s) hidden]
```

### F008/AC-063 A yes/no week whose only record is on a Paused commitment has no value — pending

Step trace:
```
Given the registered user `f8.ac063.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac063.user@example.com` recorded Yes for 2026-11-16
-> skipped because of previous errors
And at 2026-11-16T09:00:00Z `f8.ac063.user@example.com` paused the goal from 2026-11-16 until 2026-11-16 without a reason
-> skipped because of previous errors
When at 2026-11-23T10:00:00Z `f8.ac063.user@example.com` reads the report of the goal
-> skipped because of previous errors
Then the trend contains (2026-11-16 – 2026-11-22, no value, target 2)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AYesNoWeekWhoseOnlyRecordIsOnAPausedCommitmentHasNoValue() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 665
[non-AC frame(s) hidden]
```

### F008/AC-035 Resuming a per-week goal in a new week leaves the earlier partly paused week Paused — pending

Step trace:
```
Given the registered user `f8.ac035.user@example.com`, who created the goal `Yoga, 3 times a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} times a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaTimesAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-18T08:00:00Z `f8.ac035.user@example.com` paused the goal from 2026-11-19 until 2026-11-25 without a reason
-> skipped because of previous errors
When at 2026-11-23T10:00:00Z `f8.ac035.user@example.com` resumes the goal
-> skipped because of previous errors
Then the result is "resumed"
-> skipped because of previous errors
And the pauses of the goal are (2026-11-19, 2026-11-22, no reason, past)
-> skipped because of previous errors
And the history of the goal from 2026-11-16 to 2026-11-29 gives 2026-11-16 – 2026-11-22 Paused and 2026-11-23 – 2026-11-29 Due
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} – {DateTime} Paused and {DateTime} – {DateTime} Due")]
public void ThenTheHistoryOfTheGoalFromToGivesPausedAndDue(DateTime p0, DateTime p1, DateTime p2, DateTime p3, DateTime p4, DateTime p5)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ResumingAPer_WeekGoalInANewWeekLeavesTheEarlierPartlyPausedWeekPaused() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 376
[non-AC frame(s) hidden]
```

### F008/AC-059 A Paused week of a per-week goal is not counted and does not break the streak — pending

Step trace:
```
Given the registered user `f8.ac059.user@example.com`, who created the goal `Yoga, 3 times a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} times a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaTimesAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-09T08:00:00Z `f8.ac059.user@example.com` paused the goal from 2026-11-09 until 2026-11-15 without a reason
-> skipped because of previous errors
And `f8.ac059.user@example.com` records Yes for each date from 2026-11-02 to 2026-11-04
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheUserRecords().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("`f{int}\.ac{int}\.user@example\.com` records Yes for each date from {DateTime} to {DateTime}")]
public void GivenF_Ac_UserExample_ComRecordsYesForEachDateFromTo(int p0, int p1, DateTime p2, DateTime p3)
{
    throw new PendingStepException();
}

And `f8.ac059.user@example.com` records Yes for each date from 2026-11-16 to 2026-11-18
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheUserRecords().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("`f{int}\.ac{int}\.user@example\.com` records Yes for each date from {DateTime} to {DateTime}")]
public void GivenF_Ac_UserExample_ComRecordsYesForEachDateFromTo(int p0, int p1, DateTime p2, DateTime p3)
{
    throw new PendingStepException();
}

When at 2026-11-25T12:00:00Z `f8.ac059.user@example.com` reads the report of the goal
-> skipped because of previous errors
Then the consistency is "100% (2 of 2)"
-> skipped because of previous errors
And the current streak is "2 weeks"
-> skipped because of previous errors
And the longest streak is "2 weeks"
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.APausedWeekOfAPer_WeekGoalIsNotCountedAndDoesNotBreakTheStreak() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 631
[non-AC frame(s) hidden]
```

### F008/AC-028 The pause checks come in their order(n: "04", days: "from 2026-11-24 until 2026-11-30", reason: "`x` repeated 201 times", result: "\"This goal already has a pause on these dates.\"", __pickleIndex: "53", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac028.04.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac028.04.user@example.com` paused the goal from 2026-11-23 until 2026-11-25 without a reason
-> skipped because of previous errors
When at 2026-11-16T09:00:00Z `f8.ac028.04.user@example.com` pauses the goal from 2026-11-24 until 2026-11-30 with the reason `x` repeated 201 times
-> skipped because of previous errors
Then the result is "This goal already has a pause on these dates."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ThePauseChecksComeInTheirOrder(String n, String days, String reason, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 303
[non-AC frame(s) hidden]
```

### F008/AC-010 A date that is not a commitment of the goal cannot be excused(n: "01", date: "2026-11-10", __pickleIndex: "18", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac010.01.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-12T10:00:00Z `f8.ac010.01.user@example.com` excuses 2026-11-10 without a reason
-> skipped because of previous errors
Then the result is "Nothing is scheduled on this date."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ADateThatIsNotACommitmentOfTheGoalCannotBeExcused(String n, String date, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 106
[non-AC frame(s) hidden]
```

### F008/AC-029 For a goal that has not started, "today" is the account's today(n: "01", first: "2026-11-16", result: "\"paused\" with the first day 2026-11-16 and the l"···, __pickleIndex: "56", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac029.01.user@example.com`, who created the goal `Drinking water, 8 glasses a day` at 2026-11-16T08:00:00Z with the start date 2026-11-20
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day` at {DateTime}T{int}:{string}Z with the start date {DateTime}")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADayAtTZWithTheStartDate(int p0, Decimal p1, int p2, DateTime p3, int p4, string p5, DateTime p6)
{
    throw new PendingStepException();
}

When at 2026-11-16T09:00:00Z `f8.ac029.01.user@example.com` pauses the goal from 2026-11-16 until 2026-11-18 without a reason
-> skipped because of previous errors
Then the result is "paused" with the first day 2026-11-16 and the last day 2026-11-18
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ForAGoalThatHasNotStartedTodayIsTheAccountsToday(String n, String first, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 318
[non-AC frame(s) hidden]
```

### F008/AC-028 The pause checks come in their order(n: "02", days: "from 2026-11-15 until 2026-11-14", reason: "\"Away\"", result: "\"A pause cannot start before today.\"", __pickleIndex: "51", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac028.02.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac028.02.user@example.com` paused the goal from 2026-11-23 until 2026-11-25 without a reason
-> skipped because of previous errors
When at 2026-11-16T09:00:00Z `f8.ac028.02.user@example.com` pauses the goal from 2026-11-15 until 2026-11-14 with the reason "Away"
-> skipped because of previous errors
Then the result is "A pause cannot start before today."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ThePauseChecksComeInTheirOrder(String n, String days, String reason, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 303
[non-AC frame(s) hidden]
```

### F008/AC-025 The last day cannot be before the first day — pending

Step trace:
```
Given the registered user `f8.ac025.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-16T09:00:00Z `f8.ac025.user@example.com` pauses the goal from 2026-11-20 until 2026-11-19 without a reason
-> skipped because of previous errors
Then the result is "The last day cannot be before the first day."
-> skipped because of previous errors
And the pauses of the goal are empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.TheLastDayCannotBeBeforeTheFirstDay() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 269
[non-AC frame(s) hidden]
```

### F008/AC-009 A commitment of a period-based goal cannot be excused(n: "02", goal: "`Drinking water, 8 glasses a day`", entry: "2026-11-11 Pending", __pickleIndex: "17", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac009.02.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-12T10:00:00Z `f8.ac009.02.user@example.com` excuses 2026-11-11 without a reason
-> skipped because of previous errors
Then the result is "Only commitments can be excused. This goal is evaluated by period."
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Pending
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Pending")]
public void ThenTheHistoryOfTheGoalFromToGivesPending(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ACommitmentOfAPeriod_BasedGoalCannotBeExcused(String n, String goal, String entry, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 95
[non-AC frame(s) hidden]
```

### F008/AC-018 Removing an excusal just before, at and just after the end of the grace period(n: "01", time: "2026-11-13T23:59:59Z", result: "\"excusal removed\" with the state Pending", state: "Pending", __pickleIndex: "31", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac018.01.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac018.01.user@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
When at 2026-11-13T23:59:59Z `f8.ac018.01.user@example.com` removes the excusal of 2026-11-11
-> skipped because of previous errors
Then the result is "excusal removed" with the state Pending
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Pending
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Pending")]
public void ThenTheHistoryOfTheGoalFromToGivesPending(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.RemovingAnExcusalJustBeforeAtAndJustAfterTheEndOfTheGracePeriod(String n, String time, String result, String state, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 197
[non-AC frame(s) hidden]
```

### F008/AC-007 Excusing just before, at and just after the end of the grace period(n: "01", time: "2026-11-13T23:59:59Z", result: "\"excused\"", state: "Excused", __pickleIndex: "10", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac007.01.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-13T23:59:59Z `f8.ac007.01.user@example.com` excuses 2026-11-11 without a reason
-> skipped because of previous errors
Then the result is "excused"
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Excused
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Excused")]
public void ThenTheHistoryOfTheGoalFromToGivesExcused(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ExcusingJustBeforeAtAndJustAfterTheEndOfTheGracePeriod(String n, String time, String result, String state, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 70
[non-AC frame(s) hidden]
```

### F008/AC-009 A commitment of a period-based goal cannot be excused(n: "01", goal: "`Yoga, 3 times a week`", entry: "2026-11-09 – 2026-11-15 Due", __pickleIndex: "16", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac009.01.user@example.com`, who created the goal `Yoga, 3 times a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} times a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaTimesAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-12T10:00:00Z `f8.ac009.01.user@example.com` excuses 2026-11-11 without a reason
-> skipped because of previous errors
Then the result is "Only commitments can be excused. This goal is evaluated by period."
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-09 – 2026-11-15 Due
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} – {DateTime} Due")]
public void ThenTheHistoryOfTheGoalFromToGivesDue(DateTime p0, DateTime p1, DateTime p2, DateTime p3)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ACommitmentOfAPeriod_BasedGoalCannotBeExcused(String n, String goal, String entry, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 95
[non-AC frame(s) hidden]
```

### F008/AC-021 An excused commitment cannot be recorded — pending

Step trace:
```
Given the registered user `f8.ac021.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac021.user@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
When at 2026-11-12T11:00:00Z `f8.ac021.user@example.com` records Yes for 2026-11-11
-> skipped because of previous errors
Then the result is "This commitment is excused. Remove the excusal to record it."
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Excused, no record
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Excused, no record")]
public void ThenTheHistoryOfTheGoalFromToGivesExcusedNoRecord(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnExcusedCommitmentCannotBeRecorded() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 232
[non-AC frame(s) hidden]
```

### F008/AC-037 An excusal under a pause is kept and shows again when the pause is removed — pending

Step trace:
```
Given the registered user `f8.ac037.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T07:00:00Z `f8.ac037.user@example.com` excused 2026-11-16 with the reason "Ill"
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac037.user@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
When at 2026-11-16T09:00:00Z `f8.ac037.user@example.com` resumes the goal
-> skipped because of previous errors
Then the result is "resumed"
-> skipped because of previous errors
And the pauses of the goal are empty
-> skipped because of previous errors
And the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToGivesExcusedReason(DateTime p0, DateTime p1, DateTime p2, string ill3)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnExcusalUnderAPauseIsKeptAndShowsAgainWhenThePauseIsRemoved() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 395
[non-AC frame(s) hidden]
```

### F008/AC-074 An anonymous visitor cannot use any use case of this slice(n: "01", action: "excuses 2026-11-18 without a reason", __pickleIndex: "120", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac074.01.a@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac074.01.a@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac074.01.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac074.01.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z the anonymous visitor excuses 2026-11-18 without a reason on the goal of `f8.ac074.01.a@example.com`
-> skipped because of previous errors
Then the result is "You are not signed in."
-> skipped because of previous errors
And the pauses of the goal, read by `f8.ac074.01.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac074.01.a@example.com`, gives 2026-11-11 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime}, read by `f{int}\.ac{float}\.a@example\.com`, gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToReadByF_Ac_AExample_ComGivesExcusedReason(DateTime p0, DateTime p1, int p2, Decimal p3, DateTime p4, string ill5)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnAnonymousVisitorCannotUseAnyUseCaseOfThisSlice(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 794
[non-AC frame(s) hidden]
```

### F008/AC-086 For a goal that has not started, a pause is current or past by the account's today(n: "02", time: "2026-11-19T10:00:00Z", kind: "past", __pickleIndex: "146", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac086.02.user@example.com`, who created the goal `Drinking water, 8 glasses a day` at 2026-11-16T08:00:00Z with the start date 2026-11-20
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day` at {DateTime}T{int}:{string}Z with the start date {DateTime}")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADayAtTZWithTheStartDate(int p0, Decimal p1, int p2, DateTime p3, int p4, string p5, DateTime p6)
{
    throw new PendingStepException();
}

And at 2026-11-16T09:00:00Z `f8.ac086.02.user@example.com` paused the goal from 2026-11-16 until 2026-11-18 without a reason
-> skipped because of previous errors
When at 2026-11-19T10:00:00Z `f8.ac086.02.user@example.com` reads the pauses of the goal
-> skipped because of previous errors
Then they are (2026-11-16, 2026-11-18, no reason, past)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ForAGoalThatHasNotStartedAPauseIsCurrentOrPastByTheAccountsToday(String n, String time, String kind, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 924
[non-AC frame(s) hidden]
```

### F008/AC-019 Removing an excusal that does not exist(n: "01", date: "2026-11-11", state: "Pending", __pickleIndex: "34", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac019.01.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-12T10:00:00Z `f8.ac019.01.user@example.com` removes the excusal of 2026-11-11
-> skipped because of previous errors
Then the result is "This commitment is not excused."
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Pending
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Pending")]
public void ThenTheHistoryOfTheGoalFromToGivesPending(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.RemovingAnExcusalThatDoesNotExist(String n, String date, String state, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 210
[non-AC frame(s) hidden]
```

### F008/AC-026 A pause cannot share a date with another pause of the goal, but may touch it(n: "02", first: "2026-11-22", last: "2026-11-22", result: "\"This goal already has a pause on these dates.\"", __pickleIndex: "44", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac026.02.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T09:00:00Z `f8.ac026.02.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 with the reason "Family visit"
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac026.02.user@example.com` pauses the goal from 2026-11-22 until 2026-11-22 without a reason
-> skipped because of previous errors
Then the result is "This goal already has a pause on these dates."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.APauseCannotShareADateWithAnotherPauseOfTheGoalButMayTouchIt(String n, String first, String last, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 276
[non-AC frame(s) hidden]
```

### F008/AC-018 Removing an excusal just before, at and just after the end of the grace period(n: "03", time: "2026-11-14T00:00:01Z", result: "\"This excusal can no longer be removed.\"", state: "Excused", __pickleIndex: "33", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac018.03.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac018.03.user@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
When at 2026-11-14T00:00:01Z `f8.ac018.03.user@example.com` removes the excusal of 2026-11-11
-> skipped because of previous errors
Then the result is "This excusal can no longer be removed."
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Excused
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Excused")]
public void ThenTheHistoryOfTheGoalFromToGivesExcused(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.RemovingAnExcusalJustBeforeAtAndJustAfterTheEndOfTheGracePeriod(String n, String time, String result, String state, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 197
[non-AC frame(s) hidden]
```

### F008/AC-055 Recent results include Excused and Paused results and count each state — pending

Step trace:
```
Given the registered user `f8.ac055.user@example.com`, who created the goal `Meditation, 10 min daily`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Meditation, {int} min daily`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalMeditationMinDaily(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-10T08:00:00Z `f8.ac055.user@example.com` excused 2026-11-09 without a reason
-> skipped because of previous errors
And at 2026-11-13T08:00:00Z `f8.ac055.user@example.com` paused the goal from 2026-11-13 until 2026-11-15 without a reason
-> skipped because of previous errors
And `f8.ac055.user@example.com` records 10 for each date from 2026-11-02 to 2026-11-16 except 2026-11-09, 2026-11-11, 2026-11-13, 2026-11-14 and 2026-11-15
-> skipped because of previous errors
When at 2026-11-19T12:00:00Z `f8.ac055.user@example.com` reads the report of the goal
-> skipped because of previous errors
Then the recent results are, oldest first: 2026-11-02 to 2026-11-08 each Met; 2026-11-09 Excused; 2026-11-10 Met; 2026-11-11 Missed; 2026-11-12 Met; 2026-11-13, 2026-11-14 and 2026-11-15 each Paused; 2026-11-16 Met; 2026-11-17 and 2026-11-18 each Pending
-> skipped because of previous errors
And the number of each state among them is Met 10, Missed 1, Excused 1, Paused 3, Pending 2
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.RecentResultsIncludeExcusedAndPausedResultsAndCountEachState() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 588
[non-AC frame(s) hidden]
```

### F008/AC-064 A yes/no week whose commitments are all Excused is marked Excused — pending

Step trace:
```
Given the registered user `f8.ac064.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T07:00:00Z `f8.ac064.user@example.com` excused 2026-11-16 without a reason
-> skipped because of previous errors
And at 2026-11-18T07:00:00Z `f8.ac064.user@example.com` excused 2026-11-18 without a reason
-> skipped because of previous errors
And at 2026-11-20T07:00:00Z `f8.ac064.user@example.com` excused 2026-11-20 without a reason
-> skipped because of previous errors
When at 2026-11-23T10:00:00Z `f8.ac064.user@example.com` reads the report of the goal
-> skipped because of previous errors
Then the trend contains (2026-11-16 – 2026-11-22, Excused)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AYesNoWeekWhoseCommitmentsAreAllExcusedIsMarkedExcused() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 674
[non-AC frame(s) hidden]
```

### F008/AC-040 A date in a pause cannot be recorded — pending

Step trace:
```
Given the registered user `f8.ac040.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T09:00:00Z `f8.ac040.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 without a reason
-> skipped because of previous errors
When at 2026-11-17T10:00:00Z `f8.ac040.user@example.com` records 8 for 2026-11-17
-> skipped because of previous errors
Then the result is "This goal is paused on this date."
-> skipped because of previous errors
And the history of the goal from 2026-11-17 to 2026-11-17 gives 2026-11-17 Paused, no record
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Paused, no record")]
public void ThenTheHistoryOfTheGoalFromToGivesPausedNoRecord(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ADateInAPauseCannotBeRecorded() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 422
[non-AC frame(s) hidden]
```

### F008/AC-011 A date in a pause cannot be excused, also when it has not begun(n: "01", date: "2026-11-16", __pickleIndex: "20", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac011.01.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac011.01.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 without a reason
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac011.01.user@example.com` excuses 2026-11-16 without a reason
-> skipped because of previous errors
Then the result is "This goal is paused on this date."
-> skipped because of previous errors
And the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Paused
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Paused")]
public void ThenTheHistoryOfTheGoalFromToGivesPaused(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ADateInAPauseCannotBeExcusedAlsoWhenItHasNotBegun(String n, String date, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 119
[non-AC frame(s) hidden]
```

### F008/AC-073 An administrator cannot use any use case of this slice(n: "04", action: "resumes the goal", __pickleIndex: "117", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac073.04.a@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac073.04.a@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac073.04.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac073.04.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
And the administrator `f8.ac073.04.admin@example.com`
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac073.04.admin@example.com` resumes the goal on the goal of `f8.ac073.04.a@example.com`
-> skipped because of previous errors
Then the result is "Goals are for registered users."
-> skipped because of previous errors
And the pauses of the goal, read by `f8.ac073.04.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac073.04.a@example.com`, gives 2026-11-11 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime}, read by `f{int}\.ac{float}\.a@example\.com`, gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToReadByF_Ac_AExample_ComGivesExcusedReason(DateTime p0, DateTime p1, int p2, Decimal p3, DateTime p4, string ill5)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnAdministratorCannotUseAnyUseCaseOfThisSlice(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 774
[non-AC frame(s) hidden]
```

### F008/AC-074 An anonymous visitor cannot use any use case of this slice(n: "02", action: "removes the excusal of 2026-11-11", __pickleIndex: "121", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac074.02.a@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac074.02.a@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac074.02.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac074.02.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z the anonymous visitor removes the excusal of 2026-11-11 on the goal of `f8.ac074.02.a@example.com`
-> skipped because of previous errors
Then the result is "You are not signed in."
-> skipped because of previous errors
And the pauses of the goal, read by `f8.ac074.02.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac074.02.a@example.com`, gives 2026-11-11 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime}, read by `f{int}\.ac{float}\.a@example\.com`, gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToReadByF_Ac_AExample_ComGivesExcusedReason(DateTime p0, DateTime p1, int p2, Decimal p3, DateTime p4, string ill5)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnAnonymousVisitorCannotUseAnyUseCaseOfThisSlice(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 794
[non-AC frame(s) hidden]
```

### F008/AC-006 A final commitment cannot be excused(n: "01", setup: "records nothing", date: "2026-11-09", state: "Missed", __pickleIndex: "7", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac006.01.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And `f8.ac006.01.user@example.com` records nothing
-> skipped because of previous errors
When at 2026-11-12T10:00:00Z `f8.ac006.01.user@example.com` excuses 2026-11-09 without a reason
-> skipped because of previous errors
Then the result is "This commitment is final and can no longer be excused."
-> skipped because of previous errors
And the history of the goal from 2026-11-09 to 2026-11-09 gives 2026-11-09 Missed
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Missed")]
public void ThenTheHistoryOfTheGoalFromToGivesMissed(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AFinalCommitmentCannotBeExcused(String n, String setup, String date, String state, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 57
[non-AC frame(s) hidden]
```

### F008/AC-072 Another registered user cannot use any use case of this slice on someone else's goal(n: "03", action: "pauses the goal from 2026-11-28 until 2026-11-29 w"···, __pickleIndex: "110", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac072.03.a@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.a@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_AExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T10:00:00Z `f8.ac072.03.a@example.com` excused 2026-11-11 with the reason "Ill"
-> skipped because of previous errors
And at 2026-11-16T08:00:00Z `f8.ac072.03.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
-> skipped because of previous errors
And at 2026-11-16T08:01:00Z `f8.ac072.03.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
And the registered user `f8.ac072.03.b@example.com`
-> skipped because of previous errors
When at 2026-11-16T10:00:00Z `f8.ac072.03.b@example.com` pauses the goal from 2026-11-28 until 2026-11-29 without a reason on the goal of `f8.ac072.03.a@example.com`
-> skipped because of previous errors
Then the result is "This goal does not exist."
-> skipped because of previous errors
And the pauses of the goal, read by `f8.ac072.03.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac072.03.a@example.com`, gives 2026-11-11 Excused, reason "Ill"
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime}, read by `f{int}\.ac{float}\.a@example\.com`, gives {DateTime} Excused, reason {string}")]
public void ThenTheHistoryOfTheGoalFromToReadByF_Ac_AExample_ComGivesExcusedReason(DateTime p0, DateTime p1, int p2, Decimal p3, DateTime p4, string ill5)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnotherRegisteredUserCannotUseAnyUseCaseOfThisSliceOnSomeoneElsesGoal(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 753
[non-AC frame(s) hidden]
```

### F008/AC-048 An upcoming pause can be cancelled — pending

Step trace:
```
Given the registered user `f8.ac048.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{int}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, int p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac048.user@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
-> skipped because of previous errors
When at 2026-11-18T10:00:00Z `f8.ac048.user@example.com` cancels the pause 2026-11-23 – 2026-11-27
-> skipped because of previous errors
Then the result is "pause cancelled"
-> skipped because of previous errors
And the pauses of the goal are empty
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.AnUpcomingPauseCanBeCancelled() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 508
[non-AC frame(s) hidden]
```

### F008/AC-078 The demo excusals, pauses, states and notes of `demo6@example.com` — pending

Step trace:
```
Given the demo run at 2026-11-18T10:00:00Z
-> done: GivenSteps.GivenTheDemoRun("2026-11-18T10:00:00Z") (0,5s)
When at 2026-11-18T10:00:00Z `demo6@example.com` reads the history of `Meditation, 10 min daily` from 2026-10-21 to 2026-11-18
-> done: ReadSteps.WhenTheUserReadsTheHistoryOf("2026-11-18T10:00:00Z", "demo6@example.com", "Meditation, 10 mi...", "2026-10-21", "2026-11-18") (0,1s)
Then it gives 2026-10-29 Excused, reason "Travelling."; 2026-10-30 Excused, reason "Travelling."; 2026-11-04 Missed, no record; 2026-11-10 Excused, no reason; 2026-11-15 Met, record 10; 2026-11-16 Excused, no reason; 2026-11-17 Pending, no record; 2026-11-18 Due, no record
-> done: ResultThenSteps.ThenItGives("2026-10-29 Excuse...") (0,0s)
And the recent notes of `Meditation, 10 min daily` are (2026-11-04, "Overslept.", Missed)
-> done: ResultThenSteps.ThenTheRecentNotesOfAre("Meditation, 10 mi...", "(2026-11-04, "Ove...") (0,0s)
And the history of `Drinking water, 8 glasses a day` from 2026-10-30 to 2026-11-18 gives 2026-10-30 Met, record 8; 2026-10-31 to 2026-11-06 each Paused, no record; 2026-11-07 Met, record 8; 2026-11-13 Failed, record 6; 2026-11-16 Pending, record 8; 2026-11-17 Paused, no record; 2026-11-18 Paused, no record
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of `Drinking water, {int} glasses a day` from {DateTime} to {DateTime} gives {DateTime} Met, record {int}; {DateTime} to {DateTime} each Paused, no record; {DateTime} Met, record {int}; {DateTime} Failed, record {int}; {DateTime} Pending, record {int}; {DateTime} Paused, no record; {DateTime} Paused, no record")]
public void ThenTheHistoryOfDrinkingWaterGlassesADayFromToGivesMetRecordToEachPausedNoRecordMetRecordFailedRecordPendingRecordPausedNoRecordPausedNoRecord(int p0, DateTime p1, DateTime p2, DateTime p3, int p4, DateTime p5, DateTime p6, DateTime p7, int p8, DateTime p9, int p10, DateTime p11, int p12, DateTime p13, DateTime p14)
{
    throw new PendingStepException();
}

And the recent notes of `Drinking water, 8 glasses a day` are (2026-11-13, "Busy day.", Failed)
-> skipped because of previous errors
And the pauses of `Drinking water, 8 glasses a day` are, in this order, (2026-11-17, 2026-11-23, reason "Family visit.", current) and (2026-10-31, 2026-11-06, reason "Ill with flu.", past)
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.TheDemoExcusalsPausesStatesAndNotesOfDemo6Example_Com() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 851
[non-AC frame(s) hidden]
```

### F008/AC-019 Removing an excusal that does not exist(n: "02", date: "2026-11-09", state: "Missed", __pickleIndex: "35", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac019.02.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

When at 2026-11-12T10:00:00Z `f8.ac019.02.user@example.com` removes the excusal of 2026-11-09
-> skipped because of previous errors
Then the result is "This commitment is not excused."
-> skipped because of previous errors
And the history of the goal from 2026-11-09 to 2026-11-09 gives 2026-11-09 Missed
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Missed")]
public void ThenTheHistoryOfTheGoalFromToGivesMissed(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.RemovingAnExcusalThatDoesNotExist(String n, String date, String state, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 210
[non-AC frame(s) hidden]
```

### F008/AC-014 A commitment that has a record cannot be excused(n: "02", reason: "`x` repeated 201 times", __pickleIndex: "26", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac014.02.user@example.com`, who created the goal `Yoga, 3 days a week`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Yoga, {int} days a week`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalYogaDaysAWeek(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-12T09:00:00Z `f8.ac014.02.user@example.com` recorded No for 2026-11-11
-> skipped because of previous errors
When at 2026-11-12T10:00:00Z `f8.ac014.02.user@example.com` excuses 2026-11-11 with the reason `x` repeated 201 times
-> skipped because of previous errors
Then the result is "This commitment has a record. Delete the record to excuse it."
-> skipped because of previous errors
And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Pending, record No
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: HistorySteps.ThenTheHistory().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Then("the history of the goal from {DateTime} to {DateTime} gives {DateTime} Pending, record No")]
public void ThenTheHistoryOfTheGoalFromToGivesPendingRecordNo(DateTime p0, DateTime p1, DateTime p2)
{
    throw new PendingStepException();
}
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.ACommitmentThatHasARecordCannotBeExcused(String n, String reason, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 154
[non-AC frame(s) hidden]
```

### F008/AC-050 Cancelling just before and on a pause's first day(n: "02", time: "2026-11-17T00:00:00Z", result: "\"Only a pause that has not begun can be cancelled"···, __pickleIndex: "83", exampleTags: []) — pending

Step trace:
```
Given the registered user `f8.ac050.02.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
-> No matching step definition found for the step. There are matching step definitions, but none of them have matching scope for this step: ArrangeSteps.GivenTheRegisteredUser().
-> undefined: Change the scope or use the following code to create a new step definition:
        [Given("the registered user `f{int}\.ac{float}\.user@example\.com`, who created the goal `Drinking water, {int} glasses a day`")]
public void GivenTheRegisteredUserF_Ac_UserExample_ComWhoCreatedTheGoalDrinkingWaterGlassesADay(int p0, Decimal p1, int p2)
{
    throw new PendingStepException();
}

And at 2026-11-16T08:00:00Z `f8.ac050.02.user@example.com` paused the goal from 2026-11-17 until 2026-11-20 without a reason
-> skipped because of previous errors
When at 2026-11-17T00:00:00Z `f8.ac050.02.user@example.com` cancels the pause 2026-11-17 – 2026-11-20
-> skipped because of previous errors
Then the result is "Only a pause that has not begun can be cancelled. Resume the goal instead."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.MissingStepDefinitionException : No matching step definition found for one or more steps.
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F008.ExcusalsAndPausesFeature.CancellingJustBeforeAndOnAPausesFirstDay(String n, String time, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-7-c0f9d5\tests\HT.AcceptanceTests\Features\F008\excusals-and-pauses.feature:line 528
[non-AC frame(s) hidden]
```

## Regression (other features)

Total: 1060 · passed: 1060 · failed: 0 · pending: 0 · skipped: 0

| Feature | Scenario | Tags | Outcome | Duration |
|---|---|---|---|---|
| F004 | The next commitment is the first commitment that has not ended(n: "02", time: "`2026-11-18T00:00:00Z`", next: "2026-11-18", __pickleIndex: "115", exampleTags: []) | @F004 @AC-023 @FR-014 @US2-AS2 | passed | 00:00:00.0889561 |
| F002 | The anonymous visitor has no access to custom habits(n: "02", action: "reads `Secret` by its identifier", __pickleIndex: "215", exampleTags: []) | @F002 @AC-055 @FR-023 @US4-AS9 | passed | 00:00:00.0880197 |
| F001 | Demo loading creates the three demo accounts | @F001 @AC-057 @FR-016 @US7-AS1 | passed | 00:00:00.4651749 |
| F002 | Field checks refuse a custom habit with the same messages(n: "06", name: "`No icon`", description: "none", category: "Body", intent: "Build", icon: "none", result: "\"Choose an icon.\"", __pickleIndex: "224", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0849376 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "01", count: "2", marks: "none marked", result: "\"Mark one suggested starting goal for the card.\"", after: "no suggested starting goal", __pickleIndex: "109", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0856112 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "01", fields: "Build, count 1 `page`, 1 time per day", storedUnit: "`page`", __pickleIndex: "138", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0927280 |
| F001 | An unknown email, a wrong password and a blocked account give the same result | @F001 @AC-014 @FR-007 @FR-008 | passed | 00:00:00.3474834 |
| F004 | A time zone is given only to its own account | @F004 @AC-080 @FR-024 @US10-AS9 | passed | 00:00:00.2593510 |
| F005 | The anonymous visitor gets "You are not signed in." from every use case of this slice(n: "03", useCase: "delete, with the owner's record identifier", __pickleIndex: "203", exampleTags: []) | @F005 @AC-050 @FR-018 @US6-AS4 | passed | 00:00:00.0937058 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "03", browserTimeZone: "the browser time zone `Mars/Olympus`", timeZone: "`UTC`", __pickleIndex: "224", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0786857 |
| F004 | A custom habit is looked up only among the acting user's custom habits(n: "04", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "the identifier of `A4.033 04 Reading`", __pickleIndex: "144", exampleTags: []) | @F004 @AC-033 @FR-008 @US3-AS5 | passed | 00:00:00.2392288 |
| F007 | A latest Failed or Missed result gives a current streak of 0(n: "01", nov6: "12", __pickleIndex: "17", exampleTags: []) | @F007 @AC-011 @FR-003 @US2-AS3 | passed | 00:00:00.1282898 |
| F005 | A note can be added to a Missed commitment and leaves it Missed | @F005 @AC-035 @FR-007 @US4-AS1 | passed | 00:00:00.1037078 |
| F005 | A final per-week Build count period counts only the records that reach the amount(n: "02", records: "20 for 2026-11-16, 25 for 2026-11-17, 12 for 2026-"···, state: "Met", progress: "progress 3 of 3 times", __pickleIndex: "71", exampleTags: []) | @F005 @AC-018 @FR-014 @US2-AS8 | passed | 00:00:00.1083291 |
| F002 | The anonymous visitor has no access to custom habits(n: "04", action: "edits `Secret` by its identifier, with the name `S"···, __pickleIndex: "217", exampleTags: []) | @F002 @AC-055 @FR-023 @US4-AS9 | passed | 00:00:00.0866087 |
| F005 | The anonymous visitor gets "You are not signed in." from every use case of this slice(n: "02", useCase: "change, with the owner's record identifier", __pickleIndex: "202", exampleTags: []) | @F005 @AC-050 @FR-018 @US6-AS4 | passed | 00:00:00.1046110 |
| F007 | Changing a Met record to an amount below the target makes it Failed | @F007 @AC-017 @FR-008 @US3-AS2 | passed | 00:00:00.1214342 |
| F005 | Around a clock change the grace period lasts exactly 48 elapsed hours(n: "02", createdAt: "`2026-10-24T08:00:00Z`", date: "2026-10-24", time: "`2026-10-26T21:00:00Z`", start: "`2026-10-23T21:00:00Z`", end: "`2026-10-24T21:00:00Z`", entry: "Missed, no grace end", __pickleIndex: "103", exampleTags: []) | @F005 @AC-023 @FR-016 @US2-AS13 | passed | 00:00:00.0995654 |
| F004 | Reading a goal gives all its fields, its settings version and its attempt | @F004 @AC-025 @FR-005 @FR-016 @FR-018 @US2-AS4 | passed | 00:00:00.1737727 |
| F004 | Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "06", submitted: "Build, count 20 `\"  pages  \"`, 3 times per week", stored: "count 20 `pages`", targetText: "\"20 pages, 3 times a week\"", __pickleIndex: "71", exampleTags: []) | @F004 @AC-013 @FR-002 @FR-006 | passed | 00:00:00.1699671 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "18", settings: "Reduce, yes/no, fixed days Monday to Friday", targetText: "\"avoided on Mon–Fri\"", __pickleIndex: "104", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0896643 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "04", action: "starts a new goal from", habit: "an identifier that the test creates and that belon"···, __pickleIndex: "75", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | passed | 00:00:00.1738776 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "02", second: "Build, no measurement, 1 time per day", result: "\"Choose how it is measured.\"", __pickleIndex: "117", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0879173 |
| F007 | demo5's Strength training and Screen time goals on other weekdays(n: "02", run: "2026-11-17T10:00:00Z", strength: "\"80% (8 of 10)\"", current: "\"2 weeks\"", longest: "\"4 weeks\"", __pickleIndex: "86", exampleTags: []) | @F007 @AC-049 @FR-032 | passed | 00:00:00.5797955 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@.example.com\"", whyItIsNotValid: "empty domain part before two non-empty ones", __pickleIndex: "14", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002822 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "20", second: "Build, duration 15 minutes, 0 times per week", result: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "135", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0919001 |
| F004 | An administrator gets no goal use case(n: "04", action: "reads `f4.ac031.user.04@example.com`'s goal", __pickleIndex: "132", exampleTags: []) | @F004 @AC-031 @FR-021 @US3-AS3 | passed | 00:00:00.1692949 |
| F002 | Each of the six categories can be given to a catalog habit(n: "04", category: "Money", __pickleIndex: "161", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0811485 |
| F005 | The demo goals' recent notes give the demo notes with their states | @F005 @AC-056 @FR-008 @FR-030 @US7-AS2 | passed | 00:00:00.4825080 |
| F004 | A suggested starting goal position the habit does not have is refused(n: "02", position: "0", __pickleIndex: "5", exampleTags: []) | @F004 @AC-004 @FR-007 @US1-AS4 | passed | 00:00:00.1749265 |
| F007 | Every streak carries the unit of its goal, singular for 1(n: "02", settings: "Build, yes/no, 1 time per day", created: "2026-11-01T08:00:00Z", start: "2026-11-01", yes: "2026-11-02 and 2026-11-03", read: "2026-11-06T10:00:00Z", streak: "\"2 days\"", number: "2", unit: "\"days\"", __pickleIndex: "24", exampleTags: []) | @F007 @AC-014 @FR-004 @US2-AS6 | passed | 00:00:00.1034417 |
| F004 | A goal name has at most 60 characters after trimming(n: "03", name: "two spaces, a text of 60 characters and two spaces", result: "creating succeeds, and reading the goal gives the "···, __pickleIndex: "59", exampleTags: []) | @F004 @AC-011 @FR-006 @FR-009 | passed | 00:00:00.1874029 |
| F004 | A fixed-days goal has one commitment per scheduled day, from midnight to midnight in its time zone | @F004 @AC-036 @FR-003 @FR-011 @FR-012 @FR-013 @US4-AS1 @US10-AS3 | passed | 00:00:00.1059404 |
| F004 | Each goal follows a time zone change from its own next day or week(n: "01", schedule: "1 time per day", startDate: "2026-11-16", from: "2026-11-18", to: "2026-11-19", entries: "2026-11-18 from `2026-11-17T22:00:00Z` to `2026-11"···, __pickleIndex: "240", exampleTags: []) | @F004 @AC-072 @FR-026 @FR-027 @US10-AS5 | passed | 00:00:00.0997797 |
| F005 | The demo goal "Sugar, at most 1 sweet a day" shows Met, Failed with a note, Missed, Pending and Due | @F005 @AC-053 @FR-030 @US7-AS2 | passed | 00:00:00.4512656 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "17", settings: "Reduce, yes/no, 2 times per week", records: "Yes for 2026-11-16, Yes for 2026-11-17 and Yes for"···, state: "Failed", progress: "progress 3 of at most 2", __pickleIndex: "253", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1188598 |
| F007 | Around a clock change forward, a grace period ends at 01:00 local time and a 23-hour day counts once(n: "03", read: "2026-03-31T22:00:00Z", consistency: "\"85% (6 of 7)\"", current: "\"1 commitment\"", date: "2026-03-29", state: "Met", __pickleIndex: "111", exampleTags: []) | @F007 @AC-061 @FR-001 @FR-003 @FR-008 | passed | 00:00:00.1408582 |
| F004 | A new version's first period that has already ended is kept | @F004 @AC-083 @FR-027 | passed | 00:00:00.1088835 |
| F001 | Demo loading without a demo password creates no account(demoPassword: "\"\"", __pickleIndex: "85", exampleTags: []) | @F001 @AC-055 @FR-016 @US7-AS3 | passed | 00:00:00.3262173 |
| F001 | A password of only spaces creates no account | @F001 @AC-044 @FR-014 @US4-AS5 | passed | 00:00:00.0876629 |
| F005 | An evaluation period takes at most 50 records, but its records can still be changed or deleted(n: "03", action: "records no value for 2026-11-18", result: "the result is \"Choose Yes or No.\" and the histor"···, __pickleIndex: "127", exampleTags: []) | @F005 @AC-033 @FR-003 @FR-005 | passed | 00:00:00.2197216 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "18", second: "Build, duration 15 minutes, no schedule", result: "\"Choose fixed days or how many times.\"", __pickleIndex: "133", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0900861 |
| F005 | A date that belongs to no commitment or period cannot be recorded(n: "05", schedule: "3 times per week", createdAt: "`2026-11-18T10:00:00Z`", startDate: "2026-11-18", date: "2026-11-22", why: "before the first week, and not begun: the date che"···, __pickleIndex: "8", exampleTags: []) | @F005 @AC-005 @FR-002 @US1-AS5 | passed | 00:00:00.1122507 |
| F004 | The catalog suggestion text of the starting values follows the goal rules | @F004 @AC-019 @FR-007 | passed | 00:00:00.1809723 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "05", search: "`\"\"`", morning: "contains", evening: "contains", __pickleIndex: "12", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.0823495 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "27", settings: "Reduce, yes/no, 2 times per day", records: "Yes and No for 2026-11-18", state: "Met", progress: "progress 1 of at most 2", __pickleIndex: "263", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1077381 |
| F004 | A habit that changed between starting and creating is looked up again(n: "01", habit: "`A4.015 01 Reading`", change: "the administrator unpublished `A4.015 01 Reading` "···, message: "\"This habit is not in the catalog.\"", __pickleIndex: "80", exampleTags: []) | @F004 @AC-015 @FR-008 @FR-034 | passed | 00:00:00.1904026 |
| F004 | Each field refusal of goal creation, with its message(n: "08", settings: "Build, duration 0 minutes, 3 times per week", message: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "15", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1734606 |
| F002 | Only the 24 habit icons are accepted(n: "16", icon: "`salad`", result: "success", __pickleIndex: "179", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0899740 |
| F007 | After a time zone change each result is evaluated with the version of its own date(n: "01", read: "2026-11-06T23:00:00Z", consistency: "\"100% (3 of 3)\"", results: "2026-11-01 Met, 2026-11-02 Met, 2026-11-03 Met, 20"···, __pickleIndex: "39", exampleTags: []) | @F007 @AC-020 @FR-009 @US3-AS5 | passed | 00:00:00.1112071 |
| F002 | Custom habits never appear in browsing, for any audience(n: "03", actingUser: "the administrator `ac006.admin.03@example.com`", __pickleIndex: "16", exampleTags: []) | @F002 @AC-006 @FR-024 @US1-AS6 | passed | 00:00:00.2565616 |
| F004 | Another user's goal does not exist for them(n: "04", action: "reads the evaluation calendar from 2026-11-18 to 2"···, goal: "an identifier that the test creates and that belon"···, __pickleIndex: "127", exampleTags: []) | @F004 @AC-029 @FR-020 @US3-AS1 | passed | 00:00:00.2542386 |
| F005 | A final Reduce count commitment is Met at or below its limit and Failed above it(n: "04", count: "count 2 `cups`", value: "3", state: "Failed", __pickleIndex: "59", exampleTags: []) | @F005 @AC-015 @FR-014 @US2-AS5 | passed | 00:00:00.0932845 |
| F002 | A name of 60 characters and a description of 500 characters are accepted | @F002 @AC-028 @FR-005 @US3-AS10 | passed | 00:00:00.0906718 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "06", state: "Unpublished", action: "edits `A026 06 Other`", __pickleIndex: "69", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | passed | 00:00:00.0993301 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@.\"", whyItIsNotValid: "domain has no non-empty part", __pickleIndex: "10", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002764 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "04", action: "reads a custom habit with the identifier of `A058 "···, __pickleIndex: "234", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.1924240 |
| F004 | A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "02", schedule: "fixed days Sunday", date: "2027-10-31", entry: "2027-10-31 from `2027-10-30T21:00:00Z` to `2027-10"···, __pickleIndex: "165", exampleTags: []) | @F004 @AC-040 @FR-012 @FR-013 @US4-AS5 | passed | 00:00:00.0970028 |
| F002 | The anonymous visitor cannot manage the catalog(n: "02", action: "reads `A031 02 Published` as an administrator", __pickleIndex: "95", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0840516 |
| F007 | demo5 has one result Missed this week on every weekday(n: "03", run: "2026-11-18T10:00:00Z", date: "2026-11-15", __pickleIndex: "94", exampleTags: []) | @F007 @AC-053 @FR-032 @FR-012 @US8-AS11 | passed | 00:00:00.5310703 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "03", name: "a name of 61 characters with the prefix `A027 03`", description: "none", category: "Body", intent: "Build", icon: "`book-open`", result: "\"The name can have at most 60 characters.\"", __pickleIndex: "72", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0785612 |
| F001 | Registration checks the email before the password rule | @F001 @AC-009 @FR-003 | passed | 00:00:00.0011979 |
| F002 | The Home preview has fewer than four habits when fewer are Published | @F002 @AC-072 @FR-008 @US5-AS2 | passed | 00:00:00.2799184 |
| F001 | A registered user gets only their own account details | @F001 @AC-029 @FR-010 @FR-012 @US3-AS4 | passed | 00:00:00.1358578 |
| F004 | The edges of every target range are accepted(n: "01", settings: "Build, count 1 `pages`, 3 times per week", targetText: "\"1 pages, 3 times a week\"", __pickleIndex: "44", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | passed | 00:00:00.1614469 |
| F002 | A registered user cannot manage the catalog(n: "03", action: "creates the catalog habit `A030 03 New`", __pickleIndex: "90", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1847140 |
| F002 | The anonymous visitor cannot manage the catalog(n: "03", action: "creates the catalog habit `A031 03 New`", __pickleIndex: "96", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0841231 |
| F004 | Starting from a suggested starting goal position gives that suggestion | @F004 @AC-002 @FR-007 @US1-AS2 | passed | 00:00:00.1792924 |
| F005 | Progress is given at any state, as the goal's kind of target counts it(n: "03", settings: "Reduce, count 1 `sweet`, 1 time per day", records: "1 for 2026-11-18 and 1 for 2026-11-18", progress: "progress total 2 of at most 1", __pickleIndex: "169", exampleTags: []) | @F005 @AC-044 @FR-014 @US5-AS2 | passed | 00:00:00.1019485 |
| F002 | Another registered user cannot reach a custom habit(n: "03", action: "deletes", __pickleIndex: "206", exampleTags: []) | @F002 @AC-053 @FR-021 @FR-022 @US4-AS7 | passed | 00:00:00.1657008 |
| F004 | A fixed-days goal starting on an unscheduled day begins at its next scheduled day | @F004 @AC-045 @FR-003 @FR-013 | passed | 00:00:00.0958293 |
| F004 | "Only habits I track" returns the Published habits with the user's goals, with the other filters(n: "03", filter: "the category Body", result: "no habit", __pickleIndex: "193", exampleTags: []) | @F004 @AC-053 @FR-031 @US6-AS2 | passed | 00:00:00.1984390 |
| F005 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit(n: "05", settings: "count 0 `sweets`, fixed days on all seven days", records: "nothing", state: "Missed, no record", __pickleIndex: "97", exampleTags: []) | @F005 @AC-022 @FR-013 @US2-AS12 | passed | 00:00:00.1054174 |
| F007 | An anonymous visitor and an administrator are refused by every use case of this slice(n: "03", actor: "the anonymous visitor", useCase: "the goals overview", result: "\"You are not signed in.\"", __pickleIndex: "67", exampleTags: []) | @F007 @AC-041 @FR-015 @FR-007 @US7-AS3 @US7-AS4 | passed | 00:00:00.1621272 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "10", settings: "Reduce, duration 30 minutes, fixed days on all sev"···, records: "30 for 2026-11-18", state: "Met", progress: "progress 30 of 30", __pickleIndex: "246", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0989203 |
| F004 | A goal without a name of its own is named after its habit and target(n: "02", name: "the name \"Morning pages\"", goalName: "\"Morning pages\"", own: "its own", __pickleIndex: "83", exampleTags: []) | @F004 @AC-016 @FR-009 @FR-010 @US1-AS9 | passed | 00:00:00.1925558 |
| F007 | A per-week or per-day result counts only from the end of its grace period(n: "01", settings: "Build, yes/no, 1 time per week", created: "2026-11-02T08:00:00Z", start: "2026-11-02", records: "nothing", read: "2026-11-10T23:59:59Z", consistency: "\"N/A\"", current: "\"0 weeks\"", first: "2026-11-02 – 2026-11-08 Pending", __pickleIndex: "112", exampleTags: []) | @F007 @AC-062 @FR-001 @FR-003 @FR-008 | passed | 00:00:00.1091596 |
| F001 | Five failed sign-ins in a row block however far apart they are | @F001 @AC-021 @FR-008 | passed | 00:00:00.2461197 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "04", settings: "Build, yes/no, fixed days on all seven days", targetText: "\"daily\"", __pickleIndex: "90", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0941690 |
| F004 | Each field refusal of goal creation, with its message(n: "07", settings: "Build, count 1000 `pages`, 3 times per week", message: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "14", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1584260 |
| F005 | The anonymous visitor gets "You are not signed in." from every use case of this slice(n: "01", useCase: "record", __pickleIndex: "201", exampleTags: []) | @F005 @AC-050 @FR-018 @US6-AS4 | passed | 00:00:00.0938138 |
| F004 | Each field refusal of goal creation, with its message(n: "03", settings: "Build, yes/no with the amount 5, fixed days Monday", message: "\"A yes/no goal has no amount or unit.\"", __pickleIndex: "10", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.2069339 |
| F005 | Deleting the only record of a commitment keeps its note(n: "01", time: "`2026-11-18T12:00:00Z`", result: "deleting succeeds with the state Due, not a correc"···, state: "Due", __pickleIndex: "277", exampleTags: []) | @F005 @AC-070 @FR-007 @US4-AS8 | passed | 00:00:00.1082613 |
| F002 | Names and descriptions are trimmed before they are checked and stored(n: "03", name: "`\"   \"` + `A070 03 ` followed by `x` up to 60 ch"···, description: "none", storedName: "the 60-character name", storedDescription: "no description", __pickleIndex: "252", exampleTags: []) | @F002 @AC-070 @FR-004 @FR-005 | passed | 00:00:00.0794752 |
| F002 | Suggested starting goals are checked in list order, and each by its first failing field(n: "01", goals: "(1) no Build or Reduce, no measurement, no schedul"···, result: "\"Choose Build or Reduce.\"", position: "1", __pickleIndex: "154", exampleTags: []) | @F002 @AC-041 @FR-007 | passed | 00:00:00.0830825 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "24", settings: "Reduce, duration 30 minutes, 1 time per day", records: "20 and 9 for 2026-11-18", state: "Met", progress: "progress total 29 of at most 30", __pickleIndex: "260", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0960814 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "01", second: "no Build or Reduce, yes/no, 1 time per day", result: "\"Choose Build or Reduce.\"", __pickleIndex: "116", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0961649 |
| F002 | Only the 24 habit icons are accepted(n: "03", icon: "`smartphone`", result: "success", __pickleIndex: "166", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0922085 |
| F007 | Consistency counts Met against Met, Failed and Missed final results | @F007 @AC-001 @FR-001 @FR-002 @FR-007 @US1-AS1 | passed | 00:00:00.1636220 |
| F005 | An evaluation period takes at most 50 records, but its records can still be changed or deleted(n: "01", action: "records Yes for 2026-11-18", result: "the result is \"This evaluation period already has"···, __pickleIndex: "125", exampleTags: []) | @F005 @AC-033 @FR-003 @FR-005 | passed | 00:00:00.2223309 |
| F004 | Each field refusal of goal creation, with its message(n: "02", settings: "Build, no measurement, 3 times per week", message: "\"Choose how it is measured.\"", __pickleIndex: "9", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1659306 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "10", browserTimeZone: "the browser time zone `europe/kyiv`", timeZone: "`UTC`", __pickleIndex: "231", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0873236 |
| F005 | Demo history loading takes a demo password only with at least 8 characters and a digit(n: "01", demoPassword: "`exampl1`", why: "7 characters, with a digit", result: "its result is \"demo password does not meet the pa"···, __pickleIndex: "273", exampleTags: []) | @F005 @AC-069 @FR-030 @US7-AS6 | passed | 00:00:00.3830589 |
| F004 | A custom habit is looked up only among the acting user's custom habits(n: "03", action: "starts a new goal from", habit: "the identifier of `A4.033 03 Reading`", __pickleIndex: "143", exampleTags: []) | @F004 @AC-033 @FR-008 @US3-AS5 | passed | 00:00:00.2600831 |
| F004 | A second change replaces a version not yet begun, and follows a version already in effect(n: "02", schedule: "1 time per day", startDate: "2026-11-16", from: "2026-11-18", to: "2026-11-20", versions: "the time zone `Asia/Tokyo` from 2026-11-20 and no "···, entries: "2026-11-18 from `2026-11-17T22:00:00Z` to `2026-11"···, __pickleIndex: "269", exampleTags: []) | @F004 @AC-081 @FR-026 @US10-AS10 | passed | 00:00:00.0989937 |
| F004 | A goal on an Unpublished habit keeps working, and no new goal can use that habit(n: "05", actingUser: "`f4.ac060.other.05@example.com`", action: "creates a goal on `A4.060 05 Reading` with Build, "···, result: "the result is \"This habit is not in the catalog.\"···, __pickleIndex: "212", exampleTags: []) | @F004 @AC-060 @FR-016 @FR-034 @US7-AS6 | passed | 00:00:00.2669354 |
| F007 | Every streak carries the unit of its goal, singular for 1(n: "09", settings: "Build, yes/no, fixed days on all seven days", created: "2026-11-01T08:00:00Z", start: "2026-11-01", yes: "no date", read: "2026-11-06T10:00:00Z", streak: "\"0 commitments\"", number: "0", unit: "\"commitments\"", __pickleIndex: "31", exampleTags: []) | @F007 @AC-014 @FR-004 @US2-AS6 | passed | 00:00:00.1059771 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "01", browserTimeZone: "the browser time zone `Europe/Kyiv`", timeZone: "`Europe/Kyiv`", __pickleIndex: "222", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0875790 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"sixth.admin@example.com\"", password: "\"\"", __pickleIndex: "69", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0039070 |
| F004 | After a move east, a commitment in progress is still the next one(n: "03", time: "`2026-11-19T05:00:00Z`", next: "2026-11-20", __pickleIndex: "265", exampleTags: []) | @F004 @AC-078 @FR-014 @FR-026 | passed | 00:00:00.1085395 |
| F005 | The history gives every commitment of the range in date order, each with its state at the supplied time | @F005 @AC-042 @FR-009 @FR-010 @US5-AS1 | passed | 00:00:00.1066803 |
| F004 | Demo loading gives each created demo account its demo goals | @F004 @AC-064 @FR-040 @US9-AS1 | passed | 00:00:00.5217005 |
| F004 | A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "05", schedule: "1 time per week", date: "2027-03-28", entry: "2027-03-22 to 2027-03-28 from `2027-03-21T22:00:00"···, __pickleIndex: "168", exampleTags: []) | @F004 @AC-040 @FR-012 @FR-013 @US4-AS5 | passed | 00:00:00.0984715 |
| F007 | Without a valid demo password demo report loading creates nothing(n: "01", password: "no demo password", result: "\"no demo password configured\"", __pickleIndex: "90", exampleTags: []) | @F007 @AC-052 @FR-032 @US8-AS10 | passed | 00:00:00.4572576 |
| F007 | The overview puts goals with a Pending result first, then those with a Due result, each group in name and creation order | @F007 @AC-030 @FR-011 @US5-AS2 | passed | 00:00:00.1393062 |
| F007 | Another user's goal does not exist for the goal report(n: "02", goal: "the identifier `3f2b8c1e-0000-4000-8000-0000000000"···, __pickleIndex: "63", exampleTags: []) | @F007 @AC-039 @FR-014 @FR-007 @US7-AS1 | passed | 00:00:00.1969297 |
| F007 | A goal that has not started has streaks of 0, no recent results and no trend(n: "03", settings: "Build, yes/no, fixed days on all seven days", start: "2026-11-25", streak: "\"0 commitments\"", __pickleIndex: "34", exampleTags: []) | @F007 @AC-015 @FR-004 @FR-005 @FR-006 @US2-AS7 | passed | 00:00:00.1005387 |
| F007 | Every streak carries the unit of its goal, singular for 1(n: "04", settings: "Build, yes/no, 1 time per week", created: "2026-10-26T08:00:00Z", start: "2026-10-26", yes: "2026-11-02", read: "2026-11-11T10:00:00Z", streak: "\"1 week\"", number: "1", unit: "\"week\"", __pickleIndex: "26", exampleTags: []) | @F007 @AC-014 @FR-004 @US2-AS6 | passed | 00:00:00.1073680 |
| F004 | A goal without a name of its own is named after its habit and target(n: "03", name: "the name \"  Morning pages  \"", goalName: "\"Morning pages\"", own: "its own", __pickleIndex: "84", exampleTags: []) | @F004 @AC-016 @FR-009 @FR-010 @US1-AS9 | passed | 00:00:00.1714212 |
| F005 | Another registered user gets "This goal does not exist." from every use case of this slice(n: "09", useCase: "history", goal: "a goal identifier that belongs to no goal", __pickleIndex: "191", exampleTags: []) | @F005 @AC-047 @FR-017 @FR-018 @US6-AS1 | passed | 00:00:00.1880139 |
| F004 | Reading a goal gives the version of its current date and the one later version(n: "02", schedule: "fixed days Monday, Wednesday and Friday", startDate: "2026-11-16", time: "`2026-11-19T04:59:59Z`", versions: "the time zone `Europe/Kyiv` from 2026-11-16 and a "···, __pickleIndex: "244", exampleTags: []) | @F004 @AC-073 @FR-016 @FR-026 @US10-AS6 | passed | 00:00:00.1046757 |
| F002 | Only the 24 habit icons are accepted(n: "14", icon: "`languages`", result: "success", __pickleIndex: "177", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0971869 |
| F004 | Every suggested starting goal of the seed catalog can be created unchanged | @F004 @AC-020 @FR-007 | passed | 00:00:00.5279665 |
| F001 | A blocked account still fails with the correct password 4 minutes 59 seconds after the fifth failure | @F001 @AC-015 @FR-007 @FR-008 @US2-AS4 | passed | 00:00:00.2559066 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "01", action: "lists custom habits", __pickleIndex: "207", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1655275 |
| F001 | The end of a block resets the count of failed sign-ins | @F001 @AC-018 @FR-008 @US2-AS7 | passed | 00:00:00.4471848 |
| F002 | Names and descriptions are trimmed before they are checked and stored(n: "04", name: "`\"A070 04 Long\"`", description: "`\"  \"` + a text of 500 characters + `\"  \"` (50"···, storedName: "`A070 04 Long`", storedDescription: "the 500-character description", __pickleIndex: "253", exampleTags: []) | @F002 @AC-070 @FR-004 @FR-005 | passed | 00:00:00.0958787 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@example.com.\"", whyItIsNotValid: "empty domain part after two non-empty ones", __pickleIndex: "15", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0003158 |
| F002 | Demo loading loads the seed catalog into an empty catalog | @F002 @AC-060 @FR-030 @US6-AS1 | passed | 00:00:00.4759851 |
| F007 | demo5 has one result Missed this week on every weekday(n: "01", run: "2026-11-16T10:00:00Z", date: "2026-11-13", __pickleIndex: "92", exampleTags: []) | @F007 @AC-053 @FR-032 @FR-012 @US8-AS11 | passed | 00:00:00.5883583 |
| F005 | A final per-week Build count period counts only the records that reach the amount(n: "04", records: "20 for 2026-11-16, 20 for 2026-11-17 and 20 for 20"···, state: "Met", progress: "progress 3 of 3 times", __pickleIndex: "73", exampleTags: []) | @F005 @AC-018 @FR-014 @US2-AS8 | passed | 00:00:00.1101064 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "09", settings: "Build, duration 30 minutes, fixed days on all seve"···, targetText: "\"30 min daily\"", __pickleIndex: "95", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.1020773 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "23", settings: "Reduce, duration 30 minutes, fixed days on all sev"···, records: "29 for 2026-11-18", state: "Met", progress: "progress 29 of 30", __pickleIndex: "259", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1003731 |
| F007 | The recent results count each state, and the figures count every result of the attempt | @F007 @AC-022 @FR-005 @FR-001 @FR-003 @US4-AS2 | passed | 00:00:00.1987926 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "04", actingUser: "the registered user `ac016.user.04@example.com`", habit: "`A016 04 Draft habit`", __pickleIndex: "47", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1747989 |
| F004 | At registration, the name is checked after slice 1's checks | @F004 @AC-094 @FR-028 | passed | 00:00:00.0503037 |
| F004 | "Only habits I track" returns the Published habits with the user's goals, with the other filters(n: "02", filter: "the category Health", result: "`A4.053 02 Sugar` only", __pickleIndex: "192", exampleTags: []) | @F004 @AC-053 @FR-031 @US6-AS2 | passed | 00:00:00.1931310 |
| F004 | An anonymous visitor gets no goal use case(n: "05", action: "reads the evaluation calendar of `f4.ac032.user.05"···, __pickleIndex: "139", exampleTags: []) | @F004 @AC-032 @FR-022 @US3-AS4 | passed | 00:00:00.1796779 |
| F005 | Any begun date of a week names that week's one note(n: "03", action: "saves the note \"Week note.\" for 2026-11-20", result: "the result is \"You cannot record a day that has n"···, __pickleIndex: "154", exampleTags: []) | @F005 @AC-039 @FR-002 @FR-007 @US4-AS5 | passed | 00:00:00.0944471 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "04", second: "Build, yes/no with the unit `pages`, 1 time per da"···, result: "\"A yes/no suggested starting goal has no amount o"···, __pickleIndex: "119", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0885419 |
| F004 | Each field refusal of goal creation, with its message(n: "18", settings: "Build, yes/no, no number of times, per week", message: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "25", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1608274 |
| F005 | What can be recorded now is every Due and every Pending commitment or period, newest first(n: "02", schedule: "1 time per day", startDate: "2026-11-16", time: "`2026-11-19T10:00:00Z`", entries: "in this order: 2026-11-19, Due; 2026-11-18, Pendin"···, __pickleIndex: "175", exampleTags: []) | @F005 @AC-045 @FR-011 @US5-AS3 | passed | 00:00:00.1280175 |
| F001 | Registration refuses an email that is not valid(email: "\"ana.example.com\"", whyItIsNotValid: "no \"@\"", __pickleIndex: "7", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002874 |
| F001 | Administrator creation keeps the email in its trimmed, lower-case form | @F001 @AC-062 @FR-003 @FR-012 @FR-014 | passed | 00:00:00.1000607 |
| F007 | A per-week or per-day result counts only from the end of its grace period(n: "05", settings: "Reduce, count 1 `sweets`, 1 time per day", created: "2026-11-01T08:00:00Z", start: "2026-11-01", records: "0 for 2026-11-01", read: "2026-11-04T00:00:00Z", consistency: "\"100% (1 of 1)\"", current: "\"1 day\"", first: "2026-11-01 Met", __pickleIndex: "116", exampleTags: []) | @F007 @AC-062 @FR-001 @FR-003 @FR-008 | passed | 00:00:00.1100566 |
| F004 | An administrator gets no goal use case(n: "03", action: "lists their goals", __pickleIndex: "131", exampleTags: []) | @F004 @AC-031 @FR-021 @US3-AS3 | passed | 00:00:00.2119396 |
| F005 | Another registered user gets "This goal does not exist." from every use case of this slice(n: "05", useCase: "history", goal: "the goal of `f5.ac047.owner.<n>@example.com`", __pickleIndex: "187", exampleTags: []) | @F005 @AC-047 @FR-017 @FR-018 @US6-AS1 | passed | 00:00:00.1831346 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "04", fields: "Build, yes/no, 1 time per week", text: "\"once a week\"", kind: "\"Yes/no · flexible\"", __pickleIndex: "31", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0954961 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "01", value: "`Mars/Olympus`", __pickleIndex: "249", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.1015756 |
| F002 | A Reduce amount of 0 is accepted and shown as "at most 0" | @F002 @AC-038 @FR-007 @FR-017 @US3-AS17 | passed | 00:00:00.1051039 |
| F005 | A day that has not begun on the goal's timeline cannot be recorded(n: "01", timeZone: "UTC", schedule: "fixed days on all seven days", time: "`2026-11-18T10:00:00Z`", date: "2026-11-19", result: "the result is \"You cannot record a day that has n"···, __pickleIndex: "9", exampleTags: []) | @F005 @AC-006 @FR-002 @US1-AS6 | passed | 00:00:00.1073009 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "05", action: "creates the custom habit `Admin habit`", __pickleIndex: "211", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1728602 |
| F005 | Recording again for a commitment replaces its one record | @F005 @AC-002 @FR-003 @US1-AS2 | passed | 00:00:00.1502352 |
| F001 | Administrator creation creates an administrator | @F001 @AC-039 @FR-014 @US4-AS1 | passed | 00:00:00.0897853 |
| F001 | Registration refuses a password that breaks the password rule(email: "\"no.digit@example.com\"", password: "\"walkingfar\"", whyItBreaksTheRule: "10 characters, no digit", __pickleIndex: "3", exampleTags: []) | @F001 @AC-002 @FR-002 @US1-AS3 @US1-AS4 | passed | 00:00:00.0370915 |
| F002 | Only the 24 habit icons are accepted(n: "04", icon: "`footprints`", result: "success", __pickleIndex: "167", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0880868 |
| F002 | Only the 24 habit icons are accepted(n: "13", icon: "`pen-line`", result: "success", __pickleIndex: "176", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0885288 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "08", name: "`A027 08 Menu icon`", description: "none", category: "Body", intent: "Build", icon: "`menu`", result: "\"Choose an icon.\"", __pickleIndex: "77", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0889666 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "05", measurement: "yes/no", before: "Yes", change: "the record of 2026-11-18 to no value", result: "the result is \"Choose Yes or No.\" and the histor"···, __pickleIndex: "229", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.0980819 |
| F005 | A value that does not fit the goal's measurement is refused with its message(n: "03", measurement: "count 5 `pages`", value: "-1", message: "\"Enter a whole number from 0 to 9999.\"", __pickleIndex: "19", exampleTags: []) | @F005 @AC-007 @FR-005 @US1-AS7 | passed | 00:00:00.0983104 |
| F007 | demo4's dashboard has Pending and Due results, no Missed one, and its notes | @F007 @AC-045 @FR-030 @FR-012 @US8-AS4 | passed | 00:00:00.5795765 |
| F002 | For custom habits the acting user and ownership come first, the name check last(n: "03", actingUser: "the registered user `ac057.b.03@example.com`", action: "edits an unknown identifier with the name `\"   \""···, result: "\"This custom habit does not exist.\"", __pickleIndex: "228", exampleTags: []) | @F002 @AC-057 @FR-005 @FR-016 @US4-AS11 | passed | 00:00:00.1599173 |
| F001 | An administrator reads their own account details | @F001 @AC-028 @FR-009 @FR-012 @FR-015 @US3-AS3 | passed | 00:00:00.0861310 |
| F002 | For custom habits the acting user and ownership come first, the name check last(n: "02", actingUser: "the registered user `ac057.b.02@example.com`", action: "edits `Secret` of `ac057.a.02@example.com` with th"···, result: "\"This custom habit does not exist.\"", __pickleIndex: "227", exampleTags: []) | @F002 @AC-057 @FR-005 @FR-016 @US4-AS11 | passed | 00:00:00.1587849 |
| F007 | Demo loading and demo history loading are unchanged, and demo1 to demo3 have no final result | @F007 @AC-046 @FR-031 @US8-AS5 | passed | 00:00:00.6955684 |
| F002 | Custom-habit names and descriptions are trimmed before they are checked and stored(n: "03", name: "`\"   \"` + `Habit ` followed by `x` up to 60 char"···, description: "none", storedName: "the 60-character name", storedDescription: "no description", __pickleIndex: "258", exampleTags: []) | @F002 @AC-073 @FR-004 @FR-005 | passed | 00:00:00.0874050 |
| F002 | Browsing by intent returns only that intent(n: "02", intent: "Build", sugar: "does not contain", water: "contains", __pickleIndex: "6", exampleTags: []) | @F002 @AC-004 @FR-009 @US1-AS4 | passed | 00:00:00.0877757 |
| F004 | The next commitment is the first commitment that has not ended(n: "03", time: "`2026-11-18T23:59:59Z`", next: "2026-11-18", __pickleIndex: "116", exampleTags: []) | @F004 @AC-023 @FR-014 @US2-AS2 | passed | 00:00:00.0860147 |
| F004 | Each field refusal of goal creation, with its message(n: "19", settings: "Build, yes/no, 0 times per week", message: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "26", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1599659 |
| F002 | A habit without suggested starting goals is shown without one | @F002 @AC-014 @FR-008 @FR-010 @US2-AS2 | passed | 00:00:00.0873101 |
| F004 | After a move east, a commitment in progress is still the next one(n: "01", time: "`2026-11-18T16:00:00Z`", next: "2026-11-18", __pickleIndex: "263", exampleTags: []) | @F004 @AC-078 @FR-014 @FR-026 | passed | 00:00:00.1025272 |
| F004 | A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "01", schedule: "fixed days Sunday", date: "2027-03-28", entry: "2027-03-28 from `2027-03-27T22:00:00Z` to `2027-03"···, __pickleIndex: "164", exampleTags: []) | @F004 @AC-040 @FR-012 @FR-013 @US4-AS5 | passed | 00:00:00.0940309 |
| F001 | A password change to a password that breaks the password rule is refused(email: "\"weak.new7@example.com\"", newPassword: "\"runnin4\"", whyItBreaksTheRule: "7 characters, with a digit", __pickleIndex: "53", exampleTags: []) | @F001 @AC-034 @FR-002 @FR-013 @US6-AS3 | passed | 00:00:00.1751654 |
| F005 | A record identifier that is not a record of the goal named in the call does not exist(n: "04", action: "deletes a record identifier that belongs to no rec"···, __pickleIndex: "124", exampleTags: []) | @F005 @AC-032 @FR-005 @FR-017 @US3-AS7 | passed | 00:00:00.1046738 |
| F007 | Reduce results count like Build results(n: "01", settings: "Reduce, count 1 `sweets`, 1 time per day", nov1: "0", nov2: "3", __pickleIndex: "9", exampleTags: []) | @F007 @AC-006 @FR-001 @US1-AS5 | passed | 00:00:00.1054036 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "05", browserTimeZone: "the browser time zone `Etc/GMT+3`", timeZone: "`UTC`", __pickleIndex: "226", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0808784 |
| F005 | A commitment is Upcoming before its start instant and Due from it(n: "02", time: "`2026-11-18T23:59:59Z`", first: "Due, grace ends `2026-11-21T00:00:00Z`", second: "Upcoming, no grace end", __pickleIndex: "42", exampleTags: []) | @F005 @AC-012 @FR-010 @US2-AS1 | passed | 00:00:00.0946623 |
| F001 | Administrator creation checks for a missing password before it looks for an existing administrator | @F001 @AC-051 @FR-014 | passed | 00:00:00.1137670 |
| F005 | A value that does not fit the goal's measurement is refused with its message(n: "01", measurement: "yes/no", value: "no value", message: "\"Choose Yes or No.\"", __pickleIndex: "17", exampleTags: []) | @F005 @AC-007 @FR-005 @US1-AS7 | passed | 00:00:00.0919894 |
| F004 | Use cases check the acting user, the role, the identifier and the fields, in this order(n: "04", actingUser: "the anonymous visitor", action: "creates a goal on `A4.035 04 Draft` with no Build "···, message: "\"You are not signed in.\"", __pickleIndex: "150", exampleTags: []) | @F004 @AC-035 @FR-023 | passed | 00:00:00.1763502 |
| F007 | demo5 has one result Missed this week on every weekday(n: "05", run: "2026-11-20T10:00:00Z", date: "2026-11-17", __pickleIndex: "96", exampleTags: []) | @F007 @AC-053 @FR-032 @FR-012 @US8-AS11 | passed | 00:00:00.5975401 |
| F004 | An anonymous visitor gets no goal use case(n: "02", action: "creates a goal on `A4.032 02 Reading` with Build, "···, __pickleIndex: "136", exampleTags: []) | @F004 @AC-032 @FR-022 @US3-AS4 | passed | 00:00:00.1654400 |
| F005 | An ended commitment or period stays Pending during its grace period, even when its records reach the target(n: "04", settings: "duration 20 minutes, fixed days on all seven days", value: "12", time: "`2026-11-20T23:59:59Z`", entry: "Pending, grace ends `2026-11-21T00:00:00Z`, progre"···, __pickleIndex: "89", exampleTags: []) | @F005 @AC-021 @FR-012 @US2-AS11 | passed | 00:00:00.1090390 |
| F005 | A day that has not begun on the goal's timeline cannot be recorded(n: "03", timeZone: "UTC", schedule: "fixed days Monday, Wednesday and Friday", time: "`2026-11-18T10:00:00Z`", date: "2026-11-19", result: "the result is \"Nothing is scheduled on this date."···, __pickleIndex: "11", exampleTags: []) | @F005 @AC-006 @FR-002 @US1-AS6 | passed | 00:00:00.0892257 |
| F007 | Around a clock change back, a grace period ends at 23:00 local time and a 25-hour day counts once(n: "03", read: "2026-10-27T22:59:59Z", consistency: "\"80% (4 of 5)\"", current: "\"0 commitments\"", date: "2026-10-25", state: "Pending", __pickleIndex: "104", exampleTags: []) | @F007 @AC-057 @FR-001 @FR-003 @FR-008 | passed | 00:00:00.1269607 |
| F002 | For custom habits the acting user and ownership come first, the name check last(n: "01", actingUser: "the anonymous visitor", action: "creates a custom habit with the name `\"   \"`", result: "\"You are not signed in.\"", __pickleIndex: "226", exampleTags: []) | @F002 @AC-057 @FR-005 @FR-016 @US4-AS11 | passed | 00:00:00.1588316 |
| F005 | Deleting the only record of a commitment keeps its note(n: "02", time: "`2026-11-22T10:00:00Z`", result: "deleting succeeds with the state Missed, a correct"···, state: "Missed", __pickleIndex: "278", exampleTags: []) | @F005 @AC-070 @FR-007 @US4-AS8 | passed | 00:00:00.1039997 |
| F005 | Progress is given at any state, as the goal's kind of target counts it(n: "02", settings: "Build, count 20 `pages`, 3 times per week", records: "20 for 2026-11-16, 25 for 2026-11-17 and 12 for 20"···, progress: "progress 2 of 3 times", __pickleIndex: "168", exampleTags: []) | @F005 @AC-044 @FR-014 @US5-AS2 | passed | 00:00:00.1028646 |
| F004 | A habit's details give the user's goal count and their goals on it, in name order | @F004 @AC-054 @FR-031 @US6-AS3 | passed | 00:00:00.2136662 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "06", value: "`+02:00`", __pickleIndex: "254", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.0932000 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "16", fields: "Reduce, count 5 `cups`, 2 times per week", text: "\"at most 5 cups, 2 times a week\"", kind: "\"Count · flexible\"", __pickleIndex: "43", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0949116 |
| F001 | Administrator creation finds a registered user before it checks the password rule | @F001 @AC-050 @FR-014 | passed | 00:00:00.0971551 |
| F001 | Failed sign-ins for an email that has no account block nothing | @F001 @AC-025 @FR-007 @FR-008 | passed | 00:00:00.0890131 |
| F004 | An administrator gets no goal use case(n: "01", action: "starts a new goal from `A4.031 01 Reading`", __pickleIndex: "129", exampleTags: []) | @F004 @AC-031 @FR-021 @US3-AS3 | passed | 00:00:00.1855674 |
| F004 | Each field refusal of goal creation, with its message(n: "10", settings: "Reduce, count 1000 `cups`, 1 time per day", message: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "17", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1637480 |
| F002 | The anonymous visitor cannot manage the catalog(n: "05", action: "publishes `A031 05 Published`", __pickleIndex: "98", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0856437 |
| F004 | A custom habit is looked up only among the acting user's custom habits(n: "02", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "`f4.ac033.b.02@example.com`'s custom habit `Cold s"···, __pickleIndex: "142", exampleTags: []) | @F004 @AC-033 @FR-008 @US3-AS5 | passed | 00:00:00.2540172 |
| F002 | Only the 24 habit icons are accepted(n: "18", icon: "`cigarette-off`", result: "success", __pickleIndex: "181", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0890238 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "31", settings: "Build, count 5 `pages`, 2 times per day", records: "10 for 2026-11-18", state: "Failed", progress: "progress 1 of 2 times", __pickleIndex: "267", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0942352 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "07", actingUser: "the administrator `ac016.admin.07@example.com`", habit: "`A016 07 Draft habit`", __pickleIndex: "50", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1731082 |
| F005 | The demo Yoga goal's weeks when R is a Sunday or a Tuesday(n: "01", runTime: "`2026-11-22T10:00:00Z`", r: "Sunday 2026-11-22", startDate: "2026-10-25", lastDate: "2026-11-22", entries: "4 entries: 2026-10-26 to 2026-11-01, Missed, no re"···, __pickleIndex: "271", exampleTags: []) | @F005 @AC-068 @FR-030 @US7-AS2 | passed | 00:00:00.4763766 |
| F001 | Administrator creation checks for a missing password before the email | @F001 @AC-048 @FR-014 | passed | 00:00:00.0009323 |
| F002 | Only the 24 habit icons are accepted(n: "15", icon: "`music`", result: "success", __pickleIndex: "178", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0852310 |
| F001 | Administrator creation leaves an existing administrator unchanged | @F001 @AC-041 @FR-014 @US4-AS2 | passed | 00:00:00.1255633 |
| F005 | A final Reduce count commitment is Met at or below its limit and Failed above it(n: "02", count: "count 2 `cups`", value: "1", state: "Met", __pickleIndex: "57", exampleTags: []) | @F005 @AC-015 @FR-014 @US2-AS5 | passed | 00:00:00.1022909 |
| F005 | A final per-day yes/no period counts its Yes records(n: "04", settings: "Build, yes/no, 2 times per day", records: "Yes for 2026-11-18 and Yes for 2026-11-18", state: "Met", progress: "progress 2 of 2 times", __pickleIndex: "82", exampleTags: []) | @F005 @AC-020 @FR-014 @US2-AS10 | passed | 00:00:00.1081987 |
| F001 | Demo loading with a password that breaks the password rule creates no account(demoPassword: "\"exampl1\"", whyItBreaksTheRule: "7 characters, with a digit", __pickleIndex: "88", exampleTags: []) | @F001 @AC-056 @FR-002 @FR-016 @US7-AS4 | passed | 00:00:00.3403637 |
| F004 | An administrator gets no goal use case(n: "05", action: "reads the evaluation calendar of `f4.ac031.user.05"···, __pickleIndex: "133", exampleTags: []) | @F004 @AC-031 @FR-021 @US3-AS3 | passed | 00:00:00.1982541 |
| F002 | The Home preview is the first four Published habits in name order | @F002 @AC-012 @FR-008 @FR-024 @US5-AS2 | passed | 00:00:00.3947389 |
| F002 | Publish and unpublish check the acting user and the role before the identifier(n: "04", actingUser: "the anonymous visitor", action: "unpublishes", result: "\"You are not signed in.\"", __pickleIndex: "196", exampleTags: []) | @F002 @AC-046 @FR-016 | passed | 00:00:00.0944470 |
| F004 | Use cases check the acting user, the role, the identifier and the fields, in this order(n: "05", actingUser: "`f4.ac035.admin.05@example.com`", action: "creates a goal on `A4.035 05 Draft` with no Build "···, message: "\"Goals are for registered users.\"", __pickleIndex: "151", exampleTags: []) | @F004 @AC-035 @FR-023 | passed | 00:00:00.1650436 |
| F002 | Details with a custom habit's identifier give "not in the catalog"(n: "02", actingUser: "the administrator `ac017.admin.02@example.com`", __pickleIndex: "54", exampleTags: []) | @F002 @AC-017 @FR-010 @FR-024 @US2-AS5 | passed | 00:00:00.1678638 |
| F002 | Browsing returns only Published habits, with their card content, to every audience(n: "01", actingUser: "the anonymous visitor", __pickleIndex: "0", exampleTags: []) | @F002 @AC-001 @FR-008 @US1-AS1 | passed | 00:00:00.1852096 |
| F005 | Recording checks the acting user, the goal, the date, the value and the note in that order(n: "06", actor: "`f5.ac010.owner.<n>@example.com`", date: "2026-11-18", value: "no value", note: "a text of 501 characters", result: "\"Enter a whole number from 0 to 9999.\"", __pickleIndex: "38", exampleTags: []) | @F005 @AC-010 @FR-005 @FR-018 @US1-AS7 | passed | 00:00:00.2833115 |
| F005 | A record identifier that is not a record of the goal named in the call does not exist(n: "01", action: "changes the second goal's record of 2026-11-18 to "···, __pickleIndex: "121", exampleTags: []) | @F005 @AC-032 @FR-005 @FR-017 @US3-AS7 | passed | 00:00:00.1991311 |
| F005 | A record added to a Missed commitment is a correction that makes it Met | @F005 @AC-024 @FR-006 @US3-AS1 | passed | 00:00:00.1139882 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "11", browserTimeZone: "the browser time zone `\"\"`", timeZone: "`UTC`", __pickleIndex: "232", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0853001 |
| F002 | A demo email that a person registered first keeps its owner's data untouched | @F002 @AC-067 @FR-031 @US6-AS8 | passed | 00:00:00.3674124 |
| F002 | A custom habit is not a catalog habit for the administrator(n: "02", action: "edits, with the name `A033 02 Taken over`, the cat"···, __pickleIndex: "105", exampleTags: []) | @F002 @AC-033 @FR-013 @FR-024 @US3-AS13 | passed | 00:00:00.1657128 |
| F007 | "Missed this week" starts on Monday at 00:00 in the account's time zone and includes the previous Friday(n: "03", zone: "`Europe/Kyiv`", read: "2026-11-15T21:59:59Z", missed: "2026-11-12", pending: "2026-11-13", __pickleIndex: "56", exampleTags: []) | @F007 @AC-034 @FR-012 @US6-AS4 | passed | 00:00:00.1153320 |
| F005 | An account that already has the demo email gets no demo goals and keeps its password | @F005 @AC-058 @FR-030 @US7-AS3 | passed | 00:00:00.4591364 |
| F002 | Field checks refuse a custom habit with the same messages(n: "05", name: "`No intent`", description: "none", category: "Body", intent: "none", icon: "`droplet`", result: "\"Choose Build or Reduce.\"", __pickleIndex: "223", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0816180 |
| F004 | Reading a goal gives the version of its current date and the one later version(n: "03", schedule: "fixed days Monday, Wednesday and Friday", startDate: "2026-11-16", time: "`2026-11-19T05:00:00Z`", versions: "the time zone `America/New_York` from 2026-11-19 a"···, __pickleIndex: "245", exampleTags: []) | @F004 @AC-073 @FR-016 @FR-026 @US10-AS6 | passed | 00:00:00.0938568 |
| F005 | A note given with a record is the note; no note leaves it unchanged(n: "03", action: "records Yes for 2026-11-18 with the note `\"\"`", note: "no note", __pickleIndex: "140", exampleTags: []) | @F005 @AC-037 @FR-007 @US4-AS3 | passed | 00:00:00.1109364 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@@example.com\"", whyItIsNotValid: "two \"@\"", __pickleIndex: "17", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002662 |
| F005 | A date that belongs to no commitment or period cannot be recorded(n: "03", schedule: "1 time per day", createdAt: "`2026-11-18T10:00:00Z`", startDate: "2026-11-18", date: "2026-11-17", why: "before the start date", __pickleIndex: "6", exampleTags: []) | @F005 @AC-005 @FR-002 @US1-AS5 | passed | 00:00:00.0915491 |
| F002 | Field checks refuse a custom habit with the same messages(n: "07", name: "`Menu icon`", description: "none", category: "Body", intent: "Build", icon: "`menu`", result: "\"Choose an icon.\"", __pickleIndex: "225", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0784914 |
| F004 | A Reduce goal with an amount has 1 time per period(n: "01", settings: "Reduce, count 2 `cups`, 1 time per week", result: "creating succeeds", __pickleIndex: "60", exampleTags: []) | @F004 @AC-012 @FR-004 @FR-006 | passed | 00:00:00.1808846 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "06", goal: "on `A4.008 06 Reading` with Build, count 20 with n"···, message: "\"Enter a unit, for example pages.\"", __pickleIndex: "35", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1882456 |
| F005 | What can be recorded now is every Due and every Pending commitment or period, newest first(n: "01", schedule: "fixed days on all seven days", startDate: "2026-11-16", time: "`2026-11-19T10:00:00Z`", entries: "in this order: 2026-11-19, Due, grace ends `2026-1"···, __pickleIndex: "174", exampleTags: []) | @F005 @AC-045 @FR-011 @US5-AS3 | passed | 00:00:00.1051913 |
| F002 | Only the 24 habit icons are accepted(n: "22", icon: "`sprout`", result: "success", __pickleIndex: "185", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0817913 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "03", state: "Unpublished", action: "creates a catalog habit", __pickleIndex: "66", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | passed | 00:00:00.0843168 |
| F004 | Unpublishing a habit that goals use needs confirmation and gives only the number of goals(n: "02", goals: "`f4.ac057.a.02@example.com` created one goal", count: "1", text: "\"1 goal uses this habit.\"", __pickleIndex: "203", exampleTags: []) | @F004 @AC-057 @FR-033 @US7-AS1 @US7-AS3 @US7-AS5 | passed | 00:00:00.2857229 |
| F004 | Demo goals are left out when their catalog habit is not Published | @F004 @AC-066 @FR-040 @US9-AS3 | passed | 00:00:00.5841362 |
| F004 | The start date is today or up to 365 days later(n: "04", startDate: "the start date 2027-11-19", result: "\"Choose a start date within a year from today.\"", __pickleIndex: "55", exampleTags: []) | @F004 @AC-010 @FR-006 @FR-012 | passed | 00:00:00.1852441 |
| F007 | The longest streak keeps the longest run of Met results | @F007 @AC-010 @FR-003 @US2-AS2 | passed | 00:00:00.1382631 |
| F002 | The category filter counts only Published catalog habits(n: "02", actingUser: "the registered user `ac009.user.02@example.com`", __pickleIndex: "21", exampleTags: []) | @F002 @AC-009 @FR-002 @FR-009 @FR-024 @US1-AS9 | passed | 00:00:00.3829696 |
| F005 | An evaluation period takes at most 50 records, but its records can still be changed or deleted(n: "04", action: "changes the first record of 2026-11-18 to No", result: "changing succeeds with the state Due, not a correc"···, __pickleIndex: "128", exampleTags: []) | @F005 @AC-033 @FR-003 @FR-005 | passed | 00:00:00.2246746 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "03", settings: "Build, count 5 `pages`, fixed days on all seven da"···, records: "6 for 2026-11-18", state: "Met", progress: "progress 6 of 5", __pickleIndex: "239", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0922114 |
| F004 | The profile gives "member since" with the month and year of creation in the account's time zone(n: "03", time: "`2026-11-30T22:30:00Z`", browserTimeZone: "no browser time zone", memberSince: "\"member since November 2026\"", __pickleIndex: "291", exampleTags: []) | @F004 @AC-089 @FR-030 @US11-AS6 | passed | 00:00:00.0854632 |
| F001 | Demo loading run again changes nothing | @F001 @AC-059 @FR-016 @US7-AS2 | passed | 00:00:00.6796971 |
| F007 | A latest Failed or Missed result gives a current streak of 0(n: "02", nov6: "none", __pickleIndex: "18", exampleTags: []) | @F007 @AC-011 @FR-003 @US2-AS3 | passed | 00:00:00.1160533 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "01", actingUser: "the anonymous visitor", habit: "`A016 01 Draft habit`", __pickleIndex: "44", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1684550 |
| F007 | The percentage is rounded down to a whole number(n: "04", nov1: "12", nov2: "none", nov3: "0", text: "\"0% (0 of 3)\"", percentage: "0", successful: "0", __pickleIndex: "7", exampleTags: []) | @F007 @AC-004 @FR-002 @US1-AS4 | passed | 00:00:00.1052716 |
| F005 | The anonymous visitor gets "You are not signed in." from every use case of this slice(n: "07", useCase: "recent notes", __pickleIndex: "207", exampleTags: []) | @F005 @AC-050 @FR-018 @US6-AS4 | passed | 00:00:00.1124530 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "07", fields: "Build, count 5 with a unit of 20 characters, 1 tim"···, storedUnit: "that unit of 20 characters", __pickleIndex: "144", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.1006806 |
| F002 | An unknown identifier does not exist for the administrator(n: "04", action: "unpublishes", __pickleIndex: "103", exampleTags: []) | @F002 @AC-032 @FR-013 @US3-AS12 | passed | 00:00:00.0953663 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "11", settings: "Reduce, duration 30 minutes, fixed days on all sev"···, records: "31 for 2026-11-18", state: "Failed", progress: "progress 31 of 30", __pickleIndex: "247", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0982992 |
| F004 | Anonymous visitors and administrators get the catalog without goal information(n: "01", actingUser: "the anonymous visitor", action: "browses the catalog as the anonymous visitor with "···, result: "the result contains `A4.055 01 Reading` with no go"···, __pickleIndex: "195", exampleTags: []) | @F004 @AC-055 @FR-032 @US6-AS4 | passed | 00:00:00.1755894 |
| F007 | A commitment-based yes/no goal has one bar per week with its commitments as the target | @F007 @AC-025 @FR-006 @US4-AS5 | passed | 00:00:00.1128631 |
| F005 | A final per-week Build yes/no period is Met with at least N Yes records(n: "03", records: "Yes for 2026-11-16, Yes for 2026-11-17, Yes for 20"···, state: "Met", progress: "progress 4 of 3 times", __pickleIndex: "68", exampleTags: []) | @F005 @AC-017 @FR-014 @US2-AS7 | passed | 00:00:00.1550854 |
| F004 | The next commitment is the first commitment that has not ended(n: "01", time: "`2026-11-16T08:00:00Z`", next: "2026-11-16", __pickleIndex: "114", exampleTags: []) | @F004 @AC-023 @FR-014 @US2-AS2 | passed | 00:00:00.1044378 |
| F002 | Suggested starting goals are checked in list order, and each by its first failing field(n: "03", goals: "(1) Build, yes/no, 1 time per day (marked); (2) Bu"···, result: "\"Choose fixed days or how many times.\"", position: "3", __pickleIndex: "156", exampleTags: []) | @F002 @AC-041 @FR-007 | passed | 00:00:00.0927307 |
| F005 | Records on the same date of a per-day period keep the order in which they were stored | @F005 @AC-004 @FR-003 @FR-009 @FR-014 @US1-AS4 | passed | 00:00:00.0972800 |
| F002 | Only the 24 habit icons are accepted(n: "01", icon: "`book-open`", result: "success", __pickleIndex: "164", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0820268 |
| F005 | A final yes/no commitment is Met on Yes for Build and on No for Reduce(n: "04", intent: "Reduce", value: "Yes", state: "Failed", __pickleIndex: "65", exampleTags: []) | @F005 @AC-016 @FR-014 @US2-AS6 | passed | 00:00:00.1017572 |
| F005 | An evaluation period takes at most 50 records, but its records can still be changed or deleted(n: "02", action: "records Yes for 2026-11-18 with the note a text of"···, result: "the result is \"The note can have at most 500 char"···, __pickleIndex: "126", exampleTags: []) | @F005 @AC-033 @FR-003 @FR-005 | passed | 00:00:00.2126119 |
| F005 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit(n: "07", settings: "yes/no, 1 time per day", records: "No for 2026-11-18", state: "Met", __pickleIndex: "99", exampleTags: []) | @F005 @AC-022 @FR-013 @US2-AS12 | passed | 00:00:00.0973996 |
| F004 | A renamed catalog habit renames the goals based on it | @F004 @AC-062 @FR-010 @FR-016 @US7-AS7 | passed | 00:00:00.2328232 |
| F004 | After a move west, a commitment already over is not the next one(n: "01", time: "`2026-11-18T21:59:59Z`", next: "2026-11-18", __pickleIndex: "261", exampleTags: []) | @F004 @AC-077 @FR-014 @FR-026 | passed | 00:00:00.1016920 |
| F001 | Demo loading without a demo password creates no account(demoPassword: "\"   \"", __pickleIndex: "86", exampleTags: []) | @F001 @AC-055 @FR-016 @US7-AS3 | passed | 00:00:00.3403031 |
| F005 | Recording checks the acting user, the goal, the date, the value and the note in that order(n: "05", actor: "`f5.ac010.owner.<n>@example.com`", date: "2026-11-19", value: "no value", note: "a text of 501 characters", result: "\"You cannot record a day that has not begun.\"", __pickleIndex: "37", exampleTags: []) | @F005 @AC-010 @FR-005 @FR-018 @US1-AS7 | passed | 00:00:00.2669785 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "12", fields: "Build, duration 20 minutes, 1 time per day", text: "\"20 min a day\"", kind: "\"Duration · flexible\"", __pickleIndex: "39", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0883640 |
| F002 | An unknown identifier does not exist for the administrator(n: "02", action: "edits, with the name `A032 02 Name`, the category "···, __pickleIndex: "101", exampleTags: []) | @F002 @AC-032 @FR-013 @US3-AS12 | passed | 00:00:00.0898940 |
| F001 | A password change with a wrong current password is refused | @F001 @AC-033 @FR-013 @US6-AS2 | passed | 00:00:00.2097443 |
| F001 | Five failed attempts during the block do not start a new block | @F001 @AC-023 @FR-008 | passed | 00:00:00.3226689 |
| F007 | Around a clock change forward, a grace period ends at 01:00 local time and a 23-hour day counts once(n: "02", read: "2026-03-30T23:00:00Z", consistency: "\"83% (5 of 6)\"", current: "\"0 commitments\"", date: "2026-03-28", state: "Missed", __pickleIndex: "110", exampleTags: []) | @F007 @AC-061 @FR-001 @FR-003 @FR-008 | passed | 00:00:00.1699436 |
| F005 | An administrator gets "Goals are for registered users." from every use case of this slice(n: "07", useCase: "recent notes", __pickleIndex: "200", exampleTags: []) | @F005 @AC-049 @FR-018 @US6-AS3 | passed | 00:00:00.1866920 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "14", second: "Reduce, duration 1000 hours, 1 time per day", result: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "129", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0884313 |
| F002 | Only the 24 habit icons are accepted(n: "17", icon: "`utensils`", result: "success", __pickleIndex: "180", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0866108 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "06", fields: "Build, duration 999 hours, 1 time per week", storedUnit: "hours", __pickleIndex: "143", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0973299 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "11", settings: "Reduce, count 0 `cigarettes`, 1 time per day", targetText: "\"at most 0 cigarettes a day\"", __pickleIndex: "97", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0965645 |
| F001 | Demo loading accepts a demo password at the lower bound of the password rule | @F001 @AC-058 @FR-002 @FR-016 | passed | 00:00:00.3918395 |
| F005 | An administrator gets "Goals are for registered users." from every use case of this slice(n: "01", useCase: "record", __pickleIndex: "194", exampleTags: []) | @F005 @AC-049 @FR-018 @US6-AS3 | passed | 00:00:00.1840893 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "04", browserTimeZone: "the browser time zone `FLE Standard Time`", timeZone: "`UTC`", __pickleIndex: "225", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0787947 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "05", action: "deletes a custom habit with the identifier of `A05"···, __pickleIndex: "235", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.1808576 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "04", action: "deletes `Secret` by its identifier", __pickleIndex: "210", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1687098 |
| F005 | After a time zone change, each commitment keeps the instants and grace period of its own settings version | @F005 @AC-030 @FR-015 @FR-016 | passed | 00:00:00.1511279 |
| F004 | An anonymous visitor gets no Home heading, no name change and no profile(n: "02", action: "changes their name to \"Yuri\"", __pickleIndex: "294", exampleTags: []) | @F004 @AC-091 @FR-023 @FR-028 @FR-029 @FR-030 @US11-AS8 | passed | 00:00:00.0015522 |
| F007 | An anonymous visitor and an administrator are refused by every use case of this slice(n: "08", actor: "the administrator `f7.ac041.admin.08@example.com`", useCase: "the dashboard", result: "\"Goals are for registered users.\"", __pickleIndex: "72", exampleTags: []) | @F007 @AC-041 @FR-015 @FR-007 @US7-AS3 @US7-AS4 | passed | 00:00:00.1957586 |
| F005 | A per-week note gives its week's first and last date, and a goal with fewer notes gives only those | @F005 @AC-041 @FR-008 | passed | 00:00:00.1250369 |
| F004 | After a move west, a fixed-days goal keeps the old time zone up to its current date | @F004 @AC-071 @FR-011 @FR-026 @FR-027 @US10-AS4 | passed | 00:00:00.0986329 |
| F005 | The history's range has 1 to 366 days and is checked after the goal(n: "02", actor: "`f5.ac043.owner.<n>@example.com`", first: "2026-11-16", last: "2027-11-16", result: "it gives 364 entries, the first 2026-11-18 and the"···, __pickleIndex: "162", exampleTags: []) | @F005 @AC-043 @FR-009 @FR-018 @US5-AS4 | passed | 00:00:00.1789686 |
| F004 | Reading a goal gives the version of its current date and the one later version(n: "04", schedule: "fixed days Monday, Wednesday and Friday", startDate: "2026-11-16", time: "`2026-11-20T12:00:00Z`", versions: "the time zone `America/New_York` from 2026-11-19 a"···, __pickleIndex: "246", exampleTags: []) | @F004 @AC-073 @FR-016 @FR-026 @US10-AS6 | passed | 00:00:00.1035255 |
| F002 | Custom habits never appear in browsing, for any audience(n: "02", actingUser: "the registered user `ac006.other.02@example.com`", __pickleIndex: "15", exampleTags: []) | @F002 @AC-006 @FR-024 @US1-AS6 | passed | 00:00:00.2604233 |
| F002 | A user's custom-habit name is taken, ignoring case and spaces(n: "02", action: "edits `Evening walk`", __pickleIndex: "202", exampleTags: []) | @F002 @AC-051 @FR-004 @FR-005 @US4-AS5 | passed | 00:00:00.0882884 |
| F002 | Details with a custom habit's identifier give "not in the catalog"(n: "01", actingUser: "the registered user `ac017.owner.01@example.com`", __pickleIndex: "53", exampleTags: []) | @F002 @AC-017 @FR-010 @FR-024 @US2-AS5 | passed | 00:00:00.1650710 |
| F004 | "Only habits I track" returns the Published habits with the user's goals, with the other filters(n: "01", filter: "no category", result: "`A4.053 01 Reading` and `A4.053 01 Sugar`, and not"···, __pickleIndex: "191", exampleTags: []) | @F004 @AC-053 @FR-031 @US6-AS2 | passed | 00:00:00.2072099 |
| F004 | Each goal follows a time zone change from its own next day or week(n: "02", schedule: "3 times per week", startDate: "2026-11-16", from: "2026-11-16", to: "2026-11-29", entries: "2026-11-16 to 2026-11-22 from `2026-11-15T22:00:00"···, __pickleIndex: "241", exampleTags: []) | @F004 @AC-072 @FR-026 @FR-027 @US10-AS5 | passed | 00:00:00.1004477 |
| F004 | The edges of every target range are accepted(n: "02", settings: "Build, count 999 `pages`, 3 times per week", targetText: "\"999 pages, 3 times a week\"", __pickleIndex: "45", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | passed | 00:00:00.1714728 |
| F005 | A demo goal is left out when its catalog habit is not Published, and the results stay the same(n: "03", habits: "a Draft `Reading`, a Draft `Sugar` and a Draft `Yo"···, goals: "empty", __pickleIndex: "219", exampleTags: []) | @F005 @AC-060 @FR-030 | passed | 00:00:00.5067074 |
| F004 | An anonymous visitor gets no Home heading, no name change and no profile(n: "01", action: "reads the Home heading", __pickleIndex: "293", exampleTags: []) | @F004 @AC-091 @FR-023 @FR-028 @FR-029 @FR-030 @US11-AS8 | passed | 00:00:00.0003195 |
| F004 | A Reduce goal with an amount has 1 time per period(n: "03", settings: "Reduce, count 2 `cups`, 3 times per day", result: "\"A Reduce goal with an amount has 1 time per peri"···, __pickleIndex: "62", exampleTags: []) | @F004 @AC-012 @FR-004 @FR-006 | passed | 00:00:00.1696974 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "07", second: "Build, count 1000 `pages`, 1 time per day", result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "122", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0922539 |
| F007 | An anonymous visitor and an administrator are refused by every use case of this slice(n: "06", actor: "the administrator `f7.ac041.admin.06@example.com`", useCase: "the goal report for the identifier `3f2b8c1e-0000-"···, result: "\"Goals are for registered users.\"", __pickleIndex: "70", exampleTags: []) | @F007 @AC-041 @FR-015 @FR-007 @US7-AS3 @US7-AS4 | passed | 00:00:00.2052330 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"fourth.admin@example.com\"", password: "absent", __pickleIndex: "64", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0002432 |
| F007 | An anonymous visitor and an administrator are refused by every use case of this slice(n: "01", actor: "the anonymous visitor", useCase: "the goal report for the goal of `f7.ac041.owner.01"···, result: "\"You are not signed in.\"", __pickleIndex: "65", exampleTags: []) | @F007 @AC-041 @FR-015 @FR-007 @US7-AS3 @US7-AS4 | passed | 00:00:00.1868885 |
| F007 | A Pending result that becomes Missed resets the streak at its grace end | @F007 @AC-013 @FR-003 @FR-008 @US2-AS5 | passed | 00:00:00.1194464 |
| F007 | "Missed this week" starts on Monday at 00:00 in the account's time zone and includes the previous Friday(n: "02", zone: "`UTC`", read: "2026-11-16T00:00:00Z", missed: "2026-11-13", pending: "2026-11-14", __pickleIndex: "55", exampleTags: []) | @F007 @AC-034 @FR-012 @US6-AS4 | passed | 00:00:00.1185608 |
| F002 | Category, intent and search apply together | @F002 @AC-010 @FR-009 | passed | 00:00:00.0880244 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "14", settings: "Reduce, yes/no, 1 time per day", targetText: "\"at most once a day\"", __pickleIndex: "100", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.1054073 |
| F005 | After a move east, the old and the new day are both Due while they overlap(n: "01", time: "`2026-11-18T23:00:00Z`", entries: "2026-11-19 from `2026-11-18T22:00:00Z` to `2026-11"···, __pickleIndex: "181", exampleTags: []) | @F005 @AC-046 @FR-011 @US5-AS3 | passed | 00:00:00.1088327 |
| F007 | 199 Met of 200 final results is "99% (199 of 200)" | @F007 @AC-005 @FR-002 @FR-003 @US1-AS4 | passed | 00:00:00.5574794 |
| F004 | Starting from a custom habit gives only Build or Reduce from its intent(n: "02", intent: "Reduce", __pickleIndex: "182", exampleTags: []) | @F004 @AC-046 @FR-007 @US5-AS1 | passed | 00:00:00.0872897 |
| F002 | A custom habit is not a catalog habit for the administrator(n: "04", action: "unpublishes", __pickleIndex: "107", exampleTags: []) | @F002 @AC-033 @FR-013 @FR-024 @US3-AS13 | passed | 00:00:00.1642191 |
| F001 | An attempt during the block does not extend it | @F001 @AC-017 @FR-008 @US2-AS6 | passed | 00:00:00.2907333 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "07", name: "`A027 07 No icon`", description: "none", category: "Body", intent: "Build", icon: "none", result: "\"Choose an icon.\"", __pickleIndex: "76", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0753719 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "04", count: "6", marks: "the first marked", result: "\"A habit can have at most 5 suggested starting go"···, after: "no suggested starting goal", __pickleIndex: "112", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0877248 |
| F005 | A partial amount recorded for today is kept and the commitment is Due | @F005 @AC-001 @FR-001 @FR-004 @FR-006 @FR-009 @FR-010 @US1-AS1 | passed | 00:00:00.0995800 |
| F004 | A per-week goal tells when its first week starts(n: "01", startDate: "2026-11-18", time: "`2026-11-18T10:00:00Z`", list: "first week starts 2026-11-23, no start date and no"···, __pickleIndex: "160", exampleTags: []) | @F004 @AC-039 @FR-014 @FR-016 @US4-AS4 | passed | 00:00:00.0907645 |
| F002 | Browsing with no match gives an empty result | @F002 @AC-007 @FR-008 @FR-009 @US1-AS7 | passed | 00:00:00.0901124 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "11", second: "Reduce, count 1000 `sweets`, 1 time per day", result: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "126", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0927020 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "20", settings: "Build, count 5 `pages`, 2 times per day", records: "6 and 6 for 2026-11-18", state: "Met", progress: "progress 2 of 2 times", __pickleIndex: "256", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1066729 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "06", settings: "Build, duration 20 minutes, 1 time per day", records: "19 for 2026-11-18", state: "Failed", progress: "progress 0 of 1 times", __pickleIndex: "242", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0894561 |
| F001 | Administrator creation refuses an email that is not valid | @F001 @AC-045 @FR-003 @FR-014 @US4-AS6 | passed | 00:00:00.0013139 |
| F002 | Publish and unpublish check the acting user and the role before the identifier(n: "01", actingUser: "the registered user `ac046.user.01@example.com`", action: "publishes", result: "\"Only administrators can manage the catalog.\"", __pickleIndex: "193", exampleTags: []) | @F002 @AC-046 @FR-016 | passed | 00:00:00.0792027 |
| F004 | A new goal can start without a habit, but not be created without one | @F004 @AC-063 @FR-006 @FR-007 @US8-AS4 | passed | 00:00:00.0888777 |
| F005 | The lowest and highest amounts are accepted and kept(n: "04", measurement: "duration 20 minutes", value: "1440", __pickleIndex: "28", exampleTags: []) | @F005 @AC-008 @FR-004 @FR-005 | passed | 00:00:00.0930335 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "08", browserTimeZone: "the browser time zone `GMT`", timeZone: "`UTC`", __pickleIndex: "229", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0874109 |
| F004 | The calendar range has 1 to 366 days(n: "04", from: "2026-11-18", to: "2026-11-18", result: "a calendar of 1 commitment, on 2026-11-18", __pickleIndex: "175", exampleTags: []) | @F004 @AC-043 @FR-013 @US4-AS7 | passed | 00:00:00.1017885 |
| F002 | Each of the six categories can be given to a catalog habit(n: "03", category: "Health", __pickleIndex: "160", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0925525 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "02", state: "Published", action: "creates a catalog habit", __pickleIndex: "65", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | passed | 00:00:00.0945550 |
| F007 | The recent results are the latest 30 ended results, oldest first | @F007 @AC-021 @FR-005 @US4-AS1 | passed | 00:00:00.2026826 |
| F002 | Another user, and a catalog habit, may have the same name | @F002 @AC-052 @FR-004 @US4-AS6 | passed | 00:00:00.2683681 |
| F002 | The anonymous visitor cannot manage the catalog(n: "06", action: "unpublishes `A031 06 Published`", __pickleIndex: "99", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0795079 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "15", settings: "Build, yes/no, fixed days Monday, Wednesday and Fr"···, targetText: "\"3 days a week\"", __pickleIndex: "101", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0914608 |
| F002 | Unpublishing a Published habit takes it out of the catalog at once | @F002 @AC-021 @FR-011 @FR-015 @US3-AS4 | passed | 00:00:00.1055866 |
| F004 | "Today" follows the new time zone at once | @F004 @AC-076 @FR-012 @FR-026 | passed | 00:00:00.1013410 |
| F002 | A Draft and an Unpublished habit can be edited and keep their state | @F002 @AC-069 @FR-012 | passed | 00:00:00.1000606 |
| F002 | Field checks come in a fixed order(n: "04", fields: "the name `A029 04 Cat`, no category, no intent", result: "\"Choose a category.\"", __pickleIndex: "82", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0906801 |
| F005 | The demo Yoga goal's weeks when R is a Sunday or a Tuesday(n: "02", runTime: "`2026-11-24T10:00:00Z`", r: "Tuesday 2026-11-24", startDate: "2026-10-27", lastDate: "2026-11-29", entries: "4 entries: 2026-11-02 to 2026-11-08, Missed, no re"···, __pickleIndex: "272", exampleTags: []) | @F005 @AC-068 @FR-030 @US7-AS2 | passed | 00:00:00.4660285 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "30", settings: "Reduce, duration 60 minutes, 1 time per week", records: "30 for 2026-11-16 and 31 for 2026-11-18", state: "Failed", progress: "progress total 61 of at most 60", __pickleIndex: "266", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0980623 |
| F005 | A final per-day yes/no period counts its Yes records(n: "03", settings: "Reduce, yes/no, 1 time per day", records: "No for 2026-11-18", state: "Met", progress: "progress 0 of at most 1", __pickleIndex: "81", exampleTags: []) | @F005 @AC-020 @FR-014 @US2-AS10 | passed | 00:00:00.1018595 |
| F002 | Only the 24 habit icons are accepted(n: "27", icon: "`search`", result: "\"Choose an icon.\"", __pickleIndex: "190", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0901026 |
| F007 | The percentage is rounded down to a whole number(n: "03", nov1: "20", nov2: "20", nov3: "20", text: "\"100% (3 of 3)\"", percentage: "100", successful: "3", __pickleIndex: "6", exampleTags: []) | @F007 @AC-004 @FR-002 @US1-AS4 | passed | 00:00:00.1074745 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "02", settings: "Build, count 5 `pages`, fixed days on all seven da"···, records: "5 for 2026-11-18", state: "Met", progress: "progress 5 of 5", __pickleIndex: "238", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1001168 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "06", settings: "Build, yes/no, fixed days Sunday", targetText: "\"once a week\"", __pickleIndex: "92", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0900886 |
| F005 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit(n: "06", settings: "yes/no, fixed days on all seven days", records: "No for 2026-11-18", state: "Met", __pickleIndex: "98", exampleTags: []) | @F005 @AC-022 @FR-013 @US2-AS12 | passed | 00:00:00.0939970 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "04", fields: "Reduce, duration 999 minutes, 20 times per day", storedUnit: "minutes", __pickleIndex: "141", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0979717 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "09", fields: "Build, count 5 with the unit `\"  \"` followed by "···, storedUnit: "that unit of 20 characters, without the spaces", __pickleIndex: "146", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0956227 |
| F005 | Demo history loading's results name the demo email and no goal, habit, note, unit or amount | @F005 @AC-059 @FR-030 @US7-AS4 | passed | 00:00:00.3757991 |
| F005 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit(n: "04", settings: "duration 30 minutes, 1 time per day", records: "nothing", state: "Missed, no record", __pickleIndex: "96", exampleTags: []) | @F005 @AC-022 @FR-013 @US2-AS12 | passed | 00:00:00.0962891 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "01", action: "reads `Gone` by its identifier", __pickleIndex: "231", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.2082148 |
| F005 | An ended commitment or period stays Pending during its grace period, even when its records reach the target(n: "02", settings: "duration 20 minutes, fixed days on all seven days", value: "20", time: "`2026-11-20T23:59:59Z`", entry: "Pending, grace ends `2026-11-21T00:00:00Z`, progre"···, __pickleIndex: "87", exampleTags: []) | @F005 @AC-021 @FR-012 @US2-AS11 | passed | 00:00:00.1140125 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "07", measurement: "count 5 `pages`", before: "5", change: "a record identifier that belongs to no record to n"···, result: "the result is \"This record does not exist.\" (the"···, __pickleIndex: "231", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.0986694 |
| F005 | A note has at most 500 characters(n: "01", action: "saves the note a text of 501 characters for 2026-1"···, result: "the result is \"The note can have at most 500 char"···, __pickleIndex: "146", exampleTags: []) | @F005 @AC-038 @FR-005 @FR-007 @US4-AS4 | passed | 00:00:00.0942518 |
| F002 | The administrator's list is in name order and can be searched | @F002 @AC-040 @FR-004 @FR-012 @US3-AS19 | passed | 00:00:00.0984781 |
| F007 | The percentage is rounded down to a whole number(n: "02", nov1: "20", nov2: "12", nov3: "none", text: "\"33% (1 of 3)\"", percentage: "33", successful: "1", __pickleIndex: "5", exampleTags: []) | @F007 @AC-004 @FR-002 @US1-AS4 | passed | 00:00:00.1111989 |
| F002 | Publishing a Published habit is a success that changes nothing | @F002 @AC-024 @FR-011 @US3-AS7 | passed | 00:00:00.0906431 |
| F004 | The start date is today or up to 365 days later(n: "05", startDate: "no start date", result: "\"Choose a start date.\"", __pickleIndex: "56", exampleTags: []) | @F004 @AC-010 @FR-006 @FR-012 | passed | 00:00:00.1886108 |
| F002 | The anonymous visitor has no access to custom habits(n: "03", action: "creates the custom habit `Visitor habit`", __pickleIndex: "216", exampleTags: []) | @F002 @AC-055 @FR-023 @US4-AS9 | passed | 00:00:00.0881897 |
| F007 | Demo report loading creates demo5 with its three goals | @F007 @AC-047 @FR-032 @FR-031 @US8-AS7 | passed | 00:00:00.5216749 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "15", settings: "Reduce, count 3 `sweets`, 1 time per week", records: "2 for 2026-11-16 and 2 for 2026-11-18", state: "Failed", progress: "progress total 4 of at most 3", __pickleIndex: "251", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1044354 |
| F002 | Only the 24 habit icons are accepted(n: "11", icon: "`bike`", result: "success", __pickleIndex: "174", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0839156 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "02", action: "starts a new goal from", habit: "`A4.014 02 Gone`", __pickleIndex: "73", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | passed | 00:00:00.1780559 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "03", fields: "Reduce, count 0 `sweets`, 1 time per day", storedUnit: "`sweets`", __pickleIndex: "140", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0850014 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "07", action: "edits `Secret` by its identifier, with the name `\"···, __pickleIndex: "213", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1748583 |
| F005 | The lowest and highest amounts are accepted and kept(n: "02", measurement: "count 5 `pages`", value: "9999", __pickleIndex: "26", exampleTags: []) | @F005 @AC-008 @FR-004 @FR-005 | passed | 00:00:00.1087497 |
| F005 | A final per-day yes/no period counts its Yes records(n: "01", settings: "Reduce, yes/no, 1 time per day", records: "Yes for 2026-11-18", state: "Met", progress: "progress 1 of at most 1", __pickleIndex: "79", exampleTags: []) | @F005 @AC-020 @FR-014 @US2-AS10 | passed | 00:00:00.1013412 |
| F004 | A goal name has at most 60 characters after trimming(n: "01", name: "a text of 60 characters", result: "creating succeeds, and reading the goal gives that"···, __pickleIndex: "57", exampleTags: []) | @F004 @AC-011 @FR-006 @FR-009 | passed | 00:00:00.1837223 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "21", settings: "Reduce, yes/no, fixed days Monday, Tuesday, Wednes"···, targetText: "\"avoided on Mon–Wed, Sat\"", __pickleIndex: "107", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0980347 |
| F001 | A registered user reads their own account details | @F001 @AC-027 @FR-009 @FR-012 @US3-AS2 | passed | 00:00:00.0950167 |
| F004 | The start date is today or up to 365 days later(n: "02", startDate: "the start date 2026-11-17", result: "\"The start date cannot be in the past.\"", __pickleIndex: "53", exampleTags: []) | @F004 @AC-010 @FR-006 @FR-012 | passed | 00:00:00.1741861 |
| F002 | Details give every suggested starting goal in order, with its text and kind | @F002 @AC-013 @FR-001 @FR-010 @FR-017 @US2-AS1 | passed | 00:00:00.0944566 |
| F005 | The first record of a Missed commitment that has a note: the note given is the note, no note keeps it(n: "04", action: "records 20 for 2026-11-18", note: "note \"Away for the weekend.\"", __pickleIndex: "282", exampleTags: []) | @F005 @AC-071 @FR-007 @US4-AS9 | passed | 00:00:00.1035285 |
| F001 | Registration keeps the email in its lower-case form | @F001 @AC-006 @FR-003 @FR-006 @FR-012 | passed | 00:00:00.0986798 |
| F001 | A successful sign-in resets the count of failed sign-ins | @F001 @AC-019 @FR-008 @US2-AS8 | passed | 00:00:00.4575910 |
| F005 | The first record of a Missed commitment that has a note: the note given is the note, no note keeps it(n: "01", action: "records 20 for 2026-11-18 with the note \"Away for"···, note: "note \"Away for the weekend.\"", __pickleIndex: "279", exampleTags: []) | @F005 @AC-071 @FR-007 @US4-AS9 | passed | 00:00:00.1049536 |
| F004 | An anonymous visitor gets no Home heading, no name change and no profile(n: "03", action: "reads their profile", __pickleIndex: "295", exampleTags: []) | @F004 @AC-091 @FR-023 @FR-028 @FR-029 @FR-030 @US11-AS8 | passed | 00:00:00.0010491 |
| F001 | Registration accepts the shortest valid email and a domain of three parts(email: "\"ana@sub.example.com\"", __pickleIndex: "22", exampleTags: []) | @F001 @AC-005 @FR-001 @FR-003 | passed | 00:00:00.0804153 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "03", action: "edits `Secret` by its identifier, with the name `S"···, __pickleIndex: "209", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1559873 |
| F004 | A goal on an Unpublished habit keeps working, and no new goal can use that habit(n: "02", actingUser: "`f4.ac060.owner.02@example.com`", action: "reads the goal", result: "the result has the habit name `A4.060 02 Reading`,"···, __pickleIndex: "209", exampleTags: []) | @F004 @AC-060 @FR-016 @FR-034 @US7-AS6 | passed | 00:00:00.3018590 |
| F001 | Registration accepts a password at the lower bound of the password rule(email: "\"digits.pw@example.com\"", password: "\"12345678\"", whyItMeetsTheRule: "8 characters, all digits", __pickleIndex: "6", exampleTags: []) | @F001 @AC-003 @FR-001 @FR-002 | passed | 00:00:00.0767729 |
| F005 | An ended commitment or period stays Pending during its grace period, even when its records reach the target(n: "01", settings: "duration 20 minutes, fixed days on all seven days", value: "20", time: "`2026-11-19T00:00:00Z`", entry: "Pending, grace ends `2026-11-21T00:00:00Z`, progre"···, __pickleIndex: "86", exampleTags: []) | @F005 @AC-021 @FR-012 @US2-AS11 | passed | 00:00:00.0972471 |
| F007 | Without a valid demo password demo report loading creates nothing(n: "02", password: "the demo password `abc`", result: "\"demo password does not meet the password rule\"", __pickleIndex: "91", exampleTags: []) | @F007 @AC-052 @FR-032 @US8-AS10 | passed | 00:00:00.4454527 |
| F002 | Demo loading without a valid demo password creates no catalog and no custom habit(n: "03", demoPassword: "`walk4`", result: "\"demo password does not meet the password rule\"", __pickleIndex: "243", exampleTags: []) | @F002 @AC-063 @FR-030 @FR-031 @US6-AS4 | passed | 00:00:00.3094663 |
| F005 | A commitment is Upcoming before its start instant and Due from it(n: "01", time: "`2026-11-18T10:00:00Z`", first: "Due, grace ends `2026-11-21T00:00:00Z`", second: "from `2026-11-19T00:00:00Z` to `2026-11-20T00:00:0"···, __pickleIndex: "41", exampleTags: []) | @F005 @AC-012 @FR-010 @US2-AS1 | passed | 00:00:00.0973664 |
| F007 | A duration goal in hours has its trend values and target in minutes | @F007 @AC-058 @FR-006 | passed | 00:00:00.1040375 |
| F007 | A Reduce yes/no week counts its "Yes" records against 0, a Build duration week counts records that reach the amount | @F007 @AC-028 @FR-006 @US4-AS7 | passed | 00:00:00.1389906 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "01", goal: "with no habit, no Build or Reduce, yes/no, fixed d"···, message: "\"Choose a habit.\"", __pickleIndex: "30", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1762368 |
| F005 | Demo history loading takes a demo password only with at least 8 characters and a digit(n: "04", demoPassword: "`exampl12`", why: "8 characters, with a digit", result: "its results are, in this order, \"created\", \"dem"···, __pickleIndex: "276", exampleTags: []) | @F005 @AC-069 @FR-030 @US7-AS6 | passed | 00:00:00.4268592 |
| F002 | Field checks come in a fixed order(n: "05", fields: "the name `A029 05 Int`, the category Body, no inte"···, result: "\"Choose Build or Reduce.\"", __pickleIndex: "83", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0744504 |
| F001 | Registration refuses a password that breaks the password rule(email: "\"eight.nodigit@example.com\"", password: "\"walkingx\"", whyItBreaksTheRule: "8 characters, no digit", __pickleIndex: "4", exampleTags: []) | @F001 @AC-002 @FR-002 @US1-AS3 @US1-AS4 | passed | 00:00:00.0417822 |
| F005 | A change is a correction only from the instant its grace period ends(n: "03", before: "12 for 2026-11-18", time: "`2026-11-18T12:00:00Z`", action: "deletes the record of 2026-11-18", result: "deleting succeeds with the state Due, not a correc"···, __pickleIndex: "112", exampleTags: []) | @F005 @AC-027 @FR-006 | passed | 00:00:00.1072470 |
| F004 | A goal keeps the values it was created with, whatever happens to the suggested starting goal | @F004 @AC-092 @FR-007 | passed | 00:00:00.1959580 |
| F004 | A Reduce goal with an amount has 1 time per period(n: "04", settings: "Reduce, yes/no, 3 times per week", result: "creating succeeds", __pickleIndex: "63", exampleTags: []) | @F004 @AC-012 @FR-004 @FR-006 | passed | 00:00:00.1774544 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "05", second: "Build, count with no amount and the unit `pages`, "···, result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "120", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0911579 |
| F004 | The Home greeting follows the time of day in the account's time zone(n: "06", time: "`2026-11-18T16:00:00Z` (18:00 in Kyiv)", greeting: "\"Good evening, Yuri\"", __pickleIndex: "283", exampleTags: []) | @F004 @AC-086 @FR-029 @US11-AS3 | passed | 00:00:00.0923049 |
| F005 | Lowering the record of a Met commitment below its target is a correction that makes it Failed(n: "01", action: "changes the record of 2026-11-18 to 12", result: "changing", __pickleIndex: "107", exampleTags: []) | @F005 @AC-025 @FR-006 @US3-AS2 | passed | 00:00:00.1105857 |
| F004 | Unpublishing a habit that goals use needs confirmation and gives only the number of goals(n: "01", goals: "`f4.ac057.a.01@example.com` created one goal and `"···, count: "2", text: "\"2 goals use this habit.\"", __pickleIndex: "202", exampleTags: []) | @F004 @AC-057 @FR-033 @US7-AS1 @US7-AS3 @US7-AS5 | passed | 00:00:00.3009017 |
| F002 | For custom habits the acting user and ownership come first, the name check last(n: "05", actingUser: "the registered user `ac057.a.05@example.com`", action: "edits `Secret` to the name `TAKEN` and the icon `m"···, result: "\"Choose an icon.\"", __pickleIndex: "230", exampleTags: []) | @F002 @AC-057 @FR-005 @FR-016 @US4-AS11 | passed | 00:00:00.1723771 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "11", fields: "Build, yes/no, 2 times per day", text: "\"2 times a day\"", kind: "\"Yes/no · flexible\"", __pickleIndex: "38", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0877970 |
| F002 | Custom-habit names and descriptions are trimmed before they are checked and stored(n: "04", name: "`\"Long\"`", description: "`\"  \"` + a text of 500 characters + `\"  \"` (50"···, storedName: "`Long`", storedDescription: "the 500-character description", __pickleIndex: "259", exampleTags: []) | @F002 @AC-073 @FR-004 @FR-005 | passed | 00:00:00.0915567 |
| F005 | A value that does not belong to the goal's measurement is not stored(n: "03", measurement: "duration 20 minutes", given: "the amount 12 together with No", stored: "(2026-11-18, 12), with neither Yes nor No", __pickleIndex: "32", exampleTags: []) | @F005 @AC-009 @FR-004 | passed | 00:00:00.0994438 |
| F007 | Every streak carries the unit of its goal, singular for 1(n: "08", settings: "Build, yes/no, fixed days on all seven days", created: "2026-11-01T08:00:00Z", start: "2026-11-01", yes: "2026-11-02 and 2026-11-03", read: "2026-11-06T10:00:00Z", streak: "\"2 commitments\"", number: "2", unit: "\"commitments\"", __pickleIndex: "30", exampleTags: []) | @F007 @AC-014 @FR-004 @US2-AS6 | passed | 00:00:00.1085855 |
| F004 | An account name is trimmed, and empty means no name(n: "01", name: "the name \"  Yuri  \"", result: "the name \"Yuri\"", __pickleIndex: "273", exampleTags: []) | @F004 @AC-084 @FR-019 @FR-028 @US11-AS1 | passed | 00:00:00.0863691 |
| F001 | An administrator gets only their own account details | @F001 @AC-030 @FR-010 @FR-012 @FR-015 @US3-AS5 | passed | 00:00:00.1325607 |
| F005 | The anonymous visitor gets "You are not signed in." from every use case of this slice(n: "05", useCase: "history", __pickleIndex: "205", exampleTags: []) | @F005 @AC-050 @FR-018 @US6-AS4 | passed | 00:00:00.1045847 |
| F007 | "Missed this week" starts on Monday at 00:00 in the account's time zone and includes the previous Friday(n: "01", zone: "`UTC`", read: "2026-11-15T23:59:59Z", missed: "2026-11-12", pending: "2026-11-13", __pickleIndex: "54", exampleTags: []) | @F007 @AC-034 @FR-012 @US6-AS4 | passed | 00:00:00.1212232 |
| F004 | A custom habit is looked up only among the acting user's custom habits(n: "01", action: "starts a new goal from", habit: "`f4.ac033.b.01@example.com`'s custom habit `Cold s"···, __pickleIndex: "141", exampleTags: []) | @F004 @AC-033 @FR-008 @US3-AS5 | passed | 00:00:00.2401735 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "22", second: "Build, duration 15 minutes, 3 times with no period", result: "\"Choose per day or per week.\"", __pickleIndex: "137", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0925344 |
| F002 | Details with a custom habit's identifier give "not in the catalog"(n: "03", actingUser: "the anonymous visitor", __pickleIndex: "55", exampleTags: []) | @F002 @AC-017 @FR-010 @FR-024 @US2-AS5 | passed | 00:00:00.1697111 |
| F007 | An anonymous visitor and an administrator are refused by every use case of this slice(n: "04", actor: "the anonymous visitor", useCase: "the dashboard", result: "\"You are not signed in.\"", __pickleIndex: "68", exampleTags: []) | @F007 @AC-041 @FR-015 @FR-007 @US7-AS3 @US7-AS4 | passed | 00:00:00.1716552 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "03", count: "6", marks: "none marked", result: "\"A habit can have at most 5 suggested starting go"···, after: "no suggested starting goal", __pickleIndex: "111", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0881663 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "29", settings: "Reduce, duration 60 minutes, 1 time per week", records: "30 for 2026-11-16 and 30 for 2026-11-18", state: "Met", progress: "progress total 60 of at most 60", __pickleIndex: "265", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1051882 |
| F002 | Another registered user cannot reach a custom habit(n: "02", action: "edits, with the name `Mine now`, the category Body"···, __pickleIndex: "205", exampleTags: []) | @F002 @AC-053 @FR-021 @FR-022 @US4-AS7 | passed | 00:00:00.1630337 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "11", goal: "on `A4.008 11 Reading` with Reduce, count 2 `cups`"···, message: "\"A Reduce goal with an amount has 1 time per peri"···, __pickleIndex: "40", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.2070261 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "09", fields: "Build, yes/no, fixed days Saturday and Sunday", text: "\"2 days a week\"", kind: "\"Yes/no · fixed days Sat, Sun\"", __pickleIndex: "36", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0789208 |
| F001 | A new email creates an additional administrator | @F001 @AC-053 @FR-014 @US4-AS9 | passed | 00:00:00.1677246 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "33", settings: "Build, duration 20 minutes, 3 times per week", records: "60 for 2026-11-16, 1 for 2026-11-17 and 59 for 202"···, state: "Failed", progress: "progress 2 of 3 times", __pickleIndex: "269", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1109044 |
| F005 | A day that has not begun on the goal's timeline cannot be recorded(n: "05", timeZone: "UTC", schedule: "fixed days on all seven days", time: "`2026-11-19T00:00:00Z`", date: "2026-11-19", result: "recording succeeds with the state Due, not a corre"···, __pickleIndex: "13", exampleTags: []) | @F005 @AC-006 @FR-002 @US1-AS6 | passed | 00:00:00.0953380 |
| F002 | A new catalog habit is a Draft that the public catalog does not show | @F002 @AC-018 @FR-011 @FR-012 @US3-AS1 | passed | 00:00:00.0918273 |
| F002 | The administrator edits the description, intent and icon of a catalog habit | @F002 @AC-071 @FR-012 | passed | 00:00:00.1189745 |
| F005 | What can be recorded now is every Due and every Pending commitment or period, newest first(n: "07", schedule: "fixed days Friday", startDate: "2026-11-20", time: "`2026-11-18T10:00:00Z`", entries: "no entry", __pickleIndex: "180", exampleTags: []) | @F005 @AC-045 @FR-011 @US5-AS3 | passed | 00:00:00.0949854 |
| F005 | Progress is given at any state, as the goal's kind of target counts it(n: "05", settings: "Build, duration 1 in hours, fixed days on all seve"···, records: "45 for 2026-11-18", progress: "progress 45 of 60 minutes", __pickleIndex: "171", exampleTags: []) | @F005 @AC-044 @FR-014 @US5-AS2 | passed | 00:00:00.1107546 |
| F002 | Field checks come in a fixed order(n: "09", fields: "the name `a029 09 taken`, the category Body, the i"···, result: "\"A catalog habit with this name already exists.\"", __pickleIndex: "87", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0810887 |
| F004 | A second change replaces a version not yet begun, and follows a version already in effect(n: "01", schedule: "3 times per week", startDate: "2026-11-16", from: "2026-11-16", to: "2026-11-29", versions: "the time zone `Europe/Kyiv` from 2026-11-16 and a "···, entries: "2026-11-16 to 2026-11-22 from `2026-11-15T22:00:00"···, __pickleIndex: "268", exampleTags: []) | @F004 @AC-081 @FR-026 @US10-AS10 | passed | 00:00:00.1164498 |
| F005 | The recent notes are the 5 with the latest dates, newest first, each with its state | @F005 @AC-040 @FR-008 @US4-AS6 | passed | 00:00:00.1377906 |
| F002 | Demo loading adds no seed to a catalog that has a habit | @F002 @AC-062 @FR-024 @FR-030 @US6-AS3 | passed | 00:00:00.4780729 |
| F004 | Reading a goal that has not started gives its first version and attempt from its start date | @F004 @AC-026 @FR-005 @FR-016 @FR-018 | passed | 00:00:00.0959734 |
| F001 | An administrator cannot change another account's password | @F001 @AC-038 @FR-010 @FR-013 @FR-015 @US6-AS7 | passed | 00:00:00.2577291 |
| F007 | Every streak carries the unit of its goal, singular for 1(n: "06", settings: "Build, yes/no, 1 time per week", created: "2026-10-26T08:00:00Z", start: "2026-10-26", yes: "no date", read: "2026-11-11T10:00:00Z", streak: "\"0 weeks\"", number: "0", unit: "\"weeks\"", __pickleIndex: "28", exampleTags: []) | @F007 @AC-014 @FR-004 @US2-AS6 | passed | 00:00:00.1010669 |
| F007 | A commitment-based duration goal has one bar per commitment, the last 14 | @F007 @AC-023 @FR-006 @US4-AS3 | passed | 00:00:00.1842201 |
| F007 | A commitment-based yes/no trend keeps its last 12 weeks | @F007 @AC-064 @FR-006 @US4-AS5 | passed | 00:00:00.1801151 |
| F004 | After a large move east, a version already begun is in effect, and a second change follows it | @F004 @AC-082 @FR-026 @FR-027 | passed | 00:00:00.0999860 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "01", fields: "Build, duration 30 minutes, fixed days Monday to S"···, text: "\"30 min daily\"", kind: "\"Duration · fixed days every day\"", __pickleIndex: "28", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0953526 |
| F005 | A day that has not begun on the goal's timeline cannot be recorded(n: "04", timeZone: "UTC", schedule: "fixed days on all seven days", time: "`2026-11-18T23:59:59Z`", date: "2026-11-19", result: "the result is \"You cannot record a day that has n"···, __pickleIndex: "12", exampleTags: []) | @F005 @AC-006 @FR-002 @US1-AS6 | passed | 00:00:00.1002942 |
| F007 | demo5 has one result Missed this week on every weekday(n: "04", run: "2026-11-19T10:00:00Z", date: "2026-11-16", __pickleIndex: "95", exampleTags: []) | @F007 @AC-053 @FR-032 @FR-012 @US8-AS11 | passed | 00:00:00.5832435 |
| F001 | Administrator creation matches the email whatever its case and spaces | @F001 @AC-054 @FR-003 @FR-014 | passed | 00:00:00.0920878 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "26", settings: "Reduce, yes/no, 2 times per week", records: "Yes for 2026-11-16 and No for 2026-11-18", state: "Met", progress: "progress 1 of at most 2", __pickleIndex: "262", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1020104 |
| F004 | Use cases check the acting user, the role, the identifier and the fields, in this order(n: "07", actingUser: "`f4.ac035.admin.07@example.com`", action: "starts a new goal from `A4.035 07 Draft` with the "···, message: "\"Goals are for registered users.\"", __pickleIndex: "153", exampleTags: []) | @F004 @AC-035 @FR-023 | passed | 00:00:00.1725697 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "21", settings: "Build, duration 20 minutes, 1 time per day", records: "21 for 2026-11-18", state: "Met", progress: "progress 1 of 1 times", __pickleIndex: "257", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1032793 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "16", browserTimeZone: "the browser time zone `UTC`", timeZone: "`UTC`", __pickleIndex: "237", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0801573 |
| F004 | Each goal follows a time zone change from its own next day or week(n: "03", schedule: "1 time per day", startDate: "2026-12-01", from: "2026-12-01", to: "2026-12-01", entries: "2026-12-01 from `2026-12-01T05:00:00Z` to `2026-12"···, __pickleIndex: "242", exampleTags: []) | @F004 @AC-072 @FR-026 @FR-027 @US10-AS5 | passed | 00:00:00.0955181 |
| F002 | The administrator reads a Draft with all its fields and its state | @F002 @AC-042 @FR-001 @FR-012 | passed | 00:00:00.0906182 |
| F005 | An administrator gets "Goals are for registered users." from every use case of this slice(n: "04", useCase: "save note", __pickleIndex: "197", exampleTags: []) | @F005 @AC-049 @FR-018 @US6-AS3 | passed | 00:00:00.1672963 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "04", measurement: "duration 20 minutes", before: "20", change: "the record of 2026-11-18 to 1441", result: "the result is \"Enter a whole number of minutes fr"···, __pickleIndex: "228", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.1595227 |
| F004 | Reading a goal gives the version of its current date and the one later version(n: "01", schedule: "fixed days Monday, Wednesday and Friday", startDate: "2026-11-16", time: "`2026-11-18T12:00:00Z`", versions: "the time zone `Europe/Kyiv` from 2026-11-16 and a "···, __pickleIndex: "243", exampleTags: []) | @F004 @AC-073 @FR-016 @FR-026 @US10-AS6 | passed | 00:00:00.0947791 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@example..com\"", whyItIsNotValid: "empty domain part between two non-empty ones", __pickleIndex: "13", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0005125 |
| F004 | A habit that no goal uses is unpublished at once(n: "01", confirmation: "without confirming", __pickleIndex: "206", exampleTags: []) | @F004 @AC-059 @FR-019 @FR-033 @US7-AS4 | passed | 00:00:00.1021587 |
| F004 | Each field refusal of goal creation, with its message(n: "22", settings: "Reduce, count 2 `cups`, 2 times per day", message: "\"A Reduce goal with an amount has 1 time per peri"···, __pickleIndex: "29", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1821205 |
| F004 | The days around a clock change meet at local midnight | @F004 @AC-041 @FR-012 @FR-013 @US4-AS5 | passed | 00:00:00.1010162 |
| F004 | The profile gives "member since" with the month and year of creation in the account's time zone(n: "02", time: "`2026-11-30T22:30:00Z`", browserTimeZone: "the browser time zone `Europe/Kyiv`", memberSince: "\"member since December 2026\"", __pickleIndex: "290", exampleTags: []) | @F004 @AC-089 @FR-030 @US11-AS6 | passed | 00:00:00.0869948 |
| F002 | The category filter counts only Published catalog habits(n: "01", actingUser: "the anonymous visitor", __pickleIndex: "20", exampleTags: []) | @F002 @AC-009 @FR-002 @FR-009 @FR-024 @US1-AS9 | passed | 00:00:00.3801100 |
| F001 | Administrator creation does not turn a registered user into an administrator | @F001 @AC-042 @FR-014 @US4-AS3 | passed | 00:00:00.0911122 |
| F001 | A failed attempt during the block does not count after the block | @F001 @AC-022 @FR-008 | passed | 00:00:00.4654709 |
| F004 | A custom habit that goals use cannot be deleted | @F004 @AC-048 @FR-035 @US5-AS3 | passed | 00:00:00.1015915 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "05", fields: "Build, duration 1 minutes, 1 time per week", storedUnit: "minutes", __pickleIndex: "142", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.1078175 |
| F005 | A note has at most 500 characters(n: "02", action: "saves the note a text of 500 characters for 2026-1"···, result: "saving the note succeeds and the history from 2026"···, __pickleIndex: "147", exampleTags: []) | @F005 @AC-038 @FR-005 @FR-007 @US4-AS4 | passed | 00:00:00.0961210 |
| F007 | Each goal of the overview has its figures, its last 15 results and what can be recorded | @F007 @AC-029 @FR-011 @FR-005 @US5-AS1 | passed | 00:00:00.1911252 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "12", goal: "on `A4.008 12 Reading` with Build, yes/no, fixed d"···, message: "\"Choose a start date.\"", __pickleIndex: "41", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1679632 |
| F002 | Only the 24 habit icons are accepted(n: "09", icon: "`dumbbell`", result: "success", __pickleIndex: "172", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0908469 |
| F005 | A goal whose catalog habit is Unpublished is still recorded and given its states | @F005 @AC-065 @FR-001 @FR-010 | passed | 00:00:00.2027945 |
| F004 | The edges of every target range are accepted(n: "06", settings: "Build, yes/no, 1 time per week", targetText: "\"once a week\"", __pickleIndex: "49", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | passed | 00:00:00.1710593 |
| F007 | A Pending result neither extends nor resets the streak until it is Met(n: "02", nov6: "20", read: "2026-11-09T00:00:00Z", current: "\"3 commitments\"", longest: "\"3 commitments\"", last: "2026-11-06 Met", __pickleIndex: "20", exampleTags: []) | @F007 @AC-012 @FR-003 @US2-AS4 | passed | 00:00:00.1228444 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "10", settings: "Build, count 10 `pages`, 1 time per day", targetText: "\"10 pages a day\"", __pickleIndex: "96", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0990689 |
| F004 | The next commitment is the first commitment that has not ended(n: "04", time: "`2026-11-19T00:00:00Z`", next: "2026-11-20", __pickleIndex: "117", exampleTags: []) | @F004 @AC-023 @FR-014 @US2-AS2 | passed | 00:00:00.0895796 |
| F007 | Every streak carries the unit of its goal, singular for 1(n: "01", settings: "Build, yes/no, 1 time per day", created: "2026-11-01T08:00:00Z", start: "2026-11-01", yes: "2026-11-03", read: "2026-11-06T10:00:00Z", streak: "\"1 day\"", number: "1", unit: "\"day\"", __pickleIndex: "23", exampleTags: []) | @F007 @AC-014 @FR-004 @US2-AS6 | passed | 00:00:00.1100083 |
| F005 | A final per-day Reduce count period is Met when its total is within the limit(n: "04", records: "2 for 2026-11-18", state: "Failed", progress: "progress total 2 of at most 1", __pickleIndex: "78", exampleTags: []) | @F005 @AC-019 @FR-014 @US2-AS9 | passed | 00:00:00.1298024 |
| F005 | A final per-week Build yes/no period is Met with at least N Yes records(n: "04", records: "Yes for 2026-11-16 and Yes for 2026-11-18", state: "Failed", progress: "progress 2 of 3 times", __pickleIndex: "69", exampleTags: []) | @F005 @AC-017 @FR-014 @US2-AS7 | passed | 00:00:00.1382100 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@example.\"", whyItIsNotValid: "domain has one non-empty part, before an empty one", __pickleIndex: "12", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002565 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "05", actingUser: "the registered user `ac016.user.05@example.com`", habit: "`A016 05 Old habit`", __pickleIndex: "48", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1757758 |
| F004 | A custom habit is looked up only among the acting user's custom habits(n: "05", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "an identifier that the test creates and that belon"···, __pickleIndex: "145", exampleTags: []) | @F004 @AC-033 @FR-008 @US3-AS5 | passed | 00:00:00.2370750 |
| F005 | Another registered user gets "This goal does not exist." from every use case of this slice(n: "04", useCase: "save note", goal: "the goal of `f5.ac047.owner.<n>@example.com`", __pickleIndex: "186", exampleTags: []) | @F005 @AC-047 @FR-017 @FR-018 @US6-AS1 | passed | 00:00:00.1718732 |
| F005 | A record identifier that is not a record of the goal named in the call does not exist(n: "03", action: "changes a record identifier that belongs to no rec"···, __pickleIndex: "123", exampleTags: []) | @F005 @AC-032 @FR-005 @FR-017 @US3-AS7 | passed | 00:00:00.1107144 |
| F004 | The Home greeting follows the time of day in the account's time zone(n: "05", time: "`2026-11-18T15:59:00Z` (17:59 in Kyiv)", greeting: "\"Good afternoon, Yuri\"", __pickleIndex: "282", exampleTags: []) | @F004 @AC-086 @FR-029 @US11-AS3 | passed | 00:00:00.0961882 |
| F005 | Any begun date of a week names that week's one note(n: "04", action: "saves the note \"Week note.\" for 2026-11-15", result: "the result is \"Nothing is scheduled on this date."···, __pickleIndex: "155", exampleTags: []) | @F005 @AC-039 @FR-002 @FR-007 @US4-AS5 | passed | 00:00:00.0927053 |
| F004 | A named time zone is kept as given, and the same time zone changes nothing(n: "01", value: "`Europe/Kyiv`", later: "no later version", __pickleIndex: "257", exampleTags: []) | @F004 @AC-075 @FR-024 @FR-026 | passed | 00:00:00.1017460 |
| F005 | The lowest and highest amounts are accepted and kept(n: "01", measurement: "count 5 `pages`", value: "0", __pickleIndex: "25", exampleTags: []) | @F005 @AC-008 @FR-004 @FR-005 | passed | 00:00:00.0927244 |
| F002 | The anonymous visitor has no access to custom habits(n: "05", action: "deletes `Secret` by its identifier", __pickleIndex: "218", exampleTags: []) | @F002 @AC-055 @FR-023 @US4-AS9 | passed | 00:00:00.0955248 |
| F007 | A Pending result neither extends nor resets the streak until it is Met(n: "03", nov6: "none", read: "2026-11-08T23:59:59Z", current: "\"2 commitments\"", longest: "\"2 commitments\"", last: "2026-11-06 Pending", __pickleIndex: "21", exampleTags: []) | @F007 @AC-012 @FR-003 @US2-AS4 | passed | 00:00:00.1022248 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "03", measurement: "count 5 `pages`", before: "5", change: "the record of 2026-11-18 to 10000", result: "the result is \"Enter a whole number from 0 to 999"···, __pickleIndex: "227", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.1080986 |
| F002 | Another registered user cannot reach a custom habit(n: "01", action: "reads", __pickleIndex: "204", exampleTags: []) | @F002 @AC-053 @FR-021 @FR-022 @US4-AS7 | passed | 00:00:00.1599767 |
| F007 | Another user's goal does not exist for the goal report(n: "01", goal: "the goal of `f7.ac039.owner.01@example.com`", __pickleIndex: "62", exampleTags: []) | @F007 @AC-039 @FR-014 @FR-007 @US7-AS1 | passed | 00:00:00.1917936 |
| F007 | demo4's Reading goal reads "92% (24 of 26)" with a streak of 3 and a longest of 18 | @F007 @AC-042 @FR-030 @US8-AS1 | passed | 00:00:00.6290763 |
| F004 | Each field refusal of goal creation, with its message(n: "09", settings: "Reduce, count -1 `cups`, 1 time per day", message: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "16", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1651728 |
| F007 | Every streak carries the unit of its goal, singular for 1(n: "05", settings: "Build, yes/no, 1 time per week", created: "2026-10-26T08:00:00Z", start: "2026-10-26", yes: "2026-10-26 and 2026-11-02", read: "2026-11-11T10:00:00Z", streak: "\"2 weeks\"", number: "2", unit: "\"weeks\"", __pickleIndex: "27", exampleTags: []) | @F007 @AC-014 @FR-004 @US2-AS6 | passed | 00:00:00.1074046 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "06", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "`A4.014 06 Gone`", __pickleIndex: "77", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | passed | 00:00:00.1719539 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "28", settings: "Reduce, duration 60 minutes, 1 time per week", records: "30 for 2026-11-16 and 29 for 2026-11-18", state: "Met", progress: "progress total 59 of at most 60", __pickleIndex: "264", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0984252 |
| F004 | The Home date is today in the account's time zone, in English whatever the culture(n: "01", culture: "the invariant culture", __pickleIndex: "286", exampleTags: []) | @F004 @AC-088 @FR-029 @US11-AS5 | passed | 00:00:00.0983754 |
| F005 | Results corrected after the grace period equal the same records entered on time | @F005 @AC-029 @FR-006 @FR-015 @US3-AS5 | passed | 00:00:00.1405533 |
| F005 | Another registered user gets "This goal does not exist." from every use case of this slice(n: "03", useCase: "delete, with the owner's record identifier", goal: "the goal of `f5.ac047.owner.<n>@example.com`", __pickleIndex: "185", exampleTags: []) | @F005 @AC-047 @FR-017 @FR-018 @US6-AS1 | passed | 00:00:00.1916966 |
| F005 | A final Reduce count commitment is Met at or below its limit and Failed above it(n: "03", count: "count 2 `cups`", value: "2", state: "Met", __pickleIndex: "58", exampleTags: []) | @F005 @AC-015 @FR-014 @US2-AS5 | passed | 00:00:00.1083178 |
| F004 | The profile gives "member since" with the month and year of creation in the account's time zone(n: "01", time: "`2026-11-17T10:00:00Z`", browserTimeZone: "the browser time zone `Europe/Kyiv`", memberSince: "\"member since November 2026\"", __pickleIndex: "289", exampleTags: []) | @F004 @AC-089 @FR-030 @US11-AS6 | passed | 00:00:00.0932088 |
| F005 | A final per-day yes/no period counts its Yes records(n: "02", settings: "Reduce, yes/no, 1 time per day", records: "Yes for 2026-11-18 and Yes for 2026-11-18", state: "Failed", progress: "progress 2 of at most 1", __pickleIndex: "80", exampleTags: []) | @F005 @AC-020 @FR-014 @US2-AS10 | passed | 00:00:00.0937196 |
| F004 | Listing gives every goal with its summary, in name order | @F004 @AC-021 @FR-014 @FR-015 @US2-AS1 | passed | 00:00:00.1861786 |
| F002 | Only the 24 habit icons are accepted(n: "12", icon: "`brain`", result: "success", __pickleIndex: "175", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0826564 |
| F005 | Recording checks the acting user, the goal, the date, the value and the note in that order(n: "01", actor: "the anonymous visitor", date: "2026-11-17", value: "no value", note: "a text of 501 characters", result: "\"You are not signed in.\"", __pickleIndex: "33", exampleTags: []) | @F005 @AC-010 @FR-005 @FR-018 @US1-AS7 | passed | 00:00:00.2515631 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "23", settings: "Reduce, yes/no, fixed days Wednesday", targetText: "\"avoided on Wed\"", __pickleIndex: "109", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0913489 |
| F002 | Only the 24 habit icons are accepted(n: "05", icon: "`piggy-bank`", result: "success", __pickleIndex: "168", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0828661 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "04", state: "Draft", action: "edits `A026 04 Other`", __pickleIndex: "67", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | passed | 00:00:00.0947457 |
| F005 | A final Build duration commitment is Met at or above its target and Failed below it(n: "07", duration: "duration 1 in hours", value: "60", state: "Met", progress: "progress 60 of 60 minutes", __pickleIndex: "54", exampleTags: []) | @F005 @AC-014 @FR-004 @FR-014 @US2-AS4 | passed | 00:00:00.1150058 |
| F005 | An administrator gets "Goals are for registered users." from every use case of this slice(n: "02", useCase: "change, with the owner's record identifier", __pickleIndex: "195", exampleTags: []) | @F005 @AC-049 @FR-018 @US6-AS3 | passed | 00:00:00.1661623 |
| F005 | The demo goal "Reading, 20 min daily" shows Met, Failed with a partial amount, Missed with a note, Pending and Due | @F005 @AC-052 @FR-030 @US7-AS2 | passed | 00:00:00.4554235 |
| F002 | A user's custom-habit name is taken, ignoring case and spaces(n: "01", action: "creates a custom habit", __pickleIndex: "201", exampleTags: []) | @F002 @AC-051 @FR-004 @FR-005 @US4-AS5 | passed | 00:00:00.0863507 |
| F005 | The history's range has 1 to 366 days and is checked after the goal(n: "01", actor: "`f5.ac043.owner.<n>@example.com`", first: "2026-11-18", last: "2026-11-18", result: "it gives exactly one entry, 2026-11-18", __pickleIndex: "161", exampleTags: []) | @F005 @AC-043 @FR-009 @FR-018 @US5-AS4 | passed | 00:00:00.1751434 |
| F005 | A note given with a record is the note; no note leaves it unchanged(n: "05", action: "changes the record of 2026-11-16 to No", note: "the note \"Knee was sore.\"", __pickleIndex: "142", exampleTags: []) | @F005 @AC-037 @FR-007 @US4-AS3 | passed | 00:00:00.1114759 |
| F005 | Deleting the only record of a final commitment is a correction that makes it Missed | @F005 @AC-026 @FR-006 @US3-AS3 | passed | 00:00:00.1023149 |
| F002 | Field checks refuse a custom habit with the same messages(n: "03", name: "`Long text`", description: "a text of 501 characters", category: "Body", intent: "Build", icon: "`droplet`", result: "\"The description can have at most 500 characters."···, __pickleIndex: "221", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0760719 |
| F005 | A value that does not fit the goal's measurement is refused with its message(n: "07", measurement: "duration 20 minutes", value: "1441", message: "\"Enter a whole number of minutes from 0 to 1440.\"···, __pickleIndex: "23", exampleTags: []) | @F005 @AC-007 @FR-005 @US1-AS7 | passed | 00:00:00.0962723 |
| F002 | Custom-habit names and descriptions are trimmed before they are checked and stored(n: "02", name: "`\"Blank\"`", description: "`\"   \"`", storedName: "`Blank`", storedDescription: "no description", __pickleIndex: "257", exampleTags: []) | @F002 @AC-073 @FR-004 @FR-005 | passed | 00:00:00.0859046 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "08", fields: "Build, yes/no, fixed days Monday, Tuesday, Wednesd"···, text: "\"4 days a week\"", kind: "\"Yes/no · fixed days Mon–Wed, Sat\"", __pickleIndex: "35", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.1347747 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "14", browserTimeZone: "the browser time zone `US/Eastern`", timeZone: "`US/Eastern`", __pickleIndex: "235", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0915193 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "19", settings: "Reduce, yes/no, 2 times per day", records: "Yes, Yes and Yes for 2026-11-18", state: "Failed", progress: "progress 3 of at most 2", __pickleIndex: "255", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1044986 |
| F002 | A registered user cannot manage the catalog(n: "06", action: "unpublishes `A030 06 Draft`", __pickleIndex: "93", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1642931 |
| F007 | The recent notes are the 5 with the latest dates across all goals | @F007 @AC-036 @FR-012 @US6-AS6 | passed | 00:00:00.1404488 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "06", browserTimeZone: "the browser time zone `EST`", timeZone: "`UTC`", __pickleIndex: "227", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0918685 |
| F002 | A Draft cannot be unpublished | @F002 @AC-023 @FR-011 @US3-AS6 | passed | 00:00:00.0916469 |
| F007 | Two goals on the same habit have their own figures and nothing combines them | @F007 @AC-008 @FR-010 @US1-AS7 | passed | 00:00:00.1934679 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "10", second: "Build, duration 1000 minutes, 1 time per day", result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "125", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0942724 |
| F005 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit(n: "03", settings: "count 1 `sweet`, 1 time per day", records: "nothing", state: "Missed, no record", __pickleIndex: "95", exampleTags: []) | @F005 @AC-022 @FR-013 @US2-AS12 | passed | 00:00:00.0989941 |
| F004 | The Home date is today in the account's time zone, in English whatever the culture(n: "02", culture: "`uk-UA`", __pickleIndex: "287", exampleTags: []) | @F004 @AC-088 @FR-029 @US11-AS5 | passed | 00:00:00.0859054 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "09", actingUser: "the administrator `ac016.admin.09@example.com`", habit: "an unknown identifier", __pickleIndex: "52", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1787715 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "04", settings: "Build, count 5 `pages`, 2 times per day", records: "5 and 5 for 2026-11-18", state: "Met", progress: "progress 2 of 2 times", __pickleIndex: "240", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1013701 |
| F004 | Each field refusal of goal creation, with its message(n: "17", settings: "Build, yes/no, fixed days with no day", message: "\"Choose at least one day.\"", __pickleIndex: "24", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1793461 |
| F002 | Each of the six categories can be given to a catalog habit(n: "01", category: "Body", __pickleIndex: "158", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0817821 |
| F001 | Registration accepts the shortest valid email and a domain of three parts(email: "\"a@b.c\"", __pickleIndex: "21", exampleTags: []) | @F001 @AC-005 @FR-001 @FR-003 | passed | 00:00:00.0881536 |
| F005 | A note given with a record is the note; no note leaves it unchanged(n: "01", action: "records Yes for 2026-11-18", note: "the note \"Knee was sore.\"", __pickleIndex: "138", exampleTags: []) | @F005 @AC-037 @FR-007 @US4-AS3 | passed | 00:00:00.1025979 |
| F004 | An account name is trimmed, and empty means no name(n: "04", name: "the name `\"\"`", result: "no name", __pickleIndex: "276", exampleTags: []) | @F004 @AC-084 @FR-019 @FR-028 @US11-AS1 | passed | 00:00:00.0929845 |
| F002 | A demo email held by an administrator gets no custom habit | @F002 @AC-068 @FR-025 @FR-031 | passed | 00:00:00.3750570 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "02", settings: "Build, count 20 `pages`, 3 times per week", targetText: "\"20 pages, 3 times a week\"", __pickleIndex: "88", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0893515 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "02", count: "2", marks: "both marked", result: "\"Mark one suggested starting goal for the card.\"", after: "no suggested starting goal", __pickleIndex: "110", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0854916 |
| F002 | Each of the six categories can be given to a catalog habit(n: "02", category: "Mind", __pickleIndex: "159", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0952743 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "14", fields: "Build, yes/no, fixed days Monday, Tuesday and Sund"···, text: "\"3 days a week\"", kind: "\"Yes/no · fixed days Mon, Tue, Sun\"", __pickleIndex: "41", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0802154 |
| F007 | demo5's Strength training and Screen time goals on other weekdays(n: "01", run: "2026-11-16T10:00:00Z", strength: "\"81% (9 of 11)\"", current: "\"2 weeks\"", longest: "\"5 weeks\"", __pickleIndex: "85", exampleTags: []) | @F007 @AC-049 @FR-032 | passed | 00:00:00.5807368 |
| F005 | The history's range has 1 to 366 days and is checked after the goal(n: "03", actor: "`f5.ac043.owner.<n>@example.com`", first: "2026-11-16", last: "2027-11-17", result: "the result is \"Choose a range of 1 to 366 days.\"", __pickleIndex: "163", exampleTags: []) | @F005 @AC-043 @FR-009 @FR-018 @US5-AS4 | passed | 00:00:00.1805602 |
| F007 | A per-week goal started mid-week reports from its first full week | @F007 @AC-056 @FR-005 @FR-006 @FR-001 | passed | 00:00:00.1280717 |
| F004 | A range before the start date gives an empty calendar | @F004 @AC-042 @FR-013 @US4-AS6 | passed | 00:00:00.1026706 |
| F004 | Each field refusal of goal creation, with its message(n: "14", settings: "Build, count 20 with a unit of 21 characters, 3 ti"···, message: "\"The unit can have at most 20 characters.\"", __pickleIndex: "21", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1819135 |
| F004 | After a move west, a commitment already over is not the next one(n: "02", time: "`2026-11-18T23:00:00Z`", next: "2026-11-20", __pickleIndex: "262", exampleTags: []) | @F004 @AC-077 @FR-014 @FR-026 | passed | 00:00:00.0992231 |
| F004 | A Reduce goal with an amount has 1 time per period(n: "06", settings: "Reduce, count 2 `cups`, fixed days Monday, Wednesd"···, result: "creating succeeds", __pickleIndex: "65", exampleTags: []) | @F004 @AC-012 @FR-004 @FR-006 | passed | 00:00:00.1673273 |
| F002 | Only the 24 habit icons are accepted(n: "07", icon: "`coffee`", result: "success", __pickleIndex: "170", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0877488 |
| F005 | A final yes/no commitment is Met on Yes for Build and on No for Reduce(n: "03", intent: "Reduce", value: "No", state: "Met", __pickleIndex: "64", exampleTags: []) | @F005 @AC-016 @FR-014 @US2-AS6 | passed | 00:00:00.1037093 |
| F005 | A change is a correction only from the instant its grace period ends(n: "04", before: "12 for 2026-11-18", time: "`2026-11-20T23:59:59Z`", action: "deletes the record of 2026-11-18", result: "deleting succeeds with the state Pending, not a co"···, __pickleIndex: "113", exampleTags: []) | @F005 @AC-027 @FR-006 | passed | 00:00:00.1056693 |
| F002 | An unknown identifier does not exist for the administrator(n: "01", action: "reads", __pickleIndex: "100", exampleTags: []) | @F002 @AC-032 @FR-013 @US3-AS12 | passed | 00:00:00.0847400 |
| F002 | Only the 24 habit icons are accepted(n: "25", icon: "`menu`", result: "\"Choose an icon.\"", __pickleIndex: "188", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0910808 |
| F005 | Any begun date of a week names that week's one note(n: "01", action: "saves the note \"Week note.\" for 2026-11-18", result: "saving the note succeeds and the history from 2026"···, __pickleIndex: "152", exampleTags: []) | @F005 @AC-039 @FR-002 @FR-007 @US4-AS5 | passed | 00:00:00.1159438 |
| F007 | Each commitment, day or week counts once, however many records it has(n: "02", settings: "Build, yes/no, fixed days on Monday, Wednesday and"···, created: "2026-11-02T08:00:00Z", start: "2026-11-02", records: "`Yes` for 2026-11-02, `No` for 2026-11-04 and `Yes"···, read: "2026-11-11T10:00:00Z", text: "\"33% (1 of 3)\"", __pickleIndex: "12", exampleTags: []) | @F007 @AC-007 @FR-001 @US1-AS6 | passed | 00:00:00.1180367 |
| F005 | A value that does not fit the goal's measurement is refused with its message(n: "02", measurement: "count 5 `pages`", value: "no value", message: "\"Enter a whole number from 0 to 9999.\"", __pickleIndex: "18", exampleTags: []) | @F005 @AC-007 @FR-005 @US1-AS7 | passed | 00:00:00.1062391 |
| F005 | A note has at most 500 characters(n: "05", action: "saves the note a padded text of 500 characters for"···, result: "saving the note succeeds and the history from 2026"···, __pickleIndex: "150", exampleTags: []) | @F005 @AC-038 @FR-005 @FR-007 @US4-AS4 | passed | 00:00:00.1023222 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "04", value: "`Etc/UTC`", __pickleIndex: "252", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.1407841 |
| F004 | Browsing and details without an acting user give the slice 2 result | @F004 @AC-056 @FR-019 @FR-031 | passed | 00:00:00.1743113 |
| F004 | A per-week goal tells when its first week starts(n: "04", startDate: "2026-11-23", time: "`2026-11-18T10:00:00Z`", list: "first week starts 2026-11-23, start date 2026-11-2"···, __pickleIndex: "163", exampleTags: []) | @F004 @AC-039 @FR-014 @FR-016 @US4-AS4 | passed | 00:00:00.1026046 |
| F005 | An administrator gets "Goals are for registered users." from every use case of this slice(n: "05", useCase: "history", __pickleIndex: "198", exampleTags: []) | @F005 @AC-049 @FR-018 @US6-AS3 | passed | 00:00:00.1827208 |
| F005 | An unrecorded commitment is Pending for 48 hours after its end and then Missed by itself(n: "01", time: "`2026-11-19T00:00:00Z`", entry: "Pending, grace ends `2026-11-21T00:00:00Z`, no cor"···, __pickleIndex: "44", exampleTags: []) | @F005 @AC-013 @FR-010 @FR-016 @US2-AS2 @US2-AS3 | passed | 00:00:00.1077858 |
| F004 | Each field refusal of goal creation, with its message(n: "12", settings: "Build, count 20 with the unit `\"   \"`, 3 times p"···, message: "\"Enter a unit, for example pages.\"", __pickleIndex: "19", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1716653 |
| F007 | Around a clock change forward, a grace period ends at 01:00 local time and a 23-hour day counts once(n: "01", read: "2026-03-30T22:59:59Z", consistency: "\"100% (5 of 5)\"", current: "\"5 commitments\"", date: "2026-03-28", state: "Pending", __pickleIndex: "109", exampleTags: []) | @F007 @AC-061 @FR-001 @FR-003 @FR-008 | passed | 00:00:00.1379944 |
| F002 | An Unpublished habit can be published again | @F002 @AC-022 @FR-011 @US3-AS5 | passed | 00:00:00.0889253 |
| F007 | A user with no goals gets counts of 0 and empty lists | @F007 @AC-037 @FR-013 @US6-AS7 | passed | 00:00:00.0960271 |
| F005 | A final Build duration commitment is Met at or above its target and Failed below it(n: "08", duration: "duration 1 in hours", value: "61", state: "Met", progress: "progress 61 of 60 minutes", __pickleIndex: "55", exampleTags: []) | @F005 @AC-014 @FR-004 @FR-014 @US2-AS4 | passed | 00:00:00.0993079 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "06", second: "Build, count 0 `pages`, 1 time per day", result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "121", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0917082 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "01", name: "`\"   \"`", description: "none", category: "Body", intent: "Build", icon: "`book-open`", result: "\"Enter a name.\"", __pickleIndex: "70", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0822185 |
| F004 | The start date is today or up to 365 days later(n: "03", startDate: "the start date 2027-11-18", result: "creating succeeds", __pickleIndex: "54", exampleTags: []) | @F004 @AC-010 @FR-006 @FR-012 | passed | 00:00:00.1730353 |
| F007 | demo5 has one result Missed this week on every weekday(n: "06", run: "2026-11-21T10:00:00Z", date: "2026-11-18", __pickleIndex: "97", exampleTags: []) | @F007 @AC-053 @FR-032 @FR-012 @US8-AS11 | passed | 00:00:00.5901102 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "06", measurement: "count 5 `pages`", before: "5", change: "the record of 2026-11-18 to 3 with the note a text"···, result: "the result is \"The note can have at most 500 char"···, __pickleIndex: "230", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.1620640 |
| F004 | A user's list never contains another user's goals | @F004 @AC-030 @FR-020 @US3-AS2 | passed | 00:00:00.2495198 |
| F007 | Deleting the only record of a final commitment makes it Missed | @F007 @AC-018 @FR-008 @US3-AS3 | passed | 00:00:00.1438418 |
| F002 | Names and descriptions are trimmed before they are checked and stored(n: "01", name: "`\"   A070 01 Trimmed   \"`", description: "`\"  Read daily.  \"`", storedName: "`A070 01 Trimmed`", storedDescription: "the description \"Read daily.\"", __pickleIndex: "250", exampleTags: []) | @F002 @AC-070 @FR-004 @FR-005 | passed | 00:00:00.0824860 |
| F005 | A final per-week Build yes/no period is Met with at least N Yes records(n: "02", records: "Yes for 2026-11-16, Yes for 2026-11-18 and No for "···, state: "Failed", progress: "progress 2 of 3 times", __pickleIndex: "67", exampleTags: []) | @F005 @AC-017 @FR-014 @US2-AS7 | passed | 00:00:00.1397158 |
| F007 | Upcoming entries end the Today list for goals with no Due result, up to 6 days ahead | @F007 @AC-038 @FR-012 @US6-AS8 | passed | 00:00:00.1200506 |
| F005 | An administrator gets "Goals are for registered users." from every use case of this slice(n: "06", useCase: "recordable now", __pickleIndex: "199", exampleTags: []) | @F005 @AC-049 @FR-018 @US6-AS3 | passed | 00:00:00.1699122 |
| F004 | Each field refusal of goal creation, with its message(n: "16", settings: "Build, yes/no, no schedule", message: "\"Choose fixed days or how many times.\"", __pickleIndex: "23", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1729659 |
| F001 | The anonymous visitor cannot change a password, whatever the new password(newPassword: "\"running42\"", whyItMatters: "meets the password rule", __pickleIndex: "55", exampleTags: []) | @F001 @AC-036 @FR-011 @FR-013 @US6-AS5 | passed | 00:00:00.0007370 |
| F005 | Demo loading still names only the three slice-4 demo accounts, and their goals get no records | @F005 @AC-062 @FR-019 @FR-031 @US7-AS5 | passed | 00:00:00.5548164 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "13", browserTimeZone: "the browser time zone `Asia/Calcutta`", timeZone: "`Asia/Calcutta`", __pickleIndex: "234", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0905091 |
| F004 | Use cases check the acting user, the role, the identifier and the fields, in this order(n: "01", actingUser: "the anonymous visitor", action: "reads the evaluation calendar from 2026-11-24 to 2"···, message: "\"You are not signed in.\"", __pickleIndex: "147", exampleTags: []) | @F004 @AC-035 @FR-023 | passed | 00:00:00.1698593 |
| F007 | A late record for a Missed day joins the runs on both sides | @F007 @AC-016 @FR-008 @US3-AS1 | passed | 00:00:00.1347679 |
| F005 | Another registered user gets "This goal does not exist." from every use case of this slice(n: "06", useCase: "recordable now", goal: "the goal of `f5.ac047.owner.<n>@example.com`", __pickleIndex: "188", exampleTags: []) | @F005 @AC-047 @FR-017 @FR-018 @US6-AS1 | passed | 00:00:00.1792915 |
| F002 | Browsing returns only Published habits, with their card content, to every audience(n: "03", actingUser: "the administrator `ac001.admin.03@example.com`", __pickleIndex: "2", exampleTags: []) | @F002 @AC-001 @FR-008 @US1-AS1 | passed | 00:00:00.2080677 |
| F005 | Around a clock change the grace period lasts exactly 48 elapsed hours(n: "03", createdAt: "`2027-03-27T08:00:00Z`", date: "2027-03-27", time: "`2027-03-29T21:59:59Z`", start: "`2027-03-26T22:00:00Z`", end: "`2027-03-27T22:00:00Z`", entry: "Pending, grace ends `2027-03-29T22:00:00Z` (01:00 "···, __pickleIndex: "104", exampleTags: []) | @F005 @AC-023 @FR-016 @US2-AS13 | passed | 00:00:00.0901416 |
| F002 | Browsing by intent returns only that intent(n: "03", intent: "both", sugar: "contains", water: "contains", __pickleIndex: "7", exampleTags: []) | @F002 @AC-004 @FR-009 @US1-AS4 | passed | 00:00:00.0980840 |
| F004 | Each field refusal of goal creation, with its message(n: "15", settings: "Build, duration 15 with no duration unit, 3 times "···, message: "\"Choose minutes or hours.\"", __pickleIndex: "22", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1645406 |
| F002 | Field checks come in a fixed order(n: "06", fields: "the name `A029 06 Taken`, the category Body, the i"···, result: "\"Choose an icon.\"", __pickleIndex: "84", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0846723 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "06", fields: "Build, count 10 `pages`, fixed day Saturday", text: "\"10 pages, once a week\"", kind: "\"Count · fixed days Sat\"", __pickleIndex: "33", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0868995 |
| F002 | Only the 24 habit icons are accepted(n: "20", icon: "`wallet`", result: "success", __pickleIndex: "183", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0914135 |
| F004 | Catalog goal counts are the acting user's own | @F004 @AC-034 @FR-032 @US3-AS6 | passed | 00:00:00.2657246 |
| F007 | demo4's Yoga goal reads its weeks by the weekday of the run(n: "02", run: "2026-11-22T10:00:00Z", consistency: "\"33% (1 of 3)\"", current: "\"1 week\"", longest: "\"1 week\"", __pickleIndex: "76", exampleTags: []) | @F007 @AC-044 @FR-030 @US8-AS3 | passed | 00:00:00.7603675 |
| F001 | Sign-in matches the email whatever its case | @F001 @AC-010 @FR-005 @FR-006 @US2-AS1 | passed | 00:00:00.0885075 |
| F007 | Pending and Due results are not counted, also when they reach the target | @F007 @AC-002 @FR-001 @US1-AS2 | passed | 00:00:00.1783323 |
| F005 | A final Build duration commitment is Met at or above its target and Failed below it(n: "03", duration: "duration 20 minutes", value: "21", state: "Met", progress: "progress 21 of 20 minutes", __pickleIndex: "50", exampleTags: []) | @F005 @AC-014 @FR-004 @FR-014 @US2-AS4 | passed | 00:00:00.0944305 |
| F004 | A named time zone is kept as given, and the same time zone changes nothing(n: "03", value: "`UTC`", later: "a later version `UTC` from 2026-11-19", __pickleIndex: "259", exampleTags: []) | @F004 @AC-075 @FR-024 @FR-026 | passed | 00:00:00.1063418 |
| F005 | A final yes/no commitment is Met on Yes for Build and on No for Reduce(n: "02", intent: "Build", value: "No", state: "Failed", __pickleIndex: "63", exampleTags: []) | @F005 @AC-016 @FR-014 @US2-AS6 | passed | 00:00:00.0969512 |
| F004 | The edges of every target range are accepted(n: "04", settings: "Reduce, count 999 `cups`, 1 time per day", targetText: "\"at most 999 cups a day\"", __pickleIndex: "47", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | passed | 00:00:00.1651690 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "09", second: "Build, duration 0 minutes, 1 time per day", result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "124", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0831952 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "10", measurement: "yes/no", before: "No", change: "the record of 2026-11-18 to Yes together with the "···, result: "changing succeeds with the state Met, a correction"···, __pickleIndex: "234", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.1105312 |
| F007 | A per-week or per-day result counts only from the end of its grace period(n: "03", settings: "Build, yes/no, 1 time per week", created: "2026-11-02T08:00:00Z", start: "2026-11-02", records: "`Yes` for 2026-11-02", read: "2026-11-11T00:00:00Z", consistency: "\"100% (1 of 1)\"", current: "\"1 week\"", first: "2026-11-02 – 2026-11-08 Met", __pickleIndex: "114", exampleTags: []) | @F007 @AC-062 @FR-001 @FR-003 @FR-008 | passed | 00:00:00.1175700 |
| F004 | A goal on a custom habit is based on it and named after it | @F004 @AC-047 @FR-001 @FR-010 @US5-AS2 | passed | 00:00:00.1051906 |
| F002 | Each of the six categories can be given to a catalog habit(n: "06", category: "Other", __pickleIndex: "163", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0837325 |
| F002 | Custom habits never appear in browsing, for any audience(n: "01", actingUser: "the registered user `ac006.owner.01@example.com`", __pickleIndex: "14", exampleTags: []) | @F002 @AC-006 @FR-024 @US1-AS6 | passed | 00:00:00.2388437 |
| F007 | An anonymous visitor and an administrator are refused by every use case of this slice(n: "07", actor: "the administrator `f7.ac041.admin.07@example.com`", useCase: "the goals overview", result: "\"Goals are for registered users.\"", __pickleIndex: "71", exampleTags: []) | @F007 @AC-041 @FR-015 @FR-007 @US7-AS3 @US7-AS4 | passed | 00:00:00.1716214 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"spaces.admin@example.com\"", password: "\"   \"", __pickleIndex: "68", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0002003 |
| F005 | A final per-week Build yes/no period is Met with at least N Yes records(n: "01", records: "Yes for 2026-11-16, Yes for 2026-11-18 and Yes for"···, state: "Met", progress: "progress 3 of 3 times", __pickleIndex: "66", exampleTags: []) | @F005 @AC-017 @FR-014 @US2-AS7 | passed | 00:00:00.1352850 |
| F005 | A note has at most 500 characters(n: "03", action: "records 20 for 2026-11-18 with the note a text of "···, result: "the result is \"The note can have at most 500 char"···, __pickleIndex: "148", exampleTags: []) | @F005 @AC-038 @FR-005 @FR-007 @US4-AS4 | passed | 00:00:00.1005162 |
| F005 | The lowest and highest amounts are accepted and kept(n: "03", measurement: "duration 20 minutes", value: "0", __pickleIndex: "27", exampleTags: []) | @F005 @AC-008 @FR-004 @FR-005 | passed | 00:00:00.0985846 |
| F007 | demo5's reports on a Wednesday run(n: "01", goal: "\"Screen time, at most 120 min a day\"", consistency: "\"93% (77 of 82)\"", current: "\"0 days\"", longest: "\"34 days\"", bars: "14", __pickleIndex: "82", exampleTags: []) | @F007 @AC-048 @FR-032 @US8-AS8 | passed | 00:00:00.6529814 |
| F005 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit(n: "02", settings: "yes/no, 1 time per day", records: "nothing", state: "Missed, no record", __pickleIndex: "94", exampleTags: []) | @F005 @AC-022 @FR-013 @US2-AS12 | passed | 00:00:00.0975065 |
| F005 | Demo history loading without a demo password creates no account(n: "01", demoPassword: "absent", __pickleIndex: "220", exampleTags: []) | @F005 @AC-061 @FR-030 @US7-AS6 | passed | 00:00:00.3793860 |
| F002 | Field checks come in a fixed order(n: "07", fields: "the name `A029 07 Taken`, the category Body, the i"···, result: "\"Mark one suggested starting goal for the card.\"", __pickleIndex: "85", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0843485 |
| F004 | The Home date is today in the account's time zone, in English whatever the culture(n: "03", culture: "`de-DE`", __pickleIndex: "288", exampleTags: []) | @F004 @AC-088 @FR-029 @US11-AS5 | passed | 00:00:00.0921435 |
| F002 | Demo loading without a valid demo password creates no catalog and no custom habit(n: "01", demoPassword: "absent", result: "\"no demo password configured\"", __pickleIndex: "241", exampleTags: []) | @F002 @AC-063 @FR-030 @FR-031 @US6-AS4 | passed | 00:00:00.3459207 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "03", second: "Build, yes/no with the amount 5, 1 time per day", result: "\"A yes/no suggested starting goal has no amount o"···, __pickleIndex: "118", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0919085 |
| F004 | A second change replaces a version not yet begun, and follows a version already in effect(n: "03", schedule: "1 time per day", startDate: "2026-12-01", from: "2026-12-01", to: "2026-12-01", versions: "the time zone `Asia/Tokyo` from 2026-12-01 and no "···, entries: "2026-12-01 from `2026-11-30T15:00:00Z` to `2026-12"···, __pickleIndex: "270", exampleTags: []) | @F004 @AC-081 @FR-026 @US10-AS10 | passed | 00:00:00.1073089 |
| F002 | Browsing gives the count of the habits it returns | @F002 @AC-011 @FR-008 | passed | 00:00:00.0985792 |
| F005 | A commitment is Upcoming before its start instant and Due from it(n: "03", time: "`2026-11-19T00:00:00Z`", first: "Pending, grace ends `2026-11-21T00:00:00Z`", second: "Due, grace ends `2026-11-22T00:00:00Z`", __pickleIndex: "43", exampleTags: []) | @F005 @AC-012 @FR-010 @US2-AS1 | passed | 00:00:00.0924869 |
| F004 | Each field refusal of goal creation, with its message(n: "13", settings: "Build, count 20 with no unit, 3 times per week", message: "\"Enter a unit, for example pages.\"", __pickleIndex: "20", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1714129 |
| F005 | The evaluation calendar still gives no state | @F005 @AC-063 @FR-019 | passed | 00:00:00.1180674 |
| F004 | Reading a custom habit gives the number of goals that use it | @F004 @AC-051 @FR-035 | passed | 00:00:00.1135957 |
| F002 | Only the 24 habit icons are accepted(n: "23", icon: "`sparkles`", result: "success", __pickleIndex: "186", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0812249 |
| F002 | Publishing a Draft makes it public | @F002 @AC-019 @FR-011 @US3-AS2 | passed | 00:00:00.0999872 |
| F007 | Every streak carries the unit of its goal, singular for 1(n: "03", settings: "Build, yes/no, 1 time per day", created: "2026-11-01T08:00:00Z", start: "2026-11-01", yes: "no date", read: "2026-11-06T10:00:00Z", streak: "\"0 days\"", number: "0", unit: "\"days\"", __pickleIndex: "25", exampleTags: []) | @F007 @AC-014 @FR-004 @US2-AS6 | passed | 00:00:00.0988044 |
| F004 | An administrator gets no goal use case(n: "06", action: "browses the catalog as `f4.ac031.admin.06@example."···, __pickleIndex: "134", exampleTags: []) | @F004 @AC-031 @FR-021 @US3-AS3 | passed | 00:00:00.1931426 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "17", second: "Build, duration 15 with no unit, 1 time per day", result: "\"Choose minutes or hours.\"", __pickleIndex: "132", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0924710 |
| F004 | An account name has at most 60 characters | @F004 @AC-085 @FR-028 @US11-AS2 | passed | 00:00:00.1745520 |
| F002 | Each of the six categories can be given to a catalog habit(n: "05", category: "Home", __pickleIndex: "162", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0880678 |
| F004 | A goal on an Unpublished habit keeps working, and no new goal can use that habit(n: "04", actingUser: "`f4.ac060.owner.04@example.com`", action: "creates a goal on `A4.060 04 Reading` with Build, "···, result: "the result is \"This habit is not in the catalog.\"···, __pickleIndex: "211", exampleTags: []) | @F004 @AC-060 @FR-016 @FR-034 @US7-AS6 | passed | 00:00:00.2656182 |
| F002 | Only the 24 habit icons are accepted(n: "06", icon: "`candy`", result: "success", __pickleIndex: "169", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0891191 |
| F004 | The start date is today or up to 365 days later(n: "01", startDate: "the start date 2026-11-18", result: "creating succeeds", __pickleIndex: "52", exampleTags: []) | @F004 @AC-010 @FR-006 @FR-012 | passed | 00:00:00.1754746 |
| F004 | The calendar range has 1 to 366 days(n: "01", from: "2026-11-24", to: "2026-11-18", result: "\"Choose a range of 1 to 366 days.\"", __pickleIndex: "172", exampleTags: []) | @F004 @AC-043 @FR-013 @US4-AS7 | passed | 00:00:00.0963546 |
| F005 | A final per-week Build count period counts only the records that reach the amount(n: "05", records: "60 for 2026-11-16", state: "Failed", progress: "progress 1 of 3 times", __pickleIndex: "74", exampleTags: []) | @F005 @AC-018 @FR-014 @US2-AS8 | passed | 00:00:00.0961254 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "06", name: "`A027 06 No intent`", description: "none", category: "Body", intent: "none", icon: "`book-open`", result: "\"Choose Build or Reduce.\"", __pickleIndex: "75", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0781706 |
| F001 | A password change to a password that breaks the password rule is refused(email: "\"weak.new@example.com\"", newPassword: "\"runningfar\"", whyItBreaksTheRule: "10 characters, no digit", __pickleIndex: "52", exampleTags: []) | @F001 @AC-034 @FR-002 @FR-013 @US6-AS3 | passed | 00:00:00.1716600 |
| F005 | After a move east, the old and the new day are both Due while they overlap(n: "02", time: "`2026-11-19T05:00:00Z`", entries: "2026-11-19, Due, grace ends `2026-11-21T22:00:00Z`"···, __pickleIndex: "182", exampleTags: []) | @F005 @AC-046 @FR-011 @US5-AS3 | passed | 00:00:00.1135504 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "08", value: "`\"\"`", __pickleIndex: "256", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.1000113 |
| F002 | The anonymous visitor has no access to custom habits(n: "01", action: "lists custom habits", __pickleIndex: "214", exampleTags: []) | @F002 @AC-055 @FR-023 @US4-AS9 | passed | 00:00:00.0900685 |
| F002 | Browsing by category returns only that category | @F002 @AC-003 @FR-009 @US1-AS3 | passed | 00:00:00.0825816 |
| F002 | Field checks refuse a custom habit with the same messages(n: "02", name: "a name of 61 characters with the prefix `Habit`", description: "none", category: "Body", intent: "Build", icon: "`droplet`", result: "\"The name can have at most 60 characters.\"", __pickleIndex: "220", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0915305 |
| F004 | An account name is trimmed, and empty means no name(n: "02", name: "no name", result: "no name", __pickleIndex: "274", exampleTags: []) | @F004 @AC-084 @FR-019 @FR-028 @US11-AS1 | passed | 00:00:00.0879456 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "01", state: "Draft", action: "creates a catalog habit", __pickleIndex: "64", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | passed | 00:00:00.0874177 |
| F005 | What can be recorded now is every Due and every Pending commitment or period, newest first(n: "03", schedule: "fixed days Monday, Wednesday and Friday", startDate: "2026-11-16", time: "`2026-11-19T10:00:00Z`", entries: "exactly one entry: 2026-11-18, Pending, grace ends"···, __pickleIndex: "176", exampleTags: []) | @F005 @AC-045 @FR-011 @US5-AS3 | passed | 00:00:00.1229238 |
| F004 | Reading a goal gives the version of its current date and the one later version(n: "06", schedule: "1 time per day", startDate: "2026-12-01", time: "`2026-11-18T12:00:00Z`", versions: "the time zone `America/New_York` from 2026-12-01 a"···, __pickleIndex: "248", exampleTags: []) | @F004 @AC-073 @FR-016 @FR-026 @US10-AS6 | passed | 00:00:00.1002268 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "03", settings: "Reduce, count 1 `sweet`, 1 time per day", targetText: "\"at most 1 sweet a day\"", __pickleIndex: "89", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.1017570 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "10", fields: "Build, duration 1 hours, fixed day Sunday", storedUnit: "hours", __pickleIndex: "147", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0937246 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "17", settings: "Reduce, duration 1 hours, 1 time per day", targetText: "\"at most 1 h a day\"", __pickleIndex: "103", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0972533 |
| F005 | Demo history loading takes a demo password only with at least 8 characters and a digit(n: "03", demoPassword: "`abc`", why: "3 characters, no digit", result: "its result is \"demo password does not meet the pa"···, __pickleIndex: "275", exampleTags: []) | @F005 @AC-069 @FR-030 @US7-AS6 | passed | 00:00:00.3941854 |
| F004 | An anonymous visitor gets no goal use case(n: "04", action: "reads `f4.ac032.user.04@example.com`'s goal", __pickleIndex: "138", exampleTags: []) | @F004 @AC-032 @FR-022 @US3-AS4 | passed | 00:00:00.1637515 |
| F004 | Anonymous visitors and administrators get the catalog without goal information(n: "06", actingUser: "`f4.ac055.admin.06@example.com`", action: "browses the catalog as `f4.ac055.admin.06@example."···, result: "the result is \"Goals are for registered users.\"", __pickleIndex: "200", exampleTags: []) | @F004 @AC-055 @FR-032 @US6-AS4 | passed | 00:00:00.1639541 |
| F005 | A final per-day Reduce count period is Met when its total is within the limit(n: "01", records: "0 for 2026-11-18 and 1 for 2026-11-18", state: "Met", progress: "progress total 1 of at most 1", __pickleIndex: "75", exampleTags: []) | @F005 @AC-019 @FR-014 @US2-AS9 | passed | 00:00:00.1451105 |
| F004 | A goal name has at most 60 characters after trimming(n: "02", name: "a text of 61 characters", result: "\"The name can have at most 60 characters.\"", __pickleIndex: "58", exampleTags: []) | @F004 @AC-011 @FR-006 @FR-009 | passed | 00:00:00.1864801 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "02", actingUser: "the anonymous visitor", habit: "`A016 02 Old habit`", __pickleIndex: "45", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1767923 |
| F005 | Recording checks the acting user, the goal, the date, the value and the note in that order(n: "04", actor: "`f5.ac010.owner.<n>@example.com`", date: "2026-11-17", value: "no value", note: "a text of 501 characters", result: "\"Nothing is scheduled on this date.\"", __pickleIndex: "36", exampleTags: []) | @F005 @AC-010 @FR-005 @FR-018 @US1-AS7 | passed | 00:00:00.2565369 |
| F007 | demo5 has one result Missed this week on every weekday(n: "07", run: "2026-11-22T10:00:00Z", date: "2026-11-19", __pickleIndex: "98", exampleTags: []) | @F007 @AC-053 @FR-032 @FR-012 @US8-AS11 | passed | 00:00:00.6103775 |
| F007 | A per-week or per-day result counts only from the end of its grace period(n: "02", settings: "Build, yes/no, 1 time per week", created: "2026-11-02T08:00:00Z", start: "2026-11-02", records: "nothing", read: "2026-11-11T00:00:00Z", consistency: "\"0% (0 of 1)\"", current: "\"0 weeks\"", first: "2026-11-02 – 2026-11-08 Missed", __pickleIndex: "113", exampleTags: []) | @F007 @AC-062 @FR-001 @FR-003 @FR-008 | passed | 00:00:00.1165837 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "13", settings: "Reduce, duration 30 minutes, 1 time per day", records: "20 and 11 for 2026-11-18", state: "Failed", progress: "progress total 31 of at most 30", __pickleIndex: "249", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1015509 |
| F002 | Only the 24 habit icons are accepted(n: "19", icon: "`wine-off`", result: "success", __pickleIndex: "182", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0802282 |
| F007 | demo5's Strength training and Screen time goals on other weekdays(n: "03", run: "2026-11-22T10:00:00Z", strength: "\"81% (9 of 11)\"", current: "\"3 weeks\"", longest: "\"4 weeks\"", __pickleIndex: "87", exampleTags: []) | @F007 @AC-049 @FR-032 | passed | 00:00:00.6107741 |
| F001 | Administrator creation finds an existing administrator before it checks the password rule | @F001 @AC-047 @FR-014 @US4-AS8 | passed | 00:00:00.0797161 |
| F005 | Recording checks the acting user, the goal, the date, the value and the note in that order(n: "03", actor: "`f5.ac010.other.<n>@example.com`", date: "2026-11-17", value: "no value", note: "a text of 501 characters", result: "\"This goal does not exist.\"", __pickleIndex: "35", exampleTags: []) | @F005 @AC-010 @FR-005 @FR-018 @US1-AS7 | passed | 00:00:00.2590583 |
| F002 | Field checks come in a fixed order(n: "01", fields: "the name `\"   \"`, no category and no icon", result: "\"Enter a name.\"", __pickleIndex: "79", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0883030 |
| F005 | A final Reduce count commitment is Met at or below its limit and Failed above it(n: "05", count: "count 0 `cigarettes`", value: "0", state: "Met", __pickleIndex: "60", exampleTags: []) | @F005 @AC-015 @FR-014 @US2-AS5 | passed | 00:00:00.1098542 |
| F002 | A custom habit is not a catalog habit for the administrator(n: "03", action: "publishes", __pickleIndex: "106", exampleTags: []) | @F002 @AC-033 @FR-013 @FR-024 @US3-AS13 | passed | 00:00:00.1642848 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "13", settings: "Reduce, yes/no, 3 times per week", targetText: "\"at most 3 times a week\"", __pickleIndex: "99", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0954062 |
| F002 | A registered user deletes their custom habit | @F002 @AC-050 @FR-020 @US4-AS4 | passed | 00:00:00.0919695 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "20", settings: "Reduce, yes/no, fixed days on all seven days", targetText: "\"avoided every day\"", __pickleIndex: "106", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0977470 |
| F001 | Demo loading without a demo password creates no account(demoPassword: "absent", __pickleIndex: "84", exampleTags: []) | @F001 @AC-055 @FR-016 @US7-AS3 | passed | 00:00:00.3111754 |
| F001 | The block ends exactly 5 minutes after the fifth failure | @F001 @AC-016 @FR-008 @US2-AS5 | passed | 00:00:00.2868999 |
| F004 | Each field refusal of goal creation, with its message(n: "01", settings: "no Build or Reduce, count 20 `pages`, 3 times per "···, message: "\"Choose Build or Reduce.\"", __pickleIndex: "8", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1701501 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "03", action: "deletes `Gone` by its identifier", __pickleIndex: "233", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.1757540 |
| F005 | Progress is given at any state, as the goal's kind of target counts it(n: "04", settings: "Reduce, yes/no, 1 time per day", records: "Yes, Yes and No for 2026-11-18", progress: "progress 2 of at most 1", __pickleIndex: "170", exampleTags: []) | @F005 @AC-044 @FR-014 @US5-AS2 | passed | 00:00:00.1101432 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "01", measurement: "count 5 `pages`", before: "5", change: "the record of 2026-11-18 to no value", result: "the result is \"Enter a whole number from 0 to 999"···, __pickleIndex: "225", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.1087616 |
| F004 | Each field refusal of goal creation, with its message(n: "06", settings: "Build, count 0 `pages`, 3 times per week", message: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "13", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1569239 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "13", goal: "on `A4.008 13 Reading` with Build, yes/no, fixed d"···, message: "\"The start date cannot be in the past.\"", __pickleIndex: "42", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1642712 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "02", name: "`\"\"`", description: "none", category: "Body", intent: "Build", icon: "`book-open`", result: "\"Enter a name.\"", __pickleIndex: "71", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0828485 |
| F004 | A named time zone is kept as given, and the same time zone changes nothing(n: "02", value: "`Europe/Kiev`", later: "a later version `Europe/Kiev` from 2026-11-19", __pickleIndex: "258", exampleTags: []) | @F004 @AC-075 @FR-024 @FR-026 | passed | 00:00:00.0949990 |
| F004 | A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "04", schedule: "1 time per day", date: "2027-10-31", entry: "2027-10-31 from `2027-10-30T21:00:00Z` to `2027-10"···, __pickleIndex: "167", exampleTags: []) | @F004 @AC-040 @FR-012 @FR-013 @US4-AS5 | passed | 00:00:00.0919277 |
| F005 | Demo history loading run again leaves the existing demo account with history unchanged | @F005 @AC-057 @FR-030 @US7-AS3 | passed | 00:00:00.4701741 |
| F005 | Around a clock change the grace period lasts exactly 48 elapsed hours(n: "01", createdAt: "`2026-10-24T08:00:00Z`", date: "2026-10-24", time: "`2026-10-26T20:59:59Z`", start: "`2026-10-23T21:00:00Z`", end: "`2026-10-24T21:00:00Z`", entry: "Pending, grace ends `2026-10-26T21:00:00Z` (23:00 "···, __pickleIndex: "102", exampleTags: []) | @F005 @AC-023 @FR-016 @US2-AS13 | passed | 00:00:00.1213758 |
| F005 | Demo history loading without a demo password creates no account(n: "03", demoPassword: "`\"   \"`", __pickleIndex: "222", exampleTags: []) | @F005 @AC-061 @FR-030 @US7-AS6 | passed | 00:00:00.3960995 |
| F004 | An account name is given only to its own account; administrators have their own | @F004 @AC-090 @FR-028 @FR-029 @US11-AS7 | passed | 00:00:00.2753535 |
| F001 | Registration refuses an email that is not valid(email: "\"\"", whyItIsNotValid: "empty", __pickleIndex: "19", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002465 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "05", value: "`EST`", __pickleIndex: "253", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.0921045 |
| F005 | Another registered user gets "This goal does not exist." from every use case of this slice(n: "02", useCase: "change, with the owner's record identifier", goal: "the goal of `f5.ac047.owner.<n>@example.com`", __pickleIndex: "184", exampleTags: []) | @F005 @AC-047 @FR-017 @FR-018 @US6-AS1 | passed | 00:00:00.1964294 |
| F005 | The history's range has 1 to 366 days and is checked after the goal(n: "05", actor: "`f5.ac043.other.<n>@example.com`", first: "2026-11-16", last: "2027-11-17", result: "the result is \"This goal does not exist.\"", __pickleIndex: "165", exampleTags: []) | @F005 @AC-043 @FR-009 @FR-018 @US5-AS4 | passed | 00:00:00.1803865 |
| F004 | An anonymous visitor gets no goal use case(n: "01", action: "starts a new goal from `A4.032 01 Reading`", __pickleIndex: "135", exampleTags: []) | @F004 @AC-032 @FR-022 @US3-AS4 | passed | 00:00:00.1889565 |
| F005 | What can be recorded now is every Due and every Pending commitment or period, newest first(n: "04", schedule: "fixed days Monday, Wednesday and Friday", startDate: "2026-11-16", time: "`2026-11-21T10:00:00Z`", entries: "exactly one entry: 2026-11-20, Pending, grace ends"···, __pickleIndex: "177", exampleTags: []) | @F005 @AC-045 @FR-011 @US5-AS3 | passed | 00:00:00.1307724 |
| F005 | Around a clock change the grace period lasts exactly 48 elapsed hours(n: "04", createdAt: "`2027-03-27T08:00:00Z`", date: "2027-03-27", time: "`2027-03-29T22:00:00Z`", start: "`2027-03-26T22:00:00Z`", end: "`2027-03-27T22:00:00Z`", entry: "Missed, no grace end", __pickleIndex: "105", exampleTags: []) | @F005 @AC-023 @FR-016 @US2-AS13 | passed | 00:00:00.0973020 |
| F004 | Use cases check the acting user, the role, the identifier and the fields, in this order(n: "03", actingUser: "`f4.ac035.user.03@example.com`", action: "reads the evaluation calendar from 2026-11-24 to 2"···, message: "\"This goal does not exist.\"", __pickleIndex: "149", exampleTags: []) | @F004 @AC-035 @FR-023 | passed | 00:00:00.1645839 |
| F005 | After a move east, the old and the new day can both be recorded while they overlap | @F005 @AC-067 @FR-002 @FR-011 @US5-AS3 | passed | 00:00:00.1022401 |
| F005 | A date that belongs to no commitment or period cannot be recorded(n: "04", schedule: "3 times per week", createdAt: "`2026-11-18T10:00:00Z`", startDate: "2026-11-18", date: "2026-11-18", why: "before the first week, 2026-11-23 to 2026-11-29", __pickleIndex: "7", exampleTags: []) | @F005 @AC-005 @FR-002 @US1-AS5 | passed | 00:00:00.1018550 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "02", browserTimeZone: "no browser time zone", timeZone: "`UTC`", __pickleIndex: "223", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0862319 |
| F002 | Custom habits never appear in browsing, for any audience(n: "04", actingUser: "the anonymous visitor", __pickleIndex: "17", exampleTags: []) | @F002 @AC-006 @FR-024 @US1-AS6 | passed | 00:00:00.2462217 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "08", actingUser: "the administrator `ac016.admin.08@example.com`", habit: "`A016 08 Old habit`", __pickleIndex: "51", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1928245 |
| F004 | Creating a goal with changed values keeps the changed values | @F004 @AC-006 @FR-002 @FR-010 @US1-AS6 | passed | 00:00:00.1858882 |
| F005 | A per-week period holds one record per recording, listed in date order | @F005 @AC-003 @FR-003 @FR-009 @FR-014 @US1-AS3 | passed | 00:00:00.1182323 |
| F007 | A per-week goal has one bar per week, the last 12, with the week's progress | @F007 @AC-024 @FR-006 @US4-AS4 | passed | 00:00:00.1922535 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "16", settings: "Reduce, yes/no, 2 times per week", records: "Yes for 2026-11-16, Yes for 2026-11-17 and No for "···, state: "Met", progress: "progress 2 of at most 2", __pickleIndex: "252", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1025735 |
| F007 | A Failed result resets the current streak | @F007 @AC-009 @FR-003 @FR-004 @US2-AS1 | passed | 00:00:00.1640295 |
| F004 | Another user's goal does not exist for them(n: "03", action: "reads the goal", goal: "an identifier that the test creates and that belon"···, __pickleIndex: "126", exampleTags: []) | @F004 @AC-029 @FR-020 @US3-AS1 | passed | 00:00:00.2456269 |
| F002 | Field checks come in a fixed order(n: "08", fields: "the name `A029 08 List`, the category Body, the in"···, result: "\"Mark one suggested starting goal for the card.\"", __pickleIndex: "86", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0860237 |
| F004 | Each field refusal of goal creation, with its message(n: "11", settings: "Reduce, duration with no amount and the unit minut"···, message: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "18", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1598143 |
| F002 | For custom habits the acting user and ownership come first, the name check last(n: "04", actingUser: "the registered user `ac057.a.04@example.com`", action: "creates a custom habit with the name `taken` and n"···, result: "\"Choose a category.\"", __pickleIndex: "229", exampleTags: []) | @F002 @AC-057 @FR-005 @FR-016 @US4-AS11 | passed | 00:00:00.1746947 |
| F007 | Every streak carries the unit of its goal, singular for 1(n: "07", settings: "Build, yes/no, fixed days on all seven days", created: "2026-11-01T08:00:00Z", start: "2026-11-01", yes: "2026-11-03", read: "2026-11-06T10:00:00Z", streak: "\"1 commitment\"", number: "1", unit: "\"commitment\"", __pickleIndex: "29", exampleTags: []) | @F007 @AC-014 @FR-004 @US2-AS6 | passed | 00:00:00.1070748 |
| F007 | A goal with no final result has the consistency "N/A"(n: "02", created: "2026-11-21T08:00:00Z", start: "2026-11-21", recorded: "2026-11-21 to 2026-11-23", note: "2026-11-21 and 2026-11-22 Pending, 2026-11-23 Due", __pickleIndex: "3", exampleTags: []) | @F007 @AC-003 @FR-002 @US1-AS3 | passed | 00:00:00.1152784 |
| F004 | The calendar range has 1 to 366 days(n: "03", from: "2026-11-18", to: "2027-11-18", result: "a calendar of 53 commitments, the first on 2026-11"···, __pickleIndex: "174", exampleTags: []) | @F004 @AC-043 @FR-013 @US4-AS7 | passed | 00:00:00.1047301 |
| F005 | A day that has not begun on the goal's timeline cannot be recorded(n: "06", timeZone: "Europe/Kyiv", schedule: "fixed days on all seven days", time: "`2026-11-18T21:59:59Z`", date: "2026-11-19", result: "the result is \"You cannot record a day that has n"···, __pickleIndex: "14", exampleTags: []) | @F005 @AC-006 @FR-002 @US1-AS6 | passed | 00:00:00.1021920 |
| F005 | A note saved on its own replaces the note, and an empty one removes it(n: "01", note: "`\"\"`", result: "no note", __pickleIndex: "134", exampleTags: []) | @F005 @AC-036 @FR-007 @US4-AS2 | passed | 00:00:00.0975287 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "21", second: "Build, duration 15 minutes, 21 times per week", result: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "136", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0918101 |
| F005 | The history's range has 1 to 366 days and is checked after the goal(n: "06", actor: "the anonymous visitor", first: "2026-11-16", last: "2027-11-17", result: "the result is \"You are not signed in.\"", __pickleIndex: "166", exampleTags: []) | @F005 @AC-043 @FR-009 @FR-018 @US5-AS4 | passed | 00:00:00.1768723 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "02", action: "edits `Gone` by its identifier, with the name `Bac"···, __pickleIndex: "232", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.1764815 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"\"", password: "\"curator2026\"", __pickleIndex: "66", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0001892 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "07", goal: "on `A4.008 07 Reading` with Build, count 20 with a"···, message: "\"The unit can have at most 20 characters.\"", __pickleIndex: "36", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1685751 |
| F004 | Anonymous visitors and administrators get the catalog without goal information(n: "04", actingUser: "`f4.ac055.admin.04@example.com`", action: "reads the details of `A4.055 04 Reading` as `f4.ac"···, result: "the details have no goal count and no goals", __pickleIndex: "198", exampleTags: []) | @F004 @AC-055 @FR-032 @US6-AS4 | passed | 00:00:00.1704506 |
| F005 | Any begun date of a week names that week's one note(n: "05", action: "saves the note a text of 501 characters for 2026-1"···, result: "the result is \"You cannot record a day that has n"···, __pickleIndex: "156", exampleTags: []) | @F005 @AC-039 @FR-002 @FR-007 @US4-AS5 | passed | 00:00:00.0932359 |
| F004 | Each field refusal of goal creation, with its message(n: "21", settings: "Build, yes/no, 3 times with no period", message: "\"Choose per day or per week.\"", __pickleIndex: "28", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1850506 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "19", second: "Build, duration 15 minutes, fixed days with no day", result: "\"Choose at least one day.\"", __pickleIndex: "134", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0982498 |
| F002 | Only the 24 habit icons are accepted(n: "08", icon: "`moon`", result: "success", __pickleIndex: "171", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0830128 |
| F007 | The dashboard lists every goal with its consistency and streaks in the overview order | @F007 @AC-035 @FR-012 @US6-AS5 | passed | 00:00:00.1256817 |
| F005 | A note has at most 500 characters(n: "04", action: "records 20 for 2026-11-18 with the note a text of "···, result: "recording succeeds with the state Due, not a corre"···, __pickleIndex: "149", exampleTags: []) | @F005 @AC-038 @FR-005 @FR-007 @US4-AS4 | passed | 00:00:00.1001102 |
| F004 | Several goals on one habit are independent | @F004 @AC-017 @FR-001 @US1-AS10 | passed | 00:00:00.2056308 |
| F004 | "Today" is the date in the user's own time zone(n: "02", timeZone: "`Europe/Kyiv`", time: "`2026-11-16T22:30:00Z`", startDate: "2026-11-17", result: "\"The start date cannot be in the past.\"", __pickleIndex: "177", exampleTags: []) | @F004 @AC-044 @FR-006 @FR-007 @FR-012 @US4-AS8 | passed | 00:00:00.1048974 |
| F004 | The demo shows a goal on an Unpublished habit, a goal with its own name and an account name | @F004 @AC-069 @FR-028 @FR-034 @FR-040 @US9-AS5 | passed | 00:00:00.5049863 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "19", settings: "Reduce, yes/no, fixed days Saturday and Sunday", targetText: "\"avoided on Sat, Sun\"", __pickleIndex: "105", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0945495 |
| F004 | Creating a goal with the starting values unchanged | @F004 @AC-005 @FR-001 @FR-002 @FR-005 @FR-015 @US1-AS5 | passed | 00:00:00.1987956 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "08", settings: "Build, duration 2 hours, 3 times per day", targetText: "\"2 h, 3 times a day\"", __pickleIndex: "94", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0992907 |
| F004 | A suggested starting goal position the habit does not have is refused(n: "01", position: "4", __pickleIndex: "4", exampleTags: []) | @F004 @AC-004 @FR-007 @US1-AS4 | passed | 00:00:00.1658773 |
| F001 | Administrator creation without an email or a password configures nothing(email: "absent", password: "\"curator2026\"", __pickleIndex: "65", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0001978 |
| F007 | An anonymous visitor and an administrator are refused by every use case of this slice(n: "05", actor: "the administrator `f7.ac041.admin.05@example.com`", useCase: "the goal report for the goal of `f7.ac041.owner.05"···, result: "\"Goals are for registered users.\"", __pickleIndex: "69", exampleTags: []) | @F007 @AC-041 @FR-015 @FR-007 @US7-AS3 @US7-AS4 | passed | 00:00:00.1665933 |
| F005 | A note has at most 500 characters(n: "06", action: "records 20 for 2026-11-18 with the note a padded t"···, result: "recording succeeds with the state Due, not a corre"···, __pickleIndex: "151", exampleTags: []) | @F005 @AC-038 @FR-005 @FR-007 @US4-AS4 | passed | 00:00:00.0919148 |
| F004 | A custom habit that no goal uses is deleted | @F004 @AC-049 @FR-019 @FR-035 @US5-AS4 | passed | 00:00:00.1002215 |
| F002 | The category filter counts only Published catalog habits(n: "03", actingUser: "the administrator `ac009.admin.03@example.com`", __pickleIndex: "22", exampleTags: []) | @F002 @AC-009 @FR-002 @FR-009 @FR-024 @US1-AS9 | passed | 00:00:00.3606616 |
| F002 | Demo loading run again leaves the demo accounts and their custom habits unchanged | @F002 @AC-065 @FR-031 @US6-AS6 | passed | 00:00:00.4748641 |
| F001 | Registration refuses an email that is not valid(email: "\"@example.com\"", whyItIsNotValid: "nothing before the \"@\"", __pickleIndex: "18", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0004551 |
| F002 | Browsing by intent returns only that intent(n: "01", intent: "Reduce", sugar: "contains", water: "does not contain", __pickleIndex: "5", exampleTags: []) | @F002 @AC-004 @FR-009 @US1-AS4 | passed | 00:00:00.0841816 |
| F005 | A final Reduce count commitment is Met at or below its limit and Failed above it(n: "06", count: "count 0 `cigarettes`", value: "1", state: "Failed", __pickleIndex: "61", exampleTags: []) | @F005 @AC-015 @FR-014 @US2-AS5 | passed | 00:00:00.1093775 |
| F002 | A registered user creates a custom habit | @F002 @AC-047 @FR-001 @FR-020 @US4-AS1 | passed | 00:00:00.0978277 |
| F007 | Reduce results count like Build results(n: "02", settings: "Reduce, yes/no, fixed days on all seven days", nov1: "No", nov2: "Yes", __pickleIndex: "10", exampleTags: []) | @F007 @AC-006 @FR-001 @US1-AS5 | passed | 00:00:00.0994426 |
| F002 | Field checks refuse a custom habit with the same messages(n: "04", name: "`No category`", description: "none", category: "none", intent: "Build", icon: "`droplet`", result: "\"Choose a category.\"", __pickleIndex: "222", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0846277 |
| F005 | A missing record added to a Failed week is a correction that makes it Met | @F005 @AC-028 @FR-006 @FR-014 @US3-AS4 | passed | 00:00:00.0990674 |
| F005 | The lowest and highest amounts are accepted and kept(n: "05", measurement: "duration 1 in hours", value: "1440", __pickleIndex: "29", exampleTags: []) | @F005 @AC-008 @FR-004 @FR-005 | passed | 00:00:00.0998213 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "09", browserTimeZone: "the browser time zone `+02:00`", timeZone: "`UTC`", __pickleIndex: "230", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0843195 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@example\"", whyItIsNotValid: "domain has one part", __pickleIndex: "9", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0004369 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "08", second: "Build, duration with no amount and the unit minute"···, result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "123", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0952266 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "08", goal: "on `A4.008 08 Reading` with Build, duration 15 wit"···, message: "\"Choose minutes or hours.\"", __pickleIndex: "37", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1833557 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "05", count: "5", marks: "the third marked", result: "success", after: "5 suggested starting goals, the third marked", __pickleIndex: "113", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0949967 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "03", fields: "Reduce, count 1 `sweet`, 1 time per day", text: "\"at most 1 sweet a day\"", kind: "\"Count · flexible\"", __pickleIndex: "30", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0956148 |
| F005 | An administrator gets "Goals are for registered users." from every use case of this slice(n: "03", useCase: "delete, with the owner's record identifier", __pickleIndex: "196", exampleTags: []) | @F005 @AC-049 @FR-018 @US6-AS3 | passed | 00:00:00.1953575 |
| F001 | The anonymous visitor cannot change a password, whatever the new password(newPassword: "\"run\"", whyItMatters: "breaks the password rule, which is checked only fo"···, __pickleIndex: "56", exampleTags: []) | @F001 @AC-036 @FR-011 @FR-013 @US6-AS5 | passed | 00:00:00.0024352 |
| F005 | Another registered user gets "This goal does not exist." from every use case of this slice(n: "08", useCase: "record", goal: "a goal identifier that belongs to no goal", __pickleIndex: "190", exampleTags: []) | @F005 @AC-047 @FR-017 @FR-018 @US6-AS1 | passed | 00:00:00.1835638 |
| F005 | A final Build duration commitment is Met at or above its target and Failed below it(n: "02", duration: "duration 20 minutes", value: "20", state: "Met", progress: "progress 20 of 20 minutes", __pickleIndex: "49", exampleTags: []) | @F005 @AC-014 @FR-004 @FR-014 @US2-AS4 | passed | 00:00:00.1008495 |
| F004 | An anonymous visitor gets no goal use case(n: "06", action: "browses the catalog as the anonymous visitor with "···, __pickleIndex: "140", exampleTags: []) | @F004 @AC-032 @FR-022 @US3-AS4 | passed | 00:00:00.1672693 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "06", count: "0", marks: "none marked", result: "success", after: "no suggested starting goal", __pickleIndex: "114", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0923044 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "13", second: "Reduce, count with no amount and the unit `sweets`"···, result: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "128", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0839112 |
| F001 | Registration refuses an email that is not valid(email: "\"ana @example.com\"", whyItIsNotValid: "a space inside", __pickleIndex: "16", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002567 |
| F007 | The dashboard and the overview contain only the acting user's goals | @F007 @AC-040 @FR-014 @US7-AS2 | passed | 00:00:00.1901265 |
| F001 | A signed-in user changes their password | @F001 @AC-031 @FR-013 @US6-AS1 | passed | 00:00:00.2876491 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "12", browserTimeZone: "the browser time zone `Europe/Kiev`", timeZone: "`Europe/Kiev`", __pickleIndex: "233", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0806188 |
| F005 | Any begun date of a week names that week's one note(n: "02", action: "saves the note \"Week note.\" for 2026-11-16 and t"···, result: "both succeed and the history from 2026-11-16 to 20"···, __pickleIndex: "153", exampleTags: []) | @F005 @AC-039 @FR-002 @FR-007 @US4-AS5 | passed | 00:00:00.1041622 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "14", goal: "on `A4.008 14 Reading` with Build, yes/no, fixed d"···, message: "\"Choose a start date within a year from today.\"", __pickleIndex: "43", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1707320 |
| F004 | The Home greeting follows the time of day in the account's time zone(n: "02", time: "`2026-11-18T03:00:00Z` (05:00 in Kyiv)", greeting: "\"Good morning, Yuri\"", __pickleIndex: "279", exampleTags: []) | @F004 @AC-086 @FR-029 @US11-AS3 | passed | 00:00:00.0905495 |
| F007 | demo5's reports on a Wednesday run(n: "02", goal: "\"Strength training, 2 times a week\"", consistency: "\"81% (9 of 11)\"", current: "\"3 weeks\"", longest: "\"4 weeks\"", bars: "12", __pickleIndex: "83", exampleTags: []) | @F007 @AC-048 @FR-032 @US8-AS8 | passed | 00:00:00.6498842 |
| F002 | Only the 24 habit icons are accepted(n: "24", icon: "`tv`", result: "success", __pickleIndex: "187", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0880558 |
| F005 | A final per-day Reduce count period is Met when its total is within the limit(n: "03", records: "0 for 2026-11-18", state: "Met", progress: "progress total 0 of at most 1", __pickleIndex: "77", exampleTags: []) | @F005 @AC-019 @FR-014 @US2-AS9 | passed | 00:00:00.0952239 |
| F007 | Each commitment, day or week counts once, however many records it has(n: "03", settings: "Build, yes/no, 2 times per day", created: "2026-11-01T08:00:00Z", start: "2026-11-01", records: "`Yes` twice for 2026-11-01, `Yes` once for 2026-11"···, read: "2026-11-06T10:00:00Z", text: "\"66% (2 of 3)\"", __pickleIndex: "13", exampleTags: []) | @F007 @AC-007 @FR-001 @US1-AS6 | passed | 00:00:00.1636378 |
| F002 | Only the 24 habit icons are accepted(n: "21", icon: "`receipt`", result: "success", __pickleIndex: "184", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0851108 |
| F001 | Sign-in with an email that has no account fails with the single message | @F001 @AC-012 @FR-007 @US2-AS2 | passed | 00:00:00.0033903 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "22", settings: "Reduce, yes/no, fixed days Sunday and Monday", targetText: "\"avoided on Mon, Sun\"", __pickleIndex: "108", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0952017 |
| F001 | Registration refuses an email that already has an account, in any case and with spaces | @F001 @AC-007 @FR-003 @US1-AS2 | passed | 00:00:00.1256188 |
| F007 | A goal that has not started has streaks of 0, no recent results and no trend(n: "01", settings: "Build, yes/no, 1 time per day", start: "2026-11-25", streak: "\"0 days\"", __pickleIndex: "32", exampleTags: []) | @F007 @AC-015 @FR-004 @FR-005 @FR-006 @US2-AS7 | passed | 00:00:00.0891482 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "22", settings: "Build, duration 20 minutes, 3 times per week", records: "21 for 2026-11-16, 21 for 2026-11-17 and 21 for 20"···, state: "Met", progress: "progress 3 of 3 times", __pickleIndex: "258", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1013834 |
| F002 | Demo loading run again leaves the seed catalog unchanged | @F002 @AC-061 @FR-030 @US6-AS2 | passed | 00:00:00.4461973 |
| F007 | Each commitment, day or week counts once, however many records it has(n: "01", settings: "Build, yes/no, 3 times per week", created: "2026-10-05T08:00:00Z", start: "2026-10-05", records: "`Yes` for 2026-10-05, 2026-10-06, 2026-10-07, 2026"···, read: "2026-11-11T10:00:00Z", text: "\"60% (3 of 5)\"", __pickleIndex: "11", exampleTags: []) | @F007 @AC-007 @FR-001 @US1-AS6 | passed | 00:00:00.1683288 |
| F007 | demo4's Yoga goal reads its weeks by the weekday of the run(n: "03", run: "2026-11-17T10:00:00Z", consistency: "\"0% (0 of 2)\"", current: "\"0 weeks\"", longest: "\"0 weeks\"", __pickleIndex: "77", exampleTags: []) | @F007 @AC-044 @FR-030 @US8-AS3 | passed | 00:00:00.6201350 |
| F004 | A Reduce goal with an amount has 1 time per period(n: "02", settings: "Reduce, duration 30 minutes, 2 times per week", result: "\"A Reduce goal with an amount has 1 time per peri"···, __pickleIndex: "61", exampleTags: []) | @F004 @AC-012 @FR-004 @FR-006 | passed | 00:00:00.1694337 |
| F004 | Goals are listed by name ignoring case, then in creation order | @F004 @AC-022 @FR-009 @FR-015 | passed | 00:00:00.1065717 |
| F001 | Sign-in ignores spaces around the email | @F001 @AC-011 @FR-006 | passed | 00:00:00.0902669 |
| F005 | A note given with a record is the note; no note leaves it unchanged(n: "06", action: "changes the record of 2026-11-16 to No with the no"···, note: "the note \"Better today.\"", __pickleIndex: "143", exampleTags: []) | @F005 @AC-037 @FR-007 @US4-AS3 | passed | 00:00:00.1131583 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "32", settings: "Build, duration 20 minutes, 3 times per week", records: "60 for 2026-11-16", state: "Failed", progress: "progress 1 of 3 times", __pickleIndex: "268", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0976878 |
| F005 | A day that has not begun on the goal's timeline cannot be recorded(n: "02", timeZone: "UTC", schedule: "1 time per day", time: "`2026-11-18T10:00:00Z`", date: "2026-11-19", result: "the result is \"You cannot record a day that has n"···, __pickleIndex: "10", exampleTags: []) | @F005 @AC-006 @FR-002 @US1-AS6 | passed | 00:00:00.0936862 |
| F005 | A final per-week Build count period counts only the records that reach the amount(n: "01", records: "20 for 2026-11-16, 25 for 2026-11-17 and 12 for 20"···, state: "Failed", progress: "progress 2 of 3 times", __pickleIndex: "70", exampleTags: []) | @F005 @AC-018 @FR-014 @US2-AS8 | passed | 00:00:00.1113317 |
| F001 | Registration refuses a password that breaks the password rule(email: "\"short.pw@example.com\"", password: "\"walk4\"", whyItBreaksTheRule: "5 characters", __pickleIndex: "1", exampleTags: []) | @F001 @AC-002 @FR-002 @US1-AS3 @US1-AS4 | passed | 00:00:00.0394987 |
| F004 | A per-week goal has one evaluation period per Monday-to-Sunday week, from its first full week(n: "04", startDate: "2026-11-23", from: "2026-11-23", to: "2026-11-29", periods: "1 period: 2026-11-23 to 2026-11-29 from `2026-11-2"···, __pickleIndex: "159", exampleTags: []) | @F004 @AC-038 @FR-013 @US4-AS3 | passed | 00:00:00.1049493 |
| F001 | Administrator creation finds a registered user whatever the case and spaces of the email | @F001 @AC-063 @FR-003 @FR-014 | passed | 00:00:00.0918674 |
| F005 | A final Build duration commitment is Met at or above its target and Failed below it(n: "06", duration: "duration 1 in hours", value: "59", state: "Failed", progress: "progress 59 of 60 minutes", __pickleIndex: "53", exampleTags: []) | @F005 @AC-014 @FR-004 @FR-014 @US2-AS4 | passed | 00:00:00.0997068 |
| F007 | "This week" follows the account's time zone, also for days of an older version(n: "01", read: "2026-11-15T21:59:59Z", missed: "1", entries: "(the goal, 2026-11-13, Pending, grace ends 2026-11"···, __pickleIndex: "122", exampleTags: []) | @F007 @AC-067 @FR-012 @US6-AS4 | passed | 00:00:00.1577045 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "05", name: "`A027 05 No category`", description: "none", category: "none", intent: "Build", icon: "`book-open`", result: "\"Choose a category.\"", __pickleIndex: "74", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0739136 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "02", measurement: "count 5 `pages`", before: "5", change: "the record of 2026-11-18 to -1", result: "the result is \"Enter a whole number from 0 to 999"···, __pickleIndex: "226", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.1077037 |
| F004 | A per-day goal has one evaluation period per day | @F004 @AC-037 @FR-003 @FR-013 @US4-AS2 | passed | 00:00:00.1005808 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "01", action: "starts a new goal from", habit: "`A4.014 01 Draft`", __pickleIndex: "72", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | passed | 00:00:00.1720772 |
| F004 | A per-week goal tells when its first week starts(n: "02", startDate: "2026-11-18", time: "`2026-11-22T23:59:59Z`", list: "first week starts 2026-11-23, no start date and no"···, __pickleIndex: "161", exampleTags: []) | @F004 @AC-039 @FR-014 @FR-016 @US4-AS4 | passed | 00:00:00.0961286 |
| F004 | The edges of every target range are accepted(n: "08", settings: "Build, duration 999 hours, 1 time per day", targetText: "\"999 h a day\"", __pickleIndex: "51", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | passed | 00:00:00.1694508 |
| F007 | An account that already has the demo5 email, in any case, is left unchanged | @F007 @AC-051 @FR-032 @US8-AS9 | passed | 00:00:00.4696700 |
| F004 | A user with no goals gets an empty list | @F004 @AC-027 @FR-015 @US2-AS5 | passed | 00:00:00.0884520 |
| F005 | A value that does not fit the goal's measurement is refused with its message(n: "06", measurement: "duration 20 minutes", value: "-1", message: "\"Enter a whole number of minutes from 0 to 1440.\"···, __pickleIndex: "22", exampleTags: []) | @F005 @AC-007 @FR-005 @US1-AS7 | passed | 00:00:00.0946787 |
| F004 | A goal on an Unpublished habit keeps working, and no new goal can use that habit(n: "03", actingUser: "`f4.ac060.owner.03@example.com`", action: "starts a new goal from `A4.060 03 Reading`", result: "the result is \"This habit is not in the catalog.\"···, __pickleIndex: "210", exampleTags: []) | @F004 @AC-060 @FR-016 @FR-034 @US7-AS6 | passed | 00:00:00.2964902 |
| F002 | An edit of a Published habit is public on the next read | @F002 @AC-020 @FR-012 @US3-AS3 | passed | 00:00:00.0969904 |
| F005 | Another registered user gets "This goal does not exist." from every use case of this slice(n: "07", useCase: "recent notes", goal: "the goal of `f5.ac047.owner.<n>@example.com`", __pickleIndex: "189", exampleTags: []) | @F005 @AC-047 @FR-017 @FR-018 @US6-AS1 | passed | 00:00:00.1868896 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "25", settings: "Reduce, count 3 `sweets`, 1 time per week", records: "1 for 2026-11-16 and 1 for 2026-11-18", state: "Met", progress: "progress total 2 of at most 3", __pickleIndex: "261", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1036963 |
| F004 | A per-week goal has one evaluation period per Monday-to-Sunday week, from its first full week(n: "01", startDate: "2026-11-18", from: "2026-11-16", to: "2026-12-06", periods: "2 periods: 2026-11-23 to 2026-11-29 from `2026-11-"···, __pickleIndex: "156", exampleTags: []) | @F004 @AC-038 @FR-013 @US4-AS3 | passed | 00:00:00.0932394 |
| F005 | A final Reduce count commitment is Met at or below its limit and Failed above it(n: "01", count: "count 2 `cups`", value: "0", state: "Met", __pickleIndex: "56", exampleTags: []) | @F005 @AC-015 @FR-014 @US2-AS5 | passed | 00:00:00.1041979 |
| F005 | A note saved on its own replaces the note, and an empty one removes it(n: "03", note: "\"New note.\"", result: "the note \"New note.\"", __pickleIndex: "136", exampleTags: []) | @F005 @AC-036 @FR-007 @US4-AS2 | passed | 00:00:00.1001766 |
| F005 | A note given with a record is the note; no note leaves it unchanged(n: "04", action: "records Yes for 2026-11-18 with the note \"Better "···, note: "the note \"Better today.\"", __pickleIndex: "141", exampleTags: []) | @F005 @AC-037 @FR-007 @US4-AS3 | passed | 00:00:00.0964309 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "10", fields: "Build, count 2 `glasses`, 3 times per day", text: "\"2 glasses, 3 times a day\"", kind: "\"Count · flexible\"", __pickleIndex: "37", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0867366 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "11", measurement: "count 5 `pages`", before: "5", change: "the record of 2026-11-18 to -1 with the note a tex"···, result: "the result is \"Enter a whole number from 0 to 999"···, __pickleIndex: "235", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.1075032 |
| F005 | A final per-week Build count period counts only the records that reach the amount(n: "03", records: "19 for 2026-11-16, 19 for 2026-11-17 and 19 for 20"···, state: "Failed", progress: "progress 0 of 3 times", __pickleIndex: "72", exampleTags: []) | @F005 @AC-018 @FR-014 @US2-AS8 | passed | 00:00:00.1023394 |
| F004 | The edges of every target range are accepted(n: "07", settings: "Build, yes/no, 20 times per week", targetText: "\"20 times a week\"", __pickleIndex: "50", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | passed | 00:00:00.1841824 |
| F004 | Anonymous visitors and administrators get the catalog without goal information(n: "03", actingUser: "the anonymous visitor", action: "reads the details of `A4.055 03 Reading` as the an"···, result: "the details have no goal count and no goals", __pickleIndex: "197", exampleTags: []) | @F004 @AC-055 @FR-032 @US6-AS4 | passed | 00:00:00.1791197 |
| F002 | The anonymous visitor cannot manage the catalog(n: "01", action: "lists the catalog habits as an administrator", __pickleIndex: "94", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0850776 |
| F007 | A goal whose catalog habit is Unpublished keeps its reports | @F007 @AC-059 @FR-008 @FR-001 | passed | 00:00:00.2542836 |
| F005 | A change is a correction only from the instant its grace period ends(n: "01", before: "nothing", time: "`2026-11-20T23:59:59Z`", action: "records 20 for 2026-11-18", result: "recording succeeds with the state Pending, not a c"···, __pickleIndex: "110", exampleTags: []) | @F005 @AC-027 @FR-006 | passed | 00:00:00.1223548 |
| F001 | A registered user cannot change another account's password | @F001 @AC-037 @FR-010 @FR-013 @US6-AS6 | passed | 00:00:00.2382982 |
| F007 | The dashboard counts Due, Pending, Missed-this-week results and goals | @F007 @AC-032 @FR-012 @US6-AS1 @US6-AS3 | passed | 00:00:00.1227385 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "08", fields: "Build, count 5 with the unit `\"  Pages  \"` (5 ch"···, storedUnit: "`Pages`", __pickleIndex: "145", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0871144 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "02", search: "`\"MORNING\"`", morning: "contains", evening: "does not contain", __pickleIndex: "9", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.0881845 |
| F004 | A goal that has not started gives its start date | @F004 @AC-024 @FR-013 @FR-014 @US2-AS3 | passed | 00:00:00.1102692 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "09", settings: "Build, duration 20 minutes, 3 times per week", records: "20 for 2026-11-16, 20 for 2026-11-17 and 20 for 20"···, state: "Met", progress: "progress 3 of 3 times", __pickleIndex: "245", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1036327 |
| F001 | A password change accepts a new password at the lower bound of the password rule | @F001 @AC-032 @FR-002 @FR-013 | passed | 00:00:00.2838420 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "08", measurement: "count 5 `pages`", before: "5", change: "the record of 2026-11-18 to 9999", result: "changing succeeds with the state Met, a correction"···, __pickleIndex: "232", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.1423107 |
| F005 | Recording checks the acting user, the goal, the date, the value and the note in that order(n: "02", actor: "the administrator `f5.ac010.admin.<n>@example.com`", date: "2026-11-17", value: "no value", note: "a text of 501 characters", result: "\"Goals are for registered users.\"", __pickleIndex: "34", exampleTags: []) | @F005 @AC-010 @FR-005 @FR-018 @US1-AS7 | passed | 00:00:00.2406439 |
| F004 | The Home greeting follows the time of day in the account's time zone(n: "04", time: "`2026-11-18T10:00:00Z` (12:00 in Kyiv)", greeting: "\"Good afternoon, Yuri\"", __pickleIndex: "281", exampleTags: []) | @F004 @AC-086 @FR-029 @US11-AS3 | passed | 00:00:00.0894089 |
| F001 | Administrator creation checks for a missing password before it looks for a registered user | @F001 @AC-052 @FR-014 | passed | 00:00:00.0989940 |
| F002 | Browsing orders habits by name, ignoring case | @F002 @AC-002 @FR-004 @FR-008 @US1-AS2 | passed | 00:00:00.0930120 |
| F007 | A week's bar begins with its first commitment | @F007 @AC-026 @FR-006 @US4-AS5 | passed | 00:00:00.1059039 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "03", action: "starts a new goal from", habit: "the identifier of their custom habit `Cold shower`", __pickleIndex: "74", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | passed | 00:00:00.1615732 |
| F007 | Records entered late give the same report as records entered on time | @F007 @AC-019 @FR-008 @US3-AS4 | passed | 00:00:00.1542927 |
| F005 | Another registered user gets "This goal does not exist." from every use case of this slice(n: "01", useCase: "record", goal: "the goal of `f5.ac047.owner.<n>@example.com`", __pickleIndex: "183", exampleTags: []) | @F005 @AC-047 @FR-017 @FR-018 @US6-AS1 | passed | 00:00:00.1781680 |
| F004 | Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "05", submitted: "Build, yes/no, 2 times per day, with the fixed day"···, stored: "2 times per day, no fixed days", targetText: "\"2 times a day\"", __pickleIndex: "70", exampleTags: []) | @F004 @AC-013 @FR-002 @FR-006 | passed | 00:00:00.1771396 |
| F005 | A final yes/no commitment is Met on Yes for Build and on No for Reduce(n: "01", intent: "Build", value: "Yes", state: "Met", __pickleIndex: "62", exampleTags: []) | @F005 @AC-016 @FR-014 @US2-AS6 | passed | 00:00:00.1004569 |
| F005 | The anonymous visitor gets "You are not signed in." from every use case of this slice(n: "04", useCase: "save note", __pickleIndex: "204", exampleTags: []) | @F005 @AC-050 @FR-018 @US6-AS4 | passed | 00:00:00.1079167 |
| F004 | A Reduce goal with an amount has 1 time per period(n: "05", settings: "Build, count 20 `pages`, 3 times per day", result: "creating succeeds", __pickleIndex: "64", exampleTags: []) | @F004 @AC-012 @FR-004 @FR-006 | passed | 00:00:00.1819044 |
| F005 | A change is a correction only from the instant its grace period ends(n: "02", before: "nothing", time: "`2026-11-21T00:00:00Z`", action: "records 20 for 2026-11-18", result: "recording succeeds with the state Met, a correctio"···, __pickleIndex: "111", exampleTags: []) | @F005 @AC-027 @FR-006 | passed | 00:00:00.0975440 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "03", value: "`Etc/GMT+3`", __pickleIndex: "251", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.0957934 |
| F001 | Administrator creation without an email or a password configures nothing(email: "absent", password: "absent", __pickleIndex: "63", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0002327 |
| F005 | A final per-day yes/no period counts its Yes records(n: "06", settings: "Build, yes/no, 2 times per day", records: "Yes for 2026-11-18 and No for 2026-11-18", state: "Failed", progress: "progress 1 of 2 times", __pickleIndex: "84", exampleTags: []) | @F005 @AC-020 @FR-014 @US2-AS10 | passed | 00:00:00.0955794 |
| F002 | A registered user edits every field of their custom habit | @F002 @AC-049 @FR-004 @FR-020 @US4-AS3 | passed | 00:00:00.1048239 |
| F007 | demo5's amounts and notes on a Wednesday run | @F007 @AC-066 @FR-032 @FR-006 @FR-012 @US8-AS8 | passed | 00:00:00.5393615 |
| F002 | Custom-habit names and descriptions are trimmed before they are checked and stored(n: "01", name: "`\"   Cold shower   \"`", description: "`\"  In the morning.  \"`", storedName: "`Cold shower`", storedDescription: "the description \"In the morning.\"", __pickleIndex: "256", exampleTags: []) | @F002 @AC-073 @FR-004 @FR-005 | passed | 00:00:00.0828509 |
| F004 | Browsing gives a registered user their goal count per habit and the tracked count(n: "02", search: "`A4.052 02 R`", result: "the result is `A4.052 02 Reading` only, with the g"···, __pickleIndex: "189", exampleTags: []) | @F004 @AC-052 @FR-031 @US6-AS1 | passed | 00:00:00.1878865 |
| F004 | Browsing gives a registered user their goal count per habit and the tracked count(n: "03", search: "`A4.052 03 Y`", result: "the result is `A4.052 03 Yoga` only, with the goal"···, __pickleIndex: "190", exampleTags: []) | @F004 @AC-052 @FR-031 @US6-AS1 | passed | 00:00:00.2231818 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"   \"", password: "\"curator2026\"", __pickleIndex: "67", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0008625 |
| F005 | The history's range has 1 to 366 days and is checked after the goal(n: "04", actor: "`f5.ac043.owner.<n>@example.com`", first: "2026-11-19", last: "2026-11-18", result: "the result is \"Choose a range of 1 to 366 days.\"", __pickleIndex: "164", exampleTags: []) | @F005 @AC-043 @FR-009 @FR-018 @US5-AS4 | passed | 00:00:00.1832537 |
| F004 | Editing a custom habit renames its goals but never changes their settings | @F004 @AC-050 @FR-010 @FR-035 @US5-AS5 | passed | 00:00:00.1069555 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "09", goal: "on `A4.008 09 Reading` with Build, yes/no, fixed d"···, message: "\"Choose at least one day.\"", __pickleIndex: "38", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1669086 |
| F002 | A registered user cannot manage the catalog(n: "04", action: "edits `A030 04 Draft` to the name `A030 04 Renamed"···, __pickleIndex: "91", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1911376 |
| F005 | A value that does not fit the goal's measurement is refused with its message(n: "05", measurement: "duration 20 minutes", value: "no value", message: "\"Enter a whole number of minutes from 0 to 1440.\"···, __pickleIndex: "21", exampleTags: []) | @F005 @AC-007 @FR-005 @US1-AS7 | passed | 00:00:00.1014010 |
| F004 | Reading a goal gives the version of its current date and the one later version(n: "05", schedule: "3 times per week", startDate: "2026-11-16", time: "`2026-11-18T12:00:00Z`", versions: "the time zone `Europe/Kyiv` from 2026-11-16 and a "···, __pickleIndex: "247", exampleTags: []) | @F004 @AC-073 @FR-016 @FR-026 @US10-AS6 | passed | 00:00:00.1139773 |
| F004 | Without a name the greeting has no name | @F004 @AC-087 @FR-029 @US11-AS4 | passed | 00:00:00.0889720 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "02", fields: "Build, count 999 `pages`, 20 times per week", storedUnit: "`pages`", __pickleIndex: "139", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0888118 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "18", settings: "Reduce, yes/no, 2 times per day", records: "Yes and Yes for 2026-11-18", state: "Met", progress: "progress 2 of at most 2", __pickleIndex: "254", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1051671 |
| F004 | A per-week goal has one evaluation period per Monday-to-Sunday week, from its first full week(n: "03", startDate: "2026-11-18", from: "2026-11-18", to: "2026-11-22", periods: "no period", __pickleIndex: "158", exampleTags: []) | @F004 @AC-038 @FR-013 @US4-AS3 | passed | 00:00:00.1009255 |
| F005 | Another account's record identifier does not exist on one's own goal(n: "02", action: "deletes the record", __pickleIndex: "193", exampleTags: []) | @F005 @AC-048 @FR-017 @US6-AS2 | passed | 00:00:00.1952370 |
| F004 | After a move east, a commitment in progress is still the next one(n: "02", time: "`2026-11-19T04:59:59Z`", next: "2026-11-18", __pickleIndex: "264", exampleTags: []) | @F004 @AC-078 @FR-014 @FR-026 | passed | 00:00:00.0961152 |
| F007 | demo5's dashboard on Wednesday 2026-11-18 | @F007 @AC-054 @FR-032 @FR-011 @FR-012 @US8-AS12 | passed | 00:00:00.5336083 |
| F005 | An ended commitment or period stays Pending during its grace period, even when its records reach the target(n: "03", settings: "duration 20 minutes, fixed days on all seven days", value: "20", time: "`2026-11-21T00:00:00Z`", entry: "Met, no grace end, progress 20 of 20 minutes", __pickleIndex: "88", exampleTags: []) | @F005 @AC-021 @FR-012 @US2-AS11 | passed | 00:00:00.1327010 |
| F004 | A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "06", schedule: "1 time per week", date: "2027-10-31", entry: "2027-10-25 to 2027-10-31 from `2027-10-24T21:00:00"···, __pickleIndex: "169", exampleTags: []) | @F004 @AC-040 @FR-012 @FR-013 @US4-AS5 | passed | 00:00:00.0957514 |
| F004 | Use cases check the acting user, the role, the identifier and the fields, in this order(n: "06", actingUser: "`f4.ac035.user.06@example.com`", action: "starts a new goal from `A4.035 06 Draft` with the "···, message: "\"This habit is not in the catalog.\"", __pickleIndex: "152", exampleTags: []) | @F004 @AC-035 @FR-023 | passed | 00:00:00.1623811 |
| F002 | The role and the identifier are checked before the fields(n: "03", actingUser: "the administrator `ac039.admin.03@example.com`", action: "edits an unknown identifier with the name `\"   \""···, result: "\"This catalog habit does not exist.\"", __pickleIndex: "151", exampleTags: []) | @F002 @AC-039 @FR-016 @US3-AS18 | passed | 00:00:00.1604509 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "02", action: "reads `Secret` by its identifier", __pickleIndex: "208", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1628077 |
| F005 | A value that does not belong to the goal's measurement is not stored(n: "01", measurement: "yes/no", given: "Yes together with the amount 5", stored: "(2026-11-18, Yes), with no amount", __pickleIndex: "30", exampleTags: []) | @F005 @AC-009 @FR-004 | passed | 00:00:00.1000412 |
| F001 | An account registered with spaces around the email has the trimmed email | @F001 @AC-061 @FR-003 @FR-012 | passed | 00:00:00.0856674 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "15", browserTimeZone: "the browser time zone `America/Argentina/Buenos_Ai"···, timeZone: "`America/Argentina/Buenos_Aires`", __pickleIndex: "236", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0837474 |
| F007 | A per-week or per-day result counts only from the end of its grace period(n: "04", settings: "Reduce, count 1 `sweets`, 1 time per day", created: "2026-11-01T08:00:00Z", start: "2026-11-01", records: "0 for 2026-11-01", read: "2026-11-03T23:59:59Z", consistency: "\"N/A\"", current: "\"0 days\"", first: "2026-11-01 Pending", __pickleIndex: "115", exampleTags: []) | @F007 @AC-062 @FR-001 @FR-003 @FR-008 | passed | 00:00:00.1010753 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "06", action: "creates a custom habit with the name `\"   \"` and"···, __pickleIndex: "212", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1885882 |
| F005 | What can be recorded now is every Due and every Pending commitment or period, newest first(n: "06", schedule: "3 times per week", startDate: "2026-11-16", time: "`2026-11-25T00:00:00Z`", entries: "exactly one entry: 2026-11-23 to 2026-11-29, Due", __pickleIndex: "179", exampleTags: []) | @F005 @AC-045 @FR-011 @US5-AS3 | passed | 00:00:00.1300434 |
| F005 | An unrecorded commitment is Pending for 48 hours after its end and then Missed by itself(n: "04", time: "`2026-11-21T00:00:01Z`", entry: "Missed, no grace end, correction", __pickleIndex: "47", exampleTags: []) | @F005 @AC-013 @FR-010 @FR-016 @US2-AS2 @US2-AS3 | passed | 00:00:00.1009627 |
| F005 | A record belongs to the week of its date, not to the week of the instant it is saved(n: "03", time: "`2026-11-23T00:00:00Z`", date: "2026-11-23", state: "Due", firstWeek: "no record", secondWeek: "records (2026-11-23, Yes)", __pickleIndex: "132", exampleTags: []) | @F005 @AC-034 @FR-001 @FR-009 | passed | 00:00:00.0934825 |
| F004 | A per-week goal tells when its first week starts(n: "03", startDate: "2026-11-18", time: "`2026-11-23T00:00:00Z`", list: "no first week starts, no start date and no next co"···, __pickleIndex: "162", exampleTags: []) | @F004 @AC-039 @FR-014 @FR-016 @US4-AS4 | passed | 00:00:00.0982552 |
| F004 | "Today" is the date in the user's own time zone(n: "03", timeZone: "`America/New_York`", time: "`2026-11-17T03:00:00Z`", startDate: "2026-11-16", result: "creating succeeds", __pickleIndex: "178", exampleTags: []) | @F004 @AC-044 @FR-006 @FR-007 @FR-012 @US4-AS8 | passed | 00:00:00.0990706 |
| F004 | The Home greeting follows the time of day in the account's time zone(n: "03", time: "`2026-11-18T09:59:00Z` (11:59 in Kyiv)", greeting: "\"Good morning, Yuri\"", __pickleIndex: "280", exampleTags: []) | @F004 @AC-086 @FR-029 @US11-AS3 | passed | 00:00:00.0860244 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "06", actingUser: "the registered user `ac016.user.06@example.com`", habit: "an unknown identifier", __pickleIndex: "49", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1750388 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "14", settings: "Reduce, count 3 `sweets`, 1 time per week", records: "1 for 2026-11-16 and 2 for 2026-11-18", state: "Met", progress: "progress total 3 of at most 3", __pickleIndex: "250", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1047343 |
| F002 | Demo loading without a valid demo password creates no catalog and no custom habit(n: "02", demoPassword: "`\"   \"`", result: "\"no demo password configured\"", __pickleIndex: "242", exampleTags: []) | @F002 @AC-063 @FR-030 @FR-031 @US6-AS4 | passed | 00:00:00.3344090 |
| F005 | A day that has not begun on the goal's timeline cannot be recorded(n: "07", timeZone: "Europe/Kyiv", schedule: "fixed days on all seven days", time: "`2026-11-18T22:00:00Z`", date: "2026-11-19", result: "recording succeeds with the state Due, not a corre"···, __pickleIndex: "15", exampleTags: []) | @F005 @AC-006 @FR-002 @US1-AS6 | passed | 00:00:00.0978545 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "16", settings: "Reduce, count 2 `cups`, fixed days Monday to Frida"···, targetText: "\"at most 2 cups, 5 days a week\"", __pickleIndex: "102", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0927581 |
| F002 | Publish and unpublish check the acting user and the role before the identifier(n: "02", actingUser: "the registered user `ac046.user.02@example.com`", action: "unpublishes", result: "\"Only administrators can manage the catalog.\"", __pickleIndex: "194", exampleTags: []) | @F002 @AC-046 @FR-016 | passed | 00:00:00.0814390 |
| F005 | An ended commitment or period stays Pending during its grace period, even when its records reach the target(n: "07", settings: "yes/no, 1 time per day", value: "Yes", time: "`2026-11-21T00:00:00Z`", entry: "Met, no grace end, progress 1 of 1 times", __pickleIndex: "92", exampleTags: []) | @F005 @AC-021 @FR-012 @US2-AS11 | passed | 00:00:00.1015120 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "07", settings: "Build, duration 20 minutes, 1 time per day", records: "20 for 2026-11-18", state: "Met", progress: "progress 1 of 1 times", __pickleIndex: "243", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0925581 |
| F005 | An ended commitment or period stays Pending during its grace period, even when its records reach the target(n: "06", settings: "yes/no, 1 time per day", value: "Yes", time: "`2026-11-20T23:59:59Z`", entry: "Pending, grace ends `2026-11-21T00:00:00Z`, progre"···, __pickleIndex: "91", exampleTags: []) | @F005 @AC-021 @FR-012 @US2-AS11 | passed | 00:00:00.1057618 |
| F005 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit(n: "09", settings: "count 0 `sweets`, fixed days on all seven days", records: "0 for 2026-11-18", state: "Met", __pickleIndex: "101", exampleTags: []) | @F005 @AC-022 @FR-013 @US2-AS12 | passed | 00:00:00.1017605 |
| F005 | An unrecorded commitment is Pending for 48 hours after its end and then Missed by itself(n: "03", time: "`2026-11-21T00:00:00Z`", entry: "Missed, no grace end, correction", __pickleIndex: "46", exampleTags: []) | @F005 @AC-013 @FR-010 @FR-016 @US2-AS2 @US2-AS3 | passed | 00:00:00.0981852 |
| F004 | Starting from a custom habit gives only Build or Reduce from its intent(n: "01", intent: "Build", __pickleIndex: "181", exampleTags: []) | @F004 @AC-046 @FR-007 @US5-AS1 | passed | 00:00:00.0935697 |
| F005 | The first record of a Missed commitment that has a note: the note given is the note, no note keeps it(n: "02", action: "records 20 for 2026-11-18 with the note `\"\"`", note: "no note", __pickleIndex: "280", exampleTags: []) | @F005 @AC-071 @FR-007 @US4-AS9 | passed | 00:00:00.0992468 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "10", goal: "on `A4.008 10 Reading` with Build, yes/no, 21 time"···, message: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "39", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1662535 |
| F001 | A block applies only to its own account | @F001 @AC-024 @FR-008 | passed | 00:00:00.3368361 |
| F007 | "Missed this week" starts on Monday at 00:00 in the account's time zone and includes the previous Friday(n: "04", zone: "`Europe/Kyiv`", read: "2026-11-15T22:00:00Z", missed: "2026-11-13", pending: "2026-11-14", __pickleIndex: "57", exampleTags: []) | @F007 @AC-034 @FR-012 @US6-AS4 | passed | 00:00:00.1179476 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "17", browserTimeZone: "the browser time zone `America/New_York`", timeZone: "`America/New_York`", __pickleIndex: "238", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0873604 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "13", fields: "Build, yes/no, fixed days Monday to Saturday", text: "\"6 days a week\"", kind: "\"Yes/no · fixed days Mon–Sat\"", __pickleIndex: "40", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0897448 |
| F007 | The slice 4 goal list is unchanged, and the overview carries its entries | @F007 @AC-063 @FR-016 @FR-011 @US5-AS4 | passed | 00:00:00.1273188 |
| F004 | A catalog habit without suggested starting goals gives only Build or Reduce from its intent | @F004 @AC-003 @FR-007 @US1-AS3 | passed | 00:00:00.1825696 |
| F007 | A Pending result neither extends nor resets the streak until it is Met(n: "01", nov6: "20", read: "2026-11-08T23:59:59Z", current: "\"2 commitments\"", longest: "\"2 commitments\"", last: "2026-11-06 Pending", __pickleIndex: "19", exampleTags: []) | @F007 @AC-012 @FR-003 @US2-AS4 | passed | 00:00:00.1116052 |
| F004 | Demo loading run again gives existing demo accounts no goals | @F004 @AC-065 @FR-040 @US9-AS2 | passed | 00:00:00.5117889 |
| F005 | A note given with a record is trimmed | @F005 @AC-011 @FR-007 @US1-AS8 | passed | 00:00:00.0980681 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "07", count: "1", marks: "none marked", result: "\"Mark one suggested starting goal for the card.\"", after: "no suggested starting goal", __pickleIndex: "115", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0922166 |
| F004 | Starting from a catalog habit without a position gives its marked suggested starting goal(n: "02", suggestions: "1. Build, count 20 `pages`, 3 times per week; 2. B"···, __pickleIndex: "1", exampleTags: []) | @F004 @AC-001 @FR-007 @US1-AS1 | passed | 00:00:00.1600763 |
| F002 | Unpublishing an Unpublished habit is a success that changes nothing | @F002 @AC-025 @FR-011 @US3-AS8 | passed | 00:00:00.0950203 |
| F002 | Suggested starting goals keep their order and the card shows the marked one | @F002 @AC-034 @FR-006 @US3-AS14 | passed | 00:00:00.0936032 |
| F002 | Names are ordered ordinally, ignoring case, not by the current culture | @F002 @AC-045 @FR-004 | passed | 00:00:00.0894187 |
| F002 | A registered user cannot manage the catalog(n: "05", action: "publishes `A030 05 Draft`", __pickleIndex: "92", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1804074 |
| F005 | A record belongs to the week of its date, not to the week of the instant it is saved(n: "02", time: "`2026-11-23T00:00:00Z`", date: "2026-11-22", state: "Pending", firstWeek: "records (2026-11-22, Yes)", secondWeek: "no record", __pickleIndex: "131", exampleTags: []) | @F005 @AC-034 @FR-001 @FR-009 | passed | 00:00:00.1075472 |
| F001 | Sign-in with a wrong password fails with the single message | @F001 @AC-013 @FR-007 @US2-AS3 | passed | 00:00:00.0725783 |
| F002 | A custom habit with a name of 60 characters and a description of 500 characters is accepted | @F002 @AC-059 @FR-005 | passed | 00:00:00.0850434 |
| F002 | Only the 24 habit icons are accepted(n: "26", icon: "`trash-2`", result: "\"Choose an icon.\"", __pickleIndex: "189", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0796213 |
| F005 | A final Build duration commitment is Met at or above its target and Failed below it(n: "04", duration: "duration 20 minutes", value: "45", state: "Met", progress: "progress 45 of 20 minutes", __pickleIndex: "51", exampleTags: []) | @F005 @AC-014 @FR-004 @FR-014 @US2-AS4 | passed | 00:00:00.0970978 |
| F004 | Anonymous visitors and administrators get the catalog without goal information(n: "05", actingUser: "the anonymous visitor", action: "browses the catalog as the anonymous visitor with "···, result: "the result is \"You are not signed in.\"", __pickleIndex: "199", exampleTags: []) | @F004 @AC-055 @FR-032 @US6-AS4 | passed | 00:00:00.1671094 |
| F001 | Demo loading decides per demo email, and a demo email registered earlier keeps its owner's password | @F001 @AC-060 @FR-003 @FR-016 | passed | 00:00:00.5051953 |
| F002 | Browsing returns only Published habits, with their card content, to every audience(n: "02", actingUser: "the registered user `ac001.user.02@example.com`", __pickleIndex: "1", exampleTags: []) | @F002 @AC-001 @FR-008 @US1-AS1 | passed | 00:00:00.1779350 |
| F007 | A demo goal whose catalog habit is not Published is left out | @F007 @AC-055 @FR-032 | passed | 00:00:00.6283843 |
| F002 | A registered user cannot manage the catalog(n: "01", action: "lists the catalog habits as an administrator", __pickleIndex: "88", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1740449 |
| F005 | A demo goal is left out when its catalog habit is not Published, and the results stay the same(n: "01", habits: "a Published `Yoga`, a Draft `Reading` and an Unpub"···, goals: "exactly one goal, on the catalog habit `Yoga`, Bui"···, __pickleIndex: "217", exampleTags: []) | @F005 @AC-060 @FR-030 | passed | 00:00:00.5584053 |
| F005 | A note given with a record is the note; no note leaves it unchanged(n: "08", action: "changes the record of 2026-11-16 to No with the no"···, note: "the note \"Knee was sore.\"", __pickleIndex: "145", exampleTags: []) | @F005 @AC-037 @FR-007 @US4-AS3 | passed | 00:00:00.1056911 |
| F005 | A note saved on its own replaces the note, and an empty one removes it(n: "04", note: "\"  New note.  \"", result: "the note \"New note.\"", __pickleIndex: "137", exampleTags: []) | @F005 @AC-036 @FR-007 @US4-AS2 | passed | 00:00:00.1061287 |
| F004 | Another user's goal does not exist for them(n: "02", action: "reads the evaluation calendar from 2026-11-18 to 2"···, goal: "`f4.ac029.a.02@example.com`'s goal", __pickleIndex: "125", exampleTags: []) | @F004 @AC-029 @FR-020 @US3-AS1 | passed | 00:00:00.2518997 |
| F001 | Four failed sign-ins in a row do not block | @F001 @AC-020 @FR-008 | passed | 00:00:00.2561081 |
| F004 | A habit that no goal uses is unpublished at once(n: "02", confirmation: "with confirmation", __pickleIndex: "207", exampleTags: []) | @F004 @AC-059 @FR-019 @FR-033 @US7-AS4 | passed | 00:00:00.0867227 |
| F002 | Deleted demo custom habits are not created again | @F002 @AC-066 @FR-031 @US6-AS7 | passed | 00:00:00.5059919 |
| F004 | Registration keeps a named browser time zone, and otherwise gives the default(n: "07", browserTimeZone: "the browser time zone `CET`", timeZone: "`UTC`", __pickleIndex: "228", exampleTags: []) | @F004 @AC-070 @FR-019 @FR-024 @FR-025 @US10-AS1 @US10-AS2 | passed | 00:00:00.0877570 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "07", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "the identifier of their custom habit `Cold shower`", __pickleIndex: "78", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | passed | 00:00:00.1693849 |
| F002 | The anonymous visitor cannot manage the catalog(n: "04", action: "edits `A031 04 Published` to the name `A031 04 Ren"···, __pickleIndex: "97", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0796862 |
| F004 | A habit published again is in the catalog again for goals | @F004 @AC-061 @FR-034 | passed | 00:00:00.2719881 |
| F005 | Recording checks the acting user, the goal, the date, the value and the note in that order(n: "07", actor: "`f5.ac010.owner.<n>@example.com`", date: "2026-11-18", value: "3", note: "a text of 501 characters", result: "\"The note can have at most 500 characters.\"", __pickleIndex: "39", exampleTags: []) | @F005 @AC-010 @FR-005 @FR-018 @US1-AS7 | passed | 00:00:00.2554153 |
| F005 | A note given with a record is the note; no note leaves it unchanged(n: "02", action: "records Yes for 2026-11-18 with the note `\"   \"`", note: "no note", __pickleIndex: "139", exampleTags: []) | @F005 @AC-037 @FR-007 @US4-AS3 | passed | 00:00:00.0965737 |
| F004 | "Today" is the date in the user's own time zone(n: "04", timeZone: "`America/New_York`", time: "`2026-11-17T05:00:00Z`", startDate: "2026-11-17", result: "\"The start date cannot be in the past.\"", __pickleIndex: "179", exampleTags: []) | @F004 @AC-044 @FR-006 @FR-007 @FR-012 @US4-AS8 | passed | 00:00:00.0955477 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "07", fields: "Reduce, yes/no, 1 time per day", text: "\"at most once a day\"", kind: "\"Yes/no · flexible\"", __pickleIndex: "34", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0818174 |
| F002 | Names and descriptions are trimmed before they are checked and stored(n: "02", name: "`\"A070 02 Blank\"`", description: "`\"   \"`", storedName: "`A070 02 Blank`", storedDescription: "no description", __pickleIndex: "251", exampleTags: []) | @F002 @AC-070 @FR-004 @FR-005 | passed | 00:00:00.0788778 |
| F005 | A commitment in the old time zone becomes final, and is corrected, at the end of its own grace period(n: "01", time: "`2026-11-19T21:59:59Z`", state: "Pending", correction: "not a correction", __pickleIndex: "119", exampleTags: []) | @F005 @AC-031 @FR-006 @FR-015 @US3-AS6 | passed | 00:00:00.1191834 |
| F007 | A second run of demo report loading leaves demo5 unchanged | @F007 @AC-050 @FR-032 @US8-AS9 | passed | 00:00:00.5941165 |
| F005 | A change is a correction only from the instant its grace period ends(n: "05", before: "12 for 2026-11-18", time: "`2026-11-20T23:59:59Z`", action: "changes the record of 2026-11-18 to 20", result: "changing succeeds with the state Pending, not a co"···, __pickleIndex: "114", exampleTags: []) | @F005 @AC-027 @FR-006 | passed | 00:00:00.1009367 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "03", search: "`\"ning pa\"`", morning: "contains", evening: "does not contain", __pickleIndex: "10", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.0829390 |
| F004 | Each field refusal of goal creation, with its message(n: "20", settings: "Build, yes/no, 21 times per week", message: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "27", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1750011 |
| F004 | Administrators read and change their own time zone; anonymous visitors cannot | @F004 @AC-079 @FR-026 @US10-AS8 | passed | 00:00:00.1101147 |
| F004 | Use cases check the acting user, the role, the identifier and the fields, in this order(n: "02", actingUser: "`f4.ac035.admin.02@example.com`", action: "reads the evaluation calendar from 2026-11-24 to 2"···, message: "\"Goals are for registered users.\"", __pickleIndex: "148", exampleTags: []) | @F004 @AC-035 @FR-023 | passed | 00:00:00.1711582 |
| F007 | Around a clock change back, a grace period ends at 23:00 local time and a 25-hour day counts once(n: "01", read: "2026-10-26T21:59:59Z", consistency: "\"100% (4 of 4)\"", current: "\"4 commitments\"", date: "2026-10-24", state: "Pending", __pickleIndex: "102", exampleTags: []) | @F007 @AC-057 @FR-001 @FR-003 @FR-008 | passed | 00:00:00.1177578 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "02", fields: "Reduce, duration 2 hours, 1 time per day", text: "\"at most 2 h a day\"", kind: "\"Duration · flexible\"", __pickleIndex: "29", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0805869 |
| F004 | The edges of every target range are accepted(n: "03", settings: "Reduce, count 0 `cups`, 1 time per day", targetText: "\"at most 0 cups a day\"", __pickleIndex: "46", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | passed | 00:00:00.1633985 |
| F005 | Any begun date of a week names that week's one note(n: "06", action: "saves the note a text of 501 characters for 2026-1"···, result: "the result is \"Nothing is scheduled on this date."···, __pickleIndex: "157", exampleTags: []) | @F005 @AC-039 @FR-002 @FR-007 @US4-AS5 | passed | 00:00:00.0932015 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "06", action: "edits a custom habit with the identifier of `A058 "···, __pickleIndex: "236", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.1738250 |
| F004 | An anonymous visitor gets no goal use case(n: "03", action: "lists their goals", __pickleIndex: "137", exampleTags: []) | @F004 @AC-032 @FR-022 @US3-AS4 | passed | 00:00:00.1721287 |
| F007 | Around a clock change back, a grace period ends at 23:00 local time and a 25-hour day counts once(n: "02", read: "2026-10-26T22:00:00Z", consistency: "\"80% (4 of 5)\"", current: "\"0 commitments\"", date: "2026-10-24", state: "Missed", __pickleIndex: "103", exampleTags: []) | @F007 @AC-057 @FR-001 @FR-003 @FR-008 | passed | 00:00:00.1251818 |
| F004 | Each field refusal of goal creation, with its message(n: "04", settings: "Build, yes/no with the count unit `pages`, fixed d"···, message: "\"A yes/no goal has no amount or unit.\"", __pickleIndex: "11", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1641449 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "06", search: "`\"   \"`", morning: "contains", evening: "contains", __pickleIndex: "13", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.1185720 |
| F004 | "Today" is the date in the user's own time zone(n: "01", timeZone: "`Europe/Kyiv`", time: "`2026-11-16T21:30:00Z`", startDate: "2026-11-16", result: "creating succeeds", __pickleIndex: "176", exampleTags: []) | @F004 @AC-044 @FR-006 @FR-007 @FR-012 @US4-AS8 | passed | 00:00:00.0920687 |
| F005 | Progress is given at any state, as the goal's kind of target counts it(n: "01", settings: "Build, duration 20 minutes, fixed days on all seve"···, records: "12 for 2026-11-18", progress: "progress 12 of 20 minutes", __pickleIndex: "167", exampleTags: []) | @F005 @AC-044 @FR-014 @US5-AS2 | passed | 00:00:00.0928456 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "05", state: "Published", action: "edits `A026 05 Other`", __pickleIndex: "68", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | passed | 00:00:00.0971341 |
| F005 | A change is a correction only from the instant its grace period ends(n: "06", before: "12 for 2026-11-18", time: "`2026-11-21T00:00:00Z`", action: "changes the record of 2026-11-18 to 20", result: "changing succeeds with the state Met, a correction", __pickleIndex: "115", exampleTags: []) | @F005 @AC-027 @FR-006 | passed | 00:00:00.1072035 |
| F004 | The Home greeting follows the time of day in the account's time zone(n: "07", time: "`2026-11-18T21:59:59Z` (23:59:59 in Kyiv)", greeting: "\"Good evening, Yuri\"", __pickleIndex: "284", exampleTags: []) | @F004 @AC-086 @FR-029 @US11-AS3 | passed | 00:00:00.0887987 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "12", second: "Reduce, count -1 `sweets`, 1 time per day", result: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "127", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0934138 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "12", settings: "Reduce, duration 30 minutes, 1 time per day", records: "20 and 10 for 2026-11-18", state: "Met", progress: "progress total 30 of at most 30", __pickleIndex: "248", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0968058 |
| F004 | The Home greeting follows the time of day in the account's time zone(n: "01", time: "`2026-11-18T02:59:00Z` (04:59 in Kyiv)", greeting: "\"Good evening, Yuri\"", __pickleIndex: "278", exampleTags: []) | @F004 @AC-086 @FR-029 @US11-AS3 | passed | 00:00:00.0900078 |
| F002 | Only the 24 habit icons are accepted(n: "10", icon: "`droplet`", result: "success", __pickleIndex: "173", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0913556 |
| F005 | A final Build duration commitment is Met at or above its target and Failed below it(n: "05", duration: "duration 20 minutes", value: "0", state: "Failed", progress: "progress 0 of 20 minutes", __pickleIndex: "52", exampleTags: []) | @F005 @AC-014 @FR-004 @FR-014 @US2-AS4 | passed | 00:00:00.1105647 |
| F004 | Each field refusal of goal creation, with its message(n: "05", settings: "Build, count with no amount and the unit `pages`, "···, message: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "12", exampleTags: []) | @F004 @AC-007 @FR-006 @US1-AS7 | passed | 00:00:00.1601512 |
| F001 | Registration creates a registered user | @F001 @AC-001 @FR-001 @FR-004 @FR-009 @US1-AS1 | passed | 00:00:00.0928691 |
| F004 | A goal without a name of its own is named after its habit and target(n: "04", name: "the name `\"   \"`", goalName: "\"A4.016 04 Reading, 15 min, 5 days a week\"", own: "not its own", __pickleIndex: "85", exampleTags: []) | @F004 @AC-016 @FR-009 @FR-010 @US1-AS9 | passed | 00:00:00.1705558 |
| F005 | A note saved on its own replaces the note, and an empty one removes it(n: "02", note: "`\"   \"`", result: "no note", __pickleIndex: "135", exampleTags: []) | @F005 @AC-036 @FR-007 @US4-AS2 | passed | 00:00:00.1142194 |
| F007 | The percentage is rounded down to a whole number(n: "01", nov1: "20", nov2: "20", nov3: "12", text: "\"66% (2 of 3)\"", percentage: "66", successful: "2", __pickleIndex: "4", exampleTags: []) | @F007 @AC-004 @FR-002 @US1-AS4 | passed | 00:00:00.1163716 |
| F002 | Suggested starting goals are checked in list order, and each by its first failing field(n: "02", goals: "(1) Build, yes/no, 1 time per day (marked); (2) Bu"···, result: "\"Choose how it is measured.\"", position: "2", __pickleIndex: "155", exampleTags: []) | @F002 @AC-041 @FR-007 | passed | 00:00:00.0913154 |
| F002 | Field checks come in a fixed order(n: "03", fields: "the name `A029 03 Long`, a description that is a t"···, result: "\"The description can have at most 500 characters."···, __pickleIndex: "81", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0838716 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "08", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "an identifier that the test creates and that belon"···, __pickleIndex: "79", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | passed | 00:00:00.1793245 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "15", second: "Build, count 5 with the unit `\"  \"`, 1 time per "···, result: "\"Enter a unit, for example pages.\"", __pickleIndex: "130", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0891515 |
| F007 | A per-day goal of 2 times has one bar per day with the day's progress against 2 | @F007 @AC-065 @FR-006 | passed | 00:00:00.1386091 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "01", settings: "Build, count 5 `pages`, fixed days on all seven da"···, records: "4 for 2026-11-18", state: "Failed", progress: "progress 4 of 5", __pickleIndex: "237", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1060531 |
| F001 | Administrator creation refuses a password that breaks the password rule(email: "\"seventh.admin@example.com\"", password: "\"curatorship\"", whyItBreaksTheRule: "11 characters, no digit", __pickleIndex: "72", exampleTags: []) | @F001 @AC-046 @FR-002 @FR-014 @US4-AS7 | passed | 00:00:00.0891998 |
| F007 | A per-week or per-day result counts only from the end of its grace period(n: "06", settings: "Reduce, count 1 `sweets`, 1 time per day", created: "2026-11-01T08:00:00Z", start: "2026-11-01", records: "2 for 2026-11-01", read: "2026-11-04T00:00:00Z", consistency: "\"0% (0 of 1)\"", current: "\"0 days\"", first: "2026-11-01 Failed", __pickleIndex: "117", exampleTags: []) | @F007 @AC-062 @FR-001 @FR-003 @FR-008 | passed | 00:00:00.1134726 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "12", settings: "Build, yes/no, 2 times per day", targetText: "\"2 times a day\"", __pickleIndex: "98", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0926202 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "05", goal: "on `A4.008 05 Reading` with Build, count 0 with no"···, message: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "34", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1614496 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@.example\"", whyItIsNotValid: "domain has one non-empty part, after an empty one", __pickleIndex: "11", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0007954 |
| F004 | Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "03", submitted: "Build, yes/no with the count unit `\"   \"`, fixed"···, stored: "yes/no, no amount and no unit", targetText: "\"daily\"", __pickleIndex: "68", exampleTags: []) | @F004 @AC-013 @FR-002 @FR-006 | passed | 00:00:00.1908507 |
| F005 | A final per-day Reduce count period is Met when its total is within the limit(n: "02", records: "1 for 2026-11-18 and 1 for 2026-11-18", state: "Failed", progress: "progress total 2 of at most 1", __pickleIndex: "76", exampleTags: []) | @F005 @AC-019 @FR-014 @US2-AS9 | passed | 00:00:00.1321425 |
| F005 | The anonymous visitor gets "You are not signed in." from every use case of this slice(n: "06", useCase: "recordable now", __pickleIndex: "206", exampleTags: []) | @F005 @AC-050 @FR-018 @US6-AS4 | passed | 00:00:00.1020044 |
| F005 | Another account's record identifier does not exist on one's own goal(n: "01", action: "changes the record to 12", __pickleIndex: "192", exampleTags: []) | @F005 @AC-048 @FR-017 @US6-AS2 | passed | 00:00:00.1892480 |
| F001 | The anonymous visitor cannot read account details | @F001 @AC-026 @FR-011 @US3-AS1 | passed | 00:00:00.0007852 |
| F005 | A final per-day yes/no period counts its Yes records(n: "05", settings: "Build, yes/no, 2 times per day", records: "Yes for 2026-11-18", state: "Failed", progress: "progress 1 of 2 times", __pickleIndex: "83", exampleTags: []) | @F005 @AC-020 @FR-014 @US2-AS10 | passed | 00:00:00.0964914 |
| F005 | The first record of a Missed commitment that has a note: the note given is the note, no note keeps it(n: "03", action: "records 20 for 2026-11-18 with the note `\"   \"`", note: "no note", __pickleIndex: "281", exampleTags: []) | @F005 @AC-071 @FR-007 @US4-AS9 | passed | 00:00:00.1146194 |
| F004 | A per-week goal has one evaluation period per Monday-to-Sunday week, from its first full week(n: "02", startDate: "2026-11-18", from: "2026-11-25", to: "2026-12-01", periods: "2 periods: 2026-11-23 to 2026-11-29 from `2026-11-"···, __pickleIndex: "157", exampleTags: []) | @F004 @AC-038 @FR-013 @US4-AS3 | passed | 00:00:00.1014959 |
| F002 | An unknown identifier does not exist for the administrator(n: "03", action: "publishes", __pickleIndex: "102", exampleTags: []) | @F002 @AC-032 @FR-013 @US3-AS12 | passed | 00:00:00.0836670 |
| F007 | A goal with no final result has the consistency "N/A"(n: "01", created: "2026-11-23T08:00:00Z", start: "2026-11-25", recorded: "no date", note: "not started", __pickleIndex: "2", exampleTags: []) | @F007 @AC-003 @FR-002 @US1-AS3 | passed | 00:00:00.1054504 |
| F001 | A password change checks the current password before the password rule | @F001 @AC-035 @FR-013 @US6-AS4 | passed | 00:00:00.1405286 |
| F005 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit(n: "01", settings: "yes/no, fixed days on all seven days", records: "nothing", state: "Missed, no record", __pickleIndex: "93", exampleTags: []) | @F005 @AC-022 @FR-013 @US2-AS12 | passed | 00:00:00.1065935 |
| F005 | What can be recorded now is every Due and every Pending commitment or period, newest first(n: "05", schedule: "3 times per week", startDate: "2026-11-16", time: "`2026-11-24T10:00:00Z`", entries: "in this order: 2026-11-23 to 2026-11-29, Due, grac"···, __pickleIndex: "178", exampleTags: []) | @F005 @AC-045 @FR-011 @US5-AS3 | passed | 00:00:00.1254974 |
| F007 | demo4's Yoga goal reads its weeks by the weekday of the run(n: "01", run: "2026-11-18T10:00:00Z", consistency: "\"33% (1 of 3)\"", current: "\"1 week\"", longest: "\"1 week\"", __pickleIndex: "75", exampleTags: []) | @F007 @AC-044 @FR-030 @US8-AS3 | passed | 00:00:00.6893890 |
| F005 | Demo history loading takes a demo password only with at least 8 characters and a digit(n: "02", demoPassword: "`examples`", why: "8 characters, no digit", result: "its result is \"demo password does not meet the pa"···, __pickleIndex: "274", exampleTags: []) | @F005 @AC-069 @FR-030 @US7-AS6 | passed | 00:00:00.3831546 |
| F004 | The edges of every target range are accepted(n: "05", settings: "Build, count 5 with a unit of 20 characters, 1 tim"···, targetText: "\"5 uuuuuuuuuuuuuuuuuuuu a day\"", __pickleIndex: "48", exampleTags: []) | @F004 @AC-009 @FR-002 @FR-006 | passed | 00:00:00.1669101 |
| F002 | Field checks come in a fixed order(n: "02", fields: "a name of 61 characters with the prefix `A029 02`,"···, result: "\"The name can have at most 60 characters.\"", __pickleIndex: "80", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0845447 |
| F002 | The role and the identifier are checked before the fields(n: "04", actingUser: "the registered user `ac039.user.04@example.com`", action: "edits an unknown identifier with the name `\"   \""···, result: "\"Only administrators can manage the catalog.\"", __pickleIndex: "152", exampleTags: []) | @F002 @AC-039 @FR-016 @US3-AS18 | passed | 00:00:00.1730939 |
| F002 | Publish and unpublish check the acting user and the role before the identifier(n: "03", actingUser: "the anonymous visitor", action: "publishes", result: "\"You are not signed in.\"", __pickleIndex: "195", exampleTags: []) | @F002 @AC-046 @FR-016 | passed | 00:00:00.0788486 |
| F007 | demo4's Sugar goal reads "92% (24 of 26)" with a streak of 1 day and a longest of 16 days | @F007 @AC-043 @FR-030 @US8-AS2 | passed | 00:00:00.5164978 |
| F007 | "This week" follows the account's time zone, also for days of an older version(n: "02", read: "2026-11-15T22:00:00Z", missed: "0", entries: "(the goal, 2026-11-13, Pending, grace ends 2026-11"···, __pickleIndex: "123", exampleTags: []) | @F007 @AC-067 @FR-012 @US6-AS4 | passed | 00:00:00.1114043 |
| F005 | Changing the record of a commitment without a note, with a note, adds the note | @F005 @AC-072 @FR-007 @US4-AS10 | passed | 00:00:00.1041865 |
| F005 | A value that does not belong to the goal's measurement is not stored(n: "02", measurement: "count 5 `pages`", given: "the amount 3 together with Yes", stored: "(2026-11-18, 3), with neither Yes nor No", __pickleIndex: "31", exampleTags: []) | @F005 @AC-009 @FR-004 | passed | 00:00:00.0923624 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "04", goal: "on `A4.008 04 Reading` with Build, yes/no with the"···, message: "\"A yes/no goal has no amount or unit.\"", __pickleIndex: "33", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1712000 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "02", value: "`FLE Standard Time`", __pickleIndex: "250", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.1087515 |
| F007 | A goal that has not started has streaks of 0, no recent results and no trend(n: "02", settings: "Build, yes/no, 1 time per week", start: "2026-11-30", streak: "\"0 weeks\"", __pickleIndex: "33", exampleTags: []) | @F007 @AC-015 @FR-004 @FR-005 @FR-006 @US2-AS7 | passed | 00:00:00.1014330 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@\"", whyItIsNotValid: "no domain", __pickleIndex: "8", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0003494 |
| F004 | A confirmed unpublish acts even when goals were added after the count was given | @F004 @AC-093 @FR-033 | passed | 00:00:00.2522634 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "16", second: "Build, count 5 with a unit of 21 characters, 1 tim"···, result: "\"The unit can have at most 20 characters.\"", __pickleIndex: "131", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0884538 |
| F004 | A demo goal finds its catalog habit by name ignoring case, and only "Coffee" may be Unpublished | @F004 @AC-067 @FR-040 | passed | 00:00:00.6013657 |
| F005 | When R is a Monday, the demo Yoga goal's last full week is Pending and it has a week before week 3 | @F005 @AC-055 @FR-030 @US7-AS2 | passed | 00:00:00.4618185 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "05", settings: "Build, yes/no, 1 time per day", targetText: "\"once a day\"", __pickleIndex: "91", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0940109 |
| F001 | Registration accepts a password at the lower bound of the password rule(email: "\"eight.pw@example.com\"", password: "\"walking4\"", whyItMeetsTheRule: "8 characters, with a digit", __pickleIndex: "5", exampleTags: []) | @F001 @AC-003 @FR-001 @FR-002 | passed | 00:00:00.0851621 |
| F001 | Registration refuses an email that is not valid(email: "\"   \"", whyItIsNotValid: "only spaces, empty after trimming", __pickleIndex: "20", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0031965 |
| F005 | The demo goal "Yoga, 3 times a week" shows a Met, a Failed and a Missed week and a Due week when R is a Wednesday | @F005 @AC-054 @FR-030 @US7-AS2 | passed | 00:00:00.4599119 |
| F005 | A date that belongs to no commitment or period cannot be recorded(n: "01", schedule: "fixed days Monday, Wednesday and Friday", createdAt: "`2026-11-16T08:00:00Z`", startDate: "2026-11-16", date: "2026-11-17", why: "a Tuesday", __pickleIndex: "4", exampleTags: []) | @F005 @AC-005 @FR-002 @US1-AS5 | passed | 00:00:00.1076215 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "08", settings: "Build, duration 20 minutes, 3 times per week", records: "20 for 2026-11-16, 19 for 2026-11-17 and 20 for 20"···, state: "Failed", progress: "progress 2 of 3 times", __pickleIndex: "244", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.1090703 |
| F002 | The category filter starts with All and follows the fixed order | @F002 @AC-008 @FR-002 @FR-009 @US1-AS8 | passed | 00:00:00.0970035 |
| F005 | A value that does not fit the goal's measurement is refused with its message(n: "04", measurement: "count 5 `pages`", value: "10000", message: "\"Enter a whole number from 0 to 9999.\"", __pickleIndex: "20", exampleTags: []) | @F005 @AC-007 @FR-005 @US1-AS7 | passed | 00:00:00.0870747 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "07", settings: "Build, yes/no, 1 time per week", targetText: "\"once a week\"", __pickleIndex: "93", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.1057702 |
| F004 | An account name is trimmed, and empty means no name(n: "03", name: "the name `\"   \"`", result: "no name", __pickleIndex: "275", exampleTags: []) | @F004 @AC-084 @FR-019 @FR-028 @US11-AS1 | passed | 00:00:00.0847308 |
| F005 | Demo history loading creates the demo account with history and its three demo goals | @F005 @AC-051 @FR-030 @US7-AS1 | passed | 00:00:00.4804175 |
| F002 | Demo accounts that demo loading creates get their demo custom habits | @F002 @AC-064 @FR-024 @FR-031 @US6-AS5 | passed | 00:00:00.5811667 |
| F007 | demo5's reports on a Wednesday run(n: "03", goal: "\"Walking, 30 min, 3 days a week\"", consistency: "\"82% (29 of 35)\"", current: "\"7 commitments\"", longest: "\"15 commitments\"", bars: "14", __pickleIndex: "84", exampleTags: []) | @F007 @AC-048 @FR-032 @US8-AS8 | passed | 00:00:00.6426163 |
| F005 | A record belongs to the week of its date, not to the week of the instant it is saved(n: "01", time: "`2026-11-22T23:59:59Z`", date: "2026-11-22", state: "Due", firstWeek: "records (2026-11-22, Yes)", secondWeek: "no record", __pickleIndex: "130", exampleTags: []) | @F005 @AC-034 @FR-001 @FR-009 | passed | 00:00:00.0969933 |
| F004 | Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "01", submitted: "Build, count 20 `pages` with the duration unit hou"···, stored: "count 20 `pages`, no duration unit", targetText: "\"20 pages, 3 times a week\"", __pickleIndex: "66", exampleTags: []) | @F004 @AC-013 @FR-002 @FR-006 | passed | 00:00:00.1730841 |
| F005 | A day that has not begun on the goal's timeline cannot be recorded(n: "08", timeZone: "UTC", schedule: "3 times per week", time: "`2026-11-18T10:00:00Z`", date: "2026-11-20", result: "the result is \"You cannot record a day that has n"···, __pickleIndex: "16", exampleTags: []) | @F005 @AC-006 @FR-002 @US1-AS6 | passed | 00:00:00.0898828 |
| F005 | A value that does not fit the goal's measurement is refused with its message(n: "08", measurement: "duration 1 in hours", value: "1441", message: "\"Enter a whole number of minutes from 0 to 1440.\"···, __pickleIndex: "24", exampleTags: []) | @F005 @AC-007 @FR-005 @US1-AS7 | passed | 00:00:00.0976569 |
| F005 | Progress is given at any state, as the goal's kind of target counts it(n: "06", settings: "Build, yes/no, 2 times per day", records: "Yes and No for 2026-11-18", progress: "progress 1 of 2 times", __pickleIndex: "172", exampleTags: []) | @F005 @AC-044 @FR-014 @US5-AS2 | passed | 00:00:00.1049245 |
| F002 | The role and the identifier are checked before the fields(n: "02", actingUser: "the anonymous visitor", action: "creates a catalog habit with the name `\"   \"`", result: "\"You are not signed in.\"", __pickleIndex: "150", exampleTags: []) | @F002 @AC-039 @FR-016 @US3-AS18 | passed | 00:00:00.1722524 |
| F007 | An anonymous visitor and an administrator are refused by every use case of this slice(n: "02", actor: "the anonymous visitor", useCase: "the goal report for the identifier `3f2b8c1e-0000-"···, result: "\"You are not signed in.\"", __pickleIndex: "66", exampleTags: []) | @F007 @AC-041 @FR-015 @FR-007 @US7-AS3 @US7-AS4 | passed | 00:00:00.1658021 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "02", goal: "on `A4.008 02 Draft` with Build, no measurement, f"···, message: "\"This habit is not in the catalog.\"", __pickleIndex: "31", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1779345 |
| F005 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement(n: "09", measurement: "count 5 `pages`", before: "5", change: "the record of 2026-11-18 to the amount 7 together "···, result: "changing succeeds with the state Met, a correction"···, __pickleIndex: "233", exampleTags: []) | @F005 @AC-064 @FR-004 @FR-005 @FR-006 @FR-007 | passed | 00:00:00.1410616 |
| F004 | Another user's goal does not exist for them(n: "01", action: "reads the goal", goal: "`f4.ac029.a.01@example.com`'s goal", __pickleIndex: "124", exampleTags: []) | @F004 @AC-029 @FR-020 @US3-AS1 | passed | 00:00:00.2608455 |
| F004 | A value that is not a named time zone is refused and changes nothing(n: "07", value: "`europe/kyiv`", __pickleIndex: "255", exampleTags: []) | @F004 @AC-074 @FR-024 @FR-026 @US10-AS7 | passed | 00:00:00.0958599 |
| F007 | After a time zone change each result is evaluated with the version of its own date(n: "02", read: "2026-11-08T23:00:00Z", consistency: "\"66% (4 of 6)\"", results: "2026-11-01 Met, 2026-11-02 Met, 2026-11-03 Met, 20"···, __pickleIndex: "40", exampleTags: []) | @F007 @AC-020 @FR-009 @US3-AS5 | passed | 00:00:00.1071545 |
| F004 | The target text follows the slice 2 rules, with the "avoided on" exception(n: "01", settings: "Build, duration 15 minutes, fixed days Monday to F"···, targetText: "\"15 min, 5 days a week\"", __pickleIndex: "87", exampleTags: []) | @F004 @AC-018 @FR-004 @FR-010 | passed | 00:00:00.0970044 |
| F004 | A goal on an Unpublished habit keeps working, and no new goal can use that habit(n: "01", actingUser: "`f4.ac060.owner.01@example.com`", action: "lists their goals", result: "the list contains \"A4.060 01 Reading, 15 min, 5 d"···, __pickleIndex: "208", exampleTags: []) | @F004 @AC-060 @FR-016 @FR-034 @US7-AS6 | passed | 00:00:00.2824895 |
| F004 | A habit that changed between starting and creating is looked up again(n: "02", habit: "their custom habit `Cold shower`", change: "`f4.ac015.user.02@example.com` deleted their custo"···, message: "\"This custom habit does not exist.\"", __pickleIndex: "81", exampleTags: []) | @F004 @AC-015 @FR-008 @FR-034 | passed | 00:00:00.1750898 |
| F004 | Starting from a catalog habit without a position gives its marked suggested starting goal(n: "01", suggestions: "1. Build, duration 15 minutes, fixed days Monday t"···, __pickleIndex: "0", exampleTags: []) | @F004 @AC-001 @FR-007 @US1-AS1 | passed | 00:00:00.1899592 |
| F004 | The calendar range has 1 to 366 days(n: "02", from: "2026-11-18", to: "2027-11-19", result: "\"Choose a range of 1 to 366 days.\"", __pickleIndex: "173", exampleTags: []) | @F004 @AC-043 @FR-013 @US4-AS7 | passed | 00:00:00.0895203 |
| F002 | Only the 24 habit icons are accepted(n: "28", icon: "`rocket`", result: "\"Choose an icon.\"", __pickleIndex: "191", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0947363 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "04", name: "`A027 04 Long text`", description: "a text of 501 characters", category: "Body", intent: "Build", icon: "`book-open`", result: "\"The description can have at most 500 characters."···, __pickleIndex: "73", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0869679 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "15", fields: "Build, yes/no, fixed days Monday, Tuesday, Wednesd"···, text: "\"6 days a week\"", kind: "\"Yes/no · fixed days Mon–Wed, Fri–Sun\"", __pickleIndex: "42", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0832903 |
| F005 | A commitment in the old time zone becomes final, and is corrected, at the end of its own grace period(n: "02", time: "`2026-11-19T22:00:00Z`", state: "Met", correction: "a correction", __pickleIndex: "120", exampleTags: []) | @F005 @AC-031 @FR-006 @FR-015 @US3-AS6 | passed | 00:00:00.1085737 |
| F002 | A user's list holds only their own custom habits, in name order ignoring case | @F002 @AC-048 @FR-004 @FR-020 @US4-AS2 | passed | 00:00:00.2506701 |
| F007 | The overview gives the progress of each Due entry | @F007 @AC-031 @FR-011 @US5-AS3 | passed | 00:00:00.1270691 |
| F004 | Unpublishing with confirmation unpublishes a habit that goals use | @F004 @AC-058 @FR-033 @US7-AS2 | passed | 00:00:00.2452259 |
| F001 | Administrator creation accepts a password at the lower bound of the password rule | @F001 @AC-040 @FR-002 @FR-014 | passed | 00:00:00.0891868 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "03", actingUser: "the anonymous visitor", habit: "an unknown identifier", __pickleIndex: "46", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1671820 |
| F005 | A date that belongs to no commitment or period cannot be recorded(n: "02", schedule: "fixed days on all seven days", createdAt: "`2026-11-18T10:00:00Z`", startDate: "2026-11-18", date: "2026-11-17", why: "before the start date", __pickleIndex: "5", exampleTags: []) | @F005 @AC-005 @FR-002 @US1-AS5 | passed | 00:00:00.0961381 |
| F007 | Around a clock change back, a grace period ends at 23:00 local time and a 25-hour day counts once(n: "04", read: "2026-10-27T23:00:00Z", consistency: "\"83% (5 of 6)\"", current: "\"1 commitment\"", date: "2026-10-25", state: "Met", __pickleIndex: "105", exampleTags: []) | @F007 @AC-057 @FR-001 @FR-003 @FR-008 | passed | 00:00:01.0175327 |
| F004 | Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "04", submitted: "Build, yes/no, fixed days Monday and Wednesday, wi"···, stored: "fixed days Monday and Wednesday, no number of time"···, targetText: "\"2 days a week\"", __pickleIndex: "69", exampleTags: []) | @F004 @AC-013 @FR-002 @FR-006 | passed | 00:00:00.1689409 |
| F004 | Values that do not belong to the chosen measurement or schedule are neither refused nor stored(n: "02", submitted: "Build, duration 30 minutes with the count unit `pa"···, stored: "duration 30 minutes, no count unit", targetText: "\"30 min, 3 times a week\"", __pickleIndex: "67", exampleTags: []) | @F004 @AC-013 @FR-002 @FR-006 | passed | 00:00:00.1835656 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "04", search: "`\"a005 04 e\"`", morning: "does not contain", evening: "contains", __pickleIndex: "11", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.0828392 |
| F007 | demo5 has one result Missed this week on every weekday(n: "02", run: "2026-11-17T10:00:00Z", date: "2026-11-14", __pickleIndex: "93", exampleTags: []) | @F007 @AC-053 @FR-032 @FR-012 @US8-AS11 | passed | 00:00:00.5656452 |
| F005 | A note given with a record is the note; no note leaves it unchanged(n: "07", action: "changes the record of 2026-11-16 to No with the no"···, note: "no note", __pickleIndex: "144", exampleTags: []) | @F005 @AC-037 @FR-007 @US4-AS3 | passed | 00:00:00.1300504 |
| F002 | The role and the identifier are checked before the fields(n: "01", actingUser: "the registered user `ac039.user.01@example.com`", action: "creates a catalog habit with the name `\"   \"`", result: "\"Only administrators can manage the catalog.\"", __pickleIndex: "149", exampleTags: []) | @F002 @AC-039 @FR-016 @US3-AS18 | passed | 00:00:00.1806408 |
| F001 | Administrator creation refuses a password that breaks the password rule(email: "\"short.admin@example.com\"", password: "\"curat42\"", whyItBreaksTheRule: "7 characters, with a digit", __pickleIndex: "73", exampleTags: []) | @F001 @AC-046 @FR-002 @FR-014 @US4-AS7 | passed | 00:00:00.0898910 |
| F005 | An evaluation period takes at most 50 records, but its records can still be changed or deleted(n: "05", action: "deletes the first record of 2026-11-18 and then re"···, result: "deleting and recording both succeed with the state"···, __pickleIndex: "129", exampleTags: []) | @F005 @AC-033 @FR-003 @FR-005 | passed | 00:00:00.2159946 |
| F005 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit(n: "08", settings: "count 1 `sweet`, 1 time per day", records: "0 for 2026-11-18", state: "Met", __pickleIndex: "100", exampleTags: []) | @F005 @AC-022 @FR-013 @US2-AS12 | passed | 00:00:00.1064989 |
| F002 | A registered user cannot manage the catalog(n: "02", action: "reads `A030 02 Draft` as an administrator", __pickleIndex: "89", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1798189 |
| F007 | demo4's Yoga goal reads its weeks by the weekday of the run(n: "04", run: "2026-11-16T10:00:00Z", consistency: "\"33% (1 of 3)\"", current: "\"0 weeks\"", longest: "\"1 week\"", __pickleIndex: "78", exampleTags: []) | @F007 @AC-044 @FR-030 @US8-AS3 | passed | 00:00:00.6399676 |
| F004 | A day or week with a clock change keeps local midnights and lasts 23 or 25 hours(n: "03", schedule: "1 time per day", date: "2027-03-28", entry: "2027-03-28 from `2027-03-27T22:00:00Z` to `2027-03"···, __pickleIndex: "166", exampleTags: []) | @F004 @AC-040 @FR-012 @FR-013 @US4-AS5 | passed | 00:00:00.0910279 |
| F002 | A custom habit is not a catalog habit for the administrator(n: "01", action: "reads", __pickleIndex: "104", exampleTags: []) | @F002 @AC-033 @FR-013 @FR-024 @US3-AS13 | passed | 00:00:00.1692459 |
| F004 | An administrator gets no goal use case(n: "02", action: "creates a goal on `A4.031 02 Reading` with Build, "···, __pickleIndex: "130", exampleTags: []) | @F004 @AC-031 @FR-021 @US3-AS3 | passed | 00:00:00.2008765 |
| F004 | A goal without a name of its own is named after its habit and target(n: "01", name: "no name", goalName: "\"A4.016 01 Reading, 15 min, 5 days a week\"", own: "not its own", __pickleIndex: "82", exampleTags: []) | @F004 @AC-016 @FR-009 @FR-010 @US1-AS9 | passed | 00:00:00.1767118 |
| F005 | A demo goal is left out when its catalog habit is not Published, and the results stay the same(n: "02", habits: "a Published `reading`, a Draft `Sugar` and a Draft"···, goals: "exactly one goal, on the catalog habit `reading`, "···, __pickleIndex: "218", exampleTags: []) | @F005 @AC-060 @FR-030 | passed | 00:00:00.5591812 |
| F007 | The Today list has Pending, then Due, then this week's Missed results | @F007 @AC-033 @FR-012 @US6-AS2 @US6-AS3 | passed | 00:00:00.1715800 |
| F005 | The other kinds of target, just inside, at and just past their target(n: "05", settings: "Build, count 5 `pages`, 2 times per day", records: "5 and 4 for 2026-11-18", state: "Failed", progress: "progress 1 of 2 times", __pickleIndex: "241", exampleTags: []) | @F005 @AC-066 @FR-014 | passed | 00:00:00.0948676 |
| F002 | Field checks refuse a custom habit with the same messages(n: "01", name: "`\"   \"`", description: "none", category: "Body", intent: "Build", icon: "`droplet`", result: "\"Enter a name.\"", __pickleIndex: "219", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0906018 |
| F007 | A per-week goal's Pending week is in the Today list | @F007 @AC-060 @FR-012 @FR-032 @US6-AS3 | passed | 00:00:00.5327358 |
| F005 | An ended commitment or period stays Pending during its grace period, even when its records reach the target(n: "05", settings: "duration 20 minutes, fixed days on all seven days", value: "12", time: "`2026-11-21T00:00:00Z`", entry: "Failed, no grace end, progress 12 of 20 minutes", __pickleIndex: "90", exampleTags: []) | @F005 @AC-021 @FR-012 @US2-AS11 | passed | 00:00:00.1085368 |
| F005 | Demo history loading without a demo password creates no account(n: "02", demoPassword: "`\"\"`", __pickleIndex: "221", exampleTags: []) | @F005 @AC-061 @FR-030 @US7-AS6 | passed | 00:00:00.4300910 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "05", fields: "Build, duration 1 hours, fixed days Monday, Wednes"···, text: "\"1 h, 3 days a week\"", kind: "\"Duration · fixed days Mon, Wed, Fri\"", __pickleIndex: "32", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0875144 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "01", search: "`\"  a005 01 morn  \"`", morning: "contains", evening: "does not contain", __pickleIndex: "8", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.0881899 |
| F004 | Browsing gives a registered user their goal count per habit and the tracked count(n: "01", search: "`A4.052 01`", result: "`A4.052 01 Reading` has the goal count 2, `A4.052 "···, __pickleIndex: "188", exampleTags: []) | @F004 @AC-052 @FR-031 @US6-AS1 | passed | 00:00:00.1778489 |
| F004 | The next commitment is the first commitment that has not ended(n: "05", time: "`2026-11-21T00:00:00Z`", next: "2026-11-23", __pickleIndex: "118", exampleTags: []) | @F004 @AC-023 @FR-014 @US2-AS2 | passed | 00:00:00.0902328 |
| F005 | Lowering the record of a Met commitment below its target is a correction that makes it Failed(n: "02", action: "records 12 for 2026-11-18", result: "recording", __pickleIndex: "108", exampleTags: []) | @F005 @AC-025 @FR-006 @US3-AS2 | passed | 00:00:00.1112971 |
| F001 | Demo loading with a password that breaks the password rule creates no account(demoPassword: "\"examplepass\"", whyItBreaksTheRule: "11 characters, no digit", __pickleIndex: "87", exampleTags: []) | @F001 @AC-056 @FR-002 @FR-016 @US7-AS4 | passed | 00:00:00.3388186 |
| F004 | Anonymous visitors and administrators get the catalog without goal information(n: "02", actingUser: "`f4.ac055.admin.02@example.com`", action: "browses the catalog as `f4.ac055.admin.02@example."···, result: "the result contains `A4.055 02 Reading` with no go"···, __pickleIndex: "196", exampleTags: []) | @F004 @AC-055 @FR-032 @US6-AS4 | passed | 00:00:00.1821961 |
| F004 | Goal creation gives the first refusal in FR-006 order(n: "03", goal: "on `A4.008 03 Reading` with no Build or Reduce, no"···, message: "\"Choose Build or Reduce.\"", __pickleIndex: "32", exampleTags: []) | @F004 @AC-008 @FR-006 @FR-023 | passed | 00:00:00.1668044 |
| F005 | Progress is given at any state, as the goal's kind of target counts it(n: "07", settings: "Reduce, duration 30 minutes, 1 time per day", records: "10 and 15 for 2026-11-18", progress: "progress total 25 of at most 30", __pickleIndex: "173", exampleTags: []) | @F005 @AC-044 @FR-014 @US5-AS2 | passed | 00:00:00.0975203 |
| F005 | A final per-day yes/no period counts its Yes records(n: "07", settings: "Build, yes/no, 2 times per day", records: "Yes, Yes and Yes for 2026-11-18", state: "Met", progress: "progress 3 of 2 times", __pickleIndex: "85", exampleTags: []) | @F005 @AC-020 @FR-014 @US2-AS10 | passed | 00:00:00.1055854 |
| F001 | Registration checks the password rule before whether the email is taken | @F001 @AC-008 @FR-003 @US1-AS6 | passed | 00:00:00.0400378 |
| F005 | A record identifier that is not a record of the goal named in the call does not exist(n: "02", action: "deletes the second goal's record of 2026-11-18", __pickleIndex: "122", exampleTags: []) | @F005 @AC-032 @FR-005 @FR-017 @US3-AS7 | passed | 00:00:00.1163675 |
| F001 | Administrator creation checks the email before the password rule | @F001 @AC-049 @FR-003 @FR-014 | passed | 00:00:00.0010423 |
| F005 | A final Build duration commitment is Met at or above its target and Failed below it(n: "01", duration: "duration 20 minutes", value: "19", state: "Failed", progress: "progress 19 of 20 minutes", __pickleIndex: "48", exampleTags: []) | @F005 @AC-014 @FR-004 @FR-014 @US2-AS4 | passed | 00:00:00.1106615 |
| F005 | An unrecorded commitment is Pending for 48 hours after its end and then Missed by itself(n: "02", time: "`2026-11-20T23:59:59Z`", entry: "Pending, grace ends `2026-11-21T00:00:00Z`, no cor"···, __pickleIndex: "45", exampleTags: []) | @F005 @AC-013 @FR-010 @FR-016 @US2-AS2 @US2-AS3 | passed | 00:00:00.1012840 |
| F004 | A goal stays as it was created | @F004 @AC-028 @FR-017 @FR-018 | passed | 00:00:00.1895938 |
| F004 | A habit that is not a Published catalog habit is refused as a catalog habit(n: "05", action: "creates a goal with Build, yes/no, fixed days Mond"···, habit: "`A4.014 05 Draft`", __pickleIndex: "76", exampleTags: []) | @F004 @AC-014 @FR-008 @US1-AS8 | passed | 00:00:00.1709525 |
| F002 | Only the 24 habit icons are accepted(n: "02", icon: "`person-standing`", result: "success", __pickleIndex: "165", exampleTags: []) | @F002 @AC-044 @FR-003 | passed | 00:00:00.0867821 |
| F007 | A recorded 0 is 0, and a bar without a record has no value, also for Reduce | @F007 @AC-027 @FR-006 @US4-AS6 | passed | 00:00:00.1290559 |
| F001 | Registration refuses a password that breaks the password rule(email: "\"seven.pw@example.com\"", password: "\"walkin4\"", whyItBreaksTheRule: "7 characters, with a digit", __pickleIndex: "2", exampleTags: []) | @F001 @AC-002 @FR-002 @US1-AS3 @US1-AS4 | passed | 00:00:00.0466964 |
| F004 | Demo loading results name the demo email but never a goal, habit or account name | @F004 @AC-068 @FR-040 @US9-AS4 | passed | 00:00:00.3232801 |
| F004 | Unpublishing a habit that goals use needs confirmation and gives only the number of goals(n: "03", goals: "`f4.ac057.a.03@example.com` created two goals and "···, count: "3", text: "\"3 goals use this habit.\"", __pickleIndex: "204", exampleTags: []) | @F004 @AC-057 @FR-033 @US7-AS1 @US7-AS3 @US7-AS5 | passed | 00:00:00.2863328 |
