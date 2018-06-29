FROM microsoft/dotnet:2.1-sdk as builder

COPY . /build/
WORKDIR /build

RUN chmod 777 *.sh

RUN ["/bin/sh", "/build/build.sh"]

FROM microsoft/dotnet:2.1-runtime

#This is required for the webpi to run properly
RUN apt-get update && apt-get install -y \
    libunwind8 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

COPY --from=builder /build/artifacts/WebApi .

ENTRYPOINT ["dotnet", "VSS.Productivity3D.TagFileAuth.WebAPI.dll"]
