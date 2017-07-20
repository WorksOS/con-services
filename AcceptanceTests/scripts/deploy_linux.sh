cd ..
rm -rf deploy
mkdir deploy
cp Dockerfile deploy/
cp scripts/runtests.sh deploy/
cp scripts/wait-for-it.sh deploy/
cp scripts/rm_cr.sh deploy/
mkdir deploy/testresults

dotnet restore --no-cache

cd tests
dotnet publish IntegrationTests -o ../../deploy/IntegrationTests -f netcoreapp1.1
dotnet publish WebApiTests -o ../../deploy/WebApiTests -f netcoreapp1.1
dotnet publish ExecutorTests -o ../../deploy/ExecutorTests -f netcoreapp1.1

cd ..
cd utilities
dotnet publish TestRun -o ../../deploy/TestRun -f netcoreapp1.1