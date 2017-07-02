#!/bin/bash
result="$(aws ecr get-login --region us-west-2 --profile vss-grant)"
replold="-e none"
replstr=" "
#echo "${result}"
newresult="${result/$replold/$replstr}"
#echo "${newresult}"
eval "${newresult}"
