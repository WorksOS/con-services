FROM 276986344560.dkr.ecr.us-west-2.amazonaws.com/base-images:dotnet-2.1-sdk as builder
ARG SERVICE_PATH

COPY . /build/
WORKDIR /build

RUN dotnet publish ./$SERVICE_PATH/src/WebApi/VSS.Productivity3D.WebApi.csproj -o /artifacts -f netcoreapp2.1 --runtime linux-x64

FROM 276986344560.dkr.ecr.us-west-2.amazonaws.com/base-images:dotnet-2.1-runtime

WORKDIR /app

ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

COPY --from=builder /artifacts/ .

RUN ls -la /app

ENTRYPOINT ["dotnet", "VSS.Productivity3D.WebApi.dll"]
