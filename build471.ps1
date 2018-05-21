param ([switch] $uploadArtifact = $false,
       [string] $artifactFilename = "TagFileAuth_WebApi.zip")

$artifactsDir = "$PSScriptRoot/artifacts"

# Tidy up old artifacts.
Write-Host "Removing existing build artifacts..." -ForegroundColor "darkgray"

if (Test-Path -path $artifactsDir) {
    Remove-Item -Force -Recurse -Path $artifactsDir 
}

If (Test-path $artifactFilename) {
    Remove-item $artifactFilename
}

# Restore, build/publish for configuration net471.
Write-Host "Restoring .NET packages..." -ForegroundColor "darkgray"
Invoke-Expression "dotnet restore --no-cache VSS.TagFileAuth.Service.sln"

Write-Host "Publishing WebApi project..." -ForegroundColor "darkgray"
Invoke-Expression "dotnet publish ./src/WebApi/VSS.Productivity3D.TagFileAuth.WebAPI.csproj -o ../../artifacts/TagFileAuthWebApiNet471 -f net471"
if ($LastExitCode -ne 0) {
    throw "Publish of web api project **** Failed ****"
}

$artifactsWorkingDir = "$artifactsDir\TagFileAuthWebApiNet471"

# Copy build files to working artifacts folder.
if (!(Test-Path -path $artifactsWorkingDir)) {
    New-Item $artifactsWorkingDir -Type Directory
}

Write-Host "Copying static deployment files..." -ForegroundColor "darkgray"
Copy-Item -Path "$PSScriptRoot\src\WebApi\appsettings.json" -Destination $artifactsWorkingDir
Copy-Item -Path "$PSScriptRoot\src\WebApi\Dockerfile" -Destination $artifactsWorkingDir
Copy-Item -Path "$PSScriptRoot\src\WebApi\web.config" -Destination $artifactsWorkingDir
Copy-Item -Path "$PSScriptRoot\src\WebApi\log4net.xml" -Destination $artifactsWorkingDir

# Compress build artifacts.
Write-Host "Compressing build artifacts..." -ForegroundColor "darkgray"

Add-Type -assembly "system.io.compression.filesystem"
[io.compression.zipfile]::CreateFromDirectory($artifactsDir, "$PSScriptRoot/$artifactFilename") 

# Upload build artifacts to S3.
if ($uploadArtifact) {
    Write-Host "Uploading build artifacts to S3..." -ForegroundColor "darkgray"
    Invoke-Expression "aws s3 cp $artifactFilename s3://vss-merino/Productivity3D/Releases/$artifactFilename --acl public-read --profile vss-grant"
}