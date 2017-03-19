#!/bin/bash

result=""
while IFS='' read -r line || [[ -n "$line" ]]; do
result+=" $line"
done < "$1"

echo "Waiting for containers $result"
{ docker wait $result; } &
{ docker-compose ps -q | xargs docker inspect -f '{{ .State.ExitCode }}' | grep -v 0
  while [ $? <>  0 ]; do.
   echo Containers ok
   sleep 1s
   docker-compose ps -q | xargs docker inspect -f '{{ .State.ExitCode }}' | grep -v 0
  done; } &

wait -n
pkill -P $$
