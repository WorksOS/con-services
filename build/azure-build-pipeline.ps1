PARAM (
    # General
    [Parameter(Mandatory = $true)][string]$service, # The application service to build, test.
    [Parameter(Mandatory = $true)][string]$action, # Actions include build | unittest | publish 
    [Parameter(Mandatory = $false)][string]$dockerFile, # Optional Dockerfile. If defined will be used in the selected 'action'.
    # Unit testing
    [Parameter(Mandatory = $false)][ValidateSet("true", "false")][string]$recordTestResults = "true",
    [Parameter(Mandatory = $false)][ValidateSet("true", "false")][string]$collectCoverage = "true",
    # Image publish parameters
    [Parameter(Mandatory = $false)][string]$sourceArtifactPath = '',
    [Parameter(Mandatory = $false)][string]$destArtifactPath = '',
    # Image Push parameters
    [Parameter(Mandatory = $false)][string]$awsRepositoryName = '940327799086.dkr.ecr.us-west-2.amazonaws.com',
    [Parameter(Mandatory = $false)][string]$branch,
    [Parameter(Mandatory = $false)][string]$ecrRepositoryName,
    [Parameter(Mandatory = $false)][string]$imageSuffix
    # Update NuGet sources
    #[Parameter(Mandatory = $false)][string]$systemAccessToken, 
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
    OPERATION_FAILED
}

$services = @{
    Common         = 'Common'
    Entitlements   = 'service/Entitlements'
    Healthz        = 'service/Healthz'
    Megalodon      = 'service/Megalodon'
    Mock           = 'service/MockProjectWebApi'
    Productivity3d = 'service/3DP'
    Push           = 'service/Push'
    ThreeDNow      = 'service/3dNow'
    TRex           = 'service/TRex'
    TRexWebTools   = 'service/TRex' # placeholder
}

$servicePath = ''
$serviceName = ''

function Build-Solution {
    Login-Aws

    $imageTag = "$serviceName-build"
    if (-not($dockerFile)) { $dockerFile = 'Dockerfile.build' }

    Write-Host "`nBuilding container image '$imageTag'..." -ForegroundColor Green
    docker build -f $servicePath/build/$dockerFile --tag $imageTag --no-cache --build-arg SERVICE_PATH=$servicePath .

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
}

function Run-Unit-Tests {
    $container_name = "$serviceName-unittest"
    if (-not($dockerFile)) { $dockerFile = 'Dockerfile.unittest' }

    # Ensure required image exists
    $buildImage = "$serviceName-build:latest"

    if ($(docker images $buildImage -q).Count -eq 0) {
        Write-Host "Unable to find required build image '$buildImage'." -ForegroundColor Red
        Write-Host "Found the following '$serviceName*' images:`n" -ForegroundColor Red
        docker images $serviceName*

        Exit-With-Code ([ReturnCode]::UNABLE_TO_FIND_IMAGE)
    }

    # Clean up from earlier runs
    $localTestResultsFolder = "UnitTestResults"
    Remove-Item -Path "$servicePath/$localTestResultsFolder" -Recurse -ErrorAction SilentlyContinue

    # Build and run containerized unit tests
    Write-Host "`nBuilding unit test container..." -ForegroundColor Green

    # We don't require a build context here because everything needed is present in the already present [service]-build image.
    # Docker build allows the - < token indicating the dockerfile is passed via STDIN; this means the build context only consists of the Dockerfile.
    # Powershell doesn't have an input redirection feature so it's done using the Get-Content cmdlet.
    Get-Content $servicePath/build/$dockerFile | docker build --tag $container_name `
        --no-cache `
        --build-arg FROM_IMAGE=$buildImage `
        --build-arg SERVICE_PATH=$servicePath `
        --build-arg COLLECT_COVERAGE=$collectCoverage - 
    
    Write-Host "`nCreating unit test container..." -ForegroundColor Green
    $unique_container_name = "$container_name`_$(Get-Random -Minimum 1000 -Maximum 9999)"

    # Had instances of the docker login failing, possibly due to AWS token expiry? 
    Login-Aws

    # Start the container image and terminate and detach immediately
    docker create --name $unique_container_name $container_name
    if (-not $?) { Exit-With-Code ([ReturnCode]::CONTAINER_CREATE_FAILED) }

    if ($recordTestResults -eq $true -Or $collectCoverage -eq $true) {
        Write-Host "Copying files from container /UnitTestResults/ to local host..." -ForegroundColor Green
        docker cp $unique_container_name`:/build/$servicePath/UnitTestResults/. $servicePath/$localTestResultsFolder
        Write-Host "Listing results of file copy..." -ForegroundColor Green
        Get-ChildItem $servicePath/$localTestResultsFolder

        if ($recordTestResults -eq $true) {
            if (-not (Test-Path -Path $servicePath/$localTestResultsFolder/* -Include *.trx)) {
                Write-Host "Unable to find any .trx results files on local host." -ForegroundColor Red
                Exit-With-Code ([ReturnCode]::UNABLE_TO_FIND_TEST_RESULTS)
            }
        }
    
        if ($collectCoverage -eq $true) {
            $coveragePath = "$servicePath/$localTestResultsFolder/coverage*cobertura.xml"
    
            if (-not (Test-Path $coveragePath -PathType Leaf)) {
                Write-Host "Unable to find test coverage file '$coveragePath' on local host." -ForegroundColor Red
                Exit-With-Code ([ReturnCode]::UNABLE_TO_FIND_TEST_COVERAGE)
            }
        }
    }

    Write-Host "`nRemoving test container..." -ForegroundColor Green
    docker rm $unique_container_name
    Write-Host "`nUnit test run complete" -ForegroundColor Green
}

function Publish-Service {
    $publishImage = "$serviceName-webapi"

    # Ensure required image exists
    $buildImage = "$serviceName-build:latest"

    if ($(docker images $buildImage -q).Count -eq 0) {
        Write-Host "Unable to find required build image '$buildImage'." -ForegroundColor Red
        Write-Host "Found the following '$serviceName' images:`n" -ForegroundColor Red
        docker images $serviceName*

        Exit-With-Code ([ReturnCode]::UNABLE_TO_FIND_IMAGE)
    }

    # Build and run containerized unit tests
    Write-Host "`nBuilding published service container..." -ForegroundColor Green

    # We don't require a build context here because everything needed is present in the already present [service]-build image.
    # Docker build allows the - < token indicating the dockerfile is passed via STDIN; this means the build context only consists of the Dockerfile.
    # Powershell doesn't have an input redirection feature so it's done using the Get-Content cmdlet.

    # Artifact paths are optional where we let the Dockerfile define the default values.
    $sourceArtifactPathArg = '';
    if ($sourceArtifactPath) { $sourceArtifactPathArg = '=' + $sourceArtifactPath }
    $destArtifactPathArg = ''
    if ($destArtifactPath) { $destArtifactPathArg = '=' + $destArtifactPath }

    if (-not($dockerFile)) { $dockerFile = 'Dockerfile.runtime' }

    Get-Content $servicePath/build/$dockerFile | docker build `
    --tag $publishImage `
    --no-cache `
    --build-arg FROM_IMAGE=$buildImage `
    --build-arg SERVICE_PATH=$servicePath `
    --build-arg SOURCE_PATH$sourceArtifactPathArg `
    --build-arg DEST_PATH$destArtifactPathArg -

    if (-not $?) { Exit-With-Code ([ReturnCode]::CONTAINER_BUILD_FAILED) }

    Write-Host "`nPublish application complete" -ForegroundColor Green
}

function Push-Container-Image {
    Login-Aws

    $publishImage = "$serviceName-webapi"

    if ($(docker images $publishImage -q).Count -eq 0) {
        Write-Host "Unable to find required publish image '$publishImage'. Looking for build image..." -ForegroundColor Green
        $publishImage = "$serviceName-build"

        if ($(docker images $publishImage -q).Count -eq 0) {
            Write-Host "Unable to find required build image '$publishImage'." -ForegroundColor Red
            Write-Host "Found the following '$serviceName' images:`n" -ForegroundColor Red
            docker images $serviceName*
    
            Exit-With-Code ([ReturnCode]::UNABLE_TO_FIND_IMAGE)
        }
        else {
            Write-Host "Found fallback image '$publishImage'" -ForegroundColor Green
        }
    }

    $branch = $branch -replace '.*/' # Remove everything up to and including the last forward slash.

    # Create the full ECR image URI, e.g. 123456789012.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-trex:merge-5776.ProjectRebuilder
    $tagSuffix = $branch + "-" + $imageSuffix
    $ecrRepository = "${awsRepositoryName}/${ecrRepositoryName}:${tagSuffix}"

    Write-Host "`nPushing image '$ecrRepository'..." -ForegroundColor Green
    docker tag $publishImage $ecrRepository
    if (-not $?) { Exit-With-Code ([ReturnCode]::IMAGE_TAG_FAILED) }

    docker push $ecrRepository
    if (-not $?) { Exit-With-Code ([ReturnCode]::IMAGE_PUSH_FAILED) }

    Write-Host "`nImage push complete" -ForegroundColor Green
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

# May be required when interacting with TGL or trmb-ccss Nuget servers.
# function Update-Nuget-Sources {
#     $sourceName = 'trmb-ccss'

#     if (-not $(nuget sources List | Select-String -Pattern 'trmb-ccss' -Quiet)) {
#         Write-Host "`nAdding source '$sourceName' to the NuGet configuration file..." -ForegroundColor Green
#         & '..\build\nuget\nuget.exe' sources add -Name "${sourceName}" -Source "https://pkgs.dev.azure.com/trmb-ccss/_packaging/trmb-ccss/nuget/v3/index.json" -ConfigFile "NuGet.Config"
#         if (-not $?) { Exit-With-Code ([ReturnCode]::OPERATION_FAILED) }
#     }

#     Write-Host "`nUpdating credentials for NuGet source '$sourceName'..." -ForegroundColor Green
#     & '..\build\nuget\nuget.exe' sources update -Name "${sourceName}" -Username "az" -Password "${systemAccessToken}" -ConfigFile "NuGet.Config"
#     if (-not $?) { Exit-With-Code ([ReturnCode]::OPERATION_FAILED) }
# }
function TrackTime($Time) {
    if (!($Time)) { 
        Return 
    }
    Else {
        $executionTime = ((Get-Date) - $Time)
        $executionMinutes = "{0:N2}" -f $executionTime.TotalMinutes
        Write-Host "Script completed in ${executionMinutes} minutes."
    }
}

function Docker-Image-Prune {
    # This should help prevent the build agent from becoming too cluttered.
    Write-Host "`nPrune all images created more than 48 hours ago..." -ForegroundColor Green
    docker image prune -a --force --filter "until=48h"
}

function Docker-Container-Prune {
    # Remove any running or stopped containers on this build agent.
    Write-Host "`nRemoving old application containers...`n" -ForegroundColor Green
    docker ps
    docker container prune --force --filter "until=12h"
}
function Exit-With-Code {
    param(
        [ReturnCode][Parameter(Mandatory = $true)]$code
    )

    TrackTime $timeStart

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
$servicePath = $services[$service -replace '-']
$serviceName = $service.ToLower()

Write-Host 'Script Variables:' -ForegroundColor Green
Write-Host "  service = '$service'"
Write-Host "  action = '$action'"
Write-Host "  branch = '$branch'"
Write-Host "  buildId = '$buildId'"
Write-Host "  servicePath = '$servicePath'"
Write-Host "  serviceName = '$serviceName'"
Write-Host "  recordTestResults = $recordTestResults"
Write-Host "  collectCoverage = $collectCoverage"
Write-Host "  sourceArtifactPath = '$sourceArtifactPath'"
Write-Host "  destArtifactPath = '$destArtifactPath'"
Write-Host "  ecrRepositoryName = '$ecrRepositoryName'"
Write-Host "  awsRepositoryName = '$awsRepositoryName'"
Write-Host "  Working Directory ="($pwd).path

$timeStart = Get-Date

# Run the appropriate action.
switch ($action) {
    'dockerImagePrune' {
        Docker-Image-Prune
        continue
    }
    'dockerContainerPrune' {
        Docker-Container-Prune
        continue
    }
    'build' {
        Build-Solution
        continue
    }
    'unittest' {
        Run-Unit-Tests
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
    'publishAndPushImage' {
        Publish-Service
        Push-Container-Image
        continue
    }
    
    'updateNugetSources' {
        Update-Nuget-Sources
        continue
    }
    default {
        Write-Host "Invalid action ($action)"
        Exit-With-Code ([ReturnCode]::INVALID_ACTION)
    }
}

Exit-With-Code ([ReturnCode]::SUCCESS)
