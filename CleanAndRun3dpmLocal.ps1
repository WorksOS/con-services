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

WriteMsg "Remove docker images and build 3D pm web api " "green" $True
WriteMsg "You need to be run powershell in administrator mode" "green" $True

.\build471.ps1

.\AcceptanceTests\scripts\runLocal.ps1

