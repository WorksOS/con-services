docker stop $(docker ps -a -q)
docker rm $(docker ps -a -q)
docker rmi $(docker images -q --filter "dangling=true")

$Cmd = 'aws'
$Args = 'ecr', 'get-login'
 
$LoginID = &$Cmd $Args
 
$LoginID = $LoginID -replace "-e none", " "
Write $LoginID
Invoke-Expression $LoginID

docker-compose pull
docker-compose up --build -d