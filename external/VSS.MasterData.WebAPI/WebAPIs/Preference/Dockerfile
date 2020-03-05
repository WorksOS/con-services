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
#COPY Preference /build/Preference/
#COPY Repositories/PreferenceRepository /build/Repositories/PreferenceRepository/
#COPY Preference.Tests /build/Preference.Tests/
#COPY PreferenceRepository.Tests /build/PreferenceRepository.Tests/
#COPY ClientModel /build/ClientModel/
#COPY DbModel /build/DbModel/
#COPY Data.MySql /build/Data.MySql/
#COPY Transactions /build/Transactions/
#COPY Interfaces /build/Interfaces/
#COPY Data.Confluent /build/Data.Confluent/
#COPY KafkaModel /build/KafkaModel/
#COPY Utilities /build/Utilities/

# Compile WebAPI
WORKDIR /build/WebAPIs/Preference
RUN dotnet publish --source https://visionlink.pkgs.visualstudio.com/_packaging/VSS/nuget/v3/index.json \
    --source https://api.nuget.org/v3/index.json \
    -o /WebAPIs/Preference \
    --framework $Framework -c $Configuration

# Compile tests
WORKDIR /build/WebAPI.Tests/Preference.Tests
RUN dotnet build --source https://visionlink.pkgs.visualstudio.com/_packaging/VSS/nuget/v3/index.json \
    --source https://api.nuget.org/v3/index.json \
    -o /WebAPI.Tests/Preference.Tests

WORKDIR /build/Repositories.Tests/PreferenceRepository.Tests
RUN dotnet build --source https://visionlink.pkgs.visualstudio.com/_packaging/VSS/nuget/v3/index.json \
    --source https://api.nuget.org/v3/index.json \
    -o /Repositories.Tests/PreferenceRepository.Tests

# Run those tests
WORKDIR /build/WebAPI.Tests/Preference.Tests
RUN dotnet test Preference.Tests.csproj \
    --no-build --logger:trx \
    -o /WebAPI.Tests/Preference.Tests \
    -r /results

WORKDIR /build/Repositories.Tests/PreferenceRepository.Tests
RUN dotnet test PreferenceRepository.Tests.csproj \
    --no-build --logger:trx \
    -o /Repositories.Tests/PreferenceRepository.Tests \
    -r /results


#################################
# Runtime section
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS runtime

# Copy built artifacts from last stage into runtime container
COPY --from=build /WebAPIs/Preference/ /Preference
COPY --from=build /results/ /results
WORKDIR /Preference
EXPOSE 80
ENTRYPOINT [ "dotnet", "VSS.MasterData.WebAPI.Preference.dll" ]