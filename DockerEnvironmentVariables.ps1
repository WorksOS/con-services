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
[Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_VSPDB", "localhost", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_ReadVSPDB", "localhost", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_DATABASE_NAME", "VSS-Productivity3D-Scheduler", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_PORT", "3306", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_USERNAME", "root", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_ROOT_PASSWORD", "abc123", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_VSPDB_FILTER", "localhost", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_DATABASE_NAME_FILTER", "VSS-Productivity3D-Filter", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_PORT_FILTER", "3306", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_USERNAME_FILTER", "root", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_ROOT_PASSWORD_FILTER", "abc123", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_VSPDB_PROJECT", "localhost", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_DATABASE_NAME_PROJECT", "VSS-Productivity3D-Project", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_PORT_PROJECT", "3306", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_USERNAME_PROJECT", "root", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_ROOT_PASSWORD_PROJECT", "abc123", "Machine")
[Environment]::SetEnvironmentVariable("MSSQL_SERVER_NAME", "dbmssql", "Machine")
[Environment]::SetEnvironmentVariable("MSSQL_DATABASE_NAME", "NH_OP", "Machine")
[Environment]::SetEnvironmentVariable("MSSQL_USERNAME", "sa", "Machine")
[Environment]::SetEnvironmentVariable("MSSQL_ROOT_PASSWORD", "d3vRDS1234_", "Machine")
[Environment]::SetEnvironmentVariable("SCHEDULER_FILTER_CLEANUP_TASK_RUN", "true", "Machine")
[Environment]::SetEnvironmentVariable("SCHEDULER_IMPORTEDPROJECTFILES_SYNC_TASK_RUN", "true", "Machine")
[Environment]::SetEnvironmentVariable("TCCFILESPACEID", "mock", "Machine")
[Environment]::SetEnvironmentVariable("RAPTOR_NOTIFICATION_API_URL", "http://mockprojectwebapi:5001/api/v2/notification", "Machine")
[Environment]::SetEnvironmentVariable("RAPTOR_JWT_TOKEN", "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9ubmFtZSI6IkNvbXBhY3Rpb24tRGV2ZWxvcC1DSSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcGFzc3dvcmRQb2xpY3lEZXRhaWxzIjoiZXlKMWNHUmhkR1ZrVkdsdFpTSTZNVFE1TVRFM01ERTROamszTWl3aWFHbHpkRzl5ZVNJNld5STJOVE5pWmpJeU9EZzJOamM1TldVd05ERTVNakEyTnpFMFkyVXpNRFpsTURNeVltUXlNalppWkRVMFpqUXpOamcxTkRJME5UZGxaVEl4TURnMU5UQXdJaXdpTWpFMk56ZG1OemxpTlRWbVpqY3pOamxsTVdWbU9EQmhOV0V3WVRGaVpXSTRNamcwWkdJME16WTVNekEzT1RreFpUbGpaRFUzTkRnMk16VmpZVGRsTWlJc0ltTTVOVEF3TURaak5USXpaV0kxT0RkaFpHRXpNRFUxTWpJMFlXUmxabUUzTjJJeE1EYzJZV1JsT1RnMk1qRTBaakpqT0RJek1qWTRNR1l5TnprMk1EVWlYWDA9IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9rZXl0eXBlIjoiUFJPRFVDVElPTiIsInNjb3BlcyI6Im9wZW5pZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW1haWxWZXJpZmllZCI6InRydWUiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3N1YnNjcmliZXIiOiJkZXYtdnNzYWRtaW5AdHJpbWJsZS5jb20iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3VzZXJ0eXBlIjoiQVBQTElDQVRJT05fVVNFUiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcm9sZSI6InB1Ymxpc2hlciIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdFVwZGF0ZVRpbWVTdGFtcCI6IjE0OTcyNzgyMDQ5MjIiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FjY291bnR1c2VybmFtZSI6IkRhdmlkX0dsYXNzZW5idXJ5IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9pZGVudGl0eS91bmxvY2tUaW1lIjoiMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYWNjb3VudG5hbWUiOiJ0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZmlyc3RuYW1lIjoiVGVzdCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcGFzc3dvcmRQb2xpY3kiOiJISUdIIiwiaXNzIjoid3NvMi5vcmcvcHJvZHVjdHMvYW0iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2xhc3RuYW1lIjoiUHJvamVjdE1ETSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBwbGljYXRpb25pZCI6IjM3NDMiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3ZlcnNpb24iOiIxLjQiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJ0ZXN0UHJvamVjdE1ETUB0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdXVpZCI6Ijk4Y2RiNjE5LWIwNmItNDA4NC1iN2M1LTVkY2NjYzgyYWYzYiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW5kdXNlclRlbmFudElkIjoiMSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZ2l2ZW5uYW1lIjoiRGF2ZSIsImV4cCI6MTQ5ODE4MTI0NCwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9pZGVudGl0eS9mYWlsZWRMb2dpbkF0dGVtcHRzIjoiMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvaWRlbnRpdHkvYWNjb3VudExvY2tlZCI6ImZhbHNlIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcGljb250ZXh0IjoiL3QvdHJpbWJsZS5jb20vdnNzLWRldi1wcm9qZWN0cyIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdExvZ2luVGltZVN0YW1wIjoiMTQ5ODE2NTAxOTM3MCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdGllciI6IlVubGltaXRlZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvc3RhdHVzIjoiZXlKQ1RFOURTMFZFSWpvaVptRnNjMlVpTENKWFFVbFVTVTVIWDBaUFVsOUZUVUZKVEY5V1JWSkpSa2xEUVZSSlQwNGlPaUptWVd4elpTSXNJa0pTVlZSRlgwWlBVa05GWDB4UFEwdEZSQ0k2SW1aaGJITmxJaXdpUVVOVVNWWkZJam9pZEhKMVpTSjkiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2xhc3RQd2RTZXRUaW1lU3RhbXAiOiIxNDkxMTcwMTg3Mjk3IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbnRpZXIiOiJVbmxpbWl0ZWQiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VtYWlsYWRkcmVzcyI6InRlc3RQcm9qZWN0TURNQHRyaW1ibGUuY29tIiwianRpIjoiYTU3ZTYwYWQtY2YzNC00YzY4LTk0YmQtOTQxY2E1NWFkMTVhIiwiaWF0IjoxNDk4MTc3NDc5fQ.cTQq_4hmspQ9ojOXeau1q4ZywCwwC2fIOkY_tESA5FU", "Machine")
[Environment]::SetEnvironmentVariable("VETA_EXPORT_URL", "http://mockprojectwebapi:5001/api/v1/mock/vetaexport", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_URI", "http://webapi:80/", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_DEBUG_URI", "http://localhost:3001/", "Machine")
#>
<#  Dev environment
[Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_VSPDB", "rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_DATABASE_NAME", "VSS-Productivity3D-Scheduler", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_PORT", "3306", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_USERNAME", "root", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_ROOT_PASSWORD", "d3vRDS1234_", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_VSPDB_FILTER", "rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_DATABASE_NAME_FILTER", "VSS-Productivity3D-Filter", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_PORT_FILTER", "3306", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_USERNAME_FILTER", "root", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_ROOT_PASSWORD_FILTER", "d3vRDS1234_", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_VSPDB_PROJECT", "rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_DATABASE_NAME_PROJECT", "VSS-Productivity3D-Project", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_PORT_PROJECT", "3306", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_USERNAME_PROJECT", "root", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_ROOT_PASSWORD_PROJECT", "abc123", "Machine")
[Environment]::SetEnvironmentVariable("MSSQL_SERVER_NAME", "dbmssql", "Machine")
[Environment]::SetEnvironmentVariable("MSSQL_DATABASE_NAME", "NH_OP", "Machine")
[Environment]::SetEnvironmentVariable("MSSQL_USERNAME", "sa", "Machine")
[Environment]::SetEnvironmentVariable("MSSQL_ROOT_PASSWORD", "d3vRDS1234_", "Machine")
[Environment]::SetEnvironmentVariable("SCHEDULER_FILTER_CLEANUP_TASK_RUN", "true", "Machine")
[Environment]::SetEnvironmentVariable("SCHEDULER_IMPORTEDPROJECTFILES_SYNC_TASK_RUN", "true", "Machine")
[Environment]::SetEnvironmentVariable("TCCFILESPACEID", "mock", "Machine")
[Environment]::SetEnvironmentVariable("RAPTOR_NOTIFICATION_API_URL", "http://mockprojectwebapi:5001/api/v2/notification", "Machine")
[Environment]::SetEnvironmentVariable("RAPTOR_JWT_TOKEN", "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9ubmFtZSI6IkNvbXBhY3Rpb24tRGV2ZWxvcC1DSSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcGFzc3dvcmRQb2xpY3lEZXRhaWxzIjoiZXlKMWNHUmhkR1ZrVkdsdFpTSTZNVFE1TVRFM01ERTROamszTWl3aWFHbHpkRzl5ZVNJNld5STJOVE5pWmpJeU9EZzJOamM1TldVd05ERTVNakEyTnpFMFkyVXpNRFpsTURNeVltUXlNalppWkRVMFpqUXpOamcxTkRJME5UZGxaVEl4TURnMU5UQXdJaXdpTWpFMk56ZG1OemxpTlRWbVpqY3pOamxsTVdWbU9EQmhOV0V3WVRGaVpXSTRNamcwWkdJME16WTVNekEzT1RreFpUbGpaRFUzTkRnMk16VmpZVGRsTWlJc0ltTTVOVEF3TURaak5USXpaV0kxT0RkaFpHRXpNRFUxTWpJMFlXUmxabUUzTjJJeE1EYzJZV1JsT1RnMk1qRTBaakpqT0RJek1qWTRNR1l5TnprMk1EVWlYWDA9IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9rZXl0eXBlIjoiUFJPRFVDVElPTiIsInNjb3BlcyI6Im9wZW5pZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW1haWxWZXJpZmllZCI6InRydWUiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3N1YnNjcmliZXIiOiJkZXYtdnNzYWRtaW5AdHJpbWJsZS5jb20iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3VzZXJ0eXBlIjoiQVBQTElDQVRJT05fVVNFUiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcm9sZSI6InB1Ymxpc2hlciIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdFVwZGF0ZVRpbWVTdGFtcCI6IjE0OTcyNzgyMDQ5MjIiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FjY291bnR1c2VybmFtZSI6IkRhdmlkX0dsYXNzZW5idXJ5IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9pZGVudGl0eS91bmxvY2tUaW1lIjoiMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYWNjb3VudG5hbWUiOiJ0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZmlyc3RuYW1lIjoiVGVzdCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcGFzc3dvcmRQb2xpY3kiOiJISUdIIiwiaXNzIjoid3NvMi5vcmcvcHJvZHVjdHMvYW0iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2xhc3RuYW1lIjoiUHJvamVjdE1ETSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBwbGljYXRpb25pZCI6IjM3NDMiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3ZlcnNpb24iOiIxLjQiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJ0ZXN0UHJvamVjdE1ETUB0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdXVpZCI6Ijk4Y2RiNjE5LWIwNmItNDA4NC1iN2M1LTVkY2NjYzgyYWYzYiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW5kdXNlclRlbmFudElkIjoiMSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZ2l2ZW5uYW1lIjoiRGF2ZSIsImV4cCI6MTQ5ODE4MTI0NCwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9pZGVudGl0eS9mYWlsZWRMb2dpbkF0dGVtcHRzIjoiMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvaWRlbnRpdHkvYWNjb3VudExvY2tlZCI6ImZhbHNlIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcGljb250ZXh0IjoiL3QvdHJpbWJsZS5jb20vdnNzLWRldi1wcm9qZWN0cyIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdExvZ2luVGltZVN0YW1wIjoiMTQ5ODE2NTAxOTM3MCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdGllciI6IlVubGltaXRlZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvc3RhdHVzIjoiZXlKQ1RFOURTMFZFSWpvaVptRnNjMlVpTENKWFFVbFVTVTVIWDBaUFVsOUZUVUZKVEY5V1JWSkpSa2xEUVZSSlQwNGlPaUptWVd4elpTSXNJa0pTVlZSRlgwWlBVa05GWDB4UFEwdEZSQ0k2SW1aaGJITmxJaXdpUVVOVVNWWkZJam9pZEhKMVpTSjkiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2xhc3RQd2RTZXRUaW1lU3RhbXAiOiIxNDkxMTcwMTg3Mjk3IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbnRpZXIiOiJVbmxpbWl0ZWQiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VtYWlsYWRkcmVzcyI6InRlc3RQcm9qZWN0TURNQHRyaW1ibGUuY29tIiwianRpIjoiYTU3ZTYwYWQtY2YzNC00YzY4LTk0YmQtOTQxY2E1NWFkMTVhIiwiaWF0IjoxNDk4MTc3NDc5fQ.cTQq_4hmspQ9ojOXeau1q4ZywCwwC2fIOkY_tESA5FU", "Machine")
[Environment]::SetEnvironmentVariable("VETA_EXPORT_URL", "http://mockprojectwebapi:5001/api/v1/mock/vetaexport", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_URI", "http://10.97.96.103:3001/", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_DEBUG_URI", "http://10.97.96.103:3001/", "Machine")
#>
