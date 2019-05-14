function SetEnvironmentVariable {
    PARAM ([string] $key, [string] $value)

    Write-Host "  " $key ": " -ForegroundColor Gray -NoNewline
    Write-Host $value -ForegroundColor DarkGray
    [Environment]::SetEnvironmentVariable($key, $value, "Machine")
}

Write-Host "`nConfiguring environment for local Kafka event processing..."

# Get the IP address from the network adaptor and set KAFKA_ADVERTISED_HOST_NAME. Doing so allows any containerized Kafka instance to correctly find the host.
# NOTE: We need to get the first adapter here; so split the IPAddresses assuming the machine has multiple interfaces present (e.g. VPN connections).
$ipV4 = ( Get-NetIPConfiguration | Where-Object { $_.IPv4DefaultGateway -ne $null -and  $_.NetAdapter.Status -ne "Disconnected" }).IPv4Address.IPAddress.Split(' ')[0]

SetEnvironmentVariable "KAFKA_ADVERTISED_HOST_NAME" $ipV4

# Update the Docker-Compose file, if present.
IF (Test-Path docker-compose-local.env) {
    (Get-Content docker-compose-local.env) | ForEach-Object { $_ -Replace "(?=KAFKA_ADVERTISED_HOST_NAME=)[^""]*", "KAFKA_ADVERTISED_HOST_NAME=$ipV4" } | Set-Content docker-compose-local.env
    Write-Host "Updated docker-compose-local.env with new KAFKA_ADVERTISED_HOST_NAME value."
}
ELSE {
    Write-Host "Error: Unable to find file docker-compose-local.env locally." -ForegroundColor Red
}
