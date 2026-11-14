# Stepper

A number input with − and + buttons and a unit after it, for targets and recorded amounts.

- Native `input[type=number]` between two buttons with `aria-label` "Less" / "More". Unit text in `ht-unit`.
- Step 1 for counts, 5 for minutes.
- Minimum 0 when recording. For a target, the minimum follows Build or Reduce: 1 for a Build target ("at least"), 0 for a Reduce target ("at most", so "at most 0 sweets a day" can be set). Maximum 999 for a target. The New goal form and the administrator's suggested starting goal editor both follow this rule.
