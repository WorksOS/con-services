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
dotnet publish EventTests/EventTests.csproj -o ..\..\deploy\EventTests -f netcoreapp1.1
dotnet publish KafkaTests/KafkaTests.csproj -o ..\..\deploy\KafkaTests -f netcoreapp1.1
dotnet publish RepositoryTests/RepositoryTests.csproj -o ..\..\deploy\RepositoryTests -f netcoreapp1.1

copy KafkaTests\appsettings.json ..\deploy\KafkaTests\
copy RepositoryTests\appsettings.json ..\deploy\RepositoryTests\

cd ..
cd utilities
dotnet publish TestRun/TestRun.csproj -o ..\..\deploy\TestRun -f netcoreapp1.1
cd ..