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
[Environment]::SetEnvironmentVariable("WEBAPI_URI", "http://localhost:5000/", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_DEBUG_URI", "http://localhost:5000/", "Machine")
[Environment]::SetEnvironmentVariable("TCCBASEURL", "mock", "Machine")
[Environment]::SetEnvironmentVariable("TCCFILESPACEID", "mock", "Machine")
<# #>
<#  Dev environment 
[Environment]::SetEnvironmentVariable("WEBAPI_URI", "http://10.97.96.103:3001/", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_DEBUG_URI", "http://10.97.96.103:3001/", "Machine")
[Environment]::SetEnvironmentVariable("TCCBASEURL", "mock", "Machine")
[Environment]::SetEnvironmentVariable("TCCFILESPACEID", "mock", "Machine")
#>
