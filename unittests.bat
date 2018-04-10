"C:\Program Files\dotnet\dotnet.exe" restore ./test/UnitTests/WebApiTests/VSS.Productivity3D.WebApi.Tests.csproj
cd test/UnitTests/WebApiTests

"C:\Program Files\dotnet\dotnet.exe" build VSS.Productivity3D.WebApi.Tests.csproj -c Debug
"C:\Program Files\dotnet\dotnet.exe" vstest bin\Debug\net471\VSS.Productivity3D.WebApiTests.dll /Platform:x64
