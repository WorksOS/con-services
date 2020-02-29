#!/bin/bash
docker pull microsoft/dotnet:2.1-sdk 
docker pull microsoft/dotnet:2.0-sdk 
docker pull mcr.microsoft.com/dotnet/core/sdk:3.1
docker pull microsoft/dotnet:2.1-runtime
docker pull microsoft/dotnet:2.0-runtime  
docker pull mcr.microsoft.com/dotnet/core/runtime:3.1  
docker pull node:8
docker tag microsoft/dotnet:2.1-sdk 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.1-sdk
docker tag microsoft/dotnet:2.0-sdk 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.0-sdk
docker tag mcr.microsoft.com/dotnet/core/sdk:3.1 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-3.1-sdk
docker tag microsoft/dotnet:2.1-runtime 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.1-runtime
docker tag microsoft/dotnet:2.0-runtime 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.0-runtime
docker tag mcr.microsoft.com/dotnet/core/runtime:3.1 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-3.1-runtime
docker tag node:8 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:node-8
docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.1-sdk
docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.0-sdk
docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-3.1-sdk
docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.1-runtime
docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.0-runtime
docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-3.1-runtime
docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:node-8