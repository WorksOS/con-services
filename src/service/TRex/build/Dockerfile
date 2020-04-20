ARG BUILD_CONTAINER
FROM ${BUILD_CONTAINER} as build_container

# Now create runtime container
# Why bash? it seems to be needed for ignite to run (maybe a dependency?)
FROM 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:trex-3.1-runtime as runtime_container
ARG COMPONENT

ENV TPAAS_AUTH_URL=https://identity-stg.trimble.com/i/oauth2 \
    TPAAS_APP_TOKEN=MGh1X25tYXlEQWFkMFdpY1hDekVHVTE3U2ZVYTppVWN3eEZ1cFRDRWFsaFVFOTRwWGhkVVNEa0Vh \
    COORDINATE_SERVICE_URL=https://api-stg.trimble.com/t/trimble.com/coordinates/1.0

#Bring libs accross
COPY --from=build_container /trex/libs/ /trex/libs/

WORKDIR /trex

# Make sure this is done in the final container
RUN touch prometheusConfig.yaml

# Copy built artifacts from common build container into runtime container
COPY --from=build_container /trex/${COMPONENT}/ /trex/

