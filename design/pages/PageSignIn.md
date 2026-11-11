# PageSignIn

Sign in: a centered 420px column (`ht-auth`) with one Card form.

- Email, Password, "Keep me signed in", full-width primary "Sign in"; below it "New here? Create an account".
- There is no "Forgot password?": the app sends no email, so there is no password recovery (ADR 0009).
- Errors: one message under the password field, "Email or password is not correct.", never which of the two was wrong. The password field gets `aria-invalid="true"`; the entered email is kept, the password is not.
- A deactivated account sees a Notice: "This account is deactivated. Contact the administrators." (slice 11).
- Sign in is the app's own page, a server-rendered form with these classes; it does not use the ASP.NET Core Identity pages.
