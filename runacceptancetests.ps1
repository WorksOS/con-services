# param (
#   [Parameter(Mandatory=$true)][string]$CONTAINER_NAME
# )

foreach($line in Get-Content .\container.txt) {
    if($line -match "\w*_webapi_\w*"){
        $CONTAINER_NAME = $line
        break
    }
}

$IP_ADDRESS = docker inspect --format "{{ .NetworkSettings.Networks.nat.IPAddress }}" $CONTAINER_NAME

PowerShell.exe -ExecutionPolicy Bypass -Command .\waitForContainer.ps1 -IP $IP_ADDRESS

# $? is true if last command was in error, false otherwise
if ($?) {
    "NO IP ADRESS SET"
    docker ps -a
    ping 0.0.0.0 -n 10
    $IP_ADDRESS = docker inspect --format "{{ .NetworkSettings.Networks.nat.IPAddress }}" $CONTAINER_NAME
    PowerShell.exe -ExecutionPolicy Bypass -Command .\waitForContainer.ps1 -IP $IP_ADDRESS   
}

# SET ENVIRONMENT VARIABLES
$env:TEST_DATA_PATH="../../TestData/"
$env:COMPACTION_SVC_BASE_URI=":80"
$env:NOTIFICATION_SVC_BASE_URI=":80"
$env:REPORT_SVC_BASE_URI=":80"
$env:TAG_SVC_BASE_URI=":80"
$env:COORD_SVC_BASE_URI=":80"
$env:PROD_SVC_BASE_URI=":80"
$env:FILE_ACCESS_SVC_BASE_URI=":80"
$env:RAPTOR_WEBSERVICES_HOST=$IP_ADDRESS

cd AcceptanceTests\tests\ProductionDataSvc.AcceptanceTests\bin\Debug
del *.trx
$IP_ADDRESS > TestData\webapiaddress.txt
mstest /testcontainer:ProductionDataSvc.AcceptanceTests.dll /resultsfile:testresults.trx
docker logs $CONTAINER_NAME > logs.txt
exit 0