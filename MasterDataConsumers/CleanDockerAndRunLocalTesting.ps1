& .\UpdateEnvFileIpAddress.ps1

# Setting the COMPOSE_CONVERT_WINDOWS_PATHS environment variable before trying 
# to bring up the containers seems to fix the docker-compose bug reported here: https://github.com/docker/for-win/issues/1829
$Env:COMPOSE_CONVERT_WINDOWS_PATHS=1

docker stop $(docker ps -a -q)
docker rm $(docker ps -a -q)
docker rmi $(docker images -q --filter "dangling=true")
docker rmi 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-masterdataconsumer-db
docker rmi 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-masterdataconsumer
docker rmi 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-webapi
docker rmi 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-mockproject-webapi:latest-linux

$Cmd = 'aws'
$Args = 'ecr', 'get-login'

$LoginID = &$Cmd $Args

$LoginID = $LoginID -replace "-e none", " "
Write-Output $LoginID
Invoke-Expression $LoginID

& .\RunLocalTesting.bat