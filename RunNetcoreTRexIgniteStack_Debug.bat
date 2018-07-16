REM Build the TRex.netstandard.sln in VS2017 before running this batch file

start cmd.exe /k "dotnet src/netstandard/services/VSS.TRex.Server.DesignElevation/bin/Debug/netcoreapp2.0/VSS.TRex.Server.DesignElevation.dll"
timeout 7
start cmd.exe /k "dotnet src/netstandard/services/VSS.TRex.Server.MutableData/bin/Debug/netcoreapp2.0/VSS.TRex.Server.MutableData.dll"
timeout 7
start cmd.exe /k "dotnet src/netstandard/services/VSS.TRex.Server.PSNode/bin/Debug/netcoreapp2.0/VSS.TRex.Server.PSNode.dll"
timeout 7
start cmd.exe /k "dotnet src/netstandard/services/VSS.TRex.Server.Tile/bin/Debug/netcoreapp2.0/VSS.TRex.Server.Application.dll"