cd ..
rm -rf deploy
mkdir deploy
cp Dockerfile deploy/
cp scripts/runtests.sh deploy/
cp scripts/wait-for-it.sh deploy/
cp scripts/rm_cr.sh deploy/
mkdir deploy/testresults

dotnet restore

cd tests
dotnet publish EventTests -o ../deploy/EventTests -f netcoreapp1.1
dotnet publish ExecutorTests -o ../deploy/ExecutorTests -f netcoreapp1.1
dotnet publish IntegrationTests -o ../deploy/IntegrationTests -f netcoreapp1.1
dotnet publish RepositoryTests -o ../deploy/RepositoryTests -f netcoreapp1.1
dotnet publish WebApiTests -o ../deploy/WebApiTests -f netcoreapp1.1

cp ExecutorTests/appsettings.json ../deploy/ExecutorTests/
cp RepositoryTests/appsettings.json ../deploy/RepositoryTests/

dotnet publish EventTests -o ..\deploy\EventTests -f netcoreapp1.1
dotnet publish ExecutorTests -o ..\deploy\ExecutorTests -f netcoreapp1.1
dotnet publish IntegrationTests -o ..\deploy\IntegrationTests -f netcoreapp1.1
dotnet publish RepositoryTests -o ..\deploy\RepositoryTests -f netcoreapp1.1
dotnet publish WebApiTests -o ..\deploy\WebApiTests -f netcoreapp1.1

copy ExecutorTests\appsettings.json ..\deploy\ExecutorTests\
copy RepositoryTests\appsettings.json ..\deploy\RepositoryTests\



cd ..
cd utilities
dotnet publish TestRun -o ../deploy/TestRun -f netcoreapp1.1