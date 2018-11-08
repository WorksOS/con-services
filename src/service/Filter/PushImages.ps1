param (
  [Parameter(Mandatory=$true)][string]$fullVersion
)
$command = & aws ecr get-login --region us-west-2 --profile vss-grant
$fixedCommand = $command.Replace("https://","")
Invoke-Expression $fixedCommand

& docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-productivity3d-filter-webapi:${fullVersion}-win
& docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-productivity3d-filter-webapi:latest-win
& docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-productivity3d-filter-webapi:${fullVersion}-win
& docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-productivity3d-filter-webapi:latest-win
