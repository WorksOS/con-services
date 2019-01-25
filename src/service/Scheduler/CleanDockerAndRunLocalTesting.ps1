& $PSScriptRoot/DockerEnvironmentVariables.ps1

Write-Host "Stopping Docker containers" -ForegroundColor DarkGray
docker stop $(docker ps -aq)

Write-Host "Removing old application containers" -ForegroundColor DarkGray
docker rm $(docker ps -aq --filter "name=scheduler_schema")
docker rm $(docker ps -aq --filter "name=scheduler_webapi")
docker rm $(docker ps -aq --filter "name=scheduler_accepttest")
docker rm $(docker ps -aq --filter "name=scheduler_db")
docker rm $(docker ps -aq --filter "name=scheduler_mockprojectwebapi")

Write-Host "Connecting to image host" -ForegroundColor DarkGray
Invoke-Expression -Command (aws ecr get-login --no-include-email --region us-west-2)

Write-Host "Building solution" -ForegroundColor DarkGray

$artifactsWorkingDir = "${PSScriptRoot}/artifacts/VSS.Productivity3D.Scheduler.WebApi"

Remove-Item -Path ./artifacts -Recurse -Force -ErrorAction Ignore
Invoke-Expression "dotnet publish ./src/VSS.Productivity3D.Scheduler.WebApi/VSS.Productivity3D.Scheduler.WebApi.csproj -o ../../artifacts/VSS.Productivity3D.Scheduler.WebApi -f netcoreapp2.1 -c Docker"
Invoke-Expression "dotnet build ./test/UnitTests/VSS.Productivity3D.Scheduler.Tests/VSS.Productivity3D.Scheduler.Tests.csproj"
Copy-Item ./src/VSS.Productivity3D.Scheduler.WebApi/appsettings.json $artifactsWorkingDir
New-Item -ItemType directory ./artifacts/logs | out-null

Write-Host "Copying static deployment files" -ForegroundColor DarkGray
Set-Location ./src/VSS.Productivity3D.Scheduler.WebApi
Copy-Item ./appsettings.json $artifactsWorkingDir
Copy-Item ./Dockerfile $artifactsWorkingDir
Copy-Item ./web.config $artifactsWorkingDir
Copy-Item ./log4net.xml $artifactsWorkingDir
Copy-Item ./bin/Docker/netcoreapp2.1/VSS.Productivity3D.Scheduler.WebAPI.xml $artifactsWorkingDir

& $PSScriptRoot/AcceptanceTests/Scripts/deploy_win.ps1

Write-Host "Building image dependencies" -ForegroundColor DarkGray
Set-Location $PSScriptRoot
Invoke-Expression "docker-compose --file docker-compose-local.yml pull"

Write-Host "Building Docker containers" -ForegroundColor DarkGray

# This legacy setting suppresses logging to the console by piping it to a file on disk. If you're looking for the application logs from within the container see .artifacts/logs/.
$logToFile = IF ($args -contains "--no-log") { "" } ELSE { "> C:\Temp\output.log" }
$detach = IF ($args -contains "--detach") { "--detach" } ELSE { "" }

Invoke-Expression "docker-compose --file docker-compose-local.yml up --build $detach $logToFile"

IF ($args -notcontains "--no-test") {
    Write-Host "Running acceptance tests" -ForegroundColor DarkGray
    Start-Sleep -s 20 # Wait for the database container to come up.
    Invoke-Expression "dotnet test .\AcceptanceTests\VSS.Productivity3D.Scheduler.AcceptanceTests.sln --logger:trx"
}

[Console]::ResetColor()

IF (-not $?) {
    Write-Host "Error: Environment failed to start" -ForegroundColor Red
    EXIT 1
}

Write-Host "Finished" -ForegroundColor Green
