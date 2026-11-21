<#
.SYNOPSIS
    Writes a short Markdown summary of one Stryker.NET JSON report (ADR 0022).

.DESCRIPTION
    Prints the project's mutation score and every mutant the tests did not detect (Survived or NoCoverage),
    with its file, line, mutator and source line. Mutants Stryker did not test or could not judge (Ignored,
    CompileError, RuntimeError) are only counted; like Stryker, the score leaves them out. Runtime errors mean the
    test host crashed, so the summary warns about them: the score then rests on fewer mutants than it seems. The mutation workflow appends the output to the job summary; locally it reads like a to-do list.

    Score as Stryker computes it: detected (Killed + Timeout) / (detected + undetected (Survived + NoCoverage)).

    One kind of undetected mutant is counted but not listed: ConfigureAwait(false) turned into ConfigureAwait(true).
    No test can tell the two apart, so the list would only bury the real findings. The config does not ignore
    ConfigureAwait instead, because Stryker's ignore-methods also drops mutants of the expression around the call,
    such as the negated condition of `if (await x.ConfigureAwait(false))` (ADR 0022).

.PARAMETER ReportPath
    Path of Stryker's reports/mutation-report.json.

.PARAMETER Project
    Name shown in the heading, for example HT.Domain.

.PARAMETER MaxRows
    Most undetected mutants listed; the rest are counted. Keeps the job summary well below its size limit.

.EXAMPLE
    pwsh tools/mutation/Write-MutationSummary.ps1 -ReportPath StrykerOutput/HT.Domain/reports/mutation-report.json -Project HT.Domain
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $ReportPath,
    [Parameter(Mandatory)] [string] $Project,
    [int] $MaxRows = 100
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $ReportPath)) {
    "### $Project"
    ''
    "No report: Stryker found nothing to mutate, or failed before writing one."
    ''
    return
}

$report = Get-Content -LiteralPath $ReportPath -Raw | ConvertFrom-Json

$counts = @{}
$undetected = [System.Collections.Generic.List[object]]::new()
$configureAwait = 0

# True when the mutant turns the `false` of ConfigureAwait(false) into `true`. Columns are 1-based, end exclusive.
function Test-ConfigureAwaitFlip($mutant, [string[]] $lines) {
    $start = $mutant.location.start
    $end = $mutant.location.end
    if ($mutant.mutatorName -ne 'Boolean mutation' -or $start.line -ne $end.line) { return $false }
    $text = $lines[$start.line - 1]
    if ($end.column - 1 -gt $text.Length) { return $false }
    $span = $text.Substring($start.column - 1, $end.column - $start.column)
    $before = $text.Substring(0, $start.column - 1)
    return $span -eq 'false' -and $before -match 'ConfigureAwait\(\s*(continueOnCapturedContext\s*:\s*)?$'
}

foreach ($file in $report.files.PSObject.Properties) {
    # Report keys are absolute paths on the machine that ran Stryker; show them from src/ on.
    $relative = if ($file.Name -match '(src[\\/].*)$') { $Matches[1] -replace '\\', '/' } else { $file.Name -replace '\\', '/' }
    $lines = $file.Value.source -split "`r?`n"

    foreach ($mutant in $file.Value.mutants) {
        $counts[$mutant.status] = 1 + ($counts[$mutant.status] ?? 0)
        if ($mutant.status -in 'Survived', 'NoCoverage') {
            if (Test-ConfigureAwaitFlip $mutant $lines) { $configureAwait++; continue }
            $line = [int]$mutant.location.start.line
            $code = $lines[$line - 1].Trim()
            if ($code.Length -gt 100) { $code = $code.Substring(0, 100) + '…' }
            $undetected.Add([pscustomobject]@{
                File     = $relative
                Line     = $line
                Location = "${relative}:$line"
                Status   = $mutant.status
                Mutator  = $mutant.mutatorName
                Code     = $code
            })
        }
    }
}

$detected = ($counts['Killed'] ?? 0) + ($counts['Timeout'] ?? 0)
$missed = ($counts['Survived'] ?? 0) + ($counts['NoCoverage'] ?? 0)
$score = if ($detected + $missed -gt 0) {
    [string]::Format([cultureinfo]::InvariantCulture, '{0:0.00}%', 100.0 * $detected / ($detected + $missed))
} else { 'N/A' }

"### $Project — mutation score $score"
''
"Killed $($counts['Killed'] ?? 0) · Timeout $($counts['Timeout'] ?? 0) · Survived $($counts['Survived'] ?? 0) · " +
"No coverage $($counts['NoCoverage'] ?? 0) · Ignored $($counts['Ignored'] ?? 0) · Compile errors $($counts['CompileError'] ?? 0) · " +
"Runtime errors $($counts['RuntimeError'] ?? 0)"
''
if (($counts['RuntimeError'] ?? 0) -gt 0) {
    "**Warning:** $($counts['RuntimeError']) mutants crashed the test host (runtime errors). They are not in the " +
    'score, so it rests on fewer mutants than were created; see the job log.'
    ''
}
if ($configureAwait -gt 0) {
    "Not listed: $configureAwait undetected ``ConfigureAwait(false)`` → ``true`` mutants, which no test can detect. " +
    'They are in the score.'
    ''
}

if ($detected + $missed -eq 0) {
    'No mutant was tested: the change touches no mutable code of this project, or none of its tests.'
    ''
    return
}

if ($undetected.Count -eq 0) {
    'Every other tested mutant was detected.'
    ''
    return
}

'| Location | Status | Mutator | Source line |'
'| --- | --- | --- | --- |'
foreach ($row in ($undetected | Sort-Object File, Line | Select-Object -First $MaxRows)) {
    $code = $row.Code -replace '\|', '\|' -replace '`', "'"
    "| ``$($row.Location)`` | $($row.Status) | $($row.Mutator) | ``$code`` |"
}
if ($undetected.Count -gt $MaxRows) {
    ''
    "…and $($undetected.Count - $MaxRows) more; see the HTML report in the run's artifacts."
}
''
