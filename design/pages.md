# Pages

Every page follows layout A: TopNav, a page head, content centered at `container-max`, Footer. The previews in the "Pages" group show each page with demo data.

## Home · visitor (`PageHomeVisitor`)
1. TopNav, visitor variant (Catalog, Community · Sign in, Register).
2. Hero band on `surface`: `display` headline "Keep the habits you choose", one sentence, "Register free" (primary) + "Browse the catalog" (secondary), privacy line. Right: an example GoalStats card with ConsistencyStrip.
3. How it works: three steps (Pick a habit · Record what happened · See your consistency).
4. Popular in the catalog: four HabitCards, link "See all".
5. Community: BandSummary with the opt-in / alphabetical sentence.

## Home · signed in (`PageHomeSignedIn`)
1. TopNav, signed-in variant with "+ New goal" and avatar.
2. Page head: greeting in `heading-1` and today's date.
3. Four StatTiles: Due today · Pending · Missed this week · Active goals.
4. Two columns: Today card (CommitmentRows: Pending, Due today, Missed) | GoalSummary + Recent notes.

## Catalog · visitor (`PageCatalogVisitor`)
1. Page head "Habit catalog" + SearchField.
2. Notice: "Register to turn any habit into your own goal" + Register.
3. FilterPanel (Category, Intent) | count + sort, grid of HabitCards with "See details".

## Catalog · signed in (`PageCatalogSignedIn`)
1. Page head + SearchField + "+ Create custom habit".
2. Tabs: Public catalog · My custom habits.
3. FilterPanel with "Only habits I track" | HabitCards: tracked ones show "N goals" and "Add another goal"; others "Use this habit".

## Profile (`PageProfile`)
1. Header: large avatar, name, email, member since.
2. Tabs: Account · Community · Goal history · Custom habits.
3. Community tab: participation Toggle, public display name, Save · "What others can see" card with the current band.
4. Goal history tab: DataTable of ended attempts (goal, attempt, ended, outcome, note).

## Admin (`PageAdmin`)
1. TopNav, admin variant.
2. Page head "Administration" + "+ New catalog habit".
3. Neutral privacy Notice (always visible).
4. Tabs: Catalog · Suggestions · Feedback · Accounts; each is a SearchField + DataTable with row actions.

## Goal detail (`PageGoalDetail`)
1. BackLink "← Goals", `heading-1` goal name, tags (category, Build / Reduce, Commitment-based / Period-based), attempt line.
2. Actions on the right: Edit settings (ghost), Pause (secondary), End goal (ghost), Record (primary). Pause and End goal open a Dialog.
3. Left column: GoalStats with ConsistencyStrip · TrendChart (amount per week or day, target line) · History table (date, recorded, StatusChip, note, Record / Edit / Add record).
4. Right column: Settings (KeyValue, "changes apply from the next commitment") · Pauses · Recent notes · Earlier attempts with outcome.

## New goal (`PageNewGoal`)
1. BackLink to where the user came from, `heading-1` "New goal", one line naming the catalog habit.
2. Form card in sections: Habit · Build / Reduce · Measure (Yes/no, Count, Duration) · Target ("At least" / "At most" + Stepper + unit) · When (fixed days with DayPicker, or N times per day / week) · Start date and optional name · Create goal / Cancel.
3. Sticky summary card: the goal as one plain sentence, how it is evaluated, the catalog suggestion note.

## Community (`PageCommunity`)
1. `heading-1` "Community" + "Last 30 days · only people who opted in".
2. Notice with the user's participation (public name, band, "Change in Profile"); other text for non-participants and visitors.
3. BandList (alphabetical names) | "How bands work" rules.
4. Catalog habit performance table: band and contributors, or "Insufficient community data" below 5 contributors.

## Dialogs (on Today and Goal detail)
- RecordDialog: "For" (today or a Pending date), the measure field (Yes/No, count Stepper, minutes Stepper with progress), optional note, Save record / Cancel, "Excuse instead" for commitment-based goals.
- NoteDialog: a private note on one date; it does not change the result.
- Excuse, Pause and End goal use the same Dialog and explain the effect before the confirm button.

## Goals (`PageGoals`)
1. `heading-1` "Goals" (New goal stays in the TopNav).
2. Tabs: Active · Paused · Ended.
3. One `ht-list` card of GoalListItems; EmptyState when there are none.

## Catalog habit details (`PageHabitDetail`)
1. BackLink "← Catalog", large glyph, name, tags, primary "Use this habit" (visitor: "Register to use this habit").
2. Description as a lead paragraph.
3. Suggested starting goals with "Start with this" · Your goals on this habit (signed in) | community performance.

## Sign in (`PageSignIn`) and Register (`PageRegister`)
1. Visitor TopNav, a centered 420px column (`ht-auth`), one Card form, a full-width primary button, a link to the other page.
2. Register says community comparison is off by default; the public name is asked only when opting in.

## Custom habit (`PageCustomHabit`)
1. BackLink "← My custom habits", `heading-1` "New custom habit".
2. Form: Name, Description, Category, usual intent, Icon. Actions: Save and create goal · Save habit · Cancel.
3. Sticky privacy card (private, never in statistics, Suggest for catalog later).

## Admin tabs (`PageAdminSuggestions`, `PageAdminFeedback`, `PageAdminAccounts`)
- Same head, privacy Notice and Tabs as `PageAdmin`.
- Suggestions: queue list | review form with Approve & publish · Save as draft · Reject.
- Feedback: filter + search, table with Open / Resolved Tags, the opened message with an internal note.
- Accounts: search + filter, table (email, public name, joined, community, status), Deactivate / Reactivate with DeactivateDialog.

## More dialogs
- PauseDialog (From, Until, reason), EndGoalDialog (outcome + note), FeedbackDialog (topic + message), SuggestDialog (custom habit → catalog), DeactivateDialog (admin, reason required).
- Edit goal settings reuses the New goal form with the title "Edit goal" and a Notice: "Changes apply from the next commitment. Past results keep their rules."

## Privacy (`PagePrivacy`)
1. TopNav (the variant of the session), page head `heading-1` "Privacy", one lead sentence.
2. Three Cards in one column: What stays private · Community comparison is opt-in · What administrators can see.
3. Footer. Anyone can open it from the Footer's "Privacy" link.

## Slice 1 (accounts and access)

What slice 1 builds of the pages above. A page shows only the sections and links whose slice has shipped (ADR 0008); the full previews stay as they are.

- Shell: TopNav and MobileNav with "Habit Tracker" linking to Home. Visitor: "Sign in", "Register". Registered user and administrator: "Today" and the avatar menu (Profile, Sign out); the narrow-screen menu lists Today, Profile, Sign out. Footer: the project line and "Privacy".
- Sign in (`PageSignIn`): Email, Password, "Keep me signed in", "Sign in", "New here? Create an account". No "Forgot password?". The failure message sits under the password field, the email is kept.
- Register (`PageRegister`): Email, Password with the rule as help text, Repeat password, the neutral Notice, "Create account". Error states next to their fields (taken email with a "Sign in" link, invalid email, the password rule, "The passwords do not match."); the email is kept.
- Home, visitor (`PageHomeVisitor`): the hero (headline, sentence, "Register free", privacy line) and "How it works" only. No "Browse the catalog", no example goal card, no catalog or community sections.
- Home, signed in (`PageHomeSignedIn`): the heading "Today" only.
- Profile (`PageProfile`): header with the avatar letter and the email, one "Account" Card without Tabs: the email and the "Change password" form with its error states and the Notice "Your password is changed.".
- Privacy (`PagePrivacy`): as above.
- Not found and Error: an EmptyState in the normal layout (see `components/EmptyState.md`).

## Slice 2 (habit catalog)

What slice 2 builds of the pages above, on top of slice 1. A page shows only the sections and links whose slice has shipped (ADR 0008); the full previews stay as they are, and each page's "Slice 2 state" block shows the built state.

- Shell: TopNav and MobileNav add "Catalog" for every audience and "Admin" for administrators. No Goals, Community or "+ New goal" yet.
- Catalog, visitor (`PageCatalogVisitor`, `/catalog`): page head "Habit catalog" with SearchField "Search habits"; the register Notice; FilterPanel with Category (radio buttons, "All" first, then each category that has a Published habit) and Intent (the checkboxes Build and Reduce); the count and "Sort: A–Z" as a plain label; HabitCards with "Suggested starting goal: …" and "See details". With no match, an EmptyState with "Clear filters". Filters and search are kept in the address (`q`, `category`, `intent`).
- Catalog, registered user (`PageCatalogSignedIn`): two tabs, each with its own address, as link Tabs with counts: "Public catalog" (`/catalog`, counts all Published habits) and "My custom habits" (`/custom-habits`, counts all of the user's custom habits). "+ Create custom habit" in the page head. Public catalog cards have "Details" only; My custom habits shows the user's custom habits with the "Private" Tag and "Edit", or an EmptyState. No "Only habits I track", no goal tags or goal actions.
- Catalog, administrator: the Public catalog without Tabs, without "+ Create custom habit" and without the register Notice.
- Catalog habit details (`PageHabitDetail`, `/catalog/{id}`): "← Catalog", glyph, name and tags, the description, "Suggested starting goals" with "Examples to adjust, not prescribed targets." and each one's text and kind. Visitors get the head button "Register to use this habit"; signed-in users get no head button. No "Your goals", no community card, no "Start with this". A Draft, an Unpublished or an unknown habit shows the Not found page.
- Custom habit (`PageCustomHabit`, `/custom-habits/new` and `/custom-habits/{id}`): "← My custom habits", the form (Name, Description, Category, "Usually you want to", the 24 Habit icons), "Save habit", Cancel and the privacy card; editing adds "Delete" with its confirmation Dialog. Refusals under their field, one at a time; "This custom habit does not exist." as a Notice above the form. After Save or Delete, back to `/custom-habits`.
- Admin (`PageAdmin`, `/admin`): "Administration", "+ New catalog habit", the privacy Notice, SearchField "Search catalog", "Showing N of M", and the catalog table (Habit, Category, Intent, Status as a Tag, Edit and Publish or Unpublish per row). One section, no Tabs, no "Updated" column.
- Administrator's catalog habit form (`PageAdminCatalogHabit`, new, `/admin/catalog/new` and `/admin/catalog/{id}`): the habit fields as the custom habit form, then up to 5 suggested starting goals (add, remove, move up and down, mark the card's one) with the New goal form's SegmentedControl, Stepper and DayPicker; refusals under their suggested starting goal's field and the list refusals above the list. Save and Cancel lead back to the Admin table.
- Profile (`PageProfile`): registered users get the link Tabs "Account" (`/profile`) and "Custom habits" (`/profile?tab=custom-habits`); Account keeps its slice 1 content. An administrator's Profile stays Account only, without Tabs.
- Home, visitor (`PageHomeVisitor`): the hero gains "Browse the catalog"; after "How it works", "Popular in the catalog" with four HabitCards and "See all N →"; the section is absent with no Published habit.
- Home, signed in (`PageHomeSignedIn`): a registered user gets an EmptyState under "Today" with "Browse the catalog"; an administrator's Home is unchanged.
