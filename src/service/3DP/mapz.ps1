#This script sets up the envirionment variables for RaptorServices WebAPI
Write-host "SetupWebAPI.ps1 Version:1.0" 
Write-host "The user `"$env:username`" run SetupWebAPI.ps1 on machine `"$env:computername`" on $(Get-Date)"

$OKTORUN = "OK"

$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
cd $dir
Write-host "Running from folder $dir"

$ASNIP = "10.97.96.134"
Write-host "ASNODEIP=$ASNIP"

$IONIP = "10.97.96.92"
Write-host "IONODEIP=$IONIP"

$SHAREUNC = "\\dev-iolv01.vssengg.com\ProductionData"


$RAPTORUSERNAME = "ad-vspengg\svcRaptor"

if ($ASNIP -eq $null)
  { Write-host "Error! Environment variable ASNODEIP is not set"  -ForegroundColor Red; $OKTORUN = "Bad"}
else 
  { (Get-Content velociraptor.config.xml).replace('[ASNodeIP]', $ASNIP) | Set-Content velociraptor.config.xml}

if ($IONIP -eq $null)
  { Write-host "Error! Environment variable IONODEIP is not set"  -ForegroundColor Red; $OKTORUN = "Bad"}
else 
  { (Get-Content velociraptor.config.xml).replace('[IONodeIP]', $IONIP) | Set-Content velociraptor.config.xml}


   & sc.exe qc lanmanworkstation
   & sc.exe config lanmanworkstation depend= "MrxSmb20/NSI"
   & sc.exe qc lanmanworkstationf
   & sc.exe start lanmanworkstation
   Write-Host "Mapping Raptor ProductionData folder to W: drive"
   $mappedDrivePassword = ConvertTo-SecureString "v3L0c1R^pt0R!" -AsPlainText -Force
   $mappedDriveUsername = $RAPTORUSERNAME
   $mappedDriveCredentials = New-Object System.Management.Automation.PSCredential ($mappedDriveUsername, $mappedDrivePassword)
   New-PSDrive -Name "W" -PSProvider FileSystem -Root $SHAREUNC -Persist -Credential $mappedDriveCredentials
   & W:
   $DL = (get-location).Drive.Name
   Write-host "Current Drive=$DL"
   if ($DL -eq "W")
    {  & dir; & C:}
   else
    {Write-Host "Warning! Could not map IONode productionData to drive W:"}



if ($OKTORUN -eq "OK")
  {& .\\VSS.Productivity3D.WebApi.exe}
else
  { Write-host "Error! Not running VSS.Productivity3D.WebApi due to setup error. Check Environment variables ASNODEIP, IONODEIP and SHAREUNC are defined"  -ForegroundColor Red;}

