echo "ENVIRONMENT VARIABLES ARE"
echo %RAPTOR_WEBSERVICES_HOST%
echo %TEST_DATA_PATH%
cd AcceptanceTests\tests\ProductionDataSvc.AcceptanceTests\bin\Debug
del *.trx
rem echo %ipaddress% > TestData\webapiaddress.txt
mstest /testcontainer:ProductionDataSvc.AcceptanceTests.dll /resultsfile:testresults.trx
docker logs %container_id% > logs.txt
rem exit 0