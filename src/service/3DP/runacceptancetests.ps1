Write-Host "Running acceptance tests" -ForegroundColor DarkGray

$global:ipAddress = ""

function WaitForContainer {
    PowerShell.exe -ExecutionPolicy Bypass -Command .\waitForContainer.ps1 -containerIPAddress $global:ipAddress

    if ($LastExitCode -ne 0) {
        Write-Host "Unable to connect to Raptor container service at $ipAddress, the '$containerName' container may not be responding or has stopped." -ForegroundColor DarkRed
        Write-Host "Docker containers:" -ForegroundColor DarkGray
        docker ps --all
        Write-Host "Aborting..." -ForegroundColor DarkGray
        Exit -1
    }
}

# Validate the required containers are running
$containers = docker ps
$mockWebAPIContainerName = $containers | Select-String -Pattern "[a-zA-Z0-9-_-]+mockprojectwebapi_\d"  | Select-Object -ExpandProperty Matches |  Select-Object -First 1 -ExpandProperty Value
$containerName = $containers | Select-String -Pattern "[a-zA-Z0-9-_-]+_webapi_\d"  | Select-Object -ExpandProperty Matches |  Select-Object -First 1 -ExpandProperty Value

if ($mockWebAPIContainerName.Length -lt 1) {
    Write-Host "Failed to find `mockprojectwebapi` container. Exiting" -ForegroundColor DarkRed
    Exit -1
}

if ($containerName.Length -lt 1) {
    Write-Host "Failed to find `3DP webapi` container. Exiting" -ForegroundColor DarkRed
    Exit -1
}

$global:ipAddress = docker inspect --format "{{ .NetworkSettings.Networks.nat.IPAddress }}" $containerName
Write-Host "Global ipAddress set to: $global:ipAddress" -ForegroundColor DarkGray
WaitForContainer

Write-Host "Setting session environment variables..." -ForegroundColor DarkGray
$env:COMPACTION_SVC_BASE_URI=":80"
$env:NOTIFICATION_SVC_BASE_URI=":80"
$env:REPORT_SVC_BASE_URI=":80"
$env:TAG_SVC_BASE_URI=":80"
$env:COORD_SVC_BASE_URI=":80"
$env:PROD_SVC_BASE_URI=":80"
$env:FILE_ACCESS_SVC_BASE_URI=":80"
$env:RAPTOR_WEBSERVICES_HOST=$ipAddress

Write-Host "Removing test file artifacts..." -ForegroundColor DarkGray
#Set-Location .\AcceptanceTests\tests\ProductionDataSvc.AcceptanceTests\bin\Debug\netcoreapp2.0
Set-Location .\AcceptanceTests
Remove-Item *.trx

Write-Host "Running tests..." -ForegroundColor DarkGray
dotnet test .\VSS.Productivity3D.Service.AcceptanceTests.sln --logger "trx;LogFileName=TestResults.trx"
docker logs $containerName > logs.txt

Set-Location $PSScriptRoot
Exit 0