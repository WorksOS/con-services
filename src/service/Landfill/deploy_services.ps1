Set-ExecutionPolicy Unrestricted 2>>c:\error.txt
& "C:\Landfill\LandFillDataSync\LandFillServiceDataSynchronizer.exe" install 2>>c:\error.txt
& "C:\Landfill\LandFillDataSync\LandFillServiceDataSynchronizer.exe" start 2>>c:\error.txt
& "C:\Landfill\LandFillMDM\VSS.VisionLink.Landfill.MDMService.exe" install 2>>c:\error.txt
& "C:\Landfill\LandFillMDM\VSS.VisionLink.Landfill.MDMService.exe" start 2>>c:\error.txt

& "C:\Landfill\LandFillCustomer\VSS.MasterData.Customer.Processor.exe" install 2>>c:\error.txt
& "C:\Landfill\LandFillCustomer\VSS.MasterData.Customer.Processor.exe" start 2>>c:\error.txt
& "C:\Landfill\LandFillSubscription\VSS.Subscription.Processor.exe.exe" install 2>>c:\error.txt
& "C:\Landfill\LandFillSubscription\VSS.Subscription.Processor.exe.exe" start 2>>c:\error.txt
