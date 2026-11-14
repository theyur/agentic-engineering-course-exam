@F002
Feature: Habit catalog

  @AC-001 @FR-008 @US1-AS1
  Scenario Outline: Browsing returns only Published habits, with their card content, to every audience
    Given the administrator `ac001.admin.<n>@example.com` and the registered user `ac001.user.<n>@example.com`
    And a Published `A001 <n> Reading` with the category Mind, the intent Build, the icon `book-open`, the description "Read books and articles." and the suggested starting goal Build, count 20 `pages`, 3 times per week
    And a Published `A001 <n> Yoga` with the category Body, the intent Build and the icon `person-standing`
    And a Draft `A001 <n> Journaling` and an Unpublished `A001 <n> Stretching`
    When <acting user> browses the catalog
    Then the result contains `A001 <n> Reading` with the category Mind, the intent Build, the icon `book-open`, the description "Read books and articles." and the suggested starting goal with the text "20 pages, 3 times a week" and the kind "Count · flexible"
    And the result contains `A001 <n> Yoga` with the category Body, the intent Build, the icon `person-standing`, no description and no suggested starting goal
    And the result does not contain `A001 <n> Journaling` or `A001 <n> Stretching`

    Examples:
      | n  | acting user                                     |
      | 01 | the anonymous visitor                           |
      | 02 | the registered user `ac001.user.02@example.com` |
      | 03 | the administrator `ac001.admin.03@example.com`  |

  @AC-002 @FR-004 @FR-008 @US1-AS2
  Scenario: Browsing orders habits by name, ignoring case
    Given the administrator `ac002.admin@example.com`
    And a Published `A002 walking`, a Published `A002 Breathing` and a Published `A002 apple`, created in this order
    When the anonymous visitor browses the catalog with the search `A002`
    Then the result is `A002 apple`, `A002 Breathing`, `A002 walking`, in this order

  @AC-003 @FR-009 @US1-AS3
  Scenario: Browsing by category returns only that category
    Given the administrator `ac003.admin@example.com`
    And a Published `A003 Saving` with the category Money and a Published `A003 Running` with the category Body
    When the anonymous visitor browses the catalog with the category Money
    Then the result contains `A003 Saving` and does not contain `A003 Running`

  @AC-004 @FR-009 @US1-AS4
  Scenario Outline: Browsing by intent returns only that intent
    Given the administrator `ac004.admin.<n>@example.com`
    And a Published `A004 <n> Sugar` with the category Health and the intent Reduce, and a Published `A004 <n> Water` with the category Health and the intent Build
    When the anonymous visitor browses the catalog with the intent <intent>
    Then the result <Sugar> `A004 <n> Sugar` and <Water> `A004 <n> Water`

    Examples:
      | n  | intent | Sugar            | Water            |
      | 01 | Reduce | contains         | does not contain |
      | 02 | Build  | does not contain | contains         |
      | 03 | both   | contains         | contains         |

  @AC-005 @FR-009 @US1-AS5
  Scenario Outline: Search matches part of the name, trimmed and ignoring case
    Given the administrator `ac005.admin.<n>@example.com`
    And a Published `A005 <n> Morning pages` and a Published `A005 <n> Evening walk`
    When the anonymous visitor browses the catalog with the search <search>
    Then the result <Morning> `A005 <n> Morning pages` and <Evening> `A005 <n> Evening walk`

    Examples:
      | n  | search               | Morning          | Evening          |
      | 01 | `"  a005 01 morn  "` | contains         | does not contain |
      | 02 | `"MORNING"`          | contains         | does not contain |
      | 03 | `"ning pa"`          | contains         | does not contain |
      | 04 | `"a005 04 e"`        | does not contain | contains         |
      | 05 | `""`                 | contains         | contains         |
      | 06 | `"   "`              | contains         | contains         |

  @AC-006 @FR-024 @US1-AS6
  Scenario Outline: Custom habits never appear in browsing, for any audience
    Given the administrator `ac006.admin.<n>@example.com` and a Published `A006 <n> Cold water`
    And the registered user `ac006.owner.<n>@example.com`, who created the custom habit `A006 <n> Cold shower`
    And the registered user `ac006.other.<n>@example.com`
    When <acting user> browses the catalog with the search `A006 <n>`
    Then the result contains `A006 <n> Cold water` and does not contain `A006 <n> Cold shower`

    Examples:
      | n  | acting user                                      |
      | 01 | the registered user `ac006.owner.01@example.com` |
      | 02 | the registered user `ac006.other.02@example.com` |
      | 03 | the administrator `ac006.admin.03@example.com`   |
      | 04 | the anonymous visitor                            |

  @AC-007 @FR-008 @FR-009 @US1-AS7
  Scenario: Browsing with no match gives an empty result
    Given the administrator `ac007.admin@example.com` and a Published `A007 Rowing` with the category Body
    When the anonymous visitor browses the catalog with the category Money and the search `A007`
    Then the result has no habit and the count 0

  @AC-008 @FR-002 @FR-009 @US1-AS8
  Scenario: The category filter starts with All and follows the fixed order
    Given the administrator `ac008.admin@example.com`
    And a Published `A008 Budget` with the category Money and a Published `A008 Lifting` with the category Body
    When the anonymous visitor asks for the category filter
    Then its first entry is "All"
    And it contains "Body" and "Money", with "Body" before "Money"

  @AC-009 @FR-002 @FR-009 @FR-024 @US1-AS9
  Scenario Outline: The category filter counts only Published catalog habits
    Given the AC database is reset, so no account and no habit exists
    And the administrator `ac009.admin.<n>@example.com`
    And a Published `A009 <n> Lifting` with the category Body, a Draft `A009 <n> Tidying` with the category Home and an Unpublished `A009 <n> Budget` with the category Money
    And the registered user `ac009.user.<n>@example.com`, who created the custom habit `A009 <n> Plan` with the category Mind
    When <acting user> asks for the category filter
    Then it is exactly "All", "Body", in this order

    Examples:
      | n  | acting user                                     |
      | 01 | the anonymous visitor                           |
      | 02 | the registered user `ac009.user.02@example.com` |
      | 03 | the administrator `ac009.admin.03@example.com`  |

  @AC-010 @FR-009
  Scenario: Category, intent and search apply together
    Given the administrator `ac010.admin@example.com`
    And a Published `A010 Night walk` with the category Body and the intent Build
    And a Published `A010 Night snacks` with the category Body and the intent Reduce
    And a Published `A010 Day walk` with the category Body and the intent Build
    And a Published `A010 Night reading` with the category Mind and the intent Build
    When the anonymous visitor browses the catalog with the category Body, the intent Build and the search `A010 night`
    Then the result is exactly `A010 Night walk`

  @AC-011 @FR-008
  Scenario: Browsing gives the count of the habits it returns
    Given the administrator `ac011.admin@example.com`
    And a Published `A011 One`, a Published `A011 Two`, a Published `A011 Three` and a Draft `A011 Four`
    When the anonymous visitor browses the catalog with the search `A011`
    Then the result has the 3 habits `A011 One`, `A011 Three`, `A011 Two`, in this order, and the count 3

  @AC-012 @FR-008 @FR-024 @US5-AS2
  Scenario: The Home preview is the first four Published habits in name order
    Given the AC database is reset, so no account and no habit exists
    And the administrator `ac012.admin@example.com`
    And a Published `A012 Eta`, a Published `A012 Beta`, a Published `A012 Delta`, a Published `A012 Alpha` and a Published `A012 Gamma`
    And a Draft `A012 Aardvark` and an Unpublished `A012 Abacus`
    And the registered user `ac012.user@example.com`, who created the custom habit `A012 Aaa private`
    When the anonymous visitor asks for the Home preview
    Then it is exactly `A012 Alpha`, `A012 Beta`, `A012 Delta`, `A012 Eta`, in this order

  @AC-013 @FR-001 @FR-010 @FR-017 @US2-AS1
  Scenario: Details give every suggested starting goal in order, with its text and kind
    Given the administrator `ac013.admin@example.com`
    And a Published `A013 Reading` with the category Mind, the intent Build, the icon `book-open`, the description "Read books and articles." and the suggested starting goals, in this order: (1) Build, duration 15 minutes, fixed days Monday, Tuesday, Wednesday, Thursday and Friday; (2) Build, count 20 `pages`, 3 times per week (marked); (3) Build, yes/no, 1 time per day
    When the anonymous visitor reads the details of `A013 Reading`
    Then the result has the name `A013 Reading`, the category Mind, the intent Build, the icon `book-open` and the description "Read books and articles."
    And it has three suggested starting goals in this order, with the fields given above, and the second is the marked one
    And their texts are "15 min, 5 days a week", "20 pages, 3 times a week" and "once a day"
    And their kinds are "Duration · fixed days Mon–Fri", "Count · flexible" and "Yes/no · flexible"

  @AC-014 @FR-008 @FR-010 @US2-AS2
  Scenario: A habit without suggested starting goals is shown without one
    Given the administrator `ac014.admin@example.com` and a Published `A014 Walking` with no suggested starting goal
    When the anonymous visitor browses the catalog with the search `A014` and reads the details of `A014 Walking`
    Then browsing gives `A014 Walking` with no suggested starting goal
    And the details give `A014 Walking` with no suggested starting goal

  @AC-015 @FR-017 @US2-AS3
  Scenario Outline: The text and kind of a suggested starting goal follow fixed rules
    Given the administrator `ac015.admin.<n>@example.com`
    And a Published `A015 <n>` whose only suggested starting goal is <fields>
    When the anonymous visitor reads the details of `A015 <n>`
    Then the suggested starting goal has the text <text> and the kind <kind>

    Examples:
      | n  | fields                                                                            | text                             | kind                                   |
      | 01 | Build, duration 30 minutes, fixed days Monday to Sunday (all seven)               | "30 min daily"                   | "Duration · fixed days every day"      |
      | 02 | Reduce, duration 2 hours, 1 time per day                                          | "at most 2 h a day"              | "Duration · flexible"                  |
      | 03 | Reduce, count 1 `sweet`, 1 time per day                                           | "at most 1 sweet a day"          | "Count · flexible"                     |
      | 04 | Build, yes/no, 1 time per week                                                    | "once a week"                    | "Yes/no · flexible"                    |
      | 05 | Build, duration 1 hours, fixed days Monday, Wednesday and Friday                  | "1 h, 3 days a week"             | "Duration · fixed days Mon, Wed, Fri"  |
      | 06 | Build, count 10 `pages`, fixed day Saturday                                       | "10 pages, once a week"          | "Count · fixed days Sat"               |
      | 07 | Reduce, yes/no, 1 time per day                                                    | "at most once a day"             | "Yes/no · flexible"                    |
      | 08 | Build, yes/no, fixed days Monday, Tuesday, Wednesday and Saturday                 | "4 days a week"                  | "Yes/no · fixed days Mon–Wed, Sat"     |
      | 09 | Build, yes/no, fixed days Saturday and Sunday                                     | "2 days a week"                  | "Yes/no · fixed days Sat, Sun"         |
      | 10 | Build, count 2 `glasses`, 3 times per day                                         | "2 glasses, 3 times a day"       | "Count · flexible"                     |
      | 11 | Build, yes/no, 2 times per day                                                    | "2 times a day"                  | "Yes/no · flexible"                    |
      | 12 | Build, duration 20 minutes, 1 time per day                                        | "20 min a day"                   | "Duration · flexible"                  |
      | 13 | Build, yes/no, fixed days Monday to Saturday                                      | "6 days a week"                  | "Yes/no · fixed days Mon–Sat"          |
      | 14 | Build, yes/no, fixed days Monday, Tuesday and Sunday                              | "3 days a week"                  | "Yes/no · fixed days Mon, Tue, Sun"    |
      | 15 | Build, yes/no, fixed days Monday, Tuesday, Wednesday, Friday, Saturday and Sunday | "6 days a week"                  | "Yes/no · fixed days Mon–Wed, Fri–Sun" |
      | 16 | Reduce, count 5 `cups`, 2 times per week                                          | "at most 5 cups, 2 times a week" | "Count · flexible"                     |

  @AC-016 @FR-010 @US2-AS4
  Scenario Outline: Details of a habit that is not Published give the same refusal to every audience
    Given the administrator `ac016.admin.<n>@example.com`, a Draft `A016 <n> Draft habit` and an Unpublished `A016 <n> Old habit`
    And the registered user `ac016.user.<n>@example.com`
    When <acting user> reads the catalog details of <habit>
    Then the result is "This habit is not in the catalog."

    Examples:
      | n  | acting user                                     | habit                 |
      | 01 | the anonymous visitor                           | `A016 01 Draft habit` |
      | 02 | the anonymous visitor                           | `A016 02 Old habit`   |
      | 03 | the anonymous visitor                           | an unknown identifier |
      | 04 | the registered user `ac016.user.04@example.com` | `A016 04 Draft habit` |
      | 05 | the registered user `ac016.user.05@example.com` | `A016 05 Old habit`   |
      | 06 | the registered user `ac016.user.06@example.com` | an unknown identifier |
      | 07 | the administrator `ac016.admin.07@example.com`  | `A016 07 Draft habit` |
      | 08 | the administrator `ac016.admin.08@example.com`  | `A016 08 Old habit`   |
      | 09 | the administrator `ac016.admin.09@example.com`  | an unknown identifier |

  @AC-017 @FR-010 @FR-024 @US2-AS5
  Scenario Outline: Details with a custom habit's identifier give "not in the catalog"
    Given the registered user `ac017.owner.<n>@example.com`, who created the custom habit `Private one`
    And the administrator `ac017.admin.<n>@example.com`
    When <acting user> reads the catalog details with the identifier of `Private one`
    Then the result is "This habit is not in the catalog."

    Examples:
      | n  | acting user                                      |
      | 01 | the registered user `ac017.owner.01@example.com` |
      | 02 | the administrator `ac017.admin.02@example.com`   |
      | 03 | the anonymous visitor                            |

  @AC-018 @FR-011 @FR-012 @US3-AS1
  Scenario: A new catalog habit is a Draft that the public catalog does not show
    Given the administrator `ac018.admin@example.com`
    When the administrator creates the catalog habit `A018 Stretching` with the category Body, the intent Build, the icon `person-standing`, the description "Gentle stretches." and the suggested starting goal Build, duration 10 minutes, 1 time per day
    Then the result is success
    And the administrator's list contains `A018 Stretching` with the state Draft
    And browsing does not contain `A018 Stretching`

  @AC-019 @FR-011 @US3-AS2
  Scenario: Publishing a Draft makes it public
    Given the administrator `ac019.admin@example.com` and a Draft `A019 Stretching`
    When the administrator publishes `A019 Stretching`
    Then the result is success
    And the administrator's list gives `A019 Stretching` the state Published
    And browsing contains `A019 Stretching`

  @AC-020 @FR-012 @US3-AS3
  Scenario: An edit of a Published habit is public on the next read
    Given the administrator `ac020.admin@example.com` and a Published `A020 Stretching` with the category Body
    When the administrator edits it to the name `A020 Morning stretch` and the category Health, keeping its other fields
    Then the result is success
    And browsing with the category Health contains `A020 Morning stretch`
    And browsing with the search `A020` does not contain `A020 Stretching`
    And the administrator's list gives `A020 Morning stretch` the state Published

  @AC-021 @FR-011 @FR-015 @US3-AS4
  Scenario: Unpublishing a Published habit takes it out of the catalog at once
    Given the administrator `ac021.admin@example.com` and a Published `A021 Stretching`
    When the administrator unpublishes `A021 Stretching`, as one call with nothing before or after it
    Then the result is success
    And the administrator's list gives `A021 Stretching` the state Unpublished
    And browsing does not contain `A021 Stretching`
    And the anonymous visitor reading its details gets "This habit is not in the catalog."

  @AC-022 @FR-011 @US3-AS5
  Scenario: An Unpublished habit can be published again
    Given the administrator `ac022.admin@example.com` and an Unpublished `A022 Stretching`
    When the administrator publishes `A022 Stretching`
    Then the result is success
    And the administrator's list gives it the state Published
    And browsing contains `A022 Stretching`

  @AC-023 @FR-011 @US3-AS6
  Scenario: A Draft cannot be unpublished
    Given the administrator `ac023.admin@example.com` and a Draft `A023 Stretching`
    When the administrator unpublishes `A023 Stretching`
    Then the result is "This catalog habit is not published."
    And the administrator's list gives it the state Draft

  @AC-024 @FR-011 @US3-AS7
  Scenario: Publishing a Published habit is a success that changes nothing
    Given the administrator `ac024.admin@example.com` and a Published `A024 Stretching`
    When the administrator publishes `A024 Stretching`
    Then the result is success
    And the administrator's list gives it the state Published
    And browsing contains `A024 Stretching`

  @AC-025 @FR-011 @US3-AS8
  Scenario: Unpublishing an Unpublished habit is a success that changes nothing
    Given the administrator `ac025.admin@example.com` and an Unpublished `A025 Stretching`
    When the administrator unpublishes `A025 Stretching`
    Then the result is success
    And the administrator's list gives it the state Unpublished
    And browsing does not contain `A025 Stretching`

  @AC-026 @FR-004 @FR-005 @US3-AS9
  Scenario Outline: A catalog-habit name is taken in every state, ignoring case and spaces
    Given the administrator `ac026.admin.<n>@example.com`, a <state> `A026 <n> Reading` and a Draft `A026 <n> Other`
    When the administrator <action> with the name `" a026 <n> READING "`
    Then the result is "A catalog habit with this name already exists."
    And the administrator's list has exactly one habit named `A026 <n> Reading` and still has `A026 <n> Other`

    Examples:
      | n  | state       | action                  |
      | 01 | Draft       | creates a catalog habit |
      | 02 | Published   | creates a catalog habit |
      | 03 | Unpublished | creates a catalog habit |
      | 04 | Draft       | edits `A026 04 Other`   |
      | 05 | Published   | edits `A026 05 Other`   |
      | 06 | Unpublished | edits `A026 06 Other`   |

  @AC-027 @FR-003 @FR-005 @US3-AS10
  Scenario Outline: Field checks refuse a catalog habit with a message per field
    Given the administrator `ac027.admin.<n>@example.com`
    When the administrator creates a catalog habit with the name <name>, the description <description>, the category <category>, the intent <intent> and the icon <icon>
    Then the result is <result>
    And the administrator's list contains no habit whose name starts with `A027 <n>`

    Examples:
      | n  | name                                              | description              | category | intent | icon        | result                                             |
      | 01 | `"   "`                                           | none                     | Body     | Build  | `book-open` | "Enter a name."                                    |
      | 02 | `""`                                              | none                     | Body     | Build  | `book-open` | "Enter a name."                                    |
      | 03 | a name of 61 characters with the prefix `A027 03` | none                     | Body     | Build  | `book-open` | "The name can have at most 60 characters."         |
      | 04 | `A027 04 Long text`                               | a text of 501 characters | Body     | Build  | `book-open` | "The description can have at most 500 characters." |
      | 05 | `A027 05 No category`                             | none                     | none     | Build  | `book-open` | "Choose a category."                               |
      | 06 | `A027 06 No intent`                               | none                     | Body     | none   | `book-open` | "Choose Build or Reduce."                          |
      | 07 | `A027 07 No icon`                                 | none                     | Body     | Build  | none        | "Choose an icon."                                  |
      | 08 | `A027 08 Menu icon`                               | none                     | Body     | Build  | `menu`      | "Choose an icon."                                  |

  @AC-028 @FR-005 @US3-AS10
  Scenario: A name of 60 characters and a description of 500 characters are accepted
    Given the administrator `ac028.admin@example.com`
    When the administrator creates a catalog habit whose name is `A028 ` followed by `x` up to 60 characters in all, with a description that is a text of 500 characters
    Then the result is success
    And the administrator's list contains that name, and reading it gives that 500-character description

  @AC-029 @FR-005 @FR-016
  Scenario Outline: Field checks come in a fixed order
    Given the administrator `ac029.admin.<n>@example.com` and a Draft `A029 <n> Taken`
    When the administrator creates a catalog habit with <fields>
    Then the result is <result>

    Examples:
      | n  | fields                                                                                                                                                                                                       | result                                             |
      | 01 | the name `"   "`, no category and no icon                                                                                                                                                                    | "Enter a name."                                    |
      | 02 | a name of 61 characters with the prefix `A029 02`, a description that is a text of 501 characters                                                                                                            | "The name can have at most 60 characters."         |
      | 03 | the name `A029 03 Long`, a description that is a text of 501 characters, no category                                                                                                                         | "The description can have at most 500 characters." |
      | 04 | the name `A029 04 Cat`, no category, no intent                                                                                                                                                               | "Choose a category."                               |
      | 05 | the name `A029 05 Int`, the category Body, no intent, the icon `menu`                                                                                                                                        | "Choose Build or Reduce."                          |
      | 06 | the name `A029 06 Taken`, the category Body, the intent Build, the icon `menu`, and the suggested starting goals (1) Build, yes/no, 1 time per day and (2) Build, yes/no, 1 time per week, none marked         | "Choose an icon."                                  |
      | 07 | the name `A029 07 Taken`, the category Body, the intent Build, the icon `book-open`, and the suggested starting goals (1) Build, yes/no, 1 time per day and (2) Build, yes/no, 1 time per week, none marked    | "Mark one suggested starting goal for the card."   |
      | 08 | the name `A029 08 List`, the category Body, the intent Build, the icon `book-open`, and the suggested starting goals (1) Build, no measurement, 1 time per day and (2) Build, yes/no, 1 time per week, none marked | "Mark one suggested starting goal for the card."   |
      | 09 | the name `a029 09 taken`, the category Body, the intent Build, the icon `book-open`                                                                                                                          | "A catalog habit with this name already exists."   |

  @AC-030 @FR-014 @US3-AS11
  Scenario Outline: A registered user cannot manage the catalog
    Given the administrator `ac030.admin.<n>@example.com` and a Draft `A030 <n> Draft`
    And the registered user `ac030.user.<n>@example.com`
    When the registered user `ac030.user.<n>@example.com` <action>
    Then the result is "Only administrators can manage the catalog."
    And the administrator's list still gives `A030 <n> Draft` the state Draft and contains no `A030 <n> New` and no `A030 <n> Renamed`

    Examples:
      | n  | action                                       |
      | 01 | lists the catalog habits as an administrator |
      | 02 | reads `A030 02 Draft` as an administrator    |
      | 03 | creates the catalog habit `A030 03 New`      |
      | 04 | edits `A030 04 Draft` to the name `A030 04 Renamed` |
      | 05 | publishes `A030 05 Draft`                    |
      | 06 | unpublishes `A030 06 Draft`                  |

  @AC-031 @FR-014 @US3-AS11
  Scenario Outline: The anonymous visitor cannot manage the catalog
    Given the administrator `ac031.admin.<n>@example.com` and a Published `A031 <n> Published`
    When the anonymous visitor <action>
    Then the result is "You are not signed in."
    And the administrator's list still gives `A031 <n> Published` the state Published and contains no `A031 <n> New` and no `A031 <n> Renamed`

    Examples:
      | n  | action                                                  |
      | 01 | lists the catalog habits as an administrator            |
      | 02 | reads `A031 02 Published` as an administrator           |
      | 03 | creates the catalog habit `A031 03 New`                 |
      | 04 | edits `A031 04 Published` to the name `A031 04 Renamed` |
      | 05 | publishes `A031 05 Published`                           |
      | 06 | unpublishes `A031 06 Published`                         |

  @AC-032 @FR-013 @US3-AS12
  Scenario Outline: An unknown identifier does not exist for the administrator
    Given the administrator `ac032.admin.<n>@example.com`
    When the administrator <action> an unknown identifier
    Then the result is "This catalog habit does not exist."

    Examples:
      | n  | action                                                                                         |
      | 01 | reads                                                                                          |
      | 02 | edits, with the name `A032 02 Name`, the category Body, the intent Build and the icon `book-open`, |
      | 03 | publishes                                                                                      |
      | 04 | unpublishes                                                                                    |

  @AC-033 @FR-013 @FR-024 @US3-AS13
  Scenario Outline: A custom habit is not a catalog habit for the administrator
    Given the registered user `ac033.owner.<n>@example.com`, who created the custom habit `Private` with the category Mind, the intent Reduce and the icon `moon`
    And the administrator `ac033.admin.<n>@example.com`
    When the administrator <action> with the identifier of `Private`
    Then the result is "This catalog habit does not exist."
    And the administrator's list contains no habit with the identifier of `Private`
    And the registered user `ac033.owner.<n>@example.com` still reads `Private` with the category Mind, the intent Reduce and the icon `moon`

    Examples:
      | n  | action                                                                                               |
      | 01 | reads                                                                                                |
      | 02 | edits, with the name `A033 02 Taken over`, the category Body, the intent Build and the icon `book-open`, |
      | 03 | publishes                                                                                            |
      | 04 | unpublishes                                                                                          |

  @AC-034 @FR-006 @US3-AS14
  Scenario: Suggested starting goals keep their order and the card shows the marked one
    Given the administrator `ac034.admin@example.com` and a Draft `A034 Reading` with no suggested starting goal
    When the administrator edits it to the suggested starting goals, in this order: (1) Build, count 20 `pages`, 3 times per week; (2) Reduce, duration 2 hours, 1 time per day (marked)
    And the administrator publishes `A034 Reading`
    Then reading it as the administrator gives both suggested starting goals in this order, with the second marked
    And browsing with the search `A034` gives `A034 Reading` with the suggested starting goal of text "at most 2 h a day"

  @AC-035 @FR-006 @US3-AS15
  Scenario Outline: The list of suggested starting goals is checked for its length first, then for its mark
    Given the administrator `ac035.admin.<n>@example.com` and a Draft `A035 <n> Reading` with no suggested starting goal
    When the administrator edits it to <count> suggested starting goals, each Build, yes/no, 1 time per day, with <marks>
    Then the result is <result>
    And reading `A035 <n> Reading` as the administrator gives <after>

    Examples:
      | n  | count | marks            | result                                                 | after                                        |
      | 01 | 2     | none marked      | "Mark one suggested starting goal for the card."       | no suggested starting goal                   |
      | 02 | 2     | both marked      | "Mark one suggested starting goal for the card."       | no suggested starting goal                   |
      | 03 | 6     | none marked      | "A habit can have at most 5 suggested starting goals." | no suggested starting goal                   |
      | 04 | 6     | the first marked | "A habit can have at most 5 suggested starting goals." | no suggested starting goal                   |
      | 05 | 5     | the third marked | success                                                | 5 suggested starting goals, the third marked |
      | 06 | 0     | none marked      | success                                                | no suggested starting goal                   |
      | 07 | 1     | none marked      | "Mark one suggested starting goal for the card."       | no suggested starting goal                   |

  @AC-036 @FR-007 @US3-AS16
  Scenario Outline: Each field of a suggested starting goal has its own refusal, with the position
    Given the administrator `ac036.admin.<n>@example.com` and a Draft `A036 <n> Reading` with no suggested starting goal
    When the administrator edits it to two suggested starting goals: (1) Build, yes/no, 1 time per day (marked); (2) <second>
    Then the result is <result> with the position 2
    And reading `A036 <n> Reading` as the administrator gives no suggested starting goal

    Examples:
      | n  | second                                                              | result                                                    |
      | 01 | no Build or Reduce, yes/no, 1 time per day                          | "Choose Build or Reduce."                                 |
      | 02 | Build, no measurement, 1 time per day                               | "Choose how it is measured."                              |
      | 03 | Build, yes/no with the amount 5, 1 time per day                     | "A yes/no suggested starting goal has no amount or unit." |
      | 04 | Build, yes/no with the unit `pages`, 1 time per day                 | "A yes/no suggested starting goal has no amount or unit." |
      | 05 | Build, count with no amount and the unit `pages`, 1 time per day    | "Enter a whole number from 1 to 999."                     |
      | 06 | Build, count 0 `pages`, 1 time per day                              | "Enter a whole number from 1 to 999."                     |
      | 07 | Build, count 1000 `pages`, 1 time per day                           | "Enter a whole number from 1 to 999."                     |
      | 08 | Build, duration with no amount and the unit minutes, 1 time per day | "Enter a whole number from 1 to 999."                     |
      | 09 | Build, duration 0 minutes, 1 time per day                           | "Enter a whole number from 1 to 999."                     |
      | 10 | Build, duration 1000 minutes, 1 time per day                        | "Enter a whole number from 1 to 999."                     |
      | 11 | Reduce, count 1000 `sweets`, 1 time per day                         | "Enter a whole number from 0 to 999."                     |
      | 12 | Reduce, count -1 `sweets`, 1 time per day                           | "Enter a whole number from 0 to 999."                     |
      | 13 | Reduce, count with no amount and the unit `sweets`, 1 time per day  | "Enter a whole number from 0 to 999."                     |
      | 14 | Reduce, duration 1000 hours, 1 time per day                         | "Enter a whole number from 0 to 999."                     |
      | 15 | Build, count 5 with the unit `"  "`, 1 time per day                 | "Enter a unit, for example pages."                        |
      | 16 | Build, count 5 with a unit of 21 characters, 1 time per day         | "The unit can have at most 20 characters."                |
      | 17 | Build, duration 15 with no unit, 1 time per day                     | "Choose minutes or hours."                                |
      | 18 | Build, duration 15 minutes, no schedule                             | "Choose fixed days or how many times."                    |
      | 19 | Build, duration 15 minutes, fixed days with no day                  | "Choose at least one day."                                |
      | 20 | Build, duration 15 minutes, 0 times per week                        | "Enter a number of times from 1 to 20."                   |
      | 21 | Build, duration 15 minutes, 21 times per week                       | "Enter a number of times from 1 to 20."                   |
      | 22 | Build, duration 15 minutes, 3 times with no period                  | "Choose per day or per week."                             |

  @AC-037 @FR-007 @US3-AS16
  Scenario Outline: The bounds of a suggested starting goal's fields are accepted
    Given the administrator `ac037.admin.<n>@example.com` and a Draft `A037 <n> Bounds`
    When the administrator edits it to the single suggested starting goal <fields>
    Then the result is success
    And reading `A037 <n> Bounds` as the administrator gives one suggested starting goal with Build or Reduce, the measurement, the amount and the schedule given above, and the unit <stored unit>

    Examples:
      | n  | fields                                                                                                                           | stored unit                                    |
      | 01 | Build, count 1 `page`, 1 time per day                                                                                            | `page`                                         |
      | 02 | Build, count 999 `pages`, 20 times per week                                                                                      | `pages`                                        |
      | 03 | Reduce, count 0 `sweets`, 1 time per day                                                                                         | `sweets`                                       |
      | 04 | Reduce, duration 999 minutes, 20 times per day                                                                                   | minutes                                        |
      | 05 | Build, duration 1 minutes, 1 time per week                                                                                       | minutes                                        |
      | 06 | Build, duration 999 hours, 1 time per week                                                                                       | hours                                          |
      | 07 | Build, count 5 with a unit of 20 characters, 1 time per day                                                                      | that unit of 20 characters                     |
      | 08 | Build, count 5 with the unit `"  Pages  "` (5 characters after trimming), 1 time per day                                         | `Pages`                                        |
      | 09 | Build, count 5 with the unit `"  "` followed by a unit of 20 characters and `"  "` (20 characters after trimming), 1 time per day | that unit of 20 characters, without the spaces |
      | 10 | Build, duration 1 hours, fixed day Sunday                                                                                        | hours                                          |

  @AC-038 @FR-007 @FR-017 @US3-AS17
  Scenario: A Reduce amount of 0 is accepted and shown as "at most 0"
    Given the administrator `ac038.admin@example.com` and a Published `A038 Sugar` with the intent Reduce
    When the administrator edits it to the single suggested starting goal Reduce, count 0 `sweets`, 1 time per day
    Then the result is success
    And browsing with the search `A038` gives `A038 Sugar` with the suggested starting goal of text "at most 0 sweets a day"

  @AC-039 @FR-016 @US3-AS18
  Scenario Outline: The role and the identifier are checked before the fields
    Given the administrator `ac039.admin.<n>@example.com` and the registered user `ac039.user.<n>@example.com`
    When <acting user> <action>
    Then the result is <result>

    Examples:
      | n  | acting user                                     | action                                             | result                                        |
      | 01 | the registered user `ac039.user.01@example.com` | creates a catalog habit with the name `"   "`      | "Only administrators can manage the catalog." |
      | 02 | the anonymous visitor                           | creates a catalog habit with the name `"   "`      | "You are not signed in."                      |
      | 03 | the administrator `ac039.admin.03@example.com`  | edits an unknown identifier with the name `"   "` | "This catalog habit does not exist."          |
      | 04 | the registered user `ac039.user.04@example.com` | edits an unknown identifier with the name `"   "` | "Only administrators can manage the catalog." |

  @AC-040 @FR-004 @FR-012 @US3-AS19
  Scenario: The administrator's list is in name order and can be searched
    Given the administrator `ac040.admin@example.com`, a Draft `A040 rowing`, a Published `A040 Running` and a Draft `A040 Swimming`
    When the administrator lists the catalog habits with the search `"a040 r"`
    Then the list is exactly `A040 rowing` with the state Draft and `A040 Running` with the state Published, in this order
    And the result gives the total number of catalog habits, which is the same as for the administrator's list with no search
    And the administrator's list with no search contains `A040 rowing`, `A040 Running` and `A040 Swimming`, in this order

  @AC-041 @FR-007
  Scenario Outline: Suggested starting goals are checked in list order, and each by its first failing field
    Given the administrator `ac041.admin.<n>@example.com` and a Draft `A041 <n> Order`
    When the administrator edits it to the suggested starting goals <goals>
    Then the result is <result> with the position <position>

    Examples:
      | n  | goals                                                                                                                                      | result                                 | position |
      | 01 | (1) no Build or Reduce, no measurement, no schedule (marked); (2) Build, yes/no, 1 time per day                                            | "Choose Build or Reduce."              | 1        |
      | 02 | (1) Build, yes/no, 1 time per day (marked); (2) Build, no measurement, no schedule; (3) Build, duration 15 minutes, no schedule             | "Choose how it is measured."           | 2        |
      | 03 | (1) Build, yes/no, 1 time per day (marked); (2) Build, yes/no, 1 time per day; (3) Build, duration 15 minutes, no schedule                  | "Choose fixed days or how many times." | 3        |

  @AC-042 @FR-001 @FR-012
  Scenario: The administrator reads a Draft with all its fields and its state
    Given the administrator `ac042.admin@example.com`
    And a Draft `A042 Journaling` with the category Mind, the intent Build, the icon `pen-line`, the description "A few lines in the evening." and the suggested starting goal Build, yes/no, fixed days Monday, Wednesday and Friday
    When the administrator reads `A042 Journaling`
    Then the result has the name `A042 Journaling`, the category Mind, the intent Build, the icon `pen-line`, the description "A few lines in the evening.", the state Draft and that suggested starting goal, marked

  @AC-043 @FR-002
  Scenario Outline: Each of the six categories can be given to a catalog habit
    Given the administrator `ac043.admin.<n>@example.com`
    When the administrator creates the catalog habit `A043 <n> <category>` with the category <category> and publishes it
    Then browsing with the category <category> contains `A043 <n> <category>`

    Examples:
      | n  | category |
      | 01 | Body     |
      | 02 | Mind     |
      | 03 | Health   |
      | 04 | Money    |
      | 05 | Home     |
      | 06 | Other    |

  @AC-044 @FR-003
  Scenario Outline: Only the 24 habit icons are accepted
    Given the administrator `ac044.admin.<n>@example.com`
    When the administrator creates the catalog habit `A044 <n>` with the icon <icon>
    Then the result is <result>

    Examples:
      | n  | icon              | result            |
      | 01 | `book-open`       | success           |
      | 02 | `person-standing` | success           |
      | 03 | `smartphone`      | success           |
      | 04 | `footprints`      | success           |
      | 05 | `piggy-bank`      | success           |
      | 06 | `candy`           | success           |
      | 07 | `coffee`          | success           |
      | 08 | `moon`            | success           |
      | 09 | `dumbbell`        | success           |
      | 10 | `droplet`         | success           |
      | 11 | `bike`            | success           |
      | 12 | `brain`           | success           |
      | 13 | `pen-line`        | success           |
      | 14 | `languages`       | success           |
      | 15 | `music`           | success           |
      | 16 | `salad`           | success           |
      | 17 | `utensils`        | success           |
      | 18 | `cigarette-off`   | success           |
      | 19 | `wine-off`        | success           |
      | 20 | `wallet`          | success           |
      | 21 | `receipt`         | success           |
      | 22 | `sprout`          | success           |
      | 23 | `sparkles`        | success           |
      | 24 | `tv`              | success           |
      | 25 | `menu`            | "Choose an icon." |
      | 26 | `trash-2`         | "Choose an icon." |
      | 27 | `search`          | "Choose an icon." |
      | 28 | `rocket`          | "Choose an icon." |

  @AC-045 @FR-004
  Scenario: Names are ordered ordinally, ignoring case, not by the current culture
    Given the administrator `ac045.admin@example.com`
    And a Published `A045 Éclair`, a Published `A045 zebra` and a Published `A045 Apple`
    When the anonymous visitor browses the catalog with the search `A045`
    Then the result is `A045 Apple`, `A045 zebra`, `A045 Éclair`, in this order (`É` and `é` come after every letter A to Z and a to z when compared ordinally, in either case)

  @AC-046 @FR-016
  Scenario Outline: Publish and unpublish check the acting user and the role before the identifier
    Given the registered user `ac046.user.<n>@example.com`
    When <acting user> <action> an unknown identifier
    Then the result is <result>

    Examples:
      | n  | acting user                                     | action      | result                                        |
      | 01 | the registered user `ac046.user.01@example.com` | publishes   | "Only administrators can manage the catalog." |
      | 02 | the registered user `ac046.user.02@example.com` | unpublishes | "Only administrators can manage the catalog." |
      | 03 | the anonymous visitor                           | publishes   | "You are not signed in."                      |
      | 04 | the anonymous visitor                           | unpublishes | "You are not signed in."                      |

  @AC-047 @FR-001 @FR-020 @US4-AS1
  Scenario: A registered user creates a custom habit
    Given the registered user `ac047.user@example.com`
    When the registered user creates the custom habit `Cold shower` with the category Body, the intent Build, the icon `droplet` and the description "At least 1 minute of cold water."
    Then the result is success
    And their list of custom habits contains `Cold shower` with the category Body, the intent Build, the icon `droplet` and the description "At least 1 minute of cold water."
    And reading it gives the same values

  @AC-048 @FR-004 @FR-020 @US4-AS2
  Scenario: A user's list holds only their own custom habits, in name order ignoring case
    Given the registered user `ac048.user@example.com`, who created the custom habits `B habit` and then `a habit`
    And the registered user `ac048.other@example.com`, who created the custom habit `C habit`
    And the administrator `ac048.admin@example.com` and a Published `A048 Catalog habit`
    When the registered user `ac048.user@example.com` lists their custom habits
    Then the list is exactly `a habit`, `B habit`, in this order

  @AC-049 @FR-004 @FR-020 @US4-AS3
  Scenario: A registered user edits every field of their custom habit
    Given the registered user `ac049.user@example.com`, who created the custom habit `Cold shower` with the category Body, the intent Build, the icon `droplet` and no description
    When the registered user edits it to the name `Cold shower, 2 min` and the intent Reduce, keeping its other fields
    And then edits it, keeping the name `Cold shower, 2 min`, to the category Health, the icon `moon` and the description "In the morning."
    Then both results are success
    And reading it gives the name `Cold shower, 2 min`, the category Health, the intent Reduce, the icon `moon` and the description "In the morning."

  @AC-050 @FR-020 @US4-AS4
  Scenario: A registered user deletes their custom habit
    Given the registered user `ac050.user@example.com`, who created the custom habits `Cold shower` and `Evening walk`
    When the registered user deletes `Cold shower`
    Then the result is success
    And their list is exactly `Evening walk`
    And reading `Cold shower` by its identifier gives "This custom habit does not exist."

  @AC-051 @FR-004 @FR-005 @US4-AS5
  Scenario Outline: A user's custom-habit name is taken, ignoring case and spaces
    Given the registered user `ac051.user.<n>@example.com`, who created the custom habits `Cold shower` and `Evening walk`
    When the registered user <action> with the name `" cold SHOWER "`
    Then the result is "You already have a custom habit with this name."
    And their list is still exactly `Cold shower`, `Evening walk`

    Examples:
      | n  | action                 |
      | 01 | creates a custom habit |
      | 02 | edits `Evening walk`   |

  @AC-052 @FR-004 @US4-AS6
  Scenario: Another user, and a catalog habit, may have the same name
    Given the registered user `ac052.a@example.com`, who created the custom habit `Cold shower`
    And the administrator `ac052.admin@example.com` and a Published `A052 Reading`
    And the registered user `ac052.b@example.com`
    When the registered user `ac052.b@example.com` creates the custom habit `Cold shower` and the custom habit `A052 Reading`
    Then both results are success
    And the list of `ac052.b@example.com` is exactly `A052 Reading`, `Cold shower`
    And the list of `ac052.a@example.com` is exactly `Cold shower`
    And browsing with the search `A052` gives exactly one habit, `A052 Reading`, with the identifier of the catalog habit

  @AC-053 @FR-021 @FR-022 @US4-AS7
  Scenario Outline: Another registered user cannot reach a custom habit
    Given the registered user `ac053.a.<n>@example.com`, who created the custom habit `Secret` with the category Mind and the intent Build
    And the registered user `ac053.b.<n>@example.com`
    When the registered user `ac053.b.<n>@example.com` <action> the custom habit `Secret` of `ac053.a.<n>@example.com` by its identifier
    Then the result is "This custom habit does not exist."
    And the list of `ac053.b.<n>@example.com` does not contain `Secret`
    And `ac053.a.<n>@example.com` still reads `Secret` with the category Mind and the intent Build

    Examples:
      | n  | action                                                                                     |
      | 01 | reads                                                                                      |
      | 02 | edits, with the name `Mine now`, the category Body, the intent Reduce and the icon `droplet`, |
      | 03 | deletes                                                                                    |

  @AC-054 @FR-016 @FR-025 @US4-AS8
  Scenario Outline: An administrator has no access to custom habits, whatever the fields
    Given the registered user `ac054.owner.<n>@example.com`, who created the custom habit `Secret` with the category Mind
    And the administrator `ac054.admin.<n>@example.com`
    When the administrator <action>
    Then the result is the refusal "Custom habits are for registered users." and carries no custom habit
    And `ac054.owner.<n>@example.com` still reads `Secret` with the category Mind

    Examples:
      | n  | action                                                                                                            |
      | 01 | lists custom habits                                                                                               |
      | 02 | reads `Secret` by its identifier                                                                                  |
      | 03 | edits `Secret` by its identifier, with the name `Seen`, the category Body, the intent Build and the icon `droplet` |
      | 04 | deletes `Secret` by its identifier                                                                                |
      | 05 | creates the custom habit `Admin habit`                                                                            |
      | 06 | creates a custom habit with the name `"   "` and no category                                                     |
      | 07 | edits `Secret` by its identifier, with the name `"   "`                                                           |

  @AC-055 @FR-023 @US4-AS9
  Scenario Outline: The anonymous visitor has no access to custom habits
    Given the registered user `ac055.owner.<n>@example.com`, who created the custom habit `Secret`
    When the anonymous visitor <action>
    Then the result is "You are not signed in."
    And `ac055.owner.<n>@example.com` still lists exactly `Secret`

    Examples:
      | n  | action                                                                                                            |
      | 01 | lists custom habits                                                                                               |
      | 02 | reads `Secret` by its identifier                                                                                  |
      | 03 | creates the custom habit `Visitor habit`                                                                          |
      | 04 | edits `Secret` by its identifier, with the name `Seen`, the category Body, the intent Build and the icon `droplet` |
      | 05 | deletes `Secret` by its identifier                                                                                |

  @AC-056 @FR-005 @US4-AS10
  Scenario Outline: Field checks refuse a custom habit with the same messages
    Given the registered user `ac056.user.<n>@example.com`
    When the registered user creates a custom habit with the name <name>, the description <description>, the category <category>, the intent <intent> and the icon <icon>
    Then the result is <result>
    And their list of custom habits is empty

    Examples:
      | n  | name                                            | description              | category | intent | icon      | result                                             |
      | 01 | `"   "`                                         | none                     | Body     | Build  | `droplet` | "Enter a name."                                    |
      | 02 | a name of 61 characters with the prefix `Habit` | none                     | Body     | Build  | `droplet` | "The name can have at most 60 characters."         |
      | 03 | `Long text`                                     | a text of 501 characters | Body     | Build  | `droplet` | "The description can have at most 500 characters." |
      | 04 | `No category`                                   | none                     | none     | Build  | `droplet` | "Choose a category."                               |
      | 05 | `No intent`                                     | none                     | Body     | none   | `droplet` | "Choose Build or Reduce."                          |
      | 06 | `No icon`                                       | none                     | Body     | Build  | none      | "Choose an icon."                                  |
      | 07 | `Menu icon`                                     | none                     | Body     | Build  | `menu`    | "Choose an icon."                                  |

  @AC-057 @FR-005 @FR-016 @US4-AS11
  Scenario Outline: For custom habits the acting user and ownership come first, the name check last
    Given the registered user `ac057.a.<n>@example.com`, who created the custom habits `Secret` and `Taken`
    And the registered user `ac057.b.<n>@example.com`
    When <acting user> <action>
    Then the result is <result>

    Examples:
      | n  | acting user                                  | action                                                               | result                              |
      | 01 | the anonymous visitor                        | creates a custom habit with the name `"   "`                         | "You are not signed in."            |
      | 02 | the registered user `ac057.b.02@example.com` | edits `Secret` of `ac057.a.02@example.com` with the name `"   "`     | "This custom habit does not exist." |
      | 03 | the registered user `ac057.b.03@example.com` | edits an unknown identifier with the name `"   "`                    | "This custom habit does not exist." |
      | 04 | the registered user `ac057.a.04@example.com` | creates a custom habit with the name `taken` and no category         | "Choose a category."                |
      | 05 | the registered user `ac057.a.05@example.com` | edits `Secret` to the name `TAKEN` and the icon `menu`               | "Choose an icon."                   |

  @AC-058 @FR-022
  Scenario Outline: A catalog habit's identifier and a deleted custom habit do not exist as custom habits
    Given the registered user `ac058.user.<n>@example.com`, who created and then deleted the custom habit `Gone`
    And the administrator `ac058.admin.<n>@example.com` and a Published `A058 <n> Reading`
    When the registered user <action>
    Then the result is "This custom habit does not exist."
    And browsing with the search `A058 <n>` still contains `A058 <n> Reading` with the category Other and the intent Build, and contains no `Taken over`

    Examples:
      | n  | action                                                                                                                                        |
      | 01 | reads `Gone` by its identifier                                                                                                                |
      | 02 | edits `Gone` by its identifier, with the name `Back`, the category Body, the intent Build and the icon `droplet`                               |
      | 03 | deletes `Gone` by its identifier                                                                                                              |
      | 04 | reads a custom habit with the identifier of `A058 04 Reading`                                                                                 |
      | 05 | deletes a custom habit with the identifier of `A058 05 Reading`                                                                               |
      | 06 | edits a custom habit with the identifier of `A058 06 Reading`, with the name `Taken over`, the category Body, the intent Build and the icon `droplet` |

  @AC-059 @FR-005
  Scenario: A custom habit with a name of 60 characters and a description of 500 characters is accepted
    Given the registered user `ac059.user@example.com`
    When the registered user creates a custom habit whose name is `Habit ` followed by `x` up to 60 characters in all, with a description that is a text of 500 characters
    Then the result is success
    And reading it gives that name and that 500-character description

  @AC-060 @FR-030 @US6-AS1
  Scenario: Demo loading loads the seed catalog into an empty catalog
    Given the AC database is reset, so no account and no habit exists
    And the administrator `ac060.admin@example.com`
    When demo loading runs with the demo password `example2026`
    Then the catalog result is "seed catalog loaded"
    And browsing gives a count of at least 20, and every habit it gives has a description
    And the category filter is exactly "All", "Body", "Mind", "Health", "Money", "Home", "Other", in this order
    And browsing with the intent Reduce gives a count of at least 4
    And the administrator's list has exactly one habit with the state Draft and exactly one with the state Unpublished, and every other habit has the state Published
    And no two habits of the administrator's list have the same icon
    And among the suggested starting goals in the details of the Published habits there is at least one yes/no, one count and one duration, at least one with fixed days and one with N times
    And at least one Published habit has no suggested starting goal, and at least one has three or more

  @AC-061 @FR-030 @US6-AS2
  Scenario: Demo loading run again leaves the seed catalog unchanged
    Given the AC database is reset, so no account and no habit exists
    And demo loading ran with the demo password `example2026` and its catalog result was "seed catalog loaded"
    And the administrator `ac061.admin@example.com` listed the catalog habits, with their names, states and total
    When demo loading runs again with the demo password `example2026`
    Then the catalog result is "catalog not empty"
    And the administrator's list has exactly the same names with the same states, and the same total

  @AC-062 @FR-024 @FR-030 @US6-AS3
  Scenario: Demo loading adds no seed to a catalog that has a habit
    Given the AC database is reset, so no account and no habit exists
    And the administrator `ac062.admin@example.com` and a Draft `A062 Rowing`
    When demo loading runs with the demo password `example2026`
    Then the catalog result is "catalog not empty"
    And the account result is "created" and the custom-habit result is "demo custom habits added" for `demo1@example.com`, for `demo2@example.com` and for `demo3@example.com`
    And signed in with `example2026`, `demo1@example.com` lists exactly `Cold shower`, `No phone in bed`
    And the administrator's list is exactly `A062 Rowing`, with the total 1

  @AC-063 @FR-030 @FR-031 @US6-AS4
  Scenario Outline: Demo loading without a valid demo password creates no catalog and no custom habit
    Given the AC database is reset, so no account and no habit exists
    When demo loading runs with the demo password <demo password>, where absent means that no value is supplied
    Then the result is <result>
    And browsing gives a count of 0
    And demo loading run afterwards with the demo password `example2026` gives "created" for `demo1@example.com`, `demo2@example.com` and `demo3@example.com`, "demo custom habits added" for each of them, and the catalog result "seed catalog loaded"

    Examples:
      | n  | demo password | result                                           |
      | 01 | absent        | "no demo password configured"                    |
      | 02 | `"   "`       | "no demo password configured"                    |
      | 03 | `walk4`       | "demo password does not meet the password rule"  |

  @AC-064 @FR-024 @FR-031 @US6-AS5
  Scenario: Demo accounts that demo loading creates get their demo custom habits
    Given the AC database is reset, so no account and no habit exists
    And the administrator `ac064.admin@example.com`
    When demo loading runs with the demo password `example2026`
    Then the account result is "created" and the custom-habit result is "demo custom habits added" for `demo1@example.com`, for `demo2@example.com` and for `demo3@example.com`
    And no result text of demo loading contains any of the names `Cold shower`, `No phone in bed`, `Evening stretch`, `Plan tomorrow`, `Late snacks` or `Water the plants`
    And signed in with `example2026`, each demo account lists exactly these custom habits:
      | account             | name             | category | intent | icon              |
      | `demo1@example.com` | Cold shower      | Body     | Build  | `droplet`         |
      | `demo1@example.com` | No phone in bed  | Home     | Reduce | `smartphone`      |
      | `demo2@example.com` | Evening stretch  | Body     | Build  | `person-standing` |
      | `demo2@example.com` | Plan tomorrow    | Mind     | Build  | `pen-line`        |
      | `demo3@example.com` | Late snacks      | Health   | Reduce | `candy`           |
      | `demo3@example.com` | Water the plants | Home     | Build  | `sprout`          |
    And no identifier of these six custom habits is the identifier of a habit that browsing gives
    And no identifier of these six custom habits is the identifier of a habit of the administrator's list

  @AC-065 @FR-031 @US6-AS6
  Scenario: Demo loading run again leaves the demo accounts and their custom habits unchanged
    Given the AC database is reset, so no account and no habit exists
    And demo loading ran with the demo password `example2026`
    When demo loading runs again with the demo password `example2026`
    Then the account result is "already exists" for `demo1@example.com`, `demo2@example.com` and `demo3@example.com`
    And the result has no custom-habit result for any account
    And signed in with `example2026`, `demo1@example.com` lists exactly `Cold shower`, `No phone in bed`
    And `demo2@example.com` lists exactly `Evening stretch`, `Plan tomorrow`
    And `demo3@example.com` lists exactly `Late snacks`, `Water the plants`

  @AC-066 @FR-031 @US6-AS7
  Scenario: Deleted demo custom habits are not created again
    Given the AC database is reset, so no account and no habit exists
    And demo loading ran with the demo password `example2026`
    And `demo1@example.com`, signed in with `example2026`, deleted `Cold shower`
    And `demo2@example.com`, signed in with `example2026`, deleted `Evening stretch` and `Plan tomorrow`
    When demo loading runs again with the demo password `example2026`
    Then the result has no custom-habit result for any account
    And `demo1@example.com` lists exactly `No phone in bed`
    And `demo2@example.com` lists no custom habit

  @AC-067 @FR-031 @US6-AS8
  Scenario: A demo email that a person registered first keeps its owner's data untouched
    Given the AC database is reset, so no account and no habit exists
    And the anonymous visitor registered `demo2@example.com` with the password `owner2026`
    When demo loading runs with the demo password `example2026`
    Then the account result is "created" for `demo1@example.com` and `demo3@example.com` and "already exists" for `demo2@example.com`
    And the custom-habit result is "demo custom habits added" for `demo1@example.com` and `demo3@example.com`, and there is no custom-habit result for `demo2@example.com`
    And `demo2@example.com`, signed in with `owner2026`, lists no custom habit

  @AC-068 @FR-025 @FR-031
  Scenario: A demo email held by an administrator gets no custom habit
    Given the AC database is reset, so no account and no habit exists
    And administrator creation ran with the email `demo1@example.com` and the password `admin2026` and gave "administrator created"
    When demo loading runs with the demo password `example2026`
    Then the account result is "already exists" for `demo1@example.com` and "created" for `demo2@example.com` and `demo3@example.com`
    And there is no custom-habit result for `demo1@example.com`
    And `demo1@example.com`, signed in with `admin2026`, listing custom habits gets "Custom habits are for registered users."

  @AC-069 @FR-012
  Scenario: A Draft and an Unpublished habit can be edited and keep their state
    Given the administrator `ac069.admin@example.com`, a Draft `A069 Draft` and an Unpublished `A069 Old`
    When the administrator edits `A069 Draft` to the category Mind and `A069 Old` to the category Home
    Then both results are success
    And the administrator's list gives `A069 Draft` the state Draft and `A069 Old` the state Unpublished
    And reading them as the administrator gives the categories Mind and Home
    And browsing with the search `A069` gives no habit

  @AC-070 @FR-004 @FR-005
  Scenario Outline: Names and descriptions are trimmed before they are checked and stored
    Given the administrator `ac070.admin.<n>@example.com`
    When the administrator creates a catalog habit with the name <name> and the description <description>
    Then the result is success
    And reading it as the administrator gives the name <stored name> and <stored description>

    Examples:
      | n  | name                                                                                                       | description                                                                   | stored name           | stored description              |
      | 01 | `"   A070 01 Trimmed   "`                                                                                  | `"  Read daily.  "`                                                           | `A070 01 Trimmed`     | the description "Read daily."   |
      | 02 | `"A070 02 Blank"`                                                                                          | `"   "`                                                                       | `A070 02 Blank`       | no description                  |
      | 03 | `"   "` + `A070 03 ` followed by `x` up to 60 characters + `"   "` (66 characters, 60 after trimming)        | none                                                                          | the 60-character name | no description                  |
      | 04 | `"A070 04 Long"`                                                                                           | `"  "` + a text of 500 characters + `"  "` (504 characters, 500 after trimming) | `A070 04 Long`        | the 500-character description   |

  @AC-071 @FR-012
  Scenario: The administrator edits the description, intent and icon of a catalog habit
    Given the administrator `ac071.admin@example.com` and a Published `A071 Walking` with the description "Walks outside.", the intent Build and the icon `footprints`
    When the administrator edits it, keeping the name `A071 Walking`, to the description "Short walks after meals.", the intent Reduce and the icon `bike`
    Then the result is success
    And browsing with the search `A071` gives `A071 Walking` with the description "Short walks after meals.", the intent Reduce and the icon `bike`

  @AC-072 @FR-008 @US5-AS2
  Scenario: The Home preview has fewer than four habits when fewer are Published
    Given the AC database is reset, so no account and no habit exists
    And the administrator `ac072.admin@example.com`
    And a Published `A072 Beta`, a Published `A072 Alpha`, a Draft `A072 Aardvark` and an Unpublished `A072 Abacus`
    When the anonymous visitor asks for the Home preview
    Then it is exactly `A072 Alpha`, `A072 Beta`, in this order

  @AC-073 @FR-004 @FR-005
  Scenario Outline: Custom-habit names and descriptions are trimmed before they are checked and stored
    Given the registered user `ac073.user.<n>@example.com`
    When the registered user creates a custom habit with the name <name> and the description <description>
    Then the result is success
    And reading it gives the name <stored name> and <stored description>

    Examples:
      | n  | name                                                                                                    | description                                                                     | stored name           | stored description                |
      | 01 | `"   Cold shower   "`                                                                                   | `"  In the morning.  "`                                                         | `Cold shower`         | the description "In the morning." |
      | 02 | `"Blank"`                                                                                               | `"   "`                                                                         | `Blank`               | no description                    |
      | 03 | `"   "` + `Habit ` followed by `x` up to 60 characters + `"   "` (66 characters, 60 after trimming)      | none                                                                            | the 60-character name | no description                    |
      | 04 | `"Long"`                                                                                                | `"  "` + a text of 500 characters + `"  "` (504 characters, 500 after trimming) | `Long`                | the 500-character description     |
