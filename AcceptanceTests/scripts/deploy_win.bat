cd ..
RMDIR /S /Q deploy
mkdir deploy
copy Dockerfile deploy\
copy scripts\runtests.sh deploy\
copy scripts\wait-for-it.sh deploy\
copy scripts\rm_cr.sh deploy\
mkdir deploy\testresults

dotnet restore --no-cache

cd tests
dotnet publish IntegrationTests -o ..\deploy\IntegrationTests -f netcoreapp2.0

cd ..
cd utilities
dotnet publish TestRun -o ..\deploy\TestRun -f netcoreapp2.0
cd ..