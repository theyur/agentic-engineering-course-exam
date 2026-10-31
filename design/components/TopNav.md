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
