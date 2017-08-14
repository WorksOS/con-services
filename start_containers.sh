#!/bin/bash

#eval '$(aws ecr get-login --region us-west-2 --profile vss-grant)' 
docker-compose pull
docker-compose up --build -d 2>&1 >/dev/null | grep -o '\b\w*test_\w*\b' > testcontainers
