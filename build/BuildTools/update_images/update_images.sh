#!/bin/bash -x
#sleep while dockerd starts usually only takes a second
sleep 10
eval $(aws ecr get-login --no-include-email --region us-west-2)
docker pull microsoft/dotnet:2.1-sdk 
docker pull microsoft/dotnet:2.0-sdk 
docker pull mcr.microsoft.com/dotnet/core/sdk:3.1
docker pull microsoft/dotnet:2.1-runtime
docker pull microsoft/dotnet:2.1-aspnetcore-runtime
docker pull microsoft/dotnet:2.0-runtime
docker pull mcr.microsoft.com/dotnet/core/runtime:3.1
docker pull mcr.microsoft.com/dotnet/core/aspnet:3.1
docker pull node:8

docker tag microsoft/dotnet:2.1-sdk 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.1-sdk
docker tag microsoft/dotnet:2.0-sdk 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.0-sdk
docker tag mcr.microsoft.com/dotnet/core/sdk:3.1 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-3.1-sdk
docker tag microsoft/dotnet:2.1-runtime 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.1-runtime
docker tag microsoft/dotnet:2.1-aspnetcore-runtime 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.1-aspnetcore-runtime
docker tag microsoft/dotnet:2.0-runtime 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.0-runtime
docker tag mcr.microsoft.com/dotnet/core/runtime:3.1 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-3.1-runtime
docker tag mcr.microsoft.com/dotnet/core/aspnet:3.1 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-3.1-aspnetcore-runtime
docker tag node:8 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:node-8
docker build -f ./Dockerfile.TRex.build -t 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:trex-3.1-build .
docker build -f ./Dockerfile.TRex.runtime -t 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:trex-3.1-runtime .

docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.1-sdk
docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.0-sdk
docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-3.1-sdk
docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.1-runtime
docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.1-aspnetcore-runtime
docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.0-runtime
docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-3.1-runtime
docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-3.1-aspnetcore-runtime
docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:node-8
docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:trex-3.1-build
docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:trex-3.1-runtime

echo "finished update killing supervisor and stopping"
pkill -f supervisord