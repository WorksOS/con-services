dotnet restore --no-cache
dotnet pack -c Release
nuget push .\bin\release\*.nupkg qATxVIHO5rIPF3K7 -so https://packages.vspengg.com/ -verbosity detailed