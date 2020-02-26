param (
  [Parameter(Mandatory=$true)][string]$fullVersion
)
$command = & aws ecr get-login --region us-west-2 --no-include-email
$fixedCommand = $command.Replace("https://","")
Invoke-Expression $fixedCommand

& docker push 300213723870.dkr.ecr.us-west-2.amazonaws.com/ccss-mockproject-webapi:${fullVersion}
& docker push 300213723870.dkr.ecr.us-west-2.amazonaws.com/ccss-mockproject-webapi:latest
& docker rmi -f 300213723870.dkr.ecr.us-west-2.amazonaws.com/ccss-mockproject-webapi:${fullVersion}
& docker rmi -f 300213723870.dkr.ecr.us-west-2.amazonaws.com/ccss-mockproject-webapi:latest
