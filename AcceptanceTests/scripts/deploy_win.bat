cd ..
RMDIR /S /Q deploy
if exist deploy rd /s /q deploy
mkdir deploy
copy Dockerfile deploy\
copy scripts\runtests.bat deploy\
mkdir deploy\testresults

REM dotnet restore

cd tests
REM dotnet publish WebApiTests -o ..\deploy\WebApiTests -f net451

