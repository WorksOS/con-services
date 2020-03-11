Invoke-Expression -Command (aws ecr get-login --no-include-email --region us-west-2)
docker build -t 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:jenkinsslave-docker-k8s ./
docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:jenkinsslave-docker-k8s