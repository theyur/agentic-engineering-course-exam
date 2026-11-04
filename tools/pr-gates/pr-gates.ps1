<#
.SYNOPSIS
  Static-analysis gate status (SonarQube Cloud + CodeQL) for a PR's current head SHA.

.DESCRIPTION
  Reads the check runs GitHub has for the PR head commit and classifies each gate as
  GREEN / RED / PENDING / MISSING / SKIPPED / BUILD_FAILED / INFRA_FAILED (see PrGates.psm1).
  Overall: READY_FOR_COPILOT (both GREEN), BLOCKED, PENDING, or STALE (head moved while reading).

  Both analyzers scan the PR merge commit (refs/pull/N/merge), but GitHub attaches their check runs
  to the PR head SHA, so the head SHA is the key here.

  With -Wait it polls (bounded) until no gate is PENDING. Run it in the background from Claude Code:
  the harness wakes the session when the process exits. Nothing wakes a session that has ended.

.EXAMPLE
  pwsh -NoProfile -File tools/pr-gates/pr-gates.ps1 -Pr 12 -Wait

.OUTPUTS
  JSON report on stdout (or a one-line-per-gate summary with -Summary).
  Exit codes: 0 READY_FOR_COPILOT, 10 BLOCKED, 20 PENDING (timed out), 30 STALE, 1 error.
#>
param(
    [int]$Pr,                         # default: the PR for the current branch
    [string]$Repo,                    # default: the repo of the current checkout
    [switch]$Wait,
    [int]$TimeoutMinutes = 25,
    [int]$IntervalSeconds = 30,
    [switch]$Summary
)
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'PrGates.psm1') -Force

try {
    if (-not $Repo) { $Repo = Get-RepoSlug }
    $deadline = [datetime]::UtcNow.AddMinutes($TimeoutMinutes)
    $firstSha = $null
    while ($true) {
        $report = Get-GateReport $Repo $Pr
        if (-not $Pr) { $Pr = $report.pr }
        if (-not $firstSha) { $firstSha = $report.headSha }
        # A push during the wait means the old revision no longer matters: report STALE, don't wait on.
        if ($report.headSha -ne $firstSha) { $report.overall = 'STALE' }
        if (-not $Wait -or $report.overall -ne 'PENDING' -or [datetime]::UtcNow -ge $deadline) { break }
        Start-Sleep -Seconds $IntervalSeconds
    }
} catch {
    [Console]::Error.WriteLine("pr-gates: $($_.Exception.Message)")
    exit 1
}

if ($Summary) {
    "PR #$($report.pr) $($report.head) @ $($report.headSha.Substring(0,7)) -> $($report.overall)"
    foreach ($g in $report.gates.PSObject.Properties) { "  {0,-7} {1,-13} {2}" -f $g.Name, $g.Value.state, $g.Value.reason }
} else {
    $report | ConvertTo-Json -Depth 6
}
switch ($report.overall) {
    'READY_FOR_COPILOT' { exit 0 }
    'BLOCKED' { exit 10 }
    'PENDING' { exit 20 }
    'STALE' { exit 30 }
}
exit 1
