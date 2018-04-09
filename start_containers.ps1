Write-Host "Logging in to image host" -ForegroundColor DarkGray
Invoke-Expression -Command (aws ecr get-login --no-include-email --profile vss-grant --region us-west-2)

docker-compose pull
docker-compose up --build --detach