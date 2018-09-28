start /D "src/tools/VSS.TRex.Webtools/" "WebTools" cmd.exe /k "dotnet run"
timeout 30
http://localhost:64416/
