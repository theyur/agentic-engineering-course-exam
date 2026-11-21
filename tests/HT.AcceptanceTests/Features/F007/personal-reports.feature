@F007
Feature: Personal reports

  @AC-001 @FR-001 @FR-002 @FR-007 @US1-AS1
  Scenario: Consistency counts Met against Met, Failed and Missed final results
    Given the registered user `f7.ac001.user@example.com`, who created the custom habit `Reading`
    And at 2026-11-01T08:00:00Z `f7.ac001.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-01
    And `f7.ac001.user@example.com` records 20 for each date from 2026-11-01 to 2026-11-20 except 2026-11-05 and 2026-11-09
    And `f7.ac001.user@example.com` records 12 for 2026-11-05
    When at 2026-11-23T10:00:00Z `f7.ac001.user@example.com` reads the report of the goal
    Then the consistency is "90% (18 of 20)" with the percentage 90, the successful number 18 and the counted number 20

  @AC-002 @FR-001 @US1-AS2
  Scenario: Pending and Due results are not counted, also when they reach the target
    Given the registered user `f7.ac002.user@example.com`, who created the custom habit `Reading`
    And at 2026-11-01T08:00:00Z `f7.ac002.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-01
    And `f7.ac002.user@example.com` records 20 for each date from 2026-11-01 to 2026-11-23 except 2026-11-05 and 2026-11-09
    And `f7.ac002.user@example.com` records 12 for 2026-11-05
    When at 2026-11-23T10:00:00Z `f7.ac002.user@example.com` reads the report of the goal
    Then the consistency is "90% (18 of 20)"
    And the recent results end with 2026-11-21 Pending and 2026-11-22 Pending, and contain no entry for 2026-11-23

  @AC-003 @FR-002 @US1-AS3
  Scenario Outline: A goal with no final result has the consistency "N/A"
    Given the registered user `f7.ac003.user.<n>@example.com`, who created the custom habit `Reading`
    And at <created> `f7.ac003.user.<n>@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date <start>
    And `f7.ac003.user.<n>@example.com` records 20 for each date of <recorded>
    When at 2026-11-23T10:00:00Z `f7.ac003.user.<n>@example.com` reads the report of the goal
    Then the consistency is "N/A" with no percentage and the counted number 0

    Examples:
      | n  | created              | start      | recorded                 | note                                             |
      | 01 | 2026-11-23T08:00:00Z | 2026-11-25 | no date                  | not started                                      |
      | 02 | 2026-11-21T08:00:00Z | 2026-11-21 | 2026-11-21 to 2026-11-23 | 2026-11-21 and 2026-11-22 Pending, 2026-11-23 Due |

  @AC-004 @FR-002 @US1-AS4
  Scenario Outline: The percentage is rounded down to a whole number
    Given the registered user `f7.ac004.user.<n>@example.com`, who created the custom habit `Reading`
    And at 2026-11-01T08:00:00Z `f7.ac004.user.<n>@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-01
    And `f7.ac004.user.<n>@example.com` records <nov1> for 2026-11-01, <nov2> for 2026-11-02 and <nov3> for 2026-11-03, where "none" records nothing
    When at 2026-11-06T10:00:00Z `f7.ac004.user.<n>@example.com` reads the report of the goal
    Then the consistency is <text> with the percentage <percentage>, the successful number <successful> and the counted number 3

    Examples:
      | n  | nov1 | nov2 | nov3 | text            | percentage | successful |
      | 01 | 20   | 20   | 12   | "66% (2 of 3)"  | 66         | 2          |
      | 02 | 20   | 12   | none | "33% (1 of 3)"  | 33         | 1          |
      | 03 | 20   | 20   | 20   | "100% (3 of 3)" | 100        | 3          |
      | 04 | 12   | none | 0    | "0% (0 of 3)"   | 0          | 0          |

  @AC-005 @FR-002 @FR-003 @US1-AS4
  Scenario: 199 Met of 200 final results is "99% (199 of 200)"
    Given the registered user `f7.ac005.user@example.com`, who created the custom habit `Reading`
    And at 2026-04-01T08:00:00Z `f7.ac005.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-04-01
    And `f7.ac005.user@example.com` records 20 for each date from 2026-04-01 to 2026-10-17 except 2026-07-01
    When at 2026-10-20T10:00:00Z `f7.ac005.user@example.com` reads the report of the goal
    Then the consistency is "99% (199 of 200)" with the percentage 99
    And the current streak is "108 commitments" and the longest streak is "108 commitments"

  @AC-006 @FR-001 @US1-AS5
  Scenario Outline: Reduce results count like Build results
    Given the registered user `f7.ac006.user.<n>@example.com`, who created the custom habit `Sugar`
    And at 2026-11-01T08:00:00Z `f7.ac006.user.<n>@example.com` created a goal on their custom habit `Sugar` with <settings> and the start date 2026-11-01
    And `f7.ac006.user.<n>@example.com` records <nov1> for 2026-11-01 and <nov2> for 2026-11-02, and nothing for 2026-11-03
    When at 2026-11-06T10:00:00Z `f7.ac006.user.<n>@example.com` reads the report of the goal
    Then the consistency is "33% (1 of 3)"
    And the recent results begin with 2026-11-01 Met, 2026-11-02 Failed and 2026-11-03 Missed

    Examples:
      | n  | settings                                     | nov1 | nov2 |
      | 01 | Reduce, count 1 `sweets`, 1 time per day     | 0    | 3    |
      | 02 | Reduce, yes/no, fixed days on all seven days | No   | Yes  |

  @AC-007 @FR-001 @US1-AS6
  Scenario Outline: Each commitment, day or week counts once, however many records it has
    Given the registered user `f7.ac007.user.<n>@example.com`, who created the custom habit `Yoga`
    And at <created> `f7.ac007.user.<n>@example.com` created a goal on their custom habit `Yoga` with <settings> and the start date <start>
    And `f7.ac007.user.<n>@example.com` records <records>
    When at <read> `f7.ac007.user.<n>@example.com` reads the report of the goal
    Then the consistency is <text>

    Examples:
      | n  | settings                                                  | created              | start      | records                                                                                                                                                    | read                 | text           |
      | 01 | Build, yes/no, 3 times per week                           | 2026-10-05T08:00:00Z | 2026-10-05 | `Yes` for 2026-10-05, 2026-10-06, 2026-10-07, 2026-10-12, 2026-10-13, 2026-10-26, 2026-10-27, 2026-10-28, 2026-10-29, 2026-11-02, 2026-11-03 and 2026-11-04 | 2026-11-11T10:00:00Z | "60% (3 of 5)" |
      | 02 | Build, yes/no, fixed days on Monday, Wednesday and Friday | 2026-11-02T08:00:00Z | 2026-11-02 | `Yes` for 2026-11-02, `No` for 2026-11-04 and `Yes` for 2026-11-09                                                                                         | 2026-11-11T10:00:00Z | "33% (1 of 3)" |
      | 03 | Build, yes/no, 2 times per day                            | 2026-11-01T08:00:00Z | 2026-11-01 | `Yes` twice for 2026-11-01, `Yes` once for 2026-11-02 and `Yes` three times for 2026-11-03                                                                 | 2026-11-06T10:00:00Z | "66% (2 of 3)" |

  @AC-008 @FR-010 @US1-AS7
  Scenario: Two goals on the same habit have their own figures and nothing combines them
    Given the registered user `f7.ac008.user@example.com`, who created the custom habit `Reading`
    And at 2026-11-01T08:00:00Z `f7.ac008.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-01
    And at 2026-11-01T08:00:00Z `f7.ac008.user@example.com` created a second goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-01
    And `f7.ac008.user@example.com` records 20 for each date from 2026-11-01 to 2026-11-03 on the first goal and nothing on the second goal
    When at 2026-11-06T10:00:00Z `f7.ac008.user@example.com` reads the report of the first goal, the report of the second goal and the dashboard
    Then the first goal's report has the consistency "100% (3 of 3)" and the current streak "3 commitments"
    And the second goal's report has the consistency "0% (0 of 3)" and the current streak "0 commitments"
    And the dashboard lists exactly two goals: the first goal with "100% (3 of 3)", then the second goal with "0% (0 of 3)"

  @AC-009 @FR-003 @FR-004 @US2-AS1
  Scenario: A Failed result resets the current streak
    Given the registered user `f7.ac009.user@example.com`, who created the custom habit `Reading`
    And at 2026-11-01T08:00:00Z `f7.ac009.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-01
    And `f7.ac009.user@example.com` records 20 for 2026-11-01, 20 for 2026-11-02, 12 for 2026-11-03, 20 for 2026-11-04, 20 for 2026-11-05 and 20 for 2026-11-06
    When at 2026-11-09T10:00:00Z `f7.ac009.user@example.com` reads the report of the goal
    Then the current streak is "3 commitments" with the number 3 and the unit "commitments"
    And the longest streak is "3 commitments"

  @AC-010 @FR-003 @US2-AS2
  Scenario: The longest streak keeps the longest run of Met results
    Given the registered user `f7.ac010.user@example.com`, who created the custom habit `Reading`
    And at 2026-11-01T08:00:00Z `f7.ac010.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-01
    And `f7.ac010.user@example.com` records 20 for each date from 2026-11-01 to 2026-11-18 except 2026-11-12
    When at 2026-11-21T10:00:00Z `f7.ac010.user@example.com` reads the report of the goal
    Then the current streak is "6 commitments" and the longest streak is "11 commitments"

  @AC-011 @FR-003 @US2-AS3
  Scenario Outline: A latest Failed or Missed result gives a current streak of 0
    Given the registered user `f7.ac011.user.<n>@example.com`, who created the custom habit `Reading`
    And at 2026-11-01T08:00:00Z `f7.ac011.user.<n>@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-01
    And `f7.ac011.user.<n>@example.com` records 20 for each date from 2026-11-01 to 2026-11-05 and <nov6> for 2026-11-06, where "none" records nothing
    When at 2026-11-09T10:00:00Z `f7.ac011.user.<n>@example.com` reads the report of the goal
    Then the current streak is "0 commitments" and the longest streak is "5 commitments"

    Examples:
      | n  | nov6 |
      | 01 | 12   |
      | 02 | none |

  @AC-012 @FR-003 @US2-AS4
  Scenario Outline: A Pending result neither extends nor resets the streak until it is Met
    Given the registered user `f7.ac012.user.<n>@example.com`, who created the custom habit `Reading`
    And at 2026-11-02T08:00:00Z `f7.ac012.user.<n>@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on Monday, Wednesday and Friday and the start date 2026-11-02
    And `f7.ac012.user.<n>@example.com` records 20 for 2026-11-02, 20 for 2026-11-04 and <nov6> for 2026-11-06, where "none" records nothing
    When at <read> `f7.ac012.user.<n>@example.com` reads the report of the goal
    Then the current streak is <current> and the longest streak is <longest>
    And the recent results end with <last>

    Examples:
      | n  | nov6 | read                 | current         | longest         | last               |
      | 01 | 20   | 2026-11-08T23:59:59Z | "2 commitments" | "2 commitments" | 2026-11-06 Pending |
      | 02 | 20   | 2026-11-09T00:00:00Z | "3 commitments" | "3 commitments" | 2026-11-06 Met     |
      | 03 | none | 2026-11-08T23:59:59Z | "2 commitments" | "2 commitments" | 2026-11-06 Pending |

  @AC-013 @FR-003 @FR-008 @US2-AS5
  Scenario: A Pending result that becomes Missed resets the streak at its grace end
    Given the registered user `f7.ac013.user@example.com`, who created the custom habit `Reading`
    And at 2026-11-02T08:00:00Z `f7.ac013.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on Monday, Wednesday and Friday and the start date 2026-11-02
    And at 2026-11-04T20:00:00Z `f7.ac013.user@example.com` records 20 for 2026-11-02 and 20 for 2026-11-04
    And at 2026-11-08T23:59:59Z `f7.ac013.user@example.com` read the report of the goal and got the current streak "2 commitments"
    When at 2026-11-09T00:00:00Z `f7.ac013.user@example.com` reads the report of the goal
    Then the current streak is "0 commitments" and the longest streak is "2 commitments"
    And the consistency is "66% (2 of 3)"
    And the recent results end with 2026-11-06 Missed, no record

  @AC-014 @FR-004 @US2-AS6
  Scenario Outline: Every streak carries the unit of its goal, singular for 1
    Given the registered user `f7.ac014.user.<n>@example.com`, who created the custom habit `Yoga`
    And at <created> `f7.ac014.user.<n>@example.com` created a goal on their custom habit `Yoga` with <settings> and the start date <start>
    And `f7.ac014.user.<n>@example.com` records `Yes` for <yes>
    When at <read> `f7.ac014.user.<n>@example.com` reads the report of the goal
    Then the current streak is <streak> with the number <number> and the unit <unit>
    And the longest streak is <streak>

    Examples:
      | n  | settings                                    | created              | start      | yes                       | read                 | streak          | number | unit          |
      | 01 | Build, yes/no, 1 time per day               | 2026-11-01T08:00:00Z | 2026-11-01 | 2026-11-03                | 2026-11-06T10:00:00Z | "1 day"         | 1      | "day"         |
      | 02 | Build, yes/no, 1 time per day               | 2026-11-01T08:00:00Z | 2026-11-01 | 2026-11-02 and 2026-11-03 | 2026-11-06T10:00:00Z | "2 days"        | 2      | "days"        |
      | 03 | Build, yes/no, 1 time per day               | 2026-11-01T08:00:00Z | 2026-11-01 | no date                   | 2026-11-06T10:00:00Z | "0 days"        | 0      | "days"        |
      | 04 | Build, yes/no, 1 time per week              | 2026-10-26T08:00:00Z | 2026-10-26 | 2026-11-02                | 2026-11-11T10:00:00Z | "1 week"        | 1      | "week"        |
      | 05 | Build, yes/no, 1 time per week              | 2026-10-26T08:00:00Z | 2026-10-26 | 2026-10-26 and 2026-11-02 | 2026-11-11T10:00:00Z | "2 weeks"       | 2      | "weeks"       |
      | 06 | Build, yes/no, 1 time per week              | 2026-10-26T08:00:00Z | 2026-10-26 | no date                   | 2026-11-11T10:00:00Z | "0 weeks"       | 0      | "weeks"       |
      | 07 | Build, yes/no, fixed days on all seven days | 2026-11-01T08:00:00Z | 2026-11-01 | 2026-11-03                | 2026-11-06T10:00:00Z | "1 commitment"  | 1      | "commitment"  |
      | 08 | Build, yes/no, fixed days on all seven days | 2026-11-01T08:00:00Z | 2026-11-01 | 2026-11-02 and 2026-11-03 | 2026-11-06T10:00:00Z | "2 commitments" | 2      | "commitments" |
      | 09 | Build, yes/no, fixed days on all seven days | 2026-11-01T08:00:00Z | 2026-11-01 | no date                   | 2026-11-06T10:00:00Z | "0 commitments" | 0      | "commitments" |

  @AC-015 @FR-004 @FR-005 @FR-006 @US2-AS7
  Scenario Outline: A goal that has not started has streaks of 0, no recent results and no trend
    Given the registered user `f7.ac015.user.<n>@example.com`, who created the custom habit `Yoga`
    And at 2026-11-23T08:00:00Z `f7.ac015.user.<n>@example.com` created a goal on their custom habit `Yoga` with <settings> and the start date <start>
    When at 2026-11-23T10:00:00Z `f7.ac015.user.<n>@example.com` reads the report of the goal
    Then the current streak is <streak> and the longest streak is <streak>
    And the consistency is "N/A"
    And the recent results are empty, with 0 results of each state
    And the trend has no bar

    Examples:
      | n  | settings                                    | start      | streak          |
      | 01 | Build, yes/no, 1 time per day               | 2026-11-25 | "0 days"        |
      | 02 | Build, yes/no, 1 time per week              | 2026-11-30 | "0 weeks"       |
      | 03 | Build, yes/no, fixed days on all seven days | 2026-11-25 | "0 commitments" |

  @AC-016 @FR-008 @US3-AS1
  Scenario: A late record for a Missed day joins the runs on both sides
    Given the registered user `f7.ac016.user@example.com`, who created the custom habit `Reading`
    And at 2026-11-01T08:00:00Z `f7.ac016.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-01
    And at 2026-11-09T09:00:00Z `f7.ac016.user@example.com` records 20 for each date from 2026-11-01 to 2026-11-06 except 2026-11-04
    And at 2026-11-09T09:30:00Z `f7.ac016.user@example.com` read the report of the goal and got the consistency "83% (5 of 6)", the current streak "2 commitments" and the longest streak "3 commitments"
    When at 2026-11-09T10:00:00Z `f7.ac016.user@example.com` records 20 for 2026-11-04 and reads the report of the goal
    Then the consistency is "100% (6 of 6)"
    And the current streak is "6 commitments" and the longest streak is "6 commitments"
    And the recent results contain 2026-11-04 Met, progress 20 of 20
    And the trend contains the bar (2026-11-04, 20)

  @AC-017 @FR-008 @US3-AS2
  Scenario: Changing a Met record to an amount below the target makes it Failed
    Given the registered user `f7.ac017.user@example.com`, who created the custom habit `Reading`
    And at 2026-11-01T08:00:00Z `f7.ac017.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-01
    And at 2026-11-09T09:00:00Z `f7.ac017.user@example.com` records 20 for each date from 2026-11-01 to 2026-11-06
    And at 2026-11-09T09:30:00Z `f7.ac017.user@example.com` read the report of the goal and got the consistency "100% (6 of 6)"
    When at 2026-11-09T10:00:00Z `f7.ac017.user@example.com` changes the record of 2026-11-03 to 12 and reads the report of the goal
    Then the consistency is "83% (5 of 6)"
    And the current streak is "3 commitments" and the longest streak is "3 commitments"
    And the recent results contain 2026-11-03 Failed, progress 12 of 20
    And the trend contains the bar (2026-11-03, 12)

  @AC-018 @FR-008 @US3-AS3
  Scenario: Deleting the only record of a final commitment makes it Missed
    Given the registered user `f7.ac018.user@example.com`, who created the custom habit `Reading`
    And at 2026-11-01T08:00:00Z `f7.ac018.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-01
    And at 2026-11-09T09:00:00Z `f7.ac018.user@example.com` records 20 for each date from 2026-11-01 to 2026-11-06
    When at 2026-11-09T10:00:00Z `f7.ac018.user@example.com` deletes the record of 2026-11-03 and reads the report of the goal
    Then the consistency is "83% (5 of 6)"
    And the current streak is "3 commitments" and the longest streak is "3 commitments"
    And the recent results contain 2026-11-03 Missed, no record
    And the trend contains the bar (2026-11-03, no value)

  @AC-019 @FR-008 @US3-AS4
  Scenario: Records entered late give the same report as records entered on time
    Given the registered user `f7.ac019.user@example.com`, who created the custom habit `Reading`
    And at 2026-11-01T08:00:00Z `f7.ac019.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-01
    And at 2026-11-01T08:00:00Z `f7.ac019.user@example.com` created a second goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-01
    And on the first goal `f7.ac019.user@example.com` records 20 for 2026-11-01 at 2026-11-01T20:00:00Z, 12 for 2026-11-02 at 2026-11-02T20:00:00Z, 25 for 2026-11-04 at 2026-11-04T20:00:00Z, 20 for 2026-11-05 at 2026-11-05T20:00:00Z and 0 for 2026-11-06 at 2026-11-06T20:00:00Z
    And at 2026-11-09T09:00:00Z, on the second goal, `f7.ac019.user@example.com` records 20 for 2026-11-01, 12 for 2026-11-02, 25 for 2026-11-04, 20 for 2026-11-05 and 0 for 2026-11-06
    When at 2026-11-09T10:00:00Z `f7.ac019.user@example.com` reads the report of the first goal and the report of the second goal
    Then each report has the consistency "50% (3 of 6)", the current streak "0 commitments" and the longest streak "2 commitments"
    And each report's recent results are, in this order: 2026-11-01 Met, 2026-11-02 Failed, 2026-11-03 Missed, 2026-11-04 Met, 2026-11-05 Met, 2026-11-06 Failed, 2026-11-07 Pending, 2026-11-08 Pending
    And each report's trend is, in this order: (2026-11-01, 20), (2026-11-02, 12), (2026-11-03, no value), (2026-11-04, 25), (2026-11-05, 20), (2026-11-06, 0), (2026-11-07, no value), (2026-11-08, no value), (2026-11-09, no value), with the target 20

  @AC-020 @FR-009 @US3-AS5
  Scenario Outline: After a time zone change each result is evaluated with the version of its own date
    Given the registered user `f7.ac020.user.<n>@example.com`, who created the custom habit `Reading`
    And at 2026-11-01T08:00:00Z `f7.ac020.user.<n>@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-01
    And at 2026-11-04T12:00:00Z `f7.ac020.user.<n>@example.com` changed the time zone of their account to `Europe/Kyiv`
    And `f7.ac020.user.<n>@example.com` records 20 for 2026-11-01, 2026-11-02, 2026-11-03 and 2026-11-05
    When at <read> `f7.ac020.user.<n>@example.com` reads the report of the goal
    Then the consistency is <consistency>
    And the recent results are, in this order: <results>

    Examples:
      | n  | read                 | consistency     | results                                                                                                                                |
      | 01 | 2026-11-06T23:00:00Z | "100% (3 of 3)" | 2026-11-01 Met, 2026-11-02 Met, 2026-11-03 Met, 2026-11-04 Pending, 2026-11-05 Pending, 2026-11-06 Pending                             |
      | 02 | 2026-11-08T23:00:00Z | "66% (4 of 6)"  | 2026-11-01 Met, 2026-11-02 Met, 2026-11-03 Met, 2026-11-04 Missed, 2026-11-05 Met, 2026-11-06 Missed, 2026-11-07 Pending, 2026-11-08 Pending |

  @AC-021 @FR-005 @US4-AS1
  Scenario: The recent results are the latest 30 ended results, oldest first
    Given the registered user `f7.ac021.user@example.com`, who created the custom habit `Reading`
    And at 2026-10-01T08:00:00Z `f7.ac021.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-10-01
    And `f7.ac021.user@example.com` records 20 for each date from 2026-10-01 to 2026-11-10 except 2026-10-05, 2026-10-20 and 2026-11-01
    And `f7.ac021.user@example.com` records 12 for 2026-11-01
    When at 2026-11-10T10:00:00Z `f7.ac021.user@example.com` reads the report of the goal
    Then the recent results are 30 entries, one for each date from 2026-10-11 to 2026-11-09, in date order
    And the first entry is 2026-10-11 Met, progress 20 of 20, and the last two are 2026-11-08 Pending, progress 20 of 20, and 2026-11-09 Pending, progress 20 of 20
    And they contain 2026-10-20 Missed, no record, and 2026-11-01 Failed, progress 12 of 20
    And they contain no entry for 2026-10-10 or 2026-11-10

  @AC-022 @FR-005 @FR-001 @FR-003 @US4-AS2
  Scenario: The recent results count each state, and the figures count every result of the attempt
    Given the registered user `f7.ac022.user@example.com`, who created the custom habit `Reading`
    And at 2026-10-01T08:00:00Z `f7.ac022.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-10-01
    And `f7.ac022.user@example.com` records 20 for each date from 2026-10-01 to 2026-11-10 except 2026-10-05, 2026-10-20 and 2026-11-01
    And `f7.ac022.user@example.com` records 12 for 2026-11-01
    When at 2026-11-10T10:00:00Z `f7.ac022.user@example.com` reads the report of the goal
    Then the recent results count 26 Met, 1 Failed, 1 Missed and 2 Pending
    And the consistency is "92% (35 of 38)"
    And the current streak is "6 commitments" and the longest streak is "14 commitments"

  @AC-023 @FR-006 @US4-AS3
  Scenario: A commitment-based duration goal has one bar per commitment, the last 14
    Given the registered user `f7.ac023.user@example.com`, who created the custom habit `Reading`
    And at 2026-11-01T08:00:00Z `f7.ac023.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-01
    And `f7.ac023.user@example.com` records 20 for each date from 2026-11-01 to 2026-11-15, 12 for 2026-11-16 and 25 for 2026-11-18
    When at 2026-11-18T10:00:00Z `f7.ac023.user@example.com` reads the report of the goal
    Then the trend has 14 bars with the target 20, in this order: (2026-11-05, 20), (2026-11-06, 20), (2026-11-07, 20), (2026-11-08, 20), (2026-11-09, 20), (2026-11-10, 20), (2026-11-11, 20), (2026-11-12, 20), (2026-11-13, 20), (2026-11-14, 20), (2026-11-15, 20), (2026-11-16, 12), (2026-11-17, no value), (2026-11-18, 25)

  @AC-024 @FR-006 @US4-AS4
  Scenario: A per-week goal has one bar per week, the last 12, with the week's progress
    Given the registered user `f7.ac024.user@example.com`, who created the custom habit `Yoga`
    And at 2026-08-24T08:00:00Z `f7.ac024.user@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, 3 times per week and the start date 2026-08-24
    And `f7.ac024.user@example.com` records `Yes` for the Monday, the Tuesday and the Wednesday of each week from the week of 2026-08-24 to the week of 2026-10-26
    And `f7.ac024.user@example.com` records `Yes` for 2026-11-09, 2026-11-10 and 2026-11-16
    When at 2026-11-18T10:00:00Z `f7.ac024.user@example.com` reads the report of the goal
    Then the trend has 12 bars with the target 3, in this order: (2026-08-31 – 2026-09-06, 3), (2026-09-07 – 2026-09-13, 3), (2026-09-14 – 2026-09-20, 3), (2026-09-21 – 2026-09-27, 3), (2026-09-28 – 2026-10-04, 3), (2026-10-05 – 2026-10-11, 3), (2026-10-12 – 2026-10-18, 3), (2026-10-19 – 2026-10-25, 3), (2026-10-26 – 2026-11-01, 3), (2026-11-02 – 2026-11-08, no value), (2026-11-09 – 2026-11-15, 2), (2026-11-16 – 2026-11-22, 1)

  @AC-025 @FR-006 @US4-AS5
  Scenario: A commitment-based yes/no goal has one bar per week with its commitments as the target
    Given the registered user `f7.ac025.user@example.com`, who created the custom habit `Walking`
    And at 2026-11-04T08:00:00Z `f7.ac025.user@example.com` created a goal on their custom habit `Walking` with Build, yes/no, fixed days on Monday, Wednesday and Friday and the start date 2026-11-04
    And `f7.ac025.user@example.com` records `Yes` for 2026-11-04, `No` for 2026-11-06, and `Yes` for 2026-11-09, 2026-11-11, 2026-11-13 and 2026-11-16
    When at 2026-11-18T10:00:00Z `f7.ac025.user@example.com` reads the report of the goal
    Then the trend has 3 bars, in this order: (2026-11-02 – 2026-11-08, 1, target 2), (2026-11-09 – 2026-11-15, 3, target 3), (2026-11-16 – 2026-11-22, 1, target 3)

  @AC-026 @FR-006 @US4-AS5
  Scenario: A week's bar begins with its first commitment
    Given the registered user `f7.ac026.user@example.com`, who created the custom habit `Walking`
    And at 2026-11-03T08:00:00Z `f7.ac026.user@example.com` created a goal on their custom habit `Walking` with Build, yes/no, fixed days on Tuesday and Thursday and the start date 2026-11-03
    And `f7.ac026.user@example.com` records `Yes` for 2026-11-03, 2026-11-10 and 2026-11-12
    When at 2026-11-16T10:00:00Z `f7.ac026.user@example.com` reads the report of the goal
    Then the trend has 2 bars with the target 2, in this order: (2026-11-02 – 2026-11-08, 1), (2026-11-09 – 2026-11-15, 2)

  @AC-027 @FR-006 @US4-AS6
  Scenario: A recorded 0 is 0, and a bar without a record has no value, also for Reduce
    Given the registered user `f7.ac027.user@example.com`, who created the custom habit `Reading`, the custom habit `Sugar` and the custom habit `Screen`
    And at 2026-11-01T08:00:00Z `f7.ac027.user@example.com` created a goal on their custom habit `Reading` with Build, count 5 `pages`, fixed days on all seven days and the start date 2026-11-01
    And at 2026-11-01T08:00:00Z `f7.ac027.user@example.com` created a goal on their custom habit `Sugar` with Reduce, count 1 `sweets`, 1 time per day and the start date 2026-11-01
    And at 2026-11-02T08:00:00Z `f7.ac027.user@example.com` created a goal on their custom habit `Screen` with Reduce, duration 120 minutes, 1 time per week and the start date 2026-11-02
    And `f7.ac027.user@example.com` records 0 for 2026-11-01 and 7 for 2026-11-03 on the `Reading` goal, and 0 for 2026-11-01 and 2 for 2026-11-03 on the `Sugar` goal, at 2026-11-05T09:00:00Z
    And `f7.ac027.user@example.com` records 60 for 2026-11-03, 30 for 2026-11-05 and 0 for 2026-11-16 on the `Screen` goal, at 2026-11-18T09:00:00Z
    When `f7.ac027.user@example.com` reads the report of the `Reading` goal and of the `Sugar` goal at 2026-11-05T10:00:00Z, and the report of the `Screen` goal at 2026-11-18T10:00:00Z
    Then the `Reading` goal's trend is, with the target 5: (2026-11-01, 0), (2026-11-02, no value), (2026-11-03, 7), (2026-11-04, no value), (2026-11-05, no value)
    And the `Sugar` goal's trend is, with the target 1: (2026-11-01, 0), (2026-11-02, no value), (2026-11-03, 2), (2026-11-04, no value), (2026-11-05, no value)
    And the `Screen` goal's trend is, with the target 120: (2026-11-02 – 2026-11-08, 90), (2026-11-09 – 2026-11-15, no value), (2026-11-16 – 2026-11-22, 0)

  @AC-028 @FR-006 @US4-AS7
  Scenario: A Reduce yes/no week counts its "Yes" records against 0, a Build duration week counts records that reach the amount
    Given the registered user `f7.ac028.user@example.com`, who created the custom habit `Smoking` and the custom habit `Cycling`
    And at 2026-11-02T08:00:00Z `f7.ac028.user@example.com` created a goal on their custom habit `Smoking` with Reduce, yes/no, fixed days on Monday, Wednesday and Friday and the start date 2026-11-02
    And at 2026-11-02T08:00:00Z `f7.ac028.user@example.com` created a goal on their custom habit `Cycling` with Build, duration 30 minutes, 3 times per week and the start date 2026-11-02
    And `f7.ac028.user@example.com` records `Yes` for 2026-11-02, `No` for 2026-11-04 and `No` for 2026-11-06 on the `Smoking` goal
    And `f7.ac028.user@example.com` records 30 for 2026-11-02, 25 for 2026-11-04 and 40 for 2026-11-06 on the `Cycling` goal
    When at 2026-11-09T10:00:00Z `f7.ac028.user@example.com` reads the report of each goal
    Then the `Smoking` goal's trend is, with the target 0: (2026-11-02 – 2026-11-08, 1), (2026-11-09 – 2026-11-15, no value)
    And the `Cycling` goal's trend is, with the target 3: (2026-11-02 – 2026-11-08, 2), (2026-11-09 – 2026-11-15, no value)

  @AC-029 @FR-011 @FR-005 @US5-AS1
  Scenario: Each goal of the overview has its figures, its last 15 results and what can be recorded
    Given the registered user `f7.ac029.user@example.com`, who created the custom habit `Reading` and the custom habit `Yoga`
    And at 2026-11-01T08:00:00Z `f7.ac029.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-01
    And at 2026-11-01T08:00:00Z `f7.ac029.user@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, 1 time per day and the start date 2026-11-25
    And `f7.ac029.user@example.com` records 20 for each date from 2026-11-01 to 2026-11-21 except 2026-11-10 on the `Reading` goal
    When at 2026-11-21T10:00:00Z `f7.ac029.user@example.com` reads the goals overview
    Then it lists the `Reading` goal first, named "Reading, 20 min daily", Build, with the consistency "94% (17 of 18)" and the current streak "8 commitments"
    And the `Reading` goal's last 15 ended results are one entry for each date from 2026-11-06 to 2026-11-20 in date order, of which 2026-11-10 is Missed, 2026-11-19 and 2026-11-20 are Pending and every other is Met
    And the `Reading` goal has a Pending result, something can be recorded now, and its Due entry is 2026-11-21, progress 20 of 20
    And it lists the `Yoga` goal second, with the consistency "N/A", the current streak "0 days", no ended result, no Pending result, nothing that can be recorded now and no Due entry

  @AC-030 @FR-011 @US5-AS2
  Scenario: The overview puts goals with a Pending result first, then those with a Due result, each group in name and creation order
    Given the registered user `f7.ac030.user@example.com`, who created the custom habit `Walking`
    And at 2026-11-16T08:00:00Z `f7.ac030.user@example.com` created, in this order, goals on their custom habit `Walking` with Build, yes/no, fixed days on all seven days: with the name `C` and the start date 2026-11-20; with the name `D` and the start date 2026-11-16; with the name `A` and the start date 2026-11-18; with the name `B` and the start date 2026-11-16; with the name `A` and the start date 2026-11-18
    When at 2026-11-18T10:00:00Z `f7.ac030.user@example.com` reads the goals overview
    Then the goals are, in this order: `B`, `D`, the `A` created third, the `A` created fifth, `C`
    And `B` and `D` have a Pending result, and `A`, `A` and `C` have none

  @AC-031 @FR-011 @US5-AS3
  Scenario: The overview gives the progress of each Due entry
    Given the registered user `f7.ac031.user@example.com`, who created the custom habit `Yoga` and the custom habit `Reading`
    And at 2026-11-16T08:00:00Z `f7.ac031.user@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, 3 times per week, the name `Yoga week` and the start date 2026-11-16
    And at 2026-11-16T08:00:00Z `f7.ac031.user@example.com` created a goal on their custom habit `Reading` with Build, count 20 `pages`, fixed days on all seven days, the name `Pages` and the start date 2026-11-18
    And at 2026-11-16T08:00:00Z `f7.ac031.user@example.com` created a goal on their custom habit `Reading` with Build, count 20 `pages`, fixed days on all seven days, the name `Later pages` and the start date 2026-11-20
    And `f7.ac031.user@example.com` records `Yes` for 2026-11-16 on `Yoga week` and 12 for 2026-11-18 on `Pages`
    When at 2026-11-18T10:00:00Z `f7.ac031.user@example.com` reads the goals overview
    Then the overview lists, in this order: `Pages`, `Yoga week`, `Later pages`
    And it gives `Pages` the Due entry 2026-11-18, progress 12 of 20, `Yoga week` the Due entry 2026-11-16 – 2026-11-22, progress 1 of 3, and `Later pages` no Due entry

  @AC-032 @FR-012 @US6-AS1 @US6-AS3
  Scenario: The dashboard counts Due, Pending, Missed-this-week results and goals
    Given the registered user `f7.ac032.user@example.com`, who created the custom habit `Reading` and the custom habit `Yoga`
    And at 2026-11-09T08:00:00Z `f7.ac032.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-09
    And at 2026-11-09T08:00:00Z `f7.ac032.user@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, 3 times per week and the start date 2026-11-09
    And at 2026-11-18T09:00:00Z `f7.ac032.user@example.com` records, on the `Reading` goal, 20 for 2026-11-09, 2026-11-10, 2026-11-11 and 2026-11-14, 12 for 2026-11-16 and 5 for 2026-11-18, and, on the `Yoga` goal, `Yes` for 2026-11-09, 2026-11-10 and 2026-11-16
    When at 2026-11-18T10:00:00Z `f7.ac032.user@example.com` reads the dashboard
    Then the counts are 2 Due, 2 Pending, 2 Missed this week and 2 goals

  @AC-033 @FR-012 @US6-AS2 @US6-AS3
  Scenario: The Today list has Pending, then Due, then this week's Missed results
    Given the registered user `f7.ac033.user@example.com`, who created the custom habit `Reading` and the custom habit `Yoga`
    And at 2026-11-09T08:00:00Z `f7.ac033.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-09
    And at 2026-11-09T08:00:00Z `f7.ac033.user@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, 3 times per week and the start date 2026-11-09
    And at 2026-11-18T09:00:00Z `f7.ac033.user@example.com` records, on the `Reading` goal, 20 for 2026-11-09, 2026-11-10, 2026-11-11 and 2026-11-14, 12 for 2026-11-16 and 5 for 2026-11-18, and, on the `Yoga` goal, `Yes` for 2026-11-09, 2026-11-10 and 2026-11-16
    And at 2026-11-18T09:30:00Z `f7.ac033.user@example.com` saves the note "Overslept." for 2026-11-15 on the `Reading` goal
    When at 2026-11-18T10:00:00Z `f7.ac033.user@example.com` reads the dashboard
    Then the Today list is, in this order:
      | entry                                                                                                                       |
      | ("Reading, 20 min daily", Build, 2026-11-16, Pending, progress 12 of 20, grace ends 2026-11-19T00:00:00Z, record, no note) |
      | ("Reading, 20 min daily", Build, 2026-11-17, Pending, grace ends 2026-11-20T00:00:00Z, no record, no note)                 |
      | ("Reading, 20 min daily", Build, 2026-11-18, Due, progress 5 of 20, record, no note)                                       |
      | ("Yoga, 3 times a week", Build, 2026-11-16 – 2026-11-22, Due, progress 1 of 3, record, no note)                            |
      | ("Reading, 20 min daily", Build, 2026-11-15, Missed, no record, note)                                                      |
      | ("Reading, 20 min daily", Build, 2026-11-13, Missed, no record, no note)                                                   |
    And the Today list has no entry for 2026-11-12 and no entry for the `Yoga` week 2026-11-09 – 2026-11-15

  @AC-034 @FR-012 @US6-AS4
  Scenario Outline: "Missed this week" starts on Monday at 00:00 in the account's time zone and includes the previous Friday
    Given the registered user `f7.ac034.user.<n>@example.com` in <zone>, who created the custom habit `Walking`
    And at 2026-11-09T08:00:00Z `f7.ac034.user.<n>@example.com` created a goal on their custom habit `Walking` with Build, yes/no, fixed days on all seven days and the start date 2026-11-09
    And at 2026-11-15T20:00:00Z `f7.ac034.user.<n>@example.com` records `Yes` for 2026-11-09, 2026-11-10, 2026-11-11, 2026-11-14 and 2026-11-15
    When at <read> `f7.ac034.user.<n>@example.com` reads the dashboard
    Then the count of Missed this week is 1
    And the only Missed entry of the Today list is (the goal, <missed>, Missed), and <pending> is Pending

    Examples:
      | n  | zone          | read                 | missed     | pending    |
      | 01 | `UTC`         | 2026-11-15T23:59:59Z | 2026-11-12 | 2026-11-13 |
      | 02 | `UTC`         | 2026-11-16T00:00:00Z | 2026-11-13 | 2026-11-14 |
      | 03 | `Europe/Kyiv` | 2026-11-15T21:59:59Z | 2026-11-12 | 2026-11-13 |
      | 04 | `Europe/Kyiv` | 2026-11-15T22:00:00Z | 2026-11-13 | 2026-11-14 |

  @AC-035 @FR-012 @US6-AS5
  Scenario: The dashboard lists every goal with its consistency and streaks in the overview order
    Given the registered user `f7.ac035.user@example.com`, who created the custom habit `Reading` and the custom habit `Yoga`
    And at 2026-11-09T08:00:00Z `f7.ac035.user@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, 3 times per week and the start date 2026-11-09
    And at 2026-11-09T08:00:00Z `f7.ac035.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-09
    And at 2026-11-18T09:00:00Z `f7.ac035.user@example.com` records, on the `Reading` goal, 20 for 2026-11-09, 2026-11-10, 2026-11-11 and 2026-11-14, 12 for 2026-11-16 and 5 for 2026-11-18, and, on the `Yoga` goal, `Yes` for 2026-11-09, 2026-11-10 and 2026-11-16
    When at 2026-11-18T10:00:00Z `f7.ac035.user@example.com` reads the dashboard
    Then its goals are, in this order:
      | goal                                                                                                                        |
      | "Reading, 20 min daily" with the consistency "57% (4 of 7)", the current streak "0 commitments" and the longest streak "3 commitments" |
      | "Yoga, 3 times a week" with the consistency "0% (0 of 1)", the current streak "0 weeks" and the longest streak "0 weeks"            |

  @AC-036 @FR-012 @US6-AS6
  Scenario: The recent notes are the 5 with the latest dates across all goals
    Given the registered user `f7.ac036.user@example.com`, who created the custom habit `Reading` and the custom habit `Yoga`
    And at 2026-11-09T08:00:00Z `f7.ac036.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-09
    And at 2026-11-09T08:00:00Z `f7.ac036.user@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, 3 times per week and the start date 2026-11-09
    And at 2026-11-18T09:00:00Z `f7.ac036.user@example.com` records 20 for 2026-11-09, 2026-11-10, 2026-11-11, 2026-11-13, 2026-11-14 and 2026-11-15 on the `Reading` goal
    And at 2026-11-18T09:30:00Z `f7.ac036.user@example.com` saves, on the `Reading` goal, the note "Short walk first." for 2026-11-10, "Forgot." for 2026-11-12, "Good day." for 2026-11-14, "Late start." for 2026-11-16 and "Tired." for 2026-11-17, and, on the `Yoga` goal, the note "Missed class." for 2026-11-09 and "New mat." for 2026-11-16
    When at 2026-11-18T10:00:00Z `f7.ac036.user@example.com` reads the dashboard
    Then the recent notes are, in this order:
      | recent note                                                         |
      | ("Reading, 20 min daily", 2026-11-17, "Tired.", Pending)            |
      | ("Reading, 20 min daily", 2026-11-16, "Late start.", Pending)       |
      | ("Yoga, 3 times a week", 2026-11-16 – 2026-11-22, "New mat.", Due) |
      | ("Reading, 20 min daily", 2026-11-14, "Good day.", Met)             |
      | ("Reading, 20 min daily", 2026-11-12, "Forgot.", Missed)            |
    And they contain neither "Short walk first." nor "Missed class."

  @AC-037 @FR-013 @US6-AS7
  Scenario: A user with no goals gets counts of 0 and empty lists
    Given the registered user `f7.ac037.user@example.com`
    When at 2026-11-18T10:00:00Z `f7.ac037.user@example.com` reads the dashboard and the goals overview
    Then neither is refused
    And the dashboard counts 0 Due, 0 Pending, 0 Missed this week and 0 goals, and its Today list, goal list and recent notes are empty
    And the goals overview is empty

  @AC-038 @FR-012 @US6-AS8
  Scenario: Upcoming entries end the Today list for goals with no Due result, up to 6 days ahead
    Given the registered user `f7.ac038.user@example.com`, who created the custom habit `Walking`
    And at 2026-11-09T08:00:00Z `f7.ac038.user@example.com` created goals on their custom habit `Walking`, in this order: with Build, yes/no, fixed days on Monday, Wednesday and Friday, the name `Walk` and the start date 2026-11-09; with Build, yes/no, fixed days on all seven days, the name `Daily` and the start date 2026-11-17; with Build, yes/no, fixed days on Monday, the name `Weekly` and the start date 2026-11-09; with Build, yes/no, fixed days on all seven days, the name `Soon` and the start date 2026-11-23; with Build, yes/no, fixed days on all seven days, the name `Later` and the start date 2026-11-24
    And `f7.ac038.user@example.com` records `Yes` for 2026-11-09, 2026-11-11 and 2026-11-16 on `Walk`, and `Yes` for 2026-11-09 and 2026-11-16 on `Weekly`
    When at 2026-11-17T10:00:00Z `f7.ac038.user@example.com` reads the dashboard
    Then the Today list is, in this order: (`Walk`, 2026-11-16, Pending, grace ends 2026-11-19T00:00:00Z), (`Weekly`, 2026-11-16, Pending, grace ends 2026-11-19T00:00:00Z), (`Daily`, 2026-11-17, Due), (`Walk`, 2026-11-13, Missed), (`Walk`, 2026-11-18, Upcoming), (`Weekly`, 2026-11-23, Upcoming), (`Soon`, 2026-11-23, Upcoming)
    And the Today list has no Upcoming entry for `Daily` and no entry for `Later`
    And every entry gives the identifier of the goal it names, as creating that goal returned it, and Build
    And the counts are 1 Due, 2 Pending, 1 Missed this week and 5 goals

  @AC-039 @FR-014 @FR-007 @US7-AS1
  Scenario Outline: Another user's goal does not exist for the goal report
    Given the registered user `f7.ac039.owner.<n>@example.com`, who created the custom habit `Reading`
    And at 2026-11-01T08:00:00Z `f7.ac039.owner.<n>@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-01
    And `f7.ac039.owner.<n>@example.com` records 20 for each date from 2026-11-01 to 2026-11-06
    And the registered user `f7.ac039.other.<n>@example.com`
    When at 2026-11-09T10:00:00Z `f7.ac039.other.<n>@example.com` reads the report of <goal>
    Then the result is "This goal does not exist." and no figure is returned

    Examples:
      | n  | goal                                                                            |
      | 01 | the goal of `f7.ac039.owner.01@example.com`                                     |
      | 02 | the identifier `3f2b8c1e-0000-4000-8000-000000000039`, which belongs to no goal |

  @AC-040 @FR-014 @US7-AS2
  Scenario: The dashboard and the overview contain only the acting user's goals
    Given the registered user `f7.ac040.owner@example.com`, who created the custom habit `Reading`
    And at 2026-11-09T08:00:00Z `f7.ac040.owner@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-09
    And `f7.ac040.owner@example.com` records 20 for 2026-11-16 and saves the note "Private note." for 2026-11-16
    And the registered user `f7.ac040.other@example.com`, who created the custom habit `Yoga`
    And at 2026-11-09T08:00:00Z `f7.ac040.other@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, fixed days on all seven days and the start date 2026-11-09
    When at 2026-11-18T10:00:00Z `f7.ac040.other@example.com` reads the goals overview and the dashboard
    Then the overview lists exactly one goal, the goal of `f7.ac040.other@example.com`
    And every entry of the dashboard's Today list, goal list and recent notes belongs to that goal, and no entry contains "Private note." or the goal of `f7.ac040.owner@example.com`
    And the dashboard counts 1 goal

  @AC-041 @FR-015 @FR-007 @US7-AS3 @US7-AS4
  Scenario Outline: An anonymous visitor and an administrator are refused by every use case of this slice
    Given the registered user `f7.ac041.owner.<n>@example.com`, who created the custom habit `Reading`
    And at 2026-11-01T08:00:00Z `f7.ac041.owner.<n>@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-01
    And the administrator `f7.ac041.admin.<n>@example.com`
    When at 2026-11-09T10:00:00Z <actor> calls <use case>
    Then the result is <result> and no data is returned

    Examples:
      | n  | actor                                             | use case                                                                                         | result                            |
      | 01 | the anonymous visitor                             | the goal report for the goal of `f7.ac041.owner.01@example.com`                                  | "You are not signed in."          |
      | 02 | the anonymous visitor                             | the goal report for the identifier `3f2b8c1e-0000-4000-8000-000000000041`, which belongs to no goal | "You are not signed in."          |
      | 03 | the anonymous visitor                             | the goals overview                                                                               | "You are not signed in."          |
      | 04 | the anonymous visitor                             | the dashboard                                                                                    | "You are not signed in."          |
      | 05 | the administrator `f7.ac041.admin.05@example.com` | the goal report for the goal of `f7.ac041.owner.05@example.com`                                  | "Goals are for registered users." |
      | 06 | the administrator `f7.ac041.admin.06@example.com` | the goal report for the identifier `3f2b8c1e-0000-4000-8000-000000000041`, which belongs to no goal | "Goals are for registered users." |
      | 07 | the administrator `f7.ac041.admin.07@example.com` | the goals overview                                                                               | "Goals are for registered users." |
      | 08 | the administrator `f7.ac041.admin.08@example.com` | the dashboard                                                                                    | "Goals are for registered users." |

  @AC-042 @FR-030 @US8-AS1
  Scenario: demo4's Reading goal reads "92% (24 of 26)" with a streak of 3 and a longest of 18
    Given the demo run at 2026-11-18T10:00:00Z
    When at 2026-11-18T10:00:00Z `demo4@example.com` reads the report of its goal "Reading, 20 min daily"
    Then the consistency is "92% (24 of 26)" with the percentage 92, the successful number 24 and the counted number 26
    And the current streak is "3 commitments" and the longest streak is "18 commitments"

  @AC-043 @FR-030 @US8-AS2
  Scenario: demo4's Sugar goal reads "92% (24 of 26)" with a streak of 1 day and a longest of 16 days
    Given the demo run at 2026-11-18T10:00:00Z
    When at 2026-11-18T10:00:00Z `demo4@example.com` reads the report of its goal "Sugar, at most 1 sweet a day"
    Then the consistency is "92% (24 of 26)"
    And the current streak is "1 day" and the longest streak is "16 days"

  @AC-044 @FR-030 @US8-AS3
  Scenario Outline: demo4's Yoga goal reads its weeks by the weekday of the run
    Given the demo run at <run>
    When at <run> `demo4@example.com` reads the report of its goal "Yoga, 3 times a week"
    Then the consistency is <consistency>
    And the current streak is <current> and the longest streak is <longest>

    Examples:
      | n  | run                  | consistency    | current   | longest   |
      | 01 | 2026-11-18T10:00:00Z | "33% (1 of 3)" | "1 week"  | "1 week"  |
      | 02 | 2026-11-22T10:00:00Z | "33% (1 of 3)" | "1 week"  | "1 week"  |
      | 03 | 2026-11-17T10:00:00Z | "0% (0 of 2)"  | "0 weeks" | "0 weeks" |
      | 04 | 2026-11-16T10:00:00Z | "33% (1 of 3)" | "0 weeks" | "1 week"  |

  @AC-045 @FR-030 @FR-012 @US8-AS4
  Scenario: demo4's dashboard has Pending and Due results, no Missed one, and its notes
    Given the demo run at 2026-11-18T10:00:00Z
    When at 2026-11-18T10:00:00Z `demo4@example.com` reads the dashboard
    Then the counts are 3 Due, 4 Pending, 0 Missed this week and 3 goals
    And the Today list is, in this order:
      | entry                                                                                                 |
      | ("Reading, 20 min daily", 2026-11-16, Pending, progress 25 of 20, grace ends 2026-11-19T00:00:00Z) |
      | ("Sugar, at most 1 sweet a day", 2026-11-16, Pending, grace ends 2026-11-19T00:00:00Z)             |
      | ("Reading, 20 min daily", 2026-11-17, Pending, grace ends 2026-11-20T00:00:00Z)                    |
      | ("Sugar, at most 1 sweet a day", 2026-11-17, Pending, grace ends 2026-11-20T00:00:00Z)             |
      | ("Reading, 20 min daily", 2026-11-18, Due)                                                         |
      | ("Sugar, at most 1 sweet a day", 2026-11-18, Due)                                                  |
      | ("Yoga, 3 times a week", 2026-11-16 – 2026-11-22, Due)                                             |
    And the recent notes are, in this order:
      | recent note                                                                           |
      | ("Sugar, at most 1 sweet a day", 2026-11-14, "Birthday cake at work.", Failed)        |
      | ("Reading, 20 min daily", 2026-11-12, "Fell asleep early.", Failed)                   |
      | ("Reading, 20 min daily", 2026-11-08, "Away for the weekend.", Missed)                |
      | ("Yoga, 3 times a week", 2026-11-02 – 2026-11-08, "Knee was sore.", Failed)           |

  @AC-046 @FR-031 @US8-AS5
  Scenario: Demo loading and demo history loading are unchanged, and demo1 to demo3 have no final result
    When the demo run at 2026-11-18T10:00:00Z
    Then every result of demo history loading names `demo4@example.com`
    And no result of demo loading or of demo history loading names `demo5@example.com`
    And at 2026-11-18T10:00:00Z, for each of `demo1@example.com`, `demo2@example.com` and `demo3@example.com`, every goal of its dashboard has the consistency "N/A", a current streak with the number 0 and a longest streak with the number 0

  @AC-047 @FR-032 @FR-031 @US8-AS7
  Scenario: Demo report loading creates demo5 with its three goals
    When the demo run at 2026-11-18T10:00:00Z
    Then the results of demo report loading are exactly, in this order: (`demo5@example.com`, "created"), (`demo5@example.com`, "demo goals added"), (`demo5@example.com`, "demo records added")
    And no result of demo report loading contains "Walking", "Strength training", "Screen time", "On holiday.", "Sore shoulder.", "Long train ride.", "min" or "120"
    And sign-in with `demo5@example.com` and `example2026` succeeds, and its goals overview lists exactly three goals, in this order: "Screen time, at most 120 min a day", "Walking, 30 min, 3 days a week", "Strength training, 2 times a week"

  @AC-048 @FR-032 @US8-AS8
  Scenario Outline: demo5's reports on a Wednesday run
    Given the demo run at 2026-11-18T10:00:00Z
    When at 2026-11-18T10:00:00Z `demo5@example.com` reads the report of each of its goals
    Then <goal> has the consistency <consistency>, the current streak <current>, the longest streak <longest> and <bars> trend bars

    Examples:
      | n  | goal                                 | consistency      | current         | longest          | bars |
      | 01 | "Screen time, at most 120 min a day" | "93% (77 of 82)" | "0 days"        | "34 days"        | 14   |
      | 02 | "Strength training, 2 times a week"  | "81% (9 of 11)"  | "3 weeks"       | "4 weeks"        | 12   |
      | 03 | "Walking, 30 min, 3 days a week"     | "82% (29 of 35)" | "7 commitments" | "15 commitments" | 14   |

  @AC-049 @FR-032
  Scenario Outline: demo5's Strength training and Screen time goals on other weekdays
    Given the demo run at <run>
    When at <run> `demo5@example.com` reads the report of "Strength training, 2 times a week" and of "Screen time, at most 120 min a day"
    Then "Strength training, 2 times a week" has the consistency <strength>, the current streak <current>, the longest streak <longest> and 12 trend bars
    And "Screen time, at most 120 min a day" has the consistency "93% (77 of 82)", the current streak "0 days", the longest streak "34 days" and 14 trend bars

    Examples:
      | n  | run                  | strength        | current   | longest   |
      | 01 | 2026-11-16T10:00:00Z | "81% (9 of 11)" | "2 weeks" | "5 weeks" |
      | 02 | 2026-11-17T10:00:00Z | "80% (8 of 10)" | "2 weeks" | "4 weeks" |
      | 03 | 2026-11-22T10:00:00Z | "81% (9 of 11)" | "3 weeks" | "4 weeks" |

  @AC-050 @FR-032 @US8-AS9
  Scenario: A second run of demo report loading leaves demo5 unchanged
    Given the demo run at 2026-11-18T10:00:00Z
    When at 2026-11-19T10:00:00Z demo report loading runs again with the demo password `example2026`
    Then its results are exactly (`demo5@example.com`, "already exists")
    And at 2026-11-19T10:00:00Z `demo5@example.com`'s goals overview lists exactly the goals "Screen time, at most 120 min a day", "Walking, 30 min, 3 days a week" and "Strength training, 2 times a week"
    And at 2026-11-19T10:00:00Z the report of "Screen time, at most 120 min a day" has the consistency "93% (78 of 83)", the current streak "1 day" and the longest streak "34 days"

  @AC-051 @FR-032 @US8-AS9
  Scenario: An account that already has the demo5 email, in any case, is left unchanged
    Given the AC database is reset
    And at 2026-11-18T10:00:00Z demo loading and demo history loading run with the demo password `example2026`
    And the anonymous visitor registered the email `Demo5@Example.com` with the password `walking42`, with no time zone and no name, and registration succeeded
    When at 2026-11-18T10:00:00Z demo report loading runs with the demo password `example2026`
    Then its results are exactly (`demo5@example.com`, "already exists")
    And sign-in with `Demo5@Example.com` and `walking42` succeeds, and that account's goals overview is empty

  @AC-052 @FR-032 @US8-AS10
  Scenario Outline: Without a valid demo password demo report loading creates nothing
    Given the AC database is reset
    And at 2026-11-18T10:00:00Z demo loading and demo history loading run with the demo password `example2026`
    When at 2026-11-18T10:00:00Z demo report loading runs with <password>
    Then its results are exactly the one result <result>
    And sign-in with `demo5@example.com` and `example2026` fails

    Examples:
      | n  | password                | result                                           |
      | 01 | no demo password        | "no demo password configured"                    |
      | 02 | the demo password `abc` | "demo password does not meet the password rule" |

  @AC-053 @FR-032 @FR-012 @US8-AS11
  Scenario Outline: demo5 has one result Missed this week on every weekday
    Given the demo run at <run>
    When at <run> `demo5@example.com` reads the dashboard
    Then the count of Missed this week is 1
    And the only Missed entry of the Today list is ("Screen time, at most 120 min a day", <date>, Missed, no record)

    Examples:
      | n  | run                  | date       |
      | 01 | 2026-11-16T10:00:00Z | 2026-11-13 |
      | 02 | 2026-11-17T10:00:00Z | 2026-11-14 |
      | 03 | 2026-11-18T10:00:00Z | 2026-11-15 |
      | 04 | 2026-11-19T10:00:00Z | 2026-11-16 |
      | 05 | 2026-11-20T10:00:00Z | 2026-11-17 |
      | 06 | 2026-11-21T10:00:00Z | 2026-11-18 |
      | 07 | 2026-11-22T10:00:00Z | 2026-11-19 |

  @AC-054 @FR-032 @FR-011 @FR-012 @US8-AS12
  Scenario: demo5's dashboard on Wednesday 2026-11-18
    Given the demo run at 2026-11-18T10:00:00Z
    When at 2026-11-18T10:00:00Z `demo5@example.com` reads the dashboard
    Then the counts are 3 Due, 3 Pending, 1 Missed this week and 3 goals
    And the Today list is, in this order:
      | entry                                                                                                   |
      | ("Screen time, at most 120 min a day", Reduce, 2026-11-16, Pending, grace ends 2026-11-19T00:00:00Z) |
      | ("Walking, 30 min, 3 days a week", Build, 2026-11-16, Pending, grace ends 2026-11-19T00:00:00Z)      |
      | ("Screen time, at most 120 min a day", Reduce, 2026-11-17, Pending, grace ends 2026-11-20T00:00:00Z) |
      | ("Screen time, at most 120 min a day", Reduce, 2026-11-18, Due)                                      |
      | ("Walking, 30 min, 3 days a week", Build, 2026-11-18, Due)                                           |
      | ("Strength training, 2 times a week", Build, 2026-11-16 – 2026-11-22, Due)                          |
      | ("Screen time, at most 120 min a day", Reduce, 2026-11-15, Missed)                                   |
    And the Today list has no Upcoming entry

  @AC-055 @FR-032
  Scenario: A demo goal whose catalog habit is not Published is left out
    Given the AC database is reset
    And the administrator `f7.ac055.admin@example.com`
    And at 2026-11-18T10:00:00Z demo loading runs with the demo password `example2026`
    And `f7.ac055.admin@example.com` unpublished the catalog habit `Walking` with confirmation
    When at 2026-11-18T10:00:00Z demo history loading and demo report loading run with the demo password `example2026`
    Then the results of demo report loading are exactly, in this order: (`demo5@example.com`, "created"), (`demo5@example.com`, "demo goals added"), (`demo5@example.com`, "demo records added")
    And `demo5@example.com`'s goals overview lists exactly two goals, in this order: "Screen time, at most 120 min a day", "Strength training, 2 times a week"

  @AC-056 @FR-005 @FR-006 @FR-001
  Scenario: A per-week goal started mid-week reports from its first full week
    Given the registered user `f7.ac056.user@example.com`, who created the custom habit `Yoga`
    And at 2026-11-04T08:00:00Z `f7.ac056.user@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, 1 time per week and the start date 2026-11-04
    And `f7.ac056.user@example.com` records `Yes` for 2026-11-10
    When at 2026-11-18T10:00:00Z `f7.ac056.user@example.com` reads the report of the goal
    Then the recent results are exactly one entry, 2026-11-09 – 2026-11-15 Met
    And the consistency is "100% (1 of 1)" and the current streak is "1 week"
    And the trend is, with the target 1: (2026-11-09 – 2026-11-15, 1), (2026-11-16 – 2026-11-22, no value)

  @AC-057 @FR-001 @FR-003 @FR-008
  Scenario Outline: Around a clock change back, a grace period ends at 23:00 local time and a 25-hour day counts once
    Given the registered user `f7.ac057.user.<n>@example.com` in `Europe/Berlin`, who created the custom habit `Walking`
    And at 2026-10-19T20:00:00Z `f7.ac057.user.<n>@example.com` created a goal on their custom habit `Walking` with Build, yes/no, fixed days on all seven days and the start date 2026-10-20
    And at 2026-10-26T12:00:00Z `f7.ac057.user.<n>@example.com` records `Yes` for 2026-10-20, 2026-10-21, 2026-10-22, 2026-10-23 and 2026-10-25
    When at <read> `f7.ac057.user.<n>@example.com` reads the report of the goal
    Then the consistency is <consistency> and the current streak is <current>
    And the recent results contain <date> <state> exactly once

    Examples:
      | n  | read                 | consistency     | current         | date       | state   |
      | 01 | 2026-10-26T21:59:59Z | "100% (4 of 4)" | "4 commitments" | 2026-10-24 | Pending |
      | 02 | 2026-10-26T22:00:00Z | "80% (4 of 5)"  | "0 commitments" | 2026-10-24 | Missed  |
      | 03 | 2026-10-27T22:59:59Z | "80% (4 of 5)"  | "0 commitments" | 2026-10-25 | Pending |
      | 04 | 2026-10-27T23:00:00Z | "83% (5 of 6)"  | "1 commitment"  | 2026-10-25 | Met     |

  @AC-058 @FR-006
  Scenario: A duration goal in hours has its trend values and target in minutes
    Given the registered user `f7.ac058.user@example.com`, who created the custom habit `Reading`
    And at 2026-11-01T08:00:00Z `f7.ac058.user@example.com` created a goal on their custom habit `Reading` with Build, duration 1 in hours, fixed days on all seven days and the start date 2026-11-01
    And `f7.ac058.user@example.com` records 45 for 2026-11-01 and 60 for 2026-11-02
    When at 2026-11-03T10:00:00Z `f7.ac058.user@example.com` reads the report of the goal
    Then the trend is, with the target 60: (2026-11-01, 45), (2026-11-02, 60), (2026-11-03, no value)

  @AC-059 @FR-008 @FR-001
  Scenario: A goal whose catalog habit is Unpublished keeps its reports
    Given the administrator `f7.ac059.admin@example.com`, who at 2026-11-01T07:00:00Z created the catalog habit `A7.059 Rowing` and published it
    And the registered user `f7.ac059.user@example.com`
    And at 2026-11-01T08:00:00Z `f7.ac059.user@example.com` created a goal on the catalog habit `A7.059 Rowing` with Build, yes/no, fixed days on all seven days and the start date 2026-11-01
    And at 2026-11-06T09:00:00Z `f7.ac059.user@example.com` records `Yes` for 2026-11-01, 2026-11-02 and 2026-11-03
    And at 2026-11-06T09:30:00Z `f7.ac059.admin@example.com` unpublished the catalog habit `A7.059 Rowing` with confirmation
    When at 2026-11-06T10:00:00Z `f7.ac059.user@example.com` reads the report of the goal
    Then the consistency is "100% (3 of 3)" and the current streak is "3 commitments"

  @AC-060 @FR-012 @FR-032 @US6-AS3
  Scenario: A per-week goal's Pending week is in the Today list
    Given the demo run at 2026-11-16T10:00:00Z
    When at 2026-11-16T10:00:00Z `demo5@example.com` reads the dashboard
    Then the Today list contains ("Strength training, 2 times a week", 2026-11-09 – 2026-11-15, Pending, progress 2 of 2, grace ends 2026-11-18T00:00:00Z)
    And it contains ("Strength training, 2 times a week", 2026-11-16 – 2026-11-22, Due)

  @AC-061 @FR-001 @FR-003 @FR-008
  Scenario Outline: Around a clock change forward, a grace period ends at 01:00 local time and a 23-hour day counts once
    Given the registered user `f7.ac061.user.<n>@example.com` in `Europe/Berlin`, who created the custom habit `Walking`
    And at 2026-03-22T20:00:00Z `f7.ac061.user.<n>@example.com` created a goal on their custom habit `Walking` with Build, yes/no, fixed days on all seven days and the start date 2026-03-23
    And at 2026-03-30T12:00:00Z `f7.ac061.user.<n>@example.com` records `Yes` for 2026-03-23, 2026-03-24, 2026-03-25, 2026-03-26, 2026-03-27 and 2026-03-29
    When at <read> `f7.ac061.user.<n>@example.com` reads the report of the goal
    Then the consistency is <consistency> and the current streak is <current>
    And the recent results contain <date> <state> exactly once

    Examples:
      | n  | read                 | consistency     | current         | date       | state   |
      | 01 | 2026-03-30T22:59:59Z | "100% (5 of 5)" | "5 commitments" | 2026-03-28 | Pending |
      | 02 | 2026-03-30T23:00:00Z | "83% (5 of 6)"  | "0 commitments" | 2026-03-28 | Missed  |
      | 03 | 2026-03-31T22:00:00Z | "85% (6 of 7)"  | "1 commitment"  | 2026-03-29 | Met     |

  @AC-062 @FR-001 @FR-003 @FR-008
  Scenario Outline: A per-week or per-day result counts only from the end of its grace period
    Given the registered user `f7.ac062.user.<n>@example.com`, who created the custom habit `Habit`
    And at <created> `f7.ac062.user.<n>@example.com` created a goal on their custom habit `Habit` with <settings> and the start date <start>
    And `f7.ac062.user.<n>@example.com` records <records>
    When at <read> `f7.ac062.user.<n>@example.com` reads the report of the goal
    Then the consistency is <consistency> and the current streak is <current>
    And the recent results begin with <first>

    Examples:
      | n  | settings                                 | created              | start      | records              | read                 | consistency     | current   | first                           |
      | 01 | Build, yes/no, 1 time per week           | 2026-11-02T08:00:00Z | 2026-11-02 | nothing              | 2026-11-10T23:59:59Z | "N/A"           | "0 weeks" | 2026-11-02 – 2026-11-08 Pending |
      | 02 | Build, yes/no, 1 time per week           | 2026-11-02T08:00:00Z | 2026-11-02 | nothing              | 2026-11-11T00:00:00Z | "0% (0 of 1)"   | "0 weeks" | 2026-11-02 – 2026-11-08 Missed  |
      | 03 | Build, yes/no, 1 time per week           | 2026-11-02T08:00:00Z | 2026-11-02 | `Yes` for 2026-11-02 | 2026-11-11T00:00:00Z | "100% (1 of 1)" | "1 week"  | 2026-11-02 – 2026-11-08 Met     |
      | 04 | Reduce, count 1 `sweets`, 1 time per day | 2026-11-01T08:00:00Z | 2026-11-01 | 0 for 2026-11-01     | 2026-11-03T23:59:59Z | "N/A"           | "0 days"  | 2026-11-01 Pending              |
      | 05 | Reduce, count 1 `sweets`, 1 time per day | 2026-11-01T08:00:00Z | 2026-11-01 | 0 for 2026-11-01     | 2026-11-04T00:00:00Z | "100% (1 of 1)" | "1 day"   | 2026-11-01 Met                  |
      | 06 | Reduce, count 1 `sweets`, 1 time per day | 2026-11-01T08:00:00Z | 2026-11-01 | 2 for 2026-11-01     | 2026-11-04T00:00:00Z | "0% (0 of 1)"   | "0 days"  | 2026-11-01 Failed               |

  @AC-063 @FR-016 @FR-011 @US5-AS4
  Scenario: The slice 4 goal list is unchanged, and the overview carries its entries
    Given the registered user `f7.ac063.user@example.com`, who created the custom habit `Walking`
    And at 2026-11-16T08:00:00Z `f7.ac063.user@example.com` created, in this order, goals on their custom habit `Walking` with Build, yes/no, fixed days on all seven days: with the name `C` and the start date 2026-11-20; with the name `A` and the start date 2026-11-18; with the name `B` and the start date 2026-11-16
    And at 2026-11-18T09:00:00Z `f7.ac063.user@example.com` read the goal list and got the entries of `A`, `B` and `C`, in this order
    And at 2026-11-18T09:30:00Z `f7.ac063.user@example.com` records `Yes` for 2026-11-16 on `B` and reads the goals overview, which lists `B`, `A`, `C` in this order
    When at 2026-11-18T09:45:00Z `f7.ac063.user@example.com` reads the goal list and the goals overview
    Then the goal list lists `A`, `B` and `C`, in this order, and each entry is equal, field by field, to the entry of the same goal that the goal list gave in the earlier read
    And the goals overview lists `B`, `A`, `C`, in this order, and gives each goal the same name, Build or Reduce, schedule and start or next information as its goal list entry

  @AC-064 @FR-006 @US4-AS5
  Scenario: A commitment-based yes/no trend keeps its last 12 weeks
    Given the registered user `f7.ac064.user@example.com`, who created the custom habit `Walking`
    And at 2026-08-24T08:00:00Z `f7.ac064.user@example.com` created a goal on their custom habit `Walking` with Build, yes/no, fixed days on Monday, Wednesday and Friday and the start date 2026-08-24
    And `f7.ac064.user@example.com` records `Yes` for every Monday, Wednesday and Friday from 2026-08-24 to 2026-11-16
    When at 2026-11-18T10:00:00Z `f7.ac064.user@example.com` reads the report of the goal
    Then the trend has 12 bars with the target 3, in this order: (2026-08-31 – 2026-09-06, 3), (2026-09-07 – 2026-09-13, 3), (2026-09-14 – 2026-09-20, 3), (2026-09-21 – 2026-09-27, 3), (2026-09-28 – 2026-10-04, 3), (2026-10-05 – 2026-10-11, 3), (2026-10-12 – 2026-10-18, 3), (2026-10-19 – 2026-10-25, 3), (2026-10-26 – 2026-11-01, 3), (2026-11-02 – 2026-11-08, 3), (2026-11-09 – 2026-11-15, 3), (2026-11-16 – 2026-11-22, 1)

  @AC-065 @FR-006
  Scenario: A per-day goal of 2 times has one bar per day with the day's progress against 2
    Given the registered user `f7.ac065.user@example.com`, who created the custom habit `Water` and the custom habit `Reading`
    And at 2026-11-01T08:00:00Z `f7.ac065.user@example.com` created a goal on their custom habit `Water` with Build, yes/no, 2 times per day and the start date 2026-11-01
    And at 2026-11-01T08:00:00Z `f7.ac065.user@example.com` created a goal on their custom habit `Reading` with Build, count 10 `pages`, 2 times per day and the start date 2026-11-01
    And `f7.ac065.user@example.com` records, on the `Water` goal, `Yes` twice for 2026-11-01, `Yes` once for 2026-11-02 and `Yes` three times for 2026-11-04
    And `f7.ac065.user@example.com` records, on the `Reading` goal, 12 and 8 for 2026-11-01, and 10 and 15 for 2026-11-02
    When at 2026-11-04T10:00:00Z `f7.ac065.user@example.com` reads the report of each goal
    Then the `Water` goal's trend is, with the target 2: (2026-11-01, 2), (2026-11-02, 1), (2026-11-03, no value), (2026-11-04, 3)
    And the `Reading` goal's trend is, with the target 2: (2026-11-01, 1), (2026-11-02, 2), (2026-11-03, no value), (2026-11-04, no value)

  @AC-066 @FR-032 @FR-006 @FR-012 @US8-AS8
  Scenario: demo5's amounts and notes on a Wednesday run
    Given the demo run at 2026-11-18T10:00:00Z
    When at 2026-11-18T10:00:00Z `demo5@example.com` reads the report of "Walking, 30 min, 3 days a week", the report of "Screen time, at most 120 min a day" and the dashboard
    Then the trend of "Walking, 30 min, 3 days a week" is, with the target 30: (2026-10-19, 30), (2026-10-21, 30), (2026-10-23, 18), (2026-10-26, 18), (2026-10-28, 18), (2026-10-30, 30), (2026-11-02, 30), (2026-11-04, 35), (2026-11-06, 35), (2026-11-09, 35), (2026-11-11, 30), (2026-11-13, 30), (2026-11-16, no value), (2026-11-18, no value)
    And the trend of "Screen time, at most 120 min a day" is, with the target 120: (2026-11-05, 110), (2026-11-06, 90), (2026-11-07, 110), (2026-11-08, 90), (2026-11-09, 110), (2026-11-10, 90), (2026-11-11, 110), (2026-11-12, 90), (2026-11-13, 110), (2026-11-14, 90), (2026-11-15, no value), (2026-11-16, 100), (2026-11-17, no value), (2026-11-18, no value)
    And the dashboard's recent notes are, in this order:
      | recent note                                                                                 |
      | ("Strength training, 2 times a week", 2026-10-19 – 2026-10-25, "Sore shoulder.", Failed) |
      | ("Screen time, at most 120 min a day", 2026-10-18, "Long train ride.", Failed)            |
      | ("Walking, 30 min, 3 days a week", 2026-10-05, "On holiday.", Missed)                     |

  @AC-067 @FR-012 @US6-AS4
  Scenario Outline: "This week" follows the account's time zone, also for days of an older version
    Given the registered user `f7.ac067.user.<n>@example.com`, who created the custom habit `Walking`
    And at 2026-11-09T08:00:00Z `f7.ac067.user.<n>@example.com` created a goal on their custom habit `Walking` with Build, yes/no, fixed days on all seven days and the start date 2026-11-09
    And at 2026-11-14T12:00:00Z `f7.ac067.user.<n>@example.com` changed the time zone of their account to `Europe/Kyiv`
    And `f7.ac067.user.<n>@example.com` records `Yes` for 2026-11-09, 2026-11-10, 2026-11-11 and 2026-11-14
    When at <read> `f7.ac067.user.<n>@example.com` reads the dashboard
    Then the count of Missed this week is <missed>
    And the Today list is, in this order: <entries>

    Examples:
      | n  | read                 | missed | entries                                                                                                                                                                                                                   |
      | 01 | 2026-11-15T21:59:59Z | 1      | (the goal, 2026-11-13, Pending, grace ends 2026-11-16T00:00:00Z), (the goal, 2026-11-14, Pending, grace ends 2026-11-17T00:00:00Z), (the goal, 2026-11-15, Due), (the goal, 2026-11-12, Missed)                            |
      | 02 | 2026-11-15T22:00:00Z | 0      | (the goal, 2026-11-13, Pending, grace ends 2026-11-16T00:00:00Z), (the goal, 2026-11-14, Pending, grace ends 2026-11-17T00:00:00Z), (the goal, 2026-11-15, Pending, grace ends 2026-11-17T22:00:00Z), (the goal, 2026-11-16, Due) |
