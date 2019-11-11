[Console]::ResetColor()

# Set KAFKA_ADVERTISED_HOST_NAME for local testing.
& ../../config/apply-kafka-config-local.ps1

Write-Host "Stopping Docker containers"
docker ps -q | ForEach-Object { docker stop $_ }

# This is not ideal; but too often the containers fail to start due to drive or volume errors on the existing containers.
Write-Host "Removing old application containers"
docker ps -aq --filter "name=project_" | ForEach-Object { docker rm $_ }

Write-Host "Connecting to image host" -ForegroundColor DarkGray
Invoke-Expression -Command (aws ecr get-login --no-include-email --region us-west-2)

IF (-not $?) {
    Write-Host "Error: Logging in to AWS, won't pull latest images for container dependencies." -ForegroundColor Red
}

IF ($args -notcontains "--no-build") {
    Write-Host "Building solution" -ForegroundColor DarkGray

    $artifactsWorkingDir = "${PSScriptRoot}/artifacts/ProjectWebApi"

    Remove-Item -Path ./artifacts -Recurse -Force -ErrorAction Ignore
    Invoke-Expression "dotnet publish ./src/ProjectWebApi/VSS.Project.WebApi.csproj -o $artifactsWorkingDir -f netcoreapp2.1 -c Docker"
    Invoke-Expression "dotnet build ./test/UnitTests/MasterDataProjectTests/VSS.Project.WebApi.Tests.csproj"
    Copy-Item ./src/ProjectWebApi/appsettings.json $artifactsWorkingDir
    New-Item -ItemType directory ./artifacts/logs | out-null

    Write-Host "Copying static deployment files" -ForegroundColor DarkGray
    Set-Location ./src/ProjectWebApi
    Copy-Item ./appsettings.json $artifactsWorkingDir
    Copy-Item ./Dockerfile $artifactsWorkingDir
    Copy-Item ./web.config $artifactsWorkingDir

    & $PSScriptRoot/AcceptanceTests/Scripts/deploy_win.ps1
}

Write-Host "Building image dependencies" -ForegroundColor DarkGray

Set-Location $PSScriptRoot
Invoke-Expression "docker-compose --file docker-compose-local.yml pull"

Write-Host "Building Docker containers" -ForegroundColor DarkGray
Invoke-Expression "docker-compose --file docker-compose-local.yml up --build --detach > ${PSScriptRoot}/artifacts/logs/output.log"

IF ($args -contains "--no-test") { 
    $acceptanceTestContainerName = "project_accepttest"
    Write-Host "`nOpted out of running acceptance tests, stopping container $acceptanceTestContainerName..." -ForegroundColor DarkGray
    docker ps -q --filter="name=$acceptanceTestContainerName" | ForEach-Object { docker stop $_ }
}

IF (-not $?) {
    Write-Host "Error: Environment failed to start" -ForegroundColor Red
    EXIT 1
}

Write-Host "Finished`n" -ForegroundColor Green
