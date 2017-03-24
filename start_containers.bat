aws ecr get-login --region us-west-2 --profile vss-grant > temp.cmd
call temp.cmd
del temp.cmd

docker pull 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-mockproject-webapi:latest

docker-compose up --build -d 2>&1 | grep -o \w*_webapi_\w* > container.txt