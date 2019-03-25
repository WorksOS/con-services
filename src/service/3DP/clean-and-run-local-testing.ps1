[Console]::ResetColor()

IF ($args -contains "--set-vars") { & ./set-environment-variables.ps1 }

# Switch to Windows daemon
$dockerDaemon = Docker version | Select-Object -Last 2

if ($dockerDaemon -like "*linux*") {
    Write-Host "Current Docker Desktop daemon: Linux"

    IF (docker ps -q --filter "name=mockprojectwebapi") {
        Write-Host "Found mockwebapi container running on Linux daemon, terminating..." -ForegroundColor DarkGray
        docker stop $(docker ps -q --filter "name=mockprojectwebapi")
    }

    Write-Host "Switching to Windows daemon..." -ForegroundColor DarkGray
    & 'C:\Program Files\Docker\Docker\DockerCli.exe' -SwitchDaemon
    Write-Host "Done" -ForegroundColor Green
}

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

# Workaround to put appender-ref=RollingFile back so we have log files when running locally on Windows. See commit c86a567.
Set-Location ./src/WebApi
Copy-Item ./log4net.xml "$PSScriptRoot/artifacts/webapi"
Set-Location $PSScriptRoot
# End workaround.

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
