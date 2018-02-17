#!/bin/bash

docker-compose pull
docker-compose up --build -d 2>&1 >/dev/null | grep -o '\b\w*test_\w*\b' > testcontainers
if [[ $rc != 0 ]]; then exit $rc; fi