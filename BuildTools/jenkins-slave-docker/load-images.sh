#!/bin/sh
until pgrep -f docker
do
   echo  Waiting
   sleep 1
done

until docker info
do
   echo Waiting
   sleep 1
done

docker load < /usr/local/bin/dotnet20-sdk.tar
docker load < /usr/local/bin/dotnet20-runtime.tar
docker load < /usr/local/bin/dotnet21-sdk.tar
docker load < /usr/local/bin/dotnet21-runtime.tar
docker load < /usr/local/bin/node8.tar

exec "$@"
