[CmdletBinding()]
param(
    [switch]$IncludeSqlServer,
    [switch]$IncludeSeq,
    [switch]$SkipPull
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$composeFile = Join-Path $repoRoot 'ops/local/docker-compose.yml'
$envFile = Join-Path $repoRoot 'ops/local/.env'
$apiProject = Join-Path $repoRoot 'src/Backend/Api/BT.Api/BT.Api.csproj'

function New-LocalSecret {
    $bytes = [System.Security.Cryptography.RandomNumberGenerator]::GetBytes(24)
    return [Convert]::ToHexString($bytes)
}

function Read-EnvironmentFile([string]$path) {
    $values = @{}
    foreach ($line in Get-Content -LiteralPath $path) {
        if ($line -match '^\s*([^#][^=]*)=(.*)$') {
            $values[$matches[1].Trim()] = $matches[2].Trim()
        }
    }

    return $values
}

if (-not (Test-Path -LiteralPath $envFile)) {
    $rabbitPassword = New-LocalSecret
    $redisPassword = New-LocalSecret
    $sqlPassword = "Bt!$(New-LocalSecret)"

    @(
        'RABBITMQ_USER=btdev'
        "RABBITMQ_PASSWORD=$rabbitPassword"
        "REDIS_PASSWORD=$redisPassword"
        'REDIS_HOST_PORT=6380'
        "MSSQL_SA_PASSWORD=$sqlPassword"
    ) | Set-Content -LiteralPath $envFile -Encoding utf8
}

$environment = Read-EnvironmentFile $envFile
if (-not $environment.ContainsKey('REDIS_HOST_PORT')) {
    Add-Content -LiteralPath $envFile -Value 'REDIS_HOST_PORT=6380'
    $environment = Read-EnvironmentFile $envFile
}

$requiredKeys = @('RABBITMQ_USER', 'RABBITMQ_PASSWORD', 'REDIS_PASSWORD', 'REDIS_HOST_PORT', 'MSSQL_SA_PASSWORD')
foreach ($key in $requiredKeys) {
    if ([string]::IsNullOrWhiteSpace($environment[$key])) {
        throw "Local platform setting '$key' is missing from $envFile."
    }
}

$volumeNames = @(
    'llancore-basetemplate_rabbitmq-data',
    'llancore-basetemplate_redis-data',
    'llancore-basetemplate_seq-data',
    'llancore-basetemplate_azurite-data',
    'llancore-basetemplate_sqlserver-data'
)
foreach ($volumeName in $volumeNames) {
    & docker volume inspect $volumeName *> $null
    if ($LASTEXITCODE -ne 0) {
        & docker volume create $volumeName *> $null
        if ($LASTEXITCODE -ne 0) {
            throw "Could not create Docker volume '$volumeName'."
        }
    }
}

dotnet user-secrets set 'Messaging:Enabled' 'true' --project $apiProject
dotnet user-secrets set 'Messaging:Transport' 'RabbitMq' --project $apiProject
dotnet user-secrets set 'Messaging:RabbitMq:Host' 'localhost' --project $apiProject
dotnet user-secrets set 'Messaging:RabbitMq:VirtualHost' '/' --project $apiProject
dotnet user-secrets set 'Messaging:RabbitMq:Username' $environment.RABBITMQ_USER --project $apiProject
dotnet user-secrets set 'Messaging:RabbitMq:Password' $environment.RABBITMQ_PASSWORD --project $apiProject
dotnet user-secrets set 'CacheSettings:Provider' 'Redis' --project $apiProject
dotnet user-secrets set 'CacheSettings:Redis:ConnectionString' "localhost:$($environment.REDIS_HOST_PORT),password=$($environment.REDIS_PASSWORD),abortConnect=false" --project $apiProject
dotnet user-secrets set 'EmailSettings:Host' 'localhost' --project $apiProject
dotnet user-secrets set 'EmailSettings:Port' '1025' --project $apiProject
dotnet user-secrets set 'EmailSettings:EnableSsl' 'false' --project $apiProject
dotnet user-secrets set 'EmailSettings:UseAuthentication' 'false' --project $apiProject
dotnet user-secrets set 'ProfileImageStorage:Provider' 'Azurite' --project $apiProject
dotnet user-secrets set 'ProfileImageStorage:Azurite:ConnectionString' 'UseDevelopmentStorage=true' --project $apiProject
dotnet user-secrets set 'ProfileImageStorage:Azurite:ContainerName' 'profile-images' --project $apiProject

$profiles = @()
if ($IncludeSqlServer) {
    $profiles += @('--profile', 'database')
    $sqlConnection = "Server=localhost,14333;Database=BT;User Id=sa;Password=$($environment.MSSQL_SA_PASSWORD);Encrypt=True;TrustServerCertificate=True;"
    dotnet user-secrets set 'ConnectionStrings:DefaultConnection' $sqlConnection --project $apiProject
}
if ($IncludeSeq) {
    $profiles += @('--profile', 'observability')
}

$composeArguments = @('compose', '--env-file', $envFile, '-f', $composeFile)
$composeArguments += $profiles
if (-not $SkipPull) {
    & docker @composeArguments pull
    if ($LASTEXITCODE -ne 0) {
        throw "Docker Compose pull failed with exit code $LASTEXITCODE."
    }
}

& docker @composeArguments up -d --wait
if ($LASTEXITCODE -ne 0) {
    throw "Docker Compose startup failed with exit code $LASTEXITCODE."
}

Write-Host ''
Write-Host 'Local platform is ready:'
Write-Host '  RabbitMQ:  amqp://localhost:5672'
Write-Host '  Management: http://localhost:15672'
Write-Host "  Redis:      localhost:$($environment.REDIS_HOST_PORT)"
Write-Host '  Mailpit:    http://localhost:8025'
if ($IncludeSeq) { Write-Host '  Seq:        http://localhost:5341' }
Write-Host '  Azurite:    http://localhost:10000'
if ($IncludeSqlServer) { Write-Host '  SQL Server: localhost,14333' }
