#!/bin/bash
docker pull microsoft/dotnet:2.1-sdk 
docker pull microsoft/dotnet:2.0-sdk 
docker pull microsoft/dotnet:2.1-runtime
docker pull microsoft/dotnet:2.0-runtime  
docker pull node:8
docker tag microsoft/dotnet:2.1-sdk 276986344560.dkr.ecr.us-west-2.amazonaws.com/base-images:dotnet-2.1-sdk
docker tag microsoft/dotnet:2.0-sdk 276986344560.dkr.ecr.us-west-2.amazonaws.com/base-images:dotnet-2.0-sdk
docker tag microsoft/dotnet:2.1-runtime 276986344560.dkr.ecr.us-west-2.amazonaws.com/base-images:dotnet-2.1-runtime
docker tag microsoft/dotnet:2.0-runtime 276986344560.dkr.ecr.us-west-2.amazonaws.com/base-images:dotnet-2.0-runtime
docker tag node:8 276986344560.dkr.ecr.us-west-2.amazonaws.com/base-images:node-8
docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/base-images:dotnet-2.1-sdk
docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/base-images:dotnet-2.0-sdk
docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/base-images:dotnet-2.1-runtime
docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/base-images:dotnet-2.0-runtime
docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/base-images:node-8