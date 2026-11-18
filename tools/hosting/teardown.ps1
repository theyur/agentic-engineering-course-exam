# Deletes the hosted demo after 2027-01-31 (ADR 0015, research R-03): the resource group with everything in it,
# the Entra application registration of the deployment workflow (its service principal goes with it), and the
# GitHub environment `demo` with its variables. Run by the Operator after `az login` and `gh auth login`.
#
#   pwsh -NoProfile -File tools/hosting/teardown.ps1 -Subscription <id or name> -ResourceGroup <rg> `
#       -Repository <owner/name> -ClientId <AZURE_CLIENT_ID of the environment demo>
#
# Asks for confirmation by typing the resource group name. Each part is skipped when it is already gone, so a
# second run after a failure finishes the rest. Prints identifiers only.
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Subscription,
    [Parameter(Mandatory)][ValidatePattern('^[A-Za-z0-9._-]+$')][string]$ResourceGroup,
    [Parameter(Mandatory)][ValidatePattern('^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$')][string]$Repository,
    [Parameter(Mandatory)][ValidatePattern('^[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}$')][string]$ClientId
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'HostingFunctions.ps1')

$EnvironmentName = 'demo'

foreach ($tool in 'az', 'gh') {
    if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) { throw "$tool is not installed or not on PATH." }
}

Invoke-NativeTool 'az' @('account', 'set', '--subscription', $Subscription, '--only-show-errors') | Out-Null
$subscriptionId = ((Invoke-NativeTool 'az' @('account', 'show', '--query', 'id', '--output', 'tsv', '--only-show-errors')) -join '').Trim()

Write-Host "This deletes, without a way back:"
Write-Host "  - resource group $ResourceGroup in subscription $subscriptionId, with the web app, the SQL server and the database"
Write-Host "  - application registration $ClientId and its service principal"
Write-Host "  - GitHub environment $EnvironmentName of $Repository, with its variables"
$answer = Read-Host -Prompt "Type the resource group name ($ResourceGroup) to confirm"
if ($answer -cne $ResourceGroup) {
    Write-Host 'Not confirmed. Nothing was deleted.'
    exit 1
}

# 1. Resource group (waits until Azure has deleted it).
$groupExists = ((Invoke-NativeTool 'az' @('group', 'exists', '--name', $ResourceGroup, '--output', 'tsv', '--only-show-errors')) -join '').Trim() -eq 'true'
if ($groupExists) {
    Write-Host "Deleting resource group $ResourceGroup (this takes a few minutes)"
    Invoke-NativeTool 'az' @('group', 'delete', '--name', $ResourceGroup, '--yes', '--only-show-errors') | Out-Null
    Write-Host "Resource group $ResourceGroup deleted"
}
else {
    Write-Host "Resource group $ResourceGroup does not exist; skipped"
}

# 2. Application registration. Deleting it deletes its service principal and federated credential; the role
# assignment went with the resource group.
$apps = ((Invoke-NativeTool 'az' @('ad', 'app', 'list', '--app-id', $ClientId, '--query', '[].appId', '--output', 'json', '--only-show-errors')) -join "`n") | ConvertFrom-Json
if (@($apps).Count -gt 0) {
    Invoke-NativeTool 'az' @('ad', 'app', 'delete', '--id', $ClientId, '--only-show-errors') | Out-Null
    Write-Host "Application registration $ClientId deleted"
}
else {
    Write-Host "Application registration $ClientId does not exist; skipped"
}

# 3. GitHub environment (REST: DELETE /repos/{owner}/{repo}/environments/{name}; its variables go with it).
$environments = ((Invoke-NativeTool 'gh' @('api', "repos/$Repository/environments")) -join "`n") | ConvertFrom-Json
if (@($environments.environments | Where-Object { $_.name -eq $EnvironmentName }).Count -gt 0) {
    Invoke-NativeTool 'gh' @('api', '--method', 'DELETE', "repos/$Repository/environments/$EnvironmentName") | Out-Null
    Write-Host "GitHub environment $EnvironmentName deleted"
}
else {
    Write-Host "GitHub environment $EnvironmentName does not exist; skipped"
}

Write-Host 'Teardown finished.'
