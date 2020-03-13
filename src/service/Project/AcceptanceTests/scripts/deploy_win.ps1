Set-Location $PSScriptRoot/../

Remove-Item -Path ./Deploy -Recurse -Force -ErrorAction Ignore

New-Item -ItemType directory ./Deploy | out-null
New-Item -ItemType directory ./Deploy/TestResults | out-null

Copy-Item ./Dockerfile ./Deploy
Copy-Item ./scripts/runtests.sh ./Deploy
Copy-Item ./scripts/wait-for-it.sh ./Deploy
Copy-Item ./scripts/rm_cr.sh ./Deploy

Set-Location ./Tests

Write-Host "Publishing acceptance test projects" -ForegroundColor DarkGray
Invoke-Expression "dotnet publish IntegrationTests\IntegrationTests.csproj -o ..\..\Deploy\IntegrationTests -f netcoreapp2.1"
