Import-Module WebAdministration
New-Item "IIS:\Sites\Default Web Site\Landfill" -physicalPath "C:\Landfill\Landfill" -type Application
& "C:\Landfill\LandFillDataSync\LandFillServiceDataSynchronizer.exe" install
& "C:\Landfill\LandFillDataSync\LandFillServiceDataSynchronizer.exe" start