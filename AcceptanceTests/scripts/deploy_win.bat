cd ..
RMDIR /S /Q deploy
mkdir deploy
copy Dockerfile deploy\
copy scripts\runtests.sh deploy\
copy scripts\wait-for-it.sh deploy\
copy scripts\rm_cr.sh deploy\
mkdir deploy\testresults

dotnet restore --no-cache FileAccessAcceptanceTests.sln

cd tests
dotnet publish IntegrationTests\IntegrationTests.csproj -o ..\deploy\IntegrationTests -f netcoreapp2.0
cd ..