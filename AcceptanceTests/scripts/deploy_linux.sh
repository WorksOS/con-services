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
dotnet publish IntegrationTests -o ../deploy/IntegrationTests -f netcoreapp1.0
dotnet publish WebApiTests -o ../deploy/WebApiTests -f netcoreapp1.0
dotnet publish EventTests -o ../deploy/EventTests -f netcoreapp1.0
dotnet publish KafkaTests -o ../deploy/KafkaTests -f netcoreapp1.0
dotnet publish RepositoryTests -o ../deploy/RepositoryTests -f netcoreapp1.0

cp KafkaTests/appsettings.json ../deploy/KafkaTests/

cd ..
cd utilities
dotnet publish TestRun -o ../deploy/TestRun -f netcoreapp1.0