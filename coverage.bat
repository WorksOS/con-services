echo "Calculating coverage with OpenCover"
"%ProgramFiles%\dotnet\dotnet.exe" restore test\UnitTests\WebApiTests\VSS.Productivity3D.WebApi.Tests.csproj

"%ProgramFiles%\dotnet\dotnet.exe" build test\UnitTests\WebApiTests\VSS.Productivity3D.WebApi.Tests.csproj -c Debug
.\tools\opencover.console.exe -target:"%ProgramFiles%\dotnet\dotnet.exe" -targetargs:"vstest test\UnitTests\WebApiTests\bin\Debug\net471\VSS.Productivity3D.WebApiTests.dll /Platform:x64" -mergeoutput -hideskipped:File -output:coverage.xml -register:user -oldstyle -filter:"+[*]VSS.Productivity3D.*"
echo "Generating HTML report"
.\tools\OpenCoverToCoberturaConverter.exe -input:coverage.xml -output:outputCobertura.xml -sources:%WORKSPACE%
.\tools\Report\ReportGenerator.exe -reports:coverage.xml -targetdir:.\CoverageReport