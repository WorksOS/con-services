param(
  [string]$branch = ""
  )
 

"Logging in to image host"
Invoke-Expression -Command (aws ecr get-login --no-include-email --profile vss-grant --region us-west-2)

docker-compose pull
if ($branch -eq "Release" -or $branch -eq "master") {
  Write-Host "Building $branch containers for testing against alpha raptor"
  docker-compose -f docker-compose-alpha.yml up --build -d 2>&1 > container.txt
} else {
  Write-Host "Building containers for testing against dev raptor"
  docker-compose up --build -d 2>&1 > container.txt
}


# Parse the docker-compose output and retrieve the container name.
$containerName = Get-Content container.txt | select-string -Pattern "[a-zA-Z0-9-_-]+_webapi_\d"  | Select-Object -ExpandProperty Matches |  Select-Object -First 1 -ExpandProperty Value

$containerName > container.txt
Write-Host "Container name: $containerName"