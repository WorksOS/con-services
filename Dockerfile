FROM microsoft/dotnet:2.0-sdk as builder

COPY . /build/
WORKDIR /build

RUN chmod 777 *.sh

RUN ["/bin/sh", "build.sh"]

FROM microsoft/dotnet:2.0-runtime

#This is required for the webpi to run properly
RUN apt-get update && apt-get install -y \
    libunwind8 \
    && rm -rf /var/lib/apt/lists/*


ENV CORECLR_ENABLE_PROFILING=1 \
    CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
    CORECLR_NEWRELIC_HOME=/usr/local/newrelic-netcore20-agent \
    CORECLR_PROFILER_PATH=/usr/local/newrelic-netcore20-agent/libNewRelicProfiler.so 

COPY ./newrelic /newrelic/

RUN dpkg -i /newrelic/newrelic-netcore20*.deb

RUN ls -la /usr/local/newrelic-netcore20-agent

WORKDIR /app

ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

COPY --from=builder /build/artifacts/TagFileHarvester .
RUN ls -la

ENTRYPOINT ["dotnet", "TagFileHarvester.netcore.dll"]
