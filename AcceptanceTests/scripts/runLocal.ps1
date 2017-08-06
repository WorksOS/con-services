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

function GetContainerIpAddress
{
    Param([string]$containerName)

    $containerId = Invoke-Expression "docker ps -aqf name=$containerName"
    $containerIpAddress = Invoke-Expression "docker inspect --format '{{ .NetworkSettings.Networks.nat.IPAddress }}' $containerId"

    WriteMsg "`n  Container Name: " "gray" $True
    WriteMsg $containerName
    WriteMsg "  Container ID: " "gray" $True
    WriteMsg $containerId
    WriteMsg "  IP Address: " "gray" $True
    WriteMsg $containerIpAddress`n
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
    GetContainerIpAddress vssproductivity3dservice_webapi
}
else {
    WriteMsg "Docker failed to start" "red"
}