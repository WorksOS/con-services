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
[Environment]::SetEnvironmentVariable("IMPORTED_FILE_API_URL2", "https://api-stg.trimble.com/t/trimble.com/vss-dev-projects/1.4/importedfile", "Machine")
[Environment]::SetEnvironmentVariable("CUSTOMERSERVICE_API_URL", "http://mockprojectwebapi:5001/api/v1/mock/getcustomersforme", "Machine")
[Environment]::SetEnvironmentVariable("VETA_EXPORT_URL", "http://mockprojectwebapi:5001/api/v2/mock/export/veta", "Machine")
[Environment]::SetEnvironmentVariable("AWS_BUCKET_NAME", "vss-exports-stg", "Machine")
[Environment]::SetEnvironmentVariable("AWS_ACCESS_KEY", "AKIAIBGOEETXHMANDX7A", "Machine")
[Environment]::SetEnvironmentVariable("AWS_SECRET_KEY", "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", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_URI", "http://localhost:3001", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_DEBUG_URI", "http://localhost:3001/", "Machine")
[Environment]::SetEnvironmentVariable("DOWNLOAD_FOLDER", "C:/temp/", "Machine")
[Environment]::SetEnvironmentVariable("LOG_MAX_CHAR", "1000", "Machine")
[Environment]::SetEnvironmentVariable("MAX_FILE_SIZE", "100000000", "Machine")

#>
<#  Dev environment
[Environment]::SetEnvironmentVariable("MYSQL_SERVER_NAME_VSPDB", "rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_DATABASE_NAME", "VSS-Productivity3D-Scheduler", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_PORT", "3306", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_USERNAME", "root", "Machine")
[Environment]::SetEnvironmentVariable("MYSQL_ROOT_PASSWORD", "d3vRDS1234_", "Machine")
[Environment]::SetEnvironmentVariable("IMPORTED_FILE_API_URL2", "https://api-stg.trimble.com/t/trimble.com/vss-dev-projects/1.4/importedfile", "Machine")
[Environment]::SetEnvironmentVariable("CUSTOMERSERVICE_API_URL", "https://api-stg.trimble.com/t/trimble.com/vss-alpha-customerservice/1.0/Customers/me", "Machine")
[Environment]::SetEnvironmentVariable("VETA_EXPORT_URL", "http://mockprojectwebapi:5001/api/v2/mock/export/veta", "Machine")
[Environment]::SetEnvironmentVariable("AWS_BUCKET_NAME", "vss-exports-stg", "Machine")
[Environment]::SetEnvironmentVariable("AWS_ACCESS_KEY", "AKIAIBGOEETXHMANDX7A", "Machine")
[Environment]::SetEnvironmentVariable("AWS_SECRET_KEY", "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", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_URI", "http://10.97.96.103:3001/", "Machine")
[Environment]::SetEnvironmentVariable("WEBAPI_DEBUG_URI", "http://10.97.96.103:3001/", "Machine")
[Environment]::SetEnvironmentVariable("DOWNLOAD_FOLDER", "C:/temp/", "Machine")
[Environment]::SetEnvironmentVariable("LOG_MAX_CHAR", "1000", "Machine")
[Environment]::SetEnvironmentVariable("MAX_FILE_SIZE", "100000000", "Machine")
#>
