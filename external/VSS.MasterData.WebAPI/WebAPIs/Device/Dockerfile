###############################
# SDK Section
FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build

# Use these variables where needed
ARG Configuration=Release
ARG Framework=netcoreapp3.0
ARG TOKEN

RUN mkdir -p /results
RUN mkdir -p /build

# Get the nuget credential provider installed
RUN wget -qO- https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh | bash
ENV NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED true
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS {\"endpointCredentials\": [{\"endpoint\":\"https://visionlink.pkgs.visualstudio.com/_packaging/VSS/nuget/v3/index.json\", \"username\":\"vsts\", \"password\":\"$TOKEN\"}]}
RUN export DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0

# Copy source files for both WebAPI and tests
COPY ./ /build/
#COPY Device /build/Device/
#COPY Repositories/DeviceRepository /build/Repositories/DeviceRepository/
#COPY Repositories/AssetRepository /build/Repositories/AssetRepository/
#COPY Device.Tests /build/Device.Tests/
#COPY Device.Repository.Tests /build/Device.Repository.Tests/
#COPY ClientModel /build/ClientModel/
#COPY DbModel /build/DbModel/
#COPY Data.MySql /build/Data.MySql/
#COPY Transactions /build/Transactions/
#COPY Data.Confluent /build/Data.Confluent/
#COPY KafkaModel /build/KafkaModel/
#COPY Interfaces /build/Interfaces/
#COPY Utilities /build/Utilities/

# Compile WebAPI
WORKDIR /build/WebAPIs/Device
RUN dotnet publish --source https://visionlink.pkgs.visualstudio.com/_packaging/VSS/nuget/v3/index.json \
    --source https://api.nuget.org/v3/index.json \
    -o /WebAPIs/Device \
    --framework $Framework -c $Configuration

# Compile tests
WORKDIR /build/WebAPI.Tests/Device.Tests
RUN dotnet build --source https://visionlink.pkgs.visualstudio.com/_packaging/VSS/nuget/v3/index.json \
    --source https://api.nuget.org/v3/index.json \
    -o /WebAPI.Tests/Device.Tests

WORKDIR /build/Repositories.Tests/Device.Repository.Tests
RUN dotnet build --source https://visionlink.pkgs.visualstudio.com/_packaging/VSS/nuget/v3/index.json \
    --source https://api.nuget.org/v3/index.json \
    -o /Repositories.Tests/Device.Repository.Tests

# Run those tests
WORKDIR /build/WebAPI.Tests/Device.Tests
RUN dotnet test Device.Tests.csproj \
    --no-build --logger:trx \
    -o /WebAPI.Tests/Device.Tests \
    -r /results
#
WORKDIR /build/Repositories.Tests/Device.Repository.Tests
RUN dotnet test Device.Repository.Tests.csproj \
    --no-build --logger:trx \
    -o /Repositories.Tests/Device.Repository.Tests \
    -r /results


#################################
# Runtime section
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS runtime

# Copy built artifacts from last stage into runtime container
COPY --from=build /WebAPIs/Device/ /Device
COPY --from=build /results/ /results
# Get NewRelic .NET Agent installed
RUN mkdir -p /newrelic
ADD https://s3-us-west-2.amazonaws.com/newrelic-agents/netcorelinux/newrelic-netcore20-agent.deb /newrelic/nr.deb
# Add environment variable to configmap to enable NewRelic APM
# CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
   CORECLR_NEWRELIC_HOME=/usr/local/newrelic-netcore20-agent \
   CORECLR_PROFILER_PATH=/usr/local/newrelic-netcore20-agent/libNewRelicProfiler.so
RUN dpkg -i /newrelic/nr.deb
WORKDIR /Device
RUN rm -f newrelic.config
COPY CustomInstrumentation.xml /usr/local/newrelic-netcore20-agent/extensions/CustomInstrumentation.xml
EXPOSE 80
ENTRYPOINT [ "dotnet", "VSS.MasterData.WebAPI.Device.dll" ]