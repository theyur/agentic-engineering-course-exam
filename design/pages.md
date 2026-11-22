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
3. Four StatTiles: Due now · Pending · Missed this week · Active goals.
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
3. Left column: GoalStats with ConsistencyStrip · TrendChart (recorded progress per commitment, day or week, target line) · History table (date, recorded, StatusChip, note, Record / Edit / Add record).
4. Right column: Settings (KeyValue, "changes apply from the next commitment") · Pauses · Recent notes · Earlier attempts with outcome.

## New goal (`PageNewGoal`)
1. BackLink to where the user came from, `heading-1` "New goal", one line naming the habit.
2. Form card in sections: Habit · Build / Reduce · Measure (Yes/no, Count, Duration) · Target ("At least" / "At most" + Stepper + unit) · When (fixed days with DayPicker, or N times per day / week) · Start date and optional name · Create goal / Cancel.
3. Sticky summary card: the goal as one plain sentence, how it is evaluated, the catalog suggestion note.

## Community (`PageCommunity`)
1. `heading-1` "Community" + "Last 30 days · only people who opted in".
2. Notice with the user's participation (public name, band, "Change in Profile"); other text for non-participants and visitors.
3. BandList (alphabetical names) | "How bands work" rules.
4. Catalog habit performance table: band and contributors, or "Insufficient community data" below 5 contributors.

## Dialogs (on Today and Goal detail)
- RecordDialog: "For" (every Due and every Pending commitment or period), the measure field (Yes/No, count Stepper, minutes Stepper with progress), optional note, Save record / Cancel, "Excuse instead" for commitment-based goals.
- NoteDialog: a private note on one date; it does not change the result.
- Excuse, Pause and End goal use the same Dialog and explain the effect before the confirm button. The Excuse dialog has its own guide, `components/ExcuseDialog` (slice 7).

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
- Edit settings and Start new attempt reuse the New goal form on pages of their own (`PageEditGoalSettings`, `PageNewAttempt`, below). Changed 2026-11-22 for slice 8: this line said that Edit goal settings reuses the form "with the title 'Edit goal' and a Notice: 'Changes apply from the next commitment. Past results keep their rules.'", which disagreed with the spec (HP-002: "Changes apply from {Thu 12 Nov}. Past results keep their rules." above the actions).

## Edit settings (`PageEditGoalSettings`)
1. BackLink "← {goal name}", `heading-1` "Edit settings".
2. The New goal form card without Habit and Start date: Build / Reduce · Measure · Target · When (the goal's schedule kind only) · optional name · the neutral Notice "Changes apply from Thu 12 Nov. Past results keep their rules." · Save changes / Cancel.
3. The New goal page's sticky summary card.

## Start new attempt (`PageNewAttempt`)
1. BackLink "← {goal name}", `heading-1` "Start new attempt", the line "Attempt 2 starts with its own history and a streak from zero.".
2. The New goal form card without Habit and Name: Build / Reduce · Measure · Target · When (every schedule kind) · Start date · Start attempt / Cancel.
3. The New goal page's sticky summary card.

## Attempt view (`PageAttemptDetail`)
1. BackLink "← {goal name}", `heading-1` "Attempt 1 · 2 Nov – 15 Nov" (or "Attempt 1 · ended before it began"), the attempt's name, the outcome Tag and the ending note.
2. One column: GoalStats with the eyebrow "Attempt 1" · History table with the goal page's record and note actions. No actions, trend, Settings, Pauses or Recent notes.

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

## Slice 4 (goals)

What slice 4 builds of the pages above, on top of slices 1 and 2. A page shows only the sections and links whose slice has shipped (ADR 0008); the full previews stay as they are, and each page's "Slice 4 state" block shows the built state. Texts that disagreed with the spec were replaced in the full previews too: "Yoga, 3 days a week" for fixed days Mon, Wed and Fri, "Coffee, at most 2 cups a day", the streak unit "commitments" for commitment-based goals, "Each scheduled day", the "Build or reduce" row, and the summary sentence of HP-002.

- Shell: TopNav and MobileNav add "Goals" and "+ New goal" for registered users only (MobileNav: Goals among the main links, "New goal" after the divider, before Profile). The avatar letter is the first letter of the account name, or of the email without one.
- New goal (`PageNewGoal`, `/goals/new`, interactive): the head line for a catalog habit, a custom habit or no habit; Habit with Change and the habit chooser (a Dialog with a SearchField over the Published catalog habits and the user's custom habits); Build or Reduce, measure and "When?" as SegmentedControl and ChoiceCards; Target with Stepper and unit; Start date and "Name (optional)"; "Create goal" with no confirmation. The summary card: the HP-002 sentence, Evaluated, "Met when", "Streak counted in", "Late records", the first-week Notice and the catalog suggestion Notice, or "Complete the form to see your goal here." while the form is not complete. Refusals under their field, one at a time.
- Goals (`PageGoals`, `/goals`): "Goals", no Tabs, one `ht-list` of reduced GoalListItems (name link, Build or Reduce Tag, meta line with the schedule and "first week starts …", "starts …" or "next …"); the EmptyState "No goals yet" with "Browse the catalog" and "Create custom habit".
- Goal page (`PageGoalDetail`, `/goals/{id}`): "← Goals", the name, the tags, the attempt line, the Notice for a habit no longer in the catalog, and the Settings card (Habit, Build or reduce, Measure, Target, Schedule, Evaluated, First week for per-week goals, Started) with "Changes apply from the next commitment. Past results keep their rules.". No actions, stats, trend, history, pauses, notes or earlier attempts.
- Catalog, registered user (`PageCatalogSignedIn`): "N goals" and "Add another goal" on tracked habits' cards, "Use this habit" on the others, "Show: Only habits I track" (`tracked=true`), and "· you track N" after the count. Visitors and administrators: unchanged.
- Catalog habit details (`PageHabitDetail`), registered user: the head button "Use this habit", "You track this · N goals", "Start with this" on each suggested starting goal, and "Your goals on this habit" with links to the goal pages. No community card.
- Custom habit (`PageCustomHabit`): primary "Save and create goal" beside "Save habit"; "Delete" only when no goal uses the habit.
- Admin (`PageAdmin`): Unpublish of a habit that goals use opens a Dialog with "N goals use this habit." and a sentence that the goals keep working for their owners, then "Unpublish" and "Cancel". Still no "Updated" column.
- Home, signed in (`PageHomeSignedIn`): the greeting ("Good evening, Yuri", or "Good evening" without a name) as `heading-1` and the date ("Tuesday, 17 November") for registered users and administrators, replacing "Today"; a registered user with no goals sees the EmptyState "No goals yet" with "Browse the catalog" and "Create custom habit".
- Home, visitor (`PageHomeVisitor`): unchanged.
- Profile (`PageProfile`): the header with the name, the email and "member since November 2026" (each only when known); the Account tab adds the "Name" form ("Your name is changed.") and the "Time zone" form ("Your time zone is changed. Your goals use it from their next day or week; earlier days keep their time zone.").
- Register (`PageRegister`): the optional "Name" field, and the browser's time zone sent in a hidden input, with no visible field.

## Slice 5 (recording and evaluation)

What slice 5 builds of the pages and dialogs above, on top of slices 1, 2 and 4. A page shows only the sections and links whose slice has shipped (ADR 0008); the full previews stay as they are, and each preview's "Slice 5 state" block shows the built state. Texts that disagreed with the spec were replaced in the full previews too: a Reduce goal's Met is "Met", with no word of its own (the Reduce row of the Today card on the signed-in Home and in CommitmentRow, a day still running, says "Due today"), the RecordDialog's Reduce rule is no longer provisional, and its "For" rule is FR-011's (every Due and every Pending commitment or period).

- Goal page (`PageGoalDetail`, `/goals/{id}`):
  1. Slice 4's "← Goals", name, tags, attempt line and Notice, with the primary action "Record" on the right of the page head (absent when nothing can be recorded now). No Edit settings, Pause or End goal.
  2. Left column: the History card. Its head has the title "History" and the muted sentence "Corrections are evaluated with the rules of that date.". Then a DataTable with Date, Recorded, Result (StatusChip), Note and an actions column: the commitments or periods that have begun in the last 366 days up to the goal's current date, newest first, the first 10, then "Show all N →". Rows: "Add note" (no note) or "Edit note" (a note), then "Add record", on a commitment or period without a record; "Edit" on a commitment with a record; "Add record" on a period with 1 to 49 records, nothing on one with 50; each period record as its own button; the Note cell is the note as plain text (`ht-muted`), or "—"; final rows included. Amended 2026-11-20, specs/006-note-editing, ADR 0019: the Note cell was "Add note" or the note as a button on every row. Amended 2026-11-20 (owner's decision after implementation): the note control now comes before "Add record", so the record control is always the rightmost; it came after "Add record" before.
  3. Right column: slice 4's Settings card, then the Recent notes card (at most 5, newest first, each with its date and StatusChip; absent without notes).
  4. No "Record", History card or Recent notes card while the goal has no begun entry. No GoalStats, strip, trend, pauses or earlier attempts.
- RecordDialog (`components/RecordDialog`, `RecordForms`): the title "Record · {goal name}"; the sub line with Build or Reduce and the target; "For" with every Due and every Pending entry ("Today, Fri 30 Oct", "Wed 28 Oct · Pending, 22 h left", a week as "26 Oct – 1 Nov" with a "Date" select of its dates), or fixed to the History row it was opened from; the field "Did you do it?", "Amount" or "Time spent"; the live progress line; the partial-progress sentence (Build count and duration goals) or the Reduce help; "Note (optional)", opened with the current note of the entry "For" names (empty without one; another "For" replaces it, another "Date" keeps it), with "Clear the field to remove the note." (commitment-based), "This note is for the whole day. Clear the field to remove it." (per-day period) or "This note is for the whole week. Clear the field to remove it." (per-week period); "Save record" and "Cancel"; "Edit" of one record adds "Delete record". No "Excuse instead". Amended 2026-11-20, specs/006-note-editing, ADR 0019: the note field was empty, with "An empty field keeps the existing note.".
- NoteDialog (`components/NoteDialog`): opened only from a History row without a record, by its "Add note" (the title "Add note · {goal name}", empty) or "Edit note" (the title "Edit note · {goal name}", filled), the title fixed on open; the sub line with the date and the state ("Tue 27 Oct · Missed"), the help "Notes are private. They do not change the result.", "Save note" and "Cancel"; saving it empty removes the note. Amended 2026-11-20, specs/006-note-editing, ADR 0019: it was opened from a History row's "Add note" or its note, always titled "Add note · {goal name}".
- StatusChip (`components/StatusChip`): "Met", "Failed", "Missed", "Pending", "Due today" (a commitment or a day) and "Due this week" (a week), always with their word. No Upcoming chip, Excused or Paused.
- New goal (`PageNewGoal`): "How do you measure it?" gains its help line "You can record partial amounts; the target decides the result.".
- Home, signed in (`PageHomeSignedIn`), Goals (`PageGoals`) and the other pages: unchanged in slice 5.

## Slice 6 (personal reports)

What slice 6 builds of the pages above, on top of slices 1, 2, 4 and 5. A page shows only the sections and links whose slice has shipped (ADR 0008); the full previews stay as they are, and each preview's "Slice 6 state" block shows the built state. Texts that disagreed with the spec were replaced in the full previews too: the upcoming StatusChip reads "Upcoming" (not "Due Mon"), the Home's first StatTile reads "Due now" (not "Due today"), the TrendChart shows recorded progress, an amount or a number of records, not verdicts, with weeks for per-week goals and commitment-based yes/no goals and commitments or days otherwise, and a commitment-based yes/no goal's trend is titled "Days done per week" (not "Sessions per week"). In every slice-6 state a consistency or streak value is one whole text ("90% (18 of 20)", "6 commitments"), with no `<small>` part.

- Goal page (`PageGoalDetail`, `/goals/{id}`): slice 5's page, with two cards added in the left column above the History card:
  1. GoalStats: the eyebrow "This attempt", the title "Consistency and streaks", "Consistency" ("92% (24 of 26)" or "N/A"), "Streak, {unit}" and "Longest, {unit}" (plural unit), then the ConsistencyStrip of the last 30 ended results with its legend (Met, Failed, Missed, Pending);
  2. TrendChart: the title and sub line of the goal's kind ("Minutes per day", "Last 14 days"), the bars (an empty outline for no record), the dashed target line "Target {n} min" or "Target {n}", the date labels.

  A goal with no begun entry keeps the slice 5 page without these cards. No pauses or earlier attempts.
- Goals (`PageGoals`, `/goals`): still no Tabs; full GoalListItems, goals with a Pending result first, then those with a Due one, then the others, each group by name. Each item: the name link, the Build or Reduce Tag, a Pending StatusChip, slice 4's meta line plus the progress of a Due entry with a record ("12 of 20 min today", "1 of 2 this week"), "Consistency", "Current streak", the mini strip of the last 15 results, and "Record" (primary with a Due entry, secondary with only Pending ones, absent otherwise). The footnote "Each goal is counted on its own. Consistency counts only finalized results; pending ones are left out." No goals: slice 4's EmptyState.
- Home, signed in (`PageHomeSignedIn`), a registered user with goals, under slice 4's page head:
  1. four StatTiles across the page: "Due now", "Pending · still can record", "Missed this week" (buttons with `aria-pressed` that filter the Today list) and "Active goals" (a link to `/goals`);
  2. two columns: the Today card (CommitmentRows: Pending, then Due, then Missed this week, then Upcoming; "Record", "Edit", "Add note" or "Edit note", none on Upcoming; "Nothing to record right now." with no row) | the "Active goals" card (GoalSummary, at most 5, then "All goals →") and the "Recent notes" card (at most 5; absent without notes).

  No goals: slice 4's EmptyState. An administrator: the page head only. No "Excuse" (slice 7).
- Home, visitor (`PageHomeVisitor`): the hero gains the GoalStats card labelled "Example goal" ("Yoga, 3 days a week", "90% (18 of 20)", streak 6 and longest 11 commitments, a strip of 21 fixed results with its legend). Its figures are fixed examples that belong to no account; it shows with or without Published habits. The rest is slice 2's.
- StatusChip (`components/StatusChip`): slice 5's chips plus "Upcoming" (`ht-status--due`) on the Home's Upcoming rows.
- The other pages and dialogs: unchanged in slice 6.

## Slice 7 (excusals and pauses)

What slice 7 builds of the pages and dialogs above, on top of slices 1, 2 and 4 to 6. A page shows only the sections and links whose slice has shipped (ADR 0008); the full previews stay as they are, and each preview's "Slice 7 state" block shows the built state. Texts that disagreed with the spec were replaced in the full previews too: the RecordDialog's "Excuse instead" is confirmed (no longer provisional) and offered only while the commitment is Due or Pending with no record (D-2, D-3); the PauseDialog's title, effect and Notice example follow HP-002 ("Pause · {goal name}", "Paused days are not counted. Your streak stays as it is. Final results do not change.", no sub line, help or placeholder); the Dialog preview's Excuse content follows HP-003 ("Excuse · {goal name}", "Excuse commitment"); the goal page's Pauses card lists its pauses latest first as "Mon 21 Sep – Sun 4 Oct", with no StatusChip. In every slice-7 state the words "Excused" and "Paused" are StatusChip words, and the strip's legend lists Met, Failed, Missed, Excused, Paused, Pending.

- Goal page (`PageGoalDetail`, `/goals/{id}`): slice 6's page, with:
  1. the secondary "Pause" before "Record" in the page head (absent while the goal is paused), which opens the PauseDialog;
  2. under the head, while the goal is paused, the Notice "Paused until Sun 22 Nov · Resume now", "Resume now" a button;
  3. in the right column after Settings, the "Pauses" card: each pause as "Mon 16 Nov – Sun 22 Nov", latest first, its reason in `ht-muted`, "Resume now" on the current pause, "Cancel pause" on an upcoming one, nothing on a past one; absent with no pause;
  4. in the History card, the StatusChips "Excused" (with its reason after the chip, and the ghost "Remove excusal" while its grace period is open) and "Paused"; neither row has "Add record"; a record kept on a Paused row keeps its edit control; both keep their note actions;
  5. in the GoalStats card, Excused and Paused cells and the six-entry legend; in the TrendChart, the dashed Paused and Excused bars, told apart in their `title` and in the chart's `aria-label`.

  Still no Edit settings, End goal or earlier attempts.
- Goals (`PageGoals`, `/goals`): the link Tabs "Active" (`/goals`) and "Paused" (`/goals?tab=paused`), no "Ended" and no counts. Active lists the goals that are not paused, as slice 6 does. Paused lists the paused goals in the same order, each with the meta "Paused until Sun 22 Nov", the Pending StatusChip when it has a Pending result, its figures and mini strip, and the secondary "Resume"; an empty Paused tab says "No paused goals.". The footnote reads "Each goal is counted on its own. Consistency counts only finalized results; pending, excused and paused ones are left out.". No goals: slice 4's EmptyState in place of the tabs.
- Home, signed in (`PageHomeSignedIn`): the ghost "Excuse" after "Record" on Pending rows of commitment-based goals with no record, which opens the Excuse dialog; the "Active goals" tile and card count and list only goals that are not paused. Excused and Paused entries are not rows and are not counted in the tiles. Nothing else changes.
- Home, visitor (`PageHomeVisitor`): the example GoalStats card's legend lists the six entries, through the shared component; its strip, figures and label are slice 6's.
- PauseDialog (`components/PauseDialog`): the title "Pause · {goal name}"; the effect first; "From" (the page's current date by default, `min` today), "Until" (required) and "Reason (optional)"; "Pause goal" and "Cancel"; one refusal at a time under its field: "Choose the first and the last day." (the empty field), "A pause cannot start before today." ("From"), "The last day cannot be before the first day." and "This goal already has a pause on these dates." ("Until"), "Keep the reason to 200 characters or fewer." ("Reason").
- Excuse dialog (`components/ExcuseDialog`, new, a guide of its own; `Dialog.md` links to it): the title "Excuse · {goal name}"; the sub line with the date and state ("Wed 11 Nov · Pending, 22 h left", "Today, Fri 13 Nov"); the effect "An excused commitment is not counted. Your streak stays as it is."; "Reason (optional)"; "Excuse commitment" and "Cancel"; a refusal of the reason under the field, any other above it. It opens from the Home's "Excuse" and the RecordDialog's "Excuse instead".
- RecordDialog (`components/RecordDialog`): "For" lists no Excused or Paused entry; the ghost "Excuse instead", pushed right, shows only for a commitment-based goal whose selected "For" entry is Due or Pending with no record, and opens the Excuse dialog in the RecordDialog's place.
- StatusChip, ConsistencyStrip, GoalStats, GoalListItem, CommitmentRow and TrendChart (`components/`): each has a "Slice 7 state" block: the chips "Excused" (`ht-status--excused`) and "Paused" (`ht-status--paused`); the cells `ht-cell--excused` and `ht-cell--paused` with their shapes and the strip label that counts them ("Last 17 commitments: 10 met, 1 missed, 1 excused, 3 paused, 2 pending"); the item's Paused form; the row's "Excuse"; the marked bars and the new `ht-bar--excused` class in `css/ht-components.css`.
- The other pages and dialogs: unchanged in slice 7.

## Slice 8 (goal changes)

What slice 8 builds of the pages and dialogs above, on top of slices 1, 2 and 4 to 7. A page shows only the sections and links whose slice has shipped (ADR 0008); the full previews stay as they are, and each preview's "Slice 8 state" block shows the built state. Texts that disagreed with the spec were replaced in the full previews too: the EndGoalDialog opens with no outcome chosen (it marked "Achieved") and its title names the goal ("End goal · Yoga, 3 days a week", not "End goal · Yoga"); the goal page's Earlier attempts card shows "Attempt 1 · 2 Mar – 30 Jun" as a link with the outcome Tag (not "Attempt 1" with "2 Mar – 30 Jun 2026 · Abandoned"); and the Edit settings page is a page of its own with the line "Changes apply from Thu 12 Nov. Past results keep their rules." (not "Edit goal" with "Changes apply from the next commitment."). Every outcome word, "Ended …", attempt line, "From …", "Changes apply from …" and "Attempt n starts with …" comes from `ReportText` or `GoalChangeTexts` (HP-008), and an outcome Tag always carries its word.

- Goal page (`PageGoalDetail`, `/goals/{id}`): slice 7's page, with:
  1. in the head of a goal that is not ended, the ghost "Edit settings" first (a link to the Edit settings page), slice 7's "Pause", the ghost "End goal" (opens the EndGoalDialog), then "Record";
  2. the attempt line for the latest attempt: "Attempt 2 · started 16 Nov", "Attempt 2 · starts 23 Nov", or "Attempt 1 · ended before it began" for an ended latest attempt that never began;
  3. in the Settings card, the row "From Thu 12 Nov" with the later target text after the Target row when a later version exists, and the start row "Started" or "Starts" with the latest attempt's date, absent for an ended goal that never began;
  4. for an ended goal, under the head, the Notice "Ended Mon 16 Nov · Abandoned" with "Start new attempt", and no "Record", "Edit settings", "Pause" or "End goal"; the History card keeps its record and note actions for the ended attempt's dates;
  5. in the right column after Recent notes, the "Earlier attempts" card: each ended attempt that is not the latest, latest first, "Attempt 1 · 2 Nov – 13 Nov" (or "Attempt 1 · ended before it began") as a link to the attempt view, the outcome Tag, the consistency, the attempt's name when it differs from the goal's, and the note in `ht-muted`; absent with one attempt;
  6. each History row, and the RecordDialog and NoteDialog fixed to it, use that row's own settings.
- Edit settings (`PageEditGoalSettings`, new, `/goals/{id}/edit`): "← {goal name}", "Edit settings"; the New goal form's Build or Reduce, measurement, target, schedule (the goal's kind only: no "On fixed days" / "A number of times" choice, no "Per day" / "Per week" control) and "Name (optional)", filled with the latest settings; no Habit and no start date; above the actions the neutral Notice "Changes apply from Thu 12 Nov. Past results keep their rules."; "Save changes" and "Cancel"; the summary card, its sentence with the date the change applies from; one refusal at a time under its field. An ended goal returns to its goal page.
- Start new attempt (`PageNewAttempt`, new, `/goals/{id}/new-attempt`): "← {goal name}", "Start new attempt" with the line "Attempt 2 starts with its own history and a streak from zero." under it; the New goal form with every schedule kind and the start date, filled with the starting values; no Habit and no name; "Start attempt" and "Cancel"; the summary card; one refusal at a time under its field, with "A new attempt starts after the day the goal ended." under the start date. A goal that is not ended returns to its goal page. It is a page of its own, not a state of `PageNewGoal` (HP-004 allows either): its address, its head line and its missing fields differ, and it starts from an existing goal rather than a habit (settled in T005).
- Attempt view (`PageAttemptDetail`, new, `/goals/{id}/attempts/{n}`): "← {goal name}", the heading "Attempt 1 · 2 Nov – 15 Nov" or "Attempt 1 · ended before it began", the attempt's name, the outcome Tag and the note; one column with GoalStats ("Attempt 1") and the History card with the goal page's record and note actions; no head actions, trend, Settings, Pauses or Recent notes card. An attempt with no date shows "N/A" and no History card. The latest attempt's number shows the Not found page.
- EndGoalDialog (`components/EndGoalDialog`): the title "End goal · {goal name}", the attempt line as sub line, the effect first, "How did it end?" as ChoiceCards Achieved, Abandoned and No longer relevant in one column with none chosen, "Note (optional)", "End goal" and "Cancel"; "Choose how the goal ended." under "How did it end?" and "The note can have at most 500 characters." under the note; after an ending the focus goes to the Notice's "Start new attempt".
- Goals (`PageGoals`, `/goals`): the link Tabs "Active", "Paused" and "Ended" (`/goals?tab=ended`). Active and Paused leave ended goals out. Ended lists the ended goals in the overview's order, each with the outcome Tag, the Pending StatusChip when it has a Pending result, the meta "Ended Mon 16 Nov", its figures and mini strip, and the secondary "Start new attempt" in place of "Record". An empty Ended tab says "No ended goals.". The footnote is slice 7's under every tab.
- Home, signed in (`PageHomeSignedIn`): the Today list and the "Pending · still can record" tile keep the Pending rows of ended attempts until their grace period ends, a row of an ended goal with the Tag "Ended" after its name, a row of a restarted goal's earlier attempt with the Tag "Attempt 1" after that attempt's name; ended goals have no other row and are left out of the other tiles, the "Active goals" card and the recent notes.
- Profile (`PageProfile`): registered users get the tab "Goal history" (`/profile?tab=history`) between "Account" and "Custom habits", no Community tab; a DataTable with Goal (a link to the attempt view, or to the goal page for the goal's latest attempt), Attempt, Ended ("Mon 16 Nov"), Outcome (Tag) and Note (`ht-muted`, or "—"), the latest ending first; "No ended goals yet." in place of an empty table. An administrator's Profile keeps its Account content without Tabs.
- GoalListItem and CommitmentRow (`components/`): each has a "Slice 8 state" block: the item's Ended form; the row's "Ended" and "Attempt n" Tags, before a Reduce goal's "Reduce" Tag.
- No `design/css` change: every part uses existing tokens and `ht-` classes.
- The other pages and dialogs: unchanged in slice 8.
