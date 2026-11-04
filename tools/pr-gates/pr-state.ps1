<#
.SYNOPSIS
  Local feedback ledger for /process-pr-feedback: attempts per stage and decisions per finding.

.DESCRIPTION
  Stored in <git common dir>/pr-feedback/pr-<N>.json - shared by worktrees, never committed.

  show     print the ledger
  attempt  record that a fix round starts from -Sha for -Stage. Idempotent per SHA. Exits 3 when
           the stage already used -Max (default 2) rounds: stop and escalate to the user.
  decide   record a decision for finding -Id on -Sha (fixed, false-positive, duplicate, stale,
           wont-fix, needs-user) with an optional -Note.
  reset    clear the attempt counter of -Stage (only after the user said to continue).

.EXAMPLE
  pwsh -NoProfile -File tools/pr-gates/pr-state.ps1 attempt -Pr 12 -Stage static -Sha abc1234
#>
param(
    [Parameter(Mandatory, Position = 0)][ValidateSet('show', 'attempt', 'decide', 'reset')][string]$Action,
    [Parameter(Mandatory)][int]$Pr,
    [ValidateSet('static', 'copilot')][string]$Stage,
    [string]$Sha,
    [string]$Id,
    [ValidateSet('fixed', 'false-positive', 'duplicate', 'stale', 'wont-fix', 'needs-user')][string]$Decision,
    [string]$Note,
    [int]$Max = 2
)
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'PrGates.psm1') -Force

$state = Read-FeedbackState $Pr
function Need([string[]]$Names) {
    foreach ($n in $Names) { if (-not (Get-Variable $n -ValueOnly)) { throw "-$n is required for '$Action'" } }
}

switch ($Action) {
    'show' { $state | ConvertTo-Json -Depth 10; exit 0 }
    'attempt' {
        Need 'Stage', 'Sha'
        $list = [System.Collections.Generic.List[string]]@($(if ($state.attempts.ContainsKey($Stage)) { $state.attempts[$Stage] } else { @() }))
        if ($list.Contains($Sha)) { "attempt $($list.IndexOf($Sha) + 1)/$Max for $Stage already recorded on $Sha"; exit 0 }
        if ($list.Count -ge $Max) {
            [Console]::Error.WriteLine("LIMIT: $Stage stage already used $($list.Count)/$Max fix rounds on PR #$Pr ($($list -join ', ')). Escalate to the user.")
            exit 3
        }
        $list.Add($Sha)
        $state.attempts[$Stage] = @($list)
        Write-FeedbackState $state
        "attempt $($list.Count)/$Max for $Stage recorded on $Sha"
    }
    'decide' {
        Need 'Sha', 'Id', 'Decision'
        $hist = @($(if ($state.findings.ContainsKey($Id)) { $state.findings[$Id] } else { @() }))
        $hist += @{ sha = $Sha; decision = $Decision; note = $Note; at = [datetime]::UtcNow.ToString('o') }
        $state.findings[$Id] = $hist
        Write-FeedbackState $state
        "recorded $Decision for $Id on $Sha"
    }
    'reset' {
        Need 'Stage'
        $state.attempts[$Stage] = @()
        Write-FeedbackState $state
        "reset $Stage attempts on PR #$Pr"
    }
}
