Set-Location $PSScriptRoot
Invoke-Expression "docker-compose --file docker-compose-local.yml down"
