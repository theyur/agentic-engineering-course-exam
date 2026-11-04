# Self-test for tools/pr-gates and .claude/hooks/block-pr-review.ps1. Offline: no GitHub or Sonar calls.
# Prints PASSED, or FAILED plus one line per broken case (exit 1).
#   pwsh -NoProfile -File tools/pr-gates/selftest.ps1
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'PrGates.psm1') -Force

$now = [datetime]'2026-11-04T12:00:00Z'
$old = '2026-11-04T11:50:00Z'      # finished well outside the grace window
$recent = '2026-11-04T11:59:30Z'   # finished inside the grace window

function Run([string]$Name, [string]$Status = 'completed', [string]$Conclusion = 'success', [string]$CompletedAt = $old, [long]$Id = 1, [string]$StartedAt = '2026-11-04T11:40:00Z') {
    [pscustomobject]@{ id = $Id; name = $Name; status = $Status; conclusion = $(if ($Status -eq 'completed') { $Conclusion } else { $null })
        started_at = $StartedAt; completed_at = $(if ($Status -eq 'completed') { $CompletedAt } else { $null }); html_url = "https://example/$Id" }
}
$job = 'Build, test and analyze'; $sq = 'SonarCloud Code Analysis'; $cq = 'CodeQL'

# An Actions workflow run as /actions/runs returns it (default setup: path dynamic/github-code-scanning/codeql).
function Wf([string]$Status = 'completed', [string]$Conclusion = 'success', [string]$UpdatedAt = $old,
    [string]$Path = 'dynamic/github-code-scanning/codeql', [long]$Id = 100) {
    [pscustomobject]@{ id = $Id; name = 'PR #1'; path = $Path; status = $Status
        conclusion = $(if ($Status -eq 'completed') { $Conclusion } else { $null }); updated_at = $UpdatedAt; html_url = "https://example/run/$Id" }
}

$failures = [System.Collections.Generic.List[string]]::new()
function Expect([string]$Case, [string]$Actual, [string]$Expected) {
    if ($Actual -ne $Expected) { $failures.Add("$Case : got $Actual, expected $Expected") }
}

# --- SonarQube Cloud -------------------------------------------------------------------------
$sonar = @(
    @{ n = 'sonar nothing yet';            runs = @();                                                               e = 'MISSING' }
    @{ n = 'sonar job queued';             runs = @((Run $job 'queued'));                                            e = 'PENDING' }
    @{ n = 'sonar job running';            runs = @((Run $job 'in_progress'));                                       e = 'PENDING' }
    @{ n = 'sonar green';                  runs = @((Run $job), (Run $sq -Id 2));                                    e = 'GREEN' }
    @{ n = 'sonar gate red';               runs = @((Run $job -Conclusion failure), (Run $sq -Conclusion failure -Id 2)); e = 'RED' }
    @{ n = 'sonar skipped: no token';      runs = @((Run $job));                                                     e = 'SKIPPED' }
    @{ n = 'sonar gate check lagging';     runs = @((Run $job -CompletedAt $recent));                                e = 'PENDING' }
    @{ n = 'sonar build failed';           runs = @((Run $job -Conclusion failure));                                 e = 'BUILD_FAILED' }
    @{ n = 'sonar cancelled';              runs = @((Run $job -Conclusion cancelled));                               e = 'INFRA_FAILED' }
    @{ n = 'sonar timed out';              runs = @((Run $job -Conclusion timed_out), (Run $sq -Id 2));              e = 'INFRA_FAILED' }
    @{ n = 'sonar gate check running';     runs = @((Run $job), (Run $sq 'in_progress' -Id 2));                      e = 'PENDING' }
    @{ n = 'sonar gate neutral';           runs = @((Run $job), (Run $sq -Conclusion neutral -Id 2));                e = 'INFRA_FAILED' }
    @{ n = 'sonar rerun wins (red->green)'; runs = @((Run $job -Conclusion failure -Id 1), (Run $sq -Conclusion failure -Id 2),
                                                    (Run $job -Id 3 -StartedAt '2026-11-04T11:45:00Z'), (Run $sq -Id 4 -StartedAt '2026-11-04T11:45:00Z')); e = 'GREEN' }
    @{ n = 'sonar rerun in progress';      runs = @((Run $job -Id 1), (Run $sq -Id 2), (Run $job 'in_progress' -Id 3 -StartedAt '2026-11-04T11:45:00Z')); e = 'PENDING' }
    # Re-runs: newest attempt by id even without started_at; an older attempt's gate check is not reused.
    @{ n = 'sonar queued rerun, no started_at'; runs = @((Run $job -Id 1), (Run $sq -Id 2), (Run $job 'queued' -Id 3 -StartedAt '')); e = 'PENDING' }
    @{ n = 'sonar rerun skipped analysis, old gate'; runs = @((Run $job -Id 1), (Run $sq -Id 2), (Run $job -Id 3));          e = 'SKIPPED' }
    @{ n = 'sonar rerun finished, gate on its way';  runs = @((Run $job -Id 1), (Run $sq -Id 2), (Run $job -Id 3 -CompletedAt $recent)); e = 'PENDING' }
    @{ n = 'sonar rerun red after old green';  runs = @((Run $job -Id 1), (Run $sq -Id 2), (Run $job -Conclusion failure -Id 3), (Run $sq -Conclusion failure -Id 4)); e = 'RED' }
)
foreach ($c in $sonar) { Expect $c.n (Get-SonarGateState -CheckRuns $c.runs -Now $now).state $c.e }

# --- CodeQL --------------------------------------------------------------------------------
$cs = 'Analyze (csharp)'; $ac = 'Analyze (actions)'
$codeql = @(
    @{ n = 'codeql not enabled';           runs = @((Run $job));                                                     e = 'MISSING' }
    @{ n = 'codeql analyzing';             runs = @((Run $cs 'in_progress'), (Run $ac -Id 2));                       e = 'PENDING' }
    @{ n = 'codeql green';                 runs = @((Run $cs), (Run $ac -Id 2), (Run $cq -Id 3));                    e = 'GREEN' }
    @{ n = 'codeql new alerts';            runs = @((Run $cs), (Run $ac -Id 2), (Run $cq -Conclusion failure -Id 3)); e = 'RED' }
    @{ n = 'codeql analysis failed';       runs = @((Run $cs -Conclusion failure), (Run $ac -Id 2));                 e = 'INFRA_FAILED' }
    @{ n = 'codeql analysis cancelled';    runs = @((Run $cs -Conclusion cancelled), (Run $cq -Id 3));               e = 'INFRA_FAILED' }
    @{ n = 'codeql results lagging';       runs = @((Run $cs -CompletedAt $recent));                                 e = 'PENDING' }
    @{ n = 'codeql results never posted';  runs = @((Run $cs));                                                      e = 'MISSING' }
    @{ n = 'codeql results running';       runs = @((Run $cs), (Run $cq 'in_progress' -Id 3));                       e = 'PENDING' }
    @{ n = 'codeql results neutral';       runs = @((Run $cs), (Run $cq -Conclusion neutral -Id 3));                 e = 'INFRA_FAILED' }
    @{ n = 'codeql results check only';    runs = @((Run $cq -Id 3));                                                e = 'GREEN' }
    # Real ordering seen on PR #11: the results check completes before the last Analyze job does.
    @{ n = 'codeql results before last job completes'; runs = @((Run $cs -Id 1 -CompletedAt $old), (Run $ac -Id 2), (Run $cq -Id 3 -CompletedAt '2026-11-04T11:45:00Z')); e = 'GREEN' }
    @{ n = 'codeql rerun, new results pending';  runs = @((Run $cs -Id 1), (Run $cq -Id 2), (Run $cs -Id 3 -CompletedAt $recent)); e = 'PENDING' }
    @{ n = 'codeql rerun, results never came';   runs = @((Run $cs -Id 1), (Run $cq -Id 2), (Run $cs -Id 3));                    e = 'MISSING' }
    @{ n = 'codeql queued rerun, old green';     runs = @((Run $cs -Id 1), (Run $cq -Id 2), (Run $cs 'queued' -Id 3 -StartedAt '')); e = 'PENDING' }
    # The default-setup workflow run covers language jobs that are not registered as check runs yet.
    @{ n = 'codeql later language not registered, run in progress'; runs = @((Run $ac -Id 2), (Run $cq -Id 3)); wf = @((Wf 'in_progress')); e = 'PENDING' }
    @{ n = 'codeql run done, all green';          runs = @((Run $cs), (Run $ac -Id 2), (Run $cq -Id 3)); wf = @((Wf));                  e = 'GREEN' }
    @{ n = 'codeql run failed, visible jobs green'; runs = @((Run $ac -Id 2), (Run $cq -Id 3));          wf = @((Wf -Conclusion failure)); e = 'INFRA_FAILED' }
    @{ n = 'codeql run queued, no checks yet';    runs = @();                                            wf = @((Wf 'queued'));           e = 'PENDING' }
    @{ n = 'codeql workflow re-run, old green';   runs = @((Run $cs), (Run $ac -Id 2), (Run $cq -Id 3)); wf = @((Wf 'in_progress'));      e = 'PENDING' }
    @{ n = 'codeql run done, results lagging';    runs = @();                                            wf = @((Wf -UpdatedAt $recent)); e = 'PENDING' }
    @{ n = 'codeql run done, results never came'; runs = @();                                            wf = @((Wf));                    e = 'MISSING' }
    @{ n = 'codeql other workflows ignored';      runs = @((Run $cs), (Run $cq -Id 3)); wf = @((Wf 'in_progress' -Path '.github/workflows/sonar.yml')); e = 'GREEN' }
)
foreach ($c in $codeql) {
    $wfRuns = if ($c.ContainsKey('wf')) { $c.wf } else { @() }
    Expect $c.n (Get-CodeQLGateState -CheckRuns $c.runs -WorkflowRuns $wfRuns -Now $now).state $c.e
}

# --- Overall ---------------------------------------------------------------------------------
function G([string]$s) { [pscustomobject]@{ state = $s } }
Expect 'overall both green'       (Get-OverallState @((G GREEN), (G GREEN)))        'READY_FOR_COPILOT'
Expect 'overall one pending'      (Get-OverallState @((G RED), (G PENDING)))        'PENDING'
Expect 'overall red + green'      (Get-OverallState @((G RED), (G GREEN)))          'BLOCKED'
Expect 'overall skipped + green'  (Get-OverallState @((G SKIPPED), (G GREEN)))      'BLOCKED'
Expect 'overall missing + green'  (Get-OverallState @((G GREEN), (G MISSING)))      'BLOCKED'

# --- -Wait continuation (startup grace for checks not registered yet) ------------------------
$wait = @(
    @{ n = 'wait: pending keeps waiting';          o = 'PENDING';           g = @((G PENDING), (G GREEN));  s = 900; e = $true }
    @{ n = 'wait: missing during startup grace';   o = 'BLOCKED';           g = @((G MISSING), (G MISSING)); s = 20;  e = $true }
    @{ n = 'wait: missing + green during grace';   o = 'BLOCKED';           g = @((G MISSING), (G GREEN));   s = 60;  e = $true }
    @{ n = 'wait: missing after grace is final';   o = 'BLOCKED';           g = @((G MISSING), (G GREEN));   s = 400; e = $false }
    @{ n = 'wait: red is final even in grace';     o = 'BLOCKED';           g = @((G RED), (G MISSING));     s = 20;  e = $false }
    @{ n = 'wait: skipped is final even in grace'; o = 'BLOCKED';           g = @((G SKIPPED), (G GREEN));   s = 20;  e = $false }
    @{ n = 'wait: ready stops';                    o = 'READY_FOR_COPILOT'; g = @((G GREEN), (G GREEN));     s = 20;  e = $false }
    @{ n = 'wait: stale stops';                    o = 'STALE';             g = @((G MISSING), (G MISSING)); s = 20;  e = $false }
)
foreach ($c in $wait) { Expect $c.n "$(Test-KeepWaiting $c.o $c.g $c.s 300)" "$($c.e)" }

# --- Fingerprints ----------------------------------------------------------------------------
$a = Get-FindingFingerprint 'sonar' 'csharpsquid:S1481' 'src/A.cs' 'Remove the unused  local variable'
$b = Get-FindingFingerprint 'sonar' 'csharpsquid:S1481' 'src/A.cs' 'remove the unused local variable '
$c2 = Get-FindingFingerprint 'sonar' 'csharpsquid:S1481' 'src/B.cs' 'Remove the unused local variable'
Expect 'fingerprint ignores case/whitespace' ($a -eq $b) 'True'
Expect 'fingerprint differs by file' ($a -eq $c2) 'False'

# --- Copilot thread resolution ---------------------------------------------------------------
# Modeled on PR #13: review 1 on commit A raised a thread (comment 101), fix commit F, head H = F,
# review 2 on H. What differs per case is what review 2's overview says about comment 101.
$A = 'a' * 40; $F = 'f' * 40; $H = $F; $X = 'e' * 40
function Overview([string]$Verdict = 'Approval recommended', [string]$Sections = '') {
    "<!-- ccr-overview-v2 -->`n`n### 🟢 $Verdict`n`nSummary.`n`n$Sections`n<details>`n<summary><strong>What changed in this PR</strong></summary>`nSee #discussion_r999 for context.`n</details>"
}
function Section([string]$Heading, [long[]]$Ids) {
    "<details open>`n<summary><strong>$Heading</strong></summary>`n`n" + (($Ids | ForEach-Object { "- [Finding](https://github.com/o/r/pull/1#discussion_r$_) · Fixed" }) -join "`n") + "`n</details>`n"
}
$resolvedBody = Overview -Sections (Section '1 resolved since last review' 101)
$openBody     = Overview -Sections (Section '1 open issue' 101)      # what PR #13's re-review actually said
$changesBody  = Overview 'Changes recommended' (Section '1 resolved since last review' 101)
function Rev([long]$Id, [string]$Commit, [string]$At, [string]$Body = $resolvedBody) { [pscustomobject]@{ id = $Id; commit = $Commit; submittedAt = $At; body = $Body } }
function Evt([string]$Event, [string]$At, [string]$Reviewer = '') { [pscustomobject]@{ event = $Event; createdAt = $At; reviewer = $Reviewer } }
function Cm([string]$Author, [string]$Body, [string]$At, $ReviewId = $null, [string]$Type = 'User', [string]$Assoc = 'OWNER', [long]$DbId = 200) {
    [pscustomobject]@{ databaseId = $DbId; author = $Author; authorType = $Type; authorAssociation = $Assoc; body = $Body; createdAt = $At; reviewId = $ReviewId }
}
function CopilotCm([long]$ReviewId, [string]$At = '2026-11-04T20:54:30Z', [long]$DbId = 101) { Cm 'copilot-pull-request-reviewer' 'Gate can miss late jobs.' $At $ReviewId 'Bot' 'NONE' $DbId }
function Th([string]$Id, [object[]]$Comments, [string]$Path = 'tools/pr-gates/pr-gates.ps1', [bool]$Resolved = $false, [bool]$Outdated = $true) {
    [pscustomobject]@{ id = $Id; isResolved = $Resolved; isOutdated = $Outdated; path = $Path; originalCommit = $A; comments = $Comments }
}
function Ready([string]$Body) { Get-CopilotReReviewState -Reviews @((Rev 1 $A '2026-11-04T20:54:31Z' (Overview 'Changes recommended')), (Rev 2 $H '2026-11-04T21:05:07Z' $Body)) -Events @() -HeadSha $H }
$fixReply = Cm 'theyur' "Fixed in $($F.Substring(0,7)): the gate now waits for the workflow run." '2026-11-04T21:00:22Z'
$ready = Get-CopilotReReviewState -Reviews @((Rev 1 $A '2026-11-04T20:54:31Z' (Overview 'Changes recommended')), (Rev 2 $H '2026-11-04T21:05:07Z')) -HeadSha $H -Events @(
    (Evt review_requested '2026-11-04T20:51:27Z' Copilot), (Evt copilot_work_started '2026-11-04T20:51:52Z'),
    (Evt review_requested '2026-11-04T21:01:56Z' Copilot), (Evt copilot_work_started '2026-11-04T21:02:23Z'))
# Fake commit lookups: F is in the head after A; X is not in the head; A is the commented commit.
$commits = {
    param($Sha, $Thread)
    switch -Regex ($Sha) {
        '^f+$'  { [pscustomobject]@{ found = $true; sha = $F; inHead = $true; afterFinding = $true; files = @('tools/pr-gates/pr-gates.ps1') } }
        '^e+$'  { [pscustomobject]@{ found = $true; sha = $X; inHead = $false; afterFinding = $true; files = @('tools/pr-gates/pr-gates.ps1') } }
        '^a+$'  { [pscustomobject]@{ found = $true; sha = $A; inHead = $true; afterFinding = $false; files = @('tools/pr-gates/pr-gates.ps1') } }
        default { [pscustomobject]@{ found = $false; sha = $Sha; inHead = $false; afterFinding = $false; files = @() } }
    }
}
$good = Th 't1' @((CopilotCm 1), $fixReply)

# Overview parsing.
$ov = Get-CopilotOverviewStatus (Overview -Sections ((Section '2 resolved since last review' 101, 102) + (Section '1 open issue' 103) + (Section '1 previously missed' 104) + (Section 'Something new' 105)))
Expect 'overview: resolved section'       $ov['101'] 'resolved'
Expect 'overview: open section'           $ov['103'] 'open'
Expect 'overview: previously missed'      $ov['104'] 'open'
Expect 'overview: unknown section'        $ov['105'] 'unknown'
Expect 'overview: "what changed" link'    $ov['999'] 'unknown'
Expect 'overview: open beats resolved'    (Get-CopilotOverviewStatus (Overview -Sections ((Section '1 resolved since last review' 101) + (Section '1 open issue' 101))))['101'] 'open'
Expect 'overview: no marker, no status'   (Get-CopilotOverviewStatus ((Section '1 resolved since last review' 101))).Count '0'
Expect 'verdict: approve'                 (Get-CopilotVerdict $resolvedBody) 'APPROVE'
Expect 'verdict: changes'                 (Get-CopilotVerdict $changesBody) 'CHANGES'
Expect 'verdict: no overview marker'      (Get-CopilotVerdict '### 🟢 Approval recommended') 'UNKNOWN'

# PR-level re-review state.
$r1 = Rev 1 $A '2026-11-04T20:54:31Z' (Overview 'Changes recommended'); $r2 = Rev 2 $H '2026-11-04T21:05:07Z'
$rr = @(
    @{ n = 'rereview: on head';                  r = @($r1, $r2); ev = @(); e = 'READY' }
    @{ n = 'rereview: none';                     r = @();         ev = @(); e = 'NO_REVIEW' }
    @{ n = 'rereview: only on old commit';       r = @($r1);      ev = @(); e = 'NOT_ON_HEAD' }
    @{ n = 'rereview: requested, not started';   r = @($r1, $r2); ev = @((Evt review_requested '2026-11-04T21:10:00Z' Copilot)); e = 'PENDING' }
    @{ n = 'rereview: running';                  r = @($r1, $r2); ev = @((Evt copilot_work_started '2026-11-04T21:10:00Z')); e = 'PENDING' }
    @{ n = 'rereview: other reviewer requested'; r = @($r1, $r2); ev = @((Evt review_requested '2026-11-04T21:10:00Z' someone)); e = 'READY' }
    @{ n = 'rereview: unknown format';           r = @($r1, (Rev 2 $H '2026-11-04T21:05:07Z' 'Looks good to me')); ev = @(); e = 'UNKNOWN_FORMAT' }
    @{ n = 'rereview: empty body';               r = @($r1, (Rev 2 $H '2026-11-04T21:05:07Z' '')); ev = @(); e = 'UNKNOWN_FORMAT' }
)
foreach ($c in $rr) { Expect $c.n (Get-CopilotReReviewState -Reviews $c.r -Events $c.ev -HeadSha $H).state $c.e }
Expect 'rereview: PR #13 event sequence' $ready.state 'READY'

# Per-thread decisions against a re-review that lists comment 101 as resolved. Only the RESOLVE
# rows may resolve; each KEEP row removes exactly one piece of evidence.
$sha7 = $F.Substring(0, 7)
$tc = @(
    @{ n = 'thread: both confirmations resolve';   t = $good; e = 'RESOLVE' }
    @{ n = 'thread: full-length sha';              t = (Th 't1' @((CopilotCm 1), (Cm 'theyur' "Fixed in $F." '2026-11-04T21:00:22Z'))); e = 'RESOLVE' }
    @{ n = 'thread: already resolved';             t = (Th 't1' @((CopilotCm 1), $fixReply) -Resolved $true); e = 'SKIP' }
    @{ n = 'thread: human thread ignored';         t = (Th 't1' @((Cm 'theyur' 'note' '2026-11-04T20:00:00Z'), $fixReply)); e = 'SKIP' }
    @{ n = 'thread: no reply';                     t = (Th 't1' @((CopilotCm 1))); e = 'KEEP' }
    @{ n = 'thread: rejected (false positive)';    t = (Th 't1' @((CopilotCm 1), (Cm 'theyur' 'Not changed: this is intended, see ADR 0001.' '2026-11-04T21:00:22Z'))); e = 'KEEP' }
    @{ n = 'thread: partial fix wording';          t = (Th 't1' @((CopilotCm 1), (Cm 'theyur' "Partially fixed in $sha7; the rest needs a decision." '2026-11-04T21:00:22Z'))); e = 'KEEP' }
    @{ n = 'thread: fix mentioned mid-sentence';   t = (Th 't1' @((CopilotCm 1), (Cm 'theyur' "Same gap, fixed in $sha7." '2026-11-04T21:00:22Z'))); e = 'KEEP' }
    @{ n = 'thread: "Fixed in" not leading';       t = (Th 't1' @((CopilotCm 1), (Cm 'theyur' "Reverted: Fixed in $sha7 was wrong." '2026-11-04T21:00:22Z'))); e = 'KEEP' }
    @{ n = 'thread: Copilot answered after fix';   t = (Th 't1' @((CopilotCm 1), $fixReply, (CopilotCm 2 '2026-11-04T21:05:07Z' 300))); e = 'KEEP' }
    @{ n = 'thread: user followed up after fix';   t = (Th 't1' @((CopilotCm 1), $fixReply, (Cm 'theyur' 'Keep this open, still flaky.' '2026-11-04T21:06:00Z'))); e = 'KEEP' }
    @{ n = 'thread: reply by outsider';            t = (Th 't1' @((CopilotCm 1), (Cm 'drive-by' "Fixed in $sha7" '2026-11-04T21:00:22Z' -Assoc 'NONE'))); e = 'KEEP' }
    @{ n = 'thread: reply by a bot';               t = (Th 't1' @((CopilotCm 1), (Cm 'some-bot' "Fixed in $sha7" '2026-11-04T21:00:22Z' -Type 'Bot' -Assoc 'NONE'))); e = 'KEEP' }
    @{ n = 'thread: fix commit not in head';       t = (Th 't1' @((CopilotCm 1), (Cm 'theyur' "Fixed in $($X.Substring(0,7))" '2026-11-04T21:00:22Z'))); e = 'KEEP' }
    @{ n = 'thread: fix commit unknown';           t = (Th 't1' @((CopilotCm 1), (Cm 'theyur' 'Fixed in 1234567' '2026-11-04T21:00:22Z'))); e = 'KEEP' }
    @{ n = 'thread: fix predates finding';         t = (Th 't1' @((CopilotCm 1), (Cm 'theyur' "Fixed in $($A.Substring(0,7))" '2026-11-04T21:00:22Z'))); e = 'KEEP' }
    @{ n = 'thread: raised by latest review';      t = (Th 't1' @((CopilotCm 2), $fixReply)); e = 'KEEP' }
    @{ n = 'thread: finding not in overview';      t = (Th 't1' @((CopilotCm 1 -DbId 555), $fixReply)); e = 'KEEP' }
)
foreach ($c in $tc) { Expect $c.n (Get-ThreadResolution -Thread $c.t -ReReview $ready -Threads @($c.t) -CommitInfo $commits).decision $c.e }

# Copilot's per-finding status decides; its verdict does not.
Expect 'thread: PR #13 - approval but finding still open' (Get-ThreadResolution -Thread $good -ReReview (Ready $openBody) -Threads @($good) -CommitInfo $commits).decision 'KEEP'
Expect 'thread: changes recommended, finding resolved'    (Get-ThreadResolution -Thread $good -ReReview (Ready $changesBody) -Threads @($good) -CommitInfo $commits).decision 'RESOLVE'
Expect 'thread: approval, no sections at all'             (Get-ThreadResolution -Thread $good -ReReview (Ready (Overview)) -Threads @($good) -CommitInfo $commits).decision 'KEEP'
Expect 'thread: unknown section heading'                  (Get-ThreadResolution -Thread $good -ReReview (Ready (Overview -Sections (Section 'Addressed maybe' 101))) -Threads @($good) -CommitInfo $commits).decision 'KEEP'
# Re-raise / duplicate handling.
$newOnSamePath = Th 't9' @((CopilotCm 2 '2026-11-04T21:05:06Z' 301))
Expect 'thread: re-raised on same file'   (Get-ThreadResolution -Thread $good -ReReview $ready -Threads @($good, $newOnSamePath) -CommitInfo $commits).decision 'KEEP'
Expect 'thread: new thread elsewhere ok'  (Get-ThreadResolution -Thread $good -ReReview $ready -Threads @($good, (Th 't9' @((CopilotCm 2 -DbId 301)) -Path 'README.md')) -CommitInfo $commits).decision 'RESOLVE'
Expect 'thread: resolved re-raise ignored' (Get-ThreadResolution -Thread $good -ReReview $ready -Threads @($good, (Th 't9' @((CopilotCm 2 -DbId 301)) -Resolved $true)) -CommitInfo $commits).decision 'RESOLVE'
# Qualifying thread with no usable re-review: wait, never resolve.
$prLevel = @('PENDING', 'NOT_ON_HEAD', 'UNKNOWN_FORMAT', 'NO_REVIEW')
foreach ($s in $prLevel) {
    $st = [pscustomobject]@{ state = $s; reason = $s; review = $r2 }
    Expect "thread: re-review $s" (Get-ThreadResolution -Thread $good -ReReview $st -Threads @($good) -CommitInfo $commits).decision 'WAIT'
}

# Approval mode (-AcceptApproval): the PR #13 case resolves; everything that removes evidence other
# than Copilot's per-finding listing still keeps the thread open.
function Decide($Thread, $Body, [object[]]$All = @($Thread)) {
    (Get-ThreadResolution -Thread $Thread -ReReview (Ready $Body) -Threads $All -CommitInfo $commits -AcceptApproval).decision
}
$notOutdatedGood = Th 't1' @((CopilotCm 1), $fixReply) -Outdated $false
$ap = @(
    @{ n = 'approval: PR #13 - approval, finding listed open';  d = (Decide $good $openBody);   e = 'RESOLVE' }
    @{ n = 'approval: approval, finding not listed';           d = (Decide $good (Overview));  e = 'RESOLVE' }
    @{ n = 'approval: confirmed still resolves';               d = (Decide $good $resolvedBody); e = 'RESOLVE' }
    @{ n = 'approval: changes recommended, finding open';      d = (Decide $good (Overview 'Changes recommended' (Section '1 open issue' 101))); e = 'KEEP' }
    @{ n = 'approval: verdict unrecognized';                   d = (Decide $good (Overview 'Looks fine' (Section '1 open issue' 101))); e = 'KEEP' }
    @{ n = 'approval: unknown section heading';                d = (Decide $good (Overview -Sections (Section 'Addressed maybe' 101))); e = 'KEEP' }
    @{ n = 'approval: not outdated, fix touches file';         d = (Decide $notOutdatedGood $openBody); e = 'RESOLVE' }
    @{ n = 'approval: not outdated, fix elsewhere';            d = (Decide (Th 't1' @((CopilotCm 1), $fixReply) -Path 'README.md' -Outdated $false) $openBody); e = 'KEEP' }
    @{ n = 'approval: re-raised on same file';                 d = (Decide $good $openBody @($good, $newOnSamePath)); e = 'KEEP' }
)
foreach ($c in $ap) { Expect $c.n $c.d $c.e }
# Every claim-side negative from the strict table stays open in approval mode too.
foreach ($c in @($tc | Where-Object { $_.e -eq 'KEEP' -and $_.n -ne 'thread: finding not in overview' })) {
    Expect "approval/$($c.n)" (Decide $c.t $openBody) 'KEEP'
}
foreach ($s in $prLevel) {
    $st = [pscustomobject]@{ state = $s; reason = $s; review = $r2 }
    Expect "approval: re-review $s" (Get-ThreadResolution -Thread $good -ReReview $st -Threads @($good) -CommitInfo $commits -AcceptApproval).decision 'WAIT'
}
Expect 'approval: basis reported' (Get-ThreadResolution -Thread $good -ReReview (Ready $openBody) -Threads @($good) -CommitInfo $commits -AcceptApproval).basis 'approval'
Expect 'strict: basis reported'   (Get-ThreadResolution -Thread $good -ReReview $ready -Threads @($good) -CommitInfo $commits).basis 'copilot-confirmed'
$approvalKeeps = @($tc | Where-Object { $_.e -eq 'KEEP' -and $_.n -ne 'thread: finding not in overview' }).Count
$resolutionCases = 10 + $rr.Count + 1 + $tc.Count + 4 + 3 + $prLevel.Count + $ap.Count + $approvalKeeps + $prLevel.Count + 2


# --- /pr-review guard hook -------------------------------------------------------------------
$hook = Join-Path $PSScriptRoot '..\..\.claude\hooks\block-pr-review.ps1'
$hookCases = @(
    @{ n = 'hook: typed /pr-review';          j = '{"hook_event_name":"UserPromptExpansion","command_name":"pr-review","prompt":"/pr-review master"}'; e = 2 }
    @{ n = 'hook: typed other command';       j = '{"hook_event_name":"UserPromptExpansion","command_name":"speckit-plan","prompt":"/speckit-plan"}'; e = 0 }
    @{ n = 'hook: Skill pr-review';           j = '{"hook_event_name":"PreToolUse","tool_name":"Skill","tool_input":{"skill":"pr-review","args":"x"}}'; e = 2 }
    @{ n = 'hook: Skill qualified pr-review'; j = '{"hook_event_name":"PreToolUse","tool_name":"Skill","tool_input":{"skill":"user:pr-review"}}'; e = 2 }
    @{ n = 'hook: Skill pr-review-toolkit';   j = '{"hook_event_name":"PreToolUse","tool_name":"Skill","tool_input":{"skill":"pr-review-toolkit:review-pr"}}'; e = 0 }
    @{ n = 'hook: Skill other';               j = '{"hook_event_name":"PreToolUse","tool_name":"Skill","tool_input":{"skill":"process-pr-feedback"}}'; e = 0 }
    @{ n = 'hook: unreadable input';          j = 'not json'; e = 0 }
)
foreach ($c in $hookCases) {
    $null = $c.j | pwsh -NoProfile -File $hook 2>$null
    Expect $c.n "$LASTEXITCODE" "$($c.e)"
}

$total = $sonar.Count + $codeql.Count + 5 + $wait.Count + 2 + $resolutionCases + $hookCases.Count
if ($failures.Count) {
    'FAILED'
    $failures | ForEach-Object { "  $_" }
    exit 1
}
"PASSED ($total cases)"
