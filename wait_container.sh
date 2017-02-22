#!/bin/bash

result=""
while IFS='' read -r line || [[ -n "$line" ]]; do
result+=" $line"
done < "$1"

echo "Waiting for containers $result"
docker wait $result