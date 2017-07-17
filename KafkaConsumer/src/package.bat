dotnet restore --no-cache VSS.KafkaConsumer.csproj
dotnet pack VSS.KafkaConsumer.csproj -c Release
nuget push .\bin\release\*.nupkg -so https://packages.vspengg.com/ qATxVIHO5rIPF3K7 -verbosity detailed