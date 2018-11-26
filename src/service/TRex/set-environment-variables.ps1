$environment = $args[0]

# Help
if ($environment -ieq "--help" -or $environment -ieq "-h") {
  Write-Host "`nUsage: set-environment-variables [environment]`n"
  Write-Host "Environment Options:"
  Write-Host "  -d, --dev`tSet environmment variables for running TRex services on localhost, connecting to DEV TPaaS APIs. (Default option)"
  Write-Host "  -s, --staging`tSet environmment variables for running TRex locally and connecting to STAGING TPaaS APIs.`n"
  Exit 0
}

$commonVariables = @()

$environmentVariablesDev = @(
  @{key = "TPAAS_AUTH_URL"; value = "https://identity-stg.trimble.com/i/oauth2"},
  @{key = "TPAAS_APP_TOKEN"; value = "MGh1X25tYXlEQWFkMFdpY1hDekVHVTE3U2ZVYTppVWN3eEZ1cFRDRWFsaFVFOTRwWGhkVVNEa0Vh"},
  @{key = "CONNECTED_SITE_URL"; value = "https://api-stg.trimble.com/t/trimble.com/cws/connectedsite"},
  @{key = "COORDINATE_SERVICE_URL"; value = "https://api-stg.trimble.com/t/trimble.com/coordinates/1.0"})

$environmentVariablesStaging = @(
  @{key = "TPAAS_AUTH_URL"; value = "https://identity-stg.trimble.com/i/oauth2"},
  @{key = "TPAAS_APP_TOKEN"; value = "MGh1X25tYXlEQWFkMFdpY1hDekVHVTE3U2ZVYTppVWN3eEZ1cFRDRWFsaFVFOTRwWGhkVVNEa0Vh"},
  @{key = "CONNECTED_SITE_URL"; value = "https://api-stg.trimble.com/t/trimble.com/cws/connectedsite"},
  @{key = "COORDINATE_SERVICE_URL"; value = "https://api-stg.trimble.com/t/trimble.com/coordinates/1.0"})

if ($environment -ieq "--staging" -or $environment -ieq "-s") {
  $environmentVariables = $environmentVariablesStaging
  Write-Host "`nSetting environment variables for STAGING collaborator testing...`n"
} else {
  $environmentVariables = $environmentVariablesDev
  Write-Host "`nSetting environment variables for DEV collaborator testing...`n"
}

foreach ($_ in $commonVariables) {
  Write-Host "  " $_.key ": " -ForegroundColor Gray -NoNewline
  Write-Host $_.value -ForegroundColor DarkGray
  [Environment]::SetEnvironmentVariable($_.key, $_.value, "Machine")
}

foreach ($_ in $environmentVariables) {
  Write-Host "  " $_.key ": " -ForegroundColor Gray -NoNewline
  Write-Host $_.value -ForegroundColor DarkGray
  [Environment]::SetEnvironmentVariable($_.key, $_.value, "Machine")
}

Write-Host "`nFinished`n" -ForegroundColor Green