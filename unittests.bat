"%ProgramFiles%\dotnet\dotnet.exe" restore ./test/UnitTests/WebApiTests/VSS.Productivity3D.WebApi.Tests.csproj
cd test/UnitTests/WebApiTests

"%ProgramFiles%\dotnet\dotnet.exe" build VSS.Productivity3D.WebApi.Tests.csproj -c Debug
"%ProgramFiles%\dotnet\dotnet.exe" vstest bin\Debug\net471\VSS.Productivity3D.WebApiTests.dll /Platform:x64
