dotnet restore ./test/UnitTests/WebApiTests/WebApiTests.csproj  
cd test/UnitTests/WebApiTests
dotnet test WebApiTests.csproj -f net47 
