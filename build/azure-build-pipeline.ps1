PARAM (
    [Parameter(Mandatory = $false)][string]$service,
    [Parameter(Mandatory = $false)][string]$action,
    [Parameter(Mandatory = $false)][string]$awsRepositoryName = '940327799086.dkr.ecr.us-west-2.amazonaws.com',
    [Parameter(Mandatory = $false)][string]$branch,
    [Parameter(Mandatory = $false)][string]$buildNumber
)

enum ReturnCode {
    SUCCESS
    INVALID_ACTION
    CANNOT_FIND_PATH
    CONTAINER_BUILD_FAILED
    CONTAINER_CREATE_FAILED
    UNABLE_TO_FIND_IMAGE
    UNABLE_TO_FIND_TEST_RESULTS
    UNABLE_TO_FIND_TEST_COVERAGE
    IMAGE_TAG_FAILED
    IMAGE_PUSH_FAILED
    AWS_ECR_LOGIN_FAILED
}

$services = @{
    Common = 'common'
    Push   = 'service/Push'
}

$servicePath = ''
$serviceName = ''

function Build-Solution {
    $imageTag = "$serviceName-build"

    Write-Host "`nBuilding container image '$imageTag'..." -ForegroundColor Green
    docker build -f $servicePath/build/Dockerfile.build --tag $imageTag --no-cache --build-arg SERVICE_PATH=$servicePath .

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
        Write-Host "`nBuild of '$imageTag' image complete" -ForegroundColor Green
    }

    Exit-With-Code ([ReturnCode]::SUCCESS)
}

function Unit-Test-Solution {
    $container_name = "$serviceName-unittest"

    # Ensure required image exists
    $buildImage = "$serviceName-build:latest"

    if ($(docker images $buildImage -q).Count -eq 0) {
        Write-Host "Unable to find required build image '$buildImage'." -ForegroundColor Green
        Write-Host "Found the following '$serviceName*' images:`n" -ForegroundColor Green
        docker images $serviceName*

        Exit-With-Code ([ReturnCode]::UNABLE_TO_FIND_IMAGE)
    }

    # Clean up from earlier runs
    Remove-Item -Path UnitTestResults -Recurse -ErrorAction SilentlyContinue

    # Build and run containerized unit tests
    Write-Host "`nBuilding unit test container..." -ForegroundColor Green
    docker build --file $servicePath/build/Dockerfile.unittest --tag $container_name --no-cache --build-arg SERVICE_PATH=$servicePath .
    if (-not $?) { Exit-With-Code ([ReturnCode]::CONTAINER_BUILD_FAILED) }

    Write-Host "`nCreating unit test container..." -ForegroundColor Green
    $unique_container_name = "$container_name`_$(Get-Random -Minimum 1000 -Maximum 9999)"

    # Start the container image and terminate and detach immediately
    docker create --name $unique_container_name $container_name
    if (-not $?) { Exit-With-Code ([ReturnCode]::CONTAINER_CREATE_FAILED) }

    Write-Host "`nCopying test results and coverage files..." -ForegroundColor Green
    docker cp $unique_container_name`:/build/$servicePath/test/UnitTests/UnitTestResults/. $servicePath/test/UnitTestResults

    if (-not (Test-Path "$servicePath/test/UnitTestResults/TestResults.xml" -PathType Leaf)) {
        Write-Host 'Unable to find TestResults file on local host.' -ForegroundColor Red
        Exit-With-Code ([ReturnCode]::UNABLE_TO_FIND_TEST_RESULTS)
    }
    if (-not (Test-Path "$servicePath/test/UnitTestResults/Coverage.xml" -PathType Leaf)) {
        Write-Host 'Unable to find test coverage file on local host.' -ForegroundColor Red
        Exit-With-Code ([ReturnCode]::UNABLE_TO_FIND_TEST_COVERAGE)
    }

    Write-Host "`nRemoving test container..." -ForegroundColor Green
    docker rm $unique_container_name
    Write-Host "`nUnit test run complete" -ForegroundColor Green

    Exit-With-Code ([ReturnCode]::SUCCESS)
}

function Publish-Service {
    $publishImage = "$serviceName-webapi"

    # Ensure required image exists
    $buildImage = "push-build:latest"

    if ($(docker images $buildImage -q).Count -eq 0) {
        Write-Host "Unable to find required build image '$buildImage'." -ForegroundColor Green
        Write-Host "Found the following 'pulse*' images:`n" -ForegroundColor Green
        docker images $serviceName*

        Exit-With-Code ([ReturnCode]::UNABLE_TO_FIND_IMAGE)
    }

    # Build and run containerized unit tests
    Write-Host "`nBuilding published service container..." -ForegroundColor Green
    docker build --file $servicePath/build/Dockerfile.runtime --tag $publishImage --no-cache --build-arg SERVICE_PATH=$servicePath .
    if (-not $?) { Exit-With-Code ([ReturnCode]::CONTAINER_BUILD_FAILED) }

    Write-Host "`nPublish application complete" -ForegroundColor Green
    Exit-With-Code ([ReturnCode]::SUCCESS)
}

function Push-Container-Image {
    $publishImage = "$serviceName-webapi"
    $ecr_prefix = 'rpd-ccss-'
    $branch = $branch -replace '.*/' # Remove everything up to and including the last forward slash.

    $versionNumber = $branch + "-" + $buildNumber
    $ecrRepository = "${awsRepositoryName}/${ecr_prefix}${publishImage}:${versionNumber}"

    Write-Host "`nPushing image '$ecrRepository'..." -ForegroundColor Green
    docker tag $publishImage $ecrRepository
    if (-not $?) { Exit-With-Code ([ReturnCode]::IMAGE_TAG_FAILED) }

    docker push $ecrRepository
    if (-not $?) { Exit-With-Code ([ReturnCode]::IMAGE_PUSH_FAILED) }

    Write-Host "`nImage push complete" -ForegroundColor Green

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

# Get on with the real work...

# Set the environment working directory.
Push-Location $PSScriptRoot
[Environment]::CurrentDirectory = $PWD

Set-Location -Path '../src'
if (!$?) { Exit-With-Code ([ReturnCode]::CANNOT_FIND_PATH) }

# Run the script action.
$servicePath = $services[$service]
$serviceName = $service.ToLower()

Write-Host 'Script Variables:' -ForegroundColor Green
Write-Host "  action = $action"
Write-Host "  service = $service"
Write-Host "  servicePath = $servicePath"
Write-Host "  serviceName = $serviceName"
Write-Host "  awsRepositoryName = $awsRepositoryName"
Write-Host "  Working Directory ="($pwd).path

Write-Host "`nAuthenticating with AWS ECR..." -ForegroundColor Green
Invoke-Expression -Command (aws ecr get-login --no-include-email --region us-west-2 --profile default)
if (-not $?) { Exit-With-Code ([ReturnCode]::AWS_ECR_LOGIN_FAILED) }

# Run the appropriate action.
switch ($action) {
    'build' {
        Build-Solution
        continue
    }
    'unittest' {
        Unit-Test-Solution
        continue
    }
    'publish' {
        Publish-Service
        continue
    }
    'pushImage' {
        Push-Container-Image
        continue
    }
    default {
        Write-Host "Invalid action ($action)"
        Exit-With-Code ([ReturnCode]::INVALID_ACTION)
    }
}
