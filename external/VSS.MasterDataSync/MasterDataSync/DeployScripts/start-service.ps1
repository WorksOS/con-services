$ScriptDirectory = Split-Path $MyInvocation.MyCommand.Path
. (Join-Path $ScriptDirectory CommonDeploymentFunctions.ps1)


$Services = @("_NHMasterDataSyncService")

InstallService -ServiceName "_NHMasterDataSyncService" -ServicePath "C:\Services\MasterDataSyncService\VSS.Nighthawk.MasterDataSync.exe"

StartServices $Services

