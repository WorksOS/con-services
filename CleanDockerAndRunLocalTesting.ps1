docker stop $(docker ps -a -q)
docker rm $(docker ps -a -q)
docker rmi $(docker images -q --filter "dangling=true")
docker rmi 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-masterdataconsumer-db
docker rmi 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-masterdataconsumer
docker rmi vsstagfileauthservice_accepttest
docker rmi vsstagfileauthservice_webapi

$Cmd = 'aws'
$Args = 'ecr', 'get-login'

$LoginID = &$Cmd $Args

$LoginID = $LoginID -replace "-e none", " "
Write $LoginID
Invoke-Expression $LoginID

& .\RunLocalTesting.bat