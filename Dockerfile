FROM microsoft/dotnet:2.1-sdk as builder

COPY . /build/
WORKDIR /build

RUN chmod 777 *.sh

RUN ["/bin/sh", "build.sh"]

FROM microsoft/dotnet:2.1-runtime

#This is required for the webpi to run properly
RUN apt install libunwind8

WORKDIR /app

ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

COPY --from=builder /build/artifacts/ProjectWebApi .

ENTRYPOINT ["dotnet", "VSS.MasterData.Project.WebAPI.dll"]
