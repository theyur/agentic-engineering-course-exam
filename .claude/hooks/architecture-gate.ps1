# Claude Code PreToolUse hook: reject Bash git commit attempts when ArchUnitNET fails.
# Input is the JSON tool event supplied by Claude Code on stdin.
# TEMP: invocation marker for testing the settings.json "if" condition; remove after testing.
Add-Content -Path (Join-Path $env:TEMP 'architecture-gate.log') -Value "$(Get-Date -Format o) architecture-gate invoked"
$ErrorActionPreference = 'Stop'

try {
    $inputJson = [Console]::In.ReadToEnd() | ConvertFrom-Json
    $command = [string]$inputJson.tool_input.command

    # Avoid expensive tests on unrelated Bash invocations.
    if ($command -notmatch '(?i)(?:^|[;&|]\s*|\s)git(?:\.exe)?\s+commit\b') {
        exit 0
    }

    $projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
    $testProject = Join-Path $projectRoot 'tests/HT.ArchitectureTests/HT.ArchitectureTests.csproj'

    [Console]::Error.WriteLine('Architecture gate: running ArchUnitNET tests before git commit...')
    Push-Location $projectRoot
    try {
        & dotnet test $testProject --configuration Debug --nologo --verbosity minimal 2>&1 | ForEach-Object { [Console]::Error.WriteLine($_) }
        if ($LASTEXITCODE -ne 0) {
            [Console]::Error.WriteLine('BLOCKED: architectural tests failed. Fix the dependency before committing; do not bypass or disable the gate.')
            exit 2
        }
    }
    finally {
        Pop-Location
    }
}
catch {
    [Console]::Error.WriteLine("BLOCKED: architecture gate could not run: $($_.Exception.Message)")
    exit 2
}

exit 0
