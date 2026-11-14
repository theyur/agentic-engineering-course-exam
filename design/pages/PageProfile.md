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
