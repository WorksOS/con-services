enum ReturnCode {
    SUCCESS
    AWS_ECR_LOGIN_FAILED
    OPERATION_FAILED
}

# Load common script functions
$buildScriptsDir = "$PSScriptRoot/../../../../../build"

. $buildScriptsDir/aws-login.ps1
. $buildScriptsDir/build-common-functions.ps1
# END SETUP

Login-Aws

# This is not ideal; but too often the containers fail to start due to drive or volume errors on the existing containers.
# It's possible the build agent will be reused and containers from an earlier build are still running.
Write-Host "`n##[section]Removing old application containers..." -ForegroundColor Green
docker ps -q --filter "name=acceptancetests_" | ForEach-Object { docker stop $_ }
docker ps -aq --filter "name=acceptancetests_" | ForEach-Object { docker rm $_ }

Write-Host "`n##[section]Removing old acceptance test images..." -ForegroundColor Green
docker images acceptancetests* -q | Foreach-Object { docker image rm $_ }

Push-Location $PSScriptRoot/../..
Set-Location $PSScriptRoot/../..
[Environment]::CurrentDirectory = $PWD

Remove-Item -Path ./build/artifacts -Recurse -Force -ErrorAction Ignore
New-Item -ItemType directory ./build/artifacts/logs -ErrorAction Ignore | out-null

Write-Host "`n##[section]Publishing WebAPI project..." -ForegroundColor Green
Invoke-Expression "dotnet publish ./src/WebApi/VSS.Productivity3D.TagFileAuth.WebAPI.csproj --configuration Release --output ./build/artifacts/WebApi --framework netcoreapp3.1 --runtime linux-x64 -nowarn:NU1701 -nowarn:CS1591"

if ($LASTEXITCODE -ne 0) { Exit-With-Code ([ReturnCode]::OPERATION_FAILED) }

Write-Host "`n##[section]Copying static deployment files... " -NoNewline -ForegroundColor Green
Copy-Item ./src/WebApi/appsettings.json ./build/artifacts/WebApi
Copy-Item ./src/WebApi/Dockerfile ./build/artifacts/WebApi
Copy-Item ./src/WebApi/web.config ./build/artifacts/WebApi
Write-Host "Done"

Write-Host "`n##[section]Build and setup Acceptance Tests solution..." -ForegroundColor Green
Set-Location $PSScriptRoot/../../AcceptanceTests

Remove-Item -Path ./deploy -Recurse -Force -ErrorAction Ignore

New-Item -ItemType directory ./deploy | out-null
New-Item -ItemType directory ./deploy/testresults | out-null

Copy-Item ./Dockerfile ./deploy
Copy-Item ./scripts/runtests.sh ./deploy
#Copy-Item ./scripts/wait-for-it.sh ./deploy
#Copy-Item ./scripts/rm_cr.sh ./deploy

Write-Host "##[section]Publishing acceptance test projects..." -ForegroundColor Green
Invoke-Expression "dotnet publish ./tests/WebApiTests/WebApiTests.csproj -o ./deploy/IntegrationTests -f netcoreapp3.1 -nowarn:NU1701 -nowarn:CS1591"

if ($LASTEXITCODE -ne 0) { Exit-With-Code ([ReturnCode]::OPERATION_FAILED) }

# Docker Compose Pull & Up
Push-Location $PSScriptRoot/../..
Write-Host "`n##[section]Processing docker-compose.yml for 'pull'..." -ForegroundColor Green
Invoke-Expression "docker-compose --file build/acceptancetests/docker-compose.yml pull"

Write-Host "`n##[section]Building Docker containers" -ForegroundColor Green
Invoke-Expression "docker-compose --file build/acceptancetests/docker-compose.yml up --build --detach"

Write-Host "`n##[section]Running Docker containers:`n"
Invoke-Expression "docker ps"

Write-Host "`n##[command]docker logs --follow acceptancetests_accepttest_1:`n"
docker logs --follow acceptancetests_accepttest_1 

# Run the acceptance tests
Write-Host "`n##[section]Copying Acceptance tests results file..." -NoNewline -ForegroundColor Green
Remove-Item -Path ./AcceptanceTestResults -Recurse -Force -ErrorAction Ignore
New-Item -ItemType directory ./AcceptanceTestResults | out-null

docker cp acceptancetests_accepttest_1:/app/AcceptanceTestResults/. AcceptanceTestResults

Write-Host "Done"
Write-Host "`nListing results of file copy..."
Get-ChildItem AcceptanceTestResults

Write-Host "`n##[section]Tidyup and exit..." -ForegroundColor Green
#Invoke-Expression "docker-compose --file build/acceptancetests/docker-compose.yml down"

Pop-Location
Exit-With-Code ([ReturnCode]::SUCCESS)
