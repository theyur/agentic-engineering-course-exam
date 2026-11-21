# Habit Tracker — design system

Design: **Ink Blue & Haze**, **layout A** (top navigation, centered content, sections stacked down the page).
This folder is the single source of truth for the UI of `HT.Ui` (Blazor).

## How to use this folder (for people and for Claude Code)

1. Read [brand-book.md](brand-book.md) first: writing rules, colors, type, spacing, shapes, icons.
2. Read [pages.md](pages.md) for the structure of each page and dialog, then open the page preview in `pages/`.
3. Before building a UI piece, read its guide in `components/<Name>.md`; its `.html` file shows the exact markup and classes.
4. In the app, load the two stylesheets and use only their variables and classes:
   - `css/tokens.css` — design tokens as CSS variables (`--accent`, `--space-4`, `--radius-md`, …). Generated from `tokens.json`; change values in `tokens.json` and regenerate, do not edit by hand.
   - `css/ht-components.css` — component classes with the `ht-` prefix. Wrap pages in `class="ht"`.

All `.html` files open directly in a browser (they link `../css/*.css`). They are references with demo data, not production code.

Many previews end with "Slice N state" blocks after the full design: what slice N builds of that page or component (ADR 0008). Build from the latest slice block that exists, and keep the full design as the target of later slices. [pages.md](pages.md) sums up each slice; slice 4 (goals) adds the states of New goal, Goals, the goal page, the signed-in catalog, the habit details, the custom habit page, Admin, the signed-in Home, Profile, Register, TopNav, MobileNav, HabitCard and GoalListItem; slice 5 (recording and evaluation) adds the states of the goal page (Record, History and Recent notes), RecordDialog, RecordForms, NoteDialog and StatusChip; slice 6 (personal reports) adds the states of the goal page (GoalStats and TrendChart), Goals, the signed-in Home (StatTiles, Today, Active goals, Recent notes), the visitor Home (the example GoalStats card), GoalStats, ConsistencyStrip, TrendChart, GoalListItem, CommitmentRow, StatTile, StatusChip ("Upcoming") and GoalSummary; slice 7 (excusals and pauses) adds the states of the goal page (Pause, the paused Notice, the Pauses card, Excused and Paused History rows, cells and bars), Goals (the Tabs Active and Paused), the signed-in Home ("Excuse", Active goals without paused goals), the visitor Home (the six-entry legend), PauseDialog, ExcuseDialog (new), RecordDialog ("Excuse instead"), StatusChip, ConsistencyStrip, GoalStats, GoalListItem, CommitmentRow and TrendChart. In every slice-6 state a consistency or streak value is one whole text ("90% (18 of 20)", "6 commitments"), with no `<small>` part.

## Rules for implementation

- Copy `css/tokens.css` and `css/ht-components.css` to `src/HT.Ui/wwwroot/css/` and link them in `App.razor` (tokens first). Remove the template Bootstrap leftovers from `app.css`.
- Use only `var(--…)` tokens and `ht-` classes. No hard-coded colors, sizes or shadows; no Bootstrap; no new colors.
- Build each component once as a Razor component (`HtButton`, `StatusChip`, `HabitCard`, `CommitmentRow`, `HtDialog`, …) that renders the markup from its preview, and reuse it.
- Match page structure and wording of the page previews. Use the SPEC words exactly: Pending, Met, Failed, Missed, Excused, Paused (a Reduce goal's Met is "Met" too); consistency as "90% (18 of 20)", "N/A" when there is no data.
- A status is never shown by color alone: always a word (StatusChip) or a shape (ConsistencyStrip).
- A catalog habit's marked suggested starting goal is labelled "Suggested starting goal: …" on its HabitCard, never "Suggested start"; the detail page lists all of them under "Examples to adjust, not prescribed targets.".
- Keep text contrast at least 4.5:1 and the 2px `--accent` focus ring on every focusable element.
- Dialogs: focus the first field on open, close with Esc and ×, return focus to the opener.
- Layout breakpoints: 860px (columns stack, nav links collapse) and 480px (phone padding).
- Fonts: Sora (headings, numbers) and Source Sans 3 (text) from Google Fonts, imported at the top of `ht-components.css`.
- `ht-backdrop--static` is only for documentation previews; the app uses `ht-backdrop`.

## Pages

- `PageAdmin` — The admin page: privacy Notice, Tabs for Catalog, Suggestions, Feedback and Accounts, each a DataTable. ([notes](pages/PageAdmin.md), [preview](pages/PageAdmin.html))
- `PageAdminCatalogHabit` — The administrator's catalog habit form (create and edit): the habit fields and an editor for up to 5 suggested starting goals. New in slice 2. ([notes](pages/PageAdminCatalogHabit.md), [preview](pages/PageAdminCatalogHabit.html))
- `PageAdminAccounts` — Admin · Accounts tab: search, filter (All · Active · Deactivated) and a table of accounts. ([notes](pages/PageAdminAccounts.md), [preview](pages/PageAdminAccounts.html))
- `PageAdminFeedback` — Admin · Feedback tab: filter (Open · Resolved · All), search, a table of messages and the opened message below. ([notes](pages/PageAdminFeedback.md), [preview](pages/PageAdminFeedback.html))
- `PageAdminSuggestions` — Admin · Suggestions tab: a queue on the left, the review form on the right. ([notes](pages/PageAdminSuggestions.md), [preview](pages/PageAdminSuggestions.html))
- `PageCatalogSignedIn` — The catalog for signed-in users: Public / My custom habits tabs, extra filter and goal actions on each card. ([notes](pages/PageCatalogSignedIn.md), [preview](pages/PageCatalogSignedIn.html))
- `PageCatalogVisitor` — The catalog for visitors: search, a register Notice, FilterPanel on the left and a grid of HabitCards. ([notes](pages/PageCatalogVisitor.md), [preview](pages/PageCatalogVisitor.html))
- `PageCommunity` — The full Community page: participation status, Consistency Bands with names, rules, and catalog habit performance. ([notes](pages/PageCommunity.md), [preview](pages/PageCommunity.html))
- `PageCustomHabit` — Create or edit a private custom habit. ([notes](pages/PageCustomHabit.md), [preview](pages/PageCustomHabit.html))
- `PageGoalDetail` — The goal page: everything about one attempt of one goal — stats, trend, history with corrections, settings, pauses, notes and earlier attempts. Slice 4 builds the head and the Settings card; slice 5 adds "Record", the History card and the Recent notes card; slice 6 adds the GoalStats and TrendChart cards above History; slice 7 adds Pause, the paused Notice, the Pauses card, and Excused and Paused rows, cells and bars. ([notes](pages/PageGoalDetail.md), [preview](pages/PageGoalDetail.html))
- `PageGoals` — The Goals list: Tabs Active · Paused · Ended, and one GoalListItem per goal. Built first in slice 4, without Tabs; slice 6 shows the full items and the footnote, still without Tabs; slice 7 adds the Tabs Active and Paused. ([notes](pages/PageGoals.md), [preview](pages/PageGoals.html))
- `PageHabitDetail` — The catalog habit page, opened from "See details" / "Details" on a HabitCard. ([notes](pages/PageHabitDetail.md), [preview](pages/PageHabitDetail.html))
- `PageHomeSignedIn` — The signed-in Home (Today): greeting, four StatTiles, the Today list and active goals. Built in slice 6 for users with goals: "Due now", Pending, Missed this week and Active goals, the Today rows with Upcoming last, Active goals and Recent notes. Slice 7 adds "Excuse" on Pending rows and leaves paused goals out of Active goals. ([notes](pages/PageHomeSignedIn.md), [preview](pages/PageHomeSignedIn.html))
- `PageHomeVisitor` — The visitor Home in layout A: hero with an example goal, How it works, catalog preview and community bands. Slice 6 adds the example goal card, with fixed example figures that belong to no account; slice 7 gives its legend six entries. ([notes](pages/PageHomeVisitor.md), [preview](pages/PageHomeVisitor.html))
- `PageNewGoal` — The New goal form, opened from "Use this habit" (pre-filled with the suggested starting goal) or "+ New goal". ([notes](pages/PageNewGoal.md), [preview](pages/PageNewGoal.html))
- `PagePrivacy` — Privacy: what stays private, that community comparison is opt-in and off by default, and what administrators cannot see. ([notes](pages/PagePrivacy.md), [preview](pages/PagePrivacy.html))
- `PageProfile` — The personal profile: header with avatar, name and email, then Tabs for Account, Community, Goal history and Custom habits. ([notes](pages/PageProfile.md), [preview](pages/PageProfile.html))
- `PageRegister` — Register: same `ht-auth` layout as Sign in. ([notes](pages/PageRegister.md), [preview](pages/PageRegister.html))
- `PageSignIn` — Sign in: a centered 420px column (`ht-auth`) with one Card form. ([notes](pages/PageSignIn.md), [preview](pages/PageSignIn.html))

## Components

**Foundations**
- `Icons` — The icon set is Lucide (version 0.460.0, ISC license): 24px line icons, drawn with `stroke-width` 1.75, round caps, `currentColor`. ([guide](components/Icons.md), [preview](components/Icons.html))

**Navigation**
- `BackLink` — A small link above the page title that goes one level up: "← Goals", "← Catalog". ([guide](components/BackLink.md), [preview](components/BackLink.html))
- `Footer` — A thin footer on every page with the project line and links to Privacy and Send feedback. ([guide](components/Footer.md), [preview](components/Footer.html))
- `MobileNav` — The top navigation under 860px: product name, avatar and a menu button that opens the links as a full-width list. ([guide](components/MobileNav.md), [preview](components/MobileNav.html))
- `SideMenu` — A vertical list of links for a section with several sub-pages, used inside a page (not as the main navigation). ([guide](components/SideMenu.md), [preview](components/SideMenu.html))
- `Tabs` — Tabs switch between views of the same page: catalog vs. my custom habits, profile sections, admin queues. ([guide](components/Tabs.md), [preview](components/Tabs.html))
- `TopNav` — The top navigation bar of layout A: product name on the left, main links, account actions on the right. ([guide](components/TopNav.md), [preview](components/TopNav.html))

**Content**
- `Card` — A card is the main container on every page: white `surface`, 1px `line` border, `radius-md`, `shadow-card`, 18px padding. ([guide](components/Card.md), [preview](components/Card.html))
- `EmptyState` — What a list shows when it has nothing yet: an icon, a short title, one or two sentences and the next step. ([guide](components/EmptyState.md), [preview](components/EmptyState.html))
- `KeyValue` — A two-column list of labels and values for read-only settings: `dl.ht-kv` with `dt` (muted) and `dd` (semibold). ([guide](components/KeyValue.md), [preview](components/KeyValue.html))
- `Notice` — A notice is a full-width message inside a page: an invitation (accent-soft) or a neutral rule (surface-2). ([guide](components/Notice.md), [preview](components/Notice.html))

**Actions**
- `Button` — Buttons start an action; one primary button per card or page area, for the thing that area is for. ([guide](components/Button.md), [preview](components/Button.html))

**Status**
- `StatusChip` — A status chip shows the evaluation state of one commitment or period, always as a word plus a coloured dot. Slice 5 uses Met, Failed, Missed, Pending, Due today and Due this week; slice 6 adds Upcoming; slice 7 adds Excused and Paused. ([guide](components/StatusChip.md), [preview](components/StatusChip.html))
- `Tag` — A tag is a small neutral label for categories, Build / Reduce intent and admin states. ([guide](components/Tag.md), [preview](components/Tag.html))

**Tracking**
- `CommitmentRow` — One row per commitment or period in the Today list: name, schedule or progress, a status chip and the next action. ([guide](components/CommitmentRow.md), [preview](components/CommitmentRow.html))
- `ConsistencyStrip` — A grid of small squares, one per commitment or period, oldest first, showing how each was evaluated. ([guide](components/ConsistencyStrip.md), [preview](components/ConsistencyStrip.html))
- `GoalListItem` — One goal in the Goals list: name and schedule, consistency, current streak, the last 15 results and the next action. Slice 4 builds a reduced item: name, Build or Reduce Tag and meta line; slice 6 builds the full item; slice 7 adds its Paused form. ([guide](components/GoalListItem.md), [preview](components/GoalListItem.html))
- `GoalStats` — The goal summary card: consistency, current streak and longest streak, with the history strip below. ([guide](components/GoalStats.md), [preview](components/GoalStats.html))
- `GoalSummary` — A compact list of active goals with consistency and streak, used on the signed-in Home. ([guide](components/GoalSummary.md), [preview](components/GoalSummary.html))
- `StatTile` — A tile with one big number and a short label, in a row of four at the top of the signed-in Home. ([guide](components/StatTile.md), [preview](components/StatTile.html))
- `TrendChart` — A simple bar chart of recorded progress per commitment, day or week, with the target as a dashed line. Slice 6 fixes its title and sub line per goal kind; slice 7 adds the dashed Paused and Excused bars. ([guide](components/TrendChart.md), [preview](components/TrendChart.html))

**Catalog**
- `FilterPanel` — The left column of the catalog: category (single choice) and intent (Build / Reduce). ([guide](components/FilterPanel.md), [preview](components/FilterPanel.html))
- `HabitCard` — A catalog habit: line icon, name, category and intent tags, the suggested starting goal, and actions. ([guide](components/HabitCard.md), [preview](components/HabitCard.html))

**Community**
- `BandList` — The full community view: public display names inside each band. ([guide](components/BandList.md), [preview](components/BandList.html))
- `BandSummary` — Four Consistency Bands with participant counts, used as the community preview on the visitor Home. ([guide](components/BandSummary.md), [preview](components/BandSummary.html))

**Forms**
- `ChoiceCard` — Radio options shown as cards with a title and one line of description: measure type, schedule type. ([guide](components/ChoiceCard.md), [preview](components/ChoiceCard.html))
- `DayPicker` — Seven toggle buttons for choosing fixed weekdays of a schedule. ([guide](components/DayPicker.md), [preview](components/DayPicker.html))
- `SearchField` — A search input with a leading magnifier icon, used on catalog pages and in admin tables. ([guide](components/SearchField.md), [preview](components/SearchField.html))
- `SegmentedControl` — Two or three mutually exclusive options shown side by side: Build | Reduce, Yes | No. ([guide](components/SegmentedControl.md), [preview](components/SegmentedControl.html))
- `Stepper` — A number input with − and + buttons and a unit after it, for targets and recorded amounts. ([guide](components/Stepper.md), [preview](components/Stepper.html))
- `TextArea` — A multi-line input for notes and descriptions: `textarea.ht-input.ht-textarea` inside a TextField. ([guide](components/TextArea.md), [preview](components/TextArea.html))
- `TextField` — A label, an input, and help or error text, stacked. ([guide](components/TextField.md), [preview](components/TextField.html))
- `Toggle` — An on/off switch for settings that apply at once, such as community participation. ([guide](components/Toggle.md), [preview](components/Toggle.html))

**Overlays**
- `DeactivateDialog` — Admin confirmation before deactivating an account for misuse. ([guide](components/DeactivateDialog.md), [preview](components/DeactivateDialog.html))
- `Dialog` — A modal window for short tasks that belong to the current page: Record, Add note, Excuse, Pause, End goal. ([guide](components/Dialog.md), [preview](components/Dialog.html))
- `EndGoalDialog` — Ends the current attempt of a goal and records its outcome. ([guide](components/EndGoalDialog.md), [preview](components/EndGoalDialog.html))
- `ExcuseDialog` — Excuses one Due or Pending commitment with no record, with an optional reason. New in slice 7, opened from the Home's "Excuse" and the RecordDialog's "Excuse instead"; a guide of its own rather than a `Dialog` variant. ([guide](components/ExcuseDialog.md), [preview](components/ExcuseDialog.html))
- `FeedbackDialog` — Lets a signed-in user send feedback to the admins; opened from "Send feedback" in the Footer and the mobile menu. ([guide](components/FeedbackDialog.md), [preview](components/FeedbackDialog.html))
- `NoteDialog` — A dialog to add or edit a private note on one commitment or period, for example to explain a Missed or partial result. Built in slice 5, opened from the goal page's History rows. ([guide](components/NoteDialog.md), [preview](components/NoteDialog.html))
- `PauseDialog` — Pauses a goal (commitment-based or period-based) for a date range. Built in slice 7, opened from the goal page's "Pause". ([guide](components/PauseDialog.md), [preview](components/PauseDialog.html))
- `RecordDialog` — The form to record what happened for one commitment or period; the field depends on the goal's measure. Built in slice 5, without "Excuse instead"; slice 7 adds it. ([guide](components/RecordDialog.md), [preview](components/RecordDialog.html))
- `RecordForms` — The yes/no and count variants of RecordDialog, shown without the backdrop. ([guide](components/RecordForms.md), [preview](components/RecordForms.html))
- `SuggestDialog` — Sends a custom habit to the admins as a catalog suggestion. ([guide](components/SuggestDialog.md), [preview](components/SuggestDialog.html))

**Data**
- `DataTable` — A table for admin lists (catalog entries, suggestions, feedback, accounts) and for goal history. ([guide](components/DataTable.md), [preview](components/DataTable.html))

## Icons

`icons/` holds the Lucide SVG files used by the design (Lucide 0.460.0, ISC license in `icons/LICENSE`). See `components/Icons.md` for the meaning of each icon: the 24 "Habit icons" a habit can have, and the app icons.

## Not designed yet

A dark theme (not needed for version 1). The "Not found" and Error pages use EmptyState inside the normal layout (see `components/EmptyState.md`). Edit goal settings reuses the New goal form (see `pages.md`).
