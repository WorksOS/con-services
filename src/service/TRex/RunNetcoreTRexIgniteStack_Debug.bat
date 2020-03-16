REM Build the TRex.netstandard.sln in VS2017 before running this batch file
start /D "src/netstandard/services/VSS.TRex.Server.PSNode/bin/Debug/netcoreapp3.1/" "PSNode" cmd.exe /k "dotnet VSS.TRex.Server.PSNode.dll"
timeout 4
start /D "src/netstandard/services/VSS.TRex.Server.MutableData/bin/Debug/netcoreapp3.1/" "MutableData" cmd.exe /k "dotnet VSS.TRex.Server.MutableData.dll"
timeout 4
start /D "src/netstandard/services/VSS.TRex.Server.Application/bin/Debug/netcoreapp3.1/" "Application" cmd.exe /k "dotnet VSS.TRex.Server.Application.dll"
timeout 4
start /D "src/netstandard/services/VSS.TRex.Server.DesignElevation/bin/Debug/netcoreapp3.1/" "DesignElevation" cmd.exe /k "dotnet VSS.TRex.Server.DesignElevation.dll"
timeout 4
start /D "src/netstandard/services/VSS.TRex.Server.TINSurfaceExport/bin/Debug/netcoreapp3.1/" "TINSurfaceExport" cmd.exe /k "dotnet VSS.TRex.Server.TINSurfaceExport.dll"
timeout 4
start /D "src/netstandard/services/VSS.TRex.Server.TileRendering/bin/Debug/netcoreapp3.1/" "TileRendering" cmd.exe /k "dotnet VSS.TRex.Server.TileRendering.dll"
timeout 4
start /D "src/netstandard/services/VSS.TRex.Server.QuantizedMesh/bin/Debug/netcoreapp3.1/" "QuantizedMesh" cmd.exe /k "dotnet VSS.TRex.Server.QuantizedMesh.dll"
timeout 4
start /D "src/netstandard/services/VSS.TRex.Server.Reports/bin/Debug/netcoreapp3.1/" "Reports" cmd.exe /k "dotnet VSS.TRex.Server.Reports.dll"

