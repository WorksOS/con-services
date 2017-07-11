dotnet restore --no-cache KafkaConsumer.csproj
dotnet pack KafkaConsumer.csproj -c Release
nuget push .\bin\release\*.nupkg -so https://packages.vspengg.com/ qATxVIHO5rIPF3K7 -verbosity detailed