param(
  [string]$branch = ""
)

Write-Host "Logging in to image host" -ForegroundColor DarkGray
Invoke-Expression -Command (aws ecr get-login --no-include-email --profile vss-grant --region us-west-2)

docker-compose pull
if ($branch -eq "Release" -or $branch -eq "master") {
  Write-Host "Building $branch containers for testing against alpha raptor"
  docker-compose --file docker-compose-alpha.yml up --build --detach
} else {
  Write-Host "Building containers for testing against dev raptor"
  docker-compose up --build --detach
}