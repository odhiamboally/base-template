[CmdletBinding(SupportsShouldProcess, ConfirmImpact = 'High')]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^[A-Za-z][A-Za-z0-9 ]*$')]
    [string]$NewName,

    [ValidatePattern('^[A-Za-z][A-Za-z0-9]*$')]
    [string]$NamespacePrefix = 'BT'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$displayName = $NewName.Trim()
$compactName = $displayName -replace '[^A-Za-z0-9]', ''
$kebabName = (($displayName -creplace '([a-z0-9])([A-Z])', '$1-$2') -replace '\s+', '-').ToLowerInvariant()
$lowerCompactName = $compactName.ToLowerInvariant()

$excludedDirectoryNames = @('.git', '.vs', '.idea', 'bin', 'obj', 'node_modules', 'Logs')
$excludedFiles = @(
    (Join-Path $repoRoot 'ops/local/.env')
)
$textExtensions = @(
    '.cs', '.csproj', '.props', '.targets', '.sln', '.slnx', '.json', '.yml', '.yaml',
    '.md', '.html', '.razor', '.css', '.js', '.ts', '.ps1', '.sh', '.xml', '.config',
    '.http', '.editorconfig', '.gitignore', '.gitattributes', '.windsurfrules', '.mdc'
)

function Test-IsExcludedPath([string]$path) {
    if ($excludedFiles -contains $path) {
        return $true
    }

    if ($path.EndsWith('.user', [StringComparison]::OrdinalIgnoreCase) -or
        $path.EndsWith('.suo', [StringComparison]::OrdinalIgnoreCase)) {
        return $true
    }

    $relativePath = [System.IO.Path]::GetRelativePath($repoRoot, $path)
    $segments = $relativePath -split '[\\/]'
    return $segments | Where-Object { $excludedDirectoryNames -contains $_ } | Select-Object -First 1
}

function Get-ReplacedText([string]$value) {
    $updated = $value.Replace('Base Template', $displayName)
    $updated = $updated.Replace('BaseTemplate', $compactName)
    $updated = $updated.Replace('base-template', $kebabName)
    $updated = $updated.Replace('basetemplate', $lowerCompactName)

    if ($NamespacePrefix -ne 'BT') {
        $updated = $updated.Replace('BT.', "$NamespacePrefix.")
    }

    return $updated
}

$files = Get-ChildItem -LiteralPath $repoRoot -Recurse -File -Force |
    Where-Object {
        -not (Test-IsExcludedPath $_.FullName) -and
        ($textExtensions -contains $_.Extension -or $_.Name -in @('Dockerfile', 'AGENTS.md', 'CLAUDE.md', 'GEMINI.md'))
    }

foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $updated = Get-ReplacedText $content
    if ($updated -ne $content -and $PSCmdlet.ShouldProcess($file.FullName, 'Replace template identity')) {
        [System.IO.File]::WriteAllText($file.FullName, $updated, [System.Text.UTF8Encoding]::new($false))
    }
}

$paths = Get-ChildItem -LiteralPath $repoRoot -Recurse -Force |
    Where-Object { -not (Test-IsExcludedPath $_.FullName) } |
    Sort-Object { $_.FullName.Length } -Descending

foreach ($path in $paths) {
    $newLeafName = Get-ReplacedText $path.Name
    if ($newLeafName -ne $path.Name -and $PSCmdlet.ShouldProcess($path.FullName, "Rename to '$newLeafName'")) {
        Rename-Item -LiteralPath $path.FullName -NewName $newLeafName
    }
}

Write-Host "Template identity updated to '$displayName' with namespace prefix '$NamespacePrefix'."
Write-Host 'Review configuration, URLs, tenant seed data, and Azure resource names before the first run.'
