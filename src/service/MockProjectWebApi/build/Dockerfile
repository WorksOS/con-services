FROM 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.0-sdk as builder
ARG SERVICE_PATH

COPY . /build/
WORKDIR /build

RUN dotnet publish ./$SERVICE_PATH/MockProjectWebApi.csproj -o /artifacts -f netcoreapp2.0 --runtime linux-x64

FROM 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.0-runtime

WORKDIR /app

ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

COPY --from=builder /artifacts/ .

RUN ls -la /app

ENTRYPOINT ["dotnet", "src.dll"]
