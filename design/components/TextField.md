# TextField

A label, an input, and help or error text, stacked.

- Label above the input (`ht-field__label`), never only a placeholder.
- Help text says why or where the value is used. Error text says what to do: "Enter a number of 1 or more."
- Invalid: `aria-invalid="true"` on the input (border turns `danger`) and an `ht-field__error` line.
- Blazor: style `InputText` with `class="ht-input"` and map `.invalid` to the same look.
