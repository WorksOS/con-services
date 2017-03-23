#This script sets up the envirionment variables for RaptorServices WebAPI
Write-host "The user `"$env:username`" run SetupWebAPI.ps1 on machine `"$env:computername`" on $(Get-Date)"  -ForegroundColor Yellow

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
   & sc.exe qc lanmanworkstation
   & sc.exe config lanmanworkstation depend= "MrxSmb20/NSI"
   & sc.exe qc lanmanworkstation
   & sc.exe start lanmanworkstation

   # format $SHAREUNC = "\\dev-iolv01.vssengg.com\productiondata"
   #$pass="v3L0c1R^pt0R!"|ConvertTo-SecureString -AsPlainText -Force
   #$Cred = New-Object System.Management.Automation.PsCredential('svcRaptor',$pass)
   Write-host "Mapping drive Z to $SHAREUNC"
   #New-PSDrive -Persist -Name "Z" -PSProvider "FileSystem" -Root $SHAREUNC -Credential $cred
   myCmd = "net use z: $HAREUNC v3L0c1R^pt0R! /user:svcRaptor /persistent:yes"
   Invoke-Command $myCmd
  }


if ($OKTORUN -eq "OK")
  {& .\\WebAPI.exe}
else
  { Write-host "Error! Not running WebAPI due to setup error. Check Environment variables ASNODEIP, IONODEIP and SHAREUNC are defined"  -ForegroundColor Red}

