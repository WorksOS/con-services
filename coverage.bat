echo "Calculating coverage with OpenCover"
dotnet restore NugetPackages.sln
dotnet build NugetPackages.sln

.\tools\opencover.console.exe -target:"dotnet.exe"  -targetargs:"test MasterDataProxies\test\UnitTests\VSS.MasterData.Proxies.UnitTests.csproj" -mergeoutput -hideskipped:File -output:coverage.xml -register:user -oldstyle -filter:"+[*]VSS.*"
.\tools\opencover.console.exe -target:"dotnet.exe"  -targetargs:"test TCCFileAccess\test\UnitTests\VSS.TCCFileAccess.UnitTests.csproj" -mergeoutput -hideskipped:File -output:coverage.xml -register:user -oldstyle -filter:"+[*]VSS.*"
.\tools\opencover.console.exe -target:"dotnet.exe"  -targetargs:"test ConfigurationStore\test\UnitTests\VSS.ConfigurationStore.UnitTests.csproj" -mergeoutput -hideskipped:File -output:coverage.xml -register:user -oldstyle -filter:"+[*]VSS.*"
.\tools\opencover.console.exe -target:"dotnet.exe"  -targetargs:"test MasterDataModels\test\UnitTests\VSS.MasterData.Models.UnitTests.csproj" -mergeoutput -hideskipped:File -output:coverage.xml -register:user -oldstyle -filter:"+[*]VSS.*"


echo "Generating HTML report"
.\tools\OpenCoverToCoberturaConverter.exe -input:coverage.xml -output:outputCobertura.xml -sources:%WORKSPACE%
.\tools\Report\ReportGenerator.exe -reports:coverage.xml -targetdir:.\CoverageReport