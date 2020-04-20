PARAM (
    [Parameter(Mandatory = $false)][string]$globalConfigFile = "../../config/dev-config.yaml",
    [Parameter(Mandatory = $false)][string]$localConfigFile = "build/yaml/testing-configmap.yaml",
    [bool]$useLocalOverride = $true # When set will use a local projection, typically service URLs for locally running services, e.g. MockWebApi.
)

& "../../config/apply-config-local.ps1" -globalConfigFile $globalConfigFile -localConfigFile $localConfigFile -useLocalOverride $useLocalOverride