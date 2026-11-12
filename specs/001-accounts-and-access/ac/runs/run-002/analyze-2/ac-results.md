# AC results

## Current feature (F001)

Total: 93 · passed: 93 · failed: 0 · pending: 0 · skipped: 0

| Feature | Scenario | Tags | Outcome | Duration |
|---|---|---|---|---|
| F001 | Five failed sign-ins in a row block however far apart they are | @F001 @AC-021 @FR-008 | passed | 00:00:00.2623063 |
| F001 | Administrator creation checks the email before the password rule | @F001 @AC-049 @FR-003 @FR-014 | passed | 00:00:00.0042052 |
| F001 | Administrator creation refuses an email that is not valid | @F001 @AC-045 @FR-003 @FR-014 @US4-AS6 | passed | 00:00:00.0018993 |
| F001 | Registration keeps the email in its lower-case form | @F001 @AC-006 @FR-003 @FR-006 @FR-012 | passed | 00:00:00.0881092 |
| F001 | Registration checks the password rule before whether the email is taken | @F001 @AC-008 @FR-003 @US1-AS6 | passed | 00:00:00.0410285 |
| F001 | Registration refuses a password that breaks the password rule(email: "\"eight.nodigit@example.com\"", password: "\"walkingx\"", whyItBreaksTheRule: "8 characters, no digit", __pickleIndex: "4", exampleTags: []) | @F001 @AC-002 @FR-002 @US1-AS3 @US1-AS4 | passed | 00:00:00.0439434 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@@example.com\"", whyItIsNotValid: "two \"@\"", __pickleIndex: "17", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0005434 |
| F001 | Administrator creation checks for a missing password before it looks for a registered user | @F001 @AC-052 @FR-014 | passed | 00:00:00.1319449 |
| F001 | A password change to a password that breaks the password rule is refused(email: "\"weak.new@example.com\"", newPassword: "\"runningfar\"", whyItBreaksTheRule: "10 characters, no digit", __pickleIndex: "52", exampleTags: []) | @F001 @AC-034 @FR-002 @FR-013 @US6-AS3 | passed | 00:00:00.1546974 |
| F001 | A blocked account still fails with the correct password 4 minutes 59 seconds after the fifth failure | @F001 @AC-015 @FR-007 @FR-008 @US2-AS4 | passed | 00:00:00.2668753 |
| F001 | A new email creates an additional administrator | @F001 @AC-053 @FR-014 @US4-AS9 | passed | 00:00:00.1698276 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"   \"", password: "\"curator2026\"", __pickleIndex: "67", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0009507 |
| F001 | Administrator creation checks for a missing password before it looks for an existing administrator | @F001 @AC-051 @FR-014 | passed | 00:00:00.4480745 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@example..com\"", whyItIsNotValid: "empty domain part between two non-empty ones", __pickleIndex: "13", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0006117 |
| F001 | Administrator creation leaves an existing administrator unchanged | @F001 @AC-041 @FR-014 @US4-AS2 | passed | 00:00:00.1326598 |
| F001 | Five failed attempts during the block do not start a new block | @F001 @AC-023 @FR-008 | passed | 00:00:00.3360717 |
| F001 | Sign-in ignores spaces around the email | @F001 @AC-011 @FR-006 | passed | 00:00:00.1004178 |
| F001 | The anonymous visitor cannot change a password, whatever the new password(newPassword: "\"running42\"", whyItMatters: "meets the password rule", __pickleIndex: "55", exampleTags: []) | @F001 @AC-036 @FR-011 @FR-013 @US6-AS5 | passed | 00:00:00.0007328 |
| F001 | A block applies only to its own account | @F001 @AC-024 @FR-008 | passed | 00:00:00.3342864 |
| F001 | A successful sign-in resets the count of failed sign-ins | @F001 @AC-019 @FR-008 @US2-AS8 | passed | 00:00:00.4406217 |
| F001 | Registration refuses a password that breaks the password rule(email: "\"seven.pw@example.com\"", password: "\"walkin4\"", whyItBreaksTheRule: "7 characters, with a digit", __pickleIndex: "2", exampleTags: []) | @F001 @AC-002 @FR-002 @US1-AS3 @US1-AS4 | passed | 00:00:00.0434343 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"\"", password: "\"curator2026\"", __pickleIndex: "66", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0004972 |
| F001 | Registration refuses an email that is not valid(email: "\"@example.com\"", whyItIsNotValid: "nothing before the \"@\"", __pickleIndex: "18", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0005419 |
| F001 | Administrator creation finds a registered user before it checks the password rule | @F001 @AC-050 @FR-014 | passed | 00:00:00.0999221 |
| F001 | A password of only spaces creates no account | @F001 @AC-044 @FR-014 @US4-AS5 | passed | 00:00:00.0853479 |
| F001 | Sign-in matches the email whatever its case | @F001 @AC-010 @FR-005 @FR-006 @US2-AS1 | passed | 00:00:00.1024535 |
| F001 | Administrator creation refuses a password that breaks the password rule(email: "\"seventh.admin@example.com\"", password: "\"curatorship\"", whyItBreaksTheRule: "11 characters, no digit", __pickleIndex: "72", exampleTags: []) | @F001 @AC-046 @FR-002 @FR-014 @US4-AS7 | passed | 00:00:00.1034247 |
| F001 | Administrator creation checks for a missing password before the email | @F001 @AC-048 @FR-014 | passed | 00:00:00.0016090 |
| F001 | Registration refuses an email that already has an account, in any case and with spaces | @F001 @AC-007 @FR-003 @US1-AS2 | passed | 00:00:00.1311093 |
| F001 | Registration accepts a password at the lower bound of the password rule(email: "\"digits.pw@example.com\"", password: "\"12345678\"", whyItMeetsTheRule: "8 characters, all digits", __pickleIndex: "6", exampleTags: []) | @F001 @AC-003 @FR-001 @FR-002 | passed | 00:00:00.0750987 |
| F001 | The block ends exactly 5 minutes after the fifth failure | @F001 @AC-016 @FR-008 @US2-AS5 | passed | 00:00:00.2947632 |
| F001 | Registration checks the email before the password rule | @F001 @AC-009 @FR-003 | passed | 00:00:00.0012093 |
| F001 | An unknown email, a wrong password and a blocked account give the same result | @F001 @AC-014 @FR-007 @FR-008 | passed | 00:00:00.3376165 |
| F001 | An administrator reads their own account details | @F001 @AC-028 @FR-009 @FR-012 @FR-015 @US3-AS3 | passed | 00:00:00.0890640 |
| F001 | Administrator creation accepts a password at the lower bound of the password rule | @F001 @AC-040 @FR-002 @FR-014 | passed | 00:00:00.0901988 |
| F001 | Failed sign-ins for an email that has no account block nothing | @F001 @AC-025 @FR-007 @FR-008 | passed | 00:00:00.0818620 |
| F001 | A registered user gets only their own account details | @F001 @AC-029 @FR-010 @FR-012 @US3-AS4 | passed | 00:00:00.1363416 |
| F001 | Demo loading without a demo password creates no account(demoPassword: "\"   \"", __pickleIndex: "86", exampleTags: []) | @F001 @AC-055 @FR-016 @US7-AS3 | passed | 00:00:00.2896884 |
| F001 | A password change checks the current password before the password rule | @F001 @AC-035 @FR-013 @US6-AS4 | passed | 00:00:00.1382473 |
| F001 | Registration accepts the shortest valid email and a domain of three parts(email: "\"a@b.c\"", __pickleIndex: "21", exampleTags: []) | @F001 @AC-005 @FR-001 @FR-003 | passed | 00:00:00.0766882 |
| F001 | Administrator creation finds a registered user whatever the case and spaces of the email | @F001 @AC-063 @FR-003 @FR-014 | passed | 00:00:00.1044733 |
| F001 | Registration refuses an email that is not valid(email: "\"\"", whyItIsNotValid: "empty", __pickleIndex: "19", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0005486 |
| F001 | A password change to a password that breaks the password rule is refused(email: "\"weak.new7@example.com\"", newPassword: "\"runnin4\"", whyItBreaksTheRule: "7 characters, with a digit", __pickleIndex: "53", exampleTags: []) | @F001 @AC-034 @FR-002 @FR-013 @US6-AS3 | passed | 00:00:00.1487207 |
| F001 | Administrator creation does not turn a registered user into an administrator | @F001 @AC-042 @FR-014 @US4-AS3 | passed | 00:00:00.0833905 |
| F001 | Registration accepts the shortest valid email and a domain of three parts(email: "\"ana@sub.example.com\"", __pickleIndex: "22", exampleTags: []) | @F001 @AC-005 @FR-001 @FR-003 | passed | 00:00:00.0938824 |
| F001 | An administrator cannot change another account's password | @F001 @AC-038 @FR-010 @FR-013 @FR-015 @US6-AS7 | passed | 00:00:00.2557953 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@example\"", whyItIsNotValid: "domain has one part", __pickleIndex: "9", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0005575 |
| F001 | Registration accepts a password at the lower bound of the password rule(email: "\"eight.pw@example.com\"", password: "\"walking4\"", whyItMeetsTheRule: "8 characters, with a digit", __pickleIndex: "5", exampleTags: []) | @F001 @AC-003 @FR-001 @FR-002 | passed | 00:00:00.0835267 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"fourth.admin@example.com\"", password: "absent", __pickleIndex: "64", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0005814 |
| F001 | Sign-in with a wrong password fails with the single message | @F001 @AC-013 @FR-007 @US2-AS3 | passed | 00:00:00.0813581 |
| F001 | An attempt during the block does not extend it | @F001 @AC-017 @FR-008 @US2-AS6 | passed | 00:00:00.3282349 |
| F001 | Four failed sign-ins in a row do not block | @F001 @AC-020 @FR-008 | passed | 00:00:00.2383584 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@.\"", whyItIsNotValid: "domain has no non-empty part", __pickleIndex: "10", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0005718 |
| F001 | Demo loading run again changes nothing | @F001 @AC-059 @FR-016 @US7-AS2 | passed | 00:00:00.6494205 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@.example.com\"", whyItIsNotValid: "empty domain part before two non-empty ones", __pickleIndex: "14", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0009817 |
| F001 | Demo loading creates the three demo accounts | @F001 @AC-057 @FR-016 @US7-AS1 | passed | 00:00:00.4067198 |
| F001 | Administrator creation creates an administrator | @F001 @AC-039 @FR-014 @US4-AS1 | passed | 00:00:00.0866808 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@example.com.\"", whyItIsNotValid: "empty domain part after two non-empty ones", __pickleIndex: "15", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0005303 |
| F001 | A password change with a wrong current password is refused | @F001 @AC-033 @FR-013 @US6-AS2 | passed | 00:00:00.1937381 |
| F001 | Administrator creation without an email or a password configures nothing(email: "absent", password: "\"curator2026\"", __pickleIndex: "65", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0005418 |
| F001 | Registration refuses an email that is not valid(email: "\"ana @example.com\"", whyItIsNotValid: "a space inside", __pickleIndex: "16", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0006495 |
| F001 | Administrator creation refuses a password that breaks the password rule(email: "\"short.admin@example.com\"", password: "\"curat42\"", whyItBreaksTheRule: "7 characters, with a digit", __pickleIndex: "73", exampleTags: []) | @F001 @AC-046 @FR-002 @FR-014 @US4-AS7 | passed | 00:00:00.0962844 |
| F001 | Demo loading with a password that breaks the password rule creates no account(demoPassword: "\"exampl1\"", whyItBreaksTheRule: "7 characters, with a digit", __pickleIndex: "88", exampleTags: []) | @F001 @AC-056 @FR-002 @FR-016 @US7-AS4 | passed | 00:00:00.2818458 |
| F001 | Demo loading without a demo password creates no account(demoPassword: "\"\"", __pickleIndex: "85", exampleTags: []) | @F001 @AC-055 @FR-016 @US7-AS3 | passed | 00:00:00.2697722 |
| F001 | The anonymous visitor cannot change a password, whatever the new password(newPassword: "\"run\"", whyItMatters: "breaks the password rule, which is checked only fo"···, __pickleIndex: "56", exampleTags: []) | @F001 @AC-036 @FR-011 @FR-013 @US6-AS5 | passed | 00:00:00.0020817 |
| F001 | Demo loading with a password that breaks the password rule creates no account(demoPassword: "\"examplepass\"", whyItBreaksTheRule: "11 characters, no digit", __pickleIndex: "87", exampleTags: []) | @F001 @AC-056 @FR-002 @FR-016 @US7-AS4 | passed | 00:00:00.3077364 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"sixth.admin@example.com\"", password: "\"\"", __pickleIndex: "69", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0030060 |
| F001 | Administrator creation without an email or a password configures nothing(email: "absent", password: "absent", __pickleIndex: "63", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0005139 |
| F001 | A password change accepts a new password at the lower bound of the password rule | @F001 @AC-032 @FR-002 @FR-013 | passed | 00:00:00.2355170 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@\"", whyItIsNotValid: "no domain", __pickleIndex: "8", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0005434 |
| F001 | A failed attempt during the block does not count after the block | @F001 @AC-022 @FR-008 | passed | 00:00:00.4876398 |
| F001 | Registration refuses a password that breaks the password rule(email: "\"short.pw@example.com\"", password: "\"walk4\"", whyItBreaksTheRule: "5 characters", __pickleIndex: "1", exampleTags: []) | @F001 @AC-002 @FR-002 @US1-AS3 @US1-AS4 | passed | 00:00:00.0515351 |
| F001 | Registration refuses an email that is not valid(email: "\"ana.example.com\"", whyItIsNotValid: "no \"@\"", __pickleIndex: "7", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0005303 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@.example\"", whyItIsNotValid: "domain has one non-empty part, after an empty one", __pickleIndex: "11", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0009707 |
| F001 | Administrator creation keeps the email in its trimmed, lower-case form | @F001 @AC-062 @FR-003 @FR-012 @FR-014 | passed | 00:00:00.0901574 |
| F001 | A signed-in user changes their password | @F001 @AC-031 @FR-013 @US6-AS1 | passed | 00:00:00.2927573 |
| F001 | Administrator creation finds an existing administrator before it checks the password rule | @F001 @AC-047 @FR-014 @US4-AS8 | passed | 00:00:00.0815508 |
| F001 | Registration refuses an email that is not valid(email: "\"ana@example.\"", whyItIsNotValid: "domain has one non-empty part, before an empty one", __pickleIndex: "12", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0005359 |
| F001 | Demo loading without a demo password creates no account(demoPassword: "absent", __pickleIndex: "84", exampleTags: []) | @F001 @AC-055 @FR-016 @US7-AS3 | passed | 00:00:00.2709529 |
| F001 | Demo loading accepts a demo password at the lower bound of the password rule | @F001 @AC-058 @FR-002 @FR-016 | passed | 00:00:00.3376234 |
| F001 | A registered user reads their own account details | @F001 @AC-027 @FR-009 @FR-012 @US3-AS2 | passed | 00:00:00.0949799 |
| F001 | Administrator creation without an email or a password configures nothing(email: "\"spaces.admin@example.com\"", password: "\"   \"", __pickleIndex: "68", exampleTags: []) | @F001 @AC-043 @FR-014 @US4-AS4 @US4-AS5 | passed | 00:00:00.0005260 |
| F001 | An administrator gets only their own account details | @F001 @AC-030 @FR-010 @FR-012 @FR-015 @US3-AS5 | passed | 00:00:00.1449858 |
| F001 | Administrator creation matches the email whatever its case and spaces | @F001 @AC-054 @FR-003 @FR-014 | passed | 00:00:00.0956103 |
| F001 | The anonymous visitor cannot read account details | @F001 @AC-026 @FR-011 @US3-AS1 | passed | 00:00:00.0009537 |
| F001 | An account registered with spaces around the email has the trimmed email | @F001 @AC-061 @FR-003 @FR-012 | passed | 00:00:00.0978421 |
| F001 | Registration refuses a password that breaks the password rule(email: "\"no.digit@example.com\"", password: "\"walkingfar\"", whyItBreaksTheRule: "10 characters, no digit", __pickleIndex: "3", exampleTags: []) | @F001 @AC-002 @FR-002 @US1-AS3 @US1-AS4 | passed | 00:00:00.0406716 |
| F001 | The end of a block resets the count of failed sign-ins | @F001 @AC-018 @FR-008 @US2-AS7 | passed | 00:00:00.4682082 |
| F001 | Demo loading decides per demo email, and a demo email registered earlier keeps its owner's password | @F001 @AC-060 @FR-003 @FR-016 | passed | 00:00:00.4805013 |
| F001 | Registration refuses an email that is not valid(email: "\"   \"", whyItIsNotValid: "only spaces, empty after trimming", __pickleIndex: "20", exampleTags: []) | @F001 @AC-004 @FR-003 @US1-AS5 | passed | 00:00:00.0036007 |
| F001 | Sign-in with an email that has no account fails with the single message | @F001 @AC-012 @FR-007 @US2-AS2 | passed | 00:00:00.0052448 |
| F001 | Registration creates a registered user | @F001 @AC-001 @FR-001 @FR-004 @FR-009 @US1-AS1 | passed | 00:00:00.0837442 |
| F001 | A registered user cannot change another account's password | @F001 @AC-037 @FR-010 @FR-013 @US6-AS6 | passed | 00:00:00.2540896 |

## Regression (other features)

Total: 0 · passed: 0 · failed: 0 · pending: 0 · skipped: 0

| Feature | Scenario | Tags | Outcome | Duration |
|---|---|---|---|---|
