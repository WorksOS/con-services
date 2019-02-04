[Console]::ResetColor()

IF (-NOT($args -contains "--no-var")) {
    & $PSScriptRoot/set-environment-variables.ps1
}

Write-Host "Removing old Filter service application containers" -ForegroundColor DarkGray

# Stop and remove Filter service containers only; leave non affected containers running.
$array = @("filter_kafka", "filter_webapi", "filter_accepttest")

FOR ($i = 0; $i -lt $array.length; $i++) {
    $containerName = $array[$i]

    IF (docker ps -q --filter "name=$containerName") {
        docker stop $(docker ps -q --filter "name=$containerName")
    }

    IF (docker ps -aq --filter "name=$containerName") {
        docker rm $(docker ps -aq --filter "name=$containerName")
    }
}

Write-Host "Done" -ForegroundColor Green

Write-Host "Building solution" -ForegroundColor DarkGray

$artifactsWorkingDir = "${PSScriptRoot}/artifacts/VSS.Productivity3D.Filter.WebApi"

Remove-Item -Path ./artifacts -Recurse -Force -ErrorAction Ignore
Invoke-Expression "dotnet publish ./src/VSS.Productivity3D.Filter.WebApi/VSS.Productivity3D.Filter.WebApi.csproj -o ../../artifacts/VSS.Productivity3D.Filter.WebApi -f netcoreapp2.1 -c Docker"
Invoke-Expression "dotnet build ./test/UnitTests/VSS.Productivity3D.Filter.Tests/VSS.Productivity3D.Filter.Tests.csproj"
Copy-Item ./src/VSS.Productivity3D.Filter.WebApi/appsettings.json $artifactsWorkingDir
New-Item -ItemType directory ./artifacts/logs | out-null

Write-Host "Copying static deployment files" -ForegroundColor DarkGray
Set-Location ./src/VSS.Productivity3D.Filter.WebApi
Copy-Item ./appsettings.json $artifactsWorkingDir
Copy-Item ./Dockerfile $artifactsWorkingDir
Copy-Item ./web.config $artifactsWorkingDir
Copy-Item ./log4net.xml $artifactsWorkingDir

& $PSScriptRoot/AcceptanceTests/Scripts/deploy_win.ps1

Write-Host "Connecting to image host" -ForegroundColor DarkGray
Invoke-Expression -Command (aws ecr get-login --no-include-email --region us-west-2)

Write-Host "Building image dependencies" -ForegroundColor DarkGray

Set-Location $PSScriptRoot
Invoke-Expression "docker-compose --file docker-compose-local.yml pull"

Write-Host "Building Docker containers" -ForegroundColor DarkGray

# This legacy setting suppresses logging to the console by piping it to a file on disk. If you're looking for the application logs from within the container see .artifacts/logs/.
$logToFile = IF ($args -contains "--no-log") { "" } ELSE { "> C:\Temp\output.log" }
$detach = IF ($args -contains "--detach") { "--detach" } ELSE { "" }

Invoke-Expression "docker-compose --file docker-compose-local.yml up --build $detach $logToFile"

IF (-not $?) {
    Write-Host "Error: Environment failed to start" -ForegroundColor Red
    EXIT 1
}

Write-Host "Finished" -ForegroundColor Green
