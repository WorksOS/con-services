Write-Host "Removing old application containers" -ForegroundColor "darkgray"
& docker-compose -f docker-compose-local.yml down

Write-Host "Connecting to image host" -ForegroundColor DarkGray
& aws ecr get-login --no-include-email --region us-west-2 --profile okta

If (-not $?) {
    Write-Host "Error: Logging in to AWS, won't pull latest images for container dependencies." -ForegroundColor Red
    Exit -1
}

& .\build.ps1

Write-Host "Composing containers" -ForegroundColor "darkgray"
& docker-compose -f docker-compose-local.yml up --build --force-recreate

IF (-not $?) {
    Write-Host "Error: Environment failed to start" -ForegroundColor Red
    Exit 1
}

Write-Host "Finished" -ForegroundColor Green
