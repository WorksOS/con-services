cd ..
rm -rf deploy
mkdir deploy
cp Dockerfile deploy/
cp scripts/runtests.sh deploy/
cp scripts/wait-for-it.sh deploy/
cp scripts/rm_cr.sh deploy/
mkdir deploy/testresults

dotnet restore --no-cache VSS.TagFileAuth.Service.AcceptanceTests.sln

cd tests
Write-Host "Publishing acceptance test projects" -ForegroundColor DarkGray
dotnet publish  WebApiTests/WebApiTests.csproj -o ../../deploy/WebApiTests -f netcoreapp3.1

cp WebApiTests/appsettings.json ../../deploy/WebApiTests/

cd ..