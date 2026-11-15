# Self-test for tools/hosting: the parts of provisioning that run without Azure or GitHub.
# Offline: no az or gh call. Prints PASSED, or FAILED plus one line per broken case (exit 1).
#   pwsh -NoProfile -File tools/hosting/selftest.ps1
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$failures = [System.Collections.Generic.List[string]]::new()
$script:caseCount = 0

# Every assertion goes through Check, which counts it. The test returns $true to pass; anything else, or a
# thrown error, is a failure. Only booleans are compared, so no generated value is ever printed.
function Check([string]$Case, [scriptblock]$Test) {
    $script:caseCount++
    try {
        $result = & $Test
        if ($result -isnot [bool] -or -not $result) { $failures.Add("$Case : expected True") }
    }
    catch {
        $failures.Add("$Case : threw $($_.Exception.Message)")
    }
}

$functions = Join-Path $PSScriptRoot 'HostingFunctions.ps1'
try {
    . $functions
}
catch {
    'FAILED'
    "  HostingFunctions.ps1 could not be loaded: $($_.Exception.Message)"
    exit 1
}

# --- Test-DemoAdministratorEmail (HP-014) ----------------------------------------------------
$accepted = @('admin@example.com', 'Admin@Example.com')
$refused = @('admin@example.org', 'admin@sub.example.com', 'admin@example.com.evil', 'a@b@example.com', '', '   ',
    '@example.com', 'admin example@example.com', 'admin@example.com ')
foreach ($e in $accepted) { Check "email accepted: '$e'" { (Test-DemoAdministratorEmail $e) -eq $true } }
foreach ($e in $refused) { Check "email refused: '$e'" { (Test-DemoAdministratorEmail $e) -eq $false } }
Check 'email refused: null' { (Test-DemoAdministratorEmail $null) -eq $false }

# --- Invoke-WithSecretFile -------------------------------------------------------------------
$sentinel = 'sentinel-secret-1 "quoted" \ back'
$script:seenPath = $null
$script:seenContent = $null
$returned = Invoke-WithSecretFile $sentinel {
    param($Path)
    $script:seenPath = $Path
    $script:seenContent = [IO.File]::ReadAllText($Path)
    'action-result'
}
Check 'secret file: path given to the action' { $null -ne $script:seenPath }
Check 'secret file: in the temporary folder' {
    [IO.Path]::GetDirectoryName($script:seenPath).TrimEnd([IO.Path]::DirectorySeparatorChar) -eq
    [IO.Path]::GetTempPath().TrimEnd([IO.Path]::DirectorySeparatorChar)
}
Check 'secret file: holds exactly the value (no trailing newline)' { $script:seenContent -ceq $sentinel }
Check 'secret file: action output returned' { $returned -eq 'action-result' }
Check 'secret file: gone after a normal return' { -not (Test-Path -LiteralPath $script:seenPath) }

$script:thrownPath = $null
$script:caught = $false
try {
    Invoke-WithSecretFile 'sentinel-secret-2' {
        param($Path)
        $script:thrownPath = $Path
        throw 'boom'
    }
}
catch {
    $script:caught = $_.Exception.Message -eq 'boom'
}
Check 'secret file: the action''s error propagates' { $script:caught }
Check 'secret file: gone after the action threw' {
    ($null -ne $script:thrownPath) -and -not (Test-Path -LiteralPath $script:thrownPath)
}

# --- Test-PowerShellParses -------------------------------------------------------------------
$broken = Join-Path ([IO.Path]::GetTempPath()) "ht-selftest-$([guid]::NewGuid().ToString('N'))-broken.ps1"
$valid = Join-Path ([IO.Path]::GetTempPath()) "ht-selftest-$([guid]::NewGuid().ToString('N'))-valid.ps1"
try {
    Set-Content -LiteralPath $broken -Value "function Broken {`n    if (`$true {`n" -NoNewline
    Set-Content -LiteralPath $valid -Value "function Fine { param([int]`$x) `$x + 1 }`n" -NoNewline
    Check 'parses: syntax error is false' { (Test-PowerShellParses $broken) -eq $false }
    Check 'parses: valid file is true' { (Test-PowerShellParses $valid) -eq $true }
}
finally {
    Remove-Item -LiteralPath $broken, $valid -Force -ErrorAction SilentlyContinue
}
Check 'parses: missing file is false' { (Test-PowerShellParses (Join-Path $PSScriptRoot 'no-such-file.ps1')) -eq $false }
foreach ($name in 'provision.ps1', 'teardown.ps1', 'HostingFunctions.ps1') {
    $path = Join-Path $PSScriptRoot $name
    Check "parses: $name" { (Test-Path -LiteralPath $path) -and (Test-PowerShellParses $path) }
}

# --- New-SqlAdministratorPassword ------------------------------------------------------------
$passwords = @(1..20 | ForEach-Object { New-SqlAdministratorPassword })
Check 'sql password: 32 letters and digits' { @($passwords | Where-Object { $_ -cnotmatch '^[A-Za-z0-9]{32}$' }).Count -eq 0 }
Check 'sql password: upper, lower and digit in each' {
    @($passwords | Where-Object { $_ -cnotmatch '[A-Z]' -or $_ -cnotmatch '[a-z]' -or $_ -notmatch '[0-9]' }).Count -eq 0
}
Check 'sql password: no repeats' { @($passwords | Sort-Object -Unique -CaseSensitive).Count -eq $passwords.Count }

# --- New-SqlConnectionString -----------------------------------------------------------------
$cs = New-SqlConnectionString -ServerFqdn 'srv.database.windows.net' -Database 'ht-demo' -User 'htsqladmin' -Password 'pw123ABC'
Check 'connection string: server and port' { $cs.Contains('Server=tcp:srv.database.windows.net,1433;') }
Check 'connection string: database' { $cs.Contains('Initial Catalog=ht-demo;') }
Check 'connection string: user and password' { $cs.Contains('User ID=htsqladmin;') -and $cs.Contains('Password=pw123ABC;') }
Check 'connection string: encrypted' { $cs.Contains('Encrypt=True;') -and $cs.Contains('TrustServerCertificate=False;') }

# --- ConvertTo-AppSettingsJson ---------------------------------------------------------------
$json = ConvertTo-AppSettingsJson ([ordered]@{ 'Operator__DemoPassword' = 'p"w\1'; 'Operator__AdministratorEmail' = 'a@example.com' })
$parsed = @($json | ConvertFrom-Json)
Check 'app settings json: a list of name/value pairs' { $parsed.Count -eq 2 -and $parsed[0].name -eq 'Operator__DemoPassword' }
Check 'app settings json: value survives escaping' { $parsed[0].value -ceq 'p"w\1' }
Check 'app settings json: not a slot setting' { $parsed[0].slotSetting -eq $false }
$single = @((ConvertTo-AppSettingsJson ([ordered]@{ 'Operator__DemoPassword' = 'x' })) | ConvertFrom-Json)
Check 'app settings json: one setting is still a list' { $single.Count -eq 1 -and $single[0].name -eq 'Operator__DemoPassword' }

# --- Get-IPv4AddressList ---------------------------------------------------------------------
$ips = @(Get-IPv4AddressList '20.1.2.3,20.1.2.4, 20.1.2.3,2603:1030::1,not-an-ip,')
Check 'outbound addresses: IPv4 only, distinct, in order' { ($ips -join ',') -eq '20.1.2.3,20.1.2.4' }
Check 'outbound addresses: empty input' { @(Get-IPv4AddressList '').Count -eq 0 }

# --- Invoke-NativeTool -----------------------------------------------------------------------
$pwsh = (Get-Process -Id $PID).Path
Check 'native tool: output returned' { (Invoke-NativeTool $pwsh @('-NoProfile', '-Command', "'ok'")) -eq 'ok' }
$script:nativeMessage = ''
try { Invoke-NativeTool $pwsh @('-NoProfile', '-Command', 'exit 3') } catch { $script:nativeMessage = $_.Exception.Message }
Check 'native tool: non-zero exit throws with the exit code' { $script:nativeMessage -match 'exit code 3' }
Check 'native tool: message names no option' { $script:nativeMessage -notmatch '-Command' }

if ($failures.Count) {
    'FAILED'
    $failures | ForEach-Object { "  $_" }
    exit 1
}
"PASSED ($script:caseCount cases)"
