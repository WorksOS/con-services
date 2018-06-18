<# 
  Don't forget to also change the Velociraptor.Config.xml to the one for the correct environment
 #>
<# Alpha environment #>
<#
[Environment]::SetEnvironmentVariable("CUSTOMERSERVICE_API_URL", "https://api-stg.trimble.com/t/trimble.com/vss-alpha-customerservice/1.0/Customers/me", "Machine")
[Environment]::SetEnvironmentVariable("GEOFENCE_API_URL", "https://api-stg.trimble.com/t/trimble.com/vss-alpha-geofenceservice/1.0", "Machine")
[Environment]::SetEnvironmentVariable("FILTER_API_URL", "http://10.97.96.103:9001/api/v1", "Machine")
[Environment]::SetEnvironmentVariable("IMPORTED_FILE_API_URL", "https://api-stg.trimble.com/t/trimble.com/vss-alpha-projects/1.4/importedfiles", "Machine")
[Environment]::SetEnvironmentVariable("PREFERENCE_API_URL", "https://api-stg.trimble.com/t/trimble.com/vss-alpha-preferenceservice/1.0", "Machine")
[Environment]::SetEnvironmentVariable("PROJECT_API_URL", "https://api-stg.trimble.com/t/trimble.com/vss-alpha-projects/1.4/project?includeLandfill=true", "Machine")
[Environment]::SetEnvironmentVariable("PROJECT_SETTINGS_API_URL", "https://api-stg.trimble.com/t/trimble.com/vss-alpha-projects/1.4", "Machine")
[Environment]::SetEnvironmentVariable("SCHEDULER_INTERNAL_EXPORT_URL", "http://10.97.96.103:9011/internal/v1/export", "Machine")
[Environment]::SetEnvironmentVariable("TCCFILESPACEID", "ua9bc5eaf-583a-44d3-b33b-24c035946ae2", "Machine")
[Environment]::SetEnvironmentVariable("TCCFILESPACENAME", "vldatastore-beta", "Machine")
[Environment]::SetEnvironmentVariable("TCCORG", "vlbeta", "Machine")
[Environment]::SetEnvironmentVariable("TCCPWD", "vlbeta_key8", "Machine")
[Environment]::SetEnvironmentVariable("TCCUSERNAME", "vlbeta", "Machine")
#>

<# Localhost with mocks #>

[Environment]::SetEnvironmentVariable("CUSTOMERSERVICE_API_URL", "http://localhost:5001/api/v1/mock/getcustomersforme", "Machine")
[Environment]::SetEnvironmentVariable("GEOFENCE_API_URL", "http://localhost:5001/api/v1/mock/geofences", "Machine")
[Environment]::SetEnvironmentVariable("FILTER_API_URL", "http://localhost:5001/api/v1/mock", "Machine")
[Environment]::SetEnvironmentVariable("IMPORTED_FILE_API_URL", "http://localhost:5001/api/v4/mock/importedfiles", "Machine")
[Environment]::SetEnvironmentVariable("PREFERENCE_API_URL", "http://localhost:5001/api/v1/mock/preferences", "Machine")
[Environment]::SetEnvironmentVariable("PROJECT_API_URL", "http://localhost:5001/api/v4/mockproject", "Machine")
[Environment]::SetEnvironmentVariable("PROJECT_SETTINGS_API_URL", "http://localhost:5001/api/v4/mock", "Machine")
[Environment]::SetEnvironmentVariable("SCHEDULER_INTERNAL_EXPORT_URL", "http://localhost:5001/internal/v1/mock/export", "Machine")
[Environment]::SetEnvironmentVariable("TCCFILESPACEID", "u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01", "Machine")
[Environment]::SetEnvironmentVariable("TCCFILESPACENAME", "vldatastore-dev", "Machine")
[Environment]::SetEnvironmentVariable("TCCORG", "vldev", "Machine")
[Environment]::SetEnvironmentVariable("TCCPWD", "vldev_key", "Machine")
[Environment]::SetEnvironmentVariable("TCCUSERNAME", "vdev", "Machine")


<# Dev Environment #>
<#
[Environment]::SetEnvironmentVariable("CUSTOMERSERVICE_API_URL", "https://api-stg.trimble.com/t/trimble.com/vss-alpha-customerservice/1.0/Customers/me", "Machine")
[Environment]::SetEnvironmentVariable("GEOFENCE_API_URL", "https://api-stg.trimble.com/t/trimble.com/vss-alpha-geofenceservice/1.0", "Machine")
[Environment]::SetEnvironmentVariable("FILTER_API_URL", "http://10.97.96.103:3010/api/v1", "Machine")
[Environment]::SetEnvironmentVariable("IMPORTED_FILE_API_URL", "https://api-stg.trimble.com/t/trimble.com/vss-dev-projects/1.4/importedfiles", "Machine")
[Environment]::SetEnvironmentVariable("PREFERENCE_API_URL", "https://api-stg.trimble.com/t/trimble.com/vss-alpha-preferenceservice/1.0", "Machine")
[Environment]::SetEnvironmentVariable("PROJECT_API_URL", "https://api-stg.trimble.com/t/trimble.com/vss-dev-projects/1.4/project?includeLandfill=true", "Machine")
[Environment]::SetEnvironmentVariable("PROJECT_SETTINGS_API_URL", "https://api-stg.trimble.com/t/trimble.com/vss-dev-projects/1.4", "Machine")
[Environment]::SetEnvironmentVariable("SCHEDULER_INTERNAL_EXPORT_URL", "http://10.97.96.103:3011/internal/v1/export", "Machine")
[Environment]::SetEnvironmentVariable("TCCFILESPACEID", "u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01", "Machine")
[Environment]::SetEnvironmentVariable("TCCFILESPACENAME", "vldatastore-dev", "Machine")
[Environment]::SetEnvironmentVariable("TCCORG", "vldev", "Machine")
[Environment]::SetEnvironmentVariable("TCCPWD", "vldev_key", "Machine")
[Environment]::SetEnvironmentVariable("TCCUSERNAME", "vdev", "Machine")
#>


