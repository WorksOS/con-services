& .\UpdateEnvFileIpAddress.ps1

docker stop $(docker ps -a -q)
docker rm $(docker ps -a -q)
docker rmi $(docker images -q --filter "dangling=true")
docker rmi 276986344560.dkr.ecr.us-west-2.amazonaws.com/vssproductivity3dscheduler_webapi

$Cmd = 'aws'
$Args = 'ecr', 'get-login'

$LoginID = &$Cmd $Args

$LoginID = $LoginID -replace "-e none", " "
Write $LoginID
Invoke-Expression $LoginID

& .\RunLocalTesting.bat