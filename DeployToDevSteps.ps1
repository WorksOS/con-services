#dotnet restore --no-cache VSS.Productivity3D.Service.sln
#& build47.ps1
#Invoke-Expression -Command (aws ecr get-login --no-include-email --region us-west-2)
#docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-webapi:latest ./Artifacts/WebApi
#docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-webapi:latest

$env:DOCKER_TLS_VERIFY=1
$env:DOCKER_HOST="tcp://10.97.96.42:2376"
$currentFolder = (Resolve-Path .\).Path
$env:DOCKER_CERT_PATH="$currentFolder/certs"

docker-compose --verbose -f ./docker-compose-dev.yml down
docker-compose --verbose -f ./docker-compose-dev.yml pull
docker-compose --verbose -f ./docker-compose-dev.yml up --build -d
