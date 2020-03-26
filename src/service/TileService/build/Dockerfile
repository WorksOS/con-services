FROM 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-3.1-sdk as builder

COPY . /build/
WORKDIR /build
ARG SERVICE_PATH

RUN dotnet publish /nowarn:1591 ./$SERVICE_PATH/src/VSS.Tile.Service.WebApi/VSS.Tile.Service.WebApi.csproj -o /build/artifacts/VSS.Tile.Service.WebApi --framework netcoreapp3.1 --runtime linux-x64

FROM 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-3.1-aspnetcore-runtime

#This is required for the webpi to run properly
RUN apt-get update && apt-get install -y \
    libunwind8 \
    && rm -rf /var/lib/apt/lists/*

# Required for GDI drawing during tile generation
RUN apt-get update && apt-get install --assume-yes \
    libc6-dev \
    wget

RUN wget https://download.mono-project.com/repo/ubuntu/pool/main/libg/libgdiplus/libgdiplus_5.6-0xamarin3+debian9b1_amd64.deb && \
    apt-get install -y ./libgdiplus_5.6-0xamarin3+debian9b1_amd64.deb

ENV CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
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
