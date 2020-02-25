$command = & aws ecr get-login --region us-west-2 --profile vss-grant
$fixedCommand = $command.Replace("https://","")
Invoke-Expression $fixedCommand

& docker build -t 300213723870.dkr.ecr.us-west-2.amazonaws.com/vss-flyway-win:latest ./
& docker push 300213723870.dkr.ecr.us-west-2.amazonaws.com/vss-flyway-win:latest
& docker rmi -f 300213723870.dkr.ecr.us-west-2.amazonaws.com/vss-flyway-win:latest
