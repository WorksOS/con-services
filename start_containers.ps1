"Logging in to image host"
Invoke-Expression -Command (aws ecr get-login --no-include-email --profile default --region us-west-2)

docker-compose pull
docker-compose up --build -d 2>&1 > container.txt

# Parse the docker-compose output and retrieve the container name.
$containerName = Get-Content container.txt | select-string -Pattern "\w*_webapi_\w*"  | Select-Object -ExpandProperty Matches |  Select-Object -First 1 -ExpandProperty Value

$containerName > container.txt
Write-Host $containerName