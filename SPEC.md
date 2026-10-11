# Habit Tracker — Product Specification

**Status:** Foundation baseline for the Agentic Engineering course exam  
**Scope:** Version 1 product behavior; no implementation design

## 1. Purpose

Habit Tracker is a **personal consistency tracker**. It helps individuals build or reduce recurring behaviors, define commitments, record what happened, and understand their consistency over time.

The primary purpose of this project is to practice **agentic software development and the software development lifecycle with Claude Code**. Commercial value and sophisticated product optimization are secondary. The product must nevertheless be coherent and demonstrable.

Community comparison is an optional, secondary motivation—not the measure of product success.

## 2. Vocabulary

| Term | Meaning |
| --- | --- |
| **Catalog habit** | Public, reusable description of a behavior, such as Reading or Yoga. |
| **Custom habit** | A behavior created privately by an individual user. |
| **Personal goal** | An individual's measurable commitment based on a catalog or custom habit. One habit may support several independent active goals. |
| **Suggested starting goal** | Optional, fully customizable example shown with a catalog habit; not a prescribed target. |
| **Build / Reduce** | Intent to increase/practice a behavior, or reduce/limit one. |
| **Commitment / scheduled occurrence** | An individually expected activity in a commitment-based goal. |
| **Evaluation period** | The day, week, or other defined interval over which a period-based goal is assessed. |
| **Evaluation** | Determination of whether a commitment or period met its applicable target. |
| **Excused** | An individual scheduled commitment deliberately exempted from evaluation. |
| **Pause** | Time-bounded suspension of a personal goal's expectations. |
| **Attempt** | A continuous pursuit of a personal goal, from beginning until it is ended. Restarting an ended goal starts a new attempt. |
| **Consistency** | Percentage of eligible, finalized evaluations successfully met. |
| **Consistency bands** | Community groupings by absolute consistency ranges, not statistical percentiles or numerical ranks. |

**Privacy distinction:** A public catalog habit is a publicly available *idea*, not a declaration that any individual's participation, goals, or history are public.

## 3. Audience and access

- **Anonymous visitor:** browse the public habit catalog and publicly available community comparison views.
- **Registered user:** all anonymous capabilities, plus manage personal goals, record activity, see personal reports, create private custom habits, suggest catalog additions, and opt into community participation.
- **Administrator:** maintain the public catalog, review suggestions and feedback, and deactivate/reactivate accounts; **no access to private goals, activity logs, journals, or individual personal reports**.

## 4. Habit catalog

- The public catalog offers broadly reusable habit descriptions, optionally categorized and accompanied by suggested starting goals.
- Starting suggestions are optional and entirely customizable. They are illustrations, not individualized coaching or medical advice.
- Users may define private custom habits for their own use.
- A user can **Suggest for catalog** a custom habit. An administrator may approve, edit before publishing, or reject the suggestion.
- Publishing a suggestion creates an **independent catalog habit**. It does not publish or modify the submitter's private custom habit, personal goals, identity, activity, or notes, and it does not automatically reconnect their existing goal to the new catalog entry.
- Administrators can create, edit, publish, and unpublish catalog entries.

## 5. Personal goals and tracking

### 5.1 Goal definition

- A personal goal defines a behavior to **Build** or **Reduce**, a measurable target, and expectations for its evaluation.
- Support **yes/no**, **count**, and **duration** measurements.
- Support both **fixed schedules** (e.g., yoga Monday/Wednesday/Friday) and **flexible periodic targets** (e.g., yoga three times per week).
- Each goal uses one scheduling approach at a time; users may create multiple independent goals for the same habit.
- At the business level, goals may use **commitment-based evaluation** (individual scheduled activities) or **period-based evaluation** (results across a day/week/etc.). These distinctions must remain clear in user-facing progress and reports.

### 5.2 Recording and evaluation

- Record actual activity, including amounts below or above target, not just a pass/fail checkbox.
- **Partial progress is preserved**, but meeting the applicable target is required for a successful evaluation.
- For a Build target, success generally means meeting or exceeding the required activity. For a Reduce target, success means remaining at or below the permitted amount.
- A missing record is initially **Pending**; the application has a **system-wide 48-hour grace period** after the relevant commitment/period ends for retroactive recording.
- After the grace period, an unrecorded commitment/period becomes **Missed**. A recorded but insufficient result is a failure rather than an unrecorded miss.
- Users can **correct finalized historical records**. Reports and streaks are recalculated using the **original targets and evaluation rules** for the corrected dates; the 48-hour deadline is not a permanent edit lock.
- Notes/journaling may accompany recorded activity, including context for missed or partial results.

### 5.3 Excusals and pauses

- Users may mark **individual scheduled commitments** as **Excused**, without approval. A reason is optional.
- Excusals are for commitment-based goals, not arbitrary exemptions from individual period-based evaluations.
- Users may **pause either commitment-based or period-based goals** for a stated start/end interval and may resume early.
- Excused commitments and paused expectations are excluded from consistency calculations; they neither advance nor break a streak.
- A pause does not erase activity or failures already recorded and cannot retroactively convert an already finalized failure into an excusal.

### 5.4 Changes and ending goals

- Goal settings (including targets, schedules, and measurement configuration) change **prospectively**. Completed historical evaluations retain the rules in effect at the time; new rules apply at the next evaluation boundary.
- Users can **end** a goal, retaining its history and recording an outcome such as **Achieved**, **Abandoned**, or **No longer relevant**, optionally with a note.
- Ending stops future commitments without creating a failure; ended attempts leave the active-goal view but remain accessible in history.
- Restarting an ended goal creates a **new attempt**, optionally copying prior settings, with an independent history and streak beginning at zero. A temporary interruption instead uses Pause.

## 6. Personal reports and dashboard

- Show upcoming, due, and overdue commitments, plus finalized missed results, on the dashboard. These are **in-app indicators**, not outbound reminders.
- Provide progress and trend views, including success/failure, partial amounts, consistency, current streak, and longest streak.
- **Strict streak:** successful finalized evaluations extend it; a finalized failure resets it; excused/paused expectations preserve but do not extend it; pending evaluations do not prematurely reset it. Streaks use the goal's relevant unit (e.g., occurrences or weeks) and must display that unit.
- **Consistency** = successful eligible finalized evaluations / (successful + failed eligible finalized evaluations) × 100%.
- Pending, excused, and paused evaluations are excluded from numerator and denominator.
- Display the evaluated count with each percentage, e.g., **90% (18 of 20)**. Display **N/A** when there are no eligible finalized evaluations.
- Independent goals are **not combined** into one habit-level personal consistency score in version 1.

## 7. Community comparison (secondary feature)

### 7.1 Participation and privacy

- Community participation is **off by default** and requires explicit, revocable opt-in.
- One consent covers both public user Consistency Bands and aggregate Catalog Habit Performance. Withdrawing consent removes the user from public bands and removes their contributions from community statistics, without deleting private history.
- Participants appear in user bands under a **public display name**; account identity need not be publicly revealed.
- Individual personal goals, activity records, and journal notes remain private. Custom habits are never displayed in public catalog performance statistics.
- Community views are accessible to anonymous visitors.

### 7.2 Consistency Bands (not ranked leaderboards)

- Use a **rolling 30-day window**.
- Participants qualify with **at least 10 eligible finalized evaluations** in that window across applicable goals.
- Group by *absolute consistency percentage*, not by relative percentile:

  | Band | Consistency |
  | --- | --- |
  | 90%+ | 90–100% |
  | 70%+ | 70% to below 90% |
  | 50%+ | 50% to below 70% |
  | Below 50% | Below 50% |

- Within each band, display public names **alphabetically**, not in score order. No numerical podium or cross-frequency effort ranking.
- Band membership reflects adherence to each person's chosen commitments, **not** the difficulty, volume, or value of those commitments.

### 7.3 Catalog Habit Performance

- Provide a public comparison view for catalog habits using aggregate consistency from eligible, consenting contributors within the same rolling 30-day window. Prefer bands/groupings to a numerical leaderboard.
- A catalog habit requires **at least five distinct eligible consenting contributors with relevant finalized evaluations** before its aggregate performance can be displayed.
- Otherwise display **Insufficient community data**; do not expose small-group performance values.
- Show contributor counts alongside published aggregate statistics.

## 8. Administration

- Maintain public catalog content and review custom-habit publication suggestions.
- Review user feedback and mark items resolved.
- Deactivate/reactivate accounts for misuse. Deactivated accounts cannot use the app; their data is retained and their public community participation is suspended.
- Administrative capabilities do not permit browsing individual users' private tracking data.

## 9. Version 1 exclusions

- Native mobile or desktop applications (the product is browser-based only).
- Social follows, messaging, comments, or public sharing of personal progress.
- AI coaching, individualized automated recommendations, or automatic goal generation.
- Wearable/fitness-platform integrations.
- Payments, subscriptions, or monetization.
- Email, push, SMS, or other externally delivered reminders.
- Points, badges, rewards, and other elaborate gamification beyond Consistency Bands.
- Admin inspection of private goal/activity/journal information.
- Production-scale availability, redundancy, or disaster-recovery requirements.

## 10. Demonstration expectation

The Azure-hosted version 1 application should be accessible to reviewers in a standard browser and include **representative demonstration data** sufficient to explore progress reports, the catalog, and community views without waiting weeks for real activity.

## 11. Open product questions — intentionally not decided here

These are **not implementation instructions**; they must be resolved before the affected features are built:

1. Exact commitment deadlines/timezone rules and how a 48-hour grace period behaves around local clock changes.
2. How partially overlapping pauses affect weekly or other multi-day evaluation periods.
3. Whether an Excused commitment may be recorded retroactively after finalization, given the separate rule that a pause cannot erase finalized failures.
4. How to interpret an entirely unrecorded **Reduce** period: a missing count cannot prove the user stayed under a limit; this requires explicit recording semantics.
5. Aggregation method for Catalog Habit Performance when participants have different numbers of evaluations or several goals based on the same catalog habit.
6. Whether community contributions are limited to goals based on public catalog habits (as implied by the initial proposal's public-habit condition); consent alone must not accidentally expose custom-goal activity.
7. How to treat an ongoing pending evaluation when reporting a *current* streak, and the exact meaning of a streak when goal evaluation settings change.

The initial product baseline should guide later feature-by-feature planning, rather than force premature decisions here.
