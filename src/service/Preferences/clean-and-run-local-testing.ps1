[Console]::ResetColor()

IF ($args -contains "--set-vars") { & ./set-environment-variables.ps1 }

Write-Host "Stopping Docker containers"
docker ps -q | ForEach-Object { docker stop $_ }

# This is not ideal; but too often the containers fail to start due to drive or volume errors on the existing containers.
Write-Host "Removing old application containers"
docker ps -aq --filter "name=project_" | ForEach-Object { docker rm $_ }

Write-Host "Connecting to image host" -ForegroundColor DarkGray
Invoke-Expression -Command (aws ecr get-login --no-include-email --region us-west-2 --profile okta)

IF (-not $?) {
    Write-Host "Error: Logging in to AWS, won't pull latest images for container dependencies." -ForegroundColor Red
}

IF ($args -notcontains "--no-build") {
    Write-Host "Building solution" -ForegroundColor DarkGray

    $artifactsWorkingDir = "${PSScriptRoot}/artifacts/PreferencesWebApi"

    Remove-Item -Path ./artifacts -Recurse -Force -ErrorAction Ignore
    Invoke-Expression "dotnet publish ./src/CCSS.Productivity3D.Preferences/CCSS.Productivity3D.Preferences.csproj -o $artifactsWorkingDir -f netcoreapp3.1 -c Docker"
    Invoke-Expression "dotnet build ./test/UnitTests/CCSS.Productivity3D.Preferences.Tests.csproj"
    Copy-Item ./src/CCSS.Productivity3D.Preferences/appsettings.json $artifactsWorkingDir
    New-Item -ItemType directory ./artifacts/logs | out-null


    Write-Host "Copying static deployment files" -ForegroundColor DarkGray
    Set-Location ./src/CCSS.Productivity3D.Preferences
    Copy-Item ./appsettings.json $artifactsWorkingDir
    Copy-Item ./Dockerfile $artifactsWorkingDir
    Copy-Item ./web.config $artifactsWorkingDir

    & $PSScriptRoot/AcceptanceTests/Scripts/deploy_win.ps1
}

Write-Host "Building image dependencies" -ForegroundColor DarkGray

Set-Location $PSScriptRoot
$dockerComposeConfig = "docker-compose-local"

Invoke-Expression "docker-compose --file $dockerComposeConfig.yml pull"

Write-Host "Building Docker containers" -ForegroundColor DarkGray
Invoke-Expression "docker-compose --file $dockerComposeConfig.yml up --build --detach > ${PSScriptRoot}/artifacts/logs/output.log"

IF ($args -contains "--no-test") { 
    $acceptanceTestContainerName = "preference_accepttest"
    Write-Host "`nOpted out of running acceptance tests, stopping container $acceptanceTestContainerName..." -ForegroundColor DarkGray
    docker ps -q --filter="name=$acceptanceTestContainerName" | ForEach-Object { docker stop $_ }
}

IF (-not $?) {
    Write-Host "Error: Environment failed to start" -ForegroundColor Red
    EXIT 1
}

Write-Host "Finished`n" -ForegroundColor Green
