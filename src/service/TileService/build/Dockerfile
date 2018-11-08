FROM registry.k8s.vspengg.com:80/dotnet:2.1-sdk as builder

COPY . /build/
WORKDIR /build
ARG SERVICE_PATH

RUN dotnet publish ./$SERVICE_PATH/src/VSS.Tile.Service.WebApi/VSS.Tile.Service.WebApi.csproj -o /build/artifacts/VSS.Tile.Service.WebApi --framework netcoreapp2.1 --runtime linux-x64

FROM registry.k8s.vspengg.com:80/dotnet:2.1-runtime

ENV CORECLR_ENABLE_PROFILING=1 \
    CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
    CORECLR_NEWRELIC_HOME=/usr/local/newrelic-netcore20-agent \
    CORECLR_PROFILER_PATH=/usr/local/newrelic-netcore20-agent/libNewRelicProfiler.so 

COPY ./build/newrelic /newrelic/

RUN dpkg -i /newrelic/newrelic-netcore20*.deb

RUN ls -la /usr/local/newrelic-netcore20-agent

WORKDIR /app

ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

COPY --from=builder /build/artifacts/VSS.Tile.Service.WebApi .


ENTRYPOINT ["dotnet", "VSS.Tile.Service.WebApi.dll"]
