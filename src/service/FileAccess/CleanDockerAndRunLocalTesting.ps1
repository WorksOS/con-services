[console]::ResetColor()

# If regularly re running the script on the same service it's faster to opt out of setting the environment vars each time.
IF (-not($args -contains "--no-vars")) { & .\set-environment-variables.ps1 }

Write-Host "Stopping Docker containers"
docker ps -q | ForEach-Object { docker stop $_ }

# This is not ideal; but too often the containers fail to start due to drive or volume errors on the existing containers.
Write-Host "Removing old application containers"
docker ps -aq --filter "name=fileaccess_" | ForEach-Object { docker rm $_ }

Write-Host "Logging in to image host"
Invoke-Expression -Command (aws ecr get-login --no-include-email --region us-west-2)

Write-Host "Building solution"
Invoke-Expression "& .\build.ps1"
Set-Location AcceptanceTests\scripts
Invoke-Expression ".\deploy_win.bat"

Write-Host "Composing containers"
Set-Location $PSScriptRoot
Invoke-Expression "docker-compose rm -f"
Invoke-Expression "docker-compose -f docker-compose-local.yml pull"
Invoke-Expression "docker-compose -f docker-compose-local.yml up --build | Tee-Object -FilePath c:\temp\Productivity3D.FileAccess.WebApi.log"

IF (-not $?) {
    Write-Host "Error: Environment failed to start" -ForegroundColor Red
    Exit 1
}

Write-Host "Finished" -ForegroundColor Green
