$serviceLocation = "C:\Services\MasterDataSyncService"
$serviceArchivePath = "D:\Services\MasterDataSyncService"

$temp = $serviceLocation + "\*"
Remove-Item -Recurse -Path $temp

$src = $serviceArchivePath + "\*"

Write-Host ("Copying binaries from $src to $serviceLocation")
Copy-Item -Path $src $serviceLocation -Recurse