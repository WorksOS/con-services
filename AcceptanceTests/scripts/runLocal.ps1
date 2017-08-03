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

WriteMsg "Removing Docker containers and images"
docker stop $(docker ps -a -q)
docker rm $(docker ps -a -q)
docker rmi $(docker images -q --filter "dangling=true")

WriteMsg "Building login credentials"
$Cmd = 'aws'
$Args = 'ecr', 'get-login'

$LoginID = &$Cmd $Args
$LoginID = $LoginID -replace "-e none", " "

Write-Output $LoginID

WriteMsg "Logging in to image host"
Invoke-Expression $LoginID

WriteMsg "Pulling service images"
docker-compose pull
WriteMsg "Building services"
docker-compose up --build -d

if ($?) {
    WriteMsg "Docker started successfully" "darkcyan" $True
    WriteMsg " (Running in Detached mode)"
}
else {
    WriteMsg "Docker failed to start" "red"
}