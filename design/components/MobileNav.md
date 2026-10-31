# MobileNav

The top navigation under 860px: product name, avatar and a menu button that opens the links as a full-width list.

- `button.ht-nav__menu-btn` (Lucide `menu`, `x` when open) with `aria-expanded`; it is hidden on wide screens automatically.
- The open panel `nav.ht-mobile-menu` lists the main links with icons (current one marked `aria-current="page"`), a divider, then New goal, Profile, Send feedback, Sign out. Visitors get Catalog, Community, Sign in, Register.
- "+ New goal" in the bar is hidden on phones (`ht-hide-sm`); it moves into the menu.
- Close the menu on link click, Esc, or tap outside. In Blazor this is the only piece of TopNav that needs state.
