#!/bin/bash 
docker stop $(docker ps -a -q)
docker rm $(docker ps -a -q)
docker rmi $(docker images -q)
docker rm $(docker ps -a -q)
docker rmi $(docker images -q)

sh '''eval '$(aws ecr get-login --region us-west-2 --profile vss-grant)' '''
docker pull 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-webapi:latest
docker pull 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-masterdataconsumer:latest

docker volume create --name logs

docker run -i -t --detach --name vss-project-masterdataproject \
-v /ProjectMDM/app/logs:/app/logs \
-e MYSQL_SERVER_NAME_VSPDB='rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com' \
-e MYSQL_SERVER_NAME_ReadVSPDB='rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com' \
-e MYSQL_DATABASE_NAME='VSS-MasterData-Project-Only' \
-e MYSQL_PORT='3306' \
-e MYSQL_USERNAME='root' \
-e MYSQL_ROOT_PASSWORD='d3vRDS1234_' \
-e KAFKA_URI='kafka2.dev.vssengg.com' \
-e KAFKA_PORT='9092' \
-e KAFKA_GROUP_NAME='Project-Consumer' \
-e KAFKA_TOPIC_NAME_SUFFIX='-Dev' \
-e KAFKA_OFFSET='latest' \
-e KAFKA_TOPICS='VSS.Interfaces.Events.MasterData.IProjectEvent' \
 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-masterdataconsumer:latest /bin/bash

docker run -i -t --detach --name vss-project-masterdatacustomer \
-v /ProjectMDM/app/logs:/app/logs \
-e MYSQL_SERVER_NAME_VSPDB='rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com' \
-e MYSQL_SERVER_NAME_ReadVSPDB='rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com' \
-e MYSQL_DATABASE_NAME='VSS-MasterData-Project' \
-e MYSQL_PORT='3306' \
-e MYSQL_USERNAME='root' \
-e MYSQL_ROOT_PASSWORD='d3vRDS1234_' \
-e KAFKA_URI='kafka2.dev.vssengg.com' \
-e KAFKA_PORT='9092' \
-e KAFKA_GROUP_NAME='Project-Consumer' \
-e KAFKA_TOPIC_NAME_SUFFIX='-Dev' \
-e KAFKA_OFFSET='latest' \
-e KAFKA_TOPICS='VSS.Interfaces.Events.MasterData.ICustomerEvent' \
 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-masterdataconsumer:latest /bin/bash

docker run -i -t --detach --name vss-project-masterdatageofence \
-v /ProjectMDM/app/logs:/app/logs \
-e MYSQL_SERVER_NAME_VSPDB='rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com' \
-e MYSQL_SERVER_NAME_ReadVSPDB='rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com' \
-e MYSQL_DATABASE_NAME='VSS-MasterData-Project' \
-e MYSQL_PORT='3306' \
-e MYSQL_USERNAME='root' \
-e MYSQL_ROOT_PASSWORD='d3vRDS1234_' \
-e KAFKA_URI='kafka2.dev.vssengg.com' \
-e KAFKA_PORT='9092' \
-e KAFKA_GROUP_NAME='Project-Consumer' \
-e KAFKA_TOPIC_NAME_SUFFIX='-Dev' \
-e KAFKA_OFFSET='latest' \
-e KAFKA_TOPICS='VSS.Interfaces.Events.MasterData.IGeofenceEvent' \
276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-masterdataconsumer:latest /bin/bash 

docker run -i -t --detach --name vss-project-masterdatasubscription \
-v /ProjectMDM/app/logs:/app/logs \
-e MYSQL_SERVER_NAME_VSPDB='rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com' \
-e MYSQL_SERVER_NAME_ReadVSPDB='rdsmysql-8469.c31ahitxrkg7.us-west-2.rds.amazonaws.com' \
-e MYSQL_DATABASE_NAME='VSS-MasterData-Project' \
-e MYSQL_PORT='3306' \
-e MYSQL_USERNAME='root' \
-e MYSQL_ROOT_PASSWORD='d3vRDS1234_' \
-e KAFKA_URI='kafka2.dev.vssengg.com' \
-e KAFKA_PORT='9092' \
-e KAFKA_GROUP_NAME='Project-Consumer' \
-e KAFKA_TOPIC_NAME_SUFFIX='-Dev' \
-e KAFKA_OFFSET='latest' \
-e KAFKA_TOPICS='VSS.Interfaces.Events.MasterData.ISubscriptionEvent' \
276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-masterdataconsumer:latest /bin/bash 
