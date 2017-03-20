# This script runs a remote script on the ASnode to download the build package from S3 and update Raptor Services
$computer1 = "dev-aslv01.vssengg.com"
$user = "svcRaptor"
$password = ConvertTo-SecureString "v3L0c1R^pt0R!" -AsPlainText -Force

# One way of doing it
#$user = (Get-Item env:UserName -ErrorAction Stop).Value
#$password = (Get-Item env:Password -ErrorAction Stop).Value

# More secure way
$File ="RaptorToken"
#$cred=New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $User, (Get-Content $File | ConvertTo-SecureString)
$cred=New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $user,$password

Write-Host "Running remote deploy script on $computer1"
Invoke-Command -ComputerName $computer1 -Credential $cred -ScriptBlock { & "C:\VLPDArtifacts\Scripts\Functions\UpdateRaptorServicesDocker.ps1"} -ErrorAction Stop
