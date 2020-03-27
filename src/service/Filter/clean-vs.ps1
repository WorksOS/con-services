Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
Write-Host "Recursively removing all /bin and /obj folders..." -ForegroundColor DarkGray
Get-ChildItem .\ -include bin,obj -Recurse | ForEach-Object ($_) { Write-Host "Processing '$_'..."; Remove-Item $_.fullname -Force -Recurse }
Write-Host "Done." -ForegroundColor Green