# GoalSummary

A compact list of active goals with consistency and streak, used on the signed-in Home.

- `ht-goal` rows: `ht-goal__name`, `ht-goal__pct` (`stat-small`, count in `small`), `ht-goal__meta` (streak with unit · longest).
- Show N/A when there is nothing finalized yet, with the reason in the meta line.
- Link each row to the goal page. Ended goals are not listed here (they are in Profile → Goal history).

## Slice 6 state

The last part of the preview shows the card as slice 6 builds it in the Home's right column (HP-003, constitution I, ADR 0008). Read on Wed 18 Nov 2026.

- `section.ht-card` with the title "Active goals" (`h2.ht-card__title`), then one `ht-goal` row per goal, at most 5, in the goal order of the Goals page (goals with a Pending result, then with a Due one, then the others, each group by name), then the link "All goals →" (`a.ht-card__link`, → `/goals`).
- Each row:
  - `ht-goal__top`: the goal's name as a link to its goal page (`a.ht-goal__name`), then its consistency as one whole text in `ht-goal__pct`: "92% (25 of 27)", or "N/A". No `<small>` part (tasks, Conventions);
  - `ht-goal__meta`: "Streak {n} {unit} · longest {m}" ("Streak 6 commitments · longest 11", "Streak 0 weeks · longest 3"; {unit} as in the streak, singular for exactly 1: "Streak 1 day · longest 4"), or, with no finalized result, "No finalized {unit} yet" with the plural unit ("No finalized weeks yet") beside "N/A".
- Each goal is shown on its own; goals on the same habit are never combined.
- The texts come from `ReportText` (HP-005); the card computes nothing.
