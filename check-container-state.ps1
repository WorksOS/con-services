# In Jenkins we need to check that the required containers are NOT already running. If they are, abort the build so we don't fail both with out of resources errors.
$containers = docker ps
$mockWebAPIContainerName = $containers | Select-String -Pattern "[a-zA-Z0-9-_-]+mockprojectwebapi_\d"  | Select-Object -ExpandProperty Matches |  Select-Object -First 1 -ExpandProperty Value
$3dpWebAPIcontainerName = $containers | Select-String -Pattern "[a-zA-Z0-9-_-]+_webapi_\d"  | Select-Object -ExpandProperty Matches |  Select-Object -First 1 -ExpandProperty Value

if ($mockWebAPIContainerName.Length -gt 0) {
    Write-Host "`mockprojectwebapi` container is already running, aborting..." -ForegroundColor DarkRed
    Exit -1
}

if ($3dpWebAPIcontainerName.Length -gt 0) {
    Write-Host "`3DP webapi` container is already running, aborting..." -ForegroundColor DarkRed
    Exit -1
}

Exit 0