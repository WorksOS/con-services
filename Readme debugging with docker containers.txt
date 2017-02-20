How to build local docker containers for database and kafka, to enable you to debug the Datafeed, WebAPI or the service component tests against. 

0. startup PowerShell in the solution directory C:\Nighthawk2\NextGenCode\VSS.VisionLink.Project

1. Clean up in PowerShell
	docker-compose -f docker-compose-local-debug.yml down

    If you have really bad problems then in Git Bash window
		docker rmi $(docker images -q) --force
       which will remove all images and force them to be downloaded again when you next run docker-compose

2. setup the database and kafka (also zookeeper) containers
    From the VSS.VisionLink.Project directory run: 
        docker-compose -f .\docker-compose-local-debug.yml up -d
		
    Note: The -d is optional, and starts up containers quietly in background, returning the prompt to you. 
    You can then view the 3 containers: 
     	docker ps 
    Or view the log for a container (using the name from above list): 
        docker logs vssvisionlinkproject_kafka_1 -f
		docker logs vssvisionlinkproject_schema_1 -f etc

3. e.g. to debug the webAPI locally :
	Select WebApi as startup project and WebApi configuration from toolbar next to green arrow.
  
 
 
Notes:
Application environment variables: THe service components use appsettings.json files to override (or supply missing) environment variables. 
            However should you wish to setup local environment variables (as would occur between docker containers) there is an example in:
            DockerEnvironmentVariables.ps1.
			
To view e.g. kafka log in its container do:
     docker ps    to get the appropriate reference e.g. d95ea4d0c621 then
     docker logs d95ea4d0c621 -f

to debug WebAPI - after some data is loaded in db:
1. build VSS.VisionLink.Project solution (VS)
2. run VSS.VisionLink.Project\build.bat 
    To run a batch file, either double click in Windows Explorer or ./build.bat in PowerShell
    this rebuilds .\src components (datafeed, webAPI) into appropriate docker directories	
3. run VSS.VisionLink.Project\AcceptanceTests\scripts\deploy_win.bat	
4. build all containers and run acceptance tests from PS: docker-compose -f docker-compose-local.yml up --build > c:\temp\outputlog.log
5. now there will be data in db
6. kill the WebAPI container: docker ps   
   docker stop sdfsfdsf
7. point client to localhost WebAPI
8. start VSS.VisionLink.Project webAPI in debugger (VS)   
