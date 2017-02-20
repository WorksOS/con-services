dotnet restore
dotnet pack -c Release
nuget push .\bin\release\*.nupkg -s https://packages.vspengg.com/ qATxVIHO5rIPF3K7 -verbosity detailed