# Notice

A notice is a full-width message inside a page: an invitation (accent-soft) or a neutral rule (surface-2).

- `ht-notice`: soft blue, may hold one primary button on the right (visitor catalog: "Register").
- `ht-notice--neutral`: grey, for rules people must know (the admin privacy note). No button.
- Keep it to one or two sentences. Not for errors of a form: use the field error text.

## Connection notices (the app shell)

Two notices belong to the Blazor host, not to a page. Both use `ht-notice` styling on `surface` with `shadow-raised`; their rules are in `css/ht-components.css`.

- **Reconnect notice** (`#components-reconnect-modal`, a `dialog`): shown when the live connection to the server is lost. Centered, at most 360px wide, on the `overlay` backdrop. One sentence per state, and a button where the user can act:
  - trying: "Connecting to the server again.";
  - trying again later: "The connection is lost. Trying again in N seconds.";
  - failed: "The connection could not be restored. Retry, or reload the page." with "Retry" (primary);
  - paused by the server: "The session is paused." with "Resume" (primary);
  - resume failed: "The session could not be resumed. Reload the page." with "Resume" (primary).
- **Unexpected-error bar** (`#blazor-error-ui`): hidden until the app shows it. A full-width bar fixed at the bottom on `surface` with a `line` top border and `shadow-raised`: "Something went wrong. Reload the page to continue." with a "Reload" link and an icon-only dismiss button (`x` icon, `aria-label="Dismiss"`).
- Neither notice shows technical detail, an error code or a request id.
