#!/bin/bash

result=""
while IFS='' read -r line || [[ -n "$line" ]]; do
result+=" $line"
done < "$1"

echo "Waiting for containers $result"
{ docker wait $result; } &
{ SEARCH_RESULT="0"
  while [ "$SEARCH_RESULT" ==  "0" ] || [[ -z "${SEARCH_RESULT// }" ]]; do
   echo Containers ok
   sleep 1s
   SEARCH_RESULT=`docker-compose ps -q | xargs docker inspect -f '{{ .State.ExitCode }}' | grep -v 0`
  done; } &

wait -n
pkill -P $$
