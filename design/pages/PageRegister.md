# PageRegister

Register: same `ht-auth` layout as Sign in.

- Email, Password (rule in help text), Repeat password, a neutral Notice that community comparison is off by default, primary "Create account".
- No public display name here: it is asked only when the user opts in to community (Profile → Community). The optional "Name" of slice 4 is the private account name, not the public display name.
- After registering, the user is signed in and goes to the signed-in Home.

## Error states

Each message sits under its field as `ht-field__error`, with `aria-invalid="true"` on the input and `aria-describedby` pointing at the message. The entered email is kept; both password fields are emptied. The preview shows two states.

- Invalid email: "Enter a valid email address." under Email.
- Password rule not met: "At least 8 characters, with a number." under Password, in place of the help text.
- Email already has an account: "An account with this email already exists." under Email, followed by a "Sign in" link.
- Repeated password differs: "The passwords do not match." under Repeat password. The page checks this before anything else is checked.

## Slice 4 state

The last part of the preview shows Register as slice 4 builds it (HP-016, HP-017, ADR 0008).

- "Name (optional)" comes first, with the help "Only you can see it. Home uses it to greet you.". It is the private account name (FR-028), not the public display name of community participation. A name of more than 60 characters: "The name can have at most 60 characters." under the field, after slice 1's checks; the name and the email are kept, the password fields are emptied.
- The browser's time zone is sent with the form with no visible field: a hidden input marked `data-ht-browser-time-zone`, filled with `Intl.DateTimeFormat().resolvedOptions().timeZone` by the app's script when the form is submitted. A browser that gives none, or a value that is not a named time zone, registers the account with the installation's default time zone (FR-025). Nothing on the page shows or asks for the time zone; Profile changes it later.
