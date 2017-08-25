echo "Calculating coverage with OpenCover"
dotnet restore test\UnitTests\VSS.Productivity3D.Scheduler.Tests\VSS.Productivity3D.Scheduler.Tests.csproj
.\tools\opencover.console.exe -target:"dotnet.exe"  -targetargs:"test test\UnitTests\VSS.Productivity3D.Scheduler.Tests\VSS.Productivity3D.Scheduler.Tests.csproj" -mergeoutput -hideskipped:File -output:coverage.xml -register:user -oldstyle -filter:"+[*]VSS.Productivity3D.*"
echo "Generating HTML report"
.\tools\OpenCoverToCoberturaConverter.exe -input:coverage.xml -output:outputCobertura.xml -sources:%WORKSPACE%
.\tools\Report\ReportGenerator.exe -reports:coverage.xml -targetdir:.\CoverageReport