FROM 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-3.1-sdk as builder
ARG SERVICE_PATH

COPY . /build/
WORKDIR /build

RUN dotnet publish /nowarn:CS1591 ./$SERVICE_PATH/Service/CustomerWebApi/CustomerWebApi.csproj -o /build/artifacts/Customer --framework netcoreapp3.1 --runtime linux-x64

FROM 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-3.1-runtime

#This is required for the webpi to run properly
RUN apt-get update && apt-get install -y \
    libunwind8 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

COPY --from=builder /build/artifacts/Customer .


ENTRYPOINT ["dotnet", "CustomerWbApi.dll"]
