ARG APP_BUILD_IMAGE=registry-docker-registry.jenkins:5000/dotnet:2.1-sdk

FROM ${APP_BUILD_IMAGE}
COPY . /app
WORKDIR /app

# Create the mount point to hold volume from host for test results
VOLUME testresults

ENTRYPOINT ["/bin/bash", "runtests.sh"]
