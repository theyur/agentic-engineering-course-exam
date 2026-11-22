# PageProfile

The personal profile: header with avatar, name and email, then Tabs for Account, Community, Goal history and Custom habits.

- Shown with the Community tab: participation Toggle, public display name field, and a card that lists exactly what others can and cannot see.
- Goal history (ended attempts with outcome Achieved / Abandoned / No longer relevant) uses a `DataTable`.

## Slice 1 state (Account only)

The second part of the preview shows Profile as slice 1 builds it (HP-014, ADR 0008).

- Header: the large avatar with the first letter of the email in upper case ("Y"), the email as the heading, and "Profile" as the muted line. No name and no "member since": slice 1 has neither.
- One section only, so there are no Tabs: `design/components/Tabs` asks for 2 to 5 tabs. The section is one Card titled "Account". Tabs come back when a second section ships (Community, slice 9); Account is then the first tab.
- Account content: the email (KeyValue), then the form "Change password" with Current password, New password (the rule "At least 8 characters, with a number." as help text), Repeat new password and the primary button "Change password".
- Refusals sit under their field as `ht-field__error`, one at a time: "The current password is not correct." under Current password, the password rule under New password, "The passwords do not match." under Repeat new password.
- After a successful change: the Notice "Your password is changed." (`role="status"`) above the form, and all three fields are empty.

## Slice 2 state (Account and Custom habits)

The third part of the preview shows Profile as slice 2 builds it for registered users (HP-008, US4-AS14, ADR 0008). An administrator's Profile keeps the slice 1 state: Account only, without Tabs (administrators have no custom habits).

- The same header, then the link Tabs "Account" (`/profile`) and "Custom habits" (`/profile?tab=custom-habits`). They are links, so the page stays a normal page and the change-password form keeps working.
- Account: the slice 1 content unchanged (the email and the "Change password" form with its states).
- Custom habits: a Card "Custom habits" with secondary "+ Create custom habit" (→ the custom habit form), the line "Private habits for your own goals. Only you can see them.", then one row per custom habit in name order: glyph, name, category and intent, the strong "Private" Tag and "Edit" (→ the custom habit form). With none, an EmptyState "No custom habits yet" with "+ Create custom habit".

## Slice 4 state

The last part of the preview shows Profile as slice 4 builds it (HP-015, HP-017, ADR 0008). An administrator's Profile has the same header and Account content, without Tabs.

- Header: the large avatar with the first letter of the account name in upper case ("Y" for Yuri), or of the email without a name; then
  - with a name: the name as the `h1` and the muted line "yuri@example.com · member since November 2026";
  - without a name: the email as the `h1` and "member since November 2026";
  - an account created before slice 4 has no "member since": with a name the muted line is the email alone; without a name the header is the slice 1 one (the email and "Profile").
  "member since" gives the month and year of creation in the account's time zone.
- The Account tab, in one Card "Account": the email (KeyValue), then the "Name" form, the "Time zone" form and "Change password", each with its own button. The Name and Time zone forms use secondary "Save"; "Change password" stays the primary button.
  - "Name (optional)" with the help "Only you can see it. Home uses it to greet you.". After Save: the Notice "Your name is changed." (`role="status"`) above the field, and the avatar letter follows the new name. A name of more than 60 characters: "The name can have at most 60 characters." under the field. An empty name removes the name.
  - "Time zone": a select with "UTC" first, then the named time zones of the installation's time zone data in ordinal order ("America/New_York", "Asia/Tokyo", "Europe/Kyiv", …), plus the current value when it is not among them ("Europe/Kiev"), so the current value is always selected. The help: "Your goals' days and weeks start at midnight in this time zone.". After Save: the Notice "Your time zone is changed. Your goals use it from their next day or week; earlier days keep their time zone." (`role="status"`). A value that is not a named time zone: "Choose a time zone from the list." under the field.
- The full preview's avatar shows one letter ("Y"), not initials.

## Slice 8 state

The last part of the preview shows Profile as slice 8 builds it (HP-006, US4-AS3, US4-AS7, contracts/host.md "Profile", ADR 0008). Today is Mon 16 Nov 2026, 12:00 in the preview (spec US4): "Meditation, 10 min daily" ended at 10:00 with "Abandoned"; "Sugar, at most 1 sweet a day" ended on Sat 14 Nov with "Achieved" and a note, and was restarted today.

- Header: slice 4's, unchanged.
- Registered users: slice 2's link Tabs (`nav.ht-tabs`, `aria-label="Profile sections"`) with the tab "Goal history" after "Account": "Account" (`/profile`), "Goal history" (`/profile?tab=history`) and "Custom habits" (`/profile?tab=custom-habits`). No Community tab (slice 9) and no counts. The selected tab is kept in the address; `?tab=history` shows Goal history.
- Goal history: a DataTable (`components/DataTable.md`: `ht-table-wrap`, `table.ht-table` with `aria-label="Goal history"`, `th scope="col"`), with no card title (the tab names it; settled here, T010). The columns, in this order:
  1. Goal: the attempt's name (FR-004, as it was at its last date), in bold, as a link to the attempt view (`/goals/{id}/attempts/{n}`), or to the goal page (`/goals/{id}`) when the attempt is the goal's latest (`IsLatestAttempt`; Meditation in the preview);
  2. Attempt: its number ("1");
  3. Ended: the ending date, "Mon 16 Nov" (`DateTexts`);
  4. Outcome: the outcome Tag (`ht-chip`, "Achieved", "Abandoned" or "No longer relevant", always with its word);
  5. Note: the ending note as plain text in `td.ht-muted`, or "—" without one.

  The rows are in `GetGoalHistoryAsync`'s order (FR-015): the latest ending date first, then by the attempt's name and number. No row actions.
- With no ended attempt: a Card with the sentence "No ended goals yet." (`ht-card__sub`) in place of the table (settled here, T010, as the Goals page's empty tabs).
- Account and Custom habits: unchanged (slice 4 and slice 2).
- An administrator's Profile keeps its Account content without Tabs (slice 4), and has no Goal history; `?tab=history` is ignored there (FR-023).
- Every text comes from `ReportText`, `GoalChangeTexts` and `DateTexts` (HP-008); the page builds none.
