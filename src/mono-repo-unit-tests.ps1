Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
Write-Host "Recursively running all unit test projects..." -ForegroundColor DarkGray
Get-ChildItem .\ -include *unit*.csproj -Recurse | ForEach-Object ($_) { Write-Host "Processing '$_'..."; dotnet test $_.fullname }
Write-Host "Done.`n" -ForegroundColor Green
