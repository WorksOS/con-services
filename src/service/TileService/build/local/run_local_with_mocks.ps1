$(aws ecr get-login --no-include-email)
docker run -d -p 5001:5001 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-mockproject-webapi:latest-linux
docker run -d -p 80:80 --env-file .\mocks.env 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-tile-webapi:latest
