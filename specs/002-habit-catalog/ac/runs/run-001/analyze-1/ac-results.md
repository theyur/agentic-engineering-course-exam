# AC results

## Current feature (F002)

Total: 260 · passed: 207 · failed: 53 · pending: 0 · skipped: 0

| Feature | Scenario | Tags | Outcome | Duration |
|---|---|---|---|---|
| F002 | Another registered user cannot reach a custom habit(n: "03", action: "deletes", __pickleIndex: "206", exampleTags: []) | @F002 @AC-053 @FR-021 @FR-022 @US4-AS7 | passed | 00:00:00.1651585 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "04", fields: "Build, yes/no, 1 time per week", text: "\"once a week\"", kind: "\"Yes/no · flexible\"", __pickleIndex: "31", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0792146 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "05", action: "deletes a custom habit with the identifier of `A05"···, __pickleIndex: "235", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.1720739 |
| F002 | Only the 24 habit icons are accepted(n: "20", icon: "`wallet`", result: "success", __pickleIndex: "183", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0913718 |
| F002 | A Draft cannot be unpublished | @F002 @AC-023 @FR-011 @US3-AS6 | passed | 00:00:00.0831717 |
| F002 | The administrator's list is in name order and can be searched | @F002 @AC-040 @FR-004 @FR-012 @US3-AS19 | passed | 00:00:00.1043467 |
| F002 | A custom habit is not a catalog habit for the administrator(n: "02", action: "edits, with the name `A033 02 Taken over`, the cat"···, __pickleIndex: "105", exampleTags: []) | @F002 @AC-033 @FR-013 @FR-024 @US3-AS13 | passed | 00:00:00.1901767 |
| F002 | A registered user edits every field of their custom habit | @F002 @AC-049 @FR-004 @FR-020 @US4-AS3 | failed | 00:00:00.1024489 |
| F002 | The category filter counts only Published catalog habits(n: "01", actingUser: "the anonymous visitor", __pickleIndex: "20", exampleTags: []) | @F002 @AC-009 @FR-002 @FR-009 @FR-024 @US1-AS9 | passed | 00:00:00.3413702 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "06", fields: "Build, count 10 `pages`, fixed day Saturday", text: "\"10 pages, once a week\"", kind: "\"Count · fixed days Sat\"", __pickleIndex: "33", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0941196 |
| F002 | Publishing a Published habit is a success that changes nothing | @F002 @AC-024 @FR-011 @US3-AS7 | passed | 00:00:00.1013817 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "07", fields: "Reduce, yes/no, 1 time per day", text: "\"at most once a day\"", kind: "\"Yes/no · flexible\"", __pickleIndex: "34", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0828719 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "10", second: "Build, duration 1000 minutes, 1 time per day", result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "125", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0875190 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "16", fields: "Reduce, count 5 `cups`, 2 times per week", text: "\"at most 5 cups, 2 times a week\"", kind: "\"Count · flexible\"", __pickleIndex: "43", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0858401 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "20", second: "Build, duration 15 minutes, 0 times per week", result: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "135", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0887344 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "03", fields: "Reduce, count 1 `sweet`, 1 time per day", text: "\"at most 1 sweet a day\"", kind: "\"Count · flexible\"", __pickleIndex: "30", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0790604 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "01", action: "reads `Gone` by its identifier", __pickleIndex: "231", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.2030423 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "12", second: "Reduce, count -1 `sweets`, 1 time per day", result: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "127", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0961153 |
| F002 | A registered user cannot manage the catalog(n: "03", action: "creates the catalog habit `A030 03 New`", __pickleIndex: "90", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | failed | 00:00:00.1840695 |
| F002 | Unpublishing an Unpublished habit is a success that changes nothing | @F002 @AC-025 @FR-011 @US3-AS8 | passed | 00:00:00.0898845 |
| F002 | A registered user creates a custom habit | @F002 @AC-047 @FR-001 @FR-020 @US4-AS1 | passed | 00:00:00.0963544 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "06", count: "0", marks: "none marked", result: "success", after: "no suggested starting goal", __pickleIndex: "114", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0932540 |
| F002 | Only the 24 habit icons are accepted(n: "22", icon: "`sprout`", result: "success", __pickleIndex: "185", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0811265 |
| F002 | A name of 60 characters and a description of 500 characters are accepted | @F002 @AC-028 @FR-005 @US3-AS10 | passed | 00:00:00.0871988 |
| F002 | A Draft and an Unpublished habit can be edited and keep their state | @F002 @AC-069 @FR-012 | passed | 00:00:00.1110558 |
| F002 | Field checks come in a fixed order(n: "03", fields: "the name `A029 03 Long`, a description that is a t"···, result: "\"The description can have at most 500 characters."···, __pickleIndex: "81", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0897233 |
| F002 | Only the 24 habit icons are accepted(n: "05", icon: "`piggy-bank`", result: "success", __pickleIndex: "168", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0852209 |
| F002 | Browsing returns only Published habits, with their card content, to every audience(n: "02", actingUser: "the registered user `ac001.user.02@example.com`", __pickleIndex: "1", exampleTags: []) | @F002 @AC-001 @FR-008 @US1-AS1 | passed | 00:00:00.2138666 |
| F002 | Demo loading run again leaves the demo accounts and their custom habits unchanged | @F002 @AC-065 @FR-031 @US6-AS6 | passed | 00:00:00.4450528 |
| F002 | Custom habits never appear in browsing, for any audience(n: "01", actingUser: "the registered user `ac006.owner.01@example.com`", __pickleIndex: "14", exampleTags: []) | @F002 @AC-006 @FR-024 @US1-AS6 | failed | 00:00:00.0847585 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "05", actingUser: "the registered user `ac016.user.05@example.com`", habit: "`A016 05 Old habit`", __pickleIndex: "48", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1698397 |
| F002 | Field checks refuse a custom habit with the same messages(n: "01", name: "`\"   \"`", description: "none", category: "Body", intent: "Build", icon: "`droplet`", result: "\"Enter a name.\"", __pickleIndex: "219", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.1019388 |
| F002 | Field checks come in a fixed order(n: "04", fields: "the name `A029 04 Cat`, no category, no intent", result: "\"Choose a category.\"", __pickleIndex: "82", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0852323 |
| F002 | For custom habits the acting user and ownership come first, the name check last(n: "04", actingUser: "the registered user `ac057.a.04@example.com`", action: "creates a custom habit with the name `taken` and n"···, result: "\"Choose a category.\"", __pickleIndex: "229", exampleTags: []) | @F002 @AC-057 @FR-005 @FR-016 @US4-AS11 | passed | 00:00:00.1858636 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "01", state: "Draft", action: "creates a catalog habit", __pickleIndex: "64", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | passed | 00:00:00.0970537 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "07", name: "`A027 07 No icon`", description: "none", category: "Body", intent: "Build", icon: "none", result: "\"Choose an icon.\"", __pickleIndex: "76", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0793043 |
| F002 | Names and descriptions are trimmed before they are checked and stored(n: "01", name: "`\"   A070 01 Trimmed   \"`", description: "`\"  Read daily.  \"`", storedName: "`A070 01 Trimmed`", storedDescription: "the description \"Read daily.\"", __pickleIndex: "250", exampleTags: []) | @F002 @AC-070 @FR-004 @FR-005 | passed | 00:00:00.0839198 |
| F002 | The Home preview has fewer than four habits when fewer are Published | @F002 @AC-072 @FR-008 @US5-AS2 | passed | 00:00:00.2680989 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "04", actingUser: "the registered user `ac016.user.04@example.com`", habit: "`A016 04 Draft habit`", __pickleIndex: "47", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1804633 |
| F002 | Another user, and a catalog habit, may have the same name | @F002 @AC-052 @FR-004 @US4-AS6 | failed | 00:00:00.0016726 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "01", fields: "Build, duration 30 minutes, fixed days Monday to S"···, text: "\"30 min daily\"", kind: "\"Duration · fixed days every day\"", __pickleIndex: "28", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0872618 |
| F002 | Suggested starting goals are checked in list order, and each by its first failing field(n: "02", goals: "(1) Build, yes/no, 1 time per day (marked); (2) Bu"···, result: "\"Choose how it is measured.\"", position: "2", __pickleIndex: "155", exampleTags: []) | @F002 @AC-041 @FR-007 | passed | 00:00:00.0890551 |
| F002 | Field checks refuse a custom habit with the same messages(n: "07", name: "`Menu icon`", description: "none", category: "Body", intent: "Build", icon: "`menu`", result: "\"Choose an icon.\"", __pickleIndex: "225", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0945355 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "02", fields: "Reduce, duration 2 hours, 1 time per day", text: "\"at most 2 h a day\"", kind: "\"Duration · flexible\"", __pickleIndex: "29", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0994860 |
| F002 | Only the 24 habit icons are accepted(n: "09", icon: "`dumbbell`", result: "success", __pickleIndex: "172", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0953334 |
| F002 | Demo loading without a valid demo password creates no catalog and no custom habit(n: "03", demoPassword: "`walk4`", result: "\"demo password does not meet the password rule\"", __pickleIndex: "243", exampleTags: []) | @F002 @AC-063 @FR-030 @FR-031 @US6-AS4 | passed | 00:00:00.3059731 |
| F002 | Demo accounts that demo loading creates get their demo custom habits | @F002 @AC-064 @FR-024 @FR-031 @US6-AS5 | passed | 00:00:00.5536630 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "08", actingUser: "the administrator `ac016.admin.08@example.com`", habit: "`A016 08 Old habit`", __pickleIndex: "51", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1837686 |
| F002 | Field checks come in a fixed order(n: "06", fields: "the name `A029 06 Taken`, the category Body, the i"···, result: "\"Choose an icon.\"", __pickleIndex: "84", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0967699 |
| F002 | The anonymous visitor has no access to custom habits(n: "05", action: "deletes `Secret` by its identifier", __pickleIndex: "218", exampleTags: []) | @F002 @AC-055 @FR-023 @US4-AS9 | failed | 00:00:00.0002771 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "17", second: "Build, duration 15 with no unit, 1 time per day", result: "\"Choose minutes or hours.\"", __pickleIndex: "132", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0938577 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "05", fields: "Build, duration 1 minutes, 1 time per week", storedUnit: "minutes", __pickleIndex: "142", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.1037291 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "22", second: "Build, duration 15 minutes, 3 times with no period", result: "\"Choose per day or per week.\"", __pickleIndex: "137", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.1026151 |
| F002 | Demo loading run again leaves the seed catalog unchanged | @F002 @AC-061 @FR-030 @US6-AS2 | passed | 00:00:00.4162229 |
| F002 | A Reduce amount of 0 is accepted and shown as "at most 0" | @F002 @AC-038 @FR-007 @FR-017 @US3-AS17 | passed | 00:00:00.0866908 |
| F002 | Demo loading without a valid demo password creates no catalog and no custom habit(n: "01", demoPassword: "absent", result: "\"no demo password configured\"", __pickleIndex: "241", exampleTags: []) | @F002 @AC-063 @FR-030 @FR-031 @US6-AS4 | passed | 00:00:00.3074117 |
| F002 | A custom habit is not a catalog habit for the administrator(n: "04", action: "unpublishes", __pickleIndex: "107", exampleTags: []) | @F002 @AC-033 @FR-013 @FR-024 @US3-AS13 | passed | 00:00:00.1701083 |
| F002 | An edit of a Published habit is public on the next read | @F002 @AC-020 @FR-012 @US3-AS3 | passed | 00:00:00.1125960 |
| F002 | The anonymous visitor cannot manage the catalog(n: "03", action: "creates the catalog habit `A031 03 New`", __pickleIndex: "96", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | failed | 00:00:00.0868173 |
| F002 | Browsing returns only Published habits, with their card content, to every audience(n: "03", actingUser: "the administrator `ac001.admin.03@example.com`", __pickleIndex: "2", exampleTags: []) | @F002 @AC-001 @FR-008 @US1-AS1 | passed | 00:00:00.1838708 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "05", fields: "Build, duration 1 hours, fixed days Monday, Wednes"···, text: "\"1 h, 3 days a week\"", kind: "\"Duration · fixed days Mon, Wed, Fri\"", __pickleIndex: "32", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.1071232 |
| F002 | Only the 24 habit icons are accepted(n: "27", icon: "`search`", result: "\"Choose an icon.\"", __pickleIndex: "190", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0727817 |
| F002 | The administrator edits the description, intent and icon of a catalog habit | @F002 @AC-071 @FR-012 | passed | 00:00:00.1446536 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "11", second: "Reduce, count 1000 `sweets`, 1 time per day", result: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "126", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0841494 |
| F002 | A registered user cannot manage the catalog(n: "04", action: "edits `A030 04 Draft` to the name `A030 04 Renamed"···, __pickleIndex: "91", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1924510 |
| F002 | Browsing gives the count of the habits it returns | @F002 @AC-011 @FR-008 | passed | 00:00:00.1088337 |
| F002 | Browsing by intent returns only that intent(n: "02", intent: "Build", sugar: "does not contain", water: "contains", __pickleIndex: "6", exampleTags: []) | @F002 @AC-004 @FR-009 @US1-AS4 | passed | 00:00:00.0965064 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "08", second: "Build, duration with no amount and the unit minute"···, result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "123", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0962005 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "18", second: "Build, duration 15 minutes, no schedule", result: "\"Choose fixed days or how many times.\"", __pickleIndex: "133", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0874200 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "04", name: "`A027 04 Long text`", description: "a text of 501 characters", category: "Body", intent: "Build", icon: "`book-open`", result: "\"The description can have at most 500 characters."···, __pickleIndex: "73", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.1023294 |
| F002 | Details give every suggested starting goal in order, with its text and kind | @F002 @AC-013 @FR-001 @FR-010 @FR-017 @US2-AS1 | passed | 00:00:00.0941973 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "07", fields: "Build, count 5 with a unit of 20 characters, 1 tim"···, storedUnit: "that unit of 20 characters", __pickleIndex: "144", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0985182 |
| F002 | Each of the six categories can be given to a catalog habit(n: "03", category: "Health", __pickleIndex: "160", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0769982 |
| F002 | Field checks come in a fixed order(n: "07", fields: "the name `A029 07 Taken`, the category Body, the i"···, result: "\"Mark one suggested starting goal for the card.\"", __pickleIndex: "85", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0859480 |
| F002 | An unknown identifier does not exist for the administrator(n: "01", action: "reads", __pickleIndex: "100", exampleTags: []) | @F002 @AC-032 @FR-013 @US3-AS12 | passed | 00:00:00.0871934 |
| F002 | A demo email held by an administrator gets no custom habit | @F002 @AC-068 @FR-025 @FR-031 | passed | 00:00:00.3693879 |
| F002 | Only the 24 habit icons are accepted(n: "24", icon: "`tv`", result: "success", __pickleIndex: "187", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0898381 |
| F002 | The anonymous visitor has no access to custom habits(n: "02", action: "reads `Secret` by its identifier", __pickleIndex: "215", exampleTags: []) | @F002 @AC-055 @FR-023 @US4-AS9 | failed | 00:00:00.0002639 |
| F002 | Only the 24 habit icons are accepted(n: "11", icon: "`bike`", result: "success", __pickleIndex: "174", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0941844 |
| F002 | Only the 24 habit icons are accepted(n: "19", icon: "`wine-off`", result: "success", __pickleIndex: "182", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0949194 |
| F002 | The Home preview is the first four Published habits in name order | @F002 @AC-012 @FR-008 @FR-024 @US5-AS2 | failed | 00:00:00.3204295 |
| F002 | Browsing orders habits by name, ignoring case | @F002 @AC-002 @FR-004 @FR-008 @US1-AS2 | passed | 00:00:00.0854271 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "06", fields: "Build, duration 999 hours, 1 time per week", storedUnit: "hours", __pickleIndex: "143", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0837465 |
| F002 | A custom habit with a name of 60 characters and a description of 500 characters is accepted | @F002 @AC-059 @FR-005 | passed | 00:00:00.0988917 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "04", count: "6", marks: "the first marked", result: "\"A habit can have at most 5 suggested starting go"···, after: "no suggested starting goal", __pickleIndex: "112", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0909636 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "05", search: "`\"\"`", morning: "contains", evening: "contains", __pickleIndex: "12", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.0898970 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "06", action: "edits a custom habit with the identifier of `A058 "···, __pickleIndex: "236", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.1674092 |
| F002 | Details with a custom habit's identifier give "not in the catalog"(n: "01", actingUser: "the registered user `ac017.owner.01@example.com`", __pickleIndex: "53", exampleTags: []) | @F002 @AC-017 @FR-010 @FR-024 @US2-AS5 | failed | 00:00:00.0007540 |
| F002 | A custom habit is not a catalog habit for the administrator(n: "03", action: "publishes", __pickleIndex: "106", exampleTags: []) | @F002 @AC-033 @FR-013 @FR-024 @US3-AS13 | passed | 00:00:00.1729067 |
| F002 | Browsing returns only Published habits, with their card content, to every audience(n: "01", actingUser: "the anonymous visitor", __pickleIndex: "0", exampleTags: []) | @F002 @AC-001 @FR-008 @US1-AS1 | passed | 00:00:00.1883273 |
| F002 | Suggested starting goals are checked in list order, and each by its first failing field(n: "01", goals: "(1) no Build or Reduce, no measurement, no schedul"···, result: "\"Choose Build or Reduce.\"", position: "1", __pickleIndex: "154", exampleTags: []) | @F002 @AC-041 @FR-007 | passed | 00:00:00.0869042 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "11", fields: "Build, yes/no, 2 times per day", text: "\"2 times a day\"", kind: "\"Yes/no · flexible\"", __pickleIndex: "38", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0871413 |
| F002 | The administrator reads a Draft with all its fields and its state | @F002 @AC-042 @FR-001 @FR-012 | passed | 00:00:00.0886663 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "04", state: "Draft", action: "edits `A026 04 Other`", __pickleIndex: "67", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | failed | 00:00:00.0913992 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "02", count: "2", marks: "both marked", result: "\"Mark one suggested starting goal for the card.\"", after: "no suggested starting goal", __pickleIndex: "110", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0866931 |
| F002 | Demo loading adds no seed to a catalog that has a habit | @F002 @AC-062 @FR-024 @FR-030 @US6-AS3 | passed | 00:00:04.9412626 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "01", name: "`\"   \"`", description: "none", category: "Body", intent: "Build", icon: "`book-open`", result: "\"Enter a name.\"", __pickleIndex: "70", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0910292 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "03", fields: "Reduce, count 0 `sweets`, 1 time per day", storedUnit: "`sweets`", __pickleIndex: "140", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0910350 |
| F002 | The anonymous visitor cannot manage the catalog(n: "04", action: "edits `A031 04 Published` to the name `A031 04 Ren"···, __pickleIndex: "97", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0866521 |
| F002 | Only the 24 habit icons are accepted(n: "26", icon: "`trash-2`", result: "\"Choose an icon.\"", __pickleIndex: "189", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0878552 |
| F002 | The category filter counts only Published catalog habits(n: "02", actingUser: "the registered user `ac009.user.02@example.com`", __pickleIndex: "21", exampleTags: []) | @F002 @AC-009 @FR-002 @FR-009 @FR-024 @US1-AS9 | passed | 00:00:00.3541954 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "06", search: "`\"   \"`", morning: "contains", evening: "contains", __pickleIndex: "13", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.1211889 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "10", fields: "Build, count 2 `glasses`, 3 times per day", text: "\"2 glasses, 3 times a day\"", kind: "\"Count · flexible\"", __pickleIndex: "37", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0790676 |
| F002 | Unpublishing a Published habit takes it out of the catalog at once | @F002 @AC-021 @FR-011 @FR-015 @US3-AS4 | passed | 00:00:00.0986395 |
| F002 | Field checks refuse a custom habit with the same messages(n: "06", name: "`No icon`", description: "none", category: "Body", intent: "Build", icon: "none", result: "\"Choose an icon.\"", __pickleIndex: "224", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0736988 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "09", fields: "Build, yes/no, fixed days Saturday and Sunday", text: "\"2 days a week\"", kind: "\"Yes/no · fixed days Sat, Sun\"", __pickleIndex: "36", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.1042158 |
| F002 | The role and the identifier are checked before the fields(n: "01", actingUser: "the registered user `ac039.user.01@example.com`", action: "creates a catalog habit with the name `\"   \"`", result: "\"Only administrators can manage the catalog.\"", __pickleIndex: "149", exampleTags: []) | @F002 @AC-039 @FR-016 @US3-AS18 | passed | 00:00:00.2047206 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "05", second: "Build, count with no amount and the unit `pages`, "···, result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "120", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0867166 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "03", state: "Unpublished", action: "creates a catalog habit", __pickleIndex: "66", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | passed | 00:00:00.0916632 |
| F002 | A registered user cannot manage the catalog(n: "06", action: "unpublishes `A030 06 Draft`", __pickleIndex: "93", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1859291 |
| F002 | A new catalog habit is a Draft that the public catalog does not show | @F002 @AC-018 @FR-011 @FR-012 @US3-AS1 | failed | 00:00:00.0867217 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "02", action: "reads `Secret` by its identifier", __pickleIndex: "208", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1679359 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "01", count: "2", marks: "none marked", result: "\"Mark one suggested starting goal for the card.\"", after: "no suggested starting goal", __pickleIndex: "109", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0961892 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "06", state: "Unpublished", action: "edits `A026 06 Other`", __pickleIndex: "69", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | failed | 00:00:00.0824251 |
| F002 | Category, intent and search apply together | @F002 @AC-010 @FR-009 | passed | 00:00:00.1236872 |
| F002 | Only the 24 habit icons are accepted(n: "06", icon: "`candy`", result: "success", __pickleIndex: "169", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0938596 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "01", second: "no Build or Reduce, yes/no, 1 time per day", result: "\"Choose Build or Reduce.\"", __pickleIndex: "116", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0917834 |
| F002 | Only the 24 habit icons are accepted(n: "21", icon: "`receipt`", result: "success", __pickleIndex: "184", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0876128 |
| F002 | Publish and unpublish check the acting user and the role before the identifier(n: "02", actingUser: "the registered user `ac046.user.02@example.com`", action: "unpublishes", result: "\"Only administrators can manage the catalog.\"", __pickleIndex: "194", exampleTags: []) | @F002 @AC-046 @FR-016 | passed | 00:00:00.0843265 |
| F002 | The anonymous visitor cannot manage the catalog(n: "01", action: "lists the catalog habits as an administrator", __pickleIndex: "94", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0875846 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "04", action: "reads a custom habit with the identifier of `A058 "···, __pickleIndex: "234", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.1830566 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "02", actingUser: "the anonymous visitor", habit: "`A016 02 Old habit`", __pickleIndex: "45", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1847065 |
| F002 | A user's list holds only their own custom habits, in name order ignoring case | @F002 @AC-048 @FR-004 @FR-020 @US4-AS2 | failed | 00:00:00.0863793 |
| F002 | Field checks come in a fixed order(n: "02", fields: "a name of 61 characters with the prefix `A029 02`,"···, result: "\"The name can have at most 60 characters.\"", __pickleIndex: "80", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0911336 |
| F002 | Only the 24 habit icons are accepted(n: "17", icon: "`utensils`", result: "success", __pickleIndex: "180", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0807055 |
| F002 | Details with a custom habit's identifier give "not in the catalog"(n: "02", actingUser: "the administrator `ac017.admin.02@example.com`", __pickleIndex: "54", exampleTags: []) | @F002 @AC-017 @FR-010 @FR-024 @US2-AS5 | failed | 00:00:00.0016271 |
| F002 | Browsing by category returns only that category | @F002 @AC-003 @FR-009 @US1-AS3 | passed | 00:00:00.0989480 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "02", fields: "Build, count 999 `pages`, 20 times per week", storedUnit: "`pages`", __pickleIndex: "139", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0855936 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "03", action: "edits `Secret` by its identifier, with the name `S"···, __pickleIndex: "209", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1648452 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "02", search: "`\"MORNING\"`", morning: "contains", evening: "does not contain", __pickleIndex: "9", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.0963937 |
| F002 | The anonymous visitor cannot manage the catalog(n: "02", action: "reads `A031 02 Published` as an administrator", __pickleIndex: "95", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0841165 |
| F002 | Suggested starting goals are checked in list order, and each by its first failing field(n: "03", goals: "(1) Build, yes/no, 1 time per day (marked); (2) Bu"···, result: "\"Choose fixed days or how many times.\"", position: "3", __pickleIndex: "156", exampleTags: []) | @F002 @AC-041 @FR-007 | passed | 00:00:00.0832533 |
| F002 | An unknown identifier does not exist for the administrator(n: "04", action: "unpublishes", __pickleIndex: "103", exampleTags: []) | @F002 @AC-032 @FR-013 @US3-AS12 | passed | 00:00:00.0873421 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "09", fields: "Build, count 5 with the unit `\"  \"` followed by "···, storedUnit: "that unit of 20 characters, without the spaces", __pickleIndex: "146", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0844188 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "06", action: "creates a custom habit with the name `\"   \"` and"···, __pickleIndex: "212", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.2179869 |
| F002 | Names and descriptions are trimmed before they are checked and stored(n: "03", name: "`\"   \"` + `A070 03 ` followed by `x` up to 60 ch"···, description: "none", storedName: "the 60-character name", storedDescription: "no description", __pickleIndex: "252", exampleTags: []) | @F002 @AC-070 @FR-004 @FR-005 | passed | 00:00:00.0901581 |
| F002 | An unknown identifier does not exist for the administrator(n: "02", action: "edits, with the name `A032 02 Name`, the category "···, __pickleIndex: "101", exampleTags: []) | @F002 @AC-032 @FR-013 @US3-AS12 | passed | 00:00:00.0867059 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "03", count: "6", marks: "none marked", result: "\"A habit can have at most 5 suggested starting go"···, after: "no suggested starting goal", __pickleIndex: "111", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0895480 |
| F002 | Only the 24 habit icons are accepted(n: "12", icon: "`brain`", result: "success", __pickleIndex: "175", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0856222 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "21", second: "Build, duration 15 minutes, 21 times per week", result: "\"Enter a number of times from 1 to 20.\"", __pickleIndex: "136", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0838699 |
| F002 | Custom-habit names and descriptions are trimmed before they are checked and stored(n: "03", name: "`\"   \"` + `Habit ` followed by `x` up to 60 char"···, description: "none", storedName: "the 60-character name", storedDescription: "no description", __pickleIndex: "258", exampleTags: []) | @F002 @AC-073 @FR-004 @FR-005 | passed | 00:00:00.0824117 |
| F002 | Only the 24 habit icons are accepted(n: "15", icon: "`music`", result: "success", __pickleIndex: "178", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0915413 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "02", second: "Build, no measurement, 1 time per day", result: "\"Choose how it is measured.\"", __pickleIndex: "117", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0918359 |
| F002 | Only the 24 habit icons are accepted(n: "07", icon: "`coffee`", result: "success", __pickleIndex: "170", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0945964 |
| F002 | Only the 24 habit icons are accepted(n: "23", icon: "`sparkles`", result: "success", __pickleIndex: "186", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0878661 |
| F002 | The category filter counts only Published catalog habits(n: "03", actingUser: "the administrator `ac009.admin.03@example.com`", __pickleIndex: "22", exampleTags: []) | @F002 @AC-009 @FR-002 @FR-009 @FR-024 @US1-AS9 | passed | 00:00:00.3351636 |
| F002 | A custom habit is not a catalog habit for the administrator(n: "01", action: "reads", __pickleIndex: "104", exampleTags: []) | @F002 @AC-033 @FR-013 @FR-024 @US3-AS13 | passed | 00:00:00.1967760 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "19", second: "Build, duration 15 minutes, fixed days with no day", result: "\"Choose at least one day.\"", __pickleIndex: "134", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0863100 |
| F002 | The category filter starts with All and follows the fixed order | @F002 @AC-008 @FR-002 @FR-009 @US1-AS8 | passed | 00:00:00.0924659 |
| F002 | Only the 24 habit icons are accepted(n: "28", icon: "`rocket`", result: "\"Choose an icon.\"", __pickleIndex: "191", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0763757 |
| F002 | Only the 24 habit icons are accepted(n: "10", icon: "`droplet`", result: "success", __pickleIndex: "173", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0776941 |
| F002 | The anonymous visitor cannot manage the catalog(n: "06", action: "unpublishes `A031 06 Published`", __pickleIndex: "99", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0827616 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "07", count: "1", marks: "none marked", result: "\"Mark one suggested starting goal for the card.\"", after: "no suggested starting goal", __pickleIndex: "115", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0940368 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "03", second: "Build, yes/no with the amount 5, 1 time per day", result: "\"A yes/no suggested starting goal has no amount o"···, __pickleIndex: "118", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0986987 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "01", fields: "Build, count 1 `page`, 1 time per day", storedUnit: "`page`", __pickleIndex: "138", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0944073 |
| F002 | Custom-habit names and descriptions are trimmed before they are checked and stored(n: "04", name: "`\"Long\"`", description: "`\"  \"` + a text of 500 characters + `\"  \"` (50"···, storedName: "`Long`", storedDescription: "the 500-character description", __pickleIndex: "259", exampleTags: []) | @F002 @AC-073 @FR-004 @FR-005 | passed | 00:00:00.0935254 |
| F002 | Only the 24 habit icons are accepted(n: "02", icon: "`person-standing`", result: "success", __pickleIndex: "165", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0812698 |
| F002 | For custom habits the acting user and ownership come first, the name check last(n: "03", actingUser: "the registered user `ac057.b.03@example.com`", action: "edits an unknown identifier with the name `\"   \""···, result: "\"This custom habit does not exist.\"", __pickleIndex: "228", exampleTags: []) | @F002 @AC-057 @FR-005 @FR-016 @US4-AS11 | passed | 00:00:00.1831235 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "01", actingUser: "the anonymous visitor", habit: "`A016 01 Draft habit`", __pickleIndex: "44", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1836402 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "15", fields: "Build, yes/no, fixed days Monday, Tuesday, Wednesd"···, text: "\"6 days a week\"", kind: "\"Yes/no · fixed days Mon–Wed, Fri–Sun\"", __pickleIndex: "42", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.1001804 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "05", state: "Published", action: "edits `A026 05 Other`", __pickleIndex: "68", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | failed | 00:00:00.0938724 |
| F002 | The role and the identifier are checked before the fields(n: "04", actingUser: "the registered user `ac039.user.04@example.com`", action: "edits an unknown identifier with the name `\"   \""···, result: "\"Only administrators can manage the catalog.\"", __pickleIndex: "152", exampleTags: []) | @F002 @AC-039 @FR-016 @US3-AS18 | passed | 00:00:00.1854893 |
| F002 | The role and the identifier are checked before the fields(n: "02", actingUser: "the anonymous visitor", action: "creates a catalog habit with the name `\"   \"`", result: "\"You are not signed in.\"", __pickleIndex: "150", exampleTags: []) | @F002 @AC-039 @FR-016 @US3-AS18 | passed | 00:00:00.1867694 |
| F002 | Only the 24 habit icons are accepted(n: "13", icon: "`pen-line`", result: "success", __pickleIndex: "176", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0916158 |
| F002 | Deleted demo custom habits are not created again | @F002 @AC-066 @FR-031 @US6-AS7 | failed | 00:00:00.3176286 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "07", action: "edits `Secret` by its identifier, with the name `\"···, __pickleIndex: "213", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1724390 |
| F002 | Custom-habit names and descriptions are trimmed before they are checked and stored(n: "02", name: "`\"Blank\"`", description: "`\"   \"`", storedName: "`Blank`", storedDescription: "no description", __pickleIndex: "257", exampleTags: []) | @F002 @AC-073 @FR-004 @FR-005 | passed | 00:00:00.0802371 |
| F002 | Field checks refuse a custom habit with the same messages(n: "05", name: "`No intent`", description: "none", category: "Body", intent: "none", icon: "`droplet`", result: "\"Choose Build or Reduce.\"", __pickleIndex: "223", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0844919 |
| F002 | Publish and unpublish check the acting user and the role before the identifier(n: "03", actingUser: "the anonymous visitor", action: "publishes", result: "\"You are not signed in.\"", __pickleIndex: "195", exampleTags: []) | @F002 @AC-046 @FR-016 | passed | 00:00:00.0810720 |
| F002 | Publish and unpublish check the acting user and the role before the identifier(n: "01", actingUser: "the registered user `ac046.user.01@example.com`", action: "publishes", result: "\"Only administrators can manage the catalog.\"", __pickleIndex: "193", exampleTags: []) | @F002 @AC-046 @FR-016 | passed | 00:00:00.0750067 |
| F002 | A registered user cannot manage the catalog(n: "05", action: "publishes `A030 05 Draft`", __pickleIndex: "92", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1747447 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "12", fields: "Build, duration 20 minutes, 1 time per day", text: "\"20 min a day\"", kind: "\"Duration · flexible\"", __pickleIndex: "39", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0825303 |
| F002 | The anonymous visitor has no access to custom habits(n: "01", action: "lists custom habits", __pickleIndex: "214", exampleTags: []) | @F002 @AC-055 @FR-023 @US4-AS9 | failed | 00:00:00.0011906 |
| F002 | Only the 24 habit icons are accepted(n: "04", icon: "`footprints`", result: "success", __pickleIndex: "167", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0886253 |
| F002 | Publishing a Draft makes it public | @F002 @AC-019 @FR-011 @US3-AS2 | passed | 00:00:00.0886963 |
| F002 | The anonymous visitor has no access to custom habits(n: "04", action: "edits `Secret` by its identifier, with the name `S"···, __pickleIndex: "217", exampleTags: []) | @F002 @AC-055 @FR-023 @US4-AS9 | failed | 00:00:00.0002110 |
| F002 | Each of the six categories can be given to a catalog habit(n: "01", category: "Body", __pickleIndex: "158", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0862295 |
| F002 | Custom habits never appear in browsing, for any audience(n: "02", actingUser: "the registered user `ac006.other.02@example.com`", __pickleIndex: "15", exampleTags: []) | @F002 @AC-006 @FR-024 @US1-AS6 | failed | 00:00:00.0769613 |
| F002 | Names and descriptions are trimmed before they are checked and stored(n: "04", name: "`\"A070 04 Long\"`", description: "`\"  \"` + a text of 500 characters + `\"  \"` (50"···, storedName: "`A070 04 Long`", storedDescription: "the 500-character description", __pickleIndex: "253", exampleTags: []) | @F002 @AC-070 @FR-004 @FR-005 | passed | 00:00:00.1005528 |
| F002 | Each of the six categories can be given to a catalog habit(n: "05", category: "Home", __pickleIndex: "162", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0771270 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "02", name: "`\"\"`", description: "none", category: "Body", intent: "Build", icon: "`book-open`", result: "\"Enter a name.\"", __pickleIndex: "71", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0912419 |
| F002 | Another registered user cannot reach a custom habit(n: "01", action: "reads", __pickleIndex: "204", exampleTags: []) | @F002 @AC-053 @FR-021 @FR-022 @US4-AS7 | passed | 00:00:00.1577091 |
| F002 | Field checks come in a fixed order(n: "05", fields: "the name `A029 05 Int`, the category Body, no inte"···, result: "\"Choose Build or Reduce.\"", __pickleIndex: "83", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0938970 |
| F002 | The list of suggested starting goals is checked for its length first, then for its mark(n: "05", count: "5", marks: "the third marked", result: "success", after: "5 suggested starting goals, the third marked", __pickleIndex: "113", exampleTags: []) | @F002 @AC-035 @FR-006 @US3-AS15 | passed | 00:00:00.0907559 |
| F002 | Only the 24 habit icons are accepted(n: "18", icon: "`cigarette-off`", result: "success", __pickleIndex: "181", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0962741 |
| F002 | Only the 24 habit icons are accepted(n: "25", icon: "`menu`", result: "\"Choose an icon.\"", __pickleIndex: "188", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0852591 |
| F002 | A catalog-habit name is taken in every state, ignoring case and spaces(n: "02", state: "Published", action: "creates a catalog habit", __pickleIndex: "65", exampleTags: []) | @F002 @AC-026 @FR-004 @FR-005 @US3-AS9 | passed | 00:00:00.0873428 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "13", fields: "Build, yes/no, fixed days Monday to Saturday", text: "\"6 days a week\"", kind: "\"Yes/no · fixed days Mon–Sat\"", __pickleIndex: "40", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0869550 |
| F002 | Another registered user cannot reach a custom habit(n: "02", action: "edits, with the name `Mine now`, the category Body"···, __pickleIndex: "205", exampleTags: []) | @F002 @AC-053 @FR-021 @FR-022 @US4-AS7 | passed | 00:00:00.1741263 |
| F002 | Browsing by intent returns only that intent(n: "01", intent: "Reduce", sugar: "contains", water: "does not contain", __pickleIndex: "5", exampleTags: []) | @F002 @AC-004 @FR-009 @US1-AS4 | passed | 00:00:00.0814064 |
| F002 | Only the 24 habit icons are accepted(n: "14", icon: "`languages`", result: "success", __pickleIndex: "177", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0761199 |
| F002 | Custom habits never appear in browsing, for any audience(n: "04", actingUser: "the anonymous visitor", __pickleIndex: "17", exampleTags: []) | @F002 @AC-006 @FR-024 @US1-AS6 | failed | 00:00:00.0781182 |
| F002 | The anonymous visitor has no access to custom habits(n: "03", action: "creates the custom habit `Visitor habit`", __pickleIndex: "216", exampleTags: []) | @F002 @AC-055 @FR-023 @US4-AS9 | failed | 00:00:00.0006393 |
| F002 | An Unpublished habit can be published again | @F002 @AC-022 @FR-011 @US3-AS5 | passed | 00:00:00.0960182 |
| F002 | Each of the six categories can be given to a catalog habit(n: "06", category: "Other", __pickleIndex: "163", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0845127 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "08", name: "`A027 08 Menu icon`", description: "none", category: "Body", intent: "Build", icon: "`menu`", result: "\"Choose an icon.\"", __pickleIndex: "77", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0917345 |
| F002 | Field checks come in a fixed order(n: "01", fields: "the name `\"   \"`, no category and no icon", result: "\"Enter a name.\"", __pickleIndex: "79", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0935060 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "06", second: "Build, count 0 `pages`, 1 time per day", result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "121", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0976251 |
| F002 | A registered user cannot manage the catalog(n: "02", action: "reads `A030 02 Draft` as an administrator", __pickleIndex: "89", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.1781390 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "01", search: "`\"  a005 01 morn  \"`", morning: "contains", evening: "does not contain", __pickleIndex: "8", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.0959171 |
| F002 | A registered user cannot manage the catalog(n: "01", action: "lists the catalog habits as an administrator", __pickleIndex: "88", exampleTags: []) | @F002 @AC-030 @FR-014 @US3-AS11 | passed | 00:00:00.2200757 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "07", actingUser: "the administrator `ac016.admin.07@example.com`", habit: "`A016 07 Draft habit`", __pickleIndex: "50", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1750053 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "06", name: "`A027 06 No intent`", description: "none", category: "Body", intent: "none", icon: "`book-open`", result: "\"Choose Build or Reduce.\"", __pickleIndex: "75", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0850347 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "04", second: "Build, yes/no with the unit `pages`, 1 time per da"···, result: "\"A yes/no suggested starting goal has no amount o"···, __pickleIndex: "119", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0940953 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "03", action: "deletes `Gone` by its identifier", __pickleIndex: "233", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.1712173 |
| F002 | Field checks come in a fixed order(n: "09", fields: "the name `a029 09 taken`, the category Body, the i"···, result: "\"A catalog habit with this name already exists.\"", __pickleIndex: "87", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0795575 |
| F002 | Browsing by intent returns only that intent(n: "03", intent: "both", sugar: "contains", water: "contains", __pickleIndex: "7", exampleTags: []) | @F002 @AC-004 @FR-009 @US1-AS4 | passed | 00:00:00.0878788 |
| F002 | Only the 24 habit icons are accepted(n: "16", icon: "`salad`", result: "success", __pickleIndex: "179", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0838807 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "13", second: "Reduce, count with no amount and the unit `sweets`"···, result: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "128", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0954289 |
| F002 | A registered user deletes their custom habit | @F002 @AC-050 @FR-020 @US4-AS4 | passed | 00:00:00.0978613 |
| F002 | For custom habits the acting user and ownership come first, the name check last(n: "05", actingUser: "the registered user `ac057.a.05@example.com`", action: "edits `Secret` to the name `TAKEN` and the icon `m"···, result: "\"Choose an icon.\"", __pickleIndex: "230", exampleTags: []) | @F002 @AC-057 @FR-005 @FR-016 @US4-AS11 | passed | 00:00:00.1742912 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "08", fields: "Build, count 5 with the unit `\"  Pages  \"` (5 ch"···, storedUnit: "`Pages`", __pickleIndex: "145", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0942496 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "14", fields: "Build, yes/no, fixed days Monday, Tuesday and Sund"···, text: "\"3 days a week\"", kind: "\"Yes/no · fixed days Mon, Tue, Sun\"", __pickleIndex: "41", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.0819386 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "09", second: "Build, duration 0 minutes, 1 time per day", result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "124", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0979978 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "04", action: "deletes `Secret` by its identifier", __pickleIndex: "210", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1563077 |
| F002 | Demo loading without a valid demo password creates no catalog and no custom habit(n: "02", demoPassword: "`\"   \"`", result: "\"no demo password configured\"", __pickleIndex: "242", exampleTags: []) | @F002 @AC-063 @FR-030 @FR-031 @US6-AS4 | passed | 00:00:00.3123763 |
| F002 | A catalog habit's identifier and a deleted custom habit do not exist as custom habits(n: "02", action: "edits `Gone` by its identifier, with the name `Bac"···, __pickleIndex: "232", exampleTags: []) | @F002 @AC-058 @FR-022 | passed | 00:00:00.1877190 |
| F002 | Only the 24 habit icons are accepted(n: "03", icon: "`smartphone`", result: "success", __pickleIndex: "166", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0933822 |
| F002 | Only the 24 habit icons are accepted(n: "01", icon: "`book-open`", result: "success", __pickleIndex: "164", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0747294 |
| F002 | A user's custom-habit name is taken, ignoring case and spaces(n: "02", action: "edits `Evening walk`", __pickleIndex: "202", exampleTags: []) | @F002 @AC-051 @FR-004 @FR-005 @US4-AS5 | failed | 00:00:00.0799480 |
| F002 | Field checks refuse a custom habit with the same messages(n: "02", name: "a name of 61 characters with the prefix `Habit`", description: "none", category: "Body", intent: "Build", icon: "`droplet`", result: "\"The name can have at most 60 characters.\"", __pickleIndex: "220", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0905554 |
| F002 | Field checks come in a fixed order(n: "08", fields: "the name `A029 08 List`, the category Body, the in"···, result: "\"Mark one suggested starting goal for the card.\"", __pickleIndex: "86", exampleTags: []) | @F002 @AC-029 @FR-005 @FR-016 | passed | 00:00:00.0747405 |
| F002 | Suggested starting goals keep their order and the card shows the marked one | @F002 @AC-034 @FR-006 @US3-AS14 | passed | 00:00:00.1105015 |
| F002 | An unknown identifier does not exist for the administrator(n: "03", action: "publishes", __pickleIndex: "102", exampleTags: []) | @F002 @AC-032 @FR-013 @US3-AS12 | passed | 00:00:00.0943491 |
| F002 | Field checks refuse a custom habit with the same messages(n: "03", name: "`Long text`", description: "a text of 501 characters", category: "Body", intent: "Build", icon: "`droplet`", result: "\"The description can have at most 500 characters."···, __pickleIndex: "221", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0761910 |
| F002 | The text and kind of a suggested starting goal follow fixed rules(n: "08", fields: "Build, yes/no, fixed days Monday, Tuesday, Wednesd"···, text: "\"4 days a week\"", kind: "\"Yes/no · fixed days Mon–Wed, Sat\"", __pickleIndex: "35", exampleTags: []) | @F002 @AC-015 @FR-017 @US2-AS3 | passed | 00:00:00.6826098 |
| F002 | Only the 24 habit icons are accepted(n: "08", icon: "`moon`", result: "success", __pickleIndex: "171", exampleTags: []) | @F002 @AC-044 @FR-003 | failed | 00:00:00.0809434 |
| F002 | Names are ordered ordinally, ignoring case, not by the current culture | @F002 @AC-045 @FR-004 | passed | 00:00:00.0969091 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "01", action: "lists custom habits", __pickleIndex: "207", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | passed | 00:00:00.1677646 |
| F002 | Each of the six categories can be given to a catalog habit(n: "04", category: "Money", __pickleIndex: "161", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0770784 |
| F002 | Custom-habit names and descriptions are trimmed before they are checked and stored(n: "01", name: "`\"   Cold shower   \"`", description: "`\"  In the morning.  \"`", storedName: "`Cold shower`", storedDescription: "the description \"In the morning.\"", __pickleIndex: "256", exampleTags: []) | @F002 @AC-073 @FR-004 @FR-005 | passed | 00:00:00.0797114 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "16", second: "Build, count 5 with a unit of 21 characters, 1 tim"···, result: "\"The unit can have at most 20 characters.\"", __pickleIndex: "131", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0889398 |
| F002 | A user's custom-habit name is taken, ignoring case and spaces(n: "01", action: "creates a custom habit", __pickleIndex: "201", exampleTags: []) | @F002 @AC-051 @FR-004 @FR-005 @US4-AS5 | passed | 00:00:00.0916335 |
| F002 | Names and descriptions are trimmed before they are checked and stored(n: "02", name: "`\"A070 02 Blank\"`", description: "`\"   \"`", storedName: "`A070 02 Blank`", storedDescription: "no description", __pickleIndex: "251", exampleTags: []) | @F002 @AC-070 @FR-004 @FR-005 | passed | 00:00:00.0934207 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "04", fields: "Reduce, duration 999 minutes, 20 times per day", storedUnit: "minutes", __pickleIndex: "141", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0878797 |
| F002 | Each of the six categories can be given to a catalog habit(n: "02", category: "Mind", __pickleIndex: "159", exampleTags: []) | @F002 @AC-043 @FR-002 | passed | 00:00:00.0854189 |
| F002 | Demo loading loads the seed catalog into an empty catalog | @F002 @AC-060 @FR-030 @US6-AS1 | passed | 00:00:00.5500796 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "03", actingUser: "the anonymous visitor", habit: "an unknown identifier", __pickleIndex: "46", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1711137 |
| F002 | For custom habits the acting user and ownership come first, the name check last(n: "02", actingUser: "the registered user `ac057.b.02@example.com`", action: "edits `Secret` of `ac057.a.02@example.com` with th"···, result: "\"This custom habit does not exist.\"", __pickleIndex: "227", exampleTags: []) | @F002 @AC-057 @FR-005 @FR-016 @US4-AS11 | passed | 00:00:00.1615840 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "03", search: "`\"ning pa\"`", morning: "contains", evening: "does not contain", __pickleIndex: "10", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.0928281 |
| F002 | Search matches part of the name, trimmed and ignoring case(n: "04", search: "`\"a005 04 e\"`", morning: "does not contain", evening: "contains", __pickleIndex: "11", exampleTags: []) | @F002 @AC-005 @FR-009 @US1-AS5 | passed | 00:00:00.0848093 |
| F002 | For custom habits the acting user and ownership come first, the name check last(n: "01", actingUser: "the anonymous visitor", action: "creates a custom habit with the name `\"   \"`", result: "\"You are not signed in.\"", __pickleIndex: "226", exampleTags: []) | @F002 @AC-057 @FR-005 @FR-016 @US4-AS11 | passed | 00:00:00.1581343 |
| F002 | The role and the identifier are checked before the fields(n: "03", actingUser: "the administrator `ac039.admin.03@example.com`", action: "edits an unknown identifier with the name `\"   \""···, result: "\"This catalog habit does not exist.\"", __pickleIndex: "151", exampleTags: []) | @F002 @AC-039 @FR-016 @US3-AS18 | passed | 00:00:00.1742626 |
| F002 | Custom habits never appear in browsing, for any audience(n: "03", actingUser: "the administrator `ac006.admin.03@example.com`", __pickleIndex: "16", exampleTags: []) | @F002 @AC-006 @FR-024 @US1-AS6 | failed | 00:00:00.0875224 |
| F002 | The anonymous visitor cannot manage the catalog(n: "05", action: "publishes `A031 05 Published`", __pickleIndex: "98", exampleTags: []) | @F002 @AC-031 @FR-014 @US3-AS11 | passed | 00:00:00.0825021 |
| F002 | A habit without suggested starting goals is shown without one | @F002 @AC-014 @FR-008 @FR-010 @US2-AS2 | passed | 00:00:00.0836855 |
| F002 | Publish and unpublish check the acting user and the role before the identifier(n: "04", actingUser: "the anonymous visitor", action: "unpublishes", result: "\"You are not signed in.\"", __pickleIndex: "196", exampleTags: []) | @F002 @AC-046 @FR-016 | passed | 00:00:00.0775969 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "15", second: "Build, count 5 with the unit `\"  \"`, 1 time per "···, result: "\"Enter a unit, for example pages.\"", __pickleIndex: "130", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0918224 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "06", actingUser: "the registered user `ac016.user.06@example.com`", habit: "an unknown identifier", __pickleIndex: "49", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.2021689 |
| F002 | Browsing with no match gives an empty result | @F002 @AC-007 @FR-008 @FR-009 @US1-AS7 | passed | 00:00:00.0799014 |
| F002 | The bounds of a suggested starting goal's fields are accepted(n: "10", fields: "Build, duration 1 hours, fixed day Sunday", storedUnit: "hours", __pickleIndex: "147", exampleTags: []) | @F002 @AC-037 @FR-007 @US3-AS16 | passed | 00:00:00.0921455 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "14", second: "Reduce, duration 1000 hours, 1 time per day", result: "\"Enter a whole number from 0 to 999.\"", __pickleIndex: "129", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0870440 |
| F002 | Each field of a suggested starting goal has its own refusal, with the position(n: "07", second: "Build, count 1000 `pages`, 1 time per day", result: "\"Enter a whole number from 1 to 999.\"", __pickleIndex: "122", exampleTags: []) | @F002 @AC-036 @FR-007 @US3-AS16 | passed | 00:00:00.0914568 |
| F002 | Details of a habit that is not Published give the same refusal to every audience(n: "09", actingUser: "the administrator `ac016.admin.09@example.com`", habit: "an unknown identifier", __pickleIndex: "52", exampleTags: []) | @F002 @AC-016 @FR-010 @US2-AS4 | passed | 00:00:00.1660791 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "05", name: "`A027 05 No category`", description: "none", category: "none", intent: "Build", icon: "`book-open`", result: "\"Choose a category.\"", __pickleIndex: "74", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0832406 |
| F002 | A demo email that a person registered first keeps its owner's data untouched | @F002 @AC-067 @FR-031 @US6-AS8 | passed | 00:00:00.3600350 |
| F002 | Field checks refuse a catalog habit with a message per field(n: "03", name: "a name of 61 characters with the prefix `A027 03`", description: "none", category: "Body", intent: "Build", icon: "`book-open`", result: "\"The name can have at most 60 characters.\"", __pickleIndex: "72", exampleTags: []) | @F002 @AC-027 @FR-003 @FR-005 @US3-AS10 | passed | 00:00:00.0843626 |
| F002 | Field checks refuse a custom habit with the same messages(n: "04", name: "`No category`", description: "none", category: "none", intent: "Build", icon: "`droplet`", result: "\"Choose a category.\"", __pickleIndex: "222", exampleTags: []) | @F002 @AC-056 @FR-005 @US4-AS10 | passed | 00:00:00.0848723 |
| F002 | An administrator has no access to custom habits, whatever the fields(n: "05", action: "creates the custom habit `Admin habit`", __pickleIndex: "211", exampleTags: []) | @F002 @AC-054 @FR-016 @FR-025 @US4-AS8 | failed | 00:00:00.1632285 |
| F002 | Details with a custom habit's identifier give "not in the catalog"(n: "03", actingUser: "the anonymous visitor", __pickleIndex: "55", exampleTags: []) | @F002 @AC-017 @FR-010 @FR-024 @US2-AS5 | failed | 00:00:00.0003626 |

### F002/AC-044 Only the 24 habit icons are accepted(n: "20", icon: "`wallet`", result: "success", __pickleIndex: "183", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.20@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.20@ex...") (0,1s)
When the administrator creates the catalog habit `A044 20` with the icon `wallet`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-049 A registered user edits every field of their custom habit — failed

Step trace:
```
Given the registered user `ac049.user@example.com`, who created the custom habit `Cold shower` with the category Body, the intent Build, the icon `droplet` and no description
-> done: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit("ac049.user@exampl...", "Cold shower", "the category Body...") (0,1s)
When the registered user edits it to the name `Cold shower, 2 min` and the intent Reduce, keeping its other fields
-> done: EditSteps.WhenEditsItTo("the registered user", "the name `Cold sh...") (0,0s)
And then edits it, keeping the name `Cold shower, 2 min`, to the category Health, the icon `moon` and the description "In the morning."
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.EditSteps.WhenEditsItKeepingTheName(String, String, String)' should have 2 parameters
Then both results are success
-> skipped because of previous errors
And reading it gives the name `Cold shower, 2 min`, the category Health, the intent Reduce, the icon `moon` and the description "In the morning."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.EditSteps.WhenEditsItKeepingTheName(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.ARegisteredUserEditsEveryFieldOfTheirCustomHabit() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 614
[non-AC frame(s) hidden]
```

### F002/AC-030 A registered user cannot manage the catalog(n: "03", action: "creates the catalog habit `A030 03 New`", __pickleIndex: "90", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac030.admin.03@example.com` and a Draft `A030 03 Draft`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("ac030.admin.03@ex...", "a Draft `A030 03 ...") (0,1s)
And the registered user `ac030.user.03@example.com`
-> done: ArrangeSteps.GivenTheRegisteredUser("ac030.user.03@exa...") (0,1s)
When the registered user `ac030.user.03@example.com` creates the catalog habit `A030 03 New`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 2 parameters
Then the result is "Only administrators can manage the catalog."
-> skipped because of previous errors
And the administrator's list still gives `A030 03 Draft` the state Draft and contains no `A030 03 New` and no `A030 03 Renamed`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.ARegisteredUserCannotManageTheCatalog(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 340
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "22", icon: "`sprout`", result: "success", __pickleIndex: "185", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.22@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.22@ex...") (0,1s)
When the administrator creates the catalog habit `A044 22` with the icon `sprout`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "05", icon: "`piggy-bank`", result: "success", __pickleIndex: "168", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.05@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.05@ex...") (0,1s)
When the administrator creates the catalog habit `A044 05` with the icon `piggy-bank`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-006 Custom habits never appear in browsing, for any audience(n: "01", actingUser: "the registered user `ac006.owner.01@example.com`", __pickleIndex: "14", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac006.admin.01@example.com` and a Published `A006 01 Cold water`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("ac006.admin.01@ex...", "a Published `A006...") (0,1s)
And the registered user `ac006.owner.01@example.com`, who created the custom habit `A006 01 Cold shower`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
And the registered user `ac006.other.01@example.com`
-> skipped because of previous errors
When the registered user `ac006.owner.01@example.com` browses the catalog with the search `A006 01`
-> skipped because of previous errors
Then the result contains `A006 01 Cold water` and does not contain `A006 01 Cold shower`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.CustomHabitsNeverAppearInBrowsingForAnyAudience(String n, String actingUser, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 70
[non-AC frame(s) hidden]
```

### F002/AC-052 Another user, and a catalog habit, may have the same name — failed

Step trace:
```
Given the registered user `ac052.a@example.com`, who created the custom habit `Cold shower`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
And the administrator `ac052.admin@example.com` and a Published `A052 Reading`
-> skipped because of previous errors
And the registered user `ac052.b@example.com`
-> skipped because of previous errors
When the registered user `ac052.b@example.com` creates the custom habit `Cold shower` and the custom habit `A052 Reading`
-> skipped because of previous errors
Then both results are success
-> skipped because of previous errors
And the list of `ac052.b@example.com` is exactly `A052 Reading`, `Cold shower`
-> skipped because of previous errors
And the list of `ac052.a@example.com` is exactly `Cold shower`
-> skipped because of previous errors
And browsing with the search `A052` gives exactly one habit, `A052 Reading`, with the identifier of the catalog habit
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.AnotherUserAndACatalogHabitMayHaveTheSameName() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 645
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "09", icon: "`dumbbell`", result: "success", __pickleIndex: "172", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.09@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.09@ex...") (0,1s)
When the administrator creates the catalog habit `A044 09` with the icon `dumbbell`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-055 The anonymous visitor has no access to custom habits(n: "05", action: "deletes `Secret` by its identifier", __pickleIndex: "218", exampleTags: []) — failed

Step trace:
```
Given the registered user `ac055.owner.05@example.com`, who created the custom habit `Secret`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
When the anonymous visitor deletes `Secret` by its identifier
-> skipped because of previous errors
Then the result is "You are not signed in."
-> skipped because of previous errors
And `ac055.owner.05@example.com` still lists exactly `Secret`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.TheAnonymousVisitorHasNoAccessToCustomHabits(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 685
[non-AC frame(s) hidden]
```

### F002/AC-031 The anonymous visitor cannot manage the catalog(n: "03", action: "creates the catalog habit `A031 03 New`", __pickleIndex: "96", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac031.admin.03@example.com` and a Published `A031 03 Published`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("ac031.admin.03@ex...", "a Published `A031...") (0,1s)
When the anonymous visitor creates the catalog habit `A031 03 New`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 2 parameters
Then the result is "You are not signed in."
-> skipped because of previous errors
And the administrator's list still gives `A031 03 Published` the state Published and contains no `A031 03 New` and no `A031 03 Renamed`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.TheAnonymousVisitorCannotManageTheCatalog(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 356
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "27", icon: "`search`", result: "\"Choose an icon.\"", __pickleIndex: "190", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.27@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.27@ex...") (0,1s)
When the administrator creates the catalog habit `A044 27` with the icon `search`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is "Choose an icon."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "24", icon: "`tv`", result: "success", __pickleIndex: "187", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.24@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.24@ex...") (0,1s)
When the administrator creates the catalog habit `A044 24` with the icon `tv`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-055 The anonymous visitor has no access to custom habits(n: "02", action: "reads `Secret` by its identifier", __pickleIndex: "215", exampleTags: []) — failed

Step trace:
```
Given the registered user `ac055.owner.02@example.com`, who created the custom habit `Secret`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
When the anonymous visitor reads `Secret` by its identifier
-> skipped because of previous errors
Then the result is "You are not signed in."
-> skipped because of previous errors
And `ac055.owner.02@example.com` still lists exactly `Secret`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.TheAnonymousVisitorHasNoAccessToCustomHabits(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 685
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "11", icon: "`bike`", result: "success", __pickleIndex: "174", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.11@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.11@ex...") (0,1s)
When the administrator creates the catalog habit `A044 11` with the icon `bike`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "19", icon: "`wine-off`", result: "success", __pickleIndex: "182", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.19@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.19@ex...") (0,1s)
When the administrator creates the catalog habit `A044 19` with the icon `wine-off`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-012 The Home preview is the first four Published habits in name order — failed

Step trace:
```
Given the AC database is reset, so no account and no habit exists
-> done: ArrangeSteps.GivenTheAcDatabaseIsReset() (0,2s)
And the administrator `ac012.admin@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac012.admin@examp...") (0,1s)
And a Published `A012 Eta`, a Published `A012 Beta`, a Published `A012 Delta`, a Published `A012 Alpha` and a Published `A012 Gamma`
-> done: ArrangeSteps.GivenCatalogHabits("a Published `A012...") (0,0s)
And a Draft `A012 Aardvark` and an Unpublished `A012 Abacus`
-> done: ArrangeSteps.GivenCatalogHabits("a Draft `A012 Aar...") (0,0s)
And the registered user `ac012.user@example.com`, who created the custom habit `A012 Aaa private`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
When the anonymous visitor asks for the Home preview
-> skipped because of previous errors
Then it is exactly `A012 Alpha`, `A012 Beta`, `A012 Delta`, `A012 Eta`, in this order
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.TheHomePreviewIsTheFirstFourPublishedHabitsInNameOrder() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 133
[non-AC frame(s) hidden]
```

### F002/AC-017 Details with a custom habit's identifier give "not in the catalog"(n: "01", actingUser: "the registered user `ac017.owner.01@example.com`", __pickleIndex: "53", exampleTags: []) — failed

Step trace:
```
Given the registered user `ac017.owner.01@example.com`, who created the custom habit `Private one`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
And the administrator `ac017.admin.01@example.com`
-> skipped because of previous errors
When the registered user `ac017.owner.01@example.com` reads the catalog details with the identifier of `Private one`
-> skipped because of previous errors
Then the result is "This habit is not in the catalog."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.DetailsWithACustomHabitsIdentifierGiveNotInTheCatalog(String n, String actingUser, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 202
[non-AC frame(s) hidden]
```

### F002/AC-026 A catalog-habit name is taken in every state, ignoring case and spaces(n: "04", state: "Draft", action: "edits `A026 04 Other`", __pickleIndex: "67", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac026.admin.04@example.com`, a Draft `A026 04 Reading` and a Draft `A026 04 Other`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("ac026.admin.04@ex...", "a Draft `A026 04 ...") (0,1s)
When the administrator edits `A026 04 Other` with the name `" a026 04 READING "`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.EditSteps.WhenEditsWithTheName(String, String, String, String)' should have 3 parameters
Then the result is "A catalog habit with this name already exists."
-> skipped because of previous errors
And the administrator's list has exactly one habit named `A026 04 Reading` and still has `A026 04 Other`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.EditSteps.WhenEditsWithTheName(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.ACatalog_HabitNameIsTakenInEveryStateIgnoringCaseAndSpaces(String n, String state, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 280
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "26", icon: "`trash-2`", result: "\"Choose an icon.\"", __pickleIndex: "189", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.26@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.26@ex...") (0,1s)
When the administrator creates the catalog habit `A044 26` with the icon `trash-2`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is "Choose an icon."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-018 A new catalog habit is a Draft that the public catalog does not show — failed

Step trace:
```
Given the administrator `ac018.admin@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac018.admin@examp...") (0,1s)
When the administrator creates the catalog habit `A018 Stretching` with the category Body, the intent Build, the icon `person-standing`, the description "Gentle stretches." and the suggested starting goal Build, duration 10 minutes, 1 time per day
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
And the administrator's list contains `A018 Stretching` with the state Draft
-> skipped because of previous errors
And browsing does not contain `A018 Stretching`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.ANewCatalogHabitIsADraftThatThePublicCatalogDoesNotShow() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 216
[non-AC frame(s) hidden]
```

### F002/AC-026 A catalog-habit name is taken in every state, ignoring case and spaces(n: "06", state: "Unpublished", action: "edits `A026 06 Other`", __pickleIndex: "69", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac026.admin.06@example.com`, a Unpublished `A026 06 Reading` and a Draft `A026 06 Other`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("ac026.admin.06@ex...", "a Unpublished `A0...") (0,1s)
When the administrator edits `A026 06 Other` with the name `" a026 06 READING "`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.EditSteps.WhenEditsWithTheName(String, String, String, String)' should have 3 parameters
Then the result is "A catalog habit with this name already exists."
-> skipped because of previous errors
And the administrator's list has exactly one habit named `A026 06 Reading` and still has `A026 06 Other`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.EditSteps.WhenEditsWithTheName(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.ACatalog_HabitNameIsTakenInEveryStateIgnoringCaseAndSpaces(String n, String state, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 280
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "06", icon: "`candy`", result: "success", __pickleIndex: "169", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.06@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.06@ex...") (0,1s)
When the administrator creates the catalog habit `A044 06` with the icon `candy`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "21", icon: "`receipt`", result: "success", __pickleIndex: "184", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.21@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.21@ex...") (0,1s)
When the administrator creates the catalog habit `A044 21` with the icon `receipt`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-048 A user's list holds only their own custom habits, in name order ignoring case — failed

Step trace:
```
Given the registered user `ac048.user@example.com`, who created the custom habits `B habit` and then `a habit`
-> done: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabits("ac048.user@exampl...", "B habit", "a habit") (0,1s)
And the registered user `ac048.other@example.com`, who created the custom habit `C habit`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
And the administrator `ac048.admin@example.com` and a Published `A048 Catalog habit`
-> skipped because of previous errors
When the registered user `ac048.user@example.com` lists their custom habits
-> skipped because of previous errors
Then the list is exactly `a habit`, `B habit`, in this order
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.AUsersListHoldsOnlyTheirOwnCustomHabitsInNameOrderIgnoringCase() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 606
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "17", icon: "`utensils`", result: "success", __pickleIndex: "180", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.17@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.17@ex...") (0,1s)
When the administrator creates the catalog habit `A044 17` with the icon `utensils`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-017 Details with a custom habit's identifier give "not in the catalog"(n: "02", actingUser: "the administrator `ac017.admin.02@example.com`", __pickleIndex: "54", exampleTags: []) — failed

Step trace:
```
Given the registered user `ac017.owner.02@example.com`, who created the custom habit `Private one`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
And the administrator `ac017.admin.02@example.com`
-> skipped because of previous errors
When the administrator `ac017.admin.02@example.com` reads the catalog details with the identifier of `Private one`
-> skipped because of previous errors
Then the result is "This habit is not in the catalog."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.DetailsWithACustomHabitsIdentifierGiveNotInTheCatalog(String n, String actingUser, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 202
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "12", icon: "`brain`", result: "success", __pickleIndex: "175", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.12@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.12@ex...") (0,1s)
When the administrator creates the catalog habit `A044 12` with the icon `brain`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "15", icon: "`music`", result: "success", __pickleIndex: "178", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.15@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.15@ex...") (0,1s)
When the administrator creates the catalog habit `A044 15` with the icon `music`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "07", icon: "`coffee`", result: "success", __pickleIndex: "170", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.07@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.07@ex...") (0,1s)
When the administrator creates the catalog habit `A044 07` with the icon `coffee`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "23", icon: "`sparkles`", result: "success", __pickleIndex: "186", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.23@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.23@ex...") (0,1s)
When the administrator creates the catalog habit `A044 23` with the icon `sparkles`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "28", icon: "`rocket`", result: "\"Choose an icon.\"", __pickleIndex: "191", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.28@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.28@ex...") (0,1s)
When the administrator creates the catalog habit `A044 28` with the icon `rocket`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is "Choose an icon."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "10", icon: "`droplet`", result: "success", __pickleIndex: "173", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.10@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.10@ex...") (0,1s)
When the administrator creates the catalog habit `A044 10` with the icon `droplet`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "02", icon: "`person-standing`", result: "success", __pickleIndex: "165", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.02@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.02@ex...") (0,1s)
When the administrator creates the catalog habit `A044 02` with the icon `person-standing`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-026 A catalog-habit name is taken in every state, ignoring case and spaces(n: "05", state: "Published", action: "edits `A026 05 Other`", __pickleIndex: "68", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac026.admin.05@example.com`, a Published `A026 05 Reading` and a Draft `A026 05 Other`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("ac026.admin.05@ex...", "a Published `A026...") (0,1s)
When the administrator edits `A026 05 Other` with the name `" a026 05 READING "`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.EditSteps.WhenEditsWithTheName(String, String, String, String)' should have 3 parameters
Then the result is "A catalog habit with this name already exists."
-> skipped because of previous errors
And the administrator's list has exactly one habit named `A026 05 Reading` and still has `A026 05 Other`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.EditSteps.WhenEditsWithTheName(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.ACatalog_HabitNameIsTakenInEveryStateIgnoringCaseAndSpaces(String n, String state, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 280
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "13", icon: "`pen-line`", result: "success", __pickleIndex: "176", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.13@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.13@ex...") (0,1s)
When the administrator creates the catalog habit `A044 13` with the icon `pen-line`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-066 Deleted demo custom habits are not created again — failed

Step trace:
```
Given the AC database is reset, so no account and no habit exists
-> done: ArrangeSteps.GivenTheAcDatabaseIsReset() (0,1s)
And demo loading ran with the demo password `example2026`
-> done: DemoLoadingSteps.GivenDemoLoadingRan("example2026") (0,2s)
And `demo1@example.com`, signed in with `example2026`, deleted `Cold shower`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.DemoLoadingSteps.GivenSignedInDeleted(String, String, String, String)' should have 3 parameters
And `demo2@example.com`, signed in with `example2026`, deleted `Evening stretch` and `Plan tomorrow`
-> skipped because of previous errors
When demo loading runs again with the demo password `example2026`
-> skipped because of previous errors
Then the result has no custom-habit result for any account
-> skipped because of previous errors
And `demo1@example.com` lists exactly `No phone in bed`
-> skipped because of previous errors
And `demo2@example.com` lists no custom habit
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.DemoLoadingSteps.GivenSignedInDeleted(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.DeletedDemoCustomHabitsAreNotCreatedAgain() in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 836
[non-AC frame(s) hidden]
```

### F002/AC-055 The anonymous visitor has no access to custom habits(n: "01", action: "lists custom habits", __pickleIndex: "214", exampleTags: []) — failed

Step trace:
```
Given the registered user `ac055.owner.01@example.com`, who created the custom habit `Secret`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
When the anonymous visitor lists custom habits
-> skipped because of previous errors
Then the result is "You are not signed in."
-> skipped because of previous errors
And `ac055.owner.01@example.com` still lists exactly `Secret`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.TheAnonymousVisitorHasNoAccessToCustomHabits(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 685
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "04", icon: "`footprints`", result: "success", __pickleIndex: "167", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.04@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.04@ex...") (0,1s)
When the administrator creates the catalog habit `A044 04` with the icon `footprints`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-055 The anonymous visitor has no access to custom habits(n: "04", action: "edits `Secret` by its identifier, with the name `S"···, __pickleIndex: "217", exampleTags: []) — failed

Step trace:
```
Given the registered user `ac055.owner.04@example.com`, who created the custom habit `Secret`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
When the anonymous visitor edits `Secret` by its identifier, with the name `Seen`, the category Body, the intent Build and the icon `droplet`
-> skipped because of previous errors
Then the result is "You are not signed in."
-> skipped because of previous errors
And `ac055.owner.04@example.com` still lists exactly `Secret`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.TheAnonymousVisitorHasNoAccessToCustomHabits(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 685
[non-AC frame(s) hidden]
```

### F002/AC-006 Custom habits never appear in browsing, for any audience(n: "02", actingUser: "the registered user `ac006.other.02@example.com`", __pickleIndex: "15", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac006.admin.02@example.com` and a Published `A006 02 Cold water`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("ac006.admin.02@ex...", "a Published `A006...") (0,1s)
And the registered user `ac006.owner.02@example.com`, who created the custom habit `A006 02 Cold shower`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
And the registered user `ac006.other.02@example.com`
-> skipped because of previous errors
When the registered user `ac006.other.02@example.com` browses the catalog with the search `A006 02`
-> skipped because of previous errors
Then the result contains `A006 02 Cold water` and does not contain `A006 02 Cold shower`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.CustomHabitsNeverAppearInBrowsingForAnyAudience(String n, String actingUser, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 70
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "18", icon: "`cigarette-off`", result: "success", __pickleIndex: "181", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.18@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.18@ex...") (0,1s)
When the administrator creates the catalog habit `A044 18` with the icon `cigarette-off`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "25", icon: "`menu`", result: "\"Choose an icon.\"", __pickleIndex: "188", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.25@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.25@ex...") (0,1s)
When the administrator creates the catalog habit `A044 25` with the icon `menu`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is "Choose an icon."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "14", icon: "`languages`", result: "success", __pickleIndex: "177", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.14@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.14@ex...") (0,1s)
When the administrator creates the catalog habit `A044 14` with the icon `languages`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-006 Custom habits never appear in browsing, for any audience(n: "04", actingUser: "the anonymous visitor", __pickleIndex: "17", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac006.admin.04@example.com` and a Published `A006 04 Cold water`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("ac006.admin.04@ex...", "a Published `A006...") (0,1s)
And the registered user `ac006.owner.04@example.com`, who created the custom habit `A006 04 Cold shower`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
And the registered user `ac006.other.04@example.com`
-> skipped because of previous errors
When the anonymous visitor browses the catalog with the search `A006 04`
-> skipped because of previous errors
Then the result contains `A006 04 Cold water` and does not contain `A006 04 Cold shower`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.CustomHabitsNeverAppearInBrowsingForAnyAudience(String n, String actingUser, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 70
[non-AC frame(s) hidden]
```

### F002/AC-055 The anonymous visitor has no access to custom habits(n: "03", action: "creates the custom habit `Visitor habit`", __pickleIndex: "216", exampleTags: []) — failed

Step trace:
```
Given the registered user `ac055.owner.03@example.com`, who created the custom habit `Secret`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
When the anonymous visitor creates the custom habit `Visitor habit`
-> skipped because of previous errors
Then the result is "You are not signed in."
-> skipped because of previous errors
And `ac055.owner.03@example.com` still lists exactly `Secret`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.TheAnonymousVisitorHasNoAccessToCustomHabits(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 685
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "16", icon: "`salad`", result: "success", __pickleIndex: "179", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.16@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.16@ex...") (0,1s)
When the administrator creates the catalog habit `A044 16` with the icon `salad`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "03", icon: "`smartphone`", result: "success", __pickleIndex: "166", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.03@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.03@ex...") (0,1s)
When the administrator creates the catalog habit `A044 03` with the icon `smartphone`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "01", icon: "`book-open`", result: "success", __pickleIndex: "164", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.01@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.01@ex...") (0,1s)
When the administrator creates the catalog habit `A044 01` with the icon `book-open`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-051 A user's custom-habit name is taken, ignoring case and spaces(n: "02", action: "edits `Evening walk`", __pickleIndex: "202", exampleTags: []) — failed

Step trace:
```
Given the registered user `ac051.user.02@example.com`, who created the custom habits `Cold shower` and `Evening walk`
-> done: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabits("ac051.user.02@exa...", "Cold shower", "Evening walk") (0,1s)
When the registered user edits `Evening walk` with the name `" cold SHOWER "`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.EditSteps.WhenEditsWithTheName(String, String, String, String)' should have 3 parameters
Then the result is "You already have a custom habit with this name."
-> skipped because of previous errors
And their list is still exactly `Cold shower`, `Evening walk`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.EditSteps.WhenEditsWithTheName(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.AUsersCustom_HabitNameIsTakenIgnoringCaseAndSpaces(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 629
[non-AC frame(s) hidden]
```

### F002/AC-044 Only the 24 habit icons are accepted(n: "08", icon: "`moon`", result: "success", __pickleIndex: "171", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac044.admin.08@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac044.admin.08@ex...") (0,1s)
When the administrator creates the catalog habit `A044 08` with the icon `moon`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
Then the result is success
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CatalogAdministrationSteps.WhenCreatesTheCatalogHabit(String, String, String, String)' should have 3 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.OnlyThe24HabitIconsAreAccepted(String n, String icon, String result, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 539
[non-AC frame(s) hidden]
```

### F002/AC-006 Custom habits never appear in browsing, for any audience(n: "03", actingUser: "the administrator `ac006.admin.03@example.com`", __pickleIndex: "16", exampleTags: []) — failed

Step trace:
```
Given the administrator `ac006.admin.03@example.com` and a Published `A006 03 Cold water`
-> done: ArrangeSteps.GivenTheAdministratorAndCatalogHabits("ac006.admin.03@ex...", "a Published `A006...") (0,1s)
And the registered user `ac006.owner.03@example.com`, who created the custom habit `A006 03 Cold shower`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
And the registered user `ac006.other.03@example.com`
-> skipped because of previous errors
When the administrator `ac006.admin.03@example.com` browses the catalog with the search `A006 03`
-> skipped because of previous errors
Then the result contains `A006 03 Cold water` and does not contain `A006 03 Cold shower`
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.CustomHabitsNeverAppearInBrowsingForAnyAudience(String n, String actingUser, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 70
[non-AC frame(s) hidden]
```

### F002/AC-054 An administrator has no access to custom habits, whatever the fields(n: "05", action: "creates the custom habit `Admin habit`", __pickleIndex: "211", exampleTags: []) — failed

Step trace:
```
Given the registered user `ac054.owner.05@example.com`, who created the custom habit `Secret` with the category Mind
-> done: ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit("ac054.owner.05@ex...", "Secret", "the category Mind") (0,1s)
And the administrator `ac054.admin.05@example.com`
-> done: ArrangeSteps.GivenTheAdministrator("ac054.admin.05@ex...") (0,1s)
When the administrator creates the custom habit `Admin habit`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CustomHabitSteps.WhenCreatesTheCustomHabit(String, String, String)' should have 2 parameters
Then the result is the refusal "Custom habits are for registered users." and carries no custom habit
-> skipped because of previous errors
And `ac054.owner.05@example.com` still reads `Secret` with the category Mind
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.CustomHabitSteps.WhenCreatesTheCustomHabit(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.AnAdministratorHasNoAccessToCustomHabitsWhateverTheFields(String n, String action, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 668
[non-AC frame(s) hidden]
```

### F002/AC-017 Details with a custom habit's identifier give "not in the catalog"(n: "03", actingUser: "the anonymous visitor", __pickleIndex: "55", exampleTags: []) — failed

Step trace:
```
Given the registered user `ac017.owner.03@example.com`, who created the custom habit `Private one`
-> binding error: Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
And the administrator `ac017.admin.03@example.com`
-> skipped because of previous errors
When the anonymous visitor reads the catalog details with the identifier of `Private one`
-> skipped because of previous errors
Then the result is "This habit is not in the catalog."
-> skipped because of previous errors
```

Exception:
```
Reqnroll.BindingException : Parameter count mismatch! The binding method 'HT.AcceptanceTests:HT.AcceptanceTests.StepDefinitions.F002.ArrangeSteps.GivenTheRegisteredUserWhoCreatedTheCustomHabit(String, String, String)' should have 2 parameters
```

Stack trace (AC project frames only):
```
[non-AC frame(s) hidden]
at HT.AcceptanceTests.Features.F002.HabitCatalogFeature.DetailsWithACustomHabitsIdentifierGiveNotInTheCatalog(String n, String actingUser, String __pickleIndex, String[] exampleTags) in E:\Personal\agentic-engineering-course-exam\.claude\worktrees\speckit-specify-slice-2-habits-461103\tests\HT.AcceptanceTests\Features\F002\habit-catalog.feature:line 202
[non-AC frame(s) hidden]
```

## Regression (other features)

Total: 93 · passed: 93 · failed: 0 · pending: 0 · skipped: 0

| Feature | Scenario | Tags | Outcome | Duration |
|---|---|---|---|---|
| F001 | Demo loading decides per demo email, and a demo email registered earlier keeps its owner's password | @F001 @AC-060 @FR-003 @FR-016 | passed | 00:00:00.4852531 |
| F001 | Administrator creation checks for a missing password before the email | @F001 @AC-048 @FR-014 | passed | 00:00:00.0010869 |
| F001 | Registration creates a registered user | @F001 @AC-001 @FR-001 @FR-004 @FR-009 @US1-AS1 | passed | 00:00:00.0958373 |
| F001 | Administrator creation accepts a password at the lower bound of the password rule | @F001 @AC-040 @FR-002 @FR-014 | passed | 00:00:00.0890967 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@example.\"", whyItIsNotValid: "domain has one non-empty part, before an empty one", __pickleIndex: "12", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0001951 |
| F001 | The anonymous visitor cannot change a password, whatever the new password(newPassword: "\"run\"", whyItMatters: "breaks the password rule, which is checked only fo"···, __pickleIndex: "56", exampleTags: []) | @F001 @AC-036 @FR-011 @FR-013 @US6-AS5 | passed | 00:00:00.0015132 |
| F001 | Administrator creation finds a registered user whatever the case and spaces of the email | @F001 @AC-063 @FR-003 @FR-014 | passed | 00:00:00.0887812 |
| F001 | Registration keeps the email in its lower-case form | @F001 @AC-006 @FR-003 @FR-006 @FR-012 | passed | 00:00:00.0898437 |
| F001 | An unknown email, a wrong password and a blocked account give the same result | @F001 @AC-014 @FR-007 @FR-008 | passed | 00:00:00.3360020 |
| F001 | Demo loading without a demo password creates no account(demoPassword: "absent", __pickleIndex: "84", exampleTags: []) | @F001 @AC-055 @FR-016 @US7-AS3 | passed | 00:00:00.2786939 |
| F001 | Registration checks the password rule before whether the email is taken | @F001 @AC-008 @FR-003 @US1-AS6 | passed | 00:00:00.0410584 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@example.com.\"", whyItIsNotValid: "empty domain part after two non-empty ones", __pickleIndex: "15", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002453 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@.example\"", whyItIsNotValid: "domain has one non-empty part, after an empty one", __pickleIndex: "11", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0008178 |
| F001 | Registration refuses an email that is not valid(email: "\"ana.example.com\"", whyItIsNotValid: "no \"@\"", __pickleIndex: "7", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0025029 |
| F001 | A registered user cannot change another account's password | @F001 @AC-037 @FR-010 @FR-013 @US6-AS6 | passed | 00:00:00.2464709 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@\"", whyItIsNotValid: "no domain", __pickleIndex: "8", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002818 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@.example.com\"", whyItIsNotValid: "empty domain part before two non-empty ones", __pickleIndex: "14", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0001910 |
| F001 | Failed sign-ins for an email that has no account block nothing | @F001 @AC-025 @FR-007 @FR-008 | passed | 00:00:00.0950922 |
| F001 | A successful sign-in resets the count of failed sign-ins | @F001 @AC-019 @FR-008 @US2-AS8 | passed | 00:00:00.4514060 |
| F001 | Demo loading with a password that breaks the password rule creates no account(demoPassword: "\"exampl1\"", whyItBreaksTheRule: "7 characters, with a digit", __pickleIndex: "88", exampleTags: []) | @F001 @AC-056 @FR-002 @FR-016 @US7-AS4 | passed | 00:00:00.2925532 |
| F001 | Five failed sign-ins in a row block however far apart they are | @F001 @AC-021 @FR-008 | passed | 00:00:00.2368751 |
| F001 | A signed-in user changes their password | @F001 @AC-031 @FR-013 @US6-AS1 | passed | 00:00:00.2736452 |
| F001 | A password change to a password that breaks the password rule is refused(email: "\"weak.new7@example.com\"", newPassword: "\"runnin4\"", whyItBreaksTheRule: "7 characters, with a digit", __pickleIndex: "53", exampleTags: []) | @F001 @AC-034 @FR-002 @FR-013 @US6-AS3 | passed | 00:00:00.1691349 |
| F001 | A password change accepts a new password at the lower bound of the password rule | @F001 @AC-032 @FR-002 @FR-013 | passed | 00:00:00.2728677 |
| F001 | Demo loading run again changes nothing | @F001 @AC-059 @FR-016 @US7-AS2 | passed | 00:00:00.6356720 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@.\"", whyItIsNotValid: "domain has no non-empty part", __pickleIndex: "10", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002238 |
| F001 | Administrator creation refuses an email that is not valid | @F001 @AC-045 @FR-003 @FR-014 @US4-AS6 | passed | 00:00:00.0008468 |
| F001 | A block applies only to its own account | @F001 @AC-024 @FR-008 | passed | 00:00:00.3641814 |
| F001 | Administrator creation leaves an existing administrator unchanged | @F001 @AC-041 @FR-014 @US4-AS2 | passed | 00:00:00.1372398 |
| F001 | An attempt during the block does not extend it | @F001 @AC-017 @FR-008 @US2-AS6 | passed | 00:00:00.3076590 |
| F001 | Administrator creation refuses a password that breaks the password rule(email: "\"short.admin@example.com\"", password: "\"curat42\"", whyItBreaksTheRule: "7 characters, with a digit", __pickleIndex: "73", exampleTags: []) | @F001 @AC-046 @FR-002 @FR-014 @US4-AS7 | passed | 00:00:00.0929013 |
| F001 | A new email creates an additional administrator | @F001 @AC-053 @FR-014 @US4-AS9 | passed | 00:00:00.1654860 |
| F001 | Administrator creation checks for a missing password before it looks for a registered user | @F001 @AC-052 @FR-014 | passed | 00:00:00.1141743 |
| F001 | Demo loading without a demo password creates no account(demoPassword: "\"\"", __pickleIndex: "85", exampleTags: []) | @F001 @AC-055 @FR-016 @US7-AS3 | passed | 00:00:00.2865129 |
| F001 | An administrator reads their own account details | @F001 @AC-028 @FR-009 @FR-012 @FR-015 @US3-AS3 | passed | 00:00:00.0866775 |
| F001 | A registered user reads their own account details | @F001 @AC-027 @FR-009 @FR-012 @US3-AS2 | passed | 00:00:00.0806126 |
| F001 | A password change to a password that breaks the password rule is refused(email: "\"weak.new@example.com\"", newPassword: "\"runningfar\"", whyItBreaksTheRule: "10 characters, no digit", __pickleIndex: "52", exampleTags: []) | @F001 @AC-034 @FR-002 @FR-013 @US6-AS3 | passed | 00:00:00.1650949 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"fourth.admin@example.com\"", password: "absent", __pickleIndex: "64", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0001435 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"spaces.admin@example.com\"", password: "\"   \"", __pickleIndex: "68", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0001376 |
| F001 | Sign-in with a wrong password fails with the single message | @F001 @AC-013 @FR-007 @US2-AS3 | passed | 00:00:00.0747694 |
| F001 | A blocked account still fails with the correct password 4 minutes 59 seconds after the fifth failure | @F001 @AC-015 @FR-007 @FR-008 @US2-AS4 | passed | 00:00:00.2346559 |
| F001 | Registration refuses a password that breaks the password rule(email: "\"seven.pw@example.com\"", password: "\"walkin4\"", whyItBreaksTheRule: "7 characters, with a digit", __pickleIndex: "2", exampleTags: []) | @F001 @AC-002 @FR-002 @US1-AS3 @US1-AS4 | passed | 00:00:00.0487252 |
| F001 | Registration accepts the shortest valid email and a domain of three parts(email: "\"ana@sub.example.com\"", __pickleIndex: "22", exampleTags: []) | @F001 @AC-005 @FR-001 @FR-003 | passed | 00:00:00.0855592 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@@example.com\"", whyItIsNotValid: "two \"@\"", __pickleIndex: "17", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002457 |
| F001 | Sign-in ignores spaces around the email | @F001 @AC-011 @FR-006 | passed | 00:00:00.0983979 |
| F001 | Registration accepts a password at the lower bound of the password rule(email: "\"digits.pw@example.com\"", password: "\"12345678\"", whyItMeetsTheRule: "8 characters, all digits", __pickleIndex: "6", exampleTags: []) | @F001 @AC-003 @FR-001 @FR-002 | passed | 00:00:00.0823625 |
| F001 | Registration refuses a password that breaks the password rule(email: "\"no.digit@example.com\"", password: "\"walkingfar\"", whyItBreaksTheRule: "10 characters, no digit", __pickleIndex: "3", exampleTags: []) | @F001 @AC-002 @FR-002 @US1-AS3 @US1-AS4 | passed | 00:00:00.0425095 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"sixth.admin@example.com\"", password: "\"\"", __pickleIndex: "69", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0023294 |
| F001 | The anonymous visitor cannot read account details | @F001 @AC-026 @FR-011 @US3-AS1 | passed | 00:00:00.0007986 |
| F001 | An administrator cannot change another account's password | @F001 @AC-038 @FR-010 @FR-013 @FR-015 @US6-AS7 | passed | 00:00:00.2712631 |
| F001 | A password of only spaces creates no account | @F001 @AC-044 @FR-014 @US4-AS5 | passed | 00:00:00.0945130 |
| F001 | The anonymous visitor cannot change a password, whatever the new password(newPassword: "\"running42\"", whyItMatters: "meets the password rule", __pickleIndex: "55", exampleTags: []) | @F001 @AC-036 @FR-011 @FR-013 @US6-AS5 | passed | 00:00:00.0003534 |
| F001 | Registration accepts a password at the lower bound of the password rule(email: "\"eight.pw@example.com\"", password: "\"walking4\"", whyItMeetsTheRule: "8 characters, with a digit", __pickleIndex: "5", exampleTags: []) | @F001 @AC-003 @FR-001 @FR-002 | passed | 00:00:00.0816511 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@example..com\"", whyItIsNotValid: "empty domain part between two non-empty ones", __pickleIndex: "13", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002080 |
| F001 | An administrator gets only their own account details | @F001 @AC-030 @FR-010 @FR-012 @FR-015 @US3-AS5 | passed | 00:00:00.1377141 |
| F001 | Registration refuses an email that already has an account, in any case and with spaces | @F001 @AC-007 @FR-003 @US1-AS2 | passed | 00:00:00.1259085 |
| F001 | Registration refuses a password that breaks the password rule(email: "\"eight.nodigit@example.com\"", password: "\"walkingx\"", whyItBreaksTheRule: "8 characters, no digit", __pickleIndex: "4", exampleTags: []) | @F001 @AC-002 @FR-002 @US1-AS3 @US1-AS4 | passed | 00:00:00.0446438 |
| F001 | Administrator creation checks for a missing password before it looks for an existing administrator | @F001 @AC-051 @FR-014 | passed | 00:00:00.1130800 |
| F001 | Demo loading creates the three demo accounts | @F001 @AC-057 @FR-016 @US7-AS1 | passed | 00:00:00.4454131 |
| F001 | Registration refuses an email that is not valid(email: "\"ana @example.com\"", whyItIsNotValid: "a space inside", __pickleIndex: "16", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002001 |
| F001 | Administrator creation checks the email before the password rule | @F001 @AC-049 @FR-003 @FR-014 | passed | 00:00:00.0008823 |
| F001 | Sign-in matches the email whatever its case | @F001 @AC-010 @FR-005 @FR-006 @US2-AS1 | passed | 00:00:00.0942975 |
| F001 | Administrator creation finds a registered user before it checks the password rule | @F001 @AC-050 @FR-014 | passed | 00:00:00.0995632 |
| F001 | A password change checks the current password before the password rule | @F001 @AC-035 @FR-013 @US6-AS4 | passed | 00:00:00.1369684 |
| F001 | A password change with a wrong current password is refused | @F001 @AC-033 @FR-013 @US6-AS2 | passed | 00:00:00.2095437 |
| F001 | Administrator creation refuses a password that breaks the password rule(email: "\"seventh.admin@example.com\"", password: "\"curatorship\"", whyItBreaksTheRule: "11 characters, no digit", __pickleIndex: "72", exampleTags: []) | @F001 @AC-046 @FR-002 @FR-014 @US4-AS7 | passed | 00:00:00.1047342 |
| F001 | An account registered with spaces around the email has the trimmed email | @F001 @AC-061 @FR-003 @FR-012 | passed | 00:00:00.0820909 |
| F001 | Administrator creation creates an administrator | @F001 @AC-039 @FR-014 @US4-AS1 | passed | 00:00:00.0889651 |
| F001 | Registration refuses a password that breaks the password rule(email: "\"short.pw@example.com\"", password: "\"walk4\"", whyItBreaksTheRule: "5 characters", __pickleIndex: "1", exampleTags: []) | @F001 @AC-002 @FR-002 @US1-AS3 @US1-AS4 | passed | 00:00:00.0465683 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@example\"", whyItIsNotValid: "domain has one part", __pickleIndex: "9", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002001 |
| F001 | Five failed attempts during the block do not start a new block | @F001 @AC-023 @FR-008 | passed | 00:00:00.3255900 |
| F001 | The block ends exactly 5 minutes after the fifth failure | @F001 @AC-016 @FR-008 @US2-AS5 | passed | 00:00:00.2986124 |
| F001 | Demo loading with a password that breaks the password rule creates no account(demoPassword: "\"examplepass\"", whyItBreaksTheRule: "11 characters, no digit", __pickleIndex: "87", exampleTags: []) | @F001 @AC-056 @FR-002 @FR-016 @US7-AS4 | passed | 00:00:00.2995935 |
| F001 | Sign-in with an email that has no account fails with the single message | @F001 @AC-012 @FR-007 @US2-AS2 | passed | 00:00:00.0029354 |
| F001 | Registration refuses an email that is not valid(email: "\"\"", whyItIsNotValid: "empty", __pickleIndex: "19", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002607 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"   \"", password: "\"curator2026\"", __pickleIndex: "67", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0003936 |
| F001 | Administrator creation does not turn a registered user into an administrator | @F001 @AC-042 @FR-014 @US4-AS3 | passed | 00:00:00.0853339 |
| F001 | Administrator creation finds an existing administrator before it checks the password rule | @F001 @AC-047 @FR-014 @US4-AS8 | passed | 00:00:00.0805260 |
| F001 | Registration refuses an email that is not valid(email: "\"@example.com\"", whyItIsNotValid: "nothing before the \"@\"", __pickleIndex: "18", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0002030 |
| F001 | The end of a block resets the count of failed sign-ins | @F001 @AC-018 @FR-008 @US2-AS7 | passed | 00:00:00.4694236 |
| F001 | Administrator creation without an email or a password configures nothing(email: "absent", password: "\"curator2026\"", __pickleIndex: "65", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0001646 |
| F001 | A registered user gets only their own account details | @F001 @AC-029 @FR-010 @FR-012 @US3-AS4 | passed | 00:00:00.1328006 |
| F001 | A failed attempt during the block does not count after the block | @F001 @AC-022 @FR-008 | passed | 00:00:00.4803983 |
| F001 | Demo loading accepts a demo password at the lower bound of the password rule | @F001 @AC-058 @FR-002 @FR-016 | passed | 00:00:00.3464970 |
| F001 | Registration accepts the shortest valid email and a domain of three parts(email: "\"a@b.c\"", __pickleIndex: "21", exampleTags: []) | @F001 @AC-005 @FR-001 @FR-003 | passed | 00:00:00.0771845 |
| F001 | Four failed sign-ins in a row do not block | @F001 @AC-020 @FR-008 | passed | 00:00:00.2621642 |
| F001 | Administrator creation without an email or a password configures nothing(email: "absent", password: "absent", __pickleIndex: "63", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0002153 |
| F001 | Demo loading without a demo password creates no account(demoPassword: "\"   \"", __pickleIndex: "86", exampleTags: []) | @F001 @AC-055 @FR-016 @US7-AS3 | passed | 00:00:00.2895646 |
| F001 | Registration refuses an email that is not valid(email: "\"   \"", whyItIsNotValid: "only spaces, empty after trimming", __pickleIndex: "20", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0035325 |
| F001 | Administrator creation keeps the email in its trimmed, lower-case form | @F001 @AC-062 @FR-003 @FR-012 @FR-014 | passed | 00:00:00.0917658 |
| F001 | Registration checks the email before the password rule | @F001 @AC-009 @FR-003 | passed | 00:00:00.0009978 |
| F001 | Administrator creation matches the email whatever its case and spaces | @F001 @AC-054 @FR-003 @FR-014 | passed | 00:00:00.0942727 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"\"", password: "\"curator2026\"", __pickleIndex: "66", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0001493 |
