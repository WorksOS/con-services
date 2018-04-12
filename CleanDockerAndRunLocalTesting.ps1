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

WriteMsg "Stopping Docker containers"
docker stop $(docker ps -a -q)
WriteMsg "Removing Docker containers"
docker rm $(docker ps -a -q)

WriteMsg "Logging in to image host"
Invoke-Expression -Command (aws ecr get-login --no-include-email --region us-west-2)

$Env:COMPOSE_CONVERT_WINDOWS_PATHS=1

WriteMsg "Building solution"
& .\RunLocalTesting.bat

if (-not $?) {
    WriteMsg "Error: Environment failed to start" "red"
}