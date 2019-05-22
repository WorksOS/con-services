#This script sets up the envirionment variables for RaptorServices WebAPI
Write-host "SetupWebAPI.ps1 Version:1.0" 
Write-host "The user `"$env:username`" run SetupWebAPI.ps1 on machine `"$env:computername`" on $(Get-Date)"

function Retry-Command
{
    param (
    [Parameter(Mandatory=$true)][string]$command, 
    [Parameter(Mandatory=$true)][hashtable]$args, 
    [Parameter(Mandatory=$false)][int]$retries = 50000, 
    [Parameter(Mandatory=$false)][int]$milliSecondsDelay = 10,
	[Parameter(Mandatory=$false)][bool]$randomDelay = $false
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
            else 
            {
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

$OKTORUN = "OK"

$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
cd $dir
Write-host "Running from folder $dir"

$ASNIP = (Get-ChildItem Env:\ASNODEIP).Value
Write-host "ASNODEIP=$ASNIP"

$IONIP = (Get-ChildItem Env:\IONODEIP).Value
Write-host "IONODEIP=$IONIP"

$SHAREUNC = (Get-ChildItem Env:\SHAREUNC).Value
Write-host "SHAREUNC=$SHAREUNC"

$RAPTORUSERNAME = (Get-ChildItem Env:\RAPTORUSERNAME).Value
Write-host "RAPTORUSERNAME=$RAPTORUSERNAME"


if ($RAPTORUSERNAME -eq $null)
{ $RAPTORUSERNAME = "ad-vspengg\svcRaptor" }

if ($ASNIP -eq $null)
  { Write-host "Error! Environment variable ASNODEIP is not set"  -ForegroundColor Red; $OKTORUN = "Bad"}
else 
  { (Get-Content velociraptor.config.xml).replace('[ASNodeIP]', $ASNIP) | Set-Content velociraptor.config.xml}

if ($IONIP -eq $null)
  { Write-host "Error! Environment variable IONODEIP is not set"  -ForegroundColor Red; $OKTORUN = "Bad"}
else 
  { (Get-Content velociraptor.config.xml).replace('[IONodeIP]', $IONIP) | Set-Content velociraptor.config.xml}

# now we need to mount a share for the design files and reports
if ($SHAREUNC -eq $null)
  { Write-host "Error! Environment variable SHAREUNC is not set"  -ForegroundColor Red; $OKTORUN = "Bad"}
else 
  { 
    Write-Host "Mapping Raptor ProductionData folder to Z: drive"
    Retry-Command -Command 'New-SmbMapping' -Args @{ 
      LocalPath = "Z:" 
      RemotePath = $SHAREUNC
      UserName = $RAPTORUSERNAME
      Password = "v3L0c1R^pt0R!"}  -RandomDelay $true -Verbose
    & Z:
    $DL = (get-location).Drive.Name
    Write-host "Current Drive=$DL"
    if ($DL -eq "Z")
      {  & dir; & C:}
    else
      {Write-Host "Warning! Could not map IONode productionData to drive Z:"}
  }


if ($OKTORUN -eq "OK")
  {& .\\VSS.Productivity3D.WebApi.exe}
else
  { Write-host "Error! Not running VSS.Productivity3D.WebApi due to setup error. Check Environment variables ASNODEIP, IONODEIP and SHAREUNC are defined"  -ForegroundColor Red;}

