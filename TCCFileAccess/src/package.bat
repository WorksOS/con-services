dotnet restore --no-cache TCCFileAccess.csproj
dotnet pack TCCFileAccess.csproj -c Release
nuget push .\bin\release\*.nupkg -so https://packages.vspengg.com/ qATxVIHO5rIPF3K7 -verbosity detailed