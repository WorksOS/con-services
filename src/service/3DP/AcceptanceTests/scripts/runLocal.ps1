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
    [Environment]::SetEnvironmentVariable("COMPACTION_SVC_BASE_URI", ":80", "Machine")
    [Environment]::SetEnvironmentVariable("NOTIFICATION_SVC_BASE_URI", ":80", "Machine")
    [Environment]::SetEnvironmentVariable("REPORT_SVC_BASE_URI", ":80", "Machine")
    [Environment]::SetEnvironmentVariable("TAG_SVC_BASE_URI", ":80", "Machine")
    [Environment]::SetEnvironmentVariable("COORD_SVC_BASE_URI", ":80", "Machine")
    [Environment]::SetEnvironmentVariable("PROD_SVC_BASE_URI", ":80", "Machine")
    [Environment]::SetEnvironmentVariable("FILE_ACCESS_SVC_BASE_URI", ":80", "Machine")
    [Environment]::SetEnvironmentVariable("RAPTOR_WEBSERVICES_HOST", "$containerIpAddress", "Machine")
}

WriteMsg "Logging in to image host"
Invoke-Expression -Command (aws ecr get-login --no-include-email --region us-west-2)

WriteMsg "Pulling service images"
docker-compose pull --no-parallel
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