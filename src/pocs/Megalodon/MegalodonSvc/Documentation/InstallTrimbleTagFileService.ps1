# Set-ExecutionPolicy Unrestricted 

function ReinstallService ($serviceName, $binaryPath, $description)
{
    Write-Host "Trying to create service: $serviceName"

    Write-Host "path: $binaryPath"
    #Check Parameters
    if ((Test-Path $binaryPath)-eq $false)
    {
        Write-Host "BinaryPath to service not found: $binaryPath"
        Write-Host "Service was NOT installed."
        return
    }

    # Verify if the service already exists, and if yes remove it first
    if (Get-Service $serviceName -ErrorAction SilentlyContinue)
    {
        # using WMI to remove Windows service because PowerShell does not have CmdLet for this
        $serviceToRemove = Get-WmiObject -Class Win32_Service -Filter "name='$serviceName'"
        $serviceToRemove.delete()
        Write-Host "Service removed: $serviceName"
    }

    # Creating Windows Service using all provided parameters
    Write-Host "Installing service: $serviceName"
    New-Service -Name $serviceName -BinaryPathName $binaryPath -DisplayName $serviceName -StartupType "Automatic" -Description $description

    Write-Host "Installation completed: $serviceName"

    # Trying to start new service
    Write-Host "Trying to start new service: $serviceName"
    $serviceToStart = Get-WmiObject -Class Win32_Service -Filter "name='$serviceName'"
    $serviceToStart.startservice()
 #  Start-Service $serviceName -PassThru
    Write-Host "Service started: $serviceName"

    #SmokeTest
    Write-Host "Waiting 5 seconds to give time service to start..."
    Start-Sleep -s 5
    $SmokeTestService = Get-Service -Name $serviceName
    if ($SmokeTestService.Status -ne "Running")
    {
        Write-Host "Smoke test: FAILED. (SERVICE FAILED TO START)"
        Throw "Smoke test: FAILED. (SERVICE FAILED TO START)"
    }
    else
    {
        Write-Host "Smoke test: OK."
    }

}


Write-Host "*** Trimble Tagfile Service Installer Begin *** " -foregroundcolor green

$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath

# Dir setup
$sourceFile = Join-Path $dir "TagfileSvc.zip"

$installDIR = "C:\Trimble"

$serviceName = "TrimbleTagfileService"

$description = "Trimble Tagfile Service"

$binaryPath = "C:\Trimble\TagfileSvc\TagfileSvc.exe"

$UpdateFolder = "C:\Trimble\TagfileSvc"

if (Test-Path $UpdateFolder -PathType Container) # if folder exists
{
  Remove-Item $UpdateFolder -Force -Recurse
}

# Extracts files from package.zip
Write-Host "Extracting TagfileSvc.zip.." -foregroundcolor green
[System.Reflection.Assembly]::LoadWithPartialName('System.IO.Compression.FileSystem');
[System.IO.Compression.ZipFile]::ExtractToDirectory($SourceFile, $InstallDIR);

Write-Host "Installing Trimble Service.." -foregroundcolor green

ReinstallService $serviceName $binaryPath $description

Write-Host "*** Install End ***" -foregroundcolor green
