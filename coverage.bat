echo "Calculating coverage with OpenCover"
dotnet restore test/TagFileHarvesterTests/VSS.Productivity3D.TagFileHarvester.Tests.csproj
.\tools\opencover.console.exe -target:"dotnet.exe"  -targetargs:"test test\TagFileHarvesterTests\VSS.Productivity3D.TagFileHarvester.Tests.csproj" -mergeoutput -hideskipped:File -output:coverage.xml -register:user -oldstyle -filter:"+[*]VSS.Productivity3D.*"
echo "Generating HTML report"
.\tools\OpenCoverToCoberturaConverter.exe -input:coverage.xml -output:outputCobertura.xml -sources:%WORKSPACE%
.\tools\Report\ReportGenerator.exe -reports:coverage.xml -targetdir:.\CoverageReport