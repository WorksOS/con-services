cd ..
RMDIR /S /Q deploy
if exist deploy rd /s /q deploy
mkdir deploy
copy Dockerfile deploy\
copy scripts\runtests.bat deploy\
mkdir deploy\testresults

.nuget\nuget restore
"C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild"


cd tests
REM dotnet publish WebApiTests -o ..\deploy\WebApiTests -f net46

