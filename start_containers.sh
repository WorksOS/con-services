#!/bin/bash

eval '$(aws ecr get-login --region us-west-2 --profile vss-grant)' 
docker pull 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-masterdataconsumer:latest
docker pull 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-masterdataconsumer-db:latest

docker-compose up --build -d 2>&1 >/dev/null | grep -v -o '\b\w*schema_\w*\b' > testcontainers
