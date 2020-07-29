# Load component scripts.
. ../../../build/scripts/aws-docker-login.ps1

Login-Aws

$tag = '940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-trex'
Write-Host "`nPulling TGLDatabase image from $tag..." -ForegroundColor Green
docker pull $tag`:TGLDatabase
docker tag 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-trex:TGLDatabase tgldatabase

# Start the container image and terminate and detach immediately
Write-Host "`nCopying TGLDatabase files to %APPDATA%/TGLGeodata..." -ForegroundColor Green
docker create tgldatabase tgldatabase_tmp | ForEach-Object { docker cp $_`:/tgl_geodata/. $env:APPDATA/TGLGeodata; docker rm $_ }
Write-Host "`nDone" -ForegroundColor Green
