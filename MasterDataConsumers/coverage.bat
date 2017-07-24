echo "Calculating coverage with OpenCover"
dotnet restore test\UnitTests\MasterDataConsumerTests\MasterDataConsumerTests.csproj
dotnet build test\UnitTests\MasterDataConsumerTests\MasterDataConsumerTests.csproj
.\tools\opencover.console.exe -target:"dotnet.exe"  -targetargs:"test test\UnitTests\MasterDataConsumerTests\MasterDataConsumerTests.csproj" -mergeoutput -hideskipped:File -output:coverage.xml -register:user -filter:"+[VSS*]*" 
echo "Generating HTML report"
.\tools\OpenCoverToCoberturaConverter.exe -input:coverage.xml -output:outputCobertura.xml -sources:%WORKSPACE%