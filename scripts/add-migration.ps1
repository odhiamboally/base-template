param(
    [Parameter(Mandatory=$true)]
    [string]$Name,

    [Parameter(Mandatory=$true)]
    [ValidateSet("Iam", "Banking", "Hr", "Shared", "ControlPlane")]
    [string]$ContextName
)

$ErrorActionPreference = "Stop"

Write-Host "Adding migrations for $ContextName bounded context..." -ForegroundColor Cyan

# Paths
$PersistenceProject = "src/Backend/Persistence/BT.Persistence/BT.Persistence.csproj"
$StartupProject = "src/Backend/Api/BT.Api/BT.Api.csproj"

# DB Context Names
$SqlServerContext = "${ContextName}SqlServerDBContext"
$PostgreSqlContext = "${ContextName}PostgreSqlDBContext"

# Run for SQL Server
Write-Host "`n[1/2] Generating SQL Server migration ($SqlServerContext)..." -ForegroundColor Yellow
dotnet ef migrations add $Name -c $SqlServerContext -p $PersistenceProject -s $StartupProject

# Run for PostgreSQL
Write-Host "`n[2/2] Generating PostgreSQL migration ($PostgreSqlContext)..." -ForegroundColor Yellow
dotnet ef migrations add $Name -c $PostgreSqlContext -p $PersistenceProject -s $StartupProject

Write-Host "`nDone! Migrations for both SQL Server and PostgreSQL have been added." -ForegroundColor Green
