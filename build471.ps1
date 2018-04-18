param ([switch] $uploadArtifact = $false)

$artifactsDir = "$PSScriptRoot/artifacts"
$artfifactZip = "3DPRaptorWebApi.zip"

# Tidy up old artifacts.
Write-Host "Removing existing build artifacts..." -ForegroundColor "darkgray"

if (Test-Path -path $artifactsDir) {
    Remove-Item -Force -Recurse -Path $artifactsDir 
}

If (Test-path $artfifactZip) {
    Remove-item $artfifactZip
}

# Restore, build/publish for configuration net471.
Write-Host "Restoring .NET packages..." -ForegroundColor "darkgray"
Invoke-Expression "dotnet restore ./VSS.Productivity3D.Service.sln --no-cache"

Write-Host "Publishing WebApi project..." -ForegroundColor "darkgray"
Invoke-Expression "dotnet publish ./src/WebApi/VSS.Productivity3D.WebApi.csproj -o ../../Artifacts/WebApi -f net471"
if ($LastExitCode -ne 0)
   {   throw "Publish of web api project **** Failed ****"  }

Write-Host "Build Unit Tests project..." -ForegroundColor "darkgray"
Invoke-Expression "dotnet build ./test/UnitTests/WebApiTests/VSS.Productivity3D.WebApi.Tests.csproj"
if ($LastExitCode -ne 0)
   {   throw "Build unit tests project **** Failed ****"  }

Write-Host "Build Acceptance Tests project..." -ForegroundColor "darkgray"
Invoke-Expression "dotnet build ./AcceptanceTests/VSS.Productivity3D.Service.AcceptanceTests.sln"
if ($LastExitCode -ne 0)
   {   throw "Build Acceptance Tests project **** Failed ****"  }

$artifactsWorkingDir = "$artifactsDir\webapi"

# Copy build files to working artifacts folder.
if (!(Test-Path -path $artifactsWorkingDir)) {
    New-Item $artifactsWorkingDir -Type Directory
    New-Item "$artifactsWorkingDir/logs" -Type Directory # This is a carry over from build.bat, is it needed? (Aaron)
}

Write-Host "Copying static deployment files..." -ForegroundColor "darkgray"
Copy-Item -Path "$PSScriptRoot\src\WebApi\appsettings.json" -Destination $artifactsWorkingDir
Copy-Item -Path "$PSScriptRoot\src\WebApi\Dockerfile" -Destination $artifactsWorkingDir
Copy-Item -Path "$PSScriptRoot\src\WebApi\SetupWebAPI.ps1" -Destination $artifactsWorkingDir
Copy-Item -Path "$PSScriptRoot\src\WebApi\Velociraptor.Config.Xml" -Destination $artifactsWorkingDir
Copy-Item -Path "$PSScriptRoot\src\WebApi\web.config" -Destination $artifactsWorkingDir
Copy-Item -Path "$PSScriptRoot\src\WebApi\log4net.xml" -Destination $artifactsWorkingDir

# Compress build artifacts.
Write-Host "Compressing build artifacts..." -ForegroundColor "darkgray"

Add-Type -assembly "system.io.compression.filesystem"
[io.compression.zipfile]::CreateFromDirectory($artifactsDir, "$PSScriptRoot/$artfifactZip") 

# Upload build artifacts to S3.
if ($uploadArtifact) {
    Write-Host "Uploading build artifacts to S3..." -ForegroundColor "darkgray"
    Invoke-Expression "aws s3 cp $artfifactZip s3://vss-merino/Productivity3D/Releases/$artfifactZip --acl public-read --profile vss-grant"
}