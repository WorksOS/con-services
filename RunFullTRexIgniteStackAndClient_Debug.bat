start C:\Dev\VSS.TRex\src\framework\TRexImmutableDataServer\bin\debug\TRexImmutableDataServer.exe SpatialDivision=0
timeout 7
start C:\Dev\VSS.TRex\src\framework\TRexMutableDataServer\bin\debug\TRexMutableDataServer.exe SpatialDivision=0
timeout 7
start C:\Dev\VSS.TRex\src\framework\TRexServerApplication\bin\debug\TRexTileServer.exe
timeout 7
start C:\Dev\VSS.TRex\src\framework\TRexDesignElevationsServer\bin\debug\TRexDesignElevationsServer.exe
timeout 7
start C:\Dev\VSS.TRex\src\framework\TRexIgniteTest\bin\debug\TRexIgniteTest.exe
timeout 7
start C:\Dev\VSS.TRex\src\framework\TRexGridActivator\bin\debug\TRexGridActivator.exe
rem dotnet C:\Dev\VSS.TRex\src\tools\VSS.TRex.Service.Deployer\bin\Debug\netcoreapp2.0\VSS.TRex.Service.Deployer.dll

 