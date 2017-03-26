aws ecr get-login --region us-west-2 --profile vss-grant > temp.cmd
call temp.cmd
del temp.cmd

docker-compose pull
docker-compose up --build -d 2>&1 | grep -o \w*_webapi_\w* > container.txt