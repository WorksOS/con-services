$dockerComposeConfig = "docker-compose-local"

Write-Host "Removing old service application containers" -ForegroundColor DarkGray
Invoke-Expression "docker-compose --file $dockerComposeConfig.yml down"

Write-Host "Connecting to image host" -ForegroundColor DarkGray
Invoke-Expression -Command (aws ecr get-login --no-include-email --region us-west-2)

IF (-not $?) {
    Write-Host "Error: Logging in to AWS, won't pull latest images for container dependancies." -ForegroundColor Red
}

Remove-Item -Path ./artifacts -Recurse -Force -ErrorAction Ignore
New-Item -ItemType directory ./artifacts/logs | out-null

Write-Host "Building image dependencies" -ForegroundColor DarkGray
Invoke-Expression "docker-compose --file $dockerComposeConfig.yml pull"

Write-Host "Building Docker containers" -ForegroundColor DarkGray
Invoke-Expression "docker-compose --file $dockerComposeConfig.yml up --build --detach > ${PSScriptRoot}/artifacts/logs/output.log"

IF (-not $?) {
    Write-Host "Error: Environment failed to start" -ForegroundColor Red
    EXIT 1
}

Write-Host "Finished" -ForegroundColor Green
