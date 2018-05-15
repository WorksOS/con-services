start C:\Dev\VSS.TRex\RaptorPSNodeServer\bin\debug\RaptorPSNodeServer.exe SpatialDivision=0
timeout 7
rem start C:\Dev\VSS.TRex\RaptorPSNodeServer\bin\debug\RaptorPSNodeServer.exe SpatialDivision=1
rem timeout 7
rem start C:\Dev\VSS.TRex\RaptorPSNodeServer\bin\debug\RaptorPSNodeServer.exe SpatialDivision=2
rem timeout 7
rem start C:\Dev\VSS.TRex\RaptorPSNodeServer\bin\debug\RaptorPSNodeServer.exe SpatialDivision=3
rem timeout 7
start C:\Dev\VSS.TRex\RaptorMutableDataServer\bin\debug\RaptorMutableDataServer.exe SpatialDivision=0
rem start C:\Dev\VSS.TRex\RaptorTAGFileServer\bin\debug\RaptorTAGFileServer.exe SpatialDivision=0
timeout 7
start C:\Dev\VSS.TRex\RaptorServerApplication\bin\debug\RaptorTileServer.exe
timeout 7
start C:\Dev\VSS.TRex\RaptorDesignElevationsServer\bin\debug\RaptorDesignElevationsServer.exe
timeout 7
start C:\Dev\VSS.TRex\WindowsFormsApplication1\bin\debug\RaptorIgniteTest.exe
timeout 7
start C:\Dev\VSS.TRex\RaptorGridActivator\bin\debug\RaptorGridActivator.exe
dotnet C:\Dev\VSS.TRex\VSS.TRex.Service.Deployer\bin\Debug\netcoreapp2.0\VSS.TRex.Service.Deployer.dll

 