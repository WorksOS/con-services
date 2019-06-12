#This script sets up the envirionment variables for RaptorServices WebAPI
Write-host "SetupWebAPI.ps1 Version:1.0" 
Write-host "The user `"$env:username`" run SetupWebAPI.ps1 on machine `"$env:computername`" on $(Get-Date)"

function ExecuteCommandWithRetry {
    param (
        [Parameter(Mandatory = $true)][string]$command, 
        [Parameter(Mandatory = $true)][hashtable]$args, 
        [Parameter(Mandatory = $false)][int]$retries = 50000, 
        [Parameter(Mandatory = $false)][int]$milliSecondsDelay = 10,
        [Parameter(Mandatory = $false)][bool]$randomDelay = $false
    )
    
    # Setting ErrorAction to Stop is important. This ensures any errors that occur in the command are 
    # treated as terminating errors, and will be caught by the catch block.
    $args.ErrorAction = "Stop"
    
    $retrycount = 0
    $completed = $false

    while (-not $completed) {
        try {
            & $command @args
            Write-Verbose ("Command [{0}] succeeded." -f $command)
            $completed = $true
        } 
        catch {
            if ($retrycount -ge $retries) {
                Write-Verbose ("Command [{0}] failed the maximum number of {1} times." -f $command, $retrycount)
                throw
            } 
            else {
                if ($randomDelay -eq $true) {
                    $milliSecondsDelay = Get-Random -Minimum 1 -Maximum 500
                }
                Write-Verbose ("Command [{0}] failed. Retrying in {1} milliseconds. This was attempt {2}" -f $command, $milliSecondsDelay, $retrycount)
                Start-Sleep -milliseconds $milliSecondsDelay
                $retrycount++
            }
        }
    }
}

$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath

Set-Location $dir
Write-host "Running from folder $dir"

Write-Host "Adding AWS Route"
invoke-expression -Command .\SetupAwsRoute.ps1


(Get-ChildItem Env:\RAPTORUSERNAME -ErrorAction SilentlyContinue).Value | ForEach-Object { IF ($null -eq $_) { $RAPTORUSERNAME = "ad-vspengg\svcRaptor" } ELSE { $RAPTORUSERNAME = $_ } };
Write-host "RAPTORUSERNAME=$RAPTORUSERNAME"

$SHAREUNC = (Get-ChildItem Env:\SHAREUNC).Value
Write-host "SHAREUNC=$SHAREUNC"

# now we need to mount a share for the design files and reports
if ($null -eq $SHAREUNC) {
    Write-host "Error! Environment variable SHAREUNC is not set" -ForegroundColor Red
    Exit 1
}

Write-Host "Mapping Raptor ProductionData folder to Z: drive"
ExecuteCommandWithRetry -Command 'New-SmbMapping' -Args @{ 
    LocalPath  = "Z:" 
    RemotePath = $SHAREUNC
    UserName   = $RAPTORUSERNAME
    Password   = "v3L0c1R^pt0R!"
}  -RandomDelay $true -Verbose
& Z:
$DL = (get-location).Drive.Name
Write-host "Current Drive=$DL"
if ($DL -eq "Z") {
    & Get-ChildItem; & C:
}
else {
    Write-Host "Warning! Could not map IONode productionData to drive Z:"
}

& .\\VSS.Productivity3D.WebApi.exe
