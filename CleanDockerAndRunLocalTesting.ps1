# Setting the COMPOSE_CONVERT_WINDOWS_PATHS environment variable before trying 
# to bring up the containers seems to fix the docker-compose bug reported here: https://github.com/docker/for-win/issues/1829
$Env:COMPOSE_CONVERT_WINDOWS_PATHS=1

function WriteMsg
{
    Param([string]$message, [string]$color = "darkgray", [bool]$noNewLine = $False)

    if ($noNewLine) {
        Write-Host $message -ForegroundColor $color -NoNewline
    }
    else
    {
        Write-Host $message -ForegroundColor $color
    }

    [Console]::ResetColor() 
}

WriteMsg "Updating environment IP address"
& .\UpdateEnvFileIpAddress.ps1

if ($args -notcontains "--no-rm") {
    WriteMsg "Stopping Docker containers"
    docker stop $(docker ps -a -q)
    WriteMsg "Removing Docker containers"
    docker rm $(docker ps -a -q)
}

if ($args -notcontains "--no-rmi") {
    Write-Host "Removing Docker images" -ForegroundColor "darkgray"
    docker stop $(docker ps -a -q)
    docker rmi $(docker images -q --filter "dangling=true")
    docker rmi 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-masterdataconsumer-db
    docker rmi 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-masterdataconsumer
    docker rmi 276986344560.dkr.ecr.us-west-2.amazonaws.com/vssproductivity3dfilter_webapi
    docker rmi 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-mockproject-webapi:latest-linux
}

WriteMsg "Logging in to image host"
Invoke-Expression -Command (aws ecr get-login --no-include-email --region us-west-2)

WriteMsg "Building solution"
& .\RunLocalTesting.bat

if (-not $?) {
    WriteMsg "Error: Environment failed to start" "red"
}