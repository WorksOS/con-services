dotnet restore --no-cache VSS.MasterData.Proxies.csproj
dotnet pack VSS.MasterData.Proxies.csproj -c Release
nuget push .\bin\release\*.nupkg qATxVIHO5rIPF3K7 -so https://packages.vspengg.com/ -verbosity detailed