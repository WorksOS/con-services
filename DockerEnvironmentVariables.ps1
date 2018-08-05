<#
   To setup environment variables to allow you to debug using your local mysql container:
   
    USAGE: $ DockerEnvironmentVariables -localhost // Runs using local MYSQL instance
    Default: Uses Dev server.

   1) File\Open Windows PowerShell\Open Windows PowerShell as administrator
   2) change directory to the folder containing this file
   3) type (or copy) this command and run in PS: Set-ExecutionPolicy RemoteSigned
   4) type (or copy) this command and press enter in PS: .\DockerEnvironmentVariables.ps1
   Note: If you change environment settings whilst you have the Visual Studio open you'll need to resart Visual Studio.
#>

function SetEnvironmentVariableLocalhost {
    Write-Host "Setting environment variables for LOCALHOST server..."
    [Environment]::SetEnvironmentVariable("MYSQL_ROOT_PASSWORD", "abc123", "Machine")
    [Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_VSPDB", "localhost", "Machine")
    [Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_ReadVSPDB", "localhost", "Machine")
    [Environment]::SetEnvironmentVariable("KAFKA_URI", "localhost", "Machine")
    [Environment]::SetEnvironmentVariable("KAFKA_ADVERTISED_PORT", "9092", "Machine")
    [Environment]::SetEnvironmentVariable("KAFKA_ADVERTISED_HOST_NAME", "", "Machine")
    [Environment]::SetEnvironmentVariable("KAFKA_GROUP_NAME", "Filter-Producer", "Machine")
    [Environment]::SetEnvironmentVariable("KAFKA_TOPIC_NAME_SUFFIX", "-Filter", "Machine")
    [Environment]::SetEnvironmentVariable("WEBAPI_URI", "http://webapi:80/", "Machine")
    [Environment]::SetEnvironmentVariable("WEBAPI_DEBUG_URI", "http://localhost:5000/", "Machine")
    [Environment]::SetEnvironmentVariable("RAPTOR_NOTIFICATION_API_URL", "http://localhost:5001/api/v2/notification", "Machine")
    [Environment]::SetEnvironmentVariable("CUSTOMERSERVICE_API_URL", "http://localhost:5001/api/v1/mock/getcustomersforme", "Machine")
    [Environment]::SetEnvironmentVariable("PROJECT_API_URL","http://localhost:5001/api/v4/mockproject", "Machine")
    [Environment]::SetEnvironmentVariable("IMPORTED_FILE_API_URL","http://localhost:5001/api/v4/mock/importedfiles", "Machine")
}

function SetEnvironmentVariableDevServer {
    Write-Host "Setting environment variables for DEV server..."

    [Environment]::SetEnvironmentVariable("MYSQL_ROOT_PASSWORD", "d3vRDS1234_", "Machine")
    [Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_VSPDB", "rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com", "Machine")
    [Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_ReadVSPDB", "rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com", "Machine")
    [Environment]::SetEnvironmentVariable("KAFKA_URI", "10.97.99.172", "Machine")
    [Environment]::SetEnvironmentVariable("KAFKA_TOPIC_NAME_SUFFIX", "-Dev", "Machine")
    [Environment]::SetEnvironmentVariable("WEBAPI_URI", "http://10.97.96.103:3001/", "Machine")
    [Environment]::SetEnvironmentVariable("WEBAPI_DEBUG_URI", "http://10.97.96.103:3001/", "Machine")
    [Environment]::SetEnvironmentVariable("RAPTOR_NOTIFICATION_API_URL", "http://mockprojectwebapi:5001/api/v2/notification", "Machine")
    [Environment]::SetEnvironmentVariable("CUSTOMERSERVICE_API_URL", "http://mockprojectwebapi:5001/api/v1/mock/getcustomersforme", "Machine")
    [Environment]::SetEnvironmentVariable("IMPORTED_FILE_API_URL", "https://api-stg.trimble.com/t/trimble.com/vss-dev-projects/1.4/importedfiles", "Machine")
}

Write-Host "Setting common variables..."
[Environment]::SetEnvironmentVariable("MYSQL_DATABASE_NAME", "VSS-Productivity3D-Filter", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_PORT", "3306", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_USERNAME", "root", "Machine")
[Environment]::SetEnvironmentVariable("KAFKA_PORT", "9092", "Machine")


if ($args -ccontains "--devserver" -Or $args -ccontains "-d") {
    SetEnvironmentVariableDevServer
}
else {
    SetEnvironmentVariableLocalhost
}

Write-Host "Done." -ForegroundColor "darkcyan"
[Console]::ResetColor()
