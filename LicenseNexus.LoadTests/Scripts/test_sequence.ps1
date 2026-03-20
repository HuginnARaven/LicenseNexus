dotnet run -c Release -- read
Start-Sleep -Seconds 120

dotnet run -c Release -- textsearch
Start-Sleep -Seconds 120

dotnet run -c Release -- mixed
Start-Sleep -Seconds 120

dotnet run -c Release -- consistency
Start-Sleep -Seconds 120

dotnet run -c Release -- checkout
Start-Sleep -Seconds 120

dotnet run -c Release -- write