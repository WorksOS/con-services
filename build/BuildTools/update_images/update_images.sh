#!/bin/bash -x
#sleep while dockerd starts usually only takes a second
sleep 10
eval $(aws ecr get-login --no-include-email --region us-west-2)
docker pull microsoft/dotnet:2.1-sdk 
docker pull microsoft/dotnet:2.0-sdk 
docker pull microsoft/dotnet:2.1-runtime
docker pull microsoft/dotnet:2.1-aspnetcore-runtime
docker pull microsoft/dotnet:2.0-runtime  
docker pull node:8
docker tag microsoft/dotnet:2.1-sdk 300213723870.dkr.ecr.us-west-2.amazonaws.com/base-images:dotnet-2.1-sdk
docker tag microsoft/dotnet:2.0-sdk 300213723870.dkr.ecr.us-west-2.amazonaws.com/base-images:dotnet-2.0-sdk
docker tag microsoft/dotnet:2.1-runtime 300213723870.dkr.ecr.us-west-2.amazonaws.com/base-images:dotnet-2.1-runtime
docker tag microsoft/dotnet:2.0-runtime 300213723870.dkr.ecr.us-west-2.amazonaws.com/base-images:dotnet-2.0-runtime
docker tag microsoft/dotnet:2.1-aspnetcore-runtime 300213723870.dkr.ecr.us-west-2.amazonaws.com/base-images:dotnet-2.1-aspnetcore-runtime
docker tag node:8 300213723870.dkr.ecr.us-west-2.amazonaws.com/base-images:node-8
docker push 300213723870.dkr.ecr.us-west-2.amazonaws.com/base-images:dotnet-2.1-sdk
docker push 300213723870.dkr.ecr.us-west-2.amazonaws.com/base-images:dotnet-2.0-sdk
docker push 300213723870.dkr.ecr.us-west-2.amazonaws.com/base-images:dotnet-2.1-runtime
docker push 300213723870.dkr.ecr.us-west-2.amazonaws.com/base-images:dotnet-2.0-runtime
docker push 300213723870.dkr.ecr.us-west-2.amazonaws.com/base-images:dotnet-2.1-aspnetcore-runtime
docker push 300213723870.dkr.ecr.us-west-2.amazonaws.com/base-images:node-8
echo "finished update killing supervisor and stopping"
pkill -f supervisord