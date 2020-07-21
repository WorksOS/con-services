PARAM (
    [Parameter(Mandatory = $false)][string]$action = '',
    [Parameter(Mandatory = $false)][string]$awsRepositoryName = '940327799086.dkr.ecr.us-west-2.amazonaws.com',
    [Parameter(Mandatory = $false)][string]$ecrRepositoryName = 'rpd-ccss-trex'
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
    IMAGE_TAG_FAILED
    IMAGE_PUSH_FAILED
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

function Push-Container-Image {
    Login-Aws
    
    if ($(docker images ${imageTag} -q).Count -eq 0) {
        Write-Host "Unable to find required build image '${imageTag}'" -ForegroundColor Red

        Exit-With-Code ([ReturnCode]::UNABLE_TO_FIND_IMAGE)
    }

    # Create the full ECR image URI, e.g. 123456789012.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-trex:merge-5776.ProjectRebuilder
    $ecrRepositoryTag = "${awsRepositoryName}/${ecrRepositoryName}:TGLDatabase"

    Write-Host "`Tagging image '$imageTag' as '$ecrRepositoryTag'..." -ForegroundColor Green
    docker tag $imageTag $ecrRepositoryTag
    if (-not $?) { Exit-With-Code ([ReturnCode]::IMAGE_TAG_FAILED) }

    Write-Host "`Pushing image '$ecrRepositoryTag'..." -ForegroundColor Green
    docker push $ecrRepositoryTag
    if (-not $?) { Exit-With-Code ([ReturnCode]::IMAGE_PUSH_FAILED) }

    Write-Host "`nImage push complete" -ForegroundColor Green

    Exit-With-Code ([ReturnCode]::SUCCESS)
}

function Login-Aws {
    Write-Host "`nAuthenticating with AWS ECR..." -ForegroundColor Green
    Write-Host "Determining AWS CLI version..."

    aws --version

    $awsVersion = (aws --version).Split(' ')[0].Split('/')[1].Split(' ')
    $versionMajorMinor = [decimal]($awsVersion[0].SubString(0, $awsVersion.LastIndexOf('.')))
    $canUseGetLoginPassword = $versionMajorMinor -ge 1.18

    if ($canUseGetLoginPassword) {
        # Azure pipelines use a recent version of AWS CLI that has replaced get-login with get-login-password.
        aws ecr get-login-password --region us-west-2 | docker login --username AWS --password-stdin 940327799086.dkr.ecr.us-west-2.amazonaws.com
        if (-not $?) { Exit-With-Code ([ReturnCode]::AWS_ECR_LOGIN_FAILED) }
    }
    else {
        # Retain backward compatibility for running locally on team development PCs with older AWS CLI installed.
        Write-Host "Found older version of AWS CLI, failing back to 'get-login'`n"
        Invoke-Expression -Command (aws ecr get-login --no-include-email --region us-west-2 --profile fsm-okta)
        if (-not $?) { Exit-With-Code ([ReturnCode]::AWS_ECR_LOGIN_FAILED) }
    }
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
    'pushImage' {
        Push-Container-Image
        continue
    }
    '' {
        Write-Host "`nNo Action set; running build and push (image)." -ForegroundColor Green
        Build-Container-Image
        Push-Container-Image
        continue
    }
    default {
        Write-Host "Invalid action ($action)"
        Exit-With-Code ([ReturnCode]::INVALID_ACTION)
    }
}
  