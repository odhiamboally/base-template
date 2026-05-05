param(
    [switch]$SkipBuild,
    [switch]$SkipArchitecture
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

function Invoke-GuardrailCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Description,

        [Parameter(Mandatory = $true)]
        [string[]]$Command
    )

    Write-Host $Description
    & $Command[0] @($Command | Select-Object -Skip 1)

    if ($LASTEXITCODE -ne 0) {
        throw "$Description failed with exit code $LASTEXITCODE."
    }
}

if (-not $SkipBuild) {
    Invoke-GuardrailCommand `
        -Description "Running API build..." `
        -Command @("dotnet", "build", "src\Backend\Api\BT.Api\BT.Api.csproj", "--no-restore")
}

if (-not $SkipArchitecture) {
    Invoke-GuardrailCommand `
        -Description "Running architecture tests..." `
        -Command @("dotnet", "test", "tests\BT.Tests.Architecture\BT.Tests.Architecture.csproj", "--no-restore")
}

Write-Host "Local guardrails passed."
