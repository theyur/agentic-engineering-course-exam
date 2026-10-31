# RecordDialog

The form to record what happened for one commitment or period; the field depends on the goal's measure.

## Variants (see also RecordForms)
- **Yes / no**: SegmentedControl "Did you do it?" Yes | No.
- **Count**: Stepper with the unit ("cups", "times").
- **Duration**: Stepper in minutes, with a progress bar and "12 of 20 min".

## Rules
- "For" select: today plus every commitment still Pending (inside the 48 h grace period). Finalized dates are corrected from the goal page History table.
- Show the target in the sub line ("Build · at least 20 min · every day") and progress under the field.
- Say what partial progress means: it is kept, and below target the result is Failed when finalized.
- Reduce goals: ask for an explicit number, 0 included. An empty day is never treated as success.
- "Excuse instead" (ghost, right) only for commitment-based goals and only while the commitment is upcoming or Pending.
- Note is optional and private.
