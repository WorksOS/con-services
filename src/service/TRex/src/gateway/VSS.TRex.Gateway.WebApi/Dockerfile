#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
FROM mcr.microsoft.com/dotnet/core/sdk:3.1

#debconf shouldn't expect an interactive terminal
ENV DEBIAN_FRONTEND=noninteractive

#Slim based images don't have man dirs which will cause autoconf to fail during java install. Create the dirs here
RUN seq 1 8 | xargs -I{} mkdir -p /usr/share/man/man{}

RUN apt-get update && apt-get install -y \
    gnupg2

RUN curl -sL https://deb.nodesource.com/setup_8.x | bash -
RUN apt-get install -y \
    nodejs \ 
    npm

RUN wget -q https://packages.microsoft.com/config/ubuntu/19.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb

#RUN apt-get update && apt-get install -y \ 
#    dotnet-sdk-3.1

RUN apt-get update && apt-get install -y \
    software-properties-common

RUN wget -O- https://apt.corretto.aws/corretto.key | apt-key add - && \
    add-apt-repository 'deb https://apt.corretto.aws stable main'

RUN apt-get update && apt-get install -y \
    java-11-amazon-corretto-jdk

RUN apt-get update && apt-get install -y \
    libc6-dev

RUN apt-get update && apt-get install -y \
    wget

RUN wget https://download.mono-project.com/repo/ubuntu/pool/main/libg/libgdiplus/libgdiplus_5.6-0xamarin3+debian9b1_amd64.deb && \
    apt-get install -y ./libgdiplus_5.6-0xamarin3+debian9b1_amd64.deb

#Copy files from scm into build container and build
COPY . /app/build

WORKDIR /app
EXPOSE 80

#FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster AS build
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["src/service/TRex/src/gateway/VSS.TRex.Gateway.WebApi/VSS.TRex.Gateway.WebApi.csproj", "src/service/TRex/src/gateway/VSS.TRex.Gateway.WebApi/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Rendering/VSS.TRex.Rendering.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Rendering/"]
COPY ["src/service/TRex/src/interfaces/VSS.TRex.Rendering.Abstractions/VSS.TRex.Rendering.Abstractions.csproj", "src/service/TRex/src/interfaces/VSS.TRex.Rendering.Abstractions/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.GridFabric.Models/VSS.TRex.GridFabric.Models.csproj", "src/service/TRex/src/netstandard/VSS.TRex.GridFabric.Models/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Common/VSS.TRex.Common.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Common/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Types/VSS.TRex.Types.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Types/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Common.Exceptions/VSS.TRex.Common.Exceptions.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Common.Exceptions/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Logging/VSS.TRex.Logging.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Logging/"]
COPY ["src/Common/ConfigurationStore/src/VSS.ConfigurationStore.csproj", "ars/Common/ConfigurationStore/src/"]
COPY ["src/Common/LoggingExtensions/SerilogExtensions/VSS.Serilog.Extensions.csproj", "src/Common/LoggingExtensions/SerilogExtensions/"]
COPY ["src/Common/VSS.Common.Abstractions/src/VSS.Common.Abstractions.csproj", "src/Common/VSS.Common.Abstractions/src/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.IO/VSS.TRex.IO.csproj", "src/service/TRex/src/netstandard/VSS.TRex.IO/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.DI/VSS.TRex.DI.csproj", "src/service/TRex/src/netstandard/VSS.TRex.DI/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Common.Interfaces/VSS.TRex.Common.Interfaces.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Common.Interfaces/"]
COPY ["src/Common/Productivity3DModels/src/VSS.Productivity3D.Models.csproj", "src/Common/Productivity3DModels/src/"]
COPY ["src/service/3DP/Clients/Models/VSS.Productivity3D.Productivity3D.Models.csproj", "src/Service/3DP/Clients/Models/"]
COPY ["src/Common/MasterDataModels/src/VSS.MasterData.Models.csproj", "src/Common/MasterDataModels/src/"]
COPY ["src/Common/VSS.VisionLink.Interfaces/src/VSS.Visionlink.Interfaces.Core.csproj", "src/Common/VSS.VisionLink.Interfaces/src/"]
COPY ["src/Common/Pegasus/VSS.Pegasus.Client.Models/VSS.Pegasus.Client.Models.csproj", "src/Common/Pegasus/VSS.Pegasus.Client.Models/"]
COPY ["src/service/Filter/Clients/VSS.Productivity3D.Filter.Abstractions/VSS.Productivity3D.Filter.Abstractions.csproj", "src/service/Filter/Clients/VSS.Productivity3D.Filter.Abstractions/"]
COPY ["src/Common/MasterDataRepositories/src/VSS.MasterData.Repositories.csproj", "src/Common/MasterDataRepositories/src/"]
COPY ["src/Common/AWS.TransferProxy/src/VSS.AWS.TransferProxy.csproj", "src/Common/AWS.TransferProxy/src/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.SubGridTrees.Core/VSS.TRex.SubGridTrees.Core.csproj", "src/service/TRex/src/netstandard/VSS.TRex.SubGridTrees.Core/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Geometry/VSS.TRex.Geometry.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Geometry/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.SubGridTrees.Interfaces/VSS.TRex.SubGridTrees.Interfaces.csproj", "src/service/TRex/src/netstandard/VSS.TRex.SubGridTrees.Interfaces/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.ElevationSmoothing/VSS.TRex.DataSmoothing.csproj", "src/service/TRex/src/netstandard/VSS.TRex.ElevationSmoothing/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.SubGridTrees.Client/VSS.TRex.SubGridTrees.Client.csproj", "src/service/TRex/src/netstandard/VSS.TRex.SubGridTrees.Client/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Profiling.Models/VSS.TRex.Profiling.Models.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Profiling.Models/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Cells/VSS.TRex.Cells.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Cells/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.GridFabric/VSS.TRex.GridFabric.csproj", "src/service/TRex/src/netstandard/VSS.TRex.GridFabric/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.TAGFiles.Models/VSS.TRex.TAGFiles.Models.csproj", "src/service/TRex/src/netstandard/VSS.TRex.TAGFiles.Models/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.SubGridTrees.Server.Interfaces/VSS.TRex.SubGridTrees.Server.Interfaces.csproj", "src/service/TRex/src/netstandard/VSS.TRex.SubGridTrees.Server.Interfaces/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Storage.Interfaces/VSS.TRex.Storage.Interfaces.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Storage.Interfaces/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Storage.Models/VSS.TRex.Storage.Models.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Storage.Models/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.GridFabric.Interfaces/VSS.TRex.GridFabric.Interfaces.csproj", "src/service/TRex/src/netstandard/VSS.TRex.GridFabric.Interfaces/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.SiteModelChangeMaps.Interfaces/VSS.TRex.SiteModelChangeMaps.Interfaces.csproj", "src/service/TRex/src/netstandard/VSS.TRex.SiteModelChangeMaps.Interfaces/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Filters.Interfaces/VSS.TRex.Filters.Interfaces.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Filters.Interfaces/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Designs.Models/VSS.TRex.Designs.Models.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Designs.Models/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.SubGridTrees.Client.Interfaces/VSS.TRex.SubGridTrees.Client.Interfaces.csproj", "src/service/TRex/src/netstandard/VSS.TRex.SubGridTrees.Client.Interfaces/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Events.Models/VSS.TRex.Events.Models.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Events.Models/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Filters.Models/VSS.TRex.Filters.Models.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Filters.Models/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Caching.Interfaces/VSS.TRex.Caching.Interfaces.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Caching.Interfaces/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.SiteModels.Interfaces/VSS.TRex.SiteModels.Interfaces.csproj", "src/service/TRex/src/netstandard/VSS.TRex.SiteModels.Interfaces/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.SurveyedSurfaces.Interfaces/VSS.TRex.SurveyedSurfaces.Interfaces.csproj", "src/service/TRex/src/netstandard/VSS.TRex.SurveyedSurfaces.Interfaces/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Events.Interfaces/VSS.TRex.Events.Interfaces.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Events.Interfaces/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Machines.Interfaces/VSS.TRex.Machines.Interfaces.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Machines.Interfaces/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Designs.Interfaces/VSS.TRex.Designs.Interfaces.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Designs.Interfaces/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Designs.TTM.Optimised/VSS.TRex.Designs.TTM.Optimised.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Designs.TTM.Optimised/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Alignments.Interfaces/VSS.TRex.Alignments.Interfaces.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Alignments.Interfaces/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Profiling.Interfaces/VSS.TRex.Profiling.Interfaces.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Profiling.Interfaces/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Pipelines/VSS.TRex.Pipelines.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Pipelines/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.SubGrids/VSS.TRex.SubGrids.csproj", "src/service/TRex/src/netstandard/VSS.TRex.SubGrids/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Filters/VSS.TRex.Filters.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Filters/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Machines/VSS.TRex.Machines.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Machines/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Designs/VSS.TRex.Designs.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Designs/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.ExistenceMaps.Interfaces/VSS.TRex.ExistenceMaps.Interfaces.csproj", "src/service/TRex/src/netstandard/VSS.TRex.ExistenceMaps.Interfaces/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Designs.SVL/VSS.TRex.Designs.SVL.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Designs.SVL/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Designs.TTM/VSS.TRex.Designs.TTM.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Designs.TTM/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.CoordinateSystems/VSS.TRex.CoordinateSystems.csproj", "src/service/TRex/src/netstandard/VSS.TRex.CoordinateSystems/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.HttpClients/VSS.TRex.HttpClients.csproj", "src/service/TRex/src/netstandard/VSS.TRex.HttpClients/"]
COPY ["src/Common/VSS.Tpaas.Client/src/VSS.Tpaas.Client.csproj", "src/Common/VSS.Tpaas.Client/src/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.CoordinateSystems.Models/VSS.TRex.CoordinateSystems.Models.csproj", "src/service/TRex/src/netstandard/VSS.TRex.CoordinateSystems.Models/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Events/VSS.TRex.Events.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Events/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.SubGrids.Interfaces/VSS.TRex.SubGrids.Interfaces.csproj", "src/service/TRex/src/netstandard/VSS.TRex.SubGrids.Interfaces/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.SurveyedSurfaces/VSS.TRex.SurveyedSurfaces.csproj", "src/service/TRex/src/netstandard/VSS.TRex.SurveyedSurfaces/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Caching/VSS.TRex.Caching.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Caching/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Profiling/VSS.TRex.Profiling.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Profiling/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Pipelines.Interfaces/VSS.TRex.Pipelines.Interfaces.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Pipelines.Interfaces/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.SubGridTrees.Server/VSS.TRex.SubGridTrees.Server.csproj", "src/service/TRex/src/netstandard/VSS.TRex.SubGridTrees.Server/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Compression/VSS.TRex.Compression.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Compression/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.SiteModels/VSS.TRex.SiteModels.csproj", "src/service/TRex/src/netstandard/VSS.TRex.SiteModels/"]
COPY ["src/service/TRex/src/gateway/VSS.TRex.Gateway.Common/VSS.TRex.Gateway.Common.csproj", "src/service/TRex/src/gateway/VSS.TRex.Gateway.Common/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Exports/VSS.TRex.Exports.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Exports/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Reports/VSS.TRex.Reports.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Reports/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Alignments/VSS.TRex.Alignments.csproj", "ssrc/service/TRex/rc/netstandard/VSS.TRex.Alignments/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Rendering.Implementations.Core2/VSS.TRex.Rendering.Implementations.Core2.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Rendering.Implementations.Core2/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Volumes/VSS.TRex.Volumes.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Volumes/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Volumes.Interfaces/VSS.TRex.Volumes.Interfaces.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Volumes.Interfaces/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.TAGFiles/VSS.TRex.TAGFiles.csproj", "src/service/TRex/src/netstandard/VSS.TRex.TAGFiles/"]
COPY ["src/service/TagFileAuth/Clients/Abstractions/VSS.Productivity3D.TagFileAuth.Abstractions.csproj", "src/service/TagFileAuth/Clients/Abstractions/"]
COPY ["src/service/TagFileAuth/Clients/Models/VSS.Productivity3D.TagFileAuth.Models.csproj", "src/service/TagFileAuth/Clients/Models/"]
COPY ["src/Common/MasterDataProxies/src/VSS.MasterData.Proxies.csproj", "src/Common/MasterDataProxies/src/"]
COPY ["src/Common/VSS.Authentication/VSS.Authentication.JWT/VSS.Authentication.JWT.csproj", "src/Common/VSS.Authentication/VSS.Authentication.JWT/"]
COPY ["src/service/TRex/src/interfaces/VSS.TRex.ConnectedSite.Gateway.Abstractions/VSS.TRex.ConnectedSite.Gateway.Abstractions.csproj", "src/service/TRex/src/interfaces/VSS.TRex.ConnectedSite.Gateway.Abstractions/"]
COPY ["src/Common/VSS.Common.Cache.MemoryCache/src/VSS.Common.Cache.MemoryCache.csproj", "src/Common/VSS.Common.Cache.MemoryCache/src/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.CellDatum/VSS.TRex.CellDatum.csproj", "src/service/TRex/src/netstandard/VSS.TRex.CellDatum/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Analytics/VSS.TRex.Analytics.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Analytics/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.QuantizedMesh/VSS.TRex.QuantizedMesh.csproj", "src/service/TRex/src/netstandard/VSS.TRex.QuantizedMesh/"]
COPY ["src/Common/WebApi.Common/src/VSS.WebApi.Common.csproj", "src/Common/WebApi.Common/src/"]
COPY ["src/Common/VSS.Common.ServiceDiscovery/src/VSS.Common.ServiceDiscovery.csproj", "src/Common/VSS.Common.ServiceDiscovery/src/"]
COPY ["src/Common/VSS.Common.Kubernetes/VSS.Common.Kubernetes.csproj", "src/Common/VSS.Common.Kubernetes/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.SiteModelChangeMaps/VSS.TRex.SiteModelChangeMaps.csproj", "src/service/TRex/src/netstandard/VSS.TRex.SiteModelChangeMaps/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.Storage/VSS.TRex.Storage.csproj", "src/service/TRex/src/netstandard/VSS.TRex.Storage/"]
COPY ["src/service/TRex/src/netstandard/VSS.TRex.ExistenceMaps/VSS.TRex.ExistenceMaps.csproj", "src/service/TRex/src/netstandard/VSS.TRex.ExistenceMaps/"]
RUN dotnet restore "src/service/TRex/src/gateway/VSS.TRex.Gateway.WebApi/VSS.TRex.Gateway.WebApi.csproj"
COPY . .
WORKDIR "/src/src/service/TRex/src/gateway/VSS.TRex.Gateway.WebApi"
RUN dotnet build "VSS.TRex.Gateway.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VSS.TRex.Gateway.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VSS.TRex.Gateway.WebApi.dll"]