dotnet restore --no-cache MasterDataModels.csproj
dotnet pack MasterDataModels.csproj -c Release
nuget push .\bin\release\*.nupkg qATxVIHO5rIPF3K7 -so https://packages.vspengg.com/ -verbosity detailed