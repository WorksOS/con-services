function SetEnvironmentVariableLocalhost {
    Write-Host "Setting environment variables for LOCALHOST server..."
    [Environment]::SetEnvironmentVariable("CUSTOMERSERVICE_API_URL", "http://localhost:5001/api/v1/mock/getcustomersforme", "Machine")
    [Environment]::SetEnvironmentVariable("FILTERSERVICE_KAFKA_TOPIC_NAME", "VSS.Interfaces.Events.MasterData.IFilterEvent", "Machine")
    [Environment]::SetEnvironmentVariable("IMPORTED_FILE_API_URL","http://localhost:5001/api/v4/mock/importedfiles", "Machine")
    [Environment]::SetEnvironmentVariable("KAFKA_ADVERTISED_HOST_NAME", "LOCALIPADDRESS", "Machine")
    [Environment]::SetEnvironmentVariable("KAFKA_ADVERTISED_PORT", "9092", "Machine")
    [Environment]::SetEnvironmentVariable("KAFKA_AUTO_CREATE_TOPICS_ENABLE", "true", "Machine")
    [Environment]::SetEnvironmentVariable("KAFKA_GROUP_NAME", "Filter-Producer", "Machine")
    [Environment]::SetEnvironmentVariable("KAFKA_OFFSET", "latest", "Machine")
    [Environment]::SetEnvironmentVariable("KAFKA_PRODUCER_SESSION_TIMEOUT", "1000", "Machine")
    [Environment]::SetEnvironmentVariable("KAFKA_TOPIC_NAME_SUFFIX", "-Filter", "Machine")
    [Environment]::SetEnvironmentVariable("KAFKA_URI", "localhost", "Machine")
    [Environment]::SetEnvironmentVariable("MYSQL_ROOT_PASSWORD", "abc123", "Machine")
    [Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_ReadVSPDB", "localhost", "Machine")
    [Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_VSPDB", "localhost", "Machine")
    [Environment]::SetEnvironmentVariable("NOTIFICATION_HUB_URL", "http://push.alpha.eks.vspengg.com/notifications", "Machine")
    [Environment]::SetEnvironmentVariable("PROJECT_API_URL","http://localhost:5001/api/v4/mockproject", "Machine")
    [Environment]::SetEnvironmentVariable("PUSH_NO_AUTHENTICATION_HEADER", "true", "Machine")
    [Environment]::SetEnvironmentVariable("RAPTOR_NOTIFICATION_API_URL", "http://localhost:5001/api/v2/notification", "Machine")
    [Environment]::SetEnvironmentVariable("RAPTOR_PROJECT_SETTINGS_API_URL", "http://localhost:5001/api/v2", "Machine")
    [Environment]::SetEnvironmentVariable("WEBAPI_DEBUG_URI", "http://localhost:5000/", "Machine")
    [Environment]::SetEnvironmentVariable("WEBAPI_URI", "http://webapi:80/", "Machine")
}

function SetEnvironmentVariableDevServer {
    Write-Host "Setting environment variables for DEV server" -ForegroundColor DarkGray

    [Environment]::SetEnvironmentVariable("CUSTOMERSERVICE_API_URL", "http://localhost:5001/api/v1/mock/getcustomersforme", "Machine")
    [Environment]::SetEnvironmentVariable("IMPORTED_FILE_API_URL", "https://api-stg.trimble.com/t/trimble.com/vss-dev-projects/1.4/importedfiles", "Machine")
    [Environment]::SetEnvironmentVariable("KAFKA_TOPIC_NAME_SUFFIX", "-Dev", "Machine")
    [Environment]::SetEnvironmentVariable("KAFKA_URI", "10.97.99.172", "Machine")
    [Environment]::SetEnvironmentVariable("MYSQL_ROOT_PASSWORD", "d3vRDS1234_", "Machine")
    [Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_ReadVSPDB", "rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com", "Machine")
    [Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_VSPDB", "rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com", "Machine")
    [Environment]::SetEnvironmentVariable("NOTIFICATION_HUB_URL", "http://push.alpha.eks.vspengg.com/notifications", "Machine")
    [Environment]::SetEnvironmentVariable("PUSH_NO_AUTHENTICATION_HEADER", "true", "Machine")
    [Environment]::SetEnvironmentVariable("RAPTOR_NOTIFICATION_API_URL", "http://localhost:5001/api/v2/notification", "Machine")
    [Environment]::SetEnvironmentVariable("WEBAPI_DEBUG_URI", "http://10.97.96.103:3001/", "Machine")
    [Environment]::SetEnvironmentVariable("WEBAPI_URI", "http://10.97.96.103:3001/", "Machine")
}

Write-Host "Setting common variables" -ForegroundColor DarkGray
[Environment]::SetEnvironmentVariable("MYSQL_DATABASE_NAME", "VSS-Productivity3D-Filter", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_PORT", "3306", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_USERNAME", "root", "Machine")
[Environment]::SetEnvironmentVariable("KAFKA_PORT", "9092", "Machine")


IF ($args -ccontains "--devserver" -Or $args -ccontains "-d") {
    SetEnvironmentVariableDevServer
}
ELSE {
    SetEnvironmentVariableLocalhost
}

[Console]::ResetColor()
