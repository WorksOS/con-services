echo "Calculating coverage with OpenCover"
dotnet restore test\UnitTests\MasterDataProjectTests\VSS.Project.WebApi.Tests.csproj
.\tools\opencover.console.exe -target:"dotnet.exe"  -targetargs:"test test\UnitTests\MasterDataProjectTests\VSS.Project.WebApi.Tests.csproj" -mergeoutput -hideskipped:File -output:coverage.xml -register:user -oldstyle -filter:"+[*]VSS.MasterData.Project.*"
echo "Generating HTML report"
.\tools\OpenCoverToCoberturaConverter.exe -input:coverage.xml -output:outputCobertura.xml -sources:%WORKSPACE%
.\tools\Report\ReportGenerator.exe -reports:coverage.xml -targetdir:.\CoverageReport