set content=
for /F "delims=" %%i in (container.txt) do set content=%%i
docker inspect --format '{{ .NetworkSettings.Networks.nat.IPAddress }}' dev_webapi_1
for /f %%i in ('docker inspect --format "{{ .NetworkSettings.Networks.nat.IPAddress }}" %content%') do set ipaddress=%%i
cd AcceptanceTests\tests\ProductionDataSvc.AcceptanceTests\bin\Debug
del *.trx
echo %ipaddress% > TestData\webapiaddress.txt
mstest /testcontainer:ProductionDataSvc.AcceptanceTests.dll /resultsfile:testresults.trx