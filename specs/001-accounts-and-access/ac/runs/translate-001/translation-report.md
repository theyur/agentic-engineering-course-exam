# Translation report — F001 / translate-001

## Build

success, 2 attempts (the first ran in parallel with writing the feature file, so it was repeated once the
file existed; both succeeded), no remaining errors.

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
| AC-055 | translated | Demo loading without a demo password creates no account |
| AC-056 | translated | Demo loading with a password that breaks the password rule creates no account |
| AC-057 | translated | Demo loading creates the three demo accounts |
| AC-058 | translated | Demo loading accepts a demo password at the lower bound of the password rule |
| AC-059 | translated | Demo loading run again changes nothing |
| AC-060 | translated | Demo loading decides per demo email, and a demo email registered earlier keeps its owner's password |

## Issues

No issues.

## Interpretations

- All items: the Markdown list items `- Given` / `- When` / `- Then` / `- And` became Gherkin steps with the
  same keywords; the `### AC-NNN (refs) Title` heading became the scenario tags (`@AC-NNN` plus one tag per
  reference, in the given order) and the scenario name (the title, verbatim). Scenarios keep the order of
  `acceptance.md` (AC-061 stays between AC-006 and AC-007).
- All items: concrete values written in backticks (emails, passwords, UTC instants) are written in double
  quotes in steps and Examples cells; the text inside is unchanged, including leading, trailing and inner
  spaces (`" Taken@Example.com "`, `"  trim.me@example.com  "`, `"  spaced@example.com  "`,
  `" Case.Admin@Example.com "`, `" Demo2@Example.com "`). Messages and named results already in double quotes
  are unchanged.
- AC-004, AC-043, AC-044, AC-055: the empty text `""` and the three-space text `"   "` are written as `""` and
  `"   "` (three spaces) inside the quotes, so Gherkin's trimming of table cells keeps them; `absent` stays the
  bare word `absent` (no quotes), as the acceptance-criteria conventions define it.
- AC-002, AC-003, AC-004, AC-005, AC-034, AC-036, AC-043, AC-046, AC-055, AC-056: "Examples:" with a Markdown
  table became a `Scenario Outline` with an `Examples:` Gherkin table (Markdown separator row dropped). Headers
  are kept verbatim, including the explanatory columns ("why it breaks the rule", "why it meets the rule",
  "why it is not valid", "why it matters"), which no step uses. Placeholders `<email>`, `<password>`,
  `<new password>` and `<demo password>` are unchanged and unquoted in the steps; quotes come from the cells.
- AC-032, AC-040, AC-058: the parenthetical "(8 characters, with a digit)" is kept in the step text as written.
- AC-045, AC-046, AC-048, AC-049: the items have no Given line; the scenarios start with `When`, as written.
- The `Feature:` line is tagged `@F001` and titled "Accounts and access" (spec title). No golden sample is
  named by any item; the golden samples folder `tests/HT.AcceptanceTests/Samples/F001/` does not exist, which
  affects no item.
