<#
	Sets the environment variables required for running the tests locally

	TEST_DATA_PATH -> Relative path to the test data directory
	COMPACTION_SVC_BASE_URI -> compaction service end point, ususally a port or /compaction
	NOTIFICATION_SVC_BASE_URI -> notification service end point, usually a port or /notification
	REPORT_SVC_BASE_URI  -> report service end point, ususally a port or /Report
	TAG_SVC_BASE_URI -> Tag file service, usually a port or /TagProc
	COORD_SVC_BASE_URI Coordinates service, usually a port or /Coord
	PROD_SVC_BASE_URI Prod service, usually a port or /ProdData
	FILE_ACCESS_SVC_BASE_URI Coordinate service, usually a port or /FileAccess
	RAPTOR_WEBSERVICES_HOST -> server used for base

	#>

[Environment]::SetEnvironmentVariable("TEST_DATA_PATH", "../../TestData/", "Machine")
[Environment]::SetEnvironmentVariable("COMPACTION_SVC_BASE_URI", ":80", "Machine")
[Environment]::SetEnvironmentVariable("NOTIFICATION_SVC_BASE_URI", "/notification", "Machine")
[Environment]::SetEnvironmentVariable("REPORT_SVC_BASE_URI", ":80", "Machine")
[Environment]::SetEnvironmentVariable("TAG_SVC_BASE_URI", ":80", "Machine")
[Environment]::SetEnvironmentVariable("COORD_SVC_BASE_URI", ":80", "Machine")
[Environment]::SetEnvironmentVariable("PROD_SVC_BASE_URI", ":80", "Machine")
[Environment]::SetEnvironmentVariable("FILE_ACCESS_SVC_BASE_URI", ":80", "Machine")
[Environment]::SetEnvironmentVariable("RAPTOR_WEBSERVICES_HOST", "172.18.16.251", "Machine")
