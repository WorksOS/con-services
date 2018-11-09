FROM registry.k8s.vspengg.com:80/dotnet:2.0-sdk as builder
ARG SERVICE_PATH

COPY . /build/
WORKDIR /build

dotnet publish ./$SERVICE_PATH/src/MockProjectWebApi/MockProjectWebApi.csproj -o /artifacts -f netcoreapp2.0 --runtime linux-x64

FROM registry.k8s.vspengg.com:80/dotnet:2.0-runtime

WORKDIR /app

ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

COPY --from=builder /artifacts/ .

RUN ls -la /app

ENTRYPOINT ["dotnet", "src.dll"]
