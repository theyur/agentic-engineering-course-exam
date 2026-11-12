# Translation report — F001 / translate-002

## Build

success, 1 attempt, no remaining errors (0 warnings, 0 errors).

## Items

| AC id | Status (translated / issue) | Scenario name |
|---|---|---|
| AC-001 | translated | Registration creates a registered user |
| AC-002 | translated | Registration refuses a password that breaks the password rule |
| AC-003 | translated | Registration accepts a password at the lower bound of the password rule |
| AC-004 | translated | Registration refuses an email that is not valid |
| AC-005 | translated | Registration accepts the shortest valid email and a domain of three parts |
| AC-006 | translated | Registration keeps the email in its lower-case form |
| AC-061 | translated | An account registered with spaces around the email has the trimmed email |
| AC-007 | translated | Registration refuses an email that already has an account, in any case and with spaces |
| AC-008 | translated | Registration checks the password rule before whether the email is taken |
| AC-009 | translated | Registration checks the email before the password rule |
| AC-010 | translated | Sign-in matches the email whatever its case |
| AC-011 | translated | Sign-in ignores spaces around the email |
| AC-012 | translated | Sign-in with an email that has no account fails with the single message |
| AC-013 | translated | Sign-in with a wrong password fails with the single message |
| AC-014 | translated | An unknown email, a wrong password and a blocked account give the same result |
| AC-015 | translated | A blocked account still fails with the correct password 4 minutes 59 seconds after the fifth failure |
| AC-016 | translated | The block ends exactly 5 minutes after the fifth failure |
| AC-017 | translated | An attempt during the block does not extend it |
| AC-018 | translated | The end of a block resets the count of failed sign-ins |
| AC-019 | translated | A successful sign-in resets the count of failed sign-ins |
| AC-020 | translated | Four failed sign-ins in a row do not block |
| AC-021 | translated | Five failed sign-ins in a row block however far apart they are |
| AC-022 | translated | A failed attempt during the block does not count after the block |
| AC-023 | translated | Five failed attempts during the block do not start a new block |
| AC-024 | translated | A block applies only to its own account |
| AC-025 | translated | Failed sign-ins for an email that has no account block nothing |
| AC-026 | translated | The anonymous visitor cannot read account details |
| AC-027 | translated | A registered user reads their own account details |
| AC-028 | translated | An administrator reads their own account details |
| AC-029 | translated | A registered user gets only their own account details |
| AC-030 | translated | An administrator gets only their own account details |
| AC-031 | translated | A signed-in user changes their password |
| AC-032 | translated | A password change accepts a new password at the lower bound of the password rule |
| AC-033 | translated | A password change with a wrong current password is refused |
| AC-034 | translated | A password change to a password that breaks the password rule is refused |
| AC-035 | translated | A password change checks the current password before the password rule |
| AC-036 | translated | The anonymous visitor cannot change a password, whatever the new password |
| AC-037 | translated | A registered user cannot change another account's password |
| AC-038 | translated | An administrator cannot change another account's password |
| AC-039 | translated | Administrator creation creates an administrator |
| AC-040 | translated | Administrator creation accepts a password at the lower bound of the password rule |
| AC-041 | translated | Administrator creation leaves an existing administrator unchanged |
| AC-042 | translated | Administrator creation does not turn a registered user into an administrator |
| AC-043 | translated | Administrator creation without an email or a password configures nothing |
| AC-044 | translated | A password of only spaces creates no account |
| AC-045 | translated | Administrator creation refuses an email that is not valid |
| AC-046 | translated | Administrator creation refuses a password that breaks the password rule |
| AC-047 | translated | Administrator creation finds an existing administrator before it checks the password rule |
| AC-048 | translated | Administrator creation checks for a missing password before the email |
| AC-049 | translated | Administrator creation checks the email before the password rule |
| AC-050 | translated | Administrator creation finds a registered user before it checks the password rule |
| AC-051 | translated | Administrator creation checks for a missing password before it looks for an existing administrator |
| AC-052 | translated | Administrator creation checks for a missing password before it looks for a registered user |
| AC-053 | translated | A new email creates an additional administrator |
| AC-054 | translated | Administrator creation matches the email whatever its case and spaces |
| AC-062 | translated | Administrator creation keeps the email in its trimmed, lower-case form |
| AC-063 | translated | Administrator creation finds a registered user whatever the case and spaces of the email |
| AC-055 | translated | Demo loading without a demo password creates no account |
| AC-056 | translated | Demo loading with a password that breaks the password rule creates no account |
| AC-057 | translated | Demo loading creates the three demo accounts |
| AC-058 | translated | Demo loading accepts a demo password at the lower bound of the password rule |
| AC-059 | translated | Demo loading run again changes nothing |
| AC-060 | translated | Demo loading decides per demo email, and a demo email registered earlier keeps its owner's password |

## Issues

No issues.

## Interpretations

- All items: the item heading `### AC-NNN (REF; REF) Title` became tags `@AC-NNN @REF @REF` plus `Scenario:` / `Scenario Outline:` with the title verbatim as the scenario name; the `Feature:` line is tagged `@F001` and titled "Accounts and access" (spec title). Markdown list bullets (`- `) were removed and the leading keyword kept; scenarios are in the order of the acceptance-criteria document (AC-061 after AC-006, AC-062 and AC-063 after AC-054).
- All items: markdown code quoting (backticks) around values in step text was replaced by Gherkin double quotes, with the value inside unchanged, including leading and trailing spaces (for example `" Taken@Example.com "`, `"  trim.me@example.com  "`). Quoted messages already in double quotes were kept as is. The AC conventions' `"   "` (three spaces) and `""` (empty text) are already double-quoted and stay identical.
- AC-002, AC-003, AC-004, AC-005, AC-034, AC-036, AC-043, AC-046, AC-055, AC-056: `Examples:` tables were re-formatted as Gherkin tables (markdown separator row `|---|` removed, columns aligned). Placeholders in steps (`<email>`, `<password>`, `<new password>`, `<demo password>`) are written unquoted; each value cell carries its value in double quotes (for example `"walk4"`, `""`, `"   "`), so that after substitution every value is quoted exactly as in the non-outline steps. The bare keyword `absent` (AC-043, AC-055) stays unquoted, as the items define it ("absent means that no value is supplied"). The explanatory columns ("why it breaks the rule", "why it meets the rule", "why it is not valid", "why it matters") were kept verbatim as extra columns; they are not used by any step.
- AC-032, AC-040, AC-058: the parenthetical remark "(8 characters, with a digit)" was kept verbatim inside the step text.
- AC-014: the single When line naming three sign-ins was kept as one When step, verbatim.
- AC-043, AC-055: the clause ", where absent means that no value is supplied" was kept verbatim in the Given / When step. AC-048, AC-051, AC-052: "the password absent, that is, with no password supplied" kept verbatim.
- AC-001, AC-006, AC-061, AC-012, AC-039, AC-040, AC-045, AC-046, AC-048, AC-049, AC-062: the item has no Given line; none was added.
- Golden samples: no item names a golden sample; the folder tests/HT.AcceptanceTests/Samples/F001/ does not exist, which affects no item.
