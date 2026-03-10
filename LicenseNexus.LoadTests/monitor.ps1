$outputFile = "hardware_metrics.csv"
"Time,Container,CPU(%),RAM" | Out-File $outputFile

Write-Host "Metrics collection has started. Press Ctrl+C to stop."
while ($true) {
    $time = Get-Date -Format "HH:mm:ss"
    $stats = docker stats licensenexus_mssql licensenexus_mongo licensenexus_redis --no-stream --format "$time,{{.Name}},{{.CPUPerc}},{{.MemUsage}}"
    $stats | Out-File -Append $outputFile
    Start-Sleep -Seconds 2
}