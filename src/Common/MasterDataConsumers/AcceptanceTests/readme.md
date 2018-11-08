Docker
======

The deploy folder is where the docker image is built from. And because of the way we use docker-compose.yml and 
also for test result file retrieval, this folder need to have exactly the following structure:

deploy
|
|---Dockerfile
|
|---runtests.sh
|
+---testresults
|   
+---TestProjectFolder1
| 
+---TestProjectFolder2


So when publishing projects do the following:

In Windows: run deploy.bat from the AcceptanceTests directory
In Linux: run deploy.sh from the AcceptanceTests directory
