param (
    [string] $environment
)

# Used when running the collaborating services locally.
$localhostEnvironmentVariables = @(
    @{key = "MYSQL_DATABASE_NAME"; value = "VSS-TagFileAuth"},
    @{key = "MYSQL_PORT"; value = "3306"},
    @{key = "MYSQL_USERNAME"; value = "root"},
    @{key = "MYSQL_ROOT_PASSWORD"; value = "abc123"},
    @{key = "MYSQL_SERVER_NAME_VSPDB"; value = "localhost"},
    @{key = "MYSQL_SERVER_NAME_ReadVSPDB"; value = "localhost"},
    @{key = "KAFKA_URI"; value = "localhost"},
    @{key = "KAFKA_PORT"; value = "9092"},
	@{key = "KAFKA_ADVERTISED_HOST_NAME"; value = "LOCALIPADDRESS"},
	@{key = "KAFKA_ADVERTISED_PORT"; value = "9092"},
    @{key = "KAFKA_GROUP_NAME"; value = "TagFileAuth-Datafeed"},
    @{key = "KAFKA_TOPIC_NAME_NOTIFICATIONS"; value = "VSS.Interfaces.Events.MasterData.INotificationEvent"},
    @{key = "KAFKA_TOPIC_NAME_SUFFIX"; value = "-TFA"},
	@{key = "KAFKA_AUTO_CREATE_TOPICS_ENABLE"; value="true"},
	# @{key = "KAFKA_OFFSET"; value="earliest"},
    @{key = "WEBAPI_URI"; value = "http://webapi:5000/"},
    @{key = "WEBAPI_DEBUG_URI"; value = "http://localhost:5000/"})

# Used when connecting to the remote development server services.
$devEnvironmentVariables = @(
    @{key = "MYSQL_DATABASE_NAME"; value = "VSS-TagFileAuth"},
    @{key = "MYSQL_PORT"; value = "3306"},
    @{key = "MYSQL_USERNAME"; value = "root"},
    @{key = "MYSQL_ROOT_PASSWORD"; value = "d3vRDS1234_"},
    @{key = "MYSQL_SERVER_NAME_VSPDB"; value = "rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com"},
    @{key = "MYSQL_SERVER_NAME_ReadVSPDB"; value = "rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com"},
    @{key = "KAFKA_URI"; value = "10.97.99.172"},
    @{key = "KAFKA_PORT"; value = "9092"},
    @{key = "KAFKA_GROUP_NAME"; value = "TagFileAuth-Datafeed"},
    @{key = "KAFKA_TOPIC_NAME_NOTIFICATIONS"; value = "VSS.Interfaces.Events.MasterData.INotificationEvent"},
    @{key = "KAFKA_TOPIC_NAME_SUFFIX"; value = "-TFA"},
    @{key = "WEBAPI_URI"; value = "http://10.97.96.103:3001/"},
    @{key = "WEBAPI_DEBUG_URI"; value = "http://10.97.96.103:3001/"})

if ($environment -ieq "--localhost") {
    $environmentVariables = $localhostEnvironmentVariables
    Write-Host "Setting environment variables for LOCALHOST..."
}
else {
    $environmentVariables = $devEnvironmentVariables
    Write-Host "Setting environment variables for REMOTE DEV Server..."
}

foreach ($_ in $environmentVariables) {
    $line = "  " + $_.key + ": " + $_.value
    Write-Host $line -ForegroundColor DarkGray
    [Environment]::SetEnvironmentVariable($_.key, $_.value, "Machine")
}

Write-Host "`nFinished" -ForegroundColor Green
