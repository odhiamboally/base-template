[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }
$headers = @{ "Stripe-Signature" = "t=123,v1=abc" }
$body = '{"id":"test"}'
try {
    $response = Invoke-WebRequest -Uri "https://localhost:7049/api/v1/shared/payments/stripe/webhook" -Method POST -Headers $headers -Body $body -ContentType "application/json"
    Write-Host "StatusCode: $($response.StatusCode)"
    Write-Host "Content: $($response.Content)"
} catch {
    Write-Host "Error: $($_.Exception.Message)"
    if ($_.ErrorDetails) {
        Write-Host "Details: $($_.ErrorDetails.Message)"
    } else {
        if ($_.Exception.Response) {
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $errBody = $reader.ReadToEnd()
            Write-Host "Details: $errBody"
        }
    }
}
