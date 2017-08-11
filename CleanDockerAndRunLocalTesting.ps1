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
}

WriteMsg "Updating environment IP address"
& .\UpdateEnvFileIpAddress.ps1

WriteMsg "Stopping Docker containers"
docker stop $(docker ps -a -q)
WriteMsg "Removing Docker containers"
docker rm $(docker ps -a -q)

WriteMsg "Building login credentials"
$Cmd = 'aws'
$Args = 'ecr', 'get-login'
$LoginID = &$Cmd $Args
$LoginID = $LoginID -replace "-e none", " "

WriteMsg "Logging in to image host"
Invoke-Expression $LoginID

WriteMsg "Building solution"
& .\RunLocalTesting.bat

if (-not $?) {
    WriteMsg "Environment failed to start" "red"
}