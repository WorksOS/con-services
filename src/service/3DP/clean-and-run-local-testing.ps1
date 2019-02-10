[Console]::ResetColor()

IF ($args -contains "--set-vars") { & ./set-environment-variables.ps1 }

Write-Host "Removing old 3DP application containers" -ForegroundColor DarkGray

# Stop and remove 3DP containers only; leave non affected containers running.
$array = @("3dp_webapi", "3dp_mockprojectwebapi")

FOR ($i = 0; $i -lt $array.length; $i++) {
    $containerName = $array[$i]

    IF (docker ps -q --filter "name=$containerName") { docker stop $(docker ps -q --filter "name=$containerName") }
    IF (docker ps -aq --filter "name=$containerName") { docker rm $(docker ps -aq --filter "name=$containerName") }
}

Write-Host "Done" -ForegroundColor Green

& $PSScriptRoot/build471.ps1

$runTests = IF ($args -contains "--no-test") { $false } ELSE { $true }

IF ($runTests) {
    Write-Host "Running unit tests" -ForegroundColor DarkGray
    & $PSScriptRoot/run-unit-tests.ps1
}

Write-Host "Building services" -ForegroundColor DarkGray
& ./start-containers.ps1

IF ($runTests) {
    & $PSScriptRoot/run-acceptance-tests.ps1
}

Write-Host "Finished`n" -ForegroundColor Green
