$artifactsDir = "$PSScriptRoot/artifacts"

# Tidy up old artifacts.
Write-Host "Removing existing build artifacts..." -ForegroundColor "darkgray"

if (Test-Path -path $artifactsDir) {
    Remove-Item -Force -Recurse -Path $artifactsDir 
}

# Restore, build/publish for configuration net47.
Write-Host "Publishing WebApi project..." -ForegroundColor "darkgray"
$artifactsWorkingDir = "$artifactsDir\ProjectWebApi"

Invoke-Expression "dotnet publish ./src/ProjectWebApi/VSS.Project.WebApi.csproj -o $artifactsWorkingDir -f netcoreapp2.0"

# FIX: To avoid runtime error with Confluence Kafka, see https://github.com/confluentinc/confluent-kafka-dotnet/issues/364
$rdKafkaPath = "$artifactsWorkingDir/runtimes/win7-x86/native/"

if (-Not (Test-Path -path $rdKafkaPath)) {
    New-Item -Path $rdKafkaPath -ItemType Directory
}

Copy-Item "$artifactsWorkingDir/librdkafka.dll" $rdKafkaPath
# <-- END FIX