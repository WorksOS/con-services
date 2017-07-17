dotnet restore --no-cache VSS.MasterData.Models.csproj
dotnet pack VSS.MasterData.Models.csproj -c Release
nuget push .\bin\release\*.nupkg -so https://packages.vspengg.com/ qATxVIHO5rIPF3K7 -verbosity detailed