set content=
rem Get Raptor webservices container ID
for /F "delims=" %%i in (container.txt) do set container_id=%%i
rem Get Raptor webservices container IP from the ID
for /f %%i in ('docker inspect --format "{{ .NetworkSettings.Networks.nat.IPAddress }}" %container_id%') do set RAPTOR_WEBSERVICES_IP=%%i
PowerShell.exe -ExecutionPolicy Bypass -Command .\waitForContainer.ps1 -IP %RAPTOR_WEBSERVICES_IP%
PowerShell.exe -ExecutionPolicy Bypass -Command .\AcceptanceTests\scripts\setEnvironmentVariablesLocal.ps1 -RAPTOR_WEBSERVICES_HOST %RAPTOR_WEBSERVICES_IP%