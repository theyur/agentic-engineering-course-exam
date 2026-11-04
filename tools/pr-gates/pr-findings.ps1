<#
.SYNOPSIS
  Findings for a PR's current head SHA, normalized across SonarQube Cloud, CodeQL and Copilot.

.DESCRIPTION
  -Stage static   SonarQube Cloud quality-gate conditions, open issues and hotspots for the PR;
                  CodeQL results-check annotations (what the PR introduces) and open code-scanning
                  alerts on refs/pull/N/head (default setup), each tied to the analyzed commit.
  -Stage copilot  Copilot review submissions and its inline review threads (resolved / outdated
                  state from GraphQL), separate from ordinary PR comments.

  Every finding carries a fingerprint (source|rule|path|message), the SHA it applies to, a `stale`
  flag when it was produced for an older revision, and its history from the local feedback state
  (see pr-state.ps1). Sources that fail are reported under `errors`, not as findings.

.EXAMPLE
  pwsh -NoProfile -File tools/pr-gates/pr-findings.ps1 -Pr 12 -Stage static
#>
param(
    [int]$Pr,
    [Parameter(Mandatory)][ValidateSet('static', 'copilot')][string]$Stage,
    [string]$Repo,
    [string]$SonarProjectKey
)
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'PrGates.psm1') -Force

if (-not $Repo) { $Repo = Get-RepoSlug }
$info = Get-PrInfo $Repo $Pr
$Pr = $info.number
$head = $info.headRefOid
$state = Read-FeedbackState $Pr
$findings = [System.Collections.Generic.List[object]]::new()
$errors = [System.Collections.Generic.List[object]]::new()
$meta = [ordered]@{}

function Add-Finding([string]$Source, [string]$Kind, [string]$Severity, [string]$Rule, [string]$Path, $Line,
    [string]$Message, [string]$Url, [string]$Sha) {
    $id = Get-FindingFingerprint $Source $Rule $Path $Message
    $findings.Add([pscustomobject]@{
            id = $id; source = $Source; kind = $Kind; severity = $Severity; rule = $Rule
            path = $Path; line = $Line; message = $Message; url = $Url; sha = $Sha
            stale = [bool]($Sha -and $Sha -ne $head)
            history = $(if ($state.findings.ContainsKey($id)) { $state.findings[$id] } else { @() })
        })
}

function Get-SonarKey {
    if ($SonarProjectKey) { return $SonarProjectKey }
    $wf = Join-Path $PSScriptRoot '..\..\.github\workflows\sonar.yml'
    if ((Test-Path $wf) -and ((Get-Content $wf -Raw) -match "SONAR_PROJECT_KEY:.*\|\|\s*'([^']+)'")) { return $Matches[1] }
    throw 'Sonar project key unknown; pass -SonarProjectKey.'
}

if ($Stage -eq 'static') {
    # ---- SonarQube Cloud (public web API; PR analyses are keyed by PR number) ----
    try {
        $key = Get-SonarKey
        $prs = Invoke-SonarApi "project_pull_requests/list?project=$key"
        $analysis = $prs.pullRequests | Where-Object key -eq "$Pr" | Select-Object -First 1
        $sonarSha = if ($analysis) { $analysis.commit.sha } else { $null }
        $meta.sonar = [ordered]@{ projectKey = $key; analyzedSha = $sonarSha; qualityGate = $(if ($analysis) { $analysis.status.qualityGateStatus })
            url = "https://sonarcloud.io/summary/new_code?id=$key&pullRequest=$Pr" }
        if (-not $analysis) { $errors.Add([pscustomobject]@{ source = 'sonar'; error = "no SonarQube Cloud analysis for PR #$Pr" }) }
        else {
            $gate = Invoke-SonarApi "qualitygates/project_status?projectKey=$key&pullRequest=$Pr"
            foreach ($c in @($gate.projectStatus.conditions | Where-Object status -eq 'ERROR')) {
                Add-Finding 'sonar' 'gate-condition' 'BLOCKER' $c.metricKey '' $null `
                    "Quality gate condition $($c.metricKey): actual $($c.actualValue), threshold $($c.comparator) $($c.errorThreshold)" `
                    $meta.sonar.url $sonarSha
            }
            foreach ($i in @(Get-SonarAll "issues/search?componentKeys=$key&pullRequest=$Pr&resolved=false" 'issues')) {
                $path = ($i.component -replace "^$([regex]::Escape($key)):", '')
                $line = if ($i.PSObject.Properties['line']) { $i.line } else { $null }
                Add-Finding 'sonar' "issue:$($i.type)" $i.severity $i.rule $path $line $i.message `
                    "https://sonarcloud.io/project/issues?id=$key&pullRequest=$Pr&open=$($i.key)" $sonarSha
            }
            foreach ($h in @(Get-SonarAll "hotspots/search?projectKey=$key&pullRequest=$Pr&status=TO_REVIEW" 'hotspots')) {
                $path = ($h.component -replace "^$([regex]::Escape($key)):", '')
                $line = if ($h.PSObject.Properties['line']) { $h.line } else { $null }
                Add-Finding 'sonar' 'hotspot' $h.vulnerabilityProbability $h.ruleKey $path $line $h.message `
                    "https://sonarcloud.io/project/security_hotspots?id=$key&pullRequest=$Pr&hotspots=$($h.key)" $sonarSha
            }
        }
    } catch { $errors.Add([pscustomobject]@{ source = 'sonar'; error = $_.Exception.Message }) }

    # ---- CodeQL ----
    try {
        $runs = Get-CheckRuns $Repo $head
        $cfg = Get-PrGatesConfig
        $check = Select-LatestCheck $runs $cfg.CodeQLGateCheck
        $meta.codeql = [ordered]@{ resultsCheck = $(if ($check) { $check.html_url }); conclusion = $(if ($check) { $check.conclusion }) }
        if ($check) {
            # Annotations of the results check = alerts the PR introduces, reported against the head SHA.
            foreach ($a in @(Invoke-GhList "repos/$Repo/check-runs/$($check.id)/annotations?per_page=100")) {
                Add-Finding 'codeql' 'annotation' $a.annotation_level $a.title $a.path $a.start_line $a.message $check.html_url $head
            }
        }
        # Open alerts on the PR ref, each tied to the commit its latest instance was found in.
        # Default setup analyzes refs/pull/N/head at the head SHA (verified on PR #12). An advanced
        # workflow would analyze refs/pull/N/merge instead; there the merge commit's second parent is
        # the PR head it belongs to. Either way, an instance from another SHA is marked stale.
        $alertRef = "refs/pull/$Pr/head"
        $alerts = @(Invoke-GhList "repos/$Repo/code-scanning/alerts?ref=$alertRef&state=open&per_page=100")
        if (-not $alerts.Count) {
            $merged = @(Invoke-GhList "repos/$Repo/code-scanning/alerts?ref=refs/pull/$Pr/merge&state=open&per_page=100")
            if ($merged.Count) { $alerts = $merged; $alertRef = "refs/pull/$Pr/merge" }
        }
        $headOf = @{}
        foreach ($al in $alerts) {
            $inst = $al.most_recent_instance
            if (-not $headOf.ContainsKey($inst.commit_sha)) {
                $headOf[$inst.commit_sha] = if ($alertRef -like '*/merge') {
                    ((Invoke-Gh @('api', "repos/$Repo/commits/$($inst.commit_sha)", '--jq', '[.parents[].sha] | join(" ")')).Trim() -split ' ')[-1]
                } else { $inst.commit_sha }
            }
            $sev = if ($al.rule.security_severity_level) { $al.rule.security_severity_level } else { $al.rule.severity }
            Add-Finding 'codeql' 'alert' $sev $al.rule.id $inst.location.path $inst.location.start_line `
                $inst.message.text $al.html_url $headOf[$inst.commit_sha]
        }
        $meta.codeql.alertRef = $alertRef
        # Which language analyses exist for this PR head - a missing category means that language's
        # results are not in yet (or its analysis failed).
        $meta.codeql.analyses = @(Invoke-GhList "repos/$Repo/code-scanning/analyses?ref=refs/pull/$Pr/head&per_page=20" |
            Where-Object { $_.commit_sha -eq $head } |
            ForEach-Object { [pscustomobject]@{ category = $_.category; results = $_.results_count; createdAt = $_.created_at } })
    } catch {
        $msg = $_.Exception.Message
        if ($msg -match 'no analysis found') { $msg = 'no code scanning analysis for this PR yet' }
        $errors.Add([pscustomobject]@{ source = 'codeql'; error = $msg })
    }
}

if ($Stage -eq 'copilot') {
    $isCopilot = { param($login) $login -match '(?i)copilot' }
    try {
        $reviews = Invoke-GhList "repos/$Repo/pulls/$Pr/reviews?per_page=100"
        $meta.copilot = [ordered]@{ reviews = @() }
        foreach ($r in @($reviews | Where-Object { & $isCopilot $_.user.login })) {
            $meta.copilot.reviews += [pscustomobject]@{ id = $r.id; login = $r.user.login; state = $r.state
                submittedAt = $r.submitted_at; commit = $r.commit_id; stale = ($r.commit_id -ne $head); url = $r.html_url }
            if ($r.body -and $r.body.Trim()) {
                Add-Finding 'copilot' 'review-body' 'INFO' 'review' '' $null $r.body $r.html_url $r.commit_id
            }
        }
        $owner, $name = $Repo -split '/'
        $query = @'
query($owner: String!, $name: String!, $number: Int!) {
  repository(owner: $owner, name: $name) {
    pullRequest(number: $number) {
      reviewThreads(first: 100) {
        pageInfo { hasNextPage }
        nodes {
          id isResolved isOutdated path line originalLine
          comments(first: 30) {
            pageInfo { hasNextPage }
            nodes { databaseId author { login } body url createdAt originalCommit { oid } }
          }
        }
      }
    }
  }
}
'@
        $data = Invoke-Gh @('api', 'graphql', '-f', "query=$query", '-F', "owner=$owner", '-F', "name=$name", '-F', "number=$Pr") | ConvertFrom-Json
        $threads = $data.data.repository.pullRequest.reviewThreads
        # Not paged further: say so instead of presenting a partial list as complete.
        if ($threads.pageInfo.hasNextPage) { $errors.Add([pscustomobject]@{ source = 'copilot'; error = 'INCOMPLETE: more than 100 review threads; only the first 100 were read' }) }
        foreach ($t in @($threads.nodes)) {
            if ($t.comments.pageInfo.hasNextPage) { $errors.Add([pscustomobject]@{ source = 'copilot'; error = "INCOMPLETE: thread $($t.id) has more than 30 comments; later replies not read" }) }
            $first = $t.comments.nodes | Select-Object -First 1
            if (-not $first -or -not (& $isCopilot $first.author.login)) { continue }
            $line = if ($t.line) { $t.line } else { $t.originalLine }
            Add-Finding 'copilot' 'review-thread' $(if ($t.isResolved) { 'RESOLVED' } else { 'OPEN' }) 'thread' $t.path $line `
                $first.body $first.url $first.originalCommit.oid
            $last = $findings[$findings.Count - 1]
            $last | Add-Member threadId $t.id
            $last | Add-Member commentId $first.databaseId   # for pulls/<N>/comments/<commentId>/replies
            $last | Add-Member isResolved $t.isResolved
            $last | Add-Member isOutdated $t.isOutdated
            $last | Add-Member replies @($t.comments.nodes | Select-Object -Skip 1 | ForEach-Object { "$($_.author.login): $($_.body)" })
        }
        if (-not $meta.copilot.reviews.Count) { $errors.Add([pscustomobject]@{ source = 'copilot'; error = 'no Copilot review on this PR yet' }) }
    } catch { $errors.Add([pscustomobject]@{ source = 'copilot'; error = $_.Exception.Message }) }
}

# The head can move while we read. Findings collected for an old head must not drive a fix round.
$headNow = (Get-PrInfo $Repo $Pr).headRefOid
$headMoved = $headNow -ne $head
if ($headMoved) { $errors.Add([pscustomobject]@{ source = 'pr'; error = "STALE: head moved from $head to $headNow while collecting; re-run" }) }

[pscustomobject]@{
    repo = $Repo; pr = $Pr; headSha = $head; headMoved = $headMoved; stage = $Stage
    attempts = @($(if ($state.attempts.ContainsKey($Stage)) { $state.attempts[$Stage] }))
    sources = [pscustomobject]$meta
    errors = $errors
    findings = $findings
} | ConvertTo-Json -Depth 8
if ($headMoved) { exit 30 }
