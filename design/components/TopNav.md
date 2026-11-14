# TopNav

The top navigation bar of layout A: product name on the left, main links, account actions on the right.

## Variants
- Visitor: links Catalog, Community; right side "Sign in" (ghost) and "Register" (primary, small).
- Signed in: links Today, Goals, Catalog, Community; right side "+ New goal" and the avatar (opens Profile and Sign out).
- Administrator: Today, Catalog, Community, Admin; no private pages are linked.

## Rules
- Mark the current page with `aria-current="page"` on its link (shows the `accent` underline).
- Height `nav-height` (60px), white `surface` with a `line` bottom border. Content is centered at `container-max`.
- Under 860px the links are hidden and a menu button appears; see MobileNav. Visitor "Sign in" and "Register" carry `ht-hide-sm` and move into the menu.
- The product has no logo yet: set "Habit Tracker" in plain `heading` type, no mark.

## Avatar menu

The avatar is a button that opens a small floating menu with Profile and Sign out. The preview shows it closed, focused and open.

- The avatar shows the first letter of the account's email in upper case ("Y" for yuri@example.com).
- Markup: `div.ht-account` holds `button.ht-avatar.ht-avatar-btn` and, when open, `div.ht-popover` with the items. The button has `aria-label` ("Account menu for <email>"), `aria-haspopup="true"`, `aria-expanded` (`false` closed, `true` open) and `aria-controls` naming the menu's `id`.
- Items: "Profile" (a link, `user` icon) and "Sign out" (`log-out` icon), a button inside a form that posts to sign out. Sign out is never a link.
- Open: the menu hangs under the avatar, aligned to its right edge, on `surface` with `shadow-raised`. A transparent `div.ht-clickaway` covers the page under it; the menu and its button sit above that layer (`ht-layer-top`).
- Closes on Esc, on a click outside (on the click-away layer), and on choosing an item. Focus goes back to the avatar button.
- Focus: the 2px `accent` ring on the avatar button and on each item.
- In slice 1 the signed-in bar has only "Today" and the avatar (registered users and administrators alike; no Admin link yet). Other links appear with their slices (ADR 0008).

## Slice 2

The last three bars of the preview are slice 2 (HP-011, ADR 0008):
- Visitor: the link "Catalog", then "Sign in" and "Register".
- Registered user: "Today" and "Catalog", then the avatar menu.
- Administrator: "Today", "Catalog" and "Admin", then the avatar menu.
- No "Goals", "Community" or "+ New goal" yet (slices 4 and 9). The current page's link has `aria-current="page"`; "Catalog" is current on the catalog pages, and "Admin" on the Admin page and the administrator's catalog habit form.
