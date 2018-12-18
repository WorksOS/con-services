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
  @{key = "SCHEDULED_JOB_TIMEOUT"; value = "300000"},
  @{key = "MAX_FILE_SIZE"; value = "100000000"},
  @{key = "TILE_CACHE_LIFE"; value = "00:15:00"},
  @{key = "CUSTOMER_CACHE_LIFE"; value = "00:30:00"},
  @{key = "ELEVATION_EXTENTS_CACHE_LIFE"; value = "00:30:00"},
  @{key = "FILTER_CACHE_LIFE"; value = "00:15:00"},
  @{key = "GEOFENCE_CACHE_LIFE"; value = "00:15:00"},
  @{key = "IMPORTED_FILE_CACHE_LIFE"; value = "00:15:00"},
  @{key = "PROJECT_CACHE_LIFE"; value = "00:30:00"},
  @{key = "PROJECT_SETTINGS_CACHE_LIFE"; value = "00:15:00"},
  @{key = "TILE_RENDER_JOB_TIMEOUT"; value = "00:00:10"},
  @{key = "DEFAULT_CONNECTION_LIMIT"; value = "64"},
  @{key = "LIBUV_THREAD_COUNT"; value = "32"},
  @{key = "MAX_IO_THREADS"; value = "1024"},
  @{key = "MAX_WORKER_THREADS"; value = "512"},
  @{key = "MIN_IO_THREADS"; value = "1024"},
  @{key = "MIN_WORKER_THREADS"; value = "512"},
  @{key = "SCHEDULED_JOB_TIMEOUT"; value = "900000"})

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
  @{key = "TCCFILESPACENAME"; value = "vldatastore-dev"},
  @{key = "ENABLE_RAPTOR_GATEWAY_TAGFILE"; value = "true"},
  @{key = "ENABLE_TREX_GATEWAY_TAGFILE"; value = "false"},
  @{key = "ENABLE_TREX_GATEWAY_STATIONOFFSET"; value = "false"},
  @{key = "ENABLE_TREX_GATEWAY_GRIDREPORT"; value = "false"})
  
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
  @{key = "TCCSynchOtherIssueFolder"; value = "Other... (Issue)"},
  @{key = "ENABLE_RAPTOR_GATEWAY_TAGFILE"; value = "true"},
  @{key = "ENABLE_TREX_GATEWAY_TAGFILE"; value = "false"},
  @{key = "ENABLE_TREX_GATEWAY_STATIONOFFSET"; value = "false"},
  @{key = "ENABLE_TREX_GATEWAY_GRIDREPORT"; value = "false"})

# Used when running the collaborating services against a locally running MockWebApi service.
$localhostEnvironmentVariables = @(
  @{key = "CUSTOMERSERVICE_API_URL"; value = "http://localhost:5001/api/v1/mock/getcustomersforme"},
  @{key = "FILTER_API_URL"; value = "http://localhost:5001/api/v1/mock"},
  @{key = "GEOFENCE_API_URL"; value = "http://localhost:5001/api/v1/mock/geofences"},
  @{key = "IMPORTED_FILE_API_URL"; value = "http://localhost:5001/api/v4/mock/importedfiles"},
  @{key = "PREFERENCE_API_URL"; value = "http://localhost:5001/api/v1/mock/preferences"},
  @{key = "PROJECT_API_URL"; value = "http://localhost:5001/api/v4/mockproject"},
  @{key = "PROJECT_SETTINGS_API_URL"; value = "http://localhost:5001/api/v4/mock"},
  @{key = "TREX_TAGFILE_API_URL"; value = "http://localhost:5001/api/v2/mocktrextagfile"},
  @{key = "TREX_GATEWAY_API_URL"; value = "http://localhost:55750/api/v1"},
  @{key = "SCHEDULER_INTERNAL_EXPORT_URL"; value = "http://localhost:5001/internal/v1/mock/export"},
  @{key = "TCCFILESPACENAME"; value = "vldatastore-dev"},
  @{key = "TCCORG"; value = "vldev"},
  @{key = "TCCPWD"; value = "vldev_key"},
  @{key = "TCCUSERNAME"; value = "vldev"},
  @{key = "ENABLE_RAPTOR_GATEWAY_TAGFILE"; value = "true"},
  @{key = "ENABLE_TREX_GATEWAY_TAGFILE"; value = "false"},
  @{key = "ENABLE_TREX_GATEWAY_STATIONOFFSET"; value = "false"},
  # todoJeannie temp
  @{key = "ENABLE_TREX_GATEWAY_GRIDREPORT"; value = "true"},
  @{key = "MYSQL_SERVER_NAME_VSPDB"; value = "db"},
  @{key = "MYSQL_PORT"; value = "3306"},
  @{key = "MYSQL_USERNAME"; value = "root"},
  @{key = "MYSQL_ROOT_PASSWORD"; value = "abc123"},
  @{key = "MYSQL_CAP_DATABASE_NAME"; value = "VSS-Productivity3D-CAP-Dev"},
  @{key = "MYSQL_CAP_TABLE_PREFIX"; value = "3dpm"},
  @{key = "MYSQL_USERNAME_BUILD"; value = "root"},
  @{key = "MYSQL_PASSWORD_BUILD"; value = "abc123"},
  @{key = "KAFKA_URI"; value = "kafka"},
  @{key = "KAFKA_PORT"; value = "9092"},
  @{key = "KAFKA_CAP_GROUP_NAME"; value = "3dpm-CAP"},
  @{key = "KAFKA_TOPIC_NAME_SUFFIX"; value = "-3dpm"})

# Used when running 3DP service locally but connecting to /dev deployed collaborating services
# NOTE: Some of these services point to Alpha collaborators because the Dev versions do not exist or function incorrectly.
$devCollaboratorsEnvironmentVariables = @(
  @{key = "CUSTOMERSERVICE_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-customerservice/1.0/Customers/me"}, # Use Alpha service here
  @{key = "FILTER_API_URL"; value = "http://filter.dev.k8s.vspengg.com/api/v1"},
  @{key = "GEOFENCE_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-geofenceservice/1.0"}, # Use Alpha service here
  @{key = "IMPORTED_FILE_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-dev-projects/1.4/importedfiles"},
  @{key = "PREFERENCE_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-preferenceservice/1.0"}, # Use Alpha service here
  @{key = "PROJECT_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-dev-projects/1.4/project"},
  @{key = "PROJECT_SETTINGS_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-dev-projects/1.4"},
  @{key = "TREX_TAGFILE_API_URL"; value = "http://api-stg.trimble.com/t/trimble.com/vss-dev-trexgateway/2.0/tagfiles"},
  @{key = "TREX_GATEWAY_API_URL"; value = "http://api-stg.trimble.com/t/trimble.com/vss-dev-trexgateway/2.0"},
  @{key = "SCHEDULER_INTERNAL_EXPORT_URL"; value = "http://scheduler.dev.k8s.vspengg.com/internal/v1/export"},
  @{key = "TCCFILESPACENAME"; value = "vldatastore-dev"},
  @{key = "TCCORG"; value = "vldev"},
  @{key = "TCCPWD"; value = "vldev_key"},
  @{key = "TCCUSERNAME"; value = "vldev"},
  @{key = "ENABLE_RAPTOR_GATEWAY_TAGFILE"; value = "true"},
  @{key = "ENABLE_TREX_GATEWAY_TAGFILE"; value = "false"},
  @{key = "ENABLE_TREX_GATEWAY_STATIONOFFSET"; value = "false"},
  @{key = "ENABLE_TREX_GATEWAY_GRIDREPORT"; value = "false"})

# Used when running 3DP service locally but connecting to /alpha deployed collaborating services
$alphaCollaboratorsEnvironmentVariables = @(
  @{key = "CUSTOMERSERVICE_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-customerservice/1.0/Customers/me"},
  @{key = "FILTER_API_URL"; value = "http://filter.alpha.k8s.vspengg.com/api/v1"},
  @{key = "GEOFENCE_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-geofenceservice/1.0"},
  @{key = "IMPORTED_FILE_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-projects/1.4/importedfiles"},
  @{key = "PREFERENCE_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-preferenceservice/1.0"},
  @{key = "PROJECT_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-projects/1.4/project"},
  @{key = "PROJECT_SETTINGS_API_URL"; value = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-projects/1.4"},
  @{key = "TREX_TAGFILE_API_URL"; value = "http://api-stg.trimble.com/t/trimble.com/vss-alpha-trexgateway/2.0/tagfiles"},
  @{key = "TREX_GATEWAY_API_URL"; value = "http://api-stg.trimble.com/t/trimble.com/vss-alpha-trexgateway/2.0"}
  @{key = "SCHEDULER_INTERNAL_EXPORT_URL"; value = "http://scheduler.alpha.k8s.vspengg.com/internal/v1/export"},
  @{key = "TCCFILESPACENAME"; value = "vldatastore-alpha"},
  @{key = "TCCORG"; value = "vlalpha"},
  @{key = "TCCPWD"; value = "vlalpha_key"},
  @{key = "TCCUSERNAME"; value = "vlalpha"},
  @{key = "ENABLE_RAPTOR_GATEWAY_TAGFILE"; value = "true"},
  @{key = "ENABLE_TREX_GATEWAY_TAGFILE"; value = "false"},
  @{key = "ENABLE_TREX_GATEWAY_STATIONOFFSET"; value = "false"},
  @{key = "ENABLE_TREX_GATEWAY_GRIDREPORT"; value = "false"})

# Used when running 3DP service locally but connecting to /prod deployed collaborating services
$prodCollaboratorsEnvironmentVariables = @(
  @{key = "CUSTOMERSERVICE_API_URL"; value = "https://api.trimble.com/t/trimble.com/vss-customerservice/1.0/Customers/me"},
  @{key = "FILTER_API_URL"; value = "https://api.trimble.com/t/trimble.com/vss-3dfilter/1.0"},
  @{key = "GEOFENCE_API_URL"; value = "https://api.trimble.com/t/trimble.com/vss-geofenceservice/1.0"},
  @{key = "IMPORTED_FILE_API_URL"; value = "https://api.trimble.com/t/trimble.com/vss-projectservice/1.4/importedfiles"},
  @{key = "PREFERENCE_API_URL"; value = "https://api.trimble.com/t/trimble.com/vss-preferenceservice/1.0"},
  @{key = "PROJECT_API_URL"; value = "https://api.trimble.com/t/trimble.com/vss-projectservice/1.4/project"},
  @{key = "PROJECT_SETTINGS_API_URL"; value = "https://api.trimble.com/t/trimble.com/vss-projectservice/1.4"},
  @{key = "TREX_TAGFILE_API_URL"; value = "http://api.trimble.com/t/trimble.com/vss-trexgateway/2.0/tagfiles"},
  @{key = "TREX_GATEWAY_API_URL"; value = "http://api.trimble.com/t/trimble.com/vss-trexgateway/2.0"},
  @{key = "SCHEDULER_INTERNAL_EXPORT_URL"; value = ""},
  @{key = "TCCFILESPACENAME"; value = "vldatastore-prod"},
  @{key = "TCCORG"; value = "vlprod"},
  @{key = "TCCPWD"; value = "vlprod_key"},
  @{key = "TCCUSERNAME"; value = "vlprod"},
  @{key = "ENABLE_RAPTOR_GATEWAY_TAGFILE"; value = "true"},
  @{key = "ENABLE_TREX_GATEWAY_TAGFILE"; value = "false"},
  @{key = "ENABLE_TREX_GATEWAY_STATIONOFFSET"; value = "false"},
  @{key = "ENABLE_TREX_GATEWAY_GRIDREPORT"; value = "false"})

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

Write-Host "`nRemember to change your Velociraptor.Config.xml to one compatible with the environment you've chosen."
Write-Host "Finished`n" -ForegroundColor Green