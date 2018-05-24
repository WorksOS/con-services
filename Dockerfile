FROM jenkins-cache-docker-registry.jenkins/dotnet:2.1-sdk-alpine as builder

COPY . /build/
WORKDIR /build

RUN chmod 777 *.sh

RUN ["/bin/sh", "build.sh"]
RUN ["/bin/sh", "unittests.sh"]




FROM jenkins-cache-docker-registry.jenkins/dotnet:2.1-runtime-alpine

WORKDIR /app

ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

COPY --from=builder /build/artifacts/ProjectWebApi .
RUN ls -la


ENTRYPOINT ["dotnet", "vss.masterdata.project.webapi.dll"]
