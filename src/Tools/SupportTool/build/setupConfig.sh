#!/bin/bash

echo "const config = (() => {
    return { "

# Get all the env variables
for var in $(compgen -e); do
    if [[ $var == VUE_APP* ]];
    then
        echo -e "\t\t\"$var\" : \"${!var}\","
    fi
done

echo "
    };
  })();"