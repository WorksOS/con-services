dotnet restore --no-cache VSS.FlowJSHandler.csproj
dotnet pack VSS.FlowJSHandler.csproj -c Release
nuget push .\bin\release\*.nupkg -so https://packages.vspengg.com/ qATxVIHO5rIPF3K7 -verbosity detailed