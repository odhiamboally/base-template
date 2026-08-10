$files = @(
    "BankingTenantIsolationTests",
    "HrTenantIsolationTests",
    "IamTenantIsolationTests",
    "SharedTenantIsolationTests",
    "TenantConnectionInterceptorIntegrationTests",
    "TenantIsolationMutationTests"
)

foreach ($file in $files) {
    $path = "tests\BT.Tests.Integration\$file.cs"
    $content = Get-Content $path -Raw
    
    # Extract the common usings and namespace
    $usings = ($content -split "`r`n`r`n")[0]
    $namespaceMatch = [regex]::Match($content, "(?m)^namespace .*;")
    if ($namespaceMatch.Success) {
        $namespace = $namespaceMatch.Value
    } else {
        $namespace = "namespace BT.Tests.Integration;"
    }
    
    # For TenantConnectionInterceptorIntegrationTests, the classes have a different suffix
    $postgresSuffix = "_Postgres"
    $sqlServerSuffix = "_SqlServer"
    if ($file -eq "TenantConnectionInterceptorIntegrationTests" -or $file -eq "TenantIsolationMutationTests") {
        $postgresSuffix = "_PostgreSql"
    }

    $postgresRegex = "(?ms)^public class ${file}${postgresSuffix}.*?^}"
    $sqlServerRegex = "(?ms)^public class ${file}${sqlServerSuffix}.*?^}"
    
    $postgresClass = [regex]::Match($content, $postgresRegex).Value
    $sqlServerClass = [regex]::Match($content, $sqlServerRegex).Value
    
    if ($postgresClass) {
        Set-Content "tests\BT.Tests.Integration\${file}${postgresSuffix}.cs" "$usings`r`n`r`n$namespace`r`n`r`n$postgresClass"
    }
    
    if ($sqlServerClass) {
        Set-Content "tests\BT.Tests.Integration\${file}${sqlServerSuffix}.cs" "$usings`r`n`r`n$namespace`r`n`r`n$sqlServerClass"
    }
    
    # Remove from original
    $newContent = $content -replace $postgresRegex, ""
    $newContent = $newContent -replace $sqlServerRegex, ""
    
    # Rename base class to match filename
    $newContent = $newContent -replace "public abstract class ${file}Base", "public abstract class ${file}"
    $newContent = $newContent -replace "public class ${file}Base", "public abstract class ${file}"
    
    # Fix constructor name
    $newContent = $newContent -replace "protected ${file}Base", "protected ${file}"
    $newContent = $newContent -replace "public ${file}Base", "public ${file}"
    
    Set-Content $path $newContent
}
