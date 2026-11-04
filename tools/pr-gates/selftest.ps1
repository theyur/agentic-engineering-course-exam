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
)
foreach ($c in $codeql) { Expect $c.n (Get-CodeQLGateState -CheckRuns $c.runs -Now $now).state $c.e }

# --- Overall ---------------------------------------------------------------------------------
function G([string]$s) { [pscustomobject]@{ state = $s } }
Expect 'overall both green'       (Get-OverallState @((G GREEN), (G GREEN)))        'READY_FOR_COPILOT'
Expect 'overall one pending'      (Get-OverallState @((G RED), (G PENDING)))        'PENDING'
Expect 'overall red + green'      (Get-OverallState @((G RED), (G GREEN)))          'BLOCKED'
Expect 'overall skipped + green'  (Get-OverallState @((G SKIPPED), (G GREEN)))      'BLOCKED'
Expect 'overall missing + green'  (Get-OverallState @((G GREEN), (G MISSING)))      'BLOCKED'

# --- Fingerprints ----------------------------------------------------------------------------
$a = Get-FindingFingerprint 'sonar' 'csharpsquid:S1481' 'src/A.cs' 'Remove the unused  local variable'
$b = Get-FindingFingerprint 'sonar' 'csharpsquid:S1481' 'src/A.cs' 'remove the unused local variable '
$c2 = Get-FindingFingerprint 'sonar' 'csharpsquid:S1481' 'src/B.cs' 'Remove the unused local variable'
Expect 'fingerprint ignores case/whitespace' ($a -eq $b) 'True'
Expect 'fingerprint differs by file' ($a -eq $c2) 'False'

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

$total = $sonar.Count + $codeql.Count + 5 + 2 + $hookCases.Count
if ($failures.Count) {
    'FAILED'
    $failures | ForEach-Object { "  $_" }
    exit 1
}
"PASSED ($total cases)"
