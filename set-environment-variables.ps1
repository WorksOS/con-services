$environment = $args[0]

# Help
if ($environment -ieq "--help" -or $environment -ieq "-h") {
  Write-Host "`nUsage: set-environment-variables [environment]`n"
  Write-Host "Environment Options:"
  Write-Host "  -l, --local`tSet environmment variables for running 3DP and MockWebAPI locally. (Default option)"
  Write-Host "  -d, --dev`tSet environmment variables for running 3DP locally and connect to DEV hosted collaborating services."
  Write-Host "  -a, --alpha`tSet environmment variables for running 3DP locally and connect to ALPHA hosted collaborating services.`n"
  Exit 0
}

$commonVariables = @(
  @{key = "PROJECT_CACHE_LIFE"; value = "00:15:00"},
  @{key = "SCHEDULED_JOB_TIMEOUT"; value = "300000"})

# Used for the Acceptance Tests client to connect to a local (LOCALHOST) instance of 3DP service.
# Always set implicitly, regardless of the $environment type.
$acceptanceTestsEnvironmentVariables = @(
  @{key = "COMPACTION_SVC_BASE_URI"; value = ":5000"},
  @{key = "COORD_SVC_BASE_URI"; value = ":5000"},
  @{key = "FILE_ACCESS_SVC_BASE_URI"; value = ":5000"},
  @{key = "FILE_ACCESS_SVC_BASE_URI"; value = ":5000"},
  @{key = "NOTIFICATION_SVC_BASE_URI"; value = ":5000"},
  @{key = "PROD_SVC_BASE_URI"; value = ":5000"},
  @{key = "RAPTOR_WEBSERVICES_HOST"; value = "localhost"},
  @{key = "REPORT_SVC_BASE_URI"; value = ":5000"},
  @{key = "TAG_SVC_BASE_URI"; value = ":5000"},
  @{key = "TCCFILESPACENAME"; value = "vldatastore-dev"})

# Common TCC and AWS variables, will be set for any chosen $environment type.
$tccAndAwsEnvironmentVariables = @(
  @{key = "ALK_KEY"; value = "97CC5BD1CA28934796791B229AE9C3FA"},
  @{key = "AWS_ACCESS_KEY"; value = "AKIAIBGOEETXHMANDX7A"},
  @{key = "AWS_TAGFILE_BUCKET_NAME"; value = "vss-stg-directtagfile-archives"},
  @{key = "AWS_SECRET_KEY"; value = "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"},
  @{key = "TCCBASEURL"; value = "https://www.myconnectedsite.com"},
  @{key = "TCCFILESPACEID"; value = "u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01"},
  @{key = "TILE_RENDER_JOB_TIMEOUT"; value = "00:00:10"},
  @{key = "TILE_RENDER_MAX_ZOOM_LEVEL"; value = ""},
  @{key = "TILE_RENDER_MAX_ZOOM_RANGE"; value = "14"},
  @{key = "TILE_RENDER_WAIT_INTERVAL"; value = "2000"},
  @{key = "TCCSynchProductionDataArchivedFolder"; value = "Production-Data (Archived)"},
  @{key = "TCCSynchProjectBoundaryIssueFolder"; value = "Project Boundary (Issue)"},    
  @{key = "TCCSynchSubscriptionIssueFolder"; value = "Subscription (Issue)"},
  @{key = "TCCSynchOtherIssueFolder"; value = "Other... (Issue)"})

# Used when running the collaborating services against a locally running MockWebApi service.
$localhostEnvironmentVariables = @(
  @{key = "CUSTOMERSERVICE_API_URL"; value = "http://localhost:5001/api/v1/mock/getcustomersforme"},
  @{key = "FILTER_API_URL"; value = "http://localhost:5001/api/v1/mock"},
  @{key = "GEOFENCE_API_URL"; value = "http://localhost:5001/api/v1/mock/geofences"},
  @{key = "IMPORTED_FILE_API_URL"; value = "http://localhost:5001/api/v4/mock/importedfiles"},
  @{key = "PREFERENCE_API_URL"; value = "http://localhost:5001/api/v1/mock/preferences"},
  @{key = "PROJECT_API_URL"; value = "http://localhost:5001/api/v4/mockproject"},
  @{key = "PROJECT_SETTINGS_API_URL"; value = "http://localhost:5001/api/v4/mock"},
  @{key = "SCHEDULER_INTERNAL_EXPORT_URL"; value = "http://localhost:5001/internal/v1/mock/export"},
  @{key = "TCCFILESPACENAME"; value = "vldatastore-dev"},
  @{key = "TCCORG"; value = "vldev"},
  @{key = "TCCPWD"; value = "vldev_key"},
  @{key = "TCCUSERNAME"; value = "vldev"})

# Used when running 3DP service locally but connecting to /dev deployed collaborating services
# NOTE: Some of these services point to Alpha collaborators because the Dev versions do not exist or function incorrectly.
$devCollaboratorsEnvironmentVariables = @(
  @{key = "CUSTOMERSERVICE_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-customerservice/1.0/Customers/me"}, # Use Alpha service here
  @{key = "FILTER_API_URL"; value = "http://10.97.96.103:3010/api/v1"},
  @{key = "GEOFENCE_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-geofenceservice/1.0"}, # Use Alpha service here
  @{key = "IMPORTED_FILE_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-dev-projects/1.4/importedfiles"},
  @{key = "PREFERENCE_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-preferenceservice/1.0"}, # Use Alpha service here
  @{key = "PROJECT_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-dev-projects/1.4/project?includeLandfill=true"},
  @{key = "PROJECT_SETTINGS_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-dev-projects/1.4"},
  @{key = "SCHEDULER_INTERNAL_EXPORT_URL"; value = "http://10.97.96.103:3011/internal/v1/export"},
  @{key = "TCCFILESPACENAME"; value = "vldatastore-dev"},
  @{key = "TCCORG"; value = "vldev"},
  @{key = "TCCPWD"; value = "vldev_key"},
  @{key = "TCCUSERNAME"; value = "vldev"})

# Used when running 3DP service locally but connecting to /alpha deployed collaborating services
$alphaCollaboratorsEnvironmentVariables = @(
  @{key = "CUSTOMERSERVICE_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-customerservice/1.0/Customers/me"},
  @{key = "FILTER_API_URL"; value = "http://filter.alpha.k8s.vspengg.com/api/v1"},
  @{key = "GEOFENCE_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-geofenceservice/1.0"},
  @{key = "IMPORTED_FILE_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-projects/1.4/importedfiles"},
  @{key = "PREFERENCE_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-preferenceservice/1.0"},
  @{key = "PROJECT_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-projects/1.4/project?includeLandfill=true"},
  @{key = "PROJECT_SETTINGS_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-projects/1.4"},
  @{key = "SCHEDULER_INTERNAL_EXPORT_URL"; value = "http://10.97.96.103:9011/internal/v1/export"},
  @{key = "TCCFILESPACENAME"; value = "vldatastore-alpha"},
  @{key = "TCCORG"; value = "vlalpha"},
  @{key = "TCCPWD"; value = "vlalpha_key"},
  @{key = "TCCUSERNAME"; value = "vlalpha"})

# Used when running 3DP service locally but connecting to /prod deployed collaborating services
$prodCollaboratorsEnvironmentVariables = @(
  @{key = "CUSTOMERSERVICE_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-prod-customerservice/1.0/Customers/me"},
  @{key = "FILTER_API_URL"; value = "http://10.211.10.253:5010/api/1.0"},
  @{key = "GEOFENCE_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-prod-geofenceservice/1.0"},
  @{key = "IMPORTED_FILE_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-prod-projects/1.4/importedfiles"},
  @{key = "PREFERENCE_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-prod-preferenceservice/1.0"},
  @{key = "PROJECT_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-prod-projects/1.4/project?includeLandfill=true"},
  @{key = "PROJECT_SETTINGS_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-prod-projects/1.4"},
  @{key = "SCHEDULER_INTERNAL_EXPORT_URL"; value = ""},
  @{key = "TCCFILESPACENAME"; value = "vldatastore-prod"},
  @{key = "TCCORG"; value = "vlprod"},
  @{key = "TCCPWD"; value = "vlprod_key"},
  @{key = "TCCUSERNAME"; value = "vlprod"})

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

Write-Host "`nRemember to change your Velociraptor.Config.xml one compatible with the environment you've chosen."
Write-Host "Finished`n" -ForegroundColor Green