& .\UpdateEnvFileIpAddress.ps1

docker stop $(docker ps -a -q)
docker rm $(docker ps -a -q)
docker rmi $(docker images -q --filter "dangling=true")
docker rmi vssproductivity3dscheduler_schema
docker rmi vssproductivity3dscheduler_webapi
docker rmi vssproductivity3dscheduler_accepttest

Write-Host "Logging in to image host" -ForegroundColor "darkgray"
Invoke-Expression -Command (aws ecr get-login --no-include-email --region us-west-2)

Write-Host "Executing local testing script..." -ForegroundColor "darkgray"
& .\RunLocalTesting.bat

Write-Host "Finished." -ForegroundColor "darkgray"
[Console]::ResetColor()