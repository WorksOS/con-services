#!/bin/bash
echo "Accept tests are starting .... "
echo "Check the database and kafka to see if port is available"
# Polling the database and kafka status before test
echo ">>> Making sure MySQL is up"
eval ./wait-for-it.sh "$MYSQL_SERVER_NAME_VSPDB:$MYSQL_PORT" -t 0
echo "<<< Done checking on MySQL"

echo ">>> Making sure Kafka is up"
eval ./wait-for-it.sh "$KAFKA_URI:$KAFKA_PORT" -t 0
echo "<<< Done checking on Kafka"

echo "Wait for 60 seconds"
sleep 60s

# Run the component tests
echo "Run the component tests"
echo "KafkaTests starting...."
dotnet TestRun/TestRun.dll results=/testresults/KafkaTestsResults project=KafkaTests messages=false
echo "KafkaTests finished"

echo "RepositoryTests starting...."
dotnet TestRun/TestRun.dll results=/testresults/RepositoryTestsResults project=RepositoryTests messages=false
echo "RepositoryTests finished"

echo "RepositoryLandfillTests starting...."
dotnet TestRun/TestRun.dll results=/testresults/RepositoryLandfillTestsResults project=RepositoryLandfillTests messages=false
echo "RepositoryLandfillTests finished"

echo "Run the component/acceptance tests"
echo "EventTests event tests starting...."
dotnet TestRun/TestRun.dll results=/testresults/EventTestsResults project=EventTests messages=false
echo "EventTests event tests finished"

echo " "
echo " "
echo " All acceptance tests completed"
echo " "

