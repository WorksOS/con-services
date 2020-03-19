param (
  [Parameter(Mandatory=$true)][string]$fullVersion
)
$command = & aws ecr get-login --region us-west-2
$fixedCommand = $command.Replace("https://","")
$fixedCommand = $fixedCommand -replace "-e none", " "
Invoke-Expression $fixedCommand
& docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-raptor-webapi:${fullVersion}
#& docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-raptor-webapi:latest
& docker rmi -f 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-raptor-webapi:${fullVersion}
#& docker rmi -f 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-raptor-webapi:latest