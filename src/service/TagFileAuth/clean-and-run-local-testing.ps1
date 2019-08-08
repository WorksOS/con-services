[console]::ResetColor()

# If regularly re running the script on the same service it's faster to opt out of setting the environment vars each time.
IF ($args -contains "--set-vars") { & ./set-environment-variables.ps1 }

# Set KAFKA_ADVERTISED_HOST_NAME for local testing.
& ../../config/apply-kafka-config-local.ps1

Write-Host "Stopping Docker containers"
docker ps -q | ForEach-Object { docker stop $_ }

# This is not ideal; but too often the containers fail to start due to drive or volume errors on the existing containers.
Write-Host "Removing old application containers"
docker ps -aq --filter "name=tagfileauth_" | ForEach-Object { docker rm $_ }

Write-Host "Connecting to image host" -ForegroundColor DarkGray
Invoke-Expression -Command (aws ecr get-login --no-include-email --region us-west-2)

& .\RunLocalTesting.bat