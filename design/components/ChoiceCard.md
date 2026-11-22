# ChoiceCard

Radio options shown as cards with a title and one line of description: measure type, schedule type.

- `label.ht-choice` with a native `input[type=radio]`, `ht-choice__title`, `ht-choice__desc`. The checked card gets the `accent` border and `accent-soft` fill (CSS `:has`).
- Use 2–4 cards in `ht-choices` (auto-fit grid). Keep the description to a few words.
- One column (slice 8, `components/EndGoalDialog.md`): `ht-choices` with `style="grid-template-columns:1fr"`, as the EndGoalDialog preview has it. The Razor component's `OneColumn` parameter renders that style, and its `FocusFirstAsync` moves the focus to the first radio for a dialog whose first field the group is (settled in T076).
