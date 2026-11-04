# PR gate classification and GitHub / SonarQube Cloud data access for tools/pr-gates.
#
# The Get-*GateState functions are pure: they take check runs as GitHub's REST API returns them
# (name, status, conclusion, completed_at, app.slug) plus "now", and return a state. selftest.ps1
# drives them from fixtures. Everything that talks to GitHub or SonarQube Cloud lives below them.
#
# Gate states:
#   GREEN         analysis ran on this head SHA and its gate passed
#   RED           analysis ran and its gate failed (real findings)
#   PENDING       queued / in progress, or a result is expected shortly
#   MISSING       no analysis has started for this head SHA
#   SKIPPED       the workflow ran but did not analyze (e.g. SONAR_TOKEN absent) - NOT green
#   BUILD_FAILED  build or tests failed before the analyzer could report
#   INFRA_FAILED  analysis was cancelled, timed out, or ended in an unexpected state

Set-StrictMode -Version Latest

$script:Config = [ordered]@{
    SonarWorkflowCheck = 'Build, test and analyze'   # job in .github/workflows/sonar.yml
    SonarGateCheck     = 'SonarCloud Code Analysis'  # posted by the SonarQube Cloud app
    CodeQLGateCheck    = 'CodeQL'                    # posted by GitHub Advanced Security (code scanning)
    CodeQLAnalyzeRegex = '^Analyze \('               # per-language jobs of the CodeQL default setup
    # How long after the analysis job finished a missing gate check still counts as "on its way".
    GateCheckGraceSeconds = 180
}

function Get-PrGatesConfig { $script:Config }

$failedToRun = @('cancelled', 'timed_out', 'stale', 'action_required', 'startup_failure')

# Latest check run of that name (re-runs create new check runs with the same name). Ordered by id,
# i.e. creation order: a queued re-run has no started_at yet but is still the newest attempt.
function Select-LatestCheck([object[]]$CheckRuns, [string]$Name) {
    @($CheckRuns | Where-Object { $_.name -eq $Name }) | Sort-Object { [long]$_.id } | Select-Object -Last 1
}

# A gate check belongs to the newest analysis attempt only if it was created after that attempt's
# job(s). Check-run ids are creation-ordered; timestamps are not usable (the CodeQL results check can
# complete before the last Analyze job does). An older gate check is a previous attempt's result.
function Test-ResultIsCurrent($Gate, [object[]]$Jobs) {
    $newestJob = @($Jobs | Where-Object { $_ }) | Measure-Object -Property id -Maximum
    if (-not $newestJob.Count) { return $true }
    [long]$Gate.id -gt [long]$newestJob.Maximum
}

function New-GateResult([string]$State, [string]$Reason, [object[]]$Checks) {
    [pscustomobject]@{
        state  = $State
        reason = $Reason
        checks = @($Checks | Where-Object { $_ } | ForEach-Object {
                [pscustomobject]@{ name = $_.name; status = $_.status; conclusion = $_.conclusion; url = $_.html_url }
            })
    }
}

function Test-WithinGrace($Check, [datetime]$Now) {
    if (-not $Check.completed_at) { return $true }
    ($Now - [datetime]$Check.completed_at).TotalSeconds -lt $script:Config.GateCheckGraceSeconds
}

function Get-SonarGateState {
    param([object[]]$CheckRuns, [datetime]$Now = [datetime]::UtcNow)
    $job = Select-LatestCheck $CheckRuns $script:Config.SonarWorkflowCheck
    $gate = Select-LatestCheck $CheckRuns $script:Config.SonarGateCheck
    if ($job -and $gate -and -not (Test-ResultIsCurrent $gate @($job))) { $gate = $null }   # previous attempt's result
    $checks = @($job, $gate)

    if (-not $job -and -not $gate) { return New-GateResult 'MISSING' 'SonarQube Cloud workflow has not started for this head SHA.' $checks }
    if (-not $job) {
        # Gate check without the workflow job: analysis from somewhere else (e.g. Automatic Analysis).
        if ($gate.status -ne 'completed') { return New-GateResult 'PENDING' 'Sonar quality gate check is still running.' $checks }
        $state = if ($gate.conclusion -eq 'success') { 'GREEN' } elseif ($gate.conclusion -eq 'failure') { 'RED' } else { 'INFRA_FAILED' }
        return New-GateResult $state "Sonar quality gate check '$($gate.conclusion)' without a CI analysis job." $checks
    }
    if ($job.status -ne 'completed') { return New-GateResult 'PENDING' 'SonarQube Cloud workflow is queued or running.' $checks }
    if ($job.conclusion -in $failedToRun) { return New-GateResult 'INFRA_FAILED' "SonarQube Cloud workflow ended '$($job.conclusion)'." $checks }
    if ($job.conclusion -eq 'skipped') { return New-GateResult 'SKIPPED' 'SonarQube Cloud job was skipped.' $checks }

    if (-not $gate) {
        if (Test-WithinGrace $job $Now) { return New-GateResult 'PENDING' 'Workflow finished; waiting for the SonarQube Cloud app to post its gate check.' $checks }
        if ($job.conclusion -eq 'success') { return New-GateResult 'SKIPPED' 'Workflow succeeded but no quality gate was reported - analysis was skipped (SONAR_TOKEN missing?).' $checks }
        return New-GateResult 'BUILD_FAILED' "Workflow ended '$($job.conclusion)' before SonarQube reported (build or test failure)." $checks
    }
    if ($gate.status -ne 'completed') { return New-GateResult 'PENDING' 'Sonar quality gate check is still running.' $checks }
    switch ($gate.conclusion) {
        'success' {
            if ($job.conclusion -eq 'success') { return New-GateResult 'GREEN' 'Quality gate passed.' $checks }
            return New-GateResult 'INFRA_FAILED' "Quality gate passed but the workflow ended '$($job.conclusion)'." $checks
        }
        'failure' { return New-GateResult 'RED' 'Quality gate failed.' $checks }
        default   { return New-GateResult 'INFRA_FAILED' "Sonar quality gate check ended '$($gate.conclusion)'." $checks }
    }
}

function Get-CodeQLGateState {
    param([object[]]$CheckRuns, [datetime]$Now = [datetime]::UtcNow)
    $names = @($CheckRuns | Where-Object { $_.name -match $script:Config.CodeQLAnalyzeRegex } | ForEach-Object name | Sort-Object -Unique)
    $jobs = @($names | ForEach-Object { Select-LatestCheck $CheckRuns $_ })
    $gate = Select-LatestCheck $CheckRuns $script:Config.CodeQLGateCheck
    if ($gate -and $jobs.Count -and -not (Test-ResultIsCurrent $gate $jobs)) { $gate = $null }   # previous attempt's result
    $checks = @($jobs) + @($gate)

    if ($jobs.Count -eq 0 -and -not $gate) { return New-GateResult 'MISSING' 'CodeQL has not started for this head SHA (is code scanning default setup enabled?).' $checks }
    $running = @($jobs | Where-Object { $_.status -ne 'completed' })
    if ($running.Count) { return New-GateResult 'PENDING' "CodeQL analysis running: $(($running.name) -join ', ')." $checks }
    $broken = @($jobs | Where-Object { $_.conclusion -ne 'success' })
    if ($broken.Count) { return New-GateResult 'INFRA_FAILED' "CodeQL analysis did not complete: $(($broken | ForEach-Object { "$($_.name)=$($_.conclusion)" }) -join ', ')." $checks }

    if (-not $gate) {
        $last = $jobs | Sort-Object { [datetime]$_.completed_at } | Select-Object -Last 1
        if ($last -and (Test-WithinGrace $last $Now)) { return New-GateResult 'PENDING' 'Analysis finished; waiting for the CodeQL results check.' $checks }
        return New-GateResult 'MISSING' "Analysis finished but no '$($script:Config.CodeQLGateCheck)' results check was posted." $checks
    }
    if ($gate.status -ne 'completed') { return New-GateResult 'PENDING' 'CodeQL results check is still running.' $checks }
    switch ($gate.conclusion) {
        'success' { return New-GateResult 'GREEN' 'CodeQL found no new alerts at or above the failure threshold.' $checks }
        'failure' { return New-GateResult 'RED' 'CodeQL reports new alerts at or above the failure threshold.' $checks }
        default   { return New-GateResult 'INFRA_FAILED' "CodeQL results check ended '$($gate.conclusion)' (not treated as green)." $checks }
    }
}

# READY_FOR_COPILOT only when every gate is GREEN; PENDING while any gate can still change.
function Get-OverallState([object[]]$Gates) {
    if (@($Gates | Where-Object state -eq 'PENDING').Count) { return 'PENDING' }
    if (@($Gates | Where-Object state -ne 'GREEN').Count) { return 'BLOCKED' }
    'READY_FOR_COPILOT'
}

# -Wait keeps polling while the result can still change. Right after a push GitHub may not have
# registered the analyzers' checks yet, so MISSING counts as "not started yet" during a startup grace.
function Test-KeepWaiting([string]$Overall, [object[]]$Gates, [double]$ElapsedSeconds, [double]$StartupGraceSeconds) {
    if ($Overall -eq 'PENDING') { return $true }
    if ($Overall -ne 'BLOCKED' -or $ElapsedSeconds -ge $StartupGraceSeconds) { return $false }
    # Only MISSING (no red, no failure) may still turn into a real result.
    @($Gates | Where-Object { $_.state -notin 'GREEN', 'MISSING' }).Count -eq 0
}

# ---------------------------------------------------------------------------------------------
# Data access (gh CLI for GitHub, public web API for SonarQube Cloud)

function Invoke-Gh([string[]]$Arguments) {
    $out = & gh @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) { throw "gh $($Arguments -join ' ') failed: $out" }
    $out -join "`n"
}

function Get-RepoSlug {
    (Invoke-Gh @('repo', 'view', '--json', 'nameWithOwner', '--jq', '.nameWithOwner')).Trim()
}

# PR number + head SHA + refs. With no number, the PR for the current branch.
function Get-PrInfo([string]$Repo, [int]$Pr) {
    $ghArgs = @('pr', 'view')
    if ($Pr) { $ghArgs += "$Pr" }
    $ghArgs += @('--repo', $Repo, '--json', 'number,headRefOid,headRefName,baseRefName,state,url,isDraft')
    Invoke-Gh $ghArgs | ConvertFrom-Json
}

# All items of a paginated REST list. `gh api --paginate` prints one JSON document per page, which
# is not one valid JSON value, so emit one item per line with --jq and parse each line.
function Invoke-GhList([string]$Path, [string]$Jq = '.[]') {
    $json = Invoke-Gh @('api', '--paginate', $Path, '--jq', $Jq)
    @($json -split "`n" | Where-Object { $_.Trim() } | ForEach-Object { $_ | ConvertFrom-Json })
}

function Get-CheckRuns([string]$Repo, [string]$Sha) {
    Invoke-GhList "repos/$Repo/commits/$Sha/check-runs?per_page=100" '.check_runs[]'
}

function Invoke-SonarApi([string]$Path) {
    $headers = @{}
    # Public projects need no token; a private project can use SONAR_TOKEN from the environment.
    if ($env:SONAR_TOKEN) { $headers.Authorization = "Bearer $($env:SONAR_TOKEN)" }
    Invoke-RestMethod -Uri "https://sonarcloud.io/api/$Path" -Headers $headers
}

# Every page of a Sonar search endpoint; throws rather than returning a silently truncated list.
function Get-SonarAll([string]$Path, [string]$ItemsProperty, [int]$PageSize = 500, [int]$MaxPages = 20) {
    $items = [System.Collections.Generic.List[object]]::new()
    for ($p = 1; $p -le $MaxPages; $p++) {
        $page = Invoke-SonarApi "$Path&ps=$PageSize&p=$p"
        $batch = @($page.$ItemsProperty)
        $batch | ForEach-Object { $items.Add($_) }
        $total = if ($page.PSObject.Properties['paging']) { [int]$page.paging.total } elseif ($page.PSObject.Properties['total']) { [int]$page.total } else { $items.Count }
        if ($items.Count -ge $total -or $batch.Count -eq 0) {
            if ($items.Count -lt $total) { throw "Sonar $ItemsProperty incomplete: got $($items.Count) of $total" }
            return $items.ToArray()
        }
    }
    throw "Sonar $ItemsProperty incomplete: more than $($MaxPages * $PageSize) results"
}

function Get-GateReport([string]$Repo, [int]$Pr) {
    $info = Get-PrInfo $Repo $Pr
    $runs = Get-CheckRuns $Repo $info.headRefOid
    $now = [datetime]::UtcNow
    $gates = [ordered]@{
        sonar  = Get-SonarGateState -CheckRuns $runs -Now $now
        codeql = Get-CodeQLGateState -CheckRuns $runs -Now $now
    }
    # The head can move while we read; a report for an old head is useless.
    $headNow = (Get-PrInfo $Repo $info.number).headRefOid
    $overall = if ($headNow -ne $info.headRefOid) { 'STALE' } else { Get-OverallState @($gates.Values) }
    [pscustomobject]@{
        repo    = $Repo
        pr      = $info.number
        url     = $info.url
        state   = $info.state
        head    = $info.headRefName
        base    = $info.baseRefName
        headSha = $info.headRefOid
        checkedAt = $now.ToString('o')
        gates   = [pscustomobject]$gates
        overall = $overall
    }
}

# ---------------------------------------------------------------------------------------------
# Feedback state: which findings were handled on which SHA, attempts per stage. Local to the clone
# (under the git common dir, shared by worktrees), never committed.

function Get-StatePath([int]$Pr) {
    $common = (& git rev-parse --path-format=absolute --git-common-dir).Trim()
    if ($LASTEXITCODE -ne 0) { throw 'not inside a git repository' }
    $dir = Join-Path $common 'pr-feedback'
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir | Out-Null }
    Join-Path $dir "pr-$Pr.json"
}

function Read-FeedbackState([int]$Pr) {
    $path = Get-StatePath $Pr
    if (Test-Path $path) { return Get-Content $path -Raw | ConvertFrom-Json -AsHashtable }
    @{ pr = $Pr; attempts = @{}; findings = @{} }
}

function Write-FeedbackState([hashtable]$State) {
    $State | ConvertTo-Json -Depth 10 | Set-Content -Path (Get-StatePath $State.pr) -Encoding utf8
}

# Stable identity of a finding across revisions: source, rule, file and message - not the line,
# which shifts when code above it changes.
function Get-FindingFingerprint([string]$Source, [string]$Rule, [string]$Path, [string]$Message) {
    $text = ($Source, $Rule, $Path, ($Message -replace '\s+', ' ').Trim().ToLowerInvariant()) -join '|'
    $bytes = [System.Security.Cryptography.SHA1]::HashData([System.Text.Encoding]::UTF8.GetBytes($text))
    ([System.Convert]::ToHexString($bytes)).Substring(0, 12).ToLowerInvariant()
}

Export-ModuleMember -Function Get-PrGatesConfig, Select-LatestCheck, Test-ResultIsCurrent, Get-SonarGateState,
    Get-CodeQLGateState, Get-OverallState, Test-KeepWaiting, Invoke-Gh, Invoke-GhList, Get-RepoSlug, Get-PrInfo,
    Get-CheckRuns, Invoke-SonarApi, Get-SonarAll, Get-GateReport,
    Get-StatePath, Read-FeedbackState, Write-FeedbackState, Get-FindingFingerprint
