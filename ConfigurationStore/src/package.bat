dotnet restore --no-cache VSS.ConfigurationStore.csproj
dotnet pack VSS.ConfigurationStore.csproj -c Release
nuget push .\bin\release\*.nupkg -so https://packages.vspengg.com/ qATxVIHO5rIPF3K7 -verbosity detailed