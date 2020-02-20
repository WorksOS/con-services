Invoke-Expression -Command (aws ecr get-login --no-include-email --region us-west-2)
docker build -t 300213723870.dkr.ecr.us-west-2.amazonaws.com/base-images:jenkinsslave-core21 ./
docker push 300213723870.dkr.ecr.us-west-2.amazonaws.com/base-images:jenkinsslave-core21
