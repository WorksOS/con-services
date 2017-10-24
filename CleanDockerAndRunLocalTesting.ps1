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

WriteMsg "Building login credentials"
$Cmd = 'aws'
$Args = 'ecr', 'get-login'
$LoginID = &$Cmd $Args
#$LoginID = $LoginID -replace "-p", "--password-stdin"
$LoginID = $LoginID -replace "-e none", " "

Write-Host $LoginID;

WriteMsg "Logging in to image host"
Invoke-Expression $LoginID

WriteMsg "Building solution"
& .\RunLocalTesting.bat

if (-not $?) {
    WriteMsg "Error: Environment failed to start" "red"
}