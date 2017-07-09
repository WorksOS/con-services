<#
	Sets the environment variables required for running the tests locally


	TEST_ENVIRONMENT -> The environment the tests are running in - may be unneeded in future
	TEST_DATA_PATH -> Relative path to the test data directory
	COMPACTION_SVC_BASE_URI -> compaction service end point, ususally a port or /compaction
	REPORT_SVC_BASE_URI  -> report service end point, ususally a port or /Report
	TAG_SVC_BASE_URI -> Tag file service, usually a port or /TagProc
	COORD_SVC_BASE_URI Coordinates service, usually a port or /TagProc
	PROD_SVC_BASE_URI Prod service, usually a port or /TagProc
	COORD_SVC_BASE_URI Coordinate service, usually a port or /TagProc
	SERVER -> server used for base - this would typically be set by runAcceptanceTests, but can be manually configured if required

	#>

param (
[string]$RAPTOR_WEBSERVICES_HOST = "localhost"
)
	
[Environment]::SetEnvironmentVariable("TEST_DATA_PATH", "../../TestData/", "Machine")
[Environment]::SetEnvironmentVariable("COMPACTION_SVC_BASE_URI", ":80", "Machine")
[Environment]::SetEnvironmentVariable("NOTIFICATION_SVC_BASE_URI", "80", "Machine")
[Environment]::SetEnvironmentVariable("REPORT_SVC_BASE_URI", ":80", "Machine")
[Environment]::SetEnvironmentVariable("TAG_SVC_BASE_URI", ":80", "Machine")
[Environment]::SetEnvironmentVariable("COORD_SVC_BASE_URI", ":80", "Machine")
[Environment]::SetEnvironmentVariable("PROD_SVC_BASE_URI", ":80", "Machine")
[Environment]::SetEnvironmentVariable("COORD_SVC_BASE_URI", ":80", "Machine")
[Environment]::SetEnvironmentVariable("RAPTOR_WEBSERVICES_HOST", $RAPTOR_WEBSERVICES_HOST, "Machine")