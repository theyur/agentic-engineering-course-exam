# Brand book

Habit Tracker is a calm, private consistency tracker. The design is **Ink Blue & Haze**: white cards on a cool haze-grey page, one ink-blue accent, Sora for headings and numbers, Source Sans 3 for text. Layout A: a top navigation bar, centered content, sections stacked down the page.

## Content fundamentals

- Write plain, short English. Many readers are not native speakers: one idea per sentence, no idioms, no jokes.
- Address the user as "you"; the product does not say "we" or "I". Example: "Your goals and notes stay private."
- Sentence case everywhere: titles, buttons, tabs ("Use this habit", "My custom habits"). Uppercase only through the `eyebrow` style.
- Buttons start with a verb: Record, Add time, Excuse, Use this habit, Approve & publish.
- Use the product words exactly as the spec defines them: catalog habit, custom habit, goal, attempt, Build / Reduce, commitment, Pending, Met, Failed, Missed, Excused, Paused, consistency, streak, Consistency Bands. Never "done", "skipped", "overdue", "rank", "leaderboard", "top", "score".
- Consistency always comes with its count: **90% (18 of 20)**. No eligible data: **N/A**. Streaks always carry their unit: "6 sessions", "3 weeks".
- Suggested starting goals are examples, never advice: "Suggested start: 15 min, 5 days a week" and "An example to adjust, not a prescribed target."
- Privacy is stated, not implied: say what others can see and what they never see. Community is "opt-in"; names are "listed alphabetically, never ranked".
- No emoji, no exclamation marks, no gamified praise ("Great job!"). Neutral facts: "Streak 6 sessions".
- Dates: "Friday, 30 October", short "Wed 28 Oct". Time left: "22 h left to record".

## Visual foundations

### Color
- Page ground is `bg` (Haze). Cards, nav, hero band, inputs and tables are `surface` (white). Soft boxes inside cards (stat boxes, tags, progress track) are `surface-2`.
- Text is `ink`; secondary text and metadata are `ink-muted`. Both pass 4.5:1 on `bg`, `surface` and `surface-2`.
- `accent` (Ink Blue) is the only brand colour. Use it for primary buttons, links, the current nav underline, selected tab, focus ring and progress fill. Put `on-accent` text on it. Hover is `accent-hover`. Tinted areas use `accent-soft` with `accent` text.
- Status colours are for evaluation states only: `met`, `failed`, `missed` (same hue as failed, hatched), `pending` on `pending-soft`, `excused` (dashed), `paused`. `met` and `failed` have the same lightness, so a status is never shown by colour alone: always a word (StatusChip) or a shape (ConsistencyStrip cell).
- `danger` (alias of `failed`) is for destructive actions and form errors.
- Borders: `line` for card edges and row dividers (decoration). Controls (inputs, checkboxes, toggle) use `line-strong`, which meets 3:1 on `surface`.
- One light theme ("Haze") for now. No gradients, no coloured card backgrounds, no coloured left-border cards.

### Type
- Headings and numbers: Sora (`--font-head`), weight 600. Text: Source Sans 3 (`--font-body`). Both load from Google Fonts (imported at the top of `css/ht-components.css`).
- Scale: `display` 40/44 (visitor hero only), `heading-1` 28/34 (one per page), `heading-2` 24/30 (section titles), `heading-3` 18/24 (card titles), `title` 17/22 (habit names), `body` 15/22, `body-large` 17/26 (hero text), `small` 13/18 (meta and help), `label` 12/16 bold (chips), `eyebrow` 12/16 bold uppercase +0.07em.
- Numbers: `stat-large` 28, `stat` 26, `stat-small` 18, with tabular figures (`ht-num`). The count after a percentage is `small` muted: "90% (18 of 20)".

### Spacing and layout (layout A)
- Content is centered at `container-max` (1040px) with a `space-6` (24px) gutter; 16px on phones. The nav and hero are full-width bands with the same centered inner width.
- Rhythm: `space-7` (40px) between page sections, `space-4` (16px) between cards, `space-5` (18px) card padding, `space-3` (12px) row padding.
- Page structure: TopNav → page head (`heading-1` + actions on the right) → content → Footer. Two-column pages use `ht-grid-2` (1.5 : 1); catalog uses `ht-sidebar-layout` (200px filters + card grid). Under 860px everything stacks to one column.

### Shape, depth and states
- Radii: `radius-md` 12px cards, notices and tables; `radius-sm` 8px buttons, inputs, stat boxes, glyph tiles; `radius-chip` 6px chips; `radius-full` avatar, step numbers, toggle, progress.
- Cards have a 1px `line` border AND `shadow-card`. Floating menus use `shadow-raised`. Nothing else casts a shadow.
- Controls are 38px high (`control-height`), small ones 32px.
- Focus: a solid 2px `accent` outline with 2px offset on every focusable element (≥3:1 on all surfaces). Hover: primary goes to `accent-hover`; secondary and menu items get `surface-2`; ghost gets `accent-soft`. Disabled: 50% opacity.
- Dialogs sit on the `overlay` backdrop with `shadow-raised`, 480px wide at most.
- Motion: only short state changes (toggle knob 150ms). No animated counters or confetti.

### Imagery
- No photos or illustrations. The visitor hero shows a real GoalStats card with demo data instead of a picture.

## Iconography

- Icons are **Lucide** 0.460.0 (ISC license): 24px line icons, `stroke-width` 1.75, round caps and joins, `currentColor`. The chosen icons and their meanings are in the Icons card; use only those, one meaning each.
- Sizes: `ht-icon--sm` 16px in inputs and small buttons, `ht-icon` 20px in menus and buttons, `ht-icon--lg` 24px in habit glyph tiles (`accent` on `accent-soft`) and empty states.
- Icons sit next to a word. Icon-only buttons (menu, close) need an `aria-label`.
- Every catalog habit has one icon (book-open for Reading, person-standing for Yoga…), picked from the Icons list.
- No emoji as icons.

## Logo

- There is no logo yet. Set the name "Habit Tracker" in Sora 600, 17px, `ink`, in the TopNav. Do not invent a mark.

## Using the components

- Components are plain CSS classes with the `ht-` prefix in `css/ht-components.css`, on top of the token variables in `css/tokens.css`. There is no JavaScript; wrap a page in `class="ht"`.
- In the Blazor app (HT.Ui), copy `css/tokens.css` and `css/ht-components.css` into `wwwroot/css/` and make small Razor components that render this markup (for example `HtButton`, `StatusChip`, `HabitCard`, `CommitmentRow`). Each component's README lists its classes and rules.
- The page previews in `pages/` show how the components fit together: Home for visitors and for signed-in users, Catalog for visitors and for signed-in users, Goal detail, New goal, Community, Profile, Admin. Record, note and excuse forms are dialogs (in `components/`). See `pages.md` for the structure of each.
