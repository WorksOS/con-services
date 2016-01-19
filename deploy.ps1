Set-ExecutionPolicy Unrestricted 2>>c:\error.txt
Import-Module WebAdministration
New-Item "IIS:\Sites\Default Web Site\Landfill" -physicalPath "C:\Landfill\Landfill" -type Application 2>>c:\error.txt
& "C:\Landfill\LandFillDataSync\LandFillServiceDataSynchronizer.exe" install 2>>c:\error.txt
& "C:\Landfill\LandFillDataSync\LandFillServiceDataSynchronizer.exe" start 2>>c:\error.txt