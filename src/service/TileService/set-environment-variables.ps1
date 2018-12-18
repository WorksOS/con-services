$environment = $args[0]

# Help
if ($environment -ieq "--help" -or $environment -ieq "-h") {
  Write-Host "`nUsage: set-environment-variables [environment]`n"
  Write-Host "Environment Options:"
  Write-Host "  -l, --local`tSet environmment variables for running Tile and MockWebAPI locally. (Default option)"
  Write-Host "  -d, --dev`tSet environmment variables for running Tile locally and connect to DEV hosted collaborating services."
  Write-Host "  -a, --alpha`tSet environmment variables for running Tile locally and connect to ALPHA hosted collaborating services.`n"
  Exit 0
}

# Used for the Acceptance Tests client to connect to a local (LOCALHOST) instance of the Tile service.
# Always set implicitly, regardless of the $environment type.
$acceptanceTestsEnvironmentVariables = @(
  @{key = "TILE_WEBSERVICES_URL"; value = ":5000"}
)

# Common TCC and AWS variables, will be set for any chosen $environment type.
$tccAndAwsEnvironmentVariables = @(
  @{key = "ALK_KEY"; value = "97CC5BD1CA28934796791B229AE9C3FA"}
  @{key = "TCCBASEURL"; value = "https://www.myconnectedsite.com"}
  @{key = "TCCFILESPACEID"; value = "u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01"}
  @{key = "TILE_CACHE_LIFE"; value = "00:15:00"}
)

# Used when running the collaborating services against a locally running MockWebApi service.
$localhostEnvironmentVariables = @(
  @{key = "CUSTOMERSERVICE_API_URL"; value = "http://localhost:5001/api/v1/mock/getcustomersforme"}
  @{key = "GEOFENCE_API_URL"; value = "http://localhost:5001/api/v1/mock/geofences"}
  @{key = "IMPORTED_FILE_API_URL"; value = "http://localhost:5001/api/v4/mock/importedfiles"}
  @{key = "LOADDUMP_API_URL"; value = "http://localhost:5001/api/v1/mock/loaddump"}
  @{key = "NOTIFICATION_HUB_URL"; value = "http://push.dev.k8s.vspengg.com/notifications"}
  @{key = "PREFERENCE_API_URL"; value = "http://localhost:5001/api/v1/mock/preferences"}
  @{key = "PROJECT_API_URL"; value = "http://localhost:5001/api/v4/mockproject"}
  @{key = "PROJECT_SETTINGS_API_URL"; value = "http://localhost:5001/api/v4/mock"}
  @{key = "PUSH_NO_AUTHENTICATION_HEADER"; value = "true"}
  @{key = "RAPTOR_3DPM_API_URL"; value = "http://localhost:5001/api/v2"}
  @{key = "SCHEDULER_INTERNAL_EXPORT_URL"; value = "http://localhost:5001/internal/v1/mock/export"}
  @{key = "TCCFILESPACENAME"; value = "vldatastore-dev"}
  @{key = "TCCORG"; value = "vldev"}
  @{key = "TCCPWD"; value = "vldev_key"}
  @{key = "TCCUSERNAME"; value = "vldev"}
  @{key = "TREX_TAGFILE_API_URL"; value = "http://mockprojectwebapi:5001/api/v2/mocktrextagfile"}
  @{key = "WEBAPI_DEBUG_URI"; value = "http://localhost:80/"}
  @{key = "WEBAPI_URI"; value = "http://localhost:80/"}
)

# Used when running Tile service locally but connecting to /dev deployed collaborating services
# NOTE: Some of these services point to Alpha collaborators because the Dev versions do not exist or function incorrectly.
$devCollaboratorsEnvironmentVariables = @(
  # TBC
)

# Used when running Tile service locally but connecting to /alpha deployed collaborating services
$alphaCollaboratorsEnvironmentVariables = @(
  # TBC
)

# Used when running Tile service locally but connecting to /prod deployed collaborating services
$prodCollaboratorsEnvironmentVariables = @(
  # TBC
)

if ($environment -ieq "--dev" -or $environment -ieq "-d") {
  $environmentVariables = $devCollaboratorsEnvironmentVariables
  Write-Host "`nSetting environment variables for remote /dev collaborator testing...`n"
}
elseif ($environment -ieq "--alpha" -or $environment -ieq "-a") {
  $environmentVariables = $alphaCollaboratorsEnvironmentVariables
  Write-Host "`nSetting environment variables for remote /alpha collaborator testing...`n"
}
elseif ($environment -ieq "--prod" -or $environment -ieq "-a") {
  $environmentVariables = $prodCollaboratorsEnvironmentVariables
  Write-Host "`nSetting environment variables for remote /prod collaborator testing...`n"
}
# Implicit --local environment
else {
  Write-Host "`nSetting environment variables for Acceptance Tests...`n"
  foreach ($_ in $acceptanceTestsEnvironmentVariables) {
    Write-Host "  " $_.key ": " -ForegroundColor Gray -NoNewline
    Write-Host $_.value -ForegroundColor DarkGray
    [Environment]::SetEnvironmentVariable($_.key, $_.value, "Machine")
  }

  $environmentVariables = $localhostEnvironmentVariables
  Write-Host "`nSetting environment variables for local development testing...`n"
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

# Common TCC and AWS configuration settings.
foreach ($_ in $tccAndAwsEnvironmentVariables) {
  Write-Host "  " $_.key ": " -ForegroundColor Gray -NoNewline
  Write-Host $_.value -ForegroundColor DarkGray
  [Environment]::SetEnvironmentVariable($_.key, $_.value, "Machine")
}

Write-Host "`nFinished`n" -ForegroundColor Green