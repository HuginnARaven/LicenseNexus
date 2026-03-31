$repoRoot = Resolve-Path "$PSScriptRoot\..\.."
Set-Location $repoRoot

Write-Host "Repository root detected: $((Get-Location).Path)" -ForegroundColor DarkGray

$scenarios = @("read", "textsearch", "mixed", "consistency", "checkout", "write")

foreach ($scenario in $scenarios) {
    Write-Host "=====================================================" -ForegroundColor Cyan
    Write-Host " Preparing clean baseline for: $scenario" -ForegroundColor Cyan
    Write-Host "=====================================================" -ForegroundColor Cyan
    
    Write-Host "Stopping API container..."
    docker compose stop licensenexus.api

    # Write-Host "Flushing Redis and Mongo databases..."
    # docker exec licensenexus_redis redis-cli FLUSHALL
    # docker exec licensenexus_mongo mongosh LicenseNexus_Mongo --eval "db.dropDatabase()"

    # Write-Host "Restarting Database containers to clear RAM caches..." -ForegroundColor Magenta
    # docker compose restart mssql mongo redis
    
    Write-Host "Seeding baseline data..."
    Set-Location ".\LicenseNexus.DataSeeder"
    dotnet run -c Release
    Set-Location $repoRoot
    
    Write-Host "Starting API container..."
    docker compose start licensenexus.api

    $targetLog = "Content root path: /app"
    $timeoutSeconds = 300
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $isReady = $false

    Write-Host "Waiting for container to report readiness..." -ForegroundColor Yellow

    while ($stopwatch.Elapsed.TotalSeconds -lt $timeoutSeconds) {
        $logs = docker logs --tail 50 licensenexus_api 2>&1

        if ($logs -match $targetLog) {
            $isReady = $true
            break
        }
        Start-Sleep -Seconds 2
    }

    $stopwatch.Stop()

    if ($isReady) {
        Write-Host "API and databases are fully ready! (Took $($stopwatch.Elapsed.ToString("mm\:ss")))" -ForegroundColor Green
    } else {
        Write-Host "TIMEOUT ERROR: API did not start within $timeoutSeconds seconds!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Waiting for 30s..."
    Start-Sleep -Seconds 30
    
    Write-Host ">>> RUNNING SCENARIO: $scenario <<<" -ForegroundColor Green
    Set-Location ".\LicenseNexus.LoadTests"
    dotnet run -c Release -- $scenario
    Set-Location $repoRoot
    
    Write-Host "Cooling down for metrics collection (30s)..."
    Start-Sleep -Seconds 30
}

Write-Host "All load tests completed!" -ForegroundColor Green