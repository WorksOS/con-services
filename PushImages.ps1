$command = '$(aws ecr get-login --region us-west-2 --profile vss-grant)'
Invoke-Expression $command
& docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-webapi:${fullVersion}
& docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-webapi
& docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-webapi:${fullVersion}
& docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-webapi:latest