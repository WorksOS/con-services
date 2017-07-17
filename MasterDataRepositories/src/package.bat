dotnet restore --no-cache VSS.MasterData.Repositories.csproj
dotnet pack VSS.MasterData.Repositories.csproj -c Release
nuget push .\bin\release\*.nupkg -so https://packages.vspengg.com/ qATxVIHO5rIPF3K7 -verbosity detailed