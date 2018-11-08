#!/bin/bash
echo ">>> Making sure MySQL is up"
eval ./wait-for-it.sh "$MYSQL_SERVER_NAME_VSPDB:$MYSQL_PORT" -t 0
echo "<<< Done checking on MySQL"

echo ">>> Making sure Kafka is up"
eval ./wait-for-it.sh "$KAFKA_URI:$KAFKA_PORT" -t 0
echo "<<< Done checking on Kafka"


echo "Master data kafka consumer starting in 60 seconds....."
sleep 30s

echo "Master data kafka consumer starting Now "
dotnet VSS.Productivity3D.MasterDataConsumer.dll
