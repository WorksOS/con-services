echo "Calculating coverage with OpenCover"
dotnet restore test\UnitTests\MasterDataConsumerTests\VSS.Productivity3D.MasterDataConsumer.Tests.csproj
dotnet build test\UnitTests\MasterDataConsumerTests\VSS.Productivity3D.MasterDataConsumer.Tests.csproj
.\tools\opencover.console.exe -target:"dotnet.exe"  -targetargs:"test test\UnitTests\MasterDataConsumerTests\VSS.Productivity3D.MasterDataConsumer.Tests.csproj" -mergeoutput -hideskipped:File -output:coverage.xml -register:user -oldstyle
echo "Generating HTML report"
.\tools\OpenCoverToCoberturaConverter.exe -input:coverage.xml -output:outputCobertura.xml -sources:%WORKSPACE%