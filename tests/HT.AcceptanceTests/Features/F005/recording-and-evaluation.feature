@F005
Feature: Recording and evaluation

  @AC-001 @FR-001 @FR-004 @FR-006 @FR-009 @FR-010 @US1-AS1
  Scenario: A partial amount recorded for today is kept and the commitment is Due
    Given the registered user `f5.ac001.user@example.com`, who created the custom habit `Reading`
    And `f5.ac001.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    When `f5.ac001.user@example.com` records 12 for 2026-11-18
    Then recording succeeds with the state Due, not a correction
    And reading the history of the goal from 2026-11-18 to 2026-11-18 gives exactly one entry: 2026-11-18 from `2026-11-18T00:00:00Z` to `2026-11-19T00:00:00Z`, Due, grace ends `2026-11-21T00:00:00Z`, records (2026-11-18, 12), progress 12 of 20 minutes, no note, no correction

  @AC-002 @FR-003 @US1-AS2
  Scenario: Recording again for a commitment replaces its one record
    Given the registered user `f5.ac002.user@example.com`, who created the custom habit `Reading`
    And `f5.ac002.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    And `f5.ac002.user@example.com` recorded 12 for 2026-11-18
    When `f5.ac002.user@example.com` records 25 for 2026-11-18
    Then recording succeeds with the state Due, not a correction
    And reading the history of the goal from 2026-11-18 to 2026-11-18 gives the entry 2026-11-18 with records (2026-11-18, 25) only, and progress 25 of 20 minutes

  @AC-003 @FR-003 @FR-009 @FR-014 @US1-AS3
  Scenario: A per-week period holds one record per recording, listed in date order
    Given the registered user `f5.ac003.user@example.com`, who created the custom habit `Yoga`
    And at `2026-11-16T08:00:00Z` `f5.ac003.user@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, 3 times per week and the start date 2026-11-16
    When at `2026-11-18T10:00:00Z` `f5.ac003.user@example.com` records Yes for 2026-11-18 and then records Yes for 2026-11-16
    Then each recording succeeds with the state Due, not a correction
    And reading the history of the goal from 2026-11-16 to 2026-11-22 gives exactly one entry: 2026-11-16 to 2026-11-22 from `2026-11-16T00:00:00Z` to `2026-11-23T00:00:00Z`, Due, grace ends `2026-11-25T00:00:00Z`, records (2026-11-16, Yes), (2026-11-18, Yes), progress 2 of 3 times

  @AC-004 @FR-003 @FR-009 @FR-014 @US1-AS4
  Scenario: Records on the same date of a per-day period keep the order in which they were stored
    Given the registered user `f5.ac004.user@example.com`, who created the custom habit `Sugar`
    And `f5.ac004.user@example.com` created a goal on their custom habit `Sugar` with Reduce, count 1 `sweet`, 1 time per day and the start date 2026-11-18
    When `f5.ac004.user@example.com` records 1 for 2026-11-18, then records 2 for 2026-11-18, then records 0 for 2026-11-18
    Then each recording succeeds with the state Due, not a correction
    And reading the history of the goal from 2026-11-18 to 2026-11-18 gives the entry 2026-11-18 with records (2026-11-18, 1), (2026-11-18, 2), (2026-11-18, 0), progress total 3 of at most 1

  @AC-005 @FR-002 @US1-AS5
  Scenario Outline: A date that belongs to no commitment or period cannot be recorded
    Given the registered user `f5.ac005.user.<n>@example.com`, who created the custom habit `Practice`
    And at <created at> `f5.ac005.user.<n>@example.com` created a goal on their custom habit `Practice` with Build, yes/no, <schedule> and the start date <start date>
    When at `2026-11-18T10:00:00Z` `f5.ac005.user.<n>@example.com` records Yes for <date>
    Then the result is "Nothing is scheduled on this date."
    And reading the history of the goal from 2026-11-15 to 2026-11-29 gives no record

    Examples:
      | n  | schedule                                | created at             | start date | date       | why                                                               |
      | 01 | fixed days Monday, Wednesday and Friday | `2026-11-16T08:00:00Z` | 2026-11-16 | 2026-11-17 | a Tuesday                                                         |
      | 02 | fixed days on all seven days            | `2026-11-18T10:00:00Z` | 2026-11-18 | 2026-11-17 | before the start date                                             |
      | 03 | 1 time per day                          | `2026-11-18T10:00:00Z` | 2026-11-18 | 2026-11-17 | before the start date                                             |
      | 04 | 3 times per week                        | `2026-11-18T10:00:00Z` | 2026-11-18 | 2026-11-18 | before the first week, 2026-11-23 to 2026-11-29                   |
      | 05 | 3 times per week                        | `2026-11-18T10:00:00Z` | 2026-11-18 | 2026-11-22 | before the first week, and not begun: the date check comes first |

  @AC-006 @FR-002 @US1-AS6
  Scenario Outline: A day that has not begun on the goal's timeline cannot be recorded
    Given the registered user `f5.ac006.user.<n>@example.com` in <time zone>, who created the custom habit `Practice`
    And at `2026-11-16T08:00:00Z` `f5.ac006.user.<n>@example.com` created a goal on their custom habit `Practice` with Build, yes/no, <schedule> and the start date 2026-11-16
    When at <time> `f5.ac006.user.<n>@example.com` records Yes for <date>
    Then <result>

    Examples:
      | n  | time zone   | schedule                                | time                   | date       | result                                                                               |
      | 01 | UTC         | fixed days on all seven days            | `2026-11-18T10:00:00Z` | 2026-11-19 | the result is "You cannot record a day that has not begun." and nothing is recorded |
      | 02 | UTC         | 1 time per day                          | `2026-11-18T10:00:00Z` | 2026-11-19 | the result is "You cannot record a day that has not begun." and nothing is recorded |
      | 03 | UTC         | fixed days Monday, Wednesday and Friday | `2026-11-18T10:00:00Z` | 2026-11-19 | the result is "Nothing is scheduled on this date." and nothing is recorded          |
      | 04 | UTC         | fixed days on all seven days            | `2026-11-18T23:59:59Z` | 2026-11-19 | the result is "You cannot record a day that has not begun." and nothing is recorded |
      | 05 | UTC         | fixed days on all seven days            | `2026-11-19T00:00:00Z` | 2026-11-19 | recording succeeds with the state Due, not a correction                              |
      | 06 | Europe/Kyiv | fixed days on all seven days            | `2026-11-18T21:59:59Z` | 2026-11-19 | the result is "You cannot record a day that has not begun." and nothing is recorded |
      | 07 | Europe/Kyiv | fixed days on all seven days            | `2026-11-18T22:00:00Z` | 2026-11-19 | recording succeeds with the state Due, not a correction                              |
      | 08 | UTC         | 3 times per week                        | `2026-11-18T10:00:00Z` | 2026-11-20 | the result is "You cannot record a day that has not begun." and nothing is recorded |

  @AC-007 @FR-005 @US1-AS7
  Scenario Outline: A value that does not fit the goal's measurement is refused with its message
    Given the registered user `f5.ac007.user.<n>@example.com`, who created the custom habit `Practice`
    And `f5.ac007.user.<n>@example.com` created a goal on their custom habit `Practice` with Build, <measurement>, fixed days on all seven days and the start date 2026-11-18
    When `f5.ac007.user.<n>@example.com` records <value> for 2026-11-18
    Then the result is <message>
    And nothing is recorded

    Examples:
      | n  | measurement         | value    | message                                           |
      | 01 | yes/no              | no value | "Choose Yes or No."                               |
      | 02 | count 5 `pages`     | no value | "Enter a whole number from 0 to 9999."            |
      | 03 | count 5 `pages`     | -1       | "Enter a whole number from 0 to 9999."            |
      | 04 | count 5 `pages`     | 10000    | "Enter a whole number from 0 to 9999."            |
      | 05 | duration 20 minutes | no value | "Enter a whole number of minutes from 0 to 1440." |
      | 06 | duration 20 minutes | -1       | "Enter a whole number of minutes from 0 to 1440." |
      | 07 | duration 20 minutes | 1441     | "Enter a whole number of minutes from 0 to 1440." |
      | 08 | duration 1 in hours | 1441     | "Enter a whole number of minutes from 0 to 1440." |

  @AC-008 @FR-004 @FR-005
  Scenario Outline: The lowest and highest amounts are accepted and kept
    Given the registered user `f5.ac008.user.<n>@example.com`, who created the custom habit `Practice`
    And `f5.ac008.user.<n>@example.com` created a goal on their custom habit `Practice` with Build, <measurement>, fixed days on all seven days and the start date 2026-11-18
    When `f5.ac008.user.<n>@example.com` records <value> for 2026-11-18
    Then recording succeeds with the state Due, not a correction
    And reading the history of the goal from 2026-11-18 to 2026-11-18 gives the entry 2026-11-18 with records (2026-11-18, <value>)

    Examples:
      | n  | measurement         | value |
      | 01 | count 5 `pages`     | 0     |
      | 02 | count 5 `pages`     | 9999  |
      | 03 | duration 20 minutes | 0     |
      | 04 | duration 20 minutes | 1440  |
      | 05 | duration 1 in hours | 1440  |

  @AC-009 @FR-004
  Scenario Outline: A value that does not belong to the goal's measurement is not stored
    Given the registered user `f5.ac009.user.<n>@example.com`, who created the custom habit `Practice`
    And `f5.ac009.user.<n>@example.com` created a goal on their custom habit `Practice` with Build, <measurement>, fixed days on all seven days and the start date 2026-11-18
    When `f5.ac009.user.<n>@example.com` records <given> for 2026-11-18
    Then recording succeeds with the state Due, not a correction
    And reading the history of the goal from 2026-11-18 to 2026-11-18 gives the entry 2026-11-18 with one record, <stored>

    Examples:
      | n  | measurement         | given                           | stored                                     |
      | 01 | yes/no              | Yes together with the amount 5  | (2026-11-18, Yes), with no amount          |
      | 02 | count 5 `pages`     | the amount 3 together with Yes  | (2026-11-18, 3), with neither Yes nor No   |
      | 03 | duration 20 minutes | the amount 12 together with No  | (2026-11-18, 12), with neither Yes nor No  |

  @AC-010 @FR-005 @FR-018 @US1-AS7
  Scenario Outline: Recording checks the acting user, the goal, the date, the value and the note in that order
    Given the registered user `f5.ac010.owner.<n>@example.com`, who created the custom habit `Reading`
    And `f5.ac010.owner.<n>@example.com` created a goal on their custom habit `Reading` with Build, count 5 `pages`, 1 time per day and the start date 2026-11-18
    And the registered user `f5.ac010.other.<n>@example.com`
    And the administrator `f5.ac010.admin.<n>@example.com`
    When <actor> records <value> for <date> with the note <note> on the goal of `f5.ac010.owner.<n>@example.com`
    Then the result is <result>
    And nothing is recorded

    Examples:
      | n  | actor                                              | date       | value    | note                     | result                                        |
      | 01 | the anonymous visitor                              | 2026-11-17 | no value | a text of 501 characters | "You are not signed in."                      |
      | 02 | the administrator `f5.ac010.admin.<n>@example.com` | 2026-11-17 | no value | a text of 501 characters | "Goals are for registered users."             |
      | 03 | `f5.ac010.other.<n>@example.com`                   | 2026-11-17 | no value | a text of 501 characters | "This goal does not exist."                   |
      | 04 | `f5.ac010.owner.<n>@example.com`                   | 2026-11-17 | no value | a text of 501 characters | "Nothing is scheduled on this date."          |
      | 05 | `f5.ac010.owner.<n>@example.com`                   | 2026-11-19 | no value | a text of 501 characters | "You cannot record a day that has not begun." |
      | 06 | `f5.ac010.owner.<n>@example.com`                   | 2026-11-18 | no value | a text of 501 characters | "Enter a whole number from 0 to 9999."        |
      | 07 | `f5.ac010.owner.<n>@example.com`                   | 2026-11-18 | 3        | a text of 501 characters | "The note can have at most 500 characters."   |

  @AC-011 @FR-007 @US1-AS8
  Scenario: A note given with a record is trimmed
    Given the registered user `f5.ac011.user@example.com`, who created the custom habit `Yoga`
    And `f5.ac011.user@example.com` created a goal on their custom habit `Yoga` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    When `f5.ac011.user@example.com` records 15 for 2026-11-18 with the note "  Back was tired.  "
    Then recording succeeds with the state Due, not a correction
    And reading the history of the goal from 2026-11-18 to 2026-11-18 gives the entry 2026-11-18 with records (2026-11-18, 15) and the note "Back was tired."

  @AC-012 @FR-010 @US2-AS1
  Scenario Outline: A commitment is Upcoming before its start instant and Due from it
    Given the registered user `f5.ac012.user.<n>@example.com`, who created the custom habit `Reading`
    And `f5.ac012.user.<n>@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    When at <time> `f5.ac012.user.<n>@example.com` reads the history of the goal from 2026-11-18 to 2026-11-19
    Then it gives, in this order, 2026-11-18 <first> and 2026-11-19 <second>

    Examples:
      | n  | time                   | first                                     | second                                                                              |
      | 01 | `2026-11-18T10:00:00Z` | Due, grace ends `2026-11-21T00:00:00Z`     | from `2026-11-19T00:00:00Z` to `2026-11-20T00:00:00Z`, Upcoming, no grace end      |
      | 02 | `2026-11-18T23:59:59Z` | Due, grace ends `2026-11-21T00:00:00Z`     | Upcoming, no grace end                                                              |
      | 03 | `2026-11-19T00:00:00Z` | Pending, grace ends `2026-11-21T00:00:00Z` | Due, grace ends `2026-11-22T00:00:00Z`                                              |

  @AC-013 @FR-010 @FR-016 @US2-AS2 @US2-AS3
  Scenario Outline: An unrecorded commitment is Pending for 48 hours after its end and then Missed by itself
    Given the registered user `f5.ac013.user.<n>@example.com`, who created the custom habit `Reading`
    And `f5.ac013.user.<n>@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    And no other use case is called for the goal after it was created
    When at <time> `f5.ac013.user.<n>@example.com` reads the history of the goal from 2026-11-18 to 2026-11-18
    Then it gives the entry 2026-11-18, no record, <entry>

    Examples:
      | n  | time                   | entry                                                     |
      | 01 | `2026-11-19T00:00:00Z` | Pending, grace ends `2026-11-21T00:00:00Z`, no correction |
      | 02 | `2026-11-20T23:59:59Z` | Pending, grace ends `2026-11-21T00:00:00Z`, no correction |
      | 03 | `2026-11-21T00:00:00Z` | Missed, no grace end, correction                          |
      | 04 | `2026-11-21T00:00:01Z` | Missed, no grace end, correction                          |

  @AC-014 @FR-004 @FR-014 @US2-AS4
  Scenario Outline: A final Build duration commitment is Met at or above its target and Failed below it
    Given the registered user `f5.ac014.user.<n>@example.com`, who created the custom habit `Reading`
    And `f5.ac014.user.<n>@example.com` created a goal on their custom habit `Reading` with Build, <duration>, fixed days on all seven days and the start date 2026-11-18
    And `f5.ac014.user.<n>@example.com` recorded <value> for 2026-11-18
    When at `2026-11-21T00:00:00Z` `f5.ac014.user.<n>@example.com` reads the history of the goal from 2026-11-18 to 2026-11-18
    Then it gives the entry 2026-11-18, <state>, records (2026-11-18, <value>), <progress>

    Examples:
      | n  | duration            | value | state  | progress                  |
      | 01 | duration 20 minutes | 19    | Failed | progress 19 of 20 minutes |
      | 02 | duration 20 minutes | 20    | Met    | progress 20 of 20 minutes |
      | 03 | duration 20 minutes | 21    | Met    | progress 21 of 20 minutes |
      | 04 | duration 20 minutes | 45    | Met    | progress 45 of 20 minutes |
      | 05 | duration 20 minutes | 0     | Failed | progress 0 of 20 minutes  |
      | 06 | duration 1 in hours | 59    | Failed | progress 59 of 60 minutes |
      | 07 | duration 1 in hours | 60    | Met    | progress 60 of 60 minutes |
      | 08 | duration 1 in hours | 61    | Met    | progress 61 of 60 minutes |

  @AC-015 @FR-014 @US2-AS5
  Scenario Outline: A final Reduce count commitment is Met at or below its limit and Failed above it
    Given the registered user `f5.ac015.user.<n>@example.com`, who created the custom habit `Coffee`
    And `f5.ac015.user.<n>@example.com` created a goal on their custom habit `Coffee` with Reduce, <count>, fixed days on all seven days and the start date 2026-11-18
    And `f5.ac015.user.<n>@example.com` recorded <value> for 2026-11-18
    When at `2026-11-21T00:00:00Z` `f5.ac015.user.<n>@example.com` reads the history of the goal from 2026-11-18 to 2026-11-18
    Then it gives the entry 2026-11-18, <state>, records (2026-11-18, <value>)

    Examples:
      | n  | count                | value | state  |
      | 01 | count 2 `cups`       | 0     | Met    |
      | 02 | count 2 `cups`       | 1     | Met    |
      | 03 | count 2 `cups`       | 2     | Met    |
      | 04 | count 2 `cups`       | 3     | Failed |
      | 05 | count 0 `cigarettes` | 0     | Met    |
      | 06 | count 0 `cigarettes` | 1     | Failed |

  @AC-016 @FR-014 @US2-AS6
  Scenario Outline: A final yes/no commitment is Met on Yes for Build and on No for Reduce
    Given the registered user `f5.ac016.user.<n>@example.com`, who created the custom habit `Practice`
    And `f5.ac016.user.<n>@example.com` created a goal on their custom habit `Practice` with <intent>, yes/no, fixed days on all seven days and the start date 2026-11-18
    And `f5.ac016.user.<n>@example.com` recorded <value> for 2026-11-18
    When at `2026-11-21T00:00:00Z` `f5.ac016.user.<n>@example.com` reads the history of the goal from 2026-11-18 to 2026-11-18
    Then it gives the entry 2026-11-18, <state>, records (2026-11-18, <value>)

    Examples:
      | n  | intent | value | state  |
      | 01 | Build  | Yes   | Met    |
      | 02 | Build  | No    | Failed |
      | 03 | Reduce | No    | Met    |
      | 04 | Reduce | Yes   | Failed |

  @AC-017 @FR-014 @US2-AS7
  Scenario Outline: A final per-week Build yes/no period is Met with at least N Yes records
    Given the registered user `f5.ac017.user.<n>@example.com`, who created the custom habit `Yoga`
    And at `2026-11-16T08:00:00Z` `f5.ac017.user.<n>@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, 3 times per week and the start date 2026-11-16
    And at `2026-11-23T10:00:00Z` `f5.ac017.user.<n>@example.com` recorded <records>
    When at `2026-11-25T00:00:00Z` `f5.ac017.user.<n>@example.com` reads the history of the goal from 2026-11-16 to 2026-11-22
    Then it gives the entry 2026-11-16 to 2026-11-22, <state>, <progress>

    Examples:
      | n  | records                                                                            | state  | progress              |
      | 01 | Yes for 2026-11-16, Yes for 2026-11-18 and Yes for 2026-11-20                      | Met    | progress 3 of 3 times |
      | 02 | Yes for 2026-11-16, Yes for 2026-11-18 and No for 2026-11-20                       | Failed | progress 2 of 3 times |
      | 03 | Yes for 2026-11-16, Yes for 2026-11-17, Yes for 2026-11-18 and Yes for 2026-11-19 | Met    | progress 4 of 3 times |
      | 04 | Yes for 2026-11-16 and Yes for 2026-11-18                                          | Failed | progress 2 of 3 times |

  @AC-018 @FR-014 @US2-AS8
  Scenario Outline: A final per-week Build count period counts only the records that reach the amount
    Given the registered user `f5.ac018.user.<n>@example.com`, who created the custom habit `Reading`
    And at `2026-11-16T08:00:00Z` `f5.ac018.user.<n>@example.com` created a goal on their custom habit `Reading` with Build, count 20 `pages`, 3 times per week and the start date 2026-11-16
    And at `2026-11-23T10:00:00Z` `f5.ac018.user.<n>@example.com` recorded <records>
    When at `2026-11-25T00:00:00Z` `f5.ac018.user.<n>@example.com` reads the history of the goal from 2026-11-16 to 2026-11-22
    Then it gives the entry 2026-11-16 to 2026-11-22, <state>, <progress>

    Examples:
      | n  | records                                                                        | state  | progress              |
      | 01 | 20 for 2026-11-16, 25 for 2026-11-17 and 12 for 2026-11-18                     | Failed | progress 2 of 3 times |
      | 02 | 20 for 2026-11-16, 25 for 2026-11-17, 12 for 2026-11-18 and 30 for 2026-11-19 | Met    | progress 3 of 3 times |
      | 03 | 19 for 2026-11-16, 19 for 2026-11-17 and 19 for 2026-11-18                     | Failed | progress 0 of 3 times |
      | 04 | 20 for 2026-11-16, 20 for 2026-11-17 and 20 for 2026-11-18                     | Met    | progress 3 of 3 times |
      | 05 | 60 for 2026-11-16                                                              | Failed | progress 1 of 3 times |

  @AC-019 @FR-014 @US2-AS9
  Scenario Outline: A final per-day Reduce count period is Met when its total is within the limit
    Given the registered user `f5.ac019.user.<n>@example.com`, who created the custom habit `Sugar`
    And `f5.ac019.user.<n>@example.com` created a goal on their custom habit `Sugar` with Reduce, count 1 `sweet`, 1 time per day and the start date 2026-11-18
    And `f5.ac019.user.<n>@example.com` recorded <records>
    When at `2026-11-21T00:00:00Z` `f5.ac019.user.<n>@example.com` reads the history of the goal from 2026-11-18 to 2026-11-18
    Then it gives the entry 2026-11-18, <state>, <progress>

    Examples:
      | n  | records                               | state  | progress                      |
      | 01 | 0 for 2026-11-18 and 1 for 2026-11-18 | Met    | progress total 1 of at most 1 |
      | 02 | 1 for 2026-11-18 and 1 for 2026-11-18 | Failed | progress total 2 of at most 1 |
      | 03 | 0 for 2026-11-18                      | Met    | progress total 0 of at most 1 |
      | 04 | 2 for 2026-11-18                      | Failed | progress total 2 of at most 1 |

  @AC-020 @FR-014 @US2-AS10
  Scenario Outline: A final per-day yes/no period counts its Yes records
    Given the registered user `f5.ac020.user.<n>@example.com`, who created the custom habit `Practice`
    And `f5.ac020.user.<n>@example.com` created a goal on their custom habit `Practice` with <settings> and the start date 2026-11-18
    And `f5.ac020.user.<n>@example.com` recorded <records>
    When at `2026-11-21T00:00:00Z` `f5.ac020.user.<n>@example.com` reads the history of the goal from 2026-11-18 to 2026-11-18
    Then it gives the entry 2026-11-18, <state>, <progress>

    Examples:
      | n  | settings                       | records                                   | state  | progress                |
      | 01 | Reduce, yes/no, 1 time per day | Yes for 2026-11-18                        | Met    | progress 1 of at most 1 |
      | 02 | Reduce, yes/no, 1 time per day | Yes for 2026-11-18 and Yes for 2026-11-18 | Failed | progress 2 of at most 1 |
      | 03 | Reduce, yes/no, 1 time per day | No for 2026-11-18                         | Met    | progress 0 of at most 1 |
      | 04 | Build, yes/no, 2 times per day | Yes for 2026-11-18 and Yes for 2026-11-18 | Met    | progress 2 of 2 times   |
      | 05 | Build, yes/no, 2 times per day | Yes for 2026-11-18                        | Failed | progress 1 of 2 times   |
      | 06 | Build, yes/no, 2 times per day | Yes for 2026-11-18 and No for 2026-11-18  | Failed | progress 1 of 2 times   |
      | 07 | Build, yes/no, 2 times per day | Yes, Yes and Yes for 2026-11-18           | Met    | progress 3 of 2 times   |

  @AC-021 @FR-012 @US2-AS11
  Scenario Outline: An ended commitment or period stays Pending during its grace period, even when its records reach the target
    Given the registered user `f5.ac021.user.<n>@example.com`, who created the custom habit `Reading`
    And `f5.ac021.user.<n>@example.com` created a goal on their custom habit `Reading` with Build, <settings> and the start date 2026-11-18
    And `f5.ac021.user.<n>@example.com` recorded <value> for 2026-11-18
    When at <time> `f5.ac021.user.<n>@example.com` reads the history of the goal from 2026-11-18 to 2026-11-18
    Then it gives the entry 2026-11-18, <entry>

    Examples:
      | n  | settings                                          | value | time                   | entry                                                                  |
      | 01 | duration 20 minutes, fixed days on all seven days | 20    | `2026-11-19T00:00:00Z` | Pending, grace ends `2026-11-21T00:00:00Z`, progress 20 of 20 minutes |
      | 02 | duration 20 minutes, fixed days on all seven days | 20    | `2026-11-20T23:59:59Z` | Pending, grace ends `2026-11-21T00:00:00Z`, progress 20 of 20 minutes |
      | 03 | duration 20 minutes, fixed days on all seven days | 20    | `2026-11-21T00:00:00Z` | Met, no grace end, progress 20 of 20 minutes                           |
      | 04 | duration 20 minutes, fixed days on all seven days | 12    | `2026-11-20T23:59:59Z` | Pending, grace ends `2026-11-21T00:00:00Z`, progress 12 of 20 minutes |
      | 05 | duration 20 minutes, fixed days on all seven days | 12    | `2026-11-21T00:00:00Z` | Failed, no grace end, progress 12 of 20 minutes                        |
      | 06 | yes/no, 1 time per day                            | Yes   | `2026-11-20T23:59:59Z` | Pending, grace ends `2026-11-21T00:00:00Z`, progress 1 of 1 times     |
      | 07 | yes/no, 1 time per day                            | Yes   | `2026-11-21T00:00:00Z` | Met, no grace end, progress 1 of 1 times                               |

  @AC-022 @FR-013 @US2-AS12
  Scenario Outline: A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit
    Given the registered user `f5.ac022.user.<n>@example.com`, who created the custom habit `Sugar`
    And `f5.ac022.user.<n>@example.com` created a goal on their custom habit `Sugar` with Reduce, <settings> and the start date 2026-11-18
    And `f5.ac022.user.<n>@example.com` recorded <records>
    When at `2026-11-21T00:00:00Z` `f5.ac022.user.<n>@example.com` reads the history of the goal from 2026-11-18 to 2026-11-18
    Then it gives the entry 2026-11-18, <state>

    Examples:
      | n  | settings                                       | records           | state             |
      | 01 | yes/no, fixed days on all seven days           | nothing           | Missed, no record |
      | 02 | yes/no, 1 time per day                         | nothing           | Missed, no record |
      | 03 | count 1 `sweet`, 1 time per day                | nothing           | Missed, no record |
      | 04 | duration 30 minutes, 1 time per day            | nothing           | Missed, no record |
      | 05 | count 0 `sweets`, fixed days on all seven days | nothing           | Missed, no record |
      | 06 | yes/no, fixed days on all seven days           | No for 2026-11-18 | Met               |
      | 07 | yes/no, 1 time per day                         | No for 2026-11-18 | Met               |
      | 08 | count 1 `sweet`, 1 time per day                | 0 for 2026-11-18  | Met               |
      | 09 | count 0 `sweets`, fixed days on all seven days | 0 for 2026-11-18  | Met               |

  @AC-023 @FR-016 @US2-AS13
  Scenario Outline: Around a clock change the grace period lasts exactly 48 elapsed hours
    Given the registered user `f5.ac023.user.<n>@example.com` in `Europe/Kyiv`, who created the custom habit `Reading`
    And at <created at> `f5.ac023.user.<n>@example.com` created a goal on their custom habit `Reading` with Build, yes/no, fixed days on all seven days and the start date <date>
    When at <time> `f5.ac023.user.<n>@example.com` reads the history of the goal from <date> to <date>
    Then it gives the entry <date> from <start> to <end>, no record, <entry>

    Examples:
      | n  | created at             | date       | time                   | start                  | end                    | entry                                                                                     |
      | 01 | `2026-10-24T08:00:00Z` | 2026-10-24 | `2026-10-26T20:59:59Z` | `2026-10-23T21:00:00Z` | `2026-10-24T21:00:00Z` | Pending, grace ends `2026-10-26T21:00:00Z` (23:00 in Kyiv on Monday 26 October 2026)    |
      | 02 | `2026-10-24T08:00:00Z` | 2026-10-24 | `2026-10-26T21:00:00Z` | `2026-10-23T21:00:00Z` | `2026-10-24T21:00:00Z` | Missed, no grace end                                                                      |
      | 03 | `2027-03-27T08:00:00Z` | 2027-03-27 | `2027-03-29T21:59:59Z` | `2027-03-26T22:00:00Z` | `2027-03-27T22:00:00Z` | Pending, grace ends `2027-03-29T22:00:00Z` (01:00 in Kyiv on Tuesday 30 March 2027)     |
      | 04 | `2027-03-27T08:00:00Z` | 2027-03-27 | `2027-03-29T22:00:00Z` | `2027-03-26T22:00:00Z` | `2027-03-27T22:00:00Z` | Missed, no grace end                                                                      |

  @AC-024 @FR-006 @US3-AS1
  Scenario: A record added to a Missed commitment is a correction that makes it Met
    Given the registered user `f5.ac024.user@example.com`, who created the custom habit `Reading`
    And `f5.ac024.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    When at `2026-11-22T10:00:00Z` `f5.ac024.user@example.com` records 20 for 2026-11-18
    Then recording succeeds with the state Met, a correction
    And reading the history of the goal from 2026-11-18 to 2026-11-18 gives the entry 2026-11-18, Met, records (2026-11-18, 20), correction

  @AC-025 @FR-006 @US3-AS2
  Scenario Outline: Lowering the record of a Met commitment below its target is a correction that makes it Failed
    Given the registered user `f5.ac025.user.<n>@example.com`, who created the custom habit `Reading`
    And `f5.ac025.user.<n>@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    And `f5.ac025.user.<n>@example.com` recorded 20 for 2026-11-18
    When at `2026-11-22T10:00:00Z` `f5.ac025.user.<n>@example.com` <action>
    Then <result> succeeds with the state Failed, a correction
    And reading the history of the goal from 2026-11-18 to 2026-11-18 gives the entry 2026-11-18, Failed, records (2026-11-18, 12), progress 12 of 20 minutes

    Examples:
      | n  | action                                 | result    |
      | 01 | changes the record of 2026-11-18 to 12 | changing  |
      | 02 | records 12 for 2026-11-18              | recording |

  @AC-026 @FR-006 @US3-AS3
  Scenario: Deleting the only record of a final commitment is a correction that makes it Missed
    Given the registered user `f5.ac026.user@example.com`, who created the custom habit `Reading`
    And `f5.ac026.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    And `f5.ac026.user@example.com` recorded 20 for 2026-11-18
    When at `2026-11-22T10:00:00Z` `f5.ac026.user@example.com` deletes the record of 2026-11-18
    Then deleting succeeds with the state Missed, a correction
    And reading the history of the goal from 2026-11-18 to 2026-11-18 gives the entry 2026-11-18, Missed, no record

  @AC-027 @FR-006
  Scenario Outline: A change is a correction only from the instant its grace period ends
    Given the registered user `f5.ac027.user.<n>@example.com`, who created the custom habit `Reading`
    And `f5.ac027.user.<n>@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    And `f5.ac027.user.<n>@example.com` recorded <before>
    When at <time> `f5.ac027.user.<n>@example.com` <action>
    Then <result>

    Examples:
      | n  | before            | time                   | action                                 | result                                                                                                                                                         |
      | 01 | nothing           | `2026-11-20T23:59:59Z` | records 20 for 2026-11-18              | recording succeeds with the state Pending, not a correction                                                                                                    |
      | 02 | nothing           | `2026-11-21T00:00:00Z` | records 20 for 2026-11-18              | recording succeeds with the state Met, a correction                                                                                                            |
      | 03 | 12 for 2026-11-18 | `2026-11-18T12:00:00Z` | deletes the record of 2026-11-18       | deleting succeeds with the state Due, not a correction, and the history from 2026-11-18 to 2026-11-18 gives the entry 2026-11-18, Due, no record               |
      | 04 | 12 for 2026-11-18 | `2026-11-20T23:59:59Z` | deletes the record of 2026-11-18       | deleting succeeds with the state Pending, not a correction, and the history from 2026-11-18 to 2026-11-18 gives the entry 2026-11-18, Pending, no record       |
      | 05 | 12 for 2026-11-18 | `2026-11-20T23:59:59Z` | changes the record of 2026-11-18 to 20 | changing succeeds with the state Pending, not a correction                                                                                                     |
      | 06 | 12 for 2026-11-18 | `2026-11-21T00:00:00Z` | changes the record of 2026-11-18 to 20 | changing succeeds with the state Met, a correction                                                                                                             |

  @AC-028 @FR-006 @FR-014 @US3-AS4
  Scenario: A missing record added to a Failed week is a correction that makes it Met
    Given the registered user `f5.ac028.user@example.com`, who created the custom habit `Yoga`
    And at `2026-11-16T08:00:00Z` `f5.ac028.user@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, 3 times per week and the start date 2026-11-16
    And at `2026-11-18T10:00:00Z` `f5.ac028.user@example.com` recorded Yes for 2026-11-16 and Yes for 2026-11-18
    When at `2026-11-26T10:00:00Z` `f5.ac028.user@example.com` records Yes for 2026-11-20
    Then recording succeeds with the state Met, a correction
    And reading the history of the goal from 2026-11-16 to 2026-11-22 gives the entry 2026-11-16 to 2026-11-22, Met, records (2026-11-16, Yes), (2026-11-18, Yes), (2026-11-20, Yes), progress 3 of 3 times

  @AC-029 @FR-006 @FR-015 @US3-AS5
  Scenario: Results corrected after the grace period equal the same records entered on time
    Given the registered user `f5.ac029.user@example.com`, who created the custom habit `Reading`
    And `f5.ac029.user@example.com` created two goals on their custom habit `Reading`, each with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    And at `2026-11-18T10:00:00Z` `f5.ac029.user@example.com` recorded 20 for 2026-11-18 on the first goal and 5 for 2026-11-18 on the second goal
    And at `2026-11-19T10:00:00Z` `f5.ac029.user@example.com` recorded 12 for 2026-11-19 on the first goal and 30 for 2026-11-19 on the second goal
    And at `2026-11-20T10:00:00Z` `f5.ac029.user@example.com` recorded 20 for 2026-11-20 on the second goal
    When at `2026-11-25T10:00:00Z` `f5.ac029.user@example.com` changes the record of 2026-11-18 of the second goal to 20, changes its record of 2026-11-19 to 12 and deletes its record of 2026-11-20
    Then the three changes succeed with the states Met, Failed and Missed, each a correction
    And reading the history from 2026-11-18 to 2026-11-20 gives, for the first goal and for the second goal alike: 2026-11-18, Met, records (2026-11-18, 20), progress 20 of 20 minutes; 2026-11-19, Failed, records (2026-11-19, 12), progress 12 of 20 minutes; 2026-11-20, Missed, no record

  @AC-030 @FR-015 @FR-016
  Scenario: After a time zone change, each commitment keeps the instants and grace period of its own settings version
    Given the registered user `f5.ac030.user@example.com` in `Europe/Kyiv`, who created the custom habit `Reading`
    And at `2026-11-16T08:00:00Z` `f5.ac030.user@example.com` created a goal on their custom habit `Reading` with Build, yes/no, fixed days on all seven days and the start date 2026-11-16
    And at `2026-11-18T10:00:00Z` `f5.ac030.user@example.com` changed their time zone to `America/New_York`, and the change succeeded
    When at `2026-11-19T21:59:59Z` `f5.ac030.user@example.com` reads the history of the goal from 2026-11-17 to 2026-11-20
    Then it gives 4 entries, in this order:
    And 2026-11-17 from `2026-11-16T22:00:00Z` to `2026-11-17T22:00:00Z`, Pending, grace ends `2026-11-19T22:00:00Z`
    And 2026-11-18 from `2026-11-17T22:00:00Z` to `2026-11-18T22:00:00Z`, Pending, grace ends `2026-11-20T22:00:00Z`
    And 2026-11-19 from `2026-11-19T05:00:00Z` to `2026-11-20T05:00:00Z`, Due, grace ends `2026-11-22T05:00:00Z`
    And 2026-11-20 from `2026-11-20T05:00:00Z` to `2026-11-21T05:00:00Z`, Upcoming, no grace end

  @AC-031 @FR-006 @FR-015 @US3-AS6
  Scenario Outline: A commitment in the old time zone becomes final, and is corrected, at the end of its own grace period
    Given the registered user `f5.ac031.user.<n>@example.com` in `Europe/Kyiv`, who created the custom habit `Reading`
    And at `2026-11-16T08:00:00Z` `f5.ac031.user.<n>@example.com` created a goal on their custom habit `Reading` with Build, yes/no, fixed days on all seven days and the start date 2026-11-16
    And at `2026-11-18T10:00:00Z` `f5.ac031.user.<n>@example.com` changed their time zone to `America/New_York`, and the change succeeded
    When at <time> `f5.ac031.user.<n>@example.com` records Yes for 2026-11-17
    Then recording succeeds with the state <state>, <correction>
    And reading the history of the goal from 2026-11-17 to 2026-11-17 gives the entry 2026-11-17 from `2026-11-16T22:00:00Z` to `2026-11-17T22:00:00Z`, <state>, records (2026-11-17, Yes)

    Examples:
      | n  | time                   | state   | correction       |
      | 01 | `2026-11-19T21:59:59Z` | Pending | not a correction |
      | 02 | `2026-11-19T22:00:00Z` | Met     | a correction     |

  @AC-032 @FR-005 @FR-017 @US3-AS7
  Scenario Outline: A record identifier that is not a record of the goal named in the call does not exist
    Given the registered user `f5.ac032.user.<n>@example.com`, who created the custom habit `Reading`
    And `f5.ac032.user.<n>@example.com` created two goals on their custom habit `Reading`, each with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    And `f5.ac032.user.<n>@example.com` recorded 20 for 2026-11-18 on the first goal and 30 for 2026-11-18 on the second goal
    When `f5.ac032.user.<n>@example.com` <action> on the first goal
    Then the result is "This record does not exist."
    And reading the history from 2026-11-18 to 2026-11-18 gives records (2026-11-18, 20) for the first goal and records (2026-11-18, 30) for the second goal

    Examples:
      | n  | action                                                      |
      | 01 | changes the second goal's record of 2026-11-18 to 12        |
      | 02 | deletes the second goal's record of 2026-11-18              |
      | 03 | changes a record identifier that belongs to no record to 12 |
      | 04 | deletes a record identifier that belongs to no record       |

  @AC-033 @FR-003 @FR-005
  Scenario Outline: An evaluation period takes at most 50 records, but its records can still be changed or deleted
    Given the registered user `f5.ac033.user.<n>@example.com`, who created the custom habit `Water`
    And `f5.ac033.user.<n>@example.com` created a goal on their custom habit `Water` with Build, yes/no, 1 time per day and the start date 2026-11-18
    And `f5.ac033.user.<n>@example.com` recorded Yes for 2026-11-18 50 times
    When `f5.ac033.user.<n>@example.com` <action>
    Then <result>

    Examples:
      | n  | action                                                             | result                                                                                                                                                   |
      | 01 | records Yes for 2026-11-18                                         | the result is "This evaluation period already has 50 records." and the history from 2026-11-18 to 2026-11-18 gives 50 records                            |
      | 02 | records Yes for 2026-11-18 with the note a text of 501 characters  | the result is "The note can have at most 500 characters." and the history from 2026-11-18 to 2026-11-18 gives 50 records and no note                     |
      | 03 | records no value for 2026-11-18                                    | the result is "Choose Yes or No." and the history from 2026-11-18 to 2026-11-18 gives 50 records                                                         |
      | 04 | changes the first record of 2026-11-18 to No                       | changing succeeds with the state Due, not a correction, and the history from 2026-11-18 to 2026-11-18 gives 50 records, exactly one of them (2026-11-18, No) |
      | 05 | deletes the first record of 2026-11-18 and then records Yes for 2026-11-18 | deleting and recording both succeed with the state Due, not a correction, and the history from 2026-11-18 to 2026-11-18 gives 50 records             |

  @AC-034 @FR-001 @FR-009
  Scenario Outline: A record belongs to the week of its date, not to the week of the instant it is saved
    Given the registered user `f5.ac034.user.<n>@example.com`, who created the custom habit `Yoga`
    And at `2026-11-16T08:00:00Z` `f5.ac034.user.<n>@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, 3 times per week and the start date 2026-11-16
    When at <time> `f5.ac034.user.<n>@example.com` records Yes for <date>
    Then recording succeeds with the state <state>, not a correction
    And reading the history of the goal from 2026-11-16 to 2026-11-29 gives 2026-11-16 to 2026-11-22 with <first week> and 2026-11-23 to 2026-11-29 with <second week>

    Examples:
      | n  | time                   | date       | state   | first week                | second week               |
      | 01 | `2026-11-22T23:59:59Z` | 2026-11-22 | Due     | records (2026-11-22, Yes) | no record                 |
      | 02 | `2026-11-23T00:00:00Z` | 2026-11-22 | Pending | records (2026-11-22, Yes) | no record                 |
      | 03 | `2026-11-23T00:00:00Z` | 2026-11-23 | Due     | no record                 | records (2026-11-23, Yes) |

  @AC-035 @FR-007 @US4-AS1
  Scenario: A note can be added to a Missed commitment and leaves it Missed
    Given the registered user `f5.ac035.user@example.com`, who created the custom habit `Reading`
    And `f5.ac035.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    When at `2026-11-22T10:00:00Z` `f5.ac035.user@example.com` saves the note "Away for the weekend." for 2026-11-18
    Then saving the note succeeds
    And reading the history of the goal from 2026-11-18 to 2026-11-18 gives the entry 2026-11-18, Missed, no record, note "Away for the weekend."

  @AC-036 @FR-007 @US4-AS2
  Scenario Outline: A note saved on its own replaces the note, and an empty one removes it
    Given the registered user `f5.ac036.user.<n>@example.com`, who created the custom habit `Reading`
    And `f5.ac036.user.<n>@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    And `f5.ac036.user.<n>@example.com` saved the note "Back was tired." for 2026-11-18
    When `f5.ac036.user.<n>@example.com` saves the note <note> for 2026-11-18
    Then saving the note succeeds
    And reading the history of the goal from 2026-11-18 to 2026-11-18 gives the entry 2026-11-18 with <result>

    Examples:
      | n  | note            | result              |
      | 01 | `""`            | no note             |
      | 02 | `"   "`         | no note             |
      | 03 | "New note."     | the note "New note." |
      | 04 | "  New note.  " | the note "New note." |

  @AC-037 @FR-007 @US4-AS3
  Scenario Outline: A note given with a record is the note; no note leaves it unchanged
    Given the registered user `f5.ac037.user.<n>@example.com`, who created the custom habit `Yoga`
    And at `2026-11-16T08:00:00Z` `f5.ac037.user.<n>@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, 3 times per week and the start date 2026-11-16
    And at `2026-11-18T10:00:00Z` `f5.ac037.user.<n>@example.com` recorded Yes for 2026-11-16 and saved the note "Knee was sore." for 2026-11-16
    When `f5.ac037.user.<n>@example.com` <action>
    Then it succeeds with the state Due, not a correction
    And reading the history of the goal from 2026-11-16 to 2026-11-22 gives the entry 2026-11-16 to 2026-11-22 with <note>

    Examples:
      | n  | action                                                                 | note                     |
      | 01 | records Yes for 2026-11-18                                             | the note "Knee was sore." |
      | 02 | records Yes for 2026-11-18 with the note `"   "`                       | no note                  |
      | 03 | records Yes for 2026-11-18 with the note `""`                          | no note                  |
      | 04 | records Yes for 2026-11-18 with the note "Better today."               | the note "Better today."  |
      | 05 | changes the record of 2026-11-16 to No                                 | the note "Knee was sore." |
      | 06 | changes the record of 2026-11-16 to No with the note "Better today."   | the note "Better today."  |
      | 07 | changes the record of 2026-11-16 to No with the note `""`              | no note                  |
      | 08 | changes the record of 2026-11-16 to No with the note "Knee was sore."  | the note "Knee was sore." |

  @AC-038 @FR-005 @FR-007 @US4-AS4
  Scenario Outline: A note has at most 500 characters
    Given the registered user `f5.ac038.user.<n>@example.com`, who created the custom habit `Reading`
    And `f5.ac038.user.<n>@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    And `f5.ac038.user.<n>@example.com` saved the note "Back was tired." for 2026-11-18
    When `f5.ac038.user.<n>@example.com` <action>
    Then <result>

    Examples:
      | n  | action                                                                  | result                                                                                                                                                                    |
      | 01 | saves the note a text of 501 characters for 2026-11-18                  | the result is "The note can have at most 500 characters." and the history from 2026-11-18 to 2026-11-18 gives the note "Back was tired."                                  |
      | 02 | saves the note a text of 500 characters for 2026-11-18                  | saving the note succeeds and the history from 2026-11-18 to 2026-11-18 gives a text of 500 characters as the note                                                        |
      | 03 | records 20 for 2026-11-18 with the note a text of 501 characters        | the result is "The note can have at most 500 characters." and the history from 2026-11-18 to 2026-11-18 gives no record and the note "Back was tired."                    |
      | 04 | records 20 for 2026-11-18 with the note a text of 500 characters        | recording succeeds with the state Due, not a correction, and the history from 2026-11-18 to 2026-11-18 gives records (2026-11-18, 20) and a text of 500 characters as the note |
      | 05 | saves the note a padded text of 500 characters for 2026-11-18           | saving the note succeeds and the history from 2026-11-18 to 2026-11-18 gives a text of 500 characters as the note                                                        |
      | 06 | records 20 for 2026-11-18 with the note a padded text of 500 characters | recording succeeds with the state Due, not a correction, and the history from 2026-11-18 to 2026-11-18 gives a text of 500 characters as the note                        |

  @AC-039 @FR-002 @FR-007 @US4-AS5
  Scenario Outline: Any begun date of a week names that week's one note
    Given the registered user `f5.ac039.user.<n>@example.com`, who created the custom habit `Yoga`
    And at `2026-11-16T08:00:00Z` `f5.ac039.user.<n>@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, 3 times per week and the start date 2026-11-16
    When at `2026-11-18T10:00:00Z` `f5.ac039.user.<n>@example.com` <action>
    Then <result>

    Examples:
      | n  | action                                                                                      | result                                                                                                                                                       |
      | 01 | saves the note "Week note." for 2026-11-18                                                  | saving the note succeeds and the history from 2026-11-16 to 2026-11-22 gives the entry 2026-11-16 to 2026-11-22 with the note "Week note."                   |
      | 02 | saves the note "Week note." for 2026-11-16 and then the note "Second note." for 2026-11-17 | both succeed and the history from 2026-11-16 to 2026-11-22 gives the entry 2026-11-16 to 2026-11-22 with the note "Second note."                             |
      | 03 | saves the note "Week note." for 2026-11-20                                                  | the result is "You cannot record a day that has not begun." and the history from 2026-11-16 to 2026-11-22 gives no note                                     |
      | 04 | saves the note "Week note." for 2026-11-15                                                  | the result is "Nothing is scheduled on this date." and the history from 2026-11-16 to 2026-11-22 gives no note                                              |
      | 05 | saves the note a text of 501 characters for 2026-11-20                                      | the result is "You cannot record a day that has not begun." (the date is checked before the note) and the history from 2026-11-16 to 2026-11-22 gives no note |
      | 06 | saves the note a text of 501 characters for 2026-11-15                                      | the result is "Nothing is scheduled on this date." (the date is checked before the note) and the history from 2026-11-16 to 2026-11-22 gives no note        |

  @AC-040 @FR-008 @US4-AS6
  Scenario: The recent notes are the 5 with the latest dates, newest first, each with its state
    Given the registered user `f5.ac040.user@example.com`, who created the custom habit `Reading`
    And at `2026-11-16T08:00:00Z` `f5.ac040.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-16
    And at `2026-11-18T10:00:00Z` `f5.ac040.user@example.com` recorded 12 for 2026-11-18
    And at `2026-11-19T10:00:00Z` `f5.ac040.user@example.com` recorded 20 for 2026-11-19
    And at `2026-11-22T10:00:00Z` `f5.ac040.user@example.com` saved, in this order, the note "Note 22." for 2026-11-22, "Note 16." for 2026-11-16, "Note 20." for 2026-11-20, "Note 18." for 2026-11-18, "Note 17." for 2026-11-17, "Note 21." for 2026-11-21 and "Note 19." for 2026-11-19
    When at `2026-11-22T10:00:00Z` `f5.ac040.user@example.com` reads the recent notes of the goal
    Then it gives exactly 5 notes, in this order:
    And 2026-11-22, "Note 22.", Due
    And 2026-11-21, "Note 21.", Pending
    And 2026-11-20, "Note 20.", Pending
    And 2026-11-19, "Note 19.", Met
    And 2026-11-18, "Note 18.", Failed

  @AC-041 @FR-008
  Scenario: A per-week note gives its week's first and last date, and a goal with fewer notes gives only those
    Given the registered user `f5.ac041.user@example.com`, who created the custom habit `Yoga`
    And at `2026-11-16T08:00:00Z` `f5.ac041.user@example.com` created a goal on their custom habit `Yoga` with Build, yes/no, 3 times per week and the start date 2026-11-16, and then a second goal on their custom habit `Yoga` with Build, yes/no, 2 times per week and the start date 2026-11-16
    And at `2026-11-23T10:00:00Z` `f5.ac041.user@example.com` saved the note "Week one." for 2026-11-18 and the note "Week two." for 2026-11-23 on the first goal
    When at `2026-11-23T10:00:00Z` `f5.ac041.user@example.com` reads the recent notes of the first goal and of the second goal
    Then the first goal gives exactly 2 notes, in this order: 2026-11-23 to 2026-11-29, "Week two.", Due; and 2026-11-16 to 2026-11-22, "Week one.", Pending
    And the second goal gives no note

  @AC-042 @FR-009 @FR-010 @US5-AS1
  Scenario: The history gives every commitment of the range in date order, each with its state at the supplied time
    Given the registered user `f5.ac042.user@example.com`, who created the custom habit `Reading`
    And at `2026-11-16T08:00:00Z` `f5.ac042.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-16
    And at `2026-11-16T10:00:00Z` `f5.ac042.user@example.com` recorded 20 for 2026-11-16
    And at `2026-11-17T10:00:00Z` `f5.ac042.user@example.com` recorded 12 for 2026-11-17
    And at `2026-11-18T10:00:00Z` `f5.ac042.user@example.com` saved the note "Away." for 2026-11-18
    And at `2026-11-19T10:00:00Z` `f5.ac042.user@example.com` recorded 25 for 2026-11-19
    When at `2026-11-21T10:00:00Z` `f5.ac042.user@example.com` reads the history of the goal from 2026-11-16 to 2026-11-29
    Then it gives 14 entries, one for each date from 2026-11-16 to 2026-11-29, in date order, each from 00:00:00Z of its date to 00:00:00Z of the next date
    And 2026-11-16, Met, no grace end, records (2026-11-16, 20), progress 20 of 20 minutes, no note, correction
    And 2026-11-17, Failed, no grace end, records (2026-11-17, 12), progress 12 of 20 minutes, no note, correction
    And 2026-11-18, Missed, no grace end, no record, note "Away.", correction
    And 2026-11-19, Pending, grace ends `2026-11-22T00:00:00Z`, records (2026-11-19, 25), progress 25 of 20 minutes, no note, no correction
    And 2026-11-20, Pending, grace ends `2026-11-23T00:00:00Z`, no record, no note, no correction
    And 2026-11-21, Due, grace ends `2026-11-24T00:00:00Z`, no record, no note, no correction
    And each date from 2026-11-22 to 2026-11-29, Upcoming, no grace end, no record, no note, no correction

  @AC-043 @FR-009 @FR-018 @US5-AS4
  Scenario Outline: The history's range has 1 to 366 days and is checked after the goal
    Given the registered user `f5.ac043.owner.<n>@example.com`, who created the custom habit `Reading`
    And `f5.ac043.owner.<n>@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    And the registered user `f5.ac043.other.<n>@example.com`
    When <actor> reads the history of the goal of `f5.ac043.owner.<n>@example.com` from <first> to <last>
    Then <result>

    Examples:
      | n  | actor                            | first      | last       | result                                                             |
      | 01 | `f5.ac043.owner.<n>@example.com` | 2026-11-18 | 2026-11-18 | it gives exactly one entry, 2026-11-18                             |
      | 02 | `f5.ac043.owner.<n>@example.com` | 2026-11-16 | 2027-11-16 | it gives 364 entries, the first 2026-11-18 and the last 2027-11-16 |
      | 03 | `f5.ac043.owner.<n>@example.com` | 2026-11-16 | 2027-11-17 | the result is "Choose a range of 1 to 366 days."                   |
      | 04 | `f5.ac043.owner.<n>@example.com` | 2026-11-19 | 2026-11-18 | the result is "Choose a range of 1 to 366 days."                   |
      | 05 | `f5.ac043.other.<n>@example.com` | 2026-11-16 | 2027-11-17 | the result is "This goal does not exist."                          |
      | 06 | the anonymous visitor            | 2026-11-16 | 2027-11-17 | the result is "You are not signed in."                             |

  @AC-044 @FR-014 @US5-AS2
  Scenario Outline: Progress is given at any state, as the goal's kind of target counts it
    Given the registered user `f5.ac044.user.<n>@example.com`, who created the custom habit `Practice`
    And at `2026-11-16T08:00:00Z` `f5.ac044.user.<n>@example.com` created a goal on their custom habit `Practice` with <settings> and the start date 2026-11-16
    And at `2026-11-18T10:00:00Z` `f5.ac044.user.<n>@example.com` recorded <records>
    When at `2026-11-18T10:00:00Z` `f5.ac044.user.<n>@example.com` reads the history of the goal from 2026-11-16 to 2026-11-22
    Then its entry that contains 2026-11-18 is Due, <progress>

    Examples:
      | n  | settings                                                 | records                                                    | progress                        |
      | 01 | Build, duration 20 minutes, fixed days on all seven days | 12 for 2026-11-18                                          | progress 12 of 20 minutes       |
      | 02 | Build, count 20 `pages`, 3 times per week                | 20 for 2026-11-16, 25 for 2026-11-17 and 12 for 2026-11-18 | progress 2 of 3 times           |
      | 03 | Reduce, count 1 `sweet`, 1 time per day                  | 1 for 2026-11-18 and 1 for 2026-11-18                      | progress total 2 of at most 1   |
      | 04 | Reduce, yes/no, 1 time per day                           | Yes, Yes and No for 2026-11-18                             | progress 2 of at most 1         |
      | 05 | Build, duration 1 in hours, fixed days on all seven days | 45 for 2026-11-18                                          | progress 45 of 60 minutes       |
      | 06 | Build, yes/no, 2 times per day                           | Yes and No for 2026-11-18                                  | progress 1 of 2 times           |
      | 07 | Reduce, duration 30 minutes, 1 time per day              | 10 and 15 for 2026-11-18                                   | progress total 25 of at most 30 |

  @AC-045 @FR-011 @US5-AS3
  Scenario Outline: What can be recorded now is every Due and every Pending commitment or period, newest first
    Given the registered user `f5.ac045.user.<n>@example.com`, who created the custom habit `Practice`
    And at `2026-11-16T08:00:00Z` `f5.ac045.user.<n>@example.com` created a goal on their custom habit `Practice` with Build, yes/no, <schedule> and the start date <start date>
    When at <time> `f5.ac045.user.<n>@example.com` reads what can be recorded now for the goal
    Then it gives <entries>

    Examples:
      | n  | schedule                                | start date | time                   | entries                                                                                                                                                                                   |
      | 01 | fixed days on all seven days            | 2026-11-16 | `2026-11-19T10:00:00Z` | in this order: 2026-11-19, Due, grace ends `2026-11-22T00:00:00Z`; 2026-11-18, Pending, grace ends `2026-11-21T00:00:00Z`; 2026-11-17, Pending, grace ends `2026-11-20T00:00:00Z` |
      | 02 | 1 time per day                          | 2026-11-16 | `2026-11-19T10:00:00Z` | in this order: 2026-11-19, Due; 2026-11-18, Pending; 2026-11-17, Pending                                                                                                                  |
      | 03 | fixed days Monday, Wednesday and Friday | 2026-11-16 | `2026-11-19T10:00:00Z` | exactly one entry: 2026-11-18, Pending, grace ends `2026-11-21T00:00:00Z`                                                                                                                 |
      | 04 | fixed days Monday, Wednesday and Friday | 2026-11-16 | `2026-11-21T10:00:00Z` | exactly one entry: 2026-11-20, Pending, grace ends `2026-11-23T00:00:00Z`                                                                                                                 |
      | 05 | 3 times per week                        | 2026-11-16 | `2026-11-24T10:00:00Z` | in this order: 2026-11-23 to 2026-11-29, Due, grace ends `2026-12-02T00:00:00Z`; 2026-11-16 to 2026-11-22, Pending, grace ends `2026-11-25T00:00:00Z`                                    |
      | 06 | 3 times per week                        | 2026-11-16 | `2026-11-25T00:00:00Z` | exactly one entry: 2026-11-23 to 2026-11-29, Due                                                                                                                                          |
      | 07 | fixed days Friday                       | 2026-11-20 | `2026-11-18T10:00:00Z` | no entry                                                                                                                                                                                  |

  @AC-046 @FR-011 @US5-AS3
  Scenario Outline: After a move east, the old and the new day are both Due while they overlap
    Given the registered user `f5.ac046.user.<n>@example.com` in `America/New_York`, who created the custom habit `Practice`
    And at `2026-11-16T15:00:00Z` `f5.ac046.user.<n>@example.com` created a goal on their custom habit `Practice` with Build, yes/no, fixed days on all seven days and the start date 2026-11-16
    And at `2026-11-18T10:00:00Z` `f5.ac046.user.<n>@example.com` changed their time zone to `Europe/Kyiv`, and the change succeeded
    When at <time> `f5.ac046.user.<n>@example.com` reads what can be recorded now for the goal
    Then it gives, in this order, <entries>

    Examples:
      | n  | time                   | entries                                                                                                                                                                                                                                                                                                        |
      | 01 | `2026-11-18T23:00:00Z` | 2026-11-19 from `2026-11-18T22:00:00Z` to `2026-11-19T22:00:00Z`, Due, grace ends `2026-11-21T22:00:00Z`; 2026-11-18 from `2026-11-18T05:00:00Z` to `2026-11-19T05:00:00Z`, Due, grace ends `2026-11-21T05:00:00Z`; 2026-11-17, Pending, grace ends `2026-11-20T05:00:00Z`; 2026-11-16, Pending, grace ends `2026-11-19T05:00:00Z` |
      | 02 | `2026-11-19T05:00:00Z` | 2026-11-19, Due, grace ends `2026-11-21T22:00:00Z`; 2026-11-18, Pending, grace ends `2026-11-21T05:00:00Z`; 2026-11-17, Pending, grace ends `2026-11-20T05:00:00Z`                                                                                                                                             |

  @AC-047 @FR-017 @FR-018 @US6-AS1
  Scenario Outline: Another registered user gets "This goal does not exist." from every use case of this slice
    Given the registered user `f5.ac047.owner.<n>@example.com`, who created the custom habit `Reading`
    And `f5.ac047.owner.<n>@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    And `f5.ac047.owner.<n>@example.com` recorded 20 for 2026-11-18 with the note "Private note."
    And the registered user `f5.ac047.other.<n>@example.com`
    When `f5.ac047.other.<n>@example.com` calls <use case> with <goal>
    Then the result is "This goal does not exist." and it gives no record, note or state
    And reading the history of the goal from 2026-11-18 to 2026-11-18 as `f5.ac047.owner.<n>@example.com` gives records (2026-11-18, 20) and the note "Private note."

    Examples:
      | n  | use case                                   | goal                                         |
      | 01 | record                                     | the goal of `f5.ac047.owner.<n>@example.com` |
      | 02 | change, with the owner's record identifier | the goal of `f5.ac047.owner.<n>@example.com` |
      | 03 | delete, with the owner's record identifier | the goal of `f5.ac047.owner.<n>@example.com` |
      | 04 | save note                                  | the goal of `f5.ac047.owner.<n>@example.com` |
      | 05 | history                                    | the goal of `f5.ac047.owner.<n>@example.com` |
      | 06 | recordable now                             | the goal of `f5.ac047.owner.<n>@example.com` |
      | 07 | recent notes                               | the goal of `f5.ac047.owner.<n>@example.com` |
      | 08 | record                                     | a goal identifier that belongs to no goal    |
      | 09 | history                                    | a goal identifier that belongs to no goal    |

  @AC-048 @FR-017 @US6-AS2
  Scenario Outline: Another account's record identifier does not exist on one's own goal
    Given the registered user `f5.ac048.owner.<n>@example.com`, who created the custom habit `Reading`
    And `f5.ac048.owner.<n>@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    And `f5.ac048.owner.<n>@example.com` recorded 20 for 2026-11-18
    And the registered user `f5.ac048.other.<n>@example.com`, who created the custom habit `Reading`
    And `f5.ac048.other.<n>@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    And `f5.ac048.other.<n>@example.com` recorded 30 for 2026-11-18
    When `f5.ac048.other.<n>@example.com` <action> on their own goal, with the record identifier of `f5.ac048.owner.<n>@example.com`'s record of 2026-11-18
    Then the result is "This record does not exist."
    And the history from 2026-11-18 to 2026-11-18 gives records (2026-11-18, 20) for the goal of `f5.ac048.owner.<n>@example.com` and records (2026-11-18, 30) for the goal of `f5.ac048.other.<n>@example.com`

    Examples:
      | n  | action                  |
      | 01 | changes the record to 12 |
      | 02 | deletes the record      |

  @AC-049 @FR-018 @US6-AS3
  Scenario Outline: An administrator gets "Goals are for registered users." from every use case of this slice
    Given the registered user `f5.ac049.owner.<n>@example.com`, who created the custom habit `Reading`
    And `f5.ac049.owner.<n>@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    And `f5.ac049.owner.<n>@example.com` recorded 20 for 2026-11-18 with the note "Private note."
    And the administrator `f5.ac049.admin.<n>@example.com`
    When `f5.ac049.admin.<n>@example.com` calls <use case> with the goal of `f5.ac049.owner.<n>@example.com`
    Then the result is "Goals are for registered users." and it gives no record, note or state
    And reading the history of the goal from 2026-11-18 to 2026-11-18 as `f5.ac049.owner.<n>@example.com` gives records (2026-11-18, 20) and the note "Private note."

    Examples:
      | n  | use case                                   |
      | 01 | record                                     |
      | 02 | change, with the owner's record identifier |
      | 03 | delete, with the owner's record identifier |
      | 04 | save note                                  |
      | 05 | history                                    |
      | 06 | recordable now                             |
      | 07 | recent notes                               |

  @AC-050 @FR-018 @US6-AS4
  Scenario Outline: The anonymous visitor gets "You are not signed in." from every use case of this slice
    Given the registered user `f5.ac050.owner.<n>@example.com`, who created the custom habit `Reading`
    And `f5.ac050.owner.<n>@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    And `f5.ac050.owner.<n>@example.com` recorded 20 for 2026-11-18 with the note "Private note."
    When the anonymous visitor calls <use case> with the goal of `f5.ac050.owner.<n>@example.com`
    Then the result is "You are not signed in." and it gives no record, note or state
    And reading the history of the goal from 2026-11-18 to 2026-11-18 as `f5.ac050.owner.<n>@example.com` gives records (2026-11-18, 20) and the note "Private note."

    Examples:
      | n  | use case                                   |
      | 01 | record                                     |
      | 02 | change, with the owner's record identifier |
      | 03 | delete, with the owner's record identifier |
      | 04 | save note                                  |
      | 05 | history                                    |
      | 06 | recordable now                             |
      | 07 | recent notes                               |

  @AC-051 @FR-030 @US7-AS1
  Scenario: Demo history loading creates the demo account with history and its three demo goals
    Given the AC database is reset, so no account and no habit exists
    And at `2026-11-18T10:00:00Z` demo loading ran
    When at `2026-11-18T10:00:00Z` demo history loading runs
    Then its results are, in this order, "created", "demo goals added" and "demo records added", each for `demo4@example.com`, and no other result
    And sign-in with `demo4@example.com` and `example2026` succeeds, as a registered user
    And `demo4@example.com`'s goal list has exactly three goals: "Reading, 20 min daily" (catalog habit `Reading`, Build, duration, Commitment-based), "Sugar, at most 1 sweet a day" (catalog habit `Sugar`, Reduce, count, Period-based) and "Yoga, 3 times a week" (catalog habit `Yoga`, Build, yes/no, Period-based)
    And reading each demo goal gives: "Reading, 20 min daily" duration 20 minutes, fixed days on all seven days; "Sugar, at most 1 sweet a day" count 1 `sweet`, 1 time per day; "Yoga, 3 times a week" 3 times per week
    And reading each demo goal gives the start date 2026-10-21 and the time zone `UTC`
    And `demo4@example.com` has no account name and no custom habits

  @AC-052 @FR-030 @US7-AS2
  Scenario: The demo goal "Reading, 20 min daily" shows Met, Failed with a partial amount, Missed with a note, Pending and Due
    Given the AC database is reset, so no account and no habit exists
    And at `2026-11-18T10:00:00Z` demo loading and then demo history loading ran (R is 2026-11-18)
    When at `2026-11-18T10:00:00Z` `demo4@example.com` reads the history of "Reading, 20 min daily" from 2026-10-21 to 2026-11-18
    Then it gives 29 entries, one for each date from 2026-10-21 to 2026-11-18, in date order
    And each date from 2026-10-21 to 2026-11-15 except 2026-11-08 and 2026-11-12: Met, one record of 20, no note
    And 2026-11-08: Missed, no record, note "Away for the weekend."
    And 2026-11-12: Failed, records (2026-11-12, 12), progress 12 of 20 minutes, note "Fell asleep early."
    And 2026-11-16: Pending, grace ends `2026-11-19T00:00:00Z`, records (2026-11-16, 25), no note
    And 2026-11-17: Pending, grace ends `2026-11-20T00:00:00Z`, no record, no note
    And 2026-11-18: Due, no record, no note

  @AC-053 @FR-030 @US7-AS2
  Scenario: The demo goal "Sugar, at most 1 sweet a day" shows Met, Failed with a note, Missed, Pending and Due
    Given the AC database is reset, so no account and no habit exists
    And at `2026-11-18T10:00:00Z` demo loading and then demo history loading ran (R is 2026-11-18)
    When at `2026-11-18T10:00:00Z` `demo4@example.com` reads the history of "Sugar, at most 1 sweet a day" from 2026-10-21 to 2026-11-18
    Then it gives 29 entries, one for each date from 2026-10-21 to 2026-11-18, in date order
    And each of 2026-10-21, 2026-10-23, 2026-10-25, 2026-10-27, 2026-10-29, 2026-10-31, 2026-11-02, 2026-11-04, 2026-11-08, 2026-11-10 and 2026-11-12: Met, one record of 0, progress total 0 of at most 1, no note
    And each of 2026-10-22, 2026-10-24, 2026-10-26, 2026-10-28, 2026-10-30, 2026-11-01, 2026-11-03, 2026-11-05, 2026-11-07, 2026-11-09, 2026-11-11, 2026-11-13 and 2026-11-15: Met, one record of 1, progress total 1 of at most 1, no note
    And 2026-11-06: Missed, no record, no note
    And 2026-11-14: Failed, records (2026-11-14, 3), progress total 3 of at most 1, note "Birthday cake at work."
    And 2026-11-16: Pending, grace ends `2026-11-19T00:00:00Z`, no record
    And 2026-11-17: Pending, grace ends `2026-11-20T00:00:00Z`, no record
    And 2026-11-18: Due, no record

  @AC-054 @FR-030 @US7-AS2
  Scenario: The demo goal "Yoga, 3 times a week" shows a Met, a Failed and a Missed week and a Due week when R is a Wednesday
    Given the AC database is reset, so no account and no habit exists
    And at `2026-11-18T10:00:00Z` demo loading and then demo history loading ran (R is Wednesday 2026-11-18; the goal starts 2026-10-21, so its first week is 2026-10-26 to 2026-11-01)
    When at `2026-11-18T10:00:00Z` `demo4@example.com` reads the history of "Yoga, 3 times a week" from 2026-10-21 to 2026-11-22
    Then it gives 4 entries, in this order:
    And 2026-10-26 to 2026-11-01: Missed, no record, no note
    And 2026-11-02 to 2026-11-08: Failed, records (2026-11-02, Yes), (2026-11-04, Yes), progress 2 of 3 times, note "Knee was sore."
    And 2026-11-09 to 2026-11-15: Met, records (2026-11-09, Yes), (2026-11-11, Yes), (2026-11-13, Yes), progress 3 of 3 times, no note
    And 2026-11-16 to 2026-11-22: Due, grace ends `2026-11-25T00:00:00Z`, no record, no note

  @AC-055 @FR-030 @US7-AS2
  Scenario: When R is a Monday, the demo Yoga goal's last full week is Pending and it has a week before week 3
    Given the AC database is reset, so no account and no habit exists
    And at `2026-11-23T10:00:00Z` demo loading and then demo history loading ran (R is Monday 2026-11-23; the goal starts 2026-10-26)
    When at `2026-11-23T10:00:00Z` `demo4@example.com` reads the history of "Yoga, 3 times a week" from 2026-10-26 to 2026-11-29
    Then it gives 5 entries, in this order:
    And 2026-10-26 to 2026-11-01: Met, records (2026-10-26, Yes), (2026-10-28, Yes), (2026-10-30, Yes), no note
    And 2026-11-02 to 2026-11-08: Missed, no record, no note
    And 2026-11-09 to 2026-11-15: Failed, records (2026-11-09, Yes), (2026-11-11, Yes), progress 2 of 3 times, note "Knee was sore."
    And 2026-11-16 to 2026-11-22: Pending, grace ends `2026-11-25T00:00:00Z`, records (2026-11-16, Yes), (2026-11-18, Yes), (2026-11-20, Yes), progress 3 of 3 times, no note
    And 2026-11-23 to 2026-11-29: Due, no record, no note

  @AC-056 @FR-008 @FR-030 @US7-AS2
  Scenario: The demo goals' recent notes give the demo notes with their states
    Given the AC database is reset, so no account and no habit exists
    And at `2026-11-18T10:00:00Z` demo loading and then demo history loading ran
    When at `2026-11-18T10:00:00Z` `demo4@example.com` reads the recent notes of each of its demo goals
    Then "Reading, 20 min daily" gives exactly, in this order: 2026-11-12, "Fell asleep early.", Failed; and 2026-11-08, "Away for the weekend.", Missed
    And "Sugar, at most 1 sweet a day" gives exactly: 2026-11-14, "Birthday cake at work.", Failed
    And "Yoga, 3 times a week" gives exactly: 2026-11-02 to 2026-11-08, "Knee was sore.", Failed

  @AC-057 @FR-030 @US7-AS3
  Scenario: Demo history loading run again leaves the existing demo account with history unchanged
    Given the AC database is reset, so no account and no habit exists
    And at `2026-11-18T10:00:00Z` demo loading and then demo history loading ran
    When at `2026-11-19T10:00:00Z` demo loading runs and then demo history loading runs again
    Then demo history loading's results are "already exists" for `demo4@example.com`, and no goal or record result
    And `demo4@example.com`'s goal list still has exactly three goals, each with the start date 2026-10-21
    And at `2026-11-19T10:00:00Z` `demo4@example.com` reads the history of "Reading, 20 min daily" from 2026-11-15 to 2026-11-19, which gives, in this order: 2026-11-15, Met, records (2026-11-15, 20); 2026-11-16, Met, records (2026-11-16, 25); 2026-11-17, Pending, no record; 2026-11-18, Pending, no record; 2026-11-19, Due, no record
    And `demo4@example.com`'s recent notes of "Reading, 20 min daily" are still exactly 2026-11-12, "Fell asleep early." and 2026-11-08, "Away for the weekend."

  @AC-058 @FR-030 @US7-AS3
  Scenario: An account that already has the demo email gets no demo goals and keeps its password
    Given the AC database is reset, so no account and no habit exists
    And the registered user `Demo4@Example.com`
    And demo loading ran
    When demo history loading runs
    Then its results are "already exists" for `demo4@example.com`, and no goal or record result
    And sign-in with `demo4@example.com` and `walking42` succeeds, and its goal list is empty

  @AC-059 @FR-030 @US7-AS4
  Scenario: Demo history loading's results name the demo email and no goal, habit, note, unit or amount
    Given the AC database is reset, so no account and no habit exists
    And demo loading ran
    When demo history loading runs
    Then every result names `demo4@example.com`
    And no result contains any of `Reading`, `Sugar`, `Yoga`, `sweet`, `min`, `Fell asleep early.`, `Away for the weekend.`, `Birthday cake at work.`, `Knee was sore.`, `12`, `20` or `25`

  @AC-060 @FR-030
  Scenario Outline: A demo goal is left out when its catalog habit is not Published, and the results stay the same
    Given the AC database is reset, so no account and no habit exists
    And the administrator `f5.ac060.admin.<n>@example.com` created <habits>
    And demo loading ran, so the seed catalog was not loaded into the catalog, which was not empty
    When demo history loading runs
    Then its results are, in this order, "created", "demo goals added" and "demo records added", each for `demo4@example.com`
    And `demo4@example.com`'s goal list is <goals>

    Examples:
      | n  | habits                                                           | goals                                                                                                      |
      | 01 | a Published `Yoga`, a Draft `Reading` and an Unpublished `Sugar` | exactly one goal, on the catalog habit `Yoga`, Build, yes/no, 3 times per week                             |
      | 02 | a Published `reading`, a Draft `Sugar` and a Draft `Yoga`        | exactly one goal, on the catalog habit `reading`, Build, duration 20 minutes, fixed days on all seven days |
      | 03 | a Draft `Reading`, a Draft `Sugar` and a Draft `Yoga`            | empty                                                                                                      |

  @AC-061 @FR-030 @US7-AS6
  Scenario Outline: Demo history loading without a demo password creates no account
    Given the AC database is reset, so no account and no habit exists
    And demo loading ran
    When demo history loading runs with the demo password <demo password>, where absent means that no value is supplied
    Then its result is "no demo password configured", and no other result
    And demo history loading run afterwards with the demo password `example2026` gives "created" for `demo4@example.com`

    Examples:
      | n  | demo password |
      | 01 | absent        |
      | 02 | `""`          |
      | 03 | `"   "`       |

  @AC-062 @FR-019 @FR-031 @US7-AS5
  Scenario: Demo loading still names only the three slice-4 demo accounts, and their goals get no records
    Given the AC database is reset, so no account and no habit exists
    When at `2026-11-18T10:00:00Z` demo loading runs and then demo history loading runs
    Then demo loading's results name `demo1@example.com`, `demo2@example.com` and `demo3@example.com` and no other email, and none names `demo4@example.com`
    And reading the history from 2026-11-18 to 2026-11-25 of every goal of `demo1@example.com`, `demo2@example.com` and `demo3@example.com` gives no record and no note

  @AC-063 @FR-019
  Scenario: The evaluation calendar still gives no state
    Given the registered user `f5.ac063.user@example.com`, who created the custom habit `Reading`
    And `f5.ac063.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    And `f5.ac063.user@example.com` recorded 20 for 2026-11-18
    When at `2026-11-21T10:00:00Z` `f5.ac063.user@example.com` reads the evaluation calendar of the goal from 2026-11-18 to 2026-11-19
    Then it has 2 commitments, 2026-11-18 from `2026-11-18T00:00:00Z` to `2026-11-19T00:00:00Z` and 2026-11-19 from `2026-11-19T00:00:00Z` to `2026-11-20T00:00:00Z`
    And no entry has a status

  @AC-064 @FR-004 @FR-005 @FR-006 @FR-007
  Scenario Outline: Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement
    Given the registered user `f5.ac064.user.<n>@example.com`, who created the custom habit `Practice`
    And `f5.ac064.user.<n>@example.com` created a goal on their custom habit `Practice` with Build, <measurement>, fixed days on all seven days and the start date 2026-11-18
    And `f5.ac064.user.<n>@example.com` recorded <before> for 2026-11-18 with the note "First note."
    When at `2026-11-22T10:00:00Z` `f5.ac064.user.<n>@example.com` changes <change>
    Then <result>

    Examples:
      | n  | measurement         | before | change                                                                | result                                                                                                                                                                                       |
      | 01 | count 5 `pages`     | 5      | the record of 2026-11-18 to no value                                  | the result is "Enter a whole number from 0 to 9999." and the history from 2026-11-18 to 2026-11-18 gives the entry 2026-11-18, Met, records (2026-11-18, 5), note "First note."            |
      | 02 | count 5 `pages`     | 5      | the record of 2026-11-18 to -1                                        | the result is "Enter a whole number from 0 to 9999." and the history from 2026-11-18 to 2026-11-18 gives records (2026-11-18, 5)                                                            |
      | 03 | count 5 `pages`     | 5      | the record of 2026-11-18 to 10000                                     | the result is "Enter a whole number from 0 to 9999." and the history from 2026-11-18 to 2026-11-18 gives records (2026-11-18, 5)                                                            |
      | 04 | duration 20 minutes | 20     | the record of 2026-11-18 to 1441                                      | the result is "Enter a whole number of minutes from 0 to 1440." and the history from 2026-11-18 to 2026-11-18 gives records (2026-11-18, 20)                                                |
      | 05 | yes/no              | Yes    | the record of 2026-11-18 to no value                                  | the result is "Choose Yes or No." and the history from 2026-11-18 to 2026-11-18 gives records (2026-11-18, Yes)                                                                             |
      | 06 | count 5 `pages`     | 5      | the record of 2026-11-18 to 3 with the note a text of 501 characters  | the result is "The note can have at most 500 characters." and the history from 2026-11-18 to 2026-11-18 gives records (2026-11-18, 5) and the note "First note."                           |
      | 07 | count 5 `pages`     | 5      | a record identifier that belongs to no record to no value             | the result is "This record does not exist." (the record is checked before the value) and the history from 2026-11-18 to 2026-11-18 gives records (2026-11-18, 5)                           |
      | 08 | count 5 `pages`     | 5      | the record of 2026-11-18 to 9999                                      | changing succeeds with the state Met, a correction, and the history from 2026-11-18 to 2026-11-18 gives records (2026-11-18, 9999) and the note "First note."                              |
      | 09 | count 5 `pages`     | 5      | the record of 2026-11-18 to the amount 7 together with Yes            | changing succeeds with the state Met, a correction, and the history from 2026-11-18 to 2026-11-18 gives one record, (2026-11-18, 7), with neither Yes nor No                                |
      | 10 | yes/no              | No     | the record of 2026-11-18 to Yes together with the amount 5            | changing succeeds with the state Met, a correction, and the history from 2026-11-18 to 2026-11-18 gives one record, (2026-11-18, Yes), with no amount                                       |
      | 11 | count 5 `pages`     | 5      | the record of 2026-11-18 to -1 with the note a text of 501 characters | the result is "Enter a whole number from 0 to 9999." (the value is checked before the note) and the history from 2026-11-18 to 2026-11-18 gives records (2026-11-18, 5) and the note "First note." |

  @AC-065 @FR-001 @FR-010
  Scenario: A goal whose catalog habit is Unpublished is still recorded and given its states
    Given the administrator `f5.ac065.admin@example.com`, who created a Published `A5.065 Run`
    And the registered user `f5.ac065.user@example.com`
    And at `2026-11-16T08:00:00Z` `f5.ac065.user@example.com` created a goal on the catalog habit `A5.065 Run` with Build, yes/no, fixed days on all seven days and the start date 2026-11-16
    And at `2026-11-17T10:00:00Z` `f5.ac065.admin@example.com` unpublished `A5.065 Run` with confirmation, as a habit that goals are based on needs (spec edge case "A goal whose catalog habit is Unpublished"), and its state is Unpublished
    When at `2026-11-18T10:00:00Z` `f5.ac065.user@example.com` records Yes for 2026-11-18
    Then recording succeeds with the state Due, not a correction
    And reading the history of the goal from 2026-11-16 to 2026-11-18 gives, in this order: 2026-11-16, Pending, grace ends `2026-11-19T00:00:00Z`, no record; 2026-11-17, Pending, grace ends `2026-11-20T00:00:00Z`, no record; 2026-11-18, Due, records (2026-11-18, Yes)

  @AC-066 @FR-014
  Scenario Outline: The other kinds of target, just inside, at and just past their target
    Given the registered user `f5.ac066.user.<n>@example.com`, who created the custom habit `Practice`
    And at `2026-11-16T08:00:00Z` `f5.ac066.user.<n>@example.com` created a goal on their custom habit `Practice` with <settings> and the start date 2026-11-16
    And at `2026-11-18T10:00:00Z` `f5.ac066.user.<n>@example.com` recorded <records>
    When at `2026-11-25T00:00:00Z` `f5.ac066.user.<n>@example.com` reads the history of the goal from 2026-11-16 to 2026-11-22
    Then its entry that contains 2026-11-18 is <state>, <progress>

    Examples:
      | n  | settings                                                   | records                                                      | state  | progress                        |
      | 01 | Build, count 5 `pages`, fixed days on all seven days       | 4 for 2026-11-18                                             | Failed | progress 4 of 5                 |
      | 02 | Build, count 5 `pages`, fixed days on all seven days       | 5 for 2026-11-18                                             | Met    | progress 5 of 5                 |
      | 03 | Build, count 5 `pages`, fixed days on all seven days       | 6 for 2026-11-18                                             | Met    | progress 6 of 5                 |
      | 04 | Build, count 5 `pages`, 2 times per day                    | 5 and 5 for 2026-11-18                                       | Met    | progress 2 of 2 times           |
      | 05 | Build, count 5 `pages`, 2 times per day                    | 5 and 4 for 2026-11-18                                       | Failed | progress 1 of 2 times           |
      | 06 | Build, duration 20 minutes, 1 time per day                 | 19 for 2026-11-18                                            | Failed | progress 0 of 1 times           |
      | 07 | Build, duration 20 minutes, 1 time per day                 | 20 for 2026-11-18                                            | Met    | progress 1 of 1 times           |
      | 08 | Build, duration 20 minutes, 3 times per week               | 20 for 2026-11-16, 19 for 2026-11-17 and 20 for 2026-11-18   | Failed | progress 2 of 3 times           |
      | 09 | Build, duration 20 minutes, 3 times per week               | 20 for 2026-11-16, 20 for 2026-11-17 and 20 for 2026-11-18   | Met    | progress 3 of 3 times           |
      | 10 | Reduce, duration 30 minutes, fixed days on all seven days  | 30 for 2026-11-18                                            | Met    | progress 30 of 30               |
      | 11 | Reduce, duration 30 minutes, fixed days on all seven days  | 31 for 2026-11-18                                            | Failed | progress 31 of 30               |
      | 12 | Reduce, duration 30 minutes, 1 time per day                | 20 and 10 for 2026-11-18                                     | Met    | progress total 30 of at most 30 |
      | 13 | Reduce, duration 30 minutes, 1 time per day                | 20 and 11 for 2026-11-18                                     | Failed | progress total 31 of at most 30 |
      | 14 | Reduce, count 3 `sweets`, 1 time per week                  | 1 for 2026-11-16 and 2 for 2026-11-18                        | Met    | progress total 3 of at most 3   |
      | 15 | Reduce, count 3 `sweets`, 1 time per week                  | 2 for 2026-11-16 and 2 for 2026-11-18                        | Failed | progress total 4 of at most 3   |
      | 16 | Reduce, yes/no, 2 times per week                           | Yes for 2026-11-16, Yes for 2026-11-17 and No for 2026-11-18 | Met    | progress 2 of at most 2         |
      | 17 | Reduce, yes/no, 2 times per week                           | Yes for 2026-11-16, Yes for 2026-11-17 and Yes for 2026-11-18 | Failed | progress 3 of at most 2        |
      | 18 | Reduce, yes/no, 2 times per day                            | Yes and Yes for 2026-11-18                                   | Met    | progress 2 of at most 2         |
      | 19 | Reduce, yes/no, 2 times per day                            | Yes, Yes and Yes for 2026-11-18                              | Failed | progress 3 of at most 2         |
      | 20 | Build, count 5 `pages`, 2 times per day                    | 6 and 6 for 2026-11-18                                       | Met    | progress 2 of 2 times           |
      | 21 | Build, duration 20 minutes, 1 time per day                 | 21 for 2026-11-18                                            | Met    | progress 1 of 1 times           |
      | 22 | Build, duration 20 minutes, 3 times per week               | 21 for 2026-11-16, 21 for 2026-11-17 and 21 for 2026-11-18   | Met    | progress 3 of 3 times           |
      | 23 | Reduce, duration 30 minutes, fixed days on all seven days  | 29 for 2026-11-18                                            | Met    | progress 29 of 30               |
      | 24 | Reduce, duration 30 minutes, 1 time per day                | 20 and 9 for 2026-11-18                                      | Met    | progress total 29 of at most 30 |
      | 25 | Reduce, count 3 `sweets`, 1 time per week                  | 1 for 2026-11-16 and 1 for 2026-11-18                        | Met    | progress total 2 of at most 3   |
      | 26 | Reduce, yes/no, 2 times per week                           | Yes for 2026-11-16 and No for 2026-11-18                     | Met    | progress 1 of at most 2         |
      | 27 | Reduce, yes/no, 2 times per day                            | Yes and No for 2026-11-18                                    | Met    | progress 1 of at most 2         |
      | 28 | Reduce, duration 60 minutes, 1 time per week               | 30 for 2026-11-16 and 29 for 2026-11-18                      | Met    | progress total 59 of at most 60 |
      | 29 | Reduce, duration 60 minutes, 1 time per week               | 30 for 2026-11-16 and 30 for 2026-11-18                      | Met    | progress total 60 of at most 60 |
      | 30 | Reduce, duration 60 minutes, 1 time per week               | 30 for 2026-11-16 and 31 for 2026-11-18                      | Failed | progress total 61 of at most 60 |
      | 31 | Build, count 5 `pages`, 2 times per day                    | 10 for 2026-11-18                                            | Failed | progress 1 of 2 times           |
      | 32 | Build, duration 20 minutes, 3 times per week               | 60 for 2026-11-16                                            | Failed | progress 1 of 3 times           |
      | 33 | Build, duration 20 minutes, 3 times per week               | 60 for 2026-11-16, 1 for 2026-11-17 and 59 for 2026-11-18    | Failed | progress 2 of 3 times           |

  @AC-067 @FR-002 @FR-011 @US5-AS3
  Scenario: After a move east, the old and the new day can both be recorded while they overlap
    Given the registered user `f5.ac067.user@example.com` in `America/New_York`, who created the custom habit `Practice`
    And at `2026-11-16T15:00:00Z` `f5.ac067.user@example.com` created a goal on their custom habit `Practice` with Build, yes/no, fixed days on all seven days and the start date 2026-11-16
    And at `2026-11-18T10:00:00Z` `f5.ac067.user@example.com` changed their time zone to `Europe/Kyiv`, and the change succeeded
    When at `2026-11-18T23:00:00Z` `f5.ac067.user@example.com` records Yes for 2026-11-18 and then records Yes for 2026-11-19
    Then each recording succeeds with the state Due, not a correction
    And reading the history of the goal from 2026-11-18 to 2026-11-19 gives, in this order: 2026-11-18 from `2026-11-18T05:00:00Z` to `2026-11-19T05:00:00Z`, Due, records (2026-11-18, Yes); and 2026-11-19 from `2026-11-18T22:00:00Z` to `2026-11-19T22:00:00Z`, Due, records (2026-11-19, Yes)

  @AC-068 @FR-030 @US7-AS2
  Scenario Outline: The demo Yoga goal's weeks when R is a Sunday or a Tuesday
    Given the AC database is reset, so no account and no habit exists
    And at <run time> demo loading and then demo history loading ran (R is <R>)
    When at <run time> `demo4@example.com` reads the history of "Yoga, 3 times a week" from <start date> to <last date>
    Then reading the goal gives the start date <start date>
    And the history gives, in this order, <entries>

    Examples:
      | n  | run time               | R                  | start date | last date  | entries                                                                                                                                                                                                                                                                                                                                                       |
      | 01 | `2026-11-22T10:00:00Z` | Sunday 2026-11-22  | 2026-10-25 | 2026-11-22 | 4 entries: 2026-10-26 to 2026-11-01, Missed, no record; 2026-11-02 to 2026-11-08, Failed, records (2026-11-02, Yes), (2026-11-04, Yes), note "Knee was sore."; 2026-11-09 to 2026-11-15, Met, records (2026-11-09, Yes), (2026-11-11, Yes), (2026-11-13, Yes); 2026-11-16 to 2026-11-22, Due, grace ends `2026-11-25T00:00:00Z`, no record                     |
      | 02 | `2026-11-24T10:00:00Z` | Tuesday 2026-11-24 | 2026-10-27 | 2026-11-29 | 4 entries: 2026-11-02 to 2026-11-08, Missed, no record; 2026-11-09 to 2026-11-15, Failed, records (2026-11-09, Yes), (2026-11-11, Yes), note "Knee was sore."; 2026-11-16 to 2026-11-22, Pending, grace ends `2026-11-25T00:00:00Z`, records (2026-11-16, Yes), (2026-11-18, Yes), (2026-11-20, Yes), progress 3 of 3 times; 2026-11-23 to 2026-11-29, Due, no record |

  @AC-069 @FR-030 @US7-AS6
  Scenario Outline: Demo history loading takes a demo password only with at least 8 characters and a digit
    Given the AC database is reset, so no account and no habit exists
    And demo loading ran
    When demo history loading runs with the demo password <demo password>
    Then <result>

    Examples:
      | n  | demo password | why                        | result                                                                                                                                                                                         |
      | 01 | `exampl1`     | 7 characters, with a digit | its result is "demo password does not meet the password rule", and no other result, and demo history loading run afterwards with the demo password `example2026` gives "created" for `demo4@example.com` |
      | 02 | `examples`    | 8 characters, no digit     | its result is "demo password does not meet the password rule", and no other result, and demo history loading run afterwards with the demo password `example2026` gives "created" for `demo4@example.com` |
      | 03 | `abc`         | 3 characters, no digit     | its result is "demo password does not meet the password rule", and no other result, and demo history loading run afterwards with the demo password `example2026` gives "created" for `demo4@example.com` |
      | 04 | `exampl12`    | 8 characters, with a digit | its results are, in this order, "created", "demo goals added" and "demo records added", each for `demo4@example.com`, and sign-in with `demo4@example.com` and `exampl12` succeeds            |

  @AC-070 @FR-007 @US4-AS8
  Scenario Outline: Deleting the only record of a commitment keeps its note
    Given the registered user `f5.ac070.user.<n>@example.com`, who created the custom habit `Reading`
    And `f5.ac070.user.<n>@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    And `f5.ac070.user.<n>@example.com` recorded 12 for 2026-11-18 with the note "Fell asleep early."
    When at <time> `f5.ac070.user.<n>@example.com` deletes the record of 2026-11-18
    Then <result>
    And reading the history of the goal from 2026-11-18 to 2026-11-18 gives the entry 2026-11-18, <state>, no record, note "Fell asleep early."

    Examples:
      | n  | time                   | result                                                 | state  |
      | 01 | `2026-11-18T12:00:00Z` | deleting succeeds with the state Due, not a correction | Due    |
      | 02 | `2026-11-22T10:00:00Z` | deleting succeeds with the state Missed, a correction  | Missed |

  @AC-071 @FR-007 @US4-AS9
  Scenario Outline: The first record of a Missed commitment that has a note: the note given is the note, no note keeps it
    Given the registered user `f5.ac071.user.<n>@example.com`, who created the custom habit `Reading`
    And `f5.ac071.user.<n>@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    And at `2026-11-22T10:00:00Z` `f5.ac071.user.<n>@example.com` saved the note "Away for the weekend." for 2026-11-18
    When at `2026-11-22T10:00:00Z` `f5.ac071.user.<n>@example.com` <action>
    Then recording succeeds with the state Met, a correction
    And reading the history of the goal from 2026-11-18 to 2026-11-18 gives the entry 2026-11-18, Met, records (2026-11-18, 20), <note>

    Examples:
      | n  | action                                                          | note                         |
      | 01 | records 20 for 2026-11-18 with the note "Away for the weekend." | note "Away for the weekend." |
      | 02 | records 20 for 2026-11-18 with the note `""`                    | no note                      |
      | 03 | records 20 for 2026-11-18 with the note `"   "`                 | no note                      |
      | 04 | records 20 for 2026-11-18                                       | note "Away for the weekend." |

  @AC-072 @FR-007 @US4-AS10
  Scenario: Changing the record of a commitment without a note, with a note, adds the note
    Given the registered user `f5.ac072.user@example.com`, who created the custom habit `Reading`
    And `f5.ac072.user@example.com` created a goal on their custom habit `Reading` with Build, duration 20 minutes, fixed days on all seven days and the start date 2026-11-18
    And `f5.ac072.user@example.com` recorded 15 for 2026-11-18
    When `f5.ac072.user@example.com` changes the record of 2026-11-18 to 20 with the note "Back was tired."
    Then changing succeeds with the state Due, not a correction
    And reading the history of the goal from 2026-11-18 to 2026-11-18 gives the entry 2026-11-18, Due, records (2026-11-18, 20), note "Back was tired."
