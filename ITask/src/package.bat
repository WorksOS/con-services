dotnet restore --no-cache VSS.ITask.csproj
dotnet pack VSS.ITask.csproj -c Release
nuget push .\bin\release\*.nupkg -so https://packages.vspengg.com/ qATxVIHO5rIPF3K7 -verbosity detailed