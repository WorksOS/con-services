enum ReturnCode {
    SUCCESS
    AWS_ECR_LOGIN_FAILED
    OPERATION_FAILED
}

function Login-Aws {
    Write-Host "`n##[section]Authenticating with AWS ECR..." -ForegroundColor Green
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
        Write-Host "##[section]Found older version of AWS CLI, failing back to 'get-login'`n" -ForegroundColor Green
        Invoke-Expression -Command (aws ecr get-login --no-include-email --region us-west-2 --profile fsm-okta)
        if (-not $?) { Exit-With-Code ([ReturnCode]::AWS_ECR_LOGIN_FAILED) }
    }
}

function Exit-With-Code {
    param(
        [ReturnCode][Parameter(Mandatory = $true)]$code
    )

    if ($code -eq [ReturnCode]::SUCCESS) {
        Write-Host "`n##[command]Exiting: $code" -ForegroundColor Green
    }
    else {
        Write-Host "`n##[error]Exiting with error: $code" -ForegroundColor Red
    }

    Pop-Location
    Exit $code
}

Login-Aws

# This is not ideal; but too often the containers fail to start due to drive or volume errors on the existing containers.
# It's possible the build agent will be reused and containers from an earlier build are still running.
Write-Host "`n##[section]Removing old application containers..." -ForegroundColor Green
docker ps -q --filter "name=acceptancetests_" | ForEach-Object { docker stop $_ }
docker ps -aq --filter "name=acceptancetests_" | ForEach-Object { docker rm $_ }

Write-Host "`n##[section]Removing old acceptance test images..." -ForegroundColor Green
docker images acceptancetests* -q | Foreach-Object { docker image rm $_ }

Push-Location $PSScriptRoot/../..
Set-Location $PSScriptRoot/../..
[Environment]::CurrentDirectory = $PWD

Remove-Item -Path ./build/artifacts -Recurse -Force -ErrorAction Ignore
New-Item -ItemType directory ./build/artifacts/logs -ErrorAction Ignore | out-null

Write-Host "`n##[section]Publishing WebAPI project..." -ForegroundColor Green
Invoke-Expression "dotnet publish ./src/ProjectWebApi/VSS.Project.WebApi.csproj --configuration Release --output ./build/artifacts/Project --framework netcoreapp3.1 --runtime linux-x64 -nowarn:NU1701 -nowarn:CS1591"

if ($LASTEXITCODE -ne 0) { Exit-With-Code ([ReturnCode]::OPERATION_FAILED) }

Write-Host "`n##[section]Copying static deployment files... " -NoNewline -ForegroundColor Green
Copy-Item ./src/ProjectWebApi/appsettings.json ./build/artifacts/Project
Copy-Item ./src/ProjectWebApi/Dockerfile ./build/artifacts/Project
Copy-Item ./src/ProjectWebApi/web.config ./build/artifacts/Project
Write-Host "Done"

Write-Host "`n##[section]Build and setup Acceptance Tests solution..." -ForegroundColor Green
Set-Location $PSScriptRoot/../../AcceptanceTests

Remove-Item -Path ./deploy -Recurse -Force -ErrorAction Ignore

New-Item -ItemType directory ./deploy | out-null
New-Item -ItemType directory ./deploy/testresults | out-null

Copy-Item ./Dockerfile ./deploy
Copy-Item ./scripts/runtests.sh ./deploy
Copy-Item ./scripts/wait-for-it.sh ./deploy
Copy-Item ./scripts/rm_cr.sh ./deploy

Write-Host "##[section]Publishing acceptance test projects..." -ForegroundColor Green
Invoke-Expression "dotnet publish ./tests/IntegrationTests/IntegrationTests.csproj -o ./deploy/IntegrationTests -f netcoreapp3.1 -nowarn:NU1701 -nowarn:CS1591"

if ($LASTEXITCODE -ne 0) { Exit-With-Code ([ReturnCode]::OPERATION_FAILED) }

# Load environment variables
#& "${PSScriptRoot}/testingvars.ps1"

# Docker Compose Pull & Up
Push-Location $PSScriptRoot/../..
Write-Host "`n##[section]Processing docker-compose.yml for 'pull'..." -ForegroundColor Green
Invoke-Expression "docker-compose --file build/acceptancetests/docker-compose.yml pull"

Write-Host "`n##[section]Building Docker containers" -ForegroundColor Green
Invoke-Expression "docker-compose --file build/acceptancetests/docker-compose.yml up --build --detach"

Write-Host "`n##[section]Running Docker containers:`n"
Invoke-Expression "docker ps"

Write-Host "`n##[command]docker logs --follow acceptancetests_accepttest_1:`n"
docker logs --follow acceptancetests_accepttest_1 
#docker wait acceptancetests_accepttest_1

# Run the acceptance tests

# Invoke-Expression "dotnet test ./AcceptanceTests/tests/IntegrationTests/IntegrationTests.csproj --logger trx --results-directory AcceptanceTestResults -nowarn:NU1701"

Write-Host "`n##[section]Copying Acceptance tests results file..." -NoNewline -ForegroundColor Green
Remove-Item -Path ./AcceptanceTestResults -Recurse -Force -ErrorAction Ignore
New-Item -ItemType directory ./AcceptanceTestResults | out-null

docker cp acceptancetests_accepttest_1:/app/AcceptanceTestResults/. AcceptanceTestResults

Write-Host "Done"
Write-Host "`nListing results of file copy..."
Get-ChildItem AcceptanceTestResults

Write-Host "`n##[section]Tidyup and exit..." -ForegroundColor Green
#Invoke-Expression "docker-compose --file build/acceptancetests/docker-compose.yml down"

Pop-Location
Exit-With-Code ([ReturnCode]::SUCCESS)
