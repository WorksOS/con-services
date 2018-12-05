Set-Location $PSScriptRoot/../

Remove-Item -Path ./Deploy -Recurse -Force

New-Item -ItemType directory ./Deploy | out-null
New-Item -ItemType directory ./Deploy/TestResults | out-null

Copy-Item ./Dockerfile ./Deploy
Copy-Item ./scripts/runtests.sh ./Deploy
Copy-Item ./scripts/wait-for-it.sh ./Deploy
Copy-Item ./scripts/rm_cr.sh ./Deploy

Set-Location ./Tests

Write-Host "Publishing acceptance test projects" -ForegroundColor DarkGray
Invoke-Expression "dotnet publish WebApiTests/WebApiTests.csproj -o ..\..\deploy\WebApiTests -f netcoreapp2.0"
Invoke-Expression "dotnet publish ExecutorTests/ExecutorTests.csproj -o ..\..\deploy\ExecutorTests -f netcoreapp2.0"
Invoke-Expression "dotnet publish RepositoryTests/RepositoryTests.csproj -o ..\..\deploy\RepositoryTests -f netcoreapp2.0"
