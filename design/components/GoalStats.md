# GoalStats

The goal summary card: consistency, current streak and longest streak, with the history strip below.

## Rules from the product spec
- Consistency is always shown with its count: **90% (18 of 20)**. With no eligible finalized evaluations show **N/A**.
- Streaks always show their unit: sessions, days or weeks ("6 sessions", "3 weeks").
- Pending, excused and paused evaluations are not counted; say so in help text where people ask.
- Each goal has its own stats; never combine goals into one habit score.

Markup: `ht-stats` with three `ht-stat` boxes (`ht-stat__value`, `ht-stat__label`), then `ConsistencyStrip`.
