#!/bin/bash
result="$(aws ecr get-login --region us-west-2 --profile vss-grant)"
replold="-e none"
replnew=" "
newresult="${result/$replold/$replnew}"
eval "${newresult}"
