#This script sets up the envirionment variables for RaptorServices WebAPI
Write-host "The user `"$env:username`" logged in to laptop  `"$env:computername`" on $(Get-Date)"  -ForegroundColor Yellow

$OKTORUN = "OK"

$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
cd $dir
Write-host "Running from folder $dir"

$ASNIP = (Get-ChildItem Env:\ASNODEIP).Value
Write-host "ASNODEIP=$ASNIP"

$IONIP = (Get-ChildItem Env:\IONODEIP).Value
Write-host "IONODEIP=$IONIP"

if ($ASNIP -eq $null)
  { Write-host "Error! Environment variable ASNODEIP is not set"  -ForegroundColor Red; $OKTORUN = "Bad"}
else 
  { (Get-Content velociraptor.config.xml).replace('[ASNodeIP]', $ASNIP) | Set-Content velociraptor.config.xml}

if ($IONIP -eq $null)
  { Write-host "Error! Environment variable IONODEIP is not set"  -ForegroundColor Red; $OKTORUN = "Bad"}
else 
  { (Get-Content velociraptor.config.xml).replace('[IONodeIP]', $IONIP) | Set-Content velociraptor.config.xml}

if ($OKTORUN -eq "OK")
 {Write-Host "SetupWebAPI finsihed OK"}
 # {& WebAPI.exe}
else
  { Write-host "Error! Not running WebAPI due to setup error. Check Environment variables ASNODEIP and IONODEIP are defined"  -ForegroundColor Red}
