param(
  [String]$Tests = "true"
)

if ($Tests -eq "true") {
    Write-Host "Unit and acceptance tests will be run." -ForegroundColor DarkGray
    Write-Host "If you want to stop tests then run powershell script with 'tests=false'" -ForegroundColor DarkGray
}
else {
    Write-Host "Unit and acceptance tests will NOT be run. You have excluded then from running." -ForegroundColor Yellow
}

.\build471.ps1

if ($Tests -eq "true") {
    & 'C:\Program Files\dotnet\dotnet.exe' vstest test\UnitTests\WebApiTests\bin\Debug\net471\VSS.Productivity3D.WebApiTests.dll /Platform:x64
}

.\AcceptanceTests\scripts\runLocal.ps1

if ($Tests -eq "true") {
    .\runacceptancetests.ps1
}

Set-Location $PSScriptRoot
