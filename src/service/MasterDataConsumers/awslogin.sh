#!/bin/bash
result="$(aws ecr get-login --region us-west-2 --profile vss-grant)"
replold="-e none"
replnew="       "
echo "${result}"
newresult="${result/$replold/$replnew}"
echo "${newresult}"
eval "${newresult}"