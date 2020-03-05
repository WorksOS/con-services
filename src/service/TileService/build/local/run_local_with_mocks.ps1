$currentFolder = Get-Location
Set-Location $PSScriptRoot

IF (docker ps -q --filter "name=mockprojectwebapi") {
    Write-Host "Found existing mockwebapi container already running, terminating..." -ForegroundColor DarkGray
    docker stop $(docker ps -q --filter "name=mockprojectwebapi")
    Write-Host "Done" -ForegroundColor Green
}

Write-Host "Connecting to image host" -ForegroundColor DarkGray
$(aws ecr get-login --no-include-email)

IF (-not $?) {
    Write-Host "Error: Logging in to AWS, are you authenticated?" -ForegroundColor Red
    EXIT 1
}

Write-Host "Running Docker containers..." -ForegroundColor DarkGray
docker run -d -p 5001:5001 940327799086.dkr.ecr.us-west-2.amazonaws.com/ccss-mockproject-webapi:latest-linux
docker run -d -p 80:80 --env-file .\mocks.env 940327799086.dkr.ecr.us-west-2.amazonaws.com/vss-tile-webapi:latest

Set-Location $currentFolder
Write-Host "Finished`n" -ForegroundColor Green
