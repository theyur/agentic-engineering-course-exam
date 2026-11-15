# Creates or updates the hosted demo (ADR 0015; research R-02, R-03, R-07, R-08, R-09;
# specs/003-demo-hosting/contracts/hosted-configuration.md). Run by the Operator after `az login` and
# `gh auth login`, once, again after a change, and with -RotateSqlPassword to rotate the SQL password.
# Idempotent: each resource is shown before it is created, and every setting is set again on every run.
#
#   pwsh -NoProfile -File tools/hosting/provision.ps1 -Subscription <id or name> -Location <region> `
#       -ResourceGroup <rg> -PlanName <plan> -WebAppName <app> -SqlServerName <server> `
#       -SqlDatabaseName <db> -Repository <owner/name> [-RotateSqlPassword]
#
# Secrets: the administrator email and the two passwords are read with masked prompts; the SQL password is
# generated here. Each reaches Azure only through an `@<file>` argument written by Invoke-WithSecretFile, never
# as a command-line argument, and this script prints none of them (nor the administrator email). It prints step
# names and non-secret identifiers only.
#
# The Azure CLI runs as az.cmd on Windows, where cmd.exe re-parses the arguments: an argument without spaces
# that holds | & < > ^ or parentheses would break the command. So no argument here holds those characters;
# lists are fetched as plain name lists (`--query [].name`) and filtered in PowerShell.
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Subscription,
    [Parameter(Mandatory)][string]$Location,
    [Parameter(Mandatory)][ValidatePattern('^[A-Za-z0-9._-]+$')][string]$ResourceGroup,
    [Parameter(Mandatory)][ValidatePattern('^[A-Za-z0-9-]+$')][string]$PlanName,
    [Parameter(Mandatory)][ValidatePattern('^[A-Za-z0-9-]+$')][string]$WebAppName,
    [Parameter(Mandatory)][ValidatePattern('^[a-z0-9-]+$')][string]$SqlServerName,
    [Parameter(Mandatory)][ValidatePattern('^[A-Za-z0-9_-]+$')][string]$SqlDatabaseName,
    [Parameter(Mandatory)][ValidatePattern('^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$')][string]$Repository,
    [switch]$RotateSqlPassword
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'HostingFunctions.ps1')

$Runtime = 'DOTNETCORE:10.0'
$LinuxFxVersion = 'DOTNETCORE|10.0'          # how the web app's config reports $Runtime
$SqlAdministratorLogin = 'htsqladmin'        # not secret; fixed so every run and rotation agree
$ConnectionStringName = 'HabitTracker'
$EnvironmentName = 'demo'
$AppRegistrationName = "$WebAppName-github-deploy"
$FederatedCredentialName = 'github-environment-demo'
$LogsApiVersion = '2023-12-01'              # Microsoft.Web API version for the config/logs resource
$StepCount = 8

function Write-Step([int]$Number, [string]$Title) {
    Write-Host ''
    Write-Host "== Step $Number/$StepCount`: $Title"
}

function Write-Info([string]$Text) {
    Write-Host "   $Text"
}

# az with errors only (no warnings) on the console; throws on a non-zero exit code.
function Invoke-Az([string[]]$Arguments) {
    Invoke-NativeTool -FilePath 'az' -ArgumentList ($Arguments + '--only-show-errors')
}

# az with JSON output, parsed. A JSON list comes out as its items (wrap the call in @() for a list); empty
# output or an empty list gives nothing.
function Invoke-AzJson([string[]]$Arguments) {
    $text = (Invoke-Az ($Arguments + @('--output', 'json'))) -join "`n"
    if ([string]::IsNullOrWhiteSpace($text)) { return }
    $text | ConvertFrom-Json
}

# az with one plain value as output (tsv).
function Invoke-AzValue([string[]]$Arguments) {
    return ((Invoke-Az ($Arguments + @('--output', 'tsv'))) -join "`n").Trim()
}

# The names of a list command's items (`--query [].name`). Only names reach this script, never values.
function Get-AzNames([string[]]$Arguments) {
    return @(Invoke-AzJson ($Arguments + @('--query', '[].name')))
}

function Invoke-Gh([string[]]$Arguments) {
    Invoke-NativeTool -FilePath 'gh' -ArgumentList $Arguments
}

# Asks for one Operator value with a masked prompt, twice. Returns the value, or $null when the Operator
# pressed Enter and a value is stored (the stored value is kept). Never prints the value.
function Read-OperatorValue([string]$Name, [string]$Prompt, [bool]$Stored, [scriptblock]$Validate, [string]$Rule) {
    while ($true) {
        $hint = if ($Stored) { 'Enter keeps the stored value' } else { 'required' }
        $value = Read-Host -Prompt "   $Prompt ($hint)" -MaskInput
        if ([string]::IsNullOrEmpty($value)) {
            if ($Stored) {
                Write-Info "$Name`: keeping the stored value"
                return $null
            }
            Write-Info "$Name`: refused, no value is stored yet"
            continue
        }
        if ([string]::IsNullOrWhiteSpace($value)) {
            Write-Info "$Name`: refused, the value is only whitespace"
            continue
        }
        if ($null -ne $Validate -and -not (& $Validate $value)) {
            Write-Info "$Name`: refused, $Rule"
            continue
        }
        $repeat = Read-Host -Prompt "   Repeat $Prompt" -MaskInput
        if ($repeat -cne $value) {
            Write-Info "$Name`: the two entries differ, try again"
            continue
        }
        Write-Info "$Name`: new value accepted"
        return $value
    }
}

foreach ($tool in 'az', 'gh') {
    if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) { throw "$tool is not installed or not on PATH." }
}

# --- 1. Subscription and runtime ---------------------------------------------------------------
Write-Step 1 'subscription and the .NET 10 runtime'
Invoke-Az @('account', 'set', '--subscription', $Subscription) | Out-Null
$account = Invoke-AzJson @('account', 'show')
$subscriptionId = [string]$account.id
$tenantId = [string]$account.tenantId
Write-Info "subscription $subscriptionId, tenant $tenantId"
$runtimes = @(Invoke-AzJson @('webapp', 'list-runtimes', '--os', 'linux'))
if ($runtimes -notcontains $Runtime) {
    throw "App Service on Linux does not offer $Runtime (az webapp list-runtimes --os linux). Nothing was created."
}
Write-Info "$Runtime is available"

# --- 2. Resource group, plan, web app ----------------------------------------------------------
Write-Step 2 'resource group, App Service plan, web app'
$groupExists = (Invoke-AzValue @('group', 'exists', '--name', $ResourceGroup)) -eq 'true'
if (-not $groupExists) {
    Invoke-Az @('group', 'create', '--name', $ResourceGroup, '--location', $Location, '--output', 'none') | Out-Null
    Write-Info "resource group $ResourceGroup created in $Location"
}
else {
    Write-Info "resource group $ResourceGroup exists"
}
$resourceGroupId = Invoke-AzValue @('group', 'show', '--name', $ResourceGroup, '--query', 'id')

if ((Get-AzNames @('appservice', 'plan', 'list', '--resource-group', $ResourceGroup)) -notcontains $PlanName) {
    Invoke-Az @('appservice', 'plan', 'create', '--name', $PlanName, '--resource-group', $ResourceGroup,
        '--location', $Location, '--is-linux', '--sku', 'B1', '--number-of-workers', '1', '--output', 'none') | Out-Null
    Write-Info "plan $PlanName created (Linux, B1)"
}
else {
    if ((Invoke-AzValue @('appservice', 'plan', 'show', '--name', $PlanName, '--resource-group', $ResourceGroup, '--query', 'reserved')) -ne 'true') {
        throw "The plan $PlanName exists but is not a Linux plan. Delete it or choose another name."
    }
    Write-Info "plan $PlanName exists"
}
# One instance at B1, set again on every run.
Invoke-Az @('appservice', 'plan', 'update', '--name', $PlanName, '--resource-group', $ResourceGroup,
    '--sku', 'B1', '--number-of-workers', '1', '--output', 'none') | Out-Null
Write-Info "plan $PlanName`: B1, one instance"

if ((Get-AzNames @('webapp', 'list', '--resource-group', $ResourceGroup)) -notcontains $WebAppName) {
    Invoke-Az @('webapp', 'create', '--name', $WebAppName, '--resource-group', $ResourceGroup, '--plan', $PlanName,
        '--runtime', $Runtime, '--output', 'none') | Out-Null
    Write-Info "web app $WebAppName created ($Runtime)"
}
else {
    Write-Info "web app $WebAppName exists"
}
$fxVersion = Invoke-AzValue @('webapp', 'config', 'show', '--name', $WebAppName, '--resource-group', $ResourceGroup, '--query', 'linuxFxVersion')
if ($fxVersion -ne $LinuxFxVersion) {
    # Not set here: its value holds '|', which az.cmd cannot take as an argument (see the header).
    throw "The web app runs '$fxVersion', not '$LinuxFxVersion'. Set the stack to .NET 10 in the portal, then run again."
}
$webApp = Invoke-AzJson @('webapp', 'show', '--name', $WebAppName, '--resource-group', $ResourceGroup)
$webAppId = [string]$webApp.id
$defaultHostName = [string]$webApp.defaultHostName
$baseUrl = "https://$defaultHostName"

# --- 3. Web app configuration and the operator log ----------------------------------------------
Write-Step 3 'web app configuration and log retention'
Invoke-Az @('webapp', 'update', '--name', $WebAppName, '--resource-group', $ResourceGroup, '--https-only', 'true', '--output', 'none') | Out-Null
Invoke-Az @('webapp', 'config', 'set', '--name', $WebAppName, '--resource-group', $ResourceGroup,
    '--min-tls-version', '1.2', '--always-on', 'true', '--web-sockets-enabled', 'true', '--output', 'none') | Out-Null
Write-Info 'HTTPS only, minimum TLS 1.2, Always On, WebSockets on'
# Container output to the App Service file system (research R-09). This command sets the retention to 3 days
# and 100 MB (hard-coded in the Azure CLI), so the retention is set after it.
Invoke-Az @('webapp', 'log', 'config', '--name', $WebAppName, '--resource-group', $ResourceGroup,
    '--docker-container-logging', 'filesystem', '--output', 'none') | Out-Null
# File-system log retention lives in the web app's config/logs resource (SiteLogsConfig), as
# properties.httpLogs.fileSystem.retentionInDays: the field `az webapp log config` fills for container logging
# and the portal shows as "Retention Period (Days)". Unverified until the first run (quickstart B1).
$logsId = "$webAppId/config/logs"
Invoke-Az @('resource', 'update', '--ids', $logsId, '--api-version', $LogsApiVersion,
    '--set', 'properties.httpLogs.fileSystem.retentionInDays=30', '--output', 'none') | Out-Null
$retention = Invoke-AzValue @('resource', 'show', '--ids', $logsId, '--api-version', $LogsApiVersion,
    '--query', 'properties.httpLogs.fileSystem.retentionInDays')
if ($retention -ne '30') {
    throw "The file-system log retention reads back as '$retention' days, not 30."
}
Write-Info 'container logging to the file system, retention read back: 30 days'

# --- 4. SQL server and the connection string ------------------------------------------------------
Write-Step 4 'SQL server and the connection string'
$serverExists = (Get-AzNames @('sql', 'server', 'list', '--resource-group', $ResourceGroup)) -contains $SqlServerName
$connectionStringStored = (Get-AzNames @('webapp', 'config', 'connection-string', 'list',
        '--name', $WebAppName, '--resource-group', $ResourceGroup)) -contains $ConnectionStringName

if ($serverExists -and -not $RotateSqlPassword -and -not $connectionStringStored) {
    throw 'The SQL server exists, but the web app has no HabitTracker connection string, so the SQL password is unknown: run again with -RotateSqlPassword.'
}

if (-not $serverExists -or $RotateSqlPassword) {
    $sqlPassword = New-SqlAdministratorPassword
    if (-not $serverExists) {
        $serverFqdn = Invoke-WithSecretFile $sqlPassword {
            param($PasswordFile)
            Invoke-AzValue @('sql', 'server', 'create', '--name', $SqlServerName, '--resource-group', $ResourceGroup,
                '--location', $Location, '--admin-user', $SqlAdministratorLogin, '--admin-password', "@$PasswordFile",
                '--query', 'fullyQualifiedDomainName')
        }
        $administratorLogin = $SqlAdministratorLogin
        Write-Info "SQL server $SqlServerName created ($serverFqdn), SQL authentication"
    }
    else {
        $administratorLogin = Invoke-AzValue @('sql', 'server', 'show', '--name', $SqlServerName,
            '--resource-group', $ResourceGroup, '--query', 'administratorLogin')
        $serverFqdn = Invoke-WithSecretFile $sqlPassword {
            param($PasswordFile)
            Invoke-AzValue @('sql', 'server', 'update', '--name', $SqlServerName, '--resource-group', $ResourceGroup,
                '--admin-password', "@$PasswordFile", '--query', 'fullyQualifiedDomainName')
        }
        Write-Info "SQL server $SqlServerName`: administrator password rotated"
    }
    # At once, before any further step: the connection string is the only place the password is kept.
    # File format `<name>=<value>`: every Azure CLI version splits it at the first '='; newer versions try JSON
    # first and fall back to this form. The type comes from --connection-string-type.
    $connectionString = New-SqlConnectionString -ServerFqdn $serverFqdn -Database $SqlDatabaseName `
        -User $administratorLogin -Password $sqlPassword
    try {
        Invoke-WithSecretFile "$ConnectionStringName=$connectionString" {
            param($SettingsFile)
            Invoke-Az @('webapp', 'config', 'connection-string', 'set', '--name', $WebAppName,
                '--resource-group', $ResourceGroup, '--connection-string-type', 'SQLAzure',
                '--settings', "@$SettingsFile", '--output', 'none') | Out-Null
        }
    }
    catch {
        throw "The SQL password was set, but writing the connection string failed ($($_.Exception.Message)). Run again with -RotateSqlPassword."
    }
    finally {
        $sqlPassword = $null
        $connectionString = $null
    }
    Write-Info "connection string $ConnectionStringName written (SQLAzure)"
    if ($RotateSqlPassword -and $serverExists) {
        Write-Info 'start a refresh run of the deployment workflow so the app uses the new password'
    }
}
else {
    Write-Info "SQL server $SqlServerName exists; connection string $ConnectionStringName kept unchanged"
}

# --- 5. Database and firewall --------------------------------------------------------------------
Write-Step 5 'database and firewall rules'
if ((Get-AzNames @('sql', 'db', 'list', '--resource-group', $ResourceGroup, '--server', $SqlServerName)) -notcontains $SqlDatabaseName) {
    Invoke-Az @('sql', 'db', 'create', '--name', $SqlDatabaseName, '--resource-group', $ResourceGroup,
        '--server', $SqlServerName, '--service-objective', 'Basic', '--output', 'none') | Out-Null
    Write-Info "database $SqlDatabaseName created (Basic)"
}
else {
    Write-Info "database $SqlDatabaseName exists"
}

$outbound = @(Get-IPv4AddressList (Invoke-AzValue @('webapp', 'show', '--name', $WebAppName,
            '--resource-group', $ResourceGroup, '--query', 'possibleOutboundIpAddresses')))
if ($outbound.Count -eq 0) { throw "The web app reports no possible outbound IPv4 address." }
$rules = Get-AzNames @('sql', 'server', 'firewall-rule', 'list', '--resource-group', $ResourceGroup, '--server', $SqlServerName)
for ($i = 0; $i -lt $outbound.Count; $i++) {
    $ruleName = "webapp-out-$($i + 1)"
    $verb = if ($rules -contains $ruleName) { 'update' } else { 'create' }
    Invoke-Az @('sql', 'server', 'firewall-rule', $verb, '--resource-group', $ResourceGroup, '--server', $SqlServerName,
        '--name', $ruleName, '--start-ip-address', $outbound[$i], '--end-ip-address', $outbound[$i], '--output', 'none') | Out-Null
}
Write-Info "firewall rules webapp-out-1 to webapp-out-$($outbound.Count) set for the web app's possible outbound addresses"
# Rules left from a longer address list, and "Allow Azure services" (0.0.0.0), are removed. The workflow's
# deploy-demo-runner rule is left alone.
foreach ($rule in $rules) {
    $stale = ($rule -match '^webapp-out-(\d+)$' -and [int]$Matches[1] -gt $outbound.Count) -or $rule -eq 'AllowAllWindowsAzureIps'
    if ($stale) {
        Invoke-Az @('sql', 'server', 'firewall-rule', 'delete', '--resource-group', $ResourceGroup,
            '--server', $SqlServerName, '--name', $rule, '--output', 'none') | Out-Null
        Write-Info "firewall rule $rule removed"
    }
}

# --- 6. App settings ------------------------------------------------------------------------------
Write-Step 6 'app settings'
Invoke-Az @('webapp', 'config', 'appsettings', 'set', '--name', $WebAppName, '--resource-group', $ResourceGroup,
    '--settings', 'DataProtection__KeysDirectory=/home/data-protection-keys', 'ASPNETCORE_FORWARDEDHEADERS_ENABLED=true',
    '--output', 'none') | Out-Null
Write-Info 'DataProtection__KeysDirectory and ASPNETCORE_FORWARDEDHEADERS_ENABLED set'

# Only the names of the stored settings are fetched; a stored value is never read.
$storedSettings = Get-AzNames @('webapp', 'config', 'appsettings', 'list', '--name', $WebAppName, '--resource-group', $ResourceGroup)
$operatorValues = [ordered]@{}
$email = Read-OperatorValue -Name 'Operator__AdministratorEmail' -Prompt 'Administrator email (at example.com)' `
    -Stored ($storedSettings -contains 'Operator__AdministratorEmail') `
    -Validate { param($v) Test-DemoAdministratorEmail $v } -Rule 'the address must be at example.com (HP-014)'
if ($null -ne $email) { $operatorValues['Operator__AdministratorEmail'] = $email }
$adminPassword = Read-OperatorValue -Name 'Operator__AdministratorPassword' -Prompt 'Administrator password' `
    -Stored ($storedSettings -contains 'Operator__AdministratorPassword')
if ($null -ne $adminPassword) { $operatorValues['Operator__AdministratorPassword'] = $adminPassword }
$demoPassword = Read-OperatorValue -Name 'Operator__DemoPassword' -Prompt 'Demo password' `
    -Stored ($storedSettings -contains 'Operator__DemoPassword')
if ($null -ne $demoPassword) { $operatorValues['Operator__DemoPassword'] = $demoPassword }
$email = $null
$adminPassword = $null
$demoPassword = $null

if ($operatorValues.Count -gt 0) {
    # Bulk format: a JSON list of {name, value, slotSetting}. --output none: the command's result lists settings.
    Invoke-WithSecretFile (ConvertTo-AppSettingsJson $operatorValues) {
        param($SettingsFile)
        Invoke-Az @('webapp', 'config', 'appsettings', 'set', '--name', $WebAppName, '--resource-group', $ResourceGroup,
            '--settings', "@$SettingsFile", '--output', 'none') | Out-Null
    }
    Write-Info "set: $(@($operatorValues.Keys) -join ', ')"
}
else {
    Write-Info 'Operator values: all stored values kept'
}
$operatorValues = $null

# --- 7. Entra application for the deployment workflow -------------------------------------------
Write-Step 7 'Entra application, federated credential, role assignment'
$apps = @(Invoke-AzJson @('ad', 'app', 'list', '--display-name', $AppRegistrationName, '--query', '[].appId'))
if ($apps.Count -gt 1) {
    throw "More than one application registration is named $AppRegistrationName; delete the extra ones."
}
if ($apps.Count -eq 0) {
    $clientId = Invoke-AzValue @('ad', 'app', 'create', '--display-name', $AppRegistrationName, '--query', 'appId')
    Write-Info "application $AppRegistrationName created, client ID $clientId"
}
else {
    $clientId = [string]$apps[0]
    Write-Info "application $AppRegistrationName exists, client ID $clientId"
}

$principals = @(Invoke-AzJson @('ad', 'sp', 'list', '--filter', "appId eq '$clientId'", '--query', '[].id'))
if ($principals.Count -eq 0) {
    $principalId = Invoke-AzValue @('ad', 'sp', 'create', '--id', $clientId, '--query', 'id')
    Write-Info "service principal created, object ID $principalId"
}
else {
    $principalId = [string]$principals[0]
    Write-Info "service principal exists, object ID $principalId"
}

# Subject: the workflow's job runs in the environment `demo` (contracts/deploy-workflow.md).
$credential = [ordered]@{
    name        = $FederatedCredentialName
    issuer      = 'https://token.actions.githubusercontent.com'
    subject     = "repo:${Repository}:environment:$EnvironmentName"
    description = "GitHub Actions, $Repository, environment $EnvironmentName"
    audiences   = @('api://AzureADTokenExchange')
}
$credentialNames = Get-AzNames @('ad', 'app', 'federated-credential', 'list', '--id', $clientId)
# --parameters takes a JSON file path or inline JSON (Azure CLI docs); a file avoids quoting through az.cmd.
Invoke-WithSecretFile (ConvertTo-Json -InputObject $credential -Compress) {
    param($CredentialFile)
    if ($credentialNames -contains $FederatedCredentialName) {
        Invoke-Az @('ad', 'app', 'federated-credential', 'update', '--id', $clientId,
            '--federated-credential-id', $FederatedCredentialName, '--parameters', $CredentialFile, '--output', 'none') | Out-Null
    }
    else {
        Invoke-Az @('ad', 'app', 'federated-credential', 'create', '--id', $clientId,
            '--parameters', $CredentialFile, '--output', 'none') | Out-Null
    }
}
Write-Info "federated credential $FederatedCredentialName`: subject $($credential.subject)"

$assignments = @(Invoke-AzJson @('role', 'assignment', 'list', '--assignee', $principalId, '--role', 'Contributor',
        '--scope', $resourceGroupId, '--query', '[].id'))
if ($assignments.Count -eq 0) {
    # Object ID and principal type: no directory lookup, so a just-created service principal still works.
    Invoke-Az @('role', 'assignment', 'create', '--assignee-object-id', $principalId,
        '--assignee-principal-type', 'ServicePrincipal', '--role', 'Contributor', '--scope', $resourceGroupId,
        '--output', 'none') | Out-Null
    Write-Info "role Contributor assigned on $ResourceGroup"
}
else {
    Write-Info "role Contributor on $ResourceGroup exists"
}

# --- 8. GitHub environment and variables ----------------------------------------------------------
Write-Step 8 "GitHub environment $EnvironmentName and its variables"
# REST: PUT /repos/{owner}/{repo}/environments/{name} creates or updates the environment; custom branch
# policies on, protected branches off, so only the branch policies below decide.
$environmentBody = ConvertTo-Json -Compress -InputObject ([ordered]@{
        deployment_branch_policy = [ordered]@{ protected_branches = $false; custom_branch_policies = $true }
    })
Invoke-WithSecretFile $environmentBody {
    param($BodyFile)
    Invoke-Gh @('api', '--method', 'PUT', "repos/$Repository/environments/$EnvironmentName", '--input', $BodyFile) | Out-Null
}
$policiesPath = "repos/$Repository/environments/$EnvironmentName/deployment-branch-policies"
$policies = ((Invoke-Gh @('api', $policiesPath)) -join "`n") | ConvertFrom-Json
$hasMaster = $false
foreach ($policy in @($policies.branch_policies)) {
    $type = if ($policy.PSObject.Properties.Name -contains 'type') { [string]$policy.type } else { 'branch' }
    if ($policy.name -ceq 'master' -and $type -eq 'branch') {
        $hasMaster = $true
    }
    else {
        Invoke-Gh @('api', '--method', 'DELETE', "$policiesPath/$($policy.id)") | Out-Null
        Write-Info "deployment branch policy '$($policy.name)' removed"
    }
}
if (-not $hasMaster) {
    Invoke-Gh @('api', '--method', 'POST', $policiesPath, '-f', 'name=master', '-f', 'type=branch') | Out-Null
}
Write-Info "environment $EnvironmentName`: deployments from master only"

$variables = [ordered]@{
    AZURE_CLIENT_ID       = $clientId
    AZURE_TENANT_ID       = $tenantId
    AZURE_SUBSCRIPTION_ID = $subscriptionId
    AZURE_RESOURCE_GROUP  = $ResourceGroup
    AZURE_WEBAPP_NAME     = $WebAppName
    AZURE_SQL_SERVER      = $SqlServerName
    AZURE_SQL_DATABASE    = $SqlDatabaseName
    DEMO_BASE_URL         = $baseUrl
}
foreach ($key in $variables.Keys) {
    Invoke-Gh @('variable', 'set', $key, '--env', $EnvironmentName, '--repo', $Repository, '--body', $variables[$key]) | Out-Null
    Write-Info "$key = $($variables[$key])"
}

Write-Host ''
Write-Host "Provisioning finished. Published address: $baseUrl"
