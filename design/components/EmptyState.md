# EmptyState

What a list shows when it has nothing yet: an icon, a short title, one or two sentences and the next step.

- Used for: no goals, no custom habits, no ended goals, no notes, an empty admin queue ("No suggestions waiting").
- Title says what is missing ("No goals yet"); text says how to get started; one primary action (plus one secondary at most).
- Inside a Card or `ht-list`; centered, `ht-empty__icon` uses a Lucide icon on `accent-soft`.

## Not found and Error pages

Both pages are an EmptyState in a Card inside the normal layout (TopNav, page content, Footer), centered at most 560px wide, with the `circle-alert` icon. No technical detail: no status code text, request id, exception, environment or configuration value.

- Not found: title "Page not found", text "The page you asked for does not exist. Check the address, or go to Home.", primary link "Go to Home".
- Error: title "Something went wrong", text "The page could not be shown. Try again later, or go to Home.", primary link "Go to Home".
- The title is the page's `heading-1` (an `h1` styled as the EmptyState title).
