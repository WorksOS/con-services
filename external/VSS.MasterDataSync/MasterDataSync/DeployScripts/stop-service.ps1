$ScriptDirectory = Split-Path $MyInvocation.MyCommand.Path
. (Join-Path $ScriptDirectory CommonDeploymentFunctions.ps1)


$Services = @("_NHMasterDataSyncService")

StopServices $Services