dotnet restore --no-cache
dotnet pack -c Debug
nuget push .\bin\Debug\*.nupkg -s https://packages.vspengg.com/ qATxVIHO5rIPF3K7 -verbosity detailed