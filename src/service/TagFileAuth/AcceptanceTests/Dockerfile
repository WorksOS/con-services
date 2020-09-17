FROM 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-3.1-sdk

COPY . /app
WORKDIR /app

ENTRYPOINT ["/bin/bash", "runtests.sh"]
