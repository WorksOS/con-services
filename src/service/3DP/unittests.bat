"C:\Program Files\dotnet\dotnet.exe" restore ./test/UnitTests/WebApiTests/WebApiTests.csproj
cd test/UnitTests/WebApiTests

"C:\Program Files\dotnet\dotnet.exe" build WebApiTests.csproj -c Debug
"C:\Program Files\dotnet\dotnet.exe" vstest bin\Debug\net471\VSS.Productivity3D.WebApiTests.dll /Platform:x64
