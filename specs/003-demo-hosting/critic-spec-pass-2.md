# Critic pass 2: spec for 003-demo-hosting

**Verdict**: APPROVE
**Date**: 2026-11-15
**Reviewed**: `specs/003-demo-hosting/spec.md`, `specs/003-demo-hosting/checklists/requirements.md` (working
tree, revised after pass 1), with the two new `CONTEXT.md` entries ("Demo refresh", "Hosted demo") that the
spec's Assumptions name as part of the same change.

Checked against: `.specify/memory/constitution.md`, `SPEC.md`, `ARCHITECTURE.md`, `CLAUDE.md`, `CONTEXT.md`
(with `git diff CONTEXT.md`), `docs/roadmap.md`, ADR 0003 / 0006 / 0007 / 0013,
`specs/001-accounts-and-access/spec.md`, `specs/002-habit-catalog/spec.md`, `README.md`,
`.github/workflows/sonar.yml`, and `src/HT.Ui` (`Program.cs`, `Operator/StartupTasks.cs`, `appsettings*.json`
key names only, `Properties/launchSettings.json`, `Components/Pages/Error.razor`, `Components/Pages/SignIn.razor`,
`Components/Layout/MainLayout.razor`, render modes of the pages). No secret value was read; nothing was built
or run. Line numbers of `spec.md` refer to the working-tree file.

## Previous notes: resolution status

Only pass 1 exists, so there is no older note that could have regressed.

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 deployment/refresh result undefined | Resolved | HP-017 (l.293-296) defines success by the startup-task results and a faulted start as failed; HP-004 (l.255-257) orders the schema before administrator creation and demo loading; HP-009 (l.271-272) covers refreshes; US3-AS4/AS5 (l.178-182); the state after a failure is stated (l.63-64, HP-017, edge l.227-229). Matches ADR 0007 "Results"/"Schema" and `StartupTasks.RunStepAsync`. Wording left inconsistent by the new text is in MINOR-3 and MINOR-4 (new). |
| MAJOR-2 boundary of "secret" | Partly | The contradiction is gone: secret values are defined once (l.57-60, HP-007 l.264-267), Key Entities (l.306-311) separates the administrator email from the secret values, HP-010 (l.273-276) and SC-004 (l.327-328) are decidable, and the demo emails and address are public. Whether the repository documentation states the administrator email is still unclear: MINOR-6. |
| MAJOR-3 "the owner" for Operator | Partly | Stories, HP, SC, Key Entities and Assumptions use "Operator"; l.54-56 bridges the verbatim owner answers; "Hosted demo" and "Demo refresh" are in `CONTEXT.md`. "Deployment", the third term pass 1 named, is still undefined and now used in two senses: MINOR-5. |
| MINOR-1 time-relative demo data | Partly | Spec: edge case l.236-239 and decision bullets l.344-353 now assume hosting after slice 5. The roadmap rows the spec's "Decisions due" section is built on still describe the opposite order: MINOR-9. |
| MINOR-2 markers for ARCHITECTURE §9 choices | Resolved | Four `[NEEDS CLARIFICATION: ARCHITECTURE §9]` markers (l.344, l.348, l.350, l.354); the plan gate is named (l.338-340); checklist l.16 unticked with the reason (l.43-45). |
| MINOR-3 "SPEC §10 is then met" | Resolved | l.46-47 now claims only reachability; l.71-72 states that the demo data may lack progress reports and community views (roadmap R5). |
| MINOR-4 administrator outside "visibly synthetic" | Resolved | l.67-68 and HP-014 (l.286-287) put the configured administrator under the `example.com` rule (constitution VI). Author's decision, pending owner confirmation (MINOR-1). |
| MINOR-5 session across an idle restart | Resolved | l.65-66, US1-AS6 (l.118-119), HP-018 (l.297-298), edge l.230-231, decision bullet l.354-355. The cited basis is imprecise: MINOR-7 (new). |
| MINOR-6 criteria without a way to check | Resolved | SC-004/005/006 are run by the Operator through the smoke check (l.80-82, l.327-332, HP-015 l.288-290); US2-AS4 (l.145-148) tests the repository, the pages and the Operator documentation. The author's rationale is sound: user and project `CLAUDE.md` forbid an agent to read the secret values these checks need, and the exam submission is outside the repository (l.371-372). SC-007 still has no number, but that is now recorded as open (marker l.344-345, checklist l.18 unticked, l.46). |
| MINOR-7 HP-003 differences, wrong error surface | Resolved | HP-003 (l.252-254) lists the error page, strict-transport headers and no migrations at start, which is what `Program.cs` l.66-71 and `StartupTasks.cs` l.30-37 do outside Development; edge l.222-224 names the error page or the error notice (`MainLayout.razor` `blazor-error-ui`). |
| MINOR-8 `master` / "pending migration" | Resolved | "the deployed commit of `master`" (l.95-96, HP-003), "a chosen commit" (l.157, HP-016), US3-AS1 "whose schema differs from the deployed one" (l.170-172), SC-003 (l.325). |
| MINOR-9 impossible ordering of the use-case fallback | Resolved | l.86-89: the hosting ADR, taken before `/speckit-plan`, triggers the revision. |
| MINOR-10 Azure not stated | Resolved | l.12-13, HP-001 (l.248), HP-013 (l.283), Key Entities (l.302), Assumptions (l.366). |
| MINOR-11 start date in five places | Resolved | The date stands only at l.46; l.16 and l.356-357 refer to it; SC-008 (l.334) is an outcome only. The author's reason for keeping it once is sound: it is the owner's recorded answer (constitution, Decision Discipline; roadmap R2 row). |
| MINOR-12 checklist stale | Partly | (a) trigger no longer listed as open, (b) iterations 3 and 4 recorded (l.39-48), (c) `design/` removed from l.34-36, (e) HP-016 in numeric order (spec l.291-292). (d) the item is now unticked, but its note is inaccurate: MINOR-4. |
| MINOR-13 statements about earlier slices | Resolved | l.69-70 answers slice 2's handed-over seed question; l.373-374 names slices 1 and 2; HP-010 (l.273-274) and Key Entities (l.314) now agree on who can read the operator log. |

## Notes

### MINOR-1: Requirements rest on decisions the owner has not confirmed, and the confirmation is tracked nowhere but one sentence

- **Where**: spec.md "Critic revision 2026-11-15" (l.49-72, gate at l.51-52); HP-014 (l.286-287), HP-017
  (l.293-296), HP-018 (l.297-298); "Decisions due before `/speckit-plan`" (l.336-357); checklist Iteration 4
  note (l.41-48).
- **Against**: constitution, Decision Discipline ("The answer comes from the project owner ... and is recorded
  in the spec's `## Clarifications` section with its date"; "Later features MUST cite a recorded decision");
  `specs/002-habit-catalog/spec.md` Assumptions (l.871-872: seed content for the hosted demo "needs its own
  decision"); roadmap "Decisions each slice must settle" (l.132-135).
- **Continues**: new
- **Reasoning**: The subsection sits under `## Clarifications`, where the constitution puts owner answers, and
  holds decisions the author took: the definition of a succeeded deployment, session survival, the synthetic
  administrator email, and "no extra seed content" (the question slice 2 handed to this slice as needing its
  own decision). Three of them are already written as MUST requirements (HP-014, HP-017, HP-018). That they
  are unconfirmed is said once, at l.51-52. The section that lists what must happen before `/speckit-plan`
  (l.336-357) has five bullets and none is this confirmation; the checklist note (l.41) says only "decisions
  listed under 'Critic revision 2026-11-15'". Nothing in the artifacts would show, after the slice resumes,
  whether the confirmation took place, and a later slice citing these lines as recorded decisions cannot tell
  them from owner answers.

### MINOR-2: The success definition and the smoke check are fixed to "three demo accounts", although the slice is built after slices that need more demo users

- **Where**: spec.md l.61-63 ("all three demo accounts and the seed catalog"), HP-017 (l.293-295), US2-AS1
  (l.137-139, `demo1@example.com` to `demo3@example.com`), SC-005 (l.329-330, "all three demo accounts"),
  against HP-005 (l.258-260), US2-AS2 (l.140-142) and US4-AS4 (l.210-211), which say "every shipped slice's
  demo data".
- **Against**: spec.md l.15-16 and roadmap R2 (the slice is built last, "whatever has shipped by then"); ADR
  0007 "Later slices" (l.46: "Each later slice extends demo loading with its own demo data"); roadmap slice
  note 9 (l.95-96: "enough consenting demo users to fill bands, and catalog habits both above and below the
  5-contributor minimum"); SPEC §7.3 ("at least five distinct eligible consenting contributors").
- **Continues**: new (text added for pass-1 MAJOR-1)
- **Reasoning**: The definition of a succeeded deployment, and the check that confirms it, enumerate the demo
  content of slices 1 and 2. A catalog habit above the five-contributor minimum cannot be shown with three demo
  users, so slice 9's demo data needs more accounts than HP-017 and SC-005 name. If such a slice has shipped
  when this slice resumes, a deployment whose demo loading created the three accounts and the seed catalog but
  faulted on later demo data is "succeeded" under HP-017 and failing under HP-005 / US2-AS2. The spec says in
  other places that it covers every shipped slice; its success definition does not.

### MINOR-3: The new failure-state text is inconsistent in four places

- **Where**: spec.md HP-017 (l.293-296) against HP-011 (l.277-278); Edge Cases l.232-235 against HP-017
  (l.295-296) and US3-AS5 (l.180-182); l.63-64 against SC-006 (l.331-332); Edge Cases l.222-224 against HP-018
  (l.297-298) and l.230-231.
- **Against**: internal consistency; ADR 0007 "Results" (l.40-41: the host starts normally whatever the
  result).
- **Continues**: new (text added for pass-1 MAJOR-1)
- **Reasoning**: (a) HP-017 makes a demo refresh succeed "only when the new version serves requests", while
  HP-011 defines a refresh as happening "without a new version". (b) The demo-email edge case says a
  deployment whose demo loading faulted "is not served as the demo"; HP-017 allows a failed deployment to
  leave the hosted demo "incomplete", and US3-AS5 describes exactly such a start that serves requests at the
  public address. In that state a reviewer can register a demo email, which the edge case declares impossible.
  (c) l.63-64 says the time the demo is "unavailable or incomplete" after a failure "counts against SC-006",
  but SC-006 measures only whether the smoke check "reaches" the hosted demo, which an incomplete demo passes.
  (d) The database-fault edge case says that when the database is unreachable at start "the deployment is
  reported as failed (HP-017)". The spec allows the app to sleep and restart without a deployment (HP-018,
  l.230-231); such a start has no deployment to report, and nothing says how its fault is classified or seen
  beyond the operator log.

### MINOR-4: A failed demo refresh has no scenario or edge case, and the checklist note on missing scenarios is inaccurate

- **Where**: spec.md US4 (l.200-211), Edge Cases (l.217-242), HP-009 (l.271-272), HP-010 (l.273-274), HP-017
  (l.293-296); checklist l.27, l.47-48, and l.18 against l.29.
- **Against**: spec.md US2-AS4 (l.145-148), US3-AS2/AS4 (l.173-179); pass-1 MAJOR-1 point 3 and MINOR-12 (d).
- **Continues**: MINOR-12
- **Reasoning**: HP-009 and HP-017 require that the Operator sees whether a demo refresh succeeded and why it
  failed. Every US4 scenario assumes a refresh that succeeds; US3-AS2 and AS4 are about deployments; the Edge
  Cases list a failed migration of a deployment but no failed refresh. The checklist explains its unticked
  item with "HP-012, HP-013, HP-014 and HP-015 have no acceptance scenario". HP-015 is named in US2-AS4, while
  the refresh half of HP-009 and HP-017 and the second clause of HP-010 ("no page or public address of the
  hosted app exposes it") have no scenario and are not listed. The checklist also ticks "Feature meets
  measurable outcomes defined in Success Criteria" (l.29) while "Success criteria are measurable" (l.18) is
  unticked for SC-007.

### MINOR-5: "Deployment" is defined as a rollout but used in the glossary and slice 1 as an installation; it has no `CONTEXT.md` entry

- **Where**: spec.md Key Entities "Deployment" (l.304-305) and "Demo refresh" (l.312-313); Assumptions
  (l.377-378); `CONTEXT.md` l.36 (new "Demo refresh"), l.39 (new "Hosted demo"), l.3 (`updated_at`).
- **Against**: constitution I ("A change that introduces a new domain term, or sharpens the meaning of an
  existing one, MUST add or update its `CONTEXT.md` entry"); `CONTEXT.md` l.42 ("Operator — the person who
  runs a deployment and holds its secret configuration") and l.45 ("a new deployment shows a full catalog");
  `specs/001-accounts-and-access/spec.md` l.266-267 ("The operator of a deployment ... the deployment's secret
  configuration"); ADR 0007 l.22-23 ("a deployment without a demo password gets no demo accounts").
- **Continues**: MAJOR-3
- **Reasoning**: Pass 1 named three terms the spec kept out of the glossary. Two were added. The third,
  "Deployment", is a Key Entity defined as "one rollout of a chosen commit", an event with a result. The
  glossary, slice 1 and ADR 0007 use the word for a running installation that has secret configuration and an
  operator log. The two new glossary entries use both senses side by side: "NOT a local or test deployment"
  (installation, l.39) and "it happens on every deployment" (rollout, l.36). In the same entry, and in the
  spec's Key Entity, "whenever the operator starts one" has no clear antecedent: read as "starts a
  deployment" it repeats the first half and loses the refresh without a new version that HP-011 requires.
  `CONTEXT.md`'s `updated_at` still reads 2026-11-12, although the two earlier specification changes to the
  file each set it (commits 054b16d, 3c479bb).

### MINOR-6: Whether the repository documentation states the administrator email is still open

- **Where**: spec.md US2-AS4 (l.145-148), HP-015 (l.288-290), Assumptions (l.369-372), against l.57-60 and Key
  Entities (l.306-311).
- **Against**: `specs/001-accounts-and-access/spec.md` HP-019 (l.582-583: "The administrator credentials ...
  MUST come from configuration outside source control"); project `CLAUDE.md` "Build and test" (the
  `Operator:*` values are never read or printed).
- **Continues**: MAJOR-2
- **Reasoning**: l.57-59 says the administrator email "stays out of source control". US2-AS4 requires that the
  Operator documentation, which HP-015 places in the repository, "lists ... the published address, the demo
  emails, the administrator email and where the two passwords come from". For the address and the demo emails
  the values may be in the repository; for the administrator email the same sentence can be read as the value
  or as the item. HP-015 forbids only "any secret value", and by l.57-58 the administrator email is not one,
  so no requirement or success criterion (SC-004 searches for the passwords and the connection only) settles
  it. Separately, Assumptions l.369-370 says "the demo password lives only in the exam submission", while Key
  Entities lists it in the hosted demo's secret configuration.

### MINOR-7: HP-018 cites a slice 1 requirement that does not cover a host restart, and "only a deployment or demo refresh ends it" is not accurate

- **Where**: spec.md l.65-66, US1-AS6 (l.118-119), HP-018 (l.297-298).
- **Against**: `specs/001-accounts-and-access/spec.md` HP-004 (l.541-543: with "Keep me signed in" the session
  survives *browser* restarts; "Without it, the session MUST end when the browser closes"), HP-006 (l.546-547)
  and HP-007 (l.548-550: a password change ends every other session of that account);
  `src/HT.Ui/Program.cs` l.29-48 (no key-storage configuration; the security stamp is validated on every
  request).
- **Continues**: new (text added for pass-1 MINOR-5)
- **Reasoning**: The spec grounds the requirement in "as it does locally (slice 1 HP-004)". HP-004 is about
  the browser being restarted and about the 14-day renewal; no slice 1 requirement says a session survives a
  restart of the host, so the citation does not carry the claim. "Only a deployment or demo refresh ends it"
  leaves out the ways slice 1 ends a session: sign-out, closing the browser without "Keep me signed in", and
  HP-007. The last one matters on the hosted demo, because reviewers share the demo accounts: one reviewer
  changing a demo account's password ends the sessions of the others. US1-AS6's "they are still signed in"
  carries no such condition.

### MINOR-8: The demo-account "lock" is described as lasting until the next refresh, and the glossary term is not used

- **Where**: spec.md US4 (l.190-191, "lock it with failed sign-ins"), Edge Cases (l.217-219, "locks it out
  with failed sign-ins: other reviewers cannot use that account until the next refresh").
- **Against**: `specs/001-accounts-and-access/spec.md` FR-008 (l.479-482: blocked "until exactly 5 minutes
  later") and Edge Cases (l.442: "its 5-minute block"); `CONTEXT.md` l.46 ("Sign-in block — a temporary state
  ... sign-in fails for 5 minutes"); ADR 0007 Consequences (l.59: "trigger its block"); constitution I
  ("Synonyms for defined terms MUST NOT appear").
- **Continues**: new (the text predates pass 1, which did not raise it)
- **Reasoning**: The edge case joins a changed password and a sign-in block in one consequence. A changed
  password does last until the next refresh. A sign-in block ends by itself after 5 minutes, so "until the
  next refresh" is wrong for it, and it is the only consequence the sentence gives. "Lock" and "locks it out"
  stand for the glossary term "Sign-in block", which the glossary separates from lasting states.

### MINOR-9: The roadmap rows this slice owns still describe hosting before slice 5

- **Where**: spec.md "Decisions due" (l.338, "The roadmap places these technical decisions in this slice";
  l.345-347; l.350-353).
- **Against**: `docs/roadmap.md` l.175 ("Scheduled evaluation (slice 5) may need more; changing this then
  needs a superseding ADR"), l.177 ("Time-relative generation (slice 5) may reopen it with a new or
  superseding ADR"; "on the owner's demand"); roadmap "Updating this file" (l.243-244: the feature branch
  updates the decision rows it settles); roadmap R2 answer (l.207).
- **Continues**: MINOR-1
- **Reasoning**: Pass 1 found two spec bullets that described opposite orders of hosting and slice 5. The spec
  bullets now say the hosting ADR "also covers what slices shipped by then need". The roadmap rows for the
  same two decisions, in the file that records R2's answer, still say slice 5 comes later and may reopen
  them. The spec presents its decision list as the roadmap's, so the two documents disagree on the scope of
  the hosting ADR and of the refresh decision. The refresh row also still names "the owner" for the actor the
  spec now calls the Operator.

## Summary

0 MAJOR, 9 MINOR. All three MAJOR notes of pass 1 are answered at requirement level: a succeeded deployment or
refresh is defined against the non-fatal startup tasks, the secret boundary is stated once and matches the
logging code, and the actor is the glossary's Operator. The author's two partial positions (Operator-run
smoke check for SC-004/005/006; the start date kept once as the owner's answer) are sound. What remains is
cleanup: the gap-filling decisions are written as requirements while their owner confirmation is tracked only
in one sentence (MINOR-1), the success definition is tied to slice 1-2 demo content although the slice is
built last (MINOR-2), and the new failure-state and glossary text carries several small inconsistencies
(MINOR-3 to MINOR-9). None of these blocks the next step, which for this parked slice is the owner's
confirmation and the four marked ARCHITECTURE §9 decisions, not `/speckit-plan`.
