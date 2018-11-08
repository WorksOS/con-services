$command = & aws ecr get-login --region us-west-2 --profile vss-grant
$fixedCommand = $command.Replace("https://","")
Invoke-Expression $fixedCommand

& docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-flyway-win:latest ./
& docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-flyway-win:latest
& docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-flyway-win:latest
