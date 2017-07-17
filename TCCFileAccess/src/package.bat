dotnet restore --no-cache VSS.TCCFileAccess.csproj
dotnet pack VSS.TCCFileAccess.csproj -c Release
nuget push .\bin\release\*.nupkg -so https://packages.vspengg.com/ qATxVIHO5rIPF3K7 -verbosity detailed