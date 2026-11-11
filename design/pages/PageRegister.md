# PageRegister

Register: same `ht-auth` layout as Sign in.

- Email, Password (rule in help text), Repeat password, a neutral Notice that community comparison is off by default, primary "Create account".
- No public name here: it is asked only when the user opts in to community (Profile → Community).
- After registering, the user is signed in and goes to the signed-in Home.

## Error states

Each message sits under its field as `ht-field__error`, with `aria-invalid="true"` on the input and `aria-describedby` pointing at the message. The entered email is kept; both password fields are emptied. The preview shows two states.

- Invalid email: "Enter a valid email address." under Email.
- Password rule not met: "At least 8 characters, with a number." under Password, in place of the help text.
- Email already has an account: "An account with this email already exists." under Email, followed by a "Sign in" link.
- Repeated password differs: "The passwords do not match." under Repeat password. The page checks this before anything else is checked.
