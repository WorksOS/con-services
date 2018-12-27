Invoke-Expression -Command (aws ecr get-login --no-include-email --profile vss-grant --region us-west-2)
docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/base-images:jenkinsslave-docker-k8s ./
docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/base-images:jenkinsslave-docker-k8s