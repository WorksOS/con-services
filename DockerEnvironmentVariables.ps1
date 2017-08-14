<#
   To setup environment variables to allow you to debug using your local mysql container:
   
   1) File\Open Windows PowerShell\Open Windows PowerShell as administrator
   2) change directory to the folder containing this file
   3) type (or copy) this command and run in PS: Set-ExecutionPolicy RemoteSigned
   4) type (or copy) this command and press enter in PS: .\DockerEnvironmentVariables.ps1
   Note: if you change environment settings whilst you have the Visual Studio open.
   	You need to resart Visual Studio

#>
<# #>
[Environment]::SetEnvironmentVariable("MYSQL_DATABASE_NAME", "VSS-Productivity3D-Scheduler", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_PORT", "3306", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_USERNAME", "root", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_ROOT_PASSWORD", "abc123", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME", "localhost", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_DATABASE_NAME_FILTER", "VSS-Productivity3D-Filter", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_PORT_FILTER", "3306", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_USERNAME_FILTER", "root", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_ROOT_PASSWORD_FILTER", "abc123", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_FILTER", "localhost", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_URI", "http://webapi:80/", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_DEBUG_URI", "http://localhost:3001/", "Machine")
#>
<#  Dev environment
[Environment]::SetEnvironmentVariable("MYSQL_DATABASE_NAME", "VSS-Productivity3D-Scheduler", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_PORT", "3306", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_USERNAME", "root", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_ROOT_PASSWORD", "d3vRDS1234_", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_VSPDB", "rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_URI", "http://10.97.96.103:3001/", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_DEBUG_URI", "http://10.97.96.103:3001/", "Machine")
#>
