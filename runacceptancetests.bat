set content=
for /F "delims=" %%i in (container.txt) do set content=%%i
for /f %%i in ('docker inspect --format "{{ .NetworkSettings.Networks.nat.IPAddress }}" %content%') do set ipaddress=%%i
cd AcceptanceTests\tests\ProductionDataSvc.AcceptanceTests\bin\Debug
del *.trx
echo %ipaddress% > TestData\webapiaddress.txt
PowerShell.exe -ExecutionPolicy Bypass -Command .\\waitForContainer.ps1 -IP %ipaddress%
mstest /testcontainer:ProductionDataSvc.AcceptanceTests.dll /resultsfile:testresults.trx
docker logs %content% > logs.txt
exit 0