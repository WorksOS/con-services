param ([switch] $uploadArtifact = $false)

$artifactsDir = "$PSScriptRoot/artifacts"
$artfifactZip = "ProjectWebApiNet47.zip"

# Tidy up old artifacts.
Write-Host "Removing existing build artifacts..." -ForegroundColor "darkgray"

if (Test-Path -path $artifactsDir) {
    Remove-Item -Force -Recurse -Path $artifactsDir 
}

If (Test-path $artfifactZip) {
    Remove-item $artfifactZip
}

# Restore, build/publish for configuration net47.
Write-Host "Restoring .NET packages..." -ForegroundColor "darkgray"
Invoke-Expression "dotnet restore --no-cache VSS.Visionlink.Project.sln"

Write-Host "Publishing WebApi project..." -ForegroundColor "darkgray"
$artifactsWorkingDir = "$artifactsDir\ProjectWebApiNet47"

Invoke-Expression "dotnet publish ./src/ProjectWebApi/VSS.Project.WebApi.csproj -o $artifactsWorkingDir -f net47"

# FIX: To avoid runtime error with Confluence Kafka, see https://github.com/confluentinc/confluent-kafka-dotnet/issues/364
$rdKafkaPath = "$artifactsWorkingDir/runtimes/win7-x86/native/"

if (-Not (Test-Path -path $rdKafkaPath)) {
    New-Item -Path $rdKafkaPath -ItemType Directory
}

Copy-Item "$artifactsWorkingDir/librdkafka.dll" $rdKafkaPath
# <-- END FIX

# Compress build artifacts.
Write-Host "Compressing build artifacts..." -ForegroundColor "darkgray"

Add-Type -assembly "system.io.compression.filesystem"
[io.compression.zipfile]::CreateFromDirectory($artifactsDir, "$PSScriptRoot/$artfifactZip") 
[io.compression.zipfile]::CreateFromDirectory("$PSScriptRoot/database", "$PSScriptRoot/ProjectWebApiDb.zip")

# Upload build artifacts to S3.
if ($uploadArtifact) {
    Write-Host "Uploading build artifacts to S3..." -ForegroundColor "darkgray"
    Invoke-Expression "aws s3 cp $artfifactZip s3://vss-merino/Productivity3D/Releases/$artfifactZip --acl public-read --profile vss-grant"
}