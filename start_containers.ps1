WriteMsg "Logging in to image host"
Invoke-Expression -Command (aws ecr get-login --no-include-email --profile vss-grant --region us-west-2)

docker-compose pull
#Two matches are required to to filter out the string _webapi_ which started appearing when grep alone is used in powershell
$matchPattern = "\w*_webapi_\w*"
(docker-compose up --build -d 2>&1) -match $matchPattern | grep -o $matchPattern > container.txt
$result = docker-compose up --build -d 2>&1