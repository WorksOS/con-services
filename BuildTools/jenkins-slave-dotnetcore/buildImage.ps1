Invoke-Expression -Command (aws ecr get-login --no-include-email --profile vss-grant --region us-west-2)
docker build -t registry.k8s.vspengg.com:80/vss-jenkinsslave:core21 ./
docker push registry.k8s.vspengg.com:80/vss-jenkinsslave:core21
