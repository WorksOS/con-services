#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-3.1-sdk AS base
WORKDIR /app
EXPOSE 80

FROM 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-3.1-sdk AS build
WORKDIR /src
COPY ["CustomerWebApi/CustomerWebApi.csproj", "CustomerWebApi/"]
RUN dotnet restore "CustomerWebApi/CustomerWebApi.csproj"
COPY . .
WORKDIR "/src/CustomerWebApi"
RUN dotnet build "CustomerWebApi.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "CustomerWebApi.csproj" -c Debug -o /app/publish 
ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CustomerWebApi.dll"]