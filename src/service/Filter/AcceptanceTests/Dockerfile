ARG APP_BUILD_IMAGE=registry-docker-registry.jenkins:5000/dotnet:2.1-sdk

FROM ${APP_BUILD_IMAGE}
COPY . /app
WORKDIR /app

# Remove CR (Carrage Return '\r') in sh files
RUN sed -i 's/\r$//' rm_cr.sh
RUN chmod 775 rm_cr.sh
RUN ["/bin/bash", "rm_cr.sh"]

# Create the mount point to hold volume from host for test results
VOLUME testresults

ENTRYPOINT ["/bin/bash", "runtests.sh"]
