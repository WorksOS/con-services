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

WriteMsg "-------------------------------------------------------" "green" $False
WriteMsg "Remove docker images and build 3D pm web api " "green" $False
WriteMsg "You need to be run powershell in administrator mode" "green" $False
WriteMsg "-------------------------------------------------------" "green" $False

.\build471.ps1

WriteMsg "-------------------------------------------------------" "green" $False
WriteMsg "Build of solutions complete " "green" $False
WriteMsg "Run unit tests" "green" $False
WriteMsg "-------------------------------------------------------" "green" $False

& 'C:\Program Files\dotnet\dotnet.exe' vstest test\UnitTests\WebApiTests\bin\Debug\net471\VSS.Productivity3D.WebApiTests.dll /Platform:x64

WriteMsg "-------------------------------------------------------" "green" $False
WriteMsg "Run the web api in windows container " "green" $False
WriteMsg "-------------------------------------------------------" "green" $False
.\AcceptanceTests\scripts\runLocal.ps1


WriteMsg "-------------------------------------------------------" "green" $False
WriteMsg "Run the acceptance tests " "green" $False
WriteMsg "-------------------------------------------------------" "green" $False

.\runacceptancetests.ps1