param (
  [Parameter(Mandatory=$true)][string]$fullVersion
)
$command = & aws ecr get-login --region us-west-2 --profile vss-grant
$fixedCommand = $command.Replace("https://","")
$fixedCommand = $fixedCommand -replace "-e none", " "
Invoke-Expression $fixedCommand
& docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-webapi:${fullVersion}
#& docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-webapi:latest
& docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-webapi:${fullVersion}
#& docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-webapi:latest