param(
  [String]$Tests = "true"
)

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

if ($Tests -eq "true") {
    WriteMsg "Unit and acceptance tests will be run" "Yellow" $False
    WriteMsg "If you want to stop tests then run powershell script with 'tests=false'" "Yellow" $False
}
else {
    WriteMsg "Unit and acceptance tests will NOT be run. You have excluded then from running" "Yellow" $False
}

WriteMsg "-------------------------------------------------------" "green" $False

.\build471.ps1

WriteMsg "-------------------------------------------------------" "green" $False
WriteMsg "Build of solutions complete " "green" $False
WriteMsg "-------------------------------------------------------" "green" $False

if ($Tests -eq "true") {
    WriteMsg "-------------------------------------------------------" "green" $False
    WriteMsg "Run unit tests" "green" $False
    WriteMsg "-------------------------------------------------------" "green" $False

    & 'C:\Program Files\dotnet\dotnet.exe' vstest test\UnitTests\WebApiTests\bin\Debug\net471\WebApiTests.dll /Platform:x64
}

WriteMsg "-------------------------------------------------------" "green" $False
WriteMsg "Run the web api in windows container " "green" $False
WriteMsg "-------------------------------------------------------" "green" $False

.\AcceptanceTests\scripts\runLocal.ps1

if ($Tests -eq "true") {
    WriteMsg "-------------------------------------------------------" "green" $False
    WriteMsg "Run the acceptance tests " "green" $False
    WriteMsg "-------------------------------------------------------" "green" $False
    .\runacceptancetests.ps1
}

Set-Location $PSScriptRoot