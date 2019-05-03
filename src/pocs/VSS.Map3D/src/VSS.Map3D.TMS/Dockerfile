FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["src/VSS.Map3D.TMS/VSS.Map3D.TMS.csproj", "src/VSS.Map3D.TMS/"]
RUN dotnet restore "src/VSS.Map3D.TMS/VSS.Map3D.TMS.csproj"
COPY . .
WORKDIR "/src/src/VSS.Map3D.TMS"
RUN dotnet build "VSS.Map3D.TMS.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "VSS.Map3D.TMS.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "VSS.Map3D.TMS.dll"]