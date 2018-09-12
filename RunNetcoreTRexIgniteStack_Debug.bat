REM Build the TRex.netstandard.sln in VS2017 before running this batch file
start /D "src/netstandard/services/VSS.TRex.Server.PSNode/bin/Debug/netcoreapp2.1/" "PSNode" cmd.exe /k "dotnet VSS.TRex.Server.PSNode.dll"
timeout 6
start /D "src/netstandard/services/VSS.TRex.Server.MutableData/bin/Debug/netcoreapp2.1/" "MutableData" cmd.exe /k "dotnet VSS.TRex.Server.MutableData.dll"
timeout 6
start /D "src/netstandard/services/VSS.TRex.Server.Application/bin/Debug/netcoreapp2.1/" "Application" cmd.exe /k "dotnet VSS.TRex.Server.Application.dll"
timeout 6
start /D "src/netstandard/services/VSS.TRex.Server.DesignElevation/bin/Debug/netcoreapp2.1/" "DesignElevation" cmd.exe /k "dotnet VSS.TRex.Server.DesignElevation.dll"
timeout 6
start /D "src/netstandard/services/VSS.TRex.Server.TINSurfaceExport/bin/Debug/netcoreapp2.1/" "TINSurfaceExport" cmd.exe /k "dotnet VSS.TRex.Server.TINSurfaceExport.dll"
timeout 6
start /D "src/netstandard/services/VSS.TRex.Server.TileRendering/bin/Debug/netcoreapp2.1/" "TileRendering" cmd.exe /k "dotnet VSS.TRex.Server.TileRendering.dll"

