# MobileNav

The top navigation under 860px: product name, avatar and a menu button that opens the links as a full-width list.

- `button.ht-nav__menu-btn` (Lucide `menu`, `x` when open) with `aria-expanded`; it is hidden on wide screens automatically.
- The open panel `nav.ht-mobile-menu` lists the main links with icons (current one marked `aria-current="page"`), a divider, then New goal, Profile, Send feedback, Sign out. Visitors get Catalog, Community, Sign in, Register.
- "+ New goal" in the bar is hidden on phones (`ht-hide-sm`); it moves into the menu.
- Close the menu on link click, Esc, or tap outside. In Blazor this is the only piece of TopNav that needs state.

## Slice 1

- Signed in (registered user and administrator): the bar shows the avatar letter (first letter of the email, upper case) as the avatar-menu button of TopNav, and the menu lists Today, a divider, Profile and Sign out.
- Visitor: the menu lists Sign in and Register.
- The menu button has `aria-label` ("Menu", "Close menu" when open), `aria-expanded` and `aria-controls` naming the panel's `id`. When open, a transparent `div.ht-clickaway` lies under the bar and the panel; the button and the panel sit above it (`ht-layer-top`).
- Sign out is a button inside a form that posts to sign out, styled like the links.
- Closes on Esc, on a tap outside, and on choosing an item; focus goes back to the menu button. Opening one menu closes the other.
- The panel is never shown on wide screens, even if it was left open when the window grew.
