@F008
Feature: Excusals and pauses

  @AC-001 @FR-001 @FR-002 @FR-017 @US1-AS1
  Scenario: Excusing a Pending commitment with a reason
    Given the registered user `f8.ac001.user@example.com`, who created the goal `Yoga, 3 days a week`
    When at 2026-11-12T10:00:00Z `f8.ac001.user@example.com` excuses 2026-11-11 with the reason "Ill"
    Then the result is "excused" with the date 2026-11-11
    And the history of the goal from 2026-11-09 to 2026-11-15 gives 2026-11-11 Excused, no record, no grace end, correction no, reason "Ill", removable
    And what can be recorded now contains no entry for 2026-11-11

  @AC-002 @FR-001 @FR-002 @US1-AS2
  Scenario: Excusing a Due commitment without a reason
    Given the registered user `f8.ac002.user@example.com`, who created the goal `Yoga, 3 days a week`
    When at 2026-11-13T07:00:00Z `f8.ac002.user@example.com` excuses 2026-11-13 without a reason
    Then the result is "excused" with the date 2026-11-13
    And the history of the goal from 2026-11-13 to 2026-11-13 gives 2026-11-13 Excused, no reason, removable
    And what can be recorded now contains no entry for 2026-11-13

  @AC-003 @FR-001
  Scenario Outline: The reason is trimmed, and a reason that is empty after trimming is none
    Given the registered user `f8.ac003.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
    When at 2026-11-12T10:00:00Z `f8.ac003.<n>.user@example.com` excuses 2026-11-11 with the reason <given>
    Then the result is "excused" with the date 2026-11-11
    And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Excused, <stored>

    Examples:
      | n  | given     | stored       |
      | 01 | "  Ill  " | reason "Ill" |
      | 02 | "   "     | no reason    |

  @AC-004 @FR-003 @FR-017 @US1-AS3
  Scenario Outline: An excused commitment stays Excused after its grace period would have ended
    Given the registered user `f8.ac004.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-12T10:00:00Z `f8.ac004.<n>.user@example.com` excused 2026-11-11 with the reason "Ill"
    When at <time> `f8.ac004.<n>.user@example.com` reads the history of the goal from 2026-11-11 to 2026-11-11
    Then it gives 2026-11-11 Excused, no record, no grace end, correction no, reason "Ill", not removable

    Examples:
      | n  | time                 |
      | 01 | 2026-11-14T00:00:00Z |
      | 02 | 2026-12-01T00:00:00Z |

  @AC-005 @FR-003 @FR-025
  Scenario: An excusal applies at every supplied time, also one earlier than the excusing call
    Given the registered user `f8.ac005.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-12T10:00:00Z `f8.ac005.user@example.com` excused 2026-11-11 without a reason
    When at 2026-11-12T09:00:00Z `f8.ac005.user@example.com` reads the history of the goal from 2026-11-11 to 2026-11-11
    Then it gives 2026-11-11 Excused, no reason

  @AC-006 @FR-002 @US1-AS4
  Scenario Outline: A final commitment cannot be excused
    Given the registered user `f8.ac006.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
    And `f8.ac006.<n>.user@example.com` <setup>
    When at 2026-11-12T10:00:00Z `f8.ac006.<n>.user@example.com` excuses <date> without a reason
    Then the result is "This commitment is final and can no longer be excused."
    And the history of the goal from <date> to <date> gives <date> <state>

    Examples:
      | n  | setup                      | date       | state  |
      | 01 | records nothing            | 2026-11-09 | Missed |
      | 02 | records Yes for 2026-11-04 | 2026-11-04 | Met    |
      | 03 | records No for 2026-11-06  | 2026-11-06 | Failed |

  @AC-007 @FR-002
  Scenario Outline: Excusing just before, at and just after the end of the grace period
    Given the registered user `f8.ac007.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
    When at <time> `f8.ac007.<n>.user@example.com` excuses 2026-11-11 without a reason
    Then the result is <result>
    And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 <state>

    Examples:
      | n  | time                 | result                                                   | state   |
      | 01 | 2026-11-13T23:59:59Z | "excused"                                                | Excused |
      | 02 | 2026-11-14T00:00:00Z | "This commitment is final and can no longer be excused." | Missed  |
      | 03 | 2026-11-14T00:00:01Z | "This commitment is final and can no longer be excused." | Missed  |

  @AC-008 @FR-002 @US1-AS5
  Scenario Outline: An Upcoming commitment cannot be excused, also with a reason that is too long
    Given the registered user `f8.ac008.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
    When at <time> `f8.ac008.<n>.user@example.com` excuses 2026-11-13 with the reason <reason>
    Then the result is <result>

    Examples:
      | n  | time                 | reason                 | result                                               |
      | 01 | 2026-11-12T23:59:59Z | "Away"                 | "You cannot excuse a commitment that has not begun." |
      | 02 | 2026-11-13T00:00:00Z | "Away"                 | "excused"                                            |
      | 03 | 2026-11-12T10:00:00Z | `x` repeated 201 times | "You cannot excuse a commitment that has not begun." |

  @AC-009 @FR-002 @US1-AS6
  Scenario Outline: A commitment of a period-based goal cannot be excused
    Given the registered user `f8.ac009.<n>.user@example.com`, who created the goal <goal>
    When at 2026-11-12T10:00:00Z `f8.ac009.<n>.user@example.com` excuses 2026-11-11 without a reason
    Then the result is "Only commitments can be excused. This goal is evaluated by period."
    And the history of the goal from 2026-11-11 to 2026-11-11 gives <entry>

    Examples:
      | n  | goal                              | entry                         |
      | 01 | `Yoga, 3 times a week`            | 2026-11-09 – 2026-11-15 Due   |
      | 02 | `Drinking water, 8 glasses a day` | 2026-11-11 Pending            |

  @AC-010 @FR-002 @US1-AS7
  Scenario Outline: A date that is not a commitment of the goal cannot be excused
    Given the registered user `f8.ac010.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
    When at 2026-11-12T10:00:00Z `f8.ac010.<n>.user@example.com` excuses <date> without a reason
    Then the result is "Nothing is scheduled on this date."

    Examples:
      | n  | date       |
      | 01 | 2026-11-10 |
      | 02 | 2026-10-30 |

  @AC-011 @FR-002 @FR-007
  Scenario Outline: A date in a pause cannot be excused, also when it has not begun
    Given the registered user `f8.ac011.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-16T08:00:00Z `f8.ac011.<n>.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 without a reason
    When at 2026-11-16T10:00:00Z `f8.ac011.<n>.user@example.com` excuses <date> without a reason
    Then the result is "This goal is paused on this date."
    And the history of the goal from <date> to <date> gives <date> Paused

    Examples:
      | n  | date       |
      | 01 | 2026-11-16 |
      | 02 | 2026-11-18 |

  @AC-012 @FR-002 @FR-007
  Scenario: A commitment excused before a pause covered it is refused as paused, not as already excused
    Given the registered user `f8.ac012.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-16T07:00:00Z `f8.ac012.user@example.com` excused 2026-11-16 without a reason
    And at 2026-11-16T08:00:00Z `f8.ac012.user@example.com` paused the goal from 2026-11-16 until 2026-11-16 without a reason
    When at 2026-11-16T09:00:00Z `f8.ac012.user@example.com` excuses 2026-11-16 without a reason
    Then the result is "This goal is paused on this date."
    And the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Paused

  @AC-013 @FR-002
  Scenario Outline: A commitment that is already excused cannot be excused again
    Given the registered user `f8.ac013.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-12T10:00:00Z `f8.ac013.<n>.user@example.com` excused 2026-11-11 with the reason "Ill"
    When at <time> `f8.ac013.<n>.user@example.com` excuses 2026-11-11 with the reason "Away"
    Then the result is "This commitment is already excused."
    And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Excused, reason "Ill"

    Examples:
      | n  | time                 |
      | 01 | 2026-11-12T11:00:00Z |
      | 02 | 2026-11-14T10:00:00Z |

  @AC-014 @FR-002 @US1-AS10
  Scenario Outline: A commitment that has a record cannot be excused
    Given the registered user `f8.ac014.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-12T09:00:00Z `f8.ac014.<n>.user@example.com` recorded No for 2026-11-11
    When at 2026-11-12T10:00:00Z `f8.ac014.<n>.user@example.com` excuses 2026-11-11 with the reason <reason>
    Then the result is "This commitment has a record. Delete the record to excuse it."
    And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Pending, record No

    Examples:
      | n  | reason                 |
      | 01 | "Ill"                  |
      | 02 | `x` repeated 201 times |

  @AC-015 @FR-002 @US1-AS10
  Scenario: After its record is deleted, a commitment can be excused
    Given the registered user `f8.ac015.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-12T09:00:00Z `f8.ac015.user@example.com` recorded No for 2026-11-11
    And at 2026-11-12T10:00:00Z `f8.ac015.user@example.com` deleted the record of 2026-11-11
    When at 2026-11-12T10:05:00Z `f8.ac015.user@example.com` excuses 2026-11-11 without a reason
    Then the result is "excused" with the date 2026-11-11
    And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Excused, no record

  @AC-016 @FR-001 @FR-002 @US1-AS11
  Scenario Outline: A reason has at most 200 characters
    Given the registered user `f8.ac016.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
    When at 2026-11-12T10:00:00Z `f8.ac016.<n>.user@example.com` excuses 2026-11-11 with the reason <reason>
    Then the result is <result>
    And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 <entry>

    Examples:
      | n  | reason                 | result                                        | entry                                  |
      | 01 | `x` repeated 200 times | "excused"                                     | Excused, reason `x` repeated 200 times |
      | 02 | `x` repeated 201 times | "Keep the reason to 200 characters or fewer." | Pending                                |

  @AC-017 @FR-004 @US1-AS8
  Scenario: An excusal can be removed while the grace period is open
    Given the registered user `f8.ac017.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-12T10:00:00Z `f8.ac017.user@example.com` excused 2026-11-11 with the reason "Ill"
    When at 2026-11-13T23:59:00Z `f8.ac017.user@example.com` removes the excusal of 2026-11-11
    Then the result is "excusal removed" with the state Pending
    And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Pending, no record, grace ends 2026-11-14T00:00:00Z
    And what can be recorded now contains 2026-11-11 Pending

  @AC-018 @FR-004 @US1-AS8
  Scenario Outline: Removing an excusal just before, at and just after the end of the grace period
    Given the registered user `f8.ac018.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-12T10:00:00Z `f8.ac018.<n>.user@example.com` excused 2026-11-11 with the reason "Ill"
    When at <time> `f8.ac018.<n>.user@example.com` removes the excusal of 2026-11-11
    Then the result is <result>
    And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 <state>

    Examples:
      | n  | time                 | result                                   | state   |
      | 01 | 2026-11-13T23:59:59Z | "excusal removed" with the state Pending | Pending |
      | 02 | 2026-11-14T00:00:00Z | "This excusal can no longer be removed." | Excused |
      | 03 | 2026-11-14T00:00:01Z | "This excusal can no longer be removed." | Excused |

  @AC-019 @FR-004
  Scenario Outline: Removing an excusal that does not exist
    Given the registered user `f8.ac019.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
    When at 2026-11-12T10:00:00Z `f8.ac019.<n>.user@example.com` removes the excusal of <date>
    Then the result is "This commitment is not excused."
    And the history of the goal from <date> to <date> gives <date> <state>

    Examples:
      | n  | date       | state   |
      | 01 | 2026-11-11 | Pending |
      | 02 | 2026-11-09 | Missed  |

  @AC-020 @FR-004 @FR-024
  Scenario: A commitment whose excusal is removed is evaluated as if it had never been excused
    Given the registered user `f8.ac020.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-13T07:00:00Z `f8.ac020.user@example.com` excused 2026-11-13 without a reason
    And at 2026-11-13T08:00:00Z `f8.ac020.user@example.com` removed the excusal of 2026-11-13, with the result "excusal removed" and the state Due
    And at 2026-11-13T09:00:00Z `f8.ac020.user@example.com` recorded Yes for 2026-11-13
    When at 2026-11-16T00:00:00Z `f8.ac020.user@example.com` reads the history of the goal from 2026-11-13 to 2026-11-13
    Then it gives 2026-11-13 Met, record Yes

  @AC-021 @FR-005 @US1-AS9
  Scenario: An excused commitment cannot be recorded
    Given the registered user `f8.ac021.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-12T10:00:00Z `f8.ac021.user@example.com` excused 2026-11-11 with the reason "Ill"
    When at 2026-11-12T11:00:00Z `f8.ac021.user@example.com` records Yes for 2026-11-11
    Then the result is "This commitment is excused. Remove the excusal to record it."
    And the history of the goal from 2026-11-11 to 2026-11-11 gives 2026-11-11 Excused, no record

  @AC-022 @FR-005 @FR-009
  Scenario Outline: The record checks for a pause and an excusal come after the date check, the pause first
    Given the registered user `f8.ac022.<n>.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-16T07:00:00Z `f8.ac022.<n>.user@example.com` excused 2026-11-16 without a reason
    And at 2026-11-16T08:00:00Z `f8.ac022.<n>.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 without a reason
    When at 2026-11-17T10:00:00Z `f8.ac022.<n>.user@example.com` records Yes for <date>
    Then the result is <result>
    And the history of the goal from 2026-11-16 to 2026-11-18 gives 2026-11-16 Paused, no record and 2026-11-18 Paused, no record, and no entry for 2026-11-17

    Examples:
      | n  | date       | result                               |
      | 01 | 2026-11-16 | "This goal is paused on this date."  |
      | 02 | 2026-11-17 | "Nothing is scheduled on this date." |

  @AC-023 @FR-006 @FR-007 @FR-018 @US2-AS1
  Scenario: Pausing a goal from today
    Given the registered user `f8.ac023.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
    When at 2026-11-16T09:00:00Z `f8.ac023.user@example.com` pauses the goal from 2026-11-16 until 2026-11-22 with the reason "Family visit"
    Then the result is "paused" with the first day 2026-11-16 and the last day 2026-11-22
    And the goals overview gives `Drinking water, 8 glasses a day` paused until 2026-11-22
    And the history of the goal from 2026-11-15 to 2026-11-16 gives 2026-11-15 Pending and 2026-11-16 Paused, no grace end
    And what can be recorded now contains 2026-11-15 Pending and no entry for 2026-11-16

  @AC-024 @FR-006 @US2-AS2
  Scenario: A pause cannot start before today
    Given the registered user `f8.ac024.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
    When at 2026-11-16T09:00:00Z `f8.ac024.user@example.com` pauses the goal from 2026-11-15 until 2026-11-22 without a reason
    Then the result is "A pause cannot start before today."
    And the pauses of the goal are empty

  @AC-025 @FR-006 @US2-AS3
  Scenario: The last day cannot be before the first day
    Given the registered user `f8.ac025.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
    When at 2026-11-16T09:00:00Z `f8.ac025.user@example.com` pauses the goal from 2026-11-20 until 2026-11-19 without a reason
    Then the result is "The last day cannot be before the first day."
    And the pauses of the goal are empty

  @AC-026 @FR-006 @US2-AS4
  Scenario Outline: A pause cannot share a date with another pause of the goal, but may touch it
    Given the registered user `f8.ac026.<n>.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
    And at 2026-11-16T09:00:00Z `f8.ac026.<n>.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 with the reason "Family visit"
    When at 2026-11-16T10:00:00Z `f8.ac026.<n>.user@example.com` pauses the goal from <first> until <last> without a reason
    Then the result is <result>

    Examples:
      | n  | first      | last       | result                                                               |
      | 01 | 2026-11-21 | 2026-11-24 | "This goal already has a pause on these dates."                      |
      | 02 | 2026-11-22 | 2026-11-22 | "This goal already has a pause on these dates."                      |
      | 03 | 2026-11-17 | 2026-11-18 | "This goal already has a pause on these dates."                      |
      | 04 | 2026-11-23 | 2026-11-29 | "paused" with the first day 2026-11-23 and the last day 2026-11-29   |

  @AC-027 @FR-006
  Scenario Outline: A pause needs a first and a last day
    Given the registered user `f8.ac027.<n>.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
    When at 2026-11-16T09:00:00Z `f8.ac027.<n>.user@example.com` pauses the goal <days> without a reason
    Then the result is "Choose the first and the last day."
    And the pauses of the goal are empty

    Examples:
      | n  | days                                |
      | 01 | with no first day until 2026-11-22  |
      | 02 | from 2026-11-16 with no last day    |
      | 03 | with no first day and no last day   |

  @AC-028 @FR-006
  Scenario Outline: The pause checks come in their order
    Given the registered user `f8.ac028.<n>.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
    And at 2026-11-16T08:00:00Z `f8.ac028.<n>.user@example.com` paused the goal from 2026-11-23 until 2026-11-25 without a reason
    When at 2026-11-16T09:00:00Z `f8.ac028.<n>.user@example.com` pauses the goal <days> with the reason <reason>
    Then the result is <result>

    Examples:
      | n  | days                             | reason                 | result                                                             |
      | 01 | from 2026-11-15 with no last day | "Away"                 | "Choose the first and the last day."                               |
      | 02 | from 2026-11-15 until 2026-11-14 | "Away"                 | "A pause cannot start before today."                               |
      | 03 | from 2026-11-24 until 2026-11-23 | `x` repeated 201 times | "The last day cannot be before the first day."                     |
      | 04 | from 2026-11-24 until 2026-11-30 | `x` repeated 201 times | "This goal already has a pause on these dates."                    |
      | 05 | from 2026-11-17 until 2026-11-18 | `x` repeated 201 times | "Keep the reason to 200 characters or fewer."                      |
      | 06 | from 2026-11-17 until 2026-11-18 | `x` repeated 200 times | "paused" with the first day 2026-11-17 and the last day 2026-11-18 |

  @AC-029 @FR-006
  Scenario Outline: For a goal that has not started, "today" is the account's today
    Given the registered user `f8.ac029.<n>.user@example.com`, who created the goal `Drinking water, 8 glasses a day` at 2026-11-16T08:00:00Z with the start date 2026-11-20
    When at 2026-11-16T09:00:00Z `f8.ac029.<n>.user@example.com` pauses the goal from <first> until 2026-11-18 without a reason
    Then the result is <result>

    Examples:
      | n  | first      | result                                                             |
      | 01 | 2026-11-16 | "paused" with the first day 2026-11-16 and the last day 2026-11-18 |
      | 02 | 2026-11-15 | "A pause cannot start before today."                               |

  @AC-030 @FR-006
  Scenario Outline: The first day is compared with the current date on the goal's own timeline
    Given the registered user `f8.ac030.<n>.user@example.com` in `<zone>`, who created the goal `Drinking water, 8 glasses a day`
    When at 2026-11-15T23:00:00Z `f8.ac030.<n>.user@example.com` pauses the goal from 2026-11-15 until 2026-11-20 without a reason
    Then the result is <result>

    Examples:
      | n  | zone        | result                                                             |
      | 01 | UTC         | "paused" with the first day 2026-11-15 and the last day 2026-11-20 |
      | 02 | Europe/Kyiv | "A pause cannot start before today."                               |

  @AC-031 @FR-006 @FR-007 @US2-AS11
  Scenario: Pausing a commitment-based goal makes its commitments in the range Paused
    Given the registered user `f8.ac031.user@example.com`, who created the goal `Yoga, 3 days a week`
    When at 2026-11-18T08:00:00Z `f8.ac031.user@example.com` pauses the goal from 2026-11-18 until 2026-11-20 without a reason
    Then the result is "paused" with the first day 2026-11-18 and the last day 2026-11-20
    And the history of the goal from 2026-11-16 to 2026-11-22 gives 2026-11-16 Pending, 2026-11-18 Paused and 2026-11-20 Paused

  @AC-032 @FR-010 @US2-AS11
  Scenario: Resuming a commitment-based goal makes today's commitment Due again
    Given the registered user `f8.ac032.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-18T08:00:00Z `f8.ac032.user@example.com` paused the goal from 2026-11-18 until 2026-11-20 without a reason
    When at 2026-11-20T08:00:00Z `f8.ac032.user@example.com` resumes the goal
    Then the result is "resumed"
    And the pauses of the goal are (2026-11-18, 2026-11-19, no reason, past)
    And the history of the goal from 2026-11-18 to 2026-11-20 gives 2026-11-18 Paused and 2026-11-20 Due

  @AC-033 @FR-007 @FR-018 @US2-AS12
  Scenario: A per-week period is Paused as a whole while one of its dates is paused
    Given the registered user `f8.ac033.user@example.com`, who created the goal `Yoga, 3 times a week`
    When at 2026-11-18T08:00:00Z `f8.ac033.user@example.com` pauses the goal from 2026-11-19 until 2026-11-25 without a reason
    Then the result is "paused" with the first day 2026-11-19 and the last day 2026-11-25
    And the history of the goal from 2026-11-16 to 2026-11-29 gives 2026-11-16 – 2026-11-22 Paused and 2026-11-23 – 2026-11-29 Paused
    And the pauses of the goal are (2026-11-19, 2026-11-25, no reason, upcoming)
    And the goals overview gives `Yoga, 3 times a week` not paused

  @AC-034 @FR-005 @FR-009 @US2-AS12
  Scenario: A date of a Paused week that is not itself paused cannot be recorded
    Given the registered user `f8.ac034.user@example.com`, who created the goal `Yoga, 3 times a week`
    And at 2026-11-18T08:00:00Z `f8.ac034.user@example.com` paused the goal from 2026-11-19 until 2026-11-25 without a reason
    When at 2026-11-18T09:00:00Z `f8.ac034.user@example.com` records Yes for 2026-11-18
    Then the result is "This week is paused."
    And the history of the goal from 2026-11-16 to 2026-11-22 gives 2026-11-16 – 2026-11-22 Paused, no record

  @AC-035 @FR-010 @US2-AS12
  Scenario: Resuming a per-week goal in a new week leaves the earlier partly paused week Paused
    Given the registered user `f8.ac035.user@example.com`, who created the goal `Yoga, 3 times a week`
    And at 2026-11-18T08:00:00Z `f8.ac035.user@example.com` paused the goal from 2026-11-19 until 2026-11-25 without a reason
    When at 2026-11-23T10:00:00Z `f8.ac035.user@example.com` resumes the goal
    Then the result is "resumed"
    And the pauses of the goal are (2026-11-19, 2026-11-22, no reason, past)
    And the history of the goal from 2026-11-16 to 2026-11-29 gives 2026-11-16 – 2026-11-22 Paused and 2026-11-23 – 2026-11-29 Due

  @AC-036 @FR-007 @FR-013
  Scenario: One paused day exempts its whole week, also when its records reach the target
    Given the registered user `f8.ac036.user@example.com`, who created the goal `Yoga, 3 times a week`
    And at 2026-11-18T09:00:00Z `f8.ac036.user@example.com` recorded Yes for each date from 2026-11-16 to 2026-11-18
    And at 2026-11-18T10:00:00Z `f8.ac036.user@example.com` paused the goal from 2026-11-22 until 2026-11-22 without a reason
    When at 2026-11-25T12:00:00Z `f8.ac036.user@example.com` reads the history of the goal from 2026-11-16 to 2026-11-22
    Then it gives 2026-11-16 – 2026-11-22 Paused, records Yes, Yes, Yes
    And the report of the goal gives the consistency "0% (0 of 2)"

  @AC-037 @FR-007 @FR-010
  Scenario: An excusal under a pause is kept and shows again when the pause is removed
    Given the registered user `f8.ac037.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-16T07:00:00Z `f8.ac037.user@example.com` excused 2026-11-16 with the reason "Ill"
    And at 2026-11-16T08:00:00Z `f8.ac037.user@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
    When at 2026-11-16T09:00:00Z `f8.ac037.user@example.com` resumes the goal
    Then the result is "resumed"
    And the pauses of the goal are empty
    And the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Excused, reason "Ill"

  @AC-038 @FR-008 @US2-AS8
  Scenario: A pause never changes a result that was Pending or final when it was set
    Given the registered user `f8.ac038.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
    And at 2026-11-13T20:00:00Z `f8.ac038.user@example.com` recorded 6 for 2026-11-13
    And at 2026-11-14T20:00:00Z `f8.ac038.user@example.com` recorded 8 for 2026-11-14
    When at 2026-11-16T09:00:00Z `f8.ac038.user@example.com` pauses the goal from 2026-11-16 until 2026-11-22 without a reason
    Then the result is "paused" with the first day 2026-11-16 and the last day 2026-11-22
    And the history of the goal from 2026-11-13 to 2026-11-16 gives 2026-11-13 Failed, 2026-11-14 Pending, 2026-11-15 Pending and 2026-11-16 Paused
    And at 2026-11-18T12:00:00Z the history of the goal from 2026-11-13 to 2026-11-15 gives 2026-11-13 Failed, 2026-11-14 Met and 2026-11-15 Missed

  @AC-039 @FR-008 @FR-009
  Scenario: A Due entry whose records already exceed a Reduce limit becomes Paused and keeps its records
    Given the registered user `f8.ac039.user@example.com`, who created the goal `Sugar, at most 1 sweet a day`
    And at 2026-11-16T08:00:00Z `f8.ac039.user@example.com` recorded 3 for 2026-11-16
    When at 2026-11-16T09:00:00Z `f8.ac039.user@example.com` pauses the goal from 2026-11-16 until 2026-11-16 without a reason
    Then the result is "paused" with the first day 2026-11-16 and the last day 2026-11-16
    And the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Paused, record 3
    And at 2026-11-19T00:00:00Z the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Paused, record 3

  @AC-040 @FR-009 @US2-AS9
  Scenario: A date in a pause cannot be recorded
    Given the registered user `f8.ac040.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
    And at 2026-11-16T09:00:00Z `f8.ac040.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 without a reason
    When at 2026-11-17T10:00:00Z `f8.ac040.user@example.com` records 8 for 2026-11-17
    Then the result is "This goal is paused on this date."
    And the history of the goal from 2026-11-17 to 2026-11-17 gives 2026-11-17 Paused, no record

  @AC-041 @FR-009 @FR-017 @US2-AS10
  Scenario Outline: A record kept on a Paused date can be changed or deleted, and the date stays Paused
    Given the registered user `f8.ac041.<n>.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
    And at 2026-11-16T08:00:00Z `f8.ac041.<n>.user@example.com` recorded 8 for 2026-11-16
    And at 2026-11-16T09:00:00Z `f8.ac041.<n>.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 without a reason
    When at 2026-11-16T10:00:00Z `f8.ac041.<n>.user@example.com` <action>
    Then the result is the state Paused, correction no
    And the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Paused, <records>, correction no

    Examples:
      | n  | action                                | records   |
      | 01 | changes the record of 2026-11-16 to 9 | record 9  |
      | 02 | deletes the record of 2026-11-16      | no record |

  @AC-042 @FR-010 @FR-012 @FR-018 @US2-AS5
  Scenario: Resuming during a pause ends it on the day before today
    Given the registered user `f8.ac042.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
    And at 2026-11-16T09:00:00Z `f8.ac042.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 with the reason "Family visit"
    When at 2026-11-18T08:00:00Z `f8.ac042.user@example.com` resumes the goal
    Then the result is "resumed"
    And the pauses of the goal are (2026-11-16, 2026-11-17, reason "Family visit", past)
    And the history of the goal from 2026-11-16 to 2026-11-18 gives 2026-11-16 Paused, 2026-11-17 Paused and 2026-11-18 Due
    And what can be recorded now contains 2026-11-18 Due
    And the goals overview gives `Drinking water, 8 glasses a day` not paused

  @AC-043 @FR-010 @FR-024 @US2-AS6
  Scenario: Resuming on a pause's first day removes the pause
    Given the registered user `f8.ac043.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
    And at 2026-11-16T08:00:00Z `f8.ac043.user@example.com` recorded 8 for 2026-11-16
    And at 2026-11-16T09:00:00Z `f8.ac043.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 with the reason "Family visit"
    When at 2026-11-16T18:00:00Z `f8.ac043.user@example.com` resumes the goal
    Then the result is "resumed"
    And the pauses of the goal are empty
    And the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Due, record 8
    And at 2026-11-19T00:00:00Z the history of the goal from 2026-11-16 to 2026-11-16 gives 2026-11-16 Met, record 8

  @AC-044 @FR-010
  Scenario Outline: A goal without a current pause cannot be resumed
    Given the registered user `f8.ac044.<n>.user@example.com`, who created the goal `Yoga, 3 times a week`
    And at 2026-11-16T08:00:00Z `f8.ac044.<n>.user@example.com` paused the goal from <first> until <last> without a reason
    When at <time> `f8.ac044.<n>.user@example.com` resumes the goal
    Then the result is "This goal is not paused."
    And the pauses of the goal are (<first>, <last>, no reason, <kind>)

    Examples:
      | n  | first      | last       | time                 | kind     |
      | 01 | 2026-11-19 | 2026-11-20 | 2026-11-16T10:00:00Z | upcoming |
      | 02 | 2026-11-16 | 2026-11-16 | 2026-11-17T10:00:00Z | past     |

  @AC-045 @FR-007 @FR-010 @FR-018
  Scenario: Resuming in the middle of a week leaves that week Paused, while the goal is not paused
    Given the registered user `f8.ac045.user@example.com`, who created the goal `Yoga, 3 times a week`
    And at 2026-11-16T08:00:00Z `f8.ac045.user@example.com` paused the goal from 2026-11-16 until 2026-11-29 without a reason
    When at 2026-11-18T10:00:00Z `f8.ac045.user@example.com` resumes the goal
    Then the result is "resumed"
    And the pauses of the goal are (2026-11-16, 2026-11-17, no reason, past)
    And the history of the goal from 2026-11-16 to 2026-11-29 gives 2026-11-16 – 2026-11-22 Paused and 2026-11-23 – 2026-11-29 Upcoming
    And the goals overview gives `Yoga, 3 times a week` not paused

  @AC-046 @FR-007 @FR-018 @FR-019
  Scenario: A pause later in the current week pauses that week, while the goal is not paused and is counted
    Given the registered user `f8.ac046.user@example.com`, who created the goal `Yoga, 3 times a week`
    And at 2026-11-16T08:00:00Z `f8.ac046.user@example.com` paused the goal from 2026-11-19 until 2026-11-20 without a reason
    When at 2026-11-16T09:00:00Z `f8.ac046.user@example.com` reads the dashboard
    Then the number of goals is 1
    And the Today list contains (`Yoga, 3 times a week`, 2026-11-09 – 2026-11-15, Pending) and no entry for 2026-11-16 – 2026-11-22
    And the summary of `Yoga, 3 times a week` says it is not paused
    And the goals overview gives `Yoga, 3 times a week` not paused

  @AC-047 @FR-011 @FR-024
  Scenario: Cancelling the upcoming pause that paused the current week makes the week Due again
    Given the registered user `f8.ac047.user@example.com`, who created the goal `Yoga, 3 times a week`
    And at 2026-11-16T08:00:00Z `f8.ac047.user@example.com` paused the goal from 2026-11-19 until 2026-11-20 without a reason
    When at 2026-11-16T09:00:00Z `f8.ac047.user@example.com` cancels the pause 2026-11-19 – 2026-11-20
    Then the result is "pause cancelled"
    And the pauses of the goal are empty
    And the history of the goal from 2026-11-16 to 2026-11-22 gives 2026-11-16 – 2026-11-22 Due

  @AC-048 @FR-011 @US2-AS7
  Scenario: An upcoming pause can be cancelled
    Given the registered user `f8.ac048.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
    And at 2026-11-16T08:00:00Z `f8.ac048.user@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
    When at 2026-11-18T10:00:00Z `f8.ac048.user@example.com` cancels the pause 2026-11-23 – 2026-11-27
    Then the result is "pause cancelled"
    And the pauses of the goal are empty

  @AC-049 @FR-011 @US2-AS7
  Scenario Outline: A current or past pause cannot be cancelled
    Given the registered user `f8.ac049.<n>.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
    And at 2026-11-16T09:00:00Z `f8.ac049.<n>.user@example.com` paused the goal from 2026-11-16 until <last> with the reason "Family visit"
    When at 2026-11-18T10:00:00Z `f8.ac049.<n>.user@example.com` cancels the pause 2026-11-16 – <last>
    Then the result is "Only a pause that has not begun can be cancelled. Resume the goal instead."
    And the pauses of the goal are (2026-11-16, <last>, reason "Family visit", <kind>)

    Examples:
      | n  | last       | kind    |
      | 01 | 2026-11-22 | current |
      | 02 | 2026-11-17 | past    |

  @AC-050 @FR-011
  Scenario Outline: Cancelling just before and on a pause's first day
    Given the registered user `f8.ac050.<n>.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
    And at 2026-11-16T08:00:00Z `f8.ac050.<n>.user@example.com` paused the goal from 2026-11-17 until 2026-11-20 without a reason
    When at <time> `f8.ac050.<n>.user@example.com` cancels the pause 2026-11-17 – 2026-11-20
    Then the result is <result>

    Examples:
      | n  | time                 | result                                                                        |
      | 01 | 2026-11-16T23:59:59Z | "pause cancelled"                                                             |
      | 02 | 2026-11-17T00:00:00Z | "Only a pause that has not begun can be cancelled. Resume the goal instead." |

  @AC-051 @FR-011 @FR-021 @US5-AS4
  Scenario Outline: A pause is found only through its own goal
    Given the registered user `f8.ac051.<n>.a@example.com`, who created the goal `Yoga, 3 days a week` and the goal `Drinking water, 8 glasses a day`
    And at 2026-11-16T08:00:00Z `f8.ac051.<n>.a@example.com` paused `Yoga, 3 days a week` from 2026-11-16 until 2026-11-20 without a reason
    And at 2026-11-16T08:01:00Z `f8.ac051.<n>.a@example.com` paused `Yoga, 3 days a week` from 2026-11-23 until 2026-11-27 without a reason
    And the registered user `f8.ac051.<n>.b@example.com`, who created the goal `Drinking water, 8 glasses a day`
    When at 2026-11-16T09:00:00Z `<actor>` cancels the pause <pause> through their goal `Drinking water, 8 glasses a day`
    Then the result is "This pause does not exist."
    And the pauses of `Yoga, 3 days a week` of `f8.ac051.<n>.a@example.com` are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)

    Examples:
      | n  | actor                     | pause                   |
      | 01 | f8.ac051.01.a@example.com | 2026-11-23 – 2026-11-27 |
      | 02 | f8.ac051.02.b@example.com | 2026-11-23 – 2026-11-27 |
      | 03 | f8.ac051.03.a@example.com | 2026-11-16 – 2026-11-20 |
      | 04 | f8.ac051.04.b@example.com | 2026-11-16 – 2026-11-20 |

  @AC-052 @FR-012 @US2-AS13
  Scenario: The pauses of a goal are listed with the latest first day first
    Given the registered user `f8.ac052.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
    And at 2026-11-16T09:00:00Z `f8.ac052.user@example.com` paused the goal from 2026-11-16 until 2026-11-17 with the reason "Ill"
    And at 2026-11-16T09:01:00Z `f8.ac052.user@example.com` paused the goal from 2026-11-19 until 2026-11-22 with the reason "Family visit"
    And at 2026-11-16T09:02:00Z `f8.ac052.user@example.com` paused the goal from 2026-11-25 until 2026-11-26 without a reason
    When at 2026-11-20T10:00:00Z `f8.ac052.user@example.com` reads the pauses of the goal
    Then they are, in this order, (2026-11-25, 2026-11-26, no reason, upcoming), (2026-11-19, 2026-11-22, reason "Family visit", current) and (2026-11-16, 2026-11-17, reason "Ill", past)

  @AC-053 @FR-013 @US3-AS1
  Scenario: Consistency leaves Excused and Paused results out
    Given the registered user `f8.ac053.user@example.com`, who created the goal `Meditation, 10 min daily`
    And at 2026-11-10T08:00:00Z `f8.ac053.user@example.com` excused 2026-11-09 without a reason
    And at 2026-11-13T08:00:00Z `f8.ac053.user@example.com` paused the goal from 2026-11-13 until 2026-11-15 without a reason
    And `f8.ac053.user@example.com` records 10 for each date from 2026-11-02 to 2026-11-16 except 2026-11-09, 2026-11-11, 2026-11-13, 2026-11-14 and 2026-11-15
    When at 2026-11-19T12:00:00Z `f8.ac053.user@example.com` reads the report of the goal
    Then the consistency is "90% (10 of 11)"

  @AC-054 @FR-014 @US3-AS2
  Scenario: Excused and Paused results neither extend nor reset a streak
    Given the registered user `f8.ac054.user@example.com`, who created the goal `Meditation, 10 min daily`
    And at 2026-11-10T08:00:00Z `f8.ac054.user@example.com` excused 2026-11-09 without a reason
    And at 2026-11-13T08:00:00Z `f8.ac054.user@example.com` paused the goal from 2026-11-13 until 2026-11-15 without a reason
    And `f8.ac054.user@example.com` records 10 for each date from 2026-11-02 to 2026-11-16 except 2026-11-09, 2026-11-11, 2026-11-13, 2026-11-14 and 2026-11-15
    When at 2026-11-19T12:00:00Z `f8.ac054.user@example.com` reads the report of the goal
    Then the current streak is "2 commitments"
    And the longest streak is "8 commitments"

  @AC-055 @FR-015 @US3-AS3
  Scenario: Recent results include Excused and Paused results and count each state
    Given the registered user `f8.ac055.user@example.com`, who created the goal `Meditation, 10 min daily`
    And at 2026-11-10T08:00:00Z `f8.ac055.user@example.com` excused 2026-11-09 without a reason
    And at 2026-11-13T08:00:00Z `f8.ac055.user@example.com` paused the goal from 2026-11-13 until 2026-11-15 without a reason
    And `f8.ac055.user@example.com` records 10 for each date from 2026-11-02 to 2026-11-16 except 2026-11-09, 2026-11-11, 2026-11-13, 2026-11-14 and 2026-11-15
    When at 2026-11-19T12:00:00Z `f8.ac055.user@example.com` reads the report of the goal
    Then the recent results are, oldest first: 2026-11-02 to 2026-11-08 each Met; 2026-11-09 Excused; 2026-11-10 Met; 2026-11-11 Missed; 2026-11-12 Met; 2026-11-13, 2026-11-14 and 2026-11-15 each Paused; 2026-11-16 Met; 2026-11-17 and 2026-11-18 each Pending
    And the number of each state among them is Met 10, Missed 1, Excused 1, Paused 3, Pending 2

  @AC-056 @FR-016 @US3-AS4
  Scenario: The trend marks Excused and Paused bars, which have no value
    Given the registered user `f8.ac056.user@example.com`, who created the goal `Meditation, 10 min daily`
    And at 2026-11-10T08:00:00Z `f8.ac056.user@example.com` excused 2026-11-09 without a reason
    And at 2026-11-13T08:00:00Z `f8.ac056.user@example.com` paused the goal from 2026-11-13 until 2026-11-15 without a reason
    And `f8.ac056.user@example.com` records 10 for each date from 2026-11-02 to 2026-11-16 except 2026-11-09, 2026-11-11, 2026-11-13, 2026-11-14 and 2026-11-15
    When at 2026-11-19T12:00:00Z `f8.ac056.user@example.com` reads the report of the goal
    Then the trend is, with the target 10: (2026-11-06, 10), (2026-11-07, 10), (2026-11-08, 10), (2026-11-09, Excused), (2026-11-10, 10), (2026-11-11, no value), (2026-11-12, 10), (2026-11-13, Paused), (2026-11-14, Paused), (2026-11-15, Paused), (2026-11-16, 10), (2026-11-17, no value), (2026-11-18, no value), (2026-11-19, no value)

  @AC-057 @FR-004 @FR-013 @FR-014 @FR-024 @US3-AS5
  Scenario: Removing an excusal and recording the commitment late gives the figures of an on-time record
    Given the registered user `f8.ac057.user@example.com`, who created the goal `Meditation, 10 min daily`
    And at 2026-11-10T08:00:00Z `f8.ac057.user@example.com` excused 2026-11-09 without a reason
    And at 2026-11-10T12:00:00Z `f8.ac057.user@example.com` removed the excusal of 2026-11-09
    And at 2026-11-13T08:00:00Z `f8.ac057.user@example.com` paused the goal from 2026-11-13 until 2026-11-15 without a reason
    And `f8.ac057.user@example.com` records 10 for each date from 2026-11-02 to 2026-11-16 except 2026-11-09, 2026-11-11, 2026-11-13, 2026-11-14 and 2026-11-15
    And `f8.ac057.user@example.com` records 4 for 2026-11-09
    When at 2026-11-19T12:00:00Z `f8.ac057.user@example.com` reads the report of the goal
    Then the consistency is "83% (10 of 12)"
    And the longest streak is "7 commitments"
    And the current streak is "2 commitments"

  @AC-058 @FR-013 @FR-014 @US3-AS6
  Scenario: A goal whose every ended result is exempt has "N/A" and streaks of 0
    Given the registered user `f8.ac058.user@example.com`, who created the goal `Meditation, 10 min daily`
    And at 2026-11-03T08:00:00Z `f8.ac058.user@example.com` excused 2026-11-02 without a reason
    And at 2026-11-03T08:00:00Z `f8.ac058.user@example.com` paused the goal from 2026-11-03 until 2026-11-08 without a reason
    When at 2026-11-09T12:00:00Z `f8.ac058.user@example.com` reads the report of the goal
    Then the consistency is "N/A"
    And the current streak is "0 commitments"
    And the longest streak is "0 commitments"

  @AC-059 @FR-013 @FR-014 @US3-AS7
  Scenario: A Paused week of a per-week goal is not counted and does not break the streak
    Given the registered user `f8.ac059.user@example.com`, who created the goal `Yoga, 3 times a week`
    And at 2026-11-09T08:00:00Z `f8.ac059.user@example.com` paused the goal from 2026-11-09 until 2026-11-15 without a reason
    And `f8.ac059.user@example.com` records Yes for each date from 2026-11-02 to 2026-11-04
    And `f8.ac059.user@example.com` records Yes for each date from 2026-11-16 to 2026-11-18
    When at 2026-11-25T12:00:00Z `f8.ac059.user@example.com` reads the report of the goal
    Then the consistency is "100% (2 of 2)"
    And the current streak is "2 weeks"
    And the longest streak is "2 weeks"

  @AC-060 @FR-016 @US3-AS7
  Scenario: The trend of a per-week goal marks a Paused week
    Given the registered user `f8.ac060.user@example.com`, who created the goal `Yoga, 3 times a week`
    And at 2026-11-09T08:00:00Z `f8.ac060.user@example.com` paused the goal from 2026-11-09 until 2026-11-15 without a reason
    And `f8.ac060.user@example.com` records Yes for each date from 2026-11-02 to 2026-11-04
    And `f8.ac060.user@example.com` records Yes for each date from 2026-11-16 to 2026-11-18
    When at 2026-11-25T12:00:00Z `f8.ac060.user@example.com` reads the report of the goal
    Then the trend is, with the target 3: (2026-11-02 – 2026-11-08, 3), (2026-11-09 – 2026-11-15, Paused), (2026-11-16 – 2026-11-22, 3), (2026-11-23 – 2026-11-29, no value)

  @AC-061 @FR-016
  Scenario: The trend of a per-day goal marks its Paused days
    Given the registered user `f8.ac061.user@example.com`, who created the goal `Drinking water, 8 glasses a day`
    And at 2026-11-16T09:00:00Z `f8.ac061.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 without a reason
    And `f8.ac061.user@example.com` records 8 for each date from 2026-11-02 to 2026-11-15
    When at 2026-11-19T10:00:00Z `f8.ac061.user@example.com` reads the report of the goal
    Then the trend is, with the target 1: (2026-11-06, 1), (2026-11-07, 1), (2026-11-08, 1), (2026-11-09, 1), (2026-11-10, 1), (2026-11-11, 1), (2026-11-12, 1), (2026-11-13, 1), (2026-11-14, 1), (2026-11-15, 1), (2026-11-16, Paused), (2026-11-17, Paused), (2026-11-18, Paused), (2026-11-19, Paused)

  @AC-062 @FR-016
  Scenario: A yes/no week's target and value leave a Paused commitment out
    Given the registered user `f8.ac062.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-16T08:00:00Z `f8.ac062.user@example.com` recorded Yes for 2026-11-16
    And at 2026-11-16T09:00:00Z `f8.ac062.user@example.com` paused the goal from 2026-11-16 until 2026-11-16 without a reason
    And `f8.ac062.user@example.com` records Yes for 2026-11-09, 2026-11-11, 2026-11-13, 2026-11-18 and 2026-11-20
    When at 2026-11-23T10:00:00Z `f8.ac062.user@example.com` reads the report of the goal
    Then the trend is (2026-11-02 – 2026-11-08, no value, target 3), (2026-11-09 – 2026-11-15, 3, target 3), (2026-11-16 – 2026-11-22, 2, target 2), (2026-11-23 – 2026-11-29, no value, target 3)

  @AC-063 @FR-016
  Scenario: A yes/no week whose only record is on a Paused commitment has no value
    Given the registered user `f8.ac063.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-16T08:00:00Z `f8.ac063.user@example.com` recorded Yes for 2026-11-16
    And at 2026-11-16T09:00:00Z `f8.ac063.user@example.com` paused the goal from 2026-11-16 until 2026-11-16 without a reason
    When at 2026-11-23T10:00:00Z `f8.ac063.user@example.com` reads the report of the goal
    Then the trend contains (2026-11-16 – 2026-11-22, no value, target 2)

  @AC-064 @FR-016
  Scenario: A yes/no week whose commitments are all Excused is marked Excused
    Given the registered user `f8.ac064.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-16T07:00:00Z `f8.ac064.user@example.com` excused 2026-11-16 without a reason
    And at 2026-11-18T07:00:00Z `f8.ac064.user@example.com` excused 2026-11-18 without a reason
    And at 2026-11-20T07:00:00Z `f8.ac064.user@example.com` excused 2026-11-20 without a reason
    When at 2026-11-23T10:00:00Z `f8.ac064.user@example.com` reads the report of the goal
    Then the trend contains (2026-11-16 – 2026-11-22, Excused)

  @AC-065 @FR-016
  Scenario: A yes/no week whose commitments are all Excused or Paused, at least one Paused, is marked Paused
    Given the registered user `f8.ac065.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-16T07:00:00Z `f8.ac065.user@example.com` excused 2026-11-16 without a reason
    And at 2026-11-16T08:00:00Z `f8.ac065.user@example.com` paused the goal from 2026-11-17 until 2026-11-22 without a reason
    When at 2026-11-23T10:00:00Z `f8.ac065.user@example.com` reads the report of the goal
    Then the trend contains (2026-11-16 – 2026-11-22, Paused)

  @AC-066 @FR-018 @US4-AS1
  Scenario: The goals overview says which goal is paused and until when
    Given the registered user `f8.ac066.user@example.com`, who created the goal `Drinking water, 8 glasses a day` and the goal `Meditation, 10 min daily`
    And at 2026-11-16T09:00:00Z `f8.ac066.user@example.com` paused `Drinking water, 8 glasses a day` from 2026-11-16 until 2026-11-22 without a reason
    And `f8.ac066.user@example.com` records on target for each date from 2026-11-02 to 2026-11-13: 8 for `Drinking water, 8 glasses a day` and 10 for `Meditation, 10 min daily`
    When at 2026-11-16T10:00:00Z `f8.ac066.user@example.com` reads the goals overview
    Then it gives `Drinking water, 8 glasses a day` paused until 2026-11-22
    And it gives `Meditation, 10 min daily` not paused

  @AC-067 @FR-019 @US4-AS2
  Scenario: The dashboard expects nothing of a paused goal and does not count it
    Given the registered user `f8.ac067.user@example.com`, who created the goal `Drinking water, 8 glasses a day` and the goal `Meditation, 10 min daily`
    And at 2026-11-16T09:00:00Z `f8.ac067.user@example.com` paused `Drinking water, 8 glasses a day` from 2026-11-16 until 2026-11-22 without a reason
    And `f8.ac067.user@example.com` records on target for each date from 2026-11-02 to 2026-11-13: 8 for `Drinking water, 8 glasses a day` and 10 for `Meditation, 10 min daily`
    When at 2026-11-16T10:00:00Z `f8.ac067.user@example.com` reads the dashboard
    Then the number of goals is 1
    And the Due count is 1 and the Pending count is 4
    And the Today list contains (`Drinking water, 8 glasses a day`, 2026-11-14, Pending), (`Drinking water, 8 glasses a day`, 2026-11-15, Pending) and (`Meditation, 10 min daily`, 2026-11-16, Due)
    And the Today list contains no Due and no Upcoming entry of `Drinking water, 8 glasses a day`
    And the summary of `Drinking water, 8 glasses a day` says it is paused, and the summary of `Meditation, 10 min daily` says it is not paused

  @AC-068 @FR-019 @US4-AS3
  Scenario: An excused commitment leaves the Pending count and the Today list
    Given the registered user `f8.ac068.user@example.com`, who created the goal `Drinking water, 8 glasses a day` and the goal `Meditation, 10 min daily`
    And at 2026-11-16T09:00:00Z `f8.ac068.user@example.com` paused `Drinking water, 8 glasses a day` from 2026-11-16 until 2026-11-22 without a reason
    And `f8.ac068.user@example.com` records on target for each date from 2026-11-02 to 2026-11-13: 8 for `Drinking water, 8 glasses a day` and 10 for `Meditation, 10 min daily`
    And at 2026-11-16T10:00:00Z `f8.ac068.user@example.com` excused 2026-11-15 of `Meditation, 10 min daily` without a reason
    When at 2026-11-16T10:01:00Z `f8.ac068.user@example.com` reads the dashboard
    Then the Pending count is 3
    And the Today list contains (`Meditation, 10 min daily`, 2026-11-14, Pending) and no entry of `Meditation, 10 min daily` for 2026-11-15

  @AC-069 @FR-019 @US4-AS4
  Scenario: The Upcoming entry skips a Paused commitment
    Given the registered user `f8.ac069.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-16T08:00:00Z `f8.ac069.user@example.com` paused the goal from 2026-11-18 until 2026-11-18 without a reason
    When at 2026-11-17T10:00:00Z `f8.ac069.user@example.com` reads the dashboard
    Then the Today list contains (`Yoga, 3 days a week`, 2026-11-16, Pending) and (`Yoga, 3 days a week`, 2026-11-20, Upcoming)
    And the Today list contains no entry for 2026-11-18
    And the number of goals is 1

  @AC-070 @FR-019
  Scenario: An Excused commitment whose grace period would have ended this week is not Missed
    Given the registered user `f8.ac070.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-13T07:00:00Z `f8.ac070.user@example.com` excused 2026-11-13 without a reason
    When at 2026-11-18T10:00:00Z `f8.ac070.user@example.com` reads the dashboard
    Then the Missed count is 0
    And the Today list contains (`Yoga, 3 days a week`, 2026-11-16, Pending) and (`Yoga, 3 days a week`, 2026-11-18, Due) and no entry for 2026-11-13

  @AC-071 @FR-020
  Scenario: Recent notes show Excused and Paused as the state
    Given the registered user `f8.ac071.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-13T07:00:00Z `f8.ac071.user@example.com` excused 2026-11-13 without a reason
    And at 2026-11-13T08:00:00Z `f8.ac071.user@example.com` saved the note "Travel" for 2026-11-13
    And at 2026-11-16T08:00:00Z `f8.ac071.user@example.com` paused the goal from 2026-11-16 until 2026-11-16 without a reason
    And at 2026-11-16T09:00:00Z `f8.ac071.user@example.com` saved the note "Visit" for 2026-11-16
    When at 2026-11-16T10:00:00Z `f8.ac071.user@example.com` reads the recent notes of the goal
    Then they are, in this order, (2026-11-16, "Visit", Paused) and (2026-11-13, "Travel", Excused)
    And the dashboard's recent notes are, in this order, (`Yoga, 3 days a week`, 2026-11-16, "Visit", Paused) and (`Yoga, 3 days a week`, 2026-11-13, "Travel", Excused)

  @AC-072 @FR-021 @US5-AS1
  Scenario Outline: Another registered user cannot use any use case of this slice on someone else's goal
    Given the registered user `f8.ac072.<n>.a@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-12T10:00:00Z `f8.ac072.<n>.a@example.com` excused 2026-11-11 with the reason "Ill"
    And at 2026-11-16T08:00:00Z `f8.ac072.<n>.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
    And at 2026-11-16T08:01:00Z `f8.ac072.<n>.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
    And the registered user `f8.ac072.<n>.b@example.com`
    When at 2026-11-16T10:00:00Z `f8.ac072.<n>.b@example.com` <action> on the goal of `f8.ac072.<n>.a@example.com`
    Then the result is "This goal does not exist."
    And the pauses of the goal, read by `f8.ac072.<n>.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
    And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac072.<n>.a@example.com`, gives 2026-11-11 Excused, reason "Ill"

    Examples:
      | n  | action                                                            |
      | 01 | excuses 2026-11-18 without a reason                               |
      | 02 | removes the excusal of 2026-11-11                                 |
      | 03 | pauses the goal from 2026-11-28 until 2026-11-29 without a reason |
      | 04 | resumes the goal                                                  |
      | 05 | cancels the pause 2026-11-23 – 2026-11-27                         |
      | 06 | reads the pauses of the goal                                      |

  @AC-073 @FR-022 @US5-AS2
  Scenario Outline: An administrator cannot use any use case of this slice
    Given the registered user `f8.ac073.<n>.a@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-12T10:00:00Z `f8.ac073.<n>.a@example.com` excused 2026-11-11 with the reason "Ill"
    And at 2026-11-16T08:00:00Z `f8.ac073.<n>.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
    And at 2026-11-16T08:01:00Z `f8.ac073.<n>.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
    And the administrator `f8.ac073.<n>.admin@example.com`
    When at 2026-11-16T10:00:00Z `f8.ac073.<n>.admin@example.com` <action> on the goal of `f8.ac073.<n>.a@example.com`
    Then the result is "Goals are for registered users."
    And the pauses of the goal, read by `f8.ac073.<n>.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
    And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac073.<n>.a@example.com`, gives 2026-11-11 Excused, reason "Ill"

    Examples:
      | n  | action                                                            |
      | 01 | excuses 2026-11-18 without a reason                               |
      | 02 | removes the excusal of 2026-11-11                                 |
      | 03 | pauses the goal from 2026-11-28 until 2026-11-29 without a reason |
      | 04 | resumes the goal                                                  |
      | 05 | cancels the pause 2026-11-23 – 2026-11-27                         |
      | 06 | reads the pauses of the goal                                      |

  @AC-074 @FR-022 @US5-AS3
  Scenario Outline: An anonymous visitor cannot use any use case of this slice
    Given the registered user `f8.ac074.<n>.a@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-12T10:00:00Z `f8.ac074.<n>.a@example.com` excused 2026-11-11 with the reason "Ill"
    And at 2026-11-16T08:00:00Z `f8.ac074.<n>.a@example.com` paused the goal from 2026-11-16 until 2026-11-20 without a reason
    And at 2026-11-16T08:01:00Z `f8.ac074.<n>.a@example.com` paused the goal from 2026-11-23 until 2026-11-27 without a reason
    When at 2026-11-16T10:00:00Z the anonymous visitor <action> on the goal of `f8.ac074.<n>.a@example.com`
    Then the result is "You are not signed in."
    And the pauses of the goal, read by `f8.ac074.<n>.a@example.com`, are (2026-11-23, 2026-11-27, no reason, upcoming) and (2026-11-16, 2026-11-20, no reason, current)
    And the history of the goal from 2026-11-11 to 2026-11-11, read by `f8.ac074.<n>.a@example.com`, gives 2026-11-11 Excused, reason "Ill"

    Examples:
      | n  | action                                                            |
      | 01 | excuses 2026-11-18 without a reason                               |
      | 02 | removes the excusal of 2026-11-11                                 |
      | 03 | pauses the goal from 2026-11-28 until 2026-11-29 without a reason |
      | 04 | resumes the goal                                                  |
      | 05 | cancels the pause 2026-11-23 – 2026-11-27                         |
      | 06 | reads the pauses of the goal                                      |

  @AC-075 @FR-023
  Scenario: A goal without excusals or pauses keeps slice 6's figures and is counted
    Given the registered user `f8.ac075.user@example.com`, who created the goal `Meditation, 10 min daily` at 2026-11-01T08:00:00Z with the start date 2026-11-01
    And `f8.ac075.user@example.com` records 10 for each date from 2026-11-01 to 2026-11-20 except 2026-11-05 and 2026-11-09
    And `f8.ac075.user@example.com` records 4 for 2026-11-05
    When at 2026-11-23T10:00:00Z `f8.ac075.user@example.com` reads the report of the goal
    Then the consistency is "90% (18 of 20)"
    And the current streak is "11 commitments" and the longest streak is "11 commitments"
    And the goals overview gives `Meditation, 10 min daily` not paused
    And the dashboard's number of goals is 1

  @AC-076 @FR-031 @US6-AS1
  Scenario: Demo pause loading creates `demo6@example.com` and names no private data in its results
    Given the earlier demo tasks run at 2026-11-18T10:00:00Z
    When demo pause loading runs at 2026-11-18T10:00:00Z with the demo password `example2026`
    Then its results are, in this order, (demo6@example.com, created), (demo6@example.com, demo goals added) and (demo6@example.com, demo records added)
    And no result contains "Meditation", "Drinking water", "Travelling.", "Ill with flu.", "Family visit.", "Overslept.", "Busy day.", "glasses" or "min"
    And sign-in with `demo6@example.com` and `example2026` succeeds

  @AC-077 @FR-031 @US6-AS2
  Scenario Outline: The demo figures of `demo6@example.com` hold on every weekday
    Given the demo run at <time>
    When at <time> `demo6@example.com` reads the report of `Meditation, 10 min daily` and the report of `Drinking water, 8 glasses a day`
    Then `Meditation, 10 min daily` has the consistency "95% (22 of 23)", the current streak "10 commitments" and the longest streak "12 commitments"
    And `Drinking water, 8 glasses a day` has the consistency "94% (18 of 19)", the current streak "2 days" and the longest streak "16 days"
    And the goals overview gives `Drinking water, 8 glasses a day` paused until <last> and `Meditation, 10 min daily` not paused
    And the dashboard gives the Due count 1, the Pending count 2, the Missed count 0 and the number of goals 1

    Examples:
      | n  | time                 | last       |
      | 01 | 2026-11-16T10:00:00Z | 2026-11-21 |
      | 02 | 2026-11-17T10:00:00Z | 2026-11-22 |
      | 03 | 2026-11-18T10:00:00Z | 2026-11-23 |
      | 04 | 2026-11-19T10:00:00Z | 2026-11-24 |
      | 05 | 2026-11-20T10:00:00Z | 2026-11-25 |
      | 06 | 2026-11-21T10:00:00Z | 2026-11-26 |
      | 07 | 2026-11-22T10:00:00Z | 2026-11-27 |

  @AC-078 @FR-031 @US6-AS2
  Scenario: The demo excusals, pauses, states and notes of `demo6@example.com`
    Given the demo run at 2026-11-18T10:00:00Z
    When at 2026-11-18T10:00:00Z `demo6@example.com` reads the history of `Meditation, 10 min daily` from 2026-10-21 to 2026-11-18
    Then it gives 2026-10-29 Excused, reason "Travelling."; 2026-10-30 Excused, reason "Travelling."; 2026-11-04 Missed, no record; 2026-11-10 Excused, no reason; 2026-11-15 Met, record 10; 2026-11-16 Excused, no reason; 2026-11-17 Pending, no record; 2026-11-18 Due, no record
    And the recent notes of `Meditation, 10 min daily` are (2026-11-04, "Overslept.", Missed)
    And the history of `Drinking water, 8 glasses a day` from 2026-10-30 to 2026-11-18 gives 2026-10-30 Met, record 8; 2026-10-31 to 2026-11-06 each Paused, no record; 2026-11-07 Met, record 8; 2026-11-13 Failed, record 6; 2026-11-16 Pending, record 8; 2026-11-17 Paused, no record; 2026-11-18 Paused, no record
    And the recent notes of `Drinking water, 8 glasses a day` are (2026-11-13, "Busy day.", Failed)
    And the pauses of `Drinking water, 8 glasses a day` are, in this order, (2026-11-17, 2026-11-23, reason "Family visit.", current) and (2026-10-31, 2026-11-06, reason "Ill with flu.", past)

  @AC-079 @FR-031 @US6-AS3
  Scenario: Demo pause loading leaves an existing `demo6@example.com` unchanged
    Given the demo run at 2026-11-18T10:00:00Z
    When demo pause loading runs at 2026-11-19T10:00:00Z with the demo password `example2026`
    Then its results are (demo6@example.com, already exists)
    And at 2026-11-19T10:00:00Z the pauses of `Drinking water, 8 glasses a day` of `demo6@example.com` are, in this order, (2026-11-17, 2026-11-23, reason "Family visit.", current) and (2026-10-31, 2026-11-06, reason "Ill with flu.", past)
    And at 2026-11-19T10:00:00Z the goals overview of `demo6@example.com` lists exactly `Meditation, 10 min daily` and `Drinking water, 8 glasses a day`

  @AC-080 @FR-031 @US6-AS4
  Scenario Outline: Demo pause loading without a valid demo password creates nothing
    Given the earlier demo tasks run at 2026-11-18T10:00:00Z
    When demo pause loading runs at 2026-11-18T10:00:00Z <password>
    Then its results are (<result>)
    And sign-in with `demo6@example.com` and `example2026` does not succeed

    Examples:
      | n  | password                     | result                                        |
      | 01 | with no demo password        | no demo password configured                   |
      | 02 | with the demo password `abc` | demo password does not meet the password rule |

  @AC-081 @FR-031
  Scenario Outline: A demo goal whose catalog habit is not Published is left out
    Given the earlier demo tasks run at 2026-11-18T10:00:00Z
    And the administrator `f8.ac081.<n>.admin@example.com`
    And `f8.ac081.<n>.admin@example.com` unpublished <habits> with confirmation
    When demo pause loading runs at 2026-11-18T10:00:00Z with the demo password `example2026`
    Then its results are, in this order, (demo6@example.com, created), (demo6@example.com, demo goals added) and (demo6@example.com, demo records added)
    And at 2026-11-18T10:00:00Z the goals overview of `demo6@example.com` lists exactly <goals>

    Examples:
      | n  | habits                                               | goals                      |
      | 01 | the catalog habit "Drinking water"                   | `Meditation, 10 min daily` |
      | 02 | the catalog habits "Meditation" and "Drinking water" | no goal                    |

  @AC-082 @FR-030 @US6-AS5
  Scenario: The earlier demo accounts get no excusal and no pause
    Given the demo run at 2026-11-18T10:00:00Z
    When at 2026-11-18T10:00:00Z each account that a result of demo loading, demo history loading or demo report loading gives as created, `demo1@example.com`, `demo4@example.com` and `demo5@example.com` included, signs in and reads the pauses and the history from 2026-08-01 to 2026-11-18 of each of its goals
    Then no goal of those accounts has a pause
    And no history entry of those goals is Excused or Paused

  @AC-083 @FR-016
  Scenario: A yes/no week's target leaves an Excused commitment out
    Given the registered user `f8.ac083.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-16T07:00:00Z `f8.ac083.user@example.com` excused 2026-11-16 without a reason
    And `f8.ac083.user@example.com` records Yes for 2026-11-18 and 2026-11-20
    When at 2026-11-23T10:00:00Z `f8.ac083.user@example.com` reads the report of the goal
    Then the trend contains (2026-11-16 – 2026-11-22, 2, target 2)

  @AC-084 @FR-002
  Scenario: A date in a pause that is not a commitment is refused as not scheduled
    Given the registered user `f8.ac084.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-16T08:00:00Z `f8.ac084.user@example.com` paused the goal from 2026-11-16 until 2026-11-22 without a reason
    When at 2026-11-17T10:00:00Z `f8.ac084.user@example.com` excuses 2026-11-17 without a reason
    Then the result is "Nothing is scheduled on this date."
    And the history of the goal from 2026-11-16 to 2026-11-18 gives 2026-11-16 Paused and 2026-11-18 Paused, and no entry for 2026-11-17

  @AC-085 @FR-019
  Scenario: An Excused Due commitment is not counted or listed as Due
    Given the registered user `f8.ac085.user@example.com`, who created the goal `Yoga, 3 days a week`
    And at 2026-11-13T07:00:00Z `f8.ac085.user@example.com` excused 2026-11-13 without a reason
    When at 2026-11-13T08:00:00Z `f8.ac085.user@example.com` reads the dashboard
    Then the Due count is 0 and the Pending count is 1
    And the Today list contains (`Yoga, 3 days a week`, 2026-11-11, Pending) and no entry for 2026-11-13
    And the number of goals is 1

  @AC-086 @FR-012
  Scenario Outline: For a goal that has not started, a pause is current or past by the account's today
    Given the registered user `f8.ac086.<n>.user@example.com`, who created the goal `Drinking water, 8 glasses a day` at 2026-11-16T08:00:00Z with the start date 2026-11-20
    And at 2026-11-16T09:00:00Z `f8.ac086.<n>.user@example.com` paused the goal from 2026-11-16 until 2026-11-18 without a reason
    When at <time> `f8.ac086.<n>.user@example.com` reads the pauses of the goal
    Then they are (2026-11-16, 2026-11-18, no reason, <kind>)

    Examples:
      | n  | time                 | kind    |
      | 01 | 2026-11-16T10:00:00Z | current |
      | 02 | 2026-11-19T10:00:00Z | past    |

  @AC-087 @FR-010
  Scenario: A goal that has not started can be resumed from its current pause
    Given the registered user `f8.ac087.user@example.com`, who created the goal `Drinking water, 8 glasses a day` at 2026-11-16T08:00:00Z with the start date 2026-11-20
    And at 2026-11-16T09:00:00Z `f8.ac087.user@example.com` paused the goal from 2026-11-16 until 2026-11-18 without a reason
    When at 2026-11-17T10:00:00Z `f8.ac087.user@example.com` resumes the goal
    Then the result is "resumed"
    And the pauses of the goal are (2026-11-16, 2026-11-16, no reason, past)

  @AC-088 @FR-006 @FR-014
  Scenario: A pause of several months is allowed and leaves the streak before it unchanged
    Given the registered user `f8.ac088.user@example.com`, who created the goal `Meditation, 10 min daily` at 2026-08-03T08:00:00Z with the start date 2026-08-03
    And at 2026-08-07T20:00:00Z `f8.ac088.user@example.com` recorded 10 for each date from 2026-08-03 to 2026-08-07
    And at 2026-08-08T08:00:00Z `f8.ac088.user@example.com` paused the goal from 2026-08-08 until 2026-11-15 without a reason, with the result "paused" with the first day 2026-08-08 and the last day 2026-11-15
    When at 2026-11-16T10:00:00Z `f8.ac088.user@example.com` reads the report of the goal
    Then the current streak is "5 commitments" and the longest streak is "5 commitments"
    And the pauses of the goal are (2026-08-08, 2026-11-15, no reason, past)
