# Differential oracle for .claude/hooks/architecture-gate.ps1 (see README.md).
# For each case: ask the guard for its verdict; run the command through the real shell inside a
# throwaway worktree with a fake git.exe first on PATH that records the exact argv/cwd/GIT_* env git
# would receive; decide from that ground truth whether a commit would skip the pre-commit hook or
# leave this clone; compare.
#   BYPASS          ground truth: violation, guard allowed        -> failure
#   EXPECT_FAIL     guard verdict differs from the case's "expect" -> failure
#   UNKNOWN         mentions commit, guard allowed, no git call recorded (e.g. git by absolute path)
#   FALSE_POSITIVE  guard blocked, ground truth clean (fine for deliberately refused shapes)
# Exit code 1 when there is any failure.
param(
    [string[]]$CasesFile = @(Get-ChildItem (Join-Path $PSScriptRoot 'cases') -Filter '*.json' | ForEach-Object FullName),
    [string]$Bash,
    [string]$GuardPath,
    [switch]$ShowAll,
    [switch]$KeepSandbox
)
$ErrorActionPreference = 'Stop'
$sep = [IO.Path]::DirectorySeparatorChar

$repoRoot = (git -C $PSScriptRoot rev-parse --show-toplevel).Trim()
if (-not $GuardPath) { $GuardPath = Join-Path $repoRoot '.claude/hooks/architecture-gate.ps1' }
$cloneRoot = ((git -C $repoRoot worktree list --porcelain | Select-Object -First 1) -replace '^worktree ', '').Trim()
$sandbox = Join-Path $repoRoot '.claude/worktrees/guard-oracle-sandbox'
if (-not $Bash) {
    $gitExe = (Get-Command git -CommandType Application | Select-Object -First 1).Source
    $Bash = if ($IsWindows) { Join-Path (Split-Path (Split-Path $gitExe)) 'bin\bash.exe' } else { (Get-Command bash).Source }
}

# Fake git: build on first run or when its source changed.
$shimProject = Join-Path $PSScriptRoot 'shim'
$shim = Join-Path $shimProject 'bin/publish'
$shimExe = Join-Path $shim ($IsWindows ? 'git.exe' : 'git')
if (-not (Test-Path $shimExe) -or (Get-Item (Join-Path $shimProject 'Program.cs')).LastWriteTime -gt (Get-Item $shimExe).LastWriteTime) {
    dotnet publish $shimProject -c Release -o $shim --nologo -v q | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'building the fake git failed' }
}
$shimBash = if ($IsWindows) { '/' + $shim.Substring(0, 1).ToLower() + ($shim.Substring(2) -replace '\\', '/') } else { $shim }

if (-not (Test-Path (Join-Path $sandbox '.git'))) {
    git -C $repoRoot worktree add -q --detach $sandbox HEAD
    if ($LASTEXITCODE -ne 0) { throw "could not create sandbox worktree $sandbox" }
}
$sandboxBash = if ($IsWindows) { '/' + $sandbox.Substring(0, 1).ToLower() + ($sandbox.Substring(2) -replace '\\', '/') } else { $sandbox }

$roots = @(git -C $sandbox worktree list --porcelain | Where-Object { $_ -like 'worktree *' } |
    ForEach-Object { [IO.Path]::GetFullPath($_.Substring(9)).TrimEnd('\', '/') + $sep })
$aliases = @{}
git -C $sandbox config --get-regexp '^alias\.' 2>$null | ForEach-Object { $k, $v = $_ -split ' ', 2; $aliases[$k.Substring(6)] = $v }

$commitLong = @('quiet', 'verbose', 'file', 'author', 'date', 'message', 'reedit-message', 'reuse-message', 'fixup', 'squash',
    'reset-author', 'trailer', 'signoff', 'template', 'edit', 'cleanup', 'status', 'gpg-sign', 'no-verify', 'verify', 'dry-run',
    'short', 'branch', 'ahead-behind', 'porcelain', 'long', 'null', 'amend', 'post-rewrite', 'all', 'include', 'interactive',
    'patch', 'only', 'untracked-files', 'pathspec-from-file', 'pathspec-file-nul', 'allow-empty', 'allow-empty-message')
$commitLongWithValue = @('file', 'author', 'date', 'message', 'reedit-message', 'reuse-message', 'fixup', 'squash', 'trailer',
    'template', 'cleanup', 'pathspec-from-file')

function Resolve-Long([string]$name) {
    # git accepts unique prefixes of long options, and --no-<bool>.
    if ($commitLong -contains $name) { return $name }
    $hits = @($commitLong | Where-Object { $_.StartsWith($name) })
    if ($name.StartsWith('no-')) { $hits += @($commitLong | Where-Object { ('no-' + $_).StartsWith($name) } | ForEach-Object { "no-$_" }) }
    $hits = @($hits | Select-Object -Unique)
    if ($hits.Count -eq 1) { return $hits[0] }
    return $null   # ambiguous or unknown: git errors out, no commit
}

# Ground truth for one recorded git call: reasons it would skip the hook or leave the clone.
function Get-Violations($record) {
    $v = @()
    $gitArgs = @($record.args)
    $dir = $record.cwd
    $i = 0
    while ($i -lt $gitArgs.Count -and $gitArgs[$i].StartsWith('-')) {
        $a = $gitArgs[$i]
        $name, $inline = $a -split '=', 2
        switch -CaseSensitive ($name) {
            '-C' { $dir = [IO.Path]::GetFullPath([IO.Path]::Combine($dir, $gitArgs[$i + 1])); $i += 2; continue }
            '-c' { $kv = $gitArgs[$i + 1]; if (($kv -split '=', 2)[0] -match '(?i)^(core\.hookspath|include\.|includeif\.)') { $v += "config override -c $kv" }; $i += 2; continue }
            '--config-env' {
                $kv = if ($null -ne $inline) { $inline } else { $i++; $gitArgs[$i] }
                if (($kv -split '=', 2)[0] -match '(?i)^(core\.hookspath|include\.|includeif\.)') { $v += "config override --config-env $kv" }
                $i++; continue
            }
            { $_ -in '--git-dir', '--work-tree' } { $v += "repository override $a"; if ($null -eq $inline) { $i++ }; $i++; continue }
            { $_ -in '--namespace', '--super-prefix' } { if ($null -eq $inline) { $i++ }; $i++; continue }
            default { $i++ }
        }
    }
    if ($i -ge $gitArgs.Count) { return @() }
    $sub = $gitArgs[$i]
    $rest = @($gitArgs | Select-Object -Skip ($i + 1))
    if ($aliases.ContainsKey($sub)) {
        $expansion = $aliases[$sub]
        if ($expansion.StartsWith('!')) { return @("shell alias '$sub' (unverifiable)") }
        $parts = @($expansion -split '\s+' | Where-Object { $_ })
        $sub = $parts[0]; $rest = @($parts | Select-Object -Skip 1) + $rest
    }
    if ($sub -ne 'commit') { return @() }

    foreach ($key in $record.env.PSObject.Properties.Name) {
        $value = [string]$record.env.$key
        if ($key -match '(?i)^GIT_(DIR|WORK_TREE|COMMON_DIR)$') { $v += "environment override $key" }
        elseif ($key -match '(?i)^GIT_CONFIG_(GLOBAL|SYSTEM)$|^GIT_CONFIG$') { $v += "config file override $key=$value" }
        elseif ($key -match '(?i)^GIT_CONFIG_KEY_\d+$' -and $value -match '(?i)^(core\.hookspath|include\.|includeif\.)') { $v += "config override $key=$value" }
        elseif ($key -match '(?i)^GIT_CONFIG_PARAMETERS$' -and $value -match '(?i)hookspath|include') { $v += "config override $key" }
    }

    $expectValue = $false
    foreach ($a in $rest) {
        if ($expectValue) { $expectValue = $false; continue }
        if ($a -eq '--') { break }
        if ($a.StartsWith('--')) {
            $name, $inline = $a.Substring(2) -split '=', 2
            $long = Resolve-Long $name
            if ($long -eq 'no-verify') { $v += "skips hooks ($a)" }
            if ($long -in $commitLongWithValue -and $null -eq $inline) { $expectValue = $true }
            continue
        }
        if ($a -cmatch '^-.') {
            for ($k = 1; $k -lt $a.Length; $k++) {
                $c = [string]$a[$k]
                if ($c -ceq 'n') { $v += "skips hooks ($a)"; break }
                if ($c -cin 'm', 'F', 'C', 'c', 't') { $expectValue = $k -eq $a.Length - 1; break }
                if ($c -cin 'S', 'u') { break }
                # Unknown switch: git exits with an error before committing.
                if ($c -cnotin 'q', 'v', 's', 'e', 'a', 'i', 'p', 'o', 'z', 'h') { return @() }
            }
        }
    }

    $full = $dir.TrimEnd('\', '/') + $sep
    if (-not ($roots | Where-Object { $full.StartsWith($_, [StringComparison]::OrdinalIgnoreCase) })) { $v += "commits outside this clone ($dir)" }
    return $v
}

function Invoke-Truth([string]$command, [string]$tool) {
    $log = [IO.Path]::GetTempFileName()
    $psi = [Diagnostics.ProcessStartInfo]::new()
    if ($tool -eq 'PowerShell') {
        $psi.FileName = 'pwsh'
        foreach ($a in '-NoProfile', '-NonInteractive', '-Command') { $psi.ArgumentList.Add($a) }
        $psi.ArgumentList.Add("`$env:PATH = '$shim' + [IO.Path]::PathSeparator + `$env:PATH`n$command")
    }
    else {
        $psi.FileName = $Bash
        $psi.ArgumentList.Add('-c')
        # Git's bash.exe puts its own git first on PATH; re-prepend the fake one inside the shell.
        $psi.ArgumentList.Add("PATH='$shimBash':`$PATH; hash -r`n$command")
    }
    $psi.WorkingDirectory = $sandbox
    $psi.RedirectStandardInput = $true; $psi.RedirectStandardOutput = $true; $psi.RedirectStandardError = $true
    $psi.Environment['PATH'] = $shim + [IO.Path]::PathSeparator + $env:PATH
    $psi.Environment['ORACLE_LOG'] = $log
    foreach ($k in @($psi.Environment.Keys)) { if ($k -match '^GIT_') { $psi.Environment.Remove($k) | Out-Null } }
    $p = [Diagnostics.Process]::Start($psi)
    $p.StandardInput.Close()
    $stderr = $p.StandardError.ReadToEndAsync(); $null = $p.StandardOutput.ReadToEndAsync()
    $timedOut = -not $p.WaitForExit(15000)
    if ($timedOut) { $p.Kill($true) }
    $records = @(Get-Content $log -ErrorAction SilentlyContinue | Where-Object { $_ } | ForEach-Object { $_ | ConvertFrom-Json })
    Remove-Item $log -ErrorAction SilentlyContinue
    [pscustomobject]@{ Records = $records; TimedOut = $timedOut; Stderr = ($stderr.Result -split "`n" | Select-Object -First 2) -join ' | ' }
}

function Invoke-Guard([string]$command, [string]$tool) {
    $json = @{ cwd = $sandbox; hook_event_name = 'PreToolUse'; tool_name = $tool; tool_input = @{ command = $command } } | ConvertTo-Json -Compress
    $out = $json | pwsh -NoProfile -File $GuardPath 2>&1
    [pscustomobject]@{ Exit = $LASTEXITCODE; Reason = (($out | Select-String 'BLOCKED') -join ' ') }
}

function Expand-Placeholders([string]$command) {
    $command.Replace('{SANDBOX_BASH}', $sandboxBash).Replace('{SANDBOX}', ($sandbox -replace '\\', '/')).
        Replace('{CLONE_WIN}', ($cloneRoot -replace '/', '\')).Replace('{CLONE}', ($cloneRoot -replace '\\', '/'))
}

$failures = 0
try {
    # Self-check: the fake git must intercept plain `git` in both shells, or results are meaningless (and unsafe).
    foreach ($tool in 'Bash', 'PowerShell') {
        $probe = Invoke-Truth 'git oracle-probe' $tool
        if (-not ($probe.Records | Where-Object { $_.args -contains 'oracle-probe' })) {
            throw "fake git did not intercept 'git' under $tool; aborting. $($probe.Stderr)"
        }
    }

    foreach ($file in $CasesFile) {
        $counts = [ordered]@{ OK = 0; BYPASS = 0; EXPECT_FAIL = 0; UNKNOWN = 0; FALSE_POSITIVE = 0 }
        $cases = @(Get-Content $file -Raw | ConvertFrom-Json)
        foreach ($case in $cases) {
            $tool = if ($case.tool) { $case.tool } else { 'Bash' }
            $command = Expand-Placeholders $case.cmd
            $verdict = Invoke-Guard $command $tool
            $truth = Invoke-Truth $command $tool
            $violations = @($truth.Records | ForEach-Object { Get-Violations $_ })
            $allowed = $verdict.Exit -eq 0
            $class = if ($violations.Count -and $allowed) { 'BYPASS' }
            elseif ($case.expect -and $case.expect -ne ($allowed ? 'allow' : 'block')) { 'EXPECT_FAIL' }
            elseif (-not $case.expect -and $truth.Records.Count -eq 0 -and $command -match 'commit' -and $allowed) { 'UNKNOWN' }
            elseif (-not $violations.Count -and -not $allowed) { 'FALSE_POSITIVE' }
            else { 'OK' }
            $counts[$class]++
            if ($class -in 'BYPASS', 'EXPECT_FAIL') { $failures++ }
            if ($ShowAll -or $class -in 'BYPASS', 'EXPECT_FAIL', 'UNKNOWN') {
                "[$class] ($tool) guard=$($allowed ? 'allow' : 'block')$(if ($case.expect) { " expect=$($case.expect)" }) | $($case.cmd -replace "`r?`n", '\n')"
                if ($violations.Count) { "      truth: $($violations -join '; ')" }
                elseif ($truth.Records.Count) { "      truth: git called $($truth.Records.Count)x, no violation" }
                else { "      truth: no git call recorded$(if ($truth.TimedOut) { ' (timed out)' }) $($truth.Stderr)" }
                if ($verdict.Reason) { "      guard: $($verdict.Reason)" }
            }
        }
        "SUMMARY $(Split-Path $file -Leaf): cases=$($cases.Count) $(($counts.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join ' ')"
    }
}
finally {
    if (-not $KeepSandbox) { git -C $repoRoot worktree remove --force $sandbox 2>$null | Out-Null }
}
if ($failures) { "FAILED: $failures case(s)"; exit 1 }
'PASSED'
