cd ..
RMDIR /S /Q deploy
mkdir deploy
copy Dockerfile deploy\
copy scripts\runtests.sh deploy\
copy scripts\wait-for-it.sh deploy\
copy scripts\rm_cr.sh deploy\
mkdir deploy\testresults

dotnet restore --no-cache VSS.TagFileAuth.Service.AcceptanceTests.sln

cd tests
Write-Host "Publishing acceptance test projects" -ForegroundColor DarkGray
dotnet publish WebApiTests\WebApiTests.csproj -o ..\..\deploy\WebApiTests -f netcoreapp3.1

copy WebApiTests\appsettings.json ..\..\deploy\WebApiTests\

cd ..
