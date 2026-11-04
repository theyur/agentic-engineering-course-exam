# 0002 — Resolving Copilot review threads automatically after a verified re-review

**Status:** Accepted, 2026-11-05. Extends the Copilot stage of ADR 0001; replaces the
`/process-pr-feedback` rule "Do not resolve threads - the user does".

## Context

On PR #13 Copilot raised two high-severity threads on `50b1fc4`. Claude fixed both in `cd5c1dd`
and replied in each thread. Copilot re-reviewed `cd5c1dd` and its overview changed from
**Changes recommended** to **Approval recommended**. Copilot left the threads open, and the user
resolved them by hand after merging.

What the data shows:

- Copilot's reviews are `COMMENTED`. The verdict is free text in the overview body
  (`<!-- ccr-overview-v2 -->`, `### 🟢 Approval recommended`). The thread state `isResolved` is a
  separate field.
- The approving re-review still listed **both** findings under "2 open issues". It had no
  "Resolved since last review" section, and Copilot wrote nothing in either thread. So Copilot did
  not confirm either fix, and the approval verdict said nothing about them.
- Since 2026-09 Copilot resolves its own threads during a re-review when it judges a later commit
  fixed them, and lists them under "Resolved since last review". Sources: GitHub changelog
  2026-09-11 ("auto-resolution") and 2026-09-18 ("improved review experience"). Copilot keeps a
  thread open when a reply asks for that. On PR #13 the native mechanism did not fire. The likely
  reason is that Copilot did not consider the findings fixed; the evidence can't show why.
- On PR #14 the native mechanism did fire. Copilot's re-review resolved all four of its threads
  itself (`resolvedBy: Copilot`) and listed them under "4 resolved since last review". It used a
  third verdict, **Needs a closer look**, for a finding reported only in the overview (no inline
  thread).
- Approvals are a separate, opt-in repository setting (GA 2026-10-28). They are not enabled here,
  and an approval would not be per-thread evidence either.

## Options

| | Approach | Complexity | False-resolution risk | Permissions |
|---|---|---|---|---|
| A | Rely only on Copilot's native auto-resolution | none | lowest | none |
| B | Claude, inside `/process-pr-feedback`, runs `tools/pr-gates/pr-resolve.ps1` after a re-review | one script plus self-test | low to moderate: needs a verified fix commit plus a re-review of the head that either confirms the finding or approves without re-raising it | the user's `gh` token (`repo` scope) |
| C | GitHub Actions on `pull_request_review` from Copilot | workflow plus the same rules | same as B | `pull-requests: write`, and per community reports `contents: write`, on an event triggered by a bot |

A is the main path, but on its own it is not observable from the workflow, and it did not fire on
PR #13. C adds a privileged workflow and duplicates B's logic for no gain while a Claude session
drives the loop anyway (ADR 0001 consequences). If that loop is ever automated, C can run the same
script.

## Decision

**A + B, with B in approval mode by default.** Copilot resolves what it confirms.
`pr-resolve.ps1` reports every thread's state and resolves a thread only when both sides agree:

1. **Claude's claim, checked against git.** The thread's *last* comment is a reply that starts
   with `Fixed in <sha>`, by a user whose actual repository permission includes push
   (`collaborators/<user>/permission`). `authorAssociation` is not used: MEMBER and COLLABORATOR
   can be read-only. `<sha>` is in the PR head (`compare` ahead or identical) and comes after the
   commented commit.
2. **Copilot's side.** Only the exact reviewer bot counts (`copilot-pull-request-reviewer`, a
   `Bot`); look-alike logins are not Copilot. Its latest *active* review is on the current head
   SHA, so it saw the fix; dismissed or unsubmitted reviews are ignored. No Copilot review is
   requested or running since then (`review_requested`, `copilot_work_started`).
   That review raised no new Copilot thread on the same file, resolved since or not, which would
   point to a partial fix or a duplicate. In addition, one of these holds:
   - **`copilot-confirmed`** (always on): the overview lists the finding (`#discussion_r<id>`) in a
     "resolved" section.
   - **`approval`** (`-AcceptApproval`, the `/process-pr-feedback` default): the verdict is
     "Approval recommended". The overview does not put the finding under an unrecognized heading.
     The fix commit touches the commented file, or GitHub marks the thread outdated.

The `approval` basis is the PR #13 case. It trusts Copilot's approval plus its silence on the
finding, not a per-finding confirmation: Copilot still listed the finding as open. This trade-off
was chosen explicitly, because otherwise the PR #13 pattern always leaves threads for a human. It
stays bounded. A "Changes recommended" or unrecognized verdict resolves only `copilot-confirmed`
findings, and every other negative below still applies. Without the flag the script runs strict:
only `copilot-confirmed`.

Everything else stays open with a stated reason:

- a rejected, partial or disputed reply
- a later comment by Copilot or a person
- a fix commit that is missing or was rewritten
- a re-review that is pending or on an older commit
- in strict mode, an overview that still lists the finding as open or does not list it
- an overview or verdict in an unknown format
- a fix that touches neither the commented file nor makes the thread outdated (`approval` basis)
- incomplete data (more than 100 threads or 50 comments)

How it runs:

- The script is a dry run by default. `-Apply` uses `resolveReviewThread` with `resolutionReason:
  ADDRESSED`. Right before each mutation it re-checks the head SHA and Copilot's latest review, and
  re-reads the thread. A thread with a new comment, an edited last comment (`updatedAt` or text)
  or a changed state is left alone. A finding the overview lists under contradictory sections takes
  the least-resolved status (open > unknown > resolved). A small window
  between that re-read and the mutation remains; GitHub offers no conditional resolve.
- Already-resolved threads are never touched, so re-runs are no-ops.
- It reads GitHub and writes thread state only. It does not touch checks, so it cannot affect
  SonarQube Cloud or CodeQL.
- The decision logic is pure and covered by `selftest.ps1`, including the PR #13 case, which must
  stay open.
- The GitHub MCP server's `pull_request_review_write` (`resolve_thread`) could do the mutation, but
  it is not configured here, and `gh` is already the transport for `tools/pr-gates`.

## Consequences

- Claude's replies follow a fixed format: `Fixed in <sha>: ...`, `Partially addressed in ...`,
  `Not changed: ...`. Only the first counts as a fix claim.
- Threads are resolved automatically in the PR #13 pattern too: fix pushed, `Fixed in` reply,
  Copilot re-review on the head with "Approval recommended", and nothing re-raised. A wrong
  resolution needs Copilot to approve while missing that the fix is incomplete. The thread can be
  reopened with "Unresolve conversation".
- Each resolution's `basis` is in the script output, so approval-based resolutions stay
  distinguishable from confirmed ones.
- Resolutions by the script show the token owner as `resolvedBy`, with reason ADDRESSED. The
  script's JSON output is the audit trail; no extra PR comment is posted.
- The overview format is undocumented and may change. Unrecognized headings or markers fail
  closed (nothing is resolved), and a format change shows up as threads kept with "does not list
  this finding as resolved".
