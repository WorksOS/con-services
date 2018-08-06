REM Build the TRex.netstandard.sln in VS2017 before running this batch file
start "PSNode" cmd.exe /k "dotnet src/netstandard/services/VSS.TRex.Server.PSNode/bin/Debug/netcoreapp2.0/VSS.TRex.Server.PSNode.dll"
timeout 7
start "MutableData" cmd.exe /k "dotnet src/netstandard/services/VSS.TRex.Server.MutableData/bin/Debug/netcoreapp2.0/VSS.TRex.Server.MutableData.dll"
timeout 7
start "Application" cmd.exe /k "dotnet src/netstandard/services/VSS.TRex.Server.Application/bin/Debug/netcoreapp2.0/VSS.TRex.Server.Application.dll"
timeout 7
start "DesignElevation" cmd.exe /k "dotnet src/netstandard/services/VSS.TRex.Server.DesignElevation/bin/Debug/netcoreapp2.0/VSS.TRex.Server.DesignElevation.dll"
timeout 7
start "TINSurfaceExport" cmd.exe /k "dotnet src/netstandard/services/VSS.TRex.Server.TINSurfaceExport/bin/Debug/netcoreapp2.0/VSS.TRex.Server.TINSurfaceExport.dll"
timeout 7
