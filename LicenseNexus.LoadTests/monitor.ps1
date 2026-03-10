param (
[string]$OutputFile = "metrics.csv"
)

"Time,Container,CPU(%),RAM" | Out-File $OutputFile -Encoding utf8

Write-Host "Monitoring started. Writing to $OutputFile..."

try {
    while ($true) {
        $time = Get-Date -Format "HH:mm:ss"
        # Замініть імена контейнерів на ваші точні, якщо вони відрізняються
        $stats = docker stats licensenexus_mssql licensenexus_mongo licensenexus_redis --no-stream --format "$time,{{.Name}},{{.CPUPerc}},{{.MemUsage}}" 2>$null
        $stats | Out-File -Append $OutputFile -Encoding utf8
        Start-Sleep -Seconds 2
    }
}
finally {
    Write-Host "Monitoring stopped."
}