Write-Host "Running acceptance tests" -ForegroundColor DarkGray

$global:ipAddress = ""

function WaitForContainer {
    PowerShell.exe -ExecutionPolicy Bypass -Command .\waitForContainer.ps1 -IP $global:ipAddress
}

# Validate the required containers are running
$containers = docker ps
$mockWebAPIContainerName = $containers | Select-String -Pattern "[a-zA-Z0-9-_-]+mockprojectwebapi_\d"  | Select-Object -ExpandProperty Matches |  Select-Object -First 1 -ExpandProperty Value
$3dpWebAPIcontainerName = $containers | Select-String -Pattern "[a-zA-Z0-9-_-]+_webapi_\d"  | Select-Object -ExpandProperty Matches |  Select-Object -First 1 -ExpandProperty Value

if ($mockWebAPIContainerName.Length -lt 1) {
    Write-Host "Failed to find `mockprojectwebapi` container. Exiting" -ForegroundColor DarkRed
    Exit -1
}

if ($3dpWebAPIcontainerName.Length -lt 1) {
    Write-Host "Failed to find `3DP webapi` container. Exiting" -ForegroundColor DarkRed
    Exit -1
}

$global:ipAddress = docker inspect --format "{{ .NetworkSettings.Networks.nat.IPAddress }}" $3dpWebAPIcontainerName
WaitForContainer

# $? is true if last command was a success, false otherwise
if (!$?) {
    Write-Host "No IP address set, attempting again in 10 seconds..." - -ForegroundColor DarkRed
    docker ps -a
    Start-Sleep -Seconds 10
    WaitForContainer
}

# Set (session) environment variables
$env:TEST_DATA_PATH="../../TestData/"
$env:COMPACTION_SVC_BASE_URI=":80"
$env:NOTIFICATION_SVC_BASE_URI=":80"
$env:REPORT_SVC_BASE_URI=":80"
$env:TAG_SVC_BASE_URI=":80"
$env:COORD_SVC_BASE_URI=":80"
$env:PROD_SVC_BASE_URI=":80"
$env:FILE_ACCESS_SVC_BASE_URI=":80"
$env:RAPTOR_WEBSERVICES_HOST=$ipAddress

Set-Location AcceptanceTests\tests\ProductionDataSvc.AcceptanceTests\bin\Debug
Remove-Item *.trx

$ipAddress > TestData\webapiaddress.txt

mstest /testcontainer:ProductionDataSvc.AcceptanceTests.dll /resultsfile:testresults.trx
docker logs $3dpWebAPIcontainerName > logs.txt

Exit 0