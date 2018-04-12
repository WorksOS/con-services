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
[Environment]::SetEnvironmentVariable("MYSQL_DATABASE_NAME", "VSS-MasterData-Project", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_PORT", "3306", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_USERNAME", "root", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_ROOT_PASSWORD", "abc123", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_VSPDB", "localhost", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_ReadVSPDB", "localhost", "Machine")
[Environment]::SetEnvironmentVariable("KAFKA_URI", "localhost", "Machine")
[Environment]::SetEnvironmentVariable("KAFKA_PORT", "9092", "Machine")
[Environment]::SetEnvironmentVariable("KAFKA_ADVERTISED_HOST_NAME", "LOCALIPADDRESS", "Machine")
[Environment]::SetEnvironmentVariable("KAFKA_ADVERTISED_PORT", "9092", "Machine")
[Environment]::SetEnvironmentVariable("KAFKA_GROUP_NAME", "Project-Consumer", "Machine")
[Environment]::SetEnvironmentVariable("KAFKA_TOPIC_NAME_SUFFIX", "-Project", "Machine")
[Environment]::SetEnvironmentVariable("KAFKA_AUTO_CREATE_TOPICS_ENABLE", "true", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_URI", "http://webapi:80/", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_DEBUG_URI", "http://localhost:5000/", "Machine")
[Environment]::SetEnvironmentVariable("ASSOCIATESUBSPROJECT_API_URL", "http://localhost:5001/dummy/", "Machine")
[Environment]::SetEnvironmentVariable("CREATEGEOFENCE_API_URL", "http://localhost:5001/dummy/", "Machine")
[Environment]::SetEnvironmentVariable("COORDSYSVALIDATE_API_URL", "http://localhost:5001/dummy/", "Machine")
[Environment]::SetEnvironmentVariable("COORDSYSPOST_API_URL", "http://localhost:5001/api/v1/mock/coordsystem", "Machine")
[Environment]::SetEnvironmentVariable("RAPTOR_NOTIFICATION_API_URL", "http://localhost:5001/api/v2/notification", "Machine")
[Environment]::SetEnvironmentVariable("CUSTOMERSERVICE_API_URL", "http://localhost:5001/api/v1/mock/getcustomersforme", "Machine")
[Environment]::SetEnvironmentVariable("RAPTOR_PROJECT_SETTINGS_API_URL", "http://mockprojectwebapi:5001/api/v2", "Machine")
[Environment]::SetEnvironmentVariable("PROJECTSERVICE_KAFKA_TOPIC_NAME","VSS.Interfaces.Events.MasterData.IProjectEvent", "Machine")
[Environment]::SetEnvironmentVariable("TCCBASEURL", "mock", "Machine")
[Environment]::SetEnvironmentVariable("TCCFILESPACEID", "mock", "Machine")
<# #>
<#  Dev environment 
[Environment]::SetEnvironmentVariable("MYSQL_DATABASE_NAME", "VSS-MasterData-Project", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_PORT", "3306", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_USERNAME", "root", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_ROOT_PASSWORD", "d3vRDS1234_", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_VSPDB", "rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_ReadVSPDB", "rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com", "Machine")
[Environment]::SetEnvironmentVariable("KAFKA_URI", "10.97.99.172", "Machine")
[Environment]::SetEnvironmentVariable("KAFKA_PORT", "9092", "Machine")
[Environment]::SetEnvironmentVariable("KAFKA_GROUP_NAME", "Project-Consumer", "Machine")
[Environment]::SetEnvironmentVariable("KAFKA_TOPIC_NAME_SUFFIX", "-Dev", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_URI", "http://10.97.96.103:3001/", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_DEBUG_URI", "http://10.97.96.103:3001/", "Machine")
[Environment]::SetEnvironmentVariable("ASSOCIATESUBSPROJECT_API_URL", "http://mockprojectwebapi:5001/", "Machine")
[Environment]::SetEnvironmentVariable("CREATEGEOFENCE_API_URL", "http://mockprojectwebapi:5001/", "Machine")
[Environment]::SetEnvironmentVariable("COORDSYSVALIDATE_API_URL", "http://mockprojectwebapi:5001/", "Machine")
[Environment]::SetEnvironmentVariable("COORDSYSPOST_API_URL", "http://mockprojectwebapi:5001/", "Machine")
[Environment]::SetEnvironmentVariable("RAPTOR_NOTIFICATION_API_URL", "http://mockprojectwebapi:5001/api/v2/notification", "Machine")
[Environment]::SetEnvironmentVariable("CUSTOMERSERVICE_API_URL", "http://mockprojectwebapi:5001/api/v1/mock/getcustomersforme", "Machine")
[Environment]::SetEnvironmentVariable("RAPTOR_PROJECT_SETTINGS_API_URL", "http://mockprojectwebapi:5001/api/v2", "Machine")
[Environment]::SetEnvironmentVariable("PROJECTSERVICE_KAFKA_TOPIC_NAME","VSS.Interfaces.Events.MasterData.IProjectEvent", "Machine")
[Environment]::SetEnvironmentVariable("TCCBASEURL", "mock", "Machine")
[Environment]::SetEnvironmentVariable("TCCFILESPACEID", "mock", "Machine")
#>
