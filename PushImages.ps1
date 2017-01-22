param (
  [Parameter(Mandatory=$true)][string]$fullVersion
)
& echo Dumping Var
& echo $env:AWS_CONFIG_FILE
& echo Dumping home
& echo $env:HOME
& echo Dumping homepath
& echo $env:HOMEPATH
$command = '$(aws ecr get-login --region us-west-2 --profile vss-grant)'
Invoke-Expression $command
& docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-webapi:${fullVersion}
& docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-webapi
& docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-webapi:${fullVersion}
& docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-webapi:latest