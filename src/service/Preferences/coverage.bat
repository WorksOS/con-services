echo "Calculating coverage with OpenCover"
dotnet restore test\UnitTests\CCSS.Productivity3D.Preferences.Tests.csproj
.\tools\opencover.console.exe -target:"dotnet.exe"  -targetargs:"test test\UnitTests\CCSS.Productivity3D.Preferences.Tests.csproj" -mergeoutput -hideskipped:File -output:coverage.xml -register:user -oldstyle -filter:"+[*]CCSS.Productivity3D.Preferences.*"
echo "Generating HTML report"
.\tools\OpenCoverToCoberturaConverter.exe -input:coverage.xml -output:outputCobertura.xml -sources:%WORKSPACE%
.\tools\Report\ReportGenerator.exe -reports:coverage.xml -targetdir:.\CoverageReport