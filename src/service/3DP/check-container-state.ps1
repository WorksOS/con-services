# In Jenkins we need to check that the required containers are NOT already running. If they are, abort the build so we don't fail both with out of resources errors.
Write-Host "Removing old 3DP application containers" -ForegroundColor DarkGray

# Stop and remove 3DP containers only; leave non affected containers running.
$array = @("3dp_webapi", "3dp_mockprojectwebapi")

FOR ($i = 0; $i -lt $array.length; $i++) {
    $containerName = $array[$i]

    IF (docker ps -q --filter "name=$containerName") { docker stop $(docker ps -q --filter "name=$containerName") }
    IF (docker ps -aq --filter "name=$containerName") { docker rm $(docker ps -aq --filter "name=$containerName") }
}

IF (-not $?) {
    Write-Host "Error: Failed to remove already running 3DP containers." -ForegroundColor Red
    EXIT 1
}
