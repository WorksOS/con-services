set content=
for /F "delims=" %%i in (container.txt) do set content=%%i
for /f %%i in ('docker inspect --format "{{ .NetworkSettings.Networks.nat.IPAddress }}" %content%') do set ipaddress=%%i
PowerShell.exe -ExecutionPolicy Bypass -Command .\waitForContainer.ps1 -IP %ipaddress%

rem SET ENVIRONMENT VARIABLES
set TEST_DATA_PATH=../../TestData/
set COMPACTION_SVC_BASE_URI=:80
set NOTIFICATION_SVC_BASE_URI=:80
set REPORT_SVC_BASE_URI=:80
set TAG_SVC_BASE_URI=:80
set COORD_SVC_BASE_URI=:80
set PROD_SVC_BASE_URI=:80
set FILE_ACCESS_SVC_BASE_URI=:80
set RAPTOR_WEBSERVICES_HOST=%ipaddress%

cd AcceptanceTests\tests\ProductionDataSvc.AcceptanceTests\bin\Debug
del *.trx
echo %ipaddress% > TestData\webapiaddress.txt
mstest /testcontainer:ProductionDataSvc.AcceptanceTests.dll /resultsfile:testresults.trx
docker logs %content% > logs.txt
exit 0