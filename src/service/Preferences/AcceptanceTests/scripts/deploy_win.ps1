Set-Location $PSScriptRoot/../

Remove-Item -Path ./deploy -Recurse -Force -ErrorAction Ignore

New-Item -ItemType directory ./deploy | out-null
New-Item -ItemType directory ./deploy/testresults | out-null

Copy-Item ./Dockerfile ./deploy
Copy-Item ./scripts/runtests.sh ./deploy
Copy-Item ./scripts/wait-for-it.sh ./deploy
Copy-Item ./scripts/rm_cr.sh ./deploy

Set-Location ./Tests

Write-Host "Publishing acceptance test preferences" -ForegroundColor DarkGray
Invoke-Expression "dotnet publish RepositoryTests\RepositoryTests.csproj -o ..\..\deploy\RepositoryTests -f netcoreapp3.1"
Invoke-Expression "dotnet publish WebApiTests\WebApiTests.csproj -o ..\..\deploy\WebApiTests -f netcoreapp3.1"
