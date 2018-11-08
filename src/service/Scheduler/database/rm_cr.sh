#!/bin/bash

DIRECTORY="."

if [ $# -gt 1 ] ||  [ "$1" == "-h" ]  || [ "$1" == "--help" ]; then
  echo "Usage: `basename $0` [directory]"
  echo "Recursively find sh files in a directory (default to current dir) and remove trailing \r character in them that cause the '\r command not found' error."
  exit 0
elif [ $# -eq 1 ]; then
  DIRECTORY="$1"
fi

if [ ! -d "$DIRECTORY" ]; then
  echo "Directory '$DIRECTORY' does not exist!"
  exit 1
fi

for i in $(find $DIRECTORY -type f -name "*.sh" 2>/dev/null); 
  do
    echo ">>> chmod 775 $i"
    chmod 775 $i
    echo ">>> sed -i 's/\r$//' $i"
    sed -i 's/\r$//' $i
  done
