PARAM (
    [Parameter(Mandatory = $false)][string]$action = ""
)

# Set the environment working directory to the script's location.
Push-Location $PSScriptRoot
[Environment]::CurrentDirectory = $PWD

enum ReturnCode {
    SUCCESS
    INVALID_ACTION
    CANNOT_FIND_PATH
    CONTAINER_BUILD_FAILED
    CONTAINER_CREATE_FAILED
    CONTAINER_RUN_FAILED
    UNABLE_TO_FIND_IMAGE
    DOCKER_COPY_FAILED
    S3_UPLOAD_FAILED
}

$imageTag = 'trex_tgl_geodata'

function Build-Container-Image {
    Write-Host "`nBuilding container image '$imageTag'..." -ForegroundColor Green
    docker build --file Dockerfile.tgl --tag $imageTag --no-cache .

    if (!$?) { Exit-With-Code ([ReturnCode]::CONTAINER_BUILD_FAILED) }
  
    Write-Host "`nImage details:" -ForegroundColor Green
    $image = docker images $imageTag
  
    if ($image.Count -eq 0) {
        Write-Host "`nUnable to validate container image" -ForegroundColor Red
        Exit-With-Code ([ReturnCode]::UNABLE_TO_FIND_IMAGE)
    }
    else {
        Write-Host $image[0]
        Write-Host $image[1]
        Write-Host "`nImage build completed" -ForegroundColor Green
    }
  
    Exit-With-Code ([ReturnCode]::SUCCESS)
}

function Sync-With-S3 {
    $s3host = 's3://s3-pdxa-rpd-geoid-data'
    $awsProfile = 'fsm-okta'

    Remove-Item ./tmp -Recurse -ErrorAction Continue

    Write-Host "`nCopy GeoData files from the container to host..." -ForegroundColor Green
    docker cp ${imageTag}:/app/tgl_geodata/ ./tmp/
    if (!$?) { Exit-With-Code ([ReturnCode]::DOCKER_COPY_FAILED) }

    Remove-Item ./tmp/.* -ErrorAction Continue

    Write-Host "`nRunning s3 sync to '$s3Host' using AWS profile '$awsProfile'" -ForegroundColor Green
    aws s3 sync ./tmp $s3Host --profile $awsProfile --delete
    if (!$?) { Exit-With-Code ([ReturnCode]::S3_UPLOAD_FAILED) }

    Write-Host "`nFile sync process completed" -ForegroundColor Green
    Exit-With-Code ([ReturnCode]::SUCCESS)
}

function Exit-With-Code {
    param(
        [ReturnCode][Parameter(Mandatory = $true)]$code
    )
  
    if ($code -eq [ReturnCode]::SUCCESS) {
        Write-Host "`nExiting: $code" -ForegroundColor Green
    }
    else {
        Write-Host "`nExiting with error: $code" -ForegroundColor Red
    }
  
    Pop-Location
    Exit $code
}

# Run the appropriate action.
switch ($action) {
    'build' {
        Build-Container-Image
        continue
    }
    'sync' {
        Sync-With-S3
        continue
    }
    '' {
        Write-Host "`nNo Action set; running build and sync." -ForegroundColor Green
        Build-Container-Image
        Sync-With-S3
        continue
    }
    default {
        Write-Host "Invalid action ($action)"
        Exit-With-Code ([ReturnCode]::INVALID_ACTION)
    }
}
  